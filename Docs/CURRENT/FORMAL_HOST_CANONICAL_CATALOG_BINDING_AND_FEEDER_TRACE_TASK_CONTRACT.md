# Formal Host Canonical Catalog Binding And Feeder Trace Task Contract

## Outcome

让现有 `UnifiedBattlePageShell.prefab` 的 `UnifiedBattleFormalSceneHost` 序列化绑定现有唯一 `ItemBalanceWorkbenchCatalog.asset`，随后沿 WorldMap 正常路线追踪真实奖励 `ItemInstance` 是否以同一身份、`placed+lit` 状态和 Canonical battle fact 到达 `C1FormalRealtimeBattleSession` 输入。

本包只闭合正式 Host / Feeder，不实现 Battle effect family，不以 I004 或任何单一道具作为运行时特例。

## Task Class And Ownership

- Class: `COMPLEX_GUARDED_ONCE`
- Primary / Development Owner: 当前 Codex 主对话窗口
- Unity serialization owner: 当前 Codex 主对话窗口的一次干净、可记录 batch 生命周期
- User Review Gate: `NONE`，不改变产品语义、掉落算法或数值

## Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1FormalObtainRebuildBattleLoopSceneAuthoring.cs`
- `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab`
- `Logs/formal_host_canonical_catalog_binding.log`
- 本 Task Contract
- `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md` 的本包结果段
- Notion 技术报告 13 的本包结果段

## Protected Boundaries

- 不改 Reward 概率、Canonical Item Definition、Item Authority、Battle effect 实现、Enemy、Stage、Save。
- 不改 Item Detail、正式 Presentation/VFX、Board/Tray/Card 行为。
- 不新建 Catalog、Authority、Session、Bridge、Scene、Prefab 或 Test Framework。
- 不恢复 Pool15、Starter 私有数据库或 Sandbox 正式所有权。
- 不运行旧的全量 `ApplySingleAuthoringPass`；只执行本次 Catalog Prefab binding。

## Verification

1. Unity 生命周期开始前必须是 `CLOSED_CLEAN`。
2. 既有 authoring owner 对唯一 Catalog 做幂等 Prefab 序列化绑定。
3. Unity import / compile 成功，Prefab Host 的 `canonicalItemCatalogSource` 指向唯一 Catalog。
4. 静态 feeder trace 命名 Host 后的第一个确切断点。
5. 玩家正常路径证据仍是最终门；没有 WorldMap Fresh Play 时最高状态为 `INTERNAL_QA / NOT_READY`。

## Stop Conditions

- `FEEDER_PASS_SESSION_MUTATION_GAP`
- `FEEDER_CODE_GAP`
- `PRODUCT_SEMANTIC_GAP`
- `EXTERNAL_UNITY_OWNER`

## Status

`PACKAGE_COMPLETE / FEEDER_PASS_SESSION_MUTATION_GAP / INTERNAL_QA / PLAYER_PATH_NOT_READY`

## Result — 2026-08-11

- 既有 `C1FormalObtainRebuildBattleLoopSceneAuthoring` 增加了单一、幂等的 Canonical Catalog Prefab binding 入口；既有全量 authoring 在以后重跑时也会保持该绑定。
- 任务自有 Unity batch 生命周期以 exit code `0` 结束，日志给出 `CANONICAL_CATALOG_BINDING_PASS`，Unity 已干净退出。
- `UnifiedBattlePageShell.prefab` 的 Host 已序列化 `canonicalItemCatalogSource`，其 GUID 与 `ItemBalanceWorkbenchCatalog.asset.meta` 一致。
- Feeder 静态真链已逐段确认：
  1. Reward bridge 从当前唯一 Item Authority 创建累计 Battle input。
  2. Authority 为每个普通 Item 保留 `itemInstanceId`、`baseItemId`、generated instance、Canonical Definition、placement、lit/direct-lit。
  3. Adapter 只投影 `placed+lit` 行，不改实例身份，并将 generated instance 与 Canonical Definition 放入 direct Item fact。
  4. Session request 接收 direct Item facts；`TryStart` 原样写入 `activeItemFacts`，随后执行 startup effect 检查并调度 Item action。
- 本轮未发现 Host 后、Session 输入前的第二个 feeder 断点；首个未闭合层是 Session 中真实 HP / Shell / Guard / Nian / Action / Status mutation 的正常路径证据。
- 本轮没有实现 Battle effect、没有单独处理 I004、没有运行 Fresh Play。因此不能提升为玩家可验收或里程碑完成。
