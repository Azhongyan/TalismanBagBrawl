# CODEX VERSION PIPELINE LOCK

本文件锁定《符箓背包》在 Codex 内的长期版本生产工作流。

它不是开发任务书，不授权任何窗口直接开发；它只定义各角色如何流转、何时进入开发、何时回流修复、以及 Guard-agents 如何做边界收口。

角色窗口是否已注册、是否允许自动发送或读取，见 `Docs/LOCKED/CODEX_ROLE_WINDOW_REGISTRY.md`。
交付形态、用户可见性、测试独立性与放行证据，见 `Docs/LOCKED/DELIVERY_ACCEPTANCE_GATE.md`。

## 0. 当前默认模式：Light Guard + RepoOps

从用户最新决策起，Codex 侧边栏多角色流水线默认停用，不再每包自动流转。

默认工作方式：

```text
用户在 GPT / 外部策划流程拆包
→ Guard 做边界、红线、Package Queue 与记忆收口
→ 用户创建当前任务窗口
→ 任务窗口按 Guard 收口后的当前包 / assignment 开发
→ 任务窗口自行完成基础 QA、自动验证、用户手测清单与结果汇报
→ 用户手测通过 / 不通过
→ 任务窗口只同步 Guard + RepoOps
→ Guard 更新记忆与 Package Queue
→ RepoOps 记录版本状态
```

默认不再自动调用：

```text
producer tech pm
tech architect
codex task writer
QA reviewer
PASS_SYNC_BROADCAST 多角色广播
```

以上角色窗口不删除、不清空；仅锁为“停用自动流转”。以后遇到难开发任务、边界极不清任务、连续失败、红线 / 架构 / 存档 / Boss / 奖励 / 数值 / 主流程风险时，用户可以明确说“这个任务走重流程”，再临时恢复 Task Writer / QA / 其他角色协助。

每包完成后的默认同步协议改为：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS
```

同步对象只包括：

```text
Guard / 记忆治理窗口
RepoOps / 版本管理
```

同步内容至少包含：

- 包体名 / 小版本名
- 开发窗口名称或来源
- 本包完成状态：开发完成 / 用户手测通过 / 用户手测不通过 / 阻塞
- 修改文件清单
- 自动验证结果
- 用户手测原文或失败原因
- 已知风险
- 是否触碰红线或越界
- 下一步建议

RepoOps 只负责版本状态记录；没有用户单独明确授权，不得自动 commit、tag、push、切分支或回滚。

## 1. 长期角色分工

### 用户 / 产品源头

- 用户先在 GPT 内与策划完成产品总体 rundown、当前大版本方向或当前版本功能构想。
- 用户负责最终确认版本方向、任务包是否可下发、QA 是否通过。
- 用户的“通过”或“不通过 + 原因”是版本任务进入下一状态的关键触发。

### producer tech pm

- 自动流转状态：`DISABLED / 旧线程发送失败，线程归档文件缺失`。
- 版本方向、长线规划、产品 rundown 与优先级讨论回到用户的 GPT / 外部策划流程。
- Codex 不再自动发送 producer tech pm 旧线程，也不要求该线程返回正式产物。
- 若用户将 GPT / 外部策划后的版本计划落盘到 ROADMAP / CURRENT / Package Queue，Codex 直接消费这些文件。
- 一旦版本路线已经进入 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md`，日常小包执行不再每包回到 producer tech pm。
- 仅当用户改变版本方向、改变优先级、出现队列外新需求或需要重排大版本节奏时，才停止并等待用户提供新的 GPT / 外部策划结论。

### tech architect

- 自动流转状态：`DISABLED / 旧线程发送失败，线程归档文件缺失`。
- 技术拆包、架构判断、异常失败归因回到用户的 GPT / 外部技术讨论流程。
- Codex 不再自动发送 tech architect 旧线程，也不要求该线程返回正式产物。
- 若用户将 GPT / 外部技术结论落盘到 Package Queue、任务档案输入或失败归因摘要，Codex 直接消费这些文件。
- 一旦包体顺序和技术路线已经进入 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md`，日常小包执行不再每包回到 tech architect。
- 仅当当前包边界不清、Guard 无法从队列收口出安全 assignment、QA 失败涉及架构 / 状态机 / 存档 / Boss / 奖励 / 数值 / 主流程、同一包连续修复失败、或任务需要触碰硬红线时，才停止并等待用户提供 GPT / 外部技术归因或明确替代输入。

### codex task writer

- 自动流转状态：`DISABLED / 日常停用`。
- 普通包不再要求 codex task writer 写正式任务档案。
- 当前任务窗口可直接依据用户在 GPT / 外部策划后的拆包、Package Queue、Guard 收口和用户明确 assignment 开发。
- 仅当用户明确要求“难任务走 Task Writer”或 Guard 判定必须补正式任务档案时，才临时启用。

### guard-agents

- 本长期置顶常驻窗口认领 guard-agents 职责。
- 只负责守边界、红线和版本收口，不直接开发。
- 普通日常小包由 Guard 做轻量收口：确认包名、范围、红线、当前 Package Queue 指针和任务窗口可执行边界。
- Guard 收口不等于开发、不替代用户手测。
- Guard 负责接收任务窗口完工后的 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS`，并更新记忆与 Package Queue。
- 仅在任务越出当前版本范围、修改 `AGENTS.md` / `Docs/LOCKED/*` / Package Queue、执行窗口想触碰红线文件、外部工具协同可能污染主流程、QA 不通过原因涉及边界 / 架构 / 红线时，才要求明确 `GUARD_PASS` / `GUARD_RETURN`。
- 检查异常任务包是否越过当前版本范围。
- 检查异常修复任务是否借机扩功能。
- 保护 V0.2 稳定基线。
- 维护当前版本技术收口摘要。
- 对触发 Guard 的任务档案和修复任务档案给出明确 `GUARD_PASS` 或 `GUARD_RETURN`，沉默不视为通过。
- 检查自动测试是否替被测功能主动制造成功状态。
- 锁功能边界、禁止项、版本范围和基线安全；不锁实现细节。

### 当前版本开发窗口

- 由用户在确认任务包后手动创建。
- 开发窗口必须先读取 `AGENTS.md` 与 `Docs/LOCKED/*`。
- 开发窗口必须读取当前版本的 Package Queue，例如 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md`。
- 开发窗口只执行用户指定的小版本名 / 包体编号。
- 开发窗口默认依据 GPT / 外部策划拆包、Package Queue、Guard 收口和用户明确 assignment 执行；不再要求 codex task writer 任务档案。
- 若用户明确直安排外部协同实测或临时执行说明，开发窗口 / 外部工具只能按对应 assignment 文件执行，不得自行扩权。
- 开发窗口不得自行扩大版本范围，不得把 QA 不通过解释为自由重构授权。
- 开发完成后，开发窗口必须自行汇报自动验证、手测清单、修改文件、风险与通过 / 不通过建议。
- 用户手测通过后，开发窗口只同步 Guard + RepoOps，不再同步 Task Writer / QA Reviewer / PM / Architect。

### QA reviewer

- 自动流转状态：`DISABLED / 日常停用`。
- 普通包不再自动交给 QA reviewer。
- 当前任务窗口自行给出 QA 汇报和用户手测清单。
- 仅当用户明确要求“独立 QA”或 Guard 判定必须独立验收时，才临时启用。

### Cross-System Executor / 跨系统执行体

- 当前状态：`DISABLED / 暂停生效`。
- 用户已要求先禁用 Cross-System Executor 协议但不删除文件。
- 未重新获得用户明确审批启用前，ZCode 不得作为正式跨系统执行体承接开发、QA 或修复任务。
- 不是固定角色窗口，不获得治理权。
- 当前试运行对象：`ZCode`。
- 只在用户明确指定时，可临时承接已审批任务包的开发执行、外部 QA、或已审批修复包的修复执行。
- 不能承接 producer tech pm、tech architect、codex task writer、guard-agents 或 RepoOps 主责。
- 若未来重新启用，完工后必须按 `Docs/LOCKED/CROSS_SYSTEM_EXECUTOR_PROTOCOL.md` 写交接包；没有交接包时，Codex 不得声称已经了解 ZCode 做了什么、怎么做的、做得怎样。当前协议为 `DISABLED`，本段不授权 ZCode 承接任务。

### RepoOps

- 接收用户确认“通过 / QA 通过 / 手测通过”后的版本状态记录请求。
- 与 Guard 一起作为默认完工同步对象。
- 负责按大版本 / 小版本节奏记录版本状态、变更摘要、任务包来源、QA 结论、用户手测结论、风险点、回滚点和下一步建议。
- 小版本小记录：每个任务包通过后记录一次。
- 大版本大记录：大版本阶段收口或用户明确要求时记录一次。
- RepoOps 默认只做版本记录与归档规划；未获用户单独明确授权时，不自动执行 commit、tag、push、分支操作或回滚。

## 2. Package Queue Light Path / 日常轻量包体快线

版本规划期只跑一次：

```text
GPT / 策划产品 rundown
→ 用户在 GPT / 外部策划流程中确认版本计划与技术拆包
→ 形成当前版本 Package Queue
```

当前 V0.3 的 Package Queue 固定记录在：

```text
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
```

进入 Package Queue 的普通包，日常执行默认走快线：

```text
Package Queue 指向当前包
→ Guard 轻量收口当前包边界 / 红线 / 可执行 assignment
→ 开发窗口 / 指定执行体按 assignment 执行
→ 开发窗口自行完成基础 QA、自动验证和用户手测清单
→ 用户手动验证并回复“通过”或“不通过 + 原因”
→ 通过后开发窗口发送 TASK_STATUS_SYNC_TO_GUARD_REPOOPS
→ Guard 更新记忆与 Package Queue
→ RepoOps 做小版本状态记录
→ Package Queue 指向下一包
```

只有用户确认“通过”后，才允许进入下一任务包。

用户确认“通过 / QA 通过 / 手测通过”后，任务窗口只需要同步 Guard + RepoOps。RepoOps 记录完成不等于授权 commit / tag / push。

默认同步协议为 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS`，不再使用 `PASS_SYNC_BROADCAST` 多角色广播作为硬门。

`TASK_STATUS_SYNC_TO_GUARD_REPOOPS` 至少包含：

- 包体名 / 小版本名
- 开发窗口名称或来源
- 开发摘要
- 修改文件清单
- 自动验证结果
- 开发窗口自测 / QA 结论
- 用户手测通过原文或等价摘录
- RepoOps 记录状态或待记录事项
- 本包最终结论
- 下一步建议

`TASK_STATUS_SYNC_TO_GUARD_REPOOPS` 只表示“已同步给 Guard + RepoOps 或已生成手动转发包”，不表示授权 commit / tag / push。

`PASS_SYNC_BROADCAST` 默认停用；不得再把它作为新任务下发、任务生成、开发授权或进入下一包的硬门。

普通已排队包不再要求每包 `GUARD_PASS`。只有触发 Guard 例外条件的任务包，才必须取得 `GUARD_PASS` 后继续。

## 2.1 Queue Intake / 队列取包规则

新任务开始先看 Package Queue，而不是默认重跑 producer tech pm / tech architect。

普通已排队包入口如下：

```text
用户创建或指定当前开发窗口
→ 开发窗口读取 AGENTS.md / Docs/LOCKED/* / 当前版本 Package Queue
→ 确认当前包与上一包验收状态
→ Guard 基于 Package Queue 和用户 GPT / 外部策划输入收口当前包 assignment
→ 用户确认执行
→ 开发窗口只按当前包任务档案或 assignment 执行
→ 执行窗口改当前任务栏 / 窗口名称并进入开发
```

只有以下情况才停止 Light Path，并等待用户提供 GPT / 外部策划或技术归因输入：

```text
Package Queue 不存在
当前任务不在 Package Queue
用户改变版本方向 / 优先级
当前包边界不清
Guard 无法从 Package Queue 收口出可执行 assignment
任务触碰硬红线
QA 失败涉及架构 / 状态机 / 存档 / Boss / 奖励 / 数值 / 主流程
```

旧版完整 `SINGLE_LANE_TASK_INTAKE` 不再自动发送 producer tech pm / tech architect 旧线程。当前替代流程为：

```text
用户在 GPT / 外部策划流程中确认版本方向、拆包或失败归因
→ 将结论落盘到 ROADMAP / CURRENT / Package Queue，或作为明确输入给 Codex
→ Guard 收口边界
→ 必要时临时启用 codex task writer / QA reviewer
→ 用户确认
→ 开发窗口
```

新任务入口的硬规则：

- producer tech pm 旧线程停用自动流转，不再接收自动下发或通过后广播。
- tech architect 旧线程停用自动流转，不再接收自动下发、失败回流或通过后广播。
- codex task writer 默认停用；不再是日常开发窗口的唯一正式工单来源。
- guard-agents 只在越界、红线、改规则或异常失败时审边界，不下发开发内容。
- 开发窗口不得把 producer tech pm 或 tech architect 的产物当作开发任务档案。
- 开发窗口不得合并多角色并发回复后自行形成工单。
- Cross-System Executor 当前为 `DISABLED`。若未来重新启用，ZCode 作为跨系统执行体承接开发也必须遵守同一条 `SINGLE_LANE_TASK_INTAKE`，不得从 producer tech pm 或 tech architect 直接接工单。
- 如果开发窗口同时收到多个旧角色并发内容，必须停线，只认 Package Queue、Guard 收口和用户明确 assignment；其他内容只能作为上游上下文。
- 普通已排队包没有 Guard 收口 assignment 或用户确认时，开发窗口必须保持 `WAITING_ASSIGNMENT`。
- 触发 Guard 例外条件时，没有 `GUARD_PASS` 和用户确认不得继续。

## 3. 不通过回流

如果用户回复“不通过”并写明原因，流程不得直接进入自由修复。

普通当前包实现 bug 的快线回流：

```text
用户回复“不通过 + 原因”
→ 当前开发窗口停止自由修改
→ 开发窗口把失败原因同步给 Guard
→ 若属于普通当前包实现 bug，用户可直接授权当前开发窗口按原 assignment 范围修复
→ 开发窗口修复并重新自测
→ 用户再次手动验证
```

只有异常失败才进入 Guard / GPT 外部归因：

```text
失败原因涉及架构、状态机、存档、Boss、奖励、数值、主流程或红线
→ 当前开发窗口停止自由修改
→ 用户在 GPT / 外部技术流程中确认失败归因或明确替代输入
→ Guard 收口修复边界
→ 必要时临时启用 codex task writer / QA reviewer
→ 用户审批修复任务包
→ 开发窗口按修复档案执行
→ 开发窗口重新自测并给用户手测清单
→ 用户再次手动验证
```

异常失败包括：

```text
任务边界不清
任务不在 Package Queue
需要改 RunFlow / PageState / FormationState / Save / Boss / Reward / 数值 / DamageText / V02FormationGridFrame
同一包连续修复失败
外部工具交接污染主流程
```

tech architect、codex task writer、QA reviewer 旧线程均已停用自动流转；当前开发窗口不得再尝试自动发送这些线程。异常失败时只能生成失败回流报告并停止，等待用户提供 GPT / 外部技术归因或明确替代输入，并由 Guard 收口。

异常失败回流中，如果缺少应有的用户确认技术归因 / 替代输入、Guard 修复边界收口、必要的 `GUARD_PASS` 或用户对修复任务包的审批，开发窗口不得重新获得修复开发权限。

普通当前包实现 bug 不强制旧角色介入；但开发窗口必须向 Guard 同步失败状态，避免队列和记忆误判为通过。

不通过时的禁止行为：

- 开发窗口不得自行猜测修复方案并直接大改。
- 不得借修 Bug 扩展新功能。
- 异常失败不得绕过用户确认的 GPT / 外部技术归因或明确替代输入。
- 不得绕过 Guard 的修复边界收口。
- 触发 Guard 例外条件时，不得绕过 guard-agents 的边界检查。
- 触发 Guard 例外条件时，不得把 Guard 沉默、未响应或只收到摘要解释为边界检查通过。
- 不得把子代理临时分析草案解释为用户确认的正式失败归因或 Guard 收口。
- 不得把“已生成失败回流报告”表述为“已完成 Guard / RepoOps 同步”。

## 4. 新开发窗口启动协议

用户以后从 GPT / 外部策划流程拿到小版本命名或包体编号，并经 Guard 收口后，可以创建新的当前版本开发窗口。

开发窗口启动时应遵守：

```text
你是 [版本名 / 包体编号] 开发窗口。
先读取 AGENTS.md 和 Docs/LOCKED/*。
再读取当前版本 Package Queue，例如 Docs/V0.3/V0.3_PACKAGE_QUEUE.md。
你的任务包是 Package Queue 指向的当前包，或用户明确指定的包。
不得自行扩展范围。
任务依据：Package Queue + Guard 收口 + 用户明确 assignment。
完成后你自行给出自测结果、修改文件、风险点和用户手测清单。
如果我回复“通过 / 手测通过”，只同步 Guard + RepoOps。
如果我回复“不通过 + 原因”，停止自由修改，先把失败状态同步给 Guard；
普通当前包 bug 可在用户授权后按原范围修复；
若涉及红线、架构、状态机、存档、Boss、奖励、数值或主流程，停止并等待用户提供 GPT / 外部技术归因或明确替代输入，必要时由 Guard 决定是否临时启用重流程。
```

如果开发窗口无法读取对应任务档案、QA 报告或失败原因，必须停止并回报，不得凭空补全。

如果用户在新开发窗口中说“开始流转新任务”或等价指令，开发窗口必须先读取 Package Queue。若当前包已在队列中，直接进入 Light Path；若任务不在队列中，必须进入 `WAITING_ASSIGNMENT`，等待用户提供 GPT / 外部策划输入与 Guard 收口。不得并发请求多个角色下发内容，不得把 `PASS_SYNC_BROADCAST` 当作任务启动方式。

## 5. 同步边界

当前默认不再执行多角色自动流转。同步只表示任务窗口把状态交给 Guard + RepoOps，不表示任何窗口获得无限授权。

- 默认同步目标只有 Guard 与 RepoOps。
- 有线程协作、窗口读取或消息工具时，可以使用工具同步，但必须真实发送 / 真实读取并说明结果。
- 没有可用工具、目标角色未注册或目标窗口不可确认时，必须生成清晰摘要交给用户转发。
- 不得假装已经通知了其他窗口。
- 不得假装已经读取了不可访问窗口的内容。
- 不得把“自动”理解为跳过用户确认。
- 不得由一个窗口自行扮演多个旧角色来补齐流程，除非用户明确临时恢复重流程且不违反记忆权限规则。
- 子代理可以辅助调查或形成草案，但不能自动获得固定角色注册身份；其产物必须明确标记为临时草案。
- 用户确认通过后的状态同步必须有 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS` 证据；没有真实发送或手动转发包时，不得声称 Guard 或 RepoOps 已知晓通过状态。
- 新任务启动必须先走 Package Queue Light Path；队列外或异常任务等待用户提供 GPT / 外部策划输入与 Guard 收口，不得用 `PASS_SYNC_BROADCAST` 或多角色并发消息替代任务生成。

## 6. 阶段产物硬门与用户审批

流水线按产物推进，不按口头声称推进。

| 阶段 | 必需正式产物 | 下一步条件 |
| --- | --- | --- |
| Package Queue | 当前包、上一包状态、下一包指针 | 普通包直接进入快线 |
| GPT / 外部策划输入 | 版本方向、拆包或异常失败归因 | 由用户确认后给 Codex |
| guard-agents | 边界收口、红线判断、Package Queue / 记忆更新；必要时 `GUARD_PASS` / `GUARD_RETURN` | 普通包轻量收口；越界、红线、改规则或异常失败时明确回执 |
| 用户 | 当前包执行确认或修复任务包审批 | 用户明确批准 |
| 当前开发窗口 | 实现、自测报告、修改文件、用户手测清单 | 用户执行手测 |
| 用户验收 | “通过”或“不通过 + 原因” | 决定下一包或失败回流 |
| 当前开发窗口 | `TASK_STATUS_SYNC_TO_GUARD_REPOOPS` | 通过后只同步 Guard + RepoOps |
| RepoOps | 版本状态记录或记录阻塞原因 | 通过后完成小版本记录；不自动 commit / tag / push |
| 当前开发窗口 | `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` | 新任务开始先取队列当前包，不得默认重跑 PM / Architect |
| Cross-System Executor / ZCode | 当前 `DISABLED` | 未重新审批启用前不得作为正式外部执行或 QA 交接产物 |

为了减少用户负担，普通队列包不再每包要求 producer tech pm、tech architect、codex task writer、QA reviewer 的中间产物；路线与技术归因改由用户在 GPT / 外部流程确认后回填给 Codex，Guard 做收口。当前包执行确认、QA 不通过后的修复方向、最终手测结果和记忆文件修改仍必须由用户决定。

RepoOps 记录属于通过后的收口动作，不要求用户二次审批记录内容；但涉及 commit、tag、push、分支、回滚等仓库状态变化时，必须另行取得用户明确授权。

## 7. 锁功能，不锁细节

Guard-agents 锁定的是功能边界和版本收口，不锁普通实现细节。

应锁定：

- 当前版本做什么 / 不做什么。
- 哪些入口必须存在或保持占位。
- 哪些系统不能被误接入。
- 哪些基线不能被破坏。
- 哪些页面职责不能混淆。
- QA 通过 / 不通过后的流转规则。

不应锁定，除非任务档案另有明确要求：

- 导航栏按钮先后顺序。
- UI 尺寸、颜色、间距、动效。
- 临时文案微调。
- 灰盒布局细节。
- 具体实现类名、函数名或局部组织方式。

实现细节可以由开发窗口在任务档案边界内决定；一旦实现细节会改变功能职责、版本范围、稳定基线或禁止项，则必须回到 guard-agents 做边界判断。
