using HarmonyLib;
using Photon.Realtime;
using System;
using System.Reflection;

namespace GorillaShirts.Utilities
{
    public static class RigUtils
    {
        private static Assembly GTAssembly => typeof(GorillaTagger).Assembly;

        private static Type RigContainer => GTAssembly.GetType("RigContainer");

        private static Type RigCache_Type => GTAssembly.GetType("VRRigCache");

        private static object RicCache_Instance => AccessTools.Property(RigCache_Type, "Instance").GetValue(RigCache_Type, null);

        public static VRRig GetVRRig(NetPlayer player)
        {
            if (RicCache_Instance == null) return default;

            object[] parameters = [player, null];
            bool method = (bool)AccessTools.Method(RigCache_Type, "TryGetVrrig", [typeof(NetPlayer), GTAssembly.GetType("RigContainer&")]).Invoke(RicCache_Instance, parameters);

            if (method)
            {
                return (VRRig)AccessTools.Property(RigContainer, "Rig").GetValue(parameters[1]);
            }

            return default;
        }

        public static VRRig GetPlayerRig(Player player)
        {
            if (RicCache_Instance == null) return default;

            object[] parameters = [player, null];
            bool method = (bool)AccessTools.Method(RigCache_Type, "TryGetVrrig", [typeof(Player), GTAssembly.GetType("RigContainer&")]).Invoke(RicCache_Instance, parameters);

            if (method)
            {
                return (VRRig)AccessTools.Property(RigContainer, "Rig").GetValue(parameters[1]);
            }

            return default;
        }
    }
}
