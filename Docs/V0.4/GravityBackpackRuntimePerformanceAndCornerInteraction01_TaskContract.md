# V0.4-GravityBackpackRuntimePerformanceAndCornerInteraction01 Task Contract

Status: `FROZEN_FOR_SINGLE_VISIBLE_TASK`

## User handtest result and outcome

The relative tabletop scale direction is accepted, but the first normal-Editor
handtest found two blocking defects:

1. the 31-item tabletop simulation is visibly stuttery;
2. the three-cell corner item (`I010`, `shape_corner3`) cannot be reliably
   selected/dragged.

This package optimizes the accepted planar tabletop and repairs contour-aware UI
input. It does not redesign the backpack, change item sizes, or add a test panel.

## Classification

- `CONTAINED_ONE_GUARD`
- Primary Guard: Item Guard
- Development: continue the existing visible tabletop task, renamed for this
  package; do not create or reuse a hidden/archived task
- QA: offline compile + deterministic focused tests; user performs the only
  normal-Editor Play handtest

## Read-only root-cause evidence

### Performance

The current steady physics path does avoid work only after every body sleeps,
but while bodies are moving it performs all of the following every fixed tick:

- `4` substeps x up to `18` solver passes;
- every pass scans every body pair (`31 choose 2 = 465`), even when their AABBs
  are far apart or both bodies sleep;
- every pair can repeatedly transform contour vertices and allocate a two-source
  enumerable in SAT;
- `Tick` fully validates the incoming snapshot, then fully validates the outgoing
  snapshot again, including another all-pairs contour-overlap scan;
- every outgoing immutable snapshot eagerly sorts, allocates lookup dictionaries,
  formats a full canonical string signature, and the runtime adapter compares that
  signature again;
- every changed physics tick publishes through the full tray transaction, rebuilding
  placement arrays, inventory sets and all card views when only position/rotation
  changed.

This is algorithmic fixed-tick overhead, not a PNG/VFX/import problem.

### Three-cell corner interaction

Physics now uses the accepted Sprite-derived contour, but UGUI input still relies
on rectangular `Image` raycasts. In planar mode `TrayLayoutCellLayer` is hidden;
cards that previously depended on those cell images can lose their hit surface.
Conversely, a transparent rectangular image area can steal input from a visible
neighbor. The existing tests exercise resolver hold/move by item ID, not the actual
UGUI contour hit path, so this escaped offline QA.

## Frozen implementation

### A. Runtime performance

1. Preserve the immutable public `PlanarTabletopSnapshot` contract and all accepted
   physical behavior, wall bounds, shared scale, capacity and contour identity.
2. Add a bounded broad phase (uniform spatial bins or equivalent deterministic AABB
   candidate generation). SAT may run only for candidate pairs whose bounds can
   overlap. Candidate ordering must be stable by identity/index.
3. Skip sleeping-sleeping contact pairs. A moving/held body contacting a sleeping
   body must still wake and resolve it.
4. Cache per-body trigonometry, world vertices and AABB for a solver pass/substep;
   invalidate only after position/rotation correction. Remove per-pair `new[]`, LINQ
   and enumerable allocations from the hot SAT path.
5. Use a fast internal structural check for already-accepted snapshots at `Tick`
   entry. Full semantic/no-overlap validation remains mandatory at initialization,
   external restore, membership commits and explicit verifier calls.
6. Build the immutable post-tick snapshot once. Canonical signature generation may
   become lazy because snapshots are immutable; transactional paths that read the
   signature retain byte-identical deterministic semantics.
7. The adapter trusts `PlanarTabletopResolveResult.Changed` for a physics tick and
   must not eagerly compare canonical strings again.
8. Add a motion-only presentation publication path: steady physics updates only the
   existing card position/rotation/accepted shared scale. Full tray transactions are
   reserved for membership/filter/hold-remove-return changes.
9. Keep the current physical tuning unless a focused test proves that one bounded
   reduction of substeps/passes is required. Do not solve performance by disabling
   collision, allowing overlap, reducing the roster, hiding items or changing scale.

### B. Contour-aware input

1. Planar mode gets one explicit card-root input owner plus an `ICanvasRaycastFilter`
   (or equivalent zero-allocation contour filter) configured from the same prepared
   normalized Sprite proxy used by physics.
2. A pointer on visible Sprite contour is accepted; a pointer in transparent empty
   space is rejected so the EventSystem can continue to a neighboring visible item.
3. Hidden legacy cell renderers and presentation-only artwork images must not remain
   competing raycast owners in planar mode.
4. The filter must follow the card's shared scale, rotation and movement without
   rebuilding contour geometry per frame.
5. Missing/invalid contour fails closed for that card and emits one bounded diagnostic;
   it must not fall back to a full rectangular blocker.
6. This repairs `I010` and every irregular/long/multi-cell item through the common
   input path; no item-ID exception is allowed.

## Allowed writes and dispatch baselines

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs` | `974DF06E9C2F2A0CECF8407C2303C38A314D9F796F08FAA1826BCF7A2F1B32C2` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackSnapshot.cs` | `C1E06179C7EE11090D50FD23FC1607047938E0930F12C7B86230F374DCA57BE5` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs` | `35464DE89C9E7D4E03EFE67A6CA431C1DE987869591E634D518C9B10D4DD8AFE` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `5CF46F1EB898C69FC7A49C8819E38A7BC156A253027B8BE33632EFF14CBFEBDC` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `AA0AE1569D413CEC1880D3B3EADD6B19B900509AD8E94B9AE6B8AE6E9A49858F` |
| `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs` | `B6682912F51E662EB3C9740DF91AEA7E08133909B0E551414C60FDFD80E846D5` |

Prefer six files or fewer. `GravityBackpackSnapshot.cs` is allowed only for lazy,
thread-confined immutable signature/canonical optimization; no schema or field
semantics change.

## Protected

- Scene, Prefab, PNG/meta/import settings and all authored layout;
- current shared scale `0.8477369`, target area `0.47499996`, item proportions,
  capacity `63/80` and 31-item roster;
- ItemSystem/Board/data/shape truth, ItemDetail, VFX, Battle, Enemy, Save, Drop,
  Reward and formal flow;
- tray-to-board and board-to-tray transaction semantics;
- no test/debug panel, no Scene runtime fallback hierarchy, no Git;
- do not start Unity, Fresh Play, batch or a second Editor. The user owns final Play.

## Focused verification

1. Offline Runtime and Editor compile.
2. Deterministic 31-body test proves no overlap/escape and exact scale/capacity after
   movement, sleep, drag cancel, remove and return.
3. Instrumented hot-path test proves broad-phase candidates are materially below
   brute-force pairs in the representative scattered roster; sleeping-sleeping pairs
   and full input-snapshot overlap validation are absent from steady Tick.
4. Repeated 600-tick benchmark records elapsed time and allocation counters as evidence;
   do not use a fragile machine-specific absolute time as the sole pass condition.
5. Contour input fixture proves an `I010`-style L/corner shape accepts points in all
   three visible arms, rejects the missing transparent corner, and allows an underlying
   visible neighbor to receive that rejected point.
6. No runtime collection/material/GameObject growth across 20 drag/cancel cycles.
7. Return only `USER_MANUAL_HANDTEST_READY` with a short two-part checklist. No Unity
   automated QA and no monitoring loop.

## User handtest

In one fresh Play:

1. let all 31 items move and settle; confirm the Editor remains responsive and motion
   is visibly smooth;
2. drag/shake several items for 20-30 seconds; confirm no escalating stutter;
3. select and drag I010 from each of its three visible arms; its empty corner must not
   block the item underneath;
4. move I010 to Board and back, then repeat; size, collision and input stay aligned.
