# V0.4-EnemyOfflineReadinessEvaluator01 Assignment

Guard assignment:

```text
ENEMY_GUARD_ASSIGNMENT_ENEMYOFFLINEREADINESSEVALUATOR01
```

生成日期：2026-07-14
所属：Enemy / Boss / Encounter System Guard
包序号：E08

## 1. 包定位

本包建立 Enemy 系统长期可复用的离线 Build Readiness 纯函数 Evaluator：

```text
E07 BuildCapabilitySnapshot
+ E06 BuildPressureProfile
+ 调用方传入的 MapRule capability adjustments
-> DeveloperReadinessSnapshot
+ PlayerHintProjection
```

本包只回答“这个抽象 Build 能力快照面对一个抽象 PressureProfile 时，Required / Recommended 要求组的离线覆盖情况是什么”。

本包不是战斗模拟器、伤害预测器、自动配装器、Item Adapter、Boss AI、运行时窗口控制器或正式结算器。

“机制题”“破题”仍只是 Build 压力的内部形容词。Evaluator 不得给玩家展示完整答案，也不得替玩家选择 Build 或核心道具。

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
Docs/V0.4/EnemyOfflineReadinessEvaluator01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/**
Docs/V0.4/Reports/CounterWindowAndPressureSchemaFieldMatrix.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchemaFixtureRows.csv
Docs/V0.4/Reports/BuildCapabilityReadContractFieldMatrix.csv
Docs/V0.4/Reports/BuildCapabilityReadContractFixtureRows.csv
```

旧 BuildProblem 与 Item/BuildSandbox 文件只允许用于禁止依赖和受保护 hash 扫描，不得作为 E08 内容源。

## 3. 事实源与依赖方向

必须维持：

```text
E02 BuildCapability / PlayerHintCategory vocabulary
  = 抽象能力键与玩家提示类别权威

E03 MapRule identity
  = 可引用 MapRule 稳定 ID 权威

E06 BuildPressureProfile
  = RequirementGroup、Required/Recommended、Any/All、阈值与公开压力提示权威

E07 BuildCapabilitySnapshot
  = Known capability value、Complete/Sparse 与 Known Zero/Unknown 权威

E07 ReadinessBand / CapabilityGap shape
  = E08 开发者结果使用的数据类型权威

E08 Evaluator
  = Requirement、Group、ReadinessBand、Gap 与粗粒度玩家严重度计算权威
```

E08 不得复制 E02 键表，不得复制 E03 MapRule 内容，不得复制 E06 Pressure 内容，也不得生成第二份 BuildCapabilitySnapshot。

允许依赖方向：

```text
E08 ReadinessEvaluation runtime
  -> System.*
  -> E06 PressureWindow 只读 Catalog / Snapshot
  -> E07 CapabilityRead 只读 Snapshot / diagnostics shape
  -> E08 自有 resolver interface

E08 Editor Verifier
  -> E02 默认 Vocabulary Provider
  -> E03 EnemyValidationContentNormalizer.CreateSnapshot()
  -> 使用 E06 Provider 构造 synthetic validated catalog
  -> 使用 E07 Provider 构造 synthetic validated capability snapshots
  -> 构造瞬时只读 resolver
```

E08 runtime 与 Verifier 不得直接引用 `TalismanBag.BuildSandbox` 或 Item 系统。现有 E03 Editor-only legacy reader 总数必须继续为 1。

## 4. 输入边界

`EnemyReadinessEvaluationInput` 至少包含：

```text
BuildCapabilitySnapshot
CounterWindowAndPressureCatalogSnapshot
BuildPressureProfileId
MapRuleCapabilityAdjustments
```

规则：

```text
BuildCapabilitySnapshot 必须来自 E07 Provider
Pressure Catalog 必须来自 E06 Provider
BuildPressureProfileId 必须通过 E06 Catalog Ordinal lookup 解析
一次 Evaluate 只评估一个 PressureProfile
输入对象与嵌套集合只读，Evaluator 不得修改
```

使用经过 Provider 验证的 E06 Catalog，而不是接受任意裸 `BuildPressureProfileSnapshot`，避免 E08 重建第二套 Pressure Validator。

## 5. MapRule capability adjustment

本包允许定义输入数据形状：

```text
MapRuleCapabilityAdjustmentSnapshot
  MapRuleId
  BuildCapabilityKey
  DeltaBasisPoints
  DeveloperReasonId
```

规则：

```text
MapRuleId 必须解析到 E03
BuildCapabilityKey 必须解析到 E02 BuildCapability
DeltaBasisPoints 为 -10000..10000 且不得为 0
同一 MapRuleId + BuildCapabilityKey 组合唯一
DeveloperReasonId 为稳定开发者 ID，不得含路径
多个不同 MapRule 对同一能力的 delta 使用 long 求和后 clamp 到 0..10000
输入顺序不影响结果
```

Known capability：

```text
effective = clamp(baseValue + sum(delta), 0, 10000)
```

Unknown capability：

```text
保持 Unknown
不得因 MapRule buff/debuff 凭空变成 Known
不得用 0 代替未知输入参与比较
```

MapRule adjustment 只是调用方传入的离线评估上下文。本包不读取旧 MapRule buff/debuff，不保存默认内容；实际 devOnly 内容留给 E10。

## 6. Requirement 结果语义

每个 E06 `BuildCapabilityRequirement` 产生一个 developer-only evaluation row：

```text
RequirementGroupId
RequirementRole
RequirementMatchMode
BuildCapabilityKey
CapabilityValueAvailability
BaseValueBasisPoints
MapDeltaBasisPoints
EffectiveValueBasisPoints
RequiredValueBasisPoints
GapBasisPoints
RequirementStatus: Met / Unmet / Unknown
```

Known 规则：

```text
effective >= required -> Met，gap = 0
effective < required -> Unmet，gap = required - effective
```

Unknown 规则：

```text
status = Unknown
CapabilityValueAvailability = Unknown
Base / Effective / GapBasisPoints = 0 仅作为无值占位
不得把 GapBasisPoints=0 解读为已满足
```

E08 必须使用 E07 `CapabilityGapSnapshot` 承载对应 developer-only Gap 形状，但需要额外 RequirementStatus 与 GroupStatus 防止 Any 组的未满足替代项被误解为阻断。

## 7. Any / All Group 语义

`RequirementGroupStatus` 至少包含：

```text
Met
Unmet
Unknown
```

Any：

```text
任一 Requirement Met -> Group Met
没有 Met，且至少一个 Unknown -> Group Unknown
全部 Unmet -> Group Unmet
```

All：

```text
任一 Requirement Unmet -> Group Unmet
没有 Unmet，且至少一个 Unknown -> Group Unknown
全部 Met -> Group Met
```

一个 Any 组已经 Met 时，组内其他 Unmet 替代项不得标记为“阻断”；Developer result 必须同时保留 row status 与 group status。

RequirementGroup 的输入顺序不得影响结果。所有比较使用整数 basis points。

## 8. ReadinessBand 精确规则

ReadinessBand 只允许进入 DeveloperReadinessSnapshot：

```text
任一 Required Group Unmet -> Blocked
无 Required Unmet，但至少一个 Required Unknown -> Unknown
全部 Required Met，且不存在 Recommended Group -> Ready
全部 Required Met，存在 Recommended Group 且全部 Recommended Met -> Strong
全部 Required Met，存在 Recommended Group 且至少一个 Recommended Unmet/Unknown -> Strained
```

如果 PressureProfile 没有 Required Group：

```text
无 Recommended Group -> Ready
全部 Recommended Met -> Strong
任一 Recommended Unmet/Unknown -> Strained
```

E08 不得用伤害、DPS、胜率、装备稀有度、Item 数量或棋盘占格推导 ReadinessBand。

## 9. DeveloperReadinessSnapshot

至少包含：

```text
BuildSnapshotId
BuildSnapshotCanonicalSignature
PressureCatalogCanonicalSignature
BuildPressureProfileId
ReadinessBand
RequirementGroupEvaluations
RequirementEvaluations
AppliedMapRuleAdjustments
DeveloperCanonicalSignature
```

全部字段均为 developer-only。结果必须可解释：开发者可以看到哪个 Required/Recommended Group 是 Met、Unmet 或 Unknown，以及 Known 输入下的精确 Gap。

不得在 DeveloperReadinessSnapshot 保存：

```text
Item / Affix / Synergy 实例或定义 ID
棋盘坐标或槽位
场景对象
真实战斗伤害结果
正式奖励、掉落或存档引用
```

## 10. PlayerHintProjection

玩家安全结果只允许：

```text
BuildPressureProfileId
PublicPressureLabelKey
PublicPressureHintKey
PlayerHintCategoryKeys
PlayerHintSeverity: None / Notice / Warning / Critical
PlayerSafeCanonicalSignature
```

Severity 映射固定为：

```text
ReadinessBand.Unknown -> Notice
ReadinessBand.Blocked -> Critical
ReadinessBand.Strained -> Warning
ReadinessBand.Ready -> None
ReadinessBand.Strong -> None
```

公开 label、hint 与 category 必须直接来自选中 E06 PressureProfile 的 PlayerSafe projection，不得由 E08 发明答案文本。

禁止进入玩家 payload：

```text
ReadinessBand
BuildCapabilityKey
Base / Effective / Required / GapBasisPoints
RequirementGroupId / Role / MatchMode / Status
CapabilityValueAvailability
MapRuleId / Delta / DeveloperReasonId
Build snapshot ID / revision / signature
Pressure catalog signature
SourceSummary
完整答案、Boss 六钥匙、DropBias
```

本包不修改任何 UI、BattleContract、浮字、失败提示或反馈 Controller。PlayerHintProjection 只是未来可选择消费的数据。

## 11. 必须建立的纯数据结构

在独立新目录建立：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/**
```

至少包含等价语义：

```text
EnemyOfflineReadinessSchema
RequirementEvaluationStatus
RequirementGroupStatus
PlayerHintSeverity
MapRuleCapabilityAdjustmentSnapshot
EnemyReadinessEvaluationInput
RequirementCapabilityEvaluationSnapshot
RequirementGroupEvaluationSnapshot
DeveloperReadinessSnapshot
EnemyReadinessPlayerHintProjection
EnemyReadinessEvaluationResult
IEnemyReadinessReferenceResolver
IEnemyOfflineReadinessEvaluator
EnemyReadinessValidationIssue
EnemyReadinessEvaluationException
DefaultEnemyOfflineReadinessEvaluator
```

类名可按既有风格微调，但不得扩张为 Adapter、MonoBehaviour、Controller、事件订阅器或战斗 Runtime。

## 12. Resolver

Resolver 至少提供：

```text
HasBuildCapabilityKey(string key)
HasMapRule(string mapRuleId)
```

Verifier resolver 必须直接包装 E02 Vocabulary 与 E03 Normalization 快照。不得复制 16 个能力键或 10 个 MapRule ID。

E08 runtime 只依赖 resolver interface，不得直接读取 E03 legacy adapter。

## 13. 纯函数与确定性

Evaluator 必须满足：

```text
相同输入得到完全相同结果与签名
不读取时间、随机数、静态可变状态或环境变量
不写入输入对象
不缓存并返回可变集合
不同输入集合排列不影响结果
所有 ID 比较使用 StringComparer.Ordinal
所有加法用 long 中间值并明确 clamp
```

禁止：

```text
DateTime.Now / Stopwatch / Random
Update / Coroutine / Task.Delay / event subscription
网络、磁盘、PlayerPrefs、SaveData 读取
UnityEngine 数学或场景 API
```

## 14. Canonical Signature

Developer 与 Player-safe 签名都必须为：

```text
sha256: + 64 lowercase hex
```

DeveloperCanonicalSignature 必须覆盖：

```text
schemaId / schemaVersion
Build snapshot ID 与 canonical signature
Pressure catalog canonical signature 与 selected profile ID
全部 MapRule adjustments
全部 Requirement / Group evaluation
ReadinessBand
```

PlayerSafeCanonicalSignature 只覆盖第 10 节字段。

必须验证：

```text
相同输入重复评估，两个签名相同
仅改变 adjustment 输入顺序，两个签名相同
改变能力值、阈值、Map delta 或 Group 语义，Developer 签名变化
内部变化但 Severity 不变时，Player-safe 签名不变
ReadinessBand 跨越 Severity 映射边界时，Player-safe 签名变化
修改 E06 public pressure hint 时，两个签名都变化
```

## 15. Verifier scenarios

Verifier 必须使用 E02/E03 当前只读快照，并通过 E06/E07 Provider 构造仅存在测试方法内的 synthetic fixtures。

至少覆盖：

```text
Strong: Required 与 Recommended 全部 Met
Ready: Required 全部 Met，且没有 Recommended
Strained: Required Met，Recommended Unmet
Blocked: 至少一个 Required Unmet
Unknown: Sparse 缺失 Required capability
Any alternative: 一项 Met、另一项 Unmet，Group 必须 Met
Any unknown: 无 Met、至少一项 Unknown，Group 必须 Unknown
All unmet: 一项 Unmet 即 Group Unmet
Map buff: Known value 从 Blocked 翻转到非 Blocked
Map debuff: Known value从 Ready/Strong 翻转到 Blocked
Clamp low / high: 有效值稳定落在 0 / 10000
Unknown + Map adjustment: 仍为 Unknown
Adjustment order reversed: 结果与签名相同
```

至少输出：

```text
13 个 scenario
Required / Recommended、Any / All 均覆盖
Known Zero 与 Unknown 均覆盖
Positive / Negative MapRule delta 均覆盖
PlayerHintSeverity 四个枚举值均覆盖
```

fixture 使用通用 ID，不得使用 3-10、4-10、正式章节、真实 Build 或 Item 数据。

## 16. 允许新增文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessInputSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessInputSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessResultSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyReadinessResultSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyOfflineReadinessEvaluator.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/EnemyOfflineReadinessEvaluator.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs.meta
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorReport.md
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorSpec.csv
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorFieldMatrix.csv
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorScenarioRows.csv
Docs/V0.4/Reports/EnemyOfflineReadinessEvaluatorLeakCheckReport.md
```

预期新增文件：16。
允许修改已有文件：0。

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 17. 受保护文件与基线

以下源码必须保持 byte-identical：

```text
E01 Domain / Contracts / Verifier: 6
E02 Vocabulary / Verifier: 4
E03 Normalization / Editor adapter / Verifier: 4
E04 Composition / Verifier: 4
E05 SkillPhase / Verifier: 5
E06 PressureWindow / Verifier: 5
E07 CapabilityRead / Verifier: 5
合计: 33/33 unchanged
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
E07 fixture: 16 KnownKeys / 16 CompleteValues / 4 SparseValues / 6 SourceSummaries / 3 GapShapes / 5 Bands
```

## 18. 绝对禁止

```text
不修改任何已有文件
不修改 Scene / Prefab / Config
不新增 MonoBehaviour / ScriptableObject
不引用 GameObject、Transform、UnityEngine 场景对象或 Addressables
不引用 ItemSystemSnapshot 或任何 Item/Inventory/Equipment/Affix/Synergy 具体实现
不编写 Item -> BuildCapability Adapter
不读取棋盘、槽位、拖拽、摆放或真实 Build
不订阅战斗事件，不读取 HP、护盾、伤害、技能状态或 Boss 状态
不执行伤害预测、胜率预测或正式战斗结算
不自动选择、修改或推荐具体 Build / Item / 核心道具
不执行 CounterWindow、技能、Boss Phase 或 Encounter
不修改 BattleSandbox / BattleContract / Battle Bridge / UnifiedBattlePage
不修改浮字、失败提示、Boss 反馈或任何视觉层
不生成正式 DropBias，不发奖励，不写 SaveData
不读取旧 BuildProblem 作为运行内容
不实现 E09 聚合 Snapshot
不实现 3-10 / 4-10 SeedData，留给 E10
不修改 E01 / E02 / E03 / E04 / E05 / E06 / E07
不修改正式 1-10 / 2-10
不接正式章节入口
不覆盖或回退工作区既有未提交改动
不 commit / tag / push
```

## 19. Validator 最低规则

必须拒绝：

```text
null Input / BuildSnapshot / PressureCatalog / Resolver
E06 / E07 schemaId 或 schemaVersion 不匹配
空、带外侧空白或无法 Ordinal 解析的 BuildPressureProfileId
MapRule adjustment null
空、带外侧空白或无法解析的 MapRuleId / BuildCapabilityKey / DeveloperReasonId
DeveloperReasonId 含磁盘、Asset 或 Scene 路径
DeltaBasisPoints 超出 -10000..10000 或等于 0
重复 MapRuleId + BuildCapabilityKey
输入 devOnly / disabled / formal-flow 隔离不满足
Selected Pressure 缺少 PlayerSafe / DeveloperOnly 数据
player-safe payload 出现第 10 节禁止字段
```

所有输入集合必须防御性复制并只读暴露。所有 ID 使用 `StringComparer.Ordinal`。

## 20. Verifier 最低验收

至少验证：

```text
第 6-10 节全部计算语义
13 个 scenario 全部通过
Any / All / Required / Recommended 优先级精确
Known Zero / Unknown / Sparse 语义精确
Map delta long 求和、clamp 与顺序无关
输入不可变、输出不可变、无共享可变集合
Developer result 可解释每个 Group 与 Requirement
Player projection 只含公开提示与 Severity
Developer 与 player-safe 签名格式、确定性、敏感性和隔离有效
E08 runtime BuildSandbox / Item / Battle / Board / Scene references = 0
E08 runtime damage / reward / save / random / time / event code = 0
章节硬编码、Boss keys、DropBias = 0
Editor-only legacy reader total = 1
E01-E07 protected hash = 33/33 unchanged
Legacy source hash = 3/3 unchanged
E02-E07 基线不变
Scene / Prefab / Config / Battle / Board / Item modifications = 0
package path whitelist PASS
GUID 冲突 = 0
新增文件尾随空白 = 0
```

Unity batch 与同源离线入口必须调用同一套检查逻辑，不得维护两份断言。

当前工作区可能存在与本包无关的 Scene 尾随空白。必须：

```text
不得修复或改写无关 Scene
运行全局 git diff --check 并如实记录
若全局失败只来自开工前无关文件，记录 PREEXISTING_UNRELATED_DIFF
对白名单新增文件单独执行尾随空白与 package-scope 检查并要求 PASS
不得为了让全局命令变绿而触碰包外文件
```

## 21. 报告、自动验证与回报

`EnemyOfflineReadinessEvaluatorFieldMatrix.csv` 至少包含：

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

`EnemyOfflineReadinessEvaluatorScenarioRows.csv` 至少包含：

```text
scenarioId
coverageMode
pressureProfileId
requirementGroupId
role
matchMode
capabilityKey
availability
baseValue
mapDelta
effectiveValue
requiredValue
gapValue
requirementStatus
groupStatus
readinessBand
playerHintSeverity
fixtureOnly
```

主报告必须列出：

```text
Scenario 数量与各 ReadinessBand 分布
Any / All / Required / Recommended 覆盖
Known Zero / Unknown 覆盖
Map buff / debuff / clamp 覆盖
DeveloperCanonicalSignature
PlayerSafeCanonicalSignature
Player leak check
```

Leak 报告必须逐项列出禁止依赖与扫描结果，不得只写总数。

必须尝试：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
全局 git diff --check
package-scope whitespace check
GUID 冲突检查
精确变更文件清单
```

如果同一工程被用户 Unity Editor 占用，不得关闭 Editor 或结束进程；如实记录 Unity 为 BLOCKED，继续完成同源离线 Verifier，由 Guard 决定收口。

本包无需场景手测。

完成回报格式：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyOfflineReadinessEvaluator01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 16
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 必须为 0 / 0 / 0 / 0 / 0 / 0
E01-E07 protected hash: 33/33 unchanged
Legacy source hash: 3/3 unchanged
Scenarios: 必须至少 13
ReadinessBand coverage: Unknown / Blocked / Strained / Ready / Strong
Any-All semantics: PASS / FAIL
Known Zero-Unknown semantics: PASS / FAIL
Map adjustment-clamp-order: PASS / FAIL
Player-safe isolation: PASS / FAIL
Unresolved references: 必须为 0
Verifier: 通过数 / 总数
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
Leak: 必须为 0
GUID 冲突: 必须为 0
新增文件尾随空白: 必须为 0
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
未 commit / tag / push
```

## 22. 通过后

下一候选包：

```text
V0.4-EnemyDataValidatorAndSnapshot01
```

E09 不得自动启动。
