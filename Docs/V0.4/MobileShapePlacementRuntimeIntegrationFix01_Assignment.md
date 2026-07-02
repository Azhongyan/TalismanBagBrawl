# V0.4-MobileShapePlacementRuntimeIntegrationFix01 Assignment

Guard 回执：

```text
GUARD_PASS_MOBILE_SHAPE_PLACEMENT_RUNTIME_INTEGRATION_FIX01
```

## 1. 包定位

```text
V0.4 多格摆放运行时接入修复包
```

本包不是新玩法包，不是 UI 重画包，不是机制提示包。

本包目标：

```text
把已经存在的 ShapePlacementSession / ShapeAwareItemTrayGrid / MobileShapePlacementInputExtension
真正接入 BattlePrepareComponentAdapterRuntimePlaytest。
```

让用户能在 Unity Play 中完整跑通：

```text
点选拿起
点选旋转
拖动 GhostPreview
松手锁定
点击虚影确认
取消
```

## 2. 当前前置状态

已通过 / 已接受：

```text
V0.4-ShapePlacementSession01
V0.4-ShapeAwareItemTrayGrid01
V0.4-MobileShapePlacementInteraction01
V0.4-MechanicHintFeedbackPreview01
```

已停止路线：

```text
Fix03 overlay + ignoreLayout + 延后一帧读 drop
```

当前问题：

```text
协议、grid、input extension 已存在。
但 RuntimePlaytest 中实际完整摆放体验还没有稳定串起来。
```

## 3. 上游必须读取

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/ShapePlacementSession01_TechSurvey_Assignment.md
Docs/V0.4/MobileShapePlacementInteraction01_Assignment.md
Docs/V0.4/BattlePrepareRuntimePlaytestFixtureFeedbackFix03_ShapeAwareTrayPackingDrag_Assignment.md
```

并参考最新报告：

```text
ShapePlacementSessionProtocolReport.md
ShapeAwareItemTrayGridReport.md
MobileShapePlacementInteractionReport.md
```

## 4. 允许范围

允许新增 / 修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

优先修改 / 接入：

```text
BattlePrepareComponentAdapterRuntimePlaytest.cs
ShapePlacementSession.cs
ShapeAwareItemTrayGrid.cs
MobileShapePlacementInputExtension.cs
```

允许新增：

```text
MobileShapePlacementRuntimeIntegration
MobileShapePlacementRuntimeIntegrationValidator
MobileShapePlacementRuntimeIntegrationReportWriter
```

## 5. 必须完成

必须把以下对象串成同一条运行时链路：

```text
ShapePlacementSession
ShapeItemPayload
ShapeAwareItemTrayGrid
MobileShapePlacementInputExtension
BattlePrepareComponentAdapterRuntimePlaytest
成熟 BattlePrepare 道具栏 / 棋盘 / 拖拽 / 上拉容器
```

用户在 runtime playtest 中必须能测试：

```text
1. 点击道具栏多格道具 → 进入 HoldingItem。
2. 再点击已选中道具 → 旋转。
3. 拖动到棋盘 → 显示 GhostPreview。
4. 松手 → 进入 PreviewLocked，不正式提交。
5. 点击虚影 → Commit。
6. 点击取消 → Cancel。
7. 非法位置 → 红色反馈，不提交。
```

## 6. 必须使用统一状态源

运行时链路必须以以下内容为权威：

```text
ShapePlacementSession
ShapeItemPayload
ShapeGridReceiver
```

禁止恢复：

```text
overlay + ignoreLayout + 延后一帧读 drop
RectTransform 视觉位置作为逻辑权威
LayoutGroup 下一帧补救
多个 anchor state 同时抢权威
```

## 7. 禁止范围

禁止：

```text
重画棋盘
重画道具栏
重写上拉动画
替换成熟 BattlePrepare UI
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

## 8. 报告要求

至少输出：

```text
Docs/V0.4/Reports/MobileShapePlacementRuntimeIntegrationReport.md
Docs/V0.4/Reports/MobileShapePlacementRuntimeIntegrationStateReport.csv
Docs/V0.4/Reports/MobileShapePlacementRuntimeIntegrationLeakCheckReport.md
```

报告必须说明：

```text
是否真实接入 RuntimePlaytest
是否使用 ShapePlacementSession 作为唯一状态源
是否使用 ShapeAwareItemTrayGrid
是否能进入 HoldingItem / PreviewLocked / Committed / Cancelled
是否恢复了旧 overlay 方案
是否重画 UI
是否触碰正式主线 / 存档 / Boss / 奖励 / 数值
用户手测步骤
```

## 9. 用户手测清单

用户在 Unity 里运行：

```text
Tools / Talisman Bag / V0.4 / BuildSandbox / BattlePrepareComponentAdapterRuntimePlaytest01 / [Manual Only] Open Runtime Playtest
```

手测：

```text
1. 点击 x2 / x3 / x4 道具，能拿起。
2. 再点选中道具，能旋转。
3. 拖到棋盘，出现同形状 GhostPreview。
4. 松手后虚影保留，不正式放下。
5. 点击虚影后才正式放下。
6. 非法位置显示红色反馈。
7. 取消按钮能取消。
8. 道具栏 / 棋盘 / 上拉动画仍是成熟 BattlePrepare 手感。
9. Console 无本包红色 Error / 黄色 Warning。
```

## 10. 通过条件

只有用户手测确认以上链路通过，才可标记：

```text
USER_ACCEPTED / RUNTIME_SHAPE_PLACEMENT_INTEGRATION_VERIFIED
```

否则不得继续进入：

```text
ShapePlacementHandfeelTuning01
ShapePlacementRegression01
DevChapterBalanceRun01
```
