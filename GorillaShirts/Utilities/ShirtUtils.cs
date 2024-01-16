using GorillaNetworking;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Utilities
{
    public static class ShirtUtils
    {
        public static Dictionary<string, Shirt> ShirtDict = new();

        public static void RemoveItem(CosmeticsController.CosmeticCategory category, CosmeticsController.CosmeticSlots slot)
        {
            // https://github.com/developer9998/GorillaCosmetics/blob/c489b4d8eb513d88bb52e33c1e3554be044c3119/GorillaCosmetics/UI/SelectionManager.cs#L453
            try
            {
                bool updateCart = false;

                var nullItem = CosmeticsController.instance.nullItem;

                var items = CosmeticsController.instance.currentWornSet.items;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].itemCategory == category && !items[i].isNullItem)
                    {
                        updateCart = true;
                        items[i] = nullItem;
                    }
                }

                items = CosmeticsController.instance.tryOnSet.items;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].itemCategory == category && !items[i].isNullItem)
                    {
                        updateCart = true;
                        items[i] = nullItem;
                    }
                }

                // TODO: Check if this call is necessary
                if (updateCart)
                {
                    CosmeticsController.instance.UpdateShoppingCart();
                    CosmeticsController.instance.UpdateWornCosmetics(true);

                    PlayerPrefs.SetString(CosmeticsController.CosmeticSet.SlotPlayerPreferenceName(slot), nullItem.itemName);
                    PlayerPrefs.Save();
                }
            }
            catch (Exception e)
            {
                Logging.Error($"Failed to remove game badge\n{e.GetType().Name} ({e.Message})");
            }
        }
    }
}
