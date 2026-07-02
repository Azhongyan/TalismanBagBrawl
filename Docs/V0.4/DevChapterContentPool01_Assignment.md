# V0.3/V0.4-BuildSandbox-DevChapterContentPool01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_DEVCHAPTERCONTENTPOOL01
```

## 1. 包定位

```text
开发用章节 / 关卡配置池
```

本包是 BuildSandbox 队列的 Package 8。目标是创建一批 devOnly 开发章节 / 关卡配置，供 Build 验证和模拟器调用。

本包不是正式章节包，不得进入正式主线、正式关卡入口、正式奖励或正式掉落。

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
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/EnemyBossValidationPool01_Assignment.md
```

## 3. 必须做

新增 devOnly 开发章节 / 关卡配置池，至少包含：

```text
DevBuildTest_Thunder_01~10
DevBuildTest_Fire_01~10
DevBuildTest_Shield_01~10
DevBuildTest_Cleanse_01~10
DevBuildTest_Control_01~10
DevBuildTest_Energy_01~10
DevBossTest_Mixed_01~05
```

全部配置默认：

```text
devOnly = true
isEnabled = false
```

## 4. 与模拟器关系

本包允许让 `BuildSimulationBenchmark01` 的沙盒模拟器读取 devOnly chapter profile。

章节可引用前置 `EnemyBossValidationPool01` 中的 devOnly 敌人 / Boss profile。

禁止引用正式主线章节、正式敌人池、正式 Boss 池或正式奖励。

## 5. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 BuildSandbox GuardRunner、ConfigValidator、ReportWriter，使其能输出开发章节池报告。

允许输出：

```text
Docs/V0.4/Reports/DevChapterContentPoolReport.md
Docs/V0.4/Reports/DevChapterBuildMappingReport.csv
Docs/V0.4/Reports/DevChapterLeakCheckReport.md
```

## 6. 禁止范围

本包禁止：

```text
接正式主线
修改正式 StageConfig
显示在正式关卡入口
影响 1-10 / 2-10
引用正式敌人 / Boss / 奖励 / 掉落
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改当前正式战斗 UI
修改当前正式整备交互
默认启用任何 BuildSandbox FeatureFlag
让 devOnly 内容进入正式流程
开发 LedgerTaskBuildHooks 正式任务
commit / tag / push
```

## 7. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
Chapter profile 是否 devOnly=true
Chapter profile 是否 isEnabled=false
Chapter id 是否为空或重复
Chapter 是否引用正式 StageConfig / RunFlow / 主线入口
Chapter 引用的敌人 / Boss 是否均为 devOnly
Chapter 是否带 Build 验证目标
是否存在 FeatureFlag 默认 true
是否泄漏到正式流程
```

## 8. 报告要求

必须输出：

```text
Docs/V0.4/Reports/DevChapterContentPoolReport.md
Docs/V0.4/Reports/DevChapterBuildMappingReport.csv
Docs/V0.4/Reports/DevChapterLeakCheckReport.md
```

报告至少包含：

```text
chapterId
中文定位
验证目标 Build
引用 enemyId / bossId
devOnly
isEnabled
是否进入正式流程：必须为 false
模拟器是否可读取
```

## 9. 验收标准

必须满足：

```text
1. C# 编译通过。
2. Dev 关卡能被模拟器调用。
3. Dev 关卡不会进入正式入口。
4. Dev 关卡引用的敌人 / Boss 都是 devOnly。
5. 至少 65 个 dev chapter / boss test profile 存在。
6. 所有 profile devOnly=true / isEnabled=false。
7. Config Validator 能发现 devOnly 泄漏。
8. 报告完整输出。
9. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 10. 用户手测建议

本包仍是后台沙盒包，用户不需要进正式关卡验证。

用户只需确认：

```text
当前 UI 手调没有被影响。
报告中章节 / 关卡都标为 devOnly / isEnabled=false。
报告显示它们只供模拟器读取。
正式 1-10 / 2-10 没出现这些内容。
```

## 11. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-LedgerTaskBuildHooks01
```

