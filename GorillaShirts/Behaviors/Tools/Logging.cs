using BepInEx.Logging;

namespace GorillaShirts.Behaviors.Tools
{
    public class Logging
    {
        private static ManualLogSource _logSource;

        public static void Log(object data)
        {
            _logSource ??= Logger.CreateLogSource("GorillaShirts");
            _logSource?.Log(LogLevel.Info, data);
        }

        public static void Warning(object data)
        {
            _logSource ??= Logger.CreateLogSource("GorillaShirts");
            _logSource?.Log(LogLevel.Warning, data);
        }

        public static void Error(object data)
        {
            _logSource ??= Logger.CreateLogSource("GorillaShirts");
            _logSource?.Log(LogLevel.Error, data);
        }
    }
}
