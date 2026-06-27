# 《符箓背包》版本路线图

## 1. 当前稳定基线

当前稳定基线：

```text
V0.2-Tune01 后稳定版本
```

该版本已经证明：

```text
1-10 / 2-10 核心战斗闭环能跑
奖励、第一次培养、Boss 前停止、Boss 手动触发成立
```

必须保护：

```text
战斗棋盘显示
伤害数字显示
道具触发
阵盘供能
1-10 / 2-10 主线闭环
1-10 Boss 章节结算
第一次符箓培养
2-9 Boss 前停止
2-10 Boss 手动触发
奖励到账
存档不丢
```

---

# 2. V0.3 总定位

```text
V0.3 = 产品基础流转版
```

V0.3 要证明：

```text
这个玩法开始像一个完整游戏产品
```

V0.3 不是系统扩张版，不追求内容量，不追求正式商业化，不追求正式美术全量接入。

V0.3 的成功标准：

```text
1. App 能启动并进入产品流程
2. 玩家看到的是照灯小铺，不是程序灰盒
3. 玩家知道当前目标
4. 玩家能从首页进入试练
5. 玩家能进入战斗
6. 玩家能进入整备
7. 战斗能打
8. Boss 前能停
9. Boss 能手动触发
10. Boss 后能回首页
11. 能解锁锻造 / 首次升级
12. 能从首页进入 2-1
13. V0.2 战斗主链路不坏
```

---

# 3. V0.3 总蓝图命名

正式总蓝图：

```text
V0.3-ProductFlow01
```

注意：

```text
V0.3-ProductFlow01 是总目标，不是单个 Codex 开发任务。
```

`MainHomeScene01-Retry` 只是其中一个子包：

```text
V0.3-MainHomeScene01-Retry = 首页空间化导航包
```

禁止让 Codex 误解为：

```text
MainHomeScene01 要一次性做完 Loading、首页、战斗整备、Boss、锻造、2-1。
```

---

# 4. V0.3 正式拆包顺序

## 0. V0.2-StabilityProtocol01

定位：

```text
V0.3 前置安全包
```

说明：

```text
这不是 V0.3 功能包，但必须在 V0.3 之前执行。
```

目标：

```text
确认 V0.2 稳定基线
建立 AGENTS.md / Docs/LOCKED
锁 CoreLoop01、RunFlow、Formation、DamageText、BossInfoPanel
重跑 V0.2 黄金路径
```

不做任何 V0.3 功能。

---

## 1. V0.3-BootEntryFlow01

定位：

```text
App 启动与首次进入流程包
```

实现：

```text
LoadingPage
StartGamePage
服务器 / 区服占位
开始游戏按钮
首次进入判断
OpeningStoryPanel
跳过按钮
新玩家进入首场教学战斗
老玩家进入照灯小铺首页
```

不做：

```text
真实账号
真实服务器
热更新
SDK
完整 CG
公告系统
```

---

## 2. V0.3-MainHomeScene01-Retry

定位：

```text
照灯小铺首页空间化导航包
```

实现：

```text
照灯小铺首页
顶部资源 / 主线进度 / 设置
中部空间热点
柜台 / 梦签柜 / 罗盘台 / 符桌 / 后屋 / 旧物架 / 师父旧物
当前目标提示
ComingSoon 入口
```

底部导航口径：

```text
首页 / 探索 / 试练 / 锻造 / 更多
```

注意：

```text
首页视觉上是照灯小铺。
底栏命名用“首页”，不是“小店”。
锻造承接 1-10 Boss 后首次升级引导。
```

---

## 3. V0.3-BottomNavAndHomeHotspot01

定位：

```text
首页热点与底部导航职责拆分包
```

实现：

```text
底部导航顺序：首页 / 探索 / 试练 / 锻造 / 更多
梦签不进底栏
背包不进底栏
梦签作为首页中部空间热点
战斗背包只属于战斗页整备流程
```

防止两个错误：

```text
1. 把梦签做成底栏入口
2. 把战斗背包做成底栏入口
```

---

## 4. V0.3-BattlePrepareInteraction01

定位：

```text
战斗页内整备升降交互包
```

风险等级：

```text
高风险
```

必须先 Code Survey，不允许 Codex 直接开写。

实现：

```text
BattleNormalState
BattlePrepareState
点击整备
道具栏从下方上拉
下半部分上拉道具栏 UI 与上半部分棋盘 UI 同规格，目标区域为 800×800
道具栏区域内使用格子化道具布局
道具栏区域内可滚动，以容纳道具数量增多的情况
分类标签可切换并筛选道具种类
棋盘被顶起
战斗展示区 50% 黑遮罩
显示整备道具栏
道具栏滚动
分类标签
拖动道具到棋盘
继续战斗
道具栏下收
棋盘归位
遮罩消失
```

可能触碰：

```text
V02FormationGridFrame
战斗页 Root
拖拽系统
FormationState
PageState
RunFlow
新手引导遮罩
```

因此必须单独拆包，不能和首页、EntryFlow 混在一起。

---

## 5. V0.3-PrepareTutorial01

定位：

```text
新手整备引导包
```

依赖：

```text
必须在 V0.3-BattlePrepareInteraction01 通过后执行
```

实现：

```text
整备态 50% 黑遮罩
高亮目标道具
道具信息窗口
高亮目标棋盘格
拖动路径提示
引导拖动道具到棋盘
放置成功反馈
```

不能提前做。

---

## 6. V0.3-BossGuideResult01

定位：

```text
1-10 Boss 前后流转包
```

实现：

```text
BossNodeGuideState
高亮挑战 Boss 按钮 / 小地图 Boss 点
BossInfoState
Boss 信息面板
Boss 技能
Boss 威胁
推荐克制
挑战奖励
带词条道具奖励位预留
BossBattleState
BossResultState
关键道具结算
继续挑战 / 回首页
```

限制：

```text
只能接现有 Boss 手动触发逻辑
不允许重写 Boss 流程
不允许破坏 2-9 Boss 前停止
不允许让 2-10 Boss 自动触发
```

---

## 7. V0.3-ForgeFirstUpgradeGuide01

定位：

```text
1-10 Boss 后首次锻造 / 升级引导包
```

实现：

```text
回首页触发锻造解锁
首页 50% 黑遮罩
高亮锻造入口
进入锻造页
高亮关键道具
道具信息窗口
高亮升级按钮
完成首次升级
升级成功反馈
回首页
高亮试练入口
进入 2-1
```

技术口径：

```text
“锻造”包装现有第一次符箓培养。
不要重做完整锻造系统。
```

---

## 8. V0.3-ProductFlowRegression01

定位：

```text
V0.3 产品基础流转整体验收包
```

验证完整路径：

```text
Loading
→ StartGame
→ 服务器占位
→ 开场剧情
→ 首场教学战斗
→ 整备引导
→ 1-10 Boss
→ Boss 信息
→ Boss 战
→ Boss 结算
→ 回首页
→ 锻造解锁
→ 首次升级
→ 回首页
→ 点击试练
→ 进入 2-1
```

同时回归 V0.2 黄金路径：

```text
棋盘显示
伤害数字
道具触发
阵盘供能
2-9 Boss 前停止
2-10 Boss 手动触发
奖励到账
存档不丢
```

---

# 5. V0.3 当前开发顺序锁定

```text
1. V0.2-StabilityProtocol01
2. V0.3-BootEntryFlow01
3. V0.3-MainHomeScene01-Retry
4. V0.3-BottomNavAndHomeHotspot01
5. V0.3-BattlePrepareInteraction01
6. V0.3-PrepareTutorial01
7. V0.3-BossGuideResult01
8. V0.3-ForgeFirstUpgradeGuide01
9. V0.3-ProductFlowRegression01
```

---

# 6. 高风险包规则

从以下包开始，必须进入 Guard 审查和 Code Survey：

```text
V0.3-BattlePrepareInteraction01
V0.3-PrepareTutorial01
V0.3-BossGuideResult01
V0.3-ForgeFirstUpgradeGuide01
```

其中最高风险是：

```text
V0.3-BattlePrepareInteraction01
```

因为它可能碰到：

```text
战斗页 Root
棋盘位置
背包显示
拖拽
FormationGrid
战斗状态切换
新手引导遮罩
```

---

# 7. V0.3 总裁决

```text
V0.3-ProductFlow01 可以作为产品基础流转总蓝图。
但不能作为单个 Codex 开发任务下发。
必须拆成多个小包逐个进入 Codex。
```
