# ArrayBonusCellResolver01 Leak Check Report

- Result: PASS
- Scope: independent Item Sandbox array bonus state resolver and detail projection.
- BuildSettings: no writes; Item Sandbox remains manual-only.
- Forbidden integrations: formal battle resolver, bridge, run flow, save data, rewards, drops, boss data, and BuildSettings writers.
- Not implemented: numeric bonuses, attack/defense/cooldown multipliers, combat settlement, snapshot serialization, or formal Build activation.
- Known unrelated dirty file intentionally untouched: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.

## Passed Checks / Notes
- Rule constants checked: boardSize=5, eyeCell=(2,2), AP01/AP02/AP03/AP04 are fixed and exclude the eye.
- Placement rules checked: AP cells are legal, eyeCell remains blocked, multi-cell item can occupy two AP cells without covering the eye.
- Array samples checked: single-cell, unlit occupied, off-array, and legal multi-cell AP occupation all resolve as expected.
- placementId identity checked: two I001 placements resolve independently in lighting and array snapshots.
- No-light rule checked: occupying AP01 does not directly light, relay, or activate by itself.
- ItemDetailViewModel projection checked: placementId, AP occupancy, and active flags are visible in detail state.
- Source scope check completed: Item Sandbox array bonus files do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.

## Errors
- None

