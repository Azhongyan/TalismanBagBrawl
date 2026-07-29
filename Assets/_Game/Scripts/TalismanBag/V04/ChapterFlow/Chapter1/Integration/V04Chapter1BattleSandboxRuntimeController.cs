using System;
using System.Linq;
using TalismanBag.BattleBridge.ShougunuPhase1;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using UnityEngine;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Integration
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(720)]
    public sealed class V04Chapter1BattleSandboxRuntimeController :
        MonoBehaviour
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

        private void Awake()
        {
            flow = new V04Chapter1ContinuousFlowSession();
        }

        private void Update()
        {
            UpdateNormalBattle();
            UpdateBossBattle();
        }

        public void StartChapter1Session()
        {
            ResetRuntimeAdapters();
            resetGeneration++;
            if (flow.StartChapter1())
            {
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
            if (Snapshot?.phase != V04ChapterFlowPhase.Prepare
                || Snapshot.currentStageId == "1-10")
            {
                LastDiagnosticCode = "NORMAL_STAGE_NOT_AVAILABLE";
                LastDiagnosticDetail =
                    "Normal Battle starts only from Prepare for 1-1..1-9.";
                return;
            }

            V04Chapter1EncounterBindingDefinition binding =
                V04Chapter1EncounterManifest.Bindings.FirstOrDefault(
                    value => string.Equals(
                        value.stageId,
                        Snapshot.currentStageId,
                        StringComparison.Ordinal));
            C1EnemyRuntimeProfileSnapshot profile =
                C1EnemyRuntimeCatalog.GetProfiles().SingleOrDefault(
                    value => string.Equals(
                        value.ContentId,
                        binding?.activeEnemyContentId,
                        StringComparison.Ordinal));
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
                return;
            }

            if (!V04Chapter1NormalEnemyBattleAdapter.TryPrepare(
                    adapters[0].Authority,
                    profile,
                    ++resetGeneration,
                    out V04Chapter1NormalEnemyBattleAdapter prepared,
                    out string diagnostic))
            {
                LastDiagnosticCode = diagnostic;
                LastDiagnosticDetail =
                    "Real Item request / C1 target preparation rejected; ChapterFlow did not advance.";
                return;
            }

            BattleStartRequest request = flow.RequestCurrentBattle(
                profile.RuntimeProfileId,
                V04Chapter1ContinuousFlowSession.StableId(
                    "seed",
                    resetGeneration));
            if (request == null
                || !prepared.BindAcceptedBattleRequest(request)
                || !flow.ConfirmBattleStarted(request))
            {
                LastDiagnosticCode = "NORMAL_BATTLE_START_REJECTED";
                LastDiagnosticDetail =
                    flow.LastDiagnosticCode + " / "
                    + prepared.LastDiagnosticCode;
                return;
            }

            normalBattle = prepared;
            normalElapsedTick = 0L;
            LastDiagnosticCode = "NORMAL_BATTLE_RUNNING";
            LastDiagnosticDetail =
                V04Chapter1ContinuousBattleIntegrationContract.CadenceLabels
                + " / "
                + V04Chapter1ContinuousBattleIntegrationContract
                    .ResolverLabels;
        }

        public void ContinueFromSettlementOrDropCandidate()
        {
            if (!flow.ContinueFromCurrentBoundary())
            {
                SetFlowDiagnostic();
                return;
            }
            normalBattle = null;
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
            normalElapsedTick += Math.Max(
                1L,
                (long)Math.Floor(Time.unscaledDeltaTime * 1000f));
            normalBattle.AdvanceTo(normalElapsedTick);
            if (normalBattle.IsDefeated)
            {
                BattleResultSnapshot result =
                    normalBattle.CreateTerminalResult(++resultSequence);
                if (result != null && flow.AcceptBattleResult(result))
                {
                    LastDiagnosticCode = "NORMAL_SETTLEMENT_READY";
                    LastDiagnosticDetail =
                        "Terminal Enemy lifecycle accepted. Settlement is visible; reward/drop grants remain zero.";
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
            normalBattle = null;
            bossRuntime = null;
            pendingBossRequest = null;
            bossStartConfirmed = false;
            bossHandoffElapsed = 0f;
            bossHandoffTimedOut = false;
            normalElapsedTick = 0L;
            resultSequence = 0L;
        }

        private void SetFlowDiagnostic()
        {
            LastDiagnosticCode = flow?.LastDiagnosticCode
                ?? "FLOW_NOT_READY";
            LastDiagnosticDetail =
                "Accepted ChapterFlow reducer rejected the requested transition.";
        }
    }
}
