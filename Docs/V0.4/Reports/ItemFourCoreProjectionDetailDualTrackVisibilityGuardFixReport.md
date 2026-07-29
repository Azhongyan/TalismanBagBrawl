# Item Four Core Projection Detail Dual Track Visibility GuardFix Report

- Assignment SHA-256: `c02c1a09e309af2a7888302e7c8b838f49637ee8881c113d8689e4c564b323ce`
- Phase A classification: `REACHABLE_BY_SCROLL_NO_LAYOUT_DEFECT`
- Phase B: `NOT_REQUIRED`
- View changes: `NONE`
- Scene/Prefab changes: `NONE`

## Runtime conclusion

The real I004 final model contains both non-empty Build sections. Both authored sections and rows are active after the same Bind. The QiLei authored rows are below the initial viewport and intersect the viewport at valid normalized position 0.75. At normalized bottom 0.0 they have already moved above the viewport because later authored sections follow them; this does not make them unreachable.

## Markers

- `PHASE_A_RUNTIME_EVIDENCE_PASS`: PASS - PASS
- `I004_DUAL_FINAL_VIEWMODEL_PASS`: PASS - PASS
- `I004_DUAL_AUTHORED_SECTIONS_ACTIVE_PASS`: PASS - PASS
- `I004_DUAL_SCROLL_REACHABILITY_PASS`: PASS - PASS
- `I001_QILEIONLY_REGRESSION_PASS`: PASS - PASS
- `I009_FAMENONLY_REGRESSION_PASS`: PASS - PASS
- `I006_NONE_KNOWNZERO_REGRESSION_PASS`: PASS - PASS
- `FOUR_CORE_BUILD_COEXISTENCE_PASS`: PASS - PASS
- `PROTECTED_HASHES_PASS`: PASS - PASS
- `LEAKCHECK_PASS`: PASS - PASS
- `PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS`: PASS - PASS
- `REACHABLE_BY_SCROLL_NO_LAYOUT_DEFECT`
- `USER_HANDTEST_WAITING`
- `PREFAB_MIGRATION_NOT_STARTED`
