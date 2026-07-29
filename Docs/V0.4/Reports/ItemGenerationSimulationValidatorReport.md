# ItemGenerationSimulationValidator01 Report

- Package: `V0.4-ItemGenerationSimulationValidator01-GuardFix01`
- Result: FAIL
- Marker: `ITEM_GENERATION_SIMULATION_VALIDATOR01_GUARDFIX01_FAIL`
- Result schema: `ItemGenerationSimulationResult.v1`
- Suite schema: `ItemGenerationSimulationSuiteResult.v1`
- simulationVersion: `1`
- generationVersion: `1`
- fixtureStatus: `QA_FIXTURE_ONLY|NOT_BALANCE_APPROVED|NOT_FORMAL_GENERATION_DATA`
- Isolation markers: `QA_FIXTURE_ONLY / NOT_BALANCE_APPROVED / NOT_FORMAL_GENERATION_DATA`
- Notice: 仅用于验证算法分布，不代表正式游戏概率或数值。
- Threshold marker: `QA_DISTRIBUTION_TEST_THRESHOLD_ONLY`; zScore <= 6; positive expectedCount >= 100.
- Execution path: `ItemDropGenerationSandbox -> ItemInstanceRollEngine -> ItemGeneratedInstanceSnapshot`.
- Scope: Editor/QA-only in-memory simulation and report export.
- Not connected: Reward, RunFlow, Inventory, SaveData, Battle, Boss reward, formal drop UI, formal Catalog, pity or source tables.
- Scenarios: 10
- Accepted scenario samples: 107560
- Total real generation calls including replay/reversal/batches: 243969
- Spec: 65/65 PASS
- Expected failures: 11/11 PASS
- Suite expectedFailureRows: 11; external Clear attack: PASS
- Determinism: 12/12 PASS
- Candidate max zScore: 2.122783
- Rarity max zScore: 1.897931
- Stat/affix range checks: 233188; violations: 0
- Endpoint buckets: 62; covered: 62; missing: 0
- Mutex checks: 5000; violations: 0
- Repeat checks: 5512; violations: 0
- I031 ordinary leak count: 0
- Invariant violations: 0
- Canonical Signature: `B5337745FB009970`

## Scenario Results

| Scenario | Samples | Success | Failure | Signature |
| --- | ---: | ---: | ---: | --- |
| A_STAGE_1_10_WHITE_LOCK | 10000 | 10000 | 0 | `D9619DF9BFE1BC98` |
| B_CANDIDATE_EQUAL_30 | 30000 | 30000 | 0 | `56A0236F64311074` |
| B_CANDIDATE_WEIGHTED_QA | 30000 | 30000 | 0 | `4B67D83E3D1DC381` |
| C_STAGE_11_RARITY_AND_RICH_ROLL | 30000 | 30000 | 0 | `9878CC0C90A80AF2` |
| G_MUTEX_RULES | 5000 | 5000 | 0 | `CA39CE1D10AA01A7` |
| H_ALLOW_DUPLICATE | 512 | 512 | 0 | `AA263AE8845483A3` |
| M_BUILD_EXACT_BLUE | 512 | 512 | 0 | `1FE33490EC73AE6F` |
| M_BUILD_EXACT_GREEN | 512 | 512 | 0 | `9CD5C550341E65C9` |
| M_BUILD_EXACT_ORANGE | 512 | 512 | 0 | `8D23A29FAE2E01E7` |
| M_BUILD_EXACT_PURPLE | 512 | 512 | 0 | `E4B4A514E149EAC2` |

## Exact Build Profile Match

| Rarity | Required Profile | Required Qualification | Matched Samples | Mismatches |
| --- | --- | --- | ---: | ---: |
| green | `qa_exact_build_green` | FaMenOnly | 512 | 0 |
| blue | `qa_exact_build_blue` | QiLeiOnly | 512 | 0 |
| purple | `qa_exact_build_purple` | Dual | 512 | 0 |
| orange | `qa_exact_build_orange` | None | 512 | 0 |

- Swapped, mismatched, and missing corresponding profiles are independent rejecting specs.

## Endpoint Buckets

| valueKind | profileId | rarity | minObserved | maxObserved | stepViolationCount | sampleCount |
| --- | --- | --- | --- | --- | ---: | ---: |
| affix | `qa_fixed_guard_value` | blue | true | true | 0 | 6532 |
| affix | `qa_fixed_guard_value` | green | true | true | 0 | 4522 |
| affix | `qa_fixed_guard_value` | orange | true | true | 0 | 10606 |
| affix | `qa_fixed_guard_value` | purple | true | true | 0 | 8470 |
| affix | `qa_fixed_guard_value` | white | true | true | 0 | 6234 |
| affix | `qa_mutex_a_value` | green | true | true | 0 | 2567 |
| affix | `qa_mutex_b_value` | green | true | true | 0 | 2433 |
| affix | `qa_mutex_support_value` | green | true | true | 0 | 5000 |
| affix | `qa_random_a_value` | blue | true | true | 0 | 694 |
| affix | `qa_random_a_value` | green | true | true | 0 | 1477 |
| affix | `qa_random_a_value` | orange | true | true | 0 | 1057 |
| affix | `qa_random_a_value` | purple | true | true | 0 | 830 |
| affix | `qa_random_a_value` | white | true | true | 0 | 633 |
| affix | `qa_random_b_value` | blue | true | true | 0 | 1880 |
| affix | `qa_random_b_value` | green | true | true | 0 | 1318 |
| affix | `qa_random_b_value` | orange | true | true | 0 | 3180 |
| affix | `qa_random_b_value` | purple | true | true | 0 | 2555 |
| affix | `qa_random_b_value` | white | true | true | 0 | 1928 |
| affix | `qa_random_c_value` | blue | true | true | 0 | 3958 |
| affix | `qa_random_c_value` | green | true | true | 0 | 2751 |
| affix | `qa_random_c_value` | orange | true | true | 0 | 6369 |
| affix | `qa_random_c_value` | purple | true | true | 0 | 5085 |
| affix | `qa_random_c_value` | white | true | true | 0 | 3673 |
| stat | `I001@qa_power` | blue | true | true | 0 | 6532 |
| stat | `I001@qa_power` | green | true | true | 0 | 10034 |
| stat | `I001@qa_power` | orange | true | true | 0 | 10606 |
| stat | `I001@qa_power` | purple | true | true | 0 | 8470 |
| stat | `I001@qa_power` | white | true | true | 0 | 6234 |
| stat | `I001@qa_tempo` | blue | true | true | 0 | 6532 |
| stat | `I001@qa_tempo` | green | true | true | 0 | 10034 |
| stat | `I001@qa_tempo` | orange | true | true | 0 | 10606 |
| stat | `I001@qa_tempo` | purple | true | true | 0 | 8470 |
| stat | `I001@qa_tempo` | white | true | true | 0 | 6234 |
| stat | `I002@qa_power` | white | true | true | 0 | 10353 |
| stat | `I003@qa_power` | white | true | true | 0 | 19416 |
| stat | `I004@qa_power` | white | true | true | 0 | 1346 |
| stat | `I005@qa_power` | white | true | true | 0 | 1302 |
| stat | `I006@qa_power` | white | true | true | 0 | 1319 |
| stat | `I007@qa_power` | white | true | true | 0 | 1334 |
| stat | `I008@qa_power` | white | true | true | 0 | 1337 |
| stat | `I009@qa_power` | white | true | true | 0 | 1327 |
| stat | `I010@qa_power` | white | true | true | 0 | 1322 |
| stat | `I011@qa_power` | white | true | true | 0 | 1299 |
| stat | `I012@qa_power` | white | true | true | 0 | 1369 |
| stat | `I013@qa_power` | white | true | true | 0 | 1319 |
| stat | `I014@qa_power` | white | true | true | 0 | 1325 |
| stat | `I015@qa_power` | white | true | true | 0 | 1385 |
| stat | `I016@qa_power` | white | true | true | 0 | 1343 |
| stat | `I017@qa_power` | white | true | true | 0 | 1312 |
| stat | `I018@qa_power` | white | true | true | 0 | 1350 |
| stat | `I019@qa_power` | white | true | true | 0 | 1322 |
| stat | `I020@qa_power` | white | true | true | 0 | 1327 |
| stat | `I021@qa_power` | white | true | true | 0 | 1305 |
| stat | `I022@qa_power` | white | true | true | 0 | 1311 |
| stat | `I023@qa_power` | white | true | true | 0 | 1380 |
| stat | `I024@qa_power` | white | true | true | 0 | 1270 |
| stat | `I025@qa_power` | white | true | true | 0 | 1280 |
| stat | `I026@qa_power` | white | true | true | 0 | 1372 |
| stat | `I027@qa_power` | white | true | true | 0 | 1353 |
| stat | `I028@qa_power` | white | true | true | 0 | 1297 |
| stat | `I029@qa_power` | white | true | true | 0 | 1351 |
| stat | `I030@qa_power` | white | true | true | 0 | 1358 |

## Fixed Affix Same-Profile Same-Rarity Values

| affixId + affixValueProfileId + rarity | Distinct Count | Value@FirstRootSeed Proof |
| --- | ---: | --- |
| `qa_fixed_guard|qa_fixed_guard_value|blue` | 3 | `10@300008; 12@300005; 14@300018` |
| `qa_fixed_guard|qa_fixed_guard_value|green` | 3 | `6@300004; 8@300010; 10@300003` |
| `qa_fixed_guard|qa_fixed_guard_value|orange` | 3 | `18@300013; 20@300006; 22@300000` |
| `qa_fixed_guard|qa_fixed_guard_value|purple` | 3 | `14@300009; 16@300021; 18@300002` |
| `qa_fixed_guard|qa_fixed_guard_value|white` | 3 | `2@333; 4@73; 6@54` |

## Expected Failure Suite Rows

| Scenario | expectedCode | actualCode | snapshotIsNull | isPass |
| --- | --- | --- | --- | --- |
| drop-request-null | `REQUEST_NULL` | `REQUEST_NULL` | true | true |
| drop-generation-version-invalid | `GENERATION_VERSION_UNSUPPORTED` | `GENERATION_VERSION_UNSUPPORTED` | true | true |
| drop-fixture-status-invalid | `GENERATION_DATA_STATUS_INVALID` | `GENERATION_DATA_STATUS_INVALID` | true | true |
| stage11-rarity-policy-unresolved | `RARITY_POLICY_UNRESOLVED` | `RARITY_POLICY_UNRESOLVED` | true | true |
| i031-injection-explicit-failure | `I031_FORBIDDEN` | `I031_FORBIDDEN` | true | true |
| repeat-pool-capacity-insufficient | `INSTANCE_ROLL_FAILED` | `INSTANCE_ROLL_FAILED` | true | true |
| mutex-exhaustion-explicit-failure | `INSTANCE_ROLL_FAILED` | `INSTANCE_ROLL_FAILED` | true | true |
| zero-weight-entry-rejected-and-never-generated | `INSTANCE_ROLL_FAILED` | `INSTANCE_ROLL_FAILED` | true | true |
| higher-rarity-build-profile-missing | `BUILD_ROLL_PROFILE_MISSING` | `BUILD_ROLL_PROFILE_MISSING` | true | true |
| higher-rarity-build-profile-duplicate | `BUILD_ROLL_PROFILE_DUPLICATE` | `BUILD_ROLL_PROFILE_DUPLICATE` | true | true |
| default-formal-empty-cannot-pass | `CORE_PROFILE_MISSING` | `CORE_PROFILE_MISSING` | true | true |

## Historical Regressions

- Foundation: FAIL
- StatRange: FAIL
- CorePotential: FAIL
- AffixSchema: FAIL
- RollEngine: FAIL
- DropSandbox: FAIL
- ItemInnerDataCatalog: PASS
- ItemSystemValidatorAndSnapshot: PASS
- BuildSynergyCore: PASS
- CoreAwakeningPreview: PASS
- ItemSkillTriggerContract: PASS
- ItemDetailProjectionComplete: FAIL
- JuNian Lighting: PASS
- ArrayBonus: PASS
- Foundation: 159/159 PASS
- StatRange: 39/39 PASS
- CorePotential: 69/69 PASS
- AffixSchema: 88/88 PASS
- RollEngine: 72/72 PASS
- DropSandbox: 87/87 Spec, 6/6 Determinism PASS

## LeakCheck

- Categories: 24; total leaks: 0
- Scene / Prefab / BuildSettings modifications by this verifier: 0.

## Errors

- Historical regression report is not PASS: Foundation.
- Historical regression report is not PASS: StatRange.
- Historical regression report is not PASS: CorePotential.
- Historical regression report is not PASS: AffixSchema.
- Historical regression report is not PASS: RollEngine.
- Historical regression report is not PASS: DropSandbox.
- Historical regression report is not PASS: ItemDetailProjectionComplete.
