# CharacterIdlePreview Leak Check

Package: `V0.4-CharacterIdlePreview01`
Date: `2026-07-14`

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

## Runtime Isolation

- `BuildSandboxCharacterIdlePreview` is a devOnly BuildSandbox component.
- `CharacterIdlePreview_IdleAnim` is an editable sandbox scene UI preview object created under `BattleLikePreviewArea`.
- Default frames are read from `Resources/anim/video_6a56065e5485ff99517e29da_1784022639683`.
- Inspector frame slots are local serialized Sprite references and preserve manual frame order.
- Texture fallback sprites are transient and use `HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild`.

## Validation Notes

- Package-scoped `git diff --check` passed for the touched BuildSandbox scripts and this report set.
- No `.sln` or `.csproj` was found for `dotnet build`; Unity compile and Play validation still need an Editor import check.
