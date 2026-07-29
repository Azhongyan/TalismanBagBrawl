# V0.4-BattleSandboxBoardCoordinateParityGuardFix01 Assignment

Package:

```text
V0.4-BattleSandboxBoardCoordinateParityGuardFix01
```

Parent packages:

```text
V0.4-BattleSandboxTrayArtworkDirectionGuardFix01
USER_HANDTEST_FAIL / FINAL_ACCEPTANCE_HOLD

V0.4-BattleSandboxTrayPhysicalOccupancyParityGuardFix01
STATIC_GATE_ACCEPTED / USER_HANDTEST_FAIL_ON_BOARD_PHYSICAL_DIRECTION
```

Primary Guard:

```text
Item System Guard
```

Overall authorization:

```text
DIRECT_OVERALL_GUARD_FIX_REQUEST
```

Target receipt:

```text
ITEM_GUARD_PASS_BATTLESANDBOARDCOORDINATEPARITYGUARDFIX01
```

## 1. User-Visible Failure

The tray artwork and tray physical occupancy are now correct.

User handtest:

```text
I004 / I005: PASS
I010 / I016 tray visual direction: PASS
I010 / I016 tray physical hit/reservation: PASS
I010 / I016 board physical occupied corner after drop: FAIL
```

The corner artwork and tray cells agree, but the BattleSandbox board occupies
the vertically mirrored corner after drop.

The parent packages remain on final acceptance hold until this package passes
user handtest.

## 2. Read-Only Evidence and First Missing Layer

Authoritative Item shape truth is unchanged and correct:

```text
I010 / I016
shapeId = shape_corner3
local cells = (0,0);(1,0);(0,1)
```

The BattleSandbox board Scene uses:

```text
GridLayoutGroup.startCorner = UpperLeft
BoardGridCell_01 = x:0, y:0
BoardGridCell_06 = x:0, y:1
BoardGridCell_11 = x:0, y:2
```

Therefore the authored board slot Y values increase from top to bottom.

The Item/ItemSystem coordinate contract remains canonical data coordinates,
where positive local Y is projected upward on a top-down visual board.

The current controller does the opposite:

```text
ResolveBoardDataCellFromVisualCell()
top-down visual rows -> returns visual cell unchanged
non-top-down rows     -> flips Y
```

This branch is reversed.

The same wrong lookup feeds:

```text
visual pointer hit
-> board data anchor
-> board receiver CanPlace/collision
-> ItemSystem authority preview/commit
-> ItemSystemSnapshot.v2 OccupiedCells
-> accepted snapshot cache rebuild
-> board placed visuals and drag source
```

First missing layer:

```text
BuildSandbox board visual/data coordinate boundary
```

## 3. BoardAuthority Decision

`ItemSystemBattleSandboxBoardAuthority.cs` is NOT writable in this package.

Reason:

1. BoardAuthority receives canonical anchor/rotation.
2. It rebuilds placements through the accepted Item catalog and
   ItemSystemSnapshot provider.
3. ItemSystemSnapshot validation requires canonical catalog cells plus
   rotation.
4. BoardAuthority collision and accepted snapshot state are correct in
   canonical Item coordinates.
5. Modifying BoardAuthority to emit visual coordinates would duplicate and
   contaminate Item shape truth.

The correct fix is an explicit boundary conversion in
`BuildGridInteractionPreviewController.cs`.

## 4. Frozen Coordinate Contract

Establish:

```text
ONE_AUTHORITATIVE_BOARD_COORDINATE_BOUNDARY
```

Canonical rules:

```text
Item catalog local cells: unchanged
ItemSystem anchor/OccupiedCells: canonical data coordinates
BoardAuthority: canonical data coordinates
UpperLeft Unity board slots: visual top-down coordinates
```

Boundary conversion:

```text
visualCell.x = dataCell.x
dataCell.x   = visualCell.x

for UpperLeft / UpperRight visual rows:
dataCell.y   = BoardRows - 1 - visualCell.y
visualCell.y = BoardRows - 1 - dataCell.y

for LowerLeft / LowerRight visual rows:
dataCell.y   = visualCell.y
visualCell.y = dataCell.y
```

The mapping is a deterministic involution and must be the single conversion
used by:

```text
authored slot lookup
pointer hit
soft-gap/bounds fallback hit
board preview
collision
commit
accepted ItemSystemSnapshot redraw
board item click
board drag source
drag ghost
move and rotation
rejection rollback
return-to-tray transition
```

Do not mirror Item local cells and do not write presentation cells into
ItemSystemSnapshot.

For a canonical rotation-0 corner at data anchor `(1,1)`:

```text
canonical ItemSystem cells:
(1,1);(2,1);(1,2)

visual UpperLeft board slots:
(1,3);(2,3);(1,2)

normalized visual footprint:
(0,0);(0,1);(1,1)
```

That normalized visual footprint must match the accepted tray footprint.

## 5. Task-Start Stop Gate

The Assignment was frozen only after:

```text
Unity/Tuanjie/UnityShaderCompiler/bee_backend processes: 0
Temp/UnityLockfile: ABSENT
Library/EditorInstance.json: ABSENT
BattleSandbox Scene repeated SHA reads: STABLE 3/3
```

Task-start Scene:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
SHA-256:
329696dde34e5f6569ca69b4bde926a25ca95e4b3d2f2158c198d26264e6a441
```

The Scene is protected and must remain byte-identical.

Task-start external git-status evidence:

```text
rows: 1258
Ordinal status digest:
7382505cdfe1f630fae26bd943e2d9a4b73c6c6cdea20cfe4d2f48c1702bc959
```

This digest is attribution evidence only. Do not clean, revert or claim
unrelated dirty/untracked files.

Stop immediately if:

1. Unity/Tuanjie or another batch process is active at task start.
2. UnityLockfile or EditorInstance exists.
3. The target Scene hash differs from the frozen value.
4. The allowed-existing source hash differs from section 7.
5. The fix requires BoardAuthority, Scene, Prefab, Item shape or PNG changes.

## 6. Exact Existing-File Write Whitelist

Only this existing runtime file may change:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs
```

Allowed changes:

1. Correct the visual-to-data Y conversion for top-down board grids.
2. Make the same conversion available to the board receiver bounds/gap
   fallback path.
3. Use one generic conversion helper; do not duplicate divergent formulas.
4. Keep authored-slot dictionary keys in canonical data coordinates.
5. Keep pointer, preview, collision, commit, snapshot redraw and board drag
   source on that same canonical key space.
6. Preserve null handling, deterministic ordering and current 5x5 dimensions.
7. Keep the mapping generic; no `itemId`, `shapeId`, `I010` or `I016` branch.

No other existing runtime file is writable.

## 7. Allowed-Existing Task-Start Hash

```text
BuildGridInteractionPreviewController.cs
531894cb6b36e6ef84afc93cb15918d4324b5542094dc3c5ddaa3915115e5aff
```

Development must use the current disk file as the baseline. Do not restore it
from Git HEAD.

## 8. Exact New-File Whitelist

Verifier:

```text
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxBoardCoordinateParityGuardFixVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxBoardCoordinateParityGuardFixVerifier.cs.meta
```

Reports:

```text
Docs/V0.4/Reports/BattleSandboxBoardCoordinateParityGuardFixReport.md
Docs/V0.4/Reports/BattleSandboxBoardCoordinateParityGuardFixSpec.csv
Docs/V0.4/Reports/BattleSandboxBoardCoordinateParityMatrix30.csv
Docs/V0.4/Reports/BattleSandboxBoardPhysicalInteractionMatrix.csv
Docs/V0.4/Reports/BattleSandboxBoardCoordinateParityManualTest.md
Docs/V0.4/Reports/BattleSandboxBoardCoordinateParityGuardFixLeakCheckReport.md
```

No Docs `.meta` files are allowed.

Exact implementation/report count:

```text
1 modified existing runtime file
2 new verifier files
6 new report files
9 implementation/report files total
```

The Assignment is Guard-owned and is not part of the implementation count.

## 9. Protected Files and Task-Start Hashes

The following must remain byte-identical:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
329696dde34e5f6569ca69b4bde926a25ca95e4b3d2f2158c198d26264e6a441

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs
d232a34f6a761d745564c5015c977f9eef26e2c0d23cd764b6cfa2704d008214

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapePlacementSession.cs
9d3eb2b209343f013deb1ec8ed062f85f51b3cfca4530b6e28dcc615348390a5

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs
da8c1b628a02565908e7bc7a0ef9e56dc2e00bec25af549df9382769372665bc

Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs
a4075e2279bb937858be97a5e41365ce0eff58f12277f0d19b93b7efac29b590

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs
6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs
116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs
ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec

Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs
794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827

Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs
606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs
5e0f1f59d11ef6607af586a6b7fb7865c52e04db02bc5b2b7a4607a71aa6450f

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs
29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21

Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier.cs
bf84243833adae743d26efb8972b66df5b9eb64cf5f7db067af831bbdc4c5198

Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixReport.md
c3d78759a2be6e337e1ef15e326083eeff8a62e3552179a6bab70867ba229ebe

Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixSpec.csv
521798ca3676143c2d49f745601da92a83d764d940874e7a1d8ccf699e0846ac

Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29

Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab
2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c

ProjectSettings/EditorBuildSettings.asset
08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59

Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionSpec.csv
e94ab078b92b98f02b98af049185dc8340b17c6448b0ae4fe0fa9364966da698
```

Also protect:

- all Item PNGs, Sprites and import settings;
- all other Scenes, Prefabs and UI geometry;
- previous tray GuardFix implementation, verifier and reports as historical
  evidence;
- Item shape/catalog, Roll/RNG/probability, Build qualification and core data;
- EnemySystem, EncounterPresentation and BoneAspect work;
- RunFlow, SaveData, Reward, Chapter, Boss and formal Battle;
- `AGENTS.md`, `Docs/LOCKED/*` and Package Queues.

## 10. Required Runtime Verification

The dedicated verifier must exercise production runtime classes and the real
BattleSandbox Scene in memory. It must not synthesize expected values and
report them as runtime results.

### 10.1 Scene Coordinate Proof

Assert from the real target Scene:

```text
board slot count = 25
coordinates are unique and cover x/y 0..4
GridLayout startCorner = UpperLeft
BoardGridCell_01 serial cell = 0,0
BoardGridCell_06 serial cell = 0,1
BoardGridCell_11 serial cell = 0,2
```

Assert the runtime lookup maps:

```text
visual 0,0 -> data 0,4
visual 0,4 -> data 0,0
visual 4,0 -> data 4,4
visual 4,4 -> data 4,0
```

The map must be bijective and reversible for all 25 cells.

### 10.2 I010 / I016 Exact Board Path

For each item at rotation 0, use a non-edge legal visual drop anchor and the
real controller/receiver/authority path.

Assert:

1. Tray normalized physical/visual footprint remains
   `0,0;0,1;1,1`.
2. Pointer authored-slot hit returns the correct canonical data anchor.
3. Pointer gap/bounds fallback returns the same canonical data anchor.
4. Drag ghost visual slots are the tray-equivalent corner.
5. Board cache preview and ItemSystem authority preview agree.
6. Collision blocks each of the three visible cells.
7. The visual hole does not block, reserve, click or begin a board drag.
8. Accepted commit succeeds.
9. ItemSystemSnapshot.v2 stores canonical cells from
   `anchor + rotated catalog local cells`.
10. Mapping those snapshot cells back to visual slots exactly matches preview,
    ghost, collision and placed artwork cells.
11. Board click and board drag can begin from all three visible occupied cells.
12. Board click and board drag do not begin from the hole.
13. Move to a second legal anchor rebuilds both snapshot and visual cells.
14. Rotation 90/180/270 preserves canonical snapshot rules and visual parity.
15. Rejected overlap and rejected out-of-bounds attempts preserve the prior
    snapshot reference/canonical signature and prior visible occupied cells.
16. Return to tray clears board snapshot/receiver/visual state and restores
    the accepted tray mapping.
17. Repeat tray -> board -> move -> reject -> return three times with no drift.

### 10.3 I004 / I005 Regression

Assert:

- tray direction and tray physical occupancy remain accepted;
- board rotation-0 visible footprint remains vertical;
- canonical snapshot cells remain vertical;
- artwork rotation remains absolute zero;
- drag, move, reject rollback and return do not reintroduce stale rotation.

### 10.4 Thirty-Item Rotation-0 Parity

For I001-I030:

```text
normalize(tray accepted occupied cells)
==
normalize(map canonical board occupied cells to visual board slots)
```

Also assert:

- occupied-cell count unchanged;
- canonical catalog/local cells unchanged;
- no duplicate, overlap or out-of-range board cells;
- preview, collision, commit, snapshot redraw and placed visuals use the same
  canonical-to-visual mapping;
- all 26 items outside I004/I005/I010/I016 preserve accepted behavior.

### 10.5 Rotation Regression

For every rotatable shape and supported rotation:

1. Apply existing canonical rotation only once.
2. Build canonical ItemSystem cells.
3. Project canonical cells through the board boundary.
4. Normalize visual board cells.
5. Compare with the accepted tray presentation mapping for that rotation.

Do not change rotation algorithms or create a second shape table.

### 10.6 Authority and Snapshot Isolation

Assert byte-identical protected hashes and prove:

```text
BoardAuthority source unchanged
ItemSystemSnapshot source unchanged
Catalog shape source unchanged
```

Assert ItemSystemSnapshot contains canonical data cells, not visual rows.

## 11. Historical Verifier Handling

The prior tray physical verifier remains immutable historical evidence.

Its `BOARD_DATA_COORDINATES_UNCHANGED` assertion proved only that the tray
package did not alter canonical board data. It did not prove board visual/data
projection parity and must not be used as the final board acceptance gate.

The new report must record:

```text
BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier
Disposition = RETAINED_TRAY_EVIDENCE_SUPERSEDED_FOR_BOARD_PARITY
```

Do not edit or overwrite the old verifier or reports.

## 12. Required Markers

```text
ONE_AUTHORITATIVE_BOARD_COORDINATE_BOUNDARY_PASS
UPPERLEFT_VISUAL_DATA_BIJECTION_25_PASS
POINTER_AUTHORED_AND_FALLBACK_PARITY_PASS
I010_I016_DRAG_GHOST_PARITY_PASS
I010_I016_BOARD_PREVIEW_COLLISION_PASS
I010_I016_ACCEPTED_COMMIT_SNAPSHOT_PASS
I010_I016_BOARD_HIT_AND_HOLE_PASS
I010_I016_MOVE_ROTATION_PASS
I010_I016_REJECTION_ROLLBACK_PASS
I010_I016_RETURN_TO_TRAY_PASS
I004_I005_REGRESSION_PASS
TRAY_TO_BOARD_ROTATION0_PARITY_30_PASS
ROTATION_REGRESSION_PASS
BOARD_AUTHORITY_UNCHANGED_PASS
ITEMSYSTEM_SNAPSHOT_CANONICAL_PASS
SCENE_BYTE_IDENTICAL_PASS
PNG_AGGREGATE_UNCHANGED_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_WAITING
```

Unity batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.BattleSandboxBoardCoordinateParityGuardFixVerifier.VerifyStaticBatch
```

Unity executable:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
```

Do not run Unity concurrently with another task or the user's Editor. Never
close another process.

## 13. Canonical and Determinism Boundary

Allowed change:

- BattleSandbox visual/data board coordinate conversion and its in-memory
  pointer/preview/collision/placed-view results.

Must remain unchanged:

- Item catalog/local shape cells and core cells;
- ItemSystemSnapshot schema, canonical cells and canonical algorithm;
- BoardAuthority state/atomic/no-op behavior;
- tray accepted mapping;
- Roll, RNG, rarity, affix, core and Build qualification;
- PNGs, Scene, Prefab and UI geometry;
- formal Battle and all formal-flow signatures.

The mapping must be deterministic, invariant-culture safe and independent of
dictionary enumeration order.

## 14. Forbidden Scope

Do not modify:

```text
Scene or Prefab
RectTransform or hierarchy
BuildGridPreviewSlotView
ShapePlacementSession
ShapeAwareItemTrayGrid
TrayPlacementViewModel
BuildItemPreviewCardView
BuildItemTrayPreviewView
ItemSystemBattleSandboxBoardAuthority
ItemSystemBattleSandboxBoardAdapter
ItemSystemBattleSandboxViewProjection
ItemSystemSnapshot
Item catalog or shape truth
PNG or Sprite import settings
Item Detail
Roll / RNG / rarity / affix / core / Build qualification
Enemy / Battle results / formal Battle
RunFlow / SaveData / Reward / Chapter / Boss
BuildSettings / ProjectSettings
AGENTS.md / Docs/LOCKED/* / Package Queues
```

Do not:

- add an `itemId` or `shapeId` special case;
- mirror or replace PNGs;
- rewrite canonical Item local cells;
- write visual coordinates into ItemSystemSnapshot;
- create a second board state owner;
- weaken the user failure into a static PASS;
- edit the previous tray verifier/report history;
- run a Builder or asset-writing historical regression;
- clean unrelated dirty/untracked files;
- add, commit, tag, push, reset or rollback Git state;
- start Prefab migration, consumer migration or legacy cleanup.

## 15. Delivery State

Development may stop only at:

```text
DEV_COMPLETE
QA_STATIC_PASS
REAL_BATTLESANDBOX_BOARD_COORDINATE_PATH_PASS
USER_HANDTEST_WAITING
```

Automated QA does not replace user handtest.

Required user handtest:

1. Open `Scene_TalismanBag_V04_BattleSandboxPreview.unity`.
2. Enter Play.
3. Confirm I004/I005 remain correct in tray and board.
4. Drag I010 and I016 to several board positions.
5. Confirm the three physical board cells match the artwork corner.
6. Press/drag from each of the three visible board cells; all must select the
   same item.
7. Press the visual hole; it must not select or drag the item.
8. Move and rotate I010/I016 through all supported rotations.
9. Try an overlap and an out-of-bounds drop; the previous valid placement must
   remain unchanged.
10. Return each item to the tray and repeat three times.
11. Spot-check one single, horizontal, vertical and four-cell item.

Final Guard acceptance remains held until the user explicitly confirms PASS.

## 16. Required Status Sync

Return:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BattleSandboxBoardCoordinateParityGuardFix01

AssignmentSHA256:

Result:

ModifiedFiles:

CoordinateContract:

I010I016RuntimeEvidence:

I004I005Regression:

ThirtyItemMatrix:

RotationRegression:

AuthoritySnapshotIsolation:

ProtectedHashes:

Unity:

LeakCheck:

GitOperations:
NONE

USER_HANDTEST_WAITING
PREFAB_MIGRATION_NOT_STARTED
LEGACY_CLEANUP_NOT_STARTED
NEXT_PACKAGE_NOT_STARTED
```
