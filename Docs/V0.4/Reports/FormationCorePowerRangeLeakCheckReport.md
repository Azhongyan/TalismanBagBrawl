# Formation Core And Power Range Leak Check Report

Package: `V0.4-FormationCoreAndPowerRange01`
Guard Pass: `GUARD_PASS_FORMATION_CORE_POWER_RANGE01`
Generated: `2026-07-06 static Codex snapshot`
Status: `PASS`
Errors: `0`
Warnings: `0`

## Leak Counters

| Check | Count | Expected |
| --- | ---: | ---: |
| `formationPowerScopeLeakCount` | 0 | 0 |
| `formationFeedbackRows` | 1 | >=1 |
| `featureFlagDefaultTrue` | 0 | 0 |
| `formalFlowLeakCount` | 0 | 0 |
| `playerSideAnswerLeakCount` | 0 | 0 |
| `eyeCellMissing` | 0 | 0 |
| `eyeCellOccupied` | 0 | 0 |
| `weakPulseCells` | 4 | >=1 |
| `poweredProviderCount` | 1 | >=1 |
| `poweredCells` | 5 | >=1 |
| `forbiddenProviderMisidentified` | 0 | 0 |
| `tagAutoPowerProvider` | 0 | 0 |
| `oldIsPoweredMainJudgmentMismatch` | 0 | 0 |
| `rectTransformMovesAuthored` | 0 | 0 |
| `sceneYamlWrites` | 0 | 0 |
| `saveDataWrites` | 0 | 0 |
| `rewardWrites` | 0 | 0 |
| `chapterWrites` | 0 | 0 |
| `totalLeaks` | 0 | 0 |

## Boundary Confirmation

- Runtime visual feedback is driven from `FormationEnergyContractResolver.Apply(snapshot)`.
- Eye cell `(2,2)` is visualized as layout state and WeakPulse source only.
- Powered range is shown only for `spirit_stone_basic` / spirit-or-energy-stone provider family rows.
- Energy incense, furnace/core, peach-wood, seal, and provider tags have zero provider auto-promotion.
- `isPowered` remains report-visible only as `energyState == Powered` compatibility data; it is not the visual/provider source of truth.
- The overlay objects are runtime `DontSave` objects and do not write scene YAML or RectTransform/Layout/Image/Text authored fields.
- No formal `RunFlow`, `PageState`, `FormationState`, `SaveData`, `PlayerPrefs`, `Reward`, `Drop`, `Boss`, `Chapter`, or BuildSettings paths were touched.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| UI Layout Guard | `PASS` | 0 | 0 | 29 |
| Formation Core And Power Range 01 | `PASS` | 0 | 0 | 1 |
