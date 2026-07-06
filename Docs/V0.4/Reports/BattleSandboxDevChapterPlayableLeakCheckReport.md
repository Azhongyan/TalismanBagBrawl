# BattleSandbox Dev Chapter Playable Leak Check Report

Package: `V0.4-BattleSandboxDevChapterPlayable01`
Generated: `2026-07-04 23:26:34`
Status: `PASS`
Errors: `0`
Warnings: `0`

## Leak Counters

| Check | Count | Expected | Result |
| --- | ---: | ---: | --- |
| `playerSideAnswerLeaks` | 0 | 0 | `PASS` |
| `formalFlowOrDataLeaks` | 0 | 0 | `PASS` |
| `featureFlagDefaultTrue` | 0 | 0 | `PASS` |
| `runtimeUiLayoutWrites` | 0 | 0 | `PASS` |
| `selectorRectTransformWrites` | 0 | 0 | `PASS` |
| `formalRunFlowConnected` | 0 | 0 | `PASS` |
| `saveDataWrites` | 0 | 0 | `PASS` |
| `rewardGrants` | 0 | 0 | `PASS` |
| `chapterProgress` | 0 | 0 | `PASS` |
| `formalStageConfigTouch` | 0 | 0 | `PASS` |
| `formalBossTableTouch` | 0 | 0 | `PASS` |
| `formalRewardTableTouch` | 0 | 0 | `PASS` |
| `v02OrV03Touch` | 0 | 0 | `PASS` |
| `sceneBinderRuns` | 0 | 0 | `PASS` |
| `totalLeaks` | 0 | 0 | `PASS` |

## Masking Confirmation

- Complete answers are hidden.
- DropBias weights are hidden.
- Boss six-key answers are hidden.
- The player side only receives level label, enemy/Boss display name, mechanic feedback, Build pressure, and short sandbox result text.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| UI Layout Guard | `PASS` | 0 | 0 | 29 |
| BattleSandbox Combat Kernel Adapter 01 | `PASS` | 0 | 0 | 52 |
| BattleSandbox Build Combat Preview 01 | `PASS` | 0 | 0 | 34 |
| BattleSandbox ManaLoop Runtime 01 | `PASS` | 0 | 0 | 1 |
| BattleSandbox Runtime Loop 01 | `PASS` | 0 | 0 | 66 |
| BattleSandbox Playable Loop 01 | `PASS` | 0 | 0 | 24 |
| BattleSandbox Playable Full Roster Regression 01 | `PASS` | 0 | 0 | 14 |
| BattleSandbox Dev Chapter Playable 01 | `PASS` | 0 | 0 | 26 |

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
| `Info` | `RUNTIME_LOOP_RUNNING_STATE` | Required flag remains true. | `V0.4-BattleSandboxRuntimeLoop01` |
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
| `Info` | `RUNTIME_LOOP_ROW_COUNT` | runtime loop row count pass. actual=52, expected>=12. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_MANA_ROWS` | mana row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_COOLDOWN_ROWS` | cooldown row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_TRIGGER_ROWS` | item trigger row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ENEMY_HP_ROWS` | enemy HP row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ENEMY_SHIELD_ROWS` | enemy shield update row count pass. actual=3, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_PLAYER_HP_ROWS` | player HP row count pass. actual=6, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_PLAYER_SHIELD_ROWS` | player shield row count pass. actual=11, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_BOSS_CAST_ROWS` | Boss cast-bar row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ATTACK_ROWS` | enemy attack row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ATTACK_TIMER_ADVANCES` | enemy attack timer advance row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ATTACK_DAMAGE_PROFILE_ROWS` | devOnly profile attack damage row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SHIELD_FIRST_ROWS` | shield-first player damage row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_LOG_ROWS` | combat log row count pass. actual=52, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FLOATING_ROWS` | floating text row count pass. actual=52, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SANDBOX_RESULT_ROWS` | sandbox result row count pass. actual=1, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_NO_SETTLEMENT_ZERO` | legacy no-settlement row pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ROWS_READY` | Rows=52, mana=8, trigger=8, bossCast=8, attacks=4, sandboxResults=1. | `BattleSandboxRuntimeLoopPreview` |
| `Info` | `RUNTIME_LOOP_SOURCE_KERNEL_ROWS` | CombatKernelAdapter source row count pass. actual=9, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SOURCE_BUILD_PREVIEW_ROWS` | BuildCombatPreview source row count pass. actual=85, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SOURCE_ITEM_STAT_ROWS` | ItemStat source profile count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SOURCE_ATTACK_DAMAGE` | devOnly enemy/profile attack damage count pass. actual=55, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_RUNTIME_DEFAULT_INPUT_FALSE` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_DEV_ENEMY_SCENARIOS` | dev enemy scenario count pass. actual=4, expected>=2. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_DEV_ENEMY_310` | 3-10 dev enemy scenario count pass. actual=2, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_DEV_ENEMY_410` | 4-10 dev enemy scenario count pass. actual=2, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SANDBOX_VICTORY_COVERAGE` | sandbox victory scenario count pass. actual=2, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SANDBOX_DEFEAT_COVERAGE` | sandbox defeat scenario count pass. actual=2, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_PLACED_ITEMS_ZERO` | empty board placed item count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_NO_DEFAULT_LAYOUT` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_NO_FALLBACK` | Isolation flag remains false. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_ITEM_STAT_ZERO` | empty board item stat source count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_TRIGGER_ROWS_ZERO` | empty board item trigger row count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_COOLDOWN_ROWS_ZERO` | empty board item cooldown row count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_ENEMY_HP_ROWS_ZERO` | empty board enemy HP damage row count pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_PLAYER_ITEM_DAMAGE_ZERO` | empty board player item enemy HP damage pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_ENEMY_HP_UNCHANGED` | empty board final enemy HP pass. actual=132. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_PLAYER_HP_ROWS` | empty board player HP row count pass. actual=2, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_DEFEAT_RESULT` | empty board sandbox defeat row count pass. actual=1, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_VICTORY_ZERO` | empty board sandbox victory row pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_EMPTY_FINAL_PLAYER_HP_ZERO` | empty board final player HP pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `PLAYABLE_LOOP_DEVONLY_TRUE` | Playable loop remains devOnly. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_ENABLED_FALSE` | Playable loop remains disabled by default. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_REUSES_RUNTIME_LOOP` | Playable loop reuses BattleSandboxRuntimeLoop01. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_REWRITE_FALSE` | Playable loop does not rewrite the battle loop. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_RUNNING_STATE_VISIBLE` | Runtime loop exposes a running state after battle mode starts. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_ATTACK_TIMER_ADVANCES` | Enemy attack timer advances in runtime rows. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_ATTACK_DAMAGE_PROFILE` | Enemy attack damage comes from devOnly profile rows. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_SHIELD_BEFORE_HP` | Enemy attack consumes shield before HP. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_HP_ZERO_ALLOWED` | Playable loop allows player HP to reach zero. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_VICTORY_HP_ZERO` | Victory condition is enemy HP <= 0. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_DEFEAT_HP_ZERO` | Defeat condition is player HP <= 0. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_RESULT_PROMPTS` | Victory and defeat sandbox prompts are visible. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_BUILD_BRIEF` | Result prompt includes brief Build feedback. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_RESTART_RETAINS_BUILD` | Restart retains the current board Build snapshot. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_RESTART_RESETS_STATE` | Restart resets HP, shield, mana, cooldown, log, and cast surfaces. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_SWITCH_310_410` | Target switch covers devOnly 3-10 and 4-10 targets. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_FEATURE_FLAGS_ZERO` | feature flag default true count pass. actual=0. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_FORMAL_LEAK_ZERO` | formal flow leak count pass. actual=0. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_SETTLEMENT_LEAK_ZERO` | formal settlement leak count pass. actual=0. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_PLAYER_LEAK_ZERO` | player-side answer leak count pass. actual=0. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_UI_LAYOUT_WRITE_ZERO` | UI layout write count pass. actual=0. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_BUILDS_SETTINGS_FALSE` | Preview scene is not inserted into formal Build Settings. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_V02_V03_TOUCH_FALSE` | Playable loop does not touch V0.2/V0.3 surfaces. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `PLAYABLE_LOOP_FORMAL_UI_LAYOUT_FALSE` | Playable loop does not author formal UI layout. | `V0.4-BattleSandboxPlayableLoop01` |
| `Info` | `FULL_ROSTER_SCENE_READY` | V04 battle sandbox preview scene has required bindings. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_TRAY_23_READY` | All 23 roster items appear and pack into the V04 item tray. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_BASIC_1X1_READY` | Basic items default to Single1 and have legal board placements. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_MULTI_SHAPES_READY` | x2, x3, x4, and vertical_3 items have legal placements. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_EMPTY_BOARD_DEFEAT_READY` | Empty board keeps enemy HP unchanged, damages player, and ends in sandbox defeat. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_ATTACK_DAMAGE_READY` | Attack item sample reduces enemy HP. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_SHIELD_FIRST_READY` | Enemy attacks consume player shield before HP. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_SUPPORT_NO_DAMAGE_READY` | Support/heal/cleanse/control/aura/rhythm rows do not damage enemy HP. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_RESULT_READY` | Sandbox victory and defeat result rows are both present. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_RESTART_SWITCH_READY` | Restart and switch-target paths can produce another runtime preview. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_PLAYER_LEAK_CLEAR` | Player-side answer leak counters are zero. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_FORMAL_SCOPE_CLEAR` | No SaveData, Reward, Chapter, RunFlow, scene, or layout write scope is used. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_FLAGS_FALSE` | All BuildSandbox feature flags default false. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `FULL_ROSTER_DEVONLY_DISABLED` | Sandbox surfaces remain devOnly and disabled by default. | `V0.4-BattleSandboxPlayableFullRosterRegression01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_DEVONLY_TRUE` | Dev chapter playable remains devOnly. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_ENABLED_FALSE` | Dev chapter playable remains disabled by default. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_SCENE_EXISTS` | V04 battle sandbox preview scene exists. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_REUSE_RUNTIME` | Reuses BattleSandboxRuntimeLoop and does not rewrite battle loop. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_REUSE_PLAYABLE_LOOP` | Reuses the existing PlayableLoop result. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_REUSE_FULL_ROSTER` | Reuses the existing FullRoster result. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_310_SELECTABLE` | 3-10 devOnly level is selectable. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_410_SELECTABLE` | 4-10 devOnly level is selectable. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_SELECTOR_BOUND` | V04 selector is bound to runtime chapter selection. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_DISTINCT_PROFILE` | 3-10 and 4-10 use different devOnly boss profiles. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_DISTINCT_MECHANIC` | 3-10 and 4-10 show different mechanic feedback. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_DISTINCT_PRESSURE` | 3-10 and 4-10 show different Build pressure. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_RESULTS` | Victory and failure paths are available. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_RESTART_SWITCH` | Restart and chapter switch paths are available. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_PLAYER_LEAK_ZERO` | player-side answer leak count pass. actual=0. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_FORMAL_LEAK_ZERO` | formal flow leak count pass. actual=0. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_FEATURE_FLAG_ZERO` | feature flag default true count pass. actual=0. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_UI_LAYOUT_ZERO` | UI layout write count pass. actual=0. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_SELECTOR_LAYOUT_ZERO` | selector layout write count pass. actual=0. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_FORMAL_RUNFLOW_FALSE` | No formal RunFlow connection. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_SAVE_FALSE` | No SaveData writes. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_REWARD_FALSE` | No Reward grants. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_CHAPTER_FALSE` | No Chapter progress. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_FORMAL_TABLE_FALSE` | No formal StageConfig, Boss table, or Reward table touch. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_V02_V03_FALSE` | No V0.2/V0.3 formal chapter touch. | `V0.4-BattleSandboxDevChapterPlayable01` |
| `Info` | `DEV_CHAPTER_PLAYABLE_BINDER_FALSE` | No scene binder is run. | `V0.4-BattleSandboxDevChapterPlayable01` |
