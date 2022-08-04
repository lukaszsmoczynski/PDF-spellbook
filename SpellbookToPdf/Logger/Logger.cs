namespace SpellbookToPdf.Logger
{
    public enum LogType
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        CRITICAL
    }

    interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Critical(string message);
        void Log(string message, LogType logType);
    }
}
