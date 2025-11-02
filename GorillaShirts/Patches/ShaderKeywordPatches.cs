using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(Shader))]
    internal class ShaderKeywordPatches
    {
        [HarmonyPatch(nameof(Shader.EnableKeyword), argumentTypes: [typeof(string)]), HarmonyPrefix, HarmonyWrapSafe]
        public static void EnableKeywordPatch(string keyword)
        {
            if (keyword != "_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX" || !ShirtManager.HasInstance || ShirtManager.Instance.ShirtStand is not Stand stand) return;
            stand.SetMaterialState(true);
        }

        [HarmonyPatch(nameof(Shader.DisableKeyword), argumentTypes: [typeof(string)]), HarmonyPrefix, HarmonyWrapSafe]
        public static void DisableKeywordPatch(string keyword)
        {
            if (keyword != "_ZONE_DYNAMIC_LIGHTS__CUSTOMVERTEX" || !ShirtManager.HasInstance || ShirtManager.Instance.ShirtStand is not Stand stand) return;
            stand.SetMaterialState(false);
        }
    }
}
