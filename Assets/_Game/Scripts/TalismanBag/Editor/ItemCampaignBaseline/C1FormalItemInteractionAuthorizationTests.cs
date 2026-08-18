using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TalismanBag.Items.CampaignBaseline;
using TalismanBag.Items.Canonical;

namespace TalismanBag.Editor.ItemCampaignBaseline
{
    public static class C1FormalItemInteractionAuthorizationTests
    {
        public const string TerminalMarker =
            "C1_FORMAL_ITEM_INTERACTION_AUTHORIZATION_FOCUSED_TESTS_PASS";

        private const string RuntimeRoot =
            "Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/";
        private const string PresenterPath = RuntimeRoot
            + "C1ExactBattleSandboxItemArrangementPresenter.cs";
        private const string BoardPath = RuntimeRoot
            + "C1ExactBattleSandboxItemBoardView.cs";
        private const string TrayPath = RuntimeRoot
            + "C1ExactBattleSandboxItemTrayView.cs";
        private const string CardPath = RuntimeRoot
            + "C1ExactBattleSandboxItemCardView.cs";
        private const string AuthorizationPath = RuntimeRoot
            + "C1FormalItemInteractionAuthorization.cs";
        private const string UseSnapshotValue = "\u0001";

        private static int assertionCount;

        public static int Main()
        {
            return RunFocused();
        }

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
                    "C1_FORMAL_ITEM_INTERACTION_AUTHORIZATION_FOCUSED_TESTS_FAIL "
                    + exception);
                return 1;
            }
        }

        public static void RunAllOrThrow()
        {
            assertionCount = 0;
            VerifyExactPublicSurface();
            VerifyDefaultLockedAndValidEnable();
            VerifyValidationAndFailClosedMatrix();
            VerifyResetInvalidatesStaleEnable();
            VerifyActiveInteractionCancellationAndStaleCallbacks();
            VerifyBoardDragBindResetLifecycle();
            VerifyInputGateAndScrollStaticMatrix();
            VerifyEnabledRotationAndDetailRegressionBoundaries();
            VerifyForbiddenRuntimeBoundary();
        }

        private static void VerifyExactPublicSurface()
        {
            string[] names = Enum.GetNames(typeof(C1FormalItemInteractionMode));
            Equal(2, names.Length, "exact interaction mode count");
            Equal("BATTLE_LOCKED", names[0], "locked mode name");
            Equal("PREPARE_ENABLED", names[1], "enabled mode name");
            Equal(0, (int)C1FormalItemInteractionMode.BATTLE_LOCKED,
                "locked mode value");
            Equal(1, (int)C1FormalItemInteractionMode.PREPARE_ENABLED,
                "enabled mode value");

            Type presenter = typeof(
                C1ExactBattleSandboxItemArrangementPresenter);
            PropertyInfo current = presenter.GetProperty(
                "CurrentInteractionAuthorization",
                BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.DeclaredOnly);
            Require(current != null && current.CanRead && !current.CanWrite
                    && current.PropertyType
                    == typeof(C1FormalItemInteractionAuthorizationResult),
                "PRESENTER_AUTHORIZATION_CURRENT_SURFACE_MISSING");
            MethodInfo apply = presenter.GetMethod(
                "ApplyInteractionAuthorization",
                BindingFlags.Instance | BindingFlags.Public
                | BindingFlags.DeclaredOnly,
                null,
                new[]
                {
                    typeof(C1FormalItemInteractionAuthorizationRequest)
                },
                null);
            Require(apply != null && apply.ReturnType
                    == typeof(C1FormalItemInteractionAuthorizationResult),
                "PRESENTER_AUTHORIZATION_APPLY_SURFACE_MISSING");
            PropertyInfo internalEnabled = presenter.GetProperty(
                "IsItemInteractionEnabled",
                BindingFlags.Instance | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly);
            Require(internalEnabled != null
                    && internalEnabled.PropertyType == typeof(bool),
                "PRESENTER_INTERNAL_GATE_MISSING");
            Require(presenter.GetProperty(
                    "IsItemInteractionEnabled",
                    BindingFlags.Instance | BindingFlags.Public) == null,
                "PRESENTER_INTERNAL_GATE_MUST_NOT_BE_PUBLIC");
        }

        private static void VerifyDefaultLockedAndValidEnable()
        {
            C1FormalItemSessionAuthority authority = CreateAuthority(
                "authorization-default");
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                CreatePresenter(authority);
            C1FormalItemInteractionAuthorizationResult initial =
                presenter.CurrentInteractionAuthorization;
            Require(!initial.accepted && !initial.changed
                    && !initial.interactionEnabled,
                "DEFAULT_STATE_MUST_FAIL_CLOSED");
            Equal(C1FormalItemInteractionMode.BATTLE_LOCKED,
                initial.resolvedMode,
                "default resolved mode");

            C1FormalItemInteractionAuthorizationRequest enable =
                C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                    authority.Current,
                    C1FormalItemInteractionMode.PREPARE_ENABLED);
            C1FormalItemInteractionAuthorizationResult accepted =
                presenter.ApplyInteractionAuthorization(enable);
            Require(accepted.accepted && accepted.changed
                    && accepted.interactionEnabled,
                "VALID_ENABLE_MUST_BE_ACCEPTED");
            Equal(authority.Current.sessionToken,
                accepted.sourceSessionToken,
                "enabled source token");
            Equal(authority.Current.resetGeneration,
                accepted.sourceResetGeneration,
                "enabled source generation");
            Equal(authority.Current.canonicalSignature,
                accepted.sourceSessionCanonicalSignature,
                "enabled source signature");
            Require(!string.IsNullOrWhiteSpace(accepted.sourceLineage)
                    && !string.IsNullOrWhiteSpace(accepted.canonicalSignature),
                "ENABLED_RESULT_SIGNATURES_REQUIRED");
            Require(InvokePresenterGate(presenter),
                "CENTRAL_GATE_MUST_OPEN_AFTER_VALID_ENABLE");

            C1FormalItemInteractionAuthorizationResult duplicate =
                presenter.ApplyInteractionAuthorization(enable);
            Require(duplicate.accepted && !duplicate.changed
                    && duplicate.interactionEnabled,
                "DUPLICATE_ENABLE_MUST_BE_ACCEPTED_NO_OP");
            Equal(C1FormalItemInteractionAuthorizationDiagnostics
                    .DuplicateAcceptedNoOp,
                duplicate.diagnostic,
                "duplicate diagnostic");

            C1ExactBattleSandboxItemArrangementPresenter second =
                CreatePresenter(authority);
            C1FormalItemInteractionAuthorizationResult deterministic =
                second.ApplyInteractionAuthorization(enable);
            Equal(accepted.canonicalSignature,
                deterministic.canonicalSignature,
                "deterministic accepted result signature");
            Equal(enable.canonicalSignature,
                C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                    authority.Current,
                    C1FormalItemInteractionMode.PREPARE_ENABLED)
                    .canonicalSignature,
                "deterministic request signature");
        }

        private static void VerifyValidationAndFailClosedMatrix()
        {
            C1FormalItemSessionAuthority authority = CreateAuthority(
                "authorization-rejections");
            C1FormalItemSessionSnapshot snapshot = authority.Current;
            VerifyRejectedLocks(authority, null,
                C1FormalItemInteractionAuthorizationDiagnostics.RequestNull);
            VerifyRejectedLocks(authority, Request(
                    snapshot,
                    productContext: "DEV_SHOWCASE_LV40"),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .ProductContextMismatch);
            VerifyRejectedLocks(authority, Request(snapshot, sessionToken: ""),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .SessionTokenRequired);
            VerifyRejectedLocks(authority, Request(
                    snapshot,
                    sessionToken: snapshot.sessionToken + ".stale"),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .SessionTokenMismatch);
            VerifyRejectedLocks(authority, Request(snapshot, resetGeneration: 0L),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .ResetGenerationInvalid);
            VerifyRejectedLocks(authority, Request(
                    snapshot,
                    resetGeneration: snapshot.resetGeneration + 1L),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .ResetGenerationMismatch);
            VerifyRejectedLocks(authority, Request(
                    snapshot,
                    expectedSignature: ""),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .ExpectedSessionSignatureRequired);
            VerifyRejectedLocks(authority, Request(
                    snapshot,
                    expectedSignature: snapshot.canonicalSignature + ".stale"),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .ExpectedSessionSignatureMismatch);
            VerifyRejectedLocks(authority, Request(
                    snapshot,
                    mode: (C1FormalItemInteractionMode)99),
                C1FormalItemInteractionAuthorizationDiagnostics
                    .ModeUnsupported);

            C1ExactBattleSandboxItemArrangementPresenter presenter =
                CreatePresenter(authority);
            Enable(presenter, snapshot);
            C1FormalItemInteractionAuthorizationResult locked =
                presenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        snapshot,
                        C1FormalItemInteractionMode.BATTLE_LOCKED));
            Require(locked.accepted && locked.changed
                    && !locked.interactionEnabled,
                "VALID_LOCK_MUST_CLOSE_GATE");
            Require(!InvokePresenterGate(presenter),
                "CENTRAL_GATE_MUST_CLOSE_AFTER_VALID_LOCK");
            C1FormalItemInteractionAuthorizationResult duplicateLock =
                presenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        snapshot,
                        C1FormalItemInteractionMode.BATTLE_LOCKED));
            Require(duplicateLock.accepted && !duplicateLock.changed,
                "DUPLICATE_LOCK_MUST_BE_ACCEPTED_NO_OP");
        }

        private static void VerifyResetInvalidatesStaleEnable()
        {
            C1FormalItemSessionAuthority authority = CreateAuthority(
                "authorization-reset");
            C1FormalItemInteractionAuthorizationRequest stale =
                C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                    authority.Current,
                    C1FormalItemInteractionMode.PREPARE_ENABLED);
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                CreatePresenter(authority);
            Enable(presenter, authority.Current);
            C1FormalItemSessionSnapshot beforeReset = authority.Current;
            C1FormalItemSessionOperationResult reset = authority.Reset(
                new C1FormalItemResetCommand(
                    beforeReset.sessionToken,
                    beforeReset.resetGeneration,
                    "authorization-reset-command",
                    beforeReset.canonicalSignature));
            Require(reset.accepted && reset.changed
                    && authority.Current.resetGeneration
                    == beforeReset.resetGeneration + 1L,
                "FORMAL_RESET_FIXTURE_FAILED");
            C1FormalItemInteractionAuthorizationResult rejected =
                presenter.ApplyInteractionAuthorization(stale);
            Require(!rejected.accepted && rejected.changed
                    && !rejected.interactionEnabled,
                "STALE_ENABLE_AFTER_RESET_MUST_LOCK");
            Equal(C1FormalItemInteractionAuthorizationDiagnostics
                    .ResetGenerationMismatch,
                rejected.diagnostic,
                "stale reset diagnostic");
        }

        private static void VerifyActiveInteractionCancellationAndStaleCallbacks()
        {
            C1FormalItemSessionAuthority authority = CreateAuthority(
                "authorization-cancel");
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                CreatePresenter(authority);
            Enable(presenter, authority.Current);
            string instanceId = CanonicalInitialItemAcquisitionPolicy.ItemInstanceId;
            SetPrivateField(presenter, "activeItemInstanceId", instanceId);
            SetPrivateField(presenter, "activeExpectedSignature",
                authority.Current.canonicalSignature);
            SetPrivateField(presenter, "activeRotation", 3);
            SetPrivateField(presenter, "activeCandidateLocallyValid", true);
            SetPrivateField(presenter, "rotateZoneLatched", true);
            SetPrivateField(presenter, "activeCandidate",
                new C1FormalItemPlacementCandidate(
                    new UnityEngine.Vector2Int(0, 0),
                    270));
            Require(presenter.HasActiveInteraction,
                "ACTIVE_INTERACTION_FIXTURE_NOT_STAGED");
            string authoritySignature = authority.Current.canonicalSignature;

            C1FormalItemInteractionAuthorizationResult locked =
                presenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        authority.Current,
                        C1FormalItemInteractionMode.BATTLE_LOCKED));
            Require(locked.accepted && locked.changed,
                "ACTIVE_DRAG_LOCK_REJECTED");
            Require(!presenter.HasActiveInteraction,
                "ACTIVE_ITEM_ID_NOT_CLEARED_ON_LOCK");
            Equal(string.Empty,
                GetPrivateField<string>(presenter, "activeExpectedSignature"),
                "active expected signature cleared");
            Equal(0, GetPrivateField<int>(presenter, "activeRotation"),
                "active rotation cleared");
            Require(!GetPrivateField<bool>(
                    presenter,
                    "activeCandidateLocallyValid")
                    && !GetPrivateField<bool>(presenter, "rotateZoneLatched")
                    && GetPrivateField<object>(presenter, "activeCandidate") == null,
                "ACTIVE_PREVIEW_STATE_NOT_CLEARED_ON_LOCK");

            presenter.EndBoardDrag(instanceId, null);
            presenter.EndCardDrag(null, null);
            Require(!presenter.CommitPreviewedCandidate(out string diagnostic),
                "STALE_COMMIT_MUST_REJECT_WHILE_LOCKED");
            Equal(C1FormalItemInteractionAuthorizationDiagnostics
                    .InteractionLocked,
                diagnostic,
                "stale commit diagnostic");
            Require(!presenter.SubmitReturnToTray(instanceId, out diagnostic),
                "STALE_RETURN_MUST_REJECT_WHILE_LOCKED");
            Require(!presenter.PreviewCandidate(
                    instanceId,
                    new UnityEngine.Vector2Int(0, 0),
                    0,
                    out bool locallyValid)
                    && !locallyValid,
                "STALE_PREVIEW_MUST_REJECT_WHILE_LOCKED");
            Equal(authoritySignature,
                authority.Current.canonicalSignature,
                "locked stale callbacks must not mutate placement");
        }

        private static void VerifyBoardDragBindResetLifecycle()
        {
            C1FormalItemSessionAuthority authority = CreateAuthority(
                "board-drag-bind-reset");
            C1FormalItemSessionSnapshot snapshot = authority.Current;
            C1ExactBattleSandboxItemBoardView board = CreateBoardView();
            string instanceId = CanonicalInitialItemAcquisitionPolicy.ItemInstanceId;

            SetPrivateField(board, "snapshot", snapshot);
            SetPrivateField(board, "pressedItemInstanceId", instanceId);
            SetPrivateField(board, "activeBoardDragItemInstanceId",
                string.Empty);
            InvokePrivateMethod(
                board,
                "CapturePressedIdentityForActiveBoardDrag",
                Type.EmptyTypes,
                Array.Empty<object>());
            Equal(instanceId,
                GetPrivateField<string>(
                    board,
                    "activeBoardDragItemInstanceId"),
                "begin drag captures stable board identity");
            InvokePrivateMethod(
                board,
                "PreparePointerStateForBind",
                new[] { typeof(C1FormalItemSessionSnapshot) },
                new object[] { snapshot });
            Equal(string.Empty,
                GetPrivateField<string>(board, "pressedItemInstanceId"),
                "benign bind clears transient press identity");
            Equal(instanceId,
                GetPrivateField<string>(
                    board,
                    "activeBoardDragItemInstanceId"),
                "benign bind preserves stable board drag identity");

            SetPrivateField(board, "pressedItemInstanceId", instanceId);
            SetPrivateField(board, "activeBoardDragItemInstanceId", instanceId);
            InvokePrivateMethod(
                board,
                "ClearPointerIdentities",
                Type.EmptyTypes,
                Array.Empty<object>());
            VerifyBoardPointerIdentitiesCleared(
                board,
                "explicit cancel");

            C1ExactBattleSandboxItemArrangementPresenter presenter =
                CreatePresenter(authority);
            Enable(presenter, authority.Current);
            SetPrivateField(board, "pressedItemInstanceId", instanceId);
            SetPrivateField(board, "activeBoardDragItemInstanceId", instanceId);
            C1FormalItemInteractionAuthorizationResult locked =
                presenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        authority.Current,
                        C1FormalItemInteractionMode.BATTLE_LOCKED));
            Require(locked.accepted && !locked.interactionEnabled,
                "BOARD_DRAG_LOCK_FIXTURE_REJECTED");
            InvokePrivateMethod(
                board,
                "ClearPointerIdentities",
                Type.EmptyTypes,
                Array.Empty<object>());
            VerifyBoardPointerIdentitiesCleared(board, "battle lock");

            Enable(presenter, authority.Current);
            SetPrivateField(board, "pressedItemInstanceId", instanceId);
            SetPrivateField(board, "activeBoardDragItemInstanceId", instanceId);
            C1FormalItemInteractionAuthorizationResult rejected =
                presenter.ApplyInteractionAuthorization(Request(
                    authority.Current,
                    sessionToken: "stale-board-drag-session"));
            Require(!rejected.accepted && !rejected.interactionEnabled,
                "BOARD_DRAG_REJECTED_AUTHORIZATION_MUST_FAIL_CLOSED");
            InvokePrivateMethod(
                board,
                "ClearPointerIdentities",
                Type.EmptyTypes,
                Array.Empty<object>());
            VerifyBoardPointerIdentitiesCleared(
                board,
                "rejected authorization");

            SetPrivateField(board, "pressedItemInstanceId", instanceId);
            SetPrivateField(board, "activeBoardDragItemInstanceId", instanceId);
            InvokePrivateMethod(
                board,
                "ClearPointerIdentities",
                Type.EmptyTypes,
                Array.Empty<object>());
            VerifyBoardPointerIdentitiesCleared(board, "unbind");

            C1FormalItemSessionSnapshot beforeReset = authority.Current;
            SetPrivateField(board, "snapshot", beforeReset);
            SetPrivateField(board, "pressedItemInstanceId", instanceId);
            SetPrivateField(board, "activeBoardDragItemInstanceId", instanceId);
            C1FormalItemSessionOperationResult reset = authority.Reset(
                new C1FormalItemResetCommand(
                    beforeReset.sessionToken,
                    beforeReset.resetGeneration,
                    "board-drag-reset-command",
                    beforeReset.canonicalSignature));
            Require(reset.accepted && reset.changed,
                "BOARD_DRAG_SESSION_RESET_FIXTURE_FAILED");
            InvokePrivateMethod(
                board,
                "PreparePointerStateForBind",
                new[] { typeof(C1FormalItemSessionSnapshot) },
                new object[] { authority.Current });
            VerifyBoardPointerIdentitiesCleared(board, "session reset bind");

            presenter = CreatePresenter(authority);
            Enable(presenter, authority.Current);
            SetPrivateField(presenter, "activeItemInstanceId",
                "mismatched-board-drag-instance");
            SetPrivateField(board, "pressedItemInstanceId", instanceId);
            SetPrivateField(board, "activeBoardDragItemInstanceId", instanceId);
            string authoritySignature = authority.Current.canonicalSignature;
            presenter.UpdateBoardDrag(instanceId, null);
            Equal(instanceId,
                GetPrivateField<string>(
                    board,
                    "activeBoardDragItemInstanceId"),
                "stale drag remains local until end");
            Equal(authoritySignature,
                authority.Current.canonicalSignature,
                "stale drag must not mutate authority");
            presenter.EndBoardDrag(instanceId, null);
            InvokePrivateMethod(
                board,
                "ClearPointerIdentities",
                Type.EmptyTypes,
                Array.Empty<object>());
            VerifyBoardPointerIdentitiesCleared(board, "stale end drag");
            Equal(authoritySignature,
                authority.Current.canonicalSignature,
                "stale end drag must not mutate authority");
        }

        private static void VerifyInputGateAndScrollStaticMatrix()
        {
            string presenter = ReadRequiredSource(PresenterPath);
            foreach (string method in new[]
                     {
                         "public void SelectCard(",
                         "public void SelectBoardItem(",
                         "public void BeginCardDrag(",
                         "public void UpdateCardDrag(",
                         "public void EndCardDrag(",
                         "public void BeginBoardDrag(",
                         "public void UpdateBoardDrag(",
                         "public void EndBoardDrag(",
                         "public bool PreviewCandidate("
                     })
                RequireMethodContains(
                    presenter,
                    method,
                    "RejectUnlessInteractionEnabled()",
                    "PRESENTER_GATE_MISSING");
            RequireMethodContains(
                presenter,
                "public bool CommitPreviewedCandidate(",
                "if (!IsItemInteractionEnabled)",
                "PRESENTER_COMMIT_GATE_MISSING");
            RequireMethodContains(
                presenter,
                "public bool SubmitReturnToTray(",
                "if (!IsItemInteractionEnabled)",
                "PRESENTER_RETURN_GATE_MISSING");
            RequireMethodContains(
                presenter,
                "private bool SubmitExactlyOne(",
                "if (!IsItemInteractionEnabled || authority == null)",
                "PRESENTER_COMMAND_GATE_MISSING");

            string board = ReadRequiredSource(BoardPath);
            foreach (string method in new[]
                     {
                         "public void OnPointerDown(",
                         "public void OnPointerClick(",
                         "public void OnBeginDrag(",
                         "public void OnDrag(",
                         "public void OnEndDrag("
                     })
                RequireMethodContains(
                    board,
                    method,
                    "presenter.IsItemInteractionEnabled",
                    "BOARD_LOCAL_GATE_MISSING");
            RequireMethodContains(
                board,
                "internal void CancelInteractionState(",
                "ClearPointerIdentities();",
                "BOARD_POINTER_STATE_CANCEL_MISSING");
            RequireMethodContains(
                board,
                "internal void CancelInteractionState(",
                "ClearInteractionFeedback();",
                "BOARD_FEEDBACK_CANCEL_MISSING");
            RequireMethodContains(
                board,
                "internal void BindPresenter(",
                "CancelInteractionState();",
                "BOARD_UNBIND_OR_REBIND_CANCEL_MISSING");
            RequireMethodContains(
                board,
                "internal void Bind(",
                "PreparePointerStateForBind(configuredSnapshot);",
                "BOARD_BIND_POINTER_STATE_POLICY_MISSING");
            RequireMethodContains(
                board,
                "private void PreparePointerStateForBind(",
                "if (!HasSameSessionIdentity(snapshot, configuredSnapshot))",
                "BOARD_BIND_SESSION_RESET_POLICY_MISSING");
            RequireMethodContains(
                board,
                "public void OnDrag(",
                "activeBoardDragItemInstanceId",
                "BOARD_DRAG_STABLE_IDENTITY_MISSING");
            RequireMethodContains(
                board,
                "public void OnEndDrag(",
                "ClearPointerIdentities();",
                "BOARD_END_DRAG_CLEAR_MISSING");
            RequireMethodContainsInOrder(
                board,
                "public void OnBeginDrag(",
                new[]
                {
                    "CapturePressedIdentityForActiveBoardDrag();",
                    "presenter.BeginBoardDrag("
                },
                "BOARD_BEGIN_DRAG_CAPTURE_ORDER_INVALID");
            RequireMethodContainsInOrder(
                board,
                "public void OnEndDrag(",
                new[]
                {
                    "presenter.EndBoardDrag(",
                    "ClearPointerIdentities();"
                },
                "BOARD_END_DRAG_FORWARD_CLEAR_ORDER_INVALID");
            RequireMethodContains(
                presenter,
                "ApplyInteractionAuthorization(",
                "if (!result.interactionEnabled) CancelAllInteractionResidue();",
                "PRESENTER_REJECTED_AUTHORIZATION_CANCEL_MISSING");
            RequireMethodContains(
                presenter,
                "public void UpdateBoardDrag(",
                "activeItemInstanceId",
                "PRESENTER_STALE_BOARD_DRAG_GUARD_MISSING");
            RequireMethodContains(
                presenter,
                "public void EndBoardDrag(",
                "activeItemInstanceId",
                "PRESENTER_STALE_BOARD_END_GUARD_MISSING");

            string card = ReadRequiredSource(CardPath);
            string cardPointerDown = ExtractMethodBody(
                card,
                "public void OnPointerDown(");
            Require(cardPointerDown.IndexOf(
                        "suppressClick = !interactionEnabled || !HasBoundItem;",
                        StringComparison.Ordinal) >= 0,
                "CARD_CLICK_MUST_USE_CARD_BINDING_NOT_DRAG_CELL_HIT_TEST");
            RequireMethodContains(card, "public void OnPointerClick(",
                "if (!ItemInteractionEnabled)",
                "CARD_CLICK_GATE_MISSING");
            string cardClick = ExtractMethodBody(
                card,
                "public void OnPointerClick(");
            Require(cardClick.IndexOf(
                        "presenter?.SelectCard(this);",
                        StringComparison.Ordinal) >= 0,
                "CARD_CLICK_DETAIL_ROUTE_MISSING");
            Require(cardClick.IndexOf(
                        "TryResolveRawGrabbedCell",
                        StringComparison.Ordinal) < 0,
                "CARD_CLICK_MUST_NOT_REUSE_DRAG_CELL_HIT_TEST");
            RequireMethodContains(card, "private void BeginItemDrag(",
                "if (!ItemInteractionEnabled)",
                "CARD_ITEM_DRAG_GATE_MISSING");
            RequireMethodContains(card, "public void OnDrag(",
                "dragRoute == DragRoute.Item && ItemInteractionEnabled",
                "CARD_UPDATE_DRAG_GATE_MISSING");
            RequireMethodContains(card, "public void OnEndDrag(",
                "dragRoute == DragRoute.Item && ItemInteractionEnabled",
                "CARD_END_DRAG_GATE_MISSING");
            RequireMethodContains(card, "private void ResolvePendingRoute(",
                "TryBeginVerticalScroll(eventData);",
                "LOCKED_VERTICAL_SCROLL_ROUTE_MISSING");
            RequireMethodContains(card, "private void TryBeginVerticalScroll(",
                "ExecuteEvents.beginDragHandler",
                "TRAY_SCROLL_FORWARDING_MISSING");
            RequireMethodContains(card, "internal void CancelInteractionState(",
                "SetDragging(false);",
                "CARD_DRAG_VISUAL_CANCEL_MISSING");
            RequireMethodContains(card, "internal void CancelInteractionState(",
                "suppressClick = true;",
                "CARD_STALE_CLICK_CANCEL_MISSING");

            string tray = ReadRequiredSource(TrayPath);
            RequireMethodContains(tray, "internal void CancelItemInteractions(",
                "card.CancelInteractionState();",
                "TRAY_CARD_CANCEL_FANOUT_MISSING");
            RequireMethodContains(presenter,
                "private void CancelAllInteractionResidue(",
                "boardView.CancelInteractionState();",
                "PRESENTER_BOARD_CANCEL_MISSING");
            RequireMethodContains(presenter,
                "private void CancelAllInteractionResidue(",
                "trayView.CancelItemInteractions();",
                "PRESENTER_TRAY_CANCEL_MISSING");
        }

        private static void VerifyEnabledRotationAndDetailRegressionBoundaries()
        {
            int[] section17FormalDegrees = { 0, 270, 180, 90 };
            for (int turns = 0; turns < 4; turns++)
            {
                Require(C1ExactBattleSandboxItemRotationAdapter
                        .TryVisualQuarterTurnsToFormalDegrees(
                            turns,
                            out int degrees),
                    "SECTION17_VISUAL_TO_FORMAL_ROTATION_REJECTED " + turns);
                Equal(section17FormalDegrees[turns], degrees,
                    "section-17 formal rotation degrees " + turns);
                Require(C1ExactBattleSandboxItemRotationAdapter
                        .TryFormalDegreesToVisualQuarterTurns(
                            degrees,
                            out int roundTrip),
                    "SECTION17_FORMAL_TO_VISUAL_ROTATION_REJECTED " + degrees);
                Equal(turns, roundTrip,
                    "section-17 rotation round trip " + turns);
            }

            C1FormalItemPlacementCandidate candidate = new(
                new UnityEngine.Vector2Int(3, 1),
                270);
            Equal(new UnityEngine.Vector2Int(3, 1),
                candidate.anchorCell,
                "candidate anchor identity");
            Equal(270, candidate.rotation,
                "candidate formal rotation identity");

            string presenter = ReadRequiredSource(PresenterPath);
            string beginInteraction = ExtractMethodBody(
                presenter,
                "private void BeginInteraction(");
            Require(beginInteraction.IndexOf(
                        "OpenSelectedFormalItemDetail(",
                        StringComparison.Ordinal) < 0,
                "ENABLED_DRAG_MUST_NOT_EMIT_DETAIL_OPEN_INTENT");
            Require(beginInteraction.IndexOf(
                        "selectedItemInstanceId = itemInstanceId;",
                        StringComparison.Ordinal) >= 0,
                "ENABLED_DRAG_SELECTION_IDENTITY_REGRESSION");
            Require(beginInteraction.IndexOf(
                        "PublishCurrent();",
                        StringComparison.Ordinal) >= 0,
                "ENABLED_DRAG_SELECTION_PUBLICATION_REGRESSION");
            RequireMethodContains(presenter,
                "public void SelectBoardItem(",
                "OpenSelectedFormalItemDetail(",
                "ENABLED_CLICK_DETAIL_SELECTION_REGRESSION");
            RequireMethodContains(presenter,
                "public void SelectCard(",
                "SelectBoardItem(card.ItemInstanceId);",
                "ENABLED_CARD_CLICK_DETAIL_SELECTION_REGRESSION");
            RequireMethodContains(presenter,
                "private void ApplyFormalItemDetailResult(",
                "FORMAL_ITEM_DETAIL_REJECTED itemInstanceId=",
                "DETAIL_FIRST_REJECT_DIAGNOSTIC_MISSING");
            Require(presenter.Contains(
                    "NormalizeVisualQuarterTurns(activeRotation + 1)"),
                "ROTATION_ZONE_DIRECTION_CHANGED");
        }

        private static void VerifyForbiddenRuntimeBoundary()
        {
            string combined = string.Join("\n", new[]
            {
                ReadRequiredSource(PresenterPath),
                ReadRequiredSource(BoardPath),
                ReadRequiredSource(TrayPath),
                ReadRequiredSource(CardPath),
                ReadRequiredSource(AuthorizationPath)
            });
            foreach (string forbidden in new[]
                     {
                         "BuildSandbox",
                         "GameObject.Find",
                         "new GameObject",
                         "Resources.Load",
                         "Time.timeScale",
                         "PauseBattle",
                         "ResumeBattle",
                         "UnityEditor"
                     })
                Require(combined.IndexOf(
                            forbidden,
                            StringComparison.Ordinal) < 0,
                    "FORBIDDEN_RUNTIME_BOUNDARY_TOKEN " + forbidden);
        }

        private static void VerifyRejectedLocks(
            C1FormalItemSessionAuthority authority,
            C1FormalItemInteractionAuthorizationRequest request,
            string expectedDiagnostic)
        {
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                CreatePresenter(authority);
            Enable(presenter, authority.Current);
            C1FormalItemInteractionAuthorizationResult rejected =
                presenter.ApplyInteractionAuthorization(request);
            Require(!rejected.accepted && rejected.changed
                    && !rejected.interactionEnabled,
                "REJECTION_MUST_FAIL_CLOSED " + expectedDiagnostic);
            Equal(C1FormalItemInteractionMode.BATTLE_LOCKED,
                rejected.resolvedMode,
                "rejection resolved mode");
            Equal(expectedDiagnostic, rejected.diagnostic,
                "rejection diagnostic");
            Require(!InvokePresenterGate(presenter),
                "REJECTION_LEFT_CENTRAL_GATE_ENABLED " + expectedDiagnostic);
        }

        private static C1FormalItemInteractionAuthorizationRequest Request(
            C1FormalItemSessionSnapshot snapshot,
            string productContext = UseSnapshotValue,
            string sessionToken = UseSnapshotValue,
            long? resetGeneration = null,
            string expectedSignature = UseSnapshotValue,
            C1FormalItemInteractionMode? mode = null)
        {
            return new C1FormalItemInteractionAuthorizationRequest(
                string.Equals(
                    productContext,
                    UseSnapshotValue,
                    StringComparison.Ordinal)
                    ? snapshot.productContext
                    : productContext,
                string.Equals(
                    sessionToken,
                    UseSnapshotValue,
                    StringComparison.Ordinal)
                    ? snapshot.sessionToken
                    : sessionToken,
                resetGeneration ?? snapshot.resetGeneration,
                string.Equals(
                    expectedSignature,
                    UseSnapshotValue,
                    StringComparison.Ordinal)
                    ? snapshot.canonicalSignature
                    : expectedSignature,
                mode ?? C1FormalItemInteractionMode.PREPARE_ENABLED);
        }

        private static C1FormalItemSessionAuthority CreateAuthority(string suffix)
        {
            C1FormalItemSessionCreationResult creation =
                C1FormalItemSessionAuthority.Create(
                    "c1-item-authorization-tests." + suffix,
                    1L);
            Require(creation != null && creation.isSuccess
                    && creation.authority != null,
                "FORMAL_AUTHORITY_CREATE_FAILED "
                + creation?.diagnosticCode);
            return creation.authority;
        }

        private static C1ExactBattleSandboxItemArrangementPresenter
            CreatePresenter(C1FormalItemSessionAuthority authority)
        {
#pragma warning disable SYSLIB0050
            C1ExactBattleSandboxItemArrangementPresenter presenter =
                (C1ExactBattleSandboxItemArrangementPresenter)
                FormatterServices.GetUninitializedObject(typeof(
                    C1ExactBattleSandboxItemArrangementPresenter));
#pragma warning restore SYSLIB0050
            SetPrivateField(presenter, "authority", authority);
            return presenter;
        }

        private static C1ExactBattleSandboxItemBoardView CreateBoardView()
        {
#pragma warning disable SYSLIB0050
            return (C1ExactBattleSandboxItemBoardView)
                FormatterServices.GetUninitializedObject(typeof(
                    C1ExactBattleSandboxItemBoardView));
#pragma warning restore SYSLIB0050
        }

        private static void VerifyBoardPointerIdentitiesCleared(
            C1ExactBattleSandboxItemBoardView board,
            string label)
        {
            Equal(string.Empty,
                GetPrivateField<string>(board, "pressedItemInstanceId"),
                label + " transient press identity");
            Equal(string.Empty,
                GetPrivateField<string>(
                    board,
                    "activeBoardDragItemInstanceId"),
                label + " stable drag identity");
        }

        private static void Enable(
            C1ExactBattleSandboxItemArrangementPresenter presenter,
            C1FormalItemSessionSnapshot snapshot)
        {
            C1FormalItemInteractionAuthorizationResult result =
                presenter.ApplyInteractionAuthorization(
                    C1FormalItemInteractionAuthorizationRequest.FromSnapshot(
                        snapshot,
                        C1FormalItemInteractionMode.PREPARE_ENABLED));
            Require(result.accepted && result.interactionEnabled,
                "ENABLE_FIXTURE_FAILED " + result.diagnostic);
        }

        private static bool InvokePresenterGate(
            C1ExactBattleSandboxItemArrangementPresenter presenter)
        {
            const string failure =
                "REFLECTION_PRESENTER_REJECT_UNLESS_INTERACTION_ENABLED";
            MethodInfo method = typeof(
                    C1ExactBattleSandboxItemArrangementPresenter)
                .GetMethod(
                    "RejectUnlessInteractionEnabled",
                    BindingFlags.Instance | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly,
                    null,
                    Type.EmptyTypes,
                    null);
            Require(method != null && method.ReturnType == typeof(bool),
                failure + "_SIGNATURE_MISSING");
            return (bool)Invoke(method, presenter, Array.Empty<object>(), failure);
        }

        private static void SetPrivateField<T>(object target, string name, T value)
        {
            FieldInfo field = RequirePrivateField(target.GetType(), name);
            Require(field.FieldType.IsAssignableFrom(typeof(T))
                    || value == null,
                "REFLECTION_FIELD_TYPE_MISMATCH " + name);
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string name)
        {
            FieldInfo field = RequirePrivateField(target.GetType(), name);
            object value = field.GetValue(target);
            Require(value == null || value is T,
                "REFLECTION_FIELD_RESULT_MISMATCH " + name);
            return value == null ? default : (T)value;
        }

        private static FieldInfo RequirePrivateField(Type type, string name)
        {
            FieldInfo field = type.GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly);
            Require(field != null,
                "REFLECTION_FIELD_SIGNATURE_MISSING " + type.FullName
                + "." + name);
            return field;
        }

        private static object Invoke(
            MethodInfo method,
            object target,
            object[] arguments,
            string diagnostic)
        {
            try
            {
                return method.Invoke(target, arguments);
            }
            catch (TargetInvocationException exception)
            {
                throw new InvalidOperationException(
                    diagnostic + "_INVOKE_FAILED",
                    exception.InnerException ?? exception);
            }
        }

        private static object InvokePrivateMethod(
            object target,
            string name,
            Type[] parameterTypes,
            object[] arguments)
        {
            MethodInfo method = target.GetType().GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly,
                null,
                parameterTypes,
                null);
            Require(method != null,
                "REFLECTION_METHOD_SIGNATURE_MISSING "
                + target.GetType().FullName + "." + name);
            return Invoke(
                method,
                target,
                arguments,
                "REFLECTION_" + name);
        }

        private static void RequireMethodContains(
            string source,
            string declaration,
            string required,
            string diagnostic)
        {
            string body = ExtractMethodBody(source, declaration);
            Require(body.IndexOf(required, StringComparison.Ordinal) >= 0,
                diagnostic + " " + declaration + " requires=" + required);
        }

        private static void RequireMethodContainsInOrder(
            string source,
            string declaration,
            IEnumerable<string> requiredInOrder,
            string diagnostic)
        {
            string body = ExtractMethodBody(source, declaration);
            int cursor = 0;
            foreach (string required in requiredInOrder)
            {
                int index = body.IndexOf(
                    required,
                    cursor,
                    StringComparison.Ordinal);
                Require(index >= cursor,
                    diagnostic + " " + declaration
                    + " requires=" + required);
                cursor = index + required.Length;
            }
        }

        private static string ExtractMethodBody(
            string source,
            string declaration)
        {
            int declarationIndex = source.IndexOf(
                declaration,
                StringComparison.Ordinal);
            Require(declarationIndex >= 0,
                "METHOD_DECLARATION_MISSING " + declaration);
            int open = source.IndexOf('{', declarationIndex);
            Require(open >= 0, "METHOD_BODY_MISSING " + declaration);
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
                "METHOD_BODY_UNTERMINATED " + declaration);
        }

        private static string ReadRequiredSource(string path)
        {
            Require(File.Exists(path), "REQUIRED_SOURCE_MISSING " + path);
            return File.ReadAllText(path);
        }

        private static void Require(bool condition, string message)
        {
            assertionCount++;
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Equal<T>(T expected, T actual, string label)
        {
            Require(EqualityComparer<T>.Default.Equals(expected, actual),
                label + " expected=" + expected + " actual=" + actual);
        }
    }
}
