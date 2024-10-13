using GorillaShirts.Behaviours;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(ModIOMapLoader), "FinalizeUnload"), HarmonyWrapSafe]
    internal class FinalizeUnloadPatch
    {
        public static void Prefix()
        {
            Main.Instance.OnZoneChange([ GTZone.customMaps ]);
        }
    }
}
