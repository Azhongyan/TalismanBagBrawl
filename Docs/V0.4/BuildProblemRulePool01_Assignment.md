# V0.4-BuildProblemRulePool01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDPROBLEMRULEPOOL01
```

## 1. 包定位

```text
Build 验证题库规则层
```

本包只建立 V0.4 BuildSandbox 的题库规则结构，不开发正式章节，不接正式战斗，不做可玩 UI。

它解决：

```text
地图规则怎么出题
普通怪怎么暴露 Build 缺口
阻尼关怎么提示玩家补构筑
Boss 怎么用多钥匙锁检查 Build 完整度
BuildReadiness 怎么报告破题准备度
DropBias 怎么服务 devOnly 定向掉落倾向
```

## 2. 上游前置

必须读取并遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
```

Phase 1 可复用的上层发动机：

```text
SynergyEvaluator
ItemShapeOccupancy
AffixRaritySandbox
ModifierEventBridge
BuildSimulationBenchmark
EnemyBossValidationPool
DevChapterContentPool
LedgerTaskBuildHooks
ConfigValidator
FinalIntegrationDryRun 报告体系
```

## 3. 允许新增 / 修改范围

允许新增或修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增配置 / 数据结构：

```text
MapRuleConfig
EnemyProblemConfig
BossProblemConfig
BuildReadinessCheckConfig
WeaknessWindowConfig
DropBiasConfig
FailureHintConfig
BuildProblemRuleSet
```

允许新增 Editor-only 验证与报告：

```text
BuildProblemRulePoolValidator
BuildProblemRulePoolReportWriter
BuildSandboxGuardRunner 菜单 / batch 入口
```

建议菜单：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/BuildProblemRulePool01/[QA Only] Run Build Problem Rule Pool
```

## 4. 本包不负责的内容

本包不填完整题库种子。完整数据丰满放到下一包：

```text
V0.4-BuildProblemSeedData01
```

本包不做：

```text
独立沙盒场景
V0.4 配置面板
拖拽交互
可视化数据面板
3-10 / 4-10 devOnly 章节流
正式主线接入
```

## 5. 必须默认字段

所有新增配置默认：

```text
devOnly = true
isEnabled = false
```

所有相关 FeatureFlag 默认：

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
BattlePageViewAdapter 反向写正式 UI
commit / tag / push
```

## 7. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BuildProblemRulePoolReport.md
Docs/V0.4/Reports/MapRuleSchemaReport.csv
Docs/V0.4/Reports/EnemyProblemSchemaReport.csv
Docs/V0.4/Reports/BossProblemKeySchemaReport.csv
Docs/V0.4/Reports/BuildReadinessSchemaReport.md
Docs/V0.4/Reports/DropBiasLeakCheckReport.md
```

报告必须说明：

```text
规则层状态 PASS / FAIL
新增配置类型
devOnly / isEnabled 默认值
FeatureFlag 状态
是否引用正式流程
是否触碰正式数据
下一包 SeedData 需要补充的数据量
```

## 8. 验收标准

必须满足：

```text
C# 编译通过
BuildProblemRulePool 菜单 / batch 可运行
FeatureFlag 全 false
所有配置 devOnly=true / isEnabled=false
Formal flow leak check 通过
无正式 StageConfig / Reward / Drop / Boss / SaveData 接入
无 Runtime UI / RectTransform 写入
报告完整输出
```

## 9. 用户验收建议

本包仍是后台规则结构包。

用户只需确认：

```text
菜单运行无红色 Error / 黄色 Warning
BuildProblemRulePoolReport.md 显示 PASS
DropBiasLeakCheckReport.md 无正式掉落泄漏
报告里明确下一包需要补 10 地图规则 / 10 敌人题目 / 6 Boss 种子
```

