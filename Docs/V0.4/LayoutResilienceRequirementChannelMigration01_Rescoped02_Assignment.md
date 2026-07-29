# V0.4-LayoutResilienceRequirementChannelMigration01 Rescoped Revision 2 Joint Guard Assignment

## 0. Assignment identity

```text
Package: V0.4-LayoutResilienceRequirementChannelMigration01
Queue Id: N01C-P5-M
Revision: Rescoped02
Package Risk: YELLOW / CROSS_SYSTEM_OVERLAY_ONLY
Package Status: CROSS_SYSTEM_GUARD_ASSIGNED / WAITING_JOINT_GUARD_CONFIRMATION / NOT_STARTED

Cross-System Guard receipt:
GUARD_PASS_ASSIGNMENT_LAYOUTRESILIENCEREQUIREMENTCHANNELMIGRATION01_RESCOPED02

Required Enemy Guard receipt:
ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCEREQUIREMENTCHANNELMIGRATION01

Required Capability Algorithm Guard receipt:
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEREQUIREMENTCHANNELMIGRATION01

Target completion receipt:
GUARD_PASS_LAYOUTRESILIENCEREQUIREMENTCHANNELMIGRATION01
```

本 Assignment 只有在 Enemy Guard 与 Capability Algorithm Guard 两个回执均到齐后才可进入开发。当前 Cross-System Guard 回执不能单独解释为 `READY_FOR_DEV`。

## 1. 前置事实

以下前置已完成并通过 Guard：

```text
N01B  EnemyRequirementChannelApplicabilitySchemaContract01 = QA_PASS
N01C  LayoutResilienceStructuralPredicateContract01 = QA_PASS
P1    LayoutResilienceItemFactProjectionAdapter01 = QA_PASS
P2    AuthoredLayoutPressureSourceAdapter01 = QA_PASS
P3    LayoutResilienceEvaluationInputAssembler01 = QA_PASS
P4    LayoutResilienceStructuralReadinessConsumer01 = QA_PASS
P5-S  LayoutResilienceRequirementMigrationSurvey01 = QA_PASS
P5-A  DevEncounterLayoutPressureAuthoring01 = QA_PASS
User D1-D9 narrowed decisions = APPROVED
User 4 authored pressure maps = ACCEPTED
```

用户验收只授权本包把四个固定 Context 建立为 devOnly overlay route。P5-A 源文件仍保持：

```text
devOnly = true
isEnabled = false
userCoordinateAccepted = false
activated = false
```

本包不得回写或修改这些源字段。用户验收状态由本包独立只读字段 `coordinateBaselineAccepted=true` 记录。

## 2. 本包唯一目标

建立独立 Schema：

```text
LayoutResilienceRequirementChannelMigration.v1
schemaVersion = 1
```

只为以下两个候选、四个已验收 Context 建立 overlay route：

```text
layout_resilience.formation_eye.placement_shape
layout_resilience.polluted_tile.placement_shape
```

每条 route 只完成：

```text
exact candidate identity
+ exact owner/group/key provenance
+ exact Encounter/Map Context
+ exact accepted pressureInputId
+ StructuralPredicate declared/evaluation channel
+ Applicable routing state
+ quarantined legacy BP provenance
→ one immutable devOnly overlay row
```

本包不执行 N01C Evaluator，不生成 P3/P4 结果，不形成总 Readiness，不修改真实 E10 requirement source。

## 3. 明确不做

```text
不修改 E06 / E08 / E10 任何 requirement、group、threshold 或 readiness 逻辑
不修改 Enemy、Item、BuildSandbox、Battle、Board、Map、Config、Scene、Prefab、UI
不把旧 BP 转成结构分数、格子、阈值或 fallback
不由 overlay 直接调用 P3、P4、N01C Validator/Evaluator；P5-A→P2→N01C Validator 的四次权威传递调用不属于 overlay 直连
不接正式 Encounter、正式地图、正式战斗或 UnifiedBattle
不改变 C02 32/32 blocked 状态
不推进 bronze formation general 或 spirit thief
不实现跨通道 Required/Recommended、All/Any 聚合
不启动 RealLayoutResilienceEvaluationPipeline01、N02、PA01、C02B、C02R1、C03
```

## 4. 四条固定 route

Catalog 必须逐行显式作者化以下四条 route；不得按候选、Encounter 或 Map 自动生成。

### Route 1

```text
migrationRouteId = route.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
candidateMigrationRequirementId = layout_resilience.formation_eye.placement_shape
ownerId = dev_enemy_formation_eye_problem
requirementGroupId = dev_enemy_formation_eye_problem.required
role = Required
matchMode = All
referenceKind = BuildCapability
buildCapabilityKey = capability.placement_shape
legacyMinimumCapabilityBasisPoints = 5340
seedId = dev_seed_4_10_furnace_core
encounterId = dev_encounter_4_10_furnace_core
mapRuleId = dev_map_furnace_ash_fall
pressureInputId = pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
```

### Route 2

```text
migrationRouteId = route.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1
candidateMigrationRequirementId = layout_resilience.formation_eye.placement_shape
ownerId = dev_enemy_formation_eye_problem
requirementGroupId = dev_enemy_formation_eye_problem.required
role = Required
matchMode = All
referenceKind = BuildCapability
buildCapabilityKey = capability.placement_shape
legacyMinimumCapabilityBasisPoints = 5340
seedId = dev_seed_4_10_thunder_fire_cross
encounterId = dev_encounter_4_10_thunder_fire_cross
mapRuleId = dev_map_bluestone_crack
pressureInputId = pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1
```

### Route 3

```text
migrationRouteId = route.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1
candidateMigrationRequirementId = layout_resilience.polluted_tile.placement_shape
ownerId = dev_enemy_polluted_tile_problem
requirementGroupId = dev_enemy_polluted_tile_problem.required
role = Required
matchMode = All
referenceKind = BuildCapability
buildCapabilityKey = capability.placement_shape
legacyMinimumCapabilityBasisPoints = 5383
seedId = dev_seed_3_10_cleanse_corner
encounterId = dev_encounter_3_10_cleanse_corner
mapRuleId = dev_map_bluestone_damp
pressureInputId = pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1
```

### Route 4

```text
migrationRouteId = route.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
candidateMigrationRequirementId = layout_resilience.polluted_tile.placement_shape
ownerId = dev_enemy_polluted_tile_problem
requirementGroupId = dev_enemy_polluted_tile_problem.required
role = Required
matchMode = All
referenceKind = BuildCapability
buildCapabilityKey = capability.placement_shape
legacyMinimumCapabilityBasisPoints = 5383
seedId = dev_seed_4_10_furnace_core
encounterId = dev_encounter_4_10_furnace_core
mapRuleId = dev_map_furnace_ash_fall
pressureInputId = pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
```

四条 route 共同冻结：

```text
declaredChannel = StructuralPredicate
evaluationChannel = StructuralPredicate
applicabilityState = Applicable
devOnly = true
isEnabled = false
coordinateBaselineAccepted = true
activated = false
formalRequirementSourceModified = false
behaviorChanged = false
legacyBpQuarantined = true
legacyBpProvenanceVerified = true
legacyBpEvaluated = false
legacyBpConverted = false
legacyBpComparedForCapabilityDecision = false
legacyBpUsedAsThreshold = false
```

`Applicable` 只表示这条 route 在该 Context 应由 StructuralPredicate 通道评估，不表示 Build 已通过、失败、得零分或达到 Readiness。

## 5. N01B overlay 规则

本包必须生成一个 N01B snapshot：

```text
snapshotId = overlay.layout_resilience.requirement_channel_migration.v1
evaluationChannel = StructuralPredicate
rows = 4
```

每行固定为：

```text
RequirementId = migrationRouteId
DeclaredChannel = StructuralPredicate
ApplicabilityState = Applicable
```

`migrationRouteId` 是 overlay 自有的 Context route identity，不是 E10 source requirement id，也不是旧 requirementGroupId。

默认合法路径只允许调用一次：

```text
DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance.CreateSnapshot(input)
```

禁止直接调用 N01B Validator，禁止复制 N01B channel/applicability 校验，禁止将 `NotInChannel` 当成第四通道。

## 6. P5-A 权威来源规则

默认合法路径必须只调用一次：

```text
DevEncounterLayoutPressureValidation.ValidateAndProject(
    DevEncounterLayoutPressureCatalog.CreateCandidateSource())
```

然后按 exact `candidateMigrationRequirementId + seedId + encounterId + mapRuleId + pressureInputId` 匹配四行。

本包不得直接调用 P2 Adapter，也不得复制 P2/N01C 的 board、domain、usable、eye、edge、clause 空间校验。P5-A 行必须为 `CandidateComplete`，P2 status/completeness 必须为 `Complete/Complete`。

调用计数必须区分 overlay 直接调用与权威链路传递调用：

```text
Overlay 直接调用/引用 N01C Validator = 0
P5-A ValidateAndProject → P2 Project = 4 个 Context
P2 Project → N01C Validator 权威传递调用 = 4
N01C Evaluator = 0
P3 / P4 / readiness = 0
```

上述 4 次 N01C Validator 是 P5-A/P2 已验收权威路径的组成部分，不是本 overlay 新增的直接依赖，Verifier 不得把它们误报为 leak。

结果行只读复制 P5-A 已验收事实，至少包括：

```text
pressureKinds
effectiveEyeCell
requiredPredicateClauses
p2CanonicalSignature
p5aCanonicalSignature
```

Formation route clauses 必须精确为：

```text
CountedLayoutPresent
EffectiveEyeAnchorUsable
EyeToCountedCoreStructurallyConnected
```

Polluted route clauses 必须精确为：

```text
CountedLayoutPresent
CountedPlacementCellsUsable
CountedPlacementCoresUsable
```

## 7. 身份与状态合同

Status 精确冻结为：

```text
Complete = 1
Unknown = 2
Invalid = 3
```

未定义 enum 必须 Invalid。优先级：

```text
Invalid > Unknown > Complete
```

身份规则：

```text
CandidateMigrationRequirementId = 2 个机制身份，可跨 Context 复用
MigrationRouteId = 4/4 全局唯一
PressureInputId = 4/4 全局唯一
Context identity = candidate + seed + encounter + map，4/4 唯一
同一 Candidate 的 owner/group/key/role/match/reference/legacy BP 必须稳定
```

缺 source、缺 row、缺 P5-A 行或缺权威 snapshot 为 Unknown。重复、错配、身份漂移、非法 flag、错误 channel/applicability、错误 legacy provenance、未定义 enum 或 N01B 拒绝输入为 Invalid。Unknown/Invalid 不得补零或降级为 Complete。

Legacy BP 只允许做来源一致性核对：

```text
legacyBpProvenanceVerified = exact 5340/5383 与冻结证据一致
legacyBpComparedForCapabilityDecision = false
legacyBpEvaluated = false
legacyBpConverted = false
legacyBpUsedAsThreshold = false
```

来源值漂移判 Invalid 属于 provenance verification，不属于能力通过条件、数值比较或结构阈值。

## 8. Public API

所有集合必须 defensive copy + read-only；不得使用 MonoBehaviour、ScriptableObject、Scene object、正式 Config 或 Runtime registration。

```text
namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RequirementMigrationOverlay

public static class LayoutResilienceRequirementChannelMigrationSchema
public enum LayoutResilienceRequirementChannelMigrationStatus
public sealed class LayoutResilienceRequirementChannelMigrationSource
public sealed class LayoutResilienceRequirementChannelMigrationSourceRow
public sealed class LayoutResilienceRequirementChannelMigrationRowSnapshot
public sealed class LayoutResilienceRequirementChannelMigrationIssue
public sealed class LayoutResilienceRequirementChannelMigrationPayload
public sealed class LayoutResilienceRequirementChannelMigrationResult

public static class LayoutResilienceRequirementChannelMigrationCatalog
  public static LayoutResilienceRequirementChannelMigrationSource CreateOverlaySource()

public static class LayoutResilienceRequirementChannelMigrationValidation
  public static LayoutResilienceRequirementChannelMigrationResult ValidateAndCreateOverlay(
      LayoutResilienceRequirementChannelMigrationSource source)
```

字段合同精确冻结如下；不得添加同义字段、公开 setter 或省略 nullable/集合语义。

```text
LayoutResilienceRequirementChannelMigrationSource
- SchemaId: string
- SchemaVersion: int
- DevOnly: bool
- IsEnabled: bool
- CoordinateBaselineAccepted: bool
- Activated: bool
- Rows: IReadOnlyList<LayoutResilienceRequirementChannelMigrationSourceRow>

LayoutResilienceRequirementChannelMigrationSourceRow
- MigrationRouteId: string
- CandidateMigrationRequirementId: string
- OwnerId: string
- RequirementGroupId: string
- Role: string
- MatchMode: string
- ReferenceKind: string
- BuildCapabilityKey: string
- LegacyMinimumCapabilityBasisPoints: int
- LegacyEvidencePath: string
- LegacyEvidenceRowIdentity: string
- LegacyBpQuarantined: bool
- LegacyBpProvenanceVerified: bool
- LegacyBpEvaluated: bool
- LegacyBpConverted: bool
- LegacyBpComparedForCapabilityDecision: bool
- LegacyBpUsedAsThreshold: bool
- SeedId: string
- EncounterId: string
- MapRuleId: string
- PressureInputId: string
- DeclaredChannel: EnemyRequirementChannel
- EvaluationChannel: EnemyRequirementChannel
- ApplicabilityState: EnemyRequirementApplicabilityState
- DevOnly: bool
- IsEnabled: bool
- CoordinateBaselineAccepted: bool
- Activated: bool
- FormalRequirementSourceModified: bool
- BehaviorChanged: bool

LayoutResilienceRequirementChannelMigrationRowSnapshot
- MigrationRouteId: string
- CandidateMigrationRequirementId: string
- OwnerId: string
- RequirementGroupId: string
- Role: string
- MatchMode: string
- ReferenceKind: string
- BuildCapabilityKey: string
- LegacyMinimumCapabilityBasisPoints: int
- LegacyEvidencePath: string
- LegacyEvidenceRowIdentity: string
- LegacyBpQuarantined: bool
- LegacyBpProvenanceVerified: bool
- LegacyBpEvaluated: bool
- LegacyBpConverted: bool
- LegacyBpComparedForCapabilityDecision: bool
- LegacyBpUsedAsThreshold: bool
- SeedId: string
- EncounterId: string
- MapRuleId: string
- PressureInputId: string
- DeclaredChannel: EnemyRequirementChannel
- EvaluationChannel: EnemyRequirementChannel
- ApplicabilityState: EnemyRequirementApplicabilityState
- DevOnly: bool
- IsEnabled: bool
- CoordinateBaselineAccepted: bool
- Activated: bool
- FormalRequirementSourceModified: bool
- BehaviorChanged: bool
- AuthoringStatus: DevEncounterLayoutPressureAuthoringStatus
- P2Status: AuthoredLayoutPressureSourceStatus
- P2Completeness: LayoutResilienceInputCompleteness
- PressureKinds: IReadOnlyList<LayoutPressureKind>
- EffectiveEyeCell: LayoutCellCoordinate
- RequiredPredicateClauses: IReadOnlyList<LayoutResiliencePredicateClauseKind>
- P2CanonicalSignature: string
- ChannelApplicabilityRow: EnemyRequirementChannelApplicabilityRowSnapshot

LayoutResilienceRequirementChannelMigrationIssue
- Code: string
- Status: LayoutResilienceRequirementChannelMigrationStatus
- Path: string
- Message: string

LayoutResilienceRequirementChannelMigrationPayload
- Rows: IReadOnlyList<LayoutResilienceRequirementChannelMigrationRowSnapshot>
- ChannelApplicabilitySnapshot: EnemyRequirementChannelApplicabilitySnapshot
- P5ACanonicalSignature: string
- N01BCanonicalSignature: string
- FormalRequirementSourceModified: bool
- BehaviorChanged: bool

LayoutResilienceRequirementChannelMigrationResult
- SchemaId: string
- SchemaVersion: int
- Status: LayoutResilienceRequirementChannelMigrationStatus
- DevOnly: bool
- IsEnabled: bool
- CoordinateBaselineAccepted: bool
- Activated: bool
- Payload: LayoutResilienceRequirementChannelMigrationPayload
- Issues: IReadOnlyList<LayoutResilienceRequirementChannelMigrationIssue>
- CanonicalSignature: string
```

输出真值表冻结为：

| Status | Payload | Payload.Rows | N01B Snapshot | Issues |
| --- | --- | ---: | --- | --- |
| `Complete` | 非 null | 精确 4 | 非 null，精确 4 行 | 精确 0 |
| `Unknown` | null | 不存在 | null | 至少 1 条 `Unknown`，且不得含 `Invalid` |
| `Invalid` | null | 不存在 | null | 至少 1 条 `Invalid`；可同时保留稳定 `Unknown` issue |

禁止输出 partial payload。`Complete` 之外不得暴露可消费 rows 或 N01B snapshot；诊断只通过稳定 issue 输出。

N01B provider 抛出 `EnemyRequirementChannelApplicabilityValidationException` 时必须转换为唯一稳定 issue：

```text
Code = N01B_SNAPSHOT_REJECTED
Status = Invalid
Path = n01b
Message = The authoritative N01B provider rejected the overlay input.
```

原始异常 message、issue 文本、类型名、stack trace、时间、机器路径和 Culture 文本均不得进入 Result、Report 或 Canonical。

`CreateOverlaySource()` 每次返回 defensive copy。公开 API 禁止注入 N01B provider、P5-A result、Evaluator、readiness consumer 或其他生产者。

## 9. 允许新增文件：固定 15 个

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay/LayoutResilienceRequirementChannelMigrationPrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay/LayoutResilienceRequirementChannelMigrationPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay/LayoutResilienceRequirementChannelMigrationCatalog.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay/LayoutResilienceRequirementChannelMigrationCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay/LayoutResilienceRequirementChannelMigrationValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay/LayoutResilienceRequirementChannelMigrationValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementChannelMigrationVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementChannelMigrationVerifier.cs.meta
Docs/V0.4/Reports/LayoutResilienceRequirementChannelMigrationReport.md
Docs/V0.4/Reports/LayoutResilienceRequirementChannelMigrationRouteRows.csv
Docs/V0.4/Reports/LayoutResilienceRequirementChannelMigrationChannelRows.csv
Docs/V0.4/Reports/LayoutResilienceRequirementChannelMigrationLegacyProvenance.csv
Docs/V0.4/Reports/LayoutResilienceRequirementChannelMigrationContextBindingMatrix.csv
Docs/V0.4/Reports/LayoutResilienceRequirementChannelMigrationLeakCheckReport.md
```

新增必须为 15，修改已有文件必须为 0。任务窗口不得修改本 Assignment、Queue、CurrentRules、AGENTS 或 LOCKED。

## 10. 验证要求

必须覆盖：

```text
Schema/status exactness = PASS
Default overlay result = Complete
Route rows = 4 exact
Candidate identities = 2 exact
MigrationRouteIds = 4/4 unique
PressureInputIds = 4/4 unique
Context identities = 4/4 unique
N01B snapshot / rows = 1 / 4
N01B rows StructuralPredicate + Applicable = 4/4
P5-A result / rows = CandidateComplete / 4
P5-A ValidateAndProject default-path calls = exactly 1
Direct P2 calls = 0
Overlay direct N01C Validator calls/references = 0
P5-A → P2 → N01C Validator transitive calls = exactly 4
N01B CreateSnapshot default-path calls = exactly 1
Direct N01B Validator calls = 0
N01C Evaluator / P3 / P4 / readiness calls = 0 / 0 / 0 / 0
Legacy BP quarantined = 4/4
Legacy BP provenance verified = 4/4
Legacy BP evaluated/converted/capability-decision-compared/threshold = 0/0/0/0
devOnly/isEnabled = true/false for source, 4 rows and result
coordinateBaselineAccepted/activated = true/false for 4/4
formal source modifications / behavior changes = 0 / 0
Real E10 rows changed = 0
C02 blocked rows / actual Unknown reduction = 32/32 / 0
Ordinal / InvariantCulture / reverse-input determinism = PASS
Defensive copy / immutable collections = PASS
Leak Count / GUID conflicts / trailing whitespace = 0
git diff --check = PASS
Forbidden scope touched = 0
HEAD unchanged
commit / tag / push = none
Next package = NOT_STARTED
```

Offline fixtures必须包括：合法默认、反序、重复构建、null source、缺 row、错误 Schema、错误隔离 flag、未验收坐标、activated=true、错误 route 数、重复 routeId、重复 PressureInputId、重复 Context、owner/group/key 漂移、role/match/reference 漂移、legacy BP 漂移、channel 漂移、Unknown/NotApplicable/NotInChannel 误用、缺 P5-A 匹配。不得通过公开 provider injection 实现测试。

Verifier 入口固定：

```text
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceRequirementChannelMigrationVerifier.VerifyOffline
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceRequirementChannelMigrationVerifier.VerifyStaticBatch
```

两个入口必须使用同一 assertion core；不得运行 Scene/Prefab Builder 或历史回归入口。

## 11. 报告职责

```text
MigrationReport.md
  状态、四 route、P5-A/N01B调用、确定性、边界和后续门。

RouteRows.csv
  四条完整 route identity、Context、pressure、channel、applicability 与隔离 flag。

ChannelRows.csv
  N01B snapshotId、RequirementId、DeclaredChannel、EvaluationChannel、ApplicabilityState。

LegacyProvenance.csv
  5340/5383 原始来源、provenanceVerified=true 与全部 capability-use false flag。

ContextBindingMatrix.csv
  四条 route 与 P5-A exact row、pressure kinds、effective eye、clauses 的绑定。

LeakCheckReport.md
  allowlist、调用次数、E10/P3/P4/Evaluator/readiness/Scene/正式流程和 Protected scope。
```

## 12. Canonical

新包独立 Canonical Signature 必须覆盖：

```text
Schema/status
source/result isolation flags
4 route full identities and provenance
4 context/pressure bindings
N01B snapshot identity/canonical/4 rows
P5-A canonical and selected row facts
pressure kinds/effective eye/clauses/P2 signature
legacy BP quarantine/provenance verification/capability-use flags
Complete/Unknown/Invalid payload truth-table state
sanitized stable N01B rejection issue
issues
zero formal-source and behavior impact
```

编码固定为 UTF-8 无 BOM、每行 LF。文本 Ordinal exact；整数 InvariantCulture decimal；bool lowercase；null、空字符串、空集合必须区分。route 按 migrationRouteId Ordinal；N01B 行按 RequirementId；clauses/kinds 按 enum numeric；issue 按 path、code、status、message Ordinal。输入反序、Culture 变化、重复构建必须同签名；任一 identity、binding、channel、state、flag、legacy provenance、P5-A/N01B canonical 或 issue 变化必须改签名。

## 13. Protected baseline

固定前置：

```text
P5-A exact15 = a4b48cb79328fa77ae952911da6ff906e1a1ccb79b0fef3b87e1e473511e576a
P5-A Canonical = sha256:8b9ee4674020c5405d7b90cf2793b37be32bf0ea08e8a981291ed6790309da54
P5-A Assignment Revision01 = a470d9e6e8c845365fe08eae9b1755b23a1ea8074924baf7dee5bf483c1c52e2
P5-S exact9 = bebd04922410cb83db1690ecd6505d62c58417e151b3b6bc94d7c0774f68a704
P5-S Canonical = sha256:4a77a47181d41ecd18f589f86be911ce08fc466725807a16c42ab1d3e5068126
N01B exact14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
N01B Canonical = sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326
N01C exact16 = 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b
N01C Canonical = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
P2 exact14 = 15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050
P2 Canonical = sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc
P3 exact14 = c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c
P3 Canonical = sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a
P4 exact14 = 8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530
P4 Canonical = sha256:e38538f2de1b85ce00721236790f9b2beab63f19df076a34e0508aad4993c01a
E10 exact21 = 3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be
Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
Enemy89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

`P5-A exact15` 使用与既有 verifier 相同的 aggregate 规则：15 个相对路径 Ordinal 排序，每行 `path|lowercase-file-sha256`，UTF-8，LF，末尾保留 LF，再计算 lowercase SHA-256。

任务开始时还必须以当前磁盘状态记录所有 Protected scope，结束时逐字节复核。不得以 Git HEAD 代替当前磁盘 baseline，不得清理、回退、暂存或认领既有 dirty/untracked 内容。

## 14. 绝对禁止

```text
修改任何已有文件
修改或生成 E06/E08/E10 真实 requirement rows
将 requirementGroupId 当作唯一迁移 identity
推进 bronze formation general 或 spirit thief
修改 P5-A source flags 或坐标
由 overlay 直接调用 P2、P3、P4、N01C Validator/Evaluator/readiness；P5-A→P2→N01C Validator 的 4 次权威传递调用除外
直接调用 N01B Validator或复制其语义
把 Applicable 当作 Passed、KnownTrue、KnownZero或 Readiness
把 legacy BP 5340/5383 用于能力计算、转换、结构阈值、通过条件或 capability decision 比较；仅允许 exact provenance verification
建立跨通道 Required/Recommended、All/Any 聚合
接 Scene/Prefab/Board/Map/Battle/RunFlow/SaveData/Reward/Chapter/BuildSettings
运行会保存 Scene/Prefab 的 Builder或历史回归
commit / tag / push / reset / rollback
自动启动下一包
```

## 15. 完成回传

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-LayoutResilienceRequirementChannelMigration01
Guard receipts
New / modified files
Schema / Status / Canonical Signature
4 exact route rows / 2 candidate IDs
4 unique migrationRouteIds / pressureInputIds / Context identities
P5-A result/call count
N01B snapshot/row count/CreateSnapshot call count
Direct P2/N01B Validator/overlay-direct N01C Validator calls
P5-A→P2→N01C Validator transitive calls
N01C Evaluator/P3/P4/readiness calls
Legacy BP quarantine/provenance verification/capability-use counts
devOnly/isEnabled/coordinateBaselineAccepted/activated
E10/formal source/behavior changes
C02 blocked rows / actual Unknown reduction
Offline / Unity verifier
Protected hashes
Leak/GUID/whitespace/git diff
Forbidden scope
HEAD
Commit/tag/push
Next package status
```

本包通过后也不得自动启动 `RealLayoutResilienceEvaluationPipeline01`。下一包仍需用户明确要求并由 Joint Guard 另行签发。
