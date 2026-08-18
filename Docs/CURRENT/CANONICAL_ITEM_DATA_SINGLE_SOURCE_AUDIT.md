# 道具数据单一真相审计（2026-08-08）

## 当前结论

工程尚未实现道具数据单一真相。当前同时存在三条会影响正式产品的旧真相：

1. `ItemInnerDataCatalog`：I001-I031 的身份、形状、法门、器类与展示文本。
2. `ItemBalanceWorkbenchCatalog`：I001-I030 的五档数值、词缀、核心、Build 与逐件效果载荷。
3. `C1CampaignLootEligibleItemPool15Carrier`、`C1CampaignPool15BaseCombatFacts`、`C1Lv1StarterItemBaselineCatalog`：正式 Reward、Battle 与固定 I001/I002 使用的局部副本。

正式产品故障不是单一道具错误，而是同一个 ItemInstance 在 Reward、Authority、Battle 和 Presentation 被不同数据源重新解释。

## 唯一保留的逻辑真相

正式运行只允许通过 `TalismanBag.Items.Canonical.CanonicalItemDefinitionResolver` 读取静态道具定义。

Resolver 的编译输入暂时来自现有唯一 `ItemBalanceWorkbenchCatalog.asset` 与 `ItemInnerDataCatalog`。输入资产不向下游暴露；运行时下游只能得到不可变 Canonical Definition、Rarity Profile、生成后的 ItemInstance 与最终 Battle Fact。

- I001-I030：普通道具，同一套标签、形状、五档数值和规则。
- I031：特殊点亮来源，不进入普通掉落。
- I001：不在 Catalog 或掉落算法中承担“固定初始道具”身份。剧情/新手引导以后只通过独立获取策略选择一个 canonical `baseItemId`。
- Reward：先按品阶权重抽品阶，再在符合条件的普通道具中按重复衰减和 Build 推进加权抽 Item。
- ItemInstance：实例生成时固定本次 rolled 属性、词缀、核心资格和 Build 资格。
- Battle：只消费 placed+lit 的累计实例快照，不回查 Pool15、Starter 或 Workbench。

## 已完成的数据覆盖

- 31 个唯一身份：I001-I031。
- 30 个普通掉落候选：I001-I030。
- 150 个普通品阶版本：30 件 × 白/绿/蓝/紫/橙。
- 30 条逐件基础效果规则：来自每个 Profile 的 `signatureAffix.effectPayload`，包含触发、条件、目标、操作、单位和参数。
- 每个普通道具的主属性、副属性、念消耗、冷却、固定词缀、随机词缀池、核心候选、形状、法门和器类。
- I031 仅保留特殊点亮来源身份。

## 引用盘点

| 旧入口 | Runtime 文件 | Editor 文件 | 处理 |
| --- | ---: | ---: | --- |
| `ItemInnerDataCatalog` | 25 | 64 | 只允许 Canonical 编译器和源数据维护工具直读 |
| `ItemBalanceWorkbenchCatalog` | 12 | 38 | 只允许 Canonical 编译器和资产维护工具直读 |
| `C1CampaignLootEligibleItemPool15Carrier` | 12 | 9 | 正式消费者全部迁移后隔离 |
| `C1CampaignPool15BaseCombatFacts` | 7 | 7 | 正式消费者全部迁移后隔离 |
| `C1Lv1StarterItemBaselineCatalog` | 12 | 11 | 从正式 Item Authority/Reward/Battle 移除；保留为历史候选直至无引用 |

## 正式运行必须迁移的读取点

### Reward / materialization

- `V04/RewardDrop/Runtime/CampaignLoot/C1CampaignDirectLootPoolAndPolicy.cs`
- `V04/RewardDrop/Runtime/CampaignLoot/C1CampaignDirectLootSessionResolver.cs`
- `Items/CampaignLoot/C1CampaignLootItemInstanceAndEntitlementCarrier.cs`
- `Items/CampaignBaseline/C1FormalCampaignLootEntitlementAcceptance.cs`

目标：调用 Canonical Drop Policy 和 Canonical ItemInstance Factory；Reward 结果携带同一生成实例，不再重新生成或回查 Pool15。

### Item Authority / Board / Tray

- `Items/CampaignBaseline/C1FormalItemSessionAndArrangementAuthority.cs`
- `Items/CampaignBaseline/C1ExactBattleSandboxItemArrangementPresenter.cs`
- `Items/CampaignBaseline/C1ExactBattleSandboxItemBoardView.cs`
- `Items/CampaignBaseline/C1ExactBattleSandboxItemCardView.cs`

目标：Authority 持有累计 ItemInstance；Board/Tray 只移动实例与形成 placed+lit 快照，不读取另一份属性或战斗事实。

### Battle

- `BattleBridge/Formal/C1FormalRealtimeBattleSessionAdapter.cs`
- `BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `BattleBridge/Formal/C1Lv1FormalBattleApplicationAdapter.cs`
- `BattleBridge/Formal/C1Lv1FormalBattleApplicationEngine.cs`

目标：所有普通道具走同一 Action/Effect 翻译边界；不再区分固定 I001、固定 I002、Pool15 和其他奖励道具。

### Presentation / Detail

- `Items/CampaignBaseline/C1FormalItemPresentationCatalog.cs`
- `Items/CampaignBaseline/C1FormalItemDetailProjectionAndSelection.cs`
- `Presentation/FormalBattle/FormalBattlePresentationRoot.cs`

目标：图片、详情、Card VFX 与 causal VFX 读取同一 ItemInstance/Combat Fact；表现不决定战斗结果。

## 活跃开发工具

`ItemSandbox`、`BuildSandbox` 与 `ShougunuPhase1 BattleSandbox` 仍有价值，但只能作为 Canonical Resolver 的消费者，不再直接拼接 Workbench 与 InnerCatalog。资产维护窗口可以编辑 Workbench；预览和运行验证必须先编译 Resolver。

## 迁移后隔离候选

- `C1CampaignLootEligibleItemPool15Carrier.cs`
- `C1CampaignPool15BaseCombatFacts.cs`
- `C1CampaignLootPool15BattleEffectFamilyFoundation.cs`
- `C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.cs`
- `C1CampaignPool15BuildEnvelopeSimulator.cs`
- `C1Lv1StarterItemBaseline.cs`
- `C1FixedFirstClearItemInstanceCarrier.cs`
- `C1Stage1FirstClearGuaranteedRewardData.cs` 中固定 I002 路径

隔离前提是正式运行和仍在使用的工具已无直接引用。当前不得先删文件再补调用方。

## 唯一尚未自动决定的产品语义

现有 30 条 `signatureAffix.effectPayload` 在旧资产中被标为候选、不接正式 Battle。将它们接入正式战斗需要一次明确的 PLAYTEST_V1 晋升决定：

- `signatureAffix.effectPayload` 是每件道具的基础规则；
- rolled `damage` 驱动直接伤害；
- rolled `nianCost` 驱动念消耗；
- rolled `cooldown` 驱动触发间隔；
- 其他 rolled 主/副属性和 effect payload 按目标 stat/operation 进入现有 HP、护壳、念、行动、治疗、目标或状态 sink；
- 只有 placed+lit 执行；同一 `sourceItemInstanceId` 在真实 mutation 后发 cue。

未确认这组语义前，不得把候选载荷标成正式执行，也不得用 VFX/cue 冒充战斗结果。

## 停止条件

1. 正式 Runtime 中不再直接调用 Pool15 与 Starter Catalog。
2. 除 Canonical 编译器和资产维护工具外，不再直接读取 InnerCatalog/Workbench。
3. Reward 产生的同一 ItemInstance 可追踪到 Tray、Board、placed+lit Battle Fact 和 cue。
4. I001 与 I030 使用同一普通规则；I031 唯一特殊。
5. 新增 I032（若未来需要）只增加一条 Canonical 定义/资产，不修改 Reward、Board、Battle 或 Presentation 分支。

## 2026-08-08 单窗口实施回收

### 已落地

- `CanonicalItemDefinitionResolver` 已覆盖 I001-I031 身份，以及 30 个普通道具的五档数值、词缀、核心候选、Build 标签、形状、展示字段与基础效果载荷。
- `CanonicalItemInstanceFactory` 已复用现有实例生成引擎，产出唯一 rolled `ItemGeneratedInstanceSnapshot`。
- `CanonicalItemDropPolicy` 已实现“先抽品阶、再抽 Item”的算法权重；I001-I030 同规则，I031 结构性排除，未写 per-ID 掉落率。
- 正式 `UnifiedBattleFormalSceneHost` 已增加唯一 Workbench 序列化输入，并在接收正式上下文前只创建一个 `CanonicalItemDefinitionResolver`。
- 现有 Unified Scene Authoring 负责把当前唯一 Workbench 资产绑定到 Host；没有移动资产、没有新 Scene、没有手改 YAML。
- `C1FormalObtainRebuildBattleLoop` 已要求从 Host 接收同一 Resolver；正式 `C1FormalItemSessionAuthority` 已持有该 Resolver。
- Authority 生成 Board、Tray、lighting 与 Build 的 `ItemSystemSnapshot` 时，已经由 Resolver 投影 I001-I031 身份数据，不再让正式 Board/Tray 自行回查静态 InnerCatalog。
- 仍在运行正式 Loop 的两个 Editor 入口已改为先加载同一 Workbench 并创建 Resolver。
- C# Runtime 直接编译通过；当前只有两个既有未使用字段 warning。

### 明确没有做

- 没有新建第二套 Reward Session、Item Authority、Battle Session 或 Bridge。
- 没有把 I001 重新包装后继续写死为初始道具；现有固定 I001 入口仍是待替换历史阻塞。
- 没有把 Canonical 数据以并行字段塞进旧 Pool15 `GrantBundle`；该做法会形成双轨 Reward Contract。
- 没有把候选 effect payload 直接解释成 HP、念或冷却 mutation；Battle 语义尚未获一次性晋升确认。
- 没有操作 Unity、没有序列化 Scene、没有 Fresh Play。

### 正式 Runtime 仍直接依赖历史道具事实的文件

1. Reward 契约链：`C1CampaignDirectLootPoolAndPolicy.cs`、`C1CampaignDirectLootSessionResolver.cs`、`C1CampaignLootItemInstanceAndEntitlementCarrier.cs`、`C1FormalCampaignLootEntitlementAcceptance.cs`。
2. 固定初始/首奖链：`C1Lv1StarterItemBaseline.cs`、`C1FixedFirstClearItemInstanceCarrier.cs`、`C1Stage1FirstClearGuaranteedRewardData.cs`。
3. Battle 旧事实链：`C1CampaignPool15BaseCombatFacts.cs`、`C1CampaignLootPool15BattleEffectFamilyFoundation.cs`、`C1FormalRealtimeBattleSession.cs`、`C1FormalRealtimeBattleSessionAdapter.cs`、`C1Lv1FormalBattleApplicationAdapter.cs`、`C1Lv1FormalBattleApplicationEngine.cs`。
4. Presentation/Detail 旧投影链：`C1FormalItemPresentationCatalog.cs`、`C1FormalItemDetailProjectionAndSelection.cs`。

### 唯一安全迁移顺序

1. 原位替换现有 Reward Resolver/Grant/Entitlement 契约的数据来源和实例载荷；不增加 Canonical Reward 分支。
2. 让现有 Authority 接受上述同一个 rolled ItemInstance；随后删除正式调用中的 Pool15 materialization。
3. 用独立的剧情/新手“获取策略输入”替换 Authority 内固定 I001；策略只选择 canonical `baseItemId`，不得成为第二份道具数据库。
4. 以同一个 generated instance 形成 placed+lit Battle Fact，再一次性替换 Pool15/Starter 战斗投影。
5. Presentation/Detail/VFX 只消费同一实例与 mutation 后 cue。
6. 正式链零引用后再把 Pool15、Starter 固定数据和旧 verifier 标记为隔离/删除候选。

### 需要用户明确批准的两项契约替换

- `REWARD_CONTRACT_REPLACE_IN_PLACE`：允许在现有 Reward Session/Grant/Entitlement 类型内原位移除 Pool15 与固定十五件物化语义，改为承载 Canonical Drop 产生的 30 件 rolled instance；不得并行保留两条正式分支。
- `INITIAL_ACQUISITION_POLICY_REQUIRED`：允许从 Authority 移除固定 I001；在剧情/新手策略资产尚未提供前，正式初始载荷不得自行猜一个替代 Item。

在这两项未确认前，继续修改 Reward 或固定 I001 都会重建双轨或再次把产品规则写死，因此当前不会把“Catalog/Host 已接入”误报成正式全流程完成。

## 2026-08-09 Cleanroom Recovery 冻结审计

### 用户决策已更新

- `REWARD_CONTRACT_REPLACE_IN_PLACE`：已批准。
- `INITIAL_ACQUISITION_POLICY_REQUIRED`：已批准；初始道具由独立策略从 Canonical 普通道具中选择，不在掉落数据库内写死 I001。
- `signatureAffix.effectPayload`：已批准作为 `PLAYTEST_V1` 基础战斗规则；rolled `damage`、`nianCost`、`cooldown` 与该规则共同进入正式 Battle。
- 正式运行只保留 I001-I031 一套数据库；Pool15 与固定 Starter 不是兼容层，而是完成接管后同包删除的历史结构。

### 唯一最终运行图

```text
Workbench + Inner Identity
        -> CanonicalItemDefinitionResolver
        -> CanonicalItemDropPolicy / CanonicalInitialItemAcquisitionPolicy
        -> CanonicalItemInstanceFactory
        -> one ItemGeneratedInstanceSnapshot
        -> existing C1FormalItemSessionAuthority
        -> cumulative placed+lit C1FormalItemBattleInputSnapshot
        -> existing C1FormalRealtimeBattleSession
        -> real HP / shell / Nian / action / heal / target / status mutation
        -> same sourceItemInstanceId cue
        -> existing Unified Card / causal VFX / Item Detail projection
```

任何下游模块不得再次读取 Workbench、InnerCatalog、Pool15 或 Starter 来重新解释同一实例。

### 四个责任岛的第一个真实断点

| 责任岛 | 当前状态 | 冻结判断 |
| --- | --- | --- |
| Reward | 正常路线已经用 Canonical Drop 生成唯一 `ItemGeneratedInstanceSnapshot`，Grant 与 Entitlement 保留同一实例；旧 Pool15 overload、物化器与签名逻辑仍残留 | `KEEP` 现有 Session/Grant 外壳；`REPLACE` 旧 overload；零引用后 `DELETE` Pool15 私有实现 |
| Authority / Initial Acquisition | Resolver、同一 generated instance、独立初始获取策略已经存在；Tray/Board/lighting 已读同一 Authority | `KEEP` Authority/Board/Tray；删除固定 I001/I002 获取入口 |
| Authority -> Battle 边界 | `C1FormalItemOrdinaryInstanceSnapshot` 持有 generated instance 与 canonical definition，但 `C1FormalItemBattleItemRow` 丢弃二者，只保留旧 `combatFact/actionCostFact` | **首个真实数据断点**；原位让 Battle row 携带同一 generated instance + definition |
| Battle | Adapter 按 I001/I002/Pool15 分支重建事实；Session 仍要求 Pool15 envelope。直接伤害循环、敌人 HP/壳、念、行动进度、玩家护势与 cue ledger 可复用 | `KEEP` 单一 live Session 与真实状态槽；`REPLACE` Item feeder/effect interpreter；删除 Pool15/Starter oracle |
| Detail / Presentation | 正式展示资产只有 17 行（I001、15 件旧池、I031），但 I001-I030 图片文件均存在；Detail 仍回查 Starter/Pool15 | `ASSET-KEEP` 现有图片与共享 Card/Board Prefab；`REPLACE` 查询来源；补齐 30 件普通道具的图片绑定覆盖，不新增 Prefab/Scene |

### 30 条 PLAYTEST_V1 规则覆盖

- 30 个普通道具全部具有非空 `effectId / trigger / condition / target / operation / unit`。
- operation 只使用现有十类通用操作：`AddFlat`、`AddPercent`、`Multiply`、`ReduceFlat`、`ReducePercent`、`Refund`、`ExtraTarget`、`ExtraTrigger`、`Convert`、`Override`。
- 目标状态覆盖：`damage`、`break`、`nian`、`nianCost`、`cooldown`、`guard`、`burn`、`ember`、`heal`、`cleanse`、`castProgress`、`targetCount`、`targeting` 及几类转换目标。
- I004 不是单独补丁。它与其他 29 件一样先执行 rolled direct damage；其 `on_direct_lit / next_zhenlei_trigger / ExtraTarget targetCount` 由同一 effect interpreter 解释。
- Profile 的所有 trigger、condition、target、unit 均有显式值，因此本次没有需要猜测的空语义；离散 `turn` 统一使用已批准的 `1 turn = 400ms`。

### 组件账本

| 组件 | 标签 | 理由 / 接管条件 |
| --- | --- | --- |
| `CanonicalItemDefinitionResolver` | `KEEP` | 唯一静态定义出口 |
| `CanonicalItemInstanceFactory` | `KEEP` | 唯一 rolled instance 工厂 |
| `CanonicalItemDropPolicy` | `KEEP` | 30 件统一算法掉落，I031 结构性排除 |
| `CanonicalInitialItemAcquisitionPolicy` | `KEEP` | 与掉落解耦，不写死 I001 |
| `C1FormalItemSessionAuthority` | `KEEP + REPLACE BOUNDARY` | 保留 Owner；Battle row 改传同一实例与 definition |
| 现有 5x5 Board、Tray、共享 Card Prefab | `KEEP` | 玩家证据已证明领取与摆放通过 |
| `C1FormalRealtimeBattleSession` | `KEEP + REPLACE FEEDER` | 保留唯一 live loop 与真实状态槽；移除 Pool15/Starter 输入 |
| `C1FormalRealtimeBattleSessionAdapter` | `REPLACE IN PLACE` | 统一翻译所有 placed+lit generated instances，无 per-ID 分支 |
| `C1FormalItemPresentationCatalog.asset` | `ASSET-KEEP + DATA COVERAGE` | 仅保留图片绑定职责；从 17 行补齐 I001-I031，不承载战斗真相 |
| Unified pulse/ribbon/impact 与七层 Card VFX | `ASSET-KEEP` | 只消费 mutation 后 cue，不决定伤害 |
| Pool15 carrier/base facts/effect foundation/candidate profiles/build simulator | `DELETE` | 最后消费者切换后删除，不保留兼容期 |
| Starter baseline/fixed first-clear/fixed I001/I002 oracle | `DELETE` | 初始获取与 Battle 已由 Canonical 接管 |
| 旧 Pool15/Starter focused verifier | `DELETE OR REWRITE` | 只保留证明单一 Canonical 链与真实 mutation 的验证 |
| 无关 Sandbox 与历史 Scene | `OUT OF SCOPE` | 本次不清理，不参与正式运行 |

### 原子切换顺序与停止条件

1. Reward：删除旧 Resolve/Select/materialization 消费者；正式调用只剩 Canonical grant。停止条件：正式 Runtime 无 Pool15 Reward 引用。
2. Authority：Battle row 保留同一 generated instance + definition；移除旧 combat hydration 与固定首奖入口。停止条件：Reward/Authority/Battle 的 `itemInstanceId` 和对象载荷一致。
3. Battle：Adapter 无 I001/I002/Pool15 分支；Session 对所有 placed+lit 项执行 rolled direct damage、念消耗、冷却与通用 effect mutation，mutation 后才发同源 cue。停止条件：30 个普通 ID 经同一路径产生可观察真实状态变化。
4. Detail/Presentation：文字/属性/effect 从同一实例与 definition 读取；图片绑定补齐且单图失败不清空其他 Card。停止条件：I001-I030 均可显示同一实例内容，Presentation 不回写数值。
5. 清退：删除零引用 Pool15/Starter 正式结构及其专用测试。停止条件：正式 Runtime 静态零引用且定向编译通过。

本节取代上方“仍需用户批准”的旧状态；当前 `User Decision Point = NONE`。
