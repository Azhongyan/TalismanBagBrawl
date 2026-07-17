# Enemy Domain Data Contract 01 Report

- Package: `V0.4-EnemyDomainDataContract01`
- Result: `DEV_COMPLETE / OFFLINE_VERIFIER_PASS / UNITY_BATCH_VERIFIER_BLOCKED`
- Execution: `Pure C# offline fallback`
- Unity batch compile: `PASS` — `Logs/codex_enemy_domain_contract_compile.log`, return code 0.
- Unity batch Verifier: `BLOCKED` — the same project was already open in a user Unity Editor instance; no Unity Verifier PASS is claimed.
- Current-source fallback compile + Verifier: `PASS` — exact sources compiled under .NET 9 and the shared Verifier core passed `34/34`.
- Schema: `EnemyDomainContract.v1`
- Schema version: `1`
- Verifier: `34/34`
- Canonical Signature: `sha256:63d670ad464c12b50c82562034d31460f2e4e3c91a7910c8fb3b444e83c43a06`

## Contract delivered

- Immutable `EnemyArchetypeSnapshot` and `BossArchetypeSnapshot` identity models.
- Typed Mechanic, SkillPattern, BossPhase, MapRule, Encounter, and CounterWindow references.
- Immutable `EnemyDomainSnapshot`, `IEnemyDomainSnapshotProvider`, and ordinal Enemy/Boss lookups.
- Defensive copies at input, snapshot, and nested reference-list boundaries.
- Order-independent SHA-256 canonical signature.
- Offline validation for schema, stable IDs, duplicate IDs, unresolved references, categories, and dev isolation.

## Isolation defaults

```text
devOnly = true
isEnabled = false
entersFormalFlow = false
```

## Verification summary

| Area | Passed | Total |
|---|---:|---:|
| canonical | 4 | 4 |
| contract | 3 | 3 |
| immutability | 3 | 3 |
| isolation | 1 | 1 |
| leak | 5 | 5 |
| lookup | 4 | 4 |
| schema | 2 | 2 |
| scope | 3 | 3 |
| validation | 9 | 9 |

## Scope

- New Enemy Domain/Contract/Editor files and the three named reports only.
- Scene, Prefab, Battle, Board, Item, RunFlow, Reward, and SaveData package entries: `0`.
- No user scene hand-test is required for this data-only package.
