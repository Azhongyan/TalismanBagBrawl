# V0.4-GravityBackpackTabletopScaleSeparationRecovery01 Task Contract

Status: `FROZEN_FOR_SINGLE_DISPATCH`

## Outcome

Correct the tabletop size model after
`PLANAR_TABLETOP_INITIAL_PACKING_BLOCKED`:

- keep `63/80` as logical capacity only;
- uniformly shrink all tabletop item artwork and collision proxies;
- preserve every item's aspect ratio and relative size differences;
- use the exact same scale for the visible card and its collision proxy;
- keep Board presentation/size unchanged and restore tabletop scale on return;
- fit the 31-item reference roster with enough free space to move.

Accepted marker:
`USER_APPROVED_RELATIVE_TABLETOP_DOWNSCALE`.

## Classification

- `CONTAINED_ONE_GUARD`
- Primary Guard: Item Guard
- Development: one new visible task
- Prior V2 proxy task: `ARCHITECTURE_FAIL`; it must not be reused

## Frozen sizing model

1. Capacity cost and physical area are independent.
2. Derive each item's unscaled tabletop proxy from its accepted artwork geometry.
3. Compute one session-stable uniform scale from the initial authoritative 31-item
   roster. Target aggregate proxy area: 45%-50% of the 100x140 tabletop area.
4. Clamp the shared scale to a conservative bounded range. Never use per-item scale
   exceptions and never normalize all items to one cell.
5. Freeze that scale for the session. Removing, moving or returning an item must not
   resize other items.
6. Apply the same scale to visual RectTransform projection and collision contour.
7. Initial packing uses the scaled proxies. It must report the first unplaceable
   identity plus scale/area metrics if packing still fails.

No changes to the accepted bounded convex-proxy algorithm are allowed in this
package. This package changes coordinate/scale mapping only.

## Allowed writes and dispatch baselines

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `974c6ac156b59da984e63fa03c2de0533eb125076a738abc428031a5026ae261` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `5cf46f1eb898c69fc7a49c8819e38a7bc156a253027b8be33632eff14cbfebdc` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackResolver.cs` | `28ec45d63c59b946615799a34c1c8a5762176fa7c4ec023a8cc79555d51abbd6` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/GravityBackpack/GravityBackpackRuntimeAdapter.cs` | `6a27dea4004388dc3a314fa50f0218033d556a28e68d612849b2d6c47d564b6d` |
| `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/GravityBackpackPhase1PrototypeTests.cs` | `3bc062ef6b90d923b604dd45f7e98a572521ff4f64ece36bff0dd00eb9e9ed36` |

Use at most these five files and preferably three or fewer.

## Protected

- Scene, Prefab, PNG/meta/import settings;
- ItemSystem/Board/data/shape/capacity truth;
- ItemDetail, VFX, Battle, Enemy, Save, Drop, Reward and formal flow;
- exact proxy algorithm, solver passes/substeps and tabletop wall dimensions;
- ProjectSettings, Packages, BuildSettings, AGENTS, LOCKED, Notion and Git.

## Verification

- offline Runtime/Editor compile;
- non-Unity tests proving one shared scale, 45%-50% target area, relative-size and
  aspect-ratio preservation, session stability and 31-item initial packing;
- no Unity/Fresh Play/batch/harness;
- return `USER_MANUAL_HANDTEST_READY` for one user Play.

User checks:

1. Play remains responsive and no `INITIAL_PACKING_BLOCKED` appears.
2. All 31 items are visible and have comfortable free tabletop space.
3. I009/I010/I016/I018/I031 retain visibly different proportions.
4. Visual edges and collision edges stay aligned while dragging.
5. Tray-to-Board and Board-to-Tray do not permanently change size.

Any further failure that requires changing collision architecture stops at
`ARCHITECTURE_FAIL`; no additional structural patch in this task.

