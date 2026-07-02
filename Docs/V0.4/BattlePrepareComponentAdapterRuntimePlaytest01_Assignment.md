# V0.4-BattlePrepareComponentAdapterRuntimePlaytest01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLEPREPARE_COMPONENT_ADAPTER_RUNTIME_PLAYTEST01
```

## 1. 包定位

```text
V0.4 BuildSandbox → 成熟 BattlePrepare 组件的 devOnly 运行时手感验证包
```

本包必须真实进入 Unity Play 可摸路径。

本包不是静态报告包，不是正式主线接入包，不是重画 UI 包。

目标：

```text
在 devOnly / sandbox 条件下，让用户实际体验：
V0.4 多格 / 旋转 / 占格反馈
是否能吃到 V0.2 / V0.3 成熟 BattlePrepare 棋盘、道具栏、拖拽、上拉动画手感。
```

## 2. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/BattlePrepareComponentAdapterPlaytest01_Assignment.md
Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestReport.md
Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestMap.csv
Docs/V0.4/Reports/BattlePrepareComponentAdapterPlaytestLeakCheckReport.md
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
V0.3 BattlePrepare 成熟组件
V0.2 / V0.3 道具栏 / 棋盘 / 拖拽 / 上拉相关脚本和场景节点
```

允许新增：

```text
BattlePrepareComponentAdapterRuntimePlaytestController
BattlePrepareComponentAdapterRuntimePlaytestLauncher
BattlePrepareComponentAdapterRuntimePlaytestValidator
BattlePrepareComponentAdapterRuntimePlaytestReportWriter
QA-only / Manual-only Unity 菜单入口
```

建议菜单：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[Manual Only] Open Runtime Playtest
Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[QA Only] Run Runtime Playtest Validation
```

## 4. 必须真实可摸

本包必须提供用户可执行路径：

```text
打开指定场景或菜单
进入 Play
打开 devOnly BattlePrepare runtime playtest
看到成熟 BattlePrepare 背包 / 棋盘 / 拖拽 / 上拉手感
看到 V0.4 多格 / 旋转 / 占格反馈作为扩展层出现
```

如果无法做到真实 Play 可摸，必须返回：

```text
RUNTIME_PLAYTEST_BLOCKED
```

并写明阻塞原因，不得只用静态报告冒充通过。

## 5. 接入原则

必须遵守：

```text
Base Component：V0.2 / V0.3 成熟 BattlePrepare 棋盘、道具栏、拖拽、上拉动画。
Extension Layer：多格占位、旋转、形状预览、合法性反馈。
Adapter Layer：BuildSandboxPreviewContext → BattlePrepare component view model。
```

允许新增 devOnly seam：

```text
只用于 runtime playtest
默认关闭
不进入正式流程
不改变正式默认行为
一键可停用或仅由菜单 / devOnly launcher 触发
```

## 6. 禁止范围

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

若必须触碰成熟 BattlePrepare 正式脚本，必须满足：

```text
只新增 devOnly / adapter extension seam
不改变默认正式行为
不改变原 UI 手感
不改变正式场景布局
报告中列明新增 seam
```

若必须修改正式场景或正式 BattlePrepare 核心流程，必须停止并回 Guard。

## 7. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattlePrepareComponentAdapterRuntimePlaytestReport.md
Docs/V0.4/Reports/BattlePrepareComponentAdapterRuntimePlaytestSteps.md
Docs/V0.4/Reports/BattlePrepareComponentAdapterRuntimePlaytestLeakCheckReport.md
```

报告必须说明：

```text
用户如何打开 runtime playtest
是否真实进入 Unity Play 可摸
是否复用成熟 BattlePrepare 组件
是否只新增 Extension / Adapter
是否重画 UI
是否修改 V02 / V03 正式场景
是否覆盖用户手调 RectTransform
是否接入正式 RunFlow / SaveData / Boss / 奖励 / 数值
FeatureFlag / devOnly / isEnabled 状态
如果被阻塞，阻塞原因是什么
```

## 8. 用户手测清单

用户手测时确认：

```text
1. 能按报告步骤打开 runtime playtest。
2. Play 中能看到 / 操作成熟 BattlePrepare 风格的背包、棋盘、拖拽、上拉动画。
3. V0.4 多格 / 旋转 / 占格反馈作为扩展层出现。
4. 没有另一套新画 UI 取代成熟组件。
5. Console 无本包红色 Error / 黄色 Warning。
6. 退出 Play 后正式 V02 / V03 场景和用户手调 UI 没有被覆盖。
```

## 9. 下一步关系

本包通过后再进入：

```text
V0.4-MechanicHintFeedbackPreview01
```

如果本包失败，不得继续 MechanicHint；必须先回收手感验证问题。
