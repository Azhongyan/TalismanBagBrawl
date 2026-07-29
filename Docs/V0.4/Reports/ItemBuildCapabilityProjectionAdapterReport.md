# Item Build Capability Projection Adapter Report

- Package: `V0.4-ItemBuildCapabilityProjectionAdapter01`
- Guard receipt: `GUARD_PASS_ITEMBUILDCAPABILITYPROJECTIONADAPTER01`
- Source contract: `ItemInstanceProjectionContractSnapshot.v1`
- Target contract: `BuildCapabilityReadContract.v1`
- Mapping version: `ItemBuildCapabilityProjectionAdapter.mapping.v1`
- Mode: `Unity batch`
- Result: `PASS`
- Known capability keys: `16`
- Representative mapping counts (SUPPORTED / KNOWN_ZERO / UNKNOWN_NOT_MAPPED / OUT_OF_SCOPE): `3/0/11/2`
- Empty complete-source counts: `0/3/11/2`
- Fixture rows: `16`
- Determinism signature: `sha256:77a877fe03b3c495406f3fb9b0da1b412c40e379b8f55ccdf293af6c1cd7c6e2`
- Input immutable: `PASS`
- Item protected hash: `7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663`
- Enemy protected hash: `7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae`
- Formal-system leak count: `0`
- Unity validation status: `PASS`

## Checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| resolver.authoritative-key-count | 16 | 16 | PASS |
| resolver.unique-ordinal-keys | 16 | 16 | PASS |
| field-map.every-resolver-key | 16 | 16 | PASS |
| source-contract.schema | ItemInstanceProjectionContractSnapshot.v1 | ItemInstanceProjectionContractSnapshot.v1 | PASS |
| target-contract.schema | BuildCapabilityReadContract.v1 | BuildCapabilityReadContract.v1 | PASS |
| mapping.version | ItemBuildCapabilityProjectionAdapter.mapping.v1 | ItemBuildCapabilityProjectionAdapter.mapping.v1 | PASS |
| snapshot.coverage | Sparse | Sparse | PASS |
| snapshot.protected-flags | true/false/false | True/False/False | PASS |
| mapping.supported-counts | 3/0/11/2 | 3/0/11/2 | PASS |
| mapping.empty-counts | 0/3/11/2 | 0/3/11/2 | PASS |
| value.capability.break_power | 1000 | 1000 | PASS |
| value.capability.guard_power | 800 | 800 | PASS |
| value.capability.cooldown_recovery | 900 | 900 | PASS |
| value.capability.break_power | 10000 | 10000 | PASS |
| known-zero.explicit | 3 emitted zero values | 3 values | PASS |
| unknown.missing-schema | Sparse omission | 0 values | PASS |
| unknown.missing-projection | Sparse omission | 0 values | PASS |
| unknown.unit-mismatch | break omitted | break omitted | PASS |
| unknown.negative-not-zero | break omitted | break omitted | PASS |
| unknown.stable-source-diagnostics | all categories present | all categories present | PASS |
| values.range | 0..10000 | 0..10000 | PASS |
| determinism.canonical-signature | sha256:77a877fe03b3c495406f3fb9b0da1b412c40e379b8f55ccdf293af6c1cd7c6e2 | sha256:77a877fe03b3c495406f3fb9b0da1b412c40e379b8f55ccdf293af6c1cd7c6e2 | PASS |
| determinism.source-revision | ibcr04d9784f35bb0c2e8eefd6e8be5a189d7b3506fd3a08dae3cf468e5a1b610f6a | ibcr04d9784f35bb0c2e8eefd6e8be5a189d7b3506fd3a08dae3cf468e5a1b610f6a | PASS |
| determinism.unknown-diagnostics | BUILD_QUALIFICATION_RULE_UNCONFIRMED\|\|Dual\|Build qualification is categorical and is not converted to capability BP. BUILD_QUALIFICATION_RULE_UNCONFIRMED\|\|FaMenOnly\|Build qualification is categorical and is not converted to capability BP. CAPABILITY_REQUIRES_RUNTIME_TIMING_FACT\|capability.caster_interrupt\|\|Runtime timing or caster-state evidence is outside this read-only adapter. CAPABILITY_REQUIRES_RUNTIME_TIMING_FACT\|capability.interrupt_timing\|\|Runtime timing or caster-state evidence is outside this read-only adapter. CAPABILITY_RULE_NOT_CONFIRMED\|capability.burst_window\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.chain_reaction\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.cleanse_power\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.clear_power\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.control_power\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.debuff_counter\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.energy_stability\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.placement_shape\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.spirit_lock\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.sustained_damage\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.thunder_chain\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. ITEM_SYSTEM_BUILD_FACTS_MISSING\|\|\|Shape, lighting, Build tag, and active-core facts are absent and remain Unknown. UNMAPPED_ELIGIBLE_CORE_EFFECT_KEY\|\|core_stable_eligible\|Eligibility does not prove an active capability contribution. UNMAPPED_STABLE_AFFIX_KEY\|\|affix_unmapped_stable\|Stable affix key has no confirmed target capability rule. UNMAPPED_STABLE_STAT_KEY\|\|damage\|Stat rawUnits has no confirmed basis-point capability conversion. UNMAPPED_VISIBLE_CORE_EFFECT_KEY\|\|core_stable_eligible\|Visible core effect has no confirmed capability conversion. | BUILD_QUALIFICATION_RULE_UNCONFIRMED\|\|Dual\|Build qualification is categorical and is not converted to capability BP. BUILD_QUALIFICATION_RULE_UNCONFIRMED\|\|FaMenOnly\|Build qualification is categorical and is not converted to capability BP. CAPABILITY_REQUIRES_RUNTIME_TIMING_FACT\|capability.caster_interrupt\|\|Runtime timing or caster-state evidence is outside this read-only adapter. CAPABILITY_REQUIRES_RUNTIME_TIMING_FACT\|capability.interrupt_timing\|\|Runtime timing or caster-state evidence is outside this read-only adapter. CAPABILITY_RULE_NOT_CONFIRMED\|capability.burst_window\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.chain_reaction\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.cleanse_power\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.clear_power\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.control_power\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.debuff_counter\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.energy_stability\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.placement_shape\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.spirit_lock\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.sustained_damage\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. CAPABILITY_RULE_NOT_CONFIRMED\|capability.thunder_chain\|\|No stable, unit-safe Item-to-capability rule is confirmed; value is omitted. ITEM_SYSTEM_BUILD_FACTS_MISSING\|\|\|Shape, lighting, Build tag, and active-core facts are absent and remain Unknown. UNMAPPED_ELIGIBLE_CORE_EFFECT_KEY\|\|core_stable_eligible\|Eligibility does not prove an active capability contribution. UNMAPPED_STABLE_AFFIX_KEY\|\|affix_unmapped_stable\|Stable affix key has no confirmed target capability rule. UNMAPPED_STABLE_STAT_KEY\|\|damage\|Stat rawUnits has no confirmed basis-point capability conversion. UNMAPPED_VISIBLE_CORE_EFFECT_KEY\|\|core_stable_eligible\|Visible core effect has no confirmed capability conversion. | PASS |
| input.immutable | 35e64c6b136677d7c0cedcd930e0f7aa5ff1b54692a506650723e905e5d78251 | 35e64c6b136677d7c0cedcd930e0f7aa5ff1b54692a506650723e905e5d78251 | PASS |
| fixture.minimum-count | >=11 | 16 | PASS |
| fixture.rarity-coverage | 5 | 5 | PASS |
| protected.item.file-count | 105 | 105 | PASS |
| protected.item.hash | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | PASS |
| protected.enemy.file-count | 89 | 89 | PASS |
| protected.enemy.hash | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | PASS |
| leak.count | 0 | 0 | PASS |
| package.expected-files | all present | all present | PASS |
