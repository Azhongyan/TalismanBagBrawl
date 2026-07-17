using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    public enum ItemGridPlacementInvalidReason
    {
        None = 0,
        MissingItem = 1,
        ShapeInvalid = 2,
        OutOfGrid = 3,
        EyeCellCovered = 4,
        CellOccupied = 5
    }

    public sealed class ItemGridPlacedItemPreview
    {
        private readonly Vector2Int[] occupiedCells;

        public ItemGridPlacedItemPreview(
            string itemId,
            string displayName,
            Vector2Int anchorCell,
            IReadOnlyList<Vector2Int> occupiedCells,
            Vector2Int coreCellWorld,
            string placementId = null)
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = NormalizePlacementId(placementId, this.itemId);
            this.displayName = displayName ?? string.Empty;
            this.anchorCell = anchorCell;
            this.occupiedCells = (occupiedCells ?? Array.Empty<Vector2Int>()).ToArray();
            this.coreCellWorld = coreCellWorld;
        }

        public string itemId;
        public string placementId;
        public string displayName;
        public Vector2Int anchorCell;
        public Vector2Int coreCellWorld;
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;

        public bool ContainsCell(Vector2Int cell)
        {
            return occupiedCells.Contains(cell);
        }

        private static string NormalizePlacementId(string placementId, string itemId)
        {
            return string.IsNullOrWhiteSpace(placementId)
                ? itemId ?? string.Empty
                : placementId;
        }
    }

    public sealed class ItemGridPlacementEvaluation
    {
        private readonly Vector2Int[] shapeCells;
        private readonly Vector2Int[] occupiedCells;

        public ItemGridPlacementEvaluation(
            string itemId,
            string displayName,
            Vector2Int anchorCell,
            IReadOnlyList<Vector2Int> shapeCells,
            IReadOnlyList<Vector2Int> occupiedCells,
            Vector2Int coreCellLocal,
            Vector2Int coreCellWorld,
            ItemGridPlacementInvalidReason invalidReason,
            string message,
            string placementId = null)
        {
            this.itemId = itemId ?? string.Empty;
            this.placementId = NormalizePlacementId(placementId, this.itemId);
            this.displayName = displayName ?? string.Empty;
            this.anchorCell = anchorCell;
            this.shapeCells = (shapeCells ?? Array.Empty<Vector2Int>()).ToArray();
            this.occupiedCells = (occupiedCells ?? Array.Empty<Vector2Int>()).ToArray();
            this.coreCellLocal = coreCellLocal;
            this.coreCellWorld = coreCellWorld;
            this.invalidReason = invalidReason;
            this.message = message ?? string.Empty;
        }

        public string itemId;
        public string placementId;
        public string displayName;
        public Vector2Int anchorCell;
        public Vector2Int coreCellLocal;
        public Vector2Int coreCellWorld;
        public ItemGridPlacementInvalidReason invalidReason;
        public string message;
        public IReadOnlyList<Vector2Int> ShapeCells => shapeCells;
        public IReadOnlyList<Vector2Int> OccupiedCells => occupiedCells;
        public bool IsValid => invalidReason == ItemGridPlacementInvalidReason.None;

        private static string NormalizePlacementId(string placementId, string itemId)
        {
            return string.IsNullOrWhiteSpace(placementId)
                ? itemId ?? string.Empty
                : placementId;
        }
    }

    public static class ItemGridPlacementRulePreview
    {
        private static readonly Vector2Int EyeCellValue = new(2, 2);

        private static readonly Vector2Int[] ArrayBonusCellValues =
        {
            new(2, 1),
            new(2, 3),
            new(1, 2),
            new(3, 2)
        };

        public static int BoardSize => 5;
        public static Vector2Int EyeCell => EyeCellValue;
        public static IReadOnlyList<Vector2Int> ArrayBonusCells => ArrayBonusCellValues;

        public static ItemGridPlacementEvaluation Evaluate(
            ItemInnerDataDefinition item,
            Vector2Int anchorCell,
            IEnumerable<ItemGridPlacedItemPreview> lockedPlacements = null,
            string placementId = null)
        {
            if (item == null)
            {
                return new ItemGridPlacementEvaluation(
                    string.Empty,
                    string.Empty,
                    anchorCell,
                    Array.Empty<Vector2Int>(),
                    Array.Empty<Vector2Int>(),
                    Vector2Int.zero,
                    anchorCell,
                    ItemGridPlacementInvalidReason.MissingItem,
                    "Missing catalog item.",
                    placementId);
            }

            IReadOnlyList<Vector2Int> shapeCells = item.ShapeCells ?? Array.Empty<Vector2Int>();
            List<Vector2Int> occupiedCells = BuildOccupiedCells(shapeCells, anchorCell);
            Vector2Int coreCellWorld = anchorCell + item.coreCellLocal;

            if (!HasValidShape(item, shapeCells))
            {
                return BuildResult(
                    item,
                    anchorCell,
                    shapeCells,
                    occupiedCells,
                    coreCellWorld,
                    ItemGridPlacementInvalidReason.ShapeInvalid,
                    "shapeCells are empty, duplicated, or do not include coreCellLocal.",
                    placementId);
            }

            if (occupiedCells.Any(cell => !IsWithinBoard(cell)))
            {
                return BuildResult(
                    item,
                    anchorCell,
                    shapeCells,
                    occupiedCells,
                    coreCellWorld,
                    ItemGridPlacementInvalidReason.OutOfGrid,
                    "occupiedCells exceed the 5x5 board.",
                    placementId);
            }

            if (occupiedCells.Contains(EyeCell))
            {
                return BuildResult(
                    item,
                    anchorCell,
                    shapeCells,
                    occupiedCells,
                    coreCellWorld,
                    ItemGridPlacementInvalidReason.EyeCellCovered,
                    "occupiedCells cover the fixed eyeCell (2,2).",
                    placementId);
            }

            HashSet<Vector2Int> lockedCells = CollectLockedCells(lockedPlacements);
            if (occupiedCells.Any(lockedCells.Contains))
            {
                return BuildResult(
                    item,
                    anchorCell,
                    shapeCells,
                    occupiedCells,
                    coreCellWorld,
                    ItemGridPlacementInvalidReason.CellOccupied,
                    "occupiedCells overlap an existing preview placement.",
                    placementId);
            }

            return BuildResult(
                item,
                anchorCell,
                shapeCells,
                occupiedCells,
                coreCellWorld,
                ItemGridPlacementInvalidReason.None,
                "Placement preview is valid.",
                placementId);
        }

        public static List<Vector2Int> BuildOccupiedCells(IReadOnlyList<Vector2Int> shapeCells, Vector2Int anchorCell)
        {
            List<Vector2Int> cells = new();
            if (shapeCells == null)
            {
                return cells;
            }

            foreach (Vector2Int localCell in shapeCells)
            {
                cells.Add(anchorCell + localCell);
            }

            return cells;
        }

        public static bool IsWithinBoard(Vector2Int cell)
        {
            return cell.x >= 0
                && cell.y >= 0
                && cell.x < BoardSize
                && cell.y < BoardSize;
        }

        public static bool IsEyeCell(Vector2Int cell)
        {
            return cell == EyeCell;
        }

        public static bool IsArrayBonusCell(Vector2Int cell)
        {
            return ArrayBonusCellValues.Contains(cell);
        }

        public static string FormatCell(Vector2Int cell)
        {
            return $"({cell.x},{cell.y})";
        }

        public static string FormatCells(IEnumerable<Vector2Int> cells)
        {
            return string.Join(";", (cells ?? Array.Empty<Vector2Int>()).Select(FormatCell));
        }

        public static string BuildStatusText(ItemGridPlacementEvaluation evaluation)
        {
            if (evaluation == null)
            {
                return "No placement preview.";
            }

            string state = evaluation.IsValid ? "PASS" : "BLOCKED";
            return string.Join(Environment.NewLine, new[]
            {
                $"Placement: {state} / {evaluation.invalidReason}",
                $"Item: {evaluation.itemId} {evaluation.displayName}",
                $"placementId: {evaluation.placementId}",
                $"AnchorCell: {FormatCell(evaluation.anchorCell)}",
                $"shapeCells: {FormatCells(evaluation.ShapeCells)}",
                $"occupiedCells: {FormatCells(evaluation.OccupiedCells)}",
                $"coreCellLocal: {FormatCell(evaluation.coreCellLocal)}",
                $"coreCellWorld: {FormatCell(evaluation.coreCellWorld)}",
                $"eyeCell: {FormatCell(EyeCell)} / cannot be covered",
                $"arrayBonusCells: {FormatCells(ArrayBonusCells)} / identity only",
                evaluation.message
            });
        }

        private static bool HasValidShape(ItemInnerDataDefinition item, IReadOnlyList<Vector2Int> shapeCells)
        {
            return item != null
                && shapeCells != null
                && shapeCells.Count > 0
                && shapeCells.Distinct().Count() == shapeCells.Count
                && shapeCells.Contains(item.coreCellLocal);
        }

        private static HashSet<Vector2Int> CollectLockedCells(IEnumerable<ItemGridPlacedItemPreview> lockedPlacements)
        {
            HashSet<Vector2Int> cells = new();
            if (lockedPlacements == null)
            {
                return cells;
            }

            foreach (ItemGridPlacedItemPreview placement in lockedPlacements)
            {
                if (placement == null)
                {
                    continue;
                }

                foreach (Vector2Int cell in placement.OccupiedCells)
                {
                    cells.Add(cell);
                }
            }

            return cells;
        }

        private static ItemGridPlacementEvaluation BuildResult(
            ItemInnerDataDefinition item,
            Vector2Int anchorCell,
            IReadOnlyList<Vector2Int> shapeCells,
            IReadOnlyList<Vector2Int> occupiedCells,
            Vector2Int coreCellWorld,
            ItemGridPlacementInvalidReason invalidReason,
            string message,
            string placementId = null)
        {
            return new ItemGridPlacementEvaluation(
                item.itemId,
                item.displayName,
                anchorCell,
                shapeCells,
                occupiedCells,
                item.coreCellLocal,
                coreCellWorld,
                invalidReason,
                message,
                placementId);
        }
    }
}
