using System.Collections.Generic;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [CreateAssetMenu(
        menuName = "Talisman Bag/BuildSandbox/Item Shape Config",
        fileName = "ItemShapeConfig")]
    public sealed class ItemShapeConfig : BuildSandboxDevOnlyConfig
    {
        public string shapeId = "Single1";
        public string shapeName = "Single 1x1";
        public int cellCount = 1;
        public List<ItemShapeCell> occupiedOffsets = new();
        public ItemShapeRotation defaultRotation = ItemShapeRotation.Rotation0;
        public bool rotationAllowed;
        public string visualKey = "shape_single1_placeholder";
    }
}
