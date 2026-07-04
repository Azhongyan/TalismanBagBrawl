# V0.4-BattleSandboxBuildCombatPreview01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_BUILD_COMBAT_PREVIEW01
```

## 1. 包定位

```text
V0.4 沙盒 Build 战斗反馈预览包
```

本包承接已通过内容：

```text
V04 sandbox x2 / x3 / x4 摆放交互主干
Enemy / Boss devOnly 数据
Boss 状态 / 技能短句 / 施法条 / 机制浮字可见性
BuildSandbox 词条 / 羁绊 / Modifier / Benchmark / Readiness 数据底座
```

本包目标是在 `Scene_TalismanBag_V04_BattleSandboxPreview` 中，把“玩家摆好的 Build”和“当前 devOnly 敌人 / Boss 机制”连接成可看的沙盒战斗反馈预览。

注意：

```text
这不是正式战斗系统接入。
这不是正式伤害结算。
这不是正式 3-10 / 4-10。
```

## 2. 核心目标

在 V04 沙盒 battle mode 下实现：

```text
读取当前棋盘已放置道具 / shape / tags / affix preview
计算当前 Build 的 synergy / modifier / readiness preview
读取当前 devOnly enemy / boss problem
生成沙盒战斗反馈
通过现有 Boss 状态 / 施法条 / 机制浮字 / 战斗反馈文本表现出来
```

玩家侧只看现象和反馈，不看答案。

## 3. 输入来源

允许读取：

```text
当前 V04 sandbox 棋盘放置状态
BuildSandboxPreviewContext
ShapePlacementSession / board snapshot
ItemShapeOccupancy / ItemShapeConfig
SynergyEvaluator
AffixRaritySandbox preview
ModifierEventBridge
BuildProblemRulePool / BuildProblemSeedData
EnemyBossValidationPool
MechanicHintFeedbackPreview
```

允许使用当前 V04 沙盒：

```text
EnemyCombatFeedbackRuntime
EnemyCombatFeedbackPanel
EnemyCombatFeedbackFloatingRoot
Boss / enemy cast bar preview
```

## 4. 输出内容

允许输出到玩家侧：

```text
Boss 状态变化短句
Boss 技能 / 施法反馈
机制浮字
破盾 / 护盾 / 净化 / 控制 / 爆发窗口等战斗现象反馈
当前 Build 的模糊表现反馈
失败或缺口的玩家侧短句
```

示例：

```text
护盾被削弱
邪气正在聚集
净化窗口出现
控制抗性过高
爆发窗口错过
雷符触发了破盾反馈
```

禁止玩家侧直接输出：

```text
hardSolutionTags
requiredSynergy
requiredAffix
requiredStats
DropBias 权重
Boss 六钥匙完整答案
BuildReadiness 精确分数
“你需要 XX Build 才能过”
```

## 5. 开发者侧保留

允许在报告 / 开发者数据面板中输出完整数据：

```text
readiness score
missing tags
matched synergy
matched affix
modifier bundle
event bundle
weakness window match
drop bias preview
failure reason detail
```

但这些不得直接进入玩家侧 UI。

## 6. 允许修改范围

允许修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

优先修改 / 新增：

```text
BuildCombatPreview runtime / controller
BuildCombatPreview result model
BuildCombatPreview validator / report writer
EnemyCombatFeedbackRuntime 的 devOnly input adapter
```

## 7. 禁止范围

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
调用正式战斗伤害结算
改变正式敌人血量
发正式奖励
写正式存档
推进正式章节
启用正式 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

## 8. UI 与手调保护

不得重建或移动用户已经调好的：

```text
V04 棋盘
V04 道具栏
V04 道具卡
V04 Boss / enemy feedback panel 布局
```

如果需要新增按钮，优先使用最小 devOnly 控制按钮：

```text
Run Preview
Pause Preview
Reset Preview
```

不得新增大面积答案面板。

## 9. 本包不做

本包不做：

```text
正式战斗回合制 / 自动战斗循环
正式伤害数字
正式 HP 扣减
真实奖励结算
真实掉落
正式章节推进
正式 3-10 / 4-10 入口
PromoteToMainline
```

本包只做：

```text
devOnly 沙盒战斗反馈预览
```

## 10. 验收清单

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Play 后确认：

```text
1. x2 / x3 / x4 摆放交互不回退。
2. 当前敌人 / Boss 反馈面板仍可见。
3. 可以触发一次 BuildCombatPreview。
4. 预览会读取当前棋盘道具摆放。
5. 预览会根据当前敌人 / Boss 输出不同机制反馈。
6. Boss 施法条 / 状态短句 / 机制浮字会随预览变化。
7. 玩家侧不显示 hardSolutionTags / requiredSynergy / requiredAffix / requiredStats / DropBias 权重 / Boss 六钥匙。
8. 不进入正式战斗、不发奖励、不写存档、不推进章节。
9. Console 无本包红色 Error / 黄色 Warning。
```

## 11. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattleSandboxBuildCombatPreviewReport.md
Docs/V0.4/Reports/BattleSandboxBuildCombatPreviewRows.csv
Docs/V0.4/Reports/BattleSandboxBuildCombatPreviewLeakCheckReport.md
```

报告必须说明：

```text
preview scenario count
placed item snapshot count
synergy match count
modifier bundle count
mechanic feedback count
player-side answer leak count
formal flow leak count
FeatureFlag default status
devOnly / isEnabled isolation status
```

## 12. 通过后下一包

本包通过后进入：

```text
V0.4-DevChapterBalanceRun01
```

下一包才允许把 devOnly 3-10 / 4-10 难度曲线和调参验证流接进来。
