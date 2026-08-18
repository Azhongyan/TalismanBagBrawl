using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.BattleBridge.Formal;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items.Balance;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.Generation;
using TalismanBag.Navigation;
using TalismanBag.Presentation.FormalBattle;
using TalismanBag.Presentation.StageThemes;
using TalismanBag.UnifiedBattle.Presentation.ExactItemDetail;
using TalismanBag.UnifiedBattle.Presentation.ExactNavigation;
using TalismanBag.UnifiedBattle.Presentation.ExactPrepare;
using TalismanBag.UnifiedBattle.Presentation.Reward;
using TalismanBag.UnifiedBattle.StageThemes;
using TalismanBag.V04.Campaign.Chapter1;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.UnifiedBattle
{
    [DisallowMultipleComponent]
    public sealed class UnifiedBattleFormalSceneHost : MonoBehaviour
    {
        private const string ItemSourceCatalogInvalidDiagnostic =
            "UNIFIED_FORMAL_ITEM_SOURCE_CATALOG_INVALID";
        private const string ItemSourceProviderBindingInvalidDiagnostic =
            "UNIFIED_FORMAL_ITEM_SOURCE_PROVIDER_BINDING_INVALID";
        private const string CanonicalItemCatalogSourceMissingDiagnostic =
            "UNIFIED_FORMAL_CANONICAL_ITEM_CATALOG_SOURCE_MISSING";
        private const string CanonicalItemCatalogInvalidDiagnostic =
            "UNIFIED_FORMAL_CANONICAL_ITEM_CATALOG_INVALID";

        [SerializeField] private UnifiedBattlePageShell shell;
        [SerializeField] private UnifiedBattlePageShellMarker marker;
        [SerializeField] private C1ExactBattleSandboxCombatLogView
            combatLogView;
        [SerializeField] private C1ExactBattleSandboxNavigationPresenter
            navigationPresenter;
        [SerializeField] private C1ExactBattleSandboxPrepareSurfacePresenter
            prepareSurfacePresenter;
        [SerializeField] private Image gameBackgroundImage;
        [SerializeField] private C1StageThemeBackgroundRegistry
            stageThemeBackgroundRegistry;
        [SerializeField] private C1StageThemeBackgroundPresenter
            stageThemeBackgroundPresenter;
        [SerializeField] private C1ExactBattleSandboxItemArrangementPresenter
            itemPresenter;
        [SerializeField] private C1ExactBattleSandboxItemDetailPresenter
            itemDetailPresenter;
        [SerializeField] private C1ExactBattleSandboxItemBoardView itemBoardView;
        [SerializeField] private C1ExactBattleSandboxItemTrayView itemTrayView;
        [SerializeField] private FormalBattlePresentationRoot
            formalPresentationRoot;
        [SerializeField] private ItemBalanceWorkbenchCatalog
            canonicalItemCatalogSource;
        [SerializeField] private FormalBattleRewardPopupView rewardPopupView;

        private readonly List<C1FormalRealtimeBattleCue> acceptedCueViewBuffer =
            new List<C1FormalRealtimeBattleCue>();
        private bool navigationBound;
        private bool contextAdmissionAttempted;
        private bool arrangementSurfaceVisible;
        private int prepareLifecycleGeneration;
        private C1FormalPrepareSurfaceTransition prepareSurfaceTransition;
        private long prepareItemRefreshSequence;
        private string pendingPrepareItemRefreshCommandId = string.Empty;
        private bool battleLogOpen;
        private int lastPrimaryActionFrame = -1;
        private int playbackRate = 1;
        private long observedCueSequence;
        private BattleLaunchContext contextSnapshot;
        private C1FormalObtainRebuildBattleLoop formalLoop;
        private CanonicalItemDefinitionResolver canonicalItemResolver;
        private string acceptedStageId = string.Empty;
        private string currentSummary = string.Empty;
        private double pendingBattleMilliseconds;

        public string AcceptedStageId => acceptedStageId;
        public BattleLaunchContext ContextSnapshot => contextSnapshot;
        public C1FormalRealtimeBattleTerminalResult RealtimeTerminalSnapshot =>
            formalLoop == null ? null : formalLoop.CurrentRealtimeTerminal;
        public C1ExactBattleSandboxNavigationPresenter NavigationPresenter =>
            navigationPresenter;
        public C1ExactBattleSandboxPrepareSurfacePresenter
            PrepareSurfacePresenter => prepareSurfacePresenter;
        public Image GameBackgroundImage => gameBackgroundImage;
        public C1StageThemeBackgroundRegistry StageThemeBackgroundRegistry =>
            stageThemeBackgroundRegistry;
        public C1StageThemeBackgroundPresenter StageThemeBackgroundPresenter =>
            stageThemeBackgroundPresenter;
        public C1ExactBattleSandboxItemDetailPresenter ItemDetailPresenter =>
            itemDetailPresenter;
        public C1ExactBattleSandboxCombatLogView CombatLogView =>
            combatLogView;
        public CanonicalItemDefinitionResolver CanonicalItemResolver =>
            canonicalItemResolver;

        public bool ValidateNavigationAuthoredBinding()
        {
            return shell != null
                   && navigationPresenter != null
                   && navigationPresenter.transform.parent == shell.transform
                   && navigationPresenter.ValidateAuthoredReferences();
        }

        private void Awake()
        {
            BindNavigation();
            ApplyFormalDormantState();
        }

        private void OnEnable()
        {
            TryAcceptContextOnce();
        }

        private void Update()
        {
            AdvancePrepareSurfacePresentation();

            if (formalLoop == null
                || (formalLoop.Phase !=
                    C1FormalObtainRebuildBattlePhase.StageOneBattle
                    && formalLoop.Phase !=
                    C1FormalObtainRebuildBattlePhase.StageTwoBattle))
            {
                pendingBattleMilliseconds = 0d;
                return;
            }

            if (!C1FormalPrepareOrchestration.ShouldSubmitBattleAdvance(
                    arrangementSurfaceVisible
                    || prepareSurfaceTransition !=
                    C1FormalPrepareSurfaceTransition.None,
                    formalLoop.IsRealtimeBattlePaused))
            {
                pendingBattleMilliseconds = 0d;
                return;
            }

            pendingBattleMilliseconds += Math.Max(
                0d,
                Time.unscaledDeltaTime * 1000d * playbackRate);
            long deltaMs = (long)pendingBattleMilliseconds;
            if (deltaMs <= 0L)
            {
                return;
            }

            pendingBattleMilliseconds -= deltaMs;
            if (!formalLoop.TryAdvanceBy(deltaMs, out string diagnostic))
            {
                RenderPhase(diagnostic);
                return;
            }

            IReadOnlyList<C1FormalRealtimeBattleCue> emittedCues =
                TransportOrderedCuesUnchanged(
                    formalLoop.LastRealtimeTick == null
                        ? null
                        : formalLoop.LastRealtimeTick.emittedCues);
            CaptureAcceptedCues(emittedCues);

            if (!formalPresentationRoot.ConsumeRealtime(
                    formalLoop.RealtimeState,
                    emittedCues,
                    itemPresenter.Current,
                    out string presentationDiagnostic))
            {
                Debug.LogError(
                    "[UnifiedFormalHost] " + presentationDiagnostic,
                    formalPresentationRoot);
            }

            if (formalLoop.Phase ==
                C1FormalObtainRebuildBattlePhase.RewardClaimed)
            {
                itemPresenter.PublishCurrent();
            }

            if (formalLoop.Phase ==
                    C1FormalObtainRebuildBattlePhase.StageOneResult
                || formalLoop.Phase ==
                    C1FormalObtainRebuildBattlePhase.StageTwoResult
                || formalLoop.Phase ==
                    C1FormalObtainRebuildBattlePhase.RewardClaimed)
            {
                playbackRate = 1;
            }

            if (formalLoop.Phase ==
                    C1FormalObtainRebuildBattlePhase.StageOneResult
                || formalLoop.Phase ==
                    C1FormalObtainRebuildBattlePhase.StageTwoResult
                || formalLoop.Phase ==
                    C1FormalObtainRebuildBattlePhase.RewardClaimed)
            {
                ApplyBattleLockedSurfaceState();
            }

            RenderPhase();
        }

        private void OnDisable()
        {
            ClearFormalLoop();
            contextAdmissionAttempted = false;
        }

        private void OnDestroy()
        {
            ClearFormalLoop();
            UnbindNavigation();
            FormalBattleLaunchTransit.ClearResidualContext();
        }

        public bool ValidateAuthoredBindings(out string diagnostic)
        {
            if (shell == null || marker == null || combatLogView == null
                || navigationPresenter == null
                || prepareSurfacePresenter == null
                || gameBackgroundImage == null
                || stageThemeBackgroundRegistry == null
                || stageThemeBackgroundPresenter == null
                || itemPresenter == null
                || itemBoardView == null
                || itemTrayView == null || formalPresentationRoot == null
                || rewardPopupView == null)
            {
                diagnostic = "UNIFIED_FORMAL_HOST_AUTHORED_REFERENCE_MISSING";
                return false;
            }

            if (canonicalItemCatalogSource == null)
            {
                diagnostic = CanonicalItemCatalogSourceMissingDiagnostic;
                return false;
            }

            if (!stageThemeBackgroundRegistry.TryValidate(out diagnostic)
                || !stageThemeBackgroundPresenter.ValidateAuthoredReferences(
                    out diagnostic))
            {
                return false;
            }

            List<string> missingSlots = shell.CollectMissingRequiredSlots();
            if (missingSlots.Count > 0)
            {
                diagnostic = "UNIFIED_FORMAL_HOST_SHELL_SLOT_MISSING " +
                             string.Join(",", missingSlots);
                return false;
            }

            if (marker.DevOnly || !marker.IsEnabled || !marker.FormalFlow
                || !marker.ConnectedToFormalRoute)
            {
                diagnostic = "UNIFIED_FORMAL_HOST_MARKER_NOT_FORMAL";
                return false;
            }

            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
            C1StageThemeBackgroundPresenter[] backgroundPresenters =
                shell.GetComponentsInChildren<
                    C1StageThemeBackgroundPresenter>(true);
            if (!slots.TryGetValue(
                    UnifiedBattlePageShellSlotNames.EnemyInfoArea,
                    out Transform enemyInfoSlot)
                || !slots.TryGetValue(
                    UnifiedBattlePageShellSlotNames.BattleFeedbackLayer,
                    out Transform feedbackSlot))
            {
                diagnostic = "UNIFIED_FORMAL_SHELL_RUNTIME_SLOT_RESOLUTION_INVALID";
                return false;
            }

            if (prepareSurfacePresenter.transform.parent != shell.transform
                || gameBackgroundImage.transform.parent != shell.transform
                || gameBackgroundImage.raycastTarget
                || backgroundPresenters.Length != 1
                || backgroundPresenters[0] != stageThemeBackgroundPresenter
                || stageThemeBackgroundPresenter.gameObject !=
                    gameBackgroundImage.gameObject
                || stageThemeBackgroundPresenter.TargetImage !=
                    gameBackgroundImage
                || !(gameBackgroundImage.transform is RectTransform
                    backgroundRect)
                || backgroundRect.sizeDelta != new Vector2(1300f, 900f))
            {
                diagnostic = "UNIFIED_FORMAL_PREPARE_BACKGROUND_BINDING_INVALID";
                return false;
            }

            if (itemPresenter.transform != prepareSurfacePresenter.transform
                || !itemBoardView.transform.IsChildOf(
                    prepareSurfacePresenter.transform)
                || !itemTrayView.transform.IsChildOf(
                    prepareSurfacePresenter.transform))
            {
                diagnostic = "UNIFIED_FORMAL_ITEM_SURFACE_HIERARCHY_INVALID";
                return false;
            }

            if (formalPresentationRoot.transform.parent != enemyInfoSlot
                || formalPresentationRoot.DamageFloatPool == null
                || formalPresentationRoot.DamageFloatPool.transform.parent
                    != feedbackSlot
                || formalPresentationRoot.CueFxAudioRoot == null
                || formalPresentationRoot.CueFxAudioRoot.transform.parent
                    != feedbackSlot)
            {
                diagnostic = "UNIFIED_FORMAL_PRESENTATION_HIERARCHY_INVALID";
                return false;
            }

            if (combatLogView.transform.parent != shell.transform
                || !combatLogView.ValidateAuthoredReferences())
            {
                diagnostic = "UNIFIED_FORMAL_COMBAT_LOG_BINDING_INVALID";
                return false;
            }

            if (!itemBoardView.TryResolvePresentationCatalog(
                    out _,
                    out string itemPresentationCatalogDiagnostic))
            {
                diagnostic =
                    "UNIFIED_FORMAL_ITEM_PRESENTATION_CATALOG_INVALID " +
                    itemPresentationCatalogDiagnostic;
                return false;
            }

            if (!itemPresenter.ValidateAuthoredReferences())
            {
                diagnostic = "UNIFIED_FORMAL_ITEM_PRESENTER_BINDING_INVALID";
                return false;
            }

            if (!prepareSurfacePresenter.ValidateAuthoredReferences()
                || prepareSurfacePresenter.View.BoardView != itemBoardView
                || prepareSurfacePresenter.View.TrayView != itemTrayView)
            {
                diagnostic = "UNIFIED_FORMAL_PREPARE_SURFACE_BINDING_INVALID";
                return false;
            }

            if (!ValidateNavigationAuthoredBinding())
            {
                diagnostic = "UNIFIED_FORMAL_NAVIGATION_BINDING_INVALID";
                return false;
            }

            if (rewardPopupView.transform.parent != shell.transform
                || !rewardPopupView.ValidateAuthoredReferences())
            {
                diagnostic = "UNIFIED_FORMAL_REWARD_POPUP_BINDING_INVALID";
                return false;
            }

            if (!formalPresentationRoot.ValidateAuthoredReferences())
            {
                diagnostic = "UNIFIED_FORMAL_PRESENTATION_ROOT_BINDING_INVALID";
                return false;
            }

            if (EvaluateItemDetailAdmission(out string itemDetailDiagnostic) !=
                C1FormalItemDetailAdmissionState.Ready)
            {
                diagnostic = itemDetailDiagnostic;
                return false;
            }

            diagnostic = "UNIFIED_FORMAL_HOST_AUTHORED_BINDINGS_VALID";
            return true;
        }

        public void AssignCoreForEditor(
            UnifiedBattlePageShell configuredShell,
            UnifiedBattlePageShellMarker configuredMarker,
            C1ExactBattleSandboxCombatLogView configuredCombatLogView)
        {
            shell = configuredShell;
            marker = configuredMarker;
            combatLogView = configuredCombatLogView;
        }

        // Temporary compile shims for the REV11 DELETE inventory. The retiring
        // authoring paths are not callable from a menu and are deleted after the
        // new Shell-only authoring pass validates.
        public void AssignForEditor(
            UnifiedBattlePageShell configuredShell,
            UnifiedBattlePageShellMarker configuredMarker,
            Text configuredSummaryText,
            Button configuredReturnButton)
        {
            shell = configuredShell;
            marker = configuredMarker;
        }

        public void AssignForEditor(
            UnifiedBattlePageShell configuredShell,
            UnifiedBattlePageShellMarker configuredMarker,
            Text configuredSummaryText,
            Button configuredPrimaryButton,
            Text configuredPrimaryActionLabel,
            C1ExactBattleSandboxItemArrangementPresenter configuredItemPresenter,
            C1ExactBattleSandboxItemBoardView configuredItemBoardView,
            C1ExactBattleSandboxItemTrayView configuredItemTrayView)
        {
            shell = configuredShell;
            marker = configuredMarker;
            itemPresenter = configuredItemPresenter;
            itemBoardView = configuredItemBoardView;
            itemTrayView = configuredItemTrayView;
        }

        public void AssignPresentationForEditor(
            FormalBattlePresentationRoot configuredPresentationRoot)
        {
            formalPresentationRoot = configuredPresentationRoot;
        }

        public void AssignRewardPopupForEditor(
            FormalBattleRewardPopupView configuredRewardPopupView)
        {
            rewardPopupView = configuredRewardPopupView;
        }

        public void AssignNavigationForEditor(
            C1ExactBattleSandboxNavigationPresenter configuredPresenter)
        {
            navigationPresenter = configuredPresenter;
        }

        public void AssignPrepareSurfaceForEditor(
            C1ExactBattleSandboxPrepareSurfacePresenter configuredPresenter,
            Image configuredGameBackground,
            C1ExactBattleSandboxItemArrangementPresenter configuredItemPresenter,
            C1ExactBattleSandboxItemBoardView configuredBoardView,
            C1ExactBattleSandboxItemTrayView configuredTrayView)
        {
            prepareSurfacePresenter = configuredPresenter;
            gameBackgroundImage = configuredGameBackground;
            itemPresenter = configuredItemPresenter;
            itemBoardView = configuredBoardView;
            itemTrayView = configuredTrayView;
        }

        public void AssignStageThemeBackgroundForEditor(
            C1StageThemeBackgroundRegistry configuredRegistry,
            C1StageThemeBackgroundPresenter configuredPresenter)
        {
            stageThemeBackgroundRegistry = configuredRegistry;
            stageThemeBackgroundPresenter = configuredPresenter;
        }

        public void AssignItemDetailForEditor(
            C1ExactBattleSandboxItemDetailPresenter configuredPresenter)
        {
            itemDetailPresenter = configuredPresenter;
        }

        public void AssignCanonicalItemCatalogForEditor(
            ItemBalanceWorkbenchCatalog configuredCatalog)
        {
            canonicalItemCatalogSource = configuredCatalog;
        }

        private void TryAcceptContextOnce()
        {
            if (contextAdmissionAttempted)
            {
                return;
            }

            contextAdmissionAttempted = true;
            formalPresentationRoot?.UnbindItemSourceProvider();
            ResetStageThemePresentation();
            if (!ValidateAuthoredBindings(out string bindingDiagnostic))
            {
                SetSummary(bindingDiagnostic + "\n点击此槽返回世界地图");
                Debug.LogError("[UnifiedFormalHost] " + bindingDiagnostic, this);
                return;
            }

            if (!CanonicalItemDefinitionResolver.TryCreate(
                    canonicalItemCatalogSource,
                    out canonicalItemResolver,
                    out IReadOnlyList<string> canonicalErrors))
            {
                string canonicalDiagnostic =
                    CanonicalItemCatalogInvalidDiagnostic + " " +
                    string.Join(",", canonicalErrors ?? Array.Empty<string>());
                SetSummary(canonicalDiagnostic);
                Debug.LogError(
                    "[UnifiedFormalHost] " + canonicalDiagnostic,
                    this);
                return;
            }
            itemBoardView.BindCanonicalItemResolver(canonicalItemResolver);

            if (!FormalBattleLaunchTransit.TryConsume(
                    out BattleLaunchContext consumed,
                    out Chapter1CampaignStageConfig config,
                    out string consumeDiagnostic))
            {
                SetSummary(consumeDiagnostic + "\n未启动战斗或奖励；点击此槽返回世界地图");
                return;
            }

            if (!stageThemeBackgroundRegistry.TryResolve(
                    consumed.StageThemeProfileId,
                    out C1StageThemeBackgroundProfile resolvedThemeProfile,
                    out string themeResolutionDiagnostic))
            {
                ResetStageThemePresentation();
                SetSummary(
                    themeResolutionDiagnostic +
                    "\n正式战斗未启动；点击此槽返回世界地图");
                Debug.LogError(
                    "[UnifiedFormalHost] " + themeResolutionDiagnostic,
                    this);
                return;
            }

            if (!C1FormalObtainRebuildBattleLoop.TryCreate(
                    consumed,
                    config,
                    canonicalItemResolver,
                    out formalLoop,
                    out string loopDiagnostic))
            {
                ResetStageThemePresentation();
                SetSummary(
                    loopDiagnostic + "\n" +
                    "正式战斗未产生存档或奖励命令；点击此槽返回世界地图");
                Debug.LogError("[UnifiedFormalHost] " + loopDiagnostic, this);
                return;
            }

            C1FormalItemDetailAdmissionState itemDetailState =
                EvaluateItemDetailAdmission(out string itemDetailStateDiagnostic);
            if (itemDetailState ==
                C1FormalItemDetailAdmissionState.PartialOrInvalid)
            {
                ResetStageThemePresentation();
                formalLoop.Reset();
                formalLoop = null;
                SetSummary(itemDetailStateDiagnostic);
                Debug.LogError(
                    "[UnifiedFormalHost] " + itemDetailStateDiagnostic,
                    this);
                return;
            }

            if (!itemPresenter.Bind(
                    formalLoop.ItemAuthority,
                    out string presenterDiagnostic))
            {
                ResetStageThemePresentation();
                formalLoop.Reset();
                formalLoop = null;
                SetSummary(presenterDiagnostic + "\n正式 Item 装配未启动");
                Debug.LogError("[UnifiedFormalHost] " + presenterDiagnostic, this);
                return;
            }

            if (itemDetailState == C1FormalItemDetailAdmissionState.Ready
                && !itemDetailPresenter.Bind(
                    itemPresenter,
                    out string itemDetailDiagnostic))
            {
                ResetStageThemePresentation();
                itemPresenter.Unbind();
                formalLoop.Reset();
                formalLoop = null;
                SetSummary(itemDetailDiagnostic +
                           "\n正式 Item 详情未启动");
                Debug.LogError(
                    "[UnifiedFormalHost] " + itemDetailDiagnostic,
                    this);
                return;
            }

            if (!stageThemeBackgroundPresenter.Apply(
                    resolvedThemeProfile,
                    out string themeApplyDiagnostic))
            {
                formalPresentationRoot.UnbindItemSourceProvider();
                itemDetailPresenter?.Unbind();
                itemPresenter.Unbind();
                formalLoop.Reset();
                formalLoop = null;
                ResetStageThemePresentation();
                SetSummary(
                    themeApplyDiagnostic +
                    "\n正式背景应用失败；点击此槽返回世界地图");
                Debug.LogError(
                    "[UnifiedFormalHost] " + themeApplyDiagnostic,
                    this);
                return;
            }

            if (!TryBindFormalItemSourceProvider(
                    out string itemSourceDiagnostic))
            {
                ClearFormalLoop();
                SetSummary(itemSourceDiagnostic);
                Debug.LogError(
                    "[UnifiedFormalHost] " + itemSourceDiagnostic,
                    this);
                return;
            }

            contextSnapshot = formalLoop.StageOneContext;
            acceptedStageId = consumed.StageId;
            ApplyFormalInteractiveState();
            ResetAcceptedCueView(formalLoop.CurrentRealtimeStart);
            if (!formalPresentationRoot.BeginRealtime(
                    formalLoop.CurrentRealtimeStart,
                    itemPresenter.Current,
                    acceptedStageId,
                    out string presentationDiagnostic))
            {
                ClearFormalLoop();
                SetSummary(presentationDiagnostic);
                Debug.LogError(
                    "[UnifiedFormalHost] " + presentationDiagnostic,
                    formalPresentationRoot);
                return;
            }
            RenderPhase();
        }

        private C1FormalItemDetailAdmissionState EvaluateItemDetailAdmission(
            out string diagnostic)
        {
            Transform popupSlot = shell == null
                ? null
                : shell.ItemDetailPopupSlot;
            bool presenterPresent = itemDetailPresenter != null;
            bool slotPresent = popupSlot != null;
            return C1FormalItemDetailAdmissionGate.Evaluate(
                presenterPresent,
                slotPresent,
                presenterPresent && slotPresent
                    && itemDetailPresenter.transform == popupSlot,
                presenterPresent
                    && itemDetailPresenter.ValidateAuthoredReferences(),
                out diagnostic);
        }

        private void ApplyFormalDormantState()
        {
            formalPresentationRoot?.UnbindItemSourceProvider();
            ResetStageThemePresentation();
            ResetPrepareSurfaceForLifecycle();
            if (shell == null)
            {
                return;
            }

            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
            SetSlotActive(slots, UnifiedBattlePageShellSlotNames.EnemyInfoArea, false);
            SetSlotActive(slots, UnifiedBattlePageShellSlotNames.BattleFeedbackLayer, false);
            arrangementSurfaceVisible = false;
            battleLogOpen = false;
            playbackRate = 1;
            acceptedCueViewBuffer.Clear();
            observedCueSequence = 0L;
            navigationPresenter?.ResetPresentation();
            SetPrimaryActionSurfaceVisible(true);
            rewardPopupView?.Hide();
            itemDetailPresenter?.ResetPresentation();
            combatLogView?.ResetPresentation();
        }

        private void ApplyFormalInteractiveState()
        {
            ResetPrepareSurfaceForLifecycle();
            if (shell == null)
            {
                return;
            }

            IReadOnlyDictionary<string, Transform> slots = shell.BuildSlotMap();
            SetSlotActive(slots, UnifiedBattlePageShellSlotNames.EnemyInfoArea, true);
            SetSlotActive(slots, UnifiedBattlePageShellSlotNames.BattleFeedbackLayer, true);
            SetPrimaryActionSurfaceVisible(true);
            rewardPopupView?.Hide();
            ApplyItemInteractionMode(C1FormalItemInteractionMode.BATTLE_LOCKED);
            RenderNavigationAuxiliary();
        }

        private void BindNavigation()
        {
            if (navigationBound || navigationPresenter == null)
            {
                return;
            }

            if (!navigationPresenter.Bind(
                    HandleBackAction,
                    HandlePrimaryAction,
                    HandleArrangementToggle,
                    HandleSpeedToggle,
                    HandleBattleLogToggle,
                    out string diagnostic))
            {
                Debug.LogError("[UnifiedFormalHost] " + diagnostic, this);
                return;
            }

            if (rewardPopupView == null)
            {
                navigationPresenter.Unbind();
                Debug.LogError(
                    "[UnifiedFormalHost] FORMAL_REWARD_POPUP_BINDING_MISSING",
                    this);
                return;
            }

            if (!rewardPopupView.Bind(
                    HandleRewardPopupAction,
                    out string rewardPopupDiagnostic))
            {
                navigationPresenter.Unbind();
                Debug.LogError(
                    "[UnifiedFormalHost] " + rewardPopupDiagnostic,
                    this);
                return;
            }

            navigationBound = true;
        }

        private void UnbindNavigation()
        {
            if (navigationBound && navigationPresenter != null)
            {
                navigationPresenter.Unbind();
            }

            rewardPopupView?.Unbind();

            navigationBound = false;
        }

        private void HandleBackAction()
        {
            TryReturnWorldMap();
        }

        private void HandlePrimaryAction()
        {
            if (lastPrimaryActionFrame == Time.frameCount)
            {
                return;
            }

            lastPrimaryActionFrame = Time.frameCount;
            if (formalLoop == null)
            {
                return;
            }

            CloseBattleLog();

            switch (formalLoop.Phase)
            {
                case C1FormalObtainRebuildBattlePhase.StageOneBattle:
                case C1FormalObtainRebuildBattlePhase.StageTwoBattle:
                    RenderPhase("C1_FORMAL_LOOP_BATTLE_STILL_RUNNING");
                    break;
                case C1FormalObtainRebuildBattlePhase.StageOneResult:
                case C1FormalObtainRebuildBattlePhase.StageTwoResult:
                    if (formalLoop.CurrentRealtimeTerminal != null
                        && formalLoop.CurrentRealtimeTerminal.accepted
                        && formalLoop.CurrentRealtimeTerminal.lose)
                    {
                        TryReturnWorldMap();
                        break;
                    }

                    RenderPhase(
                        C1FormalObtainRebuildBattleLoop.PhaseRejectedDiagnostic);
                    break;
                case C1FormalObtainRebuildBattlePhase.RewardClaimed:
                    HandleRewardPopupAction();
                    break;
                case C1FormalObtainRebuildBattlePhase.RebuildForStageTwo:
                    if (prepareSurfacePresenter != null
                        && prepareSurfacePresenter.State ==
                        C1ExactBattleSandboxPrepareSurfaceState.Open)
                    {
                        if (!BeginRebuildCloseForStageTwo(
                                out string closeDiagnostic))
                        {
                            RenderPhase(closeDiagnostic);
                        }

                        break;
                    }

                    if (prepareSurfaceTransition !=
                        C1FormalPrepareSurfaceTransition.None)
                    {
                        RenderPhase(
                            C1FormalPrepareOrchestration.PhaseRejectedDiagnostic);
                    }
                    else
                    {
                        TryStartStageTwoFromRebuild();
                    }
                    break;
                default:
                    RenderPhase("C1_FORMAL_LOOP_PHASE_REJECTED");
                    break;
            }
        }

        private void HandleRewardPopupAction()
        {
            if (formalLoop == null
                || formalLoop.Phase !=
                C1FormalObtainRebuildBattlePhase.RewardClaimed)
            {
                RenderPhase(
                    C1FormalObtainRebuildBattleLoop.PhaseRejectedDiagnostic);
                return;
            }

            CloseBattleLog();
            if (prepareSurfaceTransition !=
                C1FormalPrepareSurfaceTransition.None)
            {
                RenderPhase(
                    C1FormalPrepareOrchestration.PhaseRejectedDiagnostic);
                return;
            }

            if (!TryOpenRebuildArrangement(
                    out string rewardArrangementDiagnostic))
            {
                RenderPhase(rewardArrangementDiagnostic);
                return;
            }

            if (!formalLoop.TryBeginRewardArrangement(
                    out string rebuildDiagnostic))
            {
                ResetPrepareSurfaceForLifecycle();
                RenderPhase(rebuildDiagnostic);
                return;
            }

            RenderPhase(rebuildDiagnostic);
        }

        private void TryReturnWorldMap()
        {
            if (!TalismanSceneNavigationOwner.TryNavigate(
                    TalismanSceneRoute.WorldMap,
                    this))
            {
                if (formalLoop == null)
                {
                    SetSummary(
                        "UNIFIED_RETURN_NAVIGATION_REJECTED\n" +
                        "未启动正式流程；可再次点击返回");
                }
                else
                {
                    RenderPhase("UNIFIED_RETURN_NAVIGATION_REJECTED");
                }

                return;
            }

            ClearFormalLoop();
            FormalBattleLaunchTransit.ClearResidualContext();
        }

        private void ClearFormalLoop()
        {
            ResetStageThemePresentation();
            ApplyBattleLockedSurfaceState();

            if (formalPresentationRoot != null)
            {
                formalPresentationRoot.UnbindItemSourceProvider();
                formalPresentationRoot.ResetPresentation();
            }

            if (itemPresenter != null)
            {
                itemDetailPresenter?.Unbind();
                itemPresenter.Unbind();
            }

            if (formalLoop != null)
            {
                formalLoop.Reset();
                formalLoop = null;
            }
            contextSnapshot = null;
            acceptedStageId = string.Empty;
            currentSummary = string.Empty;
            pendingBattleMilliseconds = 0d;
            arrangementSurfaceVisible = false;
            battleLogOpen = false;
            playbackRate = 1;
            observedCueSequence = 0L;
            acceptedCueViewBuffer.Clear();
            navigationPresenter?.ResetPresentation();
            SetPrimaryActionSurfaceVisible(true);
            rewardPopupView?.Hide();
            itemDetailPresenter?.ResetPresentation();
            combatLogView?.ResetPresentation();
        }

        private void ResetStageThemePresentation()
        {
            stageThemeBackgroundPresenter?.ResetPresentation();
        }

        private void RenderPhase(string diagnostic = null)
        {
            if (formalLoop == null)
            {
                return;
            }

            C1FormalRealtimeBattleTerminalResult liveTerminal =
                formalLoop.CurrentRealtimeTerminal;
            if (liveTerminal != null
                && liveTerminal.accepted
                && liveTerminal.lose)
            {
                RenderDefeat(liveTerminal, diagnostic);
                return;
            }

            string headline;
            string instruction;
            switch (formalLoop.Phase)
            {
                case C1FormalObtainRebuildBattlePhase.StageOneBattle:
                case C1FormalObtainRebuildBattlePhase.StageTwoBattle:
                    RenderLiveBattle(diagnostic);
                    return;
                case C1FormalObtainRebuildBattlePhase.StageOneResult:
                case C1FormalObtainRebuildBattlePhase.StageTwoResult:
                    headline = formalLoop.CurrentStageId + " 正式战斗胜利";
                    instruction = "奖励自动接收未完成";
                    SetPrimaryActionLabel("奖励接收失败");
                    SetPrimaryActionEnabled(false);
                    SetPrimaryActionSurfaceVisible(false);
                    rewardPopupView?.Hide();
                    break;
                case C1FormalObtainRebuildBattlePhase.RewardClaimed:
                    C1CampaignStageRewardProgressResult claimedReward =
                        formalLoop.LastRewardProgress;
                    string claimedIdentity = claimedReward == null
                        ? "掉落身份不可用"
                        : claimedReward.selectedBaseItemId + " / "
                          + claimedReward.rarityIdentity;
                    headline = claimedIdentity + " 已领取";
                    instruction = "奖励已进入道具栏；点击领取道具进入整备";
                    SetPrimaryActionLabel("开始战斗");
                    SetPrimaryActionEnabled(true);
                    SetPrimaryActionSurfaceVisible(false);
                    if (!TryRenderClaimedRewardPopup(
                            out string rewardPopupDiagnostic))
                    {
                        rewardPopupView?.Hide();
                        SetPrimaryActionSurfaceVisible(true);
                        Debug.LogError(
                            "[UnifiedFormalHost] " + rewardPopupDiagnostic,
                            this);
                        diagnostic = string.IsNullOrEmpty(diagnostic)
                            ? rewardPopupDiagnostic
                            : diagnostic + "\n" + rewardPopupDiagnostic;
                    }
                    break;
                case C1FormalObtainRebuildBattlePhase.RebuildForStageTwo:
                    rewardPopupView?.Hide();
                    SetPrimaryActionSurfaceVisible(true);
                    C1CampaignStageRewardProgressResult reward =
                        formalLoop.LastRewardProgress;
                    string rewardIdentity = reward == null
                        ? "掉落身份不可用"
                        : reward.selectedBaseItemId + " / "
                          + reward.itemInstanceId + " / "
                          + reward.rarityIdentity;
                    headline = rewardIdentity + " 已加入正式道具栏";
                    instruction = formalLoop.ShouldReturnWorldMapAfterRebuild
                        ? "完成重摆后返回世界地图"
                        : "完成摆放后点击开始战斗";
                    SetPrimaryActionLabel(
                        formalLoop.ShouldReturnWorldMapAfterRebuild
                            ? "完成并返回"
                            : "开始战斗");
                    SetPrimaryActionEnabled(
                        prepareSurfaceTransition ==
                            C1FormalPrepareSurfaceTransition.None
                        && prepareSurfacePresenter != null
                        && prepareSurfacePresenter.State ==
                            C1ExactBattleSandboxPrepareSurfaceState.Open);
                    break;
                default:
                    return;
            }

            string resultLine = liveTerminal == null
                ? "结果不可用"
                : "结果：胜利 / "
                  + (liveTerminal.durationMs / 1000m).ToString(
                      "0.000",
                      CultureInfo.InvariantCulture)
                  + "秒 / 玩家生命 "
                  + liveTerminal.playerSnapshot.currentHp + "/"
                  + liveTerminal.playerSnapshot.maxHp + " / 敌方行动 "
                  + liveTerminal.enemyApplicationCount + " 次";
            RenderNavigationAuxiliary();
            SetSummary(
                headline + "\n" + resultLine + "\n" + instruction +
                (string.IsNullOrEmpty(diagnostic)
                    ? string.Empty
                    : "\n" + diagnostic));
        }

        private void RenderDefeat(
            C1FormalRealtimeBattleTerminalResult terminal,
            string diagnostic)
        {
            rewardPopupView?.Hide();
            SetPrimaryActionSurfaceVisible(true);
            SetPrimaryActionLabel("返回世界地图");
            SetPrimaryActionEnabled(true);
            RenderNavigationAuxiliary();
            SetSummary(
                terminal.stageId + " 正式战斗失败\n"
                + "结果：失败 / "
                + (terminal.durationMs / 1000m).ToString(
                    "0.000",
                    CultureInfo.InvariantCulture)
                + "秒 / 玩家生命 "
                + (terminal.playerSnapshot == null
                    ? "0/0"
                    : terminal.playerSnapshot.currentHp.ToString(
                          CultureInfo.InvariantCulture)
                      + "/" + terminal.playerSnapshot.maxHp.ToString(
                          CultureInfo.InvariantCulture))
                + "\n点击返回世界地图"
                + (string.IsNullOrEmpty(diagnostic)
                    ? string.Empty
                    : "\n" + diagnostic));
        }

        private void RenderLiveBattle(string diagnostic)
        {
            rewardPopupView?.Hide();
            SetPrimaryActionSurfaceVisible(true);
            C1FormalRealtimeBattleSessionStateSnapshot state =
                formalLoop.RealtimeState;
            if (state == null)
            {
                SetPrimaryActionLabel("战斗进行中");
                SetPrimaryActionEnabled(false);
                RenderNavigationAuxiliary();
                SetSummary("正式实时战斗状态不可用\n" + (diagnostic ?? string.Empty));
                return;
            }

            string headline = formalLoop.CurrentStageId + " 正式实时战斗";
            string actorLine = string.Join(" / ", state.actorSnapshots.Select(actor =>
                "敌" + (actor.stableActorOrder + 1).ToString(
                    CultureInfo.InvariantCulture) + "生命 " +
                actor.currentHp.ToString(CultureInfo.InvariantCulture) + "/" +
                actor.maxHp.ToString(CultureInfo.InvariantCulture)));
            C1FormalRealtimeBattleCue lastCue =
                acceptedCueViewBuffer.LastOrDefault();
            string cueLine = lastCue == null
                ? "事件：等待首个到期动作"
                : "事件：" + lastCue.cueKind + " / " + lastCue.sourceId;
            SetPrimaryActionLabel("战斗进行中");
            SetPrimaryActionEnabled(false);
            RenderNavigationAuxiliary();
            SetSummary(
                headline + "\n时间：" +
                (state.battleTimeMs / 1000m).ToString(
                    "0.000",
                    CultureInfo.InvariantCulture) + "秒" +
                " / 玩家生命 " + state.playerSnapshot.currentHp + "/" +
                state.playerSnapshot.maxHp + "\n" + actorLine + "\n" + cueLine +
                (string.IsNullOrEmpty(diagnostic)
                    || string.Equals(
                        diagnostic,
                        C1FormalObtainRebuildBattleLoop.LiveTickAcceptedDiagnostic,
                        StringComparison.Ordinal)
                    ? string.Empty
                    : "\n" + diagnostic));
        }

        private void SetPrimaryActionLabel(string value)
        {
            navigationPresenter?.RenderPrimary(
                value,
                navigationPresenter.View != null
                && navigationPresenter.View.PrimaryButton.interactable);
        }

        private void SetPrimaryActionEnabled(bool enabled)
        {
            string label = navigationPresenter?.View == null
                ? string.Empty
                : navigationPresenter.View.PrimaryLabelText;
            navigationPresenter?.RenderPrimary(label, enabled);
        }

        private void SetPrimaryActionSurfaceVisible(bool visible)
        {
            Button primary = navigationPresenter?.View == null
                ? null
                : navigationPresenter.View.PrimaryButton;
            if (primary != null && primary.gameObject.activeSelf != visible)
            {
                primary.gameObject.SetActive(visible);
            }
        }

        private bool TryRenderClaimedRewardPopup(out string diagnostic)
        {
            C1CampaignStageRewardProgressResult progress =
                formalLoop?.LastRewardProgress;
            var grant = progress?.grant;
            var generated = grant?.generatedItem;
            CanonicalItemDefinition definition = grant?.itemDefinition;
            if (progress == null || grant == null || generated == null
                || definition == null || rewardPopupView == null)
            {
                diagnostic = "FORMAL_REWARD_POPUP_CONTENT_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(progress.itemInstanceId)
                || !string.Equals(
                    progress.itemInstanceId,
                    generated.itemInstanceId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    progress.selectedBaseItemId,
                    definition.baseItemId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    generated.baseItemId,
                    definition.baseItemId,
                    StringComparison.Ordinal))
            {
                diagnostic = "FORMAL_REWARD_POPUP_EXACT_INSTANCE_MISMATCH";
                return false;
            }

            C1FormalItemPresentationCatalogSnapshot catalog =
                itemBoardView?.PresentationCatalogSnapshot;
            if (catalog == null
                || !catalog.TryResolveArtwork(
                    generated.baseItemId,
                    generated.rarity.ToStableKey(),
                    C1FormalItemArtworkLightingState.Unlit,
                    out _,
                    out Sprite artwork,
                    out _)
                || artwork == null)
            {
                diagnostic = "FORMAL_REWARD_POPUP_ARTWORK_MISSING";
                return false;
            }

            string effectDescription =
                definition.presentation?.basicEffectDescription;
            if (string.IsNullOrWhiteSpace(effectDescription))
            {
                effectDescription = definition.combatEffect?.description;
            }

            if (string.IsNullOrWhiteSpace(definition.displayName)
                || string.IsNullOrWhiteSpace(effectDescription))
            {
                diagnostic = "FORMAL_REWARD_POPUP_SEMANTIC_TEXT_MISSING";
                return false;
            }

            string effectName = definition.combatEffect?.displayName;
            string effectSummary = string.IsNullOrWhiteSpace(effectName)
                ? effectDescription.Trim()
                : effectName.Trim() + "\n" + effectDescription.Trim();
            rewardPopupView.RenderClaimed(
                progress.itemInstanceId,
                definition.displayName,
                generated.rarity.ToDisplayName(),
                effectSummary,
                artwork,
                generated.rarity);
            if (!string.Equals(
                    rewardPopupView.PresentedItemInstanceId,
                    progress.itemInstanceId,
                    StringComparison.Ordinal))
            {
                diagnostic = "FORMAL_REWARD_POPUP_PRESENTED_INSTANCE_MISMATCH";
                return false;
            }

            diagnostic = "FORMAL_REWARD_POPUP_RENDERED";
            return true;
        }

        private void SetSummary(string value)
        {
            currentSummary = value ?? string.Empty;
            RenderCombatLogCarrier();
        }

        private bool HandleArrangementToggle()
        {
            if (formalLoop == null || shell == null)
            {
                return arrangementSurfaceVisible;
            }

            CloseBattleLog();
            if (prepareSurfaceTransition !=
                C1FormalPrepareSurfaceTransition.None)
            {
                RenderPhase(C1FormalPrepareOrchestration.PhaseRejectedDiagnostic);
                return arrangementSurfaceVisible;
            }

            bool live = formalLoop.Phase ==
                            C1FormalObtainRebuildBattlePhase.StageOneBattle
                        || formalLoop.Phase ==
                            C1FormalObtainRebuildBattlePhase.StageTwoBattle;
            string diagnostic;
            if (live)
            {
                if (prepareSurfacePresenter.State ==
                    C1ExactBattleSandboxPrepareSurfaceState.Open)
                {
                    CloseLivePrepare(out diagnostic);
                }
                else if (prepareSurfacePresenter.State ==
                         C1ExactBattleSandboxPrepareSurfaceState.Closed
                         && formalLoop.IsRealtimeBattlePaused)
                {
                    TryResumeClosedPausedBattle(out diagnostic);
                }
                else
                {
                    OpenLivePrepare(out diagnostic);
                }

                RenderPhase(diagnostic);
                return arrangementSurfaceVisible;
            }

            if (formalLoop.Phase ==
                C1FormalObtainRebuildBattlePhase.RebuildForStageTwo)
            {
                if (prepareSurfacePresenter.State ==
                    C1ExactBattleSandboxPrepareSurfaceState.Open)
                {
                    BeginRebuildClose(false, out diagnostic);
                }
                else
                {
                    TryOpenRebuildArrangement(out diagnostic);
                }

                RenderPhase(diagnostic);
                return arrangementSurfaceVisible;
            }

            ApplyBattleLockedSurfaceState();
            RenderPhase(C1FormalPrepareOrchestration.PhaseRejectedDiagnostic);
            return arrangementSurfaceVisible;
        }

        private bool OpenLivePrepare(out string diagnostic)
        {
            arrangementSurfaceVisible = false;
            prepareSurfacePresenter.SetOpenInputEnabled(
                false,
                prepareLifecycleGeneration);
            C1FormalRealtimeBattleTickResult pauseTick = PauseRealtimeBattle();
            ConsumeRealtimeLifecycleTick(pauseTick);
            pendingBattleMilliseconds = 0d;
            if (!IsAcceptedPausedLifecycleTick(pauseTick))
            {
                ResetPrepareSurfaceForLifecycle();
                RenderNavigationAuxiliary();
                diagnostic = C1FormalPrepareOrchestration
                    .BattlePauseRejectedDiagnostic + " "
                    + LifecycleTickDiagnostic(pauseTick);
                return false;
            }

            C1FormalItemInteractionAuthorizationResult locked =
                ApplyItemInteractionMode(
                    C1FormalItemInteractionMode.BATTLE_LOCKED);
            if (locked == null || !locked.accepted
                || locked.interactionEnabled
                || !prepareSurfacePresenter.TryBeginOpen(
                    prepareLifecycleGeneration))
            {
                C1FormalRealtimeBattleTickResult compensation =
                    ResumeRealtimeBattle();
                ConsumeRealtimeLifecycleTick(compensation);
                ResetPrepareSurfaceForLifecycle();
                diagnostic = IsAcceptedRunningLifecycleTick(compensation)
                    ? C1FormalPrepareOrchestration.ItemLockRejectedDiagnostic
                    : C1FormalPrepareOrchestration
                          .CompensationResumeRejectedDiagnostic;
                RenderNavigationAuxiliary();
                return false;
            }

            prepareSurfaceTransition =
                C1FormalPrepareSurfaceTransition.OpeningLive;
            diagnostic = C1FormalPrepareParityDiagnostics.PullUpStarted;
            RenderNavigationAuxiliary();
            return true;
        }

        private bool CloseLivePrepare(out string diagnostic)
        {
            if (prepareSurfaceTransition ==
                    C1FormalPrepareSurfaceTransition.ClosingLive
                && pendingPrepareItemRefreshCommandId.Length > 0)
            {
                diagnostic = C1FormalPrepareParityDiagnostics.LowerStarted;
                return true;
            }

            C1FormalItemInteractionAuthorizationResult authorization =
                ApplyItemInteractionMode(
                    C1FormalItemInteractionMode.BATTLE_LOCKED);
            prepareSurfacePresenter.SetOpenInputEnabled(
                false,
                prepareLifecycleGeneration);
            pendingBattleMilliseconds = 0d;
            bool locked = IsAcceptedLockedAuthorization(authorization);
            if (!locked)
            {
                arrangementSurfaceVisible = false;
                diagnostic = C1FormalPrepareOrchestration
                                 .ItemLockRejectedDiagnostic + " "
                             + (authorization?.diagnostic ?? "RESULT_MISSING");
                RenderNavigationAuxiliary();
                return false;
            }

            pendingPrepareItemRefreshCommandId =
                NewPrepareItemRefreshCommandId();
            C1FormalRealtimeBattleItemRefreshResult refresh =
                RefreshRealtimeItemsWhilePaused(
                    pendingPrepareItemRefreshCommandId);
            if (refresh == null
                || !refresh.accepted
                || refresh.stateSnapshot == null
                || !refresh.stateSnapshot.paused)
            {
                pendingPrepareItemRefreshCommandId = string.Empty;
                arrangementSurfaceVisible = false;
                diagnostic = C1FormalPrepareOrchestration
                                 .BattleItemRefreshRejectedDiagnostic + " "
                             + (refresh?.errorCode ?? "RESULT_MISSING");
                RenderNavigationAuxiliary();
                return false;
            }

            ConsumeRealtimeItemRefresh(refresh);
            arrangementSurfaceVisible = false;
            if (!prepareSurfacePresenter.TryBeginClose(
                    prepareLifecycleGeneration))
            {
                pendingPrepareItemRefreshCommandId = string.Empty;
                diagnostic = C1FormalPrepareOrchestration.PhaseRejectedDiagnostic;
                RenderNavigationAuxiliary();
                return false;
            }

            prepareSurfaceTransition = C1FormalPrepareSurfaceTransition.ClosingLive;
            diagnostic = C1FormalPrepareParityDiagnostics.LowerStarted;
            RenderNavigationAuxiliary();
            return true;
        }

        private bool TryOpenRebuildArrangement(out string diagnostic)
        {
            C1FormalItemInteractionAuthorizationResult locked =
                ApplyItemInteractionMode(
                    C1FormalItemInteractionMode.BATTLE_LOCKED);
            arrangementSurfaceVisible = false;
            prepareSurfacePresenter.SetOpenInputEnabled(
                false,
                prepareLifecycleGeneration);
            if (!IsAcceptedLockedAuthorization(locked)
                || !prepareSurfacePresenter.TryBeginOpen(
                    prepareLifecycleGeneration))
            {
                ResetPrepareSurfaceForLifecycle();
                RenderNavigationAuxiliary();
                diagnostic = C1FormalPrepareOrchestration.ItemLockRejectedDiagnostic
                             + " "
                             + (locked?.diagnostic ?? "RESULT_MISSING");
                return false;
            }

            prepareSurfaceTransition =
                C1FormalPrepareSurfaceTransition.OpeningRebuild;
            RenderNavigationAuxiliary();
            diagnostic = C1FormalPrepareParityDiagnostics.PullUpStarted;
            return true;
        }

        private bool TryApplyBattleLockedSurfaceState(out string diagnostic)
        {
            C1FormalItemInteractionAuthorizationResult authorization =
                ApplyItemInteractionMode(
                    C1FormalItemInteractionMode.BATTLE_LOCKED);
            ResetPrepareSurfaceForLifecycle();
            RenderNavigationAuxiliary();
            if (!IsAcceptedLockedAuthorization(authorization))
            {
                diagnostic = C1FormalPrepareOrchestration
                    .ItemLockRejectedDiagnostic + " "
                    + (authorization?.diagnostic ?? "RESULT_MISSING");
                return false;
            }

            diagnostic = C1FormalPrepareOrchestration.PrepareClosedAcceptedDiagnostic;
            return true;
        }

        private void ApplyBattleLockedSurfaceState()
        {
            TryApplyBattleLockedSurfaceState(out _);
        }

        private C1FormalItemInteractionAuthorizationResult
            ApplyItemInteractionMode(C1FormalItemInteractionMode mode)
        {
            if (itemPresenter == null || itemPresenter.Current == null)
            {
                return C1FormalItemInteractionAuthorizationResult.InitialLocked();
            }

            return itemPresenter.ApplyInteractionAuthorization(
                C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                    itemPresenter.Current,
                    mode));
        }

        private C1FormalRealtimeBattleTickResult PauseRealtimeBattle()
        {
            if (formalLoop == null)
            {
                return null;
            }

            formalLoop.TryPauseRealtimeBattle(
                out C1FormalRealtimeBattleTickResult tick,
                out _);
            return tick;
        }

        private C1FormalRealtimeBattleTickResult ResumeRealtimeBattle()
        {
            if (formalLoop == null)
            {
                return null;
            }

            formalLoop.TryResumeRealtimeBattle(
                out C1FormalRealtimeBattleTickResult tick,
                out _);
            return tick;
        }

        private C1FormalRealtimeBattleItemRefreshResult
            RefreshRealtimeItemsWhilePaused(string refreshCommandId)
        {
            if (formalLoop == null)
            {
                return null;
            }

            formalLoop.TryRefreshRealtimeItemsWhilePaused(
                refreshCommandId,
                out C1FormalRealtimeBattleItemRefreshResult result,
                out _);
            return result;
        }

        private void ConsumeRealtimeItemRefresh(
            C1FormalRealtimeBattleItemRefreshResult result)
        {
            if (result == null || !result.accepted
                || result.stateSnapshot == null)
            {
                return;
            }

            IReadOnlyList<C1FormalRealtimeBattleCue> emittedCues =
                TransportOrderedCuesUnchanged(result.emittedCues);
            CaptureAcceptedCues(emittedCues);
            if (formalPresentationRoot != null
                && !formalPresentationRoot.ConsumeRealtime(
                    result.stateSnapshot,
                    emittedCues,
                    itemPresenter.Current,
                    out string presentationDiagnostic))
            {
                Debug.LogError(
                    "[UnifiedFormalHost] " + presentationDiagnostic,
                    formalPresentationRoot);
            }
        }

        private string NewPrepareItemRefreshCommandId()
        {
            prepareItemRefreshSequence = unchecked(
                prepareItemRefreshSequence + 1L);
            C1FormalRealtimeBattleSessionStateSnapshot state =
                formalLoop == null ? null : formalLoop.RealtimeState;
            return string.Join(".", new[]
            {
                "c1.formal.prepare.item-refresh",
                state == null ? "missing" : state.sessionId,
                state == null
                    ? "0"
                    : state.sessionGeneration.ToString(
                        CultureInfo.InvariantCulture),
                state == null
                    ? "0"
                    : state.resetGeneration.ToString(
                        CultureInfo.InvariantCulture),
                prepareItemRefreshSequence.ToString(
                    "D6",
                    CultureInfo.InvariantCulture)
            });
        }

        private void ConsumeRealtimeLifecycleTick(
            C1FormalRealtimeBattleTickResult tick)
        {
            if (tick == null || !tick.accepted || tick.stateSnapshot == null)
            {
                return;
            }

            IReadOnlyList<C1FormalRealtimeBattleCue> emittedCues =
                TransportOrderedCuesUnchanged(tick.emittedCues);
            CaptureAcceptedCues(emittedCues);
            if (formalPresentationRoot != null
                && !formalPresentationRoot.ConsumeRealtime(
                    tick.stateSnapshot,
                    emittedCues,
                    itemPresenter?.Current,
                    out string presentationDiagnostic))
            {
                Debug.LogError(
                    "[UnifiedFormalHost] " + presentationDiagnostic,
                    formalPresentationRoot);
            }
        }

        private void AdvancePrepareSurfacePresentation()
        {
            if (prepareSurfacePresenter == null
                || prepareSurfaceTransition ==
                C1FormalPrepareSurfaceTransition.None
                || !prepareSurfacePresenter.Tick(
                    Time.unscaledDeltaTime,
                    out C1ExactBattleSandboxPrepareSurfaceState completedState,
                    out int completedGeneration)
                || completedGeneration != prepareLifecycleGeneration)
            {
                return;
            }

            if (completedState ==
                C1ExactBattleSandboxPrepareSurfaceState.Open)
            {
                CompletePrepareSurfaceOpen();
            }
            else if (completedState ==
                     C1ExactBattleSandboxPrepareSurfaceState.Closed)
            {
                CompletePrepareSurfaceClosed();
            }
        }

        private void CompletePrepareSurfaceOpen()
        {
            bool live = prepareSurfaceTransition ==
                        C1FormalPrepareSurfaceTransition.OpeningLive;
            bool rebuild = prepareSurfaceTransition ==
                           C1FormalPrepareSurfaceTransition.OpeningRebuild;
            if (!live && !rebuild)
            {
                ResetPrepareSurfaceForLifecycle();
                return;
            }

            C1FormalItemInteractionAuthorizationResult authorization =
                ApplyItemInteractionMode(
                    C1FormalItemInteractionMode.PREPARE_ENABLED);
            if (IsAcceptedEnabledAuthorization(authorization)
                && prepareSurfacePresenter.SetOpenInputEnabled(
                    true,
                    prepareLifecycleGeneration))
            {
                arrangementSurfaceVisible = true;
                prepareSurfaceTransition =
                    C1FormalPrepareSurfaceTransition.None;
                RenderNavigationAuxiliary();
                RenderPhase(
                    C1FormalPrepareOrchestration.PrepareOpenAcceptedDiagnostic);
                return;
            }

            ApplyItemInteractionMode(C1FormalItemInteractionMode.BATTLE_LOCKED);
            prepareSurfacePresenter.SetOpenInputEnabled(
                false,
                prepareLifecycleGeneration);
            arrangementSurfaceVisible = false;
            if (!prepareSurfacePresenter.TryBeginClose(
                    prepareLifecycleGeneration))
            {
                ResetPrepareSurfaceForLifecycle();
                if (live)
                {
                    C1FormalRealtimeBattleTickResult compensation =
                        ResumeRealtimeBattle();
                    ConsumeRealtimeLifecycleTick(compensation);
                }

                RenderPhase(C1FormalPrepareOrchestration
                                .ItemEnableRejectedDiagnostic + " "
                            + (authorization?.diagnostic ?? "RESULT_MISSING"));
                return;
            }

            prepareSurfaceTransition = live
                ? C1FormalPrepareSurfaceTransition
                    .ClosingLiveAfterEnableFailure
                : C1FormalPrepareSurfaceTransition
                    .ClosingRebuildAfterEnableFailure;
            RenderNavigationAuxiliary();
            RenderPhase(C1FormalPrepareOrchestration
                            .ItemEnableRejectedDiagnostic + " "
                        + (authorization?.diagnostic ?? "RESULT_MISSING"));
        }

        private void CompletePrepareSurfaceClosed()
        {
            C1FormalPrepareSurfaceTransition completedTransition =
                prepareSurfaceTransition;
            prepareSurfaceTransition = C1FormalPrepareSurfaceTransition.None;
            arrangementSurfaceVisible = false;
            prepareSurfacePresenter.SetOpenInputEnabled(
                false,
                prepareLifecycleGeneration);

            switch (completedTransition)
            {
                case C1FormalPrepareSurfaceTransition.ClosingLive:
                case C1FormalPrepareSurfaceTransition
                    .ClosingLiveAfterEnableFailure:
                    pendingPrepareItemRefreshCommandId = string.Empty;
                    C1FormalRealtimeBattleTickResult resumeTick =
                        ResumeRealtimeBattle();
                    ConsumeRealtimeLifecycleTick(resumeTick);
                    pendingBattleMilliseconds = 0d;
                    RenderNavigationAuxiliary();
                    RenderPhase(IsAcceptedRunningLifecycleTick(resumeTick)
                        ? C1FormalPrepareOrchestration
                            .PrepareClosedAcceptedDiagnostic
                        : C1FormalPrepareOrchestration
                              .BattleResumeRejectedDiagnostic + " "
                          + LifecycleTickDiagnostic(resumeTick));
                    return;
                case C1FormalPrepareSurfaceTransition
                    .ClosingLiveLockRejected:
                    RenderNavigationAuxiliary();
                    RenderPhase(
                        C1FormalPrepareOrchestration.ItemLockRejectedDiagnostic);
                    return;
                case C1FormalPrepareSurfaceTransition
                    .ClosingRebuildForStageTwo:
                    RenderNavigationAuxiliary();
                    TryStartStageTwoFromRebuild();
                    return;
                case C1FormalPrepareSurfaceTransition.ClosingRebuild:
                    RenderNavigationAuxiliary();
                    RenderPhase(C1FormalPrepareOrchestration
                        .PrepareClosedAcceptedDiagnostic);
                    return;
                case C1FormalPrepareSurfaceTransition
                    .ClosingRebuildAfterEnableFailure:
                    RenderNavigationAuxiliary();
                    RenderPhase(
                        C1FormalPrepareOrchestration.ItemEnableRejectedDiagnostic);
                    return;
                default:
                    ResetPrepareSurfaceForLifecycle();
                    return;
            }
        }

        private bool BeginRebuildCloseForStageTwo(out string diagnostic)
        {
            return BeginRebuildClose(true, out diagnostic);
        }

        private bool BeginRebuildClose(
            bool startStageTwoAfterClosed,
            out string diagnostic)
        {
            C1FormalItemInteractionAuthorizationResult authorization =
                ApplyItemInteractionMode(
                    C1FormalItemInteractionMode.BATTLE_LOCKED);
            prepareSurfacePresenter.SetOpenInputEnabled(
                false,
                prepareLifecycleGeneration);
            arrangementSurfaceVisible = false;
            if (!IsAcceptedLockedAuthorization(authorization)
                || !prepareSurfacePresenter.TryBeginClose(
                    prepareLifecycleGeneration))
            {
                diagnostic = C1FormalPrepareOrchestration
                                 .ItemLockRejectedDiagnostic + " "
                             + (authorization?.diagnostic ?? "RESULT_MISSING");
                RenderNavigationAuxiliary();
                return false;
            }

            prepareSurfaceTransition = startStageTwoAfterClosed
                ? C1FormalPrepareSurfaceTransition
                    .ClosingRebuildForStageTwo
                : C1FormalPrepareSurfaceTransition.ClosingRebuild;
            diagnostic = C1FormalPrepareParityDiagnostics.LowerStarted;
            RenderNavigationAuxiliary();
            return true;
        }

        private bool TryResumeClosedPausedBattle(out string diagnostic)
        {
            C1FormalItemInteractionAuthorizationResult authorization =
                ApplyItemInteractionMode(
                    C1FormalItemInteractionMode.BATTLE_LOCKED);
            ResetPrepareSurfaceForLifecycle();
            if (!IsAcceptedLockedAuthorization(authorization))
            {
                diagnostic = C1FormalPrepareOrchestration
                                 .ItemLockRejectedDiagnostic + " "
                             + (authorization?.diagnostic ?? "RESULT_MISSING");
                return false;
            }

            string refreshCommandId = NewPrepareItemRefreshCommandId();
            C1FormalRealtimeBattleItemRefreshResult refresh =
                RefreshRealtimeItemsWhilePaused(refreshCommandId);
            if (refresh == null
                || !refresh.accepted
                || refresh.stateSnapshot == null
                || !refresh.stateSnapshot.paused)
            {
                diagnostic = C1FormalPrepareOrchestration
                                 .BattleItemRefreshRejectedDiagnostic + " "
                             + (refresh?.errorCode ?? "RESULT_MISSING");
                RenderNavigationAuxiliary();
                return false;
            }

            ConsumeRealtimeItemRefresh(refresh);

            C1FormalRealtimeBattleTickResult resumeTick = ResumeRealtimeBattle();
            ConsumeRealtimeLifecycleTick(resumeTick);
            pendingBattleMilliseconds = 0d;
            diagnostic = IsAcceptedRunningLifecycleTick(resumeTick)
                ? C1FormalPrepareOrchestration.PrepareClosedAcceptedDiagnostic
                : C1FormalPrepareOrchestration.BattleResumeRejectedDiagnostic
                  + " " + LifecycleTickDiagnostic(resumeTick);
            RenderNavigationAuxiliary();
            return IsAcceptedRunningLifecycleTick(resumeTick);
        }

        private void TryStartStageTwoFromRebuild()
        {
            if (!TryApplyBattleLockedSurfaceState(out string lockDiagnostic))
            {
                RenderPhase(lockDiagnostic);
                return;
            }

            if (formalLoop.ShouldReturnWorldMapAfterRebuild)
            {
                TryReturnWorldMap();
                return;
            }

            if (!formalLoop.TryStartNextStage(out string battleDiagnostic))
            {
                RenderPhase(battleDiagnostic);
                return;
            }

            itemPresenter.PublishCurrent();
            contextSnapshot = formalLoop.CurrentContext;
            acceptedStageId = contextSnapshot.StageId;
            formalPresentationRoot.UnbindItemSourceProvider();
            formalPresentationRoot.ResetPresentation();
            ResetStageThemePresentation();
            string themeApplyDiagnostic = string.Empty;
            if (!stageThemeBackgroundRegistry.TryResolve(
                    contextSnapshot.StageThemeProfileId,
                    out C1StageThemeBackgroundProfile nextTheme,
                    out string themeResolutionDiagnostic)
                || !stageThemeBackgroundPresenter.Apply(
                    nextTheme,
                    out themeApplyDiagnostic))
            {
                string themeDiagnostic = string.IsNullOrEmpty(
                    themeResolutionDiagnostic)
                    ? themeApplyDiagnostic
                    : themeResolutionDiagnostic;
                RenderPhase(themeDiagnostic);
                Debug.LogError(
                    "[UnifiedFormalHost] " + themeDiagnostic,
                    this);
                return;
            }

            if (!TryBindFormalItemSourceProvider(
                    out string itemSourceDiagnostic))
            {
                ClearFormalLoop();
                SetSummary(itemSourceDiagnostic);
                Debug.LogError(
                    "[UnifiedFormalHost] " + itemSourceDiagnostic,
                    this);
                return;
            }

            ResetAcceptedCueView(formalLoop.CurrentRealtimeStart);
            if (!formalPresentationRoot.BeginRealtime(
                    formalLoop.CurrentRealtimeStart,
                    itemPresenter.Current,
                    acceptedStageId,
                    out string presentationDiagnostic))
            {
                RenderPhase(presentationDiagnostic);
                Debug.LogError(
                    "[UnifiedFormalHost] " + presentationDiagnostic,
                    formalPresentationRoot);
                return;
            }

            RenderPhase();
        }

        private bool TryBindFormalItemSourceProvider(out string diagnostic)
        {
            formalPresentationRoot?.UnbindItemSourceProvider();
            C1FormalItemPresentationCatalogSnapshot catalog =
                itemBoardView?.PresentationCatalogSnapshot;
            if (formalPresentationRoot == null
                || itemPresenter == null
                || catalog == null)
            {
                diagnostic = ItemSourceCatalogInvalidDiagnostic;
                return false;
            }

            if (!formalPresentationRoot.BindItemSourceProvider(
                    itemPresenter,
                    catalog,
                    out string providerDiagnostic))
            {
                diagnostic = ItemSourceProviderBindingInvalidDiagnostic
                             + " " + providerDiagnostic;
                return false;
            }

            diagnostic = providerDiagnostic;
            return true;
        }

        private static IReadOnlyList<C1FormalRealtimeBattleCue>
            TransportOrderedCuesUnchanged(
                IReadOnlyList<C1FormalRealtimeBattleCue> emittedCues)
        {
            return emittedCues;
        }

        private void ResetPrepareSurfaceForLifecycle()
        {
            prepareLifecycleGeneration = unchecked(
                prepareLifecycleGeneration + 1);
            prepareSurfaceTransition = C1FormalPrepareSurfaceTransition.None;
            arrangementSurfaceVisible = false;
            pendingPrepareItemRefreshCommandId = string.Empty;
            prepareSurfacePresenter?.ResetForLifecycle(
                prepareLifecycleGeneration);
        }

        private static bool IsAcceptedLockedAuthorization(
            C1FormalItemInteractionAuthorizationResult authorization)
        {
            return authorization != null
                   && authorization.accepted
                   && !authorization.interactionEnabled
                   && authorization.resolvedMode ==
                   C1FormalItemInteractionMode.BATTLE_LOCKED;
        }

        private static bool IsAcceptedEnabledAuthorization(
            C1FormalItemInteractionAuthorizationResult authorization)
        {
            return authorization != null
                   && authorization.accepted
                   && authorization.interactionEnabled
                   && authorization.resolvedMode ==
                   C1FormalItemInteractionMode.PREPARE_ENABLED;
        }

        private static bool IsAcceptedPausedLifecycleTick(
            C1FormalRealtimeBattleTickResult tick)
        {
            return tick != null && tick.accepted && tick.stateSnapshot != null
                   && tick.stateSnapshot.paused;
        }

        private static bool IsAcceptedRunningLifecycleTick(
            C1FormalRealtimeBattleTickResult tick)
        {
            return tick != null && tick.accepted && tick.stateSnapshot != null
                   && !tick.stateSnapshot.paused;
        }

        private static string LifecycleTickDiagnostic(
            C1FormalRealtimeBattleTickResult tick)
        {
            return tick == null || string.IsNullOrEmpty(tick.errorCode)
                ? "RESULT_MISSING"
                : tick.errorCode;
        }

        private int HandleSpeedToggle()
        {
            bool live = formalLoop != null
                        && (formalLoop.Phase ==
                            C1FormalObtainRebuildBattlePhase.StageOneBattle
                            || formalLoop.Phase ==
                            C1FormalObtainRebuildBattlePhase.StageTwoBattle);
            if (live)
            {
                playbackRate = playbackRate == 1 ? 2 : 1;
                RenderNavigationAuxiliary();
            }

            return playbackRate;
        }

        private bool HandleBattleLogToggle()
        {
            if (formalLoop == null || acceptedCueViewBuffer.Count == 0)
            {
                return battleLogOpen;
            }

            battleLogOpen = !battleLogOpen;
            if (battleLogOpen)
            {
                RenderCombatLogCarrier();
            }
            else
            {
                combatLogView?.Render(
                    currentSummary,
                    acceptedCueViewBuffer,
                    false);
            }

            return battleLogOpen;
        }

        private void CloseBattleLog()
        {
            if (!battleLogOpen)
            {
                return;
            }

            battleLogOpen = false;
            combatLogView?.Render(
                currentSummary,
                acceptedCueViewBuffer,
                false);
        }

        private void ResetAcceptedCueView(
            C1FormalRealtimeBattleSessionStartSnapshot start)
        {
            acceptedCueViewBuffer.Clear();
            observedCueSequence = 0L;
            battleLogOpen = false;
            playbackRate = 1;
            CaptureAcceptedCues(start?.initialCues);
            RenderNavigationAuxiliary();
        }

        private void CaptureAcceptedCues(
            IReadOnlyList<C1FormalRealtimeBattleCue> cues)
        {
            if (cues == null)
            {
                return;
            }

            foreach (C1FormalRealtimeBattleCue cue in cues)
            {
                if (cue == null || cue.sequence <= observedCueSequence)
                {
                    continue;
                }

                acceptedCueViewBuffer.Add(cue);
                observedCueSequence = cue.sequence;
            }

            if (battleLogOpen)
            {
                RenderCombatLogCarrier();
            }
        }

        private void RenderCombatLogCarrier()
        {
            if (combatLogView == null)
            {
                return;
            }

            combatLogView.Render(
                currentSummary,
                acceptedCueViewBuffer,
                battleLogOpen);
        }

        private void RenderNavigationAuxiliary()
        {
            bool hasLoop = formalLoop != null;
            bool live = hasLoop
                        && (formalLoop.Phase ==
                            C1FormalObtainRebuildBattlePhase.StageOneBattle
                            || formalLoop.Phase ==
                            C1FormalObtainRebuildBattlePhase.StageTwoBattle);
            bool arrangementInteractable = live
                || (hasLoop && formalLoop.Phase ==
                    C1FormalObtainRebuildBattlePhase.RebuildForStageTwo);
            arrangementInteractable = arrangementInteractable
                                      && prepareSurfaceTransition ==
                                      C1FormalPrepareSurfaceTransition.None;
            navigationPresenter?.RenderAuxiliary(
                playbackRate,
                arrangementInteractable,
                live,
                hasLoop && acceptedCueViewBuffer.Count > 0);
        }

        private static void SetSlotActive(
            IReadOnlyDictionary<string, Transform> slots,
            string slotName,
            bool active)
        {
            if (slots != null && slots.TryGetValue(slotName, out Transform slot)
                && slot != null && slot.gameObject.activeSelf != active)
            {
                slot.gameObject.SetActive(active);
            }
        }
    }

    internal enum C1FormalPrepareSurfaceTransition
    {
        None = 0,
        OpeningLive = 1,
        OpeningRebuild = 2,
        ClosingLive = 3,
        ClosingLiveAfterEnableFailure = 4,
        ClosingLiveLockRejected = 5,
        ClosingRebuild = 6,
        ClosingRebuildForStageTwo = 7,
        ClosingRebuildAfterEnableFailure = 8
    }

    internal static class C1FormalPrepareParityDiagnostics
    {
        internal const string PullUpStarted =
            "C1_FORMAL_PREPARE_PULL_UP_STARTED";
        internal const string LowerStarted =
            "C1_FORMAL_PREPARE_LOWER_STARTED";
    }

    internal enum C1FormalItemDetailAdmissionState
    {
        Absent = 0,
        Ready = 1,
        PartialOrInvalid = 2
    }

    internal static class C1FormalItemDetailAdmissionGate
    {
        internal const string AbsentDiagnostic =
            "UNIFIED_FORMAL_ITEM_DETAIL_ABSENT";
        internal const string ReadyDiagnostic =
            "UNIFIED_FORMAL_ITEM_DETAIL_READY";
        internal const string PartialOrInvalidDiagnostic =
            "UNIFIED_FORMAL_ITEM_DETAIL_PARTIAL_OR_INVALID";

        internal static C1FormalItemDetailAdmissionState Evaluate(
            bool presenterPresent,
            bool popupSlotPresent,
            bool exactSlotIdentity,
            bool authoredReferencesValid,
            out string diagnostic)
        {
            if (!presenterPresent && !popupSlotPresent)
            {
                diagnostic = AbsentDiagnostic;
                return C1FormalItemDetailAdmissionState.Absent;
            }

            if (presenterPresent && popupSlotPresent
                && exactSlotIdentity && authoredReferencesValid)
            {
                diagnostic = ReadyDiagnostic;
                return C1FormalItemDetailAdmissionState.Ready;
            }

            diagnostic = PartialOrInvalidDiagnostic;
            return C1FormalItemDetailAdmissionState.PartialOrInvalid;
        }
    }

    internal static class C1FormalPrepareOrchestration
    {
        internal const string PrepareOpenAcceptedDiagnostic =
            "C1_FORMAL_PREPARE_OPEN_ACCEPTED";
        internal const string PrepareClosedAcceptedDiagnostic =
            "C1_FORMAL_PREPARE_CLOSED_ACCEPTED";
        internal const string BattlePauseRejectedDiagnostic =
            "C1_FORMAL_PREPARE_BATTLE_PAUSE_REJECTED";
        internal const string ItemEnableRejectedDiagnostic =
            "C1_FORMAL_PREPARE_ITEM_ENABLE_REJECTED";
        internal const string CompensationResumeRejectedDiagnostic =
            "C1_FORMAL_PREPARE_COMPENSATION_RESUME_REJECTED";
        internal const string ItemLockRejectedDiagnostic =
            "C1_FORMAL_PREPARE_ITEM_LOCK_REJECTED";
        internal const string BattleResumeRejectedDiagnostic =
            "C1_FORMAL_PREPARE_BATTLE_RESUME_REJECTED";
        internal const string BattleItemRefreshRejectedDiagnostic =
            "C1_FORMAL_PREPARE_BATTLE_ITEM_REFRESH_REJECTED";
        internal const string PhaseRejectedDiagnostic =
            "C1_FORMAL_PREPARE_PHASE_REJECTED";

        internal static bool ShouldSubmitBattleAdvance(
            bool arrangementVisible,
            bool authoritativeBattlePaused)
        {
            return !arrangementVisible && !authoritativeBattlePaused;
        }

        internal static bool TryOpen(
            Func<C1FormalRealtimeBattleTickResult> pauseBattle,
            Func<C1FormalItemInteractionAuthorizationResult> enableItem,
            Action showArrangement,
            Action hideArrangement,
            Func<C1FormalRealtimeBattleTickResult> compensateResume,
            Action publishNavigation,
            out C1FormalRealtimeBattleTickResult pauseTick,
            out C1FormalItemInteractionAuthorizationResult authorization,
            out C1FormalRealtimeBattleTickResult compensationTick,
            out string diagnostic)
        {
            pauseTick = null;
            authorization = null;
            compensationTick = null;
            if (pauseBattle == null || enableItem == null
                || showArrangement == null || hideArrangement == null
                || compensateResume == null || publishNavigation == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            pauseTick = pauseBattle();
            if (!IsAcceptedPausedTick(pauseTick))
            {
                hideArrangement();
                publishNavigation();
                diagnostic = BattlePauseRejectedDiagnostic + " "
                    + TickDiagnostic(pauseTick);
                return false;
            }

            authorization = enableItem();
            if (authorization == null
                || !authorization.accepted
                || !authorization.interactionEnabled
                || authorization.resolvedMode !=
                    C1FormalItemInteractionMode.PREPARE_ENABLED)
            {
                hideArrangement();
                compensationTick = compensateResume();
                publishNavigation();
                if (!IsAcceptedRunningTick(compensationTick))
                {
                    diagnostic = CompensationResumeRejectedDiagnostic + " "
                        + (authorization?.diagnostic ?? "RESULT_MISSING") + " "
                        + TickDiagnostic(compensationTick);
                    return false;
                }

                diagnostic = ItemEnableRejectedDiagnostic + " "
                    + (authorization?.diagnostic ?? "RESULT_MISSING");
                return false;
            }

            showArrangement();
            publishNavigation();
            diagnostic = PrepareOpenAcceptedDiagnostic;
            return true;
        }

        internal static bool TryClose(
            Func<C1FormalItemInteractionAuthorizationResult> lockItem,
            Action hideArrangement,
            Func<C1FormalRealtimeBattleTickResult> resumeBattle,
            Action publishNavigation,
            out C1FormalItemInteractionAuthorizationResult authorization,
            out C1FormalRealtimeBattleTickResult resumeTick,
            out string diagnostic)
        {
            authorization = null;
            resumeTick = null;
            if (lockItem == null || hideArrangement == null
                || resumeBattle == null || publishNavigation == null)
            {
                diagnostic = PhaseRejectedDiagnostic;
                return false;
            }

            authorization = lockItem();
            hideArrangement();
            if (authorization == null
                || !authorization.accepted
                || authorization.interactionEnabled
                || authorization.resolvedMode !=
                    C1FormalItemInteractionMode.BATTLE_LOCKED)
            {
                publishNavigation();
                diagnostic = ItemLockRejectedDiagnostic + " "
                    + (authorization?.diagnostic ?? "RESULT_MISSING");
                return false;
            }

            resumeTick = resumeBattle();
            publishNavigation();
            if (!IsAcceptedRunningTick(resumeTick))
            {
                diagnostic = BattleResumeRejectedDiagnostic + " "
                    + TickDiagnostic(resumeTick);
                return false;
            }

            diagnostic = PrepareClosedAcceptedDiagnostic;
            return true;
        }

        private static bool IsAcceptedPausedTick(
            C1FormalRealtimeBattleTickResult tick)
        {
            return tick != null && tick.accepted && tick.stateSnapshot != null
                   && tick.stateSnapshot.paused;
        }

        private static bool IsAcceptedRunningTick(
            C1FormalRealtimeBattleTickResult tick)
        {
            return tick != null && tick.accepted && tick.stateSnapshot != null
                   && !tick.stateSnapshot.paused;
        }

        private static string TickDiagnostic(
            C1FormalRealtimeBattleTickResult tick)
        {
            return tick == null || string.IsNullOrEmpty(tick.errorCode)
                ? "RESULT_MISSING"
                : tick.errorCode;
        }
    }
}
