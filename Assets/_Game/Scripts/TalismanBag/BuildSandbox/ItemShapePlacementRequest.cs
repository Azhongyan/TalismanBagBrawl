namespace TalismanBag.BuildSandbox
{
    public sealed class ItemShapePlacementRequest
    {
        public ItemShapePlacementRequest(
            string itemId,
            string shapeId,
            ItemShapeCell anchorCell,
            ItemShapeRotation rotation = ItemShapeRotation.Rotation0)
        {
            ItemId = itemId ?? string.Empty;
            ShapeId = shapeId ?? string.Empty;
            AnchorCell = anchorCell;
            Rotation = rotation;
        }

        public string ItemId { get; }
        public string ShapeId { get; }
        public ItemShapeCell AnchorCell { get; }
        public ItemShapeRotation Rotation { get; }
    }
}
