# Project Cleanroom Component Ledger

## Core Battle Readability Decor Audit - 2026-08-14

- Existing Formal player/enemy/detail Prefabs are the accepted carriers and will be extended in place; new Scene or replacement HUD/popup count remains `0`.
- Player carrier already has avatar and HP; enemy carrier already has visual identity and HP; Guard, Shell and Formal status strips are the first missing visual layer.
- Exact Detail already has a complete authored header/artwork/scroll/section hierarchy; the next work is player reading order and empty/debug visibility, not rebuilding the popup.
- BattleSandbox status hierarchy is `VISUAL_ONLY_REFERENCE`; V02 status runtime/controllers remain `DO_NOT_MIGRATE`.
- Status: `DECOR_AUDIT_COMPLETE / CONTRACT_FROZEN / EXTERNAL_UNITY_OWNER / PREFAB_WRITES_NOT_STARTED / NOT_READY`.

## Core Battle Readability Water Contract - 2026-08-14

- `C1FormalRealtimeBattleSessionStateSnapshot`: `KEEP / FORMAL_OBSERVABILITY_SOURCE`. It now exposes player and enemy current statuses alongside the existing HP, Guard and Shell state; ownership remains in the live Session.
- `C1FormalItemDetailProjectionAndSelection`: `KEEP / CANONICAL_PLAYER_SEMANTIC_SOURCE`. It derives readable effect, trigger, condition, target and stat text from the one Canonical definition plus rolled values; runtime Item-ID text routing is absent.
- `C1ExactBattleSandboxItemDetailPopupStandalone`: `KEEP / FORMAL_DETAIL_VISUAL_CARRIER`. No Prefab or presenter change was made in the WATER package.
- `FormalBattlePlayerPresentation` / `FormalBattleEnemySlot`: `KEEP / FORMAL_VISUAL_CARRIER`. They remain the intended DECOR carriers for portrait, HP, Guard/Shell and Buff/Debuff readability.
- V02 BattleSandbox HUD hierarchy: `VISUAL_ONLY_REFERENCE`; V02 status runtime/controllers: `LEGACY_RUNTIME / DO_NOT_MIGRATE`.
- Runtime offline compile: `PASS / 0 errors / 2 pre-existing warnings`; focused Editor verifier source compile: `PASS / 0 errors / 0 warnings`.
- `PRESENTER_EVENT_CHANGED_ONLY_GATE_MISSING`: `VERIFIER_FALSE_POSITIVE / FIXED_IN_PLACE`. Presenter behavior already enforced null, accepted and changed gates in order; only the brittle same-line source assertion was updated. Runtime behavior was not changed.
- `VISIBLE_DETAIL_PROJECTION_REJECTED I001`: `INVALID_EDITOR_FIXTURE / FIXED_IN_PLACE`. The old fake Sprite had no Unity native object; the verifier now supplies a real transient in-memory Sprite. Formal Runtime artwork validation remains unchanged.
- User-owned Unity evidence: `Canonical Item Detail Player Semantics Focused Verify = PASS / assertions=128`.
- User-owned Unity evidence: `Canonical Effect Operator Focused Verify = COMPLETE / checks=43`.
- Fresh Play: `NOT RUN`.
- Status: `WATER_COMPLETE / ITEM_DETAIL_FOCUSED_PASS_128 / BATTLE_STATUS_FOCUSED_PASS_43 / DECOR_READY / NOT_READY`.

## Canonical Application Feedback And Float Reliability - 2026-08-14

- `C1FormalRealtimeBattleSession`: `KEEP / APPLICATION_RESULT_FEEDBACK_OWNER`. Real state mutation remains authoritative; generic feedback reports only the truthful requested/applied result. Zero-benefit Heal and Cleanse are readable without fabricating HP or status mutation.
- `C1FormalRealtimeBattleCueKinds.ApplicationFeedbackPayload`: `KEEP / GENERIC_SEMANTIC_FEEDBACK_CONTRACT`. One cue family covers Heal, Guard, Cleanse, Nian, Control and Shell semantics without Item-ID routing or a second ledger.
- Canonical effect damage: `FIXED / SAME_INSTANCE_DAMAGE_FLOAT_AFTER_REAL_COMMIT`. The prior HP/Shell mutation had no damage-float payload; it now emits through the existing causal presentation path.
- `FormalBattlePresentationRoot`: `KEEP / EXISTING_FEEDBACK_CONSUMER`. It routes the new generic payload to the already-authored pool and does not infer gameplay success.
- `FormalBattleDamageFloatPool`: `KEEP / BOUNDED_QUEUE_NO_ACTIVE_OVERWRITE`. Ten authored slots remain unchanged; a 32-entry wait queue and same-receiver stagger prevent active-slot replacement and exact-position overlap during ordinary bursts.
- Product values, Scene/Prefab/Profile, VFX grammar, Item Authority, Reward, Board/Tray, Stage, Save and Item Detail were not changed.
- Verification: Runtime offline compile `PASS / 0 errors`; focused Editor verifier source compile `PASS / 0 errors`; Unity/focused menu/Fresh Play `NOT RUN`. Status: `INTERNAL_CODE_CLOSE / EXTERNAL_FOCUSED_VERIFY_PENDING / PLAYER_READABILITY_RECHECK_PENDING / NOT_READY`.

状态日期：2026-08-11

## 当前结论

全项目 Cleanroom 已启动，并已对经过消费者审计的孤岛执行物理退休；全局清理仍未完成。后续仍坚持“先证明正式消费者已切换，再整岛删除”，不得把带有 Reward、Battle、Detail、初始获取或正式表现职责的结构按名称直接删除。

## 长期治理绑定｜2026-08-11

- `Docs/CURRENT/CLEANROOM_GOVERNANCE_BASELINE_V1.md` 已成为 Cleanroom 后续长期治理入口。
- 本 Ledger 同时承担 CURRENT Component Ledger 与 Owner Registry 的事实记录，不再建立第二套资产台账。
- 新增或替换 Owner 级结构前必须按治理基线执行 Owner Review；普通任务默认属于“装修”。
- `scripts/check_project_hygiene.ps1` 是只读 Hygiene Gate，只报告高信号风险，不自动修复或创建兼容层。
- 当前工程仍有已登记的 `REPLACE`、`QUARANTINE`、Pool15/Starter 与 Formal→Sandbox 债务，因此治理状态为 `ACTIVE`，工程状态不得宣称 `CLEAN`。

## BattleLike Preview Bridge Island Retirement｜2026-08-11

- 本段是该责任岛的 CURRENT 结论，覆盖本文后续 2026-08-09 的 `QUARANTINE / DELETE-CANDIDATE` 历史标签；旧行保留用于说明当时为何没有提前裸删。
- 已整岛退休旧 `BattleLikePreviewAreaBridge`：dev-only Host、只服务该 Host 的 Editor Builder、零正式消费者 Prefab、配套报告与检查表均已删除。
- Host 唯一运行字段为 `BuildGridInteractionPreviewController`；其声明明确为 `devOnly=true / formalFlow=false / save=false / reward=false / chapter=false`，不是正式 Board、Tray、Battle 或玩家表现 Owner。
- Host 脚本 GUID 只由该旧 Prefab 消费；Prefab GUID 在 Scene、Prefab 与 ScriptableObject 中无外部序列化消费者。
- `C1UnifiedSingleFormalPathAuthoring` 原本已把 Host、Builder 与 Prefab 列入删除清单；本次完成物理退休后，仅移除这些已经完成的待删除输入，不新增替代 Bridge 或兼容层。
- 历史 Assignment 和报告中的文字记录继续作为历史证据，不构成现行消费者。
- 目标结果：清除 Hygiene Gate 命中的首个 Formal→BuildSandbox 显式依赖；正式 Unified Scene、Exact Board/Tray/Card、Battle Session、Reward、Save 与 VFX 均未修改。
- 静态收尾：全局 Hygiene Gate 已返回 `RoomStatus: CLEAN`，现行 `Assets` 中不存在该 Host、Builder、Prefab GUID 或类型的消费者。
- Unity 编译证据：连续两次由本窗口持有的 batch Unity 均停在工程加载前，未创建项目锁或指定日志；已只停止对应自有进程，未触碰用户 Unity。该结果记为 `UNITY_BATCH_STARTUP_BLOCKED`，不得伪装成编译通过，也不改变本责任岛的静态退休结论。

## Unified Single Formal Path Authoring 收口｜2026-08-11

- `C1UnifiedSingleFormalPathAuthoring` 仍被正式 Loop 测试读取，并保留当前 Shell 的结构校验与两种既有 Item Detail 局部修复职责，因此本轮不整文件删除。
- 已移除该工具内全部文件摘要常量、摘要计算、摘要比较和摘要日志；输入保护改由现有路径、Prefab 来源、Hierarchy、Component、序列化字段与几何事实校验承担。
- 已停止通过旧底稿摘要进入“整页重建”分支；当前公共执行只接受已完成结构或两个能够被具体结构事实识别的局部修复状态，未知结构继续在该 Owner 处失败关闭。
- 已移除无实际类型消费的 `BuildSandbox` namespace 引用；未修改正式 Scene、Prefab、Battle、Reward、Item Authority、Save 或 Presentation 资产。
- 旧整页重建 helper 目前仍留在同文件内但已无执行入口，标签为 `REPLACE / HISTORICAL_AUTHORING_SUBGRAPH`。Owner：`C1UnifiedSingleFormalPathAuthoring`；Purpose：等待与仍读取其私有方法/源码的旧 Loop 测试一起收口；CreatedByTask：历史 REV11 authoring；DeleteWhen：相关测试不再以反射或源码文本绑定该 authoring，且当前 Shell 的结构校验入口已独立证明可用。
- Hygiene Gate 已新增项目 C# 禁用摘要逻辑汇总。当前仍有 `95` 个文件分布在 BattleBridge、BuildSandbox、Contracts、CrossSystem、Editor、EnemySystem、Items、V04，因此全局 `RoomStatus: WARNING`；后续必须按责任岛清理，不做全局字符串替换。

## V04 Presentation Hidden Automation Retirement｜2026-08-11

- 已退休 4 个旧 V04 Presentation/FreshPlay 验证器：`BattleSandboxCleanPlaySurfaceValidator`、`C1AuthoredMultiEnemyFreshPlayVerifier`、`C1AuthoredMultiEnemyPresentationVerifier`、`C1ThreeSceneJourneyPresentationVerifier` 及其 `.meta`。
- 四者没有当前代码调用者、菜单入口脚本、命令行脚本或序列化 GUID 消费者；其中前两者仍通过 `InitializeOnLoad` 在每次 Editor 加载时订阅 PlayMode、Scene dirty、update/delayCall，属于会持续污染编辑器生命周期的历史自动化。
- 旧 Presentation verifier 与 FreshPlay verifier 只构成内部二节点依赖；其余两个是独立入口。当前工程仍保留 `C1EncounterOwnershipAndVisualSlotHandoffVerifier`、`C1EncounterOwnershipFreshPlayVerifier` 与显式 `C1ThreeSceneJourneyHandtestMenu`，没有删除现行 Encounter/Visual Slot 或人工 Handtest 入口。
- 同批删除 11 份只由这些旧工具生成/读取的报告，以及一份旧 FreshPlay evidence 日志；历史 Assignment 不删除，继续说明当时需求。
- 本批未修改 Runtime、Scene、Prefab、Battle、Reward、Item、Enemy、Presentation 资产或 Save。静态类型/路径引用归零；Hygiene Gate 中 Editor 禁用摘要文件由 `44` 降至 `40`，全局由 `95` 降至 `91`。

## Standalone Sandbox Evidence Verifier Retirement｜2026-08-11

- 已退休 3 个无菜单入口、无自动加载、无当前代码/脚本/序列化消费者的独立旧 Verifier：`LayoutResilienceBattleSandboxPlaytestAdapterVerifier`、`I031InventoryPlacementAndLightingContractVerifier`、`ItemPlacementUniqueBaseRuleVerifier`，以及它们独占的 12 份旧报告。
- 对应 Runtime Adapter、Canonical Item 放置规则、5x5 Board、Tray、Reward、Battle、Scene/Prefab 和 Presentation 均未修改；本批只去除已经失去当前证明职责的 Editor 证据壳。
- Hygiene Gate 中禁用摘要实现的 Editor 文件由 `40` 降至 `37`，全局由 `91` 降至 `88`。

## Real Layout Resilience Legacy Evidence Generator Retirement｜2026-08-11

- 已退休无菜单、无自动加载、无当前任务/脚本/序列化消费者的 `RealLayoutResilienceEvaluationPipelineVerifier`，以及它独占生成的 6 份历史报告。
- 正式 `DefaultRealLayoutResilienceEvaluationPipeline` 与其 Runtime primitives/validation 保留；当前 Board Adapter 与 LayoutResilience Playtest Adapter 对它的真实代码依赖未改变。
- `ItemEnemyMatchupMatrix.csv` 是该旧 Verifier 的输入而不是独占输出，继续保留；历史 Assignment 继续作为历史需求记录，不构成现行消费者。
- 本批未修改 Runtime、Scene、Prefab、ScriptableObject、Item/Enemy 数据、Reward、Battle、Save 或 Presentation。Hygiene Gate 中禁用摘要实现的 Editor 文件由 `37` 降至 `36`，全局由 `88` 降至 `87`。

## Enemy Validation Connected Evidence Island｜2026-08-11

- 已完成整岛消费者图审计并退休 25 个 Editor source/`.meta` 与 130 份仅由该岛生成、读取或保护的历史报告；删除范围覆盖 Enemy Validation 旧聚合链及其 CrossSystem ItemEnemy/LayoutResilience 旧证据链，没有保留失去执行入口的叶节点。
- 明确保留 `BoneSwapRemnantRuntimeOperatorVerifier`、`C1Lv1EarlyEncounterBalanceVerifier`、`C1ShatteredHostPorcelainHoundRuntimeVerifier`，以及 Runtime Enemy Normalization、SystemSnapshot、SeedData、LayoutResilience Runtime 与 BuildSandbox adapter。`DevEncounterSeedPressureWindowRows.csv`、`ItemEnemyMatchupMatrix.csv` 继续由真实消费者持有；ItemInstance Placement Binding 已改由原位行为验证器直接证明，不再依赖历史报告。
- 删除后已删类型的当前代码、脚本与序列化引用为 0，受影响目录孤立 `.cs.meta` 为 0；Hygiene Gate 的旧完整性门禁实现由全局 87 降至 65、Editor 36 降至 14，工程仍为 `WARNING`，不得宣称 Cleanroom 已完成。
- 为完成当前源码编译核对，原位补齐 `C1UnifiedSingleFormalPathAuthoring` 对现有 `TalismanBag.BuildSandbox` 命名空间的缺失引用；没有改变 Runtime 或玩家逻辑。当前文件系统枚举的 580 个 Runtime C# 与 181 个 Editor C# 均离线编译通过，仅保留两条既有未使用字段警告。
- 终态：`ENEMY_VALIDATION_CROSSSYSTEM_EVIDENCE_ISLAND_RETIRED / INTERNAL_COMPILE_PASS / UNITY_NOT_RUN / PLAYER_PATH_NOT_READY`。下一责任岛为仍在运行的 Formal Battle/Enemy Verifier 基础设施；必须继续按消费者图审计，不得从单个报告或单个类名开始裸删。

## 工程规模快照

| 范围 | 当前数量 | 备注 |
| --- | ---: | --- |
| Runtime C# | 580 | 不含 `Editor/**`；文件系统枚举包含被忽略规则隐藏但仍在磁盘的 4 个 `Items/Build` 权威源文件 |
| Editor C# | 181 | 已退休一次性 GuardFix、报告型 Verifier、隐藏 FreshPlay 自动化、旧实验室门禁、孤立 Builder 与 Enemy/CrossSystem 旧证据岛 |
| BuildSandbox Runtime C# | 100 | 尚未证明全部可删；正式路径不得依赖其运行图 |
| ItemSandbox Runtime C# | 21 | 作为 Canonical 消费型实验室审计 |
| Scene | 8 | 启用状态另以 Build Settings 与 CURRENT 工程快照为准 |
| Prefab | 19 | 包含 Exact/Formal Item 与 Unified/Battle 表现载体 |
| Resources 非 meta 文件 | 827 | 图片与表现资源仍须按加载键和消费者审计，不按名称批删 |
| ScriptableObject 非 meta 文件 | 0 | 旧 `Assets/_Game/ScriptableObjects` 目录已按已批准范围退休 |
| Config 非 meta 文件 | 57 | Workbench 31、BuildSandbox 25、ItemDetail 1 |
| asmdef | 0 | 当前没有清晰 Runtime/Editor 编译边界 |

## Enabled Build Scenes — KEEP UNTIL PROVEN OTHERWISE

1. `Scene_TalismanBag_V03_BootEntry`
2. `Scene_TalismanBag_V03_MainHome`
3. `Scene_TalismanBag_V04_WorldMap`
4. `Scene_TalismanBag_V03_TalismanUpgrade`
5. `Scene_TalismanBag_V02_FormationCounter`
6. `Scene_TalismanBag_V04_UnifiedBattlePageShell`

未启用 Scene：

- `Scene_TalismanBag_V04_BattleSandboxPreview` — `QUARANTINE / DEV LAB`
- `Scene_TalismanBag_V04_ItemSandbox` — `QUARANTINE / DEV LAB`
- `Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab` — `QUARANTINE / PRODUCT LAB`

未启用不等于可删除；必须先证明没有保留的设计/验证职责。

## Subsystem Ledger — First Pass

| 责任岛 | 标签 | 当前判断 | 删除前条件 |
| --- | --- | --- | --- |
| Canonical Item catalog/factory/drop/acquisition | `KEEP` | 唯一新真相地基 | 保持唯一输入输出，不向下游暴露 Workbench/InnerCatalog |
| Existing Reward Session/Grant/Entitlement shells | `KEEP + REPLACE` | 外壳可复用，旧 Pool15 overload 仍在 | Canonical grant 成为唯一正式调用 |
| Formal Item Authority / 5x5 Board / Tray / Card | `KEEP + REPLACE BOUNDARY` | 玩家领取和摆放已证明有价值 | 删除固定 I001/I002 与旧 combat hydration |
| `C1FormalRealtimeBattleSession` | `KEEP + REPLACE FEEDER` | 唯一 live loop 和真实状态槽 | 移除 Pool15 envelope/operator 私有路径 |
| Unified Battle Scene / shell / bridge | `KEEP` | 正式玩家载体 | 不允许 Sandbox runtime graph 回流 |
| Formal Battle/Item presentation Prefabs and VFX | `ASSET-KEEP` | 保留已完成画面与共享表现 | 数据只读同一 ItemInstance 与 mutation cue |
| Enemy/StageConfig/Save | `KEEP / PROTECTED` | 不属于当前清退根因 | 不在清理中顺手改写 |
| BattleSandbox / ItemSandbox runtime | `QUARANTINE` | 实验室仍可能有调试价值 | 必须改为 Canonical 消费者；不得成为正式 Owner |
| V02/V03 runtime and ScriptableObjects | `AUDIT REQUIRED` | 仍承载 Boot/Home/Upgrade/Formation，不能按版本号删除 | 逐 Scene 证明正式职责或替代关系 |
| Custom Editor verifiers/report writers | `AUDIT REQUIRED` | 价值不一且缺少标准入口 | 保留证明真实 Owner 的最小集合 |

## Item Historical Structures — DELETE-CANDIDATE

以下 8 个主要候选文件当前全部仍存在，且尚有 Runtime 消费者，因此本轮没有提前删除：

1. `Items/CampaignLoot/C1CampaignLootEligibleItemPool15Carrier.cs`
2. `Items/CampaignLoot/C1CampaignPool15BaseCombatFacts.cs`
3. `BattleBridge/CampaignLoot/C1CampaignLootPool15BattleEffectFamilyFoundation.cs`
4. `BattleBridge/CampaignLoot/C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.cs`
5. `Items/CampaignLoot/C1CampaignPool15BuildEnvelopeSimulator.cs`
6. `Items/CampaignBaseline/C1Lv1StarterItemBaseline.cs`
7. `Items/CampaignBaseline/C1FixedFirstClearItemInstanceCarrier.cs`
8. `V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs`

当前直接引用规模：

| 旧入口 | Runtime 文件数 | Editor 文件数 |
| --- | ---: | ---: |
| Pool15 eligible carrier | 10 | 9 |
| Pool15 base combat facts | 7 | 7 |
| Pool15 effect family | 5 | 3 |
| Starter baseline catalog | 11 | 11 |
| Fixed first-clear carrier | 4 | 7 |

至少 18 个 Editor 文件仍直接依赖旧 Item 真相。它们必须在正式消费者切换后删除或改写，不能继续作为 Canonical 完成证据。

## Immediate Execution Order

1. 完成 Item 责任岛消费者切换并达到正式 Runtime 零 Pool15/Starter 引用。
2. 删除上述 8 个私有历史结构及只服务它们的测试。
3. 核对 Exact/Formal Item Prefab 双组真实挂载，先统一职责再删重复载体。
4. 核对 `Resources/item` 与 `Resources/item_daoju` 的加载键、序列化引用和图片覆盖，不按名称或文件大小猜测。
5. 逐 Scene 审计 V02/V03、Sandbox 与 Product Lab。
6. 收敛 Editor 工具并更新工程事实快照。

## Scene / Prefab Mount Audit — 2026-08-09

### 正式嵌套链（`KEEP / ASSET-KEEP`）

`Scene_TalismanBag_V04_UnifiedBattlePageShell`
→ `UnifiedBattlePageShell.prefab`
→ Exact Prepare / Navigation / CombatLog / Background / Detail Popup
→ Exact Board / Tray / Card
→ `ItemRarityContourBloomVfx.prefab`

同一个 Shell 还直接承载 Formal Battle Presentation Root、Cue/Audio、Damage Float；Presentation Root 再承载 Player 与 Enemy Presentation。以上 Prefab 都有可追溯的正式序列化链，禁止按名称中的 `Sandbox` 误删。

### 重复或孤立候选

| Prefab 责任岛 | 当前引用事实 | 标签 |
| --- | --- | --- |
| `C1FormalItemBoard` → `C1FormalItemCard` | 未挂入任何 Scene；仍被旧 Editor Authoring 引用 | `REPLACE / DELETE-CANDIDATE` |
| `ItemTrayArea` → `C1FormalItemTray` → `C1FormalItemCard` | 整个入口 Prefab 在全部 `Assets` 中无序列化引用、无代码路径引用 | `DELETE`（入口已删除；下游继续审计） |
| `ItemDetailPanel.prefab` | 未被 Scene/Prefab 引用；大量旧 Editor verifier 引用；正式 Shell 使用的是 Exact Popup | `QUARANTINE / DELETE-CANDIDATE` |
| `BattleLikePreviewAreaBridge.prefab` | 历史判断：当时尚有旧 Builder/Verifier；现已完成整岛退休 | `RETIRED / 2026-08-11` |
| `DraggableTalismanItem.prefab` / `TalismanGridSlot.prefab` | 资产本身零序列化引用，但启用的 V02 场景仍含同类组件，且旧 Scene Builder 仍引用路径 | `KEEP UNTIL V02/BUILDER AUDIT` |

### Scene 脚本事实

- 6 个启用 Scene 的直接脚本合计覆盖 57 个项目脚本；Unified Scene 的行为主要通过嵌套 Shell Prefab 引入，不能只统计 Scene YAML。
- 3 个未启用实验 Scene 独占 40 个直接挂载脚本；这些脚本先标 `QUARANTINE`，仍需排除正式 Prefab/代码调用后才能删除。
- Exact Item Detail、Inner Catalog 与 Exact Arrangement 部分脚本虽也出现在实验 Scene，但同时由正式 Shell Prefab 使用，属于正式消费者，不可按“dev-only scene”误删。

## Physical Retirement Log

1. `Assets/_Game/Prefabs/TalismanBag/Items/ItemTrayArea.prefab` 及 `.meta`
   - 删除理由：全 `Assets` 序列化引用为 0；精确代码路径引用为 0；正式 Shell 使用同名槽位而非该 Prefab。
   - 影响边界：不改 Scene、Shell、Board、Tray Runtime、VFX 或 Item 数据。
2. `Assets/_Game/Resources/item/**/.DS_Store` 共 16 个
   - 删除理由：macOS Finder 元数据，不是 Unity 资产，没有 `.meta`、加载键或玩家内容职责。
3. `Assets/_Game/Resources/item_daoju/**/.DS_Store` 共 33 个
   - 删除理由：同上；31 件真实道具图片与目录结构全部保留。
4. 其余 `Assets/_Game/Resources/**/.DS_Store` 共 4 个
   - 删除理由：同上；战斗页 UI 与动画资源全部保留。
5. `ItemBalanceCandidateSeedBuilder.cs.orig` 与 `ItemCompleteCandidateContent.cs.orig` 及其 `.meta`
   - 删除理由：补丁/冲突备份；现行 `.cs` 均存在，备份内容已分叉，工程内无引用且不参与 C# 编译。
6. Reward 旧 Pool15 选择责任岛
   - `C1CampaignDirectLootPoolAndPolicy.cs` 已收敛为 Canonical Catalog/Drop 的唯一正式入口；固定 15 名单、旧 Pool Snapshot、第二套选择算法和签名实现已删除。
   - `C1CampaignDirectLootSessionResolver.cs` 保留现有兼容字段，但身份值改为可读稳定文本，不再计算哈希。
   - 删除 1271 行、调用已过期 API 的旧 Pool15 确定性测试；Canonical Foundation 与正式 Reward/Entitlement 验证继续保留。
7. Pool15 Build Envelope 分析岛
   - 删除 834 行旧 Build Envelope Simulator、761 行配套测试，以及 1443 行仅消费该分析结果且没有调用者的 Stage 3–5 Editor Verifier。
   - Enemy 正式数据、StageConfig、Encounter Truth 与运行时数值未修改。
8. BuildSandbox 一次性 GuardFix / 报告型 Editor 工具
   - 删除 5 个只引用自身的 `GuardFixVerifier`，不删除任何 Board、Tray、Card、Scene、Prefab 或运行时组件。
   - 删除旧全道具可见性报告器、一次性 Tray 方向 Authoring、旧 DevSession roster 测试。
   - 删除 4 个零外部调用的旧 Item Detail / Build Track / 四核心迁移 / BattleSandbox Board Adapter 验证器。
   - 本批共退休 12 个 Editor C# 及其 `.meta`；正式 Shell、Exact Popup、共享 Item Card 与两套 VFX 均未修改。
9. ItemSandbox 旧详情/修正门禁
   - 删除两份冻结 Item Detail 模板验证器、旧四核心觉醒修正总门禁、旧 ItemSandbox Board/Detail Adapter 验证器。
   - 4 个文件均为 Editor-only 且零外部调用；31 件 Canonical 规则、I031、正式 Authority、Board/Tray/Card 与运行时数据未删除。
10. 项目根目录临时编译产物
   - 删除 `TempCodexCompile` 与 `TempCodexFocusedTests`，合计约 131 MB。
   - 两目录只含离线 runner、响应文件、DLL/PDB、`bin/obj` 与临时 SDK 状态；Assets、Packages、ProjectSettings、Docs 均无正式引用。
   - `Archives`、`Builds`、`Logs` 未跟随删除：它们分别按产品存档、交付包、诊断证据独立审计，禁止用“都是生成物”一刀切。
11. 旧 Sandbox APK 与 V03 编译二进制
   - 删除 `Builds` 中仅有的 BattleSandboxPreview 与 ItemSandboxVisual APK，共约 453 MB；它们不是正式玩家交付包。
   - 删除 `Logs/V03Compile` 与 3 个根级 V03 MainHome DLL/PDB 编译副本；文本日志、截图、CSV 仍保留。
   - 相关 Editor Builder 只写这些输出路径，不读取现存 APK/DLL；Builder 是否保留将在 Tooling 波次单独判断。
12. 旧 Pool15/固定初始物品 Entitlement Acceptance 测试
   - 删除 `C1FormalCampaignLootEntitlementAcceptanceTests` 及其 `.meta`，并从 Promotion 测试移除唯一一个重复调用。
   - 该文件写死旧 Pool15 与固定 I001，且预处理边界错误导致 Editor 离线编译失败；没有给它补兼容引用。

## Resource Responsibility Audit

- `Resources/item`：详情 UI 的图标、背景、分割线与状态视觉，正式 Detail View 通过 `item/...` 加载键消费，标记 `KEEP`。
- `Resources/item_daoju`：31 件道具本体图片与聚念石状态图，正式详情/卡牌投影通过 `item_daoju/...` 加载键消费，标记 `KEEP`。
- 两个目录名称相近但职责不同，不属于重复数据库；不得为了目录收口而合并。
- 两个目录内共 49 个 `.DS_Store` 已全部删除；全 `Assets` 另 4 个同类文件也已删除；Unity 图片与加载键未改动。

## Reward / Item Single-Source Audit — 2026-08-09

- 正式 `C1CampaignDirectLootSessionResolver` 已只通过 `CanonicalItemDropPolicy` 选择奖励。
- Reward 入口中的固定 15 名单、旧 Pool Snapshot 与第二套选择算法已删除；兼容身份字段只保存可读稳定文本。
- 旧 Policy/Selection 专用测试已删除；Canonical Catalog、Drop Policy、Grant 与 Entitlement 外壳保留。
- 其他责任岛仍有旧完整性摘要/门禁实现；它们只能逐 Owner 原地收口，不能按关键词批量删除。

## Formal Battle Adapter Cutover Audit — 2026-08-09

- Start 与 paused refresh 的正式入口都已调用 `TryTranslateCanonicalItemSnapshot`，直接消费同一 `ItemGeneratedInstanceSnapshot` 与 `CanonicalItemDefinition`。
- 旧 `TryTranslateCumulativeItemSnapshot`、Pool15/固定 I001/I002 投影助手、旧快照重算与第二投影构造器已经删除。
- 删除后 Runtime 与 Editor 离线全编译均为 0 错误；正式 Canonical Start/Refresh、Session contract 与兼容请求字段保持不变。
- Adapter 内仍有固定 I001/I002 时调用确定性 Battle Oracle 的旧分支；它会改变 1-1/1-2 的终局 Owner，不能作为普通死代码删除。
- 下一决策点：是否授权 1-1..1-5 全部只以 Live Session 为终局真相，并同步删除 Unified Loop/Host 的旧 BattleAdapter 依赖。

## Deterministic Battle Oracle Island Retirement — 2026-08-09

- `C1FormalRealtimeBattleSessionRequest` 与 `C1FormalRealtimeBattleTerminalResult` 已移除旧 Oracle 结果与 Canonical 字段；1-1..1-5 的正式终局只由 Live Session 负责。
- 1-1/1-2 的关卡与敌人身份改由 `C1Lv1EarlyEncounterBalanceContract` / `C1Lv1EarlyEncounterBalanceCatalog` 提供；道具身份继续只读 Canonical Item Catalog，没有新增兼容层或第二套数据库。
- `buildPerformanceSummary` 由 Live Session 的真实时长、道具应用次数与敌方行动次数生成，不再复制 Oracle 摘要。
- 已物理删除旧岛 4 个实现文件及其 `.meta`：`C1Lv1FormalBattleApplicationAdapter`、`C1Lv1FormalBattleApplicationEngine`、`C1Lv1FormalBattleApplicationContracts`、`C1Lv1FormalBattleApplicationVerifier`。
- 全项目 C# 扫描对旧类型、旧请求/终局字段、旧 Oracle 错误码与 parity 标记均为零命中。
- 删除后离线全量编译：Runtime `0` 错误，Editor `0` 错误；保留 2 条与本波无关的既有未使用字段警告。
- 本波状态：`CODE_CLEANROOM_COMPLETE / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Offline Compile Gate — 2026-08-09

- 使用 Unity 安装目录自带 Roslyn 编译器与现有 Bee 引用/define，重新枚举当前 Assets 源码；输出只写 Codex 外部工作区临时目录，不启动 Unity、不写 Library。
- 当前结果：Runtime `0` 错误，Editor `0` 错误；仅有 2 条既存未使用字段警告。
- 该门可用于后续每个 Runtime 收口波次；它不能替代 Unity Fresh Play，但能阻止删除后留下悬空类型或调用。

## Canonical Battle Truth Correction — 2026-08-10

- 更正 I007 证据：原聚焦验证器通过 `ItemCompleteCandidateContentSeed` 在内存中构造定义，并未读取正式 `ItemBalanceWorkbenchCatalog.asset`。正式 `ItemBalanceProfile_I007.asset` 当时仍缺少完整 status 字段，因此此前“正式数据已闭合”的结论过早。
- 已把 `I007_STATUS_PLAYTEST_V1` 语义写入正式 Profile，并把聚焦验证器改为从真实 `ItemBalanceWorkbenchCatalog.asset` 创建 `CanonicalItemDefinitionResolver` 后读取 I007；不再由 Editor seed 伪造正式战斗定义。
- 正式 Adapter 的真实 feeder 已确认只读取累计快照中的同一 `ItemGeneratedInstanceSnapshot + CanonicalItemDefinition`；`pool15ActiveItemFacts` 在正式路径恒为空。旧 Pool15 application ledger 不是正式玩家链证据。
- `C1FormalObtainRebuildBattleLoopTests` 的 1-3 聚焦口径已从“旧 Pool15 ledger 出现非零记录”改为“同一奖励 `ItemInstanceId` 产生非零 HP/护壳真实 mutation，并带同一 accepted event cue”。Tray 中未 placed+lit 的实例只按真实 cue 验证不执行。
- 当前离线全量编译：Runtime `0` 错误，Editor `0` 错误；未启动 Unity，未执行 Fresh Play，因此仍为 `INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。
- `ItemCompleteCandidateContentSeed` 及其 Editor seed/builder 仍是 `REPLACE / DELETE-CANDIDATE`；只有在其余 Editor 消费者完成切换后才可物理删除。
- 旧 Pool15 自动效果分支仍保留。直接删除会同时移除额外目标、护壳修正、念返还、行动延迟等玩家行为；必须先由 Canonical operator 提供等价真实 state sink 和覆盖证据，不能以“正式 feeder 为空”冒充零行为风险。

## Current Status

- Cleanroom contract: `ACTIVE`
- Component ledger: `SCENE_PREFAB_PASS_COMPLETE / TOOLING_WAVE_1_COMPLETE`
- Physical deletion: `WAVE_1_ACTIVE — isolated Prefab, stale analyses and 16 obsolete Editor tools retired`
- Unity compile/Fresh Play: `NOT RUN`
- Player milestone: `NOT_READY`

## Live Session Terminal Cutover — 2026-08-09

- `C1FormalRealtimeBattleSessionAdapter` 已删除固定 I001/I002 识别与确定性 Oracle 执行分支；正式 Start 现在无条件把 Canonical Item Combat Facts 送入同一个 Live Session。
- `C1FormalObtainRebuildBattleLoop` 已移除旧 `C1Lv1FormalBattleApplicationAdapter` 字段、构造参数、启动校验和 Reset 生命周期；胜负阶段只由 `C1FormalRealtimeBattleTerminalResult` 推进。
- Loop 已移除 `StageOneResult`、`StageTwoResult` 与 `CurrentResult` 旧结果接口；奖励接纳继续读取同一 Live Terminal，不再读取 `oracleApplicationResult`。
- `UnifiedBattleFormalSceneHost` 已移除旧 BattleAdapter 与 ApplicationResult 缓存，公开 `RealtimeTerminalSnapshot`，结算展示只读取 Live Terminal 的时长、玩家 HP 与敌方行动次数。
- Loop 与 Host 的直接调用方、Presentation Authoring 和相关测试已经同步到新签名；旧“1-2 必须存在固定 I002”测试已删除，实时终局断言已改读 Live Terminal。
- 离线全量编译：Runtime `0` 错误，Editor `0` 错误；仅保留两条既有未使用字段警告。Unity/Fresh Play 未运行，因此玩家状态仍为 `NOT_READY`。
- 旧 `C1Lv1FormalBattleApplicationAdapter`、Engine、Contract 与专用 Verifier 已在独立零消费者审计后物理删除；正式战斗不再保留 Oracle 兼容入口。
- Formal Item Authority 仍含固定 starter / first-clear carrier 引用。它们不再影响 Battle terminal Owner，但仍属于后续“初始道具与剧情规则从掉落数据库分离”清理边界，不能与本次战斗真值切换混删。

## Initial Acquisition / First-Clear Island Retirement — 2026-08-09

- 本节覆盖并纠正上方“Formal Item Authority 仍含 fixed first-clear carrier”的旧状态：正式初始获取已由 `CanonicalInitialItemAcquisitionPolicy` 从唯一 Canonical Catalog 选择并生成，不再读取固定 I001 实例常量。
- 正式胜利奖励继续沿用 `C1CampaignDirectLootSessionResolver` → `C1FormalItemSessionAuthority.AcceptCanonicalCampaignLootEntitlement`；Reward、Tray、Board 与后续 Battle 保持同一个 `ItemInstanceId`。
- 已物理删除旧首通奖励孤岛及专属验证器（含 `.meta`）：
  - `Items/CampaignBaseline/C1FixedFirstClearItemInstanceCarrier.cs`
  - `V04/RewardDrop/Data/FirstClear/C1Stage1FirstClearGuaranteedRewardData.cs`
  - `Editor/ItemCampaignBaseline/C1FixedFirstClearItemInstanceCarrierTests.cs`
  - `Editor/V04/RewardDrop/C1Stage1FirstClearGuaranteedRewardData01Verifier.cs`
- `C1FormalItemSessionAndArrangementAuthorityTests` 没有整文件删除：其中有效的 Authority、Tray、Board、Reset 与幂等覆盖已迁移到 canonical initial-acquisition / direct-loot API；Promotion 门改用新的 `C1FormalCanonicalItemSessionAuthorityTests`，验证初始获取、奖励一次、同实例入 Tray、重复领取不新增以及 Reset 仅清本局奖励。
- 全项目 C# 扫描对 `C1FixedFirstClearItemInstanceCarrier`、`C1Stage1FirstClearGuaranteedReward`、`C1FormalItemEntitlementCommand`、`ConsumeEntitlement`、`FixedI001ItemInstanceId` 与 `FixedI001InstanceCanonicalSignature` 均为零命中。
- `C1Lv1StarterItemBaseline.cs` 本轮未删除，标签仍为 `REPLACE`：它仍被 `C1CampaignPool15BaseCombatFacts`、`C1FormalItemDetailProjectionAndSelection`、`C1FormalItemPresentationCatalog` 与 Authority action-cost 投影直接消费。必须先逐个把这些真实消费者切到 Canonical Definition/Combat Fact，不能先删文件再补兼容层。
- 离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误。没有启动 Unity，也没有执行 Fresh Play。
- 本波状态：`INITIAL_ACQUISITION_FIRST_CLEAR_ISLAND_RETIRED / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。
- 下一清理目标：沿 Battle 的 Basic State Operator 真链，把现行 Pool15 effect/fact 私有入口逐族切到 Canonical Combat Fact 与真实 Live Session state sink；在消费者归零前不删除 `C1CampaignPool15BaseCombatFacts`、effect-family foundation 或 Starter baseline。

## Canonical Effect / Legacy Materialization Audit — 2026-08-09

- 正式 `C1FormalRealtimeBattleSessionAdapter` 已只把同一 `ItemGeneratedInstanceSnapshot + CanonicalItemDefinition` 投递为 `directItemFacts`；正式投影中的 `pool15ActiveItemFacts` 恒为空。旧 Pool15 event/foundation 不是正式 feeder。
- 旧 Pool15 event resolve 的大多数分支只写 `effectState` 与 cue；只有 `player.guard` 和行动进度会进入 Live Session 真实状态。该路径不能作为 Canonical effect 接管证据，也不能修补后继续承担正式职责。
- Canonical rolled `damage / nianCost / cooldown` 已进入统一 Item Action；现有 Canonical Basic Mutation 还可提交 `break / guard / heal` 到敌方壳、玩家护势与生命。
- effect grammar 的第一个明确缺口位于 I007：定义要求 `burn` 层数、持续回合和后续 tick，但当前 `MutableActor` 只有 HP、shell 与 shell-broken，Canonical 数据也没有给出基础 burn tick 伤害公式。按合同停止该 effect/status 子岛，不猜数值，不把 detached `effectState` 当真实状态。
- I001–I006 的现有 HP/shell/Nian/行动/目标槽可复用；I007 及后续 burn/debuff/flaw/cleanse/ember 需要先有明确 status owner 与状态转移公式。完整 successor 出现前，`C1CampaignPool15BaseCombatFacts`、effect-family foundation 与 Session 旧 Pool15 类型保持 `REPLACE / DELETE-CANDIDATE`，不得提前删除。
- 已物理删除无正式消费者的旧平行 Reward 物化岛（含 `.meta`）：
  - `Items/CampaignLoot/C1CampaignLootItemInstanceAndEntitlementCarrier.cs`
  - `Editor/ItemCampaignLoot/C1CampaignLootEligibleItemPool15CarrierTests.cs`
- 已物理删除只由自身菜单入口运行、零外部调用的旧 Starter 报告器（含 `.meta`）：
  - `Editor/ItemCampaignBaseline/C1Lv1StarterItemBaselineVerifier.cs`
- 同步从旧 Pool15 dependency inventory 移除已删除 materialization request/result contract 标识；现行 Canonical direct-loot、Authority entitlement、Tray、Board 与 Battle 输入未修改。
- 删除后全项目 C# 对旧 materialization / entitlement 类型与专用测试均为零命中。离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误；Unity/Fresh Play 未运行。
- `C1FormalItemOrdinaryInstanceSnapshot.combatFact` 与 `C1FormalItemBattleItemRow.actionCostFact` 在当前正式 Adapter 中已不再作为 feeder，但它们属于受保护的 Authority 合同。本波没有强删字段：缺少外部 Fresh Play 时，字段级合同收缩的回归面高于当前清理收益；先标 `REPLACE / RETIRE AFTER INTEGRATION EVIDENCE`。
- 本波状态：`LEGACY_REWARD_MATERIALIZATION_ISLAND_RETIRED / CANONICAL_EFFECT_STATUS_GAP_NAMED / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。
- 下一独立清理目标：把 Formal Item Detail / Presentation 对 Starter 与 Pool15 combat catalog 的回查切换为同一 Canonical generated instance/definition；先减少真实消费者，再决定旧 BaseFacts 与 Starter baseline 的删除时点。

## Canonical Status Semantics Contract Wave — 2026-08-10

- `ItemCandidateEffectPayload` 与 `CanonicalItemEffectDefinition` 已增加通用状态语义字段：生命周期、重复施加策略、时间单位、首次 tick 延迟、tick 间隔、tick 伤害公式与数值、消耗、到期和净化规则。
- 这些字段属于唯一 Canonical Item Definition；没有新增 Pool15 数据库、第二 Authority、第二 Session、Bridge 或按 Item ID 分支。
- 未给任何现有道具写入新公式或新数值，因此本波不会改变现有 31 件道具的伤害与效果行为。
- Canonical 校验已增加“状态语义要么完全未声明，要么完整声明”的边界；不允许半套状态定义静默进入正式战斗。
- I007 仍为 `PRODUCT_SEMANTIC_GAP`：现有权威数据未定义 tick 伤害公式、重复施加、持续时间解释与净化语义；按最终策略不采用旧的 10% 候选，也不猜测新值。
- 离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误；未启动 Unity，未执行 Fresh Play。
- 本波状态：`CANONICAL_STATUS_CONTRACT_READY / PRODUCT_SEMANTIC_GAP_I007 / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。
- 下一门：先冻结 I007 的四项产品语义，再在现有 `C1FormalRealtimeBattleSession` 内实现通用 `BattleStatusState` 生命周期与同一 source instance cue；没有该语义前不写运行时猜测实现。

## Canonical I007 Live Status Closure — 2026-08-10

- `I007_STATUS_PLAYTEST_V1` 现在只存在于 Canonical Item 数据行：每次行动造成直接伤害并增加 1 层余焰，最多 3 层，共享 5 秒持续时间，首次 tick 延迟与 tick 间隔均为 1 秒；重复施加增加层数并刷新持续时间，但不重置 tick；单层 tick 为来源已结算直接伤害的 10%，最低 1。
- `C1FormalRealtimeBattleSession` 内新增一个通用 `BattleStatusState` 生命周期。运行时按 Canonical trigger、condition、target、policy 与 formula 选择 operator；Battle 中没有 `I007` 道具 ID 分支，也没有第二套状态系统。
- 每次 tick 先真实修改目标 shell/HP，再发出属于同一 `sourceItemInstanceId` 的 status、shell/HP 和 damage-float cue。同一时间戳固定为 Item action -> apply/refresh -> status tick -> expiry；目标死亡、到期或净化时整组移除。
- 新增聚焦可执行验证，覆盖 Canonical 数据、1→3 层增长、满层刷新但不重置 tick、真实敌方状态下降、来源 cue、最后一次 tick 先于到期，以及到期后完整移除。结果：`CANONICAL_STATUS_FOCUSED_PASS`。
- 当前源码离线全量编译：Runtime `0` 错误，Editor `0` 错误；仅保留 2 条与本波无关的既有 Runtime 未使用字段警告。未启动 Unity，也未伪造 Fresh Play。
- 旧 Pool15 类型/operator 仍标记为 `REPLACE / DELETE-CANDIDATE`；它们已经不是 Canonical 正式 feeder，但尚未达到零消费者，本波没有提前删除。
- 本波状态：`CANONICAL_I007_LIVE_SINK_INTERNAL_QA_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Legacy Editor Evidence Retirement — 2026-08-10

- 已删除 12 个零外部消费者、只验证旧实验链、旧一次性交付包或已完成迁移的 Editor 验证/改写工具及其 `.meta`：
  - `ItemCombatEffectRequestContractVerifier.cs`
  - `ItemCoreEffectIdentityAndRuntimeStateContractVerifier.cs`
  - `V04Chapter1ContinuousBattleIntegrationVerifier.cs`
  - `C1CampaignLootPool15BattleEffectFamilyFoundationVerifier.cs`
  - `ItemFourCoreCandidateDataCorrectionMigrator.cs`
  - `ItemFourCoreCandidateDataCorrectionVerifier.cs`
  - `DropRewardImmutableContracts01Verifier.cs`
  - `ItemShapeCatalogBatchCorrectionVerifier.cs`
  - `ItemBalanceCandidateSeedBuilder.cs`
  - `ItemCompleteCandidateContentWorkbenchVerifier.cs`
  - `ItemCapabilityUnitContractVerifier.cs`
  - `ItemCapabilityRuntimeFactContractVerifier.cs`
- 这些文件均未被当前 Runtime、正常玩家线路或现行正式工具执行；仅有旧 Assignment、Archive、旧报告文本或允许文件清单提及。其内部仍包含已被长期工程规则禁止的 SHA/ProtectedHashes 门禁，不能继续作为当前完成证据。
- 四核心迁移器与验证器只互相引用，且会从旧 Candidate Seed 重写正式 Profile；在 Profile 已成为权威数据后继续保留会产生误回写风险。Reward Immutable 验证器则是独立旧交付程序，Canonical Reward、Session Resolver 与现行聚焦测试均不调用它。
- 本波只删除旧证据生成器，不删除其仍可能被 BattleSandbox 视觉组件消费的运行时类型，也不删除 Scene、Prefab、VFX、Canonical Item、Reward、Authority、Board/Tray、Enemy、Stage 或 Save。
- 历史判断曾保留 `BattleSandboxCleanPlaySurfaceValidator` 与旧多敌/旅程 Fresh Play 验证器；在后续确认其当前代码、脚本、序列化消费者归零，且现行 Encounter Ownership/Visual Slot verifier 与显式 Handtest Menu 已存在后，已于 2026-08-11 按完整责任岛退休。该历史等待条件现已满足。
- 删除后离线全量编译：Runtime `0` 错误（保留 2 条既有未使用字段警告），Editor `0` 错误。未启动 Unity，未执行 Fresh Play。
- 本波状态：`LEGACY_EDITOR_EVIDENCE_WAVE_2_RETIRED / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Canonical Item Database / Seed Retirement — 2026-08-10

- `ItemBalanceWorkbenchCatalog.asset` 继续作为唯一 Item 数据库；`ItemBalanceWorkbenchCatalog.CatalogAssetPath` 与数据修订号现在由 Catalog 合同自身声明，不再依赖 Seed 类。
- Workbench 已移除“缺库自动创建”“从 Seed 重置”“Seed 生成建议区间”与会先执行旧生成器的 CSV Export 入口。缺少权威 Catalog 时明确报错并停止，不再静默生成第二套数据。
- 已物理删除 `ItemBalanceCandidateSeedBuilder` 与唯一会调用它的旧完整候选导出验证器；现行 Workbench Validation、Compiler、Preview、CSV Import Preview 和手工保存仍保留。
- 已从 Runtime 物理删除 500 余行 `ItemCompleteCandidateContentSeed` 第二真相。仍被正式 Profile/Catalog 序列化使用的六个纯数据合同迁入 `ItemCandidateContentContracts.cs`，沿用原脚本 GUID；全 `Assets` 无旧 GUID 外部序列化引用。
- 旧能力调查器 `ItemCapabilityMappingGapSurveyVerifier` 的证据读取已切到唯一 Catalog 资产，不再读取已删除 Seed 文件；其余仍写死旧路径的跨系统历史验证链标记为 `QUARANTINE / AUDIT AS A WHOLE`，不通过伪造新路径掩盖其 Git/哈希门禁和过期前提。
- 离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误。未启动 Unity，未执行 Fresh Play。
- 本波状态：`ITEM_SEED_SECOND_TRUTH_RETIRED / SINGLE_CATALOG_OWNER_ESTABLISHED / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Formal Item Detail Canonical-Only Cutover — 2026-08-10

- 正式普通 Item 详情的唯一数据链已收口为：当前 `C1FormalItemSessionSnapshot` 中的精确 `ItemInstanceId` → 同一 `ItemGeneratedInstanceSnapshot` → 同一 `CanonicalItemDefinition` → 详情 ViewModel 与战斗事实展示。I001–I030 不再回查 Starter baseline、Pool15 combat facts 或 Inner Catalog Provider。
- `UnifiedBattleFormalSceneHost`、`C1ExactBattleSandboxItemArrangementPresenter`、Exact Item Detail Presenter 与后续 Editor authoring 已移除 `IItemDetailViewModelProvider` / `ItemInnerDataCatalogProvider` 正式绑定要求；没有建立替代 Provider、第二 Authority、第二 Detail System 或 Item ID 分支。
- 正式详情 Seam 内已删除失去入口的 Starter / Pool15 私有校验与 Pool15 专用展示拼装逻辑。聚焦证据覆盖 I001、I007 与旧 accepted-15 范围外的 I016，要求显示名、实例身份、Canonical trigger/source 与全部 rolled stats 来自同一 Canonical 实例。
- 聚焦测试的 `RunAllOrThrow` 已只执行静态边界与 Canonical 普通 Item 详情验证；同文件内未再执行的旧 Provider/Pool15 测试辅助体仍标记为 `DELETE-CANDIDATE`，不作为正式消费者或本波通过证据。本轮没有为清测试体扩大 Runtime/Scene 写域。
- I031 特殊光源详情在 Phase 1 保持明确不适用；没有用普通掉落定义伪造特殊详情。现有 Scene 中可能残留的旧序列化 Provider 字段、Board artwork 的 Presentation Catalog 前置门与 Presentation Catalog 本体属于 Phase 2，未在本波修改 Scene、Prefab 或 Board。
- 离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误。聚焦测试源码已进入 Editor 编译，但由于本波不启动或控制 Unity，未执行 Unity 测试与 Fresh Play。
- 本波状态：`FORMAL_DETAIL_CANONICAL_ONLY_CUTOVER / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。下一独立边界是 Phase 2 Formal Presentation Catalog 迁移；在该阶段获批前不动 Board artwork 顺序、Scene YAML、VFX 或其他玩家系统。

## Canonical Presentation Identity Migration — 2026-08-10

- 方案 1 已执行：正式表现身份被定义为唯一 Canonical Item 数据库中的显式字段 `effectFamilyKey`、`presentationStyleKey`、`cueIdentity`；`CanonicalItemPresentationIdentityResolver` 只读取这些字段，不再根据法门、属性或效果类别猜测 VFX。
- 已从旧 `C1FormalItemPresentationCatalog` 逐项迁移 15 件可追溯的普通道具原值：I002、I003、I004、I007、I010、I011、I013、I014、I015、I022、I024、I025、I026、I028、I030。
- I001 的旧表现身份属于已废弃的“固定新手初始道具释放”规则，未迁入普通掉落数据；I031 仍是特殊点亮源，不并入 I001–I030 普通掉落表现合同。
- 权威数据缺口为 I001、I005、I006、I008、I009、I012、I016、I017、I018、I019、I020、I021、I023、I027、I029。它们拥有 Canonical 战斗属性与效果标签，但没有正式 VFX 家族、样式与 cue 的既有定义；BattleSandbox 旧样例不是这些 Item 的正式身份来源。
- 因 15 件普通道具仍缺表现身份，本波未把 Board / Host / Formal Presentation Root 强制切换到不完整数据，也未删除仍被 Prefab 序列化引用的旧 Catalog。旧 Catalog 标记为 `REPLACE / DELETE AFTER DATA COMPLETION AND SERIALIZATION CUTOVER`，不再作为未来权威数据源。
- 离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误。未启动 Unity，未执行 Fresh Play。
- 本波状态：`CANONICAL_PRESENTATION_CONTRACT_READY / 15_OF_30_IDENTITIES_MIGRATED / PRODUCT_DATA_GAP_15 / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Canonical Presentation Confirmation State — 2026-08-10

- Canonical Item 展示合同新增唯一显式状态：`UNCONFIRMED`、`CANDIDATE`、`CONFIRMED`。
- 从旧 Formal Presentation Catalog 可追溯迁移的 15 条身份标为 `CANDIDATE`；它们可用于内部预览，但不等于最终视觉确认。
- 其余 15 件普通道具标为 `UNCONFIRMED`，不写入伪造的效果家族、样式或 cue 身份。
- Canonical 校验会拒绝半套身份，也会拒绝 `UNCONFIRMED` 行携带身份值；Resolver 不会为 `UNCONFIRMED` 数据输出表现身份。
- 本边界不改 Board artwork、Scene/Prefab 序列化、VFX、Battle mutation、Reward、Authority 或存档。最终视觉确认与正式 Presentation 切换继续作为后续独立工作。
- 离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误。未启动 Unity，未执行 Fresh Play。
- 本波状态：`PRESENTATION_STATE_BOUNDARY_READY / 15_CANDIDATE / 15_UNCONFIRMED / NO_PLACEHOLDER_IDENTITY / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Canonical Effect Execution Coverage Audit — 2026-08-10

- 唯一 Catalog 的 I001–I030 都有 Canonical signature effect 数据，共覆盖 22 个 `operation + targetStatId` 组合和 22 个 trigger ID。
- 正式 Adapter 只把累计 `ItemGeneratedInstanceSnapshot + CanonicalItemDefinition` 投入 `directItemFacts`；旧 `pool15ActiveItemFacts` 固定为空。因此旧 Pool15 Foundation、Candidate Profile 和 Event API 不是正式玩家线路的效果 feeder。
- Live Session 当前对每个 placed+lit Item 都执行 rolled `damage / break / guard / heal / nianCost / cooldown` 基础战斗事实；signature effect 层只有 I007 的完整 Canonical status semantics 会进入真实状态生命周期。
- I007 之外的 29 条 signature effect 目前没有从 Canonical `combatEffect` 到 Live Session mutation 的通用 dispatch。旧 Pool15 分支中的历史算子和 cue 不得当作这 29 条的正式实现证据。
- 首个真实断点已命名为：`CanonicalItemDefinition.combatEffect -> Live Session generic effect operator dispatch`。Presentation 不是该断点；先发生真实 mutation，再产生同 source instance cue 和表现事件。
- 不应按 29 个 Item ID 增加分支。下一实现边界必须按 trigger / condition / operation / target / status grammar 分组，并且只实现已有明确状态 Owner 与数值语义的组合；缺语义的组合保持 `PRODUCT_SEMANTIC_GAP`，不猜值。
- 旧 Pool15/Starter 岛仍有 Runtime 与 Editor 消费者，当前标记维持 `REPLACE / DELETE-CANDIDATE`；应在 Canonical operator 覆盖和正式 Presentation cutover 后按责任岛退役，不能只按文件名批量删除。
- 本审计状态：`EFFECT_EXECUTION_FIRST_BREAK_NAMED / 1_OF_30_SIGNATURE_EFFECTS_LIVE / 29_REQUIRE_GRAMMAR_CLOSURE / NO_ITEM_ID_PATCH / NOT_READY`。

## Formal Detail Legacy Projection Retirement — 2026-08-10

- `C1FormalItemDetailProjectionAndSelection` 的普通 Item 构造入口现只保留 Canonical generated instance/definition 与 I031 `NotApplicable`；旧 Starter projection 与 Pool15 fact 构造器已删除。
- Detail combat facts 不再携带 Pool15 component collection，也不再拼装旧 component signature；正式详情只展示同一 Canonical ItemInstance 的 rolled stats、trigger 与 combat source。
- 聚焦 Editor 测试已删除未被 `RunAllOrThrow` 执行的旧 I001/I002/Pool15 场景、旧 provider、旧 fixture 与配套辅助链；当前只保留静态边界、Canonical 普通 Item 详情及其最小构造夹具。
- `StarterSourceKind` 暂时保留，因为旧 Formal Presentation Catalog 仍有真实序列化消费者；`C1FormalItemOrdinaryInstanceSnapshot.combatFact` 也仍属于 Authority 合同岛，本波未跨域强删。
- 删除后扫描确认：Detail Runtime 已无 `FromReleasedProjection`、`FromPool15Fact`、Pool15 components 或 Pool15 source kind；测试中剩余 Pool15 类型仅用于禁止旧调用的源码边界和现有 Authority 构造签名。
- 离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误。未启动 Unity，未执行 Fresh Play。
- 本波状态：`FORMAL_DETAIL_LEGACY_PROJECTION_RETIRED / ACTIVE_TEST_CHAIN_CANONICAL_ONLY / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Authority Legacy Combat Fact Boundary Audit — 2026-08-10

- 正式初始获取与 Campaign Reward 都以同一 `ItemGeneratedInstanceSnapshot + CanonicalItemDefinition` 构造 `C1FormalItemOrdinaryInstanceSnapshot`；两条生产构造路径对旧 `C1CampaignPool15BaseCombatFact` 均只传入 `null`。
- `C1FormalItemCombatFactHydration.TryResolveExact` 在全工程没有调用者；`C1FormalItemBattleItemRow.combatFact` 只复制 Ordinary Snapshot 的旧字段，正式 Battle Adapter 不读取它，而是从 generated instance/definition 构造 Canonical direct facts。
- 已删除零外部调用、且专门要求旧 Reward Hydration 的 `RunRev08FixedItemCombatFactHydration` Editor 测试入口；Editor 离线编译 `0` 错误。
- `C1FormalItemSessionAuthority` 是当前 Cleanroom Task Contract 的 Protected Boundary。虽然旧字段与 Hydration 已满足“生产零消费者”证据，本波没有越权收缩 Authority 快照/构造合同，也没有借测试清理绕过该边界。
- 下一决策点：如单独授权 `AUTHORITY_CONTRACT_SHRINK`，可在一个原子波次删除 Ordinary/Battle Row 的旧 `combatFact` 字段、零调用 Hydration、构造参数及剩余反射夹具；在此之前标签为 `DELETE-CANDIDATE / BLOCKED_BY_PROTECTED_CONTRACT`。
- 本波状态：`AUTHORITY_LEGACY_COMBAT_FACT_DEAD_ISLAND_PROVEN / DEAD_REV08_TEST_RETIRED / AUTHORITY_CONTRACT_UNCHANGED / INTERNAL_EDITOR_COMPILE_PASS / NOT_READY`。

## Sandbox / History Responsibility Pass — 2026-08-10

- 三个未启用实验 Scene 均不满足直接删除门：BattleSandbox 仍被正式 Presentation/Prefab authoring 当作来源；ItemSandbox 仍承载独立开发工具；CoreLoop Lab 是明确隔离的产品方向实验室。当前统一标记为 `QUARANTINE`，不进入正式玩家路线，也不按名称批量删除。
- `ItemInstanceQualifiedBuildStateAdapterVerifier` 经全工程符号扫描为零外部调用，只生成三份旧 V0.4 报告，并保留已被长期工程规则禁止的旧式完整性门禁。它没有 Runtime、Scene、Prefab 或正式测试消费者。
- 已物理删除该 Editor verifier、`.meta` 及三份专属旧报告；没有修改 Item Build Runtime、Board、Detail、Sandbox Scene 或正式玩家流程。
- 删除后 Editor 离线编译 `0` 错误；临时离线编译响应清单已同步移除被删源码，不向工程增加兼容桩。
- `CoreAwakeningPreviewVerifier` 仍被多个旧 Editor 验证器串联调用，不能单文件删除；其整条调用链标为下一轮 `QUARANTINE / AUDIT_AS_ONE_TOOLING_ISLAND`。
- 本波状态：`SANDBOX_SCENES_QUARANTINED / ZERO_CALL_LEGACY_VERIFIER_RETIRED / INTERNAL_EDITOR_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Legacy Item Tooling Island Retirement — 2026-08-10

- 已退休 3 个零入口聚合工具：`ItemFullDetailCompletePresentationRegressionRunner`、`ItemFullDetailBuildSandboxWorkbenchRegressionRunner`、`ItemBalanceWorkbenchVerifier`；它们没有菜单、当前/锁定任务或目录外消费者，只负责串联旧 V0.4 报告。
- 已退休 2 个随聚合链失去消费者的 Sandbox 验证节点：`ItemDetailInstanceDataAdapterVerifier`、`ItemBalanceCandidateDetailSandboxAdapterVerifier`，并删除 7 份专属旧报告；对应 Runtime Adapter 与 ItemSandbox Scene 均保留。
- `Editor/ItemGeneration` 的 9 个验证器构成封闭互调图，没有目录外 C# 消费者或菜单入口，已整岛物理删除；Item 生成、属性区间、词条、Roll Engine、Drop Core/Policy 等 Runtime 实现未删除。
- 上述生成验证器专属的 38 份零引用 V0.4 报告已删除；它们不是 Catalog 或运行时数据源。
- 随后失去全部消费者的 4 个 Detail/Awakening/Workbench Verifier 及 16 份专属报告也已退休；实际 Detail、Awakening、Board 与 ItemSandbox Runtime 继续保留。
- Editor 离线编译在每个代码删除波次后均为 `0` 错误；临时编译响应清单只同步移除被删源码。
- 剩余 `Editor/ItemSandbox` 的 10 个行为验证器虽然代码调用图封闭，但覆盖 I031 点亮、Board 规则、技能触发、Build Synergy 与 ItemSystem。由于这些属于受保护玩家系统且尚无正式替代验证入口，当前标签为 `QUARANTINE / RETIRE_AFTER_REPLACEMENT`，本轮未删除。
- 本波状态：`LEGACY_ITEM_TOOLING_ISLAND_RETIRED / RUNTIME_OWNERS_PRESERVED / PROTECTED_BEHAVIOR_VERIFIERS_QUARANTINED / INTERNAL_EDITOR_COMPILE_PASS / NOT_READY`。

## Zero-Call BuildSandbox Report Retirement — 2026-08-10

- `BattleSnapshotAdapterReportWriter` 经全工程符号扫描只有自身声明，没有 Validator、GuardRunner、菜单、当前/锁定任务或 Runtime 消费者。
- 已删除该纯 Editor writer、`.meta` 与四份专属旧报告；`BattleContractFieldValidator`、Battle contracts、BuildSandbox Runtime 和正式 Unified Battle 均未修改。
- 删除后 Editor 离线编译 `0` 错误；该波不启动 Unity，不执行 Fresh Play。
- 其他 BuildSandbox ReportWriter 仍有 Validator 或 `BuildSandboxGuardRunner` 消费者，继续保留并按调用岛逐项审计，不做批量删名。
- 本波状态：`ZERO_CALL_BATTLE_SNAPSHOT_REPORT_WRITER_RETIRED / ACTIVE_BUILD_SANDBOX_REPORTERS_PRESERVED / INTERNAL_EDITOR_COMPILE_PASS / NOT_READY`。

## BuildSandbox Legacy Aggregate Report Entry Retirement — 2026-08-10

- 已按完整调用图审计 BuildSandbox Validator、ReportWriter、`BuildSandboxGuardRunner` 与外部菜单消费者。结论不是整目录可删：仍被 Preview Context、配置面板和当前 Sandbox 入口消费的底层 Validator 继续保留。
- 用户已明确批准 `BUILDSANDBOX_GUARD_RUNNER_CONTRACT_SHRINK`。本波从 `BuildSandboxGuardRunner` 移除 14 组旧报告链的 Menu、Batch 与 Run 入口，共 42 个旧入口；保留当前仍被 `TalismanBagEditorMenu` 使用的 `RunBattleSandboxPreviewSceneMenu` 与 `RunBattleSandboxDevChapterPlayableMenu`。
- 已删除 9 个目录外零消费者的旧 ReportWriter 及其 `.meta`，并删除 45 份仅由这些旧链生成的历史报告输出；没有删除仍被当前 Preview/配置链消费的 Validator。
- 保护边界未改变：Scene、Prefab、BattleSandbox Runtime、Canonical Item、Reward、Authority、Board/Tray、Presentation、VFX、Enemy、StageConfig 与 Save 均未修改；没有新建兼容层、第二系统或替代入口。
- 删除后残留扫描确认：旧 Writer 类名与 14 组旧 Runner 入口在 `Assets/_Game` 中为零引用；离线编译响应清单也已移除所有被删源码路径。
- 离线全量编译结果：Runtime `0` 错误（保留 2 条既有未使用字段警告），Editor `0` 错误。未启动 Unity，未执行 Fresh Play，因此玩家状态仍为 `NOT_READY`。
- 工具清理在此停止：`STOP_TOOL_CLEANUP`。下一 P0 固定为独立高风险 Battle Runtime 包 `CANONICAL_EFFECT_OPERATOR`，只闭合 `CanonicalItemDefinition.combatEffect -> Battle Fact -> generic operator dispatch -> authoritative live state sink -> MutationResult -> same sourceItemInstanceId cue`；不得在当前 Cleanroom 合同下顺手进入 Battle 实现。
- 本波状态：`BUILDSANDBOX_LEGACY_REPORT_ISLAND_RETIRED / CURRENT_SANDBOX_ENTRIES_PRESERVED / INTERNAL_COMPILE_PASS / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Canonical Effect Operator Implementation Checkpoint — 2026-08-10

- 工具与历史清理继续保持 `STOP_TOOL_CLEANUP`；本波只修改现有 Battle Runtime 写域，没有删除旧代码、修改 Scene/Prefab/Profile，也没有建立第二套 Item、Battle 或状态系统。
- 新增无状态 `C1FormalRealtimeBattleCanonicalEffectOperator`，按 `operation + targetStatId + unit` 解析唯一 Canonical Catalog。I001–I030 的 30 行、22 个语法组合全部可解析；写域内没有 Item-ID dispatch。
- 现有 `C1FormalRealtimeBattleSession` 已承接可证明的真实状态落点：敌方 HP/shell、玩家 Nian/guard/heal、敌方与 Item 待执行行动、目标选择和现有 burn status。I004 额外目标、镜像伤害转护势与效果结束推进相邻冷却均已改为事件发生后修改真实状态，再发送同源 cue。
- focused verifier 仅证明 Catalog 语法映射覆盖，不作为玩家路径、触发条件或 mutation 完成证明。
- 首个缺失层：正式战斗没有“玩家负面状态 -> 净化成功 -> 同一 target/source transition”的权威 Owner/API；因此 `on_cleanse` 家族及依赖净化的 heal-to-guard、Nian refund、连续净化与蓄水治疗无法真实触发。需要复制负面状态剩余时间的持续治疗也缺少正式 HoT 时间轴。未用敌方 burn 或 detached state 冒充这些语义。
- 定向离线编译：Runtime `0` 错误（2 条既有警告），Editor `0` 错误；Unity/Fresh Play 未运行。
- 当前判定：`CANONICAL_OPERATOR_GRAMMAR_READY / PROVEN_LIVE_SINKS_PARTIAL / CLEANSE_OWNER_API_MISSING / ARCHITECTURE_REASSESS_REQUIRED / USER_HANDTEST_NOT_RUN / NOT_READY`。

## Canonical Effect Runtime Proof Ledger — 2026-08-10

- 证明载体：`KEEP`。复用既有 `C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier`、真实 Canonical Catalog 与现有 `C1FormalRealtimeBattleSession`；没有第二套 Battle/Test Framework。
- `PASS`：I004 ExtraTarget（真实多目标 HP/Shell + Nian）、I007 Afterglow（真实 Status + tick）、Guard（真实玩家 Guard）、Heal（真实玩家 HP）、Enemy Action Delay（真实 pending Action due time）、Tray/unlit（拒绝且无 Nian/success cue）。
- `CODE_GAP`：`NO_MUTATION`。无 HP/Shell/Guard/Action/Status mutation 时仍扣除 Nian。标签：`KEEP OWNER / FIX IN SEPARATE AUTHORIZED BATTLE RUNTIME TASK`；本轮不修。
- `SEMANTIC_GAP`：本轮七项无新增。Cleanse 仍维持既有独立产品语义缺口，未实现、未伪造、未纳入本轮结果。
- 保护边界：Authority、Presentation、Scene/Prefab、Reward、Save、旧 Sandbox/Pool15 均未修改或删除；工具清理仍保持 `STOP_TOOL_CLEANUP`。
- 运行证据：Editor 定向离线编译 `0` 错误；独占任务 Unity batch PID `13364` 已退出，无残留 Unity。返回码 `1` 对应唯一 Code Gap 的失败关闭。
- Ledger 状态：`CANONICAL_EFFECT_RUNTIME_PROOF_COMPLETE / 6_PASS / NO_MUTATION_CODE_GAP / STOP / NOT_READY`。

## Canonical Formal Feeder Proof Attempt — 2026-08-10

- 复用现有 `C1FormalObtainRebuildBattleLoopTests` owner-chain 集成入口，没有新建 Battle/Test Framework；只增加独立 batch 入口和同一实例/rolled stats 证据检查。
- 旧入口首次暴露 `FIXED_BASELINE_UNAVAILABLE`，原因是测试仍以无 Resolver 的旧方式创建 Authority；已迁移到正式代码正在使用的 Canonical Resolver 创建方式，未恢复固定 I001。
- 最终首个断点发生在 Reward 之前：自动 runner 无法取得 `1-1` 实时胜利，因此无法生成 I004 并进入后续 feeder。标签：`QUARANTINE / QA_HARNESS_LIFECYCLE_GAP / NOT_PRODUCT_FEEDER_EVIDENCE`。
- 用户正常路径已证明可胜利、领奖、摆放与推进，强于该离线失败；因此 Reward、Authority、Adapter、Session 均保持 `UNPROVEN_BY_THIS_TASK`，不得误标 `CODE_GAP` 或 `PASS`。
- Editor 定向离线编译 `0` 错误；任务 Unity PID `29424` 已退出，无残留进程。Runtime、Scene/Prefab、Presentation、Reward、Authority、Battle、Save 与旧 Sandbox/Pool15 均未修改。
- Ledger 状态：`FORMAL_FEEDER_PROOF_STOPPED_AT_PRE_REWARD_LIFECYCLE_GAP / NO_PRODUCT_FIX / NOT_READY`。

## Cleanroom Return / Temporary Trace Retirement — 2026-08-11

- 用户明确停止 1-3 / I004 局部追踪与补丁路线，恢复 11 号责任岛清扫流程。
- 已整岛删除 `CANONICAL_FORMAL_HOST_FEEDER_TRACE`：Reward Bridge、Formal Adapter、Live Session 内仅用于 I004 的 `UNITY_EDITOR` 日志和临时状态全部移除；临时 Task Contract 一并删除。
- 未删除 Session 中既有 I004 行为、Canonical Effect Operator 或其他正式玩法实现；这些属于独立行为责任岛，当前标签为 `REPLACE / AUDIT_AS_BEHAVIOR_ISLAND`。
- 1-3 当前首个运行阻断发生在 Host 接受 Battle feeder 之前：`UnifiedBattlePageShell.prefab` 没有序列化 `canonicalItemCatalogSource`，而 Host 已把 Canonical Catalog 设为必需输入。标签：`BLOCKED / SERIALIZATION_MIGRATION_GAP / REQUIRES_SEPARATE_UNITY_ATOMIC_MIGRATION`。
- 旧 Editor authoring 内仍存在 `ProtectedHashes`，违反项目长期禁止 Hash/SHA 的规则；在确认 authoring 消费者和替代校验后整岛删除，当前标签为 `DELETE-CANDIDATE / RULE_VIOLATION`。
- 删除后残留扫描对 `CanonicalFormalHostFeederTrace`、`TraceI004CanonicalHost`、`targetI004` 为零命中。
- 离线全量编译：Runtime `0` 错误（保留 2 条既有未使用字段警告），Editor `0` 错误；未启动 Unity。
- Ledger 状态：`CLEANROOM_RESUMED / TEMP_TRACE_RETIRED / SERIALIZATION_GAP_LABELED / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY`。

## Legacy Integrity Mechanism Responsibility Audit — 2026-08-11

- 账本旧条目中的 `CoreAwakeningPreviewVerifier` 已不在当前磁盘；当前只有 ItemSandbox Runtime 的 `ItemSandboxCoreAwakeningPreviewCatalog` 消费者。旧“下一工具岛”标签已过期，不得据此继续删名。
- 旧式文件完整性机制仍分布在 Runtime 与 Editor 多个责任岛；后续只按真实消费者和替代职责处理，不保存或汇报关键字命中数量。
- 这些文件分为三类：
  - Editor 文件固定门禁：`REPLACE / REMOVE_HASH_GATE`；保留真实结构、组件、资源和保存后校验。
  - 无可见入口的旧 Verifier/Survey 调用岛：`QUARANTINE / AUDIT_WHOLE_CALL_ISLAND`；必须连同目录外调用者、报告输出和命令行入口一起审计，不能单文件删。
  - Runtime canonical signature、snapshot identity 与跨系统合同：`REPLACE / CROSS_SYSTEM_IDENTITY_MIGRATION / BLOCKED`；不得在 Editor 清扫波次中直接移除。
- 已从 `C1FormalObtainRebuildBattleLoopSceneAuthoring` 删除固定 15 文件 SHA 门禁；现有 Runtime Source 边界、Prefab/Scene 结构与最终状态校验继续保留。
- 已从 `C1StageThemeBackgroundMainlineBindingAuthoring` 删除固定输入 SHA、前后 Hash 和 Hash 回执；改用必需路径存在性、现有组件/资源/几何校验，以及单次 authoring 内的原文不变比较。
- 两次修改后离线全量编译均为 Runtime `0` 错误、Editor `0` 错误；未启动 Unity。
- `Editor/CrossSystem/ItemEnemy` 当前 15 个文件构成旧调查链，且仍被 I031、BuildSandbox Layout Resilience 与 Enemy Requirement 三个外部 Verifier 引用；没有证据支持单文件删除，当前整体标签为 `QUARANTINE / RETIRE_AFTER_REPLACEMENT`。

## Pool15 / Starter Residual Responsibility Audit — 2026-08-11

- 正式 Runtime 仍有 7 个文件直接携带 Pool15 类型或字段：Formal Adapter、Live Session、Battle Contract、Pool15 Base Facts、Pool15 Carrier、Formal Authority 与 Formal Presentation Catalog。
- 固定 Starter 相关 Runtime 仍分布在 6 个文件，其中 V02 Reward 属历史版本边界；它们不是当前唯一 Canonical Drop 数据库，但仍是合同、Presentation 或历史运行图的消费者。
- `ItemCompleteCandidateContentSeed` / `ItemBalanceCandidateSeedBuilder` 当前 C# 消费者为 0；其旧种子实现已不在当前磁盘，账本不再保留“下一步迁移 Seed”假任务。
- Pool15 残留不能现在整岛删除：Live Session、Presentation Catalog 与 Authority 仍有真实调用，且 Canonical Effect 仍有 `Cleanse` / HoT 产品语义缺口。当前标签为 `REPLACE / BLOCKED_BY_CANONICAL_EFFECT_AND_PRESENTATION_CUTOVER`。
- 当前清扫不会修 1-3，也不会用删除 Pool15 来掩盖 `canonicalItemCatalogSource` 序列化缺口。

## Legacy Integrity Editor Tooling Wave 2 — 2026-08-11

- 已从 `C1StageThemeBackgroundPrefabAuthoring` 与 `C1StageThemeBackgroundCarrierTests` 删除 SHA 计算和 Hash 命名；同一次 authoring 前后的 PNG 原始字节比较继续保护真实资产不被工具改写。
- 已从 `C1ExactBattleSandboxItemDetailPhaseBAuthoring` 删除固定文件 `ProtectedHashes` 门禁；Prefab 视觉一致性继续按完整 hierarchy/component/serialized-property 结构文本逐字段比较，不改变 Item Detail Runtime、Scene 或 Prefab。
- 已从 `CoreLoopLabSceneAuthoring` 删除 source Scene SHA 与日志输出；Save-As 前后的 source Scene 改为原始字节不变比较。该 Lab Scene 仍保持 `QUARANTINE / PRODUCT_LAB`，本波没有删除。
- 已从 `FormalBattlePresentationPrefabAuthoring` 删除 Profile coverage SHA；同一次 authoring 的保护文件改为原始字节不变比较，现有 Profile 内容、Prefab 和 Runtime Presentation 均未修改。
- 当前清扫范围继续按具体文件、入口、消费者和责任岛记录，不保存或汇报完整性算法命中数量。
- 11 个剩余 Menu 工具不是同一职责：Battle/Enemy verifier 属于旧报告与证明链；Item/Unified authoring 把旧 Prefab/Scene 文件形态写进控制流。统一标签为 `REPLACE / STRUCTURAL_VALIDATION_MIGRATION`，禁止只删除公共 helper 或按关键字批量改写。
- 无 Menu 的旧 verifier 只有在全工程当前代码、Task Contract、序列化入口和有效命令行入口均归零，且独有验证职责已有正式替代时，才从 `QUARANTINE / AUDIT_WHOLE_CALL_ISLAND` 升级为 `DELETE`。历史 Assignment 或 Archive 中的文字引用只保留历史证据，不等同当前消费者。
- 本波每次修改后 Runtime / Editor 离线编译均为 0 错误；保留 2 条既有 Runtime 未使用字段警告；未启动 Unity。

### Remaining Menu Tool Labels

| Tool island | Label | Reason |
| --- | --- | --- |
| `C1FormalRealtimeBattleSessionVerifier` / `C1FormalBattleDiscreteProgressOperatorsVerifier` | `KEEP / BEHAVIOR_ONLY_VALIDATION` | 正式 Battle 行为验证已保留；Assignment、文件保护、旧报告生成与 8 份专属报告已退休 |
| `BoneSwapRemnantRuntimeOperatorVerifier` / `C1Lv1EarlyEncounterBalanceVerifier` / `C1ShatteredHostPorcelainHoundRuntimeVerifier` | `KEEP / BEHAVIOR_ONLY_VALIDATION` | 保留 operator、encounter、调度、伤害、重置和边界行为验证；旧 Assignment、文件保护、报告生成与报告自证基础设施已退休 |
| `ItemLivingGradientOutlineVfxPrototypeVerifier` / `LiHuoCombatFeedbackOrchestrationVerifier` | `QUARANTINE / DEV_LAB / BEHAVIOR_ONLY_VALIDATION / RETIRE_AFTER_PRESENTATION_REPLACEMENT` | 保留静态行为、Rendered QA 与 Android 资产包含验证；旧文件完整性门禁已移除，且不得成为正式玩家路径 Owner |
| `C1ExactBattleSandboxItemPromotionEditor` | `REPLACE / ITEM_PREFAB_PROMOTION_MIGRATION` | 仍有正式 Item Prefab/authoring 职责，但混有多轮历史文件形态与报告门禁 |
| `C1ExactBattleSandboxItemDetailPopupStandalonePrefabAuthoring` | `KEEP + REPLACE_STRUCTURAL_SIGNATURE` | 正式 Shell 仍消费其 Popup 载体；只允许原位迁移验证，不允许裸删 Prefab |
| `C1ExactBattleSandboxNavigationPhaseAAuthoring` | `REPLACE / HISTORICAL_INTAKE_STATE_MACHINE` | 旧 Scene/Prefab 状态被写入 authoring 控制流；需整岛改为当前结构迁移 |
| `C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring` | `REPLACE / HISTORICAL_INTAKE_STATE_MACHINE` | 旧 Shell 状态分类仍控制修复分支；需在独立序列化迁移前重构 |
| `C1UnifiedSingleFormalPathAuthoring` | `REPLACE / HISTORICAL_INTAKE_STATE_MACHINE` | 正式 Shell 单路径 authoring 仍有价值，但不能继续依赖历史文件摘要识别状态 |

### Formal Battle Verifier Infrastructure Closure｜2026-08-11

- `C1FormalRealtimeBattleSessionVerifier` 与 `C1FormalBattleDiscreteProgressOperatorsVerifier` 保留菜单、focused 入口以及真实 Session、adapter、action queue、mutation、拒绝原子性与阶段回归行为验证。
- 两文件已移除旧 Assignment 绑定、保护文件表、运行前后文件完整性比较、静态包白名单、重复报告写入和报告格式自证；8 份已经没有活动代码或脚本消费者的专属历史报告同步退休。
- 未修改 `C1FormalRealtimeBattleSession`、Formal Adapter、Battle Contract、Reward、Authority、Stage、Enemy、Scene/Prefab、Presentation 或 Save；没有建立第二套 Battle/Test Framework。
- 离线全量编译为 Runtime 580 / Editor 181，均 0 错误；仅保留两条既有未使用字段警告。Hygiene Gate 由全局 65 降至 63、Editor 14 降至 12，仍为 `WARNING`。
- 下一子岛为 `BoneSwapRemnantRuntimeOperatorVerifier`、`C1Lv1EarlyEncounterBalanceVerifier` 与其调用的 `C1ShatteredHostPorcelainHoundRuntimeVerifier`。仍按“保留行为、移除旧证据基础设施”处理，不删除 Enemy Runtime 或产品数据。

### Runtime Identity Labels

| Runtime group | Label | Current action |
| --- | --- | --- |
| Formal Reward / Stage / Authority / Battle contracts / Detail | `KEEP + REPLACE_IDENTITY_CONTRACT / BLOCKED` | 跨系统文本身份迁移；不在 Editor 清扫中直接删除 |
| Pool15 / Starter / old CampaignLoot contracts | `REPLACE / BLOCKED_BY_FORMAL_CONSUMER_CUTOVER` | 等正式消费者归零与语义迁移完成后整岛退休 |
| BattleSandbox / CrossSystem LayoutResilience / Shougunu dev runtime | `QUARANTINE / RETIRE_AFTER_FORMAL_REPLACEMENT` | 不进入正式路径；先迁移仍有价值的验证职责 |
| Enemy contracts and snapshots still consumed by campaign/runtime | `KEEP + REPLACE_IDENTITY_CONTRACT` | 保留 Enemy 事实；后续只迁移身份表示，不改变敌人数值或行为 |

## Legacy Aggregate Verifier Retirement — 2026-08-11

- 已按完整调用岛物理退休 4 个无当前代码、Menu、CURRENT Task 或序列化消费者的旧 Editor 聚合验证器及其 `.meta`：`ShougunuPhase1BattleApplicationContractVerifier`、`EnemyDataValidatorAndSnapshotVerifier`、`ShougunuPhase1RuntimeAndActionContractVerifier`、`V04Chapter1EncounterManifestVerifier`。
- 同步删除 27 份只由上述验证岛生成或消费的历史报告；没有删除 Shougunu Runtime、Enemy Runtime/Schema、Chapter 1 Runtime Manifest、正式关卡数据或玩家行为。
- 删除前已沿报告依赖图审计 `V04Chapter1EncounterManifestVerifier` 与两个 Shougunu verifier，避免保留断裂报告依赖或添加兼容层。
- `V04ChapterFlowContractVerifier` 未删除：其 Manifest/Reducer 验证职责仍覆盖被 WorldMap 与连续流程消费的运行时类型。当前标签为 `KEEP + REPLACE_LEGACY_INTEGRITY_INFRA`，后续原位移除旧 Assignment/报告完整性基础设施，不以裸删验证职责替代迁移。
- 当前范围以仍有现行消费者的具体责任岛为准，不使用关键字命中数量作为范围或质量门禁。
- 退休后离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误；未启动 Unity，未执行 Fresh Play。
- 本波状态：`FOUR_LEGACY_AGGREGATE_VERIFIER_ISLANDS_RETIRED / 27_DEDICATED_REPORTS_RETIRED / CHAPTER_FLOW_CORE_VALIDATION_PRESERVED / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY`。

## BoneAspect Zero-Consumer Data Island Retirement — 2026-08-11

- 已整岛退休 `BoneAspect/VocabularyExtension`：4 个 Runtime 数据/组合/验证脚本、1 个旧 Editor Verifier、配套 `.meta` 与 6 份专属报告。
- 已整岛退休 `BoneAspect/CarrierPresentation`：5 个 Runtime 候选展示目录脚本、1 个旧 Editor Verifier、配套 `.meta` 与 7 份专属报告。
- 两岛内所有公开类型的工程外消费者均为各自旧 Verifier；Verifier 无当前 C#、Menu、CURRENT Task 或命令入口；全部脚本与目录 GUID 在 `Assets` 中序列化引用为 0。
- 这些对象是候选词汇/展示数据模型，不是当前 Enemy Runtime、Prefab、图片、StageConfig 或正式 Presentation。历史 Assignment 与旧报告中的文字引用保留为历史记录，不作为现行消费者。
- 删除规模：11 个 C#、13 个 `.meta`、13 份专属报告。删除后全工程 C# 对两岛类型与报告路径为 0 命中。
- 删除后离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误；未启动 Unity，未执行 Fresh Play。
- 本波状态：`BONE_ASPECT_ZERO_CONSUMER_DATA_ISLANDS_RETIRED / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY`。

## CrossSystem ItemEnemy / LayoutResilience Connected-Island Audit — 2026-08-11

- `CrossSystem/ItemEnemy` 当前没有 Formal Battle、Reward、Authority、WorldMap 或 Enabled Build Scene 的直接运行时消费者；其脚本 GUID 也没有 Scene/Prefab/ScriptableObject 序列化挂载。
- 该 Runtime 岛仍被 `BuildSandbox` 的 Board Authority、Board Adapter、LayoutResilience playtest adapter/feedback 消费，不能按“Formal 不用”直接删除。
- `Editor/CrossSystem/ItemEnemy` 是一条互相调用的旧调查/报告链，并被 I031、BuildSandbox LayoutResilience、Enemy Requirement 三个外部 Verifier 引用；这些外部验证器尚未由 Formal focused validation 接管。
- Runtime 与 Editor 两侧统一标签：`QUARANTINE / RETIRE_AFTER_FORMAL_REPLACEMENT`。Formal Runtime 不得依赖它；待 Board/I031/布局韧性验证职责迁入正式验证入口后整岛退休。
- `FormationEnergyContractValidator` 与 `LegacyItemBehaviorIntegrationValidator` 保留在同一隔离岛：没有独立入口不等于职责已被替代，其报告仍与现存 BuildSandbox 供能/行为资料交叉引用。
- 本轮没有删除或修改该连接岛内文件，也没有启动 Unity。该结论是停止误删的标签闭账，不是玩家进展。

## BuildSandbox Tooling And V02 Builder Audit — 2026-08-11

- BuildSandbox 剩余 ReportWriter 的当前消费者均可追到 Validator、`BuildSandboxGuardRunner` 或对应开发窗口；标签为 `KEEP INSIDE QUARANTINE`，不再进行单文件删除尝试。
- `FormationEnergyContractValidator` / `FormationEnergyContractReportWriter`：`QUARANTINE / RETIRE_AFTER_FORMAL_REPLACEMENT`。
- `LegacyItemBehaviorIntegrationValidator` / `LegacyItemBehaviorIntegrationReportWriter`：`QUARANTINE / RETIRE_AFTER_FORMAL_REPLACEMENT`。
- `TalismanBagSceneBuilder`：`QUARANTINE / RETIRE_WITH_V02_SCENE_MIGRATION`。它没有当前入口与序列化挂载，但仍维护启用中的 V02 Formation Scene，并能重建根级旧 Run/Shop/Item/Enemy/Prefab 资产；删除必须与 V02 Scene 归属迁移同批完成。
- 本轮未运行上述 Builder、Validator 或 Unity。

## ScriptableObject / Config / Resource Labels — 2026-08-11

| Responsibility island | Consumer evidence | Label / action |
| --- | --- | --- |
| `Configs/ItemBalanceWorkbench` | Catalog 序列化持有全部 Item Profiles；Formal focused verifier、Reward/Unified authoring、Resolver 与现存 Adapter 直接读取 Catalog | `KEEP / CANONICAL_ITEM_DATABASE` |
| `Configs/ItemDetail/ItemDetailVisualTheme.asset` | 正式 Exact Popup Prefab 与实验 Scene 序列化消费 | `KEEP / PRESENTATION_ASSET` |
| `Configs/BuildSandbox/**` | BuildSandbox Config Validator 通过类型扫描读取；Synergy/Shape/Affix 评估链仍消费 | `QUARANTINE / LAB_DATA / RETIRE_AFTER_FORMAL_REPLACEMENT` |
| `ScriptableObjects/TalismanBag/V02/**` | 启用 V02 Formation Scene、V02 内部资产链及 CoreLoop Boss Config 消费 | `QUARANTINE / RETIRE_WITH_V02_SCENE_MIGRATION` |
| 根级旧 Item/Enemy/Run/Shop SO | 旧 Scene Builder、旧 `DraggableTalismanItem.prefab` 与内部 Run/Shop 链消费 | `QUARANTINE / RETIRE_WITH_LEGACY_BUILDER` |
| `Resources/item` / `Resources/item_daoju` | 正式 Item Detail、Canonical artwork 与 Card 投影按加载键消费 | `KEEP / PLAYER_ITEM_ART` |
| `Resources/anim/Enemy/d1/d1_3`、`di_1` 与 `Resources/Enemy/guciquan_*` | Formal Battle Presentation 与 Chapter 1 Enemy Visual Profile 消费 | `KEEP / FORMAL_ENEMY_PRESENTATION` |
| `Resources/Enemy/守骨奴 第一阶段状态` | V04 Chapter Flow 仍调用其 Visual Prototype Owner | `KEEP INSIDE QUARANTINE / RETIRE_AFTER_FORMAL_ENEMY_PRESENTATION_CUTOVER` |
| `Resources/V04/ItemLivingGradientOutlineDevOnly` | BattleSandbox living VFX supplier 仍消费，正式替代尚未完成 | `QUARANTINE / PRESENTATION_SUPPLIER_ASSET` |
| `Resources/anim_MP4` | 无 Runtime 加载或序列化引用；源视频已保留到项目归档区 | `RELOCATED / SOURCE_MEDIA_OUTSIDE_RESOURCES` |

### Physical Retirement — Resource And Empty-Directory Hygiene

- 删除零消费者动画岛 `Assets/_Game/Resources/anim/Enemy/d1/d1_2` 及其 `.meta`；未触及 `d1_3`、`di_1` 或正式 Enemy Profile。
- 删除空且无引用的 `Assets/_Game/Resources/anim/video_6a4e13ef8ea9ccfeebcfd8ee_1783501848145` 目录与 `.meta`；其同名源视频已经随源媒体迁移保存到项目归档区。
- 将 `Assets/_Game/Resources/anim_MP4` 的两份无消费者源视频移至 `Archives/SourceMedia/anim_MP4`；删除旧 Unity `.meta` 与空目录，避免源媒体继续被打入玩家资源包。
- 删除经复核为空且目录 GUID 无消费者的目录壳：`Prefabs/TalismanBag/Feedback`、`Prefabs/TalismanBag/V02`、`Prefabs/TalismanBag/VFX`、`ScriptableObjects/TalismanBag/EnemyConfigs`、`Scripts/TalismanBag/Balance`、`Editor/ItemCampaignLoot`、`Editor/ItemGeneration`、`Editor/ItemShape`、`Editor/V04/RewardDrop`、`V02/Enemies`、`V02/Grid`、`V02/Items`、`V04/RewardDrop/Data/FirstClear` 与随后变空的 `V04/RewardDrop/Data`；同步删除各目录 `.meta`。
- 本次物理清理未改 Battle、Reward、Authority、Presentation、Scene/Prefab、Save 或产品数值。

## Prefab Consumer Recheck And Disabled Code Retirement — 2026-08-11

- 剩余 Prefab 已重新按外部序列化 GUID、显式文件路径、Builder 与 authoring 消费者核对；没有出现新的零职责 Prefab。
- `DraggableTalismanItem.prefab` / `TalismanGridSlot.prefab`：`QUARANTINE / RETIRE_WITH_V02_SCENE_AND_LEGACY_BUILDER`。
- `C1FormalItemBoard.prefab` / `C1FormalItemTray.prefab`：`QUARANTINE / RETIRE_AFTER_ITEM_AUTHORING_CUTOVER`；仍被旧 Formal Item authoring 与 Unified authoring 路径读取。
- `BattleLikePreviewAreaBridge.prefab`：该历史等待条件已在 2026-08-11 满足；Bridge Builder、Host 与 Prefab 已整岛退休，Single Formal Path authoring 的待删除输入已同步移除，当前标签为 `RETIRED`。
- `ItemDetailPanel.prefab`：虽然没有 Scene/Prefab 实例挂载，但仍被 Exact Popup、Unified mount 与 ItemSandbox authoring 以路径/身份读取；保持 `QUARANTINE / DELETE-CANDIDATE_AFTER_AUTHORING_CUTOVER`。
- 删除 `ItemDetailSectionView.cs` 尾部 `ITEMDETAIL_LEGACY_AUTO_PREVIEW_DISABLED` 的永久禁用 Editor 自动预览块。该块已经由显式手动 authoring 替代，不参与编译；Runtime 类本体与 Detail 行为未修改。
- 当前资源无孤立 `.meta`、`Assets/_Game` 无缺失 `.meta` 对象、无残留空目录。
- 离线全量编译：Runtime `0` 错误，Editor `0` 错误；保留 2 条既有未使用字段警告。临时编译输出位于外部工作区且验证后已删除；未启动 Unity或 Fresh Play。

## Remaining Resource-Island Closeout — 2026-08-11

| Responsibility island | Current consumers | Label / action |
| --- | --- | --- |
| `Resources/V04/beibao` | Exact Board、Tray、Prepare Surface Prefab；BattleSandbox、CoreLoop Lab、ItemSandbox Scene | `KEEP / SHARED_ITEM_UI_ART` |
| `Resources/anim/guajian` | Prepare Surface Prefab；BattleSandbox、CoreLoop Lab、ItemSandbox Scene | `KEEP / SHARED_PREPARE_PRESENTATION` |
| `Resources/anim/照煞镜_VFX_RGBA_9帧` | Formal Battle Presentation Root、Enemy Slot、Cue FX Prefab 与 Formal Presentation Profile | `KEEP / FORMAL_BATTLE_PRESENTATION_ASSET` |
| `Resources/V04/ItemLivingGradientOutlineDevOnly` | BattleSandbox runtime auto-install controller、material/profile library 与 verifier | `QUARANTINE / PRESENTATION_SUPPLIER_ASSET / RETIRE_AFTER_FORMAL_CUTOVER` |
| `Resources/V04/LiHuoCombatFeedbackDevOnly` | BattleSandbox runtime auto-install controller 与 orchestration verifier | `QUARANTINE / PRESENTATION_SUPPLIER_ASSET / RETIRE_AFTER_FORMAL_CUTOVER` |
| `Resources/anim/video_6a56065e5485ff99517e29da_1784022639683` | BattleSandbox、CoreLoop Lab Scene 与 `BuildSandboxCharacterIdlePreview` | `QUARANTINE / LAB_PRESENTATION_ASSET` |
| `Resources/V03/Battle/chenbaichaun.png` | 无代码加载键、无外部序列化消费者 | `RELOCATED / UNREFERENCED_SOURCE_ART_OUTSIDE_RESOURCES` |

- `chenbaichaun.png` 已移到 `Archives/SourceMedia/UnreferencedV03Battle`，旧 Unity `.meta` 已删除；未删除源美术文件。
- 当前 Resource 波次不再存在可仅凭“DevOnly”“Sandbox”或历史命名直接删除的对象；有现实消费者的实验供应资产统一留在隔离区，等正式载体接管后整岛退休。
- 已通过的正常领取、5x5 Board 摆放与关卡推进不属于本轮写域；1-3 战斗效果问题未修补，也未被本次清扫结果冒充为完成。

## Scene Responsibility Closeout — 2026-08-11

| Scene island | Current role | Label / action |
| --- | --- | --- |
| BootEntry、MainHome、WorldMap、TalismanUpgrade、UnifiedBattlePageShell | 当前 Build Settings 玩家入口与正式路线 | `KEEP / ACTIVE_BUILD_ROUTE` |
| `Scene_TalismanBag_V02_FormationCounter` | 启用；Navigation、Battle snapshot、Chapter Flow verifier 与旧 Builder 消费 | `KEEP INSIDE QUARANTINE / ENABLED_LEGACY_SCENE / RETIRE_WITH_NAVIGATION_MIGRATION` |
| `Scene_TalismanBag_V04_BattleSandboxPreview` | Battle/Enemy/VFX 原型、Prefab authoring 与多项 verifier 来源 | `QUARANTINE / AUTHORING_SOURCE_AND_LAB` |
| `Scene_TalismanBag_V04_ItemSandbox` | Item Detail authoring、Board/Skill/Lighting verifier 与独立 build menu | `QUARANTINE / ITEM_TOOLING_LAB` |
| `Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab` | 产品方向实验与 CoreLoop Lab authoring | `QUARANTINE / PRODUCT_LAB` |

- 当前磁盘 9 个 Scene 均有现实消费者或启用职责，没有 Scene 满足完整删除门。
- 三个 Lab Scene 不属于正式玩家路线；后续只有在其 authoring、验证与实验职责完成迁移后，才能分别归档或整岛退休。
- V02 Formation 的退休必须同时处理 Build Settings、Navigation 目标、snapshot 默认来源、Chapter Flow 验证和 Scene Builder，禁止拆成多次兼容补丁。

## V02 Formation Ownership Migration Ledger — 2026-08-11

| Component island | Actual current consumer | Label / next action |
| --- | --- | --- |
| Boot `SkipOpeningStory` → `TalismanSceneRoute.Trial` | 新玩家真实入口 | `REPLACE / CUTOVER_TO_WORLDMAP_IN_ATOMIC_MIGRATION` |
| Upgrade bottom Trial route | Upgrade 页真实入口 | `REPLACE / CUTOVER_TO_WORLDMAP_IN_ATOMIC_MIGRATION` |
| `MainTrialFlowService` + `MainTrialPhase` upgrade guide | Upgrade Scene 序列化挂载、升级页与首次升级引导 | `QUARANTINE / REMOVE_OLD_CHAPTER_OWNERSHIP_FROM_UPGRADE` |
| `Scene_TalismanBag_V02_FormationCounter` | Boot/Upgrade Trial 路由、Build Settings；Scene 内完整旧 Run/Battle/Reward 图 | `KEEP_INSIDE_QUARANTINE / RETIRE_AFTER_ENTRY_CUTOVER` |
| `V02RunFlowController` + Scene 内 BattlePrepare/AutoCombat consumers | 只在 V02 Scene 运行图内实际挂载 | `QUARANTINE / RETIRE_WITH_V02_SCENE` |
| `V03BattleStartRequestExporter` | 无正式运行调用；只剩定义/历史身份 | `QUARANTINE / RETIRE_WITH_V02_CONTRACT_MIGRATION` |
| `V03BattleLayoutSnapshotExporter` | 仅 Battle Contract 静态验证样例 | `QUARANTINE / RETIRE_OR_REWRITE_VALIDATION_AFTER_V02_REMOVAL` |
| V02 名称型 verifier / exclusion strings | Editor 验证与历史边界，不是玩家 Runtime feeder | `REPLACE / MIGRATE_VALIDATION_RESPONSIBILITY` |
| `TalismanBagSceneBuilder` V02 branch | 无当前入口，但可重建 V02 Scene 与旧资产 | `QUARANTINE / RETIRE_WITH_V02_SCENE_MIGRATION` |
| `ScriptableObjects/TalismanBag/V02/**`、旧 V02 Prefab/RunConfig | V02 Scene、旧 Builder 与内部资产链 | `QUARANTINE / RECHECK_ZERO_CONSUMER_AFTER_SCENE_RETIREMENT` |

- MainHome Trial 已走 WorldMap，证明现有正式替代入口存在；V02 仍存活的原因是 Boot 与 Upgrade 两条入口尚未迁移，不是缺少新 Scene。
- 迁移载体固定为现有 BootEntry、TalismanUpgrade、Navigation Owner、WorldMap 与 Build Settings；不创建第二 Route、第二 Campaign、第二 Save 或兼容 Adapter。
- 当前结论：`V02_FORMATION_AUDIT_COMPLETE / PHYSICAL_DELETE_BLOCKED_BY_ATOMIC_UNITY_SERIALIZATION_MIGRATION / PLAYER_PATH_NOT_READY`。

## V02 Historical Data / Resource / Tool Island Retirement — 2026-08-11

| Responsibility island | Final label | Evidence / retained boundary |
| --- | --- | --- |
| `Assets/_Game/ScriptableObjects/TalismanBag/**` | `DELETE / PHYSICALLY_RETIRED` | 旧根级 Enemy/Item/ShopPool/RunConfig 与 V02 子岛已在用户精确授权后整岛删除；外部序列化消费者均随同一退休包删除 |
| `Resources/CoreLoop/BossConfigs`、`DropTables`、`Rewards`、`IdleDropConfig`、`TutorialGuideConfig_Fix03` | `DELETE / PHYSICALLY_RETIRED` | 旧 V02 Run/CoreLoop 资源岛；正式五 Scene 不消费。`CoreLoopTalismanUpgradeConfig.asset` 明确保留 |
| `DraggableTalismanItem.prefab`、`TalismanGridSlot.prefab` | `DELETE / PHYSICALLY_RETIRED` | 只属于旧 Builder/旧 ScriptableObject 链；现行 5x5 Board/Tray/Card Prefab 不在该路径 |
| `TalismanBagSceneBuilder` | `DELETE / PHYSICALLY_RETIRED` | 零现行入口，且会重建已退休的旧数据、Prefab 与 V02 Scene 图 |
| `V02/Config/Editor/**`、`V02/Balance/Editor/**` | `DELETE / PHYSICALLY_RETIRED` | 仅维护已退休数据岛；15 个旧 Editor 源文件已从离线编译源清单消失 |
| `TalismanItemBalancePanel`、`TalismanEnemyBalancePanel` | `KEEP / DECOUPLED_FROM_V02_EDITOR_UI` | 保留生产性面板；改用 Unity 原生 Editor 控件并独立打开，不再依赖旧 DataCatalog Editor |
| `TalismanBagEditorMenu` | `KEEP / CURRENT_MENU_SURFACE` | 只移除已不存在的 Stage Config 与旧 Data Catalog Validate 入口；Item Balance Workbench、WorldMap、Upgrade、BattleSandbox 等现行入口保留 |
| `V04ChapterFlowContractVerifier` | `KEEP / LEGACY_INTEGRITY_INFRA_RETIRED` | 保留 Manifest、Reducer、Projection、source-isolation；SHA/哈希/baseline/whitelist 与旧 V02 路径门禁已删除 |
| `BattlePrepareComponentAdapterRuntimePlaytest` shape helper、`V03BattlePrepareInteractionController`、`AutoCombatController` 连接岛 | `KEEP UNTIL RESPONSIBILITY CUTOVER` | ShapeAware validator 仍消费共享算法；禁止为清理建立第二 Adapter/Bridge/Test Framework |
| V02 `TutorialGuideService`、`V02RunFlowController`、`RewardConfig`、`IdleDropConfig` 等剩余 Runtime | `QUARANTINE / RETIRE_AFTER_RESPONSIBILITY_CUTOVER` | 对 Scene/Prefab 的脚本 GUID 序列化消费者均为 0；仍有旧 Resource key 文本，后续按完整责任岛处理，不在本包补兼容层 |

- 删除规模：307 个文件与 19 个空目录；正式 Build Settings 仍为 5 个 Scene。
- 离线全量编译：Runtime `0` 错误、Editor `0` 错误；仅保留 2 条既有未使用字段警告。
- Unity/Fresh Play：`NOT RUN`。本次是历史岛清理完成证据，不是 1-3 战斗效果或玩家里程碑完成证据。
- 当前状态：`V02_HISTORICAL_DATA_RESOURCE_TOOL_ISLAND_RETIRED / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY / STOP`。
# Formal Host Canonical Catalog Binding And Feeder Trace — 2026-08-11

- `UnifiedBattlePageShell.prefab / UnifiedBattleFormalSceneHost`: `KEEP / FORMAL_HOST`。已绑定唯一 `ItemBalanceWorkbenchCatalog.asset`；任务自有 Unity batch import/compile/serialization PASS。
- `ItemBalanceWorkbenchCatalog.asset`: `KEEP / CANONICAL_ITEM_DATABASE`。仍是唯一正式 Item Definition 数据源；本轮没有新建或恢复 Pool15 数据库。
- `C1CampaignStageClearRewardProgressBridge -> C1FormalItemSessionAuthority -> C1FormalRealtimeBattleSessionAdapter`: `KEEP / FORMAL_FEEDER_PASS`。静态追踪确认累计快照保留同一 ItemInstance、Canonical Definition、generated stats 与 placed/lit 状态，并仅将 placed+lit 行投影到 Session request。
- `C1FormalRealtimeBattleSession`: `KEEP / NEXT_RUNTIME_PROOF_BOUNDARY`。Session 输入接收 direct Item facts 并写入 `activeItemFacts`；本轮未证明正常路径真实 mutation，因此下一责任边界是 live Session mutation，而不是再次修改 Reward、Board、Catalog 或 Host。
- Package result: `FEEDER_PASS_SESSION_MUTATION_GAP / INTERNAL_QA / PLAYER_PATH_NOT_READY`。未运行 Fresh Play，未实现效果家族，未写死 I004。

## Formal Shell Exact Binding Diagnosis — 2026-08-11

- 玩家从 WorldMap 进入 1-1、1-2、1-3 均在共享 `UnifiedBattleFormalSceneHost.ValidateAuthoredBindings()` 阶段停止，Battle Session 尚未到达；该问题按共享 Formal Shell 处理，不按关卡或 Item 修补。
- Host 的旧聚合错误已拆分为 Shell slot、Prepare/Background、Item Surface hierarchy、Formal Presentation hierarchy、Combat Log、Item Presentation Catalog、Item Presenter、Prepare Surface、Navigation 与 Formal Presentation Root 十个责任层诊断，保持原有 fail-closed 行为。
- 现有 authoring owner 增加只读 Prefab/Scene 诊断入口，不保存资产、不重建 Scene/Prefab、不调用旧全量 authoring。
- 离线全量编译：Runtime `0` 错误、Editor `0` 错误；2 条既有未使用字段警告。
- 任务 Unity 与不加载工程的版本探针均在日志初始化前停住；任务进程已全部回收，无残留 Unity 与锁文件。因精确运行 predicate 尚未返回，本轮没有猜测性修改 Catalog、Board、Tray、Presentation、Battle、Reward 或 Stage。
- 静态证据确认 Scene 无关键 Host 覆盖、Shell 已绑定唯一 Canonical Catalog；旧 Formal Item Presentation Catalog 仍是 Board/Tray 的前置门并依赖旧 Pool15/Starter，标签维持 `REPLACE / SUSPECTED_SHARED_GATE / CUTOVER_ONLY_AFTER_EXACT_RUNTIME_DIAGNOSTIC`。
- 当前状态：`FORMAL_SHELL_EXACT_DIAGNOSTICS_COMPILE_PASS / USER_EDITOR_IMPORT_GATE / PLAYER_PATH_NOT_READY`。

### Formal Item Presentation Catalog Cutover — 2026-08-11

- 用户运行将共享失败精确到 `C1_ITEM_PRESENTATION_PROFILE_SIGNATURE_MISMATCH`；旧 Presentation Catalog 的 Pool15／Starter Profile 签名门禁被确认是第一断点。
- `C1FormalItemPresentationCatalog.cs`：`KEEP / PRESENTATION_ONLY / GAMEPLAY_AUTHORITY_REMOVED`。保留图片、效果族、样式、Cue、精确身份和 I031 明暗图结构校验；不再查询旧 Pool15、旧 Starter 或 Inner Catalog，不再用 Profile／Row／Catalog 签名决定运行合法性。
- 旧 Asset 中尚未由 Unity 保存清除的签名字段：`LEGACY_SERIALIZED_DATA / IGNORED_AT_RUNTIME / REMOVE_ON_FUTURE_OWNED_SERIALIZATION`。本次不手改 YAML，不建立迁移兼容层。
- Catalog／Row 下游身份改为可读稳定身份；Canonical Item Database 仍是玩法身份、属性与 Combat Fact 的唯一 Owner。
- 离线全量编译：Runtime `0` 错误、Editor `0` 错误；Unity／Fresh Play `NOT RUN`。当前状态：`PRESENTATION_CATALOG_AUTHORITY_CUTOVER_COMPILE_PASS / USER_EDITOR_IMPORT_RUNTIME_RECHECK / PLAYER_PATH_NOT_READY`。
- 用户 Editor 运行回传：正式关卡不再产生红色 Shell／Presentation Catalog 错误；`UNIFIED_EXACT_ITEM_DETAIL_RUNTIME_BINDING_IDENTITY` 为普通 `Debug.Log`，不是失败。状态更新为 `FORMAL_SHELL_BINDING_INTERNAL_PASS / PLAYER_PATH_NOT_READY / STOP`。

## Enemy Behavior Verifier Legacy Infrastructure Closure — 2026-08-11

- `BoneSwapRemnantRuntimeOperatorVerifier`、`C1Lv1EarlyEncounterBalanceVerifier` 与 `C1ShatteredHostPorcelainHoundRuntimeVerifier` 继续保留真实 operator、early encounter、目录/schema、调度、伤害、重置、负例、确定性与边界行为验证。
- 三个 Verifier 已原位移除旧 Assignment 绑定、保护文件表、文件完整性比较、写域白名单、重复报告生成、报告格式自证和仅服务于旧证据链的数据容器；没有建立新的 Test Framework。
- 共退休 24 份仅由该三文件拥有且活动代码/脚本消费者为 0 的专属历史报告；Starter baseline 与 Item Combat Effect Request 等非本岛所有的输入资料未顺手删除，继续按各自消费者图处理。
- 本子岛没有修改 Enemy Runtime、Stage、Reward、Item Authority、Battle Session、Scene/Prefab、Presentation、Save 或产品数值。
- 文件系统枚举下 Runtime 580 个 C#、Editor 181 个 C# 均离线编译通过；仅保留两条既有未使用字段警告。Hygiene Gate 由全局 63 降至 60、Editor 12 降至 9，仍为 `WARNING`。
- Unity 与 Fresh Play 均未运行；当前结果是 `FORMAL_AND_ENEMY_BEHAVIOR_VERIFIER_INFRASTRUCTURE_CLOSED / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY`，不是玩家路径完成声明。

## VFX Lab Verifier Legacy Integrity Closure — 2026-08-11

- `ItemLivingGradientOutlineVfxPrototypeVerifier` 与 `LiHuoCombatFeedbackOrchestrationVerifier` 继续保留静态资源／Profile／Shader／Cue Gate／Style Routing／Source Boundary、Rendered QA 和 Android 包内资产验证。
- 已移除固定文件完整性表和相关跨生命周期文件指纹门禁。Rendered QA 直接使用 Unity Scene dirty 状态确认未修改场景；Android QA 直接比较构建前后的 `PlayerSettings` Preloaded Assets 序列，证明临时构建输入已恢复。
- 没有修改 BattleSandbox Scene、VFX Runtime、材质、Shader、Profile、正式 Presentation、Item Authority、Battle、Reward、Stage、Save 或玩家语义，也没有新建验证框架或兼容层。
- 当前 TalismanBag 源码枚举为 Runtime 580／Editor 173；离线全量编译 0 错误。Hygiene Gate 由全局 60 降至 58、Editor 9 降至 7，仍为 `WARNING`。
- Unity／Rendered QA／Android Build QA／Fresh Play 本轮均未运行。当前标签为 `VFX_LAB_VERIFIERS_BEHAVIOR_ONLY_VALIDATION / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY`。

## ItemInstance Placement Binding Verifier Infrastructure Closure — 2026-08-11

- `ItemInstancePlacementBindingContractVerifier` 保留显式 `itemInstanceId`／`placementId`／`baseItemId` 三方绑定、确定性、重复／孤立／错配、I031 普通实例排除、不可变快照和异常状态验证，标签更新为 `KEEP / BEHAVIOR_ONLY_VALIDATION`。
- 已删除固定文件／目录完整性基线、前后文件比较、包文件清单、源码关键词泄漏扫描、重复报告写入与报告自证；没有改动 Runtime `ItemInstancePlacementBindingContract` 或 Validator。
- 三份仅由旧验证链拥有、活动代码与脚本消费者为 0 的专属报告已退休；历史 Assignment 中的文字链接保留为历史记录，不再是现行执行门。
- 离线全量编译 0 错误／0 警告；Hygiene Gate 由全局 58 降至 57、Editor 7 降至 6，仍为 `WARNING`。
- 未修改 Reward、Canonical Item Database、Item Authority、Board、Battle、Enemy、Stage、Scene／Prefab、Presentation、Save 或玩家语义；Unity 与 Fresh Play 均未运行。

## Shougunu Phase1 BattleSandbox Verifier Infrastructure Closure — 2026-08-11

- `ShougunuPhase1BattleSandboxVerticalSliceVerifier` 保留为 `KEEP / BEHAVIOR_ONLY_VALIDATION / QUARANTINE_LAB`：保留真实 Item Projection、50 次伤害应用、Shell/HP、敌方动作、Nian 联动、reset/stale、Scene 绑定、现有 UI 载体与正常 Editor Play Smoke 行为检查。
- BattleSandbox Scene 已经序列化持有 Runtime、SceneBinder 与 VisualCueAdapter 三个组件；旧 `InstallSceneBindings` 没有当前代码、菜单、CURRENT Task 或命令消费者，且会再次改写 Scene，因此作为一次性安装器退休。Scene、组件和序列化绑定均未修改。
- 删除固定文件门禁、源码字符串门禁、结构摘要、报告生成/回写和 7 份无活动消费者的专属历史报告。历史 Assignment 中的文字路径继续作为历史记录，不是现行执行依赖。
- I031 focused verifier 仍只读取该行为验证器的 `VerifyRealProjectionLaunch` 证据；该真实方法及其 I008 两邻接投影断言完整保留，没有为清理改写 I031 或运行语义。
- 离线全量编译：0 错误；现有序列化字段告警不属于本责任岛新增错误。Hygiene Gate：全局 55、Editor 5，`RoomStatus: WARNING`。Unity／Play Smoke／Fresh Play：`NOT RUN`。
- 未修改 BattleSandbox Runtime、Item/Enemy 数据、正式 Battle、Reward、Authority、Scene/Prefab、Presentation、Save 或玩家语义。下一低风险候选是 `BattleSandboxTrayStableLayoutAndManualSortVerifier`；其余 4 个为活动 authoring/serialization 工具，必须分别确认消费者后再决定收口。

## BattleSandbox Tray Stable Layout Verifier Infrastructure Closure — 2026-08-11

- `BattleSandboxTrayStableLayoutAndManualSortVerifier` 被现有 Tray authoring 直接调用，并拥有真实指针路径的 Editor 生命周期，标签为 `KEEP / BEHAVIOR_ONLY_VALIDATION / QUARANTINE_LAB`，不得删除整个类。
- 保留原子 Tray publication、Artwork 可见性、稳定过滤投影、行吸附、30 件普通道具 + I031 确定性装盘、Arrange Button Scene contract，以及真实 Category/ScrollRect 指针驱动。
- 删除源码关键词合同、文件摘要、报告生成与 7 份无活动消费者的专属历史报告；确定性继续由两次真实 placement signature 直接比较证明，不再写摘要值。
- 真实指针入口现在要求目标 Scene 开始前就是 clean；若存在未保存修改则明确失败。已移除通过反射清除 Scene dirty 状态的旧行为，验证器不会再替用户隐藏未保存状态。
- 现有 `BattleSandboxTrayStableLayoutAndManualSortAuthoring.ApplyVerifyAndSaveClosedCleanBatch()` 调用关系保持不变；没有运行 authoring、没有保存 Scene，也没有改 Board/Tray Runtime。
- 离线全量编译 0 错误；Hygiene Gate：全局 54、Editor 4，`RoomStatus: WARNING`。Unity／真实指针 QA／Fresh Play：`NOT RUN`。
- 剩余 4 个 Editor 命中均为活动 authoring/serialization 工具：Item Promotion、Exact Detail Popup Prefab、Navigation、Unified Popup Mount。下一阶段先建四岛消费者图，再逐岛决定 `KEEP + REPLACE_LEGACY_INTEGRITY_INFRA`，禁止批量删除。

## Exact Item Detail Standalone Prefab Authoring Infrastructure Closure — 2026-08-11

- `C1ExactBattleSandboxItemDetailPopupStandalonePrefabAuthoring`: `KEEP / DIRECT_STRUCTURAL_COMPARE / PRODUCTION_AUTHORING_OWNER`。仍拥有现有 Standalone Popup Prefab 的提取、清样本、去外部引用与结构一致性验证职责。
- 冻结文件签名、外部保护文件字节门、报告生成和 PrefabStage 展示性检查：`DELETE / RETIRED`。Authoring 只验证它实际读取的 Source Scene 与实际写出的 Prefab，不再维护旁路完整性证据。
- `C1ExactBattleSandboxItemDetailPopupStandalonePrefabParity.md`: `QUARANTINE / RETIRE_WITH_POPUP_MOUNT_CONSUMER_CUTOVER`。当前仍被 Unified Popup Mount Authoring 的旧保护输入表引用，本岛不跨责任边界删除。
- 编译：`PASS / 0 ERRORS`。Unity／Authoring／序列化／Fresh Play：`NOT RUN`。下一岛：`C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring` 与其直接测试消费者。

## Generic Reward ItemInstance State-Sink Proof Attempt — 2026-08-12

- `C1FormalObtainRebuildBattleLoopTests` 已移除 `I004`、`Pool15` 和内部 `Golden Path` 固定身份，改为读取算法实际奖励实例及其 Canonical `combatEffect`。
- 真实 sink 判据已按 HP/Shell、Guard、Player HP、Nian、Action progress 与 BattleStatusState 分离；禁止用伤害代替非伤害效果。
- 首个断点仍在奖励效果执行之前：旧 runner 把 1-1 奖励留在 Tray，要求旧 Build 先赢 1-2，之后才 placed+lit 进入 1-3；该顺序不符合正式“领奖重排后进入下一关”路径，并在 `accepted real-time victory 1-2` 停止。
- Runtime/Editor 定向编译 `0` 错误；任务 Unity batch PID `6828` 已退出。Runtime、数值、Scene/Prefab、Presentation、Reward、Authority 与 Battle Session 均未修改。
- Ledger 状态：`GENERIC_REWARD_PROOF_STOPPED_BEFORE_PLACED_LIT / QA_HARNESS_LIFECYCLE_GAP / NO_PRODUCT_FIX / STOP / NOT_READY`。

## Generic Reward Next-Battle Lifecycle Proof — 2026-08-12

- `C1FormalObtainRebuildBattleLoopTests` 已原位纠正为正式顺序：1-1 领奖 → 同一 Prepare 摆放并点亮实际奖励 → 1-2 使用最新累计快照；不再要求未使用奖励的旧 Build 先赢 1-2，也不再把证明推迟到 1-3。
- 1-2 focused Session 已实际启动并进入 Status 调度；首个运行失败为 `EVENT_BUFFER_EXCEEDED`，栈位于 `ProcessStatusTick → ScheduleStatusAction → AddAction`。
- 只读合同核对显示 verifier 过度要求 `ItemTriggerAccepted.effectVariantId == Canonical EffectId`。现有基础 `ItemTriggerAccepted` 已携带同一 ItemInstance/BaseItem/application identity，而 Status 的效果身份与 mutation 由 `StatusApplied / StatusRefreshed` 及 `BattleStatusState` contribution 证明。当前失败因此先归类为 QA Cue 识别缺口，不能提升为 Runtime event leak 或 State Sink 失败。
- Runtime/Editor 定向离线编译 `0` 错误；任务 Unity PID `17256` 已自然退出，无 Unity Editor 残留。Runtime、数值、Scene/Prefab、Presentation、Reward、Authority、Battle Session 均未修改。
- Ledger 状态：`QA_HARNESS_CUE_IDENTITY_GAP / EVENT_BUFFER_EXCEEDED_IS_DOWNSTREAM_SYMPTOM / NO_RUNTIME_FIX / STOP / NOT_READY`。

## Canonical Reward Cue Identity Proof Closure — 2026-08-12

- `C1FormalObtainRebuildBattleLoopTests`: `KEEP / BEHAVIOR_ONLY_VALIDATION / CUE_IDENTITY_CORRECTED`。基础 action commit 现在按同一 ItemInstance、BaseItem 与 application identity 识别，不再要求所有效果族在基础 Cue 中重复填入 EffectId。
- State Sink 证明继续由实际奖励自己的 Canonical plan 决定；本轮算法奖励为 `I013 / signature_i013 / PlayerGuardFlat`，真实 `PLAYER_GUARD` 增加，未改写成伤害语义。
- 首次运行已移除旧 `EVENT_BUFFER_EXCEEDED` 假象；同包去掉 Guard 的额外 effect-specific Cue 过严要求后，任务 Unity PID `21804` 以 `PASS assertions=72` 自然退出，无 Unity 残留。
- Runtime、Battle Session、Canonical Item 数据、数值、Scene/Prefab、ScriptableObject、Presentation、Reward、Authority、Save 均未修改；没有新建 Battle/Test Framework 或兼容层。
- Ledger 状态：`CANONICAL_FORMAL_FEEDER_INTERNAL_QA_PASS / ACTUAL_REWARD_STATE_SINK_PASS / NO_RUNTIME_FIX / STOP / PLAYER_HANDTEST_NOT_RUN`。

## Canonical Reward Player Handtest Gate — 2026-08-12

- 用户正常路线实际奖励不固定：回报包含 1-1“离火符”和 1-2“震雷破壳印”。这支持唯一 Canonical 奖励线路正在产生不同道具，但不等于对应 Battle mutation 已由玩家画面证明。
- 用户在 1-2 因敌人血量过厚未能完成当前路线；玩家首阻塞转为 `CAMPAIGN_NORMAL_LV1_STAGE_1_2_EARLY_CURVE_BLOCKER`。
- Cue identity 与 focused feeder 内部 PASS 不回退；玩家 mutation 可见性保持 `UNPROVEN_BY_HANDTEST`。禁止据此重新补 Item/Cue/Presentation，亦禁止直接写死某个 Item 或只削单个敌人。
- 下一候选责任岛：`CANONICAL_LV1_POWER_BAND_AND_EARLY_STAGE_CURVE_AUDIT / READ_ONLY_FIRST`。先统一审计 1-1～1-5 正式曲线，再由独立授权决定最小数值调整。

## Canonical LV1 Power Band And Early Stage Curve Audit — 2026-08-12

- 当前正式初始普通道具是 I007 离火符：白色 8～12 伤害、2.0～2.4 秒 cadence；I031 为 27 上限、每 500 ms 生成 1 念。该 Current Canonical 运行带已经取代历史 I001 82 伤害/2 秒参考带。
- 现行有效耐久为 1-1=48、1-2=1920、1-3=1900（含壳）、1-4=1750（含壳并有 Bone Swap）、1-5=1900（含壳并有 Bone Swap）。1-2 相比 1-1 突增 40 倍，且高于所有后续关卡裸血。
- 玩家回报的双离火符 1-2 路径在枚举白色属性范围后为 0/400 胜利，58.5 秒前仅造成约 382～575 总伤害；320 HP 时 400/400 胜利、中位 TTK 42 秒。根因归类为 `HISTORICAL_ITEM_POWER_ASSUMPTION_STILL_OWNS_ENEMY_DURABILITY`，不是 Reward、ItemInstance、Cue 或 Presentation 回归。
- 只读审计推荐：1-1 保持 48；1-2 raw HP 280～320；1-3 raw HP 200～240 并保留两层现有 100 Shell；1-4 raw HP 340～400；1-5 raw HP 420～500。1-4/1-5 仍须在同一未来 Enemy 包内用当前 Session 的四/五道具合法 Build 选定区间内值。
- 本轮未修改 Enemy、Item、Battle、Reward、Stage、Scene/Prefab、Presentation、Save 或 Unity 资产；未运行 Unity/Fresh Play。
- Ledger 状态：`EARLY_CURVE_AUDIT_COMPLETE / HISTORICAL_POWER_BAND_MISMATCH_CONFIRMED / ENEMY_ONLY_PLAYTEST_V1_PACKAGE_RECOMMENDED / NO_PRODUCT_CHANGE / STRATEGY_DECISION_REQUIRED / STOP`。
## 2026-08-12 ENEMY_ONLY_EARLY_CURVE_PLAYTEST_V1

- Status: `IMPLEMENTATION_ACTIVE / INTERNAL_QA_PENDING`.
- Owner: existing Enemy balance/data contracts; current visible task, no second task or Worktree.
- Current PLAYTEST_V1 effective durability budget: `1-1 48 / 1-2 180 / 1-3 40+200 Shell / 1-4 180+100 Shell+BoneSwap / 1-5 220+100 Shell+BoneSwap`.
- KEEP: current rosters, attack cadence, Shell facts, BoneSwap, Item database, initial acquisition policy, Reward, I031 Nian, Battle, Stage flow, Presentation and Save.
- Replaced values: obsolete Enemy HP rows derived from the retired I001 82-damage / 2-second reference.
- Completion remains gated by existing exact validation and one later normal-route player check; no player-ready claim is made here.

## 2026-08-12 Enemy Early Curve Internal Closure

- Authoritative effective durability is now `1-1 48 / 1-2 180 / 1-3 240 / 1-4 280 / 1-5 320`; roster, cadence, Shell and BoneSwap semantics are unchanged.
- Existing focused Enemy verification passes for 1-1/1-2 and for 1-3 through 1-5; the latter completed 195 assertions.
- Runtime and Editor offline full compilation both pass with 0 errors.
- `C1Lv1EarlyEncounterBalanceVerifier`: `KEEP / ENEMY_ONLY_BEHAVIOR_VALIDATION`. Retired I001/I002 starter projections, fixed-Build TTK scenarios and historical sensitivity fixtures were removed from this Enemy verifier so retired Item power assumptions cannot gate current Enemy data.
- Item, Reward, Nian, Battle, Stage flow, Scene/Prefab, Presentation, VFX and Save were not changed.
- Status: `ENEMY_EARLY_CURVE_PLAYTEST_V1_INTERNAL_QA / PLAYER_HANDTEST_NOT_RUN / WAITING_PLAYER_HANDTEST`.

## 2026-08-12 I031 Formal Load Envelope Implementation

- 全 30 件 Canonical 凡品的持续负载已按真实 `nianCost / (cooldownUnits * 0.4s)` 统一审计：单件中位 2.50、五件 P95 15.13、五件最大 18.75 念/秒；旧 I031 的 2 念/秒是三件构筑停火的首个真实断点。
- `C1FormalI031NianCapacityProjection`: `KEEP / FORMAL_NIAN_BASE_PROFILE_V1`。Lv1 更新为初始/上限 50、每 500 ms 生成 10，即 20 念/秒；该包络覆盖任意五件凡品最坏持续负载与 45 点最坏同拍消耗，不绑定 Stage 或 Item ID。
- `C1FormalRealtimeBattleSession`: `KEEP / FORMAL_NIAN_READY_SCHEDULER`。念力不足的 Item 现在等到下一次 I031 供念事件重试，不再重新等待完整 cooldown；扣念、真实 mutation 与同源 cue 的既有先后关系保持不变。
- `C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier`: `KEEP / BEHAVIOR_ONLY_VALIDATION`。新增五件凡品满载和强制超载公平重试两条证明；没有新建 Battle/Test Framework。
- Enemy、Reward、Board/Tray、I001-I030 数据、Scene/Prefab、Presentation、Save 和 Item Detail 未修改；旧 BattleSandbox Nian Engine 未接回正式线路。
- 设计合同：`Docs/CURRENT/I031_FORMAL_RULE_AND_LOAD_ENVELOPE_DESIGN.md`。
- 当前状态：`IMPLEMENTED / STATIC_AUDIT_PASS / UNITY_QA_NOT_RUN / USER_HANDTEST_FAIL / NOT_READY`。下一门只允许 Unity 编译、既有 focused verification 与随后一次 WorldMap 正常路线；不得用 Enemy 或单 Item 补丁替代。

## Canonical Detail And Presentation Consumer Cutover — 2026-08-13

- `C1FormalItemDetailProjectionAndSelection`: `KEEP / CANONICAL_SEMANTIC_DETAIL`。普通道具详情不再假设每件道具都有 `damage`；无伤害道具保留自己的 rolled stats 与 Canonical effect 语义，重复 damage 与缺失/重复 cooldown 仍在真实 Owner 边界拒绝。
- `CanonicalItemDefinitionResolver`: `KEEP / SINGLE_ITEM_PRESENTATION_OWNER`。向正式消费者提供普通道具 Canonical definitions；没有新增数据库或 Adapter。
- `C1FormalItemPresentationCatalog.asset`: `ASSET-KEEP / SPECIAL_PRESENTATION_RESOURCE_ONLY`。运行时普通道具行已被 Canonical 动态投影取代；旧普通行不再拥有普通道具名单，I031 明暗图继续保留。
- `C1ExactBattleSandboxItemBoardView` 与 `UnifiedBattleFormalSceneHost`: `KEEP / CANONICAL_PRESENTATION_CONSUMER`。Host 创建唯一 resolver 后交给现有 Board/Tray/Source Anchor 链；没有改 Scene、Prefab、Reward、Battle、Enemy、Stage、Save 或 VFX 设计。
- I001-I030 的 Presentation 身份已全部回收到唯一 Canonical Profile；运行时由 resolver 动态生成 30 件 × 5 品阶普通展示行，旧 Presentation Catalog 只保留 I031 特殊明暗图资源职责。
- 新补齐身份按 Canonical effect 的真实结果归类而非按 Item ID 猜测：破壳/控制进入 `ENEMY_CONTROL`，直接伤害/状态/念节奏进入 `DIRECT_TEMPO`，护势/治疗/净化进入 `GUARD_SUSTAIN`；没有新建第六家族、第二张表或 Runtime per-ID 分支。
- `CanonicalItemFoundationVerifier`：`KEEP / SINGLE_SOURCE_PRESENTATION_COVERAGE / UNITY_QA_PASS`。覆盖 30 个正式 Presentation 身份、150 个普通精确展示行与唯一 I031 特殊行；离线 Runtime/Editor 全量编译均为 `0` 错误。随后 owned Unity Batch 完成导入、全量脚本编译和 verifier，输出 `PASS catalog=31 ordinaryDrop=30 combatRules=30 rarityVersions=150` 并以 return code `0` 自然退出；Fresh Play 仍未运行。
- 离线全量编译：Runtime `0` 错误，Editor `0` 错误；Unity/Fresh Play `NOT RUN`。状态：`DETAIL_GATE_FIXED / CANONICAL_PRESENTATION_CONSUMER_CUTOVER / SEMANTIC_GAP_15 / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY / STOP`。

## 2026-08-13 Detail / Causal Source Two-Gate Ledger

- `C1ExactBattleSandboxItemDetailPresenter.ValidateFormalProjection`: `KEEP / CANONICAL_SEMANTIC_DETAIL_GATE`。移除错误的全局 `directDamage > 0` 前提；无 per-ID 分支，非伤害定义仍必须满足现有正式投影完整性。
- `C1Pool15FormalItemArtworkAndSourceAnchorBinding.TryCreateCurrentSourceAnchorRequest`: `KEEP / SINGLE_CAUSAL_SOURCE_REQUEST_OWNER`。保持原行为与单一 Owner，只增加首个拒绝条件输出，供真实运行定位共享接缝。
- `FormalBattlePresentationRoot.TryResolveCausalSource`: `KEEP / CAUSAL_PRESENTATION_CONSUMER`。现有拒绝日志现在包含 exact instance/base/rarity 与首个条件；不得以 cue/VFX 成功替代 Battle mutation。
- `C1FormalItemDetailProjectionAndSelectionTests`、`C1ExactBattleSandboxItemPromotionTests`: `KEEP / FOCUSED_EXISTING_FRAMEWORK`。覆盖非伤害详情接受和 placed+lit 来源请求空拒绝原因；没有第二套测试框架。
- 验证：Runtime/Editor 离线编译均 `0` 错误；Unity/Fresh Play `NOT RUN`。状态：`INTERNAL_CODE_CLOSE / REAL_RUNTIME_CAUSAL_REJECT_PENDING / NOT_READY / STOP_BEFORE_GUESS_PATCH`。

### Runtime source identity closure

- `C1Pool15FormalItemSourceAnchorRequest.expectedInstanceCanonicalSignature`: `KEEP / OPAQUE_INSTANCE_IDENTITY`。实例身份必须逐字保留，不得作为普通标签文本裁剪；这条规则同时适用于初始获取与 Reward 生成实例。
- 首断点证据：`SNAPSHOT_INSTANCE_IDENTITY_MISMATCH`，而 item/base/rarity 已一致。原构造器裁掉生成实例身份的末尾结构，导致当前请求错误地被判为 stale。
- 原位修复完成；未修改 Item 数据、Reward、Battle、Scene/Prefab、Enemy、Stage、Save 或 Presentation 设计。Runtime/Editor 离线编译均为 `0` 错误。
- 状态：`SINGLE_SOURCE_IDENTITY_PRESERVED / CAUSAL_SOURCE_SHARED_SEAM_FIXED / PLAYER_CONFIRMATION_PENDING`。

### Canonical non-damage detail sections

- `C1FormalItemDetailProjectionAndSelection.CreateFormalViewModel`: `KEEP / ALL_EFFECT_CANONICAL_DETAIL_PROJECTOR`。所有 Canonical 效果家族先生成现有 UI 的 stats/trigger/basic 分区；直接伤害格式化器只覆盖自己的展示文案，不再拥有弹窗准入权。
- `C1ExactBattleSandboxItemDetailPresenter.ValidateFormalProjection`: `KEEP / STRUCTURAL_VISIBLE_GATE`。继续检查实例、图片与有效内容，但不得要求 Cleanse、Guard、Heal、Control 伪装成 direct damage。
- 玩家证据：紫色 I020 在 1-2 奖励后点击无弹窗；共享归因为非伤害分区缺失。修复未改 I020 数据、Battle Cleanse 语义、Reward、Scene/Prefab 或 UI 样式。
- Runtime/Editor 离线编译均 `0` 错误；状态：`INTERNAL_FIX_COMPLETE / PLAYER_RETEST_PENDING`。

## Canonical Cleanse Core Loop Correction — 2026-08-13

- `CanonicalItemDefinitionResolver`: `KEEP / SINGLE_ITEM_SEMANTIC_OWNER / GENERATED_AFFIX_RESOLUTION`。固定与随机词缀都回到同一 Canonical Catalog 解析；没有增加第二张道具表或 Pool15 语义源。
- `CanonicalItemDropPolicy`: `KEEP / CAPABILITY_AWARE_ADMISSION`。只有下一场正式战斗具备 `player.status.cleanseable` 时，依赖净化目标的定义才进入候选；无关普通道具不受影响。
- `C1CampaignDirectLootPoolAndPolicy`: `KEEP / NEXT_ENCOUNTER_CAPABILITY_FEEDER`。能力来自下一关 Actor 同一行对齐的 Shattered Host action/request，不来自 Item 身份或专用关卡奖励表。
- `C1FormalRealtimeBattleSession`: `KEEP / PLAYER_STATUS_AND_CLEANSE_STATE_OWNER`。现有敌方攻击波施加有界玩家污染；现有 Session 清除它，并且只在可提交真实 mutation 后扣念、在实际移除后发 exact-source 成功 cue。
- Generated Cleanse affixes: `KEEP / CANONICAL_ROLLED_EFFECT_SOURCE`。本包只释放 Cleanse 家族；其他 generated affix 语义仍不在本包扩展。
- `on_cleanse` ownership: `EXACT_ITEM_INSTANCE`。连锁与水充能不再跨棋盘全部净化道具累计，只推进并释放本次实际行动实例自己的派生效果。
- `C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier`: `KEEP / BEHAVIOR_ONLY_VALIDATION`。原验证器新增真实污染移除、无状态 NoMutation、能力准入三项证明；没有第二套测试框架。
- Product data、数值、Scene、Prefab、Presentation/VFX、Detail、Save、Board/Tray、Item Authority 与 I031 均未修改。
- 首次 focused execution：真实污染在 4500 ms 建立，但 verifier 在 5000 ms 检查；其输入 cooldown 实际把 Cleanse 首次行动排到 8000 ms，因此该失败归类为 `QA_HARNESS_TIMING_GAP`，不能归类为已执行 Cleanse 未移除状态。
- verifier 已原位改为合同真实时序：有污染路径 4500 ms Apply、4800 ms Cleanse；无污染路径 4000 ms Cleanse、早于污染。修正后 Editor 离线编译 `PASS / 0 errors`。
- 验证：Runtime 离线编译 `PASS / 0 errors`；Editor 离线编译 `PASS / 0 errors`；focused verifier rerun `PENDING`；Fresh Play `NOT RUN`。
- 状态：`CANONICAL_CLEANSE_IMPLEMENTED / QA_HARNESS_TIMING_GAP_FIXED / FOCUSED_VERIFY_RERUN_PENDING / PLAYER_PATH_NOT_READY`。

## Canonical Activation / Application Semantic Closure — 2026-08-13

- `C1FormalRealtimeBattleSession`: `KEEP / SINGLE_ACTIVATION_AND_APPLICATION_OWNER`。合法的 scheduled placed+lit Item 行动不再因预测 application=0 被提前拒绝；现有生命周期、活目标、念力、冷却与重试边界保持不变。

### Per-Actor Enemy Action Water Closure｜2026-08-17

- `C1FormalRealtimeBattleSession`: `KEEP / PER_ACTOR_ENEMY_ACTION_OWNER`。旧的单一共享 `ENEMY_BASIC_WAVE` 已收口为每个存活 Actor 自己的 Action；精确来源、伤害份额、间隔、动作请求与 Player 目标留在同一 Session 调度器内，没有第二套 Enemy Runtime。
- 原 Attack Wave 总伤害预算按稳定 Actor 顺序分配，避免多敌人导致总伤害倍增；击败 Actor 只取消自己的待执行基本攻击与既有 BoneSwap 待执行动作，其他 Actor 继续。
- `C1FormalRealtimeBattleSessionVerifier`: `KEEP / PER_ACTOR_ENEMY_ACTION_FOCUSED_PROOF`。Unity 结果为 `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=3`；只证明独立调度、精确来源真实扣血和死亡取消边界，不替代正常玩家路线。
- 下一责任边界：`Formal Battle Presentation / DECOR`。攻击动画与来源可读性必须按 `cue.sourceId` 选择对应 Enemy Slot；不得继续按 target 或第一个存活敌人猜来源。
- Activation 与 Application 已分层：全血 Heal、无负面状态 Cleanse 仍扣除一次念力并发送 exact-source `ItemTriggerAccepted`，但不伪造 HP、Status、Hit、伤害数字或 Cleanse 成功连锁。
- `ApplyCanonicalBaseCleanse`: `KEEP / SUCCESS_ONLY_FOLLOW_UP_BOUNDARY`。只有真实移除玩家负面状态后才推进同实例 chain/charge 并释放 `on_cleanse`。
- `CanCanonicalActionMutate`: `DELETE / OBSOLETE_PREDICTION_GATE`。该私有预测门把“本次收益为零”误判为“未释放”，已从唯一 Formal Session 原位删除，没有建立替代 Manager、Bridge 或第二状态系统。
- `C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier`: `KEEP / BEHAVIOR_ONLY_VALIDATION`。新增无状态 Cleanse 与全血 Heal 的 zero-benefit activation 证明，并继续保留真实 Cleanse success 与 Tray/unlit rejection；通用语义选择不依赖 Item-ID 分支。
- Product data、数值、Reward、Authority、Board/Tray、Scene/Prefab、Presentation/VFX、Detail、Enemy、Stage、Save 与 I031 均未修改。
- 验证：Runtime 离线编译 `PASS / 0 errors`（两条既有未使用字段 warning）；Editor 离线编译 `PASS / 0 errors`（一条既有 obsolete Sprite API warning）；Unity/focused verifier/Fresh Play `NOT RUN`。
- 状态：`ACTIVATION_APPLICATION_SEMANTIC_IMPLEMENTED / OFFLINE_COMPILE_PASS / FOCUSED_VERIFY_PENDING / PLAYER_PATH_NOT_READY`。

## Chapter 1 Canonical Enemy Water Closure — 2026-08-18

- `C1FormalEnemyDefinitionCatalog`: `KEEP / SINGLE_FORMAL_ENEMY_DEFINITION_OWNER`。碎骨附身者、骨瓷犬与换骨残相的 HP、Shell、基础攻击伤害、首击时间、攻击间隔和已释放机制统一由一套 PLAYTEST_V1 定义提供；不是把战斗真相写入视觉 Prefab。
- `V04Chapter1StageWaveEncounterTruth`: `KEEP / STAGE_COMPOSITION_OWNER`。只声明 Stage -> Wave -> Actor 组成和顺序；正式 1-1～1-5 通过同一 Enemy 定义解析每个 Actor，不再由分关卡目录复制敌人数值。
- `C1FormalRealtimeBattleSession`: `KEEP / LIVE_ENEMY_INSTANCE_AND_ACTION_OWNER`。每个存活 Enemy 实例拥有自己的当前 HP/Shell/状态和攻击 Action；按自己的伤害与节奏提交玩家 HP mutation，死亡只取消自己的待执行行动，其他敌人继续。
- 现有污染、Shell 与 BoneSwap operator 保持原位。换骨残相通过 released operator profile 绑定既有回击流程，没有新增第二套 Enemy Runtime、Manager、Bridge 或按 Stage/Enemy ID 的 Session 分支。
- `C1Lv1EarlyEncounterBalanceCatalog` 与 `C1Stage3To5PlaytestEnemyEncounterCatalog`: `QUARANTINE / LEGACY_FORMAL_FEEDER_REPLACED`。当前 1-1～1-5 正式 Adapter 不再消费它们；本包不裸删，待独立消费者图确认其剩余 verifier/工具引用后再退休。
- `C1FormalRealtimeBattleSessionVerifier`: `KEEP / EXISTING_FOCUSED_BEHAVIOR_PROOF`。同一验证入口聚合证明五关组成、共享数值、Hound Shell、Remnant operator、每实例独立攻击、精确来源伤害和死亡取消；Unity 结果 `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`。
- 验证：Unity 脚本编译通过，聚焦验证通过；任务自有 Unity 已自然退出且 Lock 已清除。未运行 WorldMap 正常路线 Fresh Play。
- 保护域未修改：Reward、Item、I031/Nian、Save、WorldMap、Scene/Prefab、Presentation/HUD 与装修资产。
- 状态：`CHAPTER1_CANONICAL_ENEMY_INTERNAL_QA_COMPLETE / PLAYER_PATH_NOT_RUN / STOP`。

## Chapter 1 Enemy PLAYTEST_V2 Pressure Correction — 2026-08-18

- 玩家证据覆盖上一轮内部结论：100 HP 玩家无法承受 PLAYTEST_V1 的每实例完整攻击，状态曾为 `USER_HANDTEST_FAIL`；保留独立行动架构，只撤回失配数值。
- `C1FormalEnemyDefinitionCatalog`: `KEEP / PLAYTEST_V4_EARLY_HOST_HP_SINGLE_ENEMY_DATA_OWNER`。V3 统一翻倍使三只碎骨组成的 1-2 对一次随机奖励后的早期 Build 风险过高，因此只回调共享碎骨模板为 `60 HP / 2伤害 / 2.4秒首击 / 4.0秒间隔`；骨瓷犬保持 `160 HP + 60 Shell`，换骨残相保持 `280 HP`，全部攻击与技能不变。1-1～1-5 总耐久为 `120 / 180 / 280 / 500 / 560`，仍禁止关卡专属覆盖。
- `C1FormalRealtimeBattleSession`: `KEEP / GENERIC_ENEMY_STARTUP_PHASE_OWNER`。只在初始调度按稳定 Actor 顺序增加通用 400 ms 相位，解决同类敌人同帧合击；后续攻击仍按各自模板间隔循环，不含关卡或 Enemy-ID 特判。
- 1-1～1-5 的共享模板有效耐久为 `80 / 120 / 180 / 280 / 320`。100 HP、无防御、无治疗且完全不击杀敌人的保守基础攻击生存时间为 `98.8 / 66.8 / 63.0 / 53.4 / 42.2秒`；该计算不冒充真实构筑胜率。
- 污染继续使用现有有界玩家状态；本包未新增污染持续伤害。Boss与1-6～1-10正式运行数值未进入本包。
- 玩家侧保护域未修改：Player HP、Item、I031/Nian、Guard/Heal算法、Reward、Save、WorldMap、Scene/Prefab、Presentation/HUD均保持不变。
- 验证：Unity编译通过；既有聚焦验证 `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`；任务Unity自然退出且Lock清除。
- 状态：`ENEMY_PLAYTEST_V2_INTERNAL_QA_COMPLETE / PLAYER_RETEST_PENDING / STOP`。

## Chapter 1 Enemy Skill Component Runtime — 2026-08-18

- `C1FormalEnemyDefinitionCatalog` 与 `C1FormalEnemySkillDefinitions`：`KEEP / SINGLE_ENEMY_AND_SKILL_DATA_OWNER`。Enemy 只承载基础属性和 Skill 列表；Skill 统一按 Active/Passive、触发条件、目标、效果和 Status 行为组合，不在 Session 按 Enemy ID 或 Stage ID 写分支。
- 碎骨附身者：基础攻击命中叠加污染；污染持续 6 秒、每 2 秒结算、最多 3 层、叠层并刷新，每层每次造成 1 点真实伤害；主动污染打击按 4 秒首次、8 秒冷却、0.8 秒施法，造成 5 点直伤并叠一层污染。
- 骨瓷犬：初始 Shell 不再是静态快照赠送，而由通用 OnSpawn Shield 被动真实提交 60 Shell；主动冲撞按 5 秒首次、9 秒冷却、1 秒施法造成 11 点真实伤害。
- 换骨残相：主动返相窗口按 1.5 秒首次、5 秒冷却、0.7 秒施法施加 8 秒正面状态，最多 2 层并刷新；OnDamaged 被动只在窗口存在时立即反伤，每层 10%，单次上限 6，正伤害最低返回 1。它不再伪装成敌人基础攻击，也不走旧延迟返伤支线。
- `C1FormalRealtimeBattleSession`：`KEEP / GENERIC_ENEMY_SKILL_TRIGGER_EFFECT_STATUS_OWNER`。统一处理 OnSpawn、Timer、OnBasicAttackHit、OnDamaged，以及 DirectDamage、Shield、ApplyStatus、ReflectDamage；状态统一拥有真实叠层、刷新、tick、expire、精确来源 Actor/目标和 Cue。
- 旧 `BoneSwapBinding`、telegraph-wake、delayed-return 与 pending/recovery 运行入口已从当前 Formal Session 删除；旧合同常量和独立历史验证资产暂留兼容区，不属于当前正式执行链。
- 表现交接：装修只消费现有 `EnemySkillScheduled`、`EnemySkillAccepted`、`EnemyPassiveTriggered` 以及 `StatusApplied/Refreshed/Tick/Removed` Cue，按精确 source Actor、target、effect identity 播放；不得反推技能或改 Battle 事实。
- 验证：Unity 编译通过；`ENEMY_SKILL_COMPONENT_RUNTIME_PASS checks=4`；`PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`；Unity 自然退出且 Lock 清除。未运行 Fresh Play。
- 保护域未修改：Player HP、Item/I031/Nian、Reward、Save、Mainline、Stage composition、Scene/Prefab、Presentation/HUD/VFX。
- 状态：`ENEMY_SKILL_COMPONENT_RUNTIME_INTERNAL_QA_COMPLETE / PLAYER_VISUAL_AND_FRESH_PLAY_PENDING / STOP`。

## Battle Feedback Fact Contract V1 — 2026-08-18

- `C1FormalRealtimeBattleCue`: `KEEP / SINGLE_IMMUTABLE_FEEDBACK_FACT_CARRIER`。在既有精确 source、target、requested/applied 与 accepted application identity 上补齐 `sourceStableOrder / triggerKind / deliveryKind / resultKind`，没有新建第二套 Feedback 状态机或语义数据库。
- `C1FormalRealtimeBattleSession`: `KEEP / POST_COMMIT_FEEDBACK_OWNER`。Basic、Skill、Passive、Item、Status、Shell、Guard、Heal、Control、Cleanse 与 Nian 结果都在真实 mutation 后携带显式分类；玩家 Guard 吸收伤害新增 `PLAYER_GUARD_CHANGED / GUARD_DAMAGE` 事实，装修不再从 HP 未变化猜护盾承伤。
- `C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier`: `KEEP / CANONICAL_ITEM_EFFECT_PROOF`。直接伤害代表按唯一 Canonical Catalog 的效果语义动态选择，不写具体 Item ID；验证器 Enemy 入口改用当前 `C1FormalEnemyDefinitionCatalog.CatalogId`，并把 Cleanse 测试边界对齐当前 Enemy Skill 时序。
- 装修交接字段：`sourceOwner/sourceId/sourceItemInstanceId/sourceStableOrder`、`triggerKind/deliveryKind/resultKind`、`targetActorId/targetStableOrder`、`requestedDamage/appliedDamage`、`acceptedApplicationEventId`。装修只消费这些 immutable facts 做 Source 发光、路径、Receiver Lane、数字与状态反馈，不反推战斗语义。
- Unity 内部验证：`BATTLE_FEEDBACK_FACT_CONTRACT_PASS checks=6`；`CanonicalEffectRuntimeProof COMPLETE checks=43`；`ENEMY_SKILL_COMPONENT_RUNTIME_PASS checks=4`；`PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`。任务 Unity 已退出且 Lock 清除。
- 保护域未修改：Scene/Prefab/Profile/Presentation/HUD/VFX、Item/Enemy 数值、Reward、Save、Mainline、Stage。Fresh Play 未运行。
- 状态：`BATTLE_FEEDBACK_FACT_CONTRACT_INTERNAL_QA_COMPLETE / DECOR_HANDOFF_READY / PLAYER_PATH_NOT_READY / STOP`。
## Historical Consumer Closure V1 — 2026-08-18

- `C1CampaignDirectLootPoolAndPolicy`: `KEEP / FORMAL_ENEMY_TRUTH_CONSUMER`。下一战可净化威胁判断已改接 `V04Chapter1StageWaveEncounterTruth + C1FormalEnemyDefinitionCatalog`；掉落算法、概率和 Item Truth 未改变。
- `C1FormalRealtimeBattleSessionAdapter`: `KEEP / CANONICAL_ENEMY_CATALOG_ONLY_FOR_1_1_TO_1_5`。当前正式阶段不再保留不可达的旧 Catalog 选择分支；Pool15 overload/API 仍属独立兼容迁移岛。
- `C1FormalRealtimeBattleEncounterResolver`: `KEEP / CANONICAL_RESOLVER_ONLY`。已删除不可达的 early / stage3to5 resolve、旧 profile validation 与旧 BoneSwap row conversion；正式 resolver 行为未改变。
- `C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier`: `KEEP / QA_HELPER_DEBT_CLOSED`。移除未参与触发计算的 `CreateItemFact.firstOffsetMs` 参数；真实 fair-retry Battle 时间判据保留。
- `C1Pool15FormalItemArtworkAndSourceAnchorBinding`: `KEEP / LIVE_FORMAL_NAMING_DEBT / DO_NOT_DELETE`。它仍是现行 Card 图片与因果来源绑定，不是旧 Pool15 数据消费者。
- Pool15 Session/Contract/API/old verifier island: `QUARANTINE / MIGRATION_REQUIRED`。正式 Adapter 已提供空 Pool15 facts，但 Contracts、Session、Authority 字段和旧 Editor QA 仍有真实代码引用；本轮不得物理删除。
- Old Enemy Contract/serialized identity/Editor QA island: `QUARANTINE / MIGRATION_REQUIRED`。正式 Reward/Adapter/Resolver 已切走旧 Catalog 行，但 Session 兼容校验、旧 Editor verifier/simulation 及 1-3～1-5 Stage 资产的历史 identity 仍未归零。
- Serialized audit: Pool15 类型与旧 Pool ID 为 0；`CampaignStage_1-3.asset`～`CampaignStage_1-5.asset` 保留 3 个旧 stage3to5 compatibility identity 命中，禁止误报为零消费者。
- Verification: 用户持有的 Unity 在本轮四个代码文件写入后自行完成脚本编译与 Domain Reload，当前 Editor log 为 `0 error CS`；未控制 Unity、未运行 focused verifier、未做 Fresh Play。
- Ledger 状态：`HISTORICAL_CONSUMER_SAFE_CUTOVER_COMPLETE / TWO_LEGACY_ISLANDS_QUARANTINED / NO_BULK_DELETE / PLAYER_VISIBLE_DELIVERY_NONE / STOP`。

## Enemy Compatibility Identity Retirement — 2026-08-18

- `Chapter1CampaignStageConfig` + 五个 `CampaignStage` 资产：`KEEP / SINGLE_STAGE_LAUNCH_IDENTITY_OWNER`。1-1～1-5 已统一当前 balance、encounter 与 enemy-presentation 身份；1-3～1-5 的 stage3to5 playtest compatibility identity 已移除。
- `BattleLaunchContext` / StageConfig fingerprint：`DELETE / RETIRED`。当前 Stage 启动链不再生成、序列化、运输或比较 fingerprint/SHA，只校验具体启动字段。
- Formal Session、Reward、Adapter、Encounter Resolver 与当前 focused QA：`KEEP / CURRENT_ENEMY_TRUTH_ONLY`。正式路径只消费 `V04Chapter1StageWaveEncounterTruth + C1FormalEnemyDefinitionCatalog`。
- `C1Lv1EarlyEncounterBalanceVerifier` 与 `C1Stage3To5PlaytestEnemyShieldFactVerifier`：`DELETE / ZERO-CONSUMER_OLD_DATA_QA`。两者只有自有菜单入口，验证的是已被替代的旧 Enemy 数据表，已连同 `.meta` 删除。
- `C1Lv1EarlyEncounterBalanceContract/Validation` 与 `C1Stage3To5PlaytestEnemyEncounterContract/Validation/Simulation`：`QUARANTINE / ONE_MIXED_VERIFIER_CONSUMER_REMAINS`。它们已没有正式 Runtime 消费者；唯一外部代码消费者是同时承载旧 Pool15/兼容测试的 `C1FormalRealtimeBattleSessionVerifier`，转交下一 Pool15 Session/Contract 清理包，不在本包强拆。
- 验证：核心身份切换后的 Unity 脚本编译通过；当前启动链旧 fingerprint/SHA 扫描为 0，正式运行路径旧 Enemy Contract/Catalog 扫描为 0。未运行 Fresh Play。
- 状态：`ENEMY_CURRENT_IDENTITY_COMPLETE / TWO_OLD_QA_FILES_DELETED / LEGACY_DATA_FILES_WAIT_FOR_MIXED_VERIFIER_RETIREMENT / PLAYER_VISIBLE_DELIVERY_NONE / STOP`。
