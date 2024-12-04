using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace IntocastGasMeterApp.services
{
    enum LogFlag
    {
        INFO,
        WARNING,
        ERROR
    }

    internal class LoggerService
    {
        public readonly string LOG_DATE_FORMAT = "dd.MM.yyyy HH:mm:ss.fff";

        private static LoggerService? instance = null;

        private readonly string _logPath;
        private readonly ConcurrentQueue<string> _logQueue;
        private readonly AutoResetEvent _logSignal;
        private bool _isRunning;
        private System.Timers.Timer timer;

        public static LoggerService GetInstance()
        {
            instance ??= new LoggerService();
            return instance;
        }        

        public LoggerService()
        {
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string myAppFolder = Path.Combine(appDataPath, appName);

            if (!Directory.Exists(myAppFolder))
            {
                Directory.CreateDirectory(myAppFolder);
            }

            this._logPath = Path.Combine(myAppFolder, "log.txt");

            if (!System.IO.File.Exists(this._logPath))
            {
                System.IO.File.Create(this._logPath);
            }

            _logQueue = new ConcurrentQueue<string>();
            _logSignal = new AutoResetEvent(false);
            _isRunning = true;

            // Start the background worker
            Task.Factory.StartNew(ProcessQueue, TaskCreationOptions.LongRunning);
        }

        private void ProcessQueue()
        {
            while (_isRunning)
            {
                // Wait for a signal or timeout
                _logSignal.WaitOne();

                while (_logQueue.TryDequeue(out var logMessage))
                {
                    try
                    {
                        File.AppendAllText(_logPath, logMessage + Environment.NewLine);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Failed to write to log: {ex.Message}");
                    }
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _logSignal.Set(); // Wake up the processing thread to let it exit
        }

        public void Log(LogFlag flag, string message)
        {
            // log format
            // ISODateTime flag message
            string logMessage = DateTime.Now.ToString("s") + " " + flag.ToString() + " " + message;
            _logQueue.Enqueue(logMessage);
            _logSignal.Set(); // Signal that there's a new log
        }

        public void LogInfo(string message)
        {
            Log(LogFlag.INFO, message);
        }

        public void LogWarning(string message)
        {
            Log(LogFlag.WARNING, message);
        }

        public void LogError(string message)
        {
            Log(LogFlag.ERROR, message);
        }
    }
}
