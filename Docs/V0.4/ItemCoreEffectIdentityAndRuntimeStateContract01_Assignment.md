# V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01 Assignment

Status: `READY_FOR_DEV / ITEM_GUARD_APPROVED / USER_MAPPING_DECISION_C_APPROVED`

Package: `V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01`

Assignment freeze date: `2026-07-23 Asia/Shanghai`

Primary Guard: `Item Guard`

Joint Guard: `NOT_REQUIRED`

## 0. Required receipt and stop point

Accepted receipt:

```text
ITEM_GUARD_PASS_ITEMCOREEFFECTIDENTITYANDRUNTIMESTATECONTRACT01
```

Required development completion state:

```text
DEV_COMPLETE
QA_PASS
COMPONENT_FIXTURE_PASS
REAL_RUNTIME_STATE_ASSEMBLY_PASS
USER_HANDTEST_NOT_APPLICABLE
NEXT_PACKAGE_NOT_STARTED
```

This Assignment authorizes only the identity and runtime-derived-state contract. It does not
authorize `V0.4-ItemDetailQualifiedCoreEffectProjection01`, Item Detail prefab migration,
UnifiedBattlePage, formal cultivation, SaveData, rewards or formal Battle integration.

## 1. Accepted prerequisites and reason

Accepted prerequisites:

```text
V0.4-ItemCoreAwakeningCore4NodeExpansion01
  DEV_COMPLETE / QA_PASS / CORE4_NODE_EXPANSION_PASS
  I001-I030 = 150/150 five-node Awakening definitions
  I031 = zero ordinary Awakening definitions

V0.4-ItemInstanceQualifiedBuildStateAdapter01
  GUARD_ACCEPTED / QA_PASS
  complete ordinary instance roster + IF01 + ItemSystemSnapshot.v2 atomic state path
```

The field-lineage audit proved that Candidate core-effect definitions and generated instance
eligibility/visibility exist, but BattleSandbox has no instance-qualified state joining those
facts to Awakening nodes, explicit cultivation levels and current placement/lighting state.
Inventory therefore loses static core-effect content and Board state silently falls back to
Lv.1. This package establishes that missing read-only state. It does not render UI.

## 2. Frozen identity decision

The user approved Decision C. For every `I001-I030`, the following five mappings are frozen:

```text
candidate_core_ixxx_01       -> Ixxx_CORE_01  / Core1    / Lv10 / white
candidate_core_ixxx_02       -> Ixxx_CORE_02  / Core2    / Lv20 / green
candidate_core_ixxx_03       -> Ixxx_CORE_03  / Core3    / Lv30 / blue
candidate_core_ixxx_04       -> Ixxx_CORE_04  / Core4    / Lv40 / purple
candidate_core_ixxx_ultimate -> Ixxx_CORE_ULT / Ultimate / Lv40 / orange
```

The implementation must build explicit rows by exact `baseItemId + machine nodeKind` from:

```text
ItemBalanceWorkbenchCatalog.profiles[].coreCandidates
DefaultItemCoreEffectDefinitionProvider.GetDefinitions(baseItemId)
```

It must not map by array index, enum numeric order, display order, ID suffix parsing, Chinese
name similarity, PNG name or rarity list position. The stable node order is explicitly:

```text
Core1, Core2, Core3, Core4, Ultimate
```

Candidate remains authoritative for candidate ID, player display name, description, payload,
nodeKind, unlock level and required rarity. Awakening remains authoritative for Awakening ID,
nodeKind, unlock threshold and unlock/active rules. Any disagreement is `Invalid`; neither
side may be silently rewritten.

The accepted complete identity catalog contains exactly 150 unique rows: five rows for each
`I001-I030`. I031 has zero rows. Missing, duplicate, one-to-many, many-to-one or cross-item
mapping is invalid.

## 3. Required independent schemas

Add the immutable contracts:

```text
ItemCoreEffectIdentityCatalogSnapshot.v1
ItemCoreEffectCultivationRosterSnapshot.v1
ItemInstanceCoreEffectRuntimeStateSnapshot.v1
```

Provide a stateless assembler boundary, for example:

```text
IItemInstanceCoreEffectRuntimeStateAssembler.Assemble(input)
```

The runtime input must explicitly carry:

```text
ItemCoreEffectIdentityCatalogSnapshot identityCatalog
ItemCoreEffectCultivationRosterSnapshot cultivationRoster
ItemInstanceProjectionSetSnapshot projectionSet
ItemInstancePlacementBindingContractSnapshot bindingSnapshot
ItemSystemSnapshot itemSystemSnapshot
ItemCoreEffectRosterCompleteness rosterCompleteness
```

The output is a derived read-only snapshot. `ItemSystemBattleSandboxBoardAuthority` remains
the owner of the accepted current state. The new assembler must not become a second Item
state owner and must not mutate any input.

## 4. Identity catalog rows

Each immutable identity row must include at minimum:

```text
baseItemId
nodeKind
candidateDefinitionId
awakeningNodeId
displayName
description
candidateUnlockLevel
awakeningUnlockLevel
requiredRarity
requiredRarityKey
candidateSourceIdentity
awakeningSourceIdentity
```

The identity catalog must retain the Candidate player-facing text and payload identity, but
must not execute `ExtraTrigger`, `MechanicConversion`, `Convert` or any effect payload.

## 5. Explicit cultivation input

Each ordinary instance cultivation row must include:

```text
itemInstanceId
baseItemId
inputLevel
sourceKey
factCompleteness
```

Rules:

```text
Complete: inputLevel is present and within 1..40.
Unknown: level is absent; no resolved level, unlocked false or Lv.1 may be invented.
Invalid: duplicate identity, item mismatch, I031 ordinary row, level below 1 or above 40.
```

BattleSandbox must use an explicit complete dev-only roster for its deterministic Orange
I001-I030 roster:

```text
inputLevel = 40
sourceKey = BattleSandboxDevHandtestAllLv40
factCompleteness = Complete
```

This is only the full-detail BattleSandbox handtest fixture. It is not formal cultivation,
does not write SaveData and does not change progression, drops, Roll, rewards or Battle.
Component fixtures must additionally cover explicit levels `1/10/20/30/40` so locked,
unlocked-inactive and active states are all verified.

## 6. Required runtime output

The immutable state snapshot must include:

```text
schemaId / status / rosterCompleteness
sourceIdentityCatalogSignature
sourceCultivationRosterSignature
sourceProjectionSetSignature
sourceBindingSignature
sourceItemSystemSignature
Items[]
validationErrors[]
canonicalSignature
```

Each ordinary instance row must include at minimum:

```text
itemInstanceId / baseItemId / rarity
location: Inventory / Board / Unknown
placementId
cultivationLevel / cultivationLevelSource / cultivationFactCompleteness
isLitFact
CoreEffectRows[]
stateCompleteness
```

Each core-effect row must include at minimum:

```text
baseItemId / itemInstanceId
nodeKind
candidateDefinitionId
awakeningNodeId
displayName / description
requiredLevel / requiredRarity / requiredRarityKey
eligibleFact
visibleFact
unlockedFact
litFact
activeFact
stateCompleteness
sourceIdentities
```

`eligibleFact` and `visibleFact` come only from the exact generated instance Projection.
`unlockedFact` and `activeFact` must reuse `ItemCoreAwakeningResolver`; thresholds or active
logic must not be copied into the new assembler. `litFact` comes only from the exact
ItemSystem placement/lighting fact.

## 7. Inventory and Board semantics

### Inventory ordinary instance

With a complete 30-instance roster and a valid Projection absent from IF01:

```text
location = Inventory
candidate identity/name/description = retained
eligible/visible = known from Projection
unlocked = resolved from explicit cultivation input through the existing Awakening resolver
placement/lit/active = NotApplicable
```

The implementation may invoke the existing resolver with an explicitly synthetic unlit
ordinary lighting input only to obtain the existing unlock rule. The emitted runtime row must
still mark Inventory `lit` and `active` as `NotApplicable`, not false.

### Board ordinary instance

`itemInstanceId + placementId + baseItemId` must match Projection, IF01 and
ItemSystemSnapshot.v2 exactly.

```text
location = Board
eligible/visible = Projection facts
unlocked = exact mapped Awakening node result
lit = exact ItemSystem placement lighting fact
active = exact mapped Awakening active result
```

BoardAuthority must pass placement-specific Awakening inputs from the explicit cultivation
roster into `ItemSystemSnapshotInput`. It must not allow `ItemSystemDefaultPreviewLv1` or
`MissingInputDefaultLv1` for an accepted complete BattleSandbox transaction.

Movement rebuilds the state against the new placement and lighting result. Return to Inventory
clears placement/lit/active and restores Inventory semantics without retaining stale Board data.

### Unknown, Invalid and Known False

```text
Unknown:
  null/incomplete roster, missing level source, missing required authority or unavailable fact.
  Missing booleans and levels remain absent; they do not become false, zero, None or Lv.1.

Invalid:
  duplicate/cross-item identity, orphan binding, stale placement, identity mismatch,
  conflicting nodeKind/level/rarity, active without unlocked/lit, illegal level,
  I031 ordinary row or input collection contradiction.

Known False:
  only a complete authority explicitly reports the fact false.
```

Unknown, NotApplicable and Known False must have distinct canonical representations.

## 8. I031 boundary

I031 remains the unique special system Item. It must not receive:

```text
ordinary itemInstanceId
ordinary Projection row
ordinary cultivation row
Candidate/Awakening identity row
ordinary core-effect runtime row
rarity/affix/Build qualification
```

Existing I031 inventory/placement/lighting behavior remains unchanged. Any attempt to include
I031 in an ordinary identity, cultivation, Projection or runtime-state roster is invalid.

## 9. BoardAuthority atomic integration

`ItemSystemBattleSandboxBoardAuthority` may expose read-only:

```text
CurrentCoreEffectRuntimeState
CoreEffectRuntimeAssemblyCount
```

On a changed commit, the accepted transaction order is:

```text
1. Build ItemSystemSnapshot.v2 with explicit placement Awakening inputs.
2. Validate IF01.
3. Assemble ItemInstanceQualifiedBuildStateSnapshot.v1.
4. Assemble ItemInstanceCoreEffectRuntimeStateSnapshot.v1.
5. Run existing P6 evaluation without changing its input or semantics.
6. Only after every required result passes, atomically replace:
   CurrentSnapshot
   CurrentBindingSnapshot
   CurrentQualifiedBuildState
   CurrentCoreEffectRuntimeState
   CurrentLayoutResilienceSnapshot
```

Invalid assembly rejects the transaction and leaves all five previous values untouched.
Unknown is retained as explicit Unknown only for legacy constructor/test paths that omit a
cultivation roster; it must never be converted to Lv.1. The real BattleSandbox adapter must
inject the complete explicit dev roster and produce a Valid runtime state.

A no-op must not reassemble, increment counters, replace snapshot identities or emit duplicate
notification. Existing IF01, qualified Build and P6 semantics remain unchanged.

## 10. Exact file whitelist

Expected new files: exactly `13`.

```text
Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState.meta
Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs
Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs
Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs
Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs.meta
Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractReport.md
Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractSpec.csv
Docs/V0.4/Reports/ItemCoreEffectIdentityMatrix.csv
Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractLeakCheckReport.md
```

Allowed existing-file modifications: exactly these two files.

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs
```

Both files are existing untracked workspace files. Development must use their task-start disk
contents and hashes, not Git HEAD. Do not clean, recreate, rollback or replace either file.
No `.meta` modification is authorized for existing files.

## 11. Canonical and immutable rules

All new collections must be defensive copies exposed as immutable/read-only collections.
External arrays and `IList<T>` mutation attempts must be blocked.

New signatures are independent and must use SHA-256, Ordinal comparison, InvariantCulture,
explicit field names, explicit value presence and length-prefixed values.

Ordering:

```text
Identity rows: baseItemId Ordinal, then explicit Core1/Core2/Core3/Core4/Ultimate rank
Cultivation rows: itemInstanceId Ordinal
Item rows: itemInstanceId Ordinal
Core rows: explicit Core1/Core2/Core3/Core4/Ultimate rank
Validation errors: code + source identities + message, all Ordinal
```

Reversed input order must produce the same canonical signature. Do not change the canonical
payload or signature algorithm of Candidate/Profile, Roll, Projection/ProjectionSet, IF01,
ItemSystemSnapshot.v2, qualified Build state, Awakening rules or P6.

## 12. Required real and fixture cases

Use the real deterministic Orange instances, including at minimum:

```text
I001 / wb_i001_orange_404310001
I004 / wb_i004_orange_404310004
I006 / wb_i006_orange_404310006
I009 / wb_i009_orange_404310009
I031 special identity
```

Required coverage:

```text
150/150 identity rows and five exact mappings per I001-I030
I031 zero identity/runtime rows
White/Green/Blue/Purple/Orange eligible and visible Projection facts
explicit Lv1/Lv10/Lv20/Lv30/Lv40 fixtures
Inventory static text/identity retained
Inventory runtime lit/active NotApplicable
Board unlit, Board lit, move and return-to-Inventory
Core4 and Ultimate remain separate at Lv40
missing cultivation source remains Unknown, never Lv1
known locked/unlocked/inactive/active facts remain distinct
stale itemInstanceId/placementId/baseItemId rejection
duplicate/missing/cross-item Candidate-Awakening mapping rejection
active without unlocked or lit rejection
external mutation blocked
reversed input determinism
atomic five-snapshot commit and rollback-on-failure
no-op identity and call-count stability
```

## 13. Frozen task-start baselines

Allowed existing files, captured from the current disk and expected to change only within this
Assignment:

```text
ItemSystemBattleSandboxBoardAuthority.cs = e1add9eddca28802cd297bc8becac378494e8cbcfcec2754ca50c732c78b0359
ItemSystemBattleSandboxBoardAdapter.cs = 6ecd25971105adc7b6183cb66598d5b0f135041a3cff392ee130b2acc05bb272
```

Protected files and values that must remain byte-identical:

```text
ItemSystemBattleSandboxViewProjection.cs = ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec
ItemSystemBattleSandboxItemDetailAdapter.cs = 1c1504948012b94dcbb1cc91310becd2df169bb736003737c5109f19cd72cc06
ItemCoreAwakeningRules.cs = 294e73d516189f541d0e1e92e0efad583a634980d4b4b10ad0d2117075cf224b
ItemCompleteCandidateContent.cs = c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb
ItemBalanceProfile.cs = fd2b3700b0a500fd7f1450c5e1acff5a90617f657251bd49136cf712005fb91d
ItemBalanceWorkbenchCatalog.cs = 8ac332b9905f912e8c6aaa9facb116fb49f4ab0523f0bad90a350682301d3428
ItemBalanceWorkbenchCatalog.asset = 5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45
Profiles30 aggregate = a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0
ItemCandidateCoreEffects150.csv = 5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188
ItemInstanceProjectionContract.cs = f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967
ItemInstancePlacementBindingContract.cs = 335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0
ItemSystemSnapshot.cs = 794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827
ItemInstanceQualifiedBuildStateContract.cs = f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464
ItemInstanceQualifiedBuildStateAssembler.cs = 798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8
ItemInstanceQualifiedBuildStateValidation.cs = 95341ddbc3572a1d116287ac4edb221698d56b1b7133768daa45fbee02c0179d
ItemDetailProjectionComposer.cs = 5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782
ItemDetailViewModel.cs = ced80cfa99b330ca3f5d67fc22198641de817105608b678603c33343ab555e94
ItemDetailPanelView.cs = 89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d
ItemDetailSectionView.cs = 957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d
ItemSandbox scene = 8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29
BattleSandbox scene = 4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6
ItemDetailPanel.prefab = 2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c
EditorBuildSettings.asset = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
ItemCoreAwakeningCore4NodeExpansion01_Assignment.md = 8f9af4b60b601f64a59fe8741c95792d8222857b66403843b70340ed46e7a03f
```

`Profiles30 aggregate` is SHA-256 over the 30 Ordinal filename rows formatted as
`relativePath|lowercaseFileSha256\n`.

Development must record its immediate pre-edit hashes and stop if either allowed existing
file differs from the baseline above. A baseline change requires Guard review; it must not be
silently accepted.

## 14. Verification and reports

Pure fixture verification must not open, save or rebuild Scene/Prefab/assets. Unity batch entry:

```text
TalismanBag.EditorTools.ItemSandbox.ItemCoreEffectIdentityAndRuntimeStateContractVerifier.VerifyStaticBatch
```

Required reports:

```text
Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractReport.md
Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractSpec.csv
Docs/V0.4/Reports/ItemCoreEffectIdentityMatrix.csv
Docs/V0.4/Reports/ItemCoreEffectIdentityAndRuntimeStateContractLeakCheckReport.md
```

Required result markers:

```text
COMPONENT_FIXTURE_PASS
REAL_RUNTIME_STATE_ASSEMBLY_PASS
IDENTITY_MAPPING_150_OF_150_PASS
CORE4_ULTIMATE_SEPARATION_PASS
EXPLICIT_DEV_CULTIVATION_SOURCE_PASS
MISSING_CULTIVATION_REMAINS_UNKNOWN_PASS
INVENTORY_BOARD_TRANSITIONS_PASS
I031_EXCLUSION_PASS
ATOMIC_COMMIT_PASS
NO_OP_STABILITY_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_NOT_APPLICABLE
```

Unity compile and batch verifier must pass. Regress at minimum CoreAwakeningPreview01,
ItemSystemValidatorAndSnapshot01, ItemInstanceProjectionContract01,
ItemInstancePlacementBindingContract01, ItemInstanceQualifiedBuildStateAdapter01 and the
current BattleSandbox board authority verifier without changing their protected sources or
canonical signatures.

The report must state that no player-visible detail change occurs until the separately assigned
`V0.4-ItemDetailQualifiedCoreEffectProjection01` consumes this state.

## 15. Hard prohibitions

Do not modify or connect:

```text
ItemDetailProjectionComposer / ItemDetailViewModel / ItemDetailPanelView / ItemDetailSectionView
Scene / Prefab / Font / Sprite / Icon / RectTransform / BuildSettings
Candidate/Profile content / Candidate values / ExtraTrigger / Convert
Roll / rarity probabilities / affix probabilities / drop or reward rules
ItemInstanceProjection / IF01 / ItemSystemSnapshot.v2 schema or signature
ItemCoreAwakeningRules thresholds, IDs, resolver behavior or signature
qualified Build state / Build thresholds / Build detail projection
I031 ownership, placement or lighting rules
Enemy / Capability / P1 / P6 semantics
formal Battle / RunFlow / SaveData / Reward / Chapter / Boss
```

Do not create a second cultivation owner, default missing levels to Lv.1, parse IDs to invent
identity, infer Inventory from an incomplete roster, convert Unknown to false/zero, execute
core effects, run a Builder, update hash/geometry baselines, start the detail projection or
Prefab packages, or commit/tag/push/reset/rollback.
