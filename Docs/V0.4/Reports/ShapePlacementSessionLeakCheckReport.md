# ShapePlacementSession Leak Check Report

Package: `V0.4-ShapePlacementSession01`
Generated: `2026-07-01 19:29:30`
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
- The package does not touch formal RunFlow, PageState, FormationState, SaveData, PlayerPrefs, Boss, rewards, drops, numeric config, DamageText, or V02FormationGridFrame.
- The package does not modify formal V02/V03 scene layout or hand-tuned RectTransform values.
- Board commit remains disabled in the protocol sample; formal multi-cell commit is deferred to a separately authorized package.

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| ShapePlacementSession Protocol | `PASS` | 0 | 0 | 29 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `SHAPE_PLACEMENT_SESSION_DEVONLY` | Protocol remains devOnly. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_ENABLED_TRUE` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_FEATURE_FLAG_TRUE` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_FIX03_CONTINUED` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_IGNORE_LAYOUT` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_DELAYED_DROP_READ` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_REDRAWS_ITEM_TRAY` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_REDRAWS_BOARD` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_WRITES_FORMAL_SCENE` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_WRITES_FORMAL_UI` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_TOUCHES_RUNFLOW` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_TOUCHES_PAGESTATE` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_TOUCHES_FORMATIONSTATE` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_TOUCHES_SAVE` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_TOUCHES_BOSS_REWARD_NUMERIC` | Isolation flag remains false. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PRESENT` | Required sample present: can_place_side_effect_light. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PRESENT` | Required sample present: tray_preview_updates_session. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PRESENT` | Required sample present: tray_commit_mutates_receiver. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PRESENT` | Required sample present: board_commit_disabled. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PRESENT` | Required sample present: cancel_clears_preview. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PRESENT` | Required sample present: rotation_preview_cells. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | can_place_side_effect_light action=CanPlace, valid=True, state=HoldingItem, reason=None. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | tray_preview_updates_session action=Preview, valid=True, state=Previewing, reason=None. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | tray_commit_mutates_receiver action=Commit, valid=True, state=Committed, reason=None. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | board_commit_disabled action=Commit, valid=False, state=InvalidPreview, reason=CommitDisabled. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | cancel_clears_preview action=Cancel, valid=False, state=Cancelled, reason=OutOfGrid. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | rotation_preview_cells action=Preview, valid=True, state=Previewing, reason=None. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | tray_overlap_rejected action=Preview, valid=False, state=InvalidPreview, reason=CellOccupied. | `V0.4-ShapePlacementSession01` |
| `Info` | `SHAPE_PLACEMENT_SESSION_SAMPLE_PASS` | screen_point_to_cell action=ScreenPointToCell, valid=True, state=Idle, reason=None. | `V0.4-ShapePlacementSession01` |
