# Battle Sandbox Shape Placement Vertical Slice Report

Package: `V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01`
Generated: `2026-07-02`
Status: `STATIC_READY_UNITY_BATCH_STARTUP_HUNG_NO_LOG`
Errors: `0 static`
Warnings: `Unity batch process started but produced no log and did not exit; the Codex-started batch process was stopped.`
Leak Count: `0 static`

## Scope

- Target scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.
- Preview tray now exposes 9 test items: the primary x2 item plus single-cell, vertical-2, corner-3, and square-4 variants.
- Non-Play editor mode now auto-populates visible `ItemCard` previews for manual UI sizing without entering Play.
- `TrayGridSlot` and `ItemCard` are separated: slots represent tray constraints/empty cells, while `ItemCardLayer` contains only real preview items for manual item sizing.
- Tray layout authority is now split explicitly: `TrayPlacementViewModel` carries `ShapeAwareItemTrayGrid` placement output, `TrayItemLayoutView` is the only class that sets tray `ItemCard` rects, and `TrayGridReservationView` is the only class that refreshes tray slot reservation visuals.
- Single click on `护阵木牌 / Vertical2` opens or refreshes item info only.
- Rotation is tray-only through the item info popup `RotateButton`; the item card itself does not show a Rotate button.
- `Vertical2` is rendered in the tray as a real vertical two-cell span by default, rotates to a horizontal two-cell span in tray, and rotates back to vertical on the next popup Rotate.
- The tray now reserves the secondary occupied slot visually, hides the empty card in that slot, and draws the spanning item card above slot backgrounds.
- The item info popup creates a header-level `RotateButton`; clicking the item body only opens or refreshes that popup.
- Popup Rotate rejects out-of-bounds or overlapping rotations and keeps the previous direction.
- Dragging the item starts the V0.4 `ShapePlacementSession`; dragging/previewing cannot rotate.
- Valid release commits directly to the board; invalid release cancels the drag and returns the item to tray.
- No Ghost click confirm requirement remains.
- No formal V02/V03 scene, RunFlow, save, Boss, reward, drop, numeric, or formal BattlePrepare runtime core is connected by this fix.

## Primary X2 Slice

| Field | Value |
| --- | --- |
| Item count | `9` |
| Item id | `preview_x2_wood_talisman` |
| Display name | `护阵木牌` |
| Shape id | `Vertical2` |
| Shape cell count | `2` |
| Default tray visual | `vertical two-cell span` |
| Default occupied tray cells | `0,0;0,1` |
| Default occupied tray slots | `0;5` |
| Rotate visual | `horizontal two-cell span` |
| Rotated occupied tray cells | `0,0;1,0` |
| Rotated occupied tray slots | `0;1` |

## Protocol Summary

| Check | Value |
| --- | --- |
| Click opens item info only | `True` |
| Item card body click rotates | `False` |
| Item info popup Rotate button exists | `True` |
| Item card Rotate button visible | `False` |
| Secondary occupied tray slot reserved visually | `True` |
| Popup Rotate only | `True` |
| No rotation while dragging | `True` |
| No rotation on board | `True` |
| Drag starts placement | `True` |
| Release commits if valid | `True` |
| Release returns to tray if invalid | `True` |
| No Ghost click confirm | `True` |
| Cancel available | `True` |
| Uses ShapePlacementSession | `True` |
| Uses ShapeAwareItemTrayGrid | `True` |
| Uses MobileShapePlacementInputExtension | `True` |
| Uses formal V02/V03 runtime core | `False` |

## Verification Notes

- Static source scan found no `ReleaseDragLockPreview`, `TapGhostToConfirm`, `TapSelectedItemToRotate`, or `AddListener(RotateSelectedItem)` call in `BuildGridInteractionPreviewController`.
- Static source scan found `BuildSandboxItemInfoPanel` creates `RotateButton` and routes clicks through the controller `SetRotateHandler`.
- Static source scan found `BuildItemPreviewCardView` keeps body click info-only and removes any legacy `TrayRotateButton` child if one exists.
- Static source scan found `BuildItemTrayPreviewView` tracks reserved visual tray slots through `visualSlotOwnersByIndex` and refreshes them on filter/rotation.
- Static source scan found `BuildItemTrayPreviewView` uses editor-only delayed refresh to populate preview cards only when no visible preview content exists.
- Static source scan found `BuildItemTrayPreviewView` tracks tray slot visuals separately from item cards, and editor migration removes legacy per-slot `ItemCard` children.
- Static source scan found `BuildItemPreviewCardView` no longer contains tray slot index, tray footprint layout, or reserved-slot coloring logic.
- Static source scan found `BuildGridInteractionPreviewController` refreshes tray UI through `TrayPlacementViewModel` sourced from `ShapeAwareItemTrayGridPlacement`.
- Static source scan found `BuildGridInteractionPreviewSceneBinder` no longer creates `TrayRotateButton` under rebuilt `ItemCard`.
- V04 preview scene/runtime/binder text was updated to describe popup Rotate and direct release placement.
- Unity batch validation was attempted through `Start-Process -Wait -WindowStyle Hidden`, but Unity stayed in startup without creating `Logs/codex_v04_shape_fix_simplified_tray_rotate_validator.log`; the spawned process was stopped.
