# Item Balance Workbench GuardFix01 Leak Check

- result: PASS
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
| buildsettings | Assets/_Game/Scripts/TalismanBag/Items/Balance; Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | 0 | PASS |
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
- matches: none

### formal_runtime_wiring

- pattern: `\b(?:RuntimeInitializeOnLoadMethod|InitializeOnLoad|Resources\.Load|Addressables\.LoadAssetAsync|ItemStatRangeSchemaCatalog|ItemAffixPoolAndRangeCatalog|ItemCorePotentialAndBuildEligibilityCatalog)\b`
- matches: none

## Protected file boundary checks

| category | scanRoot | beforeHash | afterHash | mismatchCount | result |
|---|---|---|---|---:|---|
| candidate_assets | Assets/_Game/Configs/ItemBalanceWorkbench | `7228D21FB30FC868FF4EDA234E556B82FCDEB9B6C8D1D3CB7BCC753CC2C874E7` | `7228D21FB30FC868FF4EDA234E556B82FCDEB9B6C8D1D3CB7BCC753CC2C874E7` | 0 | PASS |
| workbench_function_files | Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | `BCBE5425790A3309AD0218D48749571762B76B5212B7C09C6097EB5B744BF1D2` | `BCBE5425790A3309AD0218D48749571762B76B5212B7C09C6097EB5B744BF1D2` | 0 | PASS |
| balance_runtime_and_workbench_schema | Assets/_Game/Scripts/TalismanBag/Items/Balance | `288F44DA969C17762C3BAD36996906E73947A649A477715F4E24B962863B865C` | `288F44DA969C17762C3BAD36996906E73947A649A477715F4E24B962863B865C` | 0 | PASS |
| protected_item_runtime_and_schema | Assets/_Game/Scripts/TalismanBag/Items | `7B6892650BDC8AFC1A2A66F1036753D46C34FA2A852BE44DE55F17D8DECD6C59` | `7B6892650BDC8AFC1A2A66F1036753D46C34FA2A852BE44DE55F17D8DECD6C59` | 0 | PASS |
| scene_files | Assets/_Game/Scenes | `5DAEDEF6603F4623FB2C8AE3F9D0DFA88E3154153F572063F4BDDBE3745C3BE1` | `5DAEDEF6603F4623FB2C8AE3F9D0DFA88E3154153F572063F4BDDBE3745C3BE1` | 0 | PASS |
| prefab_files | Assets/_Game/Prefabs | `3B38B60DC7203C1B851B4C83CDD4B809803916EA1483D2647151A8ED89C9DB01` | `3B38B60DC7203C1B851B4C83CDD4B809803916EA1483D2647151A8ED89C9DB01` | 0 | PASS |
| build_settings | ProjectSettings/EditorBuildSettings.asset | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | 0 | PASS |
