# V0.4-DevEncounterSeedData01 Assignment

Guard assignment:

```text
ENEMY_GUARD_ASSIGNMENT_DEVENCOUNTERSEEDDATA01
```

生成日期：2026-07-16
所属：Enemy / Boss / Encounter System Guard
包序号：E10

## 1. 包定位

本包第一次把 E01-E09 的长期数据合同用于真实 devOnly 验证内容：

```text
E01 Domain / References
+ E02 Mechanic Vocabulary
+ E03 normalized Carrier / MechanicProfile / MapRule
+ E04 Encounter / Wave / Slot
+ E05 SkillPattern / Sequence / BossPhase
+ E06 BuildPressure / CounterWindow
+ E09 EnemySystemSnapshot
-> DevEncounterSeedData.v1
```

本包表达四个现有 3-10 / 4-10 Build 验证变体。这里的“题”是遭遇对 Build 能力的检验，不是试卷、选择题、答题 UI 或题目列表。

本包只建立不可变数据、Provider、Validator、player-safe 投影与离线报告。没有任何运行时入口，不执行战斗，不接 BattleContract，不读取 Item，不创建或修改场景。

## 2. 启动必读

开发前必须完整读取：

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UnifiedBattlePageStrategy_GuardSync.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/DevEncounterSeedData01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Composition/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/SkillPhase/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/SystemSnapshot/**
Docs/V0.4/Reports/EnemyValidation*.csv
Docs/V0.4/Reports/EncounterCompositionSchema*.csv
Docs/V0.4/Reports/EnemySkillBossPhaseSchema*.csv
Docs/V0.4/Reports/CounterWindowAndPressureSchema*.csv
Docs/V0.4/Reports/EnemyDataValidatorAndSnapshot*.csv
Docs/V0.4/Reports/DevChapterBalanceRunRows.csv
```

BuildSandbox、Item、Battle 与旧题库代码只允许作为受保护文件或 Editor 报告对照背景，不得成为 E10 Runtime 依赖。

## 3. 事实源与迁移规则

从本包开始必须维持：

```text
E03 = Carrier / MechanicProfile / MapRule 与多对多绑定权威
E04 = Encounter / Wave / Slot 编排结构权威
E05 = SkillPattern / Sequence / CarrierSkillBinding / BossPhase / Plan 结构权威
E06 = Pressure / Requirement / CounterWindow 与 source binding 结构权威
E09 = 六个静态 Catalog 的跨关系校验与根快照权威
E10 = 3-10 / 4-10 新 Enemy 数据路径的内容种子权威
```

旧 `DevChapterBalanceRun`、`BuildProblemSeedData` 与相关报告是迁移来源和对照材料，不是 E10 Runtime 数据源。未来 Enemy 适配器只能消费 E10 Snapshot，不得同时读取 E10 与旧 BuildSandbox 后再合并。

必须生成 Legacy Mapping 报告，明确每个旧 stageId 对应哪个 E10 seedId。不得在 Runtime 调用、包装或复制 `DevChapterBalanceRunBuilder`、`BuildProblemSeedDataset`、`EnemyBossValidationPool`、Item/Build snapshot。

禁止形成新的双重事实源：

```text
不得把四套内容分别硬编码在 Provider、Verifier 和报告中
唯一 Runtime 内容声明位于 DevEncounterSeedDataCatalog
Provider、Validator、签名和报告全部消费同一内容声明
Verifier 只声明期望的 ID/计数/关系，不再建立第二套完整内容对象
报告不得反向成为 Runtime 输入
```

## 4. Schema 与根对象

必须建立：

```text
SchemaId: DevEncounterSeedData.v1
SchemaVersion: 1
DevOnly = true
IsEnabled = false
EntersFormalFlow = false
```

`DevEncounterSeedDataSnapshot` 至少包含等价语义：

```text
SchemaId
SchemaVersion
SeedProfiles
EnemySystemSnapshot
PlayerSafe
CanonicalSignature
PlayerSafeCanonicalSignature
DevOnly
IsEnabled
EntersFormalFlow
```

`EnemySystemSnapshot` 必须由 E10 生成的 E04/E05/E06 Catalog 与当前 E01/E02/E03 Snapshot 经 E09 Provider 创建；不得绕过 E04-E06/E09 Validator，不得复制 E09 Root 类型。

Runtime Provider 输入只允许：

```text
EnemyMechanicVocabularySnapshot
EnemyValidationContentSnapshot
```

E01 Domain 必须从 E03 `DomainSnapshot` 取得。E04/E05/E06 由 E10 唯一内容声明构造，随后交给 E09。不得在 E10 Runtime 读取 Editor normalizer、BuildSandbox、文件系统、CSV、Resources 或 Scene。

## 5. Seed Profile 三层数据

每个 `DevEncounterSeedProfileSnapshot` 必须同时保留三层：

### Player-safe

只允许：

```text
SeedId
PublicEncounterLabelKey
PublicAtmosphereCueKey
PublicFailureObservationKey
PlayerHintCategoryKeys
```

这些字段只描述可观察现象，不给出完整解法、Build 名称、道具、形状、能力阈值或 Boss 钥匙。

### Internal-only

至少包含：

```text
DevChapterLabel
EncounterId
PrimaryMapRuleId
OrdinaryWaveId
DampingWaveId
BossWaveId
BossCarrierId
BossPhasePlanId
BuildPressureProfileIds
CounterWindowIds
```

### Developer-only

至少包含：

```text
SourceScenarioId
ValidationGoalKey
DeveloperDiagnosticCategoryKeys
CompositionNoteKey
```

完整 capability requirements、Any/All、精确阈值和 source binding 继续只存在 E06 developer/internal 层。不得把旧 shapeRuleKey、previewBuildId、具体 ItemId、SynergyId、DropBiasId 或推荐装备复制进 E10。

## 6. 四个权威 Seed

必须恰好建立以下四个 seed；`3-10`、`4-10` 只是 devOnly 标签，不是正式章节路由：

| seedId | dev label | encounterId | MapRule | ordinary slot | damping slot | Boss slot |
|---|---|---|---|---|---|---|
| `dev_seed_3_10_guard_wall` | `3-10` | `dev_encounter_3_10_guard_wall` | `dev_map_copper_bell_night` | `dev_enemy_caster_chanter` -> `dev_enemy_caster_problem` | `dev_enemy_seal_locker` -> `dev_enemy_seal_problem` | `dev_boss_burst_huzhen` -> `dev_boss_bronze_formation_general` |
| `dev_seed_3_10_cleanse_corner` | `3-10` | `dev_encounter_3_10_cleanse_corner` | `dev_map_bluestone_damp` | `dev_enemy_poison_cultist` -> `dev_enemy_poison_burn_problem` | `dev_enemy_burning_wisp` -> `dev_enemy_poison_burn_problem` | `dev_boss_debuff_jinge` -> `dev_boss_dirty_dream_mother` |
| `dev_seed_4_10_furnace_core` | `4-10` | `dev_encounter_4_10_furnace_core` | `dev_map_furnace_ash_fall` | `dev_enemy_spirit_thief` -> `dev_enemy_spirit_thief_problem` | `dev_enemy_formation_eye_jammer` -> `dev_enemy_formation_eye_problem` | `dev_boss_energy_juneng` -> `dev_boss_spirit_thief_core` |
| `dev_seed_4_10_thunder_fire_cross` | `4-10` | `dev_encounter_4_10_thunder_fire_cross` | `dev_map_bluestone_crack` | `dev_enemy_formation_eye_jammer` -> `dev_enemy_formation_eye_problem` | `dev_enemy_spirit_thief` -> `dev_enemy_spirit_thief_problem` | `dev_boss_hybrid_combo` -> `dev_boss_black_furnace_complex_eye` |

每个 Encounter 必须：

```text
3 Wave: ordinary / damping / boss
每个 Wave 恰好 1 Slot
WaveOrder: 0 / 1 / 2
SpawnOrder: 0
Enemy quantity: 1
Boss quantity: 1
前两槽为 Enemy / Normal 或 Elite 语义
末槽为 Boss / Boss 语义
```

总计固定：

```text
4 SeedProfile
4 Encounter
12 Wave
12 Slot
8 Enemy Slot
4 Boss Slot
```

所有 Carrier/MechanicProfile/MapRule 必须通过 E03 解析，所有 Slot 的 Carrier -> selected MechanicProfile 必须存在 E03 binding。

## 7. MapRule 与跨层压力

MapRule 和 Slot mechanic 是可叠加的两个压力来源，不要求 Encounter 中每个 Slot mechanic 都必须同时出现在该 MapRule 的 E03 binding 中。

特别固定：

```text
dev_seed_4_10_furnace_core
= dev_map_furnace_ash_fall 的 polluted/formation 环境压力
+ dev_enemy_spirit_thief_problem 的供能压力
+ dev_boss_spirit_thief_core 的 Boss 压力
```

这是有意的跨层组合，不是孤儿、不新增 E03 exception，也不得为了强行一对一而修改 E03 MapRule binding。

四个 MapRule 的 E06 source 关系必须完整表达：

```text
dev_map_copper_bell_night
  -> caster / seal / bronze-general pressure

dev_map_bluestone_damp
  -> poison-burn / polluted-tile / dirty-dream pressure

dev_map_furnace_ash_fall
  -> polluted-tile / formation-eye / complex-eye pressure

dev_map_bluestone_crack
  -> spirit-thief / formation-eye / complex-eye pressure
```

## 8. E05 内容种子

必须建立可复用、非章节硬编码的机制级 Skill 内容：

```text
9 SkillPattern
9 SkillSequence
10 CarrierSkillBinding
8 BossPhaseProfile
4 BossPhasePlan
```

九组 Pattern/Sequence 语义：

```text
caster long-cast
seal lock
poison/burn status spread
spirit drain
formation-eye disruption
bronze-general burst
dirty-dream status pressure
spirit-core energy/cast pressure
complex-eye composite pressure
```

Carrier binding 固定覆盖：

```text
6 Enemy carrier:
caster_chanter / seal_locker / poison_cultist / burning_wisp /
spirit_thief / formation_eye_jammer

4 Boss carrier:
burst_huzhen / debuff_jinge / energy_juneng / hybrid_combo
```

`poison_cultist` 与 `burning_wisp` 必须复用同一机制级 Sequence，证明内容按机制复用。每个 Boss 建立两阶段 Plan；阶段只声明数据与进入条件，不执行切阶段。Boss Pattern/Sequence 可在同一 Boss 的两个阶段复用，不得复制相同内容只为凑阶段数。

公开 Skill/Phase 文本只能使用 localization/text key 与 E02 `PlayerHintCategory`。Internal 层可保存 cast kind、时长、MechanicProfile 引用；Developer 层可保存诊断类别和 opaque source IDs。

## 9. E06 Pressure 内容种子

必须建立恰好 10 个机制级 BuildPressureProfile：

```text
dev_enemy_caster_problem
dev_enemy_seal_problem
dev_enemy_poison_burn_problem
dev_enemy_polluted_tile_problem
dev_enemy_spirit_thief_problem
dev_enemy_formation_eye_problem
dev_boss_bronze_formation_general
dev_boss_dirty_dream_mother
dev_boss_spirit_thief_core
dev_boss_black_furnace_complex_eye
```

前九项压力通道必须忠实表达 E03 `pressureKeys`。`dev_boss_spirit_thief_core` 的两个通道权重合计 `10000`。复合阵眼必须显式使用多通道组合且权重合计 `10000`，不得伪装成单一数值墙。

Capability Requirement 必须从 E03 当前 required/optional capability keys 投影：

```text
每个 Pressure 恰好 1 个 Required + All group
存在 optional keys 时，恰好 1 个 Recommended + Any group
Required requirement rows: 27
Recommended requirement rows: 12
Total requirement rows: 39
Requirement groups: 16
```

E10 为每个 requirement 提供 devOnly basis-point tuning seed。数值必须在合法范围内、按同一内容输入确定、写入 DeveloperOnly，且报告明确标记为非正式平衡基线。禁止把阈值显示给玩家或写进玩家文本 key。

必须使用 E02 六种 CounterWindowType 各建立一个可复用窗口实例：

```text
counter_window.interrupt_stagger
counter_window.cleanse_reveal
counter_window.energy_counter_full_array
counter_window.guard_rebound
counter_window.shell_break
counter_window.clear_core_exposure
```

Pressure 与 CounterWindow 必须是多对多。至少证明：

```text
spirit-core pressure -> interrupt + energy-counter
complex-eye pressure -> shell-break + clear-core + energy-counter
同一 cleanse-reveal 被 poison / polluted / dirty-dream pressure 复用
```

PressureSource 必须覆盖本包使用的 MechanicProfile、四个 MapRule、九个 SkillPattern 与八个 BossPhase。CounterWindowSource 必须覆盖九个 SkillPattern 和八个 BossPhase；不得只做孤立窗口清单。

## 10. Player-safe 投影

必须建立独立 `DevEncounterSeedPlayerSafeSnapshot`。它只允许包含：

```text
SchemaId / SchemaVersion
四个 DevEncounterSeedPlayerProjection
E09 EnemySystemPlayerSafeSnapshot
CanonicalSignature
```

玩家可见层允许：

```text
Encounter 名称 key
地图氛围 cue key
失败后的现象提示 key
Skill intent / target / cast cue key
Boss phase label / entry cue key
Pressure label / hint key
CounterWindow open / close cue key
E02 PlayerHintCategory keys
```

玩家可见层禁止：

```text
DevChapterLabel 与 Wave/Slot 编排
CarrierId / MechanicProfileId / MapRuleId
BuildCapabilityKey
Required / Recommended / Any / All
MinimumCapabilityBasisPoints
Pressure channel weight
BossPhase entry condition / HP threshold
完整 BuildReadiness / capability gap / tuning suggestion
shapeRuleKey / previewBuildId / ItemId / SynergyId
Boss 六钥匙 / minimumKeysRequired / key category
DropBias / reward / random seed
developer source ID / legacy mapping
“必须使用某 Build 或某核心道具”的完整答案
```

玩家只得到机制现象和反制时机，不得到完整答案。文本 key 命名本身也不得直接包含 Item、Build、能力阈值或完整解决方案。

## 11. Validator

必须建立 E10 专属纯数据 Validator/Provider，至少拒绝：

```text
null input 或 null dependency
E02/E03 schema mismatch
E03.DomainSnapshot 不一致
seedId / encounterId 重复、空白或格式非法
seed 数量或四个固定映射不符
Encounter/Wave/Slot 数量、顺序、类型或 quantity 不符
Carrier/Mechanic/MapRule/E05/E06/E09 引用未解析
Slot carrier kind 与 mechanic binding 不匹配
E05 计数、复用关系或 Boss plan 不符
E06 pressure/channel/weight/requirement/window/source 关系不符
Required/Recommended 或 Any/All 语义错误
四个 MapRule source 覆盖不符
Furnace Core 被错误要求为所有 Slot mechanic 均 map-bound
任一内容 devOnly=false、isEnabled=true、entersFormalFlow=true
Full / PlayerSafe signature 非法或不敏感
PlayerSafe property/type closure 越界
Runtime 出现 BuildSandbox / Item / Battle / UnityEngine / file IO 依赖
```

Validator issue 必须确定性排序并只读暴露。Provider 发现任一 issue 时拒绝创建 E10 Snapshot。

## 12. Canonical Signature 与不可变性

全部签名格式：

```text
sha256: + 64 lowercase hex
```

Full signature 至少覆盖：

```text
E10 schema / isolation
四个 SeedProfile 三层数据
E09 Full CanonicalSignature
全部 E04/E05/E06 内容与关系
PlayerSafeCanonicalSignature
```

PlayerSafe signature 只覆盖第 10 节白名单。

必须验证：

```text
相同依赖重复创建 -> Full / PlayerSafe 均稳定
输入集合重排 -> 签名稳定
Encounter quantity/order 改变 -> Full 变化 / PlayerSafe 不变
E05/E06 developer/internal 字段改变 -> Full 变化 / PlayerSafe 不变
公开 cue/hint key 改变 -> Full 与 PlayerSafe 都变化
requirement threshold 改变 -> Full 变化 / PlayerSafe 不变
legacy source reference 改变 -> Full 变化 / PlayerSafe 不变
输入与输出集合不可修改
Provider 不修改 E02/E03 输入与 E09 子快照
```

## 13. 允许新增文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataProvider.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataProvider.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs.meta
Docs/V0.4/Reports/DevEncounterSeedDataReport.md
Docs/V0.4/Reports/DevEncounterSeedDataSpec.csv
Docs/V0.4/Reports/DevEncounterSeedInventory.csv
Docs/V0.4/Reports/DevEncounterSeedCompositionRows.csv
Docs/V0.4/Reports/DevEncounterSeedSkillPhaseRows.csv
Docs/V0.4/Reports/DevEncounterSeedPressureWindowRows.csv
Docs/V0.4/Reports/DevEncounterSeedLegacyMapping.csv
Docs/V0.4/Reports/DevEncounterSeedDataLeakCheckReport.md
```

预期新增文件：`21`。
允许修改已有文件：`0`。

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 14. 受保护基线

E01-E09 源码必须 byte-identical：

```text
E01-E08: 38
E09 SystemSnapshot runtime + Verifier: 6
合计: 44/44 unchanged
```

Legacy 对照源码必须 byte-identical：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/DevChapterBalanceRun.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/DevChapterContentPool.cs
合计: 5/5 unchanged
```

必须保持当前 E03 基线：

```text
Carriers: 11 Enemy / 7 Boss / 18 total
Mechanic profiles: 10 Enemy / 6 Boss / 16 total
MapRules: 10
Carrier bindings: 17
MapRule bindings: 30
Intentional exceptions: 2
```

E04-E06 与 E09 旧 Verifier fixture 只属于测试，不得作为 E10 内容输入或 Runtime 默认内容。

## 15. Legacy Mapping

必须固定映射：

| legacyStageId | seedId | MapRule | primary enemy problem | Boss problem | Boss carrier |
|---|---|---|---|---|---|
| `dev_balance_3_10_guard_wall` | `dev_seed_3_10_guard_wall` | `dev_map_copper_bell_night` | `dev_enemy_caster_problem` | `dev_boss_bronze_formation_general` | `dev_boss_burst_huzhen` |
| `dev_balance_3_10_cleanse_corner` | `dev_seed_3_10_cleanse_corner` | `dev_map_bluestone_damp` | `dev_enemy_polluted_tile_problem` | `dev_boss_dirty_dream_mother` | `dev_boss_debuff_jinge` |
| `dev_balance_4_10_furnace_core` | `dev_seed_4_10_furnace_core` | `dev_map_furnace_ash_fall` | `dev_enemy_spirit_thief_problem` | `dev_boss_spirit_thief_core` | `dev_boss_energy_juneng` |
| `dev_balance_4_10_thunder_fire_cross` | `dev_seed_4_10_thunder_fire_cross` | `dev_map_bluestone_crack` | `dev_enemy_formation_eye_problem` | `dev_boss_black_furnace_complex_eye` | `dev_boss_hybrid_combo` |

`dev_enemy_polluted_tile_problem` 是 E03 明确的 `MAP_RULE_ONLY` mechanic，因此它在 cleanse seed 中由 MapRule/E06 pressure source 表达，不得伪造专属 Carrier 或强塞进 Enemy Slot。

Legacy Mapping 只允许出现在 DeveloperOnly 数据与报告。不得复制旧 simulatedWinRate、clearTime、readiness ratio、tuningSuggestion、placed Item、Synergy、Modifier、EffectEvent、shape rule 或 preview Build。

本包不替换、不删除旧 BuildSandbox 数据；只声明未来 Enemy 适配器以 E10 为唯一内容源。真正移除旧消费路径属于后置迁移包，当前冻结。

## 16. 绝对禁止

```text
不修改任何已有工程文件
不修改 AGENTS.md / Docs/LOCKED/*
不修改 Scene / Prefab / Config
不新增 MonoBehaviour / ScriptableObject / Component
不引用 UnityEngine、GameObject、Transform、Addressables 或 Scene API
不创建 Spawn、Timer、状态机、目标选择、伤害、护盾、状态或阶段执行
不读取真实 Battle、HP、战斗事件或当前棋盘
不修改 BattleSandbox 场景、棋盘摆放、拖拽、旋转、输入或手感
不修改 BattleContract / Battle Bridge / UnifiedBattlePage
不修改 Item、Affix、Synergy、Inventory、Equipment 或 ItemSystemSnapshot
不建立 Item -> BuildCapability adapter
不自动装备、推荐具体道具或生成具体 Build
不修改旧 BuildProblem / DevChapter / EnemyBossValidationPool
不把旧 BuildSandbox 类型作为 E10 Runtime 依赖
不创建答题、选择题、题目 UI 或完整答案面板
不暴露 exact requirements、threshold、Boss 六钥匙或 DropBias 给玩家
不发正式奖励或掉落，不写 SaveData
不修改正式 1-10 / 2-10
不接 3-10 / 4-10 正式入口，不推进章节
不启用任何 feature flag 或配置
不覆盖或回退工作区既有未提交改动
不 commit / tag / push
```

## 17. Verifier 必测

Verifier 必须通过当前 E02 default provider、E03 Editor normalizer 与 E10 Provider 建立真实 E10 Snapshot，不得用 synthetic E10 内容替代。

至少覆盖：

```text
4/4 Seed mapping
4 Encounter / 12 Wave / 12 Slot
8 Enemy Slot / 4 Boss Slot
9 Pattern / 9 Sequence / 10 CarrierBinding / 8 Phase / 4 Plan
10 Pressure / 6 CounterWindow
16 RequirementGroup / 39 Requirement rows
全部引用 resolved / isolation compatible
MapRule source matrix 4/4
Furnace Core cross-layer composition accepted
poison/burn sequence reuse
pressure/window many-to-many reuse
E09 Root accepted and Full signature non-empty
PlayerSafe closure / answer leak 0
Full/PlayerSafe sensitivity matrix
输入/输出不可变
Provider 重复构造与报告重复生成确定性
Runtime BuildSandbox / Item / Battle / UnityEngine references 0
正式流程引用 0
```

负例必须至少覆盖：

```text
缺一个 Seed
错误 stage mapping
Wave order gap
Boss slot 使用 Enemy carrier
未绑定 Carrier/MechanicProfile
未知 MapRule / Pattern / Phase / Pressure / Window
错误 requirement role/match mode
pressure channel weight 不等于 10000
player-safe threshold / capability / legacy source leak
devOnly / enabled / formal-flow 隔离错误
```

## 18. 报告要求

八份报告必须由同源 Verifier 每次确定性覆盖生成，禁止 `WriteIfMissing`。连续执行两次必须 `8/8 hash identical`；删除任一报告后再次执行必须可确定性重建。

`DevEncounterSeedInventory.csv` 至少包含：

```text
seedId,devChapterLabel,encounterId,mapRuleId,ordinaryCarrierId,dampingCarrierId,bossCarrierId,bossPhasePlanId,devOnly,isEnabled,entersFormalFlow,legacySourceId
```

`DevEncounterSeedCompositionRows.csv` 至少包含：

```text
seedId,encounterId,waveId,waveOrder,slotId,slotKind,slotRole,carrierId,quantity,mechanicProfileIds,resolved,isolationCompatible
```

`DevEncounterSeedSkillPhaseRows.csv` 至少包含 Pattern、Sequence、CarrierBinding、Phase、Plan 的逻辑实体与引用关系。

`DevEncounterSeedPressureWindowRows.csv` 至少包含 Pressure、channel、requirement group、requirement、source、window 与 pressure-window relation。报告可以展开行，但主报告逻辑实体计数必须去重，Actual 与 PASS 使用同一计数源。

`DevEncounterSeedLegacyMapping.csv` 至少包含：

```text
legacyStageId,seedId,mapRuleId,primaryEnemyProblemId,bossProblemId,bossCarrierId,mappingStatus,runtimeConsumesLegacy
```

其中 `runtimeConsumesLegacy` 必须全为 `false`。

Leak 报告必须逐项列出 Runtime 依赖、player-safe property/type closure、答案 token、正式流程引用、Scene/Battle/Board/Item 修改扫描结果，不得只写总数。

## 19. 自动验证与回报

必须尝试：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
八报告连续两次确定性生成
删除一份临时报告后的确定性重建证明
E01-E09 与 Legacy protected hash
全局 git diff --check
package-scope whitespace check
GUID 冲突检查
精确变更文件清单
```

当前全局 `git diff --check` 可能仍因开工前无关 BattleSandbox Scene 四处尾随空白失败。必须记录 `PREEXISTING_UNRELATED_DIFF` 并保证 E10 package-scope PASS；不得触碰该场景。

若同工程被用户 Unity Editor 占用，不得关闭 Editor 或结束进程；如实记录 Unity 为 `BLOCKED`，继续完成同源离线 Verifier，由 Guard 决定收口。

本包无需场景手测。

完成回报：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-DevEncounterSeedData01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_DEVENCOUNTERSEEDDATA01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 21
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
Seeds / Encounters / Waves / Slots: 4 / 4 / 12 / 12
Enemy / Boss Slots: 8 / 4
Pattern / Sequence / CarrierBinding / Phase / Plan: 9 / 9 / 10 / 8 / 4
Pressure / CounterWindow: 10 / 6
RequirementGroup / Requirement: 16 / 39
Legacy mapping: 4/4
Runtime consumes legacy: 必须为 0
Furnace Core cross-layer composition: PASS / FAIL
Many-to-many reuse: PASS / FAIL
Unresolved references: 必须为 0
Isolation violations: 必须为 0
Player answer leak: 必须为 0
Formal flow references: 必须为 0
Eight-report determinism: 8/8 PASS / FAIL
Missing-report regeneration: PASS / FAIL
E01-E09 protected hash: 44/44 unchanged
Legacy source hash: 5/5 unchanged
Verifier: 通过数 / 总数
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
同源离线 Verifier: 通过数 / 总数
Leak: 必须为 0
GUID 冲突: 必须为 0
新增文件尾随空白: 必须为 0
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
HEAD unchanged
未 commit / tag / push
```

## 20. 通过后

E10 通过只代表 devOnly 数据内容与未来只读消费面成立。以下后置包全部继续冻结：

```text
MapRuleRuntime01
EnemyBossSkillPatternRuntime01
BossPhaseRuntime01
EnemyFeedbackProjectionIntegration01
BattleSandbox Enemy 接线
Item -> BuildCapabilitySnapshot Adapter
BattleContract / Battle Bridge 接线
正式章节 Promote
正式 Reward / Drop / SaveData 接线
```

不得自动启动任何后置包。
