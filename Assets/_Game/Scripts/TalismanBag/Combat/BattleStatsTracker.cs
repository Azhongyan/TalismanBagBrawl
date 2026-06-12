using System.Collections.Generic;
using TalismanBag.Combo;
using TalismanBag.Feedback;
using UnityEngine;

namespace TalismanBag.Combat
{
    public sealed class BattleStatsTracker : MonoBehaviour
    {
        [SerializeField] private BattleEventRouter eventRouter;
        [SerializeField] private ComboResolver comboResolver;

        public int totalDamageDealt;
        public int totalDamageTaken;
        public int totalHealing;
        public int totalShieldGained;
        public int totalManaGenerated;
        public int totalManaSpent;
        public int totalManaWasted;
        public int totalTriggerCount;
        public int totalInterruptCount;
        public int totalSealCount;
        public int totalPurchasedItems;
        public int totalMergedItems;
        public int bossChargeStartedCount;
        public int bossChargeHitCount;
        public int bossChargeInterruptedCount;
        public int bossSealCount;
        public int bossManaDrainCount;
        public int bossEnrageTriggeredCount;
        public string mostUsedItemId = "无";
        public string mostActiveComboId = "无";

        private readonly Dictionary<string, int> itemTriggers = new();
        private readonly Dictionary<string, int> comboTicks = new();

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

        private void Update()
        {
            if (comboResolver == null)
            {
                return;
            }

            foreach (string comboId in comboResolver.GetActiveComboIds())
            {
                comboTicks.TryGetValue(comboId, out int count);
                comboTicks[comboId] = count + 1;
            }
        }

        public void ResetStats()
        {
            totalDamageDealt = 0;
            totalDamageTaken = 0;
            totalHealing = 0;
            totalShieldGained = 0;
            totalManaGenerated = 0;
            totalManaSpent = 0;
            totalManaWasted = 0;
            totalTriggerCount = 0;
            totalInterruptCount = 0;
            totalSealCount = 0;
            totalPurchasedItems = 0;
            totalMergedItems = 0;
            bossChargeStartedCount = 0;
            bossChargeHitCount = 0;
            bossChargeInterruptedCount = 0;
            bossSealCount = 0;
            bossManaDrainCount = 0;
            bossEnrageTriggeredCount = 0;
            mostUsedItemId = "无";
            mostActiveComboId = "无";
            itemTriggers.Clear();
            comboTicks.Clear();
        }

        public BattleStatsSnapshot CreateSnapshot()
        {
            return new BattleStatsSnapshot
            {
                totalDamageDealt = totalDamageDealt,
                totalDamageTaken = totalDamageTaken,
                totalHealing = totalHealing,
                totalShieldGained = totalShieldGained,
                totalManaGenerated = totalManaGenerated,
                totalManaSpent = totalManaSpent,
                totalManaWasted = totalManaWasted,
                totalTriggerCount = totalTriggerCount,
                totalInterruptCount = totalInterruptCount,
                totalSealCount = totalSealCount,
                totalPurchasedItems = totalPurchasedItems,
                totalMergedItems = totalMergedItems,
                bossChargeStartedCount = bossChargeStartedCount,
                bossChargeHitCount = bossChargeHitCount,
                bossChargeInterruptedCount = bossChargeInterruptedCount,
                bossSealCount = bossSealCount,
                bossManaDrainCount = bossManaDrainCount,
                bossEnrageTriggeredCount = bossEnrageTriggeredCount,
                mostUsedItemId = GetTopKey(itemTriggers, "无"),
                mostActiveComboId = GetTopKey(comboTicks, "无")
            };
        }

        public void RecordBossChargeStarted()
        {
            bossChargeStartedCount++;
        }

        public void RecordBossChargeHit()
        {
            bossChargeHitCount++;
        }

        public void RecordBossChargeInterrupted()
        {
            bossChargeInterruptedCount++;
        }

        public void RecordBossSeal()
        {
            bossSealCount++;
        }

        public void RecordBossManaDrain()
        {
            bossManaDrainCount++;
        }

        public void RecordBossEnrage()
        {
            bossEnrageTriggeredCount++;
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            switch (eventData.eventType)
            {
                case BattleEventType.ManaGenerated:
                    totalManaGenerated += eventData.value;
                    break;
                case BattleEventType.ManaSpent:
                    totalManaSpent += eventData.value;
                    break;
                case BattleEventType.DamageDealt:
                    totalDamageDealt += eventData.value;
                    break;
                case BattleEventType.DamageTaken:
                    totalDamageTaken += eventData.value;
                    break;
                case BattleEventType.HealReceived:
                    totalHealing += eventData.value;
                    break;
                case BattleEventType.ShieldGained:
                    totalShieldGained += eventData.value;
                    break;
                case BattleEventType.ItemTriggered:
                    totalTriggerCount++;
                    if (!string.IsNullOrEmpty(eventData.sourceId))
                    {
                        itemTriggers.TryGetValue(eventData.sourceId, out int count);
                        itemTriggers[eventData.sourceId] = count + 1;
                    }
                    break;
                case BattleEventType.EnemyInterrupted:
                    totalInterruptCount++;
                    break;
                case BattleEventType.ItemSealed:
                    totalSealCount++;
                    break;
            }
        }

        private static string GetTopKey(Dictionary<string, int> source, string fallback)
        {
            string bestKey = fallback;
            int bestCount = 0;
            foreach (KeyValuePair<string, int> pair in source)
            {
                if (pair.Value > bestCount)
                {
                    bestKey = pair.Key;
                    bestCount = pair.Value;
                }
            }

            return bestKey;
        }
    }

    public struct BattleStatsSnapshot
    {
        public int totalDamageDealt;
        public int totalDamageTaken;
        public int totalHealing;
        public int totalShieldGained;
        public int totalManaGenerated;
        public int totalManaSpent;
        public int totalManaWasted;
        public int totalTriggerCount;
        public int totalInterruptCount;
        public int totalSealCount;
        public int totalPurchasedItems;
        public int totalMergedItems;
        public int bossChargeStartedCount;
        public int bossChargeHitCount;
        public int bossChargeInterruptedCount;
        public int bossSealCount;
        public int bossManaDrainCount;
        public int bossEnrageTriggeredCount;
        public string mostUsedItemId;
        public string mostActiveComboId;
    }
}
