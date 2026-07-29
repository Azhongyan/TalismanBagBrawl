# Enemy Requirement Channel Applicability Schema Contract Report

- Package: `V0.4-EnemyRequirementChannelApplicabilitySchemaContract01`
- Guard receipt: `GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01`
- Enemy Guard confirmation: `ENEMY_GUARD_CONFIRM_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01`
- Capability Algorithm Guard receipt: `CAPABILITY_ALGORITHM_GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01`
- Result: `PASS / ISOLATED READ-ONLY SCHEMA CONTRACT`
- Added files: `14`
- Modified existing files: `0`
- Schema: `EnemyRequirementChannelApplicability.v1`
- Schema version: `1`

## Contract result

- Channel enum values: `3/3` — `ContinuousBP=1`, `StructuralPredicate=2`, `RuntimeEventSignal=3`.
- Applicability enum values: `4/4` — `Applicable=1`, `Unknown=2`, `NotApplicable=3`, `NotInChannel=4`.
- Valid channel/state combinations: `15`.
- Invalid channel/state combinations: `21`.
- Undefined enum value `0`: rejected for both enums.
- Identity comparison: `StringComparison.Ordinal`; no trim or case normalization.
- Snapshot rows: defensive copy, read-only collection, stable `RequirementId` ordinal sort.
- Canonical formatting: `InvariantCulture`, length-prefixed fields, SHA-256 lowercase hex.
- Canonical Signature: `sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326`.

`Applicable` only means that a same-channel row participates in the current evaluation view. `Unknown` preserves insufficient applicability evidence. `NotApplicable` requires an explicit stable irrelevance rule. `NotInChannel` is exclusively a cross-channel view state and carries no own-channel or readiness outcome.

## In-memory fixture result

- Fixture snapshots: `3` (one per evaluation channel).
- Distinct generic requirement IDs: `9`.
- Fixture rows: `27`.
- State counts: `Applicable=3`, `Unknown=3`, `NotApplicable=3`, `NotInChannel=18`.
- Real encounter IDs: `0`.
- Real requirement IDs: `0`.
- Real requirement migration rows: `0`.

## Isolation result

- E06/E07/E08/E10 behavior changes: `0`.
- Readiness evaluations invoked: `0`.
- Requirement catalog reads: `0`.
- BuildCapabilityRead calls: `0`.
- Runtime Producer connections: `0`.
- Readiness outcomes generated: `0`.
- Predicate outcomes generated: `0`.
- Capability values or thresholds generated: `0`.
- D1-D6 implementation rows: `0`.
- Item runtime dependencies: `0`.
- Runtime dependencies outside `System.*`: `0`.
- C02 rows: `32/32` still `BLOCKED_BY_UNKNOWN`.
- N01A actual Unknown-reference reduction: `0`; remaining decisive Unknown references: `64`.
- N01A actual blocked-row reduction: `0`; remaining blocked rows: `32/32`.

## Protected baselines

| Scope | Files | Accepted SHA-256 | Result |
|---|---:|---|---|
| Item105 existing scope | 105 | `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` | PASS |
| Enemy existing scope excluding this package | 89 | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` | PASS |
| C01 | 9 | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` | PASS |
| C02 | 9 | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` | PASS |
| C02A | 7 | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` | PASS |
| N01 | 8 | `03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52` | PASS |
| C02A-D1 | 7 | `c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0` | PASS |
| N01A | 8 | `705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de` | PASS |
| E06 | 16 | `c91d77499c07e6a6b828d8fb2e21c08096d656c6395e94159a0a547fdf3e5569` | PASS |
| E07 | 16 | `b4be0330a411537e3e8e16968c2d45a5cdbbc688d9b588044ffe7e15daa505df` | PASS |
| E08 | 16 | `45a40d4993e1bf6bb78f1ad14e6ac9dfe84c0c5a69ebffc5502fabb3815821d4` | PASS |
| E10 | 21 | `3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be` | PASS |
| Scenes | 14 | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | PASS |
| Prefabs | 16 | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | PASS |
| BuildSettings | 1 | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | PASS |

- IF01 Signature: `sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428`.
- IF02 Signature: `sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673`.
- IF03 Signature: `sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a`.
- Affix Signature: `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f`.
- Roll150 Signature: `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8`.
- Projection150 Signature: `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2`.
- N01A Canonical Signature: `sha256:de8f5d758bd8837d88621c33f80dc952c0b7c2dccd7b18f96e71c54649240e79`.
- Protected hashes before/after: `identical`.
- Pre-existing Item dirty files: `preserved byte-for-byte`.

## Verification

- Offline verifier: `PASS`.
- Unity batch compile/verifier: `PASS`.
- Leak Count: `0`.
- GUID collision count: `0`.
- Trailing whitespace count: `0`.
- `git diff --check`: `PASS`.
- Forbidden scope changes: `0`.
- HEAD: `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba`.
- Git operations: no commit, tag, push, reset, or rollback.
- Next packages: not started.
