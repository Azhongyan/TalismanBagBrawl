# V0.4-DevEncounterLayoutPressureAuthoring01 Joint Guard Assignment

## 1. 状态与回执

```text
Package: V0.4-DevEncounterLayoutPressureAuthoring01
Queue Id: N01C-P5-A
Risk: YELLOW / DEVONLY CROSS-SYSTEM AUTHORING / NO BEHAVIOR CHANGE
Status: CROSS_SYSTEM_GUARD_REVISION_1 / WAITING_JOINT_GUARD_REREVIEW

GUARD_PASS_ASSIGNMENT_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01_REVISION01
Required Enemy Guard receipt: ENEMY_GUARD_CONFIRM_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01
Required Algorithm Guard receipt: CAPABILITY_ALGORITHM_GUARD_PASS_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01
Target completion receipt: GUARD_PASS_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01
```

收到两个联合 Guard 回执前不得开发。回执齐全后，本 Assignment 自动进入 `READY_FOR_DEV`，不得扩包。

## 2. 包体目标

本包只为用户已批准的两个结构机制、四个明确 Encounter/Map Context 作者化候选 5x5 压力快照：

```text
formation_eye:
- dev_seed_4_10_furnace_core / dev_encounter_4_10_furnace_core / dev_map_furnace_ash_fall
- dev_seed_4_10_thunder_fire_cross / dev_encounter_4_10_thunder_fire_cross / dev_map_bluestone_crack

polluted_tile:
- dev_seed_3_10_cleanse_corner / dev_encounter_3_10_cleanse_corner / dev_map_bluestone_damp
- dev_seed_4_10_furnace_core / dev_encounter_4_10_furnace_core / dev_map_furnace_ash_fall
```

输出必须包含可读 5x5 格图，供用户在包完成后确认坐标。候选数据可以被 P2 Adapter 验证为 Complete，但必须保持：

```text
userCoordinateAccepted = false
activated = false
devOnly = true
isEnabled = false
migrationCreated = false
evaluationExecuted = false
```

本包不是 Survey，不增加能力分类；也不是 Migration，不创建真实 N01B row，不调用 P3/P4/N01C Evaluator。

## 3. 用户已批准的 D1-D9

```text
D1 只推进 formation_eye 与 polluted_tile；bronze formation general、spirit thief deferred。
D2 使用显式 candidate ID + exact owner/group/key tuple；禁止 group-only migration。
D3 只按明确 Encounter/Map Context 适用；broader E03-only 不激活。
D4 PressureInputId 按 Context 分开。
D5 每个作者化行必须完整提供八项 P2 字段。
D6 formation_eye clauses:
   CountedLayoutPresent
   EffectiveEyeAnchorUsable
   EyeToCountedCoreStructurallyConnected
D6 polluted_tile clauses:
   CountedLayoutPresent
   CountedPlacementCellsUsable
   CountedPlacementCoresUsable
D7 legacy BP 仅保留 quarantine literal + source hash。
D8 StructuralPredicate 保持独立诊断，不形成总 readiness。
D9 Authoring -> overlay migration -> real evaluation -> user playtest -> later total readiness。
```

## 4. 固定坐标口径

坐标必须统一为：

```text
boardSize = 5
x = 0..4，左到右
y = 0..4，上到下
baseline eye = (2,2)
LayoutDomainCells = 显式作者化列出的全部 25 格
连接只允许上下左右正交边；禁止对角边
每条无向边只保存一次，并按端点规范化
```

以下为 Guard 推荐候选方案。它们用于本包作者化和生成格图，但在用户查看格图并回复通过前不得激活或迁移。

100 个 cell row 与 126 个 edge row 必须作为 Catalog 中逐条写明的显式作者化源数据存在。Catalog 禁止通过循环、`Enumerable.Range`、矩形填充、unusable mask 取补集、切线规则、邻接算法、连接生成器或任何派生函数生成 `LayoutDomainCells`、`UsableCellsAfterPressure` 或 `PreservedStructuralConnectionsAfterPressure`。四张报告和格图只能从同一份显式 Catalog 读取并排序输出，不得另建第二套数据或在报告器中重算。

### 4.1 Formation Eye / Furnace Ash

```text
pressureInputId = pressure.layout_resilience.formation_eye.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
PressureKinds = EyeRelocationOrDisruption + StructuralConnectionCut
UsableCellsAfterPressure = 25/25
EffectiveEyeAnchorCellAfterPressure = (2,1)
PreservedStructuralConnectionsAfterPressure = 显式列出的 35 条边；语义等价于全 5x5 正交边移除所有跨 y=1 与 y=2 的 5 条边
Expected connection count = 35
```

### 4.2 Formation Eye / Bluestone Crack

```text
pressureInputId = pressure.layout_resilience.formation_eye.dev_encounter_4_10_thunder_fire_cross.dev_map_bluestone_crack.v1
PressureKinds = EyeRelocationOrDisruption + StructuralConnectionCut
UsableCellsAfterPressure = 25/25
EffectiveEyeAnchorCellAfterPressure = (3,2)
PreservedStructuralConnectionsAfterPressure = 显式列出的 35 条边；语义等价于全 5x5 正交边移除所有跨 x=2 与 x=3 的 5 条边
Expected connection count = 35
```

### 4.3 Polluted Tile / Bluestone Damp

```text
pressureInputId = pressure.layout_resilience.polluted_tile.dev_encounter_3_10_cleanse_corner.dev_map_bluestone_damp.v1
PressureKinds = PollutedCellMask
UnusableCells = (1,1);(3,3)
UsableCellsAfterPressure = 23/25
EffectiveEyeAnchorCellAfterPressure = (2,2)
PreservedStructuralConnectionsAfterPressure = 显式列出的 32 条仍可用格正交边
Expected connection count = 32
```

### 4.4 Polluted Tile / Furnace Ash

```text
pressureInputId = pressure.layout_resilience.polluted_tile.dev_encounter_4_10_furnace_core.dev_map_furnace_ash_fall.v1
PressureKinds = PollutedCellMask
UnusableCells = (1,1);(3,1);(1,3);(3,3)
UsableCellsAfterPressure = 21/25
EffectiveEyeAnchorCellAfterPressure = (2,2)
PreservedStructuralConnectionsAfterPressure = 显式列出的 24 条仍可用格正交边
Expected connection count = 24
```

不得根据中文名称、Legacy BP、MapRule 标签或占格数量改变上述候选方案。若实现发现合同 Validator 判定某个固定方案 Invalid，必须 `GUARD_RETURN`，不得静默改坐标。

## 5. 唯一身份与上下文绑定

每行必须保存：

```text
candidateMigrationRequirementId
ownerId
requirementGroupId
buildCapabilityKey
seedId
encounterId
mapRuleId
pressureInputId
```

绑定必须 Ordinal exact、case-sensitive。不得 Trim、模糊匹配、按 group 整组命中或从名称拼接 Runtime ID。

身份唯一性冻结为：

```text
CandidateMigrationRequirementId = 2 个机制身份；允许同一 Candidate ID 在不同 Context 复用。
PressureInputId = 4/4 全局唯一，不得复用。
Context identity = candidateMigrationRequirementId + seedId + encounterId + mapRuleId；4/4 组合唯一。
```

只有 PressureInputId 重复、Context identity 重复、同一 Candidate ID 的 owner/group/key 漂移，或同一 Context 绑定到不同 PressureInputId 才是 Invalid。合法跨 Context 复用 Candidate ID 不得报重复。

固定 requirement identity：

```text
layout_resilience.formation_eye.placement_shape
owner = dev_enemy_formation_eye_problem
group = dev_enemy_formation_eye_problem.required
key = capability.placement_shape

layout_resilience.polluted_tile.placement_shape
owner = dev_enemy_polluted_tile_problem
group = dev_enemy_polluted_tile_problem.required
key = capability.placement_shape
```

## 6. Schema 与状态

新增独立 Schema：

```text
DevEncounterLayoutPressureAuthoring.v1
schemaVersion = 1
```

Status 精确冻结为以下三个值，不得增加别名或第四态；所有未定义 enum 值必须 Invalid：

```text
CandidateComplete = 1
Unknown = 2
Invalid = 3
```

Public/dev-only 暴露合同精确冻结如下。所有集合必须 defensive copy + read-only；不得使用 `MonoBehaviour`、`ScriptableObject`、Scene object 或正式 Config：

```text
DevEncounterLayoutPressureAuthoringSource
- schemaId: string
- schemaVersion: int
- devOnly: bool
- isEnabled: bool
- rows: IReadOnlyList<DevEncounterLayoutPressureAuthoringSourceRow>

DevEncounterLayoutPressureAuthoringSourceRow
- candidateMigrationRequirementId: string
- ownerId: string
- requirementGroupId: string
- buildCapabilityKey: string
- seedId: string
- encounterId: string
- mapRuleId: string
- pressureInputId: string
- devOnly: bool
- isEnabled: bool
- userCoordinateAccepted: bool
- activated: bool
- pressureSource: AuthoredLayoutPressureSourceInput

DevEncounterLayoutPressureAuthoringRowSnapshot
- 上述 identity/context/flag 字段原样只读复制
- p2Status: AuthoredLayoutPressureSourceStatus
- p2Completeness: LayoutResilienceInputCompleteness
- pressureSnapshot: LayoutPressureSnapshot
- p2CanonicalSignature: string
- p2Issues: IReadOnlyList<AuthoredLayoutPressureSourceIssue>

DevEncounterLayoutPressureAuthoringIssue
- code: string
- status: DevEncounterLayoutPressureAuthoringStatus
- path: string
- message: string

DevEncounterLayoutPressureAuthoringResult
- schemaId: string
- schemaVersion: int
- status: DevEncounterLayoutPressureAuthoringStatus
- devOnly: bool
- isEnabled: bool
- rows: IReadOnlyList<DevEncounterLayoutPressureAuthoringRowSnapshot>
- issues: IReadOnlyList<DevEncounterLayoutPressureAuthoringIssue>
- canonicalSignature: string
```

类型与 API 暴露精确冻结为：

```text
namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring

public static class DevEncounterLayoutPressureAuthoringSchema
public enum DevEncounterLayoutPressureAuthoringStatus
public sealed class DevEncounterLayoutPressureAuthoringSource
public sealed class DevEncounterLayoutPressureAuthoringSourceRow
public sealed class DevEncounterLayoutPressureAuthoringRowSnapshot
public sealed class DevEncounterLayoutPressureAuthoringIssue
public sealed class DevEncounterLayoutPressureAuthoringResult

public static class DevEncounterLayoutPressureCatalog
  public static DevEncounterLayoutPressureAuthoringSource CreateCandidateSource()

public static class DevEncounterLayoutPressureValidation
  public static DevEncounterLayoutPressureAuthoringResult ValidateAndProject(
      DevEncounterLayoutPressureAuthoringSource source)
```

不得公开可变集合、setter、Runtime registration、provider injection、Scene hook 或正式 Enemy/Map/Battle 接口。`CreateCandidateSource()` 每次返回 defensive copy；`ValidateAndProject()` 是唯一包级入口，并对默认 Catalog 的四行各调用一次 P2 `Project`。

独立用户/隔离状态固定为：

```text
userCoordinateAccepted = false
activated = false
devOnly = true
isEnabled = false
```

缺任一八字段、缺 Context、缺 exact identity 为 Unknown；身份漂移、非法重复、错误 devOnly/isEnabled、未定义 enum 或 P2 返回 Invalid 为 Invalid。Unknown/Invalid 不得补零或降级为 CandidateComplete。

压力空间合法性的唯一权威路径固定为：每个 Context 恰好调用一次 `DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project`。本包 Validator 只检查包级 Schema、身份、Context、固定数量、隔离 flag 和 P2 Result 状态；不得复制 P2/N01C 的越界、domain、usable、eye、edge、clause、kind 空间校验，不得直接调用 N01C Validator/Evaluator。P2 返回 `Complete` 且包级检查通过时才可输出 CandidateComplete；P2 Unknown/Invalid 必须原样提升为本包 Unknown/Invalid。

## 7. 允许新增文件：固定 15 个

```text
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureAuthoringPrimitives.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureAuthoringPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureCatalog.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureValidation.cs
Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/DevEncounterLayoutPressureAuthoringVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/DevEncounterLayoutPressureAuthoringVerifier.cs.meta
Docs/V0.4/Reports/DevEncounterLayoutPressureAuthoringReport.md
Docs/V0.4/Reports/DevEncounterLayoutPressureAuthoringRows.csv
Docs/V0.4/Reports/DevEncounterLayoutPressureAuthoringCells.csv
Docs/V0.4/Reports/DevEncounterLayoutPressureAuthoringConnections.csv
Docs/V0.4/Reports/DevEncounterLayoutPressureAuthoringMaps.md
Docs/V0.4/Reports/DevEncounterLayoutPressureAuthoringLeakCheckReport.md
```

新增必须为 15，修改已有文件必须为 0。Assignment、Queue、CurrentRules、AGENTS、LOCKED 不得由任务窗口修改。

## 8. 验证要求

必须覆盖：

```text
Context rows = 4 exact
Unique pressureInputId = 4/4
Candidate identities = 2 exact
Unique candidate+seed+encounter+map Context identities = 4/4
Board/domain = 5 / 25 each
Usable counts = 25 / 25 / 23 / 21
Connection counts = 35 / 35 / 32 / 24
Explicit authored cell rows / edge rows in Catalog = 100 / 126
Programmatic domain/usable/edge generation paths = 0
P2 Adapter result = 4/4 Complete
DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project calls = exactly 1 per Context / 4 total
Direct N01C Validator calls = 0
Formation clauses = 3 exact
Pollution clauses = 3 exact
userCoordinateAccepted = false for 4/4
activated = false for 4/4
devOnly = true for 4/4 and result
isEnabled = false for 4/4 and result
Migration rows = 0
N01B real rows = 0
P3 calls = 0
P4 calls = 0
N01C Evaluator calls = 0
Readiness aggregation = 0
C02 impact = 0; 32/32 remains blocked
Readable maps = 4/4 and data-consistent
Ordinal / InvariantCulture / reversed-input determinism = PASS
Defensive copy / immutable collections = PASS
Leak Count / GUID conflicts / trailing whitespace = 0
git diff --check = PASS
Forbidden scope touched = 0
HEAD unchanged
Commit / tag / push = none
Next package = NOT_STARTED
```

Verifier 入口固定：

```text
TalismanBag.EditorTools.CrossSystem.ItemEnemy.DevEncounterLayoutPressureAuthoringVerifier.VerifyOffline
TalismanBag.EditorTools.CrossSystem.ItemEnemy.DevEncounterLayoutPressureAuthoringVerifier.VerifyStaticBatch
```

两入口必须使用同一 assertion core。不得运行 Scene/Prefab Builder。

## 9. 报告职责

```text
AuthoringReport.md
  包状态、四 Context、P2 结果、确定性、边界与用户验收门。

AuthoringRows.csv
  4 行 Context/identity/pressureInputId/kinds/eye/clauses/status。

AuthoringCells.csv
  从显式 Catalog 派生 4×25 = 100 行；逐格标记 Domain、Usable、BaselineEye、EffectiveEye。

AuthoringConnections.csv
  从显式 Catalog 派生 126 行；分别 35/35/32/24，记录规范化无向边。

AuthoringMaps.md
  4 张 5x5 可读格图；标注坐标、基准阵眼、有效阵眼、污染格和切断边方向。

LeakCheckReport.md
  白名单、调用、迁移、评估、场景、正式流程和 Protected scope 检查。
```

`AuthoringMaps.md` 必须面向用户可读；不得只输出内部 key。

## 10. Canonical 与保护基线

新包独立 Canonical Signature 必须覆盖 Schema、Status、四 Context 的完整身份、八项 P2 数据、全部显式格和连接、P2 Result/Issue、devOnly/disabled、用户未验收/未激活状态以及零行为影响。

Canonical 编码固定为 UTF-8 无 BOM、每行 LF。文本使用 Ordinal exact；整数使用 InvariantCulture decimal；bool 使用 lowercase `true/false`；null、空字符串与空集合必须区分。排序固定为：Context row 按 candidateMigrationRequirementId、seedId、encounterId、mapRuleId、pressureInputId；cell 按 context 后 Y/X；edge 先规范化端点，再按 context、A.Y/A.X/B.Y/B.X；PressureKinds、clauses 按 enum numeric；P2 issue 与本包 issue 均按 path、code、status、message Ordinal。输入反序、Culture 变化和重复生成必须同签名；任一 identity、cell、edge、clause、flag、status、issue 或 P2 signature 变化必须改签名。

固定前置：

```text
P5-S exact9 = bebd04922410cb83db1690ecd6505d62c58417e151b3b6bc94d7c0774f68a704
P5-S Canonical = sha256:4a77a47181d41ecd18f589f86be911ce08fc466725807a16c42ab1d3e5068126
P2 exact14 = 15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050
P2 Canonical = sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc
P3 exact14 = c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c
P3 Canonical = sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a
P4 exact14 = 8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530
P4 Canonical = sha256:e38538f2de1b85ce00721236790f9b2beab63f19df076a34e0508aad4993c01a
N01C exact16 = 691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b
N01C Canonical = sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335
N01B exact14 = acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316
N01B Canonical = sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326
E10 exact21 = 3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be
Item105 = b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
Enemy89 = 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
Scenes14 = da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs16 = 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings = 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
HEAD = f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba
```

任务开始时还必须按当前磁盘状态记录所有 Protected scope，结束时逐字节复核。不得用 Git HEAD 代替当前磁盘基线，不得清理、回退、暂存或认领既有 dirty/untracked 内容。

## 11. 绝对禁止

```text
修改任何已有文件
修改 Enemy/Item/BuildSandbox/Battle/Board/Map/Config/Scene/Prefab/UI/BuildSettings
创建或迁移真实 N01B requirement row
调用 P3、P4、N01C Evaluator或 readiness
修改 E06/E08/E10、C01/C02/C02A、N02/C02B/C02R1/C03
把候选坐标写入正式关卡、正式地图或正式运行时
把 userCoordinateAccepted 或 activated 设为 true
把 devOnly 设为 false或 isEnabled 设为 true
通过循环、mask 补集、矩形填充、切线或邻接算法生成 Catalog 的 domain/usable/edge 源数据
复制 P2/N01C 空间校验，或绕过每 Context 恰好一次的 P2 Project 权威路径
把 Legacy BP 转成格子、连接、评分、阈值或 fallback
从名称、标签、中文文案自动生成压力事实
运行会保存 Scene/Prefab 的 Builder
commit / tag / push / reset / rollback
自动启动下一包
```

## 12. 完成回传

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-DevEncounterLayoutPressureAuthoring01
Guard receipts
New / modified files
Schema / Canonical Signature
4 exact Context rows
PressureInputIds
Domain / usable / eye / connection / clause counts
P2 Complete results
100 explicit cell rows / 126 explicit connection rows / 4 readable maps
userCoordinateAccepted / activated / devOnly / isEnabled
P2 Project call count / direct N01C Validator-Evaluator calls
Migration / P3 / P4 / Evaluator / readiness calls
C02 blocked rows / Unknown reduction
Protected hashes
Offline verifier
Unity verifier
Leak Count / GUID / whitespace
git diff --check
Forbidden scope
Pre-existing dirty preservation
HEAD status
Commit / tag / push
Next package: NOT_STARTED
```

本包完成后，用户必须查看 `DevEncounterLayoutPressureAuthoringMaps.md` 并明确回复通过或指出修改；在用户通过前不得签发 overlay migration。
