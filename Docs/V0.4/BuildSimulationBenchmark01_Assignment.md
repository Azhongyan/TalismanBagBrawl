# V0.3/V0.4-BuildSandbox-BuildSimulationBenchmark01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_BUILDSIMULATIONBENCHMARK01
```

## 1. 包定位

```text
Build 自动模拟与 Benchmark 报告包
```

本包是 BuildSandbox 队列的 Package 6。目标是建立批量测试不同 Build 组合的沙盒模拟器和报告输出。

本包不是正式战斗模拟替代包，不得接正式玩家流程，不得影响正式战斗结果。

## 2. 上游前置

前置包已通过：

```text
GuardBaseline01
SynergyDataFoundation01
ItemShapeOccupancy01
SynergyEvaluatorCore01
AffixRaritySandbox01
ModifierEventBridge01
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/ModifierEventBridge01_Assignment.md
```

## 3. 必须做

新增 BuildSandbox 自动模拟与 Benchmark 底座，至少包含：

```text
BuildSimulationRunner
BuildSimulationScenario
BuildSimulationResult
BuildComparisonResult
BuildSimulationBenchmarkReport
```

必须能使用前置包产物：

```text
BuildSandboxLayoutSnapshot
SynergyEvaluator
Affix preview
CombatModifierBundle preview
EffectEventBundle preview
```

## 4. 必须支持的模拟输入

至少支持：

```text
指定 Build 组合
指定敌人类型
指定 Boss 机制
指定道具品质
指定词条组合
指定供能情况
指定摆放关系
批量跑模拟
输出报告
```

如果当前没有真实敌人 / Boss 验证池，本包可以使用 devOnly placeholder 敌人 / Boss 标签，不得引用正式敌人池。

## 5. 输出报告

必须输出：

```text
Docs/V0.4/Reports/BuildSimulationReport.md
Docs/V0.4/Reports/BuildComparison.csv
Docs/V0.4/Reports/SynergyActivationReport.csv
Docs/V0.4/Reports/AffixImpactReport.csv
```

报告字段至少包含：

```text
Build名称
激活羁绊
羁绊等级
通关时间 / 模拟耗时占位
胜率 / 模拟胜率占位
剩余血量 / 模拟剩余血量占位
总伤害 / 模拟总伤害占位
破盾效率
能量覆盖率
触发事件次数
失败原因
是否数值异常
```

若某些字段当前是沙盒估算 / placeholder，必须明确标记为 sandbox estimate，不得伪装成正式战斗数据。

## 6. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 BuildSandbox GuardRunner、ConfigValidator、ReportWriter，使其能批量生成 Benchmark 报告。

## 7. 禁止范围

本包禁止：

```text
接入正式战斗
读取正式玩家存档
修改正式敌人 / Boss / 奖励 / 数值配置
修改正式伤害结算
修改正式道具触发
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改当前正式战斗 UI
修改当前正式整备交互
默认启用任何 BuildSandbox FeatureFlag
让 devOnly 内容进入正式流程
开发 EnemyBossValidationPool 正式内容
commit / tag / push
```

## 8. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
Scenario 是否 devOnly=true
Scenario 是否 isEnabled=false
Scenario 是否引用正式敌人 / Boss / 章节
Build 输入是否为空
是否存在 FeatureFlag 默认 true
Benchmark 输出是否标记 sandbox / devOnly
是否引用正式存档或正式玩家数据
```

## 9. 验收标准

必须满足：

```text
1. C# 编译通过。
2. 能跑单个 Build 模拟。
3. 能跑批量 Build 模拟。
4. 能输出 md 和 csv 报告。
5. 能比较有无羁绊 / 有无词条。
6. 能使用 Modifier / Event preview 结果。
7. FeatureFlag 关闭时正式流程不受影响。
8. 所有 scenario / seed 默认 devOnly=true / isEnabled=false。
9. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 10. 用户手测建议

本包仍是后台沙盒包，用户不需要验证正式玩法。

用户只需确认：

```text
当前 UI 手调没有被影响。
报告显示 Benchmark 是 sandbox / devOnly。
FeatureFlag 仍为 false。
devOnly / isEnabled 隔离成立。
正式战斗没有被接入。
```

## 11. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-EnemyBossValidationPool01
```

