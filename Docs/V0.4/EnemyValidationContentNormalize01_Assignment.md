# V0.4-EnemyValidationContentNormalize01 Assignment

Guard assignment：

```text
ENEMY_GUARD_ASSIGNMENT_ENEMYVALIDATIONCONTENTNORMALIZE01
```

生成日期：2026-07-14  
所属：Enemy / Boss / Encounter System Guard  
包序号：E03

## 1. 包定位

本包把现有 devOnly Enemy/Boss 验证载体、机制验证配置和地图规则整理成一份确定性的只读归一化快照与关系矩阵。

本包解决：

```text
谁是敌人 / Boss 原型
每个机制配置表达什么机制与压力
哪些原型引用哪些机制配置
哪些地图规则引用哪些机制配置
哪些内容是有意不绑定，而不是漏配
```

本包不修改旧池，不复制第三套完整敌人数据，不执行任何机制。

## 2. 正确关系

```text
EnemyArchetype / BossArchetype
  = 玩家在打谁

MechanicValidationProfile
  = 这个敌人通过什么方式制造 Build 压力

MapRule
  = 环境如何组合或放大这些压力

Binding
  = 原型、机制配置、地图规则之间的明确引用
```

原型与机制是多对多结构，不强制一对一。

“机制验证”仍是内部形容词，不是真实题库、问卷、选择题或答案面板。

## 3. 启动必读

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
Docs/V0.4/EnemyValidationContentNormalize01_Assignment.md
```

并只读检查：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs
Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Contracts/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
```

## 4. 数据来源与依赖方向

允许的依赖方向：

```text
Editor-only legacy reader
  只读 EnemyBossValidationPool.CreateDefault()
  只读 BuildProblemSeedDataset.CreateDefault()
        ↓
Enemy Validation Normalizer
        ↓
E01 Enemy Domain Contract
+ E02 Enemy Mechanic Vocabulary
        ↓
不可变 Normalized Snapshot / Reports
```

长期 Enemy Runtime 源码不得引用 `TalismanBag.BuildSandbox`。

只有 `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/**` 下本包新增的 Editor-only reader 可以只读引用旧 BuildSandbox 类型。

不得写回旧对象、Asset、Config、Scene 或 Prefab。

## 5. 必须建立的归一化结构

在独立新目录新增纯 C# 结构：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
```

至少包含等价语义：

```text
NormalizedEnemyCarrierSnapshot
NormalizedBossCarrierSnapshot
NormalizedMechanicProfileSnapshot
NormalizedMapRuleSnapshot
CarrierMechanicBindingSnapshot
MapRuleMechanicBindingSnapshot
NormalizationExceptionSnapshot
EnemyValidationContentSnapshot
IEnemyValidationContentNormalizer
IEnemyValidationContentLookup
```

类名可按既有风格微调，但必须保持：

```text
不可变
严格 Ordinal ID
防御性复制
确定性 Canonical Signature
player-safe 与 developer-only 数据分层
```

## 6. 当前内容数量

归一化输入必须精确保持：

```text
Enemy carriers: 11
Boss carriers: 7
Carrier total: 18
Enemy mechanic profiles: 10
Boss mechanic profiles: 6
Mechanic profile total: 16
Map rules: 10
```

不得增加、删除、启用或替换旧条目。

## 7. 当前 Carrier ↔ Mechanic 绑定矩阵

普通敌人：

```text
dev_enemy_shield_guard
  → dev_enemy_shield_problem

dev_enemy_swarm_pack
  → dev_enemy_swarm_problem

dev_enemy_poison_cultist
  → dev_enemy_poison_burn_problem

dev_enemy_burning_wisp
  → dev_enemy_poison_burn_problem

dev_enemy_spirit_thief
  → dev_enemy_spirit_thief_problem

dev_enemy_seal_locker
  → dev_enemy_seal_problem

dev_enemy_burst_assassin
  → dev_enemy_burst_problem

dev_enemy_caster_chanter
  → dev_enemy_caster_problem

dev_enemy_thick_blood
  → dev_enemy_thick_blood_problem

dev_enemy_formation_eye_jammer
  → dev_enemy_formation_eye_problem
```

有意例外：

```text
dev_enemy_basic
  → BASELINE_ONLY
  原因：基础对照载体，不要求绑定 10 个机制验证配置之一。

dev_enemy_polluted_tile_problem
  → MAP_RULE_ONLY
  原因：污染格可由地图规则承载，当前没有独立敌人载体。
```

Boss：

```text
dev_boss_shield_jinglei
  → dev_boss_black_furnace_shell

dev_boss_swarm_lihuo
  → dev_boss_paper_faces

dev_boss_burst_huzhen
  → dev_boss_bronze_formation_general

dev_boss_debuff_jinge
  → dev_boss_dirty_dream_mother

dev_boss_caster_zhenhun
  → dev_boss_spirit_thief_core

dev_boss_energy_juneng
  → dev_boss_spirit_thief_core

dev_boss_hybrid_combo
  → dev_boss_black_furnace_complex_eye
```

当前预期：

```text
Carrier ↔ Mechanic bindings: 17
Intentional carrier exceptions: 1
Intentional mechanic exceptions: 1
```

绑定矩阵必须是独立关系数据，不得把机制字段复制进 Carrier。

## 8. MapRule ↔ Mechanic 关系

必须从 10 个 `MapRuleSeed` 的现有 `enemyProblemIds` 与 `bossProblemIds` 只读生成关系。

当前预期：

```text
10 MapRules
每条 2 个 Enemy mechanic profile 引用
每条 1 个 Boss mechanic profile 引用
MapRule ↔ Mechanic bindings: 30
```

不得手写第二份地图规则关系表来覆盖源数据；报告必须能追踪回原 `MapRuleSeed`。

## 9. Vocabulary 投影

所有原型与机制配置的旧标签必须通过 E02 `EnemyMechanicVocabulary.v1` 映射。

归一化结果可以持有：

```text
Mechanic stable keys
BuildCapability stable keys
PressureChannel stable keys
CounterWindowType references
PlayerHintCategory references
DeveloperDiagnosticCategory references
```

禁止直接把以下旧字段作为新的稳定语义继续传播：

```text
enemyType
bossMechanic
validationTags
pressureType
hardSolutionTags
softSolutionTags
```

这些字段只作为 migration source 留在开发者追踪信息中。

## 10. 玩家层与开发者层

Player-safe projection 只允许：

```text
原型 ID / 显示名
公开机制稳定键
公开压力通道
PlayerHintCategory 引用
CounterWindowType 引用
```

Developer-only projection 可以记录：

```text
legacy source kind / source ID
完整 BuildCapability 要求
binding reason
normalization exception
旧字段到新键的映射追踪
Boss key / weakness / DropBias 的 source reference ID
```

本包不得把以下内容放进 player-safe projection：

```text
hardSolutionTags
softSolutionTags
requiredProblemAttributes
keyRequirements
minimumKeysRequired
requiredSynergy
requiredAffix
requiredStats
DropBias ID / 权重
精确 WeaknessWindow 时间与阈值
recommendedAction 完整答案
```

## 11. 当前不做的内容

```text
不迁移战斗数值
不设计新敌人或新 Boss
不重命名现有 ID
不修改 11/7、10/6 或 10 条地图规则源数据
不创建正式配置资产
不实现 Encounter 波次
不实现 MapRule Runtime
不实现技能、Boss Phase 或 CounterWindow Runtime
不实现 BuildReadiness 计算
不接 Item Snapshot
不接 BattleContract
```

## 12. 允许新增范围

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Normalization/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizer.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyValidationContentNormalizeVerifier.cs
对应新增文件与目录的 .meta
Docs/V0.4/Reports/EnemyValidationContentNormalizeReport.md
Docs/V0.4/Reports/EnemyValidationContentNormalizeSpec.csv
Docs/V0.4/Reports/EnemyValidationCarrierInventory.csv
Docs/V0.4/Reports/EnemyValidationMechanicInventory.csv
Docs/V0.4/Reports/EnemyCarrierMechanicBinding.csv
Docs/V0.4/Reports/EnemyMapRuleMechanicBinding.csv
Docs/V0.4/Reports/EnemyValidationNormalizationExceptions.csv
Docs/V0.4/Reports/EnemyValidationContentNormalizeLeakCheckReport.md
```

开发窗口不得修改 Assignment、Guard CurrentRules 或 Package Queue。

## 13. 受保护文件

以下必须保持 byte-identical：

```text
E01 六个受保护源码 / Verifier
E02 三个 Vocabulary 源码 / Verifier
EnemyBossValidationPool.cs
BuildProblemRuleConfigs.cs
BuildProblemSeedData.cs
```

合计：

```text
E01 + E02 protected files: 10/10 unchanged
Legacy source files: 3/3 unchanged
```

开发前记录 hash，开发后复核。

## 14. 绝对禁止

```text
不改任何已有文件
不改 Scene / Prefab / Config
不新增 MonoBehaviour 或 ScriptableObject
不改 BattleSandbox、BattleContract 或 Battle Bridge
不改棋盘摆放、拖拽、旋转、托盘和输入
不改 Item 系统
不改正式 EnemyDefinition / EnemySkillDefinition / 敌人资产
不改 1-10 / 2-10
不改 RunFlow / Reward / SaveData / Boss 正式流程
不接正式章节入口
不生成正式奖励或掉落
不覆盖或回退工作区既有未提交改动
不 commit / tag / push
```

## 15. Verifier 要求

至少验证：

```text
Carrier 11 + 7 = 18
Mechanic profile 10 + 6 = 16
MapRule = 10
Carrier ↔ Mechanic bindings = 17
MapRule ↔ Mechanic bindings = 30
Intentional carrier exceptions = 1
Intentional mechanic exceptions = 1
所有 Carrier、Mechanic、MapRule ID 唯一且非空
所有 binding source / target 可解析
没有未解释的 orphan
同一 Mechanic profile 可被多个 Carrier 引用
Schema 支持同一 Carrier 引用多个 Mechanic profile
所有旧标签通过 E02 映射或明确 OUT_OF_SCOPE
运行时 Normalization 源码不引用 BuildSandbox
只有 Editor-only reader 可以读取 BuildSandbox
输入与输出不可变
输入顺序变化不影响 Canonical Signature
player-safe projection 无完整答案泄漏
E01 + E02 protected hash 10/10 不变
Legacy source hash 3/3 不变
Scene / Prefab / Config / Battle / Board / Item 修改均为 0
```

## 16. 报告要求

Carrier Inventory 至少包含：

```text
carrierKind
carrierId
displayName
mechanicProfileIds
mechanicKeys
pressureKeys
bindingStatus
devOnly
isEnabled
entersFormalFlow
```

Mechanic Inventory 至少包含：

```text
profileKind
mechanicProfileId
displayName
mechanicKeys
pressureKeys
requiredCapabilityKeys
optionalCapabilityKeys
carrierIds
mapRuleIds
developerOnly
```

Binding CSV 至少包含：

```text
sourceKind
sourceId
targetKind
targetId
bindingType
bindingReason
status
```

Exception CSV 至少包含：

```text
exceptionKind
sourceId
status
reason
developerOnly
```

## 17. 自动验证

必须尝试：

```text
Unity batch compile
Unity batch Verifier
同源离线 Verifier
git diff --check
新增文件尾随空白检查
GUID 冲突检查
精确变更文件清单
```

如同一工程被用户 Unity Editor 占用：

```text
不得关闭用户 Editor
不得结束 Unity 进程
不得声称 Unity Verifier PASS
记录 BLOCKED 原因
继续完成同源离线 Verifier
由 Guard 决定是否以记录阻塞方式收口
```

本包无需场景手测。

## 18. 完成回报格式

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-EnemyValidationContentNormalize01

Result:
DEV_COMPLETE / QA_RESULT

新增文件：
修改已有文件：必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改：必须全部为 0
E01 + E02 protected hash：10/10 unchanged
Legacy source hash：3/3 unchanged
Carriers：Enemy / Boss / Total
Mechanic profiles：Enemy / Boss / Total
MapRules：数量
Carrier bindings：数量
MapRule bindings：数量
Intentional exceptions：数量
Unresolved / unexplained orphan：必须为 0
Verifier：通过数 / 总数
Unity batch compile：PASS / FAIL / BLOCKED
Unity batch Verifier：PASS / FAIL / BLOCKED
Leak：必须为 0
git diff --check：PASS / FAIL
未 commit / tag / push
```

## 19. 通过后

通过后下一候选包：

```text
V0.4-EncounterCompositionSchema01
```

E04 不自动启动。
