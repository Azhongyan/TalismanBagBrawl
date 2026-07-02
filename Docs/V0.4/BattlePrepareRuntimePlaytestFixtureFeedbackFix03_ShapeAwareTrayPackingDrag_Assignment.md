# V0.4-BattlePrepareRuntimePlaytestFixtureFeedbackFix03-ShapeAwareTrayPackingDrag Assignment

Guard 回执：

```text
GUARD_PASS_RUNTIME_PLAYTEST_FIXTURE_FEEDBACK_FIX03_SHAPE_AWARE_TRAY_PACKING_DRAG
```

## 1. 包定位

```text
Shape-aware ItemTray 排布与拖拽持久修复包
```

本包修复 `Fix02-ShapeAwareItemTray` 的用户手测失败。

Fix02 已做到：

```text
道具栏中的 devOnly 测试道具不再都是单格 icon。
x2 / x3 / x4 已能显示为多格 footprint。
```

但用户手测不通过：

```text
1. 多格道具在道具栏中的位置不对。
2. x2 / x3 没有按类似俄罗斯方块整理规律在道具栏内摆好。
3. 多格道具超出了道具栏范围。
4. 道具无法在道具栏中移动位置。
5. 道具拖到棋盘后也无法固定。
6. 松手或操作后会回到一开始的错误位置。
```

因此 Fix02 状态应标记为：

```text
SHAPE_VISIBLE_BUT_PACKING_AND_DRAG_FAILED
```

## 2. 核心修复目标

本包只修两个问题：

```text
Shape-aware tray packing
Shape-aware drag state persistence
```

也就是：

```text
多格道具在道具栏中必须按真实 occupiedCells 排布在合法格子里。
多格道具不能超出道具栏。
拖拽移动后，视觉位置和逻辑位置必须更新，而不是回到错误初始位置。
```

## 3. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BattlePrepareRuntimePlaytestFixtureFeedbackFix02_ShapeAwareItemTray_Assignment.md
Docs/V0.4/MobileShapePlacementInteraction01_Assignment.md
```

## 4. 允许范围

允许新增 / 修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

建议修改 / 新增：

```text
ShapeAwareItemTrayFixtureView
BattlePrepareComponentAdapterRuntimePlaytest
ShapeAwareItemTrayPackingValidator
ShapeAwareItemTrayPackingReportWriter
```

允许新增内部纯逻辑：

```text
ShapeAwareTrayPackingMap
ShapeAwareTrayPlacementState
ShapeAwareTrayDragState
```

## 5. Shape-aware tray packing 规则

道具栏应被视为一个可摆放网格，不是普通 icon 列表。

必须建立道具栏内的 devOnly 逻辑占格：

```text
tray occupied cells
tray anchor cell
shape occupiedOffsets
tray bounds
overlap check
```

初始排布必须满足：

```text
Single1 / Vertical2 / Corner3 / Square4 全部在道具栏边界内。
道具之间不重叠。
多格道具的 footprint 与道具栏格子对齐。
排布尽量紧凑，类似俄罗斯方块整理规律。
```

禁止：

```text
用一个中心点随便覆盖多格视觉
用 ignoreLayout 后不参与道具栏占格计算
让多格视觉超出道具栏可视区域
只靠 RectTransform 绝对位置硬摆
```

## 6. 道具栏内拖拽规则

本包必须支持 devOnly 测试道具在道具栏内移动：

```text
按住多格道具
→ 拖到道具栏内另一个合法位置
→ 显示 tray ghost / preview
→ 松手或确认后更新 tray anchor cell
→ 道具停在新位置
```

如本包尚未进入完整 MobileShapePlacement 的二次确认协议，至少必须做到：

```text
拖拽后不会回到错误初始位置。
非法位置回退到上一个合法位置。
合法位置更新当前 devOnly tray state。
```

## 7. 拖到棋盘的最小要求

本包不是完整 `MobileShapePlacementInteraction01`，但为了验证 Fix02 缺口，必须做到：

```text
从道具栏拖到棋盘时，拖动的是完整多格视觉。
棋盘 ghost 与道具栏 footprint 一致。
松手后不得回到错误初始位置导致无法继续测试。
```

如果正式“点击虚影确认放下”仍留给 `MobileShapePlacementInteraction01`，本包必须明确标注：

```text
BoardCommit = DEFERRED_TO_MOBILE_SHAPE_PLACEMENT_INTERACTION01
```

但不能让用户完全无法把多格道具拖到棋盘预览位置。

## 8. 禁止范围

禁止：

```text
重画成熟道具栏
替换成熟 BattlePrepare UI
新增 V04NewItemTray
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

## 9. 报告要求

至少输出：

```text
Docs/V0.4/Reports/ShapeAwareTrayPackingDragReport.md
Docs/V0.4/Reports/ShapeAwareTrayPackingMapReport.csv
Docs/V0.4/Reports/ShapeAwareTrayPackingDragLeakCheckReport.md
```

报告必须说明：

```text
道具栏格子总数
x1 / x2 / x3 / x4 初始 anchor cell
每个形状的 occupiedCells
是否越界
是否重叠
是否能在道具栏内移动并持久
是否能拖到棋盘预览而不回到错误初始位置
BoardCommit 是否已实现或延期到 MobileShapePlacementInteraction01
是否重画道具栏
是否写正式配置 / 存档 / 主线
```

## 10. 用户手测清单

用户手测：

```text
1. 打开 runtime playtest。
2. 道具栏中 Single1 / Vertical2 / Corner3 / Square4 均在道具栏内部。
3. x2 / x3 / x4 按格子规律摆放，不超出托盘。
4. 道具之间不重叠。
5. 拖动多格道具时，移动的是完整 footprint。
6. 在道具栏内换位置后不会回到错误初始位置。
7. 拖到棋盘时，棋盘 ghost 与道具栏形状一致。
8. 松手后不会直接丢失或回到错误初始位置。
9. 没有新画一套道具栏。
10. Console 无本包红色 Error / 黄色 Warning。
```

## 11. 通过条件

只有同时满足：

```text
道具栏形状化显示通过
道具栏内 packing 合法
拖拽位置状态可持续
棋盘 ghost 可继续测试
devOnly 隔离成立
成熟 BattlePrepare 道具栏未被替换
```

才可继续：

```text
V0.4-MobileShapePlacementInteraction01
```
