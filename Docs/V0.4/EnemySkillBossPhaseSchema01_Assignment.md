# V0.4-EnemySkillBossPhaseSchema01 Assignment

Guard assignment:

```text
ENEMY_GUARD_ASSIGNMENT_ENEMYSKILLBOSSPHASESCHEMA01
```

生成日期：2026-07-14  
所属：Enemy / Boss / Encounter System Guard  
包序号：E05

## 1. 包定位

本包建立长期可复用的 Enemy Skill 与 Boss Phase 纯数据 Schema，用来描述：

```text
技能向玩家公开什么意图、技能名、施法提示和目标提示
技能内部使用什么施法规格与 MechanicProfile 引用
多个 SkillPattern 如何组成确定顺序的 SkillSequence
Enemy / Boss Carrier 如何选择 SkillSequence
Boss 如何组合有序 Phase，并为每个 Phase 声明进入条件
公开阶段名称、内部规格和开发者诊断如何隔离
```

本包不是技能执行器、计时器、目标选择器、伤害系统、Boss 状态机或施法条 Controller。

“机制题”“验证题”仍只是 Build 压力的内部形容词，不得在本包建立真实题库、选择题、问卷或答案系统。

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
Docs/V0.4/EnemySkillBossPhaseSchema01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs
Docs/V0.4/Reports/EnemyValidationMechanicInventory.csv
Docs/V0.4/Reports/EnemyCarrierMechanicBinding.csv
Docs/V0.4/Reports/EnemyValidationNormalizationExceptions.csv
```

旧 `EnemyBossValidationPool.cs` 只允许用于受保护 hash 复核，不得读取其攻击伤害、攻击间隔或复制为 E05 新数据。

## 3. 事实源与依赖方向

必须维持：

```text
E01 SkillPatternReference / BossPhaseReference
  = SkillPattern 与 BossPhase 身份、隔离标记的权威

E02 PlayerHintCategory / DeveloperDiagnosticCategory
  = 公开提示类别与开发诊断类别稳定键权威

E03 Enemy / Boss / MechanicProfile 与 CarrierMechanicBinding
  = 可引用载体、机制和合法绑定的权威

E05 SkillPattern / SkillSequence / CarrierSkillBinding / BossPhasePlan
  = 技能与阶段组合关系的权威
```

E05 不得复制 Enemy、Boss 或 MechanicProfile 的显示名、机制内容、BuildCapability 要求、LegacyFacts、MappingTrace 或完整对象。

允许依赖方向：

```text
E05 SkillPhase runtime
  -> E01 Domain / Contracts
  -> E02 Vocabulary 只读键类型
  -> E03 Normalization 只读引用类型

E05 Editor Verifier
  -> 调用现有 EnemyValidationContentNormalizer.CreateSnapshot()
  -> 调用 E02 默认 Vocabulary Provider
  -> 构造瞬时只读 resolver
```

E05 runtime 与新 Verifier 不得直接引用 `TalismanBag.BuildSandbox`，不得建立第二个 legacy reader。现有 E03 Editor-only legacy reader 总数必须继续为 1。

## 4. 三层字段边界

### 4.1 Player-safe projection

只允许：

```text
SkillPatternId
PublicSkillNameKey
IntentId
PublicIntentTextKey
PublicTargetCueKey
PublicCastCueKey
PlayerHintCategoryKeys
BossPhaseId
PublicPhaseLabelKey
PublicPhaseEntryCueKey
```

这些字段是未来 BattleContract 可选择性映射的公开元数据，不代表可以把全部 Skill 或 Phase Catalog 一次性显示给玩家。未来 UI 只能按当前运行状态读取当前 Skill、Intent 或 Phase。

### 4.2 Internal-only spec

允许保存但不得直接作为玩家对象序列化：

```text
SkillCastKind
CastDurationMilliseconds
RecoveryDurationMilliseconds
SkillSequence 与 Step 顺序
DelayAfterPreviousMilliseconds
RepeatCount
CarrierSkillBinding
MechanicProfileIds
BossPhasePlan 与 PhaseOrder
BossPhaseEntryCondition
HealthThresholdBasisPoints
MechanicSignalId
```

这些只是数据规格。本包不消费时间、不计算进度、不检测血量、不触发信号、不切换阶段。

### 4.3 Developer-only diagnostics

只允许开发者诊断层读取：

```text
DeveloperDiagnosticCategoryKeys
SourceReferenceIds
完整 CanonicalSignature
完整引用矩阵与验证错误
```

`SourceReferenceIds` 必须是稳定来源 ID，不得保存磁盘路径、Asset 路径或场景路径。

## 5. 必须建立的纯数据结构

在独立新目录建立：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/**
```

至少包含等价语义：

```text
EnemySkillBossPhaseSchema
SkillCastKind
BossPhaseEntryConditionKind
SkillPatternPlayerProjection
SkillPatternInternalSpec
SkillPatternDeveloperDiagnostics
EnemySkillPatternSnapshot
SkillSequenceStepSnapshot
SkillSequenceSnapshot
CarrierSkillBindingSnapshot
BossPhasePlayerProjection
BossPhaseInternalSpec
BossPhaseDeveloperDiagnostics
BossPhaseProfileSnapshot
BossPhaseEntryConditionSnapshot
BossPhasePlanEntrySnapshot
BossPhasePlanSnapshot
EnemySkillBossPhaseCatalogInput
EnemySkillBossPhaseCatalogSnapshot
IEnemySkillBossPhaseReferenceResolver
IEnemySkillBossPhaseLookup
IEnemySkillBossPhaseProvider
IEnemySkillBossPhaseValidator
EnemySkillBossPhaseValidationIssue
DefaultEnemySkillBossPhaseProvider
DefaultEnemySkillBossPhaseValidator
```

类名可按既有风格微调，但不得把数据结构扩张成 Runtime 执行。

## 6. Catalog 与 Skill 字段

Catalog 至少包含：

```text
schemaId = EnemySkillBossPhase.v1
schemaVersion = 1
skillPatterns
skillSequences
carrierSkillBindings
bossPhaseProfiles
bossPhasePlans
canonicalSignature
playerSafeCanonicalSignature
```

SkillPattern 至少包含：

```text
SkillPatternReference
PlayerSafe
InternalOnly
DeveloperOnly
```

SkillPattern internal spec 至少包含：

```text
SkillCastKind: Instant / Channeled
CastDurationMilliseconds
RecoveryDurationMilliseconds
MechanicProfileIds
```

规则：

```text
Instant 的 CastDurationMilliseconds 必须为 0
Channeled 的 CastDurationMilliseconds 必须 > 0
RecoveryDurationMilliseconds 必须 >= 0
所有持续时间使用 int 毫秒，不使用 float / double 作为 Canonical 时间事实源
TargetCue 只描述公开提示，不包含目标选择算法或目标对象引用
```

## 7. SkillSequence 与 Carrier binding

SkillSequence 至少包含：

```text
SkillSequenceId
Steps
devOnly = true
isEnabled = false
entersFormalFlow = false
```

Step 至少包含：

```text
StepOrder
SkillPatternId
DelayAfterPreviousMilliseconds
RepeatCount
```

CarrierSkillBinding 至少包含：

```text
CarrierKind: Enemy / Boss
CarrierId
SkillSequenceIds
```

规则：

```text
StepOrder 为 0..N-1 且唯一
DelayAfterPreviousMilliseconds >= 0
RepeatCount >= 1
SkillPatternId 必须解析到 E05 Catalog
CarrierId 必须按 kind 解析到 E03
SkillSequenceId 必须解析到 E05 Catalog
序列中 SkillPattern 的 MechanicProfile kind 必须与 Carrier kind 兼容
```

同一 SkillPattern 可以被多个 Sequence 复用，同一 Sequence 可以被多个合法 Carrier 或 BossPhase 复用。不得强制一对一。

## 8. BossPhase Profile 与 Plan

BossPhaseProfile 至少包含：

```text
BossPhaseReference
PlayerSafe
InternalOnly.SkillSequenceIds
InternalOnly.MechanicProfileIds
DeveloperOnly.DeveloperDiagnosticCategoryKeys
DeveloperOnly.SourceReferenceIds
```

BossPhasePlan 至少包含：

```text
BossId
Entries
devOnly = true
isEnabled = false
entersFormalFlow = false
```

Plan Entry 至少包含：

```text
PhaseOrder
BossPhaseId
EntryCondition
```

EntryCondition kind 至少支持：

```text
EncounterStart
HealthRatioAtOrBelow
PreviousPhaseCompleted
MechanicSignal
```

条件字段规则：

```text
EncounterStart: threshold = 0，signalId 为空
HealthRatioAtOrBelow: threshold 为 1..9999 basis points，signalId 为空
PreviousPhaseCompleted: threshold = 0，signalId 为空
MechanicSignal: threshold = 0，signalId 必须为非空稳定 ID
```

PhaseOrder 必须为 0..N-1。首阶段必须使用 `EncounterStart`，后续阶段不得再次使用 `EncounterStart`。

本包不得擅自规定：

```text
Boss 永远只有固定三阶段
所有阶段都必须按生命阈值进入
阶段阈值必须等距
一个 BossPhase 只能属于一个 Boss
一个 SkillSequence 只能属于一个 Phase
Boss Phase 必须对应 E04 最后一波
```

BossPhaseProfile 与 BossPhasePlan 分离，以支持 Phase/Profile 与 Sequence 的复用。

## 9. Reference Resolver 约定

Resolver 至少能回答：

```text
Enemy ID 是否存在
Boss ID 是否存在
MechanicProfile ID 及其 Enemy/Boss kind 是否存在
Carrier -> MechanicProfile binding 是否存在
PlayerHintCategory key 是否存在
DeveloperDiagnosticCategory key 是否存在
```

规则：

```text
SkillPattern MechanicProfileId 必须解析
CarrierSkillBinding 中每个 Pattern 的 MechanicProfile 必须适配 Carrier kind
BossPhase MechanicProfile 必须为 Boss kind
BossPhasePlan BossId 必须解析为 Boss
PlayerHintCategoryKeys 必须来自 E02 对应类别
DeveloperDiagnosticCategoryKeys 必须来自 E02 对应类别
```

Resolver 只能包装 E02/E03 只读快照，不得持久化第二份词汇、载体、机制或 binding 清单。

## 10. Player-safe 隔离

`PlayerSafeCanonicalSignature` 必须包含 `schemaId / schemaVersion`；除此之外，业务内容只允许来自 SkillPattern 与 BossPhase 的 Player-safe projection。

禁止进入 player-safe payload：

```text
CastDurationMilliseconds
RecoveryDurationMilliseconds
SkillSequenceId / StepOrder / Delay / RepeatCount
CarrierSkillBinding
MechanicProfileIds
BossPhasePlan / PhaseOrder
EntryCondition kind
HealthThresholdBasisPoints
MechanicSignalId
DeveloperDiagnosticCategoryKeys
SourceReferenceIds
完整未来 Skill 或 Phase 顺序
BuildCapability 要求
完整答案、Boss keys、DropBias
```

必须验证：

```text
修改 Internal-only 字段，完整签名变化，player-safe 签名不变
修改 Developer-only 字段，完整签名变化，player-safe 签名不变
修改 Player-safe 字段，两个签名都变化
```

## 11. Canonical Signature

完整与 player-safe 签名都必须为：

```text
sha256: + 64 lowercase hex
```

完整签名必须覆盖所有字段、引用、显式顺序和隔离标记。

必须满足：

```text
相同内容重复生成，签名相同
仅改变输入集合排列，签名相同
改变 StepOrder，完整签名变化
改变 PhaseOrder，完整签名变化
改变 CastDuration、Delay、RepeatCount 或 EntryCondition，完整签名变化
改变 public Intent / Skill / TargetCue / Phase label，两个签名都变化
```

输入数组顺序不是语义；显式 StepOrder 与 PhaseOrder 才是顺序事实源。

## 12. Verifier fixture

Verifier 使用 E02/E03 当前只读快照，并构造仅存在于测试方法内的 synthetic fixture：

```text
至少 4 个 SkillPattern
至少 3 个 SkillSequence
至少 2 个 CarrierSkillBinding，覆盖 Enemy 与 Boss
至少 3 个 BossPhaseProfile
至少 2 个 BossPhasePlan
至少 1 个 SkillPattern 被多个 Sequence 复用
至少 1 个 SkillSequence 被 Carrier 与 BossPhase 复用
至少 1 个 BossPhaseProfile 被多个 BossPlan 复用
至少 1 个 Instant Skill
至少 1 个 Channeled Skill
至少覆盖 EncounterStart、HealthRatioAtOrBelow、MechanicSignal 条件
```

fixture 使用通用 ID，例如：

```text
dev_skill_fixture_guard_cast
dev_sequence_fixture_pressure
dev_phase_fixture_opening
```

不得硬编码 `3-10`、`4-10` 或正式章节 ID。fixture 不是 E10 SeedData，不得保存为 Config、Asset 或默认运行内容。

## 13. 允许新增文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhasePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhasePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillPatternSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillPatternSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/BossPhaseSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/BossPhaseSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhaseValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/EnemySkillBossPhaseValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemySkillBossPhaseSchemaVerifier.cs.meta
Docs/V0.4/Reports/EnemySkillBossPhaseSchemaReport.md
Docs/V0.4/Reports/EnemySkillBossPhaseSchemaSpec.csv
Docs/V0.4/Reports/EnemySkillBossPhaseSchemaFieldMatrix.csv
Docs/V0.4/Reports/EnemySkillBossPhaseSchemaFixtureRows.csv
Docs/V0.4/Reports/EnemySkillBossPhaseSchemaLeakCheckReport.md
```

预期新增文件：16。  
允许修改已有文件：0。

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 14. 受保护文件

以下源码必须保持 byte-identical：

```text
E01 Domain / Contracts / Verifier: 6
E02 Vocabulary / Verifier: 4
E03 Normalization / Editor adapter / Verifier: 4
E04 Composition / Verifier: 4
合计: 18/18 unchanged
```

以下 Legacy source 必须保持：

```text
EnemyBossValidationPool.cs
BuildProblemRuleConfigs.cs
BuildProblemSeedData.cs
合计: 3/3 unchanged
```

E03 与 E04 基线数量也必须保持：

```text
Carriers: 11 / 7 / 18
Mechanic profiles: 10 / 6 / 16
MapRules: 10
Carrier bindings: 17
MapRule bindings: 30
Intentional exceptions: 2
E04 fixture baseline: 2 Encounter / 4 Wave / 6 Slot
```

## 15. 绝对禁止

```text
不修改任何已有文件
不修改 Scene / Prefab / Config
不新增 MonoBehaviour / ScriptableObject
不引用 GameObject、Transform、UnityEngine 场景对象或 Addressables
不实现计时器、Update、Coroutine、async delay 或时间推进
不实现状态机 Runtime 或阶段切换
不读取当前 HP、护盾、战斗时间或战斗状态
不执行目标选择
不执行伤害、护盾、状态、召唤、控制或资源变化
不定义真实伤害值、攻击间隔、技能倍率或掉落数值
不修改现有施法条、Boss 反馈、浮字或 Controller
不实现 CounterWindow / BuildPressure，留给 E06
不读取 Item 或 BuildSnapshot，留给 E07 之后的只读 Contract
不实现 BuildReadiness，留给 E08
不实现 3-10 / 4-10 SeedData，留给 E10
不新增完整答案、requiredCapability、Boss keys、weakness threshold 或 DropBias
不修改 E01 / E02 / E03 / E04
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

## 16. Validator 最低规则

必须拒绝：

```text
空 ID、ID 外侧空白、大小写替代、隐式 Trim
重复 SkillPatternId / SkillSequenceId / BossPhaseId / BossPlan BossId
空 SkillSequence 或空 BossPhasePlan
重复、不连续或负数 StepOrder / PhaseOrder
未解析 Pattern / Sequence / Carrier / Boss / MechanicProfile 引用
Carrier kind 与 MechanicProfile kind 不兼容
BossPhase 引用 Enemy MechanicProfile
错误 Vocabulary category 或未知 Vocabulary key
Instant castDuration != 0
Channeled castDuration <= 0
任意 duration / delay < 0
RepeatCount < 1
EntryCondition 字段组合不合法
首 Phase 不是 EncounterStart
后续 Phase 再次使用 EncounterStart
devOnly = false / isEnabled = true / entersFormalFlow = true
schemaId / schemaVersion 不匹配
player-safe payload 包含内部或诊断字段
```

所有 ID 比较使用 `StringComparer.Ordinal`。所有输入与输出集合必须防御性复制并以只读接口暴露。

## 17. Verifier 最低验收

至少验证：

```text
Schema ID / version 精确
所有顶层与嵌套集合不可变
Pattern / Sequence / Phase / BossPlan lookup 为 Ordinal 严格匹配
第 16 节非法输入均被明确错误码拒绝
所有 E02/E03 引用可解析
Pattern、Sequence、Phase 多处复用有效
完整签名与 player-safe 签名格式、确定性和敏感性有效
Internal / Developer 变化不污染 player-safe 签名
Player-safe 变化同时改变两个签名
player-safe payload 无完整序列、时间、阶段条件、Mechanic、Build 答案和诊断泄漏
E05 runtime BuildSandbox references = 0
E05 direct Battle / Board / Item / RunFlow / Reward / SaveData references = 0
E05 timer / state machine / scene object references = 0
章节硬编码 = 0
答案、Boss keys、DropBias 泄漏 = 0
E01 + E02 + E03 + E04 protected hash = 18/18 unchanged
Legacy source hash = 3/3 unchanged
E03 / E04 基线不变
Scene / Prefab / Config / Battle / Board / Item modifications = 0
package path whitelist PASS
GUID 冲突 = 0
尾随空白 = 0
git diff --check PASS
```

Unity batch 与同源离线入口必须调用同一套检查逻辑，不得维护两份断言。

## 18. 报告要求

`EnemySkillBossPhaseSchemaFieldMatrix.csv` 至少包含：

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

`EnemySkillBossPhaseSchemaFixtureRows.csv` 至少包含：

```text
rowKind
ownerId
order
referenceId
publicLabelOrCueKey
castKind
durationMilliseconds
entryConditionKind
thresholdBasisPoints
playerVisible
fixtureOnly
```

主报告必须分别列出：

```text
SkillPattern / SkillSequence / CarrierSkillBinding 数量
BossPhaseProfile / BossPhasePlan 数量
完整签名
player-safe 签名
复用检查
隔离检查
```

Leak 报告必须逐项列出禁止依赖与扫描结果，不得只写总数。

## 19. 自动验证

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

## 20. 完成回报格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemySkillBossPhaseSchema01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 16
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 必须为 0 / 0 / 0 / 0 / 0 / 0
E01 + E02 + E03 + E04 protected hash: 18/18 unchanged
Legacy source hash: 3/3 unchanged
Fixture: Pattern / Sequence / CarrierBinding / PhaseProfile / BossPlan 数量
Pattern reuse: PASS / FAIL
Sequence reuse: PASS / FAIL
Phase reuse: PASS / FAIL
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
V0.4-CounterWindowAndPressureSchema01
```

E06 不得自动启动。
