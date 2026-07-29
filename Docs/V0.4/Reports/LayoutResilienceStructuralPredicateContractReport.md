# Layout Resilience Structural Predicate Contract Report

- Package: `V0.4-LayoutResilienceStructuralPredicateContract01`
- Guard receipt: `GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01`
- Enemy Guard confirmation: `ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01`
- Item Guard confirmation: `ITEM_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01`
- Capability Algorithm Guard receipt: `CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01`
- Result: `PASS / ISOLATED STRUCTURAL CONTRACT / NO BEHAVIOR CHANGE`
- Added files: `16`
- Modified existing files: `0`
- Schema: `LayoutResilienceStructuralPredicate.v1`
- Schema version: `1`

## Contract semantics

This package evaluates an immutable and fully authored layout-pressure snapshot against neutral Item geometry and placement facts. It does not author pressure coordinates and does not connect to Item runtime, Board, Map, Battle, readiness, a requirement catalog, or any Runtime Producer.

- Predicate states: `KnownTrue=1`, `KnownFalse=2`, `Unknown=3`, `NotApplicable=4`.
- Completeness: `Complete=1`, `Incomplete=2`, `NotRequired=3`.
- Pressure kinds: `PollutedCellMask=1`, `EyeRelocationOrDisruption=2`, `StructuralConnectionCut=3`.
- Required clauses: `5/5` named values.
- Clause states: `Satisfied=1`, `Violated=2`.
- Invalid is validation failure and is never serialized as a predicate result.
- `Applicable` permits evaluation but does not mean true or readiness pass.
- `Unknown` never becomes false or zero.
- `NotApplicable` is produced only by explicit N01B applicability plus `NotRequired/NotRequired`.
- `NotInChannel` is rejected by this contract.

## Item source eligibility

Eligible future projection evidence is limited to ItemSystem board size and eye cell plus immutable shape, placement, occupied-cell, core-cell, lit, and counted-in-Build facts. This package does not read Item types directly and contains no Item Adapter. Unplaced Items create no row. A counted but unlit row is Invalid. Unlit and uncounted rows remain diagnostic facts but do not participate in clauses.

Shape and occupied-cell cardinality is checked only for structural integrity. BP/score/ratio conversion rows: `0`.

## Pressure independence

Pressure is supplied through `LayoutPressureSnapshot` with an explicit domain, usable-cell mask, effective eye anchor, preserved undirected connections, pressure-kind provenance, and required clause list. No coordinates are inferred from pressure names, tags, values, vocabulary, MapRule, encounters, or legacy facts. The eye is a neutral structural anchor and does not assert Powered or FormationEnergy.

## Pure evaluator

Only `Applicable + Complete + Complete` runs clauses. The evaluator considers only counted placements and only explicitly required clauses. Cell/core usability uses set inclusion. Eye-to-core connectivity uses the supplied usable undirected graph. Every required clause must be satisfied for KnownTrue; any violation yields KnownFalse. Empty counted layouts are interpreted solely through the authored clause list.

- Valid truth-table combinations: `9`.
- Invalid truth-table combinations: `27`.
- Valid outcome fixtures: `10/10`.
- Disposition: `KnownTrue=3`, `KnownFalse=3`, `Unknown=3`, `NotApplicable=1`.
- Invalid assertions: `66`.
- Canonical Signature: `sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335`.
- Ordering: PlacementId Ordinal; cells Y/X ascending; enums numeric; connections canonical; invariant decimal.
- Random/time/static mutable state reads: `0`.

## No behavior change

- Real requirement/Encounter mapping rows: `0`.
- Real pressure/config rows: `0`.
- Readiness evaluator calls: `0`.
- Runtime Producer connections: `0`.
- Board/Map/Scene/Prefab/RectTransform reads: `0`.
- Placement search, rotation, lighting recomputation, Battle ticks: `0`.
- E06/E07/E08/E10 behavior changes: `0`.
- C02 blocked rows: `32/32`.
- Actual Unknown-reference reduction: `0`.
- Projected blocked-row reduction: `0`.
- Remaining decisive Unknown references: `64`.
- placement_shape actual Unknown references: `32`.

## Protected baselines

| Scope | Files | Accepted value | Result |
|---|---:|---|---|
| Item105 | 105 | `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` | PASS |
| Enemy existing | 89 | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` | PASS |
| C01 | 9 | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` | PASS |
| C02 | 9 | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` | PASS |
| C02A | 7 | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` | PASS |
| N01 | 8 | `03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52` | PASS |
| C02A-D1 | 7 | `c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0` | PASS |
| N01A | 8 | `705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de` | PASS |
| N01B | 14 | `acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316` | PASS |
| E06 | 16 | `c91d77499c07e6a6b828d8fb2e21c08096d656c6395e94159a0a547fdf3e5569` | PASS |
| E07 | 16 | `b4be0330a411537e3e8e16968c2d45a5cdbbc688d9b588044ffe7e15daa505df` | PASS |
| E08 | 16 | `45a40d4993e1bf6bb78f1ad14e6ac9dfe84c0c5a69ebffc5502fabb3815821d4` | PASS |
| E10 | 21 | `3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be` | PASS |
| Scenes | 14 | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | PASS |
| Prefabs | 16 | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | PASS |
| BuildSettings | 1 | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | PASS |

- IF01/IF02/IF03, Affix, Roll150, Projection150 fixed signatures: `PASS`.
- N01A Canonical Signature: `sha256:de8f5d758bd8837d88621c33f80dc952c0b7c2dccd7b18f96e71c54649240e79`.
- N01B Canonical Signature: `sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326`.
- Protected hashes before/after: `identical`.
- Pre-existing Item dirty: `preserved byte-for-byte`.

## Verification

- Offline verifier: `348/348 PASS`.
- Unity batch compile/verifier: `348/348 PASS`.
- Leak Count: `0`.
- GUID conflicts: `0`.
- Trailing whitespace: `0`.
- git diff --check: `PASS`.
- Forbidden scope touched: `0`.
- HEAD: `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba`.
- Commit/tag/push: `none`.
- Next package: `NOT_STARTED`.
