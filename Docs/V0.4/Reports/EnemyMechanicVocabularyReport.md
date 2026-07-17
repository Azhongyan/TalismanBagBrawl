# Enemy Mechanic Vocabulary 01 Report

- Package: `V0.4-EnemyMechanicVocabulary01`
- Result: `PASS`
- Execution: `Unity batch`
- Schema: `EnemyMechanicVocabulary.v1`
- Schema version: `1`
- Verifier: `48/48`
- Canonical Signature: `sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833`

## Vocabulary counts

| Category | Count |
|---|---:|
| Mechanic | 12 |
| BuildCapability | 16 |
| PressureChannel | 9 |
| CounterWindowType | 6 |
| PlayerHintCategory | 10 |
| DeveloperDiagnosticCategory | 10 |

## Legacy mapping

- Total legacy keys: `124`
- Mapped: `102`
- OUT_OF_SCOPE: `22`
- Unresolved: `0`
- Current source-field aliases: `pressureType = mechanicType semantic`, `hardSolutionTags = requiredCapabilityTags semantic`, `softSolutionTags = optionalCapabilityTags semantic`.

## Boundary

- Pure C# vocabulary and immutable snapshots only.
- No Enemy/Boss content migration, runtime mechanic execution, player-answer panel, Scene, Prefab, Config, Battle, Board, or Item changes.
- E01 protected files are checked against pre-development SHA-256 baselines.
- Repository-wide pre-existing dirty files are excluded from this package manifest and must be reported separately.
