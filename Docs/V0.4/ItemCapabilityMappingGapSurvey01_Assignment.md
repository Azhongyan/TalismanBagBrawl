# V0.4-ItemCapabilityMappingGapSurvey01 Guard Assignment

状态：`GUARD_PASS_ITEMCAPABILITYMAPPINGGAPSURVEY01 / READY_FOR_DEV`  
风险：`LIGHT_YELLOW / READ_ONLY_CROSS_SYSTEM_SURVEY`  
维护方：总体 / 跨系统 Guard  
日期：2026-07-18

## 0. 包定位

本包只审计 C01 尚未映射的 11 个能力与 2 个 runtime-only 能力，建立可追溯的缺口分类与优先级。

```text
C01 FieldMap / UnknownDiagnostics
+ Item 现有稳定 Contract / Schema / Snapshot / Catalog
+ E10 Encounter Requirement 使用情况
→ Mapping Gap Survey
→ 证据矩阵 / 决策清单 / C02B 候选范围
```

本包不修改任何映射，不新增 Item 数值或规则，不运行 Enemy Readiness，不接 Battle。

## 1. 必读与 Survey

开发窗口必须先完整读取真实工程启动规则与：

```text
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ItemBuildCapabilityProjectionAdapter01_Assignment.md
Docs/V0.4/ItemEnemyMatchupSimulation01_Assignment.md
Docs/V0.4/Reports/ItemBuildCapabilityProjectionFieldMap.csv
Docs/V0.4/Reports/ItemBuildCapabilityProjectionUnknowns.csv
Docs/V0.4/Reports/ItemEnemyMatchupCapabilityCoverage.csv
Docs/V0.4/Reports/ItemEnemyMatchupUnknownBlockers.csv
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
```

必须对真实 Item Contract / Schema / Snapshot / Candidate Catalog 做只读 Code Survey。不得仅凭已有 CSV 或名称下结论。

## 2. 审计对象

必须逐项覆盖 C01 的 16 个能力键，重点覆盖：

```text
UNKNOWN_NOT_MAPPED 11 项
OUT_OF_SCOPE 2 项
SUPPORTED 3 项作为对照样本
```

E10 阻断优先项至少包含：

```text
capability.cleanse_power
capability.control_power
capability.energy_stability
capability.placement_shape
capability.burst_window
capability.clear_power
capability.spirit_lock
capability.interrupt_timing
```

## 3. 分类口径

每个能力只能归入以下一种主分类：

```text
DIRECT_MAPPABLE_EXISTING_FACT
MAPPABLE_REQUIRES_RULE_CONFIRMATION
REQUIRES_RUNTIME_SIGNAL
NO_STABLE_ITEM_SOURCE
NOT_REQUIRED_BY_E10
ALREADY_SUPPORTED
```

含义：

```text
DIRECT_MAPPABLE_EXISTING_FACT
= 已有稳定 ID、单位和确定性计算规则；C02B 可直接实现。

MAPPABLE_REQUIRES_RULE_CONFIRMATION
= 已有事实来源，但缺换算比例、聚合方式、点亮/Build 条件或策划规则；必须先列出决策项。

REQUIRES_RUNTIME_SIGNAL
= 必须读取施法时机、战斗事件、目标状态或时间窗口；不得塞进纯 Item Adapter。

NO_STABLE_ITEM_SOURCE
= 当前 Item Contract 没有可靠事实来源，不得猜测。

NOT_REQUIRED_BY_E10
= E10 当前没有要求，可后置但不得改写原有 C01 状态。

ALREADY_SUPPORTED
= C01 已支持，仅作为证据链对照。
```

## 4. 证据要求

每个能力必须记录：

```text
capabilityKey
currentC01Status
E10 requirement reference count
C02 blocked row count
candidateSourceContract
candidateStableKeyOrMember
sourceUnit
requiredConditions
aggregationQuestion
classification
confidence
recommendedNextAction
evidenceFile
evidenceLineOrSymbol
```

证据只能来自稳定技术字段，例如：

```text
stable itemId / statId / affixId / coreEffectId
shape / coreCell / lighting / Build tag 的稳定 Snapshot 字段
现有明确的 basis point 或 raw unit 定义
现有 E10 requirement key
```

禁止用中文道具名、UI 文案、文件夹名、图标名、策划比喻或章节编号建立映射。

## 5. 决策清单

对 `MAPPABLE_REQUIRES_RULE_CONFIRMATION` 必须生成最小决策问题，每项包括：

```text
decisionId
capabilityKey
现有事实
缺失规则
最多 2 个可选口径
推荐口径及原因
影响的 Encounter 数量
不决策时的安全默认：保持 Unknown
```

不得在本包替用户选择或把推荐项写入代码。

## 6. 允许新增范围

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs
对应 .meta
Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyReport.md
Docs/V0.4/Reports/ItemCapabilityMappingGapMatrix.csv
Docs/V0.4/Reports/ItemCapabilityMappingSourceEvidence.csv
Docs/V0.4/Reports/ItemCapabilityMappingDecisionList.csv
Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyLeakCheckReport.md
```

如确有必要，可在相同 Editor-only 目录新增一个同前缀的纯报告辅助文件；不得新增 Runtime 文件。

## 7. 禁止范围

必须满足：

```text
修改 C01 / C02 文件 = 0
修改 Item / Enemy 既有文件 = 0
修改 Runtime / Battle / Board = 0
修改 Scene / Prefab / Config = 0
```

禁止：

```text
实现或修改任何 Item→Capability 映射。
修改 C01 FieldMap 状态。
新增数值、比例、阈值、词条、品阶、核心效果或 Build 规则。
调用 EnemyOfflineReadinessEvaluator。
把 runtime-only 能力伪装成 Item 静态能力。
把 Unknown 改成 0、失败、Ready 或 OutOfScope 来减少缺口。
运行 Scene/Prefab Builder 或历史写资源回归入口。
修改 ItemDetailPanel.prefab 或任何美术插槽。
修改 AGENTS.md / Docs/LOCKED/* / Guard owner docs。
commit / tag / push / reset / rollback。
```

## 8. 验收

本包通过必须满足：

```text
16/16 能力都有唯一分类。
11 个 Unknown 与 2 个 OutOfScope 全部有源证据或明确无源结论。
E10 requirement reference count 与 C02 报告一致。
所有 DIRECT_MAPPABLE 条目都具备 stable key、单位、条件和确定性聚合依据。
所有 REQUIRES_RULE_CONFIRMATION 条目都有决策问题且未被擅自实现。
所有 REQUIRES_RUNTIME_SIGNAL 条目没有被推荐进入 C02B 静态映射。
Item / Enemy / C01 / C02 protected hash 前后一致。
现有文件修改数 = 0。
Scene / Prefab / Runtime diff = 0。
Leak Count = 0。
git diff --check PASS。
Offline verifier PASS。
Unity batch verifier PASS；被用户 Editor 占用时如实记录 BLOCKED。
```

## 9. 回传

完成后只回传：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemCapabilityMappingGapSurvey01
Guard receipt: GUARD_PASS_ITEMCAPABILITYMAPPINGGAPSURVEY01
新增文件 / 修改已有文件
16-key classification counts
E10 blocking priority top list
DIRECT_MAPPABLE list
REQUIRES_RULE_CONFIRMATION list
REQUIRES_RUNTIME_SIGNAL list
NO_STABLE_SOURCE list
Decision count
Protected hashes
Offline verifier
Unity verifier
Leak count
git diff --check
Forbidden scope touched
HEAD status
```

不得自动启动 C02B，不得 commit / tag / push。

