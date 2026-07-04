# BattleSandbox Runtime Loop Leak Check Report

Package: `V0.4-BattleSandboxRuntimeLoop01`
Generated: `2026-07-04 16:57:29`
Status: `PASS`
Errors: `0`
Warnings: `0`

## Leak Counters

| Check | Count | Expected |
| --- | ---: | ---: |
| `featureFlagDefaultTrue` | 0 | 0 |
| `formalFlowLeaks` | 0 | 0 |
| `settlementLeaks` | 0 | 0 |
| `playerSideAnswerLeaks` | 0 | 0 |
| `uiLayoutWrites` | 0 | 0 |
| `runFlowWrites` | 0 | 0 |
| `saveWrites` | 0 | 0 |
| `rewardGrants` | 0 | 0 |
| `chapterAdvances` | 0 | 0 |
| `totalLeaks` | 0 | 0 |

## Scope Confirmation

- Runtime loop clamps HP above zero and emits a no-settlement row instead of victory/defeat.
- Runtime loop updates existing text/fill/floating surfaces only.
- Runtime loop does not call formal combat settlement, saves, rewards, chapter, or RunFlow APIs.
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
