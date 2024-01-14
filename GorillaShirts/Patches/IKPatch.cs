using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(GorillaIK), "Awake")]
    public class IKPatch
    {
        // Check to see if the IK most likely isn't ready to be awaken, based on if the left upper arm has been assigned
        public static bool Prefix(ref Transform ___leftUpperArm) => (bool)___leftUpperArm;
    }
}
