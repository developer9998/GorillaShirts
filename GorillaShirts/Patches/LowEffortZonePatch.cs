using System;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(LowEffortZone), nameof(LowEffortZone.OnBoxTriggered))]
    public static class LowEffortZonePatch
    {
        public static event Action<LowEffortZone> OnBoxTriggered;

        public static void Postfix(LowEffortZone __instance)
        {
            OnBoxTriggered?.Invoke(__instance);
        }
    }
}
