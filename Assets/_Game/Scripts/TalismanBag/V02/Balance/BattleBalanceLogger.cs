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
using TalismanBag.V02.Run;
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
        private V02RoundConfig roundAtStart;
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

        public void BeginBattle(int roundNumber, EnemyRuntime enemy, V02RoundConfig roundConfig = null)
        {
            roundIndex = roundNumber;
            roundAtStart = roundConfig;
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

            float battleDuration = Mathf.Max(0f, Time.time - battleStartTime);
            float hpRemain = combatController != null ? combatController.PlayerHpRemainPercent : 0f;
            float hpLoss = Mathf.Clamp01(1f - hpRemain);
            BenchmarkPassFailRule rule = ResolveRule(roundAtStart, enemy);
            string benchmarkResult = ResolveBenchmarkResult(won, battleDuration, hpLoss, counterTriggerCount, enemySkillCastCount, rule, out string reason);
            List<TalismanItemRuntime> items = grid != null ? grid.GetAllPlacedItems() : new List<TalismanItemRuntime>();
            CounterRelation relation = V02CounterBalanceUtility.ResolveBuildRelation(items, enemy, powerResolver);
            string failureReason = string.Empty;
            if (!won)
            {
                if (failureTracker != null && failureTracker.roundLostIndex <= 0)
                {
                    failureTracker.roundLostIndex = roundIndex > 0 ? roundIndex : combatController != null ? combatController.CurrentRoundNumber : 0;
                }

                V02FailureReasonResult failureResult = failureReasonResolver != null
                    ? failureReasonResolver.ResolveFailureReason(failureTracker, null)
                    : default;
                failureReason = failureResult.reason == V02FailureReason.None ? string.Empty : failureResult.reason.ToString();
            }

            BattleBalanceRecord record = new()
            {
                battleId = $"{DateTime.Now:yyyyMMdd-HHmmss}-R{roundIndex}",
                levelId = ResolveLevelId(roundAtStart, roundIndex),
                levelIndex = ResolveLevelIndex(roundAtStart, roundIndex),
                intendedRole = ResolveIntendedRole(roundAtStart),
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
                battleDuration = battleDuration,
                playerHpRemainPercent = hpRemain,
                playerHpLossPercent = hpLoss,
                counterRelation = relation.ToString(),
                playerWon = won,
                counterTriggerCount = counterTriggerCount,
                shieldBreakCount = CountDelta(runStatsTracker != null ? runStatsTracker.shieldBreakCount : 0, startShieldBreak),
                cleanseCount = CountDelta(runStatsTracker != null ? runStatsTracker.cleanseCount : 0, startCleanse),
                soulSuppressCount = CountDelta(runStatsTracker != null ? runStatsTracker.soulSuppressCount : 0, startSoul),
                chainClearCount = CountDelta(runStatsTracker != null ? runStatsTracker.chainClearCount : 0, startChain),
                unsealCount = CountDelta(runStatsTracker != null ? runStatsTracker.unsealCount : 0, startUnseal),
                result = benchmarkResult,
                reason = reason,
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

        private static string ResolveLevelId(V02RoundConfig round, int fallbackRoundIndex)
        {
            if (!string.IsNullOrWhiteSpace(round?.levelId))
            {
                return round.levelId.Trim();
            }

            return fallbackRoundIndex > 0 ? $"1-{fallbackRoundIndex}" : string.Empty;
        }

        private static int ResolveLevelIndex(V02RoundConfig round, int fallbackRoundIndex)
        {
            return round != null && round.roundIndex > 0 ? round.roundIndex : Mathf.Max(0, fallbackRoundIndex);
        }

        private static string ResolveIntendedRole(V02RoundConfig round)
        {
            if (round == null)
            {
                return string.Empty;
            }

            return round.ResolvedIntendedRole;
        }

        private static BenchmarkPassFailRule ResolveRule(V02RoundConfig round, EnemyDefinition enemy)
        {
            BenchmarkPassFailRule configured = round?.benchmarkRule;
            float passDurationMax = configured != null && configured.passDurationMax > 0f
                ? configured.passDurationMax
                : round != null && round.targetDurationMax > 0f ? round.targetDurationMax : GetFallbackDurationMax(enemy);
            float weakDurationMax = configured != null && configured.weakDurationMax > 0f
                ? configured.weakDurationMax
                : passDurationMax * 1.25f;
            float passHpLossMax = configured != null && configured.passHpLossMax > 0f
                ? configured.passHpLossMax
                : round != null && round.targetHpLossMax > 0f ? round.targetHpLossMax : 0.6f;
            float weakHpLossMax = configured != null && configured.weakHpLossMax > 0f
                ? configured.weakHpLossMax
                : Mathf.Clamp01(Mathf.Max(passHpLossMax + 0.2f, passHpLossMax * 1.25f));

            return new BenchmarkPassFailRule
            {
                passDurationMax = passDurationMax,
                weakDurationMax = Mathf.Max(passDurationMax, weakDurationMax),
                passHpLossMax = Mathf.Clamp01(passHpLossMax),
                weakHpLossMax = Mathf.Clamp01(Mathf.Max(passHpLossMax, weakHpLossMax)),
                expectedCounterTriggerMin = configured != null ? Mathf.Max(0, configured.expectedCounterTriggerMin) : 0,
                expectedSkillCastMin = configured != null ? Mathf.Max(0, configured.expectedSkillCastMin) : 0
            };
        }

        private static string ResolveBenchmarkResult(bool won, float duration, float hpLoss, int counters, int skillCasts, BenchmarkPassFailRule rule, out string reason)
        {
            if (!won)
            {
                reason = "battle lost";
                return "Fail";
            }

            if (duration > rule.weakDurationMax)
            {
                reason = $"duration {duration:0.0}s > weak {rule.weakDurationMax:0.0}s";
                return "Fail";
            }

            if (hpLoss > rule.weakHpLossMax)
            {
                reason = $"hp loss {hpLoss:0.0%} > weak {rule.weakHpLossMax:0.0%}";
                return "Fail";
            }

            if (rule.expectedCounterTriggerMin > 0 && counters < rule.expectedCounterTriggerMin)
            {
                reason = $"counter triggers {counters} < expected {rule.expectedCounterTriggerMin}";
                return "Fail";
            }

            if (rule.expectedSkillCastMin > 0 && skillCasts < rule.expectedSkillCastMin)
            {
                reason = $"skill casts {skillCasts} < expected {rule.expectedSkillCastMin}";
                return "Fail";
            }

            if (duration <= rule.passDurationMax && hpLoss <= rule.passHpLossMax)
            {
                reason = "within pass duration and hp loss";
                return "Pass";
            }

            reason = "won but exceeded pass target";
            return "Weak";
        }

        private static float GetFallbackDurationMax(EnemyDefinition enemy)
        {
            return enemy?.enemyId switch
            {
                "mountain_imp_basic" => 60f,
                "turtle_guardian_shield" => 100f,
                "imp_swarm" => 110f,
                "red_poison_beast" => 120f,
                "energy_thief_ghost" => 120f,
                "seal_talisman_taoist" => 130f,
                "formation_breaker_elite" => 140f,
                "shield_swarm_trial" => 70f,
                "poison_seal_thief_trial" => 80f,
                "formation_breaker_boss" => 110f,
                _ => 120f
            };
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
