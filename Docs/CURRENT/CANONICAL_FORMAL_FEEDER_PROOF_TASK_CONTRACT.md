# Canonical Formal Feeder Proof Task Contract

## Current Window Work-Line Rule

- 本任务窗口固定为 `WATER_CORE_LINE`（水电线），不承担装修线任务。
- 当前唯一任务是纠正现有验证器的玩家生命周期顺序，并证明：实际奖励 `ItemInstance` → 自身 Canonical `combatEffect` → `MutationPreflight = committable` → 同一实例 `ItemTriggerAccepted` → 对应真实 State Sink 变化。
- 当前允许的产品外写入仅限本合同 `Allowed Writes` 中的现有验证器与治理文档；不得提前修改 Runtime 产品代码。
- 禁止在本窗口处理 UI 布局、字号、颜色、图片、Icon、动画手感、VFX 造型、Item Detail 样式或其他 `VISUAL_TASK`。
- 禁止修改 Scene、Prefab、ScriptableObject 或 Presentation 资产；以后只有用户明确授权新的水电接线任务时，才可重新定义这些写域。
- 若新请求属于装修线，当前窗口必须标记 `VISUAL_TASK / STOP_FOR_SEPARATE_OWNER`，不得在本合同内实施或混写。
- 只有验证器按正式玩家顺序运行并命名第一个真实 `CODE_GAP` 后，才能重新评估 Runtime 写域；不得为了测试通过改变道具语义、伪造胜利、固定道具 ID 或把非伤害效果解释成伤害。
- 当前窗口的只读审计状态称为 `ROOM_INSPECTION`；一旦写入验证器即属于水电施工。两种状态必须在每次动作中明确，不得以“检查”为名顺手修复。

## Outcome

证明正式 Campaign Reward 实际生成的任意普通 Canonical Item，是否以同一 `itemInstanceId`、同一 Canonical Definition 与同一 rolled Battle stats，依次经过 Item Authority、placed+lit 累计快照、Mainline Bridge、现有 Battle Adapter，并按它自己的 `combatEffect` 提交到真实 State Sink。

本任务只定位正式 feeder 的第一个真实断点，不修复产品代码。

## Product Context / Class

- Product Context: `CAMPAIGN_NORMAL_LV1`
- Task Class: `CONTAINED_ONE_GUARD`
- Primary / Development Owner: 当前单任务 Codex 窗口
- User Review Gate: `NOT_REQUIRED`（只做既有语义的运行证明）

## Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1FormalObtainRebuildBattleLoopTests.cs`
- `Docs/CURRENT/CANONICAL_FORMAL_FEEDER_PROOF_TASK_CONTRACT.md`
- `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md`

## Forbidden Writes

- Reward、Item Authority、Mainline、Battle Adapter、Battle Session 的 Runtime 实现
- Scene、Prefab、Presentation、VFX、Save、Enemy、StageConfig、Canonical Item 数值
- 新 Battle/Test Framework、新 Adapter/Bridge、旧 Sandbox/Pool15 删除、Cleanse、NoMutation 修复
- Git/worktree、哈希/SHA、批量报告或额外兼容层

## Required Proof

1. Reward 读取算法实际生成的普通 ItemInstance，不指定 `baseItemId`。
2. Authority 保存同一 `itemInstanceId`、generated instance 与 Canonical Definition。
3. 1-1 领奖后的 Prepare 先确认同一实例仍为 Tray/unlit，再由玩家语义摆放并点亮；1-2 快照必须携带同一实例与 stats，且不要求伪造完整 1-2 胜利。
4. Mainline 现有累计入口调用现有 Adapter 与 Session，不手工构造 Battle Item Fact。
5. 读取该实例自己的 Canonical `combatEffect`，满足其正常 trigger/condition 后，按 mutation kind 验证真实 State Sink：damage→HP/Shell、guard→Guard、heal→Player HP、refund→Nian、control→Action progress、status→BattleStatusState。
6. 只有 State Sink 真实改变后，同一 `sourceItemInstanceId` 的 `ItemTriggerAccepted` 才能作为成功证据；禁止把非伤害效果临时解释成伤害。

## Verification / Stop Rule

- 复用既有 `RunCumulativeItemInstanceRealOwnerChainOrThrow`，只增加独立 batch 入口与证据输出。
- Editor 定向离线编译。
- 无用户 Unity 时运行一次任务自有隐藏 Unity batch，记录 PID、日志与退出。
- 结果分类：`PASS / CODE_GAP / DATA_GAP / LIFECYCLE_GAP`。
- 一旦命名第一个断点，更新本合同与 Component Ledger，然后 `STOP`；不得在本任务顺手修复。

## Result — 2026-08-10

- 初次运行在 Reward 前被旧验证入口的 `FIXED_BASELINE_UNAVAILABLE` 拦截。验证器已按正式 `C1FormalObtainRebuildBattleLoop.TryCreate` 的现行方式加载 Canonical Resolver；没有恢复固定 I001，也没有修改 Runtime。
- 修正后 Authority 可由唯一 Canonical Catalog 建立，但既有自动 owner-chain runner 在产生 Reward 前无法满足 `accepted real-time victory 1-1`，因此没有进入 I004 Reward、Authority、placed+lit、Adapter 或 Session feeder 检查。
- 分类：`LIFECYCLE_GAP / QA_HARNESS_GAP`。该离线自动战斗夹具仍假定旧初始获取/战斗结果，不能代表当前正常玩家路线；用户已有“可胜利、可领奖、可摆放、可进入下一关”的更强正常路径证据，所以本结果不归类为 Reward、Authority 或 Battle feeder 的产品缺口。
- 已验证事实：Editor 定向离线编译 `0` 错误；任务 Unity batch PID `29424` 已退出且无残留进程。
- 未验证事实：真实 I004 在正式 Host 生命周期中从当前 Authority 进入 Adapter/Session 时的 rolled stats 与 mutation；本任务不得把未到达的检查写成 PASS。
- 终态：`FEEDER_PROOF_BLOCKED_BEFORE_REWARD / LIFECYCLE_GAP / ONE_HARNESS_CORRECTION_USED / NO_PRODUCT_FIX / STOP / NOT_READY`。

## Result — 2026-08-12 Generic Reward Instance Revision

- 已移除 focused verifier 对 `I004`、`Pool15` 名称和 `Golden Path` 术语的固定依赖；Reward 现在跟随算法实际生成的普通 Canonical ItemInstance。
- 验证器已读取实际奖励实例自己的 `combatEffect`，并按 HP/Shell、Guard、Player HP、Nian、Action progress、BattleStatusState 分类真实 sink；Cleanse、Targeting 等当前不能直接证明的类型会明确停止，不会伪装成伤害。
- Runtime 与 Editor 定向编译均为 `0` 错误；仅有两条既有未使用字段 warning。
- focused batch PID `6828` 正常退出。首个断点发生在 State Sink 之前：旧 runner 把 1-1 奖励保留在 Tray，要求先以旧 Build 打赢 1-2，之后才放置奖励进入 1-3；这与正式玩家承诺“1-1 领奖并重排后进入下一关”顺序不一致，且当前 runner 无法取得 `accepted real-time victory 1-2`。
- 已证明：实际普通 Reward 已生成、同一实例进入 Authority、1-2 Battle 启动入口可接受累计快照。未证明：该奖励在 placed+lit 后的 trigger、State Sink mutation 与同源 cue。
- 分类：`QA_HARNESS_LIFECYCLE_GAP`，不是本轮的 Item Runtime、Battle State Sink 或数值结论。不得通过加强伤害、伪造胜利或固定道具来绕过。
- 终态：`GENERIC_REWARD_PROOF_STOPPED_BEFORE_PLACED_LIT / QA_HARNESS_LIFECYCLE_GAP / NO_PRODUCT_FIX / STOP / NOT_READY`。

## Result — 2026-08-12 Next-Battle Lifecycle Correction

- 验证器已按正式玩家顺序改为：1-1 胜利并领取算法实际奖励 → 在同一 Prepare 中确认 Tray/unlit 身份 → 摆放并点亮同一实例 → 以最新累计快照进入 1-2 Live Session。旧的“奖励留在 Tray 打完整 1-2，胜利后才放进 1-3”路径已经移除。
- focused proof 不再要求完整 1-2 胜利，只推进到该奖励首次可提交的正式效果与真实 State Sink；没有加强伤害、伪造胜利、固定道具或修改产品语义。
- Runtime 与 Editor 定向离线编译均为 `0` 错误；仅保留两条既有 Runtime 未使用字段 warning。
- 任务自有 Unity batch PID `17256` 在 clean lease 下运行并自然退出。运行已进入 placed+lit 的 1-2 Session，但在验证器认定目标效果已接受之前，于 `ProcessStatusTick → ScheduleStatusAction → AddAction` 抛出 `EVENT_BUFFER_EXCEEDED`。
- 只读归因确认当前 verifier 要求 `ItemTriggerAccepted.effectVariantId == Canonical EffectId`；现有 Session 的基础 `ItemTriggerAccepted` 保留同一 ItemInstance/BaseItem 与 application identity，但该字段为空，Status 的效果身份与真实状态变化由 `StatusApplied / StatusRefreshed` 和 `BattleStatusState` contribution 承载。验证器因此可能已经越过真实接受事件仍继续推进，最终撞到事件总量上限。
- 所以本轮不能把 `EVENT_BUFFER_EXCEEDED` 判定成产品状态调度泄漏，也不能声称 State Sink PASS。第一个可确定断点是 verifier 对现有正式 Cue／Status 合同的识别条件过窄。
- 分类：`QA_HARNESS_CUE_IDENTITY_GAP / EVENT_BUFFER_EXCEEDED_IS_DOWNSTREAM_SYMPTOM / NO_RUNTIME_FIX / STOP / NOT_READY`。
- 下一独立动作只能评审并最小纠正 verifier：用同一 `sourceItemInstanceId + sourceBaseItemId + acceptedApplicationEventId` 的基础 `ItemTriggerAccepted` 证明 action commit，再由该实例自己的 Canonical plan 与对应真实 sink／status contribution 证明效果；不得要求所有效果族把 EffectId 填进同一个 Cue 字段，也不得修改 Runtime 来迁就测试。

## Result — 2026-08-12 Cue Identity Correction

- 现有 focused verifier 已改为用同一 `sourceItemInstanceId + sourceBaseItemId + acceptedApplicationEventId` 的基础 `ItemTriggerAccepted` 证明实际奖励动作已提交，不再要求基础 Cue 的 `effectVariantId` 承载所有效果族身份。
- 效果证明仍读取算法实际奖励实例自己的 Canonical plan，并按对应真实 State Sink 判断；没有固定 Item ID、没有把 Guard 等非伤害效果改写成伤害，也没有修改 Runtime 来迁就测试。
- 第一次 focused run 已越过旧 `EVENT_BUFFER_EXCEEDED`，抽到 `I013 / signature_i013 / PlayerGuardFlat`；它暴露出 verifier 对 Guard 仍残留一层额外 effect-specific Cue 要求。该要求与正式合同不一致，已在同一 verifier 修正为“同实例 action commit + Guard 真实增加”。
- 第二次任务自有 Unity batch PID `21804` 在 clean lease 下编译并自然退出，结果：`PASS assertions=72`。证据为同一算法奖励 `I013`、同一 ItemInstance、placed+lit、进入 `1-2`、`mutationKind=PlayerGuardFlat`、真实 `stateSink=PLAYER_GUARD`，且基础 action commit 的 cue source 与奖励实例一致。
- Unity 重建清理后的真实编译图成功，日志无 C# 编译错误。Runtime、Battle Session、数值、Scene、Prefab、ScriptableObject、Presentation、Reward、Authority 与 Save 均未修改。
- 分类：`PASS / CANONICAL_FORMAL_FEEDER_INTERNAL_QA_PASS / NO_RUNTIME_FIX / STOP`。这证明正式 feeder 与代表性实际奖励 State Sink 在内部 focused path 可闭合，但不是玩家正常路线手测，也不是 1-1..1-5、全部效果族或表现层完成声明。
- 下一候选动作：正常玩家路线手测同一语义（1-1 领奖 → 摆放并点亮 → 1-2 看见该道具自身效果的真实状态变化），必须独立授权后执行。

## User Handtest — 2026-08-12

- 用户正常路线回报：1-1 奖励为“离火符”；用户同时回报 1-2 奖励为“震雷破壳印”，但当前 1-2 未打通，直接体验判断为敌人血量过厚且早期曲线尚未调整。两句的具体先后关系本合同不推测，保留为用户原始证据。
- 该结果证明奖励不固定为内部 focused run 的 I013；但用户没有回报离火符对应的真实状态变化，因此本轮不能把玩家路线 mutation 标为 PASS。
- 当前玩家首阻塞：`CAMPAIGN_NORMAL_LV1_STAGE_1_2_EARLY_CURVE_BLOCKER`。这不是重新打开 Cue identity 或 feeder 修复的证据，也不授权直接削减单个敌人或加强单个道具。
- 分类：`USER_HANDTEST_BLOCKED_BY_EARLY_BALANCE / MUTATION_VISIBILITY_UNPROVEN / NOT_READY / STOP`。
- 下一候选任务：只读 `CANONICAL_LV1_POWER_BAND_AND_EARLY_STAGE_CURVE_AUDIT`，统一核对 1-1～1-5 的正式敌人 HP／Shell／伤害／行动节奏与合法早期 Build 输出，再提出有边界的数值范围；未经独立授权不修改数值。
