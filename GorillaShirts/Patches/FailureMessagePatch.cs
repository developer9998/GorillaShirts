using GorillaNetworking;
using GorillaShirts.Behaviours;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(GorillaComputer), nameof(GorillaComputer.GeneralFailureMessage))]
    internal class FailureMessagePatch
    {
        [HarmonyWrapSafe]
        public static void Postfix()
        {
            if (ShirtManager.HasInstance) ShirtManager.Instance.Initialize();
        }
    }
}