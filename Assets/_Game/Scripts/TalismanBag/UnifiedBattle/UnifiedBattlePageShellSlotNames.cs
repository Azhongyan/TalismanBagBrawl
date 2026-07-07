namespace TalismanBag.UnifiedBattle
{
    public static class UnifiedBattlePageShellSlotNames
    {
        public const string BattlePageRoot = "BattlePageRoot";
        public const string BoardArea = "BoardArea";
        public const string ItemTrayArea = "ItemTrayArea";
        public const string EnemyInfoArea = "EnemyInfoArea";
        public const string BossCastBarSlot = "BossCastBarSlot";
        public const string BattleFeedbackLayer = "BattleFeedbackLayer";
        public const string StoryGuidePopupLayer = "StoryGuidePopupLayer";
        public const string ResultRewardPlaceholder = "ResultRewardPlaceholder";
        public const string V03FlowAdapterSlot = "V03FlowAdapterSlot";
        public const string V04SandboxAdapterSlot = "V04SandboxAdapterSlot";
        public const string DevOnlyDiagnosticsSlot = "DevOnlyDiagnosticsSlot";

        public static readonly string[] RequiredSlots =
        {
            BattlePageRoot,
            BoardArea,
            ItemTrayArea,
            EnemyInfoArea,
            BossCastBarSlot,
            BattleFeedbackLayer,
            StoryGuidePopupLayer,
            ResultRewardPlaceholder,
            V03FlowAdapterSlot,
            V04SandboxAdapterSlot,
            DevOnlyDiagnosticsSlot
        };
    }
}
