using HarmonyLib;
using System;
using System.Linq;

namespace GorillaShirts.Patches
{
    [HarmonyPatch]
    public class ZonePatches
    {
        public static event Action<GTZone[]> OnMapUpdate;
        private static GTZone[] Active = [GTZone.forest];

        [HarmonyPatch(typeof(ZoneManagement), "SetActiveZones"), HarmonyPostfix]
        public static void SetZonePatch(GTZone[] zones)
        {
            if (zones != Active)
            {
                Active = zones;
                OnMapUpdate?.Invoke(zones);
            }
        }

        [HarmonyPatch(typeof(GorillaSetZoneTrigger), "OnBoxTriggered"), HarmonyPostfix]
        public static void RegionTriggerPatch(GorillaSetZoneTrigger __instance)
        {
            GTZone[] zone = (GTZone[])AccessTools.Field(__instance.GetType(), "zones").GetValue(__instance);
            if (zone != Active)
            {
                Active = zone;
                OnMapUpdate?.Invoke(zone);
            }
        }
    }
}
