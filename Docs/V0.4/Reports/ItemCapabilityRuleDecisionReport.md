# Item Capability Rule Decision Report

- Package: `V0.4-ItemCapabilityRuleDecision01`
- Guard receipt: `GUARD_PASS_ITEMCAPABILITYRULEDECISION01`
- Execution: `Unity batch compile/verifier`
- Result: `PASS`
- Scope: `read-only rule decision material; no mapping implementation`
- Capability decisions: `6/6`
- Rule candidates: `1`
- Unit-safe candidates: `1`
- Capabilities with no unit-safe rule: `5`
- Remaining Unknown: `capability.energy_stability;capability.control_power;capability.cleanse_power;capability.placement_shape;capability.burst_window;capability.clear_power`
- Existing files modified by this package: `0`
- Mapping/status changes: `0`
- Candidate approvals: `0`
- Leak Count: `0`
- Offline verifier: `PASS`
- Unity batch verifier: `PASS`

No candidate is approved by this report. All six values remain sparse omissions/Unknown until the user selects a rule and Guard authorizes a separate implementation package.

## Decision summary

| Capability | Candidates | Unit-safe | E10 refs | E08 blocker refs | Affected encounters | Candidate blocker reduction | Blocked-row reduction | Disposition |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| capability.energy_stability | 0 | 0 | 11 | 32 | 4 | 0 | 0 | NO_UNIT_SAFE_RULE / KEEP_UNKNOWN |
| capability.control_power | 0 | 0 | 10 | 32 | 4 | 0 | 0 | NO_UNIT_SAFE_RULE / KEEP_UNKNOWN |
| capability.cleanse_power | 0 | 0 | 8 | 32 | 4 | 0 | 0 | NO_UNIT_SAFE_RULE / KEEP_UNKNOWN |
| capability.placement_shape | 0 | 0 | 7 | 32 | 4 | 0 | 0 | NO_UNIT_SAFE_RULE / KEEP_UNKNOWN |
| capability.burst_window | 1 | 1 | 5 | 24 | 3 | 40 | 0 | NEEDS_ITEM_GUARD_FACT / KEEP_UNKNOWN |
| capability.clear_power | 0 | 0 | 2 | 16 | 2 | 0 | 0 | NO_UNIT_SAFE_RULE / KEEP_UNKNOWN |

Impact counts are hypothetical only. For the sole candidate, `40 = 24 E08 requirement-Unknown references + 8 rule-confirmation references + 8 C01-Unknown references after a future C02B mapping`. Candidate approval alone can address only the 8 rule-confirmation references. The projected C02 blocked-row reduction is `0` because every affected row retains other decisive Unknown capabilities.

## Candidate

### ICRD01-BURST-A — capability.burst_window

- Source contract: `ItemInstanceProjectionContractSnapshot.v1 + ItemAffixPoolAndRangeSchemaSnapshot.v1 + ItemSystemSnapshot.v1 + REQUIRED_FUTURE_RUNTIME_TRIGGER_FACT`
- Stable/current members: `Affixes.affixId=affix_first_trigger_bonus; Affixes.rawUnits; AffixDefinition.valueUnitKey; placement binding; placement.isLit; REQUIRED_FUTURE_RUNTIME_TRIGGER_FACT(on_first_trigger,first_trigger_in_battle)`
- Source unit: `basisPoint`
- Eligibility: Complete valid projection/schema and explicit matching lit placement; count a source only when a future stable runtime contract proves its on_first_trigger + first_trigger_in_battle condition. Do not require main-Build membership and do not count mere possession.
- Aggregation: Saturating sum once per qualified itemInstanceId; deterministic itemInstanceId ordering; duplicate source identity is invalid/Unknown.
- Conversion: Identity: contributionBP = affix.rawUnits.
- Clamp/threshold: Any negative source is Unknown; each source and aggregate clamp to BuildCapabilityScale 0..10000.
- Missing source: Incomplete/invalid input, absent trigger proof, unit mismatch, missing binding, or identity mismatch is Unknown.
- Known Zero boundary: After rule approval and a complete runtime fact only, a complete eligible set with zero condition-satisfied sources is Known Zero; current absence remains Unknown.
- Runtime dependency: `YES_REQUIRED_FUTURE_STABLE_RUNTIME_TRIGGER_FACT`
- Affected encounters: `dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross`
- Advantages: Direct BP identity and the existing C01 saturating-sum precedent avoid inventing a numeric conversion.
- Risks: Conditional trigger cannot be treated as static; candidate payload is DEV_ONLY/BALANCE_CANDIDATE/NOT_BATTLE_CONNECTED; cross-item sum semantics are not approved.
- Recommendation: `NEEDS_ITEM_GUARD_FACT`; safe default remains `KEEP_UNKNOWN`.
- Evidence: `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs;Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset;Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs;Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs;Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs;Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs;Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs` / `affix_first_trigger_bonus; ItemInstanceProjectionAffixSnapshot.rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey; ItemSystemPlacementSnapshot.isLit; BuildCapabilityScale.MaximumValueBasisPoints; DIRECT_BASIS_POINT_SATURATING_SUM_0_10000`

This candidate is unit-safe but not executable today: the BP identity is stable, while the trigger qualification fact is not. Static possession must not be interpreted as the conditional trigger having occurred.

## No-unit-safe decisions

### capability.energy_stability

- Stable facts: affix_nian_efficiency schema definition and candidate payload; affix_trigger_refund candidate payload
- Source units: `affix_nian_efficiency: schema basisPoint but payload point; affix_trigger_refund: point`
- Eligibility: No safe eligibility rule can be evaluated before the nian-efficiency schema/payload conflict is resolved; both source effects are conditional.
- Aggregation: `NO_UNIT_SAFE_AGGREGATION`
- Conversion: `NO_UNIT_SAFE_CONVERSION; no ratio is selected`
- Clamp/threshold: `NO_UNIT_SAFE_CLAMP_OR_THRESHOLD`
- Missing source: Any missing, conflicting, incomplete, or conditional source remains Unknown.
- Known Zero boundary: Known Zero is unavailable while the source contract conflicts; absence is not zero.
- Runtime dependency: YES: before_trigger/nian_cost_gt_0 and after_trigger/trigger_success are runtime conditions.
- Affected encounters: `dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross`
- Impact ceiling if a later safe rule exists: `32 E08 refs; 48 total blocker refs; 0 currently projected blocked rows`.
- Risk: Schema/payload unit conflict plus two different resource semantics; a guessed ratio would corrupt the BP contract.
- Disposition: `NO_UNIT_SAFE_RULE / KEEP_UNKNOWN`
- Evidence: `Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset;Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs` / `affix_nian_efficiency unitKey/valueUnitKey; BuildRandomAffixDictionary resource candidates`

### capability.control_power

- Stable facts: ItemInstanceProjectionContractSnapshot.Affixes[affix_control_up].rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey
- Source units: `point`
- Eligibility: A complete projection and schema could identify a lit placed affix source, but eligibility does not create a point-to-BP scale.
- Aggregation: `NO_UNIT_SAFE_AGGREGATION until conversion and cross-item stacking are approved`
- Conversion: `NO_UNIT_SAFE_CONVERSION; point-to-BP ratio is absent`
- Clamp/threshold: `NO_EVIDENCE_BASED_THRESHOLD`
- Missing source: Missing projection, schema, binding, placement, or conversion remains Unknown.
- Known Zero boundary: Known Zero is unavailable until a complete rule and complete eligible source set exist.
- Runtime dependency: NO for the always/NONE payload itself; current static facts are sufficient only to identify a source, not to convert it.
- Affected encounters: `dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross`
- Impact ceiling if a later safe rule exists: `32 E08 refs; 48 total blocker refs; 0 currently projected blocked rows`.
- Risk: The unconditional target is direct, but the generic 0..10000 BP capability scale supplies no point normalization.
- Disposition: `NO_UNIT_SAFE_RULE / KEEP_UNKNOWN`
- Evidence: `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs;Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` / `BuildRandomAffixDictionary / affix_control_up; ItemInstanceProjectionAffixSnapshot.rawUnits`

### capability.cleanse_power

- Stable facts: ItemInstanceProjectionContractSnapshot.Affixes[affix_cleanse_up].rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey
- Source units: `stack`
- Eligibility: A source would require a complete lit placement binding and a successful cleanse runtime event; static possession is not eligibility.
- Aggregation: `NO_UNIT_SAFE_AGGREGATION until stack conversion and event-scoped stacking are approved`
- Conversion: `NO_UNIT_SAFE_CONVERSION; stack-to-BP ratio is absent`
- Clamp/threshold: `NO_EVIDENCE_BASED_THRESHOLD`
- Missing source: Missing runtime success, projection, schema, binding, placement, or conversion remains Unknown.
- Known Zero boundary: No event or missing event evidence is Unknown, not zero; Known Zero requires a future complete event fact and approved rule.
- Runtime dependency: YES: on_cleanse/cleanse_success.
- Affected encounters: `dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross`
- Impact ceiling if a later safe rule exists: `32 E08 refs; 48 total blocker refs; 0 currently projected blocked rows`.
- Risk: Conditional event semantics and stack units both block a static BP rule.
- Disposition: `NO_UNIT_SAFE_RULE / KEEP_UNKNOWN`
- Evidence: `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs;Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` / `BuildRandomAffixDictionary / affix_cleanse_up; ItemInstanceProjectionAffixSnapshot.rawUnits`

### capability.placement_shape

- Stable facts: ItemSystemCatalogItemSnapshot.shapeId/ShapeCells; ItemSystemPlacementSnapshot.OccupiedCells/isLit/isCountedInBuild/ActiveCoreEffectIds
- Source units: `cells; booleans; categorical IDs`
- Eligibility: The snapshot can prove placement and lighting, but no approved subset says whether size, compactness, lighting, Build membership, or core activity constitutes this capability.
- Aggregation: `NO_SEMANTICALLY_SAFE_AGGREGATION`
- Conversion: `NO_UNIT_SAFE_CONVERSION from geometry/categorical facts to BP`
- Clamp/threshold: `NO_APPROVED_DIRECTION_DENOMINATOR_OR_THRESHOLD`
- Missing source: Missing snapshot or binding remains Unknown; a present shape without an approved meaning also remains Unknown.
- Known Zero boundary: Empty placements cannot become Known Zero before the capability meaning and complete input boundary are approved.
- Runtime dependency: NO for current snapshot facts; YES only if a later rule chooses a runtime-only placement outcome, which this package does not.
- Affected encounters: `dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross`
- Impact ceiling if a later safe rule exists: `32 E08 refs; 48 total blocker refs; 0 currently projected blocked rows`.
- Risk: Stable geometry exists, but benefit direction and normalization do not; inventing either would create game rules.
- Disposition: `NO_UNIT_SAFE_RULE / KEEP_UNKNOWN`
- Evidence: `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` / `ItemSystemCatalogItemSnapshot; ItemSystemPlacementSnapshot`

### capability.clear_power

- Stable facts: ItemInstanceProjectionContractSnapshot.Affixes[affix_chain_target].rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey
- Source units: `count`
- Eligibility: A source would require a complete lit placement binding plus a stable runtime fact proving on_consecutive_trigger and chain_count_3; static possession is not eligibility.
- Aggregation: `NO_UNIT_SAFE_AGGREGATION until target-count conversion and event-scoped stacking are approved`
- Conversion: `NO_UNIT_SAFE_CONVERSION; count-to-BP ratio is absent`
- Clamp/threshold: `NO_EVIDENCE_BASED_THRESHOLD beyond the source condition, which still has no BP meaning`
- Missing source: Missing runtime chain fact, projection, schema, binding, placement, or conversion remains Unknown.
- Known Zero boundary: No chain event or missing chain evidence is Unknown, not zero; Known Zero requires a future complete event fact and approved rule.
- Runtime dependency: YES: on_consecutive_trigger/chain_count_3.
- Affected encounters: `dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross`
- Impact ceiling if a later safe rule exists: `16 E08 refs; 32 total blocker refs; 0 currently projected blocked rows`.
- Risk: Extra-target count may relate to clear breadth, but neither semantic equivalence nor normalization is contracted.
- Disposition: `NO_UNIT_SAFE_RULE / KEEP_UNKNOWN`
- Evidence: `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs;Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` / `BuildRandomAffixDictionary / affix_chain_target; ItemInstanceProjectionAffixSnapshot.rawUnits`

## Protected hashes

| Scope | Files | Before | After | Same |
|---|---:|---|---|---|
| Item | 105 | `7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663` | `7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663` | YES |
| Enemy | 89 | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` | `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae` | YES |
| C01 | 9 | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` | `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a` | YES |
| C02 | 9 | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` | `08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4` | YES |
| C02A | 7 | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` | `a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb` | YES |

## Verifier checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| decision.count | 6 | 6 | PASS |
| decision.keys | capability.energy_stability;capability.control_power;capability.cleanse_power;capability.placement_shape;capability.burst_window;capability.clear_power | capability.energy_stability;capability.control_power;capability.cleanse_power;capability.placement_shape;capability.burst_window;capability.clear_power | PASS |
| decision.safe-defaults | 6 KEEP_UNKNOWN | 6 | PASS |
| candidate.count.total | 1 | 1 | PASS |
| candidate.max-two-per-capability | true | True | PASS |
| candidate.unit-safe.count | 1 | 1 | PASS |
| capability.no-unit-safe.count | 5 | 5 | PASS |
| energy.no-candidate | 0 | 0 | PASS |
| energy.disposition | NO_UNIT_SAFE_RULE / KEEP_UNKNOWN | NO_UNIT_SAFE_RULE / KEEP_UNKNOWN | PASS |
| burst.runtime-gated | first_trigger_in_battle | Complete valid projection/schema and explicit matching lit placement; count a source only when a future stable runtime contract proves its on_first_trigger + first_trigger_in_battle condition. Do not require main-Build membership and do not count mere possession. | PASS |
| burst.not-static-possession | do not count mere possession | Complete valid projection/schema and explicit matching lit placement; count a source only when a future stable runtime contract proves its on_first_trigger + first_trigger_in_battle condition. Do not require main-Build membership and do not count mere possession. | PASS |
| burst.identity-conversion | Identity BP | Identity: contributionBP = affix.rawUnits. | PASS |
| burst.keep-unknown-disposition | NEEDS_ITEM_GUARD_FACT / KEEP_UNKNOWN | NEEDS_ITEM_GUARD_FACT / KEEP_UNKNOWN | PASS |
| candidate.required-fields | all non-empty | all non-empty | PASS |
| c02.matrix.rows | 32 | 32 | PASS |
| c02.matrix.scenarios | 8 | 8 | PASS |
| c02.matrix.encounters | 4 | 4 | PASS |
| impact.rows | 192 | 192 | PASS |
| impact.no-readiness-output | baseline status only | baseline status only | PASS |
| coverage.refs.capability.energy_stability | 11 | 11 | PASS |
| coverage.blocked.capability.energy_stability | 32 | 32 | PASS |
| blockers.requirement.capability.energy_stability | 32 | 32 | PASS |
| blockers.c01.capability.energy_stability | 8 | 8 | PASS |
| blockers.rule.capability.energy_stability | 8 | 8 | PASS |
| blockers.encounters.capability.energy_stability | 4 | 4 | PASS |
| blockers.encounter-ids.capability.energy_stability | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | PASS |
| coverage.refs.capability.control_power | 10 | 10 | PASS |
| coverage.blocked.capability.control_power | 32 | 32 | PASS |
| blockers.requirement.capability.control_power | 32 | 32 | PASS |
| blockers.c01.capability.control_power | 8 | 8 | PASS |
| blockers.rule.capability.control_power | 8 | 8 | PASS |
| blockers.encounters.capability.control_power | 4 | 4 | PASS |
| blockers.encounter-ids.capability.control_power | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | PASS |
| coverage.refs.capability.cleanse_power | 8 | 8 | PASS |
| coverage.blocked.capability.cleanse_power | 32 | 32 | PASS |
| blockers.requirement.capability.cleanse_power | 32 | 32 | PASS |
| blockers.c01.capability.cleanse_power | 8 | 8 | PASS |
| blockers.rule.capability.cleanse_power | 8 | 8 | PASS |
| blockers.encounters.capability.cleanse_power | 4 | 4 | PASS |
| blockers.encounter-ids.capability.cleanse_power | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | PASS |
| coverage.refs.capability.placement_shape | 7 | 7 | PASS |
| coverage.blocked.capability.placement_shape | 32 | 32 | PASS |
| blockers.requirement.capability.placement_shape | 32 | 32 | PASS |
| blockers.c01.capability.placement_shape | 8 | 8 | PASS |
| blockers.rule.capability.placement_shape | 8 | 8 | PASS |
| blockers.encounters.capability.placement_shape | 4 | 4 | PASS |
| blockers.encounter-ids.capability.placement_shape | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | PASS |
| coverage.refs.capability.burst_window | 5 | 5 | PASS |
| coverage.blocked.capability.burst_window | 24 | 24 | PASS |
| blockers.requirement.capability.burst_window | 24 | 24 | PASS |
| blockers.c01.capability.burst_window | 8 | 8 | PASS |
| blockers.rule.capability.burst_window | 8 | 8 | PASS |
| blockers.encounters.capability.burst_window | 3 | 3 | PASS |
| blockers.encounter-ids.capability.burst_window | dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | PASS |
| coverage.refs.capability.clear_power | 2 | 2 | PASS |
| coverage.blocked.capability.clear_power | 16 | 16 | PASS |
| blockers.requirement.capability.clear_power | 16 | 16 | PASS |
| blockers.c01.capability.clear_power | 8 | 8 | PASS |
| blockers.rule.capability.clear_power | 8 | 8 | PASS |
| blockers.encounters.capability.clear_power | 2 | 2 | PASS |
| blockers.encounter-ids.capability.clear_power | dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross | PASS |
| candidate.impact.requirement | 24 | 24 | PASS |
| candidate.impact.rule | 8 | 8 | PASS |
| candidate.impact.c01 | 8 | 8 | PASS |
| candidate.impact.total | 40 | 40 | PASS |
| candidate.impact.blocked-rows | 0 | 0 | PASS |
| no-candidate.impact.zero | true | True | PASS |
| source.energy.schema-bp | basisPoint near affix_nian_efficiency | True | PASS |
| source.energy.payload-point | point near affix_nian_efficiency | True | PASS |
| source.energy.refund-point | point near affix_trigger_refund | True | PASS |
| source.control.point | point near affix_control_up | True | PASS |
| source.cleanse.stack | stack near affix_cleanse_up | True | PASS |
| source.burst.bp | basisPoint near affix_first_trigger_bonus | True | PASS |
| source.burst.condition | first_trigger_in_battle near affix_first_trigger_bonus | True | PASS |
| source.clear.count | count near affix_chain_target | True | PASS |
| source.clear.condition | chain_count_3 near affix_chain_target | True | PASS |
| source.payload.maturity | BALANCE_CANDIDATE | BALANCE_CANDIDATE | PASS |
| source.payload.not-battle-connected | NOT_BATTLE_CONNECTED | NOT_BATTLE_CONNECTED | PASS |
| contract.projection.affix | affixId/rawUnits | affixId/rawUnits | PASS |
| contract.affix.unit | valueUnitKey | valueUnitKey | PASS |
| contract.item.placement | isLit/isCountedInBuild/OccupiedCells | isLit/isCountedInBuild/OccupiedCells | PASS |
| contract.capability.scale | MaximumValueBasisPoints = 10000 | MaximumValueBasisPoints = 10000 | PASS |
| c01.bp-precedent | DIRECT_BASIS_POINT_SATURATING_SUM_0_10000 | DIRECT_BASIS_POINT_SATURATING_SUM_0_10000 | PASS |
| c01.reject-unit-guess | no conversion is guessed | no conversion is guessed | PASS |
| leak.count | 0 | 0 | PASS |
| protected.item.file-count | 105 | 105 | PASS |
| protected.item.accepted-hash | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | PASS |
| protected.item.before-after | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | PASS |
| protected.enemy.file-count | 89 | 89 | PASS |
| protected.enemy.accepted-hash | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | PASS |
| protected.enemy.before-after | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | PASS |
| protected.c01.file-count | 9 | 9 | PASS |
| protected.c01.accepted-hash | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | PASS |
| protected.c01.before-after | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | PASS |
| protected.c02.file-count | 9 | 9 | PASS |
| protected.c02.accepted-hash | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | PASS |
| protected.c02.before-after | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | PASS |
| protected.c02a.file-count | 7 | 7 | PASS |
| protected.c02a.accepted-hash | a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb | a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb | PASS |
| protected.c02a.before-after | a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb | a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb | PASS |
| output.allowed-count | 5 | 5 | PASS |
| output.verifier-exists | true | True | PASS |
| output.directory-exists | true | True | PASS |
| reports.render-deterministic | true | True | PASS |

## Forbidden scope

- Item-to-capability mapping implementation: `0`
- Supported/Known Zero status changes: `0`
- Item / Enemy / Runtime / Battle / Board changes: `0`
- Scene / Prefab / Config changes: `0`
- Formal readiness, difficulty, win/loss, or drop conclusions: `0`
- C02B / C02R1 / C03 starts: `0`
- Commit / tag / push: `0`
