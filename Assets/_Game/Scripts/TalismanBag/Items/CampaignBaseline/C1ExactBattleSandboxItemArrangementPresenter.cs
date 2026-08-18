using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TalismanBag.Items.CampaignBaseline
{
    public static class C1ExactBattleSandboxItemRotationAdapter
    {
        public const string InvalidFormalDegreesDiagnostic =
            "C1_EXACT_BATTLESANDBOX_FORMAL_ROTATION_DEGREES_INVALID";

        public static int NormalizeVisualQuarterTurns(int value)
        {
            int normalized = value % 4;
            return normalized < 0 ? normalized + 4 : normalized;
        }

        public static bool TryVisualQuarterTurnsToFormalDegrees(
            int visualQuarterTurns,
            out int formalDegrees)
        {
            switch (visualQuarterTurns)
            {
                case 0:
                    formalDegrees = 0;
                    return true;
                case 1:
                    formalDegrees = 270;
                    return true;
                case 2:
                    formalDegrees = 180;
                    return true;
                case 3:
                    formalDegrees = 90;
                    return true;
                default:
                    formalDegrees = 0;
                    return false;
            }
        }

        public static bool TryFormalDegreesToVisualQuarterTurns(
            int formalDegrees,
            out int visualQuarterTurns)
        {
            switch (formalDegrees)
            {
                case 0:
                    visualQuarterTurns = 0;
                    return true;
                case 90:
                    visualQuarterTurns = 3;
                    return true;
                case 180:
                    visualQuarterTurns = 2;
                    return true;
                case 270:
                    visualQuarterTurns = 1;
                    return true;
                default:
                    visualQuarterTurns = 0;
                    return false;
            }
        }

        public static Vector2Int RotateVisualCell(
            Vector2Int cell,
            int visualQuarterTurns)
        {
            int normalized = NormalizeVisualQuarterTurns(visualQuarterTurns);
            if (!TryVisualQuarterTurnsToFormalDegrees(
                    normalized,
                    out int formalDegrees)
                || !TryRotateFormalCell(cell, formalDegrees, out Vector2Int rotated))
                return cell;
            return rotated;
        }

        public static bool TryRotateFormalCell(
            Vector2Int cell,
            int formalDegrees,
            out Vector2Int rotated)
        {
            switch (formalDegrees)
            {
                case 0:
                    rotated = cell;
                    return true;
                case 90:
                    rotated = new Vector2Int(-cell.y, cell.x);
                    return true;
                case 180:
                    rotated = new Vector2Int(-cell.x, -cell.y);
                    return true;
                case 270:
                    rotated = new Vector2Int(cell.y, -cell.x);
                    return true;
                default:
                    rotated = default;
                    return false;
            }
        }
    }

    internal readonly struct C1ExactBattleSandboxItemFootprintCell
    {
        internal C1ExactBattleSandboxItemFootprintCell(
            Vector2Int rawCell,
            Vector2Int normalizedCell)
        {
            RawCell = rawCell;
            NormalizedCell = normalizedCell;
        }

        internal Vector2Int RawCell { get; }
        internal Vector2Int NormalizedCell { get; }
    }

    public sealed class C1ExactBattleSandboxItemFootprint
    {
        private readonly IReadOnlyList<Vector2Int> normalizedCells;
        private readonly IReadOnlyList<C1ExactBattleSandboxItemFootprintCell>
            mappedCells;

        internal C1ExactBattleSandboxItemFootprint(
            IReadOnlyList<Vector2Int> configuredNormalizedCells,
            IReadOnlyList<C1ExactBattleSandboxItemFootprintCell>
                configuredMappedCells,
            int visualQuarterTurns,
            Vector2 trayLocalCellXAxis,
            Vector2 trayLocalCellYAxis,
            Vector2 trayLocalSize,
            int widthInCells,
            int heightInCells)
        {
            normalizedCells = configuredNormalizedCells;
            mappedCells = configuredMappedCells;
            VisualQuarterTurns = visualQuarterTurns;
            TrayLocalCellXAxis = trayLocalCellXAxis;
            TrayLocalCellYAxis = trayLocalCellYAxis;
            TrayLocalSize = trayLocalSize;
            WidthInCells = widthInCells;
            HeightInCells = heightInCells;
        }

        public IReadOnlyList<Vector2Int> NormalizedCells => normalizedCells;
        public int VisualQuarterTurns { get; }
        public Vector2 TrayLocalCellXAxis { get; }
        public Vector2 TrayLocalCellYAxis { get; }
        public Vector2 TrayLocalSize { get; }
        public int WidthInCells { get; }
        public int HeightInCells { get; }

        internal bool TryResolveRawCell(
            Vector2Int normalizedCell,
            out Vector2Int rawCell)
        {
            foreach (C1ExactBattleSandboxItemFootprintCell mapped in
                     mappedCells
                     ?? Array.Empty<C1ExactBattleSandboxItemFootprintCell>())
            {
                if (mapped.NormalizedCell != normalizedCell) continue;
                rawCell = mapped.RawCell;
                return true;
            }

            rawCell = default;
            return false;
        }
    }

    internal sealed class C1ExactBattleSandboxItemInteractionCandidate
    {
        internal C1ExactBattleSandboxItemInteractionCandidate(
            Vector2Int rawGrabbedCell,
            Vector2Int pointerCell,
            int visualQuarterTurns,
            int formalDegrees,
            Vector2Int rotatedMinimum,
            Vector2Int normalizedDisplayAnchor,
            Vector2Int authorityAnchor,
            IReadOnlyList<Vector2Int> orderedOccupiedCells,
            bool locallyValid)
        {
            RawGrabbedCell = rawGrabbedCell;
            PointerCell = pointerCell;
            VisualQuarterTurns = visualQuarterTurns;
            FormalDegrees = formalDegrees;
            RotatedMinimum = rotatedMinimum;
            NormalizedDisplayAnchor = normalizedDisplayAnchor;
            AuthorityAnchor = authorityAnchor;
            OrderedOccupiedCells = orderedOccupiedCells;
            IsLocallyValid = locallyValid;
            FormalCandidate = new C1FormalItemPlacementCandidate(
                authorityAnchor,
                formalDegrees);
        }

        internal Vector2Int RawGrabbedCell { get; }
        internal Vector2Int PointerCell { get; }
        internal int VisualQuarterTurns { get; }
        internal int FormalDegrees { get; }
        internal Vector2Int RotatedMinimum { get; }
        internal Vector2Int NormalizedDisplayAnchor { get; }
        internal Vector2Int AuthorityAnchor { get; }
        internal IReadOnlyList<Vector2Int> OrderedOccupiedCells { get; }
        internal bool IsLocallyValid { get; }
        internal C1FormalItemPlacementCandidate FormalCandidate { get; }
    }

    internal sealed class C1ExactBattleSandboxItemTrayInteractionCandidate
    {
        internal C1ExactBattleSandboxItemTrayInteractionCandidate(
            Vector2Int rawGrabbedCell,
            Vector2Int pointerCell,
            int visualQuarterTurns,
            int formalDegrees,
            C1FormalItemPlacementCandidate formalCandidate,
            IReadOnlyList<Vector2Int> orderedOccupiedCells,
            bool locallyValid)
        {
            RawGrabbedCell = rawGrabbedCell;
            PointerCell = pointerCell;
            VisualQuarterTurns = visualQuarterTurns;
            FormalDegrees = formalDegrees;
            FormalCandidate = formalCandidate;
            OrderedOccupiedCells = orderedOccupiedCells;
            IsLocallyValid = locallyValid;
        }

        internal Vector2Int RawGrabbedCell { get; }
        internal Vector2Int PointerCell { get; }
        internal int VisualQuarterTurns { get; }
        internal int FormalDegrees { get; }
        internal C1FormalItemPlacementCandidate FormalCandidate { get; }
        internal IReadOnlyList<Vector2Int> OrderedOccupiedCells { get; }
        internal bool IsLocallyValid { get; }
    }

    internal sealed class C1ExactBattleSandboxRightRotateZoneState
    {
        internal const float ActivationWidthPixels = 136f;
        internal const float ActivationHeightPixels = 180f;
        internal const float ExitPaddingPixels = 18f;
        internal const float SeekWindowSeconds = 0.28f;
        internal const float SeekMinAgeSeconds = 0.02f;
        internal const float SeekMinDeltaXPixels = 20f;
        internal const float SeekMaxUpwardDriftPixels = 44f;
        internal const float SeekMinVelocityXPixelsPerSecond = 300f;
        internal const float TriggerCooldownSeconds = 1f;
        internal const float ConfirmVisualSeconds = 0.18f;
        internal const float BoardHoldPaddingPixels = 36f;

        private bool hasSeekAnchor;
        private Vector2 seekAnchorPosition;
        private float seekAnchorTime;
        private bool seekingEntry;
        private Rect seekTargetRect;
        private Rect seekExitRect;
        private bool insideEntry;
        private float lastTriggerTime = float.NegativeInfinity;
        private float confirmUntilTime = float.NegativeInfinity;

        internal bool IsSeekingOrInside => seekingEntry || insideEntry;

        internal bool IsConfirmationVisible(float now)
        {
            return now <= confirmUntilTime;
        }

        internal bool Update(
            Vector2 pointerPosition,
            float now,
            Rect activationRect,
            bool enabled)
        {
            if (!enabled)
            {
                Reset();
                return false;
            }

            if (insideEntry)
            {
                if (seekExitRect.Contains(pointerPosition)) return false;
                insideEntry = false;
                Prime(pointerPosition, now);
                return false;
            }

            if (seekingEntry)
            {
                if (now - seekAnchorTime > SeekWindowSeconds)
                {
                    seekingEntry = false;
                    Prime(pointerPosition, now);
                    return false;
                }

                if (!seekTargetRect.Contains(pointerPosition)) return false;
                insideEntry = true;
                seekingEntry = false;
                return TryTrigger(now);
            }

            if (!hasSeekAnchor)
            {
                Prime(pointerPosition, now);
                return false;
            }

            float age = now - seekAnchorTime;
            if (age < SeekMinAgeSeconds) return false;
            Vector2 delta = pointerPosition - seekAnchorPosition;
            if (delta.x < SeekMinDeltaXPixels
                || delta.y > SeekMaxUpwardDriftPixels
                || delta.x / Mathf.Max(age, 0.001f)
                   < SeekMinVelocityXPixelsPerSecond)
            {
                if (age > SeekWindowSeconds) Prime(pointerPosition, now);
                return false;
            }

            seekingEntry = true;
            seekTargetRect = activationRect;
            seekExitRect = Expand(activationRect, ExitPaddingPixels);
            seekAnchorPosition = pointerPosition;
            seekAnchorTime = now;
            hasSeekAnchor = true;
            if (!seekTargetRect.Contains(pointerPosition)) return false;
            insideEntry = true;
            seekingEntry = false;
            return TryTrigger(now);
        }

        internal void Reset()
        {
            hasSeekAnchor = false;
            seekAnchorPosition = Vector2.zero;
            seekAnchorTime = 0f;
            seekingEntry = false;
            seekTargetRect = default;
            seekExitRect = default;
            insideEntry = false;
            lastTriggerTime = float.NegativeInfinity;
            confirmUntilTime = float.NegativeInfinity;
        }

        private bool TryTrigger(float now)
        {
            if (now - lastTriggerTime < TriggerCooldownSeconds) return false;
            lastTriggerTime = now;
            confirmUntilTime = now + ConfirmVisualSeconds;
            return true;
        }

        private void Prime(Vector2 pointerPosition, float now)
        {
            hasSeekAnchor = true;
            seekAnchorPosition = pointerPosition;
            seekAnchorTime = now;
        }

        private static Rect Expand(Rect source, float padding)
        {
            return Rect.MinMaxRect(
                source.xMin - padding,
                source.yMin - padding,
                source.xMax + padding,
                source.yMax + padding);
        }
    }

    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxItemArrangementPresenter : MonoBehaviour
    {
        public const string MissingReferenceDiagnostic =
            "C1_EXACT_BATTLESANDBOX_PRESENTATION_AUTHORED_REFERENCE_MISSING";
        public const string InteractionNotActiveDiagnostic =
            "C1_EXACT_BATTLESANDBOX_INTERACTION_NOT_ACTIVE";
        public const string CandidateInvalidDiagnostic =
            "C1_EXACT_BATTLESANDBOX_CANDIDATE_INVALID";
        public const string FootprintUnavailableDiagnostic =
            "C1_EXACT_BATTLESANDBOX_ITEM_FOOTPRINT_UNAVAILABLE";
        public const string CandidateCommitMismatchDiagnostic =
            "C1_EXACT_BATTLESANDBOX_CANDIDATE_COMMIT_MISMATCH";

        [SerializeField] private C1ExactBattleSandboxItemBoardView boardView;
        [SerializeField] private C1ExactBattleSandboxItemTrayView trayView;

        private C1FormalItemSessionAuthority authority;
        private IC1FormalItemArtworkResolver itemArtworkResolver;
        private C1FormalItemDetailSelectionResult currentFormalItemDetail =
            C1FormalItemDetailSelectionResult.InitialClosed();
        private C1FormalItemInteractionAuthorizationResult
            currentInteractionAuthorization =
                C1FormalItemInteractionAuthorizationResult.InitialLocked();
        private Action<C1FormalItemDetailSelectionResult>
            formalItemDetailChanged;
        private long commandSequence;
        private string selectedItemInstanceId = string.Empty;
        private string activeItemInstanceId = string.Empty;
        private string activeExpectedSignature = string.Empty;
        private bool activeWasPlaced;
        private int activeRotation;
        private Vector2Int activeRawGrabbedCell;
        private bool hasActiveRawGrabbedCell;
        private Vector2Int activePointerCell;
        private bool hasActivePointerCell;
        private bool activeCandidateLocallyValid;
        // Compatibility mirror for existing lifecycle regression coverage.
        // The deterministic state machine remains the only rotate-zone truth.
        private bool rotateZoneLatched;
        private C1ExactBattleSandboxRightRotateZoneState rightRotateZoneState;
        private Vector2 trayLocalCellXAxis;
        private Vector2 trayLocalCellYAxis;
        private bool hasTrayLocalCellBasis;
        private C1ExactBattleSandboxItemInteractionCandidate
            activeInteractionCandidate;
        private C1ExactBattleSandboxItemTrayInteractionCandidate
            activeTrayInteractionCandidate;
        // Compatibility mirror for the pre-REV04 lifecycle regression harness.
        // It is always derived from activeInteractionCandidate and is never read
        // as placement truth by the runtime interaction path.
        private C1FormalItemPlacementCandidate activeCandidate;
        private C1ExactBattleSandboxItemInteractionCandidate
            lastPreviewCandidate;
        private C1ExactBattleSandboxItemInteractionCandidate
            lastSubmittedCandidate;
        private C1ExactBattleSandboxItemTrayInteractionCandidate
            lastTrayPreviewCandidate;
        private C1ExactBattleSandboxItemTrayInteractionCandidate
            lastSubmittedTrayCandidate;
        private string lastDiagnostic = C1FormalItemSessionDiagnosticCodes.None;
        private bool missingReferenceLogged;

        public bool IsBound => authority != null;
        public C1FormalItemSessionSnapshot Current => authority?.Current;
        public string SelectedItemInstanceId => selectedItemInstanceId;
        public string LastDiagnostic => lastDiagnostic;
        public bool HasActiveInteraction => activeItemInstanceId.Length > 0;
        public bool LastPreviewCommitCandidateConsistent =>
            (lastPreviewCandidate != null
             && ReferenceEquals(lastPreviewCandidate, lastSubmittedCandidate))
            || (lastTrayPreviewCandidate != null
                && ReferenceEquals(
                    lastTrayPreviewCandidate,
                    lastSubmittedTrayCandidate));
        public C1FormalItemDetailSelectionResult CurrentFormalItemDetail =>
            currentFormalItemDetail;
        public C1FormalItemInteractionAuthorizationResult
            CurrentInteractionAuthorization => currentInteractionAuthorization
                ?? C1FormalItemInteractionAuthorizationResult.InitialLocked();
        internal bool IsItemInteractionEnabled =>
            CurrentInteractionAuthorization.interactionEnabled;
        private C1ExactBattleSandboxRightRotateZoneState RightRotateZoneState =>
            rightRotateZoneState ??=
                new C1ExactBattleSandboxRightRotateZoneState();

        public event Action<C1FormalItemDetailSelectionResult>
            FormalItemDetailChanged
        {
            add
            {
                formalItemDetailChanged -= value;
                formalItemDetailChanged += value;
            }
            remove => formalItemDetailChanged -= value;
        }

        public void AssignForEditor(
            C1ExactBattleSandboxItemBoardView configuredBoardView,
            C1ExactBattleSandboxItemTrayView configuredTrayView)
        {
            boardView = configuredBoardView;
            trayView = configuredTrayView;
        }

        public bool ValidateAuthoredReferences()
        {
            return boardView != null
                   && trayView != null
                   && boardView.ValidateAuthoredReferences()
                   && trayView.ValidateAuthoredReferences();
        }

        public bool Bind(
            C1FormalItemSessionAuthority configuredAuthority,
            out string diagnostic)
        {
            if (configuredAuthority == null || !ValidateAuthoredReferences())
            {
                LockInteractionForLifecycle(
                    null,
                    C1FormalItemInteractionAuthorizationDiagnostics
                        .LifecycleReboundLocked);
                diagnostic = MissingReferenceDiagnostic;
                lastDiagnostic = diagnostic;
                LogMissingOnce();
                return false;
            }

            bool authorityChanged = !ReferenceEquals(
                authority,
                configuredAuthority);
            if (authorityChanged)
            {
                ReleaseBinding(
                    C1FormalItemDetailDiagnostics.LifecycleRebound,
                    false);
                authority = configuredAuthority;
                commandSequence = 0;
                selectedItemInstanceId = string.Empty;
                boardView.BindPresenter(this);
                trayView.BindPresenter(this);
            }
            itemArtworkResolver = boardView;
            LockInteractionForLifecycle(
                authority.Current,
                authorityChanged
                    ? C1FormalItemInteractionAuthorizationDiagnostics
                        .LifecycleBoundLocked
                    : C1FormalItemInteractionAuthorizationDiagnostics
                        .LifecycleReboundLocked);

            PublishCurrent();
            if (string.Equals(
                    lastDiagnostic,
                    FootprintUnavailableDiagnostic,
                    StringComparison.Ordinal))
            {
                diagnostic = lastDiagnostic;
                return false;
            }
            diagnostic = C1FormalItemSessionDiagnosticCodes.None;
            lastDiagnostic = diagnostic;
            return true;
        }

        public void Unbind()
        {
            ReleaseBinding(
                C1FormalItemDetailDiagnostics.LifecycleUnbound,
                true);
        }

        public C1FormalItemInteractionAuthorizationResult
            ApplyInteractionAuthorization(
                C1FormalItemInteractionAuthorizationRequest request)
        {
            C1FormalItemInteractionAuthorizationResult result =
                C1FormalItemInteractionAuthorization.Evaluate(
                    request,
                    authority?.Current,
                    CurrentInteractionAuthorization);
            currentInteractionAuthorization = result;
            if (!result.interactionEnabled) CancelAllInteractionResidue();
            return result;
        }

        public void PublishCurrent()
        {
            if (authority == null || !ValidateAuthoredReferences())
            {
                LogMissingOnce();
                return;
            }

            C1FormalItemSessionSnapshot snapshot = authority.Current;
            if (snapshot == null)
            {
                CloseFormalItemDetailForLifecycle(
                    C1FormalItemDetailDiagnostics.SessionSnapshotMissing);
                LogMissingOnce();
                return;
            }

            if (!AuthorizationMatchesSessionIdentity(snapshot))
            {
                LockInteractionForLifecycle(
                    snapshot,
                    C1FormalItemInteractionAuthorizationDiagnostics
                        .LifecycleReboundLocked);
            }

            if (selectedItemInstanceId.Length > 0
                && snapshot.FindRosterEntry(selectedItemInstanceId) == null)
            {
                selectedItemInstanceId = string.Empty;
                CloseFormalItemDetailForLifecycle(
                    C1FormalItemDetailDiagnostics.SelectedInstanceRemoved);
            }
            else
            {
                RefreshOpenFormalItemDetail(snapshot);
            }

            boardView.Bind(snapshot, selectedItemInstanceId);
            C1FormalItemPresentationCatalogSnapshot presentationCatalog =
                boardView.PresentationCatalogSnapshot;
            if (presentationCatalog == null
                || !string.Equals(
                    boardView.BoundSnapshotSignature,
                    snapshot.canonicalSignature,
                    StringComparison.Ordinal))
            {
                lastDiagnostic = MissingReferenceDiagnostic;
                trayView.Bind(
                    snapshot,
                    selectedItemInstanceId,
                    null,
                    null);
                return;
            }
            if (!TryRefreshTrayLocalCellBasis()
                || !TryBuildTrayFootprints(
                    snapshot,
                    out IReadOnlyDictionary<string,
                        C1ExactBattleSandboxItemFootprint> footprints))
            {
                lastDiagnostic = FootprintUnavailableDiagnostic;
                trayView.Bind(
                    snapshot,
                    selectedItemInstanceId,
                    null,
                    presentationCatalog);
                return;
            }

            trayView.Bind(
                snapshot,
                selectedItemInstanceId,
                footprints,
                presentationCatalog);
            lastDiagnostic = C1FormalItemSessionDiagnosticCodes.None;
        }

        public bool TryResolveCurrentPlacedLitSourceAnchor(
            C1Pool15FormalItemSourceAnchorRequest request,
            out C1Pool15FormalItemSourceAnchorBinding binding)
        {
            binding = null;
            C1FormalItemSessionSnapshot current = authority?.Current;
            return current != null
                   && request != null
                   && request.MatchesSnapshot(current)
                   && boardView != null
                   && boardView.TryResolveExactPlacedLitSourceAnchor(
                       current,
                       request,
                       out binding)
                   && binding != null
                   && binding.Matches(request);
        }

        public bool TryResolveCurrentPlacedLitPresentationSource(
            string itemInstanceId,
            string baseItemId,
            out C1Pool15FormalItemSourceAnchorBinding binding)
        {
            binding = null;
            C1FormalItemSessionSnapshot current = authority?.Current;
            return current != null
                   && !string.IsNullOrWhiteSpace(itemInstanceId)
                   && !string.IsNullOrWhiteSpace(baseItemId)
                   && boardView != null
                   && boardView.TryResolveExactPlacedLitPresentationSource(
                       current,
                       itemInstanceId,
                       baseItemId,
                       out binding)
                   && binding != null
                   && string.Equals(
                       binding.itemInstanceId,
                       itemInstanceId,
                       StringComparison.Ordinal)
                   && string.Equals(
                       binding.baseItemId,
                       baseItemId,
                       StringComparison.Ordinal);
        }

        public void SelectCard(C1ExactBattleSandboxItemCardView card)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (card == null || !card.HasBoundItem) return;
            SelectBoardItem(card.ItemInstanceId);
        }

        public void SelectBoardItem(string itemInstanceId)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (authority == null
                || authority.Current.FindRosterEntry(itemInstanceId) == null)
                return;
            bool closeSelection = currentFormalItemDetail != null
                                  && currentFormalItemDetail.isOpen
                                  && currentFormalItemDetail.projection != null
                                  && string.Equals(
                                      currentFormalItemDetail.projection
                                          .itemInstanceId,
                                      itemInstanceId,
                                      StringComparison.Ordinal);
            selectedItemInstanceId = closeSelection
                ? string.Empty
                : itemInstanceId;
            PublishCurrent();
            if (closeSelection)
            {
                CloseFormalItemDetail(
                    C1FormalItemDetailSelectionRequest.Close(
                        authority.Current));
            }
            else
            {
                OpenSelectedFormalItemDetail(
                    C1FormalItemDetailSelectionRequest.Open(
                        authority.Current,
                        itemInstanceId));
            }
        }

        public C1FormalItemDetailSelectionResult OpenSelectedFormalItemDetail(
            C1FormalItemDetailSelectionRequest request)
        {
            if (!IsItemInteractionEnabled)
                return C1FormalItemDetailProjectionAndSelection
                    .RejectAgainstCurrent(
                        C1FormalItemInteractionAuthorizationDiagnostics
                            .InteractionLocked,
                        currentFormalItemDetail);
            if (request == null
                || selectedItemInstanceId.Length == 0
                || !string.Equals(
                    request.itemInstanceId,
                    selectedItemInstanceId,
                    StringComparison.Ordinal))
                return C1FormalItemDetailProjectionAndSelection
                    .RejectAgainstCurrent(
                        C1FormalItemDetailDiagnostics.SelectedInstanceMismatch,
                        currentFormalItemDetail);
            return EvaluateAndApplyFormalItemDetail(request);
        }

        public C1FormalItemDetailSelectionResult CloseFormalItemDetail(
            C1FormalItemDetailSelectionRequest request)
        {
            return EvaluateAndApplyFormalItemDetail(request);
        }

        public void BeginCardDrag(
            C1ExactBattleSandboxItemCardView card,
            PointerEventData eventData)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (card == null || !card.HasBoundItem
                             || !card.TryGetPressedRawGrabbedCell(
                                 out Vector2Int rawGrabbedCell))
                return;
            BeginInteraction(
                card.ItemInstanceId,
                card.IsPlaced,
                card.Rotation,
                rawGrabbedCell);
            UpdatePointer(eventData);
        }

        public void UpdateCardDrag(
            C1ExactBattleSandboxItemCardView card,
            PointerEventData eventData)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (card == null || !string.Equals(
                    card.ItemInstanceId,
                    activeItemInstanceId,
                    StringComparison.Ordinal))
                return;
            UpdatePointer(eventData);
            card.SetRotateFeedback(
                activeRotation,
                activeInteractionCandidate != null);
        }

        public void EndCardDrag(
            C1ExactBattleSandboxItemCardView card,
            PointerEventData eventData)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (card == null || !string.Equals(
                    card.ItemInstanceId,
                    activeItemInstanceId,
                    StringComparison.Ordinal))
                return;
            EndInteraction(eventData);
        }

        public void BeginBoardDrag(string itemInstanceId, PointerEventData eventData)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (authority == null
                || !boardView.TryGetActiveBoardDragRawGrabbedCell(
                    itemInstanceId,
                    out Vector2Int rawGrabbedCell))
                return;
            C1FormalItemPlacementSnapshot placement =
                authority.Current.FindPlacementByInstanceId(itemInstanceId);
            if (placement == null) return;
            if (!C1ExactBattleSandboxItemRotationAdapter
                    .TryFormalDegreesToVisualQuarterTurns(
                        placement.rotation,
                        out int visualQuarterTurns))
            {
                lastDiagnostic = C1ExactBattleSandboxItemRotationAdapter
                    .InvalidFormalDegreesDiagnostic;
                boardView.ClearInteractionFeedback();
                return;
            }

            BeginInteraction(
                itemInstanceId,
                true,
                visualQuarterTurns,
                rawGrabbedCell);
            UpdatePointer(eventData);
        }

        public void UpdateBoardDrag(string itemInstanceId, PointerEventData eventData)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (!string.Equals(
                    itemInstanceId,
                    activeItemInstanceId,
                    StringComparison.Ordinal))
                return;
            UpdatePointer(eventData);
        }

        public void EndBoardDrag(string itemInstanceId, PointerEventData eventData)
        {
            if (!RejectUnlessInteractionEnabled()) return;
            if (!string.Equals(
                    itemInstanceId,
                    activeItemInstanceId,
                    StringComparison.Ordinal))
                return;
            EndInteraction(eventData);
        }

        public bool PreviewCandidate(
            string itemInstanceId,
            Vector2Int anchorCell,
            int rotation,
            out bool locallyValid)
        {
            locallyValid = false;
            if (!RejectUnlessInteractionEnabled()) return false;
            if (authority == null || authority.Current.FindRosterEntry(itemInstanceId) == null)
            {
                lastDiagnostic = InteractionNotActiveDiagnostic;
                return false;
            }

            C1FormalItemPlacementSnapshot placement =
                authority.Current.FindPlacementByInstanceId(itemInstanceId);
            int visualQuarterTurns = C1ExactBattleSandboxItemRotationAdapter
                .NormalizeVisualQuarterTurns(rotation);
            if (!TryResolveSyntheticGrabForNormalizedAnchor(
                    itemInstanceId,
                    anchorCell,
                    visualQuarterTurns,
                    out Vector2Int rawGrabbedCell,
                    out Vector2Int pointerCell))
            {
                lastDiagnostic = CandidateInvalidDiagnostic;
                return false;
            }

            BeginInteraction(
                itemInstanceId,
                placement != null,
                visualQuarterTurns,
                rawGrabbedCell);
            if (!BuildAndShowCandidate(pointerCell))
            {
                lastDiagnostic = CandidateInvalidDiagnostic;
                return false;
            }

            lastPreviewCandidate = activeInteractionCandidate;
            locallyValid = activeCandidateLocallyValid;
            lastDiagnostic = C1FormalItemSessionDiagnosticCodes.None;
            return true;
        }

        public bool CommitPreviewedCandidate(out string diagnostic)
        {
            if (!IsItemInteractionEnabled)
            {
                diagnostic = C1FormalItemInteractionAuthorizationDiagnostics
                    .InteractionLocked;
                lastDiagnostic = diagnostic;
                CancelAllInteractionResidue();
                return false;
            }
            if (authority == null || activeInteractionCandidate == null)
            {
                diagnostic = InteractionNotActiveDiagnostic;
                lastDiagnostic = diagnostic;
                CancelInteraction();
                return false;
            }

            if (!activeCandidateLocallyValid)
            {
                diagnostic = CandidateInvalidDiagnostic;
                lastDiagnostic = diagnostic;
                CancelInteraction();
                PublishCurrent();
                return false;
            }

            C1FormalItemArrangementCommandKind kind;
            if (activeWasPlaced)
            {
                kind = C1FormalItemArrangementCommandKind.MoveOnBoard;
            }
            else
            {
                kind = FindPlacedSameBase(
                        authority.Current,
                        activeItemInstanceId) == null
                    ? C1FormalItemArrangementCommandKind.PlaceFromTray
                    : C1FormalItemArrangementCommandKind
                        .ReplaceSameBaseFromTray;
            }
            lastSubmittedCandidate = activeInteractionCandidate;
            bool accepted = SubmitExactlyOne(
                kind,
                activeItemInstanceId,
                activeInteractionCandidate,
                activeExpectedSignature,
                out diagnostic);
            ResetInteractionState();
            boardView.ClearInteractionFeedback();
            trayView.ClearInteractionFeedback();
            PublishCurrent();
            return accepted;
        }

        public bool SubmitReturnToTray(
            string itemInstanceId,
            out string diagnostic)
        {
            if (!IsItemInteractionEnabled)
            {
                diagnostic = C1FormalItemInteractionAuthorizationDiagnostics
                    .InteractionLocked;
                lastDiagnostic = diagnostic;
                CancelAllInteractionResidue();
                return false;
            }
            if (authority == null)
            {
                diagnostic = InteractionNotActiveDiagnostic;
                lastDiagnostic = diagnostic;
                return false;
            }

            string expected = authority.Current.canonicalSignature;
            bool accepted = SubmitExactlyOne(
                C1FormalItemArrangementCommandKind.ReturnToTray,
                itemInstanceId,
                null,
                expected,
                out diagnostic);
            ResetInteractionState();
            boardView.ClearInteractionFeedback();
            trayView.ClearInteractionFeedback();
            PublishCurrent();
            return accepted;
        }

        public void CancelInteraction()
        {
            ResetInteractionState();
            if (boardView != null) boardView.ClearInteractionFeedback();
            if (trayView != null) trayView.ClearInteractionFeedback();
            if (authority != null && ValidateAuthoredReferences()) PublishCurrent();
        }

        private void BeginInteraction(
            string itemInstanceId,
            bool wasPlaced,
            int rotation,
            Vector2Int rawGrabbedCell)
        {
            if (!IsItemInteractionEnabled) return;
            if (authority == null || authority.Current.FindRosterEntry(itemInstanceId) == null)
                return;
            ResetInteractionState();
            trayView?.ClearInteractionFeedback();
            activeItemInstanceId = itemInstanceId;
            activeWasPlaced = wasPlaced;
            activeRotation = C1ExactBattleSandboxItemRotationAdapter
                .NormalizeVisualQuarterTurns(rotation);
            activeRawGrabbedCell = rawGrabbedCell;
            hasActiveRawGrabbedCell = true;
            activeExpectedSignature = authority.Current.canonicalSignature;
            selectedItemInstanceId = itemInstanceId;
        }

        private void UpdatePointer(PointerEventData eventData)
        {
            if (!IsItemInteractionEnabled || authority == null || eventData == null
                || activeItemInstanceId.Length == 0
                || !hasActiveRawGrabbedCell)
                return;
            Camera eventCamera = eventData.pressEventCamera ?? eventData.enterEventCamera;
            bool pointerOnBoard = boardView.TryScreenPointToCell(
                    eventData.position,
                    eventCamera,
                    out Vector2Int cell);
            if (pointerOnBoard)
            {
                activeTrayInteractionCandidate = null;
                trayView.ClearInteractionFeedback();
                BuildAndShowCandidate(cell);
            }
            else
            {
                ClearBoardCandidateFeedback();
                if (!activeWasPlaced
                    && trayView.TryScreenPointToCell(
                        eventData.position,
                        eventCamera,
                        out Vector2Int trayCell))
                    BuildAndShowTrayCandidate(trayCell);
                else
                {
                    activeTrayInteractionCandidate = null;
                    trayView.ClearInteractionFeedback();
                }
                return;
            }

            if (activeInteractionCandidate == null || !hasActivePointerCell
                || !boardView.TryLayoutRightRotateZone(
                    eventData,
                    out Rect activationRect,
                    out Rect holdRect)
                || (!pointerOnBoard && !holdRect.Contains(eventData.position)))
            {
                ClearBoardCandidateFeedback();
                return;
            }

            float now = Time.unscaledTime;
            bool rotateTriggered = RightRotateZoneState.Update(
                eventData.position,
                now,
                activationRect,
                true);
            if (rotateTriggered)
            {
                activeRotation = C1ExactBattleSandboxItemRotationAdapter
                    .NormalizeVisualQuarterTurns(activeRotation + 1);
                RefreshActiveTrayFootprint();
                BuildAndShowCandidate(activePointerCell);
                boardView.TryLayoutRightRotateZone(
                    eventData,
                    out _,
                    out _);
            }

            boardView.SetRotateFeedbackState(
                RightRotateZoneState.IsConfirmationVisible(now),
                RightRotateZoneState.IsSeekingOrInside);
            rotateZoneLatched = RightRotateZoneState.IsSeekingOrInside;
        }

        private bool BuildAndShowTrayCandidate(Vector2Int pointerCell)
        {
            if (!IsItemInteractionEnabled
                || authority == null
                || activeWasPlaced
                || !hasActiveRawGrabbedCell
                || !TryGetAuthoritativeShapeCells(
                    authority.Current,
                    activeItemInstanceId,
                    out IReadOnlyList<Vector2Int> rawShapeCells)
                || !C1ExactBattleSandboxItemRotationAdapter
                    .TryVisualQuarterTurnsToFormalDegrees(
                        activeRotation,
                        out int formalDegrees)
                || !C1FormalItemTrayLayoutRules.TryBuildCandidate(
                    rawShapeCells,
                    activeRawGrabbedCell,
                    pointerCell,
                    formalDegrees,
                    out C1FormalItemPlacementCandidate formalCandidate,
                    out IReadOnlyList<Vector2Int> occupiedCells))
            {
                activeTrayInteractionCandidate = null;
                trayView.ClearInteractionFeedback();
                return false;
            }

            HashSet<Vector2Int> blocked = new(
                authority.Current.trayLayout.placements
                    .Where(value => value.isActiveInTray
                                    && !string.Equals(
                                        value.itemInstanceId,
                                        activeItemInstanceId,
                                        StringComparison.Ordinal))
                    .SelectMany(value => value.occupiedCells));
            bool locallyValid = C1FormalItemTrayLayoutRules.IsInsideTray(
                                    occupiedCells)
                                && !occupiedCells.Any(blocked.Contains);
            activeTrayInteractionCandidate =
                new C1ExactBattleSandboxItemTrayInteractionCandidate(
                    activeRawGrabbedCell,
                    pointerCell,
                    activeRotation,
                    formalDegrees,
                    formalCandidate,
                    occupiedCells,
                    locallyValid);
            lastTrayPreviewCandidate = activeTrayInteractionCandidate;
            trayView.ShowCandidate(activeTrayInteractionCandidate);
            return true;
        }

        private bool BuildAndShowCandidate(Vector2Int pointerCell)
        {
            if (!IsItemInteractionEnabled)
            {
                CancelAllInteractionResidue();
                return false;
            }
            if (!TryBuildActiveCandidate(
                    pointerCell,
                    out C1ExactBattleSandboxItemInteractionCandidate candidate))
            {
                ClearActiveCandidateFeedback();
                return false;
            }

            activeInteractionCandidate = candidate;
            activeCandidate = candidate.FormalCandidate;
            activePointerCell = pointerCell;
            hasActivePointerCell = true;
            activeCandidateLocallyValid = candidate.IsLocallyValid;
            lastPreviewCandidate = activeInteractionCandidate;
            boardView.ShowCandidate(
                activeItemInstanceId,
                activeInteractionCandidate,
                activeCandidateLocallyValid);
            return true;
        }

        private bool TryResolveSyntheticGrabForNormalizedAnchor(
            string itemInstanceId,
            Vector2Int normalizedDisplayAnchor,
            int visualQuarterTurns,
            out Vector2Int rawGrabbedCell,
            out Vector2Int pointerCell)
        {
            rawGrabbedCell = default;
            pointerCell = default;
            if (!TryGetAuthoritativeShapeCells(
                    authority?.Current,
                    itemInstanceId,
                    out IReadOnlyList<Vector2Int> rawShapeCells)
                || !C1ExactBattleSandboxItemRotationAdapter
                    .TryVisualQuarterTurnsToFormalDegrees(
                        visualQuarterTurns,
                        out int formalDegrees))
                return false;

            rawGrabbedCell = rawShapeCells
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .First();
            Vector2Int[] rotated = rawShapeCells
                .Select(value =>
                {
                    C1ExactBattleSandboxItemRotationAdapter.TryRotateFormalCell(
                        value,
                        formalDegrees,
                        out Vector2Int result);
                    return result;
                })
                .ToArray();
            Vector2Int rotatedMinimum = new(
                rotated.Min(value => value.x),
                rotated.Min(value => value.y));
            C1ExactBattleSandboxItemRotationAdapter.TryRotateFormalCell(
                rawGrabbedCell,
                formalDegrees,
                out Vector2Int rotatedGrabbedCell);
            pointerCell = normalizedDisplayAnchor
                          + rotatedGrabbedCell
                          - rotatedMinimum;
            return true;
        }

        private bool TryBuildActiveCandidate(
            Vector2Int pointerCell,
            out C1ExactBattleSandboxItemInteractionCandidate candidate)
        {
            candidate = null;
            C1FormalItemSessionSnapshot snapshot = authority?.Current;
            if (!hasActiveRawGrabbedCell
                || !TryGetAuthoritativeShapeCells(
                    snapshot,
                    activeItemInstanceId,
                    out IReadOnlyList<Vector2Int> rawShapeCells))
                return false;

            string activePlacementId = snapshot
                .FindPlacementByInstanceId(activeItemInstanceId)
                ?.placementId;
            string replacedSameBasePlacementId = activePlacementId == null
                ? FindPlacedSameBase(snapshot, activeItemInstanceId)?.placementId
                : null;
            Vector2Int[] blockingCells = snapshot.itemSystemSnapshot.placements
                .Where(value => !string.Equals(
                                    value.placementId,
                                    activePlacementId,
                                    StringComparison.Ordinal)
                                && !string.Equals(
                                    value.placementId,
                                    replacedSameBasePlacementId,
                                    StringComparison.Ordinal))
                .SelectMany(value => value.OccupiedCells)
                .Concat(new[] { snapshot.itemSystemSnapshot.eyeCell })
                .Distinct()
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .ToArray();
            return TryCreateInteractionCandidate(
                rawShapeCells,
                activeRawGrabbedCell,
                pointerCell,
                activeRotation,
                blockingCells,
                snapshot.itemSystemSnapshot.boardSize,
                out candidate);
        }

        private static C1FormalItemPlacementSnapshot FindPlacedSameBase(
            C1FormalItemSessionSnapshot snapshot,
            string itemInstanceId)
        {
            C1FormalItemRosterEntrySnapshot roster = snapshot?.FindRosterEntry(
                itemInstanceId);
            if (roster == null || roster.isSpecialLightingSource)
                return null;
            return snapshot.placements.FirstOrDefault(value =>
                !string.Equals(
                    value.itemInstanceId,
                    itemInstanceId,
                    StringComparison.Ordinal)
                && string.Equals(
                    value.baseItemId,
                    roster.baseItemId,
                    StringComparison.Ordinal));
        }

        private static bool TryGetAuthoritativeShapeCells(
            C1FormalItemSessionSnapshot snapshot,
            string itemInstanceId,
            out IReadOnlyList<Vector2Int> rawShapeCells)
        {
            rawShapeCells = Array.Empty<Vector2Int>();
            C1FormalItemRosterEntrySnapshot roster = snapshot?.FindRosterEntry(
                itemInstanceId);
            ItemSystemCatalogItemSnapshot catalog = snapshot?.itemSystemSnapshot
                ?.catalogItems.FirstOrDefault(value => roster != null
                    && string.Equals(
                        value.itemId,
                        roster.baseItemId,
                        StringComparison.Ordinal));
            if (catalog?.ShapeCells == null || catalog.ShapeCells.Count == 0)
                return false;
            rawShapeCells = catalog.ShapeCells;
            return true;
        }

        internal static bool TryCreateInteractionCandidate(
            IReadOnlyList<Vector2Int> authoritativeRawShapeCells,
            Vector2Int rawGrabbedCell,
            Vector2Int pointerCell,
            int visualQuarterTurns,
            IReadOnlyList<Vector2Int> blockingCells,
            int boardSize,
            out C1ExactBattleSandboxItemInteractionCandidate candidate)
        {
            candidate = null;
            if (authoritativeRawShapeCells == null
                || authoritativeRawShapeCells.Count == 0
                || authoritativeRawShapeCells.Distinct().Count()
                   != authoritativeRawShapeCells.Count
                || !authoritativeRawShapeCells.Contains(rawGrabbedCell)
                || visualQuarterTurns < 0
                || visualQuarterTurns > 3
                || boardSize <= 0
                || !C1ExactBattleSandboxItemRotationAdapter
                    .TryVisualQuarterTurnsToFormalDegrees(
                        visualQuarterTurns,
                        out int formalDegrees))
                return false;

            List<KeyValuePair<Vector2Int, Vector2Int>> mapped = new();
            foreach (Vector2Int rawCell in authoritativeRawShapeCells)
            {
                if (!C1ExactBattleSandboxItemRotationAdapter
                        .TryRotateFormalCell(
                            rawCell,
                            formalDegrees,
                            out Vector2Int rotatedCell))
                    return false;
                mapped.Add(new KeyValuePair<Vector2Int, Vector2Int>(
                    rawCell,
                    rotatedCell));
            }

            if (mapped.Select(value => value.Value).Distinct().Count()
                != mapped.Count)
                return false;
            Vector2Int rotatedMinimum = new(
                mapped.Min(value => value.Value.x),
                mapped.Min(value => value.Value.y));
            Vector2Int rotatedGrabbedCell = mapped
                .Single(value => value.Key == rawGrabbedCell)
                .Value;
            Vector2Int normalizedGrabbedOffset =
                rotatedGrabbedCell - rotatedMinimum;
            Vector2Int normalizedDisplayAnchor =
                pointerCell - normalizedGrabbedOffset;
            Vector2Int authorityAnchor =
                normalizedDisplayAnchor - rotatedMinimum;
            Vector2Int[] orderedOccupiedCells = mapped
                .Select(value => authorityAnchor + value.Value)
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .ToArray();
            HashSet<Vector2Int> blocked = new(
                blockingCells ?? Array.Empty<Vector2Int>());
            bool inside = orderedOccupiedCells.All(value =>
                value.x >= 0 && value.x < boardSize
                             && value.y >= 0 && value.y < boardSize);
            bool overlap = orderedOccupiedCells.Any(blocked.Contains);
            candidate = new C1ExactBattleSandboxItemInteractionCandidate(
                rawGrabbedCell,
                pointerCell,
                visualQuarterTurns,
                formalDegrees,
                rotatedMinimum,
                normalizedDisplayAnchor,
                authorityAnchor,
                Array.AsReadOnly(orderedOccupiedCells),
                inside && !overlap);
            return true;
        }

        internal static bool TryCreateFootprint(
            IReadOnlyList<Vector2Int> authoritativeLocalShapeCells,
            int visualQuarterTurns,
            Vector2 trayLocalCellXAxis,
            Vector2 trayLocalCellYAxis,
            out C1ExactBattleSandboxItemFootprint footprint)
        {
            footprint = null;
            if (authoritativeLocalShapeCells == null
                || authoritativeLocalShapeCells.Count == 0
                || visualQuarterTurns < 0
                || visualQuarterTurns > 3
                || !IsFiniteNonZero(trayLocalCellXAxis)
                || !IsFiniteNonZero(trayLocalCellYAxis))
                return false;

            KeyValuePair<Vector2Int, Vector2Int>[] mapped =
                authoritativeLocalShapeCells
                    .Select(cell => new KeyValuePair<Vector2Int, Vector2Int>(
                        cell,
                        C1ExactBattleSandboxItemRotationAdapter
                            .RotateVisualCell(cell, visualQuarterTurns)))
                    .ToArray();
            Vector2Int[] rotated = mapped.Select(value => value.Value).ToArray();
            if (rotated.Distinct().Count() != rotated.Length) return false;
            int minX = rotated.Min(cell => cell.x);
            int minY = rotated.Min(cell => cell.y);
            Vector2Int[] normalized = rotated
                .Select(cell => new Vector2Int(cell.x - minX, cell.y - minY))
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
            C1ExactBattleSandboxItemFootprintCell[] mappedCells = mapped
                .Select(value =>
                    new C1ExactBattleSandboxItemFootprintCell(
                        value.Key,
                        value.Value - new Vector2Int(minX, minY)))
                .OrderBy(value => value.NormalizedCell.y)
                .ThenBy(value => value.NormalizedCell.x)
                .ToArray();
            int widthInCells = normalized.Max(cell => cell.x) + 1;
            int heightInCells = normalized.Max(cell => cell.y) + 1;
            Vector2 xSpan = trayLocalCellXAxis * widthInCells;
            Vector2 ySpan = trayLocalCellYAxis * heightInCells;
            Vector2 size = new(
                Mathf.Abs(xSpan.x) + Mathf.Abs(ySpan.x),
                Mathf.Abs(xSpan.y) + Mathf.Abs(ySpan.y));
            if (!IsFiniteNonZero(size)) return false;

            footprint = new C1ExactBattleSandboxItemFootprint(
                Array.AsReadOnly(normalized),
                Array.AsReadOnly(mappedCells),
                visualQuarterTurns,
                trayLocalCellXAxis,
                trayLocalCellYAxis,
                size,
                widthInCells,
                heightInCells);
            return true;
        }

        private bool TryRefreshTrayLocalCellBasis()
        {
            hasTrayLocalCellBasis = false;
            RectTransform footprintParent = trayView.FootprintParent;
            if (footprintParent == null
                || !boardView.TryGetAuthoredCellWorldVectors(
                    out Vector3 worldXAxis,
                    out Vector3 worldYAxis))
                return false;

            Vector3 localXAxis = footprintParent.InverseTransformVector(worldXAxis);
            Vector3 localYAxis = footprintParent.InverseTransformVector(worldYAxis);
            trayLocalCellXAxis = new Vector2(localXAxis.x, localXAxis.y);
            trayLocalCellYAxis = new Vector2(localYAxis.x, localYAxis.y);
            hasTrayLocalCellBasis = IsFiniteNonZero(trayLocalCellXAxis)
                                    && IsFiniteNonZero(trayLocalCellYAxis);
            return hasTrayLocalCellBasis;
        }

        private bool TryBuildTrayFootprints(
            C1FormalItemSessionSnapshot snapshot,
            out IReadOnlyDictionary<string,
                C1ExactBattleSandboxItemFootprint> footprints)
        {
            Dictionary<string, C1ExactBattleSandboxItemFootprint> result =
                new(StringComparer.Ordinal);
            footprints = result;
            if (snapshot == null || !hasTrayLocalCellBasis) return false;

            foreach (C1FormalItemRosterEntrySnapshot roster in snapshot.roster)
            {
                ItemSystemCatalogItemSnapshot catalog = snapshot.itemSystemSnapshot
                    .catalogItems.FirstOrDefault(item => string.Equals(
                        item.itemId,
                        roster.baseItemId,
                        StringComparison.Ordinal));
                C1FormalItemTrayPlacementSnapshot trayPlacement = snapshot
                    .trayLayout.FindPlacementByInstanceId(
                        roster.itemInstanceId);
                int visualQuarterTurns;
                if (string.Equals(
                        roster.itemInstanceId,
                        activeItemInstanceId,
                        StringComparison.Ordinal)
                    && !activeWasPlaced)
                {
                    visualQuarterTurns = activeRotation;
                }
                else if (trayPlacement == null
                         || !C1ExactBattleSandboxItemRotationAdapter
                             .TryFormalDegreesToVisualQuarterTurns(
                                 trayPlacement.rotation,
                                 out visualQuarterTurns))
                {
                    return false;
                }
                if (catalog == null
                    || !TryCreateFootprint(
                        catalog.ShapeCells,
                        visualQuarterTurns,
                        trayLocalCellXAxis,
                        trayLocalCellYAxis,
                        out C1ExactBattleSandboxItemFootprint footprint)
                    || result.ContainsKey(roster.itemInstanceId))
                    return false;
                result.Add(roster.itemInstanceId, footprint);
            }

            return true;
        }

        private void RefreshActiveTrayFootprint()
        {
            if (authority == null || !hasTrayLocalCellBasis
                                  || activeItemInstanceId.Length == 0
                                  || activeWasPlaced)
                return;
            C1FormalItemRosterEntrySnapshot roster = authority.Current
                .FindRosterEntry(activeItemInstanceId);
            ItemSystemCatalogItemSnapshot catalog = authority.Current
                .itemSystemSnapshot.catalogItems.FirstOrDefault(item =>
                    roster != null && string.Equals(
                        item.itemId,
                        roster.baseItemId,
                        StringComparison.Ordinal));
            if (catalog != null && TryCreateFootprint(
                    catalog.ShapeCells,
                    activeRotation,
                    trayLocalCellXAxis,
                    trayLocalCellYAxis,
                    out C1ExactBattleSandboxItemFootprint footprint))
                trayView.ApplyFootprint(activeItemInstanceId, footprint);
        }

        private static bool IsFiniteNonZero(Vector2 value)
        {
            return !float.IsNaN(value.x)
                   && !float.IsInfinity(value.x)
                   && !float.IsNaN(value.y)
                   && !float.IsInfinity(value.y)
                   && value.sqrMagnitude > 0.000001f;
        }

        private void EndInteraction(PointerEventData eventData)
        {
            if (!IsItemInteractionEnabled
                || authority == null
                || activeItemInstanceId.Length == 0)
            {
                CancelInteraction();
                return;
            }

            if (activeInteractionCandidate != null
                && activeCandidateLocallyValid)
            {
                CommitPreviewedCandidate(out _);
                return;
            }

            if (!activeWasPlaced
                && activeTrayInteractionCandidate != null
                && activeTrayInteractionCandidate.IsLocallyValid)
            {
                CommitPreviewedTrayCandidate(out _);
                return;
            }

            Camera eventCamera = eventData == null
                ? null
                : eventData.pressEventCamera ?? eventData.enterEventCamera;
            if (eventData != null && activeWasPlaced
                                  && trayView.ContainsScreenPoint(
                                      eventData.position,
                                      eventCamera))
            {
                string itemInstanceId = activeItemInstanceId;
                SubmitReturnToTray(itemInstanceId, out _);
                return;
            }

            CancelInteraction();
        }

        private bool CommitPreviewedTrayCandidate(out string diagnostic)
        {
            if (!IsItemInteractionEnabled
                || authority == null
                || activeTrayInteractionCandidate == null
                || activeWasPlaced)
            {
                diagnostic = IsItemInteractionEnabled
                    ? InteractionNotActiveDiagnostic
                    : C1FormalItemInteractionAuthorizationDiagnostics
                        .InteractionLocked;
                lastDiagnostic = diagnostic;
                CancelInteraction();
                return false;
            }

            if (!activeTrayInteractionCandidate.IsLocallyValid)
            {
                diagnostic = CandidateInvalidDiagnostic;
                lastDiagnostic = diagnostic;
                CancelInteraction();
                return false;
            }

            C1ExactBattleSandboxItemTrayInteractionCandidate candidate =
                activeTrayInteractionCandidate;
            lastSubmittedTrayCandidate = candidate;
            C1FormalItemArrangementCommand command = new(
                C1FormalItemArrangementCommandKind.MoveWithinTray,
                authority.Current.sessionToken,
                authority.Current.resetGeneration,
                NextCommandId(C1FormalItemArrangementCommandKind.MoveWithinTray),
                activeItemInstanceId,
                activeExpectedSignature,
                candidate.FormalCandidate);
            C1FormalItemSessionOperationResult result = authority.Submit(command);
            diagnostic = result?.diagnosticCode
                         ?? C1FormalItemSessionDiagnosticCodes.RequestNull;
            lastDiagnostic = diagnostic;
            bool accepted = result != null && result.accepted;
            if (accepted && !CommittedTrayPlacementMatchesCandidate(
                    activeItemInstanceId,
                    candidate))
            {
                diagnostic = CandidateCommitMismatchDiagnostic;
                lastDiagnostic = diagnostic;
                accepted = false;
            }

            ResetInteractionState();
            boardView.ClearInteractionFeedback();
            trayView.ClearInteractionFeedback();
            PublishCurrent();
            return accepted;
        }

        private bool SubmitExactlyOne(
            C1FormalItemArrangementCommandKind kind,
            string itemInstanceId,
            C1ExactBattleSandboxItemInteractionCandidate candidate,
            string expectedSignature,
            out string diagnostic)
        {
            if (!IsItemInteractionEnabled || authority == null)
            {
                diagnostic = C1FormalItemInteractionAuthorizationDiagnostics
                    .InteractionLocked;
                lastDiagnostic = diagnostic;
                return false;
            }
            C1FormalItemArrangementCommand command = new(
                kind,
                authority.Current.sessionToken,
                authority.Current.resetGeneration,
                NextCommandId(kind),
                itemInstanceId,
                expectedSignature,
                candidate?.FormalCandidate);
            C1FormalItemSessionOperationResult result = authority.Submit(command);
            diagnostic = result?.diagnosticCode
                         ?? C1FormalItemSessionDiagnosticCodes.RequestNull;
            lastDiagnostic = diagnostic;
            if (result == null || !result.accepted) return false;
            if (candidate == null) return true;
            if (!CommittedPlacementMatchesCandidate(
                    itemInstanceId,
                    candidate))
            {
                diagnostic = CandidateCommitMismatchDiagnostic;
                lastDiagnostic = diagnostic;
                return false;
            }

            return true;
        }

        private bool CommittedPlacementMatchesCandidate(
            string itemInstanceId,
            C1ExactBattleSandboxItemInteractionCandidate candidate)
        {
            C1FormalItemPlacementSnapshot placement = authority?.Current
                ?.FindPlacementByInstanceId(itemInstanceId);
            ItemSystemPlacementSnapshot resolved = placement == null
                ? null
                : authority.Current.itemSystemSnapshot.FindPlacement(
                    placement.placementId);
            return placement != null
                   && resolved != null
                   && placement.anchorCell == candidate.AuthorityAnchor
                   && placement.rotation == candidate.FormalDegrees
                   && resolved.OccupiedCells.SequenceEqual(
                       candidate.OrderedOccupiedCells);
        }

        private bool CommittedTrayPlacementMatchesCandidate(
            string itemInstanceId,
            C1ExactBattleSandboxItemTrayInteractionCandidate candidate)
        {
            C1FormalItemTrayPlacementSnapshot placement = authority?.Current
                ?.trayLayout.FindPlacementByInstanceId(itemInstanceId);
            return placement != null
                   && placement.isActiveInTray
                   && placement.anchorCell == candidate.FormalCandidate.anchorCell
                   && placement.rotation == candidate.FormalDegrees
                   && placement.occupiedCells.SequenceEqual(
                       candidate.OrderedOccupiedCells);
        }

        private string NextCommandId(C1FormalItemArrangementCommandKind kind)
        {
            commandSequence++;
            return string.Join(".", new[]
            {
                "c1exact-battlesandbox-presenter",
                authority.Current.sessionToken,
                authority.Current.resetGeneration.ToString(CultureInfo.InvariantCulture),
                ((int)kind).ToString(CultureInfo.InvariantCulture),
                commandSequence.ToString(CultureInfo.InvariantCulture)
            });
        }

        private C1FormalItemDetailSelectionResult
            EvaluateAndApplyFormalItemDetail(
                C1FormalItemDetailSelectionRequest request)
        {
            C1FormalItemDetailSelectionResult result =
                C1FormalItemDetailProjectionAndSelection.Evaluate(
                    request,
                    authority?.Current,
                    itemArtworkResolver,
                    currentFormalItemDetail);
            ApplyFormalItemDetailResult(result);
            return result;
        }

        private void ApplyFormalItemDetailResult(
            C1FormalItemDetailSelectionResult result)
        {
            if (result == null) return;
            if (!result.accepted)
            {
                lastDiagnostic = result.diagnostic;
                Debug.LogWarning(
                    "[C1ExactBattleSandboxItemArrangementPresenter] "
                    + "FORMAL_ITEM_DETAIL_REJECTED itemInstanceId="
                    + selectedItemInstanceId + " diagnostic="
                    + result.diagnostic,
                    this);
                return;
            }
            if (!result.changed) return;
            currentFormalItemDetail = result;
            NotifyFormalItemDetailChanged(result);
        }

        private void RefreshOpenFormalItemDetail(
            C1FormalItemSessionSnapshot snapshot)
        {
            if (snapshot == null || currentFormalItemDetail == null
                                 || !currentFormalItemDetail.isOpen)
                return;
            C1FormalItemDetailProjection projection =
                currentFormalItemDetail.projection;
            if (!string.Equals(
                    projection.sessionToken,
                    snapshot.sessionToken,
                    StringComparison.Ordinal))
            {
                CloseFormalItemDetailForLifecycle(
                    C1FormalItemDetailDiagnostics.LifecycleRebound);
                return;
            }

            if (projection.resetGeneration != snapshot.resetGeneration)
            {
                CloseFormalItemDetailForLifecycle(
                    C1FormalItemDetailDiagnostics.LifecycleReset);
                return;
            }

            if (selectedItemInstanceId.Length == 0
                || !string.Equals(
                    selectedItemInstanceId,
                    projection.itemInstanceId,
                    StringComparison.Ordinal)
                || string.Equals(
                    projection.sourceSessionCanonicalSignature,
                    snapshot.canonicalSignature,
                    StringComparison.Ordinal))
                return;

            C1FormalItemDetailSelectionResult refreshed =
                C1FormalItemDetailProjectionAndSelection.Evaluate(
                    C1FormalItemDetailSelectionRequest.Open(
                        snapshot,
                        selectedItemInstanceId),
                    snapshot,
                    itemArtworkResolver,
                    currentFormalItemDetail);
            if (refreshed.accepted)
                ApplyFormalItemDetailResult(refreshed);
            else
                CloseFormalItemDetailForLifecycle(refreshed.diagnostic);
        }

        private void CloseFormalItemDetailForLifecycle(string diagnostic)
        {
            C1FormalItemDetailSelectionResult closed =
                C1FormalItemDetailProjectionAndSelection.CloseForLifecycle(
                    diagnostic,
                    currentFormalItemDetail);
            ApplyFormalItemDetailResult(closed);
        }

        private void NotifyFormalItemDetailChanged(
            C1FormalItemDetailSelectionResult result)
        {
            Delegate[] subscribers = formalItemDetailChanged
                ?.GetInvocationList()
                ?? Array.Empty<Delegate>();
            foreach (Delegate subscriber in subscribers)
            {
                try
                {
                    ((Action<C1FormalItemDetailSelectionResult>)subscriber)(
                        result);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                }
            }
        }

        private void ReleaseBinding(
            string lifecycleDiagnostic,
            bool clearSubscribers)
        {
            CancelAllInteractionResidue();
            CloseFormalItemDetailForLifecycle(lifecycleDiagnostic);
            if (boardView != null) boardView.UnbindPresentation();
            if (trayView != null) trayView.UnbindPresentation();
            if (boardView != null) boardView.BindPresenter(null);
            if (trayView != null) trayView.BindPresenter(null);
            authority = null;
            itemArtworkResolver = null;
            commandSequence = 0;
            selectedItemInstanceId = string.Empty;
            hasTrayLocalCellBasis = false;
            trayLocalCellXAxis = Vector2.zero;
            trayLocalCellYAxis = Vector2.zero;
            currentInteractionAuthorization =
                C1FormalItemInteractionAuthorizationResult.LockedForLifecycle(
                    null,
                    C1FormalItemInteractionAuthorizationDiagnostics
                        .LifecycleUnboundLocked,
                    CurrentInteractionAuthorization);
            if (clearSubscribers) formalItemDetailChanged = null;
        }

        private bool RejectUnlessInteractionEnabled()
        {
            if (IsItemInteractionEnabled) return true;
            lastDiagnostic = C1FormalItemInteractionAuthorizationDiagnostics
                .InteractionLocked;
            return false;
        }

        private bool AuthorizationMatchesSessionIdentity(
            C1FormalItemSessionSnapshot snapshot)
        {
            C1FormalItemInteractionAuthorizationResult current =
                CurrentInteractionAuthorization;
            return snapshot != null
                   && string.Equals(
                       current.sourceProductContext,
                       snapshot.productContext,
                       StringComparison.Ordinal)
                   && string.Equals(
                       current.sourceSessionToken,
                       snapshot.sessionToken,
                       StringComparison.Ordinal)
                   && current.sourceResetGeneration == snapshot.resetGeneration;
        }

        private void LockInteractionForLifecycle(
            C1FormalItemSessionSnapshot snapshot,
            string diagnostic)
        {
            currentInteractionAuthorization =
                C1FormalItemInteractionAuthorizationResult.LockedForLifecycle(
                    snapshot,
                    diagnostic,
                    CurrentInteractionAuthorization);
            CancelAllInteractionResidue();
        }

        private void CancelAllInteractionResidue()
        {
            ResetInteractionState();
            if (boardView != null) boardView.CancelInteractionState();
            if (trayView != null) trayView.CancelItemInteractions();
        }

        private void ResetInteractionState()
        {
            activeItemInstanceId = string.Empty;
            activeExpectedSignature = string.Empty;
            activeWasPlaced = false;
            activeRotation = 0;
            activeRawGrabbedCell = default;
            hasActiveRawGrabbedCell = false;
            activePointerCell = default;
            hasActivePointerCell = false;
            activeCandidateLocallyValid = false;
            rotateZoneLatched = false;
            RightRotateZoneState.Reset();
            activeInteractionCandidate = null;
            activeTrayInteractionCandidate = null;
            activeCandidate = null;
        }

        private void ClearActiveCandidateFeedback()
        {
            ClearBoardCandidateFeedback();
            activeTrayInteractionCandidate = null;
            if (trayView != null) trayView.ClearInteractionFeedback();
        }

        private void ClearBoardCandidateFeedback()
        {
            activeInteractionCandidate = null;
            activeCandidate = null;
            activePointerCell = default;
            hasActivePointerCell = false;
            activeCandidateLocallyValid = false;
            rotateZoneLatched = false;
            RightRotateZoneState.Reset();
            boardView.ClearInteractionFeedback();
        }

        private void LogMissingOnce()
        {
            if (missingReferenceLogged) return;
            missingReferenceLogged = true;
            Debug.LogError("[C1ExactBattleSandboxItemArrangementPresenter] "
                           + MissingReferenceDiagnostic, this);
        }

        private void OnDisable()
        {
            Unbind();
        }
    }
}
