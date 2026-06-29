# 《符箓背包》Codex 项目守则

本文件是本仓库所有 Codex 会话的强制入口。适用于从仓库根目录及其任意子目录启动的新窗口、新线程、分叉线程和后续版本开发窗口。

## 1. 工程身份与路径

- 游戏：《符箓背包》
- IP：《明箓山下》
- 已确认 Unity 工程根目录：`F:\Porject\TalismanBagBrawl`
- 注意：磁盘上的父目录实际拼写为 `Porject`，不是 `project`。
- 开始任何任务前，必须确认根目录同时存在：
  - `Assets`
  - `ProjectSettings`
  - `Packages`
- 找不到上述真实根目录或三个 Unity 标识时，立即停止，不得猜测路径，不得在 C 盘或其他目录新建工程文件。

## 2. 每个新窗口的强制学习流程

Codex 接到任务后的第一件事是任务入场审查，不是写代码。

每个会话在分析、规划、编辑或运行工程命令前，必须完整读取以下文件：

1. `Docs/LOCKED/PROJECT_DIRECTION_LOCK.md`
2. `Docs/LOCKED/STABLE_BASELINE_LOCK.md`
3. `Docs/LOCKED/CURRENT_VERSION_SCOPE_LOCK.md`
4. `Docs/LOCKED/CORE_INVARIANTS.md`
5. `Docs/LOCKED/DO_NOT_TOUCH.md`
6. `Docs/LOCKED/PAGE_FLOW_LOCK.md`
7. `Docs/LOCKED/RISK_LEVEL_RULE.md`
8. `Docs/LOCKED/GOLDEN_PATH_QA.md`
9. `Docs/LOCKED/CODEX_PREFLIGHT_CHECK.md`
10. `Docs/LOCKED/MEMORY_FILE_APPROVAL_RULE.md`
11. `Docs/LOCKED/CODEX_VERSION_PIPELINE_LOCK.md`
12. `Docs/LOCKED/CODEX_ROLE_WINDOW_REGISTRY.md`
13. `Docs/LOCKED/DELIVERY_ACCEPTANCE_GATE.md`
14. `Docs/LOCKED/CROSS_SYSTEM_EXECUTOR_PROTOCOL.md`

若当前版本存在 ROADMAP / CURRENT / Package Queue，还必须读取：

```text
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
```

ROADMAP / CURRENT 定义当前版本总蓝图；Package Queue 用于判断当前包、上一包验收状态和下一包。默认采用 Light Guard + RepoOps 模式：用户在 GPT / 外部策划流程拆包，Guard 收口边界和队列，任务窗口开发并自测，用户手测后只同步 Guard + RepoOps。producer tech pm、tech architect、codex task writer、QA reviewer 旧线程均停用自动流转。

不得只读取本文件的摘要后直接开发。任一锁定文档缺失、不可读、相互冲突或超过可用上下文时，必须停止并回报。

## 3. 入场审查

开发前必须先判断：

- 当前任务属于哪个版本。
- 是否符合长期项目方向。
- 是否破坏 V0.2 稳定基线。
- 是否超出当前 V0.3 版本范围。
- 风险等级是绿灯、黄灯还是红灯。
- 是否触碰当前任务的禁止修改边界。
- 允许修改和禁止修改的文件分别是什么。
- 需要执行哪些黄金路径 QA，失败时如何回滚。

黄灯任务必须先完成最小 Code Survey。红灯任务未获用户针对具体范围的明确授权时，必须停止并提交冲突报告。

## 4. 稳定基线与当前收口

- 当前安全基线：`V0.2 稳定版本`
- 当前版本方向：`V0.3-ProductFlow01`
- 任何新功能不得破坏 V0.2 已稳定的主流程、战斗表现、道具、阵盘、Boss 触发、奖励、数值与存档结构。
- 当前 V0.3 是产品基础流转版，必须按 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` 小包推进；`V0.3-ProductFlow01` 是总蓝图，不是单个开发任务。
- 历史阻断 / 待回归确认：`BUILD_SETTINGS_REGRESSION_BLOCKER` 曾导致 BootEntry 加载 MainHome 报 `Scene is missing from Build Settings`。后续 ProductFlow 回归必须确认 BootEntry → MainHome / Trial 均可加载；若再次失败，优先按 Build Settings 回归处理，不得重做功能包。
- `V0.3-BossGuideResult01` 已用户手测通过，状态为 `USER_ACCEPTED / REPOOPS_UPLOAD_REQUESTED`；本轮通过点包含 BossInfo 手动挑战、整备入口、BossRewardPanel 结算、RunResultPanel 重复结算清理，以及最终的 V02StageProgressBar / V02RunResultPanel 调整。RepoOps 上传必须包含当前 HEAD `a5444cb` 之后两个 dirty 脚本 final delta：`Assets/_Game/Scripts/TalismanBag/V02/UI/V02StageProgressBar.cs`、`Assets/_Game/Scripts/TalismanBag/V02/Result/V02RunResultPanel.cs`，不得只上传 `a5444cb checkpoint: v0.3 stage progress bar tuning wip not qa passed`。当前正式包为 `V0.3-ForgeFirstUpgradeGuide01`，状态为 `GUARD_PASS_FORGEFIRSTUPGRADEGUIDE01_NEW_UPGRADE_SCENE / READY_FOR_DEV`；最新用户需求已覆盖 `IMAGE_SLOT_ONLY`，允许新建 V03 符箓升级独立场景并最小修正旧 V02 领奖后出口；`V0.3-PrepareTutorial01` 暂缓，不取消；旧 `TrialFlowUI` 队列不再是当前正式主线。
- 首页 UIUE 已完成 Guard 同步收口：`GUARD_SYNC_MAINHOME_UIUE01_ACCEPTED`。`Scene_TalismanBag_V03_MainHome` 后续口径是“照灯小铺空间化首页”，不是功能大厅；底部 Bar 固定为 `首页 / 养成 / 试炼 / 探索 / 更多`，禁止“背包”底栏入口；`FullBackgroundImageSlot / BG_Root` 是正式背景图片插槽临时占位，当前黑色满屏、不露背后内容、不拦截输入；Home 根节点 `Image / Outline` 暂不使用，不得被 Runtime / Builder 自动重开；“暂时收起”按钮不得回归；DevelopUpgradePage01 属于后续养成 / 符箓升级页，不得把升级主体功能混进 MainHome 首页。
- UI Builder / Hand Tune / Runtime Lock 已写入长期门禁：`GUARD_SYNC_UI_BUILDER_HAND_TUNE_RUNTIME_LOCK_ACCEPTED`。代码可以在 Builder 阶段搭 UI 骨架；用户在 Unity 非 Play 状态手调后的 Scene Hierarchy / RectTransform / sibling order / active 状态是正式版面真源；进入手调锁定后，Runtime 只允许绑定事件、更新文字/图片/数据、SetActive、播放动画和驱动业务状态，不得重建 Canvas / Root / SafeArea，不得移动手调对象，不得覆盖 RectTransform / sibling order，不得每次 Play 生成另一套层级。
- Play UI Snapshot 规则已提升为全局收口：`GUARD_SYNC_GLOBAL_UI_HAND_TUNE_RUNTIME_LOCK_ACCEPTED`。所有 Unity 场景 / UI 场景默认都是“非 Play 场景文件为最终基准，Play 状态跟随场景文件”；只有用户显式点击对应场景的 Editor Snapshot / Apply 工具时，才允许把 Play 状态 UI 回写为新的非 Play 基准。每个具体场景若需要该工具，必须单独建立场景级白名单和菜单项，明确目标 Scene、Root / Canvas / 子树、允许回写组件与禁止回写组件；禁止跨场景、全项目扫写、触碰 BuildSettings、存档、主流程、Button onClick / delegate / 脚本字段 / Prefab 连接。
- UI Runtime Lock / 沉余 fallback 收口计划已接受：`GUARD_SYNC_UI_RUNTIME_LOCK_REMEDIATION_PLAN_ACCEPTED`。原因是早期 Runtime fallback / Builder / Ensure / CreateRuntime 为跑通功能长期存在，导致缺节点重建、同名 Root 多份、Play 对象与 Edit 场景真源不唯一。必须分包处理，先 `V0.3-UIRuntimeLockAudit01` 审计只列清单不开发，再分 MainHome+BottomNav、BattlePrepare、Boot/Guide/Upgrade fallback、历史对象隔离包逐步收口；不得混成一个大清理包。
- `V0.3-UIRuntimeLockAudit01` 已只读审计完成，状态为 `AUDIT_COMPLETE / NO_CODE_NO_SCENE_CHANGE`；核心风险点包括 MobilePortraitLayoutRuntimeFix、V03BattlePrepareInteractionController、V03TalismanUpgradeSceneController、V03MainHomeSceneBootstrap、V03ForgeFirstUpgradeGuideController、V02RunFlowController 的 Runtime fallback / Builder / Ensure / CreateRuntime / SetAsLastSibling / RectTransform 写入风险。
- `V0.3-MainHomeBottomNavRuntimeLock01` 已开发收口并由用户确认“通过回收 下一任务”，状态为 `USER_ACCEPTED / REPOOPS_RECORD_REQUESTED`。本包完成 MainHome / BottomNav / FullBackground / Guide 层级的唯一场景真源与 Runtime 覆盖退场：Play 时只校验现有 FullBackground / BottomNav，不再运行时补建、销毁、重挂层级或重写布局；缺 V03MainHomeUiueRoot / 热点对象只报错，不再生成 UIUE / Hotspot；底栏保持 active，缺 ForgeGuide 不再 AddComponent；静态 / PlayMode 校验覆盖 MobileSafeAreaRoot、底栏唯一且 active、背景不挡输入、Home 根 Image / Outline 不复活、无“暂时收起 / 背包”回归。验证通过：Unity batch 编译、FIX02_SCENE_STATIC_SUCCESS、FIX02_PLAYMODE_FIRST_FRAME_SUCCESS / FIX02_SMOKE_SUCCESS、git diff --check；未修改 `.unity` 场景文件。
- `V0.3-BattlePrepareRuntimeLock01` 已开发收口并由用户确认“好的 当前任务通过”，状态为 `USER_ACCEPTED / REPOOPS_RECORD_REQUESTED`。本包实际修改仅限 `Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/V03BattlePrepareInteractionController.cs`：BattlePrepare Runtime 改为优先绑定场景内既有 V03 BattlePrepare 节点；缺正式锁定节点时保留过渡 fallback 但输出 BattlePrepareRuntimeLock warning / error；过渡 ItemTray 使用 `V03BattlePrepareItemTrayRoot`，不再新建同名 `V02BottomOperationArea`；旧 `V02BottomOperationArea` 只 lookup + hide legacy，不 rename / destroy；移除或避免 SetAsLastSibling、GridSlots_5x5 layout 重写、`CreatePanel("V02BottomOperationArea")`。验证通过：Unity batch compile `Logs/codex_battleprepare_runtime_lock_compile.log` return code 0；git diff --check 通过；静态检查确认 controller 内无 SetAsLastSibling、无 GridSlots_5x5、无 CreatePanel("V02BottomOperationArea")。
- `V0.3-BootGuideUpgradeRuntimeLock01` 已开发收口并由用户确认“好的 我觉得当前包流程都好了 你同步给guard吧”，状态为 `USER_CONFIRMED_FLOW_OK / REPOOPS_RECORD_REQUESTED`。本包处理 BootEntry / Guide Overlay / Upgrade fallback Runtime Lock 与必要场景真源绑定：BootEntry 不再 runtime 创建 BootEntryCanvas / pages / buttons，改为绑定场景内 BootEntryCanvas、LoadingPage、StartGamePage、OpeningStoryPanel、StartGameButton、SkipOpeningStoryButton；MainHome Guide Overlay 不再 runtime 创建黑幕 / 图片槽 / 文本或 SetAsLastSibling，改为绑定场景内 HomeUpgrade / HomeTrial / shared guide roots；V03TalismanUpgradeSceneController 不再 Play 时创建 Canvas / EventSystem / full page / runtime services，改为绑定现有 Canvas、PageRoot、guide overlay、info popup、item tray、UpgradeService、MainTrialFlowService、EventSystem；升级场景已通过显式 Editor 菜单补齐 V03Upgrade_ResourceService、V03Upgrade_UpgradeService、V03Upgrade_MainTrialFlowService、EventSystem。未有意修改 V02RunFlowController、BuildSettings、Save UI tools、SceneResidueIsolation、奖励 / Boss / RunFlow 语义、存档结构、升级数值、AGENTS / LOCKED / Package Queue。下一 UI Runtime Lock 包为 `V0.3-SceneResidueIsolation01 / WAITING_GUARD_ASSIGNMENT`；用户观察到 `Scene_TalismanBag_V02_FormationCounter` Edit Mode 仍显示 V0.2 battle page，作为该下一包候选问题，不混入本包。
- UI Slot Authoring Lock 已写入：`GUARD_SYNC_UI_SLOT_AUTHORING_LOCK_ACCEPTED`。Codex 可以管 Slot Contract：创建 Root / Slot / 默认灰盒 / 默认层级 / 脚本 / 引用 / 缺失组件 / iconKey / visualKey / 结构检查；用户手调并 Lock 后，Layout Authoring 由场景真源说了算，Runtime / Bootstrap / Ensure / CreateRuntime / Builder 不得自动覆盖 RectTransform、父节点、siblingIndex、LayoutGroup、GridLayoutGroup、ContentSizeFitter、LayoutElement、ScrollRect、Image / Outline / Text 等手调布局与视觉字段。
- 不得顺手修 Bug、扩大版本范围、进行无关重构、commit、tag、push 或回滚。

## 5. 项目记忆文件的窗口权限

以下文件属于项目记忆与锁定文档：

- `AGENTS.md`
- `Docs/LOCKED/*`

建立这些文件的本次原始 Codex 对话是唯一“记忆治理窗口”。

- 除本原始窗口外，所有其他窗口对这些文件只有读取权。
- 其他窗口不得创建、修改、补充、整理、格式化、覆盖、移动、重命名或删除这些文件。
- 即使用户在其他窗口要求或批准修改，其他窗口仍必须拒绝，并提示用户回到本原始窗口处理。
- 其他窗口不得自行认定、继承或转移“记忆治理窗口”身份。
- 本原始窗口修改这些文件前，也必须先列出拟修改文件、原因、摘要、风险及对稳定基线的影响，并等待用户明确审批。
- 权限转移、规则解除或治理窗口变更，只能由本原始窗口在用户明确审批后写入锁定文档。

详细规则见 `Docs/LOCKED/MEMORY_FILE_APPROVAL_RULE.md`。

## 6. 版本流水线与 Guard-agents 职责

长期版本生产工作流见 `Docs/LOCKED/CODEX_VERSION_PIPELINE_LOCK.md`。
角色窗口注册规则见 `Docs/LOCKED/CODEX_ROLE_WINDOW_REGISTRY.md`。
跨系统执行体规则见 `Docs/LOCKED/CROSS_SYSTEM_EXECUTOR_PROTOCOL.md`。

- 本长期置顶常驻窗口认领 `guard-agents` 职责。
- `guard-agents` 只守边界、红线、Package Queue 和记忆收口，不直接开发。
- 新开发窗口只能执行 Package Queue 指向的当前包、用户指定的小版本 / 包体编号，或经 Guard 收口后的用户明确 assignment。
- 新任务开始必须先读取当前版本 Package Queue，例如 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md`。普通已排队包走 Light Path：Package Queue → Guard 收口 assignment → 开发窗口开发 / 自测 / 用户手测清单 → 用户手测 → 同步 Guard + RepoOps。不得自动发送 producer tech pm、tech architect、codex task writer、QA reviewer 旧线程。
- `Cross-System Executor / 跨系统执行体` 当前为 `DISABLED`；ZCode 未重新获得用户审批启用前不得正式承接开发、外部 QA 或修复执行，只能作为外部参考 / 临时草案来源。即使未来启用，也不得获得治理权，不得承接 producer tech pm / tech architect / codex task writer / guard-agents / RepoOps 主责。
- 普通已排队包由 Guard 轻量收口；触发越界、红线、改规则、外部协同污染或异常失败时，必须取得明确 `GUARD_PASS`，沉默或未返回不等于通过。
- 用户回复“通过 / QA 通过 / 手测通过”后，任务窗口只向 Guard + RepoOps 同步 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS`；Guard 更新记忆与 Package Queue，RepoOps 记录版本状态。默认不再发送 `PASS_SYNC_BROADCAST`。
- 用户回复“不通过 + 原因”后，开发窗口停止自由修改并先同步 Guard。普通当前包实现 bug 可在用户授权后按原 assignment 范围修复；若涉及架构、红线、状态机、存档、Boss、奖励、数值或主流程，开发窗口必须等待用户提供 GPT / 外部技术归因或明确替代输入，并由 Guard 收口。
- 触发 Guard 例外条件的修复任务必须取得 `GUARD_PASS` 并由用户审批，开发窗口才恢复修复权限。
- 如果角色窗口未注册或当前窗口无法真实发送 / 读取目标窗口，只能生成报告交给用户手动转发，不得假装自动流转已完成。
- 子代理或当前窗口代行产生的内容只能标记为临时草案，不等于已注册固定角色的正式产物。
- 场景、页面和 UI 任务必须遵守 `DELIVERY_ACCEPTANCE_GATE.md`；自动测试不得替功能主动制造成功状态。
- UI 修改必须遵守“原地编辑优先”：能改 RectTransform / Layout / Text / Image / CanvasGroup 就不新建 GameObject；能复用原按钮就不删旧按钮重建；任何带脚本、delegate、事件绑定、引用字段、serialized data 的对象默认禁止删除重建；必须重建时先列迁移清单并等 Guard 批准。
- UI 任务还必须遵守 Builder / Hand Tune / Runtime Lock：Builder 只负责初始骨架；用户手调锁定后，Runtime / Bootstrap / Ensure / Builder / SceneBuilder 只能读取、绑定和驱动已有对象，不得重排或覆盖用户手调版面。
- Play UI Snapshot / Apply 工具不得自动运行；没有用户显式点击时，Play 状态改动不得被视为正式版面真源。若战斗运行时纯动态 HUD 需要例外，必须由任务说明 / Guard assignment 单独授权，不得默认例外。
- 锁功能边界、禁止项、版本范围和稳定基线；不锁导航顺序、布局、颜色、尺寸、临时文案等普通实现细节。

## 7. 冲突处理

当任务要求与锁定文档冲突时：

1. 不执行冲突部分。
2. 明确指出命中的锁、风险等级和可能影响。
3. 列出可安全执行的最小替代方案。
4. 等待用户在有权限的窗口中作出决定。

不得以“优化”“维护”“整理”“统一规范”或“顺手处理”为理由绕过以上规则。
