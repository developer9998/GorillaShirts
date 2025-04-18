using GorillaShirts.Behaviours;
using HarmonyLib;
using UnityEngine;
using MapLoader = CustomMapLoader;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(MapLoader), "ReplacePlaceholders"), HarmonyWrapSafe]
    internal class ReplacePlaceholderPatch
    {
        public static bool Prefix(GameObject placeholderGameObject)
        {
            if (placeholderGameObject && placeholderGameObject.name == "ShirtStandPlaceholder")
            {
                placeholderGameObject.SetActive(false);
                Main.Instance.MoveStand(placeholderGameObject.transform);
                return false;
            }

            return true;
        }
    }
}
