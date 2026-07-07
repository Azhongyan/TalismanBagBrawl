# V0.3-MainHomeFullBackgroundUnderlayFix01 Report

## Scope

- Package: `V0.3-MainHomeFullBackgroundUnderlayFix01`
- Target scene: `Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity`
- Goal: restore the missing `FullBackgroundBlackUnderlay` required by the locked MainHome first-frame verifier.
- Not in scope: UnifiedBattle Package 4, formal route wiring, BattleContract chapter data, RunFlow, SaveData, RewardService, BossInfoPanel, chapter progression, BuildSettings, ProjectSettings.

## Root Cause

`V03MainHomeSceneFix02.VerifyStaticScene()` requires exactly one `FullBackgroundBlackUnderlay` under the MainHome content root. The scene contained `FullBackgroundImageSlot` but no `FullBackgroundBlackUnderlay`, so PlayMode smoke failed before first-frame validation.

Classification: scene object missing.

## Fix

- Added a guarded editor-only repair entry:
  `Tools/Talisman Bag/V0.3/Fix02/[Writes Scene][Manual Only] Repair Full Background Underlay`
- The repair opens `Scene_TalismanBag_V03_MainHome`, creates or normalizes one `FullBackgroundBlackUnderlay`, parents it under `MobileSafeAreaRoot`, moves it to sibling index 0, and saves the scene.
- The underlay has:
  - `RectTransform` full stretch
  - black enabled `Image`
  - `raycastTarget = false`

## Verification

- Repair log:
  `Logs/codex_mainhome_underlay_fix01_repair.log`
- PlayMode smoke log:
  `Logs/codex_mainhome_underlay_fix01_playmode_noquit.log`
- Final PlayMode smoke log:
  `Logs/codex_mainhome_underlay_fix01_playmode_final.log`
- GameView evidence:
  `Logs/v0.3_mainhome_fix02_gameview.png`

Results:

- `FIX02_SCENE_STATIC_SUCCESS`: PASS
- `FIX02_PLAYMODE_FIRST_FRAME_SUCCESS`: PASS
- `FIX02_SMOKE_SUCCESS`: PASS
- `FullBackgroundBlackUnderlay is missing`: not present after fix
- `error CS`: not present
- `Script compilation failed`: not present
- `InvalidOperationException`: not present

Note: Unity startup emitted non-project licensing/IPC messages in batch logs. They did not stop compilation, repair, static verification, or PlayMode smoke.

## Non-Changes

- SaveData / PlayerPrefs / MainTrialProgressData: not modified
- RewardService / reward granting: not modified
- Chapter progression: not modified
- BossInfoPanel: not modified
- V02/V03 RunFlow: not modified
- UnifiedBattle files: not modified by this package
- BuildSettings / ProjectSettings: not modified
- Commit / tag / push: not performed
