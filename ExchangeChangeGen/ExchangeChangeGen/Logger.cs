using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeChangeGenerator
{
    class Logger
    {
        private static readonly object _lock = new object();

        public static void Log(string logMessage, LogLevel level, string serverIP)
        {
            lock (_lock)
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    w.WriteLine("[{0}][{1} - {2}]: {3}", level.ToString(), serverIP, DateTime.Now.ToString(), logMessage);
                }
            }
        }

        public static void Log(string logMessage)
        {
            lock (_lock)
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    w.WriteLine("[{0}]: {1}", DateTime.Now.ToString(), logMessage);
                }
            }
        }
    }

    enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
