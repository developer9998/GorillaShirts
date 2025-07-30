using GorillaShirts.Behaviours;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(TransferrableObject), nameof(TransferrableObject.GetTargetDock), argumentTypes: [typeof(TransferrableObject.PositionState), typeof(BodyDockPositions), typeof(VRRigAnchorOverrides)])]
    [HarmonyWrapSafe]
    internal class TargetDockPatch
    {
        public static bool Prefix(TransferrableObject.PositionState state, BodyDockPositions dockPositions, ref Transform __result)
        {
            if (state == TransferrableObject.PositionState.OnChest && dockPositions.TryGetComponent(out HumanoidContainer humanoid) && humanoid.SlingshotAnchor is not null)
            {
                __result = humanoid.SlingshotAnchor.transform;
                return false;
            }
            return true;
        }
    }
}
