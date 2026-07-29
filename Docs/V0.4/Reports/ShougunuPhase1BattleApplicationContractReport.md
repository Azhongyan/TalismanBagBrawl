# Shougunu Phase1 Battle Application Contract Report

- classification = `COMPLEX_GUARDED_ONCE`
- package = `V0.4-ShougunuPhase1BattleApplicationContract01`
- package status = `PACKAGE_COMPLETE`
- terminal item schema consumed = `ItemCombatEffectRequestSnapshot.v1`
- terminal enemy schema consumed = `ShougunuPhase1RuntimeAndActionContract.v1`
- sceneTouched = `false`
- visualTouched = `false`
- formalRouteTouched = `false`
- saveTouched = `false`
- rewardTouched = `false`
- autoCombatTouched = `false`
- user handtest = `NOT_APPLICABLE`
- P3 = `NOT_STARTED`

## Terminal nominal facts

- final tick = `75000`
- accepted Item applications = `50`
- shell breaks = `7`
- threshold skills resolved = `6`
- BasicAttack resolved = `12`
- player HP = `9735/9999`
- enemy lifecycle = `Defeated`

## Automated QA

| ID | Marker | Status | Detail |
|---|---|---|---|
| S01 | ASSIGNMENT_AND_UPSTREAM_HASHES_PASS | PASS | verified |
| S02 | TERMINAL_SCHEMAS_AND_CONSTANTS_PASS | PASS | verified |
| S03 | REAL_ITEM_SNAPSHOT_CONSUMED_PASS | PASS | verified |
| S04 | IMMUTABLE_BATTLE_CONTRACTS_PASS | PASS | verified |
| S05 | NOMINAL_75S_50_APPLICATIONS_PASS | PASS | verified |
| S06 | SIX_SKILLS_TWELVE_BASICS_SEVEN_BREAKS_PASS | PASS | verified |
| S07 | PLAYER_9999_DAMAGE_AND_SURVIVAL_PASS | PASS | verified |
| S08 | DISPLAY_EVENT_MODEL_PASS | PASS | verified |
| S09 | NEGATIVE_AND_RESET_FIXTURES_PASS | PASS | verified |
| S10 | STATIC_FORBIDDEN_REFERENCE_SCAN_PASS | PASS | verified |
| S11 | REPORTS_REPRODUCIBLE_PASS | PASS | verified |
| S12 | PACKAGE_SCOPED_TEXT_CHECK_PASS | PASS | verified |
