using TalismanBag.Combat;
using TalismanBag.Combo;
using TalismanBag.Debugging;
using TalismanBag.Progression;
using TalismanBag.Run;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UI
{
    public sealed class BattleResultPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Button closeButton;
        [SerializeField] private RunFlowControllerV2 runFlowController;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Restart);
            }

            Hide();
        }

        public void Show(bool victory, BattleStatsSnapshot stats)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = victory ? "通关成功" : "修士败北";
            }

            if (bodyText != null)
            {
                int currentRound = runFlowController != null ? runFlowController.CurrentRoundNumber : 7;
                int totalRounds = runFlowController != null && runFlowController.TotalRounds > 0 ? runFlowController.TotalRounds : 7;
                string comboName = stats.mostActiveComboId == "无" ? "未成型" : ComboResolver.GetComboDisplayName(stats.mostActiveComboId);
                string archetype = BuildArchetypeResolver.Resolve(stats);
                string rating = victory ? BuildArchetypeResolver.Describe(archetype) : GetFailureReason(stats);
                PlaytestSessionLogger.Log($"Detected Archetype: {archetype}");
                bodyText.text =
                    $"最终回合：{currentRound}/{totalRounds}\n\n" +
                    $"本局流派：{archetype}\n" +
                    $"最强阵法：{comboName}\n" +
                    $"核心评价：\n{rating}\n\n" +
                    $"关键数据：\n" +
                    $"造成伤害：{stats.totalDamageDealt}\n" +
                    $"受到伤害：{stats.totalDamageTaken}\n" +
                    $"治疗：{stats.totalHealing}\n" +
                    $"护盾：{stats.totalShieldGained}\n" +
                    $"雷符打断次数：{stats.totalInterruptCount}\n" +
                    $"封印承受次数：{stats.totalSealCount}\n\n" +
                    $"详细数据：\n" +
                    $"灵气产生：{stats.totalManaGenerated}\n" +
                    $"灵气消耗：{stats.totalManaSpent}\n" +
                    $"灵气浪费：{stats.totalManaWasted}\n" +
                    $"购买数量：{stats.totalPurchasedItems}\n" +
                    $"合成数量：{stats.totalMergedItems}\n" +
                    $"Boss蓄力：{stats.bossChargeStartedCount}\n" +
                    $"Boss大招命中：{stats.bossChargeHitCount}\n" +
                    $"Boss打断：{stats.bossChargeInterruptedCount}\n" +
                    $"Boss吸灵：{stats.bossManaDrainCount}\n" +
                    $"Boss封印：{stats.bossSealCount}\n" +
                    $"Boss狂暴：{stats.bossEnrageTriggeredCount}\n" +
                    $"符箓触发次数：{stats.totalTriggerCount}";
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void Restart()
        {
            Hide();
            runFlowController?.RestartRun();
        }

        private static string GetFailureReason(BattleStatsSnapshot stats)
        {
            if (stats.bossChargeHitCount >= 2 && stats.bossChargeInterruptedCount == 0)
            {
                return "失败原因：缺少雷符打断，心魔冲击多次命中。";
            }

            if (stats.bossManaDrainCount >= 2 && stats.totalManaGenerated < stats.totalManaSpent / 2)
            {
                return "失败原因：灵气供应不足，Boss 吸灵后阵法无法稳定运转。";
            }

            if (stats.bossSealCount >= 3)
            {
                return "失败原因：阵型过度依赖单个核心道具，被封印后输出循环崩溃。";
            }

            if (stats.totalManaGenerated < stats.totalManaSpent / 2)
            {
                return "失败原因：灵气供应不足，符箓无法稳定触发。";
            }

            if (stats.totalDamageTaken > stats.totalShieldGained + stats.totalHealing)
            {
                return "失败原因：防御和回复不足，无法承受高压攻击。";
            }

            if (stats.totalInterruptCount == 0)
            {
                return "失败原因：缺少雷符打断，强敌蓄力威胁过高。";
            }

            if (stats.totalSealCount > 3)
            {
                return "失败原因：阵型过度依赖单个核心，被封印后循环崩溃。";
            }

            return "失败原因：阵法循环不稳定，需要优化摆放和商店选择。";
        }
    }
}
