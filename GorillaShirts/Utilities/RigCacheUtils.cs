using HarmonyLib;
using Photon.Realtime;
using System;
using System.Reflection;

namespace GorillaShirts.Utilities
{
    public static class RigCacheUtils
    {
        private static Assembly GTAssembly => typeof(GorillaTagger).Assembly;

        private static Type RigCacheType => GTAssembly.GetType("VRRigCache");
        private static Type ContainerType => GTAssembly.GetType("RigContainer");

        private static object CacheInstance => AccessTools.Property(RigCacheType, "Instance").GetValue(RigCacheType, null);

        public static T GetField<T>(NetPlayer player)
        {
            if (CacheInstance == null) return default;

            object[] parameters = new object[] { player, null };
            bool method = (bool)AccessTools.Method(RigCacheType, "TryGetVrrig", new Type[] { typeof(NetPlayer), RigCacheType }).Invoke(CacheInstance, parameters);

            if (method)
            {
                string propertyName = PropertyName(typeof(T));
                return (T)AccessTools.Property(ContainerType, propertyName).GetValue(parameters[1]);
            }

            return default;
        }

        private static string PropertyName(Type type) => type.Name switch
        {
            "VRRig" => "Rig",
            "PhotonVoiceView" => "Voice",
            "PhotonView" => "photonView",
            "Boolean" => "Muted",
            "bool" => "Muted",
            _ => throw new IndexOutOfRangeException(type.FullName)
        };
    }
}
