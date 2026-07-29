# Item Enemy Matchup Simulation Report

- Package: `V0.4-ItemEnemyMatchupSimulation01`
- Guard receipt: `GUARD_PASS_ITEMENEMYMATCHUPSIMULATION01`
- Mode: `Unity batch compile/verifier`
- Isolation: `devOnly=true / isEnabled=false / entersFormalFlow=false`
- Scenario / Encounter / Matrix: `8 / 4 / 32`
- Capability coverage: `16/16`
- EVALUABLE_COMPLETE: `0`
- EVALUABLE_PARTIAL: `0`
- BLOCKED_BY_UNKNOWN: `32`
- NO_RELEVANT_SUPPORTED_CAPABILITY: `0`
- INVALID_INPUT: `0`
- C01 mapping version: `ItemBuildCapabilityProjectionAdapter.mapping.v1`
- Enemy snapshot signature: `sha256:5cbd0da2d18fde1e6355e2cd3df48f0fc9c1b5d231590ab4789a6d2d6eb1c069`
- Simulation canonical signature: `sha256:eb33c2ac195d69fa492c0f3a1f31ca6b39939631d622bf3c377f162e5f1ea36a`
- Protected C01 / Item / Enemy: `fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a / 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 / 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae`
- Checks: `39/39 PASS`

Each E10 pressure profile is evaluated independently through E08. No map-rule adjustment is inferred, no readiness band is promoted into a difficulty label, and decisive Unknown requirements suppress the matrix readinessBand field.

## Checks

| Check | Expected | Actual | Result |
|---|---|---|---|
| coverage.keyCount | 16 | 16 | PASS |
| coverage.uniqueKeys | 16 | 16 | PASS |
| determinism.matrixRows | all stable | 32 | PASS |
| determinism.signature | sha256:eb33c2ac195d69fa492c0f3a1f31ca6b39939631d622bf3c377f162e5f1ea36a | sha256:eb33c2ac195d69fa492c0f3a1f31ca6b39939631d622bf3c377f162e5f1ea36a | PASS |
| fixture.COOLDOWN_SUPPORTED.capability.cooldown_recovery | SUPPORTED | SUPPORTED | PASS |
| fixture.DEFENSE_SUPPORTED.capability.guard_power | SUPPORTED | SUPPORTED | PASS |
| fixture.MIXED_SUPPORTED.capability.break_power | SUPPORTED | SUPPORTED | PASS |
| fixture.MIXED_SUPPORTED.capability.cooldown_recovery | SUPPORTED | SUPPORTED | PASS |
| fixture.MIXED_SUPPORTED.capability.guard_power | SUPPORTED | SUPPORTED | PASS |
| fixture.OFFENSE_SUPPORTED.capability.break_power | SUPPORTED | SUPPORTED | PASS |
| fixture.SPARSE_UNKNOWN.capability.break_power | NOT_SUPPORTED | NOT_SUPPORTED | PASS |
| fixture.catalog | Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset | Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset | PASS |
| fixture.e10Isolation | true/false/false | True/False/False | PASS |
| fixture.encounterCount | 4 | 4 | PASS |
| fixture.rarityVariants | >=3 same base | 3/Green;Blue;Purple | PASS |
| fixture.sameBaseMultiInstance | 2/I003 | 2/I003 | PASS |
| fixture.scenarioCount | 8 | 8 | PASS |
| fixture.scenarioIds | COOLDOWN_SUPPORTED;DEFENSE_SUPPORTED;EMPTY;MIXED_SUPPORTED;OFFENSE_SUPPORTED;RARITY_VARIANTS;SAME_BASE_MULTI_INSTANCE;SPARSE_UNKNOWN | COOLDOWN_SUPPORTED;DEFENSE_SUPPORTED;EMPTY;MIXED_SUPPORTED;OFFENSE_SUPPORTED;RARITY_VARIANTS;SAME_BASE_MULTI_INSTANCE;SPARSE_UNKNOWN | PASS |
| invalidInputRows | 0 | 0 | PASS |
| isolation.result | true/false/false | True/False/False | PASS |
| leak.count | 0 | 0 | PASS |
| leak.formalFlow | 0 | 0 | PASS |
| matrix.crossProduct | 8x4 | 32 | PASS |
| matrix.rowCount | 32 | 32 | PASS |
| playerAnswerFields | 0 | 0 | PASS |
| protected.c01.fileCount | 9 | 9 | PASS |
| protected.c01.hash | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a | PASS |
| protected.enemy.fileCount | 89 | 89 | PASS |
| protected.enemy.hash | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae | PASS |
| protected.item.fileCount | 105 | 105 | PASS |
| protected.item.hash | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663 | PASS |
| reports.complete | 5 | 5 | PASS |
| reports.deterministic | 590f2ac60cc975738dcf85af37ff089bdb926e519371c14417bcd1f598734799 | 590f2ac60cc975738dcf85af37ff089bdb926e519371c14417bcd1f598734799 | PASS |
| trace.enemySnapshot | sha256:5cbd0da2d18fde1e6355e2cd3df48f0fc9c1b5d231590ab4789a6d2d6eb1c069 | sha256:5cbd0da2d18fde1e6355e2cd3df48f0fc9c1b5d231590ab4789a6d2d6eb1c069 | PASS |
| trace.itemSource | all C01 ibcr<64-hex> | 32 | PASS |
| trace.mappingVersion | ItemBuildCapabilityProjectionAdapter.mapping.v1 | ItemBuildCapabilityProjectionAdapter.mapping.v1 | PASS |
| unknown.bandSuppressed | all blocked/no-relevant rows blank | 32 | PASS |
| unknown.blockerKinds | 5 enums / required kinds present | ITEM_CAPABILITY_MAPPING_MISSING;ITEM_RUNTIME_FACT_MISSING;ENCOUNTER_REQUIREMENT_UNKNOWN;OUT_OF_SCOPE_RUNTIME_TIMING | PASS |
| unknown.notZero | Unknown counts retained | 376 | PASS |
