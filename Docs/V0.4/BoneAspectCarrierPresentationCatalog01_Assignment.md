# V0.4-BoneAspectCarrierPresentationCatalog01 Guard Assignment

状态：`ENEMY_GUARD_ASSIGNMENT_BONEASPECTCARRIERPRESENTATIONCATALOG01 / READY_FOR_DEVELOPMENT`
上游：`V0.4-BoneAspectContentDesignLock01-GuardFix01 / GUARD_ACCEPTED`
风险：`YELLOW / DEVONLY_CARRIER_AND_PRESENTATION_CATALOG`
维护方：Enemy / Boss / Encounter System Guard
日期：2026-07-23

## 0. 包定位

本包是《骨相》Enemy 数据线 P1。

本包只把 P0 已锁定的：

```text
12 个普通怪
4 个 Boss
稳定 contentId
中文显示名与本地化键
presentationKey
visualFamilyId
轮廓与核心识别键
美术交付槽
现有 E02 Vocabulary 的候选复用引用
```

建立为独立、不可变、可验证的 devOnly Carrier Presentation Catalog。

本包不实现敌人 Runtime，不生成 Encounter，不修改 E03/E09/E10，不接 Battle、Scene、Prefab、Item 或正式流程。

P0 中的机制承诺仍只是策划承诺。本包中的 E02 引用只能标记为 `CandidateReuseOnly`，不得声称已存在 Skill、MechanicProfile、CounterWindow、BossPhase、Runtime Producer 或 Battle Executor。

## 1. 启动必读

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
Docs/V0.4/SHARED_MODULE_LAYERING_AND_PREFAB_PRESENTATION_GUARD.md
Docs/V0.4/BoneAspectContentDesignLock01_Assignment.md
Docs/V0.4/BoneAspectCarrierPresentationCatalog01_Assignment.md
```

权威 P0 输入：

```text
Docs/V0.4/Reports/BoneAspectContentDesignLockReport.md
Docs/V0.4/Reports/BoneAspectContentCatalog.csv
Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv
Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv
Docs/V0.4/Reports/BoneAspectVisualLanguageSpec.csv
Docs/V0.4/Reports/BoneAspectExternalReferenceBoundary.csv
Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv
Docs/V0.4/Reports/BoneAspectContentDesignLockLeakCheckReport.md
```

P0 Accepted Canonical：

```text
sha256:3350c02ee92165f46ba87e92056d58b79bd6e2824f5b194cbe5a827eb97d7926
```

P0 Assignment 实际磁盘 SHA-256：

```text
4351ce9b5c070ee27e11eabc0f8debdd5e9d38c6bf4aa225c21d09175a433b23
```

历史委派消息中的 63 位 marker：

```text
4351ce9b5c070ee27e11eabc0f8debd5e9d38c6bf4aa225c21d09175a433b23
```

只作为传输元数据错误保留，不得用它替换实际 64 位 SHA-256，也不得静默改写历史证据。

E01/E02 只读依赖：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/Domain/**
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
Docs/V0.4/Reports/EnemyDomainDataContract*.*
Docs/V0.4/Reports/EnemyMechanicVocabulary*.*
```

开发任务不得在 Runtime 读取 P0 CSV 或 Markdown。P0 报告是开发输入和 Verifier 对照，不是运行时文件源。

## 2. 系统所有权与依赖裁定

固定分层：

```text
BoneAspect Carrier Presentation Catalog
  拥有《骨相》16 个 Carrier 的身份、名称、描述键、presentationKey、
  visualFamilyId、识别键、美术交付槽和候选机制引用。

E02 Enemy Mechanic Vocabulary
  继续拥有 Mechanic / PressureChannel / CounterWindowType 稳定词汇。
  本包只读验证引用，不新增或修改 Vocabulary。

Enemy Mechanic / Skill / BossPhase
  未来拥有可执行机制、技能、反制窗口和阶段数据。
  本包不创建这些数据。

Shared Encounter Presentation
  未来拥有 EncounterPresentationProfile 与 Encounter sidecar binding。
  本包不拥有场景背景、锚点、灯光、UI 安全区或 Scene。

EnemyRuntimeState / Battle Resolver
  未来拥有并执行 HP、护壳、施法、状态、阶段、目标、伤害与跨域请求。
  本包不创建 Runtime 状态或执行逻辑。
```

不得把 Carrier、Mechanic、Skill、Encounter、Scene 合并为同一个对象。

## 3. Schema 与隔离状态

必须建立：

```text
SchemaId = BoneAspectCarrierPresentationCatalog.v1
SchemaVersion = 1
```

全部 16 个 Carrier 以及 Catalog 根必须固定：

```text
devOnly = true
isEnabled = false
entersFormalFlow = false
runtimeImplemented = false
```

不得提供可切换这些值的配置、Inspector、ScriptableObject 或 feature flag。

## 4. 16 个 Carrier 精确身份

必须与 P0 `BoneAspectContentCatalog.csv` 逐字段 exact equality，且恰好为：

| contentId | kind | chapterId | displayName | presentationKey | visualFamilyId |
| --- | --- | --- | --- | --- | --- |
| `bone_aspect_enemy_c1_01_shattered_host` | Enemy | `bone_aspect_chapter_1` | 碎骨附身者 | `enemy.bone_aspect.c1.shattered_host` | `bone_aspect_visual_c1_bone_porcelain` |
| `bone_aspect_enemy_c1_02_porcelain_hound` | Enemy | `bone_aspect_chapter_1` | 骨瓷犬 | `enemy.bone_aspect.c1.porcelain_hound` | `bone_aspect_visual_c1_bone_porcelain` |
| `bone_aspect_enemy_c1_03_bone_swap_remnant` | Enemy | `bone_aspect_chapter_1` | 换骨残相 | `enemy.bone_aspect.c1.bone_swap_remnant` | `bone_aspect_visual_c1_bone_porcelain` |
| `bone_aspect_enemy_c2_01_false_identity_bone` | Enemy | `bone_aspect_chapter_2` | 假相浮骨 | `enemy.bone_aspect.c2.false_identity_bone` | `bone_aspect_visual_c2_contract_identity` |
| `bone_aspect_enemy_c2_02_contract_clerk` | Enemy | `bone_aspect_chapter_2` | 契纸吏 | `enemy.bone_aspect.c2.contract_clerk` | `bone_aspect_visual_c2_contract_identity` |
| `bone_aspect_enemy_c2_03_bone_bidder` | Enemy | `bone_aspect_chapter_2` | 竞骨客 | `enemy.bone_aspect.c2.bone_bidder` | `bone_aspect_visual_c2_contract_identity` |
| `bone_aspect_enemy_c3_01_array_stone_puppet` | Enemy | `bone_aspect_chapter_3` | 残阵石傀 | `enemy.bone_aspect.c3.array_stone_puppet` | `bone_aspect_visual_c3_array_stone_memory` |
| `bone_aspect_enemy_c3_02_old_edict_shadow` | Enemy | `bone_aspect_chapter_3` | 旧令道影 | `enemy.bone_aspect.c3.old_edict_shadow` | `bone_aspect_visual_c3_array_stone_memory` |
| `bone_aspect_enemy_c3_03_bone_dust_effigy` | Enemy | `bone_aspect_chapter_3` | 骨尘小像 | `enemy.bone_aspect.c3.bone_dust_effigy` | `bone_aspect_visual_c3_array_stone_memory` |
| `bone_aspect_enemy_c4_01_faceless_bone_figure` | Enemy | `bone_aspect_chapter_4` | 无面骨俑 | `enemy.bone_aspect.c4.faceless_bone_figure` | `bone_aspect_visual_c4_counterfeit_composite` |
| `bone_aspect_enemy_c4_02_contract_attendant` | Enemy | `bone_aspect_chapter_4` | 骨契商侍 | `enemy.bone_aspect.c4.contract_attendant` | `bone_aspect_visual_c4_counterfeit_composite` |
| `bone_aspect_enemy_c4_03_false_celestial_form` | Enemy | `bone_aspect_chapter_4` | 天骨赝相 | `enemy.bone_aspect.c4.false_celestial_form` | `bone_aspect_visual_c4_counterfeit_composite` |
| `bone_aspect_boss_c1_bone_guard` | Boss | `bone_aspect_chapter_1` | 守骨奴 | `boss.bone_aspect.c1.bone_guard` | `bone_aspect_visual_c1_bone_porcelain` |
| `bone_aspect_boss_c2_identity_bidder` | Boss | `bone_aspect_chapter_2` | 竞骨人 | `boss.bone_aspect.c2.identity_bidder` | `bone_aspect_visual_c2_contract_identity` |
| `bone_aspect_boss_c3_array_eye_guardian` | Boss | `bone_aspect_chapter_3` | 阵眼守护者 | `boss.bone_aspect.c3.array_eye_guardian` | `bone_aspect_visual_c3_array_stone_memory` |
| `bone_aspect_boss_c4_myriad_bone_beast` | Boss | `bone_aspect_chapter_4` | 万相骨兽 | `boss.bone_aspect.c4.myriad_bone_beast` | `bone_aspect_visual_c4_counterfeit_composite` |

每行必须同时复制 P0 已验收的：

```text
localizationKey
primarySilhouetteKey
coreRecognitionKey
mechanicCommitmentSummary
```

名称描述字段固定：

```text
displayName = P0 中文显示名
nameLocalizationKey = P0 localizationKey
descriptionLocalizationKey = presentationKey + ".description"
```

`descriptionLocalizationKey` 只是稳定展示键。本包不编写正式玩家文案，不修改本地化系统。

每个 Carrier 必须具有唯一：

```text
mechanicCommitmentId = contentId + ".mechanic_commitment"
```

它只标识 P0 策划承诺，不是 MechanicProfileId、SkillPatternId 或 Runtime key。

## 5. 美术交付槽

美术槽只描述未来应提供哪些展示层，不引用图片路径，不检查图片是否已存在，不创建 Sprite、Texture、Material、Animator、Prefab 或 Scene。

### 5.1 普通怪基础槽

12 个普通怪各恰好建立 6 个基础槽，对应 P0：

```text
view.normal_enemy.front_and_three_quarter
view.normal_enemy.silhouette
view.normal_enemy.primary_material
view.normal_enemy.mechanic_marker
view.normal_enemy.state_set
view.normal_enemy.small_screen
```

基础槽状态：

```text
DeliveryStatus = RequiredByLockedDesign
BlockedByDecisionId = ""
RuntimeBindingKey = ""
AssetPath = 不存在该字段
```

普通怪基础槽合计：`72`。

### 5.2 Boss 基础槽

4 个 Boss 各恰好建立 4 个基础槽，对应 P0：

```text
view.boss.front
view.boss.battle_three_quarter
view.boss.phase_states
view.boss.poster_reference_only
```

基础槽状态同上。

Boss 基础槽合计：`16`。

`view.boss.poster_reference_only` 只是构图参考槽，不代表本包生产海报、KV 或营销资产。

### 5.3 用户决策阻塞槽

必须额外建立 5 个 `BlockedByUserDecision` 槽：

| contentId | blockedByDecisionId | slotKey | 当前允许内容 |
| --- | --- | --- | --- |
| `bone_aspect_boss_c1_bone_guard` | `BA-D1` | `view.boss.deferred.protected_target_execution_state` | 只保留守护姿态与叙事关系，不画执行规则 |
| `bone_aspect_boss_c2_identity_bidder` | `BA-D2` | `view.boss.deferred.auxiliary_mechanic_variant` | 不选择虚假分身或竞价护盾 |
| `bone_aspect_enemy_c1_03_bone_swap_remnant` | `BA-D3` | `view.normal_enemy.deferred.copy_execution_state` | 可画复制母题，不画复制执行答案 |
| `bone_aspect_enemy_c3_03_bone_dust_effigy` | `BA-D3` | `view.normal_enemy.deferred.copy_execution_state` | 可画模仿母题，不画复制执行答案 |
| `bone_aspect_boss_c4_myriad_bone_beast` | `BA-D4` | `view.boss.deferred.seal_target_and_form_variants` | 只保留封存/解封母题，不决定目标与两形态 |

阻塞槽必须：

```text
DeliveryStatus = BlockedByUserDecision
BlockedByDecisionId = 对应 BA-D1..D4
runtimeImplemented = false
```

总 ArtDeliverySlot 行数固定：

```text
72 Enemy base
+ 16 Boss base
+ 5 decision-blocked
= 93
```

## 6. E02 候选机制引用

必须从 P0 `existingReuseCandidateKeys` 精确投影 34 行 typed reference。

每行字段至少包含：

```text
carrierId
vocabularyCategory
stableKey
bindingStatus
runtimeImplemented
sourceCommitmentId
```

固定：

```text
bindingStatus = CandidateReuseOnly
runtimeImplemented = false
```

精确引用：

| carrierId | E02 candidate keys |
| --- | --- |
| `bone_aspect_enemy_c1_01_shattered_host` | `mechanic.basic_pressure`; `mechanic.polluted_tile` |
| `bone_aspect_enemy_c1_02_porcelain_hound` | `mechanic.layered_shield`; `counter_window.shell_break` |
| `bone_aspect_enemy_c1_03_bone_swap_remnant` | `mechanic.basic_pressure`; `mechanic.swarm_summon` |
| `bone_aspect_enemy_c2_01_false_identity_bone` | `mechanic.basic_pressure`; `counter_window.cleanse_reveal` |
| `bone_aspect_enemy_c2_02_contract_clerk` | `mechanic.long_cast`; `mechanic.talisman_seal`; `counter_window.interrupt_stagger` |
| `bone_aspect_enemy_c2_03_bone_bidder` | `pressure.multi_target` |
| `bone_aspect_enemy_c3_01_array_stone_puppet` | `mechanic.layered_shield`; `counter_window.shell_break` |
| `bone_aspect_enemy_c3_02_old_edict_shadow` | `mechanic.long_cast`; `mechanic.talisman_seal`; `counter_window.interrupt_stagger` |
| `bone_aspect_enemy_c3_03_bone_dust_effigy` | `mechanic.basic_pressure` |
| `bone_aspect_enemy_c4_01_faceless_bone_figure` | `mechanic.swarm_summon`; `mechanic.polluted_tile` |
| `bone_aspect_enemy_c4_02_contract_attendant` | `mechanic.long_cast`; `mechanic.talisman_seal`; `counter_window.interrupt_stagger` |
| `bone_aspect_enemy_c4_03_false_celestial_form` | `mechanic.basic_pressure`; `mechanic.burst_spike` |
| `bone_aspect_boss_c1_bone_guard` | `mechanic.layered_shield`; `counter_window.shell_break` |
| `bone_aspect_boss_c2_identity_bidder` | `mechanic.long_cast`; `counter_window.interrupt_stagger` |
| `bone_aspect_boss_c3_array_eye_guardian` | `mechanic.layered_shield`; `mechanic.formation_eye_disruption`; `counter_window.shell_break` |
| `bone_aspect_boss_c4_myriad_bone_beast` | `mechanic.talisman_seal`; `mechanic.burst_spike` |

计数固定：

```text
Mechanic references = 24
PressureChannel references = 1
CounterWindowType references = 9
Total references = 34
Distinct E02 keys = 12
Unresolved E02 keys = 0
```

类别必须由 E02 typed key 校验，不得只按字符串前缀猜测后继续运行。

必须证明多对多复用，但不得生成 MechanicProfile binding：

```text
一个 Carrier 可引用多个 E02 key
一个 E02 key 可被多个 Carrier 复用
MechanicProfileId rows = 0
SkillPatternId rows = 0
BossPhaseId rows = 0
Runtime consumer rows = 0
```

P0 `newMechanicGapCandidates`、跨系统 Owner 与 BA-D1..D4 只作为 DeveloperOnly 的：

```text
gapSurveyRequired
decisionIds
```

保存，不得在本包创建新 Vocabulary key。全部 16 个 Carrier 的 `gapSurveyRequired` 必须为 `true`。

## 7. Runtime 数据模型

必须建立纯 C#、不可变结构，至少包含：

```text
BoneAspectCarrierPresentationCatalogSchema
BoneAspectCarrierKind
BoneAspectArtDeliveryStatus
BoneAspectMechanicReferenceBindingStatus
BoneAspectCarrierPresentationSnapshot
BoneAspectArtDeliverySlotSnapshot
BoneAspectMechanicReferenceSnapshot
BoneAspectCarrierPresentationCatalogInput
BoneAspectCarrierPresentationCatalogSnapshot
BoneAspectCarrierPresentationPlayerSafeSnapshot
BoneAspectCarrierPresentationCatalogValidator
BoneAspectCarrierPresentationCatalogProvider
```

所有输入集合必须防御性复制；所有输出集合只读；Provider 不得修改 E02 Snapshot 或调用方输入。

允许依赖：

```text
System
System.Collections.Generic
System.Collections.ObjectModel
System.Globalization
System.Linq
System.Security.Cryptography
System.Text
TalismanBag.EnemySystem.Vocabulary
```

Runtime 禁止依赖：

```text
UnityEngine
UnityEditor
BuildSandbox
Item / ItemSystem
Battle / BattleBridge / BattleContract
SceneManagement
Addressables / Resources
System.IO
JSON / CSV / Markdown file reader
```

## 8. Player-safe 投影

必须建立独立 Player-safe Snapshot。

允许：

```text
SchemaId / SchemaVersion
contentId
carrierKind
displayName
nameLocalizationKey
descriptionLocalizationKey
presentationKey
visualFamilyId
primarySilhouetteKey
coreRecognitionKey
RequiredByLockedDesign 的基础 art slot key
devOnly / isEnabled / entersFormalFlow
PlayerSafeCanonicalSignature
```

禁止：

```text
mechanicCommitmentSummary
mechanicCommitmentId
E02 candidate reference
gapSurveyRequired
BA-D1..D4 decisionId
Developer owner / diagnostic / survey status
BlockedByUserDecision art slot
完整机制答案
BuildCapability / requirement / threshold / Readiness
具体 Item、Affix、Synergy、DropBias、奖励概率
```

Player-safe 投影只是安全的数据视图，不代表已经接入玩家 UI。

## 9. Validator

Validator 至少拒绝：

```text
null input 或 null E02 dependency
SchemaId / SchemaVersion mismatch
Catalog isolation flag 错误
Carrier 数量不是 16，或 Enemy/Boss 不是 12/4
contentId、presentationKey、localizationKey、mechanicCommitmentId 重复或空白
P0 身份、chapter、visualFamily、识别键不一致
descriptionLocalizationKey 不符合 presentationKey + ".description"
任一 Carrier devOnly=false、isEnabled=true、entersFormalFlow=true、runtimeImplemented=true
基础 art slot 缺失、重复、错误归类或计数不为 88
决策阻塞槽不是 5 行，或 decisionId/carrierId/slotKey 映射错误
总 art slot 数不是 93
E02 reference 不是 34 行，或类型计数不为 24/1/9
E02 key 无法由当前 Vocabulary typed lookup 解析
bindingStatus 不是 CandidateReuseOnly
任一引用被标为 runtimeImplemented
出现 MechanicProfileId、SkillPatternId、BossPhaseId 或 Runtime consumer
16 个 Carrier 任一 gapSurveyRequired=false
BA-D1..D4 被选择、默认化或转成实现
Player-safe property/type closure 越界
Full / Player-safe Canonical 非确定或不敏感
```

Validation issue 必须稳定排序并只读暴露。Provider 发现任一 issue 时拒绝创建 Snapshot。

## 10. Canonical Signature

格式固定：

```text
sha256: + 64 lowercase hex
```

使用：

```text
StringComparison.Ordinal
Ordinal 排序
InvariantCulture
稳定字段分隔与集合分隔
```

Full Canonical 至少覆盖：

```text
Schema / isolation
16 Carrier 全字段
93 ArtDeliverySlot 全字段
34 E02 candidate reference 全字段
16 mechanic commitment metadata
BA-D1..D4 未决引用状态
E02 dependency CanonicalSignature
PlayerSafeCanonicalSignature
```

Player-safe Canonical 只覆盖第 8 节白名单。

必须验证：

```text
相同输入重复创建稳定
输入集合反序稳定
Culture 切换稳定
任一 Full-only 字段变化 -> Full 变化 / PlayerSafe 稳定
任一 Player-safe 字段变化 -> Full 与 PlayerSafe 均变化
BA-D1..D4 仍未决但引用顺序变化 -> Canonical 稳定
输入和输出集合不可修改
```

## 11. 精确文件白名单

只允许新增以下 21 个文件：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs.meta
Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogReport.md
Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogSpec.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationInventory.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationArtSlotRows.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationMechanicReferenceRows.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationPlayerSafeFieldMatrix.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogLeakCheckReport.md
```

新增文件：必须为 `21`。

允许修改已有文件：`0`。

不得修改本 Assignment、P0 报告、Queue、Guard rules 或其他包报告。

## 12. 受保护基线与并行隔离

任务开始时必须按磁盘当前状态记录 SHA-256，不得使用 Git HEAD 或过期全仓 hash 代替。

至少保护：

```text
P0 8 个已验收报告
EnemySystem/** 中除本包新增白名单外的全部已有文件
Editor/EnemySystem/** 中除本包新增 Verifier 外的全部已有文件
Assets/_Game/Scripts/TalismanBag/Items/**
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/CrossSystem/**
所有 Scene / Prefab / Config
ProjectSettings / Packages / BuildSettings
正式 RunFlow / SaveData / Reward / Chapter
```

本包与并行 Item P3 共用 checkout。并行约束：

```text
不得修改、格式化、还原、stage 或认领 Item/BuildSandbox/CrossSystem 文件
不得全仓格式化、清理、reset、rollback
不得关闭用户 Unity Editor 或终止其他任务进程
不得同时启动第二个 Unity batch
检测到 UnityLockfile、活动 Editor 或其他 batch 时，Unity 验证记为 BLOCKED 并继续离线验证
Unity 导入产生非白名单 .meta 时必须停止归因，不得认领
```

若 protected 文件在任务期间发生并行漂移：

```text
记录 task-start hash、current hash、精确路径
不得重定基线
不得回滚或改写
同步 Primary Enemy Guard 请求 Owner Guard 归因
包自身必须保持 scope leak = 0
```

## 13. Verifier 与报告

Editor Verifier 必须以本包 Catalog + 当前 E02 default provider 建立真实 Snapshot，不得使用另一套 synthetic 正例替代正式 16 行。

必须覆盖：

```text
16 Carrier / 12 Enemy / 4 Boss
4 chapter / 4 visual family
16 unique contentId / presentationKey / mechanicCommitmentId
93 art slots = 72 Enemy base + 16 Boss base + 5 decision-blocked
34 E02 references = 24 Mechanic + 1 PressureChannel + 9 CounterWindowType
12 distinct E02 keys / unresolved 0
many-to-many reuse PASS
CandidateReuseOnly 34/34
runtimeImplemented true count = 0
MechanicProfile / SkillPattern / BossPhase / Runtime consumer rows = 0
gapSurveyRequired 16/16
BA-D1..D4 selected count = 0
Player-safe closure / answer leak 0
不可变性 / 防御性复制
Full / Player-safe signature sensitivity
反序 / Culture / 重复构造确定性
P0 exact identity equality
E02、P0、其他 protected hash
Runtime dependency leak 0
正式流程引用 0
```

负例至少覆盖：

```text
重复 contentId
错误 kind/chapter/family
缺一个基础 art slot
错误 decisionId 或把 blocked slot 标成 ready
未知 E02 key
E02 category collision
CandidateReuseOnly 被改为 implemented
隔离标志错误
Player-safe 泄漏 mechanic/gap/decision
undefined enum
null row / null collection / schema mismatch
```

七份报告必须由同源 Verifier 确定性覆盖生成：

```text
BoneAspectCarrierPresentationCatalogReport.md
BoneAspectCarrierPresentationCatalogSpec.csv
BoneAspectCarrierPresentationInventory.csv
BoneAspectCarrierPresentationArtSlotRows.csv
BoneAspectCarrierPresentationMechanicReferenceRows.csv
BoneAspectCarrierPresentationPlayerSafeFieldMatrix.csv
BoneAspectCarrierPresentationCatalogLeakCheckReport.md
```

连续生成两次必须 `7/7 hash identical`；删除任一报告后再次执行必须确定性重建。禁止 `WriteIfMissing`。

## 14. 自动验证

必须尝试：

```text
同源离线 Verifier
Unity batch compile
Unity batch Verifier
七报告重复生成确定性
缺失报告重建
P0 / E01-E10 / 并行隔离 protected hash
GUID 唯一性
新增文件 UTF-8 / LF / no BOM / no trailing whitespace
package-scope diff check
global git diff --check
精确变更文件清单
HEAD unchanged
```

Unity 只在没有活动 Editor、UnityLockfile 和其他 batch 时运行。发生占用时必须记录 `BLOCKED`，不得干预占用者。

本包无需场景手测。

## 15. 绝对禁止

```text
不修改任何已有文件
不修改 AGENTS.md / Docs/LOCKED/*
不修改 E01-E10 或 BuildSandbox legacy 数据
不新增或修改 E02 Vocabulary
不创建 MechanicProfile / Skill / CounterWindow / BossPhase / Encounter
不实现 BA-D1..D4，不选择默认答案
不创建 MonoBehaviour / ScriptableObject / Component
不引用 UnityEngine、UnityEditor（Editor Verifier 除外）
不创建或修改 Scene / Prefab / Config / BuildSettings / ProjectSettings
不添加 Sprite、Texture、Material、Animator、VFX 或图片
不写实际 asset path、Resources key 或 Addressables key
不修改 Item / Board / Inventory / Equipment / ItemSystemSnapshot
不修改 Battle / BattleContract / Battle Bridge / UnifiedBattlePage
不创建 HP、伤害、护壳、状态、阶段、施法、目标选择、Spawn 或 AI
不创建 EncounterPresentationProfile 或场景视觉 sidecar
不接正式 1-10 / 2-10
不接 3-10 / 4-10 正式章节
不修改 RunFlow / SaveData / Reward / Drop / Chapter
不发正式奖励，不写存档
不暴露完整 Build 答案、阈值、Readiness、Boss 六钥匙或 DropBias
不启动 BoneAspectMechanicGapSurvey01 或任何后续包
不 commit / tag / push
```

## 16. 完成回报

开发完成后直接同步 Primary Enemy Guard 与 RepoOps：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BoneAspectCarrierPresentationCatalog01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_BONEASPECTCARRIERPRESENTATIONCATALOG01

Result:
DEV_COMPLETE / QA_RESULT

新增文件: 必须为 21
修改已有文件: 必须为 0
Scene / Prefab / Config / Battle / Board / Item 修改: 0 / 0 / 0 / 0 / 0 / 0
Carrier / Enemy / Boss: 16 / 12 / 4
Chapter / VisualFamily: 4 / 4
Art slots: 93 = 72 / 16 / 5
E02 references: 34 = 24 / 1 / 9
Distinct E02 keys: 12
Unresolved E02 references: 0
CandidateReuseOnly: 34/34
Runtime implementation rows: 0
MechanicProfile / SkillPattern / BossPhase rows: 0 / 0 / 0
Gap survey required: 16/16
User decisions selected: 0/4
Player-safe leak: 0
Formal flow references: 0
P0 exact equality: PASS / FAIL
P0 protected hash: 8/8 unchanged
Other protected baseline: PASS / EXTERNAL_CONCURRENT_DRIFT / FAIL
Seven-report determinism: 7/7 PASS / FAIL
Missing-report regeneration: PASS / FAIL
Verifier: 通过数 / 总数
同源离线 Verifier: PASS / FAIL
Unity batch compile: PASS / FAIL / BLOCKED
Unity batch Verifier: PASS / FAIL / BLOCKED
Runtime dependency leak: 0
GUID 冲突: 0
新增文件尾随空白: 0
Global git diff --check: PASS / PREEXISTING_UNRELATED_DIFF / FAIL
Package-scope check: PASS / FAIL
HEAD unchanged
未 commit / tag / push
```

若出现并行 drift，必须同时附：

```text
exact path
task-start SHA-256
current SHA-256
本包是否写入
已请求哪个 Owner Guard 归因
```

完成后停止，等待 Primary Enemy Guard 复验。不得自动启动 P2。

## 17. 通过后仍冻结

P1 通过只代表 16 个 devOnly Carrier 展示目录和候选机制引用成立。

继续冻结：

```text
BoneAspectMechanicGapSurvey01
BoneAspectMechanicVocabularyExtension01
BoneAspectSkillCounterWindowBossPhaseData01
EncounterPresentationProfileContract01
EnemyRepresentativeVisualSample01
EnemyRuntimeState01
CrossSystem Effect Request
BattleSandbox 接线
正式 1-10 / 2-10 迁移
Reward / Save / Drop / Chapter
```

任何后续包必须由对应 Guard 另行签发。
