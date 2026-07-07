# Mobile Rotate Zone Interaction Ops Rollback Report

Package: `V0.4-MobileRotateZoneInteraction01`
Date: `2026-07-07`
Status: `WORKTREE_ONLY / NO_COMMIT`

## Purpose

Record the current mobile rotate-zone handfeel changes so they can be reviewed or rolled back safely if Unity playtest shows regression.

No scene, prefab, binder run, commit, tag, or push was performed for this checkpoint.

## Files In Scope

- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionReport.md`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionStateReport.csv`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionLeakCheckReport.md`
- `Docs/V0.4/Reports/MobileRotateZoneInteractionOpsRollbackReport.md`

## Current Diff Snapshot

Targeted diff stat at this checkpoint:

- `BuildGridInteractionPreviewController.cs`: runtime handfeel logic and runtime-only UI visuals.
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
- `ResolveRotateZoneVisualSide`

Expected rollback result:

- Buttons return to simple idle/active color.
- No guide band appears.
- RotateSeek and drag smoothness behavior remains.

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

Use if rotate buttons feel too easy to hit during movement.

Current constants:

- `RotateButtonVisualSizePixels = 72`
- `RotateButtonActivationWidthPixels = 136`
- `RotateButtonActivationHeightPixels = 180`
- `RotateSeekWindowSeconds = 0.28`
- `RotateSeekMinDeltaXPixels = 20`
- `RotateSeekMaxDeltaYPixels = 76`
- `RotateSeekMinVelocityXPixelsPerSecond = 300`

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

## Validation At Checkpoint

- Targeted `git diff --check` for scoped files: PASS.
- Scene/prefab assets: not intentionally changed by this package.
- Unity Console/playmode validation: still requires manual Unity test.

## Recommended Recovery Procedure

If playtest fails, revert the smallest checkpoint above first instead of rolling back all mobile rotate-zone work. If a full rollback is required, restore only the files listed in `Files In Scope` from source control or ask Codex to reverse this package's current uncommitted diff for those files.
