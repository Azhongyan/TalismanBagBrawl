# V0.4-ShougunuPhase1BattleApplicationContract01 Assignment

## 0. Guard Classification

```text
Workflow: COMPLEX_GUARDED_ONCE
Status: BOUNDARY_FROZEN
Owner: NEW_BATTLE_BRIDGE_DEV_THREAD
Package: V0.4-ShougunuPhase1BattleApplicationContract01
Prerequisite Item: V0.4-ItemCombatEffectRequestContract01 / SATISFIED
Prerequisite Enemy: V0.4-ShougunuPhase1RuntimeAndActionContract01 / SATISFIED
Dispatch: AUTHORIZED
```

This package is the Battle-owned application contract for the devOnly Shougunu Phase1 vertical slice.

It must not bind Scene, UI, Visual, or formal route. P3 will do Scene integration after this package self-closes.

## 1. Purpose

Create the bounded Battle application layer that consumes:

```text
ItemCombatEffectRequestSnapshot.v1
ShougunuPhase1RuntimeAndActionContract.v1
```

and produces immutable Battle-owned application/player/display results for:

```text
I007-I012 Lv40 Orange LiHuo item pulses
Shougunu Phase1 shell-first/HP application
Shougunu BasicAttack / Skill1 / Skill2 / Skill3 effect resolution
devOnly player HP fixture 9999/9999
75s nominal trace verification
reset and stale-event rejection
```

## 2. Allowed Write Domains

Runtime code may be added only under:

```text
Assets/_Game/Scripts/TalismanBag/BattleBridge/ShougunuPhase1/
Assets/_Game/Scripts/TalismanBag/Contracts/Battle/
```

Editor verifier/report code may be added only under:

```text
Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/
```

Reports may be added only under:

```text
Docs/V0.4/Reports/
```

Optional assignment/report references may be updated only if required:

```text
Docs/V0.4/ShougunuPhase1BattleApplicationContract01_Assignment.md
```

## 3. Explicitly Forbidden

Do not modify:

```text
Assets/_Game/Scenes/
Assets/_Game/Prefabs/
ProjectSettings/
Packages/
Assets/_Game/Scripts/TalismanBag/Combat/AutoCombatController.cs
RunFlow / MainTrialFlowService / V02RunFlowController / V03NavigationFlowController
SaveData / PlayerPrefs / MainTrialProgressData
RewardService / RewardConfig / DropTable
BossInfoPanel / 1-10 / 2-10 / formal Boss flow
Item tray / placement handfeel / artwork / UI authoring
Enemy contract package files except read-only consumption
Visual runtime or authored Scene UI
```

Do not:

```text
open formal battle route
enter formal flow
write save data
grant rewards
advance chapters
add BuildSettings scenes
create duplicate floating text UI
bind Shougunu_1 or any Scene object
```

## 4. Terminal Upstream Item Schema

Consume exact disk schema from:

```text
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs
```

Required snapshot fields:

```text
ItemCombatEffectRequestSnapshot.schemaId
ItemCombatEffectRequestSnapshot.authorityRevision
ItemCombatEffectRequestSnapshot.status
ItemCombatEffectRequestSnapshot.devOnly
ItemCombatEffectRequestSnapshot.entersFormalBattle
ItemCombatEffectRequestSnapshot.sourceProjectionSetCanonicalSignature
ItemCombatEffectRequestSnapshot.sourceQualifiedProjectionSetIdentity
ItemCombatEffectRequestSnapshot.sourceBindingCanonicalSignature
ItemCombatEffectRequestSnapshot.sourceItemSystemCanonicalSignature
ItemCombatEffectRequestSnapshot.sourceQualifiedBuildCanonicalSignature
ItemCombatEffectRequestSnapshot.sourceCoreRuntimeCanonicalSignature
ItemCombatEffectRequestSnapshot.requestCount
ItemCombatEffectRequestSnapshot.telemetryCount
ItemCombatEffectRequestSnapshot.Requests
ItemCombatEffectRequestSnapshot.UnsupportedTelemetry
ItemCombatEffectRequestSnapshot.ValidationErrors
ItemCombatEffectRequestSnapshot.canonicalSignature
```

Required request row fields:

```text
ItemCombatEffectRequestRow.requestId
ItemCombatEffectRequestRow.requestKind
ItemCombatEffectRequestRow.sourceItemInstanceId
ItemCombatEffectRequestRow.sourceBaseItemId
ItemCombatEffectRequestRow.sourcePlacementId
ItemCombatEffectRequestRow.sourceRarity
ItemCombatEffectRequestRow.sourceRootSeed
ItemCombatEffectRequestRow.sourceFaMenTag
ItemCombatEffectRequestRow.sourceQiLeiTag
ItemCombatEffectRequestRow.SourceEffectIds
ItemCombatEffectRequestRow.baseDamageRawUnits
ItemCombatEffectRequestRow.additiveBasisPoints
ItemCombatEffectRequestRow.resolvedPreMitigationDamageUnits
ItemCombatEffectRequestRow.magnitudeCompleteness
ItemCombatEffectRequestRow.requiresBattleTargetValidation
ItemCombatEffectRequestRow.targetRequestKind
ItemCombatEffectRequestRow.isLitFact
ItemCombatEffectRequestRow.sourceIsCountedFact
ItemCombatEffectRequestRow.qualifiedIsCountedFact
ItemCombatEffectRequestRow.faMenBuildId
ItemCombatEffectRequestRow.faMenBuildCount
ItemCombatEffectRequestRow.faMenActiveStagePieceCount
ItemCombatEffectRequestRow.ActiveCoreEffectIds
ItemCombatEffectRequestRow.sourceProjectionCanonicalSignature
ItemCombatEffectRequestRow.sourcePlacementCanonicalFacts
ItemCombatEffectRequestRow.Contributions
```

Only apply rows where:

```text
snapshot.schemaId == ItemCombatEffectRequestSnapshot.v1
snapshot.status == Valid
snapshot.devOnly == true
snapshot.entersFormalBattle == false
row.requestKind == DirectFlatDamage
row.targetRequestKind == SingleHostileDamageableRuntimeActor
row.requiresBattleTargetValidation == true
row.resolvedPreMitigationDamageUnits > 0
row.isLitFact is accepted-lit truth
row.sourceIsCountedFact / row.qualifiedIsCountedFact remain non-forged accepted truth
```

Unsupported telemetry remains diagnostic only. Do not convert `NotExecuted`, `Unknown`, `Rejected`, or `Conflicted` into damage.

## 5. Terminal Upstream Enemy Schema

Consume exact disk schema from:

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/
```

Required enemy constants:

```text
ShougunuPhase1RuntimeContract.SchemaId = ShougunuPhase1RuntimeAndActionContract.v1
ShougunuPhase1RuntimeContract.ContentId = bone_aspect_boss_c1_bone_guard
ShougunuPhase1RuntimeContract.PhaseId = bone_aspect.phase.shougunu.phase1
ShougunuPhase1RuntimeContract.DevOnly = true
ShougunuPhase1RuntimeContract.IsEnabled = false
ShougunuPhase1RuntimeContract.EntersFormalFlow = false
ShougunuPhase1RuntimeContract.RuntimeBoundToBattle = false
ShougunuPhase1RuntimeContract.MaxHp = 980
ShougunuPhase1RuntimeContract.InitialHp = 980
ShougunuPhase1RuntimeContract.ShellLayerMax = 200
ShougunuPhase1RuntimeContract.MaxSequentialShellLayers = 7
ShougunuPhase1RuntimeContract.CoreExposeDurationTicks = 6000
ShougunuPhase1RuntimeContract.Skill1RepairUnits = 30
ShougunuPhase1RuntimeContract.BasicDamageApplicationInterval = 4
```

Required runtime snapshot fields:

```text
ShougunuPhase1RuntimeSnapshot.SchemaId
ShougunuPhase1RuntimeSnapshot.SchemaVersion
ShougunuPhase1RuntimeSnapshot.EnemyInstanceId
ShougunuPhase1RuntimeSnapshot.ContentId
ShougunuPhase1RuntimeSnapshot.PhaseId
ShougunuPhase1RuntimeSnapshot.DevOnly
ShougunuPhase1RuntimeSnapshot.IsEnabled
ShougunuPhase1RuntimeSnapshot.EntersFormalFlow
ShougunuPhase1RuntimeSnapshot.RuntimeBoundToBattle
ShougunuPhase1RuntimeSnapshot.ResetGeneration
ShougunuPhase1RuntimeSnapshot.Revision
ShougunuPhase1RuntimeSnapshot.Lifecycle
ShougunuPhase1RuntimeSnapshot.MaxHp
ShougunuPhase1RuntimeSnapshot.CurrentHp
ShougunuPhase1RuntimeSnapshot.ShellLayerIndex
ShougunuPhase1RuntimeSnapshot.MaxSequentialShellLayers
ShougunuPhase1RuntimeSnapshot.ShellLayerMax
ShougunuPhase1RuntimeSnapshot.CurrentShell
ShougunuPhase1RuntimeSnapshot.Targetable
ShougunuPhase1RuntimeSnapshot.Vulnerable
ShougunuPhase1RuntimeSnapshot.CoreExposeStartTick
ShougunuPhase1RuntimeSnapshot.CoreExposeEndTick
ShougunuPhase1RuntimeSnapshot.RecoveryAvailable
ShougunuPhase1RuntimeSnapshot.LastAcceptedBattleTick
ShougunuPhase1RuntimeSnapshot.LastAcceptedApplicationSequence
ShougunuPhase1RuntimeSnapshot.AcceptedDamageApplicationCount
ShougunuPhase1RuntimeSnapshot.BasicResolvedCount
ShougunuPhase1RuntimeSnapshot.BasicEarnedCount
ShougunuPhase1RuntimeSnapshot.BasicDebt
ShougunuPhase1RuntimeSnapshot.AcceptedApplicationEventIds
ShougunuPhase1RuntimeSnapshot.ThresholdOccurrences
ShougunuPhase1RuntimeSnapshot.ActiveAction
ShougunuPhase1RuntimeSnapshot.Cues
ShougunuPhase1RuntimeSnapshot.LatestCue
ShougunuPhase1RuntimeSnapshot.Errors
ShougunuPhase1RuntimeSnapshot.DeveloperDiagnostics
ShougunuPhase1RuntimeSnapshot.CanonicalSignature
```

Required action pattern ids:

```text
shougunu.phase1.basic_attack
shougunu.phase1.skill1.shell_repair
shougunu.phase1.skill2.rope_heavy_strike
shougunu.phase1.skill3.ground_seal_burst
```

Required effect request ids:

```text
battle.effect_request.direct_player_damage
enemy.effect_request.repair_current_shell
battle.effect_request.rope_heavy_strike
battle.effect_request.ground_seal_area_burst
```

Required HP thresholds:

```text
S1-A <= 9000 bp -> shougunu.phase1.skill1.shell_repair
S2-A <= 7500 bp -> shougunu.phase1.skill2.rope_heavy_strike
S3-A <= 6000 bp -> shougunu.phase1.skill3.ground_seal_burst
S1-B <= 4500 bp -> shougunu.phase1.skill1.shell_repair
S2-B <= 3000 bp -> shougunu.phase1.skill2.rope_heavy_strike
S3-B <= 1500 bp -> shougunu.phase1.skill3.ground_seal_burst
```

## 6. New Battle-Owned Contracts

Add immutable Battle contracts for this package. Suggested names are exact for this package:

```text
BattleSandboxPlayerCombatantSnapshot
BattleSandboxPlayerEffectApplicationResult
ShougunuPhase1BattleApplicationContext
ShougunuPhase1ItemApplicationResult
ShougunuPhase1EnemyActionApplicationResult
ShougunuPhase1BattleDisplayEvent
ShougunuPhase1BattleApplicationLedger
ShougunuPhase1BattleApplicationTrace
```

These contracts must be immutable/read-only after construction.

### BattleSandboxPlayerCombatantSnapshot

Required V1 fields:

```text
schemaId = BattleSandboxPlayerCombatantSnapshot.v1
devOnly = true
resetGeneration
revision
playerActorId
maxHp = 9999
currentHp
shield
defeated
lastAcceptedEnemyEffectId
acceptedEnemyEffectCount
canonicalSignature
developerDiagnostics
```

Activation/reset must set:

```text
maxHp = 9999
currentHp = 9999
shield = 0 unless Battle-owned test fixture explicitly sets otherwise
defeated = false
```

Player defeat remains valid if 9999 is exhausted. Do not add invulnerability or damage suppression.

## 7. Battle Clock And Item Pulse

The package owns a deterministic devOnly Battle clock:

```text
ticksPerSecond = 1000
itemPulseIntervalTicks = 1500
nominalDurationTicks = 75000
nominalAcceptedItemApplications = 50
```

Pulses must consume the terminal Item request rows in stable order:

```text
I007 -> I008 -> I009 -> I010 -> I011 -> I012 -> repeat
```

Use exact terminal magnitudes from `ItemCombatEffectRequestRealSampleMatrix.csv`:

```text
I007 wb_i007_orange_404310007 P_BOARD_I007 resolvedPreMitigationDamageUnits = 29
I008 wb_i008_orange_404310008 P_BOARD_I008 resolvedPreMitigationDamageUnits = 42
I009 wb_i009_orange_404310009 P_BOARD_I009 resolvedPreMitigationDamageUnits = 53
I010 wb_i010_orange_404310010 P_BOARD_I010 resolvedPreMitigationDamageUnits = 46
I011 wb_i011_orange_404310011 P_BOARD_I011 resolvedPreMitigationDamageUnits = 55
I012 wb_i012_orange_404310012 P_BOARD_I012 resolvedPreMitigationDamageUnits = 76
```

P2 may create synthetic application event ids for verifier fixtures:

```text
nominal.application.001 ... nominal.application.050
```

Runtime ids must still be deterministic and include reset generation and application sequence.

## 8. Target Legality

The only legal Item target is the current Shougunu Phase1 enemy runtime actor:

```text
enemyInstanceId = shougunu.phase1.dev.enemy
contentId = bone_aspect_boss_c1_bone_guard
phaseId = bone_aspect.phase.shougunu.phase1
```

Accept item damage only when enemy snapshot says:

```text
DevOnly == true
EntersFormalFlow == false
Lifecycle allows targetable/vulnerable damage channel
Targetable == true
CurrentHp > 0
ResetGeneration matches Battle context
request id has not already been accepted in the current generation
```

Reject:

```text
duplicate request
stale reset generation
stale revision
invalid target
non-devOnly request
formal request
unknown/unsupported request damage
negative/zero damage
damage channel that does not match enemy lifecycle
```

Rejected, duplicate, stale, and unsupported events must not increment:

```text
AcceptedDamageApplicationCount
BasicEarnedCount
BasicDebt
```

## 9. Damage Application Ordering

Each accepted Item row is converted into:

```text
ShougunuPhase1BattleApplication
```

The Battle layer must split damage as:

```text
if enemy.CurrentShell > 0:
    shellDamageApplied = min(CurrentShell, resolvedPreMitigationDamageUnits)
    hpDamageApplied = remaining damage only if shell reaches zero
else:
    shellDamageApplied = 0
    hpDamageApplied = min(CurrentHp, resolvedPreMitigationDamageUnits)
```

Then call:

```text
ShougunuPhase1RuntimeReducer.ApplyBattleApplication(...)
```

Battle owns the split and must not ask Visual or Scene to decide shell/HP.

## 10. Enemy Action Application

Use:

```text
ShougunuPhase1ActionScheduler.Advance(...)
```

Battle must accept/reject action resolves explicitly through `battleAcceptsResolve`.

Enemy action effects are Battle-owned:

```text
shougunu.phase1.basic_attack
  effectRequestId = battle.effect_request.direct_player_damage
  applies direct player damage

shougunu.phase1.skill1.shell_repair
  effectRequestId = enemy.effect_request.repair_current_shell
  applies enemy shell repair through scheduler/reducer

shougunu.phase1.skill2.rope_heavy_strike
  effectRequestId = battle.effect_request.rope_heavy_strike
  applies direct player damage

shougunu.phase1.skill3.ground_seal_burst
  effectRequestId = battle.effect_request.ground_seal_area_burst
  applies area-burst player damage
```

P2 may choose bounded devOnly player-damage fixture values for readability and survival, but must label them non-formal:

```text
BasicAttackPlayerDamageFixture
Skill2PlayerDamageFixture
Skill3PlayerDamageFixture
```

These fixtures must be low enough that the nominal 75s trace reaches all six HP-threshold skills and 10+ BasicAttack resolutions with player HP starting at 9999.

Enemy must never own or mutate player HP.

## 11. Threshold And Basic Ordering

Battle must preserve upstream ordering:

```text
reactive lifecycle/cue first
threshold actions before BasicAttack
pending illegal threshold actions remain pending
BasicAttack due after every 4 accepted current-generation item damage applications
50 accepted applications -> 12 BasicAttack resolves
second Skill3 must resolve before Defeated
RequestDefeat only after all threshold occurrences are resolved
```

If enemy reaches `DefeatCandidate` while unresolved threshold occurrences remain, Battle must keep terminal ordering safe and must not call final defeat until mandatory actions are resolved.

## 12. Display/Floating Text Event Model

P2 owns only data events. It must not bind UI.

Add immutable display events with enough data for P3 to map onto authored UI:

```text
displayEventId
ledgerEventId
battleTick
resetGeneration
sequence
sourceKind
sourceId
targetActorId
channel
amount
messageKey
priority
aggregationKey
anchorRoute
colorPolicy
playerVisible
developerOnly
```

Required channel values:

```text
ItemToEnemyShellDamage
ItemToEnemyHpDamage
EnemyShellBreak
EnemyCoreExpose
EnemyRecover
EnemyDefeated
EnemyToPlayerDamage
EnemyToPlayerStatusOrArea
EnemySkillIntent
EnemySkillResolved
RejectedOrNotExecutedDeveloperOnly
```

Required anchor routes for later P3 only:

```text
DamageDealtAnchor
ShieldBreakAnchor
DamageTakenAnchor
StatusDamageAnchor
MechanicFloatingText
PlayerHitFeedback
```

P2 must not reference Scene objects or Unity UI components.

## 13. Nominal Trace Requirements

Verifier must reproduce terminal upstream trace facts:

```text
50 accepted item applications
75,000 final battle tick
six HP-threshold skills
12 BasicAttack resolutions
7 shell breaks
S3-B resolved before Defeated
Defeated at 75,000
player max/current starts at 9999/9999
reset restores player 9999/9999 and increments generation
```

Nominal row evidence must include:

```text
application sequence
battle tick
source item id
source item instance id
request id
resolvedPreMitigationDamageUnits
shellDamageApplied
hpDamageApplied
enemy lifecycle
enemy currentHp
enemy currentShell
basic earned/resolved/debt
threshold occurrences
active action id/status
player currentHp
display event count
accepted/rejected reason
```

## 14. Reset And Negative Tests

Required negative fixtures:

```text
duplicate request rejected
stale reset generation rejected
stale enemy revision rejected
wrong target rejected
formal request rejected
non-devOnly request rejected
unsupported telemetry ignored
Unknown/NotExecuted/Conflicted not converted into zero damage
negative damage impossible/rejected
zero damage rejected
BattleResult does not write SaveData
BattleResult does not grant Reward
AutoCombatController not referenced
Scene/Prefab not referenced
Visual not referenced
```

## 15. Reports

Generate:

```text
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractReport.md
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationContractSpec.csv
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationNominalTrace.csv
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationPlayerTrace.csv
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationDisplayEvents.csv
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationNegativeFixtures.csv
Docs/V0.4/Reports/ShougunuPhase1BattleApplicationLeakCheckReport.md
```

Report must state:

```text
classification = COMPLEX_GUARDED_ONCE
package status
terminal item schema consumed
terminal enemy schema consumed
sceneTouched = false
visualTouched = false
formalRouteTouched = false
saveTouched = false
rewardTouched = false
autoCombatTouched = false
```

## 16. Verification

Required:

```text
Unity compile pass
Editor verifier pass
git diff --check pass for scoped files
static forbidden reference scan pass
```

Forbidden reference scan must cover:

```text
AutoCombatController
MainTrialFlowService
V02RunFlowController
V03NavigationFlowController
SaveData
PlayerPrefs
RewardService
RewardConfig
DropTable
BossInfoPanel
Scene_TalismanBag_V04_BattleSandboxPreview
UnityEngine.UI
GameObject.Find
FindObjectOfType
Resources.Load
```

`UnityEngine` may be used only if existing math/time primitives require it; prefer pure data C#.

## 17. Completion Rule

This component package may self-close on automated QA if all verifier/report checks pass.

No user handtest is required for P2 because there is no Scene or visual binding.

After P2 self-closes:

```text
P3 V0.4-RealItemDamageShougunuPhase1BattleSandboxVerticalSlice01
```

becomes immediately next and must freeze a fresh task-start Scene baseline before touching:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

## 18. Development Thread Requirement

Must use:

```text
NEW_BATTLE_BRIDGE_DEV_THREAD_REQUIRED
```

Do not reuse:

```text
Item tray dev task
ItemCombatEffectRequestContract01 task
ShougunuPhase1RuntimeAndActionContract01 task
Visual prototype task
```

## 19. Final Delivery Format

The dev task must return:

```text
package status
files added/modified
exact verifier result
nominal trace summary
negative fixture summary
leak check summary
redline confirmation
git status scoped summary
commit/tag/push: no unless user explicitly asks
```
