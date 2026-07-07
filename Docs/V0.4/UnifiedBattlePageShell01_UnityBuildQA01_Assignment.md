# V0.4-UnifiedBattlePageShell01-UnityBuildQA01 Assignment

Status: `USER_REPORTED_UNITY_ASSET_STATIC_PASS_CANDIDATE / GUARD_ACCEPTED_UNITY_ASSET_STATIC_PASS_CANDIDATE`

Maintainer: Codex Guard / boundary window

Date: 2026-07-07

## 0. Authority

Authoritative boundaries:

```text
AGENTS.md
Docs/LOCKED/*
Docs/V0.4/UnifiedBattlePageShell01_Assignment.md
Docs/V0.4/BattleSnapshotAdapter01_Assignment.md
Docs/V0.4/UnifiedBattleBridge04_06_Assignment.md
User latest explicit completion report
```

This QA gate exists to verify the `V0.4-UnifiedBattlePageShell01` code-defined shell inside Unity by generating the physical devOnly shell scene and prefab and rerunning shell reports from Unity.

## 1. Package Goal

```text
Generate the isolated UnifiedBattlePage shell scene.
Generate the isolated UnifiedBattlePage shell prefab.
Run Unity Editor validation reports.
Confirm required shell slots physically exist.
Confirm BuildSettings remains unchanged.
Confirm no formal route, save, reward, boss, or chapter integration is connected.
```

This package is QA only. It is not `UnifiedBattleRegression01` and not Package 3.

## 2. Received Completion

User reported:

```text
Unity batchmode completed successfully.
No error CS, Script compilation failed, Exception, or Fatal Error was detected in logs.
Build Shell Scene And Prefab menu generated the physical scene and prefab.
QA report menu completed with status=PASS and leakCount=0.
No formal route was connected.
No commit, tag, or push was performed by the task window.
```

Generated physical assets:

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity
Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab
```

Refreshed reports:

```text
Docs/V0.4/Reports/UnifiedBattlePageShellReport.md
Docs/V0.4/Reports/UnifiedBattlePageShellHierarchyMap.csv
Docs/V0.4/Reports/UnifiedBattlePageShellAdapterSlotMap.csv
Docs/V0.4/Reports/UnifiedBattlePageShellLeakCheckReport.md
Docs/V0.4/Reports/UnifiedBattlePageShellManualTest.md
```

## 3. Guard Verification

Guard verified in this window:

```text
Physical shell scene exists.
Physical shell prefab exists.
UnifiedBattlePageShellReport.md shows Validation source UNITY_EDITOR_MENU.
UnifiedBattlePageShellReport.md shows Status PASS.
UnifiedBattlePageShellReport.md shows Validation mode UNITY_ASSET_STATIC.
UnifiedBattlePageShellReport.md shows Scene exists YES.
UnifiedBattlePageShellReport.md shows Prefab exists YES.
UnifiedBattlePageShellReport.md shows Scene in BuildSettings NO.
UnifiedBattlePageShellReport.md shows Asset slots present 11.
UnifiedBattlePageShellLeakCheckReport.md shows Total leak count 0.
UnifiedBattlePageShellManualTest.md shows PASS summary.
ProjectSettings search did not find Scene_TalismanBag_V04_UnifiedBattlePageShell.
```

Required slots confirmed by report:

```text
BattlePageRoot
BoardArea
ItemTrayArea
EnemyInfoArea
BossCastBarSlot
BattleFeedbackLayer
StoryGuidePopupLayer
ResultRewardPlaceholder
V03FlowAdapterSlot
V04SandboxAdapterSlot
DevOnlyDiagnosticsSlot
```

Marker isolation confirmed by report:

```text
devOnly = true
isEnabled = false
formalFlow = false
connectedToFormalRoute = false
```

## 4. Guard Decision

Current accepted status:

```text
UNITY_ASSET_STATIC_PASS_CANDIDATE
GUARD_ACCEPTED_UNIFIED_BATTLE_PAGE_SHELL01_UNITY_BUILD_QA01
```

Meaning:

```text
UnifiedBattlePageShell01 is now verified as generated and report-checked inside Unity at the asset-static level.
It may unblock Package 3: UnifiedBattleRegression01 assignment.
It is still not a stable baseline.
It is still not an archive baseline.
It does not authorize LegacyChapterBattleAdapter01.
It does not authorize BattleRouteBridge01.
It does not authorize formal route, save, reward, boss, or chapter integration.
```

## 5. Remaining Red Lines

```text
Do not connect formal chapter entry.
Do not replace Scene_TalismanBag_V02_FormationCounter.
Do not add the shell scene to BuildSettings.
Do not write SaveData / PlayerPrefs / MainTrialProgressData.
Do not call RewardService.
Do not bypass BossInfoPanel.
Do not auto-start 2-10 Boss.
Do not start Package 4 / 5 / 6 before Package 3 regression passes.
Do not commit, tag, or push unless user explicitly opens RepoOps.
```

## 6. Next Package

Next allowed package:

```text
V0.4-UnifiedBattleRegression01
```

Purpose:

```text
Verify that BattleSnapshotAdapter01 and UnifiedBattlePageShell01 do not pollute formal V0.2 / V0.3 flow.
Confirm formal scenes, BuildSettings, RunFlow, SaveData, RewardService, BossInfoPanel, Boss trigger, and chapter progression remain unchanged.
Confirm shell remains devOnly / isolated and has no formal route.
```
