# ItemShapeOccupancy01 / 道具占格形状沙盒底座

本文件记录 BuildSandbox 队列中 `ItemShapeOccupancy01` 的长期口径。它不是当前首包；当前首包是 `GuardBaseline01`。

## 1. 定位

```text
道具占格形状 / 背包摆放检测沙盒底座
```

用于后续：

```text
背包乱斗式道具摆放
多格道具占位
邻接判断
供能判断
阵法羁绊判断
Build 模拟
敌人 / Boss 验证池
```

本包先作为数据与沙盒底座，不默认接入当前正式主流程。

## 2. 首批形状

| 格子数 | 形状 | 说明 |
| ---: | --- | --- |
| 1 格 | `Single1` | 1×1 |
| 2 格 | `Vertical2` | 1×2 竖向 |
| 3 格 | `Corner3 / Triangle3` | 折角三格 |
| 4 格 | `Square4` | 2×2 |

## 3. 数据结构方向

新增：

```text
ItemShapeConfig
PlacementValidator
GridOccupancyMap
```

`ItemShapeConfig` 字段：

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

ItemConfig 可扩展字段：

```text
shapeId
cellCount
anchorType
canRotate
```

## 4. 摆放检测

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

## 5. BattleLayoutSnapshot 扩展方向

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

SynergyEvaluator 只能读取该快照，不得直接操作 GameObject / UI。

## 6. 禁止

```text
不改当前正式战斗 UI
不改当前正式整备交互
不强制替换当前单格布局
不默认启用多格正式玩法
不接正式掉落
不接正式锻造
不做复杂自由旋转
不重建当前棋盘
不覆盖用户手调 UI
```

## 7. 报告

未来本包应输出：

```text
ShapePlacementReport.md
GridOccupancyReport.csv
```

报告字段：

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

