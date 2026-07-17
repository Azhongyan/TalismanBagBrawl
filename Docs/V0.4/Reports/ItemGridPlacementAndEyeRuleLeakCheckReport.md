# ItemGridPlacementAndEyeRule01 Leak Check Report

- Result: PASS
- Scene path: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`
- BuildSettings: checked readonly; Item Sandbox scene remains manual-only and unregistered.
- Scope: Item Sandbox placement preview and eye cover validation only.
- Not implemented: real lighting, relay, Build calculation, awakening, drop/acquire, upgrade, save, battle bridge, or battle settlement.
- Catalog boundary: eye stone is not a catalog item; placement preview consumes existing 31 catalog item shape fields only.
- Array cells: identity display only; no array bonus calculation is executed.

## Passed Checks / Notes
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

