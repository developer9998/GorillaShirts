using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(GorillaIK), "Awake")]
    public class IKPatch
    {
        public static bool Prefix(ref Transform ___leftUpperArm) => ___leftUpperArm != null;
    }
}
