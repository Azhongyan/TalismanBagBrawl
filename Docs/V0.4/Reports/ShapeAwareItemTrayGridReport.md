# Shape-Aware ItemTray Grid Report

Package: `V0.4-ShapeAwareItemTrayGrid01`
Generated: `2026-07-01 19:45:24`
Status: `PASS_DEVONLY_TRAY_GRID_READY`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- Adds a BuildSandbox/devOnly `ShapeAwareItemTrayGrid` receiver.
- Uses `ShapePlacementSession`, `ShapeItemPayload`, and `ShapeGridReceiver` as the placement authority.
- Covers 1/2/3/4-cell tray shapes, row-major packing, bounds checks, overlap checks, and stable tray anchors.
- Does not redraw the mature BattlePrepare item tray, replace formal UI, touch formal scenes, or continue the Fix03 overlay route.

## Grid Samples

| Sample | Action | Item | Shape | Cells | Anchor | Occupied Cells | Occupied Slots | Expected Valid | Actual Valid | Reason | State | Mutated | Stable Anchor | Note |
| --- | --- | --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `pack_single1` | `Pack` | `item_single1` | `Single1` | 1 | `0,0` | `0,0` | `0` | `True` | `True` | `None` | `Committed` | `True` | `True` | Row-major packing commits the item through ShapeGridReceiver.Commit. |
| `pack_vertical2` | `Pack` | `item_vertical2` | `Vertical2` | 2 | `1,0` | `1,0;1,1` | `1;6` | `True` | `True` | `None` | `Committed` | `True` | `True` | Row-major packing commits the item through ShapeGridReceiver.Commit. |
| `pack_corner3` | `Pack` | `item_corner3` | `Corner3` | 3 | `2,0` | `2,0;3,0;2,1` | `2;3;7` | `True` | `True` | `None` | `Committed` | `True` | `True` | Row-major packing commits the item through ShapeGridReceiver.Commit. |
| `pack_square4` | `Pack` | `item_square4` | `Square4` | 4 | `3,1` | `3,1;4,1;3,2;4,2` | `8;9;13;14` | `True` | `True` | `None` | `Committed` | `True` | `True` | Row-major packing commits the item through ShapeGridReceiver.Commit. |
| `bounds_rejects_square4` | `CanPlace` | `bounds_square4` | `Square4` | 4 | `4,7` | `4,7;5,7;4,8;5,8` | `39;40;44;45` | `False` | `False` | `OutOfGrid` | `HoldingItem` | `False` | `True` | A 2x2 shape anchored at the last slot exceeds the 5x8 tray bounds. |
| `overlap_rejects_corner3` | `CanPlace` | `overlap_corner3` | `Corner3` | 3 | `0,0` | `0,0;1,0;0,1` | `0;1;5` | `False` | `False` | `CellOccupied` | `HoldingItem` | `False` | `True` | A new item cannot overlap already committed tray cells. |
| `illegal_commit_keeps_stable_anchor` | `Preview+Commit` | `item_square4` | `Square4` | 4 | `4,7` | `4,7;5,7;4,8;5,8` | `39;40;44;45` | `False` | `False` | `OutOfGrid` | `InvalidPreview` | `False` | `True` | Illegal tray drops do not mutate the committed placement or stable tray anchor. |
| `legal_move_updates_stable_anchor` | `Preview+Commit` | `item_square4` | `Square4` | 4 | `0,2` | `0,2;1,2;0,3;1,3` | `10;11;15;16` | `True` | `True` | `None` | `Committed` | `True` | `True` | Legal tray moves update the receiver-owned stable tray anchor. |
| `rotation_normalizes_corner3` | `CanPlace` | `rotated_corner3` | `Corner3` | 3 | `0,0` | `0,0;0,1;1,1` | `0;5;6` | `True` | `True` | `None` | `HoldingItem` | `False` | `True` | Rotated offsets are normalized so the tray anchor remains the top-left stable cell. |
| `screen_point_to_cell` | `ScreenPointToCell` | `` | `` | 0 | `2,3` | `` | `` | `True` | `True` | `None` | `Idle` | `False` | `True` | Screen point maps to a tray cell without reading or writing formal UI layout. |

## State Authority

- The grid owns tray occupancy and committed tray anchors.
- Preview is side-effect-light; commit is the only path that mutates tray placement.
- Illegal drops keep the previous committed anchor, so the item does not snap to an unrelated initial slot.
- Rotated offsets are normalized inside the tray receiver to keep the anchor stable.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Shape-Aware ItemTray Grid | `PASS` | 0 | 0 | 39 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `SHAPE_ITEM_TRAY_GRID_DEVONLY` | Grid remains BuildSandbox/devOnly. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_ENABLED_TRUE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_FEATURE_FLAG_TRUE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_FIX03_CONTINUED` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_IGNORE_LAYOUT` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_DELAYED_DROP_READ` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_REDRAWS_ITEM_TRAY` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_REPLACES_MATURE_TRAY` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_WRITES_FORMAL_SCENE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_WRITES_FORMAL_UI` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_RUNFLOW` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_PAGESTATE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_FORMATIONSTATE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_SAVE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_BOSS_REWARD_NUMERIC` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_single1. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_vertical2. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_corner3. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_square4. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: bounds_rejects_square4. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: overlap_rejects_corner3. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: illegal_commit_keeps_stable_anchor. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: legal_move_updates_stable_anchor. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: rotation_normalizes_corner3. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: screen_point_to_cell. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_single1 action=Pack, valid=True, anchor=0,0, cells=0,0, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_vertical2 action=Pack, valid=True, anchor=1,0, cells=1,0;1,1, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_corner3 action=Pack, valid=True, anchor=2,0, cells=2,0;3,0;2,1, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_square4 action=Pack, valid=True, anchor=3,1, cells=3,1;4,1;3,2;4,2, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | bounds_rejects_square4 action=CanPlace, valid=False, anchor=4,7, cells=4,7;5,7;4,8;5,8, reason=OutOfGrid. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | overlap_rejects_corner3 action=CanPlace, valid=False, anchor=0,0, cells=0,0;1,0;0,1, reason=CellOccupied. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | illegal_commit_keeps_stable_anchor action=Preview+Commit, valid=False, anchor=4,7, cells=4,7;5,7;4,8;5,8, reason=OutOfGrid. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | legal_move_updates_stable_anchor action=Preview+Commit, valid=True, anchor=0,2, cells=0,2;1,2;0,3;1,3, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | rotation_normalizes_corner3 action=CanPlace, valid=True, anchor=0,0, cells=0,0;0,1;1,1, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | screen_point_to_cell action=ScreenPointToCell, valid=True, anchor=2,3, cells=, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Single1 packs as 1 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Vertical2 packs as 2 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Corner3 packs as 3 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Square4 packs as 4 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
