#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class MobileShapePlacementValidator
    {
        public const string PackageName = "V0.4-MobileShapePlacementInteraction01";
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/MobileShapePlacementInteraction01/[QA Only] Run Mobile Shape Placement";

        private static readonly string[] RequiredProtocolSamples =
        {
            "tap_select_holding",
            "tap_selected_rotates",
            "drag_generates_ghost",
            "release_locks_preview_without_commit",
            "tap_ghost_commits",
            "invalid_position_red_feedback",
            "cancel_clears_session",
            "single_shape_no_rotation_hint"
        };

        private static readonly string[] RequiredShapeIds =
        {
            "Single1",
            "Vertical2",
            "Corner3",
            "Square4"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport> { Validate() };
        }

        public static MobileShapePlacementValidationPlan BuildDefaultPlan()
        {
            MobileShapePlacementValidationPlan plan = new();
            MobileShapePlacementInputSettings settings = new();

            ShapeItemPayload vertical = Payload(
                "mobile_vertical2",
                "Vertical2",
                "0,0;0,1",
                ShapePlacementSource.Tray);
            InMemoryShapeGridReceiver board = new(
                "dev_mobile_board_receiver",
                ShapePlacementSource.Board,
                width: 8,
                height: 8,
                commitAllowed: true,
                cellSize: new Vector2(40f, 40f));
            MobileShapePlacementInputExtension input = new(settings: settings);

            MobileShapePlacementInputState before = input.CurrentState;
            bool selected = input.TapTrayItem(vertical, new ItemShapeCell(0, 0));
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
                note: "First tap selects the tray item and enters HoldingItem."));

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
                note: "Second tap rotates the selected shape through data-driven occupied offsets."));

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
                note: "Drag over the board produces a ghost preview and does not commit."));

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
                note: "Release keeps the ghost visible and locked without mutating receiver placement."));

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
                note: "Clicking the locked ghost is the only sample path that commits placement."));

            ShapeItemPayload square = Payload(
                "mobile_square4_invalid",
                "Square4",
                "0,0;1,0;0,1;1,1",
                ShapePlacementSource.Tray);
            MobileShapePlacementInputExtension invalidInput = new(settings: settings);
            invalidInput.TapTrayItem(square, new ItemShapeCell(0, 0));
            int invalidCommitBefore = board.CommitCount;
            ShapePlacementResult invalidResult = invalidInput.DragToReceiver(board, new Vector2(280f, 280f));
            bool invalidRelease = invalidInput.ReleaseDragLockPreview();
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
                    && !invalidRelease,
                releaseSubmitted: board.CommitCount != invalidCommitBefore,
                receiverCommitBefore: invalidCommitBefore,
                receiverCommitAfter: board.CommitCount,
                note: "Out-of-grid preview stays invalid, shows red feedback, and cannot lock or commit."));

            MobileShapePlacementInputExtension cancelInput = new(settings: settings);
            cancelInput.TapTrayItem(Payload("mobile_corner3_cancel", "Corner3", "0,0;1,0;0,1", ShapePlacementSource.Tray));
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
                note: "Cancel button clears the active preview without writing placement state."));

            MobileShapePlacementInputExtension singleInput = new(settings: settings);
            singleInput.TapTrayItem(Payload("mobile_single1", "Single1", "0,0", ShapePlacementSource.Tray));
            before = singleInput.CurrentState;
            bool singleRotated = singleInput.TapSelectedItemToRotate();
            plan.Rows.Add(Row(
                "single_shape_no_rotation_hint",
                "TapSelectedItemToRotate",
                before,
                singleInput.CurrentState,
                singleInput.Session.PreviewResult,
                singleInput.LastGhostPreview,
                expectedState: MobileShapePlacementInputState.HoldingItem,
                actualValid: !singleRotated && singleInput.LastHint == "该形态无需旋转",
                releaseSubmitted: false,
                receiverCommitBefore: board.CommitCount,
                receiverCommitAfter: board.CommitCount,
                note: "Single-cell shapes are data-detected as non-rotating and return a light hint."));

            plan.ShapeRows.Add(ShapeRow("Single1", "0,0", expectedCells: 1, expectedUniqueRotations: 1));
            plan.ShapeRows.Add(ShapeRow("Vertical2", "0,0;0,1", expectedCells: 2, expectedUniqueRotations: 2));
            plan.ShapeRows.Add(ShapeRow("Corner3", "0,0;1,0;0,1", expectedCells: 3, expectedUniqueRotations: 4));
            plan.ShapeRows.Add(ShapeRow("Square4", "0,0;1,0;0,1;1,1", expectedCells: 4, expectedUniqueRotations: 1));

            plan.tapGestureKind = new MobileShapePlacementInputExtension(settings: settings)
                .ClassifyGesture(Vector2.zero, new Vector2(10f, 0f), 0.2f);
            plan.dragGestureKind = new MobileShapePlacementInputExtension(settings: settings)
                .ClassifyGesture(Vector2.zero, new Vector2(15f, 0f), 0.1f);
            plan.previewLockedImplemented =
                plan.Rows.Any(row => row.SampleId == "release_locks_preview_without_commit"
                    && row.ToState == MobileShapePlacementInputState.PreviewLocked
                    && row.ActualValid);
            plan.releaseDoesNotCommit =
                plan.Rows.Any(row => row.SampleId == "release_locks_preview_without_commit"
                    && !row.ReleaseSubmitted
                    && row.ReceiverCommitBefore == row.ReceiverCommitAfter);
            plan.tapGhostCommits =
                plan.Rows.Any(row => row.SampleId == "tap_ghost_commits"
                    && row.ToState == MobileShapePlacementInputState.Placed
                    && row.ReceiverCommitAfter == row.ReceiverCommitBefore + 1);
            plan.cancelAvailable =
                plan.Rows.Any(row => row.SampleId == "cancel_clears_session"
                    && row.ToState == MobileShapePlacementInputState.Cancelled
                    && row.ActualValid);

            return plan;
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Mobile Shape Placement Interaction");
            MobileShapePlacementValidationPlan plan = BuildDefaultPlan();

            ValidateIsolation(report, plan);
            ValidateGestures(report, plan);
            ValidateProtocolSamples(report, plan);
            ValidateShapeSupport(report, plan);
            ValidateCoreLocks(report, plan);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            MobileShapePlacementValidationPlan plan)
        {
            if (plan == null)
            {
                report.AddError("MOBILE_SHAPE_PLACEMENT_PLAN_NULL", "Mobile placement validation plan was not created.", PackageName);
                return;
            }

            ValidateTrue(report, "MOBILE_SHAPE_PLACEMENT_DEVONLY", plan.devOnly, "Mobile placement remains BuildSandbox/devOnly.");
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_ENABLED_TRUE", plan.isEnabled);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_FEATURE_FLAG_TRUE", plan.featureFlagDefaultEnabled);
            ValidateTrue(report, "MOBILE_SHAPE_PLACEMENT_REUSES_MATURE_BATTLEPREPARE", plan.reusesMatureBattlePrepare, "Mature BattlePrepare remains the base component.");
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_REDRAWS_BOARD", plan.redrawsBoard);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_REDRAWS_ITEM_TRAY", plan.redrawsItemTray);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_REWRITES_DRAG_FEEL", plan.rewritesDragFeel);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_REWRITES_PULLUP", plan.rewritesPullUpAnimation);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_WRITES_FORMAL_SCENE", plan.writesFormalScene);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_WRITES_FORMAL_UI", plan.writesFormalUi);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_OVERWRITES_RECT", plan.overwritesHandTunedRectTransform);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_TOUCHES_RUNFLOW", plan.touchesRunFlow);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_TOUCHES_PAGESTATE", plan.touchesPageState);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_TOUCHES_FORMATIONSTATE", plan.touchesFormationState);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_TOUCHES_SAVE", plan.touchesSaveData);
            ValidateFalse(report, "MOBILE_SHAPE_PLACEMENT_TOUCHES_BOSS_REWARD_NUMERIC", plan.touchesBossRewardDropNumeric);
        }

        private static void ValidateGestures(
            BuildSandboxValidationReport report,
            MobileShapePlacementValidationPlan plan)
        {
            if (Math.Abs(plan.tapMoveThresholdPixels - 15f) > 0.01f)
            {
                report.AddError(
                    "MOBILE_SHAPE_PLACEMENT_TAP_DISTANCE_THRESHOLD",
                    $"Tap distance threshold must be 15px, actual={plan.tapMoveThresholdPixels}.",
                    PackageName);
            }
            else
            {
                report.AddInfo("MOBILE_SHAPE_PLACEMENT_TAP_DISTANCE_THRESHOLD", "Tap distance threshold is 15px.", PackageName);
            }

            if (Math.Abs(plan.tapTimeThresholdMilliseconds - 250f) > 0.01f)
            {
                report.AddError(
                    "MOBILE_SHAPE_PLACEMENT_TAP_TIME_THRESHOLD",
                    $"Tap time threshold must be 250ms, actual={plan.tapTimeThresholdMilliseconds}.",
                    PackageName);
            }
            else
            {
                report.AddInfo("MOBILE_SHAPE_PLACEMENT_TAP_TIME_THRESHOLD", "Tap time threshold is 250ms.", PackageName);
            }

            if (plan.tapGestureKind != MobileShapeGestureKind.Tap)
            {
                report.AddError("MOBILE_SHAPE_PLACEMENT_TAP_CLASSIFICATION", "Tap sample was not classified as Tap.", PackageName);
            }
            else
            {
                report.AddInfo("MOBILE_SHAPE_PLACEMENT_TAP_CLASSIFICATION", "Tap sample classified correctly.", PackageName);
            }

            if (plan.dragGestureKind != MobileShapeGestureKind.Drag)
            {
                report.AddError("MOBILE_SHAPE_PLACEMENT_DRAG_CLASSIFICATION", "15px movement was not classified as Drag.", PackageName);
            }
            else
            {
                report.AddInfo("MOBILE_SHAPE_PLACEMENT_DRAG_CLASSIFICATION", "Drag sample classified correctly.", PackageName);
            }
        }

        private static void ValidateProtocolSamples(
            BuildSandboxValidationReport report,
            MobileShapePlacementValidationPlan plan)
        {
            IReadOnlyList<MobileShapePlacementStateMachineRow> rows =
                plan != null ? plan.Rows : Array.Empty<MobileShapePlacementStateMachineRow>();
            foreach (string requiredSample in RequiredProtocolSamples)
            {
                if (rows.Any(row => row.SampleId == requiredSample))
                {
                    report.AddInfo("MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT", $"Required sample present: {requiredSample}.", PackageName);
                    continue;
                }

                report.AddError("MOBILE_SHAPE_PLACEMENT_SAMPLE_MISSING", $"Missing required sample: {requiredSample}.", PackageName);
            }

            foreach (MobileShapePlacementStateMachineRow row in rows)
            {
                if (row.ExpectedState != row.ToState)
                {
                    report.AddError(
                        "MOBILE_SHAPE_PLACEMENT_STATE_MISMATCH",
                        $"{row.SampleId} expected {row.ExpectedState}, actual={row.ToState}.",
                        PackageName);
                }

                if (row.ExpectedValid != row.ActualValid)
                {
                    report.AddError(
                        "MOBILE_SHAPE_PLACEMENT_VALIDITY_MISMATCH",
                        $"{row.SampleId} expected valid={row.ExpectedValid}, actual={row.ActualValid}.",
                        PackageName);
                }

                if (row.SampleId == "release_locks_preview_without_commit" && row.ReleaseSubmitted)
                {
                    report.AddError(
                        "MOBILE_SHAPE_PLACEMENT_RELEASE_COMMITTED",
                        "Release must lock preview without committing receiver state.",
                        PackageName);
                }

                if (row.SampleId == "tap_ghost_commits" && row.ReceiverCommitAfter <= row.ReceiverCommitBefore)
                {
                    report.AddError(
                        "MOBILE_SHAPE_PLACEMENT_GHOST_DID_NOT_COMMIT",
                        "Clicking the locked ghost did not commit receiver state.",
                        PackageName);
                }

                report.AddInfo(
                    "MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS",
                    $"{row.SampleId} {row.FromState}->{row.ToState} action={row.Action}, valid={row.ActualValid}.",
                    PackageName);
            }
        }

        private static void ValidateShapeSupport(
            BuildSandboxValidationReport report,
            MobileShapePlacementValidationPlan plan)
        {
            IReadOnlyList<MobileShapePlacementShapeSupportRow> rows =
                plan != null ? plan.ShapeRows : Array.Empty<MobileShapePlacementShapeSupportRow>();
            foreach (string shapeId in RequiredShapeIds)
            {
                MobileShapePlacementShapeSupportRow row = rows.FirstOrDefault(candidate => candidate.ShapeId == shapeId);
                if (row == null)
                {
                    report.AddError("MOBILE_SHAPE_PLACEMENT_SHAPE_MISSING", $"Missing shape support sample: {shapeId}.", PackageName);
                    continue;
                }

                if (row.CellCount != row.ExpectedCellCount)
                {
                    report.AddError(
                        "MOBILE_SHAPE_PLACEMENT_SHAPE_CELL_MISMATCH",
                        $"{shapeId} expected cells={row.ExpectedCellCount}, actual={row.CellCount}.",
                        PackageName);
                }

                if (row.UniqueRotationCount != row.ExpectedUniqueRotationCount)
                {
                    report.AddError(
                        "MOBILE_SHAPE_PLACEMENT_ROTATION_MISMATCH",
                        $"{shapeId} expected unique rotations={row.ExpectedUniqueRotationCount}, actual={row.UniqueRotationCount}.",
                        PackageName);
                }

                report.AddInfo(
                    "MOBILE_SHAPE_PLACEMENT_SHAPE_PASS",
                    $"{shapeId} cells={row.CellCount}, uniqueRotations={row.UniqueRotationCount}, canRotate={row.CanRotate}.",
                    PackageName);
            }
        }

        private static void ValidateCoreLocks(
            BuildSandboxValidationReport report,
            MobileShapePlacementValidationPlan plan)
        {
            ValidateTrue(report, "MOBILE_SHAPE_PLACEMENT_PREVIEW_LOCKED", plan.previewLockedImplemented, "PreviewLocked state is covered.");
            ValidateTrue(report, "MOBILE_SHAPE_PLACEMENT_RELEASE_NO_COMMIT", plan.releaseDoesNotCommit, "Release does not commit.");
            ValidateTrue(report, "MOBILE_SHAPE_PLACEMENT_TAP_GHOST_COMMITS", plan.tapGhostCommits, "Tap ghost commits.");
            ValidateTrue(report, "MOBILE_SHAPE_PLACEMENT_CANCEL_AVAILABLE", plan.cancelAvailable, "Cancel operation is available.");
            ValidateTrue(report, "MOBILE_SHAPE_PLACEMENT_PLACED_EDIT_DEFERRED", !plan.placedItemEditImplemented, "PlacedItemEdit remains deferred.");
        }

        private static MobileShapePlacementStateMachineRow Row(
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
            return new MobileShapePlacementStateMachineRow
            {
                SampleId = sampleId,
                Action = action,
                FromState = fromState,
                ToState = toState,
                ExpectedState = expectedState,
                ExpectedValid = true,
                ActualValid = actualValid,
                Anchor = result?.AnchorCell.ToString() ?? string.Empty,
                OccupiedCells = FormatCells(result?.OccupiedCells),
                InvalidReason = result?.InvalidReason ?? ShapePlacementInvalidReason.None,
                GhostVisible = ghost != null && ghost.visible,
                GhostOutline = ghost?.outlineStyle ?? GhostPlacementOutlineStyle.Hidden,
                GhostHint = ghost?.chineseHint ?? string.Empty,
                PreviewLocked = toState == MobileShapePlacementInputState.PreviewLocked,
                ReleaseSubmitted = releaseSubmitted,
                ReceiverCommitBefore = receiverCommitBefore,
                ReceiverCommitAfter = receiverCommitAfter,
                Note = note ?? string.Empty
            };
        }

        private static MobileShapePlacementShapeSupportRow ShapeRow(
            string shapeId,
            string offsets,
            int expectedCells,
            int expectedUniqueRotations)
        {
            ShapeItemPayload payload = Payload(
                $"shape_support_{shapeId.ToLowerInvariant()}",
                shapeId,
                offsets,
                ShapePlacementSource.Tray);
            return new MobileShapePlacementShapeSupportRow
            {
                ShapeId = shapeId,
                Offsets = offsets,
                CellCount = payload.OccupiedOffsets.Count,
                ExpectedCellCount = expectedCells,
                UniqueRotationCount = ItemRotationInputExtension.CountUniqueRotations(payload),
                ExpectedUniqueRotationCount = expectedUniqueRotations,
                CanRotate = ItemRotationInputExtension.CanRotate(payload)
            };
        }

        private static ShapeItemPayload Payload(
            string itemId,
            string shapeId,
            string offsets,
            ShapePlacementSource source,
            ItemShapeRotation rotation = ItemShapeRotation.Rotation0)
        {
            return new ShapeItemPayload(
                itemId,
                shapeId,
                rotation,
                ParseOffsets(offsets),
                source);
        }

        private static IReadOnlyList<ItemShapeCell> ParseOffsets(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<ItemShapeCell>();
            }

            return value
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split(','))
                .Where(parts => parts.Length == 2
                    && int.TryParse(parts[0], out _)
                    && int.TryParse(parts[1], out _))
                .Select(parts => new ItemShapeCell(int.Parse(parts[0]), int.Parse(parts[1])))
                .ToArray();
        }

        private static string FormatCells(IReadOnlyList<ItemShapeCell> cells)
        {
            return cells == null || cells.Count == 0
                ? string.Empty
                : string.Join(";", cells.Select(cell => cell.ToString()));
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
                report.AddError(code, "Expected false for this mobile placement isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
        }
    }

    public sealed class MobileShapePlacementValidationPlan
    {
        public bool devOnly = true;
        public bool isEnabled;
        public bool featureFlagDefaultEnabled =
            BuildSandboxFeatureFlags.EnableItemShapeOccupancy
            || BuildSandboxFeatureFlags.EnableShapePlacementSandbox
            || BuildSandboxFeatureFlags.EnableShapeRotation;
        public float tapMoveThresholdPixels = MobileShapePlacementInputSettings.DefaultTapMoveThresholdPixels;
        public float tapTimeThresholdMilliseconds = MobileShapePlacementInputSettings.DefaultTapTimeThresholdSeconds * 1000f;
        public float fingerGhostOffsetPixels = MobileShapePlacementInputSettings.DefaultFingerGhostOffsetPixels;
        public MobileShapeGestureKind tapGestureKind;
        public MobileShapeGestureKind dragGestureKind;
        public bool previewLockedImplemented;
        public bool releaseDoesNotCommit;
        public bool tapGhostCommits;
        public bool cancelAvailable;
        public bool placedItemEditImplemented = PlacedItemEditExtension.Implemented;
        public bool reusesMatureBattlePrepare = true;
        public bool redrawsBoard;
        public bool redrawsItemTray;
        public bool rewritesDragFeel;
        public bool rewritesPullUpAnimation;
        public bool writesFormalScene;
        public bool writesFormalUi;
        public bool overwritesHandTunedRectTransform;
        public bool touchesRunFlow;
        public bool touchesPageState;
        public bool touchesFormationState;
        public bool touchesSaveData;
        public bool touchesBossRewardDropNumeric;
        public List<MobileShapePlacementStateMachineRow> Rows = new();
        public List<MobileShapePlacementShapeSupportRow> ShapeRows = new();
    }

    public sealed class MobileShapePlacementStateMachineRow
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
        public bool PreviewLocked;
        public bool ReleaseSubmitted;
        public int ReceiverCommitBefore;
        public int ReceiverCommitAfter;
        public string Note = string.Empty;
    }

    public sealed class MobileShapePlacementShapeSupportRow
    {
        public string ShapeId = string.Empty;
        public string Offsets = string.Empty;
        public int CellCount;
        public int ExpectedCellCount;
        public int UniqueRotationCount;
        public int ExpectedUniqueRotationCount;
        public bool CanRotate;
    }
}
#endif
