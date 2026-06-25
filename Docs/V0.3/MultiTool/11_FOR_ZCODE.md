# 给 ZCode 的多工具协作入场说明

日期：2026-06-25

接收方：ZCode

本文用于让 ZCode 快速理解《符箓背包》当前多工具协作方式。它不是授权文件，不替代 Codex 官方任务档案。

## 0. 先记住一句话

Codex 仍是本项目的主要开发工具、总控、最终验收与版本收口方。

ZCode 之前提出的 `Cross-System Executor / 跨系统执行体` 方案已经被用户要求暂时禁用。

当前状态：

```text
Cross-System Executor = DISABLED。
ZCode 尚未被正式分配包体。
ZCode 现在只能作为外部参考 / 草案来源，不得正式开发、QA 或修复。
```

## 1. 我们刚刚创建了什么工作记忆

多工具同步文件已经创建在：

```text
F:\Porject\TalismanBagBrawl\Docs\V0.3\MultiTool\
```

你最重要的入口文件是：

```text
Docs/V0.3/MultiTool/00_SYNC_HUB.md
```

它的用途是“当前状态快照”，不是授权开发。你不需要每次读取 Codex、Claude、ZCode 的完整历史，只读 `00_SYNC_HUB.md` 中给你的摘要、状态卡和链接即可。

配套文件：

```text
Docs/V0.3/MultiTool/01_EXTERNAL_AI_QUICKSTART.md
Docs/V0.3/MultiTool/02_TEMPLATES.md
Docs/V0.3/MultiTool/Handoffs/
Docs/V0.3/MultiTool/Receipts/
Docs/V0.3/MultiTool/QA/
Docs/V0.3/MultiTool/WorktreeLocks/
```

## 2. 你的最小读取顺序

每次开工前按这个顺序读：

```text
1. F:\Porject\TalismanBagBrawl\AGENTS.md
2. F:\Porject\TalismanBagBrawl\Docs\LOCKED\*
3. F:\Porject\TalismanBagBrawl\Docs\V0.3\MultiTool\00_SYNC_HUB.md
4. Codex 明确分配给你的正式任务档案
5. 00_SYNC_HUB.md 中明确链接给你的交接包
```

不要默认读取 Claude Code 的完整日志。若确有依赖，只读 `00_SYNC_HUB.md` 指向的摘要或交接包。

## 3. 当前版本计划公告板

以下是从当前落盘文档可确认的 V0.3 状态。若与 `00_SYNC_HUB.md` 或 `Docs/LOCKED/*` 冲突，以后者为准。

### 当前大版本方向

```text
V0.3 = 围绕照灯小铺首页、空间热点、入口、二级页底栏与 ComingSoon / 锁定态收口。
当前锁定版本方向：V0.3-MainHomeScene01-Retry。
```

### 当前包体池

| 包体 | 任务档案 | 风险 | 当前状态 | 是否可由 ZCode 开工 |
| --- | --- | --- | --- | --- |
| `V0.3-NavigationFlow01` | `Docs/V0.3/V0.3-NavigationFlow01_Task.md` | 黄灯 | `WAITING_USER_APPROVAL`，已有 `GUARD_PASS` | 否，需用户/Codex 明确分配 |
| `V0.3-NavigationFlow01-FixPersistence01` | `Docs/V0.3/V0.3-NavigationFlow01-FixPersistence01_Task.md` | 红灯 | `WAITING_USER_APPROVAL`，已有 `GUARD_PASS`，首次审批只允许 Stage A Survey | 否，需用户/Codex 明确分配，且红灯限制更强 |

### 当前优先理解

- `NavigationFlow01`：V03 本地导航壳、Refine/Explore/More 页壳、底栏、Home 返回、Trial 单向进入 V02。
- `FixPersistence01`：修复从 V03 进 V02 后跨 Stop/Play 的 round / runtime inventory / grid layout 恢复一致性问题。
- 这两个包体当前都不是给 ZCode 的授权工单，只是公告板事实。

## 4. 如果未来 Codex 分配你一个包体，你下一步该做什么

收到明确分配后：

```text
1. 确认 Cross-System Executor 或新的多工具执行协议已由用户/Codex 重新启用。
2. 在 00_SYNC_HUB.md 中确认包体状态和文件锁。
3. 建议使用独立 worktree：
   F:\Porject\TalismanBagBrawl_zcode
4. 在 Docs/V0.3/MultiTool/WorktreeLocks/ 写你的锁文件。
5. 只读正式任务档案，不要自己改任务范围。
6. 先做本工具内部 Tech Architect 拆解。
7. 再写本工具内部 Task Writer 执行计划。
8. 再开发。
9. 再做本工具内部 QA。
10. 完工后写 Receipts / QA / Handoffs。
11. 更新 00_SYNC_HUB.md 中 ZCode 状态卡和交接链接。
12. 等 Codex 最终收口。
```

如果没有第 1 步的重新启用，你不能正式承接任务。

## 5. ZCode 内部也要模拟 Codex 的回转流程

你可以在 ZCode 内部模拟以下角色，但这些只是你自己的工作步骤，不是项目官方固定角色：

### 5.1 Tool Tech Architect

目标：把 Codex 正式任务档案拆成安全执行步骤。

必须输出：

```text
【ZCode Tool Architect 拆解】
1. 本包目标：
2. 允许修改：
3. 禁止修改：
4. 风险等级：
5. 需要先 Survey 的文件：
6. 执行步骤：
7. QA 与回滚：
```

注意：你不能用这个拆解修改 Codex 官方任务范围。

### 5.2 Tool Task Writer

目标：把拆解写成你自己的执行清单。

建议落盘到：

```text
Docs/V0.3/MultiTool/Receipts/<包体名>_LOCAL_PLAN_FROM_ZCODE.md
```

必须写清：

- 当前包体
- 使用的官方任务档案
- 计划修改文件
- 不碰哪些文件
- Survey 步骤
- 开发步骤
- QA 步骤
- 交接路径

### 5.3 Tool Developer

只执行 Tool Task Writer 清单里、且不超过 Codex 官方任务档案允许范围的内容。

不得：

- 修改 `AGENTS.md`
- 修改 `Docs/LOCKED/*`
- 改任务档案禁止文件
- 顺手修无关 bug
- 用测试制造通过
- 自动 commit / tag / push
- 发现 QA 不通过后直接自由修复

### 5.4 Tool QA

按官方任务档案 QA 标准验收。

QA 报告写到：

```text
Docs/V0.3/MultiTool/QA/<包体名>_QA_REPORT_FROM_ZCODE.md
```

QA 报告必须明确：

- 通过 / 不通过 / 阻塞
- 真实用户路径
- 自动测试是否独立
- 是否触碰禁止范围
- 用户需要手测什么
- 是否建议回 Codex Tech Architect

### 5.5 Tool RepoOps / Evidence

你可以记录 diff 证据，但不能自动 commit。

若用户单独授权 commit，commit message 必须带：

```text
[ZCode]
```

未授权 commit 时，用修改文件清单和 diff 摘要作为证据。

## 6. 完成后必须同步到公共文档

完成开发或 QA 后，至少写三份材料：

```text
Docs/V0.3/MultiTool/Receipts/<包体名>_DEV_RECEIPT_FROM_ZCODE.md
Docs/V0.3/MultiTool/QA/<包体名>_QA_REPORT_FROM_ZCODE.md
Docs/V0.3/MultiTool/Handoffs/<包体名>_HANDOFF_SUMMARY_FROM_ZCODE.md
```

然后更新：

```text
Docs/V0.3/MultiTool/00_SYNC_HUB.md
```

只更新：

- ZCode 状态卡
- 你负责包体的交接包链接
- 你持有的文件锁状态

不要删除或改写 Codex / Claude Code 的状态卡。

## 7. 如果 QA 不通过

如果你自己的 QA 不通过：

```text
1. 写 QA 报告。
2. 写清失败原因和复现路径。
3. 停止。
4. 不要直接扩范围修。
5. 更新 00_SYNC_HUB.md 状态为 RETURNED 或 BLOCKED。
6. 交回 Codex，由 Codex 决定是否进入 Tech Architect 失败归因和 Task Writer 修复工单。
```

尤其注意：Bug 修复不能由 ZCode 自己从 QA 报告直接进入开发，除非 Codex 已重新走完正式失败归因、修复任务档案、Guard、用户审批，并再次明确分配给 ZCode。

## 8. 最终提醒

你可以帮助节省 Codex 开发流量，但不能污染 Codex 已建立的官方流水线。

当前你不是正式执行体。等用户和 Codex 重新启用多工具执行协议后，才可按本文件进入包体执行。

