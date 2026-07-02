using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public sealed class TrayPlacementViewModel
    {
        public string itemId;
        public int anchorSlotIndex;
        public IReadOnlyList<int> occupiedSlotIndexes;
        public int rotation;
        public bool isValid;

        public TrayPlacementViewModel()
        {
            itemId = string.Empty;
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
                    isValid = false
                };
            }

            return new TrayPlacementViewModel
            {
                itemId = placement.ItemId ?? string.Empty,
                anchorSlotIndex = placement.AnchorCell.y * Math.Max(1, columnCount) + placement.AnchorCell.x,
                occupiedSlotIndexes = placement.OccupiedSlotIndexes
                    .OrderBy(slotIndex => slotIndex)
                    .ToArray(),
                rotation = (int)placement.Rotation,
                isValid = valid
            };
        }
    }
}
