using System.Text;
using TalismanBag.Combat;
using TalismanBag.Feedback;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.V02.Formation;
using UnityEngine;

namespace TalismanBag.V02.Result
{
    public sealed class V02RunStatsTracker : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;

        public int shieldBreakCount;
        public int cleanseCount;
        public int unsealCount;
        public int soulSuppressCount;
        public int chainClearCount;
        public int formationProtectedCount;

        public int poweredTalismanCountAtBattleStart;
        public int unpoweredTalismanCountAtBattleStart;

        public int formationPowerLostCount;
        public int sealedItemCount;

        public float battleDurationSeconds;
        public int battleDamageDealt;
        public int battleDamageTaken;
        public int battleCounterTriggerCount;
        public int battleTalismanTriggerCount;
        public bool battleFinished;
        public bool battleLost;
        public string battleEndReason = "\u672a\u5f00\u59cb";

        private bool battleTimerRunning;

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

        public void ResetStats()
        {
            shieldBreakCount = 0;
            cleanseCount = 0;
            unsealCount = 0;
            soulSuppressCount = 0;
            chainClearCount = 0;
            formationProtectedCount = 0;
            poweredTalismanCountAtBattleStart = 0;
            unpoweredTalismanCountAtBattleStart = 0;
            formationPowerLostCount = 0;
            sealedItemCount = 0;
            ResetBattleDebugStats("\u672a\u5f00\u59cb");
        }

        public void CaptureBattleStart(FormationPowerResolver powerResolver, TalismanBagGrid grid)
        {
            poweredTalismanCountAtBattleStart = 0;
            unpoweredTalismanCountAtBattleStart = 0;
            ResetBattleDebugStats("\u6218\u6597\u4e2d");
            battleTimerRunning = true;

            if (grid == null)
            {
                return;
            }

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition == null)
                {
                    continue;
                }

                bool needsPower = item.definition.requiresFormationPower;
                bool isPowered = !needsPower || (powerResolver != null && powerResolver.IsItemPowered(item));
                if (isPowered)
                {
                    poweredTalismanCountAtBattleStart++;
                }
                else
                {
                    unpoweredTalismanCountAtBattleStart++;
                }
            }
        }

        public void TickBattle(float deltaTime)
        {
            if (battleTimerRunning)
            {
                battleDurationSeconds += Mathf.Max(0f, deltaTime);
            }
        }

        public void RecordCleanse()
        {
            cleanseCount++;
            battleCounterTriggerCount++;
        }

        public void RecordUnseal()
        {
            unsealCount++;
            battleCounterTriggerCount++;
        }

        public void RecordSoulSuppressCounter()
        {
            soulSuppressCount++;
            formationProtectedCount++;
            battleCounterTriggerCount++;
        }

        public string BuildSummary()
        {
            return
                $"\u7834\u76fe {shieldBreakCount}  \u51c0\u5316 {cleanseCount}  \u89e3\u5c01 {unsealCount}\n" +
                $"\u9547\u9b42 {soulSuppressCount}  \u8fde\u9501\u6e05\u573a {chainClearCount}  \u9635\u773c\u4fdd\u62a4 {formationProtectedCount}\n" +
                $"\u5f00\u6218\u4f9b\u80fd {poweredTalismanCountAtBattleStart}  \u672a\u4f9b\u80fd {unpoweredTalismanCountAtBattleStart}\n" +
                $"\u4f9b\u80fd\u4e2d\u65ad {formationPowerLostCount}  \u5c01\u5370\u627f\u53d7 {sealedItemCount}";
        }

        public string BuildBattleDebugSummary(AutoCombatController combatController)
        {
            float duration = Mathf.Max(0f, battleDurationSeconds);
            float dpsDuration = Mathf.Max(0.01f, duration);
            float playerDps = battleDamageDealt / dpsDuration;
            float enemyDps = battleDamageTaken / dpsDuration;
            CombatStats playerStats = combatController != null ? combatController.PlayerStats : null;
            string hpText = playerStats != null
                ? $"{playerStats.hp}/{playerStats.maxHP} ({(playerStats.maxHP > 0 ? playerStats.hp / (float)playerStats.maxHP * 100f : 0f):0.#}%)"
                : "\u672a\u77e5";

            StringBuilder builder = new();
            builder.AppendLine("[\u6218\u6597\u7edf\u8ba1]");
            builder.AppendLine($"\u6218\u6597\u65f6\u957f\uff1a{duration:0.0}s");
            builder.AppendLine($"\u73a9\u5bb6\u5269\u4f59\u8840\u91cf\uff1a{hpText}");
            builder.AppendLine($"\u73a9\u5bb6 DPS\uff1a{playerDps:0.0}");
            builder.AppendLine($"\u654c\u4eba DPS\uff1a{enemyDps:0.0}");
            builder.AppendLine($"\u514b\u5236\u89e6\u53d1\u6b21\u6570\uff1a{battleCounterTriggerCount}");
            builder.AppendLine($"\u7b26\u7b93\u89e6\u53d1\u6b21\u6570\uff1a{battleTalismanTriggerCount}");
            builder.AppendLine($"\u6b7b\u4ea1\u539f\u56e0\uff1a{GetDeathReason()}");
            builder.AppendLine($"\u9020\u6210\u4f24\u5bb3\uff1a{battleDamageDealt}");
            builder.AppendLine($"\u53d7\u5230\u4f24\u5bb3\uff1a{battleDamageTaken}");
            return builder.ToString().TrimEnd();
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            string message = eventData.message ?? string.Empty;
            string source = eventData.sourceId ?? string.Empty;

            switch (eventData.eventType)
            {
                case BattleEventType.DamageDealt:
                    battleDamageDealt += Mathf.Max(0, eventData.value);
                    break;
                case BattleEventType.DamageTaken:
                    battleDamageTaken += Mathf.Max(0, eventData.value);
                    break;
                case BattleEventType.ItemTriggered:
                    battleTalismanTriggerCount += Mathf.Max(1, eventData.value);
                    break;
                case BattleEventType.BattleWin:
                    CompleteBattle(false, FirstText(message, "\u80dc\u5229"));
                    break;
                case BattleEventType.BattleLose:
                    CompleteBattle(true, FirstText(message, "\u73a9\u5bb6\u6b7b\u4ea1"));
                    break;
            }

            if (eventData.eventType == BattleEventType.EnemyCountered)
            {
                battleCounterTriggerCount++;
                if (message.Contains("\u7834\u76fe") || source.Contains("thunder"))
                {
                    shieldBreakCount++;
                }

                if (message.Contains("\u8fde\u9501") || source.Contains("chain_thunder"))
                {
                    chainClearCount++;
                }

                if (message.Contains("\u9547\u9b42") || source.Contains("soul_suppress"))
                {
                    soulSuppressCount++;
                }

                if (message.Contains("\u9635\u773c\u4fdd\u62a4"))
                {
                    formationProtectedCount++;
                }
            }

            if (eventData.eventType == BattleEventType.EnemyInterrupted && (message.Contains("\u9547\u9b42") || message.Contains("\u9635\u773c\u4fdd\u62a4")))
            {
                soulSuppressCount++;
                formationProtectedCount++;
            }

            if (eventData.eventType == BattleEventType.ItemSealed)
            {
                sealedItemCount += Mathf.Max(1, eventData.value);
            }

            if (eventData.eventType == BattleEventType.ManaSpent && eventData.logCategory == BattleLogCategory.Danger)
            {
                formationPowerLostCount++;
            }
        }

        private void ResetBattleDebugStats(string reason)
        {
            battleDurationSeconds = 0f;
            battleDamageDealt = 0;
            battleDamageTaken = 0;
            battleCounterTriggerCount = 0;
            battleTalismanTriggerCount = 0;
            battleFinished = false;
            battleLost = false;
            battleEndReason = reason;
            battleTimerRunning = false;
        }

        private void CompleteBattle(bool lost, string reason)
        {
            battleTimerRunning = false;
            battleFinished = true;
            battleLost = lost;
            battleEndReason = reason;
        }

        private string GetDeathReason()
        {
            if (!battleFinished)
            {
                return battleTimerRunning ? "\u6218\u6597\u4e2d" : battleEndReason;
            }

            return battleLost ? FirstText(battleEndReason, "\u73a9\u5bb6\u6b7b\u4ea1") : "\u672a\u6b7b\u4ea1 / \u80dc\u5229";
        }

        private static string FirstText(params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(values[i]))
                {
                    return values[i];
                }
            }

            return string.Empty;
        }
    }
}
