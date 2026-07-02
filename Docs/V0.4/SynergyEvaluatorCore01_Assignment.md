# V0.3/V0.4-BuildSandbox-SynergyEvaluatorCore01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_SYNERGYEVALUATORCORE01
```

## 1. 包定位

```text
纯函数羁绊计算器包
```

本包是 BuildSandbox 队列的 Package 3。目标是实现只读取沙盒快照、只输出计算结果的 Build / 羁绊计算核心。

本包不接正式战斗、不修改 GameObject、不操作 UI、不产生正式战斗数值影响。

## 2. 上游前置

前置包已通过：

```text
V0.3/V0.4-BuildSandbox-GuardBaseline01
USER_ACCEPTED / QA_PASSED_BY_USER

V0.3/V0.4-BuildSandbox-SynergyDataFoundation01
USER_ACCEPTED / QA_PASSED_BY_USER

V0.3/V0.4-BuildSandbox-ItemShapeOccupancy01
USER_ACCEPTED / QA_PASSED_BY_USER
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/SynergyDataFoundation01_Assignment.md
Docs/V0.4/ItemShapeOccupancy01_Assignment.md
```

## 3. 必须做

新增纯函数羁绊计算底座，至少包含：

```text
SynergyEvaluator
BuildEvaluationResult
ActiveSynergyResult
SynergyRequirementResult
NextThresholdHint
```

输入：

```text
BuildSandboxLayoutSnapshot
```

输出：

```text
BuildEvaluationResult
```

## 4. 输入快照要求

本包只能读取 BuildSandbox 沙盒快照。

快照中 placedItem 可使用：

```text
itemId
shapeId
anchorCell
occupiedCells[]
rotation
tags[]
isPowered
energySourceId
affixList[]
rarity
```

不得直接读取或操作：

```text
Unity GameObject
Transform
RectTransform
正式战斗棋盘对象
正式 UI 对象
RunFlow / PageState / FormationState
```

## 5. 输出字段

`BuildEvaluationResult` 至少包含：

```text
activeSynergies
activeThresholds
sourceItems
placementSatisfied
energySatisfied
missingRequirements
nextThresholdHint
```

## 6. 支持条件

本包至少支持：

```text
数量型：拥有 N 件指定标签道具
邻接型：指定标签道具相邻
供能型：指定标签道具被供能
同源供能型：多个同标签道具由同一 energySourceId 供能
位置型：边缘 / 角落 / 阵眼周围 / 行列条件
混合型：数量 + 供能 + 邻接
```

第一版如部分高级条件只能输出 placeholder，也必须在报告中明确标记，不得伪造已完整接入正式战斗。

## 7. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展前置 BuildSandbox 校验器和报告器，使其能检查羁绊计算、阈值激活、缺失条件、nextThresholdHint。

允许输出：

```text
Docs/V0.4/Reports/SynergyEvaluatorCoreReport.md
Docs/V0.4/Reports/SynergyActivationReport.csv
```

## 8. 禁止范围

本包禁止：

```text
接入正式战斗
修改正式伤害 / 冷却 / 破盾 / 护盾 / 控制等数值
修改正式道具触发
修改当前正式战斗 UI
修改当前正式整备交互
操作正式棋盘 GameObject
重排 V02FormationGridFrame
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改 Boss / 奖励 / 掉落 / 数值
修改正式 RewardConfig / UpgradeConfig
默认启用任何 BuildSandbox FeatureFlag
让 devOnly 内容进入正式流程
开发 AffixRaritySandbox 词条系统
开发 ModifierEventBridge 正式桥接
commit / tag / push
```

## 9. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
synergyId 是否为空或重复
synergy 是否缺标签条件
threshold 是否为 2 / 4 / 6 / 8
threshold 是否缺效果描述或结果字段
placement / energy 条件是否引用有效字段
BuildEvaluationResult 是否可由 seed snapshot 生成
所有新增配置是否 devOnly=true
所有新增配置是否 isEnabled=false
所有 FeatureFlag 是否默认 false
```

## 10. 报告要求

必须输出：

```text
Docs/V0.4/Reports/SynergyEvaluatorCoreReport.md
Docs/V0.4/Reports/SynergyActivationReport.csv
```

报告至少包含：

```text
测试 Build 名称
输入 itemId / tags / occupiedCells / isPowered / energySourceId
激活羁绊
激活阈值
sourceItems
placementSatisfied
energySatisfied
missingRequirements
nextThresholdHint
是否触碰禁止范围
```

## 11. 验收标准

必须满足：

```text
1. C# 编译通过。
2. SynergyEvaluator 为纯函数或近似纯函数；不得产生正式战斗副作用。
3. 给定沙盒 Snapshot 能输出 BuildEvaluationResult。
4. 支持 2 / 4 / 6 / 8 阈值判断。
5. 支持数量型羁绊。
6. 支持基于 occupiedCells 的邻接判断。
7. 支持 isPowered / energySourceId 的供能条件判断。
8. 支持至少一种位置型条件判断。
9. 能输出 missingRequirements 与 nextThresholdHint。
10. FeatureFlag 仍全部默认 false。
11. 所有新增配置 devOnly=true / isEnabled=false。
12. SynergyEvaluatorCoreReport.md 与 SynergyActivationReport.csv 已输出。
13. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 12. 用户手测建议

本包仍是后台沙盒包，用户不需要验证正式玩法。

用户只需确认：

```text
当前 UI 手调没有被影响。
开发窗口报告中能看到羁绊激活、阈值、缺失条件和下一档提示。
FeatureFlag 仍为 false。
devOnly / isEnabled 隔离成立。
```

## 13. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-AffixRaritySandbox01
```

