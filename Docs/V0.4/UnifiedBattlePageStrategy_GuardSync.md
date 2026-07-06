# GUARD_SYNC_UNIFIED_BATTLEPAGE_STRATEGY

日期：2026-07-06
维护方：Codex Guard / 记忆治理窗口
状态：GUARD_MEMORY_SYNC / NOT_ASSIGNMENT / NO_CODE_NO_SCENE_CHANGE

## 1. Guard 收口结论

```text
GUARD_SYNC_UNIFIED_BATTLEPAGE_STRATEGY_ACCEPTED
GUARD_HOLD_UNIFIED_BATTLEPAGE_UNTIL_FORMATION_POWER_LEGACY_FEEDBACK_FIXED
GUARD_KEEP_V03_STABLE_FLOW_AS_REFERENCE_AND_ROUTE_SOURCE
GUARD_KEEP_V04_BUILDSANDBOX_AS_NEW_BATTLE_EXPERIENCE_SOURCE
GUARD_FORBID_CONTINUING_TO_TUNE_V03_AND_V04_AS_TWO_FINAL_BATTLE_PAGES
```

本文件只记录路线判断，不等于新包 assignment，不授权修改 Unity 代码、场景、存档、奖励、数值、RunFlow 或正式章节。

## 2. 核心判断

后续不应把 `Scene_TalismanBag_V02_FormationCounter` / V0.3 战斗页和 `Scene_TalismanBag_V04_BattleSandboxPreview` 同时当成两个最终正式战斗页继续调。

正确路线是：

```text
V0.2 / V0.3 稳定战斗流转
        +
V0.4 BuildSandbox 新战斗体验
        ↓
Unified BattlePage / 统一正式战斗页
```

也就是：

```text
V0.3 取流程骨架。
V0.4 取新玩法和手感。
Unified BattlePage 承接最终正式体验。
```

## 3. V0.2 / V0.3 后续定位

V0.2 / V0.3 不再作为新玩法实验场，也不应继续投入大量 UI 手感调优作为最终页。

它后续提供：

```text
1-10 / 2-10 Golden Path
关卡推进顺序
怪物 / Boss 出场顺序
Boss 前停止
BossInfo / 手动挑战
胜利 / 失败 / 结算
奖励领取
回首页
新手流程与老道具行为参考
正式主线流转稳定性参考
```

默认禁止：

```text
为了 V0.4 新玩法大改 V02RunFlowController
为了多格道具重构 FormationState / PageState
为了新棋盘重写 V02FormationGridFrame
把 V0.4 devOnly 内容直接塞进 1-10 / 2-10 正式流
继续把 V0.3 战斗页作为最终新战斗页反复调 UI
```

## 4. V0.4 BuildSandbox 后续定位

V0.4 BuildSandbox 当前是新能力验证场，不是正式主线入口。

它后续提供：

```text
x2 / x3 / x4 / vertical_3 多格道具
新道具池与旧道具扩展能力
ItemStat / Shape / Synergy / Affix / Modifier
Build 反馈
敌人 / Boss 机制反馈
3-10 / 4-10 devOnly 可玩验证
道具栏与棋盘的新手感参考
开发者调参面板和报告
```

默认禁止：

```text
把当前 V04 sandbox 直接升为正式 BattlePage
把 devOnly 3-10 / 4-10 直接接正式章节
开启正式 FeatureFlag
写正式 SaveData / Reward / Chapter progress
把 V04 的临时反馈面板当成最终玩家 UI
继续忽视阵眼 / 聚灵石供能区域 / 旧道具行为缺口
```

## 5. Unified BattlePage 后续定位

Unified BattlePage 是未来正式战斗页，不是简单复制 V0.3，也不是直接把 V0.4 sandbox 原样上线。

它应该吸收：

```text
来自 V0.3：稳定过关流程、Boss 节点、奖励结算、回首页、老道具行为。
来自 V0.4：新棋盘 / 道具栏体验、多格道具、ItemStat、Build 反馈、Boss 机制反馈、3-10 / 4-10 验证内容。
```

它未来承接：

```text
正式战斗手感
正式棋盘 / 道具栏 / 整备页
战斗中剧情 / 引导 / Boss 台词
战斗反馈 / 施法条 / 机制浮字
新章节 3-10 / 4-10 以及后续 5-10 / 6-10 / 7-10
```

## 6. 当前不可直接进入 Unified 的原因

当前 V0.4 虽然已有可玩沙盒，但还不是可直接合入 V0.3 正式流的状态。

主要缺口：

```text
1. V0.4 棋盘还缺阵眼 / 核心格规则。
2. 聚灵石 / 醒符香 / 炉芯石等供能件还缺清晰供能区域与灵力来源表现。
3. 0.2 / 0.3 老道具行为还没有完整自然地融入 V0.4 战斗规则。
4. V0.4 战斗反馈能跑，但玩家可读性、胜负节奏、Boss 技能反馈还需要整理。
5. 当前 V0.4 是 devOnly sandbox，不应直接接正式 Save / Reward / Chapter。
```

因此 Guard 判断：先补 V0.4 战斗底座缺口，再进入 Unified BattlePage。

## 7. 推荐后续包顺序

### 7.1 V0.4-FormationCoreAndPowerRange01

目标：补齐阵眼、聚灵石供能区域、供能范围、灵力来源、供能反馈。

做：

```text
阵眼 / 核心格数据
聚灵石供能范围
道具是否被供能的可视反馈
灵力不足 / 供能成功 / 供能断开反馈
只在 V0.4 sandbox 内验证
```

不做：

```text
不接正式 SaveData
不改 V0.3 RunFlow
不改正式奖励 / 数值主线
不重画整套 UI
```

### 7.2 V0.4-LegacyItemBehaviorIntegration01

目标：把 V0.2 / V0.3 基础道具行为按家族关系接入 V0.4 sandbox。

做：

```text
火符 / 雷符 / 驱邪铃 / 净化符 / 镇魂符 / 桃木牌 / 聚灵石等基础道具行为
V0.4 进阶道具作为基础道具能力扩展，而不是重复道具
同 family 下 base / advanced / branch 的显示与战斗效果区分
```

不做：

```text
不替换正式 itemId
不改正式掉落
不写正式背包 / 存档
不把 devOnly preview ID 当正式 ID
```

### 7.3 V0.4-BattleFeedbackReadability01

目标：整理玩家侧战斗反馈，让 Boss 技能、机制、道具效果、供能状态都能读懂，但不泄漏完整答案。

做：

```text
Boss 状态 / 施法条 / 机制浮字
道具触发反馈
伤害 / 护盾 / 治疗 / 净化 / 控制反馈
失败原因提示
Build 有效 / 无效的模糊反馈
```

不做：

```text
不显示 hardSolutionTags
不显示 DropBias 权重
不显示 Boss 六钥匙完整答案
不新增大量独立 UI 框，优先复用战斗反馈语言
```

### 7.4 V0.4-UnifiedBattlePageShell01

目标：新建或指定统一正式战斗页壳，只搭承载结构，不接正式主线。

做：

```text
BattlePageRoot
BoardArea
ItemTrayArea
EnemyInfo / BossCastBar
BattleFeedbackLayer
Story / Guide / Popup Layer
Result / Reward placeholder
V0.3 flow adapter slot
V0.4 sandbox adapter slot
```

不做：

```text
不直接替换 V0.3 FormationCounter
不接正式章节入口
不写存档 / 奖励 / 章节推进
不把 V04 sandbox 临时 UI 原样当正式 UI
```

### 7.5 V0.4-LegacyChapterBattleAdapter01

目标：把 1-10 / 2-10 的关卡、Boss、奖励、胜负流转以 adapter 方式映射到 Unified BattlePage。

做：

```text
只读读取 V0.3 稳定流程数据
关卡 / Boss / 奖励 / 结算路径映射
不改原 V0.3 Golden Path
```

### 7.6 V0.4-BattleRouteBridge01

目标：让首页 / 试炼入口可以在测试模式进入 Unified BattlePage，但默认不替换正式线。

做：

```text
dev/debug route
FeatureFlag 默认 false
手测路径
回退路径
```

### 7.7 V0.4-GoldenPathBridgeRegression01

目标：统一页接入前做全链路回归。

验收：

```text
1-10 / 2-10 旧流程不坏
V0.4 sandbox 可玩不坏
Unified BattlePage 测试入口可进可退
无 SaveData / Reward / Chapter 意外写入
无 V0.3 / V0.4 双正式战斗页并行污染
```

## 8. 后续内容维护口径

以后加怪物、加道具、加章节、加剧情时按以下归属处理：

```text
新道具 / 新机制 / 新敌人题库：先进入 V0.4 BuildSandbox / devOnly 数据与调参面板。
正式战斗手感 / 棋盘 / 道具栏：进入 Unified BattlePage。
首页、升级、试炼入口、产品流转：仍归 V0.3 MainHome / ProductFlow 相关页面。
战斗中剧情 / Boss 台词 / 机制提示：未来归 Unified BattlePage 的 Story / Feedback / BossCast 层。
1-10 / 2-10 稳定过关流程：继续保留为 V0.3 Golden Path 参考和回归对象。
```

## 9. 红灯条件

以下任一出现，Guard 应拦截：

```text
继续把 V0.3 和 V0.4 当两个最终战斗页分别调
在阵眼 / 供能 / 老道具行为未补齐前直接做 Unified 正式接入
V0.4 devOnly 内容写入正式 SaveData / Reward / Chapter progress
把 V0.4 sandbox 直接接正式 3-10 / 4-10
为了 Unified 直接重构 V02RunFlowController / FormationState / PageState
把玩家侧 UI 做成完整答案面板
把 hardSolutionTags / DropBias / Boss 六钥匙答案显示给玩家
开启默认 FeatureFlag
```

## 10. 一句话收口

```text
V0.3 是稳定流程来源，V0.4 是新战斗体验来源；二者不再各自成为最终页。后续先补 V0.4 的阵眼、供能、老道具行为和反馈可读性，再新建 Unified BattlePage，把 V0.3 流程和 V0.4 体验合成一套正式战斗页。
```
