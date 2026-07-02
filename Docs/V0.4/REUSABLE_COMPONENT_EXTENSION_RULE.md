# V0.4 Reusable Component Extension Rule

维护方：Codex Guard / 记忆治理窗口

状态：

```text
GUARD_SYNC_EXTEND_EXISTING_COMPONENT_FIRST_ACCEPTED
GUARD_FORBID_REBUILD_OVERLAPPING_COMPONENTS
```

## 1. 核心结论

本规则不是禁止改 UI、禁止改交互、禁止开发新玩法。

本规则锁定的是：

```text
已有成熟组件不是不能改，而是应该作为底座扩展。
新功能优先做 Extension Layer / Adapter，不允许重建一套职责重叠的大组件。
```

也就是：

```text
成熟组件优先扩展，不重建。
```

## 2. 为什么要写这条

V0.4 BuildSandbox 引入了新玩法：

```text
多格占用
2 格 / 3 格 / 4 格形状
旋转
越界 / 重叠校验
构筑贡献
词条 / 羁绊 / Readiness / DropBias
```

这些是新功能，可以开发。

但新功能不应该导致：

```text
新建一套 V0.4 棋盘
新建一套 V0.4 道具栏
新建一套 V0.4 拖拽系统
新建一套 V0.4 道具信息弹窗
新建一套 V0.4 战斗反馈 UI
```

如果旧组件已经承担同类职责，并且交互手感已经由用户长期调好，正确方向是：

```text
在旧组件上加扩展层。
用 Adapter 接新数据。
必要时新增按钮、状态层、显示字段或校验逻辑。
```

## 3. 三层结构

后续 UI / 交互 / 系统接入默认分三层：

```text
Base Component
Extension Layer
Adapter Layer
```

### 3.1 Base Component

已经存在、已经调过手感或信息结构成熟的组件，例如：

```text
棋盘组件
道具栏组件
道具栏分类标签
道具栏滚动 / 滑动 / 拖动
战斗整备上拉动画
道具信息弹窗
BossInfoPanel
战斗提示 / 伤害反馈 / Tooltip
数据面板 Tab / Foldout / Validate / Export 形态
```

Base Component 是底座，不是不能动。

它可以为了新玩法被扩展，但不得被无理由重建。

### 3.2 Extension Layer

新玩法加在成熟组件上的扩展层，例如：

```text
BoardOccupancyExtension
ItemTrayShapeExtension
DragRotationPlacementExtension
ItemInfoBuildFieldsExtension
BattleFeedbackMechanicHintExtension
BossInfoProblemHintExtension
DataPanelBuildTuningExtension
```

Extension Layer 可以新增：

```text
按钮
状态层
占格预览
旋转交互
合法性校验
新字段显示
新分类
新数据区
失败反馈
调参字段
```

### 3.3 Adapter Layer

Adapter Layer 负责把新数据转成旧组件能吃的格式，例如：

```text
BuildSandbox item data → ItemTray item view model
Shape / occupied cells → Board occupancy view model
Affix / Synergy → ItemInfo extra fields
Readiness / DropBias → Developer data panel fields
BossProblem hint → BossInfo / BattleHint masked text
```

Adapter 不应该反向修改正式 UI，不应该绕过成熟组件直接写一套新 UI。

## 4. 判断顺序

任何新 UI / 新交互 / 新玩法任务开始前，开发窗口必须先判断：

```text
1. 这个需求属于哪个职责？
2. 项目里是否已有成熟组件承担同类职责？
3. 能否在成熟组件上新增 Extension Layer？
4. 能否通过 Adapter 接新数据？
5. 如果必须新建组件，为什么旧组件不能扩展？
```

只有当答案证明旧组件确实无法承担时，才允许新建组件。

## 5. V0.4 多格占用示例

错误方向：

```text
做 V04Board
做 V04ItemTray
做 V04DragSystem
做 V04ItemInfoPopup
```

正确方向：

```text
复用棋盘组件
→ 加 BoardOccupancyExtension

复用道具栏组件
→ 加 ItemTrayShapeExtension

复用拖拽手感
→ 加 DragRotationPlacementExtension

复用道具信息弹窗
→ 加 ItemInfoBuildFieldsExtension

复用战斗反馈 / Tooltip
→ 加 BattleFeedbackMechanicHintExtension
```

## 6. 允许改什么

允许在成熟组件上新增：

```text
按钮
子节点
插槽
显示字段
状态层
数据绑定字段
ViewModel
Adapter
校验逻辑
Preview overlay
Tooltip / Feedback 文案
```

允许为了新玩法调整组件：

```text
显示更多字段
支持新的 item shape
支持旋转操作
支持多格占用预览
支持新分类
支持新反馈状态
```

前提是：

```text
不破坏原组件已有用途。
不覆盖用户手调布局。
不改变正式主流程。
不绕过 Guard 红线。
```

## 7. 禁止什么

禁止：

```text
因为新增一项功能就重画整套棋盘
因为多格道具就重做整个道具栏
因为 V0.4 要验证就另写一套拖拽手感
已有成熟弹窗可扩展时新造一个相似弹窗
用 temporary preview 替代成熟组件
两个职责高度重叠的组件长期并存
```

## 8. 新建组件时必须说明

若开发窗口认为必须新建组件，必须先输出：

```text
旧组件名称
旧组件为什么不能扩展
新组件与旧组件的职责差异
重叠范围
未来复用范围
是否只是 temporary preview
是否会造成两套维护
对 V0.2 / V0.3 稳定基线的风险
```

没有这份说明，不得直接新建职责重叠组件。

## 9. 与 UI Reuse Source Registry 的关系

本文件定义“怎么扩展”。

`Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md` 定义“哪些组件优先复用”。

两者必须一起读：

```text
先查 Reuse Source。
再决定 Extension Layer。
最后才考虑新建组件。
```

## 10. 对 V0.4 沙盒的当前裁定

`V0.4-BuildGridInteractionPreview01` 中已创建的棋盘、背包、弹窗等 UI：

```text
只作为 temporary preview / logic test bench。
```

后续不得继续把它们精修为正式组件。

正式接入时应优先形成：

```text
BoardOccupancyExtension
ItemTrayShapeExtension
DragRotationPlacementExtension
ItemInfoBuildFieldsExtension
BattleFeedbackMechanicHintExtension
BuildTuningDataPanelExtension
```

并通过 Adapter 接到已登记成熟组件上。
