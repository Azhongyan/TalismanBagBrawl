# V0.4-BuildGridInteractionPreview01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDGRIDINTERACTIONPREVIEW01
```

最新 Guard 追加裁定：

```text
GUARD_DOWNGRADE_BUILDGRIDINTERACTIONPREVIEW01_UI_TO_TEMP_PREVIEW
GUARD_REQUIRE_UI_REUSE_CORRECTION_BEFORE_ANY_MAINLINE_PROMOTION
```

本包不整包重做，但其 UI 定位已调整：

```text
保留多格 / 拖拽 / 旋转 / 越界 / 重叠 / 分类筛选 / PreviewContext 逻辑验证价值。
V04 新建棋盘、背包、上拉动画、道具信息弹窗、ProblemSelector、BuildSandboxData 均视为 temporary preview。
不得继续把这些临时 UI 精修为正式 UI 真源。
后续正式手感必须复用 V0.2 / V0.3 成熟 UI，并通过 Adapter 接入 V0.4 数据。
```

## 1. 包定位

```text
V04 独立沙盒多格道具交互预览包
```

最新定位补充：

```text
本包是逻辑验证包，不是正式 UI / 手感定版包。
```

本包目标是在 `Scene_TalismanBag_V04_BattleSandboxPreview` 中实现 devOnly 的中间预览交互：

```text
道具栏滚动
分类筛选
多格道具预览
拖拽放置
旋转
越界检测
重叠检测
格子占用反馈
PlacementFeedback 中文提示
```

本包只服务 V04 独立沙盒预览，不接正式战斗，不改 V02 / V03 正式场景。

## 2. 上游前置

必须读取并遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/BattleSandboxPreviewScene01_Assignment.md
Docs/V0.4/BuildSandboxPreviewContext01_Assignment.md
Docs/V0.4/BattlePageViewAdapter01_Assignment.md
```

已完成前置：

```text
Scene_TalismanBag_V04_BattleSandboxPreview
BuildSandboxPreviewContext / ViewModel
BattlePageViewAdapter 只读规格
```

## 3. 允许新增 / 修改范围

允许新增或修改：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增：

```text
BuildGridInteractionPreviewController
BuildGridPreviewSlotView
BuildItemTrayPreviewView
BuildItemPreviewCardView
BuildPlacementFeedbackView
BuildGridInteractionPreviewValidator
BuildGridInteractionPreviewReportWriter
BuildSandboxGuardRunner 菜单 / batch 入口
```

建议菜单：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/BuildGridInteractionPreview01/[Writes Scene][Manual Only] Bind Grid Interaction Preview
Tools/Talisman Bag/V0.4/BuildSandbox/BuildGridInteractionPreview01/[QA Only] Run Grid Interaction Preview
```

## 4. 必须复用的沙盒结构

本包只能绑定 / 使用 V04 Preview Scene 中已有插槽：

```text
BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/BoardGridPreview
BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/ItemTrayPreview
BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/SelectedItemInfo
BuildSandboxPreviewCanvas/SafeAreaRoot/BattleLikePreviewArea/PlacementFeedback
BuildSandboxPreviewCanvas/SafeAreaRoot/DevOnlyControlBar
```

不得新建大量 UI 框。

本包已创建或后续需要创建的 V04 Board / ItemTray / Popup 等 UI，只能作为 temporary preview。不得要求用户在此包内把背包上拉、棋盘布局、道具栏滚动、道具信息弹窗手感调到正式品质。

如确实缺少必须节点：

```text
Editor 手动 Builder 可补 V04 沙盒场景节点。
Runtime / Play 不得静默重建一套 UI。
```

## 5. 交互要求

至少实现：

```text
5x8 道具栏预览
可见 5 行，内部滚动
分类筛选：全部 / 符箓 / 法器 / 材料 / 消耗 / 特殊
至少 4 种 shape：Single1 / Vertical2 / Corner3 / Square4
拖拽到 BoardGridPreview
占格预览
越界反馈
重叠反馈
旋转反馈
合法放置反馈
重置预览
中文 PlacementFeedback 文案
```

本包玩家侧 UI 只显示中文。

English stable key 只允许出现在：

```text
配置
报告
开发者数据面板字段层
```

Unity Hierarchy 命名必须使用英文稳定名：

```text
GameObject / Slot / Panel / Root / Button / File / Class / Field 使用英文 stable name。
中文只进入 Text 文案、配置 displayName、报告说明。
不得新增中文命名的 Hierarchy 对象。
```

## 6. 数据要求

允许读取 / 调用：

```text
BuildSandboxPreviewContext
BuildSandboxPreviewViewModel
GridOccupancyMap
ItemShapePlacementValidator
ItemShapeConfig seed
BattlePageViewAdapter spec
```

不得读取正式 SaveData / PlayerPrefs / MainTrialProgressData。

不得写正式系统。

## 7. 本包不做

本包不做：

```text
不实现完整 BuildDataPanel
不显示完整 Boss 六钥匙答案
不实现正式战斗效果
不接 Modifier / Event 到正式战斗
不做 Enemy / Boss 问题选择 UI
不做 DevChapter 连跑
不接正式掉落 / 奖励 / 存档
不把 V04 临时 Board / ItemTray / Popup 当作正式 UI 真源
不继续精修 V04 新建背包上拉动画 / 棋盘 / 道具栏正式手感
不长期维护新的 V04 ItemInfoPopup 作为正式弹窗
```

## 8. 红线禁止

禁止修改：

```text
Scene_TalismanBag_V02_FormationCounter
Scene_TalismanBag_V03_MainHome
Scene_TalismanBag_V03_TalismanUpgrade
ProjectSettings/EditorBuildSettings.asset，除非用户另行批准
V02RunFlowController
V02FormationGridFrame
DamageText
V03BattlePrepareInteractionController
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
把 V04 沙盒拖拽接入正式战斗页
复用正式 GameObject 而不是复用规格
Runtime 静默重建 UI
玩家侧显示 hardSolutionTags / requiredSynergy / requiredAffix / requiredStats / DropBias 权重 / Boss 六钥匙完整答案
新增中文命名的 Hierarchy GameObject
用中文对象名做查找路径 / 稳定 id
仿制 V0.2 / V0.3 已成熟 UI
绕过成熟 UI 复用源登记表直接新画相似 UI
在未做 source survey 与 Adapter 映射前，把 V04 沙盒 UI 推进到正式体验
FeatureFlag 默认 true
devOnly = false
isEnabled = true
commit / tag / push
```

## 8.1 后续 UI 复用纠偏

本包完成后必须进入或等待以下纠偏包，不得直接把 V04 沙盒 UI 当正式方向继续开发：

```text
V0.4-BuildGridInteractionPreview01-UIReuseCorrection01
```

纠偏包目标：

```text
审计本包临时 UI
登记 V0.3 BattlePrepare / V0.2 item info / battle feedback 成熟复用源
建立 V0.4 BuildSandbox 数据到成熟 UI 的 Adapter 字段映射
明确哪些 V04 UI 仅为 temporary preview
```

## 9. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BuildGridInteractionPreviewReport.md
Docs/V0.4/Reports/BuildGridInteractionPlacementReport.csv
Docs/V0.4/Reports/BuildGridInteractionUiBindingReport.csv
Docs/V0.4/Reports/BuildGridInteractionLeakCheckReport.md
```

报告必须说明：

```text
V04 Preview Scene 是否被绑定
BoardGridPreview 是否存在
ItemTrayPreview 是否存在
Slot 数量
Shape 数量
拖拽/旋转/越界/重叠/合法放置样例结果
是否写 V02 / V03 正式场景
是否修改 BuildSettings
是否读取正式存档
玩家侧是否只中文
Hierarchy 是否全部为英文稳定命名
是否隐藏完整答案
```

## 10. 验收标准

必须满足：

```text
C# 编译通过
Unity 菜单 / batch 可运行
报告 PASS
FeatureFlag 全 false
devOnly / isEnabled 隔离成立
正式流程泄漏检查通过
V04 Preview Scene 可打开
用户能在 V04 沙盒场景里拖放 / 旋转 / 看到格子反馈
不影响 V02 / V03 正式场景
不影响正式战斗
```

## 11. 用户手测建议

用户打开：

```text
Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity
```

在 Play 或编辑器预览中确认：

```text
道具栏显示
能滚动
能切分类
能拖道具到棋盘
Single / Vertical / Corner / Square 形状占格可见
越界有中文反馈
重叠有中文反馈
合法放置有中文反馈
重置按钮可用
Console 无本包红色 Error / 黄色 Warning
V02 / V03 正式场景未受影响
```
