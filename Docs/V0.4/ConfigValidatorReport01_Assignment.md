# V0.3/V0.4-BuildSandbox-ConfigValidatorReport01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_CONFIGVALIDATORREPORT01
```

## 1. 包定位

```text
配置校验与报告导出包
```

本包是 BuildSandbox 队列的 Package 10。目标是把前面所有 BuildSandbox 子系统的配置校验、覆盖率报告、泄漏检查和报告导出统一收口。

本包不开发新玩法，不接正式流程，不自动修配置。

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
LedgerTaskBuildHooks01
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
```

## 3. 必须做

建立统一配置校验与报告导出，至少覆盖：

```text
FeatureFlag 默认关闭
devOnly / isEnabled 隔离
道具标签完整性
羁绊阈值完整性
词条目标有效性
Modifier / Event 映射有效性
敌人 / Boss 验证标签
DevChapter 泄漏检查
LedgerTask 任务钩子泄漏检查
多格形状合法性
占格越界 / 重叠 / 缺配置检查
```

## 4. 必须输出

至少输出：

```text
Docs/V0.4/Reports/ConfigValidationReport.md
Docs/V0.4/Reports/SynergyCoverageReport.md
Docs/V0.4/Reports/EnemyBuildMappingReport.md
Docs/V0.4/Reports/AffixPoolReport.md
Docs/V0.4/Reports/ShapePlacementReport.md
Docs/V0.4/Reports/GridOccupancyReport.csv
Docs/V0.4/Reports/BuildSandboxLeakCheckReport.md
```

如前置报告已存在，本包可以复用并统一索引，但必须生成总报告。

## 5. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 BuildSandbox GuardRunner、ConfigValidator、ReportWriter。

允许新增统一菜单入口，例如：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/ConfigValidatorReport01/[QA Only] Run Config Validator Report
```

## 6. 禁止范围

本包禁止：

```text
自动修配置
删除资产
修改正式配置
修改 FeatureFlag 默认值为 true
接正式主线
接正式掉落 / 锻造 / 洗练 / 任务 / 奖励
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改正式 Boss / 敌人 / 章节 / 数值
修改当前正式战斗 UI
修改当前正式整备交互
开发 FinalIntegrationDryRun 正式干跑
commit / tag / push
```

## 7. 校验项

必须校验：

```text
道具是否有标签
词条是否有可用目标
羁绊是否有阈值
羁绊效果是否有 Modifier / Event 映射
敌人是否有验证标签
Boss 是否有推荐 Build
devOnly 内容是否未进入正式流程
FeatureFlag 是否默认关闭
多格形状是否配置合法
占格是否越界 / 重叠
DevChapter 是否泄漏到正式主线
LedgerTask 是否进入正式任务列表或发正式奖励
```

## 8. 报告要求

`ConfigValidationReport.md` 至少包含：

```text
总状态：PASS / FAIL
各子系统状态
错误数 / 警告数
FeatureFlag 状态
devOnly / isEnabled 状态
正式流程泄漏检查
报告索引
下一包是否可进入
```

## 9. 验收标准

必须满足：

```text
1. C# 编译通过。
2. 统一 Config Validator 可运行。
3. 所有报告完整输出。
4. 能发现缺标签。
5. 能发现无效词条目标。
6. 能发现无阈值羁绊。
7. 能发现 devOnly 泄漏。
8. 能发现 FeatureFlag 默认开启错误。
9. 能发现形状配置错误。
10. 不自动修配置、不删除资产。
11. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 10. 用户手测建议

本包仍是后台沙盒包。

用户只需确认：

```text
非 Play 状态菜单可运行。
Console 无本包红色 Error / 黄色 Warning。
ConfigValidationReport.md 显示 PASS。
报告索引能打开。
当前 UI 手调没有被影响。
```

## 11. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01
```

