# Mobile Shape Placement Runtime Integration Leak Check Report

Package: `V0.4-MobileShapePlacementRuntimeIntegrationFix01`
Generated: `2026-07-02 00:13:49`
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
| `directExternalItemEventHooks` | 0 | 0 |
| `oldFix03RouteRestored` | 0 | 0 |
| `totalLeaks` | 0 | 0 |

## Boundary Confirmation

- No mature board, mature item tray, or pull-up animation redraw is introduced.
- No user-tuned RectTransform is overwritten.
- No formal RunFlow, SaveData, Boss, reward, drop, numeric, or scene asset is written.
- The old overlay + layout-ignore + delayed drop route remains absent.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Mobile Shape Placement Runtime Integration | `PASS` | 0 | 0 | 48 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_DEVONLY` | Runtime integration remains devOnly. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_ENABLED_TRUE` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_FEATURE_FLAG_TRUE` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_REDRAWS_BOARD` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_REDRAWS_TRAY` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_REWRITES_PULLUP` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_OVERWRITES_RECT` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_TOUCHES_RUNFLOW` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_TOUCHES_SAVE` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_TOUCHES_BOSS_REWARD_NUMERIC` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_CONNECTED` | RuntimePlaytest bind is connected. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_USES_SESSION` | ShapePlacementSession is present. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_USES_TRAY_GRID` | ShapeAwareItemTrayGrid is present. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_USES_INPUT_EXTENSION` | Mobile input extension is present. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SESSION_AUTHORITY` | Input extension uses the runtime session. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_RELEASE_NO_COMMIT` | Release locks without commit. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_TAP_GHOST_COMMITS` | Tap ghost commits. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_CANCEL_AVAILABLE` | Cancel is available. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: runtime_connected. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: tap_select_holding. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: tap_selected_rotates. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: drag_generates_ghost. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: release_locks_preview_without_commit. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: drag_locked_ghost. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: tap_ghost_commits. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: invalid_position_red_feedback. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PRESENT` | Required sample present: cancel_clears_session. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | runtime_connected Idle->Idle action=BindRuntimePlaytest. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | tap_select_holding Idle->HoldingItem action=TapTrayItem. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | tap_selected_rotates HoldingItem->RotateHoldingItem action=TapSelectedItemToRotate. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | drag_generates_ghost RotateHoldingItem->DraggingPreview action=DragToReceiver. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | release_locks_preview_without_commit DraggingPreview->PreviewLocked action=ReleaseDragLockPreview. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | drag_locked_ghost PreviewLocked->PreviewLocked action=DragLockedGhostToReceiver. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | tap_ghost_commits PreviewLocked->Placed action=TapGhostToConfirm. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | invalid_position_red_feedback HoldingItem->InvalidPreview action=DragInvalidPosition. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_INTEGRATION_SAMPLE_PASS` | cancel_clears_session HoldingItem->Cancelled action=Cancel. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_HAS_INTEGRATION` | RuntimePlaytest owns MobileShapePlacementRuntimeIntegration. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_HAS_RECEIVER` | RuntimePlaytest owns runtime board receiver. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_HAS_GHOST_TARGET` | RuntimePlaytest ghost target handles drop/click. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_HAS_TAP_TRAY` | RuntimePlaytest calls TapTrayItem. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_HAS_RELEASE_LOCK` | RuntimePlaytest calls ReleaseDragLockPreview. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_HAS_TAP_GHOST` | RuntimePlaytest calls TapGhostToConfirm. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_HAS_CANCEL` | RuntimePlaytest calls Cancel. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_USES_BATTLEPREPARE_SEAM` | RuntimePlaytest injects BattlePrepareShapePlacementSeamAdapter. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_BINDS_SEAM_CALLBACKS` | RuntimePlaytest binds seam callback handlers. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_SEAM_PREVIEW_CALLBACK` | BattlePrepareShapePlacementSeamAdapter consumes PreviewGhost. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_DIRECT_ITEM_EVENT_HOOKS` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
| `Info` | `MOBILE_RUNTIME_SOURCE_RESTORES_FIX03_ROUTE` | Expected false and remained false. | `V0.4-MobileShapePlacementRuntimeIntegrationFix01` |
