# V0.4-BattleSandboxShapePlacementVerticalSlice01-FixInputConflict01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_VERTICALSLICE01_FIX_INPUT_CONFLICT
```

## 1. 包定位

```text
V0.4 独立沙盒 x2 垂直切片输入冲突修复包
```

本包是 `V0.4-BattleSandboxShapePlacementVerticalSlice01` 的手测失败回流，不是新功能扩展包。

目标只修两个问题：

```text
1. 单次点击道具打开物品信息，与点击已选中道具旋转发生冲突。
2. x2 测试道具没有在道具栏中真实显示为 2 格。
```

本包不扩：

```text
x3
x4
Boss
章节
复杂机制提示
正式战斗接入
```

## 2. 用户手测失败原文

```text
我知道为什么之前也做不好现在也做不好的原因了 现在交互是有冲突重叠的 单次点击道具现在是打开物品信息窗口 和拖动交互不冲突。但和现在点击再点击旋转的交互冲突了。另外现在也没有在道具栏中显示两格
```

## 3. Guard 裁定

当前问题不是 Unity 小偏移，也不是单个 RectTransform 问题。

这是输入语义冲突：

```text
单击道具 = 打开物品信息
再次点击道具 = 旋转
```

两者占用了同一输入入口。

因此本包必须先重新定义 V04 沙盒 x2 垂直切片的输入语义。

## 4. 新输入语义锁定

为避免和物品信息窗口冲突，当前 x2 垂直切片采用：

```text
单击道具 = 打开 / 刷新物品信息
拖动道具 = 拿起并进入摆放流程
旋转 = 使用持有态 / 虚影态的独立 Rotate 按钮
松手 = 只锁定 GhostPreview，不正式放下
点击 GhostPreview = 正式提交
取消按钮 = 取消当前拿起 / 虚影状态
```

说明：

```text
单击道具不再承担旋转。
旋转不再依赖再次点击道具。
道具信息弹窗保留为单击行为。
拖动行为仍可进入摆放，不与单击信息冲突。
```

## 5. 道具栏 x2 显示要求

`护阵木牌 / Vertical2` 必须在 V04 沙盒道具栏中真实显示为 2 格。

要求：

```text
不是一个单格 icon。
不是拖到棋盘后才变成 2 格。
不是 overlay 漂浮在托盘外。
必须在托盘限制区域内占用 2 个连续格。
不可超出道具栏可视区域。
```

当前只要求 x2。

允许方向：

```text
Vertical2 默认竖向 2 格。
点击 / 拖动进入持有态后，可通过 Rotate 按钮变成横向 2 格。
旋转后 GhostPreview 与道具视觉都应同步。
```

## 6. 允许修改范围

允许修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许继续使用：

```text
ShapePlacementSession
ShapeItemPayload
ShapeGridReceiver
ShapeAwareItemTrayGrid
MobileShapePlacementInputExtension
```

允许更新报告 / validator。

## 7. 禁止范围

禁止修改：

```text
Scene_TalismanBag_V02_FormationCounter
Scene_TalismanBag_V03_MainHome
Scene_TalismanBag_V03_TalismanUpgrade
正式 RunFlow / PageState / FormationState
正式 SaveData / PlayerPrefs / MainTrialProgressData
正式 Boss / 奖励 / 掉落 / 数值
正式 V02 / V03 BattlePrepare runtime 行为
正式 V02 / V03 用户手调 RectTransform
```

禁止恢复旧失败路线：

```text
overlay + ignoreLayout + 延后一帧读 drop
外贴 mature tray
强行抢旧 V03 BattlePrepare drag/drop 链路
```

禁止：

```text
把 V04 沙盒组件接入正式 1-10 / 2-10
默认开启 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

## 8. 修复后手测清单

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Play 后确认：

```text
1. 道具栏中 `护阵木牌 / Vertical2` 显示为真实 2 格。
2. 单击道具只打开 / 刷新物品信息，不触发旋转。
3. 拖动道具可以拿起并进入摆放流程。
4. 持有态 / 虚影态有明确 Rotate 按钮。
5. 点击 Rotate 按钮可以旋转 x2。
6. 拖到棋盘出现同形状 GhostPreview。
7. 松手后只锁定虚影，不正式放下。
8. 点击 GhostPreview 后才正式提交。
9. 取消按钮可清空拿起 / 虚影状态。
10. Console 无本包红色 Error / 黄色 Warning。
```

## 9. 报告要求

至少输出或更新：

```text
Docs/V0.4/Reports/BattleSandboxShapePlacementVerticalSliceReport.md
Docs/V0.4/Reports/BattleSandboxShapePlacementStateReport.csv
Docs/V0.4/Reports/BattleSandboxShapePlacementLeakCheckReport.md
```

报告必须明确：

```text
Click opens item info only.
Drag starts placement.
Rotate uses explicit Rotate button.
Vertical2 is rendered as 2 cells in tray.
Release does not commit.
Ghost click commits.
```

## 10. 通过条件

只有本包通过用户手测，才允许进入：

```text
V0.4-BattleSandboxShapePlacementVerticalSlice02
```

后续才扩：

```text
x3 / x4
手感调优
整体回归
```
