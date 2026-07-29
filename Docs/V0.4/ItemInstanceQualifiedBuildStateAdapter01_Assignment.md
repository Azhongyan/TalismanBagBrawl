# V0.4-ItemInstanceQualifiedBuildStateAdapter01 Assignment

Status: `READY_FOR_DEV / ITEM_GUARD_APPROVED / CROSS_SYSTEM_GUARD_APPROVED`

Package: `V0.4-ItemInstanceQualifiedBuildStateAdapter01`

Assignment freeze date: `2026-07-22 Asia/Shanghai`

## 0. Required receipts

Accepted receipts:

```text
ITEM_GUARD_PASS_ITEMINSTANCEQUALIFIEDBUILDSTATEADAPTER01
CROSS_SYSTEM_GUARD_CONFIRM_ITEMINSTANCEQUALIFIEDBUILDSTATEADAPTER01
GUARD_PASS_ITEMINSTANCEQUALIFIEDBUILDSTATEADAPTER01
```

Target development handoff:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS
Package: V0.4-ItemInstanceQualifiedBuildStateAdapter01
UserHandtest: NOT_APPLICABLE_FOR_PACKAGE_A
PackageB: NOT_STARTED
```

This Assignment authorizes Package A only. It does not authorize
`V0.4-ItemDetailQualifiedBuildTrackProjection01` or Item Detail prefab migration.

## 1. Why this package exists

The accepted field-lineage audit found that BattleSandbox owns valid generated Item
instances and valid placement identities, but its `ItemSystemSnapshot.v2` Build result does
not consume each instance's `buildQualification`. ItemSandbox currently repairs this through
the sandbox-only `ItemFullDetailQualifiedBuildAdapter`; BattleSandbox does not.

Package A establishes one immutable read-only derived state that joins:

```text
ItemInstanceProjectionContractSnapshot
  itemInstanceId / baseItemId / rarity / buildQualification

ItemInstancePlacementBindingContractSnapshot.v1 (IF01)
  itemInstanceId / placementId / baseItemId

ItemSystemSnapshot.v2
  placement / rotation / isLit / isCountedInBuild / Build track state
```

The derived state is not a second Item state owner. It is rebuilt only from accepted
authoritative inputs. It executes no Item effect and drives no UI in Package A.

## 2. Required contract

Add the independent schema:

```text
ItemInstanceQualifiedBuildStateSnapshot.v1
```

The runtime seam must expose a stateless provider/assembler, for example:

```text
IItemInstanceQualifiedBuildStateAssembler.Assemble(input)
```

The input must explicitly carry:

```text
ItemInstanceProjectionSetSnapshot projectionSet
ItemInstancePlacementBindingContractSnapshot bindingSnapshot
ItemSystemSnapshot itemSystemSnapshot
QualifiedBuildRosterCompleteness rosterCompleteness
```

`rosterCompleteness=CompleteOwnedRoster` means that the ProjectionSet is the complete
ordinary I001-I030 owned roster for this dev session. Only in that state may a valid
projection absent from IF01 be classified as `Inventory`. A missing, partial or unknown
roster must produce `Unknown`, never Inventory by inference.

## 3. Required output facts

The immutable output must contain:

```text
schemaId / status / rosterCompleteness
sourceProjectionSetIdentity
sourceBindingCanonicalSignature
sourceItemSystemCanonicalSignature
Items[] / FaMenTracks[] / QiLeiTracks[]
validationErrors[] / canonicalSignature
```

Each ordinary Item row must contain at minimum:

```text
itemInstanceId / baseItemId / rarity / buildQualification
location: Inventory / Board / Unknown
placementId
placementFactCompleteness
isLitFact
sourceIsCountedFact
qualifiedIsCountedFact
contributesToFaMen
contributesToQiLei
eligibleFaMenBuildId
eligibleQiLeiBuildId
faMenBuildCount / qiLeiBuildCount
faMenActiveStagePieceCount / qiLeiActiveStagePieceCount
```

`sourceIsCountedFact` is the exact `ItemSystemSnapshot.v2` layout fact.
`qualifiedIsCountedFact` is the final contribution after applying the generated instance's
Build qualification. They must remain separate fields and may not overwrite one another.

## 4. Status and missing-value semantics

Use explicit `Valid / Unknown / Invalid` status and explicit fact completeness. Do not use
empty strings, false or zero as a replacement for missing state.

```text
Inventory:
  qualification is known from the complete ProjectionSet.
  location is known Inventory.
  placementId is absent by definition.
  isLit/sourceIsCounted/qualifiedIsCounted are NotApplicable, not false.

Board:
  Projection + IF01 + ItemSystem placement identities must match exactly.
  isLit and sourceIsCounted come from ItemSystemSnapshot.v2.
  qualified contribution is derived by the shared assembler.

Unknown:
  null source, incomplete roster, missing source completeness, or unavailable authority.

Invalid:
  duplicate identity, orphan binding, baseItemId mismatch, placement mismatch,
  duplicate placement, illegal qualification, I031 ordinary projection, or contradictory facts.
```

Known Zero is legal only when every required authority is complete and the complete qualified
source set explicitly contributes zero items to the track.

## 5. Build qualification rules

```text
None       -> no FaMen track and no QiLei track
FaMenOnly  -> FaMen track only
QiLeiOnly  -> QiLei track only
Dual       -> both tracks
```

Qualification never changes when the same instance moves Inventory -> Board -> Inventory.
An unlit Board instance retains eligibility but contributes to no track.

Build 2/4/6 activation must reuse the existing `ItemBuildSynergyRules` /
`ItemBuildTrackResult` behavior. The new assembler must not copy or rewrite threshold logic.

I031 remains a special system Item. It has no ordinary Projection, itemInstanceId,
BuildQualification, qualified Item row, FaMen contribution or QiLei contribution.

## 6. Runtime ownership and atomic commit

`ItemSystemBattleSandboxBoardAuthority` remains the state owner. It may expose:

```text
CurrentQualifiedBuildState
```

as a read-only state derived from the same accepted transaction. On a changed commit:

```text
1. Build candidate ItemSystemSnapshot.v2.
2. Validate IF01 against exact projections and placements.
3. Assemble and validate the qualified Build state.
4. Run the already-authorized P6 evaluation without changing its inputs or semantics.
5. Only when all results pass, atomically replace:
   CurrentSnapshot
   CurrentBindingSnapshot
   CurrentQualifiedBuildState
   CurrentLayoutResilienceSnapshot
```

Any failure leaves all four previous values untouched. A no-op must not rebuild or replace
the qualified state, rerun IF01/P6, increment call counts, or emit duplicate notification.

## 7. ViewProjection boundary

`ItemSystemBattleSandboxViewProjection` may only expose/build the complete ordered I001-I030
ordinary ProjectionSet already represented by its accepted rows.

It must not modify:

```text
text / Sprite / ItemDetailViewModel / display sections / rarity / rootSeed
itemInstanceId / buildQualification / Roll result / UI rules
```

The set must contain exactly 30 unique ordinary instances sorted Ordinal by itemInstanceId.
I031 must not be inserted into the ordinary ProjectionSet.

## 8. Workbench deduplication

`ItemFullDetailQualifiedBuildAdapter` must cease to contain an independent qualification
filter implementation. Workbench must consume the shared Package A assembler or a shared
pure helper owned by Package A. Only sandbox presentation adaptation may remain in the
Workbench session.

## 9. Exact file whitelist

Expected new files: exactly `12`.

```text
Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified.meta
Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs
Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs
Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs
Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemInstanceQualifiedBuildStateAdapterVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemInstanceQualifiedBuildStateAdapterVerifier.cs.meta
Docs/V0.4/Reports/ItemInstanceQualifiedBuildStateAdapterReport.md
Docs/V0.4/Reports/ItemInstanceQualifiedBuildStateAdapterSpec.csv
Docs/V0.4/Reports/ItemInstanceQualifiedBuildStateAdapterLeakCheckReport.md
```

Allowed existing-file modifications: exactly these three files.

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs
Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemFullDetailBuildSandboxWorkbenchSession.cs
```

The first two files are existing uncommitted workspace files. The development window must
use their task-start disk contents and hashes, not Git HEAD, and must not clean, recreate or
rollback them.

## 10. Real sample matrix

All samples use the accepted BattleSandbox deterministic Orange roster.

| Sample | Qualification | Inventory | Board unlit | Board lit |
|---|---|---|---|---|
| `I001 / wb_i001_orange_404310001` | `QiLeiOnly` | only `qilei:fu` eligible | qualified count false | `qilei:fu=1/4`; no FaMen contribution |
| `I004 / wb_i004_orange_404310004` | `Dual` | `famen:zhenlei + qilei:ling` | both contributions false | both tracks receive one source |
| `I006 / wb_i006_orange_404310006` | `None` | no tracks | no tracks | no tracks; qualified count remains false |
| `I009 / wb_i009_orange_404310009` | `FaMenOnly` | only `famen:lihuo` eligible | qualified count false | `famen:lihuo=1/6`; no QiLei contribution |
| `I031` | `NotApplicable` | no ordinary row | no ordinary row | no ordinary row |

Each ordinary sample must test Inventory, Board-unlit, Board-lit, return-to-Inventory and one
identity mismatch. Counts in multi-item scenarios must use exact qualified source IDs and
must remain deterministic under reversed input order.

## 11. Required invalid and unknown cases

At minimum:

```text
null ProjectionSet -> Unknown
roster completeness Unknown -> Unknown, not Inventory
29/30 or 31/30 ordinary roster -> Unknown/Invalid as applicable
duplicate itemInstanceId -> Invalid
duplicate baseItemId in this fixed roster -> Invalid
IF01 orphan itemInstanceId -> Invalid
IF01 orphan placementId -> Invalid
Projection/IF01/ItemSystem baseItemId mismatch -> Invalid
Board placement without IF01 binding -> Invalid
IF01 binding without Board placement -> Invalid
I031 ordinary Projection/binding -> Invalid
Unresolved or illegal BuildQualification -> Invalid
external collection mutation attempt -> blocked
same complete inputs in any order -> same canonical signature
```

## 12. Canonical signature

The new signature is independent. It must use SHA-256, Ordinal ordering, InvariantCulture,
explicit field names/lengths and immutable read-only collections.

Ordering:

```text
Items: itemInstanceId Ordinal
FaMenTracks/QiLeiTracks: buildId Ordinal
source IDs: Ordinal distinct
validationErrors: code + identities + message, all Ordinal
```

Do not change the signature algorithm or canonical payload of Roll, Projection,
ProjectionSet, IF01, ItemSystemSnapshot.v2, ItemBuildSynergyRules or P6.

## 13. Protected task-start baselines

The following hashes were captured from the real disk, including current uncommitted files:

```text
BoardAuthority.cs = 3b77408373423ae0355981395e21b99e4a2077fabad69fc5f885327045ef659e
ViewProjection.cs = 180391581f3999a9d683dc34b28738b9fe15c580127bee3a91f4ca6851c69895
WorkbenchSession.cs = 51fae336d8f3c172eff39dc4a982ce01dc44d59d831d02ad65e2d8315d4f3d9e
ItemInstanceRollEngine.cs = 19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e
ItemInstanceProjectionContract.cs = f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967
ItemCorePotentialAndBuildEligibilitySchema.cs = 0a3cc80b1dc047dfe6ba6caec83e01a0be69aed78f7697e7666b401ecfac2b99
ItemInstancePlacementBindingContract.cs = 335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0
ItemSystemSnapshot.cs = 794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827
ItemBuildSynergyRules.cs = 64b9d3e2e407dc014695b4f466c4c82803384b9b37dc919c435bd181e2c13910
ItemBalanceWorkbenchCatalog.asset = 5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45
Profiles30 aggregate = a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0
ItemDetailProjectionComposer.cs = 5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782
ItemDetailPanelView.cs = 89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d
ItemDetailSectionView.cs = 957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d
ItemSandbox scene = 8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29
BattleSandbox scene = 4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6
ItemDetailPanel.prefab = 2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c
EditorBuildSettings.asset = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
```

`Profiles30 aggregate` is SHA-256 over 30 Ordinal filename rows in the form
`relativePath|lowercaseFileSha256\n`.

The first three whitelisted files may change only within this Assignment. Every other hash
above must remain byte-identical. The development window must record its own immediate
pre-edit hashes and stop if they differ from this frozen baseline.

## 14. Verification

Offline verification must exercise only pure contract/assembler code and must not open or
save Unity assets. Unity batch entry:

```text
TalismanBag.EditorTools.ItemSandbox.ItemInstanceQualifiedBuildStateAdapterVerifier.VerifyStaticBatch
```

Required results:

```text
COMPONENT_FIXTURE_PASS
REAL_RUNTIME_STATE_ASSEMBLY_PASS
USER_HANDTEST_NOT_APPLICABLE
I001/I004/I006/I009 real samples PASS
Inventory/Board/unlit/lit/return transitions PASS
Unknown/Invalid semantics PASS
sourceIsCountedFact and qualifiedIsCountedFact separation PASS
atomic commit and rollback-on-failure PASS
no-op identity/call-count stability PASS
external mutation blocked PASS
Ordinal determinism/canonical signature PASS
existing Workbench Build2/4/6 regression PASS
Roll/Projection/IF01/ItemSystem/P6 protected signatures PASS
LeakCheck PASS
git diff --check PASS
```

No Unity user handtest is required for Package A. The report must explicitly state that the
Item Detail panel will not display the new state until Package B is separately assigned.

## 15. Hard prohibitions

Do not modify or connect:

```text
ItemDetailProjectionComposer / ItemDetailViewModel / ItemDetailPanelView / ItemDetailSectionView
Scene / Prefab / Font / Sprite / RectTransform / BuildSettings
Roll algorithm / rarity probabilities / BuildQualification values
ItemSystemSnapshot.v2 / IF01 / ItemBuildSynergyRules semantics or signatures
Enemy / Capability / P1 / P6 semantics
Battle effects / RunFlow / SaveData / Reward / Chapter / Boss
```

Do not create default Build tracks, infer Inventory from an incomplete roster, treat Unknown
as false/zero, retain a second qualification filter in Workbench, run a Builder, start
Package B, start prefab migration, or commit/tag/push/reset/rollback.

