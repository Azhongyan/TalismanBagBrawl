# V0.4-ItemBuildCapabilityProjectionAdapter01 Guard Assignment

状态：`GUARD_PASS_ITEMBUILDCAPABILITYPROJECTIONADAPTER01 / READY_FOR_DEV`  
风险：`YELLOW / CROSS_SYSTEM_READ_ONLY`  
维护方：总体 / 跨系统 Guard  
日期：2026-07-18

## 0. 包定位

本包只建立一个中立、devOnly、确定性的只读 Adapter：

```text
ItemInstanceProjectionContractSnapshot.v1
+ Item 系统现有稳定只读构筑事实
→ ItemBuildCapabilityProjectionAdapter
→ BuildCapabilityReadContract.v1 / BuildCapabilitySnapshot
```

本包不做 Enemy 匹配模拟，不接 BattleSandbox Runtime，不接 UnifiedBattlePage，不改 Item 或 Enemy 规则。

## 1. 入场前置

开发窗口必须完整读取：

```text
F:/Porject/TalismanBagBrawl/AGENTS.md
F:/Porject/TalismanBagBrawl/Docs/LOCKED/*
F:/Porject/TalismanBagBrawl/Docs/ROADMAP/VERSION_ROADMAP.md
F:/Porject/TalismanBagBrawl/Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
F:/Porject/TalismanBagBrawl/Docs/V0.3/V0.3_PACKAGE_QUEUE.md
F:/Porject/TalismanBagBrawl/Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
F:/Porject/TalismanBagBrawl/Docs/V0.4/ItemSystemGuard_CurrentRules.md
F:/Porject/TalismanBagBrawl/Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
F:/Porject/TalismanBagBrawl/Docs/V0.4/EnemySystemGuard_CurrentRules.md
F:/Porject/TalismanBagBrawl/Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
```

必须先做最小只读 Code Survey，确认真实类型、命名空间、构造入口和稳定键，不得根据本 assignment 猜方法名。

## 2. 权威输入与输出

### 2.1 Item 侧只读输入

主要稳定输入：

```text
TalismanBag.Items.Generation.Projection.ItemInstanceProjectionContractSnapshot
ItemInstanceProjectionContractSnapshot.CurrentSchemaId
现有 Item System 只读 Catalog / Snapshot / Build / 点亮投影中确有稳定来源的字段
```

允许读取的事实包括：

```text
itemInstanceId
baseItemId
rarity / rarityKey
Stats
Affixes
EligibleCoreEffectIds
VisibleCoreEffectIds
buildQualification
sourceCanonicalSignature
以及现有只读快照明确提供的 shape / isLit / Build tag / core effect 激活事实
```

禁止读取 ItemSandbox UI、RectTransform、Button、场景对象或临时展示文本来推导能力。

### 2.2 Enemy 侧只读合同

目标合同：

```text
BuildCapabilityReadContract.v1
BuildCapabilitySnapshotInput
BuildCapabilitySnapshot
BuildCapabilityValueSnapshot
BuildCapabilitySourceSummarySnapshot
IBuildCapabilityVocabularyResolver
DefaultBuildCapabilitySnapshotProvider
```

能力键必须来自 Enemy 现有 Vocabulary Resolver。不得在 Adapter 内复制另一套权威键表。

### 2.3 输出边界

输出必须保持：

```text
devOnly = true
isEnabled = false
entersFormalFlow = false
valueBasisPoints in [0, 10000]
Canonical Signature deterministic
```

## 3. 允许新增范围

建议只新增：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/**
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/**
Docs/V0.4/Reports/ItemBuildCapabilityProjectionAdapterReport.md
Docs/V0.4/Reports/ItemBuildCapabilityProjectionFieldMap.csv
Docs/V0.4/Reports/ItemBuildCapabilityProjectionFixtureRows.csv
Docs/V0.4/Reports/ItemBuildCapabilityProjectionUnknowns.csv
Docs/V0.4/Reports/ItemBuildCapabilityProjectionLeakCheckReport.md
```

若真实工程已有中立 CrossSystem 目录，可在不修改既有实现的前提下沿用其命名；不得把 Adapter 放进 Item 或 Enemy 领域目录，避免任何一方反向拥有另一方规则。

## 4. 禁止修改范围

本包必须满足：

```text
修改既有 Item 文件 = 0
修改既有 Enemy 文件 = 0
修改 Battle / Board / Runtime 文件 = 0
修改 Scene / Prefab / Config = 0
修改 RunFlow / SaveData / Reward / Chapter / Boss 正式流程 = 0
修改 BuildSettings / ProjectSettings = 0
```

禁止：

```text
修改 ItemSystemSnapshot.v1 或 ItemInstanceProjectionContractSnapshot.v1。
修改 BuildCapabilityReadContract.v1、Enemy Vocabulary 或 E01-E10 数据。
修改 Item 数值、品阶、词条、概率、核心效果或 Build 规则。
通过道具中文名、英文显示名、UI 文案或文件名猜测能力。
把缺失字段按 0 处理。
把 Unknown 伪装成 Known Zero。
调用 EnemyOfflineReadinessEvaluator；该调用属于下一包 C02。
接 BattleSandbox、UnifiedBattlePage 或正式 BattleContract。
生成玩家提示、完整解题答案、DropBias 或奖励结果。
运行任何 Scene Builder、Prefab Builder 或会写资源的 Verifier。
运行 ItemFullDetailBuildSandboxWorkbenchRegressionRunner 或其他已知会改写 ItemDetailPanel.prefab 的历史入口。
修改 AGENTS.md / Docs/LOCKED/* / Item Guard / Enemy Guard 所有者文档。
commit / tag / push / reset / rollback。
```

## 5. 映射规则

### 5.1 Unknown 与 Known Zero

```text
明确有稳定规则和稳定来源，且计算结果为 0 → Known Zero
缺字段、规则未确认、无法确定来源 → Unknown / Sparse omission
```

不得用默认值掩盖数据缺口。

### 5.2 稳定标识

映射只能基于：

```text
stable itemId / baseItemId
stable statId
stable affixId
stable coreEffectId
stable Build tag / qualification key
stable shape / lighting projection key
```

所有映射必须写入 `ItemBuildCapabilityProjectionFieldMap.csv`，至少包含：

```text
targetCapabilityKey
sourceContract
sourceFieldOrStableKey
calculationRuleId
availabilityPolicy
valueScale
sourceCategoryId
notes
```

### 5.3 能力覆盖

Adapter 必须枚举 Enemy Resolver 的全部已知能力键，并将每个键明确分类为：

```text
SUPPORTED
KNOWN_ZERO
UNKNOWN_NOT_MAPPED
OUT_OF_SCOPE
```

不得要求本包把所有能力都映射为 Known。

### 5.4 确定性与只读性

相同 Item 输入与相同映射版本必须产生相同：

```text
BuildCapability values
Source summaries
SourceRevisionId
Canonical Signature
Unknown diagnostics
```

Adapter 不得改写任何输入对象。

## 6. Fixture 最低要求

至少覆盖：

```text
空 Item 集合
单个基础 Item
多个 Item 实例
同 baseItemId 的不同实例
五品阶中至少三档
带 Stats 的实例
带 Affixes 的实例
带 Core Effect 的实例
具备 Build Qualification 的实例
缺少 shape / 点亮 / Build 来源的 Sparse 输入
显式 Known Zero
无法映射的 stable key
```

Fixture 只能在新 CrossSystem QA 范围内创建，不得写回 Item/Enemy Catalog、Scene、Prefab 或 Config。

## 7. 验收合同

本包通过必须同时满足：

```text
ItemInstanceProjectionContract.v1 未修改。
BuildCapabilityReadContract.v1 未修改。
Item / Enemy 保护文件 hash 前后一致。
全部已知 capability key 均有映射状态记录。
Unknown 与 Known Zero 语义分别通过。
同输入重复运行 Canonical Signature 一致。
输入对象无突变。
devOnly=true / isEnabled=false / entersFormalFlow=false。
Scene / Prefab / Config / Battle / Board / Item / Enemy existing-file diff = 0。
Leak Count = 0。
git diff --check PASS。
Unity batch compile PASS。
Unity batch verifier PASS；若同工程被用户 Editor 占用，只能如实记录 BLOCKED，不得关闭用户 Editor，也不得伪称 PASS。
```

## 8. QA 与报告

Verifier 必须提供独立菜单与 batch 入口，名称包含：

```text
V0.4/ItemEnemyCrossSystem/ItemBuildCapabilityProjectionAdapter01/[QA Only]
```

报告必须包含：

```text
源合同版本
目标合同版本
映射版本
已知能力键总数
SUPPORTED / KNOWN_ZERO / UNKNOWN_NOT_MAPPED / OUT_OF_SCOPE 数量
Fixture 数量
确定性结果
输入不可变结果
保护 hash 结果
正式系统泄漏结果
Unity 验证状态
```

测试不得主动制造成功状态，不得修改 Item、Enemy、Scene、Prefab 或正式流程后再验证。

## 9. 交付与回流

开发窗口完成后只回传：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemBuildCapabilityProjectionAdapter01
Guard receipt: GUARD_PASS_ITEMBUILDCAPABILITYPROJECTIONADAPTER01
新增文件 / 修改已有文件数量
Item / Enemy protected hash
Capability mapping counts
Unknown / Known Zero checks
Determinism signature
Offline verifier result
Unity compile / verifier result
Leak result
git diff --check result
是否触碰禁止范围
HEAD 是否变化
```

不得自动启动 `V0.4-ItemEnemyMatchupSimulation01`。不得 commit / tag / push。

