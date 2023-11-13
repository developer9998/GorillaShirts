using BepInEx.Logging;

namespace GorillaShirts.Behaviors.Tools
{
    public class Logging
    {
        private static ManualLogSource LogSrc;

        public static void Initalize(string Name) => LogSrc = Logger.CreateLogSource(Name);

        public static void Info(object data) => Log(LogLevel.Info, data);

        public static void Warning(object data) => Log(LogLevel.Warning, data);

        public static void Error(object data) => Log(LogLevel.Error, data);

        private static void Log(LogLevel logLevel, object data)
        {
#if DEBUG
            LogSrc?.Log(logLevel, data);
#endif
        }
    }
}
