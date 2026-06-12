using System;
using System.Collections.Generic;
using TalismanBag.Items;
using UnityEngine;
using PlayerInventory = TalismanBag.Inventory.PlayerTalismanInventory;

namespace TalismanBag.Progression
{
    [Serializable]
    public sealed class MergeCandidate
    {
        public string itemId;
        public TalismanItemDefinition definition;
        public int availableCount;
    }

    public sealed class ItemMergeSystem : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;

        public void BindInventory(PlayerInventory playerInventory)
        {
            inventory = playerInventory;
        }

        public bool CanMergeAny()
        {
            return inventory != null && inventory.HasMergeableItems();
        }

        public List<MergeCandidate> GetMergeCandidates()
        {
            Dictionary<string, MergeCandidate> candidates = new();
            if (inventory == null)
            {
                return new List<MergeCandidate>();
            }

            foreach (TalismanItemRuntime item in inventory.GetAllItems())
            {
                if (item == null || item.definition == null || item.level != 1)
                {
                    continue;
                }

                string itemId = item.definition.itemId;
                if (!candidates.TryGetValue(itemId, out MergeCandidate candidate))
                {
                    candidate = new MergeCandidate
                    {
                        itemId = itemId,
                        definition = item.definition,
                        availableCount = 0
                    };
                    candidates[itemId] = candidate;
                }

                candidate.availableCount++;
            }

            List<MergeCandidate> result = new();
            foreach (MergeCandidate candidate in candidates.Values)
            {
                if (candidate.availableCount >= 2)
                {
                    result.Add(candidate);
                }
            }

            return result;
        }

        public bool TryMerge(string itemId)
        {
            if (inventory == null)
            {
                return false;
            }

            List<TalismanItemRuntime> items = inventory.GetItemsByItemId(itemId).FindAll(item => item != null && item.level == 1);
            if (items.Count < 2)
            {
                return false;
            }

            TalismanItemDefinition definition = items[0].definition;
            inventory.RemoveItem(items[0]);
            inventory.RemoveItem(items[1]);
            inventory.AddItem(definition, 2);
            return true;
        }

        public int AutoMergeAll()
        {
            int merged = 0;
            bool keepMerging = true;
            while (keepMerging)
            {
                keepMerging = false;
                foreach (MergeCandidate candidate in GetMergeCandidates())
                {
                    if (TryMerge(candidate.itemId))
                    {
                        merged++;
                        keepMerging = true;
                    }
                }
            }

            return merged;
        }
    }
}
