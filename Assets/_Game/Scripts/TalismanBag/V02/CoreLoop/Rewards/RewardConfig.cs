using System.Collections.Generic;
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
    }
}
