using GorillaShirts.Behaviours;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(ModIOMapLoader), nameof(ModIOMapLoader.CloseDoorAndUnloadMod)), HarmonyWrapSafe]
    internal class FinalizeUnloadPatch
    {
        public static void Prefix()
        {
            if (!ModIOMapLoader.IsModLoaded(0L) && !ModIOMapLoader.IsLoading) return;
            Main.Instance.OnZoneChange([ GTZone.customMaps ]);
        }
    }
}
