using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.NianResource;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.Items.Combat;
using TalismanBag.V04.ChapterFlow.Chapter1;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(720)]
    public sealed class V04Chapter1BattleSandboxRuntimeController :
        MonoBehaviour,
        IC1EncounterAdmissionAuthority,
        IBattleSandboxAcceptedDamagePresentationSource
    {
        private const float BossHandoffTimeoutSeconds = 3f;
        private V04Chapter1ContinuousFlowSession flow;
        private V04Chapter1NormalEnemyBattleAdapter normalBattle;
        private ShougunuPhase1BattleSandboxVerticalSliceRuntime bossRuntime;
        private BattleStartRequest pendingBossRequest;
        private long normalElapsedTick;
        private long resultSequence;
        private int resetGeneration;
        private bool bossStartConfirmed;
        private float bossHandoffElapsed;
        private bool bossHandoffTimedOut;
        private C1EncounterAdmissionSnapshot encounterAdmission =
            C1EncounterAdmissionSnapshot.None();
        private ShougunuPhase1BattleSandboxSceneBinder authoredSceneBinder;
        private C1BattleSandboxAuthoredEnemyPresentationAdapter
            ordinaryPresentationAdapter;
        private Button authoredStartButton;
        private bool authoredStartButtonWired;
        private bool normalBattlePausedByPrepareSurface;
        private bool normalBattleResumeFramePending;
        private long ordinaryTransitionSequence;
        private C1OrdinaryStageTransitionSnapshot ordinaryStageTransition =
            C1OrdinaryStageTransitionSnapshot.None;
        private bool ordinaryStageSwitchApplied;
        private bool ordinaryTransitionCompletionApplied;

        public V04Chapter1ContinuousFlowSession Flow => flow;
        public V04ChapterFlowStateSnapshot Snapshot => flow?.Snapshot;
        public V04Chapter1NormalEnemyBattleAdapter NormalBattle => normalBattle;
        public ShougunuPhase1BattleSandboxVerticalSliceRuntime BossRuntime =>
            bossRuntime;
        public string LastDiagnosticCode { get; private set; } =
            "DEV_HANDTEST_READY";
        public string LastDiagnosticDetail { get; private set; } =
            "Dev-only/default-off. Start Chapter 1 Session.";
        public bool NormalBattleRunning => normalBattle?.IsRunning == true;
        public bool BossBattlePendingOrRunning =>
            pendingBossRequest != null;
        public bool IsSessionOnlyChapter2Unlocked =>
            Snapshot?.unlockedChapterIds?.Contains(
                "bone_aspect_chapter_2") == true;
        public C1EncounterAdmissionSnapshot EncounterAdmission =>
            encounterAdmission;
        public string PresentationSourceId =>
            "C1_ORDINARY_ENCOUNTER";
        public bool HasActivePresentationSession =>
            encounterAdmission?.IsOrdinary == true
            && normalBattle != null;
        public int PresentationGeneration =>
            normalBattle?.Enemy?.ResetGeneration ?? 0;
        public IBattleSandboxAcceptedDamagePresentation
            LastAcceptedDamagePresentation =>
                normalBattle?.LastAcceptedPresentation;
        public ItemCombatEffectRequestSnapshot
            CurrentPresentationItemRequestSnapshot =>
                normalBattle?.ItemRequestSnapshot;
        public bool AuthoredStartSurfaceBound =>
            authoredStartButtonWired
            && authoredStartButton != null;
        public string AuthoredStartSurfaceName =>
            authoredStartButton?.gameObject.name ?? string.Empty;
        public bool OrdinaryBattlePausedByPrepareSurface =>
            normalBattle?.IsRunning == true
            && authoredSceneBinder?.GridController
                ?.IsSandboxBattleModeActive != true;
        public C1OrdinaryStageTransitionSnapshot OrdinaryStageTransition =>
            ordinaryStageTransition;
        public IReadOnlyList<V04Chapter1NormalEnemyActorSnapshot>
            ActiveNormalActors =>
                normalBattle?.ActiveActors
                ?? Array.Empty<
                    V04Chapter1NormalEnemyActorSnapshot>();
        public string SelectedTargetEnemyInstanceId =>
            normalBattle?.SelectedTargetEnemyInstanceId
            ?? string.Empty;

        public bool TrySelectCurrentNormalTarget(
            string enemyInstanceId)
        {
            if (normalBattle?.IsRunning != true)
            {
                LastDiagnosticCode =
                    "TARGET_SELECTION_BATTLE_NOT_RUNNING";
                return false;
            }
            bool selected =
                normalBattle.TrySelectTarget(enemyInstanceId);
            LastDiagnosticCode =
                normalBattle.LastDiagnosticCode;
            LastDiagnosticDetail = selected
                ? normalBattle.SelectedTargetEnemyInstanceId
                : "Selection accepts only a live targetable Actor in "
                    + "the current Wave; a valid current selection is "
                    + "preserved.";
            return selected;
        }

        private void Awake()
        {
            flow = new V04Chapter1ContinuousFlowSession();
            PublishNoAdmission();
        }

        private void Update()
        {
            TryWireAuthoredStartSurface();
            MaintainBossGateIdleSurface();
            UpdateNormalBattle();
            UpdateBossBattle();
            UpdateOrdinaryTransitionRunningPhase();
        }

        private void MaintainBossGateIdleSurface()
        {
            if (Snapshot?.phase == V04ChapterFlowPhase.BossGate
                && string.Equals(
                    Snapshot.currentStageId,
                    "1-10",
                    StringComparison.Ordinal)
                && encounterAdmission?.AdmittedEncounterKind
                    == C1EncounterAdmissionKind.None
                && pendingBossRequest == null)
            {
                RestoreAuthoredPrepareSurfaceForBossGate();
            }
        }

        public void StartChapter1Session()
        {
            ResetRuntimeAdapters();
            resetGeneration++;
            if (flow.StartChapter1())
            {
                PublishAdmissionForCurrentFlow();
                LastDiagnosticCode = "CHAPTER1_PREPARE_READY";
                LastDiagnosticDetail =
                    "Prepare the real Item board, then start stage "
                    + Snapshot.currentStageId + ".";
            }
            else
            {
                SetFlowDiagnostic();
            }
        }

        public void StartCurrentNormalStage()
        {
            if (ordinaryStageTransition.HasSequence
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.None
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.NextBattleReady
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.BattleRunning)
            {
                LastDiagnosticCode =
                    "ORDINARY_TRANSITION_PRESENTATION_NOT_READY";
                LastDiagnosticDetail =
                    "Normal Battle cannot start until the accepted stage "
                    + "transition presentation reaches Ready.";
                return;
            }
            if (Snapshot?.phase != V04ChapterFlowPhase.Prepare
                || Snapshot.currentStageId == "1-10")
            {
                LastDiagnosticCode = "NORMAL_STAGE_NOT_AVAILABLE";
                LastDiagnosticDetail =
                    "Normal Battle starts only from Prepare for 1-1..1-9.";
                return;
            }

            TryStartCurrentWave(createStageBattleRequest: true);
        }

        private bool TryStartCurrentWave(bool createStageBattleRequest)
        {
            V04Chapter1WavePlanDefinition wave =
                flow?.CurrentWavePlan;
            if (wave == null
                || wave.isBoss
                || !string.Equals(
                    wave.stageId,
                    Snapshot?.currentStageId,
                    StringComparison.Ordinal))
            {
                normalBattle = null;
                PublishNoAdmission(Snapshot?.currentStageId);
                LastDiagnosticCode = "CURRENT_WAVE_ENTRY_MISSING";
                LastDiagnosticDetail =
                    "ChapterFlow owns no ordinary wave for the current stage.";
                return false;
            }
            if (!wave.HasAllRuntimeProfiles)
            {
                normalBattle = null;
                PublishNoAdmission(wave.stageId);
                LastDiagnosticCode =
                    V04Chapter1StageWaveEncounterTruth
                        .RuntimeProfileMissingHeldByBad3;
                LastDiagnosticDetail =
                    wave.waveId + " / heldActors="
                    + string.Join(
                        ",",
                        wave.actors
                            .Where(value => !value.HasRuntimeProfile)
                            .Select(value => value.waveEntryId))
                    + ". Whole-wave preflight rejected before Actor "
                    + "creation; no substitute Runtime, merged HP, partial "
                    + "Wave, or d1_3 presentation was admitted.";
                return false;
            }

            bool everyActorResolves = wave.actors.All(actor =>
                C1EnemyRuntimeCatalog.GetProfiles().Any(value =>
                    string.Equals(
                        value.ContentId,
                        actor.enemyContentId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.RuntimeProfileId,
                        actor.runtimeProfileId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        value.PresentationKey,
                        actor.visualProfileKey,
                        StringComparison.Ordinal)));
            if (!everyActorResolves)
            {
                normalBattle = null;
                PublishNoAdmission(wave.stageId);
                LastDiagnosticCode = "CURRENT_WAVE_RUNTIME_PROFILE_MISMATCH";
                LastDiagnosticDetail =
                    "Wave identity did not resolve to one exact existing "
                    + "content/runtime/visual profile; no fallback is allowed.";
                return false;
            }

            ItemSystemBattleSandboxBoardAdapter[] adapters =
                Resources.FindObjectsOfTypeAll<
                        ItemSystemBattleSandboxBoardAdapter>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (adapters.Length != 1 || adapters[0].Authority == null)
            {
                LastDiagnosticCode = adapters.Length > 1
                    ? "ITEM_AUTHORITY_DUPLICATE"
                    : "ITEM_AUTHORITY_ABSENT";
                LastDiagnosticDetail =
                    "Exactly one current BattleSandbox Item authority is required.";
                return false;
            }

            if (!V04Chapter1NormalEnemyBattleAdapter.TryPrepare(
                    adapters[0].Authority,
                    wave,
                    flow.CurrentWaveResetGeneration,
                    out V04Chapter1NormalEnemyBattleAdapter prepared,
                    out string diagnostic))
            {
                LastDiagnosticCode = diagnostic;
                LastDiagnosticDetail =
                    "Real Item request / C1 target preparation rejected; ChapterFlow did not advance.";
                return false;
            }

            BattleStartRequest request = createStageBattleRequest
                ? flow.RequestCurrentBattle(
                    string.Empty,
                    V04Chapter1ContinuousFlowSession.StableId(
                        "seed",
                        flow.CurrentWaveResetGeneration))
                : flow.ProjectCurrentWaveBattleRequest();
            if (request == null
                || !prepared.BindAcceptedBattleRequest(request)
                || (createStageBattleRequest
                    && !flow.ConfirmBattleStarted(request)))
            {
                LastDiagnosticCode = "NORMAL_BATTLE_START_REJECTED";
                LastDiagnosticDetail =
                    flow.LastDiagnosticCode + " / "
                    + prepared.LastDiagnosticCode;
                return false;
            }

            normalBattle = prepared;
            normalElapsedTick = 0L;
            normalBattlePausedByPrepareSurface = false;
            normalBattleResumeFramePending = false;
            PublishAdmissionForCurrentFlow();
            ResolveAuthoredSceneBinder();
            authoredSceneBinder?.BeginOrdinaryGeneration(
                normalBattle.Enemy,
                normalBattle.PlayerCurrentHp);
            LastDiagnosticCode = "NORMAL_BATTLE_RUNNING";
            LastDiagnosticDetail =
                wave.waveId + " / actors=" + wave.actors.Count + " / "
                + "selected="
                + normalBattle.SelectedTargetEnemyInstanceId + " / "
                + V04Chapter1ContinuousBattleIntegrationContract.CadenceLabels
                + " / "
                + V04Chapter1ContinuousBattleIntegrationContract
                    .ResolverLabels;
            return true;
        }

        public void ContinueFromSettlementOrDropCandidate()
        {
            if (ordinaryStageTransition.HasSequence
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.None
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.BattleRunning)
            {
                LastDiagnosticCode =
                    "ORDINARY_TRANSITION_AUTO_ADVANCE_OWNED";
                LastDiagnosticDetail =
                    "Accepted victory progression is owned by the auto "
                    + "transition contract; manual continue is suppressed.";
                return;
            }
            if (!AdvanceOrdinaryVictoryBoundaryToNextStage())
            {
                return;
            }
            if (Snapshot.phase == V04ChapterFlowPhase.DropCandidatePreview)
            {
                LastDiagnosticCode = "DROP_CANDIDATE_NO_GRANT";
                LastDiagnosticDetail =
                    "Formal reward/drop is not connected; grants=0, inventoryWrites=0.";
            }
            else if (Snapshot.phase == V04ChapterFlowPhase.BossGate)
            {
                LastDiagnosticCode = "STAGE_1_9_STOP_BOSS_GATE_OPEN";
                LastDiagnosticDetail =
                    "1-9 stop observed. Keep arranging the board until "
                    + V04Chapter1BossPhase1CompletionAdapter
                        .RequiredBossItemChain
                    + " is ready, then use Manual Challenge; no automatic 1-10.";
            }
            else if (Snapshot.phase == V04ChapterFlowPhase.ChapterComplete)
            {
                LastDiagnosticCode = "PHASE1_TEMPORARY_CLEAR";
                LastDiagnosticDetail = string.Join(" / ",
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TemporaryClearLabels)
                    + " / Chapter 2 unlocked for this session.";
            }
            else
            {
                LastDiagnosticCode = "NEXT_STAGE_PREPARE_READY";
                LastDiagnosticDetail =
                    "Prepare/start " + Snapshot.currentStageId + ".";
            }
        }

        public void ManualChallengeShougunuPhase1()
        {
            if (pendingBossRequest != null)
            {
                RetryPendingBossHandoff();
                return;
            }
            if (Snapshot?.phase != V04ChapterFlowPhase.BossGate
                || Snapshot.currentStageId != "1-10")
            {
                LastDiagnosticCode = "MANUAL_BOSS_CHALLENGE_NOT_AVAILABLE";
                return;
            }

            ItemSystemBattleSandboxBoardAdapter[] itemAdapters =
                Resources.FindObjectsOfTypeAll<
                        ItemSystemBattleSandboxBoardAdapter>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (itemAdapters.Length != 1
                || itemAdapters[0].Authority == null)
            {
                LastDiagnosticCode = itemAdapters.Length > 1
                    ? "BOSS_ITEM_AUTHORITY_DUPLICATE"
                    : "BOSS_ITEM_AUTHORITY_ABSENT";
                LastDiagnosticDetail =
                    "Boss readiness blocked; ChapterFlow remains at BossGate. "
                    + "Exactly one current BattleSandbox Item authority is required. "
                    + "Keep arranging "
                    + V04Chapter1BossPhase1CompletionAdapter
                        .RequiredBossItemChain
                    + " and retry.";
                return;
            }
            if (!V04Chapter1BossPhase1CompletionAdapter
                .TryValidateBossReadiness(
                    itemAdapters[0].Authority,
                    out string readinessDiagnostic))
            {
                LastDiagnosticCode =
                    "BOSS_REAL_ITEM_CHAIN_NOT_READY_RETRYABLE";
                LastDiagnosticDetail = readinessDiagnostic;
                return;
            }

            ShougunuPhase1BattleSandboxVerticalSliceRuntime[] runtimes =
                Resources.FindObjectsOfTypeAll<
                        ShougunuPhase1BattleSandboxVerticalSliceRuntime>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            foreach (ShougunuPhase1BattleSandboxVerticalSliceRuntime runtime
                in runtimes)
            {
                runtime.BindEncounterAdmissionAuthority(this);
            }
            if (runtimes.Length != 1
                || !runtimes[0].DevOnly
                || !runtimes[0].IsEnabled
                || runtimes[0].HasActiveSession
                || runtimes[0].GridController == null
                || runtimes[0].SceneBinder?.IsBindingComplete != true
                || runtimes[0].SceneBinder.AuthoredResetButton == null
                || runtimes[0].GridController.IsSandboxBattleModeActive)
            {
                LastDiagnosticCode = runtimes.Length != 1
                    ? "SHOUGUNU_RUNTIME_COUNT_REJECTED"
                    : "SHOUGUNU_AUTHORED_HANDOFF_NOT_READY";
                LastDiagnosticDetail =
                    "Boss readiness is valid, but exactly one idle accepted Runtime "
                    + "with its authored Button and preparation surface is required. "
                    + "ChapterFlow remains at BossGate; no request was consumed.";
                return;
            }

            if (!flow.ConfirmManualBossChallenge())
            {
                SetFlowDiagnostic();
                return;
            }
            V04ChapterStageDefinition bossStage =
                V04ChapterFlowManifest.FindStage("1-10");
            BattleStartRequest request = flow.RequestCurrentBattle(
                string.Empty,
                V04Chapter1ContinuousFlowSession.StableId(
                    "boss_seed",
                    ++resetGeneration));
            if (request == null
                || !string.Equals(
                    request.bossProfileId,
                    bossStage.bossProfileId,
                    StringComparison.Ordinal))
            {
                LastDiagnosticCode = "BOSS_BATTLE_REQUEST_REJECTED";
                return;
            }
            bossRuntime = runtimes[0];
            pendingBossRequest = request;
            bossStartConfirmed = false;
            bossHandoffElapsed = 0f;
            bossHandoffTimedOut = false;
            PublishAcceptedBossAdmission(request);
            InvokeAcceptedAuthoredBossButtonOnce();
        }

        public void ResetDevSession()
        {
            if (pendingBossRequest != null
                || bossRuntime?.HasActiveSession == true)
            {
                LastDiagnosticCode =
                    "DEV_RESET_BLOCKED_DURING_ACCEPTED_BOSS_HANDOFF";
                LastDiagnosticDetail =
                    "Do not bypass the accepted Boss surface. If the bounded "
                    + "handoff diagnostic cannot be retried, exit Play Mode "
                    + "to clear all dev-only session state safely.";
                return;
            }
            ResetRuntimeAdapters();
            resetGeneration++;
            flow = new V04Chapter1ContinuousFlowSession();
            PublishNoAdmission();
            LastDiagnosticCode = "DEV_SESSION_RESET";
            LastDiagnosticDetail =
                "Session-only Chapter 2 unlock cleared; no persistent write.";
        }

        private void UpdateNormalBattle()
        {
            if (normalBattle?.IsRunning != true)
            {
                return;
            }
            ResolveAuthoredSceneBinder();
            bool battleSurfaceActive =
                authoredSceneBinder?.GridController
                    ?.IsSandboxBattleModeActive == true;
            if (!battleSurfaceActive)
            {
                normalBattlePausedByPrepareSurface = true;
                normalBattleResumeFramePending = true;
                authoredSceneBinder?.ApplyOrdinarySnapshot(
                    normalBattle.Enemy,
                    normalBattle.PlayerCurrentHp,
                    normalBattle.CurrentTick);
                LastDiagnosticCode = "NORMAL_BATTLE_PAUSED_PREPARE_SURFACE";
                LastDiagnosticDetail =
                    "Ordinary Battle clock is frozen while the authored "
                    + "prepare surface remains open.";
                return;
            }
            if (normalBattlePausedByPrepareSurface)
            {
                normalBattlePausedByPrepareSurface = false;
                if (normalBattleResumeFramePending)
                {
                    normalBattleResumeFramePending = false;
                    authoredSceneBinder?.ApplyOrdinarySnapshot(
                        normalBattle.Enemy,
                        normalBattle.PlayerCurrentHp,
                        normalBattle.CurrentTick);
                    LastDiagnosticCode =
                        "NORMAL_BATTLE_RESUME_RESYNC_FRAME";
                    LastDiagnosticDetail =
                        "Prepare closed; reserving one authored-slot resync "
                        + "frame before the ordinary Battle clock advances.";
                    return;
                }
            }
            if (ordinaryStageTransition.HasSequence
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.None
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.BattleRunning
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.NextBattleReady)
            {
                authoredSceneBinder?.ApplyOrdinarySnapshot(
                    normalBattle.Enemy,
                    normalBattle.PlayerCurrentHp,
                    normalBattle.CurrentTick);
                return;
            }
            normalElapsedTick += Math.Max(
                1L,
                (long)Math.Floor(Time.unscaledDeltaTime * 1000f));
            normalBattle.AdvanceTo(normalElapsedTick);
            authoredSceneBinder?.ApplyOrdinarySnapshot(
                normalBattle.Enemy,
                normalBattle.PlayerCurrentHp,
                normalBattle.CurrentTick);
            while (normalBattle.TryDequeuePresentation(
                out C1OrdinaryEncounterApplicationPresentationEvent
                    presentation))
            {
                if (!TryPresentAcceptedOrdinaryApplication(presentation))
                {
                    LastDiagnosticCode =
                        "NORMAL_PRESENTATION_CONSUMER_REJECTED";
                    LastDiagnosticDetail =
                        presentation.ApplicationIdentity;
                }
            }
            while (normalBattle.TryDequeueEnemyToPlayerPresentation(
                out C1OrdinaryEncounterEnemyToPlayerPresentationEvent
                    playerPresentation))
            {
                if (!TryPresentEnemyToPlayerDamage(playerPresentation))
                {
                    LastDiagnosticCode =
                        "NORMAL_PLAYER_PRESENTATION_CONSUMER_REJECTED";
                    LastDiagnosticDetail =
                        playerPresentation.EventId;
                }
            }
            if (normalBattle.IsDefeated)
            {
                V04Chapter1NormalEnemyBattleAdapter defeatedBattle =
                    normalBattle;
                V04Chapter1WavePlanDefinition defeatedWave =
                    flow.CurrentWavePlan;
                V04Chapter1WaveDefeatDisposition disposition =
                    flow.AcceptCurrentWaveDefeat(
                        defeatedWave?.waveId,
                        defeatedBattle.WaveResetGeneration);
                if (disposition ==
                    V04Chapter1WaveDefeatDisposition.NextWaveReady)
                {
                    normalBattle = null;
                    PublishNoAdmission(Snapshot?.currentStageId);
                    if (TryStartCurrentWave(
                        createStageBattleRequest: false))
                    {
                        LastDiagnosticCode = "NEXT_WAVE_BATTLE_RUNNING";
                        LastDiagnosticDetail =
                            flow.CurrentWavePlan.waveId
                            + " / sameStageRequestId="
                            + flow.ActiveRequest.requestId;
                    }
                    return;
                }
                if (disposition !=
                    V04Chapter1WaveDefeatDisposition.StageResultReady)
                {
                    LastDiagnosticCode = "CURRENT_WAVE_DEFEAT_REJECTED";
                    LastDiagnosticDetail = flow.LastDiagnosticCode;
                    return;
                }

                BattleResultSnapshot result =
                    defeatedBattle.CreateTerminalResult(++resultSequence);
                if (result != null && flow.AcceptBattleResult(result))
                {
                    if (TryQueueOrdinaryStageTransition())
                    {
                        LastDiagnosticCode =
                            "ORDINARY_TRANSITION_VICTORY_ACCEPTED";
                        LastDiagnosticDetail =
                            "Accepted victory is frozen for one automatic "
                            + "stage transition; Flow remains the sole "
                            + "stage owner.";
                    }
                    else
                    {
                        LastDiagnosticCode = "NORMAL_SETTLEMENT_READY";
                        LastDiagnosticDetail =
                            "Terminal Enemy lifecycle accepted. Settlement is visible; reward/drop grants remain zero.";
                    }
                }
                else
                {
                    LastDiagnosticCode = "NORMAL_RESULT_REJECTED";
                    LastDiagnosticDetail = flow.LastDiagnosticCode;
                }
            }
            else if (!normalBattle.IsRunning)
            {
                LastDiagnosticCode = normalBattle.LastDiagnosticCode;
                LastDiagnosticDetail = normalBattle.LastDiagnosticDetail;
            }
        }

        private bool TryPresentAcceptedOrdinaryApplication(
            C1OrdinaryEncounterApplicationPresentationEvent presentation)
        {
            if (presentation == null
                || !presentation.IsAcceptedCorrelation)
            {
                return false;
            }

            ResolveAuthoredSceneBinder();
            ResolveOrdinaryPresentationAdapter();
            BattleSandboxAuthoredTmpPresentation tmp =
                authoredSceneBinder?.AuthoredTmpPresentation;
            BattleSandboxItemTriggerFeedbackController itemFeedback =
                authoredSceneBinder?.AcceptedItemFeedbackController;
            if (tmp == null
                || !tmp.HasAuthoredTemplates
                || itemFeedback == null)
            {
                return false;
            }

            if (!itemFeedback.TryResolveBoardSourceWorldPosition(
                    presentation.SourceBaseItemId,
                    presentation.OccupiedCells,
                    out Vector3 sourceWorld))
            {
                return false;
            }

            bool sourceAccepted = tmp.TrySpawnAtWorldPosition(
                presentation.ResetGeneration,
                presentation.EventId,
                "ordinary.item.source.resolved_damage",
                sourceWorld,
                presentation.ResolvedPreMitigationDamageUnits
                    .ToString(CultureInfo.InvariantCulture),
                BattleSandboxAuthoredTmpStyle.SecondaryCyan,
                0.74f,
                0.04f,
                BattleSandboxAuthoredTmpPresentation
                    .DefaultReadableLifetime,
                presentation.SourceBaseItemId,
                new Vector2(0f, 18f));

            RectTransform targetAnchor = null;
            ordinaryPresentationAdapter?.TryResolveEnemyDamageAnchor(
                presentation.EnemySnapshot?.EnemyInstanceId,
                out targetAnchor);
            targetAnchor ??=
                ordinaryPresentationAdapter?.AuthoredRoot?.Slots
                    ?.FirstOrDefault(value => value != null
                        && value.IsRuntimeVisible
                        && string.Equals(
                            value.DisplaySlotId,
                            presentation.EnemySnapshot?.EnemyInstanceId,
                            StringComparison.Ordinal))
                    ?.RuntimeDamageAnchor;
            targetAnchor ??=
                authoredSceneBinder?.VisualCueAdapter?.DamageDealtAnchor;
            if (targetAnchor == null)
            {
                return false;
            }

            bool shellOnly = presentation.ShellDamageApplied > 0
                && presentation.HpDamageApplied == 0;
            string unit = shellOnly
                ? " SH"
                : presentation.ShellDamageApplied > 0
                    ? " DMG"
                    : " HP";
            string paletteKey = shellOnly
                ? BattleSandboxAuthoredTmpPresentation
                    .SettlementShellPaletteKey
                : BattleSandboxAuthoredTmpPresentation
                    .SettlementHpPaletteKey;
            BattleSandboxAuthoredTmpStyle style = shellOnly
                ? BattleSandboxAuthoredTmpStyle.SecondaryCyan
                : BattleSandboxAuthoredTmpStyle.PrimaryWarm;
            bool settlementAccepted = tmp.TrySpawn(
                presentation.ResetGeneration,
                presentation.EventId,
                "ordinary.item.enemy.settlement."
                + (presentation.EnemySnapshot?.EnemyInstanceId
                    ?? string.Empty),
                targetAnchor,
                "-"
                + presentation.TotalDamageApplied.ToString(
                    CultureInfo.InvariantCulture)
                + unit,
                style,
                1f,
                0.32f,
                BattleSandboxAuthoredTmpPresentation
                    .DefaultReadableLifetime,
                paletteKey);
            return sourceAccepted && settlementAccepted;
        }

        private bool TryPresentEnemyToPlayerDamage(
            C1OrdinaryEncounterEnemyToPlayerPresentationEvent presentation)
        {
            if (presentation == null
                || !presentation.IsAcceptedCorrelation)
            {
                return false;
            }

            ResolveAuthoredSceneBinder();
            BattleSandboxAuthoredTmpPresentation tmp =
                authoredSceneBinder?.AuthoredTmpPresentation;
            RectTransform playerAnchor =
                authoredSceneBinder?.VisualCueAdapter?.DamageTakenAnchor;
            if (tmp == null
                || !tmp.HasAuthoredTemplates
                || playerAnchor == null)
            {
                return false;
            }

            return tmp.TrySpawn(
                presentation.ResetGeneration,
                presentation.EventId,
                "ordinary.enemy.player.settlement",
                playerAnchor,
                "-"
                + presentation.HpDamageApplied.ToString(
                    CultureInfo.InvariantCulture)
                + " HP",
                BattleSandboxAuthoredTmpStyle.PrimaryWarm,
                1f,
                0f,
                BattleSandboxAuthoredTmpPresentation
                    .DefaultReadableLifetime,
                BattleSandboxAuthoredTmpPresentation
                    .SettlementHpPaletteKey);
        }

        private void ResolveOrdinaryPresentationAdapter()
        {
            if (ordinaryPresentationAdapter != null
                && ordinaryPresentationAdapter.gameObject.scene
                    == gameObject.scene)
            {
                return;
            }

            ordinaryPresentationAdapter = Resources
                .FindObjectsOfTypeAll<
                    C1BattleSandboxAuthoredEnemyPresentationAdapter>()
                .FirstOrDefault(value => value != null
                    && value.gameObject.scene == gameObject.scene);
        }

        private void UpdateBossBattle()
        {
            if (pendingBossRequest == null || bossRuntime == null)
            {
                return;
            }
            if (!bossStartConfirmed && bossRuntime.HasActiveSession)
            {
                if (!flow.ConfirmBattleStarted(pendingBossRequest))
                {
                    LastDiagnosticCode = "BOSS_START_CONFIRM_REJECTED";
                    LastDiagnosticDetail = flow.LastDiagnosticCode;
                    pendingBossRequest = null;
                    return;
                }
                bossStartConfirmed = true;
                bossHandoffTimedOut = false;
                LastDiagnosticCode = "PHASE1_VERTICAL_SLICE_RUNNING";
                LastDiagnosticDetail = string.Join(
                    " / ",
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TemporaryClearLabels);
            }
            else if (!bossStartConfirmed)
            {
                bossHandoffElapsed += Time.unscaledDeltaTime;
                if (!bossHandoffTimedOut
                    && bossHandoffElapsed >= BossHandoffTimeoutSeconds)
                {
                    bossHandoffTimedOut = true;
                    bool surfaceActive =
                        bossRuntime.GridController
                            ?.IsSandboxBattleModeActive == true;
                    LastDiagnosticCode = surfaceActive
                        ? "BOSS_HANDOFF_TIMEOUT_RUNTIME_NOT_READY"
                        : "BOSS_HANDOFF_TIMEOUT_SURFACE_NOT_ACTIVE_RETRYABLE";
                    LastDiagnosticDetail = surfaceActive
                        ? "The authored Battle surface became active, but the "
                            + "accepted Runtime did not obtain a session within "
                            + "3 seconds. Runtime diagnostic: "
                            + bossRuntime.LastDiagnosticCode + " / "
                            + bossRuntime.LastDiagnosticDetail
                            + ". Do not double-toggle; exit Play Mode for a safe reset."
                        : "The authored Battle surface did not become active "
                            + "within 3 seconds. The existing Boss request is "
                            + "retained; click Manual Challenge once to revalidate "
                            + "the exact Item chain and retry the authored Button.";
                }
            }
            if (!bossStartConfirmed
                || !V04Chapter1BossPhase1CompletionAdapter
                    .IsApprovedTemporaryTerminal(bossRuntime))
            {
                return;
            }
            BattleResultSnapshot result =
                V04Chapter1BossPhase1CompletionAdapter.Resolve(
                    pendingBossRequest,
                    ++resultSequence,
                    bossRuntime);
            if (result != null && flow.AcceptBattleResult(result))
            {
                LastDiagnosticCode = "PHASE1_TEMPORARY_SETTLEMENT_READY";
                LastDiagnosticDetail = string.Join(" / ",
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TemporaryClearLabels);
            }
            else
            {
                LastDiagnosticCode = "PHASE1_TEMPORARY_RESULT_REJECTED";
                LastDiagnosticDetail = flow.LastDiagnosticCode;
            }
            pendingBossRequest = null;
        }

        public bool IsShougunuPhase1StartAdmitted(
            string requesterSceneName)
        {
            return encounterAdmission?.IsAcceptedBoss == true
                && string.Equals(
                    requesterSceneName,
                    gameObject.scene.name,
                    StringComparison.Ordinal);
        }

        private void RetryPendingBossHandoff()
        {
            if (bossRuntime == null || pendingBossRequest == null)
            {
                LastDiagnosticCode = "BOSS_HANDOFF_RETRY_STATE_INVALID";
                return;
            }
            if (bossRuntime.HasActiveSession)
            {
                LastDiagnosticCode = "BOSS_RUNTIME_SESSION_CONFIRM_PENDING";
                LastDiagnosticDetail =
                    "The accepted Runtime is active; ChapterFlow start confirmation "
                    + "will be applied by the controller update.";
                return;
            }
            if (!bossHandoffTimedOut)
            {
                LastDiagnosticCode = "BOSS_HANDOFF_STILL_PENDING";
                LastDiagnosticDetail =
                    "Wait for the bounded 3-second authored-surface handoff diagnostic.";
                return;
            }
            if (bossRuntime.GridController?.IsSandboxBattleModeActive == true)
            {
                LastDiagnosticCode =
                    "BOSS_HANDOFF_ACTIVE_SURFACE_NO_DOUBLE_TOGGLE";
                LastDiagnosticDetail =
                    "The accepted Battle surface is already active. The integration "
                    + "will not invoke the Button again; exit Play Mode for a safe reset.";
                return;
            }

            ItemSystemBattleSandboxBoardAdapter[] itemAdapters =
                Resources.FindObjectsOfTypeAll<
                        ItemSystemBattleSandboxBoardAdapter>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            string readinessDiagnostic = string.Empty;
            bool hasSingleAuthority = itemAdapters.Length == 1
                && itemAdapters[0].Authority != null;
            bool readinessAccepted = hasSingleAuthority
                && V04Chapter1BossPhase1CompletionAdapter
                    .TryValidateBossReadiness(
                        itemAdapters[0].Authority,
                        out readinessDiagnostic);
            if (!readinessAccepted)
            {
                LastDiagnosticCode =
                    "BOSS_HANDOFF_RETRY_ITEM_CHAIN_NOT_READY";
                LastDiagnosticDetail = hasSingleAuthority
                    ? readinessDiagnostic.Replace(
                        "ChapterFlow remains at BossGate",
                        "the existing Boss request remains unstarted")
                    : "Retry requires exactly one current Item authority; "
                        + "the existing Boss request remains unstarted.";
                return;
            }
            bossHandoffElapsed = 0f;
            bossHandoffTimedOut = false;
            InvokeAcceptedAuthoredBossButtonOnce();
        }

        private void InvokeAcceptedAuthoredBossButtonOnce()
        {
            if (bossRuntime?.SceneBinder?.AuthoredResetButton == null)
            {
                bossHandoffTimedOut = true;
                LastDiagnosticCode = "BOSS_AUTHORED_BUTTON_MISSING";
                LastDiagnosticDetail =
                    "The accepted authored Button is unavailable; no direct Runtime "
                    + "fallback is allowed.";
                return;
            }
            bossRuntime.SceneBinder.AuthoredResetButton.onClick.Invoke();
            LastDiagnosticCode =
                "PHASE1_AUTHORED_BATTLE_SURFACE_HANDOFF_PENDING";
            LastDiagnosticDetail =
                "Invoked the accepted authored Button exactly once; waiting for "
                + "the grid to transition toward Battle and the Runtime session. "
                + string.Join(
                    " / ",
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TemporaryClearLabels);
        }

        private void ResetRuntimeAdapters()
        {
            ResolveAuthoredSceneBinder();
            authoredSceneBinder?.ReleaseOrdinaryAdmission();
            ClearOrdinaryStageTransition();
            normalBattle = null;
            bossRuntime = null;
            pendingBossRequest = null;
            bossStartConfirmed = false;
            bossHandoffElapsed = 0f;
            bossHandoffTimedOut = false;
            normalElapsedTick = 0L;
            normalBattlePausedByPrepareSurface = false;
            normalBattleResumeFramePending = false;
            resultSequence = 0L;
        }

        public bool TryAdvanceOrdinaryStageTransitionPhase(
            long sequence,
            C1OrdinaryStageTransitionPhase phase)
        {
            if (!ordinaryStageTransition.HasSequence
                || sequence != ordinaryStageTransition.Sequence
                || phase == C1OrdinaryStageTransitionPhase.None
                || (int)phase < (int)ordinaryStageTransition.Phase)
            {
                return false;
            }
            ordinaryStageTransition = new C1OrdinaryStageTransitionSnapshot(
                sequence,
                phase,
                Snapshot?.currentStageId
                    ?? ordinaryStageTransition.CurrentStageId,
                ordinaryStageTransition.NextStageId);
            LastDiagnosticCode =
                "ORDINARY_TRANSITION_" + phase.ToString().ToUpperInvariant();
            LastDiagnosticDetail =
                ordinaryStageTransition.CurrentStageId + " -> "
                + ordinaryStageTransition.NextStageId;
            return true;
        }

        public bool TryCommitOrdinaryStageSwitch(long sequence)
        {
            if (!ordinaryStageTransition.HasSequence
                || sequence != ordinaryStageTransition.Sequence)
            {
                return false;
            }
            if (ordinaryStageSwitchApplied)
            {
                return true;
            }
            if (!AdvanceOrdinaryVictoryBoundaryToNextStage())
            {
                return false;
            }
            ordinaryStageSwitchApplied = true;
            ordinaryStageTransition = new C1OrdinaryStageTransitionSnapshot(
                sequence,
                C1OrdinaryStageTransitionPhase.StageSwitchWhileBlack,
                Snapshot?.currentStageId
                    ?? ordinaryStageTransition.NextStageId,
                ordinaryStageTransition.NextStageId);
            LastDiagnosticCode = "ORDINARY_TRANSITION_STAGE_SWITCHED";
            LastDiagnosticDetail =
                ordinaryStageTransition.CurrentStageId;
            return true;
        }

        public bool TryCompleteOrdinaryStageTransition(long sequence)
        {
            if (!ordinaryStageTransition.HasSequence
                || sequence != ordinaryStageTransition.Sequence)
            {
                return false;
            }
            ordinaryTransitionCompletionApplied = true;
            ordinaryStageTransition = new C1OrdinaryStageTransitionSnapshot(
                sequence,
                C1OrdinaryStageTransitionPhase.NextBattleReady,
                Snapshot?.currentStageId
                    ?? ordinaryStageTransition.CurrentStageId,
                ordinaryStageTransition.NextStageId);
            if (Snapshot?.phase == V04ChapterFlowPhase.Prepare
                && !string.Equals(
                    Snapshot.currentStageId,
                    "1-10",
                    StringComparison.Ordinal))
            {
                StartCurrentNormalStage();
                return normalBattle?.IsRunning == true;
            }
            LastDiagnosticCode = Snapshot?.phase == V04ChapterFlowPhase.BossGate
                ? "ORDINARY_TRANSITION_BOSS_GATE_READY"
                : "ORDINARY_TRANSITION_READY";
            LastDiagnosticDetail =
                ordinaryStageTransition.CurrentStageId + " is ready.";
            return true;
        }

        private void UpdateOrdinaryTransitionRunningPhase()
        {
            if (!ordinaryTransitionCompletionApplied
                || !ordinaryStageTransition.HasSequence
                || ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.NextBattleReady
                || normalBattle?.IsRunning != true
                || Snapshot?.phase != V04ChapterFlowPhase.BattleRunning)
            {
                return;
            }
            ordinaryStageTransition = new C1OrdinaryStageTransitionSnapshot(
                ordinaryStageTransition.Sequence,
                C1OrdinaryStageTransitionPhase.BattleRunning,
                Snapshot.currentStageId,
                ordinaryStageTransition.NextStageId);
        }

        private bool TryQueueOrdinaryStageTransition()
        {
            if (ordinaryStageTransition.HasSequence
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.None
                && ordinaryStageTransition.Phase
                    != C1OrdinaryStageTransitionPhase.BattleRunning)
            {
                return true;
            }
            if (!TryResolveNextStageId(
                    Snapshot?.currentStageId,
                    out string nextStageId))
            {
                return false;
            }
            ordinaryStageTransition = new C1OrdinaryStageTransitionSnapshot(
                ++ordinaryTransitionSequence,
                C1OrdinaryStageTransitionPhase.VictoryAccepted,
                Snapshot?.currentStageId ?? string.Empty,
                nextStageId);
            ordinaryStageSwitchApplied = false;
            ordinaryTransitionCompletionApplied = false;
            return true;
        }

        private bool AdvanceOrdinaryVictoryBoundaryToNextStage()
        {
            if (Snapshot == null)
            {
                LastDiagnosticCode = "FLOW_NOT_READY";
                LastDiagnosticDetail =
                    "Accepted ChapterFlow reducer rejected the requested transition.";
                return false;
            }
            if (Snapshot.phase == V04ChapterFlowPhase.Settlement
                && !ContinueBoundaryOnce())
            {
                return false;
            }
            if (Snapshot.phase == V04ChapterFlowPhase.DropCandidatePreview
                && !ContinueBoundaryOnce())
            {
                return false;
            }
            return true;
        }

        private bool ContinueBoundaryOnce()
        {
            if (!flow.ContinueFromCurrentBoundary())
            {
                SetFlowDiagnostic();
                return false;
            }
            normalBattle = null;
            PublishAdmissionForCurrentFlow();
            return true;
        }

        private void ClearOrdinaryStageTransition()
        {
            ordinaryStageTransition =
                C1OrdinaryStageTransitionSnapshot.None;
            ordinaryStageSwitchApplied = false;
            ordinaryTransitionCompletionApplied = false;
        }

        private static bool TryResolveNextStageId(
            string currentStageId,
            out string nextStageId)
        {
            nextStageId = string.Empty;
            if (string.IsNullOrWhiteSpace(currentStageId)
                || !currentStageId.StartsWith("1-",
                    StringComparison.Ordinal))
            {
                return false;
            }
            if (!int.TryParse(
                    currentStageId.Substring(2),
                    out int stageNumber)
                || stageNumber < 1
                || stageNumber >= 10)
            {
                return false;
            }
            nextStageId = "1-" + (stageNumber + 1);
            return true;
        }

        private void PublishAdmissionForCurrentFlow()
        {
            string stageId = Snapshot?.currentStageId ?? string.Empty;
            if (string.IsNullOrWhiteSpace(stageId)
                || string.Equals(stageId, "1-10", StringComparison.Ordinal)
                || Snapshot.phase == V04ChapterFlowPhase.Disabled
                || Snapshot.phase == V04ChapterFlowPhase.ChapterComplete)
            {
                PublishNoAdmission(stageId);
                return;
            }

            V04Chapter1WavePlanDefinition wave =
                flow?.CurrentWavePlan;
            if (wave == null
                || !string.Equals(
                    wave.stageId,
                    stageId,
                    StringComparison.Ordinal)
                || wave.isBoss
                || !wave.HasAllRuntimeProfiles
                || normalBattle == null
                || normalBattle.ActiveActors.Count == 0)
            {
                PublishNoAdmission(stageId);
                if (wave?.HasHeldRuntime == true)
                {
                    LastDiagnosticCode =
                        V04Chapter1StageWaveEncounterTruth
                            .RuntimeProfileMissingHeldByBad3;
                    LastDiagnosticDetail =
                        wave.waveId
                        + " remains held as one whole Wave; "
                        + "activeActors=0; admission=NONE.";
                }
                return;
            }

            V04Chapter1ActorPlanDefinition primaryActor =
                wave.actors.OrderBy(value => value.slotOrdinal)
                    .First();
            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.GetProfiles().FirstOrDefault(
                    value => string.Equals(
                            value.ContentId,
                            primaryActor.enemyContentId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            value.RuntimeProfileId,
                            primaryActor.runtimeProfileId,
                            StringComparison.Ordinal)
                        && string.Equals(
                            value.PresentationKey,
                            primaryActor.visualProfileKey,
                            StringComparison.Ordinal));
            if (profile == null)
            {
                PublishNoAdmission(stageId);
                LastDiagnosticCode =
                    "ORDINARY_ENCOUNTER_ADMISSION_BINDING_MISSING";
                LastDiagnosticDetail =
                    "ChapterFlow could not publish a deterministic ordinary "
                    + "encounter admission for " + stageId + ".";
                return;
            }

            encounterAdmission = new C1EncounterAdmissionSnapshot(
                stageId,
                C1EncounterAdmissionKind.Ordinary,
                primaryActor.enemyContentId,
                primaryActor.runtimeProfileId,
                primaryActor.visualProfileKey,
                false,
                flow?.ActiveRequest?.requestId ?? string.Empty,
                primaryActor.waveEntryId,
                wave.waveOrdinal,
                primaryActor.occurrenceOrdinal,
                flow.CurrentWaveResetGeneration,
                wave.waveId,
                normalBattle.AdmissionActors);
            ApplyBossVisualAdmission(false);
            ResolveAuthoredSceneBinder();
            authoredSceneBinder?.SetOrdinaryAdmissionPreview(profile);
        }

        private void PublishAcceptedBossAdmission(
            BattleStartRequest request)
        {
            V04Chapter1WaveEntryDefinition bossWave =
                V04Chapter1StageWaveEncounterTruth
                    .FindPlan("1-10")?.waves.FirstOrDefault()
                    ?.actors.FirstOrDefault();
            encounterAdmission = new C1EncounterAdmissionSnapshot(
                Snapshot?.currentStageId ?? "1-10",
                C1EncounterAdmissionKind.Boss,
                bossWave?.enemyContentId ?? string.Empty,
                request?.bossProfileId ?? string.Empty,
                "enemy.bone_aspect.c1.bone_guard.phase1",
                true,
                request?.requestId ?? string.Empty,
                bossWave?.waveEntryId ?? string.Empty,
                bossWave?.waveOrdinal ?? 0,
                bossWave?.occurrenceOrdinal ?? 0,
                0,
                bossWave?.waveId ?? string.Empty);
            ResolveAuthoredSceneBinder();
            authoredSceneBinder?.ReleaseOrdinaryAdmission();
            ApplyBossVisualAdmission(true);
        }

        private void PublishNoAdmission(string stageId = "")
        {
            encounterAdmission = C1EncounterAdmissionSnapshot.None(stageId);
            ResolveAuthoredSceneBinder();
            authoredSceneBinder?.ReleaseOrdinaryAdmission();
            if (string.Equals(stageId, "1-10", StringComparison.Ordinal)
                && Snapshot?.phase == V04ChapterFlowPhase.BossGate)
            {
                RestoreAuthoredPrepareSurfaceForBossGate();
            }
            ApplyBossVisualAdmission(false);
        }

        private void RestoreAuthoredPrepareSurfaceForBossGate()
        {
            BuildGridInteractionPreviewController grid =
                authoredSceneBinder?.GridController;
            if (grid?.IsSandboxBattleModeActive != true)
            {
                return;
            }

            Button[] candidates = grid.GetComponentsInChildren<Button>(true)
                .Where(value => value != null
                    && string.Equals(
                        value.gameObject.name,
                        "V04BattlePrepareToggleButton",
                        StringComparison.Ordinal))
                .ToArray();
            if (candidates.Length != 1)
            {
                LastDiagnosticCode =
                    "BOSS_GATE_AUTHORED_PREPARE_SURFACE_AMBIGUOUS";
                LastDiagnosticDetail =
                    "Expected exactly one existing authored prepare toggle; "
                    + "actual=" + candidates.Length + ".";
                return;
            }

            candidates[0].onClick.Invoke();
        }

        private void ResolveAuthoredSceneBinder()
        {
            if (authoredSceneBinder != null
                && authoredSceneBinder.gameObject.scene
                    == gameObject.scene)
            {
                return;
            }
            ShougunuPhase1BattleSandboxSceneBinder[] binders = Resources
                .FindObjectsOfTypeAll<
                    ShougunuPhase1BattleSandboxSceneBinder>()
                .Where(value => value != null
                    && value.gameObject.scene == gameObject.scene)
                .ToArray();
            authoredSceneBinder = binders.Length == 1
                ? binders[0]
                : null;
        }

        private void TryWireAuthoredStartSurface()
        {
            ResolveAuthoredSceneBinder();
            Button candidate =
                authoredSceneBinder?.AuthoredResetButton;
            if (authoredStartButtonWired
                && authoredStartButton == candidate
                && authoredStartButton != null)
            {
                return;
            }
            if (authoredStartButtonWired
                && authoredStartButton != null)
            {
                authoredStartButton.onClick.RemoveListener(
                    HandleAuthoredStartSurface);
            }
            authoredStartButton = candidate;
            authoredStartButtonWired = false;
            if (authoredStartButton == null)
            {
                return;
            }
            authoredStartButton.onClick.AddListener(
                HandleAuthoredStartSurface);
            authoredStartButtonWired = true;
        }

        private void HandleAuthoredStartSurface()
        {
            if (Snapshot == null
                || Snapshot.phase == V04ChapterFlowPhase.Disabled)
            {
                StartChapter1Session();
            }
            if (Snapshot?.phase == V04ChapterFlowPhase.Prepare
                && !string.Equals(
                    Snapshot.currentStageId,
                    "1-10",
                    StringComparison.Ordinal))
            {
                StartCurrentNormalStage();
            }
            else if (Snapshot?.phase == V04ChapterFlowPhase.Settlement
                || Snapshot?.phase
                    == V04ChapterFlowPhase.DropCandidatePreview)
            {
                ContinueFromSettlementOrDropCandidate();
            }
        }

        private void ApplyBossVisualAdmission(bool admitted)
        {
            ShougunuPhase1VisualPrototypeController[] visualOwners =
                Resources.FindObjectsOfTypeAll<
                        ShougunuPhase1VisualPrototypeController>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            foreach (ShougunuPhase1VisualPrototypeController visualOwner
                in visualOwners)
            {
                visualOwner.SetBossEncounterAdmission(admitted);
            }
        }

        private void OnDisable()
        {
            if (authoredStartButtonWired
                && authoredStartButton != null)
            {
                authoredStartButton.onClick.RemoveListener(
                    HandleAuthoredStartSurface);
            }
            authoredStartButtonWired = false;
            authoredStartButton = null;
            ClearOrdinaryStageTransition();
            PublishNoAdmission();
        }

        private void SetFlowDiagnostic()
        {
            LastDiagnosticCode = flow?.LastDiagnosticCode
                ?? "FLOW_NOT_READY";
            LastDiagnosticDetail =
                "Accepted ChapterFlow reducer rejected the requested transition.";
        }
    }

    internal static class V04Chapter1BattleSandboxPlayerBootstrap
    {
        private const HideFlags RuntimeHideFlags =
            HideFlags.DontSaveInEditor
            | HideFlags.DontSaveInBuild
            | HideFlags.HideInHierarchy;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoaded()
        {
            if (Application.isEditor)
            {
                return;
            }
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallForActiveScene()
        {
            if (!Application.isEditor)
            {
                TryInstall(SceneManager.GetActiveScene());
            }
        }

        private static void HandleSceneLoaded(
            Scene scene,
            LoadSceneMode loadMode)
        {
            TryInstall(scene);
        }

        private static void TryInstall(Scene scene)
        {
            if (Application.isEditor
                || !scene.IsValid()
                || !scene.isLoaded
                || !string.Equals(
                    scene.name,
                    V04Chapter1ContinuousBattleIntegrationContract
                        .TargetSceneName,
                    StringComparison.Ordinal))
            {
                return;
            }

            V04Chapter1BattleSandboxRuntimeController[] controllers =
                FindSceneComponents<
                    V04Chapter1BattleSandboxRuntimeController>(scene);
            GameObject[] flowRoots = FindSceneRoots(
                scene,
                V04Chapter1ContinuousBattleIntegrationContract
                    .InjectedRootName);
            if (controllers.Length > 1 || flowRoots.Length > 1)
            {
                Debug.LogError(
                    "[BATTLESANDBOX_PLAYER_BOOTSTRAP_CARDINALITY_REJECT] "
                    + "flowControllers="
                    + controllers.Length
                    + " flowRoots="
                    + flowRoots.Length);
                return;
            }

            GameObject flowRoot;
            V04Chapter1BattleSandboxRuntimeController controller;
            if (controllers.Length == 1)
            {
                controller = controllers[0];
                flowRoot = controller.gameObject;
                if (flowRoots.Length == 1
                    && flowRoots[0] != flowRoot)
                {
                    Debug.LogError(
                        "[BATTLESANDBOX_PLAYER_BOOTSTRAP_FLOW_ROOT_REJECT] "
                        + "The unique flow controller is not hosted by the "
                        + "unique accepted runtime root.");
                    return;
                }
            }
            else if (flowRoots.Length == 1)
            {
                flowRoot = flowRoots[0];
                controller = flowRoot.GetComponent<
                    V04Chapter1BattleSandboxRuntimeController>();
                if (controller == null)
                {
                    Debug.LogError(
                        "[BATTLESANDBOX_PLAYER_BOOTSTRAP_FLOW_ROOT_MALFORMED] "
                        + "Accepted runtime root exists without its flow "
                        + "controller.");
                    return;
                }
            }
            else
            {
                flowRoot = new GameObject(
                    V04Chapter1ContinuousBattleIntegrationContract
                        .InjectedRootName);
                flowRoot.hideFlags = RuntimeHideFlags;
                SceneManager.MoveGameObjectToScene(flowRoot, scene);
                controller = flowRoot.AddComponent<
                    V04Chapter1BattleSandboxRuntimeController>();
            }
            flowRoot.hideFlags = RuntimeHideFlags;
            controller.hideFlags = RuntimeHideFlags;

            GameObject[] presentationRoots = FindSceneRoots(
                scene,
                C1JourneyPresentationProfile.RuntimeRootName);
            C1JourneyPresentationController[] presentations =
                FindSceneComponents<C1JourneyPresentationController>(
                    scene);
            C1BattleSandboxAuthoredEnemyPresentationAdapter[] adapters =
                FindSceneComponents<
                    C1BattleSandboxAuthoredEnemyPresentationAdapter>(
                    scene);
            if (presentationRoots.Length > 1
                || presentations.Length > 1
                || adapters.Length > 1)
            {
                Debug.LogError(
                    "[BATTLESANDBOX_PLAYER_BOOTSTRAP_PRESENTATION_CARDINALITY_REJECT] "
                    + "presentationRoots="
                    + presentationRoots.Length
                    + " presentations="
                    + presentations.Length
                    + " enemyAdapters="
                    + adapters.Length);
                return;
            }

            GameObject presentationRoot;
            if (presentationRoots.Length == 1)
            {
                presentationRoot = presentationRoots[0];
            }
            else if (presentations.Length == 1)
            {
                presentationRoot = presentations[0].gameObject;
            }
            else if (adapters.Length == 1)
            {
                presentationRoot = adapters[0].gameObject;
            }
            else
            {
                presentationRoot = new GameObject(
                    C1JourneyPresentationProfile.RuntimeRootName);
                presentationRoot.hideFlags = RuntimeHideFlags;
                SceneManager.MoveGameObjectToScene(
                    presentationRoot,
                    scene);
            }

            if ((presentationRoots.Length == 1
                    && presentationRoots[0] != presentationRoot)
                || (presentations.Length == 1
                    && presentations[0].gameObject != presentationRoot)
                || (adapters.Length == 1
                    && adapters[0].gameObject != presentationRoot))
            {
                Debug.LogError(
                    "[BATTLESANDBOX_PLAYER_BOOTSTRAP_PRESENTATION_ROOT_REJECT] "
                    + "Presentation components do not share the unique "
                    + "accepted runtime root.");
                return;
            }

            presentationRoot.hideFlags = RuntimeHideFlags;
            C1JourneyPresentationController presentation =
                presentations.Length == 1
                    ? presentations[0]
                    : presentationRoot.AddComponent<
                        C1JourneyPresentationController>();
            C1BattleSandboxAuthoredEnemyPresentationAdapter adapter =
                adapters.Length == 1
                    ? adapters[0]
                    : presentationRoot.AddComponent<
                        C1BattleSandboxAuthoredEnemyPresentationAdapter>();
            presentation.hideFlags = RuntimeHideFlags;
            adapter.hideFlags = RuntimeHideFlags;
            presentation.Bind(controller);
            adapter.Bind(controller);

            Debug.Log(
                "[BATTLESANDBOX_PLAYER_RUNTIME_READY] flow="
                + controller.GetType().Name
                + " presentation="
                + presentation.GetType().Name
                + " enemyAdapter="
                + adapter.GetType().Name);
        }

        private static T[] FindSceneComponents<T>(Scene scene)
            where T : Component
        {
            return Resources.FindObjectsOfTypeAll<T>()
                .Where(value => value != null
                    && value.gameObject.scene == scene)
                .ToArray();
        }

        private static GameObject[] FindSceneRoots(
            Scene scene,
            string rootName)
        {
            return scene.GetRootGameObjects()
                .Where(value => value != null
                    && string.Equals(
                        value.name,
                        rootName,
                        StringComparison.Ordinal))
                .ToArray();
        }
    }
}
