# 《符箓背包》黄金路径 QA 锁定

## 1. V0.2 稳定基线黄金路径

每次开发后必须回归：

```text
启动当前稳定版本
进入主线
1-10 能跑
1-10 Boss 能触发
1-10 Boss 后能结算
获得关键奖励
能进入第一次符箓培养
升级后战斗实际变强
能进入 2-1
2-1 到 2-9 自动推进
2-9 后停 BossInfoPanel
2-10 Boss 不自动开打
2-10 Boss 手动触发
2-10 Boss 胜利后结算
```

---

# 2. 战斗主链路黄金检查

任何包完成后，必须检查：

```text
战斗页棋盘显示
伤害数字显示
符箓能触发
敌人会掉血
敌人死亡能结算
阵盘供能状态可见
战斗中拖拽规则符合当前版本口径
道具触发反馈正常
```

以下任一失败，均为 S 级阻断：

```text
战斗页没有棋盘
伤害数字不显示
符箓不触发
敌人不掉血
战斗不结算
Boss 前停止失效
Boss 自动开打
```

---

# 3. V0.3-ProductFlow01 完整路径

完整路径：

```text
Loading
→ StartGame
→ 服务器 / 区服占位
→ 开始游戏
→ OpeningStoryPanel
→ 首场教学战斗
→ BattleNormalState
→ 点击整备进入 BattlePrepareState
→ 整备态引导拖动道具到棋盘
→ 继续战斗
→ 1-1 到 1-10
→ BossNodeGuideState
→ BossInfoState
→ BossBattleState
→ BossResultState
→ 获得关键道具
→ 回首页
→ HomeForgeUnlockGuideState
→ ForgeUpgradeGuideState
→ 完成首次升级
→ 回首页
→ HomeTrialContinueGuideState
→ 点击试练进入 2-1
```

---

# 4. V0.3 启动入口 QA

验证：

```text
LoadingPage 显示
StartGamePage 显示
服务器 / 区服占位显示
开始游戏按钮可点
首次进入进入 OpeningStoryPanel
OpeningStoryPanel 可跳过
新玩家进入首场教学战斗
老玩家进入照灯小铺首页
```

不得出现：

```text
无入口直接进战斗
无入口直接进首页
开始按钮无效
跳过按钮卡死
首次进入判断导致存档异常
```

---

# 5. V0.3 首页 QA

验证：

```text
首页是照灯小铺
顶部有资源 / 主线进度 / 设置
中部有空间热点
当前目标可见
ComingSoon 入口点击不报错
梦签是首页中部热点
梦签不在底栏
背包不在底栏
```

底栏顺序必须为：

```text
首页 / 探索 / 试练 / 锻造 / 更多
```

---

# 6. V0.3 战斗整备 QA

验证：

```text
BattleNormalState 正常显示战斗
战斗页棋盘显示
伤害数字显示
点击整备进入 BattlePrepareState
道具栏从下方上拉
棋盘被顶起
战斗展示区 50% 黑遮罩
黑色遮罩位于棋盘 + 道具栏背后，不遮挡前景交互
整备道具栏显示
当前整备道具栏不使用旧 V02BottomOperationArea 显示方案
下半部分上拉道具栏 UI 目标区域为 800×800，并与上半部分棋盘 UI 规格一致
道具栏上边缘紧贴棋盘下边缘，上拉 / 下收前后没有明显断层
道具栏区域有格子化道具布局
道具栏区域可在自身区域内滚动，以容纳道具数量增多的情况
分类标签可见，并能切换筛选道具种类
刷新供能 / 重置阵盘 不作为当前整备主操作干扰用户
用户可见“战后驻阵”口径统一为“整备”
可以拖动道具到棋盘
点击继续战斗
道具栏下收
棋盘归位
遮罩消失
战斗继续
```

不得出现：

```text
进入战斗页没有棋盘
点击整备无反应
整备按钮被锁
整备后棋盘丢失
仍显示或依赖旧 V02BottomOperationArea 作为当前整备道具栏
道具栏拉出前后没有和棋盘衔接，出现明显断层
黑色遮罩盖住棋盘 / 道具栏 / 分类标签 / 拖拽 / 继续战斗按钮
上拉道具栏区域不是 800×800 或无法容纳格子化道具布局
道具数量增多时道具栏区域不能滚动
分类标签无法切换或无法筛选道具种类
刷新供能 / 重置阵盘 仍作为当前整备主操作干扰用户
用户可见入口仍把“战后驻阵”和“整备”表现成两套独立口径
伤害数字消失
拖拽导致战斗主链路断
```

---

# 7. V0.3 Boss QA

验证：

```text
1-10 Boss 节点有引导
BossInfoState 显示 Boss 信息
Boss 技能 / 威胁 / 推荐克制可见
Boss 奖励可见
Boss 需要玩家手动挑战
BossBattleState 正常
BossResultState 正常
关键道具到账
Boss 后可以回首页
```

不得出现：

```text
Boss 自动开打
BossInfoPanel 被跳过
Boss 奖励不到账
Boss 奖励重复发放
Boss 后无法回首页
```

---

# 8. V0.3 锻造 / 首次升级 QA

验证：

```text
1-10 Boss 后回首页
锻造入口解锁
首页高亮锻造入口
进入锻造页
关键道具高亮
升级按钮高亮
可以完成首次升级
升级成功反馈显示
升级后回首页
首页高亮试练入口
点击试练进入 2-1
```

注意：

```text
锻造包装现有第一次符箓培养。
不是重做完整锻造系统。
```

不得出现：

```text
升级消耗异常
资源被扣错
升级后战斗不变强
升级后无法进入 2-1
```

---

# 9. V0.3 回滚条件

出现以下任一情况，本包不得继续叠补丁，必须回滚或暂停审查：

```text
战斗页棋盘不显示
伤害数字不显示
符箓不触发
敌人不掉血
1-10 / 2-10 主流程断
2-9 后不再停 BossInfoPanel
2-10 Boss 自动开打
奖励重复发放
培养链路断
SaveData 结构异常
首页点击试练导致 ResetBattle
首页进入后清背包 / 清阵盘
整备交互导致战斗主链路断
```

---

# 10. RepoOps verified 条件

RepoOps 打 verified 前必须满足：

```text
QA 黄金路径通过
Unity Console 无 Error
git diff --check 通过
git status 清晰
unrelated dirty files 已隔离
用户手测确认通过
```

没有用户手测确认，不得打 verified tag。
