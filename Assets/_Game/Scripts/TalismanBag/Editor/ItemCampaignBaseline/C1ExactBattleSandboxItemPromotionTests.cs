using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using TalismanBag.Items;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.CampaignLoot;
using TalismanBag.Items.Canonical;
using TalismanBag.Items.InnerCatalog;
using TalismanBag.Presentation.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public static class C1ExactBattleSandboxItemPromotionTests
    {
        public const string TerminalMarker =
            "C1_EXACT_BATTLESANDBOX_ITEM_PROMOTION_FOCUSED_TESTS_PASS";
        public const string Rev04TerminalMarker =
            "C1_EXACT_BATTLESANDBOX_ITEM_REV04_DETERMINISTIC_TESTS_PASS";
        public const string Pool15BindingTerminalMarker =
            "C1_POOL15_EXACT_ARTWORK_SOURCE_ANCHOR_BINDING_TESTS_PASS";
        public const string Rev04DynamicPhaseATerminalMarker =
            "C1_DYNAMIC_ITEM_PRESENTATION_REV04_PHASE_A_TESTS_PASS";
        public const string Rev06CarrierPhaseATerminalMarker =
            "C1_AUTHORED_SEVEN_LAYER_ITEM_CARRIERS_REV06_PHASE_A_TESTS_PASS";
        public const string Rev07RewardedItemDragTerminalMarker =
            "C1_REV07_REWARDED_ITEM_DRAG_PLACEMENT_TESTS_PASS";

        private const string BoardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemBoard.prefab";
        private const string TrayPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemTray.prefab";
        private const string CardPrefabPath =
            "Assets/_Game/Prefabs/TalismanBag/Items/"
            + "C1ExactBattleSandboxItemCard.prefab";
        private const string PromotionEditorPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCampaignBaseline/"
            + "C1ExactBattleSandboxItemPromotionEditor.cs";

        private static readonly string[] RuntimeSourcePaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1ExactBattleSandboxItemArrangementPresenter.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1ExactBattleSandboxItemBoardView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1ExactBattleSandboxItemTrayView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1ExactBattleSandboxItemCardView.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1Pool15FormalItemArtworkAndSourceAnchorBinding.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/"
            + "C1FormalItemPresentationCatalog.cs"
        };

        private static int assertionCount;

        public static int RunFocused()
        {
            try
            {
                RunAllOrThrow();
                Console.WriteLine(TerminalMarker + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_EXACT_BATTLESANDBOX_ITEM_PROMOTION_FOCUSED_TESTS_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        public static int RunRev04Deterministic()
        {
            try
            {
                RunRev04DeterministicOrThrow();
                Console.WriteLine(Rev04TerminalMarker + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_EXACT_BATTLESANDBOX_ITEM_REV04_DETERMINISTIC_TESTS_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        public static int RunPool15BindingFocused()
        {
            try
            {
                RunAllOrThrow();
                Console.WriteLine(Pool15BindingTerminalMarker + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_POOL15_EXACT_ARTWORK_SOURCE_ANCHOR_BINDING_TESTS_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        public static int RunRev04DynamicPresentationPhaseA()
        {
            try
            {
                RunRev04DynamicPresentationPhaseAOrThrow();
                C1FormalItemDetailProjectionAndSelectionTests.RunAllOrThrow();
                Console.WriteLine(Rev04DynamicPhaseATerminalMarker
                    + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_DYNAMIC_ITEM_PRESENTATION_REV04_PHASE_A_TESTS_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        public static int RunRev06AuthoredCarrierPhaseA()
        {
            try
            {
                assertionCount = 0;
                VerifyRev06CarrierLifecycleContract();
                VerifyRev04FormalRoundTripTransactions();
                Console.WriteLine(Rev06CarrierPhaseATerminalMarker
                    + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_AUTHORED_SEVEN_LAYER_ITEM_CARRIERS_REV06_PHASE_A_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        public static int RunRev07RewardedItemDragPlacement()
        {
            try
            {
                assertionCount = 0;
                VerifyRev07RewardedItemDragRoute();
                Console.WriteLine(Rev07RewardedItemDragTerminalMarker
                    + " / assertions="
                    + assertionCount.ToString(CultureInfo.InvariantCulture));
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    "C1_REV07_REWARDED_ITEM_DRAG_PLACEMENT_TESTS_FAIL / "
                    + exception.GetType().Name + ": " + exception.Message);
                return 1;
            }
        }

        private static void VerifyRev07RewardedItemDragRoute()
        {
            string presenterSource = File.ReadAllText(RuntimeSourcePaths[0]);
            string boardSource = File.ReadAllText(RuntimeSourcePaths[1]);
            string traySource = File.ReadAllText(RuntimeSourcePaths[2]);
            string cardSource = File.ReadAllText(RuntimeSourcePaths[3]);
            string beginInteraction = ExtractMethodBody(
                presenterSource,
                "private void BeginInteraction(");
            string beginCardDrag = ExtractMethodBody(
                presenterSource,
                "public void BeginCardDrag(");
            string commit = ExtractMethodBody(
                presenterSource,
                "public bool CommitPreviewedCandidate(");
            string clearCard = ExtractMethodBody(
                cardSource,
                "internal void Clear(");
            string recycleCard = ExtractMethodBody(
                cardSource,
                "internal void Recycle(");
            string preparePool = ExtractMethodBody(
                traySource,
                "private bool PreparePoolForDemand(");
            string resolveBoardSource = ExtractMethodBody(
                boardSource,
                "internal bool TryResolveExactPlacedLitSourceAnchor(");
            string resolvePresenterSource = ExtractMethodBody(
                presenterSource,
                "public bool TryResolveCurrentPlacedLitSourceAnchor(");

            Require(beginInteraction.IndexOf(
                        "PublishCurrent();",
                        StringComparison.Ordinal) < 0
                    && beginInteraction.IndexOf(
                        "activeItemInstanceId = itemInstanceId;",
                        StringComparison.Ordinal) >= 0
                    && beginInteraction.IndexOf(
                        "activeRawGrabbedCell = rawGrabbedCell;",
                        StringComparison.Ordinal) >= 0
                    && beginInteraction.IndexOf(
                        "activeExpectedSignature = authority.Current.canonicalSignature;",
                        StringComparison.Ordinal) >= 0,
                "REV07_DRAG_BEGIN_MUST_KEEP_CURRENT_CARD_BOUND");
            Require(beginCardDrag.IndexOf(
                        "card.ItemInstanceId",
                        StringComparison.Ordinal) >= 0
                    && beginCardDrag.IndexOf(
                        "card.Rotation",
                        StringComparison.Ordinal) >= 0
                    && beginCardDrag.IndexOf(
                        "rawGrabbedCell",
                        StringComparison.Ordinal) >= 0
                    && Regex.Matches(
                        cardSource,
                        @"presenter\?\.BeginCardDrag\(this,\s*eventData\);")
                        .Count == 1,
                "REV07_CARD_MUST_HAVE_ONE_EXACT_LIVE_DRAG_ROUTE");
            Require(commit.IndexOf(
                        "C1FormalItemArrangementCommandKind.PlaceFromTray",
                        StringComparison.Ordinal) >= 0
                    && commit.IndexOf(
                        "SubmitExactlyOne(",
                        StringComparison.Ordinal) >= 0
                    && commit.IndexOf(
                        "PublishCurrent();",
                        StringComparison.Ordinal) >= 0,
                "REV07_ACCEPT_OR_REJECT_MUST_PUBLISH_AFTER_AUTHORITY_RESULT");
            Require(clearCard.IndexOf(
                        "ResetGesture();",
                        StringComparison.Ordinal) >= 0
                    && recycleCard.IndexOf(
                        "BindPresenter(null);",
                        StringComparison.Ordinal) >= 0
                    && preparePool.IndexOf(
                        "entry.Card?.Clear();",
                        StringComparison.Ordinal) >= 0
                    && preparePool.IndexOf(
                        "entry.Card?.Recycle();",
                        StringComparison.Ordinal) >= 0
                    && preparePool.IndexOf(
                        "entry.Card?.BindPresenter(presenter);",
                        StringComparison.Ordinal) >= 0,
                "REV07_REBIND_RECYCLE_MUST_CLEAR_STALE_GESTURE_ROUTE");
            Require(resolveBoardSource.IndexOf(
                        "liveAnchor.RarityCarrier",
                        StringComparison.Ordinal) >= 0
                    && resolveBoardSource.IndexOf(
                        "BindPresentationCarrier(",
                        StringComparison.Ordinal) >= 0
                    && resolveBoardSource.IndexOf(
                        "binding.presentationCarrier != null",
                        StringComparison.Ordinal) >= 0
                    && resolvePresenterSource.IndexOf(
                        "boardView.TryResolveExactPlacedLitSourceAnchor(",
                        StringComparison.Ordinal) >= 0
                    && resolvePresenterSource.IndexOf(
                        "binding.Matches(request)",
                        StringComparison.Ordinal) >= 0,
                "REV07_ACCEPTED_PLACEMENT_MUST_EXPOSE_CURRENT_BOARD_CARRIER");
        }

        public static void RunRev04DynamicPresentationPhaseAOrThrow()
        {
            assertionCount = 0;
            VerifyDynamicPresentationAdmission();
            VerifyPool15SourceCandidateAdmissionMatrix();
            VerifyRuntimeIsolation();
        }

        private static void VerifyRev06CarrierLifecycleContract()
        {
            string cardSource = File.ReadAllText(RuntimeSourcePaths[3]);
            string boardSource = File.ReadAllText(RuntimeSourcePaths[1]);
            string bindingSource = File.ReadAllText(RuntimeSourcePaths[4]);
            string editorSource = File.ReadAllText(PromotionEditorPath);

            Require(typeof(C1ExactBattleSandboxItemCardView)
                        .GetProperty("RarityContourBloomVfx")?.PropertyType
                    == typeof(ItemRarityContourBloomVfx)
                    && cardSource.Contains("rarityContourBloomVfx.Apply(")
                    && cardSource.Contains("resolvedArtwork")
                    && cardSource.Contains("roster.rarityKey")
                    && cardSource.Contains("roster.isSpecialLightingSource")
                    && cardSource.Contains("rarityContourBloomVfx.Clear();")
                    && cardSource.Contains("internal void Recycle()")
                    && cardSource.Contains("private void OnDisable()"),
                "REV06_CARD_APPLY_CLEAR_LIFECYCLE_MISSING");

            PropertyInfo bindingCarrier = typeof(
                    C1Pool15FormalItemSourceAnchorBinding)
                .GetProperty("presentationCarrier");
            Require(bindingCarrier?.PropertyType
                    == typeof(ItemRarityContourBloomVfx)
                    && boardSource.Contains("liveAnchor.RarityCarrier")
                    && boardSource.Contains("entry.RarityCarrier.Apply(")
                    && boardSource.Contains("roster.isSpecialLightingSource")
                    && boardSource.Contains("entry.RarityCarrier.Clear();")
                    && boardSource.Contains(
                        "TryResolveCarrierAtSiblingPath(")
                    && boardSource.IndexOf(
                        "GetComponentsInChildren<ItemRarityContourBloomVfx>",
                        StringComparison.Ordinal) < 0
                    && boardSource.IndexOf(
                        "AddComponent<ItemRarityContourBloomVfx>",
                        StringComparison.Ordinal) < 0
                    && cardSource.IndexOf(
                        "AddComponent<ItemRarityContourBloomVfx>",
                        StringComparison.Ordinal) < 0,
                "REV06_BOARD_CARRIER_OR_SOURCE_BINDING_CONTRACT_MISSING");

            Require(bindingSource.Contains("BindPresentationCarrier(")
                    && bindingSource.Contains("HasAppliedPresentation")
                    && editorSource.Contains(
                        "ExecuteRev06AuthoredSevenLayerCarriersFromCommandLine")
                    && editorSource.Contains(
                        "ItemRarityContourBloomVfx.prefab")
                    && editorSource.Contains("graphics.Length == 7")
                    && editorSource.Contains("!value.raycastTarget")
                    && editorSource.Contains("value.maskable"),
                "REV06_AUTHORING_OR_SEVEN_GRAPHIC_STATIC_VALIDATION_MISSING");

            Require(InvokePool15SourceCandidateAdmission(
                    1, 1, 1, 1, 0, 1, true, true, true, true),
                "REV06_PLACED_LIT_SOURCE_REJECTED");
            Require(!InvokePool15SourceCandidateAdmission(
                        1, 1, 1, 1, 1, 1, true, true, true, true)
                    && !InvokePool15SourceCandidateAdmission(
                        1, 1, 1, 1, 0, 1, true, true, true, false),
                "REV06_TRAY_OR_UNLIT_SOURCE_ACCEPTED");

            C1FormalItemSessionSnapshot current =
                CreateDynamicPresentationSnapshot(1, 1, "rev06-current");
            C1FormalItemSessionSnapshot stale =
                CreateDynamicPresentationSnapshot(1, 1, "rev06-stale");
            C1FormalItemRosterEntrySnapshot roster = current.roster.Single();
            C1Pool15FormalItemSourceAnchorRequest request = new(
                roster.itemInstanceId,
                current.productContext,
                current.sessionToken,
                current.resetGeneration,
                current.canonicalSignature,
                current.arrangementCanonicalSignature,
                current.itemSystemCanonicalSignature,
                roster.instanceCanonicalSignature,
                "rev06.artwork",
                "rev06.family",
                "rev06.style",
                "rev06.cue",
                "rev06.catalog",
                "rev06.row");
            Require(request.IsStructurallyValid
                    && request.MatchesSnapshot(current)
                    && !request.MatchesSnapshot(stale),
                "REV06_STALE_SOURCE_REQUEST_ACCEPTED");
        }

        public static void RunRev04DeterministicOrThrow()
        {
            assertionCount = 0;
            VerifyRuntimeIsolation();
            VerifyRotationAdapter();
            VerifyGenericFootprintMapping();
            VerifyRev04MachineCandidateMatrix();
            VerifyRev04FTrayCandidateMatrix();
            VerifyRev04ArtworkOccupiedBounds();
            VerifyRev04FormalRoundTripTransactions();
            VerifyRev04GrabLifecycle();
            VerifyRev04RotateZoneStateMachine();
            VerifyRev04RotateZonePrefabRaycastState();
            VerifyRev04BInputIntentSeparation();
            VerifyRev04ERealTrayScrollRangeArbitration();
            VerifyPool15SourceCandidateAdmissionMatrix();
            VerifyDynamicPresentationAdmission();
        }

        public static void RunAllOrThrow()
        {
            assertionCount = 0;
            VerifyRuntimeIsolation();
            RunRev04DeterministicOrThrow();
            GameObject boardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                BoardPrefabPath);
            GameObject trayAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                TrayPrefabPath);
            GameObject cardAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                CardPrefabPath);
            Require(boardAsset != null, "BOARD_PREFAB_MISSING_FOR_FOCUSED_TEST");
            Require(trayAsset != null, "TRAY_PREFAB_MISSING_FOR_FOCUSED_TEST");
            Require(cardAsset != null, "CARD_PREFAB_MISSING_FOR_FOCUSED_TEST");

            GameObject boardObject = Object.Instantiate(boardAsset);
            GameObject trayObject = Object.Instantiate(trayAsset);
            GameObject presenterObject = new("C1ExactPresenter_FocusedTest");
            try
            {
                C1ExactBattleSandboxItemBoardView board =
                    boardObject.GetComponent<C1ExactBattleSandboxItemBoardView>();
                C1ExactBattleSandboxItemTrayView tray =
                    trayObject.GetComponent<C1ExactBattleSandboxItemTrayView>();
                C1ExactBattleSandboxItemArrangementPresenter presenter =
                    presenterObject.AddComponent<
                        C1ExactBattleSandboxItemArrangementPresenter>();
                Require(board != null && board.ValidateAuthoredReferences(),
                    "BOARD_AUTHORED_REFS_INVALID");
                Require(tray != null && tray.ValidateAuthoredReferences(),
                    "TRAY_AUTHORED_REFS_INVALID");
                Require(TryGetAuthoredCellWorldVectors(
                        board,
                        out Vector3 boardWorldXAxis,
                        out Vector3 boardWorldYAxis),
                    "AUTHORED_BOARD_CELL_WORLD_BASIS_INVALID");
                RectTransform footprintParent = GetFootprintParent(tray);
                Require(footprintParent != null,
                    "AUTHORED_TRAY_FOOTPRINT_PARENT_MISSING");
                Vector3 projectedXAxis3 = footprintParent.InverseTransformVector(
                    boardWorldXAxis);
                Vector3 projectedYAxis3 = footprintParent.InverseTransformVector(
                    boardWorldYAxis);
                Vector2 projectedXAxis = new(
                    projectedXAxis3.x,
                    projectedXAxis3.y);
                Vector2 projectedYAxis = new(
                    projectedYAxis3.x,
                    projectedYAxis3.y);
                Require(projectedXAxis.sqrMagnitude > 0.000001f
                        && projectedYAxis.sqrMagnitude > 0.000001f,
                    "BOARD_TO_TRAY_CELL_BASIS_COLLAPSED");
                VerifyDynamicBoardTrayBindings(
                    board,
                    tray,
                    projectedXAxis,
                    projectedYAxis);
                presenter.AssignForEditor(board, tray);
                Require(presenter.ValidateAuthoredReferences(),
                    "PRESENTER_AUTHORED_REFS_INVALID");

                C1FormalItemSessionCreationResult creation =
                    C1FormalItemSessionAuthority.Create(
                        "c1exact-focused-session",
                        1);
                Require(creation != null && creation.isSuccess,
                    "FORMAL_AUTHORITY_CREATE_FAILED " + creation?.diagnosticCode);
                C1FormalItemSessionAuthority authority = creation.authority;
                VerifyAuthoritativeCatalogFootprints(
                    authority.Current,
                    projectedXAxis,
                    projectedYAxis);
                Require(presenter.Bind(
                        authority,
                        out string bindDiagnostic),
                    "SNAPSHOT_BIND_FAILED " + bindDiagnostic);
                Equal(authority.Current.canonicalSignature,
                    board.BoundSnapshotSignature,
                    "board snapshot bind");
                Equal(authority.Current.canonicalSignature,
                    tray.BoundSnapshotSignature,
                    "tray snapshot bind");
                Equal(2, board.VisiblePlacedArtworkCount,
                    "initial formal placements visible");

                int boardChildCount = CountTransforms(boardObject.transform);
                int trayChildCount = CountTransforms(trayObject.transform);
                int persistentListeners = CountPersistentButtonListeners(trayObject);
                Require(presenter.Bind(authority, out bindDiagnostic),
                    "IDEMPOTENT_REBIND_FAILED " + bindDiagnostic);
                Require(presenter.Bind(authority, out bindDiagnostic),
                    "SECOND_IDEMPOTENT_REBIND_FAILED " + bindDiagnostic);
                Equal(boardChildCount, CountTransforms(boardObject.transform),
                    "rebind board hierarchy unchanged");
                Equal(trayChildCount, CountTransforms(trayObject.transform),
                    "rebind tray hierarchy unchanged");
                Equal(persistentListeners, CountPersistentButtonListeners(trayObject),
                    "rebind listener uniqueness");
                C1FormalItemInteractionAuthorizationResult enabledAuthorization =
                    presenter.ApplyInteractionAuthorization(
                        C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                            authority.Current,
                            C1FormalItemInteractionMode.PREPARE_ENABLED));
                Require(enabledAuthorization.accepted
                        && enabledAuthorization.interactionEnabled,
                    "REV04B_PREPARE_AUTHORIZATION_FAILED "
                    + enabledAuthorization.diagnostic);

                C1ExactBattleSandboxItemCardView[] cardViews =
                    trayObject.GetComponentsInChildren<
                        C1ExactBattleSandboxItemCardView>(true);
                Equal(tray.PresentationPoolCount,
                    cardViews.Length,
                    "one bounded reusable dynamic card pool");
                Equal(tray.PresentationPoolCount,
                    cardViews.Distinct().Count(),
                    "card refs unique");
                Require(tray.AuthoredCardCapacity > 0
                        && tray.AuthoredCardCapacity
                           <= C1ExactBattleSandboxItemTrayView
                               .PhysicalPresentationCapacity,
                    "Tray reusable authored template capacity");
                RectTransform[] authoredBoardArtworkRects =
                    ReadExactPrivateField<RectTransform[]>(
                        board,
                        "placedArtworkRects");
                Image[] authoredBoardArtworkImages =
                    ReadExactPrivateField<Image[]>(
                        board,
                        "placedArtworkImages");
                Equal(board.AuthoredArtworkCapacity,
                    authoredBoardArtworkRects.Length,
                    "Board authored Rect template capacity");
                Equal(board.AuthoredArtworkCapacity,
                    authoredBoardArtworkImages.Length,
                    "Board authored Image template capacity");
                Equal(board.AuthoredArtworkCapacity,
                    authoredBoardArtworkRects.Distinct().Count(),
                    "Board authored Rect refs unique");
                Equal(board.AuthoredArtworkCapacity,
                    authoredBoardArtworkImages.Distinct().Count(),
                    "Board authored Image refs unique");
                VerifyPool15ExactArtworkBindings(board, cardViews[0]);
                Equal(65, tray.AuthoredCellCount,
                    "REV04F exact authored Tray cell count");
                RectTransform[] authoredTrayCells =
                    ReadExactPrivateField<RectTransform[]>(tray, "trayCellRects");
                Image[] authoredTrayImages =
                    ReadExactPrivateField<Image[]>(tray, "trayCellImages");
                Equal(65, authoredTrayCells.Length,
                    "REV04F serialized Tray Rect refs");
                Equal(65, authoredTrayImages.Length,
                    "REV04F serialized Tray Image refs");
                Equal(65, authoredTrayCells.Distinct().Count(),
                    "REV04F serialized Tray Rect refs unique");
                Equal(65, authoredTrayImages.Distinct().Count(),
                    "REV04F serialized Tray Image refs unique");
                for (int index = 0; index < 65; index++)
                {
                    Equal("TrayGridSlot_" + (index + 1).ToString("00"),
                        authoredTrayCells[index].name,
                        "REV04F authored Tray row-major name " + index);
                    Require(authoredTrayImages[index] != null
                            && authoredTrayImages[index].rectTransform
                               == authoredTrayCells[index],
                        "REV04F_TRAY_IMAGE_RECT_REF_MISMATCH " + index);
                }

                string i001 = CanonicalInitialItemAcquisitionPolicy.ItemInstanceId;
                C1FormalItemPlacementSnapshot initialPlacement =
                    authority.Current.FindPlacementByInstanceId(i001);
                Require(initialPlacement != null, "I001_INITIAL_PLACEMENT_MISSING");
                C1FormalItemRosterEntrySnapshot initialRoster =
                    authority.Current.FindRosterEntry(i001);
                Require(initialRoster != null,
                    "REV04B_I001_ROSTER_ENTRY_MISSING");
                Require(C1Pool15FormalItemArtworkAndSourceAnchorBinding
                        .TryCreateCurrentSourceAnchorRequest(
                            authority.Current,
                            board.PresentationCatalogSnapshot,
                            i001,
                            out C1Pool15FormalItemSourceAnchorRequest
                                initialSourceRequest),
                    "POOL15_INITIAL_SOURCE_REQUEST_FAILED");
                Require(presenter.TryResolveCurrentPlacedLitSourceAnchor(
                            initialSourceRequest,
                            out C1Pool15FormalItemSourceAnchorBinding
                                initialSourceBinding)
                        && initialSourceBinding != null
                        && initialSourceBinding.sourceAnchor != null
                        && initialSourceBinding.artwork != null
                        && initialSourceBinding.isLit
                        && initialSourceBinding.Matches(initialSourceRequest),
                    "POOL15_INITIAL_PLACED_LIT_SOURCE_ANCHOR_FAILED");
                Require(!C1Pool15FormalItemArtworkAndSourceAnchorBinding
                        .TryCreateCurrentSourceAnchorRequest(
                            authority.Current,
                            board.PresentationCatalogSnapshot,
                            I031InventoryPlacementContract.SpecialIdentityId,
                            out _),
                    "I031_MUST_NOT_BECOME_POOL15_ORDINARY_SOURCE");
                ItemInnerDataDefinition initialDefinition =
                    ItemInnerDataCatalog.FindById(initialRoster.baseItemId);
                Require(initialDefinition?.ShapeCells != null
                        && initialDefinition.ShapeCells.Count > 0,
                    "REV04B_I001_AUTHORITATIVE_FOOTPRINT_MISSING");
                Vector2 oneCellTraySize = new(
                    Mathf.Abs(projectedXAxis.x) + Mathf.Abs(projectedYAxis.x),
                    Mathf.Abs(projectedXAxis.y) + Mathf.Abs(projectedYAxis.y));

                int detailChangeCount = 0;
                presenter.FormalItemDetailChanged += _ => detailChangeCount++;
                presenter.SelectBoardItem(i001);
                Require(presenter.CurrentFormalItemDetail.isOpen
                        && presenter.CurrentFormalItemDetail.projection != null
                        && string.Equals(
                            i001,
                            presenter.CurrentFormalItemDetail.projection
                                .itemInstanceId,
                            StringComparison.Ordinal),
                    "REV04B_EXPLICIT_BOARD_CLICK_DETAIL_OPEN_FAILED");
                C1FormalItemDetailSelectionResult explicitClose =
                    presenter.CloseFormalItemDetail(
                        C1FormalItemDetailSelectionRequest.Close(
                            authority.Current));
                Require(explicitClose.accepted
                        && !explicitClose.isOpen
                        && string.Equals(
                            i001,
                            presenter.SelectedItemInstanceId,
                            StringComparison.Ordinal),
                    "REV04B_EXPLICIT_CLOSE_SELECTION_PRECONDITION_FAILED");
                presenter.SelectBoardItem(i001);
                Require(presenter.CurrentFormalItemDetail.isOpen
                        && string.Equals(
                            i001,
                            presenter.CurrentFormalItemDetail.projection
                                .itemInstanceId,
                            StringComparison.Ordinal),
                    "REV04B_EXPLICIT_CLOSE_ONE_CLICK_REOPEN_FAILED");
                presenter.SelectBoardItem(i001);
                Require(!presenter.CurrentFormalItemDetail.isOpen,
                    "REV04B_EXPLICIT_BOARD_CLICK_DETAIL_CLOSE_FAILED");
                int detailEventsBeforeMovement = detailChangeCount;

                Vector2Int boardRawGrabbedCell = initialDefinition.ShapeCells[0];
                SetExactPrivateField(
                    board,
                    "pressedItemInstanceId",
                    i001);
                SetExactPrivateField(
                    board,
                    "pressedRawGrabbedCell",
                    boardRawGrabbedCell);
                SetExactPrivateField(
                    board,
                    "hasPressedRawGrabbedCell",
                    true);
                InvokeExactPrivateVoid(
                    board,
                    "CapturePressedIdentityForActiveBoardDrag",
                    Type.EmptyTypes,
                    Array.Empty<object>());
                presenter.BeginBoardDrag(i001, null);
                Require(presenter.HasActiveInteraction
                        && string.Equals(
                            i001,
                            presenter.SelectedItemInstanceId,
                            StringComparison.Ordinal),
                    "REV04B_BOARD_DRAG_DID_NOT_CAPTURE_STABLE_INSTANCE");
                Require(!presenter.CurrentFormalItemDetail.isOpen
                        && detailChangeCount == detailEventsBeforeMovement,
                    "REV04B_BOARD_DRAG_EMITTED_DETAIL_OPEN_INTENT");
                string unchangedSessionToken = authority.Current.sessionToken;
                long unchangedResetGeneration =
                    authority.Current.resetGeneration;
                Require(presenter.SubmitReturnToTray(
                        i001,
                        out string reentryReturnDiagnostic),
                    "REV04B_BOARD_TO_TRAY_RETURN_FAILED "
                    + reentryReturnDiagnostic);
                Equal(authority.Current.canonicalSignature,
                    board.BoundSnapshotSignature,
                    "REV04B Board publication after return");
                Equal(authority.Current.canonicalSignature,
                    tray.BoundSnapshotSignature,
                    "REV04B Tray publication after return");
                Require(!presenter.TryResolveCurrentPlacedLitSourceAnchor(
                        initialSourceRequest,
                        out _),
                    "POOL15_STALE_PRE_RETURN_SOURCE_MUST_REJECT");
                Require(C1Pool15FormalItemArtworkAndSourceAnchorBinding
                        .TryCreateCurrentSourceAnchorRequest(
                            authority.Current,
                            board.PresentationCatalogSnapshot,
                            i001,
                            out C1Pool15FormalItemSourceAnchorRequest
                                traySourceRequest)
                        && !presenter.TryResolveCurrentPlacedLitSourceAnchor(
                            traySourceRequest,
                            out _),
                    "POOL15_TRAY_SOURCE_MUST_REJECT");

                C1ExactBattleSandboxItemCardView reentryCard =
                    FindBoundCard(tray, i001);
                Require(reentryCard != null
                        && reentryCard.gameObject.activeSelf
                        && reentryCard.gameObject.activeInHierarchy,
                    "REV04B_RETURNED_CARD_NOT_ACTIVE");
                ScrollRect liveTrayScrollRect = tray.AuthoredScrollRect;
                RectTransform liveTrayContent = liveTrayScrollRect.content;
                RectTransform liveTrayViewport = liveTrayScrollRect.viewport;
                GridLayoutGroup liveTrayGrid =
                    liveTrayContent.GetComponent<GridLayoutGroup>();
                Require(liveTrayGrid != null
                        && liveTrayGrid.constraint
                           == GridLayoutGroup.Constraint.FixedColumnCount,
                    "REV04F_LIVE_TRAY_FIXED_COLUMN_GRID_MISSING");
                Equal(C1FormalItemSessionContract.TrayColumnCount,
                    liveTrayGrid.constraintCount,
                    "REV04F live Tray column count");
                int liveTrayRowCount = Mathf.CeilToInt(
                    authoredTrayCells.Length
                    / (float)liveTrayGrid.constraintCount);
                Equal(C1FormalItemSessionContract.TrayRowCount,
                    liveTrayRowCount,
                    "REV04F live Tray row count");
                float expectedLiveContentHeight =
                    liveTrayGrid.padding.top
                    + liveTrayGrid.padding.bottom
                    + liveTrayRowCount * liveTrayGrid.cellSize.y
                    + (liveTrayRowCount - 1) * liveTrayGrid.spacing.y;
                Require(Mathf.Abs(
                            liveTrayContent.rect.height
                            - expectedLiveContentHeight) <= 0.01f,
                    "REV04F_LIVE_TRAY_CONTENT_HEIGHT_NOT_GRID_DERIVED");
                Bounds liveTrayContentBounds =
                    RectTransformUtility.CalculateRelativeRectTransformBounds(
                        liveTrayViewport,
                        liveTrayContent);
                Require(liveTrayContentBounds.size.y
                        > liveTrayViewport.rect.height + 0.5f,
                    "REV04F_LIVE_TRAY_GEOMETRY_HAS_NO_POSITIVE_OVERFLOW");
                Require(InvokeLiveVerticalScrollRange(reentryCard),
                    "REV04F_EXACT_65_CELL_TRAY_MUST_HAVE_SCROLL_RANGE");
                Equal(1,
                    cardViews.Count(card => card != null
                                            && card.HasBoundItem
                                            && string.Equals(
                                                card.ItemInstanceId,
                                                i001,
                                                StringComparison.Ordinal)),
                    "REV04B_RETURNED_CARD_UNIQUE_BINDING");
                Equal(i001, reentryCard.ItemInstanceId,
                    "REV04B returned stable instance");
                Equal(initialRoster.baseItemId, reentryCard.BaseItemId,
                    "REV04B returned authoritative base identity");
                CanvasGroup reentryCanvasGroup =
                    ReadExactPrivateField<CanvasGroup>(
                        reentryCard,
                        "canvasGroup");
                Require(reentryCanvasGroup != null
                        && reentryCanvasGroup.interactable
                        && reentryCanvasGroup.blocksRaycasts,
                    "REV04B_RETURNED_CARD_INPUT_NOT_ENABLED");
                C1ExactBattleSandboxItemFootprint reentryFootprint =
                    ReadExactPrivateField<
                        C1ExactBattleSandboxItemFootprint>(
                        reentryCard,
                        "currentFootprint");
                Require(reentryFootprint != null
                        && reentryFootprint.NormalizedCells.Count
                           == initialDefinition.ShapeCells.Count,
                    "REV04B_RETURNED_CARD_FOOTPRINT_CHANGED");
                Approximately(oneCellTraySize,
                    GetCurrentFootprintSize(reentryCard),
                    "REV04B returned I001 one-cell footprint");
                Require(TryResolveFootprintRawCell(
                        reentryFootprint,
                        Vector2Int.zero,
                        out Vector2Int cardRawGrabbedCell)
                        && initialDefinition.ShapeCells.Contains(
                            cardRawGrabbedCell),
                    "REV04B_RETURNED_CARD_RAW_GRABBED_CELL_INVALID");
                SetExactPrivateField(
                    reentryCard,
                    "pressedRawGrabbedCell",
                    cardRawGrabbedCell);
                SetExactPrivateField(
                    reentryCard,
                    "hasPressedRawGrabbedCell",
                    true);
                Require(TryGetPressedRawGrabbedCell(
                            reentryCard,
                            out Vector2Int capturedCardRawCell)
                        && capturedCardRawCell == cardRawGrabbedCell,
                    "REV04B_CARD_PRESSED_RAW_IDENTITY_NOT_AVAILABLE");
                presenter.BeginCardDrag(reentryCard, null);
                Require(presenter.HasActiveInteraction,
                    "REV04B_RETURNED_CARD_DRAG_DID_NOT_BEGIN");
                Require(!presenter.CurrentFormalItemDetail.isOpen
                        && detailChangeCount == detailEventsBeforeMovement,
                    "REV04B_CARD_DRAG_EMITTED_DETAIL_OPEN_INTENT");
                Require(InvokeExactPrivateBool(
                        presenter,
                        "BuildAndShowCandidate",
                        new[] { typeof(Vector2Int) },
                        new object[] { initialPlacement.anchorCell }),
                    "REV04B_RETURNED_CARD_CANDIDATE_BUILD_FAILED");
                Require(presenter.CommitPreviewedCandidate(
                        out string reentryPlaceDiagnostic),
                    "REV04B_RETURNED_CARD_PLACE_FROM_TRAY_FAILED "
                    + reentryPlaceDiagnostic);
                Require(presenter.LastPreviewCommitCandidateConsistent,
                    "REV04B_REENTRY_PREVIEW_COMMIT_CANDIDATE_SPLIT");
                C1FormalItemPlacementSnapshot reenteredPlacement =
                    authority.Current.FindPlacementByInstanceId(i001);
                Require(reenteredPlacement != null,
                    "REV04B_REENTRY_PLACEMENT_MISSING");
                Equal(initialPlacement.anchorCell,
                    reenteredPlacement.anchorCell,
                    "REV04B reentry authoritative anchor");
                Equal(initialPlacement.rotation,
                    reenteredPlacement.rotation,
                    "REV04B reentry authoritative rotation");
                Equal(unchangedSessionToken,
                    authority.Current.sessionToken,
                    "REV04B unchanged session token");
                Equal(unchangedResetGeneration,
                    authority.Current.resetGeneration,
                    "REV04B unchanged reset generation");
                Require(ReferenceEquals(
                        enabledAuthorization,
                        presenter.CurrentInteractionAuthorization),
                    "REV04B_REENTRY_REQUESTED_NEW_AUTHORIZATION");
                Equal(detailEventsBeforeMovement,
                    detailChangeCount,
                    "REV04B movement detail event count");
                Require(C1Pool15FormalItemArtworkAndSourceAnchorBinding
                        .TryCreateCurrentSourceAnchorRequest(
                            authority.Current,
                            board.PresentationCatalogSnapshot,
                            i001,
                            out C1Pool15FormalItemSourceAnchorRequest
                                reenteredSourceRequest,
                            out string reenteredSourceRejectCondition)
                        && string.IsNullOrEmpty(
                            reenteredSourceRejectCondition)
                        && presenter.TryResolveCurrentPlacedLitSourceAnchor(
                            reenteredSourceRequest,
                            out C1Pool15FormalItemSourceAnchorBinding
                                reenteredSourceBinding)
                        && reenteredSourceBinding != null
                        && reenteredSourceBinding.isLit
                        && reenteredSourceBinding.anchorCell
                           == reenteredPlacement.anchorCell
                        && reenteredSourceBinding.rotation
                           == reenteredPlacement.rotation
                        && reenteredSourceBinding.Matches(
                            reenteredSourceRequest),
                    "POOL15_REENTRY_SOURCE_ANCHOR_REFRESH_FAILED");

                Vector2Int legalUnlitCell =
                    FindValidCandidateByCurrentLighting(
                        presenter,
                        authority.Current,
                        i001,
                        initialPlacement.anchorCell,
                        0,
                        false);
                Require(presenter.PreviewCandidate(
                        i001,
                        legalUnlitCell,
                        0,
                        out bool legalUnlitCandidate)
                    && legalUnlitCandidate,
                    "POOL15_LEGAL_UNLIT_PREVIEW_FAILED");
                Require(presenter.CommitPreviewedCandidate(
                        out string legalUnlitDiagnostic),
                    "POOL15_LEGAL_UNLIT_COMMIT_FAILED "
                    + legalUnlitDiagnostic);
                Require(presenter.LastPreviewCommitCandidateConsistent,
                    "POOL15_LEGAL_UNLIT_PREVIEW_COMMIT_CANDIDATE_SPLIT");
                C1FormalItemPlacementSnapshot legalUnlitPlacement =
                    authority.Current.FindPlacementByInstanceId(i001);
                Require(legalUnlitPlacement != null,
                    "POOL15_LEGAL_UNLIT_FORMAL_PLACEMENT_MISSING");
                Equal(legalUnlitCell,
                    legalUnlitPlacement.anchorCell,
                    "POOL15 legal-unlit formal anchor");
                Equal(0,
                    legalUnlitPlacement.rotation,
                    "POOL15 legal-unlit formal rotation");
                ItemSystemPlacementSnapshot legalUnlitItemSystemPlacement =
                    authority.Current.itemSystemSnapshot.FindPlacement(
                        legalUnlitPlacement.placementId);
                Require(legalUnlitItemSystemPlacement != null,
                    "POOL15_LEGAL_UNLIT_ITEM_SYSTEM_PLACEMENT_MISSING");
                Equal(legalUnlitPlacement.placementId,
                    legalUnlitItemSystemPlacement.placementId,
                    "POOL15 legal-unlit exact placement identity");
                Equal(initialRoster.baseItemId,
                    legalUnlitItemSystemPlacement.itemId,
                    "POOL15 legal-unlit exact base identity");
                Equal(legalUnlitCell,
                    legalUnlitItemSystemPlacement.anchorCell,
                    "POOL15 legal-unlit Item-system anchor");
                Equal(0,
                    legalUnlitItemSystemPlacement.rotation,
                    "POOL15 legal-unlit Item-system rotation");
                Require(!legalUnlitItemSystemPlacement.isLit,
                    "POOL15_LEGAL_UNLIT_ITEM_SYSTEM_FACT_MUST_BE_UNLIT");
                C1Pool15FormalItemSourceAnchorRequest legalUnlitSourceRequest;
                Require(C1Pool15FormalItemArtworkAndSourceAnchorBinding
                        .TryCreateCurrentSourceAnchorRequest(
                            authority.Current,
                            board.PresentationCatalogSnapshot,
                            i001,
                            out legalUnlitSourceRequest),
                    "POOL15_LEGAL_UNLIT_SOURCE_REQUEST_CREATION_FAILED");
                Require(!presenter.TryResolveCurrentPlacedLitSourceAnchor(
                        legalUnlitSourceRequest,
                        out _),
                    "POOL15_CURRENT_LEGAL_UNLIT_SOURCE_MUST_REJECT");
                Require(!presenter.TryResolveCurrentPlacedLitSourceAnchor(
                        reenteredSourceRequest,
                        out _),
                    "POOL15_STALE_PRE_UNLIT_MOVE_SOURCE_MUST_REJECT");

                Vector2Int validMoveCell =
                    FindValidCandidateByCurrentLighting(
                        presenter,
                        authority.Current,
                        i001,
                        legalUnlitCell,
                        1,
                        true);
                string beforeMove = authority.Current.canonicalSignature;
                Require(presenter.PreviewCandidate(
                        i001,
                        validMoveCell,
                        1,
                        out bool validMove)
                    && validMove,
                    "VALID_MOVE_PREVIEW_FAILED");
                Require(presenter.CommitPreviewedCandidate(
                        out string moveDiagnostic),
                    "VALID_MOVE_COMMIT_FAILED " + moveDiagnostic);
                Require(presenter.LastPreviewCommitCandidateConsistent,
                    "PREVIEW_COMMIT_MUST_SHARE_ONE_CANDIDATE");
                Require(!string.Equals(
                        beforeMove,
                        authority.Current.canonicalSignature,
                        StringComparison.Ordinal),
                    "VALID_MOVE_MUST_CHANGE_FORMAL_SNAPSHOT");
                C1FormalItemPlacementSnapshot moved =
                    authority.Current.FindPlacementByInstanceId(i001);
                Require(moved != null,
                    "POOL15_MOVE_FORMAL_PLACEMENT_MISSING");
                Equal(validMoveCell, moved.anchorCell, "formal move anchor");
                Equal(270, moved.rotation,
                    "first right rotation commits formal 270 degrees");
                ItemSystemPlacementSnapshot movedItemSystemPlacement =
                    authority.Current.itemSystemSnapshot.FindPlacement(
                        moved.placementId);
                Require(movedItemSystemPlacement != null,
                    "POOL15_MOVE_ITEM_SYSTEM_PLACEMENT_MISSING");
                Equal(moved.placementId,
                    movedItemSystemPlacement.placementId,
                    "POOL15 move exact placement identity");
                Equal(initialRoster.baseItemId,
                    movedItemSystemPlacement.itemId,
                    "POOL15 move exact base identity");
                Equal(validMoveCell,
                    movedItemSystemPlacement.anchorCell,
                    "POOL15 move Item-system anchor");
                Equal(270,
                    movedItemSystemPlacement.rotation,
                    "POOL15 move Item-system formal rotation");
                Require(movedItemSystemPlacement.isLit,
                    "POOL15_MOVE_ITEM_SYSTEM_FACT_MUST_BE_LIT");
                Require(!presenter.TryResolveCurrentPlacedLitSourceAnchor(
                        legalUnlitSourceRequest,
                        out _),
                    "POOL15_STALE_PRE_MOVE_SOURCE_MUST_REJECT");
                C1Pool15FormalItemSourceAnchorRequest movedSourceRequest;
                Require(C1Pool15FormalItemArtworkAndSourceAnchorBinding
                        .TryCreateCurrentSourceAnchorRequest(
                            authority.Current,
                            board.PresentationCatalogSnapshot,
                            i001,
                            out movedSourceRequest),
                    "POOL15_MOVE_SOURCE_REQUEST_CREATION_FAILED");
                C1Pool15FormalItemSourceAnchorBinding movedSourceBinding;
                Require(presenter.TryResolveCurrentPlacedLitSourceAnchor(
                        movedSourceRequest,
                        out movedSourceBinding),
                    "POOL15_MOVE_SOURCE_RESOLUTION_FAILED");
                Require(movedSourceBinding != null,
                    "POOL15_MOVE_SOURCE_BINDING_MISSING");
                Equal(validMoveCell,
                    movedSourceBinding.anchorCell,
                    "POOL15 move source binding anchor cell");
                Equal(270,
                    movedSourceBinding.rotation,
                    "POOL15 move source binding formal rotation");
                Require(movedSourceBinding.isLit,
                    "POOL15_MOVE_SOURCE_BINDING_MUST_BE_LIT");
                Require(movedSourceBinding.sourceAnchor != null,
                    "POOL15_MOVE_AUTHORED_SOURCE_ANCHOR_MISSING");
                Require(movedSourceBinding.Matches(movedSourceRequest),
                    "POOL15_MOVE_SOURCE_BINDING_REQUEST_MISMATCH");

                C1FormalItemPlacementSnapshot i031 =
                    authority.Current.FindPlacementByInstanceId(
                        I031InventoryPlacementContract.SpecialIdentityId);
                Require(i031 != null, "I031_PLACEMENT_MISSING");
                string beforeInvalid = authority.Current.canonicalSignature;
                Require(presenter.PreviewCandidate(
                        i001,
                        i031.anchorCell,
                        0,
                        out bool invalidCandidate),
                    "INVALID_PREVIEW_BUILD_FAILED");
                Require(!invalidCandidate, "OVERLAP_PREVIEW_MUST_BE_INVALID");
                Require(!presenter.CommitPreviewedCandidate(
                        out string invalidDiagnostic),
                    "INVALID_PREVIEW_MUST_NOT_COMMIT");
                Equal(C1ExactBattleSandboxItemArrangementPresenter
                        .CandidateInvalidDiagnostic,
                    invalidDiagnostic,
                    "invalid preview diagnostic");
                Equal(beforeInvalid, authority.Current.canonicalSignature,
                    "invalid preview restores exact snapshot");
                Equal(authority.Current.canonicalSignature,
                    board.BoundSnapshotSignature,
                    "invalid preview board restoration");

                Vector2Int staleCandidateCell = FindValidCandidate(
                    presenter,
                    i001,
                    moved.anchorCell,
                    2);
                Require(presenter.PreviewCandidate(
                        i001,
                        staleCandidateCell,
                        2,
                        out bool staleLocallyValid)
                    && staleLocallyValid,
                    "STALE_PREVIEW_SETUP_FAILED");
                C1FormalItemSessionOperationResult externalReturn = authority.Submit(
                    new C1FormalItemArrangementCommand(
                        C1FormalItemArrangementCommandKind.ReturnToTray,
                        authority.Current.sessionToken,
                        authority.Current.resetGeneration,
                        "c1exact-focused.external-return",
                        i001,
                        authority.Current.canonicalSignature,
                        null));
                Require(externalReturn.accepted,
                    "EXTERNAL_FORMAL_COMMAND_SETUP_FAILED "
                    + externalReturn.diagnosticCode);
                string afterExternal = authority.Current.canonicalSignature;
                Require(!presenter.TryResolveCurrentPlacedLitSourceAnchor(
                        movedSourceRequest,
                        out _),
                    "POOL15_STALE_EXTERNAL_RETURN_SOURCE_MUST_REJECT");
                Require(C1Pool15FormalItemArtworkAndSourceAnchorBinding
                        .TryCreateCurrentSourceAnchorRequest(
                            authority.Current,
                            board.PresentationCatalogSnapshot,
                            i001,
                            out C1Pool15FormalItemSourceAnchorRequest
                                externalTraySourceRequest)
                        && !presenter.TryResolveCurrentPlacedLitSourceAnchor(
                            externalTraySourceRequest,
                            out _),
                    "POOL15_EXTERNAL_TRAY_SOURCE_MUST_REJECT");
                Require(!presenter.CommitPreviewedCandidate(
                        out string staleDiagnostic),
                    "STALE_PREVIEW_MUST_REJECT");
                Equal(C1FormalItemSessionDiagnosticCodes
                        .ExpectedSessionSignatureStale,
                    staleDiagnostic,
                    "stale preview diagnostic");
                Equal(afterExternal, authority.Current.canonicalSignature,
                    "stale rejection preserves external formal truth");
                Require(presenter.LastPreviewCommitCandidateConsistent,
                    "stale rejection still submits the exact preview candidate");
                Equal(1, tray.VisibleCardCount,
                    "formal return rebuilds one tray card");
                C1ExactBattleSandboxItemCardView returnedI001 =
                    FindBoundCard(tray, i001);
                Require(returnedI001 != null,
                    "I001_RETURNED_TRAY_CARD_MISSING");
                Approximately(oneCellTraySize,
                    GetCurrentFootprintSize(returnedI001),
                    "I001 one authored Board cell in Tray space");
                Require(GetArtworkPreservesAspect(returnedI001),
                    "I001_ARTWORK_ASPECT_MUST_BE_PRESERVED");
                Require(ValidateLiveFootprintAlignment(returnedI001),
                    "I001_INPUT_OVERLAY_ARTWORK_ALIGNMENT_INVALID");
                Require(!GetSourceFootprintLayerActive(returnedI001),
                    "LEGACY_SOURCE_FOOTPRINT_LAYER_MUST_NOT_OWN_RUNTIME_SIZE");
                Equal(0, GetSourceFootprintRaycastCount(returnedI001),
                    "legacy source footprint raycasts disabled");

                Vector2Int rotatedTrayCandidate = FindValidCandidate(
                    presenter,
                    i001,
                    new Vector2Int(-1, -1),
                    1);
                Require(presenter.PreviewCandidate(
                        i001,
                        rotatedTrayCandidate,
                        1,
                        out bool rotatedTrayValid)
                    && rotatedTrayValid,
                    "I001_ROTATED_TRAY_PREVIEW_FAILED");
                returnedI001 = FindBoundCard(tray, i001);
                Require(returnedI001 != null && returnedI001.Rotation == 1,
                    "TRAY_CARD_VISUAL_ROTATION_NOT_HYDRATED");
                Approximately(oneCellTraySize,
                    GetCurrentFootprintSize(returnedI001),
                    "rotated I001 remains one transformed cell");
                Require(ValidateLiveFootprintAlignment(returnedI001),
                    "ROTATED_I001_ARTWORK_OVERLAY_ALIGNMENT_INVALID");
                presenter.CancelInteraction();
                returnedI001 = FindBoundCard(tray, i001);
                Require(returnedI001 != null && returnedI001.Rotation == 0,
                    "CANCEL_MUST_RESTORE_TRAY_CARD_ROTATION");
                Approximately(oneCellTraySize,
                    GetCurrentFootprintSize(returnedI001),
                    "cancel restores exact tray footprint");

                C1FormalItemTrayPlacementSnapshot trayBeforeMove = authority.Current
                    .trayLayout.FindPlacementByInstanceId(i001);
                Require(trayBeforeMove != null && trayBeforeMove.isActiveInTray,
                    "REV04F_TRAY_MOVE_SOURCE_MISSING");
                string beforeTrayMove = authority.Current.canonicalSignature;
                SetExactPrivateField(
                    returnedI001,
                    "pressedRawGrabbedCell",
                    initialDefinition.ShapeCells[0]);
                SetExactPrivateField(
                    returnedI001,
                    "hasPressedRawGrabbedCell",
                    true);
                presenter.BeginCardDrag(returnedI001, null);
                Require(InvokeExactPrivateBool(
                        presenter,
                        "BuildAndShowTrayCandidate",
                        new[] { typeof(Vector2Int) },
                        new object[] { new Vector2Int(4, 12) }),
                    "REV04F_TRAY_PREVIEW_BUILD_FAILED");
                InvokeExactPrivateVoid(
                    presenter,
                    "EndInteraction",
                    new[]
                    {
                        typeof(UnityEngine.EventSystems.PointerEventData)
                    },
                    new object[] { null });
                C1FormalItemTrayPlacementSnapshot trayAfterMove = authority.Current
                    .trayLayout.FindPlacementByInstanceId(i001);
                Require(trayAfterMove != null
                        && trayAfterMove.isActiveInTray
                        && trayAfterMove.anchorCell == new Vector2Int(4, 12),
                    "REV04F_TRAY_MOVE_COMMIT_MISSING");
                Require(!string.Equals(
                        beforeTrayMove,
                        authority.Current.canonicalSignature,
                        StringComparison.Ordinal),
                    "REV04F_TRAY_MOVE_SIGNATURE_UNCHANGED");
                Require(presenter.LastPreviewCommitCandidateConsistent,
                    "REV04F_TRAY_PREVIEW_COMMIT_CANDIDATE_SPLIT");
                returnedI001 = FindBoundCard(tray, i001);
                Require(returnedI001 != null,
                    "REV04F_TRAY_MOVED_CARD_NOT_REBOUND");
                Require(TryGetAuthoredTrayCellCenter(
                        tray,
                        new Vector2Int(4, 12),
                        out Vector2 authoredCenter),
                    "REV04F_TRAY_TARGET_CELL_CENTER_MISSING");
                Approximately(authoredCenter,
                    new Vector2(
                        returnedI001.transform.localPosition.x,
                        returnedI001.transform.localPosition.y),
                    "REV04F Card exact authored cell center");

                string beforeInvalidTray = authority.Current.canonicalSignature;
                SetExactPrivateField(
                    returnedI001,
                    "pressedRawGrabbedCell",
                    initialDefinition.ShapeCells[0]);
                SetExactPrivateField(
                    returnedI001,
                    "hasPressedRawGrabbedCell",
                    true);
                presenter.BeginCardDrag(returnedI001, null);
                Require(InvokeExactPrivateBool(
                        presenter,
                        "BuildAndShowTrayCandidate",
                        new[] { typeof(Vector2Int) },
                        new object[] { new Vector2Int(-1, -1) }),
                    "REV04F_INVALID_TRAY_PREVIEW_BUILD_FAILED");
                InvokeExactPrivateVoid(
                    presenter,
                    "EndInteraction",
                    new[]
                    {
                        typeof(UnityEngine.EventSystems.PointerEventData)
                    },
                    new object[] { null });
                Equal(beforeInvalidTray,
                    authority.Current.canonicalSignature,
                    "REV04F invalid Tray target restores exact snapshot");
                Equal(new Vector2Int(4, 12), authority.Current.trayLayout
                        .FindPlacementByInstanceId(i001).anchorCell,
                    "REV04F invalid Tray target restores exact anchor");

                Vector2Int placeCell = FindValidCandidate(
                    presenter,
                    i001,
                    new Vector2Int(4, 4),
                    3);
                Require(presenter.PreviewCandidate(
                        i001,
                        placeCell,
                        3,
                        out bool placeValid)
                    && placeValid,
                    "TRAY_TO_BOARD_PREVIEW_FAILED");
                string beforeCancel = authority.Current.canonicalSignature;
                presenter.CancelInteraction();
                Equal(beforeCancel, authority.Current.canonicalSignature,
                    "cancel preserves formal snapshot");
                Equal(authority.Current.canonicalSignature,
                    tray.BoundSnapshotSignature,
                    "cancel tray restoration");

                Require(presenter.PreviewCandidate(
                        i001,
                        placeCell,
                        3,
                        out placeValid)
                    && placeValid,
                    "TRAY_TO_BOARD_REPREVIEW_FAILED");
                Require(presenter.CommitPreviewedCandidate(
                        out string placeDiagnostic),
                    "TRAY_TO_BOARD_COMMIT_FAILED " + placeDiagnostic);
                Equal(90,
                    authority.Current.FindPlacementByInstanceId(i001).rotation,
                    "three visual quarter turns commit formal 90 degrees");
                Equal(0, tray.VisibleCardCount,
                    "tray-to-board clears bound tray slot");
                Equal(2, board.VisiblePlacedArtworkCount,
                    "tray-to-board rebuilds authored board artwork");
                Equal(0, CountRuntimeFallbackNames(boardObject, trayObject),
                    "no runtime fallback hierarchy");
            }
            finally
            {
                Object.DestroyImmediate(presenterObject);
                Object.DestroyImmediate(boardObject);
                Object.DestroyImmediate(trayObject);
            }
        }

        private static void VerifyDynamicPresentationAdmission()
        {
            foreach (int count in new[] { 0, 1, 7, 8, 15 })
            {
                KeyValuePair<string, string>[] duplicateBaseRows =
                    Enumerable.Range(1, count)
                        .Select(index => new KeyValuePair<string, string>(
                            "dynamic.instance." + index.ToString(
                                CultureInfo.InvariantCulture),
                            "I003"))
                        .ToArray();
                Require(InvokeExactInstanceBindingAdmission(
                        C1ExactBattleSandboxItemBoardView
                            .PhysicalPresentationCapacity,
                        duplicateBaseRows,
                        duplicateBaseRows),
                    "DYNAMIC_BOARD_ROSTER_SIZE_REJECTED " + count);
                Require(InvokeExactInstanceBindingAdmission(
                        C1ExactBattleSandboxItemTrayView
                            .PhysicalPresentationCapacity,
                        duplicateBaseRows,
                        duplicateBaseRows),
                    "DYNAMIC_TRAY_ROSTER_SIZE_REJECTED " + count);
                Equal(count,
                    duplicateBaseRows.Select(value => value.Key)
                        .Distinct(StringComparer.Ordinal).Count(),
                    "duplicate base identities preserve exact instances "
                    + count);
            }

            KeyValuePair<string, string>[] fullRoster = Enumerable.Range(1, 80)
                .Select(index => new KeyValuePair<string, string>(
                    "dynamic.full-roster." + index,
                    "I003"))
                .ToArray();
            Require(InvokeExactInstanceBindingAdmission(
                    C1ExactBattleSandboxItemBoardView
                        .PhysicalPresentationCapacity,
                    fullRoster.Take(25),
                    fullRoster),
                "FULL_ROSTER_MUST_NOT_CAP_BOARD_CONTAINER");
            Require(InvokeExactInstanceBindingAdmission(
                    C1ExactBattleSandboxItemTrayView
                        .PhysicalPresentationCapacity,
                    fullRoster.Take(65),
                    fullRoster),
                "FULL_ROSTER_MUST_NOT_CAP_TRAY_CONTAINER");
            Require(!InvokeExactInstanceBindingAdmission(
                    C1ExactBattleSandboxItemBoardView
                        .PhysicalPresentationCapacity,
                    fullRoster.Take(26),
                    fullRoster)
                    && !InvokeExactInstanceBindingAdmission(
                        C1ExactBattleSandboxItemTrayView
                            .PhysicalPresentationCapacity,
                        fullRoster.Take(66),
                        fullRoster),
                "PHYSICAL_CONTAINER_BOUNDS_MUST_FAIL_CLOSED");
            Require(!InvokeExactInstanceBindingAdmission(
                    C1ExactBattleSandboxItemBoardView
                        .PhysicalPresentationCapacity,
                    new[]
                    {
                        new KeyValuePair<string, string>("duplicate", "I003"),
                        new KeyValuePair<string, string>("duplicate", "I003")
                    },
                    fullRoster),
                "DUPLICATE_INSTANCE_MUST_FAIL_CLOSED");
            Require(!InvokeExactInstanceBindingAdmission(
                    C1ExactBattleSandboxItemBoardView
                        .PhysicalPresentationCapacity,
                    new[]
                    {
                        new KeyValuePair<string, string>(
                            "dynamic.full-roster.1",
                            "I004")
                    },
                    fullRoster),
                "BASE_IDENTITY_MISMATCH_MUST_FAIL_CLOSED");

            string boardSource = File.ReadAllText(RuntimeSourcePaths[1]);
            string traySource = File.ReadAllText(RuntimeSourcePaths[2]);
            string bindingSource = File.ReadAllText(RuntimeSourcePaths[4]);
            foreach (string source in new[] { boardSource, traySource, bindingSource })
            {
                Require(source.IndexOf("GameObject.Find", StringComparison.Ordinal) < 0
                        && source.IndexOf(
                            "Resources.LoadAll",
                            StringComparison.Ordinal) < 0,
                    "RUNTIME_FINDER_OR_DIRECTORY_PROBE_PRESENT");
            }
            Require(bindingSource.IndexOf(
                        "WhiteArtworkResourcePaths",
                        StringComparison.Ordinal) < 0
                    && bindingSource.IndexOf(
                        "TargetIds",
                        StringComparison.Ordinal) < 0,
                "FIXED_ITEM_ARTWORK_TABLE_REMAINS");
            Require(boardSource.IndexOf(
                        "private const int InitialPrewarmCount = 7",
                        StringComparison.Ordinal) >= 0
                    && traySource.IndexOf(
                        "private const int InitialPrewarmCount = 7",
                        StringComparison.Ordinal) >= 0
                    && boardSource.IndexOf(
                        "activePoolEntries.ContainsKey",
                        StringComparison.Ordinal) >= 0
                    && traySource.IndexOf(
                        "activePoolEntries.ContainsKey",
                        StringComparison.Ordinal) >= 0,
                "DYNAMIC_STABLE_POOL_REUSE_SEAM_MISSING");

            string promotionEditorSource = File.ReadAllText(
                "Assets/_Game/Scripts/TalismanBag/Editor/"
                + "ItemCampaignBaseline/"
                + "C1ExactBattleSandboxItemPromotionEditor.cs");
            string retiredExecuteMethod =
                "ExecuteRev02Growing" + "ItemBuildFromCommandLine";
            string retiredMarker =
                "C1_POOL15_GROWING_ITEM_BUILD_"
                + "REV02_AUTHORING_VALIDATION_PASS";
            Require(promotionEditorSource.IndexOf(
                        retiredExecuteMethod,
                        StringComparison.Ordinal) < 0
                    && promotionEditorSource.IndexOf(
                        retiredMarker,
                        StringComparison.Ordinal) < 0
                    && promotionEditorSource.IndexOf(
                        "ExecuteRev04DynamicItemPresentationFromCommandLine",
                        StringComparison.Ordinal) >= 0
                    && promotionEditorSource.IndexOf(
                        "C1_DYNAMIC_ITEM_PRESENTATION_REV04_"
                        + "AUTHORING_VALIDATION_PASS",
                        StringComparison.Ordinal) >= 0,
                "REV04_DYNAMIC_AUTHORING_ROUTE_NOT_EXCLUSIVE");
            Require(boardSource.IndexOf(
                        "private void Update(",
                        StringComparison.Ordinal) < 0
                    && boardSource.IndexOf(
                        "private void LateUpdate(",
                        StringComparison.Ordinal) < 0
                    && traySource.IndexOf(
                        "private void Update(",
                        StringComparison.Ordinal) < 0
                    && traySource.IndexOf(
                        "private void LateUpdate(",
                        StringComparison.Ordinal) < 0,
                "PRESENTATION_POOL_PER_FRAME_MUTATION_ROUTE_PRESENT");
        }

        private static void VerifyDynamicBoardTrayBindings(
            C1ExactBattleSandboxItemBoardView board,
            C1ExactBattleSandboxItemTrayView tray,
            Vector2 trayUnitX,
            Vector2 trayUnitY)
        {
            Require(board != null && tray != null,
                "DYNAMIC_BOUNDING_VIEW_FIXTURE_MISSING");
            foreach (int count in new[] { 0, 1, 7, 8, 15 })
            {
                VerifyDynamicDistribution(
                    board, tray, count, count, trayUnitX, trayUnitY,
                    "all-board");
                VerifyDynamicDistribution(
                    board, tray, count, 0, trayUnitX, trayUnitY,
                    "all-tray");
                VerifyDynamicDistribution(
                    board, tray, count, count / 2, trayUnitX, trayUnitY,
                    "mixed");
            }
            VerifyDynamicDistribution(
                board, tray, 25, 25, trayUnitX, trayUnitY, "board-physical");
            VerifyDynamicDistribution(
                board, tray, 65, 0, trayUnitX, trayUnitY, "tray-physical");
            VerifyDynamicDistribution(
                board, tray, 66, 1, trayUnitX, trayUnitY,
                "full-roster-larger-than-container");
        }

        private static void VerifyDynamicDistribution(
            C1ExactBattleSandboxItemBoardView board,
            C1ExactBattleSandboxItemTrayView tray,
            int rosterCount,
            int boardCount,
            Vector2 trayUnitX,
            Vector2 trayUnitY,
            string label)
        {
            C1FormalItemSessionSnapshot snapshot =
                CreateDynamicPresentationSnapshot(rosterCount, boardCount, label);
            Dictionary<string, C1ExactBattleSandboxItemFootprint> footprints =
                snapshot.roster.ToDictionary(
                    value => value.itemInstanceId,
                    value => CreateFootprint(
                        new[] { Vector2Int.zero },
                        0,
                        trayUnitX,
                        trayUnitY),
                    StringComparer.Ordinal);
            InvokeExactPrivateVoid(
                board,
                "Bind",
                new[]
                {
                    typeof(C1FormalItemSessionSnapshot),
                    typeof(string)
                },
                new object[] { snapshot, string.Empty });
            InvokeExactPrivateVoid(
                tray,
                "Bind",
                new[]
                {
                    typeof(C1FormalItemSessionSnapshot),
                    typeof(string),
                    typeof(IReadOnlyDictionary<string,
                        C1ExactBattleSandboxItemFootprint>),
                    typeof(C1FormalItemPresentationCatalogSnapshot)
                },
                new object[]
                {
                    snapshot,
                    string.Empty,
                    footprints,
                    board.PresentationCatalogSnapshot
                });
            Equal(boardCount,
                board.VisiblePlacedArtworkCount,
                "Board visible exact instances " + rosterCount + "/" + label);
            Equal(rosterCount - boardCount,
                tray.VisibleCardCount,
                "Tray visible exact instances " + rosterCount + "/" + label);
            Equal(rosterCount,
                board.VisiblePlacedArtworkCount + tray.VisibleCardCount,
                "no instance loss " + rosterCount + "/" + label);
            string[] boundBoardIds = board.BoundItemInstanceIds.ToArray();
            Equal(boardCount,
                boundBoardIds.Length,
                "Board exact-instance row count " + rosterCount + "/" + label);
            Equal(boardCount,
                boundBoardIds.Distinct(StringComparer.Ordinal).Count(),
                "Board exact-instance rows unique " + rosterCount + "/" + label);
            C1ExactBattleSandboxItemCardView stableCard = snapshot.roster
                .Where(value => snapshot.FindPlacementByInstanceId(
                    value.itemInstanceId) == null)
                .Select(value => FindBoundCard(tray, value.itemInstanceId))
                .FirstOrDefault(value => value != null);
            InvokeExactPrivateVoid(
                tray,
                "Bind",
                new[]
                {
                    typeof(C1FormalItemSessionSnapshot),
                    typeof(string),
                    typeof(IReadOnlyDictionary<string,
                        C1ExactBattleSandboxItemFootprint>),
                    typeof(C1FormalItemPresentationCatalogSnapshot)
                },
                new object[]
                {
                    snapshot,
                    string.Empty,
                    footprints,
                    board.PresentationCatalogSnapshot
                });
            if (stableCard != null)
                Require(ReferenceEquals(
                        stableCard,
                        FindBoundCard(tray, stableCard.ItemInstanceId)),
                    "STABLE_TRAY_POOL_ENTRY_NOT_REUSED " + label);
            foreach (C1FormalItemRosterEntrySnapshot roster in snapshot.roster)
            {
                bool expectedOnBoard = snapshot.FindPlacementByInstanceId(
                    roster.itemInstanceId) != null;
                C1ExactBattleSandboxItemCardView card = FindBoundCard(
                    tray,
                    roster.itemInstanceId);
                Require(expectedOnBoard
                        ? boundBoardIds.Contains(
                            roster.itemInstanceId,
                            StringComparer.Ordinal) && card == null
                        : card != null
                          && string.Equals(
                              card.ItemInstanceId,
                              roster.itemInstanceId,
                              StringComparison.Ordinal)
                          && string.Equals(
                              card.BaseItemId,
                              roster.baseItemId,
                              StringComparison.Ordinal),
                    "EXACT_INSTANCE_BINDING_LOST "
                    + roster.itemInstanceId + "/" + label);
            }
        }

        private static C1FormalItemSessionSnapshot
            CreateDynamicPresentationSnapshot(
                int rosterCount,
                int boardCount,
                string suffix)
        {
            List<C1FormalItemRosterEntrySnapshot> rosters = new();
            List<C1FormalItemPlacementSnapshot> placements = new();
            List<C1FormalItemTrayPlacementSnapshot> trayPlacements = new();
            List<ItemSystemPlacementSnapshot> itemSystemPlacements = new();
            for (int index = 0; index < rosterCount; index++)
            {
                string itemInstanceId = "dynamic." + suffix + ".instance."
                                        + index.ToString(
                                            CultureInfo.InvariantCulture);
                rosters.Add(InvokeInternalConstructor<
                    C1FormalItemRosterEntrySnapshot>(
                    new[]
                    {
                        typeof(string), typeof(string), typeof(bool),
                        typeof(string), typeof(string), typeof(string),
                        typeof(string),
                        typeof(C1FormalItemOrdinaryInstanceSnapshot)
                    },
                    itemInstanceId,
                    "I003",
                    false,
                    "white",
                    "dynamic.pool15.profile.I003",
                    "dynamic.catalog.signature",
                    "dynamic.instance.signature." + index,
                    null));
                Vector2Int cell = new(index % 5, index / 5);
                if (index < boardCount)
                {
                    string placementId = "dynamic.placement." + suffix + "."
                                         + index.ToString(
                                             CultureInfo.InvariantCulture);
                    placements.Add(InvokeInternalConstructor<
                        C1FormalItemPlacementSnapshot>(
                        new[]
                        {
                            typeof(string), typeof(string), typeof(string),
                            typeof(Vector2Int), typeof(int)
                        },
                        itemInstanceId,
                        placementId,
                        "I003",
                        cell,
                        0));
                    itemSystemPlacements.Add(new ItemSystemPlacementSnapshot(
                        placementId,
                        "I003",
                        "I003",
                        cell,
                        0,
                        new[] { cell },
                        cell,
                        false,
                        true,
                        true,
                        I031InventoryPlacementContract.ItemId,
                        "dynamic.source",
                        1,
                        Array.Empty<Vector2Int>(),
                        false,
                        false,
                        true,
                        1,
                        1,
                        Array.Empty<string>(),
                        Array.Empty<string>()));
                }
                else
                {
                    int trayIndex = index - boardCount;
                    Vector2Int trayCell = new(
                        trayIndex % C1FormalItemSessionContract.TrayColumnCount,
                        trayIndex / C1FormalItemSessionContract.TrayColumnCount);
                    trayPlacements.Add(InvokeInternalConstructor<
                        C1FormalItemTrayPlacementSnapshot>(
                        new[]
                        {
                            typeof(string), typeof(Vector2Int), typeof(int),
                            typeof(IEnumerable<Vector2Int>), typeof(bool)
                        },
                        itemInstanceId,
                        trayCell,
                        0,
                        new[] { trayCell },
                        true));
                }
            }

            C1FormalItemTrayLayoutSnapshot trayLayout =
                InvokeInternalConstructor<C1FormalItemTrayLayoutSnapshot>(
                    new[]
                    {
                        typeof(IEnumerable<C1FormalItemTrayPlacementSnapshot>)
                    },
                    trayPlacements);
            ItemSystemSnapshot itemSystem = new(
                5,
                new Vector2Int(2, 2),
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemCatalogItemSnapshot>(),
                itemSystemPlacements,
                Array.Empty<Vector2Int>(),
                Array.Empty<ItemSystemLightingResultSnapshot>(),
                Array.Empty<ItemSystemArrayBonusResultSnapshot>(),
                null,
                Array.Empty<ItemSystemAwakeningResultSnapshot>(),
                null,
                string.Empty,
                false,
                string.Empty,
                Array.Empty<ItemSystemValidationError>());
            return InvokeInternalConstructor<C1FormalItemSessionSnapshot>(
                new[]
                {
                    typeof(string), typeof(long),
                    typeof(IEnumerable<C1FormalItemRosterEntrySnapshot>),
                    typeof(IEnumerable<C1FormalItemPlacementSnapshot>),
                    typeof(C1FormalItemTrayLayoutSnapshot),
                    typeof(ItemSystemSnapshot), typeof(string)
                },
                "dynamic.presentation." + suffix + "." + rosterCount + "."
                + boardCount,
                1L,
                rosters,
                placements,
                trayLayout,
                itemSystem,
                "dynamic.entitlement.signature");
        }

        private static bool InvokeExactInstanceBindingAdmission(
            int physicalCapacity,
            IEnumerable<KeyValuePair<string, string>> containerRows,
            IEnumerable<KeyValuePair<string, string>> rosterRows)
        {
            MethodInfo method = typeof(C1ExactBattleSandboxItemBoardView)
                .GetMethod(
                    "CanBindExactInstanceRows",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(int),
                        typeof(IEnumerable<KeyValuePair<string, string>>),
                        typeof(IEnumerable<KeyValuePair<string, string>>)
                    },
                    null);
            Require(method != null,
                "EXACT_INSTANCE_BINDING_ADMISSION_API_MISSING");
            return (bool)method.Invoke(null, new object[]
            {
                physicalCapacity,
                containerRows,
                rosterRows
            });
        }

        private static Vector2Int FindValidCandidate(
            C1ExactBattleSandboxItemArrangementPresenter presenter,
            string itemInstanceId,
            Vector2Int avoid,
            int rotation)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    Vector2Int candidate = new(x, y);
                    if (candidate == avoid) continue;
                    if (presenter.PreviewCandidate(
                            itemInstanceId,
                            candidate,
                            rotation,
                            out bool locallyValid)
                        && locallyValid)
                        return candidate;
                }
            }

            throw new InvalidOperationException(
                "NO_VALID_FORMAL_ITEM_CANDIDATE item=" + itemInstanceId);
        }

        private static Vector2Int FindValidCandidateByCurrentLighting(
            C1ExactBattleSandboxItemArrangementPresenter presenter,
            C1FormalItemSessionSnapshot snapshot,
            string itemInstanceId,
            Vector2Int avoid,
            int visualQuarterTurns,
            bool requireLit)
        {
            Require(presenter != null,
                "POOL15_LIGHTING_CANDIDATE_PRESENTER_MISSING");
            Require(snapshot?.itemSystemSnapshot != null,
                "POOL15_LIGHTING_CANDIDATE_ITEM_SYSTEM_SNAPSHOT_MISSING");
            C1FormalItemRosterEntrySnapshot roster =
                snapshot.FindRosterEntry(itemInstanceId);
            Require(roster != null,
                "POOL15_LIGHTING_CANDIDATE_ROSTER_ENTRY_MISSING");
            ItemSystemCatalogItemSnapshot catalog = snapshot
                .itemSystemSnapshot.catalogItems.SingleOrDefault(value =>
                    value != null
                    && string.Equals(
                        value.itemId,
                        roster.baseItemId,
                        StringComparison.Ordinal));
            Require(catalog != null,
                "POOL15_LIGHTING_CANDIDATE_CATALOG_ENTRY_MISSING");
            Require(!catalog.isLightingSource,
                "POOL15_LIGHTING_CANDIDATE_MUST_BE_ORDINARY_ITEM");
            Vector2Int rotatedCoreCell =
                C1ExactBattleSandboxItemRotationAdapter.RotateVisualCell(
                    catalog.coreCellLocal,
                    visualQuarterTurns);
            HashSet<Vector2Int> authoritativeLitRange = new(
                snapshot.itemSystemSnapshot.LitRangeCells);
            for (int y = 0; y < snapshot.itemSystemSnapshot.boardSize; y++)
            {
                for (int x = 0; x < snapshot.itemSystemSnapshot.boardSize; x++)
                {
                    Vector2Int candidate = new(x, y);
                    if (candidate == avoid) continue;
                    bool candidateIsLit = authoritativeLitRange.Contains(
                        candidate + rotatedCoreCell);
                    if (candidateIsLit != requireLit) continue;
                    if (presenter.PreviewCandidate(
                            itemInstanceId,
                            candidate,
                            visualQuarterTurns,
                            out bool locallyValid)
                        && locallyValid)
                        return candidate;
                }
            }

            throw new InvalidOperationException(
                "NO_VALID_FORMAL_ITEM_LIGHTING_CANDIDATE item="
                + itemInstanceId
                + " lit="
                + requireLit);
        }

        private static void VerifyPool15ExactArtworkBindings(
            C1ExactBattleSandboxItemBoardView board,
            C1ExactBattleSandboxItemCardView card)
        {
            Require(board != null && card != null,
                "POOL15_ARTWORK_RESOLVER_FIXTURE_MISSING");
            C1FormalItemPresentationCatalogSnapshot catalog =
                board.PresentationCatalogSnapshot;
            Require(catalog != null
                    && catalog.Rows.Count
                       == C1CampaignLootEligibleItemPool15Carrier.Resolve()
                           .snapshot.Profiles.Count + 2,
                "REV04_DYNAMIC_PRESENTATION_CATALOG_INVENTORY_INVALID");
            HashSet<string> identities = new(StringComparer.Ordinal);
            HashSet<Sprite> artworks = new();
            foreach (C1FormalItemPresentationRowSnapshot row in catalog.Rows)
            {
                Sprite resolved = null;
                string artworkIdentity = string.Empty;
                Require(row != null
                        && identities.Add(row.rarityVersionIdentity)
                        && row.rowCanonicalSignature.Length > 0
                        && row.effectFamilyKey.Length > 0
                        && row.presentationStyleKey.Length > 0
                        && row.cueIdentity.Length > 0
                        && board.TryResolveFormalItemArtwork(
                            row.baseItemId,
                            C1FormalItemArtworkLightingState.PlacementAbsent,
                            out resolved,
                            out artworkIdentity)
                        && resolved != null
                        && string.Equals(
                            artworkIdentity,
                            C1FormalItemArtworkIdentity.Create(
                                row.baseItemId,
                                C1FormalItemArtworkLightingState
                                    .PlacementAbsent),
                            StringComparison.Ordinal),
                    "REV04_DYNAMIC_CATALOG_ROW_NOT_RESOLVABLE "
                    + row?.rarityVersionIdentity);
                if (!row.isSpecialLightingSource)
                    Require(artworks.Add(resolved),
                        "REV04_DYNAMIC_ORDINARY_ARTWORK_DUPLICATE "
                        + row.rarityVersionIdentity);
            }

            C1FormalItemPresentationRowSnapshot special = catalog.Rows.Single(
                value => value.isSpecialLightingSource);
            Require(special.TryResolveArtwork(
                        C1FormalItemArtworkLightingState.Unlit,
                        out Sprite unlit,
                        out string unlitIdentity)
                    && special.TryResolveArtwork(
                        C1FormalItemArtworkLightingState.Lit,
                        out Sprite lit,
                        out string litIdentity)
                    && unlit != null
                    && lit != null
                    && !ReferenceEquals(unlit, lit)
                    && !string.Equals(
                        unlitIdentity,
                        litIdentity,
                        StringComparison.Ordinal),
                "I031_LIT_UNLIT_ARTWORK_REGRESSION");
            Require(!board.TryResolveFormalItemArtwork(
                    "I005",
                    C1FormalItemArtworkLightingState.Lit,
                    out _,
                    out _),
                "UNKNOWN_CATALOG_ARTWORK_MUST_FAIL_CLOSED");
        }

        private static void VerifyPool15SourceCandidateAdmissionMatrix()
        {
            Require(InvokePool15SourceCandidateAdmission(
                    1, 1, 1, 1, 0, 1, true, true, true, true),
                "PLACED_LIT_SOURCE_BASELINE_REJECTED");
            object[][] rejectedDimensions =
            {
                new object[] { 0, 1, 1, 1, 0, 1, true, true, true, true },
                new object[] { 1, 0, 1, 1, 0, 1, true, true, true, true },
                new object[] { 1, 1, 0, 1, 0, 1, true, true, true, true },
                new object[] { 1, 1, 1, 2, 0, 1, true, true, true, true },
                new object[] { 1, 1, 1, 1, 1, 1, true, true, true, true },
                new object[] { 1, 1, 1, 1, 0, 0, true, true, true, true },
                new object[] { 1, 1, 1, 1, 0, 1, false, true, true, true },
                new object[] { 1, 1, 1, 1, 0, 1, true, false, true, true },
                new object[] { 1, 1, 1, 1, 0, 1, true, true, false, true },
                new object[] { 1, 1, 1, 1, 0, 1, true, true, true, false }
            };
            for (int index = 0; index < rejectedDimensions.Length; index++)
            {
                Require(!InvokePool15SourceCandidateAdmission(
                        rejectedDimensions[index]),
                    "SOURCE_FAIL_CLOSED_DIMENSION_ACCEPTED " + index);
            }

            Require(!InvokePool15SourceCandidateAdmission(
                        1, 1, 1, 1, 1, 1, true, true, true, true)
                    && !InvokePool15SourceCandidateAdmission(
                        1, 1, 1, 1, 0, 2, true, true, true, true)
                    && !InvokePool15SourceCandidateAdmission(
                        2, 1, 1, 1, 0, 1, true, true, true, true),
                "DUPLICATE_OR_TRAY_SOURCE_ACCEPTED");

            MethodInfo presenterApi = typeof(
                    C1ExactBattleSandboxItemArrangementPresenter)
                .GetMethod(
                    "TryResolveCurrentPlacedLitSourceAnchor",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[]
                    {
                        typeof(C1Pool15FormalItemSourceAnchorRequest),
                        typeof(C1Pool15FormalItemSourceAnchorBinding)
                            .MakeByRefType()
                    },
                    null);
            Require(presenterApi != null
                    && presenterApi.ReturnType == typeof(bool)
                    && typeof(RectTransform).IsAssignableFrom(
                        typeof(C1Pool15FormalItemSourceAnchorBinding)
                            .GetProperty("sourceAnchor")?.PropertyType),
                "SOURCE_ANCHOR_API_SHAPE_INVALID");
        }

        private static bool InvokePool15SourceCandidateAdmission(
            int rosterMatchCount,
            int formalPlacementMatchCount,
            int itemSystemPlacementMatchCount,
            int trayPlacementMatchCount,
            int activeTrayPlacementMatchCount,
            int liveAnchorMatchCount,
            bool exactCatalogOrdinaryIdentity,
            bool exactIdentityAligned,
            bool requestCatalogAligned,
            bool isLit)
        {
            return InvokePool15SourceCandidateAdmission(new object[]
            {
                rosterMatchCount,
                formalPlacementMatchCount,
                itemSystemPlacementMatchCount,
                trayPlacementMatchCount,
                activeTrayPlacementMatchCount,
                liveAnchorMatchCount,
                exactCatalogOrdinaryIdentity,
                exactIdentityAligned,
                requestCatalogAligned,
                isLit
            });
        }

        private static bool InvokePool15SourceCandidateAdmission(object[] values)
        {
            MethodInfo method = typeof(
                    C1Pool15FormalItemArtworkAndSourceAnchorBinding)
                .GetMethod(
                    "IsPlacedLitSourceCandidate",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof(int), typeof(int), typeof(int), typeof(int),
                        typeof(int), typeof(int), typeof(bool), typeof(bool),
                        typeof(bool), typeof(bool)
                    },
                    null);
            Require(method != null && method.ReturnType == typeof(bool),
                "SOURCE_CANDIDATE_ADMISSION_API_MISSING");
            return method.Invoke(null, values) is bool accepted && accepted;
        }

        private static void VerifyRuntimeIsolation()
        {
            foreach (string path in RuntimeSourcePaths)
            {
                Require(File.Exists(path), "RUNTIME_SOURCE_MISSING " + path);
                string source = File.ReadAllText(path);
                foreach (string forbidden in new[]
                         {
                             "TalismanBag.BuildSandbox",
                             "BuildGridInteractionPreviewController",
                             "GameObject.Find",
                             "new GameObject",
                             "UnityEditor",
                             "PlayerPrefs",
                             "Resources.FindObjectsOfTypeAll"
                         })
                {
                    Require(source.IndexOf(forbidden, StringComparison.Ordinal) < 0,
                        "RUNTIME_ISOLATION_TOKEN " + forbidden + " path=" + path);
                }
            }
        }

        private static void VerifyRotationAdapter()
        {
            int[] expectedFormalDegrees = { 0, 270, 180, 90 };
            for (int visualQuarterTurns = 0;
                 visualQuarterTurns < 4;
                 visualQuarterTurns++)
            {
                Require(C1ExactBattleSandboxItemRotationAdapter
                        .TryVisualQuarterTurnsToFormalDegrees(
                            visualQuarterTurns,
                            out int formalDegrees),
                    "VISUAL_TO_FORMAL_ROTATION_FAILED visual="
                    + visualQuarterTurns);
                Equal(expectedFormalDegrees[visualQuarterTurns], formalDegrees,
                    "exact visual-to-formal rotation");
                Require(C1ExactBattleSandboxItemRotationAdapter
                        .TryFormalDegreesToVisualQuarterTurns(
                            formalDegrees,
                            out int roundTripVisual),
                    "FORMAL_TO_VISUAL_ROTATION_FAILED degrees="
                    + formalDegrees);
                Equal(visualQuarterTurns, roundTripVisual,
                    "exact formal-to-visual rotation round trip");
            }

            foreach (int invalidDegrees in new[]
                     {
                         -90,
                         1,
                         45,
                         89,
                         91,
                         360
                     })
            {
                Require(!C1ExactBattleSandboxItemRotationAdapter
                        .TryFormalDegreesToVisualQuarterTurns(
                            invalidDegrees,
                            out _),
                    "INVALID_FORMAL_ROTATION_MUST_FAIL_CLOSED degrees="
                    + invalidDegrees);
            }
        }

        private static void VerifyGenericFootprintMapping()
        {
            Vector2 unitX = new(10f, 0f);
            Vector2 unitY = new(0f, 20f);
            Vector2Int[] single = { Vector2Int.zero };
            Vector2Int[] line =
            {
                Vector2Int.zero,
                new Vector2Int(0, 1),
                new Vector2Int(0, 2)
            };
            Vector2Int[] leftCorner =
            {
                Vector2Int.zero,
                new Vector2Int(1, 0),
                new Vector2Int(0, 1)
            };
            Vector2Int[] rightCorner =
            {
                Vector2Int.zero,
                new Vector2Int(1, 0),
                new Vector2Int(1, 1)
            };
            Vector2Int[] square =
            {
                Vector2Int.zero,
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            };

            for (int rotation = 0; rotation < 4; rotation++)
            {
                C1ExactBattleSandboxItemFootprint singleFootprint =
                    CreateFootprint(single, rotation, unitX, unitY);
                Equal(1, singleFootprint.WidthInCells,
                    "single width all rotations");
                Equal(1, singleFootprint.HeightInCells,
                    "single height all rotations");
                Approximately(new Vector2(10f, 20f),
                    singleFootprint.TrayLocalSize,
                    "single transformed size all rotations");

                C1ExactBattleSandboxItemFootprint lineFootprint =
                    CreateFootprint(line, rotation, unitX, unitY);
                bool swaps = (rotation & 1) != 0;
                Equal(swaps ? 3 : 1, lineFootprint.WidthInCells,
                    "line normalized width");
                Equal(swaps ? 1 : 3, lineFootprint.HeightInCells,
                    "line normalized height");
                Approximately(swaps
                        ? new Vector2(30f, 20f)
                        : new Vector2(10f, 60f),
                    lineFootprint.TrayLocalSize,
                    "line transformed size");

                C1ExactBattleSandboxItemFootprint squareFootprint =
                    CreateFootprint(square, rotation, unitX, unitY);
                Equal(2, squareFootprint.WidthInCells,
                    "square normalized width");
                Equal(2, squareFootprint.HeightInCells,
                    "square normalized height");
                Approximately(new Vector2(20f, 40f),
                    squareFootprint.TrayLocalSize,
                    "square transformed size");
            }

            C1ExactBattleSandboxItemFootprint left = CreateFootprint(
                leftCorner,
                0,
                unitX,
                unitY);
            C1ExactBattleSandboxItemFootprint right = CreateFootprint(
                rightCorner,
                0,
                unitX,
                unitY);
            Equal("0:0|1:0|0:1", CellSignature(left.NormalizedCells),
                "left corner normalized cells");
            Equal("0:0|1:0|1:1", CellSignature(right.NormalizedCells),
                "right corner normalized cells");
            Approximately(new Vector2(20f, 40f), left.TrayLocalSize,
                "left corner transformed size");
            Approximately(new Vector2(20f, 40f), right.TrayLocalSize,
                "right corner transformed size");

            Require(!TryCreateFootprint(
                        single,
                        4,
                        unitX,
                        unitY,
                        out _),
                "INVALID_VISUAL_ROTATION_MUST_FAIL_CLOSED");
            Require(!TryCreateFootprint(
                        new[] { Vector2Int.zero, Vector2Int.zero },
                        0,
                        unitX,
                        unitY,
                        out _),
                "DUPLICATE_AUTHORITATIVE_SHAPE_CELL_MUST_FAIL_CLOSED");

            string[] expectedCornerSignatures =
            {
                "0:0|1:0|0:1",
                "0:0|0:1|1:1",
                "1:0|0:1|1:1",
                "0:0|1:0|1:1"
            };
            for (int visual = 0; visual < 4; visual++)
            {
                C1ExactBattleSandboxItemFootprint cornerFootprint =
                    CreateFootprint(
                        leftCorner,
                        visual,
                        unitX,
                        unitY);
                Equal(
                    expectedCornerSignatures[visual],
                    CellSignature(cornerFootprint.NormalizedCells),
                    "frozen clockwise corner table visual=" + visual);
                List<Vector2Int> resolvedRawCells = new();
                int transparentCellCount = 0;
                for (int y = 0; y < cornerFootprint.HeightInCells; y++)
                {
                    for (int x = 0; x < cornerFootprint.WidthInCells; x++)
                    {
                        if (TryResolveFootprintRawCell(
                                cornerFootprint,
                                new Vector2Int(x, y),
                                out Vector2Int rawCell))
                            resolvedRawCells.Add(rawCell);
                        else
                            transparentCellCount++;
                    }
                }

                Equal(leftCorner.Length, resolvedRawCells.Count,
                    "corner occupied pointer cells visual=" + visual);
                Equal(1, transparentCellCount,
                    "corner transparent pointer cell rejects visual=" + visual);
                Equal(
                    CellSignature(leftCorner.OrderBy(cell => cell.y)
                        .ThenBy(cell => cell.x).ToArray()),
                    CellSignature(resolvedRawCells.OrderBy(cell => cell.y)
                        .ThenBy(cell => cell.x).ToArray()),
                    "corner pointer maps exact raw cells visual=" + visual);
            }
        }

        private static void VerifyRev04MachineCandidateMatrix()
        {
            string[] itemIds = { "I010", "I016", "I002", "I018" };
            foreach (string itemId in itemIds)
            {
                ItemInnerDataDefinition definition =
                    ItemInnerDataCatalog.FindById(itemId);
                Require(definition != null,
                    "REV04_MACHINE_ITEM_MISSING " + itemId);
                IReadOnlyList<Vector2Int> rawCells = definition.ShapeCells;
                Require(rawCells != null && rawCells.Count > 0,
                    "REV04_MACHINE_SHAPE_MISSING " + itemId);
                Require(rawCells.Contains(definition.coreCellLocal),
                    "REV04_MACHINE_CORE_NOT_IN_SHAPE " + itemId);

                for (int visual = 0; visual < 4; visual++)
                {
                    foreach (Vector2Int rawGrabbedCell in rawCells)
                    {
                        CandidateProbe interior = CreateInteractionCandidate(
                            rawCells,
                            rawGrabbedCell,
                            new Vector2Int(2, 2),
                            visual,
                            Array.Empty<Vector2Int>(),
                            5);
                        VerifyCandidateProbe(
                            interior,
                            rawCells,
                            rawGrabbedCell,
                            new Vector2Int(2, 2),
                            true,
                            itemId + " interior visual=" + visual);

                        CandidateProbe edge = FindLegalEdgeCandidate(
                            rawCells,
                            rawGrabbedCell,
                            visual);
                        Require(edge != null,
                            "REV04_LEGAL_EDGE_CANDIDATE_MISSING item="
                            + itemId + " visual=" + visual
                            + " raw=" + rawGrabbedCell);
                        VerifyCandidateProbe(
                            edge,
                            rawCells,
                            rawGrabbedCell,
                            edge.PointerCell,
                            true,
                            itemId + " edge visual=" + visual);
                        Require(edge.OrderedOccupiedCells.Any(cell =>
                                cell.x == 0 || cell.x == 4
                                            || cell.y == 0 || cell.y == 4),
                            "REV04_EDGE_MUST_TOUCH_BOARD_BOUNDARY item="
                            + itemId + " visual=" + visual);

                        CandidateProbe collision = CreateInteractionCandidate(
                            rawCells,
                            rawGrabbedCell,
                            interior.PointerCell,
                            visual,
                            new[] { interior.OrderedOccupiedCells[0] },
                            5);
                        Require(!collision.IsLocallyValid,
                            "REV04_COLLISION_MUST_REJECT item=" + itemId
                            + " visual=" + visual);
                        Equal(
                            CellSignature(interior.OrderedOccupiedCells),
                            CellSignature(collision.OrderedOccupiedCells),
                            "collision does not change candidate geometry");

                        CandidateProbe outOfBounds = CreateInteractionCandidate(
                            rawCells,
                            rawGrabbedCell,
                            new Vector2Int(-2, -2),
                            visual,
                            Array.Empty<Vector2Int>(),
                            5);
                        Require(!outOfBounds.IsLocallyValid,
                            "REV04_BOUNDS_MUST_REJECT item=" + itemId
                            + " visual=" + visual);
                    }
                }
            }
        }

        private static void VerifyRev04FTrayCandidateMatrix()
        {
            foreach (string itemId in new[] { "I010", "I016", "I002", "I018" })
            {
                ItemInnerDataDefinition definition =
                    ItemInnerDataCatalog.FindById(itemId);
                Require(definition?.ShapeCells != null
                        && definition.ShapeCells.Count > 0,
                    "REV04F_TRAY_MACHINE_SHAPE_MISSING " + itemId);
                IReadOnlyList<Vector2Int> rawCells = definition.ShapeCells;
                for (int visual = 0; visual < 4; visual++)
                {
                    Require(C1ExactBattleSandboxItemRotationAdapter
                            .TryVisualQuarterTurnsToFormalDegrees(
                                visual,
                                out int formalDegrees),
                        "REV04F_TRAY_ROTATION_ADAPTER_FAILED " + itemId);
                    foreach (Vector2Int rawGrabbedCell in rawCells)
                    {
                        Vector2Int pointerCell = new Vector2Int(2, 6);
                        Require(C1FormalItemTrayLayoutRules.TryBuildCandidate(
                                rawCells,
                                rawGrabbedCell,
                                pointerCell,
                                formalDegrees,
                                out C1FormalItemPlacementCandidate candidate,
                                out IReadOnlyList<Vector2Int> occupied),
                            "REV04F_TRAY_CANDIDATE_FAILED item=" + itemId
                            + " visual=" + visual + " raw=" + rawGrabbedCell);
                        Require(C1FormalItemTrayLayoutRules.IsInsideTray(occupied),
                            "REV04F_TRAY_INTERIOR_MUST_BE_LEGAL item=" + itemId
                            + " visual=" + visual);
                        Require(occupied.SequenceEqual(occupied
                                .OrderBy(value => value.y)
                                .ThenBy(value => value.x)),
                            "REV04F_TRAY_OCCUPIED_ORDER_UNSTABLE item=" + itemId
                            + " visual=" + visual);
                        Require(C1ExactBattleSandboxItemRotationAdapter
                                .TryRotateFormalCell(
                                    rawGrabbedCell,
                                    formalDegrees,
                                    out Vector2Int rotatedGrabbed),
                            "REV04F_TRAY_RAW_ROTATION_FAILED");
                        Vector2Int[] rotated = rawCells.Select(value =>
                        {
                            C1ExactBattleSandboxItemRotationAdapter
                                .TryRotateFormalCell(
                                    value,
                                    formalDegrees,
                                    out Vector2Int resolved);
                            return resolved;
                        }).ToArray();
                        Vector2Int grabbedOffset = new Vector2Int(
                            rotatedGrabbed.x - rotated.Min(value => value.x),
                            rotated.Max(value => value.y) - rotatedGrabbed.y);
                        Equal(pointerCell,
                            candidate.anchorCell + grabbedOffset,
                            "REV04F Tray grabbed cell remains under pointer");
                        Equal(formalDegrees,
                            candidate.rotation,
                            "REV04F Tray formal rotation");
                        Require(C1FormalItemTrayLayoutRules
                                .TryBuildOccupiedCells(
                                    rawCells,
                                    candidate,
                                    out IReadOnlyList<Vector2Int> rebuilt)
                                && rebuilt.SequenceEqual(occupied),
                            "REV04F_TRAY_PREVIEW_COMMIT_GEOMETRY_SPLIT item="
                            + itemId + " visual=" + visual);

                        Require(C1FormalItemTrayLayoutRules.TryBuildCandidate(
                                rawCells,
                                rawGrabbedCell,
                                new Vector2Int(-2, -2),
                                formalDegrees,
                                out _,
                                out IReadOnlyList<Vector2Int> outside)
                                && !C1FormalItemTrayLayoutRules.IsInsideTray(
                                    outside),
                            "REV04F_TRAY_BOUNDS_MUST_REJECT item=" + itemId
                            + " visual=" + visual);
                    }
                }
            }
        }

        private static CandidateProbe FindLegalEdgeCandidate(
            IReadOnlyList<Vector2Int> rawCells,
            Vector2Int rawGrabbedCell,
            int visualQuarterTurns)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    CandidateProbe candidate = CreateInteractionCandidate(
                        rawCells,
                        rawGrabbedCell,
                        new Vector2Int(x, y),
                        visualQuarterTurns,
                        Array.Empty<Vector2Int>(),
                        5);
                    if (candidate.IsLocallyValid
                        && candidate.OrderedOccupiedCells.Any(cell =>
                            cell.x == 0 || cell.x == 4
                                        || cell.y == 0 || cell.y == 4))
                        return candidate;
                }
            }

            return null;
        }

        private static void VerifyCandidateProbe(
            CandidateProbe candidate,
            IReadOnlyList<Vector2Int> rawCells,
            Vector2Int rawGrabbedCell,
            Vector2Int pointerCell,
            bool expectedLocallyValid,
            string label)
        {
            Require(candidate != null, label + " candidate");
            Require(C1ExactBattleSandboxItemRotationAdapter
                    .TryVisualQuarterTurnsToFormalDegrees(
                        candidate.VisualQuarterTurns,
                        out int expectedFormalDegrees),
                label + " visual-to-formal");
            Equal(expectedFormalDegrees, candidate.FormalDegrees,
                label + " formal degrees");
            Equal(rawGrabbedCell, candidate.RawGrabbedCell,
                label + " raw grabbed identity");
            Equal(pointerCell, candidate.PointerCell,
                label + " pointer cell");
            Equal(expectedLocallyValid, candidate.IsLocallyValid,
                label + " legality");
            Require(C1ExactBattleSandboxItemRotationAdapter
                    .TryFormalDegreesToVisualQuarterTurns(
                        candidate.FormalDegrees,
                        out int hydratedVisual),
                label + " formal hydration");
            Equal(candidate.VisualQuarterTurns, hydratedVisual,
                label + " visual round trip");
            Equal(candidate.AuthorityAnchor,
                candidate.FormalCandidate.anchorCell,
                label + " submitted authority anchor");
            Equal(candidate.FormalDegrees,
                candidate.FormalCandidate.rotation,
                label + " submitted formal rotation");
            Equal(candidate.NormalizedDisplayAnchor - candidate.RotatedMinimum,
                candidate.AuthorityAnchor,
                label + " normalized-to-raw anchor conversion");

            C1ExactBattleSandboxItemRotationAdapter.TryRotateFormalCell(
                rawGrabbedCell,
                candidate.FormalDegrees,
                out Vector2Int rotatedGrabbedCell);
            Equal(pointerCell,
                candidate.AuthorityAnchor + rotatedGrabbedCell,
                label + " grabbed cell remains under pointer");
            Vector2Int[] reconstructed = rawCells
                .Select(rawCell =>
                {
                    C1ExactBattleSandboxItemRotationAdapter
                        .TryRotateFormalCell(
                            rawCell,
                            candidate.FormalDegrees,
                            out Vector2Int rotated);
                    return candidate.AuthorityAnchor + rotated;
                })
                .OrderBy(cell => cell.y)
                .ThenBy(cell => cell.x)
                .ToArray();
            Equal(CellSignature(reconstructed),
                CellSignature(candidate.OrderedOccupiedCells),
                label + " preview/commit/accepted cells");
            Equal(candidate.OrderedOccupiedCells.Count,
                candidate.OrderedOccupiedCells.Distinct().Count(),
                label + " occupied cells unique");
        }

        private static void VerifyRev04GrabLifecycle()
        {
#pragma warning disable SYSLIB0050
            C1ExactBattleSandboxItemBoardView board =
                (C1ExactBattleSandboxItemBoardView)
                FormatterServices.GetUninitializedObject(typeof(
                    C1ExactBattleSandboxItemBoardView));
#pragma warning restore SYSLIB0050
            const string instanceId = "rev04-grabbed-instance";
            Vector2Int rawGrabbedCell = new(1, 0);
            SetExactPrivateField(board, "pressedItemInstanceId", instanceId);
            SetExactPrivateField(board, "pressedRawGrabbedCell", rawGrabbedCell);
            SetExactPrivateField(board, "hasPressedRawGrabbedCell", true);
            InvokeExactPrivateVoid(
                board,
                "CapturePressedIdentityForActiveBoardDrag",
                Type.EmptyTypes,
                Array.Empty<object>());
            Equal(instanceId,
                ReadExactPrivateField<string>(
                    board,
                    "activeBoardDragItemInstanceId"),
                "REV04 stable Board drag item identity");
            Equal(rawGrabbedCell,
                ReadExactPrivateField<Vector2Int>(
                    board,
                    "activeBoardDragRawGrabbedCell"),
                "REV04 stable Board raw grabbed identity");
            Require(ReadExactPrivateField<bool>(
                    board,
                    "hasActiveBoardDragRawGrabbedCell"),
                "REV04 stable Board raw grabbed flag");

            InvokeExactPrivateVoid(
                board,
                "PreparePointerStateForBind",
                new[] { typeof(C1FormalItemSessionSnapshot) },
                new object[] { null });
            Equal(string.Empty,
                ReadExactPrivateField<string>(board, "pressedItemInstanceId"),
                "REV04 benign bind clears transient press");
            Equal(instanceId,
                ReadExactPrivateField<string>(
                    board,
                    "activeBoardDragItemInstanceId"),
                "REV04 benign bind preserves active drag");
            Equal(rawGrabbedCell,
                ReadExactPrivateField<Vector2Int>(
                    board,
                    "activeBoardDragRawGrabbedCell"),
                "REV04 benign bind preserves active raw cell");

            InvokeExactPrivateVoid(
                board,
                "ClearPointerIdentities",
                Type.EmptyTypes,
                Array.Empty<object>());
            Equal(string.Empty,
                ReadExactPrivateField<string>(
                    board,
                    "activeBoardDragItemInstanceId"),
                "REV04 cancel/lock/end clears active item");
            Require(!ReadExactPrivateField<bool>(
                    board,
                    "hasActiveBoardDragRawGrabbedCell"),
                "REV04 cancel/lock/end clears raw grabbed cell");

            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create("rev04-reset-session", 1);
            Require(creation != null && creation.isSuccess,
                "REV04_RESET_SNAPSHOT_CREATE_FAILED");
            SetExactPrivateField<C1FormalItemSessionSnapshot>(
                board,
                "snapshot",
                null);
            SetExactPrivateField(
                board,
                "activeBoardDragItemInstanceId",
                instanceId);
            SetExactPrivateField(
                board,
                "activeBoardDragRawGrabbedCell",
                rawGrabbedCell);
            SetExactPrivateField(
                board,
                "hasActiveBoardDragRawGrabbedCell",
                true);
            InvokeExactPrivateVoid(
                board,
                "PreparePointerStateForBind",
                new[] { typeof(C1FormalItemSessionSnapshot) },
                new object[] { creation.authority.Current });
            Require(!ReadExactPrivateField<bool>(
                    board,
                    "hasActiveBoardDragRawGrabbedCell"),
                "REV04 session identity change clears raw grabbed cell");
        }

        private static void VerifyRev04FormalRoundTripTransactions()
        {
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    "rev04-formal-round-trip-session",
                    1);
            Require(creation != null && creation.isSuccess,
                "REV04_TRANSACTION_SESSION_CREATE_FAILED "
                + creation?.diagnosticCode);
            C1FormalItemSessionAuthority authority = creation.authority;
            string instanceId = CanonicalInitialItemAcquisitionPolicy.ItemInstanceId;
            C1FormalItemRosterEntrySnapshot roster = authority.Current
                .FindRosterEntry(instanceId);
            Require(roster != null,
                "REV04_TRANSACTION_ROSTER_INSTANCE_MISSING");
            ItemSystemCatalogItemSnapshot catalog = authority.Current
                .itemSystemSnapshot.catalogItems.Single(value => string.Equals(
                    value.itemId,
                    roster.baseItemId,
                    StringComparison.Ordinal));
            IReadOnlyList<Vector2Int> rawCells = catalog.ShapeCells;
            Vector2Int rawGrabbedCell = rawCells
                .OrderBy(value => value.y)
                .ThenBy(value => value.x)
                .First();
            int commandSequence = 0;

            CandidateProbe firstMove = FindValidTransactionCandidate(
                authority.Current,
                instanceId,
                rawCells,
                rawGrabbedCell,
                1,
                true);
            SubmitTransaction(
                authority,
                C1FormalItemArrangementCommandKind.MoveOnBoard,
                instanceId,
                firstMove,
                ++commandSequence,
                "REV04 first Board-to-Board");
            VerifyAcceptedTransactionPlacement(
                authority.Current,
                instanceId,
                firstMove,
                "REV04 first Board-to-Board");

            CandidateProbe secondMove = FindValidTransactionCandidate(
                authority.Current,
                instanceId,
                rawCells,
                rawGrabbedCell,
                2,
                true);
            SubmitTransaction(
                authority,
                C1FormalItemArrangementCommandKind.MoveOnBoard,
                instanceId,
                secondMove,
                ++commandSequence,
                "REV04 repeated Board-to-Board");
            VerifyAcceptedTransactionPlacement(
                authority.Current,
                instanceId,
                secondMove,
                "REV04 repeated Board-to-Board");

            string beforeRejectedCandidate = authority.Current
                .canonicalSignature;
            CandidateProbe collision = CreateInteractionCandidate(
                rawCells,
                rawGrabbedCell,
                authority.Current.itemSystemSnapshot.eyeCell,
                0,
                new[] { authority.Current.itemSystemSnapshot.eyeCell },
                authority.Current.itemSystemSnapshot.boardSize);
            Require(!collision.IsLocallyValid,
                "REV04 transaction collision preview must reject");
            Equal(beforeRejectedCandidate, authority.Current.canonicalSignature,
                "REV04 collision/cancel preserves authority snapshot");

            SubmitTransaction(
                authority,
                C1FormalItemArrangementCommandKind.ReturnToTray,
                instanceId,
                null,
                ++commandSequence,
                "REV04 Board-to-Tray");
            Require(authority.Current.FindPlacementByInstanceId(instanceId) == null,
                "REV04 Board-to-Tray removes placement");

            CandidateProbe placeFromTray = FindValidTransactionCandidate(
                authority.Current,
                instanceId,
                rawCells,
                rawGrabbedCell,
                3,
                false);
            SubmitTransaction(
                authority,
                C1FormalItemArrangementCommandKind.PlaceFromTray,
                instanceId,
                placeFromTray,
                ++commandSequence,
                "REV04 Tray-to-Board");
            VerifyAcceptedTransactionPlacement(
                authority.Current,
                instanceId,
                placeFromTray,
                "REV04 Tray-to-Board");
        }

        private static void VerifyRev04ArtworkOccupiedBounds()
        {
            const string methodName =
                "GetUnrotatedArtworkSizeForOccupiedBounds";
            MethodInfo method = typeof(C1ExactBattleSandboxItemBoardView)
                .GetMethod(
                    methodName,
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(Vector2), typeof(int) },
                    null);
            Require(method != null && method.ReturnType == typeof(Vector2),
                "REV04_ARTWORK_BOUNDS_ADAPTER_SIGNATURE_MISMATCH");
            foreach (Vector2 occupiedSize in new[]
                     {
                         new Vector2(100f, 200f),
                         new Vector2(200f, 100f),
                         new Vector2(200f, 200f)
                     })
            {
                for (int visual = 0; visual < 4; visual++)
                {
                    Vector2 unrotated = (Vector2)method.Invoke(
                        null,
                        new object[] { occupiedSize, visual });
                    bool swapsAxes = (visual & 1) != 0;
                    Vector2 expectedUnrotated = swapsAxes
                        ? new Vector2(occupiedSize.y, occupiedSize.x)
                        : occupiedSize;
                    Approximately(
                        expectedUnrotated,
                        unrotated,
                        "REV04 pre-rotation artwork size visual=" + visual);
                    Vector2 finalAxisAlignedBounds = swapsAxes
                        ? new Vector2(unrotated.y, unrotated.x)
                        : unrotated;
                    Approximately(
                        occupiedSize,
                        finalAxisAlignedBounds,
                        "REV04 artwork remains in occupied bounds visual="
                        + visual);
                }
            }
        }

        private static CandidateProbe FindValidTransactionCandidate(
            C1FormalItemSessionSnapshot snapshot,
            string itemInstanceId,
            IReadOnlyList<Vector2Int> rawCells,
            Vector2Int rawGrabbedCell,
            int visualQuarterTurns,
            bool requireChangedPlacement)
        {
            C1FormalItemPlacementSnapshot existing = snapshot
                .FindPlacementByInstanceId(itemInstanceId);
            string activePlacementId = existing?.placementId;
            Vector2Int[] blockingCells = snapshot.itemSystemSnapshot.placements
                .Where(value => !string.Equals(
                    value.placementId,
                    activePlacementId,
                    StringComparison.Ordinal))
                .SelectMany(value => value.OccupiedCells)
                .Concat(new[] { snapshot.itemSystemSnapshot.eyeCell })
                .Distinct()
                .ToArray();
            for (int y = 0; y < snapshot.itemSystemSnapshot.boardSize; y++)
            {
                for (int x = 0; x < snapshot.itemSystemSnapshot.boardSize; x++)
                {
                    CandidateProbe candidate = CreateInteractionCandidate(
                        rawCells,
                        rawGrabbedCell,
                        new Vector2Int(x, y),
                        visualQuarterTurns,
                        blockingCells,
                        snapshot.itemSystemSnapshot.boardSize);
                    bool placementChanged = existing == null
                                            || existing.anchorCell
                                            != candidate.AuthorityAnchor
                                            || existing.rotation
                                            != candidate.FormalDegrees;
                    if (candidate.IsLocallyValid
                        && (!requireChangedPlacement || placementChanged))
                        return candidate;
                }
            }

            throw new InvalidOperationException(
                "REV04_VALID_TRANSACTION_CANDIDATE_MISSING item="
                + itemInstanceId + " visual=" + visualQuarterTurns);
        }

        private static void SubmitTransaction(
            C1FormalItemSessionAuthority authority,
            C1FormalItemArrangementCommandKind kind,
            string itemInstanceId,
            CandidateProbe candidate,
            int commandSequence,
            string label)
        {
            C1FormalItemSessionSnapshot before = authority.Current;
            C1FormalItemSessionOperationResult result = authority.Submit(
                new C1FormalItemArrangementCommand(
                    kind,
                    before.sessionToken,
                    before.resetGeneration,
                    "rev04.transaction."
                    + commandSequence.ToString(CultureInfo.InvariantCulture),
                    itemInstanceId,
                    before.canonicalSignature,
                    candidate?.FormalCandidate));
            Require(result != null && result.accepted,
                label + " rejected diagnostic=" + result?.diagnosticCode);
        }

        private static void VerifyAcceptedTransactionPlacement(
            C1FormalItemSessionSnapshot snapshot,
            string itemInstanceId,
            CandidateProbe candidate,
            string label)
        {
            C1FormalItemPlacementSnapshot placement = snapshot
                .FindPlacementByInstanceId(itemInstanceId);
            Require(placement != null, label + " placement missing");
            ItemSystemPlacementSnapshot resolved = snapshot.itemSystemSnapshot
                .FindPlacement(placement.placementId);
            Require(resolved != null, label + " resolved placement missing");
            Equal(candidate.AuthorityAnchor, placement.anchorCell,
                label + " raw authority anchor");
            Equal(candidate.FormalDegrees, placement.rotation,
                label + " formal degrees");
            Equal(CellSignature(candidate.OrderedOccupiedCells),
                CellSignature(resolved.OccupiedCells),
                label + " accepted occupied cells");
        }

        private static void VerifyRev04RotateZoneStateMachine()
        {
            Type stateType = typeof(C1ExactBattleSandboxItemArrangementPresenter)
                .Assembly.GetType(
                    "TalismanBag.Items.CampaignBaseline."
                    + "C1ExactBattleSandboxRightRotateZoneState",
                    true);
            object state = Activator.CreateInstance(stateType, true);
            Rect activation = new(100f, -90f, 136f, 180f);
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(0f, 0f),
                    0f,
                    activation,
                    true),
                "REV04 rotate seek primes without trigger");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(25f, 0f),
                    0.01f,
                    activation,
                    true),
                "REV04 rotate seek minimum age");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(20f, 45f),
                    0.05f,
                    activation,
                    true),
                "REV04 rotate seek upward drift rejection");
            InvokeExactInternalVoid(stateType, state, "Reset");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(0f, 0f),
                    0f,
                    activation,
                    true),
                "REV04 rotate velocity fixture prime");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(20f, 0f),
                    0.10f,
                    activation,
                    true),
                "REV04 rotate minimum velocity rejection");
            Require(!ReadExactInternalProperty<bool>(
                    state,
                    stateType,
                    "IsSeekingOrInside",
                    "REV04_ROTATE_ZONE_VELOCITY_PROPERTY"),
                "REV04 rejected velocity does not open entry");
            InvokeExactInternalVoid(stateType, state, "Reset");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(0f, 0f),
                    0f,
                    activation,
                    true),
                "REV04 rotate seek-window fixture prime");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(20f, 0f),
                    0.05f,
                    activation,
                    true),
                "REV04 rotate seek-window opens");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(50f, 0f),
                    0.34f,
                    activation,
                    true),
                "REV04 rotate seek-window expiry");
            Require(!ReadExactInternalProperty<bool>(
                    state,
                    stateType,
                    "IsSeekingOrInside",
                    "REV04_ROTATE_ZONE_WINDOW_PROPERTY"),
                "REV04 expired seek clears entry");
            InvokeExactInternalVoid(stateType, state, "Reset");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(0f, 0f),
                    0f,
                    activation,
                    true),
                "REV04 rotate seek re-prime");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(20f, 0f),
                    0.05f,
                    activation,
                    true),
                "REV04 rotate seek opens entry window");
            Require(ReadExactInternalProperty<bool>(
                    state,
                    stateType,
                    "IsSeekingOrInside",
                    "REV04_ROTATE_ZONE_SEEKING_PROPERTY"),
                "REV04 rotate guide active while seeking");
            Require(InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(110f, 0f),
                    0.06f,
                    activation,
                    true),
                "REV04 rotate valid entry triggers once");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(120f, 0f),
                    0.20f,
                    activation,
                    true),
                "REV04 rotate hover cannot repeat");
            Require(InvokeExactInternalBool(
                    stateType,
                    state,
                    "IsConfirmationVisible",
                    new[] { typeof(float) },
                    new object[] { 0.20f }),
                "REV04 rotate confirmation lifetime active");
            Require(!InvokeExactInternalBool(
                    stateType,
                    state,
                    "IsConfirmationVisible",
                    new[] { typeof(float) },
                    new object[] { 0.25f }),
                "REV04 rotate confirmation lifetime expires");

            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(70f, 0f),
                    0.30f,
                    activation,
                    true),
                "REV04 rotate exit rearms entry");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(110f, 0f),
                    0.35f,
                    activation,
                    true),
                "REV04 rotate cooldown blocks re-entry trigger");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(70f, 0f),
                    1.10f,
                    activation,
                    true),
                "REV04 rotate second exit");
            Require(InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(110f, 0f),
                    1.16f,
                    activation,
                    true),
                "REV04 rotate trigger after cooldown and re-entry");
            Require(!InvokeRotateZoneUpdate(
                    stateType,
                    state,
                    new Vector2(130f, 0f),
                    1.20f,
                    activation,
                    false),
                "REV04 BATTLE_LOCKED rotate no-op");
            Require(!ReadExactInternalProperty<bool>(
                    state,
                    stateType,
                    "IsSeekingOrInside",
                    "REV04_ROTATE_ZONE_LOCKED_PROPERTY"),
                "REV04 BATTLE_LOCKED clears rotate residue");
        }

        private static void VerifyRev04RotateZonePrefabRaycastState()
        {
            string yaml = File.ReadAllText(BoardPrefabPath);
            foreach (string fieldName in new[]
                     {
                         "rightRotateZoneImage",
                         "rotateGuideImage"
                     })
            {
                Match reference = Regex.Match(
                    yaml,
                    fieldName + @": \{fileID: ([-0-9]+)\}");
                Require(reference.Success,
                    "REV04_ROTATE_PREFAB_REF_MISSING " + fieldName);
                string marker = "--- !u!114 &" + reference.Groups[1].Value;
                int start = yaml.IndexOf(marker, StringComparison.Ordinal);
                Require(start >= 0,
                    "REV04_ROTATE_PREFAB_COMPONENT_MISSING " + fieldName);
                int end = yaml.IndexOf(
                    "\n--- !u!",
                    start + marker.Length,
                    StringComparison.Ordinal);
                string component = end < 0
                    ? yaml.Substring(start)
                    : yaml.Substring(start, end - start);
                Require(component.IndexOf(
                            "m_RaycastTarget: 0",
                            StringComparison.Ordinal) >= 0,
                    "REV04_ROTATE_PREFAB_RAYCAST_MUST_BE_FALSE "
                    + fieldName);
            }
        }

        private static void VerifyRev04BInputIntentSeparation()
        {
            string presenter = File.ReadAllText(RuntimeSourcePaths[0]);
            string beginInteraction = ExtractMethodBody(
                presenter,
                "private void BeginInteraction(");
            Require(beginInteraction.IndexOf(
                        "OpenSelectedFormalItemDetail(",
                        StringComparison.Ordinal) < 0,
                "REV04B_DRAG_BEGIN_MUST_NOT_OPEN_DETAIL");
            Require(beginInteraction.IndexOf(
                        "selectedItemInstanceId = itemInstanceId;",
                        StringComparison.Ordinal) >= 0,
                "REV04B_DRAG_SELECTION_IDENTITY_MISSING");
            Require(beginInteraction.IndexOf(
                        "PublishCurrent();",
                        StringComparison.Ordinal) < 0,
                "REV07_DRAG_BEGIN_MUST_NOT_REBIND_ACTIVE_CARD");

            string selectBoard = ExtractMethodBody(
                presenter,
                "public void SelectBoardItem(");
            Require(selectBoard.IndexOf(
                        "OpenSelectedFormalItemDetail(",
                        StringComparison.Ordinal) >= 0,
                "REV04B_EXPLICIT_BOARD_CLICK_DETAIL_OPEN_MISSING");
            string selectCard = ExtractMethodBody(
                presenter,
                "public void SelectCard(");
            Require(selectCard.IndexOf(
                        "SelectBoardItem(card.ItemInstanceId);",
                        StringComparison.Ordinal) >= 0,
                "REV04B_EXPLICIT_CARD_CLICK_DETAIL_ROUTE_MISSING");
            foreach (string declaration in new[]
                     {
                         "public void BeginCardDrag(",
                         "public void BeginBoardDrag("
                     })
            {
                Require(ExtractMethodBody(presenter, declaration).IndexOf(
                            "BeginInteraction(",
                            StringComparison.Ordinal) >= 0,
                    "REV04B_MOVEMENT_ROUTE_MISSING " + declaration);
            }
        }

        private static void VerifyRev04ERealTrayScrollRangeArbitration()
        {
            string presenter = File.ReadAllText(RuntimeSourcePaths[0]);
            string tray = File.ReadAllText(RuntimeSourcePaths[2]);
            string card = File.ReadAllText(RuntimeSourcePaths[3]);
            string removedMarker =
                "C1_EXACT_ITEM_POINTER_" + "PAIR_REV04D";

            foreach (string source in new[] { presenter, tray, card })
            foreach (string removedToken in new[]
                     {
                         removedMarker,
                         "EditorPointerDiagnostic",
                         "EditorObserveCard",
                         "EditorDiagnostic",
                         "BuildEditorPointerDiagnostic",
                         "TryFindUniqueBoundCard(",
                         "RETURNED_ATTEMPT_",
                         "OPTIONAL_NOT_OBSERVED"
                     })
                Require(source.IndexOf(
                            removedToken,
                            StringComparison.Ordinal) < 0,
                    "REV04E_TEMP_DIAGNOSTIC_NOT_REMOVED "
                    + removedToken);

            Rect viewportRect = new(-350f, -350f, 700f, 700f);
            Require(!InvokeVerticalScrollRange(
                    viewportRect,
                    new Bounds(
                        Vector3.zero,
                        new Vector3(700f, 700f, 0f))),
                "REV04E_EQUAL_CONTENT_MUST_NOT_SCROLL");
            Require(!InvokeVerticalScrollRange(
                    viewportRect,
                    new Bounds(
                        Vector3.zero,
                        new Vector3(700f, 640f, 0f))),
                "REV04E_SMALLER_CONTENT_MUST_NOT_SCROLL");
            Require(!InvokeVerticalScrollRange(
                    viewportRect,
                    new Bounds(
                        new Vector3(0f, 48f, 0f),
                        new Vector3(700f, 700.5f, 0f))),
                "REV04E_LAYOUT_EPSILON_MUST_NOT_SCROLL");
            Require(InvokeVerticalScrollRange(
                    viewportRect,
                    new Bounds(
                        Vector3.zero,
                        new Vector3(700f, 700.51f, 0f))),
                "REV04E_POSITIVE_OVERFLOW_MUST_SCROLL");
            Require(InvokeVerticalScrollRange(
                    viewportRect,
                    new Bounds(
                        Vector3.zero,
                        new Vector3(700f, 840f, 0f))),
                "REV04E_TRANSFORM_SCALED_OVERFLOW_MUST_SCROLL");
            Require(!InvokeVerticalScrollRange(
                    new Rect(0f, 0f, 700f, 0.5f),
                    new Bounds(
                        Vector3.zero,
                        new Vector3(700f, 840f, 0f))),
                "REV04E_COLLAPSED_VIEWPORT_MUST_FAIL_CLOSED");

            Require(card.IndexOf(
                        "private bool CanScroll()",
                        StringComparison.Ordinal) < 0,
                "REV04E_CONFIGURATION_ONLY_SCROLLABILITY_REMAINS");
            string liveRange = ExtractMethodBody(
                card,
                "private bool HasScrollableVerticalRange()");
            Require(liveRange.IndexOf(
                        "authoredScrollRect.isActiveAndEnabled",
                        StringComparison.Ordinal) >= 0
                    && liveRange.IndexOf(
                        "authoredScrollRect.vertical",
                        StringComparison.Ordinal) >= 0
                    && liveRange.IndexOf(
                        "CalculateRelativeRectTransformBounds(viewport, content)",
                        StringComparison.Ordinal) >= 0
                    && liveRange.IndexOf(
                        "HasScrollableVerticalRange(viewport.rect, contentBounds)",
                        StringComparison.Ordinal) >= 0,
                "REV04E_LIVE_TRANSFORM_AWARE_RANGE_SEAM_MISSING");

            string initialize = ExtractMethodBody(
                card,
                "public void OnInitializePotentialDrag(");
            int initializeRange = initialize.IndexOf(
                "if (HasScrollableVerticalRange())",
                StringComparison.Ordinal);
            int initializeForward = initialize.IndexOf(
                "ExecuteEvents.initializePotentialDrag",
                StringComparison.Ordinal);
            Require(initializeRange >= 0
                    && initializeForward > initializeRange
                    && initialize.IndexOf(
                        "ResetGesture(",
                        StringComparison.Ordinal) < 0
                    && initialize.IndexOf(
                        "ClearPressedRawGrabbedCell(",
                        StringComparison.Ordinal) < 0,
                "REV04E_INITIALIZE_MUST_PRESERVE_RAW_PRESS_AND_GATE_SCROLL");

            string resolve = ExtractMethodBody(
                card,
                "private void ResolvePendingRoute(");
            Require(resolve.IndexOf(
                        "!HasScrollableVerticalRange()"
                        + " || ShouldBeginItemDrag(eventData)",
                        StringComparison.Ordinal) >= 0
                    && resolve.IndexOf(
                        "BeginItemDrag(eventData);",
                        StringComparison.Ordinal) >= 0
                    && resolve.IndexOf(
                        "TryBeginVerticalScroll(eventData);",
                        StringComparison.Ordinal) >= 0,
                "REV04E_ZERO_RANGE_DIRECT_ITEM_ROUTE_MISSING");

            string beginItem = ExtractMethodBody(
                card,
                "private void BeginItemDrag(");
            Require(beginItem.IndexOf(
                        "!hasPressedRawGrabbedCell",
                        StringComparison.Ordinal) >= 0
                    && beginItem.IndexOf(
                        "presenter?.BeginCardDrag(this, eventData);",
                        StringComparison.Ordinal) >= 0,
                "REV04E_ITEM_ROUTE_MUST_PRESERVE_RAW_GRABBED_IDENTITY");

            string beginScroll = ExtractMethodBody(
                card,
                "private void TryBeginVerticalScroll(");
            Require(beginScroll.IndexOf(
                        "!HasScrollableVerticalRange()",
                        StringComparison.Ordinal) >= 0
                    && beginScroll.IndexOf(
                        "Mathf.Abs(delta.y) < Mathf.Abs(delta.x) * ScrollAxisBias",
                        StringComparison.Ordinal) >= 0
                    && beginScroll.IndexOf(
                        "ExecuteEvents.beginDragHandler",
                        StringComparison.Ordinal) >= 0,
                "REV04E_REAL_OVERFLOW_VERTICAL_SCROLL_ROUTE_MISSING");

            string shouldBeginItem = ExtractMethodBody(
                card,
                "private bool ShouldBeginItemDrag(");
            foreach (string intent in new[]
                     {
                         "Time.unscaledTime - pointerDownTime >= LongPressSeconds",
                         "PointerOutsideViewport(eventData, ViewportExitMarginPixels)",
                         "Mathf.Abs(delta.x) >= HorizontalDragThresholdPixels",
                         "Mathf.Abs(delta.x) > Mathf.Abs(delta.y)"
                     })
                Require(shouldBeginItem.IndexOf(
                            intent,
                            StringComparison.Ordinal) >= 0,
                    "REV04E_OVERFLOW_ITEM_INTENT_MISSING " + intent);

            string onDrag = ExtractMethodBody(
                card,
                "public void OnDrag(");
            Require(onDrag.IndexOf(
                        "if (ShouldBeginItemDrag(eventData))",
                        StringComparison.Ordinal) >= 0
                    && onDrag.IndexOf(
                        "EndScroll(eventData);",
                        StringComparison.Ordinal) >= 0
                    && onDrag.IndexOf(
                        "BeginItemDrag(eventData);",
                        StringComparison.Ordinal) >= 0,
                "REV04E_SCROLL_TO_ITEM_TRANSITION_MISSING");
            string endScroll = ExtractMethodBody(
                card,
                "private void EndScroll(");
            Require(endScroll.IndexOf(
                        "!forwardedScrollBegin",
                        StringComparison.Ordinal) >= 0
                    && endScroll.IndexOf(
                        "ExecuteEvents.endDragHandler",
                        StringComparison.Ordinal) >= 0
                    && endScroll.IndexOf(
                        "authoredScrollRect.StopMovement();",
                        StringComparison.Ordinal) >= 0,
                "REV04E_SCROLL_MOMENTUM_STOP_MISSING");

            foreach (string unchangedThreshold in new[]
                     {
                         "LongPressSeconds = 0.22f",
                         "HorizontalDragThresholdPixels = 18f",
                         "ScrollAxisBias = 1.15f",
                         "ViewportExitMarginPixels = 28f"
                     })
                Require(card.IndexOf(
                            unchangedThreshold,
                            StringComparison.Ordinal) >= 0,
                    "REV04E_ACCEPTED_GESTURE_THRESHOLD_CHANGED "
                    + unchangedThreshold);

            string fullSequence = ExtractMethodBody(
                File.ReadAllText(
                    "Assets/_Game/Scripts/TalismanBag/Editor/"
                    + "ItemCampaignBaseline/"
                    + "C1ExactBattleSandboxItemPromotionTests.cs"),
                "public static void RunAllOrThrow()");
            foreach (string required in new[]
                     {
                         "InvokeLiveVerticalScrollRange(reentryCard)",
                         "presenter.SubmitReturnToTray(",
                         "FindBoundCard(tray, i001)",
                         "presenter.BeginCardDrag(reentryCard, null)",
                         "presenter.CommitPreviewedCandidate(",
                         "presenter.LastPreviewCommitCandidateConsistent"
                     })
                Require(fullSequence.IndexOf(
                            required,
                            StringComparison.Ordinal) >= 0,
                    "REV04E_ZERO_RANGE_BOARD_TRAY_BOARD_EVIDENCE_MISSING "
                    + required);
        }

        private static bool InvokeVerticalScrollRange(
            Rect viewportRect,
            Bounds contentBounds)
        {
            const string diagnostic =
                "REV04E_REFLECTION_CARD_VERTICAL_SCROLL_RANGE";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemCardView),
                "HasScrollableVerticalRange",
                true,
                typeof(bool),
                new[] { typeof(Rect), typeof(Bounds) },
                diagnostic);
            return InvokeExactBool(
                method,
                null,
                new object[] { viewportRect, contentBounds },
                diagnostic);
        }

        private static bool InvokeLiveVerticalScrollRange(
            C1ExactBattleSandboxItemCardView card)
        {
            const string diagnostic =
                "REV04E_REFLECTION_CARD_LIVE_VERTICAL_SCROLL_RANGE";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemCardView),
                "HasScrollableVerticalRange",
                false,
                typeof(bool),
                Type.EmptyTypes,
                diagnostic);
            return InvokeExactBool(
                method,
                card,
                Array.Empty<object>(),
                diagnostic);
        }

        private sealed class CandidateProbe
        {
            internal Vector2Int RawGrabbedCell { get; set; }
            internal Vector2Int PointerCell { get; set; }
            internal int VisualQuarterTurns { get; set; }
            internal int FormalDegrees { get; set; }
            internal Vector2Int RotatedMinimum { get; set; }
            internal Vector2Int NormalizedDisplayAnchor { get; set; }
            internal Vector2Int AuthorityAnchor { get; set; }
            internal IReadOnlyList<Vector2Int> OrderedOccupiedCells { get; set; }
            internal bool IsLocallyValid { get; set; }
            internal C1FormalItemPlacementCandidate FormalCandidate { get; set; }
        }

        private static CandidateProbe CreateInteractionCandidate(
            IReadOnlyList<Vector2Int> rawShapeCells,
            Vector2Int rawGrabbedCell,
            Vector2Int pointerCell,
            int visualQuarterTurns,
            IReadOnlyList<Vector2Int> blockingCells,
            int boardSize)
        {
            Type candidateType = typeof(
                    C1ExactBattleSandboxItemArrangementPresenter)
                .Assembly.GetType(
                    "TalismanBag.Items.CampaignBaseline."
                    + "C1ExactBattleSandboxItemInteractionCandidate",
                    true);
            const string diagnostic =
                "REV04_REFLECTION_PRESENTER_TRY_CREATE_INTERACTION_CANDIDATE";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemArrangementPresenter),
                "TryCreateInteractionCandidate",
                true,
                typeof(bool),
                new[]
                {
                    typeof(IReadOnlyList<Vector2Int>),
                    typeof(Vector2Int),
                    typeof(Vector2Int),
                    typeof(int),
                    typeof(IReadOnlyList<Vector2Int>),
                    typeof(int),
                    candidateType.MakeByRefType()
                },
                diagnostic);
            object[] arguments =
            {
                rawShapeCells,
                rawGrabbedCell,
                pointerCell,
                visualQuarterTurns,
                blockingCells,
                boardSize,
                null
            };
            Require(InvokeExactBool(method, null, arguments, diagnostic),
                diagnostic + "_REJECTED");
            object candidate = arguments[6];
            if (candidate == null || candidate.GetType() != candidateType)
                throw new InvalidOperationException(
                    diagnostic + "_OUT_VALUE_MISMATCH");
            return new CandidateProbe
            {
                RawGrabbedCell = ReadExactInternalProperty<Vector2Int>(
                    candidate,
                    candidateType,
                    "RawGrabbedCell",
                    diagnostic + "_RAW_GRABBED_CELL"),
                PointerCell = ReadExactInternalProperty<Vector2Int>(
                    candidate,
                    candidateType,
                    "PointerCell",
                    diagnostic + "_POINTER_CELL"),
                VisualQuarterTurns = ReadExactInternalProperty<int>(
                    candidate,
                    candidateType,
                    "VisualQuarterTurns",
                    diagnostic + "_VISUAL_QUARTER_TURNS"),
                FormalDegrees = ReadExactInternalProperty<int>(
                    candidate,
                    candidateType,
                    "FormalDegrees",
                    diagnostic + "_FORMAL_DEGREES"),
                RotatedMinimum = ReadExactInternalProperty<Vector2Int>(
                    candidate,
                    candidateType,
                    "RotatedMinimum",
                    diagnostic + "_ROTATED_MINIMUM"),
                NormalizedDisplayAnchor =
                    ReadExactInternalProperty<Vector2Int>(
                        candidate,
                        candidateType,
                        "NormalizedDisplayAnchor",
                        diagnostic + "_NORMALIZED_DISPLAY_ANCHOR"),
                AuthorityAnchor = ReadExactInternalProperty<Vector2Int>(
                    candidate,
                    candidateType,
                    "AuthorityAnchor",
                    diagnostic + "_AUTHORITY_ANCHOR"),
                OrderedOccupiedCells =
                    ReadExactInternalProperty<IReadOnlyList<Vector2Int>>(
                        candidate,
                        candidateType,
                        "OrderedOccupiedCells",
                        diagnostic + "_ORDERED_OCCUPIED_CELLS"),
                IsLocallyValid = ReadExactInternalProperty<bool>(
                    candidate,
                    candidateType,
                    "IsLocallyValid",
                    diagnostic + "_LOCAL_LEGALITY"),
                FormalCandidate =
                    ReadExactInternalProperty<C1FormalItemPlacementCandidate>(
                        candidate,
                        candidateType,
                        "FormalCandidate",
                        diagnostic + "_FORMAL_CANDIDATE")
            };
        }

        private static bool InvokeRotateZoneUpdate(
            Type stateType,
            object state,
            Vector2 pointerPosition,
            float now,
            Rect activationRect,
            bool enabled)
        {
            return InvokeExactInternalBool(
                stateType,
                state,
                "Update",
                new[]
                {
                    typeof(Vector2),
                    typeof(float),
                    typeof(Rect),
                    typeof(bool)
                },
                new object[]
                {
                    pointerPosition,
                    now,
                    activationRect,
                    enabled
                });
        }

        private static bool InvokeExactInternalBool(
            Type declaringType,
            object target,
            string methodName,
            Type[] parameterTypes,
            object[] arguments)
        {
            string diagnostic = "REV04_REFLECTION_"
                                + declaringType.Name + "_" + methodName;
            MethodInfo method = RequireExactInternalMethod(
                declaringType,
                methodName,
                false,
                typeof(bool),
                parameterTypes,
                diagnostic);
            return InvokeExactBool(method, target, arguments, diagnostic);
        }

        private static void InvokeExactInternalVoid(
            Type declaringType,
            object target,
            string methodName)
        {
            string diagnostic = "REV04_REFLECTION_"
                                + declaringType.Name + "_" + methodName;
            MethodInfo method = RequireExactInternalMethod(
                declaringType,
                methodName,
                false,
                typeof(void),
                Type.EmptyTypes,
                diagnostic);
            InvokeExact(method, target, Array.Empty<object>(), diagnostic);
        }

        private static void SetExactPrivateField<T>(
            object target,
            string fieldName,
            T value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly);
            if (field == null || field.FieldType != typeof(T))
                throw new InvalidOperationException(
                    "REV04_PRIVATE_FIELD_SIGNATURE_MISMATCH " + fieldName);
            field.SetValue(target, value);
        }

        private static T ReadExactPrivateField<T>(
            object target,
            string fieldName)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly);
            if (field == null || field.FieldType != typeof(T))
                throw new InvalidOperationException(
                    "REV04_PRIVATE_FIELD_SIGNATURE_MISMATCH " + fieldName);
            object value = field.GetValue(target);
            if (value == null && !typeof(T).IsValueType) return default;
            if (!(value is T typed))
                throw new InvalidOperationException(
                    "REV04_PRIVATE_FIELD_VALUE_MISMATCH " + fieldName);
            return typed;
        }

        private static void InvokeExactPrivateVoid(
            object target,
            string methodName,
            Type[] parameterTypes,
            object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly,
                null,
                parameterTypes,
                null);
            if (method == null || method.ReturnType != typeof(void))
                throw new InvalidOperationException(
                    "REV04_PRIVATE_METHOD_SIGNATURE_MISMATCH " + methodName);
            InvokeExact(
                method,
                target,
                arguments,
                "REV04_PRIVATE_METHOD_" + methodName);
        }

        private static bool InvokeExactPrivateBool(
            object target,
            string methodName,
            Type[] parameterTypes,
            object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly,
                null,
                parameterTypes,
                null);
            if (method == null || method.ReturnType != typeof(bool))
                throw new InvalidOperationException(
                    "REV04_PRIVATE_METHOD_SIGNATURE_MISMATCH " + methodName);
            return InvokeExactBool(
                method,
                target,
                arguments,
                "REV04_PRIVATE_METHOD_" + methodName);
        }

        private static void VerifyAuthoritativeCatalogFootprints(
            C1FormalItemSessionSnapshot snapshot,
            Vector2 projectedXAxis,
            Vector2 projectedYAxis)
        {
            Require(snapshot?.itemSystemSnapshot?.catalogItems != null,
                "FORMAL_ITEM_CATALOG_SNAPSHOT_MISSING");
            C1ExactBattleSandboxItemFootprint i001 = CreateCatalogFootprint(
                snapshot,
                "I001",
                projectedXAxis,
                projectedYAxis);
            C1ExactBattleSandboxItemFootprint i002 = CreateCatalogFootprint(
                snapshot,
                "I002",
                projectedXAxis,
                projectedYAxis);
            C1ExactBattleSandboxItemFootprint i031 = CreateCatalogFootprint(
                snapshot,
                "I031",
                projectedXAxis,
                projectedYAxis);
            Equal(1, i001.NormalizedCells.Count,
                "I001 authoritative one-cell shape");
            Equal(1, i001.WidthInCells, "I001 width");
            Equal(1, i001.HeightInCells, "I001 height");
            Equal(2, i002.NormalizedCells.Count,
                "I002 authoritative two-cell shape");
            Equal(1, i002.WidthInCells, "I002 authored vertical width");
            Equal(2, i002.HeightInCells, "I002 authored vertical height");
            Equal(1, i031.NormalizedCells.Count,
                "I031 authoritative one-cell shape");
            Equal(1, i031.WidthInCells, "I031 width");
            Equal(1, i031.HeightInCells, "I031 height");
        }

        private static C1ExactBattleSandboxItemFootprint CreateCatalogFootprint(
            C1FormalItemSessionSnapshot snapshot,
            string itemId,
            Vector2 unitX,
            Vector2 unitY)
        {
            ItemSystemCatalogItemSnapshot catalog = snapshot.itemSystemSnapshot
                .catalogItems.FirstOrDefault(item => string.Equals(
                    item.itemId,
                    itemId,
                    StringComparison.Ordinal));
            Require(catalog != null, "CATALOG_ITEM_MISSING " + itemId);
            return CreateFootprint(catalog.ShapeCells, 0, unitX, unitY);
        }

        private static C1ExactBattleSandboxItemFootprint CreateFootprint(
            IReadOnlyList<Vector2Int> cells,
            int rotation,
            Vector2 unitX,
            Vector2 unitY)
        {
            Require(TryCreateFootprint(
                        cells,
                        rotation,
                        unitX,
                        unitY,
                        out C1ExactBattleSandboxItemFootprint footprint),
                "FOOTPRINT_CREATE_FAILED rotation=" + rotation);
            return footprint;
        }

        private static bool TryGetAuthoredCellWorldVectors(
            C1ExactBattleSandboxItemBoardView board,
            out Vector3 worldXAxis,
            out Vector3 worldYAxis)
        {
            const string diagnostic =
                "REV03_REFLECTION_BOARD_TRY_GET_AUTHORED_CELL_WORLD_VECTORS";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemBoardView),
                "TryGetAuthoredCellWorldVectors",
                false,
                typeof(bool),
                new[]
                {
                    typeof(Vector3).MakeByRefType(),
                    typeof(Vector3).MakeByRefType()
                },
                diagnostic);
            ParameterInfo[] parameters = method.GetParameters();
            if (!parameters[0].IsOut || !parameters[1].IsOut)
                throw new InvalidOperationException(
                    diagnostic + "_OUT_SIGNATURE_MISMATCH");
            object[] arguments = { Vector3.zero, Vector3.zero };
            bool result = InvokeExactBool(method, board, arguments, diagnostic);
            if (!(arguments[0] is Vector3 resolvedXAxis)
                || !(arguments[1] is Vector3 resolvedYAxis))
                throw new InvalidOperationException(
                    diagnostic + "_OUT_VALUE_MISMATCH");
            worldXAxis = resolvedXAxis;
            worldYAxis = resolvedYAxis;
            return result;
        }

        private static RectTransform GetFootprintParent(
            C1ExactBattleSandboxItemTrayView tray)
        {
            return ReadExactInternalProperty<RectTransform>(
                tray,
                typeof(C1ExactBattleSandboxItemTrayView),
                "FootprintParent",
                "REV03_REFLECTION_TRAY_FOOTPRINT_PARENT");
        }

        private static C1ExactBattleSandboxItemCardView FindBoundCard(
            C1ExactBattleSandboxItemTrayView tray,
            string itemInstanceId)
        {
            const string diagnostic =
                "REV03_REFLECTION_TRAY_FIND_BOUND_CARD";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemTrayView),
                "FindBoundCard",
                false,
                typeof(C1ExactBattleSandboxItemCardView),
                new[] { typeof(string) },
                diagnostic);
            object value = InvokeExact(
                method,
                tray,
                new object[] { itemInstanceId },
                diagnostic);
            if (value == null) return null;
            if (!(value is C1ExactBattleSandboxItemCardView card))
                throw new InvalidOperationException(
                    diagnostic + "_RETURN_VALUE_MISMATCH");
            return card;
        }

        private static bool TryGetAuthoredTrayCellCenter(
            C1ExactBattleSandboxItemTrayView tray,
            Vector2Int cell,
            out Vector2 localCenter)
        {
            const string diagnostic =
                "REV04F_REFLECTION_TRAY_TRY_GET_AUTHORED_CELL_CENTER";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemTrayView),
                "TryGetAuthoredCellCenter",
                false,
                typeof(bool),
                new[]
                {
                    typeof(Vector2Int),
                    typeof(Vector2).MakeByRefType()
                },
                diagnostic);
            ParameterInfo[] parameters = method.GetParameters();
            if (!parameters[1].IsOut)
                throw new InvalidOperationException(
                    diagnostic + "_OUT_SIGNATURE_MISMATCH");
            object[] arguments = { cell, Vector2.zero };
            bool result = InvokeExactBool(method, tray, arguments, diagnostic);
            if (!(arguments[1] is Vector2 resolved))
                throw new InvalidOperationException(
                    diagnostic + "_OUT_VALUE_MISMATCH");
            localCenter = resolved;
            return result;
        }

        private static bool TryGetPressedRawGrabbedCell(
            C1ExactBattleSandboxItemCardView card,
            out Vector2Int rawGrabbedCell)
        {
            const string diagnostic =
                "REV04B_REFLECTION_CARD_TRY_GET_PRESSED_RAW_GRABBED_CELL";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemCardView),
                "TryGetPressedRawGrabbedCell",
                false,
                typeof(bool),
                new[] { typeof(Vector2Int).MakeByRefType() },
                diagnostic);
            ParameterInfo[] parameters = method.GetParameters();
            if (!parameters[0].IsOut)
                throw new InvalidOperationException(
                    diagnostic + "_OUT_SIGNATURE_MISMATCH");
            object[] arguments = { Vector2Int.zero };
            bool result = InvokeExactBool(method, card, arguments, diagnostic);
            if (!(arguments[0] is Vector2Int resolved))
                throw new InvalidOperationException(
                    diagnostic + "_OUT_VALUE_MISMATCH");
            rawGrabbedCell = resolved;
            return result;
        }

        private static Vector2 GetCurrentFootprintSize(
            C1ExactBattleSandboxItemCardView card)
        {
            return ReadExactInternalProperty<Vector2>(
                card,
                typeof(C1ExactBattleSandboxItemCardView),
                "CurrentFootprintSize",
                "REV03_REFLECTION_CARD_CURRENT_FOOTPRINT_SIZE");
        }

        private static bool GetArtworkPreservesAspect(
            C1ExactBattleSandboxItemCardView card)
        {
            return ReadExactInternalProperty<bool>(
                card,
                typeof(C1ExactBattleSandboxItemCardView),
                "ArtworkPreservesAspect",
                "REV03_REFLECTION_CARD_ARTWORK_PRESERVES_ASPECT");
        }

        private static bool ValidateLiveFootprintAlignment(
            C1ExactBattleSandboxItemCardView card)
        {
            const string diagnostic =
                "REV03_REFLECTION_CARD_VALIDATE_LIVE_FOOTPRINT_ALIGNMENT";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemCardView),
                "ValidateLiveFootprintAlignment",
                false,
                typeof(bool),
                Type.EmptyTypes,
                diagnostic);
            return InvokeExactBool(method, card, Array.Empty<object>(), diagnostic);
        }

        private static bool GetSourceFootprintLayerActive(
            C1ExactBattleSandboxItemCardView card)
        {
            return ReadExactInternalProperty<bool>(
                card,
                typeof(C1ExactBattleSandboxItemCardView),
                "SourceFootprintLayerActive",
                "REV03_REFLECTION_CARD_SOURCE_FOOTPRINT_LAYER_ACTIVE");
        }

        private static int GetSourceFootprintRaycastCount(
            C1ExactBattleSandboxItemCardView card)
        {
            return ReadExactInternalProperty<int>(
                card,
                typeof(C1ExactBattleSandboxItemCardView),
                "SourceFootprintRaycastCount",
                "REV03_REFLECTION_CARD_SOURCE_FOOTPRINT_RAYCAST_COUNT");
        }

        private static bool TryCreateFootprint(
            IReadOnlyList<Vector2Int> cells,
            int rotation,
            Vector2 unitX,
            Vector2 unitY,
            out C1ExactBattleSandboxItemFootprint footprint)
        {
            const string diagnostic =
                "REV03_REFLECTION_PRESENTER_TRY_CREATE_FOOTPRINT";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemArrangementPresenter),
                "TryCreateFootprint",
                true,
                typeof(bool),
                new[]
                {
                    typeof(IReadOnlyList<Vector2Int>),
                    typeof(int),
                    typeof(Vector2),
                    typeof(Vector2),
                    typeof(C1ExactBattleSandboxItemFootprint).MakeByRefType()
                },
                diagnostic);
            ParameterInfo[] parameters = method.GetParameters();
            if (!parameters[4].IsOut)
                throw new InvalidOperationException(
                    diagnostic + "_OUT_SIGNATURE_MISMATCH");
            object[] arguments = { cells, rotation, unitX, unitY, null };
            bool result = InvokeExactBool(method, null, arguments, diagnostic);
            if (arguments[4] != null
                && !(arguments[4] is C1ExactBattleSandboxItemFootprint))
                throw new InvalidOperationException(
                    diagnostic + "_OUT_VALUE_MISMATCH");
            footprint = arguments[4] as C1ExactBattleSandboxItemFootprint;
            if (result && footprint == null)
                throw new InvalidOperationException(
                    diagnostic + "_SUCCESS_WITH_NULL_FOOTPRINT");
            return result;
        }

        private static bool TryResolveFootprintRawCell(
            C1ExactBattleSandboxItemFootprint footprint,
            Vector2Int normalizedCell,
            out Vector2Int rawCell)
        {
            const string diagnostic =
                "REV04_REFLECTION_FOOTPRINT_TRY_RESOLVE_RAW_CELL";
            MethodInfo method = RequireExactInternalMethod(
                typeof(C1ExactBattleSandboxItemFootprint),
                "TryResolveRawCell",
                false,
                typeof(bool),
                new[]
                {
                    typeof(Vector2Int),
                    typeof(Vector2Int).MakeByRefType()
                },
                diagnostic);
            ParameterInfo[] parameters = method.GetParameters();
            if (!parameters[1].IsOut)
                throw new InvalidOperationException(
                    diagnostic + "_OUT_SIGNATURE_MISMATCH");
            object[] arguments = { normalizedCell, Vector2Int.zero };
            bool result = InvokeExactBool(
                method,
                footprint,
                arguments,
                diagnostic);
            if (!(arguments[1] is Vector2Int resolved))
                throw new InvalidOperationException(
                    diagnostic + "_OUT_VALUE_MISMATCH");
            rawCell = resolved;
            return result;
        }

        private static T InvokeInternalConstructor<T>(
            Type[] signature,
            params object[] arguments)
        {
            ConstructorInfo constructor = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                signature,
                null);
            string diagnostic = "REFLECTION_CONSTRUCTOR_"
                                + typeof(T).Name.ToUpperInvariant();
            Require(constructor != null,
                diagnostic + "_SIGNATURE_MISSING");
            try
            {
                return (T)constructor.Invoke(arguments);
            }
            catch (TargetInvocationException exception)
            {
                throw new InvalidOperationException(
                    diagnostic + "_INVOKE_FAILED",
                    exception.InnerException ?? exception);
            }
        }

        private static MethodInfo RequireExactInternalMethod(
            Type declaringType,
            string memberName,
            bool isStatic,
            Type returnType,
            Type[] parameterTypes,
            string diagnostic)
        {
            BindingFlags flags = BindingFlags.NonPublic
                                 | BindingFlags.DeclaredOnly
                                 | (isStatic
                                     ? BindingFlags.Static
                                     : BindingFlags.Instance);
            MethodInfo method = declaringType.GetMethod(
                memberName,
                flags,
                null,
                parameterTypes,
                null);
            if (method == null
                || method.DeclaringType != declaringType
                || method.ReturnType != returnType
                || method.IsStatic != isStatic
                || method.IsPublic
                || method.IsGenericMethodDefinition)
                throw new InvalidOperationException(
                    diagnostic + "_MISSING_OR_SIGNATURE_MISMATCH");
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != parameterTypes.Length
                || parameters.Where((parameter, index) =>
                        parameter.ParameterType != parameterTypes[index])
                    .Any())
                throw new InvalidOperationException(
                    diagnostic + "_PARAMETER_SIGNATURE_MISMATCH");
            return method;
        }

        private static T ReadExactInternalProperty<T>(
            object target,
            Type declaringType,
            string memberName,
            string diagnostic)
        {
            if (target == null || target.GetType() != declaringType)
                throw new InvalidOperationException(
                    diagnostic + "_TARGET_TYPE_MISMATCH");
            BindingFlags flags = BindingFlags.Instance
                                 | BindingFlags.NonPublic
                                 | BindingFlags.DeclaredOnly;
            PropertyInfo property = declaringType.GetProperty(memberName, flags);
            MethodInfo getter = property?.GetGetMethod(true);
            if (property == null
                || property.DeclaringType != declaringType
                || property.PropertyType != typeof(T)
                || property.GetIndexParameters().Length != 0
                || getter == null
                || getter.IsPublic
                || getter.IsStatic)
                throw new InvalidOperationException(
                    diagnostic + "_MISSING_OR_SIGNATURE_MISMATCH");
            object value;
            try
            {
                value = property.GetValue(target, null);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    diagnostic + "_INVOKE_FAILED",
                    UnwrapInvocationException(exception));
            }

            if (value == null)
            {
                if (!typeof(T).IsValueType) return default;
                throw new InvalidOperationException(
                    diagnostic + "_NULL_VALUE_MISMATCH");
            }

            if (!(value is T typedValue))
                throw new InvalidOperationException(
                    diagnostic + "_VALUE_TYPE_MISMATCH");
            return typedValue;
        }

        private static bool InvokeExactBool(
            MethodInfo method,
            object target,
            object[] arguments,
            string diagnostic)
        {
            object value = InvokeExact(method, target, arguments, diagnostic);
            if (!(value is bool result))
                throw new InvalidOperationException(
                    diagnostic + "_RETURN_VALUE_MISMATCH");
            return result;
        }

        private static object InvokeExact(
            MethodInfo method,
            object target,
            object[] arguments,
            string diagnostic)
        {
            if (!method.IsStatic
                && (target == null || target.GetType() != method.DeclaringType))
                throw new InvalidOperationException(
                    diagnostic + "_TARGET_TYPE_MISMATCH");
            try
            {
                return method.Invoke(target, arguments);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    diagnostic + "_INVOKE_FAILED",
                    UnwrapInvocationException(exception));
            }
        }

        private static Exception UnwrapInvocationException(Exception exception)
        {
            return exception is TargetInvocationException invocation
                   && invocation.InnerException != null
                ? invocation.InnerException
                : exception;
        }

        private static string ExtractMethodBody(
            string source,
            string declaration)
        {
            int declarationIndex = source.IndexOf(
                declaration,
                StringComparison.Ordinal);
            Require(declarationIndex >= 0,
                "REV04B_METHOD_DECLARATION_MISSING " + declaration);
            int open = source.IndexOf('{', declarationIndex);
            Require(open >= 0,
                "REV04B_METHOD_BODY_MISSING " + declaration);
            int depth = 0;
            for (int index = open; index < source.Length; index++)
            {
                if (source[index] == '{') depth++;
                else if (source[index] == '}')
                {
                    depth--;
                    if (depth == 0)
                        return source.Substring(open, index - open + 1);
                }
            }

            throw new InvalidOperationException(
                "REV04B_METHOD_BODY_UNTERMINATED " + declaration);
        }

        private static string CellSignature(IReadOnlyList<Vector2Int> cells)
        {
            return string.Join("|", cells.Select(cell => cell.x + ":" + cell.y));
        }

        private static int CountRuntimeFallbackNames(params GameObject[] roots)
        {
            string[] forbiddenNames =
            {
                "RuntimeRoot",
                "RuntimeFallback",
                "PlacementFeedback_Runtime"
            };
            return roots.Where(root => root != null)
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Count(value => forbiddenNames.Contains(
                    value.name,
                    StringComparer.Ordinal));
        }

        private static int CountTransforms(Transform root)
        {
            return root == null ? 0 : root.GetComponentsInChildren<Transform>(true).Length;
        }

        private static int CountPersistentButtonListeners(GameObject root)
        {
            return root.GetComponentsInChildren<Button>(true)
                .Sum(button => button.onClick.GetPersistentEventCount());
        }

        private static void Require(bool condition, string message)
        {
            assertionCount++;
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Equal<T>(T expected, T actual, string label)
        {
            assertionCount++;
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(label + " expected="
                    + expected + " actual=" + actual);
            }
        }

        private static void Approximately(
            Vector2 expected,
            Vector2 actual,
            string label)
        {
            assertionCount++;
            if (Vector2.SqrMagnitude(expected - actual) > 0.0001f)
            {
                throw new InvalidOperationException(label + " expected="
                    + expected + " actual=" + actual);
            }
        }
    }
}
