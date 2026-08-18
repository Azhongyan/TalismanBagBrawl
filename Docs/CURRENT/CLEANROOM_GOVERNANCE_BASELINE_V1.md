# Cleanroom Governance Baseline V1

状态：`ACTIVE / GOVERNANCE_ESTABLISHED / CLEANROOM_HEALTH_NOT_YET_GREEN`

生效日期：`2026-08-11`

用途：在 Cleanroom 收尾后防止工程重新出现第二真相、平行 Owner、Formal 反向依赖 Sandbox、无期限兼容层和长期双轨。本文件是短期可执行边界，不取代产品 Task Contract、玩家正常路径验收或 `PROJECT_CLEANROOM_COMPONENT_LEDGER.md`。

## 1. Architecture Constitution

1. 一个业务事实只能有一个 Formal Owner。
2. Scene、Prefab、Presenter 和 VFX 只负责装配、交互与表现，不拥有伤害、奖励、库存、摆放、关卡或存档真相。
3. Prefab 不直接查询数据库；正式数据经现有 Resolver、Snapshot 或 ViewModel 进入 Presenter Bind。
4. Formal Runtime 不得依赖 Sandbox Runtime。允许 Formal 数据和合同供 Sandbox 做实验输入。
5. Reward 只负责抽取、授予与去重，不复制第二份 Item 静态事实或 Battle Fact。
6. Battle 只消费正式 Snapshot 和 Live State，不按 itemId 回查旧 Pool、Starter、Candidate 或 Presentation Profile 决定玩法。
7. Save 只持久化已授权事实，不决定流程、奖励或战斗结果。
8. Cutover 完成必须包含正式消费者迁移、旧正式引用归零、旧职责退休和 Ledger 更新；旧职责仍在时只能标记 `REPLACE`。
9. 新建 Catalog、Database、Authority、Session、Manager、Provider、Repository、Bridge、Adapter、RuntimeState 或 Save 级对象前必须先做 Owner Review。
10. Temporary、Fallback、Compatibility 和 Sidecar 必须登记 Owner、Purpose、CreatedByTask 与 DeleteWhen。
11. Hygiene Gate 只读报告，不自动修复、重构或创建兼容层。
12. Player Ready 只能由正常玩家路径 Fresh Play 或用户真实路径证据证明；编译、静态 Profile、Cue 和局部验证不能替代。

## 2. CURRENT Owner Registry

| Responsibility | Formal Owner / Carrier | Formal Consumers | Status |
| --- | --- | --- | --- |
| Item Static Facts | `Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset`，经 `CanonicalItemCatalog` 编译/解析 | Reward、Detail、Authority、Battle feeder、Presentation resolver | `KEEP / CANONICAL` |
| Reward Pool / Roll Policy | `C1CampaignDirectLootPoolAndPolicy` | `C1CampaignDirectLootSessionResolver` | `KEEP / CANONICAL INPUT ONLY` |
| Reward Grant / Entitlement | `C1CampaignDirectLootSessionResolver` + `C1FormalCampaignLootEntitlementAcceptance` | Item Authority | `KEEP / REPLACE LEGACY OVERLOADS` |
| Owned Item Instance + Formal Placement | `C1FormalItemSessionAuthority` | Exact Board、Tray、Battle snapshot | `KEEP / FORMAL OWNER` |
| Battle Live State / Mutation | `C1FormalRealtimeBattleSession` | Battle Presenter、Terminal、Cue consumers | `KEEP / FORMAL OWNER / REPLACE LEGACY FEEDER` |
| Stage Completion Persistence | `V04CampaignStageCompletionRepository` | WorldMap、continuous stage flow | `KEEP / FORMAL OWNER` |
| Formal Presentation Binding | `UnifiedBattleFormalSceneHost` + existing formal Presenters | Shared Scene/Prefab/VFX carriers | `KEEP / CONSUMER ONLY` |
| BattleSandbox / ItemSandbox / BuildSandbox | Lab runtime and tooling only | Dev experiments and quarantined verification | `QUARANTINE / NO FORMAL TRUTH` |

Owner Registry 只记录稳定职责。每个包的临时进度、单一道具状态和一次性验证结果继续写入 Task Contract 或 Ledger，不写入本表。

## 3. Task Modes

### 装修（默认）

适用于 UI、Prefab 表现、动画、VFX、交互手感和现有参数调整。

- `Expected New Owner = 0`
- `Expected New Data Source = 0`
- `Expected New Fallback = 0`
- 必须在现有 Owner 的公开边界内完成。
- 如果必须改变 Truth、Owner 或核心 Contract，立即停止并申请升级为“水电”。

#### 装修视觉资产最低交付规则

- 凡承担图标、纹样、边框、按钮、状态、技能、Buff/Debuff、战斗反馈或其他图像识别职责的 UI，必须使用真实 `Image / Sprite / Visual Prefab`；不得用 Unicode 字符、TMP 字形、字段名、缩写或单纯色块代替应有的美术图像。
- `statusKey`、`triggerKind`、`resultKind`、`styleKey` 等字段只负责精确路由到视觉资产，不得被当作视觉资产本身。精确映射缺失时应在装修边界报告缺图，禁止回退为字符占位。
- 装修 Owner 必须在施工前主动审计与当前视觉职责相关的工程图片、Visual Prefab、共享组件、视觉母版和用户手调槽位，并自行判断“原位复用、基于母版扩展或补充新素材”。不得等待用户逐项指出已有素材，也不得把素材搜索和复用判断转交给用户。
- 找到职责和风格都匹配的现有载体时优先原位复用；只有职责、构图或视觉语义不匹配时才补充新素材。不得为了省事绕开已有成熟方案另做字符表现，也不得为了形式上的复用强行使用语义错误的图片。
- 如果经过相关范围审计仍没有合适素材，但该表现属于装修范围，装修 Owner 必须自行生成或制作一张可替换的美术素材占位，再完成绑定。占位图也必须具备可用构图、风格、颜色语义和真实透明通道；不得把棋盘格、白底或临时字符烘进图片。
- 除非缺少会改变产品美术方向的关键决定，素材复用、占位生成和初步视觉选择均由装修 Owner 负责，不作为常规用户阻塞点。
- 新素材只承载表现，不得拥有或推断 Battle、Item、Reward、Status 等玩法真相；后续替换美术时不得要求修改水电事实或玩家手调布局。
- 用户在非 Play 状态手调完成的 RectTransform、大小、间距、对齐和 sibling order 继续作为版面真源。补图和换图只写入既有图片槽，除非用户明确要求重新排版。

### 水电（显式）

适用于 Canonical 数据结构、Authority、Session、Reward、Save、正式 Bridge 或跨系统状态变更。

开始前必须回答：现有 Owner 是谁、为什么不能原位扩展、新职责是什么、谁消费、长期还是临时、临时结构何时删除。

### 检查房间（只读）

运行 `scripts/check_project_hygiene.ps1`。只报告当前风险，不修改工程。

## 4. Temporary Debt Contract

任何临时结构必须在 Task Contract 或 Ledger 中登记：

| Field | Required meaning |
| --- | --- |
| Owner | 谁对临时结构负责 |
| Purpose | 为什么当前必须存在 |
| CreatedByTask | 哪个任务引入 |
| DeleteWhen | 哪个可验证条件满足后删除 |

没有 `DeleteWhen` 的 Fallback、Compatibility 或 Sidecar 不得进入正式路线。

## 5. Project Hygiene Gate

从工程根目录运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\check_project_hygiene.ps1
```

检查本次任务写域时传入路径：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\check_project_hygiene.ps1 -ScopePath Assets/_Game/Scripts/TalismanBag/UnifiedBattle,Docs/CURRENT
```

Gate 只检查高信号边界：治理文件是否存在、Formal→Sandbox 显式依赖、Scene/Prefab Missing Script、项目 C# 中仍存在的禁用摘要逻辑（按责任域聚合为一条警告）、当前 Ledger 债务摘要，以及任务写域中新 Owner 和无 DeleteWhen 临时结构的人工复核提示。

它不检查产品语义，不自动断言第二真相已清零，不调用 Unity，也不自动改代码。

## 6. Proportional Schedule

- 小型装修：不强制全工程 Gate；任务结束确认没有改变 Owner、数据源或 Fallback。
- 中型以上开发包：运行一次 scoped Hygiene Gate，并同步 Ledger。
- 里程碑结束：运行一次全局 Gate，复核 Owner、Second Truth、Formal→Sandbox、REPLACE 与临时结构。
- 版本结束：短 Cleanroom Audit + 正常路径 Fresh Play。

## 7. ROOM CHECK

中型以上任务结束只保留以下短回执：

```text
ROOM CHECK
Task Mode: 装修 / 水电
New Owner: 0 / N
New Data Source: 0 / N
New Fallback: 0 / N
Formal -> Sandbox: 0 / N
Pending Retirement: 0 / N
Room Status: CLEAN / WARNING / TEMPORARY_DEBT
```

若不是 `CLEAN`，只写第一个真实原因及其 `DeleteWhen`，不堆叠机械 Rubric。

## 8. Baseline Health Meaning

治理基线建立不等于工程已经清扫完成。只有以下指标经当前代码、序列化消费者、Ledger 和正常路径共同证明为零，才能把 Cleanroom 标为 CLEAN：

- Second Truth Count
- Formal → Sandbox Dependency
- Unowned Fallback
- Expired REPLACE
- Unknown Critical Owner

当前已有 `REPLACE`、`QUARANTINE` 和 Formal→Sandbox 债务仍按 Ledger 继续闭账；不得为了得到绿色结果隐藏、改名或补兼容层。
