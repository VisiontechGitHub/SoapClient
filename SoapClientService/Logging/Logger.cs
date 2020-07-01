using Newtonsoft.Json;
using SoapClientLibrary;
using System;
using System.Diagnostics;
using System.IO;

namespace SoapClientService.Logging
{
    public class Logger : IDisposable
    {
        private readonly string Log = "Application";
        private readonly EventLog eventLog = new EventLog();
        //private readonly string logFilePath;

        public Logger (string ServiceName) {
            if (!EventLog.SourceExists(ServiceName))
            {
                EventLog.CreateEventSource(ServiceName, Log);
            }
            eventLog.Source = ServiceName;
            eventLog.Log = Log;
            //logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ServiceName, ServiceName + ".log");
        }

        public void LogEvent(string message, EventLogEntryType eventLogEntryType = EventLogEntryType.Information, EventTypes eventType = EventTypes.Default)
        {
            eventLog.WriteEntry(message, eventLogEntryType, Convert.ToInt32(eventType));
            /*using (StreamWriter sw = File.AppendText(logFilePath))
            {
                sw.WriteLine(message);
            }*/
        }

        public void LogEvent(object message, EventLogEntryType eventLogEntryType = EventLogEntryType.Information, EventTypes eventType = EventTypes.Default)
        {
            LogEvent(JsonConvert.SerializeObject(message), eventLogEntryType, eventType);
        }

        public void Dispose()
        {
            eventLog.Dispose();
        }
    }
}
