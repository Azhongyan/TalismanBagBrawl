# V0.4-BattlePrepareComponentAdapterPlaytest01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLEPREPARE_COMPONENT_ADAPTER_PLAYTEST01
```

## 1. 包定位

```text
V0.4 BuildSandbox → 成熟 BattlePrepare 组件的 devOnly 手感验证包
```

本包不是正式主线接入包，不是重画 UI 包。

本包目标：

```text
在 devOnly / sandbox 条件下，把 V0.4 多格、旋转、占格预览、道具形状、基础反馈接到成熟 BattlePrepare 组件的扩展点上。
验证 V0.4 数据能否吃到 V0.2 / V0.3 已调好的棋盘、道具栏、拖拽、上拉动画手感。
```

## 2. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BuildGridInteractionPreview01_UIReuseCorrection01_Assignment.md
Docs/V0.4/BattlePrepareComponentAdapter01_Assignment.md（如存在）
Docs/V0.4/Reports/BattlePrepareComponentAdapterReport.md
Docs/V0.4/Reports/BattlePrepareComponentAdapterMap.csv
Docs/V0.4/Reports/BattlePrepareComponentAdapterLeakCheckReport.md
```

## 3. 允许范围

允许新增 / 修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许只读 Survey：

```text
V03BattlePrepareInteractionController
V0.3 BattlePrepare 场景节点 / 成熟组件引用
V0.2 / V0.3 道具栏 / 棋盘 / 拖拽 / 上拉相关脚本
```

允许新增：

```text
BattlePrepareComponentAdapterPlaytestController
BattlePrepareComponentAdapterPlaytestValidator
BattlePrepareComponentAdapterPlaytestReportWriter
QA-only GuardRunner 菜单 / batch 入口
```

建议菜单：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterPlaytest01/[QA Only] Run Battle Prepare Component Adapter Playtest
```

## 4. 接入原则

必须遵守：

```text
Base Component：V0.2 / V0.3 成熟 BattlePrepare 棋盘、道具栏、拖拽、上拉动画。
Extension Layer：多格占位、旋转、形状预览、合法性反馈。
Adapter Layer：BuildSandboxPreviewContext → BattlePrepare component view model。
```

本包允许做最小 devOnly playtest：

```text
读取 V0.4 BuildSandboxPreviewContext
生成 Adapter 输出
在 sandbox / devOnly playtest 对象中调用成熟组件扩展点
验证是否能呈现成熟手感
输出报告
```

本包不得把 devOnly playtest 作为正式入口。

## 5. 禁止范围

禁止：

```text
重画 V04 棋盘
重画 V04 道具栏
重写拖拽手感
重写上拉动画
把 V04 temporary preview UI 当正式 UI
修改用户手调 RectTransform
修改 V02 / V03 正式场景布局
修改 RunFlow / PageState / FormationState
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改 V02FormationGridFrame
修改 DamageText
默认开启 FeatureFlag
devOnly = false
isEnabled = true
commit / tag / push
```

如必须触碰成熟 BattlePrepare 正式脚本，必须满足：

```text
只新增 devOnly / adapter extension seam
不改变默认正式行为
不改变原 UI 手感
不改变正式场景布局
报告中列明新增 seam
```

如果需要修改正式场景或正式 BattlePrepare 主脚本核心逻辑，必须停止并回 Guard，不得在本包内直接做。

## 6. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestReport.md
Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestMap.csv
Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestLeakCheckReport.md
```

报告必须说明：

```text
是否复用成熟 BattlePrepare 组件
是否只新增 Extension / Adapter
是否重画了 UI
是否修改了 V02 / V03 正式场景
是否覆盖用户手调 RectTransform
是否接入正式 RunFlow / SaveData / Boss / 奖励 / 数值
是否默认打开 FeatureFlag
devOnly / isEnabled 状态
用户如何手测“是否吃到成熟手感”
```

## 7. 用户手测目标

用户只需要验证：

```text
背包 / 道具栏 / 棋盘 / 拖拽 / 上拉动画是否接近 V0.2 / V0.3 已调好的手感。
V0.4 多格 / 旋转 / 占格反馈是否能作为扩展层出现。
没有出现另一套新画 UI。
Console 无本包红色 Error / 黄色 Warning。
```

如果本包只能完成静态 playtest / 报告，必须如实标注，不得声称已完成真实手感验证。

## 8. 下一步关系

本包通过后再进入：

```text
V0.4-MechanicHintFeedbackPreview01
```

MechanicHint 后续也必须复用成熟战斗提示 / Tooltip / BossInfo 语言，不得新画大量机制 UI 框。
