using GorillaShirts.Behaviours.Networking;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(RigContainer)), HarmonyWrapSafe]
    internal class RigContainerPatches
    {
        [HarmonyPatch("set_Creator"), HarmonyPostfix]
        public static void CreatorPatch(RigContainer __instance, NetPlayer value)
        {
            if (__instance.GetComponent<NetworkedPlayer>()) return;

            NetworkedPlayer component = __instance.gameObject.AddComponent<NetworkedPlayer>();
            component.Rig = __instance.Rig;
            component.Creator = value;
        }

        [HarmonyPatch(nameof(RigContainer.OnDisable)), HarmonyPostfix]
        public static void DisablePatch(RigContainer __instance)
        {
            if (__instance.TryGetComponent(out NetworkedPlayer component))
            {
                Object.Destroy(component);
            }
        }
    }
}
