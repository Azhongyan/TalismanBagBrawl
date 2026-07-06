# Mobile Rotate Zone Interaction Report

Package: `V0.4-MobileRotateZoneInteraction01`
Guard pass: `GUARD_PASS_MOBILE_ROTATE_ZONE_INTERACTION01 / READY_FOR_DEV`
Date: `2026-07-06`

## Scope

This package is limited to V0.4 BuildSandbox devOnly multicell placement interaction.
It does not connect to V0.3 formal chapter flow, RunFlow, PageState, FormationState, SaveData, Reward, Boss, or formal numeric content.

Allowed code surfaces touched:

- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/MobileShapePlacementInputExtension.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapePlacementSession.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildGridInteractionPreviewSceneBinder.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/MobileShapePlacementRuntimeIntegrationValidator.cs`
- `Docs/V0.4/Reports/*`

No scene, prefab, RectTransform asset, scene binder run, commit, tag, or push was performed.

## Implementation

Single tap now remains an information action. `BuildItemPreviewCardView` suppresses click after drag and uses a formal drag movement threshold of `18px`, while `MobileShapePlacementInputSettings` keeps tap movement at `15px`.

The previous info-panel/card rotation path is removed from the current package surface:

- `BuildSandboxItemInfoPanel.Show` hides the rotate button unless `rotateEnabled == true`.
- `BuildGridInteractionPreviewController.CanRotateItemFromInfoPanel` now returns `false`.
- `BuildGridInteractionPreviewController` disconnects the item info rotate handler with `SetRotateHandler(null)`.
- legacy compatibility entry points now only explain the drag-zone rule.

Drag rotation is handled by `BuildGridInteractionPreviewController` while the authoritative data remains in `ShapeItemPayload` and `ShapePlacementSession`.

- Runtime rotate-button UI is created with `DontSave` flags and does not write back to scenes.
- Rotate buttons are attached around the drag ghost and follow the floating item: left button on the item-left side, right button on the item-right side.
- Button placement uses the current ghost footprint width, so the button center sits at `ghostHalfWidth + 44px gap + 32px halfButton` from the ghost center. This keeps the controls away from the item instead of sticking to the item edge.
- Buttons stay visible for the whole active drag of a rotatable x2/x3 item, including tray/board/outside-board hover, until release/cancel.
- x1 and x4 shapes do not show rotate buttons.
- Rotation requires a `RotateSeek` intent before hit testing: within `0.22s`, horizontal movement must be at least `28px`, horizontal speed at least `420px/s`, and vertical drift no more than `64px`.
- Once `RotateSeekLeft` or `RotateSeekRight` starts, the target activation rect is frozen briefly so the moving finger can actually reach the button instead of chasing a button that moves away at the same speed.
- Visual button size is `64px`; activation rect is `116px x 156px` to account for the finger offset below the ghost.
- Left button rotates counterclockwise; right button rotates clockwise.
- Moving away from both buttons is required before the same button can trigger again, with `1.0s` trigger cooldown.
- If the item currently has a board preview anchor, rotation validates through `ShapeGridReceiver.CanPlace` before changing item/session rotation; if the item is only floating without a board preview, the floating orientation updates first and release validation remains authoritative.
- Invalid rotation keeps the previous rotation and shows `当前位置无法旋转`.

Legal release still commits through the existing `ShapePlacementSession.Commit` route. Illegal release or release outside board/tray cancels and returns to the source behavior already used by the placement controller.

Battle lock behavior:

- Click in battle mode opens read-only details.
- Drag in battle mode shows `阵势已启，战后可整备` and does not move the item.

## Notes

The older runtime playtest adapter in BuildSandbox no longer calls the old lock/confirm runtime path; it commits valid ghost drops directly and cancels invalid drops. The older input extension methods remain as historical compatibility/test APIs, but the current package surface does not call them.

## Validation

- Targeted `git diff --check` for this package's touched files: PASS.
- Static old-interaction prompt scan for BuildSandbox/Editor code: PASS.
- Static current-runtime call scan for old tap-rotate/ghost-confirm methods: PASS.
- Full-repo `git diff --check`: BLOCKED by pre-existing scene whitespace in dirty scene files outside this package scope.

Unity playmode/manual Console verification is still required for acceptance item 13.
