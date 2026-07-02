# Synergy Evaluator Core Report

Package: `V0.3/V0.4-BuildSandbox-SynergyEvaluatorCore01`
Generated: `2026-06-30 22:29:21`
Status: `PASS`

## Scope

- Pure BuildSandbox snapshot evaluation only.
- Input is `BuildSandboxLayoutSnapshot`; output is `BuildEvaluationResult`.
- No formal combat, UI, RunFlow, PageState, FormationState, save, reward, boss, or numeric connection.
- Feature flags remain default false; seed configs remain devOnly and disabled.

## Input Snapshot

| itemId | tags | occupiedCells | isPowered | energySourceId |
| --- | --- | --- | --- | --- |
| `seed_lihuo_1` | `离火;符箓;聚能` | `0,0` | `True` | `seed_shared_lihuo_source` |
| `seed_lihuo_2` | `离火;符箓;聚能` | `1,0` | `True` | `seed_shared_lihuo_source` |
| `seed_lihuo_3` | `离火;符箓;聚能` | `2,0` | `True` | `seed_shared_lihuo_source` |
| `seed_lihuo_4` | `离火;符箓;聚能` | `3,0` | `True` | `seed_shared_lihuo_source` |
| `seed_guardian_1` | `护阵;法器;聚能` | `0,1` | `True` | `seed_lihuo_1` |
| `seed_guardian_2` | `护阵;法器;聚能` | `1,1` | `True` | `seed_lihuo_1` |
| `seed_corruption_1` | `净厄;镇魂;黑炉污染` | `6,0` | `False` | `` |
| `seed_corruption_2` | `净厄;镇魂;黑炉污染` | `7,1` | `False` | `` |

## Active Synergies

| synergyId | activeThresholds | sourceItems | placementSatisfied | energySatisfied | nextThresholdHint |
| --- | --- | --- | --- | --- | --- |
| `bs_synergy_corruption_ward` | `bs_synergy_corruption_ward:2` | `seed_corruption_1;seed_corruption_2` | `True` | `True` | `bs_synergy_corruption_ward:2->4 missing=2` |
| `bs_synergy_guardian_energy` | `bs_synergy_guardian_energy:2;bs_synergy_guardian_energy:4;bs_synergy_guardian_energy:6` | `seed_guardian_1;seed_guardian_2;seed_lihuo_1;seed_lihuo_2;seed_lihuo_3;seed_lihuo_4` | `True` | `True` | `none` |
| `bs_synergy_lihuo_talisman` | `bs_synergy_lihuo_talisman:2;bs_synergy_lihuo_talisman:4` | `seed_lihuo_1;seed_lihuo_2;seed_lihuo_3;seed_lihuo_4` | `True` | `True` | `bs_synergy_lihuo_talisman:4->6 missing=2` |

## Missing Requirements

| synergyId | requirementId | type | requiredTag | requiredCount | actualCount | detail |
| --- | --- | --- | --- | ---: | ---: | --- |

## BuildEvaluationResult

- activeSynergies: `3`
- activeThresholds: `bs_synergy_corruption_ward:2;bs_synergy_guardian_energy:2;bs_synergy_guardian_energy:4;bs_synergy_guardian_energy:6;bs_synergy_lihuo_talisman:2;bs_synergy_lihuo_talisman:4`
- sourceItems: `seed_corruption_1;seed_corruption_2;seed_guardian_1;seed_guardian_2;seed_lihuo_1;seed_lihuo_2;seed_lihuo_3;seed_lihuo_4`
- placementSatisfied: `True`
- energySatisfied: `True`
- missingRequirements: `0`
- nextThresholdHint: `bs_synergy_corruption_ward:2->4 missing=2`

## FeatureFlag Defaults

| Flag | Default | Scope |
| --- | --- | --- |
| `EnableSynergyBuild` | `False` | Synergy build sandbox |
| `EnableAffixSystem` | `False` | Affix and rarity sandbox |
| `EnableDevBuildContent` | `False` | devOnly content pools |
| `EnableBuildModifierInCombat` | `False` | Combat modifier bridge |
| `EnableBuildDebugReport` | `False` | Debug report export |
| `EnableItemShapeOccupancy` | `False` | Item shape occupancy |
| `EnableShapePlacementSandbox` | `False` | Shape placement sandbox |
| `EnableShapeRotation` | `False` | Shape rotation sandbox |

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Config Validation | `PASS` | 0 | 0 | 211 |
| devOnly Isolation | `PASS` | 0 | 0 | 25 |
| UI Layout Guard | `PASS` | 0 | 0 | 1 |
| CoreFlow Smoke Placeholder | `PASS` | 0 | 0 | 1 |
| ItemShape Occupancy | `PASS` | 0 | 0 | 9 |
| Synergy Evaluator Core | `PASS` | 0 | 0 | 8 |
| Affix Rarity Sandbox | `PASS` | 0 | 0 | 17 |
| Modifier Event Bridge | `PASS` | 0 | 0 | 38 |
| Build Simulation Benchmark | `PASS` | 0 | 0 | 17 |
| Enemy/Boss Validation Pool | `PASS` | 0 | 0 | 46 |
| Dev Chapter Content Pool | `PASS` | 0 | 0 | 140 |
| Ledger Task Build Hooks | `PASS` | 0 | 0 | 39 |
| Final Integration Dry Run | `PASS` | 0 | 0 | 10 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableSynergyBuild=false (Synergy build sandbox). | `BuildSandboxFeatureFlags` |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableAffixSystem=false (Affix and rarity sandbox). | `BuildSandboxFeatureFlags` |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableDevBuildContent=false (devOnly content pools). | `BuildSandboxFeatureFlags` |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableBuildModifierInCombat=false (Combat modifier bridge). | `BuildSandboxFeatureFlags` |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableBuildDebugReport=false (Debug report export). | `BuildSandboxFeatureFlags` |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableItemShapeOccupancy=false (Item shape occupancy). | `BuildSandboxFeatureFlags` |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableShapePlacementSandbox=false (Shape placement sandbox). | `BuildSandboxFeatureFlags` |
| `Info` | `FEATURE_FLAG_DEFAULT_FALSE` | EnableShapeRotation=false (Shape rotation sandbox). | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned affix_bond_plus_one_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_BondPlusOne_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned affix_focused_gather_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_FocusedGather_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned affix_guardian_ward_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_GuardianWard_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned affix_lihuo_spark_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_LihuoSpark_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned affix_orange_core_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_OrangeCore_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned affix_purifying_seal_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_PurifyingSeal_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned rarity_blue_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Blue_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned rarity_green_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Green_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned rarity_orange_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Orange_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned rarity_purple_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Purple_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned rarity_white_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_White_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned guard_baseline_01. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/BuildSandboxGuardBaselineConfig.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemshape_corner3_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Corner3_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemshape_single1_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Single1_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemshape_square4_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Square4_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemshape_vertical2_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Vertical2_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned archetype_lihuo_engine_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Archetype_LihuoEngine_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned archetype_ward_control_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Archetype_WardControl_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemtag_fire_talisman_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_FireTalisman_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemtag_guardian_focus_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_GuardianFocus_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemtag_soul_furnace_residue_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_SoulFurnaceResidue_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned itemtag_thunder_sword_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_ThunderSword_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned synergy_corruption_ward_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Synergy_CorruptionWard_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned synergy_guardian_energy_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Synergy_GuardianEnergy_BuildSandbox.asset` |
| `Info` | `CONFIG_ASSET_SCANNED` | Scanned synergy_lihuo_talisman_buildsandbox. devOnly=True, isEnabled=False. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Synergy_LihuoTalisman_BuildSandbox.asset` |
| `Info` | `REQUIRED_TAGS_PRESENT` | All first-batch BuildSandbox tags are reserved in ItemTagConfig assets. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `SYNERGY_DATA_FOUNDATION_COUNTS` | ItemTagConfig=4, SynergyConfig=3, BuildArchetypeConfig=2. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `ITEM_SHAPE_REQUIRED_PRESENT` | Required ItemShapeConfig is present: Single1. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Single1_BuildSandbox.asset` |
| `Info` | `ITEM_SHAPE_REQUIRED_PRESENT` | Required ItemShapeConfig is present: Vertical2. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Vertical2_BuildSandbox.asset` |
| `Info` | `ITEM_SHAPE_REQUIRED_PRESENT` | Required ItemShapeConfig is present: Corner3. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Corner3_BuildSandbox.asset` |
| `Info` | `ITEM_SHAPE_REQUIRED_PRESENT` | Required ItemShapeConfig is present: Square4. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Square4_BuildSandbox.asset` |
| `Info` | `ITEM_SHAPE_CONFIG_COUNTS` | ItemShapeConfig=4. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `CONFIG_ENEMY_PROFILE_CONFIG_CHECKED` | Checked 11 Enemy/Boss validation profile(s). | `EnemyBossValidationPool` |
| `Info` | `CONFIG_BOSS_PROFILE_CONFIG_CHECKED` | Checked 7 Enemy/Boss validation profile(s). | `EnemyBossValidationPool` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_01 (惊雷开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_02 (惊雷开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_03 (惊雷开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_04 (惊雷开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_05 (惊雷开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_06 (惊雷开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_07 (惊雷开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_08 (惊雷开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_09 (惊雷开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_10 (惊雷开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_01 (离火开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_02 (离火开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_03 (离火开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_04 (离火开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_05 (离火开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_06 (离火开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_07 (离火开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_08 (离火开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_09 (离火开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_10 (离火开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_01 (护阵开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_02 (护阵开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_03 (护阵开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_04 (护阵开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_05 (护阵开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_06 (护阵开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_07 (护阵开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_08 (护阵开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_09 (护阵开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_10 (护阵开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_01 (净厄开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_02 (净厄开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_03 (净厄开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_04 (净厄开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_05 (净厄开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_06 (净厄开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_07 (净厄开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_08 (净厄开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_09 (净厄开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_10 (净厄开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_01 (镇魂开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_02 (镇魂开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_03 (镇魂开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_04 (镇魂开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_05 (镇魂开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_06 (镇魂开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_07 (镇魂开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_08 (镇魂开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_09 (镇魂开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_10 (镇魂开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_01 (聚能开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_02 (聚能开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_03 (聚能开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_04 (聚能开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_05 (聚能开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_06 (聚能开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_07 (聚能开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_08 (聚能开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_09 (聚能开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_10 (聚能开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_01 (混合Boss开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_02 (混合Boss开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_03 (混合Boss开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_04 (混合Boss开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_05 (混合Boss开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_05` |
| `Info` | `LEDGER_TASK_HOOK_SCANNED` | hookId=ledger_build_task_hooks_dev_preview, goals=8, devOnly=True, isEnabled=False. | `LedgerBuildTaskHook` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_activate_synergy_2: 激活一次 2 件羁绊, source=BuildEvaluationResult, required=2. | `LedgerBuildTaskGoal:ledger_goal_activate_synergy_2` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_activate_synergy_4: 激活一次 4 件羁绊, source=BuildEvaluationResult, required=4. | `LedgerBuildTaskGoal:ledger_goal_activate_synergy_4` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_activate_synergy_6: 激活一次 6 件羁绊, source=BuildEvaluationResult, required=6. | `LedgerBuildTaskGoal:ledger_goal_activate_synergy_6` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_thunder_defeat_shield_boss: 用惊雷 Build 击败护盾 Boss, source=BuildSimulationResult, required=1. | `LedgerBuildTaskGoal:ledger_goal_thunder_defeat_shield_boss` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_cleanse_negative_status: 用净厄 Build 清除负面状态, source=BuildSimulationResult, required=1. | `LedgerBuildTaskGoal:ledger_goal_cleanse_negative_status` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_complete_specified_build_validation: 完成一次指定 Build 验证, source=DevChapterContentPoolResult, required=1. | `LedgerBuildTaskGoal:ledger_goal_complete_specified_build_validation` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_obtain_orange_core_affix: 获得一个橙色核心词条, source=AffixPreview, required=1. | `LedgerBuildTaskGoal:ledger_goal_obtain_orange_core_affix` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_complete_affix_reroll: 完成一次词条洗练, source=AffixPreview, required=1. | `LedgerBuildTaskGoal:ledger_goal_complete_affix_reroll` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_activate_synergy_2. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_activate_synergy_4. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_activate_synergy_6. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_thunder_defeat_shield_boss. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_cleanse_negative_status. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_complete_specified_build_validation. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_obtain_orange_core_affix. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_complete_affix_reroll. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: BuildEvaluationResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: BuildSimulationResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: AffixPreview. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: EnemyBossValidationPoolResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: DevChapterContentPoolResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_activate_synergy_2 observed=6, required=2, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_activate_synergy_2` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_activate_synergy_4 observed=6, required=4, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_activate_synergy_4` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_activate_synergy_6 observed=6, required=6, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_activate_synergy_6` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_thunder_defeat_shield_boss observed=5, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_thunder_defeat_shield_boss` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_cleanse_negative_status observed=15, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_cleanse_negative_status` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_complete_specified_build_validation observed=5, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_complete_specified_build_validation` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_obtain_orange_core_affix observed=1, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_obtain_orange_core_affix` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_complete_affix_reroll observed=5, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_complete_affix_reroll` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_SOURCE_ISOLATION_SCANNED` | Ledger task hook source scan completed. | `LedgerBuildTaskHook` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_BondPlusOne_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_FocusedGather_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_GuardianWard_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_LihuoSpark_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_OrangeCore_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_PurifyingSeal_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Blue_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Green_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Orange_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Purple_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_White_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/BuildSandboxGuardBaselineConfig.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Corner3_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Single1_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Square4_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/ItemShapeOccupancy01/ItemShape_Vertical2_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Archetype_LihuoEngine_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Archetype_WardControl_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_FireTalisman_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_GuardianFocus_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_SoulFurnaceResidue_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/ItemTag_ThunderSword_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Synergy_CorruptionWard_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Synergy_GuardianEnergy_BuildSandbox.asset` |
| `Info` | `DEVONLY_ISOLATED` | Config is devOnly and disabled. | `Assets/_Game/Configs/BuildSandbox/SynergyDataFoundation01/Synergy_LihuoTalisman_BuildSandbox.asset` |
| `Info` | `UI_LAYOUT_GUARD_PASS` | BuildSandbox code scan found no scene access, formal UI creation, or layout write tokens. | `` |
| `Info` | `CORE_FLOW_PLACEHOLDER_ONLY` | Placeholder smoke entry is isolated. It does not load scenes, mutate saves, or enter product flow. | `` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_single_sample valid=True, reason=None, occupiedCells=1. | `shape_single_sample` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_vertical_sample valid=True, reason=None, occupiedCells=2. | `shape_vertical_sample` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_corner_sample valid=True, reason=None, occupiedCells=3. | `shape_corner_sample` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_square_sample valid=True, reason=None, occupiedCells=4. | `shape_square_sample` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_out_of_grid_sample valid=False, reason=OutOfGrid, occupiedCells=4. | `shape_out_of_grid_sample` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_overlap_sample valid=False, reason=CellOccupied, occupiedCells=2. | `shape_overlap_sample` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_missing_sample valid=False, reason=MissingShapeConfig, occupiedCells=0. | `shape_missing_sample` |
| `Info` | `PLACEMENT_SAMPLE_PASS` | shape_invalid_sample valid=False, reason=ShapeInvalid, occupiedCells=0. | `shape_invalid_sample` |
| `Info` | `LAYOUT_SNAPSHOT_OCCUPIED_CELLS_PRESENT` | Sandbox snapshot placedItems=5, each with occupiedCells. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `ACTIVE_SYNERGY_PRESENT` | Active synergies=3, activeThresholds=6. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `THRESHOLD_ACTIVATED` | Seed snapshot activates a 2-piece threshold. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `THRESHOLD_ACTIVATED` | Seed snapshot activates a 4-piece threshold. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `PLACEMENT_REQUIREMENT_SATISFIED` | Seed snapshot satisfies placement requirements. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `ENERGY_REQUIREMENT_SATISFIED` | Seed snapshot satisfies energy requirements. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `NEXT_THRESHOLD_HINT_PRESENT` | bs_synergy_corruption_ward needs 2 item(s) for 4. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `THRESHOLD_8_ACTIVATED` | Synthetic 8-piece snapshot activates the 8-piece threshold. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `MISSING_REQUIREMENTS_REPORTED` | Partial snapshot missingRequirements=3. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `RARITY_REQUIRED_PRESENT` | Required sandbox rarity is present: white. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_White_BuildSandbox.asset` |
| `Info` | `RARITY_REQUIRED_PRESENT` | Required sandbox rarity is present: green. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Green_BuildSandbox.asset` |
| `Info` | `RARITY_REQUIRED_PRESENT` | Required sandbox rarity is present: blue. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Blue_BuildSandbox.asset` |
| `Info` | `RARITY_REQUIRED_PRESENT` | Required sandbox rarity is present: purple. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Purple_BuildSandbox.asset` |
| `Info` | `RARITY_REQUIRED_PRESENT` | Required sandbox rarity is present: orange. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Rarity_Orange_BuildSandbox.asset` |
| `Info` | `AFFIX_CONFIG_COUNTS` | AffixConfig=6, RarityTierConfig=5. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_REQUIRED_PRESENT` | Required sandbox affix is present: bs_affix_orange_core. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_OrangeCore_BuildSandbox.asset` |
| `Info` | `AFFIX_REQUIRED_PRESENT` | Required sandbox affix is present: bs_affix_bond_plus_one. | `Assets/_Game/Configs/BuildSandbox/AffixRaritySandbox01/Affix_BondPlusOne_BuildSandbox.asset` |
| `Info` | `AFFIX_RARITY_REQUIREMENTS_SATISFIED` | Seed snapshot evaluated 5 item(s) without missing requirements. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_RARITY_SAMPLE_PRESENT` | Sample includes rarity: white. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_RARITY_SAMPLE_PRESENT` | Sample includes rarity: green. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_RARITY_SAMPLE_PRESENT` | Sample includes rarity: blue. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_RARITY_SAMPLE_PRESENT` | Sample includes rarity: purple. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_RARITY_SAMPLE_PRESENT` | Sample includes rarity: orange. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_SELECTION_PRESENT` | Selected affixes=6. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_SAMPLE_PRESENT` | Sample selects affix: bs_affix_orange_core. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `AFFIX_SAMPLE_PRESENT` | Sample selects affix: bs_affix_bond_plus_one. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `BRIDGE_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BRIDGE_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BRIDGE_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `MODIFIER_TYPE_RESERVED` | Reserved modifier type is supported: damageBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_RESERVED` | Reserved modifier type is supported: cooldownBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_RESERVED` | Reserved modifier type is supported: shieldBreakBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_RESERVED` | Reserved modifier type is supported: shieldBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_RESERVED` | Reserved modifier type is supported: cleanseBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_RESERVED` | Reserved modifier type is supported: controlDurationBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_RESERVED` | Reserved modifier type is supported: energyReturnBonus. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onBattleStart. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onShieldBreak. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onCleanse. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onShieldBroken. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onEnemyKill. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onBossSkillCast. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onLowHp. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_RESERVED` | Reserved event type is supported: onEnergyConnected. | `ModifierEventBridge` |
| `Info` | `BRIDGE_INPUT_ACTIVE_SYNERGY_PRESENT` | activeSynergies=3, activeThresholds=6. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `BRIDGE_INPUT_AFFIX_PREVIEW_PRESENT` | affixItems=5, selectedAffixes=6. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `MODIFIER_TYPE_PREVIEW_GENERATED` | Sample preview generated modifier type: damageBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_PREVIEW_GENERATED` | Sample preview generated modifier type: cooldownBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_PREVIEW_GENERATED` | Sample preview generated modifier type: shieldBreakBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_PREVIEW_GENERATED` | Sample preview generated modifier type: shieldBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_PREVIEW_GENERATED` | Sample preview generated modifier type: cleanseBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_PREVIEW_GENERATED` | Sample preview generated modifier type: controlDurationBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_TYPE_PREVIEW_GENERATED` | Sample preview generated modifier type: energyReturnBonus. | `ModifierEventBridge` |
| `Info` | `MODIFIER_BUNDLE_PREVIEW_PRESENT` | CombatModifierBundle preview modifiers=33. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onBattleStart. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onShieldBreak. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onCleanse. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onShieldBroken. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onEnemyKill. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onBossSkillCast. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onLowHp. | `ModifierEventBridge` |
| `Info` | `EVENT_TYPE_PREVIEW_GENERATED` | Sample preview generated event type: onEnergyConnected. | `ModifierEventBridge` |
| `Info` | `EVENT_BUNDLE_PREVIEW_PRESENT` | EffectEventBundle preview events=36. | `ModifierEventBridge` |
| `Info` | `BRIDGE_SOURCE_ISOLATION_SCANNED` | ModifierEventBridge source scan completed for forbidden formal-system tokens. | `ModifierEventBridge` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BENCHMARK_SCENARIO_SCANNED` | Scenario sandbox_no_synergy: enemy=devOnly_training_dummy, boss=none, affixes=6. | `BuildSimulationScenario:sandbox_no_synergy` |
| `Info` | `BENCHMARK_SCENARIO_SCANNED` | Scenario sandbox_no_affix: enemy=devOnly_training_dummy, boss=none, affixes=0. | `BuildSimulationScenario:sandbox_no_affix` |
| `Info` | `BENCHMARK_SCENARIO_SCANNED` | Scenario sandbox_full_preview: enemy=devOnly_training_dummy, boss=none, affixes=6. | `BuildSimulationScenario:sandbox_full_preview` |
| `Info` | `BENCHMARK_SCENARIO_SCANNED` | Scenario sandbox_boss_placeholder: enemy=devOnly_boss_shell, boss=devOnly_shield_phase_placeholder, affixes=6. | `BuildSimulationScenario:sandbox_boss_placeholder` |
| `Info` | `BENCHMARK_BATCH_RESULTS_PRESENT` | BuildSimulationRunner produced 4 result(s). | `BuildSimulationRunner` |
| `Info` | `BENCHMARK_COMPARISON_PRESENT` | Required comparison present: with_vs_without_synergy. | `BuildComparisonResult` |
| `Info` | `BENCHMARK_COMPARISON_PRESENT` | Required comparison present: with_vs_without_affix. | `BuildComparisonResult` |
| `Info` | `BENCHMARK_PREVIEW_INPUTS_USED` | Benchmark uses CombatModifierBundle and EffectEventBundle preview outputs. | `BuildSimulationRunner` |
| `Info` | `BENCHMARK_SOURCE_ISOLATION_SCANNED` | BuildSimulationBenchmark source scan completed for forbidden formal-system tokens. | `BuildSimulationRunner` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_basic (普通怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_basic` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_shield_guard (护盾怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_shield_guard` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_poison_cultist (毒怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_poison_cultist` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_burning_wisp (燃烧怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_burning_wisp` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_spirit_thief (偷灵怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_spirit_thief` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_seal_locker (封符怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_seal_locker` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_swarm_pack (群怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_swarm_pack` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_burst_assassin (高爆发怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_burst_assassin` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_caster_chanter (施法怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_caster_chanter` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_thick_blood (厚血怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_thick_blood` |
| `Info` | `ENEMY_PROFILE_SCANNED` | dev_enemy_formation_eye_jammer (阵眼干扰怪) devOnly=True, isEnabled=False, simulatorReadable=True. | `BuildSandboxEnemyProfile:dev_enemy_formation_eye_jammer` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 普通怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 护盾怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 毒怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 燃烧怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 偷灵怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 封符怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 群怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 高爆发怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 施法怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 厚血怪. | `BuildSandboxEnemyProfile` |
| `Info` | `REQUIRED_ENEMY_ROLE_PRESENT` | Enemy role present: 阵眼干扰怪. | `BuildSandboxEnemyProfile` |
| `Info` | `BOSS_PROFILE_SCANNED` | dev_boss_shield_jinglei (护盾Boss：验证惊雷) targets=jing_lei_break. | `BuildSandboxBossProfile:dev_boss_shield_jinglei` |
| `Info` | `BOSS_PROFILE_SCANNED` | dev_boss_swarm_lihuo (群怪Boss：验证离火) targets=li_huo_aoe. | `BuildSandboxBossProfile:dev_boss_swarm_lihuo` |
| `Info` | `BOSS_PROFILE_SCANNED` | dev_boss_burst_huzhen (高爆发Boss：验证护阵) targets=hu_zhen_survive. | `BuildSandboxBossProfile:dev_boss_burst_huzhen` |
| `Info` | `BOSS_PROFILE_SCANNED` | dev_boss_debuff_jinge (负面状态Boss：验证净厄) targets=jing_e_cleanse. | `BuildSandboxBossProfile:dev_boss_debuff_jinge` |
| `Info` | `BOSS_PROFILE_SCANNED` | dev_boss_caster_zhenhun (施法Boss：验证镇魂) targets=zhen_hun_control. | `BuildSandboxBossProfile:dev_boss_caster_zhenhun` |
| `Info` | `BOSS_PROFILE_SCANNED` | dev_boss_energy_juneng (供能干扰Boss：验证聚能) targets=ju_neng_energy. | `BuildSandboxBossProfile:dev_boss_energy_juneng` |
| `Info` | `BOSS_PROFILE_SCANNED` | dev_boss_hybrid_combo (混合机制Boss：验证组合Build) targets=combo_build. | `BuildSandboxBossProfile:dev_boss_hybrid_combo` |
| `Info` | `REQUIRED_BOSS_ROLE_PRESENT` | Boss role present: 护盾Boss. | `BuildSandboxBossProfile` |
| `Info` | `REQUIRED_BOSS_ROLE_PRESENT` | Boss role present: 群怪Boss. | `BuildSandboxBossProfile` |
| `Info` | `REQUIRED_BOSS_ROLE_PRESENT` | Boss role present: 高爆发Boss. | `BuildSandboxBossProfile` |
| `Info` | `REQUIRED_BOSS_ROLE_PRESENT` | Boss role present: 负面状态Boss. | `BuildSandboxBossProfile` |
| `Info` | `REQUIRED_BOSS_ROLE_PRESENT` | Boss role present: 施法Boss. | `BuildSandboxBossProfile` |
| `Info` | `REQUIRED_BOSS_ROLE_PRESENT` | Boss role present: 供能干扰Boss. | `BuildSandboxBossProfile` |
| `Info` | `REQUIRED_BOSS_ROLE_PRESENT` | Boss role present: 混合机制Boss. | `BuildSandboxBossProfile` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `ENEMY_BOSS_SIMULATION_READABLE` | BuildSimulationRunner read 18 enemy/Boss validation profile scenario(s). | `BuildSimulationRunner` |
| `Info` | `ENEMY_BOSS_SOURCE_ISOLATION_SCANNED` | Enemy/Boss validation pool source scan completed. | `EnemyBossValidationPool` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Thunder_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Fire_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Shield_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Cleanse_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Control_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_06. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_07. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_08. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_09. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBuildTest_Energy_10. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_01. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_02. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_03. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_04. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_REQUIRED_PRESENT` | Required profile present: DevBossTest_Mixed_05. | `DevChapterProfile` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_01 (惊雷开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_02 (惊雷开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_03 (惊雷开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_04 (惊雷开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_05 (惊雷开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_06 (惊雷开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_07 (惊雷开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_08 (惊雷开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_09 (惊雷开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Thunder_10 (惊雷开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Thunder_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_01 (离火开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_02 (离火开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_03 (离火开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_04 (离火开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_05 (离火开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_06 (离火开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_07 (离火开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_08 (离火开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_09 (离火开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Fire_10 (离火开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Fire_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_01 (护阵开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_02 (护阵开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_03 (护阵开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_04 (护阵开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_05 (护阵开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_06 (护阵开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_07 (护阵开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_08 (护阵开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_09 (护阵开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Shield_10 (护阵开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Shield_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_01 (净厄开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_02 (净厄开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_03 (净厄开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_04 (净厄开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_05 (净厄开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_06 (净厄开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_07 (净厄开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_08 (净厄开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_09 (净厄开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Cleanse_10 (净厄开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Cleanse_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_01 (镇魂开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_02 (镇魂开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_03 (镇魂开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_04 (镇魂开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_05 (镇魂开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_06 (镇魂开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_07 (镇魂开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_08 (镇魂开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_09 (镇魂开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Control_10 (镇魂开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Control_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_01 (聚能开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_02 (聚能开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_03 (聚能开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_04 (聚能开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_05 (聚能开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_05` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_06 (聚能开发测试 06) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_06` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_07 (聚能开发测试 07) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_07` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_08 (聚能开发测试 08) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_08` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_09 (聚能开发测试 09) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_09` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBuildTest_Energy_10 (聚能开发测试 10) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBuildTest_Energy_10` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_01 (混合Boss开发测试 01) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_01` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_02 (混合Boss开发测试 02) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_02` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_03 (混合Boss开发测试 03) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_03` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_04 (混合Boss开发测试 04) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_04` |
| `Info` | `DEV_CHAPTER_PROFILE_SCANNED` | DevBossTest_Mixed_05 (混合Boss开发测试 05) devOnly=True, isEnabled=False, simulatorReadable=True. | `DevChapterProfile:DevBossTest_Mixed_05` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `DEV_CHAPTER_SIMULATION_READABLE` | BuildSimulationRunner read 65 dev chapter scenario(s). | `BuildSimulationRunner` |
| `Info` | `DEV_CHAPTER_SOURCE_ISOLATION_SCANNED` | Dev chapter content pool source scan completed. | `DevChapterContentPool` |
| `Info` | `LEDGER_TASK_HOOK_SCANNED` | hookId=ledger_build_task_hooks_dev_preview, goals=8, devOnly=True, isEnabled=False. | `LedgerBuildTaskHook` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_activate_synergy_2: 激活一次 2 件羁绊, source=BuildEvaluationResult, required=2. | `LedgerBuildTaskGoal:ledger_goal_activate_synergy_2` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_activate_synergy_4: 激活一次 4 件羁绊, source=BuildEvaluationResult, required=4. | `LedgerBuildTaskGoal:ledger_goal_activate_synergy_4` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_activate_synergy_6: 激活一次 6 件羁绊, source=BuildEvaluationResult, required=6. | `LedgerBuildTaskGoal:ledger_goal_activate_synergy_6` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_thunder_defeat_shield_boss: 用惊雷 Build 击败护盾 Boss, source=BuildSimulationResult, required=1. | `LedgerBuildTaskGoal:ledger_goal_thunder_defeat_shield_boss` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_cleanse_negative_status: 用净厄 Build 清除负面状态, source=BuildSimulationResult, required=1. | `LedgerBuildTaskGoal:ledger_goal_cleanse_negative_status` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_complete_specified_build_validation: 完成一次指定 Build 验证, source=DevChapterContentPoolResult, required=1. | `LedgerBuildTaskGoal:ledger_goal_complete_specified_build_validation` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_obtain_orange_core_affix: 获得一个橙色核心词条, source=AffixPreview, required=1. | `LedgerBuildTaskGoal:ledger_goal_obtain_orange_core_affix` |
| `Info` | `LEDGER_TASK_GOAL_SCANNED` | ledger_goal_complete_affix_reroll: 完成一次词条洗练, source=AffixPreview, required=1. | `LedgerBuildTaskGoal:ledger_goal_complete_affix_reroll` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_activate_synergy_2. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_activate_synergy_4. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_activate_synergy_6. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_thunder_defeat_shield_boss. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_cleanse_negative_status. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_complete_specified_build_validation. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_obtain_orange_core_affix. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_GOAL_REQUIRED_PRESENT` | Required task goal present: ledger_goal_complete_affix_reroll. | `LedgerBuildTaskGoal` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: BuildEvaluationResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: BuildSimulationResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: AffixPreview. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: EnemyBossValidationPoolResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_EVENT_SOURCE_PRESENT` | Event source preview present: DevChapterContentPoolResult. | `LedgerBuildTaskEvent` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_activate_synergy_2 observed=6, required=2, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_activate_synergy_2` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_activate_synergy_4 observed=6, required=4, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_activate_synergy_4` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_activate_synergy_6 observed=6, required=6, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_activate_synergy_6` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_thunder_defeat_shield_boss observed=5, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_thunder_defeat_shield_boss` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_cleanse_negative_status observed=15, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_cleanse_negative_status` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_complete_specified_build_validation observed=5, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_complete_specified_build_validation` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_obtain_orange_core_affix observed=1, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_obtain_orange_core_affix` |
| `Info` | `LEDGER_TASK_PROGRESS_MATCHED` | ledger_goal_complete_affix_reroll observed=5, required=1, completed=True. | `LedgerBuildTaskProgressPreview:ledger_goal_complete_affix_reroll` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `LEDGER_TASK_SOURCE_ISOLATION_SCANNED` | Ledger task hook source scan completed. | `LedgerBuildTaskHook` |
| `Info` | `FINAL_FEATURE_FLAGS_ALL_FALSE` | All BuildSandbox FeatureFlags default false. | `BuildSandboxFeatureFlags` |
| `Info` | `FINAL_DEVONLY_ISOLATION_PASS` | BuildSandbox configs isolated. scanned=25. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `FINAL_OCCUPIED_CELLS_PRESENT` | ItemShapeOccupancy output placedItems=5. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `FINAL_SYNERGY_EVALUATION_PASS` | activeSynergies=3, activeThresholds=6. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `FINAL_AFFIX_PREVIEW_PASS` | affixItems=5, affixes=6. | `Assets/_Game/Configs/BuildSandbox` |
| `Info` | `FINAL_MODIFIER_EVENT_PASS` | modifiers=33, events=36. | `ModifierEventBridge` |
| `Info` | `FINAL_BENCHMARK_PASS` | benchmark=4, enemyBoss=18, devChapter=65. | `BuildSimulationRunner` |
| `Info` | `FINAL_LEDGER_HOOK_PASS` | goals=8, events=123, progress=8. | `LedgerBuildTaskHook` |
| `Info` | `FINAL_FORMAL_FLOW_LEAK_PASS` | Formal flow leak counters are all zero. | `BuildSandbox Final Leak Check` |
| `Info` | `FINAL_RUNTIME_SOURCE_SCAN_PASS` | Runtime BuildSandbox source scan found no forbidden formal-system tokens outside comments and strings. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox` |

## Forbidden Scope Check

- Formal combat connection: not touched.
- Current UI scene / RectTransform / hand-tuned layout: not touched.
- RunFlow / PageState / FormationState / V02RunFlowController / DamageText: not touched.
- SaveData / PlayerPrefs / MainTrialProgressData: not touched.
- Boss / rewards / drops / numeric configs: not touched.
- AffixRaritySandbox and ModifierEventBridge: not implemented in this package.

## Next Package Gate

`V0.3/V0.4-BuildSandbox-AffixRaritySandbox01` can enter only after compile passes and the user confirms this report.
