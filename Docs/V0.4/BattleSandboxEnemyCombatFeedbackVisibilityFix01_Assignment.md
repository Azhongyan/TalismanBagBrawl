# V0.4-BattleSandboxEnemyCombatFeedbackVisibilityFix01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_ENEMY_COMBAT_FEEDBACK_VISIBILITY_FIX01
```

## 1. 包定位

```text
V0.4 沙盒敌人 / Boss 战斗反馈可见性最小修复包
```

本包是 `V0.4-BattleSandboxEnemyCombatFeedbackUiReuse01` 的最小回流修复。

当前状态：

```text
逻辑已接入。
报告 PASS。
Leak check = 0。
但 EnemyCombatFeedbackPanel 在场景中关闭。
因此战斗模式下主反馈面板不会稳定可见。
```

## 2. 本包唯一目标

只做：

```text
在 V04 sandbox battle mode 激活 EnemyCombatFeedbackPanel。
确保 Boss 状态 / 技能 / 施法条 / 机制反馈主面板在战斗模式下可见。
```

本包不扩功能，不改设计。

## 3. 允许修改范围

允许修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

但优先只改：

```text
EnemyCombatFeedbackRuntime
EnemyCombatFeedbackPanel active 逻辑
相关 validator / report
```

## 4. 明确禁止

禁止：

```text
重跑会改 UI 布局的 scene binder
移动棋盘 RectTransform
移动道具栏 RectTransform
重建 BattleSandbox 主 UI
重建 EnemyCombatFeedbackPanel
新画一套题目面板
新增答案面板
```

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
接入正式战斗
调用正式伤害结算
发正式奖励
写正式存档
推进正式章节
默认开启 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

## 5. 可见性要求

Play 后进入 / 处于 V04 sandbox battle mode 时：

```text
EnemyCombatFeedbackPanel active = true
EnemyCombatFeedbackFloatingRoot active = true
Boss 状态短句可见
Boss 技能短句可见
Boss 施法条可见且会填充 / 倒计时
机制反馈浮字可见
```

退出或非 battle mode 时：

```text
允许隐藏面板
不得影响棋盘 / 道具栏布局
```

## 6. 不做内容

本包不做：

```text
新 Boss 技能
真实伤害
真实血条扣减
奖励
存档
章节推进
BuildCombat 计算接入
```

这些留给：

```text
V0.4-BattleSandboxBuildCombatPreview01
```

## 7. 验收清单

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Play 后确认：

```text
1. 进入 V04 sandbox battle mode 后，EnemyCombatFeedbackPanel 可见。
2. Boss 状态 / 技能短句可见。
3. Boss 施法条可见，并有填充 / 倒计时变化。
4. 机制反馈浮字可见。
5. 棋盘 / 道具栏 UI 位置不被移动。
6. 不出现题目答案面板。
7. 不进入正式战斗、不发奖励、不写存档、不推进章节。
8. Console 无本包红色 Error / 黄色 Warning。
```

## 8. 报告要求

至少更新：

```text
Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackUiReuseReport.md
Docs/V0.4/Reports/BattleSandboxEnemyCombatFeedbackLeakCheckReport.md
```

报告必须明确：

```text
EnemyCombatFeedbackPanel visible in sandbox battle mode
EnemyCombatFeedbackFloatingRoot visible
No layout rebinding
No formal flow leak
No answer panel
```

## 9. 通过后下一包

本包通过后进入：

```text
V0.4-BattleSandboxBuildCombatPreview01
```
