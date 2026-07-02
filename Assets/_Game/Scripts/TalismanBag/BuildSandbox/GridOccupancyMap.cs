using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public sealed class GridOccupancyMap
    {
        private readonly Dictionary<ItemShapeCell, string> occupiedByItemId = new();

        public GridOccupancyMap(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Grid width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Grid height must be positive.");
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyDictionary<ItemShapeCell, string> OccupiedCells => occupiedByItemId;

        public bool IsWithinBounds(ItemShapeCell cell)
        {
            return cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        }

        public bool IsOccupied(ItemShapeCell cell)
        {
            return occupiedByItemId.ContainsKey(cell);
        }

        public bool TryGetOccupant(ItemShapeCell cell, out string itemId)
        {
            return occupiedByItemId.TryGetValue(cell, out itemId);
        }

        public ShapePlacementResult TryPlace(ItemShapePlacementRequest request, ItemShapeConfig shapeConfig)
        {
            ShapePlacementResult result = ItemShapePlacementValidator.ValidatePlacement(request, shapeConfig, this);
            if (!result.IsValid)
            {
                return result;
            }

            string itemId = string.IsNullOrWhiteSpace(request.ItemId) ? request.ShapeId : request.ItemId;
            foreach (ItemShapeCell cell in result.OccupiedCells)
            {
                occupiedByItemId[cell] = itemId;
            }

            return result;
        }

        public IReadOnlyList<string> CollectAdjacentItems(IReadOnlyList<ItemShapeCell> cells, string selfItemId)
        {
            HashSet<ItemShapeCell> ownCells = new(cells ?? Array.Empty<ItemShapeCell>());
            HashSet<string> adjacent = new(StringComparer.Ordinal);
            foreach (ItemShapeCell cell in ownCells)
            {
                foreach (ItemShapeCell neighbor in EnumerateCardinalNeighbors(cell))
                {
                    if (ownCells.Contains(neighbor))
                    {
                        continue;
                    }

                    if (occupiedByItemId.TryGetValue(neighbor, out string itemId)
                        && !string.IsNullOrWhiteSpace(itemId)
                        && !string.Equals(itemId, selfItemId, StringComparison.Ordinal))
                    {
                        adjacent.Add(itemId);
                    }
                }
            }

            return adjacent.OrderBy(id => id, StringComparer.Ordinal).ToArray();
        }

        private static IEnumerable<ItemShapeCell> EnumerateCardinalNeighbors(ItemShapeCell cell)
        {
            yield return new ItemShapeCell(cell.x + 1, cell.y);
            yield return new ItemShapeCell(cell.x - 1, cell.y);
            yield return new ItemShapeCell(cell.x, cell.y + 1);
            yield return new ItemShapeCell(cell.x, cell.y - 1);
        }
    }
}
