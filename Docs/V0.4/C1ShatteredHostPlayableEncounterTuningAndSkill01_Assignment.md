# V0.4-C1ShatteredHostPlayableEncounterTuningAndSkill01 Assignment

## 1. Package identity

```text
Workflow:
COMPLEX_GUARDED_ONCE

Package:
V0.4-C1ShatteredHostPlayableEncounterTuningAndSkill01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_C1SHATTEREDHOSTPLAYABLEENCOUNTERTUNINGANDSKILL01

Primary owner:
Enemy Runtime

Supporting owners:
Battle clock / application ledger = read-only external owner
Item damage request = read-only external owner
Presentation = later consumer only

User handtest:
NOT_APPLICABLE for this component package

Next integration package:
NOT_STARTED by this package
```

This package corrects the devOnly Chapter 1 first-enemy runtime data so that the
current authoritative Lv40 LiHuo I007-I012 damage fixture produces an observable
encounter instead of defeating the enemy before its first action.

The package owns only Shattered Host Enemy runtime truth, action scheduling,
validation, deterministic fixtures, and reports. It does not own the shared
Battle cadence, Item damage, presentation playback, or Chapter flow.

## 2. Accepted evidence and superseded fixture

Accepted current evidence:

```text
Item damage rows in Ordinal base-item order:
I007=29
I008=42
I009=53
I010=46
I011=55
I012=76

Current old shared Battle cadence:
500 ticks

Current Shattered Host fixture:
MaxHp=160
Shell=0
Basic firstDue/repeat=2000/4000
Skill action count=0
```

The old `MaxHp=160`, `500-tick damage cadence`, and one-Basic-only trace are
retained only as superseded dev smoke-test evidence. They are not acceptable
playable tuning.

This package freezes a `1500`-tick cadence as a deterministic external Battle
fixture input. It must not edit
`V04Chapter1ContinuousBattleIntegrationContract.ItemApplicationCadenceTicks`.
The active integration owner may consume the accepted component output later.

## 3. Frozen playable tuning

### 3.1 Shattered Host profile

```text
contentId:
bone_aspect_enemy_c1_01_shattered_host

runtimeProfileId:
bone_aspect.runtime.c1.shattered_host.v1

MaxHp:
650

ShellMax / InitialShell / regeneration:
0 / 0 / 0

Item application cadence fixture:
1500 ticks

Player HP fixture:
9999

Death presentation hold recommendation:
1500 ticks

Expected lethal tick:
21000

Expected visible occupancy end:
22500
```

`DeathPresentationHoldRecommendationTicks=1500` is a devOnly read-only
presentation recommendation. Enemy defeat truth occurs immediately at tick
`21000`. The hold must never delay defeat, Battle result construction, damage
acceptance, or Chapter truth, and animation completion must never release or
confirm gameplay.

### 3.2 Porcelain Hound regression freeze

Porcelain Hound remains byte-semantically unchanged:

```text
MaxHp=180
ShellMax/InitialShell=100/100
actionId=c1.porcelain_hound.charge_attack
firstDue/repeat=2500/6000
telegraph/cast/resolve/recover=800/400/1200/900
effectRequest=battle.effect_request.charge_attack
```

No Porcelain Hound rebalance, new Skill, visual change, or Battle behavior is
authorized.

## 4. Frozen Shattered Host actions

### 4.1 BasicAttack

```text
actionId:
c1.shattered_host.basic_attack

cue:
BasicAttack

mechanicKey:
mechanic.basic_pressure

effectRequestKey:
battle.effect_request.direct_player_damage

firstDueTick:
2000

repeatIntervalTicks:
5000

telegraphTicks / castTicks / resolveOffsetTicks / recoverTicks:
500 / 300 / 800 / 700

priority:
100
```

### 4.2 Skill

```text
actionId:
c1.shattered_host.polluted_pulse_skill

cue:
Skill

mechanicKey:
mechanic.polluted_tile

effectRequestKey:
battle.effect_request.devonly_skill_signal

firstDueTick:
5500

repeatIntervalTicks:
9000

telegraphTicks / castTicks / resolveOffsetTicks / recoverTicks:
750 / 500 / 1250 / 750

priority:
200

effect execution:
NOT_IMPLEMENTED / NOT_CONTENT_FINAL / OBSERVATION_ONLY
```

The Skill is genuine Enemy-owned action data with its own action identity, cue,
telegraph/cast/resolve/recovery timing, monotonic execution identity, and a
neutral Battle request. This package does not infer bind, hazard, DoT, player
damage, placement mutation, or field ownership. The later Battle integration may
observe the request exactly once but must not invent an effect.

## 5. Runtime schema and scheduler refinement

The accepted v1 component is refined to
`BoneAspectC1EnemyRuntime.v2 / SchemaVersion=2`.

The change must preserve all public v1 fields needed by current read-only
consumers while adding the minimum immutable scheduling facts:

```text
C1EnemyActionPatternSnapshot:
- CueKind
- EffectRequestLifetimeClass
- RequestedDurationTicks

C1EnemyActionScheduleStateSnapshot:
- ActionPatternId
- NextDueTick
- ExecutionCount
- LastStartTick

C1EnemyRuntimeSnapshot:
- ActionScheduleStates (Ordinal by ActionPatternId)
- NextActionDueTick remains a compatibility projection of the minimum due tick
```

Required scheduler rules:

1. Each action owns an independent `NextDueTick`; one profile may have multiple
   action patterns.
2. At most one action is active at a time.
3. Due actions are selected by earliest `NextDueTick`, then higher `Priority`,
   then `ActionPatternId` using `StringComparer.Ordinal`.
4. A temporarily occupied action remains due; it is not dropped or shifted by
   Hit cues.
5. `reactiveCuePending=true` rejects the scheduler advance without consuming a
   due action or mutating schedule progress.
6. Scheduler time is monotonic. A call may deterministically close all elapsed
   phases of the current action before selecting the next legal due action.
7. Resolve requests use the authoritative `ResolveTick`, not the frame, call
   time, animation frame, animation callback, or presentation completion.
8. Each execution emits exactly one action cue and at most one effect request.
9. `NextDueTick += RepeatIntervalTicks` is based on that action's own authored
   cadence, not on another action's completion time.
10. Defeat immediately cancels the active action and forbids later starts or
    requests.
11. Reset increments generation, clears execution/dedupe state, and restores
    every action's initial due tick relative to the reset tick.
12. Existing Battle application event, cue, request, execution, revision, reset,
    and canonical identities remain monotonic and deterministic.

`C1EnemyCueKind.Skill` must be appended with an explicit stable numeric value.
Existing enum values must not be renumbered.

The scheduler API remains callable from the current adapter, but the deterministic
fixture must advance it at authoritative action milestones as well as damage
pulses. The later integration owner is responsible for advancing the scheduler
from the Battle clock independently of Item pulse delivery.

## 6. Frozen nominal trace

The deterministic test uses damage values
`29,42,53,46,55,76` repeatedly at `1500`-tick intervals.

Expected damage trace:

```text
tick   event  offered  acceptedHpDelta  cumulative  currentHp
1500   1      29       29               29          621
3000   2      42       42               71          579
4500   3      53       53               124         526
6000   4      46       46               170         480
7500   5      55       55               225         425
9000   6      76       76               301         349
10500  7      29       29               330         320
12000  8      42       42               372         278
13500  9      53       53               425         225
15000  10     46       46               471         179
16500  11     55       55               526         124
18000  12     76       76               602         48
19500  13     29       29               631         19
21000  14     42       19               650         0
```

Expected resolved actions:

```text
Basic1: start=2000  resolve=2800  recoveryEnd=3500
Skill1: start=5500  resolve=6750  recoveryEnd=7500
Basic2: start=7500  resolve=8300  recoveryEnd=9000
Basic3: start=12000 resolve=12800 recoveryEnd=13500
Skill2: start=14500 resolve=15750 recoveryEnd=16500
Basic4: start=17000 resolve=17800 recoveryEnd=18500
Defeated: tick=21000
Death hold recommendation end: tick=22500
```

Required checkpoint rows:

```text
15s:
Alive, HP=179, events=10, acceptedDamage=471,
BasicResolved=3, SkillResolved=1

18s:
Alive, HP=48, events=12, acceptedDamage=602,
BasicResolved=4, SkillResolved=2

20s:
Alive, HP=19, events=13, acceptedDamage=631,
BasicResolved=4, SkillResolved=2

22s:
Defeated, HP=0, events=14, acceptedDamage=650,
rawOfferedDamage=673, DeathHoldActive=true

25s:
Defeated, HP=0, events=14, acceptedDamage=650,
rawOfferedDamage=673, DeathHoldActive=false
```

The report must also include a `15/18/20/22/25` second acceptance row proving
the nominal lethal time is `21.0s`, inside nominal `18-22s` and hard
`15-25s`.

## 7. Presentation and integration boundary

This component emits read-only Enemy cues only:

```text
Idle:
steady Active state; no separate gameplay action

BasicAttack:
from the Basic action pattern

Skill:
from the new Skill action pattern

Hit:
only after an accepted current-generation Battle application with positive
actual delta

Death:
Defeated cue at authoritative lethal application
```

The current ordinary-enemy V1 visual contract has exactly five top-level
presentation states: Idle, BasicAttack, Skill, Hit, Death. Presence is deferred
and is not an asset requirement. This package may retain the existing runtime
Presence cue for compatibility, but does not create a Presence visual state.

Presentation may arbitrate brief Hit visibility against an action clip, but it
does not reschedule, resolve, cancel, or confirm the action. No animation
completion callback may alter Enemy or Battle truth.

## 8. Exact write whitelist

### 8.1 Existing files allowed to change: 15

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeReducer.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyActionScheduler.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1ShatteredHostPorcelainHoundRuntimeVerifier.cs
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeReport.md
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeSpec.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFieldMatrix.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeEnemyProfiles.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeActionPattern.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeFixtureTrace.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeNegativeFixtureRows.csv
Docs/V0.4/Reports/C1ShatteredHostPorcelainHoundRuntimeLeakCheckReport.md
```

### 8.2 New files allowed: 6

```text
Docs/V0.4/Reports/C1ShatteredHostPlayableEncounterTuningAndSkillReport.md
Docs/V0.4/Reports/C1ShatteredHostPlayableEncounterTuningAndSkillCheckpoints.csv
Docs/V0.4/Reports/C1ShatteredHostPlayableEncounterTuningAndSkillTrace.csv
Docs/V0.4/Reports/C1ShatteredHostPlayableEncounterTuningAndSkillActionSchedule.csv
Docs/V0.4/Reports/C1ShatteredHostPlayableEncounterTuningAndSkillNegativeFixtureRows.csv
Docs/V0.4/Reports/C1ShatteredHostPlayableEncounterTuningAndSkillLeakCheckReport.md
```

All `.meta` files are frozen. Existing-file modifications outside the 15 paths
and additions outside the 6 paths are forbidden.

## 9. Exact task-start write baselines

The developer must verify these hashes before the first write:

```text
C1EnemyRuntimePrimitives.cs
a6eeb38da79d77311b8f947f2adcf22db14aab2a9af6342a74b4a0dd0b7fb599

C1EnemyRuntimeSnapshots.cs
33e976f8f66a1e85578c912f741a76a4fbd11461e6498d89851076b71ddc1b74

C1EnemyRuntimeCatalog.cs
a5cd42cdb40f0e5945e00d8e6a23f0d09669997d2dbe19a163f18b7e4abebca4

C1EnemyRuntimeReducer.cs
b7aac7d6e033212d4f3d2b242d663eb604104c7db5edaabe442e8b1c51f4ae17

C1EnemyActionScheduler.cs
9c3994f0f2b2bd48431e7255dfd93e1ba57ccf4decc3e670d0d426dbe837d822

C1EnemyRuntimeValidation.cs
bf96a3c890e6e35b99861e011d6acca419183d3cf8c1630a6b77751b72f88a33

C1ShatteredHostPorcelainHoundRuntimeVerifier.cs
5e78224bbb4c421eee000724d6fc21af6726f70e9119ddb1f0a3d5d6f8979a8a

C1ShatteredHostPorcelainHoundRuntimeReport.md
119a0ac4a11630260ffeb003cf007472b0d2a792f6f0141c5c89ea76c53a20bd

C1ShatteredHostPorcelainHoundRuntimeSpec.csv
84f9d5d6a0a1d5b6f88f77db863e5f997bcab40e99cdeca9710b492e7c23249a

C1ShatteredHostPorcelainHoundRuntimeFieldMatrix.csv
cd95161f2043c543677560e28d86ab0b74ca8af97b0785bd942d76f163727b5b

C1ShatteredHostPorcelainHoundRuntimeEnemyProfiles.csv
c5fe946569f9df69de8807122ebc8849ad697360821b4cecfed277bc0ee15f0a

C1ShatteredHostPorcelainHoundRuntimeActionPattern.csv
9b59ade496e3a8014e5401662353cc653fce6194606184208346d5508f96eaf3

C1ShatteredHostPorcelainHoundRuntimeFixtureTrace.csv
70885373d149069c51380050a63e226b89e669130bdd4fb0129a73122594453c

C1ShatteredHostPorcelainHoundRuntimeNegativeFixtureRows.csv
af9d3b2780090f1839fba9e56c72037f32905f846aa5d366990096cae5ce3934

C1ShatteredHostPorcelainHoundRuntimeLeakCheckReport.md
107aff25e4bb7677db0af31bdd1916f80995a7e68f386e0653fc35b1e91caf46
```

The six new reports must all be absent before the first write.

Any mismatch in the exact 15 write baselines or presence of a new target is a
real overlap blocker. Stop without rebaselining or restoring.

## 10. Read-only input evidence

```text
Docs/V0.4/Reports/ItemCombatEffectRequestRealSampleMatrix.csv
cce895139d73784fab1ec48016ce067d8d5f258df31e036d346542cb03a7d6b8

Docs/V0.4/Reports/ItemCombatEffectRequestContractReport.md
3a1977f5fddd88b2556f7dc43102ac06f5a8fcd6d9fb08d2c97ce50a675e0ac1

Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1ContinuousFlowPrimitives.cs
d6ae133f0f934576235cdbf3285031f3eded88c460920a700a1732df3e121543

Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/Integration/V04Chapter1NormalEnemyBattleAdapter.cs
f4c50dada6ff4c2fefc399a470ce44dead4365027abb2ab74c3547810ae8f1d3
```

The Item sources are frozen inputs. The two integration files belong to an
independent active task and are read-only evidence only. External changes to
those two integration files do not authorize this package to rebaseline or edit
them; record the final consumed version in the report.

## 11. Required verifier coverage

Use the compile-safe existing entry:

```text
TalismanBag.EditorTools.EnemySystem.C1ShatteredHostPorcelainHoundRuntimeVerifier.RunFromCommandLine
```

The verifier must cover:

1. v2 schema and compatibility projection.
2. Shattered Host HP `650`, no shell, no formal/live balance flags.
3. Exact two Shattered Host action patterns and one unchanged Hound pattern.
4. Exact Basic and Skill fields from this Assignment.
5. Immutable, defensive-copy, Ordinal action schedule state.
6. Independent due counters and deterministic collision ordering.
7. Resolve request emitted once at authoritative resolve tick.
8. Reactive cue rejection preserves all schedule progress.
9. Large tick jump does not duplicate or drop due action milestones.
10. Accepted Hit only for accepted positive current-generation application.
11. Duplicate, stale, rejected, zero-delta, wrong-target, and post-defeat inputs
    emit no Hit and do not advance damage/event state.
12. Reset generation restores both action schedules and dedupe identity.
13. Defeat at tick `21000`, no post-defeat action request.
14. Exact `15/18/20/22/25` checkpoint rows.
15. Exact 14 accepted damage applications, raw offered `673`, accepted `650`.
16. Exact 4 resolved Basic requests and 2 resolved Skill requests before defeat.
17. Death hold is presentation-only and does not gate Battle truth.
18. Porcelain Hound current HP/shell/action/counter-window regression unchanged.
19. BA-D3 hold remains `1`; Runtime/Action/Copy/Binding remain `0/0/0/0`.
20. Runtime/Battle binding, formal flow, reward, drop, save, inventory, Item
    mutation, Scene, Prefab, and UI writes remain `0`.
21. Existing and new reports regenerate deterministically, including one
    missing-report regeneration fixture.
22. Canonical signature changes for every new scheduling field and remains
    stable under culture/input-order reversal.
23. Exact whitelist, UTF-8/LF/no BOM/no trailing whitespace, GUID and leak
    checks.

Required pass marker:

```text
C1_SHATTERED_HOST_PLAYABLE_TUNING_AND_SKILL01_PASS hp=650 cadence=1500 basic=4 skill=2 lethal=21000 battleWrites=0 itemWrites=0
```

## 12. Unity and concurrency

At Assignment freeze time, an external Unity Editor and ShaderCompiler lifecycle
is active. This does not block code/offline work.

The developer must not:

- close or kill external Unity processes;
- delete or replace another owner's `UnityLockfile` or `EditorInstance.json`;
- start Unity batch while any external lifecycle or lock exists;
- run a Builder or save a Scene/Prefab.

Run the final Unity compile/verifier only after a fresh atomic `CLOSED_CLEAN`
check. If serialization remains occupied after offline QA, stop once at
`UNITY_QA_HOLD`; do not retry repeatedly or disturb the owner.

The active C1 integration/presentation task may continue writing its own
ChapterFlow/BattleBridge/Presentation files. Those paths are not protected by a
broad aggregate here. This package must not touch them.

## 13. Hard prohibitions

Do not modify:

- Item values, Item request assembly, Build qualification, I031, or damage rows;
- shared Battle cadence or the normal-enemy Battle adapter;
- player HP/damage receiver implementation;
- Scene, Prefab, BuildSettings, ProjectSettings, UI, or visual assets;
- Porcelain Hound tuning or semantics;
- Bone Swap Remnant, BA-D3, BA-D4, Boss, Phase2/3, summon, split, or copy;
- Reward, Drop, SaveData, Inventory, Chapter progress, formal 1-10/2-10,
  RunFlow, BossInfo, BossReward, or RunResult;
- `AGENTS.md`, `Docs/LOCKED/*`, Package Queues, or Guard rules.

No commit, tag, push, reset, rollback, stage, clean, or broad formatting.
Do not start the integration follow-up or any next package.

## 14. Completion

This internal component closes on automated QA PASS; no component-level user
handtest is required. The real user handtest remains owned by the active
continuous Chapter 1 integration package after it consumes:

```text
- accepted Enemy v2 runtime/profile/action output;
- shared Battle cadence=1500;
- independent Battle-clock action advancement;
- read-only Skill cue/request;
- Death hold recommendation.
```

Final response must be one consolidated
`TASK_STATUS_SYNC_TO_GUARD_REPOOPS` containing:

- exact modified/new files;
- offline/Unity results;
- exact trace and checkpoint counts;
- old fixture supersession evidence;
- Porcelain Hound and BA-D3 regression;
- final process/lock state;
- no Git and no next package.
