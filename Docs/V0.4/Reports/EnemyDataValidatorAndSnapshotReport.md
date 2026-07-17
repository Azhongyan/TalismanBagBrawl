# Enemy Data Validator And Snapshot Report

- Package: `V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02`
- Guard marker: `ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX02`
- Mode: `unity-batch`
- Result: `PASS`
- Checks: `220/220 PASS`
- Manifest: `8/8`; static: `6`; transient: `2`
- E07/E08 embedded: `0/0`
- Full signature: `sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af`
- Player-safe signature: `sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758`
- Unresolved relations: `0`
- Identity duplicates: `0`
- Relation duplicates: `0`
- Intentional exceptions: `2 accepted`
- Player leaks: `0`
- Formal flow references: `0`
- E01-E08 protected hash: `38/38`
- Legacy hash: `3/3`
- Frozen E09 hash: `11/11`
- Relation-kind coverage: `17/17`
- Deterministic reports: `7/7 hash identical`
- Global diff check: `PREEXISTING_UNRELATED_DIFF`

## Identity counts

| IdentityKind | Count | Duplicate |
|---|---:|---:|
| Enemy | 11 | 0 |
| Boss | 7 | 0 |
| MechanicProfile | 16 | 0 |
| MapRule | 10 | 0 |
| Encounter | 2 | 0 |
| SkillPattern | 4 | 0 |
| SkillSequence | 3 | 0 |
| BossPhase | 3 | 0 |
| BossPhasePlan | 2 | 0 |
| BuildPressureProfile | 4 | 0 |
| CounterWindow | 3 | 0 |

## Relation counts

| RelationKind | Count | Unresolved | Isolation mismatch |
|---|---:|---:|---:|
| CarrierMechanicProfile | 0 | 0 | 0 |
| CarrierSkillPattern | 0 | 0 | 0 |
| BossPhaseReference | 0 | 0 | 0 |
| OptionalRegistryReference | 26 | 0 | 0 |
| NormalizedCarrierMechanicProfile | 17 | 0 | 0 |
| MapRuleMechanicProfile | 30 | 0 | 0 |
| EncounterMapRule | 3 | 0 | 0 |
| EncounterSlotCarrier | 6 | 0 | 0 |
| EncounterSlotMechanicProfile | 5 | 0 | 0 |
| SkillPatternMechanicProfile | 4 | 0 | 0 |
| SkillSequencePattern | 5 | 0 | 0 |
| CarrierSkillSequence | 4 | 0 | 0 |
| BossPhaseSequence | 3 | 0 | 0 |
| BossPlanPhase | 6 | 0 | 0 |
| PressureSourceTarget | 6 | 0 | 0 |
| CounterWindowSourceTarget | 4 | 0 | 0 |
| PressureCounterWindow | 5 | 0 | 0 |

## Coverage proofs

- many-to-many: `1/2/10/13/2/2`
- intentional exceptions: `2 accepted`
- legal unused catalog entries: `13 accepted`
- all relation kinds: `17/17`
- transient complete/sparse readiness fixtures: `Complete/Sparse/2`
- input immutability: `6/6 rejected; sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af`
- root collection immutability: `3/3 rejected; 197`
- player collection immutability: `8/8 rejected; 58`

## Signature mutation matrix

| Mutation | Full root | Player-safe root | Result |
|---|---|---|---|
| E03 developer-only | changes | stable | PASS |
| E05 developer-only | changes | stable | PASS |
| E06 developer-only | changes | stable | PASS |
| E03 player-safe | changes | changes | PASS |
| E05 player-safe | changes | changes | PASS |
| E06 player-safe | changes | changes | PASS |
| E04 composition only | changes | stable | PASS |
| E04 valid relation target | changes | stable | PASS |
| input sorting | stable | stable | PASS |

## Detailed checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| `baseline.e02.capability` | 16 | 16 | PASS |
| `baseline.e02.total` | 63 | 63 | PASS |
| `baseline.e03.carrierBindings` | 17 | 17 | PASS |
| `baseline.e03.carriers` | 11/7/18 | 11/7/18 | PASS |
| `baseline.e03.exceptions` | 2 | 2 | PASS |
| `baseline.e03.mapBindings` | 30 | 30 | PASS |
| `baseline.e03.mapRules` | 10 | 10 | PASS |
| `baseline.e03.profiles` | 10/6/16 | 10/6/16 | PASS |
| `baseline.e04` | 2/4/6 | 2/4/6 | PASS |
| `baseline.e05` | 4/3/3/3/2 | 4/3/3/3/2 | PASS |
| `baseline.e06` | 4/3/6/4/5/11 | 4/3/6/4/5/11 | PASS |
| `baseline.e07` | 16/16/4/6/3/5 | 16/16/4/6/3/5 | PASS |
| `baseline.e08` | 15 scenarios/212 checks | True/True | PASS |
| `baseline.editorLegacyReader` | 1 | 1 | PASS |
| `coverage.intentionalExceptions` | 2 accepted | 2 accepted | PASS |
| `coverage.legalUnused` | >0 accepted | 13 accepted | PASS |
| `coverage.manyToMany` | all six >0 | 1/2/10/13/2/2 | PASS |
| `coverage.relationKinds` | 17/17 | 17/17 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyDataValidatorAndSnapshotVerifier_cs_meta` | 08CE86C7875791F46DD2D47E2F385DAB0D96C9C98273F49652D5AC6795CACFE7 | 08CE86C7875791F46DD2D47E2F385DAB0D96C9C98273F49652D5AC6795CACFE7 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemDataValidation_cs` | A07FE388D1D7FCDE057FAE984EE8FF096886A1306D21C544253B4E139A8C3013 | A07FE388D1D7FCDE057FAE984EE8FF096886A1306D21C544253B4E139A8C3013 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemDataValidation_cs_meta` | 75095C083308857590BF4E186534054E8497C0CA6510017CEFE7B0B84C24FAAC | 75095C083308857590BF4E186534054E8497C0CA6510017CEFE7B0B84C24FAAC | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemPlayerSafeSnapshot_cs` | 3E03B20171403BF449510B4D539F9E5716B9EAAE1A58AFE68B4EBC2882B15E59 | 3E03B20171403BF449510B4D539F9E5716B9EAAE1A58AFE68B4EBC2882B15E59 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemPlayerSafeSnapshot_cs_meta` | 73DD2DBB77096FC5DC77FBFE8B49B4AF177B8801C1F239F05AC6DEA77D8DBB16 | 73DD2DBB77096FC5DC77FBFE8B49B4AF177B8801C1F239F05AC6DEA77D8DBB16 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemSnapshotInput_cs` | 204A1866A5F9957A6BDD16FA2BD755EECB82D3826CDDF138E44F47209004E11F | 204A1866A5F9957A6BDD16FA2BD755EECB82D3826CDDF138E44F47209004E11F | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemSnapshotInput_cs_meta` | 5378705903B272A2829002E1D360B91BDEE35FDE911423DCB5A8AA0092B8BF95 | 5378705903B272A2829002E1D360B91BDEE35FDE911423DCB5A8AA0092B8BF95 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemSnapshotPrimitives_cs` | F1339B6CEB6DF67EAFBE4D011B5F1F069755B8538B4071F6F45D57DDFD54C71D | F1339B6CEB6DF67EAFBE4D011B5F1F069755B8538B4071F6F45D57DDFD54C71D | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemSnapshotPrimitives_cs_meta` | DD000CA48485A1A17F3ABC4DB1DBD460219918F0BCB48B66E4F74FE2A4BA06D3 | DD000CA48485A1A17F3ABC4DB1DBD460219918F0BCB48B66E4F74FE2A4BA06D3 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_EnemySystemSnapshot_cs_meta` | 23F69B25352C988265AD082C411C5CA4024604E18573C2683BC802672F615013 | 23F69B25352C988265AD082C411C5CA4024604E18573C2683BC802672F615013 | PASS |
| `frozenE09.Assets__Game_Scripts_TalismanBag_EnemySystem_SystemSnapshot_meta` | A409A8735463B89DB4B9A17D5BBC8F179517B026E23B958B12EFC3485C104574 | A409A8735463B89DB4B9A17D5BBC8F179517B026E23B958B12EFC3485C104574 | PASS |
| `frozenE09.total` | 11/11 | 11/11 | PASS |
| `identity.counterWindowOwner` | E06 only | E06 | PASS |
| `identity.counterWindowTypeCollision` | accepted | accepted | PASS |
| `identity.counterWindowVocabulary` | 6 E02 vocabulary / 0 E02 identities | 6/0 | PASS |
| `identity.duplicates` | 0 | 0 | PASS |
| `immutability.input` | 6/6 rejected; signatures and content stable | 6/6 rejected; sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af | PASS |
| `immutability.playerCollections` | 8/8 rejected; signature and content stable | 8/8 rejected; 58 | PASS |
| `immutability.rootCollections` | 3/3 rejected; signatures and content stable | 3/3 rejected; 197 | PASS |
| `isolation.formalFlow` | 0 | 0 | PASS |
| `leak.player.propertyClosure` | BossPhases;Bosses;BuildPressureProfiles;CanonicalSignature;CounterWindows;Enemies;MapRules;MechanicProfiles;SchemaId;SchemaVersion;SkillPatterns | BossPhases;Bosses;BuildPressureProfiles;CanonicalSignature;CounterWindows;Enemies;MapRules;MechanicProfiles;SchemaId;SchemaVersion;SkillPatterns | PASS |
| `leak.player.validator` | 0 | 0 | PASS |
| `leak.root.e07` | absent | absent | PASS |
| `leak.root.e08` | absent | absent | PASS |
| `leak.root.propertyClosure` | CanonicalSignature;CounterWindowAndPressureCatalogSnapshot;DevOnly;EncounterCompositionCatalogSnapshot;EnemyDomainSnapshot;EnemyMechanicVocabularySnapshot;EnemySkillBossPhaseCatalogSnapshot;EnemyValidationContentSnapshot;EntersFormalFlow;IdentityIndex;IsEnabled;PlayerSafe;PlayerSafeCanonicalSignature;RelationIndex;SchemaId;SchemaManifest;SchemaVersion | CanonicalSignature;CounterWindowAndPressureCatalogSnapshot;DevOnly;EncounterCompositionCatalogSnapshot;EnemyDomainSnapshot;EnemyMechanicVocabularySnapshot;EnemySkillBossPhaseCatalogSnapshot;EnemyValidationContentSnapshot;EntersFormalFlow;IdentityIndex;IsEnabled;PlayerSafe;PlayerSafeCanonicalSignature;RelationIndex;SchemaId;SchemaManifest;SchemaVersion | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.Addressables` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.BattleContract` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.Directory_` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.File_` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.GameObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.MonoBehaviour` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.SaveData` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.ScriptableObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.System_IO` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.TalismanBag_Battle` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.TalismanBag_BuildSandbox` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.TalismanBag_Item` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.Transform` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.UnityEditor` | absent | absent | PASS |
| `leak.runtime.EnemySystemDataValidation_cs.UnityEngine` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.Addressables` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.BattleContract` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.Directory_` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.File_` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.GameObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.MonoBehaviour` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.SaveData` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.ScriptableObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.System_IO` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.TalismanBag_Battle` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.TalismanBag_BuildSandbox` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.TalismanBag_Item` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.Transform` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.UnityEditor` | absent | absent | PASS |
| `leak.runtime.EnemySystemPlayerSafeSnapshot_cs.UnityEngine` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.Addressables` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.BattleContract` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.Directory_` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.File_` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.GameObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.MonoBehaviour` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.SaveData` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.ScriptableObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.System_IO` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.TalismanBag_Battle` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.TalismanBag_BuildSandbox` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.TalismanBag_Item` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.Transform` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.UnityEditor` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotInput_cs.UnityEngine` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.Addressables` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.BattleContract` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.Directory_` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.File_` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.GameObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.MonoBehaviour` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.SaveData` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.ScriptableObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.System_IO` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.TalismanBag_Battle` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.TalismanBag_BuildSandbox` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.TalismanBag_Item` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.Transform` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.UnityEditor` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshotPrimitives_cs.UnityEngine` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.Addressables` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.BattleContract` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.Directory_` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.File_` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.GameObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.MonoBehaviour` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.SaveData` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.ScriptableObject` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.System_IO` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.TalismanBag_Battle` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.TalismanBag_BuildSandbox` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.TalismanBag_Item` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.Transform` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.UnityEditor` | absent | absent | PASS |
| `leak.runtime.EnemySystemSnapshot_cs.UnityEngine` | absent | absent | PASS |
| `legacy.Assets__Game_Scripts_TalismanBag_BuildSandbox_BuildProblemRuleConfigs_cs` | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | PASS |
| `legacy.Assets__Game_Scripts_TalismanBag_BuildSandbox_BuildProblemSeedData_cs` | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | PASS |
| `legacy.Assets__Game_Scripts_TalismanBag_BuildSandbox_EnemyBossValidationPool_cs` | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | PASS |
| `legacy.total` | 3/3 | 3/3 | PASS |
| `manifest.count` | 8 | 8 | PASS |
| `manifest.e07e08Embedded` | 0/0 | 0/0 | PASS |
| `manifest.static` | 6 | 6 | PASS |
| `manifest.staticFullSignatures` | 6/6 non-empty | 6/6 non-empty | PASS |
| `manifest.transient` | 2 | 2 | PASS |
| `negative.carrierKindMismatch` | SLOT_CARRIER_KIND_MISMATCH | BOSS_REFERENCE_UNRESOLVED;SLOT_CARRIER_KIND_MISMATCH;MECHANIC_PROFILE_KIND_MISMATCH | PASS |
| `negative.componentMismatch` | E09_DOMAIN_SIGNATURE_MISMATCH | E09_REFERENCE_UNRESOLVED;E09_DOMAIN_SIGNATURE_MISMATCH | PASS |
| `negative.isolationMismatch` | E09_RELATION_ISOLATION_MISMATCH | E09_RELATION_ISOLATION_MISMATCH | PASS |
| `negative.manifestDuplicate` | E09_MANIFEST_DUPLICATE | E09_MANIFEST_DUPLICATE;E09_MANIFEST_MISSING_COMPONENT | PASS |
| `negative.nullComponent` | E09_COMPONENT_NULL | E09_COMPONENT_NULL | PASS |
| `negative.nullInput` | E09_NULL_INPUT | E09_NULL_INPUT | PASS |
| `negative.playerLeak` | rejected upstream or E09 | rejected | PASS |
| `negative.transientSchema` | E09_MANIFEST_CONTRACT_MISMATCH | E09_MANIFEST_CONTRACT_MISMATCH | PASS |
| `negative.unresolvedOptional` | E09_REFERENCE_UNRESOLVED | E09_REFERENCE_UNRESOLVED;E09_DOMAIN_SIGNATURE_MISMATCH | PASS |
| `package.expectedFiles` | 20/20 | 20/20 | PASS |
| `package.globalDiffCheck` | PASS or PREEXISTING_UNRELATED_DIFF | PREEXISTING_UNRELATED_DIFF | PASS |
| `package.guardFixNewFiles` | 0 | 0 | PASS |
| `package.guidConflicts` | 0 | 0 | PASS |
| `package.runtimeWhitelist` | EnemySystemDataValidation.cs;EnemySystemDataValidation.cs.meta;EnemySystemPlayerSafeSnapshot.cs;EnemySystemPlayerSafeSnapshot.cs.meta;EnemySystemSnapshot.cs;EnemySystemSnapshot.cs.meta;EnemySystemSnapshotInput.cs;EnemySystemSnapshotInput.cs.meta;EnemySystemSnapshotPrimitives.cs;EnemySystemSnapshotPrimitives.cs.meta | EnemySystemDataValidation.cs;EnemySystemDataValidation.cs.meta;EnemySystemPlayerSafeSnapshot.cs;EnemySystemPlayerSafeSnapshot.cs.meta;EnemySystemSnapshot.cs;EnemySystemSnapshot.cs.meta;EnemySystemSnapshotInput.cs;EnemySystemSnapshotInput.cs.meta;EnemySystemSnapshotPrimitives.cs;EnemySystemSnapshotPrimitives.cs.meta | PASS |
| `package.trailingWhitespace` | 0 | 0 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_BuildCapabilityReadContractVerifier_cs` | F1AB9863BC777D221E073294BD2D3478DBF416617BA7F9E3534A7882440A5D34 | F1AB9863BC777D221E073294BD2D3478DBF416617BA7F9E3534A7882440A5D34 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_CounterWindowAndPressureSchemaVerifier_cs` | 2FA48E6D1A17AE7B6204655FF5F58AE1D304C2315F1CBA9AA0A7F91D9CF99DD4 | 2FA48E6D1A17AE7B6204655FF5F58AE1D304C2315F1CBA9AA0A7F91D9CF99DD4 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EncounterCompositionSchemaVerifier_cs` | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyDomainDataContractVerifier_cs` | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyMechanicVocabularyVerifier_cs` | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyOfflineReadinessEvaluatorVerifier_cs` | 13D1A5D9C81FD590978F190ACAE260C26D348B24930FC4A82FBA192F584ECABA | 13D1A5D9C81FD590978F190ACAE260C26D348B24930FC4A82FBA192F584ECABA | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemySkillBossPhaseSchemaVerifier_cs` | 3C7892A7A73A1464EE2D797B8F617EBA1639B1C7C6D4330FC166F64B32DAB267 | 3C7892A7A73A1464EE2D797B8F617EBA1639B1C7C6D4330FC166F64B32DAB267 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyValidationContentNormalizeVerifier_cs` | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_Editor_EnemySystem_EnemyValidationContentNormalizer_cs` | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_BuildCapabilityReadPrimitives_cs` | 7A97F62EC9539A4F8D3C43FCF893854FB408A4A0950B40635884BE06681B62C4 | 7A97F62EC9539A4F8D3C43FCF893854FB408A4A0950B40635884BE06681B62C4 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_BuildCapabilityReadValidation_cs` | B21881430918157951A17BE4C76AC4A88D04626C0A4A549959F2A633DA61D5B6 | B21881430918157951A17BE4C76AC4A88D04626C0A4A549959F2A633DA61D5B6 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_BuildCapabilitySnapshots_cs` | EA2C7DF08CB2798C2D0DE401A564D3EC80B0C3F7DFF67334AE4856FB021A28BC | EA2C7DF08CB2798C2D0DE401A564D3EC80B0C3F7DFF67334AE4856FB021A28BC | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_CapabilityRead_ReadinessDiagnosticSnapshots_cs` | F689F732BFABA19A7EDE16EF694857AAC86205195D84BE828983381D882C5D51 | F689F732BFABA19A7EDE16EF694857AAC86205195D84BE828983381D882C5D51 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Composition_EncounterCompositionPrimitives_cs` | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Composition_EncounterCompositionSnapshots_cs` | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Composition_EncounterCompositionValidation_cs` | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Contracts_EnemyDomainReferences_cs` | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Contracts_EnemyDomainSnapshot_cs` | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Contracts_EnemyDomainValidation_cs` | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Domain_EnemyArchetypeSnapshots_cs` | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Domain_EnemyDomainPrimitives_cs` | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Normalization_EnemyValidationContentNormalizerCore_cs` | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Normalization_EnemyValidationContentSnapshot_cs` | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_BuildPressureSnapshots_cs` | 56CC65BADAF89BE6882629AE847FCF03C2D0BAC3D800BDB562DE66E275E4D1C1 | 56CC65BADAF89BE6882629AE847FCF03C2D0BAC3D800BDB562DE66E275E4D1C1 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_CounterWindowAndPressurePrimitives_cs` | E0A5C694D1039552F5C7E24CE599B71B7CB08B29AA39F11F6B134552B4505687 | E0A5C694D1039552F5C7E24CE599B71B7CB08B29AA39F11F6B134552B4505687 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_CounterWindowAndPressureValidation_cs` | 1526BAF50EC3A59914949A152FA48B659FE898A836EAD13D947CA905D17485AE | 1526BAF50EC3A59914949A152FA48B659FE898A836EAD13D947CA905D17485AE | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_PressureWindow_CounterWindowSnapshots_cs` | 5DD48CA0F0B84A44B7356EFFAFCC03C879200DB5C5030D27E00BEC15D24921C6 | 5DD48CA0F0B84A44B7356EFFAFCC03C879200DB5C5030D27E00BEC15D24921C6 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_ReadinessEvaluation_EnemyOfflineReadinessEvaluator_cs` | BDE586DCBA0BEC0B3926AE1584586B8C1DC5799FB6619B2171EF64E8E6C484BA | BDE586DCBA0BEC0B3926AE1584586B8C1DC5799FB6619B2171EF64E8E6C484BA | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_ReadinessEvaluation_EnemyReadinessInputSnapshots_cs` | 57B81F6F619FE64467E1D7EC4CBAFDFD6BEAB60028C47F2501DED966BE650BF8 | 57B81F6F619FE64467E1D7EC4CBAFDFD6BEAB60028C47F2501DED966BE650BF8 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_ReadinessEvaluation_EnemyReadinessPrimitives_cs` | 449F856169ADACDDE235D4C883C0D347057E48B54C70709F5FF8429D02055A5D | 449F856169ADACDDE235D4C883C0D347057E48B54C70709F5FF8429D02055A5D | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_ReadinessEvaluation_EnemyReadinessResultSnapshots_cs` | 3522F34A755E53B88F117CAB43C643BD0AE96BFBEC3E348BA0E2845016D0E925 | 3522F34A755E53B88F117CAB43C643BD0AE96BFBEC3E348BA0E2845016D0E925 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_BossPhaseSnapshots_cs` | B2FD5AA3367A8950D96F57AFC89C134B89A13A0557B55FDA4593DC78452F70F2 | B2FD5AA3367A8950D96F57AFC89C134B89A13A0557B55FDA4593DC78452F70F2 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_EnemySkillBossPhasePrimitives_cs` | 4F08886ABE8E6E7C9988E515A4CDDDD09D413001EF0B07E0FAFF3DADFE6FB6B6 | 4F08886ABE8E6E7C9988E515A4CDDDD09D413001EF0B07E0FAFF3DADFE6FB6B6 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_EnemySkillBossPhaseValidation_cs` | A175DF86360A993E9EE334D0EEE19C47725EB4CAED6BE613AA2788CD67CFB959 | A175DF86360A993E9EE334D0EEE19C47725EB4CAED6BE613AA2788CD67CFB959 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_SkillPhase_EnemySkillPatternSnapshots_cs` | 22A2FD124F32908141B93EB964D5DD1B3F099EF44BE72C9F367EF86557A2F499 | 22A2FD124F32908141B93EB964D5DD1B3F099EF44BE72C9F367EF86557A2F499 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Vocabulary_DefaultEnemyMechanicVocabularyCatalog_cs` | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Vocabulary_EnemyVocabularyModel_cs` | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | PASS |
| `protected.Assets__Game_Scripts_TalismanBag_EnemySystem_Vocabulary_EnemyVocabularyValidation_cs` | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | PASS |
| `protected.total` | 38/38 | 38/38 | PASS |
| `relation.duplicates` | 0 | 0 | PASS |
| `relation.isolationMismatch` | 0 | 0 | PASS |
| `relation.unresolved` | 0 | 0 | PASS |
| `reports.deterministic` | 7/7 hash identical | 7/7 hash identical | PASS |
| `root.isolation` | true/false/false | True/False/False | PASS |
| `root.schema` | EnemySystemSnapshot.v1/1 | EnemySystemSnapshot.v1/1 | PASS |
| `root.signature.full` | sha256 lowercase | sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af | PASS |
| `root.signature.player` | sha256 lowercase | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `signature.e03.developer.full` | different | sha256:bdf5792829cc67453e3dc7aeddc126ac62af30f909c08b653f5ab8cdeaa1ae17 | PASS |
| `signature.e03.developer.player` | stable | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `signature.e03.player.full` | different | sha256:b850aba18b3dcfca6979ce0c215a9bc99bfe89e6fc34f0b97136d6c8ed51f326 | PASS |
| `signature.e03.player.player` | different | sha256:b74f839ae5109310378fff140d37694dd841d8850a81fa995e16ca0dc813f9c5 | PASS |
| `signature.e04.composition.catalog` | different | sha256:763d1b3ad022b449da75e86f949bed7401751644d9557993ef928c23e699eef4 | PASS |
| `signature.e04.composition.full` | different | sha256:698a11fb812d7bf9a90b0559c756e44df54b6cd3850f80aa2fae8eed8a2b4557 | PASS |
| `signature.e04.composition.identities` | stable | sha256:5885334204edb4debfbcd35945ec1c4592bca01422ffac85a83efee6b534b857 | PASS |
| `signature.e04.composition.player` | stable | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `signature.e04.composition.relations` | stable | sha256:3d60ad86608df48c6374bbf5923793213641d83629d2483b99af4f26cef0078f | PASS |
| `signature.e04.relationTarget.full` | different | sha256:36a0e41c929c3965cb49dbc5741e32c58039006a8d8fd9bcaae852f963fdbe5a | PASS |
| `signature.e04.relationTarget.player` | stable | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `signature.e05.developer.full` | different | sha256:5c302ec09c01e824372d4cec9e8ccba8eb65cb2ff19b6b8e254315caa127e1e6 | PASS |
| `signature.e05.developer.player` | stable | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `signature.e05.player.full` | different | sha256:5647619921dfc5f9169fb2a1345253332cb6f36141ca00ceaddf94827e692b47 | PASS |
| `signature.e05.player.player` | different | sha256:a524811cb8ea8cf0577edc7eab3460f30f54bcf9bd8002142de07d06c9f5dfd8 | PASS |
| `signature.e06.developer.full` | different | sha256:ddf071bab72ca8cc5748cbe4e6c59d833a4c37321dbdc22539954b18bdb07fc5 | PASS |
| `signature.e06.developer.player` | stable | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `signature.e06.player.full` | different | sha256:d56b1ebc5f3e81141edfa150cf80cbc5d83ed30f4b841a5bc508f686ce17387c | PASS |
| `signature.e06.player.player` | different | sha256:79a254c80c9b94be3aab53b19cc6ca98767fa098634a98ef856a9fb3dca09444 | PASS |
| `signature.order.full` | sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af | sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af | PASS |
| `signature.order.player` | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `signature.transient.fullStable` | sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af | sha256:c29ccfae71259c1aa50247a8845f269bb063bacc237755b826a826215f2d30af | PASS |
| `signature.transient.playerStable` | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | sha256:5f87b16ccb09e1312410ed05eb9b6f18be30d1d04bfb579fee58e04e0d083758 | PASS |
| `transient.e07e08Created` | complete+sparse+2 readiness | Complete/Sparse/2 | PASS |
| `transient.fixtures.buildDistinct` | different | sha256:809a135e62ae1e2da126d63e79b33a861c8e43f4edff3f58ec475a54adf79b0f / sha256:e66f6d9ff1e7ec3d60e9ac721f3de83c8d8db26d35b615c941deb9c1849855fc | PASS |
| `transient.fixtures.readinessDeveloperDistinct` | different | sha256:f51b669238f34113dbd6a0f36b5da02f08cfdb5aff7941525f9881378f5e83c5 / sha256:e21994716736584594915fdb242d982496fafa96f90c2f6166087abfafac6464 | PASS |
| `transient.fixtures.readinessPlayerDistinct` | different | sha256:3ca15a5ae8edbdb99da8c46fdf0e5f78d245d59711410db2df66d9dea8aa359a / sha256:bbf448c542a57395dcf0bad19894568629e2ee97b1ddbc9ee23a43c68dc00e80 | PASS |
| `transient.fixtures.severityDistinct` | developer and player severity differ | Strained/Unknown/Warning/Notice | PASS |
| `transient.identityRelationAbsence` | 0/0 | 0/0 | PASS |
| `transient.staticComponents` | E01-E06 identical | E01=sha256:8afe8b2c98a89dcef825749909c6ef8d8b3c9a499f65babfed920f3cfb5c8146;E02=sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833;E03=sha256:bbc3c329874019d01d05912ee3b727025432ffc57678c2e9a3ad0f3e9eaf3d46;E04=sha256:4114368013d7af0c0e51378a8cc06ac13561d0537768b5b80db5c0ae75e18618;E05=sha256:130e18c8cea8600395883f7be2477912871445cb3c15b45910440537bf9576c2;E06=sha256:5613c599bbe481d13170b93e241aed79205a5f54568a2b5d940ec4ff91618bb4 | PASS |
| `transient.staticPayloadAbsence` | 2 contracts; 0 embedded; empty static signatures | 2/0/2 | PASS |
