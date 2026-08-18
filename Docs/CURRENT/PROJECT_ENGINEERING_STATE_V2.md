# 《符箓背包》当前工程事实快照 V2

快照日期：`2026-08-11`

用途：给新任务一个短、可验证的工程入口。这里记录稳定工程事实，不记录每个开发包的实时聊天状态。

## 1. 工程身份

- Unity 工程：`F:\Porject\TalismanBagBrawl`
- Unity：`2022.3.50f1c1`
- 工程标识：`Assets`、`ProjectSettings`、`Packages` 均存在
- 当前工作树包含大量用户与并行任务改动；任何任务必须保留无关修改，不得 reset、checkout、回滚或重写不在白名单内的文件

## 2. 产品上下文必须显式

当前至少存在三种不同用途，禁止用 Scene 名或物体名隐式推断：

- `DEV_SHOWCASE_LV40`
  - 位置：以 `Scene_TalismanBag_V04_BattleSandboxPreview` 为主要样板间
  - 用途：满级 Build、Boss 长时间观察、动画 / VFX / UI / 战斗反馈证明
  - 不写正式进度、奖励或存档
- `PLAYTEST_VERTICAL_SLICE`
  - 用途：给外部测试者展示相对完整、节奏可控的代表体验
  - 可以使用专用测试 Profile，但不能冒充正式 1 级数值
- `CAMPAIGN_NORMAL_LV1`
  - 用途：正式 1-1—1-10 起步体验、低品阶掉落、正式关卡进度
  - 不消费固定 Lv40 展示数据

正式启动合同以后必须显式携带 productContext、balanceProfileId、encounterVariantId 等身份。

## 3. Sandbox 到正式房间

- BattleSandbox 是允许反复试错的样板间，不是正式 Campaign Owner。
- 可推广的不是整套 Sandbox 层级，而是已经稳定的：
  - 系统 Owner；
  - immutable input / output contract；
  - lifecycle；
  - Config / Profile；
  - 可替换 Prefab / presentation slot；
  - 最小 Adapter。
- 默认正式战斗装配目标：`Scene_TalismanBag_V04_UnifiedBattlePageShell`
- 正式方案是一套 Battle Scene 消费 StageConfig、StageThemePrefab、Enemy / Boss Prefab、BattleStart / BattleResult contracts。
- 禁止 Scene-per-stage，也禁止把 Sandbox 的 runtime injection graph 整体复制进正式场景。

## 4. 当前 Scene / Build Settings 事实

当前 Build Settings 已登记：

1. `Scene_TalismanBag_V03_BootEntry`
2. `Scene_TalismanBag_V03_MainHome`
3. `Scene_TalismanBag_V04_WorldMap`
4. `Scene_TalismanBag_V03_TalismanUpgrade`
5. `Scene_TalismanBag_V02_FormationCounter`

工程中存在但当前未登记为 Build Scene 的重要 V0.4 场景：

- `Scene_TalismanBag_V04_BattleSandboxPreview`
- `Scene_TalismanBag_V04_ItemSandbox`
- `Scene_TalismanBag_V04_UnifiedBattlePageShell`

任务不得仅因 Scene 文件存在就宣称 APK 可进入，也不得仅因未在 Build Settings 就擅自加入。

## 5. 当前工程护栏缺口

### 5.1 Cleanroom Governance Baseline V1

- `Docs/CURRENT/CLEANROOM_GOVERNANCE_BASELINE_V1.md` 已建立，冻结一个事实一个 Formal Owner、Formal 不依赖 Sandbox Runtime、临时结构必须有 `DeleteWhen` 等最小工程宪法。
- `Docs/CURRENT/PROJECT_CLEANROOM_COMPONENT_LEDGER.md` 继续作为唯一 CURRENT Component Ledger 与 Owner Registry，不新增平行台账。
- `Docs/TEMPLATES/TASK_CONTRACT_V2.md` 已加入默认“装修”、显式“水电”和中型以上任务的简短 `ROOM CHECK`。
- `scripts/check_project_hygiene.ps1` 已作为只读 Hygiene Gate 入口；它只报告风险，不自动修改工程。
- 治理基线已生效不等于 Cleanroom 已完成。现有 `REPLACE`、`QUARANTINE` 与正式旧依赖仍须按 Ledger 闭账。

截至本快照：

- 项目业务代码未形成清晰的 `.asmdef` 模块边界；
- 未发现项目正在使用的 Unity Test Framework / NUnit 标准测试入口；
- 未建立统一的 Runtime / Editor / PlayMode 测试分层；
- 已有很多项目自制 Editor Verifier / Validator / ReportWriter，价值不一；
- 根 `scripts` 已有轻量只读 Hygiene Gate，但仍没有统一的 compile、test、scene validation、APK smoke 入口；
- 工程内没有项目专用 `SKILL.md`；
- 所以当前事实是“流程文档和自制验证较多，编译边界与标准测试入口不足”。

## 6. 旧文档如何使用

- `Docs/LOCKED/*`、旧 ROADMAP、旧 CURRENT、V0.3 / V0.4 历史 Queue 和报告继续保留，作为产品决定与历史证据。
- 它们不再全部作为新窗口统一前置。
- `AGENTS.md` 第 4 节的大量记录是历史来源，不代表当前包状态。
- 当前任务只读取与 Task Contract 写域和 Owner 真正相关的文档。

## 7. V2 后续工程优先级

第二阶段建议按顺序推进：

1. 固定只读 `preflight` / `compile` / `test` / `scene-check` 脚本入口；
2. 引入最小 Unity Test Framework，并先覆盖纯逻辑、合同、Reducer、存档 / 奖励幂等；
3. 建立最小 asmdef 边界，先从新模块与测试程序集开始，不做全工程大迁移；
4. 把仍有价值的自制 Verifier 收敛到标准测试或固定脚本；
5. 基于这些入口创建项目专用 Skills；
6. 逐步建立固定 APK 构建与设备 smoke。

每一步单独立包，不能和当前主线、VFX 调参或 Scene authoring 混做。

## 8. 更新规则

只有以下稳定事实发生变化时更新本文件：

- Unity / Package / Build Settings 基础事实；
- 正式 / Sandbox 架构方向；
- 标准测试、asmdef、固定脚本或项目 Skills 已真实落地；
- 产品上下文新增或废止。

不要把每个包的开发进度、临时 PID、一次性 hash 或聊天回执写进本文件。
