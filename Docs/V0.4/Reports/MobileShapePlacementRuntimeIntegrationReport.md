# Mobile Shape Placement Runtime Integration Report

Package: `V0.4-MobileShapePlacementRuntimeIntegrationFix01`
Generated: `2026-07-02 00:13:49`
Status: `PASS_RUNTIMEPLAYTEST_MOBILE_SHAPE_PLACEMENT_CONNECTED`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## RuntimePlaytest Wiring

| Check | Value |
| --- | --- |
| `runtimePlaytestConnected` | `True` |
| `usesShapePlacementSession` | `True` |
| `usesShapeAwareItemTrayGrid` | `True` |
| `usesMobileShapePlacementInputExtension` | `True` |
| `usesSessionAuthority` | `True` |
| `usesBattlePrepareShapePlacementSeamAdapter` | `True` |
| `bindsBattlePrepareShapePlacementSeamCallbacks` | `True` |
| `seamAdapterConsumesPreviewGhost` | `True` |
| `usesDirectExternalItemHooks` | `False` |
| source path | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs` |

## User Flow Coverage

| Sample | Action | From | To | Anchor | Cells | Ghost | Release Submitted | Commit Count | Note |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `runtime_connected` | `BindRuntimePlaytest` | `Idle` | `Idle` | `` | `` | `Hidden` | `False` | `0->0` | RuntimePlaytest bind uses ShapePlacementSession, ShapeAwareItemTrayGrid, and MobileShapePlacementInputExtension. |
| `tap_select_holding` | `TapTrayItem` | `Idle` | `HoldingItem` | `` | `` | `Hidden` | `False` | `0->0` | Clicking a tray item starts the session with the tray grid anchor. |
| `tap_selected_rotates` | `TapSelectedItemToRotate` | `HoldingItem` | `RotateHoldingItem` | `` | `` | `Hidden` | `False` | `0->0` | Clicking the already selected shape rotates through the session authority. |
| `drag_generates_ghost` | `DragToReceiver` | `RotateHoldingItem` | `DraggingPreview` | `2,2` | `2,2;3,2` | `Valid` | `False` | `0->0` | Dragging over the board generates a ghost preview without committing. |
| `release_locks_preview_without_commit` | `ReleaseDragLockPreview` | `DraggingPreview` | `PreviewLocked` | `2,2` | `2,2;3,2` | `Locked` | `False` | `0->0` | Release locks the ghost and leaves receiver commit count unchanged. |
| `drag_locked_ghost` | `DragLockedGhostToReceiver` | `PreviewLocked` | `PreviewLocked` | `3,2` | `3,2;4,2` | `Locked` | `False` | `0->0` | Dragging a locked ghost moves the preview and relocks it. |
| `tap_ghost_commits` | `TapGhostToConfirm` | `PreviewLocked` | `Placed` | `3,2` | `3,2;4,2` | `Confirmed` | `False` | `0->1` | Clicking the locked ghost is the only commit path. |
| `invalid_position_red_feedback` | `DragInvalidPosition` | `HoldingItem` | `InvalidPreview` | `7,7` | `7,7;8,7;7,8;8,8` | `Invalid` | `False` | `1->1` | Invalid preview stays red, cannot lock, and does not commit. |
| `cancel_clears_session` | `Cancel` | `HoldingItem` | `Cancelled` | `` | `` | `Hidden` | `False` | `1->1` | Cancel clears the visible ghost and calls receiver cancel. |

## Source Guard

| Check | Value |
| --- | --- |
| RuntimePlaytest owns integration | `True` |
| RuntimePlaytest owns board receiver | `True` |
| RuntimePlaytest owns ghost target | `True` |
| Calls `TapTrayItem` | `True` |
| Calls `ReleaseDragLockPreview` | `True` |
| Calls `TapGhostToConfirm` | `True` |
| Calls `Cancel` | `True` |
| Injects `BattlePrepareShapePlacementSeamAdapter` | `True` |
| Binds seam callbacks | `True` |
| Adapter consumes `PreviewGhost` | `True` |
| RuntimePlaytest direct item hooks | `False` |
| Restores old Fix03 route | `False` |

## Notes

- RuntimePlaytest now enters placement through `BattlePrepareShapePlacementSeamAdapter` and then drives the shared session/input/tray-grid protocol.
- Release locks the ghost preview; only clicking the locked ghost commits to the devOnly receiver.
- Commit remains runtime-only and does not write formal RunFlow, SaveData, Boss, reward, drop, numeric, scene, or UI assets.
- Direct RuntimePlaytest item-event hooks and the rejected overlay / layout-ignore / delayed drop route are guarded as absent.

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
