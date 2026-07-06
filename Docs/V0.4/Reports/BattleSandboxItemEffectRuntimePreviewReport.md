# BattleSandbox Item Effect Runtime Preview Report

Package: `V0.4-BattleSandboxItemEffectRuntimePreview01`
Generated: `2026-07-06 13:59:47`
Status: `PASS`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- Runs only inside V0.4 BuildSandbox / BattleSandbox runtime preview surfaces.
- Reuses existing runtime loop HUD text, Boss state line, cast feedback, combat log, and floating text.
- Adds no dedicated UI frame and does not author or reorder V04 UI layout.
- Does not call or modify V0.2/V0.3 formal RunFlow, DamageText, SaveData, Reward, Boss table, Chapter, or formal numeric data.
- Player-facing runtime rows use Chinese effect text; internal effect keys stay in reports only.

## Counters

- roster items: `23`
- effect profiles: `23`
- mapped roster items: `23`
- distinct effect families: `9`
- attack profiles: `8`
- support profiles: `15`
- energy profiles: `3`
- guard profiles: `3`
- cleanse profiles: `4`
- control profiles: `5`
- peach profiles: `2`
- runtime sample rows: `1133`
- item effect runtime coverage rows: `23`
- support damage leak count: `0`
- player-side answer leak count: `0`
- formal leak count: `0`
- UI layout write count: `0`

## Effect Profiles

| Item | Display | Effect | Role | Damage | Support | Source |
| --- | --- | --- | --- | ---: | ---: | --- |
| `preview_stone_core` | 炉芯石 | 供能 | 供能 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.stone_core` |
| `preview_soul_seal` | 镇魂法印 | 镇魂 | 控制 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.soul_seal` |
| `preview_taomu_sword` | 桃木剑 | 桃木 | 破邪 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.taomu_sword` |
| `preview_cleanse_corner` | 净化折符 | 净化 | 净化 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.cleanse_corner` |
| `preview_old_bell` | 镇邪铃 | 镇魂 | 控制 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.old_bell` |
| `preview_x2_wood_talisman` | 护阵木牌 | 护阵 | 防御 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.guard_wood_plate` |
| `preview_guard_wood` | 守护木牌 | 护阵 | 防御 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.guard_wood` |
| `preview_energy_incense` | 醒符香 | 供能 | 供能 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.energy_incense` |
| `preview_fire_talisman` | 炽火符 | 火焰 | 攻击 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.fire_burst` |
| `preview_thunder_sword` | 雷引剑符 | 雷击 | 破盾 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.thunder_sword` |
| `fire_talisman_basic` | 火符 | 火焰 | 攻击 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.fire_basic` |
| `thunder_talisman_basic` | 雷符 | 雷击 | 破盾 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.thunder_basic` |
| `shield_talisman_basic` | 护身符 | 护阵 | 防御 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.guard_basic` |
| `qi_pill_basic` | 丹药 | 净化 | 净化 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.qi_pill` |
| `spirit_stone_basic` | 聚灵石 | 供能 | 供能 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.spirit_stone` |
| `sword_pill_basic` | 剑丸 | 剑击 | 攻击 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.sword_pill` |
| `chain_thunder_talisman_basic` | 连锁雷符 | 雷击 | 破盾 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.thunder_chain` |
| `purify_talisman_basic` | 净化符 | 净化 | 净化 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.purify_basic` |
| `soul_suppress_talisman_basic` | 镇魂符 | 镇魂 | 控制 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.soul_suppress` |
| `seal_basic` | 法印 | 镇魂 | 控制 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.seal_basic` |
| `water_talisman_basic` | 水符 | 净化 | 净化 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.water_talisman` |
| `exorcism_bell_basic` | 驱邪铃 | 驱邪 | 驱邪 | `True` | `False` | `BattleSandboxItemEffectRuntimePreviewCatalog.exorcism_bell` |
| `peach_wood_basic` | 桃木牌 | 桃木 | 破邪 | `False` | `True` | `BattleSandboxItemEffectRuntimePreviewCatalog.peach_wood_basic` |

## Runtime Samples

| Item | Effect | Rows | Damage | Floating | Log | Result |
| --- | --- | ---: | ---: | --- | --- | --- |
| `preview_stone_core` | 供能 / 供能 | 28/56 | 0 | 供能 +22 | 【供能】炉芯石炉芯供能，灵气 +22 | `PASS` |
| `preview_soul_seal` | 镇魂 / 控制 | 24/56 | 0 | 镇魂待发 | 【冷却】镇魂法印 2.6秒，控制待发 | `PASS` |
| `preview_taomu_sword` | 桃木 / 破邪 | 26/50 | 132 | 桃木待发 | 【冷却】桃木剑 2.4秒，破邪待发 | `PASS` |
| `preview_cleanse_corner` | 净化 / 净化 | 24/56 | 0 | 净化待发 | 【冷却】净化折符 2.1秒，净化待发 | `PASS` |
| `preview_old_bell` | 镇魂 / 控制 | 24/56 | 0 | 镇魂待发 | 【冷却】镇邪铃 2.4秒，控制待发 | `PASS` |
| `preview_x2_wood_talisman` | 护阵 / 防御 | 24/56 | 0 | 护阵待发 | 【冷却】护阵木牌 2.2秒，防御待发 | `PASS` |
| `preview_guard_wood` | 护阵 / 防御 | 24/56 | 0 | 护阵待发 | 【冷却】守护木牌 2.2秒，防御待发 | `PASS` |
| `preview_energy_incense` | 供能 / 供能 | 16/50 | 0 | 灵力流动 +26 | 【供能】醒符香香气牵动灵力，灵气 +26 | `PASS` |
| `preview_fire_talisman` | 火焰 / 攻击 | 26/54 | 132 | 火焰待发 | 【冷却】炽火符 1.4秒，攻击待发 | `PASS` |
| `preview_thunder_sword` | 雷击 / 破盾 | 26/49 | 132 | 雷击待发 | 【冷却】雷引剑符 1.9秒，破盾待发 | `PASS` |
| `fire_talisman_basic` | 火焰 / 攻击 | 17/36 | 132 | 火焰待发 | 【冷却】火符 1.5秒，攻击待发 | `PASS` |
| `thunder_talisman_basic` | 雷击 / 破盾 | 10/23 | 132 | 雷击待发 | 【冷却】雷符 2.8秒，破盾待发 | `PASS` |
| `shield_talisman_basic` | 护阵 / 防御 | 28/56 | 0 | 护盾展开 | 【护阵】护身符撑起护盾 | `PASS` |
| `qi_pill_basic` | 净化 / 净化 | 24/50 | 0 | 回气 | 【净化】丹药回气调息 | `PASS` |
| `spirit_stone_basic` | 供能 / 供能 | 12/50 | 0 | 灵气 +26 | 【供能】聚灵石聚起灵气，灵气 +26 | `PASS` |
| `sword_pill_basic` | 剑击 / 攻击 | 23/48 | 132 | 剑击待发 | 【冷却】剑丸 2.0秒，攻击待发 | `PASS` |
| `chain_thunder_talisman_basic` | 雷击 / 破盾 | 13/29 | 132 | 雷击待发 | 【冷却】连锁雷符 3.6秒，破盾待发 | `PASS` |
| `purify_talisman_basic` | 净化 / 净化 | 31/58 | 0 | 净化 | 【净化】净化符净化负面气息 | `PASS` |
| `soul_suppress_talisman_basic` | 镇魂 / 控制 | 28/55 | 0 | 镇魂 | 【镇魂】镇魂符镇魂压制 | `PASS` |
| `seal_basic` | 镇魂 / 控制 | 16/50 | 0 | 法印 | 【镇魂】法印维持法印 | `PASS` |
| `water_talisman_basic` | 净化 / 净化 | 24/50 | 0 | 水气 | 【净化】水符水气流转 | `PASS` |
| `exorcism_bell_basic` | 驱邪 / 驱邪 | 18/37 | 132 | 驱邪待发 | 【冷却】驱邪铃 3.8秒，驱邪待发 | `PASS` |
| `peach_wood_basic` | 桃木 / 破邪 | 18/52 | 0 | 破邪 | 【桃木】桃木牌桃木破邪 | `PASS` |

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| UI Layout Guard | `PASS` | 0 | 0 | 29 |
| BattleSandbox Runtime Loop 01 | `PASS` | 0 | 0 | 66 |
| BattleSandbox Item Effect Runtime Preview 01 | `PASS` | 0 | 0 | 0 |

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
| `Info` | `RUNTIME_LOOP_ROW_COUNT` | runtime loop row count pass. actual=60, expected>=12. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_MANA_ROWS` | mana row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_COOLDOWN_ROWS` | cooldown row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_TRIGGER_ROWS` | item trigger row count pass. actual=16, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ENEMY_HP_ROWS` | enemy HP row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ENEMY_SHIELD_ROWS` | enemy shield update row count pass. actual=3, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_PLAYER_HP_ROWS` | player HP row count pass. actual=6, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_PLAYER_SHIELD_ROWS` | player shield row count pass. actual=11, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_BOSS_CAST_ROWS` | Boss cast-bar row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ATTACK_ROWS` | enemy attack row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ATTACK_TIMER_ADVANCES` | enemy attack timer advance row count pass. actual=8, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ATTACK_DAMAGE_PROFILE_ROWS` | devOnly profile attack damage row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SHIELD_FIRST_ROWS` | shield-first player damage row count pass. actual=4, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_LOG_ROWS` | combat log row count pass. actual=60, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_FLOATING_ROWS` | floating text row count pass. actual=60, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SANDBOX_RESULT_ROWS` | sandbox result row count pass. actual=1, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_NO_SETTLEMENT_ZERO` | legacy no-settlement row pass. actual=0. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_ROWS_READY` | Rows=60, mana=8, trigger=16, bossCast=8, attacks=4, sandboxResults=1. | `BattleSandboxRuntimeLoopPreview` |
| `Info` | `RUNTIME_LOOP_SOURCE_KERNEL_ROWS` | CombatKernelAdapter source row count pass. actual=9, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
| `Info` | `RUNTIME_LOOP_SOURCE_BUILD_PREVIEW_ROWS` | BuildCombatPreview source row count pass. actual=87, expected>=1. | `V0.4-BattleSandboxRuntimeLoop01` |
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
