using HarmonyLib;
using System;

namespace GorillaShirts.Patches
{

    [HarmonyPatch(typeof(GorillaSetZoneTrigger), "OnBoxTriggered")]
    public class GeometryPatch
    {
        public static event Action<GTZone> OnMapUpdate;
        public static GorillaSetZoneTrigger Current;

        public static void Postfix(GorillaSetZoneTrigger __instance)
        {
            if (__instance == Current) return;
            Current = __instance;

            OnMapUpdate?.Invoke(((GTZone[])AccessTools.Field(__instance.GetType(), "zones").GetValue(__instance))[0]);
        }
    }
}
