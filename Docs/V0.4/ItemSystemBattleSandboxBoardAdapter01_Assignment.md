# V0.4-ItemSystemBattleSandboxBoardAdapter01 Assignment

Status: `GUARD_PASS_ASSIGNMENT_ITEMSYSTEMBATTLESANDBOARDADAPTER01_REVISION09_REV03 / WAITING_ITEM_GUARD_REREVIEW / NOT_READY_FOR_DEV`

Queue Id: `N01C-P7R1`

Maintainer: Cross-System Guard / boundary window

## 0. Required receipts

Revision09-Rev03 must not start until Item Guard reissues the receipt below against the exact
Revision09-Rev03 Assignment SHA-256:

```text
ITEM_GUARD_CONFIRM_ITEMSYSTEMBATTLESANDBOARDADAPTER01
```

No new Enemy Guard or Capability Algorithm Guard receipt is required for Revision09-Rev03.
Their Revision08-protected inputs, outputs, call counts and hashes remain unchanged;
Revision09-Rev03 modifies only the generic Item Detail Chinese Text rendering,
section-body presentation seams and final authored icon visibility verification.

Accepted prerequisite:

```text
V0.4-ItemShapeCatalogBatchCorrection01
V0.4-ItemShapeCatalogBatchCorrection01-GuardFix01
Status: COMPLETE / USER_HANDTEST_PASS
Approved items: 30/30
KEEP / CHANGE / UNRESOLVED: 14 / 16 / 0

V0.4-I031InventoryPlacementAndLightingContract01
Status: DEV_COMPLETE / QA_PASS / JOINT_GUARD_ACCEPTED
ItemSystem schema: ItemSystemSnapshot.v2
Verifier scenarios: S01-S25 / 25/25 PASS
Unity compile/verifier: PASS
Revision07 implementation: DEV_COMPLETE / QA_STATIC_PASS / USER_HANDTEST_FAIL
Revision08 correction: DEV_PARTIAL / ORANGE_MODEL_PASS / S20F_STATIC_FAIL
Revision09 Item Detail body-visibility correction: NOT_STARTED
```

Receipts issued against Revision02 through Revision08 do not release Revision09.
Revision08's Orange projection and passing model-level verification are retained in the
current workspace. Revision09 corrects only the generic section-body visibility behavior
proven by S20F; it does not reopen Item truth, Enemy or Capability behavior.

Target development completion receipt:

```text
GUARD_PASS_ITEMSYSTEMBATTLESANDBOARDADAPTER01
```

## 1. Why this package exists

`V0.4-LayoutResilienceBattleSandboxPlaytestAdapter01` passed static QA but selected the wrong handtest container. `Scene_TalismanBag_V04_ItemSandbox.unity` is the user's item detail and configuration workbench. It must not be repurposed as a battle board by hiding `ItemDetailPanel` or activating copied board remnants.

The actual user-adjusted V0.4 battle and prepare page is:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

It already contains the visible and hand-adjusted battle interaction surface:

```text
BattleLikePreviewArea
ItemTrayPreview
ItemTrayContent
BoardGridPreview
25 BoardGridCell nodes
PopupLayer
FeedbackRoot
BuildGridInteractionPreviewRuntime
```

Its current board authority is still the legacy BuildSandbox chain:

```text
BuildSandboxLegacyAndAdvancedItemRosterCatalog
PreviewItem
ShapePlacementSession
UiBoardShapeGridReceiver
placedItemIds
BuildSandboxLayoutSnapshot
```

The accepted Item authority is instead:

```text
ItemInnerDataCatalog I001-I031
Item generated instance projection for I001-I030
ItemSystemSnapshot.v2
ItemInstancePlacementBindingContractSnapshot.v1 (IF01)
```

This package establishes one runtime authority seam between those two layers. It does not build another board, another tray, another scene, or another layout.

### 1.1 Revision03 capacity return and approved correction

Revision02 development stopped correctly at the whitelist boundary with:

```text
DEV_PARTIAL / UNITY_COMPILE_PASS / QA_STATIC_FAIL / BLOCKED_GUARD_RETURN
Verifier: 19/20 PASS
Failure: S01 / TRAY_CAPACITY_WHITELIST_BLOCKER
I001-I030 exact footprint cells: 62
Authored tray slots: 40 / 5 columns x 8 rows
First deterministic row-major capacity that packs all 30 items: 65 / 5 columns x 13 rows
Last occupied logical slot index: 62
```

The failure is attributed only to logical tray capacity. It is not an ItemSystem,
IF01, shape, artwork, drag or board-authority failure. Revision03 permits the narrowest
correction: extend the existing ScrollRect content at runtime with 25 DontSave logical
slots. It does not authorize a Scene, Prefab, viewport, cell-size, spacing or authored
RectTransform change.

### 1.2 Revision04 user-handtest failure and attribution

Revision03 reached `DEV_COMPLETE / QA_STATIC_PASS`, but the user's real target-scene
handtest failed and stopped development:

```text
USER_HANDTEST_FAIL / DEV_STOPPED / WAITING_GUARD_REREVIEW
Failure A: some cards still display legacy V0.3/V0.4 BuildSandbox artwork.
Failure B: tapping an item still opens BuildSandboxItemInfoPanel instead of the completed ItemDetailPanel.
Confirmed artwork regression samples: I004 五雷急令, I005 照壳雷镜, I007 离火符.
```

The failures are attributed as follows:

```text
Artwork:
  the verifier proved that 30 Item sprites could be loaded, but did not prove that the
  final visible card artwork slot, drag ghost and placed-board artwork were all bound.
  The current runtime selector replaces only one Image whose name contains Artwork/Icon,
  or falls back to the card root. Existing child slots such as ItemCard_04_image and
  ItemCard_05_image can therefore retain and display legacy sprites.

Detail:
  Revision03 explicitly preserved "current detail behavior" and explicitly held full
  ItemDetailPanel integration. The implementation therefore continued to call
  BuildSandboxItemInfoPanel. This is an Assignment scope mismatch, not user error.
```

Revision04 supersedes those two clauses only. It preserves the accepted ItemSystem/IF01
authority, 65-slot tray extension, shapes, placement feel and Scene hash. It authorizes
runtime reuse of the completed ItemDetailPanel prefab and authoritative final-renderer
artwork binding without modifying the prefab, Scene, Item source or authored geometry.

### 1.3 I031 product-decision return

After Revision04 was drafted, the user rejected the current I031 behavior:

```text
Current rejected behavior:
- I031 聚念石 is forced onto the board at Reset/Initial state.
- I031 cannot enter the tray.
- I031 cannot return from board to tray.
- the visible V0.4 battle sandbox does not show a usable lighting range or powered result.

User decision:
- I031 聚念石 is a placeable item.
- it must not be permanently locked to the board.
- it must provide a visible and functional lighting/power range when legally placed.
```

The minimum Guard interpretation is frozen as follows pending Item Guard approval:

```text
I031 remains the unique special/system progression item.
I031 remains excluded from ordinary rarity Roll, ordinary affixes, ordinary drops and
ordinary IF01 itemInstance generation.
Exactly one I031 exists in this sandbox roster; duplicates remain forbidden.
Initial and Reset state place I031 in the item tray, not on the board.
The board may therefore be empty after Initial/Reset.
I031 can be dragged tray -> board, moved on the board and returned board -> tray.
I031 uses its authoritative shape_single_1 footprint and normal bounds/overlap rules.
The existing eye cell remains unplaceable unless a later user decision changes that rule.
I031 supplies lighting only while it is legally placed on the board.
The authoritative direct range is the Item Lighting contract's orthogonal four cells:
up / right / down / left, clipped to the 5x5 board.
The source cell is not an additional powered target cell.
LitRangeCells, per-item isLit/isDirectLit/litByPlacementId and isCountedInBuild must be
recomputed from the authoritative ItemSystem snapshot after every real placement change.
The V0.4 board must visibly render the four-cell range and powered/unpowered item state;
text-only claims or legacy BuildSandbox provider tags are not acceptance evidence.
```

This decision conflicts with accepted Item truth and the current adapter implementation:

```text
ItemSystemSnapshot currently rejects an empty placement set as JUNIAN_MISSING.
ItemSystemBattleSandboxBoardAuthority currently creates an I031-only Reset snapshot.
CommitFromTray currently returns I031_TRAY_COMMIT_FORBIDDEN.
ReturnToTray currently returns I031_RETURN_FORBIDDEN.
Revision03/04 acceptance assumes I031-only Initial/Reset and 30 ordinary tray items.
```

The conflict must not be hidden inside BuildSandbox. Before Revision05, an Item-owned
package must revise the authoritative snapshot/validation semantics for an owned-but-
unplaced I031, preserve its unique non-ordinary identity, and publish the lighting facts
consumed by the board adapter. Cross-System, Enemy and Capability Algorithm Guards must
then review downstream IF01/P1/P6 and layout-resilience effects.

Required stop markers:

```text
GUARD_RETURN_ITEMSYSTEMBATTLESANDBOARDADAPTER01_REVISION04_I031_SEMANTIC_CONFLICT
GUARD_HOLD_REVISION04_UNTIL_I031_ITEM_CONTRACT_ACCEPTED
ITEM_GUARD_REVIEW_REQUIRED_I031_INVENTORY_PLACEMENT
GUARD_KEEP_DEVELOPMENT_STOPPED
GUARD_KEEP_ALL_NEXT_PACKAGES_NOT_STARTED
```

The Revision04 artwork and ItemDetailPanel corrections remain drafted requirements, but
they must be rebased into Revision05 after the I031 Item contract is accepted. No
developer may implement the I031 semantic change by editing only BuildSandbox authority.

### 1.4 Revision05 release after accepted ItemSystem v2 contract

`V0.4-I031InventoryPlacementAndLightingContract01` is now accepted. Its implementation
and QA are prerequisites, not work to repeat in this package:

```text
ItemSystemSnapshot.v2
I031 specialIdentityId = SPECIAL_I031
I031 stablePlacementId = P_SYSTEM_I031
owned Inventory + empty board = Valid/Complete
owned Board + one I031 placement = Valid/Complete
I031 ordinary itemInstanceId / IF01 binding = forbidden
inventory lighting range = empty
board lighting = existing ItemLightingResolver four-direction result
S01-S25 = 25/25 PASS
```

The target BattleSandbox currently fails installation because its old authority creates
an I031 placement but omits the explicit v2 I031 state input. This is the expected
Revision05 migration boundary:

```text
Initial ItemSystem baseline was rejected: I031_LOCATION_UNKNOWN
wrapped by adapter as INSTALL_EXCEPTION_InvalidOperationException
```

Revision05 must migrate only the BuildSandbox adapter/runtime consumer to the accepted
v2 contract. It must not modify ItemSystemSnapshot, I031 contract, IF01, P1 or P6 again.
Where any older section below still describes I031-only Reset, mandatory `(0,0)`,
`JUNIAN_MISSING`, tray rejection or return rejection, this Revision05 section and the
updated explicit scenarios in sections 2, 4, 5, 6, 14 and 15 are authoritative.

### 1.5 Revision06 Item Detail runtime-state context correction

Revision05 received Enemy Guard and Capability Algorithm Guard approval, but Item Guard
correctly returned the Assignment before development with the single blocker:

```text
ITEM_GUARD_RETURN_ITEMSYSTEMBATTLESANDBOARDADAPTER01_REVISION05_ITEMDETAIL_RUNTIME_STATE_CONTEXT_MISSING
```

The Revision05 seam `Show(baseItemId, bool lit)` is insufficient. The cached candidate
detail model contains static preview defaults such as `countedInBuild=false` and Build
progress `0`. Passing only one lighting boolean can therefore display a placed item as
unlit or uncounted even when the authoritative `ItemSystemSnapshot.v2` says otherwise.

Revision06 corrects only that read-only presentation seam:

```text
Show(baseItemId, placementId, authoritative ItemSystemSnapshot.v2)
```

The presenter must clone the immutable base detail model and reuse the existing
`ItemDetailProjectionComposer` with the snapshot's public conversion outputs. It must
not mutate the cached candidate/catalog model, modify Item Detail source, duplicate Item
state algorithms or retain a stale projected model across committed board changes.

Enemy Guard and Capability Algorithm Guard Revision05 receipts remain useful boundary
evidence but do not release Revision06 because the Assignment SHA changes. All three
Guards must reissue the exact receipts in section 0 against Revision06.

### 1.6 Revision07 user-authored target-scene ItemDetailPanel

Revision06 obtained all three Guard receipts, but before development the user confirmed
that runtime prefab instantiation still produced a broken or visually disordered detail
panel. The user therefore copied the already adjusted ItemSandbox `ItemDetailPanel`
visual subtree into the real target scene in Unity Edit Mode and saved it under:

```text
Scene_TalismanBag_V04_BattleSandboxPreview
└─ PopupLayer
   └─ ItemDetailPanel
      ├─ ItemDetailPanelView
      └─ ItemSandboxDetailPanelView (compatibility component; not an authority)
```

Read-only disk verification:

```text
Target Scene SHA-256:
a7dc7e8566b8522a43dc9f5ba5c9cccf8668e25096ececbf2b028aa4e7b86d05

ItemDetailPanel GameObject fileID: 857425570
ItemDetailPanel RectTransform fileID: 857425571
ItemDetailPanelView fileID: 857425572
Parent PopupLayer RectTransform fileID: 1703867848
Serialized active state: true
```

Revision07 treats this saved subtree as the authored UI truth. Runtime must not load,
instantiate or destroy `ItemDetailPanel.prefab`. It must find exactly one direct child
`PopupLayer/ItemDetailPanel`, obtain its existing `ItemDetailPanelView`, call
`SetVisible(false)` as the first successful installation presentation step, then use only
`Bind` and `SetVisible` for interaction. The saved active state and all authored geometry
remain protected; Play-mode visibility changes must not dirty or save the Scene.

The copied `ItemSandboxDetailPanelView` may remain attached for visual-template
compatibility, but authority mode must not call it, discover ItemSandbox sessions through
it or let it become a second presenter. Missing or duplicate authored panel/view is a
stable installation failure with no runtime UI construction fallback.

Revision06 Guard receipts remain useful review evidence but do not release Revision07
because both the Assignment and target Scene baselines changed. All three Guards must
reissue the section 0 receipts against Revision07.

### 1.7 Revision07 user handtest failure and Revision08 DaoPin display baseline

Revision07 completed Unity and Offline static QA at `38/38 PASS`, but the user's real
target-scene handtest failed:

```text
USER_HANDTEST_FAIL / DEV_STOPPED / WAITING_GUARD_REREVIEW
Observed: item artwork/icons appear, but the completed detail panel does not expose the
expected complete player-facing text and item information for ordinary tray items.
```

The current BuildSandbox projection intentionally hard-codes every ordinary item to
`ItemInstanceRarity.White`, stable key `white`, artwork rarity index `1`. The authoritative
Item rarity contract defines the requested UI test tier as:

```text
ItemInstanceRarity.Orange
stable key = orange
display name = 道品
artwork rarityIndex = 5
```

Revision08 changes all ordinary dev-only rows `I001-I030` to that deterministic DaoPin
candidate tier so the existing candidate adapter supplies the richest configured detail
payload for UI coverage. `I031` remains a special system item and never receives an
ordinary rarity or generated instance.

This is only a BattleSandbox display/handtest baseline. It is not a catalog default,
formal item acquisition rule, drop probability, rarity probability, saved inventory
instance, map reward result or future random-drop decision. The later map random-drop
system remains responsible for selecting real instance rarity and generated attributes.

DaoPin does not excuse an empty visible panel. Revision08 must verify both layers:

```text
model layer: exact Orange candidate and configured player-facing fields are non-empty
view layer: after binding the authored scene panel, header and every model-visible player
            section actually contain visible text
```

If the model is populated but the bound authored panel still shows no text, Revision08
must fail with a stable diagnostic and return to Guard. It must not hard-code text into
the panel, edit ItemDetailPanelView, rebuild UI or accept the rarity change as a false fix.

### 1.8 Revision08 S20F failure and Revision09 generic Body visibility fix

Revision08 stopped correctly at its static boundary:

```text
GUARD_RETURN_REQUIRED
S20E: PASS / I001-I030 Orange models 30/30 complete
S20F: FAIL / I001 TriggerConditionSection has no visible Body text
Unity compile: PASS
Offline / StaticBatch: 39 PASS / 1 FAIL
User handtest: NOT_STARTED / BLOCKED_BY_STATIC_QA
```

Read-only diagnosis proves this is not missing trigger data. The authoritative
`ItemDetailProjectionComposer` projects a non-empty trigger section for I001, using
`当前版本暂未配置` when no specific trigger text exists. Both ItemSandbox and the copied
BattleSandbox authored UI persist `TriggerConditionSection/BodyText` inactive. The shared
`ItemDetailSectionView.SetContent` writes the projected body but does not restore the
plain Body renderer's runtime visibility, so a populated model can remain invisible.

Revision09 fixes this once in the shared section presentation seam. It must not edit
fifteen Scene sections individually and must not modify ItemDetailPanelView. Required
behavior:

```text
hidden model section:
  section root remains inactive

plain model-visible section without authored row renderer:
  section root active
  Body container active
  Body Text enabled, alpha-visible and non-empty

section rendered by authored rows:
  required authored row renderers remain visible
  source Body Text may be disabled, but its GameObject must not hide authored descendants

state transitions:
  hidden -> plain -> authored rows -> plain restore the correct visibility each time
  no stale text, duplicate body, hidden descendant or layout rewrite
```

The fix must apply consistently to both `SetContent(ItemDetailSectionViewModel)` and the
string overload. It may change only runtime active/enabled presentation state already
owned by `ItemDetailSectionView`; it must not change serialized geometry, hierarchy,
fonts, colors, Scene, Prefab or model content.

## 2. Package goal

When the user opens the real V0.4 battle sandbox scene and presses Play:

```text
1. The existing page displays the exact I001-I031 roster in Ordinal identity order: Initial/Reset places I001-I031 in the tray and leaves the board empty.
2. I001-I030 each use one deterministic dev-only DaoPin/Orange generated instance for complete UI coverage.
3. I031 is one owned special item with `SPECIAL_I031 / P_SYSTEM_I031`; it never receives an ordinary itemInstanceId and can move tray -> board -> tray.
4. Existing multi-cell tray layout, scrolling, drag, ghost, rotate-zone and return behavior remain visible and usable; a tap opens the completed ItemDetailPanel with the selected authoritative Item instance and the latest committed ItemSystem runtime state.
5. Legal placement, move, rotate and return-to-tray transactions are accepted by one ItemSystem board authority.
6. The resulting ItemSystemSnapshot.v2 and IF01 binding snapshot are the only authoritative board state.
7. The existing BuildSandbox receiver, placedItemIds and PreviewItem objects become render/input caches while this authority is installed.
8. A successful changed snapshot refreshes layout-resilience evaluation once and appends one Chinese feedback line to the existing feedback surface.
9. I031 on the board visibly shows the exact ItemSystem LitRangeCells; inventory state clears the range and all I031-derived powered states.
```

This is a devOnly visible vertical slice. It is not formal Battle, UnifiedBattle promotion, inventory, reward, save, chapter progression, or balance approval.

## 3. Target and non-target containers

Authoritative handtest target:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Explicit non-targets:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity
Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity
Assets/_Game/Scenes/Scene_TalismanBag_V03_MainHome.unity
```

`Scene_TalismanBag_V04_UnifiedBattlePageShell` remains an isolated shell/placeholder and is not promoted by this package.

## 4. Single-authority architecture

Required runtime chain:

```text
Editor-only scene bootstrap
  -> ItemSystemBattleSandboxBoardAdapter
  -> ItemSystemBattleSandboxBoardAuthority
  -> transactional candidate placements
  -> DefaultItemSystemSnapshotProvider.CreateSnapshot
  -> IF01 Validate
  -> commit authority state only after both are valid
  -> BuildGridInteractionPreviewController render/input seam
  -> P6 structural evaluation once per changed authority snapshot
  -> existing PlacementFeedbackText / FeedbackRoot
```

Forbidden chain:

```text
old board commit
  -> read boardReceiver.OccupiedCells
  -> guess instance and placement identities
  -> synthesize ItemSystemSnapshot after the fact
```

The old board receiver may continue to provide screen-point-to-cell, ghost rendering and temporary drag visualization. It must not remain a second committed-state authority when the new adapter is installed.

### 4.1 Authority transaction rules

The new authority must provide explicit operations equivalent to:

```text
PreviewPlacement(baseItemId, anchorCell, rotation, ignoredPlacementId)
CommitFromTray(baseItemId, anchorCell, rotation)
CommitMove(placementId, anchorCell, rotation)
ReturnToTray(placementId)
Reset()
```

`ReturnToTray` applies to ordinary `I001-I030` and the unique special I031 placement.
Returning I031 removes only its placement row and changes its explicit v2 location to
Inventory; ownership and both stable identities remain unchanged.

The initial and Reset authority state is the accepted v2 inventory baseline:

```text
I031 ownership = Complete
I031 location = Inventory
I031 specialIdentityId = SPECIAL_I031
I031 stablePlacementId = P_SYSTEM_I031
I031 board placement count = 0
ordinary placements = 0
tray entries = I001-I031
LitRangeCells = empty
```

`CommitFromTray(I031)` submits an explicit Board state with the stable placementId;
`CommitMove` preserves both identities; `ReturnToTray(I031)` submits the explicit
Inventory state. Reset restores Inventory, not `(0,0)`. Initial installation performs
IF01 validation and P6 refresh exactly once. Reset from a changed state performs each
exactly once; Reset while already at the same canonical baseline performs neither again.

Every successful mutation must be transactional:

```text
candidate placement rows
  -> explicit I031 ownership/location input
  -> candidate ItemSystemSnapshot
  -> candidate IF01 binding snapshot
  -> both valid
  -> replace committed authority state
```

Every authority call to `DefaultItemSystemSnapshotProvider.CreateSnapshot` must submit
the explicit v2 I031 input. The adapter may not use the legacy compatibility factory or
omit `i031StateInputs`. Expected states are:

```text
Initial / Reset / after I031 return: Complete + Inventory + zero I031 placement
I031 tray commit / move: Complete + Board + exactly P_SYSTEM_I031 placement
ordinary-only mutation: preserve the current explicit I031 state unchanged
```

IF01 input continues to contain only ordinary board placements/projections. Inventory
I031 and Board I031 both produce no ordinary IF01 row.

If either candidate is invalid, committed state remains byte-for-byte/logically unchanged. Unknown or Invalid must never be converted to empty, zero, legal, or success.

### 4.2 Render cache rules

After a successful authority commit, the existing controller must rebuild its local render/input caches from the committed ItemSystemSnapshot:

```text
boardReceiver occupied cells
placedItemIds
PreviewItem current rotation
tray hidden/visible state
tray packing state
board placed artwork
formation/lighting overlays
selected item summary
```

The controller must not independently preserve a conflicting committed placement after synchronization.

When no authority is installed, the existing BuildSandbox fallback behavior must remain available for existing static QA. The two modes must never be active simultaneously.

## 5. Roster and identity contract

Roster source:

```text
ItemInnerDataCatalog.AllItems
exactly I001-I031
Ordinal order by baseItemId
```

Ordinary items:

```text
I001-I030
one deterministic dev-only candidate instance per baseItemId
rarity = ItemInstanceRarity.Orange for every ordinary candidate
rarityKey = orange
display rarity = 道品
rarityIndex = 5 for the shared Item artwork
catalog rarityDefault is not read, mapped or interpreted by this package
seed = stable documented seed derived from package seed + item ordinal
projection identity remains explicit
```

This is a dev-only handtest choice, not a mapping between `ItemCatalogRarity` and
`ItemInstanceRarity`, and not a formal acquisition, drop or rarity rule.

System item:

```text
I031 聚念石
one system roster row
one tray placement candidate while its v2 location is Inventory
zero or one board placement according to its v2 location
specialIdentityId = SPECIAL_I031
stablePlacementId = P_SYSTEM_I031
no ordinary Item generated instance
no ordinary itemInstanceId
no IF01 ordinary binding row
```

The UI may use `baseItemId` as its unique card/view key because this package deliberately exposes only one candidate per base item. This is a UI key, not an identity inference. Internally, ordinary identity must remain:

```text
itemInstanceId != placementId
baseItemId is explicit
binding.itemInstanceId + binding.placementId + binding.baseItemId are explicit
```

Never infer identity from display name, item card index, shape, sprite, or hierarchy name.

## 6. Shape and placement rules

Shape source is only `ItemInnerDataCatalog` / `ItemSystemSnapshot`:

```text
shapeId
ShapeCells/defaultLocalCells
coreCellLocal
anchorCell
rotation
OccupiedCells
```

Do not retain the five hard-coded preview shapes as authority while ItemSystem mode is active. Runtime view shape configs may be projected from the catalog solely for the existing ghost/visual system.

Required rules:

```text
all occupied cells inside 5x5 board
no overlap
center eye cell blocked according to ItemSystem placement rules
I031 unique
same baseItemId cannot be placed twice in this one-candidate roster
rotation uses anchorCell + rotated occupied offsets
invalid preview is red and does not mutate committed state
invalid release returns to the exact source state
```

Do not convert occupied-cell count, shape size, compactness, isLit or isCountedInBuild into BP.

### 6.1 Logical tray capacity correction

Authority mode uses this exact logical tray capacity:

```text
columns = 5
logical rows = 13
logical slots = 65
authored slots retained = TrayGridSlot_01 through TrayGridSlot_40
runtime-only slots added = TrayGridSlot_Runtime_41 through TrayGridSlot_Runtime_65
ordinary footprint cells required = 62
I031 special footprint cells required = 1
Initial/Reset total tray footprint cells = 63
last occupied logical slot index at Initial/Reset = 63
packing = deterministic row-major using approved catalog shapes
```

Runtime slot rules:

```text
created only while the ItemSystem authority is installed in the exact target Scene
parented under the existing ItemTrayContent/Content used by the current ScrollRect
HideFlags = DontSaveInEditor | DontSaveInBuild
use the existing GridLayoutGroup cellSize, spacing, padding and five-column constraint
must not modify, rename, replace, reparent or reorder authored TrayGridSlot_01-40
must not change the authored viewport, ScrollRect, 800x800 visible area or card artwork geometry
only the Content vertical extent may increase at runtime to expose all 13 logical rows
authority uninstall removes slots 41-65 and restores the original Content vertical extent
non-authority fallback continues to use the existing 40 authored slots
```

The runtime extension is a capacity seam for the existing scrollable tray, not a second
tray implementation and not permission to save generated slots into the Scene or Prefab.

### 6.2 ItemSystem lighting display seam

While authority mode is active, visible power state must be projected only from the
committed ItemSystemSnapshot.v2:

```text
range cells = snapshot.LitRangeCells
source state = I031 placement.isLightingSource / isLit
ordinary state = placement.isLit / isDirectLit / litByPlacementId / litDepth
Build eligibility display = placement.isCountedInBuild
```

The controller may reuse its existing runtime `FormationCorePowerRangeOverlay` cell
objects and existing selected-item badge/text surface, but only as renderers. It must not
call `FormationEnergyContractResolver`, legacy provider tags or item names to recompute a
second power result while ItemSystem authority is active.

Required visible behavior:

```text
I031 in Inventory: no powered-range cells; every ordinary item shows unpowered/unlit.
I031 at center-adjacent legal interior cell: exact four orthogonal cells highlighted.
I031 at board edge: exact clipped two/three-cell range highlighted.
Moving I031: old range and old badges disappear before the new committed range is shown.
Returning I031: all I031-derived range, direct-lit, relay-lit and powered badges clear.
Placing an ordinary item whose core enters/leaves the range updates its visible state.
I031 artwork switches only between its accepted unlit/lit sprites from committed state.
```

All runtime range overlays must keep `blocksRaycasts=false`, must not alter authored
RectTransform geometry and must be removed/reset on authority uninstall. A Chinese text
line without visible cell range and item-state change is not acceptance evidence.

## 7. Interaction preservation

The package must preserve the current real V0.4 battle-page feel:

```text
tap tray item = open the completed ItemDetailPanel for the exact authority row/instance
tap placed board item = open the same completed ItemDetailPanel in read-only form
drag beyond current threshold = begin placement
tray scrolling remains usable
multi-cell item is displayed and dragged as its real footprint
drag ghost remains aligned by placement anchor and occupied cells
existing rotate-zone behavior remains the only drag rotation route
valid release commits
invalid release returns to source
board item can move or return to tray
battle-locked drag remains blocked with existing Chinese toast
```

No info-panel rotate button may be reintroduced. No second-confirm ghost workflow may be reintroduced.

While ItemSystem authority mode is installed, `BuildSandboxItemInfoPanel.Show` is not a
valid tap destination. The legacy panel may remain serialized and untouched for fallback
mode, but it must stay hidden and receive zero Show calls from authority-mode taps. Tap
and drag suppression remain governed by the existing gesture thresholds; opening detail
must not also begin a drag.

This package must not tune thresholds, animation timing, authored RectTransforms, colors,
scrolling speed or visible layout dimensions. Revision03 grants only the exact runtime
Content vertical-extent change required by section 6.1. Revision07 additionally permits
runtime `Image.sprite` binding/suppression required by section 8.1 and Show/Hide/Bind calls
on the one existing authored `PopupLayer/ItemDetailPanel`; it grants no authored layout,
hierarchy, component or serialized active-state change.

## 8. Item generation and catalog loading

This is an Editor Play dev-only package. It may load the existing workbench catalog at the exact path:

```text
Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset
```

The Editor-only bootstrap may use `AssetDatabase.LoadAssetAtPath` inside `#if UNITY_EDITOR`. Runtime code must receive an already resolved catalog/dependency and must not reference `UnityEditor` outside conditional compilation.

Allowed source reuse:

```text
ItemBalanceCandidateDetailSandboxAdapter
ItemInnerDataCatalog
ItemInnerDataCatalogProvider or an equivalent read-only in-memory provider
DefaultItemSystemSnapshotProvider
ItemInstanceProjectionProvider outputs
ItemInstancePlacementBindingValidator
DefaultRealLayoutResilienceEvaluationPipeline
LayoutResilienceBattleSandboxPlaytestFeedback pure Chinese mapping helper
ItemDetailProjectionComposer and ItemSystemSnapshot.v2 public conversion methods
```

Forbidden:

```text
ItemSandboxV04BoardFullDetailAdapter UI/controller reuse
ItemSandbox scene bootstrap
Builder execution
profile regeneration
rerolling formal inventories
formal generation or balance approval
```

Allowed Item-detail reuse is deliberately narrower than ItemSandbox controller reuse:

```text
PopupLayer/ItemDetailPanel authored target-scene subtree (bind/show/hide only)
TalismanBag.Items.Detail.UI.ItemDetailPanelView (public Bind/SetVisible/Close only)
ItemBalanceCandidateDetailResult.viewModel already produced by the authority projection
ItemInnerDataCatalogProvider.GetDetailViewModel for system item I031
TalismanBag.Items.Detail.ItemDetailProjectionComposer (read/reuse only)
```

The runtime bootstrap must resolve the existing authored panel only by the exact target
Scene and exact hierarchy path. It must not load `ItemDetailPanel.prefab`, call
`AssetDatabase.LoadAssetAtPath` for the panel, instantiate a replacement or use
`ItemSandboxDetailPanelView` as the authority. This remains an Editor Play devOnly adapter
and does not claim formal APK/Addressables delivery.

### 8.1 Shared Item artwork projection

The Item Detail page and the V04 battle tray/board use the same approved Item artwork.
This package must bind that existing artwork into the existing V04 card, drag ghost and
placed-board artwork slots. It must not create a second visual catalog or copy the PNGs.

For ordinary items `I001-I030`, the resource path is derived only from authoritative
Item fields and the selected deterministic candidate rarity:

```text
item_daoju/{faMenDisplayName}/{baseItemId}/{baseItemId}_{rarityIndex}
```

The mapping between rarity and `rarityIndex` must match the existing Item Detail
presentation contract. It must not be inferred from hierarchy order, PNG dimensions,
shape size, display-name keywords or card index.

For `I031`, use the existing system-item sprites:

```text
item/聚念石icon/聚念石icon_未点亮
item/聚念石icon/聚念石icon_点亮
```

Rules:

```text
I001-I030 Orange/DaoPin tray artwork resolves non-null using rarityIndex 5
the same resolved sprite is actually bound to the final visible tray artwork slot, drag ghost and board placement
rotation rotates the existing whole-item artwork with the shape; it does not select another PNG
I031 changes only between its existing lit/unlit status sprites
missing expected artwork is a stable QA failure and Chinese diagnostic
no fallback may silently reuse another item's sprite
no Image, RectTransform, Sprite asset, PNG, import setting, Scene or Prefab is modified
```

For each authority card, the binding path must identify the actual visible whole-item
artwork slot, including existing `*_image` children. It must not stop after assigning the
card root while a legacy child artwork Image remains visible. Exactly one whole-item
artwork renderer is authoritative per card; other legacy whole-item sprite renderers are
suppressed at runtime and restored on uninstall. Generated runtime cards may use their
root Image when no authored child artwork slot exists.

The runtime adapter may update existing `Image.sprite`, enabled/active presentation state
and existing render caches. It must record enough pre-authority state to restore every
touched Image on uninstall. It must not resize, reparent, reorder or recreate authored
artwork slots.

### 8.2 Completed Item detail runtime projection

Each authority row owns only an immutable base detail model and an artwork resolver:

```text
I001-I030: ItemBalanceCandidateDetailResult.viewModel for the exact Orange/DaoPin instance/rootSeed
I031: ItemInnerDataCatalogProvider.GetDetailViewModel("I031")
artwork: the same row.ResolveSprite(currentLitState) used by tray/ghost/board
```

The cached row model is not the final runtime detail model. On every `Show`, the presenter
must use the latest committed authoritative snapshot supplied by the board authority,
clone the row's base model and project the current state with the existing public Item
contracts:

```text
ItemDetailProjectionComposer.Compose(
    cloned base model,
    context kind,
    placementId,
    authoritativeSnapshot.ToLightingResolutionResult(),
    authoritativeSnapshot.ToArrayBonusResolutionResult(),
    authoritativeSnapshot.ToBuildSynergyResolutionResult(),
    authoritativeSnapshot.ToCoreAwakeningResolutionResult(),
    authoritativeSnapshot.ToSkillMonitorResolutionResult())
```

The composer is the only approved detail-state projection path. The BuildSandbox adapter
must not reproduce its lighting, direct/relay, array-bonus, Build, awakening/core or skill
monitor logic. `ItemDetailProjectionComposer` already clones its input; the presenter may
make an additional defensive clone, but must never mutate or replace the cached base model.

Runtime context rules are exact:

```text
tray/unplaced item:
  placementId = empty
  context = CatalogPreview
  snapshot must still be the latest valid ItemSystemSnapshot.v2
  do not invent a placement, KnownZero Build result or powered state

placed board item:
  placementId = exact authoritative placementId
  baseItemId + placementId must match exactly one snapshot placement by Ordinal identity
  context = PlacedInstance
  all current lighting, direct/relay, array-bonus, Build-counted/progress,
  awakening/core and skill-monitor state comes from that same snapshot

I031 in Inventory:
  placementId = empty
  use the I031 catalog/system base model and CatalogPreview context

I031 on Board:
  placementId = P_SYSTEM_I031
  use the exact v2 placement and current lighting result; never fabricate itemInstanceId
```

The detail presenter must:

```text
resolve exactly one authored PopupLayer/ItemDetailPanel and its ItemDetailPanelView
call ItemDetailPanelView.SetVisible(false) immediately after successful resolution
never instantiate, clone, reparent, resize, reorder or destroy the authored panel
call ItemDetailPanelView.Bind(projectedRuntimeModel, authoritativeSprite), then SetVisible(true)
reuse the same authored panel for every tap
re-project on every Show from the latest committed snapshot; never cache a placed result
support the authored panel's existing Close behavior
on authority uninstall, unbind adapter listeners and SetVisible(false) without destroying it
show no legacy BuildSandboxItemInfoPanel while authority mode is active
remain read-only: no rotate, placement, reroll, reward, save or formal mutation control
```

Freeze the Revision09 seam to one BuildSandbox-owned presenter with equivalent methods:

```text
ItemSystemBattleSandboxItemDetailAdapter.Initialize(
    ItemDetailPanelView authoredScenePanel,
    IReadOnlyList<ItemSystemBattleSandboxViewRow> rows)

Show(
    string baseItemId,
    string placementId,
    ItemSystemSnapshot authoritativeSnapshot)
Hide()
Uninstall()
```

`Initialize` succeeds only when all 31 rows have a non-null base detail model and required
artwork and the supplied view is the unique component at the exact authored hierarchy
path. It must hide the panel before exposing successful installation. `Show` resolves the
row by Ordinal `baseItemId`; display name, card order and sprite must never be used as
identity. It accepts only non-null, Valid/Complete
`ItemSystemSnapshot.v2`. A non-empty `placementId` must resolve exactly one matching
placement for the same `baseItemId`; missing, stale, duplicate or mismatched placement
identity is a stable failure and must clear/hide the detail instead of displaying stale
state. Empty `placementId` is valid only for an authority row currently in the tray.
`Uninstall` is idempotent. The controller may hold only this presenter seam; it must not
know ItemSandbox session/controller internals.

Missing/duplicate authored panel, panel component, base view model, authoritative snapshot,
placement identity, converted state or authoritative sprite is a stable install/QA
failure. Raw exception text, stack traces and internal IDs must not enter player-visible
text. There is no fallback to the old detail panel and no fallback to a static placed-state
model, prefab load or runtime UI construction.

## 9. Minimal runtime seams

The existing runtime files authorized for targeted Revision07 modification are:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs
```

The controller modification must be limited to:

```text
install/uninstall one item board authority
reinitialize item/shape lookup from authority view rows
delegate preview/commit/move/return/reset to authority when installed
rebuild render caches from the committed ItemSystemSnapshot
expose/read current authoritative ItemSystemSnapshot and IF01 result
append one external Chinese diagnostic line through the existing feedback view
produce BuildSandboxLayoutSnapshot only as a one-way compatibility projection from ItemSystemSnapshot while authority mode is active
route authority-mode tap detail to one installed ItemSystemBattleSandboxItemDetailAdapter
keep BuildSandboxItemInfoPanel hidden while authority mode is active
bind and restore the actual visible tray artwork renderer for every authority row
```

It must not:

```text
change serialized fields used by the scene
change existing RectTransform writes or add layout authoring
change drag thresholds or rotate-zone tuning
change battle prepare motion
change enemy combat loop
change formal flow
```

The card-view modification must be limited to an explicit authoritative-artwork bind and
restore seam. It may recognize the existing authored `*_image`, `Artwork`, `Icon`, `Image`,
`Picture`, `Sprite` or `Visual` child as the whole-item renderer, and use the root Image
only when no such child exists. It must not change gesture routing, thresholds, card
geometry, hierarchy or text layout.

The tray-view modification must be limited to:

```text
install/uninstall the section 6.1 runtime logical-slot extension on authority lifecycle
append exactly TrayGridSlot_Runtime_41-65 under the existing Content
reuse existing GridLayoutGroup geometry and existing slot visual/component pattern
extend only runtime Content vertical extent for 13 logical rows
expose the resulting 65 slot references to the existing tray/card rendering path
remove runtime slots and restore the original Content vertical extent on uninstall
preserve the existing 40-slot fallback when authority is absent
```

It must not:

```text
save runtime slots into the Scene or Prefab
modify authored TrayGridSlot_01-40
change viewport, ScrollRect, GridLayoutGroup cellSize/spacing/padding/constraint
change card RectTransform, artwork slots, scroll speed, drag threshold or rotation behavior
create a second ScrollRect, Content root, tray root or shape-grid authority
run EditMode authoring/discovery code to dirty the Scene
```

If implementation proves that any other existing BuildSandbox view or shape-grid file must be modified, stop and return to Guard with exact evidence. Do not expand the whitelist autonomously.

## 10. Runtime installation

The adapter must install without saving the scene:

```text
RuntimeInitializeOnLoadMethod / sceneLoaded, Editor Play only
exact target scene path match
exactly one BuildGridInteractionPreviewController required
exactly one direct child PopupLayer/ItemDetailPanel with ItemDetailPanelView required
hide that authored panel before authority installation is reported successful
runtime GameObject/components use DontSave flags
runtime-only slots may SetParent only to the existing tray Content
no SetParent/reorder/resize of authored UI; only the approved runtime Content height may change
no Scene save
```

Zero or duplicate target controllers, PopupLayer nodes, ItemDetailPanel roots or
ItemDetailPanelView components is Invalid and must produce one stable Chinese error plus
a deterministic diagnostic code. It must not silently fall back to a second authority,
legacy panel, prefab instance or ItemSandbox controller.

## 11. Layout-resilience refresh

After each distinct successful ItemSystemSnapshot change:

```text
IF01 validation = exactly 1
P6 evaluation = exactly 1
same snapshot reference/signature = no duplicate refresh
failed preview/commit = no P6 refresh
```

Player-visible output appends one line beginning:

```text
【阵势韧性】
```

Reuse the existing Chinese feedback mapping. Do not display pressure coordinates, internal IDs, English stable keys, full solution clauses, DropBias, answer tags, or readiness internals.

P6 output remains a separate structural diagnostic. It must not alter the existing BuildSandbox combat result, enemy HP, player HP, reward, save, or chapter state.

## 12. Allowed file scope

Allowed new files:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxItemDetailAdapter.cs.meta
```

Allowed targeted existing-file modification:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemSystemBattleSandboxBoardAdapterVerifier.cs
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardAdapterReport.md
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardRoster.csv
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardStateMatrix.csv
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardLeakCheckReport.md
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardManualTest.md
```

Revision09 resumes from the current Revision08 partial workspace. Its effective write
whitelist replaces the broader historical list above:

```text
Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemSystemBattleSandboxBoardAdapterVerifier.cs
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardAdapterReport.md
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardRoster.csv
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardStateMatrix.csv
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardLeakCheckReport.md
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardManualTest.md
```

The current Revision08 change in
`ItemSystemBattleSandboxViewProjection.cs` is retained but read-only in Revision09.
`ItemSystemBattleSandboxItemDetailAdapter.cs`, `ItemDetailPanelView.cs`, all Scene/Prefab
files and every other Item source remain read-only. If the fix cannot be completed in
`ItemDetailSectionView.cs` plus the existing verifier/reports, stop and return to Guard.

Expected additional Revision09 delta relative to the current Revision08 partial state:

```text
new files = 0
modified existing runtime/editor code files <= 2
modified existing report files <= 5
scene files modified = 0
prefab files modified = 0
Item source files modified = 1 (exact ItemDetailSectionView.cs only)
ItemSandbox source files modified = 0
CrossSystem/Enemy source files modified = 0
```

Only corresponding Unity `.meta` files listed above are allowed. Do not modify Guard queue or Assignment from the development window.

## 13. Strict forbidden scope

Do not modify:

```text
Assets/_Game/Scenes/**
Assets/_Game/Prefabs/**
ProjectSettings/**
Packages/**
Assets/_Game/Scripts/TalismanBag/ItemSandbox/**
Assets/_Game/Scripts/TalismanBag/Items/** except the exact Revision09-whitelisted
Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs
Assets/_Game/Configs/ItemBalanceWorkbench/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/**
Assets/_Game/Scripts/TalismanBag/CrossSystem/**
Assets/_Game/Scripts/TalismanBag/UnifiedBattle/**
Assets/_Game/Scripts/TalismanBag/V02/**
Assets/_Game/Scripts/TalismanBag/V03/**
```

The following nearby BuildSandbox file remains explicitly forbidden:

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayGrid.cs
```

Do not call or write:

```text
RunFlow
SaveData
RewardService
Chapter progression
formal inventory
formal battle damage
formal Boss state
drop or rarity probability
BuildSettings
Scene builder/binder
Item detail prefab builder
```

Do not commit, tag, push, reset, clean, rollback, or reorganize the dirty worktree.

## 14. Required verifier scenarios

Offline and Unity verifier must cover at least:

```text
S01 exact I001-I031 roster, Ordinal and unique; Initial/Reset packs all 31 tray items successfully
S01A deterministic row-major packing rejects 40 and 60 slots, accepts 65 slots, uses 63 footprint cells and ends at logical slot 63
S01B authority install preserves authored slots 01-40, creates exactly runtime slots 41-65, and uninstall removes them and restores original Content height
S02 I001-I030 deterministic Orange/DaoPin projections and explicit identities; catalog rarityDefault is never read
S03 I031 has no ordinary instance/binding, starts owned in Inventory, and retains SPECIAL_I031/P_SYSTEM_I031 through tray->board->move->tray
S04 every catalog shape projects exact cells/core/rotation semantics
S04A I001-I030 Orange/DaoPin artwork resolves 30/30 from the shared Item path with rarityIndex 5
S04B I031 lit/unlit artwork resolves 2/2 and no other item sprite is reused
S04C final visible tray renderer is authoritative for 31/31 cards; confirmed regression samples I004/I005/I007 pass and authored *_image children cannot retain a legacy sprite
S04D the same exact Sprite reference is used by tray, drag ghost and placed-board whole-item artwork; uninstall restores every touched card Image
S05 owned Inventory I031 plus empty placements is ItemSystemSnapshot.v2 Valid/Complete with empty LitRangeCells
S05A old caller omission of explicit I031 state is rejected deterministically and cannot install as success
S06 legal ordinary or I031 tray-to-board commit submits explicit v2 state and updates ItemSystem + IF01/P6 once
S07 overlap/out-of-bounds/eye-cell commit rejected without state change
S08 board move commits atomically
S09 rotate preview/commit uses ItemSystem legality and anchor semantics
S10 ordinary board-to-tray return restores its tray entry; I031 return removes its placement, preserves ownership/identities and clears lighting
S11 reset returns I001-I031 to tray, leaves the board empty and clears all powered range/state
S12 same snapshot, repeated initial baseline or no-op Reset does not repeat IF01/P6
S13 changed snapshot and changed-state Reset perform IF01/P6 once
S14 malformed/unknown dependency remains Invalid or Unknown, never empty success
S15 BuildSandbox compatibility snapshot is derived one-way from ItemSystem
S16 runtime target path accepts only real V04 BattleSandbox scene
S17 ItemSandbox and UnifiedBattle shell paths are rejected
S18 no scene/prefab/authored-layout/formal-system write; only approved runtime Content height, DontSave slot lifecycle and authored-panel visibility occur
S19 exactly one authored PopupLayer/ItemDetailPanel with ItemDetailPanelView is resolved, hidden on install, reused across taps and hidden without destruction on uninstall
S19A no ItemDetailPanel prefab load/instantiate, runtime detail-root creation, ItemSandbox presenter call, reparent, reorder, resize or authored component mutation occurs
S20 authority tray and board taps bind the exact row base ItemDetailViewModel plus authoritative Sprite; I001-I030 instance identity and I031 catalog identity are not guessed
S20A tray/unplaced detail uses empty placementId plus CatalogPreview against the latest valid v2 snapshot; it does not invent a placed KnownZero state
S20B placed detail resolves exact baseItemId + placementId, converts all five public v2 state snapshots and projects lighting/direct/relay/array/Build/awakening/skill-monitor state through ItemDetailProjectionComposer
S20C each Show creates a fresh projected clone; cached base models remain byte/field equivalent before and after, while a changed committed snapshot changes the visible runtime state
S20D null/invalid/wrong-schema snapshot and missing/stale/duplicate/mismatched placementId fail deterministically, clear the detail and never display a stale or static placed model
S20E I001-I030 Orange models are 30/30 non-empty for item name, 道品 rarity, item power, shape and every configured player-visible section; no candidate silently falls back to the unavailable model
S20F read-only binding against the authored scene ItemDetailPanel makes itemName/meta/power text visible and gives every model-visible player section a non-empty visible title/body; the verifier hides the panel afterward, never saves the Scene and proves the Scene SHA unchanged
S20G ItemDetailSectionView transitions hidden -> plain -> authored rows -> plain restore Body/row visibility without duplicate text, stale state, hierarchy mutation or serialized layout change
S21 authority taps call BuildSandboxItemInfoPanel.Show zero times; missing detail dependencies fail without legacy fallback
S22 tap/drag suppression, close behavior and battle read-only detail behavior remain deterministic
S23 Inventory I031 renders zero powered cells and all ordinary placements visibly unlit/uncounted
S24 interior/edge/corner I031 placements render exact snapshot LitRangeCells; source cell is not highlighted as a target
S25 moving/returning I031 clears stale range, direct/relay links, badges and lit artwork before showing committed state
S26 authority mode never calls FormationEnergyContractResolver or legacy provider-tag logic to decide power
```

The verifier must also scan that authority-active commit paths do not call `boardReceiver.Commit` or mutate `placedItemIds` before ItemSystem acceptance.
It must verify that no runtime slot is serialized, no authored slot is renamed/reparented/reordered,
and no viewport, ScrollRect or GridLayoutGroup geometry is mutated.
For S20F it may open the target Scene read-only, invoke the existing `Bind`/`SetVisible`
path in memory and inspect the referenced `Text`/section components. It must reject
disabled core header text, zero-alpha visible text, blank visible titles/bodies and a
model/view content mismatch. It must not call any Scene save API and must prove the
on-disk Scene hash is unchanged before and after the check.
It must inspect the actual runtime card hierarchy used by the target Scene, including the
existing `ItemCard_04_image` and `ItemCard_05_image` pattern; resource-resolution-only
assertions are insufficient. It must also open or inspect the target Scene read-only and
prove the exact `PopupLayer/ItemDetailPanel/ItemDetailPanelView` path is unique, all
serialized references required by the existing view remain intact, and the Scene and
prefab hashes do not change. No Builder, prefab load/instantiate path, scene save or prefab
save API may be called.

Verifier entry points:

```text
TalismanBag.EditorTools.BuildSandbox.ItemSystemBattleSandboxBoardAdapterVerifier.VerifyOffline
TalismanBag.EditorTools.BuildSandbox.ItemSystemBattleSandboxBoardAdapterVerifier.VerifyStaticBatch
```

## 15. User handtest acceptance

After static QA, status must remain:

```text
DEV_COMPLETE / QA_STATIC_PASS / WAITING_USER_HANDTEST
```

The user handtests only:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Checklist:

1. Play opens the existing real V04 battle/prepare page; ItemSandbox detail workbench is not involved.
2. The page shows the exact I001-I031 roster: the unchanged 5-column viewport scrolls through all 13 logical rows containing I001-I031, and Initial/Reset leaves the board empty.
3. On entering Play the authored ItemDetailPanel is hidden before interaction. Single-tapping any tray item opens that same scene-authored panel, not BuildSandboxItemInfoPanel or a runtime-created copy; closing it returns to the unchanged battle/prepare page.
4. The detail shows the tapped exact Orange/道品 dev instance for I001-I030 with rarityIndex 5 artwork, and the catalog/system detail for I031; no unrelated item's name, art or instance identity appears. Tray items show unplaced/catalog context rather than a fabricated placed-state zero.
4A. For several ordinary items across different法门, visibly confirm item name, 道品, item power, shape, basic stats, fixed affix, random affix, core/道痕 information and every section that the model marks visible. No visible section may be an empty box. Legitimately unavailable sections remain hidden rather than showing fabricated text.
5. Tray scroll, drag, ghost and rotate-zone feel has not regressed; a drag does not also open detail, and tray cell size, spacing and visible 800x800 area look unchanged.
6. I001-I030 can be selected and legally placed according to their approved catalog shapes; every item uses its completed Item artwork in the tray, ghost and board, with no old BuildSandbox sprite visible above or behind it.
7. Tapping an already placed board item opens the same completed read-only detail; its point-lighting/direct-or-relay, array-bonus, Build-counted/progress and awakening/core state match the current board. Dragging it still moves/returns it instead of opening detail.
8. I031 appears exactly once in the tray at reset; it can be placed on a legal non-eye cell, moved, returned to the tray, and cannot be duplicated.
9. Overlap, out-of-bounds and eye-cell releases are rejected and return cleanly.
10. Ordinary and I031 placements can move and return to the tray without jumping, disappearing or duplicating; Reset leaves the board empty.
11. I031 in the tray shows no powered range. Placing it visibly highlights the exact orthogonal ItemSystem range; moving it clears the old range; returning it clears all range and powered badges.
12. Ordinary items entering/leaving that range visibly switch powered/unpowered and Build-counted state from ItemSystem; reopening their detail reflects the new committed state without stale data. Changed layouts also append one `【阵势韧性】...` line.
13. Existing enemy/Boss feedback and battle-page controls remain visible and unchanged.
14. Console has no package Error/Warning, including no `I031_LOCATION_UNKNOWN` or wrapped install exception.
15. Exit Play leaves the authored ItemDetailPanel present and unchanged, removes the 25 runtime-only tray slots, restores every touched card Image/range overlay and the original Content height, leaves the target Scene clean and keeps its SHA-256 unchanged.

Only the user's explicit handtest PASS may release final acceptance.

## 16. Protected baseline

Revision09 task start and end must preserve these exact current-disk baselines except for
the explicitly whitelisted runtime/editor/report files in section 12:

```text
Target Scene:
a7dc7e8566b8522a43dc9f5ba5c9cccf8668e25096ececbf2b028aa4e7b86d05

BuildGridInteractionPreviewController.cs (allowed to change):
261ecac0ecc8b6fe4113fcce8517f30876f346a2a5f90da6b80b66108f784fe3

BuildItemTrayPreviewView.cs (allowed to change only under sections 6.1 and 9):
29a65246902f2eb7a193e4c53a52674e293ca3a4ff9d64cf41f4f5482b030c21

BuildItemPreviewCardView.cs (allowed to change only under sections 8.1 and 9):
693c2a578b037f0d3175ea91f983bec1895a55a276b796739cfaa36eba3b97a4

ItemSystemBattleSandboxBoardAuthority.cs (allowed to change):
6912eb4faf9f14c26891a83bd0717c0fb5243c9edb8e263bcda1511450bf5cd0

ItemSystemBattleSandboxBoardAdapter.cs (allowed to change):
0dbf41b4a1e008b15ac12d2546ad8cdfe83937db3ea08ed3e3b0aa212b88c4d0

ItemSystemBattleSandboxViewProjection.cs (allowed to change):
968bc6047443989a4f695409f2ca406e97aaecc377ad804f062836d6fdf10382

ItemSystemBattleSandboxBoardAdapterVerifier.cs (allowed to change):
07a9e8a9477816b64d9ecbd533726f051d6d235d13c79e692b07090380a23c19

ItemDetailPanel.prefab (not loaded or instantiated; must not change):
2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c

ItemDetailPanelView.cs (read/reuse only; must not change):
c9680f17a92a3623388a0575001f3a1b61667fcead103c9d45fdca05188edc88

ItemBalanceCandidateDetailSandboxAdapter.cs (read/reuse only; must not change):
65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84

ItemDetailProjectionComposer.cs (read/reuse only; must not change):
5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782

ItemFullDetailBuildSandboxWorkbenchSession.cs:
51fae336d8f3c172eff39dc4a982ce01dc44d59d831d02ad65e2d8315d4f3d9e

ItemSystemSnapshot.cs:
794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827

I031InventoryPlacementContract.cs:
6ff5a159a3cfcbb810d271098870d0487556954f1f915bf781184b24db4574bd

LayoutResilienceItemFactProjectionValidation.cs:
499d8b67a45e59a163d6a3192937113cb550207a101f8754531a32346a50047d

ItemInstancePlacementBindingContract.cs:
335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0

ItemInnerDataCatalog.cs after accepted shape correction:
606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9

Accepted shape-correction canonical evidence:
Foundation: sha256:dc0ad3a1e2f686507ad0196ef06bbb4189bd72f610406a1de2f3b16342ed80d3
Snapshot: sha256:07961b49d2c0217bc454ca30088aa7a2de216209de532c5120c3315d8b62d025
Candidate: sha256:cef35782e5b01565341eb915736d3e29d6bc78e6a50e600201a3f5143929f284
Roll150: sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8
Projection150: sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2
ItemSystem v2 debug canonical: sha256:678b293b7adb4f9ec7099021538265428c0b8baf5cf14ae88f148b079a46361f
P1 v2 fixture canonical: sha256:06f84e83e10b05ca52225f2cc2b5afcc595c1cf34d86095a531b20a61f383382
P6 v2 fixture canonical: sha256:71cac03fb6f48e8c8490dfe1ceef6ad932534d734f1556adf69bc264630d164c
Historical pre-v2 Item105 aggregate values are diagnostic only and must not be used as
Revision07 gating baselines. Revision07 protects the exact accepted v2 Item files and
canonical signatures listed above; all other Item files use task-start current-disk
before/after hashing and must remain unchanged.

Prior P7 adapter implementation:
1ee5e8bfd6c311661931ba4c62ac913e8ea54bf3217ca46ac7c7ccea24b51055

ItemBalanceWorkbenchCatalog.asset:
5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45

EditorBuildSettings.asset:
08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59

HEAD:
f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

Also preserve the accepted canonical signatures for IF01 and P6 and all current Item/Enemy/CrossSystem protected aggregates. Baselines are taken from the current disk state, not from Git HEAD. Existing dirty/untracked art and Item changes must remain untouched.

## 17. Completion receipt

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemSystemBattleSandboxBoardAdapter01
Guard receipts: list all four exact receipts
Revision07 new files / modified runtime-editor-report files
Target scene / target controller
Roster count / ordinary projection count / I031 count
Tray columns / logical rows / logical slots / runtime-only slots / footprint cells (62 ordinary + 1 I031)
Authored-slot preservation / runtime-slot cleanup / Content-height restoration
Shared artwork resolution count / final visible renderer binding count / legacy-visible count
Tray / ghost / board exact Sprite-reference match count
Authored ItemDetailPanel resolve / initial hide / reuse / close / uninstall-hide result
Runtime panel or prefab instantiate count / authored panel destroy count
Authority tap detail count / legacy BuildSandboxItemInfoPanel.Show count
Exact ItemDetailViewModel identity checks for I001-I031
Detail runtime context checks: CatalogPreview / PlacedInstance / exact placement identity
Detail projection checks: lighting / relay / array / Build / awakening / skill monitor
Base ViewModel immutability / latest-snapshot refresh / stale-placement rejection
ItemSystem snapshot status / IF01 status
I031 explicit v2 Inventory/Board state transitions and stable identity checks
LitRangeCells / visible range / powered-unpowered state checks
Legal / rejected transaction counts
IF01 / P6 calls per changed snapshot
Compatibility projection direction check
Interaction regression checklist
Chinese-only feedback check
Canonical Signature
Offline verifier
Unity compile / verifier
User handtest result
Target Scene hash before / after
Protected hashes
Leak Count
git diff --check
Forbidden scope touched
HEAD unchanged
commit / tag / push: none
Next package: NOT_STARTED
```

## 18. Following packages remain held

This package does not automatically release:

```text
UnifiedBattle promotion
formal V0.3 flow bridge
formal APK/Addressables ItemDetailPanel delivery and UnifiedBattle integration
formal combat item effect producer
N02 / PA01 / C02B / C02R1 / C03
```

The next package is selected only after static QA and user handtest of this real V04 board entry.

## 19. Revision09-Rev02 authoritative full-text rendering override

This section supersedes every earlier Revision09 sentence that limits the fix to only
`TriggerConditionSection/BodyText`, that protects `ItemDetailPanelView.cs` from the
Revision09 write whitelist, or that describes Revision09 as a single-point body fix.
All unrelated Revision02-Revision08 behavior and protection requirements remain binding.

### 19.1 Confirmed failure attribution

Revision08 proved that the Orange/DaoPin model is complete. The target Scene contains
authored `Text` components and non-empty serialized strings, but the copied Item Detail
hierarchy lost its Legacy `Text.font` references. Representative header and status fields
have `font == null`, and `ItemDetailPanelView.chineseTextFont` is also null. A Legacy
`Text` with no Font cannot build a glyph mesh, even when the object is active, enabled,
opaque and has non-empty text. Missing Font also collapses preferred layout measurements.

An independent presentation defect remains in the shared section seam: an authored
plain `BodyText` may be inactive, while `ItemDetailSectionView.SetContent` writes content
without restoring the correct plain-body visibility state.

These are presentation failures. They are not Item data, rarity, I031, lighting, Enemy,
Capability, P1 or P6 failures.

### 19.2 Required complete fix

Revision09-Rev02 must repair every Legacy `UnityEngine.UI.Text` that should be visible in
the current Item Detail state, not only one named field. Coverage includes:

```text
ItemNameText
MetaText
PowerText
all connected StatusBadgeText fields
detail/debug tab labels
player/debug section TitleText and BodyText
authored base-stat, affix, core-effect, Build and narrative row labels
runtime-created descendant Text rows
every other Legacy Text descendant under the authored ItemDetailPanel
```

`ItemDetailPanelView` must apply one generic font-recovery pass over
`GetComponentsInChildren<Text>(true)` before model binding/layout measurement and one
verification/recovery pass after binding so newly created descendant rows are covered.
It must never hard-code I001-I031 or maintain a per-field repair list.

Font resolution order is frozen as:

```text
1. valid serialized chineseTextFont, when non-null
2. first valid non-null Font already authored on an ItemDetailPanel descendant Text
3. Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
```

If no valid Font can be resolved, binding must fail with one stable Chinese diagnostic;
it must not silently show an empty panel. Recovery assigns only `Text` components whose
`font` is null. Existing valid authored fonts, font sizes, styles, colors, alignments and
materials must remain unchanged. The resolved Font is runtime-only: do not serialize it,
save the Scene or change a Prefab.

`ItemDetailSectionView` must implement deterministic visibility transitions for both
`SetContent(ItemDetailSectionViewModel)` and `SetContent(string,string,bool)`:

```text
hidden -> plain body -> authored rows -> plain body -> hidden
```

Plain body content activates the authored `BodyText`; authored-row modes hide the plain
body to prevent duplicate content; returning to plain content restores it; empty content
clears stale text/rows and follows `keepWhenEmpty`. This must not activate unrelated tabs,
debug roots or all hidden descendants indiscriminately.

After binding, the panel must rebuild its existing authored layout so active data-bearing
texts have non-zero preferred measurements. It must not change serialized RectTransform,
LayoutElement, hierarchy, sibling order, anchors, pivots, offsets, sizes or scales.

### 19.3 Explicit non-goals

The sparse middle `statusBadgeImages[1] / statusBadgeTexts[1]` slot exists in the accepted
ItemSandbox source as well as the copied target hierarchy. Revision09-Rev02 must not create
or wire a new middle badge unless a later user-authored UI package explicitly requests it.

`ItemDetailSectionView.ShowSectionChrome == false` is an accepted shared presentation
choice. Revision09-Rev02 must not enable all section titles or accents. The package repairs
rendering for fields that the current presentation state says should be visible; it does
not force mutually exclusive, hidden-tab or authored-row replacement fields visible at
the same time.

### 19.4 Effective Revision09-Rev02 write whitelist

Only these existing files may change:

```text
Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs
Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/ItemSystemBattleSandboxBoardAdapterVerifier.cs
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardAdapterReport.md
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardRoster.csv
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardStateMatrix.csv
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardLeakCheckReport.md
Docs/V0.4/Reports/ItemSystemBattleSandboxBoardManualTest.md
```

No new runtime Scene object, Prefab, font asset or UI file may be created. No `.unity`,
`.prefab`, RectTransform, Item truth, ItemDetail projection/composer, BuildSandbox authority,
I031, lighting, IF01, P1, P6, Enemy, CrossSystem or formal-flow file may change.

### 19.5 Additional acceptance gates

Add stable verifier coverage for at least:

```text
S20G hidden -> plain -> authored rows -> plain -> hidden visibility transitions
S20H target authored panel resolves a usable Font without Scene mutation
S20I every active data-bearing Legacy Text descendant has font != null
S20J ItemNameText, MetaText, PowerText and connected StatusBadgeText fields render non-empty data
S20K player-section plain bodies and authored rows are mutually exclusive and restore correctly
S20L runtime-created descendant Text rows receive a valid Font
S20M active data-bearing Text preferredWidth/preferredHeight are non-zero after layout rebuild
S20N ItemSandbox source panel and BattleSandbox target panel both pass shared-view regression
```

Static QA must fail if the model contains data but any field expected visible in the
current state has a null Font, inactive renderer, disabled Text, transparent effective
color, empty rendered body, or collapsed preferred measurement. Static QA must distinguish
intentional hidden/replacement fields from defective visible fields.

The target Scene and all Prefabs must retain their exact task-start hashes. Unity compile,
Offline verifier and StaticBatch must pass before user handtest. User handtest must confirm
all currently expected Item Detail text fields are visible for representative I001-I030
Orange/DaoPin samples and I031, with no duplicate body rows and no layout rewrite.

## 20. Revision09-Rev03 authoritative Chinese-font and authored-icon visibility override

This section supersedes Revision09-Rev02 wherever Rev02 treats any non-null fallback Font
as sufficient or validates only Text visibility. The effective whitelist remains exactly
the eight files listed in section 19.4. No new Guard domain or implementation file is added.

### 20.1 Additional confirmed attribution

The accepted ItemSandbox source panel uses a Scene-embedded dynamic Font named `SimSun`.
Equivalent BattleSandbox Text fields lost that reference and serialize `font == null`;
`chineseTextFont` is also null. This explains Chinese Text failure but does not explain an
`Image` by itself.

The following authored visual groups are children of the `BodyText` transform and therefore
also disappear when the `BodyText` GameObject is inactive, regardless of their own child
active state:

```text
FaMenBuildRows / FaMenBuildStageIconSlot_0..2
QiLeiBuildRows / QiLeiBuildStageIconSlot_0..1
CoreEffectRows / CoreEffectIconSlot_0..3 as applicable
PlacementRows / PlacementIconSlot_0
FlavorRows / FlavorIconSlot_0
```

BattleSandbox and ItemSandbox retain matching authored Sprite references for Placement and
Flavor icons. FaMen, QiLei and Core Effect icons are resolved at runtime from existing
Resources paths by `ItemDetailSectionView`. The failure must not be attributed to Item data
or repaired by changing Sprite assets without direct verifier evidence.

Current state is:

```text
QA_HANDTEST_FAIL / FONT_BINDING_AND_AUTHORED_ICON_VISIBILITY_REGRESSION
Revision09-Rev02: RETURNED / NOT_RELEASED_FOR_DEV
Revision09-Rev03: WAITING_ITEM_GUARD_REREVIEW / NOT_READY_FOR_DEV
```

### 20.2 Font source policy

The runtime must preserve the accepted Item Detail Chinese typography when possible rather
than silently treating `LegacyRuntime.ttf` as the preferred source.

Resolve one cached Font per panel using this exact order:

```text
1. valid serialized chineseTextFont
2. first valid Font already authored on a descendant Text
3. installed OS Font named SimSun, created once through Font.CreateDynamicFontFromOSFont
4. installed Chinese fallback, in order: Microsoft YaHei, Noto Sans CJK SC, Droid Sans Fallback
5. Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") as last-resort non-null fallback
```

The implementation must query installed font names with OrdinalIgnoreCase matching before
creating an OS Font. It must not create a new Font per Bind, leak runtime Font objects,
serialize a dynamic Font, add a font asset, or save a Scene/Prefab. Existing valid authored
fonts remain untouched. On a host where SimSun is installed and all authored target fonts
are null, static verification must prove that the resolved runtime font name is SimSun.
On a host without SimSun, the selected fallback and reason must be reported explicitly;
silent typography drift is forbidden.

### 20.3 BodyText container and authored-row rule

In authored-row mode the `BodyText` GameObject is the required container and must stay
active. Hide only the plain `Text` graphic, never the GameObject:

```text
plain mode:    bodyText.gameObject active, bodyText.enabled true, authored rows hidden
authored mode: bodyText.gameObject active, bodyText.enabled false, selected authored rows active
hidden section: section root inactive; stale rows and plain text cleared
```

No implementation may call `bodyText.gameObject.SetActive(false)` while any authored row
under that transform is expected visible. Returning from authored mode to plain mode must
restore `bodyText.enabled = true` and hide every authored replacement group.

### 20.4 Authored icon resolution and actual visibility gates

For every data-bearing representative I001-I030 Orange/DaoPin model, and for each applicable
row, static QA must validate after Bind and `Canvas.ForceUpdateCanvases()`:

```text
Image reference exists
Image.sprite != null
Image.enabled == true
Image.gameObject.activeInHierarchy == true
effective Image color alpha > 0
CanvasRenderer is not culled after layout update
RectTransform world rect intersects the active detail viewport world rect
resolved Sprite belongs to the expected stable Resources path or accepted authored GUID
```

Required groups:

```text
FaMen Build: 3 stage icons when the bound model contains 2/4/6-stage rows
QiLei Build: 2 stage icons when the bound model contains 2/4-stage rows
Core Effect: each displayed row, up to 4, matched to exact baseItemId and slot ordinal
Recommended Placement: PlacementIconSlot_0 with accepted authored placement Sprite
Legacy Chronicle: FlavorIconSlot_0 with accepted authored legacy-record Sprite
```

The verifier must separately report `missing sprite`, `disabled image`, `inactive ancestor`,
`transparent image`, `culled by canvas`, `outside viewport`, and `wrong resource identity`.
It must not report all icon failures as a generic font failure.

If a required Resources Sprite cannot resolve, the package may repair only the existing
generic resolver inside `ItemDetailSectionView.cs`; it may not rename, replace, import or
reassign art assets, edit Scene/Prefab references, substitute another item's art, or create
a per-item exception table. If accepted authored Placement/Flavor Sprite references are
null or changed on disk, stop and return to Guard rather than editing the Scene.

### 20.5 Revision09-Rev03 acceptance additions

Add stable cases after S20N:

```text
S20O source ItemSandbox typography identity is SimSun and target null-font evidence is recorded
S20P target resolves SimSun when installed, otherwise reports the exact approved fallback
S20Q authored mode keeps BodyText container active while disabling only the plain Text graphic
S20R FaMen Build applicable icons pass sprite/enabled/active/alpha/cull/viewport/path checks
S20S QiLei Build applicable icons pass the same checks
S20T Core Effect applicable icons pass exact baseItemId/ordinal and visibility checks
S20U Placement and Flavor authored icons retain accepted GUIDs and are actually visible
S20V plain/authored/plain round trip preserves fonts, sprites and visibility without duplicates
```

Static QA cannot pass from serialized references alone. It must bind representative runtime
models, update the Canvas and prove the final rendered Text/Image state. Any user-observed
missing field or icon overrides a weaker static result and returns the package to Guard.

## 21. Revision09-Rev03 user-preserved Scene baseline and release gate

The user has explicitly required preservation of all previously completed Item Detail
content, including the Chinese typography, authored fields, icons, layout and runtime
state presentation, and has saved the current BattleSandbox Scene. That saved Scene is
the authoritative task-start baseline for Revision09-Rev03. This section supersedes all
older target Scene hashes for the effective Revision09-Rev03 package while retaining the
older values as historical evidence only.

```text
Target Scene:
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity

Revision09-Rev03 protected SHA-256:
4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6

Historical Revision07/Revision08 SHA-256, not an active baseline:
a7dc7e8566b8522a43dc9f5ba5c9cccf8668e25096ececbf2b028aa4e7b86d05
```

Read-only structural admission check against the saved Scene passed:

```text
PopupLayer: exactly 1
PopupLayer/ItemDetailPanel: exactly 1
ItemDetailPanelView component: exactly 1
ItemDetailPanel parent: PopupLayer
ItemNameText: present
FaMenBuildRowsRoot: present
PlacementIconSlot_0: present
FlavorIconSlot_0: present
```

Development must preserve the exact Revision09-Rev03 Scene hash from task start to task
finish. The package must not save, rewrite, normalize or reserialize the Scene. The user
decision to preserve the current Scene does not authorize Scene, Prefab, RectTransform,
font-asset or Sprite-asset modification.

The full Item Detail presentation is one shared Item-owned presentation surface. The
BattleSandbox integration may bind it but must not maintain a second per-scene copy of
font, field, icon or visibility rules in code. Revision09-Rev03 is the final V0.4
presentation repair before one consolidated user handtest; no per-field follow-up package
may be auto-created.

Release remains gated only by an Item Guard receipt issued against the final exact
Revision09-Rev03 Assignment SHA-256:

```text
ITEM_GUARD_CONFIRM_ITEMSYSTEMBATTLESANDBOARDADAPTER01
```

Enemy Guard rereview and Capability Algorithm Guard rereview remain `NOT_REQUIRED`.
Development, Unity QA and user handtest remain blocked until that receipt is obtained.
