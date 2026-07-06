# BattleSandbox Playable Full Roster Regression Report

Package: `V0.4-BattleSandboxPlayableFullRosterRegression01`
Generated: `2026-07-04 22:17:27`
Status: `PASS`
Errors: `0`
Warnings: `0`

## Scope

- Scene: `Scene_TalismanBag_V04_BattleSandboxPreview` only.
- Purpose: devOnly playable regression for the current 23 item roster.
- Allowed surface: BuildSandbox devOnly runtime, validator, and report evidence.
- No V0.2/V0.3 formal assets, formal itemId replacement, formal RunFlow, SaveData, Reward, Chapter, or V04 RectTransform layout rewrite.
- No scene binder is required or run by this report.

## Regression Summary

| Requirement | Result | Evidence |
| --- | --- | --- |
| 23 items appear in V04 tray | `PASS` | roster=23; tray=23; packed=23; missing=0; packFailures=0 |
| Basic items are 1x1 and placeable | `PASS` | basic=13; single1=13; placed=13 |
| x2/x3/x4/vertical_3 placement | `PASS` | x2=3; x3=3; x4=2; vertical_3=1; failures=0 |
| Empty board defeat | `PASS` | enemy=132->132; player=100->0; hpRows=2; defeatRows=1 |
| Attack item damages enemy HP | `PASS` | candidates=8; damagingSingleRows=3; totalDamage=53 |
| Shield before HP | `PASS` | attackRows=3; shieldFirstRows=1 |
| Support does not damage enemy HP | `PASS` | supportCandidates=15; damageLeaks=0 |
| Victory/defeat/restart | `PASS` | victoryRows=1; defeatRows=1 |
| Switch target then play again | `PASS` | scenarios=4; playableSwitchPreviews=2 |
| Player-side answer leak clear | `PASS` | playerLeaks=0 |
| Formal scope clear | `PASS` | formalLeaks=0; featureFlagDefaultTrue=0; devOnlyFalse=0; isEnabledTrue=0 |

## Item Rows

| itemId | shapeId | cells | tray | board | attack | supportNoDamage | enemyHpDamage | enemyHp | result |
| --- | --- | ---: | --- | --- | --- | --- | ---: | --- | --- |
| `preview_stone_core` | `Square4` | 4 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `preview_soul_seal` | `Square4` | 4 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `preview_taomu_sword` | `vertical_3` | 3 | `PASS` | `PASS` | `True` | `False` | 14 | 138->124 | `PASS` |
| `preview_cleanse_corner` | `Corner3` | 3 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `preview_old_bell` | `Corner3` | 3 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `preview_x2_wood_talisman` | `Vertical2` | 2 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `preview_guard_wood` | `Vertical2` | 2 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `preview_energy_incense` | `Vertical2` | 2 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `preview_fire_talisman` | `Single1` | 1 | `PASS` | `PASS` | `True` | `False` | 8 | 138->130 | `PASS` |
| `preview_thunder_sword` | `Single1` | 1 | `PASS` | `PASS` | `True` | `False` | 11 | 138->127 | `PASS` |
| `fire_talisman_basic` | `Single1` | 1 | `PASS` | `PASS` | `True` | `False` | 0 | 138->138 | `PASS` |
| `thunder_talisman_basic` | `Single1` | 1 | `PASS` | `PASS` | `True` | `False` | 0 | 138->138 | `PASS` |
| `shield_talisman_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `qi_pill_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `spirit_stone_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `sword_pill_basic` | `Single1` | 1 | `PASS` | `PASS` | `True` | `False` | 0 | 138->138 | `PASS` |
| `chain_thunder_talisman_basic` | `Single1` | 1 | `PASS` | `PASS` | `True` | `False` | 0 | 138->138 | `PASS` |
| `purify_talisman_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `soul_suppress_talisman_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `seal_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `water_talisman_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |
| `exorcism_bell_basic` | `Single1` | 1 | `PASS` | `PASS` | `True` | `False` | 0 | 138->138 | `PASS` |
| `peach_wood_basic` | `Single1` | 1 | `PASS` | `PASS` | `False` | `True` | 0 | 138->138 | `PASS` |

## User Handtest Checklist

1. Open `Scene_TalismanBag_V04_BattleSandboxPreview`, press Play, and confirm the tray can expose all 23 items via the available tray/category controls.
2. Drag several basic 1x1 items to legal cells, then test x2, x3, x4, and vertical_3 items.
3. Start battle with an empty board and confirm enemy HP stays unchanged while player HP reaches defeat.
4. Place an obvious attack item, start battle, and confirm enemy HP drops.
5. Place a shield item, start battle, and confirm shield is consumed before HP.
6. Place heal/cleanse/control/aura/rhythm support items and confirm they do not damage enemy HP.
7. Confirm victory, failure, restart, and switching target allow another round.
8. Confirm player-facing text does not reveal full answers, DropBias weights, or Boss six-key answers.
9. Confirm no SaveData, Reward, Chapter, or formal RunFlow side effects appear.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Config Validation | `PASS` | 0 | 0 | 211 |
| devOnly Isolation | `PASS` | 0 | 0 | 25 |
| UI Layout Guard | `PASS` | 0 | 0 | 29 |
| CoreFlow Smoke Placeholder | `PASS` | 0 | 0 | 1 |
| ItemShape Occupancy | `PASS` | 0 | 0 | 9 |
| BattleSandbox Preview Scene 01 | `PASS` | 0 | 0 | 45 |
| BuildSandbox Preview Context 01 | `PASS` | 0 | 0 | 26 |
| Battle Page View Adapter 01 | `PASS` | 0 | 0 | 14 |
| BuildGrid Interaction Preview Scene Binding | `PASS` | 0 | 0 | 13 |
| BuildGrid Interaction ItemInfoPanel Samples | `PASS` | 0 | 0 | 24 |
| BuildGrid Interaction Preview Placement Samples | `PASS` | 0 | 0 | 8 |
| UI Layout Guard | `PASS` | 0 | 0 | 29 |
| BattleSandbox Combat Kernel Adapter 01 | `PASS` | 0 | 0 | 52 |
| BattleSandbox Build Combat Preview 01 | `PASS` | 0 | 0 | 34 |
| BattleSandbox ManaLoop Runtime 01 | `PASS` | 0 | 0 | 1 |
| BattleSandbox Runtime Loop 01 | `PASS` | 0 | 0 | 66 |
| BattleSandbox Playable Full Roster Regression 01 | `PASS` | 0 | 0 | 14 |

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
| `Info` | `BATTLE_SANDBOX_PREVIEW_SCENE_EXISTS` | Preview scene file exists and opened. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_ROOT_PRESENT` | EventSystem | `EventSystem` |
| `Info` | `BATTLE_SANDBOX_ROOT_PRESENT` | BuildSandboxPreviewRoot | `BuildSandboxPreviewRoot` |
| `Info` | `BATTLE_SANDBOX_ROOT_PRESENT` | BuildSandboxPreviewCanvas | `BuildSandboxPreviewCanvas` |
| `Info` | `BATTLE_SANDBOX_MARKER_ISOLATED` | Marker is devOnly, disabled, and not connected. | `BuildSandboxPreviewRoot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/V04BattlePrepareDarkOverlay | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/V04BattlePrepareDarkOverlay` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea/BoardGridPreview | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea/BoardGridPreview` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea/ItemTrayPreview | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea/ItemTrayPreview` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea/EnemyCombatFeedbackPanel | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea/EnemyCombatFeedbackPanel` |
| `Info` | `BATTLE_SANDBOX_PLACEMENT_FEEDBACK_RUNTIME_FALLBACK` | Placement feedback is created at runtime when the hand-tuned scene does not serialize the legacy slot. | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/MapRuleDropdownSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/MapRuleDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyProblemDropdownSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyProblemDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/BossProblemDropdownSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/BossProblemDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/DevChapterDropdownSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/DevChapterDropdownSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyCombatFeedbackControlPanel | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/ProblemSelectorPanel/EnemyCombatFeedbackControlPanel` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/BuildSummaryPanelSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/BuildSummaryPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/SynergyPanelSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/SynergyPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/ShapeOccupancyPanelSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/ShapeOccupancyPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/AffixModifierPanelSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/AffixModifierPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/ProblemReadinessPanelSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/ProblemReadinessPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/SimulationResultPanelSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/SimulationResultPanelSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyCombatFeedbackDeveloperPanel | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BuildSandboxDataPanelDock/EnemyCombatFeedbackDeveloperPanel` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/EnemyCombatFeedbackFloatingRoot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/EnemyCombatFeedbackFloatingRoot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar/RunSimulationButtonSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar/RunSimulationButtonSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar/ResetPreviewButtonSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar/ResetPreviewButtonSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar/ExportReportButtonSlot | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/DevOnlyControlBar/ExportReportButtonSlot` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions/V04BattlePrepareBackButton | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions/V04BattlePrepareBackButton` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions/V04BattlePrepareStateButton | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions/V04BattlePrepareStateButton` |
| `Info` | `BATTLE_SANDBOX_CHILD_PRESENT` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions/V04BattlePrepareToggleButton | `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/V04BattlePrepareBottomActions/V04BattlePrepareToggleButton` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableSynergyBuild=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableAffixSystem=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableDevBuildContent=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableBuildModifierInCombat=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableBuildDebugReport=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableItemShapeOccupancy=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableShapePlacementSandbox=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_FEATURE_FLAG_FALSE` | EnableShapeRotation=false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_BUILDSETTINGS_UNTOUCHED` | Preview scene is not present in Build Settings. | `ProjectSettings/EditorBuildSettings.asset` |
| `Info` | `PREVIEW_CONTEXT_ISOLATION_PASS` | Context is devOnly, disabled, and not connected to formal flow/data/scene surfaces. | `BuildSandboxPreviewContext` |
| `Info` | `PREVIEW_VIEWMODEL_SECTION_COUNT_PASS` | Preview ViewModel output sections=7. | `BuildSandboxPreviewViewModel` |
| `Info` | `PREVIEW_SUMMARY_MAP_RULE_COUNT` | MapRule count pass. actual=10, expected>=10. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SUMMARY_ENEMY_PROBLEM_COUNT` | EnemyProblem count pass. actual=10, expected>=10. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SUMMARY_BOSS_PROBLEM_COUNT` | BossProblem count pass. actual=6, expected>=6. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SUMMARY_PLACED_ITEM_COUNT` | Placed item count pass. actual=8, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SUMMARY_OCCUPIED_CELL_COUNT` | Occupied cell count pass. actual=8, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SUMMARY_TAG_COUNT` | Capability token count pass. actual=74, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SYNERGY_ACTIVE_COUNT` | Active synergy count pass. actual=3, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SYNERGY_THRESHOLD_COUNT` | Active threshold count pass. actual=6, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SHAPE_PLACED_ITEM_COUNT` | Shape placed item count pass. actual=8, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SHAPE_OCCUPIED_CELL_COUNT` | Shape occupied cell count pass. actual=8, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SHAPE_VALID_SAMPLE_COUNT` | Valid shape sample count pass. actual=5, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SHAPE_INVALID_SAMPLES_PRESENT` | Invalid placement samples are exposed for QA readability: 4. | `ShapeOccupancyViewModel` |
| `Info` | `PREVIEW_SELECTOR_MAP_RULE_COUNT` | MapRule selector count pass. actual=10, expected>=10. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SELECTOR_ENEMY_PROBLEM_COUNT` | EnemyProblem selector count pass. actual=10, expected>=10. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SELECTOR_BOSS_PROBLEM_COUNT` | BossProblem selector count pass. actual=6, expected>=6. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_READINESS_BOSS_COUNT` | Boss readiness row count pass. actual=6, expected>=6. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_READINESS_KEY_COUNT` | Boss readiness key count pass. actual=25, expected>=18. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_READINESS_DROP_BIAS_COUNT` | DropBias count pass. actual=18, expected>=12. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_AFFIX_ITEM_COUNT` | Affix item count pass. actual=5, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_AFFIX_SELECTED_COUNT` | Selected affix count pass. actual=6, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_MODIFIER_COUNT` | Modifier preview count pass. actual=33, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_EVENT_COUNT` | Event preview count pass. actual=36, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SIMULATION_SCENARIO_COUNT` | Simulation scenario count pass. actual=4, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `PREVIEW_SIMULATION_RESULT_COUNT` | Simulation result count pass. actual=4, expected>=1. | `V0.4-BuildSandboxPreviewContext01` |
| `Info` | `BATTLE_PAGE_ADAPTER_ISOLATION_PASS` | Adapter is devOnly, disabled, read-only, and disconnected from formal UI/battle/save surfaces. | `BattlePageViewAdapter` |
| `Info` | `BATTLE_PAGE_SPEC_SECTION_COUNT_PASS` | Adapter spec section count=10. | `BattlePageViewSpec` |
| `Info` | `BATTLE_PAGE_BOARD_SIZE` | Board size pass. actual=800. | `BattleGridVisualSpec` |
| `Info` | `BATTLE_PAGE_BOARD_HEIGHT` | Board height pass. actual=800. | `BattleGridVisualSpec` |
| `Info` | `BATTLE_PAGE_ITEM_TRAY_SIZE` | Item tray width pass. actual=800. | `ItemTrayVisualSpec` |
| `Info` | `BATTLE_PAGE_ITEM_TRAY_HEIGHT` | Item tray height pass. actual=800. | `ItemTrayVisualSpec` |
| `Info` | `BATTLE_PAGE_TRAY_COLUMNS` | Item tray columns pass. actual=5, expected>=5. | `V0.4-BattlePageViewAdapter01` |
| `Info` | `BATTLE_PAGE_TRAY_ROWS` | Item tray rows pass. actual=8, expected>=8. | `V0.4-BattlePageViewAdapter01` |
| `Info` | `BATTLE_PAGE_TRAY_SLOT_COUNT` | Item tray slots pass. actual=40, expected>=40. | `V0.4-BattlePageViewAdapter01` |
| `Info` | `BATTLE_PAGE_CATEGORY_COUNT` | Category tabs pass. actual=6, expected>=6. | `V0.4-BattlePageViewAdapter01` |
| `Info` | `BATTLE_UI_REUSE_PASS` | UI reuse channels present=5. | `BattleUiReuseSpec` |
| `Info` | `DEVELOPER_TUNING_FIELD_COUNT` | Developer tuning fields pass. actual=8, expected>=8. | `V0.4-BattlePageViewAdapter01` |
| `Info` | `PLAYER_HINT_MASKING_PASS` | Player hint masking rules present=6. | `PlayerHintMaskingSpec` |
| `Info` | `BATTLE_PAGE_FORMAL_REFERENCE_READONLY` | Formal battle page names are recorded as strings for read-only specification reports only. | `BattlePageViewSpec` |
| `Info` | `BUILD_GRID_PREVIEW_SCENE_BOUND` | Grid interaction controller is bound. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BUILD_GRID_BOARD_PRESENT` | BoardGridPreview exists. | `BoardGridPreview` |
| `Info` | `BUILD_GRID_TRAY_PRESENT` | ItemTrayPreview exists. | `ItemTrayPreview` |
| `Info` | `BUILD_GRID_INFO_PRESENT` | SelectedItemInfo exists. | `SelectedItemInfo` |
| `Info` | `BUILD_GRID_FEEDBACK_PRESENT` | PlacementFeedback exists. | `PlacementFeedback` |
| `Info` | `BUILD_GRID_BOARD_SLOT_COUNT` | Board slot count pass. actual=25, expected>=25. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_TRAY_SLOT_COUNT` | Tray slot count pass. actual=40, expected>=40. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_CATEGORY_COUNT` | Category filter count pass. actual=6, expected>=6. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_SHAPE_COUNT` | Shape count pass. actual=5, expected>=4. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_CONTROLLER_ISOLATION_PASS` | Controller is devOnly and isolated from formal flow/save/UI/scene surfaces. | `BuildGridInteractionPreviewController` |
| `Info` | `BUILD_GRID_BUILDSETTINGS_UNTOUCHED` | V04 Preview Scene is not present in Build Settings. | `ProjectSettings/EditorBuildSettings.asset` |
| `Info` | `BUILD_GRID_PLAYER_TEXT_CHINESE_ONLY` | Player-facing preview text is Chinese-only. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BUILD_GRID_COMPLETE_ANSWERS_HIDDEN` | Player-facing preview text does not expose complete answers. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BUILD_GRID_ITEM_INFO_CLOSE_PRESENT` | Sandbox ItemInfoPanel uses a non-Latin close mark. | `BuildSandboxItemInfoPanel` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 炉芯石. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 镇魂法印. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 桃木剑. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 净化折符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 镇邪铃. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 护阵木牌. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 守护木牌. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 醒符香. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 炽火符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 雷引剑符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 火符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 雷符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 护身符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 丹药. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 聚灵石. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 剑丸. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 连锁雷符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 净化符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 镇魂符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 法印. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 水符. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 驱邪铃. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_ITEM_INFO_SAMPLE` | ItemInfoPanel sample passes structure and masking checks: 桃木牌. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_SAMPLE_LEGAL` | Legal placement sample passes. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_SAMPLE_ROTATION` | Rotation sample passes. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_SAMPLE_OUT_OF_GRID` | Out-of-grid sample is detected. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_SAMPLE_OVERLAP` | Overlap sample is detected. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_PLACEMENT_SAMPLE` | legal_place: valid=True, reason=None, feedback=可以放置，占用 1 格。 | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_PLACEMENT_SAMPLE` | rotation_place: valid=True, reason=None, feedback=可以放置，占用 2 格。 | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_PLACEMENT_SAMPLE` | out_of_grid: valid=False, reason=OutOfGrid, feedback=越界：有格子超出棋盘。 | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_PLACEMENT_SAMPLE` | overlap: valid=False, reason=CellOccupied, feedback=重叠：目标格子已被占用。 | `V0.4-BuildGridInteractionPreview01` |
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
