# V0.4-ShougunuPhase1RuntimeAndActionContract01 Assignment

## 1. Package

```text
Package:
V0.4-ShougunuPhase1RuntimeAndActionContract01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_SHOUGUNUPHASE1RUNTIMEANDACTIONCONTRACT01

Primary owner:
Enemy Guard

Risk:
YELLOW / DEVONLY ENEMY RUNTIME CONTRACT / NO BATTLE EXECUTION
```

真实工程：

```text
F:\Porject\TalismanBagBrawl
```

本 Assignment 由 Enemy Guard 冻结。开发窗口必须完整读取真实工程 `AGENTS.md`、`Docs/LOCKED/*`、Enemy CurrentRules/Queue 与本文件后再开工。

## 2. 已接受前置

以下前置只读消费，不得重写：

- `V0.4-BoneAspectContentDesignLock01-GuardFix01`
- `V0.4-BoneAspectCarrierPresentationCatalog01`
- `V0.4-BoneAspectMechanicGapSurvey01-GuardFix01`
- `V0.4-BoneAspectBossArtMechanicLineageSurvey01`
- `V0.4-BoneAspectMechanicVocabularyExtension01`
  - `QA_PASS`
  - Offline / Unity Verifier：`53/53 PASS`
  - 七个扩展词汇已成为可只读复用的增量，不得重新打开或修改

用户已冻结：

- BA-D1：盲女只属于叙事/表现；没有 HP、不可选中、不是 Runtime Actor、不是 Damage Receiver、不是 Target Candidate、不是 Spawned Unit、不拥有保护状态、不是第二事实源，不产生跨目标伤害转移。
- BA-D2：`FALSE_CLONE`，但不属于本包。
- BA-D3 / BA-D4：继续 `HOLD / USER_DECISION_REQUIRED`，不得进入本包。

## 3. 目标

建立纯 C#、不可变、确定性的：

```text
ShougunuPhase1RuntimeAndActionContract.v1
```

本包只负责：

1. 守骨奴第一阶段 devOnly Runtime State Snapshot。
2. 护壳、露核、恢复、终止的纯 Reducer。
3. BasicAttack、Skill1、Skill2、Skill3 的 Enemy Action Pattern。
4. HP 百分比阈值技能队列和每四次有效伤害事件产生一次普攻债务。
5. 由权威结果驱动的 Hit / ShellHit / ShellBreak / CoreExpose / Recover / Defeated Cue。
6. 中立、synthetic 的 Battle command/result fixture。
7. Full 与 Presentation-safe Canonical Signature。

本包不把守骨奴接入真实 BattleSandbox，不修改 Battle 执行器，不读取 Item 快照，不绑定 Scene/UI/Visual。

## 4. 冻结 devOnly 数值

所有值仅用于当前守骨奴第一阶段 BattleSandbox 竖切，不是正式 Boss 数值：

```text
contentId = bone_aspect_boss_c1_bone_guard
phaseId = bone_aspect.phase.shougunu.phase1
devOnly = true
isEnabled = false
entersFormalFlow = false
runtimeBoundToBattle = false

maxHp = 980
initialHp = 980
shellLayerMax = 200
maxSequentialShellLayers = 7
initialShellLayerIndex = 1
coreExposeDurationTicks = 6000
ticksPerSecond = 1000
skill1RepairUnits = 30
basicDamageApplicationInterval = 4
```

七层语义：

- 第 1 层为初始护壳。
- 第 2 至第 7 层只能在前一层破壳、露核窗口结束并进入合法恢复后依次生成。
- 每次恢复把下一层护壳恢复到 `200`，HP 保持不变。
- 第 7 层破壳后不得创建第 8 层。
- 第 7 层后仍未击败时继续保持可受伤的 CoreExposed 事实，并显式记录 `recoveryAvailable=false`；不得偷偷续壳、补血或进入 Phase2。

被废弃的 `HP88 / shell2x14` 只属于早期 smoke-test 历史，不得出现在本包代码、Fixture 或报告的有效配置中。

## 5. Runtime State Schema

至少冻结以下不可变字段；允许使用项目既有命名风格调整 C# 属性大小写，但 Stable Key 与语义不得变化：

### 5.1 Identity

```text
schemaId
schemaVersion
enemyInstanceId
contentId
phaseId
devOnly
isEnabled
entersFormalFlow
resetGeneration
revision
canonicalSignature
errors
```

### 5.2 Lifecycle / combat state

```text
lifecycleState
maxHp
currentHp
maxSequentialShellLayers
currentShellLayerIndex
shellLayerMax
currentShell
shellActive
targetable
vulnerable
coreExposeActive
coreExposeStartTick
coreExposeEndTick
coreExposeRemainingTicks
recoveryAvailable
lastAcceptedBattleTick
lastAcceptedApplicationSequence
```

合法 lifecycle：

```text
Presence
LayeredShellOn
CoreExposed
Recovering
DefeatCandidate
Defeated
Invalid
```

状态真值：

- `Presence`：不可接受伤害；只允许进入初始 `LayeredShellOn`。
- `LayeredShellOn`：targetable=true；vulnerable=true；护壳存在。HP 是否受伤由 Battle 已批准的 applied delta 决定，本包不从 Item 数值猜测。
- `CoreExposed`：targetable=true；vulnerable=true；currentShell=0；窗口计时由 Battle Tick 驱动。
- `Recovering`：短暂权威转换态；先处理 Reactive Cue，再恢复下一层，不接受 scheduled action resolve。
- `DefeatCandidate`：仅用于终止顺序校验，不代表正式战斗胜利。
- `Defeated`：不可再接受伤害、动作或恢复。
- `Invalid`：畸形输入或终止顺序错误；不得降级成 Unknown、Hit 或 Defeated。

## 6. Battle-neutral application boundary

本包可以定义中立 DTO，但不得引用 Item、BuildSandbox 或 Battle 私有类型。

每个 synthetic Battle application 至少包含：

```text
applicationEventId
applicationSequence
battleTick
resetGeneration
enemyInstanceId
acceptedByBattleLedger
shellDamageApplied
hpDamageApplied
rejectionReason
```

规则：

1. Enemy 拥有 HP、护壳、targetable、vulnerable 与 reducer 合法性。
2. Battle 将来拥有时钟、目标选择、事件排序、应用账本及最终 applied delta。
3. Item 将来只能提交只读 effect request；本包不读取 ItemId、ItemSystemSnapshot、Build、Lighting 或 Projection。
4. 只有当前 generation、目标一致、顺序递增、未重复、`acceptedByBattleLedger=true` 且 `shellDamageApplied > 0 || hpDamageApplied > 0` 的事件，才是 accepted damage application。
5. duplicate、stale、wrong-generation、wrong-target、rejected、zero-delta 均不改变状态，不计 Basic debt，不触发 Hit。
6. applied delta 必须非负并符合当前状态；越界或畸形结果为 Invalid，不可静默 clamp 成合法业务结果。
7. Reducer 不推导 Item 的 damage/break 映射，不把名称、标签、动画或图片解释为伤害。

## 7. Shell / expose / recover reducer

冻结转换：

```text
Presence -> LayeredShellOn
LayeredShellOn + accepted shell damage -> ShellHit
LayeredShellOn + shell reaches 0 -> ShellBreak -> CoreExposed
CoreExposed + accepted HP damage -> Hit
CoreExposed + expose duration elapsed + layerIndex < 7 -> Recovering -> next LayeredShellOn
CoreExposed + expose duration elapsed + layerIndex == 7 -> CoreExposed(recoveryAvailable=false)
Any live state + legal terminal result -> DefeatCandidate -> Defeated
Reset -> new resetGeneration -> Presence -> initial LayeredShellOn
```

约束：

- ShellBreak 每层最多发出一次。
- CoreExpose start/end 使用 Battle Tick；Visual/动画完成回调不得推进状态。
- 恢复时只恢复下一层 shell=200，HP 不变，技能阈值历史不清除，普攻计数不清除。
- resetGeneration 变化时清除本 generation 的 dedupe、阈值触发、Pending Queue、Basic debt 与 action progress；全局 revision 仍严格递增。
- 不进入 Phase2/Phase3，不 summon，不 split。

## 8. Action Pattern

仅有四个 scheduled action：

| actionId | 展示名 | due 条件 | Enemy/Battle 语义 |
|---|---|---|---|
| `shougunu.phase1.basic_attack` | 普通攻击 | 每 4 个 accepted current-generation damage application 形成 1 个 Basic debt | 输出中立 `direct_player_damage` request；伤害值与玩家 HP 不在本包 |
| `shougunu.phase1.skill1.shell_repair` | Skill1 护壳修补 | HP <=90%、<=45% 各一次 | 仅在当前护壳已受损但未破时，于 Battle resolve tick 修补 +30、cap200；不复活护壳、不治疗 HP |
| `shougunu.phase1.skill2.rope_heavy_strike` | Skill2 骨绳重击 | HP <=75%、<=30% 各一次 | 输出 telegraphed rope heavy strike request；不得推导 bind、stun 或位移 |
| `shougunu.phase1.skill3.ground_seal_burst` | Skill3 地面封印爆发 | HP <=60%、<=15% 各一次 | 输出 telegraphed ground-seal area burst request；不得推导 hazard、DoT 或持续地面状态 |

Effect request key 只表达中立意图：

```text
battle.effect_request.direct_player_damage
enemy.effect_request.repair_current_shell
battle.effect_request.rope_heavy_strike
battle.effect_request.ground_seal_area_burst
```

除 Skill1 的护壳修补外，其余请求不含正式伤害值，不执行玩家 HP 变化。Battle 侧真实 effect contract 属于后续联合包。

### 8.1 Battle tick timing

```text
BasicAttack:
preCast=0, telegraph=400, cast=300, resolveOffset=700, recover=600

Skill1:
preCast=0, telegraph=800, cast=400, resolveOffset=1200, recover=800

Skill2:
preCast=0, telegraph=1200, cast=500, resolveOffset=1700, recover=900

Skill3:
preCast=0, telegraph=1800, cast=700, resolveOffset=2500, recover=1200

CoreExpose transition guard for BasicAttack:
600 ticks
```

以上为 devOnly fixture 输入，必须集中在 Action Pattern Catalog，不得散落为多个 magic number。它们不是 elapsed-time due 或 cooldown。

每个动作至少有：

```text
actionId
effectRequestKey
targetRequestKey
dueKind
thresholdOccurrenceId
preCastTicks
telegraphTicks
castTicks
resolveOffsetTicks
recoverTicks
stateLegality
interruptPolicy
priority
executionId
scheduledStartTick
authoritativeResolveTick
status
```

## 9. HP threshold queue

阈值固定为 basis points：

```text
S1-A = 9000
S2-A = 7500
S3-A = 6000
S1-B = 4500
S2-B = 3000
S3-B = 1500
```

交叉检测必须使用整数：

```text
hpBefore * 10000 > maxHp * thresholdBp
&&
hpAfter * 10000 <= maxHp * thresholdBp
```

规则：

1. 仅在 accepted Battle application 造成真实 HP delta 后检测。
2. 每个 occurrence 每个 resetGeneration 最多触发一次。
3. 单次 hit 跨多个阈值时，全部入队，按阈值从高到低；不得丢失、合并或重复。
4. 队列是 FIFO；已 Pending 的 threshold skill 优先于 Basic debt。
5. Skill1/2/3 在 `CoreExposed`、`ShellBreak` reactive transition 或 `Recovering` 时保持 Pending，到下一个合法点执行。
6. Skill1 还要求当前护壳 `0 < currentShell < shellLayerMax`；护壳满值时继续 Pending，护壳受损后才合法。
7. 阈值动作不由 elapsed time 变为 due；cooldown 字段应为 NotApplicable 或不存在。
8. 护壳循环不清除阈值队列或已触发集合。

## 10. Basic debt

```text
basicEarnedCount = floor(acceptedDamageApplicationCount / 4)
basicDebt = basicEarnedCount - basicResolvedCount
```

规则：

- acceptedDamageApplicationCount 跨 shell/expose/recover 持续累计。
- 第 4/8/12/... 次有效应用各产生一个 debt。
- Pending threshold skill 优先于 Basic debt。
- Basic 可在稳定 `LayeredShellOn` 执行；也可在稳定 `CoreExposed` 且 ShellBreak/CoreExpose reactive cue 已处理并经过 600 tick guard 后执行。
- 一个时间点只允许一个 scheduled action。
- debt 只在 Battle 接受对应 action resolve 时消耗；动画完成、展示开始或 request 生成均不得消耗。
- 50 个 accepted damage applications 的 nominal trace 必须产生并成功解析 12 次 BasicAttack。

## 11. Reactive cues

Reactive cue 不是 scheduled attack，不进入 HP threshold queue：

```text
Presence
ShellHit
ShellBreak
CoreExpose
Hit
Recover
Defeated
Reset
```

四个 scheduled action 可额外产生对应 action cue：

```text
BasicAttack
Skill1
Skill2
Skill3
```

每个 cue 至少包含：

```text
cueId
cueSequence
revision
resetGeneration
enemyInstanceId
cueKind
battleTick
sourceApplicationEventId
sourceExecutionId
longLivedStateAfterCue
```

约束：

- cueId / cueSequence / revision 必须单调、稳定、可去重。
- ShellHit 只来自 accepted shell delta。
- ShellBreak 只在该层首次到零时产生。
- Hit 只来自 accepted exposed HP delta。
- duplicate/stale/rejected/zero-delta 不产生 Hit/ShellHit。
- Visual 只能读取 cue/state；不能回写 HP、护壳、窗口、队列、动作或下一状态。

## 12. Defeat ordering

`currentHp == 0` 不能自动掩盖尚未解析的 mandatory threshold skill。

- Nominal 75 秒轨迹必须保证第二次 Skill3 在 Defeated 前 resolve。
- 如果某个输入导致 lethal result 同时跨过未触发阈值，Reducer 必须先记录阈值 crossing。
- 若 mandatory threshold action 尚未合法 resolve 而状态已要求 Defeated，返回：

```text
Invalid / TERMINAL_ORDERING_VIOLATION
```

- 不得把 HP 偷偷 clamp 为 1，不得跳过 Skill3，不得创建 death-throe 特例。
- 该 Invalid 用于暴露 tuning/Battle-contract 问题，不代表正式游戏规则。

## 13. Presentation-safe projection

本包提供只读 Presentation-safe Snapshot，允许后续 Visual Adapter 读取：

```text
enemyInstanceId
contentId
presentationKey
resetGeneration
revision
lifecycleState
maxHp
currentHp
currentShellLayerIndex
maxSequentialShellLayers
shellLayerMax
currentShell
targetable
vulnerable
coreExposeActive
coreExposeRemainingTicks
latestCue
errors
canonicalSignature
```

禁止进入 Presentation-safe：

- 未触发 HP 阈值及未来动作答案
- Pending threshold queue
- Basic debt 内部计数
- dedupe ledger
- developer diagnostics
- BA-D3 / BA-D4
- 任何盲女 Runtime 字段

Full Signature 必须对队列、去重、阈值、动作、调参字段敏感；Presentation-safe Signature 对纯 developer diagnostics 变化稳定，但对玩家可见状态/cue 变化敏感。

## 14. Nominal 75-second fixture

Fixture 只使用 synthetic neutral Battle results，不引用 Item ID。

冻结目标：

```text
accepted applications = 50
duration = 75 sec
accepted pulse magnitudes repeat = 29,42,53,46,55,76
shell breaks = 7
threshold skills = 6
basic attacks resolved = 12
defeated after Skill3-B resolve
```

基准观察点：

| 事件 | 预期时间 |
|---|---:|
| Break 1 | 7.5s |
| S1-A crossing | 9.0s |
| Break 2 | 18.0s |
| S2-A crossing | 22.5s |
| Break 3 | 28.5s |
| S3-A crossing | 31.5s |
| Break 4 | 39.0s |
| S1-B crossing | 42.0s |
| Break 5 | 51.0s |
| S2-B crossing | 54.0s |
| Break 6 / S3-B crossing | 63.0s |
| S3-B expected resolve | approximately 71.5s |
| Break 7 | 73.5s |
| Basic #12 legal resolve | after final break and 600-tick guard, before defeat |
| Defeat | approximately 75.0s |

连续 Battle-tick Verifier 必须证明动作时长、队列优先级和 transition guard 下仍满足以上顺序。不得只按 Item pulse 离散时刻验证，也不得让动画 completion 决定 resolve。

## 15. 精确写入白名单

本包只允许新增以下 23 个文件；修改已有文件必须为 0：

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimePrimitives.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeSnapshots.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionPatternCatalog.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeReducer.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1ActionScheduler.cs.meta
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeValidation.cs
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/ShougunuPhase1/ShougunuPhase1RuntimeValidation.cs.meta
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/ShougunuPhase1RuntimeAndActionContractVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/ShougunuPhase1RuntimeAndActionContractVerifier.cs.meta
Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractReport.md
Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractSpec.csv
Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractFieldMatrix.csv
Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionPattern.csv
Docs/V0.4/Reports/ShougunuPhase1ThresholdAndQueueFixtureRows.csv
Docs/V0.4/Reports/ShougunuPhase1NominalTrace.csv
Docs/V0.4/Reports/ShougunuPhase1NegativeFixtureRows.csv
Docs/V0.4/Reports/ShougunuPhase1RuntimeAndActionContractLeakCheckReport.md
```

不得由 Unity 自动导入认领白名单之外的 `.meta`。

## 16. Protected baseline

任务开始时以磁盘状态冻结，不以 Git HEAD 或过期全仓 hash 替代：

1. P0 8 文件。
2. P1 21 文件。
3. Gap Survey 8 文件。
4. Boss Art Lineage 9 文件。
5. Vocabulary Extension 17 文件。
6. `Assets/_Game/Scripts/TalismanBag/EnemySystem/**` 中除本包新增路径外的所有既有文件。
7. `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/**` 中除本包 verifier 新增路径外的所有既有文件。
8. `Assets/_Game/Scripts/TalismanBag/Items/**`
9. `Assets/_Game/Scripts/TalismanBag/BuildSandbox/**`
10. `Assets/_Game/Scripts/TalismanBag/CrossSystem/**`
11. Scene / Prefab / Config / BuildSettings / ProjectSettings / Packages。
12. Enemy Queue、Guard CurrentRules、AGENTS、Docs/LOCKED。

并行漂移：

- 如保护域发生变化，必须列出精确路径和 task-start/current hash。
- 不重定基线，不恢复，不格式化，不 claim 他包文件。
- 只有获得对应 Owner 精确归因后，才可记录 `EXTERNAL_CONCURRENT_DRIFT`；否则停在 Guard Review。

## 17. 禁止

- 不修改 Item、IF、Item Projection、Build、Lighting 或 I031。
- 不修改 Battle、Battle Bridge、Battle Resolver、Player HP 或应用账本。
- 不修改 BuildSandbox、Scene、Prefab、UI、RectTransform、Animator、VFX 或图片。
- 不修改 Carrier Presentation、E02 默认 Vocabulary 或已接受 Vocabulary Extension。
- 不绑定真实 Encounter/MapRule/Requirement/Readiness。
- 不实现 Phase2/Phase3、summon、split、盲女 Actor 或跨目标承伤。
- 不写 SaveData、Reward、Drop、Chapter、RunFlow 或正式 1-10/2-10。
- 不修改 Queue、Guard rules、AGENTS、Docs/LOCKED。
- 不运行 Builder，不保存 Scene/Prefab。
- 不 commit、tag、push、stage、reset、rollback、clean。
- 不自动启动 Battle Adapter、Visual Presenter、Vertical Slice 或 D2。

## 18. Verifier

唯一入口：

```text
TalismanBag.Editor.EnemySystem.ShougunuPhase1RuntimeAndActionContractVerifier.RunFromCommandLine
```

必须支持同源离线执行与 Unity batch 执行，输出同一组报告和 Canonical Signature。

最低 fixture：

1. 初始 980 HP / layer1 / shell200。
2. ShellHit、ShellBreak once、CoreExpose、6s、Recover next layer。
3. 七层上限、无第八层、HP 跨恢复保持。
4. 六个 HP 阈值各一次。
5. 单 hit 跨多阈值按 descending threshold 入 FIFO。
6. wrong generation / stale / duplicate / rejected / zero delta 不计数、不出 Hit。
7. 每四次有效应用产生 Basic debt；50 次得到 12 次 resolve。
8. threshold skill 优先 Basic。
9. CoreExpose/Recover 碰撞时 skill 保持 Pending。
10. Skill1 +30 cap200；满壳 Pending；破壳不复活；不治疗 HP。
11. Skill2 无 bind/stun/位移事实。
12. Skill3 无 hazard/DoT/持续地面事实。
13. elapsed-time due/cooldown 为 0 / NotApplicable。
14. action resolve 只由 Battle Tick/result 驱动；Visual callback 调用数为 0。
15. ShellHit/Hit/ShellBreak cue 只来自 accepted delta。
16. resetGeneration 清本地状态且 revision 继续单调。
17. immutable input/output、defensive copy、Ordinal 排序、InvariantCulture。
18. Canonical 重复生产、输入反序、字段敏感性。
19. Full 与 Presentation-safe 隔离。
20. blind woman Runtime rows = 0。
21. nominal 50-event / 75s 连续 tick trace。
22. lethal-before-mandatory-skill 为 `TERMINAL_ORDERING_VIOLATION`。
23. Scene/Prefab/Battle/Item/BuildSandbox/FormalFlow 引用均为 0。

报告必须可重复生成；连续两次 hash 一致，并覆盖 missing-report regeneration。

## 19. 验收

```text
新增文件 = 23
修改已有文件 = 0
Scene / Prefab / Config / Battle / Board / Item 修改 = 0
Runtime dependency leak = 0
Formal flow references = 0
Blind woman runtime rows = 0
Unresolved references = 0
GUID conflicts = 0
Package trailing whitespace = 0
Package-scope diff check = PASS
Protected baseline = unchanged 或精确归因的 external concurrent drift
Offline same-source compile = PASS / 0 errors / 0 warnings
Offline verifier = PASS
Unity batch compile = PASS
Unity batch verifier = PASS
Report determinism = PASS
Missing-report regeneration = PASS
```

Unity 使用必须串行：

- 开跑前检查 Unity/Tuanjie/ShaderCompiler/bee 与 `Temp/UnityLockfile`。
- 被用户或其他包占用时进入 `HOLD_UNTIL_SERIALIZED_CLEAN`，不得关闭进程、删除别人的锁或抢跑。
- 只允许本包一次正常 QA 生命周期；包拥有的进程必须自行退出。
- 无场景手测项。

## 20. 完成与停止

完成后直接同步 Enemy Guard + RepoOps：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-ShougunuPhase1RuntimeAndActionContract01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_SHOUGUNUPHASE1RUNTIMEANDACTIONCONTRACT01
```

同步必须列出：

- 23 文件清单与新增/修改数量
- Verifier 计数
- 六阈值、12 Basic、七壳、75 秒轨迹结果
- Canonical Signature
- Protected baseline
- Unity/process/lock 最终状态
- 未执行 Git

到达同步点后停止。不得自动开始 Battle、Visual、Vertical Slice、D2 或下一包。
