# ItemDetailQualifiedBuildTrackProjection01 Report

Status: PASS

- Package: `V0.4-ItemDetailQualifiedBuildTrackProjection01`
- Assignment SHA-256: `dff7c52e6350017afbf25d58013ee6acc82e894023ec397362251f47537bc91e`
- Guard receipt: `ITEM_GUARD_PASS_ITEMDETAILQUALIFIEDBUILDTRACKPROJECTION01`
- `COMPONENT_FIXTURE_PASS`
- `REAL_RUNTIME_PATH_PASS`
- `USER_HANDTEST_WAITING`
- Downstream packages: `NOT_STARTED`

## Real sample matrix

| Sample | Inventory | Board unlit | Board lit | Move/refresh | Return |
|---|---|---|---|---|---|
| I001 | PASS: QiLeiOnly / 未入阵 | PASS: isLit=False; source=False; qualified=False | PASS: isLit=True; source=True; qualified=True | PASS: latest Package A canonical consumed | PASS: Inventory / placementId Missing |
| I004 | PASS: Dual / 未入阵 | PASS: isLit=False; source=False; qualified=False | PASS: isLit=True; source=True; qualified=True | PASS: latest Package A canonical consumed | PASS: Inventory / placementId Missing |
| I006 | PASS: None / 未入阵 | PASS: isLit=False; source=False; qualified=False | PASS: isLit=True; source=True; qualified=False | PASS: latest Package A canonical consumed | PASS: Inventory / placementId Missing |
| I009 | PASS: FaMenOnly / 未入阵 | PASS: isLit=False; source=False; qualified=False | PASS: isLit=True; source=True; qualified=True | PASS: latest Package A canonical consumed | PASS: Inventory / placementId Missing |
| I031 | PASS: system detail / NotApplicable | PASS: ordinary Build row absent | PASS: system placement / ordinary Build row absent | NOT_APPLICABLE | PASS: system detail preserved |

## Verification scenarios

| Scenario | Result | Marker | Evidence |
|---|---|---|---|
| S01 | PASS | `COMPONENT_FIXTURE_PASS` | verified |
| S02 | PASS | `I001_REAL_SAMPLE_PASS` | verified |
| S03 | PASS | `I004_REAL_SAMPLE_PASS` | verified |
| S04 | PASS | `I006_REAL_SAMPLE_PASS` | verified |
| S05 | PASS | `I009_REAL_SAMPLE_PASS` | verified |
| S06 | PASS | `I031_REAL_SAMPLE_PASS` | verified |
| S07 | PASS | `UNKNOWN_KNOWN_ZERO_SEPARATION_PASS` | verified |
| S08 | PASS | `STRICT_CONTROLLER_PATH_PASS` | verified |
| S09 | PASS | `PACKAGE_A_DIRECT_STAGE_LINEAGE_PASS` | verified |
| S10 | PASS | `PROTECTED_HASHES_PASS` | verified |
| S11 | PASS | `LEAK_CHECK_PASS` | verified |
| S12 | PASS | `PACKAGE_SCOPED_DIFF_CHECK_PASS` | verified |

Unknown numeric fields remain Missing/null. Known Zero remains present `0`; their ViewModel text and canonical signatures differ.

The fixed 2/4/6 and 2/4 display rows copy `qualifiedItemCount`, `maxPieceCount`, `activeStagePieceCount`, and `nextStagePieceCount` from Package A; no UI-side Build evaluator is present.

Final gate: `DEV_COMPLETE / QA_STATIC_PASS / REAL_RUNTIME_PATH_PASS / WAITING_USER_HANDTEST` when every verifier scenario passes.
