# V0.3/V0.4-BuildSandbox-ItemShapeOccupancy01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_ITEMSHAPEOCCUPANCY01
```

## 1. 包定位

```text
道具占格形状 / 背包摆放检测沙盒底座
```

本包是 BuildSandbox 队列的 Package 2。目标是建立多格道具占用格子、形状配置、摆放检测、占格地图和报告输出。

本包不接正式战斗、不改正式整备 UI、不替换当前单格道具布局。

## 2. 上游前置

前置包已通过：

```text
V0.3/V0.4-BuildSandbox-GuardBaseline01
USER_ACCEPTED / QA_PASSED_BY_USER

V0.3/V0.4-BuildSandbox-SynergyDataFoundation01
USER_ACCEPTED / QA_PASSED_BY_USER
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/ItemShapeOccupancy01.md
```

## 3. 必须做

新增多格占位沙盒底座，至少包含：

```text
ItemShapeConfig
PlacementValidator
GridOccupancyMap
ShapePlacementResult / PlacementReport 数据结构
```

首批形状必须支持：

```text
Single1：1格
Vertical2：竖向2格
Corner3 / Triangle3：折角3格
Square4：2x2 方块4格
```

## 4. 形状定义

`Single1`：

```text
(0,0)
```

`Vertical2`：

```text
(0,0)
(0,1)
```

`Corner3 / Triangle3`：

```text
(0,0)
(1,0)
(0,1)
```

`Square4`：

```text
(0,0)
(1,0)
(0,1)
(1,1)
```

## 5. ItemShapeConfig 字段

建议字段：

```text
shapeId
shapeName
cellCount
occupiedOffsets[]
defaultRotation
rotationAllowed
visualKey
devOnly
isEnabled
```

所有 seed / 测试配置必须默认：

```text
devOnly = true
isEnabled = false
```

## 6. PlacementValidator 要求

必须支持：

```text
根据 anchorCell 计算 occupiedCells
判断越界
判断重叠
判断是否合法放置
输出非法原因
支持沙盒模拟放置
```

非法原因至少包括：

```text
OutOfGrid
CellOccupied
ShapeInvalid
MissingShapeConfig
```

## 7. BattleLayoutSnapshot 扩展方向

允许新增 BuildSandbox 独立快照结构或扩展沙盒快照字段：

```text
placedItems[]
```

每个 placedItem 包含：

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

注意：本包只允许沙盒快照 / 数据结构，不得接正式战斗 GameObject / UI。

## 8. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 Package 0 / 1 的 BuildSandbox 校验器和报告器，使其能检查形状配置、占格检测、devOnly 隔离。

允许输出：

```text
Docs/V0.4/Reports/ShapePlacementReport.md
Docs/V0.4/Reports/GridOccupancyReport.csv
```

## 9. 禁止范围

本包禁止：

```text
修改当前正式战斗 UI
修改当前正式整备交互
重建当前棋盘
重排 V02FormationGridFrame
强制替换当前单格布局
默认启用多格正式玩法
接正式掉落
接正式锻造 / 洗练
做复杂自由旋转
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改 Boss / 奖励 / 掉落 / 数值
修改正式 RewardConfig / UpgradeConfig
覆盖用户手调 UI
开发 SynergyEvaluator 深层羁绊计算
commit / tag / push
```

## 10. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
shapeId 是否为空
shapeId 是否重复
occupiedOffsets 是否为空
cellCount 是否等于 occupiedOffsets 数量
是否存在重复 offset
是否存在非法 offset
devOnly 是否为 true
isEnabled 是否为 false
FeatureFlag 是否默认 false
```

## 11. 报告要求

必须输出：

```text
Docs/V0.4/Reports/ShapePlacementReport.md
Docs/V0.4/Reports/GridOccupancyReport.csv
```

报告字段至少包含：

```text
itemId
shapeId
anchorCell
occupiedCells
isValid
invalidReason
adjacentItems
energyConnected
```

如当前尚未实现真实供能关系，可将 `energyConnected` 标为 sandbox placeholder，但不得伪造正式战斗供能结果。

## 12. 验收标准

必须满足：

```text
1. C# 编译通过。
2. 四种形状均可配置。
3. 四种形状均能计算 occupiedCells。
4. 越界检测正确。
5. 重叠检测正确。
6. MissingShapeConfig / ShapeInvalid 等非法原因能输出。
7. 沙盒快照能携带 occupiedCells。
8. FeatureFlag 仍全部默认 false。
9. 所有新增配置 devOnly=true / isEnabled=false。
10. ShapePlacementReport.md 与 GridOccupancyReport.csv 已输出。
11. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 13. 用户手测建议

本包仍是后台沙盒包，用户不需要验证正式玩法。

用户只需确认：

```text
当前 UI 手调没有被影响。
开发窗口报告中四种形状、越界、重叠、缺配置检测都通过。
FeatureFlag 仍为 false。
devOnly / isEnabled 隔离成立。
```

## 14. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-SynergyEvaluatorCore01
```

