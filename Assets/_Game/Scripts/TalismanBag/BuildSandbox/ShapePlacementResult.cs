using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public sealed class ShapePlacementResult
    {
        public ShapePlacementResult(
            string itemId,
            string shapeId,
            ItemShapeCell anchorCell,
            IReadOnlyList<ItemShapeCell> occupiedCells,
            bool isValid,
            ShapePlacementInvalidReason invalidReason,
            IReadOnlyList<string> adjacentItems = null,
            bool energyConnected = false,
            string energyConnectionNote = "sandbox_placeholder")
        {
            ItemId = itemId ?? string.Empty;
            ShapeId = shapeId ?? string.Empty;
            AnchorCell = anchorCell;
            OccupiedCells = (occupiedCells ?? new List<ItemShapeCell>()).ToArray();
            IsValid = isValid;
            InvalidReason = invalidReason;
            AdjacentItems = (adjacentItems ?? new List<string>()).ToArray();
            EnergyConnected = energyConnected;
            EnergyConnectionNote = energyConnectionNote ?? string.Empty;
        }

        public string ItemId { get; }
        public string ShapeId { get; }
        public ItemShapeCell AnchorCell { get; }
        public IReadOnlyList<ItemShapeCell> OccupiedCells { get; }
        public bool IsValid { get; }
        public ShapePlacementInvalidReason InvalidReason { get; }
        public IReadOnlyList<string> AdjacentItems { get; }
        public bool EnergyConnected { get; }
        public string EnergyConnectionNote { get; }
    }
}
