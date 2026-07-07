# Mobile Rotate Zone Interaction Ops Rollback Report

Package: `V0.4-MobileRotateZoneInteraction01`
Date: `2026-07-07`
Status: `WORKTREE_ONLY / NO_COMMIT`

## Purpose

Record the current mobile rotate-zone handfeel changes so they can be reviewed or rolled back safely if Unity playtest shows regression.

No scene, prefab, binder run, commit, tag, or push was performed for this checkpoint.

## Files In Scope

- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionReport.md`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionStateReport.csv`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionLeakCheckReport.md`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionOpsRollbackReport.md`

## Current Diff Snapshot

Targeted diff stat at this checkpoint:

- `BuildGridInteractionPreviewController.cs`: runtime handfeel logic and runtime-only UI visuals.
- `BuildGridPreviewSlotView.cs`: board slot runtime Sprite/style application and default visual restore.
- `BuildItemPreviewCardView.cs`: tray cell visual style capture from runtime Image components.
- `BuildItemTrayPreviewView.cs`: item-id lookup for tray visual style capture.
- `TrayItemLayoutView.cs`: occupied-cell visual layout now exposes top-left anchored position for board artwork overlays.
- `MobileRotateZoneInteractionReport.md`: handfeel and visual-intent notes.
- `MobileRotateZoneInteractionStateReport.csv`: static acceptance rows.
- `MobileRotateZoneInteractionLeakCheckReport.md`: leak/static check notes.

## Rollback Checkpoints

### A. Roll Back Visual Intent Only

Use if the guide band/highlight makes the UI noisy, but drag/rotate behavior feels good.

Code areas:

- `RotateZoneGuideName`
- `rotateZoneGuide`
- `rotateZoneGuideImage`
- `UpdateRotateZoneGuideRect`
- `EnsureRotateZoneGuideView`
- `SetRotateZoneButtonVisual`
- `ResolveRotateZoneHighlightSide`
- `ResolveRotateZoneGuideSide`
- `confirmedRotateZoneSide`
- `rotateZoneConfirmUntilTime`
- `RotateButtonConfirmVisualSeconds`

Expected rollback result:

- Buttons return to simple idle/active color.
- No guide band appears.
- Successful-rotation-only highlight can be removed with the same visual rollback.
- RotateSeek and drag smoothness behavior remains.

### A2. Roll Back Board-Snap Rotate Mode Only

Use if rotation controls should return to the always-following free-drag version.

Code areas:

- `HasActiveBoardRotatePreview`
- board/tray branches in `UpdateActiveDrag`
- `TrySnapDragGhostToBoardPreview`
- `TryBuildBoardCellsScreenBounds`
- begin-drag `SetRotateZonesVisible(false)` initialization
- `TryPreviewBoardSnapHold`
- `ShouldHoldBoardSnapPreview`
- `ApplyBoardDragPreview`
- `BoardSnapRotateHoldPaddingPixels`

Current behavior:

- Free/tray drag hides rotate controls.
- Board preview snaps the drag ghost to board occupied-cell bounds.
- Rotate controls appear only after board preview exists.
- Rotate-button chase reuses the previous board anchor during seek/rearm/button-area hold.

Rollback result:

- Rotate controls can appear during free drag again.
- Drag ghost always follows finger offset instead of snapping to board preview.
- Board preview no longer holds the previous anchor while chasing rotate buttons.

### B. Roll Back Smooth Drag State Machine Only

Use if x2/x3 drag needs the older rotate-seek freeze behavior.

Code area:

- `UpdateRotateZoneDuringDrag`

Current behavior:

- RotateSeek candidate and button cooldown states return `false`, allowing board/tray preview and ghost movement to continue.
- Only an actual rotate-trigger frame returns `true`.

Older behavior:

- RotateSeek candidate and cooldown states consumed drag frames via `HoldRotateInteractionVisuals`.

### C. Roll Back Illegal-Area Rotate Freedom Only

Use if rotating while outside/invalid board area causes confusing placement previews.

Code area:

- `TryRotateActiveDragFromZone`
- `TryResolveRotateAnchor`

Current behavior:

- Button rotation calls `ShapePlacementSession.RotateTo` without pre-validating `boardReceiver.CanPlace`.
- Board preview is redrawn after rotation if an anchor exists.
- Release validation remains authoritative.

Older behavior:

- Board-anchored rotation could pre-check `boardReceiver.CanPlace` before `RotateTo`.

### D. Roll Back Button Size / Small-Screen Tuning Only

Use if the right-bottom rotate button feels too easy to hit during movement.

Current constants:

- `RotateButtonVisualSizePixels = 72`
- `RotateButtonActivationWidthPixels = 136`
- `RotateButtonActivationHeightPixels = 180`
- `RotateButtonInsideEdgeInsetPixels = 6`
- `RotateSeekWindowSeconds = 0.28`
- `RotateSeekMinDeltaXPixels = 20`
- `RotateSeekMaxUpwardDriftPixels = 44`
- `RotateSeekMinVelocityXPixelsPerSecond = 300`

Current single-button tuning:

- Left rotate view is hidden/non-interactive.
- Button center is inside the ghost right-bottom frame, using visual half-size plus `6px` when possible and a `36%` width/height clamp for small footprints.
- Down-right movement is favored as rotate seek; upward drift is capped at `44px`.

Previous tuning before this small-screen pass:

- Visual size `64`
- Activation `116 x 156`
- Seek window `0.22`
- Min dx `28`
- Max dy `64`
- Min vx `420`

### E. Roll Back Drag Warning / Soft Boundary Only

Use if silent invalid feedback or edge clamping hides too much information.

Code areas:

- Per-frame drag `ShowInvalid` / `ShowValid` suppression inside `UpdateActiveDrag`
- `BoardSoftBoundaryCellPadding = 0.45`
- `TryResolveCellFromBounds`

Current behavior:

- Drag movement uses red/green visuals instead of high-frequency text warnings.
- Board pointer has a `0.45` cell soft boundary before strict `CanPlace` / `Commit`.

### F. Roll Back Tray-Board Artwork Sync Only

Use if board cells should return to color-only previews.

Code areas:

- `ShapeCellVisualStyle`
- `BuildItemPreviewCardView.TryCaptureCellVisualStyles`
- `BuildItemPreviewCardView.TryCaptureArtworkImageVisualStyle`
- `BuildItemTrayPreviewView.TryCaptureItemVisualStyles`
- `BuildItemTrayPreviewView.TryCaptureTraySlotVisualStyles`
- `BuildGridPreviewSlotView.SetPreview(..., ShapeCellVisualStyle)`
- `BuildGridPreviewSlotView.SetPlaced(..., ShapeCellVisualStyle)`
- `BuildGridInteractionPreviewController.visualStylesByItemId`
- `BuildGridInteractionPreviewController.DrawBoardPreviewArtwork`
- `BuildGridInteractionPreviewController.DrawBoardPlacedArtwork`
- `BuildGridInteractionPreviewController.ApplyDragGhostArtwork`
- `BuildGridInteractionPreviewController.ApplyWholeItemArtworkTransform`
- `BuildGridInteractionPreviewController.ResolveWholeItemArtworkTrayRotationOffsetDegrees`
- `BuildGridInteractionPreviewController.ResolveWholeItemArtworkDragGhostRotationOffsetDegrees`
- `BuildGridInteractionPreviewController.ResolveWholeItemArtworkBoardPlacedRotationOffsetDegrees`
- `BuildGridInteractionPreviewController.ClampBoardAnchorForPayload`
- `ShapePlacementSession.ShapeItemPayload.ApplyRotation`
- `ItemShapePlacementValidator.ApplyRotation`
- `MobileShapePlacementInputExtension.ApplyRotation`
- `MobileShapePlacementRuntimeIntegration.ApplyRotation`
- `BuildItemPreviewCardView.ApplyArtworkImageRotationOffset`
- `BuildItemTrayPreviewView.ApplyItemArtworkRotationOffset`
- `BuildGridInteractionPreviewController.TryResolveDragGhostLayout`
- `ShapeCellVisualLayout.AnchoredPosition`

Current behavior:

- Card child Sprite artwork is preferred over generated layout-cell color blocks.
- Layout-cell Sprite/manual styles and occupied tray-slot Sprites are fallback sources.
- Whole-item Sprite artwork renders once through runtime-only `DragGhostArtwork` / `BoardItemArtworkLayer` overlays spanning the occupied-cell bounds.
- Whole-item Sprite direction is tunable in the `BuildGridInteractionPreviewController` Inspector under `V0.4 Artwork Direction Calibration`, with separate Tray / DragGhost / BoardPlaced rotation offsets. The source card Image rotation is preserved, the tray Image can receive its own runtime offset, and DragGhost/BoardPlaced overlays add their state offsets before applying the current `ItemShapeRotation`. Defaults keep the current pass behavior: 3-cell triangle/corner DragGhost/BoardPlaced use `+90`, other whole-item artwork uses `-90`, and Tray uses `0`.
- Physical occupied-cell rotation now treats `Rotation90` as clockwise in screen/board coordinates in the payload, validator, runtime integration, and controller helper paths. This keeps Corner3 at 3 occupied cells through all four rotations and matches the artwork rotation direction.
- Rotate-button board previews clamp the anchor against the rotated payload footprint before redraw, so a board-edge Corner3 preview does not render only the two in-board cells when the full three-cell footprint can be kept on the board.
- Board/ghost cells suppress item color blocks entirely when whole-item artwork exists; color blocks remain only as the no-image fallback.
- Rotation redraw keeps whole-item artwork even when the current preview is invalid/overlapping.
- Missing styles fall back to the previous color-only rendering.

## Validation At Checkpoint

- Targeted `git diff --check` for scoped files: PASS.
- Scene/prefab assets: not intentionally changed by this package.
- Unity Console/playmode validation: still requires manual Unity test.

## Recommended Recovery Procedure

If playtest fails, revert the smallest checkpoint above first instead of rolling back all mobile rotate-zone work. If a full rollback is required, restore only the files listed in `Files In Scope` from source control or ask Codex to reverse this package's current uncommitted diff for those files.
