# V0.4-EnemyDataValidatorAndSnapshot01 Assignment

Guard assignment:

```text
ENEMY_GUARD_ASSIGNMENT_ENEMYDATAVALIDATORANDSNAPSHOT01
```

生成日期：2026-07-15  
所属：Enemy / Boss / Encounter System Guard  
包序号：E09

## 1. 包定位

本包建立 Enemy 长期数据底座的统一校验门与不可变根快照：

```text
E01 Domain
+ E02 Vocabulary
+ E03 Normalized Validation Content
+ E04 Encounter Composition
+ E05 Skill / Boss Phase
+ E06 Pressure / CounterWindow
-> EnemySystemSnapshot.v1
+ EnemySystemPlayerSafeSnapshot
+ Cross-catalog Validation / Identity / Relation Diagnostics
```

E07 `BuildCapabilitySnapshot` 与 E08 `EnemyReadinessEvaluationResult` 是每套 Build、每次离线评估的瞬时数据，不是静态 Enemy 内容：

```text
E07 / E08 只登记在八组件 Schema Manifest
E07 / E08 不嵌入 EnemySystemSnapshot
E07 / E08 不参与静态内容 CanonicalSignature
E09 Verifier 只验证它们与 E06 的合同兼容性
```

本包不创建敌人、Boss、地图规则、章节或机制题内容；不执行 Encounter、技能、阶段、CounterWindow 或战斗；不接 Item、BattleContract、Scene、正式章节、奖励或存档。

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
Docs/V0.4/EnemyDataValidatorAndSnapshot01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/**
Docs/V0.4/Reports/Enemy*Schema*.csv
Docs/V0.4/Reports/Enemy*Contract*.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchema*.csv
Docs/V0.4/Reports/BuildCapabilityReadContract*.csv
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluator*.csv
```

旧 BuildProblem、Item 与 BuildSandbox 文件只允许用于禁止依赖、受保护 hash 和 legacy-reader 总数扫描，不得成为 E09 运行时数据源。

## 3. 事实源与依赖方向

必须维持：

```text
E01 = Enemy/Boss 与抽象引用类型权威
E02 = 机制、能力、压力、窗口与提示稳定键权威
E03 = 11/7 载体、10/6 机制、10 MapRule 与多对多绑定权威
E04 = Encounter / Wave / Slot 编排权威
E05 = SkillPattern / Sequence / CarrierBinding / BossPhase / Plan 权威
E06 = BuildPressure / CounterWindow / RequirementGroup / source binding 权威
E07 = 外部可构造 BuildCapabilitySnapshot 合同权威
E08 = 瞬时 Readiness Evaluation 与 player hint severity 权威
E09 = 跨 Catalog 关系校验、统一根快照、统一签名与安全投影权威
```

E09 不得复制任何组件内容、ID 表、Validator 规则或默认 fixture 数据到 Runtime。E09 只消费 Provider 已产出的不可变 Snapshot，并补充跨组件不变量；不得重建 E01-E08 各自已有的内部 Validator。

依赖方向：

```text
E09 SystemSnapshot runtime
  -> System.*
  -> E01-E08 public schema / snapshot / projection types

E09 Editor Verifier
  -> E02 default vocabulary provider
  -> E03 Editor normalizer public snapshot entry
  -> E04-E08 public providers / validators
  -> synthetic cross-catalog fixture local to verifier methods
```

E09 runtime 不得引用 Editor、BuildSandbox、Item、Battle、UnityEngine 或文件系统。

## 4. 静态根快照输入

`EnemySystemSnapshotInput` 必须只包含六个静态组件：

```text
EnemyDomainSnapshot
EnemyMechanicVocabularySnapshot
EnemyValidationContentSnapshot
EncounterCompositionCatalogSnapshot
EnemySkillBossPhaseCatalogSnapshot
CounterWindowAndPressureCatalogSnapshot
```

规则：

```text
六项均不得为 null
六项必须来自各自现有 Provider / Normalizer
输入与嵌套集合只读，E09 不得修改
E03.DomainSnapshot 必须与传入 E01 Domain 的 CanonicalSignature 完全一致
E07 BuildCapabilitySnapshot 不得成为输入字段
E08 EnemyReadinessEvaluationResult 不得成为输入字段
```

E07/E08 只通过 Schema Manifest 声明当前兼容合同，不得把某个 synthetic Build 或 Readiness 结果固化为 Enemy 静态事实。

## 5. Schema Manifest

统一 Manifest 必须固定包含八项：

```text
E01 EnemyDomainContract.v1          StaticCatalog / Embedded
E02 EnemyMechanicVocabulary.v1      StaticCatalog / Embedded
E03 EnemyValidationContentSnapshot.v1 StaticCatalog / Embedded
E04 EncounterComposition.v1         StaticCatalog / Embedded
E05 EnemySkillBossPhase.v1          StaticCatalog / Embedded
E06 CounterWindowAndPressure.v1      StaticCatalog / Embedded
E07 BuildCapabilityReadContract.v1   TransientContract / NotEmbedded
E08 EnemyOfflineReadiness.v1         TransientContract / NotEmbedded
```

`EnemySystemSchemaManifestEntry` 至少包含等价语义：

```text
ComponentId
SchemaId
SchemaVersion
ComponentMode: StaticCatalog / TransientContract
EmbeddedInRoot
ComponentCanonicalSignature
PlayerSafeCanonicalSignature
ContractCanonicalSignature
```

要求：

```text
Manifest 恰好 8 项，按 ComponentId Ordinal 排序
静态六项 EmbeddedInRoot=true
E07/E08 EmbeddedInRoot=false
ContractCanonicalSignature 为 schemaId/version/mode/embedded 的确定性 sha256
静态组件 Full/PlayerSafe signature 使用现有组件权威结果
没有 player-safe signature 的组件使用空值，不得伪造第二份玩家投影
```

## 6. EnemySystemSnapshot.v1

必须建立：

```text
SchemaId: EnemySystemSnapshot.v1
SchemaVersion: 1
```

`EnemySystemSnapshot` 至少包含：

```text
EnemyDomainSnapshot
EnemyMechanicVocabularySnapshot
EnemyValidationContentSnapshot
EncounterCompositionCatalogSnapshot
EnemySkillBossPhaseCatalogSnapshot
CounterWindowAndPressureCatalogSnapshot
SchemaManifest
IdentityIndex
RelationIndex
PlayerSafe
CanonicalSignature
PlayerSafeCanonicalSignature
DevOnly = true
IsEnabled = false
EntersFormalFlow = false
```

全部集合必须防御性复制并只读暴露。已有组件 Snapshot 已不可变时允许保留其只读引用，不得为了复制而重新定义第二套类型或内容。

根快照不得包含：

```text
BuildCapabilitySnapshot
EnemyReadinessEvaluationResult
DeveloperReadinessSnapshot
MapRuleCapabilityAdjustmentSnapshot
Item / Affix / Synergy / Board / Placement 实例
HP / shield / cast progress / current phase 等运行时可变状态
BattleResult / Reward / Drop / SaveData / ChapterProgress
MonoBehaviour / ScriptableObject / GameObject / Asset 路径
```

## 7. Identity Index

`EnemySystemIdentitySnapshot` 至少包含：

```text
IdentityKind
StableId
OwningComponentId
DevOnly
IsEnabled
EntersFormalFlow
DeveloperOnly = true
```

IdentityKind 至少覆盖：

```text
Enemy
Boss
MechanicProfile
MapRule
Encounter
SkillPattern
SkillSequence
BossPhase
BossPhasePlan
BuildPressureProfile
CounterWindow
```

唯一性规则：

```text
同一 IdentityKind + StableId 必须 Ordinal 唯一
Enemy 与 Boss 共用 Carrier 命名空间，不得同 ID 跨类型冲突
其他不同 IdentityKind 可以使用相同文本 ID，不得错误强制全局一刀切唯一
空 ID、外侧空白 ID 均拒绝
```

E09 不得把 CSV 展开行误当成逻辑实体数；所有报告 Actual 与 PASS 判定必须共用同一逻辑计数结果。

## 8. Relation Index 与跨组件引用

`EnemySystemRelationSnapshot` 至少包含：

```text
SourceKind
SourceId
RelationKind
TargetKind
TargetId
OwningComponentId
Resolved
IsolationCompatible
DeveloperOnly = true
```

至少索引并验证：

```text
E01 Enemy/Boss -> MechanicProfile / SkillPattern / BossPhase reference
E01 可选 reference registry -> E03/E04/E05/E06 已存在目标
E03 Carrier -> MechanicProfile
E03 MapRule -> MechanicProfile
E04 Encounter -> MapRule
E04 Slot -> Enemy/Boss carrier
E04 Slot -> selected MechanicProfile
E05 SkillPattern -> MechanicProfile
E05 Sequence -> SkillPattern
E05 CarrierSkillBinding -> Sequence
E05 BossPhase -> Sequence
E05 BossPlan -> BossPhase
E06 PressureSource -> MechanicProfile / MapRule / SkillPattern / BossPhase
E06 CounterWindowSource -> source target
E06 Pressure -> CounterWindow
```

规则：

```text
所有已声明引用必须 Ordinal 解析
每个关系 identity 必须唯一
引用两端 devOnly/isEnabled/entersFormalFlow 必须兼容
E01 optional registry 中存在的引用必须解析
后置 Catalog 条目不强制必须回填到 E01 optional registry
未被 Encounter 使用的合法 Catalog 条目允许存在
E03 intentional exceptions 继续允许
不得把未引用条目自动判为 orphan
```

## 9. 多对多关系硬规则

E09 必须明确接受：

```text
一个 Enemy/Boss 绑定多个 MechanicProfile
一个 MechanicProfile 被多个 Enemy/Boss 复用
一个 MapRule 绑定多个 MechanicProfile
一个 MechanicProfile 被多个 MapRule 复用
一个 PressureProfile 被多个 source 使用
一个 CounterWindow 被多个 PressureProfile 复用
```

禁止：

```text
把 11/7 载体与 10/6 机制强制一对一
把 CarrierId 或章节号作为 MechanicProfile 主键
因一个目标有多个 source 就误报重复
因一个合法 Catalog 条目暂未被 Encounter 使用就误报 orphan
```

Verifier 必须包含 many-to-many 正例，证明上述复用不会被 Validator 拒绝。

## 10. Player-safe 根投影

`EnemySystemPlayerSafeSnapshot` 只允许聚合现有 player-safe 类型：

```text
SchemaId
SchemaVersion
Enemies: EnemyValidationPlayerProjection[]
Bosses: EnemyValidationPlayerProjection[]
MechanicProfiles: EnemyValidationPlayerProjection[]
MapRules: EnemyValidationPlayerProjection[]
SkillPatterns: SkillPatternPlayerProjection[]
BossPhases: BossPhasePlayerProjection[]
BuildPressureProfiles: BuildPressurePlayerProjection[]
CounterWindows: CounterWindowPlayerProjection[]
CanonicalSignature
```

禁止进入 player-safe 根投影：

```text
EnemySystemSnapshot 完整根对象
SchemaManifest 的 developer component signatures
Encounter / Wave / Slot / quantity / developerContentTagIds
CarrierMechanicBinding / MapRuleMechanicBinding
SkillSequence / CarrierSkillBinding
BossPhase entry condition / BossPhasePlan
PressureSourceBinding / CounterWindowSourceBinding / PressureCounterWindowBinding
InternalOnly / DeveloperOnly
BuildCapabilityKey / MinimumCapabilityBasisPoints
RequirementGroup / Required / Recommended / Any / All
ReadinessBand / CapabilityGap / RequirementStatus
MapRule adjustment / exact delta
Build snapshot ID / signature / source summary
完整答案、Boss 六钥匙、DropBias、随机 seed、路径
```

E09 不创建 Encounter 玩家投影，因为 E04 当前没有权威 player-safe Encounter projection。不得在 E09 猜测或发明玩家可见编排字段。

## 11. Canonical Signature

所有 E09 签名必须为：

```text
sha256: + 64 lowercase hex
```

Full `CanonicalSignature` 必须覆盖：

```text
E09 schemaId/version 与 isolation flags
八项 Schema Manifest
六个静态组件 Full signature
完整 IdentityIndex
完整 RelationIndex
PlayerSafeCanonicalSignature
```

`PlayerSafeCanonicalSignature` 只覆盖第 10 节允许字段。

必须验证：

```text
相同输入重复构造，两个签名相同
六组件输入排列或内部合法排列变化，签名稳定
E03/E05/E06 developer-only 内容变化 -> Full 变化 / PlayerSafe 不变
E03/E05/E06 player-safe 内容变化 -> 两个签名都变化
E04 Encounter 编排变化 -> Full 变化 / PlayerSafe 不变
Relation target 变化 -> Full 变化
E07/E08 transient fixture 内容变化 -> E09 两个静态签名均不变化
E07/E08 schema contract 变化 -> Validator 拒绝
```

## 12. Validator 边界

必须建立：

```text
IEnemySystemDataValidator
IEnemySystemSnapshotProvider
DefaultEnemySystemDataValidator
DefaultEnemySystemSnapshotProvider
EnemySystemValidationIssue
EnemySystemValidationException
```

Issue 至少包含：

```text
Code
Category: Schema / Identity / Reference / Relation / Isolation / Signature / PlayerLeak
ComponentId
Path
Message
```

Issue 必须按 Category、ComponentId、Path、Code Ordinal 确定性排序并只读暴露。Provider 发现任一 issue 时必须拒绝创建根快照。

E09 只做跨组件不变量。不得复制 E01-E08 已有字段级内部 Validator；Verifier 构造各组件时必须先经过原 Provider/Validator。

## 13. 最低拒绝规则

至少拒绝：

```text
null EnemySystemSnapshotInput
六个静态组件任一 null
任一组件 schemaId/version 不匹配
E03.DomainSnapshot 与 E01 Domain signature 不一致
静态组件 CanonicalSignature 格式非法
player-safe signature 格式非法
Manifest 非 8 项、重复项、缺项、模式或 embedded 标记错误
同 IdentityKind 重复 ID
Enemy/Boss carrier ID 冲突
引用目标不存在或类型不匹配
关系 identity 重复
引用两端 isolation 不兼容
任一静态内容 devOnly=false、isEnabled=true 或 entersFormalFlow=true
PlayerSafe property/type closure 超出第 10 节白名单
PlayerSafe canonical payload 出现禁止字段或答案 token
根快照出现 E07/E08 瞬时对象属性
```

## 14. 必须建立的纯数据结构

在独立新目录建立：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/**
```

至少包含等价语义：

```text
EnemySystemSnapshotSchema
EnemySystemComponentKind
EnemySystemComponentMode
EnemySystemValidationCategory
EnemySystemSchemaManifestEntry
EnemySystemIdentitySnapshot
EnemySystemRelationSnapshot
EnemySystemSnapshotInput
EnemySystemPlayerSafeSnapshot
EnemySystemSnapshot
IEnemySystemDataValidator
IEnemySystemSnapshotProvider
EnemySystemValidationIssue
EnemySystemValidationException
DefaultEnemySystemDataValidator
DefaultEnemySystemSnapshotProvider
```

不得扩张为 Adapter、Controller、MonoBehaviour、ScriptableObject、Service locator、事件总线或运行时执行器。

## 15. Verifier fixture

Verifier 必须：

```text
使用 E02 当前 Default Provider
通过 E03 Editor normalizer public entry 获取当前只读 normalization snapshot
通过 E04/E05/E06 原 Provider 构造 synthetic cross-catalog catalogs
通过 E07 Provider 构造 transient Complete 与 Sparse Build fixture
通过 E08 Evaluator 构造 transient readiness fixture
只把 E01-E06 静态组件传入 E09 Root Input
证明 E07/E08 fixture 不进入 Root 属性与签名
```

synthetic fixture 只存在于 Verifier 测试方法，不得形成 Runtime 默认内容；ID 必须通用，不得使用 3-10、4-10、正式章节、真实 Item 或正式 Build 数据。

至少覆盖：

```text
1 个完整合法 Root
8 项 Manifest / 6 Static / 2 Transient
所有 IdentityKind
所有第 8 节 RelationKind
many-to-many 正例
intentional exception 正例
合法未引用 Catalog 条目正例
null 与 component mismatch 负例
cross-catalog unresolved 负例
carrier kind mismatch 负例
isolation mismatch 负例
player leak 负例
full/player signature sensitivity 与 isolation
输入不可变、输出不可变、无共享可变集合
```

## 16. 允许新增文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotInput.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshotInput.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemPlayerSafeSnapshot.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemPlayerSafeSnapshot.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemSnapshot.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/EnemySystemDataValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDataValidatorAndSnapshotVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyDataValidatorAndSnapshotVerifier.cs.meta
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotReport.md
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSpec.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotFieldMatrix.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotSchemaManifest.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotIdentityInventory.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotRelationMatrix.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshotLeakCheckReport.md
```

预期新增文件：`20`。  
允许修改已有文件：`0`。

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 17. 受保护基线

以下 E01-E08 源码必须 byte-identical：

```text
E01 Domain / Contracts / Verifier: 6
E02 Vocabulary / Verifier: 4
E03 Normalization / Editor adapter / Verifier: 4
E04 Composition / Verifier: 4
E05 SkillPhase / Verifier: 5
E06 PressureWindow / Verifier: 5
E07 CapabilityRead / Verifier: 5
E08 ReadinessEvaluation / Verifier: 5
合计: 38/38 unchanged
```

Legacy source：

```text
EnemyBossValidationPool.cs
BuildProblemRuleConfigs.cs
BuildProblemSeedData.cs
合计: 3/3 unchanged
```

必须保持当前基线：

```text
E02 Vocabulary: 63 total / 16 BuildCapability
E03 Carriers: 11 / 7 / 18
E03 Mechanic profiles: 10 / 6 / 16
E03 MapRules: 10
E03 Carrier bindings: 17
E03 MapRule bindings: 30
E03 Intentional exceptions: 2
E04 fixture: 2 Encounter / 4 Wave / 6 Slot
E05 fixture: 4 Pattern / 3 Sequence / 3 Binding / 3 Phase / 2 Plan
E06 fixture: 4 Pressure / 3 Window / 6 PressureSource / 4 WindowSource / 5 PressureWindow / 11 Requirement
E07 fixture: 16 KnownKeys / 16 CompleteValues / 4 SparseValues / 6 SourceSummaries / 3 GapShapes / 5 Bands
E08 fixture: 15 scenarios / 212 checks / 5 reports deterministic
Editor-only legacy reader total: 1
```

## 18. 绝对禁止

```text
不修改任何已有文件
不修改 Scene / Prefab / Config
不新增 MonoBehaviour / ScriptableObject
不引用 UnityEngine、GameObject、Transform、Addressables 或场景 API
不引用 TalismanBag.BuildSandbox Runtime
不引用 ItemSystemSnapshot 或 Item/Inventory/Equipment/Affix/Synergy 实现
不读取棋盘、摆放、拖拽、旋转或真实 Build
不把 BuildCapabilitySnapshot 或 Readiness result 固化进 Root
不执行 Encounter、Wave、Spawn、Skill、BossPhase 或 CounterWindow
不读取 HP、护盾、伤害、施法进度或战斗事件
不修改 BattleSandbox / BattleContract / Battle Bridge / UnifiedBattlePage
不修改玩家 UI、浮字、Tooltip、失败提示或视觉反馈
不创建 3-10 / 4-10 内容，留给 E10
不读取旧 BuildProblem 作为 E09 内容
不生成 DropBias，不发奖励，不写 SaveData
不修改正式 1-10 / 2-10
不接正式章节入口
不覆盖或回退工作区既有未提交改动
不 commit / tag / push
```

## 19. 报告要求

七份报告必须全部由同源 Verifier 每次确定性覆盖生成；禁止 `WriteIfMissing` 或“文件存在就跳过”。连续运行两次后必须 `7/7 hash identical`。

`EnemyDataValidatorAndSnapshotSchemaManifest.csv` 至少包含：

```text
componentId,schemaId,schemaVersion,componentMode,embeddedInRoot,componentCanonicalSignature,playerSafeCanonicalSignature,contractCanonicalSignature
```

`EnemyDataValidatorAndSnapshotIdentityInventory.csv` 至少包含：

```text
identityKind,stableId,owningComponentId,devOnly,isEnabled,entersFormalFlow,duplicateCount,fixtureOnly
```

`EnemyDataValidatorAndSnapshotRelationMatrix.csv` 至少包含：

```text
sourceKind,sourceId,relationKind,targetKind,targetId,owningComponentId,resolved,isolationCompatible,developerOnly,fixtureOnly
```

`EnemyDataValidatorAndSnapshotFieldMatrix.csv` 至少包含：

```text
ownerType,fieldName,fieldType,cardinality,semantic,visibility,sourceOfTruth,futureConsumer
```

主报告必须列出：

```text
Manifest 8/8、Static 6、Transient 2
各 IdentityKind 数量与 duplicate 0
各 RelationKind 数量与 unresolved 0
many-to-many / intentional exception / legal unused coverage
Full 与 PlayerSafe signature
E07/E08 not embedded proof
Player leak 0
Formal flow reference 0
Protected hash 与全部既有基线
```

Leak 报告必须逐项列出禁止依赖、Root property shape、PlayerSafe property/type closure、答案 token 和正式系统引用扫描结果，不得只写总数。

## 20. 自动验证与回报

必须尝试：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
七报告连续两次确定性生成
全局 git diff --check
package-scope whitespace check
GUID 冲突检查
精确变更文件清单
```

当前全局 `git diff --check` 可能仍因开工前无关 BattleSandbox Scene 四处尾随空白失败。必须记录 `PREEXISTING_UNRELATED_DIFF`，并要求 E09 package-scope PASS；不得触碰该场景。

如果同一工程被用户 Unity Editor 占用，不得关闭 Editor 或结束进程；如实记录 Unity 为 `BLOCKED`，继续完成同源离线 Verifier，由 Guard 决定收口。

本包无需场景手测。

完成回报：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyDataValidatorAndSnapshot01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_ENEMYDATAVALIDATORANDSNAPSHOT01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 20
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
Manifest: 8/8 / Static 6 / Transient 2
E07-E08 embedded in root: 必须为 0 / 0
Identity duplicates: 必须为 0
Unresolved relations: 必须为 0
Isolation violations: 必须为 0
Invariant violations: 必须为 0
Many-to-many acceptance: PASS / FAIL
Legal unused content acceptance: PASS / FAIL
Player-safe isolation: PASS / FAIL
Seven-report determinism: 7/7 PASS / FAIL
E01-E08 protected hash: 38/38 unchanged
Legacy source hash: 3/3 unchanged
Verifier: 通过数 / 总数
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
同源离线 Verifier: 通过数 / 总数
Leak: 必须为 0
Formal flow references: 必须为 0
GUID 冲突: 必须为 0
新增文件尾随空白: 必须为 0
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
HEAD unchanged
未 commit / tag / push
```

## 21. 通过后

下一候选包：

```text
V0.4-DevEncounterSeedData01
```

E10 不得自动启动。
