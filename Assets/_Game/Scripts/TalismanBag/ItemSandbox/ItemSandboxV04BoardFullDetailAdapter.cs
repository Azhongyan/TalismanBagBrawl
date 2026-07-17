using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Detail.UI;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    [Serializable]
    public sealed class ItemSandboxV04ProtectedGeometryBaseline
    {
        public string objectName;
        public string parentName;
        public bool activeSelf;
        public int siblingIndex;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale;
        public Vector3 localEulerAngles;
    }

    public sealed class ItemSandboxV04TrayEntry
    {
        public ItemSandboxV04TrayEntry(ItemInnerDataDefinition definition)
        {
            Definition = definition;
        }

        public ItemInnerDataDefinition Definition { get; }
        public string itemId => Definition?.itemId ?? string.Empty;
        public bool isJuNian => string.Equals(itemId, "I031", StringComparison.Ordinal);
        public string displayText => Definition == null
            ? string.Empty
            : Definition.itemId + "  " + Definition.displayName + "\n"
                + Definition.shapeId + " · " + Definition.ShapeCells.Count.ToString(CultureInfo.InvariantCulture)
                + "格 · 核" + ItemInnerDataDefinition.FormatCell(Definition.coreCellLocal);
    }

    /// <summary>
    /// V0.4 ItemSandbox-only adapter. It binds the existing copied board/tray/popup nodes and
    /// delegates every placement/build/lighting/awakening decision to the existing workbench session.
    /// It never constructs, reparents, resizes, or reorders UI at runtime.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ItemSandboxV04BoardFullDetailAdapter : MonoBehaviour
    {
        private static readonly string[] RarityKeys = { "white", "green", "blue", "purple", "orange" };
        private const string LightingStatusLitResourcePath = "item/聚念石icon/聚念石icon_点亮";
        private const string LightingStatusUnlitResourcePath = "item/聚念石icon/聚念石icon_未点亮";
        private const string ArrayStatusLitResourcePath = "item/阵脉icon/阵脉icon_点亮";
        private const string ArrayStatusUnlitResourcePath = "item/阵脉icon/阵脉icon_未点亮";

        [Header("Existing providers")]
        [SerializeField] private ItemInnerDataCatalogProvider catalogProvider;
        [SerializeField] private ItemBalanceCandidateDetailSandboxProvider candidateProvider;
        [SerializeField] private ItemDetailPanelView detailPanel;

        [Header("Authored Item Detail Preview")]
        [SerializeField] private bool preserveAuthoredDetailPreviewOnPlay = true;

        [Header("Existing copied UI")]
        [SerializeField] private RectTransform battleLikePreviewArea;
        [SerializeField] private RectTransform popupLayer;
        [SerializeField] private RectTransform feedbackRoot;
        [SerializeField] private ScrollRect trayScrollRect;
        [SerializeField] private GameObject legacyItemCardLayer;
        [SerializeField] private ItemSandboxV04TraySlotView[] traySlots = Array.Empty<ItemSandboxV04TraySlotView>();
        [SerializeField] private ItemSandboxV04BoardCellView[] boardCells = Array.Empty<ItemSandboxV04BoardCellView>();

        [Header("Candidate controls")]
        [SerializeField] private Button[] rarityButtons = Array.Empty<Button>();
        [SerializeField] private Text[] rarityButtonLabels = Array.Empty<Text>();
        [SerializeField] private InputField seedInput;
        [SerializeField] private Button regenerateButton;
        [SerializeField] private Text candidateStatusText;
        [SerializeField] private GameObject candidateControlsRoot;

        [Header("Level, Build and monitors")]
        [SerializeField] private Button levelDownButton;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Text levelText;
        [SerializeField] private Text buildSummaryText;
        [SerializeField] private Button[] mainBuildButtons = Array.Empty<Button>();
        [SerializeField] private Text[] mainBuildButtonLabels = Array.Empty<Text>();
        [SerializeField] private Text[] skillMonitorTexts = Array.Empty<Text>();

        [Header("Existing popup actions and feedback")]
        [SerializeField] private Button rotateButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button closeDetailButton;
        [SerializeField] private Text feedbackText;

        [Header("Read-only protected-root baselines")]
        [SerializeField] private ItemSandboxV04ProtectedGeometryBaseline[] protectedGeometryBaselines =
            Array.Empty<ItemSandboxV04ProtectedGeometryBaseline>();

        [Header("Serialized presentation colors")]
        [SerializeField] private Color emptyCellColor = new(0.12f, 0.14f, 0.18f, 0.94f);
        [SerializeField] private Color eyeCellColor = new(0.82f, 0.55f, 0.15f, 1f);
        [SerializeField] private Color arrayCellColor = new(0.24f, 0.35f, 0.58f, 1f);
        [SerializeField] private Color occupiedUnlitColor = new(0.31f, 0.31f, 0.34f, 1f);
        [SerializeField] private Color occupiedLitColor = new(0.18f, 0.58f, 0.44f, 1f);
        [SerializeField] private Color selectedCellColor = new(0.18f, 0.66f, 0.82f, 1f);
        [SerializeField] private Color legalPreviewColor = new(0.22f, 0.70f, 0.38f, 0.9f);
        [SerializeField] private Color illegalPreviewColor = new(0.78f, 0.22f, 0.22f, 0.9f);
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color disabledTextColor = new(0.58f, 0.58f, 0.58f, 1f);

        private readonly List<ItemSandboxV04TrayEntry> trayEntries = new();
        private readonly string[] mainBuildIds = new string[6];
        private readonly UnityAction[] mainBuildActions = new UnityAction[6];
        private ItemFullDetailBuildSandboxWorkbenchSession session;
        private string selectedTrayItemId = "I001";
        private string selectedRarityKey = "white";
        private long selectedSeed = ItemBalanceCandidateDetailSandboxProvider.DefaultRootSeed;
        private string dragBaseItemId = string.Empty;
        private string movingPlacementId = string.Empty;
        private int dragRotation;
        private ItemFullDetailPlacementPreview placementPreview;
        private Camera dragCamera;

        public ItemFullDetailBuildSandboxWorkbenchSession Session => session;
        public IReadOnlyList<ItemSandboxV04TrayEntry> TrayEntries => trayEntries.AsReadOnly();
        public IReadOnlyList<ItemSandboxV04ProtectedGeometryBaseline> ProtectedGeometryBaselines => protectedGeometryBaselines;
        public string SelectedTrayItemId => selectedTrayItemId;
        public string SelectedRarityKey => selectedRarityKey;
        public long SelectedSeed => selectedSeed;

        public static IReadOnlyList<ItemSandboxV04TrayEntry> BuildTrayEntries()
        {
            return Array.AsReadOnly(ItemInnerDataCatalog.AllItems
                .Where(item => item != null && TryParseCatalogIndex(item.itemId, out int index) && index >= 1 && index <= 31)
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .Select(item => new ItemSandboxV04TrayEntry(item))
                .ToArray());
        }

#if UNITY_EDITOR
        public void ConfigureEditor(
            ItemInnerDataCatalogProvider configuredCatalogProvider,
            ItemBalanceCandidateDetailSandboxProvider configuredCandidateProvider,
            ItemDetailPanelView configuredDetailPanel,
            RectTransform configuredBattleLikePreviewArea,
            RectTransform configuredPopupLayer,
            RectTransform configuredFeedbackRoot,
            ScrollRect configuredTrayScrollRect,
            GameObject configuredLegacyItemCardLayer,
            ItemSandboxV04TraySlotView[] configuredTraySlots,
            ItemSandboxV04BoardCellView[] configuredBoardCells,
            Button[] configuredRarityButtons,
            Text[] configuredRarityButtonLabels,
            InputField configuredSeedInput,
            Button configuredRegenerateButton,
            Text configuredCandidateStatusText,
            GameObject configuredCandidateControlsRoot,
            Button configuredLevelDownButton,
            Button configuredLevelUpButton,
            Text configuredLevelText,
            Text configuredBuildSummaryText,
            Button[] configuredMainBuildButtons,
            Text[] configuredMainBuildButtonLabels,
            Text[] configuredSkillMonitorTexts,
            Button configuredRotateButton,
            Button configuredRemoveButton,
            Button configuredCloseDetailButton,
            Text configuredFeedbackText,
            ItemSandboxV04ProtectedGeometryBaseline[] configuredGeometryBaselines)
        {
            catalogProvider = configuredCatalogProvider;
            candidateProvider = configuredCandidateProvider;
            detailPanel = configuredDetailPanel;
            battleLikePreviewArea = configuredBattleLikePreviewArea;
            popupLayer = configuredPopupLayer;
            feedbackRoot = configuredFeedbackRoot;
            trayScrollRect = configuredTrayScrollRect;
            legacyItemCardLayer = configuredLegacyItemCardLayer;
            traySlots = configuredTraySlots ?? Array.Empty<ItemSandboxV04TraySlotView>();
            boardCells = configuredBoardCells ?? Array.Empty<ItemSandboxV04BoardCellView>();
            rarityButtons = configuredRarityButtons ?? Array.Empty<Button>();
            rarityButtonLabels = configuredRarityButtonLabels ?? Array.Empty<Text>();
            seedInput = configuredSeedInput;
            regenerateButton = configuredRegenerateButton;
            candidateStatusText = configuredCandidateStatusText;
            candidateControlsRoot = configuredCandidateControlsRoot;
            levelDownButton = configuredLevelDownButton;
            levelUpButton = configuredLevelUpButton;
            levelText = configuredLevelText;
            buildSummaryText = configuredBuildSummaryText;
            mainBuildButtons = configuredMainBuildButtons ?? Array.Empty<Button>();
            mainBuildButtonLabels = configuredMainBuildButtonLabels ?? Array.Empty<Text>();
            skillMonitorTexts = configuredSkillMonitorTexts ?? Array.Empty<Text>();
            rotateButton = configuredRotateButton;
            removeButton = configuredRemoveButton;
            closeDetailButton = configuredCloseDetailButton;
            feedbackText = configuredFeedbackText;
            protectedGeometryBaselines = configuredGeometryBaselines ?? Array.Empty<ItemSandboxV04ProtectedGeometryBaseline>();
        }
#endif

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
            BindControls();
            RefreshAll();
        }

        private void OnDisable()
        {
            UnbindControls();
        }

        private void Update()
        {
            CloseDetailWhenPointerPressedOutside();
        }

        private void Initialize()
        {
            detailPanel?.SetPreserveAuthoredVisualStyle(preserveAuthoredDetailPreviewOnPlay);
            detailPanel?.SetStatusBadgeSprites(
                Resources.Load<Sprite>(LightingStatusLitResourcePath),
                Resources.Load<Sprite>(LightingStatusUnlitResourcePath),
                Resources.Load<Sprite>(ArrayStatusLitResourcePath),
                Resources.Load<Sprite>(ArrayStatusUnlitResourcePath));

            if (session != null || catalogProvider == null || candidateProvider == null
                || candidateProvider.WorkbenchCatalog == null)
            {
                return;
            }

            session = new ItemFullDetailBuildSandboxWorkbenchSession(
                new ItemBalanceCandidateDetailSandboxAdapter(candidateProvider.WorkbenchCatalog, catalogProvider),
                catalogProvider);
            selectedSeed = candidateProvider.RootSeed;
            trayEntries.Clear();
            trayEntries.AddRange(BuildTrayEntries());
            if (legacyItemCardLayer != null)
            {
                legacyItemCardLayer.SetActive(false);
            }

            for (int index = 0; index < traySlots.Length; index++)
            {
                ItemSandboxV04TraySlotView slot = traySlots[index];
                if (slot == null)
                {
                    continue;
                }

                if (index < trayEntries.Count)
                {
                    slot.Bind(this, trayEntries[index].itemId);
                }
                else
                {
                    slot.Hide();
                }
            }

            foreach (ItemSandboxV04BoardCellView cell in boardCells)
            {
                cell?.Bind(this);
            }

            if (seedInput != null)
            {
                seedInput.text = selectedSeed.ToString(CultureInfo.InvariantCulture);
            }
            if (candidateControlsRoot == null)
            {
                candidateControlsRoot = FindChildGameObject(transform, "CandidateInstancePreviewControls");
            }
            detailPanel?.SetVisible(preserveAuthoredDetailPreviewOnPlay);
        }

        private void BindControls()
        {
            UnbindControls();
            BindButton(rarityButtons, 0, SelectWhite);
            BindButton(rarityButtons, 1, SelectGreen);
            BindButton(rarityButtons, 2, SelectBlue);
            BindButton(rarityButtons, 3, SelectPurple);
            BindButton(rarityButtons, 4, SelectOrange);
            regenerateButton?.onClick.AddListener(RegenerateCandidate);
            levelDownButton?.onClick.AddListener(LevelDown);
            levelUpButton?.onClick.AddListener(LevelUp);
            rotateButton?.onClick.AddListener(RotateSelected);
            removeButton?.onClick.AddListener(RemoveSelected);
            closeDetailButton?.onClick.AddListener(CloseDetail);
            seedInput?.onEndEdit.AddListener(SetSeedText);

            for (int index = 0; index < mainBuildButtons.Length && index < mainBuildActions.Length; index++)
            {
                int captured = index;
                mainBuildActions[index] = () => SelectMainBuild(captured);
                mainBuildButtons[index]?.onClick.AddListener(mainBuildActions[index]);
            }
        }

        private void UnbindControls()
        {
            UnbindButton(rarityButtons, 0, SelectWhite);
            UnbindButton(rarityButtons, 1, SelectGreen);
            UnbindButton(rarityButtons, 2, SelectBlue);
            UnbindButton(rarityButtons, 3, SelectPurple);
            UnbindButton(rarityButtons, 4, SelectOrange);
            regenerateButton?.onClick.RemoveListener(RegenerateCandidate);
            levelDownButton?.onClick.RemoveListener(LevelDown);
            levelUpButton?.onClick.RemoveListener(LevelUp);
            rotateButton?.onClick.RemoveListener(RotateSelected);
            removeButton?.onClick.RemoveListener(RemoveSelected);
            closeDetailButton?.onClick.RemoveListener(CloseDetail);
            seedInput?.onEndEdit.RemoveListener(SetSeedText);
            for (int index = 0; index < mainBuildButtons.Length && index < mainBuildActions.Length; index++)
            {
                if (mainBuildActions[index] != null)
                {
                    mainBuildButtons[index]?.onClick.RemoveListener(mainBuildActions[index]);
                    mainBuildActions[index] = null;
                }
            }
        }

        public void SelectTrayItem(string itemId)
        {
            if (session == null || !trayEntries.Any(entry => string.Equals(entry.itemId, itemId, StringComparison.Ordinal)))
            {
                return;
            }

            selectedTrayItemId = itemId;
            if (string.Equals(itemId, "I031", StringComparison.Ordinal))
            {
                session.SelectJuNian();
                ShowDetail(session.BuildSelectedDetail());
                SetFeedback("已选择唯一聚念石 I031；品阶候选不适用。拖到阵盘可放置一次。");
            }
            else
            {
                ShowCandidatePreview();
            }
            RefreshAll();
        }

        public void BeginTrayDrag(ItemSandboxV04TraySlotView slot, PointerEventData eventData)
        {
            if (slot == null || session == null)
            {
                return;
            }
            SelectTrayItemForDrag(slot.ItemId);
            dragBaseItemId = slot.ItemId;
            movingPlacementId = string.Empty;
            dragRotation = 0;
            dragCamera = eventData?.pressEventCamera;
            UpdatePlacementPreview(eventData?.position ?? Vector2.zero);
        }

        public void UpdateTrayDrag(ItemSandboxV04TraySlotView slot, PointerEventData eventData)
        {
            if (slot != null && string.Equals(dragBaseItemId, slot.ItemId, StringComparison.Ordinal))
            {
                dragCamera = eventData?.pressEventCamera;
                UpdatePlacementPreview(eventData?.position ?? Vector2.zero);
            }
        }

        public void EndTrayDrag(ItemSandboxV04TraySlotView slot, PointerEventData eventData)
        {
            if (slot == null || session == null || !TryResolveBoardCell(eventData?.position ?? Vector2.zero, out Vector2Int cell))
            {
                FinishDrag("未放入阵盘：请拖到 5×5 格子内。");
                return;
            }

            bool juNian = string.Equals(slot.ItemId, "I031", StringComparison.Ordinal);
            string createdInstanceId = string.Empty;
            if (juNian)
            {
                session.SelectJuNian();
            }
            else
            {
                long seed = NextUnusedSeed(slot.ItemId, selectedRarityKey, selectedSeed);
                ItemFullDetailWorkbenchInstance instance = session.CreateInstance(slot.ItemId, selectedRarityKey, seed);
                if (instance == null)
                {
                    FinishDrag("候选实例生成失败；目录、品阶或 Seed 校验未通过。");
                    return;
                }
                createdInstanceId = instance.itemInstanceId;
                selectedSeed = seed == long.MaxValue ? long.MinValue : seed + 1L;
                if (seedInput != null)
                {
                    seedInput.text = selectedSeed.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (!session.PlaceSelected(cell, out ItemFullDetailPlacementFailureReason reason))
            {
                if (!string.IsNullOrWhiteSpace(createdInstanceId))
                {
                    session.RemoveUnplacedInstance(createdInstanceId);
                }
                FinishDrag(DescribePlacementFailure(reason, slot.ItemId));
                return;
            }

            ShowDetail(session.BuildSelectedDetail());
            FinishDrag("放置成功：实例与 placementId 已独立生成。");
        }

        public void SelectBoardCell(Vector2Int cell)
        {
            ItemFullDetailWorkbenchPlacement placement = FindPlacementAt(cell);
            if (placement == null || !session.SelectPlacement(placement.placementId))
            {
                return;
            }
            ShowDetail(session.BuildSelectedDetail());
            SetFeedback("已选中 " + placement.baseItemId + " · " + placement.placementId);
            RefreshAll();
        }

        public void BeginBoardDrag(ItemSandboxV04BoardCellView cell, PointerEventData eventData)
        {
            ItemFullDetailWorkbenchPlacement placement = cell == null ? null : FindPlacementAt(cell.Cell);
            if (placement == null || !session.SelectPlacement(placement.placementId))
            {
                ClearDrag();
                return;
            }
            dragBaseItemId = placement.baseItemId;
            movingPlacementId = placement.placementId;
            dragRotation = placement.rotation;
            dragCamera = eventData?.pressEventCamera;
            UpdatePlacementPreview(eventData?.position ?? Vector2.zero);
        }

        public void UpdateBoardDrag(ItemSandboxV04BoardCellView cell, PointerEventData eventData)
        {
            if (!string.IsNullOrWhiteSpace(movingPlacementId))
            {
                dragCamera = eventData?.pressEventCamera;
                UpdatePlacementPreview(eventData?.position ?? Vector2.zero);
            }
        }

        public void EndBoardDrag(ItemSandboxV04BoardCellView cell, PointerEventData eventData)
        {
            if (string.IsNullOrWhiteSpace(movingPlacementId) || session == null)
            {
                FinishDrag("移动取消：当前没有可移动的已摆放道具。");
                return;
            }

            Vector2 screenPoint = eventData?.position ?? Vector2.zero;
            if (!TryResolveBoardCell(screenPoint, out Vector2Int target))
            {
                if (IsPointerOverTray(screenPoint, eventData))
                {
                    RemoveDraggedBoardPlacement();
                    return;
                }

                FinishDrag("移动取消：请拖到 5×5 格子内，或拖回道具栏移除。");
                return;
            }

            if (!session.MoveSelected(target, out ItemFullDetailPlacementFailureReason reason))
            {
                FinishDrag(DescribePlacementFailure(reason));
                return;
            }
            ShowDetail(session.BuildSelectedDetail());
            FinishDrag("移动成功；点亮、阵位、Build 与开窍状态已重算。");
        }

        private void RemoveDraggedBoardPlacement()
        {
            if (session == null || string.IsNullOrWhiteSpace(movingPlacementId))
            {
                FinishDrag("拖回道具栏失败：当前没有可移除的已摆放道具。");
                return;
            }

            string placementId = movingPlacementId;
            if (!session.SelectPlacement(placementId))
            {
                FinishDrag("拖回道具栏失败：原棋盘摆放已不存在。");
                return;
            }

            if (!session.RemoveSelected())
            {
                FinishDrag("拖回道具栏失败：当前没有可移除的已摆放道具。");
                return;
            }

            detailPanel?.SetVisible(false);
            FinishDrag("已拖回道具栏；棋盘摆放已移除，可继续从道具栏拖出。");
        }

        private void SelectWhite() => SelectRarity("white");
        private void SelectGreen() => SelectRarity("green");
        private void SelectBlue() => SelectRarity("blue");
        private void SelectPurple() => SelectRarity("purple");
        private void SelectOrange() => SelectRarity("orange");

        private void SelectTrayItemForDrag(string itemId)
        {
            if (session == null || !trayEntries.Any(entry => string.Equals(entry.itemId, itemId, StringComparison.Ordinal)))
            {
                return;
            }

            selectedTrayItemId = itemId;
            if (string.Equals(itemId, "I031", StringComparison.Ordinal))
            {
                session.SelectJuNian();
            }
            RefreshAll();
        }

        private void SelectRarity(string rarityKey)
        {
            if (string.Equals(selectedTrayItemId, "I031", StringComparison.Ordinal))
            {
                SetFeedback("I031 为唯一聚念石，不走五品阶候选生成。");
                return;
            }
            selectedRarityKey = rarityKey;
            ShowCandidatePreview();
            RefreshAll();
        }

        private void ShowCandidatePreview()
        {
            if (session == null || string.Equals(selectedTrayItemId, "I031", StringComparison.Ordinal))
            {
                return;
            }
            ItemDetailViewModel preview = session.PreviewCandidate(selectedTrayItemId, selectedRarityKey, selectedSeed);
            ShowDetail(preview);
            SetFeedback(preview == null
                ? "候选预览失败。"
                : selectedTrayItemId + " · " + selectedRarityKey + " · Seed "
                    + selectedSeed.ToString(CultureInfo.InvariantCulture) + "（仅影响下一次拖出实例）");
        }

        private void SetSeedText(string value)
        {
            if (!long.TryParse(value?.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long parsed))
            {
                if (seedInput != null)
                {
                    seedInput.text = selectedSeed.ToString(CultureInfo.InvariantCulture);
                }
                SetFeedback("Seed 必须是有符号 64 位整数。");
                return;
            }
            selectedSeed = parsed;
            ShowCandidatePreview();
        }

        private void RegenerateCandidate()
        {
            selectedSeed = selectedSeed == long.MaxValue ? long.MinValue : selectedSeed + 1L;
            if (seedInput != null)
            {
                seedInput.text = selectedSeed.ToString(CultureInfo.InvariantCulture);
            }
            ShowCandidatePreview();
        }

        private void LevelDown()
        {
            session?.SetSandboxLevel(Mathf.Max(1, (session?.SandboxLevel ?? 1) - 1));
            RefreshAfterStateChange("Sandbox Lv 已降低；仅重算只读开窍预览。");
        }

        private void LevelUp()
        {
            session?.SetSandboxLevel(Mathf.Min(40, (session?.SandboxLevel ?? 1) + 1));
            RefreshAfterStateChange("Sandbox Lv 已提高；仅重算只读开窍预览。");
        }

        private void RotateSelected()
        {
            if (session == null)
            {
                SetFeedback("ItemSandbox 会话尚未初始化。");
                return;
            }
            if (!session.RotateSelected(out ItemFullDetailPlacementFailureReason reason))
            {
                SetFeedback(DescribePlacementFailure(reason));
                return;
            }
            RefreshAfterStateChange("顺时针旋转成功；规则状态已重算。", true);
        }

        private void RemoveSelected()
        {
            if (session == null || !session.RemoveSelected())
            {
                SetFeedback("当前没有可移除的已摆放实例。");
                return;
            }
            detailPanel?.SetVisible(false);
            RefreshAfterStateChange("已从阵盘移除；候选目录仍可继续拖出新实例。");
        }

        private void CloseDetail()
        {
            detailPanel?.SetVisible(false);
            SetFeedback("详情面板已关闭；点击候选或已摆放道具可再次打开。");
        }

        private void CloseDetailWhenPointerPressedOutside()
        {
            if (detailPanel == null || !detailPanel.gameObject.activeInHierarchy)
            {
                return;
            }

            bool pressed = Input.GetMouseButtonDown(0);
            Vector2 screenPosition = Input.mousePosition;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                pressed |= touch.phase == TouchPhase.Began;
                screenPosition = touch.position;
            }

            if (!pressed)
            {
                return;
            }

            RectTransform detailRect = detailPanel.transform as RectTransform;
            if (detailRect == null)
            {
                return;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint(
                    detailRect,
                    screenPosition,
                    ResolveDetailPanelEventCamera()))
            {
                return;
            }

            detailPanel.SetVisible(false);
            SetFeedback("已点击详情面板外侧，详情面板已关闭。");
        }

        private Camera ResolveDetailPanelEventCamera()
        {
            Canvas canvas = detailPanel == null ? null : detailPanel.GetComponentInParent<Canvas>();
            return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
        }

        private void SelectMainBuild(int index)
        {
            if (session == null || index < 0 || index >= mainBuildIds.Length)
            {
                return;
            }
            string buildId = mainBuildIds[index] ?? string.Empty;
            if (!session.SelectMainBuild(buildId, "ItemSandboxV04UserClick"))
            {
                SetFeedback("主 Build 只能从当前已点亮且计数大于 0 的法门轨道中选择。");
                return;
            }
            RefreshAfterStateChange(string.IsNullOrWhiteSpace(buildId)
                ? "MainBuild = None（显式选择）"
                : "MainBuild = " + buildId + "（显式选择）");
        }

        private void RefreshAfterStateChange(string feedback, bool keepDetail = false)
        {
            if (keepDetail || !string.IsNullOrWhiteSpace(session?.SelectedPlacementId))
            {
                ShowDetail(session?.BuildSelectedDetail());
            }
            SetFeedback(feedback);
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (session == null)
            {
                return;
            }
            RefreshTray();
            RefreshCandidateControls();
            RefreshBoard();
            RefreshLevelBuildAndMonitors();
        }

        private void RefreshTray()
        {
            for (int index = 0; index < traySlots.Length; index++)
            {
                ItemSandboxV04TraySlotView slot = traySlots[index];
                if (slot == null)
                {
                    continue;
                }
                if (index >= trayEntries.Count)
                {
                    slot.Hide();
                    continue;
                }
                ItemSandboxV04TrayEntry entry = trayEntries[index];
                bool selected = string.Equals(entry.itemId, selectedTrayItemId, StringComparison.Ordinal);
                bool juNianPlaced = entry.isJuNian && session.Placements.Any(value => value.isJuNian);
                Color background = selected ? selectedCellColor
                    : entry.isJuNian ? eyeCellColor : emptyCellColor;
                slot.Bind(this, entry.itemId);
                slot.Show(entry.displayText + (juNianPlaced ? "\n已放置（唯一）" : string.Empty),
                    background, juNianPlaced ? disabledTextColor : normalTextColor, true);
            }
        }

        private void RefreshCandidateControls()
        {
            if (candidateControlsRoot != null && !candidateControlsRoot.activeSelf)
            {
                candidateControlsRoot.SetActive(true);
            }

            bool enabled = !string.Equals(selectedTrayItemId, "I031", StringComparison.Ordinal);
            for (int index = 0; index < rarityButtons.Length; index++)
            {
                if (rarityButtons[index] != null)
                {
                    rarityButtons[index].interactable = enabled;
                }
                if (index < rarityButtonLabels.Length && rarityButtonLabels[index] != null)
                {
                    bool selected = index < RarityKeys.Length
                        && string.Equals(RarityKeys[index], selectedRarityKey, StringComparison.Ordinal);
                    rarityButtonLabels[index].text = (selected ? "● " : "○ ")
                        + (index < RarityKeys.Length ? RarityKeys[index] : "?");
                    rarityButtonLabels[index].color = enabled ? normalTextColor : disabledTextColor;
                }
            }
            if (regenerateButton != null)
            {
                regenerateButton.interactable = enabled;
            }
            if (seedInput != null)
            {
                seedInput.interactable = enabled;
            }
            if (candidateStatusText != null)
            {
                candidateStatusText.text = enabled
                    ? selectedTrayItemId + " · " + selectedRarityKey + " · nextSeed="
                        + selectedSeed.ToString(CultureInfo.InvariantCulture)
                        + "\n点击品阶立即刷新候选详情；仅下一次拖出实例使用。"
                    : "I031 聚念石 · 唯一实例 · 不参与品阶候选";
            }
        }

        private void RefreshBoard()
        {
            ItemSystemSnapshot snapshot = session.Snapshot?.placementSnapshot;
            foreach (ItemSandboxV04BoardCellView cellView in boardCells)
            {
                if (cellView == null)
                {
                    continue;
                }
                Vector2Int cell = cellView.Cell;
                ItemSystemPlacementSnapshot placement = snapshot?.placements.FirstOrDefault(value => value.OccupiedCells.Contains(cell));
                bool preview = placementPreview?.OccupiedCells.Contains(cell) == true;
                string label;
                Color color;
                if (preview)
                {
                    label = placementPreview.isLegal ? "可放" : "不可";
                    color = placementPreview.isLegal ? legalPreviewColor : illegalPreviewColor;
                }
                else if (placement != null)
                {
                    bool core = placement.coreCellWorld == cell;
                    label = core ? placement.itemId + "\n核" : "·";
                    color = string.Equals(placement.placementId, session.SelectedPlacementId, StringComparison.Ordinal)
                        ? selectedCellColor : placement.isLit ? occupiedLitColor : occupiedUnlitColor;
                }
                else if (cell == new Vector2Int(2, 2))
                {
                    label = "阵眼\n禁占";
                    color = eyeCellColor;
                }
                else if (snapshot?.ArrayBonusCells.Contains(cell) == true)
                {
                    label = "阵位";
                    color = arrayCellColor;
                }
                else
                {
                    label = string.Empty;
                    color = emptyCellColor;
                }
                cellView.Show(label, color, normalTextColor);
            }
        }

        private void RefreshLevelBuildAndMonitors()
        {
            if (levelText != null)
            {
                levelText.text = "Sandbox Lv " + session.SandboxLevel.ToString(CultureInfo.InvariantCulture) + " / 40";
            }

            ItemBuildSynergyResolutionResult build = session.Snapshot?.build;
            IEnumerable<ItemBuildTrackResult> allTracks = (build?.FaMenBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Concat(build?.QiLeiBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Where(track => track.litItemCount > 0);
            if (buildSummaryText != null)
            {
                string[] lines = allTracks.Select(track =>
                    ItemSandboxBuildPresentationNames.CategoryName(track)
                    + (track.trackKind == ItemBuildTrackKind.FaMen
                        ? string.Empty
                        : " " + ItemSandboxBuildPresentationNames.TrackDisplayName(track))
                    + "\n" + session.BuildTrackEffectSummary(track)).ToArray();
                buildSummaryText.text = lines.Length == 0
                    ? "构筑：无已点亮计数"
                    : "构筑（只读）\n" + string.Join("\n", lines);
            }

            Array.Clear(mainBuildIds, 0, mainBuildIds.Length);
            mainBuildIds[0] = string.Empty;
            ItemBuildTrackResult[] selectable = (build?.FaMenBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Where(track => track.litItemCount > 0)
                .OrderBy(track => track.displayName, StringComparer.Ordinal)
                .Take(mainBuildIds.Length - 1)
                .ToArray();
            for (int index = 0; index < selectable.Length; index++)
            {
                mainBuildIds[index + 1] = selectable[index].buildId;
            }
            for (int index = 0; index < mainBuildButtons.Length; index++)
            {
                bool visible = index == 0 || index <= selectable.Length;
                if (mainBuildButtons[index] != null)
                {
                    mainBuildButtons[index].gameObject.SetActive(visible);
                    mainBuildButtons[index].interactable = visible;
                }
                if (index < mainBuildButtonLabels.Length && mainBuildButtonLabels[index] != null)
                {
                    string id = index < mainBuildIds.Length ? mainBuildIds[index] : string.Empty;
                    bool selected = string.Equals(id, session.SelectedMainBuildId, StringComparison.Ordinal);
                    string name = index == 0 ? "不选择" : index - 1 < selectable.Length
                        ? ItemSandboxBuildPresentationNames.TrackDisplayName(selectable[index - 1])
                        : string.Empty;
                    mainBuildButtonLabels[index].text = (selected ? "● " : "○ ") + "主构筑 " + name;
                }
            }

            var slots = session.Snapshot?.skillMonitor?.Slots ?? Array.Empty<TalismanBag.Items.Skills.ItemSkillMonitorSlotSnapshot>();
            for (int index = 0; index < skillMonitorTexts.Length; index++)
            {
                if (skillMonitorTexts[index] == null)
                {
                    continue;
                }
                if (index >= slots.Count)
                {
                    skillMonitorTexts[index].text = "监控槽 " + (index + 1).ToString(CultureInfo.InvariantCulture) + " · Locked";
                    continue;
                }
                var slot = slots[index];
                skillMonitorTexts[index].text = slot.displayName + " · "
                    + (slot.isMonitoring ? "Monitoring" : slot.isUnlocked ? "Unlocked" : "Locked")
                    + " · Build" + slot.requiredBuildStage.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void UpdatePlacementPreview(Vector2 screenPoint)
        {
            if (session == null || string.IsNullOrWhiteSpace(dragBaseItemId))
            {
                placementPreview = null;
                RefreshBoard();
                return;
            }

            if (!string.IsNullOrWhiteSpace(movingPlacementId) && IsPointerOverTray(screenPoint, null))
            {
                placementPreview = null;
                SetFeedback("松手拖回道具栏，移除棋盘摆放。");
                RefreshBoard();
                return;
            }

            if (!TryResolveBoardCell(screenPoint, out Vector2Int cell))
            {
                placementPreview = null;
                RefreshBoard();
                return;
            }
            placementPreview = session.PreviewPlacement(dragBaseItemId, cell, dragRotation,
                movingPlacementId, string.Equals(dragBaseItemId, "I031", StringComparison.Ordinal));
            SetFeedback(placementPreview.isLegal ? "可放置" : DescribePlacementFailure(placementPreview.failureReason, dragBaseItemId));
            RefreshBoard();
        }

        private bool TryResolveBoardCell(Vector2 screenPoint, out Vector2Int cell)
        {
            foreach (ItemSandboxV04BoardCellView view in boardCells)
            {
                if (view != null && view.RectTransform != null
                    && RectTransformUtility.RectangleContainsScreenPoint(view.RectTransform, screenPoint, dragCamera))
                {
                    cell = view.Cell;
                    return true;
                }
            }
            cell = default;
            return false;
        }

        private bool IsPointerOverTray(Vector2 screenPoint, PointerEventData eventData)
        {
            Camera camera = eventData?.pressEventCamera ?? eventData?.enterEventCamera ?? dragCamera;
            if (ContainsPointer(trayScrollRect?.viewport, screenPoint, camera)
                || ContainsPointer(trayScrollRect?.content, screenPoint, camera)
                || ContainsPointer(trayScrollRect?.transform as RectTransform, screenPoint, camera))
            {
                return true;
            }

            foreach (ItemSandboxV04TraySlotView slot in traySlots)
            {
                if (ContainsPointer(slot?.RectTransform, screenPoint, camera))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsPointer(RectTransform rect, Vector2 screenPoint, Camera camera)
        {
            return rect != null
                && rect.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, camera);
        }

        private ItemFullDetailWorkbenchPlacement FindPlacementAt(Vector2Int cell)
        {
            ItemSystemPlacementSnapshot snapshot = session?.Snapshot?.placementSnapshot?.placements
                .FirstOrDefault(value => value.OccupiedCells.Contains(cell));
            return snapshot == null ? null : session.Placements.FirstOrDefault(value =>
                string.Equals(value.placementId, snapshot.placementId, StringComparison.Ordinal));
        }

        private long NextUnusedSeed(string baseItemId, string rarityKey, long start)
        {
            long seed = start;
            while (session.Instances.Any(value => string.Equals(value.baseItemId, baseItemId, StringComparison.Ordinal)
                && string.Equals(value.rarityKey, rarityKey, StringComparison.Ordinal) && value.rootSeed == seed))
            {
                seed = seed == long.MaxValue ? long.MinValue : seed + 1L;
            }
            return seed;
        }

        private void FinishDrag(string feedback)
        {
            ClearDrag();
            SetFeedback(feedback);
            RefreshAll();
        }

        private void ClearDrag()
        {
            dragBaseItemId = string.Empty;
            movingPlacementId = string.Empty;
            dragRotation = 0;
            placementPreview = null;
            dragCamera = null;
        }

        private void ShowDetail(ItemDetailViewModel model)
        {
            if (model == null || detailPanel == null)
            {
                return;
            }
            detailPanel.Bind(model);
            detailPanel.SetVisible(true);
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message ?? string.Empty;
            }
        }

        public static string DescribePlacementFailure(ItemFullDetailPlacementFailureReason reason, string baseItemId = "")
        {
            return reason switch
            {
                ItemFullDetailPlacementFailureReason.NoSelection => "未选择可摆放实例。",
                ItemFullDetailPlacementFailureReason.AlreadyPlaced => "该实例已经摆放；请拖动阵盘上的实例移动。",
                ItemFullDetailPlacementFailureReason.DefinitionMissing => "目录中找不到该道具定义。",
                ItemFullDetailPlacementFailureReason.InvalidRotation => "该形状不支持当前旋转。",
                ItemFullDetailPlacementFailureReason.OutOfBounds => "越界：完整形状必须位于 5×5 阵盘内。",
                ItemFullDetailPlacementFailureReason.EyeCellBlocked => "阵眼 (2,2) 禁止占位，也会阻断跨阵眼中继。",
                ItemFullDetailPlacementFailureReason.CellOccupied => "格子冲突：目标形状与已有道具重叠。",
                ItemFullDetailPlacementFailureReason.DuplicateJuNian => "I031 聚念石只能存在并放置一次。",
                ItemFullDetailPlacementFailureReason.DuplicateBaseItemPlaced => "重复上阵：同一个基础道具"
                    + (string.IsNullOrWhiteSpace(baseItemId) ? string.Empty : "（" + baseItemId + "）")
                    + "在同一布局中只能放置 1 件；不同品阶、词条、Seed 或实例 ID 仍视为同一 baseItemId/itemId。",
                ItemFullDetailPlacementFailureReason.PlacementMissing => "找不到当前 placementId。",
                _ => "摆放规则未通过。"
            };
        }

        private static void BindButton(Button[] buttons, int index, UnityAction action)
        {
            if (buttons != null && index >= 0 && index < buttons.Length && buttons[index] != null)
            {
                buttons[index].onClick.AddListener(action);
            }
        }

        private static void UnbindButton(Button[] buttons, int index, UnityAction action)
        {
            if (buttons != null && index >= 0 && index < buttons.Length && buttons[index] != null)
            {
                buttons[index].onClick.RemoveListener(action);
            }
        }

        private static bool TryParseCatalogIndex(string itemId, out int index)
        {
            index = 0;
            return itemId?.Length == 4 && itemId[0] == 'I'
                && int.TryParse(itemId.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out index);
        }

        private static GameObject FindChildGameObject(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(child.name, objectName, StringComparison.Ordinal))
                {
                    return child.gameObject;
                }
            }
            return null;
        }
    }
}
