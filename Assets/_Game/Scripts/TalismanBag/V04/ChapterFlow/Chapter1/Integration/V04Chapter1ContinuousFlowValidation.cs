using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.V04.ChapterFlow.Chapter1;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    public static class V04Chapter1ContinuousFlowValidation
    {
        public static V04Chapter1ContinuousValidationResult
            ValidateNominal()
        {
            V04Chapter1ContinuousValidationResult result = new();
            ValidateContract(result);
            ValidateTruthAndCulture(result);
            ValidateTargetOwnership(result);
            ValidateThreeActorIndependence(result);
            ValidateHeldWaveFailClose(result);
            ValidateStage19AndBossGate(result);
            ValidateResetLeakBoundaries(result);
            return result;
        }

        private static void ValidateContract(
            V04Chapter1ContinuousValidationResult result)
        {
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract.SchemaId ==
                    "V04Chapter1ContinuousBattleIntegration.v1",
                "INTEGRATION_SCHEMA_ID");
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract.DevOnly
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .FeatureDefaultEnabled
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .FormalFlow
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .WritesSave
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .GrantsReward
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .GrantsDrop
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .WritesInventory
                && !V04Chapter1ContinuousBattleIntegrationContract
                    .ModifiesSceneAsset,
                "BOUNDARY_FLAGS");
            result.Require(
                V04Chapter1ContinuousBattleIntegrationContract
                    .ItemApplicationCadenceTicks == 1500L,
                "ITEM_CADENCE_UNCHANGED");
            result.Require(
                C1EnemyRuntimeCatalog.GetProfiles().Count == 2
                && C1EnemyRuntimeCatalog.GetActionPatterns().Count == 3,
                "C1_RUNTIME_OWNER_REUSED");
        }

        private static void ValidateTruthAndCulture(
            V04Chapter1ContinuousValidationResult result)
        {
            V04Chapter1StageWaveValidationResult truth =
                V04Chapter1StageWaveEncounterValidation.Validate();
            result.Require(
                truth.Passed,
                "ACTOR_TRUTH_" + string.Join("_", truth.errors));

            CultureInfo previous =
                Thread.CurrentThread.CurrentCulture;
            string invariant;
            string alternate;
            try
            {
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.InvariantCulture;
                invariant =
                    V04Chapter1ContinuousFlowSession.StableId(
                        "culture",
                        1234L);
                Thread.CurrentThread.CurrentCulture =
                    CultureInfo.GetCultureInfo("tr-TR");
                alternate =
                    V04Chapter1ContinuousFlowSession.StableId(
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

        private static void ValidateTargetOwnership(
            V04Chapter1ContinuousValidationResult result)
        {
            V04Chapter1WavePlanDefinition wave =
                V04Chapter1StageWaveEncounterTruth
                    .FindPlan("1-1")?.waves[0];
            result.Require(
                V04Chapter1NormalEnemyBattleAdapter
                    .TryCreateValidationFixture(
                        wave,
                        1,
                        out V04Chapter1NormalEnemyBattleAdapter battle,
                        out string diagnostic),
                "TARGET_FIXTURE_" + diagnostic);
            if (battle == null)
            {
                return;
            }

            V04Chapter1NormalEnemyActorSnapshot slot01 =
                battle.ActorSnapshots.Single(value =>
                    value.Plan.slotOrdinal == 1);
            V04Chapter1NormalEnemyActorSnapshot slot02 =
                battle.ActorSnapshots.Single(value =>
                    value.Plan.slotOrdinal == 2);
            result.Require(
                battle.ActorSnapshots.Count == 2
                && battle.ActiveActors.Count == 2
                && slot01.Runtime != null
                && slot02.Runtime != null
                && !string.Equals(
                    slot01.EnemyInstanceId,
                    slot02.EnemyInstanceId,
                    StringComparison.Ordinal)
                && battle.SelectedTargetEnemyInstanceId ==
                    slot01.EnemyInstanceId,
                "B_DEFAULT_SLOT01_TWO_INDEPENDENT_HOSTS");

            int slot01Hp = slot01.Runtime.CurrentHp;
            int slot02Hp = slot02.Runtime.CurrentHp;
            string before =
                battle.SelectedTargetEnemyInstanceId;
            result.Require(
                battle.ApplyDirectDamageForValidation(10, 10L),
                "C_SLOT01_DIRECT_DAMAGE_ACCEPTED");
            IReadOnlyList<V04Chapter1NormalEnemyActorSnapshot> afterFirst =
                battle.ActorSnapshots;
            result.Require(
                Find(afterFirst, slot01.EnemyInstanceId)
                    .Runtime.CurrentHp == slot01Hp - 10
                && Find(afterFirst, slot02.EnemyInstanceId)
                    .Runtime.CurrentHp == slot02Hp,
                "C_DIRECT_DAMAGE_SELECTED_ONLY_SLOT01");
            AddTargetTrace(
                result,
                "default_direct_damage",
                wave.waveId,
                slot01.EnemyInstanceId,
                before,
                battle.SelectedTargetEnemyInstanceId,
                "ACCEPT_SLOT01_ONLY");

            before = battle.SelectedTargetEnemyInstanceId;
            result.Require(
                battle.TrySelectTarget(slot02.EnemyInstanceId),
                "C_SELECT_SLOT02");
            result.Require(
                battle.ApplyDirectDamageForValidation(11, 20L),
                "C_SLOT02_DIRECT_DAMAGE_ACCEPTED");
            IReadOnlyList<V04Chapter1NormalEnemyActorSnapshot> afterSecond =
                battle.ActorSnapshots;
            result.Require(
                Find(afterSecond, slot01.EnemyInstanceId)
                    .Runtime.CurrentHp == slot01Hp - 10
                && Find(afterSecond, slot02.EnemyInstanceId)
                    .Runtime.CurrentHp == slot02Hp - 11,
                "C_DIRECT_DAMAGE_SELECTED_ONLY_SLOT02");
            AddTargetTrace(
                result,
                "explicit_select",
                wave.waveId,
                slot02.EnemyInstanceId,
                before,
                battle.SelectedTargetEnemyInstanceId,
                "ACCEPT_SLOT02_ONLY");

            string legalSelection =
                battle.SelectedTargetEnemyInstanceId;
            result.Require(
                !battle.TrySelectTarget(
                    "illegal.enemy.instance")
                && battle.SelectedTargetEnemyInstanceId ==
                    legalSelection,
                "C_ILLEGAL_ID_PRESERVES_LEGAL_SELECTION");
            AddTargetTrace(
                result,
                "illegal_select",
                wave.waveId,
                "illegal.enemy.instance",
                legalSelection,
                battle.SelectedTargetEnemyInstanceId,
                "REJECT_PRESERVE_SLOT02");

            V04Chapter1NormalEnemyActorSnapshot slot02BeforeDefeat =
                Find(
                    battle.ActorSnapshots,
                    slot02.EnemyInstanceId);
            int terminalDamage =
                slot02BeforeDefeat.Runtime.CurrentHp
                + slot02BeforeDefeat.Runtime.CurrentShell;
            before = battle.SelectedTargetEnemyInstanceId;
            result.Require(
                battle.ApplyDirectDamageForValidation(
                    terminalDamage,
                    30L)
                && battle.ActiveActors.Count == 1
                && battle.SelectedTargetEnemyInstanceId ==
                    slot01.EnemyInstanceId,
                "D_DEATH_RESELECTS_LOWEST_LIVE_SLOT");
            AddTargetTrace(
                result,
                "selected_death",
                wave.waveId,
                slot02.EnemyInstanceId,
                before,
                battle.SelectedTargetEnemyInstanceId,
                "AUTO_RESELECT_SLOT01");

            V04Chapter1NormalEnemyActorSnapshot slot01BeforeDefeat =
                Find(
                    battle.ActorSnapshots,
                    slot01.EnemyInstanceId);
            terminalDamage =
                slot01BeforeDefeat.Runtime.CurrentHp
                + slot01BeforeDefeat.Runtime.CurrentShell;
            before = battle.SelectedTargetEnemyInstanceId;
            result.Require(
                battle.ApplyDirectDamageForValidation(
                    terminalDamage,
                    40L)
                && battle.IsDefeated
                && battle.ActiveActors.Count == 0
                && battle.SelectedTargetEnemyInstanceId ==
                    string.Empty,
                "D_WAVE_CLEAR_CLEARS_SELECTION");
            AddTargetTrace(
                result,
                "wave_clear",
                wave.waveId,
                slot01.EnemyInstanceId,
                before,
                battle.SelectedTargetEnemyInstanceId,
                "CLEAR_SELECTION");
        }

        private static void ValidateThreeActorIndependence(
            V04Chapter1ContinuousValidationResult result)
        {
            V04Chapter1WavePlanDefinition wave =
                V04Chapter1StageWaveEncounterTruth
                    .FindPlan("1-2")?.waves[0];
            result.Require(
                V04Chapter1NormalEnemyBattleAdapter
                    .TryCreateValidationFixture(
                        wave,
                        2,
                        out V04Chapter1NormalEnemyBattleAdapter battle,
                        out string diagnostic),
                "THREE_ACTOR_FIXTURE_" + diagnostic);
            if (battle == null)
            {
                return;
            }

            IReadOnlyList<V04Chapter1NormalEnemyActorSnapshot> actors =
                battle.ActorSnapshots;
            V04Chapter1NormalEnemyActorSnapshot host1 =
                actors.Single(value => value.Plan.slotOrdinal == 1);
            V04Chapter1NormalEnemyActorSnapshot host2 =
                actors.Single(value => value.Plan.slotOrdinal == 2);
            V04Chapter1NormalEnemyActorSnapshot hound =
                actors.Single(value => value.Plan.slotOrdinal == 3);
            result.Require(
                actors.Count == 3
                && actors.Select(value => value.EnemyInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() == 3
                && host1.Runtime.RuntimeProfileId ==
                    host2.Runtime.RuntimeProfileId
                && host1.Runtime.CurrentHp == host2.Runtime.CurrentHp
                && hound.Runtime.CurrentShell > 0,
                "E_THREE_ACTORS_SAME_TYPE_INDEPENDENT");

            int host2Hp = host2.Runtime.CurrentHp;
            result.Require(
                battle.ApplyDirectDamageForValidation(13, 10L)
                && Find(
                        battle.ActorSnapshots,
                        host2.EnemyInstanceId)
                    .Runtime.CurrentHp == host2Hp,
                "E_SLOT01_DAMAGE_DOES_NOT_MUTATE_SAME_TYPE_SLOT02");
        }

        private static void ValidateHeldWaveFailClose(
            V04Chapter1ContinuousValidationResult result)
        {
            V04Chapter1WavePlanDefinition heldWave =
                V04Chapter1StageWaveEncounterTruth
                    .FindPlan("1-4")?.waves[0];
            bool prepared =
                V04Chapter1NormalEnemyBattleAdapter
                    .TryCreateValidationFixture(
                        heldWave,
                        4,
                        out V04Chapter1NormalEnemyBattleAdapter battle,
                        out string diagnostic);
            result.Require(
                !prepared
                && battle == null
                && diagnostic ==
                    V04Chapter1StageWaveEncounterTruth
                        .RuntimeProfileMissingHeldByBad3,
                "G_HELD_WAVE_ZERO_ACTORS");

            V04Chapter1ContinuousFlowSession flow = new();
            result.Require(
                flow.StartChapter1()
                && flow.ConfigureValidationStageFixture(
                    "1-4",
                    new[] { "1-1", "1-2", "1-3" }),
                "G_HELD_FLOW_FIXTURE");
            int completedBefore =
                flow.Snapshot.completedStageIds.Count;
            result.Require(
                flow.RequestCurrentBattle(
                    string.Empty,
                    "held.seed") == null
                && flow.LastDiagnosticCode ==
                    V04Chapter1StageWaveEncounterTruth
                        .RuntimeProfileMissingHeldByBad3
                && flow.ActiveRequest == null
                && flow.CompletedWaveIds.Count == 0
                && flow.Snapshot.completedStageIds.Count ==
                    completedBefore,
                "G_HOLD_NO_PARTIAL_NO_COMPLETED_FACT_POLLUTION");
        }

        private static void ValidateStage19AndBossGate(
            V04Chapter1ContinuousValidationResult result)
        {
            V04Chapter1ContinuousFlowSession flow = new();
            string[] completedBefore =
                Enumerable.Range(1, 8)
                    .Select(value => "1-" + value.ToString(
                        CultureInfo.InvariantCulture))
                    .ToArray();
            result.Require(
                flow.StartChapter1()
                && flow.ConfigureValidationStageFixture(
                    "1-9",
                    completedBefore),
                "F_STAGE19_FIXTURE");
            V04Chapter1WavePlanDefinition wave1 =
                flow.CurrentWavePlan;
            BattleStartRequest request = flow.RequestCurrentBattle(
                string.Empty,
                "stage19.seed");
            result.Require(
                request != null
                && flow.ConfirmBattleStarted(request)
                && wave1?.waveOrdinal == 1
                && wave1.actors.Count == 3,
                "F_STAGE19_W1_RUNNING");

            int completedStageCount =
                flow.Snapshot.completedStageIds.Count;
            int generation1 =
                flow.CurrentWaveResetGeneration;
            result.Require(
                flow.AcceptCurrentWaveDefeat(
                    wave1.waveId,
                    generation1) ==
                    V04Chapter1WaveDefeatDisposition.NextWaveReady
                && flow.CurrentWavePlan?.waveOrdinal == 2
                && flow.CurrentWavePlan.actors.Count == 3
                && flow.Snapshot.completedStageIds.Count ==
                    completedStageCount
                && flow.ActiveRequest?.requestId == request.requestId,
                "F_W1_ONLY_ADVANCES_TO_W2");

            V04Chapter1WavePlanDefinition wave2 =
                flow.CurrentWavePlan;
            int generation2 =
                flow.CurrentWaveResetGeneration;
            result.Require(
                generation2 > generation1
                && flow.AcceptCurrentWaveDefeat(
                    wave2.waveId,
                    generation2) ==
                    V04Chapter1WaveDefeatDisposition.StageResultReady,
                "F_W2_ONLY_READIES_STAGE_RESULT");
            BattleResultSnapshot stageResult =
                V04Chapter1NormalEnemyBattleAdapter
                    .CreateTerminalWinResult(
                        request,
                        1L,
                        500L,
                        "validation.stage19.all_actors_defeated");
            result.Require(
                flow.AcceptBattleResult(stageResult)
                && flow.ContinueFromCurrentBoundary()
                && flow.Snapshot.phase ==
                    V04ChapterFlowPhase.DropCandidatePreview
                && flow.ContinueFromCurrentBoundary()
                && flow.Snapshot.phase ==
                    V04ChapterFlowPhase.BossGate
                && flow.Snapshot.currentStageId == "1-10"
                && flow.CurrentWavePlan?.isBoss == true
                && flow.ActiveRequest == null,
                "F_STAGE19_TO_BOSSGATE_ONLY");
            result.normalBattles++;
            result.bossGates++;

            result.Require(
                flow.RequestCurrentBattle(
                    string.Empty,
                    "boss.before.manual") == null
                && flow.ActiveRequest == null,
                "H_NO_BOSS_SESSION_BEFORE_MANUAL_CHALLENGE");
            result.Require(
                flow.ConfirmManualBossChallenge(),
                "H_MANUAL_CHALLENGE_ACCEPTED");
            result.manualBossChallenges++;
            BattleStartRequest bossRequest =
                flow.RequestCurrentBattle(
                    string.Empty,
                    "boss.after.manual");
            result.Require(
                bossRequest != null
                && bossRequest.isBossStage
                && flow.RequestCurrentBattle(
                    string.Empty,
                    "boss.duplicate") == null
                && flow.ActiveRequest?.requestId ==
                    bossRequest.requestId,
                "H_EXACTLY_ONE_ACCEPTED_BOSS_REQUEST");
        }

        private static void ValidateResetLeakBoundaries(
            V04Chapter1ContinuousValidationResult result)
        {
            V04Chapter1ContinuousFlowSession flow = new();
            result.Require(
                flow.StartChapter1(),
                "I_RESET_START");
            int generation0 =
                flow.CurrentWaveResetGeneration;
            BattleStartRequest request = flow.RequestCurrentBattle(
                string.Empty,
                "reset.seed");
            result.Require(
                request != null
                && flow.ConfirmBattleStarted(request),
                "I_RESET_REQUEST");
            result.Require(
                flow.Reset()
                && flow.CurrentWavePlan == null
                && flow.CurrentWaveResetGeneration == 0
                && flow.CompletedWaveIds.Count == 0
                && flow.ActiveRequest == null,
                "I_RESET_ONE_CLEARS_FLOW_FACTS");
            result.Require(
                flow.StartChapter1()
                && flow.CurrentWaveResetGeneration > generation0,
                "I_RESTART_ONE_FRESH_GENERATION");
            int generation1 =
                flow.CurrentWaveResetGeneration;
            result.Require(
                flow.Reset()
                && flow.StartChapter1()
                && flow.CurrentWaveResetGeneration > generation1
                && flow.CompletedWaveIds.Count == 0
                && flow.ActiveRequest == null,
                "I_RESET_TWO_CLEARS_FLOW_FACTS");

            V04Chapter1WavePlanDefinition wave =
                V04Chapter1StageWaveEncounterTruth
                    .FindPlan("1-1")?.waves[0];
            bool oldPrepared =
                V04Chapter1NormalEnemyBattleAdapter
                    .TryCreateValidationFixture(
                        wave,
                        generation1,
                        out V04Chapter1NormalEnemyBattleAdapter oldBattle,
                        out string oldDiagnostic);
            bool freshPrepared =
                V04Chapter1NormalEnemyBattleAdapter
                    .TryCreateValidationFixture(
                        wave,
                        flow.CurrentWaveResetGeneration,
                        out V04Chapter1NormalEnemyBattleAdapter freshBattle,
                        out string freshDiagnostic);
            result.Require(
                oldPrepared && freshPrepared,
                "I_RUNTIME_FIXTURES_"
                    + oldDiagnostic + "_" + freshDiagnostic);
            if (oldBattle == null || freshBattle == null)
            {
                return;
            }
            oldBattle.ApplyDirectDamageForValidation(7, 1L);
            result.Require(
                !oldBattle.ActorSnapshots.Select(value =>
                        value.EnemyInstanceId)
                    .Intersect(
                        freshBattle.ActorSnapshots.Select(value =>
                            value.EnemyInstanceId),
                        StringComparer.Ordinal)
                    .Any()
                && freshBattle.ActorSnapshots.All(value =>
                    value.Runtime.CurrentHp ==
                        value.Runtime.MaxHp
                    && value.Runtime.CurrentShell ==
                        value.Runtime.ShellMax
                    && value.Runtime.AcceptedApplicationCount == 0
                    && value.Runtime.ActiveAction == null
                    && value.Runtime.Cues.All(cue =>
                        cue.ResetGeneration ==
                            value.Runtime.ResetGeneration)
                    && value.Runtime.PendingRequests.All(request =>
                        request.ResetGeneration ==
                            value.Runtime.ResetGeneration))
                && freshBattle.SelectedTargetEnemyInstanceId ==
                    freshBattle.ActorSnapshots
                        .OrderBy(value => value.Plan.slotOrdinal)
                        .First().EnemyInstanceId
                && freshBattle.PendingPresentationCount == 0,
                "I_NO_ACTOR_HP_SELECTION_REQUEST_CUE_ACTION_LEAK");
        }

        private static V04Chapter1NormalEnemyActorSnapshot Find(
            IReadOnlyList<V04Chapter1NormalEnemyActorSnapshot> actors,
            string enemyInstanceId)
        {
            return actors.Single(value => string.Equals(
                value.EnemyInstanceId,
                enemyInstanceId,
                StringComparison.Ordinal));
        }

        private static void AddTargetTrace(
            V04Chapter1ContinuousValidationResult result,
            string scenario,
            string waveId,
            string requestedEnemyInstanceId,
            string selectedBefore,
            string selectedAfter,
            string outcome)
        {
            result.targetTransitions.Add(
                new V04Chapter1TargetTransitionTraceRow
                {
                    sequence = result.targetTransitions.Count + 1,
                    scenario = scenario ?? string.Empty,
                    waveId = waveId ?? string.Empty,
                    requestedEnemyInstanceId =
                        requestedEnemyInstanceId ?? string.Empty,
                    selectedBefore = selectedBefore ?? string.Empty,
                    selectedAfter = selectedAfter ?? string.Empty,
                    outcome = outcome ?? string.Empty
                });
        }
    }
}
