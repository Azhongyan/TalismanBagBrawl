# Per-Actor Enemy Action Water Task Contract

## Outcome

在 `CAMPAIGN_NORMAL_LV1` 的现有正式战斗中，每个存活 Enemy Actor 独立拥有攻击行动；行动使用精确 Actor 身份、当前正式伤害/间隔、动作请求和玩家目标。Actor 被击败后只取消自己的待执行攻击，其他 Actor 继续行动。Battle 在真实扣除 Guard/HP 后发出同一精确来源 Cue，供后续装修直接消费。

## Mode / Class / Owners

- Task Mode: `水电`
- Task Class: `CONTAINED_ONE_GUARD`
- Primary Guard: Battle/Bridge
- Development Owner: 当前窗口（用户明确要求不派发、当前窗口自执行）
- Runtime State Owner: 现有 `C1FormalRealtimeBattleSession`
- Presentation Owner: 现有 Formal Battle Presentation（本包只交付 Cue，不写表现）
- Expected New Owner / Data Source / Fallback: `0 / 0 / 0`

## Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleSessionVerifier.cs`
- 本 Task Contract
- 仅在现有 Battle 合同无法承载精确来源时，最小修改 `C1FormalRealtimeBattleSessionContracts.cs`

## Forbidden Writes

- Scene、Prefab、Profile、Enemy 动画与装修表现
- Enemy/Stage 数值、Reward、Item、WorldMap、Save、Unity 序列化
- 第二套 Enemy Scheduler、Battle Session、Bridge 或数据源
- V02/BattleSandbox Runtime 回迁

## Required Behavior

1. Session 启动时，每个存活 Actor 创建一条自己的正式 Enemy attack action。
2. Action 保留精确 Actor、伤害、间隔、动作请求和玩家目标。
3. Action 到期时，仅来源 Actor 仍存活才提交真实 Guard/HP mutation，然后为同一 Actor 重新排程。
4. Actor 被击败时立即取消其未执行攻击；其他 Actor 的 due time 不被重置。
5. `EnemyAttackScheduled / Accepted / Hit / DamageFloat` 携带精确敌人来源与玩家目标；`PlayerHpChanged` 保持玩家状态事实并通过同一 application identity 串联，不由 Presentation 猜测攻击者。

## Verification

- 定向 Runtime / Editor 编译。
- 扩展现有 Formal Session verifier：三敌起始三条独立行动；首轮三条精确来源；击败一名后该来源不再产生攻击，其他来源继续。
- 本包最多声明 `INTERNAL_QA / DECOR_HANDOFF_READY`；没有正常玩家路线手测不得声明玩家完成。

## Stop Rule

- 若当前正式数据无法给出一个可执行的基本攻击请求，停止并命名第一个数据 Owner 缺口，不从旧 Sandbox 猜数值。
- 若实现需要修改 Scene/Prefab/Presentation，停止并交装修，不在水电包内代做。

## Development Result — 2026-08-17

- 状态：`IMPLEMENTED / UNITY_FOCUSED_VERIFY_PASS / INTERNAL_QA / DECOR_HANDOFF_READY / PLAYER_PATH_NOT_PROVEN`。
- 现有 `C1FormalRealtimeBattleSession` 在开战时为每个存活 Actor 建立独立 `EnemyBasicWave` Action；Action 的 `sourceId` 是精确 `actorBalanceId`，并保留自身稳定顺序、伤害、间隔、动作请求与 Player 目标。
- 旧 Attack Wave 的总伤害预算按稳定 Actor 顺序分配给各独立 Action；同一轮总伤害保持不变，不因敌人数从一名变为三名而放大三倍。Actor 后续重复行动保持自己的初始伤害份额。
- Action 到期前重新确认来源 Actor 仍存活；Actor 被击败时只取消该来源未执行的基本攻击与既有 BoneSwap 待执行动作，其他 Actor 的时间线继续。
- `EnemyAttackScheduled / EnemyAttackAccepted / HitAccepted / DamageFloatPayload` 使用同一精确敌人 `sourceId`、Player target 与 accepted application identity；真实 Guard/HP mutation 先发生，Cue 后发出。
- Unity focused verifier：`PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=3`。Unity 生命周期由本任务拥有，`CLOSED_CLEAN -> batch verify -> CLOSED_CLEAN`。
- 未修改 Scene、Prefab、Profile、Enemy/Stage 数值、Reward、Item、WorldMap、Save 或 Presentation。

## Decor Handoff

- 装修只读消费现有 Cue：通过 `cue.sourceId` 定位对应 Enemy Slot，通过 `cue.targetActorId=PLAYER` 表示受击目标。
- 不再使用 `cue.targetActorId` 或“第一个存活敌人”推断攻击者。
- 仅把同一来源的 `EnemyAttackScheduled / Accepted / Hit / DamageFloat` 串成预备、出手、命中、结果表现；禁止回写 Session、修改伤害或另建敌人调度器。
