# V0.4-BattleSandboxStableAllTrayFallbackAndTabletopRetirement01 Task Contract

Status: `CONTAINED_ONE_GUARD / FROZEN_PENDING_ARCHIVE`

Hard dispatch gate: do not create or dispatch the development task until Item
Guard has received the exact RepoOps marker
`GRAVITY_TABLETOP_EXPERIMENT_ARCHIVE_READY` from task
`019eca25-228f-7923-8513-68d075d5ccd6`.

## 1. Outcome

Restore a reliable code-only All-items tray in the current
`DEV_SHOWCASE_LV40` BattleSandbox while retiring the planar tabletop from all
active runtime paths:

- one deterministic compact tray containing every current Inventory item;
- no active category filtering or category tabs;
- smooth vertical scrolling with inertia and row-boundary/bottom-clamp settle;
- every shape remains selectable and preserves authoritative artwork,
  proportions, rarity and identity;
- Tray to Board, Board movement and Board to Tray remain atomic;
- rejected or cancelled actions restore the exact prior state;
- planar gravity, collision, free sliding, free rotation and simulation are
  unreachable and have no hidden update, input, state or maintenance owner.

This is a forward surgical detachment from the current disk state. It is not a
Git restore, old-file replacement or whole-project rollback.

## 2. Product Context

- Context: `DEV_SHOWCASE_LV40`
- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`
- Scene and Prefab are read-only.
- Formal Inventory, Save, Drop and Reward remain out of scope.
- The planar tabletop remains preserved outside `Assets` as a
  `CANDIDATE / A_B_TEST_REQUIRED` experiment owned by the RepoOps archive.

## 3. Task Class And User Review

- Class: `CONTAINED_ONE_GUARD`
- Primary Guard: Item Guard
- Development Owner: one new visible, unarchived Item development task created
  only after the archive gate passes
- User Review Gate: `YES`
- Accepted marker:
  `USER_ACCEPTED_SCOPE_STABLE_ALL_TRAY_FALLBACK_AND_TABLETOP_ARCHIVE01`
- User Decision Point: `NONE`

The archived tabletop task `019fb7c3-301d-7bb3-8839-2ddf5a8bad94` is frozen
and must never be reused for writes.

## 4. Ownership

- ItemSystem remains the only Inventory/Board membership and identity owner.
- `ShapeAwareItemTrayGrid` remains the tray placement/collision authority.
- `BuildItemTrayPreviewView` renders one All-items projection and owns only
  view publication, scroll settling and existing card visibility.
- `BuildGridInteractionPreviewController` coordinates the existing atomic tray
  and Board operations; it must not introduce another membership owner.
- Board shape, rotation and occupied cells remain unchanged.
- The archived planar experiment owns no active runtime state after this task.

## 5. Exact Allowed Writes

Modify only:

1. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
2. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs`
3. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs`

Delete only after the RepoOps archive marker is verified:

4. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack.meta`
5. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackSnapshot.cs`
6. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackSnapshot.cs.meta`
7. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs`
8. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs.meta`
9. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs`
10. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs.meta`
11. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs`
12. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs.meta`

No new runtime, Editor, report or helper file is allowed. If any additional
file is genuinely required, stop at `ARCHITECTURE_REASSESS_REQUIRED`.

## 6. Forbidden Writes

- all Scene, Prefab, RectTransform, serialized UI and BuildSettings files;
- `BuildItemPreviewCardView.cs`, `ShapeAwareItemTrayGrid.cs`,
  `TrayItemLayoutView.cs`, `BuildGridPreviewSlotView.cs`;
- ItemSystem authority/snapshot, Item identity, shape, artwork, rarity,
  Projection, Build, Core, Roll, RNG and capacity truth;
- I031 power, Formation, Item Detail, Combat, Enemy, Battle damage;
- Save, Drop, Reward, RunFlow and formal flow;
- PNG, `.meta` files other than the explicitly deleted experimental metas,
  materials, fonts and VFX;
- archived experiment contents outside `Assets`;
- AGENTS, LOCKED, Queue, Notion and Git operations.

Do not restore whole files from Git, checkpoints, old assignments or the
archive. Preserve all later unrelated Item, Battle, VFX and user changes.

## 7. Required Sources

Read only:

- this Task Contract;
- the three allowed shared runtime files;
- `ShapeAwareItemTrayGrid.cs`, `TrayItemLayoutView.cs` and
  `BuildItemPreviewCardView.cs` as mature behavior references;
- current ItemSystem/Board public APIs used by the allowed controller;
- the RepoOps archive completion receipt and manifest path.

Historical tabletop contracts and code are not design input for the stable
fallback after their active integration has been identified for removal.

## 8. Patch Budget

- existing shared files modified: exactly 3 maximum;
- archived experiment paths deleted: exactly 9 maximum;
- new files: 0;
- Scene/Prefab files: 0.

If forward detachment cannot be completed within this boundary, stop at
`ARCHITECTURE_REASSESS_REQUIRED` instead of patching more tabletop symptoms.

## 9. Required Behavior

1. On ItemSystem authority install, build one deterministic All-items master
   tray from the current Inventory roster using the existing shape-aware grid.
2. Runtime forces the All projection. Existing category controls may be hidden
   or disabled in Play without deleting or serializing their hierarchy; no
   category selection may change the active tray.
3. Ordinary item identities and `SPECIAL_I031` remain unique. Capacity and
   membership come only from ItemSystem.
4. Initial/reset compact ordering is deterministic from machine identities and
   existing shape facts. No planar position, velocity, contour, rotation,
   impulse, collision or simulation state participates.
5. Removing one item for an accepted Board move removes exactly that item.
   Remaining placements do not drift merely because the item left.
6. Board to Tray returns the exact identity through the existing atomic path,
   using remembered placement when legal and deterministic first-legal
   placement otherwise. It does not move unrelated items.
7. Rejected/cancelled Tray or Board actions restore membership, placement,
   artwork and raycast state exactly, with no duplicate or residual card.
8. The existing Arrange control may remain only as an already-authored optional
   command using the same deterministic shape-aware master. It must not be
   recreated or become required for stale artwork cleanup.
9. Vertical ScrollRect inertia remains enabled. After drag, wheel or inertia
   settles, the content stops at an exact full-row boundary or legal bottom
   clamp using the existing grid row pitch.
10. Every current shape is selectable, including both three-cell corner items;
    visual occupied cells, card anchor, raycast and physical tray slots agree.
11. All planar/gravity code paths, callbacks, fixed updates, pointer impulses,
    state properties, diagnostics and active namespace dependencies are gone
    from the three shared files before the experiment files are deleted.

## 10. Failure And Edge Cases

- Missing ItemSystem roster or invalid identity: fail closed without creating a
  fallback roster.
- Duplicate identity: reject without partial publication.
- No legal Board-to-Tray placement: keep the item on Board and restore the
  complete previous tray state.
- Cancelled or rejected drag: exact pre-drag state.
- Empty tray: valid empty All projection, no category or physics fallback.
- Reload/reset: one deterministic master; no stale callbacks or duplicate
  listeners.

## 11. Task-Start Baselines

Allowed shared files:

| Path | SHA-256 |
|---|---|
| `BuildGridInteractionPreviewController.cs` | `898D2122C78B5A2A3BB889D3FFF9BFB9D9C14185431F48E03263111F3C0CE929` |
| `BuildItemTrayPreviewView.cs` | `E8D599BED7B615B61F11DB467E668DEC214159BF44910C0E5411D9AC816ABFA5` |
| `TrayPlacementViewModel.cs` | `C9F9BCDAECBBD0D520D6E2FE4587F6BDBD3921332E30C86349F0D878524F645C` |

Experimental deletion paths:

| Path | SHA-256 |
|---|---|
| `GravityBackpack.meta` | `C96118712892F2B3D84F019004497D9C3DF30903FE75B47B9242922583A7449E` |
| `GravityBackpackSnapshot.cs` | `8CD96BEC0B98D95D26FBA96B3E69087FB15476F5221B81D97FADADBED213859E` |
| `GravityBackpackSnapshot.cs.meta` | `8ACD1B46D2A79E82AF00091A5A5698280DF7F73D521549012D5E5D3E7B1FC956` |
| `GravityBackpackResolver.cs` | `60245CC367579AD42708416EACA290EBDF8682CC0412255AE1BDA2F09904A9E5` |
| `GravityBackpackResolver.cs.meta` | `DEC8E6A1B4908371F571E00278CF198AEC8CEE3B487379D3BE3F35D1EA641946` |
| `GravityBackpackRuntimeAdapter.cs` | `7180DA220AC459691CF1167CB8BB389A1CBE32DC0EF8E32B95268BC9C2764AC8` |
| `GravityBackpackRuntimeAdapter.cs.meta` | `CF2B3580C1109D23FF43F04E542ACA3209B51A21A4C2E030625E4E63278120D3` |
| `GravityBackpackPhase1PrototypeTests.cs` | `497F5C2CAA754AC7C8856EA42447AC2DDD4C06805066752DFA325CCE7D0E1F4E` |
| `GravityBackpackPhase1PrototypeTests.cs.meta` | `FF8D4C3B0B829962DDEEBBCB944CE4BE5179B5DADC05C46F782930B8062F6568` |

Protected baselines:

| Path | SHA-256 |
|---|---|
| `BuildItemPreviewCardView.cs` | `448A69762EC157214809E1D43CD19A134D77F7928742CC890A94A41B1588D56E` |
| `ShapeAwareItemTrayGrid.cs` | `DA8C1B628A02565908E7BC7A0EF9E56DC2E00BEC25AF549DF9382769372665BC` |
| `TrayItemLayoutView.cs` | `032FDD723F40D48AECB06F090F8CA85C702948D2FBC163FC93984231928E0074` |
| `BuildGridPreviewSlotView.cs` | `D232A34F6A761D745564C5015C977F9EEF26E2C0D23CD764B6CFA2704D008214` |
| `ItemSystemBattleSandboxBoardAuthority.cs` | `6BEF274DA275D26AFAD97E6ECBC6EC61523E602D8CC000744FD290870BFD9A20` |
| `ItemSystemBattleSandboxBoardAdapter.cs` | `2D8EC6707DF670BE6BC11705F94F1EF1DCC39558183A39023DD5A91E4A7C0BCF` |
| `ItemSystemSnapshot.cs` | `794C3A6D9DBE9FB1FF96275C247BCEF4D9D248EDC1DF3EAEE6D396DD87969827` |
| `ItemInnerDataCatalog.cs` | `606ACDC6538EEDA86F7631840A6ACBB47284368F198EF413293BB844833776C9` |
| `ItemDetailPanelView.cs` | `045D20E2CC0D2ED87D477AB48BE7A00F954E3FE976904498502157239E534A90` |
| `ItemDetailPanel.prefab` | `2A6924DA0823B74E2639B593926D8C2F8422101B01EDBE4073AF33CD424E027C` |
| `BattleSandbox Scene` | `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA` |
| `EditorBuildSettings.asset` | `D6F8DA39E5E609417D534E98C601BD78CEE97A499669E476170305E1E08A0255` |

Every listed baseline must be reverified immediately before dispatch. Drift in
an allowed shared file or experimental deletion path returns
`VERIFIED_WRITE_CONFLICT`; do not refresh silently.

## 12. Verification

Minimum automated evidence:

- scoped Runtime and Editor compile using the existing offline compile path;
- source/reference check: zero active `GravityBackpack`, `PlanarTabletop` or
  planar runtime references remain under the three shared files;
- focused non-Unity checks for deterministic master layout, remove-one
  stability, return remembered/first-legal behavior, cancel/reject rollback,
  unique identities and ScrollRect row-settle math;
- exact deletion inventory for all nine experimental paths;
- protected hash verification and package-scoped whitespace check.

No Unity batch, Fresh Play harness, Scene save, report matrix or screenshots.

User manual handtest:

1. Open BattleSandbox normally and Play.
2. Confirm one All-items tray and no usable category tabs.
3. Select single, long, both corner, 2x2 and I031 items.
4. Scroll first/middle/bottom using drag, wheel and inertia; confirm full-row or
   bottom-clamp settling with cards, background and raycasts aligned.
5. Move representative shapes Tray to Board, move them again on Board, and
   return them to Tray.
6. Reject/cancel invalid drops and confirm exact restoration with no duplicate,
   stale image or displaced unrelated item.
7. Confirm no item moves, rotates, slides or collides by planar physics.

## 13. Process Ownership

- `UNITY_NOT_REQUIRED` for implementation and automated QA.
- User performs the one normal-Editor handtest.
- Do not wait for, poll, close or modify a user Unity process.
- No Git operations.

## 14. Completion

Development stops at:

`CONTAINED_ONE_GUARD / USER_MANUAL_HANDTEST_READY`

User PASS closes:

`CONTAINED_ONE_GUARD / PACKAGE_COMPLETE`

One same-package light fix is allowed only within the frozen shared-file
boundary. Any need for another architecture, Scene/Prefab change or broad
shared rewrite stops at `ARCHITECTURE_REASSESS_REQUIRED`.
