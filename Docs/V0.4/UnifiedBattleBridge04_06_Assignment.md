# UnifiedBattle Bridge 04-06 Guard Assignment

Status: `GUARD_ACCEPTED_UNIFIED_BATTLE_BRIDGE_04_06 / STAGED_ASSIGNMENT_ONLY`

Maintainer: Codex Guard / boundary window

Date: 2026-07-07

## 0. Authority

Authoritative boundaries:

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UnifiedBattlePageStrategy_GuardSync.md
Docs/V0.4/BattleSnapshotAdapter01_Assignment.md
User latest explicit PM / Guard instruction
```

This document records the Guard split for the later UnifiedBattle bridge line. It does not authorize skipping package order, starting development early, committing, tagging, pushing, or changing protected formal flow without the matching package assignment.

## 1. UnifiedBattle Package Order

The UnifiedBattle connection line is split into six continuous packages:

```text
Package 1: BattleSnapshotAdapter01
Package 2: UnifiedBattlePageShell01
Package 3: UnifiedBattleRegression01
Package 4: LegacyChapterBattleAdapter01
Package 5: BattleRouteBridge01
Package 6: GoldenPathBridgeRegression01
```

Guard boundary:

```text
Packages 1-3 = read-only contract + page shell + no-pollution validation.
Packages 4-6 = formal chapter data adapter + feature-flagged route bridge + golden path regression.
```

Important note:

```text
The earlier BattleContract technical surveys support Package 1 only.
They must not be directly reused as Package 4 / 5 / 6 development assignments.
Packages 4 / 5 / 6 each require their own Guard review before development starts.
```

## 2. Global Red Lines For Packages 4-6

Strictly forbidden unless a later Guard assignment explicitly allows the exact file and behavior:

```text
Do not let UnifiedBattlePage directly write SaveData, PlayerPrefs, or MainTrialProgressData.
Do not let UnifiedBattlePage directly call RewardService or grant formal rewards.
Do not bypass BossInfoPanel.
Do not auto-start 2-10 Boss after 2-9.
Do not delete or replace V02RunFlowController by default.
Do not default-enable formal UnifiedBattle routing.
Do not leak devOnly enemies, bosses, DropBias, build tasks, or full solution diagnostics into formal player flow.
Do not modify AGENTS.md or Docs/LOCKED/*.
Do not commit, tag, push, reset, or rollback.
```

`BattleResultSnapshot` remains result data only. Formal save, reward, boss, and chapter commit authority stays with the legacy formal flow until a later approved mainline promotion package.

## 3. Package 4: LegacyChapterBattleAdapter01

### 3.1 Positioning

```text
Convert V0.3 / V0.2 formal chapter, stage, boss, and reward-preview data into BattleContract.
```

This package does not switch pages and does not start battle. Its only purpose is to let future UnifiedBattle views read formal chapter data through a neutral contract.

### 3.2 Allowed Scope

Allowed read-only inputs:

```text
MainTrialFlowService current chapter progress
V02RunConfig / V02RoundConfig
BossInfo / BossConfig readable data
Formal enemy profile id / boss profile id
Formal reward preview data
```

Allowed outputs:

```text
BattleStartRequest
BattleEnemySnapshot
rewardPreview for display only
chapter / stage / round route metadata
validation reports
```

Allowed file categories:

```text
Neutral contract-layer adapter files.
Read-only formal-flow exporter files.
Validation-only report generator files.
Docs/V0.4/Reports/* report outputs.
```

Recommended report outputs:

```text
Docs/V0.4/Reports/LegacyChapterBattleAdapterReport.md
Docs/V0.4/Reports/LegacyChapterBattleFieldMap.csv
Docs/V0.4/Reports/LegacyChapterBattleAdapterLeakCheckReport.md
```

### 3.3 Forbidden Scope

```text
Do not modify MainTrialFlowService behavior.
Do not advance chapter progress.
Do not write SaveData.
Do not call RewardService.
Do not open the formal Battle page or UnifiedBattlePage.
Do not replace V02RunFlowController.
Do not modify BossInfoPanel.
Do not change formal reward, boss, or chapter configs.
```

### 3.4 Key Field Mapping

Minimum mapping:

```text
MainTrialProgressData.currentChapter -> BattleStartRequest.chapterId
V02RoundConfig.roundId -> BattleStartRequest.roundId
V02RoundConfig.enemyId -> BattleEnemySnapshot.enemyId
BossConfig.bossId -> BattleEnemySnapshot.bossId
isBossRound -> BattleStartRequest.isBossStage
BossInfo recommended info -> playerVisibleHints
Full mechanic answer -> developerOnlyDiagnostics
```

If exact method names differ from this assignment, the development window must first perform a read-only code survey and preserve the same data boundary.

### 3.5 Acceptance

Package 4 passes only if:

```text
Formal 1-10 can generate BattleStartRequest.
Formal 2-1 through 2-9 can generate BattleStartRequest.
Formal pre-2-10 Boss can generate Boss BattleStartRequest without auto-starting battle.
rewardPreview does not grant rewards.
chapterProgressDelta does not write save data.
V0.2 / V0.3 golden path behavior is unchanged.
```

## 4. Package 5: BattleRouteBridge01

### 4.1 Positioning

```text
Create a feature-flagged route bridge from formal entry points to UnifiedBattlePage.
```

Default behavior must remain the legacy formal flow. This package may only allow UnifiedBattlePage entry through explicit devOnly / debug / sandbox routing until Guard, PM, and QA approve a formal gray rollout.

### 4.2 Required Feature Flags

The package must introduce or use equivalent flags with these defaults:

```text
EnableUnifiedBattleRoute = false
EnableUnifiedBattleForNormalStage = false
EnableUnifiedBattleForBossStage = false
EnableUnifiedBattleResultBridge = false
```

All defaults must be:

```text
false
```

### 4.3 Target Route Shape

Target logical route:

```text
V03NavigationFlowController.EnterTrial()
-> MainTrialFlowService.GetStartupRoute()
-> BattleRouteBridge
-> if flag off: legacy V02RunFlowController
-> if flag on: generate BattleStartRequest
-> enter UnifiedBattlePageShell
```

Guard note:

```text
This route shape is a target contract, not a guarantee of exact existing method names.
The development window must read current code before implementation and keep changes minimal.
```

### 4.4 Result Bridge

Only this data type may return from UnifiedBattle:

```text
BattleResultSnapshot
```

Default result policy:

```text
devOnly = true
shouldWriteSave = false
shouldGrantReward = false
```

The legacy formal flow remains responsible for any real save write, reward grant, boss result, and chapter progression.

### 4.5 Forbidden Scope

```text
Do not default-enable UnifiedBattleRoute.
Do not delete the legacy V02RunFlowController route.
Do not let UnifiedBattlePage write SaveData.
Do not let UnifiedBattlePage call RewardService.
Do not auto-start 2-10 Boss.
Do not bypass BossInfoPanel.
Do not use the bridge for formal player routing unless PM / Guard / QA explicitly approve a gray rollout.
```

### 4.6 First-Stage Gray Rule

The first bridge stage may only support:

```text
devOnly route
debug route
sandbox route
```

Formal player paths must continue to use the legacy flow while all flags are false.

### 4.7 Acceptance

Package 5 passes only if:

```text
With all flags false, legacy formal flow is unchanged.
With devOnly route enabled, UnifiedBattlePageShell can be entered.
After 2-9, the flow still stops at BossInfoPanel.
2-10 Boss does not auto-trigger.
BattleResultSnapshot does not write formal save data.
UnifiedBattlePage does not call RewardService.
```

Recommended report outputs:

```text
Docs/V0.4/Reports/BattleRouteBridgeReport.md
Docs/V0.4/Reports/BattleRouteBridgeFlagDefaults.csv
Docs/V0.4/Reports/BattleRouteBridgeLeakCheckReport.md
```

## 5. Package 6: GoldenPathBridgeRegression01

### 5.1 Positioning

```text
Run full golden path regression after BattleRouteBridge exists.
```

Goal: prove that the bridge does not break formal mainline, rewards, save data, Boss flow, chapter progression, damage numbers, board display, formation energy, and battle edit locks.

### 5.2 Required Path A: All Flags Off

Must verify:

```text
Loading
-> StartGame
-> OpeningStory
-> 1-1
-> 1-10 Boss
-> BossReward
-> Home
-> Forge
-> FirstUpgrade
-> Home
-> Trial
-> 2-1
-> 2-9
-> BossInfoPanel
-> manual challenge 2-10 Boss
-> BossResult
```

Requirement:

```text
Behavior must match the V0.2 / V0.3 legacy formal flow.
```

### 5.3 Required Path B: devOnly UnifiedBattle

Must verify:

```text
Debug entry
-> BattleRouteBridge
-> BattleStartRequest
-> UnifiedBattlePageShell
-> BattleResultSnapshot
-> return to Debug / Sandbox Result
```

Requirements:

```text
No formal save write.
No formal reward grant.
No formal chapter progression.
```

### 5.4 Required Path C: Boss Protection

Must verify:

```text
2-9 victory
-> BossInfoPanel
-> no auto battle start
-> manual click to challenge
-> BossBattle
-> BossResult
```

Requirement:

```text
UnifiedBattle route must not take over or bypass BossInfoPanel.
```

### 5.5 Required Checks

```text
SaveData has no abnormal change.
MainTrialProgressData has no abnormal progression.
RewardService call count matches the legacy flow.
BossInfoPanel still displays.
2-10 Boss remains manually triggered.
Damage numbers still display.
Board still displays.
Formation energy still works.
Dragging is still blocked during battle.
The player can still prepare after failure.
```

### 5.6 Failure Backflow Rule

If any of the following occurs:

```text
Flags are false but formal flow changes.
UnifiedBattlePage writes SaveData.
UnifiedBattlePage calls RewardService.
2-10 Boss auto-triggers.
BossInfoPanel is bypassed.
Formal rewards are granted twice.
Formal chapter progression is abnormal.
```

Then immediately:

```text
Stop the next package.
Rollback or disable BattleRouteBridge changes.
Keep BattleContract and PageShell.
Open a separate BridgeFix package.
```

Recommended report outputs:

```text
Docs/V0.4/Reports/GoldenPathBridgeRegressionReport.md
Docs/V0.4/Reports/GoldenPathBridgeRegressionChecklist.csv
Docs/V0.4/Reports/GoldenPathBridgeRegressionLeakCheckReport.md
```

## 6. Preconditions Before Starting Each Package

Package 4 may start only after:

```text
Package 1 BattleSnapshotAdapter01 is complete and accepted.
Package 2 UnifiedBattlePageShell01 is complete and accepted.
Package 3 UnifiedBattleRegression01 proves no pollution.
Guard issues or confirms the Package 4 assignment.
```

Package 5 may start only after:

```text
Package 4 is complete and accepted.
Formal chapter data can be exported to BattleContract read-only.
rewardPreview remains display-only.
BossInfoPanel is still protected.
Guard issues or confirms the Package 5 assignment.
```

Package 6 may start only after:

```text
Package 5 is complete and accepted.
All UnifiedBattle route flags default to false.
devOnly route can enter UnifiedBattlePageShell.
Flag-off legacy formal flow is expected to remain unchanged.
Guard issues or confirms the Package 6 regression assignment.
```

## 7. Final Guard Decision

Accepted total order:

```text
1. BattleSnapshotAdapter01: contract
2. UnifiedBattlePageShell01: shell
3. UnifiedBattleRegression01: no-pollution validation
4. LegacyChapterBattleAdapter01: formal chapter data to contract
5. BattleRouteBridge01: default-off gray route bridge
6. GoldenPathBridgeRegression01: full bridge regression
```

Key boundary:

```text
Package 4 is the first package that approaches formal chapter data.
Package 5 is the first package that touches formal entry routing.
Package 6 must prove that with all flags off, the legacy flow is unchanged.
```
