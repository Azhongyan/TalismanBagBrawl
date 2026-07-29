# V0.4-BattleSandboxTrayArtworkLayerSeparationGuardFix01 Assignment

Package:

```text
V0.4-BattleSandboxTrayArtworkLayerSeparationGuardFix01
```

Primary Guard:

```text
Item System Guard
```

Overall / Shared Presentation receipt:

```text
OVERALL_SHARED_PRESENTATION_CONFIRM_BATTLESANDBOXTRAYARTWORKLAYERSEPARATIONGUARDFIX01
```

Target receipt:

```text
ITEM_GUARD_PASS_BATTLESANDBOXTRAYARTWORKLAYERSEPARATIONGUARDFIX01
```

## 1. User-Visible Failure

Two generic tray presentation defects are visible:

```text
I010 / I016:
three generated occupancy blocks obscure or replace the item artwork

I025-I030:
runtime-created cards display authoritative artwork through the root card
background Image, then body tinting makes the artwork look like a black mask
```

There is no Item ID, FaMen, QiLei or Taibai-specific black-mask branch.

## 2. Frozen Root Cause

### 2.1 Non-rectangular tray cards

`TrayItemLayoutView` creates `TrayLayoutCellLayer` Images for non-rectangular
footprints.

`BuildItemPreviewCardView.SetLayoutCellVisuals()` enables
`usesLayoutCellVisuals`.

`ApplyBodyColor()` then:

```text
clears backgroundImage
colors generated layout-cell Images
```

Those generated Images are intended to describe interaction/occupancy cells,
but they become dominant visible blocks around or over the authoritative item
artwork.

### 2.2 Runtime-created tray cards

The Scene does not author enough card objects for every I001-I030 row.
Additional cards are created by the existing runtime card factory.

Those runtime cards have no independent authored artwork child. Therefore
`BindAuthoritativeArtwork()` falls back to the card root `backgroundImage`.

The same Image is later processed by `ApplyBodyColor()` as card body. The
authoritative Sprite is tinted with the card body color.

The Taibai `_5` PNGs contain authored dark outline pixels, so this shared-slot
tinting reads visually as a black mask. PNG import is valid and must not be
changed.

First missing layer:

```text
BuildSandbox tray artwork/body rendering ownership
```

## 3. Frozen Presentation Contract

Establish:

```text
AUTHORITATIVE_ARTWORK_RENDER_LEASE
```

Rules:

1. Authoritative artwork and card-body/interaction-cell visuals have separate
   rendering ownership.
2. `ApplyBodyColor` must never recolor, clear, hide, replace or alpha-multiply
   the Image currently designated as authoritative artwork.
3. An authored independent artwork Image keeps its authored RGB modulation.
   Body color changes must not touch it.
4. If a runtime card must lease the root/background Image as artwork, the
   lease must:
   - capture the exact prior state;
   - render the authoritative Sprite with neutral white RGB and required
     opacity;
   - keep body styling from touching the leased Image;
   - restore the exact prior state on release.
5. A lease must restore:
   - activeSelf;
   - enabled;
   - sprite and overrideSprite;
   - color;
   - Image type;
   - preserveAspect;
   - fillCenter;
   - material;
   - pixelsPerUnitMultiplier;
   - raycastTarget;
   - local artwork rotation;
   - every layout-cell renderer state temporarily suppressed by the lease.
6. Lease release is mandatory on:
   - rebind;
   - explicit unbind/restore;
   - missing or invalid artwork after a prior binding;
   - disable;
   - destroy.
7. Generated non-rect layout cells may continue to own raycast/hit/occupancy
   geometry, but while authoritative artwork is present their rendering must
   be visually non-obscuring.
8. Do not disable raycast geometry merely to hide a block. Transparent or
   equivalent non-obscuring rendering must preserve physical hit semantics.
9. Authored Edit-mode colors, active states, hierarchy, RectTransforms and
   sibling order are source of truth and must round-trip unchanged after Play.
10. Do not delete, rename, reparent, resize, reorder or persist new hierarchy
    as part of the fix.
11. No Item ID, FaMen, QiLei, color-name, PNG-name or shape-specific runtime
    branch is allowed.
12. Neutral-white modulation for a leased fallback artwork Image is the only
    allowed fixed color. No black/color workaround is allowed.

## 4. Systems That Must Not Change

This is presentation-only.

The following remain authoritative and unchanged:

```text
tray placement and occupied slots
raycast/hit cells
collision/reservation
drag source and return-to-tray
board visual/data coordinate boundary
ItemSystemSnapshot
Item shape/catalog facts
PNG/Sprite assets and importer settings
Roll/RNG/probability
Build qualification/core/affix/stat data
```

## 5. Task-Start Gate

The Assignment was frozen only after:

```text
Unity/Tuanjie/UnityShaderCompiler/bee_backend: 0
Temp/UnityLockfile: ABSENT
Library/EditorInstance.json: ABSENT
BattleSandbox Scene SHA: STABLE 5/5
```

Task-start BattleSandbox Scene:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
329696dde34e5f6569ca69b4bde926a25ca95e4b3d2f2158c198d26264e6a441
```

Task-start external git-status evidence:

```text
rows: 1277
Ordinal status digest:
ccaddbb33ad02af558ef008ffbd48a66fc37c0d1a5935251e7490537981fb385
```

This digest is attribution evidence only. Do not clean, revert or claim
unrelated dirty/untracked files.

Stop immediately if:

1. Unity/Tuanjie or another batch process is active at task start.
2. UnityLockfile or EditorInstance exists.
3. BattleSandbox Scene does not match the frozen SHA.
4. Either allowed-existing file differs from section 7.
5. Any protected baseline differs from section 9.
6. The fix requires Scene, Prefab, controller, card factory, PNG or Item data
   changes.

## 6. Exact Existing-File Write Whitelist

Only:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs
```

### BuildItemPreviewCardView.cs

Allowed:

- implement the reversible authoritative-artwork rendering lease;
- prevent body styling from touching the leased artwork Image;
- make generated layout-cell renderers non-obscuring while preserving
  raycasts;
- restore exact captured states on rebind, failure, disable and destroy;
- expose internal/read-only evidence needed by the dedicated verifier;
- preserve the accepted absolute artwork rotation behavior.

### TrayItemLayoutView.cs

Allowed:

- keep generated layout cells as interaction/occupancy geometry;
- remove presentation behavior that forces them to obscure artwork;
- stop unnecessary sibling reordering of an existing authored/generated layer;
- support the artwork lease without changing cell RectTransforms, occupied
  slots or layout calculations;
- preserve runtime-only hide flags for runtime-created objects.

No other existing file is writable.

## 7. Allowed-Existing Task-Start Hashes

```text
BuildItemPreviewCardView.cs
5e0f1f59d11ef6607af586a6b7fb7865c52e04db02bc5b2b7a4607a71aa6450f

TrayItemLayoutView.cs
f5737414548377faceccdfbea456bc3a6efb0dc6a3fdfbeeca79e17902595bb3
```

Both are current disk baselines. `BuildItemPreviewCardView.cs` already contains
accepted work from earlier packages. Do not restore either file from Git HEAD.

## 8. Exact New-File Whitelist

Verifier:

```text
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkLayerSeparationGuardFixVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayArtworkLayerSeparationGuardFixVerifier.cs.meta
```

Reports:

```text
Docs/V0.4/Reports/BattleSandboxTrayArtworkLayerSeparationGuardFixReport.md
Docs/V0.4/Reports/BattleSandboxTrayArtworkLayerSeparationGuardFixSpec.csv
Docs/V0.4/Reports/BattleSandboxTrayArtworkLayerSeparationMatrix30.csv
Docs/V0.4/Reports/BattleSandboxTrayRuntimeCardArtworkMatrix.csv
Docs/V0.4/Reports/BattleSandboxTrayArtworkLeaseRestorationMatrix.csv
Docs/V0.4/Reports/BattleSandboxTrayArtworkLayerSeparationManualTest.md
Docs/V0.4/Reports/BattleSandboxTrayArtworkLayerSeparationGuardFixLeakCheckReport.md
```

No Docs `.meta` files.

Exact implementation/report count:

```text
2 modified existing runtime files
2 new verifier files
7 new reports
11 implementation/report files total
```

The Guard-owned Assignment is not part of the implementation count.

## 9. Protected Baselines

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
329696dde34e5f6569ca69b4bde926a25ca95e4b3d2f2158c198d26264e6a441

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs
29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21

Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs
6b3d3d76e179d9d40251ef1c0ae7e8b99469628c48e39af6d19f4f4973c05a5f

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs
da8c1b628a02565908e7bc7a0ef9e56dc2e00bec25af549df9382769372665bc

Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayPlacementViewModel.cs
a4075e2279bb937858be97a5e41365ce0eff58f12277f0d19b93b7efac29b590

Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs
6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20

Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs
794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827

Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs
606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9

Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29

Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab
2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c

ProjectSettings/EditorBuildSettings.asset
08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59

Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxBoardCoordinateParityGuardFixVerifier.cs
abb155ea894dbac262a856de1efd87547ffc77be0a0d96c990f8bba68e441b8a

Docs/V0.4/Reports/BattleSandboxBoardCoordinateParityGuardFixReport.md
3832e51c3c68f0ec3404da23708584951867ba47a69b54705f0f296b4e4ebfab

Docs/V0.4/Reports/BattleSandboxBoardCoordinateParityGuardFixSpec.csv
7837dc899c8ccacae59c98a9ef7e3238fd2ad60f6e19e92e9e9eecf63a416c3d

Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxTrayPhysicalOccupancyParityGuardFixVerifier.cs
bf84243833adae743d26efb8972b66df5b9eb64cf5f7db067af831bbdc4c5198

Docs/V0.4/Reports/BattleSandboxTrayPhysicalOccupancyParityGuardFixReport.md
c3d78759a2be6e337e1ef15e326083eeff8a62e3552179a6bab70867ba229ebe
```

Item artwork PNG baseline:

```text
root: Assets/_Game/Resources/item_daoju
count: 174
aggregate SHA-256:
a5379f9b1f6472ffe0c58b9a27f29e4553e12cc98ad6f03992d9468b14b67be3
```

Also protect:

- all PNG/Sprite `.meta` files and import settings;
- all other Scenes, Prefabs, RectTransforms and UI hierarchy;
- Item Detail and authoring-preview tools;
- tray physical placement, board coordinate and BoardAuthority behavior;
- Item shape/catalog, Roll/RNG/probability, affix, core and Build data;
- EnemySystem, EncounterPresentation and BoneAspect work;
- RunFlow, SaveData, Reward, Chapter, Boss and formal Battle;
- `AGENTS.md`, `Docs/LOCKED/*` and Package Queues.

## 10. Required Runtime Verification

The verifier must use production Views and the real BattleSandbox Scene in
memory. Do not save the Scene or Prefab.

### 10.1 I010 / I016

For each:

1. Use the accepted real tray placement.
2. Bind its authoritative Sprite through the real card View.
3. Assert exactly one authoritative artwork owner.
4. Assert the artwork Image remains enabled, active and visually non-obscured.
5. Assert body/selected/drag visual changes do not alter the artwork lease.
6. Assert generated corner cell Images remain raycast targets.
7. Assert generated corner cell rendering is non-obscuring while artwork is
   bound.
8. Assert the same three physical/hit slots and the same hole as the accepted
   tray physical package.
9. Assert drag, return-to-tray and repeated refresh do not restore blocks over
   the artwork.

### 10.2 I025-I030 Runtime-Card Fallback

For each:

1. Exercise the real runtime-card creation and binding path.
2. Prove no authored independent artwork child is required for success.
3. Assert fallback root/background Image enters an artwork lease.
4. Assert bound Sprite identity is exact.
5. Assert modulation is neutral white RGB and required opacity.
6. Assert body/selected/drag changes do not tint or clear the artwork.
7. Assert no generated layout renderer obscures the artwork.
8. Assert text, raycast and card interaction remain functional.
9. Assert no black/card-body contamination is introduced by View color.

The verifier may name these items as required samples. Runtime implementation
must not branch on them.

### 10.3 I004 / I005 Regression

Assert:

- accepted tray image direction remains zero-degree/vertical;
- authoritative artwork remains visible;
- body styling does not reintroduce tint or stale -90 rotation;
- physical tray and board behavior remain unchanged.

### 10.4 Thirty-Item Matrix

For I001-I030:

```text
authoritative Sprite resolved and bound
exactly one authoritative artwork owner
artwork active/enabled
artwork not body-tinted
layout renderers non-obscuring when artwork is present
tray physical occupied slots unchanged
drag source unchanged
board parity unchanged
repeated refresh/no-op stable
```

### 10.5 Lease Restoration

Use both:

```text
authored independent artwork Image
runtime fallback root/background Image
```

For each, capture every field in section 3 before bind.

Verify exact restoration after:

```text
bind -> rebind
bind -> explicit restore
bind -> missing Sprite/failure
bind -> disable -> enable/refresh
bind -> destroy
three repeated bind/refresh/restore cycles
```

No stale authoritative reference, disabled renderer, transparent color,
modified raycast, material, rotation or layout-cell state may remain.

### 10.6 Edit-Mode Round Trip

Capture before Play and compare after Play:

```text
Scene hash
authored Image colors
activeSelf values
hierarchy names/parents/sibling indexes
RectTransform geometry
```

All must remain exact.

### 10.7 Missing/Invalid Artwork

Null or invalid Sprite:

- returns failure;
- leaves no active lease;
- restores prior state;
- does not hide/tint the prior valid card;
- does not create persistent hierarchy;
- does not change physical interaction.

## 11. Source and Leak Assertions

Runtime source must contain no branch/token for:

```text
I010
I016
I025
I026
I027
I028
I029
I030
Taibai
TaiBai
FaMen
QiLei
black mask
PNG filename
```

The verifier must fail if:

- `ApplyBodyColor` can mutate the current authoritative artwork;
- runtime fallback artwork remains card-body tinted;
- layout-cell hit geometry is deleted or its raycast is disabled;
- Scene/Prefab hierarchy is saved or reordered;
- protected physical/canonical behavior changes.

## 12. Required Markers

```text
AUTHORITATIVE_ARTWORK_RENDER_LEASE_PASS
APPLY_BODY_COLOR_ARTWORK_ISOLATION_PASS
I010_I016_ARTWORK_UNOBSCURED_PASS
I010_I016_HIT_GEOMETRY_UNCHANGED_PASS
I025_I030_RUNTIME_FALLBACK_ARTWORK_PASS
I004_I005_DIRECTION_REGRESSION_PASS
TRAY_ARTWORK_PRESENCE_30_PASS
TRAY_PHYSICAL_PARITY_30_PASS
BOARD_COORDINATE_PARITY_REGRESSION_PASS
LEASE_REBIND_RESTORE_PASS
LEASE_DISABLE_DESTROY_RESTORE_PASS
MISSING_ARTWORK_SAFE_FAILURE_PASS
EDITMODE_ROUNDTRIP_PASS
SCENE_BYTE_IDENTICAL_PASS
PREFAB_BYTE_IDENTICAL_PASS
PNG_AGGREGATE_UNCHANGED_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_WAITING
```

Unity batch entry:

```text
TalismanBag.EditorTools.BuildSandbox.BattleSandboxTrayArtworkLayerSeparationGuardFixVerifier.VerifyStaticBatch
```

Unity executable:

```text
F:\2022.3.50f1c1\Editor\Unity.exe
```

Do not run Unity concurrently with the user's Editor or another task. Never
close another process.

## 13. Allowed and Forbidden Deltas

Allowed:

- runtime-only artwork lease state;
- runtime-only non-obscuring layout-cell renderer state;
- new verifier and report content.

Forbidden:

```text
Scene/Prefab save
RectTransform/hierarchy/layout geometry change
runtime card factory change
PNG/Sprite/meta/import change
Item ID/FaMen/QiLei/shape-specific branch
tray occupied/hit/collision/reservation change
board/controller/ItemSystem change
Item data/Roll/RNG/probability change
Enemy/formal flow change
```

No Builder or asset-writing historical regression.

No Git add, commit, tag, push, reset or rollback.

Do not start Prefab migration, consumer migration or legacy cleanup.

## 14. Delivery State

Development may stop only at:

```text
DEV_COMPLETE
QA_STATIC_PASS
REAL_BATTLESANDBOX_TRAY_ARTWORK_PATH_PASS
USER_HANDTEST_WAITING
```

Required user handtest:

1. Open `Scene_TalismanBag_V04_BattleSandboxPreview.unity`.
2. Enter Play.
3. Confirm I010/I016 show the complete artwork without three color blocks
   obscuring it.
4. Confirm the same three occupied cells still start drag and the hole does
   not.
5. Confirm I025-I030 display their original-color artwork without black-mask
   contamination.
6. Confirm I004/I005 remain correctly oriented.
7. Select, drag, return and filter these items repeatedly.
8. Exit Play and confirm authored card colors/layout did not change.

Automated QA does not replace user handtest.

## 15. Required Status Sync

Return:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BattleSandboxTrayArtworkLayerSeparationGuardFix01

AssignmentSHA256:

Result:

ModifiedFiles:

ArtworkLease:

I010I016:

I025I030:

I004I005:

ThirtyItemMatrix:

LeaseRestoration:

PhysicalAndBoardRegression:

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
