# B-Line Chapter 1 Continuous Battle Integration — One Final Handtest

This is the only continuous user retest for the repaired package. The previous run remains recorded as `USER_HANDTEST_FAILED` because step 7 could not enter the Boss battle surface. The same-package repair has now reached `AUTOMATED_QA_PASS / USER_RETEST_READY`; automation has not performed or claimed the user retest.

Temporary-clear labels that must remain visible:

- `PHASE1_DEV_VERTICAL_SLICE`
- `NOT_CONTENT_FINAL`
- `NOT_FORMAL_BOSS_COMPLETION`

## One continuous route

1. In Unity, choose `Tools/TalismanBag/V0.4/B-Line Chapter 1/Enable And Open Continuous Handtest`.
2. Enter Play Mode manually. Confirm one dev-only/default-off overlay appears in the exact BattleSandbox scene and the existing Item board/tray remains unchanged.
3. Click `Start Chapter 1 Session`. Confirm the overlay shows Chapter 1, stage `1-1`, phase `Prepare`.
4. For normal stages, prepare the real Item board so at least one positive real direct-damage request is valid. Click `Start Current Normal Stage`, wait for the real Item request cadence to defeat the shown new V0.4 Enemy, then use `Continue From Settlement / Drop Candidate` once to observe the no-grant drop boundary and once more to advance. Repeat continuously through `1-1` to `1-9`.
5. After clearing `1-9`, confirm the exact stop: stage `1-10`, phase `BossGate`, and no automatic Boss challenge. While still at `BossGate`, keep arranging the real board until there is exactly one valid positive real request for every `I007`, `I008`, `I009`, `I010`, `I011`, and `I012`, and `I031` is both owned and placed. If anything is missing, click `Manual Challenge Shougunu Phase1` once and confirm the overlay lists the missing prerequisites, remains at `BossGate`, consumes no Boss request, leaves the board editable, and permits retry.
6. When all Boss prerequisites are ready, click `Manual Challenge Shougunu Phase1` exactly once. Confirm the grid leaves preparation and visibly switch into the accepted authored Boss surface. The handoff must use the existing wired authored Button; it must not remain on the preparation/hidden surface or silently wait. Confirm the accepted existing real-item Shougunu Phase1 vertical-slice Runtime obtains an active session only now.
7. Defeat Shougunu Phase1 through that accepted vertical slice. Continue from its settlement.
8. Confirm the overlay shows all three exact labels: `PHASE1_DEV_VERTICAL_SLICE`, `NOT_CONTENT_FINAL`, `NOT_FORMAL_BOSS_COMPLETION`.
9. Confirm Chapter 1 is complete and Chapter 2 is shown as unlocked for this session only. Do not select or run Chapter 2.
10. exit/re-enter Play Mode and confirm there is no persistent Chapter 2 unlock, reward, drop, inventory change, Save write, achievement, or claim token. Use `Tools/TalismanBag/V0.4/B-Line Chapter 1/Disable Continuous Handtest` when finished.

Report PASS or the exact failing step/diagnostic to the package owner. Only explicit user PASS may promote this package beyond `AUTOMATED_QA_PASS / USER_RETEST_READY`; automation must not claim `PACKAGE_COMPLETE`.
