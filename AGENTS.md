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
- 当前存在全局阻断 `BUILD_SETTINGS_REGRESSION_BLOCKER`：BootEntry 加载 MainHome 报 `Scene is missing from Build Settings`。必须先按 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` 中 `GUARD_PASS_FIX_BUILD_SETTINGS01` 最小修复 Build Settings，再继续 `V0.3-BattlePrepareInteraction01`。
- 当前正式主线包为 `V0.3-BattlePrepareInteraction01`，状态为 `QA_FAIL / GUARD_PASS_FIX01_CONTINUE`；用户手测不通过后，Guard 判断继续最小修复、不重做整包；只允许按 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` 中 Fix01 范围修复，不得扩展；`V0.3-BootEntryFlow01` 已用户手测 QA 通过并完成 RepoOps 记录；`V0.3-MainHomeScene01-Retry` 已完成 ProductFlow 复查层级修复并用户手测通过；`V0.3-BottomNavAndHomeHotspot01` 已用户手测 QA 通过并已请求 RepoOps 记录；旧 `TrialFlowUI` 队列不再是当前正式主线。
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
- 锁功能边界、禁止项、版本范围和稳定基线；不锁导航顺序、布局、颜色、尺寸、临时文案等普通实现细节。

## 7. 冲突处理

当任务要求与锁定文档冲突时：

1. 不执行冲突部分。
2. 明确指出命中的锁、风险等级和可能影响。
3. 列出可安全执行的最小替代方案。
4. 等待用户在有权限的窗口中作出决定。

不得以“优化”“维护”“整理”“统一规范”或“顺手处理”为理由绕过以上规则。
