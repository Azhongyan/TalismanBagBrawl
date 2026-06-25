# CODEX PREFLIGHT CHECK

Codex 不得直接从任务文档进入写代码。每个窗口、每次新任务的第一件事必须是任务入场审查。

## 强制检查清单

1. 当前工作目录是否为 `F:\Porject\TalismanBagBrawl` 或其子目录？
2. 是否确认 `Assets / ProjectSettings / Packages` 同时存在？
3. 是否已完整读取 `AGENTS.md` 与 `Docs/LOCKED/*`？
4. 当前任务属于哪个版本？
5. 是否符合当前版本范围？
6. 风险等级是绿灯、黄灯还是红灯？
7. 是否触碰长期项目方向锁？
8. 是否触碰 V0.2 稳定基线锁？
9. 是否触碰当前版本范围锁？
10. 是否触碰当前任务技术边界锁？
11. 是否需要 Code Survey？
12. 允许修改哪些文件？
13. 禁止修改哪些文件？
14. 需要验证哪些黄金路径？
15. 回滚条件和回滚范围是什么？
16. 本任务是否试图修改项目记忆文件；当前会话是否为唯一记忆治理窗口？
17. 当前窗口在版本流水线中的角色是什么：producer tech pm、tech architect、codex task writer、guard-agents、开发窗口或 QA reviewer？
18. 若当前任务来自版本流水线，是否已有用户确认的小版本名 / 包体编号 / 任务档案？
19. 若当前状态是 QA 不通过，是否已有用户写明的不通过原因、tech architect 失败归因和 codex task writer 修复任务档案？
20. 若需要跨窗口流转，目标角色是否已在 `CODEX_ROLE_WINDOW_REGISTRY.md` 注册？
21. 当前窗口是否具备真实读取 / 发送目标窗口的工具能力；如果没有，是否已切换为手动转发模式？
22. 场景、页面、UI 或入口任务是否写明真实用户路径、交付形态、默认状态、首帧可见性和重启后预期？
23. 自动测试是否会主动激活、反射调用、临时实例化或直接切换到本应由产品自行进入的目标状态？
24. task writer 任务档案是否已有 guard-agents 的明确 `GUARD_PASS`？
25. 当前使用的 architect / task writer / QA 产物是否来自注册窗口；如果来自子代理，是否明确标记为临时草案？
26. 若为 QA 不通过后的修复，用户是否已审批经过 Guard 检查的修复任务包？
27. 若用户已回复“通过 / QA 通过 / 手测通过”，是否已调用已注册 RepoOps 窗口完成小版本记录并取得 `REPOOPS_RECORD_DONE`？
28. 若用户已回复“通过 / QA 通过 / 手测通过”，是否已向 producer tech pm、tech architect、codex task writer、QA reviewer、guard-agents 发送 `PASS_SYNC_BROADCAST` 并取得 `PASS_SYNC_SENT`？若无法真实发送，是否已生成手动转发同步包并停止等待？
29. 若当前是新任务开始或用户要求“开始流转新任务”，是否已进入 `WAITING_ASSIGNMENT` 并按 `SINGLE_LANE_TASK_INTAKE` 单线推进？
30. 当前开发依据是否来自 codex task writer 正式任务档案，而不是 producer tech pm、tech architect 或多角色并发回复的合并内容？
31. 若当前执行体声称是 ZCode 或其他跨系统执行体，是否已确认 `CROSS_SYSTEM_EXECUTOR_PROTOCOL.md` 当前状态？若为 `DISABLED`，必须停止，不得作为正式开发 / QA / 修复执行体进入流水线。
32. 若未来重新启用跨系统执行体，是否已确认其只能读 `AGENTS.md` / `Docs/LOCKED/*`，不得修改项目记忆文件，不得承接 guard-agents、producer tech pm、tech architect、codex task writer 或 RepoOps 主责？
33. 若未来重新启用且任务由 ZCode 完成，是否已按 `CROSS_SYSTEM_EXECUTOR_PROTOCOL.md` 落盘 Handoff Package，并提供 `git diff` 或经用户授权后的 `[ZCode]` commit hash？

## 停止条件

遇到以下任一情况必须停止并回报：

- 找不到真实 Unity 工程根目录。
- 无法确认三个 Unity 工程标识。
- 锁定文档缺失、不可读或相互冲突。
- 需要覆盖未知旧文件。
- 需要修改未获授权的红灯内容。
- 当前任务范围不清。
- 当前会话不是记忆治理窗口，却要求修改 `AGENTS.md` 或 `Docs/LOCKED/*`。
- 开发窗口没有已确认任务档案，却试图直接开发。
- QA 不通过后，开发窗口试图绕过 tech architect 与 codex task writer 自由修复。
- 目标角色未注册或不可达时，当前窗口试图假装已经完成跨窗口自动流转。
- 当前窗口没有拿到 tech architect 失败归因与 codex task writer 修复任务档案，却在 QA 不通过后继续修复开发。
- 场景、页面或 UI 任务没有明确交付形态与真实用户可见路径。
- 自动测试会替功能主动制造本次要验证的成功状态。
- 任务档案或修复任务档案没有取得 guard-agents 的明确 `GUARD_PASS`。
- 使用子代理临时草案冒充已注册固定角色的正式产物。
- QA 不通过后，修复任务包尚未获得用户审批却试图恢复开发。
- 用户确认通过后，未完成 RepoOps 小版本记录却直接同步进入下一任务包。
- 用户确认通过后，未完成 `PASS_SYNC_BROADCAST` / `PASS_SYNC_SENT` 却直接进入下一任务包。
- 当前窗口没有真实发送能力或未生成手动转发包，却声称其他常驻角色已经知道本包通过。
- 新任务开始时，开发窗口并发请求 producer tech pm、tech architect、codex task writer 下发内容。
- 新任务开始时，开发窗口用 `PASS_SYNC_BROADCAST` 代替 `SINGLE_LANE_TASK_INTAKE`。
- 开发窗口把 producer tech pm 或 tech architect 的产物当作正式工单直接开发。
- 开发窗口收到多角色并发回复后自行合并、改名或开始开发，而不是等待 codex task writer 正式任务档案。
- 未获用户单独明确授权时，RepoOps 或其他窗口试图自动 commit、tag、push、切分支或回滚。
- ZCode 或其他跨系统执行体试图修改 `AGENTS.md` 或 `Docs/LOCKED/*`。
- `CROSS_SYSTEM_EXECUTOR_PROTOCOL.md` 处于 `DISABLED`，但 ZCode 或其他跨系统执行体仍试图作为正式开发 / QA / 修复执行体进入流水线。
- ZCode 作为 QA 执行体发现不通过后，绕过 Codex tech architect / codex task writer / guard-agents / 用户审批直接修复。
- ZCode 未落盘 Handoff Package，却声称 Codex 已可完整了解其工作。
- ZCode 未获用户单独明确授权，却执行 commit、tag、push、切分支或回滚。
