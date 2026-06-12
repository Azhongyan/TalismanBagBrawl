using System;
using TalismanBag.Items;

namespace TalismanBag.Shop
{
    [Serializable]
    public sealed class ShopOptionRuntime
    {
        public TalismanItemDefinition item;
        public int price;
        public bool soldOut;

        public ShopOptionRuntime(TalismanItemDefinition item, int price)
        {
            this.item = item;
            this.price = price;
            soldOut = false;
        }
    }
}
