# ItemDropGenerationSandbox01 Report

- Package: `V0.4-ItemDropGenerationSandbox01`
- Run time (UTC): `2026-07-16T08:55:14.2959557Z`
- Result: PASS
- Marker: `ITEM_DROP_GENERATION_SANDBOX01_PASS`
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

- Foundation: PASS
- StatRange: PASS
- CorePotential: PASS
- AffixSchema: PASS
- RollEngine: PASS
- ItemInnerDataCatalog: PASS
- ItemSystemValidatorAndSnapshot: PASS
- BuildSynergyCore: PASS
- CoreAwakeningPreview: PASS
- ItemSkillTriggerContract: PASS
- ItemDetailProjectionComplete: PASS
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

- None.
