using GorillaShirts.Behaviours;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Patches
{
    /*
    [HarmonyPatch(typeof(ModIOMapLoader), "ReplacePlaceholders"), HarmonyWrapSafe]
    internal class ReplacePlaceholderPatch
    {
        public static bool Prefix(GameObject gameObject)
        {
            if (gameObject && gameObject.name == "ShirtStandPlaceholder")
            {
                gameObject.SetActive(false);
                Main.Instance.MoveStand(gameObject.transform);
                return false;
            }

            return true;
        }
    }
    */
}
