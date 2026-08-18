# V0.4-DropRewardImmutableContracts01 Assignment

## 0. Assignment Identity

- Package: `V0.4-DropRewardImmutableContracts01`
- Phase: `P0A`
- Dispatch mode: `COMPLEX_GUARDED_ONCE / COMPONENT_PACKAGE`
- Overall authorization:
  `OVERALL_GUARD_CONFIRM_AND_AUTHORIZE_P0A_DROP_REWARD_IMMUTABLE_CONTRACTS01`
- Product-context addendum:
  `DROP_GUARD_PRODUCT_CONTEXT_ADDENDUM_LV40_SHOWCASE_VS_CAMPAIGN_LV1`
- Guard owner: `V0.4 Reward-Drop Guard`
- Development owner: one new, independent Drop contracts development task
- Risk: `YELLOW / CONTRACT BOUNDARY FREEZE`
- User hand test: `NOT_REQUIRED`
- Git: `FORBIDDEN`
- Expected terminal marker: `DROP_REWARD_IMMUTABLE_CONTRACTS01_PASS`

This Assignment is the one-time boundary freeze for P0A. The development task
must verify the Assignment SHA supplied in its dispatch message before editing
and again before returning. It must not edit this Assignment.

## 1. Baseline and Collision Result

The Guard completed the pre-dispatch read-only check on 2026-07-30:

- Unity root markers `Assets / ProjectSettings / Packages`: present.
- Existing `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop`: absent.
- Existing `Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop`: absent.
- Existing P0A reports: absent.
- Existing RewardDrop namespace or P0A package marker: absent.
- Active `V0.4 BattleSandbox Clean Play` work is in BattleSandbox /
  presentation cleanup paths and does not overlap this whitelist.
- Item tray, C1 / 1-10 Flow, Enemy, Visual / VFX, Save, UI and WorldMap paths
  are outside this package.

If any listed path exists or any write collision appears after dispatch, the
development task must stop before editing and return
`P0A_WRITE_COLLISION_BLOCKED`.

## 2. Package Goal

Create the immutable, deterministic and canonically validated boundary
contracts for the future V0.4 Reward-Drop pipeline:

```text
BattleResult
-> accepted StageClearFact
-> DropRequest
-> DropRollResult
-> RewardResult
-> RewardClaimIdentity / RewardDedupeIdentity
```

P0A defines data ownership and correlation only. It does not roll an Item,
read a pool, bind StageFlow, grant a reward, write Inventory / Save, or render
UI.

The same Enemy, Item art or stage label may appear in different launch
contexts. Product eligibility must be decided by an explicit immutable launch
context, never by Scene name, current stage label, enemy identity, Item level
or visual content.

## 3. Exact Write Whitelist

Only the following new files may be created.

### Runtime contracts

```text
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractPrimitives.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/StageClearFact.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/StageClearFact.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/DropRequest.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/DropRequest.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/DropRollResult.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/DropRollResult.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardResult.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardResult.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardClaimAndDedupeIdentities.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardClaimAndDedupeIdentities.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropCanonical.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropCanonical.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractValidator.cs
Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Contracts/RewardDropContractValidator.cs.meta
```

### Dedicated verifier

```text
Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop/DropRewardImmutableContracts01Verifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/V04/RewardDrop/DropRewardImmutableContracts01Verifier.cs.meta
```

### Reports

```text
Docs/V0.4/Reports/DropRewardImmutableContracts01Report.md
Docs/V0.4/Reports/DropRewardImmutableContracts01Spec.csv
Docs/V0.4/Reports/DropRewardImmutableContracts01CanonicalDeterminism.csv
Docs/V0.4/Reports/DropRewardImmutableContracts01LeakCheckReport.md
```

No other tracked file may be created or modified. Verification logs under
`Logs/` are transient evidence only and are not package deliverables.

## 4. Exact Forbidden Scope

P0A must not create, modify, bind, enable or invoke:

- Item generation, Item roll, rarity roll, affix roll or Item projection;
- `TalismanBag.Items.Generation.*` source files;
- ChapterFlow, StageFlow, BattleResult mapper or C1 / 1-10 integration;
- drop-pool, probability, weight, pity, first-clear content or Chapter1 data;
- Scene, Prefab, Resources, ScriptableObject asset or Build Settings;
- WorldMap catalog, drawer, stage node or drop preview;
- Inventory, SaveData, SaveService, PlayerPrefs or migration;
- RewardService, ResourceService or any grant / claim commit service;
- runtime MonoBehaviour, service locator, singleton, event bus or UI;
- APK / formal-flow feature binding or default-enabled feature flag;
- V0.2 / V0.3 runtime and data;
- AGENTS, `Docs/LOCKED/*`, ROADMAP, CURRENT, any Package Queue;
- Git commit, tag, branch, push, reset, checkout or staging.

P0B and P1-P5 remain outside this Assignment.

## 5. Required Namespace and Runtime Qualities

- Namespace: `TalismanBag.V04.RewardDrop.Contracts`
- Runtime contracts must be pure C# and must not depend on `UnityEngine` or
  `UnityEditor`.
- Public contract state must be constructor-only and read-only.
- No public mutable fields, setters, exposed mutable arrays or exposed mutable
  lists.
- Incoming collections must be defensively copied and exposed as
  `IReadOnlyList<T>`.
- Null collection inputs must become empty immutable collections.
- Identifiers must be trimmed with ordinal comparison semantics.
- Contract validation must fail closed and return immutable validation errors.
- Runtime constructors must not perform random generation, current-time reads,
  GUID generation, file reads, Scene reads, Save reads or global-state reads.

Forbidden nondeterministic APIs include:

```text
UnityEngine.Random
System.Random
Guid.NewGuid
DateTime.Now
DateTime.UtcNow
Environment.TickCount
string.GetHashCode
```

## 6. Frozen Contract Vocabulary

### 6.1 Acquisition Mode

Define:

```text
RewardAcquisitionMode
- Unspecified = 0
- FirstClearOnline = 1
- RepeatChallengeOnline = 2
- OfflinePatrol = 3
```

`Unspecified` is always invalid in a successful request or result.

The three valid modes must remain distinct in every request, roll result,
reward result, claim identity and canonical payload. They must never collapse
into a single `StageClear` boolean.

### 6.2 Launch Context and Persistence Policy

Define:

```text
RewardLaunchContextKind
- Unspecified = 0
- CampaignNormal = 1
- DevShowcase = 2

RewardPersistencePolicy
- Unspecified = 0
- ProductClaimable = 1
- DiagnosticsOnly = 2
```

Define immutable `RewardLaunchContextIdentity` with at least:

```text
launchContextId
launchContextKind
persistencePolicy
contextVersion
```

Freeze these two initial context identities:

```text
CAMPAIGN_NORMAL_LV1
  kind = CampaignNormal
  persistencePolicy = ProductClaimable

DEV_SHOWCASE_LV40
  kind = DevShowcase
  persistencePolicy = DiagnosticsOnly
```

Rules:

- `CAMPAIGN_NORMAL_LV1` is the only P0A context eligible to validate a product
  DropRequest / DropRollResult / RewardResult / Claim identity.
- `DEV_SHOWCASE_LV40` may produce diagnostics outside this package, but must
  hard reject product DropRequest, RewardResult, Reward claim, Inventory /
  Save commit identity and campaign progression identity.
- P0A contains no diagnostics reward implementation and must never create a
  fake reward for a DiagnosticsOnly context.
- Context kind and persistence policy must be validated as a pair; a
  `DevShowcase + ProductClaimable` or `CampaignNormal + DiagnosticsOnly`
  combination is invalid for the frozen identities.
- Context identity must propagate unchanged through StageClearFact,
  DropRequest, DropRollResult, RewardResult, RewardClaimIdentity and
  RewardDedupeIdentity.
- Scene names, stage labels, enemy IDs, current level, Item rarity and visual
  content are forbidden as context inference inputs.

### 6.3 Source Fact Kind

Define:

```text
RewardSourceFactKind
- Unspecified = 0
- StageClear = 1
- OfflinePatrolSettlement = 2
```

Rules:

- `FirstClearOnline` and `RepeatChallengeOnline` require
  `RewardSourceFactKind.StageClear` and a non-empty `stageClearId`.
- `OfflinePatrol` reserves
  `RewardSourceFactKind.OfflinePatrolSettlement`, requires a non-empty
  `sourceFactId`, and must not pretend that an offline settlement is a battle
  StageClear.
- P0A does not define or implement the future offline settlement payload.

### 6.4 Reward Subject Kind

Define explicit classification:

```text
RewardSubjectKind
- Unspecified = 0
- OrdinaryGeneratedItem = 1
- OrdinaryResource = 2
- SystemRhythmSpecial = 3
- StoryCritical = 4
- CosmeticSkin = 5
- BossExclusive = 6
```

Define immutable `RewardSubjectIdentity` with at least:

```text
subjectId
subjectKind
sourceCatalogId
classificationVersion
```

Classification is authoritative input from the future Item / Reward owner.
Runtime contract code must never infer classification from `subjectId`,
prefixes, suffixes, display names or the literal item ID `I031`.

### 6.5 Reward Grant Kind

Define:

```text
RewardGrantKind
- Unspecified = 0
- OrdinaryRoll = 1
- FirstClearFixed = 2
- BossFirstClearGuaranteed = 3
- SystemGrant = 4
- StoryGrant = 5
- CosmeticGrant = 6
```

Minimum validation:

- `OrdinaryRoll` accepts only `OrdinaryGeneratedItem` or
  `OrdinaryResource`.
- `FirstClearFixed` accepts only `FirstClearOnline`.
- `BossFirstClearGuaranteed` accepts only `FirstClearOnline` and requires the
  correlated `StageClearFact.isBossStage = true`.
- `SystemGrant`, `StoryGrant` and `CosmeticGrant` are never legal in
  `RepeatChallengeOnline` or `OfflinePatrol`.
- `SystemRhythmSpecial`, `StoryCritical`, `CosmeticSkin` and `BossExclusive`
  are never legal as `OrdinaryRoll`.
- Special exclusions are based on the two explicit enums above, never on
  identity strings.

## 7. Frozen Contracts

All contracts require a constant schema ID and a canonical payload/signature.

### 7.1 `StageClearFact`

Required immutable fields:

```text
schemaId
stageClearId
battleResultId
battleRequestId
resultFingerprint
chapterId
stageId
runSessionId
clearSequence
isBossStage
RewardLaunchContextIdentity
```

Rules:

- all string identities are non-empty after normalization;
- `clearSequence > 0`;
- `stageClearId` is the dedupe source for one accepted clear boundary;
- it contains no pool, probability, reward, Inventory or Save payload.

### 7.2 `DropRequest`

Required immutable fields:

```text
schemaId
dropRequestId
sourceFactKind
sourceFactId
stageClearId
chapterId
stageId
RewardLaunchContextIdentity
acquisitionMode
rootSeed
generationVersion
poolId
poolVersion
rarityTierProfileId
attributeRollBandProfileId
rollPolicyVersion
ordered rollSlots
```

Each immutable roll slot requires:

```text
rollSlotId
grantKind
```

Rules:

- `generationVersion > 0`;
- `poolId`, `poolVersion`, `rarityTierProfileId`,
  `attributeRollBandProfileId`, `rollPolicyVersion` and roll slots are
  references only; P0A contains no pool, rarity or attribute-band content;
- future P1 `CAMPAIGN_NORMAL_LV1` data may request lowest early-game rarity
  and a low attribute roll band through these identities; P0A must not
  implement, duplicate or infer Item roll behavior;
- roll-slot IDs are non-empty and unique;
- P0A roll slots accept only `RewardGrantKind.OrdinaryRoll`;
- online modes must correlate to one supplied valid `StageClearFact`;
- request correlation must match `stageClearId/chapterId/stageId`;
- launch context and persistence policy must match the StageClearFact;
- `DEV_SHOWCASE_LV40` and every `DiagnosticsOnly` context fail before a
  product DropRequest can be accepted;
- offline mode follows the reserved source-fact rule and never grants special
  categories.

### 7.3 `DropRollResult`

Required immutable fields:

```text
schemaId
dropRollResultId
dropRequestId
sourceFactId
stageClearId
chapterId
stageId
RewardLaunchContextIdentity
acquisitionMode
rootSeed
generationVersion
algorithmId
poolId
poolVersion
rarityTierProfileId
attributeRollBandProfileId
rollPolicyVersion
ordered entries
```

Each immutable entry requires:

```text
rollEntryId
rollSlotId
grantKind
RewardSubjectIdentity
itemInstanceId
instanceCanonicalSignature
quantity
```

Rules:

- P0A creates fixtures only; it does not roll an Item;
- every entry must correlate to exactly one request roll slot;
- entry IDs and roll-slot consumption are unique;
- `quantity > 0`;
- an `OrdinaryGeneratedItem` requires non-empty `itemInstanceId` and
  `instanceCanonicalSignature`;
- an `OrdinaryResource` must not forge an Item instance;
- special subject kinds or special grant kinds fail ordinary Drop validation;
- result request, source, stage, mode, seed, version and pool fields must match
  the request exactly.
- result launch context, persistence policy, tier profile, attribute band and
  roll-policy version must match the request exactly;
- no valid DropRollResult may be produced for `DEV_SHOWCASE_LV40`.

### 7.4 `RewardResult`

Required immutable fields:

```text
schemaId
rewardResultId
sourceFactId
stageClearId
dropRequestId
dropRollResultId
chapterId
stageId
RewardLaunchContextIdentity
acquisitionMode
ordered rewardEntries
```

Each immutable reward entry requires:

```text
rewardEntryId
grantKind
RewardSubjectIdentity
sourceDefinitionId
sourceRuleVersion
itemInstanceId
instanceCanonicalSignature
quantity
```

Rules:

- ordinary rolled entries must preserve their DropRoll identity and canonical
  Item reference without reroll;
- fixed / special entries must be explicitly classified and must obey their
  acquisition-mode restrictions;
- no entry performs grant, Inventory mutation or Save;
- reward-entry IDs and Item instance IDs are unique where applicable;
- correlation to source, stage, request and roll result must be validated.
- `DEV_SHOWCASE_LV40` and every `DiagnosticsOnly` context must fail
  RewardResult validation and cannot reach a Claim identity.

### 7.5 Claim, Dedupe and Exclusion Identities

Define immutable:

```text
RewardClaimIdentity
RewardDedupeIdentity
RewardExclusionIdentity
```

Minimum required fields:

```text
RewardClaimIdentity:
  claimId
  rewardResultId
  sourceFactId
  stageClearId
  dropRequestId
  acquisitionMode
  claimScopeId
  RewardLaunchContextIdentity

RewardDedupeIdentity:
  dedupeKey
  sourceFactId
  stageClearId
  dropRequestId
  rewardResultId
  acquisitionMode
  claimScopeId
  RewardLaunchContextIdentity

RewardExclusionIdentity:
  exclusionPolicyId
  subjectKind
  prohibitedAcquisitionModes
  reasonCode
  policyVersion
```

These contracts describe identities only. They do not query or write a ledger.
No valid Claim or Dedupe identity may describe a DiagnosticsOnly context.

## 8. Canonicalization Freeze

Provide one shared canonical implementation.

- Canonical payload encoding: UTF-8, explicit ordered field keys,
  invariant-culture primitives, length-prefixed string values and explicit
  schema IDs.
- Canonical signature: lowercase SHA-256 hex of the canonical payload.
- No `ToString()`-only object serialization, JSON field-order dependency,
  locale dependency or platform newline dependency.
- Ordered semantic lists such as roll slots and result entries preserve their
  order and include a zero-based position in the payload.
- Set-like diagnostic / exclusion collections must be normalized and sorted
  with `StringComparer.Ordinal` before encoding.
- Same semantic input must produce byte-identical payload and signature.
- A change to any identity, mode, category, correlation, seed, generation
  version, pool version, quantity, Item instance identity or source canonical
  signature must change the enclosing signature.
- A change to launch context, persistence policy, rarity-tier profile,
  attribute-roll-band profile or roll-policy version must also change the
  enclosing signature.

## 9. Required Verifier Fixtures

The dedicated verifier must generate all four reports and cover at least:

### Positive

- valid ordinary first-clear online pipeline;
- valid ordinary repeat-online pipeline;
- reserved ordinary offline request/result identity without any StageClear
  forgery or special grant;
- valid Boss first-clear guaranteed RewardResult outside ordinary Roll;
- valid `CAMPAIGN_NORMAL_LV1` StageClear-to-Claim correlation;
- empty immutable collection normalization where empty is legal;
- defensive-copy proof for every incoming collection;
- same-input canonical payload/signature repeat, minimum 1,000 iterations;
- ordinal and invariant-culture proof;
- correlation round trip:
  `StageClear -> DropRequest -> DropRollResult -> RewardResult -> Claim`.

### Negative

- null / empty required identities;
- unsupported enum value and `Unspecified` mode/kind;
- zero or negative `clearSequence`, generation version or quantity;
- duplicate roll slot, roll entry, reward entry and Item instance identity;
- request/result source, stage, mode, seed, generation or pool mismatch;
- request/result launch-context, persistence-policy, rarity-tier,
  attribute-roll-band or roll-policy mismatch;
- result entry not present in request roll slots;
- reused roll slot;
- ordinary Item missing instance identity or canonical signature;
- resource forging an Item instance;
- first-clear grant on repeat or offline;
- Boss first-clear grant on non-Boss StageClear;
- Boss exclusive, system rhythm, story-critical or cosmetic subject entering
  ordinary Roll;
- special grant entering repeat or offline;
- I031 fixture classified explicitly as `SystemRhythmSpecial` rejected from
  ordinary Roll;
- a non-I031 fixture with the same `SystemRhythmSpecial` classification must
  fail identically, proving category-based exclusion;
- public mutation surface scan;
- forbidden nondeterministic API scan;
- runtime literal / prefix / suffix inference scan for I031;
- `DEV_SHOWCASE_LV40` product DropRequest hard reject;
- `DEV_SHOWCASE_LV40` RewardResult and Claim identity hard reject;
- invalid `DevShowcase + ProductClaimable` hard reject;
- invalid `CampaignNormal + DiagnosticsOnly` hard reject;
- same stage identity fixture under both frozen contexts: campaign request
  validates while showcase product grants remain zero; runtime contracts
  contain no Enemy or visual-content field from which eligibility could be
  inferred;
- Scene/UI/Inventory/Save/WorldMap/Item-roll/formal-flow dependency scan.

The verifier may use the literal `I031` only in the dedicated negative fixture
and report evidence. Runtime contract files must contain zero `I031` literals.

## 10. Reports and Acceptance

### Main report

Must state:

- package, schema IDs and terminal result;
- contract and fixture counts;
- immutable/public-mutation scan result;
- canonical repeat and delta results;
- category-based special exclusion result;
- campaign-vs-showcase context rejection result;
- exact files created;
- no formal integration and no user hand test.

### Spec CSV

One row per positive / negative assertion with:

```text
caseId,area,expected,actual,diagnosticCode,result
```

### Canonical determinism CSV

Must include:

```text
caseId,repeatCount,baselineSignature,repeatSignature,
changedField,changedSignature,result
```

### Leak check

Must prove zero:

```text
Item roll implementation
ChapterFlow / StageFlow binding
pool / probability / pity data
Scene / Prefab / Resources / BuildSettings
WorldMap / UI
Inventory / Save / PlayerPrefs
RewardService / grant / commit
V0.2 / V0.3 mutation
formal-flow / APK binding
runtime I031 string inference
DEV_SHOWCASE product grant / claim eligibility
Scene name / stage label / enemy identity context inference
Git action
out-of-whitelist tracked files
```

### Automatic PASS gate

The package may self-close only when:

1. Assignment SHA matches before and after implementation.
2. Every whitelist file exists and no out-of-whitelist tracked file was
   changed by this task.
3. Runtime contracts compile.
4. All positive and expected-negative fixtures pass.
5. Canonical 1,000-repeat determinism passes.
6. Mutation, correlation, duplicate, special-exclusion and
   campaign-vs-showcase fixtures pass.
7. Leak check is zero.
8. Main report terminal marker is exactly:

```text
DROP_REWARD_IMMUTABLE_CONTRACTS01_PASS
```

If an unrelated current workspace change blocks a broad verifier, report
`PACKAGE_SCOPE_PASS / EXTERNAL_BLOCKER_RECORDED` only when the P0A scoped
compile, fixtures, whitelist and leak checks all pass. Do not edit unrelated
files to turn a global report green.

## 11. Return Contract

Return one terminal implementation report to the Reward-Drop Guard:

```text
package
assignmentSha256Before
assignmentSha256After
terminalStatus
terminalMarker
createdFiles
contractSchemas
specPassed/specTotal
canonicalRepeatPassed/canonicalRepeatTotal
negativeFixturesPassed/negativeFixturesTotal
compileResult
leakResult
outOfWhitelistChanges
externalBlockers
userHandtestRequired=false
gitActions=0
```

Do not send routine progress updates while active. Notify the Guard only for a
verified write collision, safety issue or terminal result.
