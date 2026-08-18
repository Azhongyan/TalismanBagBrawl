# V0.4-ItemDevSessionRosterAvailability01 Task Contract

Status: `CONTAINED_ONE_GUARD / BOUNDARY_FROZEN`

## 1. Outcome

Provide one Item-owned, immutable, dev-session roster availability contract for
the approved CoreLoop A/B/C Lab:

- the Lab establishes an authoritative initial available Item roster for one
  `PLAYTEST_VERTICAL_SLICE` session;
- accepting one fixed Lab reward makes exactly that existing catalog Item newly
  available in the next arrangement;
- duplicate delivery is an idempotent no-op;
- reset or a new Lab session removes only the temporary additions and restores
  that session's initial roster;
- the tray republishes atomically from the Item-owned immutable availability
  snapshot;
- no formal Inventory, Save, Drop or Claim state is created.

Dependency consumer:
`V0.4-CoreLoopABCProductDirectionLab01`.

## 2. Product Context

- Approved host: `Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab`
- Context: `PLAYTEST_VERTICAL_SLICE`
- Original `Scene_TalismanBag_V04_BattleSandboxPreview` remains
  `DEV_SHOWCASE_LV40` with the complete I001-I031 roster and unchanged behavior.
- Player/APK/formal Campaign hosts are rejected.
- Scene and UI are read-only for this package.

## 3. Task Class And User Review

- Class: `CONTAINED_ONE_GUARD / TECHNICAL_PREREQUISITE`
- Classification reason: one Item-owned state seam and one Item controller
  publication seam; no cross-owner write, persistence, balance or presentation
  decision.
- User Review Gate: `NO`
- Marker: `NOT_REQUIRED / USER_TECHNICAL_AUTHORIZATION_GRANTED`
- Primary Guard: Item Guard
- Development Owner: one new visible, unarchived local Item development task
- User Decision Point: `NONE`

## 4. Ownership

- `ItemSystemBattleSandboxBoardAuthority` is the sole owner of authoritative
  catalog rows, session availability, Board membership and placement state.
- `BuildGridInteractionPreviewController` validates the actual dev host/context
  and atomically publishes the authority's immutable available roster to the
  existing tray.
- CoreLoop/Mainline may construct requests and consume immutable results only.
- Scene/UI must not hide, show, clone or manufacture cards to emulate roster
  ownership.
- Config/catalog is read-only and remains the source of valid base Item IDs and
  artwork facts.

## 5. Exact Allowed Writes

Modify only:

1. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs`
2. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs`

Add only:

3. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemDevSessionRosterAvailabilityTests.cs`
4. `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemDevSessionRosterAvailabilityTests.cs.meta`

The Task Contract is Guard-owned and must not be edited by the development
task. No report file is required.

## 6. Forbidden Writes

- `ItemSystemBattleSandboxBoardAdapter.cs` and its approved two-host installer
  contract;
- `BuildItemTrayPreviewView.cs`, card views, tray grid/layout, placement and
  rotation algorithms;
- ItemSystem snapshot schema, Item catalog/shape/artwork/rarity/Build/Core/Roll;
- CoreLoop Scene/scripts/configs/prefabs and all other Scene/Prefab files;
- original BattleSandbox Scene, Unified Battle, BuildSettings and
  ProjectSettings;
- formal Inventory, Save, Drop, Reward, Claim, Campaign and RunFlow;
- Battle timing/application, Enemy, VFX and presentation;
- Gravity/Tabletop archives and retired runtime paths;
- AGENTS, LOCKED, Queue, Notion and Git operations.

## 7. Required Sources

Read only:

- `AGENTS.md`, Engineering Process V2 lock and current engineering state;
- this Task Contract;
- the two allowed runtime files;
- `ItemSystemBattleSandboxBoardAdapter.cs` for the existing approved host and
  product-context guard;
- `BuildItemTrayPreviewView.cs` only to use its existing atomic publication API;
- `ItemSystemSnapshot.cs` and `ItemInnerDataCatalog.cs` as read-only truth;
- existing ItemSystem focused verifier conventions as needed.

Do not read historical Gravity/Tabletop implementation as a design source.

## 8. Patch Budget

- modified runtime files: exactly 2 maximum;
- new Editor test files: exactly 2 including `.meta`;
- new runtime files: 0;
- Scene/Prefab files: 0;
- reports: 0.

Any need for an additional product/runtime file, formal inventory migration,
UI ownership or placement algorithm change stops at
`ARCHITECTURE_REASSESS_REQUIRED`.

## 9. Immutable Contract

The Item authority exposes one immutable request/result/snapshot contract.
Names may follow local conventions, but the facts are mandatory.

Request:

- operation: `Reset` or `AddOnce`;
- approved dev host ID;
- product context `PLAYTEST_VERTICAL_SLICE`;
- non-empty session token;
- positive reset generation;
- authoritative base Item ID for `AddOnce`;
- for `Reset`, an ordered or order-independent immutable initial available
  base-Item-ID set supplied by the Lab and validated by Item authority.

Result:

- `Accepted`;
- `Changed`;
- stable diagnostic code;
- immutable current availability snapshot.

Snapshot:

- schema/version marker;
- approved host and product context;
- session token and reset generation;
- immutable canonical initial roster;
- immutable canonical current available roster;
- deterministic canonical signature.

The authority interface exposes the current availability snapshot and one
operation method. It must not expose mutable collections.

## 10. Required Behavior

1. With no dev-session scope, authority availability is the complete existing
   row roster. Original BattleSandbox installation therefore remains I001-I031.
2. A valid Lab `Reset` establishes a canonical initial subset using only unique
   authoritative `Rows` identities. Item authority becomes the sole committed
   owner of that session roster.
3. Repeating the same `Reset` restores that exact initial roster and removes
   temporary additions. A higher generation starts a new session state. A stale
   generation or conflicting same-generation baseline is rejected unchanged.
4. A valid `AddOnce` adds exactly one existing catalog/authority row not already
   available. The identical request is accepted with `Changed=false`.
5. Unknown host/context/item, empty or wrong token, stale generation, duplicate
   IDs in a reset baseline, and malformed requests fail closed without changing
   availability, placements or signatures.
6. An unavailable Item cannot be previewed or committed from the tray. Existing
   valid Board placements continue to use current placement/move semantics.
7. Reset removes only placements for session-added Items that cease to be
   available, then rebuilds ItemSystem derived snapshots atomically. Failure
   preserves both the previous roster and placements.
8. The controller provides one public Lab consumer seam. It additionally
   requires Editor runtime, the exact approved Lab Scene path, the approved
   product context and the already-installed Item authority before forwarding
   a request.
9. After an accepted changed result, the controller republishes the tray from
   `AvailableBaseItemIds` in one existing tray transaction. Publication failure
   restores the previous tray projection and does not mutate authority a second
   time or create a fake card.
10. Controller install, accepted-snapshot rebuild, reset and return paths must
    intersect tray membership with the current immutable availability snapshot
    so unavailable rows cannot reappear through a later refresh.
11. Item artwork, identity, placement, rotation, occupied cells and drag math
    continue through existing authority rows and controller paths unchanged.
12. The operation is memory-only. Scene/session restart reconstructs the
    default full authority, after which the Lab must establish its initial
    subset again. No persistence hook is permitted.

## 11. Failure And Edge Cases

- Authority not installed: reject, no publication.
- Wrong Scene/host/context: reject, no authority mutation.
- Unknown or unavailable reward ID: reject, no placeholder/fallback card.
- Duplicate `AddOnce`: accepted no-op, stable signature.
- Stale token/generation: reject, stable signature.
- Reset while a temporary Item is on Board: atomically remove only that
  temporary placement together with its availability; preserve unrelated
  placements and derived state.
- Publication exception: restore previous tray view; expose an explicit
  diagnostic and do not create a second roster owner.
- Reload/new authority instance: no retained Lab additions.

## 12. Task-Start Baselines

Allowed existing files:

| Path | SHA-256 |
|---|---|
| `ItemSystemBattleSandboxBoardAuthority.cs` | `6BEF274DA275D26AFAD97E6ECBC6EC61523E602D8CC000744FD290870BFD9A20` |
| `BuildGridInteractionPreviewController.cs` | `74C73D47175342F8298D05373D3A5A0BECF74D52F1F0632C15F915E12F229D44` |

New files must be absent at task start:

- `ItemDevSessionRosterAvailabilityTests.cs`
- `ItemDevSessionRosterAvailabilityTests.cs.meta`

Protected baselines:

| Path | SHA-256 |
|---|---|
| `ItemSystemBattleSandboxBoardAdapter.cs` | `D9CCC39EA040BE05B911904F06B69A13CC9C7E47E215CB20B16724923FBD2009` |
| `BuildItemTrayPreviewView.cs` | `06AF79CB89EB4DCA0A2313E81AF81EFA89A35B90AE6FE5E6B0BBDD3C90F62921` |
| `ItemSystemSnapshot.cs` | `794C3A6D9DBE9FB1FF96275C247BCEF4D9D248EDC1DF3EAEE6D396DD87969827` |
| `ItemInnerDataCatalog.cs` | `606ACDC6538EEDA86F7631840A6ACBB47284368F198EF413293BB844833776C9` |
| `BattleSandbox Scene` | `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA` |
| `EditorBuildSettings.asset` | `D6F8DA39E5E609417D534E98C601BD78CEE97A499669E476170305E1E08A0255` |

The CoreLoop Lab Scene is concurrently owned by its Mainline development task.
It was observed at dispatch as
`5BFC3B5963FD44F0F885101ADAA20D0B32B5AE0A77AF09D3BC7794E1A0D6769B`,
but it is not a stable package baseline and must not be written, restored or
claimed by this Item task. Use exact write ownership and package-scoped status
to prove zero Item writes there.

Any drift in either allowed existing file before the first write is a verified
write conflict. Do not refresh it silently.

## 13. Verification

Minimum automated evidence only:

- scoped Runtime and Editor compile using the existing offline compile path;
- deterministic focused tests for Reset, AddOnce, duplicate no-op, canonical
  signature, stale/wrong token/generation/host/context/item rejection;
- placement test proving reset removes only a temporary addition and preserves
  unrelated placement state;
- controller test/static proof for exact Lab host gate, one authority call,
  one atomic tray publication and rollback on publication failure;
- original no-session full I001-I031 behavior unchanged;
- package-scoped whitespace/status and protected baseline checks.

Explicitly not required:

- Unity batch or Fresh Play;
- Scene/Prefab authoring;
- user handtest for this component package;
- CSV/report matrices.

The end-to-end reward and second-arrangement handtest belongs to the consuming
CoreLoop Lab package after dependency release.

## 14. Process Ownership

`UNITY_NOT_REQUIRED`

Do not start, wait for, poll, close or clean a user or external Unity process.
No Git operations.

## 15. Completion And Dependency Release

The component package self-closes when all frozen behavior and focused QA pass:

`ITEM_DEV_SESSION_ROSTER_AVAILABILITY_PASS / DEPENDENCY_RELEASE_READY`

Terminal sync must include:

- exact changed files;
- compile and deterministic test results;
- final public request/result/snapshot/controller API names;
- protected baseline result;
- known limitations, if any;
- no Unity/Git confirmation.

Then send one direct dependency-release sync to:

- Mainline Flow Guard `019f45d7-a33b-7630-a049-976481bb7b6d`;
- Overall routing thread `019ef276-110c-7470-9342-72691805433d`.

Do not message the user or the active CoreLoop implementation task directly.
No receipt chain is required.

Stop at `ARCHITECTURE_REASSESS_REQUIRED` if formal Inventory/Save/Drop/Claim,
duplicate roster truth, UI hide/show ownership, placement/shape algorithm
changes or any additional product runtime file becomes necessary.
