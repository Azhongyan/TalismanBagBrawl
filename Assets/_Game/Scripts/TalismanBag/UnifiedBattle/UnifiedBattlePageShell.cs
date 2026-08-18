using System.Collections.Generic;
using TalismanBag.Contracts.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UnifiedBattle
{
    [DisallowMultipleComponent]
    public sealed class UnifiedBattlePageShell : MonoBehaviour
    {
        [Header("Formal Composition Anchors")]
        [SerializeField] private Transform battlePageRoot;
        [SerializeField] private Transform enemyInfoArea;
        [SerializeField] private Transform battleFeedbackLayer;
        [SerializeField] private Transform itemDetailPopupSlot;

        public Transform BattlePageRoot => battlePageRoot;
        public Transform EnemyInfoArea => enemyInfoArea;
        public Transform BattleFeedbackLayer => battleFeedbackLayer;
        public Transform ItemDetailPopupSlot => itemDetailPopupSlot;

        public IReadOnlyDictionary<string, Transform> BuildSlotMap()
        {
            return new Dictionary<string, Transform>
            {
                [UnifiedBattlePageShellSlotNames.BattlePageRoot] =
                    battlePageRoot,
                [UnifiedBattlePageShellSlotNames.EnemyInfoArea] =
                    enemyInfoArea,
                [UnifiedBattlePageShellSlotNames.BattleFeedbackLayer] =
                    battleFeedbackLayer,
                [UnifiedBattlePageShellSlotNames.ItemDetailPopupSlot] =
                    itemDetailPopupSlot
            };
        }

        public List<string> CollectMissingRequiredSlots()
        {
            IReadOnlyDictionary<string, Transform> slots = BuildSlotMap();
            List<string> missing = new List<string>();
            foreach (string requiredSlot in
                     UnifiedBattlePageShellSlotNames.RequiredSlots)
            {
                if (!slots.TryGetValue(requiredSlot, out Transform slot)
                    || slot == null)
                {
                    missing.Add(requiredSlot);
                }
            }

            return missing;
        }

#if UNITY_EDITOR
        public void AssignFormalCompositionForEditor(
            Transform configuredBattlePageRoot,
            Transform configuredEnemyInfoArea,
            Transform configuredBattleFeedbackLayer,
            Transform configuredItemDetailPopupSlot)
        {
            battlePageRoot = configuredBattlePageRoot;
            enemyInfoArea = configuredEnemyInfoArea;
            battleFeedbackLayer = configuredBattleFeedbackLayer;
            itemDetailPopupSlot = configuredItemDetailPopupSlot;
        }

        // Temporary compile shims for the REV11 DELETE inventory. The retired
        // tools are never invoked by REV11 and these methods are removed after
        // the new Shell path validates.
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
            enemyInfoArea = enemyInfo;
            battleFeedbackLayer = feedback;
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
        }

        public void AssignItemDetailPopupSlotForEditor(Transform configuredSlot)
        {
            itemDetailPopupSlot = configuredSlot;
        }

        public bool BindSampleData(
            UnifiedBattlePageShellSampleBinding binding = null)
        {
            return false;
        }

        public bool ResultRewardPlaceholderIsSafe(
            UnifiedBattlePageShellSampleBinding binding = null)
        {
            BattleResultSnapshot result = binding == null
                ? null
                : binding.resultSnapshot;
            return result != null
                   && result.devOnly
                   && !result.shouldWriteSave
                   && !result.shouldGrantReward;
        }
#endif
    }
}
