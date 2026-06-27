# CURRENT VERSION SCOPE LOCK

当前版本总蓝图：`V0.3-ProductFlow01`

当前执行方式：按 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` 小包推进，不允许把 `V0.3-ProductFlow01` 当作单个 Codex 开发任务。

权威说明：

```text
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
```

## 当前 V0.3 允许围绕

V0.3 只允许围绕首次完整产品体验流转：

```text
App 启动入口
Loading / StartGame / 服务器占位
OpeningStoryPanel
新玩家进入首场教学战斗
老玩家进入照灯小铺首页
照灯小铺首页
首页热点
底部导航：首页 / 探索 / 试练 / 锻造 / 更多
试练入口
战斗页内整备升降交互
新手整备引导
1-10 Boss 前后流转
1-10 Boss 后首次锻造 / 升级引导
回首页后进入 2-1
V0.3 产品流转总回归
```

## 当前 V0.3 正式拆包

必须按以下队列逐包推进：

```text
0. V0.2-StabilityProtocol01
1. V0.3-BootEntryFlow01
2. V0.3-MainHomeScene01-Retry
3. V0.3-BottomNavAndHomeHotspot01
4. V0.3-BattlePrepareInteraction01
5. V0.3-PrepareTutorial01
6. V0.3-BossGuideResult01
7. V0.3-ForgeFirstUpgradeGuide01
8. V0.3-ProductFlowRegression01
```

## 当前包

当前正式主线包：

```text
V0.3-BattlePrepareInteraction01
```

当前全局阻断：

```text
BUILD_SETTINGS_REGRESSION_BLOCKER
```

BootEntry 加载 MainHome 报 `Scene is missing from Build Settings`。只读确认 MainHome 场景文件存在，但 `ProjectSettings/EditorBuildSettings.asset` 当前只包含 `Scene_TalismanBag_Run15Min.unity`。必须先按 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` 中 `GUARD_PASS_FIX_BUILD_SETTINGS01` 最小修复 Build Settings，再继续 BattlePrepare 修复。

该包为高风险包，已完成 Code Survey；首轮用户手测不通过后，Guard 判断继续最小修复、不重做整包。当前只能按 `Docs/V0.3/V0.3_PACKAGE_QUEUE.md` 中 `GUARD_PASS_FIX01_CONTINUE` 范围修复，不得扩展。

旧 `V0.3-TrialFlowUI01`、`V0.3-CombatContextBar01`、`V0.3-StageTransition01` 等不再作为当前正式主线包。

最近完成：

```text
V0.3-BootEntryFlow01 已开发完成、Unity smoke 通过、用户手测 QA 通过，并取得 REPOOPS_RECORD_DONE。
V0.3-MainHomeScene01-Retry 已完成 ProductFlow 复查层级修复：MainHomeRoot 恢复为 Canvas 直接子节点；用户手测通过；未触碰存档、奖励、数值、Boss、战斗整备、DataCatalog、首页热点逻辑或试炼 / 炼符 delegate。
V0.3-BottomNavAndHomeHotspot01 已开发完成并用户手测 QA 通过：底栏口径为 首页 / 探索 / 试练 / 锻造 / 更多；梦签保留为首页中部热点；背包 / BattleBackpack 未进入底栏；未触碰 RunFlow / PageState / FormationState / SaveData / Boss / 奖励 / 数值 / DamageText / V02FormationGridFrame；RepoOps 记录请求已发送，等待回执。
```

## 交付形态硬边界

- `V0.3-ProductFlow01` 是总蓝图，不是单包。
- 每个子包必须只做自己的范围。
- 不得把 BootEntryFlow、MainHome、BattlePrepare、BossGuide、ForgeGuide 混在同一个开发任务里。
- 涉及场景、页面、UI、入口或用户可见状态时，必须写清真实用户路径、默认状态、首帧可见性和重启后预期。
- 自动测试不得主动激活、反射调用、临时实例化或直接切状态来制造通过。

## 高风险包规则

以下包必须 Code Survey，并在触碰红线前回 Guard：

```text
V0.3-BattlePrepareInteraction01
V0.3-PrepareTutorial01
V0.3-BossGuideResult01
V0.3-ForgeFirstUpgradeGuide01
```

最高风险：

```text
V0.3-BattlePrepareInteraction01
```

因为它可能触碰：

```text
战斗页 Root
棋盘位置
背包显示
上拉道具栏 800×800 区域与格子化滚动展示
分类标签筛选
拖拽
FormationGrid
战斗状态切换
新手引导遮罩
```

`V0.3-BattlePrepareInteraction01` 统一把下半部分上拉窗口命名为“道具栏”，不再叫“背包 UI”。允许实现战斗整备上拉道具栏区域的 800×800 格子化 UI、区域内滚动和分类标签筛选；这不等于授权完整多格背包系统、背包经济或 SaveData 结构调整。

Fix01 允许继续修复以下失败项：

```text
废弃当前 V0.3 整备实现中对旧 V02BottomOperationArea 的显示依赖。
修正棋盘 + 道具栏上拉 / 下收衔接，使道具栏上边缘紧贴棋盘下边缘。
将黑色遮罩放在棋盘 + 道具栏背后，只做背景压暗，不挡交互。
合并 V02PrimaryActionButtons 与当前战斗底部操作口径。
隐藏或降级刷新供能 / 重置阵盘等当前整备不需要的按钮。
将用户可见“战后驻阵”口径统一为“整备”。
```

Fix01 不允许：

```text
删除或重写 V02 基线对象导致 V0.2 回退。
重写 RunFlow / PageState / FormationState。
改变战后流程语义；“战后驻阵=整备”只允许 UI 文案、入口显示和提示口径统一。
把首页底部导航直接塞进战斗页。
借底部操作区合并扩展无关入口。
```

## 当前版本明确不做

- 真实账号
- 真实服务器
- 热更新
- SDK
- 完整 CG
- 公告系统
- 正式探索玩法
- 正式天机炉玩法
- 正式三选一肉鸽
- 正式 PVP
- 正式后屋生产玩法
- 完整多格背包系统（不含 V0.3-BattlePrepareInteraction01 内的上拉道具栏区域格子化 UI 展示）
- 五行系统
- 连携技系统
- 3-10 新内容
- 完整新战斗系统
- 战斗状态机重构
- RunFlow 重构
- FormationState 重构
- Boss 流程重写
- 奖励表调整
- 数值调优
- SaveData 结构调整

超出本文件范围的需求不得被“顺手实现”。应先回报范围冲突并等待用户决定后续版本归属。
