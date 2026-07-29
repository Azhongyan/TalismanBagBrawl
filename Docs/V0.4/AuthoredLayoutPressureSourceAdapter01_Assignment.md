# V0.4-AuthoredLayoutPressureSourceAdapter01 Assignment

Joint Guard receipts:

```text
GUARD_PASS_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01
ENEMY_GUARD_CONFIRM_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01
CAPABILITY_ALGORITHM_GUARD_PASS_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01
```

生成日期：2026-07-20

所属：Overall / Cross-System Guard + Enemy Guard + Capability Algorithm Guard

Queue Id：N01C-P2

## 1. 包定位

本包只建立纯数据 Adapter：把开发者显式作者化的中立压力输入验证并冻结为既有 `LayoutResilienceStructuralPredicate.v1` 的不可变 `LayoutPressureSnapshot`。

本包拥有：

```text
作者化压力输入 DTO 与 Complete / Incomplete 声明
完整物化 cell domain、usable mask、有效阵眼锚点和无向结构连接图
Complete / Unknown / Invalid 来源状态
压力侧 Validator、Canonical Signature、synthetic fixture 与开发诊断
```

本包不拥有：

```text
N01B applicability 作者化、选择或输出
Item/Build facts、布局韧性求值结果或 readiness
真实 Encounter/MapRule/requirement/pressure content
Board/Map/Battle runtime、Scene、Prefab、UI 或 Runtime Producer
```

风险等级：`YELLOW / AUTHORED PURE-DATA PRESSURE SOURCE ONLY`。

## 2. 前置与启动必读

已验收前置：

```text
N01B EnemyRequirementChannelApplicabilitySchemaContract01
N01C LayoutResilienceStructuralPredicateContract01 / 348/348 PASS
N01C-P1 LayoutResilienceItemFactProjectionAdapter01 / 271/271 PASS
```

N01C-P1 是顺序与 Protected baseline 前置，不是 P2 runtime 依赖。P2 不得引用或读取 P1 的 Item/Build 输出。

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
Docs/V0.4/EnemyRequirementChannelApplicabilitySchemaContract01_Assignment.md
Docs/V0.4/LayoutResilienceStructuralPredicateContract01_Assignment.md
Docs/V0.4/LayoutResilienceItemFactProjectionAdapter01_Assignment.md
Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractReport.md
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractReport.md
Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractSpec.csv
Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterReport.md
Docs/V0.4/AuthoredLayoutPressureSourceAdapter01_Assignment.md
```

只读源码：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/**
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/**
```

## 3. Enemy Guard：内容所有权与三类压力

作者化来源必须由开发者逐字段显式提供。禁止从以下内容生成或补足压力：

```text
mapRuleId / EncounterId / requirementId
名称、标签、displayName、中文文案、LegacyFacts
pressure channel、pressure BP、placementModifier
旧 E03 MapRule、E06 pressure、Enemy skill 或 vocabulary
Item/Build/Board/Map/Battle runtime 状态
```

只允许复用 N01C 已锁定的三个 `LayoutPressureKind`：

```csharp
PollutedCellMask = 1
EyeRelocationOrDisruption = 2
StructuralConnectionCut = 3
```

语义固定：

```text
PollutedCellMask
  作者直接给出完整 LayoutDomainCells 与压力后的 UsableCellsAfterPressure。
  Adapter 不从“污染格”名称生成、删除或猜测任何 cell。

EyeRelocationOrDisruption
  作者直接给出 EffectiveEyeAnchorCellAfterPressure。
  阵眼只是结构/规则锚点，不代表供能、Powered、点亮或 FormationEnergy。

StructuralConnectionCut
  作者直接给出压力后仍保留的完整无向 PreservedStructuralConnectionsAfterPressure。
  Adapter 不生成默认邻接、不比较旧图、不执行寻路，也不猜测被切断的边。
```

`PressureKinds` 只是作者化来源说明，不从 kind 名称推导输出字段。一个输入可显式组合 1..3 种 kind。

所有 fixture 必须使用 `dev_pressure_*`、中立小尺寸坐标与 synthetic graph；不得出现真实 MapRule、Encounter、requirement 或正式内容 ID。

## 4. 输入 Schema 与必填字段

固定：

```text
schemaId = AuthoredLayoutPressureSourceAdapter.v1
schemaVersion = 1
```

`AuthoredLayoutPressureAuthoringCompleteness` 恰好两个命名有效值：

```csharp
Complete = 1
Incomplete = 2
```

作者输入必须建立等价字段：

```text
SchemaId / SchemaVersion
AuthoringCompleteness
PressureInputId
DeclaredBoardSize                 // source-only validation metadata
PressureKinds
LayoutDomainCells
UsableCellsAfterPressure
EffectiveEyeAnchorCellAfterPressure
PreservedStructuralConnectionsAfterPressure
RequiredPredicateClauses
```

`DeclaredBoardSize` 不进入 `LayoutPressureSnapshot`，只用于 P2 自身的整数边界与完整 domain 校验。它不得从 Item、Build 或 Board runtime 读取。

输入 DTO 必须保留“缺字段”和“显式空集合”的差异，不得在构造时把 null 自动变成空集合：

```text
Complete 时所有集合都必须显式非 null。
UsableCellsAfterPressure 与 PreservedStructuralConnectionsAfterPressure 可以显式为空。
null 表示缺失；空集合表示作者明确声明压力后的结果为空。
```

## 5. RequiredPredicateClauses

只允许复用 N01C 五个已命名 clause：

```text
CountedLayoutPresent
CountedPlacementCellsUsable
CountedPlacementCoresUsable
EffectiveEyeAnchorUsable
EyeToCountedCoreStructurallyConnected
```

规则：

```text
Complete 输入必须显式提供非空、唯一、已定义的 RequiredPredicateClauses。
不得默认开启全部五项。
不得按 PressureKind 自动选择 clause。
不得按空集合、地图名称或 Enemy 机制猜测 clause。
fixture 必须覆盖至少三种不同 clause 子集，并证明未配置 clause 不会被加入输出。
```

## 6. Adapter 输出状态

`AuthoredLayoutPressureSourceStatus` 恰好三个命名有效值：

```csharp
Complete = 1
Unknown = 2
Invalid = 3
```

必须建立等价类型：

```text
AuthoredLayoutPressureSourceSchema
AuthoredLayoutPressureAuthoringCompleteness
AuthoredLayoutPressureSourceStatus
AuthoredLayoutPressureSourceInput
AuthoredLayoutPressureSourceIssue
AuthoredLayoutPressureSourceResult
IAuthoredLayoutPressureSourceAdapter
DefaultAuthoredLayoutPressureSourceAdapter
AuthoredLayoutPressureSourceValidationCodes
```

Result 只允许：

```text
SchemaId / SchemaVersion
Status
PressureFactsCompleteness          // N01C LayoutResilienceInputCompleteness
PressureSnapshot                  // N01C LayoutPressureSnapshot
Issues
CanonicalSignature
```

固定组合：

| Source | Status | PressureFactsCompleteness | PressureSnapshot |
|---|---|---|---|
| 非空、AuthoringCompleteness=Complete、全部验证通过 | Complete | Complete | 必须非 null |
| source 缺失 | Unknown | Incomplete | null |
| AuthoringCompleteness=Incomplete 且无内在矛盾 | Unknown | Incomplete | null |
| 任意必要字段缺失/null、必要 kinds/domain/clauses 为空、domain 缺 cell 或 effective eye 缺失；无论声明 Complete/Incomplete | Unknown | Incomplete | null |
| schema mismatch、外侧空白 ID、未定义 enum、重复、`DeclaredBoardSize <= 0`、越界/extra coordinate、自环、重复无向边、不可用端点或其他已提供事实畸形 | Invalid | Incomplete | null |

P2 不允许产生：

```text
NotApplicable
NotRequired
KnownTrue / KnownFalse
ApplicabilityState
BuildFacts / Evaluation result
```

缺失或不完整作者化数据永远保持 `Unknown + Incomplete`，不得补空 Snapshot、补零或解释为 NotApplicable。

## 7. Complete 压力不变量

Complete 输入必须全部满足：

```text
PressureInputId 非空、无外侧空白，Ordinal 身份精确
DeclaredBoardSize > 0
PressureKinds 非 null、非空、唯一，且只含 3 个已命名值
LayoutDomainCells 非 null，精确枚举：0 <= X/Y < DeclaredBoardSize 的全部方格
UsableCellsAfterPressure 非 null、唯一，且为 LayoutDomainCells 子集；允许显式空
EffectiveEyeAnchorCellAfterPressure 非 null 且位于 LayoutDomainCells；不要求属于 usable mask
PreservedStructuralConnectionsAfterPressure 非 null；允许显式空
每条 connection 两端非 null、不同、位于 usable mask
无向 connection 端点按 cell Y/X 顺序规范化；规范化后不得重复
RequiredPredicateClauses 非 null、非空、唯一且只含 N01C 五个已命名值
```

未满足上述完整性条件时不得构造 Snapshot，结果为 `Unknown + Incomplete + null`；输入声明为 Complete 也不得把缺失升级为 Invalid。只有已提供事实本身畸形时才是 Invalid。

Adapter 允许交换一条无向边的 A/B 端点以形成 N01C 规范顺序；这只是确定性规范化，不是寻路、地图模拟或结构生成。重复边、自环、越界边或不可用端点是 Invalid，不得静默删除。

任意 schema mismatch、外侧空白 ID、未定义 enum 数值、重复 cell/clause/kind、`DeclaredBoardSize <= 0`、已提供坐标越界/extra coordinate、自环、重复无向边或不可用端点均为 Invalid，即使输入声明为 Incomplete。Incomplete 只容纳缺失事实，不容纳已提供事实畸形；当二者同时存在时 Invalid 优先于 Unknown。

## 8. Incomplete 与显式空集合

```text
AuthoringCompleteness=Incomplete
  可以缺 PressureInputId、DeclaredBoardSize、集合或 effective eye。
  若已提供字段本身无矛盾，结果固定 Unknown + Incomplete + null Snapshot。
  partial data 只进入 Canonical 与诊断，不构造 LayoutPressureSnapshot。

AuthoringCompleteness=Complete
  缺任一必需字段、集合为 null、domain 不完整、缺 domain cell、缺 effective eye 或 clauses/kinds/domain 为空均为 Unknown + Incomplete + null；Complete 只是作者声明，不会把缺失升级为 Invalid。

显式空 UsableCellsAfterPressure
  在 Complete 输入中合法，表示作者明确物化“无 usable cell”；不是缺失或 zero 能力。

显式空 PreservedStructuralConnectionsAfterPressure
  在 Complete 输入中合法，表示作者明确物化“无保留结构连接”；不是缺失。
```

`PressureKinds`、`LayoutDomainCells` 与 `RequiredPredicateClauses` 在 Complete 时不得为空。

## 9. N01B 与 N01C Validator-only 边界

P2 的公开 API 不接收、不作者化、不选择且不输出 N01B applicability。N01B 仅是顺序前置，并为内部 N01C validation envelope 提供固定枚举语义。

Complete candidate 构造后，只允许建立以下内部、不可输出的 pressure-only validation envelope：

```text
ApplicabilityState = EnemyRequirementApplicabilityState.Unknown
BuildFactsCompleteness = LayoutResilienceInputCompleteness.Incomplete
BuildFacts = null
PressureFactsCompleteness = LayoutResilienceInputCompleteness.Complete
Pressure = candidate LayoutPressureSnapshot
EvaluationId = 固定通用 dev validation id
```

然后只调用 `DefaultLayoutResilienceStructuralPredicateValidator.Validate(...)`。

由于 envelope 没有 BuildFacts，N01C Validator 无法取得 BoardSize；因此 P2 必须先用 `DeclaredBoardSize` 自行完成 domain 精确性和全部越界校验。禁止创建 synthetic BuildFacts 来补 BoardSize。

绝对禁止：

```text
调用 ILayoutResilienceStructuralPredicateEvaluator 或 Default...Evaluator
调用任何 Evaluate 方法
输出、缓存或报告 validation-only EvaluationInput
生成 predicate result、clause result、KnownTrue/KnownFalse 或 readiness
把固定 Applicability=Unknown 解释为真实 Enemy applicability
```

未来 assembler 才能在独立 Assignment 下组合 P1 BuildFacts、P2 PressureSnapshot 与真实 N01B applicability。

## 10. 唯一生产流程

1. 检查 source presence、Schema 与 AuthoringCompleteness。
2. 对所有已提供字段检查 schema mismatch、外侧空白 ID、未定义值、重复、自环、不可用端点和明确越界/extra coordinate；Invalid 优先于 Unknown。
3. 任意必要事实缺失或不完整且无已提供事实畸形时返回 `Unknown + Incomplete + null`；不论 AuthoringCompleteness 声明为何，均不构造压力 Snapshot。
4. 只有满足第 7 节全部事实时才形成 Complete candidate；不得生成任何缺失 cell、edge、kind 或 clause。
5. 规范化无向 edge 端点；按固定规则排序并防御性复制。
6. 用字段原值构造 N01C `LayoutPressureSnapshot`。
7. 只对完整 candidate 按第 9 节调用 N01C Validator；此时任一 issue 代表已提供事实畸形，使结果 Invalid 且丢弃 Snapshot。
8. 验证通过后返回 `Complete + Complete + Snapshot` 和 Canonical Signature。

不得从 `DeclaredBoardSize` 自动生成 LayoutDomainCells，不得从 domain 自动生成 usable mask 或邻接边。

## 11. Canonical Signature 与不可变性

格式固定：

```text
sha256: + 64 lowercase hex
```

Canonical payload 必须覆盖：

```text
Adapter SchemaId / SchemaVersion
source presence
AuthoringCompleteness
每个可空字段的 presence marker
PressureInputId / DeclaredBoardSize
PressureKinds / LayoutDomainCells / UsableCellsAfterPressure
EffectiveEyeAnchorCellAfterPressure
PreservedStructuralConnectionsAfterPressure
RequiredPredicateClauses
Status / PressureFactsCompleteness / PressureSnapshot / Issues
```

固定规范：

```text
文本：Ordinal exact，不 Trim、不改大小写
cell：Y ascending，再 X ascending
edge：先规范化端点，再按 A.Y/A.X/B.Y/B.X 排序
enum：numeric ascending
整数：InvariantCulture decimal
bool/presence：固定 lowercase true/false
null 与显式空集合必须产生不同 payload
```

必须证明：

```text
同内容重复生产、Culture 改变、集合输入反序 -> 相同结果与签名
只交换无向 edge A/B -> 相同结果与签名
改变任一作者字段、presence、status、issue 或 output -> 签名变化
调用后修改输入集合 -> Result、Snapshot 与签名不变
不读取时间、随机数、静态可变状态、文件系统或 Unity runtime
```

## 12. Synthetic fixture

固定 16 个场景：

```text
1. PollutedCellMask / 3x3 显式 domain+usable / clause 子集 -> Complete
2. EyeRelocationOrDisruption / 显式 effective eye -> Complete
3. StructuralConnectionCut / 显式保留图 -> Complete
4. 三类组合、显式多 clause 子集 -> Complete
5. 只启用一个 required clause，未配置 clauses 不出现 -> Complete
6. 显式空 usable mask + 显式空 graph -> Complete
7. edge 端点反序被规范化，语义与签名稳定 -> Complete
8. 集合反序 / Culture 改变 / 重复生产 -> 相同 Complete 签名
9. source null -> Unknown / Incomplete / null
10. Incomplete 且缺 PressureInputId -> Unknown / Incomplete / null
11. Incomplete 且缺 domain/eye/graph/clauses -> Unknown / Incomplete / null
12. Complete 但必要字段或集合为 null -> Unknown / Incomplete / null
13. Complete 但 kinds/clauses/domain 为空、domain 缺 cell 或 effective eye 缺失 -> Unknown / Incomplete / null
14. undefined enum、duplicate cell/kind/clause -> Invalid
15. BoardSize 非法或任一 cell/eye 越界 -> Invalid
16. self-loop、重复无向 edge 或 endpoint 非 usable -> Invalid
```

Verifier 还必须证明：

```text
N01C Validator-only = PASS
N01C Evaluator symbol/call count = 0
P1 Item/Build namespace/type/path references = 0
真实 N01B applicability rows、MapRule/Encounter/requirement/readiness rows = 0
Scene/Prefab/Board/Map/Battle/UI/BuildSettings connections = 0
map simulation/pathfinding/reordering/optimal-layout code = 0
BP/score/count conversion = 0
C02 仍为 32/32 blocked rows；Actual Unknown reduction = 0
```

## 13. 精确文件白名单

任务窗口只允许新增以下 14 个文件：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourcePrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourcePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceAdapter.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceAdapter.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs.meta
Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterReport.md
Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterSpec.csv
Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterFieldMatrix.csv
Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterFixtureCases.csv
Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterLeakCheckReport.md
```

预期新增：`14`。允许修改已有文件：`0`。

本 Assignment 由 Guard 写入，不计入任务窗口 14 文件。任务窗口不得修改 Assignment、Queue、CurrentRules、AGENTS 或 LOCKED。

Runtime 依赖方向：

```text
AuthoredPressure -> System.*
AuthoredPressure -> N01C pressure DTO / Validator
AuthoredPressure -> N01B Unknown enum（validation-only envelope only）
AuthoredPressure -X-> P1 ItemFactProjection / Item / Build / IF01
AuthoredPressure -X-> N01C Evaluator / result
AuthoredPressure -X-> Enemy MapRule/Encounter/requirement/readiness
AuthoredPressure -X-> Board/Map/Battle/Scene/UI/UnityEngine
```

## 14. Protected baseline

冻结且不允许刷新：

```text
N01C-P1 exact 14 = 7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48
N01C-P1 Canonical Signature = sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd
N01C exact 16 = 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b
N01C Canonical Signature = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
N01B exact 14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
N01B Canonical Signature = sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326
Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
IF01 exact 11 = 2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8
Enemy existing 89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
C01 = fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a
C02 = 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4
C02A = a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb
N01 exact 8 = 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52
C02A-D1 exact 7 = c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0
N01A exact 8 = 705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

Exact scope 聚合：按 relativePath Ordinal 排序，每行 `relativePath|lowercaseFileSha256`，UTF-8 无 BOM、LF，保留最终 LF。

工作区存在用户 dirty/untracked 与已验收未提交包。任务窗口必须记录 pre-existing status，只按第 13 节 package delta 判断；不得清理、覆盖、还原、暂存、认领或刷新既有基线。

## 15. 绝对禁止

```text
不修改任何已有文件
不修改 ItemSystemSnapshot、IF01、P1、N01B、N01C 或 Enemy 既有合同
公开 API 不接受、生成或输出 applicability / NotApplicable / NotRequired；仅允许第 9 节固定 Unknown validation envelope
不读取 Item、Build、Battle runtime 或私有状态
不读取或修改真实 MapRule、Encounter、requirement、readiness
不修改 Scene、Prefab、Board、Map runtime、Battle、UI、RectTransform、BuildSettings
不创建 ScriptableObject/Config/asset 或真实 pressure catalog
不调用 N01C Evaluator，不生成 Build/predicate/readiness result
不执行地图模拟、寻路、摆位、重排或最优布局生成
不从名称、标签、文案、pressure BP、旧 MapRule 或占格数量推导事实/BP
不运行会保存 Scene/Prefab 的 Builder
不实现 LayoutResilience Requirement Migration
不启动 N02/PA01/C02B/C02R1/C03
不修改 AGENTS/LOCKED/ROADMAP/CURRENT/Package Queue/CurrentRules
不 commit/tag/push
不自动启动下一包
```

## 16. Verifier 与报告验收

Offline 与 Unity 入口必须调用同一套断言：

```text
TalismanBag.EditorTools.CrossSystem.ItemEnemy.AuthoredLayoutPressureSourceAdapterVerifier.VerifyOffline
TalismanBag.EditorTools.CrossSystem.ItemEnemy.AuthoredLayoutPressureSourceAdapterVerifier.VerifyStaticBatch
```

可选 Editor Menu 入口只能委托同一 assertion core，不得建立第二套断言或改变结果语义。

```text
Schema/Version、2 个 AuthoringCompleteness、3 个 Status 精确
三个 PressureKind 与七个 Snapshot 字段逐项精确
Complete/Unknown/Invalid + Complete/Incomplete + payload 组合精确
缺失/不完整 -> Unknown；已提供事实畸形 -> Invalid；Invalid 优先级精确
null 与显式空集合隔离
16/16 synthetic fixtures 通过
三类 pressure 与至少三种 clause 子集覆盖
domain/bounds/usable/eye/connection/clause 全部不变量
edge 规范化、Ordinal/InvariantCulture、防御性复制
Canonical Signature 格式、顺序不敏感、字段敏感、幂等
N01C pressure-only Validator = PASS
N01C Evaluator symbol/call count = 0
P1 Item/Build references = 0
applicability public input/output fields = 0
真实 MapRule/Encounter/requirement/readiness/config rows = 0
Board/Map/Battle/Scene/Prefab/UI/BuildSettings connections = 0
BP/score/count conversion = 0
现有文件修改 = 0
Protected hashes identical
Leak Count / GUID conflicts / trailing whitespace = 0
git diff --check = PASS
```

报告：

```text
AuthoredLayoutPressureSourceAdapterReport.md
  总结所有权、字段、完整性、压力语义、Validator-only、确定性与无行为变化。
AuthoredLayoutPressureSourceAdapterSpec.csv
  固定 Schema、状态、字段、完整性、错误码、Canonical 与禁止项。
AuthoredLayoutPressureSourceAdapterFieldMatrix.csv
  sourceField / outputField / type / presence / completeness / semantic / owner。
AuthoredLayoutPressureSourceAdapterFixtureCases.csv
  caseId / completeness / kinds / domain / usable / edgeCount / clauses / expectedStatus / codes。
AuthoredLayoutPressureSourceAdapterLeakCheckReport.md
  逐项列出 inference、applicability、P1/Item/Build、Evaluator、真实内容、runtime、BP 与白名单扫描。
```

若 Unity Editor 锁阻 batch，不得关闭用户 Editor 或结束 Unity 进程，不得声称 Unity PASS；如实报告 BLOCKED 并保留 Offline 结果。本包无需场景或玩家手测。

## 17. 完成回报

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-AuthoredLayoutPressureSourceAdapter01

Guard receipts:
GUARD_PASS_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01
ENEMY_GUARD_CONFIRM_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01
CAPABILITY_ALGORITHM_GUARD_PASS_AUTHOREDLAYOUTPRESSURESOURCEADAPTER01

Result: DEV_COMPLETE / QA_RESULT
新增文件: 必须为 14
修改已有文件: 必须为 0
Schema / completeness / status counts
Pressure fields / kind coverage
Complete / Unknown / Invalid disposition
null vs explicit empty disposition
RequiredPredicateClauses subsets
Fixture: 必须为 16/16
Canonical Signature / determinism / immutability
N01C pressure-only Validator
N01C Evaluator calls: 必须为 0
Applicability public input/output: 必须为 0
P1 Item/Build reads: 必须为 0
Real MapRule/Encounter/requirement/readiness rows: 必须为 0
C02 blocked rows: 仍为 32/32
Actual Unknown reduction: 必须为 0
Protected hashes
Offline verifier
Unity compile / verifier
Leak Count / GUID conflicts / trailing whitespace: 必须均为 0
git diff --check
Forbidden scope touched: 必须为 0
Pre-existing dirty preserved
HEAD unchanged
Commit / tag / push: none
Next package: NOT_STARTED
```

## 18. 后续门禁

P2 验收只解除作者化 Pressure Snapshot 来源前置，不组合 Build、applicability 或结果：

```text
N01C-P2 AuthoredLayoutPressureSourceAdapter01
-> 用户验收 + Guard / RepoOps 收口
-> LayoutResilience Input Assembler / Requirement Migration（必须另包，且重新联合 Guard）
-> Structural Readiness Channel Consumer（独立包）
```

`N02 / PA01 / C02B / C02R1 / C03` 继续 HOLD/BLOCKED。不得自动启动后续包。
