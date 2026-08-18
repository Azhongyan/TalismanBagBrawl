# V0.4-GravityBackpackOneAxisChutePrototype01 Task Contract

Status: `USER_HANDTEST_FAIL / SUPERSEDED_BY_PLANAR_TABLETOP_MODEL`

User direction correction:

- The one-axis chute is not the intended backpack experience.
- Do not request or accept further one-axis user handtesting.
- Its files and QA evidence remain historical attribution only.
- The only active product path is the later
  `V0.4-GravityBackpackPlanarTabletop01` contract.

## 1. Outcome

Replace the unaccepted discrete-grid Gravity Backpack experiment in
`DEV_SHOWCASE_LV40` with a deterministic one-axis physical chute:

- one authored mouth at the top of the existing BattleSandbox item tray;
- every Inventory item is one stable, non-overlapping interval on scalar axis `s`;
- items may slide, contact, rebound slightly, damp and sleep only along that axis;
- no free X/Y motion, planar pile, overlap, free rotation or Tetris placement;
- an Editor-only scalar tilt simulation and pointer impulse provide the Phase 1 feel;
- ItemSystem remains the only Inventory/Board membership owner;
- capacity remains explicit data truth and is independent of visible gaps;
- the Board continues to use its existing shape/cell/rotation authority unchanged.

The previous `V0.4-GravityBackpackPhase1Prototype01` implementation is
`USER_DIRECTION_CORRECTED / NOT_ACCEPTED / SUPERSEDED_FOR_RUNTIME_USE`.
Its Task Contract and evidence remain historical records. Its grid runtime
behavior must not coexist with this package.

## 2. Product Context

- Product context: `DEV_SHOWCASE_LV40`
- Target runtime:
  `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`
- The target Scene is read-only and must not be saved.
- This is a hand-feel prototype, not formal Inventory, Drop or Save.

## 3. Task Class And Review Gate

- Class: `COMPLEX_GUARDED_ONCE`
- Primary Guard: `Item Guard`
- Development owner: existing idle task `019fb63b-7944-7751-996e-97f241b19c36`
- User Review Gate: `YES`
- Accepted marker: `GRAVITY_BACKPACK_ONE_AXIS_CHUTE_USER_ACCEPTED_SCOPE`
- Accepted on: `2026-07-31`

Frozen user-facing choices:

1. One clipped bag viewport; player input moves the one-axis chain, not pages
   or rows.
2. Any currently visible item may be lifted from the chain and dragged to the
   Board.
3. Chain order cannot cross. New acquisitions and accepted Board returns enter
   from the mouth. A rejected drag restores the original order and state.
4. Capacity remains 80 authoritative units for V1. Real phone sensors are
   deferred; V1 uses Editor simulation.

## 4. Ownership

- ItemSystem owns stable identity and Inventory/Board membership.
- The one-axis snapshot owns only devOnly presentation/interaction state for
  current Inventory members: order, `s`, scalar velocity and sleep state.
- Existing item shape cell count supplies V1 capacity cost only.
- Backpack interval length is a separate presentation value. V1 uses one
  uniform normalized interval length and gap; Board shape never changes it.
- Board authority owns Board placement, rotation and occupied cells.
- The view projects the one-axis state but cannot create, delete or duplicate
  inventory membership.

## 5. Allowed Writes

Existing files only, using the task-start disk state below:

1. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs`
2. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
3. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs`
4. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackSnapshot.cs`
5. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs`
6. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs`
7. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs`

The rejected grid experiment may be refactored in place because it never
reached user acceptance. The final schema/canonical identifier must explicitly
identify the one-axis chute semantics and must not claim compatibility with the
old grid snapshot.

No new implementation path is authorized. A genuine need for another path is a
boundary blocker and must return to Item Guard before writing.

## 6. Forbidden Writes

- all Scene, Prefab, RectTransform and serialized UI files;
- all `.meta`, PNG, sprite, font and artwork files;
- `ItemSystemBattleSandboxBoardAuthority.cs`, `ItemSystemSnapshot.cs`;
- Item identities, generated instances, shapes, local cells and rotations;
- `ShapeAwareItemTrayGrid.cs`, `TrayItemLayoutView.cs`,
  `BuildItemPreviewCardView.cs`;
- Item Detail, Build, Core, Roll, RNG, probability and balance;
- Battle damage, Enemy, Capability and formal battle;
- Save, Drop, Reward, RunFlow and campaign flow;
- categories hierarchy cleanup, resources, stacking, currencies, search,
  manual sort and player rotation;
- real device gyro/accelerometer integration;
- AGENTS, LOCKED, Queue, Notion and Git operations.

Do not restore or replace unrelated dirty files from Git HEAD.

## 7. Task-Start Baselines

Allowed files:

| Path | SHA-256 |
|---|---|
| `BuildItemTrayPreviewView.cs` | `BF55F5AE0DC39A55339EDD87ED92D86A827E4788197C929A09CFBA16D5893F7F` |
| `BuildGridInteractionPreviewController.cs` | `FD91A1122D0FA3A319A7FDC42146B65247FC0ECCE63EC58A4A4C3890CC072EFA` |
| `TrayPlacementViewModel.cs` | `1D4825CA40666528D060C48EE714FC5B4EBA275D9121875B2EEFFE75F44BC80F` |
| `GravityBackpackSnapshot.cs` | `03DB8948CC3C2115C06E26244580CEF3F958688BAECC33C04FF505FA0F965D4A` |
| `GravityBackpackResolver.cs` | `84CF085E13969E1F20DC3A204B9D2FD1D9FFEC97AE5B6EEAEEB4A7FC38A98495` |
| `GravityBackpackRuntimeAdapter.cs` | `0C4C4E0D8B588E9DBE8480F0739D3C3F11EFF5E2FA5A637FA6B0A403781A2428` |
| `GravityBackpackPhase1PrototypeTests.cs` | `2C4CB39AD709B11EBBFBF9860E381B0D0C0D4C94FA566DB9842801F55EA37FA0` |

Protected files:

| Path | SHA-256 |
|---|---|
| `BuildItemPreviewCardView.cs` | `448A69762EC157214809E1D43CD19A134D77F7928742CC890A94A41B1588D56E` |
| `ShapeAwareItemTrayGrid.cs` | `DA8C1B628A02565908E7BC7A0EF9E56DC2E00BEC25AF549DF9382769372665BC` |
| `TrayItemLayoutView.cs` | `032FDD723F40D48AECB06F090F8CA85C702948D2FBC163FC93984231928E0074` |
| `ItemSystemBattleSandboxBoardAuthority.cs` | `6BEF274DA275D26AFAD97E6ECBC6EC61523E602D8CC000744FD290870BFD9A20` |
| `ItemSystemSnapshot.cs` | `794C3A6D9DBE9FB1FF96275C247BCEF4D9D248EDC1DF3EAEE6D396DD87969827` |
| `ItemInnerDataCatalog.cs` | `606ACDC6538EEDA86F7631840A6ACBB47284368F198EF413293BB844833776C9` |
| `ItemDetailPanelView.cs` | `045D20E2CC0D2ED87D477AB48BE7A00F954E3FE976904498502157239E534A90` |
| `ItemDetailPanel.prefab` | `2A6924DA0823B74E2639B593926D8C2F8422101B01EDBE4073AF33CD424E027C` |
| `BattleSandbox Scene` | `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA` |

Existing GravityBackpack and test `.meta` files must remain byte-identical to
their task-start state.

### REV01 External Scene Baseline Attribution

- Original frozen Scene SHA:
  `BA2D62A95229974580AF23184A89D5853F17D672416241D80EDC6901DBC1DAF4`.
- Current Scene SHA:
  `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA`.
- Current Scene last write:
  `2026-07-31 13:49:39.926 +08:00`.
- The first successful task-owned Unity process began at
  `2026-07-31 14:08:50 +08:00`.
- The task did not open, save or write the Scene.
- Guard recheck found zero Unity/Tuanjie/ShaderCompiler/bee processes,
  no UnityLockfile, no EditorInstance, and the current Scene hash was stable
  across repeated reads.

Classification:
`EXTERNAL_PREEXISTING_SCENE_SAVE / OWNER_UNPROVEN / PRESERVE_CURRENT_DISK`.

REV01 refreshes only the protected read-only Scene baseline. It does not accept
the external Scene delta as package ownership, does not authorize any Scene
write, and does not waive byte equality against the refreshed hash at final QA.

## 8. One-Axis Contract

Each Inventory body contains:

- stable identity: ordinary `itemInstanceId` or `SPECIAL_I031`;
- stable order index;
- scalar center position `s`;
- scalar velocity;
- uniform V1 interval length;
- authoritative capacity cost;
- sleeping/active motion state.

Rules:

1. Axis only: no X/Y placement, angle, free rotation or planar collider state.
2. Non-overlap: intervals may touch but never intersect logically or visually.
3. Stable order: intervals cannot pass one another.
4. Mouth and end wall are hard boundaries.
5. Fixed-step deterministic integration applies scalar acceleration, damping,
   friction, bounded restitution, contact resolution and sleep.
6. Compression feel may use a small visual squash only; logical intervals stay
   non-overlapping.
7. Same initial snapshot, tick sequence and inputs produce the same canonical
   result.
8. No-op ticks do not republish snapshots or replay settle feedback.

V1 tuning values for acceleration, damping, friction, restitution, sleep
threshold, interval length and gap must have one generic source in the
one-axis implementation. No item ID, display name, shape or rarity branch.

## 9. Capacity And Identity

- Capacity is `80` units.
- I001-I030 use current authoritative item shape cell count as capacity cost.
- SPECIAL_I031 costs one unit.
- Initial real roster remains 31 identities and 63/80 units.
- Capacity cost does not determine interval length or visible spacing.
- Visible gaps do not create capacity.
- Full rejection occurs before accepted Inventory membership changes.
- Missing/duplicate identity or capacity data fails closed; never fabricate a
  1-unit anonymous item.

## 10. Input And Presentation

- Existing tray mask/viewport is reused without serialized modification.
- Existing category controls stay authored but are hidden/disabled only while
  the one-axis prototype is active.
- Grid cells, footprint colors, row snapping, page-tail positioning and
  Arrange are inactive in one-axis mode.
- Cards remain centered on one cross-axis line. Only their scalar axis position
  changes.
- Empty-channel pointer drag/wheel may apply a scalar impulse to the chain.
- Editor-only positive/negative tilt input changes scalar acceleration.
- Real device sensor APIs are forbidden in V1.
- Any currently visible card may be lifted into the existing drag layer.
- The selected lifted item is temporarily absent from contact solving but
  remains staged until the authoritative transfer resolves.
- Input/raycast ownership must follow the visible card and cannot leave a stale
  artwork or hit target.

## 11. Atomic Transfers

Backpack to Board:

1. Capture exact one-axis snapshot and selected identity.
2. Stage lifting exactly one interval.
3. Ask existing Board/ItemSystem authority to validate and commit.
4. On acceptance, remove that Inventory interval once and let the remaining
   chain settle.
5. On rejection/cancel, restore exact order, position, velocity, sleep and
   visible/raycast state without a global reinitialization.

Board to backpack:

1. Check capacity and stable identity before membership change.
2. On acceptance, add the instance exactly once at the mouth end of the order.
3. Let it enter and settle through the same one-axis solver.
4. On rejection/full state, keep Board and backpack snapshots unchanged.

Board shape, rotation, preview, collision and committed occupied cells must be
identical to their task-start behavior.

## 12. Required Regressions

- 31 unique real identities, 63/80 capacity, no duplicate or omission;
- no grid `AnchorCell`, backpack `OccupiedCells`, columns/rows, lowest-cell or
  unsupported-cell collapse in the active one-axis path;
- scalar positions only, stable order and zero interval overlap;
- deterministic positive/negative acceleration, contact, damping and sleep;
- pointer impulse produces continuous one-axis movement, not row snapping;
- accepted Tray-to-Board removes one instance and no stale image/raycast;
- rejected/canceled drag restores exact pre-drag snapshot;
- accepted Board return enters at mouth exactly once;
- full return rejects without changing either side;
- I010/I016/I018 board shapes remain unchanged;
- SPECIAL_I031 identity and capacity participation remain correct;
- repeated ticks, transfers and reset contain no stale or duplicate state;
- categories/grid/Arrange/rotation remain inactive only in prototype Play mode;
- all protected hashes remain exact.

## 13. QA

Proportional V2 QA only:

1. scoped compile;
2. focused deterministic pure-logic tests for interval solving and transfers;
3. one normal Editor Fresh Play if a clean Unity window is available;
4. one concise user hand-feel test.

Do not create a batch Scene bootstrap, screenshots, CSV matrices or extra
reports. Do not save the Scene.

User hand-feel test:

1. Enter Play and confirm the visible backpack is one centered chute with no
   grid packing, categories or row snapping.
2. Apply positive and negative Editor tilt and confirm the ordered chain slides,
   contacts, settles and never overlaps or changes order.
3. Drag/swipe the empty channel and confirm continuous scalar movement with
   damping.
4. Lift a visible item to a legal Board cell and confirm exactly that item
   leaves, with no stale image or hit target.
5. Try an illegal Board drop and confirm the exact chain state returns.
6. Return a Board item and confirm it enters from the mouth once; test truthful
   full rejection.
7. Repeat transfers and reset, checking I010/I016/I018 and SPECIAL_I031.

## 14. Process

- The development task is idle and may be reused for this superseding package.
- Current Item Combat work is in a disjoint `Items/Combat` write domain.
- Code and pure tests may proceed while a user Editor is open because Scene and
  Prefab serialization are forbidden.
- Unity ownership is checked only before the single Fresh Play.
- Any started process records PID/command/log/timeout and must be cleaned by its
  owner before terminal status.
- No Git operation.
- Development stays quiet until terminal, verified write conflict, boundary
  expansion or unavoidable user decision.

## 15. Completion

Stop at:

`COMPLEX_GUARDED_ONCE / USER_HANDTEST_WAITING`

User PASS closes:

`COMPLEX_GUARDED_ONCE / PACKAGE_COMPLETE`

Same-package one-axis hand-feel fixes remain in the same development task.
Phone sensors, resource stacks, search, manual sort, formal Inventory,
Drop/Save/Reward and formal promotion remain separate user-reviewed work.
