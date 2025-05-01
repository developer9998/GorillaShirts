using GorillaShirts.Behaviours;
using HarmonyLib;
using MapLoader = CustomMapLoader;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(MapLoader), nameof(MapLoader.CloseDoorAndUnloadMod)), HarmonyWrapSafe]
    internal class FinalizeUnloadPatch
    {
        public static void Prefix()
        {
            if (!MapLoader.IsModLoaded(0L) && !MapLoader.IsLoading) return;
            Main.Instance.OnZoneChange([GTZone.customMaps]);
        }
    }
}
