# Item Capability Unit Contract Report

- Package: `V0.4-ItemCapabilityUnitContract01`
- Guard receipt: `GUARD_PASS_ITEMCAPABILITYUNITCONTRACT01`
- Schema: `ItemCapabilityUnitContractSnapshot.v1`
- Canonical Signature: `sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673`
- Native unit definitions: `6`
- Memory scenarios: `10`
- Mode: `Unity batch verifier`
- Offline verifier: `PASS`
- Unity verifier: `PASS`
- Result: `PASS`

## Source / Config correction

- State: `PASS`
- Detail: `Source=PASS, Config=PASS, Payload=basisPoint/ReducePercent/200/white:200-400;green:350-650;blue:550-900;purple:800-1300;orange:1200-1800`
- Generic `Ranges()` behavior: unchanged by contract; the source verifier requires a dedicated `NianEfficiencyRanges()` path.

## Canonical generation baselines

| Boundary | Before | Current | Result |
|---|---|---|---|
| Affix Schema | `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f` | `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f` | PASS |
| 150 Roll | `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` | `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` | PASS |
| 150 Projection | `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` | `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` | PASS |

- Preview count: `150`
- Preview errors: `0`

## Checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| `canonical.ordinal-invariant-stable-order` | `sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673` | `sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673` | PASS |
| `contract.definition-count` | `6` | `6` | PASS |
| `contract.immutable-definitions` | `NotSupportedException` | `NotSupportedException` | PASS |
| `contract.schema` | `ItemCapabilityUnitContractSnapshot.v1` | `ItemCapabilityUnitContractSnapshot.v1` | PASS |
| `contract.six-native-domains` | `affix_chain_target\|target_count\|count\|ExtraTarget\|1\|6;affix_cleanse_up\|cleanse_stack\|stack\|AddFlat\|1\|5;affix_control_up\|control_point\|point\|AddFlat\|1\|10;affix_first_trigger_bonus\|effect_basis_point\|basisPoint\|ExtraTrigger\|3000\|8000;affix_nian_efficiency\|nian_cost_basis_point\|basisPoint\|ReducePercent\|200\|1800;affix_trigger_refund\|nian_point\|point\|Refund\|1\|6` | `affix_chain_target\|target_count\|count\|ExtraTarget\|1\|6;affix_cleanse_up\|cleanse_stack\|stack\|AddFlat\|1\|5;affix_control_up\|control_point\|point\|AddFlat\|1\|10;affix_first_trigger_bonus\|effect_basis_point\|basisPoint\|ExtraTrigger\|3000\|8000;affix_nian_efficiency\|nian_cost_basis_point\|basisPoint\|ReducePercent\|200\|1800;affix_trigger_refund\|nian_point\|point\|Refund\|1\|6` | PASS |
| `generation.affix-schema-stable` | `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f` | `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f` | PASS |
| `generation.preview-count` | `150` | `150` | PASS |
| `generation.preview-errors` | `0` | `0` | PASS |
| `generation.projection-150-stable` | `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` | `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` | PASS |
| `generation.roll-150-stable` | `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` | `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` | PASS |
| `leak.count` | `0` | `0` | PASS |
| `package.files` | `11 present` | `11 present` | PASS |
| `protected.Assets/_Game/Configs/ItemBalanceWorkbench/Profiles` | `60/4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317` | `60/4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317` | PASS |
| `protected.Assets/_Game/Prefabs` | `16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | `16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | PASS |
| `protected.Assets/_Game/Resources/item` | `294/5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29` | `294/5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29` | PASS |
| `protected.Assets/_Game/Resources/item_daoju` | `198/bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f` | `198/bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f` | PASS |
| `protected.Assets/_Game/Scenes` | `14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | `14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/CrossSystem` | `5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1` | `5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/EnemySystem` | `89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4` | `89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/ItemSandbox` | `42/ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0` | `42/ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs` | `1/335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0` | `1/335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/Items/Detail` | `23/213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4` | `23/213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs` | `1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db` | `1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` | `1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` | `1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` | PASS |
| `protected.Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df` | `1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df` | PASS |
| `protected.ProjectSettings/EditorBuildSettings.asset` | `1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | `1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | PASS |
| `scenario.invalid.negative` | `Invalid/NEGATIVE_VALUE` | `Invalid/NEGATIVE_VALUE` | PASS |
| `scenario.invalid.operation-conflict` | `Invalid/OPERATION_CONFLICT` | `Invalid/OPERATION_CONFLICT` | PASS |
| `scenario.invalid.out-of-range` | `Invalid/VALUE_OUT_OF_RANGE` | `Invalid/VALUE_OUT_OF_RANGE` | PASS |
| `scenario.invalid.unit-conflict` | `Invalid/UNIT_CONFLICT` | `Invalid/UNIT_CONFLICT` | PASS |
| `scenario.known-value.control` | `KnownValue/NONE` | `KnownValue/NONE` | PASS |
| `scenario.known-value.nian` | `KnownValue/NONE` | `KnownValue/NONE` | PASS |
| `scenario.known-zero.explicit` | `KnownZero/NONE` | `KnownZero/NONE` | PASS |
| `scenario.unknown.capability` | `Unknown/CAPABILITY_UNKNOWN` | `Unknown/CAPABILITY_UNKNOWN` | PASS |
| `scenario.unknown.unit-missing` | `Unknown/UNIT_MISSING` | `Unknown/UNIT_MISSING` | PASS |
| `scenario.unknown.value-missing` | `Unknown/VALUE_MISSING` | `Unknown/VALUE_MISSING` | PASS |
| `source-config.nian-efficiency` | `correct basisPoint/ReducePercent/five ranges in Source and Config` | `Source=PASS, Config=PASS, Payload=basisPoint/ReducePercent/200/white:200-400;green:350-650;blue:550-900;purple:800-1300;orange:1200-1800` | PASS |
| `validator.immutable-errors` | `NotSupportedException` | `NotSupportedException` | PASS |

## Protected hashes

| Path | Files | Baseline | Current | Result |
|---|---:|---|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | 1 | `821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df` | `821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df` | PASS |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` | 1 | `f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` | `f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` | PASS |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs` | 1 | `3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db` | `3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db` | PASS |
| `Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs` | 1 | `335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0` | `335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0` | PASS |
| `ProjectSettings/EditorBuildSettings.asset` | 1 | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | PASS |
| `Assets/_Game/Configs/ItemBalanceWorkbench/Profiles` | 60 | `4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317` | `4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317` | PASS |
| `Assets/_Game/Scenes` | 14 | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | PASS |
| `Assets/_Game/Prefabs` | 16 | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | PASS |
| `Assets/_Game/Scripts/TalismanBag/EnemySystem` | 89 | `3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4` | `3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4` | PASS |
| `Assets/_Game/Scripts/TalismanBag/CrossSystem` | 5 | `d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1` | `d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1` | PASS |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail` | 23 | `213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4` | `213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4` | PASS |
| `Assets/_Game/Scripts/TalismanBag/ItemSandbox` | 42 | `ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0` | `ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0` | PASS |
| `Assets/_Game/Resources/item` | 294 | `5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29` | `5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29` | PASS |
| `Assets/_Game/Resources/item_daoju` | 198 | `bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f` | `bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f` | PASS |
