using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
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

                if (Main.HasInstance && Main.Instance.Stand is Stand stand)
                    stand.MoveStand(placeholderGameObject.transform);

                return false;
            }

            return true;
        }
    }
}
