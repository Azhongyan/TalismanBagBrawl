# UnifiedBattle Regression Report

Package: `V0.4-UnifiedBattleRegression01-UnityRuntimeQA01-Retry`
Generated: `2026-07-07`
Status: `RETRY_BLOCKED_BY_UNRELATED_UNITY_COMPILE_ERROR`

## Scope

This retry validates that `BattleSnapshotAdapter01` and `UnifiedBattlePageShell01` remain isolated from the formal V0.2 / V0.3 flow after the MainHome underlay fix.

This package did not develop `LegacyChapterBattleAdapter01`, `BattleRouteBridge01`, `GoldenPathBridgeRegression01`, formal routing, save, reward, Boss, or chapter progression behavior.

## Unity Status

- Initial Unity lock check: `RELEASED`
- Initial `Temp/UnityLockfile`: `false`
- BattleSnapshotAdapter Unity menu rerun: `PASS`
- UnifiedBattlePageShell Unity menu rerun: `PASS`
- UnifiedBattleRegression Unity menu: `NOT_AVAILABLE` (no existing Package 3 menu found)
- V03 MainHome Fix02 PlayMode first-frame smoke: `PASS`
- V03 MainHome Fix02 missing-underlay error: `NOT_REPRODUCED`
- Old formal flow MainHome Retry / Task09-10 smoke: `BLOCKED_BEFORE_SMOKE_BY_SCRIPT_COMPILE_ERROR`
- Final Unity process check after cleanup: no `Unity.exe` process found
- Final project lock check after cleanup: `Temp/UnityLockfile = false`

Unity batch logs:

```text
Logs/codex_unified_battle_regression_retry_snapshot_report.log
Logs/codex_unified_battle_regression_retry_shell_report.log
Logs/codex_unified_battle_regression_retry_mainhome_fix02_playmode.log
Logs/codex_unified_battle_regression_retry_oldflow_mainhome_smoke.log
```

## Passing Checks

### BattleSnapshotAdapter

- Report file: `Docs/V0.4/Reports/BattleSnapshotAdapterReport.md`
- Status: `PASS`
- Errors: `0`
- Warnings: `0`
- Leak Count: `0`
- Sandbox result: `devOnly=True`, `shouldWriteSave=False`, `shouldGrantReward=False`

### UnifiedBattlePageShell

- Scene exists: `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity`
- Prefab exists: `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab`
- Status: `PASS`
- Validation source: `UNITY_EDITOR_MENU`
- Validation mode: `UNITY_ASSET_STATIC`
- Required slots: `11`
- Asset slots present: `11`
- Leak count: `0`
- Shell scene in BuildSettings: `NO`

Marker isolation confirmed by refreshed shell report:

```text
devOnly = true
isEnabled = false
formalFlow = false
connectedToFormalRoute = false
```

Required shell slots confirmed:

```text
BattlePageRoot
BoardArea
ItemTrayArea
EnemyInfoArea
BossCastBarSlot
BattleFeedbackLayer
StoryGuidePopupLayer
ResultRewardPlaceholder
V03FlowAdapterSlot
V04SandboxAdapterSlot
DevOnlyDiagnosticsSlot
```

### MainHome Fix02 Smoke

`Logs/codex_unified_battle_regression_retry_mainhome_fix02_playmode.log` contains:

```text
FIX02_SCENE_STATIC_SUCCESS
FIX02_PLAYMODE_FIRST_FRAME_SUCCESS
FIX02_SMOKE_SUCCESS
```

`FullBackgroundBlackUnderlay is missing` did not appear in the retry PlayMode log.

## Blocking Failure

The old formal flow MainHome Retry / Task09-10 smoke did not execute because Unity hit script compilation errors first:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs(2376,19): error CS0103: The name 'ResolveWholeItemVisualStyle' does not exist in the current context
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs(2446,55): error CS0103: The name 'ResolveWholeItemVisualStyle' does not exist in the current context
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs(2468,55): error CS0103: The name 'ResolveWholeItemVisualStyle' does not exist in the current context
```

Observed log:

```text
Logs/codex_unified_battle_regression_retry_oldflow_mainhome_smoke.log
```

Classification:

```text
Not a UnifiedBattle route leak.
Not a MainHome underlay failure.
Not a RunFlow / SaveData / RewardService / BossInfoPanel / chapter progression modification by this package.
The failing file is an unrelated dirty BuildSandbox file outside this Package 3 retry scope.
```

Per the assignment failure rule, this retry stopped at the compile failure and did not repair the dirty BuildSandbox file.

## Formal Flow Protection

- No Package 4 work was started.
- No `LegacyChapterBattleAdapter01` was added.
- No `BattleRouteBridge01` was added.
- No formal chapter data was converted into `BattleContract`.
- No UnifiedBattle formal route was connected.
- No shell scene was added to BuildSettings.
- No UnifiedBattle code wrote SaveData, PlayerPrefs, MainTrialProgressData, rewards, Boss state, or chapter progress.
- `Scene_TalismanBag_V04_UnifiedBattlePageShell` remains absent from BuildSettings.

## Old-Flow Smoke Result

Passed:

```text
MainHome displays normally at Fix02 first frame.
FullBackgroundBlackUnderlay exists and remains visible.
```

Blocked:

```text
Formal old entry still enterable
Board display
Damage number display
Battle progression
BossInfoPanel before 2-10
2-10 manual trigger
Failure after battle still returns to prepare
Reward/save/chapter-progress runtime abnormality check
```

Reason:

```text
Unity cannot complete a fresh old-flow smoke while the unrelated BuildSandbox compile errors are present.
```

## Conclusion

UnifiedBattle-specific no-pollution checks remain clean with leak count `0`, and MainHome Fix02 PlayMode smoke now passes.

This retry is **not** a full Unity runtime QA pass because the final old-flow smoke is blocked by unrelated Unity compile errors in `BuildGridInteractionPreviewController.cs`. Guard should not record `UNIFIED_BATTLE_REGRESSION01_UNITY_RUNTIME_QA_PASS_CANDIDATE` until that compile blocker is resolved or a new authorized QA path is provided.
