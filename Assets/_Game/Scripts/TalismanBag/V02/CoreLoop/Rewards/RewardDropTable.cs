using System;
using System.Collections.Generic;
using TalismanBag.V02.Config;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Rewards
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/CoreLoop/Reward Drop Table", fileName = "CoreLoopRewardDropTable")]
    public sealed class RewardDropTable : ScriptableObject
    {
        public string tableId;
        public string displayName;
        public List<RewardDropEntry> drops = new();
        public bool isDebugOnly;
        public bool isDeprecated;
        public CatalogSourceType sourceType = CatalogSourceType.Production;

        public List<RewardEntry> Roll()
        {
            return Roll(1);
        }

        public List<RewardEntry> Roll(int rollCount)
        {
            List<RewardEntry> result = new();
            if (drops == null || rollCount <= 0)
            {
                return result;
            }

            for (int rollIndex = 0; rollIndex < rollCount; rollIndex++)
            {
                foreach (RewardDropEntry drop in drops)
                {
                    RewardEntry reward = drop?.Roll();
                    if (reward != null && reward.IsValid)
                    {
                        result.Add(reward);
                    }
                }
            }

            return result;
        }

        public string GetCatalogLabel()
        {
            string readableName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            string readableId = string.IsNullOrWhiteSpace(tableId) ? "no_id" : tableId.Trim();
            return $"{readableName} [{readableId}]";
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
