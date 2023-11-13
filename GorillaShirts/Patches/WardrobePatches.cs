using GorillaNetworking;
using HarmonyLib;
using System;
using static GorillaNetworking.CosmeticsController;

namespace GorillaShirts.Patches
{
    [HarmonyPatch]
    public class WardrobePatches
    {
        public static event Action<CosmeticCategory> CosmeticUpdated;

        [HarmonyPostfix, HarmonyPatch(typeof(CosmeticsController), "PressWardrobeItemButton")]
        public static void WardrobeItemPatch(CosmeticsController.CosmeticItem cosmeticItem) => CosmeticUpdated?.Invoke(cosmeticItem.itemCategory);

        [HarmonyPostfix, HarmonyPatch(typeof(CosmeticsController), "PressFittingRoomButton")]
        public static void FittingRoomPatch(FittingRoomButton pressedFittingRoomButton) => CosmeticUpdated?.Invoke(pressedFittingRoomButton.currentCosmeticItem.itemCategory);
    }
}
