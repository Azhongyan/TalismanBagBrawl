# V0.4-ItemShapeCatalogBatchCorrection01 Guard Assignment

```text
TASK_START_V0.4_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01
GUARD_PASS_ASSIGNMENT_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01
```

状态：`GUARD_PASS / READY_FOR_TASK_WINDOW`
主批准：`ITEM_GUARD`
联合确认：`ENEMY_GUARD`
Capability 审计：`PASS_WITH_EXPECTED_DERIVED_DELTAS`
日期：`2026-07-21`

工程根目录：

```text
F:\Porject\TalismanBagBrawl
```

## 0. 任务定位

本包只做 I001-I030 道具原型形状真源的统一数据纠错，并刷新由该真源正常派生的 Item 数据、验证布局、报告和精确 QA 保护基线。

本包不是新形状算法包，不设计核心格玩法，不调整棋盘、UI、数值、词条、核心效果、Build 资格、Enemy requirement 或 Capability BP。

唯一批准输入：

```text
Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv
```

该矩阵已经用户确认：

```text
30/30 APPROVED
14 APPROVED_KEEP
16 APPROVED_CHANGE
0 UNRESOLVED
```

任务窗口不得根据名称、器类、PNG 尺寸、旧截图或个人判断再次推测形状；只能逐行执行批准矩阵。

## 1. 唯一真源与继承合同

形状唯一运行时真源：

```text
Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs
```

权威读取链：

```text
ItemInnerDataCatalog
→ ItemSystemSnapshot.v1
→ IF01 ItemInstancePlacementBinding
→ Item Fact Projection
→ Layout Resilience / Enemy Consumer
```

规则：

```text
shapeId / ordered local cells / coreCellLocal 属于 baseItemId 原型事实。
五品阶、不同 seed、不同实例不得生成不同形状。
CandidateProfile.shapeDescription 只是派生展示字段，不是第二真源。
Enemy、CrossSystem、Capability、详情 UI 不得建立 baseItemId → shape 的第二张表。
```

## 2. 批准的最终矩阵

以下为最终形状。坐标顺序必须与批准 CSV 一致：

| ID | 最终 shapeId | ordered cells | technical core |
|---|---|---|---|
| I001 | shape_single_1 | (0,0) | (0,0) |
| I002 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I003 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I004 | shape_line2_v | (0,0);(0,1) | (0,1) |
| I005 | shape_line2_v | (0,0);(0,1) | (0,1) |
| I006 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I007 | shape_single_1 | (0,0) | (0,0) |
| I008 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I009 | shape_single_1 | (0,0) | (0,0) |
| I010 | shape_corner3 | (0,0);(1,0);(0,1) | (0,1) |
| I011 | shape_line2_v | (0,0);(0,1) | (0,1) |
| I012 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I013 | shape_single_1 | (0,0) | (0,0) |
| I014 | shape_line3_v | (0,0);(0,1);(0,2) | (0,1) |
| I015 | shape_line3_v | (0,0);(0,1);(0,2) | (0,0) |
| I016 | shape_corner3 | (0,0);(1,0);(0,1) | (0,1) |
| I017 | shape_line2_v | (0,0);(0,1) | (0,1) |
| I018 | shape_block2x2 | (0,0);(1,0);(0,1);(1,1) | (1,1) |
| I019 | shape_single_1 | (0,0) | (0,0) |
| I020 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I021 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I022 | shape_line3_v | (0,0);(0,1);(0,2) | (0,1) |
| I023 | shape_line2_v | (0,0);(0,1) | (0,1) |
| I024 | shape_line2_h | (0,0);(1,0) | (0,0) |
| I025 | shape_single_1 | (0,0) | (0,0) |
| I026 | shape_line3_v | (0,0);(0,1);(0,2) | (0,0) |
| I027 | shape_line2_v | (0,0);(0,1) | (0,0) |
| I028 | shape_line2_v | (0,0);(0,1) | (0,1) |
| I029 | shape_single_1 | (0,0) | (0,0) |
| I030 | shape_line3_v | (0,0);(0,1);(0,2) | (0,1) |

I024 的专项裁定：截图文字与现有美术冲突时，用户明确要求以已经完成的五品阶美术素材为准，因此锁定：

```text
I024 净水盂
shapeId = shape_line2_h
ordered cells = (0,0);(1,0)
technical core = (0,0)
```

## 3. 本包实际修改的 16 项

```text
I002  shape_line2_h → shape_line2_v
I004  shape_line2_h → shape_line2_v
I006  shape_line2_h → shape_line2_v
I010  shape_line2_v → shape_corner3
I011  shape_line2_h → shape_line2_v
I014  shape_line3_h → shape_line3_v
I015  shape_corner3 → shape_line3_v
I016  shape_line2_v → shape_corner3
I017  shape_line3_v → shape_line2_v
I021  shape_line2_h → shape_line2_v
I022  shape_line2_v → shape_line3_v
I023  shape_line2_h → shape_line2_v
I024  shape_corner3 → shape_line2_h
I026  shape_line2_h → shape_line3_v
I028  shape_line2_h → shape_line2_v
I030  shape_line3_h → shape_line3_v
```

其余 14 项必须保持批准矩阵中的现状。I009、I029 已有用户确认的单格修正，必须保护，不得回滚。

## 4. 核心格延期政策

```text
coreCellLocal contract field: KEEP
current status: TECHNICAL_RESERVED
player-facing mechanic: DEFERRED
formal UI / tutorial / artwork marker: NO
balance compensation / Capability BP mapping: FORBIDDEN
future activation: REQUIRES_SEPARATE_VERSION_AND_GUARD
```

本包仅把批准 CSV 的 `technicalCoreCellLocal` 写入真源，保证坐标属于最终 cells。不得：

```text
设计核心格收益
新增核心格提示、图标、颜色或新手教学
把核心格换算为伤害、Build、点亮或 Capability BP
根据当前 Sandbox 技术预览推导正式玩家规则
删除或升级 ItemSystemSnapshot.v1 的 core 字段
```

## 5. 实现要求

1. 在 `ItemInnerDataCatalog` 中只修改批准的 16 个原型形状声明及对应技术核心坐标。
2. 不创建新 shapeId；复用现有 `single / line2_h / line2_v / line3_v / corner3 / block2x2` 定义。
3. 通过既有显式生成/验证流程刷新 16 个 Candidate Profile 的 `shapeDescription` 及当前派生报告；不得直接手改报告制造 PASS。
4. 五品阶 150 个候选和所有实例通过 `baseItemId` 继承同一形状；不得在 Rarity、Roll 或 Projection 中复制形状表。
5. Placement、rotation、occupiedCells、coreCellWorld、点亮、阵脉和 Build counted 结果必须自然使用新真源。
6. 详情页只能通过已有 `displayShapeName / shapeId` 正常路由单格或多格图片槽，不得增加 ItemId 特判。
7. 如旧 verifier 使用正式 I001-I030 测通用几何，应把纯算法案例迁到 Editor-only QA Shape Fixture；Fixture 不得进入正式 Catalog、Candidate、150 品阶、掉落池或 ItemSandbox 道具栏。
8. 如既有 Sandbox 验证布局因新形状重叠或越界，只允许最小调整测试/验证坐标；不得改棋盘规则、合法性算法或用户 UI 布局。

## 6. Item 侧允许修改范围

按实际需要最小修改：

```text
Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs

Assets/_Game/Configs/ItemBalanceWorkbench/Profiles/
  仅 ItemBalanceProfile_I002/I004/I006/I010/I011/I014/I015/I016/
     I017/I021/I022/I023/I024/I026/I028/I030.asset
  且只允许 shapeDescription 的派生变化

Assets/_Game/Scripts/TalismanBag/Editor/
  与 Catalog、Placement、Rotation、Lighting、Array、Build、Candidate、
  Roll、Projection、Detail 形状断言直接相关的 verifier / Editor-only fixture

Assets/_Game/Scripts/TalismanBag/ItemSandbox/
  仅当新形状使既有验证布局非法时，允许最小修改 QA/Sandbox 摆放坐标；
  不得修改 UI、交互、正式算法或显示结构

Docs/V0.4/Reports/
  由真实 verifier 刷新的当前派生报告
  本包新增的 Report / Spec / ChangeLedger / LeakCheck

对应新增文件和目录的 Unity .meta
```

`Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv` 是只读批准输入，不得由任务窗口改写。

## 7. CrossSystem / Enemy QA 精确白名单

Enemy/CrossSystem Runtime 修改数必须为 `0`。

仅当真实新 Item aggregate 导致旧 `Item105` 保护值失败时，允许在以下 11 个 QA verifier 中定点迁移对应的 Item aggregate expected hash/count，并记录旧值、新值和计算输入：

```text
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/
  EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs

Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/
  LayoutResilienceItemFactProjectionAdapterVerifier.cs
  LayoutResilienceStructuralPredicateContractVerifier.cs
  LayoutResilienceStructuralReadinessConsumerVerifier.cs
  DevEncounterLayoutPressureAuthoringVerifier.cs
  LayoutResilienceRequirementChannelMigrationVerifier.cs
  LayoutResilienceRequirementMigrationSurveyVerifier.cs
  RealLayoutResilienceEvaluationPipelineVerifier.cs
  BuildCapabilityNormalizationRuleSurveyVerifier.cs
  BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs

Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/
  LayoutResilienceBattleSandboxPlaytestAdapterVerifier.cs
```

该白名单只允许：

```text
更新由 Item 真源修正直接导致变化的 Item105 aggregate expected hash
必要时更新报告中的同一派生值
保持 file count 与实际输入清单可审计
```

不得：

```text
运行全局 Protected Hash 自动刷新
修改 Enemy/CrossSystem Runtime
修改算法阈值、requirement、BP、Unknown/Invalid 语义
用新 expected 值掩盖非 Item shape 导致的失败
重写独立 synthetic fixture 的 Golden
```

如果这些 verifier 出现除 Item aggregate 之外的 Canonical/业务结论变化：

```text
先追踪到具体受影响 Item、摆放和字段。
若不能证明是批准形状变更的必然派生结果，停止修改并回报 Guard。
不得直接更新 Golden 让测试通过。
```

## 8. Capability 与 Enemy 不变量

必须证明：

```text
形状/格数变化不会改变连续 Capability BP。
break_power / guard_power / cooldown_recovery 的正式值与单位保持不变。
core / affix / build effect 定义与数值保持不变。
Enemy E01-E10 数据、机制 requirement、阈值和正式 readiness 保持不变。
C02 仍为 32/32 blocked。
Actual Unknown reduction 仍为 0。
```

允许自然变化：

```text
occupiedCells
coreCellWorld
边界/重叠合法性
点亮链、阵脉覆盖和 Build counted 状态
依赖修正形状的结构布局结论
```

禁止新增：

```text
placement efficiency
board-space score
cell-count → BP
shape → damage/guard/cooldown
```

## 9. Canonical 政策

合理允许变化：

```text
具体 Item Catalog / Foundation 内容签名
包含修正 Item 的 ItemSystemSnapshot / BuildDebugSignature
Candidate 内容签名
依赖真实修正布局的 Item/CrossSystem 派生签名
Item105 aggregate QA baseline
```

原则上必须保持不变：

```text
所有 SchemaId / SchemaVersion
Canonical 计算算法、Ordinal 排序、InvariantCulture
Unknown / Invalid / KnownZero 语义
ItemId、品阶、seed 和实例身份规则
ItemInstanceRollEngine 的 150 个 Roll Canonical
ItemInstanceProjectionContract 的 150 个 Projection Canonical
Enemy/Capability Runtime 算法与阈值
独立 synthetic contract fixture 的 Golden
```

若 Roll/Projection Canonical 发生变化，必须视为异常，查明非形状字段是否被误改；不得把它作为本包合理变化直接接受。

## 10. 必须新增的本包验证器与报告

建议新增：

```text
Assets/_Game/Scripts/TalismanBag/Editor/ItemShape/
  ItemShapeCatalogBatchCorrectionVerifier.cs

Docs/V0.4/Reports/
  ItemShapeCatalogBatchCorrectionReport.md
  ItemShapeCatalogBatchCorrectionSpec.csv
  ItemShapeCatalogBatchCorrectionChangeLedger.csv
  ItemShapeCatalogBatchCorrectionCanonicalDelta.csv
  ItemShapeCatalogBatchCorrectionLeakCheckReport.md
```

新验证器不得修改生产数据、Scene、Prefab、BuildSettings 或批准矩阵。建议菜单：

```text
Tools/Talisman Bag/V0.4/Item Shape/[QA Only] Run Catalog Batch Correction
```

预期 marker：

```text
ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0
```

## 11. 强制断言

至少覆盖：

```text
1. 30/30 Item 与批准 CSV 的 shapeId、ordered cells、technical core 完全一致。
2. 14 KEEP 无形状漂移；16 CHANGE 精确等于批准 Ledger；0 个额外 Item 变化。
3. I024 精确为 shape_line2_h / (0,0);(1,0) / core(0,0)。
4. I009、I029 保持 shape_single_1 / (0,0) / core(0,0)。
5. 每件 Item 的 cells 非空、无重复；technical core 必须属于 cells。
6. 30 × 4 方向旋转：格数守恒、无重复、core 属于旋转后 occupied cells。
7. 30 × 5 = 150 品阶版本均继承 baseItemId 形状；品阶和 seed 不改变形状。
8. Candidate 30 个 shapeDescription 与真源一致；150 版本无第二套形状数据。
9. Placement / rotation / occupiedCells / coreCellWorld 使用真源并保持确定性。
10. 单格详情使用既有单格图片槽；多格使用既有多格图片槽；0 个 ItemId UI 特判。
11. I031 未变化，仍排除普通 30×5 生成。
12. 同输入重复与输入反转保持稳定。
13. 所有公开集合保持只读，验证失败不抛未处理异常。
14. approved matrix、Scene、Prefab、BuildSettings、PNG 与 UI 前后哈希一致。
15. Enemy/CrossSystem Runtime 前后哈希一致。
16. 30 个 Profile 中仅批准的 16 个 shapeDescription 可变化，其他字段哈希/序列化值保持不变。
17. Roll150 / Projection150 Canonical 完全保持不变。
18. 连续 Capability BP 与三项正式 BP 通道保持不变。
19. Item105 QA baseline 每处均有旧值 → 新值 Ledger，不得无痕覆盖。
20. 不存在第二套 baseItemId → shape 正式表。
```

## 12. 回归矩阵

必须执行并汇报真实入口、marker、报告和计数：

```text
Unity C# 编译，error CS = 0
ItemInnerDataCatalog
ItemRarityInstanceFoundation 30×5
ItemSystemValidatorAndSnapshot
ItemGridPlacementAndEyeRule / ShapePlacement
JuNian Lighting / relay
ArrayBonus
ItemPlacementUniqueBaseRule
ItemBalanceWorkbench / Candidate 30/150
ItemInstanceRollEngine
ItemGenerationSimulationValidator
ItemInstanceProjectionContract
Item Detail / ItemFullDetail Sandbox
BuildSynergyCore
IF01 ItemInstancePlacementBinding
Layout Resilience P1 / N01C / P4 / P5-A / P5-M / P6
两个 Capability Survey
Enemy N01B 与 E01-E10 protected gates
C02 32/32 blocked / Unknown reduction 0
```

P7 当前如仍处于 Guard Return，不得以修改 Scene 或伪造手测使其 PASS；只执行允许的只读/离线回归并如实记录当前状态。

如果某历史套件存在包开始前即可复现的范围外失败：

```text
记录 BEFORE/AFTER 证据、失败 marker 与文件哈希；
不得修改范围外文件；
不得把失败改写为 PASS；
本包专项门禁和所有受影响回归仍必须 PASS。
```

## 13. Change Ledger 要求

`ItemShapeCatalogBatchCorrectionChangeLedger.csv` 至少包含：

```text
itemId
decisionStatus
oldShapeId
newShapeId
oldOrderedCells
newOrderedCells
oldCoreCellLocal
newTechnicalCoreCellLocal
catalogChanged
candidateDescriptionChanged
oldCatalogSignature
newCatalogSignature
oldSnapshotSignature
newSnapshotSignature
evidence
result
```

另需对每个迁移的 Item105 QA baseline 记录：

```text
verifierPath
fieldOrCheckId
oldHash
newHash
fileCount
inputScope
reason = APPROVED_ITEM_SHAPE_TRUTH_CORRECTION
```

历史 I009/I029 报告不得覆盖；当前报告可标注其已被本批次矩阵吸收，但必须保留来源。

## 14. 禁止修改

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ 下全部 Runtime
Enemy 正式 Runtime、数据、requirement、机制和阈值
Capability Runtime 算法、单位合同、归一化规则和 BP 值
ItemSystemSnapshot.v1 合同结构与 SchemaVersion
Roll / Projection Runtime、Schema 和 Canonical 算法
属性、固定词条、随机词条、核心效果、Build 效果与资格
品阶、概率、掉落、奖励、背包、存档、战斗和 Boss
Item Detail UI 布局与代码结构
Scene / Prefab / RectTransform / BuildSettings
PNG、Sprite importer、切图、美术资源
棋盘尺寸、格子规则和正式交互
AGENTS.md
Docs/LOCKED/*
Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv
其他 unrelated dirty/untracked 文件
```

禁止运行任何会重建或覆盖用户 UI、Scene、Prefab 的 Builder。

若发现 CrossSystem Runtime 存在第二套硬编码 shape 真源，立即停止，不得在本包修复；回报 Guard 申请独立 Cleanup 包。

## 15. Dirty Worktree 规则

工程当前存在大量既有 dirty/untracked 文件。任务开始时必须：

```text
记录 git status --short
记录本包批准文件的开始哈希
记录 Scene / Prefab / BuildSettings / UI / art / CrossSystem Runtime 的保护哈希
不得 reset、checkout、clean 或覆盖 unrelated 修改
不得整目录 git add
```

本包结束必须给出实际修改清单，并区分：

```text
本包修改
验证流程刷新报告
进入任务前已有修改
范围外新出现文件
```

## 16. 用户手测清单

开发窗口完成自动验证后，提供最短真实 Unity 手测路径，至少覆盖：

```text
1. 打开 Item Sandbox / 当前可用摆放验证入口。
2. 检查 5 个单格：I001/I007/I009/I013/I029。
3. 检查竖二格代表：I002/I004/I011/I017/I021/I023/I028。
4. 检查竖三格代表：I014/I015/I022/I026/I030。
5. 检查两个拐角三格：I010/I016。
6. 检查 I018 方块四格。
7. 专项检查 I024 为横向两格。
8. 对代表道具旋转 0/90/180/270，确认占格与边界合法性。
9. white/green/blue/purple/orange 中同一 baseItemId 形状一致。
10. 详情页单双格图片槽路由正确，用户既有 UI 布局未变化。
```

## 17. 完成回执

开发与自测完成后输出：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemShapeCatalogBatchCorrection01
Status:
ApprovedMatrix: 30/30
KeepChangeUnresolved: 14/16/0
ModifiedCatalogIds:
UnexpectedShapeChanges:
I024Result:
TechnicalCorePolicy:
CandidateProfiles:
FiveRarityInheritance:
RotationResult:
PlacementLightingArrayBuild:
RollCanonicalStable:
ProjectionCanonicalStable:
CapabilityBpStable:
EnemyRuntimeChanged: NO
CrossSystemRuntimeChanged: NO
Item105BaselineLedger:
C02Result:
UnityCompile:
VerifierMarker:
RegressionResults:
PreExistingBlockers:
ProtectedHashes:
GitDiffCheck:
DirtyWorktreeRisk:
UserHandTestPath:
RedlineTouched: NO
CommitTagPush: NO
```

不得 commit、tag、push、reset、切分支或清理工作树。
