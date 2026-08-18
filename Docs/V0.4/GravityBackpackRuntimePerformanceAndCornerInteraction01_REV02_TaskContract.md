# V0.4-GravityBackpackRuntimePerformanceAndCornerInteraction01 REV02 Task Contract

Status: `FROZEN_FOR_SAME_VISIBLE_TASK_LIGHT_FIX`

## User handtest failure

The performance direction is not reopened. The normal-Editor handtest found two
remaining product input failures:

1. both three-cell corner items still cannot be reliably selected or dragged;
2. an item placed on Board cannot be deliberately dragged back into the tabletop
   backpack.

The previous offline test proved only contour mathematics and direct resolver calls.
It did not prove the actual Unity EventSystem card-root route, the Board slot drag
route, or the Board-to-tabletop drop transaction. Do not claim product PASS from the
same pure hit-test fixture again.

## Classification and owner

- `CONTAINED_ONE_GUARD / SAME_PACKAGE_LIGHT_FIX_REV02`
- Primary Guard: Item Guard
- Development: continue visible task `019fb7c3-301d-7bb3-8839-2ddf5a8bad94`
- No hidden/archived/new task; no Unity/Fresh Play/batch/monitoring
- User owns the only normal-Editor Play verification

## Frozen product behavior

### A. Corner-item pointer acquisition

1. The card root must remain the single drag event owner in planar mode.
2. Both three-cell corner items must begin item drag from every visibly occupied arm.
3. Pointer accessibility may use a small bounded contour expansion in local/screen
   space, but it must not become a rectangular card blocker and must not fill the
   missing L-corner.
4. Transparent missing-corner input must continue to the visible item underneath.
5. A prepared contour/input mismatch must not silently disable every raycast Graphic
   on an otherwise visible card. It must fail with one bounded diagnostic and keep a
   safe shape-aware input owner; no item-ID special case.
6. Physics collision contour and visual scale remain unchanged. This revision changes
   pointer acquisition only where necessary.

### B. Board-to-tabletop return

1. A placed Board item must begin drag from any of its occupied Board cells. Board
   artwork remains presentation-only and must not steal the Board slot input event.
2. A deliberate Board-to-tabletop drop uses the actual pointer drop position (or a
   bounded nearest legal position around it). It must not depend solely on the top
   acquisition entrance being clear.
3. The top entrance remains the semantic route for newly acquired/dropped rewards;
   this revision changes only a user-driven return from Board.
4. Return is atomic: ItemSystem Board removal and tabletop insertion either both
   commit or both remain unchanged. A full/no-legal-position result rejects visibly
   and leaves the item on Board.
5. Board-to-Board move and tabletop-to-Board placement remain unchanged.
6. `battlePrepareStateActive` remains the edit gate. The fix must not enable dragging
   during active combat.

## Allowed writes and REV02 baselines

Prefer four files or fewer; use the conditional input-handler files only if the
first-missing EventSystem layer proves they are required.

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `8672A5A1CC0D65A75579B1F08407918503684EB456383BD240EB57F16DF96D29` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | `448A69762EC157214809E1D43CD19A134D77F7928742CC890A94A41B1588D56E` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `93B5CED83CB6E6AC26F9C70693F3C2B01EB545196891E5B2A62DAC9FE6DF65FF` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` | `D232A34F6A761D745564C5015C977F9EEF26E2C0D23CD764B6CFA2704D008214` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs` | `E65DF6E086CCAD62E6F4FA4C3B609965A52E3A3C5C39504263440661A56EABC8` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs` | `180CDC9F27014D99E905FC6E5A85728F8D79F3378E0DFAC6DC242ED8FE05E6A9` |
| `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs` | `F0A458367992B316B2C363D7142257865B8458A8BF967152BA34FF0369BB9D5C` |

## Protected

- accepted performance changes, shared scale `0.8477369`, area `0.47499996`,
  capacity `63/80`, roster and item proportions;
- Scene/Prefab/PNG/meta/import/VFX/Battle/Enemy/Save/Drop/Reward/formal flow;
- ItemSystem identity, shape, Board occupancy and accepted transaction truth;
- no test panel, runtime fallback hierarchy, item-specific exception or Git.

## Proportional verification

1. Offline Runtime + Editor compile only.
2. Focused tests must distinguish pointer contour acceptance from physics contour and
   cover two L/corner fixtures, transparent-corner pass-through and bounded tolerance.
3. Transaction fixture covers Board-to-tabletop success near pointer, entrance-blocked
   success elsewhere, full-table rejection with exact rollback, and duplicate/retry
   idempotence.
4. Static/event-route assertion covers card-root ownership and Board-slot begin/update/
   end drag routing. Do not label this a Unity product PASS.
5. Return `USER_MANUAL_HANDTEST_READY` with only the two failed checks. No Unity run.

## User handtest

In one existing/fresh Play preparation state:

1. drag both three-cell corner items from every visible arm; confirm the missing
   transparent corner can select the item beneath;
2. place each on Board, drag it back into an open tabletop position, then repeat;
   confirm size, contour, Board occupancy and backpack membership stay aligned.
