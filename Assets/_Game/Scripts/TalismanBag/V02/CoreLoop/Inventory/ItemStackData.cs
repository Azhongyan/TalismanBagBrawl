using System;

namespace TalismanBag.V02.CoreLoop.Inventory
{
    [Serializable]
    public sealed class ItemStackData
    {
        public string itemId;
        public int amount;

        public ItemStackData()
        {
        }

        public ItemStackData(string itemId, int amount)
        {
            this.itemId = itemId;
            this.amount = Math.Max(0, amount);
        }
    }
}
