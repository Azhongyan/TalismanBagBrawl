# Enemy Skill / Boss Phase Schema Report

- Package: `V0.4-EnemySkillBossPhaseSchema01`
- Mode: `Unity batch`
- Schema: `EnemySkillBossPhase.v1` / `1`
- Checks: `134/134` passed

## Fixture inventory

| Type | Count |
|---|---:|
| SkillPattern | 4 |
| SkillSequence | 3 |
| CarrierSkillBinding | 3 |
| BossPhaseProfile | 3 |
| BossPhasePlan | 2 |

## Canonical signatures

- Full: `sha256:130e18c8cea8600395883f7be2477912871445cb3c15b45910440537bf9576c2`
- Player-safe: `sha256:7839dedc9f0f82d14f9e050a4945d599cd18dd8831cc31650d0dc928f6dc466a`

## Reuse and isolation

- Pattern reuse: `PASS`
- Sequence reuse: `PASS`
- Phase reuse: `PASS`
- Internal-only isolation: `PASS`
- Developer-only isolation: `PASS`
- Player-safe payload isolation: `PASS`

## Verification checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| `baseline.e03.bossProfiles` | 6 | 6 | PASS |
| `baseline.e03.bosses` | 7 | 7 | PASS |
| `baseline.e03.carrierBindings` | 17 | 17 | PASS |
| `baseline.e03.carriers` | 18 | 18 | PASS |
| `baseline.e03.enemies` | 11 | 11 | PASS |
| `baseline.e03.enemyProfiles` | 10 | 10 | PASS |
| `baseline.e03.exceptions` | 2 | 2 | PASS |
| `baseline.e03.mapBindings` | 30 | 30 | PASS |
| `baseline.e03.mapRules` | 10 | 10 | PASS |
| `baseline.e03.profiles` | 16 | 16 | PASS |
| `baseline.e04.encounterWaves` | 4 | 4 | PASS |
| `baseline.e04.encounters` | 2 | 2 | PASS |
| `baseline.e04.fixtureRows` | 6 | 6 | PASS |
| `fixture.bossPlans` | >=2 | 2 | PASS |
| `fixture.carrierBindings` | >=2 with Enemy and Boss | 3 bindings | PASS |
| `fixture.channeled` | >=1 | 2 | PASS |
| `fixture.conditions` | EncounterStart;HealthRatioAtOrBelow;MechanicSignal | EncounterStart;HealthRatioAtOrBelow;MechanicSignal | PASS |
| `fixture.instant` | >=1 | 2 | PASS |
| `fixture.patterns` | >=4 | 4 | PASS |
| `fixture.phaseProfiles` | >=3 | 3 | PASS |
| `fixture.sequences` | >=3 | 3 | PASS |
| `immutable.defensive.copy` | source mutation isolated |  | PASS |
| `immutable.nested.collections` | read-only |  | PASS |
| `immutable.top.collections` | read-only |  | PASS |
| `isolation.developer` | full changed; player-safe unchanged | full=True;player=True | PASS |
| `isolation.internal` | full changed; player-safe unchanged | full=True;player=True | PASS |
| `isolation.playerSafe.payload` | 0 forbidden tokens | 0 | PASS |
| `isolation.playerSafe.schema` | schemaId+schemaVersion present |  | PASS |
| `leak.answerFields` | 0 | 0 | PASS |
| `leak.chapterIds` | 0 | 0 | PASS |
| `leak.legacy.soleReader` | 1 | 1 | PASS |
| `leak.prohibitedAreas` | 0 | 0 | PASS |
| `leak.runtime.dependencies` | 0 | 0 | PASS |
| `leak.runtime.execution` | 0 | 0 | PASS |
| `leak.runtimeFiles` | 4 | 4 | PASS |
| `leak.verifier.directLegacyUsing` | 0 | 0 | PASS |
| `lookup.binding.ordinal` | exact=true;case=false |  | PASS |
| `lookup.pattern.ordinal` | exact=true;case=false |  | PASS |
| `lookup.phase.ordinal` | exact=true;case=false |  | PASS |
| `lookup.plan.ordinal` | exact=true;case=false |  | PASS |
| `lookup.sequence.ordinal` | exact=true;case=false |  | PASS |
| `package.fileCount` | 16 | 16 | PASS |
| `package.globalGuidConflicts` | 0 | 0 | PASS |
| `package.metaGuidCount` | 6 | 6 | PASS |
| `package.metaGuidUnique` | 6 | 6 | PASS |
| `package.prohibitedExtensions` | 0 | 0 | PASS |
| `package.reportArtifacts` | 5 | 5 | PASS |
| `package.runtimeDirectoryArtifacts` | 8 | 8 | PASS |
| `package.trailingWhitespace` | 0 | 0 | PASS |
| `protected.e01e02e03e04.DefaultEnemyMechanicVocabularyCatalog.cs` | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | PASS |
| `protected.e01e02e03e04.EncounterCompositionPrimitives.cs` | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | PASS |
| `protected.e01e02e03e04.EncounterCompositionSchemaVerifier.cs` | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | PASS |
| `protected.e01e02e03e04.EncounterCompositionSnapshots.cs` | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | PASS |
| `protected.e01e02e03e04.EncounterCompositionValidation.cs` | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | PASS |
| `protected.e01e02e03e04.EnemyArchetypeSnapshots.cs` | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | PASS |
| `protected.e01e02e03e04.EnemyDomainDataContractVerifier.cs` | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | PASS |
| `protected.e01e02e03e04.EnemyDomainPrimitives.cs` | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | PASS |
| `protected.e01e02e03e04.EnemyDomainReferences.cs` | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | PASS |
| `protected.e01e02e03e04.EnemyDomainSnapshot.cs` | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | PASS |
| `protected.e01e02e03e04.EnemyDomainValidation.cs` | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | PASS |
| `protected.e01e02e03e04.EnemyMechanicVocabularyVerifier.cs` | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentNormalizeVerifier.cs` | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentNormalizer.cs` | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentNormalizerCore.cs` | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentSnapshot.cs` | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | PASS |
| `protected.e01e02e03e04.EnemyVocabularyModel.cs` | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | PASS |
| `protected.e01e02e03e04.EnemyVocabularyValidation.cs` | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | PASS |
| `protected.e01e02e03e04.summary` | 18/18 unchanged | 18/18 unchanged | PASS |
| `protected.legacy.BuildProblemRuleConfigs.cs` | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | PASS |
| `protected.legacy.BuildProblemSeedData.cs` | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | PASS |
| `protected.legacy.EnemyBossValidationPool.cs` | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | PASS |
| `protected.legacy.summary` | 3/3 unchanged | 3/3 unchanged | PASS |
| `reject.carrier.kindMismatch` | SKILL_MECHANIC_PROFILE_KIND_MISMATCH | SKILL_MECHANIC_PROFILE_KIND_MISMATCH;CARRIER_MECHANIC_BINDING_MISSING | PASS |
| `reject.carrier.unresolved` | BOSS_REFERENCE_UNRESOLVED | BOSS_REFERENCE_UNRESOLVED | PASS |
| `reject.cast.duration.negative` | CAST_DURATION_NEGATIVE | CAST_DURATION_NEGATIVE;INSTANT_CAST_DURATION_INVALID | PASS |
| `reject.channeled.duration` | CHANNELED_CAST_DURATION_INVALID | CHANNELED_CAST_DURATION_INVALID | PASS |
| `reject.condition.fields` | ENTRY_CONDITION_FIELDS_INVALID | ENTRY_CONDITION_FIELDS_INVALID | PASS |
| `reject.condition.healthThreshold` | ENTRY_CONDITION_FIELDS_INVALID | ENTRY_CONDITION_FIELDS_INVALID;FIRST_PHASE_CONDITION_INVALID | PASS |
| `reject.condition.signalEmpty` | ENTRY_CONDITION_FIELDS_INVALID | ENTRY_CONDITION_FIELDS_INVALID;ID_EMPTY;FIRST_PHASE_CONDITION_INVALID | PASS |
| `reject.firstPhase.condition` | FIRST_PHASE_CONDITION_INVALID | FIRST_PHASE_CONDITION_INVALID | PASS |
| `reject.id.caseReplacement` | SKILL_PATTERN_REFERENCE_UNRESOLVED | SKILL_PATTERN_REFERENCE_UNRESOLVED | PASS |
| `reject.id.empty` | ID_EMPTY | ID_EMPTY;SKILL_PATTERN_REFERENCE_UNRESOLVED | PASS |
| `reject.id.whitespace` | ID_OUTER_WHITESPACE | ID_OUTER_WHITESPACE;SKILL_PATTERN_REFERENCE_UNRESOLVED | PASS |
| `reject.input.null` | INPUT_NULL | INPUT_NULL | PASS |
| `reject.instant.duration` | INSTANT_CAST_DURATION_INVALID | INSTANT_CAST_DURATION_INVALID | PASS |
| `reject.isolation.pattern` | DEV_ISOLATION_INVALID | DEV_ISOLATION_INVALID | PASS |
| `reject.isolation.plan` | DEV_ISOLATION_INVALID | DEV_ISOLATION_INVALID | PASS |
| `reject.isolation.sequence` | DEV_ISOLATION_INVALID | DEV_ISOLATION_INVALID | PASS |
| `reject.laterPhase.encounterStart` | LATER_PHASE_ENCOUNTER_START_INVALID | LATER_PHASE_ENCOUNTER_START_INVALID | PASS |
| `reject.mechanic.unresolved` | MECHANIC_PROFILE_REFERENCE_UNRESOLVED | MECHANIC_PROFILE_REFERENCE_UNRESOLVED;CARRIER_MECHANIC_BINDING_MISSING | PASS |
| `reject.pattern.duplicate` | SKILL_PATTERN_ID_DUPLICATE | SKILL_PATTERN_ID_DUPLICATE | PASS |
| `reject.pattern.unresolved` | SKILL_PATTERN_REFERENCE_UNRESOLVED | SKILL_PATTERN_REFERENCE_UNRESOLVED | PASS |
| `reject.phase.duplicate` | BOSS_PHASE_ID_DUPLICATE | BOSS_PHASE_ID_DUPLICATE | PASS |
| `reject.phase.enemyMechanic` | MECHANIC_PROFILE_KIND_MISMATCH | MECHANIC_PROFILE_KIND_MISMATCH;BOSS_PHASE_CARRIER_MECHANIC_BINDING_MISSING | PASS |
| `reject.phase.order.duplicate` | PHASE_ORDER_DUPLICATE | PHASE_ORDER_DUPLICATE | PASS |
| `reject.phase.order.negative` | PHASE_ORDER_NEGATIVE | PHASE_ORDER_NEGATIVE;PHASE_ORDER_NON_CONTIGUOUS | PASS |
| `reject.phase.order.nonContiguous` | PHASE_ORDER_NON_CONTIGUOUS | LATER_PHASE_ENCOUNTER_START_INVALID;PHASE_ORDER_NON_CONTIGUOUS | PASS |
| `reject.plan.duplicateBoss` | BOSS_PHASE_PLAN_BOSS_ID_DUPLICATE | BOSS_PHASE_PLAN_BOSS_ID_DUPLICATE | PASS |
| `reject.plan.empty` | BOSS_PHASE_PLAN_EMPTY | BOSS_PHASE_PLAN_EMPTY | PASS |
| `reject.planBoss.unresolved` | BOSS_REFERENCE_UNRESOLVED | BOSS_REFERENCE_UNRESOLVED | PASS |
| `reject.recovery.duration.negative` | RECOVERY_DURATION_NEGATIVE | RECOVERY_DURATION_NEGATIVE | PASS |
| `reject.resolver.null` | REFERENCE_RESOLVER_NULL | REFERENCE_RESOLVER_NULL | PASS |
| `reject.schema.id` | SCHEMA_ID_MISMATCH | SCHEMA_ID_MISMATCH | PASS |
| `reject.schema.version` | SCHEMA_VERSION_MISMATCH | SCHEMA_VERSION_MISMATCH | PASS |
| `reject.sequence.duplicate` | SKILL_SEQUENCE_ID_DUPLICATE | SKILL_SEQUENCE_ID_DUPLICATE | PASS |
| `reject.sequence.empty` | SKILL_SEQUENCE_EMPTY | SKILL_SEQUENCE_EMPTY | PASS |
| `reject.sequence.unresolved` | SKILL_SEQUENCE_REFERENCE_UNRESOLVED | SKILL_SEQUENCE_REFERENCE_UNRESOLVED | PASS |
| `reject.source.path` | SOURCE_REFERENCE_PATH_FORBIDDEN | SOURCE_REFERENCE_PATH_FORBIDDEN | PASS |
| `reject.step.delay.negative` | STEP_DELAY_NEGATIVE | STEP_DELAY_NEGATIVE | PASS |
| `reject.step.order.duplicate` | STEP_ORDER_DUPLICATE | STEP_ORDER_DUPLICATE | PASS |
| `reject.step.order.negative` | STEP_ORDER_NEGATIVE | STEP_ORDER_NEGATIVE;STEP_ORDER_NON_CONTIGUOUS | PASS |
| `reject.step.order.nonContiguous` | STEP_ORDER_NON_CONTIGUOUS | STEP_ORDER_NON_CONTIGUOUS | PASS |
| `reject.step.repeat` | STEP_REPEAT_COUNT_INVALID | STEP_REPEAT_COUNT_INVALID | PASS |
| `reject.vocabulary.unknown` | DEVELOPER_DIAGNOSTIC_CATEGORY_UNRESOLVED | DEVELOPER_DIAGNOSTIC_CATEGORY_UNRESOLVED | PASS |
| `reject.vocabulary.wrongCategory` | PLAYER_HINT_CATEGORY_UNRESOLVED | PLAYER_HINT_CATEGORY_UNRESOLVED | PASS |
| `reuse.pattern` | >=1 | 1 | PASS |
| `reuse.phase` | >=1 | 3 | PASS |
| `reuse.sequence` | >=1 | 1 | PASS |
| `schema.id` | EnemySkillBossPhase.v1 | EnemySkillBossPhase.v1 | PASS |
| `schema.version` | 1 | 1 | PASS |
| `signature.castDuration` | full changed; player-safe unchanged | full=True;player=True | PASS |
| `signature.delay` | full changed; player-safe unchanged | full=True;player=True | PASS |
| `signature.deterministic.repeat` | sha256:130e18c8cea8600395883f7be2477912871445cb3c15b45910440537bf9576c2 | sha256:130e18c8cea8600395883f7be2477912871445cb3c15b45910440537bf9576c2 | PASS |
| `signature.entryCondition` | full changed; player-safe unchanged | full=True;player=True | PASS |
| `signature.full.format` | sha256:+64 lowercase hex | sha256:130e18c8cea8600395883f7be2477912871445cb3c15b45910440537bf9576c2 | PASS |
| `signature.input.permutation` | sha256:130e18c8cea8600395883f7be2477912871445cb3c15b45910440537bf9576c2 | sha256:130e18c8cea8600395883f7be2477912871445cb3c15b45910440537bf9576c2 | PASS |
| `signature.phaseOrder` | full changed; player-safe unchanged | full=True;player=True | PASS |
| `signature.player.format` | sha256:+64 lowercase hex | sha256:7839dedc9f0f82d14f9e050a4945d599cd18dd8831cc31650d0dc928f6dc466a | PASS |
| `signature.public.intent` | full changed; player-safe changed | full=True;player=True | PASS |
| `signature.public.phaseLabel` | full changed; player-safe changed | full=True;player=True | PASS |
| `signature.public.skill` | full changed; player-safe changed | full=True;player=True | PASS |
| `signature.public.targetCue` | full changed; player-safe changed | full=True;player=True | PASS |
| `signature.repeat` | full changed; player-safe unchanged | full=True;player=True | PASS |
| `signature.stepOrder` | full changed; player-safe unchanged | full=True;player=True | PASS |
