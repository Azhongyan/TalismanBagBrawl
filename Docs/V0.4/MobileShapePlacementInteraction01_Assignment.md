# V0.4-MobileShapePlacementInteraction01 Assignment

Guard 回执：

```text
GUARD_PASS_MOBILE_SHAPE_PLACEMENT_INTERACTION01
```

## 1. 包定位

```text
V0.4 多格道具移动端摆放输入协议包
```

本包不是普通 UI 拖拽包，不是 V0.3 战备交互包，不是重画棋盘 / 道具栏包。

本包目标是在已存在的成熟 BattlePrepare 棋盘、道具栏、拖拽与上拉手感上，新增移动端多格摆放输入协议：

```text
点选拿起
→ 点选中道具旋转
→ 拖动到棋盘生成虚影
→ 松手锁定虚影
→ 点击虚影确认放下
```

## 2. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BattlePrepareComponentAdapterRuntimePlaytest01_Assignment.md
Docs/V0.4/BattlePrepareComponentAdapterRuntimePlaytest01_FixtureFeedbackFix01_Assignment.md
```

前置必须满足：

```text
ItemShapeOccupancy 数据底座存在。
GridOccupancy / occupiedCells / rotation / validity 检测可用。
RuntimePlaytest 已能进入成熟 BattlePrepare 手感路径。
FixtureFeedbackFix 已能提供 devOnly 多格测试道具或等价夹具。
```

若前置未满足，必须返回：

```text
MOBILE_PLACEMENT_BLOCKED_BY_MISSING_FIXTURE_OR_SHAPE_DATA
```

## 3. 允许范围

允许新增 / 修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许新增 Extension / Adapter：

```text
MobileShapePlacementInputExtension
GhostPlacementPreviewExtension
PlacementConfirmExtension
ItemRotationInputExtension
PlacedItemEditExtension
MobileShapePlacementValidator
MobileShapePlacementReportWriter
```

允许只读 Survey：

```text
V03BattlePrepareInteractionController
成熟 BattlePrepare 棋盘 / 道具栏 / 拖拽 / 上拉脚本
ItemShapeOccupancy / GridOccupancy / PlacementValidator
```

## 4. 必须遵守的交互协议

完整流程：

```text
Idle
↓ 点击道具栏道具
HoldingItem
↓ 再次点击已选中道具
RotateHoldingItem
↓ 拖动到棋盘
DraggingPreview
↓ 松手
PreviewLocked
↓ 点击虚影
Placed
```

核心锁定：

```text
拖动松手 ≠ 正式放置
点击虚影 = 正式提交
```

## 5. 点击 / 拖动判定

点击：

```text
移动距离 < 15px
且按压时间 < 250ms
= 点击
```

拖动：

```text
移动距离 ≥ 15px
= 拖动
```

用途：

```text
第一次点击道具 = 拿起
第二次点击已选中道具 = 旋转
拖动 = 移动 GhostPreview
点击虚影 = 确认放下
```

## 6. PreviewLocked 状态

`PreviewLocked` 是本协议核心状态。

松手后必须：

```text
虚影保留
不写正式棋盘
不刷新正式道具数量
不刷新正式供能 / 羁绊 / 存档
```

允许操作：

```text
点击虚影 → 正式放下
拖动虚影 → 调整位置
点击选中道具 → 顺时针旋转，虚影实时更新
点击取消 → 取消拿起
拖回道具栏 → 取消拿起
```

V0.4 最少必须提供：

```text
取消按钮
```

## 7. GhostPreview 规则

拖动到棋盘区域后，必须根据当前旋转方向和锚点格生成虚影。

合法位置：

```text
绿色或金色描边
提示：再次点击放下
```

非法位置：

```text
红色描边
提示：当前位置无法放置
```

非法原因包括：

```text
超出棋盘
压到已有道具
压到锁定格
不满足特殊摆放规则
```

拖动时道具视觉建议显示在手指上方：

```text
fingerPosition + upwardOffset 40~60px
```

## 8. 形态旋转规则

必须支持：

```text
x1：1 格，不旋转
x2：2 格，横 / 竖
x3 三角：2x2 内占 3 格，缺 1 格，4 方向顺时针
x4 方块：4 格，不旋转
```

不可旋转时给轻提示：

```text
该形态无需旋转
```

禁止在 UI 拖拽脚本中硬写形状逻辑。

形状必须来自：

```text
ShapeDefinition / ItemShapeConfig
occupiedCells calculation
rotation calculation
placement validity check
```

## 9. 正式提交规则

只有点击虚影确认后，才允许提交：

```text
occupiedCells 写入
碰撞最终确认
供能刷新
羁绊刷新
道具栏数量刷新
阵盘状态保存（若当前 devOnly 环境允许）
```

预览阶段只允许：

```text
GhostPreview
合法 / 非法提示
旋转预览
位置预览
```

在 V0.4 BuildSandbox 阶段，提交默认仍应保持：

```text
devOnly / sandbox state
```

不得写正式 SaveData / MainTrialProgressData / PlayerPrefs。

## 10. 已放置道具编辑

如本包范围允许，建议复用同一逻辑：

```text
点击棋盘上已放置道具
→ 道具进入编辑拿起态
→ 原位置释放为虚影预览
```

然后：

```text
点击该道具 → 旋转
拖动 → 调整位置
点击虚影 → 确认放下
点击收回 → 回到道具栏
```

若本包来不及做已放置编辑，必须在报告中标注：

```text
PlacedItemEdit = DEFERRED
```

不得半实现。

## 11. 组件复用规则

必须遵守：

```text
Base Component：成熟 BattlePrepare 棋盘 / 道具栏 / 拖拽 / 上拉动画
Extension Layer：MobileShapePlacementInput / GhostPreview / Confirm / Rotation
Adapter Layer：BuildSandbox shape data → mature component view model
```

禁止：

```text
新建 V04Board
新建 V04ItemTray
新建 V04DragSystem
新建 V04ItemInfoPopup
重画棋盘 / 道具栏 / 上拉动画
替换成熟 BattlePrepare 手感
```

## 12. 红线禁止

禁止修改：

```text
正式 RunFlow / PageState / FormationState
正式 SaveData / PlayerPrefs / MainTrialProgressData
正式 Boss / 奖励 / 掉落 / 数值
正式 V02 / V03 场景布局
V02FormationGridFrame 核心行为
DamageText
用户手调 RectTransform
```

禁止：

```text
FeatureFlag 默认 true
devOnly = false
isEnabled = true
commit / tag / push
```

## 13. 报告要求

至少输出：

```text
Docs/V0.4/Reports/MobileShapePlacementInteractionReport.md
Docs/V0.4/Reports/MobileShapePlacementStateMachineReport.csv
Docs/V0.4/Reports/MobileShapePlacementLeakCheckReport.md
```

报告必须说明：

```text
点击 / 拖动阈值
状态机覆盖情况
PreviewLocked 是否实现
拖动松手是否不提交
点击虚影是否提交
取消是否可用
x1 / x2 / x3 / x4 是否支持
形状逻辑是否数据化
是否复用成熟 BattlePrepare 组件
是否重画 UI
是否写正式存档 / 主线 / Boss / 奖励 / 数值
```

## 14. 用户手测清单

用户手测：

```text
1. 点击道具栏道具，进入拿起态。
2. 再点已选中道具，能旋转。
3. 拖动到棋盘，出现虚影。
4. 松手后虚影保留，没有正式放下。
5. 点击虚影后才正式放下。
6. 非法位置显示红色 / 中文提示。
7. 合法位置显示绿色或金色 / 中文提示。
8. 取消按钮可取消拿起。
9. x1 / x2 / x3 / x4 行为符合规则。
10. 没有出现新画的棋盘 / 道具栏 / 上拉动画。
11. Console 无本包红色 Error / 黄色 Warning。
```

## 15. 下一步关系

本包通过后，才能继续：

```text
V0.4-MechanicHintFeedbackPreview01
```

如果本包失败，不得跳过进入机制反馈；必须先回收移动端摆放输入协议。
