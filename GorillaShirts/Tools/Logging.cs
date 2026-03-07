using MelonLoader;

namespace GorillaShirts.Tools
{
    internal static class Logging
    {
        public static void Message(object data) => Melon<Plugin>.Logger.Msg(data);

        public static void Info(object data) => Melon<Plugin>.Logger.Msg(MelonLoader.Logging.ColorARGB.Gray, data);

        public static void Warning(object data) => Melon<Plugin>.Logger.Warning(data);

        public static void Error(object data) => Melon<Plugin>.Logger.Error(data);

        public static void Fatal(object data) => Melon<Plugin>.Logger.Error(data);
    }
}
