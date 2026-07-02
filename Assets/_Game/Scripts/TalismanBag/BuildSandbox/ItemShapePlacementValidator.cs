using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class ItemShapePlacementValidator
    {
        public static ShapePlacementResult ValidatePlacement(
            ItemShapePlacementRequest request,
            ItemShapeConfig shapeConfig,
            GridOccupancyMap occupancyMap)
        {
            if (request == null || shapeConfig == null)
            {
                return Invalid(
                    request,
                    ShapePlacementInvalidReason.MissingShapeConfig,
                    new List<ItemShapeCell>());
            }

            if (occupancyMap == null || !IsShapeConfigValid(shapeConfig))
            {
                return Invalid(request, ShapePlacementInvalidReason.ShapeInvalid, new List<ItemShapeCell>());
            }

            if (!string.Equals(request.ShapeId, shapeConfig.shapeId, System.StringComparison.Ordinal))
            {
                return Invalid(request, ShapePlacementInvalidReason.MissingShapeConfig, new List<ItemShapeCell>());
            }

            if (request.Rotation != shapeConfig.defaultRotation && !shapeConfig.rotationAllowed)
            {
                return Invalid(request, ShapePlacementInvalidReason.ShapeInvalid, new List<ItemShapeCell>());
            }

            IReadOnlyList<ItemShapeCell> occupiedCells =
                CalculateOccupiedCells(shapeConfig, request.AnchorCell, request.Rotation);
            if (occupiedCells.Count == 0)
            {
                return Invalid(request, ShapePlacementInvalidReason.ShapeInvalid, occupiedCells);
            }

            if (occupiedCells.Any(cell => !occupancyMap.IsWithinBounds(cell)))
            {
                return Invalid(request, ShapePlacementInvalidReason.OutOfGrid, occupiedCells);
            }

            if (occupiedCells.Any(occupancyMap.IsOccupied))
            {
                return Invalid(request, ShapePlacementInvalidReason.CellOccupied, occupiedCells);
            }

            IReadOnlyList<string> adjacentItems =
                occupancyMap.CollectAdjacentItems(occupiedCells, request.ItemId);
            return new ShapePlacementResult(
                request.ItemId,
                request.ShapeId,
                request.AnchorCell,
                occupiedCells,
                isValid: true,
                ShapePlacementInvalidReason.None,
                adjacentItems,
                energyConnected: false,
                energyConnectionNote: "sandbox_placeholder");
        }

        public static IReadOnlyList<ItemShapeCell> CalculateOccupiedCells(
            ItemShapeConfig shapeConfig,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation = ItemShapeRotation.Rotation0)
        {
            if (!IsShapeConfigValid(shapeConfig))
            {
                return new List<ItemShapeCell>();
            }

            return shapeConfig.occupiedOffsets
                .Select(offset => ApplyRotation(offset, rotation))
                .Select(offset => new ItemShapeCell(anchorCell.x + offset.x, anchorCell.y + offset.y))
                .ToArray();
        }

        public static bool IsShapeConfigValid(ItemShapeConfig shapeConfig)
        {
            if (shapeConfig == null
                || string.IsNullOrWhiteSpace(shapeConfig.shapeId)
                || shapeConfig.cellCount <= 0
                || shapeConfig.occupiedOffsets == null
                || shapeConfig.occupiedOffsets.Count == 0
                || shapeConfig.cellCount != shapeConfig.occupiedOffsets.Count)
            {
                return false;
            }

            HashSet<ItemShapeCell> offsets = new();
            foreach (ItemShapeCell offset in shapeConfig.occupiedOffsets)
            {
                if (offset.x < 0 || offset.y < 0)
                {
                    return false;
                }

                if (!offsets.Add(offset))
                {
                    return false;
                }
            }

            return true;
        }

        private static ShapePlacementResult Invalid(
            ItemShapePlacementRequest request,
            ShapePlacementInvalidReason reason,
            IReadOnlyList<ItemShapeCell> occupiedCells)
        {
            return new ShapePlacementResult(
                request?.ItemId ?? string.Empty,
                request?.ShapeId ?? string.Empty,
                request?.AnchorCell ?? new ItemShapeCell(),
                occupiedCells,
                isValid: false,
                reason,
                energyConnected: false,
                energyConnectionNote: "sandbox_placeholder");
        }

        private static ItemShapeCell ApplyRotation(ItemShapeCell offset, ItemShapeRotation rotation)
        {
            switch (rotation)
            {
                case ItemShapeRotation.Rotation90:
                    return new ItemShapeCell(offset.y, -offset.x);
                case ItemShapeRotation.Rotation180:
                    return new ItemShapeCell(-offset.x, -offset.y);
                case ItemShapeRotation.Rotation270:
                    return new ItemShapeCell(-offset.y, offset.x);
                default:
                    return offset;
            }
        }
    }
}
