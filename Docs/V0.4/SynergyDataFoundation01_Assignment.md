# V0.3/V0.4-BuildSandbox-SynergyDataFoundation01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_SYNERGYDATAFOUNDATION01
```

## 1. 包定位

```text
Build / 羁绊 / 标签数据底座包
```

本包是 BuildSandbox 队列的 Package 1。目标是一次性建立 Build、羁绊、标签、阈值和 Archetype 的沙盒数据结构。

本包不接正式战斗、不接正式掉落、不改正式 UI、不改主流程。

## 2. 上游前置

前置包已通过：

```text
V0.3/V0.4-BuildSandbox-GuardBaseline01
USER_ACCEPTED / QA_PASSED_BY_USER
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/GuardBaseline01_Assignment.md
```

## 3. 必须做

新增 BuildSandbox 数据底座，至少包含：

```text
ItemTagConfig
SynergyConfig
SynergyThresholdConfig
PlacementConditionConfig
EnergyConditionConfig
BuildArchetypeConfig
```

允许根据工程风格拆分为 ScriptableObject、纯 C# 数据类、JSON-like 配置或最小可验证资产，但必须保持独立沙盒目录，不接正式流程。

## 4. 首批标签

必须预留：

```text
离火
惊雷
护阵
净厄
镇魂
聚能
剑器
符箓
法器
黑炉污染
```

标签字段建议包含：

```text
itemId
tags[]
baseFunction
energyCost
itemCategory
rarityAllowed
devOnly
isEnabled
```

## 5. 羁绊阈值

结构必须支持：

```text
2件
4件
6件
8件
```

不是每个羁绊都必须配置满 8 件，但数据结构必须支持。

## 6. 允许新增 / 修改范围

允许在独立沙盒目录新增：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 Package 0 中已有的 BuildSandbox 校验器，使其能检查本包新增配置。

允许输出：

```text
Docs/V0.4/Reports/SynergyDataFoundationReport.md
```

## 7. 禁止范围

本包禁止：

```text
接入正式战斗
接入正式掉落
接入正式锻造 / 洗练
修改当前 UI 场景或 RectTransform
修改首页 / BottomBar / BattlePrepare / Upgrade 页布局
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改 Boss / 奖励 / 掉落 / 数值
修改正式 RewardConfig / UpgradeConfig
默认启用任何 BuildSandbox FeatureFlag
让 devOnly 内容进入正式流程
开发 SynergyEvaluator 计算逻辑
开发 ItemShapeOccupancy 多格占位逻辑
commit / tag / push
```

## 8. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
道具标签配置是否缺 itemId
tags 是否为空
是否存在重复 tag
synergyId 是否重复
羁绊是否缺阈值
阈值是否只使用 2 / 4 / 6 / 8
所有新增配置是否 devOnly=true
所有新增配置是否 isEnabled=false
所有 BuildSandbox FeatureFlag 是否默认 false
```

## 9. 交付报告

本包应输出：

```text
Docs/V0.4/Reports/SynergyDataFoundationReport.md
```

报告至少包含：

```text
新增配置类型
首批标签清单
羁绊阈值支持情况
devOnly / isEnabled 检查结果
FeatureFlag 默认关闭检查结果
是否触碰禁止范围
下一包是否可进入
```

## 10. 验收标准

必须满足：

```text
1. C# 编译通过。
2. ItemTagConfig / SynergyConfig / SynergyThresholdConfig 等数据底座存在。
3. 首批 10 个标签可配置。
4. 羁绊阈值结构支持 2 / 4 / 6 / 8。
5. 所有新增配置默认 devOnly=true / isEnabled=false。
6. FeatureFlag 仍全部默认 false。
7. Config Validator 能检查本包新增配置。
8. 输出 SynergyDataFoundationReport.md。
9. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 11. 用户手测建议

本包仍是后台数据底座包，用户不需要验证正式玩法。

用户只需确认：

```text
当前 UI 手调没有被影响。
开发窗口报告中 FeatureFlag 仍为 false。
devOnly / isEnabled 隔离成立。
首批标签和阈值结构已建立。
```

## 12. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-ItemShapeOccupancy01
```

