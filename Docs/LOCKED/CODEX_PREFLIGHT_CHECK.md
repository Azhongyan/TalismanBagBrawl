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
17. 当前窗口在当前轻量流程中的角色是什么：guard-agents、开发窗口或 RepoOps？producer tech pm / tech architect / codex task writer / QA reviewer 旧线程已停用自动流转。
18. 若当前任务来自版本流水线，是否已有用户确认的小版本名 / 包体编号 / 任务档案？
18.0. 当前任务是否已对齐 `Docs/ROADMAP/VERSION_ROADMAP.md` 与 `Docs/CURRENT/V0.3_PRODUCT_FLOW01.md`；是否误把 `V0.3-ProductFlow01` 当成单个开发包？
18.1. 当前版本是否存在 Package Queue，例如 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md`；若存在，当前包是否与队列指向一致？
19. 若当前状态是 QA 不通过，是否已有用户写明的不通过原因；普通 bug 是否已同步 Guard 并获得用户授权按原 assignment 修复；异常 / 红线失败是否已有用户提供的 GPT / 外部技术归因或明确替代输入？
20. 若需要跨窗口流转，目标角色是否已在 `CODEX_ROLE_WINDOW_REGISTRY.md` 注册且未被标记为停用自动流转？
21. 当前窗口是否具备真实读取 / 发送目标窗口的工具能力；如果没有，是否已切换为手动转发模式？
22. 场景、页面、UI 或入口任务是否写明真实用户路径、交付形态、默认状态、首帧可见性和重启后预期？
23. 自动测试是否会主动激活、反射调用、临时实例化或直接切换到本应由产品自行进入的目标状态？
24. 当前任务是否触发 Guard 例外条件；若触发，task writer 任务档案是否已有 guard-agents 的明确 `GUARD_PASS`？普通已排队包不再默认要求每包 Guard 审查。
25. 当前使用的 GPT / 外部技术归因是否明确由用户提供并限定为上游输入；如果来自子代理，是否明确标记为临时草案？
26. 若为 QA 不通过后的修复，用户是否已审批经过 Guard 收口的修复范围？
27. 若用户已回复“通过 / QA 通过 / 手测通过”，是否已向 Guard + RepoOps 发送 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS`，或生成手动转发同步包？
28. 是否确认不再向 producer tech pm、tech architect、codex task writer、QA reviewer 旧线程发送默认同步或 `PASS_SYNC_BROADCAST`？
29. 若当前是新任务开始或用户要求“开始流转新任务”，是否已先读取 Package Queue；若当前包已在队列中，是否直接走 Light Guard + RepoOps 模式；若不在队列中，是否停止等待用户提供 GPT / 外部策划后的路线、拆包或失败归因输入？
30. 当前开发依据是否来自 Package Queue、Guard 收口后的 assignment，或用户明确指定的任务，而不是旧角色并发回复的合并内容？
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
- QA 不通过后，开发窗口未同步 Guard 就自由修复；若失败涉及架构 / 红线 / 状态机 / 存档 / Boss / 奖励 / 数值 / 主流程，还绕过用户提供的 GPT / 外部技术归因或明确替代输入。
- 目标角色未注册或不可达时，当前窗口试图假装已经完成跨窗口自动流转。
- 当前窗口没有拿到用户确认技术归因 / 替代输入与 Guard 修复边界收口，却在异常 QA 不通过后继续修复开发。
- 场景、页面或 UI 任务没有明确交付形态与真实用户可见路径。
- 自动测试会替功能主动制造本次要验证的成功状态。
- 触发 Guard 例外条件的任务档案或修复任务档案没有取得 guard-agents 的明确 `GUARD_PASS`。
- 使用子代理临时草案冒充已注册固定角色的正式产物。
- QA 不通过后，修复任务包尚未获得用户审批却试图恢复开发。
- 用户确认通过后，未完成 Guard + RepoOps 状态同步却直接进入下一任务包。
- 用户确认通过后，仍尝试向 producer tech pm、tech architect、codex task writer、QA reviewer 旧线程广播，浪费上下文。
- 当前窗口没有真实发送能力或未生成手动转发包，却声称 Guard 或 RepoOps 已经知道本包通过。
- 新任务开始时，开发窗口未先读取 Package Queue，直接并发请求旧角色下发内容。
- 新任务开始时，开发窗口用 `PASS_SYNC_BROADCAST` 代替 Package Queue / Light Path / 必要时用户提供的 GPT / 外部策划输入。
- 开发窗口把旧角色产物当作正式工单直接开发，而未经过 Guard 收口或用户明确 assignment。
- 开发窗口收到多角色并发回复后自行合并、改名或开始开发，而不是等待 Guard 收口或用户明确 assignment。
- 未获用户单独明确授权时，RepoOps 或其他窗口试图自动 commit、tag、push、切分支或回滚。
- ZCode 或其他跨系统执行体试图修改 `AGENTS.md` 或 `Docs/LOCKED/*`。
- `CROSS_SYSTEM_EXECUTOR_PROTOCOL.md` 处于 `DISABLED`，但 ZCode 或其他跨系统执行体仍试图作为正式开发 / QA / 修复执行体进入流水线。
- ZCode 作为 QA 执行体发现不通过后，绕过 Codex tech architect / codex task writer / guard-agents / 用户审批直接修复。
- ZCode 未落盘 Handoff Package，却声称 Codex 已可完整了解其工作。
- ZCode 未获用户单独明确授权，却执行 commit、tag、push、切分支或回滚。
