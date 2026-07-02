# V0.4-BuildSandboxPreviewContext01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOXPREVIEWCONTEXT01
```

## 1. 包定位

```text
V0.4 BuildSandbox 预览上下文 / ViewModel 包
```

本包目标是建立一个统一、只读、devOnly 的数据上下文，把 Phase 1 上层发动机与 Phase 2 题库数据整理成后续 UI 面板可展示的数据结构。

本包不做 UI 面板，不做拖拽交互，不接正式战斗，不改正式场景。

建议核心命名：

```text
BuildSandboxPreviewContext
BuildSandboxPreviewViewModel
```

## 2. 上游前置

必须读取并遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/BuildProblemRulePool01_Assignment.md
Docs/V0.4/BuildProblemSeedData01_Assignment.md
Docs/V0.4/BuildSandboxConfigPanel01_Assignment.md
Docs/V0.4/BattleSandboxPreviewScene01_Assignment.md
```

已完成前置：

```text
Phase 1 BuildSandbox 上层发动机
BuildProblemRulePool01
BuildProblemSeedData01
BuildSandboxConfigPanel01
BattleSandboxPreviewScene01
```

## 3. 允许新增 / 修改范围

允许新增或修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增：

```text
BuildSandboxPreviewContext
BuildSandboxPreviewViewModel
BuildSandboxPreviewContextBuilder
BuildSandboxPreviewContextValidator
BuildSandboxPreviewContextReportWriter
BuildSandboxGuardRunner 菜单 / batch 入口
```

建议菜单：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/BuildSandboxPreviewContext01/[QA Only] Run Preview Context
```

## 4. 数据输入

PreviewContext 可以读取 / 调用：

```text
BuildProblemSeedData
SynergyEvaluator
GridOccupancyMap
ItemShapePlacementValidator
AffixRaritySandbox preview
ModifierEventBridge preview
BuildSimulationBenchmark preview
EnemyBossValidationPool
DevChapterContentPool
LedgerTaskBuildHooks
ConfigValidator
```

## 5. 输出 ViewModel 结构

至少提供以下只读输出：

```text
BuildSummaryViewModel
SynergyViewModel
ShapeOccupancyViewModel
AffixModifierViewModel
ProblemReadinessViewModel
SimulationResultViewModel
ProblemSelectorViewModel
```

输出需要覆盖：

```text
当前可选地图规则数量
当前可选敌人题目数量
当前可选 Boss 题目数量
当前 Build 标签 / 羁绊 / 阈值
多格占位合法性
词条 / 品质 / 橙色核心词条 / 羁绊+1
Modifier / Event preview
Boss 六钥匙 Readiness
失败原因 / 推荐补法
DropBias devOnly 倾向
模拟结果摘要
```

## 6. 本包不做

本包不做：

```text
不创建 UI 面板
不向场景挂 MonoBehaviour，除非只是 devOnly marker / verifier 所需
不写 Scene_TalismanBag_V04_BattleSandboxPreview.unity
不修改 BuildSettings
不接正式战斗
不接正式 SaveData / 奖励 / 掉落 / Boss / 数值
不修改 V02 / V03 正式场景
不实现拖拽交互
```

## 7. 红线禁止

禁止修改：

```text
Scene_TalismanBag_V02_FormationCounter
Scene_TalismanBag_V03_MainHome
Scene_TalismanBag_V03_TalismanUpgrade
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
从正式系统读取玩家存档作为 PreviewContext 输入
把 PreviewContext 写回正式系统
BattlePageViewAdapter 反向写正式 UI
FeatureFlag 默认 true
devOnly = false
isEnabled = true
commit / tag / push
```

## 8. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BuildSandboxPreviewContextReport.md
Docs/V0.4/Reports/BuildSandboxPreviewViewModelReport.csv
Docs/V0.4/Reports/BuildSandboxPreviewReadinessReport.csv
Docs/V0.4/Reports/BuildSandboxPreviewContextLeakCheckReport.md
```

报告必须说明：

```text
Context 是否可创建
ViewModel 输出数量
MapRule / EnemyProblem / BossProblem 数量
Synergy / Shape / Affix / Modifier / Event 是否接入 preview
Readiness 是否有输出
Simulation 是否有摘要
是否读取正式 SaveData
是否写正式流程
```

## 9. 验收标准

必须满足：

```text
C# 编译通过
PreviewContext 菜单 / batch 可运行
报告 PASS
FeatureFlag 全 false
devOnly / isEnabled 隔离成立
正式流程泄漏检查通过
不修改 V04 Preview Scene
不修改 V02 / V03 正式场景
不读取正式存档
不写正式数据
```

## 10. 用户验收建议

本包仍是后台数据上下文包。

用户只需确认：

```text
Unity 菜单运行无红色 Error / 黄色 Warning
BuildSandboxPreviewContextReport.md 显示 PASS
ViewModel 报告里能看到 MapRule=10 / EnemyProblem=10 / BossProblem=6
Readiness 报告有 Boss 六钥匙输出
LeakCheck 无正式流程泄漏
```
