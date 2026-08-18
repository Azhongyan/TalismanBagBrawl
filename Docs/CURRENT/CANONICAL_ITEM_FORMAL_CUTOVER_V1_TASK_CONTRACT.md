# Canonical Item Formal Cutover V1 Task Contract

## 1. Outcome

`CAMPAIGN_NORMAL_LV1` 的正式运行只使用一套 I001-I031 Canonical Item 定义。Reward 生成的同一 `ItemGeneratedInstanceSnapshot` 进入 Tray、5x5 Board、placed+lit Battle input、真实 Battle mutation、Detail/Presentation 与 cue；正式运行不再读取 Pool15、固定 I001/I002 或 Starter combat 数据。

## 2. Product Context

- `CAMPAIGN_NORMAL_LV1`
- I001-I030 是同规则普通道具；I031 是唯一特殊点亮源，不进入普通掉落。
- I001 不在道具数据库或掉落算法中承担固定初始道具身份；初始获取属于独立 acquisition policy。

## 3. Task Class

- `COMPLEX_GUARDED_ONCE`
- 原因：一次正式链迁移跨 Reward、Item Authority、Battle、Detail/Presentation，但不得建立第二套 System、Session、Bridge 或并行真相。

## 3A. User Review Gate

- 是否需要：`YES`
- 状态：`USER_ACCEPTED_SCOPE`
- 已接受：一套 31 件数据库、沿用 31 件标签与规则、算法掉落、移除 Pool15、I001 初始获取规则独立、按责任岛原位切换并删除旧私有结构。

## 4. Owners

- Primary / Development Owner：当前 Codex 任务窗口
- State / Data Owner：`CanonicalItemDefinitionResolver` + `ItemGeneratedInstanceSnapshot`
- Battle Owner：现有 `C1FormalRealtimeBattleSession`
- Presentation Owner：现有 Unified Presenter/Bridge
- User Decision Point：`NONE`

## 5. Allowed Writes

- `Assets/_Game/Scripts/TalismanBag/Items/Canonical/**`
- `Assets/_Game/Scripts/TalismanBag/Items/Generation/**`（仅同一 generated instance 必需字段）
- `Assets/_Game/Scripts/TalismanBag/V04/RewardDrop/Runtime/CampaignLoot/**`
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/**`
- `Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/**`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/**`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/CampaignLoot/**`
- `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/C1CampaignStageClearRewardProgressBridge.cs`
- `Assets/_Game/Scripts/TalismanBag/Presentation/FormalBattle/**`
- 上述运行链直接对应的 `Assets/_Game/Scripts/TalismanBag/Editor/**` focused tests/verifiers
- `Assets/_Game/Configs/ItemBalanceWorkbench/**`（仅正式 PLAYTEST_V1 状态与同一规则数据）
- 本合同与 `Docs/CURRENT/CANONICAL_ITEM_DATA_SINGLE_SOURCE_AUDIT.md`

## 6. Forbidden Writes

- Scene、Prefab、ProjectSettings、Save、Enemy、StageConfig
- 新 Reward Session、Item Authority、Battle Session、Bridge、Adapter 或第二套 Item 数据库
- Item Detail 样式、弹窗布局、VFX 设计扩展
- inventory/material/roll/formation/mid-battle 持久化
- 与本迁移无关的 Sandbox、历史场景或工具清理

## 7. Required Sources

- `AGENTS.md`
- `Docs/LOCKED/ENGINEERING_PROCESS_V2_LOCK.md`
- `Docs/CURRENT/PROJECT_ENGINEERING_STATE_V2.md`
- `Docs/CURRENT/CANONICAL_ITEM_DATA_SINGLE_SOURCE_AUDIT.md`
- 当前 Canonical Catalog、Drop Policy、generated instance、正式 Reward、Authority、Battle、Detail/Presentation 直接依赖

## 8. Patch Budget

- 架构迁移独立包；按责任岛控制，不以固定文件数驱动。
- 允许修改现有文件与删除零引用旧文件。
- 只有现有类型无法承载 Canonical Definition/Instance 时才允许在现有 Owner 文件中增加最小类型；不得新建平行系统。

## 9. Required Behavior

1. Reward：先按品阶算法，再按 duplicate decay/build progression 选择 I001-I030，并生成唯一 `ItemGeneratedInstanceSnapshot`；I031 永不进入普通掉落。
2. Authority：接收并持有 Reward 的同一实例；Tray/Board 只改变该实例的位置与 placed+lit 状态，不重新生成或回查 Pool15/Starter。
3. Battle：只消费累计 placed+lit generated instances；rolled `damage`、`nianCost`、`cooldown` 与 Canonical effect grammar 提交到现有 Session 的真实 HP、shell、Nian、行动、治疗、目标或状态 sink；真实 mutation 后用同一 `sourceItemInstanceId` 发 cue。
4. Detail/Presentation：图片、文字、属性、Card VFX 和 causal VFX 都读取同一实例及 Battle 结果；Presentation 不决定数值结果。
5. Initial Acquisition：由独立 policy 选择 canonical `baseItemId` 并通过同一 factory 生成实例；不得在 Catalog/Drop 内硬编码 I001。
6. 每个责任岛完成 canonical owner 接管且旧引用为零后，同包删除该岛私有 Pool15/Starter 结构；跨岛共享旧结构延后到最后消费者切换。

## 10. Failure / Stop Conditions

- Canonical rule 缺少当前 Battle 所需的明确 trigger、target、unit 或 state sink：停止并命名第一个缺失语义，不猜数值或目标。
- 同一 `itemInstanceId` 在 Reward、Authority、Battle 间不一致：拒绝该 grant，不生成替代实例。
- 重复 stage-clear：复用已接受 grant，不重复进入 Tray。
- 单个图片解析失败不得清除其他 Item 的图片或运行状态。
- 编译、fixture、Profile 或 cue 成功不得代替正常路径玩家证据。

## 11. Delivery Shape

- Config/Profile：唯一 Workbench + Inner identity 编译输入，向下只暴露 Canonical Definition。
- Runtime：原位替换现有 Reward、Authority、Battle、Presentation 消费链。
- Editor：只保留能证明 canonical chain 与真实 state mutation 的 focused tests。
- Scene/Prefab：不修改。

## 12. Verification

- 定向 C# 编译。
- focused tests：30 个普通 base ID 都能经同一算法生成、进入 Authority、形成 placed+lit Battle input；至少按实际 sink 家族验证 HP、shell、Nian、行动、治疗/护势、目标/状态 mutation。
- 静态零引用检查：正式 Runtime 不再引用 Pool15/Starter combat 数据。
- 最终外部 Unity owner 走一次 WorldMap 正常路径 Fresh Play；本窗口不启动或控制 Unity。

## 13. Process Ownership

- `UNITY_NOT_REQUIRED`（代码迁移与定向编译阶段）
- 不创建、下发、读取或消息其他任务窗口。
- 不使用版本库分支或额外工作区。

## 14. Completion

- 正式 Runtime 只有 Canonical Item 真相；Pool15 与固定 Starter combat 路径零引用并删除。
- Reward 的同一实例可追踪到 Tray、Board、Battle mutation、cue 与 Presentation。
- I001-I030 无 per-ID Reward/Battle 分支；I031 仅保留特殊点亮源职责。
- 定向编译与 focused verification 通过；Unity Fresh Play 状态如实交付，不伪造。

## 2026-08-13 Canonical Detail And Presentation Consumer Cutover

- 非伤害道具详情不再被错误要求必须包含 `damage`；`damage` 现在允许不存在，但若重复仍拒绝，`cooldown` 仍保持唯一正式节奏字段。非伤害道具继续展示自己的 Canonical rolled stats 与 `combatEffect`，没有被临时解释成伤害。
- 普通道具正式图片、效果家族、样式与 cue 的运行时来源已改为 `CanonicalItemDefinitionResolver`。旧 `C1FormalItemPresentationCatalog.asset` 的普通道具行不再作为运行时普通道具名单；该资产仅继续承载 I031 明暗图等特殊展示资源。
- I001-I030 的 Presentation 身份现已全部保存在唯一 Canonical Profile 中，并按五个品阶动态投影为 150 条正式普通道具展示行；I031 继续由旧资产只承载一条特殊明暗图展示行。
- 本轮补齐不是 per-ID Runtime 分支：`I001/I005/I006/I029` 按真实破壳/控制结果进入 `ENEMY_CONTROL`；`I008/I009/I012/I021/I027` 按真实直接伤害、持续状态或念节奏进入 `DIRECT_TEMPO`；`I016/I017/I018/I019/I020/I023` 按真实护势、治疗或净化结果进入 `GUARD_SUSTAIN`。现有 `POSITIONAL_ADJACENCY` 与 `TARGET_SPREAD` 无需扩展。
- `CanonicalItemFoundationVerifier` 现在同时验证 30 件 Presentation 身份、30×5 精确行和唯一 I031 特殊行；新增 I032 时仍只需新增一条 Canonical 定义/资产，不修改 Reward、Board、Battle 或 Presentation 分支。
- 离线全量编译：Runtime `0` 错误，Editor `0` 错误；只保留既有警告。Unity 与 Fresh Play：`NOT RUN`。
- Unity owned Batch 已完成一次导入、全量脚本编译与现有 `CanonicalItemFoundationVerifier`；验证结果为 `PASS catalog=31 ordinaryDrop=30 combatRules=30 rarityVersions=150`，新增检查同时覆盖 30 个正式 Presentation 身份、150 个普通精确展示行和唯一 I031 特殊行。进程自然退出，return code `0`，没有 Scene/Prefab 序列化或用户 Unity 接管。
- 当前状态：`DETAIL_NON_DAMAGE_GATE_FIXED / PRESENTATION_RUNTIME_OWNER_CUTOVER / CANONICAL_PRESENTATION_30_COMPLETE / UNITY_FOCUSED_QA_PASS / FRESH_PLAY_NOT_RUN / PLAYER_HANDTEST_PENDING / STOP`。

## 2026-08-13 Detail And Causal Source Two-Gate Correction

- Gate A：最终可见的 `C1ExactBattleSandboxItemDetailPresenter` 不再把 `directDamage > 0` 当成所有普通道具的展示前提；保留现有 cooldown 与正式投影完整性边界。现有聚焦验证同时覆盖伤害、余晖与无直接伤害的 Guard 代表定义。
- Gate B：`TryCreateCurrentSourceAnchorRequest` 仍是唯一正式来源请求构造点；新增的返回值只暴露首个真实拒绝条件，并在现有 `SOURCE_REQUEST_REJECTED` 日志中携带 `itemInstanceId / cueBaseItemId / rosterBaseItemId / rarity`。没有新增第二套 Source、Bridge、Catalog 或 per-Item 分支。
- 现有 Source Anchor 聚焦验证已要求合法 placed+lit 请求成功时拒绝原因为空；没有新建测试框架。
- 离线全量编译：Runtime `0` 错误、Editor `0` 错误；仅保留既有警告。没有启动或控制 Unity，没有修改 Scene/Prefab、Reward、Battle、Enemy、Stage、Save 或产品语义。
- 当前状态：`DETAIL_VISIBLE_GATE_INTERNAL_CLOSE / CAUSAL_SOURCE_FIRST_REJECT_DIAGNOSTIC_READY / OFFLINE_COMPILE_PASS / RUNTIME_FIRST_REJECT_NOT_OBSERVED / NOT_READY / STOP_BEFORE_GUESS_PATCH`。
- 下一门：仅用正常运行中的第一条详细 `SOURCE_REQUEST_REJECTED` 命名共享断点；确认后只修该现有接缝并复用同一聚焦验证。没有这条运行证据不得继续猜修。

### Runtime first reject closure

- 正常路线证据将首断点命名为 `SNAPSHOT_INSTANCE_IDENTITY_MISMATCH`：cue、Roster 的 `itemInstanceId / baseItemId / rarity` 均一致，错误只发生在 Source Request 内的实例身份值。
- 根因是 Source Request 构造器把生成实例的完整不可变身份当作普通文本执行了 `Trim()`；生成实例身份末尾的结构化分隔符因此丢失，请求创建后立刻不再等于 Authority 中的原实例。初始 I007 与后续 Reward 实例共用该生成链，所以这是共享接缝而非单道具问题。
- 修复仅让 `expectedInstanceCanonicalSignature` 原样保存；其他 ID 文本归一化、当前快照校验、placed+lit 校验、Catalog 校验与 Source Anchor 解析均保留。
- 修复后离线编译：Runtime `0` 错误、Editor `0` 错误；只保留既有警告。状态：`SHARED_INSTANCE_IDENTITY_SEAM_FIXED / INTERNAL_COMPILE_PASS / PLAYER_PATH_CONFIRMATION_PENDING`。

### I020 non-damage detail player failure closure

- 玩家正常路线在 1-2 领取紫色 I020 后，点击无法显示详情；该证据推翻了“移除 damage 字段门即已闭合详情”的内部结论。
- 首断点位于正式 Detail Projection 与最终 Presenter 之间：Cleanse 等非伤害定义没有经过直接伤害格式化器，因此没有得到 Presenter 仍强制要求的 `stats / trigger / basic` 三个现有 UI 分区，最终 Presenter 静默关闭弹窗。
- 修复为共享 Canonical 投影：所有效果家族先以该实例自己的 rolled stats、Canonical trigger 和 Canonical basic effect 生成三个基础分区；直接伤害道具随后仍可用既有伤害文案覆盖。没有把 I020 临时解释成伤害，也没有新增 per-ID 分支。
- 聚焦样本现覆盖 I001、I007、I016 与 I020；Runtime/Editor 离线编译均 `0` 错误。Unity 与玩家复测尚未运行。
- 状态：`CANONICAL_ALL_EFFECT_DETAIL_SECTIONS_FIXED / I020_PLAYER_RETEST_PENDING / NOT_PLAYER_READY`。

### 2026-08-13 Real reward Card click boundary correction

- 最新玩家证据表明故障从 1-1 后第一件真实奖励即可出现，因此不再归因为“累计到第四件”或单一 I020 数据问题。
- 首个可执行共享断点位于 `C1ExactBattleSandboxItemCardView`：Card 单击复用了拖动所需的形状占用格命中，异形 Card 的正常点击会在进入正式 Detail Selection 前被吞掉。
- 修正后，整张已绑定 Card 都可发起 `SelectCard`；只有拖动仍要求命中真实占用格。没有修改形状、摆放、点亮、Prefab、Scene 或详情布局。
- `C1ExactBattleSandboxItemArrangementPresenter` 仅在现有 Detail Owner 边界补充一条拒绝日志，携带真实 `itemInstanceId` 与首个 `diagnostic`；不新增第二套校验或防御分支。
- Runtime 离线全量编译通过；对应 Item Interaction Editor focused compile 通过。旧 Editor 全量离线清单仍包含与当前 I031/Canonical API 不一致的历史验证器，未在本修正中扩域处理。
- “奖励没有伤害”尚未修改：当前玩家证据未携带该奖励的 `baseItemId / combatEffect / placed+lit`，而非伤害道具本来就不应临时解释成伤害。下一次正常路径只用真实奖励身份及现有 state sink 证据判断，不猜数值。
- 状态：`REAL_CARD_CLICK_BOUNDARY_FIXED / RUNTIME_COMPILE_PASS / FOCUSED_EDITOR_COMPILE_PASS / BATTLE_MUTATION_UNRESOLVED / PLAYER_RETEST_PENDING`。
