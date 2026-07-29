# V0.4-BLineChapter1ContinuousBattleIntegration01 Assignment

```text
Package: V0.4-BLineChapter1ContinuousBattleIntegration01
Workflow: COMPLEX_GUARDED_ONCE
Workflow simplification: V2
Guard mode: GUARD_FREEZE_AND_DISPATCH
Assignment status: FROZEN_FOR_SINGLE_TASK_DISPATCH
Risk: R3_CROSS_SYSTEM_DEVONLY_CONTINUOUS_FLOW
Primary owner: B-Line Chapter Flow
User authorization: DIRECT_USER_AUTHORIZATION_BLINE_CONTINUE_AFTER_STATUS_REVIEW
User hand test: ONE_FINAL_CONTINUOUS_CHAPTER1_HANDTEST_REQUIRED_AFTER_AUTOMATED_PASS
Git operations: FORBIDDEN
Next package: FORBIDDEN_TO_START
```

## 1. Objective

Create one independent, dev-only, default-off Chapter 1 integration package
that makes the accepted V0.4 contracts playable as one continuous
BattleSandbox route:

```text
session open
→ Chapter 1 select
→ preparation
→ BattleStartRequest
→ normal-enemy Battle result
→ settlement
→ non-grant drop-candidate boundary
→ next stage
→ 1-9 stop
→ Boss gate
→ manual 1-10 challenge
→ Shougunu Phase1 vertical-slice defeat
→ temporary Chapter 1 clear
→ session-only Chapter 2 unlock
```

The package must consume the accepted V0.4 ChapterFlow, Chapter 1 encounter
manifest, two-enemy Runtime, real Item damage request, and Shougunu Phase1
BattleSandbox vertical-slice contracts without changing their truth.

This is the earliest player-visible Chapter 1 closure. It is not a formal
route promotion.

## 2. Fixed Boundaries

Required package flags:

```text
schemaId=V04Chapter1ContinuousBattleIntegration.v1
devOnly=true
featureDefaultEnabled=false
formalFlow=false
entersFormalRunFlow=false
writesSave=false
grantsReward=false
grantsDrop=false
writesInventory=false
modifiesSceneAsset=false
```

The package must not:

- modify `V04ChapterFlowFeatureFlags.EnableBLineChapterFlow=false`;
- modify any accepted ChapterFlow, Chapter 1 manifest, Enemy Runtime, Item,
  Shougunu, Battle Bridge, BuildSandbox, Scene, Prefab, Config, ProjectSettings,
  Package, Queue, LOCKED, ROADMAP, CURRENT, or legacy file;
- bind any V0.2/V0.3 `EnemyDefinition`, `EnemyGroup`, `EnemySkillDefinition`,
  `EnemySkillRuntime`, `DropTable`, `AutoCombatController`,
  `V02RunFlowController`, `MainTrialFlowService`, legacy Item ID executor, or
  legacy reward/save authority;
- create or grant formal Reward, Drop, Inventory, Save, chapter progression,
  achievement, or claim token;
- implement tutorial or story content;
- implement BA-D3, BA-D4, Bone Swap Remnant, Chapter 2, 2-10, Chapter 3,
  Chapter 4, or `FALSE_CLONE`;
- claim the full Shougunu Boss is defeated or content-final;
- modify the existing BattleSandbox scene or persist injected GameObjects;
- reuse, interrupt, or send work to Item, Enemy, Visual, or Battle
  development tasks.

The integration development must run in a new independent B-line task.

## 3. Accepted Upstream Status

The task must verify these terminal facts before writing:

```text
V0.4-BLineChapterFlowContract01:
  PACKAGE_COMPLETE
  chapters=4
  stages=40
  bossGates=4
  manualBossChallenges=4
  Unity=PASS

V0.4-BLineChapter1EncounterManifest01:
  PACKAGE_COMPLETE
  1-1..1-9 bindings=9
  shatteredHost=4
  porcelainHound=5
  heldByBAD3=1
  Unity=PASS

V0.4-C1ShatteredHostPorcelainHoundRuntime01:
  PACKAGE_COMPLETE
  profiles=2
  actions=2
  heldByBad3=1
  battleBindings=0
  formalBindings=0
  offline=PASS
  Unity=PASS

V0.4-ShougunuPhase1RuntimeAndActionContract01:
  PACKAGE_COMPLETE

V0.4-ShougunuPhase1BattleApplicationContract01:
  PACKAGE_COMPLETE

V0.4-RealItemDamageShougunuPhase1BattleSandboxVerticalSlice01:
  AUTOMATED_QA_PASS
```

## 4. Chapter 1 Stage Binding

Consume the existing `V04Chapter1EncounterManifest.Bindings` read-only.
Do not restate a second authoritative stage manifest.

The integration must resolve exactly:

```text
1-1 shattered host
1-2 shattered host
1-3 shattered host
1-4 porcelain hound
1-5 porcelain hound
1-6 porcelain hound
1-7 porcelain hound
1-8 shattered host (temporary substitute)
1-9 porcelain hound
1-10 Shougunu Phase1 temporary completion adapter
```

The integration must validate:

- every 1-1 through 1-9 `contentBindingSlotId` matches ChapterFlow;
- every content ID resolves to exactly one new C1 Runtime profile;
- the Runtime profile/content/presentation identities agree;
- 1-8 remains labeled temporary and its future slot remains
  `HELD_BY_BA-D3`;
- Bone Swap Remnant has zero active Runtime, action, copy, stage, and Battle
  rows;
- 1-9 retains `stopBeforeBossAfterWin=true`;
- 1-10 retains `requiresManualBossChallenge=true`;
- old V0.2/V0.3 enemy, item, drop, and executor references remain zero.

## 5. ChapterFlow Session Orchestration

Create one session-only orchestrator that calls the accepted
`V04ChapterFlowReducer` with an explicit dev handtest enable fact. It must not
change the accepted feature flag or reducer.

Required nominal action sequence:

```text
CreateDisabled
OpenSession
SelectChapter(bone_aspect_chapter_1)
EnterPrepare

for 1-1..1-9:
  RequestBattle
  ConfirmBattleStarted
  AcceptBattleResult
  ContinueFromSettlement
  expose DropCandidatePreview with zero grant
  ContinueFromDropCandidatePreview

after 1-9:
  phase=BossGate
  OpenBossGate
  wait for explicit player Manual Boss Challenge

for 1-10:
  ConfirmManualBossChallenge
  RequestBattle
  ConfirmBattleStarted
  AcceptBattleResult
  ContinueFromSettlement
  phase=ChapterComplete
  unlockedChapterIds contains bone_aspect_chapter_2
```

Every transition, request, round, result, application, and attempt ID must be
stable, monotonic within the session, culture-independent, and generated
without `Guid`, wall-clock time, `Random`, Unity object instance IDs, or
animation completion.

Rejected, duplicate, stale-generation, wrong-stage, wrong-request, or
contradictory results must not advance ChapterFlow.

Tutorial/story Hook IDs remain owned by the accepted ChapterFlow manifest.
The integration may validate and report them but must execute zero tutorial or
story content.

## 6. Normal Enemy Battle Adapter

Create a dev-only Battle adapter for 1-1 through 1-9. It consumes:

- the stage's resolved C1 Enemy Runtime profile;
- the real `IItemSystemBattleSandboxBoardAuthority`;
- a valid `ItemCombatEffectRequestSnapshot.v1` assembled by the accepted
  `ItemCombatEffectRequestAssembler`;
- the pure C1 Enemy reducer and action scheduler.

### 6.1 Item authority

At each normal-stage Battle start:

1. find exactly one current BattleSandbox Item authority in the target scene;
2. require valid ItemSystem, placement, qualified-build, and core-runtime
   snapshots;
3. assemble a fresh real Item combat request snapshot;
4. require `status=Valid`, at least one accepted
   `DirectFlatDamage` request, positive
   `resolvedPreMitigationDamageUnits`, a current targetable Enemy instance,
   and no source-signature drift;
5. reject start without ChapterFlow advancement when the authority is absent,
   duplicate, invalid, stale, or changed during a running Battle.

The integration may not manufacture old item IDs, read an old DropTable, or
replace the Item request's resolved damage value.

### 6.2 Deterministic application

For the normal-enemy dev Battle session:

- read valid real Item damage request rows in ordinal stable order;
- use a dev-only deterministic Battle cadence of `500` ticks between accepted
  item applications;
- label cadence `DEV_ONLY_CADENCE / NOT_BALANCE_FINAL`;
- clamp each accepted application only to the Enemy's current shell/HP;
- for Porcelain Hound, submit shell delta first and HP delta only after shell
  is broken, as separate legal `C1EnemyBattleApplicationResult` records;
- for Shattered Host, submit HP delta only;
- use the accepted Enemy reducer for all HP, shell, lifecycle, cue, and dedupe
  truth;
- stop item applications immediately after `Defeated`;
- emit a normal-stage `BattleResultSnapshot` only from terminal Enemy
  lifecycle;
- set `bossDefeated=false`, `shouldWriteSave=false`,
  `shouldGrantReward=false`, empty reward/drop/claim fields.

### 6.3 Enemy action requests

Advance the accepted Enemy scheduler with the same Battle tick.

Battle owns the minimal dev-only resolution of its neutral outgoing requests:

```text
shattered host direct-player-damage fixture=10
porcelain hound charge-attack fixture=14
player max/current HP fixture=9999
labels=DEV_ONLY_BATTLE_RESOLVER_FIXTURE / NOT_BALANCE_FINAL / NOT_CONTENT_FINAL
```

Resolve each stable request ID at most once. These values are integration
fixtures, not Enemy profile data and not formal balance.

The post-defeat field-area request and shell-break counter-window request must
be observed and reported only:

```text
fieldAreaExecutionCount=0
counterWindowExecutionCount=0
formalMechanicGrantCount=0
```

Do not invent field cells, radius, ticks, collision, weakness scoring, copy
semantics, or Item mutations.

## 7. 1-10 Boss Adapter and Approved Temporary Clear

The existing Shougunu BattleSandbox vertical-slice Runtime and Battle
application contracts remain read-only.

At the Boss gate:

- call `OpenBossGate`;
- show an explicit player-facing manual challenge action;
- do not auto-enter 1-10;
- on click, call `ConfirmManualBossChallenge`, then create the accepted
  `BattleStartRequest`;
- start the existing Shougunu Phase1 BattleSandbox Runtime through its public
  dev-only entry;
- confirm Battle start only after the Runtime reports a valid active session;
- accept a win only after the accepted Phase1 Runtime terminal lifecycle is
  `Defeated` and the vertical-slice Runtime reports completion.

The Boss result adapter may project that approved Phase1 terminal fact to
`BattleResultSnapshot.bossDefeated=true` solely to satisfy the unchanged
ChapterFlow Boss result contract.

Every applicable contract, overlay, trace, report, result `eventSummary`, and
handtest instruction must include exactly:

```text
PHASE1_DEV_VERTICAL_SLICE
NOT_CONTENT_FINAL
NOT_FORMAL_BOSS_COMPLETION
```

Also require:

```text
completionResolverSlotId=bline.c1.boss_completion_resolver
replacementMode=REPLACE_COMPLETION_RESOLVER_ONLY
chapterFlowTruthChanged=false
fullBossCompletionClaimed=false
formalBossCompletionGranted=false
```

Replacing this adapter with the future complete Shougunu Boss must require
changing only the completion resolver, not ChapterFlow manifest/reducer truth.

## 8. Settlement, Drop Boundary, Return, and Unlock

For all normal-stage wins:

- expose the existing `Settlement` state;
- expose `DropCandidatePreview` as a no-grant boundary;
- display that formal drop/reward is not connected;
- continue only through accepted reducer actions.

Required zero-authority result fields:

```text
rewardPreview=[]
itemDrops=[]
rewardClaimToken=""
chapterProgressDelta=""
shouldWriteSave=false
shouldGrantReward=false
formalDropRolls=0
formalDropGrants=0
inventoryWrites=0
saveWrites=0
```

After the Phase1 temporary clear:

- `bone_aspect_chapter_2` is present only in the current
  `V04ChapterFlowStateSnapshot.unlockedChapterIds`;
- the overlay may show "Chapter 2 unlocked for this session";
- this package must not select, run, bind, or expand Chapter 2;
- exiting Play Mode or resetting the session loses that unlock;
- no `PlayerPrefs`, SaveData, Config, asset, file, cloud, analytics, or formal
  progression write may occur.

## 9. Player-Visible BattleSandbox Surface

Target Scene, read-only:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Create an Editor-only, default-off handtest launcher:

```text
Tools/TalismanBag/V0.4/B-Line Chapter 1/Enable And Open Continuous Handtest
Tools/TalismanBag/V0.4/B-Line Chapter 1/Disable Continuous Handtest
```

Required Editor preference key:

```text
TalismanBag.V04.BLineChapter1ContinuousHandtest.Enabled
default=false
```

This preference authorizes only dev handtest injection. It is not progression
or Save data.

When enabled and Play Mode enters the exact target scene, inject one root:

```text
V04_BLine_C1_ContinuousHandtest_Runtime
HideFlags=DontSaveInEditor|DontSaveInBuild|HideInHierarchy
```

The injected root owns a dedicated dev overlay and the integration controller.
It must be destroyed on disable/Play exit and must never dirty or save the
Scene.

The overlay must show at least:

- dev-only/default-off status;
- current stage and ChapterFlow phase;
- current new V0.4 Enemy content/profile/presentation identity;
- normal Enemy HP/shell and dev player HP;
- preparation readiness or exact blocking diagnostic;
- settlement/no-grant drop-candidate state;
- the 1-9 stop;
- the manual 1-10 challenge button;
- all three Phase1 temporary-clear labels;
- Chapter 1 complete and session-only Chapter 2 unlock.

Required actions:

```text
Start Chapter 1 Session
Start Current Normal Stage
Continue From Settlement / Drop Candidate
Manual Challenge Shougunu Phase1
Reset Dev Session
```

The existing Item board/tray remains its own authority and is not redrawn,
reparented, reformatted, or persisted by this package.

For normal stages, the integration overlay may block pointer input while the
dev Battle session runs. It must not invoke the existing Shougunu Runtime.

For 1-10 only, reuse the accepted Shougunu BattleSandbox Runtime and visual
surface read-only.

## 10. Read-Only Input Freeze

Aggregate algorithm:

```text
sort project-relative forward-slash paths ordinally
row = path + "|" + lowercase_file_sha256
payload = LF-joined rows + trailing LF
aggregate = lowercase SHA-256 of UTF-8 payload
```

The exact semantic input set is:

```text
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowFeatureFlags.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowPrimitives.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowSnapshots.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowManifest.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowReducer.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowBattleContractProjection.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/V04ChapterFlowValidation.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingPrimitives.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterManifest.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1DevBossCompletionPolicy.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1EncounterBindingValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs
Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleStartRequest.cs
Assets/_Game/Scripts/TalismanBag/Contracts/Battle/BattleResultSnapshot.cs
Assets/_Game/Scripts/TalismanBag/Contracts/Battle/ShougunuPhase1BattleApplicationContracts.cs
Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/ShougunuPhase1BattleApplicationEngine.cs
Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/ShougunuPhase1BattleSandboxVerticalSliceRuntime.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs
Docs/V0.4/Reports/BLineChapterFlowContractReport.md
Docs/V0.4/Reports/BLineChapter1EncounterManifestReport.md
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeReport.md
Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractReport.md
Docs/V0.4/Reports/RealItemDamageShougunuPhase1BattleSandboxVerticalSliceReport.md
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Frozen value:

```text
INTEGRATION_INPUTS:
  count=39
  aggregate=315090e977138c2befdb04f50e4f68ef4cea6bb8d641e9074c79494917ac603b
```

Key individual checks:

```text
V04ChapterFlowFeatureFlags.cs=5102418f6da377f1d40d5feab53c7c1414042e41411e2126f0f744d78f073715
V04ChapterFlowManifest.cs=acb59867743de4eeced12810fa32d930f587cf195d0c1f7384734424b89bf416
V04ChapterFlowReducer.cs=e4df582ed79893e33e933746e126b3ceacb533b5d52444ec87d3012bed91cd73
V04Chapter1EncounterManifest.cs=930930f5b0ebc1c2f9966bf84add15117af79473110fc64e887b22fa41f2d283
V04Chapter1DevBossCompletionPolicy.cs=6ea795694f303deea7397a65abc3a778e169bb3f9a7d013522eb1c19d926dd13
C1EnemyRuntimePrimitives.cs=a6eeb38da79d77311b8f947f2adcf22db14aab2a9af6342a74b4a0dd0b7fb599
C1EnemyRuntimeReducer.cs=b7aac7d6e033212d4f3d2b242d663eb604104c7db5edaabe442e8b1c51f4ae17
C1EnemyActionScheduler.cs=9c3994f0f2b2bd48431e7255dfd93e1ba57ccf4decc3e670d0d426dbe837d822
ItemCombatEffectRequestContract.cs=f5d1d46c2d8aa3654072f9c431adf59a5f6f9d815a24965e2e27519c1914aa95
ItemCombatEffectRequestAssembler.cs=5f6028b9e4801094a540df9889d58a80208ee43cd9d0fcb6f6f117ee373b0313
ShougunuPhase1BattleSandboxVerticalSliceRuntime.cs=496325305715701e553ee2a8b3523045c5d272a1a5adfdc32c24a8b71f681a03
C1ShatteredHostPorcelainHoundRuntimeReport.md=119a0ac4a11630260ffeb003cf007472b0d2a792f6f0141c5c89ea76c53a20bd
Scene_TalismanBag_V04_BattleSandboxPreview.unity=be7e9536573cf7708f4d64961661c1fd60dd275f969e2b7a003a045b0d07306e
```

Any unexplained input mismatch is a safety stop. Exact authorized,
non-overlapping concurrent drift may be attributed once by the B-line Guard;
it must not be restored, reformatted, claimed, or silently absorbed.

## 11. Protected Aggregates

Required before/after:

```text
LOCKED:
  count=15
  aggregate=7e780edae087eb746e54804b2f310da94efc4586d7a94ba5ad1e58ad5e65001f

GOVERNANCE:
  count=9
  aggregate=beb966f88802126703960ca47ef826fa3b5436d222e3a33fd0523668454b15e2

FORMAL_V03_1_10_2_10:
  count=8
  aggregate=97601405b64b17f4dd1ff01fefd36e910fe5bbf50e95a12a41a8d2ca3c99cc7d

PROJECTSETTINGS_READ_ONLY:
  count=21
  aggregate=9e3c370e01deaeb794933fb4d19103d613c5a270b8d9d211c42b4ce8971a76f5
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

The task must preserve the 39 semantic inputs byte-for-byte.

## 12. Exact Write Whitelist

### 12.1 Existing files allowed to modify

```text
NONE
```

### 12.2 Exact new Runtime files

```text
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowPrimitives.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowSession.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowSession.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1NormalEnemyBattleAdapter.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1NormalEnemyBattleAdapter.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BossPhase1CompletionAdapter.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BossPhase1CompletionAdapter.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxRuntimeController.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxRuntimeController.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxOverlay.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1BattleSandboxOverlay.cs.meta
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowValidation.cs
Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowValidation.cs.meta
```

### 12.3 Exact new Editor files

```text
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousBattleIntegrationVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousBattleIntegrationVerifier.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousHandtestMenu.cs
Assets/_Game/Scripts/TalismanBag/Editor/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousHandtestMenu.cs.meta
```

### 12.4 Exact new reports

```text
Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationReport.md
Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationSpec.csv
Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationStageTrace.csv
Docs/V0.4/Reports/BLineChapter1ContinuousBattleRequestResultMatrix.csv
Docs/V0.4/Reports/BLineChapter1Phase1TemporaryClearReport.md
Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationLeakCheckReport.md
Docs/V0.4/Reports/BLineChapter1ContinuousBattleIntegrationManualTest.md
Docs/V0.4/Reports/BLineChapter1ContinuousBattleSurfaceBindingMap.csv
```

Exact totals:

```text
new files=28
existing files modified=0
unexpected files=0
Scene/Prefab/Config/ProjectSettings/Package modifications=0/0/0/0/0
```

## 13. Verification

Compile-safe namespace:

```text
TalismanBag.Editor.V04.ChapterFlow.Chapter1.Integration
```

Required Editor entry:

```text
Menu:
Tools/TalismanBag/V0.4/Verify B-Line Chapter 1 Continuous Battle Integration 01

Batch:
TalismanBag.Editor.V04.ChapterFlow.Chapter1.Integration.V04Chapter1ContinuousBattleIntegrationVerifier.RunBatch
```

Required automated coverage:

1. exact 39-input aggregate and all key hashes;
2. exact 28-path output manifest;
3. existing-file modifications zero;
4. feature default false and manual Editor enable only;
5. exact nine normal-stage bindings and two Runtime profiles;
6. held Bone Swap Remnant zero Runtime/action/copy/Battle rows;
7. pure deterministic 1-1 through 1-10 nominal trace;
8. nine valid `BattleStartRequest` normal rows and one Boss row;
9. nine normal terminal results with `bossDefeated=false`;
10. 1-9 stop and no automatic Boss challenge;
11. one manual Boss challenge;
12. Phase1 terminal adapter and all three exact labels;
13. one Chapter 1 complete transition and one session-only Chapter 2 unlock;
14. zero Chapter 2 Battle starts;
15. result authority fields empty/false and zero Save/Reward/Drop/Inventory
    writes;
16. real Item request type/authority/source-signature consumption;
17. shell-first Porcelain Hound application and Shattered Host HP-only
    application;
18. Enemy scheduler requests resolved at most once;
19. field-area and counter-window execution counts zero;
20. duplicate/stale/wrong-stage/wrong-request/wrong-generation/result
    contradiction rejection without progression;
21. input reversal, invariant culture, deterministic IDs and reports;
22. Editor default-off injection, exact target scene only, `DontSave` root,
    scene-not-dirty, scene SHA unchanged;
23. existing Shougunu vertical slice consumed read-only only at 1-10;
24. old V0.2/V0.3 Enemy/Item/DropTable/executor references zero;
25. tutorial/story executions zero;
26. BA-D3/BA-D4/FALSE_CLONE/2-10 implementations zero;
27. Unity compile and batch verifier PASS;
28. final `CLOSED_CLEAN` and no Git.

Automation must not claim player hand feel or final user acceptance.

## 14. Reports

### Main report

`BLineChapter1ContinuousBattleIntegrationReport.md` must contain:

- Assignment path and dispatched SHA;
- package/schema/workflow/status;
- upstream terminal facts;
- exact 28-path output manifest;
- offline/static and Unity results;
- stage/request/result/transition counts;
- Item/Enemy/Shougunu adapter facts;
- Phase1 temporary-clear labels and replacement boundary;
- session-only unlock evidence;
- zero-authority counters;
- protected before/after aggregates;
- acceptance marker;
- `USER_HANDTEST_READY`, not user PASS.

### Stage trace

`BLineChapter1ContinuousBattleIntegrationStageTrace.csv`:

```text
sequence,stageId,flowPhase,contentId,runtimeProfileId,battleRequestId,resultId,outcome,bossDefeated,completedStageCount,unlockedChapterCount,labels
```

Must contain the full nominal 1-1 through 1-10 trace.

### Request/result matrix

`BLineChapter1ContinuousBattleRequestResultMatrix.csv`:

```text
stageId,requestKind,enemyProfileId,bossProfileId,itemRequestAuthority,resultFact,writeSave,grantReward,rewardRows,dropRows,result
```

### Surface binding map

`BLineChapter1ContinuousBattleSurfaceBindingMap.csv`:

```text
surfaceOrAction,source,access,scenePersisted,formalAuthority,notes
```

### Manual test

`BLineChapter1ContinuousBattleIntegrationManualTest.md` must provide one
continuous user handtest only:

1. use the enable/open menu;
2. enter Play Mode;
3. start the Chapter 1 dev session;
4. prepare the real Item board and clear 1-1 through 1-9 continuously;
5. observe the exact 1-9 stop;
6. manually challenge 1-10;
7. defeat Shougunu Phase1 through the accepted real-item vertical slice;
8. confirm all three temporary-clear labels;
9. confirm Chapter 1 complete and Chapter 2 session-only unlock;
10. exit/re-enter Play Mode and confirm no persistent unlock/reward/drop.

No component-level handtest is allowed before this final route test.

## 15. Acceptance

Automated success must emit exactly:

```text
BLINE_CHAPTER1_CONTINUOUS_BATTLE_INTEGRATION01_AUTOMATED_PASS stages=10 normalBattles=9 bossGates=1 manualBossChallenges=1 phase1TemporaryClears=1 sessionUnlocks=1 saveWrites=0 rewardGrants=0 legacyRefs=0 existingFileModifications=0
```

Failure must emit:

```text
BLINE_CHAPTER1_CONTINUOUS_BATTLE_INTEGRATION01_FAIL errors=<positive integer>
```

Automated PASS changes status to:

```text
AUTOMATED_QA_PASS / USER_HANDTEST_READY
```

The package becomes `PACKAGE_COMPLETE` only after the user completes the one
continuous handtest and explicitly reports PASS.

## 16. Operating Rules

1. Fully read project `AGENTS.md`, every `Docs/LOCKED/*`, mandatory
   ROADMAP/CURRENT/Queues, this Assignment, and all section 10 inputs.
2. Verify the dispatched Assignment full-file SHA before any write.
3. Verify section 10 and 11 hashes and all 28 targets absent.
4. Run one consolidated Guard boundary; do not ask the user about internal
   ProjectSettings/Unity lock lifecycle.
5. Implement and fix simple same-package issues uninterrupted in the same
   task.
6. Unity may run only under a fresh atomic `CLOSED_CLEAN` lease. Do not close
   user processes or delete unknown locks.
7. Do not modify an existing file to make the integration easier.
8. Do not start a second package or Chapter 2 work.
9. Do not send fragmented progress messages to other tasks.
10. At automated terminal status, return one consolidated result and the one
    continuous handtest document.
11. Do not execute any Git command or operation.
