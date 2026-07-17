using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Skills;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    [DisallowMultipleComponent]
    public sealed class ItemFullDetailWorkbenchGridCellView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Vector2Int cell;
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text labelText;
        private Action<Vector2Int> onSelected;
        private Action<Vector2Int, PointerEventData> onDragStarted;
        private Action<Vector2Int, PointerEventData> onDragEnded;

        public Vector2Int Cell => cell;

#if UNITY_EDITOR
        public void ConfigureEditor(Vector2Int configuredCell, Button configuredButton, Image configuredImage, Text configuredLabel)
        {
            cell = configuredCell;
            button = configuredButton;
            backgroundImage = configuredImage;
            labelText = configuredLabel;
        }
#endif

        public void Bind(
            Action<Vector2Int> handler,
            Action<Vector2Int, PointerEventData> dragStarted,
            Action<Vector2Int, PointerEventData> dragEnded)
        {
            onSelected = handler;
            onDragStarted = dragStarted;
            onDragEnded = dragEnded;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onSelected?.Invoke(cell));
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onDragStarted?.Invoke(cell, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onDragEnded?.Invoke(cell, eventData);
        }

        public void Show(string value, Color background, Color foreground)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = background;
            }

            if (labelText != null)
            {
                labelText.text = value;
                labelText.color = foreground;
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class ItemFullDetailBuildSandboxWorkbenchView : MonoBehaviour
    {
        public const string Marker = "ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_PASS";
        private static readonly string[] RarityKeys = { "white", "green", "blue", "purple", "orange" };
        private static readonly string[] MainBuildIds =
        {
            string.Empty,
            "famen:zhenlei", "famen:lihuo", "famen:zhongyue", "famen:xuanshui", "famen:taibai"
        };

        [Header("Read-only data providers")]
        [SerializeField] private ItemInnerDataCatalogProvider catalogProvider;
        [SerializeField] private ItemBalanceCandidateDetailSandboxProvider candidateProvider;
        [SerializeField] private ItemSandboxDetailPanelView detailPanel;

        [Header("Candidate selection")]
        [SerializeField] private ItemSandboxItemButtonView[] baseItemButtons = Array.Empty<ItemSandboxItemButtonView>();
        [SerializeField] private Button[] rarityButtons = Array.Empty<Button>();
        [SerializeField] private Text[] rarityButtonTexts = Array.Empty<Text>();
        [SerializeField] private InputField seedInput;
        [SerializeField] private Button createInstanceButton;
        [SerializeField] private Button regenerateButton;
        [SerializeField] private Button selectJuNianButton;
        [SerializeField] private Button loadValidationLayoutButton;

        [Header("Generated instance slots")]
        [SerializeField] private Button[] instanceButtons = Array.Empty<Button>();
        [SerializeField] private Text[] instanceButtonTexts = Array.Empty<Text>();

        [Header("Board and placement actions")]
        [SerializeField] private ItemFullDetailWorkbenchGridCellView[] boardCells = Array.Empty<ItemFullDetailWorkbenchGridCellView>();
        [SerializeField] private Button rotateButton;
        [SerializeField] private Button removeButton;

        [Header("Sandbox level")]
        [SerializeField] private Button levelDownButton;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Text levelText;

        [Header("Main Build and skill monitor")]
        [SerializeField] private Button[] mainBuildButtons = Array.Empty<Button>();
        [SerializeField] private Text[] mainBuildButtonTexts = Array.Empty<Text>();
        [SerializeField] private Text[] skillMonitorTexts = Array.Empty<Text>();
        [SerializeField] private Text statusText;

        [Header("Serialized visual states")]
        [SerializeField] private Color emptyCellColor = new(0.16f, 0.13f, 0.10f, 0.92f);
        [SerializeField] private Color eyeCellColor = new(0.45f, 0.12f, 0.10f, 0.96f);
        [SerializeField] private Color arrayCellColor = new(0.34f, 0.27f, 0.12f, 0.96f);
        [SerializeField] private Color unlitItemColor = new(0.28f, 0.28f, 0.27f, 0.96f);
        [SerializeField] private Color litItemColor = new(0.55f, 0.42f, 0.16f, 0.98f);
        [SerializeField] private Color arrayActiveColor = new(0.25f, 0.52f, 0.28f, 0.98f);
        [SerializeField] private Color sourceColor = new(0.62f, 0.30f, 0.10f, 0.98f);
        [SerializeField] private Color selectedCellColor = new(0.72f, 0.61f, 0.22f, 1f);
        [SerializeField] private Color normalTextColor = new(0.86f, 0.80f, 0.70f, 1f);
        [SerializeField] private Color selectedButtonColor = new(0.84f, 0.57f, 0.20f, 1f);
        [SerializeField] private Color inactiveButtonColor = new(0.35f, 0.29f, 0.20f, 1f);
        [SerializeField] private Color activeMonitorColor = new(0.38f, 0.68f, 0.31f, 1f);
        [SerializeField] private Color inactiveMonitorColor = new(0.48f, 0.45f, 0.40f, 1f);

        private ItemFullDetailBuildSandboxWorkbenchSession session;
        private string selectedBaseItemId = "I001";
        private string selectedRarityKey = "orange";
        private long selectedSeed = ItemBalanceCandidateDetailSandboxProvider.DefaultRootSeed;
        private ItemDetailViewModel temporaryPreviewModel;
        private string boardDragPlacementId = string.Empty;
        private bool bound;

        public ItemFullDetailBuildSandboxWorkbenchSession Session => session;

#if UNITY_EDITOR
        public void ConfigureEditor(
            ItemInnerDataCatalogProvider configuredCatalogProvider,
            ItemBalanceCandidateDetailSandboxProvider configuredCandidateProvider,
            ItemSandboxDetailPanelView configuredDetailPanel,
            ItemSandboxItemButtonView[] configuredBaseItemButtons,
            Button[] configuredRarityButtons,
            Text[] configuredRarityTexts,
            InputField configuredSeedInput,
            Button configuredCreateButton,
            Button configuredRegenerateButton,
            Button configuredJuNianButton,
            Button configuredValidationLayoutButton,
            Button[] configuredInstanceButtons,
            Text[] configuredInstanceTexts,
            ItemFullDetailWorkbenchGridCellView[] configuredBoardCells,
            Button configuredRotateButton,
            Button configuredRemoveButton,
            Button configuredLevelDownButton,
            Button configuredLevelUpButton,
            Text configuredLevelText,
            Button[] configuredMainBuildButtons,
            Text[] configuredMainBuildTexts,
            Text[] configuredSkillMonitorTexts,
            Text configuredStatusText)
        {
            catalogProvider = configuredCatalogProvider;
            candidateProvider = configuredCandidateProvider;
            detailPanel = configuredDetailPanel;
            baseItemButtons = configuredBaseItemButtons ?? Array.Empty<ItemSandboxItemButtonView>();
            rarityButtons = configuredRarityButtons ?? Array.Empty<Button>();
            rarityButtonTexts = configuredRarityTexts ?? Array.Empty<Text>();
            seedInput = configuredSeedInput;
            createInstanceButton = configuredCreateButton;
            regenerateButton = configuredRegenerateButton;
            selectJuNianButton = configuredJuNianButton;
            loadValidationLayoutButton = configuredValidationLayoutButton;
            instanceButtons = configuredInstanceButtons ?? Array.Empty<Button>();
            instanceButtonTexts = configuredInstanceTexts ?? Array.Empty<Text>();
            boardCells = configuredBoardCells ?? Array.Empty<ItemFullDetailWorkbenchGridCellView>();
            rotateButton = configuredRotateButton;
            removeButton = configuredRemoveButton;
            levelDownButton = configuredLevelDownButton;
            levelUpButton = configuredLevelUpButton;
            levelText = configuredLevelText;
            mainBuildButtons = configuredMainBuildButtons ?? Array.Empty<Button>();
            mainBuildButtonTexts = configuredMainBuildTexts ?? Array.Empty<Text>();
            skillMonitorTexts = configuredSkillMonitorTexts ?? Array.Empty<Text>();
            statusText = configuredStatusText;
        }
#endif

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (session == null && candidateProvider != null && catalogProvider != null)
            {
                session = new ItemFullDetailBuildSandboxWorkbenchSession(
                    new ItemBalanceCandidateDetailSandboxAdapter(
                        candidateProvider.WorkbenchCatalog,
                        catalogProvider),
                    catalogProvider);
            }

            if (!bound)
            {
                BindControls();
                bound = true;
            }

            if (seedInput != null && string.IsNullOrWhiteSpace(seedInput.text))
            {
                seedInput.text = selectedSeed.ToString(CultureInfo.InvariantCulture);
            }

            if (temporaryPreviewModel == null && session != null && session.Instances.Count == 0)
            {
                RefreshTemporaryPreview();
            }
            RefreshAll();
        }

        private void BindControls()
        {
            foreach (ItemSandboxItemButtonView itemButton in baseItemButtons.Where(value => value != null))
            {
                string itemId = itemButton.ItemId;
                Button button = itemButton.GetComponent<Button>();
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectBaseItem(itemId));
            }

            for (int index = 0; index < rarityButtons.Length && index < RarityKeys.Length; index++)
            {
                int captured = index;
                if (rarityButtons[index] != null)
                {
                    rarityButtons[index].onClick.RemoveAllListeners();
                    rarityButtons[index].onClick.AddListener(() => SelectRarity(RarityKeys[captured]));
                }
            }

            BindButton(createInstanceButton, CreateSelectedInstance);
            BindButton(regenerateButton, RegenerateSeed);
            BindButton(selectJuNianButton, SelectJuNian);
            BindButton(loadValidationLayoutButton, LoadValidationLayout);
            BindButton(rotateButton, RotateSelected);
            BindButton(removeButton, RemoveSelected);
            BindButton(levelDownButton, () => ChangeLevel(-1));
            BindButton(levelUpButton, () => ChangeLevel(1));
            foreach (ItemFullDetailWorkbenchGridCellView cell in boardCells.Where(value => value != null))
            {
                cell.Bind(HandleCellClick, HandleCellDragStarted, HandleCellDragEnded);
            }

            for (int index = 0; index < mainBuildButtons.Length && index < MainBuildIds.Length; index++)
            {
                int captured = index;
                if (mainBuildButtons[index] != null)
                {
                    mainBuildButtons[index].onClick.RemoveAllListeners();
                    mainBuildButtons[index].onClick.AddListener(() => SelectMainBuild(MainBuildIds[captured]));
                }
            }
        }

        private void SelectBaseItem(string itemId)
        {
            if (string.Equals(itemId, "I031", StringComparison.Ordinal))
            {
                SelectJuNian();
                return;
            }

            selectedBaseItemId = itemId;
            RefreshTemporaryPreview();
            RefreshAll();
        }

        private void SelectRarity(string rarityKey)
        {
            selectedRarityKey = rarityKey;
            RefreshTemporaryPreview();
            RefreshAll();
        }

        private void SyncControlsFromSelectedInstance()
        {
            if (session == null)
            {
                return;
            }

            ItemFullDetailWorkbenchInstance instance = session.Instances.FirstOrDefault(value =>
                string.Equals(value.itemInstanceId, session.SelectedInstanceId, StringComparison.Ordinal));
            if (instance == null)
            {
                return;
            }

            selectedBaseItemId = instance.baseItemId;
            selectedRarityKey = instance.rarityKey;
            selectedSeed = instance.rootSeed;
            if (seedInput != null)
            {
                seedInput.text = selectedSeed.ToString(CultureInfo.InvariantCulture);
            }

            temporaryPreviewModel = null;
        }

        private void CreateSelectedInstance()
        {
            if (session == null)
            {
                return;
            }

            if (seedInput != null && !long.TryParse(seedInput.text, NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out selectedSeed))
            {
                SetStatus("Seed输入无效；未执行Roll。", true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(session.SelectedPlacementId) && temporaryPreviewModel == null)
            {
                bool rerolled = session.TryRerollSelectedPlacement(
                    selectedRarityKey, selectedSeed, out ItemFullDetailWorkbenchInstance rerolledInstance);
                SetStatus(rerolled
                    ? $"已重Roll棋盘道具 {rerolledInstance.baseItemId} {rerolledInstance.rarityKey} seed={rerolledInstance.rootSeed} [{rerolledInstance.buildQualification}]；位置保持不变。"
                    : "当前选中棋盘道具无法重Roll；I031或无效实例不支持。",
                    !rerolled);
                if (rerolled)
                {
                    SyncControlsFromSelectedInstance();
                }

                RefreshAll();
                return;
            }

            ItemFullDetailWorkbenchInstance instance = session.CreateInstance(
                selectedBaseItemId, selectedRarityKey, selectedSeed);
            if (instance != null)
            {
                temporaryPreviewModel = null;
            }
            SetStatus(instance == null
                ? "候选实例生成失败；请查看校验信息。"
                : $"已选择实例 {instance.itemInstanceId}；点击棋盘空格放置。",
                instance == null);
            RefreshAll();
        }

        private void RegenerateSeed()
        {
            long current = selectedSeed;
            if (seedInput != null)
            {
                long.TryParse(seedInput.text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out current);
            }

            selectedSeed = current == long.MaxValue ? long.MinValue : current + 1L;
            if (seedInput != null)
            {
                seedInput.text = selectedSeed.ToString(CultureInfo.InvariantCulture);
            }

            CreateSelectedInstance();
        }

        private void SelectJuNian()
        {
            temporaryPreviewModel = null;
            session?.SelectJuNian();
            SetStatus("已选择固定核心道具 I031；点击棋盘空格放置。", false);
            RefreshAll();
        }

        private void LoadValidationLayout()
        {
            if (session == null)
            {
                return;
            }

            bool success = session.LoadValidationLayout(out IReadOnlyList<long> seeds);
            SetStatus(success
                ? "已载入可编辑震雷法门 Build6 验证布局；seeds=" + string.Join("|", seeds)
                : "真实候选资格无法组成验证布局；未伪造BuildQualification。",
                !success);
            RefreshAll();
        }

        private void HandleCellClick(Vector2Int cell)
        {
            if (session == null)
            {
                return;
            }

            ItemSystemPlacementSnapshot occupied = FindPlacementAtCell(cell);
            bool changed;
            if (occupied != null)
            {
                changed = session.SelectPlacement(occupied.placementId);
                if (changed)
                {
                    SyncControlsFromSelectedInstance();
                }
            }
            else if (!string.IsNullOrWhiteSpace(session.SelectedPlacementId))
            {
                changed = session.MoveSelected(cell);
            }
            else
            {
                changed = session.PlaceSelected(cell);
            }

            if (!changed)
            {
                SetStatus("当前操作不合法：检查阵眼、越界、重叠、I031唯一性或实例是否已摆放。", true);
            }

            RefreshAll();
        }

        private void HandleCellDragStarted(Vector2Int cell, PointerEventData eventData)
        {
            if (session == null)
            {
                boardDragPlacementId = string.Empty;
                return;
            }

            ItemSystemPlacementSnapshot occupied = FindPlacementAtCell(cell);
            boardDragPlacementId = occupied?.placementId ?? string.Empty;
        }

        private void HandleCellDragEnded(Vector2Int cell, PointerEventData eventData)
        {
            if (session == null || string.IsNullOrWhiteSpace(boardDragPlacementId))
            {
                boardDragPlacementId = string.Empty;
                return;
            }

            string placementId = boardDragPlacementId;
            boardDragPlacementId = string.Empty;
            if (!IsPointerOverInstanceTray(eventData))
            {
                return;
            }

            if (!session.SelectPlacement(placementId))
            {
                SetStatus("拖回道具栏失败：原棋盘摆放已不存在。", true);
                RefreshAll();
                return;
            }

            SyncControlsFromSelectedInstance();
            bool changed = session.RemoveSelected();
            SetStatus(changed
                ? "已拖回道具栏；棋盘摆放已移除，生成实例仍保留。"
                : "拖回道具栏失败：当前没有有效摆放。",
                !changed);
            RefreshAll();
        }

        private ItemSystemPlacementSnapshot FindPlacementAtCell(Vector2Int cell)
        {
            return session?.Snapshot?.placementSnapshot?.placements
                .FirstOrDefault(placement => placement.OccupiedCells.Contains(cell));
        }

        private bool IsPointerOverInstanceTray(PointerEventData eventData)
        {
            if (eventData == null)
            {
                return false;
            }

            Camera camera = eventData.pressEventCamera ?? eventData.enterEventCamera;
            foreach (Button button in instanceButtons ?? Array.Empty<Button>())
            {
                if (button == null)
                {
                    continue;
                }

                RectTransform rect = button.transform as RectTransform;
                if (ContainsPointer(rect, eventData.position, camera))
                {
                    return true;
                }

                RectTransform parent = rect?.parent as RectTransform;
                if (ContainsPointer(parent, eventData.position, camera))
                {
                    return true;
                }

                RectTransform grandParent = parent?.parent as RectTransform;
                if (ContainsPointer(grandParent, eventData.position, camera))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsPointer(RectTransform rect, Vector2 screenPosition, Camera camera)
        {
            return rect != null
                && rect.gameObject.activeInHierarchy
                && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, camera);
        }

        private void RotateSelected()
        {
            bool changed = session?.RotateSelected() == true;
            SetStatus(changed ? "已顺时针旋转90°。" : "旋转后布局不合法，保持原状态。", !changed);
            RefreshAll();
        }

        private void RemoveSelected()
        {
            bool changed = session?.RemoveSelected() == true;
            SetStatus(changed ? "已移除摆放；生成实例仍保留。" : "当前没有已选摆放。", !changed);
            RefreshAll();
        }

        private void ChangeLevel(int delta)
        {
            if (session == null)
            {
                return;
            }

            session.SetSandboxLevel(session.SandboxLevel + delta);
            RefreshAll();
        }

        private void SelectMainBuild(string buildId)
        {
            bool changed = session?.SelectMainBuild(buildId, "UserClick") == true;
            SetStatus(changed
                ? "主Build已显式切换为 " + (string.IsNullOrWhiteSpace(buildId) ? "None" : buildId)
                : "该法门Build不存在；主Build保持不变。",
                !changed);
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (session == null)
            {
                return;
            }

            RefreshRarityButtons();
            RefreshInstanceButtons();
            RefreshBoard();
            RefreshBuildButtons();
            RefreshSkillMonitors();
            if (levelText != null)
            {
                levelText.text = "Sandbox Lv." + session.SandboxLevel.ToString(CultureInfo.InvariantCulture);
            }

            ItemDetailViewModel model = temporaryPreviewModel ?? session.BuildSelectedDetail();
            if (model != null)
            {
                detailPanel?.Show(model);
            }

            RefreshStatusSummary();
        }

        private void RefreshRarityButtons()
        {
            for (int index = 0; index < rarityButtonTexts.Length && index < RarityKeys.Length; index++)
            {
                if (rarityButtonTexts[index] != null)
                {
                    bool selected = string.Equals(selectedRarityKey, RarityKeys[index], StringComparison.Ordinal);
                    rarityButtonTexts[index].color = selected ? selectedButtonColor : normalTextColor;
                }
            }
        }

        private void RefreshInstanceButtons()
        {
            List<(string id, string label)> rows = new()
            {
                (ItemFullDetailBuildSandboxWorkbenchSession.JuNianInstanceId, "I031 固定聚念石")
            };
            rows.AddRange(session.Instances.Select(instance => (
                instance.itemInstanceId,
                $"{instance.baseItemId} {instance.rarityKey} seed={instance.rootSeed} [{instance.buildQualification}]")));
            for (int index = 0; index < instanceButtons.Length; index++)
            {
                Button button = instanceButtons[index];
                Text label = index < instanceButtonTexts.Length ? instanceButtonTexts[index] : null;
                bool hasRow = index < rows.Count;
                if (button != null)
                {
                    button.gameObject.SetActive(hasRow);
                    button.onClick.RemoveAllListeners();
                    if (hasRow)
                    {
                        string captured = rows[index].id;
                        button.onClick.AddListener(() =>
                        {
                            temporaryPreviewModel = null;
                            if (session.SelectInstance(captured))
                            {
                                SyncControlsFromSelectedInstance();
                            }

                            RefreshAll();
                        });
                    }
                }

                if (label != null && hasRow)
                {
                    label.text = rows[index].label;
                    label.color = string.Equals(session.SelectedInstanceId, rows[index].id, StringComparison.Ordinal)
                        ? selectedButtonColor
                        : normalTextColor;
                }
            }
        }

        private void RefreshBoard()
        {
            foreach (ItemFullDetailWorkbenchGridCellView cellView in boardCells.Where(value => value != null))
            {
                Vector2Int cell = cellView.Cell;
                ItemSystemPlacementSnapshot placement = session.Snapshot.placementSnapshot.placements
                    .FirstOrDefault(value => value.OccupiedCells.Contains(cell));
                bool isEye = cell == ItemGridPlacementRulePreview.EyeCell;
                bool isArray = ItemGridPlacementRulePreview.ArrayBonusCells.Contains(cell);
                Color color = isEye ? eyeCellColor : isArray ? arrayCellColor : emptyCellColor;
                string label = $"{cell.x},{cell.y}";
                if (placement != null)
                {
                    color = placement.isLightingSource ? sourceColor
                        : placement.isArrayBonusActive ? arrayActiveColor
                        : placement.isLit ? litItemColor : unlitItemColor;
                    if (string.Equals(placement.placementId, session.SelectedPlacementId, StringComparison.Ordinal))
                    {
                        color = selectedCellColor;
                    }

                    ItemFullDetailWorkbenchPlacement mapped = session.Placements.First(value =>
                        string.Equals(value.placementId, placement.placementId, StringComparison.Ordinal));
                    label = placement.itemId + "\n" + ShortId(mapped.itemInstanceId)
                        + "\n" + (placement.isLit ? "LIT" : "UNLIT")
                        + (placement.isArrayBonusActive ? "/ARRAY" : string.Empty);
                }
                else if (isEye)
                {
                    label += "\nEYE";
                }
                else if (isArray)
                {
                    label += "\nARRAY";
                }

                cellView.Show(label, color, normalTextColor);
            }
        }

        private void RefreshBuildButtons()
        {
            for (int index = 0; index < mainBuildButtonTexts.Length && index < MainBuildIds.Length; index++)
            {
                Text label = mainBuildButtonTexts[index];
                if (label == null)
                {
                    continue;
                }

                string buildId = MainBuildIds[index];
                ItemBuildTrackResult track = string.IsNullOrWhiteSpace(buildId)
                    ? null
                    : session.Snapshot.build.FindFaMenBuild(buildId);
                label.text = string.IsNullOrWhiteSpace(buildId)
                    ? "None"
                    : ItemSandboxBuildPresentationNames.TrackDisplayName(track) + " "
                        + track?.litItemCount + "/6";
                label.color = string.Equals(session.SelectedMainBuildId, buildId, StringComparison.Ordinal)
                    ? selectedButtonColor
                    : inactiveButtonColor;
            }
        }

        private void RefreshSkillMonitors()
        {
            IReadOnlyList<ItemSkillMonitorSlotSnapshot> slots = session.Snapshot.skillMonitor?.Slots
                ?? Array.Empty<ItemSkillMonitorSlotSnapshot>();
            for (int index = 0; index < skillMonitorTexts.Length; index++)
            {
                Text text = skillMonitorTexts[index];
                if (text == null)
                {
                    continue;
                }

                ItemSkillMonitorSlotSnapshot slot = slots.FirstOrDefault(value => value.slotOrder == index);
                if (slot == null)
                {
                    text.text = "Monitor unavailable";
                    text.color = inactiveMonitorColor;
                    continue;
                }

                text.text = slot.displayName + "\n"
                    + (slot.isMonitoring ? "已激活" : slot.isUnlocked ? "准备" : "未达成")
                    + "\nsource=" + (string.IsNullOrWhiteSpace(slot.sourceBuildId) ? "None" : slot.sourceBuildId);
                text.color = slot.isMonitoring ? activeMonitorColor : inactiveMonitorColor;
            }
        }

        private void RefreshStatusSummary()
        {
            if (statusText == null)
            {
                return;
            }

            string tracks = string.Join(" | ", session.Snapshot.build.FaMenBuilds.Select(track =>
                ItemSandboxBuildPresentationNames.TrackDisplayName(track)
                + ":" + track.litItemCount + "/6 " + track.activeStageLabel));
            string selected = string.IsNullOrWhiteSpace(session.SelectedMainBuildId)
                ? "None"
                : session.SelectedMainBuildId;
            string mapping = string.IsNullOrWhiteSpace(session.SelectedPlacementId)
                ? "placement=None"
                : "placement=" + session.SelectedPlacementId + " → instance=" + session.SelectedInstanceId;
            string validation = session.Snapshot.ValidationErrors.Count == 0
                ? "None"
                : string.Join(" | ", session.Snapshot.ValidationErrors.Take(3));
            statusText.text = $"{Marker}\nbase={selectedBaseItemId} rarity={selectedRarityKey} seed={selectedSeed}\n"
                + mapping + $"\nmainBuild={selected}\nFaMen={tracks}\nvalidation={validation}";
        }

        private void SetStatus(string message, bool isError)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? eyeCellColor : normalTextColor;
            }
        }

        private void RefreshTemporaryPreview()
        {
            if (session == null || string.Equals(selectedBaseItemId, "I031", StringComparison.Ordinal))
            {
                temporaryPreviewModel = null;
                return;
            }

            if (seedInput != null)
            {
                long.TryParse(seedInput.text, NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture, out selectedSeed);
            }

            temporaryPreviewModel = session.PreviewCandidate(selectedBaseItemId, selectedRarityKey, selectedSeed);
            SetStatus(temporaryPreviewModel == null
                ? "临时候选预览生成失败；未修改实例列表。"
                : $"临时候选预览：{selectedBaseItemId} {selectedRarityKey}；点击“创建实例”后才写入Sandbox实例列表。",
                temporaryPreviewModel == null);
        }

        private static string ShortId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "None";
            }

            return value.Length <= 10 ? value : value.Substring(value.Length - 10, 10);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
