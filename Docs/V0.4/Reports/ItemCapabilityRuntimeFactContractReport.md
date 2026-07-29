# Item Capability Runtime Fact Contract Report

- Package: `V0.4-ItemCapabilityRuntimeFactContract01`
- Guard receipt: `GUARD_PASS_ITEMCAPABILITYRUNTIMEFACTCONTRACT01`
- Marker: `ITEM_CAPABILITY_RUNTIME_FACT_CONTRACT01_PASS`
- Mode: `Unity batch verifier`
- Schema: `ItemCapabilityRuntimeFactContractSnapshot.v1`
- New files / existing modified: `10 / 0`
- Facts / scenarios: `5 / 18`
- KnownTrue / KnownFalse / Unknown / Invalid: `5 / 4 / 2 / 7`
- Canonical Signature: `sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a`
- Input reversal: `PASS`
- Fact mutation sensitivity: `PASS`
- Immutable input/output: `PASS`
- Identity / duplicate / sequence / ordinal: `PASS`
- Protected hashes: `44/44 PASS`
- Leak Count: `0`
- Offline verifier: `PASS`
- Unity verifier: `PASS`
- Forbidden scope touched: `0`

## Fixed signatures

| Contract | Signature | Result |
| --- | --- | --- |
| Unit Contract | `sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673` | PASS |
| Binding Contract | `sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428` | PASS |
| Affix Schema | `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f` | protected / unchanged |
| 150 Roll | `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` | protected / unchanged |
| 150 Projection | `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` | protected / unchanged |

## Checks

| Check | Expected | Actual | Result |
| --- | --- | --- | --- |
| predecessor.binding-signature | sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428 | sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428 | PASS |
| predecessor.unit-signature | sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673 | sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673 | PASS |
| contract.schema | ItemCapabilityRuntimeFactContractSnapshot.v1 | ItemCapabilityRuntimeFactContractSnapshot.v1 | PASS |
| contract.fact-count | 5 | 5 | PASS |
| contract.no-errors | 0 | 0 | PASS |
| contract.completeness | Complete | Complete | PASS |
| contract.binding-signature-field | sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428 | sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428 | PASS |
| contract.unit-signature-field | sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673 | sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673 | PASS |
| contract.canonical-signature | sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a | sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a | PASS |
| canonical.input-reversal | sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a | sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a | PASS |
| canonical.fact-mutation | different signature | sha256:c8efdb952dde106f6defb9b34a57a480225c01697361294e00fb2e035b86d18f | PASS |
| immutable.input-defensive-copy | 5 | 5 | PASS |
| immutable.output-facts | mutation rejected | mutation rejected | PASS |
| immutable.output-errors | mutation rejected | mutation rejected | PASS |
| truth-count.known-true | 9 | 9 | PASS |
| truth-count.known-false | 11 | 11 | PASS |
| scenario-count | 18 | 18 | PASS |
| shape.nullable-bool.triggerSuccess | Nullable<Boolean> | System.Nullable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-bool.firstTriggerInBattle | Nullable<Boolean> | System.Nullable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-bool.cleanseSuccess | Nullable<Boolean> | System.Nullable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-bool.chainCount3 | Nullable<Boolean> | System.Nullable`1[[System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-long.eventSequence | Nullable<Int64> | System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-long.triggerOrdinal | Nullable<Int64> | System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-long.cleanseExtraStackCount | Nullable<Int64> | System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-long.consecutiveTriggerCount | Nullable<Int64> | System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-long.nianCostBefore | Nullable<Int64> | System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-long.nianCostAfter | Nullable<Int64> | System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-long.refundUnits | Nullable<Int64> | System.Nullable`1[[System.Int64, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] | PASS |
| shape.nullable-completeness | nullable completeness | System.Nullable`1[[TalismanBag.Items.Capability.RuntimeFacts.ItemCapabilityRuntimeFactCompleteness, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] | PASS |
| shape.provider | read-only provider interface | True | PASS |
| shape.truth-enum | KnownTrue,KnownFalse,Unknown,Invalid | KnownTrue,KnownFalse,Unknown,Invalid | PASS |
| shape.snapshot-constructor | non-public | 0 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemInstancePlacementBindingContract_cs | 1/335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0 | 1/335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemInstancePlacementBindingValidator_cs | 1/bda2f04b28eff93d6a16d585fd65251607c9964c4bd26d3d594f6f1131fa0d4c | 1/bda2f04b28eff93d6a16d585fd65251607c9964c4bd26d3d594f6f1131fa0d4c | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemCapabilityUnitContract_cs | 1/cc40035b945c203a6aaf3b0426200e925703d5f51e9b6c54435f8d6d08ca51b7 | 1/cc40035b945c203a6aaf3b0426200e925703d5f51e9b6c54435f8d6d08ca51b7 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemCapabilityUnitValidator_cs | 1/77e52c84fac2abdb71c5f346e92cdf759a8b5ea5319a142e53cd983c13667e3c | 1/77e52c84fac2abdb71c5f346e92cdf759a8b5ea5319a142e53cd983c13667e3c | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_ItemSystemSnapshot_cs | 1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df | 1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Generation_Projection_ItemInstanceProjectionContract_cs | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Generation_Affixes_ItemAffixPoolAndRangeSchema_cs | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Balance_ItemCompleteCandidateContent_cs | 1/c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb | 1/c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb | PASS |
| protected.baseline.Assets__Game_Configs_ItemBalanceWorkbench_ItemBalanceWorkbenchCatalog_asset | 1/5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45 | 1/5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45 | PASS |
| protected.baseline.ProjectSettings_EditorBuildSettings_asset | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | PASS |
| protected.baseline.Assets__Game_Configs_ItemBalanceWorkbench_Profiles | 60/4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317 | 60/4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Items_Detail | 23/213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4 | 23/213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_ItemSandbox | 42/ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0 | 42/ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Editor_ItemSandbox | 56/08b714edfa6394aaa3d51786ea867ea6eb2ac00a4a0330ff720886efbadb4f23 | 56/08b714edfa6394aaa3d51786ea867ea6eb2ac00a4a0330ff720886efbadb4f23 | PASS |
| protected.baseline.Assets__Game_Resources_item | 294/5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29 | 294/5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29 | PASS |
| protected.baseline.Assets__Game_Resources_item_daoju | 198/bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f | 198/bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f | PASS |
| protected.baseline.Assets__Game_Scenes | 14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b | 14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b | PASS |
| protected.baseline.Assets__Game_Prefabs | 16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 | 16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_EnemySystem | 89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4 | 89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem | 22/e829300198330c9aedfc66dc22aa03ed471ef114fbc1517abbd59b3d736078ec | 22/e829300198330c9aedfc66dc22aa03ed471ef114fbc1517abbd59b3d736078ec | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_CrossSystem | 5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1 | 5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1 | PASS |
| protected.baseline.Assets__Game_Scripts_TalismanBag_Editor_CrossSystem | 9/fc3c4b78fa84c7dec550b1aabe05b0f2f8cd0178166dd5621224ae6a34395017 | 9/fc3c4b78fa84c7dec550b1aabe05b0f2f8cd0178166dd5621224ae6a34395017 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.AutoCombatController | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.TalismanItemRuntime | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.MonoBehaviour | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.UnityEngine | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.TalismanBag_EnemySystem | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.TalismanBag_CrossSystem | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.BuildCapability | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.ItemSystemSnapshot | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.RunFlowController | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.SaveData | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.RewardConfig | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs.RectTransform | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.AutoCombatController | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.TalismanItemRuntime | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.MonoBehaviour | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.UnityEngine | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.TalismanBag_EnemySystem | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.TalismanBag_CrossSystem | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.BuildCapability | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.ItemSystemSnapshot | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.RunFlowController | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.SaveData | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.RewardConfig | 0 | 0 | PASS |
| leak.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs.RectTransform | 0 | 0 | PASS |
| package.file.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_meta | exists | Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts.meta | PASS |
| package.file.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs | exists | Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs | PASS |
| package.file.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactContract_cs_meta | exists | Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs.meta | PASS |
| package.file.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs | exists | Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactValidator.cs | PASS |
| package.file.Assets__Game_Scripts_TalismanBag_Items_Capability_RuntimeFacts_ItemCapabilityRuntimeFactValidator_cs_meta | exists | Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactValidator.cs.meta | PASS |
| package.file.Assets__Game_Scripts_TalismanBag_Editor_ItemCapability_ItemCapabilityRuntimeFactContractVerifier_cs | exists | Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityRuntimeFactContractVerifier.cs | PASS |
| package.file.Assets__Game_Scripts_TalismanBag_Editor_ItemCapability_ItemCapabilityRuntimeFactContractVerifier_cs_meta | exists | Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemCapabilityRuntimeFactContractVerifier.cs.meta | PASS |
| package.file.Docs_V0_4_Reports_ItemCapabilityRuntimeFactContractReport_md | exists | Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractReport.md | PASS |
| package.file.Docs_V0_4_Reports_ItemCapabilityRuntimeFactContractSpec_csv | exists | Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractSpec.csv | PASS |
| package.file.Docs_V0_4_Reports_ItemCapabilityRuntimeFactContractLeakCheckReport_md | exists | Docs/V0.4/Reports/ItemCapabilityRuntimeFactContractLeakCheckReport.md | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemInstancePlacementBindingContract_cs | 1/335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0 | 1/335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemInstancePlacementBindingValidator_cs | 1/bda2f04b28eff93d6a16d585fd65251607c9964c4bd26d3d594f6f1131fa0d4c | 1/bda2f04b28eff93d6a16d585fd65251607c9964c4bd26d3d594f6f1131fa0d4c | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemCapabilityUnitContract_cs | 1/cc40035b945c203a6aaf3b0426200e925703d5f51e9b6c54435f8d6d08ca51b7 | 1/cc40035b945c203a6aaf3b0426200e925703d5f51e9b6c54435f8d6d08ca51b7 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Capability_ItemCapabilityUnitValidator_cs | 1/77e52c84fac2abdb71c5f346e92cdf759a8b5ea5319a142e53cd983c13667e3c | 1/77e52c84fac2abdb71c5f346e92cdf759a8b5ea5319a142e53cd983c13667e3c | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_ItemSystemSnapshot_cs | 1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df | 1/821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Generation_Projection_ItemInstanceProjectionContract_cs | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | 1/f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Generation_Affixes_ItemAffixPoolAndRangeSchema_cs | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | 1/3e2cd03d5a7a1d48adf803bed07cabd75ba5041cb888f79a03857627bf9085db | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Balance_ItemCompleteCandidateContent_cs | 1/c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb | 1/c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb | PASS |
| protected.before-after.Assets__Game_Configs_ItemBalanceWorkbench_ItemBalanceWorkbenchCatalog_asset | 1/5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45 | 1/5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45 | PASS |
| protected.before-after.ProjectSettings_EditorBuildSettings_asset | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | 1/08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 | PASS |
| protected.before-after.Assets__Game_Configs_ItemBalanceWorkbench_Profiles | 60/4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317 | 60/4f49f9d6fc4d656a4ebb3ec0fcf0f20eb385417c5a342d31f8b0165eecedd317 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Items_Detail | 23/213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4 | 23/213ae49cd9be294b428ea0446da5bccb744606a76972b318798654680ceb72c4 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_ItemSandbox | 42/ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0 | 42/ecdcd919d1100de6473318cf1efb617914fdff6fb5aefc595467a957b78059a0 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Editor_ItemSandbox | 56/08b714edfa6394aaa3d51786ea867ea6eb2ac00a4a0330ff720886efbadb4f23 | 56/08b714edfa6394aaa3d51786ea867ea6eb2ac00a4a0330ff720886efbadb4f23 | PASS |
| protected.before-after.Assets__Game_Resources_item | 294/5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29 | 294/5f1c20dc01c2a9dabe15fddde9ec832b2d19f6c504a43f76e083399b02beee29 | PASS |
| protected.before-after.Assets__Game_Resources_item_daoju | 198/bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f | 198/bb7d6b3ab5eb6d761ff04363a9fe4d63859b29aad8f1e509c8752b3be663665f | PASS |
| protected.before-after.Assets__Game_Scenes | 14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b | 14/da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b | PASS |
| protected.before-after.Assets__Game_Prefabs | 16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 | 16/7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_EnemySystem | 89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4 | 89/3d75b42a7a940c244bedbbf97cddb04e7f19d28c4510a5ff321ac1c85f68e3c4 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem | 22/e829300198330c9aedfc66dc22aa03ed471ef114fbc1517abbd59b3d736078ec | 22/e829300198330c9aedfc66dc22aa03ed471ef114fbc1517abbd59b3d736078ec | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_CrossSystem | 5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1 | 5/d0ee419cd416b61a9e43173d45a802e8e87e35d5dd21054d863a73cbdd207dd1 | PASS |
| protected.before-after.Assets__Game_Scripts_TalismanBag_Editor_CrossSystem | 9/fc3c4b78fa84c7dec550b1aabe05b0f2f8cd0178166dd5621224ae6a34395017 | 9/fc3c4b78fa84c7dec550b1aabe05b0f2f8cd0178166dd5621224ae6a34395017 | PASS |
