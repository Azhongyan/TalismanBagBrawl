# Dev Chapter Balance Run Report

Package: `V0.4-DevChapterBalanceRun01`
Generated: `2026-07-04 01:43:08`
Status: `PASS`
Errors: `0`
Warnings: `0`

## Scope

- devOnly 3-10 / 4-10 balance validation run only.
- Chains BuildProblemSeedData, EnemyBossValidationPool, BuildCombatPreview, and ShapeBuildRulePreview.
- Writes reports and developer data panel fields only.
- Does not create formal chapter entrance, save, reward, chapter progress, FeatureFlag, V02/V03 scene change, or current V04 RectTransform change.
- Player-side text stays masked Chinese combat feedback; complete answers remain out of player UI.

## Summary

| Metric | Value |
| --- | ---: |
| Stage count | 4 |
| 3-10 stages | 2 |
| 4-10 stages | 2 |
| Too easy | 1 |
| Fit | 1 |
| Too hard | 2 |
| Player answer leaks | 0 |
| Formal leaks | 0 |
| FeatureFlag default true | 0 |

## Balance Rows

| Stage | Dev Chapter | Recommended Test Target | Current Build Feedback | Boss / Mechanic Feedback | Difficulty | Tuning Suggestion |
| --- | --- | --- | --- | --- | --- | --- |
| `dev_balance_3_10_guard_wall` | `3-10` | 验证竖向护阵墙能否稳定挡住连续敲阵。 | 【阵势】护阵连成墙，首领攻势变缓。；【阵势】护阵连成墙，首领攻势变缓。 | 叩阵铜将：破绽窗口已经出现，战斗节奏偏紧。铜铃会打乱施法节奏，缺少镇魂或护阵时容易被连锁施法压制。 | 过难 | 当前太难，降低连续敲阵密度，先保证护阵反馈能被稳定看见。 |
| `dev_balance_3_10_cleanse_corner` | `3-10` | 验证拐角净化阵面对污染扩散时的反馈强度。 | 【机制】拐角处亮起净化回响。；【状态】护势开始松动 | 秽签梦母：破绽窗口已经出现，战斗节奏偏紧。回潮会持续拉低供能效率，净化不足时污染格会扩大。 | 过易 | 当前太简单，提高污染扩散速度或延后净化窗口，避免测试过早结束。 |
| `dev_balance_4_10_furnace_core` | `4-10` | 验证炉芯核心阵能否撑住供能干扰和短窗爆发。 | 【核心】符光贴近炉芯，场上节奏变稳。；【状态】护势开始松动 | 偷灵炉心：破绽窗口已经出现，战斗节奏仍可观察。炉灰会污染关键摆放位，净化和护阵缺一都容易失守。 | 合适 | 当前合适，维持当前偷灵节奏，继续观察炉芯核心阵反馈差异。 |
| `dev_balance_4_10_thunder_fire_cross` | `4-10` | 验证雷火交错阵面对复合阵眼时是否短板过大。 | 【输出】雷火相邻，压制感增强。；【状态】护势开始松动 | 黑炉复合阵眼：短板会在读条后被放大，战斗节奏偏紧。场地会干扰供能与摆放，请观察阵面节奏。 | 过难 | 当前太难，降低复合阵眼前两轮压力，或补一件护阵测试件再验证。 |

## Developer Data Panel Fields

| Stage | Field | Label | Value | Source |
| --- | --- | --- | --- | --- |
| `dev_balance_3_10_guard_wall` | `devChapterLabel` | 验证关标签 | `3-10` | `DevChapterBalanceRun.stage` |
| `dev_balance_3_10_guard_wall` | `mapRuleId` | 地图规则编号 | `dev_map_copper_bell_night` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_guard_wall` | `enemyProblemId` | 敌人问题编号 | `dev_enemy_caster_problem` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_guard_wall` | `bossProblemId` | 首领问题编号 | `dev_boss_bronze_formation_general` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_guard_wall` | `bossProfileId` | 首领验证编号 | `dev_boss_burst_huzhen` | `EnemyBossValidationPool.CreateDefault` |
| `dev_balance_3_10_guard_wall` | `shapeRuleKey` | 形态规则编号 | `vertical_defense_wall` | `BattleSandboxShapeBuildRulePreviewBuilder.Evaluate` |
| `dev_balance_3_10_guard_wall` | `previewBuildId` | 预览构筑编号 | `dev_balance_3_10_guard_wall` | `BattleSandboxBuildCombatPreviewBuilder.Build` |
| `dev_balance_3_10_guard_wall` | `readinessKeyRatio` | 关键项命中率 | `4/4` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_guard_wall` | `simulatedWinRate` | 预估胜率 | `0.403` | `BuildSimulationRunner` |
| `dev_balance_3_10_guard_wall` | `simulatedClearTime` | 预估时长 | `266.7` | `BuildSimulationRunner` |
| `dev_balance_3_10_guard_wall` | `difficultyTendency` | 难度倾向 | `过难` | `DevChapterBalanceRun.difficulty` |
| `dev_balance_3_10_guard_wall` | `readyBossWindow` | 破绽窗口 | `True` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_cleanse_corner` | `devChapterLabel` | 验证关标签 | `3-10` | `DevChapterBalanceRun.stage` |
| `dev_balance_3_10_cleanse_corner` | `mapRuleId` | 地图规则编号 | `dev_map_bluestone_damp` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_cleanse_corner` | `enemyProblemId` | 敌人问题编号 | `dev_enemy_polluted_tile_problem` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_cleanse_corner` | `bossProblemId` | 首领问题编号 | `dev_boss_dirty_dream_mother` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_cleanse_corner` | `bossProfileId` | 首领验证编号 | `dev_boss_debuff_jinge` | `EnemyBossValidationPool.CreateDefault` |
| `dev_balance_3_10_cleanse_corner` | `shapeRuleKey` | 形态规则编号 | `corner_cleanse_array` | `BattleSandboxShapeBuildRulePreviewBuilder.Evaluate` |
| `dev_balance_3_10_cleanse_corner` | `previewBuildId` | 预览构筑编号 | `dev_balance_3_10_cleanse_corner` | `BattleSandboxBuildCombatPreviewBuilder.Build` |
| `dev_balance_3_10_cleanse_corner` | `readinessKeyRatio` | 关键项命中率 | `4/4` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_3_10_cleanse_corner` | `simulatedWinRate` | 预估胜率 | `0.512` | `BuildSimulationRunner` |
| `dev_balance_3_10_cleanse_corner` | `simulatedClearTime` | 预估时长 | `237.5` | `BuildSimulationRunner` |
| `dev_balance_3_10_cleanse_corner` | `difficultyTendency` | 难度倾向 | `过易` | `DevChapterBalanceRun.difficulty` |
| `dev_balance_3_10_cleanse_corner` | `readyBossWindow` | 破绽窗口 | `True` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_furnace_core` | `devChapterLabel` | 验证关标签 | `4-10` | `DevChapterBalanceRun.stage` |
| `dev_balance_4_10_furnace_core` | `mapRuleId` | 地图规则编号 | `dev_map_furnace_ash_fall` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_furnace_core` | `enemyProblemId` | 敌人问题编号 | `dev_enemy_spirit_thief_problem` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_furnace_core` | `bossProblemId` | 首领问题编号 | `dev_boss_spirit_thief_core` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_furnace_core` | `bossProfileId` | 首领验证编号 | `dev_boss_energy_juneng` | `EnemyBossValidationPool.CreateDefault` |
| `dev_balance_4_10_furnace_core` | `shapeRuleKey` | 形态规则编号 | `furnace_core_array` | `BattleSandboxShapeBuildRulePreviewBuilder.Evaluate` |
| `dev_balance_4_10_furnace_core` | `previewBuildId` | 预览构筑编号 | `dev_balance_4_10_furnace_core` | `BattleSandboxBuildCombatPreviewBuilder.Build` |
| `dev_balance_4_10_furnace_core` | `readinessKeyRatio` | 关键项命中率 | `3/4` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_furnace_core` | `simulatedWinRate` | 预估胜率 | `0.640` | `BuildSimulationRunner` |
| `dev_balance_4_10_furnace_core` | `simulatedClearTime` | 预估时长 | `199.8` | `BuildSimulationRunner` |
| `dev_balance_4_10_furnace_core` | `difficultyTendency` | 难度倾向 | `合适` | `DevChapterBalanceRun.difficulty` |
| `dev_balance_4_10_furnace_core` | `readyBossWindow` | 破绽窗口 | `True` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_thunder_fire_cross` | `devChapterLabel` | 验证关标签 | `4-10` | `DevChapterBalanceRun.stage` |
| `dev_balance_4_10_thunder_fire_cross` | `mapRuleId` | 地图规则编号 | `dev_map_bluestone_crack` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_thunder_fire_cross` | `enemyProblemId` | 敌人问题编号 | `dev_enemy_formation_eye_problem` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_thunder_fire_cross` | `bossProblemId` | 首领问题编号 | `dev_boss_black_furnace_complex_eye` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_thunder_fire_cross` | `bossProfileId` | 首领验证编号 | `dev_boss_hybrid_combo` | `EnemyBossValidationPool.CreateDefault` |
| `dev_balance_4_10_thunder_fire_cross` | `shapeRuleKey` | 形态规则编号 | `thunder_fire_cross_array` | `BattleSandboxShapeBuildRulePreviewBuilder.Evaluate` |
| `dev_balance_4_10_thunder_fire_cross` | `previewBuildId` | 预览构筑编号 | `dev_balance_4_10_thunder_fire_cross` | `BattleSandboxBuildCombatPreviewBuilder.Build` |
| `dev_balance_4_10_thunder_fire_cross` | `readinessKeyRatio` | 关键项命中率 | `2/5` | `BuildProblemSeedData.CreateDefault` |
| `dev_balance_4_10_thunder_fire_cross` | `simulatedWinRate` | 预估胜率 | `0.438` | `BuildSimulationRunner` |
| `dev_balance_4_10_thunder_fire_cross` | `simulatedClearTime` | 预估时长 | `254.1` | `BuildSimulationRunner` |
| `dev_balance_4_10_thunder_fire_cross` | `difficultyTendency` | 难度倾向 | `过难` | `DevChapterBalanceRun.difficulty` |
| `dev_balance_4_10_thunder_fire_cross` | `readyBossWindow` | 破绽窗口 | `False` | `BuildProblemSeedData.CreateDefault` |

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Dev Chapter Balance Run | `PASS` | 0 | 0 | 13 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `DEV_BALANCE_RUN_FLAGS_OK` | Run flags scanned. stages=4, leaks=0/0. | `DevChapterBalanceRun` |
| `Info` | `DEV_BALANCE_STAGE_SCANNED` | dev_balance_3_10_guard_wall 3-10 difficulty=过难 winRate=0.403. | `DevChapterBalanceRunStage:dev_balance_3_10_guard_wall` |
| `Info` | `DEV_BALANCE_STAGE_SCANNED` | dev_balance_3_10_cleanse_corner 3-10 difficulty=过易 winRate=0.512. | `DevChapterBalanceRunStage:dev_balance_3_10_cleanse_corner` |
| `Info` | `DEV_BALANCE_STAGE_SCANNED` | dev_balance_4_10_furnace_core 4-10 difficulty=合适 winRate=0.640. | `DevChapterBalanceRunStage:dev_balance_4_10_furnace_core` |
| `Info` | `DEV_BALANCE_STAGE_SCANNED` | dev_balance_4_10_thunder_fire_cross 4-10 difficulty=过难 winRate=0.438. | `DevChapterBalanceRunStage:dev_balance_4_10_thunder_fire_cross` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_BALANCE_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |

## QA Notes

- Run menu: `Tools/Talisman Bag/V0.4/BuildSandbox/DevChapterBalanceRun01/[QA Only] Run Dev Chapter Balance Run`.
- Batch method: `TalismanBag.EditorTools.BuildSandbox.DevChapterBalanceRunValidator.RunBatch`.
- Report rows are devOnly tuning estimates; they are not formal 3-10 / 4-10 data.
