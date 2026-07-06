# BattleSandbox Playable Acceptance Report

Package: `V0.4-BattleSandboxPlayableAcceptance01`
Generated: `2026-07-05`
Status: `PASS`
Errors: `0`
Warnings: `0`

## Scope

- Final acceptance wrap-up for the current V0.4 BattleSandbox playable loop.
- Report-only package: no gameplay feature, UI rewrite, scene rearrangement, SaveData, Reward, Chapter, or formal RunFlow change was added.
- Acceptance evidence is consolidated from the current V0.4 QA reports and source-level guard checks:
  - `BattleSandboxPlayableFullRosterRegressionReport.md`
  - `BattleSandboxPlayableLoopReport.md`
  - `BattleSandboxRuntimeLoopReport.md`
  - `BuildSandboxPlayableRegressionReport.md`
  - `DevChapterBalanceRunReport.md`

## Acceptance Summary

| ID | Requirement | Result | Evidence |
| --- | --- | --- | --- |
| ACC-01 | 23 items are visible and placeable. | `PASS` | FullRoster: `roster=23; tray=23; packed=23; missing=0; packFailures=0`; board placement rows all pass. |
| ACC-02 | Basic items and V0.4 advanced items coexist. | `PASS` | FullRoster item rows include 13 basic `Single1` items and 10 V0.4 preview/advanced roster items. |
| ACC-03 | Multi-cell x2 / x3 / x4 / vertical_3 are stable. | `PASS` | FullRoster: `x2=3; x3=3; x4=2; vertical_3=1; failures=0`; regression rotation/placement guards also pass. |
| ACC-04 | Empty board does not damage enemy. | `PASS` | FullRoster empty-board preview: enemy `132->132`, item enemy HP rows `0`, player `100->0`, defeat rows `1`. |
| ACC-05 | Enemy can attack player. | `PASS` | RuntimeLoop/PlayableLoop: enemy attack rows present, timer advances, player HP rows present, devOnly profile attack damage rows present. |
| ACC-06 | Shield is consumed before HP. | `PASS` | FullRoster: `attackRows=3; shieldFirstRows=1`; RuntimeLoop shield-first validation passes. |
| ACC-07 | Attack items can damage enemy. | `PASS` | FullRoster: `attackCandidates=8; damagingSingleRows=3; totalDamage=53`; RuntimeLoop enemy HP rows present. |
| ACC-08 | Support / aura / rhythm items do not accidentally damage. | `PASS` | FullRoster per-item support validation: `supportCandidates=15; damageLeaks=0`. |
| ACC-09 | Victory / failure / restart are normal. | `PASS` | PlayableLoop: victory rows `2`, defeat rows `2`, restart retains build and resets HP/shield/mana/cooldown/log/cast. |
| ACC-10 | 3-10 / 4-10 can switch and show different feedback. | `PASS` | PlayableLoop: `3-10=2; 4-10=2`; DevChapterBalanceRun has distinct 3-10 and 4-10 outcomes/feedback. |
| ACC-11 | Player side does not leak full answers, DropBias weights, or Boss six-key answers. | `PASS` | FullRoster player leaks `0`; PlayableLoop leaks `0`; Regression leaks `0`; BalanceRun player answer leaks `0`. |
| ACC-12 | No SaveData write. | `PASS` | RuntimeLoop, CombatKernel, BuildCombat, FullRoster, and PlayableLoop formal leak counters stay `0`. |
| ACC-13 | No Reward grant. | `PASS` | Isolation checks keep reward grant flags false and formal leak counters `0`. |
| ACC-14 | No Chapter progress. | `PASS` | Isolation checks keep chapter advance flags false and formal leak counters `0`. |
| ACC-15 | No formal RunFlow connection. | `PASS` | PlayableLoop reports formal RunFlow disconnected; RuntimeLoop formal flow leak count `0`. |
| ACC-16 | V0.2/V0.3 are not modified. | `PASS` | PlayableLoop V0.2/V0.3 touch check false; CombatKernel is adapter/read-only mouthfeel reuse only. |
| ACC-17 | V04 UI is not rearranged. | `PASS` | UI layout guard passes; RuntimeLoop UI layout write count `0`; report package made no scene/layout edits. |

## Evidence Details

| Evidence Source | Status | Key Counters |
| --- | --- | --- |
| `BattleSandboxPlayableFullRosterRegressionReport.md` | `PASS` | 23 roster items, 13 basic items, x2/x3/x4/vertical_3 placement, empty board defeat, attack damage, shield-first damage, support damage leaks `0`. |
| `BattleSandboxPlayableLoopReport.md` | `PASS` | Victory/defeat prompts, restart reset, 3-10/4-10 switching, answer masking, formal isolation. |
| `BattleSandboxRuntimeLoopReport.md` | `PASS` | Runtime rows, mana/cooldown/item trigger/enemy HP/player HP/player shield rows, no settlement/formal leak/UI layout write. |
| `BuildSandboxPlayableRegressionReport.md` | `PASS` | Scene readiness, tray coverage, placement/rotation guard, board read, Boss feedback, devOnly 3-10/4-10 rows, leak counters `0`. |
| `DevChapterBalanceRunReport.md` | `PASS` | Two 3-10 rows and two 4-10 rows with different difficulty feedback; player answer leaks `0`; formal leaks `0`. |

## Manual Acceptance Notes

- No necessary minimal gameplay fix was identified in this acceptance pass.
- Unity batch was not re-run in this window; this report consolidates the latest workspace QA report outputs plus static source/report review.
- User handtest is still recommended for the visual tray paging, drag placement feel, empty-board defeat, one attack-item run, one support-item run, restart, and 3-10/4-10 target switching.
