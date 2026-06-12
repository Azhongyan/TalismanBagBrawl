using System;
using System.Collections.Generic;
using TalismanBag.Items;
using UnityEngine;

namespace TalismanBag.Shop
{
    [CreateAssetMenu(menuName = "TalismanBag/Shop Pool", fileName = "ShopPoolConfig")]
    public sealed class ShopPoolConfig : ScriptableObject
    {
        public string poolId;
        public List<ShopPoolEntry> entries = new();

        public ShopPoolEntry FindEntry(TalismanItemDefinition item)
        {
            if (item == null)
            {
                return null;
            }

            foreach (ShopPoolEntry entry in entries)
            {
                if (entry != null && entry.item == item)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    [Serializable]
    public sealed class ShopPoolEntry
    {
        public TalismanItemDefinition item;
        public int price = 6;
        public int weight = 10;
    }
}
