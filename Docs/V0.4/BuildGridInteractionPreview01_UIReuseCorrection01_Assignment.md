# V0.4-BuildGridInteractionPreview01-UIReuseCorrection01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDGRIDINTERACTIONPREVIEW01_UIREUSE_CORRECTION01
```

## 1. 包定位

```text
V0.4 BuildGridInteractionPreview01 UI 复用纠偏包
```

本包不是重做 `BuildGridInteractionPreview01`，也不是继续精修 V04 沙盒 UI。

本包目标：

```text
把 V04 沙盒里新建的棋盘 / 背包 / 上拉 / 弹窗 UI 降级为 temporary preview。
明确哪些 V0.2 / V0.3 成熟 UI / 动画 / 交互手感应作为后续正式 Base Component。
建立 V0.4 BuildSandbox 数据 → Base Component 的 Extension Layer / Adapter Layer 映射草案。
```

## 2. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BuildGridInteractionPreview01_Assignment.md
Docs/V0.4/BattlePageViewAdapter01_Assignment.md
```

## 3. 允许范围

允许只读 Survey：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
V04 BuildGridInteractionPreview 相关脚本
V0.3 BattlePrepare 相关脚本 / 场景节点
V0.2 / V0.3 道具信息弹窗或 Tooltip 相关脚本 / 场景节点
BossInfoPanel / 战斗反馈 / 伤害提示 / CombatLog / Tooltip 相关脚本
```

允许新增 / 修改：

```text
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/** 仅限 QA / Report / Validator
```

允许新增报告：

```text
Docs/V0.4/Reports/UIReuseSourceSurveyReport.md
Docs/V0.4/Reports/BuildGridTemporaryUiReport.csv
Docs/V0.4/Reports/BuildSandboxToMatureUiAdapterMap.csv
Docs/V0.4/Reports/UIReuseCorrectionLeakCheckReport.md
```

## 4. 禁止范围

本包禁止：

```text
修改 V02 / V03 正式场景
修改 V04 Preview Scene 的 RectTransform / 布局 / 用户手调位置
继续重画或精修 V04 棋盘 / 背包 / 上拉动画 / 弹窗
修改 V03BattlePrepareInteractionController 正式交互
修改 V02FormationGridFrame
修改 RunFlow / PageState / FormationState
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改正式 EnemyDefinition / V02RunConfig / DropTable / BossConfig
commit / tag / push
```

## 5. 必须完成的 Survey

本包至少输出以下结论：

```text
1. V04 BuildGridInteractionPreview01 中哪些 UI 是 temporary preview。
2. 哪些能力应保留为逻辑验证：多格、旋转、越界、重叠、分类、PreviewContext。
3. 哪些 UI 手感应回收复用：背包上拉、棋盘区、道具栏滚动、拖拽、道具信息弹窗、战斗反馈。
4. V0.3 BattlePrepare 成熟 UI 的可复用源候选。
5. V0.2 / V0.3 道具信息弹窗 / Tooltip 的可复用源候选。
6. BossInfoPanel / 战斗提示 / 伤害反馈 / CombatLog / Tooltip 的可复用源候选。
7. 如果某项暂时不能复用，必须写明原因和风险，不得直接建议重画。
```

## 6. Extension / Adapter 映射草案

至少建立以下扩展层 / 字段层映射：

```text
BuildSandbox item preview → mature item info panel
BuildSandbox shape / occupied cells → mature item info extra section
BuildSandbox synergy contribution → mature item info extra section 或 developer data panel
BuildSandbox affix preview → mature item info extra section 或 developer data panel
BuildSandbox placement feedback → existing battle feedback / tooltip language
BuildProblem / BossProblem hint → BossInfo / battle hint language
Readiness / DropBias / hardSolutionTags → developer data panel only
```

同时必须判断以下 Extension Layer 候选：

```text
BoardOccupancyExtension：接入成熟棋盘组件，显示多格占用 / 越界 / 重叠。
ItemTrayShapeExtension：接入成熟道具栏组件，显示道具形状 / 分类 / 预览。
DragRotationPlacementExtension：接入成熟拖拽手感，增加旋转 / 合法性校验。
ItemInfoBuildFieldsExtension：接入成熟道具信息弹窗，增加形状 / 词条 / 羁绊 / 构筑贡献字段。
BattleFeedbackMechanicHintExtension：接入成熟战斗反馈 / Tooltip，显示机制线索与失败反馈。
BuildTuningDataPanelExtension：接入开发者数据面板，显示完整规则、key、阈值、DropBias、Readiness。
```

玩家侧：

```text
只显示中文线索 / 反馈 / 氛围提示。
不显示 hardSolutionTags / requiredSynergy / requiredAffix / requiredStats / DropBias 权重 / Boss 六钥匙完整答案。
```

开发者侧：

```text
可显示完整规则。
字段层必须有中文显示名 + English stable key。
```

## 7. 验收标准

必须满足：

```text
不改代码逻辑也可以通过，只要报告完整。
如新增 Validator / ReportWriter，C# 编译必须通过。
报告明确标记 V04 preview UI 的 temporary status。
报告明确列出 mature UI reuse source 候选。
报告明确列出 Extension Layer / Adapter map。
LeakCheck 显示未写正式场景、未改正式 UI、未触碰存档 / 奖励 / Boss / 数值。
```

## 8. 用户手测 / 审核方式

用户不需要重新调 UI。

用户只需要看报告判断：

```text
V04 临时 UI 有没有被降级。
V0.2 / V0.3 成熟 UI 是否被登记为复用源。
后续是否只做 Extension / Adapter，不再重画背包、棋盘、上拉动画和道具信息弹窗。
```

## 9. 下一步关系

本包通过后，再进入：

```text
V0.4-BuildTuningDataPanelPreview01
```

但 `BuildTuningDataPanelPreview01` 也必须遵守：

```text
开发者完整数据进入统一数据面板。
玩家 UI 不显示答案。
正式 UI 手感不在 V04 沙盒里重画。
```
