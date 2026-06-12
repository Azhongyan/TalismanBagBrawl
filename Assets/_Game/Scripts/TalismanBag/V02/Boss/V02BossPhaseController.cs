using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.Feedback;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Result;
using UnityEngine;

namespace TalismanBag.V02.Boss
{
    public enum V02BossPhase
    {
        None,
        ShieldPhase,
        SummonPhase,
        SealEyePhase
    }

    public sealed class V02BossPhaseController : MonoBehaviour
    {
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private V02FailureTracker failureTracker;
        [SerializeField] private V02RunStatsTracker runStatsTracker;
        [SerializeField] private float firstActionDelay = 4f;
        [SerializeField] private float shieldInterval = 5f;
        [SerializeField] private float summonInterval = 6f;
        [SerializeField] private float sealEyeInterval = 7f;

        private V02BossPhase currentPhase = V02BossPhase.None;
        private float actionTimer;
        private float shieldPhaseSecondTimer;

        public V02BossPhase CurrentPhase => currentPhase;

        public void ResetPhase()
        {
            currentPhase = V02BossPhase.None;
            actionTimer = 0f;
            shieldPhaseSecondTimer = 0f;
        }

        public void Tick(EnemyRuntime enemy, float deltaTime)
        {
            if (enemy?.definition == null || enemy.definition.enemyType != EnemyType.Boss || enemy.definition.maxHp <= 0)
            {
                ResetPhase();
                return;
            }

            V02BossPhase nextPhase = ResolvePhase(enemy);
            if (nextPhase != currentPhase)
            {
                EnterPhase(nextPhase);
            }

            enemy.currentIntentText = GetIntentText(currentPhase);
            if (currentPhase == V02BossPhase.ShieldPhase && failureTracker != null)
            {
                shieldPhaseSecondTimer += deltaTime;
                while (shieldPhaseSecondTimer >= 1f)
                {
                    failureTracker.bossShieldPhaseDuration++;
                    shieldPhaseSecondTimer -= 1f;
                }
            }

            actionTimer -= deltaTime;
            if (actionTimer > 0f)
            {
                return;
            }

            switch (currentPhase)
            {
                case V02BossPhase.ShieldPhase:
                    Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, "\u7834\u9635Boss\u6b63\u5728\u7ed3\u6210\u62a4\u76fe", "boss_phase_shield");
                    actionTimer = shieldInterval;
                    combatController?.V02AddEnemyShield(30, "\u7834\u9635Boss\u7ed3\u6210\u62a4\u76fe");
                    break;
                case V02BossPhase.SummonPhase:
                    Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, "\u7834\u9635Boss\u6b63\u5728\u53ec\u5524\u5c0f\u5996", "boss_phase_summon");
                    actionTimer = summonInterval;
                    if (failureTracker != null)
                    {
                        failureTracker.bossSummonDamageTaken += 12;
                    }

                    combatController?.V02DealDamageToPlayer(12, "\u7834\u9635Boss\u53ec\u5524\u5c0f\u5996\uff0c\u7fa4\u4f53\u538b\u5236\u9020\u6210 12 \u70b9\u4f24\u5bb3");
                    Emit(BattleEventType.EnemyCountered, BattleLogCategory.Danger, "\u5c0f\u5996\u9a9a\u6270", "boss_phase_summon", value: 12);
                    break;
                case V02BossPhase.SealEyePhase:
                    Emit(BattleEventType.EnemyCharging, BattleLogCategory.Danger, "\u7834\u9635Boss\u6b63\u5728\u5c01\u9501\u9635\u773c", "boss_phase_seal");
                    actionTimer = sealEyeInterval;
                    int sealedCount = combatController != null
                        ? combatController.V02SealFormationEyeArea(3f, "\u7834\u9635Boss\u5c01\u9501\u9635\u773c\u9644\u8fd1\u7b26\u7b93")
                        : 0;
                    if (failureTracker != null)
                    {
                        failureTracker.bossEyeSealCount += Mathf.Max(1, sealedCount);
                    }

                    break;
            }
        }

        private static V02BossPhase ResolvePhase(EnemyRuntime enemy)
        {
            float ratio = enemy.currentHp / (float)Mathf.Max(1, enemy.definition.maxHp);
            if (ratio > 0.7f)
            {
                return V02BossPhase.ShieldPhase;
            }

            return ratio > 0.4f ? V02BossPhase.SummonPhase : V02BossPhase.SealEyePhase;
        }

        private static string GetIntentText(V02BossPhase phase)
        {
            return phase switch
            {
                V02BossPhase.ShieldPhase => "\u7834\u9635Boss\u6b63\u5728\u7ed3\u6210\u62a4\u76fe",
                V02BossPhase.SummonPhase => "\u7834\u9635Boss\u6b63\u5728\u53ec\u5524\u5c0f\u5996",
                V02BossPhase.SealEyePhase => "\u7834\u9635Boss\u6b63\u5728\u5c01\u9501\u9635\u773c",
                _ => "\u666e\u901a\u653b\u51fb"
            };
        }

        private void EnterPhase(V02BossPhase phase)
        {
            currentPhase = phase;
            actionTimer = firstActionDelay;
            shieldPhaseSecondTimer = 0f;

            switch (phase)
            {
                case V02BossPhase.ShieldPhase:
                    Emit(BattleEventType.EnemyEnraged, BattleLogCategory.Danger, "Boss\u9636\u6bb5\u53d8\u5316\uff1a\u7ed3\u6210\u62a4\u76fe\uff01", "boss_phase_shield");
                    break;
                case V02BossPhase.SummonPhase:
                    Emit(BattleEventType.EnemyEnraged, BattleLogCategory.Danger, "Boss\u9636\u6bb5\u53d8\u5316\uff1a\u53ec\u5524\u5c0f\u5996\uff01", "boss_phase_summon");
                    break;
                case V02BossPhase.SealEyePhase:
                    Emit(BattleEventType.EnemyEnraged, BattleLogCategory.Danger, "Boss\u9636\u6bb5\u53d8\u5316\uff1a\u5c01\u9501\u9635\u773c\uff01", "boss_phase_seal");
                    break;
            }
        }

        private void Emit(BattleEventType type, BattleLogCategory category, string message, string sourceId, int value = 0)
        {
            eventRouter?.Emit(type, category, message, sourceId, value: value);
        }
    }
}
