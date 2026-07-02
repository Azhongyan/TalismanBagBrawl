# V0.4-BattleSandboxEnemyEncounterPreview01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLESANDBOX_ENEMY_ENCOUNTER_PREVIEW01
```

## 1. 包定位

```text
V0.4 独立沙盒场景敌人 / Boss 预览接入包
```

本包承接已通过的 V04 沙盒摆放主干：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
x2 / x3 / x4 shape placement
tray rotate only
valid release commit
invalid release return to tray
```

本包目标不是正式战斗接入，而是在 V04 沙盒场景中让用户能看到一个 devOnly 敌人 / Boss 题目，并为后续 BuildCombatPreview 做准备。

## 2. 核心目标

在 V04 Preview Scene 中增加：

```text
devOnly Enemy / Boss selector
当前敌人 / Boss 信息面板
地图机制 / 敌人机制 / Boss 技能中文提示
弱点窗口 / 护盾 / 负面 / 控制 / 破盾等测试目标展示
当前 Build 对该敌人的 readiness 预览入口
```

本包只显示题目与目标，不跑正式伤害结算。

## 3. 数据来源

优先读取 / 复用 V0.4 BuildSandbox 已有 devOnly 数据：

```text
EnemyBossValidationPool
BuildProblemRulePool
BuildProblemSeedData
MechanicHintFeedbackPreview
BuildSandboxPreviewContext
BuildTuningDataPanelPreview
```

允许使用：

```text
devOnly enemy profile
devOnly boss profile
map rule
enemy problem
boss problem
readiness schema
weakness window
failure hint
drop bias preview
```

## 4. 玩家侧显示原则

玩家侧 / 沙盒预览侧只显示中文线索，不显示答案。

允许显示：

```text
敌人名称 / Boss 名称
机制氛围描述
Boss 技能提示
弱点窗口提示
失败反馈文案
掉落倾向的模糊表达
当前测试目标
```

禁止显示：

```text
hardSolutionTags
requiredSynergy
requiredAffix
requiredStats
DropBias 权重
Boss 六钥匙完整答案
```

完整答案只允许留在：

```text
开发者数据面板
报告
config stable key
```

## 5. UI 规则

本包可以在 `Scene_TalismanBag_V04_BattleSandboxPreview` 中新增或绑定沙盒 UI 区块。

推荐结构：

```text
EnemyEncounterPreviewPanel
EnemySelector
EnemyInfoBlock
MechanicHintBlock
WeaknessWindowBlock
ReadinessPreviewBlock
```

命名规则：

```text
Hierarchy / GameObject / field / key 使用英文稳定命名。
玩家可见 Text 使用中文。
配置与报告字段使用 中文显示名 + English stable key。
```

如果已有 `ProblemSelector` / `BuildSandboxData` 区域可复用，优先复用，不要重复画一套职责重叠 UI。

## 6. 允许修改范围

允许修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

允许只读参考：

```text
V0.2 / V0.3 BossInfoPanel 表达语言
V0.2 / V0.3 战斗提示 / 伤害提示 / Tooltip 表达语言
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
接入正式 1-10 / 2-10
把 devOnly Boss 接进正式 Boss 列表
把 devOnly enemy 接进正式敌人池
调用正式战斗伤害结算
写正式存档 / 正式章节进度
默认开启 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

## 8. 本包不做

本包不做：

```text
正式战斗模拟
真实伤害结算
道具触发战斗效果
奖励发放
章节推进
3-10 / 4-10 正式入口
PromoteToMainline
```

这些留给后续：

```text
V0.4-BattleSandboxBuildCombatPreview01
V0.4-DevChapterBalanceRun01
V0.4-PromoteToMainline01
```

## 9. 验收清单

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

Play 后确认：

```text
1. 可以选择 / 切换 devOnly 敌人或 Boss。
2. 当前敌人 / Boss 信息能显示。
3. 地图机制 / 敌人机制 / Boss 技能以中文线索显示。
4. 弱点窗口 / 护盾 / 负面 / 控制 / 破盾等测试目标能显示。
5. 玩家侧没有直接显示 hardSolutionTags / requiredSynergy / requiredAffix / requiredStats / DropBias 权重 / Boss 六钥匙完整答案。
6. 当前 x2 / x3 / x4 摆放交互不回退。
7. 不触发正式战斗、正式奖励、正式存档或正式章节。
8. Console 无本包红色 Error / 黄色 Warning。
```

## 10. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattleSandboxEnemyEncounterPreviewReport.md
Docs/V0.4/Reports/BattleSandboxEnemyEncounterRows.csv
Docs/V0.4/Reports/BattleSandboxEnemyEncounterLeakCheckReport.md
```

报告必须说明：

```text
devOnly enemy / boss count
displayed hint count
masked answer field count
whether formal flow leak is zero
whether V02 / V03 scene changes are zero
whether FeatureFlags remain false
```

## 11. 通过后下一包

本包通过后进入：

```text
V0.4-BattleSandboxBuildCombatPreview01
```

下一包才允许把：

```text
当前棋盘 Build
Synergy / Affix / Modifier
devOnly enemy / boss problem
```

连接成沙盒战斗反馈预览。
