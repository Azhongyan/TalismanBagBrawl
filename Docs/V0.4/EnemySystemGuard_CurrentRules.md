# Enemy / Boss / Encounter System Guard 当前收口规则

生成日期：2026-07-16
负责人：Enemy / Boss / Encounter System Guard
范围：V0.4 devOnly Enemy 数据、接口、配置、离线验证与长期扩展底座

本文只记录 Enemy Guard 当前有效口径，不代表开发代码、改场景、改 Battle、改 Item、改 RunFlow、改 SaveData、改 Reward、改正式 Boss 流程或接 Battle Bridge。

## 一、当前总定位

Enemy 系统的长期目标是建设一套可持续扩展的敌人、Boss、地图规则与 Encounter 数据底座。

完成底座后，4-10、5-10 与后续章节应主要新增内容配置、表现资源和数值，不得为每个章节重新建设一套系统。

当前阶段只授权：

```text
接口
数据结构
配置
纯函数离线评估
数据校验
只读快照
诊断报告
```

当前阶段不授权任何场景、Prefab、MonoBehaviour、战斗运行时或正式流程接线。

核心原则：

```text
Enemy 系统拥有敌人、Boss、地图规则和 Encounter 的数据事实。
Battle 未来只读消费 Enemy 快照与效果请求。
Item 未来只读提供抽象 Build 能力快照。
Enemy 当前不得修改 Battle 或 Item。
```

## 二、“机制题”口径

“机制题”“Build 验证题”“破题”均为内部形容词，不是真实问卷、选择题或玩家答题系统。

正确含义：

```text
敌人 / Boss / 地图规则制造战斗压力
→ 玩家通过对应 Build、核心道具、词条、摆放或操作时机进行反制
→ 成功反制后产生虚弱、显形、打断、反震、破壳等战斗窗口
```

玩家侧禁止出现：

```text
题目
正确答案
标准答案
钥匙完成度
正确 Build
完整 BuildReadiness
```

未来内部规范命名优先使用：

```text
MechanicValidationProfile
BuildPressureProfile
BuildCapabilityRequirement
CounterWindow
PlayerMechanicHint
DeveloperDiagnostic
```

既有 `BuildProblem*` 名称属于历史 devOnly 数据名称，不得据此重新建设“题目预览面板”。

## 三、当前已有内容

### 1. V0.2 / V0.3 正式系统

当前正式系统已有：

```text
EnemyDefinition
EnemyRuntime
EnemySkillDefinition / EnemySkillRuntime
敌人意图与施法反馈
BossInfo
Boss Phase
BossReward
RunResult
1-10 / 2-10 正式流程
2-9 Boss 前停止
2-10 手动挑战
正式奖励、存档与章节推进
```

以上全部属于保护区。当前 Enemy 数据线只读参考，不修改、不迁移、不替换。

### 2. V0.4 devOnly 验证内容

当前已有：

```text
11 个 devOnly 敌人验证载体
7 个 devOnly Boss 验证载体
10 个地图规则种子
10 个普通敌人机制验证配置
6 个 Boss 机制验证配置
60 个开发章节验证条目
5 个混合 Boss Profile
devOnly 3-10 / 4-10 预览数据
机制提示、Boss 状态、施法条、浮字与失败反馈原型
```

当前所有 V0.4 Enemy / Boss / Encounter 数据继续保持：

```text
devOnly = true
isEnabled = false
entersFormalFlow = false
referencesFormalEnemyPool = false
referencesFormalBossPool = false
```

### 3. Item 系统依赖状态

Item 算法基础包当前为 `8/8 COMPLETE`，但成熟度仍是：

```text
BALANCE_CANDIDATE
EDITABLE
NOT_LIVE_LOCKED
NOT_BATTLE_CONNECTED
```

Enemy 当前不得接入 Item Runtime，也不得直接引用 Item 实例、棋盘 Controller、摆放组件或详情 UI。

## 四、领域拆分

长期 Enemy 数据底座拆分为：

```text
Enemy Catalog
Enemy Skill Pattern Schema
Boss Phase Schema
Map Rule Schema
Encounter Composition Schema
Build Capability Requirement
Mechanic Pressure Evaluation
Counter Window Schema
Player Feedback Projection Contract
Developer Diagnostic Snapshot
Data Validator / Canonical Snapshot
```

当前阶段只建设 Schema、Contract、纯函数评估与 Validator，不建设运行时执行器。

## 五、数据关系

敌人验证载体与机制验证配置不是一对一关系，正确关系为多对多：

```text
Enemy / Boss Profile
  表示谁在施加压力

MechanicValidationProfile
  表示施加什么压力、验证什么能力

MapRule
  表示环境如何改变压力

Encounter
  表示这些内容如何组合成一次战斗内容
```

允许：

```text
一个敌人复用多个机制
一个机制复用于多个敌人
同一 Boss 在不同 Encounter 使用不同地图规则或阶段组合
```

禁止：

```text
Enemy Profile 与 Mechanic Profile 同时重复定义同一事实
用章节编号作为机制逻辑
把 11/7 与 10/6 强制做成一对一
把推荐 Build 写成玩家必看的标准答案
```

## 六、V0.4 BattleSandbox 硬锁

用户已于 2026-07-13 明确：Enemy 当前只做接口和数据，不得调整 V0.4 战斗系统、棋盘摆放与操作手感；该场景后续还要接入新的 Item 系统。

以下内容全部冻结：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
V0.4 棋盘摆放、拖拽、旋转、托盘与输入逻辑
BuildGridInteractionPreviewController
BattleSandbox 战斗循环
EnemyCombatFeedbackController 及现有反馈运行时
Boss 状态、施法条、浮字、CombatLog 与 Tooltip UI
场景层级、RectTransform、Prefab 引用与 Binder
```

当前 Enemy 包必须满足：

```text
Scene modifications = 0
Prefab modifications = 0
MonoBehaviour scene attachment = 0
Battle runtime modifications = 0
Board/input modifications = 0
Item system modifications = 0
```

不得为当前数据阶段新建 Enemy 测试场景。

未来如需敌人模型、Animator、受击动作或技能 VFX 的纯视觉预览，必须另开 `EnemyVisualLab` 专项包。该视觉场景不得承载 Enemy 规则真相，也不得替代 BattleSandbox 集成验证。

## 七、Item 系统边界

Enemy 只定义自己未来需要的抽象能力，不读取 Item 内部实现。

推荐依赖方向：

```text
未来 Item 系统
→ 外部适配器
→ BuildCapabilitySnapshot
→ Enemy Readiness / Encounter 数据只读消费
```

Enemy 当前可以定义：

```text
BuildCapabilityId
BuildCapabilityRequirement
BuildCapabilitySnapshot 接口
ReadinessBand
CapabilityGap
```

抽象能力可以包括：

```text
BreakPower
ClearPower
CleansePower
GuardPower
ControlPower
EnergyStability
BurstWindow
SustainedDamage
PlacementShape
InterruptTiming
```

Enemy 当前禁止：

```text
引用 ItemSystemSnapshot 的具体实现类型
修改 Item 实例、属性、词条、品阶或核心效果
修改棋盘摆放、照亮、接力、阵位或 Build 计数
替 Item 系统判定真实技能触发
替 Reward / Drop 系统生成道具
```

## 八、BattleContract 未来只读字段草案

当前不修改 BattleContract。Enemy 只在自身 Contract 中预留未来可映射字段。

未来可提供：

```text
encounterId
enemyId
bossId
waveIndex
isBoss
displayName
enemyType
publicMechanicTags
portraitKey
currentHp / maxHp
shield
phaseId / publicPhaseLabel
intentId / publicIntentText
castSkillId / publicSkillName
castProgress / castRemainingSeconds
publicTargetCue
publicStatusSummary
isCounterWindowOpen
playerVisibleHints
```

未来可提供只读事件：

```text
EnemyIntentChanged
BossPhaseChanged
BossCastStarted
BossCastInterrupted
BossCastCompleted
CounterWindowOpened
CounterWindowClosed
MechanicHintRaised
EncounterEnemySideEnded
```

Enemy 不写 BattleResult、RewardClaimToken、SaveData、章节路线或正式结算。

## 九、玩家层与开发者诊断层

玩家可以看到：

```text
敌人 / Boss 名称与公开类型
当前生命、护盾与公开状态
当前意图、施法名称、施法进度与目标提示
阶段名称
虚弱、显形、打断、反震、破壳等状态反馈
模糊机制提示与失败提示
```

只能留在开发者诊断层：

```text
validationTargetBuilds
recommendedSynergies 完整列表
hardSolutionTags
requiredSynergy
requiredAffix
requiredStats
完整 Boss Key Requirements
minimumKeysRequired
精确能力阈值与权重
精确 CounterWindow 触发时间和判定阈值
BuildReadiness 完整评分与缺口
DropBias ID、候选和权重
随机 seed
sourceDataPath
期望胜率、战斗时长与调参系数
developerOnlyDiagnostics
```

玩家提示必须来自单独的 Player Projection，不得直接序列化开发者诊断对象。

## 十、当前测试方式

当前只允许：

```text
纯 C# / EditMode 测试
配置编译
序列化往返
确定性 Canonical Signature
ID 与多对多引用完整性检查
Schema 版本检查
玩家字段答案泄漏扫描
正式系统引用扫描
CSV / Markdown 离线报告
```

当前不要求也不允许通过修改 BattleSandbox 场景进行验证。

## 十一、当前队列

Enemy 数据与接口任务队列记录于：

```text
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
```

当前 Enemy 包状态：

```text
V0.4-EnemyDomainDataContract01
DEV_COMPLETE
Guard static review: PASS
Offline same-source Verifier: 34/34 PASS
Unity batch compile: PASS
Unity batch Verifier: BLOCKED / same project open in user Unity Editor
Final Guard result: PASS_WITH_RECORDED_UNITY_VERIFIER_BLOCK
Guard receipt: GUARD_PASS_ENEMYDOMAINDATACONTRACT01_WITH_RECORDED_UNITY_VERIFIER_BLOCK
```

本包已确认新增 19 个文件、修改已有文件 0 个，Scene / Prefab / Battle / Board / Item 修改均为 0，Leak 为 0。Guard 静态审阅未发现需要返工的代码问题。Unity Verifier 阻塞已如实记录，不得对外声称该项 PASS；根据本包 assignment 的阻塞记录规则，该项不阻止数据契约包收口。

`V0.4-EnemyMechanicVocabulary01` 已完成并由 Guard 接受：

```text
Final Guard result: QA_PASS
Guard receipt: GUARD_PASS_ENEMYMECHANICVOCABULARY01
Delivered files: 14
Modified existing files: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Vocabulary: 63
Legacy keys: 124 total / 102 mapped / 22 OUT_OF_SCOPE / 0 unresolved
E01 protected hashes: 6/6 unchanged
Offline Verifier: 48/48 PASS
Unity batch Verifier: 48/48 PASS
Unity batch compile: PASS
Leak count: 0
```

`V0.4-EnemyValidationContentNormalize01` 已在 `V0.4-EnemyValidationContentNormalize01-GuardFix01` 修复完整 `CanonicalSignature` 未包含 Enemy/Boss `DeveloperOnly` 投影的问题，并由 Guard 复审通过：

```text
Final Guard result: QA_PASS
Guard receipt: GUARD_PASS_ENEMYVALIDATIONCONTENTNORMALIZE01_AFTER_GUARDFIX01
GuardFix Verifier: 33/33 PASS
Canonical developer-content sensitivity: PASS
Player-safe developer-content isolation: PASS
Carriers: 11 / 7 / 18
Mechanic profiles: 10 / 6 / 16
MapRules: 10
Carrier bindings: 17
MapRule bindings: 30
Intentional exceptions: 2
Unresolved / unexplained orphan: 0
E01 + E02 protected hashes: 10/10 unchanged
Legacy source hashes: 3/3 unchanged
Runtime BuildSandbox references: 0
Editor-only legacy reader: 1
Unity batch compile: PASS
Unity batch Verifier: 33/33 PASS
Leak count: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
```

`V0.4-EncounterCompositionSchema01` 已完成并由 Guard 接受：

```text
Final Guard result: QA_PASS_WITH_RECORDED_UNITY_BATCH_BLOCK
Guard receipt: GUARD_PASS_ENCOUNTERCOMPOSITIONSCHEMA01_WITH_RECORDED_UNITY_BATCH_BLOCK
Delivered files: 14
Modified existing files: 0
Offline same-source Verifier: 124/124 PASS
Fixture: 2 Encounter / 4 Wave / 6 Slot
E01 + E02 + E03 protected hashes: 14/14 unchanged
Legacy source hashes: 3/3 unchanged
Unresolved references: 0
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Unity batch compile / Verifier: BLOCKED / same project open in user Unity Editor
Unity batch PASS claim: none
```

本包只建立了 Encounter、Wave、EnemySlot/BossSlot、MapRule 引用和 MechanicProfile 选择的纯数据结构。未生成对象、运行波次、接入 BattleContract、修改棋盘手感、读取 Item、硬编码 3-10/4-10 内容或实现正式流程。

`V0.4-EnemySkillBossPhaseSchema01` 已完成并由 Guard 接受：

```text
Final Guard result: QA_PASS
Guard receipt: GUARD_PASS_ENEMYSKILLBOSSPHASESCHEMA01
Delivered files: 16
Modified existing files: 0
Offline same-source Verifier: 134/134 PASS
Unity batch compile / Verifier: PASS / 134/134 PASS
Fixture: 4 Pattern / 3 Sequence / 3 Binding / 3 Phase / 2 Plan
Pattern / Sequence / Phase reuse: PASS
Player-safe isolation: PASS
E01-E04 protected hashes: 18/18 unchanged
Legacy source hashes: 3/3 unchanged
Unresolved references: 0
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
HEAD unchanged: 1050830bfebdb92401343de4a0734bbe57561c63
```

本包只建立了 SkillPattern、SkillSequence、CarrierSkillBinding、BossPhaseProfile、BossPhasePlan 及 player-safe / internal-only / developer-only 分层数据。Guard 静态复审未发现计时、目标选择、伤害、阶段切换等运行时执行，也未发现玩家完整答案泄漏或对现有施法条、战斗反馈、Battle、Board、Item、Scene 的修改。

`V0.4-CounterWindowAndPressureSchema01` 已完成并由 Guard 接受：

```text
Final Guard result: QA_PASS
Guard receipt: GUARD_PASS_COUNTERWINDOWANDPRESSURESCHEMA01
Delivered files: 16
Modified existing files: 0
Offline same-source Verifier: 296/296 PASS
Unity batch compile / Verifier: PASS / 296/296 PASS
Fixture: 4 Pressure / 3 Window / 6 PressureSource / 4 WindowSource / 5 PressureWindow / 11 Requirement
Pressure reuse: PASS
CounterWindow reuse: PASS
Any / All requirement groups: PASS
Player-safe isolation: PASS
E01-E05 protected hashes: 23/23 unchanged
Legacy source hashes: 3/3 unchanged
Unresolved references: 0
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
HEAD unchanged: 1050830bfebdb92401343de4a0734bbe57561c63
```

本包只建立了 BuildPressureProfile、CounterWindowProfile、Any/All BuildCapabilityRequirementGroup、条件规格与多对多绑定。Guard 静态复审未发现真实 Build 或 Item 读取、战斗事件监听、时间推进、窗口执行、Readiness 计算、完整答案泄漏或对反馈 Controller、Battle、Board、Item、Scene 的修改。

`V0.4-BuildCapabilityReadContract01` 已完成并由 Guard 接受：

```text
Final Guard result: QA_PASS
Guard receipt: GUARD_PASS_BUILDCAPABILITYREADCONTRACT01
Delivered files: 16
Modified existing files: 0
Unity batch compile / Verifier: PASS / 298/298 PASS
Same-source offline entry: PRESENT
Separate offline execution result in handoff: NOT_REPORTED
Fixture: 16 KnownKeys / 16 CompleteValues / 4 SparseValues / 6 SourceSummaries / 3 GapShapes / 5 Bands
Complete coverage: PASS
Sparse unknown preservation: PASS
Known Zero vs Unknown: PASS
Item-independent: PASS
E01-E06 protected hashes: 28/28 unchanged
Legacy source hashes: 3/3 unchanged
Unresolved references: 0
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
HEAD unchanged: 1050830bfebdb92401343de4a0734bbe57561c63
Guard sync: TASK_STATUS_SYNC_TO_GUARD_REPOOPS_ACCEPTED
RepoOps status: REPOOPS_RECORD_DONE
```

本包只建立了外部可构造、Enemy 可只读消费的 BuildCapabilitySnapshot Contract，以及 E08 未来使用的 ReadinessBand / CapabilityGap 数据形状。Guard 静态复审未发现 Item、Affix、Synergy、棋盘或真实 Build 读取，也未发现 Adapter、能力计算、E06 阈值比较、Readiness 生成、玩家投影或 BattleContract 接线。

用户已明确请求下一任务包，`V0.4-EnemyOfflineReadinessEvaluator01` 已签发：

```text
Current package: V0.4-EnemyOfflineReadinessEvaluator01
Status: ASSIGNED / NOT_STARTED
Scope: PURE_EVALUATOR_ONLY
Assignment: Docs/V0.4/EnemyOfflineReadinessEvaluator01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_ENEMYOFFLINEREADINESSEVALUATOR01
Expected new files: 16
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
```

本包只建立 E06 Pressure、E07 Capability 与调用方 MapRule adjustment 的纯函数离线 Readiness Evaluator。Developer 结果保留 Group/Requirement/Gap；玩家结果只保留 E06 公开提示和粗粒度 Severity。不得读取真实 Item/Build/棋盘或战斗状态，不得自动配装、执行战斗、修改反馈、接 BattleContract 或触碰 Scene。

下一队列包 `V0.4-EnemyDataValidatorAndSnapshot01` 保持 `QUEUED / NOT_STARTED / NOT_RELEASED`，不得自动启动。

`V0.4-EnemyOfflineReadinessEvaluator01` 已回报 `DEV_COMPLETE / QA_PASS`。Guard 独立复审确认：

```text
新增文件: 16/16
修改已有文件: 0
Runtime Any / All / Required / Recommended: PASS
Known Zero / Unknown: PASS
Map adjustment / clamp / order independence: PASS
Player-safe property shape and payload isolation: PASS
Scenarios: 15
ReadinessBand: 5/5 covered
E01-E07 protected hash: 33/33 unchanged
Legacy source hash: 3/3 unchanged
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
HEAD: 1050830bfebdb92401343de4a0734bbe57561c63
```

Guard 发现 E08 报告基线不自洽：

```text
baseline.e05.fixture
Expected: 4/3/3/3/2
Actual: 4/5/4/3/6
Result: PASS
```

原因是 Verifier 的 `Actual` 使用 E05 CSV 展开行数，而 PASS 判定使用逻辑实体去重数。保护 hash 证明 E05 本身未被修改，但该报告不能作为 E09 的可信输入基线。

Guard 返回：

```text
GUARD_RETURN_ENEMYOFFLINEREADINESSEVALUATOR01_BASELINEREPORT01
RepoOps status: REPOOPS_RECORD_DONE
Current package: V0.4-EnemyOfflineReadinessEvaluator01-GuardFix01
Status: ASSIGNED / NOT_STARTED
Scope: REPORT_BASELINE_COUNT_FIX_ONLY
Assignment: Docs/V0.4/EnemyOfflineReadinessEvaluator01_GuardFix01_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX01
Allowed modified files: 3
Allowed new files: 0
```

GuardFix01 通过前不签发 E08 Guard PASS；`V0.4-EnemyDataValidatorAndSnapshot01` 继续保持 `QUEUED / NOT_STARTED / NOT_RELEASED`。

`V0.4-EnemyOfflineReadinessEvaluator01-GuardFix01` 已回报 `DEV_COMPLETE / QA_PASS`。Guard 复审确认 Verifier、主报告与 Spec 已统一为 `4/3/3/3/2 / PASS`，但 LeakCheck 仍保留旧的 `actual 4/5/4/3/6`。

根因：

```text
Main Report: WriteUtf8 -> 已刷新
Spec: WriteUtf8 -> 已刷新
FieldMatrix: WriteUtf8IfMissing
ScenarioRows: WriteUtf8IfMissing
LeakCheck: WriteUtf8IfMissing -> 旧基线残留
```

GuardFix01 开发窗口严格遵守了三文件白名单；该残留来自 GuardFix01 Assignment 未纳入派生报告再生成策略，不记为开发窗口越界。

Guard 返回：

```text
GUARD_RETURN_ENEMYOFFLINEREADINESSEVALUATOR01_STALELEAKREPORT01
RepoOps status: REPOOPS_RECORD_DONE
Current package: V0.4-EnemyOfflineReadinessEvaluator01-GuardFix02
Status: ASSIGNED / NOT_STARTED
Scope: DERIVED_REPORT_REGENERATION_ONLY
Assignment: Docs/V0.4/EnemyOfflineReadinessEvaluator01_GuardFix02_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX02
Allowed content modifications: 2
Allowed new files: 0
```

GuardFix02 通过前不签发 E08 Guard PASS；`V0.4-EnemyDataValidatorAndSnapshot01` 继续保持 `QUEUED / NOT_STARTED / NOT_RELEASED`。

`V0.4-EnemyOfflineReadinessEvaluator01-GuardFix02` 已回报 `DEV_COMPLETE / QA_PASS`，并由 Guard 完成静态复审：

```text
E05 baseline across Report / Spec / LeakCheck: 4/3/3/3/2 PASS
Five-report repeat generation: 5/5 deterministic
Missing LeakCheck regeneration: PASS
Unchanged four-report hash: 4/4 unchanged
Runtime E08 hash: 9/9 unchanged
Verifier: 212/212 PASS
Unity batch compile: PASS
Unity batch Verifier: 212/212 PASS
Same-source offline Verifier: 212/212 PASS
E01-E07 protected hash: 33/33 unchanged
Legacy source hash: 3/3 unchanged
Leak: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Global git diff --check: PREEXISTING_UNRELATED_DIFF
Package-scope check: PASS
HEAD unchanged: 1050830bfebdb92401343de4a0734bbe57561c63
```

Guard receipt：

```text
GUARD_PASS_ENEMYOFFLINEREADINESSEVALUATOR01
RepoOps status: REPOOPS_RECORD_DONE
```

E08 最终通过。五份 E08 报告现在都由同源 Verifier 每次确定性重建，不再保留陈旧派生报告。Evaluator 的纯离线边界、玩家安全投影和 developer-only 完整诊断隔离保持不变。

当前没有自动释放新包。`V0.4-EnemyDataValidatorAndSnapshot01` 保持 `QUEUED / NOT_STARTED / NOT_RELEASED`，等待用户明确请求下一任务包。

用户已明确请求下一任务包，`V0.4-EnemyDataValidatorAndSnapshot01` 已签发：

```text
Current package: V0.4-EnemyDataValidatorAndSnapshot01
Status: ASSIGNED / NOT_STARTED
Scope: SYSTEM_SNAPSHOT_AND_CROSS_CATALOG_VALIDATION_ONLY
Assignment: Docs/V0.4/EnemyDataValidatorAndSnapshot01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_ENEMYDATAVALIDATORANDSNAPSHOT01
Expected new files: 20
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
```

E09 只把 E01-E06 静态 Catalog 聚合为 `EnemySystemSnapshot.v1`。E07 BuildCapability 与 E08 Readiness 是 transient contract，只进入八项 Schema Manifest，不嵌入根快照。玩家根投影只汇总现有 player-safe projection，不包含 Encounter 编排、精确要求、阈值、关系绑定或开发者答案。

下一队列包 `V0.4-DevEncounterSeedData01` 保持 `QUEUED / NOT_STARTED / NOT_RELEASED`，不得自动启动。

## 2026-07-15 E09 Guard 静态复审与 GuardFix01 释放

```text
Original package: V0.4-EnemyDataValidatorAndSnapshot01
Developer result: DEV_COMPLETE / QA_PASS
Offline Verifier: 173/173 PASS
Guard result: RETURNED_FOR_FIX
Guard receipt: GUARD_RETURN_ENEMYDATAVALIDATORANDSNAPSHOT01_VALIDATIONPROOFGAP01

Fix package: V0.4-EnemyDataValidatorAndSnapshot01-GuardFix01
Fix status: ASSIGNED / NOT_STARTED
Assignment: Docs/V0.4/EnemyDataValidatorAndSnapshot01_GuardFix01_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX01
New files allowed: 0
Content modifications allowed: 10 exact E09 files
E10: NOT_RELEASED
```

E09 原包的静态根结构、E07/E08 不嵌入、PlayerSafe 基本隔离和跨系统红线均通过，但 Guard 不接受当前 `173/173` 作为最终证明。E02 `CounterWindowTypeKey` 是窗口类型词汇，E06 `CounterWindowProfileSnapshot` 才是窗口实例；两者不得共用 `CounterWindow` 实体身份空间。当前实现还缺少 E03/E05/E06 developer/player 签名矩阵、真实 transient mutation、若干负例、不可变性和 Root 完整属性闭包证明。

GuardFix01 只允许修 E09 的 `EnemySystemSnapshot.cs`、`EnemySystemDataValidation.cs`、Verifier 与七份派生报告。不得修改 E01-E08、Scene、Battle、Board、Item 或正式流程；不得创建 E10 内容。

`V0.4-DevEncounterSeedData01` 继续保持 `QUEUED / NOT_STARTED / NOT_RELEASED`。GuardFix01 通过前不得签发 E09 Guard PASS。

## 2026-07-16 E09 GuardFix01 复审与 GuardFix02 释放

```text
Reviewed package: V0.4-EnemyDataValidatorAndSnapshot01-GuardFix01
Developer result: DEV_COMPLETE / QA_PASS
Verifier: 215/215 PASS
Unity batch compile / Verifier: PASS / PASS
Guard result: RETURNED_FOR_FIX02
Guard receipt: GUARD_RETURN_ENEMYDATAVALIDATORANDSNAPSHOT01_E04SIGNATUREOMISSION01

Fix package: V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02
Fix status: ASSIGNED / NOT_STARTED
Assignment: Docs/V0.4/EnemyDataValidatorAndSnapshot01_GuardFix02_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX02
New files allowed: 0
Content modifications allowed: 9 exact E09 files
E10: NOT_RELEASED
```

GuardFix01 已修正 CounterWindow 身份语义，并补齐签名矩阵、transient mutation、RelationKind、负例、不可变性和 Root property closure。唯一剩余问题是 Full Root 的 Canonical payload 对 E04 做了特殊排除：`ManifestRow` 将 E04 `ComponentCanonicalSignature` 置空，因此 Encounter quantity 等编排变化不会改变 Full signature。

GuardFix02 只允许恢复 E04 Full signature 敏感性、修正对应 Verifier 预期并重建七份派生报告。`EnemySystemDataValidation.cs`、E01-E08、Scene、Battle、Board、Item 与正式流程全部冻结。

`V0.4-DevEncounterSeedData01` 继续保持 `QUEUED / NOT_STARTED / NOT_RELEASED`。GuardFix02 通过前不得签发 E09 Guard PASS。

## 2026-07-16 E09 GuardFix02 最终通过

```text
Package: V0.4-EnemyDataValidatorAndSnapshot01
Final fix: V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02
Guard receipt: GUARD_PASS_ENEMYDATAVALIDATORANDSNAPSHOT01
Result: GUARD_ACCEPTED / QA_PASS
Verifier: 220/220 PASS
Offline same-source Verifier: 220/220 PASS
Unity batch compile / Verifier: PASS / 220/220 PASS
Static Manifest full signatures: 6/6 non-empty
E01-E08 protected hash: 38/38 unchanged
Legacy hash: 3/3 unchanged
Other E09 frozen hash: 11/11 unchanged
Seven-report determinism: 7/7
RepoOps status: PENDING_EXTERNAL_SYNC
E10: NOT_RELEASED
```

E09 正式通过。E02 `CounterWindowTypeKey` 与 E06 `CounterWindowProfileSnapshot` 身份已分离；Root / PlayerSafe 属性闭包、负例、不可变性、多对多与 17 类关系覆盖均通过。Full Root 无条件纳入 E01-E06 六个静态组件签名，E04 Encounter 编排变化现在正确表现为 `Full changes / PlayerSafe stable`。

E07 BuildCapability 与 E08 Readiness 继续只作为 transient contract 登记，不嵌入静态 Root、Identity、Relation 或玩家投影。E09 未接 Scene、Battle、Board、Item、正式章节、奖励或存档。

`V0.4-DevEncounterSeedData01` 当前仍为 `QUEUED / NOT_STARTED / NOT_RELEASED`，等待用户明确请求下一任务包。

## 2026-07-16 E10 DevEncounterSeedData01 释放

用户已明确请求下一任务包，E10 已签发：

```text
Current package: V0.4-DevEncounterSeedData01
Status: ASSIGNED / NOT_STARTED
Scope: DEVONLY_SEED_DATA_ONLY
Assignment: Docs/V0.4/DevEncounterSeedData01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_DEVENCOUNTERSEEDDATA01
Expected new files: 21
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
Post-E10 packages: FROZEN / NOT_RELEASED
```

E10 只建立四个 devOnly 验证 Encounter 的权威数据种子：

```text
dev_seed_3_10_guard_wall
dev_seed_3_10_cleanse_corner
dev_seed_4_10_furnace_core
dev_seed_4_10_thunder_fire_cross
```

每个 Encounter 固定 ordinary / damping / Boss 三波纯数据。E05 Skill 与 E06 Pressure/CounterWindow 按机制复用；不得按章节复制系统。`furnace_core` 是 MapRule 环境压力与偷灵/供能载体压力的合法跨层组合，不得把它强制改造成 MapRule 与全部 Slot mechanic 一对一。

E10 是未来 Enemy 适配器的唯一内容种子。旧 `DevChapterBalanceRun`、`BuildProblemSeedData` 与相关 BuildSandbox 内容只作迁移对照，不得成为 E10 Runtime 依赖；本包也不删除或替换旧消费路径。

玩家层只保留机制现象、技能意图、Boss 阶段 cue、压力提示与 CounterWindow cue。Capability requirement、Any/All、精确阈值、完整 Readiness、具体 Build/Item、Boss 六钥匙与 DropBias 全部留在 developer-only 层。

E10 没有场景手测；完成后只同步 Guard + RepoOps，不得自动启动 Runtime、Battle、Item Adapter 或正式流程包。

## 2026-07-16 E10 静态复审与 GuardFix01 释放

```text
Original package: V0.4-DevEncounterSeedData01
Developer result: DEV_COMPLETE / QA_PASS
Original Verifier: 50/50 PASS
Guard result: RETURNED_FOR_FIX
Guard receipt: GUARD_RETURN_DEVENCOUNTERSEEDDATA01_SEMANTICREACHABILITY01

Fix package: V0.4-DevEncounterSeedData01-GuardFix01
Fix status: ASSIGNED / NOT_STARTED
Scope: E10_SEMANTIC_FIDELITY_AND_WINDOW_REACHABILITY_ONLY
Assignment: Docs/V0.4/DevEncounterSeedData01_GuardFix01_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_DEVENCOUNTERSEEDDATA01_GUARDFIX01
New files allowed: 0
Content modifications allowed: 11 exact E10 files
Post-E10 packages: FROZEN / NOT_RELEASED
RepoOps status: REPOOPS_RECORD_DONE
```

Guard 已确认原 E10 的 21 文件白名单、四套 Encounter、E05/E06 逻辑实体计数、Legacy mapping、隔离、PlayerSafe、八报告确定性、Unity/离线回报与跨系统 Leak 没有越界。

当前不放行的第一项原因是 E10 Validator 尚未锁定 E03 -> E06 内容忠实性。现有校验只检查 capability requirement 数量、Required/Recommended 模式和 channel 权重总和；把 capability 或 pressure channel 换成另一条合法 E02 key 仍可能通过，无法阻止机制答案在 E03/E06 之间漂移。

第二项原因是 CounterWindow 图可解析但不完全可达。`polluted-tile -> cleanse` 与 `complex-eye -> energy` 两条 Pressure/Window 关系没有共同 `(SourceKind, SourceId)`；部分 WindowSource 还绑定到一个 Skill/Phase，而窗口 OpenCondition 只监听另一个 Skill/Phase。未来 Adapter 不能只凭当前数据稳定取得可触发窗口。

GuardFix01 只补 capability/channel/source fidelity、窗口 source-condition compatibility、Pressure/Window source reachability、对应负例与报告条件行。Snapshot public shape、四个 Seed、Encounter/Wave/Slot、E05 内容、PlayerSafe、basis-point 策略、E01-E09、Legacy、Scene、Battle、Board、Item 和正式流程全部冻结。

GuardFix01 通过前不签发 E10 Guard PASS，不释放任何后置包。

## 2026-07-16 E10 GuardFix01 最终通过

```text
Package: V0.4-DevEncounterSeedData01
Final fix: V0.4-DevEncounterSeedData01-GuardFix01
Guard receipt: GUARD_PASS_DEVENCOUNTERSEEDDATA01
Result: GUARD_ACCEPTED / QA_PASS
Verifier: 60/60 PASS
Offline same-source Verifier: 60/60 PASS
Unity batch compile / Verifier: PASS / 60/60 PASS
Capability fidelity: 10/10 PASS
Pressure channel fidelity: 10/10 PASS
Pressure source fidelity: 39/39 PASS
CounterWindowSource rows: 20
CounterWindowSource distinct Skill / Phase / Map: 9 / 8 / 2
Window source-condition compatibility: 20/20 PASS
Pressure-window exact-source reachability: 13/13 PASS
Semantic negative fixtures: 5/5 PASS
Eight-report determinism / missing-report regeneration: PASS / PASS
E01-E09 protected hash: 44/44 unchanged
Legacy source hash: 5/5 unchanged
Other E10 frozen hash: 10/10 unchanged
Runtime / player-answer / formal-flow leak: 0 / 0 / 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
RepoOps status: REPOOPS_RECORD_DONE
Commit / tag / push: none
```

E10 正式通过。E03 -> E06 的 capability、pressure channel 和 source 忠实性现在由 E10 Validator 锁定；所有 20 条 CounterWindowSource 都符合窗口 OpenCondition 兼容规则，全部 13 条 PressureWindow 都能通过共同 `(SourceKind, SourceId)` 到达对应窗口。此前缺失的 `polluted-tile -> cleanse` 与 `complex-eye -> energy` 已分别由 MapRule 与 BossPhase 来源闭合。

Enemy 数据阶段 E01-E10 已收口完成。当前没有自动释放的下一包；MapRule Runtime、Enemy/Boss Skill Runtime、BossPhase Runtime、BattleSandbox 接线、BattleContract / Battle Bridge、Item Adapter 与正式章节、奖励、存档接线继续保持 `FROZEN / NOT_RELEASED`。

## 十二、长期红线

未经独立跨系统授权，Enemy Guard 与 Enemy 开发包不得：

```text
修改 AGENTS.md
修改 Docs/LOCKED/*
修改正式 1-10 / 2-10
修改 RunFlow / PageState / FormationState
修改 Reward / Drop / SaveData
修改正式 BossInfo / BossReward / RunResult
修改 Battle Bridge / UnifiedBattlePage
修改 V0.4 BattleSandbox 场景、棋盘或输入手感
修改 Item 系统
接正式章节入口
发正式奖励
推进正式章节
commit / tag / push
```

## 十三、Shared Module Layering / Enemy Prefab Presentation

Enemy Guard 后续必须同时读取并执行：

```text
Docs/V0.4/SHARED_MODULE_LAYERING_AND_PREFAB_PRESENTATION_GUARD.md
```

Enemy 数据、Enemy Runtime 状态、Enemy View 必须分层。Enemy Prefab 只读取
Enemy/Combat ViewModel 并渲染动画、受伤、技能、状态和详情，不得挂载 Enemy 数据
真源、当前 HP/Boss阶段或第二套 EnemyManager。Enemy 只能消费 Item Snapshot 的
公开结果，不得建立 Item 形状、供能或 Build 的第二套真源。

若任务要求在每个场景重画 Enemy UI、让 Enemy View 直接改 Item/Reward/Save/Chapter，
或超过两个技术包仍没有可操作的敌人体验手测，Enemy Guard 必须暂停并向用户解释。

Enemy状态、技能、伤害、施法条、Boss阶段和机制提示进入玩家UI时，必须逐字段提供真实
Enemy Runtime端到端血缘。Enemy Fixture、Schema或离线Evaluator通过不能替代真实敌人
在指定Scene产生对应Snapshot、ViewModel和可见反馈。
