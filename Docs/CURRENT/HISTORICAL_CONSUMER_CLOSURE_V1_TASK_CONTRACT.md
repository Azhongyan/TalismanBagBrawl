# HISTORICAL_CONSUMER_CLOSURE_V1

Status: SAFE_CUTOVER_COMPLETE / UNITY_COMPILE_PASS / PLAYER_VISIBLE_DELIVERY_NONE / STOP
Classification: CONTAINED_ONE_GUARD
Task mode: 水电 + 检查房间
Product context: CAMPAIGN_NORMAL_LV1

## Outcome

已点名的历史接口不再被新的 Formal 运行路径误接。当前仍有价值的消费者改接唯一 Canonical Item / Formal Enemy 真相；无法在本包安全退休的深层兼容结构被精确登记，而不是按名称批量删除。

## User accepted scope

用户已接受：`只读消费者图 → 有效消费者改接正式真相 → focused verify → 零消费者证明 → 精确删除 → STOP`。

本包只处理：

- 旧 Pool15 入口；
- 旧 1-1～1-2 / 1-3～1-5 Enemy Catalog；
- 与上述入口绑定的历史签名门禁；
- 已失效的 Cleanse QA 时序；
- `CreateItemFact.firstOffsetMs` 误导参数；
- 对应代码和 Unity 序列化消费者。

## Consumer audit result

### A. Legacy Enemy Catalog

1. `C1CampaignDirectLootPoolAndPolicy` 仍在正式 Reward 路径读取旧 1-3～1-5 Catalog，以判断下一战是否有可净化威胁。标签：`LIVE_FORMAL / REPLACE_NOW`。
2. `C1FormalRealtimeBattleSessionAdapter` 对当前 1-1～1-5 已始终选择 `C1FormalEnemyDefinitionCatalog.CatalogId`，但仍保留不可达的旧 Catalog 选择分支。标签：`DEAD_FORMAL_BRANCH / DELETE_NOW`。
3. `C1FormalRealtimeBattleEncounterResolver.TryResolve` 已无条件委托 Canonical resolver；旧 early / stage3to5 resolve 与校验方法不可达。标签：`DEAD_FORMAL_BRANCH / DELETE_NOW`。
4. `C1FormalRealtimeBattleSession` 仍借用旧 Contract 的 Stage / Balance / Encounter 身份常量做入口校验。它不读取旧 Catalog 行，但会阻止旧 Contract 文件直接删除。标签：`COMPATIBILITY_IDENTITY_DEPENDENCY / DEFER`。
5. 旧 Enemy verifier、simulation、shield verifier 与部分大 verifier 仍直接读取旧 Catalog。标签：`LEGACY_QA_OR_SIMULATION / SEPARATE_RETIREMENT`。

### B. Pool15

1. 当前 Formal Adapter 只生成 Canonical `directItemFacts`；`pool15ActiveItemFacts` 恒为空，旧 envelope 字段恒为空。
2. 正式 Mainline 通过 `TryStartFromCumulativeItemSnapshot` 启动 Session，没有发现产品 Host 调用 `TryExecutePool15Event`。
3. 但 Pool15 类型、事件 API、状态账本、自动触发器与兼容验证仍深嵌 `C1FormalRealtimeBattleSessionContracts`、`C1FormalRealtimeBattleSession`、Adapter overload、Authority nullable field 和大量旧 Editor verifier。
4. `C1Pool15FormalItemArtworkAndSourceAnchorBinding` 是当前正式图片与因果来源绑定，只是名称陈旧；它不读取旧 Pool15 数据。标签：`LIVE_FORMAL_NAMING_DEBT / DO_NOT_DELETE`。
5. Scene、Prefab、ScriptableObject 中未发现 Pool15 数据类型或旧 Pool ID 的文本序列化引用。
6. `CampaignStage_1-3.asset`～`CampaignStage_1-5.asset` 仍保存旧 stage3to5 balance / encounter identity 字符串；当前 Canonical resolver 不以这些字符串选择 Enemy 行，但 Session 入口仍借用旧 Contract 身份做兼容校验。标签：`SERIALIZED_COMPATIBILITY_IDENTITY / DEFER`。

结论：Pool15 旧数据源已经不是正式 Truth Owner，但其兼容 API 尚未达到物理零引用。本包禁止整岛删除；先移除安全消费者，剩余 Session/Contract 迁移另立精确包。

### C. QA debt

- Cleanse focused verifier 已按当前 Enemy Skill 生命周期改正，属于 `QA_LIFECYCLE_UPDATED`，不再修改产品语义。
- `CreateItemFact.firstOffsetMs` 参数没有参与首次触发时间计算。标签：`QA_MISLEADING_PARAMETER / REMOVE_NOW`。

## Allowed writes

- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Runtime/CampaignLoot/C1CampaignDirectLootPoolAndPolicy.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSessionAdapter.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleEncounterResolver.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier.cs`
- 本 Task Contract
- `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md`

## Forbidden writes

- Pool15 Session / Contract 整岛删除
- Battle mutation、Enemy Skill、Item、Reward 掉落算法或产品语义
- Scene、Prefab、Profile、Presentation、Save、WorldMap、StageConfig
- 旧 verifier / simulator 批量删除
- 新 Catalog、Bridge、Adapter、Manager、Fallback
- 历史签名值的计算、刷新或新增

## Patch budget

- 产品 / QA 代码最多 4 个文件；文档 2 个文件。
- 超出时停止并把新增依赖登记为下一迁移岛，不扩大本包。

## Required behavior

1. Reward 的下一战可净化威胁判断读取 `V04Chapter1StageWaveEncounterTruth + C1FormalEnemyDefinitionCatalog`，不再读取旧 1-3～1-5 Catalog。
2. Formal Adapter 对支持的 1-1～1-5 只绑定 Formal Enemy Catalog，不保留不可达旧 Catalog 选择。
3. Encounter Resolver 删除不可达的旧 Catalog resolve / validation 代码；当前 Canonical resolve 行为不变。
4. QA helper 不再声明一个实际未使用的 first-offset 参数。
5. 不改变掉落算法、Battle 状态、Enemy 数值、技能时序或玩家流程。

## Verification

- 静态消费者复扫：正式 Reward / Adapter / Resolver 不再引用旧 Enemy Catalog。
- Scene / Prefab / SO 序列化扫描：Pool15 类型与旧 Pool ID 为 0；旧 Enemy stage3to5 身份在 1-3～1-5 Stage 资产中有 3 个兼容命中，已登记而未误删。
- 用户持有的 Unity 在四个代码文件写入后自行触发脚本编译与 Domain Reload；当前 Editor log 为 `0 error CS`。
- 相关 focused verifier 已纳入同次 Unity 编译；本包未控制 Unity、未运行 verifier、未要求玩家手测。

## Completion receipt — 2026-08-18

- Reward 的下一战能力判断已从旧 1-3～1-5 Catalog 改接 `V04Chapter1StageWaveEncounterTruth + C1FormalEnemyDefinitionCatalog`。
- Formal Adapter 已移除当前 1-1～1-5 的不可达旧 Catalog 选择；Encounter Resolver 已删除不可达的 early / stage3to5 resolve 与校验分支。
- Canonical Effect focused verifier 已移除未生效的 `CreateItemFact.firstOffsetMs` 参数；真实测试时间变量继续保留。
- 未修改掉落算法、Battle mutation、Enemy 数值/技能、Stage 资产、Prefab、Scene、Presentation 或玩家流程。
- 本包没有安全的整文件零消费者候选，因此没有物理删除旧 Catalog、Pool15 Session/Contract 或旧 verifier。
- `C1Pool15FormalItemArtworkAndSourceAnchorBinding` 确认为当前正式图片/来源绑定，禁止因名称含 Pool15 而删除。
- 终态：`SAFE_CUTOVER_COMPLETE / LEGACY_ISLANDS_QUARANTINED / UNITY_COMPILE_PASS / PLAYER_VISIBLE_DELIVERY_NONE / STOP`。

## Completion / STOP

- 安全改接与 dead-branch 删除完成后更新 Component Ledger。
- Pool15 Session/Contract 深层兼容岛继续保持 `QUARANTINE / MIGRATION_REQUIRED`，不得在本包强删。
- 发现新的历史岛只登记，不自动扩包。
- `UNITY_NOT_REQUIRED`。

## ROOM CHECK

- New Owner: 0
- New Data Source: 0
- New Fallback: 0
- Formal → Sandbox: 0
- Pending Retirement: 2（Pool15 Session/Contract compatibility island；旧 Enemy Contract/serialized identity/Editor QA island）
- Room Status: WARNING；DeleteWhen = Canonical Session/Contract 不再携带 Pool15 types/API，旧 Editor consumers 已迁移或删除，并且 1-3～1-5 Stage 资产与 Session 不再依赖旧 Enemy compatibility identity。
