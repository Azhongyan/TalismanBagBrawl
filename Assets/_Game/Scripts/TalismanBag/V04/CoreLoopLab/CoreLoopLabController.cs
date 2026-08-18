using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V04.CoreLoopLab
{
    [DisallowMultipleComponent]
    public sealed class CoreLoopLabController : MonoBehaviour
    {
        private const float PendingBattleLaunchTimeoutSeconds = 10f;

        public const string SceneName =
            "Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab";
        public const string ScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab.unity";

        [Header("Play-time profile selection (no runtime selector)")]
        [SerializeField] private CoreLoopLabProfile selectedProfile =
            CoreLoopLabProfile.A_CurrentSingleBattle;

        [Header("Existing authoritative owners")]
        [SerializeField] private BuildGridInteractionPreviewController
            gridController;
        [SerializeField] private BattleSandboxRuntimeLoopRuntime runtimeLoop;
        [SerializeField] private Button prepareToggleButton;
        [SerializeField] private Button prepareContinueButton;

        [Header("Authored Lab panels")]
        [SerializeField] private GameObject rewardPanelRoot;
        [SerializeField] private BuildItemPreviewCardView[] rewardCardViews;
        [SerializeField] private Button[] rewardChoiceButtons;
        [SerializeField] private GameObject resultPanelRoot;
        [SerializeField] private Text resultTitleText;
        [SerializeField] private Text resultBodyText;
        [SerializeField] private Button retryButton;
        [SerializeField] private CoreLoopLabPresentationAdapter
            presentationAdapter;

        private CoreLoopLabSession session;
        private CoreLoopLabBattleLaunchAdapter battleLaunchAdapter;
        private int encounterGenerationCounter;
        private int pendingBattleIndex;
        private int pendingEncounterGeneration;
        private float pendingBattleLaunchWaitSeconds;
        private bool suppressPrepareLaunchSignal;
        private int rosterResetGeneration;
        private string rosterSessionToken = string.Empty;
        private string pendingRewardBaseItemId = string.Empty;
        private bool rosterSessionReady;
        private bool battleAdvanceFailureLogged;

        public CoreLoopLabProfile SelectedProfile => selectedProfile;
        public string ProductContext => CoreLoopLabFixedContent.ProductContext;
        public CoreLoopLabSession Session => session;
        public bool WritesCampaignSave => false;
        public bool WritesChapterProgress => false;
        public bool WritesClaimLedger => false;
        public bool GrantsFormalReward => false;

        public void BindAuthoredScene(
            BuildGridInteractionPreviewController grid,
            BattleSandboxRuntimeLoopRuntime loop,
            Button prepareToggle,
            Button prepareContinue,
            GameObject rewardPanel,
            IReadOnlyList<BuildItemPreviewCardView> cards,
            IReadOnlyList<Button> choices,
            GameObject resultPanel,
            Text resultTitle,
            Text resultBody,
            Button retry,
            CoreLoopLabPresentationAdapter presentation)
        {
            gridController = grid;
            runtimeLoop = loop;
            prepareToggleButton = prepareToggle;
            prepareContinueButton = prepareContinue;
            rewardPanelRoot = rewardPanel;
            rewardCardViews = cards?.ToArray()
                ?? Array.Empty<BuildItemPreviewCardView>();
            rewardChoiceButtons = choices?.ToArray() ?? Array.Empty<Button>();
            resultPanelRoot = resultPanel;
            resultTitleText = resultTitle;
            resultBodyText = resultBody;
            retryButton = retry;
            presentationAdapter = presentation;
        }

        public void SetProfileForEditor(CoreLoopLabProfile profile)
        {
            if (Application.isPlaying)
            {
                return;
            }

            selectedProfile = profile;
        }

        private void Awake()
        {
            if (!ValidateAuthoredBindings(out string diagnostic))
            {
                Debug.LogError("[CoreLoopLab] AUTHORED_BINDING_REQUIRED: "
                    + diagnostic, this);
                enabled = false;
                return;
            }

            rewardPanelRoot.SetActive(false);
            resultPanelRoot.SetActive(false);
            runtimeLoop.ResetLoop();
            runtimeLoop.enabled = false;
            presentationAdapter.ApplyProfile(selectedProfile);
            WireAuthoredButtons();
            ConfigureFixedRewardCards();
            SetArrangementControlsInteractable(false);
        }

        private void Start()
        {
            if (!enabled)
            {
                return;
            }

            if (!TryStartNewSession(returnToPreparation: false))
            {
                Debug.LogError(
                    "[CoreLoopLab] DEV_ROSTER_RESET_REQUIRED",
                    this);
            }
        }

        private void Update()
        {
            if (session == null || battleLaunchAdapter == null)
            {
                return;
            }

            TryAcceptPendingBattleLaunch();
            switch (session.Phase)
            {
                case CoreLoopLabPhase.FirstBattle:
                    TryAdvanceActiveBattle();
                    presentationAdapter.Tick(
                        Time.deltaTime,
                        battleLaunchAdapter.Current);
                    TryObserveBattleResult(1);
                    break;
                case CoreLoopLabPhase.SecondBattle:
                    TryAdvanceActiveBattle();
                    presentationAdapter.Tick(
                        Time.deltaTime,
                        battleLaunchAdapter.Current);
                    TryObserveBattleResult(2);
                    break;
                default:
                    presentationAdapter.Tick(
                        Time.deltaTime,
                        battleLaunchAdapter.Current);
                    break;
            }
        }

        private void OnDisable()
        {
            ClearPendingBattleLaunch();
            battleLaunchAdapter?.Reset();
            presentationAdapter?.EndBattle();
        }

        private void OnDestroy()
        {
            ClearPendingBattleLaunch();
            battleLaunchAdapter?.Reset();
            presentationAdapter?.EndBattle();
        }

        private void WireAuthoredButtons()
        {
            for (int index = 0; index < rewardChoiceButtons.Length; index++)
            {
                int capturedIndex = index;
                rewardChoiceButtons[index].onClick.RemoveAllListeners();
                rewardChoiceButtons[index].onClick.AddListener(
                    () => SelectReward(capturedIndex));
            }

            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RetryCurrentProfile);
            prepareContinueButton.onClick.AddListener(
                HandleAuthoredPrepareLaunchRequested);
        }

        private void ConfigureFixedRewardCards()
        {
            CoreLoopLabRewardProfile rewardProfile =
                CoreLoopLabRewardProfile.Shared;
            for (int index = 0;
                 index < rewardProfile.RewardCandidates.Count;
                 index++)
            {
                CoreLoopLabRewardCandidate candidate =
                    rewardProfile.RewardCandidates[index];
                string rewardId = candidate.BaseItemId;
                BuildItemPreviewCardView card = rewardCardViews[index];
                ItemInnerDataDefinition item =
                    rewardProfile.ResolveAuthoritativeDefinition(rewardId);
                if (item == null)
                {
                    Debug.LogError("[CoreLoopLab] FIXED_REWARD_NOT_IN_CATALOG: "
                        + rewardId, this);
                    continue;
                }

                card.BindItemDisplayData(
                    null,
                    item.itemId,
                    item.displayName,
                    item.FaMenDisplayName,
                    item.shapeId + " · " + item.defaultLocalCells.Count + "格",
                    candidate.CardColor);
                card.enabled = false;
            }
        }

        private bool TryStartNewSession(bool returnToPreparation)
        {
            SetArrangementControlsInteractable(false);
            rosterSessionReady = false;
            rewardPanelRoot.SetActive(false);
            resultPanelRoot.SetActive(false);
            presentationAdapter.EndBattle();
            presentationAdapter.ApplyProfile(selectedProfile);
            battleLaunchAdapter?.Reset();
            ClearPendingBattleLaunch();
            pendingRewardBaseItemId = string.Empty;
            battleAdvanceFailureLogged = false;

            if (returnToPreparation && gridController.IsSandboxBattleModeActive)
            {
                prepareToggleButton.onClick.Invoke();
            }

            CoreLoopLabRewardProfile rewardProfile =
                CoreLoopLabRewardProfile.Shared;
            int candidateGeneration = rosterResetGeneration + 1;
            string candidateToken = "core-loop-lab-roster-"
                + Guid.NewGuid().ToString("N");
            ItemDevSessionRosterAvailabilityRequest resetRequest = new(
                ItemDevSessionRosterAvailabilityOperation.Reset,
                ItemSystemBattleSandboxBoardAdapter.CoreLoopLabScenePath,
                CoreLoopLabFixedContent.ProductContext,
                candidateToken,
                candidateGeneration,
                initialAvailableBaseItemIds:
                    rewardProfile.CanonicalInitialRoster);
            ItemDevSessionRosterAvailabilityControllerResult resetResult =
                gridController.ApplyCoreLoopLabItemDevSessionRosterAvailability(
                    resetRequest);
            if (!ValidateResetPublication(
                    resetResult,
                    candidateToken,
                    candidateGeneration,
                    rewardProfile.CanonicalInitialRoster,
                    out string resetDiagnostic))
            {
                Debug.LogError(
                    "[CoreLoopLab] DEV_ROSTER_RESET_REJECTED: "
                    + resetDiagnostic,
                    this);
                return false;
            }

            rosterResetGeneration = candidateGeneration;
            rosterSessionToken = candidateToken;
            rosterSessionReady = true;
            battleLaunchAdapter = new CoreLoopLabBattleLaunchAdapter(
                gridController,
                selectedProfile,
                rosterSessionToken,
                rosterResetGeneration);
            session = new CoreLoopLabSession(selectedProfile);
            SetArrangementControlsInteractable(true);
            Debug.Log("[CoreLoopLab] SESSION_STARTED profile="
                + selectedProfile + " productContext="
                + CoreLoopLabFixedContent.ProductContext
                + " resetGeneration=" + rosterResetGeneration, this);
            return true;
        }

        private void HandleAuthoredPrepareLaunchRequested()
        {
            if (!isActiveAndEnabled
                || suppressPrepareLaunchSignal || session == null
                || !rosterSessionReady
                || pendingBattleIndex != 0)
            {
                return;
            }

            int battleIndex = session.Phase switch
            {
                CoreLoopLabPhase.FirstPreparation => 1,
                CoreLoopLabPhase.SecondPreparation => 2,
                _ => 0
            };
            if (battleIndex <= 0)
            {
                return;
            }

            pendingBattleIndex = battleIndex;
            pendingEncounterGeneration = ++encounterGenerationCounter;
            pendingBattleLaunchWaitSeconds = 0f;
        }

        private void TryAcceptPendingBattleLaunch()
        {
            if (pendingBattleIndex <= 0 || pendingEncounterGeneration <= 0)
            {
                return;
            }

            if (!gridController.IsSandboxBattleModeActive)
            {
                pendingBattleLaunchWaitSeconds += Time.unscaledDeltaTime;
                if (pendingBattleLaunchWaitSeconds
                    >= PendingBattleLaunchTimeoutSeconds)
                {
                    ClearPendingBattleLaunch();
                    Debug.LogWarning(
                        "[CoreLoopLab] AUTHORED_PREPARE_LAUNCH_CANCELLED_TIMEOUT",
                        this);
                }
                return;
            }

            int battleIndex = pendingBattleIndex;
            CoreLoopLabEncounterProfile profile =
                CoreLoopLabEncounterProfiles.ForBattleIndex(battleIndex);
            if (!battleLaunchAdapter.TryAcceptAuthoredPrepareLaunch(
                    battleIndex,
                    pendingEncounterGeneration,
                    session.SelectedRewardId,
                    profile,
                    out BattleSandboxDevBattleSessionAcceptedStart acceptedStart,
                    out string diagnosticCode))
            {
                ClearPendingBattleLaunch();
                Debug.LogError(
                    "[CoreLoopLab] EXPLICIT_BATTLE_LAUNCH_REJECTED: "
                    + diagnosticCode,
                    this);
                return;
            }

            if (!session.AcceptBattleStarted(battleIndex, acceptedStart))
            {
                ClearPendingBattleLaunch();
                Debug.LogError(
                    "[CoreLoopLab] ACCEPTED_START_SESSION_MISMATCH",
                    this);
                return;
            }

            ClearPendingBattleLaunch();
            presentationAdapter.BeginBattle(
                acceptedStart,
                profile.EnemyVisualProfileKey);
        }

        private void ClearPendingBattleLaunch()
        {
            pendingBattleIndex = 0;
            pendingEncounterGeneration = 0;
            pendingBattleLaunchWaitSeconds = 0f;
        }

        private void TryObserveBattleResult(int battleIndex)
        {
            BattleSandboxDevBattleSessionSnapshot snapshot =
                battleLaunchAdapter?.Current;
            if (snapshot?.IsTerminal != true)
            {
                return;
            }

            if (!session.AcceptBattleCompleted(battleIndex, snapshot))
            {
                return;
            }

            if (session.Phase == CoreLoopLabPhase.RewardChoice)
            {
                rewardPanelRoot.SetActive(true);
                return;
            }

            ShowResult();
        }

        private void SelectReward(int rewardIndex)
        {
            if (rewardIndex < 0
                || rewardIndex >= CoreLoopLabFixedContent.RewardIds.Count
                || session == null
                || !rosterSessionReady
                || session.Phase != CoreLoopLabPhase.RewardChoice)
            {
                return;
            }

            string rewardId = CoreLoopLabFixedContent.RewardIds[rewardIndex];
            if (!string.IsNullOrWhiteSpace(pendingRewardBaseItemId)
                && !string.Equals(
                    pendingRewardBaseItemId,
                    rewardId,
                    StringComparison.Ordinal))
            {
                Debug.LogError(
                    "[CoreLoopLab] DEV_ROSTER_REWARD_ALREADY_CHOSEN",
                    this);
                return;
            }

            pendingRewardBaseItemId = rewardId;
            ItemDevSessionRosterAvailabilityRequest addRequest = new(
                ItemDevSessionRosterAvailabilityOperation.AddOnce,
                ItemSystemBattleSandboxBoardAdapter.CoreLoopLabScenePath,
                CoreLoopLabFixedContent.ProductContext,
                rosterSessionToken,
                rosterResetGeneration,
                baseItemId: rewardId);
            ItemDevSessionRosterAvailabilityControllerResult addResult =
                gridController.ApplyCoreLoopLabItemDevSessionRosterAvailability(
                    addRequest);
            if (!ValidateAddOncePublication(
                    addResult,
                    rewardId,
                    out string addDiagnostic))
            {
                Debug.LogError(
                    "[CoreLoopLab] DEV_ROSTER_REWARD_REJECTED: "
                    + addDiagnostic,
                    this);
                return;
            }

            if (!session.SelectFixedReward(rewardId))
            {
                Debug.LogError(
                    "[CoreLoopLab] DEV_ROSTER_REWARD_SESSION_REJECTED",
                    this);
                return;
            }

            presentationAdapter.EndBattle();
            suppressPrepareLaunchSignal = true;
            prepareToggleButton.onClick.Invoke();
            suppressPrepareLaunchSignal = false;
            rewardPanelRoot.SetActive(false);
        }

        private void TryAdvanceActiveBattle()
        {
            if (battleLaunchAdapter == null
                || !battleLaunchAdapter.IsRunning)
            {
                return;
            }

            long deltaMilliseconds = Math.Max(
                1L,
                (long)Math.Round(
                    Time.deltaTime * 1000f,
                    MidpointRounding.AwayFromZero));
            if (battleLaunchAdapter.TryAdvance(
                    deltaMilliseconds,
                    out _,
                    out string diagnosticCode))
            {
                return;
            }

            if (!battleAdvanceFailureLogged)
            {
                battleAdvanceFailureLogged = true;
                Debug.LogError(
                    "[CoreLoopLab] DEV_BATTLE_SESSION_ADVANCE_REJECTED: "
                    + diagnosticCode,
                    this);
            }
        }

        private void ShowResult()
        {
            CoreLoopLabSessionSnapshot snapshot = session.BuildSnapshot();
            string profileLabel = selectedProfile switch
            {
                CoreLoopLabProfile.A_CurrentSingleBattle => "方向 A · 当前单战闭环",
                CoreLoopLabProfile.B_TwoBattleRewardLoop => "方向 B · 双战奖励闭环",
                _ => "方向 C · 语义反馈双战闭环"
            };
            resultTitleText.text = profileLabel;
            resultBodyText.text = string.IsNullOrWhiteSpace(
                    snapshot.SelectedRewardId)
                ? "本轮战斗已结束。点击重试，回到同一套整备与数值基线。"
                : "本轮两场战斗已结束。临时奖励："
                    + RewardDisplayName(snapshot.SelectedRewardId)
                    + "。点击重试，清空 Lab 内存会话并回到整备。";
            resultPanelRoot.SetActive(true);
            Debug.Log("[CoreLoopLab] SESSION_RESULT profile="
                + selectedProfile + " logicFingerprint="
                + snapshot.LogicLedgerFingerprint + " rewardId="
                + snapshot.SelectedRewardId, this);
        }

        private void RetryCurrentProfile()
        {
            TryStartNewSession(returnToPreparation: true);
        }

        private static string RewardDisplayName(string rewardId)
        {
            ItemInnerDataDefinition item =
                CoreLoopLabRewardProfile.Shared
                    .ResolveAuthoritativeDefinition(rewardId);
            return item?.displayName ?? rewardId ?? string.Empty;
        }

        private bool ValidateResetPublication(
            ItemDevSessionRosterAvailabilityControllerResult result,
            string expectedSessionToken,
            int expectedGeneration,
            IReadOnlyList<string> expectedInitialRoster,
            out string diagnosticCode)
        {
            diagnosticCode = result?.DiagnosticCode
                ?? "DEV_ROSTER_RESET_RESULT_MISSING";
            if (result?.Accepted != true || !result.TrayPublished)
            {
                return false;
            }

            ItemDevSessionRosterAvailabilitySnapshot returned =
                result.AvailabilitySnapshot;
            ItemDevSessionRosterAvailabilitySnapshot current =
                gridController.CurrentItemDevSessionRosterAvailability;
            if (!SnapshotMatchesScope(
                    returned,
                    expectedSessionToken,
                    expectedGeneration)
                || !SnapshotMatchesScope(
                    current,
                    expectedSessionToken,
                    expectedGeneration))
            {
                diagnosticCode = "DEV_ROSTER_RESET_SCOPE_MISMATCH";
                return false;
            }

            string[] canonicalInitial = (expectedInitialRoster
                    ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!returned.InitialAvailableBaseItemIds.SequenceEqual(
                    canonicalInitial,
                    StringComparer.Ordinal)
                || !returned.AvailableBaseItemIds.SequenceEqual(
                    canonicalInitial,
                    StringComparer.Ordinal)
                || !current.InitialAvailableBaseItemIds.SequenceEqual(
                    canonicalInitial,
                    StringComparer.Ordinal)
                || !current.AvailableBaseItemIds.SequenceEqual(
                    canonicalInitial,
                    StringComparer.Ordinal)
                || !string.Equals(
                    returned.CanonicalSignature,
                    current.CanonicalSignature,
                    StringComparison.Ordinal))
            {
                diagnosticCode = "DEV_ROSTER_RESET_PUBLICATION_MISMATCH";
                return false;
            }

            diagnosticCode = "NONE";
            return true;
        }

        private bool ValidateAddOncePublication(
            ItemDevSessionRosterAvailabilityControllerResult result,
            string expectedRewardBaseItemId,
            out string diagnosticCode)
        {
            diagnosticCode = result?.DiagnosticCode
                ?? "DEV_ROSTER_ADD_RESULT_MISSING";
            if (result?.Accepted != true || !result.TrayPublished)
            {
                return false;
            }

            ItemDevSessionRosterAvailabilitySnapshot returned =
                result.AvailabilitySnapshot;
            ItemDevSessionRosterAvailabilitySnapshot current =
                gridController.CurrentItemDevSessionRosterAvailability;
            if (!SnapshotMatchesScope(
                    returned,
                    rosterSessionToken,
                    rosterResetGeneration)
                || !SnapshotMatchesScope(
                    current,
                    rosterSessionToken,
                    rosterResetGeneration))
            {
                diagnosticCode = "DEV_ROSTER_ADD_SCOPE_MISMATCH";
                return false;
            }

            string[] canonicalInitial =
                CoreLoopLabRewardProfile.Shared.CanonicalInitialRoster
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray();
            bool returnedContainsExactlyOnce = returned.AvailableBaseItemIds
                .Count(value => string.Equals(
                    value,
                    expectedRewardBaseItemId,
                    StringComparison.Ordinal)) == 1;
            bool currentContainsExactlyOnce = current.AvailableBaseItemIds
                .Count(value => string.Equals(
                    value,
                    expectedRewardBaseItemId,
                    StringComparison.Ordinal)) == 1;
            string[] expectedAvailable = canonicalInitial
                .Append(expectedRewardBaseItemId)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            if (!returned.InitialAvailableBaseItemIds.SequenceEqual(
                    canonicalInitial,
                    StringComparer.Ordinal)
                || !current.InitialAvailableBaseItemIds.SequenceEqual(
                    canonicalInitial,
                    StringComparer.Ordinal)
                || !returned.AvailableBaseItemIds.SequenceEqual(
                    expectedAvailable,
                    StringComparer.Ordinal)
                || !current.AvailableBaseItemIds.SequenceEqual(
                    expectedAvailable,
                    StringComparer.Ordinal)
                || !returnedContainsExactlyOnce
                || !currentContainsExactlyOnce
                || !string.Equals(
                    returned.CanonicalSignature,
                    current.CanonicalSignature,
                    StringComparison.Ordinal))
            {
                diagnosticCode = "DEV_ROSTER_ADD_PUBLICATION_MISMATCH";
                return false;
            }

            diagnosticCode = "NONE";
            return true;
        }

        private static bool SnapshotMatchesScope(
            ItemDevSessionRosterAvailabilitySnapshot snapshot,
            string expectedSessionToken,
            int expectedGeneration)
        {
            return snapshot != null
                && string.Equals(
                    snapshot.SchemaId,
                    ItemDevSessionRosterAvailabilitySnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                && string.Equals(
                    snapshot.ApprovedDevHostId,
                    ItemSystemBattleSandboxBoardAdapter.CoreLoopLabScenePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    snapshot.ProductContext,
                    CoreLoopLabFixedContent.ProductContext,
                    StringComparison.Ordinal)
                && string.Equals(
                    snapshot.SessionToken,
                    expectedSessionToken,
                    StringComparison.Ordinal)
                && snapshot.ResetGeneration == expectedGeneration;
        }

        private void SetArrangementControlsInteractable(bool interactable)
        {
            if (prepareToggleButton != null)
            {
                prepareToggleButton.interactable = interactable;
            }
            if (prepareContinueButton != null)
            {
                prepareContinueButton.interactable = interactable;
            }
        }

        private bool ValidateAuthoredBindings(out string diagnostic)
        {
            diagnostic = string.Empty;
            if (gridController == null
                || runtimeLoop == null
                || prepareToggleButton == null
                || prepareContinueButton == null
                || rewardPanelRoot == null
                || resultPanelRoot == null
                || resultTitleText == null
                || resultBodyText == null
                || retryButton == null
                || presentationAdapter == null)
            {
                diagnostic = "One or more explicit Scene references are null.";
                return false;
            }

            if (rewardCardViews == null
                || rewardChoiceButtons == null
                || rewardCardViews.Length
                    != CoreLoopLabFixedContent.RewardIds.Count
                || rewardChoiceButtons.Length
                    != CoreLoopLabFixedContent.RewardIds.Count
                || rewardCardViews.Any(value => value == null)
                || rewardChoiceButtons.Any(value => value == null))
            {
                diagnostic = "Exactly three authored reward cards/buttons are required.";
                return false;
            }

            return true;
        }
    }
}
