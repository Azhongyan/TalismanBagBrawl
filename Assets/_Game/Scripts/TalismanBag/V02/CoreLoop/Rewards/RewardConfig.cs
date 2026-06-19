using System.Collections.Generic;
using TalismanBag.V02.Config;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Rewards
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/CoreLoop/Reward Config", fileName = "CoreLoopRewardConfig")]
    public sealed class RewardConfig : ScriptableObject
    {
        public string rewardId;
        public string displayName;
        [TextArea] public string description;
        public List<RewardEntry> rewards = new();
        public string onceFlagKey;
        public bool isFirstClearOnly;
        public bool isDebugOnly;
        public bool isDeprecated;
        public CatalogSourceType sourceType = CatalogSourceType.Production;

        public string GetCatalogLabel()
        {
            string readableName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            string readableId = string.IsNullOrWhiteSpace(rewardId) ? "no_id" : rewardId.Trim();
            return $"{readableName} [{readableId}]";
        }

        public static RewardConfig LoadById(string rewardId)
        {
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return null;
            }

            RewardConfig[] configs = UnityEngine.Resources.LoadAll<RewardConfig>("CoreLoop/Rewards");
            foreach (RewardConfig config in configs)
            {
                if (config != null &&
                    string.Equals(config.rewardId, rewardId.Trim(), System.StringComparison.Ordinal))
                {
                    return config;
                }
            }

            return null;
        }
    }
}
