# V0.4-EnemyRequirementChannelApplicabilitySchemaContract01 Assignment

Joint Guard receipts:

```text
GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01
ENEMY_GUARD_CONFIRM_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01
CAPABILITY_ALGORITHM_GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01
```

生成日期：2026-07-19

所属：Overall / Cross-System Guard + Enemy Guard + Capability Algorithm Guard

包序号：N01B

## 1. 包定位

本包只建立 Enemy requirement 的中立通道与适用性合同：

```text
ContinuousBP
StructuralPredicate
RuntimeEventSignal

Applicable
Unknown
NotApplicable
NotInChannel
```

本包不迁移 E06/E08/E10 requirement，不修改 readiness 消费者，不实现任何能力值、结构谓词、Runtime outcome 或 Producer。

已批准的 D1-D6 仅作为合同语义依据，不等于批准行为迁移：

```text
D1 placement_shape = 受配置地图压力后的布局韧性结构谓词
D2 debuff_counter = cleanse 前/中的抵抗、免疫或减免能力族
D3 interrupt_timing = 每次有效施法窗口的 Runtime outcome，移出决定性 Offline BP
D4 spirit_lock = 战前结构保护；实际防窃取/防切断属于 Runtime 事实
D5 readiness 按通道汇总；side-channel Unknown 只抑制本通道
D6 允许建立 requirementChannel / applicability 合同；迁移和消费者修改仍须另包
```

风险等级：`YELLOW / ISOLATED CONTRACT ONLY`。

## 2. 启动必读

开发前必须完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/*
Docs/CURRENT/*
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/V0.4/ItemAlgorithmGuard_CurrentRules.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/BuildCapabilityRemainingBlockerSemanticSurvey01_Assignment.md
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticMatrix.csv
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerUserDecisionSheet.csv
Docs/V0.4/EnemyRequirementChannelApplicabilitySchemaContract01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs
```

## 3. 所有权与依赖方向

本合同拥有：

```text
Enemy requirement 的通道类型词汇
某个 evaluation channel 视图中的 applicability / participation 状态
中立只读 Snapshot、Validator 与 Canonical Signature
```

本合同不拥有：

```text
E06/E10 requirement 内容、阈值、RequirementGroup 或 MatchMode
E07 BuildCapability BP 值
E08 readiness 比较与汇总
布局韧性 predicate
debuff_counter / spirit_lock 防御事实
interrupt timing Runtime outcome
Battle Runtime Producer
Item、Battle、Board、UI、配置或正式流程
```

允许依赖方向：

```text
RequirementChannel runtime -> System.* only
Editor verifier -> RequirementChannel runtime + 只读文件/报告扫描
未来迁移 Adapter -> 本合同（不属于本包）
```

禁止本合同 runtime 依赖：

```text
PressureWindow
CapabilityRead
ReadinessEvaluation
SeedData
Item / CrossSystem / Battle / Board / Scene / UnityEngine
```

`BuildCapabilityReadContract.v1` 继续保持 BP-only。本包不得修改、扩展、包装或重用它来承载 structural predicate、runtime signal、NotApplicable 或 NotInChannel。

## 4. Schema 与精确枚举

必须建立：

```text
schemaId = EnemyRequirementChannelApplicability.v1
schemaVersion = 1
```

`EnemyRequirementChannel` 必须只有三个命名有效值：

```csharp
ContinuousBP = 1
StructuralPredicate = 2
RuntimeEventSignal = 3
```

禁止增加：

```text
Unknown channel
Unspecified channel
NotInChannel channel
NotApplicable channel
第四个 authored / evaluation channel
```

数值 `0` 以及任何未定义整数都必须被 Validator 拒绝；不得因为 enum cast 成功而接受。

`EnemyRequirementApplicabilityState` 必须只有四个命名有效值：

```csharp
Applicable = 1
Unknown = 2
NotApplicable = 3
NotInChannel = 4
```

数值 `0` 以及任何未定义整数必须被拒绝。

采用单一 applicability state 枚举，不建立独立 membership + applicability 双枚举。原因是双枚举会制造 `NotInChannel + Applicable`、`NotInChannel + NotApplicable` 等不可能组合。

## 5. 中立 Snapshot 形状

在独立目录建立等价语义：

```text
EnemyRequirementChannelApplicabilitySchema
EnemyRequirementChannel
EnemyRequirementApplicabilityState
EnemyRequirementChannelApplicabilityRowSnapshot
EnemyRequirementChannelApplicabilitySnapshotInput
EnemyRequirementChannelApplicabilitySnapshot
EnemyRequirementChannelApplicabilityValidationIssue
EnemyRequirementChannelApplicabilityValidationException
IEnemyRequirementChannelApplicabilitySnapshotProvider
IEnemyRequirementChannelApplicabilitySnapshotValidator
DefaultEnemyRequirementChannelApplicabilitySnapshotProvider
DefaultEnemyRequirementChannelApplicabilitySnapshotValidator
```

顶层 Snapshot/Input 只允许携带：

```text
SchemaId / SchemaVersion
SnapshotId
EvaluationChannel
Rows
CanonicalSignature（输出）
```

每个 Row 只允许携带：

```text
RequirementId
DeclaredChannel
ApplicabilityState
```

不得加入：

```text
BuildCapabilityKey
RequirementGroupId / role / matchMode
MinimumCapabilityBasisPoints / ValueBasisPoints / threshold / clamp
predicate kind / predicate input / predicate result
event id / event timestamp / event horizon / KnownTrue / KnownFalse
readiness status / band / gap / pass / fail
Encounter / E06 / E10 mapping
玩家文案或 PlayerSafeProjection
```

`EvaluationChannel` 是当前只读视图正在评估的通道；`DeclaredChannel` 是 Row 声明所属通道。`NotInChannel` 只能由二者不相等表达，不是第四种通道。

缺少某个 Row 不自动表示 `Unknown`、`NotApplicable` 或 `NotInChannel`；缺行只表示本 Snapshot 没有对该 Requirement 作出声明。未来完整性规则必须另包定义。

## 6. 状态语义

```text
Applicable
  DeclaredChannel == EvaluationChannel。
  明确参与该 evaluation channel。
  不表示 Met、通过、阈值满足、predicate=true 或 Runtime outcome 成功。

Unknown
  DeclaredChannel == EvaluationChannel。
  适用性或形成适用性所需的稳定证据不完整。
  不得解释为 0、false、Unmet、NotApplicable 或 NotInChannel。

NotApplicable
  DeclaredChannel == EvaluationChannel。
  已有显式、稳定的 pressure/applicability 规则证明该 Requirement 对当前上下文不相关。
  缺事实、未配置、未发生或未观察到不得自动转为 NotApplicable。

NotInChannel
  DeclaredChannel != EvaluationChannel。
  仅表示该 Requirement 不参与当前 evaluation channel。
  不表示 Requirement 在其自身通道不适用，也不表示 KnownZero。
```

本合同不包含 `KnownTrue / KnownFalse`。它们属于未来 structural/runtime 结果合同，不属于 applicability。

## 7. Validator 不变量

Validator 至少必须拒绝：

```text
null input
SchemaId / SchemaVersion 不匹配
空 SnapshotId、外侧空白、隐式 Trim 或大小写归一化替代
未定义 EvaluationChannel
null Row
空 RequirementId、外侧空白、隐式 Trim 或大小写归一化替代
同一 Snapshot 内重复 RequirementId（StringComparer.Ordinal）
未定义 DeclaredChannel
未定义 ApplicabilityState
DeclaredChannel == EvaluationChannel 且 ApplicabilityState == NotInChannel
DeclaredChannel != EvaluationChannel 且 ApplicabilityState != NotInChannel
任何把 Unknown / NotApplicable / NotInChannel 序列化为 0、false、Met 或 Unmet 的附加字段
```

必须允许空 Rows 集合。空集合只表示没有声明任何 Requirement，不得伪造一行 `NotApplicable` 或 `NotInChannel`。

所有字符串身份比较使用 `StringComparer.Ordinal`；所有集合必须防御性复制、只读暴露，并按 RequirementId Ordinal 排序。

## 8. Canonical Signature

格式固定：

```text
sha256: + 64 lowercase hex
```

签名必须覆盖：

```text
SchemaId
SchemaVersion
SnapshotId
EvaluationChannel
排序后的 RequirementId / DeclaredChannel / ApplicabilityState
```

必须满足：

```text
相同内容重复生成 -> 相同签名
仅改变输入 Rows 排列 -> 相同签名
改变 SnapshotId / EvaluationChannel / RequirementId / DeclaredChannel / ApplicabilityState -> 签名变化
输入集合在构造后被外部修改 -> Snapshot 与签名不变
```

本包不建立 player-safe signature；整个合同仅供内部合同与开发诊断使用，不允许玩家侧曝光。

## 9. No-behavior-change fixture

Verifier 只使用通用 dev fixture，不得读取、复制或映射真实 E06/E10 requirement。

固定 fixture：

```text
3 个 EvaluationChannel Snapshot
9 个通用 RequirementId：每个 DeclaredChannel 各 3 个
每个 Snapshot 包含全部 9 个 RequirementId
总计 27 个 Row
同通道 3 行分别覆盖 Applicable / Unknown / NotApplicable
异通道 6 行全部为 NotInChannel
```

ID 只能使用类似：

```text
dev_requirement_channel_fixture_bp_applicable
dev_requirement_channel_fixture_structural_unknown
dev_requirement_channel_fixture_runtime_not_applicable
```

Runtime、fixture 与生成的 Contract Spec 中禁止出现：

```text
真实 E06/E10 RequirementId
capability.placement_shape
capability.debuff_counter
capability.interrupt_timing
capability.spirit_lock
3-10 / 4-10 Encounter ID
```

报告可引用 D1-D6 作为来源说明，但不得生成真实映射行。

Verifier 必须证明：

```text
创建/验证 Snapshot 不调用 E08 evaluator
创建/验证 Snapshot 不读取 E06/E10 catalog
E06/E07/E08/E10 源码与报告基线 byte-identical
C02 32 行与 64 个剩余 Unknown 不发生变化
没有 readiness、predicate、runtime outcome 或 capability value 被生成
```

## 10. 精确文件白名单

任务窗口只允许新增以下 14 个文件：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs.meta
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractReport.md
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractSpec.csv
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFieldMatrix.csv
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFixtureRows.csv
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractLeakCheckReport.md
```

预期新增：`14`。

允许修改已有文件：`0`。

Assignment 由 Guard 写入，不计入任务窗口 14 文件。任务窗口不得修改本 Assignment、Package Queue、CurrentRules、AGENTS 或 LOCKED。

## 11. Protected baseline

继承已验收基线，不允许刷新：

```text
Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
Enemy existing 89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
C01 = fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a
C02 = 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4
C02A = a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb
IF01 = sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428
IF02 = sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673
IF03 = sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a
Affix = sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f
Roll150 = sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8
Projection150 = sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
N01 exact 8 = 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52
C02A-D1 exact 7 = c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0
N01A exact 8 = 705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de
N01A Canonical Signature = sha256:de8f5d758bd8837d88621c33f80dc952c0b7c2dccd7b18f96e71c54649240e79
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

`Enemy existing 89` 必须排除本包白名单新增文件后重新验证；不得用新增 RequirementChannel 文件刷新 Enemy89 基线。

N01/N01A/C02A-D1 exact scope 聚合算法固定：按 `relativePath` Ordinal 排序，每行 `relativePath|lowercaseFileSha256`，UTF-8 编码并保留 LF。

当前工作区存在用户的 Item visual-work 与资源 dirty/untracked 内容。任务窗口必须在开工时记录 pre-existing status，并以 package delta / 精确白名单判断泄漏；不得把这些内容加入本包、删除、覆盖、还原或用于刷新任何 Item 基线。

Item105 gating hash 之外仍须同时满足以下隔离保护：

```text
本包 package delta 严格等于第 10 节 14 文件白名单
本包 package delta 中 Item / Item visual / Item content / Item Config / Item Resource 路径 = 0
RequirementChannel runtime 对 Item namespace / assembly / type / path / reflection token 依赖泄漏 = 0
Pre-existing Item dirty/untracked 已记录并保持，不被本包修改、删除、覆盖、还原、暂存或认领
```

## 12. 绝对禁止

```text
不修改任何已有文件
不修改 E06 PressureWindow requirement snapshots
不修改 E07 BuildCapabilityReadContract
不修改 E08 evaluator、input、result、readiness status 或 player hint
不修改 E10 SeedData、RequirementGroup、阈值、Canonical Signature
不迁移任何真实 requirement
不实现 placement_shape / layout resilience
不实现 debuff_counter / spirit_lock 事实或算法
不实现 interrupt_timing outcome
不新增 Runtime Producer、事件订阅、计时器或状态机
不把 structural/runtime 状态塞入 BP 字段
不把 Applicable 解释为 Met
不把 Unknown / NotApplicable / NotInChannel 解释为 0、false、Unmet 或同一状态
不输出 ReadinessBand、难度、胜负、奖励或掉落结论
不修改 Item / Battle / Board / Scene / Prefab / Config / UI / RectTransform
不修改 RunFlow / SaveData / Reward / Chapter / Boss / BuildSettings
不修改 AGENTS / LOCKED / ROADMAP / CURRENT / Package Queue / CurrentRules
不实现 N02 / PA01 / C02B / C02R1 / C03
不运行 Scene/Prefab Builder
不清理或恢复进入任务前已有 dirty/untracked 文件
不 commit / tag / push
不自动启动下一包
```

## 13. Verifier 与报告验收

必须验证：

```text
SchemaId / SchemaVersion 精确
RequirementChannel 恰好 3 个命名值，数值为 1/2/3
ApplicabilityState 恰好 4 个命名值，数值为 1/2/3/4
所有未定义 enum 数值被拒绝
EvaluationChannel / DeclaredChannel / ApplicabilityState 组合不变量全部通过
Applicable 不等于 Met
Unknown / NotApplicable / NotInChannel 语义隔离
空 Rows 合法且不生成隐式状态
Ordinal identity / duplicate rejection / no trim / case-sensitive lookup
不可变、防御性复制、确定排序
Canonical Signature 格式、确定性、顺序不敏感与字段敏感性
3 Snapshot / 9 RequirementId / 27 Row fixture 精确
真实 E06/E10 ID 与四项 capability key fixture 泄漏 = 0
BP threshold/value、predicate/outcome、readiness/player-safe 字段泄漏 = 0
Runtime 对 E06/E07/E08/E10、Item、Battle、Board、UnityEngine 引用 = 0
现有文件修改 = 0
Protected baseline 前后相同
Item package delta paths = 0
Runtime Item dependency leak = 0
Pre-existing Item dirty preserved / not claimed by package
Package Leak Count = 0
GUID 冲突 = 0
尾随空白 = 0
git diff --check = PASS
```

报告职责：

```text
EnemyRequirementChannelApplicabilitySchemaContractReport.md
  总结合同、枚举、状态语义、fixture、保护与无行为变化结论。

EnemyRequirementChannelApplicabilitySchemaContractSpec.csv
  固定 schema、enum 数值、组合不变量、签名和错误码。

EnemyRequirementChannelApplicabilitySchemaContractFieldMatrix.csv
  ownerType / fieldName / fieldType / cardinality / semantic / visibility / sourceOfTruth / futureConsumer。

EnemyRequirementChannelApplicabilitySchemaContractFixtureRows.csv
  snapshotId / evaluationChannel / requirementId / declaredChannel / applicabilityState / expectedValid / fixtureOnly。

EnemyRequirementChannelApplicabilitySchemaContractLeakCheckReport.md
  逐项列出依赖、真实 ID、能力 key、消费者、Runtime、玩家侧和文件白名单扫描结果；不得只写总数。
```

Offline 与 Unity 入口必须调用同一套断言，不得维护两套规则。

若 Unity Editor 锁阻断 batch：

```text
不得关闭用户 Editor
不得结束 Unity 进程
不得声称 Unity PASS
如实报告 BLOCKED，并保留 Offline verifier 结果
```

本包无需场景或玩家手测。

## 14. 完成回报格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyRequirementChannelApplicabilitySchemaContract01

Guard receipts:
GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01
ENEMY_GUARD_CONFIRM_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01
CAPABILITY_ALGORITHM_GUARD_PASS_ENEMYREQUIREMENTCHANNELAPPLICABILITYSCHEMACONTRACT01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 14
修改已有文件: 必须为 0
SchemaId / Version
Channel enum values: 3/3
Applicability enum values: 4/4
Fixture snapshots / requirement ids / rows: 3 / 9 / 27
Valid / invalid combination checks
Unknown / NotApplicable / NotInChannel isolation
Canonical Signature
E06/E07/E08/E10 behavior change: 必须为 0
Real requirement/capability mapping rows: 必须为 0
N01A actual Unknown reduction: 仍为 0
C02 blocked rows: 仍为 32/32
Protected hashes
Item isolation: exact allowlist PASS / package delta Item paths 0 / runtime Item dependency leak 0
Offline verifier
Unity compile / verifier
Leak Count: 必须为 0
GUID conflicts: 必须为 0
Trailing whitespace: 必须为 0
git diff --check
Forbidden scope touched: 必须为 0
Pre-existing dirty preserved
HEAD unchanged
Commit / tag / push: none
Next package: NOT_STARTED
```

## 15. 后续包顺序

本包通过并经用户确认后，只解除通道合同前置，不自动启动任何包。

顺序固定：

```text
N01B EnemyRequirementChannelApplicabilitySchemaContract01
-> 用户验收 + Guard / RepoOps 收口
-> placement LayoutResilience Structural Predicate Contract（独立包）
-> debuff_counter / spirit_lock 语义事实合同（各自独立，按用户批准模型）
-> interrupt_timing Runtime Event Fact Contract（只建事实合同）
-> Runtime Producer（如需，另经 Battle + Enemy + Cross-System Guard）
-> N02（仅连续 BP 且事实/单位/聚合均已批准的能力）
-> Enemy requirement schema migration / readiness channel consumer（另包，不得混入事实合同）
-> C02B
-> C02R1
-> C03（仅在 C02R1 存在可信可评估结果后）
```

后续包均须单独 Assignment；本包不得预建实现、迁移或接线。
