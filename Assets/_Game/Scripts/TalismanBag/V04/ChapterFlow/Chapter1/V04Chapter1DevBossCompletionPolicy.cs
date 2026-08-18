namespace TalismanBag.V04.ChapterFlow.Chapter1
{
    public sealed class V04Chapter1DevBossCompletionPolicyDefinition
    {
        public readonly string schemaId;
        public readonly string chapterId;
        public readonly string stageId;
        public readonly string bossProfileId;
        public readonly string chapterFlowTruthOwnerId;
        public readonly string completionResolverSlotId;
        public readonly string currentSourceContractId;
        public readonly string currentTerminalFactId;
        public readonly bool temporaryClearAllowed;
        public readonly bool chapterFlowTruthMustRemainUnchanged;
        public readonly string replacementMode;
        public readonly string labelA;
        public readonly string labelB;
        public readonly string labelC;
        public readonly bool ownsChapterFlowTruth;
        public readonly bool directRuntimeInvocation;
        public readonly bool directBattleInvocation;
        public readonly bool devOnly;
        public readonly bool isEnabled;
        public readonly bool formalFlow;

        public V04Chapter1DevBossCompletionPolicyDefinition(
            string schemaId,
            string chapterId,
            string stageId,
            string bossProfileId,
            string chapterFlowTruthOwnerId,
            string completionResolverSlotId,
            string currentSourceContractId,
            string currentTerminalFactId,
            bool temporaryClearAllowed,
            bool chapterFlowTruthMustRemainUnchanged,
            string replacementMode,
            string labelA,
            string labelB,
            string labelC,
            bool ownsChapterFlowTruth,
            bool directRuntimeInvocation,
            bool directBattleInvocation,
            bool devOnly,
            bool isEnabled,
            bool formalFlow)
        {
            this.schemaId = schemaId ?? string.Empty;
            this.chapterId = chapterId ?? string.Empty;
            this.stageId = stageId ?? string.Empty;
            this.bossProfileId = bossProfileId ?? string.Empty;
            this.chapterFlowTruthOwnerId = chapterFlowTruthOwnerId ?? string.Empty;
            this.completionResolverSlotId = completionResolverSlotId ?? string.Empty;
            this.currentSourceContractId = currentSourceContractId ?? string.Empty;
            this.currentTerminalFactId = currentTerminalFactId ?? string.Empty;
            this.temporaryClearAllowed = temporaryClearAllowed;
            this.chapterFlowTruthMustRemainUnchanged =
                chapterFlowTruthMustRemainUnchanged;
            this.replacementMode = replacementMode ?? string.Empty;
            this.labelA = labelA ?? string.Empty;
            this.labelB = labelB ?? string.Empty;
            this.labelC = labelC ?? string.Empty;
            this.ownsChapterFlowTruth = ownsChapterFlowTruth;
            this.directRuntimeInvocation = directRuntimeInvocation;
            this.directBattleInvocation = directBattleInvocation;
            this.devOnly = devOnly;
            this.isEnabled = isEnabled;
            this.formalFlow = formalFlow;
        }
    }

    public static class V04Chapter1DevBossCompletionPolicy
    {
        public const string SchemaId = "V04Chapter1DevBossCompletionPolicy.v1";
        public const string Phase1DevVerticalSlice = "PHASE1_DEV_VERTICAL_SLICE";
        public const string NotContentFinal = "NOT_CONTENT_FINAL";
        public const string NotFormalBossCompletion =
            "NOT_FORMAL_BOSS_COMPLETION";

        private static readonly V04Chapter1DevBossCompletionPolicyDefinition Policy =
            new(
                SchemaId,
                "bone_aspect_chapter_1",
                "1-10",
                "bone_aspect_boss_c1_bone_guard",
                V04ChapterFlowManifest.SchemaId,
                "bline.c1.boss_completion_resolver",
                "ShougunuPhase1RuntimeAndActionContract.v1",
                "ShougunuPhase1LifecycleState.Defeated",
                true,
                true,
                "REPLACE_COMPLETION_RESOLVER_ONLY",
                Phase1DevVerticalSlice,
                NotContentFinal,
                NotFormalBossCompletion,
                false,
                false,
                false,
                true,
                false,
                false);

        public static V04Chapter1DevBossCompletionPolicyDefinition Current => Policy;
    }
}
