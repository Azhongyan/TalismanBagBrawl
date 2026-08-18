# V0.4-BLineChapter1EncounterManifest01 Assignment

```text
Package: V0.4-BLineChapter1EncounterManifest01
Workflow: COMPLEX_GUARDED_ONCE
Workflow simplification: V2
Guard mode: GUARD_FREEZE_AND_DISPATCH
Assignment status: FROZEN_FOR_SINGLE_TASK_DISPATCH
Risk: R2_ISOLATED_CHAPTER_CONTENT_BINDING_CONTRACT
Primary owner: B-Line ChapterFlow
User authorization: DIRECT_USER_DECISION_AND_BLINE_CONTINUE_C1_PHASE1_TEMPORARY_CLEAR
User hand test: NOT_REQUIRED_FOR_THIS_COMPONENT_PACKAGE
Git operations: FORBIDDEN
Next package: FORBIDDEN_TO_START
```

## 1. Objective

Create the isolated Chapter 1 encounter identity manifest that sits beside,
and does not modify, the accepted V0.4 ChapterFlow truth.

This package must:

- bind route stages `1-1` through `1-9` to only the two new V0.4 content
  identities `bone_aspect_enemy_c1_01_shattered_host` and
  `bone_aspect_enemy_c1_02_porcelain_hound`;
- keep every active normal-stage binding free of V02 EnemyDefinition,
  EnemyGroup, skill, AutoCombat, reward and drop identities;
- reserve the future `bone_aspect_enemy_c1_03_bone_swap_remnant` insertion
  point as `HELD_BY_BA-D3`, without creating or consuming its Runtime;
- validate that the accepted ChapterFlow still stops after `1-9` and requires
  a manual challenge for `1-10`;
- freeze the user's approved Phase1 temporary Chapter 1 completion policy as
  a replaceable sidecar policy, without connecting it to Battle in this
  package;
- provide pure validation, an Editor verifier and five reports.

This is not the preparation-to-Battle integration package. It must not create
or execute `BattleStartRequest`, interpret a live result, connect Enemy
Runtime, modify ChapterFlow state, or start the 1-10 Battle.

## 2. Frozen User Decision

The user has approved this exact devOnly policy:

```text
temporaryClearCondition:
  defeat Shougunu Phase1

requiredLabels:
  PHASE1_DEV_VERTICAL_SLICE
  NOT_CONTENT_FINAL
  NOT_FORMAL_BOSS_COMPLETION
```

The policy must be represented in a new immutable sidecar contract with all
three labels as exact ordinal strings.

The sidecar must keep this replacement boundary:

```text
chapterFlowTruthOwner:
  V04ChapterFlowManifest.v1

replaceableSlot:
  bline.c1.boss_completion_resolver

currentDevOnlySourceContract:
  ShougunuPhase1RuntimeAndActionContract.v1

currentDevOnlyTerminalFact:
  ShougunuPhase1LifecycleState.Defeated

futureReplacementRule:
  replace only the completion-resolver binding
  do not change chapterId, stageId, Boss gate, manual challenge,
  ChapterFlow reducer, result route or session-only unlock truth
```

This package stores and validates the policy only. It must not reference
Shougunu Runtime types, invoke the reducer, manufacture a Battle result, or
declare formal Boss completion.

## 3. Package Boundary

```text
devOnly=true
isEnabled=false
formalFlow=false
entersFormalFlow=false
activeNormalBindings=9
enemyContentIdentities=2
enemyRuntimeBindings=0
battleStartRequests=0
battleExecutorBindings=0
sceneBindings=0
uiBindings=0
rewardBindings=0
saveBindings=0
inventoryBindings=0
dropBindings=0
legacyContentBindings=0
bossRuntimeBindings=0
```

The accepted `V04ChapterFlowFeatureFlags.EnableBLineChapterFlow` stays
default `false`. This package must not modify that flag or add a second
feature flag.

## 4. Ownership and Dependencies

| Domain | Owner | This package |
| --- | --- | --- |
| Four-chapter route, 40 stages, Boss gates and reducer | Accepted B-Line ChapterFlow core | Read-only; hash protected |
| Chapter 1 stage-to-content identity selection | This package | Own the new nine-row sidecar manifest |
| 碎骨附身者 / 骨瓷犬 Runtime | Enemy owner | Not consumed yet; integration waits for its separate package |
| 换骨残相 Copy scope | User decision BA-D3 | Explicitly held; no Runtime or inferred behavior |
| 1-10 Phase1 terminal fact | Existing Enemy/Battle contracts | Stored as a string policy reference only |
| Preparation/Battle/result integration | Future B-Line integration package | Not started here |
| Scene/UI/Reward/Save/Inventory/Drop | Existing owners | No access or binding |

## 5. Read-Only Inputs

The task window must verify the exact file SHA-256 values before
implementation:

| Path | SHA-256 |
| --- | --- |
| `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs` | `5102418f6da377f1d40d5feab53c7c1414042e41411e2126f0f744d78f073715` |
| `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs` | `acb59867743de4eeced12810fa32d930f587cf195d0c1f7384734424b89bf416` |
| `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs` | `851c730347804ed6f09e84b9d4d52a53659893c3152d47166439667bea981a81` |
| `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs` | `950475adb4ba9f7d01d608d7c2377b0ee1c7226f74469165df4d68f56bff364b` |
| `Docs/V0.4/Reports/BoneAspectContentCatalog.csv` | `56bd2430adcaefb69ec2b51297f2ba2c561c63f953571e528d40080f6b6e660c` |
| `Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv` | `7d84bb15b4a878834d5f6ba2396b39c51cd7ebcbcc9f49b2b402b98db7afa3bb` |
| `Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md` | `6b6900d194f0d19f35546e5b380c0c03592c9d623a7837562ae2b040e7344fd2` |
| `Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractReport.md` | `0e15da27ea9979fac585d571a24ac45ff54dfbf64bd7fd30fd1d3b4b54840ccb` |
| `Docs/V0.4/Reports/RealItemDamageShougunuPhase1BattleSandboxVerticalSliceReport.md` | `40189b1aa91e2fa867d6b611c86aa4da8cc53adef0e8d0568edc7253c1bfe8fc` |

Any mismatch is a safety stop. Do not refresh or absorb a changed input.

## 6. Protected Aggregates

Use this exact aggregate algorithm:

```text
sort project-relative forward-slash paths ordinally
row = path + "|" + lowercase_file_sha256
payload = LF-joined rows + trailing LF
aggregate = lowercase SHA-256 of UTF-8 payload
```

Required before/after values:

```text
LOCKED:
  count=15
  aggregate=7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f

GOVERNANCE:
  count=9
  aggregate=beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2

FORMAL_V03:
  count=8
  aggregate=97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d

BLINE_CORE:
  count=14
  aggregate=04c0c1de0752c920bdde2076cced3318e0b074c21ea64b8424e066a45c1f4984

DECISION_INPUTS:
  count=5
  aggregate=bfb2ad1e9df31390a3f959fdadd18f83769718da6b20ca26f70e0536beeb6f17
```

`BLINE_CORE` includes the accepted first Assignment, seven Runtime files,
one verifier and five reports. It must remain byte-identical.

## 7. Exact Write Whitelist

### 7.1 Existing files allowed to modify

```text
NONE
```

### 7.2 New Runtime files

```text
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingPrimitives.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterManifest.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1DevBossCompletionPolicy.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingValidation.cs
```

### 7.3 New Editor verifier

```text
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/V04Chapter1EncounterManifestVerifier.cs
```

### 7.4 New reports

```text
Docs/V0.4/Reports/BLineChapter1EncounterManifestReport.md
Docs/V0.4/Reports/BLineChapter1EncounterManifest09.csv
Docs/V0.4/Reports/BLineChapter1EncounterManifestSpec.csv
Docs/V0.4/Reports/BLineChapter1EncounterManifestDecisionBoundaryReport.md
Docs/V0.4/Reports/BLineChapter1EncounterManifestLeakCheckReport.md
```

### 7.5 New Unity metadata

Only `.meta` files corresponding exactly to the new directories and C# files
in sections 7.2 and 7.3 are allowed:

```text
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/*.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/V04Chapter1EncounterManifestVerifier.cs.meta
```

No other creation or modification is allowed.

## 8. Nine-Stage Manifest

Create schema:

```text
V04Chapter1EncounterManifest.v1
```

The manifest contains exactly these active rows:

| Stage | ChapterFlow slot | Active V0.4 content identity | Purpose |
| --- | --- | --- | --- |
| `1-1` | `bline.content_binding_slot.c1.s1` | `bone_aspect_enemy_c1_01_shattered_host` | First-zone minimal encounter |
| `1-2` | `bline.content_binding_slot.c1.s2` | `bone_aspect_enemy_c1_01_shattered_host` | Repeat deterministic Runtime coverage |
| `1-3` | `bline.content_binding_slot.c1.s3` | `bone_aspect_enemy_c1_01_shattered_host` | First-zone close |
| `1-4` | `bline.content_binding_slot.c1.s4` | `bone_aspect_enemy_c1_02_porcelain_hound` | Introduce shell pressure |
| `1-5` | `bline.content_binding_slot.c1.s5` | `bone_aspect_enemy_c1_02_porcelain_hound` | Shell repeat |
| `1-6` | `bline.content_binding_slot.c1.s6` | `bone_aspect_enemy_c1_02_porcelain_hound` | Shell repeat |
| `1-7` | `bline.content_binding_slot.c1.s7` | `bone_aspect_enemy_c1_02_porcelain_hound` | Second-zone close |
| `1-8` | `bline.content_binding_slot.c1.s8` | `bone_aspect_enemy_c1_01_shattered_host` | Temporary dev substitute for held future slot |
| `1-9` | `bline.content_binding_slot.c1.s9` | `bone_aspect_enemy_c1_02_porcelain_hound` | Boss-precheck dev encounter |

This mapping is:

```text
DEV_ONLY_RUNTIME_COVERAGE
NOT_CONTENT_FINAL
NOT_BALANCE_FINAL
NOT_FORMAL_STAGE_COMPOSITION
```

It must remain replaceable by later content assignments without changing
ChapterFlow stage identity, progression, Boss gate or reducer truth.

Each row must include at least:

```text
schemaId
chapterId
stageId
chapterFlowSlotId
encounterNodeId
activeEnemyContentId
bindingPurpose
compositionMode
runtimeBindingStatus
futureReplacementSlotId
futureReplacementStatus
decisionDependency
devOnly
isEnabled
formalFlow
```

Required values:

```text
chapterId=bone_aspect_chapter_1
compositionMode=SINGLE_CONTENT_IDENTITY
runtimeBindingStatus=IDENTITY_BOUND_RUNTIME_NOT_CONNECTED
devOnly=true
isEnabled=false
formalFlow=false
```

The package binds content identity only. It must not guess the eventual Enemy
Runtime profile ID, instantiate an Enemy, or write
`BattleStartRequest.enemyProfileId`.

## 9. BA-D3 Hold

Create exactly one held future-content row:

```text
heldContentId=bone_aspect_enemy_c1_03_bone_swap_remnant
heldDisplayIdentity=换骨残相
preferredChapterFlowSlotId=bline.content_binding_slot.c1.s8
heldSlotId=bline.c1.future_content_slot.bone_swap_remnant
status=HELD_BY_BA-D3
decisionDependency=BA-D3
activeRuntimeBinding=false
activeStageBinding=false
copyScopeAuthored=false
devOnly=true
isEnabled=false
formalFlow=false
```

The active `1-8` substitute remains 碎骨附身者. The held row must not replace
that active row in this package.

Forbidden BA-D3 inference:

- copy visuals;
- copy stats;
- copy trigger behavior;
- copy Item requests;
- copy Item instances, affixes or internal implementation;
- choose a target, duration, probability or cooldown;
- create a placeholder copy Runtime.

The exact string `bone_aspect_enemy_c1_03_bone_swap_remnant` may appear only
in the held row, boundary diagnostics, verifier and reports. It must never
appear as `activeEnemyContentId`.

## 10. Phase1 Temporary Boss Completion Sidecar

Create immutable schema:

```text
V04Chapter1DevBossCompletionPolicy.v1
```

It must expose exact facts:

```text
chapterId=bone_aspect_chapter_1
stageId=1-10
bossProfileId=bone_aspect_boss_c1_bone_guard
completionResolverSlotId=bline.c1.boss_completion_resolver
currentSourceContractId=ShougunuPhase1RuntimeAndActionContract.v1
currentTerminalFactId=ShougunuPhase1LifecycleState.Defeated
temporaryClearAllowed=true
chapterFlowTruthMustRemainUnchanged=true
replacementMode=REPLACE_COMPLETION_RESOLVER_ONLY
labelA=PHASE1_DEV_VERTICAL_SLICE
labelB=NOT_CONTENT_FINAL
labelC=NOT_FORMAL_BOSS_COMPLETION
devOnly=true
isEnabled=false
formalFlow=false
```

The policy must not:

- reference a Shougunu Runtime type;
- read HP or damage;
- call Battle Application;
- create `BattleResultSnapshot`;
- set `bossDefeated`;
- unlock Chapter 2;
- grant Reward or Drop;
- modify `V04ChapterFlowManifest`, reducer, snapshots or projection.

Those operations belong to the future integration package.

## 11. ChapterFlow Parity Checks

The verifier must read the accepted core and assert:

```text
V04ChapterFlowManifest.SchemaId=V04ChapterFlowManifest.v1
Chapter1 stage count=10
normal binding slots=9
1-9.stopBeforeBossAfterWin=true
1-9.isBossStage=false
1-10.isBossStage=true
1-10.requiresManualBossChallenge=true
1-10.bossProfileId=bone_aspect_boss_c1_bone_guard
1-10.contentBindingSlotId=empty
automatic Boss start remains impossible in the accepted core
```

The new manifest must join one-to-one to the nine existing Chapter 1 normal
stage slots. Missing, duplicate, unknown or Boss-stage joins fail.

The accepted core must remain byte-identical; do not patch it to add the
bindings.

## 12. Forbidden Dependencies

Runtime source must contain zero references to:

```text
TalismanBag.Enemies
EnemyDefinition
EnemyGroupConfig
EnemyRuntime
EnemySkillController
ShougunuPhase1RuntimeSnapshot
ShougunuPhase1RuntimeReducer
ShougunuPhase1BattleApplicationEngine
BattleStartRequest
BattleResultSnapshot
AutoCombatController
V02RunFlowController
V02RunConfig
RewardService
RewardConfig
RewardDropTable
SaveData
SaveService
Inventory
ItemDropGeneration
ItemInstanceRollEngine
SceneManager
MonoBehaviour
ScriptableObject
Resources.Load
GameObject
Transform
PlayerPrefs
System.Random
UnityEngine.Random
Guid.NewGuid
DateTime.Now
DateTime.UtcNow
Environment.TickCount
UnityEditor
AssetDatabase
```

The Editor verifier may use `UnityEditor` only for static verification and
report generation. It must not edit or save Scene, Prefab, BuildSettings,
ScriptableObject or any existing file.

## 13. Verification

Create exact Editor entry:

```text
Menu:
Tools/TalismanBag/V0.4/Verify B-Line Chapter 1 Encounter Manifest 01

Batch execute method:
TalismanBag.Editor.V04.ChapterFlow.Chapter1.V04Chapter1EncounterManifestVerifier.RunBatch
```

Required checks:

```text
chapters=1
activeStageBindings=9
shatteredHostBindings=4
porcelainHoundBindings=5
heldByBAD3=1
activeBoneSwapRemnantBindings=0
bossBindings=0
legacyContentRefs=0
enemyRuntimeBindings=0
battleStartRequests=0
sceneBindings=0
uiBindings=0
rewardBindings=0
saveBindings=0
inventoryBindings=0
dropBindings=0
phase1TemporaryClearPolicies=1
requiredPhase1Labels=3
chapterFlowCoreDrift=0
protectedHashDrift=0
outOfWhitelistFiles=0
existingFileModifications=0
```

The verifier must include negative fixtures for:

- duplicate stage binding;
- unknown Stage ID;
- binding `1-10` as a normal encounter;
- missing or mismatched ChapterFlow slot ID;
- old V02 content ID;
- active 换骨残相;
- missing `HELD_BY_BA-D3`;
- inferred BA-D3 Copy behavior;
- missing or misspelled Phase1 label;
- formal/enabled policy;
- completion policy that owns ChapterFlow truth;
- completion policy that directly invokes Runtime or Battle.

Static and Unity batch QA may close automatically. Before Unity batch, confirm
that no interactive UnityLockfile exists. Do not require a component-level
user hand test.

## 14. Required Reports

### `BLineChapter1EncounterManifestReport.md`

Include package, Assignment SHA, schemas, exact counts, mapping summary,
Phase1 decision labels, BA-D3 hold, QA, protected aggregates and final marker.

### `BLineChapter1EncounterManifest09.csv`

Exactly nine active rows plus header, matching section 8 exactly.

### `BLineChapter1EncounterManifestSpec.csv`

One row per normative assertion:

```text
assertionId,category,expected,actual,result
```

### `BLineChapter1EncounterManifestDecisionBoundaryReport.md`

Must separately record:

- the approved Phase1 temporary completion policy and all three labels;
- replace-only completion resolver boundary;
- BA-D3 held content row;
- no decision taken for BA-D3, BA-D4, FALSE_CLONE, Drop, Tutorial or Story;
- `1-9` stop and manual `1-10` gate remain ChapterFlow-owned.

### `BLineChapter1EncounterManifestLeakCheckReport.md`

Include forbidden dependency counts, exact whitelist delta, protected
before/after aggregates and zero existing-file modifications.

## 15. Acceptance Marker

Successful console/report marker:

```text
BLINE_CHAPTER1_ENCOUNTER_MANIFEST01_PASS stages=9 shatteredHost=4 porcelainHound=5 heldByBAD3=1 phase1TemporaryClearPolicy=1 legacyContentRefs=0 existingFileModifications=0
```

Failure marker:

```text
BLINE_CHAPTER1_ENCOUNTER_MANIFEST01_FAIL errors=<positive integer>
```

The component package closes only when:

- all new code compiles;
- Editor verifier PASS is recorded;
- all five reports exist and agree;
- exact nine-row mapping matches;
- Phase1 policy and all three exact labels pass;
- BA-D3 held row passes and active 换骨残相 count is zero;
- `1-9`/`1-10` parity checks pass;
- accepted ChapterFlow core and all protected aggregates are unchanged;
- no existing file changed;
- no file outside the whitelist exists;
- no Git operation occurred;
- no second package started.

## 16. Task Operating Rules

1. Complete the mandatory workspace/project AGENTS and LOCKED startup read.
2. Verify this Assignment's full SHA-256 against the dispatch prompt before
   implementation.
3. Verify all input hashes and protected aggregates.
4. Work uninterrupted inside the exact whitelist.
5. Stop only for a real hash mismatch, unexpected existing target, occupied
   write domain, interactive Unity lock blocking QA, or unavoidable
   out-of-whitelist dependency.
6. Do not contact or reuse Item, Enemy, Visual or Battle development tasks.
7. Do not wait for the Enemy Runtime package; this package binds content
   identity only and may complete independently.
8. Do not start preparation/Battle/result integration.
9. Do not modify LOCKED, Queue, legacy formal 1-10/2-10, or Git.
10. Send one final completion report after automated PASS; no fragmented
    status receipts.

## 17. Guard Freeze Receipt

```text
Assignment path:
Docs/V0.4/BLineChapter1EncounterManifest01_Assignment.md

Full-file SHA-256:
SUPPLIED_OUT_OF_BAND_BY_GUARD_DISPATCH

Existing-file modification whitelist:
EMPTY

New-file whitelist:
FROZEN_BY_SECTION_7

Protected aggregates:
LOCKED=7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f
GOVERNANCE=beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2
FORMAL_V03=97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d
BLINE_CORE=04c0c1de0752c920bdde2076cced3318e0b074c21ea64b8424e066a45c1f4984
DECISION_INPUTS=bfb2ad1e9df31390a3f959fdadd18f83769718da6b20ca26f70e0536beeb6f17
```

