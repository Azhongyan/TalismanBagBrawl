# V0.4-ItemCoreAwakeningCore4NodeExpansion01 Assignment

Status: `READY_FOR_DEV / ITEM_GUARD_APPROVED / USER_MAPPING_DECISION_C_APPROVED`

Package: `V0.4-ItemCoreAwakeningCore4NodeExpansion01`

Primary Guard: `Item Guard`

Joint Guard: `NOT_REQUIRED`

Assignment freeze date: `2026-07-23 Asia/Shanghai`

Task-start branch: `backup/v0.4-itemdetail-maxdao-art-template01-wip-20260717`

Task-start HEAD: `f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba`

## 0. Required receipt and package boundary

Accepted receipt:

```text
ITEM_GUARD_PASS_ITEMCOREAWAKENINGCORE4NODEEXPANSION01
```

Target development handoff after implementation and QA:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS
Package: V0.4-ItemCoreAwakeningCore4NodeExpansion01
Development: DEV_COMPLETE
QA: PASS
Core4NodeExpansion: PASS
UserHandtest: NOT_APPLICABLE
NextPackage: NOT_STARTED
```

This Assignment authorizes only `V0.4-ItemCoreAwakeningCore4NodeExpansion01`.
It does not authorize:

```text
V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01
V0.4-ItemDetailQualifiedCoreEffectProjection01
ItemDetail Base Prefab migration
UnifiedBattlePage
```

The development window must stop after this package reaches its required reports and must
not start any downstream package automatically.

## 1. Accepted user decision

The previous Item Guard audit found five Candidate core definitions but only four Awakening
nodes for each ordinary item. The user approved global decision C for `I001-I030`:

```text
Candidate Core1    -> Ixxx_CORE_01
Candidate Core2    -> Ixxx_CORE_02
Candidate Core3    -> Ixxx_CORE_03
Candidate Core4    -> Ixxx_CORE_04
Candidate Ultimate -> Ixxx_CORE_ULT
```

Core4 and Ultimate are two independent identities. They may not share `CORE_ULT` and neither
row may be deleted, truncated, copied or inferred from display order.

The accepted design does not authorize probability, value, formal battle, SaveData, Reward,
drop, Roll or progression-system changes.

## 2. Package objective

Expand the existing Item Core Awakening definition and resolver model from four nodes to five
nodes so that every ordinary item `I001-I030` exposes an independent Core4 node.

After this package, the Awakening side must contain exactly `150` unique definitions:

```text
30 ordinary items * 5 nodes = 150 definitions
```

`I031` remains a special system item and must expose zero ordinary Awakening definitions.

This package establishes the missing Awakening identity only. It does not yet join Candidate
definitions, item instances, dev-only cultivation levels and ItemSystem runtime facts. That
join belongs exclusively to the later
`V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01` package.

## 3. Exact node identity contract

For every `I001-I030`, `DefaultItemCoreEffectDefinitionProvider` must return exactly these
five definitions in this output order:

| Order | coreEffectId | nodeKind | unlockLevel | requiredRarityKey | Candidate semantic authority |
|---:|---|---|---:|---|---|
| 1 | `Ixxx_CORE_01` | `Core1` | 10 | `white` | Candidate Core1 |
| 2 | `Ixxx_CORE_02` | `Core2` | 20 | `green` | Candidate Core2 |
| 3 | `Ixxx_CORE_03` | `Core3` | 30 | `blue` | Candidate Core3 |
| 4 | `Ixxx_CORE_04` | `Core4` | 40 | `purple` | `ExtraTrigger`, independent cooldown |
| 5 | `Ixxx_CORE_ULT` | `Ultimate` | 40 | `orange` | `MechanicConversion / Convert` |

`requiredRarityKey` is identity metadata in this package. It must not become a new rarity
unlock algorithm, probability rule or activation gate. Candidate content remains the
authority for names, descriptions, effect categories, operations and numeric payloads.

Do not copy Candidate descriptions or `ExtraTrigger` execution into the Awakening resolver.
The Awakening node may continue to use `Preview Reserved` until the later identity/runtime
contract supplies the authoritative joined definition.

## 4. Enum and compatibility requirements

Add `ItemCoreAwakeningNodeKind.Core4` without changing the existing numeric identity of
`Core1`, `Core2`, `Core3` or `Ultimate`.

The safe compatibility requirement is:

```text
Core1    = existing value
Core2    = existing value
Core3    = existing value
Ultimate = existing value
Core4    = new appended value
```

Do not rely on enum numeric order for presentation or identity mapping. The authoritative
Awakening output order must be the explicit default-node order:

```text
Core1, Core2, Core3, Core4, Ultimate
```

`Core4.isUltimate` must be false. Only `Ultimate` may satisfy the existing Ultimate-only
branches, including `highRarityUltimatePreview` handling.

All ID and item comparisons must remain `StringComparison.Ordinal`; formatting and numeric
text must remain `InvariantCulture` where applicable.

## 5. Existing algorithm reuse

The existing Awakening resolver remains the sole unlock and activation rule owner.

Required behavior:

```text
Core1 unlockLevel = 10
Core2 unlockLevel = 20
Core3 unlockLevel = 30
Core4 unlockLevel = 40
Ultimate unlockLevel = 40

unlocked = resolvedLevel >= definition.unlockLevel
active   = item is lit AND node is unlocked
```

At Lv39, only Core1/Core2/Core3 are unlocked. At Lv40, Core4 and Ultimate unlock as two
separate nodes. For an unlit item at Lv40 both are `UnlockedInactive`; for a lit item at
Lv40 both are `Active`.

The following existing behavior must not change in this package:

```text
input level clamp and validation
missing-input default-Lv1 behavior
lighting facts
ItemNotLit / LevelTooLow / DefinitionMissing behavior
Ultimate-only reserved preview behavior
I031 unsupported behavior
```

The later runtime-state contract will replace silent/default level assumptions with explicit
`DevHandtestFixture` completeness. This package must not pre-implement that work.

## 6. Definition validation requirements

The default-node and definition validator must treat Core4 as a required independent node.

At minimum validate:

```text
missing Core4 definition -> validation error + DefinitionMissing Core4 node
duplicate CORE_04 id -> validation error
duplicate Core4 nodeKind -> validation error
Core4 itemId mismatch -> validation error
empty CORE_04 id -> validation error
Core4 unlockLevel other than 40 -> validation error
Core4 mapped to Ultimate nodeKind -> validation error
Ultimate mapped to Core4 nodeKind -> validation error
Core4 and Ultimate sharing one coreEffectId -> validation error
reversed definition input order -> identical explicit output order
```

An invalid Core4 definition must not remove, rename or substitute Ultimate. An invalid
Ultimate definition must not fall back to Core4.

## 7. ItemSystemSnapshot compatibility

`ItemSystemSnapshot.v2` already freezes Awakening nodes generically and is not authorized for
modification. The verifier must prove that an ordinary five-node Awakening result round-trips
through the current snapshot without losing, merging or renaming `CORE_04` or `CORE_ULT`.

Required round-trip facts:

```text
NodeStates.Count == 5
CORE_04 nodeKind == Core4
CORE_ULT nodeKind == Ultimate
CORE_04 and CORE_ULT remain distinct after ToCoreAwakeningResolutionResult()
Lv40 lit/unlit state remains unchanged through the snapshot
I031 NodeStates.Count == 0
```

No ItemSystem snapshot schema, signature, placement, lighting or Build behavior may change.

## 8. Candidate authority protection

The existing Candidate source remains byte-identical. The verifier may read it or its
generated audit report only for regression evidence.

It must confirm:

```text
I001-I030 each still contain Core1/Core2/Core3/Core4/Ultimate
Candidate total remains 150
candidate_core_ixxx_04 remains Core4 / Lv40 / purple / ExtraTrigger
candidate_core_ixxx_ultimate remains Ultimate / Lv40 / orange / Convert
no Candidate ID is rewritten to an Awakening ID
```

This package must not create the final Candidate-to-Awakening bridge. The later identity
contract must bind the two already-approved identity sets explicitly by `baseItemId` and
machine `nodeKind`, never by array index, Chinese display name, PNG, object name or display
order.

## 9. Exact development file whitelist

Expected new development files: exactly `0`.

Allowed existing-file modifications: exactly these five files.

```text
Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs
Docs/V0.4/Reports/CoreAwakeningPreviewReport.md
Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv
Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md
```

Guard-owned control file, read-only to the development window:

```text
Docs/V0.4/ItemCoreAwakeningCore4NodeExpansion01_Assignment.md
```

Do not modify `.meta` files. Do not add a second Awakening resolver, provider, verifier or
report family. Extend the existing implementation and verifier in place.

## 10. Whitelisted task-start SHA-256

The following SHA-256 values were captured from the real task-start disk before Assignment
creation:

```text
ItemCoreAwakeningRules.cs = 8b19114ddaf11f886a387f69c037e6c1b86bcb6c63745104b282a6bdcb19e8f6
CoreAwakeningPreviewVerifier.cs = 8f744b67447f834438f512412da29493990f63715d0647a5936df36518d237ca
CoreAwakeningPreviewReport.md = ac123821319b3dfc1a57de64e7c0e0de825852c6b334fd41a2a2ca69be495904
CoreAwakeningPreviewSpec.csv = 6cd74ef75fdf77f539896fd475cada59cd9b82f93d316b3adb9efd51d50d9953
CoreAwakeningPreviewLeakCheckReport.md = 73f0623aa566855c4a8f19abf488e309f7de75b33505ee540fcc128fa3a36210
```

All five files were tracked and clean relative to the current HEAD when this Assignment was
signed. The development window must re-check immediate pre-edit disk hashes and stop if any
value differs.

## 11. Protected task-start SHA-256

All paths in this section are outside the development whitelist and must remain byte-identical:

```text
ItemSystemSnapshot.cs = 794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827
ItemCompleteCandidateContent.cs = c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb
ItemCandidateCoreEffects150.csv = 5a725aa3630aaf2ec6829fda0cc76ecded3a02346b249c16d85e21b06b698188
Profiles30 aggregate = a4cbbd2a940b0f6189280c3afca404ebae414bcaf982338f3866912a11ea00b0
ItemBalanceWorkbenchCatalog.asset = 5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45
ItemCorePotentialAndBuildEligibilitySchema.cs = 0a3cc80b1dc047dfe6ba6caec83e01a0be69aed78f7697e7666b401ecfac2b99
ItemFullDetailBuildSandboxWorkbenchSession.cs = da81315cdcd71310ea43729a4ae043b9a61cf33f0e889c706bf81d5bcf24871b
ItemBalanceCandidateDetailSandboxAdapter.cs = 65ee8a3be9a388e2673944f42822c19d1de27ca3ac7abb3232eec94231c65a84
ItemDetailProjectionComposer.cs = 5b4017a3d5fd7e2c73c73f88f6d31a7e24824e9e748ff929d74c72333055a782
ItemDetailViewModel.cs = ced80cfa99b330ca3f5d67fc22198641de817105608b678603c33343ab555e94
ItemDetailPanelView.cs = 89c6b3b0b2620ed31edfff7fda5b747849ce5b9939479c0deab8ac24f2140c2d
ItemDetailSectionView.cs = 957910d33006e8006d855cbe5eac9335685be3d9ce32592e3c362d80e578ec2d
ItemSystemBattleSandboxBoardAuthority.cs = e1add9eddca28802cd297bc8becac378494e8cbcfcec2754ca50c732c78b0359
ItemSystemBattleSandboxViewProjection.cs = ff7eb47f3d609b08ddb9175782b47bd58078e7d505acc4b9ac44351addc0e9ec
ItemSystemBattleSandboxItemDetailAdapter.cs = 1c1504948012b94dcbb1cc91310becd2df169bb736003737c5109f19cd72cc06
ItemSandbox scene = 8b0ff7c60598fdc112a5ce3e247144388da4790101ba77c2a8f7498cedd6dd29
BattleSandboxPreview scene = 4c0927ac8633c004184e232cd5e11efc000a64b456dc9c4ca1be5dac4f5b77d6
ItemDetailPanel.prefab = 2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c
EditorBuildSettings.asset = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
```

`Profiles30 aggregate` is SHA-256 over the 30 Ordinal filename rows in the form
`relativePath|lowercaseFileSha256\n`.

Several protected files already contain user-owned modifications or are untracked in the
current worktree. Their disk hashes above, not Git HEAD, are the protected baseline. Do not
clean, recreate, stage, rollback or normalize them.

## 12. Verifier changes and required reports

Extend the existing verifier and keep the existing batch entry:

```text
TalismanBag.EditorTools.ItemSandbox.CoreAwakeningPreviewVerifier.VerifyStaticBatch
```

The verifier must update the existing three reports in place and identify the current
package as `V0.4-ItemCoreAwakeningCore4NodeExpansion01`.

Required checks:

```text
I001-I030 definitions: 30 * 5 = 150
all 150 Awakening coreEffectIds unique
exact per-item order: 01, 02, 03, 04, ULT
exact nodeKind, level and rarity metadata
I031 definitions and NodeStates remain empty
Lv39/Lv40 lit and unlit boundary behavior
Core4 and Ultimate simultaneously unlocked but independently identified at Lv40
Core4 remains outside Ultimate-only branches
all definition invalid cases include Core4
reversed source order remains deterministic
ItemSystemSnapshot.v2 five-node round-trip
Candidate authority hash/count regression
external source scope and forbidden-token LeakCheck
```

The existing verifier's four-node assumptions must be updated wherever they describe the
ordinary Awakening definition set. Do not change unrelated Build, Lighting, ArrayBonus or
Item Detail behavior merely to satisfy the verifier.

Required report receipts:

```text
COMPONENT_FIXTURE_PASS
CORE4_NODE_EXPANSION_PASS
ITEMSYSTEM_SNAPSHOT_ROUNDTRIP_PASS
I001_I030_AWAKENING_150_PASS
I031_EXCLUSION_PASS
PROTECTED_HASHES_PASS
UNITY_COMPILE_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_NOT_APPLICABLE
```

## 13. Package-scoped verification

Verification must include:

1. Compile the Unity Editor project without opening or saving Scene/Prefab assets.
2. Run `CoreAwakeningPreviewVerifier.VerifyStaticBatch`.
3. Confirm the report and CSV contain Core4-specific cases and no four-node ordinary-item
   claim remains.
4. Recompute every protected hash in section 11.
5. Confirm the package diff contains only the five whitelisted files.
6. Run package-scoped `git diff --check` on the five whitelisted files.

Because the repository is intentionally dirty, repository-wide `git status` or
repository-wide `git diff` is evidence only and must not be used to clean unrelated files.

No user handtest is required for this data/identity expansion package. Runtime instance
assembly and UI correctness remain explicitly unclaimed until their later packages.

## 14. Hard prohibitions

Do not modify or create:

```text
Candidate profiles, candidate generator, candidate IDs, names, descriptions or values
ItemSystemSnapshot.v2 or its signature
ItemInstance Projection / ProjectionSet / IF01 / Package A contracts
Item Detail adapters, composers, views, layout or core-effect projection
ItemDetailPanelView / ItemDetailSectionView
ItemFullDetailBuildSandboxWorkbenchSession
Scene / Prefab / Font / Sprite / Icon / RectTransform
Build track projection or ItemBuildSynergyRules
Item lighting algorithm
Roll, rarity probability, core-effect probability, drop or reward
formal Battle, battle effect execution or ExtraTrigger execution
Enemy / Capability / RunFlow / SaveData / Chapter / Boss
BuildSettings
```

Do not:

```text
map Candidate and Awakening by array index or display order
map Core4 and Ultimate to one node
delete or hide Candidate Core4
invent a Core4 numeric payload or cooldown value
make rarity metadata unlock or activate a node
fix missing-level defaulting in this package
start the runtime-state contract, UI projection or prefab migration
commit, tag, push, reset, rollback or clean user-owned changes
```

## 15. Stop conditions

Stop and return to Item Guard if:

```text
any whitelisted task-start hash differs before edit
any protected hash changes
Core4 requires a second resolver or a new activation algorithm
adding Core4 requires ItemSystemSnapshot schema modification
Unity compilation exposes a consumer that cannot accept Core4 without UI, battle, save,
probability or Build changes
the verifier can pass only by weakening I031, duplicate-ID or nodeKind validation
the package diff exceeds the exact whitelist
```

On any stop condition, do not widen the package locally. Return a focused failure report.

## 16. Completion state

Successful development must stop at:

```text
DEV_COMPLETE
QA_PASS
CORE4_NODE_EXPANSION_PASS
USER_HANDTEST_NOT_APPLICABLE
```

Only after Item Guard accepts that result may the user separately authorize or dispatch:

```text
V0.4-ItemCoreEffectIdentityAndRuntimeStateContract01
```
