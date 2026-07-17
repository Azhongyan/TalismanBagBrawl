# V0.4-BuildCapabilityReadContract01 Assignment

Guard assignment:

```text
ENEMY_GUARD_ASSIGNMENT_BUILDCAPABILITYREADCONTRACT01
```

生成日期：2026-07-14
所属：Enemy / Boss / Encounter System Guard
包序号：E07

## 1. 包定位

本包建立 Enemy 系统未来只读消费的抽象 BuildCapability Contract，用来描述：

```text
外部系统已经汇总好的抽象 Build 能力值
能力值是否为 Complete 或 Sparse 快照
显式 0 与缺失 Unknown 的区别
能力值的聚合来源摘要
未来 E08 输出使用的 ReadinessBand 与 CapabilityGap 数据形状
不可变、确定性、无场景依赖的 BuildCapabilitySnapshot
```

数据流只能是：

```text
未来外部 Adapter
  -> 构造 BuildCapabilitySnapshot
  -> Enemy E08 只读消费
```

本包不编写 Adapter，不读取 Item、Affix、Synergy、棋盘或战斗状态，也不计算 Readiness。

“BuildCapability”是 E02 定义的抽象能力，例如破盾、清场、净化、供能稳定。它不是 Item ID、核心道具 ID、技能 ID、Affix ID 或棋盘对象。

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
Docs/V0.4/BuildCapabilityReadContract01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/**
Docs/V0.4/Reports/EnemyMechanicVocabularyInventory.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchemaFieldMatrix.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchemaFixtureRows.csv
```

Item、BuildSandbox 与棋盘代码只允许用于禁止依赖扫描，不得被 `using`、反射、字符串类型名或具体接口引用。

## 3. 事实源与依赖方向

必须维持：

```text
E02 BuildCapability vocabulary
  = BuildCapability 稳定键权威

E06 MinimumCapabilityBasisPoints
  = 未来能力比较所需阈值规格，尺度为 1..10000

E07 BuildCapabilityValueBasisPoints
  = 外部汇总后能力值，尺度为 0..10000

E08 EnemyOfflineReadinessEvaluator
  = Requirement 与 Capability 的比较、ReadinessBand 和 CapabilityGap 计算权威
```

E07 不得复制 16 个 BuildCapability 键。完整键集合必须通过包装 E02 只读 Vocabulary 获得。

允许依赖方向：

```text
E07 CapabilityRead runtime
  -> System.*
  -> E01 Domain isolation metadata
  -> E02 Vocabulary key category / read-only resolver contract

E07 Editor Verifier
  -> E02 默认 Vocabulary Provider
  -> 构造瞬时只读 resolver
```

E07 runtime 不得依赖 E03、E04、E05、E06 具体 Snapshot，也不得依赖 Item、BuildSandbox、Battle、Board、Scene 或 UnityEngine。

## 4. 可见性边界

### 4.1 Enemy read-only input

E08 未来只允许读取：

```text
SchemaId / SchemaVersion
SnapshotId
SourceRevisionId
CoverageMode
BuildCapabilityKey
ValueBasisPoints
TryGetCapabilityValue
CanonicalSignature
```

### 4.2 Developer-only diagnostics

只能用于开发者诊断或离线报告：

```text
BuildCapabilitySourceSummary
SourceCategoryId
SourceCount
ContributionBasisPoints
IsConditional
ReadinessBand
CapabilityGap
RequirementGroupId
Available / Required / GapBasisPoints
完整能力矩阵与 CanonicalSignature
```

### 4.3 玩家与 BattleContract

本包不建立 PlayerSafeProjection。以下内容不得进入玩家界面或 BattleContract：

```text
BuildCapabilitySnapshot
BuildCapabilityKey 与具体能力值
SourceSummary
ReadinessBand
CapabilityGap
RequirementGroupId
能力缺口、阈值或完整解法
```

未来玩家反馈必须由 E08 另行生成粗粒度 `PlayerHintProjection`，不能把 E07 对象直接透传。

## 5. 必须建立的纯数据结构

在独立新目录建立：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/**
```

至少包含等价语义：

```text
BuildCapabilityReadSchema
BuildCapabilityScale
BuildCapabilityCoverageMode
CapabilityValueAvailability
ReadinessBand
BuildCapabilitySourceSummarySnapshot
BuildCapabilityValueSnapshot
BuildCapabilitySnapshotInput
BuildCapabilitySnapshot
CapabilityGapSnapshot
IBuildCapabilityVocabularyResolver
IBuildCapabilityReadOnlySnapshot
IBuildCapabilitySnapshotProvider
IBuildCapabilitySnapshotValidator
BuildCapabilityValidationIssue
BuildCapabilityValidationException
DefaultBuildCapabilitySnapshotProvider
DefaultBuildCapabilitySnapshotValidator
```

类名可按既有风格微调，但不得增加 Adapter、Evaluator、Controller、MonoBehaviour 或运行时接线。

## 6. Schema 与能力尺度

必须固定：

```text
schemaId = BuildCapabilityReadContract.v1
schemaVersion = 1
minimum capability value = 0
maximum capability value = 10000
```

规则：

```text
ValueBasisPoints 为 0..10000
0 是已知且明确的零能力
E06 MinimumCapabilityBasisPoints 为 1..10000
E07 与 E06 使用同一 basis-points 尺度
不使用 float / double 作为能力事实源
不做归一化、加法、乘法、封顶或阈值比较
```

E07 只能验证输入值是否在范围内，不能生成能力值。

## 7. BuildCapabilitySnapshot

Snapshot 至少包含：

```text
SchemaId
SchemaVersion
SnapshotId
SourceRevisionId
CoverageMode
CapabilityValues
CanonicalSignature
devOnly = true
isEnabled = false
entersFormalFlow = false
```

Snapshot 必须实现只读查询：

```text
TryGetCapabilityValue(string key, out BuildCapabilityValueSnapshot value)
```

规则：

```text
查询使用 StringComparer.Ordinal
输入集合必须防御性复制
输出集合必须为 IReadOnlyList 或更严格只读接口
Snapshot 不得保存外部可变集合引用
Snapshot 不得保存 delegate、Controller、GameObject 或 Adapter 引用
```

`SnapshotId` 与 `SourceRevisionId` 是不透明稳定 ID，不得包含磁盘路径、Asset 路径、Scene 路径、用户账号或存档槽位。

## 8. Complete 与 Sparse 语义

`BuildCapabilityCoverageMode` 至少包含：

```text
Complete
Sparse
```

Complete 规则：

```text
必须包含 E02 当前全部 BuildCapability key
每个 key 精确出现一次
不得包含未知 key
```

Sparse 规则：

```text
可以只包含 E02 BuildCapability key 的任意子集
允许空 CapabilityValues，表示全部能力仍未知
缺失 key 的 TryGetCapabilityValue 必须返回 false
缺失 key 不得自动生成 ValueBasisPoints = 0
```

必须明确区分：

```text
显式存在 value=0 -> Known Zero
key 缺失 -> Unknown
```

E08 后续不得把 Unknown 静默当成 0；E07 只负责保留该区别。

## 9. BuildCapabilityValue

每个 Value 至少包含：

```text
BuildCapabilityKey
ValueBasisPoints
SourceSummaries
```

规则：

```text
BuildCapabilityKey 必须解析到 E02 BuildCapability 类别
同一 Snapshot 内 key 唯一
ValueBasisPoints 为 0..10000
SourceSummaries 可为空
SourceSummaries 只做诊断解释，不参与 E07 计算
```

## 10. BuildCapabilitySourceSummary

每个聚合摘要至少包含：

```text
SourceCategoryId
SourceCount
ContributionBasisPoints
IsConditional
```

规则：

```text
SourceCategoryId 为不透明聚合类别 ID
同一 CapabilityValue 内 SourceCategoryId 唯一
SourceCount >= 1
ContributionBasisPoints 为 0..10000
摘要贡献之和不要求等于 ValueBasisPoints
```

禁止 SourceSummary 保存：

```text
ItemInstanceId
ItemDefinitionId
AffixId
SynergyId
CoreItemId
棋盘坐标
槽位 ID
技能触发记录
运行时对象引用
```

未来 Adapter 只能提供聚合类别与计数，不得通过 E07 暴露具体道具清单。

## 11. ReadinessBand 与 CapabilityGap

`ReadinessBand` 至少定义稳定枚举：

```text
Unknown
Blocked
Strained
Ready
Strong
```

`CapabilityGapSnapshot` 至少定义：

```text
RequirementGroupId
BuildCapabilityKey
CapabilityValueAvailability: Known / Unknown
AvailableBasisPoints
RequiredBasisPoints
GapBasisPoints
```

这些类型只为 E08 建立不可变输出形状。本包禁止：

```text
从 BuildCapabilitySnapshot 生成 CapabilityGap
给任何输入分配 ReadinessBand
计算 Required - Available
处理 Any / All RequirementGroup
决定 Build 是否可过关
生成玩家提示
```

E07 Verifier 只检查字段不可变、范围可表达和 Canonical 数据类型稳定，不验证 Gap 算术；Gap 计算与语义验证全部留给 E08。

## 12. Vocabulary Resolver

Resolver 至少提供：

```text
HasBuildCapabilityKey(string key)
GetKnownBuildCapabilityKeys()
```

规则：

```text
Resolver 必须包装 E02 EnemyMechanicVocabularySnapshot
已知键集合使用 Ordinal 语义
不得在 E07 复制或硬编码 16 个能力键
Validator 必须拒绝 resolver 返回 null、重复键、空键或错误类别
Complete 集合完整性必须与 resolver 当次只读结果比较
```

## 13. Canonical Signature

CanonicalSignature 必须为：

```text
sha256: + 64 lowercase hex
```

完整签名必须覆盖：

```text
schemaId / schemaVersion
SnapshotId / SourceRevisionId
CoverageMode
所有 CapabilityValue
所有 SourceSummary
全部隔离标记
```

必须满足：

```text
相同内容重复构造，签名相同
仅改变输入集合排列，签名相同
改变能力值，签名变化
改变 SourceSummary，签名变化
改变 Complete / Sparse，签名变化
改变 SnapshotId 或 SourceRevisionId，签名变化
```

输入数组顺序不是语义；CapabilityKey 与 SourceCategoryId 是 Canonical 排序事实源。

## 14. Verifier fixture

Verifier 只能从 E02 默认 Vocabulary 生成 synthetic fixture：

```text
Known BuildCapability keys: 16
1 个 Complete Snapshot，包含 16 个 Value
1 个 Sparse Snapshot，包含 4 个 Value
至少 1 个 Sparse Empty Snapshot
至少 1 个显式 Known Zero
至少 1 个缺失 Unknown 查询
SourceSummary: 至少 6 条
CapabilityGap shape fixture: 3 条
ReadinessBand: 5 个稳定枚举值
```

fixture 必须证明：

```text
Complete 必须覆盖 E02 全部能力键
Sparse 只保留输入子集
Known Zero 与 Unknown 不相等
TryGet 使用 Ordinal 严格匹配
没有 Item 程序集、场景或棋盘也能构造与验证
```

fixture 使用通用 ID，例如：

```text
dev_build_capability_fixture_complete
dev_build_capability_fixture_sparse
source.fixture.base
source.fixture.composite
```

不得使用 3-10、4-10、正式存档、真实 Build 或 Item 数据。

## 15. 允许新增文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilitySnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilitySnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/ReadinessDiagnosticSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/ReadinessDiagnosticSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs.meta
Docs/V0.4/Reports/BuildCapabilityReadContractReport.md
Docs/V0.4/Reports/BuildCapabilityReadContractSpec.csv
Docs/V0.4/Reports/BuildCapabilityReadContractFieldMatrix.csv
Docs/V0.4/Reports/BuildCapabilityReadContractFixtureRows.csv
Docs/V0.4/Reports/BuildCapabilityReadContractLeakCheckReport.md
```

预期新增文件：16。
允许修改已有文件：0。

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 16. 受保护文件与基线

以下源码必须保持 byte-identical：

```text
E01 Domain / Contracts / Verifier: 6
E02 Vocabulary / Verifier: 4
E03 Normalization / Editor adapter / Verifier: 4
E04 Composition / Verifier: 4
E05 SkillPhase / Verifier: 5
E06 PressureWindow / Verifier: 5
合计: 28/28 unchanged
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
```

## 17. 绝对禁止

```text
不修改任何已有文件
不修改 Scene / Prefab / Config
不新增 MonoBehaviour / ScriptableObject
不引用 UnityEngine、GameObject、Transform、Addressables 或场景对象
不引用 ItemSystemSnapshot 或任何 Item/Inventory/Equipment 具体实现
不编写 Item -> BuildCapability Adapter
不读取 Item、Affix、Synergy、核心道具、棋盘、槽位或触发记录
不读取 Battle、Board、RunFlow、Reward、SaveData 或正式 Boss 流程
不接 BattleContract / Battle Bridge / UnifiedBattlePage
不计算 BuildCapabilityValue
不执行加法、乘法、封顶、衰减、权重或归一化
不比较 E06 Requirement threshold
不实现 Any / All Requirement 判定
不生成 ReadinessBand 或 CapabilityGap
不实现 E08 Evaluator 或 PlayerHintProjection
不建立玩家完整答案、Boss 六钥匙或 DropBias
不硬编码 16 个 BuildCapability 键
不硬编码 3-10 / 4-10
不修改 E01 / E02 / E03 / E04 / E05 / E06
不修改正式 1-10 / 2-10
不接正式章节入口
不发正式奖励
不覆盖或回退工作区既有未提交改动
不 commit / tag / push
```

## 18. Validator 最低规则

必须拒绝：

```text
null Input / null Resolver
schemaId / schemaVersion 不匹配
空 ID、ID 外侧空白、隐式 Trim
SnapshotId / SourceRevisionId 含路径、账号或存档槽位语义
未知 CoverageMode
重复 BuildCapabilityKey
未知或错误类别 BuildCapabilityKey
ValueBasisPoints 不在 0..10000
null CapabilityValue / null SourceSummary
重复或空 SourceCategoryId
SourceCount < 1
ContributionBasisPoints 不在 0..10000
Complete 缺少 E02 key 或包含额外 key
Resolver known-key 集合为 null、含空键、重复键或非 BuildCapability 类别
devOnly = false / isEnabled = true / entersFormalFlow = true
```

所有 ID 与键比较使用 `StringComparer.Ordinal`。Validator 不得修改、补齐或 Trim 输入。

## 19. Verifier 最低验收

至少验证：

```text
Schema ID / version 与 0..10000 尺度精确
所有顶层与嵌套集合不可变
输入集合防御性复制
TryGet 为 Ordinal 严格匹配
Complete / Sparse / Sparse Empty 行为正确
Known Zero 与 Unknown 区分正确
第 18 节非法输入均被明确错误码拒绝
E02 Resolver 直接包装当前 Vocabulary，没有复制能力键清单
完整签名格式、确定性、顺序无关性与字段敏感性有效
CapabilityGap 与 ReadinessBand 只是数据形状，无生成逻辑
E07 runtime Item / Inventory / Equipment / Affix / Synergy references = 0
E07 runtime Battle / Board / Scene / RunFlow / Reward / SaveData references = 0
E07 runtime E03 / E04 / E05 / E06 concrete Snapshot references = 0
E07 evaluator / threshold comparison / player projection code = 0
章节硬编码、Boss keys、DropBias = 0
E01-E06 protected hash = 28/28 unchanged
Legacy source hash = 3/3 unchanged
E02-E06 基线不变
Scene / Prefab / Config / Battle / Board / Item modifications = 0
package path whitelist PASS
GUID 冲突 = 0
尾随空白 = 0
git diff --check PASS
```

Unity batch 与同源离线入口必须调用同一套检查逻辑，不得维护两份断言。

## 20. 报告、自动验证与回报

`BuildCapabilityReadContractFieldMatrix.csv` 至少包含：

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

`BuildCapabilityReadContractFixtureRows.csv` 至少包含：

```text
rowKind
snapshotId
coverageMode
buildCapabilityKey
valueBasisPoints
sourceCategoryId
sourceCount
contributionBasisPoints
availability
fixtureOnly
```

主报告必须列出：

```text
Known capability key 数量
Complete / Sparse / Sparse Empty Snapshot 数量
CapabilityValue / SourceSummary 数量
Known Zero / Unknown 检查
CanonicalSignature
Item-independent 检查
```

Leak 报告必须逐项列出禁止依赖，不得只写总数。

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

如果同一工程被用户 Unity Editor 占用，不得关闭 Editor 或结束进程；如实记录 Unity 为 BLOCKED，继续完成同源离线 Verifier，由 Guard 决定收口。

本包无需场景手测。

完成回报格式：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BuildCapabilityReadContract01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 16
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 必须为 0 / 0 / 0 / 0 / 0 / 0
E01-E06 protected hash: 28/28 unchanged
Legacy source hash: 3/3 unchanged
Fixture: KnownKeys / CompleteValues / SparseValues / SourceSummaries / GapShapes / Bands 数量
Complete coverage: PASS / FAIL
Sparse unknown preservation: PASS / FAIL
Known Zero vs Unknown: PASS / FAIL
Item-independent: PASS / FAIL
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
V0.4-EnemyOfflineReadinessEvaluator01
```

E08 不得自动启动。
