# Shape-Aware Tray Packing Drag Report

Package: `V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix03-ShapeAwareTrayPackingDrag`
Generated: `2026-07-01 18:40:49`
Status: `READY_SHAPE_AWARE_TRAY_PACKING_DRAG_PLAYTEST`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Runtime Entry

- Manual Runtime Menu: `Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[Manual Only] Open Runtime Playtest`
- Fix03 QA Menu: `Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareRuntimePlaytestFixtureFeedbackFix03-ShapeAwareTrayPackingDrag/[QA Only] Run Shape-Aware Tray Packing Drag Validation`
- Extension: `ItemTrayShapeExtension`
- Tray Grid: `5 columns x 8 rows / 40 cells`
- Board Commit: `DEFERRED_TO_MOBILE_SHAPE_PLACEMENT_INTERACTION01`

## Packing And Drag Rows

| Shape | Initial Anchor | Initial Occupied Cells | Move Probe Anchor | Move Probe Cells | In Bounds | Overlap | Drag Persists | Illegal Drop Returns | Board Preview Safe | Redraw Tray | Save/Formal Writes |
| --- | ---: | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- |
| `Single1` | 0 | `0` | 4 | `4` | `True` | `False` | `True` | `True` | `True` | `False` | `False` |
| `Vertical2` | 1 | `1;6` | 10 | `10;15` | `True` | `False` | `True` | `True` | `True` | `False` | `False` |
| `Corner3` | 2 | `2;3;7` | 15 | `15;16;20` | `True` | `False` | `True` | `True` | `True` | `False` | `False` |
| `Square4` | 8 | `8;9;13;14` | 20 | `20;21;25;26` | `True` | `False` | `True` | `True` | `True` | `False` | `False` |

## Manual Handtest Checklist

- The mature BattlePrepare item tray is still the V03 tray; no replacement tray is created.
- Single1, Vertical2, Corner3, and Square4 initially appear inside the tray bounds without overlapping.
- Dragging a multi-cell fixture moves the full footprint, not a one-cell card.
- Dropping a fixture on a legal tray position persists after the runtime fixture refresh interval.
- Dropping on an illegal tray position returns to the last legal tray anchor.
- Dragging over the board keeps the same footprint preview and does not snap back to the wrong initial tray location.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Shape-Aware Tray Packing Drag Fix 03 | `PASS` | 0 | 0 | 9 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_ISOLATION_PASS` | Fix03 remains an ItemTrayShapeExtension/devOnly fixture patch and does not redraw the mature BattlePrepare tray. | `BattlePrepareComponentAdapterRuntimePlaytestPlan` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_INITIAL_PASS` | Initial packing pass. shape=Single1, anchor=0, occupied=0. | `BattlePrepareShapeAwareItemTrayRow` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_INITIAL_PASS` | Initial packing pass. shape=Vertical2, anchor=1, occupied=1;6. | `BattlePrepareShapeAwareItemTrayRow` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_INITIAL_PASS` | Initial packing pass. shape=Corner3, anchor=2, occupied=2;3;7. | `BattlePrepareShapeAwareItemTrayRow` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_INITIAL_PASS` | Initial packing pass. shape=Square4, anchor=8, occupied=8;9;13;14. | `BattlePrepareShapeAwareItemTrayRow` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_DRAG_PASS` | Drag persistence probe pass. shape=Single1, moveAnchor=4, occupied=4. | `BattlePrepareShapeAwareItemTrayRow` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_DRAG_PASS` | Drag persistence probe pass. shape=Vertical2, moveAnchor=10, occupied=10;15. | `BattlePrepareShapeAwareItemTrayRow` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_DRAG_PASS` | Drag persistence probe pass. shape=Corner3, moveAnchor=15, occupied=15;16;20. | `BattlePrepareShapeAwareItemTrayRow` |
| `Info` | `SHAPE_AWARE_TRAY_PACKING_DRAG_PASS` | Drag persistence probe pass. shape=Square4, moveAnchor=20, occupied=20;21;25;26. | `BattlePrepareShapeAwareItemTrayRow` |
