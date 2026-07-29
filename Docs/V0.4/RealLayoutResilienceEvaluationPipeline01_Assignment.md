# V0.4-RealLayoutResilienceEvaluationPipeline01 Revision01 Joint Guard Assignment

## 0. Assignment identity

```text
Package: V0.4-RealLayoutResilienceEvaluationPipeline01
Queue Id: N01C-P6
Revision: Revision01
Risk: YELLOW / CROSS_SYSTEM DEVONLY EVALUATION PIPELINE
Status: CROSS_SYSTEM_GUARD_REVISED_1 / WAITING_JOINT_GUARD_REREVIEW / NOT_STARTED

Cross-System Guard receipt:
GUARD_PASS_ASSIGNMENT_REALLAYOUTRESILIENCEEVALUATIONPIPELINE01_REVISION01

Received Item Guard receipt:
ITEM_GUARD_CONFIRM_REALLAYOUTRESILIENCEEVALUATIONPIPELINE01

Required Enemy Guard receipt:
ENEMY_GUARD_CONFIRM_REALLAYOUTRESILIENCEEVALUATIONPIPELINE01

Required Capability Algorithm Guard receipt:
CAPABILITY_ALGORITHM_GUARD_PASS_REALLAYOUTRESILIENCEEVALUATIONPIPELINE01

Target completion receipt:
GUARD_PASS_REALLAYOUTRESILIENCEEVALUATIONPIPELINE01
```

Item Guard 已确认事实边界。只有 Enemy Guard 与 Capability Algorithm Guard 对 Revision01 的两个回执全部到齐后，开发窗口才可开始。本 Assignment 不授权自动启动后续 BattleSandbox 手测、N02、PA01、C02B、C02R1 或 C03。

## 1. 前置事实

以下前置必须保持 `GUARD_ACCEPTED / QA_PASS`：

```text
N01B      EnemyRequirementChannelApplicabilitySchemaContract01
N01C      LayoutResilienceStructuralPredicateContract01
N01C-P1   LayoutResilienceItemFactProjectionAdapter01
N01C-P2   AuthoredLayoutPressureSourceAdapter01
N01C-P3   LayoutResilienceEvaluationInputAssembler01
N01C-P4   LayoutResilienceStructuralReadinessConsumer01
N01C-P5-A DevEncounterLayoutPressureAuthoring01
N01C-P5-M LayoutResilienceRequirementChannelMigration01
```

用户已验收四张 5×5 压力图；P5-M 已建立四条 `devOnly / disabled / accepted / inactive` overlay route。本包第一次把真实合同链串成可计算的独立结构诊断，但仍不接场景、正式 Enemy requirement 或总 Readiness。

## 2. 唯一目标

建立：

```text
RealLayoutResilienceEvaluationPipeline.v1
```

单次输入一份权威 `ItemSystemSnapshot.v1` 与一份 IF01 身份绑定，固定执行：

```text
P5-M 权威 overlay route 验证
+ P1 Item facts 投影
+ 每 route 的 P2 权威压力投影
→ P3 EvaluationInput 装配
→ P4 StructuralPredicate 消费
→ 4 条独立、只读、devOnly route diagnostic
```

它回答的是“当前 Item 布局在某条已验收结构压力下，N01C 谓词为 KnownTrue、KnownFalse、Unknown 还是 NotApplicable”。它不回答最终 Build Ready、Encounter Ready、胜负、难度、奖励或正式章节结果。

## 3. 明确不做

```text
不修改 Item、Enemy、N01B、N01C、P1-P5-M 任一既有文件
不修改 E06/E08/E10 requirement、group、threshold 或 readiness
不接 V0.4 Scene、Board、ItemTray、BattleSandbox runtime、UI 或 RectTransform
不接 V0.2/V0.3、UnifiedBattle、RunFlow、SaveData、Reward、Chapter、Boss 正式流程
不把 legacy 5340/5383 BP 用于计算、阈值、比较、转换或 fallback
不把 occupied cell 数量、形状大小、isLit 或 isCountedInBuild 换算为 BP
不把 KnownTrue 改名为 Ready/Pass；不把 KnownFalse 改名为 Failed/Unready
不把 Unknown、NotApplicable 或 Invalid 写成 KnownZero、false、0 或 Unmet
不执行跨通道 Required/Recommended、All/Any 聚合
不改变 C02 的 32/32 blocked 状态，不声明 Unknown reduction
不运行 Scene/Prefab Builder、历史 UI 回归或任何写资源入口
```

## 4. 固定 Schema 与公共合同

```text
schemaId = RealLayoutResilienceEvaluationPipeline.v1
schemaVersion = 1
```

Status 恰好为：

```csharp
Complete = 1
Unknown = 2
Invalid = 3
```

`0` 与未定义枚举值一律 Invalid。必须建立以下公共类型：

```text
RealLayoutResilienceEvaluationPipelineSchema
RealLayoutResilienceEvaluationPipelineStatus
RealLayoutResilienceEvaluationPipelineInput
RealLayoutResilienceEvaluationPipelineIssue
RealLayoutResilienceEvaluationRouteRowSnapshot
RealLayoutResilienceEvaluationPipelineResult
IRealLayoutResilienceEvaluationPipeline
DefaultRealLayoutResilienceEvaluationPipeline
RealLayoutResilienceEvaluationPipelineValidationCodes
DefaultRealLayoutResilienceEvaluationPipelineValidator
```

公共签名精确冻结为：

```csharp
public interface IRealLayoutResilienceEvaluationPipeline
{
    RealLayoutResilienceEvaluationPipelineResult Evaluate(
        RealLayoutResilienceEvaluationPipelineInput input);
}

public sealed class DefaultRealLayoutResilienceEvaluationPipeline
    : IRealLayoutResilienceEvaluationPipeline
{
    public static readonly DefaultRealLayoutResilienceEvaluationPipeline Instance;
    public DefaultRealLayoutResilienceEvaluationPipeline();
    public RealLayoutResilienceEvaluationPipelineResult Evaluate(
        RealLayoutResilienceEvaluationPipelineInput input);
}

public sealed class DefaultRealLayoutResilienceEvaluationPipelineValidator
{
    public static readonly DefaultRealLayoutResilienceEvaluationPipelineValidator Instance;
    public IReadOnlyList<RealLayoutResilienceEvaluationPipelineIssue> ValidateResult(
        RealLayoutResilienceEvaluationPipelineResult result);
}
```

公开构造与 `Evaluate` 不得接收或暴露任意 P1/P2/P3/P4、overlay、Evaluator、Validator、provider、delegate、service locator 或 setter。生产路径硬绑定本 Assignment 指定的权威 `Default*.Instance`。如 Verifier 需要异常探针，只允许 internal/private test seam；它不得成为第二个成功算法或 public bypass。

Input 顶层仅允许：

```text
SchemaId
SchemaVersion
EvaluationBatchId
ItemSnapshot        // ItemSystemSnapshot.v1，可 null
BindingSnapshot     // IF01，可 null；I031-only / 空布局可按 P1 权威语义缺省
```

Input 构造与属性精确冻结为：

```csharp
public RealLayoutResilienceEvaluationPipelineInput(
    string evaluationBatchId,
    ItemSystemSnapshot itemSnapshot,
    ItemInstancePlacementBindingContractSnapshot bindingSnapshot,
    string schemaId = RealLayoutResilienceEvaluationPipelineSchema.SchemaId,
    int schemaVersion = RealLayoutResilienceEvaluationPipelineSchema.SchemaVersion);

public string SchemaId { get; }
public int SchemaVersion { get; }
public string EvaluationBatchId { get; }
public ItemSystemSnapshot ItemSnapshot { get; }
public ItemInstancePlacementBindingContractSnapshot BindingSnapshot { get; }
```

不得让调用方传入自造 overlay、P2 result、P3 result、P4 result、Evaluator、Validator、route 或 readiness provider。

Result 顶层固定：

```text
SchemaId
SchemaVersion
Status
DevOnly             // 恒 true
IsEnabled           // 恒 false
EvaluationBatchId
BuildProjectionResultPresent: bool
BuildProjectionStatus: LayoutResilienceItemFactProjectionStatus?
BuildFactsCompleteness: LayoutResilienceInputCompleteness?
BuildProjectionCanonicalSignature: string
OverlayResultPresent: bool
OverlayCanonicalSignature: string
Rows: IReadOnlyList<RealLayoutResilienceEvaluationRouteRowSnapshot>
Issues: IReadOnlyList<RealLayoutResilienceEvaluationPipelineIssue>
CanonicalSignature: string
```

Issue 恰好四个只读字段：

```text
Code: string
Status: RealLayoutResilienceEvaluationPipelineStatus   // 仅 Unknown 或 Invalid
Path: string
Message: string
```

Issue 的 Code、Path、Message 必须非空；Complete issue、null issue、未定义 Status 均为非法。

每条 `RealLayoutResilienceEvaluationRouteRowSnapshot` 固定包含：

```text
MigrationRouteId: string
CandidateMigrationRequirementId: string
OwnerId: string
RequirementGroupId: string
SeedId: string
EncounterId: string
MapRuleId: string
PressureInputId: string
DeclaredChannel: EnemyRequirementChannel
EvaluationChannel: EnemyRequirementChannel
ApplicabilityState: EnemyRequirementApplicabilityState
CoordinateBaselineAccepted: bool
Activated: bool
Status: RealLayoutResilienceEvaluationPipelineStatus
P2ResultPresent: bool
P2Status: AuthoredLayoutPressureSourceStatus?
P2Completeness: LayoutResilienceInputCompleteness?
P2CanonicalSignature: string
P3ResultPresent: bool
P3Status: LayoutResilienceEvaluationInputAssemblerStatus?
P3CanonicalSignature: string
P4ResultPresent: bool
P4Status: LayoutResilienceStructuralReadinessConsumerStatus?
P4CanonicalSignature: string
P4ResultValidated: bool
PredicateResult: LayoutResiliencePredicateResultSnapshot  // 防御性复制，可 null
```

`PredicateResult` 必须完整保留 `EvaluationId / ApplicabilityState / PredicateState / ClauseRows / N01C CanonicalSignature`。不得添加 score、BP、ratio、IsReady、IsFailed、difficulty、reward 或 cross-channel summary。

## 5. 四条 route 与身份匹配

仅允许 P5-M 已验收的四条 route，按 `MigrationRouteId` Ordinal 排序：

```text
route.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
route.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1
route.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1
route.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
```

P5-M route 与 P5-A source 必须按以下完整 tuple 精确匹配：

```text
candidateMigrationRequirementId
+ ownerId
+ requirementGroupId
+ seedId
+ encounterId
+ mapRuleId
+ pressureInputId
```

禁止按 group-only、candidate-only、中文名、数组下标或相似字符串匹配。重复、缺失、错配或身份漂移均为 Invalid。

## 6. 唯一权威调用链

正常 Complete 输入每次调用固定为：

```text
1. LayoutResilienceRequirementChannelMigrationValidation.ValidateAndCreateOverlay(
       LayoutResilienceRequirementChannelMigrationCatalog.CreateOverlaySource())
   恰好 1 次

2. DefaultLayoutResilienceItemFactProjectionAdapter.Instance.Project(
       ItemSnapshot, BindingSnapshot)
   恰好 1 次

3. DevEncounterLayoutPressureCatalog.CreateCandidateSource()
   恰好 1 次，只读取四条显式作者化 source row

4. DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(
       matchedAuthoringRow.PressureSource)
   每 route 恰好 1 次，共 4 次

5. DefaultLayoutResilienceEvaluationInputAssembler.Instance.Assemble(...)
   每 route 恰好 1 次，共 4 次

6. DefaultLayoutResilienceStructuralReadinessConsumer.Instance.Consume(...)
   每 route 恰好 1 次，共 4 次

7. DefaultLayoutResilienceStructuralReadinessConsumerValidator.Instance
       .ValidateResult(p4Result)
   每 route 恰好 1 次，共 4 次；验证通过后才可复制 PredicateResult
```

P5-M 第 1 步内部已有：

```text
P5-A ValidateAndProject = 1
P2 Project = 4
N01C input Validator = 4
N01B CreateSnapshot = 1
```

第 4 步额外 4 次 P2 Project 是为获得 P3 所需的权威 `AuthoredLayoutPressureSourceResult`，不是 fallback 或第二套算法。每条重新投影的 P2 Canonical Signature 必须与 P5-M row 保存的 `P2CanonicalSignature` 精确相同，否则 Invalid。

P3 join 精确冻结为：

```text
P3 EvaluationId = EvaluationBatchId + "|" + MigrationRouteId
P3 RequirementId = MigrationRouteId
P3 ApplicabilitySnapshot = P5-M Payload.ChannelApplicabilitySnapshot
```

每个 `MigrationRouteId` 必须以 Ordinal exact 在该 N01B Snapshot 中命中恰好一行；该行 `RequirementId / DeclaredChannel / ApplicabilityState` 必须与 P5-M route 精确一致。不得为单条 route 重建 mini snapshot，不得用 candidate/group/pressure ID 替代 RequirementId，不得 fallback 到第一行或数组下标。

正常 Complete 输入的可观察调用矩阵必须是：

```text
P5-M ValidateAndCreateOverlay              1
P5-A ValidateAndProject (transitive)       1
P1 Project                                1
P2 Project total                          8  // overlay 4 + evaluation 4
N01B CreateSnapshot                       1
P3 Assemble                               4
P4 Consume                                4
N01C Evaluator                            4
N01C input Validate                      16  // P5-M→P2 4 + evaluation P2 4 + P3 4 + Evaluator 4
P3 ValidateResult                         8  // P3 internal 4 + P4 source gate 4
N01C ValidateResult                       8  // Evaluator internal 4 + P4 explicit gate 4
P4 Result Validator                       4  // Pipeline copy gate
Total readiness aggregation               0
Formal requirement write                  0
```

第 7 步若收到 null、抛出异常、返回 null issue collection、返回任一 issue，或发现 P4 schema/status/payload/canonical/enum 畸形，则该 route 固定为 Invalid，`P4ResultValidated=false`，且该 row 的 `PredicateResult=null`。不得复制、保留或修复被拒绝的 PredicateResult；不得 retry、降级 Unknown 或绕过 P4 Result Validator。

不得反造 P2 Result、复制 P2/P3/P4/N01C 空间算法、重算 overlay route、retry、fallback 或选择第二实现。

## 7. EvaluationId 与状态传播

每条 P3 `EvaluationId` 固定为：

```text
EvaluationBatchId + "|" + MigrationRouteId
```

Ordinal exact，不 Trim、不改大小写。空白 `EvaluationBatchId` 为 Invalid。

聚合优先级：

```text
Invalid > Unknown > Complete
```

固定规则：

| 条件 | Pipeline Status | Rows |
|---|---|---|
| null input、Schema mismatch、空 EvaluationBatchId、未定义枚举 | Invalid | 0 |
| P5-M 不是合法 Complete/4 rows 或 canonical/身份漂移 | Invalid | 0 |
| P1 Invalid | Invalid | 0 |
| P1 Complete/Unknown，四条权威 route 可继续评估 | 由四条 row 聚合 | 4 |
| 任一 P2/P3/P4 Invalid、P4 Result Validator 拒绝或 canonical/身份不一致 | Invalid | 4 条诊断可保留，但被拒绝 row 的 PredicateResult 必须为 null |
| 无 Invalid，至少一条 P4 Unknown | Unknown | 4 |
| 四条 P4 Complete | Complete | 4 |

`KnownFalse` 是合法 Complete route diagnostic，不升级为 Pipeline Invalid。合法空布局可以形成 Complete + KnownFalse，不得解释为输入缺失。P1 Unknown 必须沿 P3/P4 权威语义保留 Unknown，不能补零或猜测。

Result 合法组合精确冻结为：

| Pipeline Status | Rows | Issues | 合法 row 组合 |
|---|---:|---|---|
| Complete | 4 | 0 | 4 条 row 均 Complete；P2/P3/P4 present；`P4ResultValidated=true`；PredicateResult 非 null 且仅 KnownTrue/KnownFalse |
| Unknown | 4 | 至少 1 条 Unknown，0 条 Invalid | row 仅 Complete/Unknown，且至少一条 Unknown；所有 present P4 都必须通过 Result Validator；Unknown row 的 PredicateResult 仅允许 null 或 PredicateState=Unknown |
| Invalid（前置门禁） | 0 | 至少 1 条 Invalid | 不得伪造 route row 或 PredicateResult |
| Invalid（route 阶段） | 4 | 至少 1 条 Invalid | 至少一条 Invalid row；任何 P2/P3/P4 缺失或被拒绝必须以 presence/validated 字段表达；对应 PredicateResult 必须 null |

禁止 1-3 条 partial rows。Pipeline Invalid 可以同时保留稳定 Unknown issue，但必须至少包含一条 Invalid issue。P4 Complete + NotApplicable 在本包为 Invalid，因为四条 P5-M route 固定 `Applicable`；本包不得借 NotApplicable 绕过结构评估。

单条 row 合法组合固定：

```text
Complete:
  P2/P3/P4 present=true
  P2/P3/P4 status=Complete
  P4ResultValidated=true
  PredicateResult=KnownTrue or KnownFalse

Unknown:
  P2/P3/P4 present=true
  P4ResultValidated=true
  P4 status=Unknown
  PredicateResult=null or Unknown

Invalid:
  任一 authority result 缺失、畸形、异常、拒绝或身份/canonical 漂移
  P4 未产生时 P4ResultPresent=false / P4ResultValidated=false
  P4 被 Result Validator 拒绝时 P4ResultPresent=true / P4ResultValidated=false
  PredicateResult=null
```

所有 nullable upstream status/completeness 字段必须遵守：

```text
ResultPresent=false → 对应 Status/Completeness=null，CanonicalSignature=""
ResultPresent=true  → 对应 Status 为已定义值，CanonicalSignature 为合法 sha256
P4ResultValidated=true → P4ResultPresent=true 且 P4 Result Validator 返回空 issue 集合
P4ResultValidated=false → PredicateResult=null
```

Pipeline 自身 `DefaultRealLayoutResilienceEvaluationPipelineValidator.ValidateResult` 只验证 Pipeline DTO 的上述形状、不可变性和 Pipeline Canonical，不得再次调用 P1/P2/P3/P4/N01C 或任何上游 Result Validator。正常调用矩阵中的 P4 Result Validator 固定为 Pipeline 生产阶段逐 route 的 4 次，不得因 Pipeline 自验再增加到 8 次。

## 8. Canonical 与不可变性

格式固定：

```text
sha256: + 64 lowercase hex
```

Canonical 必须覆盖 Input presence/schema/id、P1 status/completeness/signature、P5-M signature、完整 route identity、P2/P3/P4 status/signature、完整 PredicateResult/ClauseRows、Pipeline Status 与 issues。

排序固定：

```text
route rows: MigrationRouteId Ordinal
clause rows: ClauseKind numeric ascending
issues: Path Ordinal → Code Ordinal → Message Ordinal
整数: InvariantCulture decimal
bool/presence: lowercase true/false
```

null 与空集合必须产生不同 payload。所有集合 defensive copy + read-only。异常只输出稳定 code/message，不得写 exception message、stack trace、时间、线程、随机数、本地路径或 runtime type identity。

## 9. Offline fixtures 与验收

Editor verifier 使用 `DefaultItemSystemSnapshotProvider` 生成 ItemSystemSnapshot，并只通过 P1 已提供的 fixture binding helper 构造 IF01 测试绑定；不得手工构造 P1/P2/P3/P4 成功 Result。

固定至少覆盖 7 个 pipeline scenario：

```text
1. 完整空布局：4 rows，Complete，4 条结构结果为 KnownFalse
2. 阵眼连接保持布局：formation_eye 至少 1 条 KnownTrue
3. 阵眼连接被切布局：formation_eye 至少 1 条 KnownFalse
4. 避开污染格布局：polluted_tile 至少 1 条 KnownTrue
5. 压到污染格布局：polluted_tile 至少 1 条 KnownFalse
6. 普通 placement 缺 IF01 binding：4 rows，Unknown，不得补零
7. 非法 Item snapshot 或身份错配：Invalid，0 evaluation rows
```

正常成功场景 route rows 合计固定为 `24`（前 6 个场景各 4；第 7 个提前 Invalid）。还必须覆盖：

```text
四条 route 4/4 命中且顺序稳定
两类 candidate 均出现 KnownTrue 与 KnownFalse
P2 re-projection canonical 4/4 与 P5-M row 一致
P3 RequirementId 4/4 等于 MigrationRouteId，且在同一 P5-M N01B Snapshot 中 Ordinal 唯一命中
正常 Complete 场景调用矩阵精确为 N01C Validate 16 / P3 ValidateResult 8 / N01C ValidateResult 8 / P4 Result Validator 4
每条 P4 输出必须先经权威 P4 Result Validator；验证通过前 PredicateResult 不得复制
P4 Result Validator 的 null、异常、issue、undefined enum、shape/canonical 拒绝 → route Invalid + PredicateResult null
重复调用、输入集合反序、Culture 改变 → 同结果同签名
改变 Item placement、rotation、occupied facts、route identity、P2/P3/P4 canonical 或任一 clause → 签名变化
结果集合不可变，调用后修改 fixture source 不影响结果
Undefined enum / malformed upstream disposition → Invalid
C02 blocked rows = 32/32；Actual Unknown reduction = 0
E10/Scene/Prefab/BuildSettings/Item/Enemy 既有文件修改 = 0
```

Unity batch 入口固定为：

```text
TalismanBag.EditorTools.CrossSystem.ItemEnemy.RealLayoutResilienceEvaluationPipelineVerifier.VerifyStaticBatch
```

同时提供 `VerifyOffline`。两者必须验证相同 truth table、调用计数、签名与 protected baseline。

## 10. 精确文件白名单

只允许新增以下 15 个文件；修改已有文件必须为 0：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelinePrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelinePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipeline.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipeline.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelineValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline/RealLayoutResilienceEvaluationPipelineValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs.meta
Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineReport.md
Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineRouteRows.csv
Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineScenarioMatrix.csv
Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineAuthorityCallMatrix.csv
Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineTruthTable.csv
Docs/V0.4/Reports/RealLayoutResilienceEvaluationPipelineLeakCheckReport.md
```

本 Assignment 不计入 15 个任务文件。开发窗口不得修改 Assignment、Queue、CurrentRules、AGENTS、LOCKED 或任何前置报告。

## 11. Protected baseline

以下值在任务开始时必须精确匹配，结束时必须保持不变：

```text
P5-M exact15 = 5e009263d68d470847fa9c0d151a64373d4d206b869f01736bef43abc363f302
P5-M Canonical = sha256:a8d40066b7d7cbaecc85e0b427dda5c5aa0ddfb209613c29fea984d3fb52bad1
P5-A exact15 = a4b48cb79328fa77ae952911da6ff906e1a1ccb79b0fef3b87e1e473511e576a
P1 exact14 = 7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48
P1 Canonical = sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd
P2 exact14 = 15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050
P2 Canonical = sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc
P3 exact14 = c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c
P3 Canonical = sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a
P4 exact14 = 8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530
P4 Canonical = sha256:e38538f2de1b85ce00721236790f9b2beab63f19df076a34e0508aad4993c01a
N01C exact16 = 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b
N01C Canonical = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
N01B exact14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
N01B Canonical = sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326
Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
Enemy89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
E10 exact21 = 3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

Verifier 必须先记录当前磁盘 protected hash，再执行纯内存验证，最后复核。不得把 Git HEAD 当作 dirty workspace 的恢复基线，不得清理、回退或认领既有 dirty/untracked 文件。

## 12. 完成回传

开发完成后只回传：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-RealLayoutResilienceEvaluationPipeline01
Guard receipts: 四项完整列出
新增文件 / 修改已有文件: 15 / 0
Pipeline scenarios / route rows: 7 / 24
P1 / P2 total / P3 / P4 calls: 1 / 8 / 4 / 4 per Complete scenario
N01C Validate / P3 ValidateResult / N01C ValidateResult / P4 Result Validator: 16 / 8 / 8 / 4
P3 RequirementId and N01B Ordinal join: 4/4 PASS
P4 Result Validator acceptance: 4/4 PASS
KnownTrue / KnownFalse / Unknown / NotApplicable distribution
P2 canonical match: 4/4
Canonical Signature
Offline verifier
Unity compile / verifier
Protected hashes
Leak Count
git diff --check
Forbidden scope touched
HEAD unchanged
commit / tag / push: none
Next package: NOT_STARTED
```

## 13. 后续门禁

本包通过后只允许提出下一个联合 Assignment：

```text
V0.4-LayoutResilienceBattleSandboxPlaytestAdapter01
```

该后续包才负责把 V0.4 棋盘的只读 `ItemSystemSnapshot + IF01` 接到本 Pipeline 并显示 devOnly 结构反馈。当前包不得提前接 Scene、UI 或 BattleSandbox runtime。

N02、PA01、C02B、C02R1、C03 与正式 UnifiedBattle 继续 HOLD。
