# ItemInstanceRollEngine01 Report

- Package: `V0.4-ItemInstanceRollEngine01`
- Result: PASS
- Marker: `ITEM_INSTANCE_ROLL_ENGINE01_PASS`
- Algorithm ID: `item-instance-roll-v1`
- generationVersion: `1`
- Domain hash: `FNV-1a 64-bit over explicit length-prefixed UTF-8 fields and segment count`
- PRNG: `SplitMix64`
- Bounded integer: `rejection sampling before modulo`
- Seed fields: `algorithmId / rootSeed / generationVersion / itemInstanceId / baseItemId / rarity / domainSegmentCount / domainSegments`
- QA data status: `QA_FIXTURE_ONLY|NOT_BALANCE_APPROVED|NOT_FORMAL_GENERATION_DATA`
- Spec: 72/72 PASS
- Determinism: 5/5 PASS
- Same-input repeat: 1000/1000 PASS
- Sample stats: 2
- Sample fixed affixes: 1
- Sample random affixes: 2
- Sample Build qualification: `QiLeiOnly`
- Core potential: read-only eligible/visible projection; no unlock or activation state.
- Mutex/repeat: legal candidates are filtered once, then one weighted selection is performed.
- Formal integration: none; no formal generation Catalog, acquisition, persistence, cultivation, combat, detail UI, or Build counting connection.

## Golden Vectors

- FNV empty: `CBF29CE484222325`; FNV a: `AF63DC4C8601EC8C`; FNV hello: `A430D84680AABD0B`.
- SplitMix64 stream from state 0: `E220A8397B1DCDAF / 6E789E6AA1B965F4 / 06C45D188009454F`.
- SplitMix64 wraparound from `ulong.MaxValue`: `E4D971771B652C20`.
- Segmented domain vector `[stat, qa_power]`: `54AE2882BEECC218`.

## Historical Regressions

- Foundation: PASS
- StatRange: PASS
- CorePotential: PASS
- AffixSchema: PASS
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

## LeakCheck

- Categories: 31; total leaks: 0

## Errors

- None.
