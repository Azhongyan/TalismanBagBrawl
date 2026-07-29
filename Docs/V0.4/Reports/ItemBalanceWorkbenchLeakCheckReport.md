# Item Balance Workbench GuardFix01 Leak Check

- result: FAIL
- source scope: Items/Balance and Editor/ItemBalance; this verifier is excluded because it contains the QA scanner and regression entry points.
- interpretation: matchCount is a concrete forbidden-reference regex hit count, not an inferred runtime state.

| category | scanRoot | matchCount | result |
|---|---|---:|---|
| reward | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| runflow | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| inventory | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| savedata | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| battle | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| boss | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| scene | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| prefab | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
| buildsettings | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 1 | FAIL |
| formal_runtime_wiring | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |

## Patterns and matches

### reward

- pattern: `\b(?:RewardConfig|RewardService|RewardResolver|RewardPipeline)\b`
- matches: none

### runflow

- pattern: `\b(?:V02RunFlowController|RunFlowController|RunFlowService|MainTrialFlowService)\b`
- matches: none

### inventory

- pattern: `\b(?:InventoryService|InventoryController|InventoryRepository|PlayerInventory)\b`
- matches: none

### savedata

- pattern: `\b(?:SaveData|SaveService|SaveRepository|PlayerPrefs)\b`
- matches: none

### battle

- pattern: `\b(?:BattleSnapshot|BattleResolver|AutoCombatController|BattleService|UnifiedBattle)\b`
- matches: none

### boss

- pattern: `\b(?:BossConfig|BossController|BossService|MiniBoss)\b`
- matches: none

### scene

- pattern: `\b(?:SceneManager|EditorSceneManager|SceneAsset)\b|\.unity\b`
- matches: none

### prefab

- pattern: `\b(?:PrefabUtility|PrefabStage|PrefabAsset)\b|\.prefab\b`
- matches: none

### buildsettings

- pattern: `\b(?:EditorBuildSettings|BuildSettings|BuildPipeline)\b`
- Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemCompleteCandidateContentWorkbenchVerifier.cs:331 => BuildSettings

### formal_runtime_wiring

- pattern: `\b(?:RuntimeInitializeOnLoadMethod|InitializeOnLoad|Resources\.Load|Addressables\.LoadAssetAsync|ItemStatRangeSchemaCatalog|ItemAffixPoolAndRangeCatalog|ItemCorePotentialAndBuildEligibilityCatalog)\b`
- matches: none

## Protected file boundary checks

| category | scanRoot | beforeHash | afterHash | mismatchCount | result |
|---|---|---|---|---:|---|
| candidate_assets | Assets/_Game/Configs/ItemBalanceWorkbench | `2B4F6D574DAFF01FCD893A3C090BCBA8DECF92547EBAE2112ED7C8172352DAA0` | `2B4F6D574DAFF01FCD893A3C090BCBA8DECF92547EBAE2112ED7C8172352DAA0` | 0 | PASS |
| workbench_function_files | Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | `40E8103EE34A33F335FC883EDDB9E3BF42B5A08ECC2EDE2154477ADF956D53B5` | `40E8103EE34A33F335FC883EDDB9E3BF42B5A08ECC2EDE2154477ADF956D53B5` | 0 | PASS |
| balance_runtime_and_workbench_schema | Assets/_Game/Scripts/TalismanBag/Items/Balance | `12BC82F037BCB0BD8025662DAA04F13551CEF962A1FBD27B6218DD515358559E` | `12BC82F037BCB0BD8025662DAA04F13551CEF962A1FBD27B6218DD515358559E` | 0 | PASS |
| protected_item_runtime_and_schema | Assets/_Game/Scripts/TalismanBag/Items | `805FAC45FA8C43A9789B325C9C563277526915BDE35556FD92B58FB272A98DEE` | `805FAC45FA8C43A9789B325C9C563277526915BDE35556FD92B58FB272A98DEE` | 0 | PASS |
| scene_files | Assets/_Game/Scenes | `9E91B90DF023B90D66AD30B1C066C2135B0931805AC22ADF36EDF84936DDFBB2` | `9E91B90DF023B90D66AD30B1C066C2135B0931805AC22ADF36EDF84936DDFBB2` | 0 | PASS |
| prefab_files | Assets/_Game/Prefabs | `85650BBB8ED3D23D8FB14433C22B97811C4AA66297C6AF2D9426E8756A58D302` | `85650BBB8ED3D23D8FB14433C22B97811C4AA66297C6AF2D9426E8756A58D302` | 0 | PASS |
| build_settings | ProjectSettings/EditorBuildSettings.asset | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | 0 | PASS |
