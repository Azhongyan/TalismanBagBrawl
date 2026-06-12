using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TalismanBag.Combat;
using TalismanBag.Enemies;
using TalismanBag.Feedback;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.V02.Feedback;
using TalismanBag.V02.Formation;
using TalismanBag.V02.Result;
using UnityEngine;

namespace TalismanBag.V02.Balance
{
    public sealed class BattleBalanceLogger : MonoBehaviour
    {
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private FormationPowerResolver powerResolver;
        [SerializeField] private V02RunStatsTracker runStatsTracker;
        [SerializeField] private V02FailureTracker failureTracker;
        [SerializeField] private V02FailureReasonResolver failureReasonResolver;
        [SerializeField] private V02CounterMultiplierConfig multiplierConfig;

        private bool recording;
        private float battleStartTime;
        private int roundIndex;
        private EnemyRuntime enemyAtStart;
        private int enemySkillCastCount;
        private int playerTotalDamage;
        private int playerDamageTaken;
        private int counterTriggerCount;
        private int startShieldBreak;
        private int startCleanse;
        private int startSoul;
        private int startChain;
        private int startUnseal;

        private void Awake()
        {
            if (eventRouter != null)
            {
                eventRouter.BattleEvent += OnBattleEvent;
            }
        }

        private void OnDestroy()
        {
            if (eventRouter != null)
            {
                eventRouter.BattleEvent -= OnBattleEvent;
            }
        }

        public void BeginBattle(int roundNumber, EnemyRuntime enemy)
        {
            roundIndex = roundNumber;
            enemyAtStart = enemy;
            battleStartTime = Time.time;
            enemySkillCastCount = 0;
            playerTotalDamage = 0;
            playerDamageTaken = 0;
            counterTriggerCount = 0;
            startShieldBreak = runStatsTracker != null ? runStatsTracker.shieldBreakCount : 0;
            startCleanse = runStatsTracker != null ? runStatsTracker.cleanseCount : 0;
            startSoul = runStatsTracker != null ? runStatsTracker.soulSuppressCount : 0;
            startChain = runStatsTracker != null ? runStatsTracker.chainClearCount : 0;
            startUnseal = runStatsTracker != null ? runStatsTracker.unsealCount : 0;
            recording = enemy?.definition != null;
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            if (!recording)
            {
                return;
            }

            switch (eventData.eventType)
            {
                case BattleEventType.DamageDealt:
                    playerTotalDamage += Mathf.Max(0, eventData.value);
                    break;
                case BattleEventType.DamageTaken:
                    playerDamageTaken += Mathf.Max(0, eventData.value);
                    break;
                case BattleEventType.EnemyCharging:
                    if (!string.IsNullOrWhiteSpace(eventData.message) || !string.IsNullOrWhiteSpace(eventData.sourceId))
                    {
                        enemySkillCastCount++;
                    }

                    break;
                case BattleEventType.EnemyCountered:
                case BattleEventType.EnemyInterrupted:
                    counterTriggerCount++;
                    break;
                case BattleEventType.BattleWin:
                    EndBattle(true);
                    break;
                case BattleEventType.BattleLose:
                    EndBattle(false);
                    break;
            }
        }

        private void EndBattle(bool won)
        {
            if (!recording)
            {
                return;
            }

            recording = false;
            EnemyDefinition enemy = enemyAtStart?.definition;
            if (enemy == null)
            {
                return;
            }

            List<TalismanItemRuntime> items = grid != null ? grid.GetAllPlacedItems() : new List<TalismanItemRuntime>();
            CounterRelation relation = V02CounterBalanceUtility.ResolveBuildRelation(items, enemy, powerResolver);
            string failureReason = string.Empty;
            if (!won)
            {
                if (failureTracker != null && failureTracker.roundLostIndex <= 0)
                {
                    failureTracker.roundLostIndex = roundIndex > 0 ? roundIndex : combatController != null ? combatController.CurrentRoundNumber : 0;
                }

                V02FailureReasonResult result = failureReasonResolver != null
                    ? failureReasonResolver.ResolveFailureReason(failureTracker, null)
                    : default;
                failureReason = result.reason == V02FailureReason.None ? string.Empty : result.reason.ToString();
            }

            BattleBalanceRecord record = new()
            {
                battleId = $"{DateTime.Now:yyyyMMdd-HHmmss}-R{roundIndex}",
                enemyId = enemy.enemyId,
                enemyClass = enemy.enemyClass,
                enemyArchetype = enemy.enemyArchetype,
                enemyMaxHp = enemy.maxHp,
                enemyAttackDamage = enemy.attackDamage,
                enemyAttackInterval = enemy.attackInterval,
                enemySkillCastCount = enemySkillCastCount,
                playerBuildTags = V02CounterBalanceUtility.BuildTags(items, powerResolver),
                poweredTalismanCount = runStatsTracker != null ? runStatsTracker.poweredTalismanCountAtBattleStart : 0,
                unpoweredTalismanCount = runStatsTracker != null ? runStatsTracker.unpoweredTalismanCountAtBattleStart : 0,
                playerTotalDamage = playerTotalDamage,
                playerDamageTaken = playerDamageTaken,
                battleDuration = Mathf.Max(0f, Time.time - battleStartTime),
                playerHpRemainPercent = combatController != null ? combatController.PlayerHpRemainPercent : 0f,
                counterRelation = relation.ToString(),
                playerWon = won,
                counterTriggerCount = counterTriggerCount,
                shieldBreakCount = CountDelta(runStatsTracker != null ? runStatsTracker.shieldBreakCount : 0, startShieldBreak),
                cleanseCount = CountDelta(runStatsTracker != null ? runStatsTracker.cleanseCount : 0, startCleanse),
                soulSuppressCount = CountDelta(runStatsTracker != null ? runStatsTracker.soulSuppressCount : 0, startSoul),
                chainClearCount = CountDelta(runStatsTracker != null ? runStatsTracker.chainClearCount : 0, startChain),
                unsealCount = CountDelta(runStatsTracker != null ? runStatsTracker.unsealCount : 0, startUnseal),
                inferredFailureReason = failureReason
            };

            string log = record.ToLogString();
            Debug.Log(log);
            AppendLocalLog(log);
        }

        private static int CountDelta(int current, int start)
        {
            return Mathf.Max(0, current - start);
        }

        private static void AppendLocalLog(string log)
        {
#if UNITY_EDITOR
            string path = Path.Combine(Application.dataPath, "../Temp/V02BalanceRecords.log");
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(path, log + Environment.NewLine + Environment.NewLine, Encoding.UTF8);
#endif
        }
    }
}
