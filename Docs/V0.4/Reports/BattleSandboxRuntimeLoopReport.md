# BattleSandbox Runtime Loop Report

Package: `V0.4-BattleSandboxRuntimeLoop01`
Generated: `2026-07-04 16:57:29`
Status: `PASS`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- Runs only inside `Scene_TalismanBag_V04_BattleSandboxPreview` runtime surfaces.
- Reads `CombatKernelAdapter`, current V04 BuildSandbox board snapshots, BuildCombatPreview, and BuildSandbox ItemStat.
- Refreshes existing HUD text/fill, Boss cast bar text/fill, combat log, and runtime floating text.
- Does not run victory/defeat settlement and does not connect RunFlow, SaveData, Reward, or Chapter.
- Does not author or reorder V04 UI layout.

## Counters

- rows: `55`
- mana rows: `8`
- cooldown rows: `8`
- item trigger rows: `8`
- enemy HP rows: `8`
- enemy shield update rows: `6`
- player HP rows: `4`
- player shield rows: `9`
- Boss cast rows: `8`
- combat log rows: `55`
- floating text rows: `55`
- no-settlement rows: `1`
- source CombatKernelAdapter rows: `9`
- source BuildCombatPreview rows: `85`
- source ItemStat profiles: `8`
- formal leak count: `0`
- settlement leak count: `0`
- UI layout write count: `0`

## Runtime Loop Rows

| Row | Kind | Item | Mana | Enemy | Player | Cast | Floating | Source |
| --- | --- | --- | ---: | --- | --- | ---: | --- | --- |
| `runtimeLoop.opening.00` | `combatLog` | `` | 31 | 168/168 shield 24 | 100/100 shield 28 | 2.4s | 战斗预览开始 | `BattleSandboxRuntimeLoopPreview.rows` |
| `runtimeLoop.mana.00` | `mana` | `` | 52 | 168/168 shield 24 | 100/100 shield 28 | 2.4s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.00` | `cooldown` | `preview_taomu_sword` | 52 | 168/168 shield 24 | 100/100 shield 28 | 2.4s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.00` | `itemTrigger` | `preview_taomu_sword` | 43 | 168/168 shield 24 | 100/100 shield 28 | 2.4s | 触发 sword | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.00` | `enemyHp` | `preview_taomu_sword` | 43 | 164/168 shield 0 | 100/100 shield 28 | 2.4s | -4 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.enemyShield.00` | `enemyShield` | `preview_taomu_sword` | 43 | 164/168 shield 10 | 100/100 shield 28 | 2.4s | 敌盾 +10 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield.enemyShieldSample` |
| `runtimeLoop.playerShield.00` | `playerShield` | `preview_taomu_sword` | 43 | 164/168 shield 10 | 100/100 shield 38 | 2.4s | 护盾 +10 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield` |
| `runtimeLoop.bossCast.00` | `bossCast` | `` | 43 | 164/168 shield 10 | 100/100 shield 38 | 1.7s | 蓄力中 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.mana.01` | `mana` | `` | 64 | 164/168 shield 10 | 100/100 shield 38 | 1.7s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.01` | `cooldown` | `preview_fire_talisman` | 64 | 164/168 shield 10 | 100/100 shield 38 | 1.7s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.01` | `itemTrigger` | `preview_fire_talisman` | 58 | 164/168 shield 10 | 100/100 shield 38 | 1.7s | 触发 talisman | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.01` | `enemyHp` | `preview_fire_talisman` | 58 | 155/168 shield 0 | 100/100 shield 38 | 1.7s | -9 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.bossCast.01` | `bossCast` | `` | 58 | 155/168 shield 0 | 100/100 shield 38 | 0.9s | 蓄力中 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.mana.02` | `mana` | `` | 79 | 155/168 shield 0 | 100/100 shield 38 | 0.9s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.02` | `cooldown` | `preview_thunder_sword` | 79 | 155/168 shield 0 | 100/100 shield 38 | 0.9s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.02` | `itemTrigger` | `preview_thunder_sword` | 71 | 155/168 shield 0 | 100/100 shield 38 | 0.9s | 触发 sword | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.02` | `enemyHp` | `preview_thunder_sword` | 71 | 129/168 shield 0 | 100/100 shield 38 | 0.9s | -26 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.playerShield.02` | `playerShield` | `preview_thunder_sword` | 71 | 129/168 shield 0 | 100/100 shield 40 | 0.9s | 护盾 +2 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield` |
| `runtimeLoop.bossCast.02` | `bossCast` | `` | 71 | 129/168 shield 0 | 100/100 shield 40 | 0.2s | 蓄力中 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.mana.03` | `mana` | `` | 92 | 129/168 shield 0 | 100/100 shield 40 | 0.2s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.03` | `cooldown` | `preview_x2_wood_talisman` | 92 | 129/168 shield 0 | 100/100 shield 40 | 0.2s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.03` | `itemTrigger` | `preview_x2_wood_talisman` | 88 | 129/168 shield 0 | 100/100 shield 40 | 0.2s | 触发 talisman | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.03` | `enemyHp` | `preview_x2_wood_talisman` | 88 | 122/168 shield 0 | 100/100 shield 40 | 0.2s | -7 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.playerShield.03` | `playerShield` | `preview_x2_wood_talisman` | 88 | 122/168 shield 0 | 100/100 shield 50 | 0.2s | 护盾 +10 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield` |
| `runtimeLoop.bossCast.03` | `bossCast` | `` | 88 | 122/168 shield 0 | 100/100 shield 50 | 0.0s | 施法释放 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.playerHp.03` | `playerHp` | `` | 88 | 122/168 shield 0 | 88/100 shield 0 | 2.4s | -12 | `BattleSandboxCombatKernelAdapterBuilder.BuildEnemyHpSample.attackDamage` |
| `runtimeLoop.mana.04` | `mana` | `` | 109 | 122/168 shield 0 | 88/100 shield 0 | 2.4s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.04` | `cooldown` | `preview_guard_wood` | 109 | 122/168 shield 0 | 88/100 shield 0 | 2.4s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.04` | `itemTrigger` | `preview_guard_wood` | 105 | 122/168 shield 0 | 88/100 shield 0 | 2.4s | 触发 wood | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.04` | `enemyHp` | `preview_guard_wood` | 105 | 115/168 shield 0 | 88/100 shield 0 | 2.4s | -7 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.enemyShield.04` | `enemyShield` | `preview_guard_wood` | 105 | 115/168 shield 7 | 88/100 shield 0 | 2.4s | 敌盾 +7 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield.enemyShieldSample` |
| `runtimeLoop.playerShield.04` | `playerShield` | `preview_guard_wood` | 105 | 115/168 shield 7 | 88/100 shield 12 | 2.4s | 护盾 +12 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield` |
| `runtimeLoop.bossCast.04` | `bossCast` | `` | 105 | 115/168 shield 7 | 88/100 shield 12 | 1.7s | 蓄力中 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.mana.05` | `mana` | `` | 126 | 115/168 shield 7 | 88/100 shield 12 | 1.7s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.05` | `cooldown` | `preview_cleanse_corner` | 126 | 115/168 shield 7 | 88/100 shield 12 | 1.7s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.05` | `itemTrigger` | `preview_cleanse_corner` | 121 | 115/168 shield 7 | 88/100 shield 12 | 1.7s | 触发 corner | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.05` | `enemyHp` | `preview_cleanse_corner` | 121 | 115/168 shield 1 | 88/100 shield 12 | 1.7s | -0 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.playerShield.05` | `playerShield` | `preview_cleanse_corner` | 121 | 115/168 shield 1 | 88/100 shield 18 | 1.7s | 护盾 +6 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield` |
| `runtimeLoop.playerHp.05` | `playerHp` | `preview_cleanse_corner` | 121 | 115/168 shield 1 | 94/100 shield 18 | 1.7s | 气血 +6 | `BattleSandboxCombatKernelAdapterBuilder.ApplyHealing` |
| `runtimeLoop.bossCast.05` | `bossCast` | `` | 121 | 115/168 shield 1 | 94/100 shield 18 | 0.9s | 蓄力中 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.mana.06` | `mana` | `` | 142 | 115/168 shield 1 | 94/100 shield 18 | 0.9s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.06` | `cooldown` | `preview_stone_core` | 142 | 115/168 shield 1 | 94/100 shield 18 | 0.9s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.06` | `itemTrigger` | `preview_stone_core` | 135 | 115/168 shield 1 | 94/100 shield 18 | 0.9s | 触发 core | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.06` | `enemyHp` | `preview_stone_core` | 135 | 99/168 shield 0 | 94/100 shield 18 | 0.9s | -16 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.playerShield.06` | `playerShield` | `preview_stone_core` | 135 | 99/168 shield 0 | 94/100 shield 30 | 0.9s | 护盾 +12 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield` |
| `runtimeLoop.bossCast.06` | `bossCast` | `` | 135 | 99/168 shield 0 | 94/100 shield 30 | 0.2s | 蓄力中 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.mana.07` | `mana` | `` | 156 | 99/168 shield 0 | 94/100 shield 30 | 0.2s | 灵气 +21 | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaGainPerTick` |
| `runtimeLoop.cooldown.07` | `cooldown` | `preview_taomu_sword` | 156 | 99/168 shield 0 | 94/100 shield 30 | 0.2s | 冷却刷新 | `BattleSandboxCombatKernelAdapterBuilder.ResolveEffectiveCooldown` |
| `runtimeLoop.itemTrigger.07` | `itemTrigger` | `preview_taomu_sword` | 147 | 99/168 shield 0 | 94/100 shield 30 | 0.2s | 触发 sword | `BuildSandboxLayoutSnapshot.placedItems[].itemStat.manaCostPerCast` |
| `runtimeLoop.enemyHp.07` | `enemyHp` | `preview_taomu_sword` | 147 | 71/168 shield 0 | 94/100 shield 30 | 0.2s | -28 | `BattleSandboxCombatKernelAdapterBuilder.ApplyEnemyDamage` |
| `runtimeLoop.playerShield.07` | `playerShield` | `preview_taomu_sword` | 147 | 71/168 shield 0 | 94/100 shield 40 | 0.2s | 护盾 +10 | `BattleSandboxCombatKernelAdapterBuilder.ApplyPlayerShield` |
| `runtimeLoop.playerHp.07` | `playerHp` | `preview_taomu_sword` | 147 | 71/168 shield 0 | 96/100 shield 40 | 0.2s | 气血 +2 | `BattleSandboxCombatKernelAdapterBuilder.ApplyHealing` |
| `runtimeLoop.bossCast.07` | `bossCast` | `` | 147 | 71/168 shield 0 | 96/100 shield 40 | 0.0s | 施法释放 | `BattleSandboxCombatKernelAdapterBuilder.BuildCastBarSample` |
| `runtimeLoop.playerHp.07` | `playerHp` | `` | 147 | 71/168 shield 0 | 84/100 shield 0 | 2.4s | -12 | `BattleSandboxCombatKernelAdapterBuilder.BuildEnemyHpSample.attackDamage` |
| `runtimeLoop.noSettlement.08` | `noSettlement` | `` | 147 | 71/168 shield 0 | 84/100 shield 0 | 2.4s | 无结算 | `Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md` |

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| UI Layout Guard | `PASS` | 0 | 0 | 29 |
| BattleSandbox Combat Kernel Adapter 01 | `PASS` | 0 | 0 | 52 |
| BattleSandbox Build Combat Preview 01 | `PASS` | 0 | 0 | 34 |
| BattleSandbox ManaLoop Runtime 01 | `PASS` | 0 | 0 | 1 |
| BattleSandbox Runtime Loop 01 | `PASS` | 0 | 0 | 40 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs` |
| `Info` | `FORMAL_UI_SOURCE_REFERENCE_ONLY_ALLOWED` | This devOnly runtime playtest references mature formal UI sources without writing formal scene assets. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxEnemyCombatFeedbackController.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxEnemyEncounterPreviewController.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxManaLoopRuntime.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxRuntimeLoop.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildPlacementFeedbackView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/MobileShapePlacementRuntimeIntegration.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayFixtureView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayGridReservationView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxCombatInfoHudSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyCombatFeedbackUiReuseSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyCombatFeedbackUiReuseValidator.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyEncounterPreviewSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyEncounterPreviewValidator.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneBuilder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneReportWriter.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneVerifier.cs` |
| `Info` | `FORMAL_UI_SOURCE_REFERENCE_ONLY_ALLOWED` | This devOnly runtime playtest references mature formal UI sources without writing formal scene assets. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxShapePlacementVerticalSliceReportWriter.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildGridInteractionPreviewReportWriter.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildGridInteractionPreviewSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildGridInteractionPreviewValidator.cs` |
| `Info` | `UI_LAYOUT_GUARD_PASS` | BuildSandbox code scan found no scene access, formal UI creation, or layout write tokens. | `` |
| `Info` | `COMBAT_KERNEL_DEVONLY_TRUE` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ENABLED_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ADAPTER_ONLY` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_RULE_MOUTHFEEL` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_SPIRIT` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_COOLDOWN` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_DAMAGE` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_SHIELD` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_HEALING` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_ENEMY_HP` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REUSES_CAST_BAR` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_RUNFLOW_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_READS_SAVE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_WRITES_SAVE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_REWARD_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_CHAPTER_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_UI_REWRITE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_STABLE_RUNTIME_WRITE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_FORMAL_DAMAGE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_FEATURE_FLAG_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_FORMAL_LEAK_ZERO` | formal flow leak count pass. actual=0. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_UI_LEAK_ZERO` | UI layout leak count pass. actual=0. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_RULE_SCOPE_LEAK_ZERO` | rule scope leak count pass. actual=0. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_DEVONLY_ISOLATION_PASS` | Required flag remains true. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ROW_COUNT` | adapter rule row count pass. actual=9, expected>=9. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_POWER_CROSS_FULL` | eye cross state pass. actual=1. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_POWER_DIAGONAL_WEAK` | eye diagonal state pass. actual=2. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_POWER_SPIRIT_FULL` | spirit nine-grid state pass. actual=1. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_POWER_WEAK_MULTIPLIER` | weak power cooldown multiplier pass. actual=1.35. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_COOLDOWN_MIN` | minimum cooldown pass. actual=0.1. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_COOLDOWN_FIRE` | fire adjacent spirit cooldown pass. actual=1.6. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_COOLDOWN_WEAK` | weak fire cooldown pass. actual=2.16. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_COOLDOWN_TRAINED_WEAK` | trained weak fire cooldown pass. actual=1.944. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_DAMAGE_BLOCKED` | shield blocked damage pass. actual=8. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_DAMAGE_HP` | HP damage pass. actual=12. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_DAMAGE_SHIELD_AFTER` | enemy shield after damage pass. actual=0. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_DAMAGE_HP_AFTER` | enemy HP after damage pass. actual=108. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_PLAYER_SHIELD_CAP` | player shield after cap pass. actual=50. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_PLAYER_SHIELD_WASTE` | wasted player shield pass. actual=13. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ENEMY_SHIELD_ADD` | enemy shield additive result pass. actual=12. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_HEAL_CLAMP` | player HP after heal pass. actual=100. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_HEAL_OVERHEAL` | overheal pass. actual=10. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ENEMY_SPAWN_HP` | enemy spawn HP pass. actual=120. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ENEMY_SPAWN_SHIELD` | enemy spawn shield pass. actual=0. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ENEMY_ATTACK_INTERVAL` | enemy attack interval pass. actual=2. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ENEMY_ENRAGE_THRESHOLD` | boss enrage HP threshold pass. actual=60. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_CAST_REMAINING` | cast remaining after tick pass. actual=0.75. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_CAST_BAR_RATIO` | cast bar remaining ratio pass. actual=0.6. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_CAST_COOLDOWN` | cooldown after complete pass. actual=4. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_CAST_ZERO_COOLDOWN_CLAMP` | zero cooldown clamp pass. actual=0.1. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_PLAYER_DOT` | player DOT damage per second pass. actual=5. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `COMBAT_KERNEL_ENEMY_BURN_DOT` | enemy burn damage per second pass. actual=3. | `V0.4-BattleSandboxCombatKernelAdapter01` |
| `Info` | `BUILD_COMBAT_DEVONLY_TRUE` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_ENABLED_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_READS_BOARD` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_READS_DEVONLY_PROBLEM` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_CALCULATES_SYNERGY` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_CALCULATES_MODIFIER` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_CALCULATES_READINESS` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_CALCULATES_SHAPE_RULES` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_CALCULATES_ITEM_STATS` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_WRITES_BOSS_STATE` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_WRITES_CAST_BAR` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_WRITES_FLOATING` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_WRITES_LOG` | Required flag remains true. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_RUNS_FORMAL_COMBAT` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_CALLS_FORMAL_DAMAGE` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_WRITES_FORMAL_FLOW` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_WRITES_SAVE` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_GRANTS_REWARD` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_ADVANCES_CHAPTER` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_OPENS_FEATURE_FLAG` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_SHOWS_FULL_ANSWERS` | Isolation flag remains false. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_SCENARIO_COUNT` | Preview scenario count pass. actual=1, expected>=1. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_PLACED_ITEM_COUNT` | Placed item snapshot count pass. actual=8, expected>=1. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_SYNERGY_COUNT` | Active synergy count pass. actual=3, expected>=1. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_MODIFIER_COUNT` | Modifier preview count pass. actual=28, expected>=1. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_EVENT_COUNT` | Effect event preview count pass. actual=37, expected>=1. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_SHAPE_RULE_DEFINITION_COUNT` | Shape build rule definition count pass. actual=4, expected>=4. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_SHAPE_RULE_MATCH_COUNT` | Shape build rule match count pass. actual=4, expected>=3. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_SHAPE_RULE_FEEDBACK_COUNT` | Shape build rule feedback row count pass. actual=4, expected>=3. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_ITEM_STAT_PROFILE_COUNT` | ItemStat profile count pass. actual=8, expected>=1. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_ITEM_STAT_FEEDBACK_COUNT` | ItemStat combat feedback row count pass. actual=4, expected>=3. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_BOSS_READINESS_COUNT` | Boss readiness row count pass. actual=6, expected>=1. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_ROW_COUNT` | Combat preview feedback row count pass. actual=85, expected>=4. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `BUILD_COMBAT_SHAPE_RULE_ROWS` | Shape build rule player feedback row count pass. actual=4, expected>=3. | `V0.4-BattleSandboxBuildCombatPreview01` |
| `Info` | `MANA_LOOP_ROWS_READY` | Mana loop rows present: 16; gain=8, spend=8. | `BattleSandboxManaLoopPreviewRow` |
| `Info` | `RUNTIME_LOOP_DEVONLY_TRUE` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ENABLED_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_READS_KERNEL_ADAPTER` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_READS_ITEM_STAT` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_READS_CURRENT_BOARD` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_READS_BUILD_PREVIEW` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_WRITES_HUD_TEXT` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_WRITES_FLOATING_TEXT` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FORMAL_COMBAT_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FORMAL_DAMAGE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FLOW_WRITE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SAVE_WRITE_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_REWARD_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_CHAPTER_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FEATURE_FLAG_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_UI_LAYOUT_TOUCH_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_VICTORY_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_DEFEAT_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FEATURE_FLAGS_ZERO` | feature flag default true count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FORMAL_LEAK_ZERO` | formal leak count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SETTLEMENT_ZERO` | victory/defeat settlement count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_PLAYER_LEAK_ZERO` | player-side answer leak count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_UI_LAYOUT_WRITE_ZERO` | UI layout write count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_DEVONLY_ISOLATION_PASS` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ROW_COUNT` | runtime loop row count pass. actual=55, expected>=12. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_MANA_ROWS` | mana row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_COOLDOWN_ROWS` | cooldown row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_TRIGGER_ROWS` | item trigger row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ENEMY_HP_ROWS` | enemy HP row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ENEMY_SHIELD_ROWS` | enemy shield update row count pass. actual=6, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_PLAYER_HP_ROWS` | player HP row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_PLAYER_SHIELD_ROWS` | player shield row count pass. actual=9, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_BOSS_CAST_ROWS` | Boss cast-bar row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_LOG_ROWS` | combat log row count pass. actual=55, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FLOATING_ROWS` | floating text row count pass. actual=55, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_NO_SETTLEMENT_ROWS` | no-settlement confirmation row count pass. actual=1, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ROWS_READY` | Rows=55, mana=8, trigger=8, bossCast=8. | `BattleSandboxRuntimeLoopPreview` |
| `Info` | `RUNTIME_LOOP_SOURCE_KERNEL_ROWS` | CombatKernelAdapter source row count pass. actual=9, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SOURCE_BUILD_PREVIEW_ROWS` | BuildCombatPreview source row count pass. actual=85, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SOURCE_ITEM_STAT_ROWS` | ItemStat source profile count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
