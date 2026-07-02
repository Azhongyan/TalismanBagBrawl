using System;
using System.Collections.Generic;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildSandboxPlacedItemSnapshot
    {
        public string itemId = string.Empty;
        public string shapeId = string.Empty;
        public ItemShapeCell anchorCell;
        public List<ItemShapeCell> occupiedCells = new();
        public ItemShapeRotation rotation = ItemShapeRotation.Rotation0;
        public List<string> tags = new();
        public bool isPowered;
        public string energySourceId = string.Empty;
        public List<string> affixList = new();
        public string rarity = "sandbox";

        public static BuildSandboxPlacedItemSnapshot FromPlacementResult(ShapePlacementResult result)
        {
            BuildSandboxPlacedItemSnapshot snapshot = new();
            if (result == null)
            {
                return snapshot;
            }

            snapshot.itemId = result.ItemId;
            snapshot.shapeId = result.ShapeId;
            snapshot.anchorCell = result.AnchorCell;
            snapshot.occupiedCells = new List<ItemShapeCell>(result.OccupiedCells);
            snapshot.isPowered = result.EnergyConnected;
            snapshot.energySourceId = result.EnergyConnectionNote;
            return snapshot;
        }
    }
}
