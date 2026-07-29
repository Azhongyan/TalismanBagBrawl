using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public static class V04Chapter1ContinuousFlowValidation
    {
        public static V04Chapter1ContinuousValidationResult ValidateNominal()
        {
            V04Chapter1ContinuousValidationResult result = new();
            ValidateContract(result);
            ValidateBindings(result);
            ValidateCultureAndOrder(result);
            ValidateEnemyReducerBoundaries(result);
            ValidateRejectedFlowInputs(result);
            BuildNominalTrace(result);
            return result;
        }

        private static void ValidateContract(
            V04Chapter1ContinuousValidationResult result)
        {
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract.SchemaId ==
                    "V04Chapter1ContinuousBattleIntegration.v1",
                "SCHEMA_ID");
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract.DevOnly
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .FeatureDefaultEnabled
                && !V04Chapter1ContinuousBattleIntegrationContract.FormalFlow
                && !V04Chapter1ContinuousBattleIntegrationContract.WritesSave
                && !V04Chapter1ContinuousBattleIntegrationContract.GrantsReward
                && !V04Chapter1ContinuousBattleIntegrationContract.GrantsDrop
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .WritesInventory
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .ModifiesSceneAsset,
                "BOUNDARY_FLAGS");
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract
                    .ItemApplicationCadenceTicks == 500L,
                "CADENCE_500");
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract
                    .TemporaryClearLabels.SequenceEqual(new[]
                    {
                        "PHASE1_DEV_VERTICAL_SLICE",
                        "NOT_CONTENT_FINAL",
                        "NOT_FORMAL_BOSS_COMPLETION"
                    }),
                "PHASE1_LABELS");
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract
                    .CompletionResolverSlotId ==
                    "bline.c1.boss_completion_resolver"
                && V04Chapter1ContinuousBattleIntegrationContract
                    .ReplacementMode ==
                    "REPLACE_COMPLETION_RESOLVER_ONLY",
                "COMPLETION_RESOLVER_BOUNDARY");
        }

        private static void ValidateBindings(
            V04Chapter1ContinuousValidationResult result)
        {
            result.Require(
                V04Chapter1EncounterBindingValidation.Validate().Passed,
                "UPSTREAM_ENCOUNTER_BINDING_VALIDATION");
            result.Require(
                V04Chapter1EncounterManifest.Bindings.Count == 9,
                "NORMAL_BINDING_COUNT");
            result.Require(
                C1EnemyRuntimeCatalog.GetProfiles().Count == 2
                && C1EnemyRuntimeCatalog.GetActionPatterns().Count == 2,
                "C1_RUNTIME_PROFILE_ACTION_COUNT");

            for (int index = 0; index < 9; index++)
            {
                string stageId = "1-" + (index + 1)
                    .ToString(CultureInfo.InvariantCulture);
                V04Chapter1EncounterBindingDefinition binding =
                    V04Chapter1EncounterManifest.Bindings.SingleOrDefault(
                        value => value.stageId == stageId);
                V04ChapterStageDefinition stage =
                    V04ChapterFlowManifest.FindStage(stageId);
                C1EnemyRuntimeProfileSnapshot profile =
                    C1EnemyRuntimeCatalog.GetProfiles().SingleOrDefault(
                        value => value.ContentId ==
                            binding?.activeEnemyContentId);
                result.Require(
                    binding != null && stage != null && profile != null,
                    "STAGE_RESOLUTION_" + stageId);
                result.Require(
                    binding?.chapterFlowSlotId == stage?.contentBindingSlotId,
                    "CONTENT_SLOT_PARITY_" + stageId);
                result.Require(
                    profile != null
                    && (profile.PresentationKey ==
                            C1EnemyRuntimeContract
                                .ShatteredHostPresentationKey
                        || profile.PresentationKey ==
                            C1EnemyRuntimeContract
                                .PorcelainHoundPresentationKey),
                    "PRESENTATION_IDENTITY_" + stageId);
            }

            V04Chapter1EncounterBindingDefinition stage18 =
                V04Chapter1EncounterManifest.Bindings.Single(
                    value => value.stageId == "1-8");
            result.Require(
                stage18.futureReplacementStatus == "HELD_BY_BA-D3"
                && stage18.activeEnemyContentId ==
                    C1EnemyRuntimeContract.ShatteredHostContentId,
                "STAGE_1_8_TEMPORARY_HOLD");
            result.Require(
                V04ChapterFlowManifest.FindStage("1-9")
                    .stopBeforeBossAfterWin,
                "STAGE_1_9_STOP");
            result.Require(
                V04ChapterFlowManifest.FindStage("1-10")
                    .requiresManualBossChallenge,
                "STAGE_1_10_MANUAL");

            C1EnemyHoldAssertion held =
                C1EnemyRuntimeCatalog.GetHoldAssertions().Single(
                    value => value.ContentId ==
                        C1EnemyRuntimeContract.BoneSwapRemnantContentId);
            result.Require(
                held.Status == "HELD_BY_BA-D3"
                && held.RuntimeProfiles == 0
                && held.ActionPatterns == 0
                && held.CopySlots == 0
                && held.RuntimeBindings == 0,
                "BONE_SWAP_REMNANT_ZERO_ROWS");
        }

        private static void ValidateCultureAndOrder(
            V04Chapter1ContinuousValidationResult result)
        {
            string forward = string.Join(",",
                V04Chapter1EncounterManifest.Bindings
                    .Select(value => value.stageId));
            string reversedThenRestored = string.Join(",",
                V04Chapter1EncounterManifest.Bindings
                    .Reverse()
                    .Reverse()
                    .Select(value => value.stageId));
            result.Require(
                forward == reversedThenRestored,
                "INPUT_REVERSAL_STABILITY");

            CultureInfo previous = Thread.CurrentThread.CurrentCulture;
            string invariant;
            string alternate;
            try
            {
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.InvariantCulture;
                invariant = V04Chapter1ContinuousFlowSession.StableId(
                    "culture",
                    1234L);
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.GetCultureInfo("tr-TR");
                alternate = V04Chapter1ContinuousFlowSession.StableId(
                    "culture",
                    1234L);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
            result.Require(
                invariant == alternate,
                "INVARIANT_CULTURE_IDS");
        }

        private static void ValidateEnemyReducerBoundaries(
            V04Chapter1ContinuousValidationResult result)
        {
            C1EnemyRuntimeSnapshot host =
                C1EnemyRuntimeReducer.CreatePresent(
                    C1EnemyRuntimeContract.ShatteredHostProfileId,
                    "validation.host",
                    1,
                    0L);
            host = C1EnemyRuntimeReducer.Activate(host, 0L).Snapshot;
            C1EnemyTransitionResult hostApplied =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    host,
                    new C1EnemyBattleApplicationResult(
                        "validation.host.hp",
                        1L,
                        1,
                        500L,
                        "validation.host",
                        true,
                        1,
                        0,
                        "real.item.request"));
            result.Require(
                hostApplied.Accepted
                && hostApplied.Snapshot.CurrentShell == 0
                && hostApplied.Snapshot.CurrentHp == host.MaxHp - 1,
                "SHATTERED_HOST_HP_ONLY");

            C1EnemyRuntimeSnapshot hound =
                C1EnemyRuntimeReducer.CreatePresent(
                    C1EnemyRuntimeContract.PorcelainHoundProfileId,
                    "validation.hound",
                    1,
                    0L);
            hound = C1EnemyRuntimeReducer.Activate(hound, 0L).Snapshot;
            C1EnemyTransitionResult shellApplied =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    hound,
                    new C1EnemyBattleApplicationResult(
                        "validation.hound.shell",
                        1L,
                        1,
                        500L,
                        "validation.hound",
                        true,
                        0,
                        hound.CurrentShell,
                        "real.item.request"));
            C1EnemyTransitionResult hpApplied =
                C1EnemyRuntimeReducer.ApplyBattleApplication(
                    shellApplied.Snapshot,
                    new C1EnemyBattleApplicationResult(
                        "validation.hound.hp",
                        2L,
                        1,
                        501L,
                        "validation.hound",
                        true,
                        1,
                        0,
                        "real.item.request"));
            result.Require(
                shellApplied.Accepted
                && shellApplied.Snapshot.CurrentShell == 0
                && shellApplied.Snapshot.CurrentHp == hound.CurrentHp
                && hpApplied.Accepted
                && hpApplied.Snapshot.CurrentHp == hound.CurrentHp - 1,
                "PORCELAIN_HOUND_SHELL_FIRST_SEPARATE_RECORDS");
        }

        private static void ValidateRejectedFlowInputs(
            V04Chapter1ContinuousValidationResult result)
        {
            V04Chapter1ContinuousFlowSession flow = new();
            result.Require(flow.StartChapter1(), "REJECTION_FIXTURE_START");
            BattleStartRequest request = flow.RequestCurrentBattle(
                C1EnemyRuntimeContract.ShatteredHostProfileId,
                "seed");
            result.Require(request != null, "REJECTION_FIXTURE_REQUEST");
            string before =
                V04Chapter1ContinuousFlowSession.StateFingerprint(
                    flow.Snapshot);

            V04ChapterFlowTransitionResult wrongRequest = flow.Probe(
                new V04ChapterFlowAction
                {
                    actionType =
                        V04ChapterFlowActionType.ConfirmBattleStarted,
                    requestId = "wrong",
                    stageId = request.stageId,
                    roundId = request.roundId
                });
            result.Require(
                !wrongRequest.accepted
                && before ==
                    V04Chapter1ContinuousFlowSession.StateFingerprint(
                        flow.Snapshot),
                "WRONG_REQUEST_NO_ADVANCE");
            result.Require(
                flow.ConfirmBattleStarted(request),
                "REJECTION_FIXTURE_CONFIRM");

            BattleResultSnapshot contradictory =
                V04Chapter1NormalEnemyBattleAdapter
                    .CreateTerminalWinResult(
                        request,
                        1L,
                        500L,
                        request.enemyProfileId);
            contradictory.lose = true;
            before = V04Chapter1ContinuousFlowSession.StateFingerprint(
                flow.Snapshot);
            V04ChapterFlowTransitionResult contradiction = flow.Probe(
                new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.AcceptBattleResult,
                    stageId = request.stageId,
                    battleResult = contradictory
                });
            result.Require(
                !contradiction.accepted
                && before ==
                    V04Chapter1ContinuousFlowSession.StateFingerprint(
                        flow.Snapshot),
                "CONTRADICTORY_RESULT_NO_ADVANCE");

            BattleResultSnapshot wrongStage =
                V04Chapter1NormalEnemyBattleAdapter
                    .CreateTerminalWinResult(
                        request,
                        2L,
                        500L,
                        request.enemyProfileId);
            result.Require(
                !flow.Probe(new V04ChapterFlowAction
                {
                    actionType = V04ChapterFlowActionType.AcceptBattleResult,
                    stageId = "1-2",
                    battleResult = wrongStage
                }).accepted,
                "WRONG_STAGE_REJECTED");
        }

        private static void BuildNominalTrace(
            V04Chapter1ContinuousValidationResult validation)
        {
            V04Chapter1ContinuousFlowSession flow = new();
            validation.Require(flow.StartChapter1(), "NOMINAL_START");
            long resultSequence = 0L;
            int traceSequence = 0;

            for (int stageIndex = 1; stageIndex <= 9; stageIndex++)
            {
                string stageId = "1-"
                    + stageIndex.ToString(CultureInfo.InvariantCulture);
                V04Chapter1EncounterBindingDefinition binding =
                    V04Chapter1EncounterManifest.Bindings.Single(
                        value => value.stageId == stageId);
                C1EnemyRuntimeProfileSnapshot profile =
                    C1EnemyRuntimeCatalog.GetProfiles().Single(
                        value => value.ContentId ==
                            binding.activeEnemyContentId);
                BattleStartRequest request = flow.RequestCurrentBattle(
                    profile.RuntimeProfileId,
                    V04Chapter1ContinuousFlowSession.StableId(
                        "validation_seed",
                        stageIndex));
                validation.Require(
                    request != null
                    && !request.isBossStage
                    && request.stageId == stageId,
                    "NORMAL_REQUEST_" + stageId);
                validation.Require(
                    flow.ConfirmBattleStarted(request),
                    "NORMAL_CONFIRM_" + stageId);
                BattleResultSnapshot battleResult =
                    V04Chapter1NormalEnemyBattleAdapter
                        .CreateTerminalWinResult(
                            request,
                            ++resultSequence,
                            500L,
                            profile.RuntimeProfileId);
                validation.Require(
                    battleResult != null
                    && !battleResult.bossDefeated
                    && !battleResult.shouldWriteSave
                    && !battleResult.shouldGrantReward
                    && battleResult.rewardPreview.Count == 0
                    && battleResult.itemDrops.Count == 0
                    && battleResult.rewardClaimToken == string.Empty,
                    "NORMAL_RESULT_AUTHORITY_" + stageId);
                validation.Require(
                    flow.AcceptBattleResult(battleResult),
                    "NORMAL_RESULT_ACCEPT_" + stageId);
                validation.normalBattles++;
                validation.trace.Add(new V04Chapter1ContinuousStageTraceRow
                {
                    sequence = ++traceSequence,
                    stageId = stageId,
                    flowPhase = flow.Snapshot.phase,
                    contentId = binding.activeEnemyContentId,
                    runtimeProfileId = profile.RuntimeProfileId,
                    battleRequestId = request.requestId,
                    resultId = battleResult.resultId,
                    outcome = "Win",
                    bossDefeated = false,
                    completedStageCount =
                        flow.Snapshot.completedStageIds.Count,
                    unlockedChapterCount =
                        flow.Snapshot.unlockedChapterIds.Count,
                    labels =
                        V04Chapter1ContinuousBattleIntegrationContract
                            .CadenceLabels
                });
                validation.Require(
                    flow.ContinueFromCurrentBoundary()
                    && flow.Snapshot.phase ==
                        V04ChapterFlowPhase.DropCandidatePreview,
                    "DROP_BOUNDARY_" + stageId);
                validation.Require(
                    flow.ContinueFromCurrentBoundary(),
                    "DROP_CONTINUE_" + stageId);
            }

            validation.Require(
                flow.Snapshot.phase == V04ChapterFlowPhase.BossGate
                && flow.Snapshot.currentStageId == "1-10",
                "STAGE_1_9_STOP_NO_AUTO_BOSS");
            validation.bossGates++;
            validation.Require(
                flow.ConfirmManualBossChallenge(),
                "MANUAL_BOSS_CHALLENGE");
            validation.manualBossChallenges++;
            BattleStartRequest bossRequest = flow.RequestCurrentBattle(
                string.Empty,
                "bline.c1.validation.boss_seed");
            validation.Require(
                bossRequest != null
                && bossRequest.isBossStage
                && bossRequest.stageId == "1-10",
                "BOSS_REQUEST_1_10");
            validation.Require(
                flow.ConfirmBattleStarted(bossRequest),
                "BOSS_CONFIRM_1_10");
            BattleResultSnapshot bossResult =
                V04Chapter1BossPhase1CompletionAdapter
                    .CreateApprovedTemporaryResult(
                        bossRequest,
                        ++resultSequence);
            validation.Require(
                bossResult != null
                && bossResult.bossDefeated
                && V04Chapter1ContinuousBattleIntegrationContract
                    .TemporaryClearLabels.All(
                        label => bossResult.eventSummary.Contains(label))
                && !bossResult.shouldWriteSave
                && !bossResult.shouldGrantReward
                && bossResult.rewardPreview.Count == 0
                && bossResult.itemDrops.Count == 0,
                "PHASE1_TEMPORARY_RESULT_AUTHORITY");
            validation.Require(
                flow.AcceptBattleResult(bossResult),
                "PHASE1_TEMPORARY_RESULT_ACCEPT");
            validation.trace.Add(new V04Chapter1ContinuousStageTraceRow
            {
                sequence = ++traceSequence,
                stageId = "1-10",
                flowPhase = flow.Snapshot.phase,
                contentId = "shougunu.phase1",
                runtimeProfileId = bossRequest.bossProfileId,
                battleRequestId = bossRequest.requestId,
                resultId = bossResult.resultId,
                outcome = "Win",
                bossDefeated = true,
                completedStageCount =
                    flow.Snapshot.completedStageIds.Count,
                unlockedChapterCount =
                    flow.Snapshot.unlockedChapterIds.Count,
                labels = string.Join(";",
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TemporaryClearLabels)
            });
            validation.Require(
                flow.ContinueFromCurrentBoundary()
                && flow.Snapshot.phase ==
                    V04ChapterFlowPhase.ChapterComplete,
                "CHAPTER1_COMPLETE");
            validation.phase1TemporaryClears++;
            validation.Require(
                flow.Snapshot.unlockedChapterIds.Contains(
                    "bone_aspect_chapter_2"),
                "SESSION_ONLY_CHAPTER2_UNLOCK");
            validation.sessionUnlocks++;
            validation.Require(
                validation.trace.Count == 10
                && validation.normalBattles == 9,
                "NOMINAL_TRACE_COUNTS");
        }
    }
}
