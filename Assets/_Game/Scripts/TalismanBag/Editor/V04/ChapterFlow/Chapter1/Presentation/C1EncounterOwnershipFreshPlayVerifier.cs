using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.Editor.V04.ChapterFlow.Chapter1.Integration;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.V04.ChapterFlow;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.Editor.V04.ChapterFlow.Chapter1.Presentation
{
    [InitializeOnLoad]
    public static class C1EncounterOwnershipFreshPlayVerifier
    {
        private const string PackageId =
            "V0.4-C1BattleSandboxEncounterOwnershipAndVisualSlotHandoffFix01";
        private const string Key = PackageId + ".FreshPlay.";

        private const int WaitRuntime = 10;
        private const int WaitBattle = 20;
        private const int ObserveBattle = 30;
        private const int WaitPauseFreeze = 31;
        private const int WaitResumeSurface = 32;
        private const int WaitResumeProgress = 33;
        private const int WaitPresentation = 34;
        private const int WaitReset = 35;
        private const int AdvanceSettlement = 40;
        private const int AdvanceDrop = 41;
        private const int WaitBoundary = 42;
        private const int WaitBossGate = 50;
        private const int WaitGenericBossRejection = 51;
        private const int WaitBossStart = 60;
        private const int Exiting = 90;

        private static double nextPoll;
        private static string RequestPath => Path.Combine(
            ProjectTempDirectory,
            "C1EncounterOwnershipFreshPlay.request");
        private static string EvidencePath => Path.Combine(
            ProjectTempDirectory,
            "C1EncounterOwnershipFreshPlay.evidence.txt");
        private static string ProjectTempDirectory => Path.GetFullPath(
            Path.Combine(
                Application.dataPath,
                "..",
                "Temp"));

        static C1EncounterOwnershipFreshPlayVerifier()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            EditorApplication.delayCall += TryStartRequestedRun;
        }
        public static void RequestFreshPlay()
        {
            Directory.CreateDirectory("Temp");
            File.WriteAllText(
                RequestPath,
                DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                Encoding.UTF8);
            TryStartRequestedRun();
        }

        public static void RequestFreshPlayBatch()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !string.Equals(
                    scene.path,
                    C1JourneyPresentationProfile.TargetScenePath,
                    StringComparison.Ordinal))
            {
                if (scene.IsValid() && scene.isDirty)
                {
                    FinishWithoutPlay(
                        "BATCH_ACTIVE_SCENE_DIRTY");
                    return;
                }
                EditorSceneManager.OpenScene(
                    C1JourneyPresentationProfile.TargetScenePath,
                    OpenSceneMode.Single);
            }
            RequestFreshPlay();
        }

        private static void TryStartRequestedRun()
        {
            if (!File.Exists(RequestPath)
                || GetInt("State") != 0
                || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            if (EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryStartRequestedRun;
                return;
            }
            Scene scene = SceneManager.GetActiveScene();
            C1EncounterOwnershipFreshPlayContext context =
                C1EncounterOwnershipFreshPlayProbe.Resolve(scene);
            bool targetSceneReady = scene.IsValid()
                && string.Equals(
                    scene.path,
                    C1JourneyPresentationProfile.TargetScenePath,
                    StringComparison.Ordinal)
                && !scene.isDirty
                && context.AuthoredSlot != null;
            if (!targetSceneReady)
            {
                if (scene.IsValid() && scene.isDirty)
                {
                    FinishWithoutPlay(
                        "EDIT_ENTRY_REJECTED:scene="
                        + scene.path
                        + ",dirty=true,slot="
                        + (context.AuthoredSlot != null));
                    return;
                }
                EditorSceneManager.OpenScene(
                    C1JourneyPresentationProfile.TargetScenePath,
                    OpenSceneMode.Single);
                scene = SceneManager.GetActiveScene();
                context = C1EncounterOwnershipFreshPlayProbe.Resolve(scene);
                if (!scene.IsValid()
                    || !string.Equals(
                        scene.path,
                        C1JourneyPresentationProfile.TargetScenePath,
                        StringComparison.Ordinal)
                    || scene.isDirty
                    || context.AuthoredSlot == null)
                {
                    FinishWithoutPlay(
                        "EDIT_ENTRY_REJECTED:scene="
                        + (scene.IsValid() ? scene.path : "INVALID")
                        + ",dirty=" + (scene.IsValid() && scene.isDirty)
                        + ",slot=" + (context.AuthoredSlot != null));
                    return;
                }
            }

            SetString(
                "OriginalSlot",
                C1EncounterOwnershipFreshPlayProbe.Fingerprint(
                    context.AuthoredSlot));
            SetBool(
                "OriginalContinuousPref",
                V04Chapter1ContinuousHandtestMenu.IsEnabled);
            SetBool(
                "OriginalJourneyPref",
                C1ThreeSceneJourneyHandtestMenu.IsEnabled);
            EditorPrefs.SetBool(
                V04Chapter1ContinuousBattleIntegrationContract
                    .EditorPreferenceKey,
                true);
            EditorPrefs.SetBool(
                C1JourneyPresentationProfile.EditorPreferenceKey,
                true);
            SetInt("Cycle", 0);
            SetInt("StageIndex", 0);
            SetInt("TotalPublished", 0);
            SetInt("VisualMask", 0);
            SetInt("FirstStageVisualMask", 0);
            SetInt("StageMask", 0);
            SetBool("HpChanged", false);
            SetBool("ShellChanged", false);
            SetBool("PauseTestPassed", false);
            SetString("Failure", string.Empty);
            SetString(
                "StartedTicks",
                DateTime.UtcNow.Ticks.ToString(
                    CultureInfo.InvariantCulture));
            SetState(WaitRuntime);
            Debug.Log(
                "[" + PackageId
                + "] FRESH_PLAY_REQUEST_ACCEPTED "
                + "authoredStart=V04BattlePrepareStateButton");
            EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            int state = GetInt("State");
            if (change == PlayModeStateChange.EnteredPlayMode
                && state > 0 && state < Exiting)
            {
                SetState(WaitRuntime);
            }
            else if (change == PlayModeStateChange.EnteredEditMode
                && state == Exiting)
            {
                EditorApplication.delayCall += FinalizeAfterPlay;
            }
        }

        private static void Poll()
        {
            int state = GetInt("State");
            if (state > 0
                && state < Exiting
                && !EditorApplication.isPlaying
                && File.Exists(RequestPath))
            {
                ClearSession();
                state = 0;
            }
            if (state == 0
                && !EditorApplication.isPlaying
                && File.Exists(RequestPath))
            {
                TryStartRequestedRun();
                state = GetInt("State");
            }
            if (state <= 0
                || state == Exiting
                || !EditorApplication.isPlaying
                || EditorApplication.timeSinceStartup < nextPoll)
            {
                return;
            }
            nextPoll = EditorApplication.timeSinceStartup + 0.05d;
            if (SecondsSince("StartedTicks") > 180d)
            {
                Fail("TIMEOUT_180S");
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !string.Equals(
                    scene.path,
                    C1JourneyPresentationProfile.TargetScenePath,
                    StringComparison.Ordinal)
                || scene.isDirty)
            {
                Fail("PLAY_SCENE_INVALID_OR_DIRTY");
                return;
            }
            C1EncounterOwnershipFreshPlayContext context =
                C1EncounterOwnershipFreshPlayProbe.Resolve(scene);

            if (state == WaitRuntime)
            {
                if (!context.Ready)
                {
                    TimeoutState(15d, "RUNTIME_BINDING_TIMEOUT");
                    return;
                }
                if (!string.Equals(
                    context.Flow.AuthoredStartSurfaceName,
                    "V04BattlePrepareStateButton",
                    StringComparison.Ordinal)
                    || context.P3.HasActiveSession)
                {
                    Fail("AUTHORED_START_OR_PRESTART_P3_INVALID");
                    return;
                }
                if (!C1EncounterOwnershipFreshPlayProbe
                    .CommitOfficialFixture(
                        context.ItemAdapter.Authority,
                        out string fixtureFailure))
                {
                    Fail(fixtureFailure);
                    return;
                }
                if (!C1EncounterOwnershipFreshPlayProbe
                    .ValidateAuthoritativeFirstEncounterTuning(
                        context.ItemAdapter.Authority,
                        out string tuningFailure))
                {
                    Fail(tuningFailure);
                    return;
                }
                context.Binder.AuthoredResetButton.onClick.Invoke();
                SetState(WaitBattle);
                return;
            }

            if (!context.Ready)
            {
                Fail("RUNTIME_DISAPPEARED");
                return;
            }
            SetInt(
                "VisualMask",
                GetInt("VisualMask")
                | C1EncounterOwnershipFreshPlayProbe.VisualStateBit(
                    context.Journey.CurrentState));
            if (GetInt("Cycle") == 0 && GetInt("StageIndex") == 0)
            {
                SetInt(
                    "FirstStageVisualMask",
                    GetInt("FirstStageVisualMask")
                    | C1EncounterOwnershipFreshPlayProbe.VisualStateBit(
                        context.Journey.CurrentState));
            }
            if (context.P3.HasActiveSession && state < WaitBossStart)
            {
                Fail("P3_STARTED_OUTSIDE_ACCEPTED_BOSS_ADMISSION");
                return;
            }

            switch (state)
            {
                case WaitBattle:
                    PollWaitBattle(context);
                    break;
                case ObserveBattle:
                    PollObserveBattle(context);
                    break;
                case WaitPauseFreeze:
                    PollWaitPauseFreeze(context);
                    break;
                case WaitResumeSurface:
                    PollWaitResumeSurface(context);
                    break;
                case WaitResumeProgress:
                    PollWaitResumeProgress(context);
                    break;
                case WaitPresentation:
                    PollWaitPresentation(context);
                    break;
                case WaitReset:
                    PollWaitReset(context);
                    break;
                case AdvanceSettlement:
                    InvokeWhenInteractable(
                        context,
                        AdvanceDrop);
                    break;
                case AdvanceDrop:
                    PollAdvanceDrop(context);
                    break;
                case WaitBoundary:
                    PollBoundary(context);
                    break;
                case WaitBossGate:
                    PollBossGate(context);
                    break;
                case WaitGenericBossRejection:
                    PollGenericBossRejection(context);
                    break;
                case WaitBossStart:
                    PollBossStart(context);
                    break;
            }
        }

        private static void PollWaitBattle(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.Flow.Snapshot?.phase
                    != V04ChapterFlowPhase.BattleRunning
                || context.Flow.NormalBattle?.IsRunning != true)
            {
                TimeoutState(
                    8d,
                    "AUTHORED_START_DID_NOT_START_ORDINARY:"
                    + context.Flow.LastDiagnosticCode);
                return;
            }
            if (!C1EncounterOwnershipFreshPlayProbe.ValidateRunningStage(
                context,
                GetInt("StageIndex"),
                out string failure))
            {
                Fail(failure);
                return;
            }
            int generation =
                context.Flow.NormalBattle.Enemy.ResetGeneration;
            if (GetInt("Cycle") == 0)
            {
                SetInt("FirstGeneration", generation);
            }
            else if (generation == GetInt("FirstGeneration"))
            {
                Fail("RESET_REUSED_ORDINARY_GENERATION");
                return;
            }
            SetState(ObserveBattle);
        }

        private static void PollObserveBattle(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.Flow.NormalBattle?.Enemy != null)
            {
                var enemy = context.Flow.NormalBattle.Enemy;
                V04Chapter1NormalEnemyBattleAdapter battle =
                    context.Flow.NormalBattle;
                string hp = context.Binder.EnemyHpText?.text
                    ?? string.Empty;
                string shell = context.Binder.ShellText?.text
                    ?? string.Empty;
                if (enemy.CurrentHp < enemy.MaxHp
                    && hp.Contains(
                        enemy.CurrentHp.ToString(
                            CultureInfo.InvariantCulture)))
                {
                    SetBool("HpChanged", true);
                }
                if (enemy.ShellMax > 0
                    && enemy.CurrentShell < enemy.ShellMax
                    && shell.Contains(
                        enemy.CurrentShell.ToString(
                            CultureInfo.InvariantCulture)))
                {
                    SetBool("ShellChanged", true);
                }
                if (GetInt("Cycle") == 0
                    && GetInt("StageIndex") == 0
                    && !GetBool("PauseTestPassed")
                    && battle.CurrentTick >= 9000L
                    && battle.AcceptedItemApplicationCount >= 6
                    && battle.OutgoingRequestsResolved >= 1)
                {
                    Button prepareToggle = FindGridButton(
                        context,
                        "V04BattlePrepareToggleButton");
                    if (prepareToggle == null
                        || !prepareToggle.interactable)
                    {
                        Fail("PREPARE_TOGGLE_NOT_INTERACTABLE");
                        return;
                    }
                    prepareToggle.onClick.Invoke();
                    CapturePauseSnapshot(context);
                    SetState(WaitPauseFreeze);
                    return;
                }
            }
            if (context.Flow.Snapshot?.phase
                == V04ChapterFlowPhase.Settlement)
            {
                SetState(WaitPresentation);
            }
            else
            {
                TimeoutState(15d, "ORDINARY_DID_NOT_SETTLE");
            }
        }

        private static void PollWaitPresentation(
            C1EncounterOwnershipFreshPlayContext context)
        {
            V04Chapter1NormalEnemyBattleAdapter battle =
                context.Flow.NormalBattle;
            if (battle == null || !battle.IsDefeated)
            {
                Fail("SETTLEMENT_WITHOUT_DEFEATED_ENEMY");
                return;
            }
            int expected =
                GetInt("TotalPublished")
                + battle.PublishedPresentationCount;
            bool caughtUp =
                battle.PublishedPresentationCount > 0
                && context.LiHuo.AcceptedPresentationCount >= expected
                && context.Outline.RealAcceptedTriggerCount >= expected
                && context.Binder.AuthoredTmpPresentation
                    ?.AcceptedRoleCount
                    >= battle.PublishedPresentationCount * 2;
            if (!caughtUp)
            {
                TimeoutState(
                    6d,
                    "PRESENTATION_NOT_CAUGHT_UP:pub="
                    + battle.PublishedPresentationCount
                    + ",liHuo="
                    + context.LiHuo.AcceptedPresentationCount
                    + ",outline="
                    + context.Outline.RealAcceptedTriggerCount);
                return;
            }
            int stageIndex = GetInt("StageIndex");
            int requiredVisualMask = 0x1F;
            int observedVisualMask = GetInt("Cycle") == 0
                    && stageIndex == 0
                ? GetInt("FirstStageVisualMask")
                : GetInt("VisualMask");
            if ((observedVisualMask & requiredVisualMask)
                != requiredVisualMask)
            {
                TimeoutState(
                    6d,
                    "VISUAL_STATE_PLAYBACK_NOT_OBSERVED:mask=0x"
                    + observedVisualMask.ToString(
                        "X",
                        CultureInfo.InvariantCulture));
                return;
            }
            if (context.LiHuo.CueGateAcceptedCount
                    != context.LiHuo.AcceptedPresentationCount
                || context.Outline.EventGateAcceptedCount
                    != context.Outline.RealAcceptedTriggerCount
                || context.Binder.AuthoredTmpPresentation
                    .RejectedDuplicateCount != 0)
            {
                Fail("DUPLICATE_PRESENTATION_EVENT");
                return;
            }
            SetInt("TotalPublished", expected);
            int index = stageIndex;
            SetInt("StageMask", GetInt("StageMask") | (1 << index));
            if (GetInt("Cycle") == 0 && index == 0)
            {
                if (!GetBool("PauseTestPassed"))
                {
                    Fail("PREPARE_PAUSE_TEST_NOT_EXECUTED");
                    return;
                }
                if (!ValidateFirstEncounterTerminalFacts(
                        battle,
                        out string firstEncounterFailure))
                {
                    Fail(firstEncounterFailure);
                    return;
                }
                CaptureFirstEncounterFacts(battle);
            }

            if (GetInt("Cycle") == 0 && index == 0)
            {
                SetInt(
                    "ResetLiHuo",
                    context.LiHuo.AcceptedPresentationCount);
                SetInt(
                    "ResetOutline",
                    context.Outline.RealAcceptedTriggerCount);
                context.Flow.ResetDevSession();
                SetInt("Cycle", 1);
                SetInt("StageIndex", 0);
                SetState(WaitReset);
                return;
            }
            SetState(AdvanceSettlement);
        }

        private static void PollWaitReset(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.Flow.Snapshot?.phase
                    != V04ChapterFlowPhase.Disabled
                || context.Flow.NormalBattle != null
                || context.Binder.OrdinaryAdmissionActive
                || context.Journey.OwnsAuthoredSlot
                || context.P3.HasActiveSession)
            {
                TimeoutState(4d, "RESET_DID_NOT_RELEASE_ORDINARY");
                return;
            }
            if (context.LiHuo.AcceptedPresentationCount
                    != GetInt("ResetLiHuo")
                || context.Outline.RealAcceptedTriggerCount
                    != GetInt("ResetOutline"))
            {
                Fail("STALE_EVENT_REPLAY_AFTER_RESET");
                return;
            }
            if (SecondsInState() >= 0.75d)
            {
                context.Binder.AuthoredResetButton.onClick.Invoke();
                SetState(WaitBattle);
            }
        }

        private static void PollAdvanceDrop(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.Flow.Snapshot?.phase
                != V04ChapterFlowPhase.DropCandidatePreview)
            {
                TimeoutState(
                    4d,
                    "SETTLEMENT_DID_NOT_ADVANCE_TO_DROP");
                return;
            }
            InvokeWhenInteractable(context, WaitBoundary);
        }

        private static void PollBoundary(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.Flow.Snapshot?.phase
                == V04ChapterFlowPhase.Prepare)
            {
                int next = GetInt("StageIndex") + 1;
                string expected = "1-" + (next + 1)
                    .ToString(CultureInfo.InvariantCulture);
                if (next >= 9
                    || !string.Equals(
                        context.Flow.Snapshot.currentStageId,
                        expected,
                        StringComparison.Ordinal))
                {
                    Fail("NEXT_STAGE_MISMATCH:"
                        + context.Flow.Snapshot.currentStageId);
                    return;
                }
                SetInt("StageIndex", next);
                InvokeWhenInteractable(context, WaitBattle);
                return;
            }
            if (context.Flow.Snapshot?.phase
                    == V04ChapterFlowPhase.BossGate
                && string.Equals(
                    context.Flow.Snapshot.currentStageId,
                    "1-10",
                    StringComparison.Ordinal))
            {
                SetState(WaitBossGate);
                return;
            }
            TimeoutState(5d, "POST_DROP_BOUNDARY_INVALID");
        }

        private static void PollBossGate(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.Binder.OrdinaryAdmissionActive
                || context.Journey.OwnsAuthoredSlot
                || context.P3.HasActiveSession
                || context.P3.ResetGeneration != 0
                || context.BossVisual.BossEncounterAdmissionGranted
                || context.Flow.EncounterAdmission
                    ?.AdmittedEncounterKind
                    != C1EncounterAdmissionKind.None)
            {
                Fail("BOSS_GATE_OWNER_LEAK");
                return;
            }
            if (context.Binder.GridController
                    ?.IsSandboxBattleModeActive == true)
            {
                TimeoutState(4d, "BOSS_GATE_GRID_NOT_PREPARE");
                return;
            }
            InvokeWhenInteractable(
                context,
                WaitGenericBossRejection);
        }

        private static void PollGenericBossRejection(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.P3.HasActiveSession
                || context.P3.ResetGeneration != 0)
            {
                Fail("GENERIC_START_STARTED_P3");
                return;
            }
            if (SecondsInState() < 1.5d
                || context.Binder.GridController
                    ?.IsSandboxBattleModeActive == true)
            {
                TimeoutState(5d, "GENERIC_PROBE_NOT_RESTORED");
                return;
            }
            context.Flow.ManualChallengeShougunuPhase1();
            if (context.Flow.EncounterAdmission?.IsAcceptedBoss != true)
            {
                Fail("MANUAL_CHALLENGE_REJECTED:"
                    + context.Flow.LastDiagnosticCode);
                return;
            }
            SetState(WaitBossStart);
        }

        private static void PollBossStart(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (!context.P3.HasActiveSession
                || context.Flow.Snapshot?.phase
                    != V04ChapterFlowPhase.BattleRunning)
            {
                TimeoutState(
                    8d,
                    "ACCEPTED_BOSS_DID_NOT_START:"
                    + context.Flow.LastDiagnosticCode + "/"
                    + context.P3.LastDiagnosticCode);
                return;
            }
            if (context.P3.ResetGeneration != 1
                || context.Journey.OwnsAuthoredSlot
                || context.Binder.OrdinaryAdmissionActive
                || !context.BossVisual.BossEncounterAdmissionGranted
                || GetInt("TotalPublished") < 10
                || GetInt("StageMask") != 0x1FF
                || (GetInt("VisualMask") & 0x1F) != 0x1F
                || !GetBool("HpChanged")
                || !GetBool("ShellChanged"))
            {
                Fail("FINAL_FRESH_PLAY_COVERAGE_INCOMPLETE");
                return;
            }
            SucceedAndExit(context);
        }

        private static void PollWaitPauseFreeze(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (context.Binder.GridController?.IsSandboxBattleModeActive
                == true)
            {
                TimeoutState(2d, "PREPARE_SURFACE_DID_NOT_OPEN");
                return;
            }
            if (context.Journey.CurrentState
                != C1JourneyEnemyVisualState.Idle)
            {
                TimeoutState(2d, "PREPARE_DID_NOT_FORCE_IDLE");
                return;
            }
            if (!PausedFactsMatch(context))
            {
                Fail("PREPARE_FREEZE_FACTS_CHANGED");
                return;
            }
            if (SecondsInState() < 3d)
            {
                TimeoutState(6d, "PREPARE_FREEZE_TIMEOUT");
                return;
            }
            if (!context.Binder.AuthoredResetButton.interactable)
            {
                TimeoutState(4d, "CONTINUE_BATTLE_NOT_INTERACTABLE");
                return;
            }
            context.Binder.AuthoredResetButton.onClick.Invoke();
            SetState(WaitResumeSurface);
        }

        private static void PollWaitResumeSurface(
            C1EncounterOwnershipFreshPlayContext context)
        {
            if (!PausedFactsMatch(context))
            {
                Fail("PREPARE_RESUME_TRANSITION_MUTATED_FACTS");
                return;
            }
            if (context.Binder.GridController?.IsSandboxBattleModeActive
                != true)
            {
                TimeoutState(4d, "CONTINUE_BATTLE_DID_NOT_RESTORE_SURFACE");
                return;
            }
            if (context.Flow.NormalBattle?.CurrentTick
                != GetInt("PauseTick"))
            {
                Fail("RESUME_SURFACE_ADVANCED_SAME_FRAME");
                return;
            }
            SetState(WaitResumeProgress);
        }

        private static void PollWaitResumeProgress(
            C1EncounterOwnershipFreshPlayContext context)
        {
            V04Chapter1NormalEnemyBattleAdapter battle =
                context.Flow.NormalBattle;
            if (battle == null)
            {
                Fail("RESUME_BATTLE_MISSING");
                return;
            }
            if (battle.CurrentTick <= GetInt("PauseTick"))
            {
                TimeoutState(4d, "RESUME_DID_NOT_PROGRESS");
                return;
            }
            if ((int)(battle.Enemy?.LatestCue?.CueSequence ?? 0L)
                <= GetInt("PauseCueSeq"))
            {
                TimeoutState(6d, "RESUME_DID_NOT_CONSUME_NEW_CUE");
                return;
            }
            SetBool("PauseTestPassed", true);
            SetState(ObserveBattle);
        }

        private static void InvokeWhenInteractable(
            C1EncounterOwnershipFreshPlayContext context,
            int nextState)
        {
            if (!context.Binder.AuthoredResetButton.interactable)
            {
                TimeoutState(4d, "AUTHORED_BUTTON_NOT_INTERACTABLE");
                return;
            }
            context.Binder.AuthoredResetButton.onClick.Invoke();
            SetState(nextState);
        }

        private static void SucceedAndExit(
            C1EncounterOwnershipFreshPlayContext context)
        {
            StringBuilder evidence = new();
            evidence.AppendLine("status=FRESH_PLAY_PASS");
            evidence.AppendLine("package=" + PackageId);
            evidence.AppendLine(
                "authoredStartSurface=V04BattlePrepareStateButton");
            evidence.AppendLine("officialFixture=I007-I012+I031");
            evidence.AppendLine("ordinaryCycles=2");
            evidence.AppendLine("stagesObservedMask=0x"
                + GetInt("StageMask").ToString(
                    "X",
                    CultureInfo.InvariantCulture));
            evidence.AppendLine("ordinaryPublishedApplications="
                + GetInt("TotalPublished"));
            evidence.AppendLine("liHuoAcceptedPresentations="
                + context.LiHuo.AcceptedPresentationCount);
            evidence.AppendLine("outlineAcceptedTriggers="
                + context.Outline.RealAcceptedTriggerCount);
            evidence.AppendLine("visualStateMask=0x"
                + GetInt("VisualMask").ToString(
                    "X",
                    CultureInfo.InvariantCulture));
            evidence.AppendLine("firstStageVisualMask=0x"
                + GetInt("FirstStageVisualMask").ToString(
                    "X",
                    CultureInfo.InvariantCulture));
            evidence.AppendLine("firstStageApplications="
                + GetInt("FirstStageApplications"));
            evidence.AppendLine("firstStageTerminalTick="
                + GetInt("FirstStageTerminalTick"));
            evidence.AppendLine("firstStageBasicCues="
                + GetInt("FirstStageBasicCues"));
            evidence.AppendLine("firstStageSkillCues="
                + GetInt("FirstStageSkillCues"));
            evidence.AppendLine("firstStageDirectRequests="
                + GetInt("FirstStageDirectRequests"));
            evidence.AppendLine("firstStageSkillRequests="
                + GetInt("FirstStageSkillRequests"));
            evidence.AppendLine("preparePauseFreezeVerified="
                + GetBool("PauseTestPassed"));
            evidence.AppendLine("ordinaryHpUiChanged=true");
            evidence.AppendLine("ordinaryShellUiChanged=true");
            evidence.AppendLine("genericBossStartP3=false");
            evidence.AppendLine(
                "acceptedManualChallengeP3Sessions=1");
            evidence.AppendLine("sceneDirtyDuringPlay=false");
            SetString("Evidence", evidence.ToString());
            SetString("Failure", string.Empty);
            SetState(Exiting);
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string failure)
        {
            SetString("Failure", failure ?? "UNKNOWN_FAILURE");
            SetState(Exiting);
            Debug.LogError(
                "[" + PackageId + "] FRESH_PLAY_FAIL "
                + GetString("Failure"));
            EditorApplication.ExitPlaymode();
        }

        private static void TimeoutState(
            double seconds,
            string failure)
        {
            if (SecondsInState() > seconds)
            {
                Fail(failure);
            }
        }

        private static void FinalizeAfterPlay()
        {
            Scene scene = SceneManager.GetActiveScene();
            C1EncounterOwnershipFreshPlayContext context =
                C1EncounterOwnershipFreshPlayProbe.Resolve(scene);
            string failure = GetString("Failure");
            bool clean = scene.IsValid() && !scene.isDirty;
            bool restored = context.AuthoredSlot != null
                && string.Equals(
                    GetString("OriginalSlot"),
                    C1EncounterOwnershipFreshPlayProbe.Fingerprint(
                        context.AuthoredSlot),
                    StringComparison.Ordinal);
            bool rootsReleased =
                C1EncounterOwnershipFreshPlayProbe
                    .HasNoRuntimeRootLeak();
            if (!clean)
            {
                failure = Append(failure, "SCENE_DIRTY_AFTER_EXIT");
            }
            if (!restored)
            {
                failure = Append(
                    failure,
                    "AUTHORED_SLOT_NOT_RESTORED");
            }
            if (!rootsReleased)
            {
                failure = Append(
                    failure,
                    "RUNTIME_ROOT_LEAK");
            }
            string evidence = string.IsNullOrWhiteSpace(failure)
                ? GetString("Evidence")
                : "status=FRESH_PLAY_FAIL\nfailure="
                    + failure + "\n";
            evidence += "sceneDirtyAfterExit=" + (!clean) + "\n"
                + "authoredSlotRestored=" + restored + "\n"
                + "runtimeRootsReleased=" + rootsReleased + "\n";
            Directory.CreateDirectory("Temp");
            File.WriteAllText(EvidencePath, evidence, Encoding.UTF8);
            RestorePreferences();
            if (File.Exists(RequestPath))
            {
                File.Delete(RequestPath);
            }
            ClearSession();
            if (string.IsNullOrWhiteSpace(failure))
            {
                Debug.Log(
                    "[" + PackageId
                    + "] C1_ENCOUNTER_OWNERSHIP_FRESH_PLAY_PASS "
                    + evidence.Replace(
                        Environment.NewLine,
                        " ").Trim());
            }
            else
            {
                Debug.LogError(
                    "[" + PackageId
                    + "] C1_ENCOUNTER_OWNERSHIP_FRESH_PLAY_FAIL "
                    + failure);
            }
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(
                    string.IsNullOrWhiteSpace(failure) ? 0 : 1);
            }
        }

        private static void FinishWithoutPlay(string failure)
        {
            Directory.CreateDirectory("Temp");
            File.WriteAllText(
                EvidencePath,
                "status=FRESH_PLAY_FAIL\nfailure="
                + failure + "\n",
                Encoding.UTF8);
            if (File.Exists(RequestPath))
            {
                File.Delete(RequestPath);
            }
            ClearSession();
            Debug.LogError(
                "[" + PackageId
                + "] C1_ENCOUNTER_OWNERSHIP_FRESH_PLAY_FAIL "
                + failure);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }

        private static void RestorePreferences()
        {
            EditorPrefs.SetBool(
                V04Chapter1ContinuousBattleIntegrationContract
                    .EditorPreferenceKey,
                GetBool("OriginalContinuousPref"));
            EditorPrefs.SetBool(
                C1JourneyPresentationProfile.EditorPreferenceKey,
                GetBool("OriginalJourneyPref"));
        }

        private static void SetState(int state)
        {
            SetInt("State", state);
            SetString(
                "StateTicks",
                DateTime.UtcNow.Ticks.ToString(
                    CultureInfo.InvariantCulture));
        }

        private static double SecondsInState()
        {
            return SecondsSince("StateTicks");
        }

        private static double SecondsSince(string suffix)
        {
            if (!long.TryParse(
                    GetString(suffix),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long ticks)
                || ticks <= 0L)
            {
                return 0d;
            }
            return TimeSpan.FromTicks(
                Math.Max(0L, DateTime.UtcNow.Ticks - ticks))
                .TotalSeconds;
        }

        private static string Append(
            string current,
            string addition)
        {
            return string.IsNullOrWhiteSpace(current)
                ? addition
                : current + ";" + addition;
        }

        private static int GetInt(string suffix)
        {
            return SessionState.GetInt(Key + suffix, 0);
        }

        private static void SetInt(string suffix, int value)
        {
            SessionState.SetInt(Key + suffix, value);
        }

        private static bool GetBool(string suffix)
        {
            return SessionState.GetBool(Key + suffix, false);
        }

        private static void SetBool(string suffix, bool value)
        {
            SessionState.SetBool(Key + suffix, value);
        }

        private static string GetString(string suffix)
        {
            return SessionState.GetString(
                Key + suffix,
                string.Empty);
        }

        private static void SetString(
            string suffix,
            string value)
        {
            SessionState.SetString(
                Key + suffix,
                value ?? string.Empty);
        }

        private static void ClearSession()
        {
            foreach (string suffix in new[]
            {
                "State", "Cycle", "StageIndex", "TotalPublished",
                "VisualMask", "FirstStageVisualMask", "StageMask",
                "FirstGeneration", "ResetLiHuo", "ResetOutline",
                "PauseTick", "PauseHp", "PauseApps", "PauseCueSeq",
                "PauseOutgoing", "FirstStageApplications",
                "FirstStageTerminalTick", "FirstStageBasicCues",
                "FirstStageSkillCues", "FirstStageDirectRequests",
                "FirstStageSkillRequests"
            })
            {
                SessionState.EraseInt(Key + suffix);
            }
            foreach (string suffix in new[]
            {
                "OriginalContinuousPref", "OriginalJourneyPref",
                "HpChanged", "ShellChanged", "PauseTestPassed"
            })
            {
                SessionState.EraseBool(Key + suffix);
            }
            foreach (string suffix in new[]
            {
                "StateTicks", "StartedTicks", "Failure",
                "OriginalSlot", "Evidence"
            })
            {
                SessionState.EraseString(Key + suffix);
            }
        }

        private static void CapturePauseSnapshot(
            C1EncounterOwnershipFreshPlayContext context)
        {
            V04Chapter1NormalEnemyBattleAdapter battle =
                context.Flow.NormalBattle;
            C1EnemyRuntimeSnapshot enemy = battle?.Enemy;
            SetInt("PauseTick", (int)(battle?.CurrentTick ?? 0L));
            SetInt("PauseHp", enemy?.CurrentHp ?? 0);
            SetInt(
                "PauseApps",
                battle?.AcceptedItemApplicationCount ?? 0);
            SetInt(
                "PauseCueSeq",
                (int)(enemy?.LatestCue?.CueSequence ?? 0L));
            SetInt(
                "PauseOutgoing",
                battle?.OutgoingRequestsResolved ?? 0);
        }

        private static bool PausedFactsMatch(
            C1EncounterOwnershipFreshPlayContext context)
        {
            V04Chapter1NormalEnemyBattleAdapter battle =
                context.Flow.NormalBattle;
            C1EnemyRuntimeSnapshot enemy = battle?.Enemy;
            return battle != null
                && enemy != null
                && battle.CurrentTick == GetInt("PauseTick")
                && enemy.CurrentHp == GetInt("PauseHp")
                && battle.AcceptedItemApplicationCount
                    == GetInt("PauseApps")
                && (int)(enemy.LatestCue?.CueSequence ?? 0L)
                    == GetInt("PauseCueSeq")
                && battle.OutgoingRequestsResolved
                    == GetInt("PauseOutgoing");
        }

        private static bool ValidateFirstEncounterTerminalFacts(
            V04Chapter1NormalEnemyBattleAdapter battle,
            out string failure)
        {
            failure = string.Empty;
            C1EnemyRuntimeSnapshot enemy = battle?.Enemy;
            if (battle == null || enemy == null)
            {
                failure = "FIRST_ENCOUNTER_FACTS_MISSING";
                return false;
            }
            int basicCues = enemy.Cues.Count(value =>
                value.Kind == C1EnemyCueKind.BasicAttack);
            int skillCues = enemy.Cues.Count(value =>
                value.Kind == C1EnemyCueKind.Skill);
            int directRequests = enemy.PendingRequests.Count(value =>
                string.Equals(
                    value.RequestKey,
                    C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                    StringComparison.Ordinal));
            int skillRequests = enemy.PendingRequests.Count(value =>
                string.Equals(
                    value.RequestKey,
                    C1EnemyRuntimeContract.PollutedPulseSkillRequest,
                    StringComparison.Ordinal));
            if (battle.AcceptedItemApplicationCount != 14
                || battle.PublishedPresentationCount != 14
                || enemy.LastAcceptedBattleTick != 21000L
                || basicCues != 4
                || skillCues != 2
                || directRequests != 4
                || skillRequests != 2
                || GetInt("FirstStageVisualMask") != 0x1F)
            {
                failure =
                    "FIRST_STAGE_TRACE_MISMATCH:"
                    + "apps=" + battle.AcceptedItemApplicationCount
                    + ",pub=" + battle.PublishedPresentationCount
                    + ",tick=" + enemy.LastAcceptedBattleTick
                    + ",basic=" + basicCues
                    + ",skill=" + skillCues
                    + ",direct=" + directRequests
                    + ",skillReq=" + skillRequests
                    + ",visualMask=0x"
                    + GetInt("FirstStageVisualMask").ToString(
                        "X",
                        CultureInfo.InvariantCulture);
                return false;
            }
            return true;
        }

        private static void CaptureFirstEncounterFacts(
            V04Chapter1NormalEnemyBattleAdapter battle)
        {
            C1EnemyRuntimeSnapshot enemy = battle.Enemy;
            SetInt(
                "FirstStageApplications",
                battle.AcceptedItemApplicationCount);
            SetInt(
                "FirstStageTerminalTick",
                (int)enemy.LastAcceptedBattleTick);
            SetInt(
                "FirstStageBasicCues",
                enemy.Cues.Count(value =>
                    value.Kind == C1EnemyCueKind.BasicAttack));
            SetInt(
                "FirstStageSkillCues",
                enemy.Cues.Count(value =>
                    value.Kind == C1EnemyCueKind.Skill));
            SetInt(
                "FirstStageDirectRequests",
                enemy.PendingRequests.Count(value =>
                    string.Equals(
                        value.RequestKey,
                        C1EnemyRuntimeContract.DirectPlayerDamageRequest,
                        StringComparison.Ordinal)));
            SetInt(
                "FirstStageSkillRequests",
                enemy.PendingRequests.Count(value =>
                    string.Equals(
                        value.RequestKey,
                        C1EnemyRuntimeContract.PollutedPulseSkillRequest,
                        StringComparison.Ordinal)));
        }

        private static Button FindGridButton(
            C1EncounterOwnershipFreshPlayContext context,
            string name)
        {
            if (context?.Binder?.GridController == null
                || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            Button[] matches = context.Binder.GridController
                .GetComponentsInChildren<Button>(true)
                .Where(value => value != null
                    && string.Equals(
                        value.gameObject.name,
                        name,
                        StringComparison.Ordinal))
                .ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }
    }
}
