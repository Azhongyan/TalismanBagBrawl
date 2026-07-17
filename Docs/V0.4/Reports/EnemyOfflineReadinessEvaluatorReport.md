# Enemy Offline Readiness Evaluator Report

- Package: `V0.4-EnemyOfflineReadinessEvaluator01`
- Mode: `Pure C# same-source offline verifier`
- Result: `PASS`
- Checks: `212/212 PASS`
- Scenarios: `15`
- ReadinessBand distribution: `Unknown=3, Blocked=4, Strained=2, Ready=4, Strong=2`
- Required / Recommended coverage: `PASS`
- Any / All coverage: `PASS`
- Known Zero / Unknown coverage: `PASS`
- Map buff / debuff / low-high clamp / order coverage: `PASS`
- DeveloperCanonicalSignature sample: `sha256:62d9ab21f62994fa36c1f3dc087192141c0fa71cd8dd0717058a99ee67c993c1`
- PlayerSafeCanonicalSignature sample: `sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9`
- Player leak check: `PASS`
- Runtime forbidden dependency check: `PASS`
- E01-E07 protected hash: `33/33 PASS`
- Legacy source hash: `3/3 PASS`
- Global git diff --check: `PREEXISTING_UNRELATED_DIFF`

## Scenario summary

| Scenario | Coverage | Band | Severity | Developer signature | Player signature |
|---|---|---|---|---|---|
| strong | Complete | Strong | None | `sha256:62d9ab21f62994fa36c1f3dc087192141c0fa71cd8dd0717058a99ee67c993c1` | `sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9` |
| ready | Sparse | Ready | None | `sha256:5c81c0a18acd7a42905d17e1774ccd8cf194351428ce3a8ee11a34ef683c29c5` | `sha256:f3433b34b821deed36d5077cc50969f761561ac76779714ff774926483f86923` |
| strained | Sparse | Strained | Warning | `sha256:3313ea9e5e5567f48006d71ee40f1daaed9acd1d10019595614f19e5641bb006` | `sha256:b497687d09a0c6622b56d0c5d72b3edf027b6aa98200a5c39c62fb45fd22d540` |
| blocked_known_zero | Sparse | Blocked | Critical | `sha256:e4f122e57eedc63f772c55a5daf978dd8b917a75d11fc735904eb28eb98c64ce` | `sha256:a4c96590ddaf67f1fe7d43d6ae1064eb30e08bf44f421950ed19bc57f6ed8c72` |
| unknown_sparse | Sparse | Unknown | Notice | `sha256:3f8aa034b0f996855c24d0cbd7781c75c49b0151de6a51403b3b09175ba74e59` | `sha256:abf3805001362d6eb0fdf937bb5aee243b78779afed18f0bc14f7352e98bec7f` |
| any_alternative | Sparse | Ready | None | `sha256:80160018ed343ef416436096e4f53de28ff42885edcf68e74603b799a4d62cf9` | `sha256:6d5b4129182453ceffcad497c065ff074209bb98445d8372bbcc17549e53aa5d` |
| any_unknown | Sparse | Unknown | Notice | `sha256:faab5848654b8c404c73fc47b45e4d9a3101bc3727066b0f183d37f9b35e9712` | `sha256:368408803a3ab48a9f7db7bfeceb78bc63747e684e95f6e67df4f655bfdfdabf` |
| all_unmet | Sparse | Blocked | Critical | `sha256:e7c3e407d5efeea76d90b5d320a3addd6081c2236bb1000a3f6e6a76e2abb5fa` | `sha256:7cf02c1226277a55412bb85d1a615bee95c14282efa51c08ad5b9168582e45ab` |
| map_buff | Sparse | Ready | None | `sha256:308228d0fcafee94b6e72b874b8088a398d91f18465d430db0598b17f5bac109` | `sha256:5fa7545705b3cffbebd39962576fa2445c001783635614ab08fe989afb77e1d7` |
| map_debuff | Sparse | Blocked | Critical | `sha256:02414c4255dfe07bb100fd630edf3653f4c8d10e4f8a07691d4fa642cbc5e500` | `sha256:f1e1f23de0900aff8efd7010b439990a9a81dcebfaf7a3a997794c0d5bd1af57` |
| clamp_low | Sparse | Blocked | Critical | `sha256:34ec5fb5b490c8bb32d3c8dcf47bbdfd777b575e17c291cf8f92f31bc396685d` | `sha256:3fa5e51f595511909bd256d839e4ae4ea5c9f30cbf5ef1675ae4bc0d3215d09f` |
| clamp_high | Sparse | Ready | None | `sha256:5b9c86c257a02c73ed4e03c91f7019ae2df8663503e2e6aa23e94d6df9d48e30` | `sha256:3b2d611241fa669ff7f43981261233a760911da6c20591418e7f3ce163bc6e13` |
| unknown_map | Sparse | Unknown | Notice | `sha256:eafd5237ab346d3dc389cbd38a96d2640fbcf2a8efd346ef998d7df360357564` | `sha256:1a5e5114037e51347c656c6939a9c14611edb870bbc44604e5197d1597798d84` |
| recommended_only_strong | Sparse | Strong | None | `sha256:071b25684ac710fbc9ace1ba4caa9f9e1cf728cbeb27011214b20be0ad371de2` | `sha256:d449c489cb77bb43874e9400ea1d9f2d90ed543b0d66b511a4e89fb59ccb611b` |
| recommended_only_strained | Sparse | Strained | Warning | `sha256:d16a1eb3d6b4329ab97b94c0a8af43b5d638aad0429d1632058f50c6f811280f` | `sha256:2c1e43fe5470046fa03d73a0587546ad9bfb82ac5bc6b4cfb6fe14bd9a7ec7fd` |

## Detailed checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| `schema.id` | EnemyOfflineReadiness.v1 | EnemyOfflineReadiness.v1 | PASS |
| `schema.version` | 1 | 1 | PASS |
| `scenarios.count` | >=13 | 15 | PASS |
| `scenario.strong.band` | Strong | Strong | PASS |
| `scenario.strong.severity` | None | None | PASS |
| `scenario.ready.band` | Ready | Ready | PASS |
| `scenario.ready.severity` | None | None | PASS |
| `scenario.strained.band` | Strained | Strained | PASS |
| `scenario.strained.severity` | Warning | Warning | PASS |
| `scenario.blocked_known_zero.band` | Blocked | Blocked | PASS |
| `scenario.blocked_known_zero.severity` | Critical | Critical | PASS |
| `scenario.unknown_sparse.band` | Unknown | Unknown | PASS |
| `scenario.unknown_sparse.severity` | Notice | Notice | PASS |
| `scenario.any_alternative.band` | Ready | Ready | PASS |
| `scenario.any_alternative.severity` | None | None | PASS |
| `scenario.any_unknown.band` | Unknown | Unknown | PASS |
| `scenario.any_unknown.severity` | Notice | Notice | PASS |
| `scenario.all_unmet.band` | Blocked | Blocked | PASS |
| `scenario.all_unmet.severity` | Critical | Critical | PASS |
| `scenario.map_buff.band` | Ready | Ready | PASS |
| `scenario.map_buff.severity` | None | None | PASS |
| `scenario.map_debuff.band` | Blocked | Blocked | PASS |
| `scenario.map_debuff.severity` | Critical | Critical | PASS |
| `scenario.clamp_low.band` | Blocked | Blocked | PASS |
| `scenario.clamp_low.severity` | Critical | Critical | PASS |
| `scenario.clamp_high.band` | Ready | Ready | PASS |
| `scenario.clamp_high.severity` | None | None | PASS |
| `scenario.unknown_map.band` | Unknown | Unknown | PASS |
| `scenario.unknown_map.severity` | Notice | Notice | PASS |
| `scenario.recommended_only_strong.band` | Strong | Strong | PASS |
| `scenario.recommended_only_strong.severity` | None | None | PASS |
| `scenario.recommended_only_strained.band` | Strained | Strained | PASS |
| `scenario.recommended_only_strained.severity` | Warning | Warning | PASS |
| `coverage.readinessBands` | Unknown;Blocked;Strained;Ready;Strong | Unknown;Blocked;Strained;Ready;Strong | PASS |
| `coverage.playerHintSeverities` | None;Notice;Warning;Critical | None;Notice;Warning;Critical | PASS |
| `semantic.knownZero` | Known/0/Unmet | Known/0/Unmet | PASS |
| `semantic.unknownPlaceholder` | Unknown/0/0/0 | Unknown/0/0/0 | PASS |
| `semantic.anyAlternative` | group Met with Met+Unmet rows | Met/Met;Unmet | PASS |
| `semantic.anyUnknown` | Unknown | Unknown | PASS |
| `semantic.allUnmet` | Unmet | Unmet | PASS |
| `map.clampLow` | 0/-14000 | 0/-14000 | PASS |
| `map.clampHigh` | 10000/14000 | 10000/14000 | PASS |
| `map.unknownPreserved` | Unknown/0/+9000 | Unknown/0/9000 | PASS |
| `map.buffFlip` | Blocked->Ready | Blocked->Ready | PASS |
| `map.debuffFlip` | Ready->Blocked | Ready->Blocked | PASS |
| `semantics.requiredRecommended` | both covered | covered | PASS |
| `semantics.anyAll` | both covered | covered | PASS |
| `signature.repeat.developer` | sha256:62d9ab21f62994fa36c1f3dc087192141c0fa71cd8dd0717058a99ee67c993c1 | sha256:62d9ab21f62994fa36c1f3dc087192141c0fa71cd8dd0717058a99ee67c993c1 | PASS |
| `signature.repeat.player` | sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9 | sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9 | PASS |
| `signature.format.developer` | sha256:+64 lowercase hex | sha256:62d9ab21f62994fa36c1f3dc087192141c0fa71cd8dd0717058a99ee67c993c1 | PASS |
| `signature.format.player` | sha256:+64 lowercase hex | sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9 | PASS |
| `signature.adjustmentOrder.developer` | sha256:5b9c86c257a02c73ed4e03c91f7019ae2df8663503e2e6aa23e94d6df9d48e30 | sha256:5b9c86c257a02c73ed4e03c91f7019ae2df8663503e2e6aa23e94d6df9d48e30 | PASS |
| `signature.adjustmentOrder.player` | sha256:3b2d611241fa669ff7f43981261233a760911da6c20591418e7f3ce163bc6e13 | sha256:3b2d611241fa669ff7f43981261233a760911da6c20591418e7f3ce163bc6e13 | PASS |
| `signature.capabilityValue.developerChanges` | different | sha256:499abbbcdc4358addd8aefd894f6bc7e66d3f2f22a0960049b9e4c372ebdabd6 | PASS |
| `signature.capabilityValue.playerStable` | sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9 | sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9 | PASS |
| `signature.threshold.developerChanges` | different | sha256:f065e6e4e3869934808413ecdba574b9b2b7b4ed17a73eb10127d1e6ef82b135 | PASS |
| `signature.threshold.playerStable` | sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9 | sha256:86296e27b26ecef57ea1cb759ca8ca1380b0d4b50a73372e07d41177b3274ff9 | PASS |
| `signature.mapDelta.developerChanges` | different | sha256:9793b74d2638df26c965c927fe91207c823be4282defdfb4e3de1f36dc133d8b | PASS |
| `signature.mapDelta.playerStable` | sha256:5fa7545705b3cffbebd39962576fa2445c001783635614ab08fe989afb77e1d7 | sha256:5fa7545705b3cffbebd39962576fa2445c001783635614ab08fe989afb77e1d7 | PASS |
| `signature.groupSemantics.developerChanges` | different | sha256:7cf11d0fc1282e3fc6ac68d0a69f423b48f119ad1de8407c25c823ddf3f0445e | PASS |
| `signature.groupSemantics.playerStable` | sha256:8d127125f1e02060e3c5e3b88a38d74c0b4dc7409ce0dabb63b9d115ca3bb0ba | sha256:8d127125f1e02060e3c5e3b88a38d74c0b4dc7409ce0dabb63b9d115ca3bb0ba | PASS |
| `signature.publicHint.developerChanges` | different | sha256:ee2928b3d1978655764853cdd5778048b64ef92439e43ae75305f9d82ae2f98c | PASS |
| `signature.publicHint.playerChanges` | different | sha256:68cc6d1dfedb675fa293e8737b404fdaa8e4aa4bdf44c58e51e437948b4ac5e5 | PASS |
| `signature.severityBoundary.playerChanges` | different | sha256:28df1557d9becf587de497ae73b5e7bfa11fd3a7071b815b3d880ae1de30275e | PASS |
| `immutability.input.adjustments` | read-only | checked | PASS |
| `immutability.output.groups` | read-only | checked | PASS |
| `immutability.output.rows` | read-only | checked | PASS |
| `immutability.output.playerHints` | read-only | checked | PASS |
| `immutability.noSharedMutableRows` | different instances | checked | PASS |
| `validation.nullInput` | INPUT_NULL | INPUT_NULL | PASS |
| `validation.nullResolver` | RESOLVER_NULL | RESOLVER_NULL | PASS |
| `validation.nullBuild` | BUILD_SNAPSHOT_NULL | BUILD_SNAPSHOT_NULL | PASS |
| `validation.nullPressureCatalog` | PRESSURE_CATALOG_NULL | PRESSURE_CATALOG_NULL | PASS |
| `validation.emptyProfileId` | ID_EMPTY | ID_EMPTY;PRESSURE_PROFILE_UNRESOLVED | PASS |
| `validation.profileOuterWhitespace` | ID_OUTER_WHITESPACE | ID_OUTER_WHITESPACE;PRESSURE_PROFILE_UNRESOLVED | PASS |
| `validation.profileUnresolved` | PRESSURE_PROFILE_UNRESOLVED | PRESSURE_PROFILE_UNRESOLVED | PASS |
| `validation.nullAdjustment` | MAP_ADJUSTMENT_NULL | MAP_ADJUSTMENT_NULL | PASS |
| `validation.zeroDelta` | MAP_DELTA_OUT_OF_RANGE | MAP_DELTA_OUT_OF_RANGE | PASS |
| `validation.deltaAboveRange` | MAP_DELTA_OUT_OF_RANGE | MAP_DELTA_OUT_OF_RANGE | PASS |
| `validation.reasonPath` | DEVELOPER_REASON_PATH_FORBIDDEN | DEVELOPER_REASON_PATH_FORBIDDEN | PASS |
| `validation.duplicateMapCapability` | MAP_ADJUSTMENT_DUPLICATE | MAP_ADJUSTMENT_DUPLICATE | PASS |
| `validation.mapRuleUnresolved` | MAP_RULE_UNRESOLVED | MAP_RULE_UNRESOLVED | PASS |
| `validation.capabilityUnresolved` | MAP_CAPABILITY_KEY_UNRESOLVED | MAP_CAPABILITY_KEY_UNRESOLVED | PASS |
| `validation.schemaGuardsPresent` | E06+E07 schema guards | scanned | PASS |
| `validation.isolationGuardsPresent` | build+pressure isolation guards | scanned | PASS |
| `baseline.e02.totalVocabulary` | 63 | 63 | PASS |
| `baseline.e02.buildCapability` | 16 | 16 | PASS |
| `baseline.e03.carriers` | 11/7/18 | 11/7/18 | PASS |
| `baseline.e03.mechanicProfiles` | 10/6/16 | 10/6/16 | PASS |
| `baseline.e03.mapRules` | 10 | 10 | PASS |
| `baseline.e03.carrierBindings` | 17 | 17 | PASS |
| `baseline.e03.mapRuleBindings` | 30 | 30 | PASS |
| `baseline.e03.exceptions` | 2 | 2 | PASS |
| `baseline.e04.fixture` | 2/4/6 | 2/4/6 | PASS |
| `baseline.e05.fixture` | 4/3/3/3/2 | 4/3/3/3/2 | PASS |
| `baseline.e06.fixture` | 4/3/6/4/5/11 | 4/3/6/4/5/11 | PASS |
| `baseline.e07.fixture` | 16/16/4/6/3/5 | 16/16/4/6/3/5 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Domain_EnemyDomainPrimitives_cs` | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Domain_EnemyArchetypeSnapshots_cs` | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Contracts_EnemyDomainReferences_cs` | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Contracts_EnemyDomainSnapshot_cs` | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Contracts_EnemyDomainValidation_cs` | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyDomainDataContractVerifier_cs` | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Vocabulary_EnemyVocabularyModel_cs` | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Vocabulary_DefaultEnemyMechanicVocabularyCatalog_cs` | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Vocabulary_EnemyVocabularyValidation_cs` | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyMechanicVocabularyVerifier_cs` | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Normalization_EnemyValidationContentSnapshot_cs` | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Normalization_EnemyValidationContentNormalizerCore_cs` | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyValidationContentNormalizer_cs` | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyValidationContentNormalizeVerifier_cs` | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Composition_EncounterCompositionPrimitives_cs` | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Composition_EncounterCompositionSnapshots_cs` | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Composition_EncounterCompositionValidation_cs` | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EncounterCompositionSchemaVerifier_cs` | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_EnemySkillBossPhasePrimitives_cs` | 4F08886ABE8E6E7C9988E515A4CDDDD09D413001EF0B07E0FAFF3DADFE6FB6B6 | 4F08886ABE8E6E7C9988E515A4CDDDD09D413001EF0B07E0FAFF3DADFE6FB6B6 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_EnemySkillPatternSnapshots_cs` | 22A2FD124F32908141B93EB964D5DD1B3F099EF44BE72C9F367EF86557A2F499 | 22A2FD124F32908141B93EB964D5DD1B3F099EF44BE72C9F367EF86557A2F499 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_BossPhaseSnapshots_cs` | B2FD5AA3367A8950D96F57AFC89C134B89A13A0557B55FDA4593DC78452F70F2 | B2FD5AA3367A8950D96F57AFC89C134B89A13A0557B55FDA4593DC78452F70F2 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_EnemySkillBossPhaseValidation_cs` | A175DF86360A993E9EE334D0EEE19C47725EB4CAED6BE613AA2788CD67CFB959 | A175DF86360A993E9EE334D0EEE19C47725EB4CAED6BE613AA2788CD67CFB959 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemySkillBossPhaseSchemaVerifier_cs` | 3C7892A7A73A1464EE2D797B8F617EBA1639B1C7C6D4330FC166F64B32DAB267 | 3C7892A7A73A1464EE2D797B8F617EBA1639B1C7C6D4330FC166F64B32DAB267 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_CounterWindowAndPressurePrimitives_cs` | E0A5C694D1039552F5C7E24CE599B71B7CB08B29AA39F11F6B134552B4505687 | E0A5C694D1039552F5C7E24CE599B71B7CB08B29AA39F11F6B134552B4505687 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_BuildPressureSnapshots_cs` | 56CC65BADAF89BE6882629AE847FCF03C2D0BAC3D800BDB562DE66E275E4D1C1 | 56CC65BADAF89BE6882629AE847FCF03C2D0BAC3D800BDB562DE66E275E4D1C1 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_CounterWindowSnapshots_cs` | 5DD48CA0F0B84A44B7356EFFAFCC03C879200DB5C5030D27E00BEC15D24921C6 | 5DD48CA0F0B84A44B7356EFFAFCC03C879200DB5C5030D27E00BEC15D24921C6 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_CounterWindowAndPressureValidation_cs` | 1526BAF50EC3A59914949A152FA48B659FE898A836EAD13D947CA905D17485AE | 1526BAF50EC3A59914949A152FA48B659FE898A836EAD13D947CA905D17485AE | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_CounterWindowAndPressureSchemaVerifier_cs` | 2FA48E6D1A17AE7B6204655FF5F58AE1D304C2315F1CBA9AA0A7F91D9CF99DD4 | 2FA48E6D1A17AE7B6204655FF5F58AE1D304C2315F1CBA9AA0A7F91D9CF99DD4 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_BuildCapabilityReadPrimitives_cs` | 7A97F62EC9539A4F8D3C43FCF893854FB408A4A0950B40635884BE06681B62C4 | 7A97F62EC9539A4F8D3C43FCF893854FB408A4A0950B40635884BE06681B62C4 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_BuildCapabilitySnapshots_cs` | EA2C7DF08CB2798C2D0DE401A564D3EC80B0C3F7DFF67334AE4856FB021A28BC | EA2C7DF08CB2798C2D0DE401A564D3EC80B0C3F7DFF67334AE4856FB021A28BC | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_ReadinessDiagnosticSnapshots_cs` | F689F732BFABA19A7EDE16EF694857AAC86205195D84BE828983381D882C5D51 | F689F732BFABA19A7EDE16EF694857AAC86205195D84BE828983381D882C5D51 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_BuildCapabilityReadValidation_cs` | B21881430918157951A17BE4C76AC4A88D04626C0A4A549959F2A633DA61D5B6 | B21881430918157951A17BE4C76AC4A88D04626C0A4A549959F2A633DA61D5B6 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_BuildCapabilityReadContractVerifier_cs` | F1AB9863BC777D221E073294BD2D3478DBF416617BA7F9E3534A7882440A5D34 | F1AB9863BC777D221E073294BD2D3478DBF416617BA7F9E3534A7882440A5D34 | PASS |
| `legacy.Assets__Game_Scripts_TalismanBag_BuildSandbox_EnemyBossValidationPool_cs` | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | PASS |
| `legacy.Assets__Game_Scripts_TalismanBag_BuildSandbox_BuildProblemRuleConfigs_cs` | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | PASS |
| `legacy.Assets__Game_Scripts_TalismanBag_BuildSandbox_BuildProblemSeedData_cs` | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | PASS |
| `protected.count` | 33/33 | 33/33 | PASS |
| `legacy.count` | 3/3 | 3/3 | PASS |
| `leak.runtime.TalismanBag_BuildSandbox` | 0 | 0 | PASS |
| `leak.runtime.TalismanBag_Items` | 0 | 0 | PASS |
| `leak.runtime.ItemSystemSnapshot` | 0 | 0 | PASS |
| `leak.runtime.Inventory` | 0 | 0 | PASS |
| `leak.runtime.Equipment` | 0 | 0 | PASS |
| `leak.runtime.Affix` | 0 | 0 | PASS |
| `leak.runtime.Synergy` | 0 | 0 | PASS |
| `leak.runtime.using_UnityEngine` | 0 | 0 | PASS |
| `leak.runtime.MonoBehaviour` | 0 | 0 | PASS |
| `leak.runtime.ScriptableObject` | 0 | 0 | PASS |
| `leak.runtime.GameObject` | 0 | 0 | PASS |
| `leak.runtime.Transform` | 0 | 0 | PASS |
| `leak.runtime.Addressables` | 0 | 0 | PASS |
| `leak.runtime.SceneManager` | 0 | 0 | PASS |
| `leak.runtime.BattleContract` | 0 | 0 | PASS |
| `leak.runtime.BattleBridge` | 0 | 0 | PASS |
| `leak.runtime.UnifiedBattlePage` | 0 | 0 | PASS |
| `leak.runtime.BuildGridInteractionPreviewController` | 0 | 0 | PASS |
| `leak.runtime.V02FormationGridFrame` | 0 | 0 | PASS |
| `leak.runtime.RunFlow` | 0 | 0 | PASS |
| `leak.runtime.RewardConfig` | 0 | 0 | PASS |
| `leak.runtime.SaveData` | 0 | 0 | PASS |
| `leak.runtime.PlayerPrefs` | 0 | 0 | PASS |
| `leak.runtime.DamageText` | 0 | 0 | PASS |
| `leak.runtime.DateTime_Now` | 0 | 0 | PASS |
| `leak.runtime.Stopwatch` | 0 | 0 | PASS |
| `leak.runtime.Random` | 0 | 0 | PASS |
| `leak.runtime.Task_Delay` | 0 | 0 | PASS |
| `leak.runtime.Coroutine` | 0 | 0 | PASS |
| `leak.runtime.StartCoroutine` | 0 | 0 | PASS |
| `leak.runtime.void_Update_` | 0 | 0 | PASS |
| `leak.runtime._event_` | 0 | 0 | PASS |
| `leak.runtime.ApplyDamage` | 0 | 0 | PASS |
| `leak.runtime.DealDamage` | 0 | 0 | PASS |
| `leak.runtime.DropBias` | 0 | 0 | PASS |
| `leak.runtime.BossKey` | 0 | 0 | PASS |
| `leak.runtime.3_10` | 0 | 0 | PASS |
| `leak.runtime.4_10` | 0 | 0 | PASS |
| `leak.runtime.total` | 0 | 0 | PASS |
| `leak.editorLegacyReaderTotal` | 1 | 1 | PASS |
| `leak.playerPropertyShape` | BuildPressureProfileId;PlayerHintCategoryKeys;PlayerHintSeverity;PlayerSafeCanonicalSignature;PublicPressureHintKey;PublicPressureLabelKey | BuildPressureProfileId;PlayerHintCategoryKeys;PlayerHintSeverity;PlayerSafeCanonicalSignature;PublicPressureHintKey;PublicPressureLabelKey | PASS |
| `leak.player.readinessBand` | 0 | 0 | PASS |
| `leak.player.buildCapabilityKey` | 0 | 0 | PASS |
| `leak.player.baseValueBasisPoints` | 0 | 0 | PASS |
| `leak.player.effectiveValueBasisPoints` | 0 | 0 | PASS |
| `leak.player.requiredValueBasisPoints` | 0 | 0 | PASS |
| `leak.player.gapBasisPoints` | 0 | 0 | PASS |
| `leak.player.requirementGroupId` | 0 | 0 | PASS |
| `leak.player.requirementRole` | 0 | 0 | PASS |
| `leak.player.requirementMatchMode` | 0 | 0 | PASS |
| `leak.player.requirementStatus` | 0 | 0 | PASS |
| `leak.player.groupStatus` | 0 | 0 | PASS |
| `leak.player.capabilityValueAvailability` | 0 | 0 | PASS |
| `leak.player.mapRuleId` | 0 | 0 | PASS |
| `leak.player.deltaBasisPoints` | 0 | 0 | PASS |
| `leak.player.developerReasonId` | 0 | 0 | PASS |
| `leak.player.buildSnapshotId` | 0 | 0 | PASS |
| `leak.player.pressureCatalogCanonicalSignature` | 0 | 0 | PASS |
| `leak.player.sourceSummary` | 0 | 0 | PASS |
| `leak.player.hardSolutionTags` | 0 | 0 | PASS |
| `leak.player.requiredSynergy` | 0 | 0 | PASS |
| `leak.player.requiredAffix` | 0 | 0 | PASS |
| `leak.player.requiredStats` | 0 | 0 | PASS |
| `leak.player.dropBias` | 0 | 0 | PASS |
| `leak.player.bossKey` | 0 | 0 | PASS |
| `leak.player.total` | 0 | 0 | PASS |
| `package.files` | 16/16 | 16/16 | PASS |
| `package.pathWhitelist` | PASS | unexpected=0,missing=0 | PASS |
| `package.trailingWhitespace` | 0 | 0 | PASS |
| `package.guid.ReadinessEvaluation.meta` | unique | 2a4f3f4a4e174f32b787d1cd36b3e901 x1 | PASS |
| `package.guid.EnemyReadinessPrimitives.cs.meta` | unique | 8d91bd52a95846b9af3761eb5d882812 x1 | PASS |
| `package.guid.EnemyReadinessInputSnapshots.cs.meta` | unique | 5b499f29424445d3a9e21f4182fc6e26 x1 | PASS |
| `package.guid.EnemyReadinessResultSnapshots.cs.meta` | unique | c1d775c7a2ce481996b0f26815ee7eb8 x1 | PASS |
| `package.guid.EnemyOfflineReadinessEvaluator.cs.meta` | unique | 13f5277299b94dc390de3248ed3a7a8f x1 | PASS |
| `package.guid.EnemyOfflineReadinessEvaluatorVerifier.cs.meta` | unique | 97b63e8aa6f84f31946c40828aeadc85 x1 | PASS |
| `package.guidConflicts` | 0 | 0 | PASS |
| `package.globalDiffCheck` | PASS or PREEXISTING_UNRELATED_DIFF | PREEXISTING_UNRELATED_DIFF | PASS |
