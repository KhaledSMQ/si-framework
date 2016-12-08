using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Si.Logging
{
    public class EmergencyLog
    {
        private static readonly Lazy<EmergencyLog> LazyInstance = new Lazy<EmergencyLog>(() => new EmergencyLog());

        private readonly string _logFilePath;
        private readonly EventLog _eventLog;

        private EmergencyLog()
        {
            _logFilePath = Path.Combine(Environment.CurrentDirectory,
                $"EmergencyLog.{DateTime.Now:yyyy-MM-dd}.txt");

            const string sourceName = "WindowsService.ExceptionLog";
            if (!EventLog.SourceExists(sourceName))
            {
                EventLog.CreateEventSource(sourceName, "Application");
            }

            _eventLog = new EventLog {Source = sourceName};
        }

        public static EmergencyLog Instance => LazyInstance.Value;

        public void Log(LogLevel logLevel, string message, [CallerMemberName] string caller = null)
        {
            DoLog(logLevel, message, caller);
        }

        public void Log(LogLevel logLevel, string message, Exception e, [CallerMemberName] string caller = null)
        {
            var builder = new StringBuilder()
                .AppendLine(message)
                .AppendLine(e.ToString());
            DoLog(logLevel, builder.ToString(), caller);
        }

        private void DoLog(LogLevel logLevel, string message, string caller)
        {
            LogToEvenLog(logLevel, message);
            LogToFile(logLevel, message, caller ?? typeof(EmergencyLog).Name);
        }

        private void LogToEvenLog(LogLevel logLevel, string message)
        {
            try
            {
                _eventLog.WriteEntry(message, GetLogType(logLevel));
            }
            catch (Exception)
            {
                // Gulp!
            }
        }

        private void LogToFile(LogLevel logLevel, string message, string caller)
        {
            try
            {
                using (var fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine($"{DateTime.UtcNow:yyyy-MMM-dd}|{DateTime.UtcNow:HH:mm.ss.fff}|{caller}|{logLevel}|{message}");
                    }
                }
            }
            catch (Exception)
            {
                // Gulp!
            }
        }
        
        private static EventLogEntryType GetLogType(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    return EventLogEntryType.Information;
                case LogLevel.Info:
                    return EventLogEntryType.Information;
                case LogLevel.Warn:
                    return EventLogEntryType.Warning;
                case LogLevel.Error:
                    return EventLogEntryType.Error;
                default:
                    return EventLogEntryType.Error;
            }
        }

    }
}
