using System;

namespace SpellbookToPdf.Logger
{
    class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {
            Console.WriteLine("ConsoleLogger created");
        }
        public void Critical(string message)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("{0} - [{1}] {2}", DateTime.Now, LogType.CRITICAL, message);
            }
            finally
            {
                Console.ForegroundColor = currentForeground;
            }
        }

        public void Debug(string message)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("{0} - [{1}] {2}", DateTime.Now, LogType.DEBUG, message);
            }
            finally
            {
                Console.ForegroundColor = currentForeground;
            }
        }

        public void Error(string message)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} - [{1}] {2}", DateTime.Now, LogType.ERROR, message);
            }
            finally
            {
                Console.ForegroundColor = currentForeground;
            }
        }

        public void Info(string message)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("{0} - [{1}] {2}", DateTime.Now, LogType.INFO, message);
            }
            finally
            {
                Console.ForegroundColor = currentForeground;
            }
        }

        public void Warning(string message)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("{0} - [{1}] {2}", DateTime.Now, LogType.WARNING, message);
            }
            finally
            {
                Console.ForegroundColor = currentForeground;
            }
        }

        public void Log(string message, LogType logType)
        {
            switch (logType)
            {
                case LogType.DEBUG: Debug(message); break;
                case LogType.INFO: Info(message); break;
                case LogType.WARNING: Warning(message); break;
                case LogType.ERROR: Error(message); break;
                case LogType.CRITICAL: Critical(message); break;
            }
        }
    }
}
