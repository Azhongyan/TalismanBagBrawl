using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.Items.Awakening.RuntimeState;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Capability;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    public sealed class ItemDevSessionRosterAvailabilityControllerResult
    {
        internal ItemDevSessionRosterAvailabilityControllerResult(
            bool accepted,
            bool changed,
            bool trayPublished,
            string diagnosticCode,
            ItemDevSessionRosterAvailabilitySnapshot availabilitySnapshot)
        {
            Accepted = accepted;
            Changed = changed;
            TrayPublished = trayPublished;
            DiagnosticCode = diagnosticCode ?? string.Empty;
            AvailabilitySnapshot = availabilitySnapshot;
        }

        public bool Accepted { get; }
        public bool Changed { get; }
        public bool TrayPublished { get; }
        public string DiagnosticCode { get; }
        public ItemDevSessionRosterAvailabilitySnapshot AvailabilitySnapshot { get; }
    }

    public sealed class BuildGridInteractionPreviewController : MonoBehaviour
    {
        public const string PackageName = "V0.4-BuildGridInteractionPreview01";
        public const string VerticalSlicePackageName = "V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01";
        public const string MobileRotateZonePackageName = "V0.4-MobileRotateZoneInteraction01";
        public const int BoardColumns = 5;
        public const int BoardRows = 5;
        public const int TrayColumns = 5;
        public const int TrayRows = 8;
        public const int TrayVisibleRows = 5;
        private const float BattlePrepareMoveSpeed = 9f;
        private const float DefaultBattleStateYOffset = -320f;
        private const string BattlePrepareActionBarName = "V04BattlePrepareBottomActions";
        private const string BattlePrepareOverlayName = "V04BattlePrepareDarkOverlay";
        private const string ItemTrayLockedOverlayName = "ItemTrayBattleLockedOverlay";
        private const string EnemyCombatFeedbackPanelName = "EnemyCombatFeedbackPanel";
        private const string EnemyCombatFeedbackFloatingRootName = "EnemyCombatFeedbackFloatingRoot";
        private const string DevChapterDropdownSlotName = "DevChapterDropdownSlot";
        private const string PlacementFeedbackRuntimeName = "PlacementFeedback_Runtime";
        private const string BattlePrepareTransitionSequenceObjectName = "guajian";
        private const string CharacterIdlePreviewObjectName = "CharacterIdlePreview_IdleAnim";
        private const string PlacementFeedbackTextName = "PlacementFeedbackText";
        private const string PlacementFeedbackDefaultText = "单击道具查看信息；合法松手直接放置，非法返回托盘。";
        private const string DragGhostCellLayerName = "DragGhostCellLayer";
        private const string DragGhostCellNamePrefix = "DragGhostCell_";
        private const string DragGhostCellUnderlayName = "CellUnderlayImage";
        private const string DragGhostArtworkName = "DragGhostArtwork";
        private const string DragGhostInvalidArtworkName = "DragGhostInvalidArtwork";
        private const string BoardArtworkLayerName = "BoardItemArtworkLayer";
        private const string BoardPreviewArtworkName = "BoardPreviewArtwork";
        private const string BoardPreviewShadowArtworkName = "BoardPreviewShadowArtwork";
        private const string BoardPreviewInvalidArtworkName = "BoardPreviewInvalidArtwork";
        private const string BoardPlacedArtworkNamePrefix = "BoardPlacedArtwork_";
        private const string FormationPowerOverlayName = "FormationCorePowerRangeOverlay";
        private const string RotateZoneLayerName = "MobileRotateZoneLayer";
        private const string RotateZoneLeftName = "MobileRotateZoneLeft";
        private const string RotateZoneRightName = "MobileRotateZoneRight";
        private const string RotateZoneGuideName = "MobileRotateZoneGuide";
        private const float RotateButtonVisualSizePixels = 72f;
        private const float RotateButtonActivationWidthPixels = 136f;
        private const float RotateButtonActivationHeightPixels = 180f;
        private const float RotateButtonInsideEdgeInsetPixels = 6f;
        private const float RotateButtonExitPaddingPixels = 18f;
        private const float RotateSeekWindowSeconds = 0.28f;
        private const float RotateSeekMinAgeSeconds = 0.02f;
        private const float RotateSeekMinDeltaXPixels = 20f;
        private const float RotateSeekMaxUpwardDriftPixels = 44f;
        private const float RotateSeekMinVelocityXPixelsPerSecond = 300f;
        private const float RotateButtonTriggerCooldownSeconds = 1f;
        private const float RotateButtonConfirmVisualSeconds = 0.18f;
        private const float BoardSnapRotateHoldPaddingPixels = 36f;
        private const float BoardSoftBoundaryCellPadding = 0.45f;
        private const float BoardPreviewShadowScale = 1.08f;
        private static readonly Vector2 BoardPreviewShadowOffset = new(0f, -18f);
        private static readonly Color BoardPreviewShadowTint = new(0.22f, 1f, 0.42f, 0.44f);
        private static readonly Color BoardPreviewInvalidTint = new(1f, 0.18f, 0.12f, 0.52f);
        private static readonly Color DragGhostInvalidTint = new(1f, 0.16f, 0.10f, 0.58f);

        private enum MobileRotateZoneSide
        {
            None = 0,
            Left = 1,
            Right = 2
        }

        private readonly struct BoardDragPlacementCandidate
        {
            public BoardDragPlacementCandidate(
                string itemId,
                ItemShapeCell normalizedAnchor,
                ItemShapeCell authorityAnchor,
                ItemShapeRotation authorityRotation,
                IReadOnlyList<ItemShapeCell> occupiedCells,
                bool isValid,
                ShapePlacementInvalidReason invalidReason)
            {
                ItemId = itemId ?? string.Empty;
                NormalizedAnchor = normalizedAnchor;
                AuthorityAnchor = authorityAnchor;
                AuthorityRotation = authorityRotation;
                OccupiedCells = Array.AsReadOnly(OrderCells(occupiedCells));
                IsValid = isValid;
                InvalidReason = invalidReason;
            }

            public string ItemId { get; }
            public ItemShapeCell NormalizedAnchor { get; }
            public ItemShapeCell AuthorityAnchor { get; }
            public ItemShapeRotation AuthorityRotation { get; }
            public IReadOnlyList<ItemShapeCell> OccupiedCells { get; }
            public bool IsValid { get; }
            public ShapePlacementInvalidReason InvalidReason { get; }
        }

        public static readonly string[] CategoryLabels =
            BuildSandboxLegacyAndAdvancedItemRosterCatalog.CategoryDisplayLabels.ToArray();

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled;
        [SerializeField] private bool readsFormalSaveData;
        [SerializeField] private bool writesFormalFlow;
        [SerializeField] private bool writesFormalUi;
        [SerializeField] private bool touchesFormalScene;
        [SerializeField] private bool showsCompleteAnswers;

        [Header("V0.4 Battle Layout")]
        [Tooltip("Battle-state Y offset from the authored BattleLikePreviewArea position. Negative moves the battle view down.")]
        [SerializeField] private float battleStateYOffset = DefaultBattleStateYOffset;

        [Header("V0.4 Battle FX")]
        [Tooltip("Optional sprite-sequence animation played when opening prepare mode and when continuing battle.")]
        [SerializeField] private BuildSandboxSpriteSequencePlayer battlePrepareTransitionSequencePlayer;
        [Tooltip("Editable character idle preview object for checking imported frame animation in the V0.4 sandbox scene.")]
        [SerializeField] private BuildSandboxCharacterIdlePreview characterIdlePreview;

        [Header("V0.4 Artwork Direction Calibration")]
        [Tooltip("3-cell triangle/corner artwork in tray. Positive rotates left, negative rotates right.")]
        [SerializeField] private float triangleTrayArtworkRotationOffsetDegrees;
        [Tooltip("3-cell triangle/corner artwork while dragging or board-previewing. Positive rotates left, negative rotates right.")]
        [SerializeField] private float triangleDragGhostArtworkRotationOffsetDegrees = 90f;
        [Tooltip("3-cell triangle/corner artwork after being placed on the board. Positive rotates left, negative rotates right.")]
        [SerializeField] private float triangleBoardPlacedArtworkRotationOffsetDegrees = 90f;
        [Tooltip("All non-triangle artwork in tray. Positive rotates left, negative rotates right.")]
        [SerializeField] private float defaultTrayArtworkRotationOffsetDegrees;
        [Tooltip("All non-triangle artwork while dragging or board-previewing. Positive rotates left, negative rotates right.")]
        [SerializeField] private float defaultDragGhostArtworkRotationOffsetDegrees = -90f;
        [Tooltip("All non-triangle artwork after being placed on the board. Positive rotates left, negative rotates right.")]
        [SerializeField] private float defaultBoardPlacedArtworkRotationOffsetDegrees = -90f;

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
        [SerializeField] private Button trayArrangeButton;
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
        private readonly Dictionary<string, List<ShapeCellVisualStyle>> visualStylesByItemId = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ItemShapeConfig> shapeById = new(StringComparer.Ordinal);
        private readonly List<ItemShapeConfig> runtimeShapeConfigs = new();
        private ShapePlacementSession placementSession;
        private MobileShapePlacementInputExtension mobileInput;
        private ShapeAwareItemTrayGrid shapeAwareTrayGrid;
        private UiBoardShapeGridReceiver boardReceiver;
        private IItemSystemBattleSandboxBoardAuthority itemSystemBoardAuthority;
        private ItemSystemBattleSandboxItemDetailAdapter itemSystemDetailAdapter;
        private string lastPublishedItemDevRosterSignature = string.Empty;
        private PreviewItem selectedItem;
        private ShapePlacementResult lastPreviewResult;
        private ItemShapeCell lastPreviewAnchor;
        private bool hasLastPreviewAnchor;
        private ShapePlacementSource lastPreviewSource = ShapePlacementSource.Unknown;
        private readonly HashSet<string> placedItemIds = new(StringComparer.Ordinal);
        // Runtime-only memory.  It is intentionally keyed by an ordinary item's instance identity,
        // never by its catalog/base id; I031 has the dedicated SPECIAL_I031 key.
        private readonly Dictionary<string, ItemShapeCell> rememberedTrayAnchorsByIdentity =
            new(StringComparer.Ordinal);
        private bool trayMasterLayoutInitialized;
        private bool resetRequiresNewTrayMasterLayout;
        private string activeDragItemId = string.Empty;
        private bool hasActiveDragGrabbedBaseOffset;
        private ItemShapeCell activeDragGrabbedBaseOffset;
        private bool hasActiveDragOriginalRotation;
        private ItemShapeRotation activeDragOriginalRotation;
        private bool hasActiveBoardCandidate;
        private BoardDragPlacementCandidate activeBoardCandidate;
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
        private RectTransform dragGhostArtwork;
        private Image dragGhostArtworkImage;
        private RectTransform dragGhostInvalidArtwork;
        private Image dragGhostInvalidArtworkImage;
        private RectTransform boardArtworkLayer;
        private RectTransform boardPreviewArtwork;
        private Image boardPreviewArtworkImage;
        private RectTransform boardPreviewShadowArtwork;
        private Image boardPreviewShadowArtworkImage;
        private RectTransform boardPreviewInvalidArtwork;
        private Image boardPreviewInvalidArtworkImage;
        private readonly Dictionary<string, RectTransform> boardPlacedArtworkByItemId = new(StringComparer.Ordinal);
        private Image dragGhostBackgroundImage;
        private Vector2 dragGhostDefaultSize;
        private Color dragGhostDefaultBackgroundColor;
        private bool hasDragGhostDefaults;
        private RectTransform rotateZoneLayer;
        private RectTransform rotateZoneLeft;
        private RectTransform rotateZoneRight;
        private Text rotateZoneLeftText;
        private Text rotateZoneRightText;
        private Image rotateZoneLeftImage;
        private Image rotateZoneRightImage;
        private RectTransform rotateZoneGuide;
        private Image rotateZoneGuideImage;
        private MobileRotateZoneSide currentRotateZoneSide;
        private MobileRotateZoneSide activeRotateSeekSide;
        private Rect activeRotateSeekTargetRect;
        private Rect activeRotateSeekExitRect;
        private Vector2 rotateSeekAnchorPosition;
        private float rotateSeekAnchorTime = -999f;
        private bool hasRotateSeekAnchor;
        private float lastRotateZoneTime = -999f;
        private MobileRotateZoneSide confirmedRotateZoneSide;
        private float rotateZoneConfirmUntilTime = -999f;
        private readonly Dictionary<ItemShapeCell, FormationPowerCellOverlay> formationPowerOverlayByCell = new();

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
        public bool UsesItemSystemBoardAuthority => itemSystemBoardAuthority != null;
        public ItemSystemSnapshot CurrentItemSystemBoardSnapshot =>
            itemSystemBoardAuthority?.CurrentSnapshot;
        public ItemInstancePlacementBindingContractSnapshot CurrentItemSystemBoardBinding =>
            itemSystemBoardAuthority?.CurrentBindingSnapshot;
        public ItemInstanceQualifiedBuildStateSnapshot CurrentItemSystemQualifiedBuildState =>
            itemSystemBoardAuthority?.CurrentQualifiedBuildState;
        public ItemInstanceCoreEffectRuntimeStateSnapshot CurrentItemSystemCoreEffectRuntimeState =>
            itemSystemBoardAuthority?.CurrentCoreEffectRuntimeState;
        public ItemSystemBattleSandboxItemDetailAdapter ItemSystemDetailAdapter =>
            itemSystemDetailAdapter;
        public ItemDevSessionRosterAvailabilitySnapshot
            CurrentItemDevSessionRosterAvailability =>
                itemSystemBoardAuthority?.CurrentDevSessionRosterAvailability;
        public bool IsSandboxBattleModeActive =>
            sandboxBattleActive && !battlePrepareStateActive && !battlePrepareContinueStateActive;
        public bool IsTrayArrangeAvailable => CanArrangeTrayLayout();
        public bool IsTrayItemDragActive =>
            !string.IsNullOrWhiteSpace(activeDragItemId);

        public void NotifyTrayCategoryChanged()
        {
            RefreshTrayArrangeButton();
        }

        public void ArrangeTrayLayout()
        {
            if (!CanArrangeTrayLayout())
            {
                placementFeedbackView?.ShowInfo("整理仅可在全部分类且没有拖动道具时使用。");
                RefreshTrayArrangeButton();
                return;
            }

            if (!TryBuildArrangedTrayMaster(out ShapeAwareItemTrayGrid stagedGrid))
            {
                placementFeedbackView?.ShowInvalid("整理失败：当前托盘无法完整容纳全部道具，已保留原布局。");
                return;
            }

            ShapeAwareItemTrayGrid previousMaster = shapeAwareTrayGrid;
            bool previousMasterInitialized = trayMasterLayoutInitialized;
            bool trayTransactionOpen = itemTrayView != null;
            itemTrayView?.BeginTrayViewTransaction();
            try
            {
                shapeAwareTrayGrid = stagedGrid;
                trayMasterLayoutInitialized = true;
                foreach (PreviewItem item in itemById.Values
                             .Where(value => value != null
                                 && IsCurrentItemSystemBaseItemAvailable(value.ItemId)
                                 && !placedItemIds.Contains(value.ItemId))
                             .OrderBy(value => value.ItemId, StringComparer.Ordinal))
                {
                    RefreshTrayPlacement(item);
                }
                itemTrayView?.ApplyFilter(BuildItemTrayPreviewView.AllCategory);
                if (trayTransactionOpen
                    && itemTrayView?.CommitTrayViewTransaction() != true)
                {
                    shapeAwareTrayGrid = previousMaster;
                    trayMasterLayoutInitialized = previousMasterInitialized;
                    itemTrayView?.RollbackTrayViewTransaction();
                    placementFeedbackView?.ShowInvalid(
                        "整理失败：托盘视图未提交，已完整恢复原布局。");
                    return;
                }
                trayTransactionOpen = false;
            }
            catch
            {
                shapeAwareTrayGrid = previousMaster;
                trayMasterLayoutInitialized = previousMasterInitialized;
                if (trayTransactionOpen)
                {
                    itemTrayView?.RollbackTrayViewTransaction();
                }
                throw;
            }
            placementFeedbackView?.ShowValid("已按固定规则整理道具栏。");
            RefreshTrayArrangeButton();
        }

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

        public bool InstallItemSystemBattleSandboxBoardAuthority(
            IItemSystemBattleSandboxBoardAuthority authority,
            ItemSystemBattleSandboxItemDetailAdapter detailAdapter,
            out string diagnosticCode)
        {
            diagnosticCode = string.Empty;
            if (authority == null || authority.CurrentSnapshot == null
                || !authority.CurrentSnapshot.isValid
                || !string.Equals(authority.CurrentSnapshot.schemaVersion,
                    ItemSystemSnapshot.CurrentSchemaVersion,
                    StringComparison.Ordinal)
                || authority.CurrentQualifiedBuildState == null
                || !string.Equals(authority.CurrentQualifiedBuildState.schemaId,
                    ItemInstanceQualifiedBuildStateSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                || authority.CurrentQualifiedBuildState.status ==
                    ItemInstanceQualifiedBuildStateStatus.Invalid
                || authority.CurrentCoreEffectRuntimeState == null
                || !string.Equals(authority.CurrentCoreEffectRuntimeState.schemaId,
                    ItemInstanceCoreEffectRuntimeStateSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                || authority.CurrentCoreEffectRuntimeState.status ==
                    ItemCoreEffectRuntimeStateStatus.Invalid
                || authority.CurrentDevSessionRosterAvailability == null
                || !string.Equals(
                    authority.CurrentDevSessionRosterAvailability.SchemaId,
                    ItemDevSessionRosterAvailabilitySnapshot.CurrentSchemaId,
                    StringComparison.Ordinal))
            {
                diagnosticCode = "ITEM_SYSTEM_AUTHORITY_INVALID";
                return false;
            }
            if (detailAdapter == null || !detailAdapter.IsInitialized)
            {
                diagnosticCode = "ITEM_SYSTEM_DETAIL_ADAPTER_INVALID";
                return false;
            }
            if (itemSystemBoardAuthority != null)
            {
                diagnosticCode = ReferenceEquals(itemSystemBoardAuthority, authority)
                    ? "ITEM_SYSTEM_AUTHORITY_ALREADY_INSTALLED"
                    : "ITEM_SYSTEM_AUTHORITY_DUPLICATE";
                return false;
            }
            if (itemTrayView == null)
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_VIEW_MISSING";
                return false;
            }
            if (!ValidateStableAllTrayRoster(authority.Rows, out diagnosticCode))
            {
                return false;
            }
            if (!itemTrayView.InstallItemSystemAuthorityRuntimeSlots(
                    out diagnosticCode))
            {
                return false;
            }
            mobileInput?.Cancel(boardReceiver);
            itemSystemBoardAuthority = authority;
            itemSystemDetailAdapter = detailAdapter;
            itemInfoPanel?.Hide();
            RebuildItemSystemAuthorityCatalog();
            InitializePlacementRuntime();
            itemTrayView?.Initialize(
                this,
                itemById.Values
                    .Where(item => item != null
                        && IsCurrentItemSystemBaseItemAvailable(item.ItemId))
                    .OrderBy(item => item.ItemId, StringComparer.Ordinal)
                    .ToList(),
                new[] { BuildItemTrayPreviewView.AllCategory });
            ApplyItemSystemAuthorityArtwork();
            if (!TryRebuildCachesFromAcceptedItemSystemSnapshot(
                    out diagnosticCode))
            {
                UninstallItemSystemBattleSandboxBoardAuthority(authority);
                return false;
            }
            itemTrayView.ApplyFilter(BuildItemTrayPreviewView.AllCategory);
            diagnosticCode = "NONE";
            return true;
        }

        public ItemDevSessionRosterAvailabilityControllerResult
            ApplyCoreLoopLabItemDevSessionRosterAvailability(
                ItemDevSessionRosterAvailabilityRequest request)
        {
            ItemDevSessionRosterAvailabilitySnapshot current =
                itemSystemBoardAuthority?.CurrentDevSessionRosterAvailability;
            if (!Application.isEditor || !Application.isPlaying)
            {
                return DevRosterControllerFailure(
                    "DEV_ROSTER_EDITOR_RUNTIME_REQUIRED", current);
            }
            Scene scene = gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded
                || !string.Equals(
                    NormalizeSceneAssetPath(scene.path),
                    ItemSystemBattleSandboxBoardAdapter.CoreLoopLabScenePath,
                    StringComparison.Ordinal))
            {
                return DevRosterControllerFailure(
                    "DEV_ROSTER_CONTROLLER_HOST_REJECTED", current);
            }
            if (request == null)
            {
                return DevRosterControllerFailure(
                    "DEV_ROSTER_REQUEST_MISSING", current);
            }
            if (!string.Equals(
                    request.ApprovedDevHostId,
                    ItemSystemBattleSandboxBoardAdapter.CoreLoopLabScenePath,
                    StringComparison.Ordinal))
            {
                return DevRosterControllerFailure(
                    "DEV_ROSTER_CONTROLLER_REQUEST_HOST_REJECTED", current);
            }
            if (!string.Equals(
                    request.ProductContext,
                    ItemSystemBattleSandboxBoardAdapter.CoreLoopLabHostContext,
                    StringComparison.Ordinal))
            {
                return DevRosterControllerFailure(
                    "DEV_ROSTER_CONTROLLER_CONTEXT_REJECTED", current);
            }
            if (itemSystemBoardAuthority == null)
            {
                return DevRosterControllerFailure(
                    "DEV_ROSTER_AUTHORITY_NOT_INSTALLED", null);
            }

            ItemDevSessionRosterAvailabilityResult authorityResult =
                itemSystemBoardAuthority.ApplyDevSessionRosterAvailability(request);
            if (authorityResult == null || !authorityResult.Accepted)
            {
                return new ItemDevSessionRosterAvailabilityControllerResult(
                    false,
                    false,
                    false,
                    authorityResult?.DiagnosticCode
                        ?? "DEV_ROSTER_AUTHORITY_RESULT_MISSING",
                    authorityResult?.Snapshot
                        ?? itemSystemBoardAuthority
                            .CurrentDevSessionRosterAvailability);
            }
            bool publicationRequired = authorityResult.Changed
                || !string.Equals(
                    lastPublishedItemDevRosterSignature,
                    authorityResult.Snapshot?.CanonicalSignature,
                    StringComparison.Ordinal);
            if (!publicationRequired)
            {
                return new ItemDevSessionRosterAvailabilityControllerResult(
                    true,
                    false,
                    true,
                    authorityResult.DiagnosticCode,
                    authorityResult.Snapshot);
            }

            if (!TryRebuildCachesFromAcceptedItemSystemSnapshot(
                    out string publicationDiagnostic))
            {
                return new ItemDevSessionRosterAvailabilityControllerResult(
                    false,
                    true,
                    false,
                    publicationDiagnostic,
                    authorityResult.Snapshot);
            }
            return new ItemDevSessionRosterAvailabilityControllerResult(
                true,
                true,
                true,
                authorityResult.DiagnosticCode,
                authorityResult.Snapshot);
        }

        private static ItemDevSessionRosterAvailabilityControllerResult
            DevRosterControllerFailure(
                string diagnosticCode,
                ItemDevSessionRosterAvailabilitySnapshot snapshot)
        {
            return new ItemDevSessionRosterAvailabilityControllerResult(
                false,
                false,
                false,
                diagnosticCode,
                snapshot);
        }

        private static string NormalizeSceneAssetPath(string scenePath)
        {
            return string.IsNullOrWhiteSpace(scenePath)
                ? string.Empty
                : scenePath.Trim().Replace('\\', '/');
        }

        public void UninstallItemSystemBattleSandboxBoardAuthority(
            IItemSystemBattleSandboxBoardAuthority authority)
        {
            if (authority == null
                || !ReferenceEquals(itemSystemBoardAuthority, authority))
            {
                return;
            }
            RestoreItemSystemAuthorityArtwork();
            itemTrayView?.UninstallItemSystemAuthorityRuntimeSlots();
            itemSystemDetailAdapter?.Uninstall();
            itemSystemDetailAdapter = null;
            lastPublishedItemDevRosterSignature = string.Empty;
            itemInfoPanel?.Hide();
            itemSystemBoardAuthority = null;
            ClearFormationPowerVisuals();
            if (!isActiveAndEnabled)
            {
                return;
            }
            BuildShapeLookup();
            BuildItemLookup();
            InitializePlacementRuntime();
            itemTrayView?.Initialize(this, itemById.Values.ToList(), CategoryLabels);
            CacheAllItemVisualStyles();
            ResetPreview();
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
            if (itemSystemBoardAuthority != null)
            {
                return ItemSystemBattleSandboxViewProjection.ToCompatibilitySnapshot(
                    itemSystemBoardAuthority.CurrentSnapshot,
                    itemSystemBoardAuthority.Rows);
            }

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
            itemTrayView?.ApplyFilter(BuildItemTrayPreviewView.AllCategory);
            placementFeedbackView?.ShowInfo(
                "当前道具栏固定显示全部道具；分类筛选已停用。");
        }

        public void CompactTrayWhenReturningToAllCategory()
        {
            // Compatibility entrypoint retained for existing callers.  Returning to All must only
            // reveal the canonical master layout; it must never mutate or repack that layout.
            RefreshTrayArrangeButton();
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

            placementFeedbackView?.ShowInfo(IsBattleInteractionLocked()
                ? $"已查看“{item.DisplayName}”。阵势已启，当前仅可查看道具详情。"
                : $"已查看“{item.DisplayName}”。单击只打开详情；拖到棋盘后向右下角按钮顺时针旋转。");
        }

        public void RotateSelectedItem()
        {
            placementFeedbackView?.ShowInfo("本包改为拖动热区旋转；请拖到棋盘后向右下角按钮顺时针旋转。");
        }

        public void RotateTrayItem(BuildItemPreviewCardView card)
        {
            if (card == null || !itemById.TryGetValue(card.ItemId, out PreviewItem item))
            {
                return;
            }

            placementFeedbackView?.ShowInfo("本包改为拖动热区旋转；请拖到棋盘后向右下角按钮顺时针旋转。");
        }

        private void RotateTrayItem(string itemId)
        {
            placementFeedbackView?.ShowInfo("本包改为拖动热区旋转；请拖到棋盘后向右下角按钮顺时针旋转。");
        }

        public void BeginDrag(BuildItemPreviewCardView card, PointerEventData eventData)
        {
            if (!battlePrepareStateActive)
            {
                ShowBattleLockedDragToast();
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

            if (!TryResolveTrayGrabbedCell(
                    item,
                    eventData,
                    out ItemShapeCell grabbedTrayCell))
            {
                placementFeedbackView?.ShowInvalid(
                    "请从道具实际占格区域开始拖动。");
                return;
            }

            CacheItemVisualStyles(item);
            if (!BeginHoldingItem(
                    item,
                    showInfoPanel: false,
                    grabbedCell: grabbedTrayCell))
            {
                return;
            }
            activeDragItemId = item.ItemId;
            RefreshTrayArrangeButton();
            itemTrayView?.SetRotateEnabled(item.ItemId, false);
            RefreshItemInfoPanel(item);
            ClearPreviewCells();
            ShowDragGhost(selectedItem, eventData, "拖动中");
            ResetRotateZoneEntry();
            SetRotateZonesVisible(false);
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
                ShowBattleLockedDragToast();
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

            if (!TryResolveCurrentBoardAnchor(
                    item,
                    out ItemShapeCell boardAnchor)
                || !BeginHoldingItem(
                    item,
                    showInfoPanel: false,
                    source: ShapePlacementSource.Board,
                    boardAnchor: boardAnchor,
                    grabbedCell: boardCell))
            {
                placementFeedbackView?.ShowInvalid(
                    "无法确认棋盘道具的原始占格；本次拖动未开始。");
                return;
            }
            activeDragItemId = item.ItemId;
            RefreshTrayArrangeButton();
            itemTrayView?.SetRotateEnabled(item.ItemId, false);
            RefreshItemInfoPanel(item);
            ClearPreviewCells();
            ShowDragGhost(selectedItem, eventData, "拖动中");
            ResetRotateZoneEntry();
            SetRotateZonesVisible(false);
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

        public bool TryShowBoardItemDetailFromCell(ItemShapeCell visualCell)
        {
            ItemShapeCell boardCell = ResolveBoardDataCellFromVisualCell(visualCell);
            if (boardReceiver == null
                || !boardReceiver.TryGetItemAtCell(boardCell, out string itemId)
                || !itemById.TryGetValue(itemId, out PreviewItem item))
            {
                return false;
            }

            selectedItem = item;
            SetSelectedItemInfoVisible(true);
            UpdateSelectedItemInfo(item);
            bool detailShown;
            if (itemSystemBoardAuthority != null)
            {
                itemInfoPanel?.Hide();
                detailShown = ShowQualifiedItemSystemDetail(item);
            }
            else
            {
                detailShown = itemInfoPanel != null;
                if (detailShown)
                {
                    ShowItemInfoPanel(item);
                }
            }

            if (!detailShown)
            {
                placementFeedbackView?.ShowInvalid("棋盘道具详情暂不可用。");
                return true;
            }

            placementFeedbackView?.ShowInfo(
                $"已查看棋盘上的“{item.DisplayName}”。");
            return true;
        }

        public void ResetPreview()
        {
            ClearRememberedTrayAnchorsForReset();
            if (itemSystemBoardAuthority != null)
            {
                ResetItemSystemAuthorityPreview();
                return;
            }

            mobileInput?.Cancel(boardReceiver);
            boardReceiver?.Clear();
            placementSequence = 0;
            placedItemIds.Clear();
            activeDragItemId = string.Empty;
            ClearActiveDragContext(restoreOriginalRotation: false);
            hasLastPreviewAnchor = false;
            lastPreviewResult = null;
            lastPreviewSource = ShapePlacementSource.Unknown;
            selectedItem = null;
            shapeAwareTrayGrid?.Clear();
            foreach (PreviewItem item in OrderInitialTrayLayoutItems(
                         itemById.Values.Where(item => item != null
                             && IsCurrentItemSystemBaseItemAvailable(item.ItemId))))
            {
                item.Rotation = ItemShapeRotation.Rotation0;
                shapeAwareTrayGrid?.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out _);
                itemTrayView?.SetItemInTray(item.ItemId, true);
                RefreshTrayPlacement(item);
                itemTrayView?.SetRotateEnabled(item.ItemId, true);
            }
            trayMasterLayoutInitialized = true;

            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                slot?.ClearPlaced();
            }

            RefreshFormationPowerVisuals();
            HideDragGhost();
            SetRotateZonesVisible(false);
            SetSelectedItemInfoVisible(false);
            UpdateSelectedItemInfo(null);
            itemInfoPanel?.Hide();
            sandboxBattleActive = false;
            battlePrepareStateActive = false;
            battlePrepareContinueStateActive = false;
            manaLoopRuntime?.ResetLoop();
            runtimeLoopRuntime?.ResetLoop();
            RefreshBattlePrepareChrome(snapMotion: true);
            placementFeedbackView?.ShowNeutral("已取消。单击查看信息；拖动摆放，棋盘上可向右下角按钮顺时针旋转。");
            RefreshTrayArrangeButton();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EnsureEditablePlacementFeedbackInHierarchy();
            BuildSandboxItemInfoPanel.EnsureEditableInScene();
            EnsureCharacterIdlePreviewInHierarchy();
        }

        private void Awake()
        {
            EnsureReferences();
            EnsureBattlePrepareTransitionSequencePlayer();
            EnsureCharacterIdlePreviewInHierarchy();
            BuildSlotLookup();
            BuildShapeLookup();
            BuildItemLookup();
            InitializePlacementRuntime();
            WireButtons();
            EnsureBattlePrepareChrome();
            EnsureManaLoopRuntime();
            EnsureRuntimeLoopRuntime();
            itemTrayView?.Initialize(this, itemById.Values.ToList(), CategoryLabels);
            CacheAllItemVisualStyles();
            ResetPreview();
        }

        private void Update()
        {
            UpdateBattlePrepareMotion();
            HandleItemSystemDetailOutsideDismissInput();
        }

        private void HandleItemSystemDetailOutsideDismissInput()
        {
            if (itemSystemDetailAdapter?.IsVisible != true
                || itemSystemDetailAdapter.VisibleSinceFrame == Time.frameCount)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryDismissItemSystemDetailAt(Input.mousePosition);
                return;
            }

            for (int index = 0; index < Input.touchCount; index++)
            {
                Touch touch = Input.GetTouch(index);
                if (touch.phase != TouchPhase.Began)
                {
                    continue;
                }

                TryDismissItemSystemDetailAt(touch.position);
                return;
            }
        }

        private bool TryDismissItemSystemDetailAt(Vector2 screenPosition)
        {
            if (itemSystemDetailAdapter?.IsVisible != true
                || itemSystemDetailAdapter.ContainsVisibleContentScreenPoint(screenPosition))
            {
                return false;
            }

            itemSystemDetailAdapter.Hide();
            return true;
        }

        private void LateUpdate()
        {
            EnsureItemSystemAuthorityFeedbackLine();
            RefreshTrayArrangeButton();
        }

        private void OnDestroy()
        {
            if (trayArrangeButton != null)
            {
                trayArrangeButton.onClick.RemoveListener(ArrangeTrayLayout);
            }
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
                itemInfoPanel.SetRotateHandler(null);
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

            RectTransform rect = FindRectTransform(PlacementFeedbackRuntimeName);
            bool created = false;
            if (rect == null)
            {
                GameObject feedbackObject = new(
                    PlacementFeedbackRuntimeName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(BuildPlacementFeedbackView));
                feedbackObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                feedbackObject.transform.SetParent(parent, false);
                rect = feedbackObject.GetComponent<RectTransform>();
                SetRuntimeAnchors(rect, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.10f));
                created = true;
            }

            Image background = rect.GetComponent<Image>();
            bool backgroundCreated = false;
            if (background == null)
            {
                background = rect.gameObject.AddComponent<Image>();
                backgroundCreated = true;
            }

            if (created || backgroundCreated)
            {
                background.color = new Color(0.18f, 0.14f, 0.09f, 0.96f);
            }

            background.raycastTarget = false;

            Text text = ResolvePlacementFeedbackText(rect);
            bool textCreated = false;
            if (text == null)
            {
                text = CreateRuntimeText(
                    PlacementFeedbackTextName,
                    rect,
                    PlacementFeedbackDefaultText,
                    16,
                    FontStyle.Normal,
                    TextAnchor.MiddleCenter);
                textCreated = true;
            }

            if (created || textCreated)
            {
                SetRuntimeAnchors(text.rectTransform, Vector2.zero, Vector2.one);
            }

            placementFeedbackView = rect.GetComponent<BuildPlacementFeedbackView>();
            if (placementFeedbackView == null)
            {
                placementFeedbackView = rect.gameObject.AddComponent<BuildPlacementFeedbackView>();
            }

            placementFeedbackView.Bind(text, background);
            placementFeedbackView.SetStateBackgroundColorsEnabled(created);
            placementFeedbackView.ShowNeutral(text.text);
        }

        private void EnsureEditablePlacementFeedbackInHierarchy()
        {
            if (gameObject == null || !gameObject.scene.IsValid())
            {
                return;
            }

            RectTransform rect = FindRectTransform(PlacementFeedbackRuntimeName);
            bool created = false;
            if (rect == null)
            {
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
                    PlacementFeedbackRuntimeName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(BuildPlacementFeedbackView));
                feedbackObject.transform.SetParent(parent, false);
                rect = feedbackObject.GetComponent<RectTransform>();
                SetRuntimeAnchors(rect, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.10f));
                created = true;
            }

            BindEditablePlacementFeedback(rect, created);
        }

        private void BindEditablePlacementFeedback(RectTransform rect, bool created)
        {
            if (rect == null)
            {
                return;
            }

            Image background = rect.GetComponent<Image>();
            if (background == null)
            {
                background = rect.gameObject.AddComponent<Image>();
                created = true;
            }

            if (created)
            {
                background.color = new Color(0.18f, 0.14f, 0.09f, 0.96f);
            }

            background.raycastTarget = false;

            Text text = ResolvePlacementFeedbackText(rect);
            bool textCreated = false;
            if (text == null)
            {
                GameObject textObject = new(PlacementFeedbackTextName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                textObject.transform.SetParent(rect, false);
                text = textObject.GetComponent<Text>();
                textCreated = true;
            }

            if (textCreated)
            {
                SetRuntimeAnchors(text.rectTransform, Vector2.zero, Vector2.one);
                text.text = PlacementFeedbackDefaultText;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 16;
                text.fontStyle = FontStyle.Normal;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(0.91f, 0.88f, 0.76f, 1f);
                text.raycastTarget = false;
            }
            else if (string.IsNullOrWhiteSpace(text.text))
            {
                text.text = PlacementFeedbackDefaultText;
            }

            placementFeedbackView = rect.GetComponent<BuildPlacementFeedbackView>();
            if (placementFeedbackView == null)
            {
                placementFeedbackView = rect.gameObject.AddComponent<BuildPlacementFeedbackView>();
            }

            placementFeedbackView.Bind(text, background);
            placementFeedbackView.SetStateBackgroundColorsEnabled(false);
        }

        private void EnsureCharacterIdlePreviewInHierarchy()
        {
            if (gameObject == null || !gameObject.scene.IsValid())
            {
                return;
            }

            RectTransform rect = FindRectTransform(CharacterIdlePreviewObjectName);
            bool created = false;
            if (rect == null)
            {
                RectTransform parent = FindRectTransform("BattleLikePreviewArea");
                if (parent == null)
                {
                    parent = boardGridPreview == null ? null : boardGridPreview.parent as RectTransform;
                }

                if (parent == null)
                {
                    return;
                }

                GameObject previewObject = new(
                    CharacterIdlePreviewObjectName,
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(BuildSandboxCharacterIdlePreview));
                if (Application.isPlaying)
                {
                    previewObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                }

                previewObject.transform.SetParent(parent, false);
                rect = previewObject.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(36f, 150f);
                rect.sizeDelta = new Vector2(220f, 294f);
                rect.localScale = Vector3.one;
                created = true;
            }

            Image image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
            }

            image.raycastTarget = false;
            image.preserveAspect = true;
            if (created || image.color.a <= 0f)
            {
                image.color = Color.white;
            }

            characterIdlePreview = rect.GetComponent<BuildSandboxCharacterIdlePreview>();
            if (characterIdlePreview == null)
            {
                characterIdlePreview = rect.gameObject.AddComponent<BuildSandboxCharacterIdlePreview>();
            }

            characterIdlePreview.EnsureDefaults();
        }

        private static Text ResolvePlacementFeedbackText(RectTransform feedbackRect)
        {
            if (feedbackRect == null)
            {
                return null;
            }

            Transform direct = feedbackRect.Find(PlacementFeedbackTextName);
            if (direct != null && direct.TryGetComponent(out Text directText))
            {
                return directText;
            }

            return feedbackRect.GetComponentInChildren<Text>(true);
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
            return ConvertBoardVisualCellToDataCell(
                visualCell,
                BoardRows,
                BoardVisualRowsAreTopDown());
        }

        private ItemShapeCell ResolveBoardVisualCellFromDataCell(ItemShapeCell dataCell)
        {
            return ConvertBoardVisualCellToDataCell(
                dataCell,
                BoardRows,
                BoardVisualRowsAreTopDown());
        }

        private static ItemShapeCell ConvertBoardVisualCellToDataCell(
            ItemShapeCell cell,
            int rowCount,
            bool visualRowsAreTopDown)
        {
            if (!visualRowsAreTopDown)
            {
                return cell;
            }

            int safeRowCount = Mathf.Max(1, rowCount);
            return new ItemShapeCell(cell.x, safeRowCount - 1 - cell.y);
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

        private static bool ValidateStableAllTrayRoster(
            IReadOnlyList<ItemSystemBattleSandboxViewRow> rows,
            out string diagnosticCode)
        {
            diagnosticCode = string.Empty;
            ItemSystemBattleSandboxViewRow[] roster =
                (rows ?? Array.Empty<ItemSystemBattleSandboxViewRow>())
                .ToArray();
            if (roster.Length == 0 || roster.Any(row => row == null))
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_ROSTER_MISSING";
                return false;
            }

            string ResolveIdentity(ItemSystemBattleSandboxViewRow row) =>
                row.IsSystemItem
                    ? I031InventoryPlacementContract.SpecialIdentityId
                    : row.ItemInstanceId?.Trim() ?? string.Empty;

            if (roster.Any(row => string.IsNullOrWhiteSpace(row.BaseItemId)
                    || string.IsNullOrWhiteSpace(row.ShapeId)
                    || row.ShapeCells == null
                    || row.ShapeCells.Count == 0
                    || string.IsNullOrWhiteSpace(ResolveIdentity(row))))
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_IDENTITY_INVALID";
                return false;
            }
            if (roster.GroupBy(row => row.BaseItemId, StringComparer.Ordinal)
                    .Any(group => group.Count() != 1)
                || roster.GroupBy(ResolveIdentity, StringComparer.Ordinal)
                    .Any(group => group.Count() != 1))
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_IDENTITY_DUPLICATE";
                return false;
            }

            ShapeAwareItemTrayGrid staged = new(
                receiverId: "battle_sandbox_stable_all_tray_preflight",
                columnCount: TrayColumns,
                slotCount: BuildItemTrayPreviewView
                    .ItemSystemAuthorityLogicalSlotCount,
                commitAllowed: true);
            IEnumerable<ShapeItemPayload> payloads = roster
                .Select(row => new ShapeItemPayload(
                    row.BaseItemId,
                    row.ShapeId,
                    ItemShapeRotation.Rotation0,
                    row.ShapeCells,
                    ShapePlacementSource.Tray))
                .Select(payload => new
                {
                    Payload = payload,
                    Offsets = payload.BuildNormalizedOffsets()
                })
                .OrderByDescending(value => value.Offsets.Count)
                .ThenByDescending(value => IsIrregularFootprint(value.Offsets))
                .ThenByDescending(value => BoundingCellCount(value.Offsets))
                .ThenBy(value => ResolveIdentity(roster.Single(row =>
                    string.Equals(row.BaseItemId, value.Payload.ItemId,
                        StringComparison.Ordinal))), StringComparer.Ordinal)
                .Select(value => value.Payload);
            foreach (ShapeItemPayload payload in payloads)
            {
                if (!staged.TryPack(payload, out ShapePlacementResult result)
                    || result?.IsValid != true)
                {
                    diagnosticCode = "ITEM_SYSTEM_TRAY_CAPACITY_INVALID";
                    return false;
                }
            }

            if (staged.Placements.Count != roster.Length)
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_PUBLICATION_INCOMPLETE";
                return false;
            }
            diagnosticCode = "NONE";
            return true;
        }

        private void RebuildItemSystemAuthorityCatalog()
        {
            DestroyRuntimeShapeConfigs();
            shapeById.Clear();
            itemById.Clear();
            foreach (IGrouping<string, ItemSystemBattleSandboxViewRow> shapeGroup in
                     itemSystemBoardAuthority.Rows
                         .Where(row => row != null)
                         .GroupBy(row => row.ShapeId, StringComparer.Ordinal))
            {
                ItemSystemBattleSandboxViewRow row = shapeGroup.First();
                ItemShapeConfig shape = CreateShape(
                    row.ShapeId,
                    row.ShapeDisplayName,
                    row.RotationAllowed,
                    row.ShapeCells.ToArray());
                shape.hideFlags = HideFlags.HideAndDontSave;
                runtimeShapeConfigs.Add(shape);
                shapeById[shape.shapeId] = shape;
            }

            foreach (ItemSystemBattleSandboxViewRow row in itemSystemBoardAuthority.Rows
                         .Where(value => value != null)
                         .OrderBy(value => value.BaseItemId, StringComparer.Ordinal))
            {
                itemById[row.BaseItemId] = new PreviewItem(
                    row.BaseItemId,
                    row.DisplayName,
                    row.CategoryDisplayName,
                    row.ShapeId,
                    row.ShapeDisplayName,
                    ResolveItemSystemAuthorityCardColor(row),
                    BuildSandboxItemStatCatalog.Resolve(row.BaseItemId),
                    new[] { row.CategoryDisplayName },
                    row.ItemInstanceId);
            }
        }


        private static Color ResolveItemSystemAuthorityCardColor(
            ItemSystemBattleSandboxViewRow row)
        {
            if (row == null)
            {
                return new Color(0.44f, 0.35f, 0.18f, 1f);
            }
            if (row.IsSystemItem)
            {
                return new Color(0.36f, 0.32f, 0.25f, 1f);
            }
            int band = Mathf.Clamp((row.Ordinal - 1) / 6, 0, 4);
            return band switch
            {
                0 => new Color(0.45f, 0.40f, 0.72f, 1f),
                1 => new Color(0.72f, 0.32f, 0.20f, 1f),
                2 => new Color(0.52f, 0.45f, 0.25f, 1f),
                3 => new Color(0.26f, 0.48f, 0.62f, 1f),
                _ => new Color(0.62f, 0.55f, 0.48f, 1f)
            };
        }

        private void DestroyRuntimeShapeConfigs()
        {
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
        }

        private void BuildShapeLookup()
        {
            DestroyRuntimeShapeConfigs();
            shapeById.Clear();
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
                slotCount: ResolveTrayGridSlotCount(),
                commitAllowed: true);
            boardReceiver = new UiBoardShapeGridReceiver(
                "battle_sandbox_x2_board",
                boardGridPreview,
                BoardColumns,
                BoardRows,
                boardSlotByCell,
                ResolveBoardDataCellFromVisualCell,
                BoardVisualRowsAreTopDown());

            foreach (PreviewItem item in OrderInitialTrayLayoutItems(
                         itemById.Values.Where(item => item != null
                             && IsCurrentItemSystemBaseItemAvailable(item.ItemId))))
            {
                shapeAwareTrayGrid.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out _);
            }
            trayMasterLayoutInitialized = true;
        }

        private bool ApplyItemSystemAuthorityArtwork()
        {
            if (itemSystemBoardAuthority == null)
            {
                return false;
            }

            ItemSystemSnapshot snapshot = itemSystemBoardAuthority.CurrentSnapshot;
            foreach (ItemSystemBattleSandboxViewRow row in itemSystemBoardAuthority.Rows
                         .Where(value => value != null))
            {
                bool lit = snapshot?.placements?.FirstOrDefault(placement =>
                    placement != null && string.Equals(placement.itemId,
                        row.BaseItemId, StringComparison.Ordinal))?.isLit == true;
                Sprite sprite = row.ResolveSprite(lit);
                visualStylesByItemId[row.BaseItemId] = new List<ShapeCellVisualStyle>
                {
                    new(
                        sprite,
                        Color.white,
                        Image.Type.Simple,
                        preserveAspect: true,
                        fillCenter: true,
                        material: null,
                        pixelsPerUnitMultiplier: 1f,
                        sourceRotationDegrees: 0f,
                        spansWholeItem: true)
                };
            }

            if (itemTrayView == null)
            {
                return false;
            }
            Dictionary<string, ItemSystemBattleSandboxViewRow> rowsById =
                itemSystemBoardAuthority.Rows
                    .Where(row => row != null)
                    .ToDictionary(row => row.BaseItemId, row => row,
                        StringComparer.Ordinal);
            HashSet<string> boundIds = new(StringComparer.Ordinal);
            HashSet<string> cardIds = new(StringComparer.Ordinal);
            HashSet<string> bindFailedIds = new(StringComparer.Ordinal);
            BuildItemPreviewCardView[] cards = itemTrayView
                .GetComponentsInChildren<BuildItemPreviewCardView>(true);
            foreach (BuildItemPreviewCardView card in cards)
            {
                if (card == null
                    || !rowsById.TryGetValue(card.ItemId,
                        out ItemSystemBattleSandboxViewRow row))
                {
                    continue;
                }
                cardIds.Add(row.BaseItemId);
                ItemSystemPlacementSnapshot placement = snapshot?.placements
                    ?.FirstOrDefault(value => value != null && string.Equals(
                        value.itemId, row.BaseItemId, StringComparison.Ordinal));
                if (card.BindAuthoritativeArtwork(
                        row.ResolveSprite(placement?.isLit == true)))
                {
                    boundIds.Add(row.BaseItemId);
                }
                else
                {
                    bindFailedIds.Add(row.BaseItemId);
                }
            }
            if (boundIds.SetEquals(rowsById.Keys))
            {
                return true;
            }

            string[] missingCardIds = rowsById.Keys
                .Where(id => !cardIds.Contains(id))
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
            string[] missingSpriteIds = rowsById.Values
                .Where(row => row.ResolveSprite(snapshot?.placements?.FirstOrDefault(
                    placement => placement != null && string.Equals(placement.itemId,
                        row.BaseItemId, StringComparison.Ordinal))?.isLit == true) == null)
                .Select(row => row.BaseItemId)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToArray();
            Debug.LogError("[BuildGridInteractionPreviewController]"
                + "[ITEM_SYSTEM_ARTWORK_BINDING_DIAGNOSTIC]"
                + " expected=" + rowsById.Count
                + ";cards=" + cards.Length
                + ";bound=" + boundIds.Count
                + ";missingCards=" + string.Join(",", missingCardIds)
                + ";missingSprites=" + string.Join(",", missingSpriteIds)
                + ";bindFailed=" + string.Join(",", bindFailedIds.OrderBy(
                    id => id, StringComparer.Ordinal)), this);
            return false;
        }

        private void RestoreItemSystemAuthorityArtwork()
        {
            if (itemTrayView == null)
            {
                return;
            }
            foreach (BuildItemPreviewCardView card in itemTrayView
                         .GetComponentsInChildren<BuildItemPreviewCardView>(true))
            {
                card?.RestoreAuthoritativeArtwork();
            }
        }

        private void RebuildCachesFromAcceptedItemSystemSnapshot()
        {
            if (!TryRebuildCachesFromAcceptedItemSystemSnapshot(
                    out string diagnosticCode))
            {
                Debug.LogError(
                    "[BuildGridInteractionPreviewController]["
                    + diagnosticCode + "]",
                    this);
            }
        }

        private bool TryRebuildCachesFromAcceptedItemSystemSnapshot(
            out string diagnosticCode)
        {
            diagnosticCode = string.Empty;
            if (itemSystemBoardAuthority?.CurrentSnapshot == null
                || !itemSystemBoardAuthority.CurrentSnapshot.isValid)
            {
                diagnosticCode = "ITEM_SYSTEM_ACCEPTED_SNAPSHOT_INVALID";
                return false;
            }
            if (itemTrayView == null)
            {
                diagnosticCode = "ITEM_SYSTEM_TRAY_VIEW_MISSING";
                return false;
            }

            ItemSystemSnapshot accepted = itemSystemBoardAuthority.CurrentSnapshot;
            boardReceiver?.ReplaceFromAcceptedSnapshot(accepted.placements);
            placedItemIds.Clear();
            foreach (ItemSystemPlacementSnapshot placement in accepted.placements
                         .Where(value => value != null))
            {
                placedItemIds.Add(placement.itemId);
            }
            if (accepted.i031State?.location == I031Location.Board)
            {
                placedItemIds.Add(I031InventoryPlacementContract.ItemId);
            }
            placementSequence = placedItemIds.Count;

            foreach (ItemSystemBattleSandboxViewRow row in itemSystemBoardAuthority.Rows
                         .Where(value => value != null)
                         .OrderBy(value => value.BaseItemId, StringComparer.Ordinal))
            {
                ItemSystemPlacementSnapshot placement = accepted.placements
                    .FirstOrDefault(value => value != null
                        && string.Equals(value.itemId, row.BaseItemId,
                            StringComparison.Ordinal));
                if (itemById.TryGetValue(row.BaseItemId, out PreviewItem item))
                {
                    item.Rotation = placement == null
                        ? item.Rotation
                        : MapAuthorityRotationToPresentation(
                            ItemSystemBattleSandboxViewProjection.DegreesToRotation(
                                placement.rotation));
                }
            }

            ShapeAwareItemTrayGrid previousMaster = shapeAwareTrayGrid;
            bool previousMasterInitialized = trayMasterLayoutInitialized;
            bool previousResetRequiresNewMaster = resetRequiresNewTrayMasterLayout;
            bool trayTransactionOpen = true;
            itemTrayView.BeginTrayViewTransaction();
            try
            {
                foreach (ItemSystemBattleSandboxViewRow row in itemSystemBoardAuthority.Rows
                             .Where(value => value != null)
                             .OrderBy(value => value.BaseItemId, StringComparer.Ordinal))
                {
                    if (itemById.TryGetValue(row.BaseItemId, out PreviewItem item))
                    {
                        itemTrayView?.SetItemInTray(
                            row.BaseItemId,
                            IsCurrentItemSystemBaseItemAvailable(row.BaseItemId)
                            && !placedItemIds.Contains(row.BaseItemId));
                    }
                }

                if (!ReconcileTrayMasterFromAcceptedItemSystemSnapshot())
                {
                    shapeAwareTrayGrid = previousMaster;
                    trayMasterLayoutInitialized = previousMasterInitialized;
                    resetRequiresNewTrayMasterLayout =
                        previousResetRequiresNewMaster;
                    itemTrayView.RollbackTrayViewTransaction();
                    trayTransactionOpen = false;
                    diagnosticCode = "DEV_ROSTER_TRAY_MASTER_RECONCILE_FAILED";
                    return false;
                }

                foreach (PreviewItem item in itemById.Values
                             .Where(value => value != null
                                 && IsCurrentItemSystemBaseItemAvailable(value.ItemId)
                                 && !placedItemIds.Contains(value.ItemId))
                             .OrderBy(value => value.ItemId, StringComparer.Ordinal))
                {
                    RefreshTrayPlacement(item);
                }

                if (trayTransactionOpen
                    && itemTrayView.CommitTrayViewTransaction() != true)
                {
                    shapeAwareTrayGrid = previousMaster;
                    trayMasterLayoutInitialized = previousMasterInitialized;
                    resetRequiresNewTrayMasterLayout =
                        previousResetRequiresNewMaster;
                    itemTrayView.RollbackTrayViewTransaction();
                    trayTransactionOpen = false;
                    diagnosticCode = "DEV_ROSTER_TRAY_TRANSACTION_COMMIT_FAILED";
                    return false;
                }
                trayTransactionOpen = false;

                foreach (ItemSystemBattleSandboxViewRow row in
                             itemSystemBoardAuthority.Rows.Where(value => value != null))
                {
                    itemTrayView?.SetRotateEnabled(
                        row.BaseItemId,
                        IsCurrentItemSystemBaseItemAvailable(row.BaseItemId)
                        && !placedItemIds.Contains(row.BaseItemId));
                }
                ApplyItemSystemAuthorityArtwork();
                RedrawBoardPlacedVisuals();
                RefreshTrayArrangeButton();
                lastPublishedItemDevRosterSignature =
                    itemSystemBoardAuthority.CurrentDevSessionRosterAvailability
                        ?.CanonicalSignature ?? string.Empty;
                diagnosticCode = "NONE";
                return true;
            }
            catch (Exception exception)
            {
                shapeAwareTrayGrid = previousMaster;
                trayMasterLayoutInitialized = previousMasterInitialized;
                resetRequiresNewTrayMasterLayout =
                    previousResetRequiresNewMaster;
                if (trayTransactionOpen)
                {
                    itemTrayView.RollbackTrayViewTransaction();
                }
                diagnosticCode = "DEV_ROSTER_TRAY_PUBLICATION_EXCEPTION_"
                    + exception.GetType().Name;
                return false;
            }
        }

        private string ResolveItemSystemPlacementId(string baseItemId)
        {
            ItemSystemPlacementSnapshot[] matches =
                (itemSystemBoardAuthority?.CurrentSnapshot?.placements ??
                    Array.Empty<ItemSystemPlacementSnapshot>())
                .Where(value => value != null && string.Equals(
                    value.itemId, baseItemId, StringComparison.Ordinal))
                .ToArray();
            return matches.Length == 1
                ? matches[0].placementId
                : string.Empty;
        }

        private void ResetItemSystemAuthorityPreview()
        {
            ClearRememberedTrayAnchorsForReset();
            mobileInput?.Cancel(boardReceiver);
            ItemSystemBattleSandboxBoardOperationResult result =
                itemSystemBoardAuthority.Reset();
            if (!result.Accepted)
            {
                placementFeedbackView?.ShowInvalid(
                    WithItemSystemAuthorityFeedback(result.ChineseMessage));
                return;
            }
            RebuildCachesFromAcceptedItemSystemSnapshot();
            ResetItemSystemAuthorityInteractionState(result.Changed
                ? "已重置：全部道具回到栏中，棋盘为空。"
                : "当前已经是初始布局。未重复刷新结构诊断。");
        }

        private void ResetItemSystemAuthorityInteractionState(string message)
        {
            activeDragItemId = string.Empty;
            ClearActiveDragContext(restoreOriginalRotation: false);
            hasLastPreviewAnchor = false;
            lastPreviewResult = null;
            lastPreviewSource = ShapePlacementSource.Unknown;
            selectedItem = null;
            ClearPreviewCells();
            HideDragGhost();
            SetRotateZonesVisible(false);
            SetSelectedItemInfoVisible(false);
            UpdateSelectedItemInfo(null);
            itemInfoPanel?.Hide();
            itemSystemDetailAdapter?.Hide();
            sandboxBattleActive = false;
            battlePrepareStateActive = false;
            battlePrepareContinueStateActive = false;
            manaLoopRuntime?.ResetLoop();
            runtimeLoopRuntime?.ResetLoop();
            RefreshBattlePrepareChrome(snapMotion: true);
            placementFeedbackView?.ShowNeutral(
                WithItemSystemAuthorityFeedback(message));
        }

        private string WithItemSystemAuthorityFeedback(string message)
        {
            string aggregate = itemSystemBoardAuthority?
                .CurrentLayoutResilienceSnapshot?.aggregateFeedbackText
                ?? string.Empty;
            return LayoutResilienceBattleSandboxPlaytestFeedback
                .ApplyToExistingFeedback(message ?? string.Empty, aggregate);
        }

        private void EnsureItemSystemAuthorityFeedbackLine()
        {
            if (itemSystemBoardAuthority == null || placementFeedbackView == null)
            {
                return;
            }
            string current = placementFeedbackView.CurrentMessage;
            string next = WithItemSystemAuthorityFeedback(current);
            if (!string.Equals(current, next, StringComparison.Ordinal))
            {
                placementFeedbackView.ShowInfo(next);
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
                slotCount: ResolveTrayGridSlotCount(),
                commitAllowed: true);

            IEnumerable<PreviewItem> items = itemById.Count > 0
                ? itemById.Values
                : CreatePreviewItems();
            foreach (PreviewItem item in OrderTrayPackingItems(
                         items.Where(item => item != null
                             && IsCurrentItemSystemBaseItemAvailable(item.ItemId))))
            {
                shapeAwareTrayGrid.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out _);
            }
        }

        private IEnumerable<PreviewItem> OrderTrayPackingItems(
            IEnumerable<PreviewItem> items)
        {
            IEnumerable<PreviewItem> safeItems =
                (items ?? Array.Empty<PreviewItem>()).Where(item => item != null);
            if (itemSystemBoardAuthority == null)
            {
                return safeItems;
            }
            return safeItems
                .OrderByDescending(item => shapeById.TryGetValue(
                    item.ShapeId, out ItemShapeConfig shape) ? shape.cellCount : 0)
                .ThenBy(item => item.ItemId, StringComparer.Ordinal);
        }

        private bool IsCurrentItemSystemBaseItemAvailable(string baseItemId)
        {
            if (itemSystemBoardAuthority == null)
            {
                return true;
            }
            return itemSystemBoardAuthority.CurrentDevSessionRosterAvailability
                ?.AvailableBaseItemIds.Contains(
                    baseItemId ?? string.Empty,
                    StringComparer.Ordinal) == true;
        }

        private int ResolveTrayGridSlotCount()
        {
            return itemSystemBoardAuthority == null
                ? TrayColumns * TrayRows
                : BuildItemTrayPreviewView.ItemSystemAuthorityLogicalSlotCount;
        }

        private void ClearRememberedTrayAnchorsForReset()
        {
            rememberedTrayAnchorsByIdentity.Clear();
            resetRequiresNewTrayMasterLayout = true;
            trayMasterLayoutInitialized = false;
        }

        private bool ReconcileTrayMasterFromAcceptedItemSystemSnapshot()
        {
            if (shapeAwareTrayGrid == null)
            {
                return false;
            }

            ShapeAwareItemTrayGrid previousMaster = shapeAwareTrayGrid;
            ShapeAwareItemTrayGrid stagedMaster = CreateEmptyTrayMaster();
            List<PreviewItem> trayItems = itemById.Values
                .Where(item => item != null
                    && IsCurrentItemSystemBaseItemAvailable(item.ItemId)
                    && !placedItemIds.Contains(item.ItemId))
                .ToList();
            HashSet<string> stagedItemIds = new(StringComparer.Ordinal);

            if (!resetRequiresNewTrayMasterLayout && trayMasterLayoutInitialized)
            {
                foreach (PreviewItem item in trayItems
                             .OrderBy(value => ResolveTrayLayoutIdentity(value), StringComparer.Ordinal))
                {
                    if (!previousMaster.TryGetPlacement(item.ItemId,
                            out ShapeAwareItemTrayGridPlacement placement)
                        || placement == null
                        || !TryCommitTrayItemAt(stagedMaster, item, placement.AnchorCell, out _))
                    {
                        continue;
                    }
                    stagedItemIds.Add(item.ItemId);
                }
            }

            foreach (PreviewItem item in OrderInitialTrayLayoutItems(
                         trayItems.Where(value => !stagedItemIds.Contains(value.ItemId))))
            {
                if (!TryRestoreOrFindFirstTrayPlacement(stagedMaster, item, out _))
                {
                    // Never publish a partial rebuild.  The authoritative board state remains intact
                    // and the last valid master remains available for a focused diagnostic.
                    return false;
                }
            }

            shapeAwareTrayGrid = stagedMaster;
            trayMasterLayoutInitialized = true;
            resetRequiresNewTrayMasterLayout = false;
            return true;
        }

        private bool TryBuildArrangedTrayMaster(out ShapeAwareItemTrayGrid stagedMaster)
        {
            stagedMaster = CreateEmptyTrayMaster();
            IEnumerable<PreviewItem> trayItems = itemById.Values
                .Where(item => item != null
                    && IsCurrentItemSystemBaseItemAvailable(item.ItemId)
                    && !placedItemIds.Contains(item.ItemId));
            foreach (PreviewItem item in OrderManualTrayArrangeItems(trayItems))
            {
                if (!TryRestoreOrFindFirstTrayPlacement(stagedMaster, item, out _,
                        allowRememberedAnchor: false))
                {
                    stagedMaster = null;
                    return false;
                }
            }
            return true;
        }

        private ShapeAwareItemTrayGrid CreateEmptyTrayMaster()
        {
            return new ShapeAwareItemTrayGrid(
                receiverId: "battle_sandbox_x2_item_tray",
                columnCount: TrayColumns,
                slotCount: ResolveTrayGridSlotCount(),
                commitAllowed: true);
        }

        private bool TryRestoreOrFindFirstTrayPlacement(
            ShapeAwareItemTrayGrid targetGrid,
            PreviewItem item,
            out ShapePlacementResult result,
            bool allowRememberedAnchor = true)
        {
            result = null;
            if (targetGrid == null || item == null)
            {
                return false;
            }

            if (allowRememberedAnchor
                && rememberedTrayAnchorsByIdentity.TryGetValue(
                    ResolveTrayLayoutIdentity(item), out ItemShapeCell rememberedAnchor)
                && TryCommitTrayItemAt(targetGrid, item, rememberedAnchor, out result))
            {
                return true;
            }

            return targetGrid.TryPack(BuildPayload(item, ShapePlacementSource.Tray), out result)
                && result != null
                && result.IsValid;
        }

        private IEnumerable<PreviewItem> OrderInitialTrayLayoutItems(
            IEnumerable<PreviewItem> items)
        {
            // Initial/reset layout is deterministic but must not strand a later
            // complex footprint in fragmented free cells.  Reuse the same
            // package-owned shape-first ordering as explicit Arrange; placement
            // itself still goes through the single tray CanPlace/TryPack path.
            return OrderManualTrayArrangeItems(items);
        }

        private IEnumerable<PreviewItem> OrderManualTrayArrangeItems(
            IEnumerable<PreviewItem> items)
        {
            return (items ?? Array.Empty<PreviewItem>())
                .Where(item => item != null)
                .Select(item => new
                {
                    Item = item,
                    Offsets = BuildPayload(item, ShapePlacementSource.Tray).BuildNormalizedOffsets()
                })
                .OrderByDescending(value => value.Offsets.Count)
                .ThenByDescending(value => IsIrregularFootprint(value.Offsets))
                .ThenByDescending(value => BoundingCellCount(value.Offsets))
                .ThenBy(value => ResolveTrayLayoutIdentity(value.Item), StringComparer.Ordinal)
                .Select(value => value.Item);
        }

        private static bool IsIrregularFootprint(IReadOnlyList<ItemShapeCell> offsets)
        {
            return offsets != null
                && offsets.Count > 0
                && offsets.Count != BoundingCellCount(offsets);
        }

        private static int BoundingCellCount(IReadOnlyList<ItemShapeCell> offsets)
        {
            if (offsets == null || offsets.Count == 0)
            {
                return 0;
            }
            int width = offsets.Max(cell => cell.x) - offsets.Min(cell => cell.x) + 1;
            int height = offsets.Max(cell => cell.y) - offsets.Min(cell => cell.y) + 1;
            return Mathf.Max(0, width * height);
        }

        private static string ResolveTrayLayoutIdentity(PreviewItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            return string.Equals(item.ItemId, I031InventoryPlacementContract.ItemId,
                    StringComparison.Ordinal)
                ? I031InventoryPlacementContract.SpecialIdentityId
                : item.ItemInstanceId;
        }

        private void RememberTrayPlacement(PreviewItem item)
        {
            if (item == null
                || shapeAwareTrayGrid == null
                || !shapeAwareTrayGrid.TryGetPlacement(item.ItemId,
                    out ShapeAwareItemTrayGridPlacement placement)
                || placement == null)
            {
                return;
            }
            rememberedTrayAnchorsByIdentity[ResolveTrayLayoutIdentity(item)] = placement.AnchorCell;
        }

        private bool CanArrangeTrayLayout()
        {
            return shapeAwareTrayGrid != null
                && string.IsNullOrWhiteSpace(activeDragItemId)
                && itemTrayView != null
                && string.Equals(itemTrayView.ActiveCategory,
                    BuildItemTrayPreviewView.AllCategory, StringComparison.Ordinal);
        }

        private void RefreshTrayArrangeButton()
        {
            if (trayArrangeButton != null)
            {
                trayArrangeButton.interactable = CanArrangeTrayLayout();
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

            if (trayArrangeButton != null)
            {
                trayArrangeButton.onClick.RemoveListener(ArrangeTrayLayout);
                trayArrangeButton.onClick.AddListener(ArrangeTrayLayout);
            }
            RefreshTrayArrangeButton();
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
            battlePrepareNormalPosition = battlePrepareOpenPosition + new Vector2(0f, battleStateYOffset);
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
                PlayBattlePrepareTransitionSequence();
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
            PlayBattlePrepareTransitionSequence();
            RefreshBattlePrepareChrome(snapMotion: false);
            placementFeedbackView?.ShowInfo("V0.4 沙盒整备界面已打开；可在道具栏与棋盘间拖动道具。");
        }

        private void PlayBattlePrepareTransitionSequence()
        {
            EnsureBattlePrepareTransitionSequencePlayer();
            battlePrepareTransitionSequencePlayer?.PlayFromStart();
        }

        private void EnsureBattlePrepareTransitionSequencePlayer()
        {
            if (battlePrepareTransitionSequencePlayer != null)
            {
                return;
            }

            battlePrepareTransitionSequencePlayer = GetComponentInChildren<BuildSandboxSpriteSequencePlayer>(true);
            if (battlePrepareTransitionSequencePlayer != null)
            {
                return;
            }

            BuildSandboxSpriteSequencePlayer fallback = null;
            foreach (BuildSandboxSpriteSequencePlayer player in FindObjectsOfType<BuildSandboxSpriteSequencePlayer>(true))
            {
                if (player == null)
                {
                    continue;
                }

                if (string.Equals(player.gameObject.name, BattlePrepareTransitionSequenceObjectName, StringComparison.OrdinalIgnoreCase))
                {
                    battlePrepareTransitionSequencePlayer = player;
                    return;
                }

                if (fallback == null && player.FrameCount > 0)
                {
                    fallback = player;
                }
            }

            battlePrepareTransitionSequencePlayer = fallback;
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
            SetGameObjectActive(enemyCombatFeedbackFloatingRoot, false);
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

        private bool BeginHoldingItem(
            PreviewItem item,
            bool showInfoPanel,
            ShapePlacementSource source = ShapePlacementSource.Tray,
            ItemShapeCell? boardAnchor = null,
            ItemShapeCell? grabbedCell = null)
        {
            if (item == null || mobileInput == null)
            {
                return false;
            }

            ClearActiveDragContext(restoreOriginalRotation: true);
            selectedItem = item;
            activeDragOriginalRotation = item.Rotation;
            hasActiveDragOriginalRotation = true;
            ShapeItemPayload payload = BuildPayload(selectedItem, source);
            ItemShapeCell? trayAnchor = null;
            if (source == ShapePlacementSource.Tray
                && itemTrayView != null
                && itemTrayView.TryGetDisplayedPlacement(item.ItemId,
                    out TrayPlacementViewModel displayedPlacement)
                && displayedPlacement != null
                && displayedPlacement.anchorSlotIndex >= 0)
            {
                trayAnchor = new ItemShapeCell(
                    displayedPlacement.anchorSlotIndex % TrayColumns,
                    displayedPlacement.anchorSlotIndex / TrayColumns);
            }
            else if (source == ShapePlacementSource.Tray
                && shapeAwareTrayGrid != null
                && shapeAwareTrayGrid.TryGetPlacement(item.ItemId, out ShapeAwareItemTrayGridPlacement placement))
            {
                trayAnchor = placement.AnchorCell;
            }

            ItemShapeCell? sourceAnchor = source == ShapePlacementSource.Board
                ? boardAnchor
                : trayAnchor;
            if (!TryCaptureActiveDragGrabbedCell(
                    payload,
                    source,
                    sourceAnchor,
                    grabbedCell))
            {
                ClearActiveDragContext(restoreOriginalRotation: true);
                return false;
            }

            bool began = source == ShapePlacementSource.Board
                ? mobileInput.TapBoardItem(payload, boardAnchor)
                : mobileInput.TapTrayItem(payload, trayAnchor);
            if (!began)
            {
                ClearActiveDragContext(restoreOriginalRotation: true);
                return false;
            }

            ClearPreviewCells();
            SetSelectedItemInfoVisible(true);
            UpdateSelectedItemInfo(item);
            if (showInfoPanel)
            {
                ShowItemInfoPanel(item);
            }

            RefreshTrayPlacement(item);
            placementFeedbackView?.ShowInfo($"正在拖动“{item.DisplayName}”。棋盘上向右下角按钮顺时针旋转，合法位置松手放置。");
            return true;
        }

        private bool TryResolveTrayGrabbedCell(
            PreviewItem item,
            PointerEventData eventData,
            out ItemShapeCell grabbedCell)
        {
            grabbedCell = default;
            return item != null
                && eventData != null
                && itemTrayView != null
                && itemTrayView.TryScreenPointToTrayCell(
                    eventData.pressPosition,
                    eventData.pressEventCamera,
                    out grabbedCell)
                && TryGetTrayPlacement(
                    item.ItemId,
                    out ShapeAwareItemTrayGridPlacement placement)
                && placement != null
                && placement.OccupiedCells.Contains(grabbedCell);
        }

        private bool TryResolveCurrentBoardAnchor(
            PreviewItem item,
            out ItemShapeCell anchorCell)
        {
            anchorCell = default;
            if (boardReceiver == null || item == null)
            {
                return false;
            }

            ItemShapeCell[] occupiedCells = boardReceiver.OccupiedCells
                .Where(pair => string.Equals(
                    pair.Value,
                    item.ItemId,
                    StringComparison.Ordinal))
                .Select(pair => pair.Key)
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
            if (occupiedCells.Length == 0)
            {
                return false;
            }

            ItemShapeCell candidateAnchor = new(
                occupiedCells.Min(cell => cell.x),
                occupiedCells.Min(cell => cell.y));
            ShapeItemPayload payload = BuildPayload(
                item,
                ShapePlacementSource.Board);
            if (!HaveSameOrderedCells(
                    payload.BuildOccupiedCells(candidateAnchor),
                    occupiedCells))
            {
                return false;
            }

            anchorCell = candidateAnchor;
            return true;
        }

        private bool TryCaptureActiveDragGrabbedCell(
            ShapeItemPayload payload,
            ShapePlacementSource source,
            ItemShapeCell? sourceAnchor,
            ItemShapeCell? grabbedCell)
        {
            hasActiveDragGrabbedBaseOffset = false;
            activeDragGrabbedBaseOffset = default;
            if (!payload.IsValid
                || !sourceAnchor.HasValue
                || !grabbedCell.HasValue)
            {
                return false;
            }

            ItemShapeCell localGrabbedCell = new(
                grabbedCell.Value.x - sourceAnchor.Value.x,
                grabbedCell.Value.y - sourceAnchor.Value.y);
            foreach (ItemShapeCell baseOffset in payload.OccupiedOffsets)
            {
                if (!ResolveFootprintOffset(payload, baseOffset, source)
                        .Equals(localGrabbedCell))
                {
                    continue;
                }

                activeDragGrabbedBaseOffset = baseOffset;
                hasActiveDragGrabbedBaseOffset = true;
                return true;
            }

            return false;
        }

        private ItemShapeCell ResolveActiveDragAnchorFromGrabbedCell(
            ShapeItemPayload payload,
            ShapePlacementSource target,
            ItemShapeCell grabbedTargetCell)
        {
            if (!hasActiveDragGrabbedBaseOffset || !payload.IsValid)
            {
                return grabbedTargetCell;
            }

            ItemShapeCell localOffset = ResolveFootprintOffset(
                payload,
                activeDragGrabbedBaseOffset,
                target);
            return new ItemShapeCell(
                grabbedTargetCell.x - localOffset.x,
                grabbedTargetCell.y - localOffset.y);
        }

        private static ItemShapeCell ResolveFootprintOffset(
            ShapeItemPayload payload,
            ItemShapeCell baseOffset,
            ShapePlacementSource coordinateSpace)
        {
            ItemShapeCell normalizedOffset = ResolveNormalizedBoardOffset(
                payload,
                baseOffset);
            if (coordinateSpace != ShapePlacementSource.Tray)
            {
                return normalizedOffset;
            }

            IReadOnlyList<ItemShapeCell> normalizedOffsets =
                payload.BuildNormalizedOffsets();
            int maxY = normalizedOffsets.Count == 0
                ? 0
                : normalizedOffsets.Max(cell => cell.y);
            return new ItemShapeCell(
                normalizedOffset.x,
                maxY - normalizedOffset.y);
        }

        private static ItemShapeCell ResolveNormalizedBoardOffset(
            ShapeItemPayload payload,
            ItemShapeCell baseOffset)
        {
            ItemShapeCell[] rotatedOffsets = payload.OccupiedOffsets
                .Select(offset => ApplyRotation(offset, payload.Rotation))
                .ToArray();
            if (rotatedOffsets.Length == 0)
            {
                return default;
            }

            int minX = rotatedOffsets.Min(cell => cell.x);
            int minY = rotatedOffsets.Min(cell => cell.y);
            ItemShapeCell rotatedGrabbedOffset = ApplyRotation(
                baseOffset,
                payload.Rotation);
            return new ItemShapeCell(
                rotatedGrabbedOffset.x - minX,
                rotatedGrabbedOffset.y - minY);
        }

        private static ItemShapeCell ResolveItemSystemAuthorityAnchor(
            ShapeItemPayload payload,
            ItemShapeCell normalizedAnchor)
        {
            ItemShapeCell[] rotatedOffsets = payload.OccupiedOffsets
                .Select(offset => ApplyRotation(offset, payload.Rotation))
                .ToArray();
            if (rotatedOffsets.Length == 0)
            {
                return normalizedAnchor;
            }

            int minX = rotatedOffsets.Min(cell => cell.x);
            int minY = rotatedOffsets.Min(cell => cell.y);
            return new ItemShapeCell(
                normalizedAnchor.x - minX,
                normalizedAnchor.y - minY);
        }

        private void ClearActiveDragContext(bool restoreOriginalRotation)
        {
            if (restoreOriginalRotation
                && hasActiveDragOriginalRotation
                && selectedItem != null)
            {
                selectedItem.Rotation = activeDragOriginalRotation;
            }

            hasActiveDragGrabbedBaseOffset = false;
            activeDragGrabbedBaseOffset = default;
            hasActiveDragOriginalRotation = false;
            activeDragOriginalRotation = ItemShapeRotation.Rotation0;
            hasActiveBoardCandidate = false;
            activeBoardCandidate = default;
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

        private bool IsBattleInteractionLocked()
        {
            return sandboxBattleActive && !battlePrepareStateActive;
        }

        private void ShowBattleLockedDragToast()
        {
            placementFeedbackView?.ShowInfo(IsBattleInteractionLocked()
                ? "阵势已启，战后可整备"
                : "请先打开整备界面再移动道具。");
        }

        private bool CanActiveDragUseRotateZones()
        {
            if (!CanDragActiveItem()
                || selectedItem == null
                || !shapeById.TryGetValue(selectedItem.ShapeId, out ItemShapeConfig shapeConfig)
                || shapeConfig == null
                || shapeConfig.cellCount <= 1
                || shapeConfig.cellCount >= 4)
            {
                return false;
            }

            return ItemRotationInputExtension.CanRotate(BuildPayload(selectedItem, placementSession.SourceContainer));
        }

        private bool UpdateRotateZoneDuringDrag(PointerEventData eventData)
        {
            bool canUseRotateZones = CanActiveDragUseRotateZones() && HasActiveBoardRotatePreview();
            SetRotateZonesVisible(canUseRotateZones);
            if (!canUseRotateZones || eventData == null)
            {
                ResetRotateZoneEntry();
                return false;
            }

            UpdateRotateZoneViewRects(eventData);
            if (currentRotateZoneSide != MobileRotateZoneSide.None)
            {
                SetRotateZoneHighlight(ResolveRotateZoneHighlightSide());
                if (activeRotateSeekExitRect.Contains(eventData.position))
                {
                    return false;
                }

                currentRotateZoneSide = MobileRotateZoneSide.None;
                PrimeRotateSeekAnchor(eventData.position);
                SetRotateZoneHighlight(MobileRotateZoneSide.None);
                return false;
            }

            if (activeRotateSeekSide != MobileRotateZoneSide.None)
            {
                if (Time.unscaledTime - rotateSeekAnchorTime > RotateSeekWindowSeconds)
                {
                    activeRotateSeekSide = MobileRotateZoneSide.None;
                    PrimeRotateSeekAnchor(eventData.position);
                    SetRotateZoneHighlight(MobileRotateZoneSide.None);
                    return false;
                }

                if (activeRotateSeekTargetRect.Contains(eventData.position))
                {
                    currentRotateZoneSide = activeRotateSeekSide;
                    activeRotateSeekSide = MobileRotateZoneSide.None;
                    if (TryTriggerRotateZone(currentRotateZoneSide, eventData))
                    {
                        HoldRotateInteractionVisuals(eventData);
                        return true;
                    }

                    return false;
                }

                return false;
            }

            if (TryBeginRotateSeek(eventData, out bool didTriggerRotate))
            {
                if (didTriggerRotate)
                {
                    HoldRotateInteractionVisuals(eventData);
                    return true;
                }

                return false;
            }

            SetRotateZoneHighlight(MobileRotateZoneSide.None);
            PrimeRotateSeekAnchorIfNeeded(eventData.position);
            return false;
        }

        private void HoldRotateInteractionVisuals(PointerEventData eventData)
        {
            ShapePlacementResult displayResult = lastPreviewSource == ShapePlacementSource.Board
                ? lastPreviewResult
                : null;
            DrawPreviewResult(displayResult, locked: false);
            UpdateRotateZoneViewRects(eventData);
        }

        private bool TryBeginRotateSeek(PointerEventData eventData, out bool didTriggerRotate)
        {
            didTriggerRotate = false;
            if (eventData == null)
            {
                return false;
            }

            if (!hasRotateSeekAnchor)
            {
                PrimeRotateSeekAnchor(eventData.position);
                return false;
            }

            float age = Time.unscaledTime - rotateSeekAnchorTime;
            if (age < RotateSeekMinAgeSeconds)
            {
                return false;
            }

            Vector2 delta = eventData.position - rotateSeekAnchorPosition;
            if (delta.x < RotateSeekMinDeltaXPixels
                || delta.y > RotateSeekMaxUpwardDriftPixels
                || delta.x / Mathf.Max(age, 0.001f) < RotateSeekMinVelocityXPixelsPerSecond)
            {
                if (age > RotateSeekWindowSeconds)
                {
                    PrimeRotateSeekAnchor(eventData.position);
                }

                return false;
            }

            MobileRotateZoneSide side = MobileRotateZoneSide.Right;
            if (!TryGetRotateButtonActivationScreenRect(
                    eventData,
                    out Rect rightRect))
            {
                PrimeRotateSeekAnchor(eventData.position);
                return false;
            }

            Rect targetRect = rightRect;
            activeRotateSeekSide = side;
            activeRotateSeekTargetRect = targetRect;
            activeRotateSeekExitRect = ExpandRect(targetRect, RotateButtonExitPaddingPixels);
            rotateSeekAnchorPosition = eventData.position;
            rotateSeekAnchorTime = Time.unscaledTime;
            hasRotateSeekAnchor = true;
            SetRotateZoneHighlight(MobileRotateZoneSide.None);

            if (targetRect.Contains(eventData.position))
            {
                currentRotateZoneSide = side;
                activeRotateSeekSide = MobileRotateZoneSide.None;
                didTriggerRotate = TryTriggerRotateZone(side, eventData);
            }

            return true;
        }

        private bool TryTriggerRotateZone(MobileRotateZoneSide zoneSide, PointerEventData eventData)
        {
            float now = Time.unscaledTime;
            if (now - lastRotateZoneTime < RotateButtonTriggerCooldownSeconds
                || !TryRotateActiveDragFromZone(zoneSide, eventData))
            {
                return false;
            }

            lastRotateZoneTime = now;
            confirmedRotateZoneSide = zoneSide;
            rotateZoneConfirmUntilTime = now + RotateButtonConfirmVisualSeconds;
            return true;
        }

        private void PrimeRotateSeekAnchorIfNeeded(Vector2 position)
        {
            if (!hasRotateSeekAnchor
                || Time.unscaledTime - rotateSeekAnchorTime > RotateSeekWindowSeconds)
            {
                PrimeRotateSeekAnchor(position);
            }
        }

        private void PrimeRotateSeekAnchor(Vector2 position)
        {
            rotateSeekAnchorPosition = position;
            rotateSeekAnchorTime = Time.unscaledTime;
            hasRotateSeekAnchor = true;
        }

        private bool TryRotateActiveDragFromZone(MobileRotateZoneSide zoneSide, PointerEventData eventData)
        {
            if (!CanActiveDragUseRotateZones()
                || placementSession == null
                || boardReceiver == null
                || selectedItem == null)
            {
                return false;
            }

            if (!hasActiveDragGrabbedBaseOffset)
            {
                return false;
            }

            if (!TryUseActiveDragCoordinateSpace(
                    ShapePlacementSource.Board,
                    out ShapeItemPayload previousPayload))
            {
                return false;
            }

            ItemShapeRotation nextRotation = ResolveClockwiseRotation(selectedItem.Rotation);
            bool hasBoardAnchor = TryResolveRotateAnchor(out ItemShapeCell anchorCell);
            ItemShapeCell grabbedBoardCell = anchorCell;
            if (hasBoardAnchor)
            {
                ItemShapeCell previousGrabbedOffset = ResolveFootprintOffset(
                    previousPayload,
                    activeDragGrabbedBaseOffset,
                    ShapePlacementSource.Board);
                grabbedBoardCell = new ItemShapeCell(
                    anchorCell.x + previousGrabbedOffset.x,
                    anchorCell.y + previousGrabbedOffset.y);
            }
            if (!placementSession.RotateTo(
                    MapPresentationRotationToAuthority(nextRotation)))
            {
                return false;
            }

            selectedItem.Rotation = nextRotation;
            if (hasBoardAnchor)
            {
                ItemShapeCell nextGrabbedOffset = ResolveFootprintOffset(
                    placementSession.CurrentPayload,
                    activeDragGrabbedBaseOffset,
                    ShapePlacementSource.Board);
                ItemShapeCell previewAnchor = new(
                    grabbedBoardCell.x - nextGrabbedOffset.x,
                    grabbedBoardCell.y - nextGrabbedOffset.y);
                ShapePlacementResult preview = placementSession.Preview(boardReceiver, previewAnchor);
                preview = ApplyItemSystemAuthorityPreview(preview);
                lastPreviewResult = preview;
                hasLastPreviewAnchor = preview != null;
                lastPreviewAnchor = previewAnchor;
                lastPreviewSource = ShapePlacementSource.Board;
                DrawPreviewResult(preview, locked: false);
                ShowDragGhost(selectedItem, eventData, "Dragging", preview, ShapePlacementSource.Board);
            }
            else
            {
                ClearPreviewCells();
                lastPreviewResult = null;
                hasLastPreviewAnchor = false;
                lastPreviewSource = ShapePlacementSource.Unknown;
                ShowDragGhost(selectedItem, eventData, "Dragging", null, ShapePlacementSource.Board);
            }

            UpdateSelectedItemInfo(selectedItem);
            RefreshItemInfoPanel(selectedItem);
            placementFeedbackView?.ShowValid($"已顺时针旋转“{selectedItem.DisplayName}”：{FormatRotation(selectedItem.Rotation)}。");
            return true;
        }

        private bool TryResolveRotateAnchor(out ItemShapeCell anchorCell)
        {
            if (lastPreviewSource == ShapePlacementSource.Board
                && lastPreviewResult != null
                && lastPreviewResult.OccupiedCells.Count > 0)
            {
                anchorCell = lastPreviewResult.AnchorCell;
                return true;
            }

            anchorCell = default;
            return false;
        }

        private bool HasActiveBoardRotatePreview()
        {
            return lastPreviewSource == ShapePlacementSource.Board
                && lastPreviewResult != null
                && lastPreviewResult.OccupiedCells.Count > 0;
        }

        private bool TryGetRotateButtonVisualScreenRect(
            PointerEventData eventData,
            out Rect rightRect)
        {
            return TryBuildRotateButtonScreenRect(
                eventData,
                Vector2.one * RotateButtonVisualSizePixels,
                out rightRect);
        }

        private bool TryGetRotateButtonActivationScreenRect(
            PointerEventData eventData,
            out Rect rightRect)
        {
            return TryBuildRotateButtonScreenRect(
                eventData,
                new Vector2(RotateButtonActivationWidthPixels, RotateButtonActivationHeightPixels),
                out rightRect);
        }

        private bool TryBuildRotateButtonScreenRect(
            PointerEventData eventData,
            Vector2 buttonSize,
            out Rect rightRect)
        {
            rightRect = default;
            if (!TryGetDragGhostScreenRect(eventData, out Rect ghostRect))
            {
                return false;
            }

            float visualInset = RotateButtonVisualSizePixels * 0.5f
                + RotateButtonInsideEdgeInsetPixels;
            float xInset = Mathf.Min(visualInset, ghostRect.width * 0.36f);
            float yInset = Mathf.Min(visualInset, ghostRect.height * 0.36f);
            Vector2 rightBottomCenter = new(ghostRect.xMax - xInset, ghostRect.yMin + yInset);
            rightRect = BuildCenteredScreenRect(
                rightBottomCenter,
                buttonSize);
            return true;
        }

        private bool TryGetDragGhostScreenRect(PointerEventData eventData, out Rect screenRect)
        {
            screenRect = default;
            if (dragGhostRoot != null && dragGhostRoot.gameObject.activeInHierarchy)
            {
                Camera eventCamera = eventData?.pressEventCamera ?? eventData?.enterEventCamera;
                Vector3[] corners = new Vector3[4];
                dragGhostRoot.GetWorldCorners(corners);
                float minX = float.PositiveInfinity;
                float minY = float.PositiveInfinity;
                float maxX = float.NegativeInfinity;
                float maxY = float.NegativeInfinity;
                for (int i = 0; i < corners.Length; i++)
                {
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
                    minX = Mathf.Min(minX, point.x);
                    minY = Mathf.Min(minY, point.y);
                    maxX = Mathf.Max(maxX, point.x);
                    maxY = Mathf.Max(maxY, point.y);
                }

                if (maxX > minX && maxY > minY)
                {
                    screenRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
                    return true;
                }
            }

            if (eventData == null)
            {
                return false;
            }

            Vector2 fallbackCenter = eventData.position
                + new Vector2(0f, MobileShapePlacementInputSettings.DefaultFingerGhostOffsetPixels);
            Vector2 fallbackSize = hasDragGhostDefaults && dragGhostDefaultSize != Vector2.zero
                ? dragGhostDefaultSize
                : Vector2.one * 96f;
            screenRect = BuildCenteredScreenRect(fallbackCenter, fallbackSize);
            return true;
        }

        private static Rect BuildCenteredScreenRect(Vector2 center, Vector2 size)
        {
            Vector2 halfSize = size * 0.5f;
            return Rect.MinMaxRect(
                center.x - halfSize.x,
                center.y - halfSize.y,
                center.x + halfSize.x,
                center.y + halfSize.y);
        }

        private static Rect ExpandRect(Rect rect, float padding)
        {
            return Rect.MinMaxRect(
                rect.xMin - padding,
                rect.yMin - padding,
                rect.xMax + padding,
                rect.yMax + padding);
        }

        private void ResetRotateZoneEntry()
        {
            currentRotateZoneSide = MobileRotateZoneSide.None;
            activeRotateSeekSide = MobileRotateZoneSide.None;
            activeRotateSeekTargetRect = default;
            activeRotateSeekExitRect = default;
            hasRotateSeekAnchor = false;
            rotateSeekAnchorTime = -999f;
            confirmedRotateZoneSide = MobileRotateZoneSide.None;
            rotateZoneConfirmUntilTime = -999f;
        }

        private static ItemShapeRotation ResolveClockwiseRotation(ItemShapeRotation rotation)
        {
            return rotation switch
            {
                ItemShapeRotation.Rotation0 => ItemShapeRotation.Rotation90,
                ItemShapeRotation.Rotation90 => ItemShapeRotation.Rotation180,
                ItemShapeRotation.Rotation180 => ItemShapeRotation.Rotation270,
                _ => ItemShapeRotation.Rotation0
            };
        }

        private void UpdateActiveDrag(PointerEventData eventData)
        {
            itemTrayView?.TryAutoScrollDuringDrag(
                eventData.position,
                eventData.pressEventCamera,
                Time.unscaledDeltaTime);

            if (UpdateRotateZoneDuringDrag(eventData))
            {
                return;
            }

            if (TryPreviewBoardSnapHold(eventData, out ShapePlacementResult heldBoardResult))
            {
                ApplyBoardDragPreview(eventData, heldBoardResult);
                return;
            }

            if (TryPreviewTrayDrag(eventData, out ShapePlacementResult trayResult))
            {
                lastPreviewResult = trayResult;
                hasLastPreviewAnchor = trayResult != null;
                lastPreviewSource = ShapePlacementSource.Tray;
                if (trayResult != null)
                {
                    lastPreviewAnchor = trayResult.AnchorCell;
                }

                ClearPreviewCells();
                ShowDragGhost(selectedItem, eventData, "拖动中", trayResult, ShapePlacementSource.Tray);
                SetRotateZonesVisible(false);
                ResetRotateZoneEntry();

                return;
            }

            ShapePlacementResult result = PreviewActiveBoardDrag(eventData);
            ApplyBoardDragPreview(eventData, result);
        }

        private bool TryUseActiveDragCoordinateSpace(
            ShapePlacementSource target,
            out ShapeItemPayload payload)
        {
            payload = default;
            if (placementSession == null
                || !placementSession.HasActivePayload
                || selectedItem == null)
            {
                return false;
            }

            ItemShapeRotation targetRotation = target == ShapePlacementSource.Board
                ? MapPresentationRotationToAuthority(selectedItem.Rotation)
                : selectedItem.Rotation;
            if (placementSession.CurrentPayload.Rotation != targetRotation
                && !placementSession.RotateTo(targetRotation))
            {
                return false;
            }

            payload = placementSession.CurrentPayload;
            return payload.IsValid;
        }

        private ShapePlacementResult PreviewActiveBoardDrag(
            PointerEventData eventData)
        {
            if (eventData == null
                || placementSession == null
                || boardReceiver == null
                || !placementSession.HasActivePayload
                || !hasActiveDragGrabbedBaseOffset)
            {
                return null;
            }

            if (!TryUseActiveDragCoordinateSpace(
                    ShapePlacementSource.Board,
                    out ShapeItemPayload boardPayload))
            {
                return null;
            }

            placementSession.UpdatePointer(eventData.position);
            if (!boardReceiver.ScreenPointToCell(
                    eventData.position,
                    eventData.pressEventCamera,
                    out ItemShapeCell grabbedTargetCell))
            {
                return mobileInput?.DragToReceiver(
                    boardReceiver,
                    eventData.position,
                    eventData.pressEventCamera);
            }

            ItemShapeCell normalizedAnchor =
                ResolveActiveDragAnchorFromGrabbedCell(
                    boardPayload,
                    ShapePlacementSource.Board,
                    grabbedTargetCell);
            return placementSession.Preview(boardReceiver, normalizedAnchor);
        }

        private bool TryPreviewBoardSnapHold(PointerEventData eventData, out ShapePlacementResult result)
        {
            result = null;
            if (!ShouldHoldBoardSnapPreview(eventData))
            {
                return false;
            }

            placementSession.UpdatePointer(eventData.position);
            result = placementSession.Preview(
                boardReceiver,
                lastPreviewAnchor);
            return true;
        }

        private ItemShapeCell ClampBoardAnchorForPayload(
            ShapeItemPayload payload,
            ItemShapeCell anchorCell)
        {
            if (boardReceiver == null || !payload.IsValid)
            {
                return anchorCell;
            }

            IReadOnlyList<ItemShapeCell> offsets = payload.BuildNormalizedOffsets();
            if (offsets == null || offsets.Count == 0)
            {
                return anchorCell;
            }

            int maxOffsetX = offsets.Max(cell => cell.x);
            int maxOffsetY = offsets.Max(cell => cell.y);
            int maxAnchorX = Mathf.Max(0, boardReceiver.Width - 1 - maxOffsetX);
            int maxAnchorY = Mathf.Max(0, boardReceiver.Height - 1 - maxOffsetY);
            return new ItemShapeCell(
                Mathf.Clamp(anchorCell.x, 0, maxAnchorX),
                Mathf.Clamp(anchorCell.y, 0, maxAnchorY));
        }

        private bool ShouldHoldBoardSnapPreview(PointerEventData eventData)
        {
            if (eventData == null
                || placementSession == null
                || boardReceiver == null
                || !hasLastPreviewAnchor
                || lastPreviewSource != ShapePlacementSource.Board
                || !CanActiveDragUseRotateZones()
                || !HasActiveBoardRotatePreview())
            {
                return false;
            }

            if (activeRotateSeekSide != MobileRotateZoneSide.None
                || currentRotateZoneSide != MobileRotateZoneSide.None
                || ResolveRotateZoneHighlightSide() != MobileRotateZoneSide.None)
            {
                return true;
            }

            if (!TryGetRotateButtonActivationScreenRect(eventData, out Rect rightRect))
            {
                return false;
            }

            Rect rightHoldRect = ExpandRect(rightRect, BoardSnapRotateHoldPaddingPixels);
            return rightHoldRect.Contains(eventData.position);
        }

        private void ApplyBoardDragPreview(PointerEventData eventData, ShapePlacementResult result)
        {
            result = ApplyItemSystemAuthorityPreview(result);
            lastPreviewResult = result;
            hasLastPreviewAnchor = result != null;
            lastPreviewSource = ShapePlacementSource.Board;
            if (result != null)
            {
                lastPreviewAnchor = result.AnchorCell;
            }

            DrawPreviewResult(result, locked: false);
            ShowDragGhost(selectedItem, eventData, "拖动中", result, ShapePlacementSource.Board);
            if (CanActiveDragUseRotateZones() && HasActiveBoardRotatePreview())
            {
                SetRotateZonesVisible(true);
                UpdateRotateZoneViewRects(eventData);
            }
            else
            {
                SetRotateZonesVisible(false);
                ResetRotateZoneEntry();
            }

            if (result != null && result.IsValid)
            {
                return;
            }
        }

        private ShapePlacementResult ApplyItemSystemAuthorityPreview(
            ShapePlacementResult cachePreview)
        {
            hasActiveBoardCandidate = false;
            activeBoardCandidate = default;
            if (cachePreview == null
                || selectedItem == null
                || placementSession == null
                || !placementSession.HasActivePayload)
            {
                return cachePreview;
            }

            ShapeItemPayload payload = placementSession.CurrentPayload;
            ItemShapeCell authorityAnchor = ResolveItemSystemAuthorityAnchor(
                payload,
                cachePreview.AnchorCell);
            ShapePlacementResult resolvedPreview = cachePreview;
            if (itemSystemBoardAuthority != null && cachePreview.IsValid)
            {
                ItemSystemBattleSandboxBoardOperationResult authorityPreview =
                    itemSystemBoardAuthority.PreviewPlacement(
                        selectedItem.ItemId,
                        authorityAnchor,
                        payload.Rotation);
                bool footprintMatches = authorityPreview.Accepted
                    && authorityPreview.OccupiedCells.Count > 0
                    && HaveSameOrderedCells(
                        authorityPreview.OccupiedCells,
                        cachePreview.OccupiedCells);
                bool accepted = authorityPreview.Accepted && footprintMatches;
                ShapePlacementInvalidReason invalidReason = accepted
                    ? ShapePlacementInvalidReason.None
                    : authorityPreview.Accepted
                        ? ShapePlacementInvalidReason.ShapeInvalid
                        : ResolveItemSystemInvalidReason(
                            authorityPreview.DiagnosticCode);
                resolvedPreview = new ShapePlacementResult(
                    cachePreview.ItemId,
                    cachePreview.ShapeId,
                    cachePreview.AnchorCell,
                    accepted
                        ? OrderCells(authorityPreview.OccupiedCells)
                        : cachePreview.OccupiedCells,
                    accepted,
                    invalidReason,
                    cachePreview.AdjacentItems,
                    cachePreview.EnergyConnected,
                    accepted
                        ? "item_system_authority_preview"
                        : "item_system_authority_preview_rejected");
            }

            activeBoardCandidate = new BoardDragPlacementCandidate(
                payload.ItemId,
                cachePreview.AnchorCell,
                authorityAnchor,
                payload.Rotation,
                resolvedPreview.OccupiedCells,
                resolvedPreview.IsValid,
                resolvedPreview.InvalidReason);
            hasActiveBoardCandidate = true;
            return resolvedPreview;
        }

        private static ShapePlacementInvalidReason ResolveItemSystemInvalidReason(
            string diagnosticCode)
        {
            return diagnosticCode switch
            {
                "ITEM_OUT_OF_BOUNDS" => ShapePlacementInvalidReason.OutOfGrid,
                "PLACEMENT_OVERLAP" => ShapePlacementInvalidReason.CellOccupied,
                "EYE_CELL_COVERED" => ShapePlacementInvalidReason.CellOccupied,
                _ => ShapePlacementInvalidReason.ShapeInvalid
            };
        }

        private bool TryGetActiveBoardCandidate(
            ShapePlacementResult previewResult,
            out BoardDragPlacementCandidate candidate)
        {
            candidate = activeBoardCandidate;
            if (!hasActiveBoardCandidate
                || previewResult == null
                || placementSession == null
                || !placementSession.HasActivePayload)
            {
                return false;
            }

            ShapeItemPayload payload = placementSession.CurrentPayload;
            return string.Equals(
                    candidate.ItemId,
                    payload.ItemId,
                    StringComparison.Ordinal)
                && candidate.NormalizedAnchor.Equals(previewResult.AnchorCell)
                && candidate.AuthorityRotation == payload.Rotation
                && candidate.IsValid == previewResult.IsValid
                && candidate.InvalidReason == previewResult.InvalidReason
                && HaveSameOrderedCells(
                    candidate.OccupiedCells,
                    previewResult.OccupiedCells);
        }

        private static bool HaveSameOrderedCells(
            IEnumerable<ItemShapeCell> left,
            IEnumerable<ItemShapeCell> right)
        {
            return OrderCells(left).SequenceEqual(OrderCells(right));
        }

        private static ItemShapeCell[] OrderCells(
            IEnumerable<ItemShapeCell> cells)
        {
            return (cells ?? Array.Empty<ItemShapeCell>())
                .Distinct()
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
        }

        private void EndActiveDrag(string itemId, PointerEventData eventData)
        {
            if (TryCommitTrayDrag(itemId, eventData))
            {
                return;
            }

            ShapePlacementResult result = PreviewActiveBoardDrag(eventData);
            result = ApplyItemSystemAuthorityPreview(result);
            lastPreviewResult = result;
            hasLastPreviewAnchor = result != null;
            lastPreviewSource = ShapePlacementSource.Board;
            if (result != null)
            {
                lastPreviewAnchor = result.AnchorCell;
            }

            if (result == null
                || !result.IsValid
                || !TryGetActiveBoardCandidate(
                    result,
                    out BoardDragPlacementCandidate candidate))
            {
                mobileInput?.Cancel(boardReceiver);
                activeDragItemId = string.Empty;
                ClearActiveDragContext(restoreOriginalRotation: true);
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                return;
            }

            if (itemSystemBoardAuthority != null)
            {
                bool movedByAuthority = placementSession.SourceContainer
                    == ShapePlacementSource.Board;
                ItemSystemBattleSandboxBoardOperationResult authorityResult =
                    movedByAuthority
                        ? itemSystemBoardAuthority.CommitMove(
                            ResolveItemSystemPlacementId(selectedItem.ItemId),
                            candidate.AuthorityAnchor,
                            candidate.AuthorityRotation)
                        : itemSystemBoardAuthority.CommitFromTray(
                            selectedItem.ItemId,
                            candidate.AuthorityAnchor,
                            candidate.AuthorityRotation);
                if (!authorityResult.Accepted)
                {
                    mobileInput?.Cancel(boardReceiver);
                    activeDragItemId = string.Empty;
                    ClearActiveDragContext(restoreOriginalRotation: true);
                    itemTrayView?.SetRotateEnabled(itemId,
                        !placedItemIds.Contains(itemId));
                    RefreshItemInfoPanel(selectedItem);
                    ClearPreviewCells();
                    HideDragGhost();
                    placementFeedbackView?.ShowInvalid(
                        WithItemSystemAuthorityFeedback(
                            authorityResult.ChineseMessage));
                    return;
                }

                if (!movedByAuthority)
                {
                    RememberTrayPlacement(selectedItem);
                }
                RebuildCachesFromAcceptedItemSystemSnapshot();
                lastPreviewResult = result;
                activeDragItemId = string.Empty;
                ClearActiveDragContext(restoreOriginalRotation: false);
                itemTrayView?.SetRotateEnabled(selectedItem.ItemId, false);
                mobileInput?.Cancel(boardReceiver);
                HideDragGhost();
                placementFeedbackView?.ShowValid(
                    WithItemSystemAuthorityFeedback(movedByAuthority
                        ? $"已移动“{selectedItem.DisplayName}”。"
                        : $"已放置“{selectedItem.DisplayName}”。"));
                UpdateSelectedItemInfo(selectedItem);
                RefreshItemInfoPanel(selectedItem);
                RefreshTrayArrangeButton();
                return;
            }

            ShapePlacementResult commitResult = placementSession.Commit(boardReceiver);
            if (commitResult == null || !commitResult.IsValid)
            {
                mobileInput?.Cancel(boardReceiver);
                activeDragItemId = string.Empty;
                ClearActiveDragContext(restoreOriginalRotation: true);
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                return;
            }

            bool movedPlacedItem = placedItemIds.Contains(selectedItem.ItemId);
            if (!movedPlacedItem)
            {
                RememberTrayPlacement(selectedItem);
            }
            placedItemIds.Add(selectedItem.ItemId);
            placementSequence = placedItemIds.Count;
            shapeAwareTrayGrid?.RemoveItem(selectedItem.ItemId);
            itemTrayView?.SetItemInTray(selectedItem.ItemId, false);
            RedrawBoardPlacedVisuals();

            lastPreviewResult = commitResult;
            activeDragItemId = string.Empty;
            ClearActiveDragContext(restoreOriginalRotation: false);
            itemTrayView?.SetRotateEnabled(selectedItem.ItemId, false);
            mobileInput?.Cancel(boardReceiver);
            HideDragGhost();
            placementFeedbackView?.ShowValid(movedPlacedItem
                ? $"已移动“{selectedItem.DisplayName}”。"
                : $"已放置“{selectedItem.DisplayName}”。");
            UpdateSelectedItemInfo(selectedItem);
            RefreshItemInfoPanel(selectedItem);
            RefreshTrayArrangeButton();
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

            if (placementSession.SourceContainer == ShapePlacementSource.Board)
            {
                if (!itemTrayView.TryScreenPointToTrayCell(
                        eventData.position,
                        eventData.pressEventCamera,
                        out _))
                {
                    return false;
                }

                if (!TryUseActiveDragCoordinateSpace(
                        ShapePlacementSource.Tray,
                        out _))
                {
                    return false;
                }

                hasActiveBoardCandidate = false;
                activeBoardCandidate = default;

                if (rememberedTrayAnchorsByIdentity.TryGetValue(
                        ResolveTrayLayoutIdentity(selectedItem), out ItemShapeCell rememberedAnchor))
                {
                    result = placementSession.Preview(shapeAwareTrayGrid, rememberedAnchor);
                    if (result != null && result.IsValid)
                    {
                        return true;
                    }
                }

                if (shapeAwareTrayGrid.TryFindFirstLegalAnchor(
                        placementSession.CurrentPayload, out ItemShapeCell firstLegalAnchor))
                {
                    result = placementSession.Preview(shapeAwareTrayGrid, firstLegalAnchor);
                }
                return true;
            }

            if (!itemTrayView.TryScreenPointToTrayCell(
                    eventData.position,
                    eventData.pressEventCamera,
                    out ItemShapeCell anchorCell))
            {
                return false;
            }

            if (!TryUseActiveDragCoordinateSpace(
                    ShapePlacementSource.Tray,
                    out ShapeItemPayload trayPayload))
            {
                return false;
            }

            hasActiveBoardCandidate = false;
            activeBoardCandidate = default;
            ItemShapeCell normalizedAnchor =
                ResolveActiveDragAnchorFromGrabbedCell(
                    trayPayload,
                    ShapePlacementSource.Tray,
                    anchorCell);
            result = placementSession.Preview(
                shapeAwareTrayGrid,
                normalizedAnchor);
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
            lastPreviewSource = ShapePlacementSource.Tray;
            if (result != null)
            {
                lastPreviewAnchor = result.AnchorCell;
            }

            if (result == null || !result.IsValid)
            {
                mobileInput?.Cancel(shapeAwareTrayGrid);
                activeDragItemId = string.Empty;
                ClearActiveDragContext(restoreOriginalRotation: true);
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshTrayPlacement(selectedItem);
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                placementFeedbackView?.ShowInvalid(
                    "道具栏没有合法空位；已恢复拖动前状态。");
                return true;
            }

            if (itemSystemBoardAuthority != null
                && placementSession.SourceContainer == ShapePlacementSource.Board)
            {
                ItemSystemBattleSandboxBoardOperationResult authorityResult =
                    itemSystemBoardAuthority.ReturnToTray(
                        ResolveItemSystemPlacementId(selectedItem.ItemId));
                if (!authorityResult.Accepted)
                {
                    mobileInput?.Cancel(boardReceiver);
                    activeDragItemId = string.Empty;
                    ClearActiveDragContext(restoreOriginalRotation: true);
                    itemTrayView?.SetRotateEnabled(itemId, false);
                    RefreshItemInfoPanel(selectedItem);
                    ClearPreviewCells();
                    HideDragGhost();
                    placementFeedbackView?.ShowInvalid(
                        WithItemSystemAuthorityFeedback(
                            authorityResult.ChineseMessage));
                    return true;
                }

                RebuildCachesFromAcceptedItemSystemSnapshot();
                lastPreviewResult = result;
                activeDragItemId = string.Empty;
                ClearActiveDragContext(restoreOriginalRotation: false);
                mobileInput?.Cancel(shapeAwareTrayGrid);
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
                placementFeedbackView?.ShowValid(
                    WithItemSystemAuthorityFeedback(
                        $"已移动“{selectedItem.DisplayName}”到道具栏空位。"));
                UpdateSelectedItemInfo(selectedItem);
                RefreshTrayArrangeButton();
                return true;
            }

            ShapePlacementResult commitResult = placementSession.Commit(shapeAwareTrayGrid);
            if (commitResult == null || !commitResult.IsValid)
            {
                mobileInput?.Cancel(shapeAwareTrayGrid);
                activeDragItemId = string.Empty;
                ClearActiveDragContext(restoreOriginalRotation: true);
                itemTrayView?.SetRotateEnabled(itemId, !placedItemIds.Contains(itemId));
                RefreshTrayPlacement(selectedItem);
                RefreshItemInfoPanel(selectedItem);
                ClearPreviewCells();
                HideDragGhost();
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
            ClearActiveDragContext(restoreOriginalRotation: false);
            itemTrayView?.SetItemInTray(selectedItem.ItemId, true);
            itemTrayView?.SetRotateEnabled(selectedItem.ItemId, !placedItemIds.Contains(selectedItem.ItemId));
            mobileInput?.Cancel(shapeAwareTrayGrid);
            RefreshTrayPlacement(selectedItem);

            RefreshItemInfoPanel(selectedItem);
            ClearPreviewCells();
            HideDragGhost();
            placementFeedbackView?.ShowValid($"已移动“{selectedItem.DisplayName}”到道具栏空位。");
            UpdateSelectedItemInfo(selectedItem);
            RefreshTrayArrangeButton();
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
            result = ApplyItemSystemAuthorityPreview(result);
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
            return TryCommitTrayItemAt(shapeAwareTrayGrid, item, anchorCell, out result);
        }

        private bool TryCommitTrayItemAt(
            ShapeAwareItemTrayGrid targetGrid,
            PreviewItem item,
            ItemShapeCell anchorCell,
            out ShapePlacementResult result)
        {
            result = null;
            if (targetGrid == null || item == null)
            {
                return false;
            }

            ShapePlacementSession traySession = new();
            traySession.Begin(BuildPayload(item, ShapePlacementSource.Tray), trayAnchorCell: anchorCell);
            result = traySession.Commit(targetGrid);
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
                source == ShapePlacementSource.Board
                    ? MapPresentationRotationToAuthority(item.Rotation)
                    : item.Rotation,
                shapeConfig.occupiedOffsets,
                source);
        }

        private static ItemShapeRotation MapPresentationRotationToAuthority(
            ItemShapeRotation presentationRotation)
        {
            return presentationRotation switch
            {
                ItemShapeRotation.Rotation90 => ItemShapeRotation.Rotation270,
                ItemShapeRotation.Rotation180 => ItemShapeRotation.Rotation180,
                ItemShapeRotation.Rotation270 => ItemShapeRotation.Rotation90,
                _ => ItemShapeRotation.Rotation0
            };
        }

        private static ItemShapeRotation MapAuthorityRotationToPresentation(
            ItemShapeRotation authorityRotation)
        {
            return authorityRotation switch
            {
                ItemShapeRotation.Rotation90 => ItemShapeRotation.Rotation270,
                ItemShapeRotation.Rotation180 => ItemShapeRotation.Rotation180,
                ItemShapeRotation.Rotation270 => ItemShapeRotation.Rotation90,
                _ => ItemShapeRotation.Rotation0
            };
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

            CacheItemVisualStyles(result.ItemId);
            Color itemColor = ResolvePreviewItemColor(result.ItemId);
            ShapeCellVisualStyle wholeItemStyle = !locked
                ? ResolveWholeItemVisualStyle(result.ItemId)
                : null;
            bool suppressCellBlocks = wholeItemStyle != null;
            for (int i = 0; i < result.OccupiedCells.Count; i++)
            {
                ItemShapeCell cell = result.OccupiedCells[i];
                if (boardSlotByCell.TryGetValue(cell, out BuildGridPreviewSlotView slot))
                {
                    if (locked && result.IsValid)
                    {
                        slot.SetLockedPreview();
                    }
                    else
                    {
                        slot.SetPreview(
                            result.IsValid,
                            itemColor,
                            suppressCellBlocks || !result.IsValid ? null : ResolveItemVisualStyle(result.ItemId, i),
                            suppressCellBlocks);
                    }
                }
            }

            if (wholeItemStyle != null)
            {
                DrawBoardPreviewArtwork(
                    result,
                    wholeItemStyle,
                    itemColor,
                    ResolveItemRotation(result.ItemId),
                    ResolveWholeItemArtworkDragGhostRotationOffsetDegrees(result.ItemId),
                    result.IsValid);
            }
        }

        private void ClearPreviewCells()
        {
            ClearBoardPreviewArtwork();
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                slot?.ClearPreview();
            }
        }

        private void RedrawBoardPlacedVisuals()
        {
            ClearBoardPreviewArtwork();
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                slot?.ClearPlaced();
            }
            ClearBoardPlacedArtwork();

            if (boardReceiver == null)
            {
                RefreshFormationPowerVisuals();
                return;
            }

            Dictionary<string, int> visualIndexByItemId = new(StringComparer.Ordinal);
            Dictionary<string, List<ItemShapeCell>> occupiedCellsByItemId = new(StringComparer.Ordinal);
            foreach (KeyValuePair<ItemShapeCell, string> occupied in boardReceiver.OccupiedCells
                         .OrderBy(pair => pair.Value, StringComparer.Ordinal)
                         .ThenBy(pair => pair.Key.y)
                         .ThenBy(pair => pair.Key.x))
            {
                if (!occupiedCellsByItemId.TryGetValue(occupied.Value, out List<ItemShapeCell> itemCells))
                {
                    itemCells = new List<ItemShapeCell>();
                    occupiedCellsByItemId[occupied.Value] = itemCells;
                }

                itemCells.Add(occupied.Key);
                if (!boardSlotByCell.TryGetValue(occupied.Key, out BuildGridPreviewSlotView slot)
                    || slot == null)
                {
                    continue;
                }

                ShapeCellVisualStyle wholeItemStyle = ResolveWholeItemVisualStyle(occupied.Value);
                visualIndexByItemId.TryGetValue(occupied.Value, out int visualIndex);
                ShapeCellVisualStyle visualStyle = wholeItemStyle == null
                    ? ResolveItemVisualStyle(occupied.Value, visualIndex)
                    : null;
                visualIndexByItemId[occupied.Value] = visualIndex + 1;
                if (itemById.TryGetValue(occupied.Value, out PreviewItem item))
                {
                    slot.SetPlaced(
                        item.DisplayName,
                        item.CardColor,
                        visualStyle,
                        wholeItemStyle != null,
                        wholeItemStyle != null);
                }
                else
                {
                    slot.SetPlaced(
                        occupied.Value,
                        ResolvePreviewItemColor(occupied.Value),
                        visualStyle,
                        wholeItemStyle != null,
                        wholeItemStyle != null);
                }
            }

            foreach (KeyValuePair<string, List<ItemShapeCell>> pair in occupiedCellsByItemId)
            {
                ShapeCellVisualStyle wholeItemStyle = ResolveWholeItemVisualStyle(pair.Key);
                if (wholeItemStyle != null)
                {
                    DrawBoardPlacedArtwork(
                        pair.Key,
                        pair.Value,
                        wholeItemStyle,
                        ResolvePreviewItemColor(pair.Key),
                        ResolveItemRotation(pair.Key),
                        ResolveWholeItemArtworkBoardPlacedRotationOffsetDegrees(pair.Key));
                }
            }

            RefreshFormationPowerVisuals();
        }

        private void DrawBoardPreviewArtwork(
            ShapePlacementResult result,
            ShapeCellVisualStyle style,
            Color fallbackColor,
            ItemShapeRotation rotation,
            float stateRotationOffsetDegrees,
            bool valid)
        {
            if (result == null
                || style == null
                || !style.SpansWholeItem
                || style.Sprite == null
                || !TryBuildBoardCellVisualLayout(result.OccupiedCells, out ShapeCellVisualLayout layout)
                || !EnsureBoardArtworkView(
                    BoardPreviewArtworkName,
                    out boardPreviewArtwork,
                    out boardPreviewArtworkImage)
                || !EnsureBoardArtworkView(
                    BoardPreviewShadowArtworkName,
                    out boardPreviewShadowArtwork,
                    out boardPreviewShadowArtworkImage))
            {
                ClearBoardPreviewArtwork();
                return;
            }

            ApplyBoardArtwork(
                boardPreviewShadowArtwork,
                boardPreviewShadowArtworkImage,
                layout,
                style,
                fallbackColor,
                BoardPreviewShadowTint.a,
                rotation,
                stateRotationOffsetDegrees,
                BoardPreviewShadowTint,
                BoardPreviewShadowOffset,
                BoardPreviewShadowScale);

            ApplyBoardArtwork(
                boardPreviewArtwork,
                boardPreviewArtworkImage,
                layout,
                style,
                fallbackColor,
                0.96f,
                rotation,
                stateRotationOffsetDegrees);

            if (valid
                || !EnsureBoardArtworkView(
                    BoardPreviewInvalidArtworkName,
                    out boardPreviewInvalidArtwork,
                    out boardPreviewInvalidArtworkImage))
            {
                ClearBoardPreviewInvalidArtwork();
                return;
            }

            ApplyBoardArtwork(
                boardPreviewInvalidArtwork,
                boardPreviewInvalidArtworkImage,
                layout,
                style,
                fallbackColor,
                BoardPreviewInvalidTint.a,
                rotation,
                stateRotationOffsetDegrees,
                BoardPreviewInvalidTint,
                Vector2.zero,
                1f);
        }

        private void DrawBoardPlacedArtwork(
            string itemId,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            ShapeCellVisualStyle style,
            Color fallbackColor,
            ItemShapeRotation rotation,
            float stateRotationOffsetDegrees)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || occupiedCells == null
                || occupiedCells.Count == 0
                || style == null
                || !style.SpansWholeItem
                || style.Sprite == null
                || !TryBuildBoardCellVisualLayout(occupiedCells, out ShapeCellVisualLayout layout)
                || !EnsureBoardArtworkView(
                    BoardPlacedArtworkNamePrefix + SanitizeRuntimeObjectName(itemId),
                    out RectTransform artworkRect,
                    out Image artworkImage))
            {
                return;
            }

            boardPlacedArtworkByItemId[itemId] = artworkRect;
            ApplyBoardArtwork(artworkRect, artworkImage, layout, style, fallbackColor, 1f, rotation, stateRotationOffsetDegrees);
        }

        private void ApplyBoardArtwork(
            RectTransform artworkRect,
            Image artworkImage,
            ShapeCellVisualLayout layout,
            ShapeCellVisualStyle style,
            Color fallbackColor,
            float maxAlpha,
            ItemShapeRotation rotation,
            float stateRotationOffsetDegrees)
        {
            ApplyBoardArtwork(
                artworkRect,
                artworkImage,
                layout,
                style,
                fallbackColor,
                maxAlpha,
                rotation,
                stateRotationOffsetDegrees,
                null,
                Vector2.zero,
                1f);
        }

        private void ApplyBoardArtwork(
            RectTransform artworkRect,
            Image artworkImage,
            ShapeCellVisualLayout layout,
            ShapeCellVisualStyle style,
            Color fallbackColor,
            float maxAlpha,
            ItemShapeRotation rotation,
            float stateRotationOffsetDegrees,
            Color? tintColor,
            Vector2 localOffset,
            float scaleMultiplier)
        {
            if (artworkRect == null || artworkImage == null || layout == null || style == null)
            {
                return;
            }

            artworkRect.gameObject.SetActive(true);
            ApplyWholeItemArtworkTransform(
                artworkRect,
                layout.AnchoredPosition,
                layout.SizeDelta,
                rotation,
                style.SourceRotationDegrees + stateRotationOffsetDegrees);
            artworkRect.anchoredPosition += localOffset;
            float safeScale = Mathf.Max(0.01f, scaleMultiplier);
            artworkRect.localScale = new Vector3(safeScale, safeScale, 1f);
            artworkRect.SetAsLastSibling();
            artworkImage.raycastTarget = false;
            ApplyArtworkStyle(artworkImage, style, fallbackColor, maxAlpha, tintColor);
        }

        private static void ApplyArtworkStyle(
            Image image,
            ShapeCellVisualStyle style,
            Color fallbackColor,
            float maxAlpha,
            Color? tintColor)
        {
            if (image == null || style == null)
            {
                return;
            }

            if (!tintColor.HasValue)
            {
                style.ApplyTo(image, fallbackColor, maxAlpha);
                return;
            }

            image.overrideSprite = null;
            image.sprite = style.Sprite;
            image.type = style.Sprite == null ? Image.Type.Simple : style.ImageType;
            image.preserveAspect = style.PreserveAspect;
            image.fillCenter = style.FillCenter;
            image.material = style.Material;
            image.pixelsPerUnitMultiplier = style.PixelsPerUnitMultiplier;

            Color targetColor = tintColor.Value;
            if (style.Sprite == null && Mathf.Approximately(targetColor.a, 0f))
            {
                targetColor = fallbackColor;
            }

            targetColor.a = Mathf.Min(targetColor.a, maxAlpha);
            image.color = targetColor;
        }

        private void ClearBoardPreviewArtwork()
        {
            if (boardPreviewArtwork != null)
            {
                boardPreviewArtwork.gameObject.SetActive(false);
            }

            if (boardPreviewShadowArtwork != null)
            {
                boardPreviewShadowArtwork.gameObject.SetActive(false);
            }

            ClearBoardPreviewInvalidArtwork();
        }

        private void ClearBoardPreviewInvalidArtwork()
        {
            if (boardPreviewInvalidArtwork != null)
            {
                boardPreviewInvalidArtwork.gameObject.SetActive(false);
            }
        }

        private void ClearBoardPlacedArtwork()
        {
            foreach (RectTransform artwork in boardPlacedArtworkByItemId.Values)
            {
                if (artwork != null)
                {
                    artwork.gameObject.SetActive(false);
                }
            }
        }

        private bool EnsureBoardArtworkView(
            string objectName,
            out RectTransform artworkRect,
            out Image artworkImage)
        {
            artworkRect = null;
            artworkImage = null;
            RectTransform layer = EnsureBoardArtworkLayer();
            if (layer == null || string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            Transform existing = layer.Find(objectName);
            GameObject target = existing == null
                ? new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement))
                : existing.gameObject;
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            target.transform.SetParent(layer, false);
            LayoutElement layoutElement = target.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }

            artworkRect = target.GetComponent<RectTransform>();
            artworkImage = target.GetComponent<Image>();
            if (artworkImage == null)
            {
                artworkImage = target.AddComponent<Image>();
            }

            artworkImage.raycastTarget = false;
            return true;
        }

        private RectTransform EnsureBoardArtworkLayer()
        {
            if (boardGridPreview == null)
            {
                return null;
            }

            if (boardArtworkLayer != null && boardArtworkLayer.parent == boardGridPreview)
            {
                boardArtworkLayer.SetAsLastSibling();
                return boardArtworkLayer;
            }

            Transform existing = boardGridPreview.Find(BoardArtworkLayerName);
            GameObject layerObject = existing == null
                ? new GameObject(BoardArtworkLayerName, typeof(RectTransform), typeof(LayoutElement))
                : existing.gameObject;
            layerObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            layerObject.transform.SetParent(boardGridPreview, false);
            boardArtworkLayer = layerObject.GetComponent<RectTransform>();
            LayoutElement layoutElement = layerObject.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                layoutElement.ignoreLayout = true;
            }

            StretchToParent(boardArtworkLayer);
            boardArtworkLayer.SetAsLastSibling();
            return boardArtworkLayer;
        }

        private static string SanitizeRuntimeObjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unknown";
            }

            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '_' && chars[i] != '-')
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private Color ResolvePreviewItemColor(string itemId)
        {
            return itemById.TryGetValue(itemId ?? string.Empty, out PreviewItem item)
                ? item.CardColor
                : new Color(0.44f, 0.35f, 0.18f, 1f);
        }

        private void RefreshFormationPowerVisuals()
        {
            EnsureFormationPowerOverlays();
            ClearFormationPowerVisuals();

            if (formationPowerOverlayByCell.Count == 0)
            {
                return;
            }

            if (itemSystemBoardAuthority != null)
            {
                RenderItemSystemFormationPowerVisuals(
                    itemSystemBoardAuthority.CurrentSnapshot);
                return;
            }

            BuildSandboxLayoutSnapshot snapshot = BuildCurrentLayoutSnapshot();
            FormationEnergyContractPreview preview = FormationEnergyContractResolver.Apply(snapshot);
            if (preview == null)
            {
                return;
            }

            ItemShapeCell eyeCell = preview.eyeConfig?.eyeCell ?? FormationCorePowerRangeResolver.DefaultCoreCell;
            ResolveFormationPowerOverlay(eyeCell)?.ShowCore();

            foreach (ItemShapeCell cell in preview.eyeRuntimeData?.basePulseCells ?? new List<ItemShapeCell>())
            {
                if (cell.Equals(eyeCell))
                {
                    continue;
                }

                ResolveFormationPowerOverlay(cell)?.ShowWeakPulseRange();
            }

            foreach (FormationEnergyContractRow row in preview.rows ?? new List<FormationEnergyContractRow>())
            {
                if (row == null
                    || !row.isEnergyStoneSource
                    || row.energyState != EnergyState.Powered)
                {
                    continue;
                }

                foreach (ItemShapeCell cell in row.powerRangeCells ?? new List<ItemShapeCell>())
                {
                    if (cell.Equals(eyeCell))
                    {
                        continue;
                    }

                    ResolveFormationPowerOverlay(cell)?.ShowPoweredRange();
                }
            }

            foreach (FormationEnergyContractRow row in preview.rows ?? new List<FormationEnergyContractRow>())
            {
                if (row == null)
                {
                    continue;
                }

                Color color = ResolveFormationEnergyBadgeColor(row);
                string badge = ResolveFormationEnergyBadge(row);
                foreach (ItemShapeCell cell in row.occupiedCells ?? new List<ItemShapeCell>())
                {
                    FormationPowerCellOverlay overlay = ResolveFormationPowerOverlay(cell);
                    if (overlay == null)
                    {
                        continue;
                    }

                    overlay.ShowItemState(badge, color);
                    if (row.touchesEyeCell || row.energyState == EnergyState.Suppressed)
                    {
                        overlay.ShowEyeCellOccupied();
                    }
                }
            }
        }

        private void RenderItemSystemFormationPowerVisuals(
            ItemSystemSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.isValid)
            {
                return;
            }

            ResolveFormationPowerOverlay(new ItemShapeCell(
                snapshot.eyeCell.x, snapshot.eyeCell.y))?.ShowCore();
            foreach (Vector2Int cell in snapshot.LitRangeCells)
            {
                ResolveFormationPowerOverlay(
                    new ItemShapeCell(cell.x, cell.y))?.ShowPoweredRange();
            }

            foreach (ItemSystemPlacementSnapshot placement in snapshot.placements
                         .Where(value => value != null))
            {
                string badge = placement.isLightingSource
                    ? "供能源·不计Build"
                    : placement.isDirectLit
                        ? (placement.isCountedInBuild
                            ? "直亮·计Build" : "直亮·不计Build")
                        : placement.isLit
                            ? (placement.isCountedInBuild
                                ? "阵脉·计Build" : "阵脉·不计Build")
                            : "未点亮·不计Build";
                Color color = placement.isLit
                    ? new Color(1f, 0.76f, 0.22f, 0.98f)
                    : new Color(0.72f, 0.74f, 0.70f, 0.92f);
                foreach (Vector2Int cell in placement.OccupiedCells)
                {
                    FormationPowerCellOverlay overlay = ResolveFormationPowerOverlay(
                        new ItemShapeCell(cell.x, cell.y));
                    overlay?.ShowItemState(badge, color);
                    if (cell == snapshot.eyeCell)
                    {
                        overlay?.ShowEyeCellOccupied();
                    }
                }
            }
        }

        private void ClearFormationPowerVisuals()
        {
            foreach (FormationPowerCellOverlay overlay in
                     formationPowerOverlayByCell.Values)
            {
                overlay?.Clear();
            }
        }

        private void EnsureFormationPowerOverlays()
        {
            foreach (KeyValuePair<ItemShapeCell, BuildGridPreviewSlotView> pair in boardSlotByCell)
            {
                if (pair.Value == null || formationPowerOverlayByCell.ContainsKey(pair.Key))
                {
                    continue;
                }

                RectTransform slotRect = pair.Value.transform as RectTransform;
                if (slotRect == null)
                {
                    continue;
                }

                formationPowerOverlayByCell[pair.Key] = FormationPowerCellOverlay.Create(slotRect);
            }
        }

        private FormationPowerCellOverlay ResolveFormationPowerOverlay(ItemShapeCell cell)
        {
            return formationPowerOverlayByCell.TryGetValue(cell, out FormationPowerCellOverlay overlay)
                ? overlay
                : null;
        }

        private static Color ResolveFormationEnergyBadgeColor(FormationEnergyContractRow row)
        {
            if (row == null)
            {
                return new Color(0.8f, 0.82f, 0.78f, 0.95f);
            }

            return row.energyState switch
            {
                EnergyState.Powered => new Color(1f, 0.76f, 0.22f, 0.98f),
                EnergyState.WeakPulse => new Color(0.58f, 0.82f, 1f, 0.96f),
                EnergyState.Suppressed => new Color(0.88f, 0.28f, 0.28f, 0.98f),
                _ => new Color(0.72f, 0.74f, 0.70f, 0.92f)
            };
        }

        private static string ResolveFormationEnergyBadge(FormationEnergyContractRow row)
        {
            if (row == null)
            {
                return string.Empty;
            }

            return row.energyState switch
            {
                EnergyState.Powered => "\u805a\u80fd\u4f9b\u80fd",
                EnergyState.WeakPulse => "\u9635\u8109\u5fae\u4eae",
                EnergyState.Suppressed => "\u88ab\u538b\u5236",
                _ => "\u672a\u4f9b\u80fd"
            };
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
                selectedItemInfoBody.text = "单击查看信息；按住拖动摆放；棋盘上可向右下角按钮顺时针旋转。";
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
                $"供能：{ResolveSelectedEnergyStateText(item)}\n" +
                $"旋转：{FormatRotation(item.Rotation)}\n" +
                $"状态：{state}";
        }

        private string ResolveSelectedEnergyStateText(PreviewItem item)
        {
            if (item == null || !placedItemIds.Contains(item.ItemId))
            {
                return "\u672a\u653e\u7f6e";
            }

            if (itemSystemBoardAuthority != null)
            {
                ItemSystemPlacementSnapshot placement =
                    itemSystemBoardAuthority.CurrentSnapshot?.placements
                        ?.SingleOrDefault(value => value != null && string.Equals(
                            value.itemId, item.ItemId, StringComparison.Ordinal));
                if (placement == null)
                {
                    return "未放置";
                }
                if (placement.isLightingSource)
                {
                    return "供能源";
                }
                return placement.isDirectLit
                    ? "直接点亮"
                    : placement.isLit
                        ? "阵脉点亮"
                        : "未供能";
            }

            FormationEnergyContractPreview preview =
                FormationEnergyContractResolver.Apply(BuildCurrentLayoutSnapshot());
            FormationEnergyContractRow row = preview?.rows?.FirstOrDefault(candidate =>
                candidate != null
                && string.Equals(candidate.itemId, item.ItemId, StringComparison.Ordinal));
            return ResolveEnergyStateText(row?.energyState ?? EnergyState.None);
        }

        private static string ResolveEnergyStateText(EnergyState state)
        {
            return state switch
            {
                EnergyState.Powered => "\u805a\u80fd\u4f9b\u80fd",
                EnergyState.WeakPulse => "\u9635\u8109\u5fae\u4eae",
                EnergyState.Suppressed => "\u88ab\u538b\u5236",
                _ => "\u672a\u4f9b\u80fd"
            };
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
            if (itemSystemBoardAuthority != null)
            {
                itemInfoPanel?.Hide();
                if (item != null)
                {
                    ShowQualifiedItemSystemDetail(item);
                }
                else
                {
                    itemSystemDetailAdapter?.Hide();
                }
                return;
            }
            if (itemInfoPanel == null || item == null)
            {
                return;
            }

            itemInfoPanel.Show(item, BuildItemInfoContext(item), CanRotateItemFromInfoPanel(item));
        }

        private void RefreshItemInfoPanel(PreviewItem item)
        {
            if (itemSystemBoardAuthority != null)
            {
                itemInfoPanel?.Hide();
                if (item != null && itemSystemDetailAdapter?.IsVisible == true
                    && string.Equals(itemSystemDetailAdapter.CurrentBaseItemId,
                        item.ItemId, StringComparison.Ordinal))
                {
                    ShowQualifiedItemSystemDetail(item);
                }
                return;
            }
            if (itemInfoPanel == null || item == null)
            {
                return;
            }

            itemInfoPanel.RefreshIfShowing(item, BuildItemInfoContext(item), CanRotateItemFromInfoPanel(item));
        }

        private bool ShowQualifiedItemSystemDetail(PreviewItem item)
        {
            if (item == null
                || itemSystemBoardAuthority == null
                || itemSystemDetailAdapter == null)
            {
                itemSystemDetailAdapter?.Hide();
                return false;
            }

            ItemSystemBattleSandboxViewRow[] exactRows =
                (itemSystemBoardAuthority.Rows ??
                    Array.Empty<ItemSystemBattleSandboxViewRow>())
                .Where(row => row != null
                    && string.Equals(row.BaseItemId, item.ItemId,
                        StringComparison.Ordinal))
                .ToArray();
            if (exactRows.Length != 1)
            {
                itemSystemDetailAdapter.Hide();
                return false;
            }

            ItemSystemBattleSandboxViewRow row = exactRows[0];
            return itemSystemDetailAdapter.Show(
                row.BaseItemId,
                row.ItemInstanceId,
                ResolveItemSystemPlacementId(row.BaseItemId),
                itemSystemBoardAuthority.CurrentSnapshot,
                itemSystemBoardAuthority.CurrentQualifiedBuildState,
                itemSystemBoardAuthority.CurrentCoreEffectRuntimeState);
        }

        private void RotateInfoPanelItem(string itemId)
        {
            placementFeedbackView?.ShowInfo("本包改为拖动热区旋转；请拖到棋盘后向右下角按钮顺时针旋转。");
        }

        private bool CanRotateItemFromInfoPanel(PreviewItem item)
        {
            return false;
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

        private void SetRotateZonesVisible(bool visible)
        {
            if (!visible)
            {
                if (rotateZoneLayer != null)
                {
                    rotateZoneLayer.gameObject.SetActive(false);
                }

                SetRotateZoneHighlight(MobileRotateZoneSide.None);
                return;
            }

            EnsureRotateZoneViews();
            if (rotateZoneLayer == null)
            {
                return;
            }

            rotateZoneLayer.gameObject.SetActive(true);
            rotateZoneLayer.SetAsLastSibling();
            if (rotateZoneLeft != null)
            {
                rotateZoneLeft.gameObject.SetActive(false);
            }

            if (rotateZoneRight != null)
            {
                rotateZoneRight.gameObject.SetActive(true);
            }
        }

        private void UpdateRotateZoneViewRects(PointerEventData eventData)
        {
            EnsureRotateZoneViews();
            if (rotateZoneLayer == null
                || !TryGetDragGhostScreenRect(eventData, out Rect ghostRect)
                || !TryGetRotateButtonVisualScreenRect(eventData, out Rect rightRect))
            {
                return;
            }

            if (rotateZoneLeft != null)
            {
                rotateZoneLeft.gameObject.SetActive(false);
            }

            SetRotateZoneRect(rotateZoneRight, rightRect, eventData);
            MobileRotateZoneSide highlightSide = ResolveRotateZoneHighlightSide();
            MobileRotateZoneSide guideSide = ResolveRotateZoneGuideSide();
            SetRotateZoneHighlight(highlightSide);
            UpdateRotateZoneGuideRect(eventData, ghostRect, rightRect, guideSide);
        }

        private void SetRotateZoneHighlight(MobileRotateZoneSide side)
        {
            if (rotateZoneLeft != null)
            {
                rotateZoneLeft.gameObject.SetActive(false);
            }

            SetRotateZoneButtonVisual(
                MobileRotateZoneSide.Right,
                rotateZoneRight,
                rotateZoneRightImage,
                rotateZoneRightText,
                side);
            if (side == MobileRotateZoneSide.None && rotateZoneGuide != null)
            {
                rotateZoneGuide.gameObject.SetActive(false);
            }
        }

        private void SetRotateZoneButtonVisual(
            MobileRotateZoneSide buttonSide,
            RectTransform rect,
            Image image,
            Text text,
            MobileRotateZoneSide activeSide)
        {
            bool active = activeSide == buttonSide;
            bool dimmed = activeSide != MobileRotateZoneSide.None && !active;
            if (image != null)
            {
                image.color = active
                    ? new Color(0.18f, 0.50f, 0.40f, 0.78f)
                    : dimmed
                        ? new Color(0.08f, 0.10f, 0.09f, 0.18f)
                        : new Color(0.10f, 0.12f, 0.11f, 0.28f);
            }

            if (text != null)
            {
                text.color = active
                    ? new Color(0.92f, 1f, 0.88f, 1f)
                    : dimmed
                        ? new Color(0.62f, 0.68f, 0.60f, 0.62f)
                        : new Color(0.78f, 0.84f, 0.74f, 0.78f);
            }

            if (rect != null)
            {
                float scale = active ? 1.12f : dimmed ? 0.92f : 1f;
                rect.localScale = Vector3.one * scale;
            }
        }

        private MobileRotateZoneSide ResolveRotateZoneHighlightSide()
        {
            if (confirmedRotateZoneSide != MobileRotateZoneSide.None
                && Time.unscaledTime <= rotateZoneConfirmUntilTime)
            {
                return confirmedRotateZoneSide;
            }

            return MobileRotateZoneSide.None;
        }

        private MobileRotateZoneSide ResolveRotateZoneGuideSide()
        {
            MobileRotateZoneSide highlightSide = ResolveRotateZoneHighlightSide();
            if (highlightSide != MobileRotateZoneSide.None)
            {
                return highlightSide;
            }

            if (activeRotateSeekSide != MobileRotateZoneSide.None)
            {
                return activeRotateSeekSide;
            }

            return currentRotateZoneSide == MobileRotateZoneSide.Right
                ? currentRotateZoneSide
                : MobileRotateZoneSide.None;
        }

        private void UpdateRotateZoneGuideRect(
            PointerEventData eventData,
            Rect ghostRect,
            Rect rightRect,
            MobileRotateZoneSide side)
        {
            if (rotateZoneGuide == null
                || rotateZoneGuideImage == null
                || rotateZoneLayer == null
                || side == MobileRotateZoneSide.None)
            {
                if (rotateZoneGuide != null)
                {
                    rotateZoneGuide.gameObject.SetActive(false);
                }

                return;
            }

            Rect targetRect = rightRect;
            float y = Mathf.Lerp(ghostRect.center.y, targetRect.center.y, 0.5f);
            Vector2 startScreen;
            Vector2 endScreen;
            if (ghostRect.Contains(targetRect.center))
            {
                startScreen = ghostRect.center;
                endScreen = targetRect.center;
            }
            else
            {
                startScreen = new Vector2(ghostRect.xMax + 8f, y);
                endScreen = new Vector2(targetRect.xMin - 8f, y);
            }

            if (Vector2.Distance(startScreen, endScreen) < 8f)
            {
                startScreen = ghostRect.center;
                endScreen = targetRect.center;
            }

            Camera eventCamera = eventData?.pressEventCamera ?? eventData?.enterEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rotateZoneLayer,
                    startScreen,
                    eventCamera,
                    out Vector2 startLocal)
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rotateZoneLayer,
                    endScreen,
                    eventCamera,
                    out Vector2 endLocal))
            {
                rotateZoneGuide.gameObject.SetActive(false);
                return;
            }

            Vector2 delta = endLocal - startLocal;
            float distance = delta.magnitude;
            if (distance <= 1f)
            {
                rotateZoneGuide.gameObject.SetActive(false);
                return;
            }

            rotateZoneGuide.gameObject.SetActive(true);
            rotateZoneGuide.SetAsFirstSibling();
            rotateZoneGuide.anchorMin = new Vector2(0.5f, 0.5f);
            rotateZoneGuide.anchorMax = new Vector2(0.5f, 0.5f);
            rotateZoneGuide.pivot = new Vector2(0.5f, 0.5f);
            rotateZoneGuide.anchoredPosition = (startLocal + endLocal) * 0.5f;
            bool triggered = ResolveRotateZoneHighlightSide() == side;
            rotateZoneGuide.sizeDelta = new Vector2(distance, triggered ? 6f : 4f);
            rotateZoneGuide.localScale = Vector3.one;
            rotateZoneGuide.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            rotateZoneGuideImage.color = triggered
                ? new Color(0.42f, 0.88f, 0.66f, 0.40f)
                : new Color(0.42f, 0.88f, 0.66f, 0.22f);
        }

        private void EnsureRotateZoneViews()
        {
            RectTransform parent = battlePrepareMotionRoot != null
                ? battlePrepareMotionRoot
                : boardGridPreview == null ? null : boardGridPreview.parent as RectTransform;
            if (parent == null)
            {
                return;
            }

            if (rotateZoneLayer == null || rotateZoneLayer.parent != parent)
            {
                Transform existingLayer = parent.Find(RotateZoneLayerName);
                GameObject layerObject = existingLayer == null
                    ? new GameObject(RotateZoneLayerName, typeof(RectTransform))
                    : existingLayer.gameObject;
                layerObject.transform.SetParent(parent, false);
                layerObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                rotateZoneLayer = layerObject.GetComponent<RectTransform>();
                StretchToParent(rotateZoneLayer);
            }

            EnsureRotateZoneGuideView();
            EnsureRotateZoneView(
                RotateZoneLeftName,
                "L 90",
                out rotateZoneLeft,
                out rotateZoneLeftImage,
                out rotateZoneLeftText);
            EnsureRotateZoneView(
                RotateZoneRightName,
                "R 90",
                out rotateZoneRight,
                out rotateZoneRightImage,
                out rotateZoneRightText);
        }

        private void EnsureRotateZoneGuideView()
        {
            if (rotateZoneLayer == null)
            {
                return;
            }

            Transform existing = rotateZoneLayer.Find(RotateZoneGuideName);
            GameObject target = existing == null
                ? new GameObject(RotateZoneGuideName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                : existing.gameObject;
            target.transform.SetParent(rotateZoneLayer, false);
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            rotateZoneGuide = target.GetComponent<RectTransform>();
            rotateZoneGuideImage = target.GetComponent<Image>();
            if (rotateZoneGuideImage == null)
            {
                rotateZoneGuideImage = target.AddComponent<Image>();
            }

            rotateZoneGuideImage.raycastTarget = false;
            rotateZoneGuideImage.color = new Color(0.42f, 0.88f, 0.66f, 0.40f);
            rotateZoneGuide.gameObject.SetActive(false);
            rotateZoneGuide.SetAsFirstSibling();
        }

        private void EnsureRotateZoneView(
            string objectName,
            string label,
            out RectTransform rect,
            out Image image,
            out Text text)
        {
            rect = null;
            image = null;
            text = null;
            if (rotateZoneLayer == null)
            {
                return;
            }

            Transform existing = rotateZoneLayer.Find(objectName);
            GameObject target = existing == null
                ? new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                : existing.gameObject;
            target.transform.SetParent(rotateZoneLayer, false);
            target.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            rect = target.GetComponent<RectTransform>();
            image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            image.raycastTarget = false;
            Transform labelTransform = target.transform.Find("Label");
            GameObject labelObject = labelTransform == null
                ? new GameObject("Label", typeof(RectTransform), typeof(Text))
                : labelTransform.gameObject;
            labelObject.transform.SetParent(target.transform, false);
            labelObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            text = labelObject.GetComponent<Text>();
            if (text == null)
            {
                text = labelObject.AddComponent<Text>();
            }

            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            StretchToParent(text.rectTransform);
            SetRotateZoneHighlight(ResolveRotateZoneHighlightSide());
        }

        private void SetRotateZoneRect(
            RectTransform zoneRect,
            Rect screenRect,
            PointerEventData eventData)
        {
            if (zoneRect == null || rotateZoneLayer == null)
            {
                return;
            }

            Camera eventCamera = eventData?.pressEventCamera ?? eventData?.enterEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rotateZoneLayer,
                    screenRect.center,
                    eventCamera,
                    out Vector2 localPoint))
            {
                return;
            }

            zoneRect.anchorMin = new Vector2(0.5f, 0.5f);
            zoneRect.anchorMax = new Vector2(0.5f, 0.5f);
            zoneRect.pivot = new Vector2(0.5f, 0.5f);
            zoneRect.anchoredPosition = localPoint;
            zoneRect.sizeDelta = screenRect.size;
            zoneRect.localScale = Vector3.one;
        }

        private static void StretchToParent(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
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
            if (!TrySnapDragGhostToBoardPreview(result, source, eventData))
            {
                dragGhostRoot.position = eventData.position
                    + new Vector2(0f, MobileShapePlacementInputSettings.DefaultFingerGhostOffsetPixels);
            }
            if (dragGhostText != null)
            {
                dragGhostText.text = $"{item.DisplayName}\n{state}";
                dragGhostText.transform.SetAsLastSibling();
            }

            if (TryResolveDragGhostLayout(item, result, source, out ShapeCellVisualLayout layout))
            {
                ApplyDragGhostLayout(layout, item, result, source);
            }
            else
            {
                RestoreDragGhostBoxVisual();
            }
        }

        private void RefreshDragGhostLayoutAtCurrentPosition(
            ShapePlacementResult result,
            ShapePlacementSource source)
        {
            if (dragGhostRoot == null || selectedItem == null)
            {
                return;
            }

            if (dragGhostText != null)
            {
                dragGhostText.text = $"{selectedItem.DisplayName}\n拖动中";
                dragGhostText.transform.SetAsLastSibling();
            }

            if (TryResolveDragGhostLayout(selectedItem, result, source, out ShapeCellVisualLayout layout))
            {
                ApplyDragGhostLayout(layout, selectedItem, result, source);
            }
            else
            {
                RestoreDragGhostBoxVisual();
            }
        }

        private bool TrySnapDragGhostToBoardPreview(
            ShapePlacementResult result,
            ShapePlacementSource source,
            PointerEventData eventData)
        {
            if (dragGhostRoot == null
                || source != ShapePlacementSource.Board
                || result == null
                || result.OccupiedCells.Count == 0
                || !TryBuildBoardCellsScreenBounds(
                    result.OccupiedCells,
                    eventData?.pressEventCamera ?? eventData?.enterEventCamera,
                    out Rect bounds))
            {
                return false;
            }

            dragGhostRoot.position = bounds.center;
            return true;
        }

        private bool TryBuildBoardCellsScreenBounds(
            IReadOnlyList<ItemShapeCell> cells,
            Camera eventCamera,
            out Rect bounds)
        {
            bounds = default;
            if (cells == null || cells.Count == 0)
            {
                return false;
            }

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            Vector3[] corners = new Vector3[4];
            foreach (ItemShapeCell cell in cells.Distinct())
            {
                RectTransform rect = ResolveBoardSlotRect(cell);
                if (rect == null)
                {
                    return false;
                }

                rect.GetWorldCorners(corners);
                for (int i = 0; i < corners.Length; i++)
                {
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[i]);
                    minX = Mathf.Min(minX, point.x);
                    maxX = Mathf.Max(maxX, point.x);
                    minY = Mathf.Min(minY, point.y);
                    maxY = Mathf.Max(maxY, point.y);
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

        private void HideDragGhost()
        {
            SetRotateZonesVisible(false);
            ResetRotateZoneEntry();
            if (dragGhostRoot != null)
            {
                dragGhostRoot.gameObject.SetActive(false);
            }

            if (dragGhostCellLayer != null)
            {
                dragGhostCellLayer.gameObject.SetActive(false);
            }

            HideDragGhostArtwork();
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

        public bool TryResolveBoardItemFeedbackAnchor(
            string itemId,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            out RectTransform itemArtworkRect,
            out RectTransform feedbackLayer,
            out Vector2 anchoredPosition,
            out Vector2 sizeDelta)
        {
            itemArtworkRect = null;
            feedbackLayer = null;
            anchoredPosition = Vector2.zero;
            sizeDelta = Vector2.zero;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return false;
            }

            if (boardPlacedArtworkByItemId.TryGetValue(itemId, out RectTransform artworkRect)
                && artworkRect != null
                && artworkRect.gameObject.activeInHierarchy)
            {
                itemArtworkRect = artworkRect;
                feedbackLayer = artworkRect.parent as RectTransform;
                anchoredPosition = artworkRect.anchoredPosition;
                sizeDelta = artworkRect.rect.size.sqrMagnitude > 0.01f
                    ? artworkRect.rect.size
                    : artworkRect.sizeDelta;
                return feedbackLayer != null;
            }

            List<ItemShapeCell> cells = occupiedCells == null
                ? new List<ItemShapeCell>()
                : occupiedCells
                    .Where(cell => boardSlotByCell.ContainsKey(cell))
                    .Select(cell => new ItemShapeCell(cell.x, cell.y))
                    .Distinct()
                    .ToList();
            if (cells.Count == 0 && boardReceiver != null)
            {
                cells = boardReceiver.OccupiedCells
                    .Where(pair => string.Equals(pair.Value, itemId, StringComparison.Ordinal))
                    .Select(pair => new ItemShapeCell(pair.Key.x, pair.Key.y))
                    .Distinct()
                    .ToList();
            }

            if (cells.Count == 0 || !TryBuildBoardCellVisualLayout(cells, out ShapeCellVisualLayout layout))
            {
                return false;
            }

            feedbackLayer = EnsureBoardArtworkLayer();
            if (feedbackLayer == null)
            {
                return false;
            }

            anchoredPosition = layout.AnchoredPosition + new Vector2(layout.SizeDelta.x * 0.5f, -layout.SizeDelta.y * 0.5f);
            sizeDelta = layout.SizeDelta;
            return true;
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
            ShapePlacementResult result,
            ShapePlacementSource source)
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

            ShapeCellVisualStyle wholeItemStyle = ResolveWholeItemVisualStyle(item?.ItemId);
            RectTransform layer = EnsureDragGhostCellLayer();
            if (layer == null)
            {
                return;
            }

            Color color = ResolveDragGhostCellColor(item, result);
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
                    if (wholeItemStyle != null)
                    {
                        ApplyDragGhostCellUnderlay(image, item, result, source, i, color);
                        continue;
                    }

                    ShapeCellVisualStyle visualStyle = result != null && !result.IsValid
                        ? null
                        : ResolveItemVisualStyle(item?.ItemId, i);
                    if (visualStyle != null)
                    {
                        visualStyle.ApplyTo(image, color, 0.82f);
                    }
                    else
                    {
                        image.overrideSprite = null;
                        image.sprite = null;
                        image.type = Image.Type.Simple;
                        image.preserveAspect = false;
                        image.fillCenter = true;
                        image.material = null;
                        image.pixelsPerUnitMultiplier = 1f;
                        image.color = color;
                    }

                    image.raycastTarget = false;
                }
            }

            if (wholeItemStyle != null)
            {
                ApplyDragGhostArtwork(
                    layout,
                    wholeItemStyle,
                    color,
                    item == null ? ItemShapeRotation.Rotation0 : item.Rotation,
                    ResolveWholeItemArtworkDragGhostRotationOffsetDegrees(item),
                    result != null && !result.IsValid);
            }
            else
            {
                HideDragGhostArtwork();
            }

            if (dragGhostText != null)
            {
                dragGhostText.transform.SetAsLastSibling();
            }
        }

        private void ApplyDragGhostCellUnderlay(
            Image image,
            PreviewItem item,
            ShapePlacementResult result,
            ShapePlacementSource source,
            int visualIndex,
            Color fallbackColor)
        {
            if (image == null)
            {
                return;
            }

            if (TryResolveDragGhostCellUnderlayStyle(item, result, source, visualIndex, out ShapeCellVisualStyle style)
                && style != null)
            {
                if (style.HasRectTransformOverride && image.transform is RectTransform cellRect)
                {
                    Image childImage = EnsureDragGhostCellUnderlayImage(cellRect);
                    HideDragGhostCellParentImage(image);
                    style.ApplyTo(childImage, fallbackColor);
                    ApplyDragGhostCellUnderlayTransform(childImage.rectTransform, style);
                }
                else
                {
                    HideDragGhostCellUnderlayImage(image.transform);
                    style.ApplyTo(image, fallbackColor);
                }
            }
            else
            {
                HideDragGhostCellUnderlayImage(image.transform);
                image.overrideSprite = null;
                image.sprite = null;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.fillCenter = true;
                image.material = null;
                image.pixelsPerUnitMultiplier = 1f;
                image.color = new Color(
                    fallbackColor.r,
                    fallbackColor.g,
                    fallbackColor.b,
                    Mathf.Min(fallbackColor.a, 0.42f));
            }

            image.raycastTarget = false;
        }

        private static Image EnsureDragGhostCellUnderlayImage(RectTransform cellRect)
        {
            Transform existing = cellRect == null ? null : cellRect.Find(DragGhostCellUnderlayName);
            GameObject underlayObject = existing == null
                ? new GameObject(DragGhostCellUnderlayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
                : existing.gameObject;
            underlayObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            underlayObject.transform.SetParent(cellRect, false);
            Image image = underlayObject.GetComponent<Image>();
            image.raycastTarget = false;
            underlayObject.SetActive(true);
            return image;
        }

        private static void ApplyDragGhostCellUnderlayTransform(
            RectTransform rect,
            ShapeCellVisualStyle style)
        {
            if (rect == null || style == null || !style.HasRectTransformOverride)
            {
                return;
            }

            rect.anchorMin = style.AnchorMin;
            rect.anchorMax = style.AnchorMax;
            rect.pivot = style.Pivot;
            rect.anchoredPosition = style.AnchoredPosition;
            rect.sizeDelta = style.SizeDelta;
            rect.localScale = style.LocalScale;
            rect.localEulerAngles = style.LocalEulerAngles;
        }

        private static void HideDragGhostCellParentImage(Image image)
        {
            if (image == null)
            {
                return;
            }

            image.overrideSprite = null;
            image.sprite = null;
            Color color = image.color;
            color.a = 0f;
            image.color = color;
            image.raycastTarget = false;
        }

        private static void HideDragGhostCellUnderlayImage(Transform cellTransform)
        {
            Transform existing = cellTransform == null ? null : cellTransform.Find(DragGhostCellUnderlayName);
            if (existing != null)
            {
                existing.gameObject.SetActive(false);
            }
        }

        private bool TryResolveDragGhostCellUnderlayStyle(
            PreviewItem item,
            ShapePlacementResult result,
            ShapePlacementSource source,
            int visualIndex,
            out ShapeCellVisualStyle style)
        {
            style = null;
            if (source == ShapePlacementSource.Board
                && TryCaptureBoardCellUnderlayStyle(result, visualIndex, out style))
            {
                return true;
            }

            if (source == ShapePlacementSource.Tray
                && itemTrayView != null
                && itemTrayView.TryCaptureTraySlotUnderlayStyle(item?.ItemId, visualIndex, out style))
            {
                return true;
            }

            if (TryCaptureBoardCellUnderlayStyle(result, visualIndex, out style))
            {
                return true;
            }

            if (itemTrayView != null
                && itemTrayView.TryCaptureTraySlotUnderlayStyle(item?.ItemId, visualIndex, out style))
            {
                return true;
            }

            return TryCaptureFirstBoardCellUnderlayStyle(out style)
                || (itemTrayView != null && itemTrayView.TryCaptureFirstTraySlotUnderlayStyle(out style));
        }

        private bool TryCaptureBoardCellUnderlayStyle(
            ShapePlacementResult result,
            int visualIndex,
            out ShapeCellVisualStyle style)
        {
            style = null;
            if (result == null
                || visualIndex < 0
                || visualIndex >= result.OccupiedCells.Count)
            {
                return false;
            }

            return boardSlotByCell.TryGetValue(result.OccupiedCells[visualIndex], out BuildGridPreviewSlotView slot)
                && slot != null
                && slot.TryCaptureCellUnderlayStyle(out style);
        }

        private bool TryCaptureFirstBoardCellUnderlayStyle(out ShapeCellVisualStyle style)
        {
            foreach (BuildGridPreviewSlotView slot in boardSlots ?? Array.Empty<BuildGridPreviewSlotView>())
            {
                if (slot != null && slot.TryCaptureCellUnderlayStyle(out style))
                {
                    return true;
                }
            }

            style = null;
            return false;
        }

        private void CacheAllItemVisualStyles()
        {
            foreach (PreviewItem item in itemById.Values)
            {
                CacheItemVisualStyles(item);
            }
        }

        private void CacheItemVisualStyles(PreviewItem item)
        {
            if (item != null)
            {
                CacheItemVisualStyles(item.ItemId);
            }
        }

        private void CacheItemVisualStyles(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId) || itemTrayView == null)
            {
                return;
            }

            PreviewItem item = ResolvePreviewItem(itemId);
            itemTrayView.ApplyItemArtworkRotationOffset(
                itemId,
                ResolveWholeItemArtworkTrayRotationOffsetDegrees(item));

            List<ShapeCellVisualStyle> styles = new();
            if (!itemTrayView.TryCaptureItemVisualStyles(itemId, styles) || styles.Count == 0)
            {
                visualStylesByItemId.Remove(itemId);
                return;
            }

            visualStylesByItemId[itemId] = styles;
        }

        private ShapeCellVisualStyle ResolveItemVisualStyle(string itemId, int cellIndex)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !visualStylesByItemId.TryGetValue(itemId, out List<ShapeCellVisualStyle> styles)
                || styles == null
                || styles.Count == 0)
            {
                return null;
            }

            List<ShapeCellVisualStyle> cellStyles = styles
                .Where(style => style != null && !style.SpansWholeItem)
                .ToList();
            if (cellStyles.Count == 0)
            {
                return null;
            }

            if (cellStyles.Count == 1)
            {
                return cellStyles[0];
            }

            int safeIndex = Mathf.Abs(cellIndex) % cellStyles.Count;
            return cellStyles[safeIndex];
        }

        private ShapeCellVisualStyle ResolveWholeItemVisualStyle(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId)
                || !visualStylesByItemId.TryGetValue(itemId, out List<ShapeCellVisualStyle> styles)
                || styles == null)
            {
                return null;
            }

            return styles.FirstOrDefault(style =>
                style != null
                && style.SpansWholeItem
                && style.Sprite != null);
        }

        private ItemShapeRotation ResolveItemRotation(string itemId)
        {
            if (!string.IsNullOrWhiteSpace(itemId)
                && selectedItem != null
                && string.Equals(selectedItem.ItemId, itemId, StringComparison.Ordinal))
            {
                return selectedItem.Rotation;
            }

            return !string.IsNullOrWhiteSpace(itemId)
                && itemById.TryGetValue(itemId, out PreviewItem item)
                && item != null
                ? item.Rotation
                : ItemShapeRotation.Rotation0;
        }

        private PreviewItem ResolvePreviewItem(string itemId)
        {
            return !string.IsNullOrWhiteSpace(itemId)
                && itemById.TryGetValue(itemId, out PreviewItem item)
                ? item
                : null;
        }

        private float ResolveWholeItemArtworkTrayRotationOffsetDegrees(PreviewItem item)
        {
            return IsThreeCellTriangleArtwork(item)
                ? triangleTrayArtworkRotationOffsetDegrees
                : defaultTrayArtworkRotationOffsetDegrees;
        }

        private float ResolveWholeItemArtworkDragGhostRotationOffsetDegrees(string itemId)
        {
            return ResolveWholeItemArtworkDragGhostRotationOffsetDegrees(ResolvePreviewItem(itemId));
        }

        private float ResolveWholeItemArtworkDragGhostRotationOffsetDegrees(PreviewItem item)
        {
            return IsThreeCellTriangleArtwork(item)
                ? triangleDragGhostArtworkRotationOffsetDegrees
                : defaultDragGhostArtworkRotationOffsetDegrees;
        }

        private float ResolveWholeItemArtworkBoardPlacedRotationOffsetDegrees(string itemId)
        {
            return ResolveWholeItemArtworkBoardPlacedRotationOffsetDegrees(ResolvePreviewItem(itemId));
        }

        private float ResolveWholeItemArtworkBoardPlacedRotationOffsetDegrees(PreviewItem item)
        {
            return IsThreeCellTriangleArtwork(item)
                ? triangleBoardPlacedArtworkRotationOffsetDegrees
                : defaultBoardPlacedArtworkRotationOffsetDegrees;
        }

        private bool IsThreeCellTriangleArtwork(PreviewItem item)
        {
            if (item == null)
            {
                return false;
            }

            bool hasTriangleHint = ContainsShapeHint(item.ShapeId, "Corner", "Triangle")
                || ContainsShapeHint(item.ShapeDisplayName, "三角", "拐角");
            if (!hasTriangleHint)
            {
                return false;
            }

            if (shapeById.TryGetValue(item.ShapeId ?? string.Empty, out ItemShapeConfig shapeConfig)
                && shapeConfig != null)
            {
                return shapeConfig.cellCount == 3;
            }

            return ContainsShapeHint(item.ShapeId, "3")
                || ContainsShapeHint(item.ShapeDisplayName, "三格", "3");
        }

        private static bool ContainsShapeHint(string value, params string[] hints)
        {
            if (string.IsNullOrWhiteSpace(value) || hints == null)
            {
                return false;
            }

            foreach (string hint in hints)
            {
                if (!string.IsNullOrWhiteSpace(hint)
                    && value.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
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

        private void ApplyDragGhostArtwork(
            ShapeCellVisualLayout layout,
            ShapeCellVisualStyle style,
            Color fallbackColor,
            ItemShapeRotation rotation,
            float stateRotationOffsetDegrees,
            bool invalid)
        {
            if (layout == null
                || style == null
                || !style.SpansWholeItem
                || style.Sprite == null
                || !EnsureDragGhostArtworkView())
            {
                HideDragGhostArtwork();
                return;
            }

            dragGhostArtwork.gameObject.SetActive(true);
            ApplyWholeItemArtworkTransform(
                dragGhostArtwork,
                Vector2.zero,
                layout.SizeDelta,
                rotation,
                style.SourceRotationDegrees + stateRotationOffsetDegrees);
            dragGhostArtwork.SetAsLastSibling();
            dragGhostArtworkImage.raycastTarget = false;
            ApplyArtworkStyle(dragGhostArtworkImage, style, fallbackColor, 0.88f, null);

            if (invalid)
            {
                ApplyDragGhostInvalidArtwork(layout, style, fallbackColor, rotation, stateRotationOffsetDegrees);
            }
            else
            {
                HideDragGhostInvalidArtwork();
            }
        }

        private void HideDragGhostArtwork()
        {
            if (dragGhostArtwork != null)
            {
                dragGhostArtwork.gameObject.SetActive(false);
            }

            HideDragGhostInvalidArtwork();
        }

        private void ApplyDragGhostInvalidArtwork(
            ShapeCellVisualLayout layout,
            ShapeCellVisualStyle style,
            Color fallbackColor,
            ItemShapeRotation rotation,
            float stateRotationOffsetDegrees)
        {
            if (layout == null
                || style == null
                || !style.SpansWholeItem
                || style.Sprite == null
                || !EnsureDragGhostInvalidArtworkView())
            {
                HideDragGhostInvalidArtwork();
                return;
            }

            dragGhostInvalidArtwork.gameObject.SetActive(true);
            ApplyWholeItemArtworkTransform(
                dragGhostInvalidArtwork,
                Vector2.zero,
                layout.SizeDelta,
                rotation,
                style.SourceRotationDegrees + stateRotationOffsetDegrees);
            dragGhostInvalidArtwork.SetAsLastSibling();
            dragGhostInvalidArtworkImage.raycastTarget = false;
            ApplyArtworkStyle(
                dragGhostInvalidArtworkImage,
                style,
                fallbackColor,
                DragGhostInvalidTint.a,
                DragGhostInvalidTint);
        }

        private void HideDragGhostInvalidArtwork()
        {
            if (dragGhostInvalidArtwork != null)
            {
                dragGhostInvalidArtwork.gameObject.SetActive(false);
            }
        }

        private bool EnsureDragGhostArtworkView()
        {
            if (dragGhostRoot == null)
            {
                return false;
            }

            if (dragGhostArtwork == null || dragGhostArtwork.parent != dragGhostRoot)
            {
                Transform existing = dragGhostRoot.Find(DragGhostArtworkName);
                GameObject artworkObject = existing == null
                    ? new GameObject(DragGhostArtworkName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement))
                    : existing.gameObject;
                artworkObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                artworkObject.transform.SetParent(dragGhostRoot, false);
                dragGhostArtwork = artworkObject.GetComponent<RectTransform>();
                LayoutElement layoutElement = artworkObject.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.ignoreLayout = true;
                }
            }

            dragGhostArtworkImage = dragGhostArtwork.GetComponent<Image>();
            if (dragGhostArtworkImage == null)
            {
                dragGhostArtworkImage = dragGhostArtwork.gameObject.AddComponent<Image>();
            }

            dragGhostArtworkImage.raycastTarget = false;
            return true;
        }

        private bool EnsureDragGhostInvalidArtworkView()
        {
            if (dragGhostRoot == null)
            {
                return false;
            }

            if (dragGhostInvalidArtwork == null || dragGhostInvalidArtwork.parent != dragGhostRoot)
            {
                Transform existing = dragGhostRoot.Find(DragGhostInvalidArtworkName);
                GameObject artworkObject = existing == null
                    ? new GameObject(DragGhostInvalidArtworkName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement))
                    : existing.gameObject;
                artworkObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                artworkObject.transform.SetParent(dragGhostRoot, false);
                dragGhostInvalidArtwork = artworkObject.GetComponent<RectTransform>();
                LayoutElement layoutElement = artworkObject.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.ignoreLayout = true;
                }
            }

            dragGhostInvalidArtworkImage = dragGhostInvalidArtwork.GetComponent<Image>();
            if (dragGhostInvalidArtworkImage == null)
            {
                dragGhostInvalidArtworkImage = dragGhostInvalidArtwork.gameObject.AddComponent<Image>();
            }

            dragGhostInvalidArtworkImage.raycastTarget = false;
            return true;
        }

        private static void ApplyWholeItemArtworkTransform(
            RectTransform artworkRect,
            Vector2 boundsTopLeft,
            Vector2 boundsSize,
            ItemShapeRotation rotation,
            float baseRotationDegrees)
        {
            if (artworkRect == null)
            {
                return;
            }

            Vector2 safeSize = new(Mathf.Max(1f, boundsSize.x), Mathf.Max(1f, boundsSize.y));
            Vector2 drawSize = IsQuarterTurn(rotation)
                ? new Vector2(safeSize.y, safeSize.x)
                : safeSize;

            artworkRect.anchorMin = new Vector2(0f, 1f);
            artworkRect.anchorMax = new Vector2(0f, 1f);
            artworkRect.pivot = new Vector2(0.5f, 0.5f);
            artworkRect.anchoredPosition = boundsTopLeft + new Vector2(safeSize.x * 0.5f, -safeSize.y * 0.5f);
            artworkRect.sizeDelta = drawSize;
            artworkRect.localScale = Vector3.one;
            artworkRect.localEulerAngles = new Vector3(0f, 0f, ResolveArtworkRotationDegrees(rotation, baseRotationDegrees));
        }

        private static bool IsQuarterTurn(ItemShapeRotation rotation)
        {
            return rotation == ItemShapeRotation.Rotation90
                || rotation == ItemShapeRotation.Rotation270;
        }

        private static float ResolveArtworkRotationDegrees(ItemShapeRotation rotation, float baseRotationDegrees)
        {
            float rotationDegrees = rotation switch
            {
                ItemShapeRotation.Rotation90 => -90f,
                ItemShapeRotation.Rotation180 => -180f,
                ItemShapeRotation.Rotation270 => -270f,
                _ => 0f
            };
            return baseRotationDegrees + rotationDegrees;
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

            HideDragGhostArtwork();
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
                ItemShapeRotation.Rotation90 => new ItemShapeCell(-offset.y, offset.x),
                ItemShapeRotation.Rotation180 => new ItemShapeCell(-offset.x, -offset.y),
                ItemShapeRotation.Rotation270 => new ItemShapeCell(offset.y, -offset.x),
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

        private sealed class FormationPowerCellOverlay
        {
            private readonly Image rangeImage;
            private readonly Text coreText;
            private readonly Text badgeText;

            private FormationPowerCellOverlay(Image rangeImage, Text coreText, Text badgeText)
            {
                this.rangeImage = rangeImage;
                this.coreText = coreText;
                this.badgeText = badgeText;
            }

            public static FormationPowerCellOverlay Create(RectTransform parent)
            {
                GameObject rootObject = new(FormationPowerOverlayName, typeof(RectTransform), typeof(CanvasGroup));
                rootObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                rootObject.transform.SetParent(parent, false);
                RectTransform root = rootObject.GetComponent<RectTransform>();
                SetRuntimeAnchors(root, Vector2.zero, Vector2.one);
                root.SetAsLastSibling();
                CanvasGroup raycastGuard = rootObject.GetComponent<CanvasGroup>();
                raycastGuard.interactable = false;
                raycastGuard.blocksRaycasts = false;

                GameObject rangeObject = new("Range", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                rangeObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                rangeObject.transform.SetParent(root, false);
                RectTransform rangeRect = rangeObject.GetComponent<RectTransform>();
                SetRuntimeAnchors(rangeRect, Vector2.zero, Vector2.one);
                Image image = rangeObject.GetComponent<Image>();
                image.raycastTarget = false;

                Text core = CreateRuntimeText(
                    "CoreLabel",
                    root,
                    string.Empty,
                    13,
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter);
                SetRuntimeAnchors(core.rectTransform, new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.30f));
                core.color = new Color(1f, 0.84f, 0.32f, 1f);

                Text badge = CreateRuntimeText(
                    "PowerBadge",
                    root,
                    string.Empty,
                    11,
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter);
                badge.resizeTextForBestFit = true;
                badge.resizeTextMinSize = 8;
                badge.resizeTextMaxSize = 12;
                badge.horizontalOverflow = HorizontalWrapMode.Wrap;
                badge.verticalOverflow = VerticalWrapMode.Truncate;
                SetRuntimeAnchors(badge.rectTransform, new Vector2(0.04f, 0.58f), new Vector2(0.96f, 0.98f));

                return new FormationPowerCellOverlay(image, core, badge);
            }

            public void Clear()
            {
                if (rangeImage != null)
                {
                    rangeImage.color = Color.clear;
                }

                if (coreText != null)
                {
                    coreText.text = string.Empty;
                    coreText.color = new Color(1f, 0.84f, 0.32f, 1f);
                }

                if (badgeText != null)
                {
                    badgeText.text = string.Empty;
                }
            }

            public void ShowCore()
            {
                if (coreText != null)
                {
                    coreText.text = "\u9635\u773c";
                }

                if (rangeImage != null && rangeImage.color.a <= 0.01f)
                {
                    rangeImage.color = new Color(1f, 0.74f, 0.22f, 0.18f);
                }
            }

            public void ShowWeakPulseRange()
            {
                if (rangeImage != null)
                {
                    rangeImage.color = new Color(0.50f, 0.62f, 1f, 0.18f);
                }
            }

            public void ShowPoweredRange()
            {
                if (rangeImage != null)
                {
                    rangeImage.color = new Color(1f, 0.72f, 0.22f, 0.24f);
                }
            }

            public void ShowEyeCellOccupied()
            {
                if (coreText != null)
                {
                    coreText.text = "\u9635\u773c\u88ab\u5360";
                    coreText.color = new Color(1f, 0.42f, 0.36f, 1f);
                }

                if (rangeImage != null)
                {
                    rangeImage.color = new Color(0.72f, 0.12f, 0.10f, 0.28f);
                }
            }

            public void ShowItemState(string badge, Color color)
            {
                if (badgeText == null)
                {
                    return;
                }

                badgeText.text = badge ?? string.Empty;
                badgeText.color = color;
            }
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
                IReadOnlyList<string> categoryIds = null,
                string itemInstanceId = null)
            {
                ItemId = itemId ?? string.Empty;
                ItemInstanceId = string.IsNullOrWhiteSpace(itemInstanceId)
                    ? ItemId
                    : itemInstanceId;
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
            public string ItemInstanceId { get; }
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
            private readonly Func<ItemShapeCell, ItemShapeCell> visualToDataCell;
            private readonly bool visualRowsAreTopDown;

            public UiBoardShapeGridReceiver(
                string receiverId,
                RectTransform boardRect,
                int width,
                int height,
                IReadOnlyDictionary<ItemShapeCell, BuildGridPreviewSlotView> slotsByCell = null,
                Func<ItemShapeCell, ItemShapeCell> visualToDataCell = null,
                bool visualRowsAreTopDown = true)
            {
                ReceiverId = string.IsNullOrWhiteSpace(receiverId) ? "battle_sandbox_board" : receiverId;
                this.boardRect = boardRect;
                Width = width;
                Height = height;
                this.visualToDataCell = visualToDataCell ?? (cell => cell);
                this.visualRowsAreTopDown = visualRowsAreTopDown;
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

                return TryResolveCellFromBounds(rect, localPoint, out anchorCell);
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

                return TryResolveCellFromBounds(bounds, localPoint, out anchorCell);
            }

            private bool TryResolveCellFromBounds(
                Rect bounds,
                Vector2 localPoint,
                out ItemShapeCell anchorCell)
            {
                anchorCell = default;
                if (bounds.width <= 0f || bounds.height <= 0f || Width <= 0 || Height <= 0)
                {
                    return false;
                }

                float cellWidth = bounds.width / Mathf.Max(1, Width);
                float cellHeight = bounds.height / Mathf.Max(1, Height);
                float paddedMinX = bounds.xMin - cellWidth * BoardSoftBoundaryCellPadding;
                float paddedMaxX = bounds.xMax + cellWidth * BoardSoftBoundaryCellPadding;
                float paddedMinY = bounds.yMin - cellHeight * BoardSoftBoundaryCellPadding;
                float paddedMaxY = bounds.yMax + cellHeight * BoardSoftBoundaryCellPadding;
                if (localPoint.x < paddedMinX
                    || localPoint.x > paddedMaxX
                    || localPoint.y < paddedMinY
                    || localPoint.y > paddedMaxY)
                {
                    return false;
                }

                float normalizedX = Mathf.Clamp01((localPoint.x - bounds.xMin) / bounds.width);
                float normalizedY = Mathf.Clamp01((localPoint.y - bounds.yMin) / bounds.height);
                int x = Mathf.Clamp(Mathf.FloorToInt(normalizedX * Width), 0, Width - 1);
                int visualY = visualRowsAreTopDown
                    ? Mathf.FloorToInt((1f - normalizedY) * Height)
                    : Mathf.FloorToInt(normalizedY * Height);
                visualY = Mathf.Clamp(visualY, 0, Height - 1);
                anchorCell = visualToDataCell(new ItemShapeCell(x, visualY));
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

            public void ReplaceFromAcceptedSnapshot(
                IReadOnlyList<ItemSystemPlacementSnapshot> placements)
            {
                occupiedByItemId.Clear();
                foreach (ItemSystemPlacementSnapshot placement in placements
                             ?? Array.Empty<ItemSystemPlacementSnapshot>())
                {
                    if (placement == null
                        || string.IsNullOrWhiteSpace(placement.itemId))
                    {
                        continue;
                    }
                    foreach (Vector2Int cell in placement.OccupiedCells)
                    {
                        occupiedByItemId[new ItemShapeCell(cell.x, cell.y)] =
                            placement.itemId;
                    }
                }
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
