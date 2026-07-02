# Shape-Aware ItemTray Grid Leak Check Report

Package: `V0.4-ShapeAwareItemTrayGrid01`
Generated: `2026-07-01 19:45:24`
Status: `PASS_DEVONLY_ISOLATED`
Errors: `0`
Warnings: `0`

## Leak Counters

| Check | Count | Expected |
| --- | ---: | ---: |
| `featureFlagDefaultTrue` | 0 | 0 |
| `fix03RouteLeaks` | 0 | 0 |
| `uiRewriteLeaks` | 0 | 0 |
| `formalSystemLeaks` | 0 | 0 |
| `totalLeaks` | 0 | 0 |

## Scope Confirmation

- Feature flags remain default false.
- The package remains BuildSandbox/devOnly and disabled by default.
- No formal scenes, hand-tuned RectTransforms, RunFlow, PageState, FormationState, SaveData, Boss, rewards, drops, or numeric config are touched.
- No overlay, `ignoreLayout`, or delayed one-frame drop read is used as placement authority.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| Shape-Aware ItemTray Grid | `PASS` | 0 | 0 | 39 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `SHAPE_ITEM_TRAY_GRID_DEVONLY` | Grid remains BuildSandbox/devOnly. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_ENABLED_TRUE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_FEATURE_FLAG_TRUE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_FIX03_CONTINUED` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_IGNORE_LAYOUT` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_DELAYED_DROP_READ` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_REDRAWS_ITEM_TRAY` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_REPLACES_MATURE_TRAY` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_WRITES_FORMAL_SCENE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_WRITES_FORMAL_UI` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_RUNFLOW` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_PAGESTATE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_FORMATIONSTATE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_SAVE` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_TOUCHES_BOSS_REWARD_NUMERIC` | Isolation flag remains false. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_single1. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_vertical2. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_corner3. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: pack_square4. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: bounds_rejects_square4. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: overlap_rejects_corner3. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: illegal_commit_keeps_stable_anchor. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: legal_move_updates_stable_anchor. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: rotation_normalizes_corner3. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PRESENT` | Required sample present: screen_point_to_cell. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_single1 action=Pack, valid=True, anchor=0,0, cells=0,0, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_vertical2 action=Pack, valid=True, anchor=1,0, cells=1,0;1,1, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_corner3 action=Pack, valid=True, anchor=2,0, cells=2,0;3,0;2,1, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | pack_square4 action=Pack, valid=True, anchor=3,1, cells=3,1;4,1;3,2;4,2, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | bounds_rejects_square4 action=CanPlace, valid=False, anchor=4,7, cells=4,7;5,7;4,8;5,8, reason=OutOfGrid. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | overlap_rejects_corner3 action=CanPlace, valid=False, anchor=0,0, cells=0,0;1,0;0,1, reason=CellOccupied. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | illegal_commit_keeps_stable_anchor action=Preview+Commit, valid=False, anchor=4,7, cells=4,7;5,7;4,8;5,8, reason=OutOfGrid. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | legal_move_updates_stable_anchor action=Preview+Commit, valid=True, anchor=0,2, cells=0,2;1,2;0,3;1,3, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | rotation_normalizes_corner3 action=CanPlace, valid=True, anchor=0,0, cells=0,0;0,1;1,1, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SAMPLE_PASS` | screen_point_to_cell action=ScreenPointToCell, valid=True, anchor=2,3, cells=, reason=None. | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Single1 packs as 1 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Vertical2 packs as 2 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Corner3 packs as 3 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
| `Info` | `SHAPE_ITEM_TRAY_GRID_SHAPE_PACKED` | Shape Square4 packs as 4 real tray cell(s). | `V0.4-ShapeAwareItemTrayGrid01` |
