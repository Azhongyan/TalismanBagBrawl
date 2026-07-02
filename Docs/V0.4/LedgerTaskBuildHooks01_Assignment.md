# V0.3/V0.4-BuildSandbox-LedgerTaskBuildHooks01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_LEDGERTASKBUILDHOOKS01
```

## 1. 包定位

```text
照灯账本 / 成长手册 Build 任务钩子
```

本包是 BuildSandbox 队列的 Package 9。目标是给后续照灯账本、成长手册、任务系统预留 Build 任务目标和事件钩子。

本包不做正式任务 UI，不接正式奖励，不接正式日常任务，不影响首页和正式玩家流程。

## 2. 上游前置

前置包已通过：

```text
GuardBaseline01
SynergyDataFoundation01
ItemShapeOccupancy01
SynergyEvaluatorCore01
AffixRaritySandbox01
ModifierEventBridge01
BuildSimulationBenchmark01
EnemyBossValidationPool01
DevChapterContentPool01
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/DevChapterContentPool01_Assignment.md
```

## 3. 必须做

新增 devOnly Build 任务钩子数据与事件模型，至少支持以下任务目标类型：

```text
激活一次 2 件羁绊
激活一次 4 件羁绊
激活一次 6 件羁绊
用惊雷 Build 击败护盾 Boss
用净厄 Build 清除负面状态
完成一次指定 Build 验证
获得一个橙色核心词条
完成一次词条洗练
```

全部配置默认：

```text
devOnly = true
isEnabled = false
entersFormalFlow = false
grantsFormalReward = false
```

## 4. 允许新增结构

允许新增或扩展：

```text
LedgerBuildTaskHook
LedgerBuildTaskGoal
LedgerBuildTaskEvent
LedgerBuildTaskProgressPreview
LedgerTaskBuildHooksValidator
LedgerTaskBuildHooksReportWriter
```

事件来源只能来自 BuildSandbox 沙盒结果：

```text
BuildEvaluationResult
BuildSimulationResult
Affix preview
EnemyBossValidationPool result
DevChapterContentPool result
```

不得读取正式任务系统、正式奖励系统或正式玩家进度。

## 5. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 BuildSandbox GuardRunner、ConfigValidator、ReportWriter，使其能输出 Build 任务钩子报告。

允许输出：

```text
Docs/V0.4/Reports/LedgerTaskBuildHooksReport.md
Docs/V0.4/Reports/LedgerBuildTaskGoalReport.csv
Docs/V0.4/Reports/LedgerBuildTaskLeakCheckReport.md
```

## 6. 禁止范围

本包禁止：

```text
做正式任务 UI
修改首页 / 照灯账本正式 UI
接正式任务列表
发正式奖励
接商业化任务
影响首页
读取或修改正式玩家任务进度
读取或修改 SaveData / MainTrialProgressData / PlayerPrefs
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改正式奖励 / 掉落 / 数值
修改正式 Boss / 敌人 / 章节
修改当前正式战斗 UI
修改当前正式整备交互
默认启用任何 BuildSandbox FeatureFlag
让 devOnly 内容进入正式流程
开发 ConfigValidatorReport01 总报告包
commit / tag / push
```

## 7. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
taskHook 是否 devOnly=true
taskHook 是否 isEnabled=false
taskHook 是否 entersFormalFlow=false
taskHook 是否 grantsFormalReward=false
taskGoal id 是否为空或重复
taskGoal 是否引用有效 BuildSandbox 事件
taskGoal 是否引用正式任务 / 奖励 / 玩家进度
是否存在 FeatureFlag 默认 true
是否泄漏到正式流程
```

## 8. 报告要求

必须输出：

```text
Docs/V0.4/Reports/LedgerTaskBuildHooksReport.md
Docs/V0.4/Reports/LedgerBuildTaskGoalReport.csv
Docs/V0.4/Reports/LedgerBuildTaskLeakCheckReport.md
```

报告至少包含：

```text
taskGoalId
中文目标描述
goalType
sourceEvent
requiredValue
devOnly
isEnabled
entersFormalFlow
grantsFormalReward
是否进入正式任务列表：必须为 false
```

## 9. 验收标准

必须满足：

```text
1. C# 编译通过。
2. Build 任务钩子可配置。
3. 能监听或消费 BuildSandbox 模拟事件。
4. 不进入正式任务列表。
5. 不发正式奖励。
6. 不读取或修改正式玩家任务进度。
7. 所有 task hook devOnly=true / isEnabled=false。
8. Config Validator 能检查任务目标引用和泄漏。
9. 报告完整输出。
10. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 10. 用户手测建议

本包仍是后台沙盒包，用户不需要进正式任务系统验证。

用户只需确认：

```text
当前 UI 手调没有被影响。
报告中任务钩子都标为 devOnly / isEnabled=false。
报告显示不进入正式任务列表、不发正式奖励。
FeatureFlag 仍为 false。
```

## 11. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-ConfigValidatorReport01
```

