# Shared Module Layering And Prefab Presentation Guard

维护方：Cross-System Guard / Item Guard / Enemy Guard / Capability Algorithm Guard

状态：

```text
GUARD_SYNC_SHARED_MODULE_LAYERING_ACCEPTED
GUARD_REQUIRE_PREFAB_PRESENTATION_READONLY
GUARD_REQUIRE_PLAIN_LANGUAGE_USER_WARNING
```

## 1. 建立目的

本文件把项目中的系统明确拆成四层：

```text
数据层 Data
状态层 State
展示层 Presentation
通信层 Communication
```

核心结论：

```text
数据层定义“它是什么”。
状态层记录“这一局现在是什么状态”。
展示层只负责“玩家看见什么、点了什么”。
通信层只负责“谁把什么请求交给谁”。
```

Prefab 属于展示层。共享 Prefab 只能读取 ViewModel / Snapshot 的投影结果并渲染，
不得持有、生成、修改或替代任何 Item、Enemy、Combat、Flow 权威数据与运行时状态。

本规则解决以下重复成本：

```text
同一弹窗在多个 Scene 重画
Scene Copy 丢失 Font / Sprite / serialized reference
ItemSandbox、BattleSandbox、UnifiedBattlePage 各维护一套 UI
Prefab 内挂载背包、敌人、战斗状态形成第二套真源
UI 直接修改数据导致跨场景行为不一致
设计师只有最后接入统一页时才能看到真实体验
```

## 2. 四层架构

### 2.1 数据层 Data Layer

数据层负责静态、可版本化、与场景无关的事实，例如：

```text
Item baseItemId、名称、形状、图标键、品阶规则、词条定义
Enemy enemyId、技能定义、阶段定义、视觉键
Stage / Encounter / Reward 的作者化配置
Build、Capability、Pressure、Requirement 的合同与规则参数
```

数据层可以使用项目现有 Catalog、Schema、Config、ScriptableObject 或纯数据合同，
但不得为了 Prefab 化重新建立第二套 `ItemData`、`EnemyData` 或正式配置真源。

数据层禁止：

```text
引用 Scene GameObject
引用运行时 View 实例
保存当前棋盘、当前 HP、当前选中道具等会话状态
因某个 UI 需要方便显示而复制一份同义数据
根据 PNG 尺寸、对象名或中文文案猜测正式规则
```

### 2.2 状态层 State Layer

状态层负责当前会话中会变化的权威状态，例如：

```text
当前背包与道具实例
itemInstanceId / placementId / baseItemId
当前棋盘占格、旋转、点亮、阵脉、Build状态
I031 Inventory / Board 状态与供能范围
玩家和敌人 HP、护盾、状态、冷却、施法条
当前关卡、波次、Boss阶段与流程状态
```

每类状态必须有且只有一个 Owner。只有该领域的 Service / Runtime Authority 可以修改；
其他系统只能读取不可变 Snapshot，或提交明确 Command 申请变更。

状态层禁止：

```text
由 Prefab 或 View 自己维护权威列表
新增 DontDestroyOnLoad 单例复制现有 Item/Enemy/Combat 状态
把缺失状态解释成 0、false、空背包或失败
由两个 Manager 同时写同一份状态
由 Scene hierarchy 充当权威存档
```

### 2.3 展示层 Presentation Layer

展示层包括：

```text
Base Prefab
Prefab Variant
View 组件
字体、Sprite、Material、Animator等视觉资源引用
Scene Slot
纯展示用 ViewModel
```

展示层唯一职责：

```text
读取 ViewModel 并渲染
把用户输入转换成 Intent
播放动画和局部视觉反馈
维护非权威的短暂显示状态
```

允许的短暂显示状态：

```text
当前页签
滚动位置
开关动画进度
Hover / Pressed / Selected 的视觉态
一次性飘字和过渡动画
```

这些状态不得反向成为 Item、Enemy、Combat 或 Flow 的权威状态。

Prefab 可以持有：

```text
Text / Image / Button / ScrollRect / Mask / Layout 等组件引用
Font / Sprite / Material / Animator 等共享视觉资产
View 脚本及内部视觉事件出口
不代表玩法事实的视觉默认值
```

Prefab 不得持有：

```text
Item、Enemy、Stage、Reward 的正式数据副本
当前背包或棋盘列表
当前 HP、护盾、技能冷却或 Boss 阶段
SaveData / Reward / RunFlow / Chapter 状态
第二套 ItemSystem、InventoryManager、EnemyManager 或 CombatManager
用于替代权威状态的 itemId / enemyId 特例表
```

确认口径：

> Prefab 展示层 UI 只读数据投影并渲染，不挂载权威数据，不拥有运行时业务状态。

### 2.4 通信层 Communication Layer

通信层包括：

```text
Coordinator / Composition Root
Presenter / Projector
Adapter
明确类型的 Command、Intent、Event 和接口
Snapshot Provider
```

通信层负责：

```text
接收 View 发出的用户 Intent
把 Intent 交给唯一状态 Owner
读取最新权威 Snapshot
投影成玩家可见 ViewModel
调用共享 View.Bind(ViewModel)
```

通信层不得拥有第二套业务状态，也不得把 EventBus、Adapter 或 Presenter 变成新的
Item/Enemy/Combat 真源。未经独立 Assignment 不得新增全局静态 EventBus、Service
Locator 或 DontDestroyOnLoad Manager。

标准链路：

```text
用户点击/拖动
→ View 发出 Intent
→ Page Coordinator 提交 Command
→ Domain State Owner 校验并更新
→ 输出新的不可变 Snapshot
→ Projector / Presenter 生成 ViewModel
→ Shared Prefab View.Bind(ViewModel)
```

View 不得跳过 Coordinator 直接写另一个系统；Item View 不得直接写 Enemy，Enemy View
不得直接改 Item，Battle View 不得直接写 Save/Reward/Chapter。

## 3. Prefab、Variant 与 Scene Slot

### 3.1 Base Prefab

Base Prefab 是共享视觉模块的唯一内部视觉真源，负责：

```text
内部层级
字体与图标引用
按钮和动画
字段与插槽
共享 View 脚本引用
内部布局和视觉规则
```

已经用户手测通过的视觉组块必须优先提取或维护为 Base Prefab。后续场景不得复制成
独立 hierarchy 长期维护。

### 3.2 Prefab Variant

只有真实存在场景差异时才建立 Variant。允许覆盖：

```text
特定页面尺寸
皮肤
少量间距
移动端适配
经 Assignment 批准的布局差异
```

Variant 不得复制或替代数据、Presenter、状态 Owner、字段语义和业务逻辑。若内部布局
完全相同，直接引用 Base Prefab，不创建无意义 Variant。

### 3.3 Scene Slot

Scene 负责最终组合。Scene Slot 只控制：

```text
模块在整页中的位置
可用尺寸
显示层级
Safe Area
Canvas / Sorting / PopupLayer 归属
```

Scene Slot 不得重新定义 Prefab 内部字段、字体、图标或业务状态。场景中的成熟共享模块
必须保持 Prefab Instance 关系；禁止 Unpack 后长期独立维护。

## 4. Sandbox 与 UnifiedBattlePage

Sandbox 是设计师可操作的真实体验容器，不是另一套产品实现。

```text
ItemSandbox：使用共享 Item Prefab + Item 测试 Provider
BattleSandbox：使用共享 Board/Tray/ItemDetail Prefab + BuildSandbox Provider
EnemySandbox：使用共享 Enemy Prefab + Enemy 测试 Provider
UnifiedBattlePage：使用同一批共享 Prefab + 正式/桥接 Provider
```

允许 Sandbox 使用 fixture 数据，但必须通过与 Unified 相同的公开 View/Presenter 接口，
不得为了测试方便复制 UI、复制状态算法或创建第二套身份体系。

桥接页不是所有后台包结束后的最后一步。它必须尽早提供可玩的 Composition Shell，按
纵向切片逐步接入真实共享模块，让用户持续看见、操作和判断体验。

## 5. 各 Guard 的职责

### 5.1 Item System Guard

必须保证：

```text
Item 数据和状态只有一个真源
Item Prefab 不持有 Item 真源或背包状态
详情 View 只消费 ItemDetailViewModel
ItemSandbox 与其他场景复用同一视觉 Prefab
不因 UI 方便新增第二套 ItemData / InventoryManager
```

### 5.2 Item / Capability Algorithm Guard

必须保证：

```text
算法只消费稳定 Snapshot / Fact / Contract
算法不读取 Scene、Prefab、Text、Image 或中文文案
算法输出结果，不直接控制 UI
Prefab 不保存算法分数、阈值或权威判定
```

### 5.3 Enemy System Guard

必须保证：

```text
Enemy 数据、Runtime状态与视觉展示分离
Enemy Prefab 只读 Enemy/Combat ViewModel
Enemy 不建立 Item 形状、供能、Build 的第二套真源
Enemy View 不直接修改 Item、Reward、Save或Chapter
```

### 5.4 Cross-System Guard

必须保证：

```text
跨系统只通过明确 Contract / Snapshot / Command / ViewModel 通信
UnifiedBattlePage 只组合，不复制各系统真源
每个字段都能回答“谁拥有、谁修改、谁只读”
共享视觉模块使用 Prefab/Variant/Slot，而不是 Scene Copy
跨层修改在 Assignment 中显式列出并取得相应 Guard批准
```

## 6. Guard 强制提醒用户

用户不是程序员。所有 Guard 在下发 Assignment、发现越层或建议新架构时，必须先用
非技术语言说明，不得只返回哈希、枚举、接口名或 `GUARD_RETURN`。

每个涉及数据、状态、UI或跨系统的 Assignment 必须包含：

```text
本包属于哪一层
唯一真源是谁
谁可以修改状态
Prefab 是否只读渲染
是否复用现有 Base Prefab
Scene 只改 Slot 还是会改内部UI
用户在哪个场景、通过什么操作手测
为什么这样开发更合理
如果越层，玩家侧可能出现什么问题
```

发现下列情况时，Guard 必须暂停放行并提醒用户：

```text
Prefab 挂载或复制权威数据
View 直接修改业务状态
Scene Copy 被当成长期共享 UI
同一系统出现第二个 Manager / Catalog / Snapshot真源
跨模块直接引用另一个模块的内部脚本或字段
成熟 UI 被重新画而不是复用 Prefab
Scene-local Font / Sprite 导致跨场景引用不稳定
Runtime Builder 重建用户已经手调的 UI
超过两个技术包仍没有可操作的设计师手测节点
最终桥接页被推迟到所有系统完成以后才开始
```

标准提醒格式：

```text
GUARD_LAYERING_WARNING
触碰层级：<Data / State / Presentation / Communication>
当前唯一真源：<owner>
发现的问题：<非技术说明>
如果继续：<玩家可见风险>
更合理方案：<Prefab / Variant / Slot / Adapter / Snapshot>
需要用户决定：<是/否或明确选项>
```

Guard 不得替用户默许“为了方便先复制一套”。如果必须做临时 Preview，必须标记退场点，
且不得在该复制品上继续投入正式美术与长期手调。

## 7. 设计师手测节奏

系统开发不得长期盲眼推进。默认要求：

```text
先做一个真实样本
→ 放入真实比例的 Sandbox / Unified Composition Shell
→ 用户手测视觉和手感
→ 固化为共享 Prefab
→ 再批量扩展全部数据和内容
```

每一至两个底层技术包后必须出现一个可操作的 Designer Handtest；若做不到，Guard 必须
提前告诉用户距离手测还剩几包、原因是什么，并得到用户继续授权。

自动测试证明代码和合同没有破坏；用户手测决定视觉、操作和体验是否通过。用户手测失败
优先于较弱的静态 PASS。

## 8. 当前 Item Detail 裁定

当前 `Revision09-Rev03` 允许完成共享 `ItemDetailPanelView` 与
`ItemDetailSectionView` 的字体、字段、authored rows 与图标可见性修复，作为既有复制
场景的兼容收口。

该包通过后，下一视觉架构步骤必须是 Item Detail Prefab 化：

```text
用户验收通过的 Item Detail
→ 唯一 Base Prefab
→ 必要时建立薄 Battle/Unified Variant
→ ItemSandbox、BattleSandbox、UnifiedBattlePage 引用同一视觉真源
```

不得继续为 ItemSandbox 与 BattleSandbox 分别修同一组字体、字段、图标和布局。Prefab
化不得重建第二套 ItemData、InventoryManager、EventBus 或运行时状态。

## 9. 红线与例外

以下为红线：

```text
Prefab 成为业务数据真源
UI 直接写 Save/Reward/Chapter
Scene hierarchy 成为 Item/Enemy/Combat 状态真源
同一视觉模块在多个场景独立维护
复制成熟 UI 后再投入正式手调而没有 Prefab 退场计划
用运行时反射、名称或中文文案猜测跨系统身份
为了快速接线新增第二套全局 Manager
```

例外必须由 Assignment 明确说明：为什么无法使用共享 Prefab、临时对象何时删除、谁负责
迁移、用户在哪个节点手测。没有退场条件的临时实现不得放行。

## 10. 什么时候必须提醒用户 Prefab 化

Guard 必须在以下时间点主动提醒用户，不得等待用户自己发现重复维护：

### 10.1 第一个代表样本手测通过后

当一个 UI 组块已经满足：

```text
核心字段和按钮基本齐全
用户确认大方向、比例和交互可用
共享 View / Presenter 接口已经稳定
```

此时进入 `PREFABIZATION_RECOMMENDED`。可以继续微调美术，但应该在 Base Prefab 中
继续调，不应继续只调 Scene Copy。

### 10.2 第二个场景准备使用之前

这是强制门槛：

```text
同一视觉组块即将进入第二个 Scene
→ 必须先 Base Prefab 化
→ 或由 Guard 明确批准有退场日期的短期 Preview
```

禁止先复制到第二个 Scene，等两个版本都调完后再考虑共享。

### 10.3 批量内容、美术或多品阶扩展之前

准备批量接入大量道具、敌人、技能、品阶、图标或字段之前，必须先确认视觉承载模块已
Prefab 化。否则批量内容会放大重复绑定与场景漂移成本。

### 10.4 进入 UnifiedBattlePage 之前

Sandbox 中已验收的可见组块必须先明确：

```text
Base Prefab
必要 Variant
Scene Slot
Presenter / Provider
```

没有共享视觉真源的组块不得直接复制进 Unified 页面。

### 10.5 出现以下信号时立即提醒

```text
用户说“复制到另一个场景”
同名 UI 在两个 Scene 各有一份
字体、图标或按钮引用开始漂移
同一修复需要在两个场景重复做
开发窗口准备 Runtime 重建成熟 UI
用户已经开始投入正式美术和长期手调
```

标准提醒：

```text
GUARD_PREFABIZATION_TIMING_WARNING
这个组块已经达到适合 Prefab 化的阶段。
如果继续在 Scene 内独立修改，后面每个场景都要重复接线和修复。
建议现在冻结一个 Base Prefab；页面差异放 Variant，整页位置放 Scene Slot。
```

## 11. 什么时候暂时不要 Prefab 化

以下阶段可以保留明确标记的临时 Prototype：

```text
交互方向还没有经过一次用户手测
核心字段仍频繁增删
用户正在比较两个完全不同的方案
模块职责尚未确定，随时可能删除
```

但临时 Prototype 必须有：

```text
devOnly / temporary 标记
禁止进入正式美术批量生产
禁止进入第二个产品场景
明确的手测点
手测通过后立即 Prefab 化的退场条件
```

“还可能调整”不能成为长期不 Prefab 化的理由。结构稳定后，视觉细节可以继续在
Base Prefab 中调整。

## 12. 什么时候适合创建新类型

允许创建新类型的条件：

```text
现有类型无法表达新的稳定业务概念
新类型有独立、清晰、长期存在的职责
输入、输出、Owner 与生命周期可以说明
不会复制现有 Catalog、Snapshot、Manager 或 ViewModel 的职责
至少有一个真实消费者，而不是为未来猜测
```

不适合创建新类型：

```text
只是现有 View 多一个字段
只是不同页面尺寸或皮肤
只是一个临时按钮或提示
只是为了绕开现有接口
只是把同一份数据换名字复制一份
```

此时优先选择：扩展字段、ViewModel、Adapter、Prefab Variant 或明确的测试 Fixture。

Guard 必须向用户说明：新类型代表什么、为什么现有类型不能扩展、以后由谁维护。

## 13. 什么时候适合创建新场景

允许创建新场景：

```text
有独立进入/退出生命周期的产品页面
需要隔离验证高风险系统且现有 Sandbox 无法安全承载
需要独立 Canvas、Camera、Lighting 或加载边界
需要长期存在的设计师测试容器，并且明确不作为模块真源
```

不适合创建新场景：

```text
只为了换一种 UI 布局
只为了测试一个弹窗或一个 Prefab
只因为另一个 Scene 接线麻烦
只为了复制成熟页面再改一份
```

这类需求优先使用现有 Sandbox、Prefab Preview、Prefab Variant、Scene Slot 或
runtime-only dev comparison。新场景必须说明是否进入 BuildSettings、是否 devOnly、
谁负责导航、何时删除、使用哪些共享 Prefab。

## 14. 什么时候适合画新 UI

允许画新 UI：

```text
玩家面对的是新的信息职责或新的交互任务
Reuse Source Survey 证明没有成熟组件可以承担
扩展旧组件会造成职责混乱或破坏既有用途
用户先确认灰盒方向，再投入正式美术
新 UI 从第一天就以共享 Base Prefab 为目标
```

不适合画新 UI：

```text
只是给已有弹窗增加字段
只是同一模块进入另一个场景
只是换皮、缩放或移动位置
只是 Adapter 尚未完成
```

此时应使用现有 Base Prefab、Extension、Variant 或 Scene Slot。

## 15. 每个任务开工前的强制设计提醒

Guard 在每个可见系统或跨系统包开工前必须向用户给出一段简明结论：

```text
当前阶段：Prototype / Handtest / Prefabization / Integration / Formalization
这次是否该 Prefab 化：是 / 否，以及原因
是否需要新类型：是 / 否，以及原因
是否需要新场景：是 / 否，以及原因
是否需要新 UI：是 / 否，以及原因
用户本轮可以看到和调整什么
下一次必须手测的节点
```

如果用户的请求会导致重复 UI、第二套数据或过晚集成，Guard 必须主动解释更合理的开发
方式；不得因为用户不是技术人员而默认其已经理解工程后果。

## 16. End-to-End Player Field Lineage Gate

分层合规不等于真实数据已经接通。任何玩家可见字段进入 Sandbox、共享 Prefab、
UnifiedBattlePage 或正式流程前，必须建立端到端字段血缘：

```text
权威 Data Source
→ Runtime Producer / State Owner
→ Snapshot / Contract Field
→ Projector / Composer / Presenter
→ ViewModel Field
→ Shared Prefab View / UI Node
→ Real Runtime Sample
→ User Handtest
```

任一环节缺失、Unknown、使用 Fixture 替代真实 Producer，或只显示兜底文案时，该字段
不得标记为接线 PASS。

### 16.1 强制 Field Lineage Matrix

每个涉及玩家可见字段的 Assignment、Verifier 与 Guard 审核必须至少提供：

| 字段 | 含义 |
| --- | --- |
| `fieldKey` | 稳定字段键 |
| `playerLabel` | 玩家看到的中文字段 |
| `dataOwner` | 静态事实唯一 Owner |
| `dataSource` | 真实 Catalog / Config / Contract 来源 |
| `runtimeProducer` | 谁生产本局值 |
| `stateOwner` | 谁可以修改本局值 |
| `snapshotField` | 权威 Snapshot / Contract 字段 |
| `projector` | 谁将其投影为显示数据 |
| `viewModelField` | 最终 ViewModel 字段 |
| `prefabNode` | 共享 Prefab 中的显示节点 |
| `realSampleId` | 用于验证的真实 Item / Enemy / Stage / Build |
| `fixtureResult` | 组件 Fixture 结果 |
| `runtimeResult` | 真实生产链结果 |
| `handtestResult` | 用户可见手测结果 |
| `fallbackMeaning` | 缺数据时的兜底含义，是否允许发布 |

不允许只填写类型名、方法存在或 `non-null`。数值、行数、身份、阶段、状态与来源必须能
对应到同一真实样本。

### 16.2 三种 PASS 必须分开

报告和回执必须分别声明：

```text
COMPONENT_FIXTURE_PASS
REAL_RUNTIME_PATH_PASS
USER_HANDTEST_PASS
```

含义：

```text
COMPONENT_FIXTURE_PASS：给定完整ViewModel时，View能够显示。
REAL_RUNTIME_PATH_PASS：真实Catalog/Runtime/Snapshot经过真实Projector后字段仍完整。
USER_HANDTEST_PASS：用户在指定Scene执行指定操作后实际看见并接受。
```

任何一个不得替代另一个。未运行必须写 `NOT_RUN`，不能省略或合并成笼统
`STATIC_PASS`。

### 16.3 真实生产链规则

玩家字段的端到端测试必须使用生产路径：

```text
真实 Catalog / Config
真实 runtime identity
真实 State Owner 生成的 Snapshot
真实 Projector / Composer
真实 ViewModel
真实共享 Prefab或当前待迁移 View
```

禁止仅使用以下方式宣布接线通过：

```text
CreateRepresentativeModel
手工构造期望Build rows
Editor-only fixture snapshot
与生产代码共用同一个Expected helper的自证测试
只检查方法被调用或结果非空
```

Fixture 仍可验证组件，但报告必须明确它没有证明真实生产数据接通。

### 16.4 兜底文案政策

兜底文案成功渲染只证明 Presentation 没崩溃，不证明业务字段通过。

例如验收目标是 Build 轨道，而玩家看到：

```text
当前道具没有可统计的Build轨道
```

则必须记录：

```text
Presentation fallback rendered: PASS
Build track real runtime path: FAIL or NOT_AVAILABLE
Feature acceptance: FAIL
```

禁止因为兜底文字可见、字体正常或 ViewModel 非空就把 Build 字段标记为 PASS。

### 16.5 Schema 存在不等于 Producer 就绪

以下只能证明结构可表达，不能证明已有真实值：

```text
字段存在
Schema合法
Snapshot整体Valid
Adapter和Composer存在
调用次数正确
Canonical Signature稳定
```

Guard 必须另外确认 `runtimeProducer` 已经为真实样本生产该字段。若 Schema 已有但没有
Producer，状态必须写 `PRODUCER_NOT_CONNECTED`；若 Producer存在但没有进入 Snapshot，
写 `SNAPSHOT_FIELD_MISSING`；若 Snapshot有值但ViewModel为空，写
`PROJECTION_MISSING`；若ViewModel有值但玩家看不到，写 `PRESENTATION_MISSING`。

### 16.6 集成时强制真实样本

每个跨层接线包至少选择一个真实纵向样本，完整执行：

```text
真实数据
→ 真实实例/运行状态
→ 真实Snapshot
→ 真实投影
→ 真实Prefab
→ 用户可操作场景
```

如果字段按家族、法门、器类、敌人机制或状态分支变化，必须每个分支至少一个真实样本。
批量内容完成时，再做全量覆盖；不得用一个合成万能 Fixture 代表所有分支。

### 16.7 Guard 返回与用户提醒

发现字段血缘断点时统一返回：

```text
GUARD_END_TO_END_FIELD_LINEAGE_WARNING
断点字段：<玩家可见字段>
最后确认有值的层：<Data / State / Snapshot / Projection / ViewModel / View>
第一个缺失的层：<layer>
玩家看到的问题：<非技术说明>
Fixture是否通过：<PASS/FAIL/NOT_RUN>
真实生产链是否通过：<PASS/FAIL/NOT_RUN>
建议修复Owner：<system>
```

Guard 必须向用户说明：“结构存在”和“真实数据已经显示”之间的区别。未定位第一个断点
前，不得通过更换 Prefab、补默认值或写假数据掩盖问题。

## 17. 当前 Item Detail Build Track 失败案例

Revision09-Rev03 用户手测观察到：

```text
字体、文字、图标最终渲染正常
真实BattleSandbox详情显示“当前道具没有可统计的Build轨道”
```

该结果必须拆分为：

```text
Presentation rendering: PASS
Build track component fixture: PASS or existing evidence
Build track real runtime path: FAIL / MULTIPLE_BREAKPOINTS
Revision09 overall user acceptance: FAIL
```

后续 Item Detail Prefab A/B 可以比较同一 ViewModel 在 Scene Copy 与 Base Prefab 中的
显示差异，但不得把 Prefab 当作 Build 数据生产者。若两个 View 都收到空 Build 轨道，
修复归属 Data / State / Communication，不归属 Prefab Presentation。

Item Guard 已完成只读字段血缘审计，确认：

```text
FirstMissingLayer: Runtime Producer / State Assembly
SecondaryBreakpoint: Communication / Projection
PresentationMissing: NO
```

Battle 当前没有把 Roll 实例的 `BuildQualification` 与 IF01、
`ItemSystemSnapshot.v2` 组装成实例资格正确的只读 Build 状态；道具栏详情又使用
`CatalogPreview + empty placementId`，因此 Composer 无法取得该实例的法门/器型轨道。

冻结修复顺序：

```text
V0.4-ItemInstanceQualifiedBuildStateAdapter01
→ V0.4-ItemDetailQualifiedBuildTrackProjection01
→ 真实BattleSandbox Build轨道用户手测
→ Shared ItemDetail Base Prefab A/B
```

上述两个包不得修改 View、Scene、Prefab、字体、图标、Roll概率、Item数值、Enemy、
Capability或正式流程。Prefab A/B 在真实 Build轨道链通过前保持阻塞。

## 18. Guard 直接流转与禁止用户代传

用户已明确接受：

```text
GUARD_DIRECT_THREAD_ROUTING_ACCEPTED
GUARD_FORBID_USER_AS_RECEIPT_COURIER
GUARD_REQUIRE_SINGLE_CONSOLIDATED_ASSIGNMENT
```

用户不是 Guard、开发窗口与 RepoOps 之间的传话人。后续默认工作流为：

```text
用户提出目标或设计决定
→ 总体 Guard 判断系统归属与主责 Guard
→ 主责 Guard 直接请求必要的联合 Guard
→ 主责 Guard 合并意见并生成一份最终 Assignment + SHA
→ Assignment 直接发送给开发任务
→ 开发结果直接同步主责 Guard + RepoOps
→ 只有设计决策或 Unity 手测需要用户参与
```

各 Guard 必须使用任务协调能力直接发送审核请求、回执、Assignment和状态同步；工具可用时，
不得要求用户复制长回执到另一个窗口。工具不可用或目标任务无法定位时，才允许向用户提供
一段最短必要的手动转发文本，并明确这是工具限制，不是标准流程。

默认只设一个主责 Guard。只有实际触发以下条件时才增加联合 Guard：

```text
修改另一系统真源
新增或改变跨系统合同语义
出现第二状态 Owner
接入正式 Battle / RunFlow / Save / Reward / Chapter
修改算法概率、单位、阈值或 Enemy requirement
```

只读 Adapter、Presenter或Snapshot消费若已处于既定授权边界内，继续由主责 Guard单独收口，
不得为同一边界逐包重复签字。

给用户的过程更新必须缩短为：

```text
现在在做什么
为什么需要这一步
什么时候需要用户查看或手测
```

直接流转不扩大任何权限。Guard不得因此自动 commit/tag/push、跳过用户手测、扩大白名单、
启动未授权后续包或代替用户作出玩法语义决定。
