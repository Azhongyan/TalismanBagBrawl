# V0.4-LayoutResilienceRequirementMigrationSurvey01 Joint Guard Assignment

## 1. Guard 结论

```text
Original Package: V0.4-LayoutResilienceRequirementChannelMigration01
Original Queue Id: N01C-P5
Original Risk: RED / REAL E10 REQUIREMENT ROUTING
Original Disposition: RETURNED_FOR_SPLIT / NOT_AUTHORIZED

Survey Package: V0.4-LayoutResilienceRequirementMigrationSurvey01
Survey Scope: REPORT_ONLY / READ_ONLY / NO_BEHAVIOR_CHANGE
Survey Status: JOINT_GUARD_ASSIGNED / READY_FOR_REPORT_ONLY

GUARD_RETURN_SPLIT_LAYOUTRESILIENCEREQUIREMENTMIGRATION01
ENEMY_GUARD_RETURN_SPLIT_LAYOUTRESILIENCEREQUIREMENTMIGRATION01
CAPABILITY_ALGORITHM_GUARD_CONFIRM_LAYOUTRESILIENCEREQUIREMENTMIGRATIONSURVEY01
```

P5 不得直接开发。当前既没有可唯一引用四条 legacy requirement 行的单字段 ID，也没有经用户批准的真实布局压力格子、结构连接与 clause 数据。任何直接迁移都会迫使开发窗口猜测身份、坐标、适用性或跨通道聚合。

本 Assignment 只授权 Survey。Survey 只读审计四条候选、给出规则候选和用户决策表，不建立 migration overlay，不产生真实 N01B row，不调用 P3/P4/N01C Evaluator，不改变 E10、E08、C02 或 readiness。

## 2. 启动前必读与前置状态

开发前完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/*
Docs/CURRENT/*
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ItemAlgorithmGuard_CurrentRules.md
Docs/V0.4/EnemyRequirementChannelApplicabilitySchemaContract01_Assignment.md
Docs/V0.4/LayoutResilienceStructuralPredicateContract01_Assignment.md
Docs/V0.4/LayoutResilienceItemFactProjectionAdapter01_Assignment.md
Docs/V0.4/AuthoredLayoutPressureSourceAdapter01_Assignment.md
Docs/V0.4/LayoutResilienceEvaluationInputAssembler01_Assignment.md
Docs/V0.4/LayoutResilienceStructuralReadinessConsumer01_Assignment.md
Docs/V0.4/DevEncounterSeedData01_Assignment.md
```

前置固定：

```text
N01B    GUARD_ACCEPTED / QA_PASS
N01C    GUARD_ACCEPTED / QA_PASS
N01C-P1 GUARD_ACCEPTED / QA_PASS
N01C-P2 GUARD_ACCEPTED / QA_PASS
N01C-P3 GUARD_ACCEPTED / QA_PASS
N01C-P4 GUARD_ACCEPTED / QA_PASS
E10     GUARD_ACCEPTED / QA_PASS
```

任何前置合同、公开字段或 Protected baseline 不匹配时必须停止并返回 Guard；不得刷新基线、增加兼容 fallback 或修改来源。

## 3. 已确认的阻断事实

### 3.1 E10 没有单条 requirement ID

E06 `BuildCapabilityRequirementSnapshot` 只有：

```text
BuildCapabilityKey
MinimumCapabilityBasisPoints
```

`RequirementGroupId / Role / MatchMode` 属于外层 `BuildCapabilityRequirementGroupSnapshot`。用户列出的四个 `*.required / *.recommended` 是 RequirementGroupId，不是行唯一 RequirementId；每个组都与其他 capability 共用：

| OwnerId | RequirementGroupId | Role | MatchMode | 同组 capability |
|---|---|---|---|---|
| `dev_boss_bronze_formation_general` | `dev_boss_bronze_formation_general.required` | Required | All | energy_stability, guard_power, placement_shape |
| `dev_enemy_formation_eye_problem` | `dev_enemy_formation_eye_problem.required` | Required | All | energy_stability, placement_shape |
| `dev_enemy_polluted_tile_problem` | `dev_enemy_polluted_tile_problem.required` | Required | All | cleanse_power, placement_shape |
| `dev_enemy_spirit_thief_problem` | `dev_enemy_spirit_thief_problem.recommended` | Recommended | Any | control_power, placement_shape |

上述目标组成员清单固定为 `9` 条 source requirement row：`4` 条 placement candidate + `5` 条 sibling non-placement row。

因此仅用 RequirementGroupId 绑定 overlay 会误路由同组非 placement 行。N01B 的 `RequirementId` 要求 exact、case-sensitive、唯一；在稳定 row identity 获批前，真实 N01B row 不得创建。

### 3.2 没有真实已批准 Pressure Snapshot

N01C-P2 只包含 synthetic fixtures；已接受报告固定：

```text
Real MapRule rows = 0
Real Encounter rows = 0
Real requirement rows = 0
Real readiness rows = 0
```

现有 MapRule、pressure channel、mechanic key、ownerId、名称、标签、显示文案、LegacyFacts 与 BP 只表达主题，不能作者化格子、阵眼、无向边、clause 或 applicability。

### 3.3 同一 profile 跨地图上下文复用

Survey 必须列出四个 owner/profile 在 E10 Seed/Encounter/Map 中的全部引用。当前权威证据固定为 `7` 条 active E10 candidate-context row：

```text
1 guard_wall + copper_bell + bronze_general
2 cleanse_corner + bluestone_damp + polluted_tile
3 furnace_core + furnace_ash + polluted_tile
4 furnace_core + furnace_ash + formation_eye
5 furnace_core + furnace_ash + spirit_thief
6 thunder_fire_cross + bluestone_crack + formation_eye
7 thunder_fire_cross + bluestone_crack + spirit_thief
```

此外，E03 normalized MapRule evidence 还有 `3` 条不属于四个 active E10 seed map 的 broader-only reference：`dev_map_formation_eye_shift` 分别引用 bronze-general、formation-eye、spirit-thief。它们只用于范围证据，不是本 Survey 或下一 authoring 包的自动目标。

Context Matrix 固定审计 `10` 行：`7 active E10 + 3 broader E03-only`，并明确 scope。不得假定一个固定 PressureInputId 对所有地图上下文都正确。

必须报告并提交用户决定：PressureInputId 是按 requirement 固定，还是按 `requirement + encounter/map context` 显式选择。不得从 MapRuleId 自动推导 PressureInputId。

### 3.4 混合通道组尚无聚合合同

将 placement_shape 路由为 StructuralPredicate 后，同一 Required/All 或 Recommended/Any 组会同时包含 ContinuousBP 与 StructuralPredicate 成员。现有 P4 只输出独立结构通道诊断，不拥有跨通道 group aggregation。

Survey 只能保留 Role/MatchMode provenance，不得定义 `Met/Unmet/Ready/Failed`，不得让结构结果代替同组 BP 成员，也不得让 legacy BP 继续评价 placement_shape。

## 4. 四条候选的权威 legacy 清单

Survey 必须从同一 E10 权威 Snapshot/报告重新核对以下精确行；不得把本表反向当作 Runtime 内容源：

| # | OwnerId | RequirementGroupId | Role | MatchMode | ReferenceKind | BuildCapabilityKey | Legacy BP |
|---:|---|---|---|---|---|---|---:|
| 1 | `dev_boss_bronze_formation_general` | `dev_boss_bronze_formation_general.required` | Required | All | BuildCapability | `capability.placement_shape` | 4631 |
| 2 | `dev_enemy_formation_eye_problem` | `dev_enemy_formation_eye_problem.required` | Required | All | BuildCapability | `capability.placement_shape` | 5340 |
| 3 | `dev_enemy_polluted_tile_problem` | `dev_enemy_polluted_tile_problem.required` | Required | All | BuildCapability | `capability.placement_shape` | 5383 |
| 4 | `dev_enemy_spirit_thief_problem` | `dev_enemy_spirit_thief_problem.recommended` | Recommended | Any | BuildCapability | `capability.placement_shape` | 2884 |

`ReferenceKind=BuildCapability` 是审计列，不得据此断言它已经是 ContinuousBP 或已经完成 StructuralPredicate migration。

## 5. Survey 必答结论

四条候选逐条回答：

```text
1. 完整 legacy source tuple 与证据位置。
2. RequirementGroupId 的碰撞成员，以及 group-only 绑定为什么不唯一。
3. 0–2 个显式 CandidateMigrationRequirementId；必须是报告中写死的候选字面量，不得是 Runtime 拼接结果。
4. 当前 disposition 固定为 `MIGRATION_CANDIDATE / APPLICABILITY_UNRESOLVED / USER_AUTHORING_REQUIRED`；未来是否迁移、NotApplicable 或 NotInChannel 只作为用户决策候选，不得激活。
5. DeclaredChannel 候选、applicability 候选与证据；所有未获批项标 USER_DECISION_REQUIRED。
6. PressureInputId 候选或 USER_AUTHORING_REQUIRED；说明是否需要 encounter/map context selector。
7. RequiredPredicateClauses 候选或 USER_AUTHORING_REQUIRED；不得默认全部五项。
8. 八项 P2 字段的逐项缺口。
9. Required/Recommended、All/Any 如何只作为 provenance 保留。
10. legacy BP 如何隔离且不被执行。
11. 哪些用户决定与未来合同完成后才允许实施。
12. 对 C02、E08、E10 与真实 readiness 的实际影响。
```

每条最多两套候选。没有证据支持时必须写 `KEEP_DEFERRED / USER_AUTHORING_REQUIRED`，不得凑数。

## 6. 当前允许的推荐，不是激活

Survey 当前状态固定为：

| 候选 | 当前 route | 未来初步推荐 | 理由边界 |
|---|---|---|---|
| bronze formation general | 无 | `MIGRATION_CANDIDATE / APPLICABILITY_UNRESOLVED / USER_AUTHORING_REQUIRED` | E10 pressure channel 是 burst-survival；仅名称/词汇不足以证明具体 placement pressure |
| formation eye | 无 | `MIGRATION_CANDIDATE / APPLICABILITY_UNRESOLVED / USER_AUTHORING_REQUIRED` | 有 placement/eye 主题证据，但无坐标、连接、clauses、applicability |
| polluted tile | 无 | `MIGRATION_CANDIDATE / APPLICABILITY_UNRESOLVED / USER_AUTHORING_REQUIRED` | 有 polluted-cell/placement 主题证据，但无完整物化压力事实 |
| spirit thief | 无 | `MIGRATION_CANDIDATE / APPLICABILITY_UNRESOLVED / USER_AUTHORING_REQUIRED` | E10 pressure channel 是 resource-disruption；不得从名称或 energy 语义推断 layout structural result |

四条都不得在本 Survey 标为已迁移、Applicable、NotApplicable 或 NotInChannel。缺事实不是 NotApplicable；NotInChannel 是相对 EvaluationChannel 的显式状态，不是“尚未迁移”的别名。

是否迁移四条、只迁移 formation-eye/polluted-tile，或继续全部 defer，均为 `USER_DECISION_REQUIRED`。

## 7. 唯一身份候选与绑定规则

Future overlay 的 legacy source locator 至少必须精确保存：

```text
OwnerId
RequirementGroupId
Role
MatchMode
ReferenceKind
BuildCapabilityKey
```

以上六字段构成 legacy locator tuple。`ExpectedLegacyMinimumCapabilityBasisPoints` 与 E10 source Canonical/manifest 必须另存为 provenance/drift evidence，不属于 identity locator，也不是 Runtime threshold。匹配必须 Ordinal、case-sensitive、locator tuple exact。零匹配、多匹配、同组 sibling 被命中、任一 locator 字段漂移、expected BP 漂移或 E10 provenance 不一致均为 Invalid；不得回退到 group-only、owner-only、capability-only、模糊匹配、Trim、大小写折叠或 legacy BP 近似匹配。

Survey 必须为每行输出 `CandidateMigrationRequirementIdA`，可选 `CandidateMigrationRequirementIdB`，并证明四行相互唯一、与现有 N01B synthetic ID 不冲突。候选 ID 只能由报告显式列出并等待用户逐项批准；Future Runtime 不得通过 OwnerId、GroupId 或 capability key 自动拼接。

候选 ID 不会修改 E10，也不是 E10 RequirementId。它只是未来 overlay 自有稳定身份。批准前全部状态为 `USER_DECISION_REQUIRED`。

## 8. Role / MatchMode 保留规则

固定：

```text
Required / Recommended 原样保存为只读 provenance。
All / Any 原样保存为只读 provenance。
不得把 Required 改为 Recommended，或把 All 改为 Any。
不得把 KnownTrue 解释为整组 Met/Ready。
不得把 KnownFalse 解释为整组 Unmet/Failed。
不得把 NotApplicable 解释为整组通过或 KnownZero。
不得把 Unknown 解释为 false、zero 或 group failure。
```

Future overlay 只能输出 route facts。跨通道 Required/All、Recommended/Any 的组合算法必须由更晚、独立的 total-readiness integration Assignment 定义；本 Survey、未来 P5 overlay 与 RealLayoutResilienceEvaluationPipeline 都不得自行发明。

## 9. Legacy BP 隔离

`4631 / 5340 / 5383 / 2884` 必须逐字保留为 legacy diagnostic provenance，且固定：

```text
quarantined = true
evaluated = false
converted = false
compared = false
includedInN01CInput = false
includedInP4Result = false
includedInStructuralCanonicalAsThreshold = false
```

禁止把 legacy BP 解释为格子数量、可用格比例、布局韧性分数、通过率、clause 数、结构阈值、权重或 fallback。Future overlay 是否携带原值，或只携带冻结 source locator/hash，须由用户决定；两种候选都不能进入结构求值。

## 10. PressureInputId、clauses、八字段缺口与 context cardinality

每行分别审计：

```text
PressureInputId binding
PressureKinds
DeclaredBoardSize
LayoutDomainCells
UsableCellsAfterPressure
EffectiveEyeAnchorCellAfterPressure
PreservedStructuralConnectionsAfterPressure
RequiredPredicateClauses
```

八项均属于 P2 缺口审计；其中 PressureInputId 是绑定，其他七项是必须经用户作者化/批准的真实压力内容。当前四行的八项状态均不得写成已批准。

`LayoutResilienceRequirementMigrationSurveyPressureFactGap.csv` 固定输出 `4 candidate × 8 field = 32` 条 candidate-level gap evidence row，同时每行必须包含 `contextCardinality=UNRESOLVED`。这 32 行只是最低缺口盘点，不是下一 authoring 包的最终 snapshot/row 数量。

`LayoutResilienceRequirementMigrationSurveyContextMatrix.csv` 固定输出 `10` 条 context evidence row：`7 active E10 + 3 broader E03-only`。每行至少包含 Scope、SeedId、EncounterId、MapRuleId、OwnerId、RequirementGroupId、BuildCapabilityKey、SourceBindingKind、PressureAuthoringTargetStatus。所有 PressureAuthoringTargetStatus 固定 `NOT_ACTIVATED / USER_DECISION_REQUIRED`。

最终作者化 cardinality 必须由用户在以下两种方案中选择：

```text
A. mechanic-level：每条 migration candidate 绑定单一 pressure snapshot。
B. encounter/map-context-specific：按显式 context selector 绑定多个 pressure snapshot；推荐用于“受配置地图压力后”的含义。
```

Survey 不得选择 A/B，不得把 32 条 gap evidence 冒充最终 authoring row count。Applicability 另列 `4` 条 candidate decision row，不属于八个 P2 pressure field。

允许的 N01C clauses 只用于候选清单：

```text
CountedLayoutPresent
CountedPlacementCellsUsable
CountedPlacementCoresUsable
EffectiveEyeAnchorUsable
EyeToCountedCoreStructurallyConnected
```

不得依据 ownerId、MapRuleId、pressure tag、机制名、中文文案、LegacyFacts、placementModifier、BP 或 occupied-cell count 自动选择 clause。阵眼只作为结构锚点，不证明 Powered、lighting、FormationEnergy 或正式供能。

任何缺失内容保持 `USER_AUTHORING_REQUIRED / INCOMPLETE`，不得补零、推断空集合、改为 NotApplicable 或生成 synthetic-to-real mapping。

## 11. 用户决策表

`LayoutResilienceRequirementMigrationSurveyUserDecisionSheet.csv` 必须至少包含：

```text
D1 四条中哪些进入未来 StructuralPredicate migration。
D2 每条唯一 MigrationRequirementId 与 exact legacy tuple 绑定方案。
D3 每条 applicability，以及是否按 encounter/map context 变化。
D4 每条 PressureInputId；固定 per-requirement 还是显式 per-context selection。
D5 每条 candidate/context 的八项 P2 字段完整性计划；只决定需要完整提供哪些 authoring rows，不代替 clause 选择。
D6 每条 RequiredPredicateClauses 的显式选择；不得默认全选。
D7 Legacy BP 在 overlay 中保留原值还是只保留 source hash；两者都 quarantine。
D8 未来混合通道 Role/MatchMode 与 total-readiness 聚合政策。
D9 是否批准 Survey -> Authoring -> Overlay -> Real Evaluation Pipeline -> Total Readiness 的分包顺序。
```

所有行默认 `USER_DECISION_REQUIRED`。Survey 不得替用户选择，也不得把初步推荐写成 APPROVED。

## 12. Canonical 与确定性

报告 Schema 固定：

```text
LayoutResilienceRequirementMigrationSurvey.v1
schemaVersion = 1
CanonicalSignature = sha256: + 64 lowercase hex
```

Canonical payload 必须覆盖：

```text
SchemaId / SchemaVersion
E10 Full / PlayerSafe Signature 与 exact source provenance
四条完整 legacy tuple
每组 sibling collision rows
每行 CandidateMigrationRequirementId 候选与 approval state
route/applicability/PressureInputId/clause 候选及证据状态
八项 pressure binding/content gap
context cardinality evidence
legacy BP quarantine disposition
D1-D9 状态
C02/E08/E10/no-behavior-change 结论
```

排序与编码固定：文本 Ordinal exact；整数 InvariantCulture decimal；bool lowercase；null、空、`USER_DECISION_REQUIRED` 三者必须区分；source tuple 按 OwnerId、RequirementGroupId、BuildCapabilityKey；collision row 按 sibling key；context row 按 SeedId、EncounterId、MapRuleId；decision row 按 D 编号。

输入反序、Culture 变化与重复生成必须得到相同报告和签名；任一 tuple、BP、sibling、candidate ID、gap、decision 或 provenance 改变必须改变签名。不得读取时间、随机数、机器路径、raw exception、stack trace 或 mutable runtime state。

## 13. 固定 20 个审计 fixture 与 112 条断言

```text
1-4   四条 exact legacy tuple、role/mode、BP 与 evidence。
5-8   四个 group sibling collision；证明 group-only ID 不唯一。
9     group-only binding candidate 必须拒绝。
10    owner-only/capability-only/fuzzy/case-insensitive candidate 必须拒绝。
11    exact tuple 零匹配或多匹配必须标 Invalid evidence。
12    legacy BP 漂移必须触发 provenance mismatch，不得近似匹配。
13    四个 CandidateMigrationRequirementId 唯一；重复/空白/case collision 被拒绝。
14    Role/MatchMode 原样保留；不产生 group Met/Unmet。
15    四行 PressureInputId 均未获批，状态 USER_AUTHORING_REQUIRED。
16    四行八项 P2 field gap 完整列出，缺失不转 NotApplicable。
17    MapRule/名称/标签/文案/BP 推导坐标、edge、clause 的候选被拒绝。
18    legacy BP 到格子/比例/score/threshold 的转换候选被拒绝。
19    多 encounter/map context 清单、per-requirement/per-context 决策门禁。
20    Ordinal、InvariantCulture、输入反序、重复生成、Canonical 字段敏感、白名单与 leak。
```

Offline 与 Unity 必须报告同一 assertion core 的 `112/112 PASS` 和 `20/20 PASS`。不得减少 fixture 或把多个真实行合并成一行以隐藏 collision/gap。

## 14. 精确文件白名单

Survey 任务窗口只允许新增以下 9 个文件：

```text
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementMigrationSurveyVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementMigrationSurveyVerifier.cs.meta
Docs/V0.4/Reports/LayoutResilienceRequirementMigrationSurveyReport.md
Docs/V0.4/Reports/LayoutResilienceRequirementMigrationSurveyCandidateRows.csv
Docs/V0.4/Reports/LayoutResilienceRequirementMigrationSurveyIdentityCollisionMatrix.csv
Docs/V0.4/Reports/LayoutResilienceRequirementMigrationSurveyPressureFactGap.csv
Docs/V0.4/Reports/LayoutResilienceRequirementMigrationSurveyContextMatrix.csv
Docs/V0.4/Reports/LayoutResilienceRequirementMigrationSurveyUserDecisionSheet.csv
Docs/V0.4/Reports/LayoutResilienceRequirementMigrationSurveyLeakCheckReport.md
```

预期新增 `9`；允许修改已有文件 `0`。本 Assignment 由 Guard 新增，不计入任务窗口白名单。任务窗口不得修改 Assignment、Queue、CurrentRules、AGENTS、LOCKED、E10 或任何前置包。

Verifier 只能只读消费 E10 公共 Snapshot/公开合同和冻结证据。不得新增 Runtime 文件，不得作为真实 source/provider/catalog，不得创建 migration object、N01B row、P2 real source、P3 input 或 P4 result。

七份报告职责固定：

```text
SurveyReport.md
  总结 RETURN_SPLIT 证据、四候选当前 disposition、实际影响、用户门禁与后续顺序。
CandidateRows.csv
  恰好 4 条 placement candidate，含 legacy locator、expected BP provenance、0–2 个显式 ID 候选及未激活状态。
IdentityCollisionMatrix.csv
  恰好 9 条目标组 source requirement membership，分为 4 placement + 5 sibling，并证明 group-only collision。
PressureFactGap.csv
  恰好 32 条 candidate-level gap evidence；4 × 8 P2 fields；全部 contextCardinality=UNRESOLVED。
ContextMatrix.csv
  恰好 10 条 context evidence；7 active E10 + 3 broader E03-only；authoring target 全部未激活。
UserDecisionSheet.csv
  D1-D9；其中 applicability 恰好 4 条 candidate decision，全部 USER_DECISION_REQUIRED。
LeakCheckReport.md
  白名单、调用、真实映射、Protected scope、用户 dirty preservation 与禁止路径逐项结果。
```

## 15. Protected baseline

以下冻结且不得刷新：

```text
E10 exact 21 = 3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be
E10 Full Canonical = sha256:377fb8d27d35dd127b915944d8a00c1b17afd6a580254dc21bc8d44e51a815a3
E10 PlayerSafe Canonical = sha256:aa1c4ccde3fccaadc09da4bcb0e72bf53ee9c110ed84d366e20ebd875bf4775d
E10 Catalog.cs = 9f08a734b82af27f9b6777946f431fd7247466017b95abccef6da4116424eaa0
E06 BuildPressureSnapshots.cs = 56cc65badaf89be6882629ae847fcf03c2d0bac3d800bdb562de66e275e4d1c1
E10 PressureWindowRows.csv = 9e543c04022bf7c4a3e646511cf67bb20d10f620f5bfb0c551c848baad9ee50c
E10 CompositionRows.csv = 7feaad7c4bef64e2ea3dcb3bd51dfbd78690a80337d576cd5f018fab2960472e
E03 MechanicInventory.csv = f993828a64b112a8ac2c640d592f50ee139bb374821550520932f7e6637b04f6

N01C-P4 exact 14 = 8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530
N01C-P4 Canonical = sha256:e38538f2de1b85ce00721236790f9b2beab63f19df076a34e0508aad4993c01a
N01C-P3 exact 14 = c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c
N01C-P3 Canonical = sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a
N01C-P2 exact 14 = 15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050
N01C-P2 Canonical = sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc
N01C-P1 exact 14 = 7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48
N01C-P1 Canonical = sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd
N01C exact 16 = 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b
N01C Canonical = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
N01B exact 14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
N01B Canonical = sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326

Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
IF01 exact 11 = 2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8
Enemy89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
C01 = fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a
C02 = 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4
C02A = a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb
N01 exact 8 = 03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52
N01A exact 8 = 705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

Exact-scope 聚合口径固定：按 relativePath Ordinal 排序；每行 `relativePath|lowercaseFileSha256`；UTF-8 无 BOM；每行含 LF。

现有用户 Item visual/content dirty/untracked 必须记录并原样保留。只以本包 9 文件 package delta、禁止路径、runtime leak 与上述 frozen scopes 判断；不得清理、覆盖、暂存、认领或用这些用户改动刷新基线。

## 16. Verifier 入口与验收

入口冻结：

```text
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceRequirementMigrationSurveyVerifier.VerifyOffline
TalismanBag.EditorTools.CrossSystem.ItemEnemy.LayoutResilienceRequirementMigrationSurveyVerifier.VerifyStaticBatch
```

两个入口必须委托同一 assertion core。可选 Editor Menu 只能委托同一核心，不得运行 Scene/Prefab Builder。

验收固定：

```text
新增文件 = 9
修改已有文件 = 0
四条 legacy tuple = 4/4 exact
legacy group membership = 9 exact rows / 4 placement / 5 sibling
group collision proof = 4/4
CandidateMigrationRequirementId = 每条 1–2 个候选，0 个 activated
真实 PressureInputId = 0 approved
真实完整 Pressure Snapshot = 0 approved
Pressure gap evidence = 32 rows / 4 candidates × 8 P2 fields / contextCardinality UNRESOLVED
Applicability decisions = 4 rows / all unresolved
Context evidence = 10 rows / 7 active E10 + 3 broader E03-only
Final pressure authoring cardinality = USER_DECISION_REQUIRED
Legacy BP quarantine = 4/4
P3 calls = 0
P4 calls = 0
N01C Evaluator calls = 0
N01C predicate/clause results = 0
N01B real rows authored = 0
E10/E08 modifications or behavior changes = 0
Real requirement migration rows = 0
Real readiness aggregation = 0
C02 blocked rows = 32/32
Actual Unknown reduction = 0
Fixtures = 20/20 PASS
Assertions = 112/112 PASS
Canonical determinism = PASS
Protected hashes before/after = identical
Leak Count = 0
GUID conflicts = 0
Trailing whitespace = 0
git diff --check = PASS
Forbidden scope touched = 0
HEAD unchanged
Commit / tag / push = none
Next package = NOT_STARTED
```

若 Unity Editor 锁阻 batch，必须如实回报 `BLOCKED`；不得结束用户 Editor、杀进程或声称 Unity PASS。本包无需 Scene/Play 手测。

## 17. 绝对禁止

```text
修改 E10 原始数据、RequirementGroup、legacy BP、Canonical 或报告
修改 E06/E08 requirement/readiness、N01B、N01C、P1、P2、P3、P4
创建 Runtime migration overlay、真实 N01B row 或真实 Pressure source
调用 P3 Assembler、P4 Consumer 或 N01C Evaluator
把 group ID 冒充 row-unique RequirementId
根据 ownerId、名称、标签、文案、MapRule、pressure BP 或 LegacyFacts 生成格子、edge、clauses 或 applicability
把 legacy BP 转为格子、比例、score、threshold、predicate result 或 fallback
把 Unknown 当 0/false，把缺事实当 NotApplicable，把未迁移当 NotInChannel
定义跨通道 Required/Recommended、All/Any 聚合或总 readiness
输出 ReadinessBand、难度、胜负、奖励、掉落或正式战斗结论
修改 Item/IF01/Battle/Board/Map/Scene/Prefab/UI/RectTransform/BuildSettings
修改 RunFlow/SaveData/Reward/Chapter/Boss/Config
修改 AGENTS/LOCKED/ROADMAP/CURRENT/Package Queue/CurrentRules/其他 Assignment
运行会保存 Scene/Prefab 的 Builder
启动 N02、PA01、C02B、C02R1、C03
commit / tag / push
自动启动任何后续包
```

## 18. 后续包顺序

固定顺序，仅记录，不自动启动：

```text
1. V0.4-LayoutResilienceRequirementMigrationSurvey01
2. 用户批准 D1-D9，并提供/确认真实坐标、图、clauses 与 context policy
3. V0.4-DevEncounterLayoutPressureAuthoring01
   - 只作者化并验收真实 P2 pressure source；不得迁移 requirement
4. V0.4-LayoutResilienceRequirementChannelMigration01
   - overlay-only；只输出唯一 route/applicability/PressureInputId/clause/provenance；不得调用 P3/P4/Evaluator
5. V0.4-RealLayoutResilienceEvaluationPipeline01
   - 独立组合 route + P1 + P2 + P3 + P4；只形成结构通道诊断
6. 未来独立 Cross-Channel Total Readiness Integration
   - 另行定义混合通道 Role/MatchMode 与总 readiness；未授权
7. N02 / PA01 / C02B / C02R1 / C03 继续按各自 Guard gate 阻塞
```

## 19. 完成回传格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-LayoutResilienceRequirementMigrationSurvey01

Guard receipts:
GUARD_RETURN_SPLIT_LAYOUTRESILIENCEREQUIREMENTMIGRATION01
ENEMY_GUARD_RETURN_SPLIT_LAYOUTRESILIENCEREQUIREMENTMIGRATION01
CAPABILITY_ALGORITHM_GUARD_CONFIRM_LAYOUTRESILIENCEREQUIREMENTMIGRATIONSURVEY01

新增文件 / 修改已有文件
4/4 legacy source tuple
9-row legacy group membership / 4 placement / 5 sibling
4/4 identity collision proof
Per-row disposition and 0–2 ID candidates
Role/MatchMode preservation
Legacy BP quarantine
PressureInputId / context selection gaps
32 pressure field gap evidence rows
10 context evidence rows
Clause candidates / USER_AUTHORING_REQUIRED
D1-D9 decision status
P3 / P4 / Evaluator calls
Real migration / readiness rows
C02 blocked rows / Actual Unknown reduction
Canonical Signature
20/20 fixtures / 112/112 assertions
Protected hashes
Offline verifier
Unity verifier
Leak Count
git diff --check
Forbidden scope
Pre-existing dirty preservation
HEAD status
Commit / tag / push
Next package: NOT_STARTED
```
