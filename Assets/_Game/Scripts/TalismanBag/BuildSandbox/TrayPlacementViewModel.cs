using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public sealed class TrayPlacementViewModel
    {
        public string itemId;
        public string shapeId;
        public int anchorSlotIndex;
        public IReadOnlyList<int> occupiedSlotIndexes;
        public int rotation;
        public bool isValid;

        public TrayPlacementViewModel()
        {
            itemId = string.Empty;
            shapeId = string.Empty;
            occupiedSlotIndexes = Array.Empty<int>();
        }

        public static TrayPlacementViewModel FromPlacement(
            ShapeAwareItemTrayGridPlacement placement,
            bool valid = true,
            int columnCount = ShapeAwareItemTrayGrid.DefaultColumnCount)
        {
            if (placement == null)
            {
                return new TrayPlacementViewModel
                {
                    shapeId = string.Empty,
                    occupiedSlotIndexes = Array.Empty<int>(),
                    isValid = false
                };
            }

            int safeColumnCount = Math.Max(1, columnCount);
            return new TrayPlacementViewModel
            {
                itemId = placement.ItemId ?? string.Empty,
                shapeId = placement.ShapeId ?? string.Empty,
                anchorSlotIndex = placement.AnchorCell.y * safeColumnCount + placement.AnchorCell.x,
                occupiedSlotIndexes = placement.OccupiedSlotIndexes
                    .OrderBy(slotIndex => slotIndex)
                    .ToArray(),
                rotation = (int)placement.Rotation,
                isValid = valid
            };
        }
    }
}
