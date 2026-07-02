# BUILD SANDBOX BOUNDARY LOCK

本文件锁定《符箓背包》Build / 词条 / 羁绊 / 多格占位 / 验证池后台沙盒线的边界。

本线可以在用户手调 UI、位置、尺寸、美术风格期间并行推进；但它不是正式玩法接入包，不得污染当前 V0.3 UI 主线、V0.2 稳定基线或正式 1-10 / 2-10 流程。

## 1. 当前 Guard 结论

```text
GUARD_ACCEPT_AS_PARALLEL_SANDBOX_QUEUE
GUARD_DO_NOT_BLOCK_ON_UI_HAND_TUNE
GUARD_KEEP_ISOLATED_FROM_V0.3_UI_MAINLINE
```

含义：

```text
用户继续在 Unity 中手调 UI / 美术 / 版面。
Codex 可以并行开发 BuildSandbox 技术底座。
两条线互不等待，但也互不污染。
```

## 2. 最高原则

```text
步子可以大，接入必须小。
数据结构可以一次性铺开，正式功能必须全部关着。
```

允许大步建设：

```text
道具标签
羁绊配置
多格占位
词条 / 品质
Build 计算
Modifier / Event 中间包
自动模拟
敌人 / Boss 验证池
开发章节池
配置校验
报告导出
```

禁止大步接入：

```text
正式 1-10 / 2-10 主线
正式掉落
正式锻造 / 洗练
正式战斗数值
正式 UI
正式存档结构
正式玩家入口
```

## 3. FeatureFlag 默认关闭

以下开关必须默认关闭：

```text
EnableSynergyBuild = false
EnableAffixSystem = false
EnableDevBuildContent = false
EnableBuildModifierInCombat = false
EnableBuildDebugReport = false
EnableItemShapeOccupancy = false
EnableShapePlacementSandbox = false
EnableShapeRotation = false
```

任何包若把上述功能默认开启，直接红灯。

## 4. devOnly 隔离

所有新增 BuildSandbox 内容默认：

```text
devOnly = true
isEnabled = false
```

不得进入：

```text
正式掉落
正式锻造
正式洗练
正式主线
正式 1-10 / 2-10
正式 UI
正式玩家入口
```

## 5. 架构方向

正确结构：

```text
当前稳定战斗系统
   ↓ 输出只读快照
BattleLayoutSnapshot
   ↓
SynergyEvaluator
   ↓
BuildEvaluationResult
   ↓
CombatModifierBundle / EffectEventBundle
   ↓
可选接入 BattleSystem
```

禁止结构：

```text
羁绊系统直接修改旧战斗脚本
羁绊系统直接操作棋盘 GameObject
羁绊系统直接改现有伤害脚本
羁绊系统直接改道具触发脚本
羁绊系统直接改 UI 表现
```

## 6. 绝对禁止触碰

BuildSandbox 包默认不得修改：

```text
当前正式 1-10 / 2-10 主流程
当前首页 UI 布局
当前战斗页 UI 布局
当前整备升降交互
用户手调 RectTransform
BottomBar
RunFlow
PageState
FormationState
DamageText
V02FormationGridFrame
SaveData / MainTrialProgressData / PlayerPrefs 结构
RewardConfig / UpgradeConfig 正式表
Boss 触发规则
奖励 / 掉落 / 数值
```

如确需触碰其中任何项，必须停止并单独回 Guard 红灯审查；不得以“沙盒需要”为理由绕过。

## 7. UI Runtime Lock 继承规则

BuildSandbox 包必须继承全局 UI 手调锁：

```text
Codex 搭骨架，用户调布局，Lock 后只读校验。
```

BuildSandbox 不得在当前 V0.3 UI 场景中：

```text
重建 Canvas / Root / SafeArea
移动用户手调对象
覆盖 RectTransform / sibling order
覆盖 Image / Outline / Text 手调视觉字段
每次 Play 创建另一套 UI
```

如需测试可视化，仅允许做 devOnly 报告、Editor-only 调试窗口或独立沙盒测试对象，不得接入正式 UI。

## 8. 当前并行队列

当前 BuildSandbox 并行队列记录在：

```text
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
```

当前可执行首包：

```text
V0.3/V0.4-BuildSandbox-GuardBaseline01
```

该包只建立护栏、FeatureFlag、devOnly 检查、Config Validation 入口、UI Layout Guard 与报告框架；不开发正式 Build 玩法。

## 9. 每包统一检查

每个 BuildSandbox 包完成后必须至少检查：

```text
Compile Check
FeatureFlag 默认关闭
devOnly Isolation Check
Config Validate
UI Layout Guard
正式 1-10 / 2-10 不受影响
报告导出
```

若某包还未接入可运行模拟器，则可先输出占位报告或校验报告，但不得伪造正式战斗通过。

## 10. 回滚与暂停条件

出现以下任一情况，本线立即暂停：

```text
正式主流程被接入
FeatureFlag 默认 true
devOnly 内容泄漏到正式流程
UI 布局被重建或手调值被覆盖
旧战斗脚本被直接修改
正式奖励 / 数值 / Boss / 存档被改
BuildSandbox 影响 V0.2 Golden Path
```

暂停后不得继续叠包，必须回 Guard 做失败收口。

## 11. Phase 2 Playable Preview 追加边界

Guard 已接收 V0.4 BuildSandbox Phase 2：

```text
GUARD_SYNC_BUILD_PROBLEM_RULE_POOL_ACCEPTED
GUARD_ACCEPT_BUILDSANDBOX_PHASE2_PLAYABLE_PREVIEW_QUEUE
GUARD_KEEP_PHASE2_DEVONLY_AND_ISOLATED
```

Phase 2 定位：

```text
中层题库规则 + 下层可玩验证舱
```

Phase 2 可以新增：

```text
BuildProblemRulePool
BuildProblemSeedData
BuildSandbox Config Panel 01
Scene_TalismanBag_V04_BattleSandboxPreview
BuildSandboxPreviewContext
BattlePageViewAdapter
BuildGridInteractionPreview
BuildDataPanelPreview
EnemyBossProblemPreview
DevChapterProblemRun
PlayableRegression
```

但仍必须保持：

```text
devOnly = true
isEnabled = false
FeatureFlag = false
```

3-10 / 4-10 当前只能作为：

```text
devOnly Build 验证章节
```

禁止：

```text
3-10 / 4-10 devOnly 章节进入正式主线
Boss 验证池进入正式 Boss 列表
DropBias 改正式掉落表
BuildReadinessCheck 改正式 BossInfoPanel
BattleSandboxPreviewScene 改正式 BattleScene
BattlePageViewAdapter 反向写正式 UI
MapRule 默认启用
FeatureFlag 默认 true
devOnly = false
```

Phase 2 如需可视化，只能使用独立沙盒场景：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
```

不得接入：

```text
Scene_TalismanBag_V02_FormationCounter
Scene_TalismanBag_V03_MainHome
Scene_TalismanBag_V03_TalismanUpgrade
```

## 12. Phase 2 UI 复用与答案遮罩规则

Phase 2 可视化遵守：

```text
UI 能复用的就尽量复用。
不要给每个机制新增大量独立 UI 框。
机制、调参、Readiness、DropBias、题库规则优先进入统一数据面板。
Boss 技能信息、地图机制、反馈提示优先复用既有战斗提示 / 伤害提示 / 战斗日志 / Tooltip / BossInfo 的 UI 语言。
```

开发者侧可以显示完整数据：

```text
Boss 六钥匙
阈值
缺口
DropBias
失败原因
推荐补法
```

玩家侧只能显示：

```text
氛围描述
机制暗示
战斗反馈
失败反馈
掉落倾向表现
弱点窗口表现
```

玩家侧禁止直接显示：

```text
hardSolutionTags
requiredSynergy
requiredAffix
requiredStats
DropBias 权重
Boss 六钥匙完整答案
```

所有配置、报告、开发者数据面板字段层应尽量建立：

```text
中文显示名 / English stable key
```

玩家侧 UI 不要求中英文双语：

```text
玩家 UI 只显示中文。
English stable key 仅用于配置、报告、开发引用和调参面板字段追踪。
```

## 13. Unity 命名稳定规则

Unity 工程对象命名必须稳定、可脚本引用、可工具扫描。

以下内容默认使用英文稳定命名：

```text
Hierarchy GameObject name
Prefab name
Scene object path
File name
Folder name
Component field name
Serialized reference key
Script class / method / enum / id
```

中文只允许作为显示内容或说明：

```text
UI Text / TMP 显示文案
配置 displayName / description
报告说明
开发者数据面板中的中文显示名
策划文档说明
```

禁止：

```text
在 Hierarchy 中新增中文 GameObject 名
用中文对象名作为脚本查找路径
用中文文件名作为 Unity 资产主路径
把中文显示文案当作稳定 id / key
```

原因：

```text
避免 Unity 引用、脚本查找、序列化、批处理工具、跨工具协作和 Git diff 出问题。
```

## 14. 成熟 UI 复用源规则

V0.4 BuildSandbox 后续可视化必须遵守成熟 UI 复用优先：

```text
成熟 UI 优先复用。
新系统只做 Adapter / ViewModel / 字段扩展。
不能复用时必须先向 Guard 写明原因，不得直接重画。
```

权威登记表：

```text
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
```

默认复用方向：

```text
V0.3 BattlePrepare 上拉背包 / 道具栏 / 棋盘整备手感
V0.2 / V0.3 道具信息弹窗或道具 Tooltip 信息结构
BossInfoPanel / Boss 信息展示语言
战斗提示 / 伤害反馈 / CombatLog / Tooltip 语言
V0.4 BuildSandbox Config Panel 01 的开发者数据面板形态
```

V0.4 沙盒中新建的 Board / ItemTray / Popup / ProblemSelector / DataBlock 默认定位为：

```text
temporary preview / logic test bench
```

这些临时 UI 只允许用于验证：

```text
多格占位
旋转
越界 / 重叠 / 合法放置
分类筛选
PreviewContext 数据流
报告 / Validator / leak check
```

不得把它们继续精修为正式 UI 真源。

后续正式体验必须走：

```text
V0.4 BuildSandbox data
→ Adapter / ViewModel
→ 已登记成熟 UI / 交互源
```

禁止：

```text
仿制 V0.2 / V0.3 已成熟 UI
重新调一套 V0.4 背包上拉动画手感
重新调一套 V0.4 棋盘 / 道具栏正式布局
长期维护新的 V0.4 ItemInfoPopup 作为正式弹窗
为了沙盒方便直接修改正式 V0.2 / V0.3 UI
绕过 Adapter 直接写正式 UI
```

如果任务窗口认为某成熟 UI 无法复用，必须先输出：

```text
不能复用原因
涉及的源 UI / 脚本 / 场景节点
尝试过的 Adapter 方案
新增 UI 的必要性
对 V0.2 / V0.3 基线的风险
```

并等待 Guard 收口，不得直接开画。

## 15. 成熟组件优先扩展规则

BuildSandbox 与后续正式接入必须遵守：

```text
成熟组件优先扩展，不重建。
```

权威规则文件：

```text
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
```

本规则不是禁止新玩法，也不是禁止修改 UI / 交互。

允许：

```text
在成熟组件上新增按钮
新增状态层
新增占格预览
新增旋转交互
新增合法性校验
新增数据字段
新增分类
新增反馈状态
新增 Adapter / ViewModel
```

禁止：

```text
因为新增一项功能就重画整套棋盘
因为多格道具就重做整个道具栏
因为 V0.4 要验证就另写一套拖拽手感
已有成熟弹窗可扩展时新造一个相似弹窗
用 temporary preview 替代成熟组件
两个职责高度重叠的组件长期并存
```

正确结构：

```text
Base Component
→ Extension Layer
→ Adapter Layer
```

例如 V0.4 多格占用应优先形成：

```text
BoardOccupancyExtension
ItemTrayShapeExtension
DragRotationPlacementExtension
ItemInfoBuildFieldsExtension
BattleFeedbackMechanicHintExtension
```

而不是：

```text
V04Board
V04ItemTray
V04DragSystem
V04ItemInfoPopup
```

如果任务窗口认为必须新建职责重叠组件，必须先输出旧组件不能扩展的原因、重叠范围、未来复用范围和风险，并等待 Guard 收口。

## 16. Stable Battle Runtime + BuildSandbox Integration Strategy

权威策略文件：

```text
Docs/V0.4/BUILD_SANDBOX_INTEGRATION_STRATEGY.md
```

Guard 当前裁决：

```text
GUARD_SYNC_STABLE_BATTLE_RUNTIME_PLUS_BUILDSANDBOX_MODULES_ACCEPTED
GUARD_FORBID_DUPLICATE_FULL_BATTLE_SYSTEM
GUARD_FORBID_FORCED_REFACTOR_OF_STABLE_BATTLE
```

长期结构锁定为三层：

```text
第一层：V0.2 / V0.3 Stable Battle Runtime
第二层：V0.4 BuildSandbox 能力模块
第三层：Snapshot / Adapter 合入
```

含义：

```text
V0.2 / V0.3 稳定战斗主链路保留。
V0.4 新能力在 BuildSandbox 中隔离开发。
纯逻辑模块可以干净复用。
UI / 拖拽 / 棋盘等高风险表现层先在 V0.4 独立沙盒验证。
未来只能通过 Snapshot / Adapter 分阶段合入正式主线。
```

禁止：

```text
把 V0.4 写成第二套完整正式战斗系统
为了 V0.4 直接重构或拆坏 V0.2 / V0.3 稳定战斗
强行把 V0.4 多格摆放塞进旧 BattlePrepare 单格 drag/drop 内核
让 V0.4 模块直接修改正式 BattleScene / RunFlow / SaveData / Boss / 奖励 / 数值
```

当前 V0.4 多格摆放路线：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
→ V0.4 ShapePlacementSession
→ V0.4 ShapeAwareItemTrayGrid
→ V0.4 ShapeGridReceiver
→ V0.4 MobileShapePlacementInput
```

当前不再继续：

```text
overlay + ignoreLayout + 延后一帧读 drop
外贴 RuntimePlaytest hook
强行抢旧 BattlePrepare Layout / Drag / Drop / Commit 链路
```

未来合入正式主线前，必须另开：

```text
V0.4-PromoteToMainline01
```

并且只能输出：

```text
PromoteCandidateDraft
```
