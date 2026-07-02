# ShapePlacementSession Tech Survey Report

Package: `V0.4-ShapePlacementSession01-TechSurvey`
Date: 2026-07-01
Status: `SURVEY_COMPLETE / DESIGN_FEASIBLE / NO_RUNTIME_CHANGE`

## Executive Conclusion

Fix03 must remain stopped. The failed route was not a hand-test mistake and not a single `RectTransform` offset issue. The structural problem is that visual, layout, drag, drop, and anchor state each have their own authority:

- Visual authority: `ShapeAwareItemTrayFixtureView` writes footprint `RectTransform`, cell visuals, and size.
- Layout authority: mature V03 tray `GridLayoutGroup`, `ItemTrayGridSlot_##`, `MoveItemToTray`, and `MoveItemWithinTray`.
- Drag authority: `DraggableTalismanItemView` owns drag parent, home parent, original position, snapback, and `placedDuringDrag`.
- Drop authority: `V03ItemTraySlotDropTarget.OnDrop` and `TalismanGridSlotView.OnDrop` consume the dragged view directly.
- Anchor authority: BuildSandbox `ShapeAwareTrayPlacementState` and `ShapeAwareTrayPackingMap` track shape anchor state separately.
- Timing bridge: `ShapeAwareTrayDragPersistence` waits one frame after drag end and tries to infer the final state after other systems have already acted.

The `ShapePlacementSession + ShapeItemPayload + ShapeGridReceiver` direction is feasible, but only if it becomes the single state source for preview and placement decisions. The first implementation should stay in the BuildSandbox/dev-only extension layer and should not rewrite mature V03 UI, formal scenes, save data, Boss, rewards, or numeric systems.

## Evidence From Current Code

- `Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/V03BattlePrepareInteractionController.cs`
  - Creates/binds the mature tray under `V03BattlePrepareItemTrayRoot`.
  - `ItemTrayScroll/Content` uses `GridLayoutGroup` and fixed `ItemTrayGridSlot_##` cells.
  - `MoveItemToTray` and `MoveItemWithinTray` assume a single visual item belongs to a single slot.
  - `V03ItemTraySlotDropTarget.OnDrop` reads `eventData.pointerDrag` and calls mature slot-swap logic.
- `Assets/_Game/Scripts/TalismanBag/UI/DraggableTalismanItemView.cs`
  - The dragged view is the implicit payload.
  - It owns original parent, original anchored position, home parent, home anchored position, current slot, drag parent, and snapback.
  - Drop receivers must call `TryPlaceOnSlot` or `AcceptInventoryDrop`; otherwise `OnEndDrag` returns the item.
- `Assets/_Game/Scripts/TalismanBag/UI/TalismanGridSlotView.cs`
  - Board drop is single-cell and direct: `OnDrop -> DraggableTalismanItemView.TryPlaceOnSlot(this)`.
- `Assets/_Game/Scripts/TalismanBag/Grid/TalismanBagGrid.cs`
  - Formal grid placement is single-cell through `CanPlaceItem` and `PlaceItem`.
  - Multi-cell board commit is not authorized for this package.
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayFixtureView.cs`
  - Provides multi-cell visuals by writing overlay layout, not by owning placement.
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs`
  - Proves shape configs, tray packing maps, board cell lookup, and occupancy previews are available.
  - Also demonstrates the failed architecture: shape state is inferred around mature drag/drop after the fact.
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
  - Proves `ScreenPointToCell -> validate -> preview -> commit` is feasible in a temporary V04 test bench.

## Survey Answers

1. V03 tray layout authority is the mature battle-prepare tray built or bound by `V03BattlePrepareInteractionController`: `V03BattlePrepareItemTrayRoot`, `ItemTrayScroll/Viewport/Content`, `GridLayoutGroup`, and `ItemTrayGridSlot_##`. It is single-cell slot parenting, not shape-footprint placement.

2. V03 drag payload is currently the `DraggableTalismanItemView` itself, plus its `RuntimeItem`, `Definition`, `CurrentSlot`, original parent/position, and captured home parent/position. There is no explicit shape payload.

3. Drop targets are slot-level receivers. Tray drops go through `V03ItemTraySlotDropTarget.OnDrop -> MoveItemWithinTray`. Board drops go through `TalismanGridSlotView.OnDrop -> DraggableTalismanItemView.TryPlaceOnSlot -> TalismanBagGrid.PlaceItem`.

4. `ScreenPointToCell` is feasible. Board lookup can reuse the existing `TalismanGridSlotView` rect iteration and `RectTransformUtility.RectangleContainsScreenPoint` approach already used by BuildSandbox runtime preview. Tray lookup can map pointer position to `ItemTrayGridSlot_##` rects or slot indexes. The missing part is a central receiver contract.

5. The board can become a read-only `ShapeGridReceiver` for preview. It can resolve board cells, run `GridOccupancyMap` validation, and render preview highlights. Commit must remain deferred/dev-only because formal `TalismanBagGrid` is single-cell and protected.

6. The tray can become a `ShapeGridReceiver` through a BuildSandbox adapter over the 40 mature V03 tray slots. Its state should be the session's tray anchor and occupied cells, not `ignoreLayout` overlay placement. A renderer may draw shape visuals from session state, but it cannot be another placement authority.

7. `ShapePlacementSession` should live first in the BuildSandbox/dev-only extension layer. It should not be embedded directly into mature `V03BattlePrepareInteractionController` in this survey package. Later promotion can expose small adapter seams only after Guard/user approval.

8. Required adapter seams are:
   - `ShapeItemPayload` factory from fixture item id / shape config / runtime item reference.
   - Tray receiver adapter over mature tray slots.
   - Board receiver adapter over `TalismanGridSlotView` cells.
   - Drag bridge that begins, updates, previews, commits, or cancels through the session.
   - Renderer that reads session state as its only visual source.
   - Optional future pre-drop hook so mature snapback logic does not race the shape session.

9. This redesign must not touch formal V03 layout constants, V03 hand-tuned tray/board positions, `TalismanBagGrid` formal placement semantics, save data, Boss, rewards, numeric balance, RunFlow, PageState, FormationState, or formal scenes.

10. Follow-up packages should be split:
    - `V0.4-ShapePlacementSession01`: pure state source, payload, receiver contract, validator integration.
    - `V0.4-ShapeAwareItemTrayGrid01`: dev-only shape-aware tray receiver/packing and renderer.
    - `V0.4-MobileShapePlacementInteraction01`: select, rotate, ghost drag, `PreviewLocked`, confirm, and cancel through the session.

## Proposed Unified State Source

`ShapePlacementSession` should be the single runtime state authority for shape placement attempts.

Suggested fields:

```csharp
public sealed class ShapePlacementSession
{
    public string SelectedItemId;
    public string ShapeId;
    public ItemShapeRotation Rotation;
    public ShapePlacementSource SourceContainer;
    public ShapePlacementState CurrentState;
    public ItemShapeCell? TrayAnchorCell;
    public ItemShapeCell? BoardAnchorCell;
    public IReadOnlyList<ItemShapeCell> OccupiedCells;
    public ItemShapeCell? LastLegalTrayAnchor;
    public ItemShapeCell? LastLegalBoardAnchor;
    public string ActiveReceiverId;
    public ShapePlacementResult PreviewResult;
    public bool IsPreviewLocked;
    public Vector2 LastPointerScreenPosition;
}
```

`ShapeItemPayload` should be the explicit drag/placement payload.

Suggested fields:

```csharp
public readonly struct ShapeItemPayload
{
    public readonly string ItemId;
    public readonly string ShapeId;
    public readonly ItemShapeRotation Rotation;
    public readonly IReadOnlyList<ItemShapeCell> OccupiedOffsets;
    public readonly ShapePlacementSource Source;
}
```

`IShapeGridReceiver` should be the only placement-facing contract for tray and board.

Suggested interface:

```csharp
public interface IShapeGridReceiver
{
    string ReceiverId { get; }
    bool ScreenPointToCell(Vector2 screenPoint, Camera eventCamera, out ItemShapeCell anchorCell);
    ShapePlacementResult CanPlace(ShapeItemPayload payload, ItemShapeCell anchorCell);
    void Preview(ShapePlacementSession session, ShapePlacementResult result);
    ShapePlacementResult Commit(ShapePlacementSession session);
    void Cancel(ShapePlacementSession session);
}
```

Preview must be side-effect-light. Commit is the only method allowed to mutate receiver-owned placement state, and board commit must remain disabled or dev-only until a separate formal-grid package is authorized.

## Forbidden Patch Routes

- Do not continue overlay plus `ignoreLayout` as placement authority.
- Do not use delayed one-frame reads after `OnEndDrag` as the commit path.
- Do not patch individual `RectTransform` offsets as if this were a coordinate bug.
- Do not create a second drag payload beside `DraggableTalismanItemView` without a session handoff.
- Do not let tray layout, drag home, drop result, and shape anchor each write final position.
- Do not modify formal board commit, save, Boss, rewards, numeric balance, or main flow for this package.

## Follow-Up Recommendation

Proceed with a report-first redesign package. The next implementation package should create pure BuildSandbox/dev-only session and receiver contracts, then wire a tray/board adapter in a controlled fixture. Mobile interaction should wait until this contract exists, because mobile confirm/cancel semantics need a stable session state and cannot be reliable on top of Fix03's delayed post-drop inference.
