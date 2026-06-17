using System;
using System.Collections.Generic;

namespace TalismanBag.V02.CoreLoop.Rewards
{
    [Serializable]
    public sealed class RewardResult
    {
        public string rewardId;
        public string displayName;
        public List<RewardEntry> rewards = new();

        public bool HasRewards => rewards != null && rewards.Count > 0;
    }
}
