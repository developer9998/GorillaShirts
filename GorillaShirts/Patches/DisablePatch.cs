using GorillaShirts.Tools;
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
            Networking.Instance.RemoveShirtRig(playerRig);
        }
    }
}
