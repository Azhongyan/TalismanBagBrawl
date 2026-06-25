# CROSS SYSTEM EXECUTOR PROTOCOL

当前状态：`DISABLED / 暂停生效`

禁用原因：用户确认此前与 ZCode 商量的方案理解有误，已要求先禁用本协议但不删除文件。

在用户重新明确审批启用或重写前：

- ZCode 不具备正式 `Cross-System Executor / 跨系统执行体` 身份。
- ZCode 不能作为正式开发执行体、外部 QA 执行体或修复执行体进入 Codex 流水线。
- ZCode 产出的内容只能视为外部参考 / 临时草案。
- Codex 不得把本文件作为授权 ZCode 承接任务的依据。

本文锁定《符箓背包》在 Codex 与外部系统之间的跨系统执行体规则。

当前试运行对象：`ZCode`

本文属于 `Docs/LOCKED/*`。只有 Codex 原始记忆治理窗口可在用户明确审批后修改；ZCode 或其他外部系统只有读取权。

## 1. 定义

类别名：`Cross-System Executor / 跨系统执行体`

身份层级：

- 非固定角色窗口。
- 低于 `CODEX_ROLE_WINDOW_REGISTRY.md` 中已注册的固定角色。
- 只在用户明确指定某次任务时临时生效。
- 不获得项目记忆治理权。

跨系统执行体不是：

- guard-agents
- producer tech pm
- tech architect
- codex task writer
- RepoOps 主责

## 2. 技术前提

Codex 与 ZCode 不共享线程级对话记忆。

跨系统共同记忆只能通过同一个仓库中的磁盘文件、`git diff`、`git log` 和交接包实现。

因此：

- ZCode 不能声称已读取 Codex 线程历史。
- Codex 也不能声称已读取 ZCode 会话历史。
- 双方互相知道对方做了什么，必须依赖落盘交接包和仓库证据。

## 3. ZCode 可临时承接的角色

ZCode 只在用户明确指定时，可临时承接以下执行角色：

### 3.1 当前版本开发窗口 / 开发执行体

允许：

- 执行已经取得 `GUARD_PASS` 且已经由用户审批的正式任务档案。
- 执行已经取得 `GUARD_PASS` 且已经由用户审批的正式修复任务档案。
- 按任务档案允许范围修改代码。
- 完工后落盘开发回执和交接总览。

禁止：

- 修改 `AGENTS.md`。
- 修改 `Docs/LOCKED/*`。
- 修改任务档案禁止范围。
- 自行扩大功能范围。
- 自行生成任务档案。
- 自行生成失败归因。
- 自行 commit、tag、push、切分支或回滚；这些操作必须取得用户单独明确授权。

### 3.2 外部 QA 执行体

允许：

- 按 codex task writer 正式任务档案中的 QA 标准验收。
- 产出 QA 报告并落盘。
- 指出通过 / 不通过、失败现象、复现路径、风险点和用户手测建议。

禁止：

- 修改代码。
- 发现 bug 后直接修复。
- 自行开修复任务。
- 自行给正式失败归因。
- 自行替代已注册 QA reviewer 的全部固定角色职责。

## 4. Bug 修复硬规则

当 ZCode 作为外部 QA 执行体发现不通过时，只能：

```text
ZCode 写 QA 报告落盘
→ 停止
→ 由 Codex tech architect 给正式失败归因
→ 由 Codex codex task writer 给正式修复任务档案
→ 由 Codex guard-agents 审查并给 GUARD_PASS
→ 用户审批修复任务包
→ 用户再决定由 Codex 或 ZCode 执行修复
```

ZCode 不得发现 bug 后自行修复，不得绕过 tech architect 失败归因，不得绕过 codex task writer 修复任务档案。

## 5. Handoff Package / 交接包

交接包是 Codex 与 ZCode 互相感知对方工作的唯一物质载体。

交接包至少包含以下类型之一：

- 开发回执
- QA 报告
- 交接总览

交接包必须包含以下字段：

- 来源系统：`ZCode` / `Codex`
- 承接角色：开发执行体 / QA 执行体
- 任务档案路径
- 是否已有 `GUARD_PASS`
- 是否已有用户审批
- 修改文件清单
- 测试结果
- 已知风险
- 是否触碰禁止范围
- `git diff` / `commit hash`，二选一；commit 非必需
- 下一步应交给谁

推荐落盘路径：

```text
Docs/V0.3/Handoffs/<包体名>_DEV_RECEIPT_FROM_ZCODE.md
Docs/V0.3/Handoffs/<包体名>_QA_REPORT_FROM_ZCODE.md
Docs/V0.3/Handoffs/<包体名>_HANDOFF_SUMMARY_FROM_ZCODE.md
```

缺少交接包时，Codex 不得声称已经了解 ZCode 做了什么、怎么做的、做得怎样。

## 6. Commit 规则

若用户单独授权 ZCode commit，commit message 必须带 `[ZCode]` 前缀。

未获用户单独明确授权时，ZCode 不得 commit。

交接包中的 `commit hash` 不是必需项；未 commit 时必须提供 `git diff` 证据或修改文件清单。

## 7. 开工纪律

ZCode 每次开工前必须读取：

- 当前 Codex 正式任务档案或正式修复任务档案
- `F:\Porject\TalismanBagBrawl\AGENTS.md`
- `F:\Porject\TalismanBagBrawl\Docs\LOCKED\*`

ZCode 每次完工必须写交接包。没有交接包，视为跨系统交接不完整。

## 8. 与现有流水线的关系

`SINGLE_LANE_TASK_INTAKE` 不变。

新任务仍必须由：

```text
producer tech pm
→ tech architect
→ codex task writer
→ guard-agents
→ 用户审批
→ 开发窗口 / 跨系统执行体
```

`PASS_SYNC_BROADCAST` 仍只用于用户验收通过后的状态同步，不得用于 ZCode 接任务或新任务下发。

ZCode 只能执行已经形成正式任务档案并完成 Guard / 用户审批的任务。

## 9. 试运行范围

V0.3 阶段作为跨系统协作试运行。

ZCode 只在用户明确指定时接手：

1. 已审批任务包的开发执行。
2. 已完成开发后的外部 QA。
3. 已审批修复包的修复执行。

不放权给 ZCode 管 QA + 修复全链路。

## 10. 回滚条款

若跨系统机制导致流水线紊乱、边界误判或 V0.2 稳定基线风险，Codex 原始记忆治理窗口可在用户审批后撤销本类别。

撤销后，ZCode 退回纯只读 / 草案身份。

撤销本类别不影响 V0.2 基线与已落盘代码本身；已落盘代码仍按普通仓库改动处理。
