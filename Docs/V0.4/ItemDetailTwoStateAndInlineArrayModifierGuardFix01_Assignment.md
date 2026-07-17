# V0.4-ItemDetailTwoStateAndInlineArrayModifierGuardFix01 Assignment

## 1. Package

```text
Package: V0.4-ItemDetailTwoStateAndInlineArrayModifierGuardFix01
Guard status: READY_FOR_TASK_WINDOW
Expected marker: ITEM_DETAIL_TWO_STATE_INLINE_ARRAY_MODIFIER_GUARDFIX01_PASS
Data maturity: BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED
```

本包是用户手测后的窄范围展示修复。目标只有两个：

1. 把 Item 详情页头部状态收敛为“点亮状态 + 阵脉状态”。
2. 阵脉生效后，把阵脉提供的数值或机制修正直接显示在前两大类中对应的属性、词条、核心或 Build 行后。

## 2. 绝对保护边界

进入任务后先记录相关资产与场景哈希，并严格遵守：

```text
不得修改用户已经手动调整的详情页整体布局
不得移动或重设既有 RectTransform
不得调整用户现有字号、间距、锚点、卡片尺寸与三大区域位置
不得修改当前三大分类的名称、顺序、模块归属或显隐原则
不得重建 Item 详情页 Prefab / Scene / UI hierarchy
不得覆盖现有 30 件道具、150 品阶版本的 Stat / Signature / Affix / Core / Build 内容与数值
不得修改 ItemSystemSnapshot.v1、Canonical Signature 或算法基础八包契约
不得连接正式 Battle、Reward、RunFlow、Inventory、SaveData、Boss 或正式掉落
```

允许的 UI 改动只能是：

```text
复用/收敛现有头部状态位
在既有内容行内部增加阵脉修正图标与增量文本
增加一个可编辑 Theme token 与阵脉修正 Sprite 引用
增加必要的只读 ViewModel / Projection 字段
```

如果实现发现必须移动用户布局或改写三大分类，立即停止并回报 Guard，不得自行处理。

## 3. 头部状态收敛

头部玩家状态只允许出现两个语义：

### 3.1 点亮状态

稳定玩家文案：

```text
尚未入阵
未点亮
点亮源
直接点亮
相邻接亮
```

规则：

```text
没有 placementId：尚未入阵
isLightingSource=true：点亮源
isDirectLit=true：直接点亮
IsRelayLit=true：相邻接亮
已入阵但 isLit=false：未点亮
```

### 3.2 阵脉状态

稳定玩家文案：

```text
阵脉未计算
未占阵脉
阵脉待亮
阵脉生效
```

规则：

```text
没有 placementId：阵脉未计算
isOnArrayBonusCell=false：未占阵脉
isOnArrayBonusCell=true && isArrayBonusActive=false：阵脉待亮
isArrayBonusActive=true：阵脉生效
```

### 3.3 开窍状态

移除/隐藏头部“已开窍 / 待开窍”状态位，不得再占用头部第三个 Badge。

开窍仍完整保留在每条核心效果内部，至少支持：

```text
未开窍
已开窍 · 未点亮
已开窍 · 生效中
品阶不可见/不具备资格
```

不得修改开窍算法、等级规则、核心资格或核心数据，只改显示归属。

## 4. 阵脉行内修正规则

### 4.1 玩家显示

阵脉不新增独立玩家大区块，也不修改三大分类。修正直接附着到实际目标行：

```text
攻击 18 [Icon_ArrayVeinModifier] +18
目标伤害提高 +10% [Icon_ArrayVeinModifier] +5%
引雷弹射 2次 [Icon_ArrayVeinModifier] +1次
雷击伤害提高 15% [Icon_ArrayVeinModifier] +5%
```

可附着目标：

```text
基础属性
本命效果 / Signature
固定词条
随机词条
核心效果
法门 Build 阶段
器类 Build 阶段
```

但不是所有行自动获得阵脉加成。只有候选载荷明确指向该目标，并且目标当前有效时，才显示修正。

### 4.2 显示条件

```text
isArrayBonusActive=false：不显示图标、不显示增量、不保留占位空格
isArrayBonusActive=true 但无目标修正：该行不显示阵脉内容
isArrayBonusActive=true 且目标修正有效：显示 Sprite 图标 + 格式化增量
目标效果未开窍、Build 阶段未激活或其他前置条件未满足：普通当前详情不显示“已生效”增量
```

移动出阵脉或失去点亮后，重新解析详情，所有阵脉行内修正必须消失；原始实例与候选数据必须保持不变。

### 4.3 数值与机制型修正

数值修正显示在同一行：

```text
基础值 + 阵脉增量
```

机制型修正无法压缩成单个数字时，作为同一目标下的阵脉专色子行：

```text
本命效果 · 引雷
命中后弹射 2 次 [阵脉图标] +1次
[阵脉图标] 最后一跳附加短暂麻痹
```

不得建立孤立的“阵脉效果尚未配置”或新的阵脉详情大区块。

### 4.4 数值格式

继续使用现有单位与精度契约：

```text
rawUnits=500, unitKey=basisPoint => +5%
不得显示为 +500%
LowerIsBetter 项必须保持正确方向，例如冷却 -0.5秒
```

UI 不得根据显示字符串反算数值，也不得在 View 中硬编码统一百分比。

## 5. 阵脉候选修正数据

Code Survey 必须首先记录当前事实：现有 `ItemArrayBonusRules` 只有 `isOnArrayBonusCell / isArrayBonusActive` 状态，没有数值或机制载荷。

为避免假显示，本包允许添加独立、可编辑、只读解析的候选数据结构。最低字段能力：

```text
modifierId
baseItemId
rarity / rarityVersionKey
sourceCellScope 或 sourceCellId
targetKind
targetId
targetParameterKey（需要时）
operation
rawUnits / unitKey / decimalPlaces / roundingMode
displayName / description（机制型修正需要）
stackingPolicy
dataMaturity
```

要求：

```text
覆盖 30/30 普通原型与 150/150 品阶版本
每个品阶版本至少有一条非空、合法、可追踪目标的阵脉候选修正
默认值为可调整的 BALANCE_CANDIDATE，不得标记 PLAYTEST_ACCEPTED 或 LIVE_LOCKED
所有目标必须能定位到当前 Item 详情中的真实 Stat / Signature / Affix / Core / Build 行
不得修改或覆写被修正目标的原始值
Workbench 必须能查看和手动编辑阵脉候选修正
迁移必须 additive + idempotent；再次运行不得覆盖用户已有阵脉编辑
```

示例中的 `+18 / +5%` 只说明显示形式，不是全局正式数值规则。任务窗口应填入保守、可编辑的候选种子，并在报告中明确 `NOT_BALANCE_APPROVED / NOT_BATTLE_CONNECTED`。

解析结果至少应提供：

```text
sourceKind = ArrayBonus
sourceId / sourceCellId
targetKind / targetId / targetParameterKey
baseRawUnits
deltaRawUnits 或 mechanismPayload
resolvedRawUnits
formattedDelta
isCurrentlyApplied
```

不得修改 `ItemSystemSnapshot.v1`。在现有阵脉布尔快照与候选配置之间建立独立 Adapter / Projection，供 Sandbox 详情只读消费。

## 6. 图标与专色

新增可编辑 Theme token：

```text
arrayModifierColor
candidate default = #55C6B3
```

新增或绑定玩家用 Sprite：

```text
Icon key: Icon_ArrayVeinModifier
建议视觉：小型菱形阵眼 + 两侧短脉络
尺寸：正文高度约 80%-90%
与增量数字基线对齐
```

规则：

```text
图标与增量使用同一阵脉专色
不得复用品阶色或 Build 激活色
不得只靠颜色表达来源
不得把 Unicode ◇、Emoji 或普通文字作为最终玩家图标
不得为了图标移动或重排用户现有模块
```

点击/长按支持来源说明时，可显示：

```text
阵脉加成 · 来源 AP02
```

该说明只能是轻量 Tooltip/Debug 展开，不得增加新的玩家大分类。

## 7. 验证矩阵

最低必须覆盖：

```text
头部可见状态语义数量 = 2
头部开窍 Badge = 0
三大分类名称/顺序/数量前后完全一致
用户手调 RectTransform / 字号 / 间距 / 锚点前后哈希或序列化值一致
未入阵：尚未入阵 + 阵脉未计算
已入阵未点亮且未占阵脉：未点亮 + 未占阵脉
已入阵未点亮且占阵脉：未点亮 + 阵脉待亮
直接点亮且占阵脉：直接点亮 + 阵脉生效
相邻接亮且占阵脉：相邻接亮 + 阵脉生效
点亮源状态正确
```

阵脉行内修正：

```text
150/150 品阶版本候选修正配置有效
基础属性目标 PASS
Signature 参数目标 PASS
固定词条目标 PASS
随机词条目标 PASS
核心目标 PASS
法门 Build 目标 PASS
器类 Build 目标 PASS
机制型子行 PASS
阵脉未生效时图标/增量计数 = 0
阵脉生效时每条有效修正均有 Sprite + arrayModifierColor
移出阵脉后恢复原始显示
失去点亮后恢复原始显示
原始实例 Canonical Signature 前后不变
basisPoint 500 => +5%
反转输入、重复解析与相同输入复现一致
孤立目标、未知 targetId、重复 modifierId、非法单位明确失败
```

历史回归必须包含：

```text
算法基础八包
ItemBalanceWorkbenchAnd150CandidateSeed01-GuardFix01
ItemCompleteCandidateContentWorkbench01
ItemBalanceCandidateDetailSandboxAdapter01
ItemFullDetailBuildSandboxWorkbench01
ItemFullDetailCompletePresentation01
ItemInnerDataCatalog
ItemSystemValidatorAndSnapshot
BuildSynergyCore
CoreAwakeningPreview
JuNian Lighting
ArrayBonus
```

## 8. LeakCheck

必须证明：

```text
Formal Battle changes = 0
Reward / RunFlow changes = 0
Inventory / SaveData changes = 0
Boss changes = 0
Formal drop changes = 0
ItemSystemSnapshot.v1 changes = 0
算法基础八包 Runtime changes = 0（只读调用除外）
用户手调 Scene / Prefab 布局变化 = 0
三大分类结构变化 = 0
```

## 9. 交付报告

至少生成：

```text
Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierReport.md
Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierSpec.csv
Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierFieldMatrix.csv
Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierStateMatrix.csv
Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierInventory.csv
Docs/V0.4/Reports/ItemDetailTwoStateInlineArrayModifierLeakCheckReport.md
```

最终 Console Marker：

```text
ITEM_DETAIL_TWO_STATE_INLINE_ARRAY_MODIFIER_GUARDFIX01_PASS
```

## 10. 用户手测

任务窗口交付时必须提供真实菜单路径，并要求用户至少验证：

1. 详情页三大分类与手调布局没有变化。
2. 头部只剩点亮与阵脉两个状态语义，没有开窍 Badge。
3. 普通未占阵脉道具看不到阵脉图标与增量。
4. 把道具放入阵脉但不点亮，仍看不到阵脉增量。
5. 直接点亮或相邻接亮并激活阵脉后，对应属性、词条、核心或 Build 行出现阵脉 Sprite 与专色增量。
6. 移出阵脉或切断点亮链路后，增量消失，原始数值恢复。
7. 点击不同品阶与不同实例时，阵脉增量来自对应候选配置，不是固定假数字。

用户手测通过前不得同步 RepoOps。
