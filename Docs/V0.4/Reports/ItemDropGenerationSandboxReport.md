# ItemDropGenerationSandbox01 Report

- Package: `V0.4-ItemDropGenerationSandbox01`
- Run time (UTC): `2026-07-21T07:30:04.9977097Z`
- Result: FAIL
- Marker: `ITEM_DROP_GENERATION_SANDBOX01_FAIL`
- Algorithm ID: `item-drop-generation-v1`
- generationVersion: `1`
- Stage 1-10 rarity policy: `LOCKED_STAGE_1_10_WHITE`; no rarity stream is created.
- Ordinary candidate pool: exactly `I001-I030`; QA relative weights only.
- I031 exclusion: explicit failure with `I031_FORBIDDEN`.
- Stage 11+ rarity policy: verifier-memory `QA_WEIGHT_PROFILE`; absent profile is unresolved.
- Candidate domain: `[drop, candidate, candidatePoolId]`.
- Rarity domain: `[drop, rarity, rarityWeightProfileId]`.
- Domain encoding: length-prefixed UTF-8 fields plus segment count; bounded selection uses rejection sampling.
- Generated instance schema: `ItemGeneratedInstanceSnapshot.v1`
- Instance generation: calls ItemInstanceRollEngine; any engine failure returns no drop snapshot.
- QA data status: `QA_FIXTURE_ONLY|NOT_BALANCE_APPROVED|NOT_FORMAL_GENERATION_DATA`
- Spec: 87/87 PASS
- Determinism: 6/6 PASS
- Samples: 40
- Same-input repeat: 1000/1000 PASS
- Golden candidate domain: `6DEE1D6E50111E1B`.
- Golden rarity domain: `7882C5BB16185E78`.
- Segmented collision vector: `57B30C3DC752992A / 12F4959138EDF7BC`.
- Default formal generation data: empty and unable to generate.
- Formal system connection: none; this package remains an isolated QA sandbox.

## Historical Regressions

- Foundation: FAIL
- StatRange: FAIL
- CorePotential: FAIL
- AffixSchema: FAIL
- RollEngine: FAIL
- ItemInnerDataCatalog: PASS
- ItemSystemValidatorAndSnapshot: PASS
- BuildSynergyCore: PASS
- CoreAwakeningPreview: PASS
- ItemSkillTriggerContract: PASS
- ItemDetailProjectionComplete: FAIL
- JuNian Lighting: PASS
- ArrayBonus: PASS
- Foundation: 159/159
- StatRange: 39/39
- CorePotential: 69/69
- AffixSchema: 88/88
- RollEngine: 72/72

## LeakCheck

- Categories: 31; total leaks: 0

## Errors

- Historical regression report is not PASS: Foundation.
- Historical regression report is not PASS: StatRange.
- Historical regression report is not PASS: CorePotential.
- Historical regression report is not PASS: AffixSchema.
- Historical regression report is not PASS: RollEngine.
- Historical regression report is not PASS: ItemDetailProjectionComplete.
