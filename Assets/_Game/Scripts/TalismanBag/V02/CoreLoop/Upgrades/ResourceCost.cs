using System;
using TalismanBag.V02.CoreLoop.Resources;

namespace TalismanBag.V02.CoreLoop.Upgrades
{
    [Serializable]
    public struct ResourceCost
    {
        public ResourceType resourceType;
        public int amount;

        public ResourceCost(ResourceType resourceType, int amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }

        public bool IsValid => amount > 0;
    }
}
