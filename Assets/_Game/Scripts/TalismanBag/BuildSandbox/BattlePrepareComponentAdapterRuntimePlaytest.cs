using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Combat;
using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.UI;
using TalismanBag.V03.BattlePrepare;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattlePrepareComponentAdapterRuntimePlaytestPlan
    {
        public const string PackageName = "V0.4-BattlePrepareComponentAdapterRuntimePlaytest01";
        public const string FixtureFeedbackPackageName =
            "V0.4-BattlePrepareComponentAdapterRuntimePlaytest01-FixtureFeedbackFix01";
        public const string ValidationMode = "ManualUnityPlayRuntimeHandFeelProbe";
        public const string ManualMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[Manual Only] Open Runtime Playtest";
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[QA Only] Run Runtime Playtest Validation";
        public const string FixtureFeedbackQaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01-FixtureFeedbackFix01/[QA Only] Run Fixture Feedback Validation";
        public const string ShapeAwareItemTrayPackageName =
            "V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix02-ShapeAwareItemTray";
        public const string ShapeAwareItemTrayQaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareRuntimePlaytestFixtureFeedbackFix02-ShapeAwareItemTray/[QA Only] Run Shape-Aware ItemTray Validation";
        public const string ShapeAwareTrayPackingDragPackageName =
            "V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix03-ShapeAwareTrayPackingDrag";
        public const string ShapeAwareTrayPackingDragQaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareRuntimePlaytestFixtureFeedbackFix03-ShapeAwareTrayPackingDrag/[QA Only] Run Shape-Aware Tray Packing Drag Validation";
        public const string MobileShapePlacementRuntimeIntegrationPackageName =
            MobileShapePlacementRuntimeIntegration.PackageName;
        public const string MobileShapePlacementRuntimeIntegrationQaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/MobileShapePlacementRuntimeIntegrationFix01/[QA Only] Run Runtime Integration Validation";
        public const string DeferredBoardCommitMode =
            "DEFERRED_TO_MOBILE_SHAPE_PLACEMENT_INTERACTION01";
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity";

        public string packageName = PackageName;
        public string validationMode = ValidationMode;
        public string manualMenuPath = ManualMenuPath;
        public string qaMenuPath = QaMenuPath;
        public string targetScenePath = TargetScenePath;
        public bool devOnly = true;
        public bool isEnabled;
        public bool manualOnly = true;
        public bool realUnityPlayTouchablePath = true;
        public bool automationClaimsHandFeelPass;
        public bool staticReportOnly;
        public bool createsDontSaveRuntimeLauncher = true;
        public bool createsRuntimeFixtureProvider = true;
        public bool fixtureProviderDevOnly = true;
        public bool fixtureDefinitionsDontSave = true;
        public bool injectsFixtureItemsIntoMatureTray = true;
        public bool fixtureItemsVisibleInTray = true;
        public bool itemTrayShapeAwareDisplay = true;
        public bool shapeAwareDragVisual = true;
        public bool ghostPreviewMatchesTrayShape = true;
        public bool selectedHighlightCoversShape = true;
        public bool opensFormalSceneReadOnly = true;
        public bool callsMaturePrepareEntrypoint = true;
        public bool usesMatureBoardTrayDragPullup = true;
        public bool writesFormalScene;
        public bool writesFormalUi;
        public bool writesFixtureToFormalPool;
        public bool writesFixtureToSaveData;
        public bool overwritesHandTunedRectTransform;
        public bool touchesRunFlow;
        public bool touchesPageState;
        public bool touchesFormationState;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public bool redrewV04Board;
        public bool redrewV04ItemTray;
        public bool rewroteDragFeel;
        public bool rewrotePullUpAnimation;
        public bool promotesTemporaryPreviewUi;
        public bool featureFlagDefaultEnabled;
        public List<BattlePrepareComponentAdapterRuntimeSurfaceRow> runtimeSurfaces = new();
        public List<BattlePrepareComponentAdapterRuntimeStepRow> userSteps = new();
        public List<BattlePrepareRuntimeFixtureItemRow> fixtureItems = new();
        public List<BattlePrepareRuntimeFixtureFeedbackRow> fixtureFeedback = new();
        public List<BattlePrepareShapeAwareItemTrayRow> shapeAwareItemTrayRows = new();

        public int RuntimeSurfaceCount => runtimeSurfaces?.Count ?? 0;
        public int UserStepCount => userSteps?.Count ?? 0;
        public int FixtureItemCount => fixtureItems?.Count ?? 0;
        public int FixtureFeedbackCount => fixtureFeedback?.Count ?? 0;
        public int ShapeAwareItemTrayRowCount => shapeAwareItemTrayRows?.Count ?? 0;

        public static BattlePrepareComponentAdapterRuntimePlaytestPlan BuildDefault()
        {
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = new()
            {
                featureFlagDefaultEnabled = BuildSandboxFeatureFlags.All.Any(flag => flag.DefaultValue)
            };

            plan.runtimeSurfaces = new List<BattlePrepareComponentAdapterRuntimeSurfaceRow>
            {
                Surface(
                    "maturePrepareOpen",
                    "成熟整备打开",
                    "V03BattlePrepareInteractionController.TryOpenPrepareThen",
                    "Runtime launcher calls the existing mature prepare entrypoint after Unity enters Play.",
                    true),
                Surface(
                    "boardOccupancyExtension",
                    "多格占格扩展",
                    "V02FormationGridFrame + TalismanGridSlotView",
                    "A DontSave overlay follows mature board slots and previews occupied cells without moving board RectTransforms.",
                    true),
                Surface(
                    "itemTrayShapeExtension",
                    "道具栏形状扩展",
                    BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot,
                    "The user selects mature BattlePrepare item views or devOnly fixture item views; shape data is shown as an extension panel.",
                    true),
                Surface(
                    "runtimeFixtureProvider",
                    "运行时测试夹具",
                    BattlePageViewFormalReferenceKeys.PrepareItemTrayRoot,
                    "Four DontSave fixture item views are injected into the mature tray at Play time only: Single1, Vertical2, Corner3, Square4.",
                    true),
                Surface(
                    "dragRotationPlacementExtension",
                    "拖拽旋转反馈",
                    "DraggableTalismanItemView",
                    "Existing drag events and pointer movement drive the extension preview; no replacement drag system is introduced.",
                    true),
                Surface(
                    "placementFeedbackExtension",
                    "占格合法性反馈",
                    "Existing board slot positions",
                    "Valid cells are green, overlapping/out-of-grid cells are red, and feedback text stays in the devOnly layer.",
                    true),
                Surface(
                    "pullUpHandFeelReuse",
                    "上拉动画复用",
                    BattlePageViewFormalReferenceKeys.PrepareMotionRoot,
                    "The mature board + tray synchronized pull-up remains owned by V03 BattlePrepare.",
                    true)
            };

            plan.userSteps = new List<BattlePrepareComponentAdapterRuntimeStepRow>
            {
                Step(
                    "openMenu",
                    "Run the Manual Only menu item.",
                    "Unity opens the V02 FormationCounter scene and enters Play with a DontSave runtime launcher."),
                Step(
                    "waitForPrepare",
                    "Wait for the devOnly runtime banner, mature BattlePrepare tray, and V0.4 fixture items.",
                    "The existing board, item tray, drag gesture, pull-up motion, and four fixture items are visible and touchable."),
                Step(
                    "selectFixture",
                    "Click or drag Single1, Vertical2, Corner3, or Square4 in the mature tray.",
                    "The V0.4 extension panel binds the matching sandbox shape to the selected devOnly fixture item."),
                Step(
                    "rotate",
                    "Press R or click Rotate Shape on Vertical2 or Corner3.",
                    "Rotation changes the occupied-cell preview without replacing drag behavior."),
                Step(
                    "validFeedback",
                    "Hover or drag over board cells.",
                    "The devOnly overlay shows green valid cells and Chinese valid-placement feedback."),
                Step(
                    "invalidFeedback",
                    "Hover a multi-cell fixture over an occupied cell or the board edge.",
                    "The devOnly overlay reports red overlap or out-of-grid Chinese feedback."),
                Step(
                    "stopPlay",
                    "Stop Play.",
                    "The DontSave launcher and extension layer disappear; no V02/V03 scene asset or hand-tuned RectTransform is saved.")
            };

            plan.fixtureItems = BuildRequiredFixtureItems();
            plan.fixtureFeedback = BuildRequiredFixtureFeedback();
            plan.shapeAwareItemTrayRows = BuildRequiredShapeAwareItemTrayRows();
            return plan;
        }

        public static List<BattlePrepareRuntimeFixtureItemRow> BuildRequiredFixtureItems()
        {
            return new List<BattlePrepareRuntimeFixtureItemRow>
            {
                FixtureItem(
                    "v04_runtime_fixture_single1",
                    "Single1 测试符",
                    "Single1",
                    1,
                    false,
                    "点击后显示单格合法反馈。"),
                FixtureItem(
                    "v04_runtime_fixture_vertical2",
                    "Vertical2 测试符",
                    "Vertical2",
                    2,
                    true,
                    "点击后按 R 或旋转按钮测试竖二格旋转。"),
                FixtureItem(
                    "v04_runtime_fixture_corner3",
                    "Corner3 测试符",
                    "Corner3",
                    3,
                    true,
                    "点击后测试拐角三格、重叠和越界反馈。"),
                FixtureItem(
                    "v04_runtime_fixture_square4",
                    "Square4 测试符",
                    "Square4",
                    4,
                    false,
                    "点击后测试方四格占格预览。")
            };
        }

        public static List<BattlePrepareRuntimeFixtureFeedbackRow> BuildRequiredFixtureFeedback()
        {
            return new List<BattlePrepareRuntimeFixtureFeedbackRow>
            {
                FixtureFeedback(
                    "rotation",
                    "旋转",
                    "Select Vertical2 or Corner3, then press R or click Rotate Shape.",
                    "旋转：90 / 180 / 270。"),
                FixtureFeedback(
                    "occupancyPreview",
                    "占格预览",
                    "Hover or drag any fixture over the mature board.",
                    "绿色或红色半透明格显示当前占用。"),
                FixtureFeedback(
                    "validPlacement",
                    "合法放置反馈",
                    "Hover a fixture over enough empty in-grid cells.",
                    "可以放置：形状名，占用 N 格。"),
                FixtureFeedback(
                    "outOfGrid",
                    "越界反馈",
                    "Hover Vertical2, Corner3, or Square4 near the board edge so one cell leaves the board.",
                    "越界：有格子超出成熟棋盘。"),
                FixtureFeedback(
                    "overlap",
                    "重叠",
                    "Hover a fixture over an occupied mature board cell.",
                    "重叠：目标格已有成熟道具。")
            };
        }

        public static List<BattlePrepareShapeAwareItemTrayRow> BuildRequiredShapeAwareItemTrayRows()
        {
            return new List<BattlePrepareShapeAwareItemTrayRow>
            {
                ShapeAwareRow("Single1", "1格", 1, 1, "0,0", 0, 4),
                ShapeAwareRow("Vertical2", "2格竖形", 1, 2, "0,0;0,1", 1, 10),
                ShapeAwareRow("Corner3", "2x2缺1格拐角", 2, 2, "0,0;1,0;0,1", 2, 15),
                ShapeAwareRow("Square4", "2x2方块", 2, 2, "0,0;1,0;0,1;1,1", 8, 20)
            };
        }

        private static BattlePrepareRuntimeFixtureItemRow FixtureItem(
            string itemId,
            string displayName,
            string shapeId,
            int cellCount,
            bool rotationAllowed,
            string manualProbe)
        {
            return new BattlePrepareRuntimeFixtureItemRow
            {
                itemId = itemId,
                displayName = displayName,
                shapeId = shapeId,
                cellCount = cellCount,
                rotationAllowed = rotationAllowed,
                manualProbe = manualProbe,
                trayVisible = true,
                devOnly = true,
                isEnabled = false,
                canDrop = false,
                writesFormalConfig = false,
                writesSaveData = false
            };
        }

        private static BattlePrepareRuntimeFixtureFeedbackRow FixtureFeedback(
            string feedbackId,
            string chineseDisplayName,
            string trigger,
            string expectedChineseFeedback)
        {
            return new BattlePrepareRuntimeFixtureFeedbackRow
            {
                feedbackId = feedbackId,
                chineseDisplayName = chineseDisplayName,
                trigger = trigger,
                expectedChineseFeedback = expectedChineseFeedback,
                required = true
            };
        }

        private static BattlePrepareShapeAwareItemTrayRow ShapeAwareRow(
            string shapeId,
            string trayFootprintChinese,
            int footprintWidth,
            int footprintHeight,
            string occupiedOffsets,
            int initialAnchorSlotIndex,
            int trayMoveProbeAnchorSlotIndex)
        {
            List<ItemShapeCell> offsets = ShapeAwareTrayPackingMap.ParseOffsets(occupiedOffsets);
            return new BattlePrepareShapeAwareItemTrayRow
            {
                shapeId = shapeId,
                trayFootprintChinese = trayFootprintChinese,
                footprintWidth = footprintWidth,
                footprintHeight = footprintHeight,
                occupiedOffsets = occupiedOffsets,
                initialAnchorSlotIndex = initialAnchorSlotIndex,
                initialOccupiedSlots = ShapeAwareTrayPackingMap.FormatOccupiedSlots(
                    ShapeAwareTrayPackingMap.BuildOccupiedSlotIndexes(
                        initialAnchorSlotIndex,
                        ShapeAwareTrayPackingMap.DefaultColumnCount,
                        ShapeAwareTrayPackingMap.DefaultSlotCount,
                        offsets)),
                trayMoveProbeAnchorSlotIndex = trayMoveProbeAnchorSlotIndex,
                trayMoveProbeOccupiedSlots = ShapeAwareTrayPackingMap.FormatOccupiedSlots(
                    ShapeAwareTrayPackingMap.BuildOccupiedSlotIndexes(
                        trayMoveProbeAnchorSlotIndex,
                        ShapeAwareTrayPackingMap.DefaultColumnCount,
                        ShapeAwareTrayPackingMap.DefaultSlotCount,
                        offsets)),
                initialPackingInBounds = true,
                initialPackingHasOverlap = false,
                trayPackingUsesOccupiedCells = true,
                trayDragStatePersists = true,
                illegalTrayDropReturnsToLastLegal = true,
                boardPreviewDragDoesNotReturnToWrongInitialPosition = true,
                boardCommitMode = DeferredBoardCommitMode,
                itemTrayShowsTrueShape = true,
                selectedHighlightCoversShape = true,
                dragVisualKeepsShape = true,
                ghostPreviewMatchesTrayShape = true,
                usesMatureItemTray = true,
                redrawsItemTray = false,
                writesFormalConfig = false,
                writesSaveData = false
            };
        }

        private static BattlePrepareComponentAdapterRuntimeSurfaceRow Surface(
            string surfaceId,
            string chineseDisplayName,
            string matureSource,
            string runtimeEvidence,
            bool touchableInPlay)
        {
            return new BattlePrepareComponentAdapterRuntimeSurfaceRow
            {
                surfaceId = surfaceId,
                chineseDisplayName = chineseDisplayName,
                matureSource = matureSource,
                runtimeEvidence = runtimeEvidence,
                touchableInPlay = touchableInPlay,
                extensionOnly = true,
                writesFormalUi = false,
                modifiesFormalScene = false,
                overwritesRectTransform = false
            };
        }

        private static BattlePrepareComponentAdapterRuntimeStepRow Step(
            string stepId,
            string action,
            string expected)
        {
            return new BattlePrepareComponentAdapterRuntimeStepRow
            {
                stepId = stepId,
                action = action,
                expected = expected
            };
        }
    }

    [Serializable]
    public sealed class BattlePrepareComponentAdapterRuntimeSurfaceRow
    {
        public string surfaceId = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string matureSource = string.Empty;
        public string runtimeEvidence = string.Empty;
        public bool touchableInPlay;
        public bool extensionOnly = true;
        public bool writesFormalUi;
        public bool modifiesFormalScene;
        public bool overwritesRectTransform;
    }

    [Serializable]
    public sealed class BattlePrepareComponentAdapterRuntimeStepRow
    {
        public string stepId = string.Empty;
        public string action = string.Empty;
        public string expected = string.Empty;
    }

    [Serializable]
    public sealed class BattlePrepareRuntimeFixtureItemRow
    {
        public string itemId = string.Empty;
        public string displayName = string.Empty;
        public string shapeId = string.Empty;
        public int cellCount;
        public bool rotationAllowed;
        public bool trayVisible;
        public bool devOnly = true;
        public bool isEnabled;
        public bool canDrop;
        public bool writesFormalConfig;
        public bool writesSaveData;
        public string manualProbe = string.Empty;
    }

    [Serializable]
    public sealed class BattlePrepareRuntimeFixtureFeedbackRow
    {
        public string feedbackId = string.Empty;
        public string chineseDisplayName = string.Empty;
        public string trigger = string.Empty;
        public string expectedChineseFeedback = string.Empty;
        public bool required = true;
    }

    [Serializable]
    public sealed class BattlePrepareShapeAwareItemTrayRow
    {
        public string shapeId = string.Empty;
        public string trayFootprintChinese = string.Empty;
        public int footprintWidth;
        public int footprintHeight;
        public string occupiedOffsets = string.Empty;
        public int initialAnchorSlotIndex = -1;
        public string initialOccupiedSlots = string.Empty;
        public int trayMoveProbeAnchorSlotIndex = -1;
        public string trayMoveProbeOccupiedSlots = string.Empty;
        public bool initialPackingInBounds;
        public bool initialPackingHasOverlap;
        public bool trayPackingUsesOccupiedCells;
        public bool trayDragStatePersists;
        public bool illegalTrayDropReturnsToLastLegal;
        public bool boardPreviewDragDoesNotReturnToWrongInitialPosition;
        public string boardCommitMode = string.Empty;
        public bool itemTrayShowsTrueShape;
        public bool selectedHighlightCoversShape;
        public bool dragVisualKeepsShape;
        public bool ghostPreviewMatchesTrayShape;
        public bool usesMatureItemTray = true;
        public bool redrawsItemTray;
        public bool writesFormalConfig;
        public bool writesSaveData;
    }

    [Serializable]
    public sealed class ShapeAwareTrayPlacementState
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public int currentAnchorSlotIndex = -1;
        public int lastLegalAnchorSlotIndex = -1;
        public string currentOccupiedSlots = string.Empty;
        public string lastLegalOccupiedSlots = string.Empty;
        public bool dragStartedFromTray;

        public void SetLegalAnchor(int anchorSlotIndex, IEnumerable<int> occupiedSlotIndexes)
        {
            string occupiedSlots = ShapeAwareTrayPackingMap.FormatOccupiedSlots(occupiedSlotIndexes);
            currentAnchorSlotIndex = anchorSlotIndex;
            lastLegalAnchorSlotIndex = anchorSlotIndex;
            currentOccupiedSlots = occupiedSlots;
            lastLegalOccupiedSlots = occupiedSlots;
        }
    }

    public static class ShapeAwareTrayPackingMap
    {
        public const int DefaultColumnCount = 5;
        public const int DefaultSlotCount = 40;

        public static List<ItemShapeCell> ParseOffsets(string occupiedOffsets)
        {
            List<ItemShapeCell> offsets = new();
            if (string.IsNullOrWhiteSpace(occupiedOffsets))
            {
                offsets.Add(new ItemShapeCell(0, 0));
                return offsets;
            }

            foreach (string token in occupiedOffsets.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = token.Split(',');
                if (parts.Length != 2
                    || !int.TryParse(parts[0], out int x)
                    || !int.TryParse(parts[1], out int y))
                {
                    continue;
                }

                offsets.Add(new ItemShapeCell(x, y));
            }

            if (offsets.Count == 0)
            {
                offsets.Add(new ItemShapeCell(0, 0));
            }

            return offsets;
        }

        public static List<int> BuildOccupiedSlotIndexes(
            int anchorSlotIndex,
            int columnCount,
            int slotCount,
            IEnumerable<ItemShapeCell> offsets)
        {
            List<int> occupied = new();
            if (anchorSlotIndex < 0 || columnCount <= 0 || slotCount <= 0)
            {
                return occupied;
            }

            int anchorColumn = anchorSlotIndex % columnCount;
            int anchorRow = anchorSlotIndex / columnCount;
            foreach (ItemShapeCell offset in offsets ?? Enumerable.Empty<ItemShapeCell>())
            {
                int column = anchorColumn + offset.x;
                int row = anchorRow + offset.y;
                int index = row * columnCount + column;
                if (column >= 0 && column < columnCount && index >= 0 && index < slotCount)
                {
                    occupied.Add(index);
                }
            }

            return occupied;
        }

        public static bool CanFit(
            int anchorSlotIndex,
            int columnCount,
            int slotCount,
            IEnumerable<ItemShapeCell> offsets,
            ISet<int> reservedSlotIndexes,
            out string invalidReason)
        {
            invalidReason = string.Empty;
            if (anchorSlotIndex < 0 || anchorSlotIndex >= slotCount || columnCount <= 0)
            {
                invalidReason = "anchor-out-of-range";
                return false;
            }

            int anchorColumn = anchorSlotIndex % columnCount;
            int anchorRow = anchorSlotIndex / columnCount;
            foreach (ItemShapeCell offset in offsets ?? Enumerable.Empty<ItemShapeCell>())
            {
                int column = anchorColumn + offset.x;
                int row = anchorRow + offset.y;
                int index = row * columnCount + column;
                if (column < 0 || column >= columnCount || index < 0 || index >= slotCount)
                {
                    invalidReason = "out-of-bounds";
                    return false;
                }

                if (reservedSlotIndexes != null && reservedSlotIndexes.Contains(index))
                {
                    invalidReason = "overlap";
                    return false;
                }
            }

            return true;
        }

        public static string FormatOccupiedSlots(IEnumerable<int> occupiedSlotIndexes)
        {
            return string.Join(";", (occupiedSlotIndexes ?? Enumerable.Empty<int>()).OrderBy(index => index));
        }
    }

    public sealed class BattlePrepareComponentAdapterRuntimePlaytestController : MonoBehaviour
    {
        private const string RuntimeObjectName = "V04BattlePrepareRuntimePlaytestController";
        private const string ExtensionLayerName = "V04BattlePrepareRuntimePlaytestExtensionLayer";
        private const string FixtureViewNamePrefix = "V04RuntimeFixtureItem_";
        private const string MatureTrayRootName = "V03BattlePrepareItemTrayRoot";
        private const string MatureTraySlotPrefix = "ItemTrayGridSlot_";
        private const int MatureTrayColumnCount = 5;
        private const int BoardColumns = 5;
        private const int BoardRows = 5;
        private static readonly Vector2 MatureTraySlotSizeFallback = new(104f, 104f);
        private static readonly Vector2 MatureTraySlotSpacingFallback = new(14f, 14f);

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool manualLaunchRequested = true;

        private readonly Dictionary<Vector2Int, TalismanGridSlotView> boardSlots = new();
        private readonly List<Image> highlightCells = new();
        private readonly List<MobileShapePlacementGhostTarget> highlightTargets = new();
        private readonly List<ItemShapeConfig> runtimeShapeConfigs = new();
        private readonly List<TalismanItemDefinition> runtimeFixtureDefinitions = new();
        private readonly List<DraggableTalismanItemView> runtimeFixtureViews = new();
        private readonly Dictionary<string, string> fixtureShapeByItemId = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ShapeAwareTrayPlacementState> trayPlacementStates = new(StringComparer.OrdinalIgnoreCase);
        private BattlePrepareComponentAdapter adapter;
        private Canvas rootCanvas;
        private RectTransform canvasRect;
        private RectTransform extensionRoot;
        private Text statusText;
        private Text feedbackText;
        private Text itemInfoText;
        private Button rotateButton;
        private Button nextShapeButton;
        private Button cancelButton;
        private DraggableTalismanItemView selectedItem;
        private ShapeAwareItemTrayGrid shapeAwareTrayGrid;
        private RuntimeBattlePrepareBoardShapeGridReceiver boardShapeReceiver;
        private MobileShapePlacementRuntimeIntegration mobilePlacementIntegration;
        private BattlePrepareShapePlacementSeamAdapter extensionSeamAdapter;
        private ShapePlacementResult lastGhostResult;
        private int selectedShapeIndex;
        private ItemShapeRotation selectedRotation = ItemShapeRotation.Rotation0;
        private float nextSlotRefreshTime;
        private bool maturePrepareOpened;
        private bool blocked;
        private float nextFixtureEnsureTime;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public bool ManualLaunchRequested => manualLaunchRequested;
        public bool MaturePrepareOpened => maturePrepareOpened;
        public bool Blocked => blocked;

        public static BattlePrepareComponentAdapterRuntimePlaytestController InstallRuntimePlaytest()
        {
            BattlePrepareComponentAdapterRuntimePlaytestController existing =
                FindObjectOfType<BattlePrepareComponentAdapterRuntimePlaytestController>(true);
            if (existing != null)
            {
                existing.manualLaunchRequested = true;
                existing.enabled = true;
                return existing;
            }

            GameObject host = new(RuntimeObjectName);
            host.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            DontDestroyOnLoad(host);
            BattlePrepareComponentAdapterRuntimePlaytestController controller =
                host.AddComponent<BattlePrepareComponentAdapterRuntimePlaytestController>();
            controller.manualLaunchRequested = true;
            return controller;
        }

        private void Awake()
        {
            if (!devOnly || isEnabled)
            {
                blocked = true;
                enabled = false;
                Debug.LogError("[BattlePrepareRuntimePlaytest01] Refusing to run because devOnly/isEnabled isolation is invalid.");
                return;
            }

            BuildRuntimeAdapter();
            BuildRuntimeShapeConfigs();
            InitializeMobileShapePlacementRuntime();
        }

        private IEnumerator Start()
        {
            if (blocked)
            {
                yield break;
            }

            for (int i = 0; i < 8; i++)
            {
                yield return null;
            }

            EnsureCanvasAndLayer();
            RefreshBoardSlots();
            yield return OpenMaturePrepareRoutine();
        }

        private void OnDestroy()
        {
            V03BattlePrepareInteractionController.ClearExtensionSeamProvider(extensionSeamAdapter);
            foreach (ItemShapeConfig shapeConfig in runtimeShapeConfigs)
            {
                if (shapeConfig == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(shapeConfig);
                }
                else
                {
                    DestroyImmediate(shapeConfig);
                }
            }

            runtimeShapeConfigs.Clear();
            runtimeFixtureViews.RemoveAll(view => view == null);
            foreach (DraggableTalismanItemView fixtureView in runtimeFixtureViews)
            {
                DestroyRuntimeObject(fixtureView.gameObject);
            }

            runtimeFixtureViews.Clear();
            foreach (TalismanItemDefinition definition in runtimeFixtureDefinitions)
            {
                DestroyRuntimeObject(definition);
            }

            runtimeFixtureDefinitions.Clear();
            fixtureShapeByItemId.Clear();
            trayPlacementStates.Clear();
        }

        private void Update()
        {
            if (blocked || extensionRoot == null)
            {
                return;
            }

            if (Time.unscaledTime >= nextSlotRefreshTime)
            {
                nextSlotRefreshTime = Time.unscaledTime + 0.5f;
                RefreshBoardSlots();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateSelectedShape();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SelectNextShape();
            }

            if (maturePrepareOpened && Time.unscaledTime >= nextFixtureEnsureTime)
            {
                nextFixtureEnsureTime = Time.unscaledTime + 0.75f;
                EnsureFixtureItemsInjected(false);
            }

            UpdatePlacementPreview();
        }

        private IEnumerator OpenMaturePrepareRoutine()
        {
            bool requested = false;
            for (int attempt = 0; attempt < 40; attempt++)
            {
                requested = V03BattlePrepareInteractionController.TryOpenPrepareThen(OnMaturePrepareOpened);
                if (requested)
                {
                    SetStatus("Runtime playtest launched. Mature BattlePrepare is opening.");
                    yield break;
                }

                yield return null;
            }

            blocked = true;
            SetStatus("RUNTIME_PLAYTEST_BLOCKED: Missing mature V03 BattlePrepare controller.");
            SetFeedback("未找到成熟整备控制器。请确认已打开 V02 FormationCounter 并进入 Play。", true);
        }

        private void OnMaturePrepareOpened()
        {
            maturePrepareOpened = true;
            EnsureCanvasAndLayer();
            RefreshBoardSlots();
            InitializeMobileShapePlacementRuntime();
            V03BattlePrepareInteractionController.TryInjectExtensionSeamProvider(extensionSeamAdapter);
            EnsureFixtureItemsInjected(true);
            SetStatus("Runtime playtest active: mature BattlePrepare + V0.4 extension layer.");
            SetFeedback("点击或拖动道具栏中的 Single1 / Vertical2 / Corner3 / Square4，再按 R 旋转或把鼠标移到棋盘上查看占格反馈。", false);
            UpdateSelectedItemInfo();
        }

        private void BuildRuntimeAdapter()
        {
            BuildSandboxPreviewContext context = BuildRuntimePreviewContext();
            adapter = BattlePrepareComponentAdapter.Build(context, BattlePageViewAdapter.CreateDefault());
        }

        private static BuildSandboxPreviewContext BuildRuntimePreviewContext()
        {
            BuildSandboxLayoutSnapshot layout = new();
            layout.placedItems.Add(Placed(
                "runtime_fire_talisman",
                "Single1",
                new ItemShapeCell(1, 1),
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(1, 1) },
                new[] { "talisman", "fire" },
                new[] { "orange_core_fire" },
                "orange",
                true));
            layout.placedItems.Add(Placed(
                "runtime_guard_wood",
                "Vertical2",
                new ItemShapeCell(3, 1),
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(3, 1), new ItemShapeCell(3, 2) },
                new[] { "tool", "guard" },
                new[] { "bond_plus_one" },
                "blue",
                true));
            layout.placedItems.Add(Placed(
                "runtime_cleanse_corner",
                "Corner3",
                new ItemShapeCell(0, 3),
                ItemShapeRotation.Rotation90,
                new[] { new ItemShapeCell(0, 3), new ItemShapeCell(0, 4), new ItemShapeCell(1, 4) },
                new[] { "talisman", "cleanse" },
                new[] { "cleanse_ready" },
                "purple",
                false));
            layout.placedItems.Add(Placed(
                "runtime_square_fixture",
                "Square4",
                new ItemShapeCell(3, 3),
                ItemShapeRotation.Rotation0,
                new[] { new ItemShapeCell(3, 3), new ItemShapeCell(4, 3), new ItemShapeCell(3, 4), new ItemShapeCell(4, 4) },
                new[] { "tool", "fixture" },
                new[] { "square_anchor" },
                "blue",
                true));

            List<ShapePlacementResult> placementSamples = new()
            {
                new ShapePlacementResult(
                    "runtime_fire_talisman",
                    "Single1",
                    new ItemShapeCell(1, 1),
                    new[] { new ItemShapeCell(1, 1) },
                    true,
                    ShapePlacementInvalidReason.None,
                    energyConnected: true),
                new ShapePlacementResult(
                    "runtime_overlap_probe",
                    "Corner3",
                    new ItemShapeCell(1, 1),
                    new[] { new ItemShapeCell(1, 1), new ItemShapeCell(2, 1), new ItemShapeCell(1, 2) },
                    false,
                    ShapePlacementInvalidReason.CellOccupied),
                new ShapePlacementResult(
                    "runtime_out_of_grid_probe",
                    "Square4",
                    new ItemShapeCell(4, 4),
                    new[] { new ItemShapeCell(4, 4), new ItemShapeCell(5, 4), new ItemShapeCell(4, 5), new ItemShapeCell(5, 5) },
                    false,
                    ShapePlacementInvalidReason.OutOfGrid)
            };

            BuildEvaluationResult buildEvaluation = new()
            {
                activeThresholds = new List<string> { "fire_2", "guard_2" },
                sourceItems = new List<string> { "runtime_fire_talisman", "runtime_guard_wood", "runtime_square_fixture" },
                placementSatisfied = true,
                energySatisfied = true,
                activeSynergies = new List<ActiveSynergyResult>
                {
                    new()
                    {
                        synergyId = "fire_guard_runtime",
                        displayName = "火势护阵",
                        matchedCount = 2,
                        activeThresholds = new List<string> { "fire_2" },
                        sourceItems = new List<string> { "runtime_fire_talisman", "runtime_guard_wood", "runtime_square_fixture" },
                        placementSatisfied = true,
                        energySatisfied = true
                    }
                }
            };

            AffixRarityEvaluationResult affixEvaluation = new()
            {
                rarityIds = new List<string> { "blue", "purple", "orange" },
                affixIds = new List<string> { "orange_core_fire", "bond_plus_one", "cleanse_ready", "square_anchor" },
                itemResults = new List<AffixRarityItemResult>
                {
                    new()
                    {
                        itemId = "runtime_fire_talisman",
                        rarityId = "orange",
                        selectedAffixes = new List<string> { "orange_core_fire" },
                        sourceTags = new List<string> { "fire", "talisman" }
                    },
                    new()
                    {
                        itemId = "runtime_square_fixture",
                        rarityId = "blue",
                        selectedAffixes = new List<string> { "square_anchor" },
                        sourceTags = new List<string> { "tool", "fixture" }
                    }
                }
            };

            return BuildSandboxPreviewContextBuilder.Build(new BuildSandboxPreviewContextBuildInput
            {
                previewBuildId = "battleprepare_runtime_playtest_context",
                layoutSnapshot = layout,
                shapePlacementResults = placementSamples,
                buildEvaluation = buildEvaluation,
                affixEvaluation = affixEvaluation
            });
        }

        private static BuildSandboxPlacedItemSnapshot Placed(
            string itemId,
            string shapeId,
            ItemShapeCell anchor,
            ItemShapeRotation rotation,
            IEnumerable<ItemShapeCell> cells,
            IEnumerable<string> tags,
            IEnumerable<string> affixes,
            string rarity,
            bool powered)
        {
            return new BuildSandboxPlacedItemSnapshot
            {
                itemId = itemId,
                shapeId = shapeId,
                anchorCell = anchor,
                rotation = rotation,
                occupiedCells = cells.ToList(),
                tags = tags.ToList(),
                affixList = affixes.ToList(),
                rarity = rarity,
                isPowered = powered,
                energySourceId = powered ? "runtime_playtest_energy" : "runtime_playtest_unpowered"
            };
        }

        private void BuildRuntimeShapeConfigs()
        {
            runtimeShapeConfigs.Clear();
            runtimeShapeConfigs.Add(CreateShape("Single1", "单格", false, new ItemShapeCell(0, 0)));
            runtimeShapeConfigs.Add(CreateShape("Vertical2", "竖二格", true, new ItemShapeCell(0, 0), new ItemShapeCell(0, 1)));
            runtimeShapeConfigs.Add(CreateShape("Corner3", "拐角三格", true, new ItemShapeCell(0, 0), new ItemShapeCell(1, 0), new ItemShapeCell(0, 1)));
            runtimeShapeConfigs.Add(CreateShape("Square4", "方四格", false, new ItemShapeCell(0, 0), new ItemShapeCell(1, 0), new ItemShapeCell(0, 1), new ItemShapeCell(1, 1)));
        }

        private static ItemShapeConfig CreateShape(
            string shapeId,
            string displayName,
            bool rotationAllowed,
            params ItemShapeCell[] occupiedOffsets)
        {
            ItemShapeConfig config = ScriptableObject.CreateInstance<ItemShapeConfig>();
            config.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            config.configId = "runtime_playtest_" + shapeId;
            config.shapeId = shapeId;
            config.shapeName = displayName;
            config.visualKey = "runtime_playtest_" + shapeId;
            config.rotationAllowed = rotationAllowed;
            config.defaultRotation = ItemShapeRotation.Rotation0;
            config.occupiedOffsets = occupiedOffsets.ToList();
            config.cellCount = config.occupiedOffsets.Count;
            config.devOnly = true;
            config.isEnabled = false;
            return config;
        }

        private void InitializeMobileShapePlacementRuntime()
        {
            shapeAwareTrayGrid ??= new ShapeAwareItemTrayGrid(
                columnCount: ShapeAwareTrayPackingMap.DefaultColumnCount,
                slotCount: ShapeAwareTrayPackingMap.DefaultSlotCount,
                commitAllowed: true);
            boardShapeReceiver ??= new RuntimeBattlePrepareBoardShapeGridReceiver(
                () => boardSlots,
                () => selectedItem);
            mobilePlacementIntegration ??= new MobileShapePlacementRuntimeIntegration(
                shapeAwareTrayGrid,
                boardShapeReceiver);
            mobilePlacementIntegration.BindRuntimePlaytest(shapeAwareTrayGrid, boardShapeReceiver);
            extensionSeamAdapter ??= new BattlePrepareShapePlacementSeamAdapter();
            extensionSeamAdapter.BindRuntimeIntegration(
                mobilePlacementIntegration,
                shapeAwareTrayGrid,
                boardShapeReceiver,
                BuildSeamShapePayload);
            extensionSeamAdapter.BindRuntimeCallbacks(
                HandleSeamGhostPreview,
                HandleSeamPlacementCancelled);
            V03BattlePrepareInteractionController.TryInjectExtensionSeamProvider(extensionSeamAdapter);
        }

        private void EnsureCanvasAndLayer()
        {
            if (rootCanvas == null)
            {
                rootCanvas = FindObjectOfType<Canvas>();
                canvasRect = rootCanvas != null ? rootCanvas.transform as RectTransform : null;
            }

            if (rootCanvas == null || canvasRect == null || extensionRoot != null)
            {
                return;
            }

            GameObject root = new(ExtensionLayerName, typeof(RectTransform));
            root.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            root.transform.SetParent(rootCanvas.transform, false);
            extensionRoot = root.GetComponent<RectTransform>();
            Stretch(extensionRoot);

            for (int i = 0; i < 4; i++)
            {
                Image image = CreateImage($"OccupiedCellPreview_{i:00}", extensionRoot, new Color(0.18f, 0.82f, 0.32f, 0.34f));
                image.raycastTarget = false;
                MobileShapePlacementGhostTarget target = image.gameObject.GetComponent<MobileShapePlacementGhostTarget>();
                if (target == null)
                {
                    target = image.gameObject.AddComponent<MobileShapePlacementGhostTarget>();
                }

                target.Bind(this);
                image.gameObject.SetActive(false);
                highlightCells.Add(image);
                highlightTargets.Add(target);
            }

            CreateControlPanel();
        }

        private void CreateControlPanel()
        {
            GameObject panel = new("RuntimePlaytestControlPanel", typeof(RectTransform), typeof(Image));
            panel.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            panel.transform.SetParent(extensionRoot, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.anchoredPosition = new Vector2(-24f, -24f);
            panelRect.sizeDelta = new Vector2(420f, 230f);
            panel.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.075f, 0.88f);

            statusText = CreateText("StatusText", panel.transform, "Runtime playtest booting...", 18, FontStyle.Bold, TextAnchor.UpperLeft);
            SetRect(statusText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(-28f, 62f));

            itemInfoText = CreateText("ItemInfoText", panel.transform, "Item: none", 16, FontStyle.Normal, TextAnchor.UpperLeft);
            SetRect(itemInfoText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -108f), new Vector2(-28f, 68f));

            feedbackText = CreateText("FeedbackText", panel.transform, "Hover board cells to preview V0.4 occupancy.", 16, FontStyle.Normal, TextAnchor.UpperLeft);
            SetRect(feedbackText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 58f), new Vector2(-28f, 66f));

            nextShapeButton = CreateButton("NextShapeButton", panel.transform, "切换形状");
            SetRect((RectTransform)nextShapeButton.transform, new Vector2(0f, 0f), new Vector2(0.33f, 0f), new Vector2(0.5f, 0f), new Vector2(78f, 22f), new Vector2(128f, 42f));
            nextShapeButton.onClick.AddListener(SelectNextShape);

            rotateButton = CreateButton("RotateShapeButton", panel.transform, "旋转形状");
            SetRect((RectTransform)rotateButton.transform, new Vector2(0.33f, 0f), new Vector2(0.66f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(128f, 42f));
            rotateButton.onClick.AddListener(RotateSelectedShape);

            cancelButton = CreateButton("CancelShapePlacementButton", panel.transform, "取消");
            SetRect((RectTransform)cancelButton.transform, new Vector2(0.66f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(-78f, 22f), new Vector2(128f, 42f));
            cancelButton.onClick.AddListener(CancelMobileShapePlacement);
        }

        private void EnsureFixtureItemsInjected(bool reportIfBlocked)
        {
            if (blocked || rootCanvas == null)
            {
                return;
            }

            List<BattlePrepareRuntimeFixtureItemRow> requiredFixtures =
                BattlePrepareComponentAdapterRuntimePlaytestPlan.BuildRequiredFixtureItems();
            runtimeFixtureViews.RemoveAll(view => view == null);

            RectTransform trayRoot = GameObject.Find(MatureTrayRootName)?.transform as RectTransform;
            if (trayRoot == null)
            {
                if (reportIfBlocked)
                {
                    SetFeedback("未找到成熟道具栏根节点，暂不能注入 V0.4 devOnly 测试道具。", true);
                }

                return;
            }

            List<RectTransform> traySlots = FindMatureTraySlots(trayRoot);
            if (traySlots.Count == 0)
            {
                if (reportIfBlocked)
                {
                    SetFeedback("成熟道具栏没有可用格子，暂不能注入 V0.4 devOnly 测试道具。", true);
                }

                return;
            }

            EnsureFixtureDefinitions(requiredFixtures);
            RectTransform trayContent = traySlots[0].parent as RectTransform;
            if (trayContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(trayContent);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(trayRoot);

            Dictionary<string, IReadOnlyList<ItemShapeCell>> shapeOffsetsByItemId =
                requiredFixtures.ToDictionary(
                    row => row.itemId,
                    row => (IReadOnlyList<ItemShapeCell>)ResolveFixtureShapeOffsets(row.shapeId),
                    StringComparer.OrdinalIgnoreCase);
            RebuildShapeAwareTrayGrid(requiredFixtures);
            Dictionary<string, BattlePrepareShapeAwareItemTrayRow> shapeRowsByShapeId =
                BattlePrepareComponentAdapterRuntimePlaytestPlan.BuildRequiredShapeAwareItemTrayRows()
                .ToDictionary(row => row.shapeId, StringComparer.OrdinalIgnoreCase);
            HashSet<int> resolvedReservedSlotIndexes = new();
            foreach (BattlePrepareRuntimeFixtureItemRow row in requiredFixtures)
            {
                TalismanItemDefinition definition =
                    runtimeFixtureDefinitions.FirstOrDefault(item => item != null && item.itemId == row.itemId);
                if (definition == null)
                {
                    continue;
                }

                DraggableTalismanItemView view = FindRuntimeFixtureView(row.itemId);
                IReadOnlyList<ItemShapeCell> shapeOffsets = shapeOffsetsByItemId[row.itemId];
                HashSet<int> reservedByOtherFixtures =
                    BuildReservedFixtureSlotsExcept(row.itemId, shapeOffsetsByItemId, traySlots);
                reservedByOtherFixtures.UnionWith(resolvedReservedSlotIndexes);
                int slotIndex = ResolveFixtureSlotIndex(
                    row,
                    shapeRowsByShapeId,
                    view,
                    shapeOffsets,
                    traySlots,
                    reservedByOtherFixtures);

                if (slotIndex < 0)
                {
                    if (reportIfBlocked)
                    {
                        SetFeedback("成熟道具栏缺少可容纳真实形状的连续空格，V0.4 devOnly 测试道具未能全部形状化显示。", true);
                    }

                    return;
                }

                if (view == null)
                {
                    view = CreateFixtureItemView(row, definition, traySlots[slotIndex], trayRoot);
                    runtimeFixtureViews.Add(view);
                }

                if (!view.IsDragging)
                {
                    ApplyShapeAwareFixtureLayout(view, row, shapeOffsets, traySlots[slotIndex], slotIndex, trayRoot);
                }

                UpdateTrayPlacementState(row, shapeOffsets, slotIndex, traySlots.Count);
                ReserveFixtureSlots(slotIndex, shapeOffsets, traySlots.Count, resolvedReservedSlotIndexes);
            }

            if (runtimeFixtureViews.Count > 0)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(trayRoot);
            }
        }

        private void RebuildShapeAwareTrayGrid(IReadOnlyList<BattlePrepareRuntimeFixtureItemRow> requiredFixtures)
        {
            InitializeMobileShapePlacementRuntime();
            shapeAwareTrayGrid.Clear();
            foreach (BattlePrepareRuntimeFixtureItemRow row in requiredFixtures ?? Array.Empty<BattlePrepareRuntimeFixtureItemRow>())
            {
                ItemShapeConfig shape = runtimeShapeConfigs.FirstOrDefault(
                    config => config != null && string.Equals(config.shapeId, row.shapeId, StringComparison.OrdinalIgnoreCase));
                ShapeItemPayload payload = mobilePlacementIntegration.BuildPayload(
                    row.itemId,
                    shape,
                    ItemShapeRotation.Rotation0,
                    ShapePlacementSource.Tray);
                mobilePlacementIntegration.PackTrayPayload(payload);
            }
        }

        private void EnsureFixtureDefinitions(IReadOnlyList<BattlePrepareRuntimeFixtureItemRow> requiredFixtures)
        {
            if (runtimeFixtureDefinitions.Count > 0)
            {
                return;
            }

            foreach (BattlePrepareRuntimeFixtureItemRow row in requiredFixtures)
            {
                TalismanItemDefinition definition = ScriptableObject.CreateInstance<TalismanItemDefinition>();
                definition.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                definition.name = "V04RuntimeFixtureDefinition_" + row.shapeId;
                definition.itemId = row.itemId;
                definition.displayName = row.displayName;
                definition.enabled = false;
                definition.itemType = ResolveFixtureItemType(row.shapeId);
                definition.uiColor = ResolveFixtureColor(row.shapeId);
                definition.baseCooldown = 1f;
                definition.baseValue = 0;
                definition.manaCost = 0;
                definition.width = 1;
                definition.height = 1;
                definition.description = "V0.4 runtime playtest devOnly fixture. Not saved, not dropped, not part of formal item pool.";
                definition.shortRoleDescription = row.manualProbe;
                definition.canDrop = false;
                definition.canUpgrade = false;
                definition.isDebugOnly = true;
                definition.unlockChapter = 999;
                runtimeFixtureDefinitions.Add(definition);
                fixtureShapeByItemId[definition.itemId] = row.shapeId;
            }
        }

        private DraggableTalismanItemView CreateFixtureItemView(
            BattlePrepareRuntimeFixtureItemRow row,
            TalismanItemDefinition definition,
            RectTransform slot,
            RectTransform trayRoot)
        {
            GameObject itemObject = new(
                FixtureViewNamePrefix + row.shapeId,
                typeof(RectTransform),
                typeof(Image),
                typeof(LayoutElement),
                typeof(CanvasGroup),
                typeof(DraggableTalismanItemView));
            itemObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            itemObject.transform.SetParent(slot, false);

            RectTransform rect = itemObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(94f, 94f);
            rect.localScale = Vector3.one;

            Image image = itemObject.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = false;
            itemObject.AddComponent<ShapeAwareItemTrayFixtureView>();

            DraggableTalismanItemView view = itemObject.GetComponent<DraggableTalismanItemView>();
            TalismanBagGrid grid = FindObjectOfType<TalismanBagGrid>(true);
            AutoCombatController combat = FindObjectOfType<AutoCombatController>(true);
            view.BindRuntime(new TalismanItemRuntime(definition), grid, rootCanvas, combat);
            view.SetInventoryDropZone(trayRoot);
            view.CaptureHome();
            return view;
        }

        private DraggableTalismanItemView FindRuntimeFixtureView(string itemId)
        {
            return runtimeFixtureViews.FirstOrDefault(
                view => view != null
                    && view.Definition != null
                    && string.Equals(view.Definition.itemId, itemId, StringComparison.Ordinal));
        }

        private int ResolveFixtureSlotIndex(
            BattlePrepareRuntimeFixtureItemRow row,
            IReadOnlyDictionary<string, BattlePrepareShapeAwareItemTrayRow> shapeRowsByShapeId,
            DraggableTalismanItemView view,
            IReadOnlyList<ItemShapeCell> shapeOffsets,
            IReadOnlyList<RectTransform> traySlots,
            HashSet<int> reservedSlotIndexes)
        {
            BattlePrepareShapeAwareItemTrayRow shapeRow = null;
            shapeRowsByShapeId?.TryGetValue(row.shapeId, out shapeRow);
            ShapeAwareTrayPlacementState state = EnsureTrayPlacementState(row, shapeRow);
            if (shapeAwareTrayGrid != null
                && shapeAwareTrayGrid.TryGetPlacement(row.itemId, out ShapeAwareItemTrayGridPlacement gridPlacement))
            {
                int gridSlotIndex = shapeAwareTrayGrid.CellToSlotIndex(gridPlacement.AnchorCell);
                if (CanFitFixtureFootprint(gridSlotIndex, traySlots, shapeOffsets, reservedSlotIndexes))
                {
                    return gridSlotIndex;
                }
            }

            int directTraySlotIndex = FindCurrentDirectTraySlotIndex(view, traySlots);
            if (directTraySlotIndex >= 0)
            {
                if (CanFitFixtureFootprint(directTraySlotIndex, traySlots, shapeOffsets, reservedSlotIndexes))
                {
                    return directTraySlotIndex;
                }

                SetFeedback("道具栏目标位置会导致真实形状越界或重叠，已回到上一次合法位置。", true);
            }

            if (CanFitFixtureFootprint(state.currentAnchorSlotIndex, traySlots, shapeOffsets, reservedSlotIndexes))
            {
                return state.currentAnchorSlotIndex;
            }

            if (CanFitFixtureFootprint(state.lastLegalAnchorSlotIndex, traySlots, shapeOffsets, reservedSlotIndexes))
            {
                return state.lastLegalAnchorSlotIndex;
            }

            if (shapeRow != null
                && CanFitFixtureFootprint(shapeRow.initialAnchorSlotIndex, traySlots, shapeOffsets, reservedSlotIndexes))
            {
                return shapeRow.initialAnchorSlotIndex;
            }

            return FindFirstFixtureSlotIndex(traySlots, shapeOffsets, reservedSlotIndexes);
        }

        private ShapeAwareTrayPlacementState EnsureTrayPlacementState(
            BattlePrepareRuntimeFixtureItemRow row,
            BattlePrepareShapeAwareItemTrayRow shapeRow)
        {
            if (!trayPlacementStates.TryGetValue(row.itemId, out ShapeAwareTrayPlacementState state))
            {
                state = new ShapeAwareTrayPlacementState
                {
                    itemId = row.itemId,
                    shapeId = row.shapeId,
                    currentAnchorSlotIndex = shapeRow != null ? shapeRow.initialAnchorSlotIndex : -1,
                    lastLegalAnchorSlotIndex = shapeRow != null ? shapeRow.initialAnchorSlotIndex : -1
                };
                trayPlacementStates[row.itemId] = state;
            }

            return state;
        }

        private HashSet<int> BuildReservedFixtureSlotsExcept(
            string exceptItemId,
            IReadOnlyDictionary<string, IReadOnlyList<ItemShapeCell>> shapeOffsetsByItemId,
            IReadOnlyList<RectTransform> traySlots)
        {
            HashSet<int> reservedSlotIndexes = new();
            foreach (KeyValuePair<string, ShapeAwareTrayPlacementState> pair in trayPlacementStates)
            {
                if (string.Equals(pair.Key, exceptItemId, StringComparison.OrdinalIgnoreCase)
                    || pair.Value == null
                    || !shapeOffsetsByItemId.TryGetValue(pair.Key, out IReadOnlyList<ItemShapeCell> shapeOffsets))
                {
                    continue;
                }

                int anchorSlotIndex = pair.Value.currentAnchorSlotIndex >= 0
                    ? pair.Value.currentAnchorSlotIndex
                    : pair.Value.lastLegalAnchorSlotIndex;
                if (!ShapeAwareTrayPackingMap.CanFit(
                        anchorSlotIndex,
                        MatureTrayColumnCount,
                        traySlots.Count,
                        shapeOffsets,
                        reservedSlotIndexes,
                        out _))
                {
                    continue;
                }

                ReserveFixtureSlots(anchorSlotIndex, shapeOffsets, traySlots.Count, reservedSlotIndexes);
            }

            return reservedSlotIndexes;
        }

        private void UpdateTrayPlacementState(
            BattlePrepareRuntimeFixtureItemRow row,
            IReadOnlyList<ItemShapeCell> shapeOffsets,
            int anchorSlotIndex,
            int slotCount)
        {
            ShapeAwareTrayPlacementState state = EnsureTrayPlacementState(
                row,
                BattlePrepareComponentAdapterRuntimePlaytestPlan.BuildRequiredShapeAwareItemTrayRows()
                .FirstOrDefault(shapeRow => string.Equals(shapeRow.shapeId, row.shapeId, StringComparison.OrdinalIgnoreCase)));
            state.SetLegalAnchor(
                anchorSlotIndex,
                ShapeAwareTrayPackingMap.BuildOccupiedSlotIndexes(
                    anchorSlotIndex,
                    MatureTrayColumnCount,
                    slotCount,
                    shapeOffsets));
        }

        private static int FindCurrentDirectTraySlotIndex(
            DraggableTalismanItemView view,
            IReadOnlyList<RectTransform> traySlots)
        {
            RectTransform parent = view != null ? view.transform.parent as RectTransform : null;
            if (parent == null)
            {
                return -1;
            }

            for (int index = 0; index < traySlots.Count; index++)
            {
                if (traySlots[index] == parent)
                {
                    return index;
                }
            }

            return -1;
        }

        private void ApplyShapeAwareFixtureLayout(
            DraggableTalismanItemView view,
            BattlePrepareRuntimeFixtureItemRow row,
            IReadOnlyList<ItemShapeCell> shapeOffsets,
            RectTransform slot,
            int slotIndex,
            RectTransform trayRoot)
        {
            if (view == null || row == null || slot == null)
            {
                return;
            }

            view.transform.SetParent(slot, false);

            ShapeAwareItemTrayFixtureView shapeView = view.GetComponent<ShapeAwareItemTrayFixtureView>();
            if (shapeView == null)
            {
                shapeView = view.gameObject.AddComponent<ShapeAwareItemTrayFixtureView>();
            }

            shapeView.Configure(
                row.shapeId,
                row.displayName,
                shapeOffsets,
                row.rotationAllowed,
                ResolveFixtureColor(row.shapeId),
                ResolveTraySlotSize(slot),
                MatureTraySlotSpacingFallback,
                Vector2.zero,
                slotIndex);
            view.SetInventoryDropZone(trayRoot);
            view.CaptureHome();
        }

        private List<ItemShapeCell> ResolveFixtureShapeOffsets(string shapeId)
        {
            ItemShapeConfig shape = runtimeShapeConfigs.FirstOrDefault(config => config != null && config.shapeId == shapeId);
            if (shape != null && shape.occupiedOffsets != null && shape.occupiedOffsets.Count > 0)
            {
                return shape.occupiedOffsets.Select(cell => new ItemShapeCell(cell.x, cell.y)).ToList();
            }

            return new List<ItemShapeCell> { new(0, 0) };
        }

        private static List<RectTransform> FindMatureTraySlots(RectTransform trayRoot)
        {
            return trayRoot
                .GetComponentsInChildren<RectTransform>(true)
                .Where(rect => rect != null && rect.name.StartsWith(MatureTraySlotPrefix, StringComparison.Ordinal))
                .OrderBy(ParseTraySlotOrder)
                .ToList();
        }

        private int FindFirstFixtureSlotIndex(
            IReadOnlyList<RectTransform> traySlots,
            IReadOnlyList<ItemShapeCell> shapeOffsets,
            HashSet<int> reservedSlotIndexes)
        {
            for (int index = 0; index < traySlots.Count; index++)
            {
                if (!CanFitFixtureFootprint(index, traySlots, shapeOffsets, reservedSlotIndexes))
                {
                    continue;
                }

                return index;
            }

            return -1;
        }

        private bool CanFitFixtureFootprint(
            int anchorIndex,
            IReadOnlyList<RectTransform> traySlots,
            IReadOnlyList<ItemShapeCell> shapeOffsets,
            HashSet<int> reservedSlotIndexes)
        {
            if (anchorIndex < 0 || anchorIndex >= traySlots.Count)
            {
                return false;
            }

            int anchorColumn = anchorIndex % MatureTrayColumnCount;
            int anchorRow = anchorIndex / MatureTrayColumnCount;
            foreach (ItemShapeCell offset in shapeOffsets)
            {
                int column = anchorColumn + offset.x;
                int row = anchorRow + offset.y;
                int index = row * MatureTrayColumnCount + column;
                if (column < 0
                    || column >= MatureTrayColumnCount
                    || index < 0
                    || index >= traySlots.Count
                    || reservedSlotIndexes.Contains(index)
                    || HasVisibleNonFixtureTrayItem(traySlots[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ReserveFixtureSlots(
            int anchorIndex,
            IReadOnlyList<ItemShapeCell> shapeOffsets,
            int slotCount,
            HashSet<int> reservedSlotIndexes)
        {
            int anchorColumn = anchorIndex % MatureTrayColumnCount;
            int anchorRow = anchorIndex / MatureTrayColumnCount;
            foreach (ItemShapeCell offset in shapeOffsets)
            {
                int column = anchorColumn + offset.x;
                int row = anchorRow + offset.y;
                int index = row * MatureTrayColumnCount + column;
                if (column >= 0 && column < MatureTrayColumnCount && index >= 0 && index < slotCount)
                {
                    reservedSlotIndexes.Add(index);
                }
            }
        }

        private bool HasVisibleNonFixtureTrayItem(RectTransform slot)
        {
            for (int i = 0; i < slot.childCount; i++)
            {
                DraggableTalismanItemView view = slot.GetChild(i).GetComponent<DraggableTalismanItemView>();
                if (view != null && view.gameObject.activeSelf && !IsRuntimeFixtureView(view))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsRuntimeFixtureView(DraggableTalismanItemView view)
        {
            string itemId = view != null && view.Definition != null ? view.Definition.itemId : string.Empty;
            return !string.IsNullOrWhiteSpace(itemId) && fixtureShapeByItemId.ContainsKey(itemId);
        }

        private static Vector2 ResolveTraySlotSize(RectTransform slot)
        {
            if (slot == null)
            {
                return MatureTraySlotSizeFallback;
            }

            Rect rect = slot.rect;
            if (rect.width > 1f && rect.height > 1f)
            {
                return new Vector2(rect.width, rect.height);
            }

            if (slot.sizeDelta.x > 1f && slot.sizeDelta.y > 1f)
            {
                return slot.sizeDelta;
            }

            return MatureTraySlotSizeFallback;
        }

        private static int ParseTraySlotOrder(RectTransform slot)
        {
            if (slot == null || !slot.name.StartsWith(MatureTraySlotPrefix, StringComparison.Ordinal))
            {
                return int.MaxValue;
            }

            string suffix = slot.name.Substring(MatureTraySlotPrefix.Length);
            return int.TryParse(suffix, out int order) ? order : int.MaxValue;
        }

        private static TalismanItemType ResolveFixtureItemType(string shapeId)
        {
            return shapeId switch
            {
                "Vertical2" => TalismanItemType.ShieldTalisman,
                "Corner3" => TalismanItemType.SupportTalisman,
                "Square4" => TalismanItemType.PassiveTool,
                _ => TalismanItemType.AttackTalisman
            };
        }

        private static Color ResolveFixtureColor(string shapeId)
        {
            return shapeId switch
            {
                "Vertical2" => new Color(0.44f, 0.78f, 1f, 0.96f),
                "Corner3" => new Color(1f, 0.78f, 0.36f, 0.96f),
                "Square4" => new Color(0.76f, 0.62f, 1f, 0.96f),
                _ => new Color(0.5f, 0.92f, 0.62f, 0.96f)
            };
        }

        private void RefreshBoardSlots()
        {
            boardSlots.Clear();
            foreach (TalismanGridSlotView slot in FindObjectsOfType<TalismanGridSlotView>(true))
            {
                if (slot != null)
                {
                    boardSlots[slot.GridPosition] = slot;
                }
            }
        }

        private void HandleSeamGhostPreview(
            BattlePrepareGhostPreviewContext context,
            ShapeItemPayload payload)
        {
            DraggableTalismanItemView view = context != null ? context.ItemView : null;
            if (view == null || !payload.IsValid)
            {
                return;
            }

            bool fromDrag = string.Equals(
                context.Message,
                "item_drag_started",
                StringComparison.OrdinalIgnoreCase);
            if (!fromDrag && selectedItem == view && IsLiveMobilePlacementSession())
            {
                RotateSelectedShape();
                return;
            }

            BeginMobileShapePlacement(
                view,
                fromDrag,
                keepCurrentShape: false,
                seamPayload: payload);
        }

        private void HandleSeamPlacementCancelled(BattlePreparePlacementCancelContext context)
        {
            lastGhostResult = null;
            if (selectedItem != null)
            {
                SetItemSelectionVisual(selectedItem, false);
                selectedItem = null;
            }

            HideHighlights();
            SetFeedback("Mobile shape placement cancelled.", false);
            UpdateSelectedItemInfo();
        }

        private bool BeginMobileShapePlacement(
            DraggableTalismanItemView view,
            bool fromDrag,
            bool keepCurrentShape = false,
            ShapeItemPayload seamPayload = default)
        {
            if (view == null)
            {
                return false;
            }

            InitializeMobileShapePlacementRuntime();
            if (selectedItem != null && selectedItem != view)
            {
                SetItemSelectionVisual(selectedItem, false);
            }

            selectedItem = view;
            SetItemSelectionVisual(selectedItem, true);
            if (!keepCurrentShape)
            {
                selectedShapeIndex = ResolveShapeIndex(view);
            }

            selectedRotation = seamPayload.IsValid
                ? seamPayload.Rotation
                : ItemShapeRotation.Rotation0;
            ItemShapeConfig shape = CurrentShape();
            ShapeItemPayload payload = seamPayload.IsValid
                ? seamPayload
                : BuildSelectedPayload(view, shape, selectedRotation);
            bool started = mobilePlacementIntegration.Input.TapTrayItem(payload, ResolveTrayAnchorCell(view));
            lastGhostResult = null;
            if (!started)
            {
                HideHighlights();
                SetFeedback("道具形态数据缺失，无法开始移动端占格流程。", true);
                UpdateSelectedItemInfo();
                return false;
            }

            string gestureHint = fromDrag ? "拖到棋盘后松手锁定虚影，再点击确认。" : "移动到棋盘预览，点击已选道具可旋转。";
            SetFeedback($"已拿起「{ResolveItemName(view)}」：V0.4 {shape.shapeName}。{gestureHint}", false);
            UpdateSelectedItemInfo();
            return true;
        }

        private ShapeItemPayload BuildSelectedPayload(
            DraggableTalismanItemView view,
            ItemShapeConfig shape,
            ItemShapeRotation rotation)
        {
            string itemId = view != null && view.Definition != null
                ? view.Definition.itemId
                : "runtime_selected_item";
            ShapePlacementSource source = view != null && view.CurrentSlot != null
                ? ShapePlacementSource.Board
                : ShapePlacementSource.Tray;
            return mobilePlacementIntegration.BuildPayload(itemId, shape, rotation, source);
        }

        private ShapeItemPayload BuildSeamShapePayload(DraggableTalismanItemView view)
        {
            if (view == null || mobilePlacementIntegration == null)
            {
                return default;
            }

            if (runtimeShapeConfigs.Count == 0)
            {
                BuildRuntimeShapeConfigs();
            }

            int shapeIndex = Mathf.Clamp(ResolveShapeIndex(view), 0, runtimeShapeConfigs.Count - 1);
            ItemShapeConfig shape = runtimeShapeConfigs[shapeIndex];
            ItemShapeRotation rotation = selectedItem == view ? selectedRotation : ItemShapeRotation.Rotation0;
            return BuildSelectedPayload(view, shape, rotation);
        }

        private ItemShapeCell? ResolveTrayAnchorCell(DraggableTalismanItemView view)
        {
            string itemId = view != null && view.Definition != null ? view.Definition.itemId : string.Empty;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            if (shapeAwareTrayGrid != null
                && shapeAwareTrayGrid.TryGetPlacement(itemId, out ShapeAwareItemTrayGridPlacement gridPlacement))
            {
                return gridPlacement.AnchorCell;
            }

            if (trayPlacementStates.TryGetValue(itemId, out ShapeAwareTrayPlacementState state)
                && state.currentAnchorSlotIndex >= 0)
            {
                return new ItemShapeCell(
                    state.currentAnchorSlotIndex % MatureTrayColumnCount,
                    state.currentAnchorSlotIndex / MatureTrayColumnCount);
            }

            return null;
        }

        private static void SetItemSelectionVisual(DraggableTalismanItemView view, bool selected)
        {
            if (view == null)
            {
                return;
            }

            ShapeAwareItemTrayFixtureView shapeAwareView = view.GetComponent<ShapeAwareItemTrayFixtureView>();
            if (shapeAwareView != null)
            {
                shapeAwareView.SetShapeSelectedVisual(selected);
                view.SetSelectedVisual(false);
                return;
            }

            view.SetSelectedVisual(selected, new Color(0.34f, 0.95f, 1f, 1f), new Vector2(5f, -5f));
        }

        private int ResolveShapeIndex(DraggableTalismanItemView view)
        {
            string itemId = view?.Definition != null ? view.Definition.itemId : string.Empty;
            if (!string.IsNullOrWhiteSpace(itemId) && fixtureShapeByItemId.TryGetValue(itemId, out string fixtureShapeId))
            {
                return IndexOfShape(fixtureShapeId);
            }

            if (ContainsAny(itemId, "guard", "shield", "wood"))
            {
                return IndexOfShape("Vertical2");
            }

            if (ContainsAny(itemId, "cleanse", "corner", "purify"))
            {
                return IndexOfShape("Corner3");
            }

            if (ContainsAny(itemId, "stone", "core", "seal", "soul"))
            {
                return IndexOfShape("Square4");
            }

            if (ContainsAny(itemId, "fixture_single1"))
            {
                return IndexOfShape("Single1");
            }

            if (ContainsAny(itemId, "fixture_vertical2"))
            {
                return IndexOfShape("Vertical2");
            }

            if (ContainsAny(itemId, "fixture_corner3"))
            {
                return IndexOfShape("Corner3");
            }

            if (ContainsAny(itemId, "fixture_square4"))
            {
                return IndexOfShape("Square4");
            }

            return IndexOfShape("Single1");
        }

        private int IndexOfShape(string shapeId)
        {
            int index = runtimeShapeConfigs.FindIndex(shape => shape != null && shape.shapeId == shapeId);
            return index >= 0 ? index : 0;
        }

        private void SelectNextShape()
        {
            if (runtimeShapeConfigs.Count == 0)
            {
                return;
            }

            selectedShapeIndex = (selectedShapeIndex + 1) % runtimeShapeConfigs.Count;
            if (!CurrentShape().rotationAllowed)
            {
                selectedRotation = ItemShapeRotation.Rotation0;
            }

            SetFeedback($"V0.4 扩展形状切换为 {CurrentShape().shapeName}。", false);
            if (selectedItem != null)
            {
                BeginMobileShapePlacement(selectedItem, fromDrag: false, keepCurrentShape: true);
            }

            UpdateSelectedItemInfo();
        }

        private void RotateSelectedShape()
        {
            if (selectedItem == null)
            {
                SetFeedback("请先选择一个道具。", true);
                return;
            }

            InitializeMobileShapePlacementRuntime();
            if (!IsLiveMobilePlacementSession())
            {
                BeginMobileShapePlacement(selectedItem, fromDrag: false, keepCurrentShape: true);
            }

            ItemShapeConfig shape = CurrentShape();
            ItemShapeCell? anchorCell =
                mobilePlacementIntegration.Session.BoardAnchorCell
                ?? mobilePlacementIntegration.Session.LastLegalBoardAnchor
                ?? mobilePlacementIntegration.Session.PreviewResult?.AnchorCell;
            bool rotated = mobilePlacementIntegration.Input.TapSelectedItemToRotate(
                boardShapeReceiver,
                anchorCell);
            selectedRotation = mobilePlacementIntegration.Session.Rotation;
            lastGhostResult = mobilePlacementIntegration.Session.PreviewResult ?? lastGhostResult;

            if (!rotated)
            {
                SetFeedback(mobilePlacementIntegration.Input.LastHint, false);
                UpdateSelectedItemInfo();
                return;
            }

            if (lastGhostResult != null)
            {
                ShowHighlights(
                    lastGhostResult,
                    mobilePlacementIntegration.Input.LastGhostPreview.outlineStyle,
                    acceptsPointer: true);
            }

            SetFeedback($"旋转：{FormatRotation(selectedRotation)}。{(shape != null ? shape.shapeName : "当前形状")} 的 ghost 已刷新。", false);
            UpdateSelectedItemInfo();
        }

        private void UpdatePlacementPreview()
        {
            if (boardSlots.Count == 0 || runtimeShapeConfigs.Count == 0 || mobilePlacementIntegration == null)
            {
                HideHighlights();
                return;
            }

            MobileShapePlacementInputExtension input = mobilePlacementIntegration.Input;
            if (input == null || !mobilePlacementIntegration.Session.HasActivePayload)
            {
                HideHighlights();
                return;
            }

            if (input.CurrentState == MobileShapePlacementInputState.Cancelled
                || input.CurrentState == MobileShapePlacementInputState.Idle)
            {
                HideHighlights();
                return;
            }

            if (input.CurrentState == MobileShapePlacementInputState.PreviewLocked)
            {
                ShapePlacementResult lockedResult = lastGhostResult ?? mobilePlacementIntegration.Session.PreviewResult;
                ShowHighlights(lockedResult, GhostPlacementOutlineStyle.Locked, acceptsPointer: true);
                SetFeedback(input.LastHint, false);
                return;
            }

            if (input.CurrentState == MobileShapePlacementInputState.Placed)
            {
                ShapePlacementResult placedResult = lastGhostResult ?? input.LastCommitResult;
                ShowHighlights(placedResult, GhostPlacementOutlineStyle.Confirmed, acceptsPointer: false);
                return;
            }

            ShapePlacementResult result = input.DragToReceiver(
                boardShapeReceiver,
                Input.mousePosition,
                GetEventCamera());
            lastGhostResult = result;
            ShowHighlights(result, input.LastGhostPreview.outlineStyle, acceptsPointer: result != null);
            UpdatePlacementFeedback(result);
        }

        private bool IsLiveMobilePlacementSession()
        {
            if (mobilePlacementIntegration == null
                || !mobilePlacementIntegration.Session.HasActivePayload
                || mobilePlacementIntegration.Input == null)
            {
                return false;
            }

            MobileShapePlacementInputState state = mobilePlacementIntegration.Input.CurrentState;
            return state != MobileShapePlacementInputState.Idle
                && state != MobileShapePlacementInputState.Cancelled
                && state != MobileShapePlacementInputState.Placed;
        }

        private void UpdatePlacementFeedback(ShapePlacementResult result)
        {
            if (result == null)
            {
                SetFeedback("移动到棋盘上预览 ghost。", false);
                return;
            }

            if (result.IsValid)
            {
                SetFeedback($"可以放置：{CurrentShape().shapeName}，占用 {result.OccupiedCells.Count} 格。松手锁定虚影，再点击确认。", false);
                return;
            }

            SetFeedback(FormatInvalid(result.InvalidReason), true);
        }

        private GridOccupancyMap BuildCurrentOccupancyMap(DraggableTalismanItemView except)
        {
            int width = Math.Max(BoardColumns, boardSlots.Keys.Select(pos => pos.x + 1).DefaultIfEmpty(BoardColumns).Max());
            int height = Math.Max(BoardRows, boardSlots.Keys.Select(pos => pos.y + 1).DefaultIfEmpty(BoardRows).Max());
            GridOccupancyMap map = new(width, height);
            ItemShapeConfig single = runtimeShapeConfigs.FirstOrDefault(shape => shape != null && shape.shapeId == "Single1") ?? CurrentShape();

            foreach (KeyValuePair<Vector2Int, TalismanGridSlotView> pair in boardSlots)
            {
                DraggableTalismanItemView occupant = pair.Value != null ? pair.Value.CurrentItemView : null;
                if (occupant == null || occupant == except)
                {
                    continue;
                }

                string itemId = occupant.Definition != null ? occupant.Definition.itemId : pair.Key.ToString();
                map.TryPlace(new ItemShapePlacementRequest(itemId, single.shapeId, new ItemShapeCell(pair.Key.x, pair.Key.y)), single);
            }

            return map;
        }

        private bool TryResolvePointerBoardCell(Vector2 screenPosition, out ItemShapeCell cell)
        {
            Camera eventCamera = GetEventCamera();
            foreach (KeyValuePair<Vector2Int, TalismanGridSlotView> pair in boardSlots)
            {
                RectTransform rect = pair.Value != null ? pair.Value.transform as RectTransform : null;
                if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, eventCamera))
                {
                    cell = new ItemShapeCell(pair.Key.x, pair.Key.y);
                    return true;
                }
            }

            cell = new ItemShapeCell();
            return false;
        }

        internal void HandleGhostDropped(PointerEventData eventData)
        {
            if (!IsLiveMobilePlacementSession())
            {
                return;
            }

            ShapePlacementResult result = mobilePlacementIntegration.Input.DragToReceiver(
                boardShapeReceiver,
                eventData != null ? eventData.position : (Vector2)Input.mousePosition,
                GetEventCamera());
            lastGhostResult = result;
            bool locked = mobilePlacementIntegration.Input.ReleaseDragLockPreview();
            GhostPlacementOutlineStyle style = locked
                ? GhostPlacementOutlineStyle.Locked
                : GhostPlacementOutlineStyle.Invalid;
            ShowHighlights(lastGhostResult, style, acceptsPointer: true);
            SetFeedback(mobilePlacementIntegration.Input.LastHint, !locked);
            UpdateSelectedItemInfo();
            eventData?.Use();
        }

        internal void HandleGhostClicked(PointerEventData eventData)
        {
            if (!IsLiveMobilePlacementSession())
            {
                return;
            }

            if (mobilePlacementIntegration.Input.CurrentState != MobileShapePlacementInputState.PreviewLocked)
            {
                HandleGhostDropped(eventData);
                return;
            }

            ShapePlacementResult result = mobilePlacementIntegration.Input.TapGhostToConfirm(boardShapeReceiver);
            lastGhostResult = result;
            selectedRotation = mobilePlacementIntegration.Session.Rotation;
            ShowHighlights(result, GhostPlacementOutlineStyle.Confirmed, acceptsPointer: false);
            if (result != null && result.IsValid)
            {
                SetFeedback($"已放下：{ResolveItemName(selectedItem)}，占用 {result.OccupiedCells.Count} 格。", false);
            }
            else
            {
                SetFeedback(FormatInvalid(result?.InvalidReason ?? ShapePlacementInvalidReason.ShapeInvalid), true);
            }

            UpdateSelectedItemInfo();
            eventData?.Use();
        }

        internal void HandleLockedGhostDragged(PointerEventData eventData)
        {
            if (!IsLiveMobilePlacementSession())
            {
                return;
            }

            ShapePlacementResult result = mobilePlacementIntegration.Input.DragLockedGhostToReceiver(
                boardShapeReceiver,
                eventData != null ? eventData.position : (Vector2)Input.mousePosition,
                GetEventCamera());
            lastGhostResult = result;
            ShowHighlights(
                result,
                mobilePlacementIntegration.Input.LastGhostPreview.outlineStyle,
                acceptsPointer: result != null);
            UpdatePlacementFeedback(result);
            eventData?.Use();
        }

        internal void HandleLockedGhostDragEnded(PointerEventData eventData)
        {
            HandleGhostDropped(eventData);
        }

        private void CancelMobileShapePlacement()
        {
            if (mobilePlacementIntegration == null || mobilePlacementIntegration.Input == null)
            {
                return;
            }

            mobilePlacementIntegration.Input.Cancel(boardShapeReceiver);
            lastGhostResult = null;
            if (selectedItem != null)
            {
                SetItemSelectionVisual(selectedItem, false);
                selectedItem = null;
            }

            HideHighlights();
            SetFeedback("已取消移动端占格流程。", false);
            UpdateSelectedItemInfo();
        }

        private void ShowHighlights(ShapePlacementResult result)
        {
            GhostPlacementOutlineStyle style = result != null && result.IsValid
                ? GhostPlacementOutlineStyle.Valid
                : GhostPlacementOutlineStyle.Invalid;
            ShowHighlights(result, style, acceptsPointer: false);
        }

        private void ShowHighlights(
            ShapePlacementResult result,
            GhostPlacementOutlineStyle style,
            bool acceptsPointer)
        {
            HideHighlights();
            if (result == null)
            {
                return;
            }

            Color color = ResolveGhostColor(result, style);

            IReadOnlyList<ItemShapeCell> cells = result.OccupiedCells ?? Array.Empty<ItemShapeCell>();
            for (int i = 0; i < cells.Count && i < highlightCells.Count; i++)
            {
                Image image = highlightCells[i];
                if (image == null)
                {
                    continue;
                }

                if (!boardSlots.TryGetValue(new Vector2Int(cells[i].x, cells[i].y), out TalismanGridSlotView slot)
                    || slot == null)
                {
                    image.gameObject.SetActive(false);
                    image.raycastTarget = false;
                    continue;
                }

                RectTransform slotRect = slot.transform as RectTransform;
                if (slotRect == null)
                {
                    image.gameObject.SetActive(false);
                    image.raycastTarget = false;
                    continue;
                }

                image.color = color;
                image.raycastTarget = acceptsPointer;
                image.gameObject.SetActive(true);
                PositionOverSlot((RectTransform)image.transform, slotRect);
            }
        }

        private static Color ResolveGhostColor(
            ShapePlacementResult result,
            GhostPlacementOutlineStyle style)
        {
            if (style == GhostPlacementOutlineStyle.Locked)
            {
                return new Color(1f, 0.82f, 0.18f, 0.46f);
            }

            if (style == GhostPlacementOutlineStyle.Confirmed)
            {
                return new Color(0.28f, 0.86f, 1f, 0.48f);
            }

            return result != null && result.IsValid
                ? new Color(0.16f, 0.88f, 0.38f, 0.36f)
                : new Color(0.95f, 0.18f, 0.12f, 0.42f);
        }

        private void PositionOverSlot(RectTransform overlay, RectTransform slot)
        {
            if (overlay == null || slot == null || extensionRoot == null)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            slot.GetWorldCorners(corners);
            Camera eventCamera = GetEventCamera();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                extensionRoot,
                RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]),
                eventCamera,
                out Vector2 min);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                extensionRoot,
                RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]),
                eventCamera,
                out Vector2 max);

            overlay.anchorMin = new Vector2(0.5f, 0.5f);
            overlay.anchorMax = new Vector2(0.5f, 0.5f);
            overlay.pivot = new Vector2(0.5f, 0.5f);
            overlay.anchoredPosition = (min + max) * 0.5f;
            overlay.sizeDelta = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        }

        private void HideHighlights()
        {
            foreach (Image image in highlightCells)
            {
                if (image != null)
                {
                    image.raycastTarget = false;
                    image.gameObject.SetActive(false);
                }
            }
        }

        private ItemShapeConfig CurrentShape()
        {
            if (runtimeShapeConfigs.Count == 0)
            {
                BuildRuntimeShapeConfigs();
            }

            selectedShapeIndex = Mathf.Clamp(selectedShapeIndex, 0, runtimeShapeConfigs.Count - 1);
            return runtimeShapeConfigs[selectedShapeIndex];
        }

        private void UpdateSelectedItemInfo()
        {
            if (itemInfoText == null)
            {
                return;
            }

            ItemShapeConfig shape = CurrentShape();
            string itemName = selectedItem != null ? ResolveItemName(selectedItem) : "未选择成熟道具";
            itemInfoText.text =
                $"Item: {itemName}\n" +
                $"Shape: {shape.shapeName} / {shape.cellCount} cells\n" +
                $"Rotation: {FormatRotation(selectedRotation)}\n" +
                $"Adapter: {adapter?.viewModel?.sourcePreviewBuildId ?? "runtime"}";
        }

        private string ResolveItemName(DraggableTalismanItemView view)
        {
            if (view == null)
            {
                return "none";
            }

            return view.Definition != null && !string.IsNullOrWhiteSpace(view.Definition.displayName)
                ? view.Definition.displayName
                : view.gameObject.name;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message ?? string.Empty;
            }
        }

        private void SetFeedback(string message, bool invalid)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message ?? string.Empty;
                feedbackText.color = invalid ? new Color(1f, 0.58f, 0.48f, 1f) : new Color(0.88f, 0.96f, 0.86f, 1f);
            }
        }

        private Camera GetEventCamera()
        {
            return rootCanvas != null && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas != null
                    ? rootCanvas.worldCamera
                    : null;
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string FormatRotation(ItemShapeRotation rotation)
        {
            return rotation switch
            {
                ItemShapeRotation.Rotation90 => "90",
                ItemShapeRotation.Rotation180 => "180",
                ItemShapeRotation.Rotation270 => "270",
                _ => "0"
            };
        }

        private static string FormatInvalid(ShapePlacementInvalidReason reason)
        {
            return reason switch
            {
                ShapePlacementInvalidReason.OutOfGrid => "越界：有格子超出成熟棋盘。",
                ShapePlacementInvalidReason.CellOccupied => "重叠：目标格已有成熟道具。",
                ShapePlacementInvalidReason.ShapeInvalid => "形状无效：请切换形状或旋转。",
                ShapePlacementInvalidReason.MissingShapeConfig => "缺少形状配置。",
                _ => "当前位置不能放置。"
            };
        }

        private static void Stretch(RectTransform rect)
        {
            SetRect(rect, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject target = new(name, typeof(RectTransform), typeof(Image));
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            target.transform.SetParent(parent, false);
            Image image = target.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            FontStyle style,
            TextAnchor alignment)
        {
            GameObject target = new(name, typeof(RectTransform), typeof(Text));
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            target.transform.SetParent(parent, false);
            Text text = target.GetComponent<Text>();
            text.text = value ?? string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = new Color(0.9f, 0.94f, 0.88f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            GameObject target = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            target.transform.SetParent(parent, false);
            Image image = target.GetComponent<Image>();
            image.color = new Color(0.18f, 0.27f, 0.22f, 0.96f);
            Button button = target.GetComponent<Button>();

            Text text = CreateText("Label", target.transform, label, 15, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.localScale = Vector3.one;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }

    internal sealed class MobileShapePlacementGhostTarget :
        MonoBehaviour,
        IPointerClickHandler,
        IDropHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        private BattlePrepareComponentAdapterRuntimePlaytestController owner;

        public void Bind(BattlePrepareComponentAdapterRuntimePlaytestController controller)
        {
            owner = controller;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            owner?.HandleGhostClicked(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            owner?.HandleLockedGhostDragged(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            owner?.HandleLockedGhostDragged(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            owner?.HandleLockedGhostDragEnded(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            owner?.HandleGhostDropped(eventData);
        }
    }
}
