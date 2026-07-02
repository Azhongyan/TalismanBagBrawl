# ShapePlacement Redesign Risk Report

Package: `V0.4-ShapePlacementSession01-TechSurvey`
Date: 2026-07-01
Status: `SURVEY_COMPLETE / FIX03_ROUTE_REJECTED / NO_RUNTIME_CHANGE`

## Risk Summary

The largest risk is continuing to patch around Fix03. Overlay visuals, `ignoreLayout`, delayed one-frame drop reads, mature single-slot drops, and separate anchor state create an unstable multi-authority system. Any additional offset correction is likely to move the failure rather than remove it.

The safer redesign is to introduce a dev-only `ShapePlacementSession` as the single state source, with `ShapeItemPayload` as the explicit payload and `ShapeGridReceiver` adapters for tray and board. Visuals should render from session state instead of deciding placement.

## Red / No-Go Risks

| Risk | Level | Trigger | Impact | Required Response |
|---|---|---|---|---|
| Continue Fix03 overlay route | Red | More `ignoreLayout`, overlay reparent, delayed one-frame reads, or offset patches | State conflicts remain; drag/drop results are non-deterministic | Stop and redesign through session |
| Formal board multi-cell commit | Red | Editing `TalismanBagGrid.PlaceItem`, save data, or formal board semantics in this package | Breaks protected battle/grid/save invariants | Defer to separate authorized package |
| Mature V03 UI rewrite | Red | Changing hand-tuned tray/board positions, pull-up motion, formal scene hierarchy, or mature layout constants | Reopens accepted BattlePrepare UX | Do not touch; use adapters |
| Drag snapback race | Red | Session result is inferred only after `DraggableTalismanItemView.OnEndDrag` | Item may return home before shape state commits | Session must own decision before or during drop/cancel |
| Cross-system scope leak | Red | Touching Boss, rewards, number balance, main flow, RunFlow, PageState, FormationState, or save | Violates locked project boundaries | Stop and request new authorization |

## Yellow Risks

| Risk | Level | Trigger | Impact | Mitigation |
|---|---|---|---|---|
| Receiver adapter drift | Yellow | Tray/board adapters duplicate mature assumptions silently | Future V03 layout changes can break shape placement | Enumerate mature slots at runtime and report missing cells |
| Rotation anchor ambiguity | Yellow | Rotated offsets can become negative or shift anchor meaning | Preview and commit may disagree | Normalize rotated occupied cells inside session/receiver before validation |
| Renderer mistaken as authority | Yellow | Visual component stores anchor/occupied cells as final state | Recreates Fix03 failure in a cleaner wrapper | Renderer receives immutable session snapshot only |
| Board preview mismatch | Yellow | Existing board items are single-cell while new shapes are multi-cell | Preview can overpromise future commit behavior | Mark board receiver preview-only until formal multi-cell grid package |
| Mobile confirm timing | Yellow | Mobile package starts before session contract exists | Confirm/cancel logic couples to old drag/drop internals | Sequence mobile package after session and tray receiver packages |

## Protected Boundaries

This survey does not authorize changes to:

- `TalismanBagGrid` formal placement semantics.
- `TalismanGridSlotView.OnDrop` formal board drop behavior.
- `DraggableTalismanItemView` core drag/snapback behavior.
- `V03BattlePrepareInteractionController` formal layout constants and accepted hand-tuned positions.
- V02/V03 formal scenes.
- RunFlow, PageState, FormationState, save data, Boss, rewards, numeric balance, damage display, item trigger logic, or formation power.

## Safe Redesign Conditions

A future implementation package is considered safe only if:

- `ShapePlacementSession` is the only authority for selected item, shape id, rotation, source container, active receiver, anchor cell, occupied cells, preview result, and commit/cancel state.
- `ShapeItemPayload` is created once at drag/select start and passed to receivers explicitly.
- Tray and board use the same `IShapeGridReceiver` contract: `ScreenPointToCell`, `CanPlace`, `Preview`, `Commit`, `Cancel`.
- Preview does not mutate formal placement data.
- Board commit is disabled, no-op, or dev-only until a separate formal multi-cell grid package is approved.
- Renderers draw from session snapshots and do not write placement state.
- No delayed post-drop inference is used as the authoritative commit path.

## Follow-Up Split

1. `V0.4-ShapePlacementSession01`
   - Add pure BuildSandbox/dev-only session, payload, receiver interface, and validator bridge.
   - Include unit-level or report-level validation of rotated offsets, anchor normalization, and receiver result consistency.

2. `V0.4-ShapeAwareItemTrayGrid01`
   - Add tray receiver adapter over mature V03 tray slots.
   - Replace overlay-as-authority with session-driven tray occupancy and renderer.
   - Keep all work dev-only and out of formal save/UI scene changes.

3. `V0.4-MobileShapePlacementInteraction01`
   - Implement select, rotate, drag ghost, `PreviewLocked`, confirm, and cancel through the session.
   - Start only after the session and tray receiver have a stable contract.

## Final Gate

The redesign can proceed technically. It should not proceed by continuing Fix03. The next package should be contract-first and dev-only, with the explicit goal of removing multiple placement authorities before any mobile interaction or formal board commit is attempted.
