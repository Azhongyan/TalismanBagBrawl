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
        private const float BattlePrepareMoveSpeed = 9f;
        private const float BattlePreparePullOffset = 320f;
        private const string BattlePrepareActionBarName = "V04BattlePrepareBottomActions";
        private const string BattlePrepareOverlayName = "V04BattlePrepareDarkOverlay";
        private const string ItemTrayLockedOverlayName = "ItemTrayBattleLockedOverlay";
        private const string EnemyCombatFeedbackPanelName = "EnemyCombatFeedbackPanel";
        private const string EnemyCombatFeedbackFloatingRootName = "EnemyCombatFeedbackFloatingRoot";
        private const string DevChapterDropdownSlotName = "DevChapterDropdownSlot";
        private const string DragGhostCellLayerName = "DragGhostCellLayer";
        private const string DragGhostCellNamePrefix = "DragGhostCell_";

        public static readonly string[] CategoryLabels =
            BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryDisplayLabels.ToArray();

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
        [SerializeField] private RectTransform battlePrepareMotionRoot;
        [SerializeField] private Image battlePrepareDarkOverlay;
        [SerializeField] private RectTransform battlePrepareActionBar;
        [SerializeField] private Button battlePrepareBackButton;
        [SerializeField] private Button battlePrepareStateButton;
        [SerializeField] private Button battlePrepareToggleButton;
        [SerializeField] private Text battlePrepareStateButtonText;
        [SerializeField] private Text battlePrepareToggleButtonText;
        [SerializeField] private RectTransform enemyCombatFeedbackPanel;
        [SerializeField] private RectTransform enemyCombatFeedbackFloatingRoot;
        [SerializeField] private BattleSandboxEnemyCombatFeedbackController enemyCombatFeedbackController;
        [SerializeField] private RectTransform devChapterDropdownSlot;
        [SerializeField] private Button devChapterDropdownButton;
        [SerializeField] private Text devChapterDropdownLabelText;
        [SerializeField] private BattleSandboxManaLoopRuntime manaLoopRuntime;
        [SerializeField] private BattleSandboxRuntimeLoopRuntime runtimeLoopRuntime;

        private readonly Dictionary<ItemShapeCell, BuildGridPreviewSlotView> boardSlotByCell = new();
        private readonly Dictionary<string, PreviewItem> itemById = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ItemShapeConfig> shapeById = new(StringComparer.Ordinal);
        private readonly List<ItemShapeConfig> runtimeShapeConfigs = new();
        private ShapePlacementSession placementSession;
        private MobileShapePlacementInputExtension mobileInput;
        private ShapeAwareItemTrayGrid shapeAwareTrayGrid;
        private UiBoardShapeGridReceiver boardReceiver;
        private PreviewItem selectedItem;
        private ShapePlacementResult lastPreviewResult;
        private ItemShapeCell lastPreviewAnchor;
        private bool hasLastPreviewAnchor;
        private readonly HashSet<string> placedItemIds = new(StringComparer.Ordinal);
        private string activeDragItemId = string.Empty;
        private int placementSequence;
        private CanvasGroup itemTrayCanvasGroup;
        private Vector2 battlePrepareNormalPosition;
        private Vector2 battlePrepareOpenPosition;
        private bool hasBattlePreparePositions;
        private bool battlePrepareStateActive;
        private bool battlePrepareContinueStateActive;
        private bool sandboxBattleActive;
        private bool enemyCombatFeedbackVisibleLastFrame;
        private Image itemTrayLockedOverlay;
        private CanvasGroup itemTrayLockedOverlayCanvasGroup;
        private RectTransform dragGhostCellLayer;
        private Image dragGhostBackgroundImage;
        private Vector2 dragGhostDefaultSize;
        private Color dragGhostDefaultBackgroundColor;
        private bool hasDragGhostDefaults;

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
        public bool IsSandboxBattleModeActive =>
            sandboxBattleActive && !battlePrepareStateActive && !battlePrepareContinueStateActive;

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
                CreateShape("vertical_3", "竖三格", true, new ItemShapeCell(0, 0), new ItemShapeCell(0, 1), new ItemShapeCell(0, 2)),
                CreateShape("Single1", "单格", false, new ItemShapeCell(0, 0)),
                CreateShape("Corner3", "拐角三格", true, new ItemShapeCell(0, 0), new ItemShapeCell(1, 0), new ItemShapeCell(0, 1)),
                CreateShape("Square4", "方四格", false, new ItemShapeCell(0, 0), new ItemShapeCell(1, 0), new ItemShapeCell(0, 1), new ItemShapeCell(1, 1))
            };
        }

        public static List<PreviewItem> CreatePreviewItems()
        {
            return BuildSandboxLegacyAndAdvancedItemRosterCatalog.AllItems
                .Select(row => new PreviewItem(
                    row.ItemId,
                    row.DisplayName,
                    row.CategoryDisplayName,
                    row.ShapeId,
                    row.ShapeDisplayName,
                    ResolveRosterCardColor(row),
                    BuildSandboxItemStatCatalog.Resolve(row.ItemId),
                    row.CategoryIds))
                .ToList();
        }

        private static Color ResolveRosterCardColor(BuildSandboxLegacyAndAdvancedItemRosterRow row)
        {
            switch (row?.ItemId ?? string.Empty)
            {
                case "preview_x2_wood_talisman":
                    return new Color(0.24f, 0.38f, 0.22f, 1f);
                case "preview_fire_talisman":
                    return new Color(0.55f, 0.20f, 0.14f, 1f);
                case "preview_guard_wood":
                    return new Color(0.22f, 0.34f, 0.40f, 1f);
                case "preview_taomu_sword":
                    return new Color(0.48f, 0.30f, 0.16f, 1f);
                case "preview_cleanse_corner":
                    return new Color(0.38f, 0.25f, 0.52f, 1f);
                case "preview_stone_core":
                    return new Color(0.36f, 0.32f, 0.25f, 1f);
                case "preview_energy_incense":
                    return new Color(0.47f, 0.35f, 0.12f, 1f);
                case "preview_old_bell":
                    return new Color(0.28f, 0.42f, 0.38f, 1f);
                case "preview_thunder_sword":
                    return new Color(0.30f, 0.31f, 0.58f, 1f);
                case "preview_soul_seal":
                    return new Color(0.42f, 0.24f, 0.30f, 1f);
                case "fire_talisman_basic":
                    return new Color(1f, 0.42f, 0.2f, 1f);
                case "thunder_talisman_basic":
                    return new Color(0.62f, 0.58f, 1f, 1f);
                case "shield_talisman_basic":
                    return new Color(0.45f, 0.9f, 0.55f, 1f);
                case "qi_pill_basic":
                    return new Color(0.95f, 0.5f, 0.92f, 1f);
                case "spirit_stone_basic":
                    return new Color(0.28f, 0.68f, 1f, 1f);
                case "sword_pill_basic":
                    return new Color(0.8f, 0.85f, 0.9f, 1f);
                case "chain_thunder_talisman_basic":
                    return new Color(0.48f, 0.72f, 1f, 1f);
                case "purify_talisman_basic":
                    return new Color(0.58f, 0.92f, 1f, 1f);
                case "soul_suppress_talisman_basic":
                    return new Color(0.72f, 0.62f, 0.96f, 1f);
                case "seal_basic":
                    return new Color(0.9f, 0.82f, 0.42f, 1f);
                case "water_talisman_basic":
                    return new Color(0.35f, 0.82f, 0.95f, 1f);
                case "exorcism_bell_basic":
                    return new Color(0.96f, 0.74f, 0.34f, 1f);
                case "peach_wood_basic":
                    return new Color(0.72f, 0.48f, 0.28f, 1f);
                default:
                    return row != null && row.HasCategory(BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryCore)
                        ? new Color(0.36f, 0.32f, 0.25f, 1f)
                        : new Color(0.44f, 0.35f, 0.18f, 1f);
            }
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

        public BuildSandboxLayoutSnapshot BuildCurrentLayoutSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            if (itemById.Count == 0)
            {
                BuildItemLookup();
            }

            EnsureShapeLookupForQuery();
            if (boardReceiver == null || boardReceiver.OccupiedCells.Count == 0)
            {
                return snapshot;
            }

            foreach (IGrouping<string, KeyValuePair<ItemShapeCell, string>> group in boardReceiver.OccupiedCells
                         .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                         .GroupBy(pair => pair.Value, StringComparer.Ordinal)
                         .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                if (!itemById.TryGetValue(group.Key, out PreviewItem item) || item == null)
                {
                    continue;
                }

                List<ItemShapeCell> cells = group
                    .Select(pair => pair.Key)
                    .OrderBy(cell => cell.x)
                    .ThenBy(cell => cell.y)
                    .ToList();
                snapshot.placedItems.Add(BattleSandboxBuildCombatPreviewBuilder.CreatePlacedItemSnapshot(
                    item.ItemId,
                    item.ShapeId,
                    item.Rotation,
                    cells));
            }

            BattleSandboxBuildCombatPreviewBuilder.ApplyPreviewEnergyLinks(snapshot);
            return snapshot;
        }

        public void ApplyCategoryFilter(string category)
        {
            string safeCategory = string.IsNullOrWhiteSpace(category) ? "全部" : category;
            itemTrayView?.ApplyFilter(safeCategory);
            placementFeedbackView?.ShowInfo($"已切换到「{safeCategory}」分类。");
        }

        public void CompactTrayWhenReturningToAllCategory()
        {
            if (shapeAwareTrayGrid == null
                || itemTrayView == null
                || !string.IsNullOrEmpty(activeDragItemId))
            {
                return;
            }

            List<PreviewItem> remainingItems = itemById.Values
                .Where(item => item != null && !placedItemIds.Contains(item.ItemId))
                .ToList();
            if (remainingItems.Count == 0)
            {
                return;
            }

            shapeAwareTrayGrid.Clear();
            List<PreviewItem> packedItems = new();
            while (remainingItems.Count > 0)
            {
                int firstEmptySlot = FindFirstEmptyTraySlotIndex();
                if (firstEmptySlot < 0)
                {
                    break;
                }

                ItemShapeCell firstEmptyCell = shapeAwareTrayGrid.SlotIndexToCell(firstEmptySlot);
                int fittingIndex = FindItemIndexThatFitsTrayCell(remainingItems, firstEmptyCell);
                if (fittingIndex >= 0)
                {
                    PreviewItem fittingItem = remainingItems[fittingIndex];
                    if (TryCommitTrayItemAt(fittingItem, firstEmptyCell, out _))
                    {
                        packedItems.Add(fittingItem);
                    }

                    remainingItems.RemoveAt(fittingIndex);
                    continue;
                }

                if (!TryPackFirstRemainingTrayItem(remainingItems, packedItems))
                {
                    break;
                }
            }

            foreach (PreviewItem item in packedItems)
            {
                RefreshTrayPlacement(item);
            }
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

            placementFeedbackView?.ShowInfo($"已查看“{item.DisplayName}”。单击只刷新信息；只可点击信息弹窗里的“旋转”按钮。");
        }

        public void RotateSelectedItem()
        {
            if (selectedItem == null)
            {
                placementFeedbackView?.ShowInfo("请先单击道具打开信息弹窗，再点击弹窗里的“旋转”按钮。");
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

            if (!battlePrepareStateActive)
            {
                placementFeedbackView?.ShowInfo("请先打开整备界面再旋转道具。");
                return;
            }

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
            if (!battlePrepareStateActive)
            {
                placementFeedbackView?.ShowInfo("请先打开整备界面再移动道具。");
                return;
            }

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
            if (!battlePrepareStateActive)
            {
                placementFeedbackView?.ShowInfo("请先打开整备界面再移动棋盘道具。");
                return;
            }

            ItemShapeCell boardCell = slot == null
                ? default
                : ResolveBoardDataCellFromVisualCell(slot.Cell);
            if (slot == null
                || boardReceiver == null
                || !boardReceiver.TryGetItemAtCell(boardCell, out string itemId)
                || !itemById.TryGetValue(itemId, out PreviewItem item))
            {
                return;
            }

            BeginHoldingItem(item, showInfoPanel: false);
            activeDragItemId = item.ItemId;
            itemTrayView?.SetRotateEnabled(item.ItemId, false);
            RefreshItemInfoPanel(item);
            ClearPreviewCells();
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
            placementFeedbackView?.ShowInfo("本包不需要点击预览影确认：合法位置松手已直接放置。");
        }

        public void ResetPreview()
        {
            mobileInput?.Cancel(boardReceiver);
            boardReceiver?.Clear();
            placementSequence = 0;
            placedItemIds.Clear();
            activeDragItemId = string.Empty;
            hasLastPreviewAnchor = false;
            lastPreviewResult = null;
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
            sandboxBattleActive = false;
            battlePrepareStateActive = false;
            battlePrepareContinueStateActive = false;
            manaLoopRuntime?.ResetLoop();
            runtimeLoopRuntime?.ResetLoop();
            RefreshBattlePrepareChrome(snapMotion: true);
            placementFeedbackView?.ShowNeutral("已取消。单击道具查看信息；在信息弹窗点“旋转”调整方向；拖到棋盘松手直接放置。");
        }

        private void Awake()
        {
            EnsureReferences();
            BuildSlotLookup();
            BuildShapeLookup();
            BuildItemLookup();
            InitializePlacementRuntime();
            WireButtons();
            EnsureBattlePrepareChrome();
            EnsureManaLoopRuntime();
            EnsureRuntimeLoopRuntime();
            itemTrayView?.Initialize(this, itemById.Values.ToList(), CategoryLabels);
            ResetPreview();
        }

        private void Update()
        {
            UpdateBattlePrepareMotion();
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
            EnsureRuntimePlacementFeedback();

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

            EnsureEnemyCombatFeedbackVisibilityReferences();
            EnsureSelectedItemInfoCloseButton();
        }

        private void EnsureRuntimePlacementFeedback()
        {
            if (placementFeedbackView != null)
            {
                return;
            }

            RectTransform parent = FindRectTransform("BattleLikePreviewArea");
            if (parent == null)
            {
                parent = boardGridPreview == null ? null : boardGridPreview.parent as RectTransform;
            }

            if (parent == null)
            {
                return;
            }

            GameObject feedbackObject = new(
                "PlacementFeedback_Runtime",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(BuildPlacementFeedbackView));
            feedbackObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            feedbackObject.transform.SetParent(parent, false);

            RectTransform rect = feedbackObject.GetComponent<RectTransform>();
            SetRuntimeAnchors(rect, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.10f));

            Image background = feedbackObject.GetComponent<Image>();
            background.color = new Color(0.18f, 0.14f, 0.09f, 0.96f);
            background.raycastTarget = false;

            Text text = CreateRuntimeText(
                "PlacementFeedbackText",
                feedbackObject.transform,
                "单击道具查看信息；合法松手直接放置，非法返回托盘。",
                16,
                FontStyle.Normal,
                TextAnchor.MiddleCenter);
            SetRuntimeAnchors(text.rectTransform, Vector2.zero, Vector2.one);

            placementFeedbackView = feedbackObject.GetComponent<BuildPlacementFeedbackView>();
            placementFeedbackView.Bind(text, background);
            placementFeedbackView.ShowNeutral(text.text);
        }

        private void BuildSlotLookup()
        {
            boardSlotByCell.Clear();
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                if (slot != null)
                {
                    slot.SetController(this);
                    boardSlotByCell[ResolveBoardDataCellFromVisualCell(slot.Cell)] = slot;
                }
            }
        }

        private ItemShapeCell ResolveBoardDataCellFromVisualCell(ItemShapeCell visualCell)
        {
            return BoardVisualRowsAreTopDown()
                ? visualCell
                : new ItemShapeCell(visualCell.x, BoardRows - 1 - visualCell.y);
        }

        private bool BoardVisualRowsAreTopDown()
        {
            GridLayoutGroup boardGrid = ResolveBoardGridLayoutGroup();
            return boardGrid != null
                && (boardGrid.startCorner == GridLayoutGroup.Corner.UpperLeft
                    || boardGrid.startCorner == GridLayoutGroup.Corner.UpperRight);
        }

        private GridLayoutGroup ResolveBoardGridLayoutGroup()
        {
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                if (slot != null && slot.transform.parent != null)
                {
                    GridLayoutGroup parentGrid = slot.transform.parent.GetComponent<GridLayoutGroup>();
                    if (parentGrid != null)
                    {
                        return parentGrid;
                    }
                }
            }

            return boardGridPreview == null
                ? null
                : boardGridPreview.GetComponentInChildren<GridLayoutGroup>(true);
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
                BoardRows,
                boardSlotByCell);

            foreach (PreviewItem item in itemById.Values)
            {
                shapeAwareTrayGrid.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out _);
            }
        }

        private void EnsureManaLoopRuntime()
        {
            if (manaLoopRuntime == null)
            {
                manaLoopRuntime = GetComponent<BattleSandboxManaLoopRuntime>();
            }

            if (manaLoopRuntime == null)
            {
                manaLoopRuntime = gameObject.AddComponent<BattleSandboxManaLoopRuntime>();
                manaLoopRuntime.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            manaLoopRuntime.Bind(this);
        }

        private void EnsureRuntimeLoopRuntime()
        {
            if (runtimeLoopRuntime == null)
            {
                runtimeLoopRuntime = GetComponent<BattleSandboxRuntimeLoopRuntime>();
            }

            if (runtimeLoopRuntime == null)
            {
                runtimeLoopRuntime = gameObject.AddComponent<BattleSandboxRuntimeLoopRuntime>();
                runtimeLoopRuntime.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            EnsureEnemyCombatFeedbackVisibilityReferences();
            runtimeLoopRuntime.Bind(this, manaLoopRuntime, enemyCombatFeedbackController);
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

        private void EnsureBattlePrepareChrome()
        {
            if (battlePrepareMotionRoot == null)
            {
                battlePrepareMotionRoot = FindRectTransform("BattleLikePreviewArea");
            }

            RectTransform safeAreaRoot = FindRectTransform("SafeAreaRoot");
            if (safeAreaRoot == null && battlePrepareMotionRoot != null)
            {
                safeAreaRoot = battlePrepareMotionRoot.parent as RectTransform;
            }

            EnsureBattlePrepareOverlay(safeAreaRoot);
            EnsureBattlePrepareActionBar(safeAreaRoot);
            EnsureBattlePrepareTrayCanvasGroup();
            EnsureItemTrayLockedOverlay();
            CaptureBattlePreparePositions();
            EnsureDevChapterSelectorBinding();
            WireBattlePrepareButtons();
            RefreshBattlePrepareChrome(snapMotion: true);
        }

        private void EnsureBattlePrepareOverlay(RectTransform parent)
        {
            if (battlePrepareDarkOverlay == null)
            {
                RectTransform existing = FindRectTransform(BattlePrepareOverlayName);
                battlePrepareDarkOverlay = existing == null ? null : existing.GetComponent<Image>();
            }

            if (battlePrepareDarkOverlay != null || parent == null)
            {
                return;
            }

            GameObject overlayObject = new(BattlePrepareOverlayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            overlayObject.transform.SetParent(parent, false);
            RectTransform rect = overlayObject.GetComponent<RectTransform>();
            SetRuntimeAnchors(rect, Vector2.zero, Vector2.one);

            battlePrepareDarkOverlay = overlayObject.GetComponent<Image>();
            battlePrepareDarkOverlay.color = new Color(0f, 0f, 0f, 0.42f);
            battlePrepareDarkOverlay.raycastTarget = false;
        }

        private void EnsureBattlePrepareActionBar(RectTransform parent)
        {
            if (battlePrepareActionBar == null)
            {
                battlePrepareActionBar = FindRectTransform(BattlePrepareActionBarName);
            }

            if (battlePrepareActionBar == null && parent != null)
            {
                GameObject barObject = new(BattlePrepareActionBarName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(GridLayoutGroup));
                barObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                barObject.transform.SetParent(parent, false);

                battlePrepareActionBar = barObject.GetComponent<RectTransform>();
                battlePrepareActionBar.anchorMin = new Vector2(0.5f, 0f);
                battlePrepareActionBar.anchorMax = new Vector2(0.5f, 0f);
                battlePrepareActionBar.pivot = new Vector2(0.5f, 0f);
                battlePrepareActionBar.sizeDelta = new Vector2(800f, 92f);
                battlePrepareActionBar.anchoredPosition = new Vector2(0f, 24f);
                battlePrepareActionBar.localScale = Vector3.one;

                Image image = barObject.GetComponent<Image>();
                image.color = new Color(0.10f, 0.11f, 0.105f, 0.96f);
                image.raycastTarget = true;

                GridLayoutGroup grid = barObject.GetComponent<GridLayoutGroup>();
                grid.padding = new RectOffset(12, 12, 7, 7);
                grid.spacing = new Vector2(16f, 0f);
                grid.cellSize = new Vector2(246f, 78f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 3;

                battlePrepareBackButton = CreateBattlePrepareButton(
                    "V04BattlePrepareBackButton",
                    battlePrepareActionBar,
                    "\u56de\u9996\u9875",
                    new Color(0.22f, 0.28f, 0.32f, 1f),
                    out _);
                battlePrepareStateButton = CreateBattlePrepareButton(
                    "V04BattlePrepareStateButton",
                    battlePrepareActionBar,
                    "\u7ee7\u7eed\u6218\u6597",
                    new Color(0.28f, 0.31f, 0.34f, 1f),
                    out battlePrepareStateButtonText);
                battlePrepareToggleButton = CreateBattlePrepareButton(
                    "V04BattlePrepareToggleButton",
                    battlePrepareActionBar,
                    "\u6574\u5907",
                    new Color(0.50f, 0.32f, 0.16f, 1f),
                    out battlePrepareToggleButtonText);
            }

            if (battlePrepareActionBar == null)
            {
                return;
            }

            if (battlePrepareBackButton == null)
            {
                battlePrepareBackButton = battlePrepareActionBar.Find("V04BattlePrepareBackButton")?.GetComponent<Button>();
            }

            if (battlePrepareStateButton == null)
            {
                battlePrepareStateButton = battlePrepareActionBar.Find("V04BattlePrepareStateButton")?.GetComponent<Button>();
            }

            if (battlePrepareToggleButton == null)
            {
                battlePrepareToggleButton = battlePrepareActionBar.Find("V04BattlePrepareToggleButton")?.GetComponent<Button>();
            }

            if (battlePrepareStateButtonText == null && battlePrepareStateButton != null)
            {
                battlePrepareStateButtonText = battlePrepareStateButton.GetComponentInChildren<Text>(true);
            }

            if (battlePrepareToggleButtonText == null && battlePrepareToggleButton != null)
            {
                battlePrepareToggleButtonText = battlePrepareToggleButton.GetComponentInChildren<Text>(true);
            }

        }

        private void EnsureBattlePrepareTrayCanvasGroup()
        {
            if (itemTrayView == null)
            {
                return;
            }

            itemTrayCanvasGroup = itemTrayView.GetComponent<CanvasGroup>();
            if (itemTrayCanvasGroup == null)
            {
                itemTrayCanvasGroup = itemTrayView.gameObject.AddComponent<CanvasGroup>();
                itemTrayCanvasGroup.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }
        }

        private void EnsureItemTrayLockedOverlay()
        {
            if (itemTrayView == null)
            {
                return;
            }

            RectTransform trayRoot = itemTrayView.transform as RectTransform;
            if (trayRoot == null)
            {
                return;
            }

            if (itemTrayLockedOverlay == null)
            {
                RectTransform existing = trayRoot.Find(ItemTrayLockedOverlayName) as RectTransform;
                itemTrayLockedOverlay = existing == null ? null : existing.GetComponent<Image>();
            }

            if (itemTrayLockedOverlay == null)
            {
                GameObject overlayObject = new(
                    ItemTrayLockedOverlayName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(CanvasGroup));
                overlayObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                overlayObject.transform.SetParent(trayRoot, false);

                RectTransform rect = overlayObject.GetComponent<RectTransform>();
                SetRuntimeAnchors(rect, Vector2.zero, Vector2.one);

                itemTrayLockedOverlay = overlayObject.GetComponent<Image>();
                itemTrayLockedOverlay.color = new Color(0f, 0f, 0f, 0.52f);
                itemTrayLockedOverlay.raycastTarget = true;

                Text hint = CreateRuntimeText(
                    "Text",
                    overlayObject.transform,
                    "\u6218\u6597\u4e2d\u4e0d\u53ef\u8c03\u6574\n\u70b9\u51fb\u300c\u6574\u5907\u300d\u540e\u89e3\u9501\u9053\u5177\u680f",
                    24,
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter);
                hint.color = new Color(0.92f, 0.96f, 1f, 1f);
                SetRuntimeAnchors(hint.rectTransform, Vector2.zero, Vector2.one);
            }

            itemTrayLockedOverlayCanvasGroup = itemTrayLockedOverlay.GetComponent<CanvasGroup>();
            if (itemTrayLockedOverlayCanvasGroup == null)
            {
                itemTrayLockedOverlayCanvasGroup = itemTrayLockedOverlay.gameObject.AddComponent<CanvasGroup>();
                itemTrayLockedOverlayCanvasGroup.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            itemTrayLockedOverlayCanvasGroup.ignoreParentGroups = true;
            itemTrayLockedOverlayCanvasGroup.interactable = false;
            itemTrayLockedOverlay.gameObject.SetActive(false);
            itemTrayLockedOverlay.rectTransform.SetAsLastSibling();
        }

        private void CaptureBattlePreparePositions()
        {
            if (hasBattlePreparePositions || battlePrepareMotionRoot == null)
            {
                return;
            }

            battlePrepareOpenPosition = battlePrepareMotionRoot.anchoredPosition;
            battlePrepareNormalPosition = battlePrepareOpenPosition + new Vector2(0f, -BattlePreparePullOffset);
            hasBattlePreparePositions = true;
        }

        private void WireBattlePrepareButtons()
        {
            if (battlePrepareBackButton != null)
            {
                battlePrepareBackButton.onClick.RemoveListener(HandleBattlePrepareBackClicked);
                battlePrepareBackButton.onClick.AddListener(HandleBattlePrepareBackClicked);
            }

            if (battlePrepareStateButton != null)
            {
                battlePrepareStateButton.onClick.RemoveListener(HandleBattlePrepareStateClicked);
                battlePrepareStateButton.onClick.AddListener(HandleBattlePrepareStateClicked);
            }

            if (battlePrepareToggleButton != null)
            {
                battlePrepareToggleButton.onClick.RemoveListener(HandleBattlePrepareToggleClicked);
                battlePrepareToggleButton.onClick.AddListener(HandleBattlePrepareToggleClicked);
            }

            if (devChapterDropdownButton != null)
            {
                devChapterDropdownButton.onClick.RemoveListener(HandleDevChapterSelectorClicked);
                devChapterDropdownButton.onClick.AddListener(HandleDevChapterSelectorClicked);
            }
        }

        private void HandleBattlePrepareBackClicked()
        {
            battlePrepareStateActive = false;
            battlePrepareContinueStateActive = false;
            sandboxBattleActive = false;
            RefreshBattlePrepareChrome(snapMotion: false);
            placementFeedbackView?.ShowInfo("V0.4 沙盒：未连接主页流程，战斗整备界面已收起。");
        }

        private void HandleBattlePrepareStateClicked()
        {
            if (battlePrepareContinueStateActive)
            {
                return;
            }

            if (sandboxBattleActive && !battlePrepareStateActive)
            {
                if (runtimeLoopRuntime != null && runtimeLoopRuntime.HasSandboxResult)
                {
                    runtimeLoopRuntime.RestartLoop();
                    RefreshBattlePrepareChrome(snapMotion: false);
                    placementFeedbackView?.ShowValid("V0.4 沙盒：已重开当前测试敌人。");
                    return;
                }

                string enemyLabel = runtimeLoopRuntime == null
                    ? string.Empty
                    : runtimeLoopRuntime.SelectNextDevEnemy();
                RefreshBattlePrepareChrome(snapMotion: false);
                placementFeedbackView?.ShowInfo(string.IsNullOrWhiteSpace(enemyLabel)
                    ? "V0.4 沙盒：已切换测试敌人。"
                    : $"V0.4 沙盒：已切换到 {enemyLabel}。");
                return;
            }

            if (battlePrepareStateActive)
            {
                battlePrepareStateActive = false;
                battlePrepareContinueStateActive = true;
                sandboxBattleActive = true;
                RefreshBattlePrepareChrome(snapMotion: false);
                placementFeedbackView?.ShowValid("V0.4 沙盒：当前摆放已读取，正在收起整备界面。");
                return;
            }

            if (!sandboxBattleActive)
            {
                sandboxBattleActive = true;
                battlePrepareStateActive = false;
                battlePrepareContinueStateActive = true;
                RefreshBattlePrepareChrome(snapMotion: false);
                placementFeedbackView?.ShowInfo("V0.4 沙盒战斗反馈预览已启动；未连接正式战斗。");
                return;
            }

            placementFeedbackView?.ShowInfo("V0.4 沙盒战斗预览运行中；点击整备可重新打开摆放界面。");
        }

        private void HandleBattlePrepareToggleClicked()
        {
            if (battlePrepareStateActive || battlePrepareContinueStateActive)
            {
                return;
            }

            battlePrepareStateActive = true;
            RefreshBattlePrepareChrome(snapMotion: false);
            placementFeedbackView?.ShowInfo("V0.4 沙盒整备界面已打开；可在道具栏与棋盘间拖动道具。");
        }

        public void RefreshSandboxBattleActionChrome()
        {
            RefreshBattlePrepareChrome(snapMotion: false);
        }

        private void UpdateBattlePrepareMotion()
        {
            if (!hasBattlePreparePositions || battlePrepareMotionRoot == null)
            {
                return;
            }

            Vector2 target = battlePrepareStateActive ? battlePrepareOpenPosition : battlePrepareNormalPosition;
            Vector2 current = battlePrepareMotionRoot.anchoredPosition;
            Vector2 next = Vector2.Lerp(current, target, Mathf.Clamp01(Time.unscaledDeltaTime * BattlePrepareMoveSpeed));
            if ((next - target).sqrMagnitude <= 1f)
            {
                next = target;
            }

            battlePrepareMotionRoot.anchoredPosition = next;
            if (battlePrepareContinueStateActive && !battlePrepareStateActive && (next - battlePrepareNormalPosition).sqrMagnitude <= 1f)
            {
                battlePrepareContinueStateActive = false;
                RefreshBattlePrepareChrome(snapMotion: false);
            }
        }

        private void RefreshBattlePrepareChrome(bool snapMotion)
        {
            CaptureBattlePreparePositions();

            if (hasBattlePreparePositions && battlePrepareMotionRoot != null && snapMotion)
            {
                battlePrepareMotionRoot.anchoredPosition = battlePrepareStateActive
                    ? battlePrepareOpenPosition
                    : battlePrepareNormalPosition;
            }

            bool prepareOrContinue = battlePrepareStateActive || battlePrepareContinueStateActive;
            bool trayLockedByBattle = !battlePrepareStateActive
                && (sandboxBattleActive || battlePrepareContinueStateActive);
            if (battlePrepareDarkOverlay != null)
            {
                battlePrepareDarkOverlay.gameObject.SetActive(prepareOrContinue);
                battlePrepareDarkOverlay.color = new Color(0f, 0f, 0f, battlePrepareStateActive ? 0.42f : 0.24f);
            }

            if (itemTrayLockedOverlay != null)
            {
                itemTrayLockedOverlay.gameObject.SetActive(trayLockedByBattle);
                itemTrayLockedOverlay.color = new Color(0f, 0f, 0f, trayLockedByBattle ? 0.52f : 0f);
                itemTrayLockedOverlay.rectTransform.SetAsLastSibling();
            }

            if (itemTrayLockedOverlayCanvasGroup != null)
            {
                itemTrayLockedOverlayCanvasGroup.alpha = trayLockedByBattle ? 1f : 0f;
                itemTrayLockedOverlayCanvasGroup.blocksRaycasts = trayLockedByBattle;
            }

            if (itemTrayCanvasGroup != null)
            {
                itemTrayCanvasGroup.alpha = battlePrepareStateActive || trayLockedByBattle ? 1f : 0.68f;
                itemTrayCanvasGroup.interactable = battlePrepareStateActive;
                itemTrayCanvasGroup.blocksRaycasts = battlePrepareStateActive || trayLockedByBattle;
            }

            if (battlePrepareStateButtonText != null)
            {
                battlePrepareStateButtonText.text = sandboxBattleActive && !battlePrepareStateActive && !battlePrepareContinueStateActive
                    ? runtimeLoopRuntime != null && runtimeLoopRuntime.HasSandboxResult
                        ? "\u91cd\u5f00\u672c\u573a"
                        : "\u5207\u6362\u654c\u4eba"
                    : "\u7ee7\u7eed\u6218\u6597";
            }

            if (battlePrepareToggleButtonText != null)
            {
                battlePrepareToggleButtonText.text = battlePrepareStateActive || battlePrepareContinueStateActive
                    ? "\u6574\u5907\u4e2d"
                    : "\u6574\u5907";
            }

            RefreshDevChapterSelectorLabel();

            if (battlePrepareStateButton != null)
            {
                battlePrepareStateButton.interactable = !battlePrepareContinueStateActive;
            }

            if (battlePrepareToggleButton != null)
            {
                battlePrepareToggleButton.interactable = !battlePrepareStateActive && !battlePrepareContinueStateActive;
            }

            if (devChapterDropdownButton != null)
            {
                devChapterDropdownButton.interactable = !battlePrepareContinueStateActive;
            }

            RefreshEnemyCombatFeedbackVisibility();

            if (battlePrepareActionBar != null)
            {
                battlePrepareActionBar.gameObject.SetActive(true);
            }
        }

        private void EnsureDevChapterSelectorBinding()
        {
            if (devChapterDropdownSlot == null)
            {
                devChapterDropdownSlot = FindRectTransform(DevChapterDropdownSlotName);
            }

            if (devChapterDropdownSlot == null)
            {
                return;
            }

            if (devChapterDropdownButton == null)
            {
                devChapterDropdownButton = devChapterDropdownSlot.GetComponent<Button>();
            }

            Image slotImage = devChapterDropdownSlot.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.raycastTarget = true;
            }

            if (devChapterDropdownButton == null)
            {
                devChapterDropdownButton = devChapterDropdownSlot.gameObject.AddComponent<Button>();
                devChapterDropdownButton.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                Color baseColor = slotImage == null ? new Color(0.22f, 0.235f, 0.22f, 0.92f) : slotImage.color;
                ColorBlock colors = devChapterDropdownButton.colors;
                colors.normalColor = baseColor;
                colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.12f);
                colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.22f);
                colors.selectedColor = colors.highlightedColor;
                colors.disabledColor = new Color(baseColor.r * 0.55f, baseColor.g * 0.55f, baseColor.b * 0.55f, 0.72f);
                devChapterDropdownButton.colors = colors;
            }

            if (devChapterDropdownLabelText == null)
            {
                devChapterDropdownLabelText = devChapterDropdownSlot.GetComponentInChildren<Text>(true);
            }

            WireBattlePrepareButtons();
            RefreshDevChapterSelectorLabel();
        }

        private void HandleDevChapterSelectorClicked()
        {
            EnsureRuntimeLoopRuntime();
            string targetLabel = runtimeLoopRuntime == null
                ? string.Empty
                : runtimeLoopRuntime.SelectNextDevChapter();
            RefreshBattlePrepareChrome(snapMotion: false);
            placementFeedbackView?.ShowInfo(string.IsNullOrWhiteSpace(targetLabel)
                ? "V0.4 沙盒：已切换验证关卡。"
                : $"V0.4 沙盒：已切换到 {targetLabel}。");
        }

        private void RefreshDevChapterSelectorLabel()
        {
            EnsureRuntimeLoopRuntime();
            if (devChapterDropdownLabelText == null)
            {
                return;
            }

            string chapter = runtimeLoopRuntime == null
                ? "3-10"
                : runtimeLoopRuntime.CurrentDevChapterLabel;
            string target = runtimeLoopRuntime == null
                ? string.Empty
                : runtimeLoopRuntime.CurrentDevEnemyLabel;
            devChapterDropdownLabelText.text = string.IsNullOrWhiteSpace(target)
                ? $"\u9a8c\u8bc1\u5173\u5361 {chapter}"
                : $"\u9a8c\u8bc1\u5173\u5361 {target}";
        }

        private void EnsureEnemyCombatFeedbackVisibilityReferences()
        {
            if (enemyCombatFeedbackPanel == null)
            {
                enemyCombatFeedbackPanel = FindRectTransform(EnemyCombatFeedbackPanelName);
            }

            if (enemyCombatFeedbackFloatingRoot == null)
            {
                enemyCombatFeedbackFloatingRoot = FindRectTransform(EnemyCombatFeedbackFloatingRootName);
            }

            if (enemyCombatFeedbackController == null)
            {
                enemyCombatFeedbackController = FindObjectOfType<BattleSandboxEnemyCombatFeedbackController>(true);
            }
        }

        private void RefreshEnemyCombatFeedbackVisibility()
        {
            EnsureEnemyCombatFeedbackVisibilityReferences();

            bool feedbackVisible = sandboxBattleActive && !battlePrepareStateActive;
            SetGameObjectActive(enemyCombatFeedbackPanel, feedbackVisible);
            SetGameObjectActive(enemyCombatFeedbackFloatingRoot, feedbackVisible);
            if (feedbackVisible && !enemyCombatFeedbackVisibleLastFrame)
            {
                enemyCombatFeedbackController?.RestartBattleModePreview();
            }

            runtimeLoopRuntime?.SetBattleModeVisible(feedbackVisible);
            enemyCombatFeedbackVisibleLastFrame = feedbackVisible;
        }

        private static void SetGameObjectActive(Component component, bool active)
        {
            if (component != null && component.gameObject.activeSelf != active)
            {
                component.gameObject.SetActive(active);
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
            itemTrayView?.TryAutoScrollDuringDrag(
                eventData.position,
                eventData.pressEventCamera,
                Time.unscaledDeltaTime);

            if (TryPreviewTrayDrag(eventData, out ShapePlacementResult trayResult))
            {
                lastPreviewResult = trayResult;
                hasLastPreviewAnchor = trayResult != null;
                if (trayResult != null)
                {
                    lastPreviewAnchor = trayResult.AnchorCell;
                }

                ClearPreviewCells();
                ShowDragGhost(selectedItem, eventData, "拖动中", trayResult, ShapePlacementSource.Tray);
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
            ShowDragGhost(selectedItem, eventData, "拖动中", result, ShapePlacementSource.Board);
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

        private int FindFirstEmptyTraySlotIndex()
        {
            if (shapeAwareTrayGrid == null)
            {
                return -1;
            }

            for (int slotIndex = 0; slotIndex < shapeAwareTrayGrid.SlotCount; slotIndex++)
            {
                ItemShapeCell cell = shapeAwareTrayGrid.SlotIndexToCell(slotIndex);
                if (!shapeAwareTrayGrid.OccupiedCells.ContainsKey(cell))
                {
                    return slotIndex;
                }
            }

            return -1;
        }

        private int FindItemIndexThatFitsTrayCell(
            IReadOnlyList<PreviewItem> items,
            ItemShapeCell anchorCell)
        {
            if (shapeAwareTrayGrid == null)
            {
                return -1;
            }

            for (int i = 0; i < (items?.Count ?? 0); i++)
            {
                PreviewItem item = items[i];
                if (item == null)
                {
                    continue;
                }

                ShapePlacementResult result = shapeAwareTrayGrid.CanPlace(
                    BuildPayload(item, ShapePlacementSource.Tray),
                    anchorCell);
                if (result != null && result.IsValid)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool TryPackFirstRemainingTrayItem(
            List<PreviewItem> remainingItems,
            List<PreviewItem> packedItems)
        {
            if (shapeAwareTrayGrid == null || remainingItems == null)
            {
                return false;
            }

            for (int i = 0; i < remainingItems.Count; i++)
            {
                PreviewItem item = remainingItems[i];
                if (item == null)
                {
                    continue;
                }

                if (shapeAwareTrayGrid.TryPack(
                        BuildPayload(item, ShapePlacementSource.Tray),
                        out ShapePlacementResult result)
                    && result != null
                    && result.IsValid)
                {
                    packedItems?.Add(item);
                    remainingItems.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private bool TryCommitTrayItemAt(
            PreviewItem item,
            ItemShapeCell anchorCell,
            out ShapePlacementResult result)
        {
            result = null;
            if (shapeAwareTrayGrid == null || item == null)
            {
                return false;
            }

            ShapePlacementSession traySession = new();
            traySession.Begin(BuildPayload(item, ShapePlacementSource.Tray), trayAnchorCell: anchorCell);
            result = traySession.Commit(shapeAwareTrayGrid);
            return result != null && result.IsValid;
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

            Color itemColor = ResolvePreviewItemColor(result.ItemId);
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
                        slot.SetPreview(result.IsValid, itemColor);
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

                if (itemById.TryGetValue(occupied.Value, out PreviewItem item))
                {
                    slot.SetPlaced(item.DisplayName, item.CardColor);
                }
                else
                {
                    slot.SetPlaced(occupied.Value);
                }
            }
        }

        private Color ResolvePreviewItemColor(string itemId)
        {
            return itemById.TryGetValue(itemId ?? string.Empty, out PreviewItem item)
                ? item.CardColor
                : new Color(0.44f, 0.35f, 0.18f, 1f);
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
                selectedItemInfoBody.text = "单击道具查看信息；只在信息弹窗点“旋转”按钮；拖到棋盘松手直接放置。";
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
                $"沙盒属性：{BuildSandboxItemStatCatalog.FormatPlayerFacing(item.ItemStat)}\n" +
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

            return battlePrepareStateActive
                && string.IsNullOrEmpty(activeDragItemId)
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

        private void ShowDragGhost(
            PreviewItem item,
            PointerEventData eventData,
            string state,
            ShapePlacementResult result = null,
            ShapePlacementSource source = ShapePlacementSource.Unknown)
        {
            if (dragGhostRoot == null || item == null || eventData == null)
            {
                return;
            }

            CacheDragGhostDefaults();
            dragGhostRoot.gameObject.SetActive(true);
            dragGhostRoot.position = eventData.position
                + new Vector2(0f, MobileShapePlacementInputSettings.DefaultFingerGhostOffsetPixels);
            if (dragGhostText != null)
            {
                dragGhostText.text = $"{item.DisplayName}\n{state}";
                dragGhostText.transform.SetAsLastSibling();
            }

            if (TryResolveDragGhostLayout(item, result, source, out ShapeCellVisualLayout layout))
            {
                ApplyDragGhostLayout(layout, item, result);
            }
            else
            {
                RestoreDragGhostBoxVisual();
            }
        }

        private void HideDragGhost()
        {
            if (dragGhostRoot != null)
            {
                dragGhostRoot.gameObject.SetActive(false);
            }

            if (dragGhostCellLayer != null)
            {
                dragGhostCellLayer.gameObject.SetActive(false);
            }
        }

        private bool TryResolveDragGhostLayout(
            PreviewItem item,
            ShapePlacementResult result,
            ShapePlacementSource source,
            out ShapeCellVisualLayout layout)
        {
            layout = null;
            if (result != null && result.OccupiedCells.Count > 0)
            {
                if (source == ShapePlacementSource.Board
                    && TryBuildBoardCellVisualLayout(result.OccupiedCells, out layout))
                {
                    return true;
                }

                if (source == ShapePlacementSource.Tray
                    && itemTrayView != null
                    && itemTrayView.TryBuildCellVisualLayout(result.OccupiedCells, out layout))
                {
                    return true;
                }
            }

            if (item != null
                && itemTrayView != null
                && TryGetTrayPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement placement)
                && placement != null
                && itemTrayView.TryBuildCellVisualLayout(placement.OccupiedCells, out layout))
            {
                return true;
            }

            ShapeItemPayload payload = BuildPayload(item, ShapePlacementSource.Board);
            return payload.IsValid
                && TryBuildBoardCellVisualLayout(payload.BuildNormalizedOffsets(), out layout);
        }

        private bool TryBuildBoardCellVisualLayout(
            IReadOnlyList<ItemShapeCell> occupiedCells,
            out ShapeCellVisualLayout layout)
        {
            return ShapeGridCellVisualLayoutUtility.TryBuildFromSlots(
                occupiedCells,
                ResolveBoardSlotRect,
                boardGridPreview,
                out layout);
        }

        private RectTransform ResolveBoardSlotRect(ItemShapeCell dataCell)
        {
            if (!boardSlotByCell.TryGetValue(dataCell, out BuildGridPreviewSlotView slot)
                || slot == null)
            {
                return null;
            }

            return slot.transform as RectTransform;
        }

        private void ApplyDragGhostLayout(
            ShapeCellVisualLayout layout,
            PreviewItem item,
            ShapePlacementResult result)
        {
            if (layout == null || dragGhostRoot == null)
            {
                return;
            }

            dragGhostRoot.sizeDelta = layout.SizeDelta;
            dragGhostRoot.localScale = Vector3.one;
            if (dragGhostBackgroundImage != null)
            {
                dragGhostBackgroundImage.color = Color.clear;
            }

            RectTransform layer = EnsureDragGhostCellLayer();
            if (layer == null)
            {
                return;
            }

            layer.gameObject.SetActive(true);
            layer.anchorMin = new Vector2(0f, 1f);
            layer.anchorMax = new Vector2(0f, 1f);
            layer.pivot = new Vector2(0f, 1f);
            layer.anchoredPosition = Vector2.zero;
            layer.sizeDelta = layout.SizeDelta;
            layer.localScale = Vector3.one;
            layer.SetAsFirstSibling();

            IReadOnlyList<ShapeCellVisualRect> cellRects = layout.CellRects;
            while (layer.childCount < cellRects.Count)
            {
                GameObject cellObject = new(
                    $"{DragGhostCellNamePrefix}{layer.childCount:00}",
                    typeof(RectTransform),
                    typeof(Image));
                cellObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                cellObject.transform.SetParent(layer, false);
            }

            Color color = ResolveDragGhostCellColor(item, result);
            for (int i = 0; i < layer.childCount; i++)
            {
                Transform child = layer.GetChild(i);
                bool active = i < cellRects.Count;
                child.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                RectTransform cellRect = child as RectTransform;
                if (cellRect != null)
                {
                    ShapeCellVisualRect visualRect = cellRects[i];
                    cellRect.anchorMin = new Vector2(0f, 1f);
                    cellRect.anchorMax = new Vector2(0f, 1f);
                    cellRect.pivot = new Vector2(0f, 1f);
                    cellRect.anchoredPosition = visualRect.AnchoredPosition;
                    cellRect.sizeDelta = visualRect.SizeDelta;
                    cellRect.localScale = Vector3.one;
                }

                Image image = child.GetComponent<Image>();
                if (image != null)
                {
                    image.color = color;
                    image.raycastTarget = false;
                }
            }
        }

        private RectTransform EnsureDragGhostCellLayer()
        {
            if (dragGhostRoot == null)
            {
                return null;
            }

            if (dragGhostCellLayer != null)
            {
                return dragGhostCellLayer;
            }

            dragGhostCellLayer = dragGhostRoot.Find(DragGhostCellLayerName) as RectTransform;
            if (dragGhostCellLayer == null)
            {
                GameObject layerObject = new(DragGhostCellLayerName, typeof(RectTransform));
                layerObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                layerObject.transform.SetParent(dragGhostRoot, false);
                dragGhostCellLayer = layerObject.GetComponent<RectTransform>();
            }

            return dragGhostCellLayer;
        }

        private Color ResolveDragGhostCellColor(PreviewItem item, ShapePlacementResult result)
        {
            Color color = result != null && !result.IsValid
                ? new Color(0.62f, 0.20f, 0.16f, 0.76f)
                : item?.CardColor ?? new Color(0.44f, 0.35f, 0.18f, 1f);
            color.a = Mathf.Min(color.a, 0.82f);
            return color;
        }

        private void RestoreDragGhostBoxVisual()
        {
            if (dragGhostRoot == null)
            {
                return;
            }

            if (hasDragGhostDefaults)
            {
                dragGhostRoot.sizeDelta = dragGhostDefaultSize;
            }

            if (dragGhostBackgroundImage != null && hasDragGhostDefaults)
            {
                dragGhostBackgroundImage.color = dragGhostDefaultBackgroundColor;
            }

            if (dragGhostCellLayer != null)
            {
                dragGhostCellLayer.gameObject.SetActive(false);
            }
        }

        private void CacheDragGhostDefaults()
        {
            if (hasDragGhostDefaults || dragGhostRoot == null)
            {
                return;
            }

            dragGhostDefaultSize = dragGhostRoot.sizeDelta;
            dragGhostBackgroundImage = dragGhostRoot.GetComponent<Image>();
            dragGhostDefaultBackgroundColor = dragGhostBackgroundImage == null
                ? Color.clear
                : dragGhostBackgroundImage.color;
            hasDragGhostDefaults = true;
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

        private static Button CreateBattlePrepareButton(
            string name,
            Transform parent,
            string label,
            Color color,
            out Text labelText)
        {
            GameObject buttonObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;

            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, 0.72f);
            button.colors = colors;

            labelText = CreateRuntimeText("Label", buttonObject.transform, label, 20, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetRuntimeAnchors(labelText.rectTransform, Vector2.zero, Vector2.one);
            return button;
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
                Color cardColor,
                BuildSandboxItemStat itemStat = null,
                IReadOnlyList<string> categoryIds = null)
            {
                ItemId = itemId ?? string.Empty;
                DisplayName = displayName ?? string.Empty;
                Category = category ?? string.Empty;
                ShapeId = shapeId ?? string.Empty;
                ShapeDisplayName = shapeDisplayName ?? string.Empty;
                CardColor = cardColor;
                Rotation = ItemShapeRotation.Rotation0;
                ItemStat = BuildSandboxItemStatCatalog.ResolveFrom(itemStat, ItemId);
                BuildSandboxItemIdentityFamilyRecord identity =
                    BuildSandboxItemIdentityFamilyCatalog.Resolve(ItemId);
                ItemFamily = identity.ItemFamily;
                BaseItemId = identity.BaseItemId;
                Tier = identity.Tier;
                RelationshipToBase = identity.RelationshipToBase;
                CategoryIds = NormalizeCategoryIds(categoryIds, Category, Tier);
            }

            public string ItemId { get; }
            public string DisplayName { get; }
            public string Category { get; }
            public string ShapeId { get; }
            public string ShapeDisplayName { get; }
            public string ItemFamily { get; }
            public string BaseItemId { get; }
            public string Tier { get; }
            public string RelationshipToBase { get; }
            public IReadOnlyList<string> CategoryIds { get; }
            public Color CardColor { get; }
            public BuildSandboxItemStat ItemStat { get; }
            public ItemShapeRotation Rotation { get; set; }

            public bool MatchesCategory(string categoryOrDisplay)
            {
                string categoryId = BuildSandboxLegacyAndAdvancedItemRosterCatalog.NormalizeCategoryId(categoryOrDisplay);
                return string.Equals(categoryId, BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryAll, StringComparison.Ordinal)
                    || CategoryIds.Any(value => string.Equals(value, categoryId, StringComparison.Ordinal));
            }

            private static IReadOnlyList<string> NormalizeCategoryIds(
                IReadOnlyList<string> categoryIds,
                string categoryDisplayName,
                string tier)
            {
                List<string> normalized = new();
                foreach (string categoryId in categoryIds ?? Array.Empty<string>())
                {
                    AddNormalizedCategory(normalized, categoryId);
                }

                AddNormalizedCategory(normalized, categoryDisplayName);
                AddNormalizedCategory(normalized, tier);
                if (normalized.Count == 0)
                {
                    normalized.Add(BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryTestDevOnly);
                }

                return normalized;
            }

            private static void AddNormalizedCategory(List<string> categories, string value)
            {
                string normalized = BuildSandboxLegacyAndAdvancedItemRosterCatalog.NormalizeCategoryId(value);
                if (string.IsNullOrWhiteSpace(normalized)
                    || string.Equals(normalized, BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryAll, StringComparison.Ordinal)
                    || categories.Contains(normalized, StringComparer.Ordinal))
                {
                    return;
                }

                categories.Add(normalized);
            }
        }

        private sealed class UiBoardShapeGridReceiver : ShapeGridReceiver
        {
            private readonly Dictionary<ItemShapeCell, string> occupiedByItemId = new();
            private readonly Dictionary<ItemShapeCell, RectTransform> slotRectsByCell = new();
            private readonly RectTransform boardRect;

            public UiBoardShapeGridReceiver(
                string receiverId,
                RectTransform boardRect,
                int width,
                int height,
                IReadOnlyDictionary<ItemShapeCell, BuildGridPreviewSlotView> slotsByCell = null)
            {
                ReceiverId = string.IsNullOrWhiteSpace(receiverId) ? "battle_sandbox_board" : receiverId;
                this.boardRect = boardRect;
                Width = width;
                Height = height;
                foreach (KeyValuePair<ItemShapeCell, BuildGridPreviewSlotView> pair
                         in slotsByCell ?? new Dictionary<ItemShapeCell, BuildGridPreviewSlotView>())
                {
                    RectTransform slotRect = pair.Value == null ? null : pair.Value.transform as RectTransform;
                    if (slotRect != null)
                    {
                        slotRectsByCell[pair.Key] = slotRect;
                    }
                }
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
                if (TryScreenPointToAuthoredSlot(screenPoint, eventCamera, out anchorCell))
                {
                    return true;
                }

                if (TryScreenPointToSlotBounds(screenPoint, eventCamera, out anchorCell))
                {
                    return true;
                }

                if (slotRectsByCell.Count > 0)
                {
                    return false;
                }

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

            private bool TryScreenPointToAuthoredSlot(
                Vector2 screenPoint,
                Camera eventCamera,
                out ItemShapeCell anchorCell)
            {
                foreach (KeyValuePair<ItemShapeCell, RectTransform> pair in slotRectsByCell)
                {
                    RectTransform slotRect = pair.Value;
                    if (slotRect == null)
                    {
                        continue;
                    }

                    if (RectTransformUtility.RectangleContainsScreenPoint(slotRect, screenPoint, eventCamera))
                    {
                        anchorCell = pair.Key;
                        return true;
                    }
                }

                anchorCell = default;
                return false;
            }

            private bool TryScreenPointToSlotBounds(
                Vector2 screenPoint,
                Camera eventCamera,
                out ItemShapeCell anchorCell)
            {
                anchorCell = default;
                if (boardRect == null
                    || slotRectsByCell.Count == 0
                    || !TryResolveSlotLocalBounds(out Rect bounds)
                    || bounds.width <= 0f
                    || bounds.height <= 0f
                    || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        boardRect,
                        screenPoint,
                        eventCamera,
                        out Vector2 localPoint))
                {
                    return false;
                }

                float normalizedX = (localPoint.x - bounds.xMin) / bounds.width;
                float normalizedY = (localPoint.y - bounds.yMin) / bounds.height;
                if (normalizedX < 0f || normalizedX >= 1f || normalizedY < 0f || normalizedY >= 1f)
                {
                    return false;
                }

                int x = Mathf.Clamp(Mathf.FloorToInt(normalizedX * Width), 0, Width - 1);
                int y = Mathf.Clamp(Mathf.FloorToInt((1f - normalizedY) * Height), 0, Height - 1);
                anchorCell = new ItemShapeCell(x, y);
                return true;
            }

            private bool TryResolveSlotLocalBounds(out Rect bounds)
            {
                bounds = default;
                if (boardRect == null || slotRectsByCell.Count == 0)
                {
                    return false;
                }

                float minX = float.PositiveInfinity;
                float maxX = float.NegativeInfinity;
                float minY = float.PositiveInfinity;
                float maxY = float.NegativeInfinity;
                Vector3[] worldCorners = new Vector3[4];
                foreach (RectTransform slotRect in slotRectsByCell.Values)
                {
                    if (slotRect == null)
                    {
                        continue;
                    }

                    slotRect.GetWorldCorners(worldCorners);
                    for (int i = 0; i < worldCorners.Length; i++)
                    {
                        Vector3 localCorner = boardRect.InverseTransformPoint(worldCorners[i]);
                        minX = Mathf.Min(minX, localCorner.x);
                        maxX = Mathf.Max(maxX, localCorner.x);
                        minY = Mathf.Min(minY, localCorner.y);
                        maxY = Mathf.Max(maxY, localCorner.y);
                    }
                }

                if (float.IsNaN(minX)
                    || float.IsInfinity(minX)
                    || float.IsNaN(maxX)
                    || float.IsInfinity(maxX)
                    || float.IsNaN(minY)
                    || float.IsInfinity(minY)
                    || float.IsNaN(maxY)
                    || float.IsInfinity(maxY)
                    || maxX <= minX
                    || maxY <= minY)
                {
                    return false;
                }

                bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
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
