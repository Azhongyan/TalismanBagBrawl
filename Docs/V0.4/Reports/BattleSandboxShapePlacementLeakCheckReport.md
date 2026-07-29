# Battle Sandbox Shape Placement Leak Check Report

Package: `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01`
Generated: `2026-07-21 15:26:21`
Status: `BLOCKED_LEAK_CHECK`
Errors: `14`
Warnings: `0`
Leak Count: `1`

## Leak Rows

| Check | Leak | Asset | Detail |
| --- | --- | --- | --- |
| `feature_flags_default_disabled` | `False` | `BuildSandboxFeatureFlags` | All BuildSandbox feature flags default false. |
| `preview_scene_not_in_build_settings` | `False` | `ProjectSettings/EditorBuildSettings.asset` | V04 preview scene is not present in Build Settings. |
| `controller_formal_surface_flags` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Controller formal surface flags remain false. |
| `runtime_forbidden_token_V03BattlePrepareInteractionController` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `V03BattlePrepareInteractionController` absent. |
| `runtime_forbidden_token_V02RunFlowController` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `V02RunFlowController` absent. |
| `runtime_forbidden_token_V02FormationGridFrame` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `V02FormationGridFrame` absent. |
| `runtime_forbidden_token_MainTrialProgressData` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `MainTrialProgressData` absent. |
| `runtime_forbidden_token_PlayerPrefs` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `PlayerPrefs` absent. |
| `runtime_forbidden_token_RunFlow` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `RunFlow` absent. |
| `runtime_forbidden_token_PageState` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `PageState` absent. |
| `runtime_forbidden_token_FormationState` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `FormationState` absent. |
| `runtime_forbidden_token_BossHand` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `BossHand` absent. |
| `runtime_forbidden_token_BossReward` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `BossReward` absent. |
| `runtime_forbidden_token_RewardDrop` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `RewardDrop` absent. |
| `runtime_forbidden_token_DropBias` | `True` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Found forbidden runtime token `DropBias`. |
| `runtime_forbidden_token_Scene_TalismanBag_V02` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `Scene_TalismanBag_V02` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V03` | `False` | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | Forbidden runtime token `Scene_TalismanBag_V03` absent. |
| `runtime_forbidden_token_V03BattlePrepareInteractionController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `V03BattlePrepareInteractionController` absent. |
| `runtime_forbidden_token_V02RunFlowController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `V02RunFlowController` absent. |
| `runtime_forbidden_token_V02FormationGridFrame` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `V02FormationGridFrame` absent. |
| `runtime_forbidden_token_MainTrialProgressData` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `MainTrialProgressData` absent. |
| `runtime_forbidden_token_PlayerPrefs` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `PlayerPrefs` absent. |
| `runtime_forbidden_token_RunFlow` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `RunFlow` absent. |
| `runtime_forbidden_token_PageState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `PageState` absent. |
| `runtime_forbidden_token_FormationState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `FormationState` absent. |
| `runtime_forbidden_token_BossHand` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `BossHand` absent. |
| `runtime_forbidden_token_BossReward` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `BossReward` absent. |
| `runtime_forbidden_token_RewardDrop` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `RewardDrop` absent. |
| `runtime_forbidden_token_DropBias` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `DropBias` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V02` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `Scene_TalismanBag_V02` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V03` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | Forbidden runtime token `Scene_TalismanBag_V03` absent. |
| `runtime_forbidden_token_V03BattlePrepareInteractionController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `V03BattlePrepareInteractionController` absent. |
| `runtime_forbidden_token_V02RunFlowController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `V02RunFlowController` absent. |
| `runtime_forbidden_token_V02FormationGridFrame` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `V02FormationGridFrame` absent. |
| `runtime_forbidden_token_MainTrialProgressData` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `MainTrialProgressData` absent. |
| `runtime_forbidden_token_PlayerPrefs` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `PlayerPrefs` absent. |
| `runtime_forbidden_token_RunFlow` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `RunFlow` absent. |
| `runtime_forbidden_token_PageState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `PageState` absent. |
| `runtime_forbidden_token_FormationState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `FormationState` absent. |
| `runtime_forbidden_token_BossHand` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `BossHand` absent. |
| `runtime_forbidden_token_BossReward` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `BossReward` absent. |
| `runtime_forbidden_token_RewardDrop` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `RewardDrop` absent. |
| `runtime_forbidden_token_DropBias` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `DropBias` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V02` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `Scene_TalismanBag_V02` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V03` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | Forbidden runtime token `Scene_TalismanBag_V03` absent. |
| `runtime_forbidden_token_V03BattlePrepareInteractionController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `V03BattlePrepareInteractionController` absent. |
| `runtime_forbidden_token_V02RunFlowController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `V02RunFlowController` absent. |
| `runtime_forbidden_token_V02FormationGridFrame` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `V02FormationGridFrame` absent. |
| `runtime_forbidden_token_MainTrialProgressData` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `MainTrialProgressData` absent. |
| `runtime_forbidden_token_PlayerPrefs` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `PlayerPrefs` absent. |
| `runtime_forbidden_token_RunFlow` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `RunFlow` absent. |
| `runtime_forbidden_token_PageState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `PageState` absent. |
| `runtime_forbidden_token_FormationState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `FormationState` absent. |
| `runtime_forbidden_token_BossHand` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `BossHand` absent. |
| `runtime_forbidden_token_BossReward` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `BossReward` absent. |
| `runtime_forbidden_token_RewardDrop` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `RewardDrop` absent. |
| `runtime_forbidden_token_DropBias` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `DropBias` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V02` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `Scene_TalismanBag_V02` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V03` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | Forbidden runtime token `Scene_TalismanBag_V03` absent. |
| `runtime_forbidden_token_V03BattlePrepareInteractionController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `V03BattlePrepareInteractionController` absent. |
| `runtime_forbidden_token_V02RunFlowController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `V02RunFlowController` absent. |
| `runtime_forbidden_token_V02FormationGridFrame` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `V02FormationGridFrame` absent. |
| `runtime_forbidden_token_MainTrialProgressData` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `MainTrialProgressData` absent. |
| `runtime_forbidden_token_PlayerPrefs` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `PlayerPrefs` absent. |
| `runtime_forbidden_token_RunFlow` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `RunFlow` absent. |
| `runtime_forbidden_token_PageState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `PageState` absent. |
| `runtime_forbidden_token_FormationState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `FormationState` absent. |
| `runtime_forbidden_token_BossHand` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `BossHand` absent. |
| `runtime_forbidden_token_BossReward` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `BossReward` absent. |
| `runtime_forbidden_token_RewardDrop` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `RewardDrop` absent. |
| `runtime_forbidden_token_DropBias` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `DropBias` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V02` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `Scene_TalismanBag_V02` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V03` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | Forbidden runtime token `Scene_TalismanBag_V03` absent. |
| `runtime_forbidden_token_V03BattlePrepareInteractionController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `V03BattlePrepareInteractionController` absent. |
| `runtime_forbidden_token_V02RunFlowController` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `V02RunFlowController` absent. |
| `runtime_forbidden_token_V02FormationGridFrame` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `V02FormationGridFrame` absent. |
| `runtime_forbidden_token_MainTrialProgressData` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `MainTrialProgressData` absent. |
| `runtime_forbidden_token_PlayerPrefs` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `PlayerPrefs` absent. |
| `runtime_forbidden_token_RunFlow` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `RunFlow` absent. |
| `runtime_forbidden_token_PageState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `PageState` absent. |
| `runtime_forbidden_token_FormationState` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `FormationState` absent. |
| `runtime_forbidden_token_BossHand` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `BossHand` absent. |
| `runtime_forbidden_token_BossReward` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `BossReward` absent. |
| `runtime_forbidden_token_RewardDrop` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `RewardDrop` absent. |
| `runtime_forbidden_token_DropBias` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `DropBias` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V02` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `Scene_TalismanBag_V02` absent. |
| `runtime_forbidden_token_Scene_TalismanBag_V03` | `False` | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` | Forbidden runtime token `Scene_TalismanBag_V03` absent. |

## Formal Scope Confirmation

- `Scene_TalismanBag_V02_FormationCounter` is not a target of this package.
- `Scene_TalismanBag_V03_MainHome` is not a target of this package.
- `Scene_TalismanBag_V03_TalismanUpgrade` is not a target of this package.
- Formal RunFlow, PageState, FormationState, save data, Boss, reward, drop, and numeric systems are not touched.
- The V0.4 preview scene remains outside Build Settings.
- The old V02/V03 BattlePrepare runtime core is not used as placement authority.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Config Validation | `PASS` | 0 | 0 | 211 |
| devOnly Isolation | `PASS` | 0 | 0 | 25 |
| BattleSandbox Preview Scene 01 | `FAIL` | 1 | 0 | 44 |
| BuildGrid Interaction Preview Scene Binding | `PASS` | 0 | 0 | 13 |
| Battle Sandbox Shape Placement Vertical Slice Scene | `FAIL` | 5 | 0 | 16 |
| Battle Sandbox Shape Placement Protocol | `FAIL` | 7 | 0 | 39 |
| Battle Sandbox Shape Placement Leak Check | `FAIL` | 1 | 0 | 91 |

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
| `Error` | `BATTLE_SANDBOX_CHILD_MISSING` | BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/BattleLikePreviewArea missing EnemyCombatFeedbackPanel. | `EnemyCombatFeedbackPanel` |
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
| `Info` | `BUILD_GRID_PREVIEW_SCENE_BOUND` | Grid interaction controller is bound. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BUILD_GRID_BOARD_PRESENT` | BoardGridPreview exists. | `BoardGridPreview` |
| `Info` | `BUILD_GRID_TRAY_PRESENT` | ItemTrayPreview exists. | `ItemTrayPreview` |
| `Info` | `BUILD_GRID_INFO_PRESENT` | SelectedItemInfo exists. | `SelectedItemInfo` |
| `Info` | `BUILD_GRID_FEEDBACK_PRESENT` | PlacementFeedback exists. | `PlacementFeedback` |
| `Info` | `BUILD_GRID_BOARD_SLOT_COUNT` | Board slot count pass. actual=25, expected>=25. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_TRAY_SLOT_COUNT` | Tray slot count pass. actual=40, expected>=40. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_CATEGORY_COUNT` | Category filter count pass. actual=6, expected>=5. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_SHAPE_COUNT` | Shape count pass. actual=5, expected>=4. | `V0.4-BuildGridInteractionPreview01` |
| `Info` | `BUILD_GRID_CONTROLLER_ISOLATION_PASS` | Controller is devOnly and isolated from formal flow/save/UI/scene surfaces. | `BuildGridInteractionPreviewController` |
| `Info` | `BUILD_GRID_BUILDSETTINGS_UNTOUCHED` | V04 Preview Scene is not present in Build Settings. | `ProjectSettings/EditorBuildSettings.asset` |
| `Info` | `BUILD_GRID_PLAYER_TEXT_CHINESE_ONLY` | Player-facing preview text is Chinese-only. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BUILD_GRID_COMPLETE_ANSWERS_HIDDEN` | Player-facing preview text does not expose complete answers. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SCENE_MISSING` | V04 preview scene must exist. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_CONTROLLER_MISSING` | Vertical slice controller must be present in V04 preview scene. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SURFACE_MISSING` | Battle-like preview area must exist for the V0.2/V0.3 visual surface. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_BOARD_MISSING` | BoardGridPreview must exist. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_TRAY_MISSING` | ItemTrayPreview must exist. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Error` | `BATTLE_SANDBOX_VSLICE_FEEDBACK_MISSING` | PlacementFeedback must exist. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_ROTATE_BUTTON_MISSING` | Rotate button must exist. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_CANCEL_BUTTON_MISSING` | Cancel button must exist. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_NOT_DEVONLY` | Controller must remain devOnly=true. | `BuildGridInteractionPreviewController` |
| `Info` | `BATTLE_SANDBOX_VSLICE_ENABLED_TRUE` | Controller must remain isEnabled=false. | `BuildGridInteractionPreviewController` |
| `Info` | `BATTLE_SANDBOX_VSLICE_READS_FORMAL_SAVE` | Controller must not read formal save data. | `BuildGridInteractionPreviewController` |
| `Info` | `BATTLE_SANDBOX_VSLICE_WRITES_FORMAL_FLOW` | Controller must not write formal flow. | `BuildGridInteractionPreviewController` |
| `Info` | `BATTLE_SANDBOX_VSLICE_WRITES_FORMAL_UI` | Controller must not write formal UI. | `BuildGridInteractionPreviewController` |
| `Info` | `BATTLE_SANDBOX_VSLICE_TOUCHES_FORMAL_SCENE` | Controller must not touch formal scenes. | `BuildGridInteractionPreviewController` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SHOWS_COMPLETE_ANSWERS` | Controller must not show complete answers. | `BuildGridInteractionPreviewController` |
| `Info` | `BATTLE_SANDBOX_VSLICE_BUILD_SETTINGS_LEAK` | V04 preview scene must not be added to Build Settings. | `ProjectSettings/EditorBuildSettings.asset` |
| `Info` | `BATTLE_SANDBOX_VSLICE_ITEM_COUNT` | Preview tray must expose the primary x2 item plus additional test items. actual=23, expected>=9. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_SHAPE_CELL_COUNT` | Vertical slice item must be an x2 shape. actual=4, expected=2. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_TRAY_X2_DISPLAY` | Tray footprint must occupy two cells. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_TRAY_X2_VISUAL` | Tray view must render the x2 item as a real two-cell span. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_SHAPE_ID` | Vertical slice item must use Vertical2 shape. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_NO_SESSION` | ShapePlacementSession must be the interaction authority. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_NO_SHAPE_TRAY` | ShapeAwareItemTrayGrid must back tray packing/display. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_NO_MOBILE_INPUT` | MobileShapePlacementInputExtension must drive the input protocol. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_FORMAL_CORE_CONNECTED` | V0.2/V0.3 runtime core must not be connected. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_CLICK_NOT_INFO_ONLY` | Single item click must keep placement input/session idle and only refresh item info. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_CARD_CLICK_ROTATES` | Item card click must not call RotateSelectedItem. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_DRAG_DOES_NOT_START_PLACEMENT` | Dragging a tray item must start the placement session. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_ROTATE_NOT_EXPLICIT_BUTTON` | Rotation must be driven by the item info popup Rotate button path. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_ROTATE_NOT_TRAY_ONLY` | Rotation must be tray-only. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_DRAG_ROTATE_ENABLED` | Rotation must be disabled while dragging/previewing. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_BOARD_ROTATE_ENABLED` | Rotation must be disabled after board placement. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_TRAY_VERTICAL_DEFAULT` | Vertical2 must default to a vertical two-cell tray footprint. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_TRAY_ROTATE_HORIZONTAL` | Popup Rotate must turn Vertical2 into a horizontal two-cell tray footprint. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_TRAY_ROTATE_BACK_VERTICAL` | Popup Rotate must toggle back to vertical. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Error` | `BATTLE_SANDBOX_VSLICE_TRAY_ROTATE_INVALID_NOT_REJECTED` | Popup Rotate must fail on out-of-bounds or overlap and keep the old direction. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: initial_idle. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: tray_vertical_two_cell_default. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: tray_rotate_to_horizontal. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: tray_rotate_back_to_vertical. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: tray_rotate_invalid_keeps_direction. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: click_item_info_only. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: drag_starts_placement. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: drag_to_board_ghost. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: release_valid_commits. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: release_invalid_returns_to_tray. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: no_ghost_click_confirm. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE_MISSING` | Missing required protocol sample: cancel_available. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Error` | `BATTLE_SANDBOX_VSLICE_VALIDITY_MISMATCH` | tray_vertical_two_cell_default expected valid=True, actual=False. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | tray_vertical_two_cell_default: ShapeAwareItemTrayGrid.TryPack Idle/Idle->Idle/Idle, valid=False. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Error` | `BATTLE_SANDBOX_VSLICE_VALIDITY_MISMATCH` | tray_rotate_to_horizontal expected valid=True, actual=False. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | tray_rotate_to_horizontal: Info popup Rotate button Idle/Idle->Idle/Committed, valid=False. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Error` | `BATTLE_SANDBOX_VSLICE_VALIDITY_MISMATCH` | tray_rotate_back_to_vertical expected valid=True, actual=False. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | tray_rotate_back_to_vertical: Info popup Rotate button again Idle/Idle->Idle/Committed, valid=False. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | tray_rotate_invalid_keeps_direction: Info popup Rotate button at right edge Idle/Idle->Idle/InvalidPreview, valid=False. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | initial_idle: Open scene Idle/Idle->Idle/Idle, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | click_item_info_only: Click item info Idle/Idle->Idle/Idle, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | drag_starts_placement: BeginDrag tray item Idle/Idle->HoldingItem/HoldingItem, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | drag_to_board_ghost: Drag to board HoldingItem/HoldingItem->DraggingPreview/Previewing, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | release_valid_commits: Release drag DraggingPreview/Previewing->Placed/Committed, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | no_ghost_click_confirm: No ghost click confirm Placed/Committed->Placed/Committed, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | release_invalid_returns_to_tray: Release invalid drag InvalidPreview/HoldingItem->Cancelled/Cancelled, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAMPLE` | cancel_available: Cancel DraggingPreview/Previewing->Cancelled/Cancelled, valid=True. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_RELEASE_VALID_NOT_COMMITTED` | Valid drag release must commit immediately. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_RELEASE_INVALID_NOT_RETURNED` | Invalid drag release must return the item to tray. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_GHOST_CONFIRM_STILL_REQUIRED` | Ghost click confirm must not be required. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_CANCEL_UNAVAILABLE` | Cancel must be available. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_FEATURE_DEFAULT_ON` | BuildSandbox feature flags must default to disabled. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_VSLICE_FORMAL_SCENE_TOUCH` | Formal V02/V03 scenes must not be touched. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_RUNFLOW_TOUCH` | RunFlow must not be touched. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_SAVE_TOUCH` | Save data must not be touched. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_BOSS_REWARD_NUMERIC_TOUCH` | Boss/reward/drop/numeric systems must not be touched. | `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | feature_flags_default_disabled: All BuildSandbox feature flags default false. | `BuildSandboxFeatureFlags` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | preview_scene_not_in_build_settings: V04 preview scene is not present in Build Settings. | `ProjectSettings/EditorBuildSettings.asset` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | controller_formal_surface_flags: Controller formal surface flags remain false. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V03BattlePrepareInteractionController: Forbidden runtime token `V03BattlePrepareInteractionController` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02RunFlowController: Forbidden runtime token `V02RunFlowController` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02FormationGridFrame: Forbidden runtime token `V02FormationGridFrame` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_MainTrialProgressData: Forbidden runtime token `MainTrialProgressData` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PlayerPrefs: Forbidden runtime token `PlayerPrefs` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RunFlow: Forbidden runtime token `RunFlow` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PageState: Forbidden runtime token `PageState` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_FormationState: Forbidden runtime token `FormationState` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossHand: Forbidden runtime token `BossHand` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossReward: Forbidden runtime token `BossReward` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RewardDrop: Forbidden runtime token `RewardDrop` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Error` | `BATTLE_SANDBOX_VSLICE_LEAK` | runtime_forbidden_token_DropBias: Found forbidden runtime token `DropBias`. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V02: Forbidden runtime token `Scene_TalismanBag_V02` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V03: Forbidden runtime token `Scene_TalismanBag_V03` absent. | `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V03BattlePrepareInteractionController: Forbidden runtime token `V03BattlePrepareInteractionController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02RunFlowController: Forbidden runtime token `V02RunFlowController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02FormationGridFrame: Forbidden runtime token `V02FormationGridFrame` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_MainTrialProgressData: Forbidden runtime token `MainTrialProgressData` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PlayerPrefs: Forbidden runtime token `PlayerPrefs` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RunFlow: Forbidden runtime token `RunFlow` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PageState: Forbidden runtime token `PageState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_FormationState: Forbidden runtime token `FormationState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossHand: Forbidden runtime token `BossHand` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossReward: Forbidden runtime token `BossReward` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RewardDrop: Forbidden runtime token `RewardDrop` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_DropBias: Forbidden runtime token `DropBias` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V02: Forbidden runtime token `Scene_TalismanBag_V02` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V03: Forbidden runtime token `Scene_TalismanBag_V03` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V03BattlePrepareInteractionController: Forbidden runtime token `V03BattlePrepareInteractionController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02RunFlowController: Forbidden runtime token `V02RunFlowController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02FormationGridFrame: Forbidden runtime token `V02FormationGridFrame` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_MainTrialProgressData: Forbidden runtime token `MainTrialProgressData` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PlayerPrefs: Forbidden runtime token `PlayerPrefs` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RunFlow: Forbidden runtime token `RunFlow` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PageState: Forbidden runtime token `PageState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_FormationState: Forbidden runtime token `FormationState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossHand: Forbidden runtime token `BossHand` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossReward: Forbidden runtime token `BossReward` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RewardDrop: Forbidden runtime token `RewardDrop` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_DropBias: Forbidden runtime token `DropBias` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V02: Forbidden runtime token `Scene_TalismanBag_V02` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V03: Forbidden runtime token `Scene_TalismanBag_V03` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V03BattlePrepareInteractionController: Forbidden runtime token `V03BattlePrepareInteractionController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02RunFlowController: Forbidden runtime token `V02RunFlowController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02FormationGridFrame: Forbidden runtime token `V02FormationGridFrame` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_MainTrialProgressData: Forbidden runtime token `MainTrialProgressData` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PlayerPrefs: Forbidden runtime token `PlayerPrefs` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RunFlow: Forbidden runtime token `RunFlow` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PageState: Forbidden runtime token `PageState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_FormationState: Forbidden runtime token `FormationState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossHand: Forbidden runtime token `BossHand` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossReward: Forbidden runtime token `BossReward` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RewardDrop: Forbidden runtime token `RewardDrop` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_DropBias: Forbidden runtime token `DropBias` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V02: Forbidden runtime token `Scene_TalismanBag_V02` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V03: Forbidden runtime token `Scene_TalismanBag_V03` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V03BattlePrepareInteractionController: Forbidden runtime token `V03BattlePrepareInteractionController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02RunFlowController: Forbidden runtime token `V02RunFlowController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02FormationGridFrame: Forbidden runtime token `V02FormationGridFrame` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_MainTrialProgressData: Forbidden runtime token `MainTrialProgressData` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PlayerPrefs: Forbidden runtime token `PlayerPrefs` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RunFlow: Forbidden runtime token `RunFlow` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PageState: Forbidden runtime token `PageState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_FormationState: Forbidden runtime token `FormationState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossHand: Forbidden runtime token `BossHand` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossReward: Forbidden runtime token `BossReward` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RewardDrop: Forbidden runtime token `RewardDrop` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_DropBias: Forbidden runtime token `DropBias` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V02: Forbidden runtime token `Scene_TalismanBag_V02` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V03: Forbidden runtime token `Scene_TalismanBag_V03` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V03BattlePrepareInteractionController: Forbidden runtime token `V03BattlePrepareInteractionController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02RunFlowController: Forbidden runtime token `V02RunFlowController` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_V02FormationGridFrame: Forbidden runtime token `V02FormationGridFrame` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_MainTrialProgressData: Forbidden runtime token `MainTrialProgressData` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PlayerPrefs: Forbidden runtime token `PlayerPrefs` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RunFlow: Forbidden runtime token `RunFlow` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_PageState: Forbidden runtime token `PageState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_FormationState: Forbidden runtime token `FormationState` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossHand: Forbidden runtime token `BossHand` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_BossReward: Forbidden runtime token `BossReward` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_RewardDrop: Forbidden runtime token `RewardDrop` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_DropBias: Forbidden runtime token `DropBias` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V02: Forbidden runtime token `Scene_TalismanBag_V02` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `BATTLE_SANDBOX_VSLICE_LEAK_CLEAR` | runtime_forbidden_token_Scene_TalismanBag_V03: Forbidden runtime token `Scene_TalismanBag_V03` absent. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |

