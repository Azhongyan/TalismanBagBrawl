# DELIVERY ACCEPTANCE GATE

本文件锁定《符箓背包》任务包的交付形态、用户可见性、测试独立性与放行证据。

它不锁 UI 布局、颜色、尺寸、导航顺序等普通实现细节；它只保证开发实际交付了用户要求的功能形态，而不是仅交付“代码存在”或“测试能够主动制造成功状态”。

## 1. 任务档案必须写清的交付契约

凡涉及场景、页面、UI、入口或用户可见状态的任务，Guard 收口后的 assignment、Package Queue 任务卡或用户明确任务说明必须写出：

- 用户从哪里进入。
- 用户执行什么操作。
- 最终应在哪个 Scene / Page / Root 中看到什么。
- 交付对象是独立新场景、现有场景内页面，还是其他明确形态。
- 默认激活 / 默认隐藏状态。
- Play 首帧是否必须可见。
- Unity 或游戏重启后是否仍必须可见。
- 哪些表现属于真实完成，哪些只能算占位或内部准备。

assignment / 任务说明缺少上述必要信息时，Guard 必须要求补齐；若用户明确要求重流程，可临时启用 codex task writer 重写。触发 Guard 例外条件时，guard-agents 必须退回重写，不得放行开发。

## 2. 用户可见性硬门

对于用户要求“打开就能看到”“进入页面就能看到”或等价结果的任务：

- 真实 Project / Scene / Hierarchy / Game View 路径必须能够到达该结果。
- 功能不得只存在于隐藏、默认 inactive、未接入口或仅供测试创建的对象中。
- 编译通过、类存在、Prefab 存在、引用存在、反射可访问，均不能单独证明用户可见交付完成。
- 如果交付形态与任务档案约定不一致，自动测试全部通过也必须判定 QA 不通过。

## 3. 测试独立性硬门

自动测试可以沿真实产品入口执行操作，但不得替被测功能完成它本来应自行完成的状态。

禁止用以下方式制造通过：

- 主动 `SetActive(true)` 打开本应默认可见的目标。
- 通过 reflection 调用正常用户路径不会调用的内部方法。
- 在测试中临时实例化本应由正式场景或正式入口提供的交付对象。
- 绕过正式 Scene / PageState / Root 流转，直接切到目标状态后再断言目标存在。
- 修改生产配置、存档或运行状态，使失败功能在断言前被测试修好。

如果测试必须进行准备，准备步骤必须使用与真实产品一致的公开入口，并且不能改变本次要验证的核心前置条件。

测试主动制造成功状态形成的结果属于假阳性，必须作废并进入 QA 不通过回流。

## 4. 放行证据

任务完成至少需要以下证据：

1. 任务档案中的交付契约已逐项对应到实现。
2. 编译或基础自动验证通过。
3. 自动测试未违反测试独立性硬门。
4. 按真实用户路径验证交付形态和可见结果。
5. 开发窗口明确给出自测 / QA 结论、风险点和用户手测项目。
6. 用户完成手测并明确回复“通过”，才算最终验收。

自动验证不能替代用户手测，guard-agents 的边界通过也不能替代用户最终验收。

## 4.1 UI 原地编辑硬规则

所有 UI 修改默认遵守“原地编辑优先”原则，尤其适用于 Unity Scene / Prefab 中已有挂载、引用和事件绑定的对象。

规则：

```text
1. 能改 RectTransform / Layout / Text / Image / CanvasGroup，就不新建 GameObject。
2. 能复用原按钮，就不删旧按钮再建新按钮。
3. 能改现有 Root 的子层级，就不另起新 Root。
4. 需要新增容器时，只允许新增“包裹层 / 装饰层”，不得替换原挂载对象。
5. 任何带脚本、delegate、事件绑定、引用字段、serialized data 的对象默认禁止删除重建。
6. 如果必须重建，先导出原对象挂载组件、引用、事件、字段，再写迁移清单，等 Guard 批准。
```

补充约束：

```text
- UI 任务不得为了“整理层级”删除或重建带脚本对象。
- UI 任务不得用新对象替代旧对象后遗漏原 serialized 引用、Button onClick、delegate 或场景引用。
- 若旧对象阻挡当前功能，优先隐藏、禁用、绕开、调整 sibling / layer 或新增非破坏性包裹层。
- 清理历史残渣必须单独开清理包；不得混入功能开发包。
```

## 4.2 UI Builder / Hand Tune / Runtime Lock 长期规则

长期 UI 工作方式锁定为：

```text
代码负责搭骨架，用户负责定版面；定版后代码不能再乱动版面。
```

三阶段：

```text
Builder 阶段：代码可以生成初始 UI 骨架、Canvas、Root、按钮、面板、占位图和基础层级。
Hand Tune 阶段：用户在 Unity 非 Play 状态下手动调整大小、位置、层级、图片、颜色、间距、文字摆放等。
Lock 阶段：用户手调后的 Scene Hierarchy / RectTransform / sibling order / active 状态成为正式版面真源；Runtime 只绑定和驱动，不重排。
```

进入 Hand Tune / Lock 后，Runtime / Play 状态代码允许：

```text
绑定按钮事件。
更新文字、图片、数据。
SetActive 显示 / 隐藏。
播放动画。
做业务状态切换。
读取并使用场景中已有对象引用。
```

进入 Hand Tune / Lock 后，Runtime / Play 状态代码禁止：

```text
重建 Canvas / Root / SafeArea 容器。
每次 Play 重新生成另一套层级。
移动用户手调对象。
覆盖 RectTransform 的 position / anchoredPosition / sizeDelta / anchors / pivot / scale。
覆盖 sibling order。
自动重排、自动改尺寸、自动改间距。
用 Ensure / Bootstrap / Builder 逻辑覆盖用户在非 Play 状态下确定的场景版面。
造成非 Play 场景层级与 Play 后视觉结果变成两套 UI。
```

对 Builder / SceneBuilder / Editor 工具的要求：

```text
1. 允许用于“第一次搭 UI 骨架”。
2. 一旦用户手调通过，Builder 必须进入不覆盖模式。
3. 后续 Builder / Verifier 只能补缺失引用、做静态检查、绑定事件或提示缺口。
4. 不得无条件重建已存在的用户手调对象。
5. 不得无条件重开被用户关闭的 Image / Outline / Layout / ContentSizeFitter / Mask / RaycastTarget 等版面相关组件。
6. 若确需迁移版面，必须先列迁移清单并取得 Guard / 用户明确批准。
7. Hand Tune / Lock 后，缺正式节点应报错或输出缺口报告，而不是 Runtime 自动重建另一套默认 UI。
8. 任何 fallback / Ensure / CreateRuntime / Builder 逻辑都必须有退场条件；不得长期覆盖场景真源。
```

全局适用范围：

```text
本规则适用于所有 Unity 场景 / UI 场景 / 页面，包括但不限于 MainHome、FormationCounter / BattlePrepare、TalismanUpgrade / 养成页、BootEntry、后续所有 V0.3+ UI 场景和任何用户已手调并要求锁定的 UI 节点。
MobileSafeAreaRoot / Runtime Bootstrap / Ensure 类逻辑如需保留，应改为读取 / 绑定已有场景对象，而不是运行时搬动或重建用户手调 UI。
所有已手调锁定的 UI 场景，Play 状态视觉结果都必须遵从非 Play 手调结果。
若某些战斗运行时纯动态 HUD 需要例外，必须在任务说明 / Guard assignment 中单独授权；不得默认例外。
```

Play 状态 UI Snapshot / Apply 工具例外规则：

```text
默认规则仍是：非 Play 场景文件为最终基准，Play 状态跟随场景文件。

如果用户在 Play 状态临时调 UI，只有在用户显式点击 Editor 工具（例如 Snapshot / Apply / Save Play UI To Scene）后，才允许把 Play 状态 UI 回写到场景文件，成为新的非 Play 基准。

该工具不得自动运行，不得在进入 Play、退出 Play、编译刷新、打开场景、运行 Builder / Bootstrap / Ensure 时自动回写。

没有用户显式点击时，Play 状态改动不得被视为正式版面真源。
```

全局 Play UI Snapshot / Apply 白名单规则：

```text
Play UI Snapshot / Apply 不是 MainHome 专用概念，可用于所有场景 / UI 场景。

但每个具体场景若需要该工具，必须单独建立场景级白名单和菜单项。

每个 Snapshot 工具必须明确：
- 目标 Scene
- 目标 Root / Canvas / 子树
- 允许回写的组件和字段
- 禁止回写的组件和字段

禁止跨场景、全项目扫写或无白名单回写。

允许记录 / 回写：
- GameObject.activeSelf
- RectTransform anchors / pivot / anchoredPosition / sizeDelta / localScale / rotation
- sibling order
- Image enabled / sprite / color / type / preserveAspect / raycastTarget
- Outline enabled / effectColor / effectDistance
- Text / TMP_Text 的 text / fontSize / color / alignment / raycastTarget

禁止记录 / 回写：
- 未在白名单中的其他 Scene / Root / 子树
- ProjectSettings / BuildSettings
- 存档 / PlayerPrefs / MainTrialProgressData / SaveData
- 主流程、RunFlow、PageState、FormationState
- 珠串进度条、战斗 HUD、运行时动态生成系统等未被该场景白名单显式纳入的 UI
- Button onClick、delegate、脚本字段、Prefab 连接、资源配置、数值配置
- 自动删除或重建用户手调对象
```

## 5. Guard 例外回执硬门

普通已进入 Package Queue 的日常小包，不再默认要求 guard-agents 每包审查或 `GUARD_PASS`。

只有以下情况触发 Guard 例外回执硬门：

```text
任务越出当前版本范围
任务或修复想触碰红线文件 / 红线系统
需要修改 AGENTS.md / Docs/LOCKED/* / Package Queue
外部工具协同可能污染主流程
QA 不通过原因涉及边界 / 架构 / 红线
Guard 无法在既定边界内收口出清晰 assignment / 交付契约
```

触发 Guard 例外时，guard-agents 必须给出明确回执：

- `GUARD_PASS`：功能边界、版本范围、稳定基线、交付契约和验收定义均可放行。
- `GUARD_RETURN`：写明缺失项、越界点或需要重写的内容。

触发 Guard 例外但没有明确 `GUARD_PASS` 时，不得提交用户审批，不得创建开发窗口，不得开始或恢复开发。

沉默、未响应、仅收到摘要、只完成自动测试或开发窗口自行判断，均不等于 Guard 已通过。

## 6. 用户审批保持低打扰

内部角色产物不要求用户逐项审批。用户保留以下关键决策：

1. 首次开发前，确认当前 Package Queue 指向的任务包、Guard 收口后的 assignment 或用户直安排任务；触发 Guard 例外时，才要求审批已经过 Guard 检查的任务包。
2. QA 后，回复“通过”或“不通过 + 原因”。
3. QA 不通过回流后，普通当前包 bug 可在同步 Guard 后审批任务窗口按原 assignment 范围修复；若涉及架构 / 红线 / 状态机 / 存档 / Boss / 奖励 / 数值 / 主流程，则先由用户提供 GPT / 外部技术归因或明确替代输入，再审批必要 Guard 检查后的修复任务包。
4. 项目记忆文件修改仍按 `MEMORY_FILE_APPROVAL_RULE.md` 单独审批。
5. 用户确认通过后，任务窗口必须向 Guard + RepoOps 发送 `TASK_STATUS_SYNC_TO_GUARD_REPOOPS`；Guard 更新记忆与 Package Queue，RepoOps 做小版本状态记录。RepoOps 记录不等于授权 commit、tag、push 或回滚。
6. 默认不再使用 `PASS_SYNC_BROADCAST` 多角色广播；producer tech pm、tech architect、codex task writer、QA reviewer 旧线程均不再发送。
7. 新任务开始必须先读取 Package Queue，普通已排队包走 Light Guard + RepoOps 模式；队列外或异常任务停止等待用户提供 GPT / 外部策划或技术归因输入，并由 Guard 收口。
8. `CROSS_SYSTEM_EXECUTOR_PROTOCOL.md` 当前状态为 `DISABLED` 时，ZCode 不得作为正式跨系统执行体执行或 QA。若未来重新启用，任务由 ZCode 执行或 QA 时，必须按协议落盘 Handoff Package；没有交接包，不得视为跨系统交接完成。

中间角色可以自动流转，但不得以减少用户操作为理由跳过上述关键审批。
