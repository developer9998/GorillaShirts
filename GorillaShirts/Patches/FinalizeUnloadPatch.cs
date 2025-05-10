using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using HarmonyLib;
using MapLoader = CustomMapLoader;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.CloseDoorAndUnloadMod)), HarmonyWrapSafe]
    internal class FinalizeUnloadPatch
    {
        public static void Prefix()
        {
            if (!MapLoader.IsModLoaded(0L) && !MapLoader.IsLoading)
                return;

            if (Main.HasInstance && Main.Instance.Stand is Stand stand)
                stand.OnZoneChange([GTZone.customMaps]);
        }
    }
}
