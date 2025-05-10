using GorillaShirts.Behaviours.Networking;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(RigContainer), "OnDisable")]
    public class RigDisablePatch
    {
        [HarmonyWrapSafe]
        public static void Postfix(RigContainer __instance)
        {
            if (__instance.TryGetComponent(out NetworkedPlayer networked_player))
            {
                Object.Destroy(networked_player);
            }
        }
    }
}
