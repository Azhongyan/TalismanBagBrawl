# V0.4-UnifiedBattlePageShell01 Assignment

Status: `UNITY_ASSET_STATIC_PASS_CANDIDATE / GUARD_READY_FOR_UNIFIED_BATTLE_REGRESSION01_ASSIGNMENT`

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
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BattlePageViewAdapter01_Assignment.md
Docs/V0.4/BattleSnapshotAdapter01_Assignment.md
Docs/V0.4/UnifiedBattleBridge04_06_Assignment.md
User latest explicit instruction
```

This assignment authorizes only `V0.4-UnifiedBattlePageShell01`. It does not authorize formal route bridging, legacy chapter adapter work, formal save / reward / chapter commits, or replacing `Scene_TalismanBag_V02_FormationCounter`.

## 1. Package Goal

`V0.4-UnifiedBattlePageShell01` creates or defines an isolated Unified BattlePage shell that can hold future formal battle UI areas and BattleContract adapter slots.

The package goal:

```text
Create the shell shape.
Expose stable English-named slots.
Bind only sample/devOnly BattleContract data if needed.
Prove the shell does not affect formal V0.2 / V0.3 flow.
```

The package is not a playable battle integration package.

Correct route:

```text
BattleSnapshotAdapter01
-> UnifiedBattlePageShell01
-> UnifiedBattleRegression01
-> LegacyChapterBattleAdapter01
-> BattleRouteBridge01
-> GoldenPathBridgeRegression01
```

## 2. Allowed Outputs

The shell must define these top-level areas or equivalent stable slots:

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

Allowed contract bindings:

```text
BattleStartRequest
BattleLayoutSnapshot
BattleItemSnapshot
BattleEnemySnapshot
BuildEvaluationSnapshot
BattleResultSnapshot
```

Allowed behavior:

```text
Display placeholder / mock / sample contract data.
Show shell areas and slot names for QA.
Keep player-visible fields separate from developer diagnostics.
Keep result and reward areas as placeholders only.
Expose devOnly QA menu or launcher if needed.
Emit validation reports and hierarchy / slot maps.
```

## 3. Allowed File Types

Allowed new or modified code files:

```text
Assets/_Game/Scripts/TalismanBag/UnifiedBattle/**
Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/** only for QA report writer integration
```

Allowed new isolated UI assets:

```text
Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/**
Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity
```

Rules for UI assets:

```text
Creating a new isolated devOnly shell scene is allowed.
Creating a new isolated shell prefab is allowed.
The development window should prefer the smallest asset surface that can prove the shell.
Do not add the new scene to BuildSettings in this package.
Do not modify existing formal scenes.
Do not modify Scene_TalismanBag_V04_BattleSandboxPreview unless Guard explicitly reopens scope.
```

Allowed report outputs:

```text
Docs/V0.4/Reports/UnifiedBattlePageShellReport.md
Docs/V0.4/Reports/UnifiedBattlePageShellHierarchyMap.csv
Docs/V0.4/Reports/UnifiedBattlePageShellAdapterSlotMap.csv
Docs/V0.4/Reports/UnifiedBattlePageShellLeakCheckReport.md
Docs/V0.4/Reports/UnifiedBattlePageShellManualTest.md
```

## 4. Required Architecture Boundary

The shell must depend on BattleContract / Snapshot / Adapter language, not direct formal flow internals.

Allowed dependency shape:

```text
UnifiedBattlePageShell
-> reads BattleStartRequest / BattleLayoutSnapshot / BattleEnemySnapshot / BuildEvaluationSnapshot
-> displays placeholders / slot bindings
-> may output BattleResultSnapshot placeholder in devOnly QA
```

Forbidden dependency shape:

```text
UnifiedBattlePageShell
-> directly calls V02RunFlowController
-> directly calls MainTrialFlowService state mutation
-> directly writes SaveData / PlayerPrefs / MainTrialProgressData
-> directly calls RewardService
-> directly triggers BossInfoPanel or Boss battle
```

If the shell needs formal data, it must use sample data or already-created BattleContract DTOs. Formal chapter data mapping belongs to `LegacyChapterBattleAdapter01`, not this package.

## 5. UI Reuse Boundary

This package may create a shell, but it must not declare the V0.4 sandbox temporary UI as the final formal UI source.

Rules:

```text
Use English stable hierarchy names and script keys.
Chinese is allowed only for display copy, reports, or displayName fields.
Prefer reusable visual language and existing BattlePageViewSpec / UI reuse reports.
Do not create many one-off mechanic panels.
Do not expose hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, DropBias weights, bossSixKeyFullAnswer, or problemReadinessFullAnswer in player UI.
Developer diagnostics must stay in DevOnlyDiagnosticsSlot, reports, or editor-only tools.
```

The shell may contain placeholders for:

```text
Board occupancy display
Item tray display
Enemy / boss readable state
Boss cast bar
Battle feedback
Story / guide / popup messages
Result / reward preview placeholder
```

The shell must not implement final art polish, final mobile handfeel, formal route ownership, or reward claim behavior.

## 6. Strict Forbidden Scope

Do not modify:

```text
Scene_TalismanBag_V02_FormationCounter.unity
Scene_TalismanBag_V03_MainHome.unity
Scene_TalismanBag_V03_TalismanUpgrade.unity
Scene_TalismanBag_V03_BootEntry.unity
Scene_TalismanBag_V04_BattleSandboxPreview.unity unless Guard explicitly reopens scope
ProjectSettings
BuildSettings
V02RunFlowController
MainTrialFlowService behavior
PageState
FormationState
AutoCombatController
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
Do not connect formal trial entry to UnifiedBattlePage.
Do not replace V0.2 / V0.3 battle page.
Do not start a formal battle from the shell.
Do not auto-start 2-10 Boss.
Do not bypass BossInfoPanel.
Do not grant rewards.
Do not write save data.
Do not advance chapters.
Do not add the shell scene to BuildSettings.
Do not default-enable any UnifiedBattle feature flag.
Do not mix in ItemRarity, formal drops, acquisition, washing, or formal Item system work.
Do not commit, tag, push, reset, or rollback.
```

## 7. Feature Flags / Entry Rules

This package does not need a formal route flag. If any flag or launcher is added, defaults must be:

```text
devOnly = true
isEnabled = false
formalFlow = false for QA/sample shell paths
```

The only allowed entry paths are:

```text
Unity Editor QA menu
devOnly manual test scene
devOnly prefab preview / isolated launcher
```

Formal player paths must remain unchanged.

## 8. Validation Report Requirements

The package must output:

```text
Docs/V0.4/Reports/UnifiedBattlePageShellReport.md
Docs/V0.4/Reports/UnifiedBattlePageShellHierarchyMap.csv
Docs/V0.4/Reports/UnifiedBattlePageShellAdapterSlotMap.csv
Docs/V0.4/Reports/UnifiedBattlePageShellLeakCheckReport.md
Docs/V0.4/Reports/UnifiedBattlePageShellManualTest.md
```

Reports must prove:

```text
Shell exists as isolated scene, prefab, or code-defined root.
Required shell slots exist or have documented equivalents.
Shell consumes BattleContract DTOs or sample snapshots only.
No formal route entry is connected.
No RunFlow / SaveData / RewardService / BossInfoPanel / chapter progression writes.
No existing formal scene was modified.
No BuildSettings / ProjectSettings change.
No formal FeatureFlag default true.
Player-visible fields do not expose developer answer-layer diagnostics.
Result / reward area is placeholder only.
```

Completion report must include:

```text
New file list
Modified file list
Created scene / prefab list
Whether existing formal scenes were modified
Whether Scene_TalismanBag_V04_BattleSandboxPreview was modified
Whether BuildSettings / ProjectSettings were modified
Whether RunFlow was modified
Whether SaveData / PlayerPrefs / MainTrialProgressData were modified
Whether RewardService was modified
Whether BossInfoPanel was modified
Whether any formal route was connected
Whether all UnifiedBattle flags / launchers default disabled
Report output paths
Unity compile / menu validation result, or explicit reason not run
git status
Known risks
```

## 9. QA / Acceptance

Required checks:

```text
Unity compile passes if Unity Editor is available.
QA report writer or validator runs if Unity Editor is available.
UnifiedBattle shell can be opened or inspected through devOnly path only.
BattlePageRoot and required slots are present.
BattleContract sample data can bind without starting formal battle.
ResultRewardPlaceholder does not grant reward or write save.
DevOnlyDiagnosticsSlot is separate from player-visible areas.
Formal V0.2 / V0.3 scenes and flow remain unchanged.
BuildSettings remains unchanged.
Leak check = 0.
```

If Unity CLI is unavailable:

```text
The development window must clearly report SOURCE_STATIC or EDITOR_NOT_RUN.
Guard may accept only source-static status, but UnifiedBattleRegression01 should then include explicit Unity Editor verification.
```

## 10. UnifiedBattleRegression01 Prerequisites

`V0.4-UnifiedBattleRegression01` may start only after:

```text
UnifiedBattlePageShell01 is dev complete.
Required shell slots exist.
Required reports exist and show PASS or clearly documented source-static limitation.
No formal scenes / BuildSettings / RunFlow / SaveData / Reward / Boss / chapter changes were made.
No formal route was connected.
User confirms moving forward.
TASK_STATUS_SYNC_TO_GUARD_REPOOPS is sent or a manual forward package is generated.
```

Even after this package passes, the next package is regression only. It must still not start `LegacyChapterBattleAdapter01` or `BattleRouteBridge01` work until Guard opens those packages.

## 11. Guard Completion Intake

Intake date: `2026-07-07`

Source of completion:

```text
User forwarded completion report for V0.4-UnifiedBattlePageShell01.
```

Guard intake status:

```text
SOURCE_STATIC_CODE_DEFINED_CANDIDATE
GUARD_STATIC_PASS_UNIFIED_BATTLE_PAGE_SHELL01
GUARD_HOLD_UNITY_SCENE_PREFAB_QA
UNITY_COMPILE_NOT_VERIFIED_BY_GUARD_SHELL
PHYSICAL_SCENE_NOT_GENERATED_IN_GUARD_SHELL
PHYSICAL_PREFAB_NOT_GENERATED_IN_GUARD_SHELL
NO_FORMAL_ROUTE_CONNECTED
NO_STABLE_BASELINE
NO_TAG_ALLOWED
```

Received completion summary:

```text
Runtime shell scripts were added under Assets/_Game/Scripts/TalismanBag/UnifiedBattle.
Editor builder, verifier, and report writer were added under Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle.
Required static reports were generated under Docs/V0.4/Reports.
No physical .unity scene or .prefab was generated because Unity CLI was unavailable.
Manual Unity menu entries were provided for scene/prefab generation and QA report rerun.
No formal route was connected.
No V02/V03 formal scene, Scene_TalismanBag_V04_BattleSandboxPreview, BuildSettings, ProjectSettings, RunFlow, SaveData, RewardService, BossInfoPanel, or chapter progression change was reported.
```

Guard verification performed in this window:

```text
Confirmed Runtime shell files exist.
Confirmed Editor builder / verifier / report writer files exist.
Read UnifiedBattlePageShellReport.md, UnifiedBattlePageShellHierarchyMap.csv, UnifiedBattlePageShellAdapterSlotMap.csv, UnifiedBattlePageShellLeakCheckReport.md, and UnifiedBattlePageShellManualTest.md.
Confirmed reports state SOURCE_STATIC_CODE_DEFINED and leak count 0.
Confirmed required 11 shell slots are code-defined.
Confirmed reports explicitly warn that physical scene / prefab was not generated.
Scanned UnifiedBattle runtime/editor directories for forbidden formal-flow, save, reward, boss, scene loading, and BuildSettings references.
Confirmed runtime shell code does not call formal flow / save / reward / boss APIs.
Confirmed BuildSettings references are Editor verifier/report checks, not BuildSettings writes.
Confirmed BattleContract field references used by runtime sample binding match current contract DTOs.
```

Observed repo state:

```text
Guard observed current branch wip/v0.3-forge-first-upgrade-guide01-clean-rollback-snapshot.
Guard observed HEAD 6952ce8 with message: checkpoint: upload current v0.4 workspace snapshot.
Guard observed git status clean at intake time.
This differs from the forwarded task report saying files were untracked and no commit/tag/push was performed.
This Guard window did not commit, tag, or push.
RepoOps ownership of commit / upload state remains separate.
```

Current Guard decision:

```text
V0.4-UnifiedBattlePageShell01 is accepted only as source-static code-defined candidate.
It is not Unity-verified.
It is not a stable baseline.
It is not a formal route integration.
It may not unlock LegacyChapterBattleAdapter01 or BattleRouteBridge01.
Before UnifiedBattleRegression01 can be meaningful, the shell scene/prefab should be generated and verified in Unity.
```

Recommended next gate:

```text
V0.4-UnifiedBattlePageShell01-UnityBuildQA01
```

Purpose:

```text
Run the manual Unity build menu.
Generate Scene_TalismanBag_V04_UnifiedBattlePageShell.unity.
Generate UnifiedBattlePageShell.prefab.
Run QA reports from Unity.
Confirm shell opens, required slots exist physically, scene is not in BuildSettings, and leak count remains 0.
```

## 12. UnityBuildQA01 Intake

Intake date: `2026-07-07`

Source of completion:

```text
User reported V0.4-UnifiedBattlePageShell01-UnityBuildQA01 complete.
```

Guard intake status:

```text
USER_REPORTED_UNITY_ASSET_STATIC_PASS_CANDIDATE
GUARD_ACCEPTED_UNIFIED_BATTLE_PAGE_SHELL01_UNITY_BUILD_QA01
UNITY_ASSET_STATIC_PASS_CANDIDATE
READY_FOR_UNIFIED_BATTLE_REGRESSION01_ASSIGNMENT
NOT_STABLE_BASELINE
NO_FORMAL_ROUTE_CONNECTED
NO_TAG_ALLOWED
```

Received Unity QA result:

```text
Unity batchmode exited successfully for shell build and report QA.
No error CS, Script compilation failed, Exception, or Fatal Error was reported.
Scene_TalismanBag_V04_UnifiedBattlePageShell.unity was generated.
UnifiedBattlePageShell.prefab was generated.
Validation source is UNITY_EDITOR_MENU.
Validation mode is UNITY_ASSET_STATIC.
Status is PASS.
Asset slots present = 11.
Warning count = 0.
Leak count = 0.
Shell scene is not in BuildSettings.
```

Guard verification performed in this window:

```text
Confirmed physical shell scene exists.
Confirmed physical shell prefab exists.
Read refreshed UnifiedBattlePageShellReport.md, UnifiedBattlePageShellHierarchyMap.csv, UnifiedBattlePageShellAdapterSlotMap.csv, UnifiedBattlePageShellLeakCheckReport.md, and UnifiedBattlePageShellManualTest.md.
Confirmed required 11 slots are present in refreshed reports.
Confirmed marker isolation is reported for scene and prefab.
Confirmed leak count remains 0.
Searched ProjectSettings and did not find Scene_TalismanBag_V04_UnifiedBattlePageShell.
```

Current Guard decision:

```text
V0.4-UnifiedBattlePageShell01 is accepted as Unity asset-static pass candidate.
Package 3 V0.4-UnifiedBattleRegression01 may now receive Guard assignment.
This still does not authorize Package 4 LegacyChapterBattleAdapter01.
This still does not authorize Package 5 BattleRouteBridge01.
This still does not authorize formal route, save, reward, boss, or chapter integration.
This is not a stable baseline and must not be tagged without explicit RepoOps / user approval.
```
