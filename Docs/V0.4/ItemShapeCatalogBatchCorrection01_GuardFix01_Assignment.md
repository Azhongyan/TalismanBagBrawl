# V0.4-ItemShapeCatalogBatchCorrection01-GuardFix01 Assignment

```text
TASK_START_V0.4_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_GUARDFIX01
GUARD_PASS_ASSIGNMENT_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_GUARDFIX01
```

状态：`GUARD_PASS / READY_FOR_TASK_WINDOW`

工程根目录：

```text
F:\Porject\TalismanBagBrawl
```

## 1. 修复定位

本包不是形状实现修复，只处理用户已经明确批准保留的外部 UI/Scene/Prefab 工作所导致的两个保护基线迁移，并在无 Unity 工程锁的环境中重新执行原形状包验证。

用户决定：

```text
PROTECTED_ASSET_DECISION: KEEP_CURRENT_UI_SCENE_PREFAB
```

用户批准保留的 tracked dirty 资产：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity
Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity
Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/BattleLikePreviewAreaBridge.prefab
```

这些资产属于用户保留的外部工作，不属于形状包修改。Guard 已在 Unity 关闭、`Unity process=0`、`Temp/UnityLockfile=absent` 时重新计算并批准以下稳定基线：

```text
Scenes: 7 files
old expected: E8E0ABC43941ECB689EF47615C4A1B937180D704DC7F503819EEBC593004693C
new approved: F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B

Prefabs: 5 files
old expected: DC689B674A701A62D4D999EA7DD26579E8B9964E0F15A9CF8B48DF1BC561187D
new approved: 3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6
```

## 2. 唯一允许的源码修改

仅允许修改：

```text
Assets/_Game/Scripts/TalismanBag/Editor/ItemShape/ItemShapeCatalogBatchCorrectionVerifier.cs
```

且只允许把 `ProtectedScopes` 中：

```text
scenes expected aggregate
prefabs expected aggregate
```

从上方 old expected 定点替换为 new approved。不得修改文件数量、文件枚举方式、聚合算法、比较逻辑、失败逻辑或其他保护域。

## 3. 允许刷新

通过原 verifier 真实运行刷新：

```text
Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionReport.md
Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionSpec.csv
Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionChangeLedger.csv
Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionCanonicalDelta.csv
Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionLeakCheckReport.md
```

允许新增一份 GuardFix 迁移记录：

```text
Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionGuardFix01Report.md
```

迁移记录必须包含用户决定、3个批准资产路径、old/new aggregate、文件数、运行前后实际 aggregate、batch log 与最终 marker。

## 4. 严格禁止

```text
不得修改、保存、重建或格式化任何 Scene / Prefab
不得打开交互式 Unity
不得运行 Scene/Prefab/UI Builder
不得修改 ItemInnerDataCatalog、30个 Profile 或批准矩阵
不得修改11处 Item105 QA baseline
不得修改 Item、Enemy、CrossSystem、Capability Runtime
不得修改 Roll/Projection Canonical、数值、Build、词条或核心效果
不得把保护检查改成 before-after-only、warning、skip 或恒真
不得吸收其他当前或未来 dirty 文件
不得运行全局 Protected Hash 自动刷新
```

如果启动 batch 前实际 Scene/Prefab aggregate 不再等于本任务批准的新值，立即停止，不得再次迁移基线，回报 Guard。

## 5. 执行步骤

1. 确认 `Unity process=0`、`Temp/UnityLockfile=absent`。
2. 计算 Scene/Prefab aggregate，必须精确等于本任务批准的新值。
3. 记录3个批准资产的单文件哈希和修改时间。
4. 仅替换 verifier 的两个 expected aggregate 常量。
5. 运行真实 Unity batch：

```text
TalismanBag.EditorTools.ItemShape.ItemShapeCatalogBatchCorrectionVerifier.VerifyStaticBatch
```

6. batch 退出后再次计算 Scene/Prefab aggregate，必须前后相同。
7. 检查实际 diff，除 verifier 两个常量、允许刷新报告和 GuardFix 报告外不得出现本包新修改。

## 6. 必须保持的验收结果

```text
Matrix: 30/30
KEEP / CHANGE / UNRESOLVED: 14 / 16 / 0
Unexpected shape changes: 0
I024: shape_line2_h / (0,0);(1,0) / core(0,0)
Rotation: 120/120 PASS
Rarity inheritance: 150/150 PASS
Dual seed inheritance: 300/300 PASS
Candidate/Profile non-shape fields: PASS
Roll Canonical: sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8
Projection Canonical: sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2
Capability BP: break=1000 / guard=800 / cooldown=900
Item105 ledger: 11/11
C02: 32/32 blocked / Actual Unknown reduction=0
Enemy Runtime changed: NO
CrossSystem Runtime changed: NO
All protected scopes: PASS
Unity compile: PASS / error CS=0
```

预期最终 marker：

```text
ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0
```

## 7. 完成回执

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemShapeCatalogBatchCorrection01-GuardFix01
Status:
UserDecisionApplied: KEEP_CURRENT_UI_SCENE_PREFAB
VerifierConstantsChanged:
SceneAggregateBeforeAfter:
PrefabAggregateBeforeAfter:
ProtectedAssetsModifiedByGuardFix: NO
UnityCompile:
FinalMarker:
CoreShapeAssertions:
RollCanonicalStable:
ProjectionCanonicalStable:
CapabilityBpStable:
Item105Ledger:
C02Result:
RuntimeChanges:
ReportPaths:
GitDiffCheck:
DirtyWorktreeRisk:
RedlineTouched: NO
CommitTagPush: NO
```

不得 commit、tag、push、reset、切分支或清理工作树。
