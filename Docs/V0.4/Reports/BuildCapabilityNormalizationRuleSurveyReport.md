# Build Capability Normalization Rule Survey Report

- Package: `V0.4-BuildCapabilityNormalizationRuleSurvey01`
- Guard receipt: `GUARD_PASS_BUILDCAPABILITYNORMALIZATIONRULESURVEY01`
- Nature: `REPORT_ONLY / CROSS_SYSTEM_RULE_SURVEY`
- Result: `PASS / REPORT_ONLY_SURVEY_COMPLETE`
- Existing-file modifications: `0`
- Formal mappings created: `0`
- Runtime Producers created: `0`
- Candidate approvals made for the user: `0`
- Survey completion state: `6/6 Unknown`
- Offline verifier: `PASS 219/219`
- Unity verifier: `COMPILE_PASS; VERIFY_PASS 219/219`
- Leak Count: `0`

This report normalizes evidence into candidate rule shapes only. It does not change Item facts, Enemy requirements, C01/C02/C02A, IF01/IF02/IF03, or any runtime consumer. Every projected C02 reduction is a conditional ceiling after explicit user approval, a future Rule Contract, a future mapping package, and—where stated—a complete Runtime Producer. The Survey itself reduces `0` Unknown references.

## Guard baseline correction validation

`GUARD_CORRECT_BUILDCAPABILITYNORMALIZATIONRULESURVEY01_SHA256_LENGTH01` supplies the accepted 105-file Item aggregate as the valid 64-character SHA-256 `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4`. The same 105-file pre-`Items/Capability*` scope is `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` before and after this package.

The inherited delta is the already-published upstream correction of `affix_nian_efficiency` from flat nian points to `nian_cost_basis_point / ReducePercent`. The current `ItemCompleteCandidateContent.cs` hash is `c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb`, exactly the file hash protected by the accepted IF03 report. Restoring the historical aggregate would therefore undo an upstream prerequisite and violate this Assignment's prohibition on modifying Item facts.

No other protected baseline, survey candidate, formula, impact row or user-decision content changed.

## Executive disposition

| Capability | Enemy planning meaning | Candidate count | Disposition after Survey | Primary blocker |
|---|---|---:|---|---|
| `capability.energy_stability` | Maintain the Build supply chain under resource reduction pressure | 2 | `KEEP_UNKNOWN` | User must choose potential versus realized model, net function, horizon, eligibility and aggregation; both candidates need Runtime facts |
| `capability.control_power` | Apply control and obtain counter-tempo | 2 | `KEEP_UNKNOWN` | No approved control-point-to-BP curve or multi-item aggregation |
| `capability.cleanse_power` | Remove negative-state or pollution effects | 2 | `KEEP_UNKNOWN` | No approved cleanse-stack-to-BP curve, runtime horizon or aggregation |
| `capability.placement_shape` | Maintain a usable layout through occupied-grid form and formation position | 0 | `SEMANTIC_UNRESOLVED / KEEP_UNKNOWN` | The vocabulary gives a high-level objective, but no benefit direction, pressure context, denominator, threshold or aggregation |
| `capability.burst_window` | Concentrate output or counter-action in a short window | 2 | `KEEP_UNKNOWN` | BP identity is unit-safe, but first-trigger semantic equivalence, sum/max choice, eligibility and runtime horizon are not approved |
| `capability.clear_power` | Handle multiple targets and summons | 2 | `KEEP_UNKNOWN` | Extra-target count is only a possible proxy; count-to-BP curve, semantic equivalence, horizon and aggregation are not approved |

Evidence for the six planning meanings is `DefaultEnemyMechanicVocabularyCatalog` in `Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs`. Enemy vocabulary is allowed to define what a capability key means; it is not allowed to contribute Item values or conversion constants.

## Contract and unit ledger

The target contract is `BuildCapabilityReadContract.v1`; its inclusive capability scale is the approved `0..10000 basisPoint` range. The target range is a clamp boundary, not a native-unit conversion.

| Native fact | Stable authority | Value domain | Unit | Approved native constants | Can become BP without a user-authored scale? |
|---|---|---|---|---|---|
| `affix_control_up` | `ItemCapabilityUnitContractSnapshot.v1` + complete instance projection | `control_point` | `point` | Range `1..10` | No. `CONTROL_BP_PER_POINT` or another curve is `USER_DECISION_REQUIRED` |
| `affix_cleanse_up` / realized extra stack | IF02 + IF03 | `cleanse_stack` | `stack` | Range `1..5` for affix; explicit runtime stack must be nonnegative | No. `CLEANSE_BP_PER_STACK` or another curve is `USER_DECISION_REQUIRED` |
| `affix_chain_target` | IF02 + complete instance projection | `target_count` | `count` | Range `1..6` | No. `CLEAR_BP_PER_TARGET_COUNT` or another curve is `USER_DECISION_REQUIRED` |
| IF03 consecutive trigger fact | `ItemCapabilityRuntimeFactContractSnapshot.v1` | `consecutive_trigger_count` | `count` | `chainCount3` is true at count `>=3` | Gate only. It is not interchangeable with `target_count` |
| `affix_trigger_refund` and IF03 nian facts | IF02 + IF03 | `nian_point` | `point` | Affix range `1..6`; runtime values nonnegative | No direct multiplier. A same-unit dimensionless ratio is possible only after the net function is approved |
| `affix_nian_efficiency` | IF02 + complete instance projection | `nian_cost_basis_point` | `basisPoint` | Range `200..1800` | Unit-safe identity is possible, but planning equivalence, runtime gate and aggregation still require approval |
| `affix_first_trigger_bonus` | IF02 + complete instance projection | `effect_basis_point` | `basisPoint` | Range `3000..8000`; successful first trigger is ordinal `1` | Unit-safe identity is possible, but semantic eligibility and aggregation still require approval |
| Placement geometry | `ItemSystemSnapshot.v1` | `shape_cell` / `occupied_cell` / booleans / IDs | `cell` / `bool` / `id` | No approved BP constants | No safe operational candidate exists |

`control_point` and `nian_point` remain different domains even though both display as `point`. `target_count` and `consecutive_trigger_count` remain different domains even though both display as `count`. `effect_basis_point` and `nian_cost_basis_point` share a target-compatible unit but remain semantically distinct facts. No row in this Survey directly exchanges these domains.

## Source eligibility

The full row-level decision is in `BuildCapabilityNormalizationSourceEligibilityMatrix.csv`.

Qualified calculation inputs are limited to:

1. A complete `ItemInstanceProjectionContractSnapshot.v1` for actual per-instance affix `rawUnits`.
2. A valid `ItemInstancePlacementBindingContractSnapshot.v1` for the exact `itemInstanceId + baseItemId + placementId` join.
3. A complete `ItemCapabilityUnitContractSnapshot.v1` for native domain, unit, operation and range validation.
4. A complete `ItemSystemSnapshot.v1` for placed, lit, Build-counted and geometry facts when the approved eligibility rule calls for them.
5. A complete `ItemCapabilityRuntimeFactContractSnapshot.v1` for event-conditioned candidates. IF03 is a contract only; there is currently no formal Producer.
6. `BuildCapabilityReadContract.v1` only for the target range and availability state.

Reference-only sources:

- Enemy vocabulary supplies planning meaning only.
- C02's fixed matrix supplies impact counting only.
- The Item balance workbench candidate payload may corroborate intent labels but is not numeric authority and is not battle-connected.

Ineligible shortcuts include possession without an explicit binding, equal-looking IDs without IF01, static projection used as proof that a runtime event fired, native range maxima used as arbitrary normalization denominators, and Enemy requirement values fed back into Item capability production.

## State semantics

| Situation | Candidate-level semantic | Total-capability semantic |
|---|---|---|
| No runtime event / untriggered, and no complete approved horizon | `Unknown` | `Unknown` |
| Complete IF03 row with a relevant condition `KnownFalse` | Explicit non-qualification for that event only after the candidate's zero boundary is approved; it is not missing | Known Zero is possible only when the complete event horizon, complete eligible item universe and approved rule all exist |
| Confirmed unplaced | Ineligible under candidates whose approved policy requires placement | Not automatically Known Zero; all eligible inputs must be complete |
| Placement fact missing | `Unknown` | `Unknown` |
| Confirmed unlit | Ineligible under candidates whose approved policy requires lighting | Not automatically Known Zero; all eligible inputs must be complete |
| Lighting fact missing | `Unknown` | `Unknown` |
| Missing projection, unit fact, IF01 binding, eligibility proof or required runtime fact | `Unknown` | `Unknown` |
| Incomplete IF03 fact or incomplete horizon | `Unknown` | `Unknown` |
| Unit conflict, identity mismatch, duplicate event/sequence, illegal or out-of-range value | `Invalid` | Invalid input must not be silently omitted or coerced |

Known Zero is therefore not a synonym for absent source, untriggered state, unplaced state, unlit state or incomplete facts. Every candidate retains `USER_DECISION_REQUIRED` for the exact zero boundary.

## Capability findings and candidates

### `capability.energy_stability`

Planning meaning: the Build's ability to maintain its supply chain under resource reduction pressure.

Eligible facts are the `nian_cost_basis_point` parameter from `affix_nian_efficiency`, the distinct `nian_point` refund source, and IF03 `nianCostBefore`, `nianCostAfter`, and `refundUnits`. IF01 identity and the approved placement policy are mandatory. Runtime participation is necessary in both candidates because the first source is conditionally active and the second candidate measures realized facts.

- `NRS01-ENERGY-A`: declared efficiency potential. Aggregate with `sum` across unique eligible item instances after a complete positive-cost runtime gate. Formula: `clamp(SUM(efficiencyBP_i), 0, 10000)`. The conversion is same-domain identity; sum semantics, eligibility and horizon remain `USER_DECISION_REQUIRED`. Refund points are deliberately excluded.
- `NRS01-ENERGY-B`: realized preservation. Aggregate as a weighted sum by native nian cost across unique events: `clamp(10000 * SUM(ENERGY_NET_NIAN(before,after,refund)) / SUM(before), 0, 10000)`. `ENERGY_NET_NIAN`, refund treatment, horizon, weighting and zero-denominator behavior remain symbolic and `USER_DECISION_REQUIRED`. This formula divides same-unit nian-point quantities; it never adds BP parameters to points.

Clamp is limited to the approved target range. `nianCostBefore>0` is evidence-backed by the source condition; no further threshold or cap is introduced. Both candidates are `NEEDS_RUNTIME_PRODUCER` and remain `KEEP_UNKNOWN`.

### `capability.control_power`

Planning meaning: ability to apply control and obtain counter-tempo.

The stable native source is `affix_control_up.rawUnits` in `control_point`. It is static and does not need Runtime facts, but it still requires complete projection, IF02, IF01 and the approved placement eligibility set.

- `NRS01-CONTROL-A`: additive model, `clamp(SUM(controlPoint_i * CONTROL_BP_PER_POINT), 0, 10000)`.
- `NRS01-CONTROL-B`: strongest-source model, `clamp(MAX(controlPoint_i) * CONTROL_BP_PER_POINT, 0, 10000)`.

`CONTROL_BP_PER_POINT`, eligibility, the sum/max choice, empty-set behavior and any extra cap remain `USER_DECISION_REQUIRED`. Weighted sum has no approved weights; count would discard native magnitudes. Both candidates remain `KEEP_UNKNOWN`.

### `capability.cleanse_power`

Planning meaning: ability to remove negative-state or pollution effects.

The stable native value is `cleanse_stack`; IF03 exposes both `cleanseSuccess` truth and `cleanseExtraStackCount`. A static affix does not prove a cleanse occurred. A Producer and a complete approved runtime horizon are required.

- `NRS01-CLEANSE-A`: declared successful capacity, `sum` once per unique successful item instance: `clamp(SUM(declaredStack_i * CLEANSE_BP_PER_STACK), 0, 10000)`.
- `NRS01-CLEANSE-B`: peak realized event, `max` over `cleanseExtraStackCount`: `clamp(MAX(realizedStack_event) * CLEANSE_BP_PER_STACK, 0, 10000)`.

`CLEANSE_BP_PER_STACK`, declared-versus-realized meaning, horizon, eligibility, sum/max and zero boundary remain `USER_DECISION_REQUIRED`. No unapproved threshold or per-event cap is introduced. Both candidates are `NEEDS_RUNTIME_PRODUCER` and remain `KEEP_UNKNOWN`.

### `capability.placement_shape`

The high-level planning meaning is supported: the Build maintains a usable layout through occupied-grid form and formation position. This is not equivalent to "more occupied cells" or "fewer occupied cells". A larger shape can provide coverage but consume scarce space; a compact shape can fit but provide no evidence of pressure resilience. `isLit`, `isCountedInBuild` and active-core IDs prove state, not a BP weight.

Two plausible semantic interpretations were surveyed but are not promoted to rule candidates:

1. Shape adaptability/compactness relative to board and encounter placement pressure.
2. Maintained usable layout, measuring whether bound placed items remain lit, counted and geometrically consistent under pressure.

Neither interpretation has an approved pressure context, benefit direction, denominator, threshold, cap or multi-item aggregation. Candidate count is therefore `0`, disposition is `SEMANTIC_UNRESOLVED / KEEP_UNKNOWN`, projected Unknown reduction is `0`, and all 32 affected C02 rows retain this decisive Unknown.

### `capability.burst_window`

Planning meaning: ability to concentrate output or counter-action in a short window.

`affix_first_trigger_bonus` supplies `effect_basis_point`; IF03 supplies `triggerSuccess`, `firstTriggerInBattle` and `triggerOrdinal`. The direct BP identity is unit-safe only when a complete runtime fact proves a successful ordinal-1 event for the same IF01-bound item.

- `NRS01-BURST-A`: `sum` once per qualified item instance per battle, clamped to `0..10000`.
- `NRS01-BURST-B`: `max` qualified first-trigger bonus per battle, clamped to `0..10000`.

There is no guessed numeric scale. User approval is still required for first-trigger-to-burst semantic equivalence, eligibility, sum/max, battle horizon and zero boundary. Both candidates are `NEEDS_RUNTIME_PRODUCER` and remain `KEEP_UNKNOWN`.

### `capability.clear_power`

Planning meaning: ability to handle multiple targets and summons.

`affix_chain_target.rawUnits` is `target_count`. IF03 `consecutiveTriggerCount` belongs to a different count domain and only proves the contracted `chainCount3` gate. The gate is true at `>=3`; the consecutive count must never be used as the extra-target contribution.

- `NRS01-CLEAR-A`: additive extra-target breadth, `clamp(SUM(targetCount_i * CLEAR_BP_PER_TARGET_COUNT), 0, 10000)` once per eligible item in the horizon.
- `NRS01-CLEAR-B`: strongest extra-target breadth, `clamp(MAX(targetCount_i) * CLEAR_BP_PER_TARGET_COUNT, 0, 10000)`.

`CLEAR_BP_PER_TARGET_COUNT`, extra-target-to-clear semantic equivalence, eligibility, horizon, sum/max and zero boundary remain `USER_DECISION_REQUIRED`. Both candidates are `NEEDS_RUNTIME_PRODUCER` and remain `KEEP_UNKNOWN`.

## Clamp, thresholds and caps

Existing approved numeric constants used by the candidates are limited to:

- Target bounds `0` and `10000` from `BuildCapabilityReadContract.v1`.
- IF02 native ranges: control `1..10`, cleanse `1..5`, target count `1..6`, refund `1..6`, first-trigger BP `3000..8000`, nian-efficiency BP `200..1800`.
- IF03 logical gates: successful first trigger uses ordinal `1`; `chainCount3` corresponds to consecutive count `>=3`; runtime numeric facts are nonnegative.

IF02 ranges validate native facts; they are not normalization denominators. No candidate introduces an unapproved per-item cap, total threshold, weight, slope, breakpoint or saturation point. The mandatory target clamp is not permission to guess the curve beneath it.

Symbolic parameters retained as `USER_DECISION_REQUIRED` include:

- `CONTROL_BP_PER_POINT`
- `CLEANSE_BP_PER_STACK`
- `CLEAR_BP_PER_TARGET_COUNT`
- `ENERGY_NET_NIAN(before, after, refund)` and refund inclusion
- Placement objective, pressure context, benefit direction and denominator
- Eligibility policy, runtime horizon, sum/max semantics, empty-set and zero-boundary policies

## C02 conditional impact

The fixed baseline is exactly `8 item scenarios x 4 encounters = 32 rows`. Impact is counted only as a reduction of an existing capability Unknown reference. Every single candidate still leaves at least one other decisive Unknown in each affected row.

| Capability | Candidate IDs | Affected encounters | Affected C02 rows / conditional Unknown references removed per candidate | Single-candidate blocked-row reduction |
|---|---|---:|---:|---:|
| `capability.energy_stability` | A / B | 4 | 32 | 0 |
| `capability.control_power` | A / B | 4 | 32 | 0 |
| `capability.cleanse_power` | A / B | 4 | 32 | 0 |
| `capability.placement_shape` | none | 4 | 0 | 0 |
| `capability.burst_window` | A / B | 3 | 24 | 0 |
| `capability.clear_power` | A / B | 2 | 16 | 0 |

Alternative A/B candidates for the same capability are mutually exclusive. Under the conditional combination "one user-approved candidate per candidate-bearing capability"—energy, control, cleanse, burst and clear—the 32-row matrix would remove `136` Unknown references:

- `dev_encounter_3_10_cleanse_corner`: `3` per scenario, `24` total; remaining `capability.debuff_counter` and `capability.placement_shape`.
- `dev_encounter_3_10_guard_wall`: `4` per scenario, `32` total; remaining `capability.interrupt_timing` and `capability.placement_shape`.
- `dev_encounter_4_10_furnace_core`: `5` per scenario, `40` total; remaining `capability.placement_shape` and `capability.spirit_lock`.
- `dev_encounter_4_10_thunder_fire_cross`: `5` per scenario, `40` total; remaining `capability.placement_shape` and `capability.spirit_lock`.

The conditional remaining decisive Unknown-reference total is `64`: placement shape `32`, debuff counter `8`, interrupt timing `8`, and spirit lock `16`.

Projected blocked-row reduction: `0`

The row-by-row evidence is `BuildCapabilityNormalizationC02ImpactMatrix.csv`. Current Survey effect remains `0` because no candidate is approved, no rule is implemented, no mapping is expanded, and no Runtime Producer exists.

## User decisions required before implementation

The writable decision fields remain `PENDING` in `BuildCapabilityNormalizationUserDecisionSheet.csv`. The user must decide:

1. Potential versus realized energy meaning, the realized net nian function, refund treatment, horizon and weighting.
2. Point/stack/count normalization curves or scale anchors; no numeric values are preselected.
3. Same-domain basis-point identity semantics and sum versus max across instances.
4. Whether eligibility requires placed, lit and/or `isCountedInBuild`; mere possession is not assumed.
5. Runtime time domain: event, item-in-battle, whole Build battle, or another explicit complete horizon.
6. KnownFalse, confirmed unplaced and confirmed unlit contribution boundaries, plus the requirements for total Known Zero.
7. The operational meaning and geometry function for `placement_shape`.
8. Extra-target-to-clear and first-trigger-to-burst planning equivalence.
9. Any additional per-item, event or total cap/threshold beyond the approved target range.
10. At most one candidate per capability for a future Rule Contract, explicit rejection/deferment of all others, and separate authorization for any Runtime Producer package.

No decision in this Survey authorizes a mapping expansion or starts a later package.

## Protected hashes

The verifier captures each scope before and after its read-only run. Accepted baselines must remain exact; no baseline refresh is allowed.

| Scope | Count | Accepted / before / after | Result |
|---|---:|---|---|
| Item existing scope | 105 | accepted / before / after `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` | PASS |
| Enemy | 89 | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` | PASS |
| C01 | 9 | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` | PASS |
| C02 | 9 | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` | PASS |
| C02A | 7 | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` | PASS |
| IF01 Canonical | — | `sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428` | PASS |
| IF02 Canonical | — | `sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673` | PASS |
| IF03 Canonical | — | `sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a` | PASS |
| Affix Schema | — | `sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f` | PASS |
| 150 Roll | — | `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` | PASS |
| 150 Projection | — | `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` | PASS |
| Scenes | 14 | `da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b` | PASS |
| Prefabs | 16 | `7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087` | PASS |
| BuildSettings | 1 | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` | PASS |

## Boundary statement

- Added artifacts are restricted to the Assignment's verifier, meta and six reports.
- Existing files modified by this package: `0`.
- Item/Enemy facts, C01/C02/C02A/C02A-D1, IF01/IF02/IF03 and all memory/queue files remain read-only.
- Formal normalization mapping, C02B and Runtime Producer implementation count: `0`.
- Scene, Prefab, Config, UI, Battle, persistence and primary-flow touches: `0`.
- Package/commit/tag/push/next-package actions: `0`.

## Final rule status

All ten candidates are `NOT_IMPLEMENTATION_READY`, `USER_DECISION_REQUIRED`, and `KEEP_UNKNOWN`. `capability.placement_shape` has no safe candidate and remains `SEMANTIC_UNRESOLVED`. Consequently all six requested capabilities remain Unknown after this Survey.
