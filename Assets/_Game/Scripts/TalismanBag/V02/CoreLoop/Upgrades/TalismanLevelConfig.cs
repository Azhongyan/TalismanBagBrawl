using System;
using System.Collections.Generic;

namespace TalismanBag.V02.CoreLoop.Upgrades
{
    [Serializable]
    public sealed class TalismanLevelConfig
    {
        public string itemId;
        public string displayName;
        public int fromLevel = 1;
        public int toLevel = 2;
        public List<ResourceCost> costs = new();
        public StatModifier statModifier = new();

        public bool IsValid => !string.IsNullOrWhiteSpace(itemId) && toLevel > fromLevel;

        public bool Matches(string targetItemId, int currentLevel)
        {
            return IsValid
                && string.Equals(itemId, targetItemId, StringComparison.Ordinal)
                && fromLevel == currentLevel;
        }
    }
}
