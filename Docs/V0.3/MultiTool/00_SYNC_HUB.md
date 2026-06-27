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
| 当前正式主线包 | `V0.3-BattlePrepareInteraction01` |
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
4. V0.3-BattlePrepareInteraction01：QA_FAIL / GUARD_PASS_FIX01_CONTINUE
5. V0.3-PrepareTutorial01：QUEUED_HIGH_RISK
6. V0.3-BossGuideResult01：QUEUED_HIGH_RISK
7. V0.3-ForgeFirstUpgradeGuide01：QUEUED_HIGH_RISK
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
当前正式主线下一包是 V0.3-BattlePrepareInteraction01；首轮手测不通过，Guard 放行 Fix01 最小修复，不重做整包。
```

## 5. 工具状态卡

### Codex

```text
当前职责：总控 / 记忆治理 / Package Queue 维护 / 最终收口
当前包体：V0.3-BattlePrepareInteraction01
正在做：维护 ProductFlow01 正式队列；BootEntryFlow01 已通过；MainHomeScene01-Retry ProductFlow 复查层级修复已手测通过；BottomNavAndHomeHotspot01 已手测通过并请求 RepoOps 记录；BattlePrepareInteraction01 首轮手测不通过，Guard 放行 Fix01 最小修复
不会做：把 ProductFlow01 当成单个大包开发
```

### Claude Code

```text
当前职责：READY_IDLE / 等待分配
当前包体：未分配
当前 worktree：F:\Porject\TalismanBagBrawl_claude
不会做：未授权写入 Unity 工程；未授权修改 LOCKED / AGENTS
下一步：读取 ProductFlow01 路线与 Package Queue；当前无外部工具分配；BattlePrepareInteraction01 仅允许按 GUARD_PASS_FIX01_CONTINUE 修复
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
