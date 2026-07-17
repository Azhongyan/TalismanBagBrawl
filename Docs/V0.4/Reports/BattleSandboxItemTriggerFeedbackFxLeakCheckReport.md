# BattleSandboxItemTriggerFeedbackFx Leak Check

Package: `V0.4-BattleSandboxItemTriggerFeedbackFx01`
Date: `2026-07-08`
Resource sequence trial update: `2026-07-10`

## Result

Static package leak check: `PASS_WITH_LOCAL_STATIC_LIMITATION`

## Checked Boundaries

- No `Assets/_Game/Scripts/TalismanBag/V02/**` changes.
- No `Assets/_Game/Scripts/TalismanBag/V03/**` changes.
- No formal RunFlow/PageState/FormationState changes.
- No SaveData / PlayerPrefs / MainTrialProgressData changes.
- No Reward / drop / formal Boss reward changes.
- No ProjectSettings / BuildSettings changes.
- No scene binder was run.
- No scene RectTransform/layout overwrite was introduced by this package.

## Runtime Isolation

- New effect controller is runtime-created and devOnly:
  - `BattleSandboxItemTriggerFeedbackController`
  - runtime-created UI objects use `HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild`
- Active/passive feedback channels are display-only fields on `BattleSandboxRuntimeLoopRow`.
- Passive build modifier feedback reads `BattleSandboxBuildCombatPreview.context.modifierBundle` as devOnly attribution data only.
- No passive feedback row writes formal combat settlement, save data, reward, or progression state.
- Board anchor lookup is read-only:
  - prefers existing placed artwork rect
  - falls back to existing board-cell layout data
- The procedural VFX layer is transient and cleared on loop reset/disable.
- Inspector sequence frame slots are local serialized Sprite references on the devOnly feedback controller and preserve manual frame order.
- Resource sequence trial only reads from `Assets/_Game/Resources/anim/照煞镜_VFX_RGBA_9帧/frames`.
- Runtime-created sequence UI objects and fallback-created sprites use `HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild`.
- The trial is gated to `preview_fire_talisman`, `damage`, and single-cell occupied rows, then falls back to procedural VFX when Resource frames are unavailable.
- Build modifier passive rows are sampled at lower step frequency, then passive playback uses global/per-item throttling inside the runtime-only controller to avoid stealing active trigger readability.

## Validation Notes

- Package-scoped `git diff --check` passed for the touched V0.4 BuildSandbox scripts and this report set.
- `git diff --check` currently reports pre-existing trailing whitespace in dirty V04 scene files. Those scene files were already modified before this package and were not edited for this package.
- No `.sln` or `.csproj` was found for `dotnet build`; Unity compile validation still needs an Editor import/Play check.
