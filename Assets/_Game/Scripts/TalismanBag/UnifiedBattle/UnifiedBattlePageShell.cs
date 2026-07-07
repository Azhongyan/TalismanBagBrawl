using System.Collections.Generic;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UnifiedBattle
{
    [DisallowMultipleComponent]
    public sealed class UnifiedBattlePageShell : MonoBehaviour
    {
        [Header("Shell Slots")]
        [SerializeField] private Transform battlePageRoot;
        [SerializeField] private Transform boardArea;
        [SerializeField] private Transform itemTrayArea;
        [SerializeField] private Transform enemyInfoArea;
        [SerializeField] private Transform bossCastBarSlot;
        [SerializeField] private Transform battleFeedbackLayer;
        [SerializeField] private Transform storyGuidePopupLayer;
        [SerializeField] private Transform resultRewardPlaceholder;
        [SerializeField] private Transform v03FlowAdapterSlot;
        [SerializeField] private Transform v04SandboxAdapterSlot;
        [SerializeField] private Transform devOnlyDiagnosticsSlot;

        [Header("Sample Text Targets")]
        [SerializeField] private Text boardSummaryText;
        [SerializeField] private Text itemTraySummaryText;
        [SerializeField] private Text enemySummaryText;
        [SerializeField] private Text bossCastSummaryText;
        [SerializeField] private Text feedbackSummaryText;
        [SerializeField] private Text resultRewardSummaryText;
        [SerializeField] private Text v03AdapterSummaryText;
        [SerializeField] private Text v04AdapterSummaryText;
        [SerializeField] private Text devDiagnosticsSummaryText;

        public IReadOnlyDictionary<string, Transform> BuildSlotMap()
        {
            return new Dictionary<string, Transform>
            {
                [UnifiedBattlePageShellSlotNames.BattlePageRoot] = battlePageRoot,
                [UnifiedBattlePageShellSlotNames.BoardArea] = boardArea,
                [UnifiedBattlePageShellSlotNames.ItemTrayArea] = itemTrayArea,
                [UnifiedBattlePageShellSlotNames.EnemyInfoArea] = enemyInfoArea,
                [UnifiedBattlePageShellSlotNames.BossCastBarSlot] = bossCastBarSlot,
                [UnifiedBattlePageShellSlotNames.BattleFeedbackLayer] = battleFeedbackLayer,
                [UnifiedBattlePageShellSlotNames.StoryGuidePopupLayer] = storyGuidePopupLayer,
                [UnifiedBattlePageShellSlotNames.ResultRewardPlaceholder] = resultRewardPlaceholder,
                [UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot] = v03FlowAdapterSlot,
                [UnifiedBattlePageShellSlotNames.V04SandboxAdapterSlot] = v04SandboxAdapterSlot,
                [UnifiedBattlePageShellSlotNames.DevOnlyDiagnosticsSlot] = devOnlyDiagnosticsSlot
            };
        }

        public List<string> CollectMissingRequiredSlots()
        {
            IReadOnlyDictionary<string, Transform> slots = BuildSlotMap();
            List<string> missing = new();
            foreach (string requiredSlot in UnifiedBattlePageShellSlotNames.RequiredSlots)
            {
                if (!slots.TryGetValue(requiredSlot, out Transform slot) || slot == null)
                {
                    missing.Add(requiredSlot);
                }
            }

            return missing;
        }

        public bool BindSampleData(UnifiedBattlePageShellSampleBinding binding = null)
        {
            UnifiedBattlePageShellSampleBinding safeBinding =
                binding ?? UnifiedBattlePageShellSampleData.CreateDefault();
            BattleLayoutSnapshot layout = safeBinding.layoutSnapshot;
            BattleEnemySnapshot enemy = safeBinding.enemySnapshot;
            BuildEvaluationSnapshot build = safeBinding.buildEvaluationSnapshot;
            BattleResultSnapshot result = safeBinding.resultSnapshot;

            SetText(boardSummaryText, $"Board {layout.boardId}: {layout.gridWidth}x{layout.gridHeight}, items={layout.placedItems.Count}");
            SetText(itemTraySummaryText, $"Item tray sample: {layout.placedItems.Count} contract items, no formal inventory write.");
            SetText(enemySummaryText, $"Enemy {enemy.enemyId} / Boss {enemy.bossId}: hp={enemy.hp}, shield={enemy.shield}");
            SetText(bossCastSummaryText, $"Cast {enemy.castSkillId}: progress={enemy.castProgress:0.00}");
            SetText(feedbackSummaryText, build.playerVisibleHints.Count > 0 ? build.playerVisibleHints[0] : "Player-safe feedback placeholder.");
            SetText(resultRewardSummaryText, $"Result placeholder: devOnly={result.devOnly}, writeSave={result.shouldWriteSave}, grantReward={result.shouldGrantReward}");
            SetText(v03AdapterSummaryText, $"V03 adapter slot: {safeBinding.startRequest.sourceController}, formalFlow={safeBinding.startRequest.formalFlow}");
            SetText(v04AdapterSummaryText, $"V04 sandbox adapter slot: build={build.snapshotId}, devOnly={build.devOnly}");
            SetText(
                devDiagnosticsSummaryText,
                $"Dev diagnostics: hardSolutionTags={build.hardSolutionTags.Count}, requiredSynergy={build.requiredSynergy.Count}. Hidden from player slots.");

            return result.devOnly && !result.shouldWriteSave && !result.shouldGrantReward;
        }

        public bool ResultRewardPlaceholderIsSafe(UnifiedBattlePageShellSampleBinding binding = null)
        {
            BattleResultSnapshot result =
                (binding ?? UnifiedBattlePageShellSampleData.CreateDefault()).resultSnapshot;
            return result.devOnly && !result.shouldWriteSave && !result.shouldGrantReward;
        }

        public void AssignSlotsForEditor(
            Transform battleRoot,
            Transform board,
            Transform itemTray,
            Transform enemyInfo,
            Transform bossCast,
            Transform feedback,
            Transform storyGuide,
            Transform resultReward,
            Transform v03Adapter,
            Transform v04Adapter,
            Transform devDiagnostics)
        {
            battlePageRoot = battleRoot;
            boardArea = board;
            itemTrayArea = itemTray;
            enemyInfoArea = enemyInfo;
            bossCastBarSlot = bossCast;
            battleFeedbackLayer = feedback;
            storyGuidePopupLayer = storyGuide;
            resultRewardPlaceholder = resultReward;
            v03FlowAdapterSlot = v03Adapter;
            v04SandboxAdapterSlot = v04Adapter;
            devOnlyDiagnosticsSlot = devDiagnostics;
        }

        public void AssignSampleTextTargetsForEditor(
            Text board,
            Text itemTray,
            Text enemy,
            Text bossCast,
            Text feedback,
            Text resultReward,
            Text v03Adapter,
            Text v04Adapter,
            Text devDiagnostics)
        {
            boardSummaryText = board;
            itemTraySummaryText = itemTray;
            enemySummaryText = enemy;
            bossCastSummaryText = bossCast;
            feedbackSummaryText = feedback;
            resultRewardSummaryText = resultReward;
            v03AdapterSummaryText = v03Adapter;
            v04AdapterSummaryText = v04Adapter;
            devDiagnosticsSummaryText = devDiagnostics;
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }
    }
}
