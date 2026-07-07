# V0.4-UnifiedBattleRegression01 Assignment

Status: `GUARD_PASS_UNIFIED_BATTLE_REGRESSION01 / READY_FOR_QA`

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
Docs/V0.4/BattleSnapshotAdapter01_Assignment.md
Docs/V0.4/BattleSnapshotAdapter01_UnityQA01_Assignment.md
Docs/V0.4/UnifiedBattlePageShell01_Assignment.md
Docs/V0.4/UnifiedBattlePageShell01_UnityBuildQA01_Assignment.md
Docs/V0.4/UnifiedBattleBridge04_06_Assignment.md
User latest explicit instruction
```

This assignment authorizes only `V0.4-UnifiedBattleRegression01`. It does not authorize `LegacyChapterBattleAdapter01`, `BattleRouteBridge01`, formal route connection, save/reward/chapter writes, or any stable-baseline tag.

## 1. Package Goal

`V0.4-UnifiedBattleRegression01` is the no-pollution regression package after:

```text
Package 1: BattleSnapshotAdapter01
Package 2: UnifiedBattlePageShell01
```

Goal:

```text
Prove BattleContract / Snapshot / Adapter and UnifiedBattlePage shell did not pollute V0.2 / V0.3 formal flow.
Prove the shell remains isolated, devOnly, and not connected to formal route.
Prove formal scenes, BuildSettings, RunFlow, SaveData, RewardService, BossInfoPanel, boss trigger, and chapter progression remain protected.
```

This package is validation / regression only. It must not add new battle features.

## 2. Allowed Scope

Allowed actions:

```text
Inspect git status and package file lists.
Run Unity compile / Editor QA menus if available.
Open / inspect the UnifiedBattle shell scene and prefab.
Run BattleSnapshotAdapter reports if useful.
Run UnifiedBattlePageShell reports.
Perform source scans for forbidden references.
Perform minimal old-flow smoke checks.
Write regression reports under Docs/V0.4/Reports.
```

Allowed new validation files:

```text
Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/UnifiedBattleRegressionValidator.cs
Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/UnifiedBattleRegressionReportWriter.cs
```

Only if needed. Prefer report-only/manual validation if existing tooling is enough.

Allowed report outputs:

```text
Docs/V0.4/Reports/UnifiedBattleRegressionReport.md
Docs/V0.4/Reports/UnifiedBattleRegressionChecklist.csv
Docs/V0.4/Reports/UnifiedBattleRegressionLeakCheckReport.md
Docs/V0.4/Reports/UnifiedBattleRegressionFileScope.csv
Docs/V0.4/Reports/UnifiedBattleRegressionManualTest.md
```

## 3. Strict Forbidden Scope

Do not modify:

```text
Scene_TalismanBag_V02_FormationCounter.unity
Scene_TalismanBag_V03_MainHome.unity
Scene_TalismanBag_V03_TalismanUpgrade.unity
Scene_TalismanBag_V03_BootEntry.unity
Scene_TalismanBag_V04_BattleSandboxPreview.unity
ProjectSettings
BuildSettings
V02RunFlowController
MainTrialFlowService behavior
AutoCombatController
PageState
FormationState
V02FormationGridFrame
DamageText
BossInfoPanel
SaveData
PlayerPrefs
MainTrialProgressData
RewardService
RewardConfig
DropTable
StageConfig
EnemyConfig
BossConfig
UpgradeConfig
formal numeric configs
AGENTS.md
Docs/LOCKED/*
```

Do not do:

```text
Do not create new gameplay.
Do not develop Package 4 / 5 / 6 work.
Do not map formal chapter data into BattleContract.
Do not connect formal trial entry to UnifiedBattlePage.
Do not replace V0.2 / V0.3 battle page.
Do not start formal battle from UnifiedBattle shell.
Do not write SaveData.
Do not call RewardService.
Do not advance chapters.
Do not bypass BossInfoPanel.
Do not auto-start 2-10 Boss.
Do not add UnifiedBattle shell scene to BuildSettings.
Do not default-enable any UnifiedBattle feature flag.
Do not commit, tag, push, reset, or rollback.
```

If regression reveals a defect, stop and report. Do not fix inside this package unless Guard opens a separate fix package.

## 4. Required Regression Areas

### 4.1 File Scope / Diff Guard

Must report whether changes are limited to allowed Package 1 / Package 2 / regression files.

Must identify unrelated dirty files separately, especially:

```text
BuildGridInteractionPreviewController.cs
MobileRotateZoneInteraction* reports
```

Regression window must not clean or revert unrelated dirty files.

### 4.2 Formal Scene Protection

Must confirm:

```text
V02/V03 formal scenes were not modified by Package 1 / Package 2.
Scene_TalismanBag_V04_BattleSandboxPreview was not modified by UnifiedBattle shell work.
UnifiedBattle shell scene exists but is isolated.
UnifiedBattle shell scene is not in BuildSettings.
```

### 4.3 Formal Flow Protection

Must scan / verify no new writes or direct calls from BattleContract / UnifiedBattle shell to:

```text
RunFlow
V02RunFlowController
MainTrialFlowService mutation methods
AutoCombatController
PageState
FormationState
SaveData
PlayerPrefs
MainTrialProgressData
RewardService
BossInfoPanel
chapter progression
```

### 4.4 Shell Isolation

Must confirm:

```text
UnifiedBattlePageShellMarker.devOnly = true
UnifiedBattlePageShellMarker.isEnabled = false
UnifiedBattlePageShellMarker.formalFlow = false
UnifiedBattlePageShellMarker.connectedToFormalRoute = false
BattlePageRoot exists.
All 11 shell slots exist.
ResultRewardPlaceholder does not write save or grant reward.
DevOnlyDiagnosticsSlot remains separate from player-visible slots.
Player-visible areas do not show answer-layer fields.
```

### 4.5 Minimal Old-Flow Smoke

This is not full Package 6 golden path. It is a minimum no-pollution smoke.

Verify if possible:

```text
Old V0.2 / V0.3 battle path can still enter.
Board still displays.
Damage numbers still display.
Battle still progresses.
BossInfoPanel still appears before 2-10 Boss where applicable.
2-10 Boss remains manually triggered.
Reward / save behavior shows no obvious abnormal duplicate or missing operation.
```

If the QA environment cannot complete a step, report the exact skipped step and residual risk.

## 5. Required Reports

Must output:

```text
Docs/V0.4/Reports/UnifiedBattleRegressionReport.md
Docs/V0.4/Reports/UnifiedBattleRegressionChecklist.csv
Docs/V0.4/Reports/UnifiedBattleRegressionLeakCheckReport.md
Docs/V0.4/Reports/UnifiedBattleRegressionFileScope.csv
Docs/V0.4/Reports/UnifiedBattleRegressionManualTest.md
```

Reports must include:

```text
Unity compile / Console status.
Whether BattleSnapshotAdapter reports pass.
Whether UnifiedBattlePageShell reports pass.
Whether UnifiedBattle shell scene/prefab exist.
Whether shell scene is absent from BuildSettings.
Whether formal scenes changed.
Whether RunFlow / SaveData / RewardService / BossInfoPanel changed.
Whether any formal route was connected.
Whether old-flow smoke was run.
Leak count.
git status.
Known risks.
```

## 6. Acceptance Criteria

Package passes only if:

```text
Unity compile has no red Error, or limitation is explicitly reported.
BattleSnapshotAdapter remains devOnly/no-save/no-reward for sandbox result.
UnifiedBattle shell scene/prefab remain devOnly and isolated.
UnifiedBattle shell scene is not in BuildSettings.
No formal route is connected.
No formal SaveData / RewardService / chapter writes are introduced.
BossInfoPanel is not bypassed.
2-10 Boss is not auto-started.
Leak check = 0.
Reports exist.
User / Guard accepts remaining QA limitations if any.
```

## 7. Next Package Gate

If this package passes, Guard may open:

```text
V0.4-LegacyChapterBattleAdapter01
```

That next package is the first package allowed to approach formal chapter / stage / boss / reward-preview data, but still only read-only and still not route bridging.

This package must not start that work.
