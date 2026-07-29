# Authored Layout Pressure Source Adapter Report

- Package: `V0.4-AuthoredLayoutPressureSourceAdapter01`
- Guard: `GUARD_PASS_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01`
- Enemy Guard: `ENEMY_GUARD_CONFIRM_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01`
- Capability Algorithm Guard: `CAPABILITY_ALGORITHM_GUARD_PASS_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01`
- Schema: `AuthoredLayoutPressureSourceAdapter.v1`
- Schema version: `1`
- Result: `PASS / ISOLATED READ-ONLY AUTHORED PRESSURE ADAPTER`
- Added files: `14`
- Modified existing files: `0`

## Contract result

The adapter accepts only explicit authored neutral layout-pressure facts. It preserves null as missing and explicit empty usable/connection collections as complete authored facts. It never derives domain cells, usable cells, connections, kinds, or predicate clauses.

- Complete: `8`
- Unknown: `5`
- Invalid: `3`
- Synthetic fixtures: `16/16`
- Missing fact disposition: `Unknown + Incomplete + null`
- Malformed supplied fact disposition: `Invalid + Incomplete + null`
- Invalid priority over Unknown: `PASS`
- Canonical Signature: `sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc`
- Canonical order: Ordinal text; enum numeric; cells Y/X; normalized edges A.Y/A.X/B.Y/B.X; invariant integers.
- Null and explicit-empty presence markers: distinct.
- Defensive copy and read-only output: `PASS`

## N01C boundary

Only a complete candidate is placed in a temporary validation-only envelope with fixed `Applicability=Unknown`, `BuildFactsCompleteness=Incomplete`, null BuildFacts, and `PressureFactsCompleteness=Complete`. The envelope is not returned or cached.

- N01C pressure-only Validator: `PASS`
- N01C Evaluator calls: `0`
- Applicability input rows: `0`
- Applicability output rows: `0`
- P1 Item/Build reads: `0`
- Real MapRule/Encounter/requirement/readiness rows: `0`
- Scene/Prefab/Board/Map/Battle/UI/BuildSettings connections: `0`
- BP/score/count conversions: `0`
- C02 blocked rows: `32/32`
- Actual Unknown reduction: `0`

## Protected baselines

| Scope | Files | Expected | Result |
|---|---:|---|---|
| P1 exact | 14 | `7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48` | PASS |
| N01C exact | 16 | `691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b` | PASS |
| N01B exact | 14 | `acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316` | PASS |
| Item105 | 105 | `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` | PASS |
| Enemy89 | 89 | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` | PASS |
| C01 | 9 | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` | PASS |
| C02 | 9 | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` | PASS |
| C02A | 7 | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` | PASS |
| N01 | 8 | `03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52` | PASS |
| C02A-D1 | 7 | `c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0` | PASS |
| N01A | 8 | `705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de` | PASS |
| Scenes | 14 | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | PASS |
| Prefabs | 16 | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | PASS |
| BuildSettings | 1 | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | PASS |

- P1 Canonical Signature: `sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd`.
- N01C Canonical Signature: `sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335`.
- N01B Canonical Signature: `sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326`.

## Verification

- Offline verifier: `PASS (110/110)`.
- Unity compile/verifier: `PASS (110/110)`.
- Protected hashes before/after: `identical / PASS`.
- Leak Count: `0`.
- GUID conflicts: `0`.
- Trailing whitespace: `0`.
- git diff --check: `PASS`.
- Forbidden scope touched: `0`.
- HEAD: `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba`.
- Commit/tag/push: `none`.
- Next package: `NOT_STARTED`.
