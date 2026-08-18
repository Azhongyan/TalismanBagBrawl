# I031 Formal Rule and Load Envelope Design

状态：`DESIGN_COMPLETE / RUNTIME_IMPLEMENTED / STATIC_AUDIT_PASS / UNITY_QA_NOT_RUN / USER_HANDTEST_FAIL`

日期：2026-08-12

## 1. 玩家问题

真实玩家路径已经证明奖励领取、Tray 到 Board 摆放和下一关推进可用，但进入 1-3 后，三个已摆放并点亮的普通道具很快耗尽念力，随后长时间没有伤害，战斗无法继续。

这不是 I004、I007、I003 或 1-3 的单点异常。当前正式 I031 只有 27 点初始/上限念力，并以每 500 ms 生成 1 点的速度持续供给，即 2 念/秒。30 件 Canonical 普通道具的凡品单件持续负载中位数已经约为 2.5 念/秒，因此基础供给连一件典型凡品都不能持续承载，27 点开局资源只会把断供延迟数秒。

本设计禁止通过 1-3 特判、单个 Item ID 分支、临时免费触发或把非伤害效果改成伤害来绕过该问题。

## 2. 全库负载事实

统一使用 I001-I030 Canonical `nianCost` 与 `cooldown`。正式冷却秒数为 `cooldownUnits * 0.4`，单件持续负载为：

`loadPerSecond = nianCost / (cooldownUnits * 0.4)`

30 件凡品统计：

- 单件：P50 2.50，P90 3.21，P95 3.50，最大 3.75 念/秒。
- 三件：P50 8.00，P90 9.11，P95 9.39，最大 11.25 念/秒。
- 五件：P50 13.33，P90 14.74，P95 15.13，最大 18.75 念/秒。
- 当前 I031：2 念/秒，只覆盖三件典型负载约 25%，覆盖五件典型负载约 15%。

以上是全 30 件 Canonical 道具的离散负载包络，不是针对当前三件道具反推的临时数值。

## 3. 正式玩家规则

I031 是特殊供念基础设施，不进入 I001-I030 普通掉落数据库，也不是另一套普通 Item 数据库。普通道具的属性、耗念和冷却仍只来自唯一 Canonical Item 数据库。

正式规则如下：

1. 只有 `placed + lit` 的普通 ItemInstance 加入战斗负载；Tray 或 unlit 实例不耗念、不排队、不触发。
2. 每个 ItemInstance 使用自己的 Canonical `nianCost`、`cooldown`、trigger、condition、target 和 effect，不按 Item ID 写分支。
3. Item 冷却完成后进入 Ready 状态。念力不足时保持 Ready，不得重新等待一整轮冷却。
4. Ready 队列按最早 Ready 时间和轮转公平顺序处理；稳定 ID 只作为同刻最终排序，不得成为永久优先级。
5. 队首 Item 念力不足时保留其资格，并在下一次生成、返念或回念后立即重新调度；成功触发后才从真实触发时刻开始下一轮冷却。
6. 只有 Mutation Preflight 确认本次效果可提交时才扣念；`NoMutation` 不扣念，也不得只发 Cue 冒充执行。
7. 实际扣念后必须由同一个 sourceItemInstanceId 提交到真实 State Sink：HP/Shell、Guard、Player HP、Nian、Enemy Action、Status 或其他已定义状态，然后再发 Cue。
8. 超载可以降低整体触发频率，但不得让固定排序中的某件道具永久饥饿；增加一个有效 Item 后，整体真实贡献不得变成零或下降为无输出。
9. 战斗结束后不保存当前念力、Ready 队列或中途调度状态。

## 4. I031 Lv1 正式候选包络

I031 Lv1 的职责是支持当前正常基础 Build，而不是靠 Nian 类掉落来修复基础不可运行。

候选值：

- `baseGenerationPerSecond = 20`
- `generationAmount = 10`
- `generationIntervalMilliseconds = 500`
- `maxNian = 50`
- `initialNian = maxNian`

理由：

- 20 念/秒覆盖任意五件凡品组合的最坏持续负载 18.75 念/秒。
- 五件凡品最坏同拍消耗为 45，50 点容量可以承受一次完整高耗同拍，并保留最小缓冲。
- 三件典型负载约 8 念/秒，不再依靠开局库存勉强维持。
- 五件高品阶道具最大负载可到约 25 念/秒，仍会形成真实超载，因此后续 I031 成长、Build 和 Nian Modifier 有存在价值。

Lv1 数值不是 1-3 专属值。它由“支持五件凡品基础 Build”这一长期产品边界推导；以后 Board 容量、品阶或章节提高时，通过 I031 成长和 Modifier 提高负载上限，不再改写基础运行语义。

## 5. 统一 Nian Economy Modifier

I031 自身只保留容量、基础生成和开局念力。其他系统不得直接写 `currentNian` 或复制一套资源引擎，只能向 Item Authority 提交标准 Modifier，由 Authority 投影一份不可变 Nian Economy Fact 给 Live Session。

标准操作只有五类：

- `MAX_NIAN_ADD`：增容。
- `GENERATION_RATE_ADD/MULTIPLY`：蓄念，提高持续供给。
- `RESTORE_ON_EVENT`：回念，在已定义事件发生时恢复资源。
- `REFUND_AFTER_ACCEPTED_MUTATION`：返念，仅在真实 Mutation 成功后返还。
- `COST_REDUCTION`：节念，按明确 scope 调整某实例/效果族的实际花费。

最终 `initialNian` 默认等于应用 Modifier 后的 `maxNian`，避免额外维护一条独立开局成长线。Item、Build、缺、Enemy/Boss 都只能成为 Modifier 来源，不拥有第二套 Nian 状态。

## 6. 正式技术边界

- Base profile owner：一个独立的 I031 Nian Economy Config。它是特殊基础设施配置，不复制普通 Item 数据库。
- Projection owner：现有 Item Authority 解析 Base Profile 和 Modifier，生成唯一 Nian Economy Fact。
- Runtime owner：现有 `C1FormalRealtimeBattleSession` 持有 `currentNian`、Ready Queue、spend/refund/generation ledger 和真实调度。
- Item data owner：现有 I001-I030 Canonical 数据库继续提供每个实例的 cost/cooldown/effect。
- Presentation：只读取 Nian snapshot/cue；不得决定能否执行。
- Save：当前切片仍只保存已通关 Stage ID。

旧 BattleSandbox Nian Source/Engine 和旧 I031 验证器不得重新接回正式线路；它们保持隔离，直到正式 focused verification 接管后再退休。

## 7. 实现包停止条件

本 Runtime 包只修改 I031 Base Profile/正式投影、现有 Live Session 的 Ready 调度，以及对应 focused verification。不得同时调整 Enemy、Reward、Board、普通 Item 数据、Scene/Prefab、Presentation 或 Save。

完成证据必须覆盖：

1. 全 30 件 Canonical 凡品负载读取无 Item ID 特判。
2. Tray/unlit 零消耗。
3. 当前 I007 + I007 + I003 三件 Build 在持续战斗中不发生资源性停摆。
4. 五件最坏凡品组合持续可运行，且最坏同拍不因容量不足丢失触发。
5. 一个超过 20 念/秒的高负载组合进入可见、公平超载；每个 eligible 实例都能在有界时间内执行，不永久饥饿。
6. `NoMutation` 不扣念；真实 mutation 先发生、同源 cue 后发生。
7. 正常 WorldMap 路径进入 1-3 后，玩家不再遇到“开局几秒后所有道具停止输出”。

若以上任一项需要第二套 Authority、第二套 Battle Session、单 Item 特判或临时敌人数值补丁，立即停止并重新评估，不继续叠补丁。

当前 focused verification 会从真实 Catalog 动态选择最大持续负载 Profile 与最大单次耗念 Profile；I031 证明不固定引用 I004 或其他普通道具身份。
