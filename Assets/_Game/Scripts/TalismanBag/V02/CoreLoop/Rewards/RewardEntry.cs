using System;
using TalismanBag.V02.CoreLoop.Resources;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Rewards
{
    [Serializable]
    public sealed class RewardEntry
    {
        public RewardType rewardType = RewardType.Resource;
        public ResourceType resourceType;
        public string itemId;
        public int amount = 1;
        public string displayName;

        public bool IsValid => amount > 0 && (rewardType switch
        {
            RewardType.Resource => true,
            RewardType.Item => !string.IsNullOrWhiteSpace(itemId),
            _ => false
        });

        public RewardEntry CloneWithAmount(int overrideAmount)
        {
            return new RewardEntry
            {
                rewardType = rewardType,
                resourceType = resourceType,
                itemId = itemId,
                amount = Mathf.Max(0, overrideAmount),
                displayName = displayName
            };
        }
    }
}
