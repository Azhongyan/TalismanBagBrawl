# Item Capability Mapping Gap Survey Report

- Package: `V0.4-ItemCapabilityMappingGapSurvey01`
- Guard receipt: `GUARD_PASS_ITEMCAPABILITYMAPPINGGAPSURVEY01`
- Mode: `Unity batch compile/verifier`
- Result: `PASS`
- Survey scope: `16/16 BuildCapability keys; read-only evidence and decisions only`
- Mapping implementation count: `0`
- C01 FieldMap modifications: `0`
- Direct mappable: `0`
- Decisions: `6`
- Leak count: `0`
- Unity validation status: `PASS`

## Classification counts

| Classification | Count |
|---|---:|
| DIRECT_MAPPABLE_EXISTING_FACT | 0 |
| MAPPABLE_REQUIRES_RULE_CONFIRMATION | 6 |
| REQUIRES_RUNTIME_SIGNAL | 2 |
| NO_STABLE_ITEM_SOURCE | 2 |
| NOT_REQUIRED_BY_E10 | 3 |
| ALREADY_SUPPORTED | 3 |

## 16-key classification

| Capability | C01 | E10 refs | C02 blocked rows | Classification | Confidence | Next action |
|---|---|---:|---:|---|---|---|
| capability.break_power | SUPPORTED | 2 | 16 | ALREADY_SUPPORTED | HIGH | NONE_C01_READ_ONLY |
| capability.burst_window | UNKNOWN_NOT_MAPPED | 5 | 24 | MAPPABLE_REQUIRES_RULE_CONFIRMATION | MEDIUM | DECISION_IMCGS01_BURST_KEEP_UNKNOWN |
| capability.caster_interrupt | OUT_OF_SCOPE | 0 | 0 | REQUIRES_RUNTIME_SIGNAL | HIGH | KEEP_OUT_OF_C02B_STATIC_MAPPING_RUNTIME_CONTRACT_REQUIRED |
| capability.chain_reaction | UNKNOWN_NOT_MAPPED | 0 | 0 | NOT_REQUIRED_BY_E10 | HIGH | DEFER_E10_SCOPE_KEEP_C01_STATUS |
| capability.cleanse_power | UNKNOWN_NOT_MAPPED | 8 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION | MEDIUM | DECISION_IMCGS01_CLEANSE_KEEP_UNKNOWN |
| capability.clear_power | UNKNOWN_NOT_MAPPED | 2 | 16 | MAPPABLE_REQUIRES_RULE_CONFIRMATION | MEDIUM | DECISION_IMCGS01_CLEAR_KEEP_UNKNOWN |
| capability.control_power | UNKNOWN_NOT_MAPPED | 10 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION | MEDIUM | DECISION_IMCGS01_CONTROL_KEEP_UNKNOWN |
| capability.cooldown_recovery | SUPPORTED | 0 | 0 | ALREADY_SUPPORTED | HIGH | NONE_C01_READ_ONLY |
| capability.debuff_counter | UNKNOWN_NOT_MAPPED | 1 | 8 | NO_STABLE_ITEM_SOURCE | HIGH | REQUEST_STABLE_ITEM_CONTRACT_SOURCE_OR_KEEP_UNKNOWN |
| capability.energy_stability | UNKNOWN_NOT_MAPPED | 11 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION | MEDIUM | DECISION_IMCGS01_ENERGY_KEEP_UNKNOWN |
| capability.guard_power | SUPPORTED | 9 | 32 | ALREADY_SUPPORTED | HIGH | NONE_C01_READ_ONLY |
| capability.interrupt_timing | OUT_OF_SCOPE | 1 | 8 | REQUIRES_RUNTIME_SIGNAL | HIGH | KEEP_OUT_OF_C02B_STATIC_MAPPING_RUNTIME_CONTRACT_REQUIRED |
| capability.placement_shape | UNKNOWN_NOT_MAPPED | 7 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION | MEDIUM | DECISION_IMCGS01_PLACEMENT_KEEP_UNKNOWN |
| capability.spirit_lock | UNKNOWN_NOT_MAPPED | 2 | 16 | NO_STABLE_ITEM_SOURCE | HIGH | REQUEST_STABLE_ITEM_CONTRACT_SOURCE_OR_KEEP_UNKNOWN |
| capability.sustained_damage | UNKNOWN_NOT_MAPPED | 0 | 0 | NOT_REQUIRED_BY_E10 | HIGH | DEFER_E10_SCOPE_KEEP_C01_STATUS |
| capability.thunder_chain | UNKNOWN_NOT_MAPPED | 0 | 0 | NOT_REQUIRED_BY_E10 | HIGH | DEFER_E10_SCOPE_KEEP_C01_STATUS |

## E10 blocking priority

Priority is an audit triage order, not an implementation order and not authorization to start C02B.

| Priority | Capability | E10 refs | Blocked rows | Classification |
|---:|---|---:|---:|---|
| 1 | capability.energy_stability | 11 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION |
| 2 | capability.control_power | 10 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION |
| 3 | capability.cleanse_power | 8 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION |
| 4 | capability.placement_shape | 7 | 32 | MAPPABLE_REQUIRES_RULE_CONFIRMATION |
| 5 | capability.burst_window | 5 | 24 | MAPPABLE_REQUIRES_RULE_CONFIRMATION |
| 6 | capability.clear_power | 2 | 16 | MAPPABLE_REQUIRES_RULE_CONFIRMATION |
| 7 | capability.spirit_lock | 2 | 16 | NO_STABLE_ITEM_SOURCE |
| 8 | capability.interrupt_timing | 1 | 8 | REQUIRES_RUNTIME_SIGNAL |
| 9 | capability.debuff_counter | 1 | 8 | NO_STABLE_ITEM_SOURCE |

## Minimal decision boundary

Only the six `MAPPABLE_REQUIRES_RULE_CONFIRMATION` keys have decision rows. Every safe default is `KEEP_UNKNOWN`; recommendations are review routes only and are not mappings, values, ratios, thresholds, affixes, core effects, or Build rules.

## Protected state

| Scope | Before | After | Same |
|---|---|---|---|
| Item | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | PASS |
| Enemy | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | PASS |
| C01 | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | PASS |
| C02 | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | PASS |

## Verifier checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| classification.row-count | 16 | 16 | PASS |
| classification.unique-keys | 16 | 16 | PASS |
| classification.count.ALREADY_SUPPORTED | 3 | 3 | PASS |
| classification.count.MAPPABLE_REQUIRES_RULE_CONFIRMATION | 6 | 6 | PASS |
| classification.count.REQUIRES_RUNTIME_SIGNAL | 2 | 2 | PASS |
| classification.count.NO_STABLE_ITEM_SOURCE | 2 | 2 | PASS |
| classification.count.NOT_REQUIRED_BY_E10 | 3 | 3 | PASS |
| classification.count.DIRECT_MAPPABLE_EXISTING_FACT | 0 | 0 | PASS |
| c01.status.supported | 3 | 3 | PASS |
| c01.status.unknown | 11 | 11 | PASS |
| c01.status.out-of-scope | 2 | 2 | PASS |
| evidence.all-keys | 16 | 16 | PASS |
| decision.rule-row-count | 6 | 6 | PASS |
| decision.rule-key-coverage | 6 | 6 | PASS |
| decision.safe-default | KEEP_UNKNOWN | KEEP_UNKNOWN | PASS |
| runtime.not-recommended-to-c02b | 2 | 2 | PASS |
| field-map.capability.break_power | SUPPORTED | SUPPORTED | PASS |
| coverage.refs.capability.break_power | 2 | 2 | PASS |
| coverage.blocked.capability.break_power | 16 | 16 | PASS |
| field-map.capability.burst_window | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.burst_window | 5 | 5 | PASS |
| coverage.blocked.capability.burst_window | 24 | 24 | PASS |
| field-map.capability.caster_interrupt | OUT_OF_SCOPE | OUT_OF_SCOPE | PASS |
| coverage.refs.capability.caster_interrupt | 0 | 0 | PASS |
| coverage.blocked.capability.caster_interrupt | 0 | 0 | PASS |
| field-map.capability.chain_reaction | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.chain_reaction | 0 | 0 | PASS |
| coverage.blocked.capability.chain_reaction | 0 | 0 | PASS |
| field-map.capability.cleanse_power | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.cleanse_power | 8 | 8 | PASS |
| coverage.blocked.capability.cleanse_power | 32 | 32 | PASS |
| field-map.capability.clear_power | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.clear_power | 2 | 2 | PASS |
| coverage.blocked.capability.clear_power | 16 | 16 | PASS |
| field-map.capability.control_power | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.control_power | 10 | 10 | PASS |
| coverage.blocked.capability.control_power | 32 | 32 | PASS |
| field-map.capability.cooldown_recovery | SUPPORTED | SUPPORTED | PASS |
| coverage.refs.capability.cooldown_recovery | 0 | 0 | PASS |
| coverage.blocked.capability.cooldown_recovery | 0 | 0 | PASS |
| field-map.capability.debuff_counter | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.debuff_counter | 1 | 1 | PASS |
| coverage.blocked.capability.debuff_counter | 8 | 8 | PASS |
| field-map.capability.energy_stability | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.energy_stability | 11 | 11 | PASS |
| coverage.blocked.capability.energy_stability | 32 | 32 | PASS |
| field-map.capability.guard_power | SUPPORTED | SUPPORTED | PASS |
| coverage.refs.capability.guard_power | 9 | 9 | PASS |
| coverage.blocked.capability.guard_power | 32 | 32 | PASS |
| field-map.capability.interrupt_timing | OUT_OF_SCOPE | OUT_OF_SCOPE | PASS |
| coverage.refs.capability.interrupt_timing | 1 | 1 | PASS |
| coverage.blocked.capability.interrupt_timing | 8 | 8 | PASS |
| field-map.capability.placement_shape | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.placement_shape | 7 | 7 | PASS |
| coverage.blocked.capability.placement_shape | 32 | 32 | PASS |
| field-map.capability.spirit_lock | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.spirit_lock | 2 | 2 | PASS |
| coverage.blocked.capability.spirit_lock | 16 | 16 | PASS |
| field-map.capability.sustained_damage | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.sustained_damage | 0 | 0 | PASS |
| coverage.blocked.capability.sustained_damage | 0 | 0 | PASS |
| field-map.capability.thunder_chain | UNKNOWN_NOT_MAPPED | UNKNOWN_NOT_MAPPED | PASS |
| coverage.refs.capability.thunder_chain | 0 | 0 | PASS |
| coverage.blocked.capability.thunder_chain | 0 | 0 | PASS |
| decision.encounters.capability.energy_stability | 4 | 4 | PASS |
| decision.encounters.capability.control_power | 4 | 4 | PASS |
| decision.encounters.capability.cleanse_power | 4 | 4 | PASS |
| decision.encounters.capability.placement_shape | 4 | 4 | PASS |
| decision.encounters.capability.burst_window | 3 | 3 | PASS |
| decision.encounters.capability.clear_power | 2 | 2 | PASS |
| evidence.E01 | affix_break_up | FOUND | PASS |
| evidence.E02 | affix_cooldown_reduction | FOUND | PASS |
| evidence.E03 | affix_guard_up | FOUND | PASS |
| evidence.E04 | affix_first_trigger_bonus | FOUND | PASS |
| evidence.E05 | affix_cleanse_up | FOUND | PASS |
| evidence.E06 | affix_chain_target | FOUND | PASS |
| evidence.E07 | affix_control_up | FOUND | PASS |
| evidence.E08 | affix_nian_efficiency | FOUND | PASS |
| evidence.E09 | affix_trigger_refund | FOUND | PASS |
| evidence.E10 | affix_nian_efficiency | FOUND | PASS |
| evidence.E11 | public IReadOnlyList<Vector2Int> ShapeCells | FOUND | PASS |
| evidence.E12 | public bool isCountedInBuild | FOUND | PASS |
| evidence.E13 | affix_debuff_target_power | FOUND | PASS |
| evidence.E14 | public IReadOnlyList<ItemInstanceProjectionAffixSnapshot> Affixes | FOUND | PASS |
| evidence.E15 | public ItemBuildQualification buildQualification | FOUND | PASS |
| evidence.E16 | capability.caster_interrupt | FOUND | PASS |
| evidence.E17 | capability.interrupt_timing | FOUND | PASS |
| evidence.E18 | "capability.chain_reaction","UNKNOWN_NOT_MAPPED","0","0" | FOUND | PASS |
| evidence.E19 | "capability.sustained_damage","UNKNOWN_NOT_MAPPED","0","0" | FOUND | PASS |
| evidence.E20 | "capability.thunder_chain","UNKNOWN_NOT_MAPPED","0","0" | FOUND | PASS |
| evidence.E21 | affix_chain_target | FOUND | PASS |
| evidence.E22 | "castProgress" | FOUND | PASS |
| evidence.E23 | "castProgress" | FOUND | PASS |
| source.no-dedicated-debuff-counter-candidate | 0 | 0 | PASS |
| source.no-dedicated-spirit-lock-candidate | 0 | 0 | PASS |
| contract.no-dedicated-debuff-counter-member | 0 | 0 | PASS |
| contract.no-dedicated-spirit-lock-member | 0 | 0 | PASS |
| catalog.candidate-maturity | BALANCE_CANDIDATE | True | PASS |
| catalog.not-battle-connected | NOT_BATTLE_CONNECTED | True | PASS |
| leak.count | 0 | 0 | PASS |
| reports.deterministic.pre-protection | dd241be112efe0039ab995660cee28ca65efe24c2d2fa6c5470e28e3525017a6 | dd241be112efe0039ab995660cee28ca65efe24c2d2fa6c5470e28e3525017a6 | PASS |
| protected.item.file-count | 105 | 105 | PASS |
| protected.item.accepted-hash | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | PASS |
| protected.enemy.file-count | 89 | 89 | PASS |
| protected.enemy.accepted-hash | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | PASS |
| protected.c01.accepted-hash | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | PASS |
| protected.c02.accepted-hash | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | PASS |
| protected.item.before-after | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | PASS |
| protected.enemy.before-after | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | PASS |
| protected.c01.before-after | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | PASS |
| protected.c02.before-after | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4 | PASS |
| reports.complete | 5 | 5 | PASS |
| verifier.exists | true | True | PASS |
