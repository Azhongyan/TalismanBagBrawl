# POOL15_SESSION_CONTRACT_RETIREMENT

Status: ACTIVE / CONSUMER_GRAPH
Classification: COMPLEX_GUARDED_ONCE
Task mode: 水电 + 历史消费者闭账
Product context: CAMPAIGN_NORMAL_LV1

## Outcome

Formal Battle 只接收累计 `ItemInstance + CanonicalItemDefinition` 战斗事实；删除已经没有正式 feeder 的 Pool15 Session envelope、事件 API、旧效果 foundation 与旧 QA。当前 Canonical Item effect operator、真实 Battle mutation、Reward、Tray/Board 和表现事实保持不变。

## Current truth owner

- `CanonicalItemCatalog` / `CanonicalItemDefinition`: 唯一道具定义与效果语义。
- `C1FormalItemSessionSnapshot`: 当前 Run 的精确 ItemInstance、Tray/Board 与 placed+lit 真相。
- `C1FormalRealtimeBattleSessionAdapter`: 从累计 Item 快照投影当前 direct Item facts。
- `C1FormalRealtimeBattleSession`: 唯一实时 Battle 状态与 Canonical effect mutation Owner。

Pool15 Carrier、BaseCombatFacts、Candidate Profile、Event Request/Result 和独立 application ledger 不再有资格承担正式真相或 fallback。

## Required work

1. 证明生产调用只走累计 Item snapshot；移除 Adapter 的 Pool15 snapshot overload 与显式 Pool15 event API。
2. 从 Battle request/refresh/state contract 删除 Pool15 envelope、计数与独立 event/result 类型；同时移除该合同内的历史 fingerprint/SHA 工具。
3. 从 Session 删除只服务旧 Pool15 envelope 的字段、自动触发、事件执行、pending action、ledger 与映射函数；保留现行 Canonical effect action 与 post-commit Cue。
4. 从 Item Authority 的 ordinary instance/row 删除永远为空的旧 combatFact 回链和死 hydration helper；继续保留 generated instance 与 canonical definition。
5. 将仍有价值的 current focused QA 保留并改接当前合同；删除旧 Pool15/旧 Enemy 数据专用测试体、Foundation、Carrier、BaseFacts 以及达到零消费者的 `.meta`。
6. 完成后全工程扫描，只报告仍有真实消费者的命名债务或独立历史岛。

## Protected behavior

- 31 件 Canonical Item 的 rolled stats、trigger、placed+lit、念力、冷却和真实效果。
- Enemy HP/Shell/攻击/技能/状态与 Stage 组成。
- Reward、ItemInstance identity、Tray/Board、关卡推进、Save。
- Battle Feedback facts、Presentation、Scene/Prefab/HUD/VFX、Item Detail。

## Forbidden

- 新建第二 Session、Battle、Adapter、Catalog、Manager 或兼容 fallback。
- 按 Item ID 补分支；把旧 Pool15 行为重新移植为第二套 Canonical operator。
- 修改产品数值或触发语义。
- 用删除旧代码掩盖当前 Canonical effect 缺口。
- hash/SHA、Git/worktree、用户 Unity 控制。

## Verification

- 生产 Runtime 的 Pool15 Session/Contract/API 与旧 combatFact 回链为 0。
- 当前 Formal Adapter 仍从累计快照生成同一精确 ItemInstance 的 direct facts。
- 当前 Canonical effect、Enemy skill、per-actor action 与 feedback focused proof 可运行时必须通过。
- Unity 脚本编译通过；若任务 Unity 无法正常启动，明确记录为未运行，不伪造结果。
- Fresh Play 不属于本技术清理包，不声明玩家交付。

## STOP

- 若某旧文件仍被当前玩家路径真实消费，停止该文件删除并记录精确消费者；不得为追求“文件名归零”改变玩家行为。
