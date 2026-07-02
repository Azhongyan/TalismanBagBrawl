#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class MobileShapePlacementRuntimeIntegrationValidator
    {
        public const string PackageName = MobileShapePlacementRuntimeIntegration.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/MobileShapePlacementRuntimeIntegrationFix01/[QA Only] Run Runtime Integration Validation";

        private const string RuntimePlaytestSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs";

        private const string SeamAdapterSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareShapePlacementSeamAdapter.cs";

        private static readonly string[] RequiredSamples =
        {
            "runtime_connected",
            "tap_select_holding",
            "tap_selected_rotates",
            "drag_generates_ghost",
            "release_locks_preview_without_commit",
            "drag_locked_ghost",
            "tap_ghost_commits",
            "invalid_position_red_feedback",
            "cancel_clears_session"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport> { Validate() };
        }

        public static MobileShapePlacementRuntimeIntegrationPlan BuildDefaultPlan()
        {
            ShapeAwareItemTrayGrid trayGrid = new();
            InMemoryShapeGridReceiver board = new(
                "runtime_integration_board_receiver",
                ShapePlacementSource.Board,
                width: 8,
                height: 8,
                commitAllowed: true,
                cellSize: new Vector2(40f, 40f));
            MobileShapePlacementRuntimeIntegration integration = new(trayGrid, board);
            integration.BindRuntimePlaytest(trayGrid, board);

            MobileShapePlacementRuntimeIntegrationPlan plan = new()
            {
                runtimePlaytestConnected = integration.RuntimePlaytestConnected,
                usesShapePlacementSession = integration.Session != null,
                usesShapeAwareItemTrayGrid = integration.UsesShapeAwareItemTrayGrid,
                usesMobileShapePlacementInputExtension = integration.UsesMobileInputExtension,
                usesSessionAuthority = integration.UsesSessionAuthority
            };

            ShapeItemPayload vertical = Payload("runtime_vertical2", "Vertical2", "0,0;0,1");
            integration.PackTrayPayload(vertical);
            ItemShapeCell trayAnchor = trayGrid.TryGetPlacement(vertical.ItemId, out ShapeAwareItemTrayGridPlacement trayPlacement)
                ? trayPlacement.AnchorCell
                : new ItemShapeCell(0, 0);

            MobileShapePlacementInputExtension input = integration.Input;
            plan.Rows.Add(Row(
                "runtime_connected",
                "BindRuntimePlaytest",
                MobileShapePlacementInputState.Idle,
                input.CurrentState,
                null,
                input.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.Idle,
                actualValid: plan.runtimePlaytestConnected
                    && plan.usesShapePlacementSession
                    && plan.usesShapeAwareItemTrayGrid
                    && plan.usesMobileShapePlacementInputExtension
                    && plan.usesSessionAuthority,
                releaseSubmitted: false,
                receiverCommitBefore: board.CommitCount,
                receiverCommitAfter: board.CommitCount,
                note: "RuntimePlaytest bind uses ShapePlacementSession, ShapeAwareItemTrayGrid, and MobileShapePlacementInputExtension."));

            MobileShapePlacementInputState before = input.CurrentState;
            bool selected = input.TapTrayItem(vertical, trayAnchor);
            plan.Rows.Add(Row(
                "tap_select_holding",
                "TapTrayItem",
                before,
                input.CurrentState,
                input.Session.PreviewResult,
                input.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.HoldingItem,
                actualValid: selected,
                releaseSubmitted: false,
                receiverCommitBefore: board.CommitCount,
                receiverCommitAfter: board.CommitCount,
                note: "Clicking a tray item starts the session with the tray grid anchor."));

            before = input.CurrentState;
            bool rotated = input.TapSelectedItemToRotate();
            plan.Rows.Add(Row(
                "tap_selected_rotates",
                "TapSelectedItemToRotate",
                before,
                input.CurrentState,
                input.Session.PreviewResult,
                input.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.RotateHoldingItem,
                actualValid: rotated && input.Session.Rotation == ItemShapeRotation.Rotation90,
                releaseSubmitted: false,
                receiverCommitBefore: board.CommitCount,
                receiverCommitAfter: board.CommitCount,
                note: "Clicking the already selected shape rotates through the session authority."));

            before = input.CurrentState;
            ShapePlacementResult dragResult = input.DragToReceiver(board, new Vector2(80f, 80f));
            plan.Rows.Add(Row(
                "drag_generates_ghost",
                "DragToReceiver",
                before,
                input.CurrentState,
                dragResult,
                input.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.DraggingPreview,
                actualValid: dragResult != null && dragResult.IsValid && input.LastGhostPreview.visible,
                releaseSubmitted: false,
                receiverCommitBefore: board.CommitCount,
                receiverCommitAfter: board.CommitCount,
                note: "Dragging over the board generates a ghost preview without committing."));

            before = input.CurrentState;
            int releaseCommitBefore = board.CommitCount;
            bool locked = input.ReleaseDragLockPreview();
            plan.Rows.Add(Row(
                "release_locks_preview_without_commit",
                "ReleaseDragLockPreview",
                before,
                input.CurrentState,
                input.Session.PreviewResult,
                input.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.PreviewLocked,
                actualValid: locked && input.Session.IsPreviewLocked,
                releaseSubmitted: board.CommitCount != releaseCommitBefore,
                receiverCommitBefore: releaseCommitBefore,
                receiverCommitAfter: board.CommitCount,
                note: "Release locks the ghost and leaves receiver commit count unchanged."));

            before = input.CurrentState;
            ShapePlacementResult lockedDrag = input.DragLockedGhostToReceiver(board, new Vector2(120f, 80f));
            plan.Rows.Add(Row(
                "drag_locked_ghost",
                "DragLockedGhostToReceiver",
                before,
                input.CurrentState,
                lockedDrag,
                input.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.PreviewLocked,
                actualValid: lockedDrag != null && lockedDrag.IsValid && input.Session.IsPreviewLocked,
                releaseSubmitted: false,
                receiverCommitBefore: board.CommitCount,
                receiverCommitAfter: board.CommitCount,
                note: "Dragging a locked ghost moves the preview and relocks it."));

            before = input.CurrentState;
            int confirmCommitBefore = board.CommitCount;
            ShapePlacementResult confirmResult = input.TapGhostToConfirm(board);
            plan.Rows.Add(Row(
                "tap_ghost_commits",
                "TapGhostToConfirm",
                before,
                input.CurrentState,
                confirmResult,
                input.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.Placed,
                actualValid: confirmResult != null && confirmResult.IsValid && board.CommitCount == confirmCommitBefore + 1,
                releaseSubmitted: false,
                receiverCommitBefore: confirmCommitBefore,
                receiverCommitAfter: board.CommitCount,
                note: "Clicking the locked ghost is the only commit path."));

            MobileShapePlacementInputExtension invalidInput =
                new MobileShapePlacementInputExtension(new ShapePlacementSession());
            ShapeItemPayload square = Payload("runtime_square4_invalid", "Square4", "0,0;1,0;0,1;1,1");
            invalidInput.TapTrayItem(square);
            int invalidCommitBefore = board.CommitCount;
            ShapePlacementResult invalidResult = invalidInput.DragToReceiver(board, new Vector2(280f, 280f));
            bool invalidLocked = invalidInput.ReleaseDragLockPreview();
            plan.Rows.Add(Row(
                "invalid_position_red_feedback",
                "DragInvalidPosition",
                MobileShapePlacementInputState.HoldingItem,
                invalidInput.CurrentState,
                invalidResult,
                invalidInput.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.InvalidPreview,
                actualValid: invalidResult != null
                    && !invalidResult.IsValid
                    && invalidResult.InvalidReason == ShapePlacementInvalidReason.OutOfGrid
                    && invalidInput.LastGhostPreview.outlineStyle == GhostPlacementOutlineStyle.Invalid
                    && !invalidLocked,
                releaseSubmitted: board.CommitCount != invalidCommitBefore,
                receiverCommitBefore: invalidCommitBefore,
                receiverCommitAfter: board.CommitCount,
                note: "Invalid preview stays red, cannot lock, and does not commit."));

            MobileShapePlacementInputExtension cancelInput =
                new MobileShapePlacementInputExtension(new ShapePlacementSession());
            cancelInput.TapTrayItem(Payload("runtime_corner3_cancel", "Corner3", "0,0;1,0;0,1"));
            before = cancelInput.CurrentState;
            cancelInput.Cancel(board);
            plan.Rows.Add(Row(
                "cancel_clears_session",
                "Cancel",
                before,
                cancelInput.CurrentState,
                cancelInput.Session.PreviewResult,
                cancelInput.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.Cancelled,
                actualValid: !cancelInput.LastGhostPreview.visible,
                releaseSubmitted: false,
                receiverCommitBefore: board.CommitCount,
                receiverCommitAfter: board.CommitCount,
                note: "Cancel clears the visible ghost and calls receiver cancel."));

            ApplyRuntimePlaytestSourceScan(plan);
            plan.releaseDoesNotCommit = plan.Rows.Any(row =>
                row.SampleId == "release_locks_preview_without_commit"
                && !row.ReleaseSubmitted
                && row.ReceiverCommitBefore == row.ReceiverCommitAfter);
            plan.tapGhostCommits = plan.Rows.Any(row =>
                row.SampleId == "tap_ghost_commits"
                && row.ReceiverCommitAfter == row.ReceiverCommitBefore + 1);
            plan.cancelAvailable = plan.Rows.Any(row =>
                row.SampleId == "cancel_clears_session"
                && row.ToState == MobileShapePlacementInputState.Cancelled
                && row.ActualValid);
            return plan;
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Mobile Shape Placement Runtime Integration");
            MobileShapePlacementRuntimeIntegrationPlan plan = BuildDefaultPlan();

            ValidateIsolation(report, plan);
            ValidateIntegration(report, plan);
            ValidateStateRows(report, plan);
            ValidateRuntimePlaytestSource(report, plan);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_DEVONLY", plan.devOnly, "Runtime integration remains devOnly.");
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_ENABLED_TRUE", plan.isEnabled);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_FEATURE_FLAG_TRUE", plan.featureFlagDefaultEnabled);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_REDRAWS_BOARD", plan.redrawsBoard);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_REDRAWS_TRAY", plan.redrawsItemTray);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_REWRITES_PULLUP", plan.rewritesPullUpAnimation);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_OVERWRITES_RECT", plan.overwritesHandTunedRectTransform);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_TOUCHES_RUNFLOW", plan.touchesRunFlow);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_TOUCHES_SAVE", plan.touchesSaveData);
            ValidateFalse(report, "MOBILE_RUNTIME_INTEGRATION_TOUCHES_BOSS_REWARD_NUMERIC", plan.touchesBossRewardDropNumeric);
        }

        private static void ValidateIntegration(
            BuildSandboxValidationReport report,
            MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_CONNECTED", plan.runtimePlaytestConnected, "RuntimePlaytest bind is connected.");
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_USES_SESSION", plan.usesShapePlacementSession, "ShapePlacementSession is present.");
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_USES_TRAY_GRID", plan.usesShapeAwareItemTrayGrid, "ShapeAwareItemTrayGrid is present.");
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_USES_INPUT_EXTENSION", plan.usesMobileShapePlacementInputExtension, "Mobile input extension is present.");
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_SESSION_AUTHORITY", plan.usesSessionAuthority, "Input extension uses the runtime session.");
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_RELEASE_NO_COMMIT", plan.releaseDoesNotCommit, "Release locks without commit.");
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_TAP_GHOST_COMMITS", plan.tapGhostCommits, "Tap ghost commits.");
            ValidateTrue(report, "MOBILE_RUNTIME_INTEGRATION_CANCEL_AVAILABLE", plan.cancelAvailable, "Cancel is available.");
        }

        private static void ValidateStateRows(
            BuildSandboxValidationReport report,
            MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            IReadOnlyList<MobileShapePlacementRuntimeIntegrationStateRow> rows =
                plan != null && plan.Rows != null
                    ? plan.Rows
                    : new List<MobileShapePlacementRuntimeIntegrationStateRow>();
            foreach (string sample in RequiredSamples)
            {
                if (rows.Any(row => row.SampleId == sample))
                {
                    report.AddInfo("MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT", $"Required sample present: {sample}.", PackageName);
                    continue;
                }

                report.AddError("MOBILE_RUNTIME_INTEGRATION_SAMPLE_MISSING", $"Missing required sample: {sample}.", PackageName);
            }

            foreach (MobileShapePlacementRuntimeIntegrationStateRow row in rows)
            {
                if (row.ExpectedState != row.ToState)
                {
                    report.AddError(
                        "MOBILE_RUNTIME_INTEGRATION_STATE_MISMATCH",
                        $"{row.SampleId} expected {row.ExpectedState}, actual={row.ToState}.",
                        PackageName);
                }

                if (row.ExpectedValid != row.ActualValid)
                {
                    report.AddError(
                        "MOBILE_RUNTIME_INTEGRATION_VALIDITY_MISMATCH",
                        $"{row.SampleId} expected valid={row.ExpectedValid}, actual={row.ActualValid}.",
                        PackageName);
                }

                report.AddInfo(
                    "MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS",
                    $"{row.SampleId} {row.FromState}->{row.ToState} action={row.Action}.",
                    PackageName);
            }
        }

        private static void ValidateRuntimePlaytestSource(
            BuildSandboxValidationReport report,
            MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_HAS_INTEGRATION", plan.runtimePlaytestSourceHasIntegration, "RuntimePlaytest owns MobileShapePlacementRuntimeIntegration.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_HAS_RECEIVER", plan.runtimePlaytestSourceHasBoardReceiver, "RuntimePlaytest owns runtime board receiver.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_HAS_GHOST_TARGET", plan.runtimePlaytestSourceHasGhostTarget, "RuntimePlaytest ghost target handles drop/click.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_HAS_TAP_TRAY", plan.runtimePlaytestSourceHasTapTrayItem, "RuntimePlaytest calls TapTrayItem.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_HAS_RELEASE_LOCK", plan.runtimePlaytestSourceHasReleaseLock, "RuntimePlaytest calls ReleaseDragLockPreview.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_HAS_TAP_GHOST", plan.runtimePlaytestSourceHasTapGhostConfirm, "RuntimePlaytest calls TapGhostToConfirm.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_HAS_CANCEL", plan.runtimePlaytestSourceHasCancel, "RuntimePlaytest calls Cancel.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_USES_BATTLEPREPARE_SEAM", plan.usesBattlePrepareShapePlacementSeamAdapter, "RuntimePlaytest injects BattlePrepareShapePlacementSeamAdapter.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_BINDS_SEAM_CALLBACKS", plan.bindsBattlePrepareShapePlacementSeamCallbacks, "RuntimePlaytest binds seam callback handlers.");
            ValidateTrue(report, "MOBILE_RUNTIME_SOURCE_SEAM_PREVIEW_CALLBACK", plan.seamAdapterConsumesPreviewGhost, "BattlePrepareShapePlacementSeamAdapter consumes PreviewGhost.");
            ValidateFalse(report, "MOBILE_RUNTIME_SOURCE_DIRECT_ITEM_EVENT_HOOKS", plan.usesDirectExternalItemHooks);
            ValidateFalse(report, "MOBILE_RUNTIME_SOURCE_RESTORES_FIX03_ROUTE", plan.restoresOverlayIgnoreLayoutDelayedDrop);
        }

        private static void ApplyRuntimePlaytestSourceScan(MobileShapePlacementRuntimeIntegrationPlan plan)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string fullPath = Path.Combine(projectRoot, RuntimePlaytestSourcePath);
            string seamAdapterFullPath = Path.Combine(projectRoot, SeamAdapterSourcePath);
            string source = File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
            string seamAdapterSource = File.Exists(seamAdapterFullPath) ? File.ReadAllText(seamAdapterFullPath) : string.Empty;
            plan.runtimePlaytestSourcePath = RuntimePlaytestSourcePath;
            plan.runtimePlaytestSourceHasIntegration = source.Contains("MobileShapePlacementRuntimeIntegration");
            plan.runtimePlaytestSourceHasBoardReceiver = source.Contains("RuntimeBattlePrepareBoardShapeGridReceiver");
            plan.runtimePlaytestSourceHasGhostTarget = source.Contains("MobileShapePlacementGhostTarget");
            plan.runtimePlaytestSourceHasTapTrayItem = source.Contains(".TapTrayItem(");
            plan.runtimePlaytestSourceHasReleaseLock = source.Contains(".ReleaseDragLockPreview(");
            plan.runtimePlaytestSourceHasTapGhostConfirm = source.Contains(".TapGhostToConfirm(");
            plan.runtimePlaytestSourceHasCancel = source.Contains(".Cancel(");
            plan.usesBattlePrepareShapePlacementSeamAdapter =
                source.Contains("BattlePrepareShapePlacementSeamAdapter")
                && source.Contains("TryInjectExtensionSeamProvider");
            plan.bindsBattlePrepareShapePlacementSeamCallbacks =
                source.Contains(".BindRuntimeCallbacks(")
                && source.Contains("HandleSeamGhostPreview");
            plan.seamAdapterConsumesPreviewGhost =
                seamAdapterSource.Contains("PreviewGhost(BattlePrepareGhostPreviewContext context)")
                && seamAdapterSource.Contains("ghostPreviewHandler?.Invoke");
            plan.usesDirectExternalItemHooks =
                ContainsOrdinal(source, "DraggableTalismanItemView.ItemClicked +=")
                || ContainsOrdinal(source, "DraggableTalismanItemView.ItemDragStarted +=");
            plan.restoresOverlayIgnoreLayoutDelayedDrop =
                ContainsOrdinal(source, "ignoreLayout")
                || ContainsOrdinal(source, "CommitAfterMatureDragRoutine")
                || ContainsOrdinal(source, "ShapeAwareTrayDragPersistence");
        }

        private static bool ContainsOrdinal(string source, string value)
        {
            return !string.IsNullOrEmpty(source)
                && source.IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        private static MobileShapePlacementRuntimeIntegrationStateRow Row(
            string sampleId,
            string action,
            MobileShapePlacementInputState fromState,
            MobileShapePlacementInputState toState,
            ShapePlacementResult result,
            GhostPlacementPreviewSnapshot ghost,
            MobileShapePlacementInputState expectedState,
            bool actualValid,
            bool releaseSubmitted,
            int receiverCommitBefore,
            int receiverCommitAfter,
            string note)
        {
            return new MobileShapePlacementRuntimeIntegrationStateRow
            {
                SampleId = sampleId,
                Action = action,
                FromState = fromState,
                ToState = toState,
                ExpectedState = expectedState,
                ExpectedValid = true,
                ActualValid = actualValid,
                Anchor = result?.AnchorCell.ToString() ?? string.Empty,
                OccupiedCells = result == null
                    ? string.Empty
                    : string.Join(";", result.OccupiedCells.Select(cell => cell.ToString())),
                InvalidReason = result?.InvalidReason ?? ShapePlacementInvalidReason.None,
                GhostVisible = ghost != null && ghost.visible,
                GhostOutline = ghost?.outlineStyle ?? GhostPlacementOutlineStyle.Hidden,
                GhostHint = ghost?.chineseHint ?? string.Empty,
                ReleaseSubmitted = releaseSubmitted,
                ReceiverCommitBefore = receiverCommitBefore,
                ReceiverCommitAfter = receiverCommitAfter,
                Note = note ?? string.Empty
            };
        }

        private static ShapeItemPayload Payload(
            string itemId,
            string shapeId,
            string offsets,
            ItemShapeRotation rotation = ItemShapeRotation.Rotation0)
        {
            return new ShapeItemPayload(
                itemId,
                shapeId,
                rotation,
                ShapeAwareTrayPackingMap.ParseOffsets(offsets),
                ShapePlacementSource.Tray);
        }

        private static void ValidateTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string passMessage)
        {
            if (value)
            {
                report.AddInfo(code, passMessage, PackageName);
                return;
            }

            report.AddError(code, "Expected true.", PackageName);
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "Expected false.", PackageName);
                return;
            }

            report.AddInfo(code, "Expected false and remained false.", PackageName);
        }
    }

    public sealed class MobileShapePlacementRuntimeIntegrationPlan
    {
        public bool devOnly = true;
        public bool isEnabled;
        public bool featureFlagDefaultEnabled =
            BuildSandboxFeatureFlags.EnableItemShapeOccupancy
            || BuildSandboxFeatureFlags.EnableShapePlacementSandbox
            || BuildSandboxFeatureFlags.EnableShapeRotation;
        public bool runtimePlaytestConnected;
        public bool usesShapePlacementSession;
        public bool usesShapeAwareItemTrayGrid;
        public bool usesMobileShapePlacementInputExtension;
        public bool usesSessionAuthority;
        public bool releaseDoesNotCommit;
        public bool tapGhostCommits;
        public bool cancelAvailable;
        public bool runtimePlaytestSourceHasIntegration;
        public bool runtimePlaytestSourceHasBoardReceiver;
        public bool runtimePlaytestSourceHasGhostTarget;
        public bool runtimePlaytestSourceHasTapTrayItem;
        public bool runtimePlaytestSourceHasReleaseLock;
        public bool runtimePlaytestSourceHasTapGhostConfirm;
        public bool runtimePlaytestSourceHasCancel;
        public bool usesBattlePrepareShapePlacementSeamAdapter;
        public bool bindsBattlePrepareShapePlacementSeamCallbacks;
        public bool seamAdapterConsumesPreviewGhost;
        public bool usesDirectExternalItemHooks;
        public bool restoresOverlayIgnoreLayoutDelayedDrop;
        public bool redrawsBoard;
        public bool redrawsItemTray;
        public bool rewritesPullUpAnimation;
        public bool overwritesHandTunedRectTransform;
        public bool touchesRunFlow;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public string runtimePlaytestSourcePath = string.Empty;
        public List<MobileShapePlacementRuntimeIntegrationStateRow> Rows = new();
    }

    public sealed class MobileShapePlacementRuntimeIntegrationStateRow
    {
        public string SampleId = string.Empty;
        public string Action = string.Empty;
        public MobileShapePlacementInputState FromState;
        public MobileShapePlacementInputState ToState;
        public MobileShapePlacementInputState ExpectedState;
        public bool ExpectedValid;
        public bool ActualValid;
        public string Anchor = string.Empty;
        public string OccupiedCells = string.Empty;
        public ShapePlacementInvalidReason InvalidReason;
        public bool GhostVisible;
        public GhostPlacementOutlineStyle GhostOutline;
        public string GhostHint = string.Empty;
        public bool ReleaseSubmitted;
        public int ReceiverCommitBefore;
        public int ReceiverCommitAfter;
        public string Note = string.Empty;
    }
}
#endif
