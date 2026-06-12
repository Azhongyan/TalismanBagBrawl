using TalismanBag.Enemies;
using UnityEngine;

namespace TalismanBag.Run
{
    [System.Serializable]
    public sealed class RoundConfig
    {
        public int roundIndex;
        public EnemyDefinition enemy;
        public int rewardSpiritJade;
        public bool unlockBagExpansion;
        public bool isBossRound;
        public string roundTitle;
        public string battleGoalText;
        public string targetDurationText;
        [TextArea] public string roundHint;
    }
}
