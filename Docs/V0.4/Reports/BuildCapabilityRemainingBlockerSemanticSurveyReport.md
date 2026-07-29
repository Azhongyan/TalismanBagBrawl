# Build Capability Remaining Blocker Semantic Survey Report

- Package: `V0.4-BuildCapabilityRemainingBlockerSemanticSurvey01`
- Guard receipt: `GUARD_PASS_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01`
- Enemy Guard confirmation: `ENEMY_GUARD_CONFIRM_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01`
- Nature: `REPORT_ONLY / CROSS_SYSTEM_SEMANTIC_ANALYSIS`
- Result: `PASS / SURVEY_ONLY / NO_BEHAVIOR_CHANGE`
- Added files: `8`
- Existing-file modifications: `0`
- Survey completion state: `4/4 Unknown`
- Actual Unknown-reference reduction: `0`
- Actual blocked-row reduction: `0`
- Remaining decisive Unknown references: `64`
- Blocked rows: `32/32`
- Offline verifier: `PASS 788/788`
- Unity verifier: `COMPILE_PASS; VERIFY_PASS 788/788`
- Leak Count: `0`
- Canonical Signature: `sha256:de8f5d758bd8837d88621c33f80dc952c0b7c2dccd7b18f96e71c54649240e79`

This Survey classifies the four remaining decisive Unknowns after N01 and records possible requirement channels. It does not change Item or Enemy facts, E08/E10 requirements, schemas, contracts, C02 evaluation, readiness behavior, or runtime production. Every recommendation remains `USER_DECISION_REQUIRED`; the actual system state remains unchanged.

## Four unique classifications

| Capability | Unique primary classification | Enemy planning meaning | Recommended channel | Current Offline blocker | Recommended Offline blocker | Survey output |
|---|---|---|---|---:|---:|---|
| `capability.placement_shape` | `STRUCTURAL_PREDICATE` | Preserve usable layout, core access and energy paths under polluted tiles, formation-eye movement/disruption or disconnection pressure; not cell count, generic compactness or single-item size | `LayoutResilience / EncounterStructuralPredicate` | true | false | `Unknown / no behavior change` |
| `capability.debuff_counter` | `NO_CURRENT_STABLE_SOURCE` | Suppress, resist, immunize or counter a negative status before or during application; separate from post-application `cleanse_power` | `StatusMitigation / Resistance` | true | false, pending user definition | `Unknown / no behavior change` |
| `capability.interrupt_timing` | `RUNTIME_EVENT_SIGNAL` | Interrupt within a valid enemy cast window before completion; owning control potential cannot prove timing or outcome | `RuntimeCounterWindow / EventSignal` | true | false | `Unknown / no behavior change` |
| `capability.spirit_lock` | `NO_CURRENT_STABLE_SOURCE` | Prevent spirit theft or energy-path cutoff at the protected source/path; separate from maintaining or recovering energy after disruption | `AntiDrainDefense structural/runtime channel` | true | false, pending user definition | `Unknown / no behavior change` |

These are the only primary classifications and recommended channels authorized for this Survey. They are not implemented migrations or approvals.

## Source eligibility summary

The row-level ledger is `BuildCapabilityRemainingBlockerSourceEvidence.csv`.

- Enemy vocabulary is qualified only for planning meaning, pressure vocabulary and applicability questions. It produces no Item value or event outcome.
- `ItemSystemSnapshot.v1` is qualified only as stable input geometry and placement state: `ShapeCells`, `OccupiedCells`, core/eye cells, `isLit` and `isCountedInBuild`. It contains no encounter pressure transform or approved layout-resilience predicate.
- `ItemInstancePlacementBindingContractSnapshot.v1` is qualified only for the explicit `itemInstanceId / placementId / baseItemId` join.
- `ItemCapabilityUnitContractSnapshot.v1` is native-unit authority only; none of its existing units creates these four meanings.
- `ItemCapabilityRuntimeFactContractSnapshot.v1` contains generic trigger, cleanse, chain and nian facts. It contains no cast-window interrupt outcome, debuff-resistance outcome, anti-drain prevention outcome or placement-pressure predicate.
- `BuildCapabilityReadContract.v1` is restricted to continuous `0..10000 basisPoint` capability values and availability. It cannot carry a structural predicate or runtime event signal.
- C02 and N01 reports are qualified only as fixed row and blocker-count baselines.

Disqualified inferences remain explicit:

- `shapeId` or occupied-cell count does not equal `placement_shape`.
- `cleanse_power`, `affix_debuff_target_power` and `target_has_debuff` do not equal `debuff_counter`.
- `control_power`, a control-point affix or possession of a control item does not prove `interrupt_timing` success.
- `energy_stability`, Item names, tags or localized copy do not prove `spirit_lock`.

## Missing, applicability and state semantics

| Capability | No relevant pressure | Applicable but incomplete | Complete authorized input | Offline BP disposition |
|---|---|---|---|---|
| `placement_shape` | `NotApplicable` | side-channel `Unknown` | bool / predicate result | `NOT_BP_CHANNEL` |
| `debuff_counter` | `NotApplicable` only after explicit applicability exists | `Unknown` | result defined by the future D2-selected source | no current stable producer |
| `interrupt_timing` | `NotApplicable` | `Unknown` when runtime horizon is incomplete | `KnownTrue / KnownFalse` for a complete horizon | `NotInChannel` |
| `spirit_lock` | `NotApplicable` | `Unknown` | future structural predicate or runtime result | no current stable producer |

`Unknown`, `NotApplicable` and `NotInChannel` are distinct and must not be interpreted as `KnownZero`. Explicit `KnownFalse` is possible only for a complete authorized runtime horizon; it is not missing evidence. Invalid identity, duplicate events, illegal facts or unit conflicts remain invalid inputs rather than false or zero.

## C02 actual and conditional impact

The fixed baseline is 8 Item scenarios by 4 encounters, exactly 32 rows. `BuildCapabilityRemainingBlockerC02Impact.csv` copies each row key and baseline Unknown set, carries forward N01's exact remaining decisive set, and applies every condition by per-row set subtraction.

Actual Survey result:

| Metric | Result |
|---|---:|
| Unknown-reference reduction | 0 |
| Projected blocked-row reduction | 0 |
| Remaining decisive Unknown references | 64 |
| Blocked rows | 32/32 |

Reference distribution and conditional ceiling:

| Condition | Covered decisive references | Conditionally unblocked rows |
|---|---:|---:|
| placement | 32 | 0 |
| debuff | 8 | 0 |
| interrupt | 8 | 0 |
| spirit | 16 | 0 |
| placement + debuff | 40 | 8 |
| placement + interrupt | 40 | 8 |
| placement + spirit | 48 | 16 |
| all four | 64 | 32 |

Every row contains `placement_shape` plus exactly one of `debuff_counter`, `interrupt_timing` or `spirit_lock`. Therefore every combination without placement leaves 32/32 rows blocked. The 8/8/16/32 conditional results require user approval, new schemas/contracts, approved N01 rules and any required Runtime Producer. They are not this Survey's actual result and are not readiness, difficulty, victory or reward conclusions.

## D1-D6 decision status

The full option and ownership ledger is `BuildCapabilityRemainingBlockerUserDecisionSheet.csv`. All six rows are `USER_DECISION_REQUIRED` with safe default `KEEP_UNKNOWN`.

| Decision | Required user choice | Guard recommendation, not approval |
|---|---|---|
| D1 | Placement predicate set and whether configured map pressure is simulated | Use a layout-resilience structural-predicate channel; predicate remains undecided |
| D2 | Resistance, duration/stack mitigation, immunity or runtime counter | Keep prevention separate from cleanse and choose a fact authority |
| D3 | Runtime horizon/outcome and whether Offline BP stops blocking | Use `RuntimeCounterWindow / EventSignal` after a new fact contract |
| D4 | Structural anti-drain protection or runtime prevention | Protect an explicit source/path and keep separate from energy recovery |
| D5 | Cross-channel applicability and summary suppression | Define state precedence before any consumer change |
| D6 | Enemy `requirementChannel` and applicability schema | Add only through a later separately approved schema contract |

No D1-D6 selection is made here. No later package is unlocked or started.

## Protected hashes

The verifier captures each protected scope before and after its read-only run.

| Scope | Count | Accepted / before / after | Result |
|---|---:|---|---|
| Item existing scope | 105 | `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` | PASS |
| Enemy | 89 | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` | PASS |
| C01 | 9 | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` | PASS |
| C02 | 9 | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` | PASS |
| C02A | 7 | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` | PASS |
| IF01 Canonical | - | `sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428` | PASS |
| IF02 Canonical | - | `sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673` | PASS |
| IF03 Canonical | - | `sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a` | PASS |
| Affix Schema | - | `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f` | PASS |
| 150 Roll | - | `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` | PASS |
| 150 Projection | - | `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` | PASS |
| Scenes | 14 | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | PASS |
| Prefabs | 16 | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | PASS |
| BuildSettings | 1 | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | PASS |
| N01 exact files | 8 | `03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52` | PASS |
| C02A-D1 exact files | 7 | `c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0` | PASS |

- HEAD baseline: `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba`

## Boundary statement

- Added artifacts are restricted to the Assignment's verifier, meta and six reports.
- Existing files modified by this package: `0`.
- Item/Enemy facts, C01/C02/C02A/C02A-D1/N01 and IF01/IF02/IF03 remain read-only.
- E08/E10 requirements, schemas, canonical signatures and readiness behavior changed: `0`.
- Runtime Producers, structural predicates, BP mappings and D1-D6 implementations created: `0`.
- Scene, Prefab, Config, UI, Battle, persistence and primary-flow touches: `0`.
- Commit, tag, push and next-package actions: `0`.

## Final survey status

All four capabilities remain `Unknown`. D1-D6 remain `USER_DECISION_REQUIRED`. N02, PA01, C02B, C02R1 and C03 remain `NOT_STARTED` and blocked pending their own Guard authorization.
