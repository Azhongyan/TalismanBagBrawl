# Mobile Shape Placement Leak Check Report

Package: `V0.4-MobileShapePlacementInteraction01`
Generated: `2026-07-01 21:41:29`
Status: `PASS_DEVONLY_ISOLATED`
Errors: `0`
Warnings: `0`

## Leak Counters

| Check | Count | Expected |
| --- | ---: | ---: |
| `featureFlagDefaultTrue` | 0 | 0 |
| `uiRewriteLeaks` | 0 | 0 |
| `formalSystemLeaks` | 0 | 0 |
| `devOnlyLeaks` | 0 | 0 |
| `totalLeaks` | 0 | 0 |

## Scope Confirmation

- The package remains BuildSandbox/devOnly and disabled by default.
- No formal V02/V03 scenes, hand-tuned RectTransform values, mature BattlePrepare layouts, or Build Settings are written.
- No formal RunFlow, PageState, FormationState, V02RunFlowController, V02FormationGridFrame, DamageText, SaveData, Boss, reward, drop, or numeric systems are touched.
- The old Fix03 overlay / ignoreLayout / delayed one-frame drop route is not used as placement authority.
- PlacedItemEdit is intentionally deferred instead of half-implemented.

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
