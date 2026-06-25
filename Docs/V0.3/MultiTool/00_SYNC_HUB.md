# MultiTool Sync Hub / 多工具协同同步中枢

状态：`SCAFFOLD / 协同脚手架`

用途：给 Codex、Claude Code、ZCode 等外部工具提供一个“当前状态快照”。它用于减少重复阅读历史，不用于授权开发。

## 0. 权威边界

本文件不是 LOCKED 文件，不改变任何既有 Codex 流水线。

若本文件与以下内容冲突，以以下内容为准：

1. `F:\Porject\TalismanBagBrawl\AGENTS.md`
2. `F:\Porject\TalismanBagBrawl\Docs\LOCKED\*`
3. Codex Task Writer 的正式任务档案
4. Guard 的 `GUARD_PASS / GUARD_RETURN`
5. 用户明确审批

当前 `Cross-System Executor / 跨系统执行体` 仍为 `DISABLED`。本文件只是在准备多工具协同看板，不授权 Claude Code 或 ZCode 正式承接任务。

## 1. 快速入场规则

每个工具开工前最少读取：

```text
1. AGENTS.md
2. Docs/LOCKED/*
3. 本文件：Docs/V0.3/MultiTool/00_SYNC_HUB.md
4. 自己被分配的正式任务档案
5. 本文件中明确链接给自己的交接包
```

每个工具不需要默认阅读其他工具的完整对话、完整日志或全部交接包。只读本文件中的摘要和必要链接。

如果本文件没有记录某个事实，不得猜测；应回到 Codex 总控或用户确认。

## 2. 官方接受状态 / Codex Maintained

本区只由 Codex 总控 / RepoOps 根据用户验收结果更新。

| 字段 | 当前值 |
| --- | --- |
| 当前大版本 | `V0.3` |
| 当前版本方向 | `V0.3-MainHomeScene01-Retry` |
| 官方可用基线 commit | `未初始化，需 RepoOps / Codex 后续写入` |
| 已被 Codex 最终接受的最新包体 | `未初始化，需 Codex 最终验收后写入` |
| 当前可作为多工具开发基线 | `未初始化，外部工具不得据此开工` |
| Cross-System Executor 状态 | `DISABLED` |
| 最终收口方 | `Codex` |

## 3. 当前版本计划公告板 / Version Plan Board

本区只记录当前已落盘、可被快速理解的版本计划事实。它不授权开发。

当前 V0.3 方向：

```text
围绕照灯小铺首页、空间热点、入口、二级页底栏、Trial 单向入口与 ComingSoon / 锁定态收口。
保护 V0.2 稳定基线：主流程、战斗、棋盘、伤害数字、道具、阵盘、Boss、奖励、数值、存档结构。
```

当前已知包体：

1. `V0.3-NavigationFlow01`
   - 类型：黄灯正式任务包
   - 目标：V03 本地导航壳、Refine / Explore / More 页壳、二级页底栏、Home 返回、Trial 单向进入 V02
   - 当前落盘状态：`WAITING_USER_APPROVAL`
   - Guard：已有 `GUARD_PASS`
   - 外部工具状态：未分配，不得开工

2. `V0.3-NavigationFlow01-FixPersistence01`
   - 类型：红灯正式修复包
   - 目标：修复从 V03 进入 V02 后跨 Stop/Play 的 round / runtime inventory / grid layout 恢复一致性
   - 当前落盘状态：`WAITING_USER_APPROVAL`
   - Guard：已有 `GUARD_PASS`
   - 特别限制：首次用户审批只可授权 Stage A 只读 Survey；`SaveData / MainTrialProgressData` 不得被首次审批预授权
   - 外部工具状态：未分配，不得开工

## 4. 包体看板 / Package Board

状态枚举：

```text
OPEN
CLAIMED
IN_PROGRESS
DEV_DONE
TOOL_QA_DONE
HANDOFF_TO_CODEX
CODEX_ACCEPTED
RETURNED
BLOCKED
```

| 包体 | 官方任务档案 | 负责人 | worktree / branch | 状态 | 文件锁 | 交接包 | Codex 最终结论 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `V0.3-NavigationFlow01` | `Docs/V0.3/V0.3-NavigationFlow01_Task.md` | `未分配` | `未分配` | `OPEN / WAITING_USER_APPROVAL` | `未授权写入` | `无` | `未收口` |
| `V0.3-NavigationFlow01-FixPersistence01` | `Docs/V0.3/V0.3-NavigationFlow01-FixPersistence01_Task.md` | `未分配` | `未分配` | `OPEN / WAITING_USER_APPROVAL / RED` | `未授权写入；Stage A 只读 Survey 待审批` | `无` | `未收口` |

## 5. 文件锁 / Worktree Locks

原则：

- 推荐每个工具使用独立 worktree，不推荐多个工具同时写同一个 Unity 工程目录。
- Scene、Prefab、ProjectSettings、Save、RunFlow、Formation、Reward、Boss、数值表等高风险文件必须显式锁定。
- 未在本区或正式任务档案中写明的文件，不代表可以随意修改；仍受 `Docs/LOCKED/*` 限制。

| 工具 | 当前包体 | worktree / branch | 允许写入范围 | 禁止触碰范围 | 锁状态 |
| --- | --- | --- | --- | --- | --- |
| Codex | `TBD` | `F:\Porject\TalismanBagBrawl` | `TBD` | `Docs/LOCKED/*` 等 | `IDLE` |
| Claude Code | `未分配` | `建议：F:\Porject\TalismanBagBrawl_claude` | `未授权` | `全工程写入禁止，除非用户分配` | `IDLE` |
| ZCode | `未分配` | `建议：F:\Porject\TalismanBagBrawl_zcode` | `未授权` | `全工程写入禁止，除非用户分配` | `IDLE` |

## 6. 工具状态卡

### Codex

```text
当前职责：总控 / 官方任务生成 / Guard / 最终收口
当前包体：维护 V0.3 包体池与多工具同步脚手架
正在做：维护官方流水线、最终验收与多工具同步中枢
不会做：绕过用户审批；让外部工具直接获得治理权
下一步：等待用户决定是否启用多工具包体执行；若启用，仍由 Codex 分配包体并最终收口
```

### Claude Code

```text
当前职责：未启用
当前包体：未分配
当前 worktree：建议 F:\Porject\TalismanBagBrawl_claude
正在做：无
不会做：未授权写入 Unity 工程；未授权修改 LOCKED / AGENTS
下一步：先读 Docs/V0.3/MultiTool/10_FOR_CLAUDE_CODE.md；等 Codex 正式分配包体后，只读本文件 + 正式任务档案
```

### ZCode

```text
当前职责：未启用；Cross-System Executor 仍为 DISABLED
当前包体：未分配
当前 worktree：建议 F:\Porject\TalismanBagBrawl_zcode
正在做：无
不会做：未授权正式开发 / QA / 修复；未授权修改 LOCKED / AGENTS
下一步：先读 Docs/V0.3/MultiTool/11_FOR_ZCODE.md；等用户和 Codex 重新定义外部工具协同规则
```

## 7. 最近完成摘要 / Compact History

只记录“给其他工具快速理解当前状态”所需的压缩摘要，不记录完整过程。

| 时间 | 工具 | 包体 | 摘要 | 详情链接 |
| --- | --- | --- | --- | --- |
| `未初始化` | `Codex` | `TBD` | `多工具同步中枢文件已创建，未启用外部正式执行权限。` | `本文件` |
| `2026-06-25` | `Codex` | `MultiTool` | `已创建 Claude Code / ZCode 定向入场文档，说明同步中枢用法、当前包体公告板与工具内部回转流程。` | `Docs/V0.3/MultiTool/10_FOR_CLAUDE_CODE.md` / `Docs/V0.3/MultiTool/11_FOR_ZCODE.md` |

## 8. 等待 Codex 收口的交接包

| 来源工具 | 包体 | 类型 | 路径 | 当前处理状态 |
| --- | --- | --- | --- | --- |
| `无` | `无` | `无` | `无` | `无待处理` |

## 9. 下次启动提示

### 如果你是 Claude Code

```text
1. 先读 AGENTS.md / Docs/LOCKED/*
2. 读本文件
3. 读 Docs/V0.3/MultiTool/10_FOR_CLAUDE_CODE.md
4. 只看 Claude Code 状态卡和分配给你的包体
5. 不要读取 ZCode 完整日志，除非本文件明确链接
6. 未被 Codex 正式分配前，不要写工程
```

### 如果你是 ZCode

```text
1. 先读 AGENTS.md / Docs/LOCKED/*
2. 读本文件
3. 读 Docs/V0.3/MultiTool/11_FOR_ZCODE.md
4. 注意 Cross-System Executor 当前为 DISABLED
5. 未重新获得用户审批启用前，不要正式开发 / QA / 修复
6. 不要读取 Claude Code 完整日志，除非本文件明确链接
```

### 如果你是 Codex

```text
1. 读取所有状态卡
2. 根据官方流水线分配包体
3. 审查外部工具交接包
4. 只由 Codex 做最终版本收口
```

## 10. 更新规则

- Codex 可更新全文件。
- Claude Code 未来若被启用，只能更新自己的状态卡、自己包体的交接链接和自己持有的文件锁。
- ZCode 未来若被启用，只能更新自己的状态卡、自己包体的交接链接和自己持有的文件锁。
- 任何工具不得删除其他工具状态卡。
- 任何工具不得通过修改本文件获得超出正式任务档案的权限。
