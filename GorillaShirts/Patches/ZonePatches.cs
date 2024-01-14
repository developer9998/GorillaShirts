using HarmonyLib;
using System;
using System.Linq;

namespace GorillaShirts.Patches
{
    [HarmonyPatch]
    public class ZonePatches
    {
        public static event Action<GTZone> OnMapUpdate;
        private static GTZone Active = GTZone.forest;


        [HarmonyPatch(typeof(ZoneManagement), "SetActiveZone"), HarmonyPostfix]
        public static void SetZonePatch(GTZone zone)
        {
            if (zone != Active)
            {
                Active = zone;
                OnMapUpdate?.Invoke(zone);
            }
        }

        [HarmonyPatch(typeof(GorillaSetZoneTrigger), "OnBoxTriggered"), HarmonyPostfix]
        public static void RegionTriggerPatch(GorillaSetZoneTrigger __instance)
        {
            GTZone zone = ((GTZone[])AccessTools.Field(__instance.GetType(), "zones").GetValue(__instance)).First();
            if (zone != Active)
            {
                Active = zone;
                OnMapUpdate?.Invoke(zone);
            }
        }
    }
}
