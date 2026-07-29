using System.Collections.Generic;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.VocabularyExtension
{
    public static class BoneAspectMechanicVocabularyExtensionCatalog
    {
        public static EnemyMechanicVocabularyExtensionSnapshot CreateSnapshot(bool reverseInputOrder = false)
        {
            List<EnemyMechanicVocabularyExtensionEntrySnapshot> entries =
                new List<EnemyMechanicVocabularyExtensionEntrySnapshot>
                {
                    Entry(
                        "ba_gap_candidate.charge_attack",
                        EnemyVocabularyCategory.Mechanic,
                        "mechanic.charge_attack",
                        "冲撞攻击",
                        "敌方以突进或冲撞作为可识别攻击意图的中立机制概念。",
                        "GAP-003"),
                    Entry(
                        "ba_gap_candidate.contested_mark",
                        EnemyVocabularyCategory.Mechanic,
                        "mechanic.contested_mark",
                        "争夺标记",
                        "敌方围绕某目标建立可争夺关系标记的中立机制概念，不定义资源结算。",
                        "GAP-015"),
                    Entry(
                        "ba_gap_candidate.damage_reduction",
                        EnemyVocabularyCategory.Mechanic,
                        "mechanic.damage_reduction",
                        "减伤",
                        "敌方以非护盾方式降低承伤的中立机制概念。",
                        "GAP-016"),
                    Entry(
                        "ba_gap_candidate.possession_state",
                        EnemyVocabularyCategory.Mechanic,
                        "mechanic.possession_state",
                        "附身状态",
                        "敌方进入或施加附身/寄宿关系状态的中立机制概念。",
                        "GAP-001"),
                    Entry(
                        "ba_gap_candidate.status_stack",
                        EnemyVocabularyCategory.Mechanic,
                        "mechanic.status_stack",
                        "状态叠加",
                        "同类敌方状态可累积层数或强度的中立机制概念，不指定状态类型。",
                        "GAP-005"),
                    Entry(
                        "ba_gap_candidate.recognition_reveal_window",
                        EnemyVocabularyCategory.CounterWindowType,
                        "counter_window.recognition_reveal",
                        "识破后显露窗口",
                        "玩家完成识破后出现显露或失防窗口的中立反制类别，不预设净化。",
                        "GAP-010"),
                    Entry(
                        "ba_gap_candidate.weakpoint_exposure_window",
                        EnemyVocabularyCategory.CounterWindowType,
                        "counter_window.weakpoint_exposure",
                        "弱点暴露窗口",
                        "敌方弱点或核心进入可暴露窗口的中立反制类别，不预设触发源。",
                        "GAP-039")
                };
            if (reverseInputOrder)
            {
                entries.Reverse();
            }

            EnemyMechanicVocabularyExtensionSnapshot snapshot =
                new EnemyMechanicVocabularyExtensionSnapshot(
                    EnemyMechanicVocabularyExtensionSchema.SchemaId,
                    EnemyMechanicVocabularyExtensionSchema.SchemaVersion,
                    EnemyMechanicVocabularyExtensionSchema.ExtensionId,
                    EnemyMechanicVocabularyExtensionSchema.BaseSchemaId,
                    EnemyMechanicVocabularyExtensionSchema.BaseSchemaVersion,
                    true,
                    false,
                    false,
                    false,
                    entries);
            EnemyMechanicVocabularyExtensionValidator.Instance.EnsureValid(snapshot);
            return snapshot;
        }

        private static EnemyMechanicVocabularyExtensionEntrySnapshot Entry(
            string candidateId,
            EnemyVocabularyCategory category,
            string stableKey,
            string developerLabelZh,
            string description,
            string surveyRowId)
        {
            return new EnemyMechanicVocabularyExtensionEntrySnapshot(
                candidateId,
                category,
                stableKey,
                developerLabelZh,
                description,
                new[] { surveyRowId },
                false,
                false,
                false);
        }
    }
}
