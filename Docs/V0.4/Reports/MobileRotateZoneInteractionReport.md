# Mobile Rotate Zone Interaction Report

Package: `V0.4-MobileRotateZoneInteraction01`
Guard pass: `GUARD_PASS_MOBILE_ROTATE_ZONE_INTERACTION01 / READY_FOR_DEV`
Date: `2026-07-07`

## Scope

This package is limited to V0.4 BuildSandbox devOnly multicell placement interaction.
It does not connect to V0.3 formal chapter flow, RunFlow, PageState, FormationState, SaveData, Reward, Boss, or formal numeric content.

Allowed code surfaces touched:

- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs`
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
- A single clockwise rotate button is attached inside the drag ghost's right-bottom frame and follows the snapped board-preview item.
- Button placement uses the current ghost footprint, so the button center is pulled inside the right-bottom frame by the visual half-size plus `6px` when there is room; small footprints clamp the inset to `36%` of the ghost width/height. This keeps the control reachable while preserving the down-right visual target.
- After the board-snap trial, the button stays hidden during free/tray drag and appears only while a rotatable x2/x3 item has an active board preview, until release/cancel.
- x1 and x4 shapes do not show the rotate button.
- Rotation requires a right/down-biased `RotateSeek` intent before hit testing: within `0.28s`, right movement must be at least `20px`, rightward speed at least `300px/s`, and upward drift no more than `44px`. Down-right movement is intentionally favored over moving the item.
- Once `RotateSeekRight` starts, the target activation rect is frozen briefly so the finger can reach the button instead of chasing a button that moves away at the same speed.
- Visual button size is `72px`; activation rect is `136px x 180px` to account for the finger offset below the ghost.
- The left/counterclockwise rotate zone is disabled for this handfeel pass; the right-bottom button rotates clockwise only.
- Moving away from the button is required before the same button can trigger again, with `1.0s` trigger cooldown.
- Rotate-button entry changes the floating payload orientation first. If a board preview anchor exists, the rotated payload is previewed at that anchor to refresh the visual footprint; `ShapeGridReceiver.CanPlace` no longer blocks rotation before `ShapePlacementSession.RotateTo`.
- Invalid rotation keeps the previous rotation without writing warning text during drag.
- Tray artwork is now the runtime visual source for ghost, board preview, and placed board cells. `BuildItemPreviewCardView` first captures a real child Sprite Image from the card artwork slot, preserves that Image's source rotation, and marks it as a whole-item artwork source, then falls back to layout-cell Images with Sprite/manual style and finally the card background. Whole-item artwork is rendered once through runtime-only `DragGhostArtwork` / `BoardItemArtworkLayer` overlays spanning the occupied-cell bounds. When whole-item artwork exists, board/ghost cells suppress item color blocks entirely and the image itself follows `ItemShapeRotation`; when no artwork exists, the previous color-block rendering remains the fallback. `BuildItemTrayPreviewView` still falls back to occupied tray-slot Images by item id when no card artwork exists, and `BuildGridPreviewSlotView` restores its default empty-cell template when cleared.
- Whole-item artwork now owns invalid/snap feedback too. When a Sprite artwork exists, invalid drag feedback is a red runtime overlay on the same image silhouette instead of a red cell block, and board snap preview draws a slightly larger, downward-offset image shadow under the snapped preview. Color-block invalid feedback remains only as the no-image fallback.

Legal release still commits through the existing `ShapePlacementSession.Commit` route. Illegal release or release outside board/tray cancels and returns to the source behavior already used by the placement controller.

Battle lock behavior:

- Click in battle mode opens read-only details.
- Drag in battle mode shows `闃靛娍宸插惎锛屾垬鍚庡彲鏁村` and does not move the item.

## Handfeel Update 2026-07-07

- Drag preview no longer writes per-frame valid/invalid placement text; board/tray visuals remain the feedback surface during movement.
- Rotation failure now stays silent during drag. Rotation is blocked only by non-rotatable payload/session state, not by current placement legality; release validation remains the authority for whether the item can be placed.
- Invalid drag-preview results are no longer used to size the drag ghost, so edge/out-of-bounds previews do not move the ghost-following rotate buttons while the player is chasing a rotate button.
- Invalid board previews may still be used as redraw anchors after rotation, but only for visual refresh and release validation, never as a pre-rotation blocker.
- The board receiver accepts a soft pointer boundary of `0.45` cell outside the visual board and clamps to the nearest edge cell. Final placement remains strict through `CanPlace` and `Commit`; impossible drops still return to source.
- `2026-07-07` small-screen tuning: rotate seek was softened to `20px`, `300px/s`, `0.28s`; the current single-button pass allows down-right drift and caps only upward drift at `44px`. The rotate button remains `72px` visual and `136px x 180px` activation.
- `2026-07-07` drag smoothness tuning: `RotateSeek` candidate/cooldown states no longer consume drag frames. Normal board/tray preview continues while seeking or waiting to leave a button; only the frame that actually triggers rotation is consumed.
- `2026-07-07` rotate-intent visual tuning: idle rotate buttons are lower alpha; candidate seek shows only a weak non-raycast guide band. A button brightens/scales up only for `0.18s` after a successful rotation trigger; cooldown and leave-to-rearm state no longer light the button.
- `2026-07-07` board-snap trial: rotate controls are hidden during free/tray drag and appear only after an active board preview exists. The drag ghost snaps to the board occupied-cell bounds while hovering the board, so rotation is adjusted from a board-attached preview state rather than a fully free-floating state.
- `2026-07-07` board-snap hold tuning: while chasing a rotate button from a board preview, the controller reuses the previous board anchor instead of letting tray/out-of-board pointer checks steal the preview. The hold applies during rotate seek, leave-to-rearm, the short successful-trigger flash, or inside the rotate button activation rect plus `36px` padding.
- `2026-07-07` single-button tuning: the left rotate zone is hidden and non-interactive. The remaining clockwise button sits inside the snapped item's right-bottom frame, and down-right drag intent is treated as rotate seek before normal placement movement.
- `2026-07-07` tray-board art sync fix: item art configured on a card child Image/art slot is preferred over generated layout cells, so runtime `TrayLayoutCell_*` color blocks no longer mask the real Sprite source. Whole-item art now renders as one overlay across the full multi-cell footprint instead of being copied into every occupied grid cell. Whole-item artwork direction is tunable in the `BuildGridInteractionPreviewController` Inspector under `V0.4 Artwork Direction Calibration`, with separate Tray / DragGhost / BoardPlaced rotation offsets. The visible tray Image can be offset first, then ghost and placed-board overlays add their own offsets on top of the captured source Image rotation and current `Rotation90/180/270`. Defaults keep the current pass behavior: the 3-cell triangle/corner shape uses `+90` for DragGhost/BoardPlaced and other whole-item artwork uses `-90`; Tray defaults to `0`. If no card artwork Sprite exists, the capture falls back to layout-cell/manual styles, then occupied tray-slot Sprites, then the previous color-only fallback.
- `2026-07-07` triangle physical rotation fix: physical cell rotation now treats `Rotation90` as clockwise in screen/board coordinates, matching the artwork rotation path. Corner3 / 3-cell corner therefore remains 3 occupied cells across `Rotation0/90/180/270` and no longer rotates physical cells opposite to the image.
- `2026-07-07` rotate-anchor clamp fix: after a rotate-button trigger from a board-snapped preview, the preview anchor is clamped against the rotated payload footprint before redraw. This prevents a rotated Corner3 footprint from showing only the in-board two cells when the previous anchor would push the third occupied cell outside the board.
- `2026-07-07` image-first visual fix: when a whole-item image exists, drag ghost, board preview, and placed board visuals suppress the old valid/invalid/placed item color blocks. The cells still own interaction and occupancy state, but the visible item body is the image only.
- `2026-07-07` rotate-after-art fix: rotation-trigger redraw no longer requires `result.IsValid` before using the whole-item artwork overlay. Invalid/overlap rotation previews still render the rotated occupied-cell layout and keep the item art visible without falling back to item color blocks.
- `2026-07-08` image feedback overlay fix: whole-item invalid feedback is now rendered by `DragGhostInvalidArtwork` / `BoardPreviewInvalidArtwork` as a red tint over the same Sprite mask, while `BoardPreviewShadowArtwork` draws a green, more enlarged, farther downward-offset snap residual under the snapped board preview. This keeps illegal/snap feedback aligned to the image silhouette instead of reintroducing per-cell blocks.
- `2026-07-08` hierarchy placement feedback helper: `PlacementFeedback_Runtime` is now created/bound as a normal Edit Mode hierarchy object by `BuildGridInteractionPreviewController.OnValidate`, without running the full scene binder. Runtime lookup first reuses this visible scene object and only falls back to a temporary `DontSave` object if no hierarchy feedback exists.
- `2026-07-08` placement feedback color authoring fix: scene-backed `PlacementFeedback_Runtime` disables state-driven background color writes, so Inspector color edits are preserved after `ShowNeutral` / `ShowValid` / `ShowInvalid` refreshes. Only the temporary runtime fallback keeps the old state-color behavior.
- `2026-07-08` cell-image underlay fix: board cells and tray slots now treat their authored `Image` Sprite/Color as an occupancy underlay. Empty cells/slots are alpha-hidden, occupied or board-preview cells restore the authored image, and whole-item drag ghosts keep `DragGhostCellLayer` visible under the item artwork so floating drag also shows the uploaded cell image. Legacy color blocks remain only as the no-art fallback path.
- `2026-07-08` battle layout authoring fix: `BuildGridInteractionPreviewController` now exposes `Battle State Y Offset` under `V0.4 Battle Layout`, replacing the fixed `320px` battle pull-down. The default remains `-320`, and more negative values move `BattleLikePreviewArea` lower during battle state.
- `2026-07-08` cell glow material helper: added `UI_CellGlow_Additive.mat` / `UI_CellGlow_Additive.shader` for authored tray/board cell images. The shader uses UI stencil/clip support with additive blending and a `Glow Intensity` slider, so slot Images can be brightened without changing placement logic.

## Notes

The older runtime playtest adapter in BuildSandbox no longer calls the old lock/confirm runtime path; it commits valid ghost drops directly and cancels invalid drops. The older input extension methods remain as historical compatibility/test APIs, but the current package surface does not call them.

## Validation

- Targeted `git diff --check` for this package's touched files: PASS.
- Static old-interaction prompt scan for BuildSandbox/Editor code: PASS.
- Static current-runtime call scan for old tap-rotate/ghost-confirm methods: PASS.
- Targeted image-feedback overlay scan for runtime object names: PASS.
- Full-repo `git diff --check`: BLOCKED by pre-existing scene whitespace in dirty scene files outside this package scope.

Unity playmode/manual Console verification is still required for acceptance item 13.
