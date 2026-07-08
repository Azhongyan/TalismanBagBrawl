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
| x1/x4 rotate button hidden | PASS | `CanActiveDragUseRotateZones` rejects `cellCount <= 1` and `cellCount >= 4`. |
| x2/x3 rotate button visible after board snap | PASS | Rotatable x2/x3 payloads pass `ItemRotationInputExtension.CanRotate`; the single right-bottom button follows the snapped board-preview ghost instead of fixed screen/board sides. |
| Rotate button sits inside item frame | PASS | Button center is pulled inside the ghost right-bottom frame by visual half-size plus `6px` when possible, clamped to `36%` of ghost width/height for small footprints. |
| RotateSeek intent gate | PASS | Rotation requires right/down-biased seek: `dx >= 20px`, `vx >= 300px/s`, and upward drift `dy <= 44px`, within a `0.28s` seek window. |
| Left rotate zone removed | PASS | The left runtime view is hidden, no left activation rect is used, and rotation resolves clockwise only. |
| Frozen activation target | PASS | Once seek starts, the activation target rect is frozen briefly so the finger can reach the button. |
| Missed seek resumes movement | PASS | If the frozen activation rect is not hit within the seek window, normal drag movement resumes without rotation. |
| RotateSeek does not freeze drag | PASS | Candidate seek and post-trigger cooldown states no longer consume drag frames; board/tray preview continues unless the frame actually triggers rotation. |
| Rotate intent visual state | PASS | Runtime rotate button uses low-alpha idle visuals; candidate seek uses a weak non-raycast guide band, and button scale/highlight is reserved for a short successful-trigger confirmation. Cooldown/rearm state stays unlit. |
| Board-snap rotate mode | PASS | Free/tray drag hides rotate controls; active board preview snaps the drag ghost to board occupied-cell bounds before showing rotate controls. |
| Board-snap hold while chasing button | PASS | Rotate seek/rearm/confirmed-flash states and the rotate activation rect plus `36px` padding reuse the previous board anchor before tray/out-of-board checks can steal the preview. |
| Trigger cooldown | PASS | Button rotation has a `1.0s` cooldown and still requires leaving the expanded exit rect before another trigger. |
| Board-anchored rotation does not pre-block | PASS | If a board preview anchor exists, button entry rotates first and then previews the rotated payload; `boardReceiver.CanPlace` no longer blocks `ShapePlacementSession.RotateTo`. |
| Board-preview rotation before release validation | PASS | Button entry updates the active payload orientation from a board-preview state regardless of current placement legality; later release validation remains authoritative. |
| Invalid preview cannot block rotation | PASS | Invalid board/tray areas can still trigger rotate-button entry; invalid preview data is used only for redraw/release validation, not as a pre-rotation blocker. |
| Tray-to-board artwork sync | PASS | Card child Sprite artwork is captured as whole-item art before generated layout cells. Drag ghost, board preview, and placed board cells render that Sprite once through runtime overlays spanning the occupied-cell bounds instead of repeating it per cell; layout/manual styles and occupied tray-slot Sprites remain fallback sources. |
| Whole-image visual priority | PASS | When whole-item artwork exists, `BuildGridPreviewSlotView` suppresses item color blocks for preview/placed cells and `ApplyDragGhostLayout` hides `DragGhostCellLayer`; color blocks remain only as the no-image fallback. |
| Whole-image invalid overlay | PASS | When whole-item artwork exists, invalid feedback is rendered by runtime-only `DragGhostInvalidArtwork` / `BoardPreviewInvalidArtwork` tint overlays using the same Sprite mask instead of reintroducing red occupied-cell blocks. |
| Board-snap image shadow | PASS | Board snap preview creates runtime-only `BoardPreviewShadowArtwork`, a green, larger, farther downward-offset copy of the same Sprite under the preview image, while placement legality remains owned by `ShapePlacementSession` / receivers. |
| Editable placement feedback | PASS | `BuildGridInteractionPreviewController.OnValidate` now creates/binds `PlacementFeedback_Runtime` as a normal Edit Mode hierarchy object; runtime code reuses that visible scene object before creating a temporary `DontSave` fallback. |
| Placement feedback color authoring | PASS | Scene-backed `PlacementFeedback_Runtime` disables state-driven background color writes, so Inspector Image color edits are not overwritten by feedback state refreshes; only the temporary runtime fallback uses state colors. |
| Cell-image occupancy underlay | PASS | `BuildGridPreviewSlotView` and `TrayGridReservationView` preserve authored slot/cell images and switch alpha for empty vs occupied/preview states; `DragGhostCellLayer` stays visible under whole-item artwork so floating drag uses the same cell-image underlay. |
| Battle layout offset authoring | PASS | `BuildGridInteractionPreviewController` exposes `Battle State Y Offset` for `BattleLikePreviewArea` battle-state positioning; default `-320` preserves previous behavior without touching scene RectTransforms. |
| Cell glow material helper | PASS | Added BuildSandbox-local `UI_CellGlow_Additive` material/shader using additive UI blending and stencil/clip support; it is an optional authoring asset and does not change placement/session logic. |
| Cell underlay child image authoring | PASS | Board/tray cells prefer child Images named `CellUnderlayImage`, `CellImage`, or `UnderlayImage`; moving/resizing that child affects only the visual underlay, while parent grid cell RectTransforms and placement hit areas remain unchanged. |
| Rotate redraw keeps artwork | PASS | `DrawPreviewResult` and `ApplyDragGhostLayout` no longer require `result.IsValid` before resolving whole-item artwork. Invalid rotation previews still use rotated `OccupiedCells` for layout and keep item art visible. |
| Whole-image rotates with item | PASS | Whole-item artwork now preserves the source card Image rotation and adds separate Inspector-tunable Tray / DragGhost / BoardPlaced calibration offsets under `V0.4 Artwork Direction Calibration`. Defaults keep the current pass behavior: the 3-cell triangle/corner shape uses `+90` for DragGhost/BoardPlaced, other whole-item artwork uses `-90`, and Tray uses `0`. `ItemShapeRotation` is then applied on top and 90/270 turns swap draw bounds so the image itself follows the rotated footprint. |
| Triangle physical clockwise rotation | PASS | `Rotation90` now maps physical occupied cells clockwise in screen/board coordinates across `ShapeItemPayload`, validator, runtime integration, and controller helper paths. Corner3 remains 3 occupied cells through all four rotations and follows the image direction. |
| Rotate-anchor full-footprint clamp | PASS | `ClampBoardAnchorForPayload` clamps the board preview anchor after rotate-button triggers so the rotated payload footprint can remain fully in-board when possible, preventing partial two-cell highlights for Corner3 near board edges. |
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
