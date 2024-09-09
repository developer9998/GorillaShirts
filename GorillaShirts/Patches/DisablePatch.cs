using GorillaShirts.Behaviours;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(RigContainer), "OnDisable")]
    public class DisablePatch
    {
        [HarmonyWrapSafe]
        public static void Postfix(RigContainer __instance)
        {
            VRRig playerRig = __instance.Rig;
            Main.Instance.RemoveShirtRig(playerRig);
        }
    }
}
