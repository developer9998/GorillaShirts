using HarmonyLib;
using Photon.Realtime;

namespace GorillaShirts.Utilities
{
    public class RigUtils
    {
        private static object _rigCache;

        public static VRRig GetRig(Player player)
        {
            var GorillaAssembly = typeof(GorillaTagger).Assembly;
            var RigCacheType = GorillaAssembly.GetType("VRRigCache");
            var ContainerType = GorillaAssembly.GetType("RigContainer");

            _rigCache ??= AccessTools.Property(RigCacheType, "Instance").GetValue(RigCacheType, null);
            if (_rigCache == null) return null;

            var GetVRRigParams = new object[] { player, null };
            bool GetVRRigMethod = (bool)AccessTools.Method(RigCacheType, "TryGetVrrig").Invoke(_rigCache, GetVRRigParams);
            return GetVRRigMethod ? (VRRig)AccessTools.Field(ContainerType, "vrrig").GetValue(GetVRRigParams[1]) : null;
        }
    }
}
