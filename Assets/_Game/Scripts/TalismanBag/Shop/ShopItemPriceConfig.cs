using System.Collections.Generic;
using TalismanBag.Items;
using UnityEngine;

namespace TalismanBag.Shop
{
    [System.Serializable]
    public sealed class ShopItemPriceEntry
    {
        public TalismanItemDefinition item;
        public int price;
    }

    [CreateAssetMenu(menuName = "Talisman Bag/Shop Price Config", fileName = "ShopItemPriceConfig")]
    public sealed class ShopItemPriceConfig : ScriptableObject
    {
        public List<ShopItemPriceEntry> prices = new();

        public int GetPrice(TalismanItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            foreach (ShopItemPriceEntry entry in prices)
            {
                if (entry.item == item)
                {
                    return Mathf.Max(0, entry.price);
                }
            }

            return 5;
        }
    }
}
