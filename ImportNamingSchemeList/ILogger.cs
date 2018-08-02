using System;

namespace ImportNamingSchemeList
{
    public enum MessageCategory
    {
        Info,
        Debug,
        Warning,
        Error,
    };

    public interface ILogger
    {
        void Log(MessageCategory category, string message);
        void Log(MessageCategory category, string format, params object[] args);
        void Log(Exception ex);
    }
}
