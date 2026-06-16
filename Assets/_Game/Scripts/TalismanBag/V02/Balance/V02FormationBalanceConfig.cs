using UnityEngine;

namespace TalismanBag.V02.Balance
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/Formation Balance Config", fileName = "V02FormationBalanceConfig")]
    public sealed class V02FormationBalanceConfig : ScriptableObject
    {
        [Header("Formation Eye")]
        public bool formationEyeCrossPowerEnabled = true;
        public bool formationEyeDiagonalWeakPowerEnabled = true;
        public float weakPowerCooldownMultiplier = 1.35f;
        public bool unpoweredTalismansCanTrigger;

        [Header("Spirit Stone")]
        public bool spiritStoneNineGridPowerEnabled = true;
        public bool spiritLinkBetweenStonesEnabled = true;
        public bool upgradedEyeNineGridEnabled = true;

        [Header("Enemy Disruption Defaults")]
        public float stealEnergyDisableDuration = 3f;
        public float sealDuration = 3f;

        [Header("Display")]
        public bool powerHighlightEnabled = true;
        public bool unpoweredHintEnabled = true;
    }
}
