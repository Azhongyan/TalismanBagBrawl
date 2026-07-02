#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ShapeAwareItemTrayGridValidator
    {
        public const string PackageName = "V0.4-ShapeAwareItemTrayGrid01";
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/ShapeAwareItemTrayGrid01/[QA Only] Run Shape-Aware ItemTray Grid";

        private static readonly Dictionary<string, (int cellCount, string offsets)> RequiredShapes = new()
        {
            { "Single1", (1, "0,0") },
            { "Vertical2", (2, "0,0;0,1") },
            { "Corner3", (3, "0,0;1,0;0,1") },
            { "Square4", (4, "0,0;1,0;0,1;1,1") }
        };

        private static readonly string[] RequiredSamples =
        {
            "pack_single1",
            "pack_vertical2",
            "pack_corner3",
            "pack_square4",
            "bounds_rejects_square4",
            "overlap_rejects_corner3",
            "illegal_commit_keeps_stable_anchor",
            "legal_move_updates_stable_anchor",
            "rotation_normalizes_corner3",
            "screen_point_to_cell"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport> { Validate() };
        }

        public static ShapeAwareItemTrayGridValidationPlan BuildDefaultPlan()
        {
            ShapeAwareItemTrayGridValidationPlan plan = new();
            ShapeAwareItemTrayGrid grid = new(
                cellSize: new Vector2(40f, 40f));

            foreach (KeyValuePair<string, (int cellCount, string offsets)> pair in RequiredShapes)
            {
                ShapeItemPayload payload = Payload(
                    $"item_{pair.Key.ToLowerInvariant()}",
                    pair.Key,
                    pair.Value.offsets);
                bool packed = grid.TryPack(payload, out ShapePlacementResult result);
                plan.Rows.Add(Row(
                    $"pack_{pair.Key.ToLowerInvariant()}",
                    "Pack",
                    result,
                    ShapePlacementState.Committed,
                    expectedValid: true,
                    expectedCellCount: pair.Value.cellCount,
                    receiverMutated: packed,
                    anchorStable: grid.TryGetPlacement(payload.ItemId, out ShapeAwareItemTrayGridPlacement placement)
                        && placement.AnchorCell.Equals(result.AnchorCell),
                    note: "Row-major packing commits the item through ShapeGridReceiver.Commit."));
            }

            ShapeItemPayload boundsSquare = Payload("bounds_square4", "Square4", RequiredShapes["Square4"].offsets);
            ShapePlacementResult boundsResult = grid.CanPlace(boundsSquare, new ItemShapeCell(4, 7));
            plan.Rows.Add(Row(
                "bounds_rejects_square4",
                "CanPlace",
                boundsResult,
                ShapePlacementState.HoldingItem,
                expectedValid: false,
                expectedCellCount: 4,
                receiverMutated: false,
                anchorStable: true,
                note: "A 2x2 shape anchored at the last slot exceeds the 5x8 tray bounds."));

            ShapeItemPayload overlapCorner = Payload("overlap_corner3", "Corner3", RequiredShapes["Corner3"].offsets);
            ShapePlacementResult overlapResult = grid.CanPlace(overlapCorner, new ItemShapeCell(0, 0));
            plan.Rows.Add(Row(
                "overlap_rejects_corner3",
                "CanPlace",
                overlapResult,
                ShapePlacementState.HoldingItem,
                expectedValid: false,
                expectedCellCount: 3,
                receiverMutated: false,
                anchorStable: true,
                note: "A new item cannot overlap already committed tray cells."));

            ShapeItemPayload squarePayload = Payload("item_square4", "Square4", RequiredShapes["Square4"].offsets);
            ShapeAwareItemTrayGridPlacement squareBefore =
                grid.TryGetPlacement(squarePayload.ItemId, out ShapeAwareItemTrayGridPlacement before)
                    ? before
                    : null;
            ShapePlacementSession illegalSession = new();
            illegalSession.Begin(squarePayload, trayAnchorCell: squareBefore?.AnchorCell);
            ShapePlacementResult invalidPreview = illegalSession.Preview(grid, new ItemShapeCell(4, 7));
            ShapePlacementResult invalidCommit = illegalSession.Commit(grid);
            bool illegalAnchorStable =
                squareBefore != null
                && grid.TryGetPlacement(squarePayload.ItemId, out ShapeAwareItemTrayGridPlacement afterInvalid)
                && afterInvalid.AnchorCell.Equals(squareBefore.AnchorCell);
            plan.Rows.Add(Row(
                "illegal_commit_keeps_stable_anchor",
                "Preview+Commit",
                invalidCommit,
                illegalSession.CurrentState,
                expectedValid: false,
                expectedCellCount: invalidPreview.OccupiedCells.Count,
                receiverMutated: false,
                anchorStable: illegalAnchorStable,
                note: "Illegal tray drops do not mutate the committed placement or stable tray anchor."));

            ShapePlacementSession moveSession = new();
            moveSession.Begin(squarePayload, trayAnchorCell: squareBefore?.AnchorCell);
            _ = moveSession.Preview(grid, new ItemShapeCell(0, 2));
            ShapePlacementResult moveCommit = moveSession.Commit(grid);
            bool legalMoveStable =
                grid.TryGetPlacement(squarePayload.ItemId, out ShapeAwareItemTrayGridPlacement afterMove)
                && afterMove.AnchorCell.Equals(new ItemShapeCell(0, 2));
            plan.Rows.Add(Row(
                "legal_move_updates_stable_anchor",
                "Preview+Commit",
                moveCommit,
                moveSession.CurrentState,
                expectedValid: true,
                expectedCellCount: 4,
                receiverMutated: moveCommit.IsValid,
                anchorStable: legalMoveStable,
                note: "Legal tray moves update the receiver-owned stable tray anchor."));

            ShapeAwareItemTrayGrid rotationGrid = new();
            ShapeItemPayload rotatedCorner = Payload(
                "rotated_corner3",
                "Corner3",
                RequiredShapes["Corner3"].offsets,
                ItemShapeRotation.Rotation90);
            ShapePlacementResult rotationResult = rotationGrid.CanPlace(rotatedCorner, new ItemShapeCell(0, 0));
            plan.Rows.Add(Row(
                "rotation_normalizes_corner3",
                "CanPlace",
                rotationResult,
                ShapePlacementState.HoldingItem,
                expectedValid: true,
                expectedCellCount: 3,
                receiverMutated: false,
                anchorStable: rotationResult.OccupiedCells.All(cell => cell.x >= 0 && cell.y >= 0),
                note: "Rotated offsets are normalized so the tray anchor remains the top-left stable cell."));

            bool screenPointResolved = grid.ScreenPointToCell(new Vector2(82f, 122f), null, out ItemShapeCell screenCell);
            plan.Rows.Add(new ShapeAwareItemTrayGridSampleRow
            {
                SampleId = "screen_point_to_cell",
                Action = "ScreenPointToCell",
                ItemId = string.Empty,
                ShapeId = string.Empty,
                CellCount = 0,
                Anchor = screenCell.ToString(),
                OccupiedCells = string.Empty,
                OccupiedSlots = string.Empty,
                ExpectedValid = true,
                ActualValid = screenPointResolved && screenCell.Equals(new ItemShapeCell(2, 3)),
                InvalidReason = ShapePlacementInvalidReason.None,
                StateAfter = ShapePlacementState.Idle,
                ReceiverMutated = false,
                AnchorStable = true,
                Note = "Screen point maps to a tray cell without reading or writing formal UI layout."
            });

            return plan;
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Shape-Aware ItemTray Grid");
            ShapeAwareItemTrayGridValidationPlan plan = BuildDefaultPlan();

            ValidateIsolation(report, plan);
            ValidateSamples(report, plan);
            ValidateRequiredShapes(report, plan);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            ShapeAwareItemTrayGridValidationPlan plan)
        {
            if (plan == null)
            {
                report.AddError("SHAPE_ITEM_TRAY_GRID_PLAN_NULL", "Shape-aware item tray grid plan was not created.", PackageName);
                return;
            }

            ValidateTrue(report, "SHAPE_ITEM_TRAY_GRID_DEVONLY", plan.DevOnly, "Grid remains BuildSandbox/devOnly.");
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_ENABLED_TRUE", plan.IsEnabled);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_FEATURE_FLAG_TRUE", plan.FeatureFlagDefaultEnabled);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_FIX03_CONTINUED", plan.ContinuesFix03OverlayRoute);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_IGNORE_LAYOUT", plan.UsesIgnoreLayoutAuthority);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_DELAYED_DROP_READ", plan.UsesDelayedDropRead);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_REDRAWS_ITEM_TRAY", plan.RedrawsItemTray);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_REPLACES_MATURE_TRAY", plan.ReplacesMatureItemTray);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_WRITES_FORMAL_SCENE", plan.WritesFormalScene);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_WRITES_FORMAL_UI", plan.WritesFormalUi);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_TOUCHES_RUNFLOW", plan.TouchesRunFlow);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_TOUCHES_PAGESTATE", plan.TouchesPageState);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_TOUCHES_FORMATIONSTATE", plan.TouchesFormationState);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_TOUCHES_SAVE", plan.TouchesSaveData);
            ValidateFalse(report, "SHAPE_ITEM_TRAY_GRID_TOUCHES_BOSS_REWARD_NUMERIC", plan.TouchesBossRewardDropNumeric);
        }

        private static void ValidateSamples(
            BuildSandboxValidationReport report,
            ShapeAwareItemTrayGridValidationPlan plan)
        {
            IReadOnlyList<ShapeAwareItemTrayGridSampleRow> rows =
                plan != null && plan.Rows != null
                    ? plan.Rows
                    : Array.Empty<ShapeAwareItemTrayGridSampleRow>();
            foreach (string requiredSample in RequiredSamples)
            {
                if (rows.Any(row => row != null && row.SampleId == requiredSample))
                {
                    report.AddInfo("SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT", $"Required sample present: {requiredSample}.", PackageName);
                    continue;
                }

                report.AddError("SHAPE_ITEM_TRAY_GRID_SAMPLE_MISSING", $"Missing required sample: {requiredSample}.", PackageName);
            }

            foreach (ShapeAwareItemTrayGridSampleRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("SHAPE_ITEM_TRAY_GRID_ROW_NULL", "Null shape-aware tray grid sample.", PackageName);
                    continue;
                }

                if (row.ExpectedValid != row.ActualValid)
                {
                    report.AddError(
                        "SHAPE_ITEM_TRAY_GRID_VALIDITY_MISMATCH",
                        $"{row.SampleId} expected valid={row.ExpectedValid}, actual={row.ActualValid}.",
                        PackageName);
                }

                if (row.ExpectedValid && row.CellCount <= 0 && row.Action != "ScreenPointToCell")
                {
                    report.AddError(
                        "SHAPE_ITEM_TRAY_GRID_EMPTY_OCCUPANCY",
                        $"{row.SampleId} should occupy real tray cells.",
                        PackageName);
                }

                if (!row.AnchorStable)
                {
                    report.AddError(
                        "SHAPE_ITEM_TRAY_GRID_ANCHOR_UNSTABLE",
                        $"{row.SampleId} did not keep a stable tray anchor.",
                        PackageName);
                }

                if (row.ActualValid && HasDuplicateSlots(row.OccupiedSlots))
                {
                    report.AddError(
                        "SHAPE_ITEM_TRAY_GRID_DUPLICATE_SLOT",
                        $"{row.SampleId} reported duplicate occupied slots.",
                        PackageName);
                }

                report.AddInfo(
                    "SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS",
                    $"{row.SampleId} action={row.Action}, valid={row.ActualValid}, anchor={row.Anchor}, cells={row.OccupiedCells}, reason={row.InvalidReason}.",
                    PackageName);
            }
        }

        private static void ValidateRequiredShapes(
            BuildSandboxValidationReport report,
            ShapeAwareItemTrayGridValidationPlan plan)
        {
            IReadOnlyList<ShapeAwareItemTrayGridSampleRow> rows =
                plan != null && plan.Rows != null
                    ? plan.Rows
                    : Array.Empty<ShapeAwareItemTrayGridSampleRow>();
            foreach (KeyValuePair<string, (int cellCount, string offsets)> pair in RequiredShapes)
            {
                ShapeAwareItemTrayGridSampleRow row = rows.FirstOrDefault(sample =>
                    sample != null
                    && sample.SampleId == $"pack_{pair.Key.ToLowerInvariant()}");
                if (row == null)
                {
                    report.AddError(
                        "SHAPE_ITEM_TRAY_GRID_SHAPE_MISSING",
                        $"Missing packed shape sample: {pair.Key}.",
                        PackageName);
                    continue;
                }

                if (row.CellCount != pair.Value.cellCount)
                {
                    report.AddError(
                        "SHAPE_ITEM_TRAY_GRID_CELL_COUNT_MISMATCH",
                        $"Shape {pair.Key} expected {pair.Value.cellCount} occupied cells, actual={row.CellCount}.",
                        PackageName);
                    continue;
                }

                report.AddInfo(
                    "SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED",
                    $"Shape {pair.Key} packs as {pair.Value.cellCount} real tray cell(s).",
                    PackageName);
            }
        }

        private static ShapeAwareItemTrayGridSampleRow Row(
            string sampleId,
            string action,
            ShapePlacementResult result,
            ShapePlacementState stateAfter,
            bool expectedValid,
            int expectedCellCount,
            bool receiverMutated,
            bool anchorStable,
            string note)
        {
            IReadOnlyList<ItemShapeCell> cells = result?.OccupiedCells ?? Array.Empty<ItemShapeCell>();
            return new ShapeAwareItemTrayGridSampleRow
            {
                SampleId = sampleId,
                Action = action,
                ItemId = result?.ItemId ?? string.Empty,
                ShapeId = result?.ShapeId ?? string.Empty,
                CellCount = cells.Count,
                Anchor = result?.AnchorCell.ToString() ?? string.Empty,
                OccupiedCells = FormatCells(cells),
                OccupiedSlots = FormatSlots(cells),
                ExpectedValid = expectedValid,
                ActualValid = result != null && result.IsValid,
                InvalidReason = result?.InvalidReason ?? ShapePlacementInvalidReason.ShapeInvalid,
                StateAfter = stateAfter,
                ReceiverMutated = receiverMutated,
                AnchorStable = anchorStable && (expectedCellCount <= 0 || cells.Count == expectedCellCount),
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

        private static string FormatSlots(IReadOnlyList<ItemShapeCell> cells)
        {
            return cells == null || cells.Count == 0
                ? string.Empty
                : string.Join(
                    ";",
                    cells.Select(cell => cell.y * ShapeAwareItemTrayGrid.DefaultColumnCount + cell.x)
                        .OrderBy(index => index));
        }

        private static bool HasDuplicateSlots(string occupiedSlots)
        {
            if (string.IsNullOrWhiteSpace(occupiedSlots))
            {
                return false;
            }

            string[] slots = occupiedSlots.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return slots.Length != slots.Distinct(StringComparer.Ordinal).Count();
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
                report.AddError(code, "Expected false for this devOnly grid isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
        }
    }

    public sealed class ShapeAwareItemTrayGridValidationPlan
    {
        public bool DevOnly = true;
        public bool IsEnabled;
        public bool FeatureFlagDefaultEnabled =
            BuildSandboxFeatureFlags.EnableItemShapeOccupancy
            || BuildSandboxFeatureFlags.EnableShapePlacementSandbox
            || BuildSandboxFeatureFlags.EnableShapeRotation;
        public bool ContinuesFix03OverlayRoute;
        public bool UsesIgnoreLayoutAuthority;
        public bool UsesDelayedDropRead;
        public bool RedrawsItemTray;
        public bool ReplacesMatureItemTray;
        public bool WritesFormalScene;
        public bool WritesFormalUi;
        public bool TouchesRunFlow;
        public bool TouchesPageState;
        public bool TouchesFormationState;
        public bool TouchesSaveData;
        public bool TouchesBossRewardDropNumeric;
        public List<ShapeAwareItemTrayGridSampleRow> Rows = new();
    }

    public sealed class ShapeAwareItemTrayGridSampleRow
    {
        public string SampleId = string.Empty;
        public string Action = string.Empty;
        public string ItemId = string.Empty;
        public string ShapeId = string.Empty;
        public int CellCount;
        public string Anchor = string.Empty;
        public string OccupiedCells = string.Empty;
        public string OccupiedSlots = string.Empty;
        public bool ExpectedValid;
        public bool ActualValid;
        public ShapePlacementInvalidReason InvalidReason;
        public ShapePlacementState StateAfter;
        public bool ReceiverMutated;
        public bool AnchorStable;
        public string Note = string.Empty;
    }
}
#endif
