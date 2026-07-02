# BattleSandbox Preview Scene Leak Check Report

Package: `V0.4-BattleSandboxPreviewScene01`
Generated: `2026-07-01 11:01:58`
Status: `PASS`

## Leak Counters

| Check | Count | Expected |
| --- | ---: | ---: |
| `featureFlagDefaultTrue` | 0 | 0 |
| `previewMarkerIsolationLeaks` | 0 | 0 |
| `previewSceneInBuildSettings` | 0 | 0 |

## Formal Scope Confirmation

- Product V02/V03 scenes: not written by this package.
- ProjectSettings/EditorBuildSettings.asset: not written by this package.
- V02RunFlowController, V02FormationGridFrame, DamageText, RunFlow, PageState, FormationState: not modified by this package.
- SaveData, PlayerPrefs, MainTrialProgressData: not modified by this package.
- Reward, drop, Boss, enemy, upgrade, and numeric configs: not modified by this package.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Config Validation | `PASS` | 0 | 0 | 211 |
| devOnly Isolation | `PASS` | 0 | 0 | 25 |
| UI Layout Guard | `PASS` | 0 | 0 | 4 |
| CoreFlow Smoke Placeholder | `PASS` | 0 | 0 | 1 |
| Build Problem Rule Pool | `PASS` | 0 | 0 | 18 |
| Build Problem Seed Data | `PASS` | 0 | 0 | 68 |
| BuildSandbox Config Panel 01 | `PASS` | 0 | 0 | 31 |
| BattleSandbox Preview Scene 01 | `PASS` | 0 | 0 | 37 |

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
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | BattleSandboxPreviewScene01 is allowed to author an independent devOnly preview scene skeleton. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneBuilder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | BattleSandboxPreviewScene01 is allowed to author an independent devOnly preview scene skeleton. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneReportWriter.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | BattleSandboxPreviewScene01 is allowed to author an independent devOnly preview scene skeleton. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneVerifier.cs` |
| `Info` | `UI_LAYOUT_GUARD_PASS` | BuildSandbox code scan found no scene access, formal UI creation, or layout write tokens. | `` |
| `Info` | `CORE_FLOW_PLACEHOLDER_ONLY` | Placeholder smoke entry is isolated. It does not load scenes, mutate saves, or enter product flow. | `` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BUILD_PROBLEM_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `RULE_CONFIG_DEFAULT_ISOLATED` | MapRuleConfig defaults devOnly=true and isEnabled=false. | `MapRuleConfig` |
| `Info` | `RULE_CONFIG_DEFAULT_ISOLATED` | EnemyProblemConfig defaults devOnly=true and isEnabled=false. | `EnemyProblemConfig` |
| `Info` | `RULE_CONFIG_DEFAULT_ISOLATED` | BossProblemConfig defaults devOnly=true and isEnabled=false. | `BossProblemConfig` |
| `Info` | `RULE_CONFIG_DEFAULT_ISOLATED` | BuildReadinessCheckConfig defaults devOnly=true and isEnabled=false. | `BuildReadinessCheckConfig` |
| `Info` | `RULE_CONFIG_DEFAULT_ISOLATED` | WeaknessWindowConfig defaults devOnly=true and isEnabled=false. | `WeaknessWindowConfig` |
| `Info` | `RULE_CONFIG_DEFAULT_ISOLATED` | DropBiasConfig defaults devOnly=true and isEnabled=false. | `DropBiasConfig` |
| `Info` | `RULE_CONFIG_DEFAULT_ISOLATED` | FailureHintConfig defaults devOnly=true and isEnabled=false. | `FailureHintConfig` |
| `Info` | `BOSS_KEY_SCHEMA_PRESENT` | BossProblemKeyRequirement schema exists for later multi-key Boss problem seeds. | `BossProblemKeyRequirement` |
| `Info` | `BUILD_PROBLEM_SOURCE_ISOLATION_SCANNED` | Build problem rule source isolation scan completed. | `BuildProblemRulePoolValidator` |
| `Info` | `NEXT_SEEDDATA_COUNTS` | Next package should add 10 map rule seed(s), 10 enemy problem seed(s), and 6 boss problem seed(s). | `Docs/V0.4/BuildProblemRulePool01_Assignment.md` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 黑炉烟尘. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 青石回潮. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 灯火不足. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 铜铃夜响. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 阵眼偏移. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 符纸受潮. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 旧巷回音. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 炉灰落阵. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 夜巡灯灭. | `MapRuleSeed` |
| `Info` | `MAP_RULE_REQUIRED_MISSING_PRESENT` | Required seed present: 青石裂纹. | `MapRuleSeed` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 黑炉烟尘 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_black_furnace_smoke` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 青石回潮 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_bluestone_damp` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 灯火不足 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_lamp_shortage` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 铜铃夜响 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_copper_bell_night` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 阵眼偏移 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_formation_eye_shift` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 符纸受潮 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_talisman_paper_damp` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 旧巷回音 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_old_lane_echo` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 炉灰落阵 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_furnace_ash_fall` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 夜巡灯灭 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_night_watch_lamp_out` |
| `Info` | `MAP_RULE_SEED_SCANNED` | 青石裂纹 devOnly=True, isEnabled=False. | `MapRuleSeed:dev_map_bluestone_crack` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 护盾. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 群怪. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 毒 / 燃. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 偷灵. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 封符. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 高爆发. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 施法. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 厚血. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 污染格. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 阵眼干扰. | `EnemyProblemSeed` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 护盾 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_shield_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 群怪 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_swarm_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 毒 / 燃 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_poison_burn_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 偷灵 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_spirit_thief_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 封符 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_seal_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 高爆发 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_burst_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 施法 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_caster_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 厚血 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_thick_blood_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 污染格 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_polluted_tile_problem` |
| `Info` | `ENEMY_PROBLEM_SEED_SCANNED` | 阵眼干扰 devOnly=True, isEnabled=False. | `EnemyProblemSeed:dev_enemy_formation_eye_problem` |
| `Info` | `BOSS_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 黑炉护壳师. | `BossProblemSeed` |
| `Info` | `BOSS_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 秽签梦母. | `BossProblemSeed` |
| `Info` | `BOSS_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 偷灵炉心. | `BossProblemSeed` |
| `Info` | `BOSS_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 叩阵铜将. | `BossProblemSeed` |
| `Info` | `BOSS_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 纸人千面阵. | `BossProblemSeed` |
| `Info` | `BOSS_PROBLEM_REQUIRED_MISSING_PRESENT` | Required seed present: 黑炉复合阵眼. | `BossProblemSeed` |
| `Info` | `BOSS_PROBLEM_SEED_SCANNED` | 黑炉护壳师 keys=4, devOnly=True, isEnabled=False. | `BossProblemSeed:dev_boss_black_furnace_shell` |
| `Info` | `BOSS_PROBLEM_SEED_SCANNED` | 秽签梦母 keys=4, devOnly=True, isEnabled=False. | `BossProblemSeed:dev_boss_dirty_dream_mother` |
| `Info` | `BOSS_PROBLEM_SEED_SCANNED` | 偷灵炉心 keys=4, devOnly=True, isEnabled=False. | `BossProblemSeed:dev_boss_spirit_thief_core` |
| `Info` | `BOSS_PROBLEM_SEED_SCANNED` | 叩阵铜将 keys=4, devOnly=True, isEnabled=False. | `BossProblemSeed:dev_boss_bronze_formation_general` |
| `Info` | `BOSS_PROBLEM_SEED_SCANNED` | 纸人千面阵 keys=4, devOnly=True, isEnabled=False. | `BossProblemSeed:dev_boss_paper_faces` |
| `Info` | `BOSS_PROBLEM_SEED_SCANNED` | 黑炉复合阵眼 keys=5, devOnly=True, isEnabled=False. | `BossProblemSeed:dev_boss_black_furnace_complex_eye` |
| `Info` | `PROBLEM_ATTRIBUTE_COVERED` | Problem attribute covered: BreakPower. | `BossProblemSeed` |
| `Info` | `PROBLEM_ATTRIBUTE_COVERED` | Problem attribute covered: CleansePower. | `BossProblemSeed` |
| `Info` | `PROBLEM_ATTRIBUTE_COVERED` | Problem attribute covered: ControlPower. | `BossProblemSeed` |
| `Info` | `PROBLEM_ATTRIBUTE_COVERED` | Problem attribute covered: GuardPower. | `BossProblemSeed` |
| `Info` | `PROBLEM_ATTRIBUTE_COVERED` | Problem attribute covered: EnergyStability. | `BossProblemSeed` |
| `Info` | `PROBLEM_ATTRIBUTE_COVERED` | Problem attribute covered: ClearPower. | `BossProblemSeed` |
| `Info` | `PROBLEM_ATTRIBUTE_COVERED` | Problem attribute covered: BurstWindow. | `BossProblemSeed` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `SEEDDATA_SOURCE_ISOLATION_SCANNED` | Build problem seed source scan completed. | `BuildProblemSeedDataValidator` |
| `Info` | `CONFIG_PANEL_MENU_PATH_PRESENT` | Tools/Talisman Bag/V0.4/BuildSandbox/Data/[Manual Only] BuildSandbox Config Panel 01 | `BuildSandboxConfigPanelWindow` |
| `Info` | `CONFIG_PANEL_TAB_COUNT_PASS` | Panel tabs=10. | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | Overview | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | MapRule | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | EnemyProblem | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | BossProblem | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | WeaknessWindow | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | DropBias | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | BuildReadiness | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_REQUIRED_TAB_PRESENT` | Validation | `BuildSandboxConfigPanelTab` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `CONFIG_PANEL_MAP_RULE_COUNT` | MapRule count pass. actual=10, expected>=10. | `MapRule` |
| `Info` | `CONFIG_PANEL_ENEMY_PROBLEM_COUNT` | EnemyProblem count pass. actual=10, expected>=10. | `EnemyProblem` |
| `Info` | `CONFIG_PANEL_BOSS_PROBLEM_COUNT` | BossProblem count pass. actual=6, expected>=6. | `BossProblem` |
| `Info` | `CONFIG_PANEL_WEAKNESS_WINDOW_COUNT` | WeaknessWindow count pass. actual=6, expected>=6. | `WeaknessWindow` |
| `Info` | `CONFIG_PANEL_DROP_BIAS_COUNT` | DropBias count pass. actual=18, expected>=18. | `DropBias` |
| `Info` | `CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED` | BreakPower | `BuildReadiness` |
| `Info` | `CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED` | CleansePower | `BuildReadiness` |
| `Info` | `CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED` | ControlPower | `BuildReadiness` |
| `Info` | `CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED` | GuardPower | `BuildReadiness` |
| `Info` | `CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED` | EnergyStability | `BuildReadiness` |
| `Info` | `CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED` | ClearPower | `BuildReadiness` |
| `Info` | `CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED` | BurstWindow | `BuildReadiness` |
| `Info` | `CONFIG_PANEL_SOURCE_ISOLATION_SCANNED` | BuildSandbox Config Panel source isolation scan completed. | `BuildSandboxConfigPanelValidator` |
| `Info` | `BATTLE_SANDBOX_PREVIEW_SCENE_EXISTS` | Preview scene file exists and opened. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_ROOT_PRESENT` | EventSystem | `EventSystem` |
| `Info` | `BATTLE_SANDBOX_ROOT_PRESENT` | BuildSandboxPreviewRoot | `BuildSandboxPreviewRoot` |
| `Info` | `BATTLE_SANDBOX_ROOT_PRESENT` | BuildSandboxPreviewCanvas | `BuildSandboxPreviewCanvas` |
| `Info` | `BATTLE_SANDBOX_MARKER_ISOLATED` | Marker is devOnly, disabled, and not connected. | `BuildSandboxPreviewRoot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot | `BuildSandboxPreviewCanvas/SafeAreaRoot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea | `BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel | `BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock | `BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar | `BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/PopupLayer | `BuildSandboxPreviewCanvas/SafeAreaRoot/PopupLayer` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/BoardGridPreview | `BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/BoardGridPreview` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/ItemTrayPreview | `BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/ItemTrayPreview` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/SelectedItemInfo | `BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/SelectedItemInfo` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/PlacementFeedback | `BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/PlacementFeedback` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/MapRuleDropdownSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/MapRuleDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/EnemyProblemDropdownSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/EnemyProblemDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/BossProblemDropdownSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/BossProblemDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/DevChapterDropdownSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/ProblemSelectorPanel/DevChapterDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/BuildSummaryPanelSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/BuildSummaryPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/SynergyPanelSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/SynergyPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/ShapeOccupancyPanelSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/ShapeOccupancyPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/AffixModifierPanelSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/AffixModifierPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/ProblemReadinessPanelSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/ProblemReadinessPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/SimulationResultPanelSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/BuildSandboxDataPanelDock/SimulationResultPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar/RunSimulationButtonSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar/RunSimulationButtonSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar/ResetPreviewButtonSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar/ResetPreviewButtonSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar/ExportReportButtonSlot | `BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar/ExportReportButtonSlot` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_BUILDSETTINGS_UNTOUCHED` | Preview scene is not present in Build Settings. | `ProjectSettings/EditorBuildSettings.asset` |
