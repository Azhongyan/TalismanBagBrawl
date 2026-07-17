using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Items.Lighting;
using TalismanBag.Items.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.ItemSandbox
{
    public sealed class ItemSandboxGridPlacementPreviewView : MonoBehaviour, IItemLightingSnapshotProvider, IItemArrayBonusSnapshotProvider, IItemBuildSynergySnapshotProvider, IItemCoreAwakeningSnapshotProvider, IItemCoreAwakeningReadOnlyInputProvider, IItemMainBuildSelectionProvider, IItemSkillMonitorSnapshotProvider
    {
        private const int ManualScenarioIndex = -1;

        [SerializeField] private MonoBehaviour catalogProviderBehaviour;
        [SerializeField] private ItemSandboxGridCellView[] cellViews;
        [SerializeField] private Button[] scenarioButtons;
        [SerializeField] private Text[] scenarioButtonTexts;
        [SerializeField] private Button[] mainBuildSelectionButtons;
        [SerializeField] private Text[] mainBuildSelectionButtonTexts;
        [SerializeField] private Image[] skillMonitorIconImages;
        [SerializeField] private Text[] skillMonitorIconTexts;
        [SerializeField] private Text statusText;
        [SerializeField] private Vector2Int anchorCell = new(0, 1);
        [SerializeField] private int activeScenarioIndex;
        [SerializeField] private int selectedMainBuildIndex;
        [SerializeField] private int selectedMainBuildRevision;
        [SerializeField] private Color baseCellColor = new(0.33f, 0.24f, 0.16f, 0.96f);
        [SerializeField] private Color eyeCellColor = new(0.42f, 0.08f, 0.06f, 1f);
        [SerializeField] private Color arrayCellColor = new(0.58f, 0.42f, 0.14f, 1f);
        [SerializeField] private Color blockerCellColor = new(0.16f, 0.15f, 0.14f, 1f);
        [SerializeField] private Color litRangeCellColor = new(0.43f, 0.43f, 0.43f, 1f);
        [SerializeField] private Color sourceCellColor = new(0.62f, 0.52f, 0.26f, 1f);
        [SerializeField] private Color directLitCellColor = new(0.24f, 0.60f, 0.46f, 1f);
        [SerializeField] private Color relayLitCellColor = new(0.24f, 0.42f, 0.70f, 1f);
        [SerializeField] private Color unlitCellColor = new(0.20f, 0.20f, 0.20f, 1f);
        [SerializeField] private Color validOccupiedColor = new(0.19f, 0.47f, 0.50f, 1f);
        [SerializeField] private Color validCoreColor = new(0.20f, 0.64f, 0.45f, 1f);
        [SerializeField] private Color invalidOccupiedColor = new(0.74f, 0.20f, 0.14f, 1f);
        [SerializeField] private Color labelColor = new(0.96f, 0.86f, 0.64f, 1f);

        private readonly List<ItemGridPlacedItemPreview> demoLockedPlacements = new();
        private ItemInnerDataCatalogProvider catalogProvider;
        private string selectedItemId;
        private string selectedPlacementId;
        private ItemGridPlacementEvaluation lastEvaluation;
        private ItemLightingResolutionResult lastLightingResult;
        private ItemArrayBonusResolutionResult lastArrayBonusResult;
        private ItemBuildSynergyResolutionResult lastBuildSynergyResult;
        private ItemCoreAwakeningResolutionResult lastCoreAwakeningResult;
        private ItemSkillMonitorResolutionResult lastSkillMonitorResult;
        private ItemSystemSnapshot lastItemSystemSnapshot;
        private Action<string> itemSelectionRequestHandler;

        public int ConfiguredCellCount => cellViews?.Length ?? 0;
        public int ConfiguredScenarioButtonCount => scenarioButtons?.Length ?? 0;
        public bool HasCatalogProviderReference => catalogProviderBehaviour != null;
        public Vector2Int AnchorCell => anchorCell;
        public string SelectedItemId => selectedItemId;
        public string SelectedPlacementId => selectedPlacementId;
        public ItemGridPlacementEvaluation LastEvaluation => lastEvaluation;
        public ItemLightingResolutionResult LastLightingResult => lastLightingResult;
        public ItemArrayBonusResolutionResult LastArrayBonusResult => lastArrayBonusResult;
        public ItemBuildSynergyResolutionResult LastBuildSynergyResult => lastBuildSynergyResult;
        public ItemCoreAwakeningResolutionResult LastCoreAwakeningResult => lastCoreAwakeningResult;
        public ItemSkillMonitorResolutionResult LastSkillMonitorResult => lastSkillMonitorResult;
        public ItemSystemSnapshot LastItemSystemSnapshot => lastItemSystemSnapshot;
        public string ActiveScenarioId => GetActiveScenario()?.scenarioId ?? "ManualPlacementPreview";
        public IReadOnlyList<ItemGridPlacedItemPreview> DemoLockedPlacements => demoLockedPlacements;
        public int ConfiguredMainBuildSelectionButtonCount => mainBuildSelectionButtons?.Length ?? 0;
        public int ConfiguredSkillMonitorIconCount => skillMonitorIconTexts?.Length ?? 0;
        public string SelectedMainBuildId => CurrentMainBuildOption().buildId;

        public static IReadOnlyList<string> MainBuildSelectionIds => MainBuildOptions
            .Select(option => option.buildId)
            .ToArray();

        private static readonly MainBuildOption[] MainBuildOptions =
        {
            new("None", string.Empty),
            new("震雷法", "famen:zhenlei"),
            new("离火法", "famen:lihuo"),
            new("中岳法", "famen:zhongyue"),
            new("玄水法", "famen:xuanshui"),
            new("太白法", "famen:taibai")
        };

        public void ConfigureEditor(
            MonoBehaviour configuredCatalogProviderBehaviour,
            ItemSandboxGridCellView[] configuredCellViews,
            Text configuredStatusText,
            Button[] configuredScenarioButtons = null,
            Text[] configuredScenarioButtonTexts = null,
            Button[] configuredMainBuildSelectionButtons = null,
            Text[] configuredMainBuildSelectionButtonTexts = null,
            Image[] configuredSkillMonitorIconImages = null,
            Text[] configuredSkillMonitorIconTexts = null)
        {
            catalogProviderBehaviour = configuredCatalogProviderBehaviour;
            cellViews = configuredCellViews;
            statusText = configuredStatusText;
            scenarioButtons = configuredScenarioButtons;
            scenarioButtonTexts = configuredScenarioButtonTexts;
            mainBuildSelectionButtons = configuredMainBuildSelectionButtons;
            mainBuildSelectionButtonTexts = configuredMainBuildSelectionButtonTexts;
            skillMonitorIconImages = configuredSkillMonitorIconImages;
            skillMonitorIconTexts = configuredSkillMonitorIconTexts;
        }

        public void ConfigureSkillMonitorEditor(
            Button[] configuredMainBuildSelectionButtons,
            Text[] configuredMainBuildSelectionButtonTexts,
            Image[] configuredSkillMonitorIconImages,
            Text[] configuredSkillMonitorIconTexts)
        {
            mainBuildSelectionButtons = configuredMainBuildSelectionButtons;
            mainBuildSelectionButtonTexts = configuredMainBuildSelectionButtonTexts;
            skillMonitorIconImages = configuredSkillMonitorIconImages;
            skillMonitorIconTexts = configuredSkillMonitorIconTexts;
        }

        public void SelectItem(string itemId)
        {
            selectedItemId = itemId;
            selectedPlacementId = string.Empty;
            Refresh();
        }

        public void BindSelectionRequest(Action<string> handler)
        {
            itemSelectionRequestHandler = handler;
        }

        public ItemDetailViewModel ProjectDetailViewModel(ItemDetailViewModel sourceModel)
        {
            if (sourceModel == null)
            {
                return null;
            }

            string placementId = ResolveProjectedPlacementId(sourceModel);
            ItemDetailProjectionContextKind contextKind = string.IsNullOrWhiteSpace(placementId)
                ? ItemDetailProjectionContextKind.CatalogPreview
                : ItemDetailProjectionContextKind.PlacedInstance;
            return ItemDetailProjectionComposer.Compose(
                sourceModel,
                contextKind,
                placementId,
                lastLightingResult,
                lastArrayBonusResult,
                lastBuildSynergyResult,
                lastCoreAwakeningResult,
                lastSkillMonitorResult);
        }

        public ItemLightingResolutionResult GetLightingSnapshot()
        {
            return lastLightingResult;
        }

        public ItemArrayBonusResolutionResult GetArrayBonusSnapshot()
        {
            return lastArrayBonusResult;
        }

        public ItemBuildSynergyResolutionResult GetBuildSynergySnapshot()
        {
            return lastBuildSynergyResult;
        }

        public ItemCoreAwakeningResolutionResult GetCoreAwakeningSnapshot()
        {
            return lastCoreAwakeningResult;
        }

        public ItemMainBuildSelectionInput GetMainBuildSelectionInput()
        {
            return new ItemMainBuildSelectionInput(
                CurrentMainBuildOption().buildId,
                "ItemSandboxPreview",
                selectedMainBuildRevision);
        }

        public ItemSkillMonitorResolutionResult GetItemSkillMonitorSnapshot()
        {
            return lastSkillMonitorResult;
        }

        public ItemCoreAwakeningInput GetAwakeningInput(string itemId, string placementId)
        {
            if (lastLightingResult == null)
            {
                return null;
            }

            IReadOnlyList<ItemCoreAwakeningInput> inputs = ItemSandboxCoreAwakeningPreviewCatalog.CreatePreviewInputs(lastLightingResult);
            if (!string.IsNullOrWhiteSpace(placementId))
            {
                ItemCoreAwakeningInput placementInput = inputs.FirstOrDefault(input =>
                    input != null && string.Equals(input.placementId, placementId, StringComparison.Ordinal));
                if (placementInput != null)
                {
                    return placementInput;
                }
            }

            return inputs.FirstOrDefault(input =>
                input != null && string.Equals(input.itemId, itemId, StringComparison.Ordinal));
        }

        private string ResolveProjectedPlacementId(ItemDetailViewModel sourceModel)
        {
            if (sourceModel == null || lastLightingResult == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(sourceModel.placementId))
            {
                ItemLightingItemResult placementResult = lastLightingResult.FindPlacementResult(sourceModel.placementId);
                if (placementResult != null
                    && string.Equals(placementResult.itemId, sourceModel.itemId, StringComparison.Ordinal))
                {
                    return placementResult.placementId;
                }
            }

            if (!string.IsNullOrEmpty(selectedPlacementId))
            {
                ItemLightingItemResult placementResult = lastLightingResult.FindPlacementResult(selectedPlacementId);
                if (placementResult != null
                    && string.Equals(placementResult.itemId, sourceModel.itemId, StringComparison.Ordinal))
                {
                    return placementResult.placementId;
                }
            }

            return string.Empty;
        }

        private void Awake()
        {
            ResolveProvider();
            EnsureDemoLockedPlacements();
            BindCells();
            BindScenarioButtons();
            BindMainBuildSelectionButtons();
        }

        private void Start()
        {
            ResolveProvider();
            EnsureDemoLockedPlacements();

            if (string.IsNullOrEmpty(selectedItemId) && catalogProvider != null)
            {
                ItemInnerDataDefinition first = catalogProvider.GetCatalogItems().FirstOrDefault();
                if (first != null)
                {
                    selectedItemId = first.itemId;
                }
            }

            Refresh();
        }

        private void ResolveProvider()
        {
            catalogProvider = catalogProviderBehaviour as ItemInnerDataCatalogProvider;
        }

        private void BindCells()
        {
            if (cellViews == null)
            {
                return;
            }

            foreach (ItemSandboxGridCellView cellView in cellViews)
            {
                cellView?.Bind(HandleCellSelected);
            }
        }

        private void BindScenarioButtons()
        {
            if (scenarioButtons == null)
            {
                return;
            }

            IReadOnlyList<ItemSandboxLightingScenarioDefinition> scenarios = ItemSandboxLightingScenarioCatalog.AllScenarios;
            for (int i = 0; i < scenarioButtons.Length; i++)
            {
                Button button = scenarioButtons[i];
                if (button == null)
                {
                    continue;
                }

                int scenarioIndex = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => HandleScenarioSelected(scenarioIndex));

                if (scenarioButtonTexts != null
                    && i < scenarioButtonTexts.Length
                    && scenarioButtonTexts[i] != null
                    && i < scenarios.Count)
                {
                    scenarioButtonTexts[i].text = scenarios[i].displayName;
                }
            }
        }

        private void BindMainBuildSelectionButtons()
        {
            if (mainBuildSelectionButtons == null)
            {
                return;
            }

            ClampMainBuildIndex();
            for (int i = 0; i < mainBuildSelectionButtons.Length; i++)
            {
                Button button = mainBuildSelectionButtons[i];
                if (button == null)
                {
                    continue;
                }

                int optionIndex = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => HandleMainBuildSelected(optionIndex));

                if (mainBuildSelectionButtonTexts != null
                    && i < mainBuildSelectionButtonTexts.Length
                    && mainBuildSelectionButtonTexts[i] != null
                    && i < MainBuildOptions.Length)
                {
                    MainBuildOption option = MainBuildOptions[i];
                    mainBuildSelectionButtonTexts[i].text = string.IsNullOrWhiteSpace(option.buildId)
                        ? option.displayName
                        : $"{option.displayName}\n{option.buildId}";
                }
            }
        }

        private void HandleCellSelected(Vector2Int selectedCell)
        {
            activeScenarioIndex = ManualScenarioIndex;
            anchorCell = selectedCell;
            Refresh();
        }

        private void HandleScenarioSelected(int scenarioIndex)
        {
            IReadOnlyList<ItemSandboxLightingScenarioDefinition> scenarios = ItemSandboxLightingScenarioCatalog.AllScenarios;
            if (scenarioIndex < 0 || scenarioIndex >= scenarios.Count)
            {
                return;
            }

            activeScenarioIndex = scenarioIndex;
            ItemSandboxLightingScenarioDefinition scenario = scenarios[scenarioIndex];
            string firstSelectableItemId = scenario.Placements
                .FirstOrDefault(placement => !string.Equals(placement.itemId, "I031", StringComparison.Ordinal))
                ?.itemId ?? scenario.Placements.FirstOrDefault()?.itemId;

            if (!string.IsNullOrEmpty(firstSelectableItemId) && itemSelectionRequestHandler != null)
            {
                itemSelectionRequestHandler(firstSelectableItemId);
                return;
            }

            Refresh();
        }

        private void HandleMainBuildSelected(int optionIndex)
        {
            if (optionIndex < 0 || optionIndex >= MainBuildOptions.Length)
            {
                return;
            }

            selectedMainBuildIndex = optionIndex;
            selectedMainBuildRevision++;
            Refresh();
        }

        private void Refresh()
        {
            ResolveProvider();
            EnsureDemoLockedPlacements();
            UpdateScenarioButtonState();

            ItemSandboxLightingScenarioDefinition scenario = GetActiveScenario();
            if (scenario != null)
            {
                RefreshScenario(scenario);
                return;
            }

            lastLightingResult = null;
            lastArrayBonusResult = null;
            lastBuildSynergyResult = null;
            lastCoreAwakeningResult = null;
            lastSkillMonitorResult = null;
            lastItemSystemSnapshot = null;
            UpdateMainBuildButtonState();
            UpdateSkillMonitorIcons(null);
            ItemInnerDataDefinition item = FindSelectedItem();
            lastEvaluation = ItemGridPlacementRulePreview.Evaluate(item, anchorCell, demoLockedPlacements);

            RefreshCells(lastEvaluation);
            if (statusText != null)
            {
                statusText.text = ItemGridPlacementRulePreview.BuildStatusText(lastEvaluation)
                    + "\nPreview-only: no lighting, relay, build, awakening, drop, save, or battle settlement is executed."
                    + "\nClick a board cell to move anchorCell.";
            }
        }

        private ItemSandboxLightingScenarioDefinition GetActiveScenario()
        {
            IReadOnlyList<ItemSandboxLightingScenarioDefinition> scenarios = ItemSandboxLightingScenarioCatalog.AllScenarios;
            if (activeScenarioIndex < 0 || activeScenarioIndex >= scenarios.Count)
            {
                return null;
            }

            return scenarios[activeScenarioIndex];
        }

        private void RefreshScenario(ItemSandboxLightingScenarioDefinition scenario)
        {
            selectedPlacementId = ResolveSelectedPlacementId(scenario);
            ItemSandboxLightingScenarioBuild build = ItemSandboxLightingScenarioCatalog.BuildScenario(scenario);
            lastEvaluation = build.PlacementEvaluations.FirstOrDefault(evaluation => evaluation.itemId == selectedItemId)
                ?? build.PlacementEvaluations.FirstOrDefault();
            lastItemSystemSnapshot = CreateItemSystemSnapshot(scenario);
            lastLightingResult = lastItemSystemSnapshot?.ToLightingResolutionResult();
            lastArrayBonusResult = lastItemSystemSnapshot?.ToArrayBonusResolutionResult();
            lastBuildSynergyResult = lastItemSystemSnapshot?.ToBuildSynergyResolutionResult();
            lastCoreAwakeningResult = lastItemSystemSnapshot?.ToCoreAwakeningResolutionResult();
            lastSkillMonitorResult = lastItemSystemSnapshot?.ToSkillMonitorResolutionResult();

            RefreshScenarioCells(build, lastLightingResult, lastArrayBonusResult, lastBuildSynergyResult, lastCoreAwakeningResult);
            UpdateMainBuildButtonState();
            UpdateSkillMonitorIcons(lastSkillMonitorResult);
            if (statusText != null)
            {
                statusText.text = BuildScenarioStatusText(scenario, build, lastItemSystemSnapshot, lastLightingResult, lastArrayBonusResult, lastBuildSynergyResult, lastCoreAwakeningResult, lastSkillMonitorResult);
            }
        }

        private ItemSystemSnapshot CreateItemSystemSnapshot(ItemSandboxLightingScenarioDefinition scenario)
        {
            ItemSystemPlacementInput[] placements = (scenario?.Placements ?? Array.Empty<ItemSandboxLightingScenarioPlacement>())
                .Select(placement => new ItemSystemPlacementInput(
                    placement.placementId,
                    placement.itemId,
                    placement.anchorCell))
                .ToArray();
            ItemMainBuildSelectionInput mainBuildInput = GetMainBuildSelectionInput();
            ItemSystemSnapshot firstPass = DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(new ItemSystemSnapshotInput(
                placements,
                mainBuildSelectionInput: mainBuildInput));
            IReadOnlyList<ItemCoreAwakeningInput> previewInputs = ItemSandboxCoreAwakeningPreviewCatalog.CreatePreviewInputs(firstPass.ToLightingResolutionResult());
            return DefaultItemSystemSnapshotProvider.Instance.CreateSnapshot(new ItemSystemSnapshotInput(
                placements,
                awakeningInputs: previewInputs,
                mainBuildSelectionInput: mainBuildInput));
        }

        private ItemInnerDataDefinition FindSelectedItem()
        {
            if (catalogProvider == null || string.IsNullOrEmpty(selectedItemId))
            {
                return null;
            }

            return catalogProvider.GetCatalogItems()
                .FirstOrDefault(item => item != null && item.itemId == selectedItemId);
        }

        private string ResolveSelectedPlacementId(ItemSandboxLightingScenarioDefinition scenario)
        {
            if (scenario == null || string.IsNullOrEmpty(selectedItemId))
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(selectedPlacementId)
                && scenario.Placements.Any(placement =>
                    string.Equals(placement.placementId, selectedPlacementId, StringComparison.Ordinal)
                    && string.Equals(placement.itemId, selectedItemId, StringComparison.Ordinal)))
            {
                return selectedPlacementId;
            }

            return scenario.Placements
                .FirstOrDefault(placement => string.Equals(placement.itemId, selectedItemId, StringComparison.Ordinal))
                ?.placementId ?? string.Empty;
        }

        private bool IsSelectedScenarioItem(ItemLightingItemResult itemResult)
        {
            if (itemResult == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(selectedPlacementId))
            {
                return string.Equals(itemResult.placementId, selectedPlacementId, StringComparison.Ordinal);
            }

            return string.Equals(itemResult.itemId, selectedItemId, StringComparison.Ordinal);
        }

        private void RefreshCells(ItemGridPlacementEvaluation evaluation)
        {
            if (cellViews == null)
            {
                return;
            }

            foreach (ItemSandboxGridCellView cellView in cellViews)
            {
                if (cellView == null)
                {
                    continue;
                }

                Vector2Int cell = cellView.Cell;
                bool selectedOccupied = evaluation != null && evaluation.OccupiedCells.Contains(cell);
                bool selectedCore = selectedOccupied && evaluation.coreCellWorld == cell;
                bool lockedOccupied = demoLockedPlacements.Any(placement => placement.ContainsCell(cell));
                bool isEye = ItemGridPlacementRulePreview.IsEyeCell(cell);
                bool isArray = ItemGridPlacementRulePreview.IsArrayBonusCell(cell);
                bool isAnchor = evaluation != null && evaluation.anchorCell == cell;

                Color backgroundColor = PickCellColor(evaluation, selectedOccupied, selectedCore, lockedOccupied, isEye, isArray);
                cellView.Show(BuildCellLabel(cell, isEye, isArray, lockedOccupied, selectedOccupied, selectedCore, isAnchor), backgroundColor, labelColor);
            }
        }

        private void RefreshScenarioCells(
            ItemSandboxLightingScenarioBuild build,
            ItemLightingResolutionResult lightingResult,
            ItemArrayBonusResolutionResult arrayBonusResult,
            ItemBuildSynergyResolutionResult buildSynergyResult,
            ItemCoreAwakeningResolutionResult coreAwakeningResult)
        {
            if (cellViews == null)
            {
                return;
            }

            foreach (ItemSandboxGridCellView cellView in cellViews)
            {
                if (cellView == null)
                {
                    continue;
                }

                Vector2Int cell = cellView.Cell;
                ItemLightingItemResult itemResult = FindItemAtCell(lightingResult, cell);
                ItemArrayBonusItemResult arrayItemResult = itemResult == null
                    ? null
                    : arrayBonusResult?.FindPlacementResult(itemResult.placementId);
                ItemCoreAwakeningItemResult awakeningItemResult = itemResult == null
                    ? null
                    : coreAwakeningResult?.FindPlacementResult(itemResult.placementId);
                bool isCore = itemResult != null && itemResult.coreCellWorld == cell;
                bool isEye = ItemGridPlacementRulePreview.IsEyeCell(cell);
                bool isArray = ItemGridPlacementRulePreview.IsArrayBonusCell(cell);
                bool inLitRange = lightingResult != null && lightingResult.LitRangeCells.Contains(cell);
                bool selected = itemResult != null && IsSelectedScenarioItem(itemResult);

                cellView.Show(
                    BuildScenarioCellLabel(cell, itemResult, arrayItemResult, awakeningItemResult, isCore, isEye, isArray, inLitRange, selected),
                    PickScenarioCellColor(itemResult, isEye, isArray, inLitRange, selected),
                    labelColor);
            }
        }

        private static ItemLightingItemResult FindItemAtCell(
            ItemLightingResolutionResult lightingResult,
            Vector2Int cell)
        {
            return lightingResult?.ItemResults.FirstOrDefault(item => item.OccupiedCells.Contains(cell));
        }

        private Color PickScenarioCellColor(
            ItemLightingItemResult itemResult,
            bool isEye,
            bool isArray,
            bool inLitRange,
            bool selected)
        {
            Color color;
            if (itemResult != null)
            {
                if (itemResult.isLightingSource)
                {
                    color = sourceCellColor;
                }
                else if (itemResult.isDirectLit)
                {
                    color = directLitCellColor;
                }
                else if (itemResult.IsRelayLit)
                {
                    color = relayLitCellColor;
                }
                else
                {
                    color = unlitCellColor;
                }
            }
            else if (isEye)
            {
                color = eyeCellColor;
            }
            else if (inLitRange)
            {
                color = litRangeCellColor;
            }
            else if (isArray)
            {
                color = arrayCellColor;
            }
            else
            {
                color = baseCellColor;
            }

            if (!selected)
            {
                return color;
            }

            return new Color(
                Mathf.Clamp01(color.r + 0.18f),
                Mathf.Clamp01(color.g + 0.18f),
                Mathf.Clamp01(color.b + 0.18f),
                color.a);
        }

        private Color PickCellColor(
            ItemGridPlacementEvaluation evaluation,
            bool selectedOccupied,
            bool selectedCore,
            bool lockedOccupied,
            bool isEye,
            bool isArray)
        {
            if (selectedOccupied)
            {
                if (evaluation != null && evaluation.IsValid && selectedCore)
                {
                    return validCoreColor;
                }

                return evaluation != null && evaluation.IsValid ? validOccupiedColor : invalidOccupiedColor;
            }

            if (lockedOccupied)
            {
                return blockerCellColor;
            }

            if (isEye)
            {
                return eyeCellColor;
            }

            if (isArray)
            {
                return arrayCellColor;
            }

            return baseCellColor;
        }

        private static string BuildScenarioCellLabel(
            Vector2Int cell,
            ItemLightingItemResult itemResult,
            ItemArrayBonusItemResult arrayItemResult,
            ItemCoreAwakeningItemResult awakeningItemResult,
            bool isCore,
            bool isEye,
            bool isArray,
            bool inLitRange,
            bool selected)
        {
            List<string> tags = new();
            if (isEye)
            {
                tags.Add("阵眼石");
            }

            if (isArray)
            {
                tags.Add("阵脉");
            }

            if (inLitRange && itemResult == null)
            {
                tags.Add("点亮范围");
            }

            if (itemResult != null)
            {
                if (itemResult.isLightingSource)
                {
                    tags.Add("聚念石");
                }
                else if (itemResult.isDirectLit)
                {
                    tags.Add("直亮");
                }
                else if (itemResult.IsRelayLit)
                {
                    tags.Add("接亮");
                }
                else
                {
                    tags.Add("未亮");
                }

                tags.Add(itemResult.itemId);
                if (!string.Equals(itemResult.placementId, itemResult.itemId, StringComparison.Ordinal))
                {
                    tags.Add(itemResult.placementId);
                }

                if (arrayItemResult != null && arrayItemResult.OccupiedArrayBonusCells.Contains(cell))
                {
                    tags.Add(arrayItemResult.isArrayBonusActive ? "ArrayActive" : "ArrayReady");
                }

                if (awakeningItemResult != null && isCore)
                {
                    tags.Add($"Lv.{awakeningItemResult.itemLevel}");
                    if (awakeningItemResult.coreEffectActive)
                    {
                        tags.Add("CoreActive");
                    }
                    else if (awakeningItemResult.coreEffectUnlocked)
                    {
                        tags.Add("CoreUnlocked");
                    }
                    else
                    {
                        tags.Add("CoreLocked");
                    }
                }

                if (isCore)
                {
                    tags.Add("core");
                }
            }

            if (selected)
            {
                tags.Add("选中");
            }

            return tags.Count == 0 ? $"{cell.x},{cell.y}" : string.Join("\n", tags);
        }

        private static string BuildCellLabel(
            Vector2Int cell,
            bool isEye,
            bool isArray,
            bool lockedOccupied,
            bool selectedOccupied,
            bool selectedCore,
            bool isAnchor)
        {
            List<string> tags = new();
            if (isEye)
            {
                tags.Add("Eye");
            }

            if (isArray)
            {
                tags.Add("Array");
            }

            if (lockedOccupied)
            {
                tags.Add("Block");
            }

            if (selectedOccupied)
            {
                tags.Add(selectedCore ? "Core" : "Occ");
            }

            if (isAnchor)
            {
                tags.Add("Anchor");
            }

            return tags.Count == 0 ? $"{cell.x},{cell.y}" : string.Join("\n", tags);
        }

        private string BuildScenarioStatusText(
            ItemSandboxLightingScenarioDefinition scenario,
            ItemSandboxLightingScenarioBuild build,
            ItemSystemSnapshot itemSystemSnapshot,
            ItemLightingResolutionResult lightingResult,
            ItemArrayBonusResolutionResult arrayBonusResult,
            ItemBuildSynergyResolutionResult buildSynergyResult,
            ItemCoreAwakeningResolutionResult coreAwakeningResult,
            ItemSkillMonitorResolutionResult skillMonitorResult)
        {
            if (scenario == null)
            {
                return "No lighting scenario.";
            }

            List<string> lines = new()
            {
                $"{scenario.displayName} / {scenario.scenarioId}",
                scenario.description,
                "默认直接点亮范围：聚念石上下左右一格，不含斜角，越界裁剪。",
                "直接点亮判定：只检查普通道具 coreCellWorld 是否进入 litRangeCells。",
                "接亮判定：已点亮普通道具的任意 occupiedCells 与普通道具上下左右相邻。"
            };

            AppendItemSystemSnapshotLines(lines, itemSystemSnapshot);

            if (build != null && build.Errors.Count > 0)
            {
                lines.Add("Scenario placement errors:");
                lines.AddRange(build.Errors);
                return string.Join(Environment.NewLine, lines);
            }

            if (lightingResult == null)
            {
                lines.Add("No lighting result.");
                return string.Join(Environment.NewLine, lines);
            }

            lines.Add($"litRangeCells: {ItemGridPlacementRulePreview.FormatCells(lightingResult.LitRangeCells)}");
            lines.Add($"directLitItemIds: {FormatIds(lightingResult.DirectLitItemIds)}");
            lines.Add($"relayLitItemIds: {FormatIds(lightingResult.RelayLitItemIds)}");
            lines.Add($"unlitItemIds: {FormatIds(lightingResult.UnlitItemIds)}");
            lines.Add($"directLitPlacementIds: {FormatIds(lightingResult.DirectLitPlacementIds)}");
            lines.Add($"relayLitPlacementIds: {FormatIds(lightingResult.RelayLitPlacementIds)}");
            lines.Add($"unlitPlacementIds: {FormatIds(lightingResult.UnlitPlacementIds)}");
            if (arrayBonusResult != null)
            {
                lines.Add($"arrayOnPlacementIds: {FormatIds(arrayBonusResult.OnArrayPlacementIds)}");
                lines.Add($"arrayActivePlacementIds: {FormatIds(arrayBonusResult.ActivePlacementIds)}");
            }

            if (buildSynergyResult != null)
            {
                lines.Add(ItemBuildSynergyResolver.FormatOverview(buildSynergyResult));
            }

            if (coreAwakeningResult != null)
            {
                lines.Add(ItemCoreAwakeningResolver.FormatOverview(coreAwakeningResult));
            }

            if (skillMonitorResult != null)
            {
                lines.Add(ItemSkillMonitorResolver.FormatOverview(skillMonitorResult));
            }

            ItemLightingItemResult selected = !string.IsNullOrWhiteSpace(selectedPlacementId)
                ? lightingResult.FindPlacementResult(selectedPlacementId)
                : lightingResult.FindItemResult(selectedItemId);
            if (selected != null)
            {
                ItemArrayBonusItemResult selectedArray = arrayBonusResult?.FindPlacementResult(selected.placementId);
                ItemBuildSynergyItemResult selectedBuild = buildSynergyResult?.FindPlacementResult(selected.placementId);
                ItemCoreAwakeningItemResult selectedAwakening = coreAwakeningResult?.FindPlacementResult(selected.placementId);
                lines.Add($"selected: {selected.itemId}/{selected.placementId} isDirectLit={selected.isDirectLit} isLit={selected.isLit} litByItemId={selected.litByItemId} litByPlacementId={selected.litByPlacementId} litDepth={selected.litDepth} isOnArrayBonusCell={selectedArray?.isOnArrayBonusCell == true} isArrayBonusActive={selectedArray?.isArrayBonusActive == true} countedInBuild={selectedBuild?.countedInBuild == true} faMenStage={ItemBuildTrackResult.BuildStageLabel(selectedBuild?.faMenActiveStagePieceCount ?? 0)} qiLeiStage={ItemBuildTrackResult.BuildStageLabel(selectedBuild?.qiLeiActiveStagePieceCount ?? 0)}");
                lines.Add($"selectedAwakening: {ItemSandboxCoreAwakeningPreviewCatalog.BuildPreviewLevelHint(selectedAwakening)} unlockedNodes={ItemCoreAwakeningResolver.FormatNodeLevels(selectedAwakening?.UnlockedNodeLevels)}");
            }

            lines.Add("Sandbox only: Build and CoreAwakening are read-only previews; no Build effect, numeric array bonus settlement, formal upgrade, battle wiring, save, reward, boss, or bridge.");
            return string.Join(Environment.NewLine, lines);
        }

        private static void AppendItemSystemSnapshotLines(List<string> lines, ItemSystemSnapshot snapshot)
        {
            if (lines == null)
            {
                return;
            }

            if (snapshot == null)
            {
                lines.Add("ItemSystemSnapshot: None");
                return;
            }

            lines.Add($"ItemSystemSnapshot: {snapshot.schemaVersion}");
            lines.Add($"isValid: {snapshot.isValid}");
            lines.Add($"placementCount: {snapshot.placements.Count}");
            lines.Add($"litCount: {snapshot.placements.Count(placement => placement.isLit)}");
            lines.Add($"activeArrayBonusCount: {snapshot.placements.Count(placement => placement.isArrayBonusActive)}");
            lines.Add($"selectedMainBuildId: {FormatOptional(snapshot.selectedMainBuildId)}");
            lines.Add($"skillMonitorSlotList: {FormatSnapshotSkillSlotList(snapshot.skillMonitorSnapshot)}");
            lines.Add($"validationErrors: {FormatSnapshotErrors(snapshot.validationErrors)}");
        }

        private static string FormatSnapshotSkillSlotList(ItemSystemSkillMonitorSnapshot skillMonitorSnapshot)
        {
            IReadOnlyList<ItemSystemSkillMonitorSlotSnapshot> slots = skillMonitorSnapshot?.Slots;
            if (slots == null || slots.Count == 0)
            {
                return "None";
            }

            return string.Join(" | ", slots.Select(slot =>
                $"{slot.slotOrder}:{slot.slotType} monitoring={slot.isMonitoring} unlocked={slot.isUnlocked} trigger={slot.isTriggered} cooldown={slot.cooldown01:0.###} charge={slot.charge01:0.###}"));
        }

        private static string FormatSnapshotErrors(IReadOnlyList<ItemSystemValidationError> errors)
        {
            return errors == null || errors.Count == 0
                ? "None"
                : string.Join("|", errors.Select(error => error.code));
        }

        private static string FormatOptional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "None" : value;
        }

        private static string FormatIds(IReadOnlyList<string> ids)
        {
            return ids == null || ids.Count == 0 ? "None" : string.Join("|", ids);
        }

        private void UpdateScenarioButtonState()
        {
            if (scenarioButtons == null)
            {
                return;
            }

            for (int i = 0; i < scenarioButtons.Length; i++)
            {
                Image image = scenarioButtons[i] != null ? scenarioButtons[i].GetComponent<Image>() : null;
                if (image != null)
                {
                    image.color = i == activeScenarioIndex
                        ? new Color(0.64f, 0.48f, 0.22f, 0.82f)
                        : new Color(0.36f, 0.25f, 0.14f, 0.68f);
                }
            }
        }

        private void UpdateMainBuildButtonState()
        {
            if (mainBuildSelectionButtons == null)
            {
                return;
            }

            ClampMainBuildIndex();
            for (int i = 0; i < mainBuildSelectionButtons.Length; i++)
            {
                Image image = mainBuildSelectionButtons[i] != null ? mainBuildSelectionButtons[i].GetComponent<Image>() : null;
                if (image != null)
                {
                    image.color = i == selectedMainBuildIndex
                        ? new Color(0.52f, 0.42f, 0.20f, 0.88f)
                        : new Color(0.25f, 0.20f, 0.15f, 0.78f);
                }
            }
        }

        private void UpdateSkillMonitorIcons(ItemSkillMonitorResolutionResult skillMonitorResult)
        {
            if (skillMonitorIconTexts == null)
            {
                return;
            }

            var slots = skillMonitorResult?.Slots;
            for (int i = 0; i < skillMonitorIconTexts.Length; i++)
            {
                var slot = slots != null && i < slots.Count ? slots[i] : null;
                bool monitoring = slot?.isMonitoring == true;
                bool unlocked = slot?.isUnlocked == true;
                if (skillMonitorIconImages != null && i < skillMonitorIconImages.Length && skillMonitorIconImages[i] != null)
                {
                    skillMonitorIconImages[i].color = monitoring
                        ? new Color(0.38f, 0.62f, 0.48f, 0.88f)
                        : new Color(0.20f, 0.20f, 0.20f, 0.86f);
                }

                if (skillMonitorIconTexts[i] != null)
                {
                    string displayName = slot?.displayName ?? SlotDisplayNameByIndex(i);
                    string state = monitoring ? "Monitoring" : "Locked";
                    string key = slot == null ? "none" : slot.iconKey;
                    skillMonitorIconTexts[i].text = $"{displayName}\n{state}\nunlocked={unlocked}\ntrigger=false\n{key}";
                }
            }
        }

        private static string SlotDisplayNameByIndex(int index)
        {
            return index switch
            {
                0 => "普攻",
                1 => "Build2",
                2 => "Build4",
                3 => "Build6",
                _ => "Slot"
            };
        }

        private MainBuildOption CurrentMainBuildOption()
        {
            ClampMainBuildIndex();
            return MainBuildOptions[selectedMainBuildIndex];
        }

        private void ClampMainBuildIndex()
        {
            if (selectedMainBuildIndex < 0 || selectedMainBuildIndex >= MainBuildOptions.Length)
            {
                selectedMainBuildIndex = 0;
            }
        }

        private void EnsureDemoLockedPlacements()
        {
            if (demoLockedPlacements.Count > 0)
            {
                return;
            }

            Vector2Int anchor = new(0, 0);
            Vector2Int[] occupiedCells =
            {
                new(0, 0),
                new(1, 0)
            };
            demoLockedPlacements.Add(new ItemGridPlacedItemPreview(
                "preview_blocker",
                "Demo occupiedCells",
                anchor,
                occupiedCells,
                anchor));
        }

        private readonly struct MainBuildOption
        {
            public MainBuildOption(string displayName, string buildId)
            {
                this.displayName = displayName;
                this.buildId = buildId;
            }

            public readonly string displayName;
            public readonly string buildId;
        }
    }
}
