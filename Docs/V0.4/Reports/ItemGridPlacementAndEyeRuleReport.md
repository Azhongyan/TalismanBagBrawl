# ItemGridPlacementAndEyeRule01 Report

- Package: `TASK_START_ITEMGRIDPLACEMENTANDEYERULE01`
- Guard receipt target: `GUARD_PASS_ITEMGRIDPLACEMENTANDEYERULE01`
- Verification: PASS
- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`

## Implemented
- Added an independent 5x5 placement preview panel to the Item Sandbox scene.
- Fixed `eyeCell=(2,2)` as a non-item, non-movable, non-coverable center cell.
- Fixed `arrayBonusCells=(2,1);(2,3);(1,2);(3,2)` as identity-only cells that can be occupied.
- Visualizes selected catalog item `shapeCells`, computed `occupiedCells`, `coreCellLocal`, and `coreCellWorld`.
- Reports invalid preview reasons for eye cover, out-of-grid, and overlap with demo occupiedCells.
- Existing item selection still refreshes `ItemSandboxDetailPanelView` through `ItemDetailViewModel`.

## Notes
- Rule constants passed: 5x5 board, eyeCell=(2,2), four fixed arrayBonusCells.
- Catalog input check passed: 31 items expose shapeCells and coreCellLocal; eye stone is not a catalog item.
- Placement rule samples passed: array occupancy allowed, eye cover blocked, out-of-grid blocked, overlap blocked, core visualized.
- BuildSettings check passed: Item Sandbox scene is not registered.
- Scene grid check passed: 25 clickable cells cover the full 5x5 board.
- Selection wiring check passed: item selection refreshes both detail panel and placement preview.
- Hierarchy naming check passed: no Chinese GameObject names were added.
- Source scope check completed for Item Sandbox placement preview files.

## Errors
- None

