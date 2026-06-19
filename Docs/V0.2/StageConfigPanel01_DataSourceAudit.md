# V0.2 StageConfigPanel01 数据源审查

日期：2026-06-18  
基线：`ba56d8d`（`feature/v0.2-coreloop01`）

## 审查结论

- 当前 CoreLoop01 正式运行入口是 `Scene_TalismanBag_V02_FormationCounter.unity` 引用的 `RunConfig_V02_15Min.asset`。
- 根目录旧 Item、Enemy、RunConfig 资产仍用于历史场景或旧工具，不是当前 CoreLoop01 的第二套并行主线。
- 工程 verified 数据优先。本次没有从旧策划表导入数值，也没有改写原有 1-10 RunConfig 字段；新增 StageConfig 字段和 2-10 列表均以当前工程快照为来源。
- 旧版与 V0.2 道具存在 7 组重复 `itemId`。旧版资产保留并标记为 `Legacy`/`Deprecated`，V0.2 资产作为 active verified 数据；未删除资产。
- 当前 DataCatalog 校验结果为 `Error=0, Warning=15`。Warning 是保留的 legacy 重复 ID 和一组历史同名 Boss 占位数据，不影响 active catalog。

## 当前数据源清单

### Item

正式/verified：

- `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/*.asset`
- 类型：`TalismanItemDefinition`
- 当前主线与 V0.2 场景引用此目录资产。

历史/legacy：

- `Assets/_Game/ScriptableObjects/TalismanBag/*.asset`
- 与 V0.2 重复的 `fire_talisman_basic`、`thunder_talisman_basic`、`shield_talisman_basic`、`spirit_stone_basic`、`qi_pill_basic`、`seal_basic`、`sword_pill_basic` 已标记为 legacy/deprecated。

新增正式定义：

- `bronze_seal_basic`：对应既有 2-10 Boss 首通奖励中的青铜法印，仅作为库存道具定义。

仍在代码中按 `itemId` 硬编码的部分：

- `AutoCombatController` 的既有符箓效果分派、组合判断与特例。
- `V02RewardPanel` 的既有展示分类。
- 这些属于现行战斗机制，本包未重写。

### Enemy

基础 verified：

- `Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/*.asset`
- 类型：`EnemyDefinition`
- 用作 2-10 基础怪物和 1-10 verified 快照的来源。

StageConfigPanel01 verified 快照：

- `Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/StageConfigPanel01/*.asset`
- 将原 `V02RunFlowController.BuildChapterOneRuntimeEnemy` 中已经验证的 1-1 至 1-10 数值原样资产化。
- 原运行时代码保留为兼容回退；当关卡挂有 `EnemyGroupConfig` 时优先读取配置。

历史/legacy：

- `Assets/_Game/ScriptableObjects/TalismanBag/Enemies/*.asset`
- `Assets/_Game/ScriptableObjects/TalismanBag/V02/v02_mountain_imp.asset` 已明确为 deprecated。

### EnemyGroup

- `Assets/_Game/ScriptableObjects/TalismanBag/V02/EnemyGroups/*.asset`
- 1-1 至 1-10、2-1 至 2-10 共 20 个唯一 `enemyGroupId`。
- 每组可配置敌人、数量、出场顺序、HP/攻击/攻速/护盾/技能频率倍率。
- 当前战斗是单个 `EnemyRuntime` 表达；运行时读取主 entry 和倍率，不扩大战斗系统为多单位系统。

### Stage

唯一正式入口：

- `Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset`
- 类型：`V02RunConfig`
- `rounds`：1-1 至 1-10。
- `chapterTwoRounds`：2-1 至 2-10。

新增配置字段：

- `chapterId`、`nextStageId`、`stageType`
- `enemyGroup`、`rewardConfig`、`dropTable`、`bossConfig`
- `tutorialGuideId`、`unlockCondition`
- `onWinAction`、`onLoseAction`
- `autoAdvance`、`allowBackpackEdit`、`stopBeforeBoss`
- `benchmarkTargetId`

运行时规则：

- `V02RunFlowController` 仍执行流程。
- `MainTrialFlowService` 仍持有存档状态机。
- `nextStageId` 可决定下一关。
- 2-9 配置 `stopBeforeBoss=true`。
- 2-10 配置 `autoAdvance=false`，仍由 BossInfoPanel 后手动开战。

### Reward

CoreLoop 正式奖励：

- `Assets/_Game/Resources/CoreLoop/Rewards/*.asset`
- 类型：`RewardConfig`
- 覆盖 1-2、1-4、1-5、1-6、1-9 固定教学奖励、1-10 章节奖励、2-10 Boss 奖励。
- `TutorialGuideConfig_Fix03.asset` 引用固定教学/1-10 奖励。
- 2-10 Boss 通过 `RewardConfig.LoadById("boss_2_10_clear")` 读取。
- 正式发放仍为 `RewardService -> ResourceService/ItemInventoryService -> SaveService`。

旧 V0.2 三选一奖励：

- `Assets/_Game/ScriptableObjects/TalismanBag/V02/Rewards/*.asset`
- 类型：`V02RewardDefinition` / `V02RewardPoolConfig`
- 仍保留给旧 fallback/debug 路径，本包没有恢复为主线奖励。

### Drop

正式来源：

- `Assets/_Game/Resources/CoreLoop/DropTables/chapter_2_normal_round_drops.asset`
- 类型：`RewardDropTable`
- `Assets/_Game/Resources/CoreLoop/IdleDropConfig.asset`
- 类型：`IdleDropConfig`
- 2-1 至 2-9 的 Stage `dropTable` 均直接引用同一张 verified 掉落表；2-10 Boss 不引用普通关掉落表。
- `IdleDropConfig` 配置普通关掉落表、每关 roll 次数与是否启用普通关胜利掉落。
- 发放继续走 `RewardService.GrantDropTable()`，再进入 `ResourceService` / `ItemInventoryService` / `SaveService`。

verified 普通关单次掉落：

- 灵石：100%，8–12
- 符纸：50%，1–2
- 朱砂：30%，1
- 初阶符胚碎片：15%，1
- 基础符箓残页：20%，1
- 完整基础道具：5%，1

新增正式物品定义：

- `basic_talisman_embryo_shard`
- `basic_talisman_page`
- `basic_tool_complete`

运行时优先读取 Stage `dropTable`，然后读取场景 override、`IdleDropConfig`，最后才使用旧运行时构造作为兼容 fallback。
Task 6 不实现离线时间模拟、洞府收益、批量离线结算或新存档结构。

### Boss

正式来源：

- `Assets/_Game/Resources/CoreLoop/BossConfigs/boss_1_10_array_breaking_warlord.asset`
- `Assets/_Game/Resources/CoreLoop/BossConfigs/boss_2_10_formation_breaker.asset`
- 类型：`BossInfoConfig`
- 1-10 与 2-10 的 Stage `bossConfig` 分别直接引用匹配的 verified BossConfig。
- 普通关 `bossConfig` 保持为空。

配置覆盖：

- Boss ID、显示名与 EnemyDefinition 引用
- 战前机制、主要威胁、推荐工具与提示
- 护盾/召唤/封阵三阶段血量阈值
- 首次行动延迟与各阶段行动间隔
- 护盾量、召唤伤害、毒/燃烧层数、封印时间与供能干扰时间
- Verified / Debug / Deprecated 数据源标记

运行时继续使用原 `V02BossPhaseController`，优先读取当前 Stage 的 BossConfig；无配置时保留场景序列化值作为兼容 fallback。
2-10 的 BossInfoPanel 继续复用同一 BossConfig 的信息字段，不新增第二套 Boss UI 或战斗系统。

### Upgrade

正式来源：

- `Assets/_Game/Resources/CoreLoop/CoreLoopTalismanUpgradeConfig.asset`
- 类型：`TalismanUpgradeConfig`
- `UpgradeService` 与 `BattleLoadoutSnapshotBuilder` 均通过 Resources 路径读取。

verified Lv.1 → Lv.2：

- 火符：伤害 `+25%`
- 雷符：伤害 `+15%`、破盾 `+30%`
- 护身符：护盾 `+25%`
- 净化符：冷却 `-20%`
- 镇魂符：压制时间 `+20%`
- 公共消耗：灵石 100、符纸 50、朱砂 8、初阶符胚 1。

结论：

- Task 8 已在 StageConfigPanel01 增加 Upgrade 页签，直接编辑同一份正式 `CoreLoopTalismanUpgradeConfig.asset`。
- 配置新增 `configId`、显示名、Verified / Debug / Deprecated 来源标记。
- `UpgradeService` 与 `BattleLoadoutSnapshotBuilder` 继续读取同一 Resources 路径，没有新增第二套升级或战斗数值来源。
- DataCatalog 现校验：
  - UpgradeConfig 空配置与重复 ID
  - 升级 ItemDefinition 缺失或 `canUpgrade=false`
  - 等级范围非法、重复 key 与等级断档
  - 培养消耗为空、数量非法或资源类型重复
  - StatModifier 缺失、倍率非正数或完全无效果
- 本任务没有修改上述 verified 数值，也没有增加 Lv.3+。

## Debug / Test / Deprecated

- `RunConfig_15Min_Test.asset`、`run_15min_mobile.asset`：旧版测试/验证局配置，不是 V0.2 CoreLoop01 主线。
- `V02RewardPoolConfig` 与 `V02RewardDefinition`：旧 V0.2 奖励选择/调试路径，不是 CoreLoop 固定奖励发放入口。
- 根目录旧 Item/Enemy 资产：标记 `Legacy`；与 V0.2 重复的 Item 标记 `Deprecated`。
- `v02_mountain_imp.asset`：明确 deprecated。
- Debug/Deprecated/Legacy 数据在 Item、Enemy 与 StageConfigPanel01 面板中默认隐藏。

## 重复加载与孤儿引用

- 未发现 CoreLoop01 同时加载旧 RunConfig 与 V02 RunConfig。
- 未发现运行时同时加载两份同 ID Item 资产；重复项来自编辑器全目录扫描，active catalog 只选 V0.2 verified 项。
- 未发现 Stage、EnemyGroup、Reward 或 Upgrade 的孤儿引用。
- DataCatalog 校验：`Error=0`。

## DataCatalog 收录范围

已收录并可定位资产路径：

- `TalismanItemDefinition`
- `EnemyDefinition`
- `EnemyGroupConfig`
- `V02RunConfig` / `V02RoundConfig`
- `RewardConfig`
- `RewardDropTable`
- `IdleDropConfig`
- `BossInfoConfig`
- `TalismanUpgradeConfig`

校验覆盖：

- 空 ID、重复 ID、同名不同 ID
- Stage `nextStageId` 孤儿引用
- Stage Enemy/EnemyGroup 缺失
- EnemyGroup 空 entry / 缺失 Enemy
- Debug/Deprecated EnemyGroup 混入正式 Stage
- Reward Item 引用不存在
- IdleNormal Stage 缺失 DropTable
- DropTable 空 entry、非法概率/数量、孤儿 Item 引用
- IdleDropConfig 缺失 DropTable 或 roll 次数非法
- Boss Stage 缺失 BossConfig
- BossConfig 与 Stage Enemy ID 不一致
- BossConfig Enemy 缺失或不是 Boss
- Boss 阶段阈值、时序与效果值非法
- UpgradeConfig 空配置或重复 ID
- Upgrade ItemDefinition 孤儿引用或未启用培养
- Upgrade 等级范围、重复 key 与等级断档
- Upgrade 消耗为空、数量非法或资源类型重复
- Upgrade StatModifier 缺失、倍率非法或无实际效果

## verified CoreLoop01 快照

1-10 关卡敌人数值来自原运行时 verified 表：

| Stage | Enemy | HP | Attack | Interval |
|---|---|---:|---:|---:|
| 1-1 | 游灯小祟 | 72 | 6 | 2.8 |
| 1-2 | 游灯小祟·复习 | 96 | 8 | 2.6 |
| 1-3 | 石甲符卫 | 110 | 7 | 2.8 |
| 1-4 | 石甲符卫·复习 | 135 | 8 | 2.7 |
| 1-5 | 铜爪压阵怪 | 140 | 13 | 2.2 |
| 1-6 | 灰火毒祟 | 130 | 8 | 2.8 |
| 1-7 | 摄灵封符鬼 | 135 | 8 | 2.8 |
| 1-8 | 摄灵封符鬼·复习 | 155 | 9 | 2.65 |
| 1-9 | 游灯小祟 + 石甲符卫 | 175 | 9 | 2.45 |
| 1-10 | 破阵妖将 | 240 | 12 | 2.8 |

奖励快照：

- 1-2：雷符 x1
- 1-4：护身符 x1
- 1-5：净化符 x1
- 1-6：镇魂符 x1
- 1-9：连锁雷符 x1
- 1-10：剑丸 x1、灵石 120、符纸 60、朱砂 10、初阶符胚 1、修为 20
- 2-10：青铜法印 x1、初阶符胚 1、灵石 80、符纸 40、朱砂 6、修为 10

## 后续边界

- Task 6 已完成 DropTable / IdleDropConfig 资产化。
- Task 7 已完成完整 BossConfig 参数面板与 1-10 / 2-10 资产化。
- Task 8 已完成 UpgradeConfig 参数面板与完整校验。
- Task 9 再处理 DebugPanel 扩展。
- 本任务不进入离线模拟、洞府收益、Tune01、3-10、新玩法或 DebugPanel 扩展。
