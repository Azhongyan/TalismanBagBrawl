# JuNianLightingAndAdjacentRelay01 Leak Check Report

- Result: PASS
- Scene path: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`
- BuildSettings: checked readonly; Item Sandbox scene remains manual-only and unregistered.
- Scope: independent Item Sandbox lighting, adjacent relay, greybox visualization, and ItemDetailViewModel projection.
- Forbidden formal integrations: BattleResolver, BattleSnapshotAdapter, UnifiedBattlePage, Battle Bridge, RunFlow, SaveData, Reward, Boss, BuildSettings writers.
- Not implemented: Build count / activation, four auto skill monitor icons, array bonus settlement, core awakening unlock, affix/drop/upgrade/save, battle damage/heal/shield/status settlement, BattleContract formal wiring.
- Known unrelated dirty file intentionally untouched: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.

## Passed Checks / Notes
- Rule constants checked: 5x5 board, eyeCell=(2,2), four arrayBonusCells, direct range is orthogonal adjacent only.
- Catalog source boundary passed: I031 聚念石 is the only isLightingSource=true item.
- Scenario samples checked: direct core-cell lighting, multi-cell core-only rule, relay chain, diagonal/gap/eye blockers, stable tie source, no-source all-unlit, ordinary relay-only.
- ItemDetailViewModel projection passed: relay-lit status, litByItemId, litDepth, and basicEffectActive are displayed through ViewModel fields.
- BuildSettings check passed: Item Sandbox scene is not registered.
- Scene scenario switch check passed: 10 repeatable greybox lighting cases are configured.
- Hierarchy naming check passed: no Chinese GameObject names were added.
- Source scope check completed: Item Sandbox lighting files do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.

## Errors
- None

