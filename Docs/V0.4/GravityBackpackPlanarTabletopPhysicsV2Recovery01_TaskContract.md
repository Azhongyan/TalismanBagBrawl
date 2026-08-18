# V0.4-GravityBackpackPlanarTabletopPhysicsV2Recovery01 Task Contract

Status: `FROZEN_FOR_SINGLE_DISPATCH`

## 1. Outcome

Recover the accepted planar tabletop backpack without freezing Unity:

- keep the authoritative 31-item roster and current artwork sizes visible;
- remove the unbounded exact Tight-mesh triangle coalescing path;
- build one small, deterministic outer-silhouette collision proxy per unique Sprite;
- prepare at most one unique Sprite proxy per frame, then activate the 31-body simulation once;
- preserve current tabletop walls, no-overlap behavior, damping, dragging and atomic Tray/Board ownership;
- require one user-run Play handtest; no automated Unity run.

This package rewrites only the devOnly planar collision-proxy layer. It does not
rewrite ItemSystem, Board authority, artwork, the Scene or formal Inventory.

## 2. Product decision and tradeoff

The collision proxy intentionally approximates the artwork's **outer silhouette**.
It must preserve width/height proportions and visible protrusions, but it does not
promise pixel-perfect concave notches, tiny transparent holes or every alpha edge.

This replaces the rejected exact compound-mesh design. Runtime responsiveness and
stable non-overlap are higher priority than mathematically exact alpha geometry.

Accepted marker: `USER_APPROVED_PLANAR_TABLETOP_PHYSICS_V2_RECOVERY`.

## 3. Task class and ownership

- Class: `CONTAINED_ONE_GUARD`
- Primary Guard: Item Guard
- Development: one new visible Codex task
- Previous task `019fb63b-7944-7751-996e-97f241b19c36`: frozen/idle; no further writes
- Scene / Prefab serialization owner: none
- User decision point: none before the final handtest

## 4. Frozen technical design

### 4.1 Proxy source

For the final authoritative Sprite:

1. use usable `Sprite.GetPhysicsShape` vertices when available;
2. otherwise use usable `Sprite.vertices` from the Tight render mesh;
3. map through the already accepted artwork RectTransform dimensions;
4. reject non-finite and duplicate points.

Do not use item IDs, names, Board cells or a universal square/circle as geometry.

### 4.2 Bounded construction

- Build a deterministic monotonic-chain convex hull (`O(n log n)`).
- Reduce deterministically to at most 16 vertices.
- Produce exactly one convex proxy per unique Sprite/accepted geometry key.
- Cache the result; repeated instances may not rebuild it.
- No fixed-point merge, all-pairs polygon merge, recursive decomposition,
  string-heavy candidate sorting, exact triangle union or unbounded loop.
- Add explicit structural guards for source vertex count, output vertex count and
  iteration count. Any guard failure returns one concise diagnostic.

### 4.3 Startup and failure behavior

- Never synchronously build all 31 proxies inside Awake/OnEnable/one publication call.
- All 31 cards and their authoritative artwork become visible first.
- Prepare at most one unique Sprite proxy per frame.
- Do not start tabletop simulation until the complete roster has valid proxies.
- A failed proxy leaves the full roster visible, does not enter simulation and emits
  one identity-scoped diagnostic. It must never hang, poll forever or clear the tray.
- No runtime debug/test panel.

### 4.4 Runtime budget

- One convex part and at most 16 vertices per body.
- Existing 31-body simulation remains the only planar physics owner.
- Do not increase solver passes, substeps or memory pools in this recovery package.
- No per-frame proxy rebuilding or material/object growth.

## 5. Allowed writes

Existing files at dispatch baseline:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `2b45b7d06baf7af3d7af9faa40527fecc50c3f7af0071ea51d84c863dcbdcc4b` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `3561cb7c801371541201ef8928cffee1dfe05329b52c08900c0386fc5c530eda` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs` | `6ab5da4dbfe72b94e6f53a4e2c73c9ed7efa1ac86107111e6964679adddd36c5` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs` | `6a27dea4004388dc3a314fa50f0218033d556a28e68d612849b2d6c47d564b6d` |
| `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs` | `66351d8258c558bd230bc842b177f41cb2555733d6238da316e83f2db0089cad` |

Optional new files:

- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/PlanarTabletopCollisionProxyBuilder.cs`
- matching `.meta`

Use fewer paths where possible. No other path may be written.

## 6. Protected areas

- all Scenes, Prefabs, PNGs, metas/import settings outside the optional new meta;
- `GravityBackpackSnapshot.cs` and ItemSystem/Board authority/data/shape truth;
- ItemDetail, Rarity VFX, Formation VFX and Combat VFX;
- Enemy/Battle/Save/Drop/Reward/RunFlow/formal flow;
- ProjectSettings, Packages, BuildSettings, AGENTS, LOCKED, Notion and Git.

No rollback/reset/checkout from Git. Replace only the rejected collision algorithm
inside the allowed current disk state.

## 7. Verification

Development verification is limited to:

- scoped Runtime and Editor compile;
- focused non-Unity tests for hull determinism, 16-vertex cap, degenerate rejection,
  cache reuse, source-order/winding invariance and bounded iteration;
- static check proving the exact coalescer is unreachable/removed;
- no Unity launch, Fresh Play, batch verifier or ten-cycle harness.

The test must include a high-density synthetic source and assert a bounded operation
count. A correctness-only test without a bound is insufficient.

## 8. User handtest gate

Return only `USER_MANUAL_HANDTEST_READY`, then ask the user to:

1. open Play and confirm the Editor remains responsive;
2. confirm all 31 items appear before physics activation;
3. confirm I009, I010, I016, I018 and I031 retain distinct size/proportion;
4. drag and release several items: no overlap/wall escape and quick settling;
5. verify Tray-to-Board and Board-to-Tray once.

If Play still hangs, items disappear, or the whole simulation is disabled by one
proxy, stop at `ARCHITECTURE_FAIL`; do not add another same-window structural patch.

## 9. Process gate

Current user Unity PID 26520 was observed hung in Play. The development task may do
read-only inspection while it exists but must not edit C# until the user has closed
that Editor. It must never terminate or manipulate that PID.

No task-owned Unity process is permitted for this package.

