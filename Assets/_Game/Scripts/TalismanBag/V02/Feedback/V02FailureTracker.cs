using UnityEngine;

namespace TalismanBag.V02.Feedback
{
    public sealed class V02FailureTracker : MonoBehaviour
    {
        public int roundLostIndex;
        public int shieldNotBrokenCount;
        public int poisonDamageTaken;
        public int burnDamageTaken;
        public int stealEnergyHitCount;
        public int stealEnergyCounteredCount;
        public int sealHitCount;
        public int sealCleansedCount;
        public int unpoweredTriggerBlockedCount;
        public int noDamageDurationSeconds;
        public int bossShieldPhaseDuration;
        public int bossSummonDamageTaken;
        public int bossEyeSealCount;

        public void ResetTracker()
        {
            roundLostIndex = 0;
            shieldNotBrokenCount = 0;
            poisonDamageTaken = 0;
            burnDamageTaken = 0;
            stealEnergyHitCount = 0;
            stealEnergyCounteredCount = 0;
            sealHitCount = 0;
            sealCleansedCount = 0;
            unpoweredTriggerBlockedCount = 0;
            noDamageDurationSeconds = 0;
            bossShieldPhaseDuration = 0;
            bossSummonDamageTaken = 0;
            bossEyeSealCount = 0;
        }
    }
}
