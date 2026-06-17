using System;
using System.Collections.Generic;

namespace TalismanBag.V02.CoreLoop.Inventory
{
    [Serializable]
    public sealed class PlayerItemInventoryData
    {
        public List<ItemStackData> items = new();

        public int GetAmount(string itemId)
        {
            string safeItemId = NormalizeItemId(itemId);
            if (string.IsNullOrEmpty(safeItemId) || items == null)
            {
                return 0;
            }

            foreach (ItemStackData stack in items)
            {
                if (stack != null && stack.itemId == safeItemId)
                {
                    return Math.Max(0, stack.amount);
                }
            }

            return 0;
        }

        public bool HasItem(string itemId)
        {
            return GetAmount(itemId) > 0;
        }

        public int AddItem(string itemId, int amount)
        {
            string safeItemId = NormalizeItemId(itemId);
            if (string.IsNullOrEmpty(safeItemId) || amount <= 0)
            {
                return GetAmount(safeItemId);
            }

            items ??= new List<ItemStackData>();
            ItemStackData stack = FindStack(safeItemId);
            if (stack == null)
            {
                stack = new ItemStackData(safeItemId, 0);
                items.Add(stack);
            }

            long total = (long)Math.Max(0, stack.amount) + amount;
            stack.amount = total > int.MaxValue ? int.MaxValue : (int)total;
            return stack.amount;
        }

        public void Normalize()
        {
            if (items == null)
            {
                items = new List<ItemStackData>();
                return;
            }

            Dictionary<string, int> merged = new();
            foreach (ItemStackData stack in items)
            {
                string safeItemId = NormalizeItemId(stack?.itemId);
                if (string.IsNullOrEmpty(safeItemId))
                {
                    continue;
                }

                int safeAmount = Math.Max(0, stack.amount);
                if (safeAmount <= 0)
                {
                    continue;
                }

                merged.TryGetValue(safeItemId, out int current);
                long total = (long)current + safeAmount;
                merged[safeItemId] = total > int.MaxValue ? int.MaxValue : (int)total;
            }

            items.Clear();
            foreach (KeyValuePair<string, int> pair in merged)
            {
                items.Add(new ItemStackData(pair.Key, pair.Value));
            }
        }

        private ItemStackData FindStack(string itemId)
        {
            if (items == null)
            {
                return null;
            }

            foreach (ItemStackData stack in items)
            {
                if (stack != null && stack.itemId == itemId)
                {
                    return stack;
                }
            }

            return null;
        }

        private static string NormalizeItemId(string itemId)
        {
            return string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
        }
    }
}
