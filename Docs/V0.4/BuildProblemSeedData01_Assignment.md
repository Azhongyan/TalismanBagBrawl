# V0.4-BuildProblemSeedData01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDPROBLEMSEEDDATA01
```

## 1. 包定位

```text
Build 验证题库种子数据包
```

本包基于 `V0.4-BuildProblemRulePool01` 已建立的规则层，填充第一批 devOnly 题库种子。

本包仍然不是正式章节开发，不接正式战斗，不修改 V0.2 / V0.3 正式数据。

## 2. 上游前置

必须读取并遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/BuildProblemRulePool01_Assignment.md
```

上游已完成：

```text
MapRuleConfig
EnemyProblemConfig
BossProblemConfig
BuildReadinessCheckConfig
WeaknessWindowConfig
DropBiasConfig
FailureHintConfig
```

## 3. 允许新增 / 修改范围

允许在独立 BuildSandbox 范围新增或修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增 devOnly seed provider / static seed / ScriptableObject seed 生成器。

允许扩展：

```text
BuildProblemRulePoolValidator
BuildProblemRulePoolReportWriter
BuildSandboxGuardRunner
```

## 4. 必须填充的数据规模

### 4.1 地图规则

至少 10 条：

```text
黑炉烟尘
青石回潮
灯火不足
铜铃夜响
阵眼偏移
符纸受潮
旧巷回音
炉灰落阵
夜巡灯灭
青石裂纹
```

每条至少包含：

```text
mapRuleId
displayName
description
affectedTags
buffTags / debuffTags
placementModifier / energyModifier / cooldownModifier
warningText
devOnly=true
isEnabled=false
```

### 4.2 敌人题目

至少 10 类：

```text
护盾
群怪
毒 / 燃
偷灵
封符
高爆发
施法
厚血
污染格
阵眼干扰
```

每类至少包含：

```text
problemType
pressureType
hardSolutionTags
softSolutionTags
validatedBuildTags
failureHint
recommendedAction
devOnly=true
isEnabled=false
```

### 4.3 Boss 题目

至少 6 个 devOnly Boss：

```text
黑炉护壳师：验证惊雷破盾 Build
秽签梦母：验证净厄反制 Build
偷灵炉心：验证聚能 / 镇魂 / 供能稳定 Build
叩阵铜将：验证护阵防守 Build
纸人千面阵：验证离火 / 清场 / 连锁 Build
黑炉复合阵眼：验证综合 Build 毕业考试
```

每个 Boss 至少检查 3-5 把钥匙：

```text
羁绊钥匙
关键道具钥匙
关键词条钥匙
破题属性钥匙
摆放形态钥匙
供能钥匙
```

### 4.4 破题属性

必须覆盖：

```text
BreakPower
CleansePower
ControlPower
GuardPower
EnergyStability
ClearPower
BurstWindow
```

### 4.5 弱点窗口

每个 Boss 至少 1 个弱点窗口。

建议覆盖：

```text
连续破三层护壳 → Boss 虚弱
镇魂打断蓄力 → Boss 僵直
净化三次污染 → Boss 本体显露
护阵挡住敲阵 → Boss 被反震
清掉全部召唤物 → Boss 核心暴露
供能链反制成功 → 玩家全阵爆发一次
```

### 4.6 DropBias

每个 Boss 至少 2-3 条 devOnly DropBias。

DropBias 只能作为沙盒倾向报告，不得接正式掉落表。

## 5. 必须默认字段

所有 seed 默认：

```text
devOnly = true
isEnabled = false
```

所有 FeatureFlag 默认：

```text
false
```

## 6. 红线禁止

禁止修改：

```text
V02RunFlowController
V02FormationGridFrame
DamageText
RunFlow / PageState / FormationState
SaveData / MainTrialProgressData / PlayerPrefs
RewardConfig / DropTable / UpgradeConfig 正式表
正式 EnemyDefinition
正式 BossConfig
正式 V02RunConfig
正式 1-10 / 2-10
V03 MainHome / BattlePrepare / TalismanUpgrade 场景
用户手调 RectTransform
```

禁止行为：

```text
3-10 / 4-10 devOnly 章节进入正式主线
Boss 验证池进入正式 Boss 列表
DropBias 改正式掉落表
BuildReadinessCheck 改正式 BossInfoPanel
MapRule 默认启用
FeatureFlag 默认 true
devOnly = false
commit / tag / push
```

## 7. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BuildProblemSeedDataReport.md
Docs/V0.4/Reports/MapRuleSeedReport.csv
Docs/V0.4/Reports/EnemyProblemSeedReport.csv
Docs/V0.4/Reports/BossProblemSeedReport.csv
Docs/V0.4/Reports/WeaknessWindowSeedReport.csv
Docs/V0.4/Reports/DropBiasSeedReport.csv
Docs/V0.4/Reports/BuildProblemSeedLeakCheckReport.md
```

报告必须说明：

```text
地图规则数量
敌人题目数量
Boss 题目数量
每个 Boss 的钥匙覆盖数
弱点窗口覆盖
DropBias 覆盖
devOnly / isEnabled 状态
正式流程泄漏检查
```

## 8. 验收标准

必须满足：

```text
C# 编译通过
BuildProblemSeedData 菜单 / batch 可运行
FeatureFlag 全 false
所有 seed devOnly=true / isEnabled=false
Formal flow leak check 通过
DropBias 未引用正式掉落表
BossProblem 未引用正式 Boss 列表
无正式 StageConfig / SaveData / Reward / Drop / Boss / UI 接入
报告完整输出
```

## 9. 用户验收建议

本包仍是后台题库数据包。

用户只需确认：

```text
菜单运行无红色 Error / 黄色 Warning
BuildProblemSeedDataReport.md 显示 PASS
10 地图规则 / 10 敌人题目 / 6 Boss 都在报告中
DropBiasSeedReport.csv 只是 devOnly 倾向
LeakCheck 无正式流程泄漏
```
