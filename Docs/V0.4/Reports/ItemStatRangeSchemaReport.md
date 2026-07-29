# ItemStatRangeSchema01 Report

- Package: `V0.4-ItemStatRangeSchema01`
- Run time (UTC): `2026-07-21T07:30:04.7982490Z`
- Overall: FAIL
- PASS marker: `NONE`
- Schema ID: `ItemStatRangeSchemaSnapshot.v1`
- Spec: 39 total / 39 PASS / 0 FAIL

## Contract

- Directions: `HigherIsBetter / LowerIsBetter / Neutral`.
- Numeric truth: signed integer `rawUnits`; display value is `rawUnits / 10^decimalPlaces`.
- Per-stat metadata: `unitKey / decimalPlaces / stepUnits / roundingMode / dataMaturity`.
- Range contract: one total envelope plus exactly `white / green / blue / purple / orange` subranges per configured `baseItemId + statId`.
- Rarity overlap: PASS; `blue 5..8` overlaps `purple 6..9`.
- Direction validation: Higher / Lower positive and negative cases PASS; Neutral non-monotonic case PASS.
- Canonical Signature: integer-only, invariant, stable after input reversal.
- Read-only attack: source mutation and exposed collection mutation checks PASS.

## QA Seed Isolation

- Seed profile: `I001 + qa_damage_example`, total `1..10`, maturity `SEED_DATA`.
- Markers: `QA_SEED_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA`.
- This is not I001 formal damage design and is not connected to any default generation path.

## Data Coverage

- Ordinary prototypes: 30
- Prototypes with QA Seed Profile: 1
- Prototypes without formal values: 30
- Formally usable value profiles: 0
- Coverage status: `UNRESOLVED_DATA_COVERAGE`
- I031 exclusion: PASS; ordinary stat profiles reject I031.

## Historical Regressions

- ItemRarityInstanceFoundation01: FAIL
- ItemInnerDataCatalog: PASS
- ItemSystemValidatorAndSnapshot: PASS
- BuildSynergyCore: PASS
- CoreAwakeningPreview: PASS
- ItemSkillTriggerContract: PASS
- ItemDetailProjectionComplete: FAIL
- JuNian Lighting: PASS
- ArrayBonus: PASS
- Foundation original Spec: 159/159 PASS

## Not Connected

- No property randomization, full item instance generation, affixes, core potential, Build eligibility, drop probability, cultivation, Battle, Reward, Inventory, SaveData, Boss, scene, prefab, or BuildSettings integration.
- No formal I002-I030 numeric profiles were created.

## Errors

- Historical regression report is not PASS: ItemRarityInstanceFoundation01.
- Historical regression report is not PASS: ItemDetailProjectionComplete.
