# UnifiedBattle Regression Manual Test

Package: `V0.4-UnifiedBattleRegression01-UnityRuntimeQA01-Retry`
Generated: `2026-07-07`
Status: `PARTIAL_PASS / OLD_FLOW_SMOKE_BLOCKED_BY_COMPILE_ERROR`

## Completed Unity Checks

```text
BattleSnapshotAdapter menu: PASS
UnifiedBattlePageShell menu: PASS
MainHome Fix02 PlayMode first frame: PASS
UnifiedBattleRegression menu: NOT_AVAILABLE
```

Logs:

```text
Logs/codex_unified_battle_regression_retry_snapshot_report.log
Logs/codex_unified_battle_regression_retry_shell_report.log
Logs/codex_unified_battle_regression_retry_mainhome_fix02_playmode.log
```

MainHome Fix02 success markers:

```text
FIX02_SCENE_STATIC_SUCCESS
FIX02_PLAYMODE_FIRST_FRAME_SUCCESS
FIX02_SMOKE_SUCCESS
```

The old `FullBackgroundBlackUnderlay is missing` failure did not recur.

## Shell Manual Checks

| Step | Expected | Current Evidence | Status |
| --- | --- | --- | --- |
| Open shell scene | Isolated devOnly scene exists | `Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` exists | PASS |
| Open shell prefab | Isolated devOnly prefab exists | `UnifiedBattlePageShell.prefab` exists | PASS |
| Inspect `BattlePageRoot` | Root exists | Shell report + hierarchy map | PASS |
| Inspect required slots | 11 slots exist | `Asset slots present: 11` | PASS |
| Inspect marker flags | devOnly true; enabled/formal/route false | Shell report | PASS |
| Inspect `ResultRewardPlaceholder` | Placeholder only, no save/reward action | Shell report / leak report | PASS |
| Inspect `DevOnlyDiagnosticsSlot` | Separate from player-visible areas | Adapter slot map | PASS |
| Inspect BuildSettings | Shell scene not listed | Shell report says `Scene in BuildSettings: NO` | PASS |

## Old-Flow Smoke

Passed:

```text
V03 MainHome Fix02 first-frame PlayMode smoke
```

Blocked before smoke execution:

```text
V03 MainHome Retry / Task09-10 smoke
```

Blocking compile errors:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs(2376,19): error CS0103: The name 'ResolveWholeItemVisualStyle' does not exist in the current context
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs(2446,55): error CS0103: The name 'ResolveWholeItemVisualStyle' does not exist in the current context
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs(2468,55): error CS0103: The name 'ResolveWholeItemVisualStyle' does not exist in the current context
```

Old-flow items not completed because Unity cannot compile:

```text
Formal old entry still enterable
Board display
Damage number display
Battle progression
BossInfoPanel before 2-10
2-10 Boss manual trigger
Failure after battle still returns to prepare
Reward/save/chapter-progress runtime abnormality check
```

## Safety Notes

```text
No Package 4 work started.
No formal route connected.
No BuildSettings change made.
No RunFlow / SaveData / RewardService / BossInfoPanel / chapter progression change made by UnifiedBattle.
No commit / tag / push performed.
```

## Required Follow-up

Guard should open or route a separate fix/QA package for the unrelated BuildSandbox compile blocker before this runtime retry can be considered a full pass.
