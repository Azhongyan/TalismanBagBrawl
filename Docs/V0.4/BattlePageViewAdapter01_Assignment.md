# V0.4-BattlePageViewAdapter01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLEPAGEVIEWADAPTER01
```

## 1. 包定位

```text
战斗整备视觉语言只读适配包
```

本包目标是为 V04 BattleSandboxPreview 提供一层只读 Adapter，让沙盒 UI 能复用“战斗整备页的视觉 / 交互语言”，但不得直接使用或修改 V02 / V03 正式场景对象。

正确理解：

```text
复用规格，不复用正式 GameObject。
只读参考，不反向写正式 UI。
优先复用既有 UI 语言，不为每个机制新增一堆框。
```

最新 Guard 补充：

```text
本包作为只读规格包已经成立。
但后续正式体验不能停留在“照规格重画 V04 UI”。
后续必须进入成熟 UI 复用源登记与 Adapter 映射：V0.4 数据 → Adapter / ViewModel → V0.2 / V0.3 成熟 UI。
```

用户核心口径：

```text
UI 能复用的就尽量复用。
Boss 技能信息、地图机制、战斗反馈、道具伤害提示等，应尽量收敛到同一套 UI 表达体系。
机制调参和完整规则应进入开发者数据面板。
玩家侧只看到线索和反馈，不直接看到答案。
中文名 + English stable key 只用于配置 / 报告 / 开发者数据面板字段层。
玩家 UI 不做中英文双语，只显示中文。
Hierarchy / GameObject / 文件名 / 脚本 key 必须使用英文稳定命名，中文只进显示文案或 displayName。
成熟 UI / 动画 / 交互手感默认复用 V0.2 / V0.3；不能复用时必须先向 Guard 说明原因。
```

## 2. 上游前置

必须读取并遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/BattleSandboxPreviewScene01_Assignment.md
Docs/V0.4/BuildSandboxPreviewContext01_Assignment.md
```

已完成前置：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
BuildSandboxPreviewContext
BuildSandboxPreviewViewModel
```

## 3. 允许新增 / 修改范围

允许新增或修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增：

```text
BattlePageViewAdapter
BattlePageViewSpec
BattlePrepareVisualSpec
BattleGridVisualSpec
ItemTrayVisualSpec
BattlePageViewAdapterValidator
BattlePageViewAdapterReportWriter
BuildSandboxGuardRunner 菜单 / batch 入口
```

建议菜单：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/BattlePageViewAdapter01/[QA Only] Run Battle Page View Adapter
```

## 4. 允许读取 / 参考

允许只读参考：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
V04 preview scene slot names
BuildSandboxPreviewContext / ViewModel
现有战斗整备页的命名和布局语言
现有战斗提示 / 伤害提示 / 战斗日志 / 状态 Tooltip / Boss 信息展示语言
```

允许只读分析正式战斗页脚本或场景文件，用于生成“规格报告”，但不得写入：

```text
Scene_TalismanBag_V02_FormationCounter
V03BattlePrepareInteractionController
V02FormationGridFrame
V02BottomOperationArea
V02PrimaryActionButtons
```

## 5. Adapter 输出

本包至少输出：

```text
BoardGrid spec
ItemTray spec
CategoryTabs spec
SelectedItemInfo spec
PlacementFeedback spec
ActionButtons spec
DataPanelDock binding slots spec
BattleHint / DamageFeedback / CombatLog / Tooltip / BossInfo reuse spec
DeveloperTuningPanel field spec with Chinese display name + English stable key
PlayerHint masking spec
```

这些输出只能作为 V04 沙盒面板 / 交互包后续使用的规格。

不得把规格写回正式战斗页。

不得把规格理解为“重新画一套正式 V04 UI”。后续若进入正式体验，必须依据：

```text
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
```

先做 source survey 与 Adapter 字段映射。

必须明确区分：

```text
Developer Tuning View：可显示完整规则、钥匙、阈值、DropBias、Readiness。
Player Hint Preview：只能显示模糊线索、战斗反馈、氛围提示、失败反馈，不显示完整答案。
```

## 6. 本包不做

本包不做：

```text
不拖拽道具
不实现多格交互
不创建正式 UI 面板
不新增大量机制专属 UI 框
不修改 V04 preview scene，除非仅添加 devOnly adapter marker 且用户允许
不修改 V02 / V03 正式场景
不修改 BuildSettings
不接正式战斗
不读取正式存档
```

## 7. 红线禁止

禁止修改：

```text
Scene_TalismanBag_V02_FormationCounter
Scene_TalismanBag_V03_MainHome
Scene_TalismanBag_V03_TalismanUpgrade
V02RunFlowController
V02FormationGridFrame
DamageText
RunFlow / PageState / FormationState
SaveData / PlayerPrefs / MainTrialProgressData
RewardConfig / DropTable / UpgradeConfig 正式表
正式 EnemyDefinition
正式 BossConfig
正式 V02RunConfig
正式 1-10 / 2-10
用户手调 RectTransform
```

禁止行为：

```text
BattlePageViewAdapter 反向写正式 UI
BattlePageViewAdapter SetParent 正式 UI
BattlePageViewAdapter 覆盖正式 RectTransform
BattlePageViewAdapter 修改 V02FormationGridFrame
BattlePageViewAdapter 修改 V03BattlePrepareInteractionController
玩家侧直接显示 hardSolutionTags / requiredSynergy / requiredAffix / requiredStats / DropBias 权重 / Boss 六钥匙完整答案
为 Boss 技能 / 地图机制 / 反馈分别新建大量不可复用 UI 框
新增中文命名的 Hierarchy GameObject
用中文对象名做查找路径 / 稳定 id
FeatureFlag 默认 true
devOnly = false
isEnabled = true
commit / tag / push
```

## 8. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattlePageViewAdapterReport.md
Docs/V0.4/Reports/BattlePageViewSpecReport.csv
Docs/V0.4/Reports/BattleUiReuseSpecReport.csv
Docs/V0.4/Reports/PlayerHintMaskingSpecReport.md
Docs/V0.4/Reports/BattlePageViewAdapterLeakCheckReport.md
```

报告必须说明：

```text
Adapter 是否可创建
输出规格数量
是否读取正式存档
是否写正式场景
是否写正式 UI
是否触碰 V02FormationGridFrame
是否触碰 V03BattlePrepareInteractionController
是否可供后续 BuildGridInteractionPreview01 使用
是否建立配置 / 报告 / 数据面板字段层的中文名 + English stable key 口径
是否确认玩家 UI 只显示中文、不做中英文双语 UI
是否证明玩家侧不会直接看到答案
是否证明机制信息可优先进入统一数据面板 / 既有反馈 UI
```

## 9. 验收标准

必须满足：

```text
C# 编译通过
BattlePageViewAdapter 菜单 / batch 可运行
报告 PASS
FeatureFlag 全 false
devOnly / isEnabled 隔离成立
正式流程泄漏检查通过
未修改 V02 / V03 正式场景
未修改 V04 Preview Scene，除非任务窗口明确报告为 devOnly marker 且用户确认
未读取正式存档
未写正式 UI
```

## 10. 用户验收建议

本包仍是规格 / Adapter 包。

用户只需确认：

```text
Unity 菜单运行无红色 Error / 黄色 Warning
BattlePageViewAdapterReport.md 显示 PASS
Spec 报告中有 BoardGrid / ItemTray / CategoryTabs / SelectedItemInfo / PlacementFeedback
UI 复用报告中有 BattleHint / DamageFeedback / CombatLog / Tooltip / BossInfo 复用建议
PlayerHintMasking 报告确认玩家侧不显示完整答案
LeakCheck 显示没有反向写正式 UI
```
