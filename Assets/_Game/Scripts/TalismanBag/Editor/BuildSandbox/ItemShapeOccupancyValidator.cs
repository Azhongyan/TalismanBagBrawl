#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ItemShapeOccupancyValidator
    {
        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("ItemShape Occupancy");
            IReadOnlyList<ItemShapeConfig> shapes = BuildSandboxConfigValidator.CollectItemShapeConfigs();
            if (shapes.Count == 0)
            {
                report.AddError(
                    "ITEM_SHAPE_CONFIG_MISSING",
                    "ItemShapeOccupancy01 requires first-batch ItemShapeConfig assets.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return report;
            }

            ValidateSamplePlacements(report);
            BuildSandboxLayoutSnapshot snapshot = BuildSampleSnapshot();
            if (snapshot.placedItems.Count < 4)
            {
                report.AddError(
                    "LAYOUT_SNAPSHOT_PLACED_ITEMS_MISSING",
                    "Sandbox layout snapshot must carry occupiedCells for the four valid sample placements.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else if (snapshot.placedItems.Any(item => item.occupiedCells == null || item.occupiedCells.Count == 0))
            {
                report.AddError(
                    "LAYOUT_SNAPSHOT_OCCUPIED_CELLS_EMPTY",
                    "Every valid placed item snapshot must include occupiedCells.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "LAYOUT_SNAPSHOT_OCCUPIED_CELLS_PRESENT",
                    $"Sandbox snapshot placedItems={snapshot.placedItems.Count}, each with occupiedCells.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            return report;
        }

        public static IReadOnlyList<ShapePlacementResult> BuildSamplePlacementResults()
        {
            return BuildSamplePlacementResults(out _);
        }

        public static IReadOnlyList<ShapePlacementResult> BuildSamplePlacementResults(out GridOccupancyMap validMap)
        {
            Dictionary<string, ItemShapeConfig> shapes = BuildSandboxConfigValidator
                .CollectItemShapeConfigs()
                .Where(shape => shape != null && !string.IsNullOrWhiteSpace(shape.shapeId))
                .GroupBy(shape => shape.shapeId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            validMap = new GridOccupancyMap(8, 4);
            List<ShapePlacementResult> results = new();
            AddPlacement(results, validMap, shapes, "shape_single_sample", "Single1", new ItemShapeCell(0, 0));
            AddPlacement(results, validMap, shapes, "shape_vertical_sample", "Vertical2", new ItemShapeCell(2, 0));
            AddPlacement(results, validMap, shapes, "shape_corner_sample", "Corner3", new ItemShapeCell(4, 0));
            AddPlacement(results, validMap, shapes, "shape_square_sample", "Square4", new ItemShapeCell(6, 0));
            AddPlacement(results, new GridOccupancyMap(8, 4), shapes, "shape_out_of_grid_sample", "Square4", new ItemShapeCell(7, 3));

            GridOccupancyMap overlapMap = new(8, 4);
            AddPlacement(results, overlapMap, shapes, "shape_overlap_base", "Single1", new ItemShapeCell(0, 0));
            AddPlacement(results, overlapMap, shapes, "shape_overlap_sample", "Vertical2", new ItemShapeCell(0, 0));

            ShapePlacementResult missingShape = ItemShapePlacementValidator.ValidatePlacement(
                new ItemShapePlacementRequest("shape_missing_sample", "MissingShape", new ItemShapeCell(0, 0)),
                null,
                new GridOccupancyMap(8, 4));
            results.Add(missingShape);

            ItemShapeConfig invalidShape = ScriptableObject.CreateInstance<ItemShapeConfig>();
            invalidShape.shapeId = "InvalidShape";
            invalidShape.cellCount = 2;
            invalidShape.occupiedOffsets = new List<ItemShapeCell> { new(0, 0) };
            ShapePlacementResult invalidResult = ItemShapePlacementValidator.ValidatePlacement(
                new ItemShapePlacementRequest("shape_invalid_sample", "InvalidShape", new ItemShapeCell(0, 0)),
                invalidShape,
                new GridOccupancyMap(8, 4));
            results.Add(invalidResult);
            UnityEngine.Object.DestroyImmediate(invalidShape);

            return results;
        }

        public static BuildSandboxLayoutSnapshot BuildSampleSnapshot()
        {
            BuildSandboxLayoutSnapshot snapshot = new();
            foreach (ShapePlacementResult result in BuildSamplePlacementResults().Where(result => result.IsValid))
            {
                snapshot.AddPlacedItem(result);
            }

            return snapshot;
        }

        private static void ValidateSamplePlacements(BuildSandboxValidationReport report)
        {
            IReadOnlyList<ShapePlacementResult> results = BuildSamplePlacementResults();
            RequireResult(report, results, "shape_single_sample", true, ShapePlacementInvalidReason.None, 1);
            RequireResult(report, results, "shape_vertical_sample", true, ShapePlacementInvalidReason.None, 2);
            RequireResult(report, results, "shape_corner_sample", true, ShapePlacementInvalidReason.None, 3);
            RequireResult(report, results, "shape_square_sample", true, ShapePlacementInvalidReason.None, 4);
            RequireResult(report, results, "shape_out_of_grid_sample", false, ShapePlacementInvalidReason.OutOfGrid);
            RequireResult(report, results, "shape_overlap_sample", false, ShapePlacementInvalidReason.CellOccupied);
            RequireResult(report, results, "shape_missing_sample", false, ShapePlacementInvalidReason.MissingShapeConfig);
            RequireResult(report, results, "shape_invalid_sample", false, ShapePlacementInvalidReason.ShapeInvalid);
        }

        private static void RequireResult(
            BuildSandboxValidationReport report,
            IReadOnlyList<ShapePlacementResult> results,
            string itemId,
            bool expectedValid,
            ShapePlacementInvalidReason expectedReason,
            int expectedCellCount = -1)
        {
            ShapePlacementResult result = results.FirstOrDefault(r => r.ItemId == itemId);
            if (result == null)
            {
                report.AddError("PLACEMENT_SAMPLE_MISSING", $"Missing placement sample: {itemId}.", itemId);
                return;
            }

            if (result.IsValid != expectedValid || result.InvalidReason != expectedReason)
            {
                report.AddError(
                    "PLACEMENT_SAMPLE_REASON_MISMATCH",
                    $"{itemId} expected valid={expectedValid}, reason={expectedReason}; got valid={result.IsValid}, reason={result.InvalidReason}.",
                    itemId);
                return;
            }

            if (expectedCellCount >= 0 && result.OccupiedCells.Count != expectedCellCount)
            {
                report.AddError(
                    "PLACEMENT_SAMPLE_CELL_COUNT_MISMATCH",
                    $"{itemId} expected occupiedCells={expectedCellCount}; got {result.OccupiedCells.Count}.",
                    itemId);
                return;
            }

            report.AddInfo(
                "PLACEMENT_SAMPLE_PASS",
                $"{itemId} valid={result.IsValid}, reason={result.InvalidReason}, occupiedCells={result.OccupiedCells.Count}.",
                itemId);
        }

        private static void AddPlacement(
            List<ShapePlacementResult> results,
            GridOccupancyMap map,
            IReadOnlyDictionary<string, ItemShapeConfig> shapes,
            string itemId,
            string shapeId,
            ItemShapeCell anchorCell)
        {
            shapes.TryGetValue(shapeId, out ItemShapeConfig shapeConfig);
            ShapePlacementResult result = map.TryPlace(
                new ItemShapePlacementRequest(itemId, shapeId, anchorCell),
                shapeConfig);
            results.Add(result);
        }
    }
}
#endif
