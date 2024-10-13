using HarmonyLib;

namespace GorillaShirts.Patches
{
#if DEBUG
    [HarmonyPatch(typeof(PrivateUIRoom))]
    internal class TestPatch
    {
        [HarmonyPatch("StopOverlay"), HarmonyPrefix]
        public static bool Prefix1() => false;

        [HarmonyPatch("StartOverlay"), HarmonyPrefix]
        public static bool Prefix2() => false;
    }
#endif
}
