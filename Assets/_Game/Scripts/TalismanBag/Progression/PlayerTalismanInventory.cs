using System.Collections.Generic;
using TalismanBag.Items;
using UnityEngine;

namespace TalismanBag.Progression
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

        public void AddItem(TalismanItemDefinition itemDefinition)
        {
            if (itemDefinition != null)
            {
                items.Add(new TalismanItemRuntime(itemDefinition));
            }
        }

        public List<TalismanItemRuntime> GetAllItems()
        {
            return new List<TalismanItemRuntime>(items);
        }

        public List<TalismanItemRuntime> GetUnplacedItems()
        {
            return items.FindAll(item => item != null && !item.isPlaced);
        }
    }
}
