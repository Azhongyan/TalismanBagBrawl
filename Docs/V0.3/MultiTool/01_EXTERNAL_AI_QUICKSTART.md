# External AI Quickstart / 外部 AI 快速入场

本文件给 Claude Code、ZCode 或其他外部 AI 工具使用。它不是授权文件，只是入场提醒。

## 1. 你必须先知道

Codex 是《符箓背包》的主要开发与最终收口工具。

外部 AI 工具可以在未来被用户指定为某个包体的执行者，但不能自动获得：

- 版本管理权
- 记忆治理权
- Guard 权限
- Tech Architect 权限
- Codex Task Writer 权限
- RepoOps 主责

当前 `Cross-System Executor / 跨系统执行体` 为 `DISABLED`。未重新审批前，外部工具不得正式承接开发、QA 或修复执行。

## 2. 每次开工最小读取

```text
F:\Porject\TalismanBagBrawl\AGENTS.md
F:\Porject\TalismanBagBrawl\Docs\LOCKED\*
F:\Porject\TalismanBagBrawl\Docs\V0.3\MultiTool\00_SYNC_HUB.md
Codex 分配给你的正式任务档案
```

不要一上来阅读所有工具的完整日志。本项目以 `00_SYNC_HUB.md` 作为当前状态快照。

## 3. 工作原则

- 只执行 Codex 官方任务档案。
- 只改任务档案允许范围内的文件。
- 不改 `AGENTS.md`。
- 不改 `Docs/LOCKED/*`。
- 不碰其他工具在 `00_SYNC_HUB.md` 中锁住的文件。
- 不把自己的内部分析当成 Codex 官方工单。
- 不通过测试制造成功状态。
- 不自动 commit / tag / push / 回滚。

## 4. 推荐 worktree

```text
Codex 主 worktree：
F:\Porject\TalismanBagBrawl

Claude Code 推荐 worktree：
F:\Porject\TalismanBagBrawl_claude

ZCode 推荐 worktree：
F:\Porject\TalismanBagBrawl_zcode
```

不要多个工具同时写同一个 Unity 工程目录。

## 5. 完工交接

未来若外部工具被正式启用，每次完工必须写交接材料到：

```text
Docs/V0.3/MultiTool/Handoffs/
Docs/V0.3/MultiTool/Receipts/
Docs/V0.3/MultiTool/QA/
```

最少说明：

- 做了什么
- 改了哪些文件
- 怎么验证
- 剩余风险
- 是否触碰禁止范围
- git diff 或用户授权后的 commit hash
- 下一步应交给谁

没有交接材料，Codex 不应视为“已经知道你做了什么”。

