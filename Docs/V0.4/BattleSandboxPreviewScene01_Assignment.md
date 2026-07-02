# V0.4-BattleSandboxPreviewScene01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOXPREVIEWSCENE01
```

## 1. 包定位

```text
V0.4 独立可玩沙盒场景骨架包
```

本包目标是建立 V0.4 BuildSandbox 的独立预览场景，用于后续承载 Build 题库、数据面板、拖拽体验和 devOnly 验证流程。

场景名锁定：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
```

本包只建独立场景骨架和基础 UI 插槽，不接正式战斗，不接正式主线，不修改 V02 / V03 正式场景。

## 2. 上游前置

必须读取并遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/BuildProblemRulePool01_Assignment.md
Docs/V0.4/BuildProblemSeedData01_Assignment.md
Docs/V0.4/BuildSandboxConfigPanel01_Assignment.md
```

已完成前置：

```text
规则层
种子数据
V0.4 BuildSandbox Config Panel 01
```

## 3. 允许新增 / 修改范围

允许新增：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增 Editor-only 场景 Builder / Verifier：

```text
BattleSandboxPreviewSceneBuilder
BattleSandboxPreviewSceneVerifier
BattleSandboxPreviewSceneReportWriter
BuildSandboxGuardRunner 菜单 / batch 入口
```

建议菜单：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxPreviewScene01/[Writes Scene][Manual Only] Build Preview Scene
Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxPreviewScene01/[QA Only] Verify Preview Scene
```

## 4. 场景结构要求

场景根结构建议：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
├── EventSystem
├── BuildSandboxPreviewRoot
└── BuildSandboxPreviewCanvas
    ├── SafeAreaRoot
    ├── BattleLikePreviewArea
    │   ├── BoardGridPreview
    │   ├── ItemTrayPreview
    │   ├── SelectedItemInfo
    │   └── PlacementFeedback
    ├── ProblemSelectorPanel
    │   ├── MapRuleDropdownSlot
    │   ├── EnemyProblemDropdownSlot
    │   ├── BossProblemDropdownSlot
    │   └── DevChapterDropdownSlot
    ├── BuildSandboxDataPanelDock
    │   ├── BuildSummaryPanelSlot
    │   ├── SynergyPanelSlot
    │   ├── ShapeOccupancyPanelSlot
    │   ├── AffixModifierPanelSlot
    │   ├── ProblemReadinessPanelSlot
    │   └── SimulationResultPanelSlot
    ├── DevOnlyControlBar
    │   ├── RunSimulationButtonSlot
    │   ├── ResetPreviewButtonSlot
    │   └── ExportReportButtonSlot
    └── PopupLayer
```

本包只需要建立插槽和灰盒骨架。

正式数据绑定、PreviewContext、拖拽交互、数据面板显示放到后续包。

## 5. UI 手调锁规则

本包允许 Editor Builder 首次创建默认 UI 骨架。

一旦用户手调：

```text
Scene / RectTransform / sibling order / active / Image / Outline / Text
```

后续 Runtime 只能绑定和驱动，不得覆盖布局。

本包禁止在 Play / Runtime 自动重建场景 UI。

## 6. 禁止范围

禁止修改：

```text
Scene_TalismanBag_V02_FormationCounter
Scene_TalismanBag_V03_MainHome
Scene_TalismanBag_V03_TalismanUpgrade
ProjectSettings/EditorBuildSettings.asset，除非 Guard 另行批准
V02RunFlowController
V02FormationGridFrame
DamageText
RunFlow / PageState / FormationState
SaveData / PlayerPrefs / MainTrialProgressData
RewardConfig / DropTable / UpgradeConfig 正式表
正式 EnemyDefinition
正式 BossConfig
正式 V02RunConfig
正式 1-10 / 2-10
用户手调 RectTransform
```

禁止行为：

```text
把预览场景设为正式启动场景
从正式首页 / 试炼页进入该场景
把 devOnly 3-10 / 4-10 接入正式试炼
打开正式 FeatureFlag
让 devOnly 内容进入正式流程
BattlePageViewAdapter 反向写正式 UI
commit / tag / push
```

## 7. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattleSandboxPreviewSceneReport.md
Docs/V0.4/Reports/BattleSandboxPreviewSceneHierarchyReport.csv
Docs/V0.4/Reports/BattleSandboxPreviewSceneLeakCheckReport.md
```

报告必须说明：

```text
场景是否存在
根节点是否完整
Canvas / EventSystem 是否存在
BattleLikePreviewArea 是否存在
ProblemSelectorPanel 是否存在
BuildSandboxDataPanelDock 是否存在
DevOnlyControlBar 是否存在
PopupLayer 是否存在
是否修改 V02 / V03 正式场景
是否修改 BuildSettings
是否接入正式流程
```

## 8. 验收标准

必须满足：

```text
C# 编译通过
场景文件存在
Preview scene verifier 通过
场景层级报告完整
未修改 V02 / V03 正式场景
未修改 BuildSettings，除非用户另行批准
未接正式主线 / 存档 / 奖励 / 掉落 / Boss / 数值
FeatureFlag 全 false
devOnly 隔离成立
```

## 9. 用户手测建议

用户在 Unity 中打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

确认：

```text
场景能打开
能看到独立灰盒预览布局
中间有战斗整备风格预览区
左侧或顶部有题目选择区
右侧有 BuildSandboxDataPanelDock
顶部有 DevOnlyControlBar
不影响 V02 FormationCounter
不影响 V03 MainHome
Console 无本包红色 Error / 黄色 Warning
```
