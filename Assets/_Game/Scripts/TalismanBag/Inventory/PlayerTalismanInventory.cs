using System.Collections.Generic;
using TalismanBag.Items;
using UnityEngine;

namespace TalismanBag.Inventory
{
    public sealed class PlayerTalismanInventory : MonoBehaviour
    {
        private readonly List<TalismanItemRuntime> items = new();

        public void ResetInventory(List<TalismanItemDefinition> startingItems)
        {
            items.Clear();
            if (startingItems == null)
            {
                return;
            }

            foreach (TalismanItemDefinition itemDefinition in startingItems)
            {
                AddItem(itemDefinition);
            }
        }

        public TalismanItemRuntime AddItem(TalismanItemDefinition itemDefinition, int level = 1)
        {
            if (itemDefinition == null)
            {
                return null;
            }

            TalismanItemRuntime item = new(itemDefinition, level);
            items.Add(item);
            return item;
        }

        public void RemoveItem(TalismanItemRuntime item)
        {
            if (item == null)
            {
                return;
            }

            items.Remove(item);
        }

        public List<TalismanItemRuntime> GetAllItems()
        {
            return new List<TalismanItemRuntime>(items);
        }

        public List<TalismanItemRuntime> GetUnplacedItems()
        {
            return items.FindAll(item => item != null && !item.isPlaced);
        }

        public List<TalismanItemRuntime> GetPlacedItems()
        {
            return items.FindAll(item => item != null && item.isPlaced);
        }

        public List<TalismanItemRuntime> GetItemsByItemId(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return new List<TalismanItemRuntime>();
            }

            return items.FindAll(item => item != null && item.definition != null && item.definition.itemId == itemId);
        }

        public bool HasMergeableItems()
        {
            Dictionary<string, int> counts = new();
            foreach (TalismanItemRuntime item in items)
            {
                if (item == null || item.definition == null || item.level != 1)
                {
                    continue;
                }

                string itemId = item.definition.itemId;
                counts.TryGetValue(itemId, out int count);
                count++;
                if (count >= 2)
                {
                    return true;
                }

                counts[itemId] = count;
            }

            return false;
        }
    }
}
