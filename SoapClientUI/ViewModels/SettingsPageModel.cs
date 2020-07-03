using AdysTech.CredentialManager;
using SoapClientUI.ViewModels.Commands;
using SoapClientLibrary;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Controls;

namespace SoapClientUI.ViewModels
{
    public class SettingsPageModel : AbstractViewModel
    {

        public readonly PasswordBox PasswordBox;

        public override string ValidateProperty(string columnName)
        {
            switch (columnName)
            {
                case "Path":
                    return Validators.NotEmptyString(Path);
                case "Username":
                    return Validators.NotEmptyString(Username);
                case "CasEndpoint":
                    return Validators.NotEmptyString(CasEndpoint);
                case "ServiceEndpoint":
                    return Validators.NotEmptyString(ServiceEndpoint);
                case "Gax":
                    return Validators.EmptyOrNumber(LapGax);
                case "LapGax":
                    return Validators.EmptyOrNumber(LapGax);
                default:
                    return string.Empty;
            }
        }

        private string path = "";
        public string Path
        {
            get { return path; }
            set
            {
                SetProperty(ref path, value, SyncInstallationBindings);
            }
        }

        private string username = "";
        public string Username
        {
            get { return username; }
            set
            {
                SetProperty(ref username, value, SyncSettingsBindings);
            }
        }

        public static readonly string defaultCasEndpoint = "https://cas.visiontech.cloud/";
        private string casEndpoint = defaultCasEndpoint;
        public string CasEndpoint
        {
            get { return casEndpoint; }
            set
            {
                SetProperty(ref casEndpoint, value, SyncSettingsBindings);
            }
        }

        public static readonly string defaultServiceEndpoint = "https://services.optoplus.cloud/";
        private string serviceEndpoint = defaultServiceEndpoint;
        public string ServiceEndpoint
        {
            get { return serviceEndpoint; }
            set
            {
                SetProperty(ref serviceEndpoint, value, SyncSettingsBindings);
            }
        }

        private string inputPath = "";
        public string InputPath
        {
            get { return inputPath; }
            set
            {
                SetProperty(ref inputPath, value, SyncSettingsBindings);
            }
        }

        private string outputPath = "";
        public string OutputPath
        {
            get { return outputPath; }
            set
            {
                SetProperty(ref outputPath, value, SyncSettingsBindings);
            }
        }

        private string processedPath = "";
        public string ProcessedPath
        {
            get { return processedPath; }
            set
            {
                SetProperty(ref processedPath, value, SyncSettingsBindings);
            }
        }

        private string errorPath = "";
        public string ErrorPath
        {
            get { return errorPath; }
            set
            {
                SetProperty(ref errorPath, value, SyncSettingsBindings);
            }
        }

        private string surfacePath = "";
        public string SurfacePath
        {
            get { return surfacePath; }
            set
            {
                SetProperty(ref surfacePath, value, SyncSettingsBindings);
            }
        }

        private string analysisPath = "";
        public string AnalysisPath
        {
            get { return analysisPath; }
            set
            {
                SetProperty(ref analysisPath, value, SyncSettingsBindings);
            }
        }

        private string outputFormat = "";
        public string OutputFormat
        {
            get { return outputFormat; }
            set
            {
                SetProperty(ref outputFormat, value, SyncSettingsBindings);
            }
        }

        private string gax = "";
        public string Gax
        {
            get { return gax; }
            set
            {
                SetProperty(ref gax, value, SyncSettingsBindings);
            }
        }

        private string lapGax = "";
        public string LapGax
        {
            get { return lapGax; }
            set
            {
                SetProperty(ref lapGax, value, SyncSettingsBindings);
            }
        }

        public IEnumerable OutputFormats { get; } = new string[] { "sdf", "hmf", "xyz" };

        private OpenFileCommand selectServiceFileCommand = null;
        public OpenFileCommand SelectServiceFileCommand
        {
            get { return selectServiceFileCommand; }
            set
            {
                SetProperty(ref selectServiceFileCommand, value);
            }
        }

        private OpenFolderCommand selectInputPathCommand = null;
        public OpenFolderCommand SelectInputPathCommand
        {
            get { return selectInputPathCommand; }
            set
            {
                SetProperty(ref selectInputPathCommand, value);
            }
        }

        private OpenFolderCommand selectOutputPathCommand = null;
        public OpenFolderCommand SelectOutputPathCommand
        {
            get { return selectOutputPathCommand; }
            set
            {
                SetProperty(ref selectOutputPathCommand, value);
            }
        }

        private OpenFolderCommand selectProcessedPathCommand = null;
        public OpenFolderCommand SelectProcessedPathCommand
        {
            get { return selectProcessedPathCommand; }
            set
            {
                SetProperty(ref selectProcessedPathCommand, value);
            }
        }

        private OpenFolderCommand selectErrorPathCommand = null;
        public OpenFolderCommand SelectErrorPathCommand
        {
            get { return selectErrorPathCommand; }
            set
            {
                SetProperty(ref selectErrorPathCommand, value);
            }
        }

        private OpenFolderCommand selectSurfacePathCommand = null;
        public OpenFolderCommand SelectSurfacePathCommand
        {
            get { return selectSurfacePathCommand; }
            set
            {
                SetProperty(ref selectSurfacePathCommand, value);
            }
        }

        private OpenFolderCommand selectAnalysisPathCommand = null;
        public OpenFolderCommand SelectAnalysisPathCommand
        {
            get { return selectAnalysisPathCommand; }
            set
            {
                SetProperty(ref selectAnalysisPathCommand, value);
            }
        }

        private WrappedCommand installCommand = null;
        public WrappedCommand InstallServiceCommand
        {
            get { return installCommand; }
            set
            {
                SetProperty(ref installCommand, value);
            }
        }

        private WrappedCommand uninstallCommand = null;
        public WrappedCommand UninstallServiceCommand
        {
            get { return uninstallCommand; }
            set
            {
                SetProperty(ref uninstallCommand, value);
            }
        }

        private bool startEnabled = false;
        public bool StartEnabled
        {
            get { return startEnabled; }
            set
            {
                SetProperty(ref startEnabled, value);
            }
        }

        private WrappedCommand startServiceCommand = null;
        public WrappedCommand StartServiceCommand
        {
            get { return startServiceCommand; }
            set
            {
                SetProperty(ref startServiceCommand, value);
            }
        }

        private WrappedCommand stopServiceCommand = null;
        public WrappedCommand StopServiceCommand
        {
            get { return stopServiceCommand; }
            set
            {
                SetProperty(ref stopServiceCommand, value);
            }
        }

        public SettingsPageModel(PasswordBox PasswordBox) {

            this.PasswordBox = PasswordBox;
            this.PasswordBox.PasswordChanged += (i1, i2) => SyncSettingsBindings();

            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationElement KeyValueConfigurationElement = configuration.AppSettings.Settings[Configurations.SERVICE_PATH];
            if (KeyValueConfigurationElement is object)
            {
                Path = KeyValueConfigurationElement.Value;
            } else {
                Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), System.IO.Path.ChangeExtension(Configurations.SERVICE_NAME, ".exe"));
            }

            SelectServiceFileCommand = new OpenFileCommand(Path => {
                this.Path = Path;
                configuration.AppSettings.Settings.Remove(Configurations.SERVICE_PATH);
                configuration.AppSettings.Settings.Add(Configurations.SERVICE_PATH, Path);
                configuration.Save();
            }, Configurations.SERVICE_NAME + " Executable|" + System.IO.Path.ChangeExtension(Configurations.SERVICE_NAME, ".exe"));

            SelectInputPathCommand = new OpenFolderCommand(Path => InputPath = Path);
            SelectOutputPathCommand = new OpenFolderCommand(Path => OutputPath = Path);
            SelectProcessedPathCommand = new OpenFolderCommand(Path => ProcessedPath = Path);
            SelectErrorPathCommand = new OpenFolderCommand(Path => ErrorPath = Path);
            SelectSurfacePathCommand = new OpenFolderCommand(Path => SurfacePath = Path);
            SelectAnalysisPathCommand = new OpenFolderCommand(Path => AnalysisPath = Path);

            StopServiceCommand = new WrappedCommand(new StopServiceCommand(Configurations.SERVICE_NAME));

        }

        private void SyncInstallationBindings()
        {
            InstallServiceCommand = new WrappedCommand(new InstallServiceCommand(Path, Configurations.SERVICE_NAME));
            UninstallServiceCommand = new WrappedCommand(new UninstallServiceCommand(Path, Configurations.SERVICE_NAME));

            NetworkCredential credential = CredentialManager.GetCredentials(Configurations.SERVICE_NAME);

            if (credential is object)
            {
                Username = credential.UserName;
                PasswordBox.Password = credential.Password;
            }

            if (File.Exists(Path))
            {
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(Path);
                CasEndpoint = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.CAS_API_ENDPOINT, defaultCasEndpoint);
                ServiceEndpoint = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.OPTOPLUS_API_ENDPOINT, defaultServiceEndpoint);
                InputPath = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.INPUT_PATH, "");
                OutputPath = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.OUTPUT_PATH, "");
                ProcessedPath = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.PROCESSED_PATH, "");
                ErrorPath = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.ERROR_PATH, "");
                SurfacePath = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.SURFACE_PATH, "");
                AnalysisPath = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.ANALYSIS_PATH, "");
                OutputFormat = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.OUTPUT_FORMAT, "");
                Gax = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.GAX, "");
                LapGax = SettingUtils.ReadSetting(configuration.AppSettings.Settings, Configurations.LAPGAX, "");
            }
        }

        private void SyncSettingsBindings()
        {
            StartEnabled = !string.IsNullOrWhiteSpace(Username) && PasswordBox is object && !string.IsNullOrWhiteSpace(PasswordBox.Password) && !string.IsNullOrWhiteSpace(CasEndpoint) && !string.IsNullOrWhiteSpace(ServiceEndpoint);

            if (StartEnabled)
            {
                NetworkCredential NetworkCredential = new NetworkCredential(Username, PasswordBox.Password);
                StartServiceCommand = new WrappedCommand(new StartServiceCommand(Configurations.SERVICE_NAME, Username, NetworkCredential.Password, CasEndpoint, ServiceEndpoint, InputPath, OutputPath, ProcessedPath, ErrorPath, SurfacePath, AnalysisPath, OutputFormat, Gax, LapGax));
                StartServiceCommand.PostCallbacks.Add((object dummy) => CredentialManager.SaveCredentials(Configurations.SERVICE_NAME, NetworkCredential));
            }

        }

    }
}
