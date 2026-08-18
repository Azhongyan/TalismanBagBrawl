# Project Cleanroom Audit and Retirement Task Contract

## Outcome

在不削弱 `CAMPAIGN_NORMAL_LV1` 玩家承诺的前提下，审计整个 Unity 工程，建立唯一组件账本；正式消费者完成接管后，删除零正式引用的重复真相、兼容层、历史运行路径和无价值工具。清理结果必须让后续新增 Item、关卡或表现只接入一个正式 Owner，不再复制 Sandbox 或历史数据链。

## Player Promise Protection

- WorldMap 正常进入 Unified 战斗，关卡、奖励、Tray、5x5 Board、实时战斗和返回流程不得因清理被替换为 Sandbox、回放或静态结果。
- Reward 产生的同一 `ItemGeneratedInstanceSnapshot` 必须继续贯穿 Authority、Board、Battle、cue 与 Presentation。
- 仅保存完成关卡 ID；不扩大存档范围。
- 保留现有 Unified pulse/ribbon/impact VFX 与共享 Card VFX。

## Task Class and Ownership

- Class: `COMPLEX_GUARDED_ONCE`
- Primary / Development Owner: 当前 Codex 任务窗口
- Unity serialization owner: 当前 Codex 任务窗口；仅限 `V02_FORMATION_OWNERSHIP_MIGRATION` 的一次干净 batch 生命周期
- User Decision Point: `NONE`（用户已批准全项目审计及删除符合条件的历史结构）

## Audit Scope

- `Assets/_Game/Scripts/TalismanBag/**`
- `Assets/_Game/Scenes/**`
- `Assets/_Game/Prefabs/**`
- `Assets/_Game/Resources/**`
- `Assets/_Game/ScriptableObjects/**`
- `Assets/_Game/Configs/**`
- `ProjectSettings/EditorBuildSettings.asset`（只读核对）
- 当前正式 Task Contract、工程状态和直接依赖文档

## Labels

- `KEEP`: 正式 Owner、正式路径或仍有明确生产/开发价值。
- `REPLACE`: 现有外壳保留，但其输入、数据源或实现必须原位切换。
- `ASSET-KEEP`: 只保留视觉/音频/Prefab 资产职责，不保留其中的旧数据真相。
- `QUARANTINE`: 不进入正式路径；暂留作参考或开发实验，后续独立判定。
- `DELETE-CANDIDATE`: 已知历史结构；只有满足删除门后才物理删除。
- `BLOCKED`: 已确认存在迁移或职责缺口，但不属于当前删除波次；必须作为独立原子任务处理，禁止在清扫中顺手补丁。
- `DELETE`: 删除门全部满足并已删除。

## Deletion Gate

一个对象只有同时满足以下条件才允许删除：

1. 正式 Runtime 无直接消费者；
2. Enabled Build Scene、正式 Prefab、Resources/ScriptableObject 配置无序列化依赖；
3. 已有正式 Owner 接管其必要职责；
4. 相关 focused compile/static verification 不依赖它；
5. 删除不会移除用户已确认保留的画面、VFX、关卡、奖励、存档或调试入口。

不计算或比较哈希/SHA。使用路径、类型/符号引用、序列化引用、编译和正常路径证据。

## Protected Boundaries

- 当前 Editor Build Settings 中启用的 6 个 Scene；是否退役必须另有 Scene 责任岛证据。
- `CanonicalItemDefinitionResolver`、`CanonicalItemInstanceFactory`、`CanonicalItemDropPolicy`、`CanonicalInitialItemAcquisitionPolicy`。
- `C1FormalItemSessionAuthority`、现有 5x5 Board/Tray/Card、`C1FormalRealtimeBattleSession`。
- Unified Battle 正式 Scene、共享 Battle/Item Prefab、现有图片与两套 VFX。
- Enemy、StageConfig、save schema 和 completion-only persistence。
- 用户拥有的 Unity 生命周期。

## Retirement Waves

1. Item truth: 原位切换 Reward、Authority、Battle、Detail/Presentation；零引用后删除 Pool15/Starter 私有结构及专用旧测试。
2. Duplicate presentation carriers: 核对 Exact/Formal Item Prefab 与旧 UI Prefab 的真实挂载关系，保留一个共享职责，不手改 Scene YAML。
3. Resources: 核对 `item`、`item_daoju`、旧动画和历史视觉目录的正式加载键与序列化引用，再决定保留或删除。
4. Sandbox/history: 审计 BattleSandbox、ItemSandbox、CoreLoop Lab、V02/V03 运行代码与未启用 Scene；保留明确实验室，清退被正式系统替代的运行图。
5. Tooling: 删除或重写依赖旧真相、写重复报告、使用过时门禁的 Verifier/ReportWriter；建立最小 compile/test/scene-check 入口。
6. Facts snapshot: 更新工程状态文档，使 Build Settings、正式 Scene、测试边界和已退役结构与磁盘一致。

## Stop Conditions

- 仍有正式或序列化消费者时不得删除。
- 无法确认 Scene/Prefab/Resources 引用时只标记，不猜测。
- 清理暴露产品语义缺口时停止该责任岛，命名缺口；不建立兼容层掩盖。
- 不因清理顺手重做玩法、视觉、数值或存档。

## Completion

- 全工程组件账本覆盖 Runtime、Editor、Scene、Prefab、Resources、ScriptableObject、Config。
- 正式路径只保留一个 Item、Reward、Authority、Battle 和 Presentation 真相。
- Pool15/固定 Starter 私有正式结构及专用旧测试零引用并删除。
- Sandbox/历史内容均有明确 KEEP/QUARANTINE/DELETE 结论。
- 工程事实快照与磁盘一致。
- 定向编译/静态检查通过；Unity Fresh Play 由外部 Unity owner 执行并如实记录。

## Cleanroom Return Wave — 2026-08-11

- 用户终止围绕 1-3 / I004 的局部诊断和修补，重新授权本合同继续按责任岛清扫。
- 本波只允许回收任务自有临时物、刷新真实消费者图、更新标签，并删除满足全部删除门的完整责任岛。
- 不在本波修改 Battle、Reward、Authority、Scene/Prefab、Presentation、存档或产品数值语义。
- `UnifiedBattlePageShell.prefab` 缺少 Host 所需 `canonicalItemCatalogSource` 序列化引用，标记为 `BLOCKED / SERIALIZATION_MIGRATION_GAP`；该问题必须在后续独立 Unity 原子迁移中处理，不与删除工作混做。
- 临时 `CANONICAL_FORMAL_HOST_FEEDER_TRACE` 责任岛已整体退休；Runtime / Editor 离线编译均为 0 错误。
- 下一审计目标按最新消费者图选择，不重复已完成的 BuildSandbox 报告岛与 Canonical Effect Operator 工作。

## Current Tooling Boundary — 2026-08-11

- 旧完整性工具只允许两种原位处理：删除没有产品价值的固定文件门禁；或把同次操作的不变性证明改为原始文本/字节/结构比较。
- 不允许在本波改写 Runtime identity、save/reward/battle contracts，也不允许把 Prefab/Scene 的已知历史状态摘要直接替换成新的隐式分类逻辑。
- 剩余具有 Menu 入口的旧 Item/Unified authoring 标记为 `REPLACE / STRUCTURAL_VALIDATION_MIGRATION`；需按单一完整 authoring 责任岛迁移，不能拆 helper 补丁。
- 无 Menu 的 verifier/survey 先按当前消费者图贴 `KEEP / QUARANTINE / DELETE-CANDIDATE`；只有满足本合同 Deletion Gate 才连同 `.meta`、专属报告和入口整岛退休。
- 当前清扫范围按具体路径、符号、入口、消费者、序列化引用和责任岛维护；不保存或汇报完整性算法命中数量。

## Legacy Aggregate Verifier Retirement Boundary — 2026-08-11

- 已退休 4 个通过完整删除门的旧聚合验证岛及 27 份专属历史报告；Runtime、Scene/Prefab、Enemy、Stage、Battle、Reward、Authority、Presentation 与 Save 均未进入本次写域。
- 报告之间构成旧依赖图时按整图审计，不保留断裂消费者，也不新增兼容报告。
- 仍覆盖当前运行时职责的 verifier 不因命中旧完整性基础设施而裸删；`V04ChapterFlowContractVerifier` 当前明确保留验证职责，并标记 `KEEP + REPLACE_LEGACY_INTEGRITY_INFRA`。
- `BoneAspect/VocabularyExtension` 与 `BoneAspect/CarrierPresentation` 已通过零代码消费者、零序列化 GUID 引用、零当前入口与非玩家资产边界证明，连同专属 Verifier/报告整岛退休；正式 Enemy 与 Presentation 不在该删除范围。

## CrossSystem ItemEnemy / LayoutResilience Boundary — 2026-08-11

- `CrossSystem/ItemEnemy` 没有 Formal Battle、Reward、Authority、WorldMap 或 Enabled Build Scene 的直接运行时消费者，脚本 GUID 也没有 Scene/Prefab/ScriptableObject 序列化挂载。
- 该目录仍被 `BuildSandbox` 的 Board Authority、Board Adapter、LayoutResilience playtest adapter/feedback 以及 I031、BuildSandbox、Enemy Requirement 三条旧验证链消费，因此不是零消费者责任岛。
- Runtime 与 `Editor/CrossSystem/ItemEnemy` 统一标记为 `QUARANTINE / RETIRE_AFTER_FORMAL_REPLACEMENT`；禁止拆文件删除，也禁止让 Formal Runtime 重新依赖这条 Sandbox 调查链。
- `FormationEnergyContractValidator` 与 `LegacyItemBehaviorIntegrationValidator` 虽无独立菜单或现行调用入口，但其报告仍与现存 BuildSandbox 供能/行为职责交叉引用。本波不物理删除，随 BuildSandbox 整体迁移后再复审。
- 本边界只更新消费者图与标签；没有修改玩法 Runtime、Scene/Prefab、Battle、Reward、Authority、Presentation 或 Save。

## BuildSandbox Tooling And V02 Builder Boundary — 2026-08-11

- BuildSandbox 剩余 ReportWriter 均仍由现行 Validator、`BuildSandboxGuardRunner` 或开发窗口入口消费；本轮工具清扫到此闭账，禁止继续按文件名逐个裸删。
- `FormationEnergyContractValidator` 与 `LegacyItemBehaviorIntegrationValidator` 继续归入 `QUARANTINE / RETIRE_AFTER_FORMAL_REPLACEMENT`，等待其供能与旧行为验证职责被正式 focused validation 接管。
- `TalismanBagSceneBuilder` 无当前 Menu 或代码调用，也无序列化挂载，但仍包含启用中的 `Scene_TalismanBag_V02_FormationCounter` 维护入口和旧 Run/Shop/Prefab 生成职责；标记为 `QUARANTINE / RETIRE_WITH_V02_SCENE_MIGRATION`，不得先删 Builder 再留下无人维护的启用 Scene。
- 该边界不恢复已不存在的 `Scene_TalismanBag_Run15Min`，也不运行 Builder 或 Unity。

## Data Asset Responsibility Boundary — 2026-08-11

- `Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset` 与其 Profiles 是唯一 Canonical Item 数据库：Formal Battle focused verifier、Reward/Unified authoring、Item resolver 与现存 Sandbox adapter 均直接读取该 Catalog；标签为 `KEEP / CANONICAL_ITEM_DATABASE`。
- `Configs/ItemDetail/ItemDetailVisualTheme.asset` 被正式 Exact Item Detail Popup Prefab 序列化消费，同时也供三个实验 Scene 使用；标签为 `KEEP / PRESENTATION_ASSET`。Item Detail 产品工作仍冻结，不因清扫改动其内容。
- `Configs/BuildSandbox/**` 没有 Formal Scene/Prefab 序列化消费者；当前由 BuildSandbox 的类型扫描 Validator、Synergy/Shape/Affix 评估链消费。整体标签为 `QUARANTINE / LAB_DATA / RETIRE_AFTER_FORMAL_REPLACEMENT`，不得流入正式 Item 真相。
- `ScriptableObjects/TalismanBag/V02/**` 仍被启用的 V02 Formation Scene、V02 资产链与 `Resources/CoreLoop/BossConfigs` 消费；整体标签为 `QUARANTINE / RETIRE_WITH_V02_SCENE_MIGRATION`。
- `ScriptableObjects/TalismanBag` 根级旧 Items、Enemies、RunConfigs 与 ShopPools 只形成旧 Builder/旧 Prefab 内部资产链；整体标签为 `QUARANTINE / RETIRE_WITH_LEGACY_BUILDER`。在 Builder 与 V02 场景职责拆分前不做单资产删除。
- `Resources/anim/Enemy/d1/d1_2` 已通过无加载键、无外部序列化引用、无正式 Presentation 消费者的删除门，整目录与 `.meta` 退休。
- `Resources/anim/video_6a4e13ef8ea9ccfeebcfd8ee_1783501848145` 是空目录且目录 GUID 无消费者，已与 `.meta` 一并删除。
- `Resources/anim_MP4` 中两份源视频没有代码加载键或序列化消费者，已移出 Unity `Assets/Resources` 到 `Archives/SourceMedia/anim_MP4`；源媒体保留，旧 Unity `.meta` 与空目录删除。状态为 `RELOCATED / SOURCE_MEDIA_OUTSIDE_RESOURCES`。
- 已清除 `Assets/_Game` 下经复核为空且目录 GUID 无消费者的空目录壳；仅删除目录和 `.meta`，未删除任何脚本、Prefab、Scene、Config 或玩家资源。

## Prefab And Dead-Code Boundary — 2026-08-11

- 当前所有剩余 Prefab 均有序列化消费者，或仍被明确的 Builder/authoring 路径消费；本轮没有新的 Prefab 满足完整删除门。
- 零外部序列化引用但仍有工具消费者的 Prefab 包括根级 V02 拖拽/格位载体、旧 Formal Board/Tray 与 BattleLike Preview Bridge；统一保持 `QUARANTINE / RETIRE_WITH_OWNER_MIGRATION`，不得拆成孤立删除。
- `ItemDetailSectionView.cs` 尾部被 `#if false` 永久禁用的旧 Editor 自动预览块，已有显式手动 authoring 替代且不参与编译或 Runtime；该死代码已原位删除，没有改变 Item Detail 玩家行为。
- 清扫后使用现有 Bee 引用与当前源码重新执行 Runtime / Editor 离线全编译，结果均为 0 错误；保留 2 条既有未使用字段警告。未启动 Unity，未执行 Fresh Play。

## Remaining Resource-Island Boundary — 2026-08-11

- `Resources/V04/beibao` 由现行 Exact Board、Tray、Prepare Surface Prefab 与三个 V04 实验/制作 Scene 序列化消费，标签为 `KEEP / SHARED_ITEM_UI_ART`。
- `Resources/anim/guajian` 由现行 Prepare Surface Prefab 与三个 V04 Scene 序列化消费，标签为 `KEEP / SHARED_PREPARE_PRESENTATION`。
- `Resources/anim/照煞镜_VFX_RGBA_9帧` 已被正式 Battle Presentation Root、Enemy Slot、Cue FX Prefab 与 Formal Presentation Profile 消费，标签为 `KEEP / FORMAL_BATTLE_PRESENTATION_ASSET`，不得按 Sandbox 素材删除。
- `Resources/V04/ItemLivingGradientOutlineDevOnly` 与 `Resources/V04/LiHuoCombatFeedbackDevOnly` 没有正式序列化挂载，但仍由 BattleSandbox 运行时自动安装代码和对应验证器按 Resources 路径读取；统一标记 `QUARANTINE / PRESENTATION_SUPPLIER_ASSET / RETIRE_AFTER_FORMAL_CUTOVER`。
- `Resources/anim/video_6a56065e5485ff99517e29da_1784022639683` 仍由 BattleSandbox、CoreLoop Lab Scene 与 `BuildSandboxCharacterIdlePreview` 消费，标记 `QUARANTINE / LAB_PRESENTATION_ASSET`。
- 已将无代码加载键、无外部序列化引用的 `Resources/V03/Battle/chenbaichaun.png` 移到 `Archives/SourceMedia/UnreferencedV03Battle`，删除旧 Unity `.meta`，状态为 `RELOCATED / UNREFERENCED_SOURCE_ART_OUTSIDE_RESOURCES`。
- 本轮没有发现新的可安全删除正式 Prefab、正式 VFX 或玩家 Item 资源；剩余对象均已进入 KEEP、QUARANTINE、RELOCATED 或 BLOCKED 四类，不以“文件名像旧资源”为删除依据。

## Scene Responsibility Boundary — 2026-08-11

- Build Settings 中的 BootEntry、MainHome、WorldMap、TalismanUpgrade 与 UnifiedBattlePageShell 共同构成当前玩家入口和正式路线，标签为 `KEEP / ACTIVE_BUILD_ROUTE`。
- `Scene_TalismanBag_V02_FormationCounter` 仍启用，且被 Navigation Owner、Battle snapshot 默认来源、Chapter Flow verifier 与旧 Builder 消费；标签为 `KEEP INSIDE QUARANTINE / ENABLED_LEGACY_SCENE / RETIRE_WITH_NAVIGATION_MIGRATION`。不得先删 Scene 或 Builder 的任一侧。
- `Scene_TalismanBag_V04_BattleSandboxPreview` 未进入 Build Settings，但仍是 Battle/Enemy/VFX 原型、正式 Prefab authoring 和多项现行 verifier 的来源；标签为 `QUARANTINE / AUTHORING_SOURCE_AND_LAB`。
- `Scene_TalismanBag_V04_ItemSandbox` 未进入 Build Settings，但仍被 Item Detail authoring、Board/Skill/Lighting verifier 与独立 ItemSandbox build menu 消费；标签为 `QUARANTINE / ITEM_TOOLING_LAB`。
- `Scene_TalismanBag_V04_CoreLoopABCProductDirectionLab` 未进入 Build Settings，但仍由 CoreLoop Lab Controller 与 Scene authoring 消费；标签为 `QUARANTINE / PRODUCT_LAB`。
- 当前 9 个 Scene 均有现实职责或消费者，本轮没有 Scene 满足完整删除门；Formal Runtime 不得新增对三个 Lab Scene 的依赖。

## V02 Formation Ownership Migration Audit — 2026-08-11

- `Scene_TalismanBag_V02_FormationCounter` 不是单纯阵型页面，而是一套完整的旧运行图：`V02RunFlowController`、`AutoCombatController`、旧 Inventory/Grid/Drag、BattlePrepare、Reward、Boss、Result、Enemy Intent、MainTrial 章节状态与旧反馈系统均在同一 Scene 内。
- 现行 MainHome 的“试炼”已直接进入 `WorldMap`；但新玩家在 `V03BootEntryFlowController.SkipOpeningStory()` 后仍通过 `TalismanSceneRoute.Trial` 进入 V02 Scene。升级页底栏的试炼入口也仍通过同一路由进入 V02。
- `MainTrialFlowService` 的脚本序列化实例只存在于 `Scene_TalismanBag_V03_TalismanUpgrade`；升级页及 `V03ForgeFirstUpgradeGuideController` 仍读取旧 `MainTrialPhase`，并写入旧首次升级/第二章状态。该服务不是当前 V04 stage-clear-only Campaign 的权威进度 Owner。
- `V02RunFlowController` 的脚本序列化实例只存在于 V02 Scene；`V03BattlePrepareInteractionController` 及 `AutoCombatController` 对它的运行时依赖属于该旧 Scene 内部运行图，不是当前 WorldMap → Unified 正式链的跨 Scene Owner。
- `V03BattleStartRequestExporter` 当前没有正式运行调用者；其 V02 Scene 默认来源、`V03MainTrial` entry source 与 `MainTrialFlowService` source controller 仅保留在旧 Adapter 定义中。`V03BattleLayoutSnapshotExporter` 当前仅被静态 Battle Contract 验证样例调用。两者标记为 `QUARANTINE / RETIRE_WITH_V02_CONTRACT_MIGRATION`，不得伪装成现行正式 feeder。
- `V04ChapterFlowContractVerifier`、`UnifiedBattlePageShellVerifier` 和若干 BuildSandbox verifier 对 V02 名称的引用属于旧保护/排除/报告职责，不等于玩家 Runtime 消费；Scene 退休时应迁移这些验证责任，禁止用修改字符串掩盖旧运行图仍存在。
- `TalismanBagSceneBuilder` 仍可重建 V02 Scene、旧 RunConfig、Item/Enemy/Shop/Reward 资产，继续保持 `QUARANTINE / RETIRE_WITH_V02_SCENE_MIGRATION`。它不得先于 Scene 退役，也不得重新运行以恢复已淘汰结构。

### Atomic migration order

1. 将 Boot 新玩家跳过剧情与 Upgrade 底栏“试炼”统一切到现有 `WorldMap` 正式入口；剧情/新手初始道具规则留给独立 Onboarding 责任，不再借 V02 Run 或普通掉落数据库承载。
2. 从 Upgrade Scene 和升级页代码中移除 `MainTrialFlowService`、旧 `MainTrialPhase` 门控及旧章节状态写入；保留 Upgrade 自身仍有效的升级职责，不迁移旧章节存档。
3. 在所有运行调用者归零后移除 `TalismanSceneRoute.Trial`、V02 Scene 路径常量和 V02 专属 Battle snapshot Adapter 默认身份。
4. 由单一 Unity 序列化 Owner 同批移除 V02 Scene 的 Build Settings 条目并退休 Scene；禁止手改 Scene YAML，禁止拆成兼容补丁。
5. Scene 退休后重新计算消费者图，只删除已经归零的 V02 Formation 专属 Runtime、RunConfig、Prefab、Builder、Editor verifier 与报告；共享 Save、Upgrade、Item/Enemy 或正式 Battle 类型按实际消费者保留。
6. 最后执行结构检查、Runtime/Editor 编译和 Boot → WorldMap → Unified 正常路径 Fresh Play；玩家正式路线通过前不宣称清扫完成。

### Current stop line

- 本轮只读审计已完成，未修改 Boot、Upgrade、Navigation、Scene、Build Settings、Save 或玩家 Runtime。
- 当前唯一阻塞是 `UNITY_SERIALIZATION_MIGRATION_REQUIRED`：Upgrade Scene 需要移除旧服务挂载，Build Settings 需要移除 V02 Scene；必须与代码切换在同一个原子迁移生命周期完成。
- 在该序列化迁移开始前，V02 Scene、`MainTrialFlowService`、`V02RunFlowController`、旧 Builder 和相关资产全部保持隔离，不做提前物理删除。

## V02 Formation Atomic Migration Authorization — 2026-08-11

- 用户已明确要求继续，授权当前窗口执行上一节已经冻结的原子迁移；不创建或下发其他任务。
- 唯一玩家结果：新玩家、主页与升级页的试炼入口均进入现有 WorldMap；V02 Formation 不再进入 Build Settings 或正式玩家路线。
- Runtime 写域：
  - `Assets/_Game/Scripts/TalismanBag/V03/BootEntry/V03BootEntryFlowController.cs`
  - `Assets/_Game/Scripts/TalismanBag/V03/Navigation/**`
  - `Assets/_Game/Scripts/TalismanBag/V03/MainHome/V03MainHomeSceneBootstrap.cs`
  - `Assets/_Game/Scripts/TalismanBag/V03/Forge/**`
  - `Assets/_Game/Scripts/TalismanBag/V02/UI/MainHomeGreyboxPanel.cs`
- Editor/serialization 写域：
  - 对应 V03 Boot/Navigation/Upgrade builder 与 smoke
  - `Assets/_Game/Scenes/Scene_TalismanBag_V03_TalismanUpgrade.unity`
  - `Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity`
  - `ProjectSettings/EditorBuildSettings.asset`
  - 仅用于本次迁移、完成后删除的临时 Editor migration 入口
- 后续删除写域：只有在上述切换和序列化保存完成后，经重新扫描确认为零 Runtime、零序列化、零正式验证消费者的 V02 Formation 专属脚本、配置、Prefab、Builder、Verifier、报告及配套 `.meta`。
- 保护边界：WorldMap、Unified、Canonical Item、Reward、Authority、正式 Battle、Enemy、StageConfig、VFX、现有 Upgrade 能力与 completion-only 存档均不改；旧 `mainTrialProgressData` 先保留为惰性历史字段，不在本迁移中改写 Save schema。
- 禁止新建第二 Route、第二 Campaign、第二 Save、兼容 Adapter 或新 Scene；禁止手改 Scene YAML。Unity 只允许一次当前任务拥有的 batch 序列化生命周期，并记录启动、完成和退出。

## V02 Historical Island Physical Retirement Authorization — 2026-08-11

- 用户已在阅读技术报告第 13 篇及其 Section 18 风险说明后明确批准物理删除 V02 历史岛；当前窗口继续作为唯一 Development Owner，不创建或下发其他任务。
- 本包只退休已经退出正式 Build Route 的旧 `TalismanBagSceneBuilder`、两个旧 V02 拖拽/格位 Prefab、根级与 V02 历史 ScriptableObject 数据岛、旧 CoreLoop Boss/Drop/Reward/Idle/Tutorial Resources，以及与这些资产一一对应的无独立职责路径/验证残留。
- `CoreLoopTalismanUpgradeConfig.asset`、`ResourceService`、`UpgradeService`、`SaveService`、Upgrade 数据类型、MainHome UI、Canonical Item、Reward、Authority、正式 Battle、Enemy、StageConfig、Presentation、五个正式 Scene 与 completion-only save 全部保持不变。
- `BattlePrepareComponentAdapterRuntimePlaytest` 中仍被 ShapeAware validator 复用的 shape helper，以及仍依赖它的 `V03BattlePrepareInteractionController` / `AutoCombatController` 连接岛，本包自动退出物理删除范围，标签为 `KEEP UNTIL RESPONSIBILITY CUTOVER`；禁止为删除它们新建 Adapter、Bridge、Fallback 或第二套验证框架。
- `V04ChapterFlowContractVerifier` 只允许原位移除旧 V02 路径及完整性算法/基线/白名单门禁，保留 Manifest、Reducer、Projection 与 source-isolation 验证职责。
- 删除后只做结构/引用检查与离线 Runtime/Editor 编译；本包不增加第二次 Unity 生命周期，不执行 Fresh Play，不修 1-3、Item Detail、VFX、Presentation 或 Product Semantic。
- 完成后更新 Component Ledger 与 Notion 13，然后立即 `STOP`。

## V02 Historical Island Physical Retirement Completion — 2026-08-11

- 用户再次明确授权删除整个 `Assets/_Game/ScriptableObjects/TalismanBag`，包括根级旧 Enemy、Item、ShopPool、RunConfig 与 `V02` 子目录，并明确授权删除配套旧 CoreLoop Resources、两个旧 Prefab 与旧 Editor 工具。
- 已物理删除 307 个资产/源码/`.meta` 文件，并清除 19 个随之变空的目录：完整旧 ScriptableObject 数据岛、旧 CoreLoop Boss/Drop/Reward/Idle/Tutorial 资源、`DraggableTalismanItem.prefab`、`TalismanGridSlot.prefab`、`TalismanBagSceneBuilder`、V02 Config Editor 与 V02 Balance Editor。
- `V04ChapterFlowContractVerifier` 已原位移除 SHA、哈希、baseline、whitelist 与旧 V02 FormalPaths 基础设施；Manifest、Reducer、Projection 与 source-isolation 验证职责保留。
- 根级 `TalismanItemBalancePanel` 与 `TalismanEnemyBalancePanel` 没有删除；它们已脱离被删除的 V02 Editor UI，改用 Unity 原生 `EditorGUILayout` 并可独立打开。统一菜单只移除了已经不存在的 Stage Config Panel 与旧 Data Catalog Validator 入口。
- 正式 Build Settings 仍只有 BootEntry、MainHome、WorldMap、TalismanUpgrade、UnifiedBattlePageShell 五个 Scene。`CoreLoopTalismanUpgradeConfig.asset`、Canonical Item Catalog、Reward、Authority、Battle、Enemy、StageConfig、Presentation、Save 与共享 Shape helper 均未删除。
- 删除后离线全量编译：Runtime `0` 错误（2 条既有未使用字段警告），Editor `0` 错误；没有启动 Unity，没有执行 Fresh Play。
- 剩余旧 CoreLoop Resource 加载字符串仅存在于零 Scene/Prefab 序列化消费者的 V02 Runtime 隔离岛，标签更新为 `QUARANTINE / RETIRE_AFTER_RESPONSIBILITY_CUTOVER`；本包不为清除字符串新建兼容层，也不扩大到 BattlePrepare/AutoCombat 共享职责岛。
- 终态：`V02_HISTORICAL_DATA_RESOURCE_TOOL_ISLAND_RETIRED / INTERNAL_COMPILE_PASS / UNITY_NOT_RUN / PLAYER_PATH_NOT_READY / STOP`。

## Editor Evidence Tool Retirement Wave — 2026-08-11

- 用户授权在 Notion 13 末尾记录后继续下一阶段清理；当前窗口继续作为唯一 Owner，不创建其他任务窗口。
- 已按 Deletion Gate 连续退休 8 个无当前可执行/序列化消费者的旧 Editor Verifier、29 份独占历史报告与 1 份旧 evidence 日志；其中两个隐藏 Editor 自动加载生命周期同时被移除。
- `DefaultRealLayoutResilienceEvaluationPipeline`、它的 Runtime primitives/validation、Board Adapter、LayoutResilience Playtest Adapter 与 `ItemEnemyMatchupMatrix.csv` 明确保留；没有把旧证据生成器与正式 Runtime Owner 一起删除。
- 删除后静态复核为：已删路径 0 残留；当前代码/脚本/序列化引用 0；Hygiene Gate 继续返回 `WARNING`，不得宣称工程已 Clean。
- 下一目标为 Enemy Validation Normalize 连接岛。`EnemyValidationContentNormalizeVerifier` 虽无独立入口，但仍被 5 个旧 Enemy Verifier 的文件保护表引用；其中 3 个上游 Verifier 又被两个更高层 Enemy/LayoutResilience Contract Verifier 的 source-boundary 证明读取。该岛已确认超过最初 6 个文件，必须自上而下区分现行结构验证与历史文件保护/报告职责，不得单文件裸删，也不得通过更新旧保护值续命。
- 本波继续保护 Enemy 产品语义、Runtime Normalizer、正式 Battle/Reward/Authority、Scene/Prefab、Presentation、Save 与玩家内容；Unity 与 Fresh Play 均不在本次 Editor 清理生命周期内。

## Enemy Validation / CrossSystem Evidence Island Retirement Completion — 2026-08-11

- 当前窗口按已批准的 Cleanroom 路线直接完成整岛消费者图审计，没有创建、读取或下发其他任务窗口。
- 已物理退休 25 个 Editor source/`.meta` 与 130 份专属历史报告。删除对象没有菜单入口、自动加载、当前脚本、Runtime 调用或真实 Scene/Prefab 序列化消费者；所有已删类型的当前代码、脚本与序列化引用为 0，受影响目录孤立 `.cs.meta` 为 0。
- `BoneSwapRemnantRuntimeOperatorVerifier`、`C1Lv1EarlyEncounterBalanceVerifier`、`C1ShatteredHostPorcelainHoundRuntimeVerifier`、Runtime Enemy Normalization/SystemSnapshot/SeedData、LayoutResilience Runtime、BuildSandbox adapters 与仍有真实消费者的 5 份输入/绑定报告全部保留。
- 当前源码核对发现并原位补齐 `C1UnifiedSingleFormalPathAuthoring` 对既有 `TalismanBag.BuildSandbox` 命名空间的缺失引用；这只是 Editor 编译引用修正，不改变 Runtime 或玩家逻辑。
- 文件系统枚举下的 Runtime 580 个 C# 与 Editor 181 个 C# 均离线编译通过，仅有两条既有未使用字段警告；没有启动或控制 Unity，没有执行 Fresh Play。
- Hygiene Gate 中旧完整性门禁实现由全局 87 降至 65、Editor 36 降至 14，仍返回 `WARNING`。终态为 `ENEMY_VALIDATION_CROSSSYSTEM_EVIDENCE_ISLAND_RETIRED / INTERNAL_COMPILE_PASS / UNITY_NOT_RUN / PLAYER_PATH_NOT_READY`。
- 下一独立目标固定为仍在运行的 Formal Battle/Enemy Verifier 基础设施。后续必须先画清菜单/自动入口、上游调用、Runtime/序列化消费者与独占报告，再决定 `KEEP / REPLACE / RETIRE`；不得把本轮完成扩展成 Runtime Battle 修复或玩家完成声明。

## Formal Battle Verifier Legacy Infrastructure Closure — 2026-08-11

- `C1FormalRealtimeBattleSessionVerifier` 与 `C1FormalBattleDiscreteProgressOperatorsVerifier` 继续作为现有正式 Battle 行为验证入口；真实 Session、adapter、action queue、mutation、拒绝原子性和阶段回归检查均保留。
- 已从两文件原位移除 Assignment/保护文件旧完整性基础设施、静态包白名单、重复报告生成与报告自证；没有新建测试框架、Bridge、Adapter 或 Runtime Owner。
- 8 份仅由这两个 Verifier 拥有且活动代码/脚本消费者为 0 的历史报告已物理退休；历史 Assignment 本身保留为历史记录，不再是执行门。
- Runtime 580 个 C# 与 Editor 181 个 C# 离线编译均 0 错误，仅有两条既有未使用字段警告。Hygiene Gate 由全局 65 降至 63、Editor 14 降至 12，继续返回 `WARNING`。
- 本子岛未修改 Runtime Battle、Reward、Authority、Enemy、Stage、Scene/Prefab、Presentation、Save 或玩家语义；Unity 与 Fresh Play 均未运行。
- 下一子岛只审计并原位收口三个仍有行为价值的 Enemy Verifier；先保留行为证明，再移除旧报告和文件保护职责。

## Enemy Behavior Verifier Legacy Infrastructure Closure — 2026-08-11

- 当前窗口继续作为唯一 Owner，未创建、读取或下发其他任务窗口。
- 已将 `BoneSwapRemnantRuntimeOperatorVerifier`、`C1Lv1EarlyEncounterBalanceVerifier`、`C1ShatteredHostPorcelainHoundRuntimeVerifier` 收口为 `KEEP / BEHAVIOR_ONLY_VALIDATION`：保留 operator、encounter、调度、damage、reset、negative、determinism 与 boundary 断言。
- 已删除旧任务绑定、保护文件表、完整性比较、写域白名单、报告写入/再生成/格式自证，并退休 24 份活动消费者为 0 的专属历史报告；没有删除非本岛所有的 Starter baseline 或 Item Combat Effect Request 资料。
- 未修改 Runtime Enemy、Battle、Reward、Authority、Stage、Scene/Prefab、Presentation、Save 或玩家数值；未新增第二套验证框架、Bridge、Adapter 或兼容层。
- 离线全量编译结果：Runtime 580 / Editor 181，均 0 错误，仅两条既有未使用字段警告。Hygiene Gate：全局 60、Editor 9，`RoomStatus: WARNING`。
- Unity/Fresh Play：`NOT RUN`。终态：`ENEMY_BEHAVIOR_VERIFIER_INFRASTRUCTURE_CLOSED / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY`。

## VFX Lab Verifier Legacy Integrity Closure — 2026-08-11

- 当前窗口继续作为唯一 Owner；未创建、读取或下发其他任务窗口，也未启动 Unity。
- `ItemLivingGradientOutlineVfxPrototypeVerifier` 与 `LiHuoCombatFeedbackOrchestrationVerifier` 已原位收口为 Sandbox/VFX 开发实验的行为验证器：保留静态行为验证、Rendered QA、Android 资产包含验证和现有 Runtime／资产消费者。
- 固定文件完整性表及跨生命周期文件指纹门禁已删除。场景未改写由 Unity Scene dirty 状态直接验证；Android 构建临时修改的恢复由构建前后 Preloaded Assets 语义序列直接验证。
- 未修改 Scene／Prefab／VFX 资产、正式 Presentation、Reward、Item Authority、Battle、Enemy、Stage、Save 或产品语义；未建立第二套测试框架、Bridge、Adapter 或兼容层。
- 离线全量编译 0 错误；Hygiene Gate 为全局 58、Editor 7，`RoomStatus: WARNING`。Unity／Rendered QA／Android Build QA／Fresh Play 均 `NOT RUN`。
- 本子岛终态：`VFX_LAB_VERIFIER_LEGACY_INTEGRITY_CLOSED / INTERNAL_COMPILE_PASS / PLAYER_PATH_NOT_READY`。下一步只审计剩余 7 个 Editor 文件的真实 authoring／contract 职责，不批量改写活动序列化工具。

## ItemInstance Placement Binding Verifier Infrastructure Closure — 2026-08-11

- 当前窗口直接完成消费者图与职责审计；该 Verifier 没有外部活动代码消费者，但其三方绑定、异常输入、I031 排除、不可变快照与确定性断言仍是当前 Runtime Contract 的有效 focused validation，因此没有删除整个文件。
- 文件已原位收口为 `KEEP / BEHAVIOR_ONLY_VALIDATION`：保留行为断言和现有离线／batch／纯 C# 入口，删除固定文件保护、包清单、源码泄漏扫描、报告生成、自证和配套数据容器。
- `ItemInstancePlacementBindingContractReport.md`、`ItemInstancePlacementBindingContractSpec.csv`、`ItemInstancePlacementBindingContractLeakCheckReport.md` 已退休；活动引用为 0。
- 离线全量编译：0 错误／0 警告。Hygiene Gate：全局 57、Editor 6，`RoomStatus: WARNING`。Unity／Fresh Play：`NOT RUN`。
- 未修改 Runtime Contract／Validator、Reward、Item Authority、Board、Battle、Scene／Prefab、Presentation、Save 或玩家语义。下一步继续按真实消费者图审计剩余 6 个 Editor 文件，优先避免活动 authoring 的批量改写。

## Shougunu Phase1 BattleSandbox Verifier Infrastructure Closure — 2026-08-11

- 当前窗口按责任岛完成 `ShougunuPhase1BattleSandboxVerticalSliceVerifier` 收口，没有创建或读取其他任务窗口，也没有启动 Unity。
- 保留离线 Battle 行为断言、Nian 联动、reset/stale、Scene binding、UI/VFX 载体和正常 Editor Play Smoke；删除无活动消费者的一次性 Scene 安装入口、固定文件/源码门禁、报告写入链及 7 份专属历史报告。
- BattleSandbox Scene 中三个运行组件的序列化引用已直接确认存在；本轮没有保存 Scene、Prefab 或任何产品资产。I031 verifier 依赖的真实投影方法与断言保持不变。
- 全量离线编译 0 错误；Hygiene Gate 为全局 55、Editor 5，继续 `WARNING`。未运行 Unity、Play Smoke 或 Fresh Play，不将本轮清扫称为玩家进展。
- 下一步只审计 `BattleSandboxTrayStableLayoutAndManualSortVerifier` 的真实 Board/Tray 行为与旧证据基础设施；4 个活动 authoring/serialization 工具保持独立责任岛，禁止批量改写。

## BattleSandbox Tray Stable Layout Verifier Infrastructure Closure — 2026-08-11

- 当前窗口保留 Tray verifier 的真实 fixture、31 件装盘、Arrange Scene contract 与真实指针生命周期；现有 Tray authoring 的直接调用未改变。
- 已移除源码关键词检查、文件摘要、报告写入及 7 份专属历史报告。真实指针入口由“自动清除 Scene dirty”改为“发现未保存 Scene 即停止”，不再隐藏用户修改。
- 未运行 Unity、authoring、Scene save、真实指针 QA 或 Fresh Play。全量离线编译 0 错误；Hygiene Gate 为全局 54、Editor 4，继续 `WARNING`。
- 低风险 verifier 收口波次结束。剩余 4 个命中对象全部是活动 authoring/serialization 工具，下一步仅允许先读消费者图与写入职责，再逐岛移除旧完整性基础设施；禁止整批删除或执行 Unity 序列化。

## Exact Item Detail Standalone Prefab Authoring Infrastructure Closure — 2026-08-11

- `C1ExactBattleSandboxItemDetailPopupStandalonePrefabAuthoring` 继续作为现有详情 Popup Prefab 的生产性 Authoring Owner；保留从 BattleSandbox 现有画面提取载体、清除样本投影与外部引用、去 Sandbox Runtime Owner、保存目标 Prefab，以及层级／组件／引用／几何的直接逐项比对。
- 已移除冻结 Source Scene 文件签名、外部保护文件字节比较、旧 Markdown 报告写入与自动打开 PrefabStage 的展示性检查；结构验证改为可读结构序列直接比较，不建立第二套 Authoring 或验证框架。
- `C1ExactBattleSandboxItemDetailPopupStandalonePrefabParity.md` 暂未删除：下游 `C1UnifiedExactBattleSandboxItemDetailPopupMountAuthoring` 仍将其列为输入。该报告标记为 `RETIRE_WITH_POPUP_MOUNT_CONSUMER_CUTOVER`，禁止提前裸删或继续生成新内容。
- 未执行 Authoring、未保存 Prefab／Scene、未启动 Unity。全量离线编译 0 错误；Hygiene Gate 为全局 54、Editor 3，仍为 `WARNING`。
- 本岛终态：`KEEP / DIRECT_STRUCTURAL_COMPARE / LEGACY_INTEGRITY_INFRA_RETIRED / UNITY_NOT_RUN / PLAYER_PATH_NOT_READY`。下一责任岛为 Unified Popup Mount；先保留真实 Shell Mount 与当前测试消费者，再退役旧报告、固定状态签名和 YAML 行匹配门禁。
