# V0.4-CounterWindowAndPressureSchema01 Assignment

Guard assignment:

```text
ENEMY_GUARD_ASSIGNMENT_COUNTERWINDOWANDPRESSURESCHEMA01
```

生成日期：2026-07-14  
所属：Enemy / Boss / Encounter System Guard  
包序号：E06

## 1. 包定位

本包建立长期可复用的 BuildPressure 与 CounterWindow 纯数据 Schema，用来描述：

```text
一个机制、地图规则、技能或 Boss 阶段产生哪些抽象压力
多个压力通道如何组成一个 BuildPressureProfile
一个压力允许哪些 Required / Recommended Build 能力组合
能力要求如何通过 Any / All 组表达多种解法
一个 CounterWindow 的公开反馈键、内部开启条件、结束条件和最大持续时间
Pressure、CounterWindow 与既有 E03/E05 来源之间的多对多绑定
玩家安全投影、内部规格和开发者完整诊断如何隔离
```

本包不是战斗窗口执行器、计时器、事件监听器、能力读取器、BuildReadiness Evaluator、Boss 状态机或反馈 Controller。

“机制题”“验证题”只是“玩家 Build 面对某类压力是否具备应对能力”的内部形容词。不得建立真实题库、选择题、问卷、答题 UI 或唯一标准答案。

## 2. 启动必读

开发前必须完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UnifiedBattlePageStrategy_GuardSync.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/CounterWindowAndPressureSchema01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs
Docs/V0.4/Reports/EnemyValidationMechanicInventory.csv
Docs/V0.4/Reports/EnemyMapRuleMechanicBinding.csv
Docs/V0.4/Reports/EnemySkillBossPhaseSchemaFixtureRows.csv
```

旧 `BuildProblemRuleConfigs.cs` 与 `BuildProblemSeedData.cs` 只允许用于受保护 hash 复核。E06 不得建立第二个 legacy reader，也不得把旧 WeaknessWindow 复制成新的运行内容。

## 3. 事实源与依赖方向

必须维持：

```text
E01 CounterWindowReference
  = CounterWindow 身份与隔离标记权威

E02 PressureChannel / CounterWindowType / BuildCapability
  = 压力、窗口类型与抽象 Build 能力稳定键权威

E02 PlayerHintCategory / DeveloperDiagnosticCategory
  = 玩家提示类别与开发者诊断类别稳定键权威

E03 MechanicProfile / MapRule 与归一化映射
  = 旧验证内容中机制、压力、窗口、能力映射的兼容权威

E05 SkillPattern / BossPhase
  = 技能与 Boss 阶段身份、内部规格和公开提示的权威

E06 BuildPressureProfile / CounterWindowProfile / bindings
  = 压力组合、能力要求组、窗口条件及其多对多关系的权威
```

E06 不得复制 E02 词汇清单，不得复制 E03 Carrier/Mechanic/MapRule 内容，不得复制 E05 Skill/Phase 内容。E06 只保存稳定键和稳定 ID 引用。

允许依赖方向：

```text
E06 PressureWindow runtime
  -> E01 Domain / Contracts
  -> E02 Vocabulary 只读键类型或 lookup
  -> E03 Normalization 只读 lookup
  -> E05 SkillPhase 只读 lookup

E06 Editor Verifier
  -> 调用现有 EnemyValidationContentNormalizer.CreateSnapshot()
  -> 调用 E02 默认 Vocabulary Provider
  -> 使用 E05 公共 Schema 构造仅存在于测试方法内的 synthetic fixture
  -> 构造瞬时只读 resolver
```

E06 runtime 与新 Verifier 不得直接引用 `TalismanBag.BuildSandbox`。现有 E03 Editor-only legacy reader 总数必须继续为 1。

## 4. 三层字段边界

### 4.1 Player-safe projection

只允许：

```text
BuildPressureProfileId
PublicPressureLabelKey
PublicPressureHintKey
PlayerHintCategoryKeys
CounterWindowId
PublicWindowLabelKey
PublicWindowOpenCueKey
PublicWindowCloseCueKey
```

这些字段只是未来 BattleContract 可按“当前已观察到的压力 / 当前已开启的窗口”选择性映射的公开元数据。不得把完整 Pressure 或 CounterWindow Catalog 一次性显示给玩家。

### 4.2 Internal-only spec

允许保存但不得直接作为玩家对象序列化：

```text
PressureChannelContribution
PressureChannelKey
WeightBasisPoints
CounterWindowTypeKey
CounterWindowOpenConditions
CounterWindowCloseConditions
ConditionMatchMode
MaximumDurationMilliseconds
PressureSourceBinding
CounterWindowSourceBinding
PressureCounterWindowBinding
```

这些字段只描述规格。本包不读取战斗事件、不消费时间、不检测窗口条件，也不执行窗口开启或关闭。

### 4.3 Developer-only diagnostics

只能留在开发者诊断层：

```text
BuildCapabilityRequirementGroup
BuildCapabilityRequirement
RequirementRole
RequirementMatchMode
MinimumCapabilityBasisPoints
DeveloperDiagnosticCategoryKeys
SourceReferenceIds
完整 CanonicalSignature
完整绑定矩阵、阈值和验证错误
```

BuildCapability 稳定键是抽象能力，不是 Item ID、核心道具 ID、Affix ID、Synergy ID 或棋盘对象引用。

## 5. 必须建立的纯数据结构

在独立新目录建立：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/**
```

至少包含等价语义：

```text
CounterWindowAndPressureSchema
PressureSourceKind
CapabilityRequirementRole
RequirementMatchMode
CounterWindowConditionKind
BuildPressurePlayerProjection
PressureChannelContributionSnapshot
BuildCapabilityRequirementSnapshot
BuildCapabilityRequirementGroupSnapshot
BuildPressureInternalSpec
BuildPressureDeveloperDiagnostics
BuildPressureProfileSnapshot
CounterWindowPlayerProjection
CounterWindowConditionSnapshot
CounterWindowInternalSpec
CounterWindowDeveloperDiagnostics
CounterWindowProfileSnapshot
PressureSourceBindingSnapshot
CounterWindowSourceBindingSnapshot
PressureCounterWindowBindingSnapshot
CounterWindowAndPressureCatalogInput
CounterWindowAndPressureCatalogSnapshot
ICounterWindowAndPressureReferenceResolver
ICounterWindowAndPressureLookup
ICounterWindowAndPressureProvider
ICounterWindowAndPressureValidator
CounterWindowAndPressureValidationIssue
DefaultCounterWindowAndPressureProvider
DefaultCounterWindowAndPressureValidator
```

类名可按既有风格微调，但不得扩张为 Runtime 执行、事件订阅或状态持有组件。

## 6. Catalog 与 BuildPressureProfile

Catalog 至少包含：

```text
schemaId = CounterWindowAndPressure.v1
schemaVersion = 1
buildPressureProfiles
counterWindowProfiles
pressureSourceBindings
counterWindowSourceBindings
pressureCounterWindowBindings
canonicalSignature
playerSafeCanonicalSignature
```

BuildPressureProfile 至少包含：

```text
BuildPressureProfileId
PlayerSafe
InternalOnly
DeveloperOnly
devOnly = true
isEnabled = false
entersFormalFlow = false
```

Internal-only PressureChannelContribution 至少包含：

```text
PressureChannelKey
WeightBasisPoints
```

规则：

```text
每个 BuildPressureProfile 至少有 1 个 PressureChannelContribution
同一 Profile 内 PressureChannelKey 唯一
PressureChannelKey 必须解析到 E02 PressureChannel
WeightBasisPoints 为 1..10000
同一 Profile 的 WeightBasisPoints 总和必须精确等于 10000
权重只表示压力构成，不是伤害倍率、掉落权重、随机概率或 DropBias
```

## 7. BuildCapabilityRequirement 多解结构

Developer-only RequirementGroup 至少包含：

```text
RequirementGroupId
RequirementRole: Required / Recommended
RequirementMatchMode: Any / All
Requirements
```

每个 Requirement 至少包含：

```text
BuildCapabilityKey
MinimumCapabilityBasisPoints
```

语义约定：

```text
Required group 表示未来 E08 Readiness 必须评估的能力组
Recommended group 表示非硬阻断的辅助能力组
Any 表示组内任一抽象能力达到阈值即可
All 表示组内全部抽象能力都要达到各自阈值
多个 Required group 之间按 All 组合
```

该结构必须能表达：

```text
破盾能力 OR 雷链能力
净化能力 AND 负面状态反制
持续输出为 Required，爆发窗口为 Recommended
同一压力存在多种合法 Build 解法
```

本包只保存要求，不读取或比较玩家 Build。`MinimumCapabilityBasisPoints` 为 1..10000 的开发者规格，E07 后续只读能力值必须使用兼容尺度；E06 不得提前定义 E07 BuildSnapshot 或实现比较。

禁止把 RequirementGroup、BuildCapabilityKey、MinimumCapabilityBasisPoints 或完整解法投影给玩家。

## 8. CounterWindowProfile

CounterWindowProfile 至少包含：

```text
CounterWindowReference
PlayerSafe
InternalOnly
DeveloperOnly
```

Internal-only 至少包含：

```text
CounterWindowTypeKey
OpenConditionMatchMode
OpenConditions
CloseConditionMatchMode
CloseConditions
MaximumDurationMilliseconds
```

Condition kind 至少支持：

```text
MechanicSignal
SkillPatternStarted
SkillPatternCompleted
SkillPatternInterrupted
BossPhaseEntered
BossPhaseExited
```

Condition 至少包含：

```text
ConditionId
Kind
ReferenceId
```

字段规则：

```text
MechanicSignal: ReferenceId 为非空稳定信号 ID
SkillPattern*: ReferenceId 必须解析到 E05 SkillPattern
BossPhase*: ReferenceId 必须解析到 E05 BossPhase
每个窗口至少有 1 个 OpenCondition
CloseConditions 可为空，但此时 MaximumDurationMilliseconds 必须 > 0
MaximumDurationMilliseconds >= 0
MaximumDurationMilliseconds = 0 表示没有计时自动关闭，必须存在 CloseCondition
存在 MaximumDurationMilliseconds 与 CloseConditions 时表示未来由先满足者关闭；E06 不执行该规则
```

本包不得擅自规定所有窗口都必须定时、所有窗口只有一个条件、所有 Boss 共用固定窗口时长，或 6 种 CounterWindowType 只能各有一个 Profile。

## 9. 多对多 Binding

PressureSourceBinding 与 CounterWindowSourceBinding 的 SourceKind 至少支持：

```text
MechanicProfile
MapRule
SkillPattern
BossPhase
```

解析规则：

```text
MechanicProfile / MapRule -> E03 lookup
SkillPattern / BossPhase -> E05 lookup
```

PressureCounterWindowBinding 至少包含：

```text
BuildPressureProfileId
CounterWindowId
```

关系必须允许：

```text
一个来源绑定多个 PressureProfile
一个 PressureProfile 被多个来源复用
一个来源绑定多个 CounterWindow
一个 CounterWindow 被多个来源复用
一个 PressureProfile 对应多个 CounterWindow
一个 CounterWindow 服务多个 PressureProfile
```

不得强制 Pressure、Window、MechanicProfile、SkillPattern 或 BossPhase 一对一。不得在 Binding 中复制来源对象的内容。

E06 不直接引用 E04 Encounter/Wave/Slot：Encounter 已通过 E04 组合 E03 MapRule 与 MechanicProfile，避免建立第二条 Encounter 压力事实源。

## 10. Reference Resolver

Resolver 至少能回答：

```text
PressureChannel key 是否来自 E02 对应类别
CounterWindowType key 是否来自 E02 对应类别
BuildCapability key 是否来自 E02 对应类别
PlayerHintCategory key 是否来自 E02 对应类别
DeveloperDiagnosticCategory key 是否来自 E02 对应类别
MechanicProfile / MapRule ID 是否存在于 E03
SkillPattern / BossPhase ID 是否存在于 E05
```

Resolver 只能包装 E02/E03/E05 只读快照，不得持久化第二份词汇、机制、地图、技能或阶段清单。

## 11. Player-safe 隔离

`PlayerSafeCanonicalSignature` 必须包含 `schemaId / schemaVersion`；除此之外，业务内容只允许来自 BuildPressure 与 CounterWindow 的 Player-safe projection。

禁止进入 player-safe payload：

```text
PressureChannelKey / WeightBasisPoints
CounterWindowTypeKey
Open / Close Condition kind、ReferenceId、MatchMode
MaximumDurationMilliseconds
所有 SourceBinding 与 PressureCounterWindowBinding
BuildCapabilityKey
RequirementGroupId / RequirementRole / RequirementMatchMode
MinimumCapabilityBasisPoints
DeveloperDiagnosticCategoryKeys
SourceReferenceIds
完整 Pressure / Window 关系
完整答案、Boss 六钥匙、DropBias、Readiness 判定细节
```

必须验证：

```text
修改 Internal-only 字段，完整签名变化，player-safe 签名不变
修改 Developer-only 字段，完整签名变化，player-safe 签名不变
修改 Binding，完整签名变化，player-safe 签名不变
修改 Player-safe 字段，两个签名都变化
```

## 12. Canonical Signature

完整与 player-safe 签名都必须为：

```text
sha256: + 64 lowercase hex
```

完整签名必须覆盖全部字段、条件、要求组、阈值、绑定和隔离标记。

必须满足：

```text
相同内容重复生成，签名相同
仅改变输入集合排列，签名相同
改变 Pressure weight、Requirement threshold、Condition、duration 或 Binding，完整签名变化
改变 public label / hint / cue，两个签名都变化
```

输入数组顺序不是语义。ConditionId、RequirementGroupId、稳定键与绑定复合键才是 Canonical 排序事实源。

## 13. Verifier fixture

Verifier 使用 E02/E03 当前只读快照，并以 E05 公共 Schema 构造仅存在于测试方法内的 synthetic Skill/Phase fixture，再构造：

```text
4 个 BuildPressureProfile
3 个 CounterWindowProfile
6 个 PressureSourceBinding
4 个 CounterWindowSourceBinding
5 个 PressureCounterWindowBinding
至少 8 个 BuildCapabilityRequirement row
至少 1 个 Any Required group
至少 1 个 All Required group
至少 1 个 Recommended group
至少 1 个 PressureProfile 被多个来源复用
至少 1 个来源绑定多个 PressureProfile
至少 1 个 PressureProfile 对应多个 CounterWindow
至少 1 个 CounterWindow 服务多个 PressureProfile
至少覆盖 MechanicSignal、SkillPatternInterrupted、BossPhaseEntered
至少覆盖定时关闭、条件关闭、定时或条件先到关闭
```

fixture 使用通用 ID，例如：

```text
dev_pressure_fixture_shield
dev_pressure_fixture_cast
dev_window_fixture_shell_break
dev_window_fixture_interrupt
```

不得硬编码 `3-10`、`4-10` 或正式章节 ID。fixture 不是 E10 SeedData，不得保存为 Config、Asset 或默认运行内容。

## 14. 允许新增文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressurePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressurePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressureValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/CounterWindowAndPressureValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs.meta
Docs/V0.4/Reports/CounterWindowAndPressureSchemaReport.md
Docs/V0.4/Reports/CounterWindowAndPressureSchemaSpec.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchemaFieldMatrix.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchemaFixtureRows.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchemaLeakCheckReport.md
```

预期新增文件：16。  
允许修改已有文件：0。

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 15. 受保护文件

以下源码必须保持 byte-identical：

```text
E01 Domain / Contracts / Verifier: 6
E02 Vocabulary / Verifier: 4
E03 Normalization / Editor adapter / Verifier: 4
E04 Composition / Verifier: 4
E05 SkillPhase / Verifier: 5
合计: 23/23 unchanged
```

以下 Legacy source 必须保持：

```text
EnemyBossValidationPool.cs
BuildProblemRuleConfigs.cs
BuildProblemSeedData.cs
合计: 3/3 unchanged
```

既有基线必须保持：

```text
E02 Vocabulary: 63
E03 Carriers: 11 / 7 / 18
E03 Mechanic profiles: 10 / 6 / 16
E03 MapRules: 10
E03 Carrier bindings: 17
E03 MapRule bindings: 30
E03 Intentional exceptions: 2
E04 fixture: 2 Encounter / 4 Wave / 6 Slot
E05 fixture: 4 Pattern / 3 Sequence / 3 Binding / 3 Phase / 2 Plan
```

## 16. 绝对禁止

```text
不修改任何已有文件
不修改 Scene / Prefab / Config
不新增 MonoBehaviour / ScriptableObject
不引用 GameObject、Transform、UnityEngine 场景对象或 Addressables
不实现计时器、Update、Coroutine、async delay 或时间推进
不订阅战斗事件，不监听技能、Boss 阶段、HP、护盾或棋盘状态
不真正开启、维持或关闭 CounterWindow
不执行伤害、护盾、状态、召唤、控制、资源或目标选择
不修改现有施法条、Boss 反馈、浮字或 Controller
不读取 Item、核心道具、Affix、Synergy 或真实 BuildSnapshot
不实现 E07 BuildCapabilityReadContract
不实现 E08 BuildReadiness 或 CapabilityGap 计算
不实现 3-10 / 4-10 SeedData，留给 E10
不定义 Item ID 到 BuildCapability 的映射
不把 BuildCapability、RequirementGroup、阈值、窗口条件或完整关系给玩家
不建立唯一标准答案或固定一对一题目关系
不新增 Boss 六钥匙、DropBias、奖励或掉落权重
不修改 E01 / E02 / E03 / E04 / E05
不修改 BattleSandbox / BattleContract / Battle Bridge / UnifiedBattlePage
不修改棋盘、摆放、拖拽、旋转、托盘、输入与战斗手感
不修改 Item 系统
不修改正式 EnemyDefinition / EnemySkillDefinition / Enemy 资产
不修改 1-10 / 2-10
不修改 RunFlow / Reward / SaveData / Boss 正式流程
不接正式章节入口
不发正式奖励
不覆盖或回退工作区既有未提交改动
不 commit / tag / push
```

## 17. Validator 最低规则

必须拒绝：

```text
空 ID、ID 外侧空白、大小写替代、隐式 Trim
重复 BuildPressureProfileId / CounterWindowId
重复 SourceBinding 复合键或 PressureCounterWindowBinding 对
空 PressureChannelContribution 或总权重不等于 10000
重复或未知 PressureChannel key
空 RequirementGroup / 空 Requirement
重复 RequirementGroupId 或组内重复 BuildCapability key
未知 BuildCapability / PlayerHintCategory / DeveloperDiagnosticCategory key
MinimumCapabilityBasisPoints 不在 1..10000
未知 CounterWindowType key
空 OpenCondition
重复 ConditionId
Condition kind 与 ReferenceId 组合不合法
未解析 MechanicProfile / MapRule / SkillPattern / BossPhase 引用
MaximumDurationMilliseconds < 0
MaximumDurationMilliseconds = 0 且 CloseConditions 为空
Pressure / Window / Binding 引用无法解析
PlayerSafe ID 与 owner ID 不一致
devOnly = false / isEnabled = true / entersFormalFlow = true
schemaId / schemaVersion 不匹配
player-safe payload 包含内部、绑定或诊断字段
```

所有 ID 比较使用 `StringComparer.Ordinal`。所有输入与输出集合必须防御性复制并以只读接口暴露。

## 18. Verifier 最低验收

至少验证：

```text
Schema ID / version 精确
所有顶层与嵌套集合不可变
Pressure / Window lookup 为 Ordinal 严格匹配
第 17 节非法输入均被明确错误码拒绝
所有 E02/E03/E05 引用可解析
Pressure、Window 与来源多对多复用有效
Any / All / Required / Recommended 要求组结构有效
完整签名与 player-safe 签名格式、确定性和敏感性有效
Internal / Developer / Binding 变化不污染 player-safe 签名
Player-safe 变化同时改变两个签名
player-safe payload 无 Pressure 权重、窗口类型、条件、时长、能力答案、阈值、绑定和诊断泄漏
E06 runtime BuildSandbox references = 0
E06 direct Battle / Board / Item / RunFlow / Reward / SaveData references = 0
E06 timer / event subscription / state machine / scene object references = 0
Editor-only legacy reader total = 1
章节硬编码 = 0
Boss keys / DropBias / formal reward 泄漏 = 0
E01 + E02 + E03 + E04 + E05 protected hash = 23/23 unchanged
Legacy source hash = 3/3 unchanged
E02-E05 基线不变
Scene / Prefab / Config / Battle / Board / Item modifications = 0
package path whitelist PASS
GUID 冲突 = 0
尾随空白 = 0
git diff --check PASS
```

Unity batch 与同源离线入口必须调用同一套检查逻辑，不得维护两份断言。

## 19. 报告要求

`CounterWindowAndPressureSchemaFieldMatrix.csv` 至少包含：

```text
ownerType
fieldName
fieldType
cardinality
semantic
visibility
sourceOfTruth
futureConsumer
```

`CounterWindowAndPressureSchemaFixtureRows.csv` 至少包含：

```text
rowKind
ownerId
sourceKind
sourceId
referenceId
role
matchMode
valueBasisPoints
conditionKind
durationMilliseconds
playerVisible
fixtureOnly
```

主报告必须分别列出：

```text
BuildPressureProfile / CounterWindowProfile 数量
RequirementGroup / Requirement 数量
三类 Binding 数量
完整签名
player-safe 签名
多对多复用检查
要求组检查
隔离检查
```

Leak 报告必须逐项列出禁止依赖与扫描结果，不得只写总数。

## 20. 自动验证与完成回报

必须尝试：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
git diff --check
新增文件尾随空白检查
GUID 冲突检查
精确变更文件清单
```

如果同一工程被用户 Unity Editor 占用：

```text
不得关闭用户 Editor
不得结束 Unity 进程
不得声称 Unity Verifier PASS
记录 BLOCKED 原因
继续完成同源离线 Verifier
由 Guard 决定是否收口
```

本包无需场景手测。

完成回报格式：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-CounterWindowAndPressureSchema01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 16
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 必须为 0 / 0 / 0 / 0 / 0 / 0
E01 + E02 + E03 + E04 + E05 protected hash: 23/23 unchanged
Legacy source hash: 3/3 unchanged
Fixture: PressureProfile / CounterWindow / PressureSourceBinding / CounterWindowSourceBinding / PressureWindowBinding / Requirement 数量
Pressure reuse: PASS / FAIL
CounterWindow reuse: PASS / FAIL
Any-All requirement groups: PASS / FAIL
Player-safe isolation: PASS / FAIL
Unresolved references: 必须为 0
Verifier: 通过数 / 总数
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
Leak: 必须为 0
GUID 冲突: 必须为 0
尾随空白: 必须为 0
git diff --check: PASS / FAIL
未 commit / tag / push
```

## 21. 通过后

下一候选包：

```text
V0.4-BuildCapabilityReadContract01
```

E07 不得自动启动。
