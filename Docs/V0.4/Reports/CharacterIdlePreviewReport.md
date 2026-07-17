# CharacterIdlePreview Report

Package: `V0.4-CharacterIdlePreview01`
Date: `2026-07-14`

## Scope

- Runtime scope: V0.4 BuildSandbox / devOnly preview scene only.
- Target scene surface: `Scene_TalismanBag_V04_BattleSandboxPreview`
- Formal V0.2/V0.3 combat, RunFlow, PageState, FormationState, save data, reward, drop, Boss reward, and BuildSettings were not modified.

## Implementation

- Added `BuildSandboxCharacterIdlePreview`, an editable UI sprite-sequence preview component.
- `BuildGridInteractionPreviewController` now ensures one scene preview object:
  - object name: `CharacterIdlePreview_IdleAnim`
  - parent preference: `BattleLikePreviewArea`
  - fallback parent: board preview parent
- Default frame source:
  - `Assets/_Game/Resources/anim/video_6a56065e5485ff99517e29da_1784022639683`
  - Resources path: `anim/video_6a56065e5485ff99517e29da_1784022639683`
  - Current trial uses the 10 cutout frames in that folder directly as a looping idle animation.
- Artist-facing controls:
  - `Inspector Frame Slots`: manual Sprite frame list, preserving Inspector order.
  - `Resources Frames Path`: fallback Resources folder path.
  - `Frames Per Second`: idle playback speed.
  - `Loop`: idle repeat toggle.
  - `Preserve Aspect`: Image aspect preservation.
- If Sprite import is not ready, the component can load `Texture2D` frames from Resources and create transient runtime sprites.

## Hand Test

- Open the V0.4 battle sandbox scene.
- Select `CharacterIdlePreview_IdleAnim` under `BattleLikePreviewArea`.
- Enter Play or let the component preview its first frame in edit mode.
- Adjust RectTransform, `Frames Per Second`, and `Inspector Frame Slots` as needed.
