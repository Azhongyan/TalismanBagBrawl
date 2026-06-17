using System;

namespace TalismanBag.V02.CoreLoop.Resources
{
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType resourceType;
        public int amount;

        public ResourceAmount(ResourceType resourceType, int amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }
    }
}
