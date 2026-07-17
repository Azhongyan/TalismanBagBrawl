# Enemy Skill / Boss Phase Schema Leak Check

Every forbidden dependency is reported separately. Counts are literal-source matches in the four E05 runtime files unless stated otherwise.

## Forbidden runtime dependencies

| Token | Matches | Result |
|---|---:|---|
| `TalismanBag.BuildSandbox` | 0 | PASS |
| `TalismanBag.Battle` | 0 | PASS |
| `TalismanBag.Item` | 0 | PASS |
| `ItemSystem` | 0 | PASS |
| `UnityEngine` | 0 | PASS |
| `MonoBehaviour` | 0 | PASS |
| `ScriptableObject` | 0 | PASS |
| `GameObject` | 0 | PASS |
| `Transform` | 0 | PASS |
| `Addressables` | 0 | PASS |
| `SceneManager` | 0 | PASS |
| `BattleContract` | 0 | PASS |
| `BattleBridge` | 0 | PASS |
| `UnifiedBattlePage` | 0 | PASS |
| `RunFlow` | 0 | PASS |
| `PageState` | 0 | PASS |
| `FormationState` | 0 | PASS |
| `Reward` | 0 | PASS |
| `SaveData` | 0 | PASS |
| `PlayerPrefs` | 0 | PASS |
| `V02FormationGridFrame` | 0 | PASS |
| `DamageText` | 0 | PASS |
| `BuildReadiness` | 0 | PASS |
| `CounterWindow` | 0 | PASS |
| `BuildPressure` | 0 | PASS |
| `requiredCapability` | 0 | PASS |
| `hardSolution` | 0 | PASS |
| `BossKey` | 0 | PASS |
| `weaknessThreshold` | 0 | PASS |
| `DropBias` | 0 | PASS |
| `void Update(` | 0 | PASS |
| `IEnumerator` | 0 | PASS |
| `Coroutine` | 0 | PASS |
| `StartCoroutine` | 0 | PASS |
| `async ` | 0 | PASS |
| `Task.Delay` | 0 | PASS |
| `Thread.Sleep` | 0 | PASS |
| `DateTime.Now` | 0 | PASS |
| `Stopwatch` | 0 | PASS |
| `currentHp` | 0 | PASS |
| `currentHealth` | 0 | PASS |
| `SelectTarget` | 0 | PASS |
| `ApplyDamage` | 0 | PASS |
| `DealDamage` | 0 | PASS |
| `ChangePhase` | 0 | PASS |
| `TransitionPhase` | 0 | PASS |

## Isolation payload

- Player-safe payload length: `2514`
- Forbidden player-safe fields: `PASS`
- Formal chapter IDs: `PASS`
- Build answer / key / drop-bias fields: `PASS`
- Direct legacy adapter dependency: `PASS`
- Sole E03 legacy reader invariant: `PASS`

## Protected sources and baselines

| Check | Expected | Actual | Result |
|---|---|---|---|
| `baseline.e03.enemies` | 11 | 11 | PASS |
| `baseline.e03.bosses` | 7 | 7 | PASS |
| `baseline.e03.carriers` | 18 | 18 | PASS |
| `baseline.e03.profiles` | 16 | 16 | PASS |
| `baseline.e03.enemyProfiles` | 10 | 10 | PASS |
| `baseline.e03.bossProfiles` | 6 | 6 | PASS |
| `baseline.e03.mapRules` | 10 | 10 | PASS |
| `baseline.e03.carrierBindings` | 17 | 17 | PASS |
| `baseline.e03.mapBindings` | 30 | 30 | PASS |
| `baseline.e03.exceptions` | 2 | 2 | PASS |
| `baseline.e04.fixtureRows` | 6 | 6 | PASS |
| `baseline.e04.encounters` | 2 | 2 | PASS |
| `baseline.e04.encounterWaves` | 4 | 4 | PASS |
| `protected.e01e02e03e04.EnemyDomainPrimitives.cs` | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | 83114BBA194226EC539A9BC401FF791AC8A76560B845E8D1B7F8E662C8122DC8 | PASS |
| `protected.e01e02e03e04.EnemyArchetypeSnapshots.cs` | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | 7D459972E78C5ACF768A296B679A00B4874DFC5497E47FC553DB82288F58C1FE | PASS |
| `protected.e01e02e03e04.EnemyDomainReferences.cs` | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | 777DA9A7370562961D34346D3604DE9C345F2B634435152B0BF0EB4CEA135E71 | PASS |
| `protected.e01e02e03e04.EnemyDomainSnapshot.cs` | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | AB1004B8ABB5C96794BB1B17B7314232E7FD439A08745B9BBDA17FEFBF9CCB1B | PASS |
| `protected.e01e02e03e04.EnemyDomainValidation.cs` | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | 44CAA85CE869E6EB6B6859328E2ED0BE496809184DACBD187435AD35BC462D80 | PASS |
| `protected.e01e02e03e04.EnemyDomainDataContractVerifier.cs` | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | ACF3AEC47318CC09511610430822663F70968EBDA8E26A573F3F9DE3B5A6600E | PASS |
| `protected.e01e02e03e04.EnemyVocabularyModel.cs` | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | 243CFBF2E781171AF84AC49BAB1DC733E02207D02FB1A5C52AA46205FEE7AD78 | PASS |
| `protected.e01e02e03e04.DefaultEnemyMechanicVocabularyCatalog.cs` | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | 6C20B76D1FF7493A3459A3D2C6FD09D7EB38ED8097A999845E92C9E93FBEBE5B | PASS |
| `protected.e01e02e03e04.EnemyVocabularyValidation.cs` | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | 49AE47A77CD5DC74CD3B7CBF1B86F1DB004FD0B4D40DB83AD5C7DBEB6FE8FA76 | PASS |
| `protected.e01e02e03e04.EnemyMechanicVocabularyVerifier.cs` | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | 4C3D56DAC6C4D610C4247B735097985E5C0CEAFC9FCC353FEAEC280E89F448F9 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentSnapshot.cs` | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | DABB846698AFFF031C242DEFBA1F3D92F7905A64DDA61D8FFF83DD30DA0C1628 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentNormalizerCore.cs` | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | 67A6D732BADA10EF77E38CF4AEC6A2519D6D8DFFF924C6D2409FA976BAD9D8F4 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentNormalizer.cs` | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | 452407A53E122E461AA133D78A3A3A445BDFF9B595E870E9E9FEDE7CE9283199 | PASS |
| `protected.e01e02e03e04.EnemyValidationContentNormalizeVerifier.cs` | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | D8C971D5F14F99514A7ACDA0287A4208C182DF8A21243D79B46C5EBCE1DD9449 | PASS |
| `protected.e01e02e03e04.EncounterCompositionPrimitives.cs` | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | 9AC645930ED3BB4ADF6B0DFDB544534C0F5FFEA19F82F734CDF79FCA0B6A9224 | PASS |
| `protected.e01e02e03e04.EncounterCompositionSnapshots.cs` | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | 33F0453F453AFA15198EADAC711E93C9F122FF080671BBD0D0B9D98A4DC19472 | PASS |
| `protected.e01e02e03e04.EncounterCompositionValidation.cs` | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | 72CA62B3CD893FC5C18BB11EFA99F2C466FB1867647DFBDB4231ECEC1BA8AFD7 | PASS |
| `protected.e01e02e03e04.EncounterCompositionSchemaVerifier.cs` | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | 894CB9B8C0E1DB591A501FD0DC54A220FEDFD6459A2C3FF2D2F3C8B31E4785A0 | PASS |
| `protected.e01e02e03e04.summary` | 18/18 unchanged | 18/18 unchanged | PASS |
| `protected.legacy.EnemyBossValidationPool.cs` | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | C2049E9FF6ACB3E92C2484B223D301DE14ED29BE3704825BD0C5262DFB8DDFF9 | PASS |
| `protected.legacy.BuildProblemRuleConfigs.cs` | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | 14BF83188C3A318182CAC179C9DA66E9C6E6FF70B33A52DD0B5734F3C9B86060 | PASS |
| `protected.legacy.BuildProblemSeedData.cs` | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | A9E50C94D79F53C7065697CF57977548AEDC383B97A5A888ADC6C9EE59B965F4 | PASS |
| `protected.legacy.summary` | 3/3 unchanged | 3/3 unchanged | PASS |
| `package.fileCount` | 16 | 16 | PASS |
| `package.runtimeDirectoryArtifacts` | 8 | 8 | PASS |
| `package.reportArtifacts` | 5 | 5 | PASS |
| `package.prohibitedExtensions` | 0 | 0 | PASS |
| `package.trailingWhitespace` | 0 | 0 | PASS |
| `package.metaGuidCount` | 6 | 6 | PASS |
| `package.metaGuidUnique` | 6 | 6 | PASS |
| `package.globalGuidConflicts` | 0 | 0 | PASS |
