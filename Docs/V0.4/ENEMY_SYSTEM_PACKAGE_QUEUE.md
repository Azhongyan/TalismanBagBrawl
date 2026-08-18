# Enemy / Boss / Encounter System Package Queue

更新时间：2026-07-16
所属：Enemy / Boss / Encounter System Guard
范围：V0.4 devOnly 数据、接口、纯函数与离线验证

## 一、总体状态

```text
Status: E10_GUARD_ACCEPTED / QA_PASS / REPOOPS_RECORDED
Authorization scope: DATA_AND_CONTRACT_ONLY / CURRENT_QUEUE_COMPLETE
Current package: NONE / WAITING_USER_REQUEST
Next queued: NONE / POST_E10_PACKAGES_FROZEN
Scene work: FORBIDDEN
Battle runtime work: FORBIDDEN
Item integration: FORBIDDEN
Formal integration: NOT_STARTED
```

本队列是 Enemy 长期底座的数据阶段队列，不代表已授权开发。每个包仍需用户明确启动，并由 Enemy Guard 输出单包 assignment。

## 二、全队列硬边界

所有包必须满足：

```text
devOnly = true
isEnabled = false
entersFormalFlow = false
referencesFormalEnemyPool = false
referencesFormalBossPool = false
Scene modifications = 0
Prefab modifications = 0
Battle runtime modifications = 0
Board/input modifications = 0
Item system modifications = 0
RunFlow/Reward/SaveData modifications = 0
```

允许新增范围只限独立 Enemy 数据、Contract、Schema、纯函数 Evaluator、Validator、测试与报告。

## 三、包顺序

| 顺序 | 包名 | 状态 | 目标 |
|---|---|---|---|
| E01 | `V0.4-EnemyDomainDataContract01` | `GUARD_ACCEPTED / PASS_WITH_RECORDED_UNITY_VERIFIER_BLOCK` | 长期 Enemy 数据模型与只读 Contract；不声称 Unity Verifier PASS |
| E02 | `V0.4-EnemyMechanicVocabulary01` | `GUARD_ACCEPTED / QA_PASS / UNITY_48_OF_48_PASS` | 统一机制与能力稳定键；63 个词汇、124 个旧键、Leak 0 |
| E03 | `V0.4-EnemyValidationContentNormalize01` | `GUARD_ACCEPTED / QA_PASS / UNITY_33_OF_33_PASS` | 归一化内容、多对多绑定、完整签名与 player-safe 隔离已通过 |
| E04 | `V0.4-EncounterCompositionSchema01` | `GUARD_ACCEPTED / QA_PASS / OFFLINE_124_OF_124_PASS / UNITY_BATCH_BLOCKED` | Encounter、Wave、Slot 与既有 E03 引用组合 Schema；未执行战斗 |
| E05 | `V0.4-EnemySkillBossPhaseSchema01` | `GUARD_ACCEPTED / QA_PASS / UNITY_134_OF_134_PASS` | 技能、序列、公开提示与 Boss 阶段纯数据 Schema；复用与 player-safe 隔离已通过 |
| E06 | `V0.4-CounterWindowAndPressureSchema01` | `GUARD_ACCEPTED / QA_PASS / UNITY_296_OF_296_PASS` | 反制窗口、Build 压力、多解要求组与多对多绑定纯数据 Schema；player-safe 隔离通过 |
| E07 | `V0.4-BuildCapabilityReadContract01` | `GUARD_ACCEPTED / QA_PASS / UNITY_298_OF_298_PASS / REPOOPS_RECORDED` | Enemy 只读消费的抽象 Build 能力 Contract；Complete/Sparse 与 Known Zero/Unknown 语义通过 |
| E08 | `V0.4-EnemyOfflineReadinessEvaluator01` | `GUARD_ACCEPTED / QA_PASS / REPOOPS_RECORDED` | 纯函数 Readiness、player-safe 隔离与五报告确定性再生成均通过 |
| E09 | `V0.4-EnemyDataValidatorAndSnapshot01` | `GUARD_ACCEPTED / QA_PASS / UNITY_220_OF_220_PASS` | E01-E06 静态根、E07/E08 transient 隔离、跨 Catalog 校验与签名矩阵全部通过 |
| E10 | `V0.4-DevEncounterSeedData01` | `GUARD_ACCEPTED / QA_PASS / UNITY_60_OF_60_PASS / REPOOPS_RECORDED` | 四个 devOnly Encounter、E03 -> E06 忠实性与 CounterWindow 可达性均已通过 |

## 四、E01 EnemyDomainDataContract01

Guard 当前验收记录（2026-07-13）：

```text
Developer result: DEV_COMPLETE / QA_RESULT: PARTIAL_PASS
Guard static review: PASS
Delivered files: 19
Modified existing files: 0
Scene / Prefab / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0
Offline same-source Verifier: 34/34 PASS
Unity batch compile: PASS / return code 0
Unity batch Verifier: BLOCKED / same project open in user Unity Editor
Leak count: 0
git diff --check: PASS
Commit / tag / push: none
```

Guard 结论：

```text
数据契约、不可变性、确定性、ID 校验、引用校验和隔离边界静态验收通过。
未发现需要返工的代码问题。
Guard 接受本包为 PASS_WITH_RECORDED_UNITY_VERIFIER_BLOCK；assignment 已要求同工程占用时如实记录阻塞，不要求为此停包。
Guard receipt: GUARD_PASS_ENEMYDOMAINDATACONTRACT01_WITH_RECORDED_UNITY_VERIFIER_BLOCK
不得声称 Unity batch Verifier PASS；该项保留为已记录诊断。
E02 不自动启动。
```

只做：

```text
EnemyProfile 数据接口
BossProfile 数据接口
EnemySkillPattern 数据接口
BossPhaseProfile 数据接口
MapRuleProfile 数据接口
EncounterProfile 数据接口
CounterWindowProfile 数据接口
只读 Snapshot 接口
schemaId / schemaVersion
稳定 ID 与 Canonical Signature 约定
```

禁止：

```text
MonoBehaviour
ScriptableObject 场景挂载
战斗 Runtime
伤害、护盾、状态结算
修改既有 EnemyDefinition
修改 BattleContract
引用 Item 具体类型
```

验收：

```text
所有 Contract 只读
相同数据 Canonical Signature 一致
无章节编号硬编码逻辑
无 Battle / Item / RunFlow / Reward / SaveData 依赖
Scene / Prefab diff = 0
```

## 五、E02 EnemyMechanicVocabulary01

Guard 验收记录（2026-07-13）：

```text
Developer result: DEV_COMPLETE / QA_PASS
Guard review: PASS
Delivered files: 14
Modified existing files: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
E01 protected hashes: 6/6 unchanged
Vocabulary: 63
Legacy keys: 124 total / 102 mapped / 22 OUT_OF_SCOPE / 0 unresolved
Offline same-source Verifier: 48/48 PASS
Unity batch Verifier: 48/48 PASS
Unity batch compile: PASS / return code 0
Leak count: 0
GUID conflicts: 0
Trailing whitespace: 0
git diff --check: PASS
Commit / tag / push: none
Guard receipt: GUARD_PASS_ENEMYMECHANICVOCABULARY01
```

Guard 结论：本包通过。未发现需要返工的代码或边界问题。E03 不自动启动。

只做：

```text
MechanicTag 稳定键
BuildCapabilityId 稳定键
PressureChannel 稳定键
CounterWindowType 稳定键
PlayerHintCategory 稳定键
DeveloperDiagnosticCategory 稳定键
旧 devOnly tag 到新稳定键的映射表
```

首批覆盖：

```text
护盾 / 破盾
群怪 / 清场
毒燃 / 净化
偷灵 / 供能稳定
封符 / 控制或净化
高爆发 / 护阵
施法 / 打断
厚血 / 持续输出
污染格 / 摆放稳定
阵眼干扰 / 供能与形态
```

禁止把内部标签直接作为玩家答案显示。

验收：稳定键唯一、旧映射可追踪、未知键可校验、玩家词与开发者词分层。

## 六、E03 EnemyValidationContentNormalize01

Guard review（2026-07-14）：

```text
Original developer report: DEV_COMPLETE / QA_PASS
Original Guard result: REWORK_REQUIRED
GuardFix package: V0.4-EnemyValidationContentNormalize01-GuardFix01
GuardFix developer result: DEV_COMPLETE / QA_PASS
Guard re-review: PASS
Canonical developer-content sensitivity: PASS
Player-safe developer-content isolation: PASS
Offline same-source Verifier: 33/33 PASS
Unity batch compile: PASS / return code 0
Unity batch Verifier: 33/33 PASS
Carriers: 11 enemy / 7 boss / 18 total
Mechanic profiles: 10 enemy / 6 boss / 16 total
MapRules: 10
Carrier bindings: 17
MapRule bindings: 30
Intentional exceptions: 2
Unresolved / unexplained orphan: 0
E01 + E02 protected hashes: 10/10 unchanged
Legacy source hashes: 3/3 unchanged
Runtime BuildSandbox references: 0
Editor-only legacy reader: 1
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Leak count: 0
git diff --check: PASS
Commit / tag / push: none
Guard receipt: GUARD_PASS_ENEMYVALIDATIONCONTENTNORMALIZE01_AFTER_GUARDFIX01
E04: NOT_RELEASED
```

只做：

```text
11 个敌人验证载体清单
7 个 Boss 验证载体清单
10 个普通机制验证配置清单
6 个 Boss 机制验证配置清单
10 个地图规则清单
Enemy/Boss ↔ Mechanic 多对多引用
Mechanic ↔ MapRule 多对多引用
字段归属与迁移报告
```

禁止：

```text
强制一对一映射
修改正式敌人资产
修改既有数值
启用 devOnly 配置
替换现有 BuildSandbox 配置
```

验收：

```text
孤儿 ID = 0
重复事实源 = 0
无法解析引用 = 0
现有条目数量不丢失
所有正式引用标记保持 false
```

## 七、E04 EncounterCompositionSchema01

Guard release（2026-07-14）：

```text
Package: V0.4-EncounterCompositionSchema01
Status: ASSIGNED / NOT_STARTED
Scope: DATA_SCHEMA_ONLY
Assignment: Docs/V0.4/EncounterCompositionSchema01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_ENCOUNTERCOMPOSITIONSCHEMA01
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
E05: NOT_RELEASED
```

Guard review（2026-07-14）：

```text
Developer result: DEV_COMPLETE / QA_PASS
Guard static review: PASS
Delivered files: 14
Modified existing files: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Offline same-source Verifier: 124/124 PASS
Fixture: 2 Encounter / 4 Wave / 6 Slot
Carrier reuse across Encounter: PASS
MechanicProfile reuse across Carrier: PASS
Unresolved references: 0
E01 + E02 + E03 protected hashes: 14/14 unchanged
Legacy source hashes: 3/3 unchanged
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
git diff --check: PASS
Unity batch compile / Verifier: BLOCKED / same project open in user Unity Editor
Unity batch PASS claim: none
Commit / tag / push: none
Guard receipt: GUARD_PASS_ENCOUNTERCOMPOSITIONSCHEMA01_WITH_RECORDED_UNITY_BATCH_BLOCK
E05: NOT_RELEASED
```

只做：

```text
EncounterId
EncounterWave
EnemySlot / BossSlot
MapRule 引用
MechanicProfile 引用
出场顺序数据
精英与 Boss 标记
开发章节内容标签
```

禁止：

```text
生成或控制 GameObject
加载场景
推进章节
开始战斗
结算胜负
发放奖励
```

验收：同一敌人可复用于多个 Encounter；同一机制可组合多个载体；3-10/4-10 只作为 devOnly 内容 ID，不进入运行时代码分支。

## 八、E05 EnemySkillBossPhaseSchema01

Guard release（2026-07-14）：

```text
Package: V0.4-EnemySkillBossPhaseSchema01
Status: ASSIGNED / NOT_STARTED
Scope: DATA_SCHEMA_ONLY
Assignment: Docs/V0.4/EnemySkillBossPhaseSchema01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_ENEMYSKILLBOSSPHASESCHEMA01
Expected new files: 16
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
E06: NOT_RELEASED
```

Guard 验收记录（2026-07-14）：

```text
Developer result: DEV_COMPLETE / QA_PASS
Guard static review: PASS
Delivered files: 16
Modified existing files: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Offline same-source Verifier: 134/134 PASS
Unity batch compile: PASS
Unity batch Verifier: 134/134 PASS
Fixture: 4 Pattern / 3 Sequence / 3 Binding / 3 Phase / 2 Plan
Pattern / Sequence / Phase reuse: PASS
Player-safe isolation: PASS
Unresolved references: 0
E01-E04 protected hashes: 18/18 unchanged
Legacy source hashes: 3/3 unchanged
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
git diff --check: PASS
HEAD unchanged: 1050830bfebdb92401343de4a0734bbe57561c63
Commit / tag / push: none
Guard receipt: GUARD_PASS_ENEMYSKILLBOSSPHASESCHEMA01
E06: NOT_RELEASED
```

Guard 结论：本包通过。技能、序列、载体绑定、Boss 阶段和阶段计划保持纯数据；未发现运行时执行、玩家完整答案泄漏或跨系统越界。E06 不自动启动。

只做：

```text
SkillPatternId
Intent 数据
Cast 数据
TargetCue 数据
SkillSequence 数据
BossPhaseId
阶段进入条件数据
阶段技能表引用
公开阶段名称与开发者诊断分层
```

禁止：

```text
计时器 Runtime
状态机 Runtime
目标选择执行
伤害执行
修改现有施法条或反馈 Controller
```

验收：技能与阶段数据可序列化、可验证、可复用，不包含场景对象引用。

## 九、E06 CounterWindowAndPressureSchema01

Guard release（2026-07-14）：

```text
Package: V0.4-CounterWindowAndPressureSchema01
Status: ASSIGNED / NOT_STARTED
Scope: DATA_SCHEMA_ONLY
Assignment: Docs/V0.4/CounterWindowAndPressureSchema01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_COUNTERWINDOWANDPRESSURESCHEMA01
Expected new files: 16
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
E07: NOT_RELEASED
```

Guard 验收记录（2026-07-14）：

```text
Developer result: DEV_COMPLETE / AUTO_QA_PASS
Guard static review: PASS
Delivered files: 16
Modified existing files: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Offline same-source Verifier: 296/296 PASS
Unity batch compile: PASS
Unity batch Verifier: 296/296 PASS
Fixture: 4 Pressure / 3 Window / 6 PressureSource / 4 WindowSource / 5 PressureWindow / 11 Requirement
Pressure reuse: PASS
CounterWindow reuse: PASS
Any / All requirement groups: PASS
Player-safe isolation: PASS
Unresolved references: 0
E01-E05 protected hashes: 23/23 unchanged
Legacy source hashes: 3/3 unchanged
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
git diff --check: PASS
HEAD unchanged: 1050830bfebdb92401343de4a0734bbe57561c63
Commit / tag / push: none
Guard receipt: GUARD_PASS_COUNTERWINDOWANDPRESSURESCHEMA01
E07: NOT_RELEASED
```

Guard 结论：本包通过。压力、能力要求组、窗口条件与三类绑定均保持纯数据；未发现窗口执行、Build/Item 读取、完整答案泄漏或跨系统越界。E07 不自动启动。

只做：

```text
BuildPressureProfile
BuildCapabilityRequirement
CounterWindowProfile
触发条件数据
持续时间数据
结束条件数据
公开反馈文本键
开发者阈值数据
```

首批窗口类型：

```text
虚弱
显形
打断
反震
破壳
清场后核心暴露
供能反制成功
```

禁止：直接开启战斗窗口、改敌人状态或把精确阈值暴露给玩家。

验收：玩家 Projection 不含阈值和完整要求；开发者 Snapshot 能保留完整诊断。

## 十、E07 BuildCapabilityReadContract01

Guard release（2026-07-14）：

```text
Package: V0.4-BuildCapabilityReadContract01
Status: ASSIGNED / NOT_STARTED
Scope: READ_CONTRACT_ONLY
Assignment: Docs/V0.4/BuildCapabilityReadContract01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_BUILDCAPABILITYREADCONTRACT01
Expected new files: 16
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
E08: NOT_RELEASED
```

Guard 验收记录（2026-07-14）：

```text
Developer result: DEV_COMPLETE / AUTO_QA_PASS
Guard static review: PASS
Delivered files: 16
Modified existing files: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Unity batch compile: PASS
Unity batch Verifier: 298/298 PASS
Same-source offline entry: PRESENT
Separate offline execution result in handoff: NOT_REPORTED
Fixture: 16 KnownKeys / 16 CompleteValues / 4 SparseValues / 6 SourceSummaries / 3 GapShapes / 5 Bands
Complete coverage: PASS
Sparse unknown preservation: PASS
Known Zero vs Unknown: PASS
Item-independent: PASS
Unresolved references: 0
E01-E06 protected hashes: 28/28 unchanged
Legacy source hashes: 3/3 unchanged
Leak / GUID conflicts / trailing whitespace: 0 / 0 / 0
git diff --check: PASS
HEAD unchanged: 1050830bfebdb92401343de4a0734bbe57561c63
Commit / tag / push: none
Guard receipt: GUARD_PASS_BUILDCAPABILITYREADCONTRACT01
Guard sync: TASK_STATUS_SYNC_TO_GUARD_REPOOPS_ACCEPTED
RepoOps status: REPOOPS_RECORD_DONE
E08: NOT_RELEASED
```

Guard 结论：本包通过。Contract 保留 Complete/Sparse、Known Zero/Unknown 和 Ordinal 查询语义；未发现能力计算、Item/Board/Battle 依赖、Readiness 生成、玩家投影或跨系统越界。Guard + RepoOps 同步已完成，E08 不自动启动。

只做：

```text
BuildCapabilitySnapshot 只读接口
BuildCapabilityValue
BuildCapabilitySourceSummary
ReadinessBand
CapabilityGap
schema/version 字段
```

禁止：

```text
引用 ItemSystemSnapshot 具体实现
编写 Item Adapter
读取棋盘 Controller
修改 Item 数据
判定真实 Item 技能触发
接 BattleContract
```

验收：Enemy Contract 可在没有 Item 程序集和没有场景的条件下独立构造与测试。

## 十一、E08 EnemyOfflineReadinessEvaluator01

Guard release（2026-07-14）：

```text
Package: V0.4-EnemyOfflineReadinessEvaluator01
Status: ASSIGNED / NOT_STARTED
Scope: PURE_EVALUATOR_ONLY
Assignment: Docs/V0.4/EnemyOfflineReadinessEvaluator01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_ENEMYOFFLINEREADINESSEVALUATOR01
Expected new files: 16
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
E09: NOT_RELEASED
```

只做：

```text
BuildCapabilitySnapshot
+ BuildPressureProfile
+ MapRule 数据修正
→ DeveloperReadinessSnapshot
+ PlayerHintProjection
```

禁止：

```text
战斗执行
伤害预测冒充正式结算
自动选择主 Build
自动替玩家解题
DropBias 正式生效
运行时接线
```

验收：纯函数、确定性、输入不可变；玩家结果无完整答案；开发者结果可解释能力缺口。

Guard review（2026-07-15）：

```text
Result: GUARD_RETURN_ENEMYOFFLINEREADINESSEVALUATOR01_BASELINEREPORT01
RepoOps status: REPOOPS_RECORD_DONE
Runtime semantics review: PASS
Player-safe isolation review: PASS
Package whitelist: 16/16 PASS
Scenario coverage: 15 PASS
Reported Verifier: 212/212 PASS
Blocking issue: baseline.e05.fixture Expected 4/3/3/3/2, Actual 4/5/4/3/6, Result PASS
Cause: Actual 使用 CSV 展开行数，PASS 使用逻辑实体去重数
```

返工包：

```text
Package: V0.4-EnemyOfflineReadinessEvaluator01-GuardFix01
Status: DEV_COMPLETE / QA_PASS / GUARD_REVIEWED
Scope: REPORT_BASELINE_COUNT_FIX_ONLY
Assignment: Docs/V0.4/EnemyOfflineReadinessEvaluator01_GuardFix01_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX01
Allowed modified files: 3
Allowed new files: 0
E09: NOT_RELEASED
```

GuardFix01 review（2026-07-15）：

```text
Verifier source: PASS
Report baseline: 4/3/3/3/2 PASS
Spec baseline: 4/3/3/3/2 PASS
LeakCheck baseline: stale actual 4/5/4/3/6
Cause: LeakCheck uses WriteUtf8IfMissing and was not regenerated
Result: GUARD_RETURN_ENEMYOFFLINEREADINESSEVALUATOR01_STALELEAKREPORT01
RepoOps status: REPOOPS_RECORD_DONE
```

第二次极小返工包：

```text
Package: V0.4-EnemyOfflineReadinessEvaluator01-GuardFix02
Status: DEV_COMPLETE / QA_PASS / GUARD_ACCEPTED
Scope: DERIVED_REPORT_REGENERATION_ONLY
Assignment: Docs/V0.4/EnemyOfflineReadinessEvaluator01_GuardFix02_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYOFFLINEREADINESSEVALUATOR01_GUARDFIX02
Allowed content modifications: 2
Allowed new files: 0
E09: NOT_RELEASED
```

GuardFix02 final review（2026-07-15）：

```text
Guard receipt: GUARD_PASS_ENEMYOFFLINEREADINESSEVALUATOR01
RepoOps status: REPOOPS_RECORD_DONE
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

Guard 结论：E08 正式通过。Evaluator 仍为纯离线数据层；未接 Battle、Board、Item、Scene、正式章节、奖励或存档。下一候选 E09 不自动启动。

## 十二、E09 EnemyDataValidatorAndSnapshot01

Guard release（2026-07-15）：

```text
Package: V0.4-EnemyDataValidatorAndSnapshot01
Status: ASSIGNED / NOT_STARTED
Scope: SYSTEM_SNAPSHOT_AND_CROSS_CATALOG_VALIDATION_ONLY
Assignment: Docs/V0.4/EnemyDataValidatorAndSnapshot01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_ENEMYDATAVALIDATORANDSNAPSHOT01
Expected new files: 20
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
Static embedded components: E01-E06
Transient contract-only components: E07-E08
E10: NOT_RELEASED
```

Guard static review（2026-07-15）：

```text
Developer result: DEV_COMPLETE / QA_PASS
Offline same-source Verifier: 173/173 PASS
Delivered files: 20/20
Modified existing files: 0
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
Protected hash: 38/38 unchanged
Legacy hash: 3/3 unchanged
Seven-report determinism: 7/7
Unity active Editor compile: PASS
Unity batch compile / Verifier: BLOCKED_BY_ACTIVE_EDITOR
Guard result: RETURNED_FOR_FIX
Guard receipt: GUARD_RETURN_ENEMYDATAVALIDATORANDSNAPSHOT01_VALIDATIONPROOFGAP01
```

返工原因：

```text
E02 CounterWindowType 被错误并入 E06 CounterWindow 实例身份空间
E03/E05/E06 developer/player 签名矩阵未逐项覆盖
E07/E08 transient stability 使用 Root 签名自比较，未构造内容变化
carrier-kind / isolation / component mismatch 与 immutability 证明不完整
Root property shape 未建立完整闭包
```

GuardFix01 release：

```text
Package: V0.4-EnemyDataValidatorAndSnapshot01-GuardFix01
Status: ASSIGNED / NOT_STARTED
Scope: E09_IDENTITY_AND_VALIDATION_PROOF_FIX_ONLY
Assignment: Docs/V0.4/EnemyDataValidatorAndSnapshot01_GuardFix01_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX01
Expected new files: 0
Allowed content modifications: 10 exact files
E01-E08 protected files: 38/38 must remain unchanged
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
E10: NOT_RELEASED
```

GuardFix01 static review（2026-07-16）：

```text
Developer result: DEV_COMPLETE / QA_PASS
Verifier: 215/215 PASS
Unity batch compile / Verifier: PASS / PASS
CounterWindow identity: 3 / E06 only PASS
RelationKind coverage: 17/17 PASS
Negative / immutability / Root closure: PASS
Guard result: RETURNED_FOR_FIX02
Guard receipt: GUARD_RETURN_ENEMYDATAVALIDATORANDSNAPSHOT01_E04SIGNATUREOMISSION01
```

唯一剩余阻塞：`EnemySystemSnapshot.ManifestRow` 对 E04 单独清空 `ComponentCanonicalSignature`，导致 Encounter quantity 等非关系编排变化无法改变 Full Root signature；Verifier 又把该错误行为写成 `stable / PASS`。这违反原 Assignment 的六静态组件 Full signature 覆盖要求。

GuardFix02 release：

```text
Package: V0.4-EnemyDataValidatorAndSnapshot01-GuardFix02
Status: ASSIGNED / NOT_STARTED
Scope: E04_FULL_ROOT_SIGNATURE_SENSITIVITY_ONLY
Assignment: Docs/V0.4/EnemyDataValidatorAndSnapshot01_GuardFix02_Assignment.md
Guard marker: ENEMY_GUARD_REWORK_ENEMYDATAVALIDATORANDSNAPSHOT01_GUARDFIX02
Expected new files: 0
Allowed content modifications: 9 exact files
E01-E08 protected files: 38/38 must remain unchanged
Other E09 frozen files: 11/11 must remain unchanged
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
E10: NOT_RELEASED
```

GuardFix02 final review（2026-07-16）：

```text
Guard receipt: GUARD_PASS_ENEMYDATAVALIDATORANDSNAPSHOT01
Developer result: DEV_COMPLETE / QA_PASS
Verifier: 220/220 PASS
Offline same-source Verifier: 220/220 PASS
Unity batch compile: PASS
Unity batch Verifier: 220/220 PASS
Static Manifest full signatures: 6/6 non-empty
E04 Catalog / Root Full sensitivity: PASS / PASS
E04 PlayerSafe / identity / relation stability: PASS / PASS / PASS
CounterWindow identity: 3 / E06 only
RelationKind coverage: 17/17
Seven-report determinism: 7/7
E01-E08 protected hash: 38/38 unchanged
Legacy hash: 3/3 unchanged
Other E09 frozen hash: 11/11 unchanged
Scene / Prefab / Config / Battle / Board / Item modifications: 0 / 0 / 0 / 0 / 0 / 0
RepoOps status: PENDING_EXTERNAL_SYNC
E10: NOT_RELEASED
```

Guard 结论：E09 正式通过。Full Root 现在无条件覆盖 E01-E06 六个静态组件签名；E04 Encounter 编排变化会改变 Full signature，但不会泄漏进 PlayerSafe。E07/E08 仍只登记 transient contract，不进入静态 Root 内容、Identity、Relation 或玩家投影。

只做：

```text
不可变 EnemySystemSnapshot.v1
ID 唯一性检查
引用完整性检查
多对多关系检查
Schema 版本检查
确定性签名检查
玩家字段答案泄漏检查
正式系统引用扫描
CSV / Markdown 报告
```

验收：

```text
Invariant violations = 0
Unresolved references = 0
Player answer leaks = 0
Formal flow references = 0
Scene / Prefab modifications = 0
```

## 十三、E10 DevEncounterSeedData01

Guard release（2026-07-16）：

```text
Package: V0.4-DevEncounterSeedData01
Status: ASSIGNED / NOT_STARTED
Scope: DEVONLY_SEED_DATA_ONLY
Assignment: Docs/V0.4/DevEncounterSeedData01_Assignment.md
Guard marker: ENEMY_GUARD_ASSIGNMENT_DEVENCOUNTERSEEDDATA01
Expected new files: 21
Modified existing files allowed: 0
Scene / Prefab / Config / Battle / Board / Item modifications allowed: 0 / 0 / 0 / 0 / 0 / 0
Post-E10 packages: FROZEN / NOT_RELEASED
```

本包固定表达四个验证 Encounter：`3-10 guard_wall`、`3-10 cleanse_corner`、`4-10 furnace_core`、`4-10 thunder_fire_cross`。每个 Encounter 为 ordinary / damping / Boss 三波纯数据；Skill、Pressure 与 CounterWindow 按机制复用，不按章节复制。

`furnace_core` 明确允许 MapRule 环境压力与偷灵/供能载体压力跨层叠加，不得强制每个 Slot mechanic 都与 MapRule 一对一绑定。E10 成为未来 Enemy 适配器的唯一内容种子；旧 BuildSandbox 只保留为迁移对照，不得成为 E10 Runtime 依赖。

Guard review（2026-07-16）：

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

原包的 21 文件白名单、4 Seed / 4 Encounter / 12 Wave / 12 Slot、E05/E06 计数、隔离、PlayerSafe、Legacy mapping、八报告确定性与跨系统 Leak 均通过静态复审。

Guard 未放行的原因有两项：第一，E10 Validator 只验证 capability/channel 的数量和权重，没有锁定 E03 -> E06 的 exact key fidelity；第二，`polluted -> cleanse` 与 `complex-eye -> energy` 的 Pressure/Window 关系没有共同 Source，且部分 CounterWindowSource 与窗口自身 OpenCondition 指向不同 Skill/Phase。当前关系虽然都能解析，但尚未形成未来 Adapter 可消费的闭合图。

GuardFix01 只允许修改 E10 Catalog、Validator、Verifier 与八份同源报告；不得修改 Snapshot shape、四套 Encounter、E05 内容、PlayerSafe、E01-E09、Legacy、Scene、Battle、Board 或 Item。

GuardFix01 final review（2026-07-16）：

```text
Package: V0.4-DevEncounterSeedData01-GuardFix01
Developer result: DEV_COMPLETE / QA_PASS
Guard result: GUARD_ACCEPTED / QA_PASS
Guard receipt: GUARD_PASS_DEVENCOUNTERSEEDDATA01
Verifier: 60/60 PASS
Offline same-source Verifier: 60/60 PASS
Unity batch compile: PASS
Unity batch Verifier: 60/60 PASS
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
Global git diff --check: PREEXISTING_UNRELATED_DIFF / BattleSandbox Scene only
Package-scope check: PASS
RepoOps status: REPOOPS_RECORD_DONE
Commit / tag / push: none
Post-E10 packages: FROZEN / NOT_RELEASED
```

Guard 结论：E10 正式通过。E03 是机制内容事实源，E06 只按 E03 精确投影 capability 与 pressure channel；39 条 PressureSource 和 20 条 CounterWindowSource 已形成可验证关系，13 条 PressureWindow 均有至少一个共同 `(SourceKind, SourceId)`。当前 Enemy 数据阶段队列完成，但没有释放 Runtime、Battle、Item Adapter 或正式流程接线。

只做：

```text
使用 E01-E09 Schema 表达 devOnly 3-10 / 4-10 内容
普通怪、阻尼关与 Boss 的 Encounter 组合数据
地图规则、机制压力与 CounterWindow 引用
开发者诊断种子
玩家提示文本键
离线校验报告
```

禁止：

```text
接正式章节入口
修改 BattleSandbox 场景
创建 PlayMode 场景接线
启用配置
写 RunFlow / SaveData / Reward
发正式掉落
推进正式章节
```

验收：3-10/4-10 数据完整可解析、保持 devOnly/disabled、可由未来适配器消费，但当前没有任何运行时入口。

## 十四、当前冻结的后置包

以下包不属于当前授权范围：

```text
MapRuleRuntime01
EnemyBossSkillPatternRuntime01
BossPhaseRuntime01
EnemyFeedbackProjectionIntegration01
BattleSandbox Enemy 接线
Item → BuildCapabilitySnapshot Adapter
BattleContract / Battle Bridge 接线
正式章节 Promote
正式 Reward / Drop / SaveData 接线
```

任一后置包都必须重新获得用户明确授权，并由对应 Guard 单独收口。

## 十五、执行规则

```text
一次只执行一个包
先有用户明确启动指令
再由 Enemy Guard 输出当前包 assignment
开发窗口只消费单包 assignment
不得顺手进入下一包
不得修改 AGENTS.md / Docs/LOCKED/*
不得 commit / tag / push
QA 通过后只同步 Guard + RepoOps
```

当前状态：

```text
V0.4-EnemyDomainDataContract01 已由 Guard 接受。
V0.4-EnemyMechanicVocabulary01 已由 Guard 接受。
V0.4-EnemyValidationContentNormalize01 已在 GuardFix01 后由 Guard 接受。
V0.4-EncounterCompositionSchema01 已由 Guard 接受；Unity Batch 占用阻塞已记录，不声称 Unity PASS。
V0.4-EnemySkillBossPhaseSchema01 已完成并由 Guard 接受；离线与 Unity Verifier 均为 134/134 PASS。
V0.4-CounterWindowAndPressureSchema01 已完成并由 Guard 接受；离线与 Unity Verifier 均为 296/296 PASS。
V0.4-BuildCapabilityReadContract01 已完成并由 Guard 接受；Unity Verifier 为 298/298 PASS，Guard + RepoOps 同步已完成。
V0.4-EnemyOfflineReadinessEvaluator01 已完成开发与 QA；Evaluator 语义和边界已通过 Guard 静态复审。
GuardFix01 已修正 E05 逻辑实体计数；GuardFix02 已修正五份派生报告的确定性再生成策略并刷新 LeakCheck。E08 已获 `GUARD_PASS_ENEMYOFFLINEREADINESSEVALUATOR01`。
E08 最终状态已由 RepoOps 记录；未执行 commit / tag / push。
V0.4-EnemyDataValidatorAndSnapshot01 已在 GuardFix02 后由 Guard 接受；Unity 与离线 Verifier 均为 220/220 PASS。
V0.4-DevEncounterSeedData01 已在 GuardFix01 后由 Guard 接受；离线与 Unity Verifier 均为 60/60 PASS。
E01-E10 数据、Contract、Schema、离线 Evaluator 与验证种子阶段已经收口完成；当前没有已释放的下一包。
所有后置 Runtime、Battle、Item Adapter 与正式流程包继续冻结，未释放。
```

## 十六、Shared Module Layering Queue Gate

所有后续 Enemy Runtime、Enemy UI、BattleSandbox 与 Unified 接线包必须读取：

```text
Docs/V0.4/SHARED_MODULE_LAYERING_AND_PREFAB_PRESENTATION_GUARD.md
```

Enemy Prefab 只读 Enemy/Combat ViewModel；Enemy 状态只有一个 Runtime Owner；Enemy
不得复制 Item/Build/供能真源。成熟 Enemy 视觉必须以 Base Prefab / Variant / Scene
Slot复用，不得每个场景重画。任务若越层或缺少可见手测节点，Enemy Guard 必须暂停并
向用户解释，不得仅返回技术枚举或哈希。
