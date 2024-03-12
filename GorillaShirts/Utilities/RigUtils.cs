using HarmonyLib;
using System;
using System.Reflection;

namespace GorillaShirts.Utilities
{
    public class RigUtils
    {
        static Assembly GorillaAssembly;
        static Type Type_RigCache, Type_RigContainer;

        static object Instance_RigCache;

        public static VRRig GetRig(NetPlayer player)
        {
            GorillaAssembly = typeof(GorillaTagger).Assembly;
            Type_RigCache = GorillaAssembly.GetType("VRRigCache");
            Type_RigContainer = GorillaAssembly.GetType("RigContainer");

            Instance_RigCache = AccessTools.Property(Type_RigCache, "Instance").GetValue(Type_RigCache, null);
            if (Instance_RigCache == null) return null;

            var GetVRRigParams = new object[] { player, null };
            bool GetVRRigMethod = (bool)AccessTools.Method(Type_RigCache, "TryGetVrrig", new Type[] { typeof(NetPlayer), Type_RigContainer }).Invoke(Type_RigCache, GetVRRigParams);
            return GetVRRigMethod ? (VRRig)AccessTools.Field(Type_RigContainer, "vrrig").GetValue(GetVRRigParams[1]) : null;
        }
    }
}
