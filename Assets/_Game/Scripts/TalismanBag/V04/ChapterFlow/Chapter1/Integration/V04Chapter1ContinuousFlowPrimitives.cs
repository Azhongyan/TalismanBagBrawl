using System;
using System.Collections.Generic;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public static class V04Chapter1ContinuousBattleIntegrationContract
    {
        public const string PackageId =
            "V0.4-BLineChapter1ContinuousBattleIntegration01";
        public const string SchemaId =
            "V04Chapter1ContinuousBattleIntegration.v1";
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        public const string TargetSceneName =
            "Scene_TalismanBag_V04_BattleSandboxPreview";
        public const string InjectedRootName =
            "V04_BLine_C1_ContinuousHandtest_Runtime";
        public const string EditorPreferenceKey =
            "TalismanBag.V04.BLineChapter1ContinuousHandtest.Enabled";
        public const string CompletionResolverSlotId =
            "bline.c1.boss_completion_resolver";
        public const string ReplacementMode =
            "REPLACE_COMPLETION_RESOLVER_ONLY";
        public const string Phase1DevVerticalSlice =
            "PHASE1_DEV_VERTICAL_SLICE";
        public const string NotContentFinal = "NOT_CONTENT_FINAL";
        public const string NotFormalBossCompletion =
            "NOT_FORMAL_BOSS_COMPLETION";
        public const string CadenceLabels =
            "DEV_ONLY_CADENCE / NOT_BALANCE_FINAL";
        public const string ResolverLabels =
            "DEV_ONLY_BATTLE_RESOLVER_FIXTURE / NOT_BALANCE_FINAL / NOT_CONTENT_FINAL";
        public const long ItemApplicationCadenceTicks = 1500L;
        public const int PlayerMaxHpFixture = 9999;
        public const int ShatteredHostOutgoingDamageFixture = 10;
        public const int PorcelainHoundOutgoingDamageFixture = 14;

        public const bool DevOnly = true;
        public const bool FeatureDefaultEnabled = false;
        public const bool FormalFlow = false;
        public const bool EntersFormalRunFlow = false;
        public const bool WritesSave = false;
        public const bool GrantsReward = false;
        public const bool GrantsDrop = false;
        public const bool WritesInventory = false;
        public const bool ModifiesSceneAsset = false;

        public static readonly IReadOnlyList<string> TemporaryClearLabels =
            Array.AsReadOnly(new[]
            {
                Phase1DevVerticalSlice,
                NotContentFinal,
                NotFormalBossCompletion
            });
    }

    public sealed class V04Chapter1ContinuousStageTraceRow
    {
        public int sequence;
        public string stageId = string.Empty;
        public V04ChapterFlowPhase flowPhase;
        public string contentId = string.Empty;
        public string runtimeProfileId = string.Empty;
        public string battleRequestId = string.Empty;
        public string resultId = string.Empty;
        public string outcome = string.Empty;
        public bool bossDefeated;
        public int completedStageCount;
        public int unlockedChapterCount;
        public string labels = string.Empty;
    }

    public sealed class V04Chapter1TargetTransitionTraceRow
    {
        public int sequence;
        public string scenario = string.Empty;
        public string waveId = string.Empty;
        public string requestedEnemyInstanceId = string.Empty;
        public string selectedBefore = string.Empty;
        public string selectedAfter = string.Empty;
        public string outcome = string.Empty;
    }

    public sealed class V04Chapter1NormalBattleFacts
    {
        public C1EnemyRuntimeSnapshot enemy;
        public IReadOnlyList<C1EnemyRuntimeSnapshot> enemies =
            Array.Empty<C1EnemyRuntimeSnapshot>();
        public string selectedTargetEnemyInstanceId = string.Empty;
        public int playerCurrentHp = V04Chapter1ContinuousBattleIntegrationContract.PlayerMaxHpFixture;
        public long battleTick;
        public int acceptedItemApplications;
        public int outgoingRequestsResolved;
        public int fieldAreaObservedCount;
        public int counterWindowObservedCount;
        public int fieldAreaExecutionCount;
        public int counterWindowExecutionCount;
        public int formalMechanicGrantCount;
        public string itemRequestCanonicalSignature = string.Empty;
    }

    public sealed class V04Chapter1ContinuousValidationResult
    {
        public readonly List<string> errors = new();
        public readonly List<V04Chapter1ContinuousStageTraceRow> trace = new();
        public readonly List<V04Chapter1TargetTransitionTraceRow>
            targetTransitions = new();
        public int normalBattles;
        public int bossGates;
        public int manualBossChallenges;
        public int phase1TemporaryClears;
        public int sessionUnlocks;
        public int saveWrites;
        public int rewardGrants;
        public int legacyRefs;

        public bool Passed => errors.Count == 0;

        public void Require(bool condition, string error)
        {
            if (!condition)
            {
                errors.Add(error ?? "UNKNOWN_VALIDATION_ERROR");
            }
        }
    }
}
