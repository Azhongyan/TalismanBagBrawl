# V0.4-BattleSnapshotAdapter01 Assignment

Status: `UNITY_QA_PASS_CANDIDATE / GUARD_HOLD_STABLE_BASELINE_UNTIL_REPOOPS_AND_EXPLICIT_TAG_APPROVAL`

Maintainer: Codex Guard / boundary window

Date: 2026-07-07

## 0. Authority

Authoritative boundaries:

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
Docs/V0.4/UnifiedBattlePageStrategy_GuardSync.md
Docs/V0.4/BUILD_SANDBOX_INTEGRATION_STRATEGY.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
User latest explicit instruction
```

This assignment records Guard scope for `V0.4-BattleSnapshotAdapter01`. It does not authorize code, scene, UI, BuildSettings, save, reward, boss, chapter, commit, tag, or push changes outside the scope below.

## 1. Package Goal

`V0.4-BattleSnapshotAdapter01` is a pure data contract and read-only adapter package between the V0.2 / V0.3 stable battle flow and V0.4 BuildSandbox.

The package goal is to establish a neutral battle data language for future `UnifiedBattlePage` work:

```text
V0.2 / V0.3 stable flow
-> BattleContract / Snapshot / Adapter
-> V0.4 BuildSandbox data modules
-> future UnifiedBattlePage shell
```

This package must not integrate V0.4 sandbox output into formal gameplay. It must only define contracts, read-only exporters / normalizers, result snapshots, field split rules, and validation reports.

## 2. Allowed File Types

Allowed new pure C# data-layer files:

```text
Assets/_Game/Scripts/TalismanBag/Contracts/Battle/...
```

Recommended namespace:

```csharp
TalismanBag.Contracts.Battle
```

Reason:

```text
Contracts/Battle is a neutral boundary layer.
It avoids making V0.4 depend on V0.3 internals as a formal owner.
It avoids making V0.3 formal flow depend on V0.4 sandbox modules.
```

Allowed pure data / read-only adapter categories:

```text
Data contract DTOs
Read-only exporter classes
Read-only normalizer classes
Sandbox result mapper classes
Validation-only report generator classes
Editor-only or batch-only report tools
```

Allowed report outputs:

```text
Docs/V0.4/Reports/BattleSnapshotAdapterReport.md
Docs/V0.4/Reports/BattleSnapshotFieldMap.csv
Docs/V0.4/Reports/BattleSnapshotPlayerDevFieldSplit.csv
Docs/V0.4/Reports/BattleSnapshotAdapterLeakCheckReport.md
```

Allowed code behavior:

```text
Read existing config/state through public or already-safe read-only APIs.
Map legacy single-cell layout to neutral snapshot data.
Normalize V0.4 sandbox layout into neutral snapshot data.
Map Build evaluation outputs into player-visible and developer-only layers.
Map sandbox battle result into a devOnly result snapshot.
Emit validation warnings and reports.
```

## 3. Forbidden Scope

Strictly forbidden:

```text
Do not develop UnifiedBattlePage.
Do not create or modify scenes.
Do not modify UI, Prefab, Canvas, RectTransform, authored layout, or hand-tuned scene objects.
Do not modify ProjectSettings or BuildSettings.
Do not modify RunFlow, V02RunFlowController, PageState, FormationState, AutoCombatController, V02FormationGridFrame, DamageText, BossInfoPanel.
Do not modify SaveData, PlayerPrefs, MainTrialProgressData, RewardService, RewardConfig, DropTable, StageConfig, EnemyConfig, BossConfig, UpgradeConfig, formal numeric configs.
Do not change boss trigger rules, boss reward rules, chapter progression, formal 1-10 / 2-10, formal 3-10 / 4-10, or formal trial entry.
Do not make V0.4 sandbox win / lose write formal save data.
Do not make V0.4 sandbox result grant formal rewards.
Do not make V0.4 devOnly enemies, bosses, problems, DropBias, Build tasks, or chapters enter formal flow.
Do not enable formal FeatureFlags by default.
Do not mix in ItemRarity formal assignment, formal drop, acquisition, washing, or formal Item system work.
Do not modify AGENTS.md or Docs/LOCKED/*.
Do not commit, tag, push, reset, or rollback.
```

If any implementation requires one of the forbidden files or behaviors, the development window must stop and return to Guard.

## 4. Minimal BattleContract Fields

### 4.1 BattleStartRequest

Purpose: describe a battle request without starting battle.

Minimum fields:

```text
requestId
chapterId
stageId
roundId
isBossStage
enemyProfileId
bossProfileId
entrySource
allowedItemRosterId
formalFlow
devOnly
```

Developer diagnostics:

```text
sourceRoute
sourceScene
sourceController
seedId
validationWarnings
```

### 4.2 BattleLayoutSnapshot

Purpose: describe board, layout, item placement, energy state, and blocked / locked cells.

Minimum fields:

```text
snapshotId
boardId
gridWidth
gridHeight
placedItems
formationEyes
energySources
blockedCells
lockedCells
currentEnergyState
createdAtFrame
devOnly
```

Developer diagnostics:

```text
sourceAdapter
legacySingleCell
energyDiagnostics
validationWarnings
```

### 4.3 BattleItemSnapshot

Purpose: describe one placed or roster item in a neutral battle snapshot.

Player-visible fields:

```text
itemInstanceId
itemId
displayName
familyId
baseItemId
shapeId
rotationIndex
anchorCell
occupiedCells
itemStats
affixes
rarity
energyState
connectedEyeId
sourceContainer
synergyTags
```

Developer-only fields:

```text
runtimeId
sourceDataPath
fullRolls
energyRoleViolation
developerOnlyDiagnostics
validationWarnings
```

Legacy single-cell mapping rule:

```text
shapeId = Single1
rotationIndex = 0
anchorCell = gridPosition
occupiedCells = [gridPosition]
legacySingleCell = true
```

This mapping must not require changing the formal V0.2 / V0.3 grid.

### 4.4 BattleEnemySnapshot

Purpose: describe enemy / boss readable state without changing enemy behavior.

Player-visible fields:

```text
enemyId
bossId
hp
shield
attackDamage
attackInterval
castSkillId
castProgress
mechanicTags
weaknessHints
```

Developer-only fields:

```text
devOnlyProfileId
validationTargetBuilds
exactWeaknessTiming
bossKeyRequirements
developerOnlyDiagnostics
```

### 4.5 BattleBuildSnapshot / BuildEvaluationSnapshot

Purpose: describe Build, synergy, affix, readiness, and modifier evaluation output.

Player-visible fields:

```text
activeSynergies
readinessSummary
modifierBundleSummary
eventBundleSummary
playerVisibleHints
recommendedAction
```

Developer-only fields:

```text
shapeBuildRules
problemReadinessFullAnswer
bossSixKeyFullAnswer
hardSolutionTags
requiredSynergy
requiredAffix
requiredStats
dropBiasWeights
sourceDataPath
developerOnlyDiagnostics
```

### 4.6 BattleResultSnapshot

Purpose: describe battle or sandbox result data without committing formal result effects.

Minimum fields:

```text
resultId
requestId
resultType
win
lose
abandon
roundId
bossDefeated
durationSeconds
chapterProgressDelta
rewardPreview
itemDrops
buildPerformanceSummary
eventSummary
rewardClaimToken
nextRouteHint
devOnly
shouldWriteSave
shouldGrantReward
```

Mandatory default for V0.4 sandbox outputs:

```text
devOnly = true
shouldWriteSave = false
shouldGrantReward = false
```

Guard recommendation:

```text
BattleResultSnapshot is result data, not a commit command.
Formal save / reward / chapter commit authority stays with V0.3 formal flow.
If a commit policy type is needed later, it must be owned by formal flow, not by V0.4 sandbox.
```

## 5. Player Field / Developer Diagnostics Split

Player-visible fields may describe what the player can reasonably infer:

```text
displayName
shape / occupied cell visual result
energyState as masked or readable status
active synergy names or summary
readinessSummary
playerVisibleHints
recommendedAction
mechanic hint text
nextRouteHint player text
```

Developer-only diagnostics may contain exact answers or tuning data:

```text
hardSolutionTags
requiredSynergy
requiredAffix
requiredStats
DropBias weights
Boss six-key full answer
problemReadinessFullAnswer
exactWeaknessTiming
validationTargetBuilds
full rolls
source data paths
raw config ids
leak check details
```

Rules:

```text
Developer-only diagnostics can appear in dev panels, reports, debug logs, and devOnly objects only.
Developer-only diagnostics must not appear in future player BattlePage UI, BossInfoPanel text, formal reward UI, or formal chapter UI.
Player-visible fields must be able to exist without revealing exact solution answers.
```

## 6. Suggested Read-only Adapter Split

Recommended implementation split for the development window:

```text
Task 01: Contracts/Battle data structures.
Task 02: V0.3 / V0.2 read-only BattleStartRequest and BattleLayoutSnapshot exporters.
Task 03: Legacy Single1 normalizer for V0.3 single-cell layout.
Task 04: V0.4 BuildSandbox layout normalizer.
Task 05: BuildEvaluationSnapshot exporter with player/developer split.
Task 06: SandboxBattleResult mapper with devOnly and no-commit defaults.
Task 07: Validation report generator.
```

Allowed adapter names are implementation details, but recommended names are:

```text
V03BattleStartRequestExporter
V03BattleLayoutSnapshotExporter
V04BattleLayoutNormalizer
BuildEvaluationSnapshotExporter
SandboxBattleResultMapper
BattleContractFieldValidator
```

All adapters must be read-only. They must not start battles, open pages, modify flow state, save data, grant rewards, or commit placement.

## 7. Formation Energy Compatibility

Short-term rule:

```text
FormationPowerResolver continues to serve V0.2 / V0.3 formal battle.
FormationEnergyContract continues to serve V0.4 BuildSandbox.
BattleContract only provides neutral read-only expression.
```

Allowed neutral fields:

```text
energyState
connectedEyeId
energySourceId
energyDiagnostics
```

Rules:

```text
energyState is the authority field.
isPowered, if present for compatibility, must be derived from energyState == Powered.
energyDiagnostics is developer-only by default.
FormationEnergyContract must not replace FormationPowerResolver in this package.
```

## 8. Validation Report Requirements

The package must output at least:

```text
Docs/V0.4/Reports/BattleSnapshotAdapterReport.md
Docs/V0.4/Reports/BattleSnapshotFieldMap.csv
Docs/V0.4/Reports/BattleSnapshotPlayerDevFieldSplit.csv
Docs/V0.4/Reports/BattleSnapshotAdapterLeakCheckReport.md
```

Reports must prove:

```text
BattleContract types exist in a neutral contract layer.
V0.3 single-cell layout maps to Single1 without changing formal grid.
V0.4 multi-cell layout can normalize into BattleLayoutSnapshot.
BuildEvaluation output splits player-visible fields from developer-only diagnostics.
Sandbox BattleResult defaults to devOnly=true.
Sandbox BattleResult defaults to shouldWriteSave=false.
Sandbox BattleResult defaults to shouldGrantReward=false.
No formal SaveData / PlayerPrefs / MainTrialProgressData writes.
No RewardService / RewardConfig / DropTable writes.
No RunFlow / V02RunFlowController / AutoCombatController / PageState / FormationState writes.
No BossInfoPanel / boss trigger / boss reward changes.
No scene / prefab / UI / BuildSettings changes.
No formal FeatureFlag default enable.
No devOnly enemy / boss / chapter / DropBias leak into formal flow.
```

Completion report must include:

```text
New file list
Modified file list
Whether RunFlow was modified
Whether AutoCombatController was modified
Whether BossInfoPanel was modified
Whether SaveData was modified
Whether RewardService was modified
Whether Scene / Prefab was modified
Whether BuildSettings was modified
Whether sandbox result defaults are devOnly=true / shouldWriteSave=false / shouldGrantReward=false
Report output paths
git status
Known risks
```

## 9. Smoke / QA Expectations

This package does not need to run or alter formal battle runtime as its primary smoke. It must run data validation and leak checks.

Required validation:

```text
V0.3 single-cell sample can export or be represented as BattleLayoutSnapshot.
V0.3 item maps to Single1 / rotation 0 / one occupied cell.
V0.4 multi-cell sample can normalize into BattleLayoutSnapshot.
BuildEvaluation sample splits player summary and developer diagnostics.
SandboxResult sample is devOnly=true.
SandboxResult sample has shouldWriteSave=false.
SandboxResult sample has shouldGrantReward=false.
Adapter leak check finds no formal writes or forbidden references.
```

Guard note:

```text
Data validation does not replace V0.2 / V0.3 Golden Path QA for formal integration.
Because this package must not touch formal flow, it should report that Golden Path was not changed rather than manufacturing formal battle pass state.
Formal Golden Path regression remains required before any future formal integration package.
```

## 10. UnifiedBattlePageShell01 Prerequisites

`V0.4-UnifiedBattlePageShell01` may start only after all of the following are true:

```text
V0.4-BattleSnapshotAdapter01 is dev complete.
BattleContract data types exist in neutral Contracts/Battle layer.
BattleStartRequest / BattleLayoutSnapshot / BattleItemSnapshot / BattleEnemySnapshot / BattleBuildSnapshot / BattleResultSnapshot field report passes.
Player-visible and developer-only field split report passes.
Adapter leak check = 0 forbidden formal writes.
Sandbox result defaults verified: devOnly=true, shouldWriteSave=false, shouldGrantReward=false.
No scene / UI / BuildSettings changes.
No RunFlow / SaveData / Reward / Boss / chapter progression changes.
User confirms the package passes or approves moving forward.
TASK_STATUS_SYNC_TO_GUARD_REPOOPS is sent or a manual forward package is generated.
```

Even after these prerequisites pass, `UnifiedBattlePageShell01` is only allowed to create / define a shell. It must not replace `Scene_TalismanBag_V02_FormationCounter`, must not connect formal chapter entry, and must not write save, reward, or chapter progress.

## 11. Guard Completion Intake

Intake date: `2026-07-07`

Package status:

```text
DEV_DONE_SOURCE_STATIC_CANDIDATE
GUARD_STATIC_PASS_BATTLE_SNAPSHOT_ADAPTER01
USER_REPORTED_UNITY_QA_PASS_CANDIDATE
GUARD_ACCEPTED_QA_PASS_CANDIDATE
UNITY_COMPILE_NOT_VERIFIED_BY_GUARD_SHELL
EDITOR_MENU_REPORT_NOT_VERIFIED_BY_GUARD_SHELL
GOLDEN_PATH_QA_USER_REPORTED_PASS
USER_HANDTEST_REPORTED_PASS
REPOOPS_CLEAN_COMMIT_NOT_DONE
TAG_NOT_ALLOWED
```

Received completion summary:

```text
BattleContract DTOs were added under Assets/_Game/Scripts/TalismanBag/Contracts/Battle.
Read-only V03 exporters, V04 normalizer, BuildEvaluation exporter, Sandbox result mapper, and field validator were added.
Editor-only report writer was added under Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox.
Required reports were generated under Docs/V0.4/Reports.
No scenes, UI, BuildSettings, RunFlow, SaveData, RewardService, BossInfoPanel, boss trigger, reward, or chapter progression files were intentionally modified by this package.
No commit, tag, or push was performed.
```

Guard verification performed in this window:

```text
Confirmed required report files exist.
Read BattleSnapshotAdapterReport.md, BattleSnapshotFieldMap.csv, BattleSnapshotPlayerDevFieldSplit.csv, and BattleSnapshotAdapterLeakCheckReport.md.
Confirmed reports state PASS with Validation mode SOURCE_STATIC.
Confirmed Unity CLI was not available in shell, so Unity compile/menu execution was not verified here.
Scanned Contracts/Battle and BattleSnapshotAdapterReportWriter.cs for forbidden SaveData / PlayerPrefs / MainTrialProgressData / RewardService / DropTable / RunFlow / AutoCombatController / PageState / FormationState / BossInfoPanel / Scene / Prefab / UI / BuildSettings usage.
Confirmed static hits are report text, enum/source labels, or validation counters rather than formal write calls.
Confirmed referenced existing types for MainTrialStartupRoute, V02RoundConfig, BattleLoadoutSnapshot, BattleLoadoutItemSnapshot, ComputedTalismanStats, BuildSandboxLayoutSnapshot, BuildEvaluationResult, BattleSandboxRuntimeLoopPreview, BuildSandboxPreviewContext, CombatModifierBundle, EffectEventBundle, and BuildSandboxFeatureFlags exist.
Confirmed SandboxBattleResultMapper defaults devOnly=true, shouldWriteSave=false, shouldGrantReward=false, chapterProgressDelta=none, and empty rewardClaimToken.
Confirmed git diff --check reports no whitespace errors; only pre-existing LF/CRLF warnings are present.
```

Current Guard decision:

```text
V0.4-BattleSnapshotAdapter01 is now accepted as a user-reported Unity QA pass candidate.
It is no longer only a source-static candidate.
Guard did not independently run Unity in this window.
It is not a RepoOps-verified baseline.
It is not a stable baseline.
It is not an archive baseline.
It must not receive a verified tag.
It must not be treated as permission to formally connect UnifiedBattlePage.
It remains a pure data-layer package and does not authorize formal route, save, reward, boss, or chapter integration.
```

Remaining requirements before stable baseline / archive baseline:

```text
RepoOps isolates this package from pre-existing dirty files and performs a clean commit only when authorized.
RepoOps tag is still forbidden until the user explicitly approves a baseline tag.
Guard / RepoOps record exact committed file list if commit is requested.
```

Current allowed next action:

```text
Proceed to V0.4-UnifiedBattlePageShell01 development assignment.
Do not call it verified baseline.
Do not call BattleSnapshotAdapter01 a stable baseline.
Do not call it stable baseline.
Do not tag.
Do not use it as formal UnifiedBattlePage integration approval.
```

## 12. UnityQA01 Intake

Intake date: `2026-07-07`

Source:

```text
User reported UnityQA01 is complete and manually tested.
```

Guard status update:

```text
BattleSnapshotAdapter01 status upgraded from SOURCE_STATIC_CANDIDATE to UNITY_QA_PASS_CANDIDATE.
UnifiedBattlePageShell01 may proceed as the next package.
This still does not authorize LegacyChapterBattleAdapter01, BattleRouteBridge01, formal route connection, save/reward/chapter writes, stable baseline tag, or RepoOps actions.
```
