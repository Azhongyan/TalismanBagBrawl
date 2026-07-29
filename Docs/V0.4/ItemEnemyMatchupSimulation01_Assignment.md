# V0.4-ItemEnemyMatchupSimulation01 Guard Assignment

状态：`GUARD_PASS_ITEMENEMYMATCHUPSIMULATION01 / READY_FOR_DEV`  
风险：`YELLOW / CROSS_SYSTEM_OFFLINE_SIMULATION`  
维护方：总体 / 跨系统 Guard  
日期：2026-07-18

## 0. 包定位

本包只做 devOnly、离线、确定性的 Item × Enemy 匹配模拟：

```text
Item 候选实例 / 组合
→ C01 ItemBuildCapabilityProjectionAdapter
→ BuildCapabilitySnapshot
→ E08 EnemyOfflineReadinessEvaluator
→ E10 devOnly Encounter / Pressure / CounterWindow
→ 开发者匹配矩阵与 Unknown 阻断报告
```

本包不执行战斗，不计算真实伤害时间轴，不修改 Item 或 Enemy 数据，不接 BattleSandbox、UnifiedBattle 或正式流程。

## 1. 入场前置

开发窗口必须完整读取真实工程启动规则，以及：

```text
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ItemBuildCapabilityProjectionAdapter01_Assignment.md
Docs/V0.4/Reports/ItemBuildCapabilityProjectionAdapterReport.md
Docs/V0.4/Reports/ItemBuildCapabilityProjectionFieldMap.csv
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
```

必须先做最小只读 Code Survey，确认 C01、E08、E10 的真实 API、Catalog 和 Provider；不得猜测方法名或复制数据事实源。

## 2. 权威输入

### 2.1 C01

必须消费现有：

```text
DefaultItemBuildCapabilityProjectionAdapter
ItemBuildCapabilityProjectionInput
ItemBuildCapabilityProjectionResult
BuildCapabilitySnapshot
FieldMap
UnknownDiagnostics
```

不得复制 C01 映射逻辑，不得在模拟器里增加新的 Item→Capability 特例。

### 2.2 Item

允许通过现有只读 Provider / Contract 构造 devOnly 固定场景：

```text
ItemInstanceProjectionContractSnapshot.v1
现有 Item candidate / generation / projection 只读入口
现有 ItemSystemSnapshot.v1 只读数据
现有 Affix Schema 只读数据
```

禁止直接 new 一套未经 Item Guard 确认的 Item 数值、词条、核心效果、品阶概率或 Build 规则。

### 2.3 Enemy

必须消费现有：

```text
EnemyOfflineReadinessEvaluator
DevEncounterSeedDataSnapshot
E10 四个 devOnly Encounter
E06 Pressure / CounterWindow / Requirement 数据
E07 BuildCapabilityReadContract.v1
E08 ReadinessResult / CapabilityGap
```

不得复制 Encounter、Pressure、CounterWindow 或阈值到 CrossSystem 新事实源。

## 3. 允许新增范围

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation*.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation*Verifier.cs
对应 .meta
Docs/V0.4/Reports/ItemEnemyMatchupSimulationReport.md
Docs/V0.4/Reports/ItemEnemyMatchupMatrix.csv
Docs/V0.4/Reports/ItemEnemyMatchupCapabilityCoverage.csv
Docs/V0.4/Reports/ItemEnemyMatchupUnknownBlockers.csv
Docs/V0.4/Reports/ItemEnemyMatchupLeakCheckReport.md
```

若为保持职责清晰需要拆分纯数据模型与 Runner，可在上述同一前缀和目录中新增文件；不得修改 C01 文件。

## 4. 禁止修改范围

必须满足：

```text
修改既有 C01 文件 = 0
修改既有 Item 文件 = 0
修改既有 Enemy 文件 = 0
修改 Battle / Board / Runtime 文件 = 0
修改 Scene / Prefab / Config = 0
修改 RunFlow / SaveData / Reward / Chapter / Boss 正式流程 = 0
修改 BuildSettings / ProjectSettings = 0
```

禁止：

```text
补写 11 个 Unknown 能力的映射。
把 Unknown 当 0、失败或能力不足。
把 Unknown 当作通过或 Ready。
用道具名称、敌人名称、中文文案或章节编号推导能力。
修改 Item 候选值、Enemy 阈值、Encounter 组成或 CounterWindow。
声明真实胜率、真实 DPS、真实 TTK 或正式难度结论。
生成正式掉落、奖励、存档、章节推进或玩家答案面板。
调用 Battle Runtime、MonoBehaviour、Scene Object 或 UI。
运行 Scene / Prefab Builder。
运行 ItemFullDetailBuildSandboxWorkbenchRegressionRunner 或其他会写资源的历史回归入口。
修改 AGENTS.md / Docs/LOCKED/* / Guard owner docs。
commit / tag / push / reset / rollback。
```

## 5. 模拟场景

至少建立以下 devOnly、确定性场景：

```text
EMPTY：空 Item 集合
OFFENSE_SUPPORTED：包含明确 break capability 来源
DEFENSE_SUPPORTED：包含明确 guard capability 来源
COOLDOWN_SUPPORTED：包含明确 cooldown recovery 来源
MIXED_SUPPORTED：组合三个已支持能力
SPARSE_UNKNOWN：包含 Item 事实但无法映射主要 Enemy 要求
SAME_BASE_MULTI_INSTANCE：同 baseItemId 多实例
RARITY_VARIANTS：同原型至少三品阶
```

场景必须来自现有 Item 只读生成/投影入口；如现有数据无法构造某场景，报告为 `FIXTURE_SOURCE_BLOCKED`，不得手填假数据绕过。

必须覆盖 E10 全部 4 个 devOnly Encounter。每个 Build 场景 × Encounter 至少生成一行矩阵。

## 6. 结果分类

每个匹配结果必须先报告可评估性，再报告 E08 readiness：

```text
EVALUABLE_COMPLETE
EVALUABLE_PARTIAL
BLOCKED_BY_UNKNOWN
NO_RELEVANT_SUPPORTED_CAPABILITY
INVALID_INPUT
```

判定原则：

```text
Encounter 所有决定性要求均为 Known → EVALUABLE_COMPLETE
部分要求 Known、且 E08 可给局部结果 → EVALUABLE_PARTIAL
任一决定性要求为 Unknown → BLOCKED_BY_UNKNOWN
当前 C01 支持能力与 Encounter 要求无交集 → NO_RELEVANT_SUPPORTED_CAPABILITY
合同、引用或输入无效 → INVALID_INPUT
```

`ReadinessBand` 只能在 E08 原本允许的语义内显示。被 Unknown 阻断时不得汇总成“过易 / 合适 / 过难”。

## 7. 输出要求

### 7.1 Matrix

`ItemEnemyMatchupMatrix.csv` 至少包含：

```text
scenarioId
itemSourceSignature
encounterId
chapterLabelDevOnly
evaluationStatus
readinessBand
knownRequirementCount
unknownRequirementCount
metRequirementCount
gapCount
supportedCapabilities
unknownCapabilities
resultCanonicalSignature
notes
```

### 7.2 Coverage

必须按 16 个能力键输出：

```text
capabilityKey
C01 mapping status
Item scenarios with Known value
Encounter requirement references
evaluable matchup count
blocked matchup count
nextAction
```

### 7.3 Unknown Blockers

必须区分：

```text
ITEM_CAPABILITY_MAPPING_MISSING
ITEM_RUNTIME_FACT_MISSING
ENCOUNTER_REQUIREMENT_UNKNOWN
OUT_OF_SCOPE_RUNTIME_TIMING
FIXTURE_SOURCE_BLOCKED
```

报告只能作为开发者诊断，不得生成玩家完整答案。

## 8. 验收合同

本包通过必须同时满足：

```text
C01 / Item / Enemy 保护 hash 前后一致。
修改已有文件数量为 0。
E10 四个 Encounter 全覆盖。
要求的 Item 场景均覆盖，或诚实记录 FIXTURE_SOURCE_BLOCKED。
矩阵行数 = 实际场景数 × 4 Encounter。
Unknown 不被转换为 0、Blocked、Ready 或难度结论。
相同输入重复运行矩阵与 Canonical Signature 一致。
所有结果可追溯到 Item source signature、C01 mapping version、Enemy snapshot signature。
玩家答案字段泄漏为 0。
正式系统泄漏为 0。
Scene / Prefab / Config / Battle / Board diff = 0。
git diff --check PASS。
Offline verifier PASS。
Unity batch compile / verifier PASS；若被用户 Editor 占用，只能如实记录 BLOCKED。
```

## 9. QA 菜单与回传

QA 菜单路径必须包含：

```text
Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/ItemEnemyMatchupSimulation01/[QA Only]
```

完成后只回传：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemEnemyMatchupSimulation01
Guard receipt: GUARD_PASS_ITEMENEMYMATCHUPSIMULATION01
新增文件 / 修改已有文件
C01 / Item / Enemy protected hash
Item scenario count
Encounter count
Matrix row count
EVALUABLE_COMPLETE / EVALUABLE_PARTIAL / BLOCKED_BY_UNKNOWN / NO_RELEVANT_SUPPORTED_CAPABILITY / INVALID_INPUT counts
16-key coverage result
Unknown semantics checks
Determinism signature
Offline verifier
Unity compile / verifier
Leak count
git diff --check
Forbidden scope touched
HEAD changed or unchanged
```

不得自动启动 C03，不得 commit / tag / push。

