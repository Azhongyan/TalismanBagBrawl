# Layout Resilience Item Fact Projection Adapter Report

- Package: `V0.4-LayoutResilienceItemFactProjectionAdapter01`
- Guard: `GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01`
- Item Guard: `ITEM_GUARD_CONFIRM_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01`
- Capability Algorithm Guard: `CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEITEMFACTPROJECTIONADAPTER01`
- Schema: `LayoutResilienceItemFactProjectionAdapter.v1`
- Schema version: `1`
- Contract nature: read-only immutable Item layout fact projection
- Existing file modifications: `0`

## Source eligibility and mapping

The adapter consumes only the public `ItemSystemSnapshot.v1` layout fields and the public `ItemInstancePlacementBindingContractSnapshot.v1` result. It copies board size, baseline eye, placement identity, base item identity, anchor, rotation, catalog shape cells, occupied cells, core cell, `isLit`, and `isCountedInBuild` into N01C `LayoutResilienceBuildFactSnapshot` DTOs.

Ordinary placements require exactly one ordinal IF01 join by `placementId`, with explicit non-empty `itemInstanceId` and exact `baseItemId == Placement.itemId`. Missing and orphan identities remain Unknown; schema faults, duplicate identities, base mismatch, and I031 forgery are Invalid.

I031 is projected only as its existing placement fact row. No ordinary `itemInstanceId` is created, inferred, or exposed. Its `isLit` and `isCountedInBuild` values are copied unchanged. Valid empty layouts, I031-only layouts, and fully uncounted layouts remain Complete.

## Result states

- Complete: `7`
- Unknown: `4`
- Invalid: `4`
- Fixture result: `15/15`
- Projected fact rows across fixtures: `16`
- Complete means N01C completeness `Complete` with a non-null payload, including zero rows.
- Unknown means N01C completeness `Incomplete`; safe full or partial build facts may remain present.
- Invalid means N01C completeness `Incomplete` with a null payload.
- Invalid has priority over Unknown when both diagnostics exist.

## N01C boundary

The adapter constructs only an internal validation-only envelope with applicability Unknown, pressure completeness Incomplete, and null pressure. It calls the N01C validator only.

- N01C validation-only checks: `PASS`
- N01C Evaluator calls: `0`
- Pressure rows: `0`
- Authored pressure: `0`
- Real requirement/readiness mappings: `0`
- BP/score/threshold algorithms: `0`
- C02 blocked rows unchanged: `32/32`
- Actual Unknown reduction: `0`

## Determinism and immutability

Catalog, placement, binding, cell, and issue rows use stable Ordinal ordering. Integers use InvariantCulture and booleans use lowercase `true` / `false`. Reversed input order and culture changes preserve the canonical signature; changes to any approved source fact or binding identity change it. Result, issue, fact-row, and cell collections are defensive read-only copies.

- Canonical Signature: `sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd`
- IF01 Canonical Signature unchanged: `sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428`
- N01C Canonical Signature unchanged: `sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335`

## Verification

- Offline verifier: `PASS (271/271)`
- Unity compile / verifier: `PASS (271/271)`
- Protected baseline before/after: `PASS`
- GUID collisions: `0`
- Trailing whitespace: `0`
- git diff --check: `PASS`
- Forbidden scope: `0`
- Leak Count: `0`
- HEAD: `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba`

## Protected hashes

| Scope | Files | Expected |
|---|---:|---|
| Item existing | 105 | b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4 |
| ItemSystemSnapshot.cs | 1 | 821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df |
| IF01 exact | 11 | 2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8 |
| N01C exact | 16 | 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b |
| Enemy existing | 89 | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae |
| C01 | 9 | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a |
| C02 | 9 | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 |
| C02A | 7 | a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb |
| N01 | 8 | 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52 |
| C02A-D1 | 7 | c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0 |
| N01A | 8 | 705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de |
| N01B | 14 | acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316 |
| Scenes | 14 | da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b |
| Prefabs | 16 | 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087 |
| BuildSettings | 1 | 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59 |

No Item provider, Item validator, IF01 validator, N01C evaluator, real map, battle producer, requirement, readiness, persistence, reward, UI, scene, or prefab is connected.
