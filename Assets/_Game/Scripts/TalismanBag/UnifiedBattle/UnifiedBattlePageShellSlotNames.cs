namespace TalismanBag.UnifiedBattle
{
    public static class UnifiedBattlePageShellSlotNames
    {
        public const string BattlePageRoot = "BattlePageRoot";
        public const string EnemyInfoArea = "EnemyInfoArea";
        public const string BattleFeedbackLayer = "BattleFeedbackLayer";
        public const string ItemDetailPopupSlot = "ItemDetailPopupSlot";

        // REV11 migration shims keep the retiring Editor tools compilable for
        // the one bounded authoring pass. They are intentionally absent from
        // RequiredSlots and are removed with those tools after validation.
        public const string BoardArea = "BoardArea";
        public const string ItemTrayArea = "ItemTrayArea";
        public const string BossCastBarSlot = "BossCastBarSlot";
        public const string StoryGuidePopupLayer = "StoryGuidePopupLayer";
        public const string ResultRewardPlaceholder = "ResultRewardPlaceholder";
        public const string V03FlowAdapterSlot = "V03FlowAdapterSlot";
        public const string V04SandboxAdapterSlot = "V04SandboxAdapterSlot";
        public const string DevOnlyDiagnosticsSlot = "DevOnlyDiagnosticsSlot";

        public static readonly string[] RequiredSlots =
        {
            BattlePageRoot,
            EnemyInfoArea,
            BattleFeedbackLayer,
            ItemDetailPopupSlot
        };
    }
}
