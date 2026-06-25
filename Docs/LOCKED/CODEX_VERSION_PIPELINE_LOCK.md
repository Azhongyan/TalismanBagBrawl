# CODEX VERSION PIPELINE LOCK

本文件锁定《符箓背包》在 Codex 内的长期版本生产工作流。

它不是开发任务书，不授权任何窗口直接开发；它只定义各角色如何流转、何时进入开发、何时回流修复、以及 Guard-agents 如何做边界收口。

角色窗口是否已注册、是否允许自动发送或读取，见 `Docs/LOCKED/CODEX_ROLE_WINDOW_REGISTRY.md`。
交付形态、用户可见性、测试独立性与放行证据，见 `Docs/LOCKED/DELIVERY_ACCEPTANCE_GATE.md`。

## 1. 长期角色分工

### 用户 / 产品源头

- 用户先在 GPT 内与策划完成产品总体 rundown、当前大版本方向或当前版本功能构想。
- 用户负责最终确认版本方向、任务包是否可下发、QA 是否通过。
- 用户的“通过”或“不通过 + 原因”是版本任务进入下一状态的关键触发。

### producer tech pm

- 接收用户确认过的产品 rundown 或当前版本功能构想。
- 拆解大版本功能、当前版本迭代计划和版本优先级。
- 判断哪些内容属于当前版本，哪些内容必须延后。

### tech architect

- 接收 producer tech pm 的版本计划。
- 将当前版本拆成若干包体和开发步骤。
- 判断包体依赖顺序、技术风险、回滚边界和是否需要 Code Survey。
- QA 不通过时，负责分析失败原因属于架构、实现、范围、遗漏、状态同步、测试误差或任务档案缺口中的哪一类。

### codex task writer

- 接收 tech architect 的包体拆解。
- 为每个包体写可执行技术任务档案。
- 在任务档案中写明允许修改范围、禁止修改范围、验收标准、QA 要点和回滚范围。
- 对场景、页面、UI 或入口任务写明用户路径、交付形态、默认激活状态、首帧可见性和重启后预期。
- 将任务摘要同步给 guard-agents 做当前版本技术收口。
- QA 不通过并经 tech architect 分析后，负责重写当前包体的修复任务档案。

### guard-agents

- 本长期置顶常驻窗口认领 guard-agents 职责。
- 只负责守边界和版本收口，不直接开发。
- 检查任务包是否越过当前版本范围。
- 检查修复任务是否借机扩功能。
- 保护 V0.2 稳定基线。
- 维护当前版本技术收口摘要。
- 在通过、不通过、重写任务档案、进入下一包体等关键节点检查边界。
- 对任务档案和修复任务档案给出明确 `GUARD_PASS` 或 `GUARD_RETURN`，沉默不视为通过。
- 检查自动测试是否替被测功能主动制造成功状态。
- 锁功能边界、禁止项、版本范围和基线安全；不锁实现细节。

### 当前版本开发窗口

- 由用户在确认任务包后手动创建。
- 开发窗口必须先读取 `AGENTS.md` 与 `Docs/LOCKED/*`。
- 开发窗口只执行用户指定的小版本名 / 包体编号。
- 开发窗口只能依据 codex task writer 已产出的、经用户确认的任务档案执行。
- 开发窗口不得自行扩大版本范围，不得把 QA 不通过解释为自由重构授权。

### QA reviewer

- 接收开发完成结果。
- 按固定格式向用户汇报：
  - 本次检查对象
  - 通过 / 不通过结论
  - 失败原因
  - 风险点
  - 用户需要手动验证的内容
  - 是否建议进入下一任务包
- QA reviewer 不负责直接改代码。

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

- 接收用户确认“通过 / QA 通过 / 手测通过”后的版本记录请求。
- 负责按大版本 / 小版本节奏记录版本状态、变更摘要、任务包来源、QA 结论、用户手测结论、风险点、回滚点和下一步建议。
- 小版本小记录：每个任务包通过后记录一次。
- 大版本大记录：大版本阶段收口或用户明确要求时记录一次。
- RepoOps 默认只做版本记录与归档规划；未获用户单独明确授权时，不自动执行 commit、tag、push、分支操作或回滚。

## 2. 标准通过流转

标准流转如下：

```text
GPT / 策划产品 rundown
→ producer tech pm 拆版本计划
→ tech architect 拆包体与步骤
→ codex task writer 写任务档案
→ guard-agents 做边界与收口检查并返回 GUARD_PASS
→ 用户审批任务包
→ 用户创建当前版本开发窗口
→ 开发窗口按任务档案开发
→ QA reviewer 验收汇报
→ 用户手动验证并回复“通过”
→ RepoOps 做小版本记录并返回 REPOOPS_RECORD_DONE
→ 开发窗口向 producer tech pm / tech architect / codex task writer / QA reviewer / guard-agents 发送 PASS_SYNC_BROADCAST
→ 广播完成并返回 PASS_SYNC_SENT
→ 同步 producer tech pm / tech architect / codex task writer 进入下一包体准备
→ guard-agents 更新当前版本技术收口
→ 下发下一任务包
```

只有用户确认“通过”后，才允许进入下一任务包。

用户确认“通过 / QA 通过 / 手测通过”后，必须先完成 RepoOps 小版本记录，再完成 `PASS_SYNC_BROADCAST`，才可进入下一任务包同步。RepoOps 记录完成不等于授权 commit / tag / push。

用户确认通过后的固定角色同步必须使用 `PASS_SYNC_BROADCAST`。如果当前窗口具备真实发送线程消息的工具能力，必须逐一发送给已注册固定角色；如果当前窗口没有真实发送能力，必须生成可手动转发的同步包，并停止在等待状态，不得声称其他常驻窗口已经知道本包通过。

`PASS_SYNC_BROADCAST` 至少包含：

- 包体名 / 小版本名
- 任务档案路径
- 开发窗口名称或来源
- QA reviewer 结论
- 用户手测通过原文或等价摘录
- RepoOps 记录状态
- 本包最终结论
- 下一步建议

`PASS_SYNC_SENT` 只表示“已真实发送或已生成手动转发包”，不表示被同步角色已经完成下一阶段产物。

`PASS_SYNC_BROADCAST` 只允许用于“用户验收通过后的状态同步”，不得用于新任务下发、任务生成或开发授权。

没有 `GUARD_PASS` 的任务包不得提交用户审批，也不得开始开发。

## 2.1 新任务单线回转 / SINGLE_LANE_TASK_INTAKE

新任务开始属于“任务生成”，不是“状态广播”。必须走单线流水线，不得由 producer tech pm、tech architect、codex task writer 并发向开发窗口下发内容。

标准新任务入口如下：

```text
用户创建或指定当前开发窗口
→ 开发窗口进入 WAITING_ASSIGNMENT，不开发、不改名、不改代码
→ 若缺少版本计划，先单线请求 producer tech pm 给出当前版本节奏 / 下一包建议
→ tech architect 基于版本计划返回 PACKAGE_ASSIGNMENT：当前窗口包名、技术分析、范围、风险、依赖、回滚边界
→ codex task writer 基于 PACKAGE_ASSIGNMENT 写正式任务档案
→ guard-agents 审查正式任务档案并返回 GUARD_PASS
→ 用户审批任务包
→ 开发窗口只从 codex task writer 的正式任务档案接收包名、标题和开发内容；Cross-System Executor 当前为 DISABLED，不参与正式接收
→ 执行窗口改当前任务栏 / 窗口名称并进入开发
```

如果用户已经从 producer tech pm 或 tech architect 拿到当前阶段计划，新任务入口可以从 tech architect 开始；但仍必须继续单线流向 codex task writer，不得跳过 task writer 正式任务档案。

新任务入口的硬规则：

- producer tech pm 只给版本节奏、优先级和下一包建议，不直接给开发窗口下发工单。
- tech architect 只给包体拆分、技术分析、风险和回滚边界，不直接授权开发。
- codex task writer 是开发窗口唯一正式工单来源。
- guard-agents 只审正式任务档案边界，不下发开发内容。
- 开发窗口不得把 producer tech pm 或 tech architect 的产物当作开发任务档案。
- 开发窗口不得合并多角色并发回复后自行形成工单。
- Cross-System Executor 当前为 `DISABLED`。若未来重新启用，ZCode 作为跨系统执行体承接开发也必须遵守同一条 `SINGLE_LANE_TASK_INTAKE`，不得从 producer tech pm 或 tech architect 直接接工单。
- 如果开发窗口同时收到 producer tech pm、tech architect、codex task writer 的并发内容，必须停线，只认 codex task writer 的正式任务档案；其他内容只能作为上游上下文。
- 没有 codex task writer 正式任务档案、guard-agents `GUARD_PASS` 和用户审批时，开发窗口必须保持 `WAITING_ASSIGNMENT`。

## 3. 不通过回流

如果用户回复“不通过”并写明原因，流程不得直接进入自由修复。

必须按以下路径回流：

```text
用户回复“不通过 + 原因”
→ 当前版本状态与失败原因返回 tech architect
→ tech architect 返回正式失败归因
→ codex task writer 返回当前包体修复任务档案
→ guard-agents 检查修复档案并返回 GUARD_PASS
→ 用户审批修复任务包
→ 开发窗口按修复档案执行
→ QA reviewer 重新验收
→ 用户再次手动验证
```

如果当前开发窗口无法真实发送给 tech architect，或 tech architect 尚未在 `CODEX_ROLE_WINDOW_REGISTRY.md` 中注册，则开发窗口只能生成失败回流报告并停止，等待用户手动转发。

如果没有拿到 tech architect 的失败归因、codex task writer 的修复任务档案、guard-agents 的 `GUARD_PASS` 与用户对修复任务包的审批，开发窗口不得重新获得修复开发权限。

不通过时的禁止行为：

- 开发窗口不得自行猜测修复方案并直接大改。
- 不得借修 Bug 扩展新功能。
- 不得绕过 tech architect 的失败归因。
- 不得绕过 codex task writer 的修复任务档案。
- 不得绕过 guard-agents 的边界检查。
- 不得把 Guard 沉默、未响应或只收到摘要解释为边界检查通过。
- 不得把子代理临时分析草案解释为已注册角色的正式失败归因或正式修复任务档案。
- 不得把“已生成失败回流报告”表述为“已完成 tech architect / codex task writer 流程”。

## 4. 新开发窗口启动协议

用户以后从 tech architect 拿到小版本命名或包体编号后，可以创建新的当前版本开发窗口。

开发窗口启动时应遵守：

```text
你是 [版本名 / 包体编号] 开发窗口。
先读取 AGENTS.md 和 Docs/LOCKED/*。
你的任务包是：[任务包名]。
不得自行扩展范围。
任务档案来源：codex task writer。
完成后交 QA reviewer。
如果我回复“不通过 + 原因”，你必须先回流 tech architect 分析，
再等待 codex task writer 的修复任务档案，不得直接自由修。
```

如果开发窗口无法读取对应任务档案、QA 报告或失败原因，必须停止并回报，不得凭空补全。

如果用户在新开发窗口中说“开始流转新任务”或等价指令，开发窗口必须按 `SINGLE_LANE_TASK_INTAKE` 进入 `WAITING_ASSIGNMENT`，不得并发请求多个角色下发内容，不得把 `PASS_SYNC_BROADCAST` 当作任务启动方式。

## 5. 自动流转边界

“自动流转”只表示窗口之间应按上述角色和状态机推进，不表示任何窗口获得无限授权。

- 自动流转必须以 `CODEX_ROLE_WINDOW_REGISTRY.md` 中的已注册角色为目标。
- 有线程协作、窗口读取或消息工具时，可以使用工具同步，但必须真实发送 / 真实读取并说明结果。
- 没有可用工具、目标角色未注册或目标窗口不可确认时，必须生成清晰摘要交给用户转发。
- 不得假装已经通知了其他窗口。
- 不得假装已经读取了不可访问窗口的内容。
- 不得把“自动”理解为跳过用户确认。
- 不得由一个窗口自行扮演多个角色来补齐流程，除非用户明确把该窗口指定为对应角色且不违反记忆权限规则。
- 子代理可以辅助调查或形成草案，但不能自动获得固定角色注册身份；其产物必须明确标记为临时草案。
- 每次跨窗口推进必须收到该阶段要求的正式产物，不能仅凭“消息已发送”继续下一阶段。
- 用户确认通过后的常驻窗口同步必须有 `PASS_SYNC_BROADCAST` 和 `PASS_SYNC_SENT` 证据；没有真实发送或手动转发包时，不得声称 producer tech pm、tech architect、codex task writer、QA reviewer 或 guard-agents 已知晓通过状态。
- 新任务启动必须使用 `SINGLE_LANE_TASK_INTAKE`；不得用 `PASS_SYNC_BROADCAST` 或多角色并发消息替代任务生成流水线。

## 6. 阶段产物硬门与用户审批

流水线按产物推进，不按口头声称推进。

| 阶段 | 必需正式产物 | 下一步条件 |
| --- | --- | --- |
| tech architect | 包体拆解或失败归因 | 已注册角色真实返回 |
| codex task writer | 开发任务档案或修复任务档案 | 已注册角色真实返回 |
| guard-agents | `GUARD_PASS` / `GUARD_RETURN` | 只有 `GUARD_PASS` 可继续 |
| 用户 | 首次任务包审批或修复任务包审批 | 用户明确批准 |
| QA reviewer | QA 报告和手测清单 | 用户执行手测 |
| 用户验收 | “通过”或“不通过 + 原因” | 决定下一包或失败回流 |
| RepoOps | `REPOOPS_RECORD_DONE` 或记录阻塞原因 | 通过后完成小版本记录，才同步进入下一包体 |
| 当前开发窗口 | `PASS_SYNC_BROADCAST` / `PASS_SYNC_SENT` | 通过后通知固定角色，才允许进入下一包体准备 |
| 当前开发窗口 | `WAITING_ASSIGNMENT` / `SINGLE_LANE_TASK_INTAKE` | 新任务开始时等待单线任务生成，不得并发下发 |
| Cross-System Executor / ZCode | 当前 `DISABLED` | 未重新审批启用前不得作为正式外部执行或 QA 交接产物 |

为了减少用户负担，architect、task writer 与 Guard 的中间产物无需用户逐份审批；但首次任务包、QA 不通过后的修复任务包、最终手测结果和记忆文件修改仍必须由用户决定。

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
