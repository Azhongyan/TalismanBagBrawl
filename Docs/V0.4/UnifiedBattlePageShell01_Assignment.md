# V0.4-UnifiedBattlePageShell01 Assignment

Status: `GUARD_PASS_UNIFIED_BATTLE_PAGE_SHELL01 / READY_FOR_DEV`

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
