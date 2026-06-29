# CURRENT VERSION SCOPE LOCK

当前版本总蓝图：`V0.3-ProductFlow01`

当前执行方式：按 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` 小包推进，不允许把 `V0.3-ProductFlow01` 当作单个 Codex 开发任务。

权威说明：

```text
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
```

## 当前 V0.3 允许围绕

V0.3 只允许围绕首次完整产品体验流转：

```text
App 启动入口
Loading / StartGame / 服务器占位
OpeningStoryPanel
新玩家进入首场教学战斗
老玩家进入照灯小铺首页
照灯小铺首页
首页热点
底部导航：首页 / 探索 / 试练 / 锻造 / 更多
试练入口
战斗页内整备升降交互
新手整备引导
1-10 Boss 前后流转
1-10 Boss 后首次锻造 / 升级引导
回首页后进入 2-1
V0.3 产品流转总回归
```

## 当前 V0.3 正式拆包

必须按以下队列逐包推进：

```text
0. V0.2-StabilityProtocol01
1. V0.3-BootEntryFlow01
2. V0.3-MainHomeScene01-Retry
3. V0.3-BottomNavAndHomeHotspot01
4. V0.3-BattlePrepareInteraction01
5. V0.3-PrepareTutorial01
6. V0.3-BossGuideResult01
7. V0.3-ForgeFirstUpgradeGuide01
8. V0.3-ProductFlowRegression01
```

## 当前包

当前正式主线包：

```text
V0.3-ForgeFirstUpgradeGuide01
```

当前包 Guard assignment：

```text
GUARD_PASS_FORGEFIRSTUPGRADEGUIDE01_NEW_UPGRADE_SCENE

允许 V0.3-ForgeFirstUpgradeGuide01 开发窗口开始执行新场景方案。
旧 IMAGE_SLOT_ONLY 边界已被用户最新需求覆盖。
本包仍不重做完整锻造系统，但允许新建 V03 符箓升级独立场景承载完整首次升级页。

锻造入口提示目标：允许 V03 首页弹“回到首页 / 升级符箓”图片引导或最小提示；点击锻造 / 炼符 / 升级入口后进入新 V03 符箓升级独立场景。不得做复杂 GuideSystem。

允许文件 / 行为：
- Assets/_Game/Scripts/TalismanBag/V03/Navigation/V03NavigationFlowController.cs
- Assets/_Game/Scripts/TalismanBag/V03/MainHome/V03MainHomeSceneBootstrap.cs
- 新增或修改 V03 Forge / Navigation 外围 guide/UI 脚本
- 新增一个 V03 符箓升级独立场景，例如 Scene_TalismanBag_V03_TalismanUpgrade.unity 或 Scene_TalismanBag_V03_ForgeUpgrade.unity（二选一）
- 必要时最小修改 V03NavigationFlow01SceneBuilder.cs / verifier 做引用绑定和静态验证
- 允许 Build Settings 接入新 V03 符箓升级场景，但不得清空既有 V03_BootEntry / V03_MainHome / V02_FormationCounter / Run15Min 或旧基线场景

允许最小触碰 V02RunFlowController.cs，仅用于修正 1-10 Boss 领奖后出口，阻断旧战斗内 OpenChapterOneHomeGreybox() / TryOpenChapterOneUpgradePanel() 培养闭环；不得改 Boss 战斗状态机、Boss 触发规则或 RunFlow 主流程语义。

禁止：完整锻造系统；资源 / 奖励 / 掉落 / 升级配置 / 数值改动；SaveData / MainTrialProgressData / PlayerPrefs 结构改动；UpgradeService 消耗 / 保存规则改动；Boss 触发、Boss 奖励、RunFlow 主流程语义、PageState、FormationState、DamageText、V02FormationGridFrame 改动；PrepareTutorial / ProductFlowRegression；清理 dirty 文件；commit/tag/push。

升级页功能边界：允许道具栏式符箓列表、道具信息弹窗、物资数量、升级前 / 后数据对比、背景图插槽；当前必须保障首次升级目标 fire_talisman_basic / chapterOneUpgradeItemId 完整可用，可展示当前拥有 / 可升级符箓列表但不得扩成完整锻造系统。
```

首页 UIUE 同步收口：

```text
GUARD_SYNC_MAINHOME_UIUE01_ACCEPTED

来源文档：Docs/V0.3/MultiTool/Handoffs/V03_MainHome_UIUE_SYNC_TO_GUARD.md
性质：首页产品 / UIUE 文档同步，不授权开发，不要求 RepoOps 上传，不代表 QA 通过。

Scene_TalismanBag_V03_MainHome 后续应朝“照灯小铺空间化首页”演进，不得继续做成灰盒功能大厅或战斗 / 养成入口堆叠页。

推荐结构：
- BG_Root / FullBackgroundImageSlot
- SceneHotspot_Root
- TopBar_Root
- RightQuickBar_Root
- BottomNavBar_Root
- PopupLayer_Root
- ToastLayer_Root
- GuideHighlight_Root

FullBackgroundImageSlot / BG_Root 是正式背景图片插槽临时占位：当前黑色满屏，不得露背后内容，不拦截输入，不依赖 Home 根节点 Image 兜底。

Home 根节点 Image / Outline 暂不使用，不得被 Runtime / Builder / Verifier 自动重开。“暂时收起”按钮不得回归。

BottomBar 固定为：首页 / 养成 / 试炼 / 探索 / 更多。禁止 BattleBackpack / 背包 / PVP / Store / Activity / Mail / Settings 成为底栏入口。

首页热点口径：照灯账本、道藏典册、旧物线索簿、梦签、小满、后屋、青石坊街口。柜台必须是照灯账本，不是单纯试炼入口。

三本册认知：照灯账本=我现在该干嘛；道藏典册=我已经获得/解锁过什么；旧物线索簿=我追查到哪里了。

TopBar 只显示玩家身份组 + 资源组 + 设置；不得显示当前关卡，不得显示当前任务。当前阶段 / 当前目标交给照灯账本承载。

DevelopUpgradePage01 属于后续养成 / 符箓升级页口径，不应把完整符箓升级 UI、符箓列表、升级前后属性对比、材料消耗等养成页主体混进 MainHome 首页。后续若启动必须先 Code Survey。
```

UI Builder / Hand Tune / Runtime Lock 长期收口：

```text
GUARD_SYNC_UI_BUILDER_HAND_TUNE_RUNTIME_LOCK_ACCEPTED

长期规则：代码负责搭骨架，用户负责定版面；定版后代码不能再乱动版面。

Builder 阶段允许代码生成初始 UI 骨架、Canvas、Root、按钮、面板、占位图和基础层级。

Hand Tune 阶段由用户在 Unity 非 Play 状态下手动调整大小、位置、层级、图片、颜色、间距、文字摆放等。

Lock 阶段后，用户手调后的 Scene Hierarchy / RectTransform / sibling order / active 状态成为正式版面真源。

Runtime 只允许绑定按钮事件、更新文字 / 图片 / 数据、SetActive、播放动画、做业务状态切换、读取已有场景对象引用。

Runtime / Bootstrap / Ensure / Builder / SceneBuilder 禁止重建 Canvas / Root / SafeArea，禁止移动用户手调对象，禁止覆盖 RectTransform position / anchoredPosition / sizeDelta / anchors / pivot / scale，禁止覆盖 sibling order，禁止每次 Play 重新生成另一套层级。

MainHome / ForgeFirstUpgradeGuide01 / 后续 V03 符箓升级场景必须遵守：Play 状态视觉结果遵从非 Play 手调结果。

Play UI Snapshot / Apply 全局例外：默认仍是非 Play 场景文件为最终基准；只有用户显式点击对应场景的 Editor Snapshot / Apply 工具时，才允许把 Play 状态 UI 回写成新的非 Play 基准。该工具不得自动运行。

Play UI Snapshot / Apply 不是 MainHome 专用概念，适用于所有 Unity 场景 / UI 场景。但每个具体场景若需要该工具，必须单独建立场景级白名单和菜单项，明确目标 Scene、Root / Canvas / 子树、允许回写组件与禁止回写组件；禁止跨场景、全项目扫写、触碰 BuildSettings、存档、主流程、Button onClick、delegate、脚本字段、Prefab 连接。战斗运行时纯动态 HUD 若需例外，必须由任务说明 / Guard assignment 单独授权。
```

UI Runtime Lock / 沉余 fallback 收口计划：

```text
GUARD_SYNC_UI_RUNTIME_LOCK_REMEDIATION_PLAN_ACCEPTED

问题不是单点 Snapshot 工具失败，而是早期 Runtime fallback / Builder / Ensure / CreateRuntime 逻辑长期存在：缺节点重建、同名 Root 多份时 runtime 自行挑对象、Play 对象与 Edit 场景真源不唯一，导致用户手调 UI 下次 Play 仍可能回退。

长期规则：Hand Tune / Lock 后缺正式节点应报错或输出缺口报告，不得 Runtime 自动重建另一套默认 UI；fallback / Ensure / CreateRuntime / Builder 必须有退场条件。

分包：
A. V0.3-UIRuntimeLockAudit01：审计包，只列清单，不开发。状态：GUARD_PASS_AUDIT_ONLY / READY_FOR_AUDIT。
B. V0.3-MainHomeBottomNavRuntimeLock01：首页 + BottomNav 唯一真源包。
C. V0.3-BattlePrepareRuntimeLock01：BattlePrepare UI Lock 包。
D. V0.3-BootGuideUpgradeRuntimeLock01：BootEntry / Guide Overlay / Upgrade fallback 包。
E. V0.3-SceneResidueIsolation01：历史文件 / 旧对象隔离包，最后处理。

每包必须有干净输出点：C# 编译通过、Console 无 Error、git diff --check 通过、git status 清晰、用户手测清单明确；branch / commit / tag / push 交给 RepoOps。
```

UIRuntimeLockAudit01 完成：

```text
V0.3-UIRuntimeLockAudit01 已只读审计完成。
状态：AUDIT_COMPLETE / NO_CODE_NO_SCENE_CHANGE。
本次无代码改动、无场景改动、无提交。

最高风险：Runtime fallback / Builder / Ensure / CreateRuntime 仍会兜底造 UI、重排层级、写 RectTransform。

高风险点：
- MobilePortraitLayoutRuntimeFix.cs
- V03BattlePrepareInteractionController.cs
- V03TalismanUpgradeSceneController.cs
- V03MainHomeSceneBootstrap.cs
- V03ForgeFirstUpgradeGuideController.cs
- V02RunFlowController.cs

V0.3-MainHomeBottomNavRuntimeLock01 已开发收口并由用户确认“通过回收 下一任务”。
状态：USER_ACCEPTED / REPOOPS_RECORD_REQUESTED。
验证：Unity batch 编译通过；FIX02_SCENE_STATIC_SUCCESS；FIX02_PLAYMODE_FIRST_FRAME_SUCCESS / FIX02_SMOKE_SUCCESS；git diff --check 通过；未修改 .unity 场景文件。
收口：MainHome / BottomNav / FullBackground / Guide 层级的唯一场景真源与 Runtime 覆盖退场已完成第一阶段。
V0.3-BattlePrepareRuntimeLock01 已开发收口并由用户确认“好的 当前任务通过”。
状态：USER_ACCEPTED / REPOOPS_RECORD_REQUESTED。
实际修改文件：Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/V03BattlePrepareInteractionController.cs。
收口：BattlePrepare Runtime 改为优先绑定场景内既有 V03 BattlePrepare 节点；缺正式锁定节点时输出 BattlePrepareRuntimeLock warning / error；过渡 ItemTray 使用 V03BattlePrepareItemTrayRoot；不再新建同名 V02BottomOperationArea；旧 V02BottomOperationArea 只 lookup + hide legacy，不 rename / destroy；移除或避免 SetAsLastSibling、GridSlots_5x5 layout 重写、CreatePanel("V02BottomOperationArea")。
验证：Unity batch compile 通过（Logs/codex_battleprepare_runtime_lock_compile.log，return code 0）；git diff --check 通过；静态检查确认 controller 内无 SetAsLastSibling、无 GridSlots_5x5、无 CreatePanel("V02BottomOperationArea")。
V0.3-BootGuideUpgradeRuntimeLock01 已开发收口并由用户确认“好的 我觉得当前包流程都好了 你同步给guard吧”。
状态：USER_CONFIRMED_FLOW_OK / REPOOPS_RECORD_REQUESTED。
实际修改文件：
- Assets/_Game/Scripts/TalismanBag/V03/BootEntry/V03BootEntryFlowController.cs
- Assets/_Game/Scripts/TalismanBag/V03/Forge/V03ForgeFirstUpgradeGuideController.cs
- Assets/_Game/Scripts/TalismanBag/V03/Forge/V03TalismanUpgradeSceneController.cs
- Assets/_Game/Scripts/TalismanBag/V03/Editor/V03BootEntryFlow01SceneBuilder.cs
- Assets/_Game/Scripts/TalismanBag/V03/Editor/V03TalismanUpgradeSceneBuilder.cs
- Assets/_Game/Scenes/Scene_TalismanBag_V03_BootEntry.unity
- Assets/_Game/Scenes/Scene_TalismanBag_V03_TalismanUpgrade.unity
收口：
- BootEntry runtime lock：V03BootEntryFlowController 不再 runtime 创建 BootEntryCanvas / pages / buttons，改为绑定场景内 BootEntryCanvas、LoadingPage、StartGamePage、OpeningStoryPanel、StartGameButton、SkipOpeningStoryButton；缺绑定报错。
- MainHome Guide Overlay runtime lock：V03ForgeFirstUpgradeGuideController 不再 runtime 创建 guide black mask / image slot / text 或 SetAsLastSibling，改为绑定场景内 HomeUpgrade / HomeTrial / shared guide roots；缺 slot 报错。
- Upgrade fallback runtime lock：V03TalismanUpgradeSceneController 不再 Play 时创建 Canvas / EventSystem / full page / runtime services，改为绑定现有 Canvas、V03TalismanUpgradePageRoot、guide overlay、info popup、item tray、UpgradeService、MainTrialFlowService、EventSystem；缺绑定报错。
- Upgrade scene authoring：显式 Editor 菜单补齐 V03Upgrade_ResourceService、V03Upgrade_UpgradeService、V03Upgrade_MainTrialFlowService、EventSystem；菜单禁用 Play Mode。
验证：Unity batch compile 曾通过；BootEntry scene-node binding 通过；BootEntry 绑定后 compile 通过；git diff --check 通过；静态检查确认 BootEntry 与 Upgrade 必需节点存在。最终以用户手测确认当前包流程 OK 为准。
下一包：V0.3-SceneResidueIsolation01。
状态：NEXT_LOCK_PACKAGE / WAITING_GUARD_ASSIGNMENT。
候选问题：用户观察到 Scene_TalismanBag_V02_FormationCounter Edit Mode 仍显示 V0.2 battle page，V0.3 battle page 不可见；本问题未混入 BootGuideUpgradeRuntimeLock01，应在 SceneResidueIsolation01 或单独 Guard assignment 中处理。

BootGuideUpgradeRuntimeLock01 assignment：

```text
GUARD_PASS_BOOTGUIDEUPGRADE_RUNTIMELOCK01

包体定位：
V0.3-BootGuideUpgradeRuntimeLock01 = BootEntry / Guide Overlay / Upgrade fallback Runtime Lock 包。
目标不是重做 BootEntry、不是重做升级页、不是重做引导系统。
目标是让 BootEntry、Guide Overlay、V03 符箓升级页在 Hand Tune / Lock 后遵守“场景对象为真源；Runtime 只绑定和驱动，不重排、不重建、不覆盖布局”的长期规则。

必须先做最小 Code Survey：
- Assets/_Game/Scripts/TalismanBag/V03/BootEntry/V03BootEntryFlowController.cs
- Assets/_Game/Scripts/TalismanBag/V03/Forge/V03ForgeFirstUpgradeGuideController.cs
- Assets/_Game/Scripts/TalismanBag/V03/Forge/V03TalismanUpgradeSceneController.cs
- Assets/_Game/Scripts/TalismanBag/V03/Editor/V03BootEntryFlow01SceneBuilder.cs
- Assets/_Game/Scripts/TalismanBag/V03/Editor/V03TalismanUpgradeSceneBuilder.cs
- 相关 smoke / verifier
- 只读调查 Assets/_Game/Scripts/TalismanBag/V02/Run/V02RunFlowController.cs 中旧 CreateRuntime 调用背景，不修改该文件。

允许范围：
- 允许修改 V03BootEntryFlowController.cs：将 EnsureCanvas / BuildPages / CreatePage / CreateLabel / CreateButton 等运行时创建 UI，改为优先绑定 BootEntry 场景内既有 LoadingPage、StartGamePage、OpeningStoryPanel、按钮与文本节点；缺正式节点时输出 BootGuideUpgradeRuntimeLock warning / error，不静默创建另一套 UI。
- 允许修改 V03ForgeFirstUpgradeGuideController.cs：将 EnsureGuideRoot / CreateMask / CreateImageSlot / SetAsLastSibling 等运行时创建或置顶引导黑幕、图片槽、文本的逻辑，改为绑定 V03 MainHome 或相关页面中已存在的 Guide Root / ImageSlot；缺节点时报缺口，不自动补建。
- 允许修改 V03TalismanUpgradeSceneController.cs：将 EnsureCanvas / EnsureEventSystem / BuildPage / CreateBackgroundSlot / CreateTopBar / CreateItemList / CreateGuideOverlay / CreateInfoPopup / CreateBottomBar / SetAsLastSibling 等运行时创建完整升级页的逻辑，改为绑定 V03 TalismanUpgrade 场景内已存在的 PageRoot、背景槽、资源栏、道具栏、详情面板、信息弹窗、GuideOverlay、BottomBar；缺节点时报缺口。
- 允许最小修改 V03BootEntryFlow01SceneBuilder.cs、V03TalismanUpgradeSceneBuilder.cs 和相关 verifier / smoke：只能作为 Editor-only 显式 Builder / 只读校验，不得在 Play / Smoke / Verify 中顺手修布局或重建场景。
- 允许最小修改场景引用绑定：Scene_TalismanBag_V03_BootEntry.unity、Scene_TalismanBag_V03_MainHome.unity 中 guide slot 相关节点、Scene_TalismanBag_V03_TalismanUpgrade.unity。场景改动仅限补齐/绑定本包正式节点，不得重排用户已手调布局。

明确禁止：
- 不改 V02RunFlowController.cs；该文件本包只读调查。
- 不改 SaveData / MainTrialProgressData / PlayerPrefs 结构。
- 不改升级数值、资源消耗、UpgradeService 规则、奖励、掉落。
- 不改 Boss / RunFlow 主流程语义、PageState、FormationState、DamageText、V02FormationGridFrame。
- 不改 BuildSettings；历史 BootEntry missing Build Settings blocker 仍留到 ProductFlowRegression 或独立 BuildSettings 回归处理。
- 不做 SceneResidueIsolation；历史对象隔离另包处理。
- 不开发 Play-state Save UI / Snapshot Apply 工具；该工具若需要另开包。
- 不修改 AGENTS.md / Docs/LOCKED/* / Package Queue；不 commit / tag / push。

交付契约：
- BootEntry、GuideOverlay、UpgradePage 进入 Play 后不得每次重建 Canvas / PageRoot / GuideRoot / UpgradePageRoot / Popup / BottomBar。
- 已存在且用户手调的 RectTransform、父节点、sibling order、Image / Outline / Text、LayoutGroup、GridLayout、ScrollRect 关键字段不得被 Runtime 覆盖。
- 缺正式节点时必须清晰 warning / error，不能静默生成默认 UI 并假装通过。
- BootEntry 仍能展示 Loading / StartGame / OpeningStory 并进入 Home / Trial 路径。
- Guide Overlay / 图片槽仍能按业务显隐，不强制 SetAsLastSibling 覆盖用户层级。
- V03 符箓升级页仍能展示道具栏、物资数量、道具信息、升级前后对比、背景图插槽，并能完成首次升级后回首页。

QA：
- 非 Play 手调 BootEntry / Guide / Upgrade 相关 UI 后进入 Play，首帧布局不回退。
- Stop Play 后再次 Play，不生成重复 Canvas / PageRoot / GuideRoot / UpgradePageRoot / Popup / BottomBar。
- BootEntry 新玩家 / 老玩家路径不坏。
- Guide Overlay 只在业务需要时显示，图片槽位置跟随场景手调。
- Upgrade 页面基础交互不坏：选择道具、查看信息、资源数量显示、升级前后对比、首次升级、回首页。
- 不触碰 V0.2 Golden Path。
```

BattlePrepareRuntimeLock01 assignment：

```text
GUARD_PASS_BATTLEPREPARE_RUNTIMELOCK01

包体定位：
V0.3-BattlePrepareRuntimeLock01 = BattlePrepare UI Lock 包。
目标不是重做战斗整备功能、不是重写拖拽系统、不是清理历史对象。
目标是让 BattlePrepare 相关 UI 在 Lock 后遵守“场景对象为真源；Runtime 只绑定和驱动，不重排、不重建、不覆盖布局”的长期规则。

允许范围：
- 主要允许修改 Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/V03BattlePrepareInteractionController.cs。
- 允许将 Runtime 自动 AddComponent / 创建 overlay、motionRoot、item tray、actionBar、V03ItemTrayTemplates 等逻辑，改为优先绑定既有场景对象；缺正式节点时输出缺口报告，不自动补建另一套 UI。
- 允许退场或收窄 V02BottomOperationArea 重命名 / 同名创建、boardFrame.SetParent(motionRoot)、GridLayoutGroup / ScrollRect / RectTransform 写入、SetAsLastSibling、item tray slot / template root 搬移等会覆盖手调布局的行为。
- 允许新增最小只读 verifier / smoke，用于检查对象存在、重复 Root、首帧未被 Runtime 重排、整备入口仍可打开、道具栏 / 棋盘 / ActionBar 关键引用存在。
- DraggableTalismanItemView 的拖拽 SetParent / anchoredPosition 属交互行为，默认只读观察；不得借本包重写拖拽核心。

禁止范围：
- 不改战斗规则、拖拽核心、FormationState、RunFlow、PageState、SaveData、PlayerPrefs、Boss、奖励、数值、DamageText、V02FormationGridFrame。
- 不改 V02RunFlowController；该文件仅作为 TryOpenPrepareThen 调用方背景，不在本锁包修改。
- 不做 BootEntry / Guide Overlay / Forge / Upgrade fallback。
- 不做 SceneResidueIsolation；历史对象隔离另包处理。
- 不修改 AGENTS.md / Docs/LOCKED/* / Package Queue；不 commit / tag / push。

交付契约：
- Play 时 BattlePrepare UI 不得每次重建或重排道具栏、阵盘、ActionBar。
- 已存在且用户手调的 RectTransform、父节点、sibling order、GridLayout、ScrollRect 关键布局字段不得被 Runtime 覆盖。
- 整备入口、道具栏、格子滚动、分类标签、道具栏内移动 / 互换、拖到棋盘、继续战斗仍保持可用。
- 棋盘、伤害数字、阵盘供能、道具触发、主流程不坏。

QA：
- 非 Play 手调 BattlePrepare 相关 UI 后进入 Play，首帧布局不回退。
- Stop Play 后再次 Play，不生成重复 overlay / motionRoot / item tray / actionBar / V02BottomOperationArea。
- 点击整备能打开既有整备态；点击继续战斗能回到战斗态。
- 道具栏 5 列 x 8 行、可滚动、分类标签、道具移动 / 互换不消失。
- V0.2 Golden Path 关键战斗表现不坏。
```
```

MainHomeBottomNavRuntimeLock01 assignment：

```text
GUARD_PASS_MAINHOME_BOTTOMNAV_RUNTIMELOCK01

定位：首页 + BottomNav 唯一真源包。

目标：让 Scene_TalismanBag_V03_MainHome 非 Play 手调结果成为 Play 真源，阻止 Runtime fallback / Bootstrap / Ensure / Builder 再覆盖 MainHome / BottomNav / FullBackground / Guide 层级。

允许：最小修改 V03MainHomeSceneBootstrap.cs、V03NavigationFlowController.cs、MainHomeGreyboxPanel.cs、HomeHotspotView.cs、V03NavigationFlow01SceneBuilder.cs、V03MainHomeSceneFix02.cs、Scene_TalismanBag_V03_MainHome.unity。

禁止：不动战斗、BattlePrepare、FormationCounter、升级主流程、V03TalismanUpgradeSceneController、V02RunFlowController、SaveData、MainTrialProgressData、PlayerPrefs、Boss、奖励、数值、FormationState、PageState；不清理全项目历史对象；不删除不确定引用对象；不恢复“暂时收起”按钮；不让背包进底栏；不修改 AGENTS / LOCKED；不 commit/tag/push。

验收：MainHome 非 Play 手调版面与 Play 首帧一致；停止 Play 后再次 Play 不回退；BottomNavBar_Root 唯一；FullBackgroundImageSlot 黑色满屏不露底且不拦截输入；Home 根 Image / Outline 不被重开；底栏为 首页 / 养成 / 试炼 / 探索 / 更多 且无背包入口。
```

UI Slot Authoring Lock：

```text
GUARD_SYNC_UI_SLOT_AUTHORING_LOCK_ACCEPTED

长期规则：Codex 搭骨架，用户调布局，Lock 后只读校验。

Slot Contract 由 Codex 负责：Root / Slot / 默认灰盒 / 默认层级 / 脚本 / 引用 / 缺失组件 / iconKey / visualKey / 结构检查。

Layout Authoring 由用户手调后的 Scene / Prefab 真源负责。

用户手调并 Lock 后，Runtime / Bootstrap / Ensure / CreateRuntime / Builder 不得自动覆盖：
- RectTransform.anchoredPosition / sizeDelta / anchorMin / anchorMax / pivot / localScale / localRotation
- transform.SetParent / 父节点
- transform.SetSiblingIndex / siblingIndex
- LayoutGroup spacing / padding
- GridLayoutGroup cellSize / constraint
- ContentSizeFitter
- LayoutElement
- RectMask2D / Mask
- ScrollRect viewport / content / movement / scrollbar
- Image / Outline / Text / TMP_Text 手调视觉字段

Ensure / Builder 退场：缺节点输出缺口报告，尺寸不同输出差异报告，父节点不同输出差异报告，不自动修。只有用户显式执行 Apply / Rebuild / Build Default Layout / Snapshot 时才允许写回场景。
```

历史完成包 Guard assignment（BossGuideResult01）：

```text
GUARD_PASS_BOSSGUIDERESULT01_NO_HIGHLIGHT

用户明确指定 V0.3-BossGuideResult01 开始任务，并排除“高亮引导”需求。
V0.3-PrepareTutorial01 暂缓，不取消。

允许：复用 BossInfoPanel / BossInfoViewModel / BossInfoConfig / V02BossRewardPanel，在 1-10 boss round 战前显示 BossInfoState，继续使用手动“开始攻打”按钮进入 BossBattleState，Boss 胜利后走现有 OpenBossRewardPanel / ConfirmBossReward 奖励到账与回首页路径。

允许最小触碰：Assets/_Game/Scripts/TalismanBag/V02/Run/V02RunFlowController.cs，仅用于 1-10 BossInfoState 接入、手动开始 Boss、复用现有 Boss 奖励结算路径；允许最小修改 BossInfoPanel 文案 / view model / 按钮口径展示。

禁止：高亮挑战 Boss 按钮 / 小地图 Boss 点；自动开 Boss；破坏 2-9 Boss 前停止或 2-10 Boss 手动挑战；改 Boss 奖励表、掉落表、数值、伤害、冷却、胜负规则；改 SaveData / PlayerPrefs / MainTrialProgressData；重写 RunFlow / PageState / FormationState；借本包开发 Forge / PrepareTutorial / ProductFlowRegression。
```

历史完成包 Fix 收口（BossGuideResult01）：

```text
GUARD_PASS_BOSSGUIDERESULT01_FIX_PREPARE_BUTTON

用户反馈：2-10 BossInfo 点整备没有拉出整备页，依然停在战斗页。

允许：只修改 Assets/_Game/Scripts/TalismanBag/V02/Run/V02RunFlowController.cs 中 HideBossInfoForPrepare()，将单纯日志改为调用现有 V03BattlePrepareInteractionController.TryOpenPrepareThen(...)；onOpened 只写提示日志；未找到整备控制器时 fallback 提示，不自动开 Boss。

禁止：不改 Boss 触发规则；不自动开 Boss；不改 V03BattlePrepareInteractionController；不改奖励表、掉落表、数值、SaveData、MainTrialProgressData、FormationState、PageState；不做高亮挑战 Boss 按钮 / 小地图 Boss 点 / PrepareTutorial。
```

最近完成 / 上传中：

```text
V0.3-BossGuideResult01 已用户手测通过。
状态：USER_ACCEPTED / REPOOPS_UPLOAD_REQUESTED。
当前 HEAD：a5444cbee7cc0756653772aa1a1bfc6065799fc5 / a5444cb checkpoint: v0.3 stage progress bar tuning wip not qa passed。
用户已明确手测通过，因此状态应从 WIP / NOT QA PASSED 更新为 USER_ACCEPTED / QA PASSED。

RepoOps 上传必须包含 a5444cb 之后两个 dirty 脚本 final delta：
- Assets/_Game/Scripts/TalismanBag/V02/UI/V02StageProgressBar.cs
- Assets/_Game/Scripts/TalismanBag/V02/Result/V02RunResultPanel.cs

最终通过点：
- BossInfo 手动挑战、整备入口、BossRewardPanel 结算、RunResultPanel 重复结算清理。
- V02StageProgressBar 串珠式进度条只展示 1-1 / 1-5 / 1-10；使用 Node_Start / Node_Mid / Node_Boss 与 Dot_Start / Dot_Mid / BossDot_Boss 稳定槽位；后续章节复用同一套槽位；ProgressLine 按 BaseLine 长度计算进度。
- V02RunResultPanel Show() 置顶，事件弹窗 / 主角败北弹窗不再被战斗页、整备层、进度条或其他面板挡住。
```

上一包收口：

```text
V0.3-BattlePrepareInteraction01 item tray tuning continuation 已用户手测 QA 通过。
状态：USER_ACCEPTED / REPOOPS_UPLOAD_REQUESTED。
用户明确要求 RepoOps 正式上传当前版本。
正式上传必须包含 c921d4c 之后未提交的最终位置 delta：
- Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/V03BattlePrepareInteractionController.cs
- NormalPosition = new(0f, -920f)
- PreparePosition = new(0f, -220f)
```

当前全局阻断：

```text
BUILD_SETTINGS_REGRESSION_BLOCKER
```

BootEntry 曾加载 MainHome 报 `Scene is missing from Build Settings`。该问题作为历史阻断 / 待回归确认项保留；后续 ProductFlowRegression01 必须确认 BootEntry → MainHome / Trial 均可加载。若再次失败，优先按 Build Settings 回归处理，不得重做功能包。

BattlePrepareInteraction01 已完成后续 item tray tuning continuation，并由用户手测 QA 通过；此处不再作为当前阻断。

旧 `V0.3-TrialFlowUI01`、`V0.3-CombatContextBar01`、`V0.3-StageTransition01` 等不再作为当前正式主线包。

最近完成：

```text
V0.3-BootEntryFlow01 已开发完成、Unity smoke 通过、用户手测 QA 通过，并取得 REPOOPS_RECORD_DONE。
V0.3-MainHomeScene01-Retry 已完成 ProductFlow 复查层级修复：MainHomeRoot 恢复为 Canvas 直接子节点；用户手测通过；未触碰存档、奖励、数值、Boss、战斗整备、DataCatalog、首页热点逻辑或试炼 / 炼符 delegate。
V0.3-BottomNavAndHomeHotspot01 已开发完成并用户手测 QA 通过：底栏口径为 首页 / 探索 / 试练 / 锻造 / 更多；梦签保留为首页中部热点；背包 / BattleBackpack 未进入底栏；未触碰 RunFlow / PageState / FormationState / SaveData / Boss / 奖励 / 数值 / DamageText / V02FormationGridFrame；RepoOps 记录请求已发送，等待回执。
V0.3-BattlePrepareInteraction01 item tray tuning continuation 已用户手测 QA 通过：道具栏 5 columns × 8 rows = 40 slots；可视区域 5 rows，内部纵向滚动和右侧 scrollbar；道具可在 item tray slot 内移动 / 互换且不会消失；NormalPosition = new(0f, -920f)，PreparePosition = new(0f, -220f)；用户明确要求 RepoOps 正式上传当前版本，且必须包含 c921d4c 之后未提交的最终位置 delta。
V0.3-BossGuideResult01 已用户手测通过：BossInfo 手动挑战、整备入口、BossRewardPanel 结算、RunResultPanel 重复结算清理、V02StageProgressBar 串珠式进度条、V02RunResultPanel 弹窗置顶均按用户手测通过为准；RepoOps 上传必须包含 a5444cb 之后两个 dirty 脚本 final delta。
```

## 交付形态硬边界

- `V0.3-ProductFlow01` 是总蓝图，不是单包。
- 每个子包必须只做自己的范围。
- 不得把 BootEntryFlow、MainHome、BattlePrepare、BossGuide、ForgeGuide 混在同一个开发任务里。
- 涉及场景、页面、UI、入口或用户可见状态时，必须写清真实用户路径、默认状态、首帧可见性和重启后预期。
- 自动测试不得主动激活、反射调用、临时实例化或直接切状态来制造通过。

## 高风险包规则

以下包必须 Code Survey，并在触碰红线前回 Guard：

```text
V0.3-BattlePrepareInteraction01
V0.3-PrepareTutorial01
V0.3-BossGuideResult01
V0.3-ForgeFirstUpgradeGuide01
```

最高风险：

```text
V0.3-BattlePrepareInteraction01
```

因为它可能触碰：

```text
战斗页 Root
棋盘位置
背包显示
上拉道具栏 800×800 区域与格子化滚动展示
分类标签筛选
拖拽
FormationGrid
战斗状态切换
新手引导遮罩
```

`V0.3-BattlePrepareInteraction01` 统一把下半部分上拉窗口命名为“道具栏”，不再叫“背包 UI”。允许实现战斗整备上拉道具栏区域的 800×800 格子化 UI、区域内滚动和分类标签筛选；这不等于授权完整多格背包系统、背包经济或 SaveData 结构调整。

Fix01 允许继续修复以下失败项：

```text
废弃当前 V0.3 整备实现中对旧 V02BottomOperationArea 的显示依赖。
修正棋盘 + 道具栏上拉 / 下收衔接，使道具栏上边缘紧贴棋盘下边缘。
将黑色遮罩放在棋盘 + 道具栏背后，只做背景压暗，不挡交互。
合并 V02PrimaryActionButtons 与当前战斗底部操作口径。
隐藏或降级刷新供能 / 重置阵盘等当前整备不需要的按钮。
将用户可见“战后驻阵”口径统一为“整备”。
```

Fix01 不允许：

```text
删除或重写 V02 基线对象导致 V0.2 回退。
为清理历史残渣而删除或重建带脚本 / 事件 / 引用 / serialized data 的对象。
重写 RunFlow / PageState / FormationState。
改变战后流程语义；“战后驻阵=整备”只允许 UI 文案、入口显示和提示口径统一。
把首页底部导航直接塞进战斗页。
借底部操作区合并扩展无关入口。
```

场景残渣处理规则：

```text
功能包内不得顺手清理历史残渣。
阻挡当前功能的旧对象，优先隐藏、禁用、绕开、调 sibling / layer 或新增非破坏性包裹层。
不阻挡当前功能的历史残渣，留待独立 SceneResidueCleanup 包处理。
```

## 当前版本明确不做

- 真实账号
- 真实服务器
- 热更新
- SDK
- 完整 CG
- 公告系统
- 正式探索玩法
- 正式天机炉玩法
- 正式三选一肉鸽
- 正式 PVP
- 正式后屋生产玩法
- 完整多格背包系统（不含 V0.3-BattlePrepareInteraction01 内的上拉道具栏区域格子化 UI 展示）
- 五行系统
- 连携技系统
- 3-10 新内容
- 完整新战斗系统
- 战斗状态机重构
- RunFlow 重构
- FormationState 重构
- Boss 流程重写
- 奖励表调整
- 数值调优
- SaveData 结构调整

超出本文件范围的需求不得被“顺手实现”。应先回报范围冲突并等待用户决定后续版本归属。
