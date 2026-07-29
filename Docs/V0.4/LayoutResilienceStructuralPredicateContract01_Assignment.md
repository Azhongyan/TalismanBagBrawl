# V0.4-LayoutResilienceStructuralPredicateContract01 Assignment

Joint Guard receipts:

```text
GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01
ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01
ITEM_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01
```

生成日期：2026-07-19

所属：Overall / Cross-System Guard + Enemy Guard + Item Guard + Capability Algorithm Guard

包序号：N01C

## 1. 包定位

本包只建立 `LayoutResilienceStructuralPredicate.v1` 中立合同：在一个独立作者化、已经完全物化的地图压力快照上，对稳定的 Item 几何与摆放事实进行确定性的纯数据结构谓词求值。

本包拥有：

```text
布局韧性输入、压力快照、输入完整性与谓词结果的中立类型
KnownTrue / KnownFalse / Unknown / NotApplicable 的严格状态机
完整输入上的确定性集合比较与连通性判定
Validator、Canonical Signature、dev fixture 与开发诊断报告
```

本包不拥有：

```text
Item 事实生产、摆放算法、点亮算法或 Build 计数算法
地图压力内容、地图坐标变换、Board/Map 模拟或压力 Producer
Enemy requirement、阈值、分组、迁移或 readiness 消费
Battle Runtime、Runtime Producer、UI、场景、存档、奖励或主流程
```

风险等级：`YELLOW / ISOLATED STRUCTURAL CONTRACT ONLY`。

## 2. 已批准语义与前置合同

用户已在 `Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md` 第十九节批准 D1、D5、D6：

```text
D1: placement_shape 是受配置地图压力后的布局韧性结构谓词。
    压力可包含污染格、阵眼移动/干扰与断线。
    禁止按占格数量、紧凑度或单件尺寸换算 BP。
D5: readiness 按通道分别汇总；side-channel Unknown 只抑制本通道。
D6: 允许建立 requirementChannel / applicability 合同；迁移和消费者修改必须另包。
```

直接前置：

```text
EnemyRequirementChannelApplicability.v1
BuildCapabilityRemainingBlockerSemanticSurvey01
ItemInstancePlacementBindingContractSnapshot.v1（仅身份来源证据，不在本包接线）
ItemSystemSnapshot（仅现有只读事实来源）
```

N01B 已通过 Guard 与 QA：`245/245 PASS`。本包必须复用 N01B 的 `EnemyRequirementApplicabilityState`，不得复制或改写 applicability 语义，也不得修改 BP-only `BuildCapabilityReadContract.v1`。

## 3. 启动必读与只读证据

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
Docs/V0.4/EnemyRequirementChannelApplicabilitySchemaContract01_Assignment.md
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticMatrix.csv
Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSourceEvidence.csv
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractReport.md
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractSpec.csv
Docs/V0.4/LayoutResilienceStructuralPredicateContract01_Assignment.md
```

只读检查：

```text
Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs
Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation/**
```

## 4. 三方所有权收口

### 4.1 Item Guard：只读事实资格

唯一合格的现有 Item 输入事实为：

```text
ItemSystemCatalogItemSnapshot.ShapeCells
ItemSystemPlacementSnapshot.placementId
ItemSystemPlacementSnapshot.itemId
ItemSystemPlacementSnapshot.anchorCell
ItemSystemPlacementSnapshot.rotation
ItemSystemPlacementSnapshot.OccupiedCells
ItemSystemPlacementSnapshot.coreCellWorld
ItemSystemPlacementSnapshot.isLit
ItemSystemPlacementSnapshot.isCountedInBuild
ItemSystemSnapshot.boardSize
ItemSystemSnapshot.eyeCell
```

资格边界：

```text
ShapeCells 是本地形状事实，不是能力值。
OccupiedCells 是已摆放后的世界格事实，不是 BP、面积分或韧性分。
eyeCell / coreCellWorld 是结构坐标，不是阈值。
isLit / isCountedInBuild 只决定该摆放是否属于战前有效 Build 主体。
未摆放 Item 不产生 placement row；不得伪造为 false 或 zero。
未点亮且未计入 Build 的 placement 可以保留为来源事实，但不参与谓词主体。
isCountedInBuild == true 且 isLit == false 是事实矛盾，必须 Invalid，不是 KnownFalse。
缺失或不完整的 Item 输入必须标记 Incomplete 并得到 Unknown，不得补零。
```

本包不得修改、重新生成或反射读取 Item 私有状态；不得以 `shapeId`、名称、标签、文案、占格数量或紧凑度替代上述事实。

### 4.2 Enemy Guard：压力与 applicability

Enemy planning meaning 固定为：

```text
在污染格、阵眼移动/干扰、阵位破坏或结构连接切断等已配置压力下，
Build 是否仍保留可用的 counted-in-Build 布局、核心格可用性与阵眼锚点到核心的已声明结构连接。
```

现有 E03 MapRule、E06 pressure、Enemy vocabulary、SeedData 与 `pressure.placement_formation` 只提供规划语义和压力归类；它们不提供完整坐标 mask、有效阵眼坐标或切断后的连接图。现有 pressure BP（包括任何 2500 等值）不具备结构谓词输入资格，不得转用为阈值、权重或结果。

不得从 `mapRuleId`、pressure tag、display name、LegacyFacts、中文文案、placementModifier 或 BP requirement 推导结构变换。N01C 只能定义新的独立压力 Snapshot 和 synthetic fixture，不接任何真实 MapRule/Encounter。

地图压力必须作为独立的、调用方作者化的 `LayoutPressureSnapshot` 输入：

```text
PressureInputId
PressureKinds
LayoutDomainCells
UsableCellsAfterPressure
EffectiveEyeAnchorCellAfterPressure
PreservedStructuralConnectionsAfterPressure
RequiredPredicateClauses
```

压力类型只允许三个命名有效值：

```csharp
PollutedCellMask = 1
EyeRelocationOrDisruption = 2
StructuralConnectionCut = 3
```

一个压力快照可以组合多个类型。类型只说明作者化来源；求值只消费完整物化后的 layout domain、cell mask、有效阵眼锚点、结构连接图和显式 RequiredPredicateClauses，不从类型名称推导坐标变换。

阵眼在本合同中只是布局/规则锚点，不是正式能量源。阵眼可达性只表示 `PreservedStructuralConnectionsAfterPressure` 已声明的结构连接连续性；不得输出或推断 Item `Powered`、点亮结果、FormationEnergy 或正式能量供应结论。未来 Adapter 只有在其自身权威合同和独立 Assignment 已批准后，才可以物化该中立结构连接图。

Applicability 必须复用 N01B：

```text
Applicable: 本结构通道明确参与；不等于通过。
Unknown: applicability 证据不完整；结果只能 Unknown。
NotApplicable: 显式稳定规则证明当前上下文没有布局压力；结果只能 NotApplicable。
NotInChannel: 本合同不得创建结果；输入必须被 Validator 拒绝。
```

空压力列表、未配置或缺少坐标事实不得自动解释为 NotApplicable。

### 4.3 Capability Algorithm Guard：谓词与确定性

本包只对已经作者化的不可变纯数据快照求值。允许：集合包含判断，以及在完整、已物化、无向的中立结构连接图上做阵眼锚点到核心的确定性可达性判断。

这不等于地图模拟。以下操作全部禁止：

```text
读取或驱动 Board/Map/Scene/RectTransform
从 MapDebuff、Enemy skill、pressure BP 或机制词汇生成 mask
移动、旋转、搜索、重排或优化任何 Item placement
调用现有摆放、点亮、战斗、计时、事件或寻路 Producer
改变压力随时间的状态，或运行任何 Battle tick
```

## 5. Schema、枚举与中立数据形状

固定：

```text
schemaId = LayoutResilienceStructuralPredicate.v1
schemaVersion = 1
```

`LayoutResiliencePredicateState` 恰好四个命名有效值：

```csharp
KnownTrue = 1
KnownFalse = 2
Unknown = 3
NotApplicable = 4
```

`LayoutResilienceInputCompleteness` 恰好三个命名有效值：

```csharp
Complete = 1
Incomplete = 2
NotRequired = 3
```

`LayoutResiliencePredicateClauseKind` 恰好五个命名有效值：

```csharp
CountedLayoutPresent = 1
CountedPlacementCellsUsable = 2
CountedPlacementCoresUsable = 3
EffectiveEyeAnchorUsable = 4
EyeToCountedCoreStructurallyConnected = 5
```

`LayoutResiliencePredicateClauseState` 恰好两个命名有效值：

```csharp
Satisfied = 1
Violated = 2
```

所有枚举的数值 `0` 与任意未定义整数必须被拒绝。不得新增 `Invalid` 结果值；Invalid 是验证失败，不能序列化成业务结果。

中立 Snapshot/Input 必须建立等价语义：

```text
LayoutCellCoordinate                 // int X, int Y；不依赖 UnityEngine.Vector2Int
LayoutCellConnection                 // 无向、端点规范化的两个 cell
LayoutResiliencePlacedItemFactSnapshot
LayoutResilienceBuildFactSnapshot
LayoutPressureSnapshot
LayoutResilienceEvaluationInput
LayoutResiliencePredicateClauseSnapshot
LayoutResiliencePredicateResultSnapshot
LayoutResilienceValidationIssue / ValidationException
ILayoutResilienceStructuralPredicateValidator
ILayoutResilienceStructuralPredicateEvaluator
DefaultLayoutResilienceStructuralPredicateValidator
DefaultLayoutResilienceStructuralPredicateEvaluator
```

Input 顶层只允许：

```text
SchemaId / SchemaVersion
EvaluationId
ApplicabilityState
BuildFactsCompleteness
PressureFactsCompleteness
BuildFacts
Pressure
```

Build facts 只允许：

```text
BoardSize
BaselineEyeCell
PlacementRows
```

`BoardSize` 与 `BaselineEyeCell` 必须无损投影自 `ItemSystemSnapshot.boardSize / eyeCell`。本包不得读取 Board runtime；`BoardSize` 仅用于中立整数坐标的合同越界验证。

Placed Item row 只允许：

```text
PlacementId / ItemId
AnchorCell / Rotation
ShapeCells / OccupiedCells
CoreCellWorld
IsLit / IsCountedInBuild
```

压力输入必须与 Item 身份独立；`LayoutPressureSnapshot` 不得携带 ItemId、PlacementId、BuildCapabilityKey、RequirementId、EncounterId、mapRuleId、pressure tag、display name、LegacyFacts、中文文案、placementModifier 或 BP requirement。

Result 只允许：

```text
SchemaId / SchemaVersion
EvaluationId
ApplicabilityState
PredicateState
ClauseRows
CanonicalSignature
```

禁止任何 BP、score、ratio、threshold、weight、clamp、readiness、difficulty、pass band、reward 或 player-safe 字段。

## 6. 完整性与状态真值表

完整性是调用方对来源快照的显式声明，不得根据空集合猜测：

```text
Complete:
  声明本次求值所需的该侧事实已全部提供；若必要字段仍缺失，Validator 返回 Invalid。
Incomplete:
  来源事实缺失或压力尚未完整作者化；可以保留部分诊断 payload，但 evaluator 不得读取它来产生 false。
NotRequired:
  只允许在 applicability == NotApplicable 时使用；payload 必须为空。
```

固定真值表：

| Applicability | Build completeness | Pressure completeness | Result |
|---|---|---|---|
| Applicable | Complete | Complete | 运行第 7 节纯谓词，得到 KnownTrue 或 KnownFalse |
| Applicable | Incomplete | Complete/Incomplete | Unknown |
| Applicable | Complete | Incomplete | Unknown |
| Unknown | Complete/Incomplete | Complete/Incomplete | Unknown，不运行空间谓词 |
| NotApplicable | NotRequired | NotRequired | NotApplicable，不运行空间谓词 |
| NotInChannel | 任意 | 任意 | Invalid；不得创建结果 |

补充不变量：

```text
Applicable 或 Unknown 与任一 NotRequired 组合 = Invalid。
NotApplicable 与任一非 NotRequired 组合 = Invalid。
Incomplete 不等于 Invalid；但已提供的 row 若自相矛盾仍为 Invalid。
Unknown 不得变成 KnownFalse、KnownZero 或空布局默认值。
NotApplicable 只能来自显式 applicability，不能由空 mask、空 Build 或未配置推导。
```

## 7. 完整输入上的唯一纯谓词

只有 `Applicable + Complete + Complete` 可以运行以下算法。压力作者必须显式给出非空 `RequiredPredicateClauses`；不得把五个 clause 隐式全部启用：

1. `counted placements` 恰为 `IsCountedInBuild == true` 的 placement rows；其他 rows 不参与结果，也不贡献 0。
2. 只计算 `RequiredPredicateClauses` 中显式声明的 clause；Result 只携带这些 clause rows。
3. `CountedLayoutPresent`：至少一个 counted placement 时 Satisfied，否则 Violated。
4. `CountedPlacementCellsUsable`：每个 counted placement 的每个 `OccupiedCell` 都存在于 `UsableCellsAfterPressure` 时 Satisfied，否则 Violated。
5. `CountedPlacementCoresUsable`：每个 counted placement 的 `CoreCellWorld` 都存在于 `UsableCellsAfterPressure` 时 Satisfied，否则 Violated。
6. `EffectiveEyeAnchorUsable`：`EffectiveEyeAnchorCellAfterPressure` 存在于 `UsableCellsAfterPressure` 时 Satisfied，否则 Violated。
7. `EyeToCountedCoreStructurallyConnected`：以 `PreservedStructuralConnectionsAfterPressure` 为无向图、只允许图中已声明 usable 的 cell；阵眼锚点到每个 counted core 都可达时 Satisfied，否则 Violated。锚点与核心同格视为零边可达。本 clause 只证明作者化的中立结构连续性，不证明 Powered、点亮、FormationEnergy 或正式能量供应。
8. 全部 required clause 都 Satisfied 才是 `KnownTrue`；任一 required clause Violated 即为 `KnownFalse`。

完整空 counted layout 是合法输入，不能自动解释为 BP zero 或通用失败。只有显式要求 `CountedLayoutPresent` 时它才违反该 clause；其余 required clause 按上述定义独立求值。空布局的最终结果因此严格取决于作者化的 `RequiredPredicateClauses`，不得硬编码。

`ShapeCells`、`AnchorCell`、`Rotation` 与 `OccupiedCells` 只用于来源完整性、结构一致性和签名，不用于评分。Evaluator 不重新旋转或投影形状；`OccupiedCells` 必须是上游 Item Snapshot 已给出的只读摆放事实。

占格数量只可用于验证 `ShapeCells` 与 `OccupiedCells` 的一一结构完整性、枚举实际 cells 和防止重复；不得用于任何 BP、比例、加权、阈值、面积分或强弱排序。

## 8. Validator 不变量

Validator 至少必须拒绝：

```text
null input；SchemaId/Version 不匹配；空或外侧空白 EvaluationId
任何未定义 enum 数值
第 6 节列出的非法 applicability/completeness 组合
Complete build facts 缺 BuildFacts、BoardSize <= 0、缺 baseline eye cell 语义或含 null row
空/外侧空白 PlacementId、ItemId；Ordinal 重复 PlacementId
Rotation 不是 0 / 90 / 180 / 270
ShapeCells 或 OccupiedCells 为空、重复，或二者 cardinality 不相等
CoreCellWorld 不在该 row 的 OccupiedCells 中
IsCountedInBuild == true 且 IsLit == false
Complete pressure 缺 Pressure；空/外侧空白 PressureInputId；PressureKinds 或 RequiredPredicateClauses 为空
压力类型、domain cells、usable cells、required clauses 或 preserved structural connections 重复
LayoutDomainCells 未精确枚举 BoardSize 声明的全部 `(0 <= X < BoardSize, 0 <= Y < BoardSize)` 坐标
usable cell 不属于 LayoutDomainCells
complete Build/Pressure 中的 anchor、occupied、core、baseline eye、effective eye anchor、domain/usable cell 或 connection endpoint 越出 BoardSize 声明边界
connection 自环、端点次序未规范化，或任一端点不属于 usable cells
NotRequired 仍携带 Build/Pressure payload
KnownTrue/KnownFalse 缺任一 required clause、携带未要求 clause、重复 clause 或含非 Satisfied/Violated 状态
KnownTrue 含 Violated；KnownFalse 没有 Violated
Unknown/NotApplicable 携带 clause rows
任何隐式 Trim、大小写归一化或 Unknown -> false 转换
```

字符串身份全部使用 `StringComparer.Ordinal`。所有集合防御性复制并只读暴露。

## 9. Canonical Signature 与确定性

格式固定：

```text
sha256: + 64 lowercase hex
```

签名必须覆盖全部输入、完整性、applicability、结果状态与 clause rows。固定规范化顺序：

```text
Placement rows: PlacementId Ordinal
Shape / occupied / usable cells: Y ascending, then X ascending
Pressure kinds / required clauses: numeric ascending
Domain / usable cells: Y ascending, then X ascending
Connections: 先规范化无向端点，再按端点 cell 顺序排序
Clause rows: ClauseKind numeric ascending
整数: invariant decimal
文本: exact ordinal payload，不 Trim、不改大小写
```

必须证明：

```text
同一内容重复求值 -> 相同结果与签名
仅改变输入集合排列 -> 相同结果与签名
改变任一身份、坐标、连接、bool、完整性、applicability 或 clause outcome -> 签名变化
构造后修改调用方原集合 -> Snapshot、结果与签名不变
Evaluator 不读取时间、随机数、静态可变状态、Scene、Board 或 Battle
```

## 10. Fixture 与 no-behavior-change

Fixture 只使用通用 `dev_layout_*` 身份，不得映射真实 E06/E10 requirement、Encounter 或 capability key。

固定 10 个有效 outcome case：

```text
1. 污染 mask 未触及 counted layout -> KnownTrue
2. counted occupied cell 不可用 -> KnownFalse
3. 阵眼移动后仍全 core 可达 -> KnownTrue
4. 连接切断后至少一个 core 不可达 -> KnownFalse
5. 完整空 counted layout + required CountedLayoutPresent -> KnownFalse
6. 完整空 counted layout + 仅要求可用性通用量词 clause -> 按显式 clause 得到 KnownTrue
7. applicability Unknown -> Unknown
8. Build facts Incomplete -> Unknown
9. Pressure facts Incomplete -> Unknown
10. 显式 NotApplicable + NotRequired/NotRequired -> NotApplicable
```

Invalid fixture 至少覆盖第 8 节全部类别，且不少于 24 个独立断言。

必须证明无行为变化：

```text
不读取或修改 E06/E08/E10 catalog
不调用 E08 readiness evaluator
不创建真实 requirement mapping
不读取 Map/Board/Scene/Prefab/RectTransform
不运行地图模拟、摆放搜索、点亮重算或 Battle
E06/E07/E08/E10 byte-identical
N01B runtime/report exact scope byte-identical
C02 仍为 32/32 blocked rows，剩余 64 decisive Unknown references
placement_shape 在本包后仍有 32 个实际 Unknown references；Projected reduction = 0
```

本包的 KnownTrue/KnownFalse 只存在于通用 dev fixture，不得进入正式 readiness、C02 或玩家侧结论。

## 11. 精确文件白名单

任务窗口只允许新增以下 16 个文件：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs.meta
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractReport.md
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractSpec.csv
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFieldMatrix.csv
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFixtureCases.csv
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractLeakCheckReport.md
```

预期新增文件：`16`。

允许修改已有文件：`0`。

本 Assignment 由 Guard 写入，不计入任务窗口 16 文件。任务窗口不得修改本 Assignment、Package Queue、CurrentRules、AGENTS 或 LOCKED。

Runtime 依赖方向固定：

```text
LayoutResilience runtime -> System.* + EnemySystem/RequirementChannel 的 applicability enum
LayoutResilience runtime -X-> Items namespace / UnityEngine / PressureWindow / CapabilityRead / ReadinessEvaluation
Editor verifier -> LayoutResilience runtime + 只读文件/报告扫描
```

本包使用中立 `LayoutCellCoordinate` 投影，故不新增对 `UnityEngine.Vector2Int` 或 Item runtime type 的直接依赖。把真实 Item Snapshot 投影为本合同输入的 Adapter 不属于本包。

## 12. Protected baseline

继承并冻结以下已验收基线，不允许刷新：

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
N01B exact 14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
N01B Canonical Signature = sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

Exact scope 聚合算法固定：按 `relativePath` Ordinal 排序，每行 `relativePath|lowercaseFileSha256`，UTF-8 无 BOM，行尾 LF，最后一行后保留 LF。

`Enemy existing 89` 排除 N01B 新增 RequirementChannel 文件；N01B 必须另按 exact 14 验证。本包不得把 N01C 文件加入或刷新任何既有基线。

当前工作区存在用户的 Item visual/content dirty/untracked 内容，以及已验收但未提交的 N01/N01A/N01B 文件。任务窗口必须在开工时记录 pre-existing status；只按第 11 节 package delta 判定本包，不得清理、覆盖、还原、暂存、认领或刷新这些内容。

## 13. 绝对禁止

```text
不修改任何已有文件
不修改 Item / Enemy / IF01 / IF02 / IF03 事实源
不修改 N01 / N01A / N01B
不修改 E06/E07/E08/E10、C01/C02/C02A
不迁移真实 requirement，不增加真实 RequirementId/EncounterId 映射
不修改 readiness 消费者、状态、汇总或 player hint
不把 structural predicate 塞入 BuildCapabilityReadContract 或任何 BP 字段
不按 occupied-cell count、compactness、shape size 计算 BP/score/ratio
不从现有 pressure BP、名称、标签或文案猜测坐标 mask
不创建或修改地图压力 Config、MapDebuff、Board、Scene、Prefab、UI、RectTransform
不执行地图模拟、摆位搜索、点亮重算、Battle 或 Runtime Producer
不修改 Battle / RunFlow / SaveData / Reward / Chapter / Boss / BuildSettings
不输出正式 ReadinessBand、难度、胜负、奖励或掉落结论
不实现 debuff_counter、interrupt_timing、spirit_lock
不实现 N02 / PA01 / C02B / C02R1 / C03
不修改 AGENTS / LOCKED / ROADMAP / CURRENT / Package Queue / CurrentRules
不运行 Scene/Prefab Builder
不清理或恢复进入任务前已有 dirty/untracked 文件
不 commit / tag / push
不自动启动下一包
```

## 14. Verifier 与报告验收

Offline 与 Unity 入口必须调用同一套断言。至少验证：

```text
SchemaId/Version 精确
四个 predicate state、三个 completeness、三个 pressure kind、五个 clause kind、两个 clause state 的名称和值精确
所有未定义 enum 数值被拒绝
第 6 节完整真值表与所有非法组合
第 7 节唯一算法、显式 RequiredPredicateClauses 与对应 clause rows 精确
10/10 有效 outcome fixture 通过
Invalid assertions >= 24，覆盖第 8 节每类不变量
Unknown / NotApplicable / KnownFalse 严格隔离
无占格数 BP/score/threshold/weight/clamp 字段或算法
Ordinal identity、重复拒绝、防御性复制、确定性排序
Canonical Signature 格式、重复确定性、输入顺序不敏感、字段敏感性
Runtime 对 Items/UnityEngine/Board/Map/Battle/PressureWindow/CapabilityRead/ReadinessEvaluation 依赖泄漏 = 0
真实 requirement、Encounter、C02 mapping row = 0
E06/E07/E08/E10 behavior change = 0
C02 actual blocked rows = 32/32；actual Unknown reduction = 0
现有文件修改 = 0
Protected baseline 前后一致
Package Leak Count = 0
GUID 冲突 = 0
尾随空白 = 0
git diff --check = PASS
```

报告职责：

```text
LayoutResilienceStructuralPredicateContractReport.md
  总结语义、来源资格、压力边界、状态机、算法、无行为变化与保护结论。
LayoutResilienceStructuralPredicateContractSpec.csv
  固定 schema、enum、真值表、clause、算法、签名与错误码。
LayoutResilienceStructuralPredicateContractFieldMatrix.csv
  ownerType / fieldName / fieldType / cardinality / semantic / sourceOfTruth / eligibility / futureConsumer。
LayoutResilienceStructuralPredicateContractFixtureCases.csv
  caseId / applicability / completeness / pressureKind / expectedState / violatedClauses / fixtureOnly。
LayoutResilienceStructuralPredicateContractLeakCheckReport.md
  逐项列出 Item/Enemy 修改、BP、requirement、readiness、Board/Map/Battle、真实 ID、依赖和白名单扫描，不得只写总数。
```

若 Unity Editor 锁阻 batch：不得关闭用户 Editor、不得结束 Unity 进程、不得声称 Unity PASS；如实报告 BLOCKED 并保留 Offline verifier 结果。本包不需要场景或玩家手测。

## 15. 完成回报格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-LayoutResilienceStructuralPredicateContract01

Guard receipts:
GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01
ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01
ITEM_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01
CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 16
修改已有文件: 必须为 0
SchemaId / Version
Predicate / completeness / pressure / clause enum counts
Applicability-completeness truth table
Outcome fixture: 必须为 10/10
Invalid assertions: 必须 >= 24
KnownTrue / KnownFalse / Unknown / NotApplicable disposition
Pure evaluator determinism / Canonical Signature
Item source eligibility summary
Pressure input independence summary
No-BP / no-readiness / no-migration invariants
Real requirement / Encounter mappings: 必须为 0
C02 blocked rows: 仍为 32/32
Projected blocked-row reduction: 必须为 0
Remaining decisive Unknown references: 仍为 64
placement_shape actual Unknown references: 仍为 32
Protected hashes
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

## 16. 后续包顺序与门禁

本包通过开发、QA、用户确认与 Guard/RepoOps 收口后，只解除布局韧性结构合同前置，不自动授权任何接线：

```text
N01C LayoutResilienceStructuralPredicateContract01
-> 用户验收 + Guard / RepoOps 收口
-> LayoutResilience Item Fact Projection Adapter（独立包；只读投影，不改 Item）
-> Authored Layout Pressure Source / Adapter（独立 Enemy + Map Guard 包；不得混入 runtime simulation）
-> LayoutResilience Requirement Migration（独立包）
-> Structural Readiness Channel Consumer（独立包）
```

其余路线仍保持：

```text
debuff_counter / spirit_lock 事实合同：各自独立 Guard 包
interrupt_timing Runtime Event Fact Contract：独立包
Runtime Producer：如需，另组 Battle + Enemy + Cross-System Guard
N02 / PA01 / C02B / C02R1 / C03：继续 HOLD/BLOCKED
```

任何后续包均须新 Assignment；本包不得预建 Adapter、真实压力内容、迁移、消费者或 Runtime Producer。
