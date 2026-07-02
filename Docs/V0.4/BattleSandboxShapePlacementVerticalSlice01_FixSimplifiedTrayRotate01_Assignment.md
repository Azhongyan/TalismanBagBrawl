# V0.4-BattleSandboxShapePlacementVerticalSlice01-FixSimplifiedTrayRotate01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_VERTICALSLICE01_FIX_SIMPLIFIED_TRAY_ROTATE
```

## 1. 包定位

```text
V0.4 独立沙盒 x2 垂直切片简化交互修复包
```

这是 `V0.4-BattleSandboxShapePlacementVerticalSlice01` 的第二次手测失败回流。

本包目标不是继续补复杂交互，而是主动砍需求，先跑通最小稳定版本。

## 2. 用户最新裁剪

用户最新结论：

```text
测试还是不通过
先把复杂的交互逻辑需求砍掉
还是用以前的逻辑
旋转不允许在任何时候发生
只在道具栏点击按钮时候可以旋转
旋转好了之后再往棋盘里放置
如果两个物体之间有重叠旋转不成功
先一步一步来
```

## 3. Guard 裁定

本包必须删除 / 禁用复杂输入链路：

```text
不做点击拿起
不做点击道具旋转
不做持有态旋转
不做虚影态旋转
不做棋盘上旋转
不做松手锁 Ghost 后再旋转
不做复杂 PreviewLocked 编辑态
```

先采用更直接、可测试的逻辑：

```text
道具栏中设置方向
拖到棋盘
合法则放置
非法则返回道具栏
```

## 4. 简化交互锁定

当前 x2 最小版本只允许：

```text
单击道具主体 = 打开 / 刷新物品信息
点击道具栏内 Rotate 按钮 = 在道具栏中旋转该道具
拖动道具 = 拖到棋盘
松手 = 尝试放置
合法位置 = 直接放置成功
非法位置 = 不放置，返回道具栏原位置
取消按钮 = 清空当前拖动 / 预览状态
```

旋转限制：

```text
只有道具仍在道具栏中时，Rotate 按钮可用。
一旦开始拖动，旋转禁用。
进入棋盘预览时，旋转禁用。
已经放到棋盘上后，旋转禁用。
```

旋转合法性：

```text
道具栏内点击 Rotate 按钮时，必须先检查旋转后的占格。
如果旋转后越界，旋转失败，保持原方向。
如果旋转后与其他道具重叠，旋转失败，保持原方向。
旋转失败时给轻提示或红色反馈。
```

## 5. 道具栏显示要求

`护阵木牌 / Vertical2` 必须在道具栏里真实显示为 2 格。

要求：

```text
默认竖向 2 格。
点击 Rotate 按钮后横向 2 格。
旋转只改变道具栏内方向。
拖到棋盘时保持当前方向。
```

禁止：

```text
显示成单格 icon
拖到棋盘后才变 2 格
用漂浮 overlay 假装 2 格
超出道具栏限制区域
```

## 6. 棋盘放置要求

拖动到棋盘时可以显示 GhostPreview，但本包不做二次确认。

规则：

```text
拖动中显示 GhostPreview。
松手时如果合法，直接提交放置。
松手时如果越界 / 重叠 / 非法，返回道具栏原位置。
```

本包暂不要求：

```text
松手锁定虚影
点击虚影确认
拖动虚影调整
虚影态旋转
棋盘上编辑已放置道具
```

## 7. 允许修改范围

允许修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许使用：

```text
ShapePlacementSession
ShapeItemPayload
ShapeGridReceiver
ShapeAwareItemTrayGrid
MobileShapePlacementInputExtension
```

但如果 `MobileShapePlacementInputExtension` 当前强绑定复杂 PreviewLocked / Ghost click confirm 逻辑，本包允许绕开或收窄它，只保留简化拖放必要部分。

## 8. 禁止范围

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

禁止扩展：

```text
x3
x4
Boss
章节
复杂机制提示
正式战斗接入
```

禁止：

```text
默认开启 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

## 9. 手测清单

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Play 后确认：

```text
1. 道具栏中 `护阵木牌 / Vertical2` 默认显示为竖向真实 2 格。
2. 单击道具主体只打开 / 刷新物品信息。
3. 道具栏内有明确 Rotate 按钮。
4. 点击 Rotate 按钮后，Vertical2 在道具栏中变为横向 2 格。
5. 再点 Rotate 可回到竖向 2 格。
6. 如果旋转后越界或重叠，旋转失败，方向保持不变。
7. 拖动道具到棋盘，方向保持拖动前在道具栏中设置好的方向。
8. 合法位置松手后直接放置成功。
9. 非法位置松手后不放置，返回道具栏原位置。
10. 拖动中、棋盘上、预览中都不能旋转。
11. Console 无本包红色 Error / 黄色 Warning。
```

## 10. 报告要求

至少输出或更新：

```text
Docs/V0.4/Reports/BattleSandboxShapePlacementVerticalSliceReport.md
Docs/V0.4/Reports/BattleSandboxShapePlacementStateReport.csv
Docs/V0.4/Reports/BattleSandboxShapePlacementLeakCheckReport.md
```

报告必须明确：

```text
Tray Rotate only.
No rotation while dragging.
No rotation on board.
No Ghost click confirm requirement.
Release commits if valid.
Release returns to tray if invalid.
Vertical2 is rendered as 2 cells in tray.
Rotation fails on tray overlap / out of bounds.
```

## 11. 通过条件

只有本包通过用户手测，才允许进入后续：

```text
V0.4-BattleSandboxShapePlacementVerticalSlice02
```

后续再逐步考虑：

```text
x3 / x4
更复杂的手机端确认交互
棋盘上编辑已放置道具
手感调优
整体回归
```
