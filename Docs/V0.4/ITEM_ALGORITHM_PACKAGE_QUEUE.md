# Item 算法 Package Queue

更新时间：2026-07-21
所属：总体 Item System 下属 Item 算法线

## 总体状态

```text
Status: 8/8 COMPLETE
Final package: V0.4-ItemInstanceProjectionContract01
Final marker: ITEM_INSTANCE_PROJECTION_CONTRACT01_PASS
Final spec: 156/156 PASS
User hand test: PASS
Cross-system integration: NOT_STARTED
```

## 已完成包

### 1. V0.4-ItemRarityInstanceFoundation01

```text
Status: COMPLETE / PASS
Algorithm Guard receipt: ITEM_ALGORITHM_GUARD_PASS_ITEMRARITYINSTANCEFOUNDATION01
User approval: CONFIRMED
```

只做：

```text
I001-I030普通原型身份
五品阶稳定定义
30×5=150派生组合
baseItemId / itemInstanceId / placementId边界
ItemInstanceIdentitySnapshot.v1
generationVersion / rootSeed
I031普通生成排除
cultivationPotentialProfileId接口预留
Verifier与报告
```

禁止：

```text
真实属性与数值
词条池与词条Roll
核心效果数量与解锁
Build资格概率
掉落与Reward
养成、Save、Battle或正式页面接线
修改ItemSystemSnapshot.v1
```

### 2. ItemStatRangeSchema01

```text
Status: COMPLETE / PASS
Marker: ITEM_STAT_RANGE_SCHEMA01_PASS
Spec: 39/39 PASS
```

建立属性字典、方向、精度、步长、总范围与五品阶子区间。先使用少量锚点道具 `SEED_DATA`，不填30件正式数值。

### 3. ItemCorePotentialAndBuildEligibilitySchema01

```text
Status: COMPLETE / PASS
Marker: ITEM_CORE_POTENTIAL_AND_BUILD_ELIGIBILITY_SCHEMA01_PASS
Spec: 69/69 PASS
```

建立品阶核心潜力与 BuildQualification Schema。只做结构和校验，不擅自填写核心数量或 Build 概率。

### 4. ItemAffixPoolAndRangeSchema01

```text
Status: COMPLETE / PASS
Marker: ITEM_AFFIX_POOL_AND_RANGE_SCHEMA01_PASS
Spec: 88/88 PASS
```

建立固定词条、随机词条、品阶数值区间、槽位、权重、互斥和重复规则结构。未获确认的数据保持 `Unresolved`。

### 5. ItemInstanceRollEngine01

```text
Status: COMPLETE / PASS
Marker: ITEM_INSTANCE_ROLL_ENGINE01_PASS
Spec: 72/72 PASS
```

使用稳定 seed 生成属性、词条、核心潜力与 Build 资格结果。必须可复现，不接 Reward、Inventory 或 Save。

### 6. ItemDropGenerationSandbox01

```text
Status: COMPLETE / PASS
Marker: ITEM_DROP_GENERATION_SANDBOX01_PASS
Spec: 87/87 PASS
Determinism: 6/6 PASS
```

建立 DropGenerationRequest、DropSourceContext、候选池、品阶权重、30件普通道具抽取和 I031 排除。只在独立 Sandbox 验证。

### 7. ItemGenerationSimulationValidator01

```text
Status: COMPLETE / PASS
Marker: ITEM_GENERATION_SIMULATION_VALIDATOR01_GUARDFIX01_PASS
Spec: 65/65 PASS
Distribution: 46/46 PASS
Determinism: 12/12 PASS
Invariant violations: 0
```

执行大样本模拟，验证区间、权重偏差、互斥、重复、seed复现和 I031 泄漏；输出报告，不改正式概率。

### 8. ItemInstanceProjectionContract01

```text
Status: COMPLETE / PASS
Marker: ITEM_INSTANCE_PROJECTION_CONTRACT01_PASS
Spec: 156/156 PASS
Instances: 6
Queries: 9
Leaks: 0
User hand test: PASS
```

把生成实例以只读契约提供给总体 Item System 与详情投影，不接正式 Battle、养成、Reward 或 Save。

## 后置跨系统包

```text
ItemDropRewardIntegration01
```

该包不在当前八包内。当前状态为 `NOT_STARTED / REQUIRES_EXPLICIT_CROSS_SYSTEM_ASSIGNMENT`，不得因算法线 8/8 完成而自动启动。

## 队列规则

```text
第一包只允许SCHEMA_ONLY
未确认概率与数值不得进入assignment
v0.5草案只能作为PROPOSAL_ONLY参考
每包先经Item算法Guard收口
涉及ItemSystemSnapshot或详情公共字段时同步总体Item Guard
涉及Reward/RunFlow/Save/Battle时停止并另开跨系统包
不commit / tag / push
RepoOps必须精确选择单包文件，不得整目录git add
```

## Balance Candidate Phase

### V0.4-ItemBalanceWorkbenchAnd150CandidateSeed01 + GuardFix01

```text
Status: COMPLETE / PASS
Algorithm Guard receipt: ITEM_ALGORITHM_GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
Item System Guard receipt: GUARD_PASS_ITEMBALANCEWORKBENCHAND150CANDIDATESEED01_GUARDFIX01
User hand test: PASS
GuardFix Spec: 1286/1286 PASS
Ordinary prototypes: 30/30
Rarity versions: 150/150
Stat ranges: 600/600
Roll / Projection / Determinism: 150/150
I031 ordinary-pool entries: 0
```

该包是算法基础八包完成后的独立平衡候选阶段，不改变 `8/8 COMPLETE` 的基础包计数。

数据成熟度固定为：

```text
BALANCE_CANDIDATE
EDITABLE
NOT_LIVE_LOCKED
NOT_BATTLE_CONNECTED
```

候选 Unity 资产可编译进既有 Stat / Affix / Core / Build / Drop Schema，并可在 Editor Workbench 中执行 Roll 与 Projection；但尚未接入正式 Item Runtime、正式 Item Detail、Battle、Reward/RunFlow、Inventory/Save 或正式掉落系统。

`AFFIX_DURATION_BRIDGE` 仅是 duration secondary-stat pool 所需的候选定义，不代表正式词条规则已获批准。

不得将本包自动提升为 `PLAYTEST_ACCEPTED` 或 `LIVE_LOCKED`。`ItemDropRewardIntegration01` 继续保持 `NOT_STARTED / REQUIRES_EXPLICIT_CROSS_SYSTEM_ASSIGNMENT`。

### V0.4-ItemDetailTwoStateAndInlineArrayModifierGuardFix01

```text
Status: READY_FOR_TASK_WINDOW
Guard rule: RULE_CONFIRMED
Assignment: Docs/V0.4/ItemDetailTwoStateAndInlineArrayModifierGuardFix01_Assignment.md
Expected marker: ITEM_DETAIL_TWO_STATE_INLINE_ARRAY_MODIFIER_GUARDFIX01_PASS
```

本包只处理详情页头部状态收敛与阵脉行内修正展示：

```text
头部仅保留点亮状态与阵脉状态
开窍状态回到核心效果行内
阵脉生效时，在前两大类的实际目标行后显示 Sprite 图标 + 专色增量
阵脉未生效时不显示该增量
不新增阵脉大区块
不改用户手调布局
不改三大分类
不覆盖现有候选内容与数值
```

因当前阵脉 Runtime 只有状态布尔量，本包允许增加独立的可编辑 `BALANCE_CANDIDATE` 阵脉修正载荷与只读解析结果；不得接正式 Battle、Reward/RunFlow、Inventory/Save，不得修改 `ItemSystemSnapshot.v1` 或算法基础八包契约。

### V0.4-I009AndI029ItemShapeFix01

```text
Status: PACKAGE_SCOPE_SELFTEST_PASS / EXTERNAL_REGRESSION_BLOCKERS_RECORDED / AWAITING_USER_HANDTEST
Guard rule: RULE_CONFIRMED
Assignment: Docs/V0.4/I009AndI029ItemShapeFix01_Assignment.md
Expected marker: I009_I029_ITEM_SHAPE_FIX01_PASS
```

本包锁定并校验：

```text
I009 离火焚邪印 = shape_single_1 / (0,0) / core(0,0)
I029 照煞镜 = shape_single_1 / (0,0) / core(0,0)
五品阶与所有实例继承原型形状
四个旋转方向均保持单格
详情页走 DaojuSingleCellImage
```

当前工作树中 I009 已存在未提交单格修正，本任务窗口必须保留并验证；I029 仍是 `shape_line2_v`，需要从真源修正并同步派生出口。不得修改其他道具形状，不得改用户手调详情布局，不得接 Battle、Reward/RunFlow、Inventory/Save 或 BuildSettings。

Guard 实施复核：专项 marker、Unity 编译、I009/I029 真源、四方向旋转、五品阶、双 seed Roll / Projection、Candidate 与详情共享单格图片槽均已通过。当前不发送最终同步，只等待用户手测。

全量历史套件的非绿项已隔离记录：详情手调布局 `12/14 active`、全体候选既有 `player-skill-monitor` 文本检查、Projection 下游 marker `155/156`、Workbench 的 BuildSettings 扫描器自身文字误命中、旧 BattleSandbox Vertical2/场景假设及入场前场景尾随空格。它们不构成本包形状回退，也不得在本包扩修；用户手测通过后的同步必须标注 `PACKAGE_SCOPE_PASS / GLOBAL_REGRESSION_BLOCKERS_RECORDED`。

### V0.4-ItemShapeCatalogBatchCorrection01

```text
Status: COMPLETE / USER_HANDTEST_PASS
Guard status: MULTI_GUARD_CONVERGED
Primary Guard: ITEM_GUARD
Joint Guard: ENEMY_GUARD
Capability review: PASS_WITH_EXPECTED_DERIVED_DELTAS
System ownership: ITEM_OWNED / CROSS_SYSTEM_CONSUMED
Assignment: Docs/V0.4/ItemShapeCatalogBatchCorrection01_Assignment.md
GuardFix assignment: Docs/V0.4/ItemShapeCatalogBatchCorrection01_GuardFix01_Assignment.md
Approved matrix: Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv
```

统一结论：Item 系统唯一持有 I001-I030 的 `shapeId / ordered cells / coreCellLocal / 默认朝向`；Enemy、Layout Resilience 与 Capability Survey 只消费 Item Snapshot / Item Fact Projection，不得建立第二套形状真源。Capability 连续 BP 不因格数变化而调整；结构通道按修正后的事实重新投影。

单包条件：用户批准完整30件矩阵；Item Runtime 真源一次修改；Candidate 与当前派生出口统一生成；通用形状测试改用 Editor-only QA Fixture；Enemy/CrossSystem Runtime 修改数必须为0。允许按精确白名单迁移由 Item105 或真实修正布局引起的 Editor QA baseline，并必须保留旧值、新值与 superseded 记录；禁止自动刷新全局 Protected Hash。

当前已确认：I009、I029 均为 `shape_single_1 / (0,0) / core(0,0)`。其余道具在用户矩阵批准前不得修改。I031 不属于本批次。

正式 Assignment 只在30件矩阵批准、Item Guard 主批准与 Enemy Guard 联合确认后生成。

用户已批准 `30/30` 矩阵：`14 KEEP / 16 CHANGE / 0 UNRESOLVED`。I024 以已制作五品阶美术主图为准，锁定为 `shape_line2_h / (0,0);(1,0)`。空间核心格保留为 `TECHNICAL_RESERVED` 合同字段，当前版本不作为玩家机制、UI、教学或平衡输入；正式核心格设计延期至独立后续版本。

矩阵批准文件：`Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv`。当前允许 Guard 生成唯一 Assignment，但在 Assignment 下发前不得修改 Runtime。

正式 Assignment 已于 2026-07-21 下发：`Docs/V0.4/ItemShapeCatalogBatchCorrection01_Assignment.md`。任务窗口只允许执行该任务书；完成后按 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS` 回传 Guard + RepoOps。

2026-07-21 实现回执：30件矩阵、16项纠错、I024、120次旋转、150品阶/300双seed继承、Roll/Projection Canonical、Capability BP、Item105 11处 Ledger 和 C02 均提供通过证据；Unity 源码编译通过。最终自动验收未通过，唯一专项失败为 `protected.scenes`。

Guard 复核确认 PID 10076 为人工控制的 Unity Editor（`BatchMode=0 / IsHumanControllingUs=1`），持有工程 `UnityLockfile`，且 ItemSandbox Scene 在任务期间偏离任务起始保护哈希。包状态更新为：

```text
Status: IMPLEMENTED / AWAITING_EXCLUSIVE_REVALIDATION
Blocker: EXTERNAL_INTERACTIVE_UNITY_LOCK_AND_PROTECTED_SCENE_DRIFT
Final PASS: NO
GuardFix: NOT_ISSUED
RepoOps completion sync: HOLD
```

用户必须先决定 Scene 改动保留或放弃，并正常关闭 Unity；Guard 随后才可批准稳定基线处理与独占复验。不得由任务窗口杀进程、回滚 Scene、吸收新哈希或削弱保护断言。

用户关闭 Unity 后，Guard 已执行真实独占 batch。工程锁已解除，Unity 编译与全部 Item 形状核心断言通过；最终 marker 仍为 `ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_FAIL errors=2`。失败已精确定位为：

```text
protected.scenes: E8E0ABC4...693C -> F44C2A85...746B
  Scene_TalismanBag_V04_ItemSandbox.unity
  Scene_TalismanBag_V04_UnifiedBattlePageShell.unity

protected.prefabs: DC689B67...187D -> 3D3807A7...FBA6
  BattleLikePreviewAreaBridge.prefab
```

当前状态更新为 `IMPLEMENTED / AWAITING_USER_PROTECTED_ASSET_DECISION`。这三个资产不属于形状包白名单；用户确认保留或不接受前，不下发基线迁移 GuardFix，不回滚，也不标 PASS。

用户已明确批准 `KEEP_CURRENT_UI_SCENE_PREFAB`。Guard 在 Unity 关闭、无工程锁时锁定新保护基线：Scenes 7=`F44C2A85...746B`，Prefabs 5=`3D3807A7...FBA6`。这些资产仍归属用户外部工作，不归入形状包修改。

已下发唯一最小复验任务：`Docs/V0.4/ItemShapeCatalogBatchCorrection01_GuardFix01_Assignment.md`。只允许更新专项 verifier 的两个 expected aggregate、刷新专项报告并独占重跑；不得修改 Scene/Prefab 或其他 Runtime。

GuardFix01 已通过自动验收：最终 marker 为 `ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0`；Scene/Prefab 批准聚合前后稳定，全部保护域、核心形状断言、Roll/Projection、Capability BP、Item105 11处与 C02 均通过，Runtime 修改为0。

当前状态：`AUTO_ACCEPTANCE_PASS / AWAITING_USER_HANDTEST`。Guard 已修正 GuardFix Assignment 中 Roll SHA 少一个字符的纯文档笔误；实际验证基线始终正确。用户手测前 RepoOps completion 保持 HOLD。

用户已回执 `USER_HANDTEST: PASS`。本包及 GuardFix01 正式关闭：

```text
Status: COMPLETE / USER_HANDTEST_PASS
Auto acceptance: PASS
Final marker: ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0
RepoOps record: READY
Commit / tag / push: NO
```

批准矩阵、Change Ledger、Canonical Delta、GuardFix 迁移记录和当前报告作为后续消费基线。后续包不得恢复旧形状或旧 Scene/Prefab 保护聚合；核心格仍为 `TECHNICAL_RESERVED`，未获正式玩法授权。

## Shared Module Layering Queue Gate

所有后续 Item、Item UI、Item→Battle 与 Item→Enemy 包必须读取：

```text
Docs/V0.4/SHARED_MODULE_LAYERING_AND_PREFAB_PRESENTATION_GUARD.md
```

Queue Gate：Prefab 只读 ViewModel，不挂载权威 Item 数据或状态；成熟 Item UI 必须转为
Base Prefab并跨场景复用；任何第二套 ItemData、InventoryManager、Scene Copy 长期维护
或无设计师手测节点的路线均不得入队，必须先向用户发出 `GUARD_LAYERING_WARNING`。
