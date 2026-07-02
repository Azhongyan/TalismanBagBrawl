using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class BuildGridInteractionPreviewController : MonoBehaviour
    {
        public const string PackageName = "V0.4-BuildGridInteractionPreview01";
        public const string VerticalSlicePackageName = "V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01";
        public const int BoardColumns = 5;
        public const int BoardRows = 5;
        public const int TrayColumns = 5;
        public const int TrayRows = 8;
        public const int TrayVisibleRows = 5;

        public static readonly string[] CategoryLabels =
        {
            "全部",
            "符箓",
            "法器",
            "材料",
            "消耗",
            "特殊"
        };

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool readsFormalSaveData;
        [SerializeField] private bool writesFormalFlow;
        [SerializeField] private bool writesFormalUi;
        [SerializeField] private bool touchesFormalScene;
        [SerializeField] private bool showsCompleteAnswers;

        [SerializeField] private RectTransform boardGridPreview;
        [SerializeField] private BuildGridPreviewSlotView[] boardSlots = Array.Empty<BuildGridPreviewSlotView>();
        [SerializeField] private BuildItemTrayPreviewView itemTrayView;
        [SerializeField] private BuildPlacementFeedbackView placementFeedbackView;
        [SerializeField] private Text selectedItemInfoTitle;
        [SerializeField] private Text selectedItemInfoBody;
        [SerializeField] private RectTransform selectedItemInfoRoot;
        [SerializeField] private Button selectedItemInfoCloseButton;
        [SerializeField] private BuildSandboxItemInfoPanel itemInfoPanel;
        [SerializeField] private Button resetPreviewButton;
        [SerializeField] private Button rotatePreviewButton;
        [SerializeField] private RectTransform dragGhostRoot;
        [SerializeField] private Text dragGhostText;

        private readonly Dictionary<ItemShapeCell, BuildGridPreviewSlotView> boardSlotByCell = new();
        private readonly Dictionary<string, PreviewItem> itemById = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ItemShapeConfig> shapeById = new(StringComparer.Ordinal);
        private readonly List<ItemShapeConfig> runtimeShapeConfigs = new();
        private ShapePlacementSession placementSession;
        private MobileShapePlacementInputExtension mobileInput;
        private ShapeAwareItemTrayGrid shapeAwareTrayGrid;
        private UiBoardShapeGridReceiver boardReceiver;
        private PreviewItem selectedItem;
        private PreviewItem lockedPreviewItem;
        private ShapePlacementResult lastPreviewResult;
        private ShapePlacementResult lockedPreviewResult;
        private ItemShapeCell lastPreviewAnchor;
        private bool hasLastPreviewAnchor;
        private bool hasLockedPreview;
        private readonly HashSet<string> placedItemIds = new(StringComparer.Ordinal);
        private string activeDragItemId = string.Empty;
        private int placementSequence;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool ReadsFormalSaveData => readsFormalSaveData;
        public bool WritesFormalFlow => writesFormalFlow;
        public bool WritesFormalUi => writesFormalUi;
        public bool TouchesFormalScene => touchesFormalScene;
        public bool ShowsCompleteAnswers => showsCompleteAnswers;
        public int BoardSlotCount => boardSlots?.Length ?? 0;
        public int TraySlotCount => itemTrayView == null ? 0 : itemTrayView.TraySlotCount;
        public int CategoryCount => itemTrayView == null ? 0 : itemTrayView.CategoryCount;
        public int ShapeCount => shapeById.Count == 0 ? CreatePreviewShapeConfigs().Count : shapeById.Count;
        public int PreviewItemCount => itemById.Count == 0 ? CreatePreviewItems().Count : itemById.Count;
        public int PlacedItemCount => placedItemIds.Count;
        public bool UsesShapePlacementSession => placementSession != null;
        public bool UsesShapeAwareItemTrayGrid => shapeAwareTrayGrid != null;
        public bool UsesMobileShapePlacementInputExtension => mobileInput != null;

        public void Bind(
            RectTransform boardRoot,
            IReadOnlyList<BuildGridPreviewSlotView> slots,
            BuildItemTrayPreviewView trayView,
            BuildPlacementFeedbackView feedbackView,
            Text itemInfoTitle,
            Text itemInfoBody,
            Button resetButton,
            Button rotateButton,
            RectTransform ghostRoot,
            Text ghostText)
        {
            boardGridPreview = boardRoot;
            boardSlots = (slots ?? Array.Empty<BuildGridPreviewSlotView>()).Where(slot => slot != null).ToArray();
            itemTrayView = trayView;
            placementFeedbackView = feedbackView;
            selectedItemInfoTitle = itemInfoTitle;
            selectedItemInfoBody = itemInfoBody;
            resetPreviewButton = resetButton;
            rotatePreviewButton = rotateButton;
            dragGhostRoot = ghostRoot;
            dragGhostText = ghostText;
        }

        public static List<ItemShapeConfig> CreatePreviewShapeConfigs()
        {
            return new List<ItemShapeConfig>
            {
                CreateShape("Vertical2", "竖二格", true, new ItemShapeCell(0, 0), new ItemShapeCell(0, 1)),
                CreateShape("Single1", "单格", false, new ItemShapeCell(0, 0)),
                CreateShape("Corner3", "拐角三格", true, new ItemShapeCell(0, 0), new ItemShapeCell(1, 0), new ItemShapeCell(0, 1)),
                CreateShape("Square4", "方四格", false, new ItemShapeCell(0, 0), new ItemShapeCell(1, 0), new ItemShapeCell(0, 1), new ItemShapeCell(1, 1))
            };
        }

        public static List<PreviewItem> CreatePreviewItems()
        {
            return new List<PreviewItem>
            {
                new("preview_x2_wood_talisman", "护阵木牌", "符箓", "Vertical2", "竖二格", new Color(0.24f, 0.38f, 0.22f, 1f)),
                new("preview_fire_talisman", "炽火符", "符箓", "Single1", "单格", new Color(0.55f, 0.20f, 0.14f, 1f)),
                new("preview_guard_wood", "守护木牌", "法器", "Vertical2", "竖二格", new Color(0.22f, 0.34f, 0.40f, 1f)),
                new("preview_cleanse_corner", "净化折符", "符箓", "Corner3", "拐角三格", new Color(0.38f, 0.25f, 0.52f, 1f)),
                new("preview_stone_core", "炉芯石", "材料", "Square4", "方四格", new Color(0.36f, 0.32f, 0.25f, 1f)),
                new("preview_energy_incense", "聚能香", "消耗", "Vertical2", "竖二格", new Color(0.47f, 0.35f, 0.12f, 1f)),
                new("preview_old_bell", "镇邪铃", "特殊", "Corner3", "拐角三格", new Color(0.28f, 0.42f, 0.38f, 1f)),
                new("preview_thunder_sword", "雷引剑符", "符箓", "Single1", "单格", new Color(0.30f, 0.31f, 0.58f, 1f)),
                new("preview_soul_seal", "镇魂法印", "法器", "Square4", "方四格", new Color(0.42f, 0.24f, 0.30f, 1f))
            };
        }

        public bool TryGetPreviewShapeFootprint(
            string shapeId,
            out int cellCount,
            out bool verticalFootprint)
        {
            return TryGetPreviewShapeFootprint(
                shapeId,
                ItemShapeRotation.Rotation0,
                out cellCount,
                out verticalFootprint);
        }

        public bool TryGetPreviewShapeFootprint(
            string shapeId,
            ItemShapeRotation rotation,
            out int cellCount,
            out bool verticalFootprint)
        {
            EnsureShapeLookupForQuery();
            if (!shapeById.TryGetValue(shapeId ?? string.Empty, out ItemShapeConfig shapeConfig)
                || shapeConfig == null)
            {
                cellCount = 1;
                verticalFootprint = false;
                return false;
            }

            cellCount = Mathf.Max(1, shapeConfig.cellCount);
            verticalFootprint = IsFootprintVertical(shapeConfig, rotation);
            return true;
        }

        public bool TryGetTrayPlacement(
            string itemId,
            out ShapeAwareItemTrayGridPlacement placement)
        {
            EnsureTrayPlacementLookupForQuery();
            placement = null;
            return shapeAwareTrayGrid != null
                && shapeAwareTrayGrid.TryGetPlacement(itemId, out placement);
        }

        public void ApplyCategoryFilter(string category)
        {
            string safeCategory = string.IsNullOrWhiteSpace(category) ? "全部" : category;
            itemTrayView?.ApplyFilter(safeCategory);
            placementFeedbackView?.ShowInfo($"已切换到「{safeCategory}」分类。");
        }

        public void SelectItem(BuildItemPreviewCardView card, bool showInfoPanel = true)
        {
            if (card == null || !itemById.TryGetValue(card.ItemId, out PreviewItem item))
            {
                return;
            }

            selectedItem = item;
            SetSelectedItemInfoVisible(true);
            UpdateSelectedItemInfo(item);
            if (showInfoPanel)
            {
                ShowItemInfoPanel(item);
            }
            else
            {
                RefreshItemInfoPanel(item);
            }

            placementFeedbackView?.ShowInfo($"已查看“{item.DisplayName}”。单击只刷新信息；只可点击信息弹窗里的 Rotate 旋转。");
        }

        public void RotateSelectedItem()
        {
            if (selectedItem == null)
            {
                placementFeedbackView?.ShowInfo("请先单击道具打开信息弹窗，再点击弹窗里的 Rotate。");
                return;
            }

            RotateTrayItem(selectedItem.ItemId);
        }

        public void RotateTrayItem(BuildItemPreviewCardView card)
        {
            if (card == null || !itemById.TryGetValue(card.ItemId, out PreviewItem item))
            {
                return;
            }

            RotateTrayItem(item.ItemId);
        }

        private void RotateTrayItem(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !itemById.TryGetValue(itemId, out PreviewItem item))
            {
                return;
            }

            selectedItem = item;
            SetSelectedItemInfoVisible(true);
            UpdateSelectedItemInfo(item);
            RefreshItemInfoPanel(item);

            if (placedItemIds.Contains(item.ItemId))
            {
                placementFeedbackView?.ShowInfo("该道具已经放到棋盘上；取消后才可重新旋转。");
                return;
            }

            if (!string.IsNullOrEmpty(activeDragItemId)
                || mobileInput != null
                && mobileInput.CurrentState != MobileShapePlacementInputState.Idle
                && mobileInput.CurrentState != MobileShapePlacementInputState.Cancelled)
            {
                placementFeedbackView?.ShowInfo("拖动中、预览中和棋盘上均禁止旋转。");
                return;
            }

            ItemShapeRotation previousRotation = item.Rotation;
            ItemShapeRotation nextRotation = NextRotation(item.Rotation);
            ShapePlacementResult result = TryCommitTrayRotation(item, nextRotation);
            if (result == null || !result.IsValid)
            {
                item.Rotation = previousRotation;
                RefreshTrayPlacement(item);
                placementFeedbackView?.ShowInvalid("旋转失败：道具栏内越界或与其他道具重叠。");
                return;
            }

            item.Rotation = nextRotation;
            RefreshTrayPlacement(item);
            UpdateSelectedItemInfo(item);
            RefreshItemInfoPanel(item);
            placementFeedbackView?.ShowInfo($"已通过信息弹窗旋转“{item.DisplayName}”：{FormatRotation(item.Rotation)}。");
        }

        public void BeginDrag(BuildItemPreviewCardView card, PointerEventData eventData)
        {
            if (card == null)
            {
                return;
            }

            if (!itemById.TryGetValue(card.ItemId, out PreviewItem item))
            {
                return;
            }

            BeginHoldingItem(item, showInfoPanel: false);
            activeDragItemId = item.ItemId;
            itemTrayView?.SetRotateEnabled(item.ItemId, false);
            RefreshItemInfoPanel(item);
            ClearPreviewCells();
            hasLockedPreview = false;
            lockedPreviewResult = null;
            lockedPreviewItem = null;
            ShowDragGhost(selectedItem, eventData, "拖动中");
        }

        public void UpdateDrag(BuildItemPreviewCardView card, PointerEventData eventData)
        {
            if (!CanDragCard(card))
            {
                return;
            }

            UpdateActiveDrag(eventData);
        }

        public void EndDrag(BuildItemPreviewCardView card, PointerEventData eventData)
        {
            if (!CanDragCard(card))
            {
                HideDragGhost();
                return;
            }

            EndActiveDrag(card.ItemId, eventData);
        }

        public void BeginBoardSlotDrag(BuildGridPreviewSlotView slot, PointerEventData eventData)
        {
            if (slot == null
                || boardReceiver == null
                || !boardReceiver.TryGetItemAtCell(slot.Cell, out string itemId)
                || !itemById.TryGetValue(itemId, out PreviewItem item))
            {
                return;
            }

            BeginHoldingItem(item, showInfoPanel: false);
            activeDragItemId = item.ItemId;
            itemTrayView?.SetRotateEnabled(item.ItemId, false);
            RefreshItemInfoPanel(item);
            ClearPreviewCells();
            hasLockedPreview = false;
            lockedPreviewResult = null;
            lockedPreviewItem = null;
            ShowDragGhost(selectedItem, eventData, "拖动中");
        }

        public void UpdateBoardSlotDrag(BuildGridPreviewSlotView slot, PointerEventData eventData)
        {
            if (!CanDragActiveItem())
            {
                return;
            }

            UpdateActiveDrag(eventData);
        }

        public void EndBoardSlotDrag(BuildGridPreviewSlotView slot, PointerEventData eventData)
        {
            if (!CanDragActiveItem())
            {
                HideDragGhost();
                return;
            }

            EndActiveDrag(activeDragItemId, eventData);
        }

        public void ConfirmLockedPreviewFromCell(ItemShapeCell cell)
        {
            placementFeedbackView?.ShowInfo("本包不需要点击 Ghost 确认：合法位置松手已直接放置。");
        }

        public void ResetPreview()
        {
            mobileInput?.Cancel(boardReceiver);
            boardReceiver?.Clear();
            placementSequence = 0;
            placedItemIds.Clear();
            activeDragItemId = string.Empty;
            hasLastPreviewAnchor = false;
            hasLockedPreview = false;
            lastPreviewResult = null;
            lockedPreviewResult = null;
            lockedPreviewItem = null;
            selectedItem = null;
            shapeAwareTrayGrid?.Clear();
            foreach (PreviewItem item in itemById.Values)
            {
                item.Rotation = ItemShapeRotation.Rotation0;
                shapeAwareTrayGrid?.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out _);
                itemTrayView?.SetItemInTray(item.ItemId, true);
                RefreshTrayPlacement(item);
                itemTrayView?.SetRotateEnabled(item.ItemId, true);
            }

            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                slot?.ClearPlaced();
            }

            HideDragGhost();
            SetSelectedItemInfoVisible(false);
            UpdateSelectedItemInfo(null);
            itemInfoPanel?.Hide();
            placementFeedbackView?.ShowNeutral("已取消。单击道具查看信息；在信息弹窗点 Rotate 调整方向；拖到棋盘松手直接放置。");
        }

        private void Awake()
        {
            EnsureReferences();
            BuildSlotLookup();
            BuildShapeLookup();
            BuildItemLookup();
            InitializePlacementRuntime();
            WireButtons();
            itemTrayView?.Initialize(this, itemById.Values.ToList(), CategoryLabels);
            ResetPreview();
        }

        private void OnDestroy()
        {
            foreach (ItemShapeConfig shapeConfig in runtimeShapeConfigs)
            {
                if (shapeConfig != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(shapeConfig);
                    }
                    else
                    {
                        DestroyImmediate(shapeConfig);
                    }
                }
            }

            runtimeShapeConfigs.Clear();
        }

        private void EnsureReferences()
        {
            if (boardGridPreview == null)
            {
                boardGridPreview = FindRectTransform("BoardGridPreview");
            }

            if ((boardSlots == null || boardSlots.Length == 0) && boardGridPreview != null)
            {
                boardSlots = boardGridPreview
                    .GetComponentsInChildren<BuildGridPreviewSlotView>(true)
                    .OrderBy(slot => slot.Y)
                    .ThenBy(slot => slot.X)
                    .ToArray();
            }

            if (itemTrayView == null)
            {
                itemTrayView = FindObjectOfType<BuildItemTrayPreviewView>(true);
            }

            if (placementFeedbackView == null)
            {
                placementFeedbackView = FindObjectOfType<BuildPlacementFeedbackView>(true);
            }

            if (selectedItemInfoRoot == null)
            {
                selectedItemInfoRoot = FindRectTransform("SelectedItemInfo");
            }

            if (itemInfoPanel == null)
            {
                itemInfoPanel = BuildSandboxItemInfoPanel.FindOrCreateInScene();
            }

            if (itemInfoPanel == null)
            {
                itemInfoPanel = FindObjectOfType<BuildSandboxItemInfoPanel>(true);
            }
            if (itemInfoPanel != null)
            {
                itemInfoPanel.SetRotateHandler(RotateInfoPanelItem);
            }

            EnsureSelectedItemInfoCloseButton();
        }

        private void BuildSlotLookup()
        {
            boardSlotByCell.Clear();
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                if (slot != null)
                {
                    slot.SetController(this);
                    boardSlotByCell[slot.Cell] = slot;
                }
            }
        }

        private void BuildShapeLookup()
        {
            shapeById.Clear();
            runtimeShapeConfigs.Clear();
            foreach (ItemShapeConfig shapeConfig in CreatePreviewShapeConfigs())
            {
                shapeConfig.hideFlags = HideFlags.HideAndDontSave;
                runtimeShapeConfigs.Add(shapeConfig);
                shapeById[shapeConfig.shapeId] = shapeConfig;
            }
        }

        private void BuildItemLookup()
        {
            itemById.Clear();
            foreach (PreviewItem item in CreatePreviewItems())
            {
                itemById[item.ItemId] = item;
            }
        }

        private void InitializePlacementRuntime()
        {
            placementSession = new ShapePlacementSession();
            mobileInput = new MobileShapePlacementInputExtension(placementSession);
            shapeAwareTrayGrid = new ShapeAwareItemTrayGrid(
                receiverId: "battle_sandbox_x2_item_tray",
                columnCount: TrayColumns,
                slotCount: TrayColumns * TrayRows,
                commitAllowed: true);
            boardReceiver = new UiBoardShapeGridReceiver(
                "battle_sandbox_x2_board",
                boardGridPreview,
                BoardColumns,
                BoardRows);

            foreach (PreviewItem item in itemById.Values)
            {
                shapeAwareTrayGrid.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out _);
            }
        }

        private void EnsureTrayPlacementLookupForQuery()
        {
            if (shapeAwareTrayGrid != null && shapeAwareTrayGrid.Placements.Count > 0)
            {
                return;
            }

            EnsureShapeLookupForQuery();
            shapeAwareTrayGrid = new ShapeAwareItemTrayGrid(
                receiverId: "battle_sandbox_x2_item_tray_query",
                columnCount: TrayColumns,
                slotCount: TrayColumns * TrayRows,
                commitAllowed: true);

            IEnumerable<PreviewItem> items = itemById.Count > 0
                ? itemById.Values
                : CreatePreviewItems();
            foreach (PreviewItem item in items)
            {
                shapeAwareTrayGrid.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out _);
            }
        }

        private void WireButtons()
        {
            if (resetPreviewButton != null)
            {
                resetPreviewButton.onClick.RemoveAllListeners();
                resetPreviewButton.onClick.AddListener(ResetPreview);
            }

            if (rotatePreviewButton != null)
            {
                rotatePreviewButton.onClick.RemoveAllListeners();
                rotatePreviewButton.interactable = false;
                rotatePreviewButton.gameObject.SetActive(false);
            }
        }

        private void BeginHoldingItem(PreviewItem item, bool showInfoPanel)
        {
            if (item == null)
            {
                return;
            }

            selectedItem = item;
            ShapeItemPayload payload = BuildPayload(selectedItem, ShapePlacementSource.Tray);
            ItemShapeCell? trayAnchor = null;
            if (shapeAwareTrayGrid != null
                && shapeAwareTrayGrid.TryGetPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement placement))
            {
                trayAnchor = placement.AnchorCell;
            }

            mobileInput.TapTrayItem(payload, trayAnchor);
            hasLockedPreview = false;
            lockedPreviewResult = null;
            lockedPreviewItem = null;
            ClearPreviewCells();
            SetSelectedItemInfoVisible(true);
            UpdateSelectedItemInfo(item);
            if (showInfoPanel)
            {
                ShowItemInfoPanel(item);
            }

            RefreshTrayPlacement(item);
            placementFeedbackView?.ShowInfo($"正在拖动“{item.DisplayName}”。拖动中禁止旋转，合法位置松手直接放置。");
        }

        private bool CanDragCard(BuildItemPreviewCardView card)
        {
            return card != null
                && selectedItem != null
                && mobileInput != null
                && boardReceiver != null
                && string.Equals(card.ItemId, selectedItem.ItemId, StringComparison.Ordinal)
                && string.Equals(card.ItemId, activeDragItemId, StringComparison.Ordinal);
        }

        private bool CanDragActiveItem()
        {
            return selectedItem != null
                && mobileInput != null
                && boardReceiver != null
                && !string.IsNullOrWhiteSpace(activeDragItemId)
                && string.Equals(selectedItem.ItemId, activeDragItemId, StringComparison.Ordinal);
        }

        private void UpdateActiveDrag(PointerEventData eventData)
        {
            ShowDragGhost(selectedItem, eventData, "拖动中");
            if (TryPreviewTrayDrag(eventData, out ShapePlacementResult trayResult))
            {
                lastPreviewResult = trayResult;
                hasLastPreviewAnchor = trayResult != null;
                if (trayResult != null)
                {
                    lastPreviewAnchor = trayResult.AnchorCell;
                }

                ClearPreviewCells();
                if (trayResult != null && trayResult.IsValid)
                {
                    placementFeedbackView?.ShowValid("松手移动到道具栏空位。");
                    return;
                }

                placementFeedbackView?.ShowInvalid(FormatInvalidFeedback(
                    trayResult?.InvalidReason ?? ShapePlacementInvalidReason.OutOfGrid));
                return;
            }

            ShapePlacementResult result = mobileInput.DragToReceiver(
                boardReceiver,
                eventData.position,
                eventData.pressEventCamera);
            lastPreviewResult = result;
            hasLastPreviewAnchor = result != null;
            if (result != null)
            {
                lastPreviewAnchor = result.AnchorCell;
            }

            DrawPreviewResult(result, locked: false);
            if (result != null && result.IsValid)
            {
                placementFeedbackView?.ShowValid($"松手直接放置“{selectedItem.DisplayName}”。");
                return;
            }

            placementFeedbackView?.ShowInvalid(FormatInvalidFeedback(
                result?.InvalidReason ?? ShapePlacementInvalidReason.OutOfGrid));
        }

        private void EndActiveDrag(string itemId, PointerEventData eventData)
        {
            if (TryCommitTrayDrag(itemId, eventData))
            {
                return;
            }

            ShapePlacementResult result = mobileInput.DragToReceiver(
                boardReceiver,
                eventData.position,
                eventData.pressEventCamera);
            lastPreviewResult = result;
            hasLastPreviewAnchor = result != null;
            if (result != null)
            {
                lastPreviewAnchor = result.AnchorCell;
            }

            if (result == null || !result.IsValid)
            {
                hasLockedPreview = false;
                lockedPreviewResult = null;
                lockedPreviewItem = null;
                mobileInput?.Cancel(boardReceiver);
                activeDragItemId = string.Empty;
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                placementFeedbackView?.ShowInvalid(FormatInvalidFeedback(
                    result?.InvalidReason ?? ShapePlacementInvalidReason.OutOfGrid));
                return;
            }

            ShapePlacementResult commitResult = placementSession.Commit(boardReceiver);
            if (commitResult == null || !commitResult.IsValid)
            {
                mobileInput?.Cancel(boardReceiver);
                activeDragItemId = string.Empty;
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                placementFeedbackView?.ShowInvalid(FormatInvalidFeedback(
                    commitResult?.InvalidReason ?? ShapePlacementInvalidReason.OutOfGrid));
                return;
            }

            bool movedPlacedItem = placedItemIds.Contains(selectedItem.ItemId);
            placedItemIds.Add(selectedItem.ItemId);
            placementSequence = placedItemIds.Count;
            shapeAwareTrayGrid?.RemoveItem(selectedItem.ItemId);
            itemTrayView?.SetItemInTray(selectedItem.ItemId, false);
            RedrawBoardPlacedVisuals();

            lastPreviewResult = commitResult;
            hasLockedPreview = false;
            lockedPreviewResult = null;
            lockedPreviewItem = null;
            activeDragItemId = string.Empty;
            itemTrayView?.SetRotateEnabled(selectedItem.ItemId, false);
            mobileInput?.Cancel(boardReceiver);
            HideDragGhost();
            placementFeedbackView?.ShowValid(movedPlacedItem
                ? $"已移动“{selectedItem.DisplayName}”。"
                : $"已放置“{selectedItem.DisplayName}”。");
            UpdateSelectedItemInfo(selectedItem);
            RefreshItemInfoPanel(selectedItem);
        }

        private bool TryPreviewTrayDrag(PointerEventData eventData, out ShapePlacementResult result)
        {
            result = null;
            if (eventData == null
                || selectedItem == null
                || itemTrayView == null
                || shapeAwareTrayGrid == null
                || placementSession == null)
            {
                return false;
            }

            if (!itemTrayView.TryScreenPointToTrayCell(
                    eventData.position,
                    eventData.pressEventCamera,
                    out ItemShapeCell anchorCell))
            {
                return false;
            }

            result = placementSession.Preview(shapeAwareTrayGrid, anchorCell);
            return true;
        }

        private bool TryCommitTrayDrag(string itemId, PointerEventData eventData)
        {
            if (!TryPreviewTrayDrag(eventData, out ShapePlacementResult result))
            {
                return false;
            }

            lastPreviewResult = result;
            hasLastPreviewAnchor = result != null;
            if (result != null)
            {
                lastPreviewAnchor = result.AnchorCell;
            }

            if (result == null || !result.IsValid)
            {
                hasLockedPreview = false;
                lockedPreviewResult = null;
                lockedPreviewItem = null;
                mobileInput?.Cancel(shapeAwareTrayGrid);
                activeDragItemId = string.Empty;
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshTrayPlacement(selectedItem);
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                placementFeedbackView?.ShowInvalid(FormatInvalidFeedback(
                    result?.InvalidReason ?? ShapePlacementInvalidReason.OutOfGrid));
                return true;
            }

            ShapePlacementResult commitResult = placementSession.Commit(shapeAwareTrayGrid);
            if (commitResult == null || !commitResult.IsValid)
            {
                mobileInput?.Cancel(shapeAwareTrayGrid);
                activeDragItemId = string.Empty;
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshTrayPlacement(selectedItem);
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                placementFeedbackView?.ShowInvalid(FormatInvalidFeedback(
                    commitResult?.InvalidReason ?? ShapePlacementInvalidReason.OutOfGrid));
                return true;
            }

            lastPreviewResult = commitResult;
            hasLockedPreview = false;
            lockedPreviewResult = null;
            lockedPreviewItem = null;
            bool movedFromBoard = placedItemIds.Remove(selectedItem.ItemId);
            if (movedFromBoard)
            {
                placementSequence = placedItemIds.Count;
                boardReceiver?.RemoveItem(selectedItem.ItemId);
                RedrawBoardPlacedVisuals();
            }

            activeDragItemId = string.Empty;
            itemTrayView?.SetItemInTray(selectedItem.ItemId, true);
            itemTrayView?.SetRotateEnabled(selectedItem.ItemId, !placedItemIds.Contains(selectedItem.ItemId));
            mobileInput?.Cancel(shapeAwareTrayGrid);
            RefreshTrayPlacement(selectedItem);
            RefreshItemInfoPanel(selectedItem);
            ClearPreviewCells();
            HideDragGhost();
            placementFeedbackView?.ShowValid($"已移动“{selectedItem.DisplayName}”到道具栏空位。");
            UpdateSelectedItemInfo(selectedItem);
            return true;
        }

        private ShapePlacementResult PreviewPlacementAtCell(
            PreviewItem item,
            ItemShapeCell anchor,
            bool locked)
        {
            if (item == null || placementSession == null || boardReceiver == null)
            {
                return null;
            }

            ShapePlacementResult result = placementSession.Preview(boardReceiver, anchor);
            lastPreviewResult = result;
            DrawPreviewResult(result, locked);
            return result;
        }

        private ShapeItemPayload BuildPayload(PreviewItem item, ShapePlacementSource source)
        {
            if (item == null || !shapeById.TryGetValue(item.ShapeId, out ItemShapeConfig shapeConfig))
            {
                return default;
            }

            return new ShapeItemPayload(
                item.ItemId,
                item.ShapeId,
                item.Rotation,
                shapeConfig.occupiedOffsets,
                source);
        }

        private ShapePlacementResult TryCommitTrayRotation(PreviewItem item, ItemShapeRotation nextRotation)
        {
            if (item == null
                || shapeAwareTrayGrid == null
                || !shapeAwareTrayGrid.TryGetPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement placement)
                || placement == null
                || !shapeById.TryGetValue(item.ShapeId, out ItemShapeConfig shapeConfig))
            {
                return null;
            }

            ShapeItemPayload rotatedPayload = new(
                item.ItemId,
                item.ShapeId,
                nextRotation,
                shapeConfig.occupiedOffsets,
                ShapePlacementSource.Tray);
            ShapePlacementSession traySession = new();
            traySession.Begin(rotatedPayload, trayAnchorCell: placement.AnchorCell);
            return traySession.Commit(shapeAwareTrayGrid);
        }

        private static ItemShapeRotation NextRotation(ItemShapeRotation rotation)
        {
            return rotation == ItemShapeRotation.Rotation0
                ? ItemShapeRotation.Rotation90
                : ItemShapeRotation.Rotation0;
        }

        private void RefreshTrayPlacement(PreviewItem item)
        {
            if (itemTrayView == null || item == null)
            {
                return;
            }

            if (TryGetTrayPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement placement)
                && placement != null)
            {
                itemTrayView.RefreshItemPlacement(TrayPlacementViewModel.FromPlacement(
                    placement,
                    true,
                    TrayColumns));
            }
        }

        private void DrawPreviewResult(ShapePlacementResult result, bool locked)
        {
            ClearPreviewCells();
            if (result == null)
            {
                return;
            }

            foreach (ItemShapeCell cell in result.OccupiedCells)
            {
                if (boardSlotByCell.TryGetValue(cell, out BuildGridPreviewSlotView slot))
                {
                    if (locked && result.IsValid)
                    {
                        slot.SetLockedPreview();
                    }
                    else
                    {
                        slot.SetPreview(result.IsValid);
                    }
                }
            }
        }

        private void ClearPreviewCells()
        {
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                slot?.ClearPreview();
            }
        }

        private void RedrawBoardPlacedVisuals()
        {
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                slot?.ClearPlaced();
            }

            if (boardReceiver == null)
            {
                return;
            }

            foreach (KeyValuePair<ItemShapeCell, string> occupied in boardReceiver.OccupiedCells)
            {
                if (!boardSlotByCell.TryGetValue(occupied.Key, out BuildGridPreviewSlotView slot)
                    || slot == null)
                {
                    continue;
                }

                string displayName = itemById.TryGetValue(occupied.Value, out PreviewItem item)
                    ? item.DisplayName
                    : occupied.Value;
                slot.SetPlaced(displayName);
            }
        }

        private void UpdateSelectedItemInfo(PreviewItem item)
        {
            if (selectedItemInfoTitle != null)
            {
                selectedItemInfoTitle.text = item == null ? "道具信息" : item.DisplayName;
            }

            if (selectedItemInfoBody == null)
            {
                return;
            }

            if (item == null)
            {
                selectedItemInfoBody.text = "单击道具查看信息；只在信息弹窗点 Rotate 旋转；拖到棋盘松手直接放置。";
                return;
            }

            string state = placedItemIds.Contains(item.ItemId)
                ? "已放置"
                : string.Equals(activeDragItemId, item.ItemId, StringComparison.Ordinal)
                    ? "拖动中"
                    : "仍在道具栏";
            selectedItemInfoBody.text =
                $"分类：{item.Category}\n" +
                $"形状：{item.ShapeDisplayName}\n" +
                $"旋转：{FormatRotation(item.Rotation)}\n" +
                $"状态：{state}";
        }

        private void EnsureSelectedItemInfoCloseButton()
        {
            if (selectedItemInfoRoot == null)
            {
                return;
            }

            if (selectedItemInfoCloseButton == null)
            {
                Transform existing = selectedItemInfoRoot.Find("SelectedItemInfoCloseButton");
                if (existing != null)
                {
                    selectedItemInfoCloseButton = existing.GetComponent<Button>();
                }
            }

            if (selectedItemInfoCloseButton == null)
            {
                GameObject buttonObject = new("SelectedItemInfoCloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                buttonObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                buttonObject.transform.SetParent(selectedItemInfoRoot, false);
                RectTransform rect = buttonObject.GetComponent<RectTransform>();
                SetRuntimeAnchors(rect, new Vector2(0.86f, 0.84f), new Vector2(0.98f, 0.98f));
                Image image = buttonObject.GetComponent<Image>();
                image.color = new Color(0.34f, 0.25f, 0.12f, 0.92f);
                selectedItemInfoCloseButton = buttonObject.GetComponent<Button>();

                Text label = CreateRuntimeText("Label", buttonObject.transform, "×", 18, FontStyle.Bold, TextAnchor.MiddleCenter);
                SetRuntimeAnchors(label.rectTransform, Vector2.zero, Vector2.one);
            }

            selectedItemInfoCloseButton.onClick.RemoveListener(HideSelectedItemInfo);
            selectedItemInfoCloseButton.onClick.AddListener(HideSelectedItemInfo);
        }

        private void HideSelectedItemInfo()
        {
            SetSelectedItemInfoVisible(false);
        }

        private void SetSelectedItemInfoVisible(bool visible)
        {
            if (selectedItemInfoRoot != null)
            {
                selectedItemInfoRoot.gameObject.SetActive(visible);
            }
        }

        private void ShowItemInfoPanel(PreviewItem item)
        {
            if (itemInfoPanel == null || item == null)
            {
                return;
            }

            itemInfoPanel.Show(item, BuildItemInfoContext(item), CanRotateItemFromInfoPanel(item));
        }

        private void RefreshItemInfoPanel(PreviewItem item)
        {
            if (itemInfoPanel == null || item == null)
            {
                return;
            }

            itemInfoPanel.RefreshIfShowing(item, BuildItemInfoContext(item), CanRotateItemFromInfoPanel(item));
        }

        private void RotateInfoPanelItem(string itemId)
        {
            RotateTrayItem(itemId);
        }

        private bool CanRotateItemFromInfoPanel(PreviewItem item)
        {
            if (item == null || placedItemIds.Contains(item.ItemId))
            {
                return false;
            }

            if (!shapeById.TryGetValue(item.ShapeId, out ItemShapeConfig shapeConfig)
                || shapeConfig == null
                || !shapeConfig.rotationAllowed)
            {
                return false;
            }

            return string.IsNullOrEmpty(activeDragItemId)
                && (mobileInput == null
                    || mobileInput.CurrentState == MobileShapePlacementInputState.Idle
                    || mobileInput.CurrentState == MobileShapePlacementInputState.Cancelled);
        }

        private BuildGridInteractionItemInfoContext BuildItemInfoContext(PreviewItem item)
        {
            if (item == null || !shapeById.TryGetValue(item.ShapeId, out ItemShapeConfig shapeConfig))
            {
                return BuildGridInteractionItemInfoContext.Empty;
            }

            IReadOnlyList<ItemShapeCell> rotatedCells =
                NormalizeCells(ItemShapePlacementValidator.CalculateOccupiedCells(
                    shapeConfig,
                    new ItemShapeCell(0, 0),
                    item.Rotation));

            string occupiedCells = $"约占 {shapeConfig.cellCount} 格；合法位置松手直接放置，非法位置返回道具栏。";
            if (lastPreviewResult != null
                && IsPlacementResultForItem(lastPreviewResult, item)
                && lastPreviewResult.OccupiedCells.Count > 0)
            {
                occupiedCells = FormatCells(lastPreviewResult.OccupiedCells);
            }

            string multiCellShape = $"{item.ShapeDisplayName}，共 {shapeConfig.cellCount} 格。";
            string rotatedText = $"{FormatRotation(item.Rotation)}：{FormatCells(rotatedCells)}";
            return new BuildGridInteractionItemInfoContext(multiCellShape, occupiedCells, rotatedText);
        }

        private static bool IsPlacementResultForItem(ShapePlacementResult result, PreviewItem item)
        {
            if (result == null || item == null)
            {
                return false;
            }

            return string.Equals(result.ItemId, item.ItemId, StringComparison.Ordinal)
                || result.ItemId.StartsWith(item.ItemId + "_", StringComparison.Ordinal);
        }

        private static IReadOnlyList<ItemShapeCell> NormalizeCells(IReadOnlyList<ItemShapeCell> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return Array.Empty<ItemShapeCell>();
            }

            int minX = cells.Min(cell => cell.x);
            int minY = cells.Min(cell => cell.y);
            return cells
                .Select(cell => new ItemShapeCell(cell.x - minX, cell.y - minY))
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
        }

        private static string FormatCells(IReadOnlyList<ItemShapeCell> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return "暂无占格";
            }

            return string.Join("、", cells.Select(cell => $"第{cell.x + 1}列第{cell.y + 1}行"));
        }

        private void ShowDragGhost(PreviewItem item, PointerEventData eventData, string state)
        {
            if (dragGhostRoot == null || item == null || eventData == null)
            {
                return;
            }

            dragGhostRoot.gameObject.SetActive(true);
            dragGhostRoot.position = eventData.position + new Vector2(0f, 50f);
            if (dragGhostText != null)
            {
                dragGhostText.text = $"{item.DisplayName}\n{state}";
            }
        }

        private void HideDragGhost()
        {
            if (dragGhostRoot != null)
            {
                dragGhostRoot.gameObject.SetActive(false);
            }
        }

        private void EnsureShapeLookupForQuery()
        {
            if (shapeById.Count > 0)
            {
                return;
            }

            foreach (ItemShapeConfig shapeConfig in CreatePreviewShapeConfigs())
            {
                shapeConfig.hideFlags = HideFlags.HideAndDontSave;
                runtimeShapeConfigs.Add(shapeConfig);
                shapeById[shapeConfig.shapeId] = shapeConfig;
            }
        }

        private static bool IsFootprintVertical(ItemShapeConfig shapeConfig, ItemShapeRotation rotation)
        {
            if (shapeConfig == null || shapeConfig.occupiedOffsets == null || shapeConfig.occupiedOffsets.Count <= 1)
            {
                return false;
            }

            ItemShapeCell[] rotatedCells = shapeConfig.occupiedOffsets
                .Select(cell => ApplyRotation(cell, rotation))
                .ToArray();
            int xRange = rotatedCells.Max(cell => cell.x) - rotatedCells.Min(cell => cell.x);
            int yRange = rotatedCells.Max(cell => cell.y) - rotatedCells.Min(cell => cell.y);
            return yRange >= xRange;
        }

        private static ItemShapeCell ApplyRotation(ItemShapeCell offset, ItemShapeRotation rotation)
        {
            return rotation switch
            {
                ItemShapeRotation.Rotation90 => new ItemShapeCell(offset.y, -offset.x),
                ItemShapeRotation.Rotation180 => new ItemShapeCell(-offset.x, -offset.y),
                ItemShapeRotation.Rotation270 => new ItemShapeCell(-offset.y, offset.x),
                _ => offset
            };
        }

        private static string FormatRotation(ItemShapeRotation rotation)
        {
            return rotation switch
            {
                ItemShapeRotation.Rotation90 => "右转",
                ItemShapeRotation.Rotation180 => "倒转",
                ItemShapeRotation.Rotation270 => "左转",
                _ => "正向"
            };
        }

        private static string FormatInvalidFeedback(ShapePlacementInvalidReason reason)
        {
            return reason switch
            {
                ShapePlacementInvalidReason.OutOfGrid => "越界：有格子超出棋盘。",
                ShapePlacementInvalidReason.CellOccupied => "重叠：目标格子已被占用。",
                ShapePlacementInvalidReason.ShapeInvalid => "形状数据不可用。",
                ShapePlacementInvalidReason.MissingShapeConfig => "缺少形状配置。",
                ShapePlacementInvalidReason.CommitDisabled => "当前不能提交放置。",
                _ => "当前位置不能放置。"
            };
        }

        private static RectTransform FindRectTransform(string objectName)
        {
            Transform[] transforms = FindObjectsOfType<Transform>(true);
            foreach (Transform target in transforms)
            {
                if (target != null && target.name == objectName)
                {
                    return target as RectTransform;
                }
            }

            return null;
        }

        private static Text CreateRuntimeText(
            string name,
            Transform parent,
            string value,
            int size,
            FontStyle style,
            TextAnchor alignment)
        {
            GameObject target = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            target.transform.SetParent(parent, false);
            Text text = target.GetComponent<Text>();
            text.text = value ?? string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = new Color(0.91f, 0.88f, 0.76f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static void SetRuntimeAnchors(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static ItemShapeConfig CreateShape(
            string shapeId,
            string displayName,
            bool rotationAllowed,
            params ItemShapeCell[] occupiedOffsets)
        {
            ItemShapeConfig config = ScriptableObject.CreateInstance<ItemShapeConfig>();
            config.configId = "preview_" + shapeId;
            config.shapeId = shapeId;
            config.shapeName = displayName;
            config.visualKey = "preview_" + shapeId;
            config.rotationAllowed = rotationAllowed;
            config.defaultRotation = ItemShapeRotation.Rotation0;
            config.occupiedOffsets = occupiedOffsets.ToList();
            config.cellCount = config.occupiedOffsets.Count;
            config.devOnly = true;
            config.isEnabled = false;
            return config;
        }

        [Serializable]
        public sealed class PreviewItem
        {
            public PreviewItem(
                string itemId,
                string displayName,
                string category,
                string shapeId,
                string shapeDisplayName,
                Color cardColor)
            {
                ItemId = itemId ?? string.Empty;
                DisplayName = displayName ?? string.Empty;
                Category = category ?? string.Empty;
                ShapeId = shapeId ?? string.Empty;
                ShapeDisplayName = shapeDisplayName ?? string.Empty;
                CardColor = cardColor;
                Rotation = ItemShapeRotation.Rotation0;
            }

            public string ItemId { get; }
            public string DisplayName { get; }
            public string Category { get; }
            public string ShapeId { get; }
            public string ShapeDisplayName { get; }
            public Color CardColor { get; }
            public ItemShapeRotation Rotation { get; set; }
        }

        private sealed class UiBoardShapeGridReceiver : ShapeGridReceiver
        {
            private readonly Dictionary<ItemShapeCell, string> occupiedByItemId = new();
            private readonly RectTransform boardRect;

            public UiBoardShapeGridReceiver(
                string receiverId,
                RectTransform boardRect,
                int width,
                int height)
            {
                ReceiverId = string.IsNullOrWhiteSpace(receiverId) ? "battle_sandbox_board" : receiverId;
                this.boardRect = boardRect;
                Width = width;
                Height = height;
            }

            public string ReceiverId { get; }
            public ShapePlacementSource ReceiverSource => ShapePlacementSource.Board;
            public int Width { get; }
            public int Height { get; }
            public IReadOnlyDictionary<ItemShapeCell, string> OccupiedCells => occupiedByItemId;

            public bool TryGetItemAtCell(ItemShapeCell cell, out string itemId)
            {
                return occupiedByItemId.TryGetValue(cell, out itemId);
            }

            public bool ScreenPointToCell(Vector2 screenPoint, Camera eventCamera, out ItemShapeCell anchorCell)
            {
                anchorCell = default;
                if (boardRect == null)
                {
                    return false;
                }

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        boardRect,
                        screenPoint,
                        eventCamera,
                        out Vector2 localPoint))
                {
                    return false;
                }

                Rect rect = boardRect.rect;
                if (rect.width <= 0f || rect.height <= 0f)
                {
                    return false;
                }

                float normalizedX = (localPoint.x - rect.xMin) / rect.width;
                float normalizedY = (localPoint.y - rect.yMin) / rect.height;
                if (normalizedX < 0f || normalizedX >= 1f || normalizedY < 0f || normalizedY >= 1f)
                {
                    return false;
                }

                int x = Mathf.Clamp(Mathf.FloorToInt(normalizedX * Width), 0, Width - 1);
                int y = Mathf.Clamp(Mathf.FloorToInt((1f - normalizedY) * Height), 0, Height - 1);
                anchorCell = new ItemShapeCell(x, y);
                return true;
            }

            public ShapePlacementResult CanPlace(ShapeItemPayload payload, ItemShapeCell anchorCell)
            {
                if (!payload.IsValid)
                {
                    return BuildResult(payload, anchorCell, Array.Empty<ItemShapeCell>(), false, ShapePlacementInvalidReason.ShapeInvalid);
                }

                IReadOnlyList<ItemShapeCell> occupiedCells = payload.BuildOccupiedCells(anchorCell);
                if (occupiedCells.Count == 0)
                {
                    return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.ShapeInvalid);
                }

                if (occupiedCells.Any(cell => !IsWithinBounds(cell)))
                {
                    return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.OutOfGrid);
                }

                if (occupiedCells.Any(cell =>
                        occupiedByItemId.TryGetValue(cell, out string itemId)
                        && !string.Equals(itemId, payload.ItemId, StringComparison.Ordinal)))
                {
                    return BuildResult(payload, anchorCell, occupiedCells, false, ShapePlacementInvalidReason.CellOccupied);
                }

                return BuildResult(payload, anchorCell, occupiedCells, true, ShapePlacementInvalidReason.None);
            }

            public void Preview(ShapePlacementSession session, ShapePlacementResult result)
            {
            }

            public ShapePlacementResult Commit(ShapePlacementSession session)
            {
                if (session == null || !session.HasActivePayload)
                {
                    return BuildResult(default, default, Array.Empty<ItemShapeCell>(), false, ShapePlacementInvalidReason.ShapeInvalid);
                }

                ItemShapeCell anchor = session.BoardAnchorCell
                    ?? session.LastLegalBoardAnchor
                    ?? session.PreviewResult?.AnchorCell
                    ?? default;
                ShapePlacementResult result = CanPlace(session.CurrentPayload, anchor);
                if (!result.IsValid)
                {
                    return result;
                }

                ReleaseItem(session.CurrentPayload.ItemId);
                foreach (ItemShapeCell cell in result.OccupiedCells)
                {
                    occupiedByItemId[cell] = session.CurrentPayload.ItemId;
                }

                return result;
            }

            public void Cancel(ShapePlacementSession session)
            {
            }

            public void Clear()
            {
                occupiedByItemId.Clear();
            }

            public bool RemoveItem(string itemId)
            {
                bool existed = !string.IsNullOrWhiteSpace(itemId)
                    && occupiedByItemId.ContainsValue(itemId);
                ReleaseItem(itemId);
                return existed;
            }

            private bool IsWithinBounds(ItemShapeCell cell)
            {
                return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
            }

            private void ReleaseItem(string itemId)
            {
                if (string.IsNullOrWhiteSpace(itemId))
                {
                    return;
                }

                foreach (ItemShapeCell cell in occupiedByItemId
                             .Where(pair => string.Equals(pair.Value, itemId, StringComparison.Ordinal))
                             .Select(pair => pair.Key)
                             .ToArray())
                {
                    occupiedByItemId.Remove(cell);
                }
            }

            private ShapePlacementResult BuildResult(
                ShapeItemPayload payload,
                ItemShapeCell anchorCell,
                IReadOnlyList<ItemShapeCell> occupiedCells,
                bool valid,
                ShapePlacementInvalidReason reason)
            {
                return new ShapePlacementResult(
                    payload.ItemId,
                    payload.ShapeId,
                    anchorCell,
                    occupiedCells ?? Array.Empty<ItemShapeCell>(),
                    valid,
                    reason,
                    CollectAdjacentItems(occupiedCells, payload.ItemId),
                    energyConnected: false,
                    energyConnectionNote: "battle_sandbox_vertical_slice_devonly");
            }

            private IReadOnlyList<string> CollectAdjacentItems(
                IReadOnlyList<ItemShapeCell> cells,
                string selfItemId)
            {
                HashSet<ItemShapeCell> ownCells = new(cells ?? Array.Empty<ItemShapeCell>());
                HashSet<string> adjacent = new(StringComparer.Ordinal);
                foreach (ItemShapeCell cell in ownCells)
                {
                    foreach (ItemShapeCell neighbor in EnumerateCardinalNeighbors(cell))
                    {
                        if (ownCells.Contains(neighbor))
                        {
                            continue;
                        }

                        if (occupiedByItemId.TryGetValue(neighbor, out string itemId)
                            && !string.IsNullOrWhiteSpace(itemId)
                            && !string.Equals(itemId, selfItemId, StringComparison.Ordinal))
                        {
                            adjacent.Add(itemId);
                        }
                    }
                }

                return adjacent.OrderBy(id => id, StringComparer.Ordinal).ToArray();
            }

            private static IEnumerable<ItemShapeCell> EnumerateCardinalNeighbors(ItemShapeCell cell)
            {
                yield return new ItemShapeCell(cell.x + 1, cell.y);
                yield return new ItemShapeCell(cell.x - 1, cell.y);
                yield return new ItemShapeCell(cell.x, cell.y + 1);
                yield return new ItemShapeCell(cell.x, cell.y - 1);
            }
        }
    }
}
