# Mobile Rotate Zone Interaction Leak Check Report

Package: `V0.4-MobileRotateZoneInteraction01`
Date: `2026-07-07`

Leak Check: `0`

## Static Checks

| Check | Result | Evidence |
| --- | --- | --- |
| Formal V0.3 flow untouched | PASS | No edits to RunFlow, PageState, FormationState, SaveData, Reward, Boss, or formal numeric files. |
| Scene/prefab assets untouched | PASS | No scene or prefab edit was made for this package. |
| Scene binder not run | PASS | Binder code text was updated only to avoid future stale wording; no binder execution was performed. |
| Current controller does not call old tap-rotate path | PASS | `BuildGridInteractionPreviewController` does not call `.TapSelectedItemToRotate(`. |
| Current controller does not call old ghost confirm path | PASS | `BuildGridInteractionPreviewController` does not call `.TapGhostToConfirm(`, `.ReleaseDragLockPreview(`, or `.DragLockedGhostToReceiver(`. |
| Info-panel rotate button hidden | PASS | `CanRotateItemFromInfoPanel` returns `false`; info panel hides rotate button when `rotateEnabled == false`; handler is disconnected. |
| x1/x4 rotate buttons hidden | PASS | `CanActiveDragUseRotateZones` rejects `cellCount <= 1` and `cellCount >= 4`. |
| x2/x3 rotate buttons visible for full active drag | PASS | Rotatable x2/x3 payloads pass `ItemRotationInputExtension.CanRotate`; buttons follow the drag ghost instead of fixed screen/board sides. |
| Rotate buttons are spaced away from item edge | PASS | Button center offset is `ghostHalfWidth + 44px + 36px`, not a fixed close-to-item offset. |
| RotateSeek intent gate | PASS | Rotation requires horizontal seek: `dx >= 20px`, `vx >= 300px/s`, `abs(dy) <= 76px`, within a `0.28s` seek window. |
| Frozen activation target | PASS | Once seek starts, the activation target rect is frozen briefly so the finger can reach the button. |
| Missed seek resumes movement | PASS | If the frozen activation rect is not hit within the seek window, normal drag movement resumes without rotation. |
| RotateSeek does not freeze drag | PASS | Candidate seek and post-trigger cooldown states no longer consume drag frames; board/tray preview continues unless the frame actually triggers rotation. |
| Rotate intent visual state | PASS | Runtime rotate buttons use low-alpha idle visuals, active target scale/highlight, opposite-side dimming, and a non-raycast guide band during rotate intent/cooldown. |
| Trigger cooldown | PASS | Button rotation has a `1.0s` cooldown and still requires leaving the expanded exit rect before another trigger. |
| Board-anchored rotation does not pre-block | PASS | If a board preview anchor exists, button entry rotates first and then previews the rotated payload; `boardReceiver.CanPlace` no longer blocks `ShapePlacementSession.RotateTo`. |
| Floating rotation before release validation | PASS | Button entry updates the floating payload orientation regardless of current placement legality; later release validation remains authoritative. |
| Invalid preview cannot block rotation | PASS | Invalid board/tray areas can still trigger rotate-button entry; invalid preview data is used only for redraw/release validation, not as a pre-rotation blocker. |
| Drag warning text suppressed | PASS | `UpdateActiveDrag` no longer writes per-frame valid/invalid placement text; invalid release cancels/returns without warning text. |
| Board soft boundary | PASS | Board `ScreenPointToCell` accepts a `0.45` cell soft boundary and clamps to the nearest edge cell before strict `CanPlace`/`Commit` validation. |
| Runtime playtest no longer calls old lock/confirm | PASS | `BattlePrepareComponentAdapterRuntimePlaytest` uses direct valid-drop commit and no longer calls the old runtime lock/confirm methods. |
| Stale old-interaction UI text removed from code templates | PASS | Source scan found no `信息弹窗`, `松手锁定`, `再点击确认`, `点击旋转`, or `锁定虚影` prompt in current BuildSandbox/Editor code. |
| Targeted package diff whitespace | PASS | `git diff --check` passes for this package's touched files. |
| Full-repo diff whitespace | BLOCKED_EXTERNAL | Full-repo `git diff --check` is blocked by pre-existing dirty scene whitespace outside this package scope. |

## Compatibility Note

`MobileShapePlacementInputExtension` still contains historical public compatibility/test methods from earlier V0.4 packages, and older editor validators still sample them directly. They are not called by the current mobile rotate-zone package controller or runtime playtest route, so they are excluded from this package leak count.

## Manual Gate

Unity playtest/Console validation remains required for final acceptance item 13.
