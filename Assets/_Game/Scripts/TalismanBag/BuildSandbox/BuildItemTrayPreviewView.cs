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
        private string activeCategory = "鍏ㄩ儴";

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
            currentItems = (items ?? Array.Empty<BuildGridInteractionPreviewController.PreviewItem>())
                .Where(item => item != null)
                .ToList();
            hiddenItemIds.Clear();

            InitializeCategoryButtons(controller, categories ?? Array.Empty<string>());
            BindViewAuthorities();
            InitializeCards(controller);
            ApplyFilter("鍏ㄩ儴");
        }

        public void ApplyFilter(string category)
        {
            activeCategory = string.IsNullOrWhiteSpace(category) ? "鍏ㄩ儴" : category;
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
            if (!itemLayoutView.TryScreenPointToSlotIndex(screenPoint, eventCamera, out int slotIndex))
            {
                return false;
            }

            cell = new ItemShapeCell(
                slotIndex % BuildGridInteractionPreviewController.TrayColumns,
                slotIndex / BuildGridInteractionPreviewController.TrayColumns);
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
                string category = i < categories.Count ? categories[i] : label.text;
                if (string.IsNullOrWhiteSpace(category))
                {
                    category = "鍏ㄩ儴";
                }

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
                itemCardLayer = contentRoot.Find("ItemCardLayer") as RectTransform;
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
            const string cardLayerName = "ItemCardLayer";
            RectTransform cardLayer = itemCardLayer;
            if (cardLayer == null)
            {
                cardLayer = contentRoot.Find(cardLayerName) as RectTransform;
            }

            bool changed = false;
            if (cardLayer == null)
            {
                GameObject layer = new(cardLayerName, typeof(RectTransform), typeof(LayoutElement));
                layer.transform.SetParent(contentRoot, worldPositionStays: false);
                cardLayer = layer.GetComponent<RectTransform>();
                changed = true;
            }

            itemCardLayer = cardLayer;
            cardLayer.anchorMin = Vector2.zero;
            cardLayer.anchorMax = Vector2.one;
            cardLayer.offsetMin = Vector2.zero;
            cardLayer.offsetMax = Vector2.zero;
            cardLayer.localScale = Vector3.one;
            cardLayer.SetAsLastSibling();

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

            if (changed)
            {
                EditorUtility.SetDirty(cardLayer);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            return cardLayer;
        }

        private int ResolvePreviewItemCardCount()
        {
            if (currentItems.Count > 0)
            {
                return currentItems.Count;
            }

            return BuildGridInteractionPreviewController.CreatePreviewItems().Count;
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
                typeof(Canvas),
                typeof(GraphicRaycaster),
                typeof(BuildItemPreviewCardView));
            cardObject.transform.SetParent(cardLayer, worldPositionStays: false);

            RectTransform rect = cardObject.GetComponent<RectTransform>();
            Image image = cardObject.GetComponent<Image>();
            image.color = new Color(0.27f, 0.22f, 0.15f, 1f);
            image.raycastTarget = true;

            CanvasGroup group = cardObject.GetComponent<CanvasGroup>();
            group.blocksRaycasts = true;
            group.interactable = true;

            Canvas canvas = cardObject.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 30;

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
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
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

        private void InitializeCards(BuildGridInteractionPreviewController controller)
        {
            cardsByItemId.Clear();
            placementModels.Clear();
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
            return new HashSet<string>(
                currentItems
                    .Where(item => activeCategory == "鍏ㄩ儴" || item.Category == activeCategory)
                    .Where(item => !hiddenItemIds.Contains(item.ItemId))
                    .Select(item => item.ItemId),
                StringComparer.Ordinal);
        }

        private void RefreshTrayViews(HashSet<string> visibleIds)
        {
            HashSet<string> safeVisibleIds = visibleIds ?? new HashSet<string>(StringComparer.Ordinal);
            itemLayoutView.Refresh(placementModels, cardsByItemId, safeVisibleIds);
            reservationView.Refresh(placementModels, safeVisibleIds);
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
            foreach (KeyValuePair<string, Text> pair in labelsByCategory)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                pair.Value.color = pair.Key == activeCategory
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

                image.color = pair.Key == activeCategory
                    ? new Color(0.42f, 0.32f, 0.16f, 1f)
                    : new Color(0.18f, 0.18f, 0.14f, 1f);
            }
        }
    }
}
