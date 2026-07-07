using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TalismanBag.BuildSandbox
{
    [ExecuteAlways]
    public sealed class BuildItemTrayPreviewView : MonoBehaviour
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
        private const float TrayScrollSensitivity = 18f;
        private const float TrayScrollDecelerationRate = 0.16f;

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
        private readonly List<TrayPlacementViewModel> placementModels = new();
        private readonly HashSet<string> hiddenItemIds = new(StringComparer.Ordinal);
        private readonly TrayItemLayoutView itemLayoutView = new();
        private readonly TrayGridReservationView reservationView = new();
        private List<BuildGridInteractionPreviewController.PreviewItem> currentItems = new();
        private BuildGridInteractionPreviewController controller;
        private string activeCategory = AllCategory;

#if UNITY_EDITOR
        private bool editModePreviewRefreshQueued;
#endif

        public int TraySlotCount => GetTraySlotCount();
        public int CategoryCount => categoryLabels.Count(label => label != null);
        public string ActiveCategory => activeCategory;

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
            ConfigureTrayScrollMomentum(scrollRect);
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
                .ToList();
            hiddenItemIds.Clear();

            InitializeCategoryButtons(controller, categories ?? Array.Empty<string>());
            BindViewAuthorities();
            InitializeCards(controller);
            EnsureRuntimeMatureScrollArea();
            ApplyFilter(AllCategory);
        }

        public void ApplyFilter(string category)
        {
            string previousCategory = NormalizeCategory(activeCategory);
            string nextCategory = NormalizeCategory(category);
            activeCategory = nextCategory;
            if (previousCategory != AllCategory && nextCategory == AllCategory)
            {
                controller?.CompactTrayWhenReturningToAllCategory();
            }

            HashSet<string> visibleIds = BuildVisibleItemIds();

            foreach (BuildItemPreviewCardView card in cards)
            {
                if (card == null || string.IsNullOrWhiteSpace(card.ItemId))
                {
                    continue;
                }

                card.SetVisible(visibleIds.Contains(card.ItemId));
            }

            RefreshTrayViews(visibleIds);

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }

            RefreshCategoryVisuals();
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
            }
            else
            {
                hiddenItemIds.Add(itemId);
                RemoveItemPlacement(itemId);
            }

            if (cardsByItemId.TryGetValue(itemId, out BuildItemPreviewCardView card)
                && card != null)
            {
                card.SetVisible(inTray && BuildVisibleItemIds().Contains(itemId));
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

            Canvas.ForceUpdateCanvases();
            return true;
        }

        private void BindViewAuthorities()
        {
            itemLayoutView.Bind(
                contentRoot,
                itemCardLayer,
                traySlotRects,
                BuildGridInteractionPreviewController.TrayColumns);
            reservationView.Bind(traySlotImages, traySlotOutlines);
        }

        private void EnsureRuntimeMatureScrollArea()
        {
            if (!Application.isPlaying || scrollRect == null || contentRoot == null)
            {
                return;
            }

            ConfigureTrayScrollMomentum(scrollRect);
            RectTransform viewport = ResolveTrayViewport();
            if (viewport != null && scrollRect.viewport == null)
            {
                scrollRect.viewport = viewport;
            }

            if (scrollRect.content == null)
            {
                scrollRect.content = contentRoot;
            }

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

        private static void ConfigureTrayScrollMomentum(ScrollRect target)
        {
            if (target == null)
            {
                return;
            }

            target.horizontal = false;
            target.vertical = true;
            target.inertia = true;
            target.decelerationRate = TrayScrollDecelerationRate;
            target.movementType = ScrollRect.MovementType.Clamped;
            target.scrollSensitivity = TrayScrollSensitivity;
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
        private void OnEnable()
        {
            QueueEditModePreviewRefresh();
        }

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
            if (!Application.isPlaying || requiredCount <= cards.Count)
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

                if (controller == null
                    || !controller.TryGetTrayPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement placement)
                    || placement == null)
                {
                    continue;
                }

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
                placementModels.Add(TrayPlacementViewModel.FromPlacement(
                    placement,
                    true,
                    BuildGridInteractionPreviewController.TrayColumns));
                card.SetVisible(true);
            }
        }

        private HashSet<string> BuildVisibleItemIds()
        {
            string normalizedActiveCategory = NormalizeCategory(activeCategory);
            return new HashSet<string>(
                currentItems
                    .Where(item => normalizedActiveCategory == AllCategory
                        || item.MatchesCategory(normalizedActiveCategory))
                    .Where(item => !hiddenItemIds.Contains(item.ItemId))
                    .Select(item => item.ItemId),
                StringComparer.Ordinal);
        }

        private void RefreshTrayViews(HashSet<string> visibleIds)
        {
            HashSet<string> safeVisibleIds = visibleIds ?? new HashSet<string>(StringComparer.Ordinal);
            IReadOnlyList<TrayPlacementViewModel> viewPlacements = BuildViewPlacements(safeVisibleIds);
            itemLayoutView.Refresh(viewPlacements, cardsByItemId, safeVisibleIds);
            reservationView.Refresh(viewPlacements, safeVisibleIds);
        }

        private IReadOnlyList<TrayPlacementViewModel> BuildViewPlacements(ISet<string> visibleIds)
        {
            if (visibleIds == null
                || visibleIds.Count == 0
                || NormalizeCategory(activeCategory) == AllCategory)
            {
                return placementModels;
            }

            return BuildCompactedFilterPlacements(visibleIds);
        }

        private List<TrayPlacementViewModel> BuildCompactedFilterPlacements(ISet<string> visibleIds)
        {
            Dictionary<string, TrayPlacementViewModel> sourcePlacements = new(StringComparer.Ordinal);
            foreach (TrayPlacementViewModel placement in placementModels)
            {
                if (placement == null || string.IsNullOrWhiteSpace(placement.itemId))
                {
                    continue;
                }

                sourcePlacements[placement.itemId] = placement;
            }

            int traySlotCount = GetTraySlotCount();
            HashSet<int> occupiedSlots = new();
            List<TrayPlacementViewModel> compactedPlacements = new();
            foreach (BuildGridInteractionPreviewController.PreviewItem item in currentItems)
            {
                if (item == null
                    || !visibleIds.Contains(item.ItemId)
                    || !sourcePlacements.TryGetValue(item.ItemId, out TrayPlacementViewModel sourcePlacement)
                    || sourcePlacement == null
                    || !sourcePlacement.isValid)
                {
                    continue;
                }

                List<ItemShapeCell> normalizedOffsets = BuildNormalizedOffsets(sourcePlacement);
                if (!TryFindCompactPlacement(
                    normalizedOffsets,
                    occupiedSlots,
                    traySlotCount,
                    out int anchorSlotIndex,
                    out List<int> itemOccupiedSlots))
                {
                    compactedPlacements.Add(sourcePlacement);
                    foreach (int slotIndex in sourcePlacement.occupiedSlotIndexes ?? Array.Empty<int>())
                    {
                        if (slotIndex >= 0)
                        {
                            occupiedSlots.Add(slotIndex);
                        }
                    }

                    continue;
                }

                foreach (int slotIndex in itemOccupiedSlots)
                {
                    occupiedSlots.Add(slotIndex);
                }

                compactedPlacements.Add(new TrayPlacementViewModel
                {
                    itemId = sourcePlacement.itemId,
                    anchorSlotIndex = anchorSlotIndex,
                    occupiedSlotIndexes = itemOccupiedSlots,
                    rotation = sourcePlacement.rotation,
                    isValid = sourcePlacement.isValid
                });
            }

            return compactedPlacements;
        }

        private static List<ItemShapeCell> BuildNormalizedOffsets(TrayPlacementViewModel placement)
        {
            IReadOnlyList<int> occupiedSlotIndexes = placement.occupiedSlotIndexes ?? Array.Empty<int>();
            int anchorSlotIndex = placement.anchorSlotIndex >= 0
                ? placement.anchorSlotIndex
                : occupiedSlotIndexes.FirstOrDefault();
            int anchorColumn = anchorSlotIndex % BuildGridInteractionPreviewController.TrayColumns;
            int anchorRow = anchorSlotIndex / BuildGridInteractionPreviewController.TrayColumns;
            IEnumerable<int> sourceSlots = occupiedSlotIndexes.Count > 0
                ? occupiedSlotIndexes.Distinct()
                : new[] { Mathf.Max(0, anchorSlotIndex) };

            List<ItemShapeCell> offsets = new();
            foreach (int slotIndex in sourceSlots)
            {
                if (slotIndex < 0)
                {
                    continue;
                }

                offsets.Add(new ItemShapeCell(
                    slotIndex % BuildGridInteractionPreviewController.TrayColumns - anchorColumn,
                    slotIndex / BuildGridInteractionPreviewController.TrayColumns - anchorRow));
            }

            if (offsets.Count == 0)
            {
                offsets.Add(new ItemShapeCell(0, 0));
            }

            int minX = offsets.Min(cell => cell.x);
            int minY = offsets.Min(cell => cell.y);
            return offsets
                .Select(cell => new ItemShapeCell(cell.x - minX, cell.y - minY))
                .Distinct()
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToList();
        }

        private static bool TryFindCompactPlacement(
            IReadOnlyList<ItemShapeCell> offsets,
            ISet<int> occupiedSlots,
            int traySlotCount,
            out int anchorSlotIndex,
            out List<int> itemOccupiedSlots)
        {
            itemOccupiedSlots = new List<int>();
            for (int slotIndex = 0; slotIndex < traySlotCount; slotIndex++)
            {
                int anchorColumn = slotIndex % BuildGridInteractionPreviewController.TrayColumns;
                int anchorRow = slotIndex / BuildGridInteractionPreviewController.TrayColumns;
                List<int> candidateSlots = new();
                bool isValid = true;

                foreach (ItemShapeCell offset in offsets ?? Array.Empty<ItemShapeCell>())
                {
                    int column = anchorColumn + offset.x;
                    int row = anchorRow + offset.y;
                    int candidateSlotIndex = row * BuildGridInteractionPreviewController.TrayColumns + column;
                    if (column < 0
                        || column >= BuildGridInteractionPreviewController.TrayColumns
                        || row < 0
                        || candidateSlotIndex < 0
                        || candidateSlotIndex >= traySlotCount
                        || occupiedSlots.Contains(candidateSlotIndex))
                    {
                        isValid = false;
                        break;
                    }

                    candidateSlots.Add(candidateSlotIndex);
                }

                if (!isValid)
                {
                    continue;
                }

                anchorSlotIndex = slotIndex;
                itemOccupiedSlots = candidateSlots;
                return true;
            }

            anchorSlotIndex = -1;
            return false;
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
