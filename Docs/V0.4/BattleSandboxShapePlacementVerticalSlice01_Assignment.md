# V0.4-BattleSandboxShapePlacementVerticalSlice01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_SHAPE_PLACEMENT_VERTICAL_SLICE01
```

权威策略：

```text
Docs/V0.4/BUILD_SANDBOX_INTEGRATION_STRATEGY.md
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
```

本包必须遵守：

```text
不复制第二套正式战斗系统。
不强拆 V0.2 / V0.3 稳定战斗。
不继续强接旧 BattlePrepare runtime 内核。
V0.4 多格摆放先在独立沙盒场景跑通。
```

## 1. 包定位

```text
V0.4 独立沙盒场景多格摆放最小垂直切片包
```

本包正式停止“强行接 V0.2 / V0.3 BattlePrepare runtime 内核”的当前开发路线。

本包回到：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
```

在 V0.4 独立沙盒场景中先跑通新玩法自己的最小闭环。

核心原则：

```text
视觉表层可以尽量复制 V0.2 / V0.3 成熟 BattlePrepare。
交互内核必须使用 V0.4 ShapePlacement。
```

一句话：

```text
复制皮，换内核。
```

## 2. 为什么改路线

此前 `V0.4-BattlePrepareComponentAdapter01` 到 `V0.4-BattlePrepareExtensionSeam01` 一系列包，证明：

```text
V0.4 多格摆放不是旧 BattlePrepare 单格拖拽的小扩展。
强行接成熟 V0.3 BattlePrepare runtime 内核，会受到旧 Layout / Drag / Drop / Commit 权威限制。
```

失败根因：

```text
视觉层 / Layout 层 / 拖拽层 / drop 层 / anchor 状态层多套权威抢位置。
```

因此当前阶段不再继续：

```text
overlay + ignoreLayout + 延后一帧读 drop
外贴 RuntimePlaytest hook
强行把 V0.4 多格摆放塞进 V0.3 BattlePrepare runtime 链路
```

## 3. 可复用沉淀

以下内容继续复用，不算废弃：

```text
ShapePlacementSession
ShapeItemPayload
ShapeGridReceiver
ShapeAwareItemTrayGrid
MobileShapePlacementInputExtension
BuildTuningDataPanelPreview01
MechanicHintFeedbackPreview01
```

以下内容仅作为未来回迁 V0.3 的参考，不作为当前路线继续推进：

```text
BattlePrepareExtensionSeam01
BattlePrepareShapePlacementSeamAdapter
MobileShapePlacementRuntimeIntegrationFix01
```

以下路线停止：

```text
Fix03 overlay + ignoreLayout + 延后一帧读 drop
ShapeAware fixture 外贴 mature tray
RuntimePlaytest 直接 / 间接抢旧拖拽链路
```

## 4. 场景目标

目标场景：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

本包只做一个最小真实闭环：

```text
一个 x2 测试道具
在 V04 沙盒道具栏中按 2 格真实显示
点击拿起
点击旋转
拖到 V04 沙盒棋盘
显示同形状 GhostPreview
松手锁定虚影
点击虚影确认放下
取消可用
非法位置红色反馈
```

先只做 x2。

本包不扩 x3 / x4，不做 Boss，不做章节，不做复杂机制提示。

## 5. UI 表层规则

允许：

```text
只读参考 V0.2 / V0.3 BattlePrepare 的视觉表层。
在 V04 场景中复制 / 重建相同风格的 UI 皮肤。
复刻尺寸、颜色、按钮风格、间距、边框、字体、上拉节奏。
```

更推荐：

```text
复制成熟 UI 子树 / prefab / serialized layout 到 V04 沙盒场景。
保留视觉字段，再替换交互内核。
```

禁止：

```text
复用旧 V03 单格 drag/drop 内核
复用旧 LayoutGroup 抢位置逻辑作为多格摆放权威
继续使用 overlay + ignoreLayout 补丁
```

## 6. 交互内核规则

必须使用 V0.4 新内核：

```text
ShapePlacementSession
ShapeItemPayload
ShapeGridReceiver
ShapeAwareItemTrayGrid
MobileShapePlacementInputExtension
```

V04 沙盒场景中可以新增自己的：

```text
BattleSandboxShapePlacementController
BattleSandboxShapeAwareItemTrayView
BattleSandboxShapeAwareBoardGridView
BattleSandboxGhostPreviewView
BattleSandboxPlacementFeedbackView
```

但这些必须是：

```text
devOnly / sandbox preview
```

不得声明为正式主线替代。

## 7. 允许范围

允许新增 / 修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许只读参考：

```text
V0.2 / V0.3 BattlePrepare 视觉层级
V0.2 / V0.3 BattlePrepare 颜色、尺寸、动画节奏、按钮风格
```

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

禁止：

```text
把 V04 沙盒组件接入正式 1-10 / 2-10
默认开启 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

## 9. 状态机要求

必须跑通：

```text
Idle
→ HoldingItem
→ DraggingPreview
→ PreviewLocked
→ Committed
```

必须支持：

```text
Cancel
InvalidPreview
```

核心红线：

```text
拖动松手 ≠ 正式放置
点击虚影 = 正式提交
```

## 10. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattleSandboxShapePlacementVerticalSliceReport.md
Docs/V0.4/Reports/BattleSandboxShapePlacementStateReport.csv
Docs/V0.4/Reports/BattleSandboxShapePlacementLeakCheckReport.md
```

报告必须说明：

```text
是否在 V04 Preview Scene 中运行
是否只做 x2 最小闭环
是否使用 ShapePlacementSession
是否使用 ShapeAwareItemTrayGrid
是否使用 MobileShapePlacementInputExtension
是否复用 / 参考 V0.2/V0.3 视觉表层
是否使用旧 V03 drag/drop 内核
是否修改 V02/V03 正式场景
是否写正式存档 / Boss / 奖励 / 数值
```

## 11. 用户手测清单

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Play 后确认：

```text
1. V04 沙盒 UI 视觉表层接近 V0.2 / V0.3 BattlePrepare。
2. 道具栏中有一个 x2 测试道具，显示为 2 格。
3. 点击 x2 道具可拿起。
4. 再点可旋转。
5. 拖到棋盘出现同形状 GhostPreview。
6. 松手后虚影保留，不正式放下。
7. 点击虚影后正式放下。
8. 取消可用。
9. 非法位置有红色反馈。
10. Console 无本包红色 Error / 黄色 Warning。
```

## 12. 通过条件

只有 x2 最小闭环通过，才允许进入：

```text
V0.4-BattleSandboxShapePlacementVerticalSlice02
```

后续再扩：

```text
x3 / x4
手感调优
整体回归
```

不得在 x2 未通时继续扩功能。
