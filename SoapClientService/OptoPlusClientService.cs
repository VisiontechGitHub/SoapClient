using AdysTech.CredentialManager;
using Microsoft.Extensions.DependencyInjection;
using Org.Visiontech.Commons;
using Org.Visiontech.Commons.Models;
using Org.Visiontech.Commons.Services;
using Org.Visiontech.Compute;
using Org.Visiontech.Credential;
using Org.Visiontech.CredentialGrouping;
using Org.Visiontech.Logout;
using SoapClientLibrary;
using SoapClientService.Error;
using SoapClientService.Logging;
using SoapClientService.Login;
using SoapClientService.Reader;
using SoapClientService.Writer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceProcess;
using System.Threading.Tasks;
using Visiontech.Services.Utils;
using VisiontechCommons;

namespace SoapClientService
{

    public partial class SoapClientService : ServiceBase
    {

        public enum OutputFormat { sdf, hmf, xyz }
        private readonly Logger Logger;
        private readonly string LdsFileExtension = ".lds";
        private readonly string LmsFileExtension = ".lms";
        private readonly Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private readonly string CAS_API_ENDPOINT = "CAS_API_ENDPOINT";
        private readonly string OPTOPLUS_API_ENDPOINT = "OPTOPLUS_API_ENDPOINT";
        private readonly string INPUT_PATH = "INPUT_PATH";
        private readonly string OUTPUT_PATH = "OUTPUT_PATH";
        private readonly string PROCESSED_PATH = "PROCESSED_PATH";
        private readonly string ERROR_PATH = "ERROR_PATH";
        private readonly string SURFACE_PATH = "SURFACE_PATH";
        private readonly string OUTPUT_FORMAT = "OUTPUT_FORMAT";
        private readonly string ANALYSIS_PATH = "ANALYSIS_PATH";
        private readonly string GAX = "GAX";
        private readonly string LAPGAX = "LAPGAX";
        private LoginManager LoginManager;
        private FileSystemWatcher Watcher;
        private RequestBuilder RequestBuilder;
        private SDFWriter SDFWriter;
        private HMFWriter HMFWriter;
        private XYZWriter XYZWriter;
        private SingleFilePathOmaBuilder SingleFilePathOmaWriter;
        private DoubleFilePathOmaBuilder DoubleFilePathOmaWriter;
        private OmaWriter OmaWriter;
        private AnalysisWriter AnalysisWriter;

        public SoapClientService()
        {
            InitializeComponent();

            Logger = new Logger(ServiceName);
        }

        private void HttpWebReponseHandler(HttpWebResponse httpWebResponse) {
            Logger.LogEvent("Http Web Response with Error");
            switch (httpWebResponse.StatusCode)
            {

                case HttpStatusCode.OK:
                    Logger.LogEvent("Ok: " + httpWebResponse.ContentType);
                    break;

                case HttpStatusCode.InternalServerError:
                    Logger.LogEvent("InternalServerError");
                    break;

                case HttpStatusCode.Unauthorized:
                    Logger.LogEvent("Unauthorized");
                    LoggedOff();
                    break;

                case HttpStatusCode.Forbidden:
                    Logger.LogEvent("Forbidden");
                    break;

                default:
                    Logger.LogEvent("Unknown: " + httpWebResponse.StatusCode);
                    break;

            }
            throw new OmaException(httpWebResponse.StatusCode.ToString(), null);
        }

        private void FaultHandler(FaultException exception)
        {
            for (Exception e = exception; e is object; e = e.InnerException)
            {
                Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
            }
            switch (exception.Reason.ToString())
            {
                case "invalid.grant":
                    Logger.LogEvent("Invalid Grant");
                    LoggedOff();
                    break;
            }
            throw new OmaException(exception.Message, exception);
        }

        private void ExceptionHandler(Exception exception)
        {
            for (Exception e = exception; e is object; e = e.InnerException)
            {
                Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
            }
            throw new OmaException("ERROR", null);
        }

        private void Watcher_Created(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Logger.LogEvent("Created " + fileSystemEventArgs.Name + ": " + fileSystemEventArgs.FullPath);
            try
            {
                using (FileStream stream = new FileStream(fileSystemEventArgs.FullPath, FileMode.Open))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                if (File.Exists(fileSystemEventArgs.FullPath))
                {
                    Task.Delay(1000).Wait();
                    Watcher_Created(sender, fileSystemEventArgs);
                }
                return;
            }
            Watcher_Renamed(sender, fileSystemEventArgs);
        }

        private void Watcher_Renamed(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            Logger.LogEvent("Renamed " + fileSystemEventArgs.Name + ": " + fileSystemEventArgs.FullPath);
            if (string.Equals(Path.GetExtension(fileSystemEventArgs.Name), LdsFileExtension, StringComparison.InvariantCultureIgnoreCase)) Manage(fileSystemEventArgs.FullPath);
        }

        private void Manage(string fullPath)
        {
            try
            {
                Logger.LogEvent("Manage " + Path.GetFileName(fullPath));
                Manage(fullPath, OmaReader.Read(fullPath));
            }
            catch (OmaException OmaException)
            {
                OmaEventWithDuration OmaEvent = OmaEvent = new OmaEventWithDuration(null, null, null, false, DateTime.Now);
                for (Exception e = OmaException; e is object; e = e.InnerException)
                {
                    Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                    Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
                }
                OmaEvent.Result = OmaException.Message;
                OmaEvent.HasErrors = true;
                OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                WriteError(fullPath, OmaException.Message, OmaException.Side, OmaException.OmaStatusCode, OmaEvent);
            }
            catch (Exception UnspecifiedException)
            {
                OmaEventWithDuration OmaEvent = new OmaEventWithDuration(null, null, null, false, DateTime.Now);
                for (Exception e = UnspecifiedException; e is object; e = e.InnerException)
                {
                    Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                    Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
                }
                OmaEvent.Result = "ERROR";
                OmaEvent.HasErrors = true;
                OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                WriteError(fullPath, null, Org.Visiontech.Compute.side.UNKNOWN, OmaStatusCode.General, OmaEvent);
            }
        }

        private ComputeSoap ProviderComputeSoap()
        {
            IAuthenticatingMessageInspector authenticatingMessageInspector = new AuthenticatingMessageInspector
            {
                HttpRequestMessageProperty = new HttpRequestMessageProperty()
            };

            authenticatingMessageInspector.HttpRequestMessageProperty.Headers.Set(HttpRequestHeader.Authorization, "Bearer " + LoginManager.RefreshServiceTicket());
            authenticatingMessageInspector.HttpRequestMessageProperty.Headers.Set("X-Grant", LoginManager.XGrant);

            LoggingMessageInspector loggingMessageInspector = new LoggingMessageInspector();

            ICallbackMessageInspector callbackMessageInspector = new CallbackMessageInspector();
            callbackMessageInspector.RequestCallbacks.Add(message => Logger.LogEvent("REQUEST " + message));
            callbackMessageInspector.ResponseCallbacks.Add(message => Logger.LogEvent("RESPONSE"));
            ICollection<IClientMessageInspector> inspectors = new Collection<IClientMessageInspector>() { authenticatingMessageInspector, loggingMessageInspector, callbackMessageInspector };
            ICollection<Action<HttpWebResponse>> handlers = new Collection<Action<HttpWebResponse>>() { HttpWebReponseHandler };
            ICollection<Action<FaultException>> faultHandlers = new Collection<Action<FaultException>>() { FaultHandler };
            ICollection<Action<Exception>> exceptionHandlers = new Collection<Action<Exception>>() { ExceptionHandler };

            return ClientBaseUtils.InitClientBase<ComputeSoap, ComputeSoapClient>(new EndpointAddress(configuration.AppSettings.Settings[OPTOPLUS_API_ENDPOINT].Value + "/optoplus-services-web/ComputeSoap"), inspectors, handlers, faultHandlers, exceptionHandlers);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Manage(string fullPath, OmaReaderResult omaReaderResult)
        {
            OmaEventWithDuration OmaEvent = EventUtils.ToOmaEventWithDuration(omaReaderResult);

            try
            {
                LoginManager.RefreshServiceTicket();

                switch (omaReaderResult)
                {
                    case OmaReaderDoubleResult OmaReaderDoubleResult:
                        Task<computeLensResponseDTO> leftTask = RequestBuilder.BuildComputeTask(OmaReaderDoubleResult.Left, Org.Visiontech.Compute.side.LEFT);
                        Task<computeLensResponseDTO> rightTask = RequestBuilder.BuildComputeTask(OmaReaderDoubleResult.Right, Org.Visiontech.Compute.side.RIGHT);
                        Task.WhenAll(leftTask, rightTask).ContinueWith(tasks => {
                            if (tasks.IsFaulted)
                            {
                                tasks.Exception.Handle(Exception => {
                                    for (Exception e = Exception; e is object; e = e.InnerException)
                                    {
                                        Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                                        Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
                                    }
                                    if (Exception is OmaException OmaException)
                                    {
                                        OmaEvent.Result = OmaException.Message;
                                        OmaEvent.HasErrors = true;
                                        OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                                        WriteError(fullPath, OmaException.Message, OmaException.Side, OmaException.OmaStatusCode, OmaEvent);
                                    } else
                                    {
                                        OmaEvent.Result = Exception.Message;
                                        OmaEvent.HasErrors = true;
                                        OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                                        WriteError(fullPath, Exception.Message, Org.Visiontech.Compute.side.UNKNOWN, OmaStatusCode.General, OmaEvent);
                                    }
                                    return true;
                                });
                                return;
                            }
                            OmaEvent.Result = "SUCCESS";
                            OmaEvent.HasErrors = false;
                            OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                            Write(OmaReaderDoubleResult, leftTask.Result, rightTask.Result, OmaEvent);
                            File.Delete(Path.Combine(configuration.AppSettings.Settings[PROCESSED_PATH].Value, Path.GetFileName(fullPath)));
                            File.Move(fullPath, Path.Combine(configuration.AppSettings.Settings[PROCESSED_PATH].Value, Path.GetFileName(fullPath)));
                        });
                        break;
                    case OmaReaderLeftResult OmaReaderLeftResult:
                        RequestBuilder.BuildComputeTask(OmaReaderLeftResult.Result, Org.Visiontech.Compute.side.LEFT).ContinueWith(task => { 
                            if (task.IsFaulted)
                            {
                                task.Exception.Handle(Exception => {
                                    for (Exception e = Exception; e is object; e = e.InnerException)
                                    {
                                        Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                                        Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
                                    }
                                    if (Exception is OmaException OmaException)
                                    {
                                        OmaEvent.Result = OmaException.Message;
                                        OmaEvent.HasErrors = true;
                                        OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                                        WriteError(fullPath, OmaException.Message, OmaException.Side, OmaException.OmaStatusCode, OmaEvent);
                                    }
                                    else
                                    {
                                        OmaEvent.Result = Exception.Message;
                                        OmaEvent.HasErrors = true;
                                        OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                                        WriteError(fullPath, Exception.Message, Org.Visiontech.Compute.side.UNKNOWN, OmaStatusCode.General, OmaEvent);
                                    }
                                    return true;
                                });
                                return;
                            }
                            OmaEvent.Result = "SUCCESS";
                            OmaEvent.HasErrors = false;
                            OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                            Write(OmaReaderLeftResult, task.Result, OmaEvent);
                            File.Delete(Path.Combine(configuration.AppSettings.Settings[PROCESSED_PATH].Value, Path.GetFileName(fullPath)));
                            File.Move(fullPath, Path.Combine(configuration.AppSettings.Settings[PROCESSED_PATH].Value, Path.GetFileName(fullPath)));
                        });
                        break;
                    case OmaReaderRightResult OmaReaderRightResult:
                        RequestBuilder.BuildComputeTask(OmaReaderRightResult.Result, Org.Visiontech.Compute.side.RIGHT).ContinueWith(task => {
                            if (task.IsFaulted)
                            {
                                task.Exception.Handle(Exception => {
                                    for (Exception e = Exception; e is object; e = e.InnerException)
                                    {
                                        Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                                        Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
                                    }
                                    if (Exception is OmaException OmaException)
                                    {
                                        OmaEvent.Result = OmaException.Message;
                                        OmaEvent.HasErrors = true;
                                        OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                                        WriteError(fullPath, OmaException.Message, OmaException.Side, OmaException.OmaStatusCode, OmaEvent);
                                    }
                                    else
                                    {
                                        OmaEvent.Result = Exception.Message;
                                        OmaEvent.HasErrors = true;
                                        OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                                        WriteError(fullPath, Exception.Message, Org.Visiontech.Compute.side.UNKNOWN, OmaStatusCode.General, OmaEvent);
                                    }
                                    return true;
                                });
                                return;
                            }
                            OmaEvent.Result = "SUCCESS";
                            OmaEvent.HasErrors = false;
                            OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                            Write(OmaReaderRightResult, task.Result, OmaEvent);
                            File.Delete(Path.Combine(configuration.AppSettings.Settings[PROCESSED_PATH].Value, Path.GetFileName(fullPath)));
                            File.Move(fullPath, Path.Combine(configuration.AppSettings.Settings[PROCESSED_PATH].Value, Path.GetFileName(fullPath)));
                        });
                        break;
                }
            }
            catch (OmaException OmaException)
            {
                for (Exception e = OmaException; e is object; e = e.InnerException)
                {
                    Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                    Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
                }
                OmaEvent.Result = OmaException.Message;
                OmaEvent.HasErrors = true;
                OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                WriteError(fullPath, OmaException.Message, OmaException.Side, OmaException.OmaStatusCode, OmaEvent);
            }
            catch (Exception UnspecifiedException)
            {
                for (Exception e = UnspecifiedException; e is object; e = e.InnerException)
                {
                    Logger.LogEvent("Exception " + e.GetType() + " " + e.Message, EventLogEntryType.Error);
                    Logger.LogEvent("StackTrace " + e.StackTrace, EventLogEntryType.Error);
                }
                OmaEvent.Result = "ERROR";
                OmaEvent.HasErrors = true;
                OmaEvent.Duration = DateTime.Now.Subtract(OmaEvent.StartDateTime);
                WriteError(fullPath, null, Org.Visiontech.Compute.side.LEFT, OmaStatusCode.General, EventUtils.ToOmaEventWithDuration(omaReaderResult));
            }

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void WriteError(string fullPath, string error, Org.Visiontech.Compute.side side, OmaStatusCode omaStatusCode, OmaEvent OmaEvent) {
            try
            {
                if (!File.Exists(fullPath)) return;
                File.Delete(Path.Combine(configuration.AppSettings.Settings[ERROR_PATH].Value, Path.GetFileName(fullPath)));
                string tempErrorPath = Path.Combine(configuration.AppSettings.Settings[ERROR_PATH].Value, Path.GetFileNameWithoutExtension(fullPath));
                File.Move(fullPath, tempErrorPath);
                
                if (string.IsNullOrWhiteSpace(error))
                {
                    File.AppendAllText(tempErrorPath, "STATUS=" + Convert.ToInt32(OmaStatusCode.General) + Environment.NewLine, System.Text.Encoding.UTF8);
                } else if (Org.Visiontech.Compute.side.UNKNOWN.Equals(side))
                {
                    File.AppendAllText(tempErrorPath, "STATUS=" + Convert.ToInt32(OmaStatusCode.General) + ";" + error + Environment.NewLine, System.Text.Encoding.UTF8);
                } else
                {
                    File.AppendAllText(tempErrorPath, "STATUS=" + Convert.ToInt32(OmaStatusCode.General) + Environment.NewLine, System.Text.Encoding.UTF8);
                    File.AppendAllText(tempErrorPath, "XSTATUS=" + (Org.Visiontech.Compute.side.LEFT.Equals(side) ? "L" : "R") + ";" + Convert.ToInt32(omaStatusCode) + ";" + error + ";E" + Environment.NewLine, System.Text.Encoding.UTF8);
                }
                
                File.Move(tempErrorPath, String.Concat(tempErrorPath, LmsFileExtension));
                Logger.LogEvent(OmaEvent, EventLogEntryType.Information, EventTypes.Result);
            }
            catch (Exception exception)
            {
                Logger.LogEvent("Exception " + exception.Message, EventLogEntryType.Error);
                Logger.LogEvent("StackTrace " + exception.StackTrace, EventLogEntryType.Error);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Write(OmaReaderSingleResult OmaReaderSingleResult, computeLensResponseDTO response, OmaEvent OmaEvent) {
            AbstractSurfaceOmaWriter<threeDimensionalPointDTO> surfaceOmaWriter;
            OutputFormat outputFormat;
            outputFormat = Enum.TryParse(configuration.AppSettings.Settings[OUTPUT_FORMAT].Value, out outputFormat) ? outputFormat : OutputFormat.sdf;
            switch (outputFormat) {
                case OutputFormat.sdf:
                    surfaceOmaWriter = SDFWriter;
                    break;
                case OutputFormat.hmf:
                    surfaceOmaWriter = HMFWriter;
                    break;
                default:
                    surfaceOmaWriter = XYZWriter;
                    break;
            }
            surfaceOmaWriter.Write(OmaReaderSingleResult, response);
            string surfaceFilePath = surfaceOmaWriter.SurfaceFilePath(OmaReaderSingleResult);
            IFilePathOmaBuilder filePathOmaBuilder;
            switch (outputFormat)
            {
                case OutputFormat.sdf:
                    filePathOmaBuilder = new SingleFilePathOmaBuilder(surfaceFilePath);
                    break;
                default:
                    switch (OmaReaderSingleResult)
                    {
                        case OmaReaderLeftResult OmaReaderLeftResult:
                            filePathOmaBuilder = new DoubleFilePathOmaBuilder(null, surfaceFilePath);
                            break;
                        default:
                            filePathOmaBuilder = new DoubleFilePathOmaBuilder(surfaceFilePath, null);
                            break;
                    }
                    break;
            }
            AnalysisWriter.Write(OmaReaderSingleResult, response);
            OmaWriter.Write(OmaReaderSingleResult, response, surfaceFilePath, filePathOmaBuilder);
            Logger.LogEvent(OmaEvent, EventLogEntryType.Information, EventTypes.Result);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Write(OmaReaderDoubleResult OmaReaderDoubleResult, computeLensResponseDTO left, computeLensResponseDTO right, OmaEvent OmaEvent)
        {
            AbstractSurfaceOmaWriter<threeDimensionalPointDTO> surfaceOmaWriter;
            switch (Enum.TryParse(configuration.AppSettings.Settings[OUTPUT_FORMAT].Value, out OutputFormat outputFormat) ? outputFormat : OutputFormat.sdf) {
                case OutputFormat.sdf:
                    surfaceOmaWriter = SDFWriter;
                    break;
                case OutputFormat.hmf:
                    surfaceOmaWriter = HMFWriter;
                    break;
                default:
                    surfaceOmaWriter = XYZWriter;
                    break;
            }
            surfaceOmaWriter.Write(OmaReaderDoubleResult, left, right);
            SurfaceFiles surfaceFiles = surfaceOmaWriter.SurfaceFilesPath(OmaReaderDoubleResult);
            IFilePathOmaBuilder filePathOmaBuilder;
            if (surfaceFiles.LeftSurfaceFile.Equals(surfaceFiles.RightSurfaceFile))
            {
                filePathOmaBuilder = new SingleFilePathOmaBuilder(surfaceFiles.LeftSurfaceFile);
            } else
            {
                filePathOmaBuilder = new DoubleFilePathOmaBuilder(surfaceFiles.RightSurfaceFile, surfaceFiles.LeftSurfaceFile);
            }
            AnalysisWriter.Write(OmaReaderDoubleResult, left, right);
            OmaWriter.Write(OmaReaderDoubleResult, left, right, surfaceFiles.LeftSurfaceFile, surfaceFiles.RightSurfaceFile, filePathOmaBuilder);
            Logger.LogEvent(OmaEvent, EventLogEntryType.Information, EventTypes.Result);
        }

        private void SaveSetting(string key, string value)
        {
            configuration.AppSettings.Settings.Remove(key);
            configuration.AppSettings.Settings.Add(key, value);
            configuration.Save();
        }

        private void RecursiveSave(IEnumerable<string> args, IEnumerable<string> keys) {
            if (args.Any() && keys.Any()) {
                SaveSetting(keys.First(), args.First());
                RecursiveSave(args.Skip(1), keys.Skip(1));
            }
        }

        protected override void OnStart(string[] args)
        {
            if (args.Length >= 2)
            {
                CredentialManager.SaveCredentials(ServiceName, new NetworkCredential(args[0], args[1]));
                Logger.LogEvent("Credential changed to " + args[0], eventType: EventTypes.Configuration);
                RecursiveSave(args.Skip(2), new List<string>() { CAS_API_ENDPOINT, OPTOPLUS_API_ENDPOINT, INPUT_PATH, OUTPUT_PATH, PROCESSED_PATH, ERROR_PATH, SURFACE_PATH, ANALYSIS_PATH, OUTPUT_FORMAT, GAX, LAPGAX });
            }
            
            string defaultPath = Path.Combine(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)), ServiceName);
            string defaultFallbackPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ServiceName);
            
            string chosenDefaultPath = null;
            string[] attemptedPaths = { defaultPath, defaultFallbackPath };
            foreach (string attemptedPath in attemptedPaths)
            {
                try
                {
                    Directory.CreateDirectory(attemptedPath);
                    chosenDefaultPath = attemptedPath;
                    break;
                }
                catch (Exception e)
                {
                    Logger.LogEvent("Unable to use " + attemptedPath + " as default directory" + Environment.NewLine + e.StackTrace, EventLogEntryType.Warning);
                }
            }
            
            new List<string>() { INPUT_PATH, OUTPUT_PATH, PROCESSED_PATH, ERROR_PATH, SURFACE_PATH, ANALYSIS_PATH }.FindAll(key => configuration.AppSettings.Settings[key] is null || string.IsNullOrWhiteSpace(configuration.AppSettings.Settings[key].Value)).ForEach(
                key => {
                    if (chosenDefaultPath != null) {
                        SaveSetting(key, Path.Combine(chosenDefaultPath, key));
                    } else {
                        string error = key + ": unable to find a writable default path. Please check the previous Application warnings emitted by " + ServiceName + " or manually specify a writable path for " + key;
                        Logger.LogEvent(error, EventLogEntryType.Warning);
                        throw new Exception(error);
                    }
                }
            );

            new List<string>() { INPUT_PATH, OUTPUT_PATH, PROCESSED_PATH, ERROR_PATH, SURFACE_PATH, ANALYSIS_PATH }.ForEach(key => Directory.CreateDirectory(configuration.AppSettings.Settings[key].Value));

            if (configuration.AppSettings.Settings[OUTPUT_FORMAT] is null)
            {
                SaveSetting(OUTPUT_FORMAT, OutputFormat.sdf.ToString());
            }

            Logger.LogEvent(configuration.AppSettings.Settings.AllKeys.Select(key => key + " " + configuration.AppSettings.Settings[key].Value).Aggregate((v1, v2) => string.Join(Environment.NewLine, new List<string> { v1, v2 })));

            NetworkCredential NetworkCredential = CredentialManager.GetCredentials(ServiceName);

            if (!new List<string>() { CAS_API_ENDPOINT, OPTOPLUS_API_ENDPOINT }.All(key => configuration.AppSettings.Settings.AllKeys.Contains(key)) || NetworkCredential is null)
            {
                Environment.FailFast("Missing Parameter");
            }

            SDFWriter = new SDFWriter(configuration.AppSettings.Settings[SURFACE_PATH].Value);
            HMFWriter = new HMFWriter(configuration.AppSettings.Settings[SURFACE_PATH].Value);
            XYZWriter = new XYZWriter(configuration.AppSettings.Settings[SURFACE_PATH].Value);
            OmaWriter = new OmaWriter(configuration.AppSettings.Settings[OUTPUT_PATH].Value, configuration.AppSettings.Settings[GAX].Value, configuration.AppSettings.Settings[LAPGAX].Value);
            AnalysisWriter = new AnalysisWriter(configuration.AppSettings.Settings[ANALYSIS_PATH].Value);

            IAuthenticatingMessageInspector authenticatingMessageInspector = new AuthenticatingMessageInspector {
                HttpRequestMessageProperty = new HttpRequestMessageProperty()
            };

            LoggingMessageInspector loggingMessageInspector = new LoggingMessageInspector();
            ICallbackMessageInspector callbackMessageInspector = new CallbackMessageInspector();
            callbackMessageInspector.RequestCallbacks.Add(message => Logger.LogEvent("REQUEST " + message));
            callbackMessageInspector.ResponseCallbacks.Add(message => Logger.LogEvent("RESPONSE"));
            ICollection<IClientMessageInspector> inspectors = new Collection<IClientMessageInspector>() { authenticatingMessageInspector, loggingMessageInspector, callbackMessageInspector };
            ICollection<Action<HttpWebResponse>> handlers = new Collection<Action<HttpWebResponse>>() { HttpWebReponseHandler };
            ICollection<Action<FaultException>> faultHandlers = new Collection<Action<FaultException>>() { FaultHandler };
            ICollection<Action<Exception>> exceptionHandlers = new Collection<Action<Exception>>() { ExceptionHandler };

            IProvider<HttpClientHandler> HttpClientHandlerProvider = new HttpClientHandlerProvider();
            IProvider<HttpClient> HttpClientProvider = new HttpClientProvider(HttpClientHandlerProvider);
            IAuthenticationService authenticationService = new AuthenticationService(new Uri(new Uri(configuration.AppSettings.Settings[CAS_API_ENDPOINT].Value), "cas/v1/tickets/"), "optoplus", HttpClientProvider);
            CredentialSoap credentialSoap = ClientBaseUtils.InitClientBase<CredentialSoap, CredentialSoapClient>(new EndpointAddress(configuration.AppSettings.Settings[OPTOPLUS_API_ENDPOINT].Value + "/optoplus-services-web/CredentialSoap"), inspectors, handlers, faultHandlers, exceptionHandlers);
            CredentialGroupingSoap credentialGroupingSoap = ClientBaseUtils.InitClientBase<CredentialGroupingSoap, CredentialGroupingSoapClient>(new EndpointAddress(configuration.AppSettings.Settings[OPTOPLUS_API_ENDPOINT].Value + "/optoplus-services-web/CredentialGroupingSoap"), inspectors, handlers, faultHandlers, exceptionHandlers);
            LogoutSoap logoutSoap = ClientBaseUtils.InitClientBase<LogoutSoap, LogoutSoapClient>(new EndpointAddress(configuration.AppSettings.Settings[OPTOPLUS_API_ENDPOINT].Value + "/optoplus-services-web/LogoutSoap"), inspectors, handlers, faultHandlers, exceptionHandlers);

            SharedServiceProvider.Reset();
            SharedServiceProvider.Services.Clear();
            SharedServiceProvider.Services.AddSingleton(serviceProvider => HttpClientHandlerProvider);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => HttpClientProvider);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => authenticatingMessageInspector);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => loggingMessageInspector);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => callbackMessageInspector);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => authenticationService);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => credentialSoap);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => credentialGroupingSoap);
            SharedServiceProvider.Services.AddSingleton(serviceProvider => logoutSoap);

            RequestBuilder = new RequestBuilder(ProviderComputeSoap);

            Watcher = new FileSystemWatcher(configuration.AppSettings.Settings[INPUT_PATH].Value, "*" + LdsFileExtension);
            Watcher.Created += Watcher_Created;
            Watcher.Renamed += Watcher_Renamed;

            LoginManager = new LoginManager(Logger, ServiceName, authenticationService, authenticatingMessageInspector, credentialSoap, credentialGroupingSoap);
            LoginManager.Login(10000, LoggedIn);
        }

        protected void LoggedIn()
        {
            Directory.GetFiles(configuration.AppSettings.Settings[INPUT_PATH].Value, "*" + LdsFileExtension).ToList().ForEach(fullPath => Manage(fullPath));
            Watcher.EnableRaisingEvents = true;
        }

        protected void LoggedOff()
        {
            LoginManager.Login(10000, LoggedIn);
            Watcher.EnableRaisingEvents = false;
        }

        protected override void OnStop()
        {
            Watcher.EnableRaisingEvents = false;
        }

    }
}
