using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Items.CampaignBaseline
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxItemTrayView : MonoBehaviour
    {
        public const string MissingReferenceDiagnostic =
            "C1_EXACT_BATTLESANDBOX_TRAY_AUTHORED_REFERENCE_MISSING";
        public const int PhysicalPresentationCapacity =
            C1FormalItemSessionContract.TrayCellCount;

        private const int InitialPrewarmCount = 7;

        [SerializeField] private RectTransform trayRect;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectMask2D viewportMask;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform itemCardLayer;
        [SerializeField] private RectTransform[] trayCellRects =
            Array.Empty<RectTransform>();
        [SerializeField] private Image[] trayCellImages = Array.Empty<Image>();
        [SerializeField] private C1ExactBattleSandboxItemCardView[] cardViews =
            Array.Empty<C1ExactBattleSandboxItemCardView>();

        private C1ExactBattleSandboxItemArrangementPresenter presenter;
        private C1FormalItemSessionSnapshot boundSnapshot;
        private string boundSnapshotSignature = string.Empty;
        private Color[] authoredCellColors = Array.Empty<Color>();
        private bool authoredCellColorsCaptured;
        private readonly List<TrayPoolEntry> presentationPool = new();
        private readonly Dictionary<string, TrayPoolEntry> activePoolEntries =
            new(StringComparer.Ordinal);
        private bool presentationPoolInitialized;

        private sealed class TrayPoolEntry
        {
            public TrayPoolEntry(C1ExactBattleSandboxItemCardView card)
            {
                Card = card;
            }

            public C1ExactBattleSandboxItemCardView Card { get; }
            public string ItemInstanceId { get; set; } = string.Empty;
        }

        public ScrollRect AuthoredScrollRect => scrollRect;
        public string BoundSnapshotSignature => boundSnapshotSignature;
        public int VisibleCardCount => activePoolEntries.Values.Count(entry =>
            entry.Card != null && entry.Card.HasBoundItem);
        public int AuthoredCardCapacity => cardViews?.Length ?? 0;
        public int PresentationPoolCount => presentationPoolInitialized
            ? presentationPool.Count
            : AuthoredCardCapacity;
        public int AuthoredCellCount => trayCellRects?.Length ?? 0;
        internal RectTransform FootprintParent => itemCardLayer;

        public void AssignForEditor(
            RectTransform configuredTrayRect,
            ScrollRect configuredScrollRect,
            RectTransform configuredViewport,
            RectMask2D configuredViewportMask,
            RectTransform configuredContentRoot,
            RectTransform configuredItemCardLayer,
            C1ExactBattleSandboxItemCardView[] configuredCardViews,
            RectTransform[] configuredTrayCellRects,
            Image[] configuredTrayCellImages)
        {
            trayRect = configuredTrayRect;
            scrollRect = configuredScrollRect;
            viewport = configuredViewport;
            viewportMask = configuredViewportMask;
            contentRoot = configuredContentRoot;
            itemCardLayer = configuredItemCardLayer;
            cardViews = configuredCardViews
                        ?? Array.Empty<C1ExactBattleSandboxItemCardView>();
            trayCellRects = configuredTrayCellRects ?? Array.Empty<RectTransform>();
            trayCellImages = configuredTrayCellImages ?? Array.Empty<Image>();
            authoredCellColorsCaptured = false;
            authoredCellColors = Array.Empty<Color>();
            presentationPoolInitialized = false;
            presentationPool.Clear();
            activePoolEntries.Clear();
        }

        public bool ValidateAuthoredReferences()
        {
            if (trayRect == null
                || scrollRect == null
                || viewport == null
                || viewportMask == null
                || contentRoot == null
                || itemCardLayer == null
                || scrollRect.viewport != viewport
                || scrollRect.content != contentRoot
                || contentRoot.parent != viewport
                || itemCardLayer.parent != contentRoot
                || trayCellRects == null
                || trayCellImages == null
                || trayCellRects.Length != C1FormalItemSessionContract.TrayCellCount
                || trayCellImages.Length != C1FormalItemSessionContract.TrayCellCount
                || trayCellRects.Distinct().Count() != trayCellRects.Length
                || trayCellImages.Distinct().Count() != trayCellImages.Length
                || cardViews == null
                || cardViews.Length == 0
                || cardViews.Length > PhysicalPresentationCapacity
                || cardViews.Any(card => card == null
                    || !card.ValidateAuthoredReferences()
                    || card.transform.parent != itemCardLayer)
                || cardViews.Distinct().Count() != cardViews.Length)
                return false;

            for (int index = 0; index < trayCellRects.Length; index++)
            {
                RectTransform rect = trayCellRects[index];
                Image image = trayCellImages[index];
                if (rect == null
                    || image == null
                    || image.rectTransform != rect
                    || rect.parent != contentRoot
                    || !string.Equals(
                        rect.name,
                        "TrayGridSlot_" + (index + 1).ToString("00"),
                        StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        internal void BindPresenter(
            C1ExactBattleSandboxItemArrangementPresenter configuredPresenter)
        {
            presenter = configuredPresenter;
            IEnumerable<C1ExactBattleSandboxItemCardView> cards =
                presentationPoolInitialized
                    ? presentationPool.Select(value => value.Card)
                    : cardViews ?? Array.Empty<C1ExactBattleSandboxItemCardView>();
            foreach (C1ExactBattleSandboxItemCardView card in cards)
            {
                if (card != null) card.BindPresenter(presenter);
            }

            if (presenter == null || !presenter.IsItemInteractionEnabled)
                CancelItemInteractions();
        }

        internal void CancelItemInteractions()
        {
            IEnumerable<C1ExactBattleSandboxItemCardView> cards =
                presentationPoolInitialized
                    ? presentationPool.Select(value => value.Card)
                    : cardViews ?? Array.Empty<C1ExactBattleSandboxItemCardView>();
            foreach (C1ExactBattleSandboxItemCardView card in cards)
            {
                if (card != null) card.CancelInteractionState();
            }
            ClearInteractionFeedback();
        }

        internal void Bind(
            C1FormalItemSessionSnapshot snapshot,
            string selectedItemInstanceId,
            IReadOnlyDictionary<string,
                C1ExactBattleSandboxItemFootprint> footprints,
            C1FormalItemPresentationCatalogSnapshot presentationCatalog)
        {
            boundSnapshot = snapshot;
            boundSnapshotSignature = snapshot?.canonicalSignature ?? string.Empty;
            if (contentRoot != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            ClearInteractionFeedback();
            if (!ValidateAuthoredReferences())
            {
                ClearBoundCards();
                boundSnapshotSignature = string.Empty;
                return;
            }
            C1FormalItemTrayPlacementSnapshot[] trayRows = snapshot?.trayLayout == null
                ? Array.Empty<C1FormalItemTrayPlacementSnapshot>()
                : snapshot.trayLayout.placements
                    .Where(row => row.isActiveInTray)
                    .OrderBy(row => row.itemInstanceId, StringComparer.Ordinal)
                    .ToArray();
            if (snapshot != null && !C1ExactBattleSandboxItemBoardView
                    .CanBindExactInstanceRows(
                        PhysicalPresentationCapacity,
                        trayRows.Select(value =>
                        {
                            C1FormalItemRosterEntrySnapshot roster = value == null
                                ? null
                                : snapshot.FindRosterEntry(value.itemInstanceId);
                            return new KeyValuePair<string, string>(
                                value?.itemInstanceId,
                                roster?.baseItemId);
                        }),
                        snapshot.roster.Select(value =>
                            new KeyValuePair<string, string>(
                                value?.itemInstanceId,
                                value?.baseItemId))))
            {
                ClearBoundCards();
                boundSnapshotSignature = string.Empty;
                return;
            }

            if (presentationCatalog == null
                || !PreparePoolForDemand(trayRows.Select(value =>
                    value.itemInstanceId)))
            {
                ClearBoundCards();
                boundSnapshotSignature = string.Empty;
                return;
            }

            foreach (C1FormalItemTrayPlacementSnapshot trayPlacement in trayRows)
            {
                if (!activePoolEntries.TryGetValue(
                        trayPlacement.itemInstanceId,
                        out TrayPoolEntry entry))
                {
                    ClearBoundCards();
                    boundSnapshotSignature = string.Empty;
                    return;
                }

                C1ExactBattleSandboxItemCardView card = entry.Card;
                C1FormalItemRosterEntrySnapshot roster = snapshot.FindRosterEntry(
                    trayPlacement.itemInstanceId);
                C1ExactBattleSandboxItemFootprint footprint = null;
                footprints?.TryGetValue(
                    trayPlacement.itemInstanceId,
                    out footprint);
                if (roster == null
                    || !card.Bind(
                        roster,
                        null,
                        null,
                        footprint,
                        presentationCatalog)
                    || !TryPositionCard(card, trayPlacement))
                {
                    ClearBoundCards();
                    boundSnapshotSignature = string.Empty;
                    return;
                }

                card.SetSelected(string.Equals(
                    selectedItemInstanceId,
                    roster.itemInstanceId,
                    StringComparison.Ordinal));
            }
        }

        private void ClearBoundCards()
        {
            if (!presentationPoolInitialized) InitializePresentationPool(false);
            foreach (TrayPoolEntry entry in presentationPool)
            {
                if (entry.Card != null) entry.Card.Recycle();
                entry.ItemInstanceId = string.Empty;
            }
            activePoolEntries.Clear();
        }

        internal void UnbindPresentation()
        {
            boundSnapshot = null;
            boundSnapshotSignature = string.Empty;
            ClearBoundCards();
            ClearInteractionFeedback();
        }

        private bool PreparePoolForDemand(IEnumerable<string> itemInstanceIds)
        {
            if (itemInstanceIds == null || !InitializePresentationPool(true))
                return false;
            string[] desired = itemInstanceIds.ToArray();
            if (desired.Length > PhysicalPresentationCapacity
                || desired.Any(string.IsNullOrWhiteSpace)
                || desired.Distinct(StringComparer.Ordinal).Count()
                   != desired.Length
                || !EnsurePresentationPoolCapacity(Math.Max(
                    desired.Length,
                    Math.Min(InitialPrewarmCount,
                        PhysicalPresentationCapacity))))
                return false;

            HashSet<string> desiredSet = new(
                desired,
                StringComparer.Ordinal);
            foreach (TrayPoolEntry entry in presentationPool)
            {
                if (entry.ItemInstanceId.Length > 0
                    && !desiredSet.Contains(entry.ItemInstanceId))
                {
                    activePoolEntries.Remove(entry.ItemInstanceId);
                    entry.ItemInstanceId = string.Empty;
                    entry.Card?.Recycle();
                }
                else
                {
                    entry.Card?.Clear();
                }
            }

            foreach (string itemInstanceId in desired)
            {
                if (activePoolEntries.ContainsKey(itemInstanceId)) continue;
                TrayPoolEntry entry = presentationPool.FirstOrDefault(value =>
                    value.ItemInstanceId.Length == 0);
                if (entry == null) return false;
                entry.ItemInstanceId = itemInstanceId;
                activePoolEntries.Add(itemInstanceId, entry);
            }

            foreach (TrayPoolEntry entry in activePoolEntries.Values)
                entry.Card?.BindPresenter(presenter);
            return activePoolEntries.Count == desired.Length;
        }

        private bool InitializePresentationPool(bool prewarm)
        {
            if (!presentationPoolInitialized)
            {
                presentationPool.Clear();
                activePoolEntries.Clear();
                if (cardViews == null || cardViews.Length == 0)
                    return false;
                foreach (C1ExactBattleSandboxItemCardView card in cardViews)
                {
                    if (card == null || card.transform.parent != itemCardLayer)
                        return false;
                    TrayPoolEntry entry = new(card);
                    presentationPool.Add(entry);
                    card.Recycle();
                }
                presentationPoolInitialized = true;
            }

            return !prewarm || EnsurePresentationPoolCapacity(Math.Min(
                InitialPrewarmCount,
                PhysicalPresentationCapacity));
        }

        private bool EnsurePresentationPoolCapacity(int requiredCapacity)
        {
            if (!presentationPoolInitialized
                || requiredCapacity < 0
                || requiredCapacity > PhysicalPresentationCapacity)
                return false;
            if (presentationPool.Count >= requiredCapacity) return true;
            C1ExactBattleSandboxItemCardView template = presentationPool
                .Select(value => value.Card)
                .FirstOrDefault(value => value != null);
            if (template == null || itemCardLayer == null) return false;
            while (presentationPool.Count < requiredCapacity)
            {
                GameObject clone = Instantiate(
                    template.gameObject,
                    itemCardLayer,
                    false);
                if (clone == null) return false;
                clone.name = "FormalItemCard_RuntimePool_"
                             + (presentationPool.Count + 1).ToString("00");
                C1ExactBattleSandboxItemCardView card = clone.GetComponent<
                    C1ExactBattleSandboxItemCardView>();
                if (card == null || !card.ValidateAuthoredReferences())
                {
                    Destroy(clone);
                    return false;
                }
                TrayPoolEntry entry = new(card);
                presentationPool.Add(entry);
                card.Recycle();
            }

            return true;
        }

        internal void ApplyFootprint(
            string itemInstanceId,
            C1ExactBattleSandboxItemFootprint footprint)
        {
            C1ExactBattleSandboxItemCardView card = FindBoundCard(itemInstanceId);
            C1FormalItemTrayPlacementSnapshot placement = boundSnapshot?.trayLayout
                ?.FindPlacementByInstanceId(itemInstanceId);
            if (card == null || placement == null || !card.ApplyFootprint(footprint))
                return;
            TryPositionCard(card, placement);
        }

        internal bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            return trayRect != null && RectTransformUtility.RectangleContainsScreenPoint(
                trayRect,
                screenPoint,
                eventCamera);
        }

        internal bool TryScreenPointToCell(
            Vector2 screenPoint,
            Camera eventCamera,
            out Vector2Int cell)
        {
            for (int index = 0; index < (trayCellRects?.Length ?? 0); index++)
            {
                RectTransform rect = trayCellRects[index];
                if (rect == null
                    || !rect.gameObject.activeInHierarchy
                    || !RectTransformUtility.RectangleContainsScreenPoint(
                        rect,
                        screenPoint,
                        eventCamera))
                    continue;
                cell = new Vector2Int(
                    index % C1FormalItemSessionContract.TrayColumnCount,
                    index / C1FormalItemSessionContract.TrayColumnCount);
                return true;
            }

            cell = default;
            return false;
        }

        internal void ShowCandidate(
            C1ExactBattleSandboxItemTrayInteractionCandidate candidate)
        {
            ClearInteractionFeedback();
            if (candidate == null || !CaptureAuthoredCellColors()) return;
            Color tint = candidate.IsLocallyValid
                ? new Color(0.3f, 1f, 0.55f, 0.42f)
                : new Color(1f, 0.28f, 0.2f, 0.48f);
            foreach (Vector2Int cell in candidate.OrderedOccupiedCells)
            {
                int index = cell.y * C1FormalItemSessionContract.TrayColumnCount
                            + cell.x;
                if (index < 0 || index >= trayCellImages.Length) continue;
                trayCellImages[index].color = tint;
            }
        }

        internal void ClearInteractionFeedback()
        {
            if (!CaptureAuthoredCellColors()) return;
            for (int index = 0; index < trayCellImages.Length; index++)
            {
                if (trayCellImages[index] != null)
                    trayCellImages[index].color = authoredCellColors[index];
            }
        }

        internal C1ExactBattleSandboxItemCardView FindBoundCard(
            string itemInstanceId)
        {
            return !string.IsNullOrWhiteSpace(itemInstanceId)
                   && activePoolEntries.TryGetValue(
                       itemInstanceId.Trim(),
                       out TrayPoolEntry entry)
                ? entry.Card
                : null;
        }

        internal bool TryGetAuthoredCellCenter(
            Vector2Int cell,
            out Vector2 localCenter)
        {
            localCenter = default;
            if (itemCardLayer == null
                || cell.x < 0
                || cell.x >= C1FormalItemSessionContract.TrayColumnCount
                || cell.y < 0
                || cell.y >= C1FormalItemSessionContract.TrayRowCount)
                return false;
            int index = cell.y * C1FormalItemSessionContract.TrayColumnCount
                        + cell.x;
            RectTransform rect = trayCellRects[index];
            if (rect == null) return false;
            Vector3 world = rect.TransformPoint(rect.rect.center);
            Vector3 local = itemCardLayer.InverseTransformPoint(world);
            localCenter = new Vector2(local.x, local.y);
            return true;
        }

        private bool TryPositionCard(
            C1ExactBattleSandboxItemCardView card,
            C1FormalItemTrayPlacementSnapshot placement)
        {
            if (card == null
                || placement == null
                || placement.occupiedCells == null
                || placement.occupiedCells.Count == 0)
                return false;
            List<Vector2> centers = new List<Vector2>();
            foreach (Vector2Int cell in placement.occupiedCells)
            {
                if (!TryGetAuthoredCellCenter(cell, out Vector2 center))
                    return false;
                centers.Add(center);
            }

            Vector2 min = new Vector2(
                centers.Min(value => value.x),
                centers.Min(value => value.y));
            Vector2 max = new Vector2(
                centers.Max(value => value.x),
                centers.Max(value => value.y));
            Vector2 target = (min + max) * 0.5f;
            RectTransform cardRect = card.transform as RectTransform;
            if (cardRect == null || cardRect.parent != itemCardLayer) return false;
            cardRect.localPosition = new Vector3(
                target.x,
                target.y,
                cardRect.localPosition.z);
            return true;
        }

        private bool CaptureAuthoredCellColors()
        {
            if (authoredCellColorsCaptured) return true;
            if (trayCellImages == null
                || trayCellImages.Length != C1FormalItemSessionContract.TrayCellCount
                || trayCellImages.Any(value => value == null))
                return false;
            authoredCellColors = trayCellImages.Select(value => value.color).ToArray();
            authoredCellColorsCaptured = true;
            return true;
        }
    }
}
