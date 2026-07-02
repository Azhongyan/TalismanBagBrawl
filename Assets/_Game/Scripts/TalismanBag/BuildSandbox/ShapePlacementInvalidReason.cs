namespace TalismanBag.BuildSandbox
{
    public enum ShapePlacementInvalidReason
    {
        None = 0,
        OutOfGrid = 1,
        CellOccupied = 2,
        ShapeInvalid = 3,
        MissingShapeConfig = 4,
        CommitDisabled = 5
    }
}
