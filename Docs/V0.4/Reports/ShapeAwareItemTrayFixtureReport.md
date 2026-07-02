# Shape-Aware ItemTray Fixture Report

Package: `V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix02-ShapeAwareItemTray`
Generated: `2026-07-01`
Status: `READY_FOR_UNITY_PLAYTEST_STATIC`
Unity Execution: `NOT_RUN_IN_SHELL`

## Summary

- Adds `ShapeAwareItemTrayFixtureView` as the runtime `ItemTrayShapeExtension` for devOnly fixture items.
- `Single1`, `Vertical2`, `Corner3`, and `Square4` now draw occupied shape cells in the mature BattlePrepare tray instead of presenting as one single icon card.
- The fixture view is an `ignoreLayout` overlay positioned from mature `ItemTrayGridSlot_##` cells inside the existing tray `Content`; it does not replace the mature tray, tabs, scroll view, drag component, or pull-up motion.
- Dragging moves the same multi-cell fixture GameObject, so the hand-held visual remains shape-aware.
- Board GhostPreview still uses the same `ItemShapeConfig.occupiedOffsets` / rotation path as the tray shape rows.

## Shape-Aware Fixture Rows

| Shape | Tray Footprint | Width | Height | Occupied Offsets | Tray True Shape | Selected Highlight | Drag Visual | Ghost Matches | Mature Tray | Redraw Tray | Formal Config | Save Data |
| --- | --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `Single1` | 1格 | 1 | 1 | `0,0` | `True` | `True` | `True` | `True` | `True` | `False` | `False` | `False` |
| `Vertical2` | 2格竖形 | 1 | 2 | `0,0;0,1` | `True` | `True` | `True` | `True` | `True` | `False` | `False` | `False` |
| `Corner3` | 2x2缺1格拐角 | 2 | 2 | `0,0;1,0;0,1` | `True` | `True` | `True` | `True` | `True` | `False` | `False` | `False` |
| `Square4` | 2x2方块 | 2 | 2 | `0,0;1,0;0,1;1,1` | `True` | `True` | `True` | `True` | `True` | `False` | `False` | `False` |

## Manual Handtest Checklist

1. Open the runtime playtest menu.
2. Confirm the mature BattlePrepare tray still appears; no replacement `V04NewItemTray` appears.
3. Confirm `Single1` appears as one occupied cell.
4. Confirm `Vertical2` appears as two vertical occupied cells in the tray.
5. Confirm `Corner3` appears as a 2x2 footprint missing one cell.
6. Confirm `Square4` appears as a 2x2 square.
7. Select each fixture and confirm highlight covers the occupied shape cells.
8. Drag each fixture and confirm the hand-held visual is the same multi-cell shape.
9. Hover over the board and confirm GhostPreview matches the tray footprint.

## Validation Note

Shell environment did not expose a callable Unity executable or `.sln/.csproj`; Unity menu execution and Play hand-feel verification were not run from Codex shell. This report is a static implementation report and should be superseded by the QA menu report after Unity Editor execution.
