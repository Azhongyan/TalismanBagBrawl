# Mobile Shape Placement Interaction Report

Package: `V0.4-MobileShapePlacementInteraction01`
Generated: `2026-07-01 21:41:29`
Status: `PASS_DEVONLY_MOBILE_PLACEMENT_READY`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- Adds a BuildSandbox/devOnly mobile placement input protocol.
- Reuses `ShapePlacementSession`, `ShapeItemPayload`, `ShapeGridReceiver`, and `ShapeAwareItemTrayGrid` direction as placement authority.
- Models tap select, tap rotate, drag ghost preview, release-to-lock preview, tap ghost confirm, invalid feedback, and cancel.
- Does not redraw the mature BattlePrepare board, item tray, drag feel, or pull-up animation.
- Does not write formal SaveData, MainTrialProgressData, PlayerPrefs, RunFlow, Boss, reward, drop, or numeric systems.

## Input Thresholds

| Setting | Value | Required |
| --- | ---: | ---: |
| `tapMoveThresholdPixels` | 15 | 15 |
| `tapTimeThresholdMilliseconds` | 250 | 250 |
| `fingerGhostOffsetPixels` | 50 | 40-60 |
| `tapGestureSample` | `Tap` | `Tap` |
| `dragGestureSample` | `Drag` | `Drag` |

## Protocol Coverage

| Check | Result |
| --- | --- |
| `PreviewLocked` implemented | `True` |
| release does not commit | `True` |
| tap ghost commits | `True` |
| cancel available | `True` |
| PlacedItemEdit | `DEFERRED` |

## State Machine Samples

| Sample | Action | From | To | Anchor | Cells | Ghost | Hint | Release Submitted | Commit Count | Note |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `tap_select_holding` | `TapTrayItem` | `Idle` | `HoldingItem` | `` | `` | `Hidden` |  | `False` | `0->0` | First tap selects the tray item and enters HoldingItem. |
| `tap_selected_rotates` | `TapSelectedItemToRotate` | `HoldingItem` | `RotateHoldingItem` | `` | `` | `Hidden` |  | `False` | `0->0` | Second tap rotates the selected shape through data-driven occupied offsets. |
| `drag_generates_ghost` | `DragToReceiver` | `RotateHoldingItem` | `DraggingPreview` | `2,2` | `2,2;3,2` | `Valid` | 再次点击放下 | `False` | `0->0` | Drag over the board produces a ghost preview and does not commit. |
| `release_locks_preview_without_commit` | `ReleaseDragLockPreview` | `DraggingPreview` | `PreviewLocked` | `2,2` | `2,2;3,2` | `Locked` | 再次点击放下 | `False` | `0->0` | Release keeps the ghost visible and locked without mutating receiver placement. |
| `tap_ghost_commits` | `TapGhostToConfirm` | `PreviewLocked` | `Placed` | `2,2` | `2,2;3,2` | `Confirmed` | 已放下 | `False` | `0->1` | Clicking the locked ghost is the only sample path that commits placement. |
| `invalid_position_red_feedback` | `DragInvalidPosition` | `HoldingItem` | `InvalidPreview` | `7,7` | `7,7;8,7;7,8;8,8` | `Invalid` | 当前位置无法放置 | `False` | `1->1` | Out-of-grid preview stays invalid, shows red feedback, and cannot lock or commit. |
| `cancel_clears_session` | `Cancel` | `HoldingItem` | `Cancelled` | `` | `` | `Hidden` | 已取消 | `False` | `1->1` | Cancel button clears the active preview without writing placement state. |
| `single_shape_no_rotation_hint` | `TapSelectedItemToRotate` | `HoldingItem` | `HoldingItem` | `` | `` | `Hidden` |  | `False` | `1->1` | Single-cell shapes are data-detected as non-rotating and return a light hint. |

## Shape Support

| Shape | Cells | Unique Rotations | Can Rotate | Offsets Source |
| --- | ---: | ---: | --- | --- |
| `Single1` | 1 | 1 | `False` | `0,0` |
| `Vertical2` | 2 | 2 | `True` | `0,0;0,1` |
| `Corner3` | 3 | 4 | `True` | `0,0;1,0;0,1` |
| `Square4` | 4 | 1 | `False` | `0,0;1,0;0,1;1,1` |

## Reuse / Isolation

- Mature BattlePrepare reused as base component: `True`.
- Redraws board: `False`.
- Redraws item tray: `False`.
- Rewrites drag feel: `False`.
- Rewrites pull-up animation: `False`.
- Feature flags default true: `False`.
- Preview stage only updates ghost state, hints, rotation preview, and occupied-cell preview.
- Confirm stage commits only to the devOnly in-memory receiver sample.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Mobile Shape Placement Interaction | `PASS` | 0 | 0 | 45 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `MOBILE_SHAPE_PLACEMENT_DEVONLY` | Mobile placement remains BuildSandbox/devOnly. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_ENABLED_TRUE` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_FEATURE_FLAG_TRUE` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_REUSES_MATURE_BATTLEPREPARE` | Mature BattlePrepare remains the base component. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_REDRAWS_BOARD` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_REDRAWS_ITEM_TRAY` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_REWRITES_DRAG_FEEL` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_REWRITES_PULLUP` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_WRITES_FORMAL_SCENE` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_WRITES_FORMAL_UI` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_OVERWRITES_RECT` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TOUCHES_RUNFLOW` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TOUCHES_PAGESTATE` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TOUCHES_FORMATIONSTATE` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TOUCHES_SAVE` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TOUCHES_BOSS_REWARD_NUMERIC` | Isolation flag remains false. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TAP_DISTANCE_THRESHOLD` | Tap distance threshold is 15px. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TAP_TIME_THRESHOLD` | Tap time threshold is 250ms. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TAP_CLASSIFICATION` | Tap sample classified correctly. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_DRAG_CLASSIFICATION` | Drag sample classified correctly. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: tap_select_holding. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: tap_selected_rotates. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: drag_generates_ghost. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: release_locks_preview_without_commit. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: tap_ghost_commits. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: invalid_position_red_feedback. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: cancel_clears_session. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PRESENT` | Required sample present: single_shape_no_rotation_hint. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | tap_select_holding Idle->HoldingItem action=TapTrayItem, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | tap_selected_rotates HoldingItem->RotateHoldingItem action=TapSelectedItemToRotate, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | drag_generates_ghost RotateHoldingItem->DraggingPreview action=DragToReceiver, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | release_locks_preview_without_commit DraggingPreview->PreviewLocked action=ReleaseDragLockPreview, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | tap_ghost_commits PreviewLocked->Placed action=TapGhostToConfirm, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | invalid_position_red_feedback HoldingItem->InvalidPreview action=DragInvalidPosition, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | cancel_clears_session HoldingItem->Cancelled action=Cancel, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SAMPLE_PASS` | single_shape_no_rotation_hint HoldingItem->HoldingItem action=TapSelectedItemToRotate, valid=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SHAPE_PASS` | Single1 cells=1, uniqueRotations=1, canRotate=False. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SHAPE_PASS` | Vertical2 cells=2, uniqueRotations=2, canRotate=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SHAPE_PASS` | Corner3 cells=3, uniqueRotations=4, canRotate=True. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_SHAPE_PASS` | Square4 cells=4, uniqueRotations=1, canRotate=False. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_PREVIEW_LOCKED` | PreviewLocked state is covered. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_RELEASE_NO_COMMIT` | Release does not commit. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_TAP_GHOST_COMMITS` | Tap ghost commits. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_CANCEL_AVAILABLE` | Cancel operation is available. | `V0.4-MobileShapePlacementInteraction01` |
| `Info` | `MOBILE_SHAPE_PLACEMENT_PLACED_EDIT_DEFERRED` | PlacedItemEdit remains deferred. | `V0.4-MobileShapePlacementInteraction01` |
