# V0.4-GravityBackpackPhase1Prototype01 Task Contract

Status: `FROZEN_FOR_SINGLE_DISPATCH`

## 1. Outcome

In `DEV_SHOWCASE_LV40`, the BattleSandbox item tray is replaced at runtime by one scrollable 5 x 16 gravity backpack:

- real I001-I030 item instances plus SPECIAL_I031 use their current authoritative shapes;
- the backpack has 80 discrete cells;
- items fall vertically to the lowest legal position without automatic horizontal movement or rotation;
- removing an item only drops items that lose support;
- backpack-to-board and board-to-backpack operations commit atomically;
- invalid or canceled operations restore the exact previous backpack and board state;
- capacity and full-state feedback are truthful;
- the user can scroll through the full backpack and evaluate the Phase 1 hand feel.

This is a devOnly interaction prototype, not formal Inventory, Drop or Save.

## 2. Product Context

- `DEV_SHOWCASE_LV40`
- Target runtime: `Scene_TalismanBag_V04_BattleSandboxPreview`
- The target Scene is protected and must not be saved or modified by this package.

## 3. Task Class

- `COMPLEX_GUARDED_ONCE`

Reason: this package introduces one new devOnly backpack layout state and atomically coordinates it with the existing ItemSystem board-location authority. It must explicitly prevent a second item-location owner.

## 3A. User Review Gate

- Required: `YES`
- Fixed category Guard: `Item Guard`
- Accepted marker: `GRAVITY_BACKPACK_USER_ACCEPTED_SCOPE`
- Accepted on: `2026-07-31`

Frozen Phase 1:

- 5 columns x 16 rows = 80 cells;
- real I001-I031 shape facts;
- deterministic discrete-cell gravity;
- vertical-only local collapse;
- BattleSandbox-to-board atomic round trip;
- capacity/full-state and scrolling feel.

Deferred:

- player rotation;
- Arrange/global repack;
- resources and stacking;
- search;
- Drop, Save, Reward and formal flow.

## 4. Owners

- Primary Guard: `Item Guard`
- Development Owner: one new independent Gravity Backpack development task
- Board location authority: existing `ItemSystemBattleSandboxBoardAuthority` / `ItemSystemSnapshot.v2`
- Backpack layout authority: new devOnly immutable `GravityBackpackSnapshot.v1`, only for items whose accepted ItemSystem location is Inventory
- Static shape authority: existing Item inner catalog / generated item projection
- Presentation owner: existing BattleSandbox tray cards, layout and ScrollRect
- User Decision Point: `NONE` for Phase 1

The backpack snapshot may never independently decide whether an item is on the board. It consumes accepted ItemSystem membership and owns only the cell layout of current Inventory members.

## 5. Allowed Writes

Existing files, using the task-start disk state as baseline:

1. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs`
2. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
3. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs`
4. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs`
5. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs`

Allowed new files:

6. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack.meta`
7. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackSnapshot.cs`
8. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackSnapshot.cs.meta`
9. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs`
10. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs.meta`
11. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs`
12. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs.meta`
13. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs`
14. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs.meta`

The developer may use fewer files. Any additional existing or new path requires a true boundary blocker and Item Guard review before writing.

## 6. Forbidden Writes

- `Assets/_Game/Scenes/**`
- `Assets/_Game/Prefabs/**`
- `ProjectSettings/**`, `Packages/**`, BuildSettings
- `ItemSystemBattleSandboxBoardAuthority.cs`
- `ItemSystemSnapshot.cs` and all accepted ItemSystem contracts/algorithms
- Item inner catalog, shapes, local cells, generated instances, Roll/RNG/probabilities
- `BuildItemPreviewCardView.cs` and artwork/icon assets
- ItemDetail views, models, Prefab and interaction
- Board coordinates, placement rules and physical occupied-cell semantics
- Board Formation VFX files and the active TBB-26 write domain
- Enemy, Battle damage, Capability, Save, Drop, Reward, RunFlow and formal battle
- resources, resource stacks, currencies, search, rotation and Arrange behavior
- category Scene hierarchy deletion or cleanup
- AGENTS, LOCKED, Queue, Notion and Git operations

Existing dirty files are user/previous-package work. Do not reset, restore, normalize or replace them from Git HEAD.

## 7. Required Sources

- `AGENTS.md`
- `Docs/LOCKED/ENGINEERING_PROCESS_V2_LOCK.md`
- `Docs/CURRENT/PROJECT_ENGINEERING_STATE_V2.md`
- this Task Contract
- `ItemShapeCatalogBatchCorrectionSpec.csv`
- current task-start versions of the five allowed existing files
- current `ItemSystemBattleSandboxBoardAuthority` and `ItemSystemSnapshot.v2` public APIs
- current BattleSandbox view projection roster and I031 special identity contract

Historical tray reports are evidence only and must not become product authority.

## 8. Task-Start Baselines

Allowed existing files:

| Path | SHA-256 |
|---|---|
| `BuildItemTrayPreviewView.cs` | `a7957ffd262dc5231622e8762c644ebad1ccb5d0b662325a4fe9ec62d23d497f` |
| `BuildGridInteractionPreviewController.cs` | `519a7b69fc16a9c50563ceb34f614967ae58854c57f4846d82bdcdee9093523a` |
| `ShapeAwareItemTrayGrid.cs` | `da8c1b628a02565908e7bc7a0ef9e56dc2e00bec25af549df9382769372665bc` |
| `TrayPlacementViewModel.cs` | `a4075e2279bb937858be97a5e41365ce0eff58f12277f0d19b93b7efac29b590` |
| `TrayItemLayoutView.cs` | `032fdd723f40d48aecb06f090f8ca85c702948d2fbc163fc93984231928e0074` |

Protected disk baselines:

| Path | SHA-256 |
|---|---|
| `ItemSystemBattleSandboxBoardAuthority.cs` | `6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20` |
| `ItemSystemSnapshot.cs` | `794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827` |
| `ItemInnerDataCatalog.cs` | `606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9` |
| `ItemDetailPanelView.cs` | `045d20e2cc0d2ed87d477ab48be7a00f954e3fe976904498502157239e534a90` |
| `ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `ba2d62a95229974580af23184a89d5853f17d672416241d80edc6901dbc1daf4` |

Protected baseline drift caused by another owner is a blocker; do not restore it or silently accept it. Allowed existing files must still match their task-start hashes before first write.

## 9. Patch Budget

- Maximum implementation paths: 14
- Existing files: at most 5
- New runtime files/meta: at most 7
- New focused test files/meta: at most 2
- Reports: none required

If the feature requires Scene/Prefab changes, a formal Inventory contract, resources, Save/Drop/Reward, or more than 14 paths, stop and return `BOUNDARY_EXPANSION_REQUIRED`.

## 10. Required Behavior

1. Initial roster:
   - consume the real BattleSandbox I001-I030 instances and SPECIAL_I031;
   - use existing shapes and current rotations;
   - create a deterministic 5 x 16 backpack layout with no overlap or out-of-bounds cells;
   - ordinary item footprint total is 62 cells; SPECIAL_I031 adds one, so the initial occupied total is 63/80.

2. Gravity:
   - a committed item keeps its current horizontal anchor and rotation;
   - it falls vertically to the greatest legal row;
   - no automatic horizontal movement or rotation;
   - an item is supported when at least one lower boundary cell touches the floor or another item;
   - after removal, repeatedly process only unsupported items in deterministic bottom-to-top order with stable identity as the tie-breaker;
   - stop when no item moves;
   - unrelated supported items must retain exact anchors.

3. Backpack internal drag:
   - pointer x selects a candidate column;
   - the staged item falls to its lowest legal row;
   - success publishes one immutable snapshot and one matching visual/raycast/reservation state;
   - invalid, out-of-bounds or canceled drag restores the exact pre-drag snapshot.

4. Backpack to board:
   - removal and gravity remain staged until the existing ItemSystem board authority accepts the operation;
   - after acceptance, publish the new backpack snapshot once;
   - rejection restores the exact backpack snapshot, artwork and raycast state without stale visuals.

5. Board to backpack:
   - ItemSystem remains the location authority;
   - the selected horizontal entry lane and current rotation must have a legal gravity result;
   - only after both sides validate may the operation commit;
   - no legal result means the board state and backpack state both remain unchanged.

6. Capacity/full state:
   - report `usedCells`, `capacityCells=80` and remaining cells;
   - geometric insertion failure is authoritative even when the raw free-cell count is large enough;
   - no item may be deleted, converted, stacked or silently omitted;
   - Phase 1 full-state feedback may reuse the existing placement feedback surface.

7. Presentation:
   - one runtime backpack view only;
   - existing category controls are hidden/disabled in Play but their authored Scene hierarchy is untouched;
   - no filtered projection and no second placement map;
   - item image, colored footprint, reservation cells, pointer hit cells and drag source use the same snapshot cells;
   - ScrollRect covers all 16 rows;
   - gesture/inertia may move freely, then settle to a complete row boundary or exact bottom clamp;
   - no runtime overwrite of user-authored Scene geometry outside the existing tray content behavior.

8. Determinism and identity:
   - same input snapshot and operation sequence produces the same canonical layout;
   - ordinary items key layout by `itemInstanceId`;
   - I031 keys by `SPECIAL_I031`;
   - never use display name, Chinese text, hierarchy order or baseItemId as a substitute for stable instance identity.

## 11. Failure / Edge Cases

- Missing shape/identity: fail closed; do not fabricate a 1 x 1 item.
- Duplicate identity or overlapping input: reject the new snapshot.
- Repeated accepted callback/no-op: no duplicate publication or animation replay.
- Invalid board snapshot: keep the previous valid backpack snapshot.
- Drag canceled during scroll: restore pre-drag snapshot and resume scroll ownership cleanly.
- Domain reload/disable: no runtime object may become a second saved Scene hierarchy.
- Failed operation: atomic rollback of layout, cards, reservations, artwork and raycast.

## 12. Delivery Shape

- Runtime: immutable snapshot + deterministic resolver + thin devOnly runtime adapter
- Existing integration: minimal changes to current tray/controller/layout files
- Scene / Prefab: none
- Config / Profile: none
- Editor: one focused deterministic test entry only if needed
- Report: none

## 13. Verification

- [x] User Review Gate complete
- [ ] task-start hashes and exact write scope rechecked before first write
- [ ] focused compile
- [ ] deterministic gravity tests
- [ ] exact initial roster: 31 identities, 63/80 occupied cells, no overlap/out-of-bounds
- [ ] local-collapse regression: only unsupported chains move
- [ ] bag internal move success/reject/cancel rollback
- [ ] bag-to-board accepted/rejected atomic behavior
- [ ] board-to-bag accepted/full rejection atomic behavior
- [ ] visual cells = raycast cells = reservation cells = drag source cells
- [ ] category controls inactive and no filtered projection publication
- [ ] one normal-Editor Fresh Play when the serialized Unity gate is available
- [ ] one concise user hand-feel test

User hand-feel test:

1. Scroll top/middle/bottom and confirm full-row settle.
2. Move an item horizontally inside the backpack and observe vertical fall.
3. Remove a supporting item and confirm only unsupported items fall.
4. Drag an item to a legal board cell and confirm the backpack collapses once.
5. Try an illegal board drop and confirm the backpack restores exactly.
6. Return a board item to the backpack and confirm one legal fall or truthful full rejection.
7. Repeat with I010/I016 corner shapes, I018 block2x2 and SPECIAL_I031.

Not required:

- Unity batch harness;
- screenshots or generated CSV matrices;
- Scene save;
- APK;
- formal Drop/Save/Reward integration.

## 14. Process Ownership

- Code and offline tests may proceed while a user Editor is open because Scene/Prefab writes are forbidden.
- Fresh Play requires a clean serialized Unity window and must not launch beside another Editor/batch.
- If Unity is launched, record command, owned PID, start time, log and timeout.
- Stop only the exact owned stalled process.
- Completion requires zero package-owned Unity/helper processes and no package-owned lock residue.
- No Git operations.

## 15. Completion

Stop at:

`COMPLEX_GUARDED_ONCE / USER_HANDTEST_WAITING`

when implementation, focused compile/tests and one reachable Fresh Play are complete.

After the user passes the seven hand-feel checks:

`COMPLEX_GUARDED_ONCE / PACKAGE_COMPLETE`

Same-package Phase 1 defects remain in the same development task. Rotation, Arrange, resources, search, Drop, Save, Reward and formal promotion require a later user-reviewed scope.
