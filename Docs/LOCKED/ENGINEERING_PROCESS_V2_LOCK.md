# 《符箓背包》Engineering Process V2 Lock

状态：`ACTIVE`

生效日期：`2026-07-30`

用户预验收门补充生效：`2026-07-31`

目标：把开发方式从“文档护栏很厚、代码护栏很薄；自制验证很多、标准测试很少”逐步改成“入口轻、边界清、代码可测、验收与风险匹配”。

## 1. 规则效力

优先级从高到低：

1. 用户在当前任务中的最新明确决定；
2. 当前 Task Contract / Assignment；
3. 本 V2 Lock；
4. 与任务直接相关的产品 / 系统 Lock；
5. 历史 ROADMAP、CURRENT、Package Queue 与报告。

V2 只替换旧流程机械规则，不解除产品边界、系统 Owner、场景真源、存档、奖励、正式流程、用户手调 UI、Git 或受保护文件规则。

## 2. 最小任务入口

每个新任务固定读取：

1. `AGENTS.md`
2. `Docs/LOCKED/ENGINEERING_PROCESS_V2_LOCK.md`
3. `Docs/CURRENT/PROJECT_ENGINEERING_STATE_V2.md`
4. 当前 Task Contract / Assignment（若存在）

之后只读取合同列出的任务相关锁、目标代码、直接依赖、数据与素材。

禁止把以下内容重新设为所有窗口的统一前置：

- 全部 `Docs/LOCKED/*`
- 旧 V0.3 CURRENT / Package Queue
- 与当前写域无关的大量历史报告
- 其他 Guard 的完整记忆

没有 Task Contract 时：

- 简单任务：用一句话冻结目标、写域、禁区和验收后直接执行；
- 其他任务：按 `Docs/TEMPLATES/TASK_CONTRACT_V2.md` 建立最小合同。

## 3. 三种任务类型

### `SIMPLE_DIRECT_CLOSE`

适用：

- 单 Owner；
- 通常 1–4 个文件；
- 不改变状态 Owner、存档结构、奖励结算、正式流程或公共架构；
- 失败易回退；
- 编译 / 定向检查 / 一次手测即可证明。

流程：开发任务直接完成并自闭合；不建立 Guard 往返链。

### `CONTAINED_ONE_GUARD`

适用：

- 单系统内的小功能或中等修复；
- 通常 3–8 个文件；
- 需要一个 Guard 冻结边界，但没有跨系统状态迁移；
- 可由同一开发任务完成实现、同包修复和最小验收。

流程：一个 Primary Guard 一次收口，一次下发；开发期间保持安静。

### `COMPLEX_GUARDED_ONCE`

适用：

- 跨系统 immutable contract / Bridge；
- 存档、奖励、正式流程、迁移、公共架构；
- Scene / Prefab 大范围结构变化；
- 可能产生第二状态 Owner 或不可逆数据变化。

流程：一个 Primary Guard 完成一次边界冻结；其他 Guard 只回答一个明确 Owner 问题，不建立回执链。

### `USER_REVIEW_GATE`

`USER_REVIEW_GATE` 不是第四种任务类型，而是发生在开发派发之前的产品验收门。任务仍须归入上述三类之一。

默认必须进入本门的任务：

- 视觉风格、动画节奏、VFX 语言、声音与战斗可读性；
- 拖拽、滚动、吸附、点击、触摸与其他手感；
- 技能语义、敌人行为表现、关卡节奏与流程体验；
- 掉落策略、奖励策略、养成节奏及其他会改变用户体验的产品选择；
- 存在两个以上合理产品方案，且不同选择会导致明显返工的任务。

默认不需要进入本门的任务：

- 明确编译错误、空引用、生命周期异常和资源缺失；
- 已冻结行为的回归修复；
- 不改变产品表现的安全修复；
- 纯 immutable contract、确定性数据验证和工具修复，且没有未决产品语义。

流程：

1. 固定类别 Guard 只读调查并用白话给出推荐方案；
2. 只向用户提出真正影响产品结果的少量选择，其余由 Guard 按专业判断收口；
3. 用户明确同意后，Guard 产出 `USER_ACCEPTED_SCOPE`；
4. 未取得 `USER_ACCEPTED_SCOPE` 前，不创建开发 Assignment、不派开发任务、不启动实现；
5. 取得后，由该固定类别 Primary Guard 自己建立 Task Contract / Assignment、创建或复用合适的开发任务并一次性派发；仍按 `SIMPLE_DIRECT_CLOSE`、`CONTAINED_ONE_GUARD` 或 `COMPLEX_GUARDED_ONCE` 执行；
6. 用户后续改变产品方向时，先回到同一固定类别 Guard 更新范围，不直接让开发任务猜测。

固定 Guard 的职责是讨论、边界冻结、Owner、开发任务创建/复用与派发、事件驱动收口、同包返修调度和终态判断；Guard 不直接实现产品代码。普通开发任务不承担产品方案讨论。

总收口 / 系统归属拆分窗口只负责把用户需求路由给正确的固定类别 Guard、处理跨类别优先级与真实写域冲突、接收终态/阻断/用户决定事件。除系统治理任务或用户明确指定外，总收口窗口不得越过固定类别 Guard 直接创建开发任务。

## 4. Guard 最小干预

- 每个包只有一个 Primary Guard。
- 完整 Assignment 下发后，不发送普通状态、归因、排队或确认消息打断开发。
- 只有以下情况允许中断：
  - 已验证的写域冲突；
  - 安全停止；
  - 无法绕开的用户设计决定。
- 同包 Light Fix 留在同一开发任务；不重新建包、不重新开 Guard 链。
- Guard 不直接实现产品代码；Guard 负责边界、Owner、调度和终态判断。
- 简单任务证据足够即可结束；不强制用户手测所有内部合同与数据包。
- 主调度窗口不得为等待开发终态而持续轮询、占用用户对话或暂停其他可并行工作。
- 开发进度由对应 Primary Guard / 类别任务窗口负责收口并在终态、阻断或用户决定点主动回传；主调度窗口采用事件驱动接收。
- 用户主动询问进度时，主调度窗口可做一次即时状态检查并返回当前快照；除非用户明确要求持续监控，否则不继续轮询。
- 等待中的任务不妨碍主调度窗口继续拆分、派发和处理其他互斥写域之外的工作。

## 5. 并行与 Unity

- 互斥写域中的代码、文档、数据和离线测试可以并行。
- Unity Scene、Prefab、ProjectSettings、BuildSettings、资源导入序列化维持单 Owner。
- Unity 空闲只影响需要真实 Unity 序列化 / Play 的步骤，不得阻塞不依赖 Unity 的代码与离线测试。
- 不得因为另一个任务在开发就停止所有管线；只判断真实写域和 Unity 序列化冲突。

## 6. 与风险匹配的验收

| 任务类型 | 默认最低验收 | 默认不做 |
|---|---|---|
| 文档 / 模板 | 链接、格式、范围和 diff 检查 | Unity |
| 纯 C# 合同 / 数据，无 Unity API | 定向编译 + 确定性单元测试 | Unity batch |
| Runtime 代码，无 Scene 写入 | 编译 + 定向测试；真实生命周期需要时一次 Fresh Play | 多轮通用 harness |
| Scene / Prefab | 一次 authoring + 编译 + 一次 Fresh Play | 反复保存 / 多批次重复 QA |
| VFX / 动画 / 手感 | 编译 + 正常 Editor 人眼手测 | 默认截图识别、键鼠注入、自制视觉 PASS |
| Save / Reward / Migration | 单元 / 集成测试 + 旧样本 + 幂等 / 回滚；必要时一次 Fresh Play | 仅凭日志宣称安全 |
| APK / 实机 | 固定 build 入口 + 安装启动 + 目标设备 smoke | 用 Editor PASS 代替实机 |

补充规则：

- 自动 harness 最多允许一次同包纠正；如果正常 Editor 能到达产品路径，转一次最短用户手测。
- 自制 Verifier 只能补充缺口，不能替代标准单元 / 集成测试。
- 不为“报告好看”生成重复矩阵；immutable contract、determinism、migration 等确有风险时才保留必要报告。
- 自动化不能主动制造成功状态绕过真实产品路径。

## 7. 工具与进程 Owner

任务启动 Unity、Tuanjie、ShaderCompiler、bee、构建器或其他长任务时，必须记录：

- task / package；
- PID 与命令；
- 启动时间；
- 日志路径；
- 预期超时。

规则：

- 任务只能停止自己准确记录的卡死进程；
- 不得清理来源不明或用户控制的进程；
- 达到超时后先保存日志，再停止自己的进程并返回明确失败类型；
- 包终态不得遗留自己启动的子进程；
- 终态包遗留进程属于 `PROCESS_GOVERNANCE_FAIL`，不能变成无限 Unity Lease 阻断。

## 8. Task Contract 与补丁预算

每个合同至少包含：

- Outcome；
- Product Context；
- Task Class；
- Primary Guard / Dev Owner / User Decision Point；
- Allowed Writes / Forbidden Writes；
- Patch Budget；
- Required Behavior；
- Verification；
- Process Ownership；
- Completion Conditions。

默认补丁预算：

| 类型 | 建议文件数 |
|---|---:|
| 小 Bug | 1–4 |
| 小功能 | 3–8 |
| 垂直切片 | 6–15 |
| 架构 / 迁移 | 单独包，不与表现调参混做 |

超过预算不自动禁止，但必须先说明是遗漏的必要依赖还是任务已经变质。禁止借任务顺手重构、整理、迁移或清理历史代码。

## 9. Bug 回传必须先分类

开发任务回传失败时必须选择首个失败层：

- `PRODUCT_LOGIC_FAIL`
- `FIXTURE_OR_DATA_NOT_READY`
- `PRESENTATION_OR_LIFECYCLE_FAIL`
- `QA_HARNESS_FAIL`
- `EXTERNAL_ENVIRONMENT_BLOCKED`

不得把 harness、Fixture、生命周期异常混写成产品逻辑失败，也不得用“可能”直接触发跨系统大修。

## 10. Scene、Prefab 与调试 UI

- Scene 只装配和切换，不成为 HP、掉落、Build、奖励或战斗真值 Owner。
- Prefab 负责可替换表现；Config / Profile 负责静态内容；System 负责计算与状态；Bridge 只翻译 immutable contract。
- authored Scene / Prefab 是正式真源；正式路径不建立 runtime fallback UI。
- `RuntimeRoot` 与 `PanelRoot` 必须分开。
- 隐藏面板只能影响面板显示，不能停止 Runtime、Battle、VFX 或 Item 事件。
- 调试面板必须显式 opt-in，默认不出现在普通 Play / APK。

## 11. Skills 的位置

- Skill 用于稳定地执行一类操作，不用于复制整套项目历史。
- Skill 必须调用或约束固定脚本 / 标准测试入口；没有底层脚本与测试时，不先写“万能 Skill”。
- Skill 不是 Guard，不增加审批，不拥有产品决定。
- 用户可以自然语言下任务；只有需要强制指定流程时才显式写 `$skill-name`。

V2 第一阶段只建立流程与 Task Contract。项目专用 Skills 在第二阶段、固定脚本与标准测试入口落地后再创建。

## 12. 第一阶段边界

本阶段只修改：

- 工作区与工程入口规则；
- 本 V2 Lock；
- 当前工程事实快照；
- Task Contract 模板。

本阶段不修改游戏代码、Scene、Prefab、资源、Package、ProjectSettings、BuildSettings、存档、奖励、数值或 Git 状态。
