# BattleSandboxItemTriggerFeedbackFx Report

Package: `V0.4-BattleSandboxItemTriggerFeedbackFx01`
Date: `2026-07-08`

## Scope

- Target scene: `Scene_TalismanBag_V04_BattleSandboxPreview`
- Runtime scope: V0.4 BuildSandbox / devOnly battle sandbox only
- Formal V0.2/V0.3 combat, RunFlow, PageState, FormationState, save data, reward, drop, Boss reward, and BuildSettings were not modified.

## Implementation

- `BattleSandboxRuntimeLoopRow` carries a board-local trigger feedback payload:
  - `playsBoardItemTriggerFeedback`
  - `boardItemTriggerFeedbackChannel`
  - `boardItemTriggerFeedbackKind`
  - `boardItemTriggerFeedbackTextChinese`
  - `boardItemTriggerFeedbackValue`
  - `boardItemTriggerOccupiedCells`
- Feedback channel semantics:
  - `active`: active item casts and direct effects. Plays every trigger with stronger pulse, flash, VFX, and floating text.
  - `passive`: passive item/build modifier effects. Plays through lower-frequency, lighter visuals.
- `BattleSandboxRuntimeLoopRuntime` binds a runtime-only `BattleSandboxItemTriggerFeedbackController` and plays board-local feedback for the current runtime row.
- `BuildGridInteractionPreviewController.TryResolveBoardItemFeedbackAnchor(...)` resolves a placed item feedback anchor without rewriting scene layout:
  - prefers the whole placed item artwork `RectTransform`
  - falls back to the occupied board-cell layout center
- `BattleSandboxItemTriggerFeedbackController` dynamically creates short-lived UI feedback objects:
  - item scale pulse
  - flash overlay
  - procedural skill VFX pulse
  - colored floating text above the item
- Build modifier passive rows are sampled at a lower step frequency, then throttled by global and per-item intervals so passive effects do not steal active skill readability.
- BuildSandbox `modifierBundle` is read as devOnly attribution data only. Modifier sources now produce passive board feedback such as `灵力加速 +6%`, without changing mana tick or formal combat settlement.

## Effect Coverage

- active damage: `-30 灼热伤害` / `-30 雷击伤害`
- active shield break: `破盾 +17`
- active shield gain: `+50 护盾`
- active cleanse: `净化 +8`
- active control: `镇压 +11`
- passive mana gain: `+12 回灵`
- passive build modifier: `灵力加速 +6%`, `伤害强化 +10%`, `冷却加速 +10%`
- not powered: `未供能`
- weak pulse: `弱脉冲触发`
- suppressed: `被压制`
- mana shortage: `灵力不足`

## Notes

- Current VFX is a procedural UI placeholder, not imported sprite-sheet animation.
- Art can later replace the procedural VFX through a `kind -> prefab` mapping without changing combat settlement.
- Old middle `EnemyCombatFeedbackFloatingRoot` floating text can remain hidden for this sandbox so board-local item feedback is easier to judge.
