# Item Instance Placement Binding Contract Report

- Package: `V0.4-ItemInstancePlacementBindingContract01`
- Guard receipt: `GUARD_PASS_ITEMINSTANCEPLACEMENTBINDINGCONTRACT01`
- Contract: `ItemInstancePlacementBindingContractSnapshot.v1`
- Source contracts: `ItemInstanceProjectionContractSnapshot.v1` + `ItemSystemSnapshot.v1`
- Mode: `Unity batch`
- Result: `FAIL`
- Offline verifier: `PASS`
- Unity compile / verifier: `FAIL`
- Legal / abnormal scenarios: `3 / 9`
- Representative Binding count: `2`
- Canonical Signature: `sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428`
- Duplicate / orphan / mismatch checks: `PASS / PASS / PASS`
- I031 ordinary-instance rejection: `PASS`
- Formal-system Leak Count: `0`

## Contract Rules

- `itemInstanceId`, `placementId`, and `baseItemId` are supplied explicitly in each row.
- `itemInstanceId == placementId` is never treated as evidence.
- `baseItemId` must match both Instance Projection `baseItemId` and Placement `itemId`.
- Missing and orphan facts remain `Unknown`; duplicate, mismatch, invalid-source, and I031 forgery facts are `Invalid`.
- I031 remains a system JuNian placement and never becomes an ordinary generated/drop instance.
- Output bindings and errors are immutable defensive collections.
- Canonical Signature uses Ordinal sorting, InvariantCulture formatting, and SHA-256.

## Checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| scenario.legal.explicit-three-way | Valid | Valid; bindings=2; codes= | PASS |
| scenario.legal.ordinal-invariant-order | Valid | Valid; bindings=2; codes= | PASS |
| scenario.missing.equal-ids-not-guessed | Unknown | Unknown; bindings=0; codes=PLACEMENT_BINDING_MISSING;PROJECTION_BINDING_MISSING | PASS |
| scenario.duplicate.item-instance | Invalid | Invalid; bindings=0; codes=BINDING_ITEM_INSTANCE_ID_DUPLICATE;PROJECTION_BINDING_MISSING | PASS |
| scenario.duplicate.placement | Invalid | Invalid; bindings=0; codes=BINDING_PLACEMENT_ID_DUPLICATE;PLACEMENT_BINDING_MISSING | PASS |
| scenario.orphan.item-instance | Unknown | Unknown; bindings=1; codes=PROJECTION_BINDING_MISSING;PROJECTION_ORPHAN | PASS |
| scenario.orphan.placement | Unknown | Unknown; bindings=1; codes=PLACEMENT_BINDING_MISSING;PLACEMENT_ORPHAN | PASS |
| scenario.mismatch.projection-base | Invalid | Invalid; bindings=0; codes=PROJECTION_BASE_ITEM_ID_MISMATCH | PASS |
| scenario.mismatch.placement-item | Invalid | Invalid; bindings=0; codes=PLACEMENT_BASE_ITEM_ID_MISMATCH | PASS |
| scenario.i031.binding-forbidden | Invalid | Invalid; bindings=0; codes=I031_ORDINARY_INSTANCE_FORBIDDEN | PASS |
| scenario.i031.projection-forbidden | Invalid | Invalid; bindings=0; codes=I031_ORDINARY_INSTANCE_FORBIDDEN | PASS |
| scenario.legal.immutable-output | Valid | Valid; bindings=2; codes= | PASS |
| semantics.abnormal-never-known-zero | all abnormal rows are Invalid or Unknown | Invalid/Unknown only | PASS |
| contract.schema-id | ItemInstancePlacementBindingContractSnapshot.v1 | ItemInstancePlacementBindingContractSnapshot.v1 | PASS |
| contract.identity-properties-readonly | no public setters | itemInstanceId/placementId/baseItemId | PASS |
| contract.separate-from-existing-signatures | new independent canonicalSignature | ItemInstancePlacementBindingContractSnapshot.v1 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_ItemSystemSnapshot_cs | 1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df | 1/794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827 | FAIL |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Generation_Projection_ItemInstanceProjectionContract_cs | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Generation_Affixes_ItemAffixPoolAndRangeSchema_cs | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | PASS |
| protected.baseline.ProjectSettings_EditorBuildSettings_asset | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_EnemySystem | 89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4 | 96/6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb | FAIL |
| protected.baseline.Assets__Game_Scripts_TalismanBag_CrossSystem | 5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1 | 63/af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48 | FAIL |
| protected.baseline.Assets__Game_Scenes | 14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b | 14/7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579 | FAIL |
| protected.baseline.Assets__Game_Prefabs | 16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 | 16/264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8 | FAIL |
| leak.formal-system-count | 0 | 0 | PASS |
| package.expected-files | 11/11 present | 11/11 present | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_ItemSystemSnapshot_cs | 1/794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827 | 1/794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Generation_Projection_ItemInstanceProjectionContract_cs | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Generation_Affixes_ItemAffixPoolAndRangeSchema_cs | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | PASS |
| protected.before-after.ProjectSettings_EditorBuildSettings_asset | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_EnemySystem | 96/6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb | 96/6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_CrossSystem | 63/af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48 | 63/af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48 | PASS |
| protected.before-after.Assets__Game_Scenes | 14/7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579 | 14/7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579 | PASS |
| protected.before-after.Assets__Game_Prefabs | 16/264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8 | 16/264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8 | PASS |

## Protected Hashes

| Scope | Phase | Expected/Before | Actual/After | Result |
|---|---|---|---|---|
| Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs | baseline | 1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df | 1/794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827 | FAIL |
| Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs | baseline | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | PASS |
| Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs | baseline | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | PASS |
| ProjectSettings/EditorBuildSettings.asset | baseline | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | PASS |
| Assets/_Game/Scripts/TalismanBag/EnemySystem | baseline | 89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4 | 96/6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb | FAIL |
| Assets/_Game/Scripts/TalismanBag/CrossSystem | baseline | 5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1 | 63/af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48 | FAIL |
| Assets/_Game/Scenes | baseline | 14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b | 14/7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579 | FAIL |
| Assets/_Game/Prefabs | baseline | 16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 | 16/264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8 | FAIL |
| Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs | before-after | 1/794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827 | 1/794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827 | PASS |
| Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs | before-after | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | PASS |
| Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs | before-after | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | PASS |
| ProjectSettings/EditorBuildSettings.asset | before-after | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | PASS |
| Assets/_Game/Scripts/TalismanBag/EnemySystem | before-after | 96/6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb | 96/6ec06b00cc7248cfb8bd6e8b34c5b55aa2fa0dec01b76258fd9a626b665e96eb | PASS |
| Assets/_Game/Scripts/TalismanBag/CrossSystem | before-after | 63/af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48 | 63/af8fef0c99cc46a1bd11845b1af3a3d2ed2dda5744efacb0c18ed113ac9c2f48 | PASS |
| Assets/_Game/Scenes | before-after | 14/7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579 | 14/7dae512c3d59e7a59585177233210ff40bdd53fbd942655d96dde45e81077579 | PASS |
| Assets/_Game/Prefabs | before-after | 16/264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8 | 16/264dc3d9c59894011fe4367886e7a7243c4b063019b5409d5ab5406b05bfe7a8 | PASS |

## Scope

- No modification to `ItemSystemSnapshot.v1`, `BuildDebugSignature`, Instance Projection Canonical Signature, or Affix Schema.
- No Item→BuildCapability mapping and no point/stack/count→BP conversion.
- No Enemy, CrossSystem, Battle, Board, RunFlow, SaveData, Reward, Chapter, Scene, Prefab, UI, RectTransform, or BuildSettings connection.

## Marker

ITEM_INSTANCE_PLACEMENT_BINDING_CONTRACT01_FAIL
