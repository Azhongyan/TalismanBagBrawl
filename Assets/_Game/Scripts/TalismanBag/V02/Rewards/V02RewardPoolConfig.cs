using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.V02.Rewards
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/Reward Pool", fileName = "V02RewardPoolConfig")]
    public sealed class V02RewardPoolConfig : ScriptableObject
    {
        public string poolId;
        public List<V02RewardDefinition> rewards = new();
    }
}
