# V0.4-BattleSandboxTrayPhysicalOccupancyParityGuardFix01 Assignment

Package:

```text
V0.4-BattleSandboxTrayPhysicalOccupancyParityGuardFix01
```

Parent package:

```text
V0.4-BattleSandboxTrayArtworkDirectionGuardFix01
USER_HANDTEST_FAIL / FINAL_ACCEPTANCE_HOLD
```

Primary Guard:

```text
Item System Guard
```

Overall authorization:

```text
DIRECT_USER_HANDTEST_FAIL_SYNC
DIRECT_USER_CLOSE_CONFIRMATION_FOR_TRAY_PHYSICAL_MAPPING_REVISION
```

Target receipt:

```text
ITEM_GUARD_PASS_BATTLESANDBOXTRAYPHYSICALOCCUPANCYPARITYGUARDFIX01
```

## 1. User-Visible Failure

The previous package corrected the visible direction of the corner artwork and
colored tray cells, but I010 and I016 still use the old physical trigger cells.

User handtest:

```text
I004 / I005: PASS
I010 / I016: visual cells match artwork, physical occupied-cell trigger FAIL
```

The parent package must remain on final acceptance hold until this revision
passes user handtest.

## 2. Frozen Root Cause

The first missing layer is:

```text
BuildSandbox tray physical occupancy mapping
```

The previous implementation introduced two different tray mappings:

```text
ShapeAwareItemTrayGridPlacement.OccupiedSlotIndexes
  = original payload/logical tray cells

TrayPlacementViewModel.occupiedSlotIndexes
  = separately transformed presentation cells
```

The visible card, colored cells and layout consume the presentation mapping.
Physical `CanPlace`, `Commit`, `OccupiedCells`, collision, pointer drop preview,
drag source anchor and return-to-tray packing still consume the original
mapping.

This split is the defect. It must not be retained.

## 3. Frozen Solution

Establish:

```text
ONE_AUTHORITATIVE_TRAY_MAPPING
```

The generic tray coordinate conversion must happen in the physical
`ShapeAwareItemTrayGrid` placement path. The accepted grid placement then
becomes the single source consumed by presentation.

Required flow:

```text
Item shape payload
  -> generic tray-coordinate mapping
  -> ShapeAwareItemTrayGrid CanPlace / Commit
  -> one accepted OccupiedCells / OccupiedSlotIndexes
  -> collision / reservation / pointer drop / return-to-tray
  -> TrayPlacementViewModel
  -> card layout / colored cells / raycast cells
```

Rules:

1. Do not keep one logical occupied set and a different presentation occupied
   set.
2. Do not branch on `I010`, `I016` or any `itemId`.
3. The generic tray transform may mirror normalized Y inside the footprint
   bounds because tray rows increase downward.
4. Rectangular shapes must remain set-equivalent after the transform.
5. Non-rectangular corner footprints must use the same transformed set for
   physical and visual behavior.
6. Board/data coordinates must remain unchanged. The transform is tray-only.
7. `ShapeItemPayload`, Item catalog shape data and board placement rules remain
   authoritative and unchanged.

Expected rotation-0 signatures:

```text
I004 / I005 tray physical+visual: 0,0;0,1
I010 / I016 catalog/board data:   0,0;1,0;0,1
I010 / I016 tray physical+visual: 0,0;0,1;1,1
I010 / I016 tray hole:            1,0
```

The tray hole must not reserve, collide or begin an item drag. The three
visible occupied cells must all be physical occupied/raycast cells.

## 4. Task-Start Gate

The Assignment was frozen only after:

```text
Unity/Tuanjie/UnityShaderCompiler/bee_backend processes: 0
Temp/UnityLockfile: ABSENT
Library/EditorInstance.json: ABSENT
BattleSandbox Scene repeated SHA reads: STABLE 5/5
```

Task-start Scene:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
SHA-256:
feb9ad623b2c00983462e9cdf7e767d9c3e93d0759244f3e6e42dedfe7318643
```

The Scene is protected and must remain byte-identical in this package.

Task-start external git-status evidence:

```text
rows: 1114
Ordinal status digest:
aeaf0173c608c4cbb068ea2c414fbde35bd8f5c224c7a21def69e2b40e045d4e
```

This digest is attribution evidence only. Do not clean, revert or claim
unrelated dirty/untracked files.

Stop immediately if:

1. Unity/Tuanjie or another batch process is active at task start.
2. UnityLockfile or EditorInstance exists.
3. The target Scene hash differs from the frozen value.
4. Any allowed-existing source hash differs from section 6.
5. The fix requires Scene, Prefab, controller, Item shape or PNG modification.

## 5. Exact Existing-File Write Whitelist

Only these existing runtime files may change:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs
```

Allowed changes:

### ShapeAwareItemTrayGrid.cs

- Apply one generic tray-coordinate mapping before `CanPlace` and `Commit`
  produce physical occupied cells.
- Ensure `OccupiedCells`, `OccupiedSlotIndexes`, collision checks, packing,
  rotation preview and return-to-tray use that same mapped result.
- Keep board/data payloads unchanged.
- Keep deterministic ordering: row then column, Ordinal where strings apply.

### TrayPlacementViewModel.cs

- Remove the second presentation-only transform.
- Consume the accepted `ShapeAwareItemTrayGridPlacement` occupied slots
  directly.
- Remove `logicalOccupiedSlotIndexes`, or guarantee that no separate divergent
  occupied collection remains. Preferred result is one occupied collection.
- Preserve null/invalid handling and deterministic ordering.

No other existing runtime file is writable.

## 6. Allowed-Existing Task-Start Hashes

```text
ShapeAwareItemTrayGrid.cs
51ed1ac2f4a45d66f73888ae82aa157695e1c47cedbd49a6c5a6542d09f50b95

TrayPlacementViewModel.cs
64763e565e56a10830cc37b45d47f1952572933dc835fcd30746ab4dec0dcf6b
```

Development must use the current disk files as the baseline. Do not restore
either file from Git HEAD.

## 7. Exact New-File Whitelist

Implementation verifier:

```text
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier.cs.meta
```

Reports:

```text
Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixReport.md
Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixSpec.csv
Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyMatrix30.csv
Docs/V0.4/Reports/BattleSandboxTrayPhysicalInteractionMatrix.csv
Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyManualTest.md
Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixLeakCheckReport.md
```

No Docs `.meta` files are allowed.

Exact package implementation count:

```text
2 modified existing runtime files
2 new verifier files
6 new report files
10 implementation/report files total
```

The Assignment itself is Guard-owned and is not part of the implementation
file count.

## 8. Protected Files and Hashes

The following must remain byte-identical:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
feb9ad623b2c00983462e9cdf7e767d9c3e93d0759244f3e6e42dedfe7318643

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs
5e0f1f59d11ef6607af586a6b7fb7865c52e04db02bc5b2b7a4607a71aa6450f

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs
29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21

Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs
f5737414548377faceccdfbea456bc3a6efb0dc6a3fdfbeeca79e17902595bb3

Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayGridReservationView.cs
72933bcdbdb0ba667ddae2b3ec9b78825bedb90906fc84d52796644d7f87f23b

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs
531894cb6b36e6ef84afc93cb15918d4324b5542094dc3c5ddaa3915115e5aff

Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs
606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9

Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs
794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs
6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs
116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204

Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs
e81d67bd8a6f9f3e0f07080b5220bdf7c71966d1a6d2d02e2fd068903d71856d

Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab
2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c

Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29

ProjectSettings/EditorBuildSettings.asset
08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59

Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionSpec.csv
e94ab078b92b98f02b98af049185dc8340b17c6448b0ae4fe0fa9364966da698

Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixReport.md
774b12bb37a90154c586b1f82aae617b8d3fa262d2ad941c994850bda18366ab

Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixSpec.csv
9f5251a0c515d476eefc1ea4d11f21fdca564a3c66415f2992156bb2b6f58cff
```

Also protect:

- all Item PNGs and Sprite import settings;
- all other Scenes and Prefabs;
- the previous GuardFix verifier, authoring tool and reports as historical
  evidence;
- Item shape/catalog, Roll/RNG/probability, Build qualification and core data;
- EnemySystem, EncounterPresentation and BoneAspect work;
- RunFlow, SaveData, Reward, Chapter, Boss and formal Battle;
- `AGENTS.md`, `Docs/LOCKED/*` and Package Queues.

## 9. Required Runtime Verification

The dedicated verifier must use production runtime classes. It must not
synthesize the expected drag/board values and then report them as runtime
results.

Required assertions:

### 9.1 Thirty-Item Single-Mapping Matrix

For I001-I030:

```text
ShapeAwareItemTrayGridPlacement.OccupiedCells
== ShapeAwareItemTrayGridPlacement.OccupiedSlotIndexes
== TrayPlacementViewModel.occupiedSlotIndexes
== TrayGridReservationView reserved slots
== TrayItemLayoutView card cell slots
```

All comparisons use actual runtime outputs and exact ordered sets.

No separate presentation transform may exist after accepted physical
placement.

### 9.2 I010 / I016 Exact Physical Tests

For both items at rotation 0:

1. Pack through real `ShapeAwareItemTrayGrid`.
2. Assert physical relative occupied cells are `0,0;0,1;1,1`.
3. Assert visual/card/raycast slots are the same set.
4. Assert the hole `1,0` is not occupied, not reserved and does not raycast to
   the item card.
5. Assert every occupied cell raycasts to the item card.
6. Put a single-cell blocker on each occupied cell and assert collision.
7. Put a single-cell blocker only in the hole and assert the corner item can
   still occupy its three legal cells.
8. Begin a real `ShapePlacementSession` from the tray placement anchor.
9. Preview/commit a return from Board source to the tray and assert the same
   physical set is restored.
10. Repeat pack -> drag-source -> return three times and assert no mapping
    drift or duplicate reservation.

Repeat equivalent mapping checks for supported tray rotations. Do not change
board rotation semantics.

### 9.3 I004 / I005 Regression

- Physical and visual tray cells remain `0,0;0,1`.
- Artwork rotation remains absolute 0 degrees.
- Scene image transforms remain the accepted identity baseline.
- Drag and return do not reintroduce the stale -90-degree rotation.

### 9.4 Other 26 Items

- Occupied-cell count unchanged.
- Physical and visual occupied sets are identical.
- No overlap, out-of-range or duplicate slot regression.
- Existing tray packing remains deterministic.
- Single, horizontal, vertical and rectangular four-cell samples retain their
  previous physical sets.

### 9.5 Board/Data Isolation

- Catalog cells remain unchanged for all 30 items.
- `ShapeItemPayload.BuildNormalizedOffsets()` remains unchanged.
- Board preview/commit uses board/data coordinates, not tray-transformed
  coordinates.
- I010/I016 board rotation-0 data remains `0,0;1,0;0,1`.
- I031 behavior remains unchanged.

## 10. Verifier Independence

The previous verifier is insufficient evidence because it assigned
drag/board signatures from a helper rather than exercising pointer, collision,
commit and return paths.

The new verifier must:

1. Use real `ShapeAwareItemTrayGrid`, `ShapePlacementSession`,
   `TrayPlacementViewModel`, `TrayItemLayoutView`,
   `TrayGridReservationView` and `BuildItemPreviewCardView` behavior.
2. Create only in-memory dev fixtures; never save the Scene or Prefab.
3. Use actual slot RectTransform centers and Unity UI raycasts/hit tests.
4. Prove both occupied cells and the non-occupied hole behavior.
5. Verify the target BattleSandbox Scene remains wired and byte-identical.
6. Fail if runtime source contains an I010/I016/itemId branch.
7. Fail if a second divergent occupied mapping remains.

Required markers:

```text
ONE_AUTHORITATIVE_TRAY_MAPPING_PASS
PHYSICAL_VISUAL_OCCUPANCY_PARITY_30_PASS
I010_I016_POINTER_HIT_PARITY_PASS
I010_I016_COLLISION_RESERVATION_PASS
I010_I016_DRAG_SOURCE_ANCHOR_PASS
I010_I016_RETURN_TO_TRAY_PASS
I004_I005_DIRECTION_REGRESSION_PASS
UNAFFECTED_ITEMS_26_PASS
BOARD_DATA_COORDINATES_UNCHANGED_PASS
SCENE_BYTE_IDENTICAL_PASS
PNG_AGGREGATE_UNCHANGED_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_WAITING
```

Unity batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier.VerifyStaticBatch
```

Unity executable:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
```

Do not run Unity concurrently with another development task or the user's
Editor. Never close another process.

## 11. Canonical and Determinism Boundary

Allowed changes:

- tray-only physical occupied cells for non-rectangular footprints;
- tray-only placement/reservation/collision results that follow the corrected
  coordinate convention;
- new package report contents and signatures.

Must remain unchanged:

- Item shape/catalog canonical facts;
- board/data occupied coordinates;
- ItemSystemSnapshot and board authority semantics;
- Roll, RNG, rarity, affix, core and Build qualification results;
- PNG aggregate and Scene/Prefab hashes;
- all formal-flow signatures.

The mapping must be deterministic, invariant-culture safe and independent of
dictionary enumeration order.

## 12. Forbidden Scope

Do not modify:

```text
Scene or Prefab
RectTransform or hierarchy
BuildItemPreviewCardView
BuildItemTrayPreviewView
TrayItemLayoutView
TrayGridReservationView
BuildGridInteractionPreviewController
Item catalog or shape truth
PNG or Sprite import settings
Item Detail
ItemSystemSnapshot / BoardAuthority / BoardAdapter
Roll / RNG / rarity / affix / core / Build qualification
Enemy / Battle results / formal Battle
RunFlow / SaveData / Reward / Chapter / Boss
BuildSettings / ProjectSettings
AGENTS.md / Docs/LOCKED/* / Package Queue
```

Do not:

- add an itemId special case;
- add a PNG rotation workaround;
- copy a second shape table;
- weaken the previous user failure into a PASS;
- run a Builder or historical asset-writing regression;
- clean unrelated dirty/untracked files;
- add, commit, tag, push, reset or rollback Git state;
- start Prefab migration, consumer migration or legacy cleanup.

## 13. Delivery State

Development may stop only at:

```text
DEV_COMPLETE
QA_STATIC_PASS
REAL_BATTLESANDBOX_TRAY_PHYSICAL_PATH_PASS
USER_HANDTEST_WAITING
```

Automated QA does not replace user handtest.

Required user handtest:

1. Open `Scene_TalismanBag_V04_BattleSandboxPreview.unity`.
2. Enter Play and test I004/I005 remain vertical and draggable.
3. For I010 and I016, press/drag from each of the three visibly occupied
   colored cells; all must start the same item drag.
4. Press the empty fourth corner of the 2x2 bounding box; it must not act as an
   occupied item cell.
5. Drag I010/I016 to the board, return them to multiple tray locations and
   repeat three times.
6. Confirm visible cells, physical collision, drag ghost and return placement
   stay aligned.
7. Spot-check one single, one horizontal, one vertical and one four-cell item.

Final Guard acceptance remains held until the user explicitly confirms PASS.

## 14. Required Status Sync

Return:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BattleSandboxTrayPhysicalOccupancyParityGuardFix01

AssignmentSHA256:

Result:

ModifiedFiles:

RuntimeEvidence:

ThirtyItemMatrix:

I010I016PointerHit:

I010I016CollisionReservation:

I010I016ReturnToTray:

I004I005Regression:

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
