# V0.4-ShapePlacementSession01 Tech Survey Assignment

Guard 回执：

```text
GUARD_REJECT_SHAPEAWARE_TRAY_OVERLAY_FIX03_CONTINUE
GUARD_ACCEPT_SHAPE_PLACEMENT_SESSION_REDESIGN
GUARD_PASS_SHAPE_PLACEMENT_SESSION01_TECH_SURVEY
```

## 1. 裁决结论

当前 `Fix03-ShapeAwareTrayPackingDrag` 路线停止。

原因不是手测误差，也不是某个 RectTransform 偏移问题。

根因是：

```text
视觉层
Layout 层
拖拽层
drop 层
anchor 状态层
```

多套权威同时存在。

当前方案：

```text
overlay 多格视觉
+ ignoreLayout
+ 外部 anchor state
+ 延后一帧读取 V03 drop 结果
```

会继续导致：

```text
位置乱跳
不在道具栏格子
不在棋盘格子
放不进去
回到错误初始位置
```

因此：

```text
不建议继续修 Fix03。
不得继续在 overlay + ignoreLayout + 延后一帧读 drop 上叠补丁。
```

## 2. 本包定位

```text
V0.4 多格摆放统一状态源技术 Survey 包
```

本包只做技术调查和新协议设计，不开发复杂 UI，不继续修 overlay。

目标：

```text
确认是否可以建立 ShapePlacementSession + ShapeItemPayload + ShapeGridReceiver 的统一 placement 协议。
调查现有 V03 拖拽、道具栏、棋盘 drop 入口能否通过 Adapter 接成 ShapeGridReceiver。
输出后续重设计拆包建议。
```

## 3. 正确新方向

建立唯一状态源：

```text
ShapePlacementSession
```

它负责：

```text
selectedItemId
shapeId
rotation
sourceContainer
currentState
trayAnchorCell
boardAnchorCell
occupiedCells
lastLegalTrayAnchor
lastLegalBoardAnchor
```

拖拽传递：

```text
ShapeItemPayload
```

包含：

```text
itemId
shapeId
rotation
occupiedOffsets
source
```

道具栏和棋盘都实现统一协议：

```text
ShapeGridReceiver
```

接口能力：

```text
ScreenPointToCell()
CanPlace(payload, anchorCell)
Preview(payload, anchorCell)
Commit(payload, anchorCell)
Cancel()
```

视觉层只负责：

```text
Render(session)
```

不得让 RectTransform / LayoutGroup / Drag / Drop / AnchorState 各自抢权威。

## 4. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BattlePrepareRuntimePlaytestFixtureFeedbackFix03_ShapeAwareTrayPackingDrag_Assignment.md
Docs/V0.4/MobileShapePlacementInteraction01_Assignment.md
```

并读取 Fix03 相关报告和用户失败反馈。

## 5. 允许范围

允许只读 Survey：

```text
BattlePrepareComponentAdapterRuntimePlaytest.cs
ShapeAwareItemTrayFixtureView.cs
V03BattlePrepareInteractionController.cs
成熟 BattlePrepare 道具栏 / 棋盘 / 拖拽 / drop 相关脚本
ItemShapeConfig / GridOccupancyMap / ItemShapePlacementValidator
```

允许新增 / 修改：

```text
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

如确实需要新增纯接口草案代码，必须限于：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
```

但本包默认建议：

```text
报告优先，不写运行时逻辑。
```

## 6. 禁止范围

禁止：

```text
继续修 overlay / ignoreLayout / 延后一帧读 drop 的 Fix03 路线
改成熟 V03 BattlePrepare UI 布局
重画道具栏
重画棋盘
重写拖拽手感
修改 RunFlow / PageState / FormationState
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改正式 V02 / V03 场景布局
覆盖用户手调 RectTransform
commit / tag / push
```

## 7. Survey 必须回答的问题

必须输出：

```text
1. 现有 V03 道具栏的布局权威在哪里？
2. 现有 V03 拖拽系统的 payload 是什么？
3. 现有 drop target 如何判断道具栏 / 棋盘？
4. 是否已有可接入 ScreenPointToCell 的格子坐标转换？
5. 现有棋盘能否只读转换为 ShapeGridReceiver？
6. 现有道具栏能否只读转换为 ShapeGridReceiver？
7. ShapePlacementSession 应该放在 BuildSandbox devOnly，还是成熟 BattlePrepare extension seam？
8. 哪些点必须新增 adapter seam？
9. 哪些点不能碰，否则会破坏 V0.2 / V0.3 手感？
10. 后续是否应拆为 ShapePlacementSession01 / ShapeAwareItemTrayGrid01 / MobileShapePlacementInteraction01？
```

## 8. 输出报告

至少输出：

```text
Docs/V0.4/Reports/ShapePlacementSessionTechSurveyReport.md
Docs/V0.4/Reports/ShapeGridReceiverSurveyReport.csv
Docs/V0.4/Reports/ShapePlacementRedesignRiskReport.md
```

报告必须包含：

```text
当前 Fix03 失败根因
新统一状态源方案
ShapePlacementSession 字段草案
ShapeItemPayload 字段草案
ShapeGridReceiver 接口草案
现有 V03 接入点清单
禁止继续补丁路线清单
后续拆包建议
```

## 9. 后续推荐拆包

若 Survey 通过，后续建议：

```text
V0.4-ShapePlacementSession01
V0.4-ShapeAwareItemTrayGrid01
V0.4-MobileShapePlacementInteraction01
```

其中：

```text
ShapePlacementSession01：状态源 / payload / receiver 接口 / 纯逻辑。
ShapeAwareItemTrayGrid01：道具栏原生 shape-aware packing。
MobileShapePlacementInteraction01：点选 / 旋转 / 拖动 / 虚影 / 确认 / 取消。
```

不得跳过 Survey 直接继续 Fix03。
