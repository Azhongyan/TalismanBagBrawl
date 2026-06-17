using System;
using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Rewards
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/CoreLoop/Reward Drop Table", fileName = "CoreLoopRewardDropTable")]
    public sealed class RewardDropTable : ScriptableObject
    {
        public string tableId;
        public string displayName;
        public List<RewardDropEntry> drops = new();

        public List<RewardEntry> Roll()
        {
            List<RewardEntry> result = new();
            if (drops == null)
            {
                return result;
            }

            foreach (RewardDropEntry drop in drops)
            {
                RewardEntry reward = drop?.Roll();
                if (reward != null && reward.IsValid)
                {
                    result.Add(reward);
                }
            }

            return result;
        }
    }

    [Serializable]
    public sealed class RewardDropEntry
    {
        [Range(0f, 1f)] public float chance = 1f;
        public RewardEntry reward = new();
        public int minAmount = 1;
        public int maxAmount = 1;

        public RewardEntry Roll()
        {
            if (reward == null || !reward.IsValid)
            {
                return null;
            }

            float safeChance = Mathf.Clamp01(chance);
            if (safeChance <= 0f || UnityEngine.Random.value > safeChance)
            {
                return null;
            }

            int min = Mathf.Max(1, minAmount);
            int max = Mathf.Max(min, maxAmount);
            int amount = UnityEngine.Random.Range(min, max + 1);
            return reward.CloneWithAmount(amount);
        }
    }
}
