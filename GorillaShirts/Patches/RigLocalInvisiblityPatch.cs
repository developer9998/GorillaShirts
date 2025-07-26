using HarmonyLib;
using System;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.SetInvisibleToLocalPlayer))]
    public static class RigLocalInvisiblityPatch
    {
        public static event Action<VRRig, bool> OnSetInvisibleToLocalPlayer;

        public static void Postfix(VRRig __instance, bool invisible)
        {
            OnSetInvisibleToLocalPlayer?.Invoke(__instance, invisible);
        }
    }
}
