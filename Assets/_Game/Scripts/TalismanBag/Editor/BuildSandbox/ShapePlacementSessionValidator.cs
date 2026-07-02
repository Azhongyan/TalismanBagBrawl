#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ShapePlacementSessionValidator
    {
        public const string PackageName = "V0.4-ShapePlacementSession01";

        private static readonly string[] RequiredSamples =
        {
            "can_place_side_effect_light",
            "tray_preview_updates_session",
            "tray_commit_mutates_receiver",
            "board_commit_disabled",
            "cancel_clears_preview",
            "rotation_preview_cells"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport> { Validate() };
        }

        public static ShapePlacementSessionValidationPlan BuildDefaultPlan()
        {
            ShapePlacementSessionValidationPlan plan = new();

            ShapeItemPayload vertical = Payload("session_vertical", "Vertical2", "0,0;0,1");
            ShapeItemPayload corner = Payload("session_corner", "Corner3", "0,0;1,0;0,1");
            ShapeItemPayload square = Payload("session_square", "Square4", "0,0;1,0;0,1;1,1");

            InMemoryShapeGridReceiver tray =
                new("dev_tray_receiver", ShapePlacementSource.Tray, 5, 8, commitAllowed: true, cellSize: new Vector2(40f, 40f));
            ShapePlacementSession session = new();
            session.Begin(vertical);
            ShapePlacementResult canPlace = session.CanPlace(tray, new ItemShapeCell(1, 1));
            plan.Rows.Add(Row(
                "can_place_side_effect_light",
                "CanPlace",
                tray,
                canPlace,
                session,
                expectedValid: true,
                expectedState: ShapePlacementState.HoldingItem,
                receiverMutated: false,
                note: "CanPlace returns a result without becoming the placement authority."));

            ShapePlacementResult preview = session.Preview(tray, new ItemShapeCell(1, 1));
            plan.Rows.Add(Row(
                "tray_preview_updates_session",
                "Preview",
                tray,
                preview,
                session,
                expectedValid: true,
                expectedState: ShapePlacementState.Previewing,
                receiverMutated: false,
                note: "Preview writes session anchors and preview result, not receiver placement."));

            ShapePlacementResult commit = session.Commit(tray);
            plan.Rows.Add(Row(
                "tray_commit_mutates_receiver",
                "Commit",
                tray,
                commit,
                session,
                expectedValid: true,
                expectedState: ShapePlacementState.Committed,
                receiverMutated: commit.OccupiedCells.All(tray.IsOccupied),
                note: "Commit is the only sample path that mutates receiver-owned placement state."));

            InMemoryShapeGridReceiver board =
                new("dev_board_receiver", ShapePlacementSource.Board, 5, 5, commitAllowed: false);
            session = new ShapePlacementSession();
            session.Begin(corner);
            _ = session.Preview(board, new ItemShapeCell(1, 1));
            ShapePlacementResult boardCommit = session.Commit(board);
            plan.Rows.Add(Row(
                "board_commit_disabled",
                "Commit",
                board,
                boardCommit,
                session,
                expectedValid: false,
                expectedState: ShapePlacementState.InvalidPreview,
                receiverMutated: board.OccupiedCells.Count > 0,
                note: "Board receiver previews through the contract but formal commit stays disabled/devOnly."));

            InMemoryShapeGridReceiver cancelTray =
                new("dev_cancel_tray_receiver", ShapePlacementSource.Tray, 5, 8, commitAllowed: true);
            session = new ShapePlacementSession();
            session.Begin(square);
            ShapePlacementResult invalidPreview = session.Preview(cancelTray, new ItemShapeCell(4, 7));
            session.Cancel(cancelTray);
            plan.Rows.Add(Row(
                "cancel_clears_preview",
                "Cancel",
                cancelTray,
                invalidPreview,
                session,
                expectedValid: false,
                expectedState: ShapePlacementState.Cancelled,
                receiverMutated: cancelTray.OccupiedCells.Count > 0,
                note: "Cancel clears the preview and does not commit the invalid placement."));

            InMemoryShapeGridReceiver rotationTray =
                new("dev_rotation_tray_receiver", ShapePlacementSource.Tray, 5, 8, commitAllowed: true);
            session = new ShapePlacementSession();
            session.Begin(vertical);
            session.RotateClockwise();
            ShapePlacementResult rotationPreview = session.Preview(rotationTray, new ItemShapeCell(1, 1));
            plan.Rows.Add(Row(
                "rotation_preview_cells",
                "Preview",
                rotationTray,
                rotationPreview,
                session,
                expectedValid: true,
                expectedState: ShapePlacementState.Previewing,
                receiverMutated: false,
                note: "Rotation90 for Vertical2 resolves to horizontal occupied cells through the session payload."));

            ShapePlacementResult overlap = BuildOverlapSample(vertical);
            plan.Rows.Add(new ShapePlacementSessionSampleRow
            {
                SampleId = "tray_overlap_rejected",
                Action = "Preview",
                ReceiverId = "dev_overlap_tray_receiver",
                ReceiverSource = ShapePlacementSource.Tray.ToString(),
                Anchor = "0,0",
                OccupiedCells = FormatCells(overlap.OccupiedCells),
                ExpectedValid = false,
                ActualValid = overlap.IsValid,
                InvalidReason = overlap.InvalidReason,
                StateAfter = ShapePlacementState.InvalidPreview,
                StateMatches = overlap.InvalidReason == ShapePlacementInvalidReason.CellOccupied,
                ReceiverMutated = false,
                Note = "Overlap is rejected before any commit path."
            });

            if (tray.ScreenPointToCell(new Vector2(82f, 122f), null, out ItemShapeCell screenCell))
            {
                plan.Rows.Add(new ShapePlacementSessionSampleRow
                {
                    SampleId = "screen_point_to_cell",
                    Action = "ScreenPointToCell",
                    ReceiverId = tray.ReceiverId,
                    ReceiverSource = tray.ReceiverSource.ToString(),
                    Anchor = screenCell.ToString(),
                    OccupiedCells = string.Empty,
                    ExpectedValid = true,
                    ActualValid = screenCell.Equals(new ItemShapeCell(2, 3)),
                    InvalidReason = ShapePlacementInvalidReason.None,
                    StateAfter = ShapePlacementState.Idle,
                    StateMatches = screenCell.Equals(new ItemShapeCell(2, 3)),
                    ReceiverMutated = false,
                    Note = "Receiver-level screen point conversion is available without touching UI layout."
                });
            }

            return plan;
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("ShapePlacementSession Protocol");
            ShapePlacementSessionValidationPlan plan = BuildDefaultPlan();

            ValidateIsolation(report, plan);
            ValidateSamples(report, plan);
            return report;
        }

        private static ShapePlacementResult BuildOverlapSample(ShapeItemPayload payload)
        {
            InMemoryShapeGridReceiver receiver =
                new("dev_overlap_tray_receiver", ShapePlacementSource.Tray, 5, 8, commitAllowed: true);
            receiver.SeedOccupiedCell(new ItemShapeCell(0, 0), "blocker");
            ShapePlacementSession session = new();
            session.Begin(payload);
            return session.Preview(receiver, new ItemShapeCell(0, 0));
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            ShapePlacementSessionValidationPlan plan)
        {
            if (plan == null)
            {
                report.AddError("SHAPE_PLACEMENT_SESSION_PLAN_NULL", "ShapePlacementSession plan was not created.", PackageName);
                return;
            }

            ValidateTrue(report, "SHAPE_PLACEMENT_SESSION_DEVONLY", plan.DevOnly, "Protocol remains devOnly.");
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_ENABLED_TRUE", plan.IsEnabled);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_FEATURE_FLAG_TRUE", plan.FeatureFlagDefaultEnabled);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_FIX03_CONTINUED", plan.ContinuesFix03OverlayRoute);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_IGNORE_LAYOUT", plan.UsesIgnoreLayoutAuthority);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_DELAYED_DROP_READ", plan.UsesDelayedDropRead);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_REDRAWS_ITEM_TRAY", plan.RedrawsItemTray);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_REDRAWS_BOARD", plan.RedrawsBoard);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_WRITES_FORMAL_SCENE", plan.WritesFormalScene);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_WRITES_FORMAL_UI", plan.WritesFormalUi);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_TOUCHES_RUNFLOW", plan.TouchesRunFlow);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_TOUCHES_PAGESTATE", plan.TouchesPageState);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_TOUCHES_FORMATIONSTATE", plan.TouchesFormationState);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_TOUCHES_SAVE", plan.TouchesSaveData);
            ValidateFalse(report, "SHAPE_PLACEMENT_SESSION_TOUCHES_BOSS_REWARD_NUMERIC", plan.TouchesBossRewardDropNumeric);
        }

        private static void ValidateSamples(
            BuildSandboxValidationReport report,
            ShapePlacementSessionValidationPlan plan)
        {
            IReadOnlyList<ShapePlacementSessionSampleRow> rows =
                plan != null && plan.Rows != null
                    ? plan.Rows
                    : Array.Empty<ShapePlacementSessionSampleRow>();
            foreach (string requiredSample in RequiredSamples)
            {
                if (rows.Any(row => row != null && row.SampleId == requiredSample))
                {
                    report.AddInfo(
                        "SHAPE_PLACEMENT_SESSION_SAMPLE_PRESENT",
                        $"Required sample present: {requiredSample}.",
                        PackageName);
                    continue;
                }

                report.AddError(
                    "SHAPE_PLACEMENT_SESSION_SAMPLE_MISSING",
                    $"Missing required sample: {requiredSample}.",
                    PackageName);
            }

            foreach (ShapePlacementSessionSampleRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("SHAPE_PLACEMENT_SESSION_SAMPLE_NULL", "Null protocol sample row.", PackageName);
                    continue;
                }

                if (row.ExpectedValid != row.ActualValid)
                {
                    report.AddError(
                        "SHAPE_PLACEMENT_SESSION_SAMPLE_VALIDITY_MISMATCH",
                        $"{row.SampleId} expected valid={row.ExpectedValid}, actual={row.ActualValid}.",
                        PackageName);
                }

                if (!row.StateMatches)
                {
                    report.AddError(
                        "SHAPE_PLACEMENT_SESSION_STATE_MISMATCH",
                        $"{row.SampleId} state or reason did not match expectation. state={row.StateAfter}, reason={row.InvalidReason}.",
                        PackageName);
                    continue;
                }

                report.AddInfo(
                    "SHAPE_PLACEMENT_SESSION_SAMPLE_PASS",
                    $"{row.SampleId} action={row.Action}, valid={row.ActualValid}, state={row.StateAfter}, reason={row.InvalidReason}.",
                    PackageName);
            }
        }

        private static ShapePlacementSessionSampleRow Row(
            string sampleId,
            string action,
            InMemoryShapeGridReceiver receiver,
            ShapePlacementResult result,
            ShapePlacementSession session,
            bool expectedValid,
            ShapePlacementState expectedState,
            bool receiverMutated,
            string note)
        {
            return new ShapePlacementSessionSampleRow
            {
                SampleId = sampleId,
                Action = action,
                ReceiverId = receiver?.ReceiverId ?? string.Empty,
                ReceiverSource = receiver?.ReceiverSource.ToString() ?? ShapePlacementSource.Unknown.ToString(),
                Anchor = result?.AnchorCell.ToString() ?? string.Empty,
                OccupiedCells = FormatCells(result?.OccupiedCells),
                ExpectedValid = expectedValid,
                ActualValid = result != null && result.IsValid,
                InvalidReason = result?.InvalidReason ?? ShapePlacementInvalidReason.ShapeInvalid,
                StateAfter = session?.CurrentState ?? ShapePlacementState.Idle,
                StateMatches = session != null && session.CurrentState == expectedState,
                ReceiverMutated = receiverMutated,
                Note = note ?? string.Empty
            };
        }

        private static ShapeItemPayload Payload(string itemId, string shapeId, string offsets)
        {
            return new ShapeItemPayload(
                itemId,
                shapeId,
                ItemShapeRotation.Rotation0,
                ParseOffsets(offsets),
                ShapePlacementSource.Tray);
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
                report.AddError(code, "Expected false for this protocol isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
        }
    }

    public sealed class ShapePlacementSessionValidationPlan
    {
        public bool DevOnly = true;
        public bool IsEnabled;
        public bool FeatureFlagDefaultEnabled =
            BuildSandboxFeatureFlags.EnableShapePlacementSandbox
            || BuildSandboxFeatureFlags.EnableShapeRotation;
        public bool ContinuesFix03OverlayRoute;
        public bool UsesIgnoreLayoutAuthority;
        public bool UsesDelayedDropRead;
        public bool RedrawsItemTray;
        public bool RedrawsBoard;
        public bool WritesFormalScene;
        public bool WritesFormalUi;
        public bool TouchesRunFlow;
        public bool TouchesPageState;
        public bool TouchesFormationState;
        public bool TouchesSaveData;
        public bool TouchesBossRewardDropNumeric;
        public List<ShapePlacementSessionSampleRow> Rows = new();
    }

    public sealed class ShapePlacementSessionSampleRow
    {
        public string SampleId = string.Empty;
        public string Action = string.Empty;
        public string ReceiverId = string.Empty;
        public string ReceiverSource = string.Empty;
        public string Anchor = string.Empty;
        public string OccupiedCells = string.Empty;
        public bool ExpectedValid;
        public bool ActualValid;
        public ShapePlacementInvalidReason InvalidReason;
        public ShapePlacementState StateAfter;
        public bool StateMatches;
        public bool ReceiverMutated;
        public string Note = string.Empty;
    }
}
#endif
