using HarmonyLib;
using Photon.Realtime;
using System;
using System.Reflection;

namespace GorillaShirts.Utilities
{
    public class RigUtils
    {
        static Assembly _assembly;
        static Type _rigCache, _container;

        static object _rigCacheInstance;

        public static VRRig GetRig(Player player)
        {
            _assembly = typeof(GorillaTagger).Assembly;
            _rigCache = _assembly.GetType("VRRigCache");
            _container = _assembly.GetType("RigContainer");

            _rigCacheInstance = AccessTools.Property(_rigCache, "Instance").GetValue(_rigCache, null);
            if (_rigCacheInstance == null) return null;

            var GetVRRigParams = new object[] { player, null };
            bool GetVRRigMethod = (bool)AccessTools.Method(_rigCache, "TryGetVrrig").Invoke(_rigCache, GetVRRigParams);
            return GetVRRigMethod ? (VRRig)AccessTools.Field(_container, "vrrig").GetValue(GetVRRigParams[1]) : null;
        }
    }
}
