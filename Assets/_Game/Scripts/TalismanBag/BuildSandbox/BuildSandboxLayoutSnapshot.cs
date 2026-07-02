using System;
using System.Collections.Generic;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BuildSandboxLayoutSnapshot
    {
        public List<BuildSandboxPlacedItemSnapshot> placedItems = new();

        public void AddPlacedItem(ShapePlacementResult result)
        {
            if (result == null || !result.IsValid)
            {
                return;
            }

            placedItems.Add(BuildSandboxPlacedItemSnapshot.FromPlacementResult(result));
        }
    }
}
