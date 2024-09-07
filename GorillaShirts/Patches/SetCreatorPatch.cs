using GorillaShirts.Tools;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(RigContainer), "set_Creator")]
    public class SetCreatorPatch
    {
        [HarmonyWrapSafe]
        public static void Postfix(RigContainer __instance)
        {
            VRRig playerRig = __instance.Rig;
            Networking.Instance.AddShirtRig(playerRig);
        }
    }
}
