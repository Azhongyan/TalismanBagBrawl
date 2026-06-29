# MultiTool Sync Hub / 多工具协同同步中枢

状态：`ACTIVE / PRODUCT_FLOW01_SYNC`

日期：2026-06-27

用途：给 Codex、Claude Code、ZCode 等外部工具提供当前状态快照，减少重复阅读历史。

## 0. 权威边界

本文件不是 `Docs/LOCKED/*`，不改变锁定文档。

若本文件与以下内容冲突，以以下内容为准：

```text
F:\Porject\TalismanBagBrawl\AGENTS.md
F:\Porject\TalismanBagBrawl\Docs\LOCKED\*
F:\Porject\TalismanBagBrawl\Docs\ROADMAP\VERSION_ROADMAP.md
F:\Porject\TalismanBagBrawl\Docs\CURRENT\V0.3_PRODUCT_FLOW01.md
F:\Porject\TalismanBagBrawl\Docs\V0.3\V0.3_PACKAGE_QUEUE.md
用户最新明确指令
```

`Cross-System Executor / 跨系统执行体` 仍为 `DISABLED`。外部工具不获得长期正式执行权或治理权。

## 1. 快速入场规则

每个工具开工前最少读取：

```text
1. AGENTS.md
2. Docs/LOCKED/*
3. Docs/ROADMAP/VERSION_ROADMAP.md
4. Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
5. Docs/V0.3/V0.3_PACKAGE_QUEUE.md
6. 本文件：Docs/V0.3/MultiTool/00_SYNC_HUB.md
7. 自己被分配的 assignment / 任务档案
```

外部 AI 不需要默认阅读其他工具的完整对话历史；先读 Package Queue 的包体卡，再读自己的 assignment。

## 2. 官方接受状态 / Codex Maintained

| 字段 | 当前值 |
| --- | --- |
| 当前大版本 | `V0.3` |
| 当前总蓝图 | `V0.3-ProductFlow01` |
| 当前正式主线包 | `V0.3-ForgeFirstUpgradeGuide01` |
| 当前稳定基线 | `V0.2-Tune01 后稳定版本` |
| Package Queue | `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` |
| Roadmap | `Docs/ROADMAP/VERSION_ROADMAP.md` |
| Current Blueprint | `Docs/CURRENT/V0.3_PRODUCT_FLOW01.md` |
| Cross-System Executor 状态 | `DISABLED` |
| 最终收口方 | `Codex` |

## 3. 当前正式队列摘要

```text
0. V0.2-StabilityProtocol01：DONE / LOCKED_BASELINE_READY
1. V0.3-BootEntryFlow01：USER_ACCEPTED / REPOOPS_RECORD_DONE
2. V0.3-MainHomeScene01-Retry：USER_ACCEPTED / PRODUCTFLOW_RECHECK_LAYER_FIX_PASSED
3. V0.3-BottomNavAndHomeHotspot01：USER_ACCEPTED / REPOOPS_REQUEST_SENT
4. V0.3-BattlePrepareInteraction01：USER_ACCEPTED / REPOOPS_UPLOAD_REQUESTED
5. V0.3-PrepareTutorial01：DEFERRED_BY_USER
6. V0.3-BossGuideResult01：USER_ACCEPTED / REPOOPS_UPLOAD_REQUESTED
7. V0.3-ForgeFirstUpgradeGuide01：GUARD_PASS_FORGEFIRSTUPGRADEGUIDE01_NEW_UPGRADE_SCENE / READY_FOR_DEV
8. V0.3-ProductFlowRegression01：QUEUED_FINAL_REGRESSION
```

## 4. 旧协同试跑状态

以下旧包不再作为当前正式主线：

```text
V0.3-NavigationFlow01
V0.3-NavigationFlow01-FixPersistence01
V0.3-TrialFlowUI01
V0.3-CombatContextBar01
V0.3-StageTransition01
V0.3-GuideNarrative01
V0.3-ArtKeyReserve01
```

处理规则：

```text
已完成内容可作为参考或被后续正式包复用。
不得把旧队列 CURRENT 状态当作当前正式主线。
若外部工具仍在旧 TrialFlowUI assignment 上工作，应暂停。
V0.3-BossGuideResult01 已用户手测通过；用户要求发公告并同步 RepoOps 上传。上传必须包含 a5444cb 之后两个 dirty 脚本 final delta：V02StageProgressBar.cs、V02RunResultPanel.cs，不得只上传当前 HEAD checkpoint。当前正式包为 V0.3-ForgeFirstUpgradeGuide01，Guard 已按新场景方案放行；PrepareTutorial01 暂缓，不取消。

MainHome UIUE：GUARD_SYNC_MAINHOME_UIUE01_ACCEPTED。来源 `Docs/V0.3/MultiTool/Handoffs/V03_MainHome_UIUE_SYNC_TO_GUARD.md`；该同步只做首页产品 / UIUE 记忆收口，不授权开发、不要求 RepoOps 上传、不代表 QA 通过。MainHome 后续按“照灯小铺空间化首页”演进；BottomBar 固定：首页 / 养成 / 试炼 / 探索 / 更多；禁止背包底栏；FullBackgroundImageSlot / BG_Root 为黑色满屏背景图片插槽占位；Home 根节点 Image / Outline 不得自动重开；“暂时收起”按钮不得回归；DevelopUpgradePage01 不得混入 MainHome 首页主体功能。

UI Builder / Hand Tune / Runtime Lock：GUARD_SYNC_UI_BUILDER_HAND_TUNE_RUNTIME_LOCK_ACCEPTED。代码负责搭骨架，用户负责定版面；用户在 Unity 非 Play 状态手调后的 Scene Hierarchy / RectTransform / sibling order / active 状态是正式版面真源。进入手调锁定后，Runtime / Bootstrap / Ensure / Builder 只能绑定和驱动已有对象，不得重建 Canvas / Root / SafeArea，不得移动手调对象，不得覆盖 RectTransform / sibling order，不得每次 Play 生成另一套 UI。

Play UI Snapshot：GUARD_SYNC_GLOBAL_UI_HAND_TUNE_RUNTIME_LOCK_ACCEPTED。默认所有 Unity 场景 / UI 场景都是非 Play 场景文件为最终基准，Play 状态跟随；只有用户显式点击对应场景的 Editor Snapshot / Apply 工具时，才允许把 Play UI 回写为新的场景基准。Snapshot 不是 MainHome 专用概念；每个具体场景必须单独建立场景级白名单和菜单项，禁止跨场景、全项目扫写、触碰 BuildSettings、存档、主流程、Button onClick、delegate、脚本字段、Prefab 连接。

UI Runtime Lock / 沉余 fallback：GUARD_SYNC_UI_RUNTIME_LOCK_REMEDIATION_PLAN_ACCEPTED。原因是早期 Runtime fallback / Builder / Ensure / CreateRuntime 长期存在，导致缺节点重建、同名 Root 多份、Play 对象与 Edit 场景真源不唯一。处理必须分包：A 审计包 V0.3-UIRuntimeLockAudit01（只列清单不开发，已放行）；B MainHome+BottomNav 唯一真源；C BattlePrepare UI Lock；D Boot/Guide/Upgrade fallback；E 历史对象隔离。每包必须有干净输出点，RepoOps 负责仓库快照。

UIRuntimeLockAudit01：AUDIT_COMPLETE / NO_CODE_NO_SCENE_CHANGE。只读审计确认最高风险为 Runtime fallback / Builder / Ensure / CreateRuntime 仍会兜底造 UI、重排层级、写 RectTransform；高风险点包括 MobilePortraitLayoutRuntimeFix、V03BattlePrepareInteractionController、V03TalismanUpgradeSceneController、V03MainHomeSceneBootstrap、V03ForgeFirstUpgradeGuideController、V02RunFlowController。MainHomeBottomNavRuntimeLock01、BattlePrepareRuntimeLock01、BootGuideUpgradeRuntimeLock01 均已完成并用户验收通过；下一包为 V0.3-SceneResidueIsolation01，需单独 Guard 收口后再开发。

MainHomeBottomNavRuntimeLock01：USER_ACCEPTED / REPOOPS_RECORD_REQUESTED。用户确认“通过回收 下一任务”。本包完成 MainHome / BottomNav / FullBackground / Guide 层级唯一场景真源与 Runtime 覆盖退场第一阶段；Play 时不再补建、销毁、重挂层级或重写布局；未修改 .unity 场景文件。验证：Unity batch 编译通过；FIX02_SCENE_STATIC_SUCCESS；FIX02_PLAYMODE_FIRST_FRAME_SUCCESS / FIX02_SMOKE_SUCCESS；git diff --check 通过。
BattlePrepareRuntimeLock01：USER_ACCEPTED / REPOOPS_RECORD_REQUESTED。用户确认“好的 当前任务通过”。本包实际修改仅限 V03BattlePrepareInteractionController.cs；Runtime 优先绑定场景内既有 V03 BattlePrepare 节点；缺正式锁定节点时输出 BattlePrepareRuntimeLock warning / error；过渡 ItemTray 使用 V03BattlePrepareItemTrayRoot；不再新建同名 V02BottomOperationArea；旧 V02BottomOperationArea 只 lookup + hide legacy，不 rename / destroy；移除或避免 SetAsLastSibling、GridSlots_5x5 layout 重写、CreatePanel("V02BottomOperationArea")。验证：Unity batch compile 通过；git diff --check 通过；静态检查通过。
BootGuideUpgradeRuntimeLock01：USER_CONFIRMED_FLOW_OK / REPOOPS_RECORD_REQUESTED。用户确认“好的 我觉得当前包流程都好了 你同步给guard吧”。BootEntry、Guide Overlay、UpgradePage 已从运行时创建 / 重排 UI 收束为绑定场景真源节点或报错；升级场景通过显式 Editor 菜单补齐 V03Upgrade_ResourceService、V03Upgrade_UpgradeService、V03Upgrade_MainTrialFlowService、EventSystem。未混入 V02RunFlowController、BuildSettings、Save UI tools、SceneResidueIsolation、奖励 / Boss / RunFlow 语义、存档结构、升级数值。
下一 UI Runtime Lock 包：V0.3-SceneResidueIsolation01 / WAITING_GUARD_ASSIGNMENT。候选问题：Scene_TalismanBag_V02_FormationCounter Edit Mode 仍显示 V0.2 battle page，V0.3 battle page 不可见；需先 Guard assignment，不得直接清理或删除。

UI Slot Authoring Lock：GUARD_SYNC_UI_SLOT_AUTHORING_LOCK_ACCEPTED。Codex 可以管 Slot Contract：Root / Slot / 默认灰盒 / 默认层级 / 脚本 / 引用 / 缺失组件 / iconKey / visualKey / 结构检查；用户手调并 Lock 后，Layout Authoring 由场景真源说了算。Runtime / Bootstrap / Ensure / CreateRuntime / Builder 不得自动覆盖 RectTransform、父节点、siblingIndex、LayoutGroup、GridLayoutGroup、ContentSizeFitter、LayoutElement、ScrollRect、Image / Outline / Text 等手调布局与视觉字段。Audit01 需额外审计 sizeDelta、anchoredPosition、anchor、pivot、SetParent、SetSiblingIndex、GridLayout、ContentSizeFitter、LayoutElement、ScrollRect viewport/content、Smoke/Verify 顺手修布局等风险。
```

## 5. 工具状态卡

### Codex

```text
当前职责：总控 / 记忆治理 / Package Queue 维护 / 最终收口
当前包体：V0.3-ForgeFirstUpgradeGuide01
正在做：维护 ProductFlow01 正式队列；BootEntryFlow01 已通过；MainHomeScene01-Retry ProductFlow 复查层级修复已手测通过；BottomNavAndHomeHotspot01 已手测通过并请求 RepoOps 记录；BattlePrepareInteraction01 item tray tuning continuation 已手测通过并请求 RepoOps 正式上传；BossGuideResult01 已手测通过并请求 RepoOps 上传；ForgeFirstUpgradeGuide01 已 Guard 放行新建 V03 符箓升级独立场景方案
不会做：把 ProductFlow01 当成单个大包开发
```

### Claude Code

```text
当前职责：READY_IDLE / 等待分配
当前包体：未分配
当前 worktree：F:\Porject\TalismanBagBrawl_claude
不会做：未授权写入 Unity 工程；未授权修改 LOCKED / AGENTS
下一步：读取 ProductFlow01 路线与 Package Queue；当前无外部工具分配；ForgeFirstUpgradeGuide01 可由 Codex 开发窗口按 Guard assignment 执行
```

### ZCode

```text
当前职责：INACTIVE / 用户近期不用
旧包体：全部暂停
当前正式包体：未分配
当前 worktree：F:\Porject\TalismanBagBrawl_zcode
不会做：参与当前 V0.3 主线；写入 Unity 工程；commit/tag/push
下一步：无需处理。未来若用户重新启用，再由 Codex / 用户写入新的 assignment
```

## 6. 等待 Codex 收口的交接包

| 来源工具 | 包体 | 类型 | 路径 | 当前处理状态 |
| --- | --- | --- | --- | --- |
| `无` | `无` | `无` | `无` | `无待处理` |

## 7. 下次启动提示

### 如果你是 ZCode

```text
1. 先读 AGENTS.md / Docs/LOCKED/*
2. 读 Docs/ROADMAP/VERSION_ROADMAP.md
3. 读 Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
4. 读 Docs/V0.3/V0.3_PACKAGE_QUEUE.md
5. 读 Docs/V0.3/MultiTool/Assignments/ZCODE_CURRENT_ASSIGNMENT.md
6. 当前状态为 INACTIVE；不要执行任何旧任务，等待用户未来重新启用
```

### 如果你是 Claude Code

```text
1. 先读 AGENTS.md / Docs/LOCKED/*
2. 读 Docs/ROADMAP/VERSION_ROADMAP.md
3. 读 Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
4. 读 Docs/V0.3/V0.3_PACKAGE_QUEUE.md
5. 未被分配前不要写工程
```

## 8. 更新规则

- Codex 可更新全文件。
- Claude Code / ZCode 未来若被启用，只能更新自己的状态卡、交接链接和自己持有的文件锁。
- 任何工具不得通过修改本文件获得超出 assignment / 任务档案的权限。
