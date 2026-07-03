# V0.4-BattleSandboxEnemyCombatFeedbackUiReuse01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_ENEMY_COMBAT_FEEDBACK_UI_REUSE01
```

## 1. 包定位

```text
V0.4 沙盒敌人 / Boss 战斗反馈 UI 复用修正包
```

本包用于纠偏 `V0.4-BattleSandboxEnemyEncounterPreview01`。

此前方向偏成了“敌人 / Boss 题目预览面板”。用户已明确纠正：

```text
Boss 题目只是开发者比喻。
玩家不是看题目和答案。
Boss 有技能，玩家通过战斗表现、机制反馈、施法条和失败反馈去摸解法。
```

因此本包不继续做题目面板。

## 2. 用户最新要求

```text
只用像 V0.2 / V0.3 中弹出伤害数字一样，加上 Boss 状态弹出机制反馈就行。
Boss 施法条还加到之前的敌人施法条上。
```

## 3. 核心目标

在 `Scene_TalismanBag_V04_BattleSandboxPreview` 中增加轻量 Boss / 敌人机制反馈：

```text
机制反馈弹字
Boss 状态反馈弹字
Boss 技能 / 施法条
敌人 / Boss 当前状态简要显示
```

表现口径：

```text
像伤害数字一样短促弹出。
像战斗提示一样给玩家感知。
不做大题目面板。
不显示完整答案。
不显示开发者字段。
```

## 4. UI 复用规则

优先只读参考 / 复用现有 V0.2 / V0.3：

```text
伤害数字弹出表现
战斗反馈文本
敌人施法条 / 意图条
BossInfo / 技能表达语言
状态图标 / Tooltip 表达语言
```

允许在 V04 沙盒场景中创建 devOnly 沙盒版本：

```text
BattleSandboxMechanicFloatingText
BattleSandboxEnemyCastBar
BattleSandboxBossStateFeedback
```

但这些必须是：

```text
devOnly
sandbox preview
视觉 / 交互语言复用正式表现
不接正式战斗链路
```

## 5. 玩家侧显示原则

玩家侧允许显示：

```text
Boss 正在蓄力
护盾变厚
邪气聚集
净化窗口出现
破盾机会
Boss 技能名
Boss 施法进度
机制反馈短句
失败原因短句
```

玩家侧禁止显示：

```text
hardSolutionTags
requiredSynergy
requiredAffix
requiredStats
DropBias 权重
Boss 六钥匙完整答案
BuildReadiness 分数
“你需要 XX Build 才能过”
```

一句话：

```text
玩家看到战斗现象，不看到解题答案。
```

## 6. 开发者数据保留位置

以下内容仍可保留，但只能在开发者侧：

```text
BuildTuningDataPanelPreview
Reports
CSV
Config stable keys
```

包括：

```text
Boss 六钥匙
requiredSynergy
requiredAffix
requiredStats
DropBias 权重
Readiness 缺口
完整失败归因
```

## 7. 允许修改范围

允许修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许只读参考：

```text
V0.2 / V0.3 DamageText
V0.2 / V0.3 敌人施法条 / 意图条
V0.2 / V0.3 BossInfoPanel 文案结构
V0.2 / V0.3 战斗反馈 / Tooltip 语言
```

## 8. 禁止范围

禁止修改：

```text
Scene_TalismanBag_V02_FormationCounter
Scene_TalismanBag_V03_MainHome
Scene_TalismanBag_V03_TalismanUpgrade
正式 RunFlow / PageState / FormationState
正式 SaveData / PlayerPrefs / MainTrialProgressData
正式 Boss / 奖励 / 掉落 / 数值
正式 EnemyDefinition / V02RunConfig / DropTable / BossConfig
正式 V02 / V03 BattlePrepare runtime 行为
正式 V02 / V03 用户手调 RectTransform
```

禁止：

```text
接入正式 1-10 / 2-10
调用正式战斗伤害结算
发正式奖励
写正式存档
推进正式章节
默认开启 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

## 9. 必须移除 / 降级的方向

不再继续玩家侧：

```text
敌人与首领题目预览
测试目标明牌面板
刷新准备度面板
完整题目答案 UI
六钥匙 / readiness / required tags 玩家展示
```

如保留这些内容，只能降级为：

```text
开发者数据面板
报告
调参工具
```

## 10. 验收清单

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Play 后确认：

```text
1. 不再把 Boss 显示成“题目 / 答案面板”。
2. 可以看到敌人 / Boss 的战斗化显示区域。
3. Boss 施法条使用 / 接近既有敌人施法条口径。
4. Boss 状态 / 机制反馈像伤害数字或战斗提示一样弹出。
5. 机制反馈是中文短句，不是完整答案。
6. 不显示 hardSolutionTags / requiredSynergy / requiredAffix / requiredStats / DropBias 权重 / Boss 六钥匙。
7. x2 / x3 / x4 摆放交互不回退。
8. 不进入正式战斗、不发奖励、不写存档、不推进章节。
9. Console 无本包红色 Error / 黄色 Warning。
```

## 11. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackUiReuseReport.md
Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackRows.csv
Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackLeakCheckReport.md
```

报告必须说明：

```text
是否移除 / 降级玩家侧题目面板
是否使用伤害数字式反馈语言
是否接入 Boss / enemy cast bar 口径
玩家侧答案泄漏数量
正式流程泄漏数量
FeatureFlag 是否仍 false
devOnly / isEnabled 是否仍隔离
```

## 12. 通过后下一包

本包通过后再进入：

```text
V0.4-BattleSandboxBuildCombatPreview01
```

下一包才允许把：

```text
棋盘 Build
羁绊 / 词条 / Modifier
Boss 技能 / 机制反馈
```

连成沙盒战斗反馈预览。
