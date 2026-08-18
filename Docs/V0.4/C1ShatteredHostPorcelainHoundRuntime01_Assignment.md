# V0.4-C1ShatteredHostPorcelainHoundRuntime01 Assignment

```text
Package: V0.4-C1ShatteredHostPorcelainHoundRuntime01
Workflow: COMPLEX_GUARDED_ONCE
Guard mode: GUARD_FREEZE_AND_DISPATCH
Assignment status: FROZEN_FOR_SINGLE_TASK_DISPATCH
Risk: R2_ENEMY_RUNTIME_CONTRACT_ISOLATED
Primary owner: Enemy System
Guard marker: ENEMY_GUARD_ASSIGNMENT_C1SHATTEREDHOSTPORCELAINHOUNDRUNTIME01
User hand test: NOT_REQUIRED_FOR_THIS_COMPONENT_CONTRACT
Git operations: FORBIDDEN
Next package: FORBIDDEN_TO_START
```

## 1. Objective

Create one dev-only, disabled, pure Enemy Runtime and Action contract for
exactly two Chapter 1 normal-enemy carriers:

```text
bone_aspect_enemy_c1_01_shattered_host / 碎骨附身者
bone_aspect_enemy_c1_02_porcelain_hound / 骨瓷犬
```

The output must be sufficient for a later B-line stage-manifest binding and a
later Battle integration package to consume. This package itself must not bind
to the B-line route, Battle executor, Scene, UI, Item, Save, Reward, Drop,
Inventory, formal RunFlow, or the protected V0.2/V0.3 Enemy runtime.

The package owns only:

- immutable Enemy runtime/profile/action snapshots;
- deterministic pure reducers and schedulers;
- Enemy-side state and validation;
- neutral effect/counter-window requests for later Battle consumption;
- deterministic verification and reports.

## 2. Frozen Content Scope

### 2.1 Shattered Host

Accepted stable identity:

```text
contentId=bone_aspect_enemy_c1_01_shattered_host
displayName=碎骨附身者
presentationKey=enemy.bone_aspect.c1.shattered_host
runtimeProfileId=bone_aspect.runtime.c1.shattered_host.v1
```

Accepted mechanic keys:

```text
mechanic.basic_pressure
mechanic.polluted_tile
mechanic.possession_state
```

The minimum runtime meaning is frozen as:

- the enemy has an Enemy-owned self-state declaring possession active;
- that state is not a relationship to another runtime actor;
- no host actor, target, player status, duration, removal condition, damage
  modifier, copied ability, or possession execution is authored;
- one deterministic BasicAttack action may emit a neutral
  `battle.effect_request.direct_player_damage` request;
- defeat may emit exactly one neutral
  `battle.effect_request.post_defeat_field_area` request for a short-lifetime
  field-area intent;
- no area cells, radius, damage, duration ticks, movement rule, Item mutation,
  or field execution are authored by Enemy.

The post-defeat request is an intent only. Its future execution owner is
Battle Field Runtime.

### 2.2 Porcelain Hound

Accepted stable identity:

```text
contentId=bone_aspect_enemy_c1_02_porcelain_hound
displayName=骨瓷犬
presentationKey=enemy.bone_aspect.c1.porcelain_hound
runtimeProfileId=bone_aspect.runtime.c1.porcelain_hound.v1
```

Accepted mechanic keys:

```text
mechanic.layered_shield
mechanic.charge_attack
counter_window.shell_break
```

The minimum runtime meaning is frozen as:

- Enemy owns current HP, current shell, shell intact/broken state, lifecycle,
  monotonic revision, accepted application identity, and Enemy cues;
- one deterministic ChargeAttack action may emit a neutral
  `battle.effect_request.charge_attack` request;
- the request does not define path, collision, hitbox, movement, target
  resolution, damage, knockback, stun, bind, or player-state mutation;
- accepted shell delta may emit ShellHit;
- the transition from positive shell to zero emits ShellBreak exactly once
  per reset generation;
- the same transition emits one
  `counter_window.shell_break` request with a dev-only requested duration;
- Battle owns the clock and the actual opening, closing, scoring, and
  application of the weakness/counter window;
- the initial contract has one shell only and does not regenerate it.

### 2.3 Explicit BA-D3 hold

This package must contain exactly one scope/hold assertion for:

```text
contentId=bone_aspect_enemy_c1_03_bone_swap_remnant
status=HELD_BY_BA-D3
runtimeProfiles=0
actionPatterns=0
copySlots=0
runtimeBindings=0
```

It must not create a Runtime profile, state, action, reducer branch, fixture
enemy, copy slot, effect request, or presentation cue for Bone Swap Remnant.

## 3. Dev-Only Fixture Values

These values are test fixtures, not formal balance and not chapter tuning:

```text
tickRate=1000

ShatteredHost:
  maxHp=160
  initialShell=0
  firstBasicDueTick=2000
  basicIntervalTicks=4000
  telegraphTicks=500
  castTicks=300
  resolveOffsetTicks=800
  recoverTicks=700
  postDefeatFieldLifetimeClass=SHORT

PorcelainHound:
  maxHp=180
  shellMax=100
  initialShell=100
  shellRegenerationCount=0
  firstChargeDueTick=2500
  chargeIntervalTicks=6000
  telegraphTicks=800
  castTicks=400
  resolveOffsetTicks=1200
  recoverTicks=900
  requestedShellBreakWindowTicks=3000
```

The reports and profiles must label all these values:

```text
devOnlyFixture=true
formalBalance=false
liveTuning=false
```

No fixture value may be written into an existing Config or ScriptableObject.

## 4. Required Contract

Create schema:

```text
schemaId=BoneAspectC1EnemyRuntime.v1
schemaVersion=1
devOnly=true
isEnabled=false
entersFormalFlow=false
runtimeBoundToBattle=false
```

### 4.1 Profile snapshot

Each of the two profiles must include at least:

```text
runtimeProfileId
contentId
displayName
presentationKey
mechanicKeys
actionPatternIds
maxHp
shellMax
initialShell
shellRegenerationCount
postDefeatEffectRequestKey
shellBreakCounterWindowKey
requestedCounterWindowTicks
devOnlyFixture
formalBalance
devOnly
isEnabled
entersFormalFlow
runtimeBoundToBattle
```

All collections must be defensively copied, immutable to consumers, sorted
ordinally for canonical output, and culture independent.

### 4.2 Runtime state snapshot

The immutable runtime state must include at least:

```text
schemaId
schemaVersion
enemyInstanceId
runtimeProfileId
contentId
presentationKey
resetGeneration
revision
lifecycle
maxHp
currentHp
shellMax
currentShell
shellState
selfPossessionState
selfPossessionGameplayEffectAuthored
targetable
lastAcceptedBattleTick
lastAcceptedApplicationSequence
acceptedApplicationCount
activeAction
pendingRequests
latestCue
errors
fullCanonicalSignature
presentationSafeCanonicalSignature
```

Required lifecycle values:

```text
Disabled
Present
Active
Defeated
```

Required shell values:

```text
NotApplicable
Intact
Broken
```

Required self-possession values:

```text
NotApplicable
DeclaredOwnStateOnly
```

For Shattered Host:

```text
selfPossessionState=DeclaredOwnStateOnly
selfPossessionGameplayEffectAuthored=false
```

The state must not include a possession target ID, host actor ID, external
state owner, copied effect, player status, or execution result.

### 4.3 Accepted Battle application result

The reducer consumes only a neutral, already-adjudicated Battle application
result. It must not consume Item requests or calculate damage.

Required input fields:

```text
applicationEventId
applicationSequence
resetGeneration
battleTick
targetEnemyInstanceId
acceptedByBattleLedger
actualHpDeltaApplied
actualShellDeltaApplied
sourceRequestId
```

Required rules:

1. Missing ID, wrong target, stale generation, non-monotonic sequence,
   non-monotonic tick, duplicate event ID, rejected ledger result, negative
   delta, impossible shell delta, or delta beyond current state is rejected
   without mutation or cue.
2. An accepted application must contain at least one positive actual delta.
3. Shattered Host rejects shell delta.
4. Porcelain Hound rejects HP delta while its shell was positive. A Battle
   integration must submit shell break and exposed HP application as legal
   adjudicated results.
5. Accepted shell delta emits ShellHit. Positive-to-zero emits ShellBreak
   once and one counter-window request.
6. Accepted HP delta emits Hit.
7. Current HP reaching zero emits Defeated once.
8. Shattered Host defeat emits one post-defeat field-area request.
9. Duplicate, stale, rejected, or zero-delta results emit no Hit, ShellHit,
   ShellBreak, Defeated, or effect request.
10. Reset increments generation and clears generation-local dedupe, counters,
    actions, requests, and cues.

Enemy owns HP/shell/lifecycle state. Battle owns the application ledger,
target selection, ordering, clock, actual player damage, field execution, and
counter-window execution.

### 4.4 Action patterns

Create exactly two scheduled action patterns:

```text
c1.shattered_host.basic_attack
c1.porcelain_hound.charge_attack
```

Each pattern must contain:

```text
actionPatternId
ownerRuntimeProfileId
mechanicKey
effectRequestKey
firstDueTick
repeatIntervalTicks
telegraphTicks
castTicks
resolveOffsetTicks
recoverTicks
interruptPolicy
priority
devOnlyFixture
```

Required scheduler behavior:

- consume a caller/Battle-supplied monotonic tick;
- never use frame time, wall-clock time, random, Unity object identity, or
  animation completion;
- one action at a time;
- reactive Hit/ShellHit/ShellBreak/Defeated processing has priority;
- a due action that is temporarily blocked stays pending deterministically;
- a defeated enemy schedules or resolves no later attack;
- resolve emits a neutral effect request at the authoritative resolve tick;
- the request carries no player HP, damage, path, collision, hitbox, or
  movement result;
- reset clears active/pending action state and restarts fixture cadence.

Required action cues:

```text
Presence
BasicAttack
ChargeAttack
Hit
ShellHit
ShellBreak
CounterWindowRequested
Defeated
PostDefeatFieldRequested
Reset
```

Every transient cue and request must have a stable monotonic identity,
generation, revision, source event/execution ID, and Battle tick.

## 5. Player-Safe and Developer Isolation

The presentation-safe snapshot may expose:

```text
enemyInstanceId
contentId
presentationKey
resetGeneration
revision
lifecycle
maxHp/currentHp
shellMax/currentShell/shellState
selfPossessionState
targetable
latestCue
publicErrors
```

It must not expose:

```text
acceptedApplicationEventIds
dedupe ledger
internal action queue
developer diagnostics
fixture tuning labels
future Battle execution answers
field-area execution data
counter-window scoring or exact success logic
BA-D3 copy semantics
```

Full and presentation-safe canonical signatures must be separately stable.
Developer-only content changes must change the full signature without leaking
into the presentation-safe payload unless the changed field is explicitly
player-safe.

## 6. Source Ownership and Isolation

| Fact or state | Owner | This package |
| --- | --- | --- |
| Carrier identity and presentation key | Accepted BoneAspect carrier catalog | Read exact values |
| Mechanic vocabulary | E02 plus accepted BoneAspect extension | Read exact stable keys |
| Enemy HP, shell, lifecycle and dedupe | Enemy Runtime | Own new immutable state |
| Battle tick, target selection and application ledger | Battle | Consume neutral fixture result only |
| Player HP/damage/effect application | Battle | Emit neutral request only |
| Post-defeat field area | Battle Field Runtime | Emit intent only |
| Shell-break weakness window execution | Battle Resolver | Emit request only |
| Visual motion and VFX | Visual/Presentation | Emit read-only cues only |
| B-line route and stage binding | B-Line ChapterFlow | No binding in this package |
| Item facts and effect requests | Item | No dependency or read |

No runtime file may reference or execute:

```text
EnemyDefinition
EnemyGroup
EnemySkillDefinition
EnemySkillRuntime
AutoCombatController
V02RunFlowController
SceneManager
MonoBehaviour
ScriptableObject
GameObject
Transform
Resources.Load
AssetDatabase
UnityEditor
ItemSystem
ItemCombatEffectRequest
Inventory
SaveData
Reward
DropTable
MainTrialFlowService
PlayerPrefs
System.Random
UnityEngine.Random
Guid.NewGuid
DateTime.Now
DateTime.UtcNow
Environment.TickCount
```

The Editor verifier may use UnityEditor for verification/report generation
only. It must not edit or save a Scene, Prefab, ScriptableObject, Config,
BuildSettings, or any existing file.

## 7. Read-Only Inputs and Frozen Hashes

Before any development, require exact SHA-256 matches:

| Path | SHA-256 |
| --- | --- |
| `Docs/V0.4/Reports/BoneAspectContentCatalog.csv` | `56bd2430adcaefb69ec2b51297f2ba2c561c63f953571e528d40080f6b6e660c` |
| `Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv` | `f41322bfb8bba99fc1af2c1c70a527feee62ddb9bc68bb1c767e272027381a64` |
| `Docs/V0.4/Reports/BoneAspectCarrierPresentationMechanicReferenceRows.csv` | `e8fa952d7799a074a68ce176f47cf461b8dfa0c4b2c39d0be9644642ac57fac6` |
| `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMatrix.csv` | `0d029675c9d9079853de446cbc9ec587ddca94de81a845b92051f7de53756a0d` |
| `Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionInventory.csv` | `1b8c3f3286f62d6fe5a79061e14e1f2823fd5ca403c609e55f9db83ab5d470d3` |
| `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs` | `ca358e5983c9d577c8942435604144c8704763d10cfeca579e5f40546ecfa85d` |
| `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs` | `c1cb6c1e68cec5b54cd5a43c9011a8ed6b335c95d80b01fdf6e12d13145a1756` |

Aggregate algorithm:

```text
sort project-relative forward-slash paths ordinally
row = path + "|" + lowercase_file_sha256
payload = LF-joined rows + trailing LF
aggregate = lowercase SHA-256 of UTF-8 payload
```

Frozen input aggregate:

```text
count=7
aggregate=c67870aa276aaf17ffa3f6c0f62e0fa80db3c59a972fd40ecae310b18827c6e3
```

Any mismatch is a safety stop. Do not refresh or reinterpret the baseline.

## 8. Protected Baselines

Use the section 7 aggregate algorithm. After implementation, exclude only the
exact 23 package targets in section 9 before recomputing the package-owned
roots.

```text
LOCKED:
  count=15
  aggregate=7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f

GOVERNANCE_AND_QUEUES:
  count=9
  aggregate=beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2

FORMAL_V03_1_10_2_10:
  count=8
  aggregate=97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d

BLINE_CHAPTER_FLOW_PACKAGE:
  count=21
  aggregate=5ceb86f913df40bc20640c797f65bd3ac9c5df5b5d5dc49ba999d27bc4d170f1

BONE_ASPECT_RUNTIME_PREPACKAGE:
  count=33
  aggregate=db18eacdb402409bcdfa81a2b7fb995a11d252cee88a1396e7a0a0f1f4f87aae

EDITOR_ENEMY_PREPACKAGE:
  count=30
  aggregate=d06b197da8bf62117f90fe391afe298ad068f1ed23081b1fa25f144f233c2639

BONE_ASPECT_AND_SHOUGUNU_REPORTS_PREPACKAGE:
  count=53
  aggregate=c18aad2f88d9e59da6fe66e55e7f6bee7a26cbdb2b019f807e313a38e7a82906

ITEMS_READ_ONLY:
  count=153
  aggregate=14a6f36f86f2a4cc2dee00eaa7b97eb65c6f55e64b2ee7351a04f84ed05b7598

BUILDSANDBOX_READ_ONLY:
  count=188
  aggregate=cbe13503ad5c16c487e9d5eec0a5b4b441d30d7b5d6e0d5c13da7d8a38051811
  baselineRevision=PREFLIGHT_EXTERNAL_DRIFT_ATTRIBUTION_REVISION01
  priorSnapshotUtc=2026-07-28T16:24:19.811Z
  exactExternalPath=Assets/_Game/Scripts/TalismanBag/BuildSandbox/LiHuoCombatFeedbackOrchestrationController.cs
  priorPathSha256=d19eca0c90176306f69c10b1b48d0ef4728daca015e8dfb9cfac548e7bdcd3cc
  acceptedPathSha256=ab09deeaa8504cda7d38c43cf81ba9612adf72a4cbefd0e650ad726d4680a541
  externalOwnerPackage=V0.4-LiHuoCombatFeedbackOrchestrationVerticalSlice01
  externalOwnerTask=019fa2e2-5cd8-734f-ae74-242fc9a02ead
  overlapWithEnemyWriteWhitelist=false
  note=EDITOR_ENEMY_PREPACKAGE and PROJECTSETTINGS_READ_ONLY currently equal their original frozen aggregates and were not rebaselined.

CROSSSYSTEM_READ_ONLY:
  count=63
  aggregate=f89f6d531b0bc97f83658918887574fca4775bed7dbe4f4ac2bd1124da154d40

SCENES_READ_ONLY:
  count=14
  aggregate=a2bc880126c7ca69143277486bd495c3d7aa144a29d3446a5a248626348920d1

PREFABS_READ_ONLY:
  count=16
  aggregate=a3c0fdc0ae331987e22ad409c5142ace4c4a03fe5f36dfbc6f7683bc9d82d208

PROJECTSETTINGS_READ_ONLY:
  count=21
  aggregate=9e3c370e01deaeb794933fb4d19103d613c5a270b8d9d211c42b4ce8971a76f5
  baselineRevision=POST_DISPATCH_UNITY_SERIALIZATION_ATTRIBUTION_REVISION02
  exactExternalPath=ProjectSettings/ProjectSettings.asset
  transientCreationTimeUtc=2026-07-28T16:53:56.5848320Z
  transientObservedLastWriteTimeUtc=2026-07-28T16:53:56.585Z
  transientObservedSha256=ef3b619ca36d5ea3c4f3efffbdec7280feda2dfb8a29530eec5369552e00db7d
  transientObservedAggregate=95cc3ba609398f3baff5b2cce4ac90a192970d63ae51315d3dc456720af92e57
  acceptedClosedCleanLastWriteTimeUtc=2026-07-28T16:55:56.2655597Z
  acceptedClosedCleanLength=23168
  acceptedClosedCleanPathSha256=88eb5cff714ac9d529a371abd53eb6bc56ae771e55986753f37408153d364957
  attribution=EXTERNAL_UNITY_EDITOR_SERIALIZATION_LIFECYCLE_CLOSED
  attributionEvidence=The transient content was observed while external UnityLockfile and EditorInstance.json existed and this package had written zero files. After that lifecycle closed, Unity serialization processes, UnityLockfile, and EditorInstance files were absent, and the path plus the 21-file aggregate returned byte-identically to the original frozen baseline.
  overlapWithEnemyWriteWhitelist=false

PACKAGES_READ_ONLY:
  count=2
  aggregate=52ad627caad2e8ca13a4cd16644a9178394c7cfe07b296f5610b00a2bcb546d2
```

The nine governance paths are:

```text
AGENTS.md
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
```

External concurrent drift in Items, BuildSandbox, CrossSystem, Scenes, or
Prefabs may be recorded only when it has an exact owning-task attribution and
there is no write overlap. It must not be restored, reformatted, staged,
claimed, or used to refresh this package's semantic baseline.

## 9. Exact Write Whitelist

### 9.1 Existing files allowed to modify

```text
NONE
```

### 9.2 Exact new files

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs.meta
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeReport.md
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeSpec.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFieldMatrix.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeEnemyProfiles.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeActionPattern.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFixtureTrace.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeNegativeFixtureRows.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeLeakCheckReport.md
```

Exact totals:

```text
new files=23
existing files modified=0
unexpected files=0
Scene/Prefab/Config/Battle/Board/Item modifications=0/0/0/0/0/0
```

The reports must enumerate this exact 23-path package manifest verbatim.

## 10. Verification

Use the compile-safe namespace:

```text
namespace TalismanBag.EditorTools.EnemySystem
```

Required Editor entry:

```text
Menu:
Tools/TalismanBag/V0.4/Verify C1 Shattered Host Porcelain Hound Runtime 01

Batch execute method:
TalismanBag.EditorTools.EnemySystem.C1ShatteredHostPorcelainHoundRuntimeVerifier.RunFromCommandLine
```

The package must provide a same-source offline verifier path and the Unity
entry above. Both must execute the same scenario source and generate identical
reports.

Required fixture coverage:

1. exactly two Runtime profiles and two action patterns;
2. exact carrier identity, presentation keys, mechanic keys, and flags;
3. Shattered Host own-state possession with zero target/relation/effect rows;
4. Shattered Host BasicAttack due, telegraph, resolve request, and repeat;
5. Shattered Host accepted HP hit, duplicate/stale rejection, defeat, and
   exactly-once post-defeat field request;
6. Porcelain Hound shell hit, exact positive-to-zero ShellBreak, one
   counter-window request, and no shell restoration;
7. Porcelain Hound ChargeAttack due, telegraph, resolve request, and repeat;
8. Porcelain Hound guarded HP-delta rejection and exposed HP hit/defeat;
9. wrong target, wrong generation, non-monotonic sequence/tick, rejected
   ledger result, zero delta, negative delta, and impossible delta;
10. reset-generation isolation and generation-local dedupe clearing;
11. action collision, reactive priority, defeated cancellation, and no
    animation-completion dependency;
12. input reversal, invariant culture, canonical field sensitivity,
    defensive copy, and immutable collections;
13. full versus presentation-safe isolation;
14. Bone Swap Remnant exact `HELD_BY_BA-D3` row and zero Runtime/action/copy
    rows;
15. old V02 Enemy/AutoCombat references zero;
16. Scene/UI/Battle-executor/Item/Save/Reward/Drop/Inventory/formal bindings
    zero;
17. repeat report generation determinism and deleted-report regeneration;
18. exact 23-path manifest, zero existing-file modifications, no protected
    drift, and no package leak.

Unity batch may run only after an atomic clean serialization check. Never stop
another process, close the user's Editor, or delete a lock not owned by this
package. An occupied Unity lease may hold only the final Unity QA; it does not
authorize scope expansion.

## 11. Required Reports

### Main report

`C1ShatteredHostPorcelainHoundRuntimeReport.md` must include:

- Assignment path and SHA supplied by dispatch;
- schema/package/marker;
- exact 23-path package manifest;
- exact two profile and two action rows;
- dev-only fixture values and non-formal labels;
- BA-D3 hold result;
- fixture/negative counts;
- offline and Unity results;
- protected before/after aggregates;
- final acceptance marker;
- explicit statement that no next package started.

### CSV reports

`C1ShatteredHostPorcelainHoundRuntimeSpec.csv`:

```text
assertionId,category,expected,actual,result
```

`C1ShatteredHostPorcelainHoundRuntimeFieldMatrix.csv`:

```text
schemaOrSnapshot,field,owner,playerSafe,developerOnly,notes
```

`C1ShatteredHostPorcelainHoundRuntimeEnemyProfiles.csv`:

One row per Runtime profile plus one explicit BA-D3 hold row.

`C1ShatteredHostPorcelainHoundRuntimeActionPattern.csv`:

Exactly two action rows with fixture cadence and neutral request keys.

`C1ShatteredHostPorcelainHoundRuntimeFixtureTrace.csv`:

Deterministic positive traces for both enemies.

`C1ShatteredHostPorcelainHoundRuntimeNegativeFixtureRows.csv`:

All required invalid/rejected cases with mutation and cue counts.

### Leak report

`C1ShatteredHostPorcelainHoundRuntimeLeakCheckReport.md` must include:

- forbidden dependency scan;
- exact package manifest;
- existing-file modification count;
- all protected aggregates;
- runtime/profile/action/copy row counts for Bone Swap Remnant;
- formal, Battle-executor, Scene, UI, Item, Save, Reward, Drop, and Inventory
  binding counts;
- report determinism and regeneration result.

## 12. Acceptance

Successful verification must emit exactly:

```text
C1_SHATTERED_HOST_PORCELAIN_HOUND_RUNTIME01_PASS profiles=2 actions=2 heldByBad3=1 battleBindings=0 formalBindings=0
```

Failure must emit:

```text
C1_SHATTERED_HOST_PORCELAIN_HOUND_RUNTIME01_FAIL errors=<positive integer>
```

The package is complete on automated QA PASS when:

- all 23 new files exist and no other file was added;
- existing-file modifications remain zero;
- Runtime and Editor code compile;
- same-source offline verifier PASS;
- Unity batch compile/verifier PASS, or an explicitly Guard-adjudicated
  serialization-only hold remains;
- generated reports agree and are deterministic;
- all input and protected hashes are unchanged, except exact attributed
  non-overlapping external concurrent drift;
- no Scene, Prefab, Config, Battle, Board, Item, Save, Reward, Drop,
  Inventory, formal RunFlow, old V02 Enemy, or AutoCombat change exists;
- Bone Swap Remnant has one hold row and zero Runtime implementation rows;
- no Git operation occurred;
- no next package started.

No component-level user hand test is required. User testing remains deferred
to the final Chapter 1 continuous BattleSandbox integration.

## 13. Operating Rules

1. Read project `AGENTS.md`, every `Docs/LOCKED/*`, mandatory roadmap/current
   files, V0.4 queues, `EnemySystemGuard_CurrentRules.md`, and this Assignment.
2. Verify this Assignment's exact SHA-256 from the dispatch before writing.
3. Verify all section 7 and 8 baselines before writing.
4. Stop on a real hash mismatch, unexpected existing target, write-domain
   conflict, or unavoidable out-of-whitelist dependency.
5. Do not ask the user to resolve BA-D3; it is explicitly held outside scope.
6. Do not contact, interrupt, or reuse Item, Visual, Battle, or B-line
   development tasks.
7. Do not modify LOCKED, queues, formal 1-10/2-10, BuildSettings, or Git.
8. Do not start another package.
9. At terminal automated QA, send one consolidated status to Enemy Guard and
   RepoOps. Do not send fragmented progress messages.
