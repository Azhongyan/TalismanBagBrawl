# V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix02-ShapeAwareItemTray Assignment

Guard 回执：

```text
GUARD_PASS_RUNTIME_PLAYTEST_FIXTURE_FEEDBACK_FIX02_SHAPE_AWARE_ITEMTRAY
```

## 1. 包定位

```text
V0.4 devOnly 多格测试道具的道具栏形状化显示修正包
```

本包修正 `FixtureFeedbackFix01` 的缺口。

已确认：

```text
devOnly 测试道具已能进入成熟 BattlePrepare 托盘。
拖到棋盘后可看到多格 / 占格反馈。
```

但用户确认：

```text
道具在道具栏中不应显示为一个单格卡片。
道具在道具栏里本身就应该是 x2 / x3 / x4 的真实形状。
以后美术插槽会按多个格子绘制，而不是一个单格 icon 落到棋盘后才展开。
```

因此 `FixtureFeedbackFix01` 当前状态：

```text
PARTIAL_PASS_BOARD_OCCUPANCY_FEEDBACK
ITEMTRAY_SHAPE_AWARE_DISPLAY_NOT_VERIFIED
```

## 2. 核心规则

必须锁定：

```text
ItemShape 在道具栏和棋盘上是一致的。
道具栏显示的就是该道具真实 occupiedCells。
拖拽时移动的是同一个多格形状视觉。
棋盘只是接收这个形状，不是在落下后才把单格卡片展开。
```

形态要求：

```text
Single1：道具栏 1 格，棋盘 1 格
Vertical2：道具栏 2 格形状，棋盘 2 格形状
Corner3：道具栏 2×2 内缺 1 格的三角形，棋盘同形状
Square4：道具栏 2×2 方块，棋盘同形状
```

## 3. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BattlePrepareComponentAdapterRuntimePlaytest01_FixtureFeedbackFix01_Assignment.md
Docs/V0.4/MobileShapePlacementInteraction01_Assignment.md
```

## 4. 允许范围

允许新增 / 修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许新增 / 修改 Extension：

```text
ItemTrayShapeExtension
ShapeAwareItemTrayFixtureView
ShapeAwareDragVisualAdapter
ShapeAwareItemTrayValidator
ShapeAwareItemTrayReportWriter
```

允许只读 Survey：

```text
成熟 BattlePrepare 道具栏
成熟 BattlePrepare 拖拽视图
ItemShapeConfig / occupiedCells / rotation
```

## 5. 必须满足

本包必须让用户在成熟 BattlePrepare 道具栏中看到：

```text
Single1 = 1 格
Vertical2 = 2 格
Corner3 = 2x2 缺 1 格
Square4 = 2x2 方块
```

并且：

```text
选中高亮覆盖完整形状
拖拽拿起完整形状
进入棋盘后的 GhostPreview 与道具栏形状一致
道具栏内的点击 / 选中区域与真实形状一致或至少不误导用户
```

## 6. 禁止范围

禁止：

```text
重画成熟道具栏
替换成熟 BattlePrepare UI
把多格道具伪装成一个单格 icon + 文本
落到棋盘后才展开形状
修改正式 RunFlow / PageState / FormationState
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改正式 V02 / V03 场景布局
覆盖用户手调 RectTransform
把测试道具写入正式配置或正式玩家存档
FeatureFlag 默认 true
devOnly = false
isEnabled = true
commit / tag / push
```

本包允许扩展成熟道具栏，但必须是：

```text
ItemTrayShapeExtension
```

不得变成：

```text
V04NewItemTray
```

## 7. 报告要求

至少输出：

```text
Docs/V0.4/Reports/ShapeAwareItemTrayFixtureReport.md
Docs/V0.4/Reports/ShapeAwareItemTrayFixtureLeakCheckReport.md
```

报告必须说明：

```text
道具栏中 x1 / x2 / x3 / x4 是否按真实形状显示
拖拽视觉是否保持真实形状
GhostPreview 是否与道具栏形状一致
是否重画道具栏
是否写正式配置 / 存档 / 主线
devOnly / isEnabled 状态
```

## 8. 用户手测清单

用户手测：

```text
1. 打开 runtime playtest。
2. 道具栏中看到 Single1 / Vertical2 / Corner3 / Square4。
3. 每个道具在道具栏里就是自己的真实形状。
4. 不是单格 icon 落到棋盘才展开。
5. 拖拽时移动的是完整形状。
6. 棋盘 GhostPreview 与道具栏形状一致。
7. 没有新画一套道具栏。
8. Console 无本包红色 Error / 黄色 Warning。
```

## 9. 通过条件

只有同时满足：

```text
道具栏形状化显示通过
棋盘占格反馈通过
devOnly 隔离成立
成熟 BattlePrepare 道具栏未被替换
```

才可进入：

```text
V0.4-MobileShapePlacementInteraction01
```
