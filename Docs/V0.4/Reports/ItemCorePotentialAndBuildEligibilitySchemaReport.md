# ItemCorePotentialAndBuildEligibilitySchema01 Report

- Package: `V0.4-ItemCorePotentialAndBuildEligibilitySchema01`
- Run time (UTC): `2026-07-16T08:55:14.1157343Z`
- Overall: PASS
- PASS marker: `ITEM_CORE_POTENTIAL_AND_BUILD_ELIGIBILITY_SCHEMA01_PASS`
- Schema ID: `ItemCorePotentialAndBuildEligibilitySchemaSnapshot.v1`
- Spec: 69 total / 69 PASS / 0 FAIL

## Default Runtime Contract

- Core rarity policies: exactly `white / green / blue / purple / orange`.
- Core resolution: all five policies remain `Unresolved`.
- Ultimate eligibility: only `orange` allows Ultimate potential.
- Formal core potential profiles: `0`.
- Build policy: `white=LockedNone`; `green/blue/purple/orange=ProbabilityUnresolved`.
- White instance qualification: `None` only.
- Formal probability profiles: `0`; every `probabilityProfileId` is empty.
- Data status: `SCHEMA_ONLY / UNRESOLVED_DATA_COVERAGE`.

## QA Fixture Isolation

- Fixture IDs: `qa_core_standard_01 / qa_core_ultimate_01 / qa_potential_profile_01`.
- Markers: `QA_FIXTURE_ONLY / NOT_DESIGN_APPROVED / NOT_FORMAL_GENERATION_DATA`.
- Fixture profiles used by verifier only: 2; Runtime default Catalog remains 0.

## Isolation and Compatibility

- The existing Build resolver remains the old runtime contract; new qualification is instance-generation Schema only.
- The existing Awakening preview remains an old preview contract; no new core count or open order is inferred from it.
- No Awakening, Build count, lighting, combat, drop, property roll, affix, Reward, Inventory, SaveData, scene, prefab, or BuildSettings connection was added.
- `ItemSystemSnapshot.v1`, Item Detail, and all legacy resolvers remain untouched.

## Determinism and Read-only

- Canonical Signature: `ItemCorePotentialAndBuildEligibilitySchemaSnapshot.v1 || C|white|0|Unresolved|0|SCHEMA_ONLY || C|green|1|Unresolved|0|SCHEMA_ONLY || C|blue|2|Unresolved|0|SCHEMA_ONLY || C|purple|3|Unresolved|0|SCHEMA_ONLY || C|orange|4|Unresolved|1|SCHEMA_ONLY || B|white|0|LockedNone||SCHEMA_ONLY || B|green|1|ProbabilityUnresolved||SCHEMA_ONLY || B|blue|2|ProbabilityUnresolved||SCHEMA_ONLY || B|purple|3|ProbabilityUnresolved||SCHEMA_ONLY || B|orange|4|ProbabilityUnresolved||SCHEMA_ONLY`
- Input reversal stability: PASS.
- Source collection mutation and exposed collection mutation attacks: PASS.
- Forbidden runtime fields and qualification probability state: absent by reflection and LeakCheck.

## Historical Regressions

- ItemRarityInstanceFoundation: PASS
- ItemStatRangeSchema: PASS
- ItemInnerDataCatalog: PASS
- ItemSystemValidatorAndSnapshot: PASS
- BuildSynergyCore: PASS
- CoreAwakeningPreview: PASS
- ItemSkillTriggerContract: PASS
- ItemDetailProjectionComplete: PASS
- JuNian Lighting: PASS
- ArrayBonus: PASS
- Foundation original Spec: 159/159 PASS
- StatRange original Spec: 39/39 PASS

## LeakCheck

- Result: PASS
- Categories: 25; total leaks: 0

## Errors

- None
