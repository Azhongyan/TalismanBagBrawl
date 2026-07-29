# V0.4-BattleSandboxTrayArtworkDirectionGuardFix01 Assignment

## 1. Assignment Identity

```text
Package:
V0.4-BattleSandboxTrayArtworkDirectionGuardFix01

Primary Guard:
Item System Guard

Overall / Shared Presentation Approval:
DIRECT_OVERALL_GUARD_FIX_REQUEST

Target Receipt:
GUARD_PASS_BATTLESANDBOXTRAYARTWORKDIRECTIONGUARDFIX01

Development Stop:
DEV_COMPLETE / QA_STATIC_PASS / USER_HANDTEST_WAITING

Next Package:
NOT_STARTED
```

This package is a narrow BuildSandbox presentation correction. It does not
change Item shape truth, PNG artwork, Item generation, Battle rules, or Item
Detail.

## 2. User-Approved Goal

Correct the BattleSandbox tray presentation for:

```text
I004 五雷急令
I005 照壳雷镜
I010 风火令旗
I016 护坛令旗
```

At `rotation=0`:

- the tray occupied silhouette and artwork direction must agree;
- the drag ghost must agree with the same Item orientation;
- the board preview and placed artwork must continue to agree with the
  authoritative Item shape;
- repeated refresh, drag, return, and re-open operations must not accumulate
  rotation.

The fix must be systemic at the tray presentation boundary. No itemId-specific
runtime branch is allowed.

## 3. Frozen Shape And Artwork Facts

The following are authoritative and must not change:

| Item | shapeId | occupied cells | coreCellLocal |
|---|---|---|---|
| I004 | `shape_line2_v` | `(0,0);(0,1)` | `(0,1)` |
| I005 | `shape_line2_v` | `(0,0);(0,1)` | `(0,1)` |
| I010 | `shape_corner3` | `(0,0);(1,0);(0,1)` | `(0,1)` |
| I016 | `shape_corner3` | `(0,0);(1,0);(0,1)` | `(0,1)` |

`ItemInnerDataCatalog` and
`ItemShapeCatalogBatchCorrectionSpec.csv` agree with these facts.

The existing PNGs are correctly authored. Do not rotate, replace, crop,
mirror, reimport, or edit them.

`I010` and `I016` are the only ordinary `shape_corner3` catalog rows.

## 4. Read-Only Root Cause Evidence

### 4.1 Stale authored Image rotation

The target Scene contains exactly two child objects named
`ItemCard_##_image`:

```text
ItemCard_04_image RectTransform fileID 1190502616
local rotation: z=-0.7071068, w=0.7071068
Euler hint: z=-90

ItemCard_05_image RectTransform fileID 1283613157
local rotation: z=-0.7071068, w=0.7071068
Euler hint: z=-90
```

No other `ItemCard_##_image` child has this authored base rotation.

`BuildItemPreviewCardView.ApplyArtworkImageRotationOffset` currently caches
the selected Image's existing local Z rotation as the original base and then
applies:

```text
originalRotationDegrees + rotationOffsetDegrees
```

Therefore the stale `-90` becomes a session base, survives a zero offset, and
is captured into `ShapeCellVisualStyle.SourceRotationDegrees`.

### 4.2 Surface calibration

The target Scene currently serializes all six controller calibration values as
zero:

```text
triangleTrayArtworkRotationOffsetDegrees = 0
triangleDragGhostArtworkRotationOffsetDegrees = 0
triangleBoardPlacedArtworkRotationOffsetDegrees = 0
defaultTrayArtworkRotationOffsetDegrees = 0
defaultDragGhostArtworkRotationOffsetDegrees = 0
defaultBoardPlacedArtworkRotationOffsetDegrees = 0
```

These values must remain zero in this package.

The authoritative BattleSandbox artwork source is created with:

```text
sourceRotationDegrees = 0
spansWholeItem = true
```

The board reconstructs whole-item artwork from the accepted Item System state
and does not depend on the tray Image's saved transform.

### 4.3 Tray visual coordinate boundary

The logical tray placement and Item shape must remain unchanged.

`TrayPlacementViewModel.FromPlacement` is the boundary that converts logical
tray cells into row-major UI slot indexes. The Unity tray grid is visually
top-down, so a non-rectangular footprint must be normalized into one common
top-left visual coordinate convention before rendering.

For `shape_corner3`, the implementation must first compare:

```text
authoritative normalized shape
tray logical placement
tray presentation silhouette
drag ghost silhouette
board rotation0 presentation silhouette
```

The tray presentation must match the already-correct board/artwork
orientation. Do not modify the logical `ShapeAwareItemTrayGrid` to achieve a
visual fix.

## 5. Task-Start Occupancy Gate

The Assignment was frozen only after:

```text
Unity/Tuanjie/UnityShaderCompiler/bee_backend processes = 0
Library/EditorInstance.json = ABSENT
Temp/UnityLockfile = ABSENT
```

The target Scene was read five times with two-second spacing:

```text
SHA-256:
f505d83387e3102ce358b90cf16c6061325057f7c7f811f98e20df5b17e71d3b

Length:
2032174 bytes

LastWrite UTC:
2026-07-23T16:41:40.5680380Z
```

If the Scene differs before development, or a Unity lock/process appears,
stop without accepting a new baseline:

```text
GUARD_RETURN_BATTLESANDBOXTRAYARTWORKDIRECTIONGUARDFIX01_TASK_START_DRIFT
```

Do not terminate the user's Editor or another package's process.

## 6. Exact Existing-File Whitelist

Only these existing files may change:

| Path | Task-start SHA-256 | Allowed change |
|---|---|---|
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `f505d83387e3102ce358b90cf16c6061325057f7c7f811f98e20df5b17e71d3b` | Only normalize the two named Image RectTransforms in Section 7 |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | `eb582e009659564e3d3dd8f857ff891dc70303142554d0785d6ac22f849a75f3` | Make authoritative tray artwork direction absolute/idempotent; do not inherit stale authored base rotation |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs` | `5dc8394571ebac115c906780ea1b5c5b1a84bcc50696e32e74f236ff6636a3ce` | Normalize logical occupied cells into the tray's top-left presentation convention without changing logical placement |

No existing `.meta` file may change.

If the corner correction cannot be implemented inside this exact whitelist,
stop and return evidence. Do not expand into `TrayItemLayoutView`,
`BuildItemTrayPreviewView`, `ShapeAwareItemTrayGrid`, or the controller without
a revised Guard Assignment.

## 7. Exact Scene Delta

The only allowed Scene geometry delta is:

```text
ItemCard_04_image / RectTransform fileID 1190502616
  m_LocalRotation -> {x:0,y:0,z:0,w:1}
  m_LocalEulerAnglesHint -> {x:0,y:0,z:0}

ItemCard_05_image / RectTransform fileID 1283613157
  m_LocalRotation -> {x:0,y:0,z:0,w:1}
  m_LocalEulerAnglesHint -> {x:0,y:0,z:0}
```

For both objects, these must remain unchanged:

```text
parent
sibling index
anchors
pivot
anchoredPosition
sizeDelta
localPosition
localScale
active state
Image component
Sprite
color
material
raycast state
```

The Scene must not create, delete, rename, or reparent any object.

The six controller calibration fields listed in Section 4.2 must remain zero.

## 8. Allowed New Files

Exactly these implementation/verifier files may be added:

```text
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkDirectionGuardFixAuthoring.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkDirectionGuardFixAuthoring.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkDirectionGuardFixVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkDirectionGuardFixVerifier.cs.meta
```

The authoring entry may open and save only the exact target Scene. It must
verify the task-start Scene SHA before writing and apply only Section 7. It
must not invoke any historical Builder.

Recommended batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.BattleSandboxTrayArtworkDirectionGuardFixAuthoring.ApplyStaticBatch
```

Required verifier batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.BattleSandboxTrayArtworkDirectionGuardFixVerifier.VerifyStaticBatch
```

## 9. Allowed Reports

Exactly these report files may be added:

```text
Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixReport.md
Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixSpec.csv
Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionMatrix30.csv
Docs/V0.4/Reports/BattleSandboxTrayArtworkSurfaceParity.csv
Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionManualTest.md
Docs/V0.4/Reports/BattleSandboxTrayArtworkDirectionGuardFixLeakCheckReport.md
```

Do not generate Docs `.meta` files.

## 10. Required Implementation Semantics

### 10.1 Absolute and idempotent artwork direction

The authoritative tray artwork direction must be derived from:

```text
authoritative source direction
+ explicit surface offset
+ Item rotation
```

It must not be derived from a stale Scene Image local rotation.

Calling refresh/cache/bind repeatedly with the same input must yield the same
final direction. No call may accumulate another `-90`, `+90`, or any other
rotation.

Do not add an I004/I005 special case. The same rule applies to all 30
ordinary items.

### 10.2 Presentation-only cell mapping

The underlying logical tray placement remains authoritative and unchanged.

The presentation ViewModel may normalize a non-rectangular footprint into the
tray grid's visual row convention. It must:

- preserve cell count;
- preserve bounding width and height;
- preserve the item's logical anchor and rotation facts;
- not mutate `ShapeAwareItemTrayGridPlacement`;
- not mutate `ShapeItemPayload`;
- not mutate catalog cells;
- not change board placement;
- remain deterministic under filter/compact/refresh.

The normalized tray silhouette for I010/I016 at rotation0 must match the
already-correct board/artwork silhouette in a common visual coordinate
convention.

Do not rotate or mirror the PNG to compensate for the footprint.

### 10.3 Rotation transitions

For rotatable items:

```text
rotation0 -> rotation90 -> return to tray -> rotation0
```

must produce stable, non-accumulating artwork and occupied silhouettes.

The package does not change which items can rotate or the rotation algorithm.

## 11. Required 30-Item Regression Matrix

The verifier must use the real I001-I030 catalog and current BattleSandbox dev
roster. No artificial replacement roster may stand in for the real path.

Each matrix row must include:

```text
baseItemId
shapeId
catalogRotation0Cells
trayLogicalRotation0Cells
trayPresentationRotation0Cells
trayArtworkFinalDegrees
dragGhostRotation0Cells
dragGhostArtworkFinalDegrees
boardPreviewRotation0Cells
boardPreviewArtworkFinalDegrees
boardPlacedRotation0Cells
boardPlacedArtworkFinalDegrees
refreshRepeatCount
directionParity
unaffectedOrTarget
result
```

Exact target assertions:

### I004 / I005

- `shape_line2_v`;
- two vertical occupied cells remain unchanged;
- authored tray Image local Z is zero;
- captured authoritative source rotation is zero;
- tray, drag ghost, board preview, and board placed artwork agree at
  rotation0;
- three repeated bind/cache/refresh cycles remain zero and do not drift.

### I010 / I016

- `shape_corner3`;
- three occupied cells remain unchanged in logical facts;
- tray presentation silhouette agrees with the board/artwork rotation0
  silhouette after conversion to one common visual coordinate convention;
- drag ghost and board continue to use the authoritative shape;
- no PNG transform or itemId-specific runtime branch exists.

### Remaining 26 items

- catalog cells unchanged;
- tray logical placement unchanged;
- visual cell count and bounding dimensions unchanged;
- rotation0 artwork direction unchanged from the correct authoritative
  orientation;
- no new direction mismatch;
- repeated refresh is idempotent.

## 12. Required Verification Markers

```text
TASK_START_GATE_PASS
SHAPE_TRUTH_30_UNCHANGED_PASS
PNG_AGGREGATE_UNCHANGED_PASS
I004_I005_STALE_BASE_ROTATION_REMOVED_PASS
AUTHORITATIVE_ARTWORK_ABSOLUTE_DIRECTION_PASS
I010_I016_CORNER_VISUAL_MAPPING_PASS
TRAY_DRAG_BOARD_ROTATION0_PARITY_30_PASS
REPEATED_REFRESH_NO_ROTATION_DRIFT_PASS
UNAFFECTED_ITEMS_26_PASS
SCENE_EXACT_TWO_TRANSFORM_DELTA_PASS
NO_HIERARCHY_DELTA_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_WAITING
PREFAB_MIGRATION_NOT_STARTED
LEGACY_CLEANUP_NOT_STARTED
NEXT_PACKAGE_NOT_STARTED
```

Static assertions that only check method existence or non-null Sprites are not
enough. The verifier must compare real surface direction/cell signatures.

## 13. Protected Baselines

These files must remain byte-identical:

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs` | `f5737414548377faceccdfbea456bc3a6efb0dc6a3fdfbeeca79e17902595bb3` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayGridReservationView.cs` | `72933bcdbdb0ba667ddae2b3ec9b78825bedb90906fc84d52796644d7f87f23b` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `531894cb6b36e6ef84afc93cb15918d4324b5542094dc3c5ddaa3915115e5aff` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapePlacementSession.cs` | `9d3eb2b209343f013deb1ec8ed062f85f51b3cfca4530b6e28dcc615348390a5` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs` | `51ed1ac2f4a45d66f73888ae82aa157695e1c47cedbd49a6c5a6542d09f50b95` |
| `Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs` | `606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9` |
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs` | `6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs` | `116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204` |
| `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs` | `e81d67bd8a6f9f3e0f07080b5220bdf7c71966d1a6d2d02e2fd068903d71856d` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity` | `8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |
| `Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionSpec.csv` | `e94ab078b92b98f02b98af049185dc8340b17c6448b0ae4fe0fa9364966da698` |
| `Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionReport.md` | `3da85c22219a3486c06f099374929f13675f0a0dfed3c9da1cff128c162a31d6` |

All PNG files under `Assets/_Game/Resources/item_daoju` are protected:

```text
File count: 174
Aggregate SHA-256:
a5379f9b1f6472ffe0c58b9a27f29e4553e12cc98ad6f03992d9468b14b67be3

Aggregate algorithm:
1. Recursively enumerate PNG files.
2. Sort relative paths using Ordinal order.
3. Build UTF-8 lines: relativePath|fileSha256.
4. Join with LF.
5. SHA-256 the resulting bytes.
```

The baseline is the task-start disk state, not Git HEAD.

## 14. Forbidden Scope

Do not modify:

- Item shape/catalog/core cell truth;
- `ShapeAwareItemTrayGrid` or `ShapePlacementSession`;
- board placement, overlap, rotation, or commit logic;
- I031 placement/lighting semantics;
- Item rarity, Roll, stats, affixes, Build qualification, drop probability;
- any PNG, Sprite import settings, resource path, or `.meta`;
- Item Detail ViewModel/View/Prefab/layout;
- ItemSandbox Scene;
- Battle flow, combat result, Enemy, RunFlow, SaveData, Reward, Chapter, Boss;
- UnifiedBattlePage or Bridge;
- BuildSettings or ProjectSettings;
- `AGENTS.md` or `Docs/LOCKED/*`;
- Prefab migration or consumer migration;
- legacy hierarchy cleanup;
- unrelated dirty/untracked files.

Do not:

- add an I004/I005/I010/I016 runtime special case;
- rotate or mirror a PNG;
- change the catalog shape to match a wrong tray picture;
- copy the board placement algorithm into the tray ViewModel;
- introduce a second placement or state owner;
- run a historical Scene Builder or historical asset-writing regression;
- accept a new Scene baseline after a mismatch;
- commit, tag, push, reset, rollback, or clean.

## 15. Unity And Concurrency Rules

Before each Unity batch:

```text
Unity/Tuanjie/UnityShaderCompiler/bee_backend = 0
Library/EditorInstance.json = ABSENT
Temp/UnityLockfile = ABSENT
```

Do not run concurrently with another Unity package.

Use Unity 2022.3.50f1c1:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
```

Run only:

1. the dedicated exact-scene authoring entry;
2. the dedicated package verifier.

Do not run other Builders or historical verifiers that write assets.

After each run:

- wait for the process to exit;
- confirm Unity process count is zero;
- confirm lock and EditorInstance are absent;
- attribute and remove only package-created non-whitelist import side effects;
- never delete or claim another package's files.

## 16. Acceptance

The package may stop at:

```text
DEV_COMPLETE
QA_STATIC_PASS
REAL_BATTLESANDBOX_PRESENTATION_PATH_PASS
USER_HANDTEST_WAITING
```

It must not claim final Guard PASS before user handtest.

Required user handtest:

1. Open
   `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`.
2. Enter Play.
3. Confirm I004 and I005 are vertical in the tray at rotation0.
4. Drag I004/I005 and confirm tray, ghost, board preview, and placed artwork
   keep the same vertical direction.
5. Confirm I010/I016 corner footprint and upright artwork point the same way
   in the tray.
6. Drag I010/I016 and confirm the ghost and placed board version keep that
   direction.
7. Return each target to the tray and repeat once; no 90-degree drift may
   appear.
8. Spot-check one single, one horizontal two-cell, one other vertical
   two-cell, and one four-cell item for regression.
9. Confirm Item Detail and unrelated BattleSandbox UI remain unchanged.

After user PASS, return:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS
Package: V0.4-BattleSandboxTrayArtworkDirectionGuardFix01
TargetReceipt: GUARD_PASS_BATTLESANDBOXTRAYARTWORKDIRECTIONGUARDFIX01
```

No next package starts automatically.
