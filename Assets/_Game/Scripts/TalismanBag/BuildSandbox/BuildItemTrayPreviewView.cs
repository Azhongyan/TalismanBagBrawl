using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TalismanBag.BuildSandbox
{
    [ExecuteAlways]
    public sealed class BuildItemTrayPreviewView : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IScrollHandler
    {
        public const bool SupportsShapeAwareCellSpans = true;
        public const string AllCategory = "\u5168\u90e8";
        private const string BasicCategory = "基础";
        private const string AdvancedCategory = "进阶";
        private const string CoreCategory = "核心";
        private const string SupportCategory = "辅助";
        private const string ArtifactCategory = "\u6cd5\u5668";
        private const string TestDevOnlyCategory = "测试";

        private const string ItemCardLayerName = "ItemCardLayer";
        private const float TrayRowSnapVelocityPitchRatio = 0.2f;
        private const float TrayRowSnapIdleSeconds = 0.12f;
        private const float TrayRowSnapTolerance = 0.25f;
        public const int AuthoredTraySlotCount = 40;
        public const int ItemSystemAuthorityLogicalSlotCount = 65;
        public const int ItemSystemAuthorityLogicalRows =
            ItemSystemAuthorityLogicalSlotCount
            / BuildGridInteractionPreviewController.TrayColumns;
        public const int ItemSystemAuthorityRuntimeSlotCount =
            ItemSystemAuthorityLogicalSlotCount - AuthoredTraySlotCount;
        public const string ItemSystemAuthorityRuntimeSlotNamePrefix =
            "TrayGridSlot_Runtime_";
        private const HideFlags RuntimeSlotHideFlags =
            HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private RectTransform itemCardLayer;
        [SerializeField] private List<Button> categoryButtons = new();
        [SerializeField] private List<Text> categoryLabels = new();
        [SerializeField] private List<RectTransform> traySlotRects = new();
        [SerializeField] private List<Image> traySlotImages = new();
        [SerializeField] private List<Outline> traySlotOutlines = new();
        [SerializeField] private List<BuildItemPreviewCardView> cards = new();

        private readonly Dictionary<string, Button> buttonsByCategory = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Text> labelsByCategory = new(StringComparer.Ordinal);
        private readonly Dictionary<string, BuildItemPreviewCardView> cardsByItemId = new(StringComparer.Ordinal);
        // This is a transient view projection only.  placementModels remains the canonical All layout.
        private readonly Dictionary<string, TrayPlacementViewModel> displayedPlacementsByItemId =
            new(StringComparer.Ordinal);
        private readonly List<TrayPlacementViewModel> placementModels = new();
        private readonly HashSet<string> hiddenItemIds = new(StringComparer.Ordinal);
        private readonly TrayItemLayoutView itemLayoutView = new();
        private readonly TrayGridReservationView reservationView = new();
        private readonly List<GameObject> itemSystemAuthorityRuntimeSlots = new();
        private List<BuildGridInteractionPreviewController.PreviewItem> currentItems = new();
        private List<RectTransform> preAuthorityTraySlotRects;
        private List<Image> preAuthorityTraySlotImages;
        private List<Outline> preAuthorityTraySlotOutlines;
        private BuildGridInteractionPreviewController controller;
        private string activeCategory = AllCategory;
        private float preAuthorityContentSizeDeltaY;
        private bool hasPreAuthorityContentHeight;
        private bool itemSystemAuthorityRuntimeSlotsInstalled;
        private int trayViewTransactionDepth;
        private bool trayViewPublishPending;
        private bool trayScrollResetToTopPending;
        private List<TrayPlacementViewModel> transactionPlacementBackup;
        private HashSet<string> transactionHiddenItemIdsBackup;
        private bool trayScrollPointerDragging;
        private bool trayRowSnapPending;
        private bool applyingTrayRowSnap;
        private float lastTrayScrollMutationTime = -999f;
        private float lastTrayAutoScrollTime = -999f;
        private float lastObservedVerticalNormalizedPosition = float.NaN;
        private int trayViewPublicationRevision;
        private ScrollRect observedRowSnapScrollRect;
        private EventTrigger observedRowSnapEventTrigger;
        private EventTrigger.Entry observedBeginDragEntry;
        private EventTrigger.Entry observedDragEntry;
        private EventTrigger.Entry observedEndDragEntry;
        private EventTrigger.Entry observedScrollEntry;
        private bool ownsObservedRowSnapEventTrigger;

#if UNITY_EDITOR
        private bool editModePreviewRefreshQueued;
#endif

        public int TraySlotCount => GetTraySlotCount();
        public int CategoryCount => categoryLabels.Count(label => label != null
            && (!Application.isPlaying || label.gameObject.activeInHierarchy));
        public string ActiveCategory => activeCategory;
        public bool HasItemSystemAuthorityRuntimeSlots =>
            itemSystemAuthorityRuntimeSlotsInstalled;
        public int InstalledItemSystemAuthorityRuntimeSlotCount =>
            itemSystemAuthorityRuntimeSlots.Count(slot => slot != null);
        public bool IsTrayViewTransactionActive => trayViewTransactionDepth > 0;
        public int TrayViewPublicationRevision => trayViewPublicationRevision;
        public bool HasProductionRowSnapObserver =>
            observedRowSnapScrollRect != null
            && observedRowSnapScrollRect == scrollRect
            && observedRowSnapEventTrigger != null;

        public bool TryGetDisplayedPlacement(
            string itemId,
            out TrayPlacementViewModel placement)
        {
            return displayedPlacementsByItemId.TryGetValue(itemId ?? string.Empty,
                out placement);
        }

        public void BeginTrayViewTransaction()
        {
            if (trayViewTransactionDepth == 0)
            {
                transactionPlacementBackup = placementModels.ToList();
                transactionHiddenItemIdsBackup =
                    new HashSet<string>(hiddenItemIds, StringComparer.Ordinal);
                trayViewPublishPending = false;
                trayScrollResetToTopPending = false;
            }
            trayViewTransactionDepth++;
        }

        public bool CommitTrayViewTransaction()
        {
            if (trayViewTransactionDepth <= 0)
            {
                return false;
            }

            trayViewTransactionDepth--;
            if (trayViewTransactionDepth > 0)
            {
                return true;
            }

            bool shouldPublish = trayViewPublishPending;
            bool shouldResetScrollToTop = trayScrollResetToTopPending;
            try
            {
                if (shouldPublish)
                {
                    PublishTrayViews(BuildVisibleItemIds());
                }
                if (shouldResetScrollToTop)
                {
                    ResetTrayScrollToTop();
                }
                ClearTrayViewTransactionBackup();
                return true;
            }
            catch
            {
                RestoreTrayViewTransactionModelBackup();
                try
                {
                    PublishTrayViews(BuildVisibleItemIds());
                }
                catch
                {
                    // Preserve the original publication exception.  The staged
                    // model has already been rolled back to the last snapshot.
                }
                ClearTrayViewTransactionBackup();
                throw;
            }
        }

        public void RollbackTrayViewTransaction()
        {
            if (trayViewTransactionDepth <= 0)
            {
                return;
            }

            RestoreTrayViewTransactionModelBackup();
            trayViewTransactionDepth = 0;
            ClearTrayViewTransactionBackup();
        }

        private void RestoreTrayViewTransactionModelBackup()
        {
            placementModels.Clear();
            placementModels.AddRange(transactionPlacementBackup
                ?? Enumerable.Empty<TrayPlacementViewModel>());
            hiddenItemIds.Clear();
            if (transactionHiddenItemIdsBackup != null)
            {
                hiddenItemIds.UnionWith(transactionHiddenItemIdsBackup);
            }
            activeCategory = AllCategory;
        }

        private void ClearTrayViewTransactionBackup()
        {
            transactionPlacementBackup = null;
            transactionHiddenItemIdsBackup = null;
            trayViewPublishPending = false;
            trayScrollResetToTopPending = false;
        }

        public bool InstallItemSystemAuthorityRuntimeSlots(out string diagnosticCode)
        {
            diagnosticCode = string.Empty;
            if (itemSystemAuthorityRuntimeSlotsInstalled)
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_SLOTS_ALREADY_INSTALLED";
                return false;
            }
            if (contentRoot == null)
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_CONTENT_MISSING";
                return false;
            }

            GridLayoutGroup grid = contentRoot.GetComponent<GridLayoutGroup>();
            if (grid == null
                || grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount
                || grid.constraintCount != BuildGridInteractionPreviewController.TrayColumns)
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_GRID_GEOMETRY_INVALID";
                return false;
            }

            List<RectTransform> authoredSlots = new(AuthoredTraySlotCount);
            int[] authoredSiblingIndexes = new int[AuthoredTraySlotCount];
            for (int index = 0; index < AuthoredTraySlotCount; index++)
            {
                string slotName = $"TrayGridSlot_{index + 1:00}";
                RectTransform slot = contentRoot.Find(slotName) as RectTransform;
                if (slot == null || slot.parent != contentRoot)
                {
                    diagnosticCode = "ITEM_SYSTEM_AUTHORED_TRAY_SLOT_MISSING";
                    return false;
                }
                authoredSlots.Add(slot);
                authoredSiblingIndexes[index] = slot.GetSiblingIndex();
            }

            for (int index = AuthoredTraySlotCount + 1;
                 index <= ItemSystemAuthorityLogicalSlotCount;
                 index++)
            {
                if (contentRoot.Find(ItemSystemAuthorityRuntimeSlotNamePrefix
                        + index.ToString("00")) != null)
                {
                    diagnosticCode = "ITEM_SYSTEM_RUNTIME_TRAY_SLOT_DUPLICATE";
                    return false;
                }
            }

            RectTransform template = authoredSlots[AuthoredTraySlotCount - 1];
            if (template.GetComponent<Image>() == null
                || template.GetComponent<Outline>() == null)
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_SLOT_TEMPLATE_INVALID";
                return false;
            }

            preAuthorityTraySlotRects = traySlotRects.ToList();
            preAuthorityTraySlotImages = traySlotImages.ToList();
            preAuthorityTraySlotOutlines = traySlotOutlines.ToList();
            preAuthorityContentSizeDeltaY = contentRoot.sizeDelta.y;
            hasPreAuthorityContentHeight = true;

            try
            {
                List<RectTransform> extendedRects = preAuthorityTraySlotRects.ToList();
                List<Image> extendedImages = preAuthorityTraySlotImages.ToList();
                List<Outline> extendedOutlines = preAuthorityTraySlotOutlines.ToList();
                for (int index = AuthoredTraySlotCount + 1;
                     index <= ItemSystemAuthorityLogicalSlotCount;
                     index++)
                {
                    GameObject runtimeSlot = Instantiate(
                        template.gameObject, contentRoot, worldPositionStays: false);
                    runtimeSlot.name = ItemSystemAuthorityRuntimeSlotNamePrefix
                        + index.ToString("00");
                    runtimeSlot.hideFlags = RuntimeSlotHideFlags;
                    foreach (Component component in runtimeSlot.GetComponents<Component>())
                    {
                        component.hideFlags = RuntimeSlotHideFlags;
                    }

                    RectTransform runtimeRect = runtimeSlot.GetComponent<RectTransform>();
                    Image runtimeImage = runtimeSlot.GetComponent<Image>();
                    Outline runtimeOutline = runtimeSlot.GetComponent<Outline>();
                    if (runtimeRect == null || runtimeImage == null || runtimeOutline == null)
                    {
                        throw new InvalidOperationException(
                            "Runtime tray slot did not preserve the authored component pattern.");
                    }

                    itemSystemAuthorityRuntimeSlots.Add(runtimeSlot);
                    extendedRects.Add(runtimeRect);
                    extendedImages.Add(runtimeImage);
                    extendedOutlines.Add(runtimeOutline);
                }

                traySlotRects = extendedRects;
                traySlotImages = extendedImages;
                traySlotOutlines = extendedOutlines;
                BindViewAuthorities();
                EnsureContentHeightFromGrid();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);

                bool authoredSlotsPreserved = authoredSlots.Select((slot, index) =>
                        slot != null
                        && slot.parent == contentRoot
                        && slot.GetSiblingIndex() == authoredSiblingIndexes[index]
                        && string.Equals(slot.name, $"TrayGridSlot_{index + 1:00}",
                            StringComparison.Ordinal))
                    .All(value => value);
                if (!authoredSlotsPreserved
                    || itemSystemAuthorityRuntimeSlots.Count
                        != ItemSystemAuthorityRuntimeSlotCount
                    || GetTraySlotCount() != ItemSystemAuthorityLogicalSlotCount)
                {
                    throw new InvalidOperationException(
                        "Runtime tray extension did not preserve the 40 authored slots.");
                }

                itemSystemAuthorityRuntimeSlotsInstalled = true;
                diagnosticCode = "NONE";
                return true;
            }
            catch
            {
                RestoreItemSystemAuthorityRuntimeSlots();
                diagnosticCode = "ITEM_SYSTEM_RUNTIME_TRAY_SLOT_INSTALL_FAILED";
                return false;
            }
        }

        public void UninstallItemSystemAuthorityRuntimeSlots()
        {
            RestoreItemSystemAuthorityRuntimeSlots();
        }

        private void RestoreItemSystemAuthorityRuntimeSlots()
        {
            foreach (GameObject runtimeSlot in itemSystemAuthorityRuntimeSlots)
            {
                if (runtimeSlot == null)
                {
                    continue;
                }
                if (Application.isPlaying)
                {
                    Destroy(runtimeSlot);
                }
                else
                {
                    DestroyImmediate(runtimeSlot);
                }
            }
            itemSystemAuthorityRuntimeSlots.Clear();

            if (preAuthorityTraySlotRects != null)
            {
                traySlotRects = preAuthorityTraySlotRects;
            }
            if (preAuthorityTraySlotImages != null)
            {
                traySlotImages = preAuthorityTraySlotImages;
            }
            if (preAuthorityTraySlotOutlines != null)
            {
                traySlotOutlines = preAuthorityTraySlotOutlines;
            }
            if (contentRoot != null && hasPreAuthorityContentHeight)
            {
                Vector2 restoredSizeDelta = contentRoot.sizeDelta;
                restoredSizeDelta.y = preAuthorityContentSizeDeltaY;
                contentRoot.sizeDelta = restoredSizeDelta;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            }

            preAuthorityTraySlotRects = null;
            preAuthorityTraySlotImages = null;
            preAuthorityTraySlotOutlines = null;
            hasPreAuthorityContentHeight = false;
            itemSystemAuthorityRuntimeSlotsInstalled = false;
            BindViewAuthorities();
        }

        public void Bind(
            ScrollRect trayScrollRect,
            RectTransform trayContentRoot,
            RectTransform trayItemCardLayer,
            IReadOnlyList<Button> buttons,
            IReadOnlyList<Text> labels,
            IReadOnlyList<RectTransform> slotRects,
            IReadOnlyList<Image> slotImages,
            IReadOnlyList<Outline> slotOutlines,
            IReadOnlyList<BuildItemPreviewCardView> cardViews)
        {
            scrollRect = trayScrollRect;
            contentRoot = trayContentRoot;
            itemCardLayer = trayItemCardLayer;
            categoryButtons = (buttons ?? Array.Empty<Button>()).Where(button => button != null).ToList();
            categoryLabels = (labels ?? Array.Empty<Text>()).Where(label => label != null).ToList();
            traySlotRects = (slotRects ?? Array.Empty<RectTransform>()).ToList();
            traySlotImages = (slotImages ?? Array.Empty<Image>()).ToList();
            traySlotOutlines = (slotOutlines ?? Array.Empty<Outline>()).ToList();
            cards = (cardViews ?? Array.Empty<BuildItemPreviewCardView>())
                .Where(card => card != null)
                .ToList();
            BindViewAuthorities();
        }

        public void Initialize(
            BuildGridInteractionPreviewController controller,
            IReadOnlyList<BuildGridInteractionPreviewController.PreviewItem> items,
            IReadOnlyList<string> categories)
        {
            this.controller = controller;
            currentItems = (items ?? Array.Empty<BuildGridInteractionPreviewController.PreviewItem>())
                .Where(item => item != null)
                .OrderBy(item => item.ItemId, StringComparer.Ordinal)
                .ToList();
            hiddenItemIds.Clear();

            InitializeCategoryButtons(
                controller,
                Application.isPlaying
                    ? new[] { AllCategory }
                    : categories ?? Array.Empty<string>());
            BindViewAuthorities();
            InitializeCards(controller);
            EnsureRuntimeMatureScrollArea();
            ApplyFilter(AllCategory);
        }

        public void ApplyFilter(string category)
        {
            activeCategory = AllCategory;
            controller?.NotifyTrayCategoryChanged();

            HashSet<string> visibleIds = BuildVisibleItemIds();
            RefreshTrayViews(visibleIds);

            if (Application.isPlaying && scrollRect != null)
            {
                if (trayViewTransactionDepth > 0)
                {
                    trayScrollResetToTopPending = true;
                }
                else
                {
                    ResetTrayScrollToTop();
                }
            }
        }

        private void ResetTrayScrollToTop()
        {
            if (scrollRect == null)
            {
                return;
            }
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
            RequestTrayRowSnap();
        }

        public void RefreshItemPlacement(TrayPlacementViewModel placement)
        {
            if (placement == null || string.IsNullOrWhiteSpace(placement.itemId))
            {
                return;
            }

            hiddenItemIds.Remove(placement.itemId);
            int existingIndex = placementModels.FindIndex(model =>
                model != null && string.Equals(model.itemId, placement.itemId, StringComparison.Ordinal));
            if (existingIndex >= 0)
            {
                placementModels[existingIndex] = placement;
            }
            else
            {
                placementModels.Add(placement);
            }

            RefreshTrayViews(BuildVisibleItemIds());
        }

        public void SetItemInTray(string itemId, bool inTray)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return;
            }

            if (inTray)
            {
                hiddenItemIds.Remove(itemId);
                // Return callers publish the new physical placement through
                // RefreshItemPlacement immediately afterwards.  Do not expose
                // a visible card with no matching master/filtered placement in
                // the small interval between those two operations.
                if (trayViewTransactionDepth <= 0
                    && !placementModels.Any(model => model != null
                        && string.Equals(model.itemId, itemId,
                            StringComparison.Ordinal)))
                {
                    return;
                }
            }
            else
            {
                hiddenItemIds.Add(itemId);
                RemoveItemPlacement(itemId);
            }

            RefreshTrayViews(BuildVisibleItemIds());
        }

        public void SetRotateEnabled(string itemId, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !cardsByItemId.TryGetValue(itemId, out BuildItemPreviewCardView card)
                || card == null)
            {
                return;
            }

            card.SetRotateButtonInteractable(enabled);
        }

        public void ApplyItemArtworkRotationOffset(string itemId, float rotationOffsetDegrees)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !cardsByItemId.TryGetValue(itemId, out BuildItemPreviewCardView card)
                || card == null)
            {
                return;
            }

            card.ApplyArtworkImageRotationOffset(rotationOffsetDegrees);
        }

        public bool TryCaptureItemVisualStyles(string itemId, List<ShapeCellVisualStyle> styles)
        {
            if (styles == null)
            {
                return false;
            }

            styles.Clear();
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (cardsByItemId.TryGetValue(itemId, out BuildItemPreviewCardView card)
                && card != null
                && card.TryCaptureCellVisualStyles(styles))
            {
                return true;
            }

            return TryCaptureTraySlotVisualStyles(itemId, styles);
        }

        private bool TryCaptureTraySlotVisualStyles(string itemId, List<ShapeCellVisualStyle> styles)
        {
            if (styles == null || string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            styles.Clear();
            TrayPlacementViewModel placement = placementModels.FirstOrDefault(model =>
                model != null
                && model.isValid
                && string.Equals(model.itemId, itemId, StringComparison.Ordinal));
            if (placement == null)
            {
                return false;
            }

            foreach (int slotIndex in placement.occupiedSlotIndexes ?? Array.Empty<int>())
            {
                if (slotIndex < 0 || slotIndex >= traySlotImages.Count)
                {
                    continue;
                }

                Image image = traySlotImages[slotIndex];
                if (image == null || (image.sprite == null && image.overrideSprite == null))
                {
                    continue;
                }

                ShapeCellVisualStyle style = ShapeCellVisualStyle.FromImage(image);
                if (style != null)
                {
                    styles.Add(style);
                }
            }

            return styles.Count > 0;
        }

        internal bool TryCaptureTraySlotUnderlayStyle(
            string itemId,
            int visualIndex,
            out ShapeCellVisualStyle style)
        {
            style = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            TrayPlacementViewModel placement = placementModels.FirstOrDefault(model =>
                model != null
                && model.isValid
                && string.Equals(model.itemId, itemId, StringComparison.Ordinal));
            IReadOnlyList<int> occupiedSlots = placement?.occupiedSlotIndexes ?? Array.Empty<int>();
            if (occupiedSlots.Count == 0)
            {
                return false;
            }

            int safeIndex = Mathf.Clamp(visualIndex, 0, occupiedSlots.Count - 1);
            return reservationView.TryCaptureSlotUnderlayStyle(occupiedSlots[safeIndex], out style);
        }

        internal bool TryCaptureFirstTraySlotUnderlayStyle(out ShapeCellVisualStyle style)
        {
            for (int i = 0; i < GetTraySlotCount(); i++)
            {
                if (reservationView.TryCaptureSlotUnderlayStyle(i, out style))
                {
                    return true;
                }
            }

            style = null;
            return false;
        }

        private void RemoveItemPlacement(string itemId)
        {
            placementModels.RemoveAll(model =>
                model != null && string.Equals(model.itemId, itemId, StringComparison.Ordinal));
        }

        public bool TryScreenPointToTrayCell(
            Vector2 screenPoint,
            Camera eventCamera,
            out ItemShapeCell cell)
        {
            cell = default;
            EnsureRuntimeMatureScrollArea();
            if (!IsScreenPointInTrayViewport(screenPoint, eventCamera))
            {
                return false;
            }

            if (!itemLayoutView.TryScreenPointToSlotIndex(screenPoint, eventCamera, out int slotIndex))
            {
                return false;
            }

            cell = new ItemShapeCell(
                slotIndex % BuildGridInteractionPreviewController.TrayColumns,
                slotIndex / BuildGridInteractionPreviewController.TrayColumns);
            return true;
        }

        internal bool TryBuildCellVisualLayout(
            IReadOnlyList<ItemShapeCell> occupiedCells,
            out ShapeCellVisualLayout layout)
        {
            return itemLayoutView.TryBuildCellVisualLayout(occupiedCells, out layout);
        }

        public bool TryAutoScrollDuringDrag(
            Vector2 screenPoint,
            Camera eventCamera,
            float deltaTime)
        {
            EnsureRuntimeMatureScrollArea();
            RectTransform viewport = ResolveTrayViewport();
            if (scrollRect == null
                || contentRoot == null
                || viewport == null
                || !scrollRect.vertical
                || contentRoot.rect.height <= viewport.rect.height + 1f
                || !RectTransformUtility.RectangleContainsScreenPoint(viewport, screenPoint, eventCamera)
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    viewport,
                    screenPoint,
                    eventCamera,
                    out Vector2 localPoint))
            {
                return false;
            }

            Rect viewportRect = viewport.rect;
            float edgeSize = Mathf.Max(36f, viewportRect.height * 0.12f);
            float topDistance = viewportRect.yMax - localPoint.y;
            float bottomDistance = localPoint.y - viewportRect.yMin;
            float direction = 0f;
            if (topDistance <= edgeSize)
            {
                direction = 1f;
            }
            else if (bottomDistance <= edgeSize)
            {
                direction = -1f;
            }

            if (Mathf.Approximately(direction, 0f))
            {
                return false;
            }

            float scrollableHeight = Mathf.Max(1f, contentRoot.rect.height - viewportRect.height);
            float pixelsPerSecond = Mathf.Max(scrollRect.scrollSensitivity * 12f, viewportRect.height * 0.75f);
            float normalizedDelta = direction * pixelsPerSecond * Mathf.Max(0f, deltaTime) / scrollableHeight;
            float previous = scrollRect.verticalNormalizedPosition;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(previous + normalizedDelta);
            if (Mathf.Approximately(previous, scrollRect.verticalNormalizedPosition))
            {
                return false;
            }

            lastTrayAutoScrollTime = Time.unscaledTime;
            RequestTrayRowSnap();
            Canvas.ForceUpdateCanvases();
            return true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            trayScrollPointerDragging = true;
            RequestTrayRowSnap();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            lastTrayScrollMutationTime = Time.unscaledTime;
            RequestTrayRowSnap();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            trayScrollPointerDragging = false;
            lastTrayScrollMutationTime = Time.unscaledTime;
            RequestTrayRowSnap();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            lastTrayScrollMutationTime = Time.unscaledTime;
            RequestTrayRowSnap();
        }

        private void OnEnable()
        {
            BindProductionRowSnapObserver();
#if UNITY_EDITOR
            QueueEditModePreviewRefresh();
#endif
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying
                || scrollRect == null
                || contentRoot == null)
            {
                return;
            }

            float normalizedPosition = scrollRect.verticalNormalizedPosition;
            if (!applyingTrayRowSnap
                && (float.IsNaN(lastObservedVerticalNormalizedPosition)
                    || Mathf.Abs(normalizedPosition
                        - lastObservedVerticalNormalizedPosition) > 0.00001f))
            {
                trayRowSnapPending = true;
            }
            lastObservedVerticalNormalizedPosition = normalizedPosition;

            if (!trayRowSnapPending
                || trayScrollPointerDragging
                || controller?.IsTrayItemDragActive == true
                || Time.unscaledTime - lastTrayAutoScrollTime
                    < TrayRowSnapIdleSeconds
                || Time.unscaledTime - lastTrayScrollMutationTime
                    < TrayRowSnapIdleSeconds
                || Mathf.Abs(scrollRect.velocity.y)
                    > ResolveTrayRowSnapVelocityThreshold())
            {
                return;
            }

            SettleTrayScrollToNearestRow();
        }

        private float ResolveTrayRowSnapVelocityThreshold()
        {
            GridLayoutGroup grid = contentRoot == null
                ? null
                : contentRoot.GetComponent<GridLayoutGroup>();
            return grid == null
                ? 0f
                : Mathf.Max(0f, grid.cellSize.y + grid.spacing.y)
                    * TrayRowSnapVelocityPitchRatio;
        }

        private void OnDisable()
        {
            UnbindProductionRowSnapObserver();
            trayScrollPointerDragging = false;
            applyingTrayRowSnap = false;
        }

        private void OnDestroy()
        {
            UnbindProductionRowSnapObserver();
        }

        private void RequestTrayRowSnap()
        {
            trayRowSnapPending = true;
        }

        private void SettleTrayScrollToNearestRow()
        {
            if (!TryResolveTrayScrollMetrics(
                    out RectTransform viewport,
                    out float rowPitch,
                    out float currentOffset,
                    out float scrollableHeight))
            {
                trayRowSnapPending = false;
                return;
            }

            if (rowPitch <= TrayRowSnapTolerance
                || scrollableHeight <= TrayRowSnapTolerance)
            {
                scrollRect.velocity = Vector2.zero;
                scrollRect.verticalNormalizedPosition = 1f;
                trayRowSnapPending = false;
                lastObservedVerticalNormalizedPosition = 1f;
                return;
            }

            float rowOffset = ResolveNearestTrayRowOffset(
                currentOffset,
                rowPitch,
                scrollableHeight);

            float targetNormalizedPosition =
                1f - rowOffset / scrollableHeight;
            applyingTrayRowSnap = true;
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition =
                Mathf.Clamp01(targetNormalizedPosition);
            Canvas.ForceUpdateCanvases();
            Bounds settledBounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(
                    viewport,
                    contentRoot);
            float settledOffset = Mathf.Clamp(
                settledBounds.max.y - viewport.rect.yMax,
                0f,
                scrollableHeight);
            float correction = rowOffset - settledOffset;
            if (Mathf.Abs(correction) > TrayRowSnapTolerance)
            {
                Vector2 anchoredPosition = contentRoot.anchoredPosition;
                anchoredPosition.y += correction;
                contentRoot.anchoredPosition = anchoredPosition;
                Canvas.ForceUpdateCanvases();
            }
            applyingTrayRowSnap = false;
            trayRowSnapPending = false;
            lastObservedVerticalNormalizedPosition =
                scrollRect.verticalNormalizedPosition;
        }

        private static float ResolveNearestTrayRowOffset(
            float currentOffset,
            float rowPitch,
            float scrollableHeight)
        {
            float safeScrollableHeight = Mathf.Max(0f, scrollableHeight);
            if (rowPitch <= TrayRowSnapTolerance
                || safeScrollableHeight <= TrayRowSnapTolerance)
            {
                return 0f;
            }

            float safeCurrentOffset = Mathf.Clamp(
                currentOffset, 0f, safeScrollableHeight);
            float rowOffset = Mathf.Clamp(
                Mathf.Round(safeCurrentOffset / rowPitch) * rowPitch,
                0f,
                safeScrollableHeight);
            return Mathf.Abs(safeScrollableHeight - safeCurrentOffset)
                   < Mathf.Abs(rowOffset - safeCurrentOffset)
                ? safeScrollableHeight
                : rowOffset;
        }

        private bool TryResolveTrayScrollMetrics(
            out RectTransform viewport,
            out float rowPitch,
            out float currentOffset,
            out float scrollableHeight)
        {
            viewport = ResolveTrayViewport();
            GridLayoutGroup grid = contentRoot == null
                ? null
                : contentRoot.GetComponent<GridLayoutGroup>();
            rowPitch = grid == null ? 0f : grid.cellSize.y + grid.spacing.y;
            currentOffset = 0f;
            scrollableHeight = 0f;
            if (scrollRect == null || contentRoot == null || viewport == null
                || grid == null)
            {
                return false;
            }

            Canvas.ForceUpdateCanvases();
            Bounds contentBounds =
                RectTransformUtility.CalculateRelativeRectTransformBounds(
                    viewport,
                    contentRoot);
            scrollableHeight = Mathf.Max(
                0f,
                contentBounds.size.y - viewport.rect.height);
            currentOffset = Mathf.Clamp(
                contentBounds.max.y - viewport.rect.yMax,
                0f,
                scrollableHeight);
            return true;
        }

        private void BindProductionRowSnapObserver()
        {
            if (!Application.isPlaying || scrollRect == null)
            {
                return;
            }
            if (observedRowSnapScrollRect == scrollRect
                && observedRowSnapEventTrigger != null)
            {
                return;
            }

            UnbindProductionRowSnapObserver();
            observedRowSnapScrollRect = scrollRect;
            observedRowSnapScrollRect.onValueChanged.AddListener(
                HandleObservedScrollValueChanged);
            observedRowSnapEventTrigger =
                scrollRect.GetComponent<EventTrigger>();
            if (observedRowSnapEventTrigger == null)
            {
                observedRowSnapEventTrigger =
                    scrollRect.gameObject.AddComponent<EventTrigger>();
                ownsObservedRowSnapEventTrigger = true;
            }
            observedRowSnapEventTrigger.triggers ??=
                new List<EventTrigger.Entry>();
            observedBeginDragEntry = AddObservedRowSnapEntry(
                EventTriggerType.BeginDrag,
                HandleObservedBeginDrag);
            observedDragEntry = AddObservedRowSnapEntry(
                EventTriggerType.Drag,
                HandleObservedDrag);
            observedEndDragEntry = AddObservedRowSnapEntry(
                EventTriggerType.EndDrag,
                HandleObservedEndDrag);
            observedScrollEntry = AddObservedRowSnapEntry(
                EventTriggerType.Scroll,
                HandleObservedScroll);
        }

        private EventTrigger.Entry AddObservedRowSnapEntry(
            EventTriggerType eventId,
            UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            EventTrigger.Entry entry = new()
            {
                eventID = eventId
            };
            entry.callback.AddListener(callback);
            observedRowSnapEventTrigger.triggers.Add(entry);
            return entry;
        }

        private void UnbindProductionRowSnapObserver()
        {
            if (observedRowSnapScrollRect != null)
            {
                observedRowSnapScrollRect.onValueChanged.RemoveListener(
                    HandleObservedScrollValueChanged);
            }
            if (observedRowSnapEventTrigger != null
                && observedRowSnapEventTrigger.triggers != null)
            {
                observedRowSnapEventTrigger.triggers.Remove(
                    observedBeginDragEntry);
                observedRowSnapEventTrigger.triggers.Remove(
                    observedDragEntry);
                observedRowSnapEventTrigger.triggers.Remove(
                    observedEndDragEntry);
                observedRowSnapEventTrigger.triggers.Remove(
                    observedScrollEntry);
            }
            if (ownsObservedRowSnapEventTrigger
                && observedRowSnapEventTrigger != null
                && Application.isPlaying)
            {
                Destroy(observedRowSnapEventTrigger);
            }

            observedRowSnapScrollRect = null;
            observedRowSnapEventTrigger = null;
            observedBeginDragEntry = null;
            observedDragEntry = null;
            observedEndDragEntry = null;
            observedScrollEntry = null;
            ownsObservedRowSnapEventTrigger = false;
        }

        private void HandleObservedScrollValueChanged(Vector2 position)
        {
            if (!Application.isPlaying || applyingTrayRowSnap)
            {
                return;
            }
            trayRowSnapPending = true;
            lastObservedVerticalNormalizedPosition = position.y;
        }

        private void HandleObservedBeginDrag(BaseEventData eventData)
        {
            trayScrollPointerDragging = true;
            lastTrayScrollMutationTime = Time.unscaledTime;
            RequestTrayRowSnap();
        }

        private void HandleObservedDrag(BaseEventData eventData)
        {
            lastTrayScrollMutationTime = Time.unscaledTime;
            RequestTrayRowSnap();
        }

        private void HandleObservedEndDrag(BaseEventData eventData)
        {
            trayScrollPointerDragging = false;
            lastTrayScrollMutationTime = Time.unscaledTime;
            RequestTrayRowSnap();
        }

        private void HandleObservedScroll(BaseEventData eventData)
        {
            lastTrayScrollMutationTime = Time.unscaledTime;
            RequestTrayRowSnap();
        }

        private void BindViewAuthorities()
        {
            itemLayoutView.Bind(
                contentRoot,
                itemCardLayer,
                traySlotRects,
                BuildGridInteractionPreviewController.TrayColumns);
            reservationView.Bind(traySlotRects, traySlotImages, traySlotOutlines);
            BindProductionRowSnapObserver();
        }

        private void EnsureRuntimeMatureScrollArea()
        {
            if (!Application.isPlaying || scrollRect == null || contentRoot == null)
            {
                return;
            }

            RectTransform viewport = ResolveTrayViewport();
            if (viewport != null && scrollRect.viewport == null)
            {
                scrollRect.viewport = viewport;
            }

            if (scrollRect.content == null)
            {
                scrollRect.content = contentRoot;
            }
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.inertia = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            if (EnsureContentHeightFromGrid())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            }
        }

        private bool EnsureContentHeightFromGrid()
        {
            if (contentRoot == null)
            {
                return false;
            }

            GridLayoutGroup grid = contentRoot.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                return false;
            }

            int columnCount = ResolveGridColumnCount(grid);
            int slotCount = Mathf.Max(traySlotRects.Count, BuildGridInteractionPreviewController.TrayColumns * BuildGridInteractionPreviewController.TrayRows);
            int rowCount = Mathf.Max(1, Mathf.CeilToInt(slotCount / (float)Mathf.Max(1, columnCount)));
            float requiredHeight =
                grid.padding.top
                + grid.padding.bottom
                + rowCount * grid.cellSize.y
                + Mathf.Max(0, rowCount - 1) * grid.spacing.y;
            if (contentRoot.rect.height + 0.5f >= requiredHeight)
            {
                return false;
            }

            contentRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);
            return true;
        }

        private static int ResolveGridColumnCount(GridLayoutGroup grid)
        {
            if (grid == null)
            {
                return BuildGridInteractionPreviewController.TrayColumns;
            }

            return grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                ? Mathf.Max(1, grid.constraintCount)
                : BuildGridInteractionPreviewController.TrayColumns;
        }

        private RectTransform ResolveTrayViewport()
        {
            if (scrollRect != null && scrollRect.viewport != null)
            {
                return scrollRect.viewport;
            }

            return contentRoot == null ? null : contentRoot.parent as RectTransform;
        }

        private bool IsScreenPointInTrayViewport(Vector2 screenPoint, Camera eventCamera)
        {
            RectTransform viewport = ResolveTrayViewport();
            return viewport == null
                || RectTransformUtility.RectangleContainsScreenPoint(viewport, screenPoint, eventCamera);
        }

        private void InitializeCategoryButtons(
            BuildGridInteractionPreviewController controller,
            IReadOnlyList<string> categories)
        {
            buttonsByCategory.Clear();
            labelsByCategory.Clear();

            if (Application.isPlaying)
            {
                foreach (Button button in categoryButtons.Where(value => value != null))
                {
                    button.onClick.RemoveAllListeners();
                    button.interactable = false;
                    button.gameObject.SetActive(false);
                }
                foreach (Text label in categoryLabels.Where(value => value != null))
                {
                    label.gameObject.SetActive(false);
                }
                return;
            }

            int count = Math.Min(categoryButtons.Count, categoryLabels.Count);
            for (int i = 0; i < count; i++)
            {
                Button button = categoryButtons[i];
                Text label = categoryLabels[i];
                if (i >= categories.Count)
                {
                    if (button != null)
                    {
                        button.gameObject.SetActive(false);
                    }

                    if (label != null)
                    {
                        label.gameObject.SetActive(false);
                    }

                    continue;
                }

                if (button != null)
                {
                    button.gameObject.SetActive(true);
                }

                if (label != null)
                {
                    label.gameObject.SetActive(true);
                }

                string category = NormalizeCategory(categories[i], i);

                label.text = category;
                buttonsByCategory[category] = button;
                labelsByCategory[category] = label;

                button.onClick.RemoveAllListeners();
                if (controller != null && Application.isPlaying)
                {
                    string captured = category;
                    button.onClick.AddListener(() => controller.ApplyCategoryFilter(captured));
                }
            }
        }

        private static string NormalizeCategory(string category, int categoryIndex = -1)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return AllCategory;
            }

            string value = category.Trim();
            if (value == "閸忋劑鍎?")
            {
                return AllCategory;
            }

            string categoryId = BuildSandboxLegacyAndAdvancedItemRosterCatalog.NormalizeCategoryId(value);
            switch (categoryId)
            {
                case BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryAll:
                    return AllCategory;
                case BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryBasic:
                    return BasicCategory;
                case BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryAdvanced:
                    return AdvancedCategory;
                case BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryCore:
                    return CoreCategory;
                case BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategorySupport:
                    return SupportCategory;
                case BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryArtifact:
                    return ArtifactCategory;
                case BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryTestDevOnly:
                    return TestDevOnlyCategory;
                default:
                    return value;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            QueueEditModePreviewRefresh();
        }

        private void QueueEditModePreviewRefresh()
        {
            if (Application.isPlaying || editModePreviewRefreshQueued)
            {
                return;
            }

            editModePreviewRefreshQueued = true;
            EditorApplication.delayCall += RefreshEditModePreviewFromEditor;
        }

        private void RefreshEditModePreviewFromEditor()
        {
            editModePreviewRefreshQueued = false;
            if (this == null)
            {
                return;
            }

            RefreshEditModePreviewIfNeeded();
        }

        private void RefreshEditModePreviewIfNeeded()
        {
            DiscoverTrayReferencesIfNeeded();
            NormalizeItemCardsForEditPreview();
            BindViewAuthorities();

            if (Application.isPlaying
                || !isActiveAndEnabled
                || cards == null
                || cards.Count == 0
                || (HasVisiblePreviewCards() && placementModels.Count > 0))
            {
                return;
            }

            BuildGridInteractionPreviewController controller =
                FindObjectOfType<BuildGridInteractionPreviewController>(true);
            if (controller == null)
            {
                return;
            }

            Initialize(
                controller,
                BuildGridInteractionPreviewController.CreatePreviewItems(),
                BuildGridInteractionPreviewController.CategoryLabels);
        }

        private void DiscoverTrayReferencesIfNeeded()
        {
            if (contentRoot == null)
            {
                return;
            }

            if (itemCardLayer == null)
            {
                itemCardLayer = contentRoot.Find(ItemCardLayerName) as RectTransform;
            }

            int expectedSlotCount = BuildGridInteractionPreviewController.TrayColumns
                * BuildGridInteractionPreviewController.TrayRows;
            if (traySlotRects.Count >= expectedSlotCount
                && traySlotImages.Count >= expectedSlotCount
                && traySlotOutlines.Count >= expectedSlotCount)
            {
                return;
            }

            List<RectTransform> discoveredRects = new();
            List<Image> discoveredImages = new();
            List<Outline> discoveredOutlines = new();
            for (int i = 0; i < expectedSlotCount; i++)
            {
                RectTransform slot = FindTraySlot(i) as RectTransform;
                if (slot == null)
                {
                    continue;
                }

                discoveredRects.Add(slot);
                discoveredImages.Add(slot.GetComponent<Image>());
                discoveredOutlines.Add(slot.GetComponent<Outline>());
            }

            if (discoveredRects.Count > 0)
            {
                traySlotRects = discoveredRects;
                traySlotImages = discoveredImages;
                traySlotOutlines = discoveredOutlines;
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        private void NormalizeItemCardsForEditPreview()
        {
            if (contentRoot == null)
            {
                return;
            }

            RectTransform cardLayer = EnsureDetachedCardLayer();
            if (cardLayer == null)
            {
                return;
            }

            int itemCardCount = ResolvePreviewItemCardCount();
            List<BuildItemPreviewCardView> normalizedCards = new();
            List<BuildItemPreviewCardView> candidates = CollectExistingItemCardCandidates(cardLayer);
            bool changed = false;

            for (int i = 0; i < itemCardCount; i++)
            {
                BuildItemPreviewCardView card = i < candidates.Count ? candidates[i] : null;
                if (card == null)
                {
                    card = CreateDetachedItemCard(cardLayer, i);
                    changed = true;
                }

                RectTransform cardRect = card.GetComponent<RectTransform>();
                if (cardRect == null)
                {
                    continue;
                }

                if (cardRect.parent != cardLayer)
                {
                    cardRect.SetParent(cardLayer, worldPositionStays: false);
                    changed = true;
                }

                string cardName = $"ItemCard_{i + 1:00}";
                if (!string.Equals(cardRect.name, cardName, StringComparison.Ordinal))
                {
                    cardRect.name = cardName;
                    changed = true;
                }

                changed |= RemoveLegacyItemCardChildren(cardRect);
                EditorUtility.SetDirty(card);
                normalizedCards.Add(card);
            }

            changed |= RemoveExtraItemCards(cardLayer, normalizedCards);
            changed |= RemoveLegacySlotItemCards(normalizedCards);

            if (cards.Count != normalizedCards.Count || !cards.SequenceEqual(normalizedCards))
            {
                cards = normalizedCards;
                changed = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        private RectTransform EnsureDetachedCardLayer()
        {
            RectTransform cardLayer = itemCardLayer;
            if (cardLayer == null)
            {
                cardLayer = contentRoot.Find(ItemCardLayerName) as RectTransform;
            }

            bool changed = false;
            if (cardLayer == null)
            {
                GameObject layer = new(ItemCardLayerName, typeof(RectTransform), typeof(LayoutElement));
                layer.transform.SetParent(contentRoot, worldPositionStays: false);
                cardLayer = layer.GetComponent<RectTransform>();
                changed = true;
            }

            itemCardLayer = cardLayer;
            changed |= ConfigureItemCardLayer(cardLayer);

            if (changed)
            {
                EditorUtility.SetDirty(cardLayer);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            return cardLayer;
        }

        private int ResolvePreviewItemCardCount()
        {
            int previewItemCount = currentItems.Count > 0
                ? currentItems.Count
                : BuildGridInteractionPreviewController.CreatePreviewItems().Count;

            if (!Application.isPlaying && cards != null && cards.Count > 0)
            {
                return Math.Max(cards.Count, previewItemCount);
            }

            return previewItemCount;
        }

        private List<BuildItemPreviewCardView> CollectExistingItemCardCandidates(RectTransform cardLayer)
        {
            List<BuildItemPreviewCardView> candidates = cardLayer
                .GetComponentsInChildren<BuildItemPreviewCardView>(true)
                .Where(card => card != null)
                .OrderBy(card => card.transform.GetSiblingIndex())
                .ToList();

            if (cards != null)
            {
                foreach (BuildItemPreviewCardView card in cards)
                {
                    if (card != null && !candidates.Contains(card))
                    {
                        candidates.Add(card);
                    }
                }
            }

            int expectedSlotCount = BuildGridInteractionPreviewController.TrayColumns
                * BuildGridInteractionPreviewController.TrayRows;
            for (int i = 0; i < expectedSlotCount; i++)
            {
                Transform legacy = FindTraySlot(i)?.Find("ItemCard");
                BuildItemPreviewCardView card = legacy == null
                    ? null
                    : legacy.GetComponent<BuildItemPreviewCardView>();
                if (card != null && !candidates.Contains(card))
                {
                    candidates.Add(card);
                }
            }

            return candidates;
        }

        private BuildItemPreviewCardView CreateDetachedItemCard(RectTransform cardLayer, int index)
        {
            GameObject cardObject = new(
                $"ItemCard_{index + 1:00}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(BuildItemPreviewCardView));
            cardObject.transform.SetParent(cardLayer, worldPositionStays: false);

            RectTransform rect = cardObject.GetComponent<RectTransform>();
            Image image = cardObject.GetComponent<Image>();
            image.color = new Color(0.27f, 0.22f, 0.15f, 1f);
            image.raycastTarget = true;

            CanvasGroup group = cardObject.GetComponent<CanvasGroup>();
            group.blocksRaycasts = true;
            group.interactable = true;

            Text title = CreateCardText(rect, "Title", 11, TextAnchor.UpperCenter);
            Text category = CreateCardText(rect, "Category", 10, TextAnchor.MiddleCenter);
            Text shape = CreateCardText(rect, "Shape", 10, TextAnchor.UpperCenter);

            BuildItemPreviewCardView card = cardObject.GetComponent<BuildItemPreviewCardView>();
            card.Bind(rect, group, image, title, category, shape);
            return card;
        }

        private static Text CreateCardText(
            RectTransform parent,
            string name,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(parent, worldPositionStays: false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.90f, 0.87f, 0.76f, 1f);
            text.raycastTarget = false;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return text;
        }

        private bool RemoveExtraItemCards(
            RectTransform cardLayer,
            IReadOnlyCollection<BuildItemPreviewCardView> keepCards)
        {
            bool changed = false;
            foreach (BuildItemPreviewCardView card in cardLayer.GetComponentsInChildren<BuildItemPreviewCardView>(true))
            {
                if (card == null || keepCards.Contains(card))
                {
                    continue;
                }

                DestroyImmediate(card.gameObject);
                changed = true;
            }

            return changed;
        }

        private bool RemoveLegacySlotItemCards(IReadOnlyCollection<BuildItemPreviewCardView> keepCards)
        {
            bool changed = false;
            int expectedSlotCount = BuildGridInteractionPreviewController.TrayColumns
                * BuildGridInteractionPreviewController.TrayRows;
            for (int i = 0; i < expectedSlotCount; i++)
            {
                Transform legacy = FindTraySlot(i)?.Find("ItemCard");
                BuildItemPreviewCardView card = legacy == null
                    ? null
                    : legacy.GetComponent<BuildItemPreviewCardView>();
                if (card == null || keepCards.Contains(card))
                {
                    continue;
                }

                DestroyImmediate(card.gameObject);
                changed = true;
            }

            return changed;
        }

        private static bool RemoveLegacyItemCardChildren(RectTransform cardRect)
        {
            if (cardRect == null)
            {
                return false;
            }

            bool changed = false;
            changed |= RemoveLegacyChild(cardRect, "ShapeFootprintRoot");
            changed |= RemoveLegacyChild(cardRect, "TrayRotateButton");
            return changed;
        }

        private static bool RemoveLegacyChild(RectTransform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return false;
            }

            DestroyImmediate(child.gameObject);
            return true;
        }

        private bool HasVisiblePreviewCards()
        {
            foreach (BuildItemPreviewCardView card in cards)
            {
                if (card != null && card.gameObject.activeSelf && card.HasPreviewContent)
                {
                    return true;
                }
            }

            return false;
        }
#endif

        private Transform FindTraySlot(int index)
        {
            return contentRoot == null
                ? null
                : contentRoot.Find($"TrayGridSlot_{index + 1:00}");
        }

        private RectTransform ResolveItemCardLayer()
        {
            if (itemCardLayer != null)
            {
                return itemCardLayer;
            }

            if (contentRoot == null)
            {
                return null;
            }

            itemCardLayer = contentRoot.Find(ItemCardLayerName) as RectTransform;
            if (itemCardLayer != null)
            {
                ConfigureItemCardLayer(itemCardLayer);
                return itemCardLayer;
            }

            GameObject layer = new(ItemCardLayerName, typeof(RectTransform), typeof(LayoutElement));
            layer.transform.SetParent(contentRoot, worldPositionStays: false);
            itemCardLayer = layer.GetComponent<RectTransform>();
            ConfigureItemCardLayer(itemCardLayer);
            return itemCardLayer;
        }

        private static bool ConfigureItemCardLayer(RectTransform cardLayer)
        {
            if (cardLayer == null)
            {
                return false;
            }

            bool changed = false;
            if (cardLayer.anchorMin != Vector2.zero)
            {
                cardLayer.anchorMin = Vector2.zero;
                changed = true;
            }

            if (cardLayer.anchorMax != Vector2.one)
            {
                cardLayer.anchorMax = Vector2.one;
                changed = true;
            }

            if (cardLayer.offsetMin != Vector2.zero)
            {
                cardLayer.offsetMin = Vector2.zero;
                changed = true;
            }

            if (cardLayer.offsetMax != Vector2.zero)
            {
                cardLayer.offsetMax = Vector2.zero;
                changed = true;
            }

            if (cardLayer.localScale != Vector3.one)
            {
                cardLayer.localScale = Vector3.one;
                changed = true;
            }

            LayoutElement layoutElement = cardLayer.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = cardLayer.gameObject.AddComponent<LayoutElement>();
                changed = true;
            }

            if (!layoutElement.ignoreLayout)
            {
                layoutElement.ignoreLayout = true;
                changed = true;
            }

            return changed;
        }

        private void EnsureRuntimeCardCapacity(int requiredCount)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // A serialized card reference can become invalid after user-authored
            // hierarchy work.  Authority installation is a 31-row contract, so
            // capacity must be measured from usable card views rather than the
            // raw list length.
            cards.RemoveAll(card => card == null);
            if (requiredCount <= cards.Count)
            {
                return;
            }

            RectTransform cardLayer = ResolveItemCardLayer();
            if (cardLayer == null)
            {
                return;
            }

            for (int i = cards.Count; i < requiredCount; i++)
            {
                BuildItemPreviewCardView card = CreateRuntimeItemCard(cardLayer, i);
                if (card != null)
                {
                    cards.Add(card);
                }
            }

            BindViewAuthorities();
        }

        private static BuildItemPreviewCardView CreateRuntimeItemCard(RectTransform cardLayer, int index)
        {
            GameObject cardObject = new(
                $"ItemCard_Runtime_{index + 1:00}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(BuildItemPreviewCardView));
            cardObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            cardObject.transform.SetParent(cardLayer, worldPositionStays: false);

            RectTransform rect = cardObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;

            Image image = cardObject.GetComponent<Image>();
            image.color = new Color(0.27f, 0.22f, 0.15f, 1f);
            image.raycastTarget = true;

            CanvasGroup group = cardObject.GetComponent<CanvasGroup>();
            group.blocksRaycasts = true;
            group.interactable = true;

            Text title = CreateRuntimeCardText(rect, "Title", 11, TextAnchor.UpperCenter);
            Text category = CreateRuntimeCardText(rect, "Category", 10, TextAnchor.MiddleCenter);
            Text shape = CreateRuntimeCardText(rect, "Shape", 10, TextAnchor.UpperCenter);

            BuildItemPreviewCardView card = cardObject.GetComponent<BuildItemPreviewCardView>();
            card.Bind(rect, group, image, title, category, shape);
            return card;
        }

        private static Text CreateRuntimeCardText(
            RectTransform parent,
            string name,
            int fontSize,
            TextAnchor alignment)
        {
            GameObject textObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            textObject.transform.SetParent(parent, worldPositionStays: false);

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.90f, 0.87f, 0.76f, 1f);
            text.raycastTarget = false;

            RectTransform rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return text;
        }

        private void InitializeCards(BuildGridInteractionPreviewController controller)
        {
            cardsByItemId.Clear();
            placementModels.Clear();
            EnsureRuntimeCardCapacity(currentItems.Count);
            foreach (BuildItemPreviewCardView card in cards)
            {
                card?.Clear();
            }

            int itemCardIndex = 0;
            for (int i = 0; i < currentItems.Count; i++)
            {
                BuildGridInteractionPreviewController.PreviewItem item = currentItems[i];
                if (item == null)
                {
                    continue;
                }

                if (itemCardIndex >= cards.Count)
                {
                    continue;
                }

                if (controller == null)
                {
                    continue;
                }

                if (!controller.TryGetTrayPlacement(
                        item.ItemId,
                        out ShapeAwareItemTrayGridPlacement placement)
                    || placement == null)
                {
                    continue;
                }
                TrayPlacementViewModel placementModel =
                    TrayPlacementViewModel.FromPlacement(
                        placement,
                        true,
                        BuildGridInteractionPreviewController.TrayColumns);

                BuildItemPreviewCardView card = cards[itemCardIndex++];
                if (card == null)
                {
                    continue;
                }

                card.BindItemDisplayData(
                    controller,
                    item.ItemId,
                    item.DisplayName,
                    item.Category,
                    item.ShapeDisplayName,
                    item.CardColor);
                cardsByItemId[item.ItemId] = card;
                placementModels.Add(placementModel);
                card.SetVisible(true);
            }
        }

        private HashSet<string> BuildVisibleItemIds()
        {
            return new HashSet<string>(
                currentItems
                    .Where(item => !hiddenItemIds.Contains(item.ItemId))
                    .Select(item => item.ItemId),
                StringComparer.Ordinal);
        }

        private void RefreshTrayViews(HashSet<string> visibleIds)
        {
            if (trayViewTransactionDepth > 0)
            {
                trayViewPublishPending = true;
                return;
            }
            PublishTrayViews(visibleIds ?? BuildVisibleItemIds());
        }

        private void PublishTrayViews(HashSet<string> visibleIds)
        {
            HashSet<string> safeVisibleIds = visibleIds ?? new HashSet<string>(StringComparer.Ordinal);
            IReadOnlyList<TrayPlacementViewModel> viewPlacements = BuildViewPlacements(safeVisibleIds);
            foreach (BuildItemPreviewCardView card in cards)
            {
                if (card == null || string.IsNullOrWhiteSpace(card.ItemId))
                {
                    continue;
                }
                card.SetVisible(safeVisibleIds.Contains(card.ItemId));
            }
            displayedPlacementsByItemId.Clear();
            foreach (TrayPlacementViewModel placement in viewPlacements.Where(value => value != null
                         && !string.IsNullOrWhiteSpace(value.itemId)))
            {
                displayedPlacementsByItemId[placement.itemId] = placement;
            }
            itemLayoutView.Refresh(
                viewPlacements,
                cardsByItemId,
                safeVisibleIds);
            reservationView.Refresh(viewPlacements, safeVisibleIds);
            RefreshCategoryVisuals();
            trayViewPublicationRevision++;
        }

        private IReadOnlyList<TrayPlacementViewModel> BuildViewPlacements(ISet<string> visibleIds)
        {
            return placementModels;
        }


        private int GetTraySlotCount()
        {
            int authoritySlotCount = reservationView.SlotCount;
            if (authoritySlotCount > 0)
            {
                return authoritySlotCount;
            }

            int serializedSlotCount = Math.Max(
                traySlotRects?.Count ?? 0,
                Math.Max(traySlotImages?.Count ?? 0, traySlotOutlines?.Count ?? 0));
            if (serializedSlotCount > 0)
            {
                return serializedSlotCount;
            }

            return BuildGridInteractionPreviewController.TrayColumns
                * BuildGridInteractionPreviewController.TrayRows;
        }

        private void RefreshCategoryVisuals()
        {
            string normalizedActiveCategory = NormalizeCategory(activeCategory);
            foreach (KeyValuePair<string, Text> pair in labelsByCategory)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                pair.Value.color = NormalizeCategory(pair.Key) == normalizedActiveCategory
                    ? new Color(1f, 0.86f, 0.42f, 1f)
                    : new Color(0.86f, 0.84f, 0.74f, 1f);
            }

            foreach (KeyValuePair<string, Button> pair in buttonsByCategory)
            {
                Image image = pair.Value == null ? null : pair.Value.GetComponent<Image>();
                if (image == null)
                {
                    continue;
                }

                image.color = NormalizeCategory(pair.Key) == normalizedActiveCategory
                    ? new Color(0.42f, 0.32f, 0.16f, 1f)
                    : new Color(0.18f, 0.18f, 0.14f, 1f);
            }
        }
    }
}
