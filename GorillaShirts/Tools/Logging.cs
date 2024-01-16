using BepInEx.Logging;

namespace GorillaShirts.Tools
{
    public class Logging
    {
        private static ManualLogSource Log;

        public static void Initalize(string Name) => Log = Logger.CreateLogSource(Name);

        public static void Info(object data) => SendLog(LogLevel.Info, data);

        public static void Warning(object data) => SendLog(LogLevel.Warning, data);

        public static void Error(object data) => SendLog(LogLevel.Error, data);

        private static void SendLog(LogLevel logLevel, object data)
        {
#if DEBUG
            Log?.Log(logLevel, data);
#endif
        }
    }
}
