# Dev Chapter Balance Run Leak Check Report

Package: `V0.4-DevChapterBalanceRun01`
Generated: `2026-07-04 01:43:08`
Status: `PASS`
Errors: `0`
Warnings: `0`

## Leak Counters

| Counter | Value | Expected |
| --- | ---: | ---: |
| Player-side answer leaks | 0 | 0 |
| Formal flow leaks | 0 | 0 |
| FeatureFlag default true | 0 | 0 |
| Formal chapter entries | 0 | 0 |
| Formal save writes | 0 | 0 |
| Formal reward writes | 0 | 0 |
| Chapter progress writes | 0 | 0 |
| V02/V03 scene touches | 0 | 0 |
| V04 RectTransform touches | 0 | 0 |

## Stage Leak Rows

| Stage | Player Leak | Formal Leak | Combat Preview Leaks | Shape Preview Leaks | Developer Fields |
| --- | --- | --- | ---: | ---: | ---: |
| `dev_balance_3_10_guard_wall` | `False` | `False` | 0 | 0 | 12 |
| `dev_balance_3_10_cleanse_corner` | `False` | `False` | 0 | 0 | 12 |
| `dev_balance_4_10_furnace_core` | `False` | `False` | 0 | 0 | 12 |
| `dev_balance_4_10_thunder_fire_cross` | `False` | `False` | 0 | 0 | 12 |

## Player Text Guard

- Player-facing balance outputs are checked for empty fields, Latin letters, and answer-like tokens.
- Developer ids remain in report/developer fields only and are masked from player UI.
- This package does not write scene UI or RectTransform data.
