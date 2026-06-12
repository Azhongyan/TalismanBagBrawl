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
        }

        public void CaptureBattleStart(FormationPowerResolver powerResolver, TalismanBagGrid grid)
        {
            poweredTalismanCountAtBattleStart = 0;
            unpoweredTalismanCountAtBattleStart = 0;

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

        public void RecordCleanse()
        {
            cleanseCount++;
        }

        public void RecordUnseal()
        {
            unsealCount++;
        }

        public void RecordSoulSuppressCounter()
        {
            soulSuppressCount++;
            formationProtectedCount++;
        }

        public string BuildSummary()
        {
            return
                $"\u7834\u76fe {shieldBreakCount}  \u51c0\u5316 {cleanseCount}  \u89e3\u5c01 {unsealCount}\n" +
                $"\u9547\u9b42 {soulSuppressCount}  \u8fde\u9501\u6e05\u573a {chainClearCount}  \u9635\u773c\u4fdd\u62a4 {formationProtectedCount}\n" +
                $"\u5f00\u6218\u4f9b\u80fd {poweredTalismanCountAtBattleStart}  \u672a\u4f9b\u80fd {unpoweredTalismanCountAtBattleStart}\n" +
                $"\u4f9b\u80fd\u4e2d\u65ad {formationPowerLostCount}  \u5c01\u5370\u627f\u53d7 {sealedItemCount}";
        }

        private void OnBattleEvent(BattleEventData eventData)
        {
            string message = eventData.message ?? string.Empty;
            string source = eventData.sourceId ?? string.Empty;

            if (eventData.eventType == BattleEventType.EnemyCountered)
            {
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
    }
}
