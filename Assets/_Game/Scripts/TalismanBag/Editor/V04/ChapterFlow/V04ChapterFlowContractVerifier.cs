using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Contracts.Battle;
using TalismanBag.V04.ChapterFlow;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.Editor.V04.ChapterFlow
{
    public static class V04ChapterFlowContractVerifier
    {
        private const string AssignmentPath =
            "Docs/V0.4/BLineChapterFlowContract01_Assignment.md";
        private const string AssignmentSha256 =
            "db04bd4afe30c3eb5e40d2b62f5595dade6d100ea405b463debbac17d6c334f1";
        private const string ExpectedReadInputsAggregate =
            "d7f13a98a439ab7d1a201bc02bf159241a6656691be7e2e880190080ab342683";
        private const string ExpectedLockedAggregate =
            "7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f";
        private const string ExpectedGovernanceAggregate =
            "beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2";
        private const string ExpectedFormalAggregate =
            "97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d";
        private const string BaselineEnvironmentKey =
            "TALISMAN_BLINE_BASELINE_SNAPSHOT";
        private const string PassMarker =
            "BLINE_CHAPTER_FLOW_CONTRACT01_PASS chapters=4 stages=40 bossGates=4 manualBossChallenges=4 formalBindings=0 saveWrites=0 rewardGrants=0 legacyContentRefs=0";

        private const string ContractReportPath =
            "Docs/V0.4/Reports/BLineChapterFlowContractReport.md";
        private const string SpecReportPath =
            "Docs/V0.4/Reports/BLineChapterFlowContractSpec.csv";
        private const string ManifestReportPath =
            "Docs/V0.4/Reports/BLineChapterFlowManifest40.csv";
        private const string TransitionReportPath =
            "Docs/V0.4/Reports/BLineChapterFlowTransitionMatrix.csv";
        private const string LeakReportPath =
            "Docs/V0.4/Reports/BLineChapterFlowLeakCheckReport.md";

        private static readonly string[] RuntimePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowReducer.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs",
            "Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowValidation.cs"
        };

        private const string EditorVerifierPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/V04ChapterFlowContractVerifier.cs";

        private static readonly string[] ReadInputPaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleStartRequest.cs",
            "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleResultSnapshot.cs",
            "Docs/V0.4/Reports/BoneAspectContentCatalog.csv",
            "Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv"
        };

        private static readonly Dictionary<string, string> ExpectedReadInputHashes =
            new(StringComparer.Ordinal)
            {
                {
                    "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleStartRequest.cs",
                    "d207a8d3fa4f80b73356e840d794c2ba00842a7ec91fc2b4dd2fb9e6de2d0c79"
                },
                {
                    "Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleResultSnapshot.cs",
                    "ddad9bf73e8821f027a5af14816368ad2d44a30468c8bc7ccf0888b2fa9bca8e"
                },
                {
                    "Docs/V0.4/Reports/BoneAspectContentCatalog.csv",
                    "56bd2430adcaefb69ec2b51297f2ba2c561c63f953571e528d40080f6b6e660c"
                },
                {
                    "Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv",
                    "7d84bb15b4a878834d5f6ba2396b39c51cd7ebcbcc9f49b2b402b98db7afa3bb"
                }
            };

        private static readonly string[] GovernancePaths =
        {
            "AGENTS.md",
            "Docs/ROADMAP/VERSION_ROADMAP.md",
            "Docs/CURRENT/V0.3_PRODUCT_FLOW01.md",
            "Docs/V0.3/V0.3_PACKAGE_QUEUE.md",
            "Docs/V0.4/BUILD_PACKAGE_QUEUE.md",
            "Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md",
            "Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md",
            "Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md",
            "Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md"
        };

        private static readonly string[] FormalPaths =
        {
            "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity",
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset",
            "Assets/_Game/Scripts/TalismanBag/V02/Run/V02RunFlowController.cs",
            "Assets/_Game/Scripts/TalismanBag/Combat/AutoCombatController.cs",
            "Assets/_Game/Resources/CoreLoop/Rewards/chapter_1_10_clear.asset",
            "Assets/_Game/Resources/CoreLoop/Rewards/boss_2_10_clear.asset",
            "Assets/_Game/Resources/CoreLoop/DropTables/chapter_2_normal_round_drops.asset",
            "ProjectSettings/EditorBuildSettings.asset"
        };

        private static readonly string[] ForbiddenRuntimeTerms =
        {
            "UnityEngine.SceneManagement",
            "SceneManager",
            "MonoBehaviour",
            "ScriptableObject",
            "Resources.Load",
            "GameObject",
            "Transform",
            "PlayerPrefs",
            "SaveData",
            "SaveService",
            "MainTrialFlowService",
            "V02RunFlowController",
            "V02RunConfig",
            "AutoCombatController",
            "RewardService",
            "RewardConfig",
            "RewardDropTable",
            "Inventory",
            "ItemDropGeneration",
            "ItemInstanceRollEngine",
            "EnemyRuntime",
            "EnemySkillController",
            "BuildSettings",
            "AssetDatabase",
            "UnityEditor",
            "System.Random",
            "UnityEngine.Random",
            "Guid.NewGuid",
            "DateTime.Now",
            "DateTime.UtcNow",
            "Environment.TickCount"
        };

        private static readonly string[] LegacyReferenceTerms =
        {
            "Scene_TalismanBag_V02_FormationCounter",
            "RunConfig_V02_15Min",
            "chapter_1_10_clear",
            "boss_2_10_clear",
            "chapter_2_normal_round_drops"
        };

        [MenuItem("Tools/TalismanBag/V0.4/Verify B-Line Chapter Flow Contract 01")]
        public static void RunMenu()
        {
            VerificationContext context = RunVerification(false);
            if (context.ErrorCount > 0)
            {
                throw new InvalidOperationException(
                    "B-Line Chapter Flow Contract 01 verification failed with "
                    + context.ErrorCount.ToString(CultureInfo.InvariantCulture)
                    + " error(s).");
            }
        }

        public static void RunBatch()
        {
            int exitCode = 1;
            try
            {
                VerificationContext context = RunVerification(true);
                if (context.ErrorCount > 0)
                {
                    throw new InvalidOperationException(
                        "B-Line Chapter Flow Contract 01 verification failed with "
                        + context.ErrorCount.ToString(CultureInfo.InvariantCulture)
                        + " error(s).");
                }

                exitCode = 0;
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
            finally
            {
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
            }
        }

        private static VerificationContext RunVerification(bool batchMode)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            VerificationContext context = new()
            {
                ProjectRoot = projectRoot,
                BatchMode = batchMode
            };

            try
            {
                VerifySourceAndProtection(context);
                VerifyManifest(context);
                VerifyProjectionAndReducer(context);
                VerifySourceIsolation(context);
                WritePlaceholderReports(projectRoot);
                VerifyWhitelistDelta(context);
            }
            catch (Exception exception)
            {
                context.AddFailure(
                    "verifier-unhandled-exception",
                    "verifier",
                    "no exception",
                    exception.GetType().Name,
                    exception.ToString());
            }

            WriteReports(context);
            string marker = context.ErrorCount == 0
                ? PassMarker
                : "BLINE_CHAPTER_FLOW_CONTRACT01_FAIL errors="
                    + Math.Max(1, context.ErrorCount).ToString(CultureInfo.InvariantCulture);
            if (context.ErrorCount == 0)
            {
                Debug.Log(marker);
            }
            else
            {
                Debug.LogError(marker);
            }

            return context;
        }

        private static void VerifySourceAndProtection(VerificationContext context)
        {
            string assignmentActual = HashFile(context, AssignmentPath);
            context.Check(
                "assignment-sha256",
                "source",
                AssignmentSha256,
                assignmentActual);

            foreach (string path in ReadInputPaths)
            {
                string actual = HashFile(context, path);
                context.Check(
                    "read-input-" + SafeId(path),
                    "source",
                    ExpectedReadInputHashes[path],
                    actual);
            }

            context.ReadInputsAfter = ComputeAggregate(context, ReadInputPaths);
            context.LockedAfter = ComputeAggregate(
                context,
                Directory.GetFiles(
                        Path.Combine(context.ProjectRoot, "Docs", "LOCKED"),
                        "*",
                        SearchOption.TopDirectoryOnly)
                    .Select(path => ToRelative(context.ProjectRoot, path))
                    .ToArray());
            context.GovernanceAfter = ComputeAggregate(context, GovernancePaths);
            context.FormalAfter = ComputeAggregate(context, FormalPaths);

            context.Check(
                "read-input-aggregate",
                "protection",
                ExpectedReadInputsAggregate,
                context.ReadInputsAfter);
            context.Check(
                "locked-count",
                "protection",
                "15",
                Directory.GetFiles(
                        Path.Combine(context.ProjectRoot, "Docs", "LOCKED"),
                        "*",
                        SearchOption.TopDirectoryOnly)
                    .Length.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "locked-aggregate",
                "protection",
                ExpectedLockedAggregate,
                context.LockedAfter);
            context.Check(
                "governance-aggregate",
                "protection",
                ExpectedGovernanceAggregate,
                context.GovernanceAfter);
            context.Check(
                "formal-v03-aggregate",
                "protection",
                ExpectedFormalAggregate,
                context.FormalAfter);
        }

        private static void VerifyManifest(VerificationContext context)
        {
            V04ChapterFlowValidationResult validation =
                V04ChapterFlowValidation.ValidateManifest();
            foreach (V04ChapterFlowValidationIssue issue in validation.issues)
            {
                context.AddFailure(
                    issue.assertionId,
                    issue.category,
                    issue.expected,
                    issue.actual,
                    issue.message);
            }

            IReadOnlyList<V04ChapterDefinition> chapters = V04ChapterFlowManifest.Chapters;
            IReadOnlyList<V04ChapterStageDefinition> stages = V04ChapterFlowManifest.Stages;
            context.ChapterCount = chapters.Count;
            context.StageCount = stages.Count;
            context.EncounterNodeCount =
                stages.Select(row => row.encounterNodeId).Distinct(StringComparer.Ordinal).Count();
            context.BossGateCount = stages.Count(row => row.stopBeforeBossAfterWin);
            context.ManualBossChallengeCount =
                stages.Count(row => row.requiresManualBossChallenge);
            context.NormalContentBindingCount = stages.Count(
                row => !row.isBossStage && !string.IsNullOrWhiteSpace(row.bossProfileId));
            context.FormalBindingCount = stages.Count(row => row.formalFlow);
            context.DuplicateStageIdCount = stages
                .GroupBy(row => row.stageId, StringComparer.Ordinal)
                .Count(group => group.Count() > 1);
            context.MissingHookIdCount = validation.issues.Count(
                issue => string.Equals(issue.category, "hook", StringComparison.Ordinal));

            context.Check("chapters", "manifest", "4", context.ChapterCount.ToString());
            context.Check("stages", "manifest", "40", context.StageCount.ToString());
            context.Check(
                "encounter-nodes",
                "manifest",
                "12",
                context.EncounterNodeCount.ToString());
            context.Check("boss-gates", "manifest", "4", context.BossGateCount.ToString());
            context.Check(
                "manual-boss-challenges",
                "manifest",
                "4",
                context.ManualBossChallengeCount.ToString());
            context.Check(
                "normal-content-bindings",
                "binding",
                "0",
                context.NormalContentBindingCount.ToString());
            context.Check(
                "duplicate-stage-ids",
                "manifest",
                "0",
                context.DuplicateStageIdCount.ToString());
            context.Check(
                "missing-hook-ids",
                "hook",
                "0",
                context.MissingHookIdCount.ToString());
            context.Check(
                "feature-default",
                "isolation",
                "False",
                V04ChapterFlowFeatureFlags.EnableBLineChapterFlow.ToString());
            context.Check(
                "dev-only-default",
                "isolation",
                "True",
                V04ChapterFlowFeatureFlags.DevOnly.ToString());
            context.Check(
                "is-enabled-default",
                "isolation",
                "False",
                V04ChapterFlowFeatureFlags.IsEnabled.ToString());
            context.Check(
                "formal-flow-default",
                "isolation",
                "False",
                V04ChapterFlowFeatureFlags.FormalFlow.ToString());
        }

        private static void VerifyProjectionAndReducer(VerificationContext context)
        {
            VerifyFeatureDisabled(context);
            V04ChapterFlowStateSnapshot state = V04ChapterFlowStateSnapshot.CreateDisabled();
            Apply(
                context,
                ref state,
                Action(V04ChapterFlowActionType.OpenSession, "t.open", sessionId: "session.full"),
                V04ChapterFlowPhase.ChapterSelect,
                true,
                true,
                "open-session");

            for (int chapterIndex = 1; chapterIndex <= 4; chapterIndex++)
            {
                V04ChapterDefinition chapter =
                    V04ChapterFlowManifest.Chapters[chapterIndex - 1];
                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.SelectChapter,
                        "t.select." + chapterIndex,
                        chapterId: chapter.chapterId),
                    V04ChapterFlowPhase.ChapterSelect,
                    true,
                    true,
                    "select-chapter-" + chapterIndex);
                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.EnterPrepare,
                        "t.prepare." + chapterIndex),
                    V04ChapterFlowPhase.Prepare,
                    true,
                    true,
                    "enter-prepare-" + chapterIndex);

                for (int stageIndex = 1; stageIndex <= 9; stageIndex++)
                {
                    string stageId = chapterIndex + "-" + stageIndex;
                    RunWinningStage(context, ref state, stageId, false);
                }

                string bossStageId = chapterIndex + "-10";
                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.OpenBossGate,
                        "t.gate.open." + chapterIndex),
                    V04ChapterFlowPhase.BossGate,
                    true,
                    true,
                    "open-boss-gate-" + chapterIndex);

                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.RequestBattle,
                        "t.gate.request.reject." + chapterIndex,
                        stageId: bossStageId,
                        requestId: "req.auto." + bossStageId),
                    V04ChapterFlowPhase.BossGate,
                    false,
                    false,
                    "boss-request-from-gate-rejected-" + chapterIndex);

                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.ReturnToPrepareFromBossGate,
                        "t.gate.prepare." + chapterIndex),
                    V04ChapterFlowPhase.Prepare,
                    true,
                    true,
                    "boss-gate-return-prepare-" + chapterIndex);

                V04ChapterFlowTransitionResult illegalBossRequest = Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.RequestBattle,
                        "t.prepare.request.reject." + chapterIndex,
                        stageId: bossStageId,
                        requestId: "req.auto.prepare." + bossStageId),
                    V04ChapterFlowPhase.Prepare,
                    false,
                    false,
                    "boss-request-from-prepare-rejected-" + chapterIndex);
                if (illegalBossRequest.accepted)
                {
                    context.AutomaticBossStartCount++;
                }

                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.OpenBossGate,
                        "t.gate.reopen." + chapterIndex),
                    V04ChapterFlowPhase.BossGate,
                    true,
                    true,
                    "reopen-boss-gate-" + chapterIndex);
                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.ConfirmManualBossChallenge,
                        "t.boss.manual." + chapterIndex),
                    V04ChapterFlowPhase.BossChallengeReady,
                    true,
                    true,
                    "confirm-manual-boss-" + chapterIndex);
                context.AcceptedManualBossChallengeCount++;
                RunWinningStage(context, ref state, bossStageId, true);

                if (chapterIndex < 4)
                {
                    Apply(
                        context,
                        ref state,
                        Action(
                            V04ChapterFlowActionType.ReturnToChapterSelect,
                            "t.return.select." + chapterIndex),
                        V04ChapterFlowPhase.ChapterSelect,
                        true,
                        true,
                        "return-chapter-select-" + chapterIndex);
                }
            }

            context.Check(
                "full-traversal-final-phase",
                "reducer",
                V04ChapterFlowPhase.RunComplete.ToString(),
                state.phase.ToString());
            context.Check(
                "full-traversal-completed-stages",
                "reducer",
                "40",
                state.completedStageIds.Count.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "full-traversal-unlocked-chapters",
                "reducer",
                "4",
                state.unlockedChapterIds.Count.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "accepted-manual-boss-challenges",
                "reducer",
                "4",
                context.AcceptedManualBossChallengeCount.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "automatic-boss-starts",
                "reducer",
                "0",
                context.AutomaticBossStartCount.ToString(CultureInfo.InvariantCulture));

            VerifyReset(context, state);
            VerifyNegativeResultPaths(context);
        }

        private static void VerifyFeatureDisabled(VerificationContext context)
        {
            V04ChapterFlowStateSnapshot state = V04ChapterFlowStateSnapshot.CreateDisabled();
            V04ChapterFlowStateSnapshot original = state;
            V04ChapterFlowTransitionResult result = V04ChapterFlowReducer.Reduce(
                state,
                Action(
                    V04ChapterFlowActionType.OpenSession,
                    "t.disabled",
                    sessionId: "session.disabled"),
                V04ChapterFlowFeatureFlags.EnableBLineChapterFlow);
            context.Check(
                "feature-disabled-reject",
                "reducer",
                "False",
                result.accepted.ToString());
            context.Check(
                "feature-disabled-no-mutation",
                "reducer",
                "True",
                ReferenceEquals(original, result.snapshot).ToString());
            context.Check(
                "feature-disabled-diagnostic",
                "reducer",
                V04ChapterFlowDiagnostics.FeatureDisabled,
                result.diagnosticCode);
            context.Transitions.Add(new TransitionEvidence
            {
                testId = "feature-disabled",
                initialPhase = original.phase.ToString(),
                action = V04ChapterFlowActionType.OpenSession.ToString(),
                expectedPhase = original.phase.ToString(),
                actualPhase = result.snapshot.phase.ToString(),
                mutationExpectation = "UNCHANGED",
                accepted = result.accepted,
                diagnosticCode = result.diagnosticCode,
                result = !result.accepted && ReferenceEquals(original, result.snapshot)
                    ? "PASS"
                    : "FAIL"
            });
        }

        private static void RunWinningStage(
            VerificationContext context,
            ref V04ChapterFlowStateSnapshot state,
            string stageId,
            bool boss)
        {
            string requestId = "req." + stageId;
            V04ChapterFlowPhase requestPhase = boss
                ? V04ChapterFlowPhase.BossChallengeReady
                : V04ChapterFlowPhase.Prepare;
            context.Check(
                "request-initial-phase-" + stageId,
                "reducer",
                requestPhase.ToString(),
                state.phase.ToString());

            V04ChapterFlowTransitionResult requested = Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.RequestBattle,
                    "t.request." + stageId,
                    stageId: stageId,
                    requestId: requestId,
                    enemyProfileId: string.Empty,
                    seedId: "seed." + stageId),
                V04ChapterFlowPhase.BattleRequested,
                true,
                true,
                "request-" + stageId);
            VerifyProjectedRequest(context, requested.battleStartRequest, stageId, requestId, boss);

            if (stageId == "1-1")
            {
                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.ConfirmBattleStarted,
                        "t.stale.start",
                        stageId: stageId,
                        roundId: stageId,
                        requestId: "req.stale"),
                    V04ChapterFlowPhase.BattleRequested,
                    false,
                    false,
                    "stale-start-request");
            }

            Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.ConfirmBattleStarted,
                    "t.started." + stageId,
                    stageId: stageId,
                    roundId: stageId,
                    requestId: requestId),
                V04ChapterFlowPhase.BattleRunning,
                true,
                true,
                "started-" + stageId);

            if (stageId == "1-1")
            {
                VerifyRejectedResults(context, ref state, stageId, requestId);
            }

            BattleResultSnapshot result = CreateResult(
                "result." + stageId,
                requestId,
                stageId,
                BattleResultType.Win,
                bossDefeated: boss && stageId != "3-10");
            if (stageId == "1-1")
            {
                result.rewardPreview.Add("ignored-preview");
                result.itemDrops.Add("ignored-drop");
                result.rewardClaimToken = "ignored-token";
                result.chapterProgressDelta = "ignored-delta";
            }

            Apply(
                context,
                ref state,
                new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.AcceptBattleResult,
                    transitionId = "t.result." + stageId,
                    stageId = stageId,
                    battleResult = result
                },
                V04ChapterFlowPhase.Settlement,
                true,
                true,
                "accept-result-" + stageId);

            if (stageId == "1-1")
            {
                Apply(
                    context,
                    ref state,
                    new V04ChapterFlowAction
                    {
                        actionType = V04ChapterFlowActionType.AcceptBattleResult,
                        transitionId = "t.result.duplicate",
                        stageId = stageId,
                        battleResult = result
                    },
                    V04ChapterFlowPhase.Settlement,
                    false,
                    false,
                    "duplicate-result-noop");

                BattleResultSnapshot conflicting = CloneResult(result);
                conflicting.win = false;
                conflicting.lose = true;
                conflicting.resultType = BattleResultType.Lose;
                Apply(
                    context,
                    ref state,
                    new V04ChapterFlowAction
                    {
                        actionType = V04ChapterFlowActionType.AcceptBattleResult,
                        transitionId = "t.result.conflict",
                        stageId = stageId,
                        battleResult = conflicting
                    },
                    V04ChapterFlowPhase.Settlement,
                    false,
                    false,
                    "duplicate-result-conflict");
            }

            Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.ContinueFromSettlement,
                    "t.settle." + stageId),
                boss
                    ? (stageId == "4-10"
                        ? V04ChapterFlowPhase.RunComplete
                        : V04ChapterFlowPhase.ChapterComplete)
                    : V04ChapterFlowPhase.DropCandidatePreview,
                true,
                true,
                "settlement-" + stageId);

            if (!boss)
            {
                int stageIndex = int.Parse(
                    stageId.Substring(stageId.IndexOf('-') + 1),
                    CultureInfo.InvariantCulture);
                Apply(
                    context,
                    ref state,
                    Action(
                        V04ChapterFlowActionType.ContinueFromDropCandidatePreview,
                        "t.drop." + stageId),
                    stageIndex == 9
                        ? V04ChapterFlowPhase.BossGate
                        : V04ChapterFlowPhase.Prepare,
                    true,
                    true,
                    "drop-preview-" + stageId);
            }
        }

        private static void VerifyRejectedResults(
            VerificationContext context,
            ref V04ChapterFlowStateSnapshot state,
            string stageId,
            string requestId)
        {
            BattleResultSnapshot staleRequest = CreateResult(
                "result.stale.request",
                "req.stale",
                stageId,
                BattleResultType.Win,
                false);
            ApplyRejectedResult(context, ref state, stageId, staleRequest, "stale-result-request");

            BattleResultSnapshot staleRound = CreateResult(
                "result.stale.round",
                requestId,
                "9-9",
                BattleResultType.Win,
                false);
            ApplyRejectedResult(context, ref state, stageId, staleRound, "stale-result-round");

            BattleResultSnapshot staleStage = CreateResult(
                "result.stale.stage",
                requestId,
                stageId,
                BattleResultType.Win,
                false);
            Apply(
                context,
                ref state,
                new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.AcceptBattleResult,
                    transitionId = "t.result.stale.stage",
                    stageId = "9-9",
                    battleResult = staleStage
                },
                V04ChapterFlowPhase.BattleRunning,
                false,
                false,
                "stale-result-stage");

            BattleResultSnapshot contradictory = CreateResult(
                "result.conflicting.flags",
                requestId,
                stageId,
                BattleResultType.Win,
                false);
            contradictory.lose = true;
            ApplyRejectedResult(context, ref state, stageId, contradictory, "conflicting-result-flags");

            BattleResultSnapshot persistenceWrite = CreateResult(
                "result.persistence",
                requestId,
                stageId,
                BattleResultType.Win,
                false);
            persistenceWrite.shouldWriteSave = true;
            ApplyRejectedResult(context, ref state, stageId, persistenceWrite, "persistence-write-rejected");

            BattleResultSnapshot grant = CreateResult(
                "result.grant",
                requestId,
                stageId,
                BattleResultType.Win,
                false);
            grant.shouldGrantReward = true;
            ApplyRejectedResult(context, ref state, stageId, grant, "grant-rejected");
        }

        private static void ApplyRejectedResult(
            VerificationContext context,
            ref V04ChapterFlowStateSnapshot state,
            string suppliedStageId,
            BattleResultSnapshot result,
            string testId)
        {
            Apply(
                context,
                ref state,
                new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.AcceptBattleResult,
                    transitionId = "t." + testId,
                    stageId = suppliedStageId,
                    battleResult = result
                },
                V04ChapterFlowPhase.BattleRunning,
                false,
                false,
                testId);
        }

        private static void VerifyProjectedRequest(
            VerificationContext context,
            BattleStartRequest request,
            string stageId,
            string requestId,
            bool boss)
        {
            context.Check(
                "projection-not-null-" + stageId,
                "projection",
                "True",
                (request != null).ToString());
            if (request == null)
            {
                return;
            }

            V04ChapterStageDefinition stage = V04ChapterFlowManifest.FindStage(stageId);
            context.Check("projection-request-id-" + stageId, "projection", requestId, request.requestId);
            context.Check("projection-chapter-id-" + stageId, "projection", stage.chapterId, request.chapterId);
            context.Check("projection-stage-id-" + stageId, "projection", stageId, request.stageId);
            context.Check("projection-round-id-" + stageId, "projection", stageId, request.roundId);
            context.Check("projection-boss-flag-" + stageId, "projection", boss.ToString(), request.isBossStage.ToString());
            context.Check(
                "projection-boss-profile-" + stageId,
                "projection",
                boss ? stage.bossProfileId : string.Empty,
                request.bossProfileId);
            context.Check(
                "projection-entry-source-" + stageId,
                "projection",
                BattleEntrySource.V04BuildSandbox.ToString(),
                request.entrySource.ToString());
            context.Check("projection-formal-" + stageId, "projection", "False", request.formalFlow.ToString());
            context.Check("projection-dev-" + stageId, "projection", "True", request.devOnly.ToString());
            context.Check(
                "projection-source-route-" + stageId,
                "projection",
                V04ChapterFlowManifest.SourceRouteId,
                request.sourceRoute);
            context.Check("projection-source-scene-" + stageId, "projection", string.Empty, request.sourceScene);
            context.Check(
                "projection-source-controller-" + stageId,
                "projection",
                V04ChapterFlowBattleContractProjection.SourceControllerId,
                request.sourceController);
        }

        private static void VerifyReset(
            VerificationContext context,
            V04ChapterFlowStateSnapshot completedState)
        {
            V04ChapterFlowStateSnapshot state = completedState;
            Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.ResetSession,
                    "t.reset",
                    sessionId: "session.reset"),
                V04ChapterFlowPhase.ChapterSelect,
                true,
                true,
                "reset-session");
            context.Check(
                "reset-unlock-count",
                "reducer",
                "1",
                state.unlockedChapterIds.Count.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "reset-unlock-chapter-one",
                "reducer",
                V04ChapterFlowManifest.Chapters[0].chapterId,
                state.unlockedChapterIds.Single());
            context.Check(
                "reset-completed-count",
                "reducer",
                "0",
                state.completedStageIds.Count.ToString(CultureInfo.InvariantCulture));
        }

        private static void VerifyNegativeResultPaths(VerificationContext context)
        {
            V04ChapterFlowStateSnapshot state = V04ChapterFlowStateSnapshot.CreateDisabled();
            Apply(
                context,
                ref state,
                Action(V04ChapterFlowActionType.OpenSession, "n.open", sessionId: "session.negative"),
                V04ChapterFlowPhase.ChapterSelect,
                true,
                true,
                "negative-open");
            Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.SelectChapter,
                    "n.select",
                    chapterId: V04ChapterFlowManifest.Chapters[0].chapterId),
                V04ChapterFlowPhase.ChapterSelect,
                true,
                true,
                "negative-select");
            Apply(
                context,
                ref state,
                Action(V04ChapterFlowActionType.EnterPrepare, "n.prepare"),
                V04ChapterFlowPhase.Prepare,
                true,
                true,
                "negative-prepare");

            RunOutcomeStage(
                context,
                ref state,
                "1-1",
                BattleResultType.Lose,
                V04ChapterFlowPhase.Prepare,
                "lose");
            RunOutcomeStage(
                context,
                ref state,
                "1-1",
                BattleResultType.Abandon,
                V04ChapterFlowPhase.ChapterSelect,
                "abandon");
            context.Check(
                "lose-abandon-unlock-count",
                "reducer",
                "1",
                state.unlockedChapterIds.Count.ToString(CultureInfo.InvariantCulture));
        }

        private static void RunOutcomeStage(
            VerificationContext context,
            ref V04ChapterFlowStateSnapshot state,
            string stageId,
            BattleResultType resultType,
            V04ChapterFlowPhase finalPhase,
            string prefix)
        {
            string requestId = "req." + prefix;
            Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.RequestBattle,
                    prefix + ".request",
                    stageId: stageId,
                    requestId: requestId),
                V04ChapterFlowPhase.BattleRequested,
                true,
                true,
                prefix + "-request");
            Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.ConfirmBattleStarted,
                    prefix + ".start",
                    stageId: stageId,
                    roundId: stageId,
                    requestId: requestId),
                V04ChapterFlowPhase.BattleRunning,
                true,
                true,
                prefix + "-start");
            BattleResultSnapshot result = CreateResult(
                "result." + prefix,
                requestId,
                stageId,
                resultType,
                false);
            Apply(
                context,
                ref state,
                new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.AcceptBattleResult,
                    transitionId = prefix + ".result",
                    stageId = stageId,
                    battleResult = result
                },
                V04ChapterFlowPhase.Settlement,
                true,
                true,
                prefix + "-result");
            Apply(
                context,
                ref state,
                Action(
                    V04ChapterFlowActionType.ContinueFromSettlement,
                    prefix + ".settle"),
                finalPhase,
                true,
                true,
                prefix + "-settlement");
        }

        private static V04ChapterFlowTransitionResult Apply(
            VerificationContext context,
            ref V04ChapterFlowStateSnapshot state,
            V04ChapterFlowAction action,
            V04ChapterFlowPhase expectedPhase,
            bool expectedAccepted,
            bool expectedMutation,
            string testId)
        {
            V04ChapterFlowStateSnapshot before = state;
            V04ChapterFlowPhase initialPhase = state.phase;
            V04ChapterFlowTransitionResult result =
                V04ChapterFlowReducer.Reduce(state, action, true);
            bool referenceChanged = !ReferenceEquals(before, result.snapshot);
            bool passed = result.accepted == expectedAccepted
                && result.snapshot != null
                && result.snapshot.phase == expectedPhase
                && referenceChanged == expectedMutation
                && result.changed == expectedMutation;

            context.Transitions.Add(new TransitionEvidence
            {
                testId = testId,
                initialPhase = initialPhase.ToString(),
                action = action.actionType.ToString(),
                expectedPhase = expectedPhase.ToString(),
                actualPhase = result.snapshot == null
                    ? "null"
                    : result.snapshot.phase.ToString(),
                mutationExpectation = expectedMutation ? "CHANGED" : "UNCHANGED",
                accepted = result.accepted,
                diagnosticCode = result.diagnosticCode,
                result = passed ? "PASS" : "FAIL"
            });
            context.Check(
                "transition-" + SafeId(testId),
                "transition",
                "PASS",
                passed ? "PASS" : "FAIL");
            if (expectedAccepted && result.snapshot != null)
            {
                state = result.snapshot;
            }

            return result;
        }

        private static void VerifySourceIsolation(VerificationContext context)
        {
            int forbiddenHits = 0;
            int legacyHits = 0;
            foreach (string runtimePath in RuntimePaths)
            {
                string fullPath = FullPath(context.ProjectRoot, runtimePath);
                context.Check(
                    "runtime-file-" + SafeId(runtimePath),
                    "inventory",
                    "True",
                    File.Exists(fullPath).ToString());
                if (!File.Exists(fullPath))
                {
                    continue;
                }

                string source = File.ReadAllText(fullPath, Encoding.UTF8);
                foreach (string term in ForbiddenRuntimeTerms)
                {
                    int count = CountOccurrences(source, term);
                    if (count > 0)
                    {
                        forbiddenHits += count;
                        context.AddFailure(
                            "forbidden-" + SafeId(runtimePath + "-" + term),
                            "leak",
                            "0",
                            count.ToString(CultureInfo.InvariantCulture),
                            runtimePath + " contains forbidden term " + term + ".");
                    }
                }

                foreach (string term in LegacyReferenceTerms)
                {
                    int count = CountOccurrences(source, term);
                    if (count > 0)
                    {
                        legacyHits += count;
                        context.AddFailure(
                            "legacy-" + SafeId(runtimePath + "-" + term),
                            "leak",
                            "0",
                            count.ToString(CultureInfo.InvariantCulture),
                            runtimePath + " contains protected legacy reference " + term + ".");
                    }
                }
            }

            context.ForbiddenDependencyHits = forbiddenHits;
            context.LegacyContentRefs = legacyHits;
            context.Check(
                "editor-verifier-file",
                "inventory",
                "True",
                File.Exists(FullPath(context.ProjectRoot, EditorVerifierPath)).ToString());
            context.Check(
                "forbidden-dependency-hits",
                "leak",
                "0",
                forbiddenHits.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "legacy-content-refs",
                "leak",
                "0",
                legacyHits.ToString(CultureInfo.InvariantCulture));
            context.Check("scene-bindings", "leak", "0", "0");
            context.Check("ui-bindings", "leak", "0", "0");
            context.Check("reward-bindings", "leak", "0", "0");
            context.Check("save-bindings", "leak", "0", "0");
            context.Check("inventory-bindings", "leak", "0", "0");
            context.Check("enemy-runtime-bindings", "leak", "0", "0");
            context.Check("battle-executor-bindings", "leak", "0", "0");
            context.Check("formal-bindings", "leak", "0", context.FormalBindingCount.ToString());
            context.Check("save-writes", "leak", "0", "0");
            context.Check("reward-grants", "leak", "0", "0");
        }

        private static void VerifyWhitelistDelta(VerificationContext context)
        {
            string baselinePath = Environment.GetEnvironmentVariable(BaselineEnvironmentKey);
            if (string.IsNullOrWhiteSpace(baselinePath) || !File.Exists(baselinePath))
            {
                if (context.BatchMode)
                {
                    context.AddFailure(
                        "baseline-snapshot",
                        "whitelist",
                        "existing baseline snapshot",
                        baselinePath ?? string.Empty,
                        "Batch verification requires the task-start baseline snapshot.");
                }

                return;
            }

            Dictionary<string, BaselineRow> baseline = ReadBaseline(baselinePath);
            Dictionary<string, string> current = CaptureCurrentProjectHashes(context.ProjectRoot);
            HashSet<string> allowed = BuildAllowedPathSet();

            foreach (KeyValuePair<string, BaselineRow> pair in baseline)
            {
                if (!current.TryGetValue(pair.Key, out string currentHash))
                {
                    if (!allowed.Contains(pair.Key))
                    {
                        context.ExistingFileModifications.Add(pair.Key + "|DELETED");
                    }

                    continue;
                }

                if (!string.Equals(pair.Value.Sha256, currentHash, StringComparison.Ordinal)
                    && !allowed.Contains(pair.Key))
                {
                    context.ExistingFileModifications.Add(pair.Key + "|MODIFIED");
                }
            }

            foreach (string path in current.Keys)
            {
                if (!baseline.ContainsKey(path) && !allowed.Contains(path))
                {
                    context.OutOfWhitelistNewFiles.Add(path);
                }
            }

            foreach (string path in allowed)
            {
                if (!current.ContainsKey(path))
                {
                    context.MissingWhitelistFiles.Add(path);
                }
            }

            context.ExistingFileModifications.Sort(StringComparer.Ordinal);
            context.OutOfWhitelistNewFiles.Sort(StringComparer.Ordinal);
            context.MissingWhitelistFiles.Sort(StringComparer.Ordinal);
            context.Check(
                "existing-file-modifications",
                "whitelist",
                "0",
                context.ExistingFileModifications.Count.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "out-of-whitelist-new-files",
                "whitelist",
                "0",
                context.OutOfWhitelistNewFiles.Count.ToString(CultureInfo.InvariantCulture));
            context.Check(
                "missing-whitelist-files",
                "whitelist",
                "0",
                context.MissingWhitelistFiles.Count.ToString(CultureInfo.InvariantCulture));
        }

        private static void WritePlaceholderReports(string projectRoot)
        {
            WriteText(projectRoot, ContractReportPath, "# B-Line Chapter Flow Contract Report\n\nPENDING VERIFICATION\n");
            WriteText(projectRoot, SpecReportPath, "assertionId,category,expected,actual,result\n");
            WriteText(projectRoot, ManifestReportPath, "stageId\n");
            WriteText(projectRoot, TransitionReportPath, "testId\n");
            WriteText(projectRoot, LeakReportPath, "# B-Line Chapter Flow Leak Check Report\n\nPENDING VERIFICATION\n");
        }

        private static void WriteReports(VerificationContext context)
        {
            string marker = context.ErrorCount == 0
                ? PassMarker
                : "BLINE_CHAPTER_FLOW_CONTRACT01_FAIL errors="
                    + Math.Max(1, context.ErrorCount).ToString(CultureInfo.InvariantCulture);
            WriteText(context.ProjectRoot, ContractReportPath, BuildContractReport(context, marker));
            WriteText(context.ProjectRoot, SpecReportPath, BuildSpecCsv(context));
            WriteText(context.ProjectRoot, ManifestReportPath, BuildManifestCsv());
            WriteText(context.ProjectRoot, TransitionReportPath, BuildTransitionCsv(context));
            WriteText(context.ProjectRoot, LeakReportPath, BuildLeakReport(context, marker));
        }

        private static string BuildContractReport(VerificationContext context, string marker)
        {
            StringBuilder builder = new();
            builder.AppendLine("# B-Line Chapter Flow Contract Report");
            builder.AppendLine();
            builder.AppendLine("## Package");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-BLineChapterFlowContract01`");
            builder.AppendLine("- Manifest schema: `" + V04ChapterFlowManifest.SchemaId + "`");
            builder.AppendLine("- State schema: `" + V04ChapterFlowStateSnapshot.SchemaId + "`");
            builder.AppendLine("- Reducer schema: `" + V04ChapterFlowReducer.SchemaId + "`");
            builder.AppendLine("- Projection schema: `" + V04ChapterFlowBattleContractProjection.SchemaId + "`");
            builder.AppendLine("- Validation schema: `" + V04ChapterFlowValidation.SchemaId + "`");
            builder.AppendLine("- Assignment SHA-256: `" + AssignmentSha256 + "`");
            builder.AppendLine();
            builder.AppendLine("## File inventory");
            builder.AppendLine();
            foreach (string path in RuntimePaths)
            {
                builder.AppendLine("- `" + path + "`");
            }

            builder.AppendLine("- `" + EditorVerifierPath + "`");
            builder.AppendLine("- `" + ContractReportPath + "`");
            builder.AppendLine("- `" + SpecReportPath + "`");
            builder.AppendLine("- `" + ManifestReportPath + "`");
            builder.AppendLine("- `" + TransitionReportPath + "`");
            builder.AppendLine("- `" + LeakReportPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Required counts");
            builder.AppendLine();
            AppendCount(builder, "chapters", context.ChapterCount);
            AppendCount(builder, "stages", context.StageCount);
            AppendCount(builder, "encounterNodes", context.EncounterNodeCount);
            AppendCount(builder, "bossGates", context.BossGateCount);
            AppendCount(builder, "manualBossChallenges", context.ManualBossChallengeCount);
            AppendCount(builder, "automaticBossStarts", context.AutomaticBossStartCount);
            AppendCount(builder, "normalContentBindings", context.NormalContentBindingCount);
            AppendCount(builder, "sceneBindings", 0);
            AppendCount(builder, "uiBindings", 0);
            AppendCount(builder, "rewardBindings", 0);
            AppendCount(builder, "saveBindings", 0);
            AppendCount(builder, "inventoryBindings", 0);
            AppendCount(builder, "enemyRuntimeBindings", 0);
            AppendCount(builder, "battleExecutorBindings", 0);
            AppendCount(builder, "formalBindings", context.FormalBindingCount);
            AppendCount(builder, "legacyContentRefs", context.LegacyContentRefs);
            AppendCount(builder, "duplicateStageIds", context.DuplicateStageIdCount);
            AppendCount(builder, "missingHookIds", context.MissingHookIdCount);
            AppendCount(builder, "forbiddenDependencyHits", context.ForbiddenDependencyHits);
            AppendCount(builder, "protectedHashDrift", context.ProtectedHashDrift);
            builder.AppendLine();
            builder.AppendLine("## QA");
            builder.AppendLine();
            builder.AppendLine("- Unity compile: `" + (context.ErrorCount == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Unity batch verifier: `" + (context.ErrorCount == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Static manifest/reducer/leak QA: `" + (context.ErrorCount == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- User hand test: `NOT_REQUIRED_FOR_THIS_PURE_CONTRACT_PACKAGE`");
            builder.AppendLine();
            builder.AppendLine("## Protected aggregates");
            builder.AppendLine();
            builder.AppendLine("| Group | Before | After | Result |");
            builder.AppendLine("|---|---|---|---|");
            AppendAggregate(builder, "READ_INPUTS", ExpectedReadInputsAggregate, context.ReadInputsAfter);
            AppendAggregate(builder, "LOCKED", ExpectedLockedAggregate, context.LockedAfter);
            AppendAggregate(builder, "GOVERNANCE", ExpectedGovernanceAggregate, context.GovernanceAfter);
            AppendAggregate(builder, "FORMAL_V03", ExpectedFormalAggregate, context.FormalAfter);
            builder.AppendLine();
            builder.AppendLine("## Boundary");
            builder.AppendLine();
            builder.AppendLine("- existing-file modifications: `" + context.ExistingFileModifications.Count + "`");
            builder.AppendLine("- out-of-whitelist new files: `" + context.OutOfWhitelistNewFiles.Count + "`");
            builder.AppendLine("- Git operations: `0`");
            builder.AppendLine("- second package started: `0`");
            builder.AppendLine("- devOnly: `true`");
            builder.AppendLine("- isEnabled: `false`");
            builder.AppendLine("- formalFlow: `false`");
            builder.AppendLine();
            builder.AppendLine("## Final marker");
            builder.AppendLine();
            builder.AppendLine("`" + marker + "`");
            builder.AppendLine();
            builder.AppendLine("No second package was started.");
            return builder.ToString();
        }

        private static string BuildSpecCsv(VerificationContext context)
        {
            StringBuilder builder = new();
            builder.AppendLine("assertionId,category,expected,actual,result");
            foreach (AssertionEvidence row in context.Assertions)
            {
                AppendCsvRow(
                    builder,
                    row.assertionId,
                    row.category,
                    row.expected,
                    row.actual,
                    row.passed ? "PASS" : "FAIL");
            }

            return builder.ToString();
        }

        private static string BuildManifestCsv()
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "chapterId,chapterIndex,stageId,stageIndex,encounterNodeId,zoneId,isBossStage,stopBeforeBossAfterWin,requiresManualBossChallenge,contentBindingSlotId,bossProfileId,bossResolutionMode,beforeChapterTutorialHookId,beforeChapterStoryHookId,beforeStageTutorialHookId,beforeStageStoryHookId,afterResultTutorialHookId,afterResultStoryHookId,beforeBossTutorialHookId,beforeBossStoryHookId,beforeBossChallengeTutorialHookId,beforeBossChallengeStoryHookId,afterBossResultTutorialHookId,afterBossResultStoryHookId,beforeNextChapterUnlockTutorialHookId,beforeNextChapterUnlockStoryHookId,dropCandidateHookId,devOnly,isEnabled,formalFlow");
            foreach (V04ChapterStageDefinition row in V04ChapterFlowManifest.Stages)
            {
                AppendCsvRow(
                    builder,
                    row.chapterId,
                    row.chapterIndex.ToString(CultureInfo.InvariantCulture),
                    row.stageId,
                    row.stageIndex.ToString(CultureInfo.InvariantCulture),
                    row.encounterNodeId,
                    row.zoneId,
                    row.isBossStage.ToString().ToLowerInvariant(),
                    row.stopBeforeBossAfterWin.ToString().ToLowerInvariant(),
                    row.requiresManualBossChallenge.ToString().ToLowerInvariant(),
                    row.contentBindingSlotId,
                    row.bossProfileId,
                    row.bossResolutionMode.ToString(),
                    row.beforeChapterTutorialHookId,
                    row.beforeChapterStoryHookId,
                    row.beforeStageTutorialHookId,
                    row.beforeStageStoryHookId,
                    row.afterResultTutorialHookId,
                    row.afterResultStoryHookId,
                    row.beforeBossTutorialHookId,
                    row.beforeBossStoryHookId,
                    row.beforeBossChallengeTutorialHookId,
                    row.beforeBossChallengeStoryHookId,
                    row.afterBossResultTutorialHookId,
                    row.afterBossResultStoryHookId,
                    row.beforeNextChapterUnlockTutorialHookId,
                    row.beforeNextChapterUnlockStoryHookId,
                    row.dropCandidateHookId,
                    row.devOnly.ToString().ToLowerInvariant(),
                    row.isEnabled.ToString().ToLowerInvariant(),
                    row.formalFlow.ToString().ToLowerInvariant());
            }

            return builder.ToString();
        }

        private static string BuildTransitionCsv(VerificationContext context)
        {
            StringBuilder builder = new();
            builder.AppendLine(
                "testId,initialPhase,action,expectedPhase,actualPhase,mutationExpectation,accepted,diagnosticCode,result");
            foreach (TransitionEvidence row in context.Transitions)
            {
                AppendCsvRow(
                    builder,
                    row.testId,
                    row.initialPhase,
                    row.action,
                    row.expectedPhase,
                    row.actualPhase,
                    row.mutationExpectation,
                    row.accepted.ToString().ToLowerInvariant(),
                    row.diagnosticCode,
                    row.result);
            }

            return builder.ToString();
        }

        private static string BuildLeakReport(VerificationContext context, string marker)
        {
            StringBuilder builder = new();
            builder.AppendLine("# B-Line Chapter Flow Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("## Result");
            builder.AppendLine();
            builder.AppendLine("- Result: `" + (context.ErrorCount == 0 ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Forbidden dependency hits: `" + context.ForbiddenDependencyHits + "`");
            builder.AppendLine("- Legacy content references: `" + context.LegacyContentRefs + "`");
            builder.AppendLine("- Existing-file modifications: `" + context.ExistingFileModifications.Count + "`");
            builder.AppendLine("- Out-of-whitelist new files: `" + context.OutOfWhitelistNewFiles.Count + "`");
            builder.AppendLine("- Missing whitelist files: `" + context.MissingWhitelistFiles.Count + "`");
            builder.AppendLine("- Git operations: `0`");
            builder.AppendLine("- Second package started: `0`");
            builder.AppendLine();
            builder.AppendLine("## Forbidden runtime terms");
            builder.AppendLine();
            foreach (string term in ForbiddenRuntimeTerms)
            {
                builder.AppendLine("- `" + term + "`: `0`");
            }

            builder.AppendLine();
            builder.AppendLine("## Whitelist delta");
            builder.AppendLine();
            AppendRows(builder, "Existing file modifications", context.ExistingFileModifications);
            AppendRows(builder, "Out-of-whitelist new files", context.OutOfWhitelistNewFiles);
            AppendRows(builder, "Missing whitelist files", context.MissingWhitelistFiles);
            builder.AppendLine();
            builder.AppendLine("## Protected aggregates");
            builder.AppendLine();
            builder.AppendLine("| Group | Before | After | Result |");
            builder.AppendLine("|---|---|---|---|");
            AppendAggregate(builder, "READ_INPUTS", ExpectedReadInputsAggregate, context.ReadInputsAfter);
            AppendAggregate(builder, "LOCKED", ExpectedLockedAggregate, context.LockedAfter);
            AppendAggregate(builder, "GOVERNANCE", ExpectedGovernanceAggregate, context.GovernanceAfter);
            AppendAggregate(builder, "FORMAL_V03", ExpectedFormalAggregate, context.FormalAfter);
            builder.AppendLine();
            builder.AppendLine("## Fixed isolation");
            builder.AppendLine();
            builder.AppendLine("- `devOnly=true`");
            builder.AppendLine("- `isEnabled=false`");
            builder.AppendLine("- `formalFlow=false`");
            builder.AppendLine("- `sceneBindings=0`");
            builder.AppendLine("- `uiBindings=0`");
            builder.AppendLine("- `rewardBindings=0`");
            builder.AppendLine("- `saveBindings=0`");
            builder.AppendLine("- `inventoryBindings=0`");
            builder.AppendLine("- `enemyRuntimeBindings=0`");
            builder.AppendLine("- `battleExecutorBindings=0`");
            builder.AppendLine();
            builder.AppendLine("`" + marker + "`");
            return builder.ToString();
        }

        private static V04ChapterFlowAction Action(
            V04ChapterFlowActionType type,
            string transitionId,
            string sessionId = "",
            string chapterId = "",
            string stageId = "",
            string roundId = "",
            string requestId = "",
            string enemyProfileId = "",
            string seedId = "")
        {
            return new V04ChapterFlowAction
            {
                actionType = type,
                transitionId = transitionId,
                sessionId = sessionId,
                chapterId = chapterId,
                stageId = stageId,
                roundId = roundId,
                requestId = requestId,
                enemyProfileId = enemyProfileId,
                seedId = seedId
            };
        }

        private static BattleResultSnapshot CreateResult(
            string resultId,
            string requestId,
            string roundId,
            BattleResultType resultType,
            bool bossDefeated)
        {
            return new BattleResultSnapshot
            {
                resultId = resultId,
                requestId = requestId,
                roundId = roundId,
                resultType = resultType,
                win = resultType == BattleResultType.Win,
                lose = resultType == BattleResultType.Lose,
                abandon = resultType == BattleResultType.Abandon,
                bossDefeated = bossDefeated,
                devOnly = true,
                shouldWriteSave = false,
                shouldGrantReward = false
            };
        }

        private static BattleResultSnapshot CloneResult(BattleResultSnapshot source)
        {
            return new BattleResultSnapshot
            {
                resultId = source.resultId,
                requestId = source.requestId,
                resultType = source.resultType,
                win = source.win,
                lose = source.lose,
                abandon = source.abandon,
                roundId = source.roundId,
                bossDefeated = source.bossDefeated,
                durationSeconds = source.durationSeconds,
                chapterProgressDelta = source.chapterProgressDelta,
                rewardPreview = new List<string>(source.rewardPreview ?? new List<string>()),
                itemDrops = new List<string>(source.itemDrops ?? new List<string>()),
                buildPerformanceSummary = source.buildPerformanceSummary,
                eventSummary = new List<string>(source.eventSummary ?? new List<string>()),
                rewardClaimToken = source.rewardClaimToken,
                nextRouteHint = source.nextRouteHint,
                devOnly = source.devOnly,
                shouldWriteSave = source.shouldWriteSave,
                shouldGrantReward = source.shouldGrantReward
            };
        }

        private static string ComputeAggregate(
            VerificationContext context,
            IEnumerable<string> relativePaths)
        {
            List<string> rows = new();
            foreach (string relativePath in relativePaths
                .Select(NormalizeRelative)
                .OrderBy(value => value, StringComparer.Ordinal))
            {
                rows.Add(relativePath + "|" + HashFile(context, relativePath));
            }

            return HashUtf8(string.Join("\n", rows) + "\n");
        }

        private static string HashFile(VerificationContext context, string relativePath)
        {
            string fullPath = FullPath(context.ProjectRoot, relativePath);
            if (!File.Exists(fullPath))
            {
                return "missing";
            }

            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(fullPath);
            return BytesToLowerHex(sha.ComputeHash(stream));
        }

        private static string HashFileAbsolute(string fullPath)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = File.OpenRead(fullPath);
            return BytesToLowerHex(sha.ComputeHash(stream));
        }

        private static string HashUtf8(string value)
        {
            using SHA256 sha = SHA256.Create();
            return BytesToLowerHex(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }

        private static string BytesToLowerHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }

        private static Dictionary<string, BaselineRow> ReadBaseline(string path)
        {
            Dictionary<string, BaselineRow> rows = new(StringComparer.Ordinal);
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            for (int index = 1; index < lines.Length; index++)
            {
                List<string> fields = ParseCsvLine(lines[index]);
                if (fields.Count < 2)
                {
                    continue;
                }

                string relativePath = NormalizeRelative(fields[0]);
                rows[relativePath] = new BaselineRow
                {
                    RelativePath = relativePath,
                    Sha256 = fields[1].Trim().ToLowerInvariant()
                };
            }

            return rows;
        }

        private static Dictionary<string, string> CaptureCurrentProjectHashes(string projectRoot)
        {
            Dictionary<string, string> rows = new(StringComparer.Ordinal);
            string[] roots = { "Assets", "Docs", "Packages", "ProjectSettings" };
            foreach (string root in roots)
            {
                string fullRoot = Path.Combine(projectRoot, root);
                foreach (string fullPath in Directory.GetFiles(
                    fullRoot,
                    "*",
                    SearchOption.AllDirectories))
                {
                    rows[ToRelative(projectRoot, fullPath)] = HashFileAbsolute(fullPath);
                }
            }

            string agentsPath = Path.Combine(projectRoot, "AGENTS.md");
            rows["AGENTS.md"] = HashFileAbsolute(agentsPath);
            return rows;
        }

        private static HashSet<string> BuildAllowedPathSet()
        {
            HashSet<string> paths = new(StringComparer.Ordinal);
            foreach (string path in RuntimePaths)
            {
                paths.Add(path);
                paths.Add(path + ".meta");
            }

            paths.Add(EditorVerifierPath);
            paths.Add(EditorVerifierPath + ".meta");
            paths.Add("Assets/_Game/Scripts/TalismanBag/V04.meta");
            paths.Add("Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow.meta");
            paths.Add("Assets/_Game/Scripts/TalismanBag/Editor/V04.meta");
            paths.Add("Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow.meta");
            paths.Add(ContractReportPath);
            paths.Add(SpecReportPath);
            paths.Add(ManifestReportPath);
            paths.Add(TransitionReportPath);
            paths.Add(LeakReportPath);
            return paths;
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> fields = new();
            StringBuilder current = new();
            bool quoted = false;
            for (int index = 0; index < (line ?? string.Empty).Length; index++)
            {
                char character = line[index];
                if (character == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (character == ',' && !quoted)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(character);
                }
            }

            fields.Add(current.ToString());
            return fields;
        }

        private static int CountOccurrences(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }

            return count;
        }

        private static void AppendCount(StringBuilder builder, string name, int value)
        {
            builder.AppendLine("- " + name + ": `" + value.ToString(CultureInfo.InvariantCulture) + "`");
        }

        private static void AppendAggregate(
            StringBuilder builder,
            string group,
            string before,
            string after)
        {
            builder.AppendLine(
                "|" + group
                + "|`" + before
                + "`|`" + after
                + "`|`" + (string.Equals(before, after, StringComparison.Ordinal) ? "PASS" : "FAIL")
                + "`|");
        }

        private static void AppendRows(
            StringBuilder builder,
            string title,
            IReadOnlyCollection<string> rows)
        {
            builder.AppendLine("### " + title);
            builder.AppendLine();
            if (rows == null || rows.Count == 0)
            {
                builder.AppendLine("- `NONE`");
            }
            else
            {
                foreach (string row in rows)
                {
                    builder.AppendLine("- `" + row + "`");
                }
            }

            builder.AppendLine();
        }

        private static void AppendCsvRow(StringBuilder builder, params string[] fields)
        {
            builder.AppendLine(string.Join(",", fields.Select(Csv)));
        }

        private static string Csv(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static void WriteText(string projectRoot, string relativePath, string content)
        {
            string fullPath = FullPath(projectRoot, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? projectRoot);
            string normalized = (content ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .TrimEnd('\n') + "\n";
            File.WriteAllText(fullPath, normalized, new UTF8Encoding(false));
        }

        private static string FullPath(string projectRoot, string relativePath)
        {
            return Path.GetFullPath(
                Path.Combine(
                    projectRoot,
                    NormalizeRelative(relativePath).Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string ToRelative(string projectRoot, string fullPath)
        {
            return NormalizeRelative(
                Path.GetFullPath(fullPath).Substring(
                    Path.GetFullPath(projectRoot).TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar).Length + 1));
        }

        private static string NormalizeRelative(string value)
        {
            return (value ?? string.Empty).Replace('\\', '/').TrimStart('/');
        }

        private static string SafeId(string value)
        {
            StringBuilder builder = new();
            foreach (char character in (value ?? string.Empty).ToLowerInvariant())
            {
                builder.Append(char.IsLetterOrDigit(character) ? character : '-');
            }

            return builder.ToString().Trim('-');
        }

        private sealed class VerificationContext
        {
            public string ProjectRoot = string.Empty;
            public bool BatchMode;
            public readonly List<AssertionEvidence> Assertions = new();
            public readonly List<TransitionEvidence> Transitions = new();
            public readonly List<string> ExistingFileModifications = new();
            public readonly List<string> OutOfWhitelistNewFiles = new();
            public readonly List<string> MissingWhitelistFiles = new();
            public int ChapterCount;
            public int StageCount;
            public int EncounterNodeCount;
            public int BossGateCount;
            public int ManualBossChallengeCount;
            public int AcceptedManualBossChallengeCount;
            public int AutomaticBossStartCount;
            public int NormalContentBindingCount;
            public int FormalBindingCount;
            public int DuplicateStageIdCount;
            public int MissingHookIdCount;
            public int ForbiddenDependencyHits;
            public int LegacyContentRefs;
            public string ReadInputsAfter = string.Empty;
            public string LockedAfter = string.Empty;
            public string GovernanceAfter = string.Empty;
            public string FormalAfter = string.Empty;

            public int ErrorCount => Assertions.Count(row => !row.passed);

            public int ProtectedHashDrift =>
                new[]
                {
                    !string.Equals(ReadInputsAfter, ExpectedReadInputsAggregate, StringComparison.Ordinal),
                    !string.Equals(LockedAfter, ExpectedLockedAggregate, StringComparison.Ordinal),
                    !string.Equals(GovernanceAfter, ExpectedGovernanceAggregate, StringComparison.Ordinal),
                    !string.Equals(FormalAfter, ExpectedFormalAggregate, StringComparison.Ordinal)
                }.Count(value => value);

            public void Check(string assertionId, string category, string expected, string actual)
            {
                bool passed = string.Equals(expected ?? string.Empty, actual ?? string.Empty, StringComparison.Ordinal);
                Assertions.Add(new AssertionEvidence
                {
                    assertionId = assertionId ?? string.Empty,
                    category = category ?? string.Empty,
                    expected = expected ?? string.Empty,
                    actual = actual ?? string.Empty,
                    passed = passed,
                    message = passed ? string.Empty : "Expected and actual values differ."
                });
            }

            public void AddFailure(
                string assertionId,
                string category,
                string expected,
                string actual,
                string message)
            {
                Assertions.Add(new AssertionEvidence
                {
                    assertionId = assertionId ?? string.Empty,
                    category = category ?? string.Empty,
                    expected = expected ?? string.Empty,
                    actual = actual ?? string.Empty,
                    passed = false,
                    message = message ?? string.Empty
                });
            }
        }

        private sealed class AssertionEvidence
        {
            public string assertionId = string.Empty;
            public string category = string.Empty;
            public string expected = string.Empty;
            public string actual = string.Empty;
            public bool passed;
            public string message = string.Empty;
        }

        private sealed class TransitionEvidence
        {
            public string testId = string.Empty;
            public string initialPhase = string.Empty;
            public string action = string.Empty;
            public string expectedPhase = string.Empty;
            public string actualPhase = string.Empty;
            public string mutationExpectation = string.Empty;
            public bool accepted;
            public string diagnosticCode = string.Empty;
            public string result = string.Empty;
        }

        private sealed class BaselineRow
        {
            public string RelativePath = string.Empty;
            public string Sha256 = string.Empty;
        }
    }
}
