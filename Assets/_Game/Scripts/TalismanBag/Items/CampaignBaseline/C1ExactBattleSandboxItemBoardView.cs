using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Canonical;
using TalismanBag.Presentation.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TalismanBag.Items.CampaignBaseline
{
    [DisallowMultipleComponent]
    public sealed class C1ExactBattleSandboxItemBoardView : MonoBehaviour,
        IPointerDownHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IC1FormalItemArtworkResolver
    {
        public const string MissingReferenceDiagnostic =
            "C1_EXACT_BATTLESANDBOX_BOARD_AUTHORED_REFERENCE_MISSING";
        public const int PhysicalPresentationCapacity = 25;

        [Obsolete("Downstream serialized-reference compatibility only; never use for Item admission.")]
        public static int AuthoredPresentationCapacity => 7;

        private const int InitialPrewarmCount = 7;
        private const float RotateButtonVisualSizePixels = 72f;
        private const float RotateButtonInsideEdgeInsetPixels = 6f;

        [SerializeField] private RectTransform boardRect;
        [SerializeField] private RectTransform gridRoot;
        [SerializeField] private Image[] cells = Array.Empty<Image>();
        [SerializeField] private RectTransform boardItemArtworkLayer;
        [SerializeField] private RectTransform[] placedArtworkRects =
            Array.Empty<RectTransform>();
        [SerializeField] private Image[] placedArtworkImages = Array.Empty<Image>();
        [SerializeField] private ItemRarityContourBloomVfx[]
            placedArtworkRarityCarriers =
                Array.Empty<ItemRarityContourBloomVfx>();
        [SerializeField] private RectTransform selectionArtworkRect;
        [SerializeField] private Image selectionArtworkImage;
        [SerializeField] private RectTransform previewArtworkRect;
        [SerializeField] private Image previewArtworkImage;
        [SerializeField] private RectTransform previewShadowArtworkRect;
        [SerializeField] private Image previewShadowArtworkImage;
        [SerializeField] private RectTransform invalidArtworkRect;
        [SerializeField] private Image invalidArtworkImage;
        [SerializeField] private RectTransform dragGhostRoot;
        [SerializeField] private RectTransform dragGhostArtworkRect;
        [SerializeField] private Image dragGhostArtworkImage;
        [SerializeField] private RectTransform dragGhostInvalidArtworkRect;
        [SerializeField] private Image dragGhostInvalidArtworkImage;
        [SerializeField] private Text dragGhostLabel;
        [SerializeField] private RectTransform rotateZoneLayer;
        [SerializeField] private RectTransform rightRotateZone;
        [SerializeField] private Image rightRotateZoneImage;
        [SerializeField] private Text rightRotateZoneLabel;
        [SerializeField] private RectTransform rotateGuide;
        [SerializeField] private Image rotateGuideImage;
        [SerializeField] private Image[] formationRangeImages = Array.Empty<Image>();
        [SerializeField] private Text[] formationCoreLabels = Array.Empty<Text>();
        [SerializeField] private Text[] lightingStateLabels = Array.Empty<Text>();
        [SerializeField] private C1FormalItemPresentationCatalog
            presentationCatalog;
        [SerializeField, HideInInspector, FormerlySerializedAs("i001WhiteArtwork")]
        private Sprite catalogMigrationStarterArtwork;
        [SerializeField, HideInInspector, FormerlySerializedAs("i002WhiteArtwork")]
        private Sprite catalogMigrationPoolArtworkSample;
        [SerializeField, HideInInspector, FormerlySerializedAs("i031UnlitArtwork")]
        private Sprite catalogMigrationSpecialUnlitArtwork;
        [SerializeField, HideInInspector, FormerlySerializedAs("i031LitArtwork")]
        private Sprite catalogMigrationSpecialLitArtwork;

        private C1ExactBattleSandboxItemArrangementPresenter presenter;
        private CanonicalItemDefinitionResolver canonicalItemResolver;
        private C1FormalItemSessionSnapshot snapshot;
        private string selectedItemInstanceId = string.Empty;
        private string pressedItemInstanceId = string.Empty;
        private Vector2Int pressedRawGrabbedCell;
        private bool hasPressedRawGrabbedCell;
        private string activeBoardDragItemInstanceId = string.Empty;
        private Vector2Int activeBoardDragRawGrabbedCell;
        private bool hasActiveBoardDragRawGrabbedCell;
        private string boundSnapshotSignature = string.Empty;
        private bool invalidFormalRotationLogged;
        private readonly Vector3[] dragGhostWorldCorners = new Vector3[4];
        private readonly List<BoardPoolEntry> presentationPool = new();
        private readonly Dictionary<string, BoardPoolEntry> activePoolEntries =
            new(StringComparer.Ordinal);
        private bool presentationPoolInitialized;

        private sealed class BoardPoolEntry
        {
            public BoardPoolEntry(
                RectTransform rect,
                Image image,
                ItemRarityContourBloomVfx rarityCarrier,
                int[] carrierSiblingPath)
            {
                Rect = rect;
                Image = image;
                RarityCarrier = rarityCarrier;
                CarrierSiblingPath = carrierSiblingPath
                                     ?? Array.Empty<int>();
            }

            public RectTransform Rect { get; }
            public Image Image { get; }
            public ItemRarityContourBloomVfx RarityCarrier { get; }
            public IReadOnlyList<int> CarrierSiblingPath { get; }
            public string ItemInstanceId { get; set; } = string.Empty;
        }

        public string BoundSnapshotSignature => boundSnapshotSignature;
        public int BoardCellCount => cells?.Length ?? 0;
        public int AuthoredArtworkCapacity => placedArtworkRects?.Length ?? 0;
        public int AuthoredRarityCarrierCapacity =>
            placedArtworkRarityCarriers?.Length ?? 0;
        public int PresentationPoolCount => presentationPoolInitialized
            ? presentationPool.Count
            : AuthoredArtworkCapacity;
        public int VisiblePlacedArtworkCount => activePoolEntries.Values.Count(
            entry => entry.Image != null && entry.Image.gameObject.activeSelf);
        public int ActiveRarityCarrierCount => activePoolEntries.Values.Count(
            entry => entry.RarityCarrier != null
                     && entry.RarityCarrier.HasAppliedPresentation);
        public IReadOnlyList<string> BoundItemInstanceIds => activePoolEntries
            .Keys.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        public Sprite CatalogMigrationStarterArtwork =>
            catalogMigrationStarterArtwork;
        public Sprite CatalogMigrationPoolArtworkSample =>
            catalogMigrationPoolArtworkSample;
        public Sprite CatalogMigrationSpecialUnlitArtwork =>
            catalogMigrationSpecialUnlitArtwork;
        public Sprite CatalogMigrationSpecialLitArtwork =>
            catalogMigrationSpecialLitArtwork;
        public C1FormalItemPresentationCatalogSnapshot PresentationCatalogSnapshot
        {
            get
            {
                return TryResolvePresentationCatalog(
                    out C1FormalItemPresentationCatalogSnapshot snapshot,
                    out _)
                    ? snapshot
                    : null;
            }
        }

        public bool TryResolvePresentationCatalog(
            out C1FormalItemPresentationCatalogSnapshot resolved,
            out string diagnostic)
        {
            resolved = null;
            if (presentationCatalog == null)
            {
                diagnostic = "CATALOG_REFERENCE_MISSING";
                return false;
            }

            C1FormalItemPresentationCatalogResult result =
                canonicalItemResolver == null
                    ? presentationCatalog.Resolve()
                    : presentationCatalog.Resolve(canonicalItemResolver);
            if (result == null)
            {
                diagnostic = "CATALOG_RESULT_MISSING";
                return false;
            }

            diagnostic = result.diagnosticCode;
            if (!result.accepted || result.snapshot == null)
            {
                return false;
            }

            resolved = result.snapshot;
            return true;
        }

        public bool TryResolveFormalItemArtwork(
            string authoritativeBaseItemId,
            C1FormalItemArtworkLightingState lightingState,
            out Sprite artwork,
            out string artworkIdentity)
        {
            artwork = null;
            artworkIdentity = string.Empty;
            C1FormalItemPresentationCatalogSnapshot catalog =
                PresentationCatalogSnapshot;
            if (catalog == null) return false;
            C1FormalItemRosterEntrySnapshot selectedRoster = snapshot
                ?.FindRosterEntry(selectedItemInstanceId);
            if (selectedRoster != null
                && string.Equals(
                    selectedRoster.baseItemId,
                    authoritativeBaseItemId,
                    StringComparison.Ordinal)
                && selectedRoster.ordinaryInstance != null
                && CanonicalItemArtworkResolver.TryResolve(
                    selectedRoster.ordinaryInstance.canonicalDefinition,
                    selectedRoster.ordinaryInstance.rarity,
                    out artwork,
                    out _))
            {
                artworkIdentity = C1FormalItemArtworkIdentity.Create(
                    authoritativeBaseItemId,
                    lightingState);
                return true;
            }
            bool resolved = selectedRoster != null
                            && string.Equals(
                                selectedRoster.baseItemId,
                                authoritativeBaseItemId,
                                StringComparison.Ordinal)
                ? catalog.TryResolveArtwork(
                    authoritativeBaseItemId,
                    selectedRoster.rarityKey,
                    lightingState,
                    out _,
                    out artwork,
                    out _)
                : catalog.TryResolveUniqueBase(
                    authoritativeBaseItemId,
                    lightingState,
                    out _,
                    out artwork,
                    out _);
            if (!resolved || artwork == null) return false;
            artworkIdentity = C1FormalItemArtworkIdentity.Create(
                authoritativeBaseItemId,
                lightingState);
            return true;
        }

        internal bool TryResolveExactPlacedLitSourceAnchor(
            C1FormalItemSessionSnapshot expectedSnapshot,
            C1Pool15FormalItemSourceAnchorRequest request,
            out C1Pool15FormalItemSourceAnchorBinding binding)
        {
            binding = null;
            if (expectedSnapshot == null
                || request == null
                || snapshot == null
                || !ReferenceEquals(snapshot, expectedSnapshot)
                || !request.MatchesSnapshot(expectedSnapshot)
                || !string.Equals(
                    boundSnapshotSignature,
                    request.expectedSessionCanonicalSignature,
                    StringComparison.Ordinal))
                return false;

            C1FormalItemRosterEntrySnapshot[] rosters = snapshot.roster
                .Where(value => value != null && string.Equals(
                    value.itemInstanceId,
                    request.itemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            C1FormalItemPlacementSnapshot[] placements = snapshot.placements
                .Where(value => value != null && string.Equals(
                    value.itemInstanceId,
                    request.itemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            C1FormalItemTrayPlacementSnapshot[] trayPlacements =
                snapshot.trayLayout.placements
                    .Where(value => value != null && string.Equals(
                        value.itemInstanceId,
                        request.itemInstanceId,
                        StringComparison.Ordinal))
                    .ToArray();
            if (rosters.Length != 1 || placements.Length != 1) return false;

            C1FormalItemRosterEntrySnapshot roster = rosters[0];
            C1FormalItemPlacementSnapshot placement = placements[0];
            ItemSystemPlacementSnapshot[] itemSystemPlacements =
                snapshot.itemSystemSnapshot.placements
                    .Where(value => value != null && string.Equals(
                        value.placementId,
                        placement.placementId,
                        StringComparison.Ordinal))
                    .ToArray();
            BoardPoolEntry[] liveAnchors = activePoolEntries.Values
                .Where(value => value != null && string.Equals(
                    value.ItemInstanceId,
                    request.itemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            int activeTrayPlacementCount = trayPlacements.Count(
                value => value.isActiveInTray);
            C1FormalItemPresentationCatalogSnapshot catalog =
                PresentationCatalogSnapshot;
            C1FormalItemPresentationRowSnapshot catalogRow = null;
            Sprite artwork = null;
            string artworkIdentity = string.Empty;
            bool exactCatalogOrdinaryIdentity = catalog != null
                && catalog.TryResolveArtwork(
                    roster.baseItemId,
                    roster.rarityKey,
                    C1FormalItemArtworkLightingState.Lit,
                    out catalogRow,
                    out artwork,
                    out artworkIdentity)
                && !roster.isSpecialLightingSource
                && !catalogRow.isSpecialLightingSource;
            bool exactIdentityAligned = itemSystemPlacements.Length == 1
                && string.Equals(
                    placement.baseItemId,
                    roster.baseItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    itemSystemPlacements[0].itemId,
                    roster.baseItemId,
                    StringComparison.Ordinal)
                && placement.anchorCell == itemSystemPlacements[0].anchorCell
                && placement.rotation == itemSystemPlacements[0].rotation;
            bool isLit = itemSystemPlacements.Length == 1
                         && itemSystemPlacements[0].isLit;
            bool requestCatalogAligned = exactCatalogOrdinaryIdentity
                && request.MatchesCatalog(
                    catalog,
                    roster,
                    C1FormalItemArtworkLightingState.Lit);
            if (!C1Pool15FormalItemArtworkAndSourceAnchorBinding
                    .IsPlacedLitSourceCandidate(
                        rosters.Length,
                        placements.Length,
                        itemSystemPlacements.Length,
                        trayPlacements.Length,
                        activeTrayPlacementCount,
                        liveAnchors.Length,
                        exactCatalogOrdinaryIdentity,
                        exactIdentityAligned,
                        requestCatalogAligned,
                        isLit))
                return false;

            BoardPoolEntry liveAnchor = liveAnchors[0];
            if (liveAnchor.Rect == null
                || liveAnchor.Image == null
                || liveAnchor.RarityCarrier == null
                || !liveAnchor.RarityCarrier.HasAppliedPresentation
                || !liveAnchor.Image.gameObject.activeSelf
                || !ReferenceEquals(liveAnchor.Image.sprite, artwork))
                return false;

            binding = new C1Pool15FormalItemSourceAnchorBinding(
                snapshot,
                roster,
                placement,
                itemSystemPlacements[0],
                liveAnchor.Rect,
                artwork,
                artworkIdentity,
                catalog,
                catalogRow);
            return binding.BindPresentationCarrier(
                       liveAnchor.RarityCarrier)
                   && binding.sourceAnchor != null
                   && binding.presentationCarrier != null
                   && binding.artwork != null
                   && binding.Matches(request);
        }

        internal bool TryResolveExactPlacedLitPresentationSource(
            C1FormalItemSessionSnapshot expectedSnapshot,
            string itemInstanceId,
            string baseItemId,
            out C1Pool15FormalItemSourceAnchorBinding binding)
        {
            binding = null;
            if (expectedSnapshot == null
                || snapshot == null
                || !ReferenceEquals(snapshot, expectedSnapshot)
                || string.IsNullOrWhiteSpace(itemInstanceId)
                || string.IsNullOrWhiteSpace(baseItemId)
                || !string.Equals(
                    boundSnapshotSignature,
                    expectedSnapshot.canonicalSignature,
                    StringComparison.Ordinal))
            {
                return false;
            }

            C1FormalItemRosterEntrySnapshot[] rosters = snapshot.roster
                .Where(value => value != null && string.Equals(
                    value.itemInstanceId,
                    itemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            C1FormalItemPlacementSnapshot[] placements = snapshot.placements
                .Where(value => value != null && string.Equals(
                    value.itemInstanceId,
                    itemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            C1FormalItemTrayPlacementSnapshot[] trayPlacements =
                snapshot.trayLayout.placements
                    .Where(value => value != null && string.Equals(
                        value.itemInstanceId,
                        itemInstanceId,
                        StringComparison.Ordinal))
                    .ToArray();
            if (rosters.Length != 1
                || placements.Length != 1
                || !string.Equals(
                    rosters[0].baseItemId,
                    baseItemId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            C1FormalItemRosterEntrySnapshot roster = rosters[0];
            C1FormalItemPlacementSnapshot placement = placements[0];
            ItemSystemPlacementSnapshot[] itemSystemPlacements =
                snapshot.itemSystemSnapshot.placements
                    .Where(value => value != null && string.Equals(
                        value.placementId,
                        placement.placementId,
                        StringComparison.Ordinal))
                    .ToArray();
            BoardPoolEntry[] liveAnchors = activePoolEntries.Values
                .Where(value => value != null && string.Equals(
                    value.ItemInstanceId,
                    itemInstanceId,
                    StringComparison.Ordinal))
                .ToArray();
            C1FormalItemPresentationCatalogSnapshot catalog =
                PresentationCatalogSnapshot;
            C1FormalItemPresentationRowSnapshot catalogRow = null;
            Sprite artwork = null;
            string artworkIdentity = string.Empty;
            bool catalogResolved = catalog != null
                && catalog.TryResolveArtwork(
                    roster.baseItemId,
                    roster.rarityKey,
                    C1FormalItemArtworkLightingState.Lit,
                    out catalogRow,
                    out artwork,
                    out artworkIdentity)
                && catalogRow != null
                && roster.isSpecialLightingSource
                    == catalogRow.isSpecialLightingSource;
            bool exactIdentityAligned = itemSystemPlacements.Length == 1
                && string.Equals(
                    placement.baseItemId,
                    roster.baseItemId,
                    StringComparison.Ordinal)
                && string.Equals(
                    itemSystemPlacements[0].itemId,
                    roster.baseItemId,
                    StringComparison.Ordinal)
                && placement.anchorCell == itemSystemPlacements[0].anchorCell
                && placement.rotation == itemSystemPlacements[0].rotation;
            if (!catalogResolved
                || !exactIdentityAligned
                || !itemSystemPlacements[0].isLit
                || trayPlacements.Length > 1
                || trayPlacements.Count(value => value.isActiveInTray) != 0
                || liveAnchors.Length != 1)
            {
                return false;
            }

            BoardPoolEntry liveAnchor = liveAnchors[0];
            if (liveAnchor.Rect == null
                || liveAnchor.Image == null
                || liveAnchor.RarityCarrier == null
                || !liveAnchor.RarityCarrier.HasAppliedPresentation
                || !liveAnchor.Image.gameObject.activeSelf
                || !ReferenceEquals(liveAnchor.Image.sprite, artwork))
            {
                return false;
            }

            binding = new C1Pool15FormalItemSourceAnchorBinding(
                snapshot,
                roster,
                placement,
                itemSystemPlacements[0],
                liveAnchor.Rect,
                artwork,
                artworkIdentity,
                catalog,
                catalogRow);
            return binding.BindPresentationCarrier(
                       liveAnchor.RarityCarrier)
                   && binding.sourceAnchor != null
                   && binding.presentationCarrier != null
                   && binding.artwork != null;
        }

        public void AssignForEditor(
            RectTransform configuredBoardRect,
            RectTransform configuredGridRoot,
            Image[] configuredCells,
            RectTransform configuredArtworkLayer,
            RectTransform[] configuredPlacedArtworkRects,
            Image[] configuredPlacedArtworkImages,
            RectTransform configuredSelectionRect,
            Image configuredSelectionImage,
            RectTransform configuredPreviewRect,
            Image configuredPreviewImage,
            RectTransform configuredPreviewShadowRect,
            Image configuredPreviewShadowImage,
            RectTransform configuredInvalidRect,
            Image configuredInvalidImage,
            RectTransform configuredDragGhostRoot,
            RectTransform configuredDragGhostArtworkRect,
            Image configuredDragGhostArtworkImage,
            RectTransform configuredDragGhostInvalidArtworkRect,
            Image configuredDragGhostInvalidArtworkImage,
            Text configuredDragGhostLabel,
            RectTransform configuredRotateZoneLayer,
            RectTransform configuredRightRotateZone,
            Image configuredRightRotateZoneImage,
            Text configuredRightRotateZoneLabel,
            RectTransform configuredRotateGuide,
            Image configuredRotateGuideImage,
            Image[] configuredFormationRangeImages,
            Text[] configuredFormationCoreLabels,
            Text[] configuredLightingStateLabels,
            C1FormalItemPresentationCatalog configuredPresentationCatalog,
            ItemRarityContourBloomVfx[] configuredPlacedArtworkRarityCarriers =
                null)
        {
            boardRect = configuredBoardRect;
            gridRoot = configuredGridRoot;
            cells = configuredCells ?? Array.Empty<Image>();
            boardItemArtworkLayer = configuredArtworkLayer;
            placedArtworkRects = configuredPlacedArtworkRects
                                 ?? Array.Empty<RectTransform>();
            placedArtworkImages = configuredPlacedArtworkImages
                                  ?? Array.Empty<Image>();
            placedArtworkRarityCarriers =
                configuredPlacedArtworkRarityCarriers
                ?? Array.Empty<ItemRarityContourBloomVfx>();
            selectionArtworkRect = configuredSelectionRect;
            selectionArtworkImage = configuredSelectionImage;
            previewArtworkRect = configuredPreviewRect;
            previewArtworkImage = configuredPreviewImage;
            previewShadowArtworkRect = configuredPreviewShadowRect;
            previewShadowArtworkImage = configuredPreviewShadowImage;
            invalidArtworkRect = configuredInvalidRect;
            invalidArtworkImage = configuredInvalidImage;
            dragGhostRoot = configuredDragGhostRoot;
            dragGhostArtworkRect = configuredDragGhostArtworkRect;
            dragGhostArtworkImage = configuredDragGhostArtworkImage;
            dragGhostInvalidArtworkRect = configuredDragGhostInvalidArtworkRect;
            dragGhostInvalidArtworkImage = configuredDragGhostInvalidArtworkImage;
            dragGhostLabel = configuredDragGhostLabel;
            rotateZoneLayer = configuredRotateZoneLayer;
            rightRotateZone = configuredRightRotateZone;
            rightRotateZoneImage = configuredRightRotateZoneImage;
            rightRotateZoneLabel = configuredRightRotateZoneLabel;
            rotateGuide = configuredRotateGuide;
            rotateGuideImage = configuredRotateGuideImage;
            formationRangeImages = configuredFormationRangeImages
                                   ?? Array.Empty<Image>();
            formationCoreLabels = configuredFormationCoreLabels
                                  ?? Array.Empty<Text>();
            lightingStateLabels = configuredLightingStateLabels
                                  ?? Array.Empty<Text>();
            presentationCatalog = configuredPresentationCatalog;
            presentationPoolInitialized = false;
            presentationPool.Clear();
            activePoolEntries.Clear();
        }

        public bool ValidateAuthoredReferences()
        {
            return boardRect != null
                   && gridRoot != null
                   && cells != null
                   && cells.Length == 25
                   && cells.All(value => value != null)
                   && cells.Distinct().Count() == 25
                   && boardItemArtworkLayer != null
                   && placedArtworkRects != null
                   && placedArtworkRects.Length > 0
                   && placedArtworkRects.Length <= PhysicalPresentationCapacity
                   && placedArtworkRects.All(value => value != null)
                   && placedArtworkRects.Distinct().Count()
                      == placedArtworkRects.Length
                   && placedArtworkImages != null
                   && placedArtworkImages.Length == placedArtworkRects.Length
                   && placedArtworkImages.All(value => value != null)
                   && placedArtworkImages.Distinct().Count()
                      == placedArtworkImages.Length
                   && placedArtworkRarityCarriers != null
                   && placedArtworkRarityCarriers.Length
                      == placedArtworkRects.Length
                   && placedArtworkRarityCarriers.All(value => value != null
                       && value.ValidateAuthoredReferences())
                   && placedArtworkRarityCarriers.Distinct().Count()
                      == placedArtworkRarityCarriers.Length
                   && Enumerable.Range(0, placedArtworkRects.Length).All(
                       index => placedArtworkImages[index].rectTransform
                                == placedArtworkRects[index]
                                && placedArtworkRects[index].parent
                                == boardItemArtworkLayer
                                && placedArtworkRarityCarriers[index].transform
                                   != placedArtworkRects[index]
                                && placedArtworkRarityCarriers[index].transform
                                    .IsChildOf(placedArtworkRects[index]))
                   && selectionArtworkRect != null
                   && selectionArtworkImage != null
                   && previewArtworkRect != null
                   && previewArtworkImage != null
                   && previewShadowArtworkRect != null
                   && previewShadowArtworkImage != null
                   && invalidArtworkRect != null
                   && invalidArtworkImage != null
                   && dragGhostRoot != null
                   && dragGhostArtworkRect != null
                   && dragGhostArtworkImage != null
                   && dragGhostInvalidArtworkRect != null
                   && dragGhostInvalidArtworkImage != null
                   && dragGhostLabel != null
                   && rotateZoneLayer != null
                   && rightRotateZone != null
                   && rightRotateZoneImage != null
                   && !rightRotateZoneImage.raycastTarget
                   && rightRotateZoneLabel != null
                   && rotateGuide != null
                   && rotateGuideImage != null
                   && !rotateGuideImage.raycastTarget
                   && formationRangeImages != null
                   && formationRangeImages.Length == 25
                   && formationRangeImages.All(value => value != null)
                   && formationCoreLabels != null
                   && formationCoreLabels.Length == 25
                   && formationCoreLabels.All(value => value != null)
                   && lightingStateLabels != null
                   && lightingStateLabels.Length == PhysicalPresentationCapacity
                   && lightingStateLabels.All(value => value != null)
                   && PresentationCatalogSnapshot != null;
        }

        internal void BindPresenter(
            C1ExactBattleSandboxItemArrangementPresenter configuredPresenter)
        {
            bool presenterChanged = presenter != configuredPresenter;
            presenter = configuredPresenter;
            if (presenterChanged || presenter == null
                                 || !presenter.IsItemInteractionEnabled)
                CancelInteractionState();
        }

        internal void BindCanonicalItemResolver(
            CanonicalItemDefinitionResolver configuredResolver)
        {
            canonicalItemResolver = configuredResolver;
        }

        internal void CancelInteractionState()
        {
            ClearPointerIdentities();
            ClearInteractionFeedback();
        }

        internal bool TryGetAuthoredCellWorldVectors(
            out Vector3 worldXAxis,
            out Vector3 worldYAxis)
        {
            worldXAxis = Vector3.zero;
            worldYAxis = Vector3.zero;
            RectTransform cellRect = cells != null && cells.Length > 0
                ? cells[0]?.rectTransform
                : null;
            if (cellRect != null
                && cellRect.rect.width > 0f
                && cellRect.rect.height > 0f)
            {
                worldXAxis = cellRect.TransformVector(
                    new Vector3(cellRect.rect.width, 0f, 0f));
                worldYAxis = cellRect.TransformVector(
                    new Vector3(0f, cellRect.rect.height, 0f));
                return worldXAxis.sqrMagnitude > 0.000001f
                       && worldYAxis.sqrMagnitude > 0.000001f;
            }

            GridLayoutGroup authoredGrid = gridRoot == null
                ? null
                : gridRoot.GetComponent<GridLayoutGroup>();
            if (authoredGrid == null
                || authoredGrid.cellSize.x <= 0f
                || authoredGrid.cellSize.y <= 0f)
                return false;
            worldXAxis = gridRoot.TransformVector(
                new Vector3(authoredGrid.cellSize.x, 0f, 0f));
            worldYAxis = gridRoot.TransformVector(
                new Vector3(0f, authoredGrid.cellSize.y, 0f));
            return worldXAxis.sqrMagnitude > 0.000001f
                   && worldYAxis.sqrMagnitude > 0.000001f;
        }

        internal void Bind(
            C1FormalItemSessionSnapshot configuredSnapshot,
            string configuredSelectedItemInstanceId)
        {
            PreparePointerStateForBind(configuredSnapshot);
            snapshot = configuredSnapshot;
            selectedItemInstanceId = configuredSelectedItemInstanceId ?? string.Empty;
            boundSnapshotSignature = snapshot?.canonicalSignature ?? string.Empty;
            ClearInteractionFeedback();
            if (!ValidateAuthoredReferences())
            {
                ClearPlacedArtwork();
                boundSnapshotSignature = string.Empty;
                return;
            }
            RefreshFormationAndLighting();
            if (snapshot == null)
            {
                ClearPlacedArtwork();
                boundSnapshotSignature = string.Empty;
                return;
            }
            C1FormalItemPlacementSnapshot[] placements = snapshot.placements
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray();
            if (!CanBindExactInstanceRows(
                    PhysicalPresentationCapacity,
                    placements.Select(value => new KeyValuePair<string, string>(
                        value?.itemInstanceId,
                        value?.baseItemId)),
                    snapshot.roster.Select(value =>
                        new KeyValuePair<string, string>(
                            value?.itemInstanceId,
                            value?.baseItemId))))
            {
                ClearPlacedArtwork();
                boundSnapshotSignature = string.Empty;
                return;
            }
            if (!PreparePoolForDemand(placements.Select(value =>
                    value.itemInstanceId)))
            {
                ClearPlacedArtwork();
                boundSnapshotSignature = string.Empty;
                return;
            }

            Dictionary<string, ItemSystemPlacementSnapshot>
                resolvedPlacementsByInstance = new(StringComparer.Ordinal);
            Dictionary<string, int> visualQuarterTurnsByInstance =
                new(StringComparer.Ordinal);
            List<KeyValuePair<string, Sprite>> artworkRows = new();
            foreach (C1FormalItemPlacementSnapshot placement in placements)
            {
                if (!activePoolEntries.TryGetValue(
                        placement.itemInstanceId,
                        out _))
                {
                    ClearPlacedArtwork();
                    boundSnapshotSignature = string.Empty;
                    return;
                }
                ItemSystemPlacementSnapshot resolved =
                    snapshot.itemSystemSnapshot.FindPlacement(placement.placementId);
                C1FormalItemRosterEntrySnapshot roster =
                    snapshot.FindRosterEntry(placement.itemInstanceId);
                if (resolved == null || roster == null)
                {
                    ClearPlacedArtwork();
                    boundSnapshotSignature = string.Empty;
                    return;
                }
                if (!C1ExactBattleSandboxItemRotationAdapter
                        .TryFormalDegreesToVisualQuarterTurns(
                            placement.rotation,
                            out int visualQuarterTurns))
                {
                    LogInvalidFormalRotationOnce(placement.rotation);
                    ClearPlacedArtwork();
                    boundSnapshotSignature = string.Empty;
                    return;
                }

                resolvedPlacementsByInstance.Add(
                    placement.itemInstanceId,
                    resolved);
                visualQuarterTurnsByInstance.Add(
                    placement.itemInstanceId,
                    visualQuarterTurns);
                artworkRows.Add(new KeyValuePair<string, Sprite>(
                    placement.itemInstanceId,
                    ResolveArtwork(roster, resolved.isLit)));
            }

            if (!TryBuildInstanceLocalArtworkBindings(
                    artworkRows,
                    out IReadOnlyDictionary<string, Sprite>
                        resolvedArtworkByInstance))
            {
                ClearPlacedArtwork();
                boundSnapshotSignature = string.Empty;
                return;
            }

            ClearSelectionArtwork();
            foreach (C1FormalItemPlacementSnapshot placement in placements)
            {
                BoardPoolEntry entry = activePoolEntries[
                    placement.itemInstanceId];
                if (!resolvedArtworkByInstance.TryGetValue(
                        placement.itemInstanceId,
                        out Sprite sprite))
                {
                    ResetPoolEntry(entry, false);
                    continue;
                }

                ItemSystemPlacementSnapshot resolved =
                    resolvedPlacementsByInstance[placement.itemInstanceId];
                int visualQuarterTurns =
                    visualQuarterTurnsByInstance[placement.itemInstanceId];
                PositionArtworkForCells(
                    entry.Rect,
                    resolved.OccupiedCells,
                    visualQuarterTurns);
                entry.Image.sprite = sprite;
                entry.Image.color = resolved.isLit
                    ? Color.white
                    : new Color(0.72f, 0.75f, 0.82f, 0.88f);
                entry.Image.raycastTarget = false;
                entry.Image.gameObject.SetActive(true);
                C1FormalItemRosterEntrySnapshot roster = snapshot
                    .FindRosterEntry(placement.itemInstanceId);
                if (roster == null
                    || entry.RarityCarrier == null)
                {
                    ResetPoolEntry(entry, false);
                    continue;
                }
                if (roster.isSpecialLightingSource)
                {
                    if (!entry.RarityCarrier.Apply(
                            sprite,
                            ItemRarityContourBloomProfile.SpecialSourceKey))
                    {
                        ResetPoolEntry(entry, false);
                        continue;
                    }
                }
                else if (!entry.RarityCarrier.Apply(
                             sprite,
                             roster.rarityKey))
                {
                    ResetPoolEntry(entry, false);
                    continue;
                }

                if (string.Equals(
                        placement.itemInstanceId,
                        selectedItemInstanceId,
                        StringComparison.Ordinal))
                {
                    PositionArtworkForCells(
                        selectionArtworkRect,
                        resolved.OccupiedCells,
                        visualQuarterTurns);
                    selectionArtworkImage.sprite = sprite;
                    selectionArtworkImage.gameObject.SetActive(true);
                }
            }
        }

        internal static bool TryBuildInstanceLocalArtworkBindings(
            IEnumerable<KeyValuePair<string, Sprite>> rows,
            out IReadOnlyDictionary<string, Sprite> resolvedBindings)
        {
            resolvedBindings = null;
            if (rows == null) return false;
            Dictionary<string, Sprite> result =
                new(StringComparer.Ordinal);
            HashSet<string> seenInstances = new(StringComparer.Ordinal);
            foreach (KeyValuePair<string, Sprite> row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Key)
                    || !seenInstances.Add(row.Key))
                    return false;
                if (!ReferenceEquals(row.Value, null))
                    result.Add(row.Key, row.Value);
            }

            resolvedBindings = result;
            return true;
        }

        internal static bool CanBindExactInstanceRows(
            int physicalCapacity,
            IEnumerable<KeyValuePair<string, string>> containerRows,
            IEnumerable<KeyValuePair<string, string>> rosterRows)
        {
            if (physicalCapacity <= 0
                || containerRows == null
                || rosterRows == null)
                return false;
            KeyValuePair<string, string>[] container = containerRows.ToArray();
            KeyValuePair<string, string>[] roster = rosterRows.ToArray();
            if (container.Length > physicalCapacity
                || container.Any(value => string.IsNullOrWhiteSpace(value.Key)
                    || string.IsNullOrWhiteSpace(value.Value))
                || roster.Any(value => string.IsNullOrWhiteSpace(value.Key)
                    || string.IsNullOrWhiteSpace(value.Value))
                || container.Select(value => value.Key)
                       .Distinct(StringComparer.Ordinal).Count()
                   != container.Length
                || roster.Select(value => value.Key)
                       .Distinct(StringComparer.Ordinal).Count()
                   != roster.Length)
                return false;
            Dictionary<string, string> rosterIdentities = roster.ToDictionary(
                value => value.Key,
                value => value.Value,
                StringComparer.Ordinal);
            return container.All(value => rosterIdentities.TryGetValue(
                    value.Key,
                    out string exactBaseItemId)
                && string.Equals(
                    value.Value,
                    exactBaseItemId,
                    StringComparison.Ordinal));
        }

        internal bool TryScreenPointToCell(
            Vector2 screenPoint,
            Camera eventCamera,
            out Vector2Int cell)
        {
            cell = default;
            if (gridRoot == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    gridRoot,
                    screenPoint,
                    eventCamera,
                    out Vector2 local))
                return false;
            Rect rect = gridRoot.rect;
            if (!rect.Contains(local) || rect.width <= 0f || rect.height <= 0f)
                return false;
            int x = Mathf.FloorToInt((local.x - rect.xMin) / rect.width * 5f);
            int y = Mathf.FloorToInt((local.y - rect.yMin) / rect.height * 5f);
            if (x < 0 || x >= 5 || y < 0 || y >= 5) return false;
            cell = new Vector2Int(x, y);
            return true;
        }

        internal void ShowCandidate(
            string itemInstanceId,
            C1ExactBattleSandboxItemInteractionCandidate candidate,
            bool locallyValid)
        {
            if (presenter == null || !presenter.IsItemInteractionEnabled)
            {
                ClearInteractionFeedback();
                return;
            }
            C1FormalItemRosterEntrySnapshot roster = snapshot?.FindRosterEntry(
                itemInstanceId);
            if (roster == null || candidate == null
                || candidate.OrderedOccupiedCells == null
                || candidate.OrderedOccupiedCells.Count == 0)
            {
                ClearInteractionFeedback();
                return;
            }

            Sprite artwork = ResolveArtworkForRoster(roster);
            if (artwork == null)
            {
                ClearInteractionFeedback();
                return;
            }

            PositionArtworkForCells(
                previewArtworkRect,
                candidate.OrderedOccupiedCells,
                candidate.VisualQuarterTurns);
            PositionArtworkForCells(
                previewShadowArtworkRect,
                candidate.OrderedOccupiedCells,
                candidate.VisualQuarterTurns);
            PositionArtworkForCells(
                invalidArtworkRect,
                candidate.OrderedOccupiedCells,
                candidate.VisualQuarterTurns);
            PositionForCells(dragGhostRoot, candidate.OrderedOccupiedCells);
            StretchToParent(dragGhostArtworkRect);
            StretchToParent(dragGhostInvalidArtworkRect);
            foreach (RectTransform rect in new[]
                     {
                         dragGhostArtworkRect,
                         dragGhostInvalidArtworkRect
                     })
            {
                ApplyVisualRotationWithinOccupiedBounds(
                    rect,
                    candidate.VisualQuarterTurns);
            }

            previewArtworkImage.sprite = artwork;
            previewShadowArtworkImage.sprite = artwork;
            invalidArtworkImage.sprite = artwork;
            dragGhostArtworkImage.sprite = artwork;
            dragGhostInvalidArtworkImage.sprite = artwork;
            previewArtworkImage.gameObject.SetActive(locallyValid);
            previewShadowArtworkImage.gameObject.SetActive(locallyValid);
            invalidArtworkImage.gameObject.SetActive(!locallyValid);
            dragGhostRoot.gameObject.SetActive(true);
            dragGhostArtworkImage.gameObject.SetActive(true);
            dragGhostInvalidArtworkImage.gameObject.SetActive(!locallyValid);
            dragGhostLabel.text = locallyValid ? "PREVIEW" : "INVALID";
            rotateZoneLayer.gameObject.SetActive(true);
            rightRotateZone.gameObject.SetActive(true);
            rotateGuide.gameObject.SetActive(false);
            rightRotateZoneLabel.text = "R 90";
        }

        internal bool TryLayoutRightRotateZone(
            PointerEventData eventData,
            out Rect activationRect,
            out Rect holdRect)
        {
            activationRect = default;
            holdRect = default;
            if (presenter == null || !presenter.IsItemInteractionEnabled
                                  || eventData == null
                                  || dragGhostRoot == null
                                  || !dragGhostRoot.gameObject.activeInHierarchy
                                  || rotateZoneLayer == null
                                  || rightRotateZone == null
                                  || rotateGuide == null)
                return false;

            Camera eventCamera = eventData.pressEventCamera
                                 ?? eventData.enterEventCamera;
            dragGhostRoot.GetWorldCorners(dragGhostWorldCorners);
            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            for (int index = 0; index < dragGhostWorldCorners.Length; index++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                    eventCamera,
                    dragGhostWorldCorners[index]);
                minX = Mathf.Min(minX, screenPoint.x);
                minY = Mathf.Min(minY, screenPoint.y);
                maxX = Mathf.Max(maxX, screenPoint.x);
                maxY = Mathf.Max(maxY, screenPoint.y);
            }

            if (maxX <= minX || maxY <= minY) return false;
            Rect ghostRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            float visualInset = RotateButtonVisualSizePixels * 0.5f
                                + RotateButtonInsideEdgeInsetPixels;
            float xInset = Mathf.Min(visualInset, ghostRect.width * 0.36f);
            float yInset = Mathf.Min(visualInset, ghostRect.height * 0.36f);
            Vector2 visualCenter = new(
                ghostRect.xMax - xInset,
                ghostRect.yMin + yInset);
            activationRect = BuildCenteredRect(
                visualCenter,
                new Vector2(
                    C1ExactBattleSandboxRightRotateZoneState
                        .ActivationWidthPixels,
                    C1ExactBattleSandboxRightRotateZoneState
                        .ActivationHeightPixels));
            holdRect = ExpandRect(
                activationRect,
                C1ExactBattleSandboxRightRotateZoneState
                    .BoardHoldPaddingPixels);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rotateZoneLayer,
                    visualCenter,
                    eventCamera,
                    out Vector2 zoneLocal))
                return false;

            rightRotateZone.anchorMin = new Vector2(0.5f, 0.5f);
            rightRotateZone.anchorMax = new Vector2(0.5f, 0.5f);
            rightRotateZone.pivot = new Vector2(0.5f, 0.5f);
            rightRotateZone.anchoredPosition = zoneLocal;
            rightRotateZone.sizeDelta = Vector2.one * RotateButtonVisualSizePixels;
            rightRotateZone.localScale = Vector3.one;
            PositionRotateGuide(ghostRect.center, visualCenter, eventCamera);
            return true;
        }

        internal void SetRotateFeedbackState(bool confirmed, bool guideVisible)
        {
            SetRotateFeedbackConfirmed(confirmed);
            if (rotateGuide != null)
                rotateGuide.gameObject.SetActive(
                    guideVisible
                    && presenter != null
                    && presenter.IsItemInteractionEnabled);
        }

        private void PositionRotateGuide(
            Vector2 startScreen,
            Vector2 endScreen,
            Camera eventCamera)
        {
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
                rotateGuide.gameObject.SetActive(false);
                return;
            }

            Vector2 delta = endLocal - startLocal;
            if (delta.sqrMagnitude <= 1f)
            {
                rotateGuide.gameObject.SetActive(false);
                return;
            }

            rotateGuide.anchorMin = new Vector2(0.5f, 0.5f);
            rotateGuide.anchorMax = new Vector2(0.5f, 0.5f);
            rotateGuide.pivot = new Vector2(0.5f, 0.5f);
            rotateGuide.anchoredPosition = (startLocal + endLocal) * 0.5f;
            rotateGuide.sizeDelta = new Vector2(delta.magnitude, 4f);
            rotateGuide.localScale = Vector3.one;
            rotateGuide.localEulerAngles = new Vector3(
                0f,
                0f,
                Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private static Rect BuildCenteredRect(Vector2 center, Vector2 size)
        {
            Vector2 half = size * 0.5f;
            return Rect.MinMaxRect(
                center.x - half.x,
                center.y - half.y,
                center.x + half.x,
                center.y + half.y);
        }

        private static Rect ExpandRect(Rect source, float padding)
        {
            return Rect.MinMaxRect(
                source.xMin - padding,
                source.yMin - padding,
                source.xMax + padding,
                source.yMax + padding);
        }

        internal void SetRotateFeedbackConfirmed(bool confirmed)
        {
            if (presenter == null || !presenter.IsItemInteractionEnabled)
                confirmed = false;
            if (rightRotateZoneImage != null)
                rightRotateZoneImage.color = confirmed
                    ? new Color(0.35f, 1f, 0.55f, 0.9f)
                    : new Color(0.42f, 0.88f, 0.66f, 0.55f);
            if (rotateGuideImage != null)
                rotateGuideImage.color = confirmed
                    ? new Color(0.35f, 1f, 0.55f, 0.55f)
                    : new Color(0.42f, 0.88f, 0.66f, 0.32f);
        }

        internal void ClearInteractionFeedback()
        {
            foreach (Image image in new[]
                     {
                         previewArtworkImage,
                         previewShadowArtworkImage,
                         invalidArtworkImage,
                         dragGhostArtworkImage,
                         dragGhostInvalidArtworkImage
                     })
            {
                if (image != null) image.gameObject.SetActive(false);
            }

            if (dragGhostRoot != null) dragGhostRoot.gameObject.SetActive(false);
            if (rotateZoneLayer != null) rotateZoneLayer.gameObject.SetActive(false);
            SetRotateFeedbackConfirmed(false);
        }

        internal C1FormalItemPlacementSnapshot FindPlacementAtCell(Vector2Int cell)
        {
            if (snapshot == null) return null;
            foreach (C1FormalItemPlacementSnapshot placement in snapshot.placements)
            {
                ItemSystemPlacementSnapshot resolved =
                    snapshot.itemSystemSnapshot.FindPlacement(placement.placementId);
                if (resolved != null && resolved.OccupiedCells.Contains(cell))
                    return placement;
            }

            return null;
        }

        internal bool TryGetActiveBoardDragRawGrabbedCell(
            string itemInstanceId,
            out Vector2Int rawGrabbedCell)
        {
            if (hasActiveBoardDragRawGrabbedCell
                && string.Equals(
                    activeBoardDragItemInstanceId,
                    itemInstanceId,
                    StringComparison.Ordinal))
            {
                rawGrabbedCell = activeBoardDragRawGrabbedCell;
                return true;
            }

            rawGrabbedCell = default;
            return false;
        }

        private bool TryResolveRawGrabbedCell(
            Vector2Int occupiedBoardCell,
            out C1FormalItemPlacementSnapshot placement,
            out Vector2Int rawGrabbedCell)
        {
            placement = FindPlacementAtCell(occupiedBoardCell);
            rawGrabbedCell = default;
            if (placement == null || snapshot == null) return false;
            C1FormalItemRosterEntrySnapshot roster = snapshot.FindRosterEntry(
                placement.itemInstanceId);
            ItemSystemCatalogItemSnapshot catalog = snapshot.itemSystemSnapshot
                .catalogItems.FirstOrDefault(value => roster != null
                    && string.Equals(
                        value.itemId,
                        roster.baseItemId,
                        StringComparison.Ordinal));
            if (catalog?.ShapeCells == null
                || catalog.ShapeCells.Count == 0)
                return false;

            bool found = false;
            foreach (Vector2Int rawCell in catalog.ShapeCells)
            {
                if (!C1ExactBattleSandboxItemRotationAdapter.TryRotateFormalCell(
                        rawCell,
                        placement.rotation,
                        out Vector2Int rotatedCell)
                    || placement.anchorCell + rotatedCell != occupiedBoardCell)
                    continue;
                if (found) return false;
                found = true;
                rawGrabbedCell = rawCell;
            }

            return found;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ClearPointerIdentities();
            if (presenter == null || !presenter.IsItemInteractionEnabled) return;
            if (eventData == null || !TryScreenPointToCell(
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2Int cell))
                return;
            if (!TryResolveRawGrabbedCell(
                    cell,
                    out C1FormalItemPlacementSnapshot placement,
                    out pressedRawGrabbedCell))
                return;
            pressedItemInstanceId = placement.itemInstanceId;
            hasPressedRawGrabbedCell = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (presenter == null || !presenter.IsItemInteractionEnabled) return;
            if (eventData == null || !TryScreenPointToCell(
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2Int cell))
                return;
            C1FormalItemPlacementSnapshot placement = FindPlacementAtCell(cell);
            if (placement != null) presenter?.SelectBoardItem(placement.itemInstanceId);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (presenter == null || !presenter.IsItemInteractionEnabled)
            {
                CancelInteractionState();
                return;
            }
            CapturePressedIdentityForActiveBoardDrag();
            if (activeBoardDragItemInstanceId.Length > 0)
                presenter.BeginBoardDrag(
                    activeBoardDragItemInstanceId,
                    eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (presenter == null || !presenter.IsItemInteractionEnabled)
            {
                CancelInteractionState();
                return;
            }
            if (activeBoardDragItemInstanceId.Length > 0)
                presenter.UpdateBoardDrag(
                    activeBoardDragItemInstanceId,
                    eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            string endedItemInstanceId = activeBoardDragItemInstanceId;
            if (presenter != null && presenter.IsItemInteractionEnabled
                                  && endedItemInstanceId.Length > 0)
                presenter.EndBoardDrag(endedItemInstanceId, eventData);
            ClearPointerIdentities();
        }

        private void CapturePressedIdentityForActiveBoardDrag()
        {
            activeBoardDragItemInstanceId = pressedItemInstanceId;
            activeBoardDragRawGrabbedCell = pressedRawGrabbedCell;
            hasActiveBoardDragRawGrabbedCell =
                activeBoardDragItemInstanceId.Length > 0
                && hasPressedRawGrabbedCell;
        }

        private void PreparePointerStateForBind(
            C1FormalItemSessionSnapshot configuredSnapshot)
        {
            pressedItemInstanceId = string.Empty;
            pressedRawGrabbedCell = default;
            hasPressedRawGrabbedCell = false;
            if (!HasSameSessionIdentity(snapshot, configuredSnapshot))
            {
                activeBoardDragItemInstanceId = string.Empty;
                activeBoardDragRawGrabbedCell = default;
                hasActiveBoardDragRawGrabbedCell = false;
            }
        }

        private void ClearPointerIdentities()
        {
            pressedItemInstanceId = string.Empty;
            pressedRawGrabbedCell = default;
            hasPressedRawGrabbedCell = false;
            activeBoardDragItemInstanceId = string.Empty;
            activeBoardDragRawGrabbedCell = default;
            hasActiveBoardDragRawGrabbedCell = false;
        }

        private static bool HasSameSessionIdentity(
            C1FormalItemSessionSnapshot left,
            C1FormalItemSessionSnapshot right)
        {
            if (left == null || right == null) return left == right;
            return string.Equals(
                       left.productContext,
                       right.productContext,
                       StringComparison.Ordinal)
                   && string.Equals(
                       left.sessionToken,
                       right.sessionToken,
                       StringComparison.Ordinal)
                   && left.resetGeneration == right.resetGeneration;
        }

        private void ClearPlacedArtwork()
        {
            if (!presentationPoolInitialized) InitializePresentationPool(false);
            foreach (BoardPoolEntry entry in presentationPool)
                ResetPoolEntry(entry, true);
            activePoolEntries.Clear();

            ClearSelectionArtwork();
        }

        private void ClearSelectionArtwork()
        {
            if (selectionArtworkImage != null)
            {
                selectionArtworkImage.sprite = null;
                selectionArtworkImage.gameObject.SetActive(false);
            }
        }

        internal void UnbindPresentation()
        {
            snapshot = null;
            selectedItemInstanceId = string.Empty;
            boundSnapshotSignature = string.Empty;
            ClearPointerIdentities();
            ClearPlacedArtwork();
            ClearInteractionFeedback();
        }

        private bool PreparePoolForDemand(IEnumerable<string> itemInstanceIds)
        {
            if (itemInstanceIds == null || !InitializePresentationPool(true))
                return false;
            string[] desired = itemInstanceIds.ToArray();
            if (desired.Length > PhysicalPresentationCapacity
                || desired.Any(string.IsNullOrWhiteSpace)
                || desired.Distinct(StringComparer.Ordinal).Count()
                   != desired.Length
                || !EnsurePresentationPoolCapacity(Math.Max(
                    desired.Length,
                    Math.Min(InitialPrewarmCount,
                        PhysicalPresentationCapacity))))
                return false;

            HashSet<string> desiredSet = new(
                desired,
                StringComparer.Ordinal);
            foreach (BoardPoolEntry entry in presentationPool)
            {
                if (entry.ItemInstanceId.Length > 0
                    && !desiredSet.Contains(entry.ItemInstanceId))
                {
                    activePoolEntries.Remove(entry.ItemInstanceId);
                    ResetPoolEntry(entry, true);
                }
                else
                {
                    ResetPoolEntry(entry, false);
                }
            }

            foreach (string itemInstanceId in desired)
            {
                if (activePoolEntries.ContainsKey(itemInstanceId)) continue;
                BoardPoolEntry entry = presentationPool.FirstOrDefault(value =>
                    value.ItemInstanceId.Length == 0);
                if (entry == null) return false;
                entry.ItemInstanceId = itemInstanceId;
                activePoolEntries.Add(itemInstanceId, entry);
            }

            return activePoolEntries.Count == desired.Length;
        }

        private bool InitializePresentationPool(bool prewarm)
        {
            if (!presentationPoolInitialized)
            {
                presentationPool.Clear();
                activePoolEntries.Clear();
                if (placedArtworkRects == null
                    || placedArtworkImages == null
                    || placedArtworkRarityCarriers == null
                    || placedArtworkRects.Length == 0
                    || placedArtworkRects.Length != placedArtworkImages.Length
                    || placedArtworkRects.Length
                       != placedArtworkRarityCarriers.Length)
                    return false;
                for (int index = 0; index < placedArtworkRects.Length; index++)
                {
                    RectTransform rect = placedArtworkRects[index];
                    Image image = placedArtworkImages[index];
                    ItemRarityContourBloomVfx rarityCarrier =
                        placedArtworkRarityCarriers[index];
                    if (rect == null || image == null || rarityCarrier == null
                                     || image.rectTransform != rect
                                     || !TryBuildCarrierSiblingPath(
                                         rect,
                                         rarityCarrier.transform,
                                         out int[] carrierSiblingPath))
                        return false;
                    BoardPoolEntry entry = new(
                        rect,
                        image,
                        rarityCarrier,
                        carrierSiblingPath);
                    presentationPool.Add(entry);
                    ResetPoolEntry(entry, true);
                }
                presentationPoolInitialized = true;
            }

            return !prewarm || EnsurePresentationPoolCapacity(Math.Min(
                InitialPrewarmCount,
                PhysicalPresentationCapacity));
        }

        private bool EnsurePresentationPoolCapacity(int requiredCapacity)
        {
            if (!presentationPoolInitialized
                || requiredCapacity < 0
                || requiredCapacity > PhysicalPresentationCapacity)
                return false;
            if (presentationPool.Count >= requiredCapacity) return true;
            BoardPoolEntry template = presentationPool.FirstOrDefault();
            if (template?.Image == null || boardItemArtworkLayer == null)
                return false;
            while (presentationPool.Count < requiredCapacity)
            {
                GameObject clone = Instantiate(
                    template.Image.gameObject,
                    boardItemArtworkLayer,
                    false);
                if (clone == null) return false;
                clone.name = "BoardPlacedArtwork_RuntimePool_"
                             + (presentationPool.Count + 1).ToString("00");
                Image image = clone.GetComponent<Image>();
                if (image == null
                    || !TryResolveCarrierAtSiblingPath(
                        clone.transform,
                        template.CarrierSiblingPath,
                        out ItemRarityContourBloomVfx rarityCarrier))
                {
                    Destroy(clone);
                    return false;
                }
                BoardPoolEntry entry = new(
                    image.rectTransform,
                    image,
                    rarityCarrier,
                    template.CarrierSiblingPath.ToArray());
                presentationPool.Add(entry);
                ResetPoolEntry(entry, true);
            }

            return true;
        }

        private static void ResetPoolEntry(
            BoardPoolEntry entry,
            bool clearIdentity)
        {
            if (entry == null) return;
            if (clearIdentity) entry.ItemInstanceId = string.Empty;
            if (entry.RarityCarrier != null)
                entry.RarityCarrier.Clear();
            if (entry.Rect != null)
            {
                entry.Rect.localRotation = Quaternion.identity;
                entry.Rect.localScale = Vector3.one;
            }
            if (entry.Image != null)
            {
                entry.Image.sprite = null;
                entry.Image.color = Color.clear;
                entry.Image.raycastTarget = false;
                entry.Image.gameObject.SetActive(false);
            }
        }

        private static bool TryBuildCarrierSiblingPath(
            Transform root,
            Transform carrier,
            out int[] siblingPath)
        {
            siblingPath = Array.Empty<int>();
            if (root == null || carrier == null || carrier == root
                             || !carrier.IsChildOf(root))
                return false;
            Stack<int> path = new();
            Transform current = carrier;
            while (current != null && current != root)
            {
                path.Push(current.GetSiblingIndex());
                current = current.parent;
            }
            if (current != root || path.Count == 0) return false;
            siblingPath = path.ToArray();
            return true;
        }

        private static bool TryResolveCarrierAtSiblingPath(
            Transform clonedRoot,
            IReadOnlyList<int> siblingPath,
            out ItemRarityContourBloomVfx rarityCarrier)
        {
            rarityCarrier = null;
            if (clonedRoot == null || siblingPath == null
                                   || siblingPath.Count == 0)
                return false;
            Transform current = clonedRoot;
            for (int index = 0; index < siblingPath.Count; index++)
            {
                int siblingIndex = siblingPath[index];
                if (siblingIndex < 0 || siblingIndex >= current.childCount)
                    return false;
                current = current.GetChild(siblingIndex);
            }
            rarityCarrier = current.GetComponent<ItemRarityContourBloomVfx>();
            return rarityCarrier != null;
        }

        private void RefreshFormationAndLighting()
        {
            for (int index = 0; index < PhysicalPresentationCapacity; index++)
            {
                bool hasSnapshot = snapshot != null;
                Vector2Int cell = new Vector2Int(index % 5, index / 5);
                bool isEye = hasSnapshot && snapshot.itemSystemSnapshot.eyeCell == cell;
                bool inLightingRange = hasSnapshot
                                       && snapshot.itemSystemSnapshot.LitRangeCells.Contains(cell);
                formationRangeImages[index].color = isEye
                    ? new Color(1f, 0.74f, 0.22f, 0.28f)
                    : inLightingRange
                        ? new Color(0.35f, 1f, 0.55f, 0.16f)
                        : Color.clear;
                formationCoreLabels[index].text = isEye ? "\u9635\u773c" : string.Empty;
                C1FormalItemPlacementSnapshot placement = hasSnapshot
                    ? FindPlacementAtCell(cell)
                    : null;
                ItemSystemPlacementSnapshot resolved = placement == null
                    ? null
                    : snapshot.itemSystemSnapshot.FindPlacement(placement.placementId);
                lightingStateLabels[index].text = resolved == null
                    ? string.Empty
                    : resolved.isLightingSource
                        ? "SOURCE"
                        : resolved.isLit ? "LIT" : "UNLIT";
            }
        }

        private Sprite ResolveArtworkForRoster(C1FormalItemRosterEntrySnapshot roster)
        {
            if (roster == null) return null;
            C1FormalItemPlacementSnapshot placement = snapshot.FindPlacementByInstanceId(
                roster.itemInstanceId);
            ItemSystemPlacementSnapshot resolved = placement == null
                ? null
                : snapshot.itemSystemSnapshot.FindPlacement(placement.placementId);
            return ResolveArtwork(roster, resolved != null && resolved.isLit);
        }

        private Sprite ResolveArtwork(
            C1FormalItemRosterEntrySnapshot roster,
            bool lit)
        {
            if (roster == null) return null;
            if (roster.ordinaryInstance != null
                && CanonicalItemArtworkResolver.TryResolve(
                    roster.ordinaryInstance.canonicalDefinition,
                    roster.ordinaryInstance.rarity,
                    out Sprite canonicalArtwork,
                    out _))
                return canonicalArtwork;
            return C1Pool15FormalItemArtworkAndSourceAnchorBinding
                .TryResolveArtwork(
                    PresentationCatalogSnapshot,
                    roster.baseItemId,
                    roster.rarityKey,
                    lit
                        ? C1FormalItemArtworkLightingState.Lit
                        : C1FormalItemArtworkLightingState.Unlit,
                    out _,
                    out Sprite artwork,
                    out _)
                ? artwork
                : null;
        }

        private static void PositionForCells(
            RectTransform rect,
            IReadOnlyList<Vector2Int> occupiedCells)
        {
            if (rect == null || occupiedCells == null || occupiedCells.Count == 0)
                return;
            int minX = occupiedCells.Min(value => value.x);
            int maxX = occupiedCells.Max(value => value.x);
            int minY = occupiedCells.Min(value => value.y);
            int maxY = occupiedCells.Max(value => value.y);
            rect.anchorMin = new Vector2(minX / 5f, minY / 5f);
            rect.anchorMax = new Vector2((maxX + 1) / 5f, (maxY + 1) / 5f);
            rect.offsetMin = new Vector2(3f, 3f);
            rect.offsetMax = new Vector2(-3f, -3f);
            rect.localScale = Vector3.one;
        }

        private static void PositionArtworkForCells(
            RectTransform rect,
            IReadOnlyList<Vector2Int> occupiedCells,
            int visualQuarterTurns)
        {
            PositionForCells(rect, occupiedCells);
            ApplyVisualRotationWithinOccupiedBounds(rect, visualQuarterTurns);
        }

        private static void ApplyVisualRotationWithinOccupiedBounds(
            RectTransform rect,
            int visualQuarterTurns)
        {
            if (rect == null) return;
            int normalized = C1ExactBattleSandboxItemRotationAdapter
                .NormalizeVisualQuarterTurns(visualQuarterTurns);
            Vector2 unrotatedSize = GetUnrotatedArtworkSizeForOccupiedBounds(
                rect.rect.size,
                normalized);
            rect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                unrotatedSize.x);
            rect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                unrotatedSize.y);
            rect.localScale = Vector3.one;
            rect.localEulerAngles = new Vector3(0f, 0f, -90f * normalized);
        }

        private static Vector2 GetUnrotatedArtworkSizeForOccupiedBounds(
            Vector2 occupiedBoundsSize,
            int visualQuarterTurns)
        {
            return (C1ExactBattleSandboxItemRotationAdapter
                    .NormalizeVisualQuarterTurns(visualQuarterTurns) & 1) != 0
                ? new Vector2(occupiedBoundsSize.y, occupiedBoundsSize.x)
                : occupiedBoundsSize;
        }

        private static void StretchToParent(RectTransform rect)
        {
            if (rect == null) return;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private void LogInvalidFormalRotationOnce(int formalDegrees)
        {
            if (invalidFormalRotationLogged) return;
            invalidFormalRotationLogged = true;
            Debug.LogError("[C1ExactBattleSandboxItemBoardView] "
                           + C1ExactBattleSandboxItemRotationAdapter
                               .InvalidFormalDegreesDiagnostic
                           + " degrees=" + formalDegrees, this);
        }
    }
}
