# V0.4-ItemDropDeterministicCorePromotion01 Assignment

```text
Package: V0.4-ItemDropDeterministicCorePromotion01
Phase: P0B
Owner: ITEM_ALGORITHM
Coordinating Guard / Close Review: REWARD_DROP_GUARD
Execution mode: COMPLEX_GUARDED_ONCE / COMPONENT_PACKAGE
Data maturity: DEV_ONLY_CANDIDATE
Formal readiness: NOT_LIVE_LOCKED / NOT_FORMAL_DROP_READY
User hand test: NOT_REQUIRED
Git: FORBIDDEN
```

This is the only implementation Assignment for P0B. It freezes one extraction:
promote the deterministic item-drop generation algorithm currently embedded in
the QA Sandbox into one reusable Item-owned core, while leaving the existing
Sandbox as a QA compatibility adapter.

P0B does not create a second item Roll implementation and does not bind any
product flow.

## 1. Authority and entry gate

P0B is authorized by:

```text
OVERALL_GUARD_AUTHORIZE_P0B_ITEM_DROP_DETERMINISTIC_CORE_PROMOTION01
```

P0A dependency:

```text
V0.4-DropRewardImmutableContracts01
DROP_REWARD_IMMUTABLE_CONTRACTS01_PASS
Assignment SHA-256:
6cb361ea6628cd6958f24247d77a43bb73d8241b1780d7e6614fea04cb914090
```

P0A contracts are read-only context in this package. P0B must not modify them
or bind them to ChapterFlow, StageClear, Scene, Inventory, Save or UI.

Before editing, the implementation task must:

1. read all mandatory `AGENTS.md` / `Docs/LOCKED/*` / ROADMAP / CURRENT /
   Package Queue entries;
2. read this Assignment in full;
3. verify this Assignment SHA-256 against the dispatch prompt;
4. recheck active Items/Generation task leases;
5. recheck every exact write target below;
6. stop with `P0B_WRITE_LEASE_COLLISION_BLOCKED` if another active package
   owns any overlapping runtime, verifier or report path.

No active overlapping Item/Generation task existed when this Assignment was
frozen. That fact must be checked again immediately before the first edit.

## 2. Frozen current truth

### 2.1 Existing single instance Roll owner

The only Item instance Roll owner remains:

```text
Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs
SHA-256:
19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e
```

Its deterministic primitive remains:

```text
Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/DeterministicItemRandom.cs
SHA-256:
021959b426899ff937dae82dfc563b15d52a83a7591d19374c0ddc67ecccf075
```

Both files are read-only in P0B.

P0B must call `ItemInstanceRollEngine.Generate` for the final instance. It must
not copy, fork, wrap-and-reroll, or reimplement:

```text
stat Roll
fixed-affix value Roll
random-affix selection/value Roll
mutex/repeat rules
core-potential resolution
BuildQualification Roll
ItemGeneratedInstanceSnapshot construction
```

### 2.2 Existing embedded drop algorithm

The current deterministic drop selection implementation is embedded in:

```text
Assets/_Game/Scripts/TalismanBag/Items/Generation/DropSandbox/
  DeterministicItemDropRandom.cs
  ItemDropGenerationSandbox.cs
```

Frozen pre-edit hashes:

```text
DeterministicItemDropRandom.cs
51bf736fd6a57a473380888b1e57f4bffffd43c4defea873f271415bfdc79c90

ItemDropGenerationSandbox.cs
65577a4bcee916be29dd7c233c8945c5ec1bd1083ce68bab7c181113b5698cee
```

The legacy immutable QA contracts and snapshot are in:

```text
Assets/_Game/Scripts/TalismanBag/Items/Generation/DropSandbox/
  ItemDropGenerationSnapshot.cs
```

Frozen read-only hash:

```text
1f09e4cfd5ed92aa91dd174b96e0e0af8be76ec803b1804f57ef98c918e8fe8a
```

P0B extracts the actual deterministic selection/generation path. It does not
delete the legacy API, change its public contracts, or require existing QA
callers to migrate.

### 2.3 Current reports are stale aggregate evidence

The following legacy reports currently contain package-local assertions that
pass but stale aggregate `Result: FAIL` claims inherited from historical
reports:

```text
Docs/V0.4/Reports/ItemInstanceRollEngineReport.md
Docs/V0.4/Reports/ItemDropGenerationSandboxReport.md
Docs/V0.4/Reports/ItemGenerationSimulationValidatorReport.md
their existing leak/spec/determinism/distribution companion reports
```

P0B must not:

```text
inherit their aggregate PASS/FAIL as its own result
rewrite them to manufacture a PASS
claim all historical Item suites are green
claim the Queue prose alone proves the current implementation
```

P0B produces a new, self-consistent report set from current source and current
execution. The new report must identify legacy aggregate reports as:

```text
LEGACY_STALE_AGGREGATE / NOT_USED_AS_P0B_VERDICT
```

### 2.4 Protected accepted behavior

The existing QA Sandbox behavior remains protected:

```text
drop algorithm ID: item-drop-generation-v1
generationVersion: 1
candidate domain golden: 6DEE1D6E50111E1B
rarity domain golden: 7882C5BB16185E78
segmented-domain collision separation:
57B30C3DC752992A / 12F4959138EDF7BC
same-input repeat: 1000/1000
ordinary identities: I001-I030
ordinary I031 output count: 0
legacy early-stage policy: stages 1-10 produce white without a rarity stream
legacy adapter snapshot schema: ItemDropGenerationSnapshot.v1
instance generation schema: ItemGeneratedInstanceSnapshot.v1
```

The read-only baseline rows are:

```text
Docs/V0.4/Reports/ItemDropGenerationSandboxSamples.csv
Docs/V0.4/Reports/ItemDropGenerationSandboxDeterminism.csv
Docs/V0.4/Reports/ItemInstanceRollEngineDeterminism.csv
```

P0B may consume these rows as explicit baseline vectors. It may not edit them.

## 3. Owner and layering freeze

The promoted ownership is:

```text
future DropRequest adapter (not built in P0B)
  -> ItemDropDeterministicCore
  -> ItemInstanceRollEngine
  -> ItemGeneratedInstanceSnapshot
```

The existing QA path becomes:

```text
ItemDropGenerationSandbox
  -> translates legacy QA request to DEV_ONLY_CANDIDATE core request
  -> ItemDropDeterministicCore
  -> projects the core result back to ItemDropGenerationSnapshot.v1
```

Hard owner rules:

```text
ItemDropDeterministicCore owns candidate selection, rarity-tier selection,
validation of the core request and orchestration of one Item instance Roll.

ItemInstanceRollEngine remains the only owner of stat/affix/core/build instance
Roll.

ItemDropGenerationSandbox owns QA fixture policy adaptation and legacy result
projection only.

Reward-Drop P0A owns immutable reward correlation contracts only.
```

The promoted core must not depend on:

```text
ChapterFlow
StageClearFact
Battle result
Enemy
Boss
Scene
Prefab
UI / WorldMap
Inventory
SaveData / PlayerPrefs
Reward claim/commit
APK / BuildSettings / formal flow
```

## 4. Exact write whitelist

Only the following product/runtime source files may be added:

```text
Assets/_Game/Scripts/TalismanBag/Items/Generation/DropCore.meta
Assets/_Game/Scripts/TalismanBag/Items/Generation/DropCore/
  ItemDropDeterministicCoreContracts.cs
  ItemDropDeterministicCoreContracts.cs.meta
  ItemDropDeterministicDomain.cs
  ItemDropDeterministicDomain.cs.meta
  ItemDropDeterministicCore.cs
  ItemDropDeterministicCore.cs.meta
```

Only the following existing runtime source files may be modified:

```text
Assets/_Game/Scripts/TalismanBag/Items/Generation/DropSandbox/
  DeterministicItemDropRandom.cs
  ItemDropGenerationSandbox.cs
```

Their `.meta` files are protected and must not change.

Only the following verifier files may be added:

```text
Assets/_Game/Scripts/TalismanBag/Editor/ItemGeneration/
  ItemDropDeterministicCorePromotionVerifier.cs
  ItemDropDeterministicCorePromotionVerifier.cs.meta
```

Only the following reports may be added or regenerated:

```text
Docs/V0.4/Reports/ItemDropDeterministicCorePromotion01Report.md
Docs/V0.4/Reports/ItemDropDeterministicCorePromotion01Spec.csv
Docs/V0.4/Reports/ItemDropDeterministicCorePromotion01Determinism.csv
Docs/V0.4/Reports/ItemDropDeterministicCorePromotion01BaselineParity.csv
Docs/V0.4/Reports/ItemDropDeterministicCorePromotion01LeakCheckReport.md
```

This Assignment is frozen and read-only after dispatch:

```text
Docs/V0.4/ItemDropDeterministicCorePromotion01_Assignment.md
```

Everything else is outside the write lease, including:

```text
AGENTS.md
Docs/LOCKED/*
all Package Queue files
P0A RewardDrop contracts/reports
ItemDropGenerationSnapshot.cs
ItemInstanceRollEngine.cs
DeterministicItemRandom.cs
all Item schema/catalog/candidate assets
all legacy Item reports
all existing Item verifiers/simulators
ChapterFlow / Battle / Enemy / Boss
Scene / Prefab / UI / WorldMap
Inventory / Save / Reward
ProjectSettings / Packages / BuildSettings
```

If the extraction cannot be completed inside this whitelist, stop with:

```text
P0B_SCOPE_EXPANSION_REQUIRED
```

Do not edit an extra file and explain it afterward.

## 5. Promoted core contract

Namespace:

```text
TalismanBag.Items.Generation.DropCore
```

Required public identities:

```text
ItemDropCoreDataStatus
ItemDropCoreValidationCodes
ItemDropCoreSourceIdentity
ItemDropCoreCandidateKind
ItemDropCoreCandidateEntry
ItemDropCoreCandidatePool
ItemDropRarityTierMode
ItemDropRarityTierEntry
ItemDropRarityTierProfile
ItemDropDeterministicCoreRequest
ItemDropDeterministicCoreSnapshot
ItemDropDeterministicCoreResult
ItemDropDeterministicDomain
ItemDropDeterministicCore
```

Naming may use `Snapshot` suffix consistently, but there must be one obvious
core request, one core snapshot and one core result. Do not create parallel
variants.

### 5.1 Maturity

The only successful core maturity is:

```text
DEV_ONLY_CANDIDATE
```

Define it as an explicit stable identity. The core must reject:

```text
empty / unspecified
QA_FIXTURE_ONLY as a direct core grant maturity
BALANCE_CANDIDATE without the DEV-only execution identity
PLAYTEST_ACCEPTED
LIVE_LOCKED
FORMAL
any unknown value
```

The QA Sandbox adapter may translate its protected legacy
`QA_FIXTURE_ONLY|NOT_BALANCE_APPROVED|NOT_FORMAL_GENERATION_DATA` input into a
`DEV_ONLY_CANDIDATE` core request. The legacy result must retain its legacy QA
status and canonical payload.

P0B must never output or claim:

```text
LIVE_LOCKED
PLAYTEST_ACCEPTED
FORMAL_DROP_READY
PRODUCT_CLAIMABLE
```

### 5.2 Core request identities

The immutable core request must carry at least:

```text
requestId
itemInstanceId
rootSeed
generationVersion
dataStatus = DEV_ONLY_CANDIDATE

sourceContextId
sourceKey
sourceOrdinal

candidatePoolId
rarityTierProfileId
attributeRollBandProfileId
rollPolicyVersion

foundation
candidatePool
rarityTierProfile
statSchema
affixSchema
coreBuildSchema
buildQualificationRollProfiles
```

`sourceOrdinal` is a generic deterministic source coordinate. It is not a
Chapter or Stage identity and must not be named or validated as a campaign
StageClear.

The following identities must be non-empty and must be preserved unchanged in
the core result and canonical payload:

```text
candidatePoolId
rarityTierProfileId
attributeRollBandProfileId
rollPolicyVersion
```

`attributeRollBandProfileId` is a request/correlation identity only in P0B.
The core passes the supplied Item schemas to `ItemInstanceRollEngine`; it must
not implement a second attribute or affix value Roll based on this string.

`rollPolicyVersion` must identify the protected Item Roll policy:

```text
item-instance-roll-v1
```

### 5.3 Candidate identity and I031 exclusion

Every core candidate must explicitly carry:

```text
baseItemId
candidateKind
weightUnits
```

The only legal candidate kind in P0B is:

```text
OrdinaryGeneratedItem
```

Validation must use Item-owned identity truth:

```text
ItemRarityInstanceFoundation.IsOrdinaryBaseItemId
ItemRarityInstanceFoundation.CoreProgressionBaseItemId
```

Legal ordinary identities are exactly:

```text
I001-I030
```

`I031` must fail ordinary core validation with an explicit stable code such as:

```text
I031_FORBIDDEN
```

Unknown or special identities must not be accepted through display-name,
prefix, suffix or scene-label inference.

Candidate IDs must be unique. Weights must be positive and their sum must not
overflow.

### 5.4 Rarity-tier input

The core consumes an explicit rarity-tier profile. It must not contain
Chapter1 or stage-number policy.

The core supports:

```text
Fixed tier profile
Weighted tier profile
```

The legacy Sandbox adapter owns this mapping only:

```text
legacy stage 1-10
  -> fixed white profile
  -> rarityTierProfileId = LOCKED_STAGE_1_10_WHITE
  -> no rarity RNG domain

legacy stage 11+
  -> existing QA weighted rarity entries
  -> legacy rarity profile ID preserved
```

The promoted core only sees the explicit profile and generic source identity.
It does not know WorldMap, Chapter, campaign stage IDs or product progression.

### 5.5 Deterministic domain

`ItemDropDeterministicDomain` becomes the only owner of drop-domain seed
construction.

Preserve:

```text
algorithmId = item-drop-generation-v1
generationVersion = 1
FNV-1a 64-bit
explicit length-prefixed UTF-8 fields
explicit domain segment count
SplitMix64 stream through existing DeterministicItemRandom.Stream
rejection sampling before modulo
ordinal sorting where order is not semantic
```

The frozen field order and legacy adapter inputs must keep the existing golden
vectors unchanged.

The old `DeterministicItemDropRandom` must become a compatibility façade that
delegates to `ItemDropDeterministicDomain`. It must contain no second hash,
stream or bounded-selection implementation.

Forbidden anywhere in the new core:

```text
UnityEngine.Random
System.Random
Random.Range
Guid.NewGuid
DateTime.Now
DateTime.UtcNow
Environment.TickCount
string.GetHashCode
```

### 5.6 Core generation

`ItemDropDeterministicCore.Generate` must:

1. validate the immutable request and all required identities;
2. deterministically choose one ordinary candidate;
3. deterministically resolve one rarity from the explicit rarity-tier profile;
4. resolve the existing Item identity/core profile;
5. create the existing `ItemInstanceIdentitySnapshot`;
6. call `ItemInstanceRollEngine.Generate` exactly once;
7. return an immutable snapshot only when all steps pass;
8. return no partial snapshot on failure.

The core must not allocate a random or clock-based `itemInstanceId`. The caller
supplies the stable instance identity and the result preserves it.

The new core runtime sources together must contain exactly one executable call
to:

```text
ItemInstanceRollEngine.Generate
```

The Sandbox adapter must contain zero such calls.

## 6. Sandbox adapter freeze

`ItemDropGenerationSandbox` must retain:

```text
public static ItemDropGenerationResult Generate(ItemDropGenerationRequest request)
```

Existing callers must compile without migration.

The adapter may:

```text
validate legacy-only QA policy fields
map legacy source/pool/rarity inputs to the core request
assign explicit DEV_ONLY_CANDIDATE execution identity
assign the protected Item roll-policy identity
assign one explicit QA attribute-roll-band identity
delegate exactly once to ItemDropDeterministicCore.Generate
map core validation codes back to existing stable legacy codes
project a successful core snapshot into ItemDropGenerationSnapshot.v1
```

The adapter may not:

```text
select a candidate
consume candidate RNG
select rarity
consume rarity RNG
create Item instance identity
call ItemInstanceRollEngine
Roll stat / affix / core / Build
change the existing legacy snapshot canonical fields
```

Legacy baseline behavior must remain byte-for-byte stable for the frozen
baseline fixtures where the old canonical string is available, and
field-for-field stable for all protected sample rows.

## 7. Canonical and immutability rules

All new request/profile/pool/result collections must be defensively copied and
exposed through read-only surfaces.

No new public mutable field, public setter or mutable collection may exist.

The core snapshot canonical payload/signature must include:

```text
schema identity
drop algorithm identity
DEV_ONLY_CANDIDATE
requestId
source identities
candidatePoolId
rarityTierProfileId
attributeRollBandProfileId
rollPolicyVersion
rootSeed
generationVersion
selectedBaseItemId
selected rarity stable key
itemInstanceId
generated Item canonical signature
```

Same canonical input must produce the same canonical output for at least 1000
repetitions.

Changing each of the following independently must change the core canonical
output or deterministically reject the request:

```text
requestId
itemInstanceId
rootSeed
generationVersion
sourceContextId
sourceKey
sourceOrdinal
candidatePoolId
rarityTierProfileId
attributeRollBandProfileId
rollPolicyVersion
candidate set/weight
rarity tier/weight
```

Input enumeration reversal must not change output where collection ordering is
not semantic.

## 8. Dedicated verifier

Add one verifier:

```text
TalismanBag.EditorTools.ItemGeneration.
ItemDropDeterministicCorePromotionVerifier
```

Required entry points:

```text
[MenuItem] VerifyMenu()
VerifyStaticBatch()
```

The verifier must be dedicated to P0B. It must not edit Scene, Prefab,
BuildSettings, Catalog assets or Package Queue.

### 8.1 Current source compile

The verifier must compile with current project source.

Safety:

```text
Do not terminate, close or disturb an existing Unity process.
Do not bypass UnityLockfile.
Do not start a second Unity process while another Unity process still owns or
may own this project.
```

At Assignment freeze time one responsive headless Unity process from a prior
batch attempt was still present, while `Temp/UnityLockfile` was absent. The
implementation task must recheck this state.

If safe Unity batch execution is unavailable, use a non-destructive scoped
compile/execution route if it genuinely compiles the current P0B sources and
dependencies. Record the exact method. Do not label a text scan as compile.

If no valid compile route exists without disturbing the existing process,
return:

```text
PACKAGE_IMPLEMENTED / EXTERNAL_UNITY_COMPILE_BLOCKED
```

Do not kill a process or manufacture a PASS.

### 8.2 Required positive coverage

At minimum:

```text
DEV_ONLY_CANDIDATE direct core request succeeds
I001-I030 each succeeds through a single-candidate core fixture
all five stable rarity keys can be represented by explicit profiles
rootSeed and generationVersion are preserved
itemInstanceId is preserved
rarityTierProfileId is preserved
attributeRollBandProfileId is preserved
rollPolicyVersion is preserved
generated Item comes from ItemInstanceRollEngine
legacy Sandbox public API still succeeds
legacy adapter and direct core select/project identical Item facts
```

### 8.3 Required negative coverage

At minimum:

```text
null request
unsupported generationVersion
empty/unknown/non-DEV maturity
LIVE_LOCKED request
empty source identity
empty candidatePoolId
empty rarityTierProfileId
empty attributeRollBandProfileId
empty/wrong rollPolicyVersion
empty candidate pool
duplicate candidate
non-positive weight
weight overflow
unknown baseItemId
I031 candidate
non-ordinary candidate category
empty rarity profile
duplicate rarity
invalid rarity
non-positive rarity weight
rarity weight overflow
missing Item foundation/schema/core/build dependency
ItemInstanceRollEngine failure
no partial snapshot on every failure
```

### 8.4 Baseline parity

The parity report must prove:

```text
40/40 frozen ItemDropGenerationSandboxSamples rows match
6/6 frozen ItemDropGenerationSandboxDeterminism rows match
candidate golden = 6DEE1D6E50111E1B
rarity golden = 7882C5BB16185E78
segmented collision vector remains distinct and exact
same-input legacy snapshot repeat = 1000/1000
old adapter output I031 count = 0
old adapter output algorithm/schema identities unchanged
```

The verifier may read the frozen baseline CSVs. It must not rewrite them.

### 8.5 Single-owner scans

Required scans:

```text
new core executable ItemInstanceRollEngine.Generate calls = 1
Sandbox adapter executable ItemInstanceRollEngine.Generate calls = 0
Sandbox adapter executable ItemDropDeterministicCore.Generate calls = 1
DeterministicItemDropRandom contains no local FNV/hash/stream implementation
new core contains no duplicate stat/affix/core/build Roll implementation
new core contains no forbidden nondeterministic API
new core/Sandbox contains no ChapterFlow/StageClear/Scene/UI/WorldMap/
Inventory/Save/Reward/APK/formal binding
runtime writes outside exact whitelist = 0
```

Comments and report text must not be counted as executable dependencies.

## 9. Report truth

The P0B main report must state all of:

```text
Package
Assignment SHA before/after
Scope verdict
Compile method/result
Core schema/algorithm identities
Data maturity = DEV_ONLY_CANDIDATE
Formal readiness = NOT_LIVE_LOCKED / NOT_FORMAL_DROP_READY
Current spec pass/total
Current deterministic pass/total
Current baseline parity pass/total
I001-I030 coverage
I031 ordinary outputs = 0
Single Roll owner scan
Leak counts
Exact files changed/created
Legacy reports = LEGACY_STALE_AGGREGATE / NOT_USED_AS_P0B_VERDICT
User hand test = NOT_REQUIRED
Git actions = 0
```

The reports must not claim:

```text
P1 pool data complete
Chapter1 drop-ready
StageClear integrated
Inventory/Save claim-ready
formal probability approved
LIVE_LOCKED
APK/formal-flow ready
```

## 10. Hard negative scope

P0B must not implement:

```text
Chapter1 1-1..1-10 pool rows
WorldMap drop preview
StageClear -> DropRequest integration
DropRequest -> core adapter for product flow
RewardResult construction
claim/dedupe commit
Inventory capacity/overflow
Save persistence
repeat challenge
offline patrol
pity/guarantee
Boss exclusive product rewards
skin/story/system reward routing
precise product probability tables
Scene/Prefab/UI changes
```

P0B may not modify I001-I030 candidate values, affix values, rarity weights,
Build probabilities or Item shapes. It verifies the current accepted Item
generation behavior only.

## 11. Automatic acceptance

P0B requires no user hand test.

It may self-close only when all of the following pass:

```text
Assignment SHA unchanged
exact write whitelist
valid current-source compile
all current spec rows
all required negative fixtures
1000-repeat determinism
40/40 sample baseline parity
6/6 deterministic baseline parity
I001-I030 coverage
I031 ordinary outputs = 0
immutable public surface scan
single Item Roll owner scan
single drop-domain owner scan
all leak scans
no stale aggregate verdict inheritance
```

Terminal PASS marker:

```text
ITEM_DROP_DETERMINISTIC_CORE_PROMOTION01_PASS
```

If implementation is complete but compile is safely blocked by the existing
Unity process, use the blocked terminal described in section 8.1. Do not use
the PASS marker.

## 12. Return contract

Return exactly:

```text
package=V0.4-ItemDropDeterministicCorePromotion01
assignmentSha256Before=<sha>
assignmentSha256After=<sha>
terminalStatus=<PASS or exact blocker>
terminalMarker=<marker or NONE>

changedExistingFiles=<list>
createdFiles=<list>

coreAlgorithmId=<id>
coreDataMaturity=DEV_ONLY_CANDIDATE
formalReadiness=NOT_LIVE_LOCKED / NOT_FORMAL_DROP_READY
specPassed/specTotal=<n>/<n>
determinismPassed/determinismTotal=<n>/<n>
baselineParityPassed/baselineParityTotal=<n>/<n>
ordinaryIdentityCoverage=<n>/30
i031OrdinaryOutputCount=0
itemInstanceRollCallOwners=<exact paths/count>
dropDomainOwners=<exact paths/count>
compileResult=<exact>
leakResult=<exact>
outOfWhitelistChanges=<n>
legacyAggregateVerdictsInherited=false
externalBlockers=<exact or NONE>
userHandtestRequired=false
gitActions=0
```

## 13. Scheduler after P0B

P0B completion does not auto-dispatch another package.

```text
P1 Chapter1 devOnly pools:
PENDING until P0B terminal acceptance and schema review.

P2 StageClear integration:
HARD HOLD until C1 unique StageClear / multi-target flow user PASS.

P3A/P3B/P4/P5:
NOT_STARTED.
```

