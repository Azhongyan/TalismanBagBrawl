using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using TalismanBag.V04.ChapterFlow.Chapter1.Integration;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(735)]
    public sealed class C1JourneyPresentationController : MonoBehaviour
    {
        private const string AuthoredBossSlotName = "Shougunu_1";
        private const string AuthoredEnemyAreaName = "V02EnemyArea";
        private const string AuthoredOrdinarySlotName = "Enemy";
        private const string AuthoredPlayerSlotName = "player";
        private const string TransitionRuntimeRootName =
            "V04_C1_Transition_Runtime";
        private const float TransitionVictoryHoldSeconds = 0.35f;
        private const float TransitionExitRunSeconds = 0.9f;
        private const float TransitionFadeOutSeconds = 0.45f;
        private const float TransitionFadeInSeconds = 0.45f;
        private const float TransitionPlayerEntrySeconds = 0.85f;
        private const float TransitionEnemyRevealSeconds = 0.4f;
        private readonly Dictionary<
                C1JourneyEnemyVisualState,
                FallbackAnimationClip>
            fallbackClips = new();
        private readonly Dictionary<Texture2D, Sprite>
            runtimeFallbackSprites = new();

        private V04Chapter1BattleSandboxRuntimeController flowController;
        private Image authoredBossSlot;
        private Image authoredOrdinarySlot;
        private Image authoredPlayerSlot;
        private C1AuthoredEnemyPresentationRoot
            authoredMultiEnemyPresentationRoot;
        private ShougunuPhase1VisualPrototypeController bossVisualOwner;
        private Sprite ordinaryOriginalSprite;
        private Material ordinaryOriginalMaterial;
        private Color ordinaryOriginalColor;
        private bool ordinaryOriginalEnabled;
        private bool ordinaryOriginalPreserveAspect;
        private bool ordinaryOriginalRaycastTarget;
        private bool ordinaryOriginalActiveSelf;
        private bool ordinaryStateCaptured;
        private Sprite playerOriginalSprite;
        private Material playerOriginalMaterial;
        private Color playerOriginalColor;
        private bool playerOriginalEnabled;
        private bool playerOriginalPreserveAspect;
        private bool playerOriginalRaycastTarget;
        private bool playerOriginalActiveSelf;
        private bool playerStateCaptured;
        private Vector3 playerOriginalWorldPosition;
        private Vector3 playerOriginalLocalScale;
        private Quaternion playerOriginalLocalRotation;
        private bool ownsAuthoredSlot;
        private string observedStageId = string.Empty;
        private V04ChapterFlowPhase observedPhase =
            V04ChapterFlowPhase.Disabled;
        private float beatStartedAt;
        private float manualPreviewUntil;
        private C1JourneyEnemyVisualState manualState;
        private int observedCueGeneration;
        private long observedCueSequence;
        private float visualStateStartedAt;
        private float visualStateUntil;
        private bool pendingAttackState;
        private bool pendingSkillState;
        private bool pendingHitState;
        private bool preparePauseActive;
        private RectTransform transitionRuntimeRoot;
        private Image transitionBackdropImage;
        private Image transitionFadeImage;
        private Image transitionExitGlowImage;
        private Sprite transitionSolidSprite;
        private Sprite transitionGlowSprite;
        private long activeTransitionSequence;
        private C1OrdinaryStageTransitionPhase activeTransitionPhase =
            C1OrdinaryStageTransitionPhase.None;
        private float activeTransitionPhaseStartedAt;
        private Vector3 transitionPlayerEntryStartWorldPosition;
        private Vector3 transitionPlayerAuthoredWorldPosition;
        private Vector3 transitionExitWorldPosition;
        private bool suppressAuthoredRestoreForPlayModeExit;

        public C1JourneyStageBeat CurrentBeat { get; private set; }
        public C1JourneyEnemyVisualState CurrentState { get; private set; } =
            C1JourneyEnemyVisualState.Idle;
        public float BeatElapsed => Mathf.Max(
            0f,
            Time.unscaledTime - beatStartedAt);
        public bool IsBound => flowController != null;
        public bool AuthoredSlotBound =>
            authoredOrdinarySlot != null
            && authoredBossSlot != null
            && authoredPlayerSlot != null;
        public bool OwnsAuthoredSlot => ownsAuthoredSlot;
        public string AuthoredSlotPath =>
            AuthoredEnemyAreaName + "/" + AuthoredOrdinarySlotName;
        public Sprite CurrentDisplayedSprite =>
            ownsAuthoredSlot
                ? authoredOrdinarySlot?.sprite
                : authoredBossSlot?.sprite;
        public C1EncounterAdmissionSnapshot EncounterAdmission =>
            flowController?.EncounterAdmission;
        public string FlowStatus
        {
            get
            {
                if (flowController == null)
                {
                    return "Flow: NOT_BOUND";
                }
                V04ChapterFlowStateSnapshot snapshot =
                    flowController.Snapshot;
                C1EncounterAdmissionSnapshot admission =
                    flowController.EncounterAdmission;
                return "Flow: "
                    + (snapshot?.currentStageId ?? "-")
                    + " / "
                    + (snapshot?.phase.ToString() ?? "Disabled")
                    + " / admission="
                    + (admission?.AdmittedEncounterKind.ToString()
                        ?? C1EncounterAdmissionKind.None.ToString())
                    + " · "
                    + flowController.LastDiagnosticCode;
            }
        }
        public string Diagnostic { get; private set; } =
            "WAITING_FOR_CHAPTER1_SESSION";

        public void Bind(
            V04Chapter1BattleSandboxRuntimeController runtimeController)
        {
            flowController = runtimeController;
            TryBindAuthoredSlots();
            ApplyAdmissionHandoff();
        }

        public void PreviewState(C1JourneyEnemyVisualState state)
        {
            if (EncounterAdmission?.IsOrdinary != true)
            {
                Diagnostic =
                    "ORDINARY_PRESENTATION_CUE_REJECTED_NOT_ADMITTED";
                return;
            }
            manualState = state;
            manualPreviewUntil = Time.unscaledTime
                + (state == C1JourneyEnemyVisualState.Idle ? 60f : 1.4f);
            CurrentState = state;
            visualStateStartedAt = Time.unscaledTime;
            Diagnostic = "AUTHORED_SLOT_CUE_PREVIEW_" + state;
            ApplyAdmissionHandoff();
        }

        public bool SetFlowPanelVisible(bool visible)
        {
            V04Chapter1BattleSandboxOverlay overlay = Resources
                .FindObjectsOfTypeAll<V04Chapter1BattleSandboxOverlay>()
                .FirstOrDefault(value => value != null
                    && value.gameObject.scene == gameObject.scene);
            if (overlay == null)
            {
                Diagnostic = "FLOW_PANEL_OVERLAY_NOT_FOUND";
                return false;
            }
            overlay.SetPanelVisible(visible);
            Diagnostic = visible
                ? "FLOW_PANEL_VISIBLE"
                : "FLOW_PANEL_HIDDEN";
            return true;
        }

        public bool ToggleFlowPanel()
        {
            V04Chapter1BattleSandboxOverlay overlay = Resources
                .FindObjectsOfTypeAll<V04Chapter1BattleSandboxOverlay>()
                .FirstOrDefault(value => value != null
                    && value.gameObject.scene == gameObject.scene);
            if (overlay == null)
            {
                Diagnostic = "FLOW_PANEL_OVERLAY_NOT_FOUND";
                return false;
            }
            return SetFlowPanelVisible(!overlay.IsVisible);
        }

        public void RequestStartChapter1Session()
        {
            if (!RequireFlowController("START_CHAPTER1"))
            {
                return;
            }
            flowController.StartChapter1Session();
            LogFlowAction("START_CHAPTER1");
            ApplyAdmissionHandoff();
        }

        public void RequestStartCurrentNormalStage()
        {
            if (!RequireFlowController("START_CURRENT_NORMAL_STAGE"))
            {
                return;
            }
            flowController.StartCurrentNormalStage();
            LogFlowAction("START_CURRENT_NORMAL_STAGE");
            ApplyAdmissionHandoff();
        }

        public void RequestContinueFromCurrentBoundary()
        {
            if (!RequireFlowController("CONTINUE_FROM_CURRENT_BOUNDARY"))
            {
                return;
            }
            flowController.ContinueFromSettlementOrDropCandidate();
            LogFlowAction("CONTINUE_FROM_CURRENT_BOUNDARY");
            ApplyAdmissionHandoff();
        }

        private void Awake()
        {
            LoadFallbackFrames();
            ResetVisualPlayback();
        }

        private void Update()
        {
            if (flowController == null)
            {
                flowController = Resources
                    .FindObjectsOfTypeAll<
                        V04Chapter1BattleSandboxRuntimeController>()
                    .FirstOrDefault(value => value != null
                        && value.gameObject.scene == gameObject.scene);
            }
            if (authoredOrdinarySlot == null
                || authoredBossSlot == null)
            {
                TryBindAuthoredSlots();
            }
            ResolveAuthoredMultiEnemyPresentationRoot();
            C1OrdinaryStageTransitionSnapshot transition =
                flowController?.OrdinaryStageTransition;

            V04ChapterFlowStateSnapshot snapshot = flowController?.Snapshot;
            if (snapshot == null
                || string.IsNullOrWhiteSpace(snapshot.currentStageId))
            {
                ApplyAdmissionHandoff();
                UpdateOrdinaryStageTransitionPresentation(transition);
                return;
            }

            bool stageChanged = !string.Equals(
                observedStageId,
                snapshot.currentStageId,
                StringComparison.Ordinal);
            bool phaseChanged = observedPhase != snapshot.phase;
            if (stageChanged)
            {
                observedStageId = snapshot.currentStageId;
                CurrentBeat = C1JourneyPresentationProfile.Find(
                    observedStageId);
                beatStartedAt = Time.unscaledTime;
                ResetVisualPlayback();
                Diagnostic = "AUTHORED_SLOT_BEAT_"
                    + (CurrentBeat?.transitionBeat.ToString()
                        ?? "UNKNOWN");
            }
            else if (phaseChanged
                && snapshot.currentStageId == "1-10"
                && snapshot.phase == V04ChapterFlowPhase.BossGate)
            {
                beatStartedAt = Time.unscaledTime;
                Diagnostic = "BOSS_GATE_SLOT_RELEASED";
            }
            observedPhase = snapshot.phase;

            bool transitionBlocksOrdinaryPlayback =
                IsTransitionPresentationBlocking(transition);
            if (!transitionBlocksOrdinaryPlayback
                && Time.unscaledTime < manualPreviewUntil)
            {
                CurrentState = manualState;
            }
            else if (!transitionBlocksOrdinaryPlayback
                && authoredMultiEnemyPresentationRoot == null)
            {
                C1EnemyRuntimeSnapshot enemy =
                    flowController?.NormalBattle?.Enemy;
                if (EncounterAdmission?.IsOrdinary == true
                    && flowController?.OrdinaryBattlePausedByPrepareSurface
                        == true)
                {
                    PauseVisualPlaybackForPrepare(enemy);
                }
                else
                {
                    ResumeVisualPlaybackAfterPrepare(enemy);
                    ObserveRuntimeCues(enemy);
                    AdvanceVisualPlayback(enemy);
                }
            }
            ApplyAdmissionHandoff();
            UpdateOrdinaryStageTransitionPresentation(transition);
        }

        private void OnDisable()
        {
            if (suppressAuthoredRestoreForPlayModeExit)
            {
                return;
            }
            ReleaseAuthoredSlot();
            RestorePlayerSlotState();
            TeardownTransitionRuntime();
            ReassertAcceptedBossOwner();
        }

        private void OnDestroy()
        {
            if (!suppressAuthoredRestoreForPlayModeExit)
            {
                ReleaseAuthoredSlot();
                RestorePlayerSlotState();
                TeardownTransitionRuntime();
                ReassertAcceptedBossOwner();
            }
            foreach (Sprite sprite in runtimeFallbackSprites.Values)
            {
                if (sprite != null)
                {
                    DestroyOwnedRuntimeObject(sprite);
                }
            }
            runtimeFallbackSprites.Clear();
            fallbackClips.Clear();
            if (transitionSolidSprite != null)
            {
                DestroyOwnedRuntimeObject(transitionSolidSprite);
            }
            if (transitionGlowSprite != null)
            {
                DestroyOwnedRuntimeObject(transitionGlowSprite);
            }
        }

        public void PrepareForPlayModeExit()
        {
            suppressAuthoredRestoreForPlayModeExit = true;
        }

        private static void DestroyOwnedRuntimeObject(
            UnityEngine.Object value)
        {
            if (value == null)
            {
                return;
            }
            if (Application.isPlaying)
            {
                Destroy(value);
            }
            else
            {
                DestroyImmediate(value);
            }
        }

        private void TryBindAuthoredSlots()
        {
            ResolveAuthoredMultiEnemyPresentationRoot();
            Image[] images = Resources.FindObjectsOfTypeAll<Image>()
                .Where(image => image != null
                    && image.gameObject.scene == gameObject.scene)
                .ToArray();
            Image[] ordinarySlots = images
                .Where(IsExactOrdinarySlot)
                .ToArray();
            if (ordinarySlots.Length != 1)
            {
                Diagnostic = ordinarySlots.Length == 0
                    ? "AUTHORED_ORDINARY_SLOT_MISSING"
                    : "AUTHORED_ORDINARY_SLOT_AMBIGUOUS_"
                        + ordinarySlots.Length;
                return;
            }
            Image[] bossSlots = images
                .Where(IsExactBossSlot)
                .ToArray();
            if (bossSlots.Length != 1)
            {
                authoredOrdinarySlot = null;
                Diagnostic = bossSlots.Length == 0
                    ? "AUTHORED_BOSS_SLOT_MISSING"
                    : "AUTHORED_BOSS_SLOT_AMBIGUOUS_"
                        + bossSlots.Length;
                return;
            }
            authoredOrdinarySlot = ordinarySlots[0];
            authoredBossSlot = bossSlots[0];
            Image[] playerSlots = images.Where(image => string.Equals(
                    image.gameObject.name,
                    AuthoredPlayerSlotName,
                    StringComparison.Ordinal))
                .ToArray();
            if (playerSlots.Length != 1)
            {
                authoredOrdinarySlot = null;
                authoredBossSlot = null;
                Diagnostic = playerSlots.Length == 0
                    ? "AUTHORED_PLAYER_SLOT_MISSING"
                    : "AUTHORED_PLAYER_SLOT_AMBIGUOUS_"
                        + playerSlots.Length;
                return;
            }
            authoredPlayerSlot = playerSlots[0];
            bossVisualOwner = authoredBossSlot.GetComponent<
                ShougunuPhase1VisualPrototypeController>();
            CaptureOrdinarySlotState();
            CapturePlayerSlotState();
            Diagnostic = ordinaryOriginalSprite != null
                && playerOriginalSprite != null
                ? "AUTHORED_PLAYER_ORDINARY_BOSS_SLOTS_BOUND"
                : "AUTHORED_ORDINARY_SLOT_SPRITE_MISSING";
        }

        private static bool IsExactBossSlot(Image image)
        {
            return string.Equals(
                    image.gameObject.name,
                    AuthoredBossSlotName,
                    StringComparison.Ordinal)
                && image.transform.parent != null
                && string.Equals(
                    image.transform.parent.name,
                    AuthoredEnemyAreaName,
                    StringComparison.Ordinal);
        }

        private static bool IsExactOrdinarySlot(Image image)
        {
            return string.Equals(
                    image.gameObject.name,
                    AuthoredOrdinarySlotName,
                    StringComparison.Ordinal)
                && image.transform.parent != null
                && string.Equals(
                    image.transform.parent.name,
                    AuthoredEnemyAreaName,
                    StringComparison.Ordinal);
        }

        private void CaptureOrdinarySlotState()
        {
            if (ordinaryStateCaptured
                || authoredOrdinarySlot == null)
            {
                return;
            }
            ordinaryOriginalSprite = authoredOrdinarySlot.sprite;
            ordinaryOriginalMaterial = authoredOrdinarySlot.material;
            ordinaryOriginalColor = authoredOrdinarySlot.color;
            ordinaryOriginalEnabled = authoredOrdinarySlot.enabled;
            ordinaryOriginalPreserveAspect =
                authoredOrdinarySlot.preserveAspect;
            ordinaryOriginalRaycastTarget =
                authoredOrdinarySlot.raycastTarget;
            ordinaryOriginalActiveSelf =
                authoredOrdinarySlot.gameObject.activeSelf;
            ordinaryStateCaptured = true;
        }

        private void CapturePlayerSlotState()
        {
            if (playerStateCaptured
                || authoredPlayerSlot == null)
            {
                return;
            }
            playerOriginalSprite = authoredPlayerSlot.sprite;
            playerOriginalMaterial = authoredPlayerSlot.material;
            playerOriginalColor = authoredPlayerSlot.color;
            playerOriginalEnabled = authoredPlayerSlot.enabled;
            playerOriginalPreserveAspect =
                authoredPlayerSlot.preserveAspect;
            playerOriginalRaycastTarget =
                authoredPlayerSlot.raycastTarget;
            playerOriginalActiveSelf =
                authoredPlayerSlot.gameObject.activeSelf;
            playerOriginalWorldPosition =
                authoredPlayerSlot.rectTransform.position;
            playerOriginalLocalScale =
                authoredPlayerSlot.rectTransform.localScale;
            playerOriginalLocalRotation =
                authoredPlayerSlot.rectTransform.localRotation;
            playerStateCaptured = true;
        }

        private void ApplyAdmissionHandoff()
        {
            C1EncounterAdmissionSnapshot admission = EncounterAdmission;
            if (admission?.IsOrdinary == true)
            {
                AcquireAuthoredSlotForOrdinary(admission);
                return;
            }

            ReleaseAuthoredSlot();
            EnsureBossVisualOwner();
            bool bossAccepted = admission?.IsAcceptedBoss == true;
            bossVisualOwner?.SetBossEncounterAdmission(bossAccepted);
            Diagnostic = bossAccepted
                ? "AUTHORED_SLOT_HANDOFF_TO_SHOUGUNU_VISUAL_OWNER"
                : admission?.CurrentStageId == "1-10"
                    ? "BOSS_GATE_IDLE_NO_P3_ADMISSION"
                    : Diagnostic;
        }

        private void AcquireAuthoredSlotForOrdinary(
            C1EncounterAdmissionSnapshot admission)
        {
            ResolveAuthoredMultiEnemyPresentationRoot();
            if (authoredMultiEnemyPresentationRoot != null)
            {
                ReleaseAuthoredSlot();
                EnsureBossVisualOwner();
                bossVisualOwner?.SetBossEncounterAdmission(false);
                Diagnostic =
                    "AUTHORED_MULTI_ENEMY_PRESENTATION_ADAPTER_OWNS_ORDINARY_SLOTS";
                return;
            }
            if (authoredOrdinarySlot == null
                || authoredBossSlot == null)
            {
                TryBindAuthoredSlots();
            }
            if (authoredOrdinarySlot == null)
            {
                return;
            }

            EnsureBossVisualOwner();
            if (!ownsAuthoredSlot)
            {
                bossVisualOwner?.SetBossEncounterAdmission(false);
            }
            Sprite sprite = CurrentFallbackSprite()
                ?? ordinaryOriginalSprite;
            if (sprite == null)
            {
                authoredOrdinarySlot.gameObject.SetActive(true);
                authoredOrdinarySlot.enabled = false;
                ownsAuthoredSlot = true;
                Diagnostic =
                    "ORDINARY_VISUAL_SOURCE_MISSING_ORDINARY_SLOT_SUPPRESSED";
                return;
            }

            authoredOrdinarySlot.gameObject.SetActive(true);
            authoredOrdinarySlot.sprite = sprite;
            authoredOrdinarySlot.material = ordinaryOriginalMaterial;
            authoredOrdinarySlot.color = ordinaryOriginalColor;
            authoredOrdinarySlot.preserveAspect =
                ordinaryOriginalPreserveAspect;
            authoredOrdinarySlot.raycastTarget = false;
            authoredOrdinarySlot.enabled = true;
            ownsAuthoredSlot = true;
            Diagnostic =
                C1JourneyPresentationProfile.TemporaryFallbackTag
                + (CurrentBeat?.runtimeMechanicHeld == true
                    ? "_HELD_VISUAL_SLOT_"
                    : "_MISSING_PROFILE_STATES_")
                + admission.CurrentStageId
                + "_RUNTIME_IDENTITY_"
                + admission.RuntimeProfileId;
        }

        private void ReleaseAuthoredSlot()
        {
            if (!ownsAuthoredSlot
                || !ordinaryStateCaptured
                || authoredOrdinarySlot == null)
            {
                ownsAuthoredSlot = false;
                return;
            }
            authoredOrdinarySlot.sprite = ordinaryOriginalSprite;
            authoredOrdinarySlot.material = ordinaryOriginalMaterial;
            authoredOrdinarySlot.color = ordinaryOriginalColor;
            authoredOrdinarySlot.preserveAspect =
                ordinaryOriginalPreserveAspect;
            authoredOrdinarySlot.raycastTarget =
                ordinaryOriginalRaycastTarget;
            authoredOrdinarySlot.enabled = ordinaryOriginalEnabled;
            authoredOrdinarySlot.gameObject.SetActive(
                ordinaryOriginalActiveSelf);
            ownsAuthoredSlot = false;
        }

        private void EnsureBossVisualOwner()
        {
            if (bossVisualOwner == null && authoredBossSlot != null)
            {
                bossVisualOwner = authoredBossSlot.GetComponent<
                    ShougunuPhase1VisualPrototypeController>();
            }
        }

        private void ReassertAcceptedBossOwner()
        {
            if (EncounterAdmission?.IsAcceptedBoss != true)
            {
                return;
            }
            EnsureBossVisualOwner();
            bossVisualOwner?.SetBossEncounterAdmission(true);
        }

        private void RestorePlayerSlotState()
        {
            if (!playerStateCaptured || authoredPlayerSlot == null)
            {
                return;
            }
            authoredPlayerSlot.sprite = playerOriginalSprite;
            authoredPlayerSlot.material = playerOriginalMaterial;
            authoredPlayerSlot.color = playerOriginalColor;
            authoredPlayerSlot.preserveAspect =
                playerOriginalPreserveAspect;
            authoredPlayerSlot.raycastTarget =
                playerOriginalRaycastTarget;
            authoredPlayerSlot.enabled = playerOriginalEnabled;
            authoredPlayerSlot.gameObject.SetActive(
                playerOriginalActiveSelf);
            RectTransform rect = authoredPlayerSlot.rectTransform;
            rect.position = playerOriginalWorldPosition;
            rect.localScale = playerOriginalLocalScale;
            rect.localRotation = playerOriginalLocalRotation;
        }

        private bool IsTransitionPresentationBlocking(
            C1OrdinaryStageTransitionSnapshot transition)
        {
            return transition != null
                && transition.HasSequence
                && transition.Phase != C1OrdinaryStageTransitionPhase.None
                && transition.Phase
                    != C1OrdinaryStageTransitionPhase.NextBattleReady
                && transition.Phase
                    != C1OrdinaryStageTransitionPhase.BattleRunning;
        }

        private void UpdateOrdinaryStageTransitionPresentation(
            C1OrdinaryStageTransitionSnapshot transition)
        {
            if (transition == null || !transition.HasSequence)
            {
                if (activeTransitionSequence != 0L)
                {
                    EndOrdinaryStageTransitionPresentation();
                }
                return;
            }
            if (transition.Phase == C1OrdinaryStageTransitionPhase.None)
            {
                EndOrdinaryStageTransitionPresentation();
                return;
            }
            if (activeTransitionSequence != transition.Sequence)
            {
                BeginOrdinaryStageTransitionPresentation(transition);
            }
            if (activeTransitionPhase != transition.Phase)
            {
                activeTransitionPhase = transition.Phase;
                activeTransitionPhaseStartedAt = Time.unscaledTime;
                PrepareTransitionPhase(transition);
            }
            StepTransitionPresentation(transition);
            if (transition.Phase == C1OrdinaryStageTransitionPhase.BattleRunning
                || (transition.Phase
                        == C1OrdinaryStageTransitionPhase.NextBattleReady
                    && string.Equals(
                        transition.CurrentStageId,
                        "1-10",
                        StringComparison.Ordinal)))
            {
                EndOrdinaryStageTransitionPresentation();
            }
        }

        private void BeginOrdinaryStageTransitionPresentation(
            C1OrdinaryStageTransitionSnapshot transition)
        {
            TryBindAuthoredSlots();
            CapturePlayerSlotState();
            EnsureTransitionRuntime();
            activeTransitionSequence = transition.Sequence;
            activeTransitionPhase = transition.Phase;
            activeTransitionPhaseStartedAt = Time.unscaledTime;
            transitionPlayerAuthoredWorldPosition =
                playerOriginalWorldPosition;
            transitionPlayerEntryStartWorldPosition =
                ResolvePlayerEntryStartWorldPosition();
            transitionExitWorldPosition =
                ResolveTransitionExitWorldPosition();
            ApplyTransitionBackdropForStage(transition.CurrentStageId);
            authoredPlayerSlot.gameObject.SetActive(true);
            authoredPlayerSlot.enabled = true;
            authoredPlayerSlot.raycastTarget = false;
            authoredPlayerSlot.rectTransform.position =
                transitionPlayerAuthoredWorldPosition;
            authoredPlayerSlot.rectTransform.localScale =
                playerOriginalLocalScale;
            authoredPlayerSlot.rectTransform.localRotation =
                playerOriginalLocalRotation;
            SetPlayerAlpha(1f);
            SetOrdinarySlotAlpha(1f);
            SetTransitionFadeAlpha(0f, true);
            SetExitGlowAlpha(0f);
            PrepareTransitionPhase(transition);
        }

        private void EndOrdinaryStageTransitionPresentation()
        {
            activeTransitionSequence = 0L;
            activeTransitionPhase = C1OrdinaryStageTransitionPhase.None;
            activeTransitionPhaseStartedAt = 0f;
            if (authoredPlayerSlot != null)
            {
                authoredPlayerSlot.gameObject.SetActive(true);
                authoredPlayerSlot.enabled = playerOriginalEnabled;
                authoredPlayerSlot.rectTransform.position =
                    playerOriginalWorldPosition;
                authoredPlayerSlot.rectTransform.localScale =
                    playerOriginalLocalScale;
                authoredPlayerSlot.rectTransform.localRotation =
                    playerOriginalLocalRotation;
                SetPlayerAlpha(1f);
            }
            SetOrdinarySlotAlpha(1f);
            SetExitGlowAlpha(0f);
            SetTransitionFadeAlpha(0f, false);
        }

        private void PrepareTransitionPhase(
            C1OrdinaryStageTransitionSnapshot transition)
        {
            switch (activeTransitionPhase)
            {
                case C1OrdinaryStageTransitionPhase.VictoryAccepted:
                    SetTransitionFadeAlpha(0f, true);
                    SetExitGlowAlpha(0f);
                    authoredPlayerSlot.rectTransform.position =
                        transitionPlayerAuthoredWorldPosition;
                    SetPlayerAlpha(1f);
                    SetOrdinarySlotAlpha(1f);
                    break;
                case C1OrdinaryStageTransitionPhase.ExitRun:
                    transitionExitWorldPosition =
                        ResolveTransitionExitWorldPosition();
                    PositionExitGlow(transitionExitWorldPosition);
                    SetExitGlowAlpha(0.6f);
                    break;
                case C1OrdinaryStageTransitionPhase.FadeOut:
                    SetExitGlowAlpha(0.55f);
                    break;
                case C1OrdinaryStageTransitionPhase.StageSwitchWhileBlack:
                    flowController?.TryCommitOrdinaryStageSwitch(
                        transition.Sequence);
                    ApplyTransitionBackdropForStage(
                        flowController?.Snapshot?.currentStageId
                        ?? transition.NextStageId);
                    transitionPlayerEntryStartWorldPosition =
                        ResolvePlayerEntryStartWorldPosition();
                    authoredPlayerSlot.rectTransform.position =
                        transitionPlayerEntryStartWorldPosition;
                    SetPlayerAlpha(1f);
                    SetExitGlowAlpha(0f);
                    if (EncounterAdmission?.IsOrdinary == true)
                    {
                        SetOrdinarySlotAlpha(0f);
                    }
                    break;
                case C1OrdinaryStageTransitionPhase.FadeIn:
                    authoredPlayerSlot.rectTransform.position =
                        transitionPlayerEntryStartWorldPosition;
                    break;
                case C1OrdinaryStageTransitionPhase.PlayerEntry:
                    SetTransitionFadeAlpha(0f, true);
                    authoredPlayerSlot.rectTransform.position =
                        transitionPlayerEntryStartWorldPosition;
                    SetPlayerAlpha(1f);
                    break;
                case C1OrdinaryStageTransitionPhase.EnemyReveal:
                    authoredPlayerSlot.rectTransform.position =
                        transitionPlayerAuthoredWorldPosition;
                    if (EncounterAdmission?.IsOrdinary == true)
                    {
                        SetOrdinarySlotAlpha(0f);
                    }
                    break;
            }
        }

        private void StepTransitionPresentation(
            C1OrdinaryStageTransitionSnapshot transition)
        {
            float elapsed = Mathf.Max(
                0f,
                Time.unscaledTime - activeTransitionPhaseStartedAt);
            switch (activeTransitionPhase)
            {
                case C1OrdinaryStageTransitionPhase.VictoryAccepted:
                    if (elapsed >= TransitionVictoryHoldSeconds)
                    {
                        TryAdvanceTransitionPhase(
                            transition,
                            C1OrdinaryStageTransitionPhase.ExitRun);
                    }
                    break;
                case C1OrdinaryStageTransitionPhase.ExitRun:
                    float exitT = Mathf.Clamp01(
                        elapsed / TransitionExitRunSeconds);
                    AnimatePlayerRun(
                        transitionPlayerAuthoredWorldPosition,
                        transitionExitWorldPosition,
                        exitT);
                    SetExitGlowAlpha(0.5f
                        + Mathf.Sin(Time.unscaledTime * 5f) * 0.18f);
                    if (exitT >= 1f)
                    {
                        TryAdvanceTransitionPhase(
                            transition,
                            C1OrdinaryStageTransitionPhase.FadeOut);
                    }
                    break;
                case C1OrdinaryStageTransitionPhase.FadeOut:
                    float fadeOutT = Mathf.Clamp01(
                        elapsed / TransitionFadeOutSeconds);
                    SetTransitionFadeAlpha(fadeOutT, true);
                    if (fadeOutT >= 1f)
                    {
                        TryAdvanceTransitionPhase(
                            transition,
                            C1OrdinaryStageTransitionPhase
                                .StageSwitchWhileBlack);
                    }
                    break;
                case C1OrdinaryStageTransitionPhase.StageSwitchWhileBlack:
                    TryAdvanceTransitionPhase(
                        transition,
                        C1OrdinaryStageTransitionPhase.FadeIn);
                    break;
                case C1OrdinaryStageTransitionPhase.FadeIn:
                    float fadeInT = Mathf.Clamp01(
                        elapsed / TransitionFadeInSeconds);
                    SetTransitionFadeAlpha(1f - fadeInT, true);
                    if (fadeInT >= 1f)
                    {
                        TryAdvanceTransitionPhase(
                            transition,
                            C1OrdinaryStageTransitionPhase.PlayerEntry);
                    }
                    break;
                case C1OrdinaryStageTransitionPhase.PlayerEntry:
                    float entryT = Mathf.Clamp01(
                        elapsed / TransitionPlayerEntrySeconds);
                    AnimatePlayerRun(
                        transitionPlayerEntryStartWorldPosition,
                        transitionPlayerAuthoredWorldPosition,
                        entryT);
                    if (entryT >= 1f)
                    {
                        TryAdvanceTransitionPhase(
                            transition,
                            C1OrdinaryStageTransitionPhase.EnemyReveal);
                    }
                    break;
                case C1OrdinaryStageTransitionPhase.EnemyReveal:
                    float revealT = Mathf.Clamp01(
                        elapsed / TransitionEnemyRevealSeconds);
                    if (EncounterAdmission?.IsOrdinary == true)
                    {
                        SetOrdinarySlotAlpha(revealT);
                    }
                    if (revealT >= 1f)
                    {
                        flowController?.TryCompleteOrdinaryStageTransition(
                            transition.Sequence);
                    }
                    break;
            }
        }

        private void AnimatePlayerRun(
            Vector3 fromWorld,
            Vector3 toWorld,
            float t)
        {
            if (authoredPlayerSlot == null)
            {
                return;
            }
            float bob = Mathf.Sin(t * Mathf.PI * 4f) * 10f;
            authoredPlayerSlot.rectTransform.position =
                Vector3.Lerp(fromWorld, toWorld, t)
                + new Vector3(0f, bob, 0f);
        }

        private bool TryAdvanceTransitionPhase(
            C1OrdinaryStageTransitionSnapshot transition,
            C1OrdinaryStageTransitionPhase nextPhase)
        {
            if (transition == null
                || transition.Sequence != activeTransitionSequence
                || flowController?.TryAdvanceOrdinaryStageTransitionPhase(
                        transition.Sequence,
                        nextPhase) != true)
            {
                return false;
            }
            activeTransitionPhase = nextPhase;
            activeTransitionPhaseStartedAt = Time.unscaledTime;
            PrepareTransitionPhase(
                flowController?.OrdinaryStageTransition ?? transition);
            return true;
        }

        private void EnsureTransitionRuntime()
        {
            if (transitionRuntimeRoot != null)
            {
                return;
            }
            Canvas canvas = authoredPlayerSlot?.canvas
                ?? authoredOrdinarySlot?.canvas;
            if (canvas == null)
            {
                return;
            }
            if (transitionSolidSprite == null)
            {
                transitionSolidSprite = CreateSolidSprite();
            }
            if (transitionGlowSprite == null)
            {
                transitionGlowSprite = CreateGlowSprite();
            }
            GameObject rootObject = new(
                TransitionRuntimeRootName,
                typeof(RectTransform));
            rootObject.hideFlags = HideFlags.DontSave;
            transitionRuntimeRoot =
                rootObject.GetComponent<RectTransform>();
            transitionRuntimeRoot.SetParent(canvas.transform, false);
            StretchRect(transitionRuntimeRoot);
            transitionBackdropImage = CreateTransitionImage(
                "SegmentBackdrop",
                transitionRuntimeRoot,
                transitionSolidSprite,
                new Color(0f, 0f, 0f, 0f),
                stretch: true);
            transitionBackdropImage.transform.SetSiblingIndex(0);
            transitionExitGlowImage = CreateTransitionImage(
                "ExitGlow",
                transitionRuntimeRoot,
                transitionGlowSprite,
                new Color(1f, 0.78f, 0.36f, 0f),
                stretch: false);
            transitionExitGlowImage.rectTransform.sizeDelta =
                new Vector2(180f, 180f);
            transitionFadeImage = CreateTransitionImage(
                "FullScreenFade",
                transitionRuntimeRoot,
                transitionSolidSprite,
                new Color(0f, 0f, 0f, 0f),
                stretch: true);
            transitionFadeImage.raycastTarget = false;
            transitionFadeImage.transform.SetAsLastSibling();
        }

        private void TeardownTransitionRuntime()
        {
            activeTransitionSequence = 0L;
            activeTransitionPhase = C1OrdinaryStageTransitionPhase.None;
            activeTransitionPhaseStartedAt = 0f;
            if (transitionRuntimeRoot != null)
            {
                Destroy(transitionRuntimeRoot.gameObject);
            }
            transitionRuntimeRoot = null;
            transitionBackdropImage = null;
            transitionFadeImage = null;
            transitionExitGlowImage = null;
        }

        private void ApplyTransitionBackdropForStage(string stageId)
        {
            if (transitionBackdropImage == null)
            {
                return;
            }
            C1JourneyStageBeat beat =
                C1JourneyPresentationProfile.Find(stageId);
            Color color = beat?.segmentId switch
            {
                var value when value ==
                    C1JourneyPresentationProfile.SegmentA =>
                    new Color(0.16f, 0.23f, 0.29f, 0.22f),
                var value when value ==
                    C1JourneyPresentationProfile.SegmentB =>
                    new Color(0.34f, 0.24f, 0.17f, 0.24f),
                _ => new Color(0.20f, 0.18f, 0.24f, 0.26f)
            };
            transitionBackdropImage.color = color;
        }

        private void SetTransitionFadeAlpha(float alpha, bool blocksInput)
        {
            if (transitionFadeImage == null)
            {
                return;
            }
            Color color = transitionFadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            transitionFadeImage.color = color;
            transitionFadeImage.raycastTarget = blocksInput;
            transitionFadeImage.enabled = color.a > 0.0001f || blocksInput;
        }

        private void SetExitGlowAlpha(float alpha)
        {
            if (transitionExitGlowImage == null)
            {
                return;
            }
            Color color = transitionExitGlowImage.color;
            color.a = Mathf.Clamp01(alpha);
            transitionExitGlowImage.color = color;
            transitionExitGlowImage.enabled = color.a > 0.0001f;
        }

        private void PositionExitGlow(Vector3 worldPosition)
        {
            if (transitionExitGlowImage == null)
            {
                return;
            }
            transitionExitGlowImage.rectTransform.position = worldPosition;
        }

        private void SetPlayerAlpha(float alpha)
        {
            if (authoredPlayerSlot == null)
            {
                return;
            }
            Color color = playerOriginalColor;
            color.a *= Mathf.Clamp01(alpha);
            authoredPlayerSlot.color = color;
        }

        private void SetOrdinarySlotAlpha(float alpha)
        {
            if (authoredMultiEnemyPresentationRoot != null
                || authoredOrdinarySlot == null
                || !authoredOrdinarySlot.gameObject.activeSelf)
            {
                return;
            }
            Color color = ordinaryOriginalColor;
            color.a *= Mathf.Clamp01(alpha);
            authoredOrdinarySlot.color = color;
        }

        private void ResolveAuthoredMultiEnemyPresentationRoot()
        {
            if (authoredMultiEnemyPresentationRoot != null)
            {
                return;
            }
            C1AuthoredEnemyPresentationRoot[] roots =
                Resources.FindObjectsOfTypeAll<
                        C1AuthoredEnemyPresentationRoot>()
                    .Where(value => value != null
                        && value.gameObject.scene == gameObject.scene)
                    .ToArray();
            if (roots.Length == 1)
            {
                authoredMultiEnemyPresentationRoot = roots[0];
            }
        }

        private Vector3 ResolveTransitionExitWorldPosition()
        {
            Vector3 origin = authoredOrdinarySlot != null
                ? authoredOrdinarySlot.rectTransform.position
                : playerOriginalWorldPosition;
            return origin + new Vector3(260f, 80f, 0f);
        }

        private Vector3 ResolvePlayerEntryStartWorldPosition()
        {
            return playerOriginalWorldPosition + new Vector3(
                -900f,
                0f,
                0f);
        }

        private static Sprite CreateSolidSprite()
        {
            Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.DontSave;
            texture.SetPixels(new[]
            {
                Color.white, Color.white, Color.white, Color.white
            });
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static Sprite CreateGlowSprite()
        {
            const int size = 64;
            Texture2D texture = new(
                size,
                size,
                TextureFormat.RGBA32,
                false);
            texture.hideFlags = HideFlags.DontSave;
            Vector2 center = new(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance =
                        Vector2.Distance(new Vector2(x, y), center) / radius;
                    float alpha = Mathf.Clamp01(1f - distance);
                    texture.SetPixel(
                        x,
                        y,
                        new Color(1f, 1f, 1f, alpha * alpha));
                }
            }
            texture.Apply(false, true);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static Image CreateTransitionImage(
            string objectName,
            RectTransform parent,
            Sprite sprite,
            Color color,
            bool stretch)
        {
            GameObject imageObject = new(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.hideFlags = HideFlags.DontSave;
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            if (stretch)
            {
                StretchRect(rect);
            }
            else
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        private static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private void LoadFallbackFrames()
        {
            foreach (C1JourneyEnemyVisualState state in
                C1JourneyPresentationProfile.FiveEnemyStates)
            {
                Texture2D[] frames = Resources.LoadAll<Texture2D>(
                        C1JourneyPresentationProfile
                            .GetFallbackStateResourcePath(state))
                    .Where(texture => texture != null
                        && texture.width > 0
                        && texture.height > 0)
                    .OrderBy(texture => texture.name, StringComparer.Ordinal)
                    .ToArray();
                TextAsset metadata = Resources.Load<TextAsset>(
                    C1JourneyPresentationProfile
                        .GetFallbackStateResourcePath(state)
                    + "/frames");
                fallbackClips[state] = new FallbackAnimationClip(
                    frames,
                    ParseFrameSequenceMetadata(metadata));
            }

            int missing = fallbackClips.Count(pair =>
                pair.Value == null || !pair.Value.HasFrames);
            Diagnostic = missing == 0
                ? "D1_3_FIVE_STATE_FRAMES_READY"
                : "D1_3_STATE_FRAME_GROUPS_MISSING_" + missing;
        }

        private Sprite CurrentFallbackSprite()
        {
            if (!fallbackClips.TryGetValue(
                    CurrentState,
                    out FallbackAnimationClip clip)
                || clip == null
                || !clip.HasFrames)
            {
                return null;
            }
            int frameIndex = clip.ResolveFrameIndex(
                Mathf.Max(0f, Time.unscaledTime - visualStateStartedAt),
                CurrentState == C1JourneyEnemyVisualState.Idle);
            Texture2D texture = clip.Frames[frameIndex];
            if (runtimeFallbackSprites.TryGetValue(
                    texture,
                    out Sprite existing))
            {
                return existing;
            }

            Sprite created = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            created.name = "C1OrdinaryFallback_" + texture.name;
            created.hideFlags = HideFlags.DontSave;
            runtimeFallbackSprites.Add(texture, created);
            return created;
        }

        private void ResetVisualPlayback()
        {
            observedCueGeneration = 0;
            observedCueSequence = 0L;
            pendingAttackState = false;
            pendingSkillState = false;
            pendingHitState = false;
            preparePauseActive = false;
            StartVisualState(C1JourneyEnemyVisualState.Idle);
        }

        private void ObserveRuntimeCues(C1EnemyRuntimeSnapshot enemy)
        {
            if (enemy == null)
            {
                return;
            }
            if (observedCueGeneration != enemy.ResetGeneration)
            {
                observedCueGeneration = enemy.ResetGeneration;
                observedCueSequence = 0L;
                pendingAttackState = false;
                pendingSkillState = false;
                pendingHitState = false;
            }
            foreach (C1EnemyCueSnapshot cue in enemy.Cues
                .Where(value => value != null
                    && value.CueSequence > observedCueSequence)
                .OrderBy(value => value.CueSequence))
            {
                observedCueSequence = cue.CueSequence;
                if (!TryMapRuntimeCue(
                        cue.Kind,
                        out C1JourneyEnemyVisualState state))
                {
                    continue;
                }
                QueueObservedState(state);
            }
            if (enemy.Lifecycle == C1EnemyLifecycle.Defeated
                && CurrentState != C1JourneyEnemyVisualState.Death)
            {
                QueueObservedState(C1JourneyEnemyVisualState.Death);
            }
        }

        private void AdvanceVisualPlayback(C1EnemyRuntimeSnapshot enemy)
        {
            if (CurrentState == C1JourneyEnemyVisualState.Death)
            {
                return;
            }
            float now = Time.unscaledTime;
            if (CurrentState != C1JourneyEnemyVisualState.Idle
                && now < visualStateUntil)
            {
                return;
            }
            if (pendingHitState)
            {
                pendingHitState = false;
                StartVisualState(C1JourneyEnemyVisualState.Hit);
                return;
            }
            if (pendingAttackState)
            {
                pendingAttackState = false;
                StartVisualState(C1JourneyEnemyVisualState.BasicAttack);
                return;
            }
            if (pendingSkillState)
            {
                pendingSkillState = false;
                StartVisualState(C1JourneyEnemyVisualState.Skill);
                return;
            }
            if (CurrentState != C1JourneyEnemyVisualState.Idle
                || enemy == null)
            {
                StartVisualState(
                    enemy?.Lifecycle == C1EnemyLifecycle.Defeated
                        ? C1JourneyEnemyVisualState.Death
                        : C1JourneyEnemyVisualState.Idle);
            }
        }

        private void StartVisualState(C1JourneyEnemyVisualState state)
        {
            CurrentState = state;
            visualStateStartedAt = Time.unscaledTime;
            if (state == C1JourneyEnemyVisualState.Idle
                || state == C1JourneyEnemyVisualState.Death)
            {
                visualStateUntil = float.PositiveInfinity;
                return;
            }
            visualStateUntil = visualStateStartedAt
                + ResolveClipDurationSeconds(state);
        }

        private void QueueObservedState(
            C1JourneyEnemyVisualState state)
        {
            if (CurrentState == C1JourneyEnemyVisualState.Death)
            {
                return;
            }
            if (state == C1JourneyEnemyVisualState.Death)
            {
                pendingAttackState = false;
                pendingSkillState = false;
                pendingHitState = false;
                StartVisualState(C1JourneyEnemyVisualState.Death);
                return;
            }
            if (state == C1JourneyEnemyVisualState.Hit)
            {
                if (CurrentState == C1JourneyEnemyVisualState.BasicAttack
                    || CurrentState == C1JourneyEnemyVisualState.Skill)
                {
                    QueueActionDebt(CurrentState);
                    StartVisualState(C1JourneyEnemyVisualState.Hit);
                }
                else if (CurrentState == C1JourneyEnemyVisualState.Idle)
                {
                    StartVisualState(C1JourneyEnemyVisualState.Hit);
                }
                else
                {
                    pendingHitState = true;
                }
                return;
            }
            if (state == C1JourneyEnemyVisualState.BasicAttack
                || state == C1JourneyEnemyVisualState.Skill)
            {
                if (CurrentState == C1JourneyEnemyVisualState.Idle
                    && !pendingHitState
                    && !HasQueuedActionDebt())
                {
                    StartVisualState(state);
                }
                else
                {
                    QueueActionDebt(state);
                }
            }
        }

        private void PauseVisualPlaybackForPrepare(
            C1EnemyRuntimeSnapshot enemy)
        {
            if (!preparePauseActive)
            {
                SyncCueCursorToTail(enemy);
                pendingAttackState = false;
                pendingSkillState = false;
                pendingHitState = false;
                preparePauseActive = true;
            }
            if (CurrentState != C1JourneyEnemyVisualState.Idle)
            {
                StartVisualState(C1JourneyEnemyVisualState.Idle);
            }
        }

        private void ResumeVisualPlaybackAfterPrepare(
            C1EnemyRuntimeSnapshot enemy)
        {
            if (!preparePauseActive)
            {
                return;
            }
            preparePauseActive = false;
            SyncCueCursorToTail(enemy);
            pendingAttackState = false;
            pendingSkillState = false;
            pendingHitState = false;
            StartVisualState(C1JourneyEnemyVisualState.Idle);
        }

        private bool HasQueuedActionDebt()
        {
            return pendingAttackState || pendingSkillState;
        }

        private void QueueActionDebt(C1JourneyEnemyVisualState state)
        {
            if (state == C1JourneyEnemyVisualState.BasicAttack)
            {
                pendingAttackState = true;
                return;
            }
            if (state == C1JourneyEnemyVisualState.Skill)
            {
                pendingSkillState = true;
            }
        }

        private void SyncCueCursorToTail(C1EnemyRuntimeSnapshot enemy)
        {
            if (enemy == null)
            {
                return;
            }
            observedCueGeneration = enemy.ResetGeneration;
            observedCueSequence = enemy.LatestCue?.CueSequence ?? 0L;
        }

        private float ResolveClipDurationSeconds(
            C1JourneyEnemyVisualState state)
        {
            return fallbackClips.TryGetValue(state, out FallbackAnimationClip clip)
                && clip != null
                ? clip.TotalDurationSeconds
                : 0.1f;
        }

        private static FrameSequenceMetadata ParseFrameSequenceMetadata(
            TextAsset metadata)
        {
            return metadata == null
                ? null
                : JsonUtility.FromJson<FrameSequenceMetadata>(metadata.text);
        }

        private static bool TryMapRuntimeCue(
            C1EnemyCueKind kind,
            out C1JourneyEnemyVisualState state)
        {
            switch (kind)
            {
                case C1EnemyCueKind.BasicAttack:
                    state = C1JourneyEnemyVisualState.BasicAttack;
                    return true;
                case C1EnemyCueKind.ChargeAttack:
                case C1EnemyCueKind.Skill:
                    state = C1JourneyEnemyVisualState.Skill;
                    return true;
                case C1EnemyCueKind.Hit:
                case C1EnemyCueKind.ShellHit:
                    state = C1JourneyEnemyVisualState.Hit;
                    return true;
                case C1EnemyCueKind.Defeated:
                    state = C1JourneyEnemyVisualState.Death;
                    return true;
                default:
                    state = C1JourneyEnemyVisualState.Idle;
                    return false;
            }
        }

        private bool RequireFlowController(string action)
        {
            if (flowController != null)
            {
                return true;
            }
            Diagnostic = action + "_FLOW_NOT_BOUND";
            Debug.LogError(
                "[" + C1JourneyPresentationProfile.PackageId + "] "
                + Diagnostic);
            return false;
        }

        private void LogFlowAction(string action)
        {
            Diagnostic = action + "_"
                + flowController.LastDiagnosticCode;
            Debug.Log(
                "[" + C1JourneyPresentationProfile.PackageId + "] "
                + action + " -> " + FlowStatus);
        }

        [Serializable]
        private sealed class FrameSequenceMetadata
        {
            public int frame_count;
            public int frame_duration_ms;
            public int total_duration_ms;
            public FrameSequenceFrameMetadata[] frames;
        }

        [Serializable]
        private sealed class FrameSequenceFrameMetadata
        {
            public int duration_ms;
        }

        private sealed class FallbackAnimationClip
        {
            private readonly int[] cumulativeFrameDurationsMs;

            public FallbackAnimationClip(
                Texture2D[] frames,
                FrameSequenceMetadata metadata)
            {
                Frames = frames ?? Array.Empty<Texture2D>();
                if (Frames.Length == 0)
                {
                    cumulativeFrameDurationsMs = Array.Empty<int>();
                    TotalDurationSeconds = 0.1f;
                    return;
                }
                int[] durations = metadata?.frames != null
                    && metadata.frames.Length == Frames.Length
                    ? metadata.frames.Select(value =>
                            Math.Max(1, value?.duration_ms ?? 100))
                        .ToArray()
                    : Enumerable.Repeat(
                            Math.Max(
                                1,
                                metadata?.frame_duration_ms ?? 100),
                            Frames.Length)
                        .ToArray();
                cumulativeFrameDurationsMs = new int[durations.Length];
                int totalMs = 0;
                for (int i = 0; i < durations.Length; i++)
                {
                    totalMs += durations[i];
                    cumulativeFrameDurationsMs[i] = totalMs;
                }
                int declaredTotalMs = metadata?.total_duration_ms ?? 0;
                TotalDurationSeconds = Mathf.Max(
                    0.1f,
                    (declaredTotalMs > 0 ? declaredTotalMs : totalMs)
                    / 1000f);
            }

            public Texture2D[] Frames { get; }
            public bool HasFrames => Frames.Length > 0;
            public float TotalDurationSeconds { get; }

            public int ResolveFrameIndex(float elapsedSeconds, bool loop)
            {
                if (Frames.Length <= 1)
                {
                    return 0;
                }
                int totalMs = cumulativeFrameDurationsMs[
                    cumulativeFrameDurationsMs.Length - 1];
                if (totalMs <= 0)
                {
                    return 0;
                }
                int elapsedMs = Mathf.Max(
                    0,
                    Mathf.FloorToInt(elapsedSeconds * 1000f));
                if (loop)
                {
                    elapsedMs %= totalMs;
                }
                else if (elapsedMs >= totalMs)
                {
                    return Frames.Length - 1;
                }
                for (int i = 0; i < cumulativeFrameDurationsMs.Length; i++)
                {
                    if (elapsedMs < cumulativeFrameDurationsMs[i])
                    {
                        return i;
                    }
                }
                return Frames.Length - 1;
            }
        }
    }
}
