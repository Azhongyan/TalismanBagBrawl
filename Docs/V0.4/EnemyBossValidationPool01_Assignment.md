# V0.3/V0.4-BuildSandbox-EnemyBossValidationPool01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_ENEMYBOSSVALIDATIONPOOL01
```

## 1. 包定位

```text
Build 验证敌人 / Boss 池
```

本包是 BuildSandbox 队列的 Package 7。目标是建立专门验证不同 Build 的 devOnly 敌人 / Boss 数据池，供模拟器调用。

本包不是正式敌人包，不得进入正式 1-10 / 2-10，不得改正式敌人 / Boss / 奖励 / 数值。

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
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BuildSimulationBenchmark01_Assignment.md
```

## 3. 必须做

新增 devOnly 敌人 / Boss 验证池，至少包含：

```text
BuildSandboxEnemyProfile
BuildSandboxBossProfile
EnemyBossValidationPool
EnemyBossValidationPoolValidator
```

全部配置默认：

```text
devOnly = true
isEnabled = false
```

## 4. 敌人类型

至少包含：

```text
普通怪
护盾怪
毒怪
燃烧怪
偷灵怪
封符怪
群怪
高爆发怪
施法怪
厚血怪
阵眼干扰怪
```

可以用英文 id，但报告中必须能看出对应中文定位。

## 5. Boss 类型

至少包含：

```text
护盾Boss：验证惊雷
群怪Boss：验证离火
高爆发Boss：验证护阵
负面状态Boss：验证净厄
施法Boss：验证镇魂
供能干扰Boss：验证聚能
混合机制Boss：验证组合Build
```

## 6. 与模拟器关系

本包允许让 `BuildSimulationBenchmark01` 的沙盒模拟器读取 devOnly 敌人 / Boss profile。

禁止读取或修改正式敌人 / Boss 配置。

如需要测试数据，可新增 devOnly seed。

## 7. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 BuildSandbox GuardRunner、ConfigValidator、ReportWriter，使其能输出敌人 / Boss 验证池报告。

允许输出：

```text
Docs/V0.4/Reports/EnemyBossValidationPoolReport.md
Docs/V0.4/Reports/EnemyBuildMappingReport.md
Docs/V0.4/Reports/BossBuildValidationReport.csv
```

## 8. 禁止范围

本包禁止：

```text
进入正式 1-10 / 2-10
出现在正式掉落 / 主线 / 玩家入口
修改正式敌人池
修改正式 Boss
修改正式 Boss 触发规则
修改正式奖励 / 掉落 / 数值
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改当前正式战斗 UI
修改当前正式整备交互
默认启用任何 BuildSandbox FeatureFlag
让 devOnly 内容进入正式流程
开发 DevChapterContentPool 正式章节
commit / tag / push
```

## 9. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
Enemy / Boss profile 是否 devOnly=true
Enemy / Boss profile 是否 isEnabled=false
Enemy / Boss id 是否为空或重复
Enemy / Boss 是否带验证标签
Boss 是否有推荐 Build / 对应 Build 标签
是否引用正式敌人 / Boss / 章节配置
是否存在 FeatureFlag 默认 true
是否泄漏到正式流程
```

## 10. 报告要求

必须输出：

```text
Docs/V0.4/Reports/EnemyBossValidationPoolReport.md
Docs/V0.4/Reports/EnemyBuildMappingReport.md
Docs/V0.4/Reports/BossBuildValidationReport.csv
```

报告至少包含：

```text
enemyId / bossId
中文定位
验证目标 Build
验证标签
推荐羁绊
devOnly
isEnabled
是否进入正式流程：必须为 false
模拟器是否可读取
```

## 11. 验收标准

必须满足：

```text
1. C# 编译通过。
2. devOnly 敌人池可被模拟器调用。
3. devOnly Boss 池可被模拟器调用。
4. 至少 11 类敌人存在。
5. 至少 7 类 Boss 存在。
6. 所有 profile devOnly=true / isEnabled=false。
7. 不进入正式流程。
8. Config Validator 能发现误接正式流程。
9. 报告完整输出。
10. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 12. 用户手测建议

本包仍是后台沙盒包，用户不需要进正式战斗验证。

用户只需确认：

```text
当前 UI 手调没有被影响。
报告中敌人 / Boss 都标为 devOnly / isEnabled=false。
报告显示这些敌人 / Boss 只供模拟器读取。
正式 1-10 / 2-10 没出现这些内容。
```

## 13. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-DevChapterContentPool01
```

