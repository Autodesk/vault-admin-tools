using System;

namespace ImportNamingSchemeList
{
    public class ConsoleLogger : ILogger
    {
        #region ILogger Members

        public void Log(MessageCategory category, string message)
        {
            if (category == MessageCategory.Debug)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            if (category == MessageCategory.Warning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (category == MessageCategory.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void Log(MessageCategory category, string format, params object[] args)
        {
            string message = string.Format(format, args);

            Log(category, message);
        }

        public void Log(Exception ex)
        {
            Log(MessageCategory.Error, "ERROR: {0}", ex.Message);
            Log(MessageCategory.Debug, " Source: " + ex.Source);
            Log(MessageCategory.Debug, " StackTrace: " + ex.StackTrace);
            Log(MessageCategory.Debug, " Target: " + ex.TargetSite);
        }

        #endregion
    }
}
