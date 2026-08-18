# V0.4-ItemCombatEffectRequestContract01 Assignment

## 1. Package

```text
Package:
V0.4-ItemCombatEffectRequestContract01

Workflow:
COMPLEX_GUARDED_ONCE

Guard marker:
ITEM_GUARD_PASS_ITEMCOMBATEFFECTREQUESTCONTRACT01

Primary owner:
Item

Implementation owner:
NEW INDEPENDENT ITEM COMBAT DEVELOPMENT TASK

Risk:
YELLOW / NEW IMMUTABLE DEVONLY ITEM COMBAT REQUEST CONTRACT

User handtest:
NOT_APPLICABLE_FOR_THIS_UPSTREAM_CONTRACT
```

真实工程：

```text
F:\Porject\TalismanBagBrawl
```

本 Assignment 由 Item Guard 在用户与 Overall Guard 已批准真实 Item 伤害竖切后一次性冻结。

开发任务开始前必须完整读取：

```text
F:\Porject\TalismanBagBrawl\AGENTS.md
F:\Porject\TalismanBagBrawl\Docs\LOCKED\*
F:\Porject\TalismanBagBrawl\Docs\ROADMAP\VERSION_ROADMAP.md
F:\Porject\TalismanBagBrawl\Docs\V0.4\BUILD_PACKAGE_QUEUE.md
F:\Porject\TalismanBagBrawl\Docs\V0.4\BUILD_PHASE2_PACKAGE_QUEUE.md
```

本包不得路由到仍保留托盘 REV02 手测历史的任务：

```text
019f8ab4-fdcc-7332-8a1a-da8eada76011
```

必须使用新的独立 Item Combat 开发任务。新任务在本 Assignment 派发前还必须重新核对本文件 SHA-256、精确白名单和保护基线。

## 2. 用户与 Overall 已冻结的目标

建立不可变、只读、确定性的 devOnly 合同：

```text
ItemCombatEffectRequestSnapshot.v1
```

它只把现有 Item 权威事实转换为 Battle 可以选择消费的“预减伤效果请求”，不执行战斗。

当前 V1 必须覆盖真实 BattleSandbox 普通实例 I007-I012：

```text
I007 / wb_i007_orange_404310007 / rootSeed 404310007
I008 / wb_i008_orange_404310008 / rootSeed 404310008
I009 / wb_i009_orange_404310009 / rootSeed 404310009
I010 / wb_i010_orange_404310010 / rootSeed 404310010
I011 / wb_i011_orange_404310011 / rootSeed 404310011
I012 / wb_i012_orange_404310012 / rootSeed 404310012
```

全部为：

```text
rarity = Orange
rarityKey = orange
cultivationLevel = 40
cultivationLevelSource = BattleSandboxDevHandtestAllLv40
```

当前真实资格保持不变：

| baseItemId | itemInstanceId | BuildQualification | FaMen | QiLei |
|---|---|---|---|---|
| I007 | wb_i007_orange_404310007 | Dual | famen:lihuo | qilei:fu |
| I008 | wb_i008_orange_404310008 | QiLeiOnly | - | qilei:fu |
| I009 | wb_i009_orange_404310009 | FaMenOnly | famen:lihuo | - |
| I010 | wb_i010_orange_404310010 | Dual | famen:lihuo | qilei:ling |
| I011 | wb_i011_orange_404310011 | None | - | - |
| I012 | wb_i012_orange_404310012 | Dual | famen:lihuo | qilei:fa |

在冻结的真实布局中：

```text
ordinary items lit/countable = 6
qualified LiHuo contributors = 4
LiHuo Build2 = active
LiHuo Build4 = active
LiHuo Build6 = inactive
```

不得把 I008 或 I011 伪造成 LiHuo Build 贡献者，也不得为了 Build6 修改 seed、Roll 或资格。

## 3. 层级与所有权

本包只属于：

```text
Item / immutable read-only effect-request construction
```

### 3.1 Item 拥有

- Item 实例、原型、品阶、seed 身份。
- Item 的基础 `damage` rawUnits。
- Item 已 Roll 的 Affix 身份与 rawUnits。
- ItemSystem.v2 的 placement、occupied cells、lit、count facts。
- QualifiedBuild 的实例资格和已计算 Build 阶段。
- CoreRuntime 的四核心 eligible/visible/unlocked/active 事实。
- 依据上述事实构造预减伤 Item 效果请求。
- 对当前不能执行的机器事实输出 `NotExecuted/Unknown` 遥测。

### 3.2 Battle 拥有，禁止本包实现

- 1.5 秒或任何其他战斗时钟。
- pulse ordinal、application sequence、dedup ledger。
- 目标选择、目标合法性确认、命中判定。
- 伤害应用、减伤、护壳/HP 分配。
- 浮字、攻击节奏、战斗结束。

### 3.3 Enemy 拥有，禁止本包实现

- 守骨奴 HP、护壳、易伤、露核、恢复、技能阈值和敌人动作。
- 盲女的叙事/表现；盲女仍没有 Runtime Actor、HP 或受击身份。

### 3.4 Visual 拥有，禁止本包实现

- Hit、ShellBreak、CoreExpose 等表现。
- 动画、VFX、音效、伤害数字。
- Visual 只能消费未来 Battle 批准的 cue，不能成为伤害真源。

## 4. 权威输入

Assembler 的公开输入必须只包含以下现有只读对象：

```text
ItemInstanceProjectionSetSnapshot projectionSet
ItemSystemSnapshot itemSystemSnapshot
ItemInstanceQualifiedBuildStateSnapshot qualifiedBuildState
ItemInstanceCoreEffectRuntimeStateSnapshot coreEffectRuntimeState
```

不得直接读取 Scene、View、Prefab、ItemDetail、文本标签或 Sprite。

不得从 `baseItemId`、中文名、图标或描述文本猜数值。

不得使用旧的 `ModifierEventBridge`、`CombatModifierBundle` 或 `EffectEventBundle` 作为伤害真源；它们是既有 preview/fallback，必须保持只读。

### 4.1 输入有效性

只有全部满足时才允许发出可执行请求：

```text
projectionSet.schemaId == ItemInstanceProjectionSetSnapshot.v1
projectionSet.isValid == true
projectionSet ordinary roster == exact I001-I030 / 30 unique instances
itemSystemSnapshot.schemaVersion == ItemSystemSnapshot.v2
itemSystemSnapshot.isValid == true
qualifiedBuildState.schemaId == ItemInstanceQualifiedBuildStateSnapshot.v1
qualifiedBuildState.isValid == true
qualifiedBuildState.rosterCompleteness == Complete
coreEffectRuntimeState.schemaId == ItemInstanceCoreEffectRuntimeStateSnapshot.v2
coreEffectRuntimeState.isValid == true
coreEffectRuntimeState roster == complete four-core authority
```

I031 必须保持特殊系统道具，不得生成普通 ItemCombat 请求或普通实例行。

### 4.2 精确身份联结

普通棋盘实例必须按以下三层身份精确联结：

```text
itemInstanceId
placementId
baseItemId
```

还必须一致：

```text
Projection rarity/rootSeed
ItemSystem placement.itemId
QualifiedBuild item location/placement/rarity
CoreRuntime item location/placement/rarity
```

禁止：

- 用 `baseItemId` 猜 `itemInstanceId`。
- 用上一帧 placement 或上一件 Item 的状态继续出请求。
- 缺失、重复、孤儿或错配时返回零伤害请求。

身份或权威签名矛盾时必须 `Invalid` 且 `Requests.Count == 0`。

事实暂不可用但没有身份矛盾时必须 `Unknown` 且 `Requests.Count == 0`。

## 5. 来源签名

输出必须保存并验证：

```text
sourceProjectionSetCanonicalSignature
sourceQualifiedProjectionSetIdentity
sourceBindingCanonicalSignature
sourceItemSystemCanonicalSignature
sourceQualifiedBuildCanonicalSignature
sourceCoreRuntimeCanonicalSignature
```

计算与交叉验证规则：

1. `sourceProjectionSetCanonicalSignature` =
   SHA-256(`projectionSet.BuildCanonicalSignature()`).
2. `sourceItemSystemCanonicalSignature` =
   SHA-256(`itemSystemSnapshot.BuildDebugSignature()`).
3. `qualifiedBuildState.sourceItemSystemCanonicalSignature` 必须等于第 2 项。
4. `coreEffectRuntimeState.sourceItemSystemSignature` 必须等于第 2 项。
5. `qualifiedBuildState.sourceBindingCanonicalSignature` 必须非空并等于
   `coreEffectRuntimeState.sourceBindingSignature`。
6. `coreEffectRuntimeState.sourceProjectionSetSignature` 必须等于第 1 项。
7. `qualifiedBuildState.sourceProjectionSetIdentity` 必须非空，原样保存在输出中。
8. 输出保存 `qualifiedBuildState.canonicalSignature` 和
   `coreEffectRuntimeState.canonicalSignature`。

不得复制或建立第二套 ItemSystem、QualifiedBuild 或 CoreRuntime Owner。

## 6. 输出 Schema

### 6.1 Snapshot

至少包含：

```text
schemaId = ItemCombatEffectRequestSnapshot.v1
authorityRevision = ITEM_COMBAT_EFFECT_REQUEST_CONTRACT01_R1
status = Valid | Unknown | Invalid
devOnly = true
entersFormalBattle = false
sourceProjectionSetCanonicalSignature
sourceQualifiedProjectionSetIdentity
sourceBindingCanonicalSignature
sourceItemSystemCanonicalSignature
sourceQualifiedBuildCanonicalSignature
sourceCoreRuntimeCanonicalSignature
requestCount
telemetryCount
Requests
UnsupportedTelemetry
ValidationErrors
canonicalSignature
```

`Valid` 可以同时包含明确的 `NotExecuted/Unknown` 遥测；这不等于把未知事实当成零。

### 6.2 可执行 Request row

每条至少包含：

```text
requestId
requestKind = DirectFlatDamage
sourceItemInstanceId
sourceBaseItemId
sourcePlacementId
sourceRarity
sourceRootSeed
sourceFaMenTag
sourceQiLeiTag
sourceEffectIds
baseDamageRawUnits
additiveBasisPoints
resolvedPreMitigationDamageUnits
magnitudeCompleteness = Complete
requiresBattleTargetValidation = true
targetRequestKind = SingleHostileDamageableRuntimeActor
isLitFact
sourceIsCountedFact
qualifiedIsCountedFact
faMenBuildId
faMenBuildCount
faMenActiveStagePieceCount
activeCoreEffectIds
sourceProjectionCanonicalSignature
sourcePlacementCanonicalFacts
```

`targetRequestKind` 只是 Item 对 Battle 的目标合法性请求，不包含 targetId，不代表 Battle 已接受。

### 6.3 Contribution row

每条 damage request 必须附不可变贡献明细：

```text
sourceKind = BaseStat | Affix | Build
sourceEffectId
operation
targetStatId
valueUnitKey
rawUnits
stackCount
appliedBasisPoints
disposition = AppliedToRequest
sourceDataMaturity
```

不得只输出一个总伤害而丢失来源。

### 6.4 Unsupported telemetry row

至少包含：

```text
sourceItemInstanceId
sourceBaseItemId
sourcePlacementId
sourceEffectId
sourceKind
machineOperation
machineUnit
magnitudePresence = Present | Missing | Conflicted
magnitudeUnits
disposition = NotExecuted | Unknown | Rejected
reasonCode
sourceIdentities
```

Unknown 与 Known Zero 必须在 Canonical 中不同。

## 7. V1 唯一允许执行的伤害映射

V1 只允许以下机器事实进入直接伤害：

### 7.1 Base stat

```text
statId = damage
unit = flat rawUnits
```

必须恰好一条且非负。缺失、重复或负值时该实例不得发请求。

### 7.2 damage_up

```text
affixId = affix_damage_up
effectCategory = NumericModifier
operation = AddPercent
targetStatId = damage
triggerEventId = always
conditionId = NONE
valueUnitKey = basisPoint
```

使用 Projection 中该实例已经 Roll 的 `rawUnits`。当前真实样本要求它位于固定 Affix 槽；不得重新 Roll 或从 Candidate 范围取中点。

### 7.3 I008 使用的相邻同法门增伤

机器合同：

```text
affixId = affix_adjacent_damage
effectCategory = SpatialEffect
operation = AddPercent
targetStatId = damage
triggerEventId = on_layout_evaluate
conditionId = adjacent_same_famen
valueUnitKey = basisPoint
stackLimit = 3
```

通用计算规则：

1. 使用 `ItemSystemSnapshot.v2` 的权威 `OccupiedCells`。
2. 只计算上下左右共享边，不算对角。
3. 同一相邻实例即使共享多个边也只计一层。
4. “同法门”读取 `ItemSystemSnapshot.catalogItems.faMenTag`，不得读取
   BuildQualification；I008 虽不是 FaMen qualified，仍属于 catalog `lihuo`。
5. 不计 self、I031、重复 placement、越界或身份错配。
6. 最终层数 `min(distinctAdjacentSameFaMenInstances, stackLimit)`。

冻结布局中 I008 必须得到：

```text
distinct adjacent same-LiHuo instances = 3
rolled rawUnits per stack = 613 basisPoint
applied = 1839 basisPoint
```

不得写 I008 或其他 itemId 特判。

### 7.4 I011 使用的伤害代价词条

机器合同：

```text
affixId = affix_damage_cost_trade
effectCategory = TradeoffEffect
operation = AddPercent
targetStatId = damage
triggerEventId = always
conditionId = NONE
valueUnitKey = basisPoint
```

Projection 的伤害分支 `rawUnits = 2200 basisPoint` 进入直接伤害。

候选 payload 的资源代价分支不由本包执行；必须另外产生：

```text
disposition = NotExecuted
reasonCode = RESOURCE_OWNER_NOT_IN_ITEM_REQUEST_CONTRACT_V1
```

不得因为资源代价未执行而删除伤害分支，也不得假装资源代价已支付。

### 7.5 LiHuo Build2

只允许消费现有 QualifiedBuild 结果，不得重算 2/4/6 阈值。

机器合同：

```text
buildId = famen:lihuo
effectId = famen:lihuo:build2:effect
stagePieceCount = 2
operation = AddPercent
targetStatId = damage
valueUnitKey = basisPoint
valueUnits = 800
conditionId = famen_stage_2
```

只有同一实例同时满足才应用：

```text
qualifiedBuildState location = Board
qualifiedIsCountedFact = True
contributesToFaMen = True
eligibleFaMenBuildId = famen:lihuo
FaMen track famen:lihuo activeStagePieceCount >= 2
```

因此当前只给 I007/I009/I010/I012 加 800 BP；I008/I011 不加。

## 8. 明确不执行的事实

以下事实必须保留遥测，禁止进入 V1 伤害：

1. `famen:lihuo:build4:effect`
   - machine operation = `ExtraTrigger`
   - 当前 Build4 已激活
   - V1 不拥有 trigger ledger，因此 `NotExecuted`。
2. `famen:lihuo:build6:effect`
   - 当前 Build6 未激活
   - 不得伪造第 5/6 个 qualified LiHuo 贡献者。
3. 余焰、爆燃、燃烧、持续伤害等仅靠文本可读但没有本 V1 已批准执行合同的语义。
   - 必须保持 `Unknown/NotExecuted`。
4. 所有四核心 Core1/Core2/Core3/ULT。
   - 保留 exact candidateDefinitionId、awakeningNodeId、eligible/visible/unlocked/active。
   - 不提供伤害数值，不进入直接伤害。
5. I012 `affix_cooldown_reduction`。
   - Projection rawUnits 当前为 1584，但 Candidate machine unit 为 `turn`。
   - 此单位冲突必须是 `magnitudePresence = Conflicted`、
     `disposition = NotExecuted`，不得当作 1584 BP。
6. I011 tradeoff 的资源代价分支。
7. `duration_up`、`nian_efficiency`、`trigger_refund`、signature affix 或任何
   未列入第 7 节白名单的效果。

禁止通过中文 display text、描述、道具名或美术推断 burn、chain、cooldown、core 或额外触发数值。

## 9. 确定性算术

全部使用 checked integer/long 算术，不得使用 float/double：

```text
resolvedPreMitigationDamageUnits =
floor(baseDamageRawUnits * (10000 + additiveBasisPoints) / 10000)
```

要求：

- 所有输入非负。
- additiveBasisPoints 由已批准 contribution 相加。
- 中间乘法溢出时整个 Snapshot `Invalid`，不得 clamp。
- 除法对非负整数自然向下取整。
- 输出是 devOnly、pre-mitigation，不是 Enemy 实际 HP delta。

当前精确结果必须为：

| Item | base damage | damage_up BP | adjacency BP | trade BP | Build2 BP | final |
|---|---:|---:|---:|---:|---:|---:|
| I007 | 24 | 1454 | 0 | 0 | 800 | 29 |
| I008 | 32 | 1327 | 1839 | 0 | 0 | 42 |
| I009 | 44 | 1450 | 0 | 0 | 800 | 53 |
| I010 | 37 | 1662 | 0 | 0 | 800 | 46 |
| I011 | 40 | 1740 | 0 | 2200 | 0 | 55 |
| I012 | 62 | 1527 | 0 | 0 | 800 | 76 |

顺序按 `itemInstanceId` Ordinal；不得按 Hierarchy、中文名或当前显示顺序。

## 10. 真实布局 Fixture

Verifier 必须使用当前真实 Item 数据、当前 deterministic instances 和生产 ItemSystem/
Binding/QualifiedBuild/CoreRuntime assembler 构造以下内存布局；不得手工伪造最终快照行：

```text
boardSize = 5
eyeCell = (2,2)

P_SYSTEM_I031 / I031 / anchor (0,0) / rotation 0
P_BOARD_I009 / I009 / anchor (1,0) / rotation 0
P_BOARD_I012 / I012 / anchor (2,0) / rotation 0
P_BOARD_I010 / I010 / anchor (1,1) / rotation 90
P_BOARD_I008 / I008 / anchor (0,2) / rotation 0
P_BOARD_I011 / I011 / anchor (1,3) / rotation 0
P_BOARD_I007 / I007 / anchor (0,4) / rotation 0
```

权威形状保持：

```text
I007 shape_single_1  (0,0)
I008 shape_line2_v   (0,0);(0,1)
I009 shape_single_1  (0,0)
I010 shape_corner3   (0,0);(1,0);(0,1)
I011 shape_line2_v   (0,0);(0,1)
I012 shape_line2_v   (0,0);(0,1)
```

预期：

- 无越界、无重叠、阵眼 `(2,2)` 未覆盖。
- 六件普通 Item 全部 lit/countable。
- I008 相邻同 LiHuo 实例精确为 3。
- 当前资格仍是 4 个 LiHuo，Build2/4 active，Build6 inactive。
- Orange Lv40 四核心全为 01/02/03/ULT；无 Core4 第五行。
- I031 普通请求行 = 0。

## 11. Canonical

`canonicalSignature` 必须对稳定 Canonical payload 做 SHA-256。

Canonical 至少明确写入：

- schema/revision/status/devOnly/formal-flow flags。
- 六个来源签名。
- 每个请求的三层身份、rarity、seed、lit/count/qualification facts。
- 每个 contribution 的 effectId、operation、unit、raw、stack、applied。
- 每个 unsupported telemetry 的 magnitude presence、disposition、reason。
- ValidationErrors。

排序：

```text
Requests:
sourceItemInstanceId Ordinal
-> sourcePlacementId Ordinal
-> requestKind Ordinal

Contributions:
sourceKind enum rank
-> sourceEffectId Ordinal

UnsupportedTelemetry:
sourceItemInstanceId Ordinal
-> sourceEffectId Ordinal
-> reasonCode Ordinal

ValidationErrors:
code Ordinal
-> itemInstanceId Ordinal
-> placementId Ordinal
-> message Ordinal
```

同输入重复 Assemble 必须返回相同 Canonical 内容；Assembler 保持无状态，不缓存 mutable
Battle 状态，不通知 UI。

Unknown、Known Zero、NotExecuted 和 Invalid 的 Canonical 必须彼此不同。

## 12. 精确开发白名单

本包只允许新增下列 16 个文件：

```text
Assets/_Game/Scripts/TalismanBag/Items/Combat.meta
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestContract.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestAssembler.cs.meta
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs
Assets/_Game/Scripts/TalismanBag/Items/Combat/ItemCombatEffectRequestValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCombatEffectRequestContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemCombatEffectRequestContractVerifier.cs.meta
Docs/V0.4/Reports/ItemCombatEffectRequestContractReport.md
Docs/V0.4/Reports/ItemCombatEffectRequestContractSpec.csv
Docs/V0.4/Reports/ItemCombatEffectRequestRealSampleMatrix.csv
Docs/V0.4/Reports/ItemCombatEffectRequestFieldLineage.csv
Docs/V0.4/Reports/ItemCombatEffectRequestUnsupportedTelemetry.csv
Docs/V0.4/Reports/ItemCombatEffectRequestCanonicalMatrix.csv
Docs/V0.4/Reports/ItemCombatEffectRequestLeakCheckReport.md
```

规则：

- 既有文件修改数必须为 `0`。
- `Docs/V0.4/Reports/*.meta` 禁止新增。
- 如果 Unity 导入生成任何非白名单 `.meta`，立即停止并按本进程副作用精确归因；
  不得认领或删除其他任务文件。
- 新目录当前不存在，不得借机移动既有 Item 文件。

## 13. 保护基线

以下为 Guard 冻结时当前磁盘 SHA-256。新开发任务开工时必须逐项重验；任何差异不得按
Git HEAD 清理或覆盖，必须停在 `BASELINE_DRIFT_HOLD` 由 Item Guard 归因。

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs` | `f40323a05b3796c706f0225864b93949881a3977b786d51d5b3b78193559d967` |
| `Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs` | `335aa9a83792cd93a2286e65840478d3844ba21e77f2081bcd531415e15252f0` |
| `Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateContract.cs` | `f06d5efee730a2dcce666472a00d2234250cf77f2a0715edee5989ecd83c1464` |
| `Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateAssembler.cs` | `798e1406a5f0c775facb82ac7f9fb63574d979470926309bd383ae7b054883e8` |
| `Assets/_Game/Scripts/TalismanBag/Items/Build/Qualified/ItemInstanceQualifiedBuildStateValidation.cs` | `95341ddbc3572a1d116287ac4edb221698d56b1b7133768daa45fbee02c0179d` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemCoreEffectIdentityAndRuntimeStateContract.cs` | `451b6d3603278a7bcbc983b69e428685c1ebdb9391ebc02cbe9644448118166a` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateAssembler.cs` | `e4ca29265d3bb3c18e7b1ff8c3de73190a1dc72b8b2c1d01f03f529343c6c904` |
| `Assets/_Game/Scripts/TalismanBag/Items/Awakening/RuntimeState/ItemInstanceCoreEffectRuntimeStateValidation.cs` | `f3534fff59bd46852c5918755caa63a79f4beec7915c876fa51aeb9ded0fd0e6` |
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `794c3a6d9dbe9fb1ff96275c247bcef4d9d248edc1df3eaee6d396dd87969827` |
| `Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs` | `15d7a0cbf1b60969e6c27030a7d1d8ccd1d733fb73421396bee5a8ee21382b4b` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemInstanceRollEngine.cs` | `19e989ffb535d8cd268c94d216300cc3d98d63b9ff13e50551a0ae2796ada63e` |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation/Rolling/ItemGeneratedInstanceSnapshot.cs` | `8ed5da129d0819978c98e0ef53deac273cc9dbc9d10f5dc9e28cbeb71fba18ef` |
| `Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalog.cs` | `606acdc6538eeda86f7631840a6acbb47284368f198ef413293bb844833776c9` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxViewProjection.cs` | `4e24931a20c3774278431fa0b3052ce3b5deb40dd0d74e00cb731e2307a00e86` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs` | `6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAdapter.cs` | `116dece9563cbd7eceb211d641e0cbc91c7cf8554cc8a32aa98d566ba62aa204` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ModifierEventBridge.cs` | `e5b2180d4468d73af3fa616efdab1bd21a3769072289f04ab658aa672c898449` |
| `Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset` | `d459e8156ca7513df1f57ec3b1379bf49a676d0e4008de7c1ec6e453e5e9beed` |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity` | `ff2d422476b94b525c0c48c261543ac8f73d55a41be1b70340b485cb73ee0796` |
| `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` | `2a6924da0823b74e2639b593926d8c2f8422101b01edbe4073af33cd424e027c` |
| `ProjectSettings/EditorBuildSettings.asset` | `08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59` |

30 Profiles aggregate：

```text
Algorithm:
ItemBalanceProfile_I001.asset ... I030.asset
按文件名 Ordinal 排序
每行 filename|lowercase-file-sha256
以 LF 连接
对 UTF-8 payload 做 SHA-256

Expected:
71fc7080eceaf9adf2aa47508e4900d00fae91d8f8158fb1a2618b69241a8143
```

额外保护：

| Path | SHA-256 |
|---|---|
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxRuntimeLoop.cs` | `301adbab43046acde70cfc51ef85ee60642b15e52caaf7ebc980dc4c3fcff1ab` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` | `519a7b69fc16a9c50563ceb34f614967ae58854c57f4846d82bdcdee9093523a` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` | `a7957ffd262dc5231622e8762c644ebad1ccb5d0b662325a4fe9ec62d23d497f` |
| `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` | `448a69762ec157214809e1d43cd19a134d77f7928742cc890a94a41b1588d56e` |
| `Docs/V0.4/ShougunuPhase1RuntimeAndActionContract01_Assignment.md` | `40f244475a2aabadffb8ba440c011ec8a7dae46c69043db7f93e928788c0c757` |

开发任务还必须把 `EnemySystem/**`、Shougunu Visual controller、全部 Scene/Prefab、
ItemDetail、Save/Reward/RunFlow、正式 Battle 和其他并行任务写域视为只读，不得因未逐项列 hash
而获得修改权限。

## 14. Verifier 必须覆盖

Verifier 必须直接消费当前磁盘的真实 Catalog/Profile/Projection，并使用生产 assembler 生成
ItemSystem、Binding、QualifiedBuild、CoreRuntime。禁止只用手写最终 DTO。

最低断言：

1. 当前普通 roster 30/30、I031 ordinary rows 0。
2. I007-I012 exact instance ID、rootSeed、Orange、Lv40 source 全匹配。
3. 第 10 节布局合法，六件 lit/countable。
4. Qualified LiHuo 4，Build2/4 active，Build6 inactive。
5. 四核心每件 4 行，01/02/03/ULT，active 4/4，无 Core4。
6. 六条 direct request 精确为 `29,42,53,46,55,76`。
7. 每条 base/damage_up contribution 可追溯。
8. I008 adjacency = 3、613 BP/stack、1839 BP total。
9. I011 trade damage = 2200 BP；资源代价未执行遥测存在。
10. Build2 只应用到 I007/I009/I010/I012，精确 800 BP。
11. Build4 active 但 ExtraTrigger 未执行；Build6 inactive。
12. active core identities 保留但 core damage contribution = 0 rows，不得写成数值 0。
13. I012 cooldown unit conflict 明确为 `Conflicted/NotExecuted`。
14. Unsupported 与 Unknown 不影响已批准直接请求，但 Canonical 保留。
15. stale placement、instance/base mismatch、重复 identity、orphan、signature mismatch 全部
    fail closed，Requests 为空。
16. Unknown facts 不暴露 zero magnitude。
17. known-zero fixture 与 Unknown Canonical 不同。
18. 同输入 deterministic；输入顺序扰动后 Canonical 不变。
19. 所有上游输入对象和 Canonical 在 Assemble 前后引用/内容不变。
20. 不引用 Enemy/Battle/Visual/Scene/UI 类型。
21. 16/16 白名单文件精确，非白名单改动 0。
22. package-scoped `git diff --check` PASS；不得清理范围外脏工作。

必须输出标记：

```text
COMPONENT_FIXTURE_PASS
REAL_CURRENT_ITEM_STATE_ASSEMBLY_PASS
ITEM_COMBAT_EFFECT_REQUEST_CONTRACT_PASS
DIRECT_DAMAGE_REQUESTS_29_42_53_46_55_76_PASS
LIHUO_BUILD2_800BP_PASS
I008_ADJACENCY_THREE_PASS
I011_TRADEOFF_DAMAGE_BRANCH_PASS
UNSUPPORTED_UNKNOWN_NOT_ZERO_PASS
STALE_MISMATCH_FAIL_CLOSED_PASS
I031_EXCLUSION_PASS
UPSTREAM_CANONICAL_IMMUTABLE_PASS
PROTECTED_HASHES_PASS
LEAKCHECK_PASS
PACKAGE_SCOPED_GIT_DIFF_CHECK_PASS
USER_HANDTEST_NOT_APPLICABLE
BATTLE_BRIDGE_NOT_STARTED
```

## 15. Unity 与 QA

本包是纯 C# 上游合同，不需要 Scene Play 或用户手测。

允许一次专用 batch：

```text
F:\2022.3.50f1c1\Editor\Unity.exe
-batchmode
-quit
-projectPath F:\Porject\TalismanBagBrawl
-executeMethod TalismanBag.EditorTools.ItemSandbox.ItemCombatEffectRequestContractVerifier.VerifyStaticBatch
```

执行前由开发任务自行遵守共享 Unity lease：

- Unity/Tuanjie/ShaderCompiler/bee processes = 0。
- `Temp/UnityLockfile` absent。
- `Library/EditorInstance.json` absent。
- 不关闭用户 Editor，不删除他包 lock。

完成条件：

```text
DEV_COMPLETE
QA_PASS
ITEM_COMBAT_EFFECT_REQUEST_CONTRACT_PASS
USER_HANDTEST_NOT_APPLICABLE
BATTLE_BRIDGE_NOT_STARTED
```

本包通过只表示 Item 能诚实地产生 devOnly 预减伤请求，不表示守骨奴已受伤或竖切已可玩。

## 16. 禁止

本包不得：

- 修改任何既有 `.cs`、`.asset`、`.meta`、Scene、Prefab 或 ProjectSettings。
- 修改 Roll、RNG、概率、品阶、词条、BuildQualification 或 Item 形状。
- 修改 ItemSystemSnapshot.v2、Binding、QualifiedBuild、CoreRuntime 的语义或 Owner。
- 修改 I031。
- 修改 BattleSandbox 托盘、棋盘交互、ItemDetail 或手调 UI。
- 修改 Enemy、守骨奴 HP/护壳/技能、盲女、Visual controller。
- 建立 Battle clock、target ledger、damage ledger 或 Enemy HP reducer。
- 执行 Build4 ExtraTrigger、Build6、burn、core 或 cooldown。
- 把 Unknown/NotExecuted/Conflicted 转成 0。
- 使用旧 ModifierEventBridge preview 值作为伤害真源。
- 启动后续 Battle/Bridge、Scene integration、Prefab migration 或正式 Battle。
- 运行 Builder、历史 verifier 或全仓资产重建。
- 修改 AGENTS、LOCKED、Queue。
- `git add/commit/tag/push/reset/rollback/clean`。

## 17. 开发回执

开发完成后只做一次合并状态同步：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-ItemCombatEffectRequestContract01

Workflow:
COMPLEX_GUARDED_ONCE

Result:
DEV_COMPLETE / QA_PASS
ITEM_COMBAT_EFFECT_REQUEST_CONTRACT_PASS
USER_HANDTEST_NOT_APPLICABLE
BATTLE_BRIDGE_NOT_STARTED
```

附：

- 16/16 精确文件。
- Assignment SHA-256 复核。
- 六样本请求与 contribution/telemetry 矩阵。
- protected hash、Unity exit code、diff-check、LeakCheck。
- 无 Git 操作。

不得自动启动 Battle/Bridge 或 Scene integration。
