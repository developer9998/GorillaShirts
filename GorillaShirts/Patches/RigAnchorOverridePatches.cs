using GorillaShirts.Behaviours;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(VRRigAnchorOverrides))]
    internal class RigAnchorOverridePatches
    {
        [HarmonyPatch("UpdateName")]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix, HarmonyWrapSafe]
        public static bool NamePatch(VRRigAnchorOverrides __instance)
        {
            if (__instance.TryGetComponent(out HumanoidContainer humanoid) && humanoid.NameTagAnchor is not null)
            {
                __instance.nameTransform.parent = humanoid.NameTagAnchor.transform;
                __instance.nameTransform.localPosition = Vector3.zero;
                __instance.nameTransform.localRotation = Quaternion.identity;
                return false;
            }
            return true;
        }

        [HarmonyPatch("UpdateBadge")]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix, HarmonyWrapSafe]
        public static bool BadgePatch(VRRigAnchorOverrides __instance, Transform ___currentBadgeTransform)
        {
            if (___currentBadgeTransform && __instance.TryGetComponent(out HumanoidContainer humanoid) && humanoid.BadgeAnchor is not null)
            {
                ___currentBadgeTransform.localPosition = humanoid.BadgeAnchor.transform.localPosition;
                ___currentBadgeTransform.localRotation = humanoid.BadgeAnchor.transform.localRotation;
                return false;
            }
            return true;
        }
    }
}
