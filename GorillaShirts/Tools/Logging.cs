using BepInEx.Logging;

namespace GorillaShirts.Tools
{
    public static class Logging
    {
#pragma warning disable IDE0052 // Remove unread private members
        private static ManualLogSource Logger;
#pragma warning restore IDE0052 // Remove unread private members

        public static void Initialize(ManualLogSource logger)
        {
            Logger = logger;
        }

        public static void Info(object data) => SendLog(LogLevel.Info, data);

        public static void Warning(object data) => SendLog(LogLevel.Warning, data);

        public static void Error(object data) => SendLog(LogLevel.Error, data);

        private static void SendLog(LogLevel logLevel, object data)
        {
#if DEBUG
            Logger?.Log(logLevel, data);
#endif
        }
    }
}
