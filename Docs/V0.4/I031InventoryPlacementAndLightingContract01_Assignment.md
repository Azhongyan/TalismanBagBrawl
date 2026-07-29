# V0.4-I031InventoryPlacementAndLightingContract01 Assignment

Status: `GUARD_PASS_ASSIGNMENT_I031INVENTORYPLACEMENTANDLIGHTINGCONTRACT01 / WAITING_JOINT_GUARD_REVIEW / NOT_READY_FOR_DEV`

Package: `V0.4-I031InventoryPlacementAndLightingContract01`

Target receipt: `GUARD_PASS_I031INVENTORYPLACEMENTANDLIGHTINGCONTRACT01`

Required joint receipts:

```text
ITEM_GUARD_CONFIRM_I031INVENTORYPLACEMENTANDLIGHTINGCONTRACT01
CAPABILITY_ALGORITHM_GUARD_PASS_I031INVENTORYPLACEMENTANDLIGHTINGCONTRACT01
ENEMY_GUARD_CONFIRM_I031INVENTORYPLACEMENTANDLIGHTINGCONTRACT01
GUARD_PASS_I031INVENTORYPLACEMENTANDLIGHTINGCONTRACT01
```

No development may start until all four receipts target this exact Assignment SHA-256.

## 1. Locked product decision

`I031` is one unique owned special progression item. It is not an ordinary generated
instance. It may be in inventory or placed on the board.

```text
initial/reset location = inventory
board may contain zero I031 placements
board may contain exactly one I031 placement
I031 inventory -> board -> move -> inventory is legal
I031 duplicate ownership or duplicate placement is Invalid
eyeCell remains non-coverable
```

The Item contract owns identity, ownership, placement state and lighting facts. The
BattleSandbox only consumes this contract in a later Revision05 package.

## 2. Authority split

### Item authority

```text
I031 ownership and inventory/board location
stable special identity
stable reserved placementId
shape/cells/coreCell
placement legality
lighting-source eligibility
LitRangeCells and per-placement lighting state
Build exclusion
ItemSystem canonical signature
```

### IF01 authority

IF01 continues to bind only ordinary generated item instances to ordinary placements.
It must never create or accept an ordinary `itemInstanceId` for I031.

### Capability/Cross-System authority

P1/P6 may consume the valid Item layout and IF01 result. They do not infer inventory
ownership, create I031 identity, score occupied cells as BP, or alter Item lighting.

### Enemy authority

No Enemy data, pressure input, applicability, requirement or readiness changes are
allowed. The center eye rule remains unchanged.

## 3. ItemSystem schema migration

The existing contract must migrate explicitly:

```text
old = ItemSystemSnapshot.v1
new = ItemSystemSnapshot.v2
```

Do not silently add the new semantics under the v1 schema key.

Add an immutable I031 state branch with equivalent public semantics:

```text
itemId = I031
specialIdentityId = SPECIAL_I031
stablePlacementId = P_SYSTEM_I031
ownershipCompleteness = Unknown | Complete
location = Unknown | Inventory | Board
isOwned
isPlaced
```

Names may vary only if the verifier field matrix records the exact final names. The
meaning and identity constants may not vary.

The state branch must be present in `BuildDebugSignature()` using Ordinal text ordering,
InvariantCulture values and deterministic enum serialization.

### Input compatibility

`ItemSystemSnapshotInput` must expose an explicit I031 state input for new callers.
Missing or incomplete explicit input is Unknown/Invalid and never Known Zero.

A narrowly named legacy compatibility factory may derive `Board` only from one explicit
I031 placement, or `Inventory` from the already locked V0.4 core-grant fixture. Revision05
must not use that compatibility path; it must submit the explicit state input.

## 4. State and identity rules

### Inventory state

```text
ownership = Complete
location = Inventory
I031 placement count = 0
LitRangeCells = empty
```

This is a Valid/Complete ItemSystem snapshot, including when all placements are empty.

### Board state

```text
ownership = Complete
location = Board
I031 placement count = 1
placement.itemId = I031
placement.placementId = P_SYSTEM_I031
```

I031 placement uses catalog truth only:

```text
shapeId = shape_single_1
cells = (0,0)
coreCellLocal = (0,0)
```

### Invalid/Unknown

At minimum reject or preserve Unknown for:

```text
ownership fact missing or incomplete
owned=false for the locked V0.4 roster
duplicate I031 ownership rows
duplicate I031 placements
Inventory state plus I031 placement
Board state without I031 placement
wrong specialIdentityId
wrong stablePlacementId
I031 placementId other than P_SYSTEM_I031
ordinary itemInstanceId or IF01 binding for I031
```

Remove `JUNIAN_MISSING` as a placement-count requirement. Replace it with explicit
ownership/state validation codes. `JUNIAN_MULTIPLE` may be retained as an alias only if
the new primary code is deterministic and documented.

## 5. Stable transition semantics

The same I031 object always retains:

```text
specialIdentityId = SPECIAL_I031
stablePlacementId = P_SYSTEM_I031
```

Returning to inventory removes the placement row but does not delete ownership or change
either identity. Replacing it on the board restores the same placementId. No counter,
random value, card index, display name or hierarchy name may generate this identity.

## 6. Lighting contract

Reuse `ItemLightingResolver` and `OrthogonalAdjacentLightingRangeRule` as the only
lighting algorithm. Do not copy the algorithm.

The protected rule remains:

```text
offsets = (0,1), (1,0), (0,-1), (-1,0)
board = 5x5
out-of-board cells are clipped
source cell is not a target LitRangeCell
```

When I031 is in inventory, no source item is passed to the resolver:

```text
LitRangeCells = empty
ordinary isLightingSource = false
ordinary isDirectLit = false
ordinary isLit = false
ordinary litByItemId = empty
ordinary litByPlacementId = empty
ordinary litDepth = -1
ordinary isCountedInBuild = false
```

When I031 is legally placed, it is the unique source. Its placement is lit at depth 0,
and ordinary items are resolved by the existing direct and adjacent-relay rules.

Every accepted placement-state change creates one fresh ItemSystem snapshot. No cached
old LitRangeCells or old relay identity may survive move or return.

## 7. IF01 contract

No IF01 schema or Canonical Signature change is allowed.

For an inventory-only I031 layout with no ordinary placements:

```text
ordinary projection subset = empty
explicit ordinary bindings = empty
IF01 status = Valid
IF01 binding count = 0
```

Do not pass all tray candidates to IF01. Only ordinary board placements and their exact
ordinary projections enter IF01. I031 never enters either set.

## 8. P1/P6 compatibility

P1 must distinguish states without inventing facts:

```text
owned Inventory I031 + empty board = Complete BuildFacts with zero placement rows
owned Board I031 = Complete BuildFacts with one I031 placement row and no IF01 row
missing/contradictory ownership = Unknown or Invalid, no complete payload
```

P1 may continue projecting only placed layout rows. The Item v2 source signature carries
the ownership state. P6 receives P1 once and must preserve Unknown/Invalid.

No predicate, pressure, threshold, BP conversion, applicability, requirement or readiness
logic may change.

## 9. Old BattleSandbox isolation

This package must not modify BattleSandbox runtime or its Scene. Revision04 remains held.

The old authority's forced reset, `I031_TRAY_COMMIT_FORBIDDEN` and
`I031_RETURN_FORBIDDEN` are not accepted behavior and cannot be used as package fixtures.
They are removed only by the later Revision05 assignment.

Until Revision05 passes, the old BattleSandbox package cannot receive final acceptance,
even if it compiles. The Item contract must not make `(0,0)` a mandatory reset location.

## 10. Allowed files

Allowed new files:

```text
Assets/_Game/Scripts/TalismanBag/Items/I031InventoryPlacementContract.cs
Assets/_Game/Scripts/TalismanBag/Items/I031InventoryPlacementContract.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/I031InventoryPlacementAndLightingContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/I031InventoryPlacementAndLightingContractVerifier.cs.meta
Docs/V0.4/Reports/I031InventoryPlacementAndLightingContractReport.md
Docs/V0.4/Reports/I031InventoryPlacementAndLightingContractSpec.csv
Docs/V0.4/Reports/I031InventoryPlacementAndLightingCanonicalMigration.csv
Docs/V0.4/Reports/I031InventoryPlacementAndLightingLeakCheckReport.md
```

Allowed targeted existing-file modifications:

```text
Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSystemValidatorAndSnapshotVerifier.cs
Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemGeneration/ItemRarityInstanceFoundationVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemGeneration/ItemInstanceProjectionContractVerifier.cs
```

Restrictions on targeted modifications:

```text
IF01 validator: diagnostic schema wording and empty ordinary-set regression only
P1 production: Item v2 source acceptance/wording only; no projection algorithm change
P1/P6 verifiers: v2 fixtures and intentional canonical migration only
generation verifiers: replace obsolete v1 compatibility assertion only
```

If any additional runtime caller must change, stop and return exact compile evidence to
Guard. Do not expand the whitelist autonomously.

## 11. Strict forbidden scope

Do not modify:

```text
Assets/_Game/Scenes/**
Assets/_Game/Prefabs/**
ProjectSettings/**
Packages/**
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemDetail*
Assets/_Game/Scripts/TalismanBag/Items/Detail/**
Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/**
Assets/_Game/Scripts/TalismanBag/Items/Generation/** runtime files
Assets/_Game/Configs/ItemBalanceWorkbench/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/**
RunFlow / SaveData / Reward / Chapter / Boss
```

Do not modify I001-I030 shape, stat, affix, core, rarity, drop or Build qualification.
Do not run Scene builders, ItemDetail builders or any verifier that writes Scene/Prefab.
Do not commit, tag, push, reset, clean, rollback or reorganize the dirty worktree.

## 12. Canonical migration

Expected to change, with old and new values recorded:

```text
ItemSystemSnapshot canonical/debug signature
ItemSystemValidator v2 golden
P1 canonical generated from Item v2 fixtures
P6 canonical generated from Item v2 fixtures
derived fixture rows that embed those signatures
```

Must remain unchanged:

```text
IF01 schemaId and canonical algorithm
Item Instance Projection canonical
Item rarity/roll/affix/core/build candidate canonicals
Item catalog and accepted shape-correction facts
Lighting offsets and resolver ordering
N01C predicate algorithm and schema
Enemy/pressure/applicability/requirement/readiness canonicals
```

The migration CSV must contain `contract`, `oldVersion`, `newVersion`, `oldSignature`,
`newSignature`, `reason`, and `status`. Never overwrite old values without a migration row.

## 13. Required verifier cases

At minimum:

```text
S01 explicit owned Inventory I031 + empty placements = Valid/Complete
S02 owned Inventory I031 + ordinary placements = Valid; no lighting source; all ordinary unlit/uncounted
S03 owned Board I031 with P_SYSTEM_I031 = Valid/Complete
S04 inventory -> board -> move -> inventory -> board preserves both stable identities
S05 duplicate ownership Invalid
S06 duplicate I031 placement Invalid
S07 ownership missing Unknown/Invalid, not zero
S08 Inventory plus I031 placement Invalid
S09 Board without I031 placement Invalid
S10 wrong specialIdentityId or stablePlacementId Invalid
S11 I031 eye-cell, overlap and out-of-bounds placement rejected
S12 center placement excludes source cell and yields four orthogonal targets
S13 corner placement yields two targets; edge non-corner yields three
S14 direct neighbor lit; range outsider unlit
S15 move removes old range and creates new range
S16 return clears LitRangeCells and all source/direct/relay state
S17 I031 never counted in FaMen/QiLei Build
S18 I031 never enters ordinary Projection, rarity, affix, roll, drop or IF01
S19 empty ordinary projection/binding set is valid IF01
S20 P1 Complete empty BuildFacts for owned inventory state
S21 P1 Complete I031 placement row without IF01 binding for board state
S22 missing/contradictory ownership remains Unknown/Invalid through P1/P6
S23 deterministic repeat/reversed-input canonical
S24 immutable collection mutation probes
S25 protected hashes and leak scan
```

## 14. Verifier entry points

```text
TalismanBag.EditorTools.ItemSandbox.I031InventoryPlacementAndLightingContractVerifier.VerifyOffline
TalismanBag.EditorTools.ItemSandbox.I031InventoryPlacementAndLightingContractVerifier.VerifyStaticBatch
```

Unity compile and the package verifier must PASS. `git diff --check` must PASS.

## 15. Protected baselines

The following current-disk files are protected and must remain byte-identical:

```text
ItemLightingRules.cs
sha256:0f837d048d30a34f6d7c23ff03b74fac4bb498bd7523e7eff51b1bf319c9bb8f

ItemInstancePlacementBindingContract.cs
sha256:335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0

ItemInnerDataCatalog.cs
sha256:606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9

ItemSystemBattleSandboxBoardAuthority.cs
sha256:6912eb4faf9f14c26891a83bd0717c0fb5243c9edb8e263bcda1511450bf5cd0

ItemSystemBattleSandboxBoardAdapter.cs
sha256:0dbf41b4a1e008b15ac12d2546ad8cdfe83937db3ea08ed3e3b0aa212b88c4d0

Scene_TalismanBag_V04_BattleSandboxPreview.unity
sha256:867fbebf42223908c5473757ebf6a54542f4646f7b8bcaf1c4bdb017e0937894

EditorBuildSettings.asset
sha256:08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
```

Baseline allowed to migrate only through the whitelisted Item schema file:

```text
ItemSystemSnapshot.cs
before sha256:821477ff5ae05e531953ef16b0e08fec84f74fbca63067c6c126be7cf7ef13df
```

Existing dirty/untracked artwork and Item Detail work must remain untouched.

## 16. Completion receipt

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-I031InventoryPlacementAndLightingContract01
Assignment SHA-256
Guard receipts
Item schema old/new
I031 identity/location cases
Lighting cases and range cells
IF01 empty-set and I031 exclusion
P1/P6 state propagation
Canonical migration rows
Offline verifier
Unity compile/verifier
Protected hashes
Leak Count
git diff --check
Forbidden scope touched
commit/tag/push: none
Next package: NOT_STARTED
```

Passing this package does not start Revision05. Cross-System Guard must issue a separate
Revision05 Assignment after all four receipts and QA evidence are accepted.
