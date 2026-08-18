# V0.4-BLineChapterFlowContract01 Assignment

```text
Package: V0.4-BLineChapterFlowContract01
Workflow: COMPLEX_GUARDED_ONCE
Guard mode: GUARD_FREEZE_AND_DISPATCH
Assignment status: FROZEN_FOR_SINGLE_TASK_DISPATCH
Risk: R2_CROSS_SYSTEM_CONTRACT_ISOLATED
Primary owner: B-Line ChapterFlow
User authorization: DIRECT_USER_AUTHORIZATION_BLINE_FLOW_START
User hand test: NOT_REQUIRED_FOR_THIS_PURE_CONTRACT_PACKAGE
Git operations: FORBIDDEN
Next package: FORBIDDEN_TO_START
```

## 1. Objective

Create the first isolated V0.4 B-line ChapterFlow contract package.

The package must implement:

- four chapters;
- exactly forty route stage nodes: `1-1` through `4-10`;
- exactly four Boss gates, each reached after the corresponding `x-9` win;
- manual-only Boss challenge for `1-10`, `2-10`, `3-10`, and `4-10`;
- deterministic pure state reduction;
- read-only `BattleStartRequest` projection;
- read-only `BattleResultSnapshot` interpretation;
- session-only chapter unlock state;
- stable tutorial and story hook IDs;
- static validation, Editor verifier, and generated reports.

The package is contract-only. It must not connect Scene, UI, Reward, Save,
Inventory, Enemy Runtime, Battle Executor, formal RunFlow, or product entry.

## 2. Frozen Boundary

```text
devOnly=true
isEnabled=false
formalFlow=false
entersFormalFlow=false
shouldWriteSave=false
shouldGrantReward=false
sceneBindings=0
uiBindings=0
rewardBindings=0
saveBindings=0
inventoryBindings=0
enemyRuntimeBindings=0
battleExecutorBindings=0
legacyContentBindings=0
```

`V04ChapterFlowFeatureFlags.EnableBLineChapterFlow` must exist in the new
package and default to `false`. No existing feature-flag class may be edited.

The implementation must use plain C# contracts and pure logic. Runtime files
must not inherit from `MonoBehaviour` or `ScriptableObject`, must not load
Resources or Scenes, and must not discover runtime objects.

## 3. Source-of-Truth Ownership

| Domain | Owner | This package may do |
| --- | --- | --- |
| Chapter route state and stage order | B-Line ChapterFlow | Own new devOnly snapshots, manifest, reducer and validation |
| Battle request/result schemas | Battle contract owner | Read existing schemas and project/interpret them without edits |
| Enemy and Boss runtime | Enemy owner | Store stable route-facing content/profile IDs only; never instantiate or execute |
| Item and drop generation | Item owner | Keep drop hook IDs only; never roll or grant |
| Reward, Inventory and Save | Their existing owners | No reference, call, write or adapter |
| Scene and UI presentation | Visual/UI owner | No binding or runtime discovery |
| Formal V0.3 1-10/2-10 | Protected stable baseline | Read-only and zero references from new runtime |

## 4. Read-Only Inputs and Frozen Hashes

The task window must compute SHA-256 before development and require exact
matches for these four read-only inputs:

| Path | SHA-256 |
| --- | --- |
| `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleStartRequest.cs` | `d207a8d3fa4f80b73356e840d794c2ba00842a7ec91fc2b4dd2fb9e6de2d0c79` |
| `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleResultSnapshot.cs` | `ddad9bf73e8821f027a5af14816368ad2d44a30468c8bc7ccf0888b2fa9bca8e` |
| `Docs/V0.4/Reports/BoneAspectContentCatalog.csv` | `56bd2430adcaefb69ec2b51297f2ba2c561c63f953571e528d40080f6b6e660c` |
| `Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv` | `7d84bb15b4a878834d5f6ba2396b39c51cd7ebcbcc9f49b2b402b98db7afa3bb` |

Read-input aggregate:

```text
algorithm:
  sort project-relative forward-slash paths ordinally
  row = path + "|" + lowercase_file_sha256
  payload = LF-joined rows + trailing LF
  aggregate = lowercase SHA-256 of UTF-8 payload

count=4
aggregate=d7f13a98a439ab7d1a201bc02bf159241a6656691be7e2e880190080ab342683
```

Any mismatch is a safety stop. The task window must report the exact path,
expected hash, and actual hash. It must not refresh a baseline.

## 5. Protected Aggregates

The following aggregates must match before and after implementation.

### 5.1 LOCKED

```text
scope: all 15 files directly under Docs/LOCKED
count=15
aggregate=7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f
```

### 5.2 Governance and queues

```text
files:
  AGENTS.md
  Docs/ROADMAP/VERSION_ROADMAP.md
  Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
  Docs/V0.3/V0.3_PACKAGE_QUEUE.md
  Docs/V0.4/BUILD_PACKAGE_QUEUE.md
  Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
  Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
  Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
  Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md

count=9
aggregate=beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2
```

### 5.3 Formal V0.3 1-10/2-10 baseline

```text
files:
  Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity
  Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset
  Assets/_Game/Scripts/TalismanBag/V02/Run/V02RunFlowController.cs
  Assets/_Game/Scripts/TalismanBag/Combat/AutoCombatController.cs
  Assets/_Game/Resources/CoreLoop/Rewards/chapter_1_10_clear.asset
  Assets/_Game/Resources/CoreLoop/Rewards/boss_2_10_clear.asset
  Assets/_Game/Resources/CoreLoop/DropTables/chapter_2_normal_round_drops.asset
  ProjectSettings/EditorBuildSettings.asset

count=8
aggregate=97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d
```

The aggregate algorithm is exactly the algorithm in section 4.

## 6. Exact Write Whitelist

### 6.1 Existing files allowed to modify

```text
NONE
```

No existing file may change.

### 6.2 New runtime files

```text
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowPrimitives.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowReducer.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowValidation.cs
```

### 6.3 New Editor verifier

```text
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/V04ChapterFlowContractVerifier.cs
```

### 6.4 New generated reports

```text
Docs/V0.4/Reports/BLineChapterFlowContractReport.md
Docs/V0.4/Reports/BLineChapterFlowContractSpec.csv
Docs/V0.4/Reports/BLineChapterFlowManifest40.csv
Docs/V0.4/Reports/BLineChapterFlowTransitionMatrix.csv
Docs/V0.4/Reports/BLineChapterFlowLeakCheckReport.md
```

### 6.5 Unity metadata

Only Unity-generated `.meta` files corresponding exactly to the new
directories and files in sections 6.2 and 6.3 are allowed:

```text
Assets/_Game/Scripts/TalismanBag/V04.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/*.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/V04ChapterFlowContractVerifier.cs.meta
```

No other file or metadata creation is allowed.

## 7. Required Contract

### 7.1 Stable identity

Use these exact chapter IDs:

```text
bone_aspect_chapter_1
bone_aspect_chapter_2
bone_aspect_chapter_3
bone_aspect_chapter_4
```

Use these exact Boss profile IDs:

```text
bone_aspect_boss_c1_bone_guard
bone_aspect_boss_c2_identity_bidder
bone_aspect_boss_c3_array_eye_guardian
bone_aspect_boss_c4_myriad_bone_beast
```

Use the twelve exact encounter node IDs and stage ranges from
`BoneAspectEncounterNodeCatalog.csv`.

The manifest must expand those ranges to exactly forty route stages:

```text
1-1 ... 1-10
2-1 ... 2-10
3-1 ... 3-10
4-1 ... 4-10
```

Normal stage runtime content must remain unbound in this package. A normal
stage may expose a stable `contentBindingSlotId`, but it must not choose or
instantiate an Enemy profile.

Boss stages may expose the locked Boss profile ID as route identity only.
This is not an Enemy Runtime binding.

### 7.2 Stage and chapter facts

Each stage row must include at least:

```text
chapterId
chapterIndex
stageId
stageIndex
encounterNodeId
isBossStage
stopBeforeBossAfterWin
requiresManualBossChallenge
contentBindingSlotId
bossProfileId
bossResolutionMode
beforeStageTutorialHookId
beforeStageStoryHookId
afterResultTutorialHookId
afterResultStoryHookId
beforeBossTutorialHookId
beforeBossStoryHookId
dropCandidateHookId
devOnly
isEnabled
formalFlow
```

Required invariants:

- only `x-10` is a Boss stage;
- only `x-9` has `stopBeforeBossAfterWin=true`;
- all four `x-10` rows have `requiresManualBossChallenge=true`;
- automatic Boss start count is zero;
- all hook IDs are stable, non-empty where applicable, ordinally
  deterministic, and contain no localized text;
- all rows have `devOnly=true`, `isEnabled=false`, and `formalFlow=false`;
- Chapter 3 Boss resolution mode represents `YIELD_AND_ALLOW_PASSAGE`;
- the other three Boss resolution modes represent route completion without
  defining Enemy damage, HP, death animation, reward, or save semantics.

### 7.3 State snapshot

Create `V04ChapterFlowStateSnapshot.v1` with explicit state sufficient to
validate:

```text
sessionId supplied by caller
currentChapterId
currentStageId
phase
activeBattleRequestId
pendingResultId
unlockedChapterIds
completedStageIds
openedHookIds
lastTransitionId
devOnly
isEnabled
formalFlow
```

Do not create IDs from time, random, GUID, frame count, process state, or
Unity object identity. Required IDs must be supplied by the caller or derived
deterministically from explicit inputs.

The default session unlock set contains Chapter 1 only.

There must be no serialization to disk, `PlayerPrefs`, SaveData, Inventory,
Resources, or Scene objects.

### 7.4 Reducer phases and actions

The reducer must be pure and deterministic.

Required phases:

```text
Disabled
ChapterSelect
Prepare
BattleRequested
BattleRunning
Settlement
DropCandidatePreview
BossGate
BossChallengeReady
ChapterComplete
RunComplete
```

Required actions:

```text
OpenSession
SelectChapter
EnterPrepare
RequestBattle
ConfirmBattleStarted
AcceptBattleResult
ContinueFromSettlement
ContinueFromDropCandidatePreview
OpenBossGate
ReturnToPrepareFromBossGate
ConfirmManualBossChallenge
ReturnToChapterSelect
ResetSession
```

Required transition rules:

1. Feature flag false rejects active flow transitions and leaves the
   snapshot unchanged with an explicit diagnostic.
2. Normal stages can request Battle only from `Prepare`.
3. Boss stages cannot request Battle from `Prepare`, `Settlement`, or
   `BossGate`.
4. `x-9` win reaches `Settlement`, then `DropCandidatePreview`, then
   `BossGate`.
5. `BossGate` may return to `Prepare` without starting Battle.
6. Only `ConfirmManualBossChallenge` may reach `BossChallengeReady`.
7. Only `BossChallengeReady` may create the `x-10` Battle request.
8. Matching normal-stage win completes that stage and advances only after
   settlement/drop acknowledgements.
9. Matching Boss win completes the chapter and unlocks only the next chapter
   in the in-memory snapshot.
10. Chapter 4 completion reaches `RunComplete`.
11. Lose and abandon never unlock a chapter and return to a legal prepare or
    chapter-select state.
12. Stale/mismatched request ID, result ID, round ID or stage ID is rejected
    without state mutation.
13. Calling the same accepted transition twice must be idempotent or return a
    deterministic duplicate diagnostic without double completion/unlock.

### 7.5 BattleStartRequest projection

The projection may create an in-memory `BattleStartRequest` only.

It must set:

```text
requestId = caller supplied stable ID
chapterId = manifest chapter ID
stageId = manifest stage ID
roundId = manifest stage ID
isBossStage = manifest value
bossProfileId = locked route identity for x-10, otherwise empty
enemyProfileId = caller-supplied binding or empty; never guessed
entrySource = an existing legal enum value
formalFlow = false
devOnly = true
sourceRoute = V0.4 B-line devOnly route identifier
sourceScene = empty
sourceController = pure ChapterFlow projector identifier
seedId = caller supplied or empty
```

The package must not call a Battle executor.

### 7.6 BattleResultSnapshot interpretation

Interpret the existing snapshot without editing it.

Required rejection:

```text
shouldWriteSave=true
shouldGrantReward=true
requestId mismatch
roundId mismatch
resultId missing
contradictory win/lose/abandon flags
duplicate accepted result with conflicting content
```

`rewardPreview`, `itemDrops`, `rewardClaimToken`, and
`chapterProgressDelta` must never be treated as authority or granted output.
They may only appear in diagnostics indicating that the pure ChapterFlow
package ignored them.

For Chapter 3 `3-10`, a matching `win=true` result may complete the route
without requiring `bossDefeated=true`, preserving the locked
stop-attacking-and-allow-passage outcome.

The package must not manufacture a reward, drop, inventory entry, save delta,
Enemy death, or Battle damage fact.

## 8. Tutorial and Story Hook Contract

Hooks are identifiers only.

Required hook categories:

```text
before_chapter
before_stage
after_result
before_boss_gate
before_boss_challenge
after_boss_result
before_next_chapter_unlock
drop_candidate_preview
```

The manifest must generate stable IDs from chapter/stage identity. It must not
contain tutorial copy, story copy, localization text, dialogue, timing,
pause behavior, rewards, or UI placement.

No hook may auto-trigger Battle or mutate chapter unlock state.

## 9. Forbidden Dependencies and Leak Rules

Runtime source must contain zero references to:

```text
UnityEngine.SceneManagement
SceneManager
MonoBehaviour
ScriptableObject
Resources.Load
GameObject
Transform
PlayerPrefs
SaveData
SaveService
MainTrialFlowService
V02RunFlowController
V02RunConfig
AutoCombatController
RewardService
RewardConfig
RewardDropTable
Inventory
ItemDropGeneration
ItemInstanceRollEngine
EnemyRuntime
EnemySkillController
BuildSettings
AssetDatabase
UnityEditor
System.Random
UnityEngine.Random
Guid.NewGuid
DateTime.Now
DateTime.UtcNow
Environment.TickCount
```

The Editor verifier may use `UnityEditor` only for verification/report
generation. It may not edit or save Scene, Prefab, ScriptableObject,
BuildSettings, or any existing file.

The verifier must fail if any file outside the exact write whitelist is
created or modified by this task.

## 10. Verification

Create this exact Editor entry:

```text
Menu:
Tools/TalismanBag/V0.4/Verify B-Line Chapter Flow Contract 01

Batch execute method:
TalismanBag.Editor.V04.ChapterFlow.V04ChapterFlowContractVerifier.RunBatch
```

Required checks:

```text
chapters=4
stages=40
encounterNodes=12
bossGates=4
manualBossChallenges=4
automaticBossStarts=0
normalContentBindings=0
sceneBindings=0
uiBindings=0
rewardBindings=0
saveBindings=0
inventoryBindings=0
enemyRuntimeBindings=0
battleExecutorBindings=0
formalBindings=0
legacyContentRefs=0
duplicateStageIds=0
missingHookIds=0
forbiddenDependencyHits=0
protectedHashDrift=0
```

The verifier must cover at least:

- full deterministic success traversal over all forty stages;
- x-9 stop-before-Boss for all chapters;
- manual-only x-10 start for all chapters;
- Boss gate return-to-prepare path;
- normal win, Boss win, lose and abandon;
- Chapter 3 yield-style completion;
- stale request/result rejection;
- conflicting result flags rejection;
- duplicate result idempotency;
- feature-flag-default-false rejection;
- session reset returning to Chapter-1-only unlock;
- no reward/save/inventory/drop interpretation.

Unity QA is allowed only after confirming no interactive Unity lock. Static
and Unity batch QA may run automatically. This pure contract package closes
on automated PASS and does not require user hand test.

## 11. Required Reports

### `BLineChapterFlowContractReport.md`

Must include:

- package and schema IDs;
- assignment SHA supplied by dispatch;
- runtime and verifier file inventory;
- all counts in section 10;
- Unity compile/batch result;
- protected before/after aggregates;
- final marker;
- explicit statement that no second package was started.

### `BLineChapterFlowContractSpec.csv`

One row per normative assertion with:

```text
assertionId,category,expected,actual,result
```

### `BLineChapterFlowManifest40.csv`

Exactly forty data rows plus header. Include stable route identity, zone,
Boss flags, resolution mode, hook IDs and dev/formal flags.

### `BLineChapterFlowTransitionMatrix.csv`

Include every tested transition, initial phase, action, expected phase,
actual phase, mutation expectation and result.

### `BLineChapterFlowLeakCheckReport.md`

Include forbidden dependency counts, whitelist delta, protected aggregates,
and zero existing-file modifications.

## 12. Acceptance Marker

The final successful console/report marker must be exactly:

```text
BLINE_CHAPTER_FLOW_CONTRACT01_PASS chapters=4 stages=40 bossGates=4 manualBossChallenges=4 formalBindings=0 saveWrites=0 rewardGrants=0 legacyContentRefs=0
```

Any failure must emit:

```text
BLINE_CHAPTER_FLOW_CONTRACT01_FAIL errors=<positive integer>
```

The task is complete only when:

- all new Runtime and Editor code compiles;
- verifier PASS is recorded;
- all five reports exist and agree;
- input and protected hashes match;
- no existing file changed;
- no out-of-whitelist file exists;
- no Git operation occurred;
- no Scene/UI/Reward/Save/Inventory/Enemy Runtime/Battle Executor connection
  exists;
- no second package was started.

## 13. Task Window Operating Rules

1. Read the workspace and project `AGENTS.md`, every `Docs/LOCKED/*`, the
   mandatory ROADMAP/CURRENT/V0.3 Queue, and relevant V0.4 queues before work.
2. Verify this Assignment file's full SHA-256 against the exact SHA supplied
   in the dispatch prompt before any implementation.
3. Verify all input and protected hashes before work.
4. Stop only for a real safety boundary, hash mismatch, unexpected existing
   target file, interactive Unity lock that prevents QA, or unavoidable
   out-of-whitelist dependency.
5. Do not request design decisions already outside this package.
6. Do not contact or reuse Item, Enemy, Visual, or other active development
   windows.
7. Do not modify Queue, LOCKED, old formal 1-10/2-10, or Git.
8. Do not start a second package.
9. After automated PASS, send one final completion report to the parent task.

## 14. Guard Freeze Receipt

```text
Assignment path:
Docs/V0.4/BLineChapterFlowContract01_Assignment.md

Full-file SHA-256:
SUPPLIED_OUT_OF_BAND_BY_GUARD_DISPATCH

Existing-file modification whitelist:
EMPTY

New-file whitelist:
FROZEN_BY_SECTION_6

Protected aggregates:
LOCKED=7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f
GOVERNANCE=beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2
FORMAL_V03=97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d
READ_INPUTS=d7f13a98a439ab7d1a201bc02bf159241a6656691be7e2e880190080ab342683
```

