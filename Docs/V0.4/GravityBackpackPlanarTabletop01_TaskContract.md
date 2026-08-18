# V0.4-GravityBackpackPlanarTabletop01 Task Contract

Status:
`USER_HANDTEST_PARTIAL_PASS_DIRECTION_ACCEPTED / REV01_BOUNDARY_FROZEN`

## REV01 User Hand-Feel Correction

The user accepts `PLANAR_TABLETOP_2D` as the product direction. This is a
same-package revision, not a new model or package. REV01 corrects three
first-pass defects:

1. the tabletop must feel heavy and controlled rather than icy;
2. each item must retain its accepted artwork footprint, aspect ratio and
   relative size instead of being normalized to one small card;
3. collision must follow the final visible artwork Sprite alpha contour rather
   than one generic circle or rectangle.

The original seven-file implementation boundary remains exact. No Scene,
Prefab, artwork, importer, VFX or Item truth file is added to the write set.

## 1. Outcome

Replace the rejected one-axis chute with one runtime-only planar tabletop
backpack in `DEV_SHOWCASE_LV40`:

- the existing authored tray viewport is a closed top-down two-dimensional box;
- every current Inventory instance has X/Y position and velocity, limited
  presentation rotation/angular velocity, collision state and sleep state;
- items may move, collide, push, rotate slightly and settle anywhere in the
  plane;
- items never overlap, tunnel through one another or cross the four walls;
- neutral input has zero persistent acceleration, so nothing automatically
  sinks, queues, stacks, sorts or snaps;
- logical capacity remains 80 units with the current real roster at 63/80;
- ItemSystem remains the only Inventory/Board membership owner;
- Board shape, placement rotation and occupied cells remain unchanged.

The grid-gravity and one-axis-chute models are both
`USER_HANDTEST_FAIL / SUPERSEDED`. They must not remain active or selectable.

## 2. Product Context

- Context: `DEV_SHOWCASE_LV40`
- Target:
  `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`
- Scene and Prefab are read-only.
- This package is a hand-feel prototype, not formal Inventory, Drop or Save.

## 3. Class And Review

- Class: `COMPLEX_GUARDED_ONCE`
- Primary Guard: `Item Guard`
- Development task: `019fb63b-7944-7751-996e-97f241b19c36`
- User Review Gate: `YES`
- Accepted marker: `GRAVITY_BACKPACK_PLANAR_TABLETOP_USER_ACCEPTED_SCOPE`
- Accepted on: `2026-07-31`

No additional product choice is required for this V1.

## 4. Ownership

- ItemSystem owns stable identity and Inventory/Board membership.
- Planar tabletop state owns only devOnly presentation/interaction state for
  accepted Inventory members.
- Existing item shape cell count supplies capacity cost only.
- Collision size and visual rotation are presentation facts, independent of
  Board cells and Board placement rotation.
- The runtime adapter stages ItemSystem membership changes and publishes the
  tabletop snapshot only after both sides accept.
- The view renders the snapshot and may not create a second membership owner.

## 5. Exact Allowed Writes

Existing files only:

1. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs`
2. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`
3. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs`
4. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackSnapshot.cs`
5. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs`
6. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs`
7. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs`

The rejected one-axis implementation may be replaced in place. The final
schema/canonical identifier must explicitly identify planar-tabletop semantics.
No active code path may retain one-axis scalar order, chute gravity or legacy
grid placement.

No new implementation path is authorized. A genuine need for another file
must return as `BOUNDARY_EXPANSION_REQUIRED` before writing.

## 6. Forbidden Writes

- all Scene, Prefab, RectTransform and serialized UI files;
- all `.meta`, artwork, icon, PNG, font and material files;
- `BuildItemPreviewCardView.cs`, `ShapeAwareItemTrayGrid.cs`,
  `TrayItemLayoutView.cs`;
- `ItemSystemBattleSandboxBoardAuthority.cs`, `ItemSystemSnapshot.cs`;
- Item identity, shape, local cells, generated instance, Build/Core/Roll/RNG;
- Board coordinate, placement, rotation and collision semantics;
- Item Detail, Battle damage, Enemy, Capability and formal battle;
- Save, Drop, Reward, RunFlow and campaign flow;
- resources, stacking, currencies, search, manual sorting and formal Inventory;
- real phone sensor APIs;
- runtime test panels or saved helper hierarchy;
- AGENTS, LOCKED, Queue, Notion and Git operations.

Do not reset, restore or normalize unrelated dirty work.

## 7. Task-Start Baselines

Allowed files:

| Path | SHA-256 |
|---|---|
| `BuildItemTrayPreviewView.cs` | `972607809FAB2345055FE100D6F22C956C5CCFB7A7DD268B6705C475AF956F83` |
| `BuildGridInteractionPreviewController.cs` | `B0150D270DB1DB5D9B24FD40A62E09B49F35A20EE3CF871271D01BB9217EA9B5` |
| `TrayPlacementViewModel.cs` | `66A06F64605DA4EE140529AB742072D9FF604008E170C91E9AF080231E558263` |
| `GravityBackpackSnapshot.cs` | `2B58081D379F121B45637A745A03EF6E837901D6D7008384F36E27DE1E70B5C9` |
| `GravityBackpackResolver.cs` | `8F1F8AE48FCC09845AEBE4DD265955CD805A41D18157EAF633932738F58811EC` |
| `GravityBackpackRuntimeAdapter.cs` | `C9738ED45E78FB5A1D7B79B03C47A6C38EB197872E5B50F77E704D5379D5D3AA` |
| `GravityBackpackPhase1PrototypeTests.cs` | `16D2CEDE629951C8B95C04ABD275E2EF023BF960538506A2072C8ED597C7111E` |

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

All existing GravityBackpack/test `.meta` files remain byte-identical.

## 8. Planar Snapshot

Each accepted Inventory body contains:

- stable identity: ordinary `itemInstanceId` or `SPECIAL_I031`;
- 2D center position;
- 2D linear velocity;
- presentation rotation, limited to a generic range within approximately
  `+/-15` to `+/-25` degrees;
- angular velocity;
- generic presentation collision radius or convex envelope;
- authoritative capacity cost;
- sleeping/active/held state.

The snapshot contains no scalar chute `s`, stable queue order, grid anchor,
occupied cells, rows, columns or backpack placement rotation.

Neutral acceleration is exactly zero. Optional V1 Editor input may supply a
temporary X/Y acceleration vector, but real sensor APIs are deferred.

## 9. Physics Contract

1. Four hard walls bound the authored tray viewport.
2. Fixed-step planar integration uses bounded velocity, damping, friction,
   collision response and sleep.
3. Use enough deterministic substeps or swept checks that V1 maximum speed
   cannot tunnel through a wall or another body.
4. Contacts resolve to non-overlap. A body may touch but never visually or
   logically intersect another body.
5. Held pointer motion participates in collision solving and may push nearby
   bodies. It may not teleport through them.
6. Releasing a body preserves capped 2D inertia, then damping brings it to
   stable sleep.
7. Repeated resting contacts must not create permanent jitter.
8. Presentation rotation affects the backpack visual/collision pose only and
   never writes Board rotation or item shape facts.
9. Tuning values have one generic source. No item ID, name, rarity, shape or
   category branch.
10. Same snapshot, fixed ticks and inputs produce the same result.

REV01 supersedes the first-pass generic circle allowance:

1. The final visible `BuildItemPreviewCardView.AuthoritativeArtworkImage` and
   its active Sprite are the only collision-art source.
2. The implementation may read the Sprite's imported physics shape through
   Unity's read-only Sprite API. It must not require PNG or importer edits.
3. The sampled local outline is cached deterministically by Sprite identity
   plus the effective artwork geometry. Collinear points and tiny spikes are
   simplified with one generic tolerance and a bounded vertex budget.
4. Concave transparent corners must remain passable. Use deterministic convex
   decomposition or an equivalent compound-contour representation; a convex
   hull, uniform circle or full rectangular fallback is not acceptable.
5. Broad-phase bounds may be conservative, but final contact and wall response
   must use the transformed contour.
6. The same card/artwork transform drives rendering, pointer hit ownership and
   collision. Scale, aspect ratio and limited presentation rotation must keep
   the contour aligned with visible pixels.
7. If the final Sprite has no usable imported physics shape, planar install
   fails closed with explicit
   `PLANAR_ARTWORK_CONTOUR_UNAVAILABLE:<stableIdentity>` evidence. ItemSystem
   membership remains untouched. Do not silently substitute a small square,
   circle or Board footprint.
8. `ItemLivingGradientOutlineVfx` and its shader are read-only references for
   final-Sprite alpha semantics only. The VFX object, material and lifecycle
   never own or mutate tabletop collision.

Board footprint remains unrelated to tabletop collision.

REV01 contact feel:

- high surface friction and high linear/angular damping;
- restitution is extremely low and permits at most one restrained visible
  rebound;
- pointer release velocity and angular velocity are strongly capped;
- ordinary release/contact motion reaches visible sleep in approximately
  `0.25` to `0.6` seconds under nominal Fresh Play input;
- resting contacts use a stable sleep/contact threshold so there is no
  perpetual micro-jitter;
- stopping must still be produced by friction, damping, contact and sleep. Do
  not lock positions, disable collision or teleport bodies to fake stability.

## 10. Initial Scatter And Entry

- Consume the real I001-I030 instances and SPECIAL_I031.
- Capacity remains 63/80.
- Initial placement is a deterministic non-grid scatter across the planar box.
- It must not reveal rows, columns, sorting or a hidden one-axis queue.
- New and Board-returned bodies enter through the existing tray entrance with
  a small bounded inward and lateral velocity.
- Entry tries deterministic legal non-overlapping candidates.
- If no valid entry state exists, reject atomically with an explicit
  `ENTRY_BLOCKED` result. Do not force overlap or infer capacity from pixels.
- Capacity-full and geometry-entry-blocked are distinct diagnostics.

## 11. Input And Presentation

- Reuse the existing authored tray viewport and clipping without saving it.
- Runtime-only physics/presentation state must be destroyed on exit and may not
  dirty the Scene.
- Remove the first-pass uniform `cardRect.sizeDelta` projection. Existing
  authored nonzero card and artwork geometry is captured before planar
  mutation and restored/preserved; planar publication may update only the
  tabletop position and limited Z presentation rotation.
- Runtime-created cards with no authored nonzero geometry use the existing
  accepted pre-planar tray visual sizing contract as their one-time
  presentation baseline. Their authoritative Sprite aspect ratio is preserved.
- Single-cell, long-strip, corner, 2x2 and I031 artwork must have visibly
  different accepted footprints and proportions. Capacity cost remains
  independent and cannot normalize visual size.
- Runtime must not rewrite authored anchors, pivot, size, scale, artwork rect,
  material, color, Sprite, sibling order or hierarchy as part of planar
  updates.
- Category, grid, row snap, Arrange, one-axis tilt and chute controls are
  inactive in planar mode.
- Pointer drag selects a visible body, opens Item Detail on the existing click
  path, or stages a Board drag through the current interaction path.
- Editor V1 may use pointer gestures and optional WASD/arrow X/Y acceleration.
- No constant downward force is allowed.
- Item artwork, raycast and collision projection must follow the same visible
  body with no stale image or hit target.

## 12. Atomic Transfers

Tray to Board:

1. Capture the exact planar snapshot and selected identity.
2. Stage exactly one held body.
3. Ask existing Board/ItemSystem authority to validate and commit.
4. On acceptance, remove that Inventory body once.
5. On reject/cancel, restore exact membership and a valid non-overlapping
   planar state without duplicate or stale presentation.

Board to Tray:

1. Validate capacity and stable identity before membership mutation.
2. Find a legal entry state at the entrance.
3. Commit membership and one runtime body atomically.
4. On capacity or entry failure, keep Board and tabletop unchanged.

Board shape, placement rotation, preview, collision and occupied cells remain
identical to the task-start behavior.

## 13. Required Regressions

- 31 unique identities and truthful 63/80 capacity;
- ten sequential entries scatter in X/Y and do not sink or form a queue;
- no overlap and no boundary escape for every fixed tick;
- dragging one body can push neighbors;
- release produces short bounded glide and stable sleep;
- 20 repeated drag/collision/cancel cycles produce no tunneling, duplicate,
  stale image, stale raycast or permanent jitter;
- legal Board transfer removes exactly one body;
- illegal/canceled Board transfer restores a valid complete state;
- Board return enters once or rejects atomically;
- visual gaps never change capacity;
- no grid snap, automatic sort, one-axis order or hourglass pile;
- I010/I016/I018 and SPECIAL_I031 retain Board behavior and identity;
- I009, one long-strip item, one corner item, one 2x2 item and SPECIAL_I031
  preserve distinct accepted artwork proportions;
- their transformed alpha contours remain aligned through limited rotation,
  contact and wall response;
- transparent corners clear contacts while visible protrusions participate;
- 20 repeated interactions do not grow live contour caches, colliders,
  materials or runtime helper objects;
- all protected hashes and metas remain exact.

## 14. QA

V2-proportional QA only:

1. scoped compile;
2. focused deterministic compound-contour collision, wall, sleep, cache and
   transaction tests;
3. one normal Editor Fresh Play;
4. one concise user hand-feel gate.

No batch Scene bootstrap, report matrices, screenshots, generated panels or
Scene save.

User hand-feel gate:

1. Compare I009, one long strip, one corner, one 2x2 and I031; confirm their
   accepted sizes/aspect ratios differ and no item is normalized to one cell.
2. Push those items together; visible alpha edges collide, transparent corners
   clear and the contours stay aligned while rotating slightly.
3. Release a pushed item; it may move briefly but settles naturally in about
   `0.25` to `0.6` seconds with no repeated bounce or micro-jitter.
4. Repeat 20 drag/collision/cancel and Tray/Board round trips with no overlap,
   tunneling, wall escape, duplicate, stale art or stale collision.
5. Confirm capacity remains data-based at 63/80 regardless of visible gaps.

## REV01 Task-Start Baselines

These hashes freeze the current disk state after the user-tested first planar
pass. They supersede section 7 only for REV01 write attribution; section 7
remains historical evidence.

| Path | SHA-256 |
|---|---|
| `BuildItemTrayPreviewView.cs` | `C41440536FC762A0CB2F061590BF863AEDD8232A2260983E1D1414551671486C` |
| `BuildGridInteractionPreviewController.cs` | `A62575C82C517EA6686376049731C699E5E22AB8D0F10777F54B3CC6DC62A698` |
| `TrayPlacementViewModel.cs` | `EC8648BDD064A6A8BE8407B930B624B2C63ADD8668C98910A6BDFBA3410D1ACC` |
| `GravityBackpackSnapshot.cs` | `3E9A83E1DB767761AEA5F29AF5859BD2B71C3BD2B859C2ACF4DADB5D89D7BCEC` |
| `GravityBackpackResolver.cs` | `2F3CEE92EE9B39F69E10202AFF41C35E49444B7ABD4764AFBAB39D2E7117304C` |
| `GravityBackpackRuntimeAdapter.cs` | `AF771D2B10A307B91CEAAF63E151531D17B73C0245B0171A4F50D846388A19FE` |
| `GravityBackpackPhase1PrototypeTests.cs` | `37BEA399EF283FE8186AF9EADC27779517F351477AE6C9B06E39B8D0A9C276F0` |
| `BattleSandbox Scene` | `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA` |

Additional read-only references:

| Path | SHA-256 |
|---|---|
| `BuildItemPreviewCardView.cs` | `448A69762EC157214809E1D43CD19A134D77F7928742CC890A94A41B1588D56E` |
| `ItemLivingGradientOutlineVfx.cs` | `5C7C97CBA93EE37A1DF109CAED4A1E9AC9515A31F52E9A1947128BC93FDBAC68` |
| `UI_ItemLivingGradientOutline.shader` | `6191B5F231B099E317BC7F638592E384FF62C07A97D12DAB9DC3ACA70F938E6E` |

## 15. Process And Completion

- Reuse the existing idle Gravity Backpack development task.
- Development remains silent until terminal, verified conflict, boundary
  expansion or unavoidable user decision.
- Code work may proceed while Scene/Prefab serialization remains untouched.
- The single Fresh Play requires a clean Unity window and package-owned process
  cleanup.
- No Git operation.

Stop at:

`COMPLEX_GUARDED_ONCE / USER_HANDTEST_WAITING`

User PASS closes:

`COMPLEX_GUARDED_ONCE / PACKAGE_COMPLETE`

Same-package planar feel defects remain in the same development task. Real
device tilt, resources, search, formal Inventory, Drop/Save/Reward and formal
promotion remain separate user-reviewed work.
