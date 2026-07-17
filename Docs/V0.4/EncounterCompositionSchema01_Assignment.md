# V0.4-EncounterCompositionSchema01 Assignment

Guard assignment:

```text
ENEMY_GUARD_ASSIGNMENT_ENCOUNTERCOMPOSITIONSCHEMA01
```

生成日期：2026-07-14  
所属：Enemy / Boss / Encounter System Guard  
包序号：E04

## 1. 包定位

本包建立可长期复用的 Encounter 组合纯数据 Schema，用来描述：

```text
一个 Encounter 使用哪些全局 MapRule
一个 Encounter 由哪些有序 Wave 组成
每个 Wave 包含哪些有序 EnemySlot / BossSlot
每个 Slot 选择该载体已经拥有的哪些 MechanicProfile
哪些 Slot 是普通、精英或 Boss 角色
哪些标签只属于开发章节与诊断层
```

本包只是“组合说明书”，不是 Encounter Runner、刷怪器、战斗控制器或章节流程。

“机制题”“验证题”仍是形容 Build 压力的内部说法，不是真实题库、选择题、问卷或答案面板。

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
Docs/V0.4/EncounterCompositionSchema01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs
Docs/V0.4/Reports/EnemyValidationCarrierInventory.csv
Docs/V0.4/Reports/EnemyValidationMechanicInventory.csv
Docs/V0.4/Reports/EnemyCarrierMechanicBinding.csv
Docs/V0.4/Reports/EnemyMapRuleMechanicBinding.csv
Docs/V0.4/Reports/EnemyValidationNormalizationExceptions.csv
```

## 3. 事实源与依赖方向

必须维持单一事实源：

```text
E01 EncounterReference
  = Encounter 身份与 devOnly / isEnabled / entersFormalFlow 权威

E03 Normalized Enemy / Boss / MechanicProfile / MapRule
  = 可引用 ID 的权威

E03 CarrierMechanicBinding
  = Carrier 可选择 MechanicProfile 的权威

E04 EncounterComposition
  = 只保存对上述稳定 ID 的组合选择与波次顺序
```

E04 不得复制 Enemy、Boss、MechanicProfile 或 MapRule 的显示名、机制字段、压力字段、BuildCapability、LegacyFacts、MappingTrace 或完整对象。

允许依赖方向：

```text
E04 Composition runtime
  -> E01 Domain / Contracts
  -> E03 Normalization 的公开只读类型或最小只读解析接口

E04 Editor Verifier
  -> 调用现有 EnemyValidationContentNormalizer.CreateSnapshot()
  -> 只读构造 E04 reference resolver
```

E04 不得直接引用 `TalismanBag.BuildSandbox`，不得新建第二个 legacy reader。现有 E03 Editor-only legacy reader 总数必须继续为 1。

## 4. 必须建立的纯数据结构

在独立新目录建立纯 C#：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/**
```

至少包含等价语义：

```text
EncounterCompositionSchema
EncounterCompositionCatalogInput
EncounterCompositionCatalogSnapshot
EncounterCompositionSnapshot
EncounterWaveSnapshot
EncounterSlotSnapshot
EncounterSlotKind
EncounterSlotRole
IEncounterCompositionReferenceResolver
IEncounterCompositionLookup
IEncounterCompositionProvider
EncounterCompositionValidationIssue
DefaultEncounterCompositionValidator
DefaultEncounterCompositionProvider
```

类名可以按既有风格微调，但职责不得缺失或扩张到 Runtime 执行。

## 5. 字段语义

Catalog 至少包含：

```text
schemaId = EncounterComposition.v1
schemaVersion = 1
encounters
canonicalSignature
```

Encounter 至少包含：

```text
EncounterReference
MapRuleIds
Waves
DeveloperContentTagIds
```

Wave 至少包含：

```text
WaveId
WaveOrder
Slots
```

Slot 至少包含：

```text
SlotId
SpawnOrder
EncounterSlotKind: Enemy / Boss
EncounterSlotRole: Normal / Elite / Boss
CarrierId
Quantity
MechanicProfileIds
```

固定语义：

```text
MapRuleIds 作用于整个 Encounter
MechanicProfileIds 是该 Slot 从既有 CarrierMechanicBinding 中选择的机制引用
WaveOrder / SpawnOrder 只表示确定顺序，不表示时间、动画或执行逻辑
Quantity 只表示该 Slot 的载体数量，不包含生命、伤害或战斗数值
DeveloperContentTagIds 只能进入开发者数据面板或诊断层
```

本包不创建 player-facing projection。Schema 中存在字段不代表允许直接显示给玩家，更不代表允许显示未来波次、完整阵容或机制答案。

## 6. 组合规则

必须支持：

```text
同一 Enemy / Boss Carrier 被多个 Encounter 复用
同一 MechanicProfile 被多个合法 Carrier 组合使用
同一 Encounter 包含多个 Wave
同一 Wave 包含多个 Slot
EnemySlot 标记 Normal 或 Elite
BossSlot 标记 Boss
同一 Encounter 引用 0 个或多个 MapRule
一个合法 Carrier 选择 1 个或多个已绑定 MechanicProfile
```

不得擅自限制：

```text
Boss 必须只出现在最后一波
一个 Encounter 最多只能有一个 Boss
Boss 与普通敌人不能同波
每个 Carrier 永远只能有一个 MechanicProfile
每个 MechanicProfile 永远只能属于一个 Carrier
```

这些属于未来内容规则或运行策略，不属于 E04 基础 Schema。

## 7. Reference Resolver 约定

E04 Validator 必须通过只读 resolver 验证引用，不得硬编码 E03 清单。Resolver 至少能回答：

```text
Enemy ID 是否存在
Boss ID 是否存在
MapRule ID 是否存在
MechanicProfile ID 及其 Enemy/Boss kind 是否存在
Carrier -> MechanicProfile binding 是否存在
Carrier 是否属于 E03 明确记录的允许无机制例外
```

规则：

```text
EnemySlot 只能引用 Enemy carrier
BossSlot 只能引用 Boss carrier
EnemySlot 不能标记 Boss role
BossSlot 必须标记 Boss role
每个 MechanicProfileId 必须解析成功
每个 Carrier -> MechanicProfile 组合必须存在于 E03 binding
空 MechanicProfileIds 只允许用于 E03 明确记录的 intentional carrier exception
每个 MapRuleId 必须解析成功
```

Resolver 只是只读适配层，不得成为第二份持久化 Enemy/Mechanic/MapRule 数据表。

## 8. 校验规则

必须拒绝：

```text
空 ID
ID 外侧空白
大小写替代或隐式 Trim
重复 EncounterId
同一 Encounter 内重复 WaveId
同一 Encounter 内重复 SlotId
重复 WaveOrder
同一 Wave 内重复 SpawnOrder
不连续或负数 WaveOrder
不连续或负数 SpawnOrder
空 Encounter wave 列表
空 Wave slot 列表
Quantity < 1
重复 MapRuleId
同一 Slot 内重复 MechanicProfileId
未解析 Enemy / Boss / MapRule / MechanicProfile 引用
不存在的 Carrier -> MechanicProfile binding
Slot kind 与 role 不一致
devOnly = false
isEnabled = true
entersFormalFlow = true
schemaId / schemaVersion 不匹配
```

所有 ID 比较必须使用 `StringComparer.Ordinal`。输入与输出集合必须防御性复制并以只读接口暴露。

## 9. Canonical Signature

必须提供完整确定性签名：

```text
sha256: + 64 lowercase hex
```

完整签名必须包含所有 Encounter、MapRule 引用、Wave、Slot、显式顺序、Quantity、MechanicProfile 引用、开发标签及隔离标记。

必须满足：

```text
相同内容重复生成，签名相同
仅改变输入集合排列，签名相同
改变 WaveOrder，签名变化
改变 SpawnOrder，签名变化
改变 CarrierId、Quantity、MapRuleId 或 MechanicProfileId，签名变化
改变 DeveloperContentTagIds，签名变化
```

输入数组顺序不是语义；显式 `WaveOrder` 与 `SpawnOrder` 才是顺序事实源。

## 10. Verifier fixture

Verifier 必须使用 E03 当前只读快照解析引用，并构造只存在于测试方法内的 synthetic fixture：

```text
至少 2 个 Encounter
至少 3 个 Wave
至少 1 个 Enemy carrier 被两个 Encounter 复用
至少 2 个不同 Enemy carrier 选择同一个合法 MechanicProfile
至少 1 个 Elite slot
至少 1 个 Boss slot
至少 1 个 MapRule 引用
```

建议使用 E03 已存在的共享绑定验证多对多能力：

```text
dev_enemy_poison_cultist
dev_enemy_burning_wisp
  -> dev_enemy_poison_burn_problem
```

fixture ID 使用通用测试 ID，例如 `dev_encounter_fixture_alpha`，不得把 `3-10`、`4-10` 或其他章节 ID 硬编码进 E04 runtime 或 verifier。

fixture 不是正式内容、不是 E10 SeedData，不得保存为 Config、Asset 或运行时默认章节表。

## 11. 允许新增文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/EncounterCompositionValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EncounterCompositionSchemaVerifier.cs.meta
Docs/V0.4/Reports/EncounterCompositionSchemaReport.md
Docs/V0.4/Reports/EncounterCompositionSchemaSpec.csv
Docs/V0.4/Reports/EncounterCompositionSchemaFieldMatrix.csv
Docs/V0.4/Reports/EncounterCompositionSchemaFixtureRows.csv
Docs/V0.4/Reports/EncounterCompositionSchemaLeakCheckReport.md
```

预期新增文件：14。  
允许修改已有文件：0。

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 12. 受保护文件

以下源码必须保持 byte-identical：

```text
E01 Domain / Contracts / Verifier: 6
E02 Vocabulary / Verifier: 4
E03 Normalization / Editor adapter / Verifier: 4
合计: 14/14 unchanged
```

以下 Legacy source 必须保持：

```text
EnemyBossValidationPool.cs
BuildProblemRuleConfigs.cs
BuildProblemSeedData.cs
合计: 3/3 unchanged
```

开发前记录 hash，开发后复核。E03 当前基线还必须保持：

```text
Carriers: 11 / 7 / 18
Mechanic profiles: 10 / 6 / 16
MapRules: 10
Carrier bindings: 17
MapRule bindings: 30
Intentional exceptions: 2
Unresolved / unexplained orphan: 0
```

## 13. 绝对禁止

```text
不修改任何已有文件
不修改 Scene / Prefab / Config
不新增 MonoBehaviour / ScriptableObject
不引用 GameObject、Transform、UnityEngine 场景对象或 Addressables
不生成或控制敌人对象
不加载场景
不开始、暂停或结算战斗
不实现刷怪计时器、波次状态机或目标选择
不实现 SkillPattern / BossPhase，留给 E05
不实现 CounterWindow / Pressure runtime，留给 E06
不读取 Item 或 BuildSnapshot，留给 E07 之后的只读 Contract
不实现 BuildReadiness，留给 E08
不实现 3-10 / 4-10 SeedData，留给 E10
不新增完整答案、requiredCapability、weakness threshold、DropBias 或奖励字段
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

## 14. Verifier 最低验收

至少验证：

```text
Schema ID / version 精确
所有顶层与嵌套集合不可变
Catalog / Encounter / Wave / Slot lookup 为 Ordinal 严格匹配
所有第 8 节非法输入均被明确错误码拒绝
所有 E03 引用与 CarrierMechanicBinding 可解析
同一 Carrier 跨 Encounter 复用有效
同一 MechanicProfile 跨合法 Carrier 复用有效
多 Wave / 多 Slot 有效
Elite / Boss role 规则有效
Canonical repeat / input-order independence / content sensitivity 有效
E04 runtime BuildSandbox references = 0
E04 direct Battle / Board / Item / RunFlow / Reward / SaveData references = 0
E04 MonoBehaviour / ScriptableObject / scene object references = 0
章节硬编码 = 0
答案、Build 要求、DropBias 泄漏 = 0
E01 + E02 + E03 protected hash = 14/14 unchanged
Legacy source hash = 3/3 unchanged
E03 基线数量不变
Scene / Prefab / Config / Battle / Board / Item modifications = 0
package path whitelist PASS
GUID 冲突 = 0
尾随空白 = 0
git diff --check PASS
```

Verifier 必须让 Unity batch 与同源离线入口调用同一套检查逻辑，不得维护两份断言。

## 15. 报告要求

`EncounterCompositionSchemaFieldMatrix.csv` 至少包含：

```text
ownerType
fieldName
fieldType
cardinality
semantic
visibility
sourceOfTruth
```

`EncounterCompositionSchemaFixtureRows.csv` 至少包含：

```text
encounterId
waveId
waveOrder
slotId
spawnOrder
slotKind
slotRole
carrierId
quantity
mechanicProfileIds
mapRuleIds
fixtureOnly
```

Leak 报告必须明确列出所有禁止依赖与扫描结果，不得只写总数。

## 16. 自动验证

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

## 17. 完成回报格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EncounterCompositionSchema01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 14
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 必须为 0 / 0 / 0 / 0 / 0 / 0
E01 + E02 + E03 protected hash: 14/14 unchanged
Legacy source hash: 3/3 unchanged
Fixture: Encounter / Wave / Slot 数量
Carrier 跨 Encounter 复用: PASS / FAIL
MechanicProfile 跨 Carrier 复用: PASS / FAIL
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

## 18. 通过后

下一候选包：

```text
V0.4-EnemySkillBossPhaseSchema01
```

E05 不得自动启动。
