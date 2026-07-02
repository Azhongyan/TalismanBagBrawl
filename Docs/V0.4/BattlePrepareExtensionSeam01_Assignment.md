# V0.4-BattlePrepareExtensionSeam01 Assignment

Guard 回执：

```text
GUARD_PASS_BATTLEPREPARE_EXTENSION_SEAM01
```

## 1. 包定位

```text
成熟 BattlePrepare 组件的最小扩展接缝包
```

本包不是全面解锁 V0.2 / V0.3，不是重写 BattlePrepare，不是重画 UI。

本包目标：

```text
给成熟 BattlePrepare 棋盘 / 道具栏 / 拖拽 / Ghost / Commit / Cancel 增加最小 Extension Seam。
让 V0.4 ShapePlacementSession 可以作为 devOnly 扩展接入。
默认正式行为不变。
```

## 2. 为什么需要本包

当前外部 Adapter / Extension 路线仍不稳定。

根因不是 V0.2 / V0.3 不能碰，而是：

```text
V0.2 / V0.3 成熟组件原本不是为多格道具设计。
如果完全不改成熟组件内部输入 / 布局 / drop 接缝，V0.4 只能在外层打补丁。
外层补丁无法稳定统一道具栏、拖拽、棋盘、ghost、commit 状态。
```

因此允许有限解锁：

```text
只新增扩展接缝。
不改变默认正式行为。
不改变正式布局。
不改变主流程 / 存档 / 数值。
```

## 3. 上游前置

必须读取：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md
Docs/V0.4/REUSABLE_COMPONENT_EXTENSION_RULE.md
Docs/V0.4/ShapePlacementSession01_TechSurvey_Assignment.md
Docs/V0.4/MobileShapePlacementRuntimeIntegrationFix01_Assignment.md
```

并理解：

```text
Fix03 overlay + ignoreLayout + 延后一帧读 drop 路线已停止。
MobileShapePlacementRuntimeIntegrationFix01 外贴接入仍不稳定。
```

## 4. 允许范围

允许只读 Survey：

```text
V03BattlePrepareInteractionController
成熟 BattlePrepare 道具栏相关脚本
成熟 BattlePrepare 棋盘相关脚本
成熟 BattlePrepare 拖拽入口
成熟 BattlePrepare Ghost / Preview / Commit / Cancel 相关逻辑
```

允许最小修改：

```text
V03BattlePrepareInteractionController
与 BattlePrepare 道具栏 / 棋盘 / 拖拽直接相关的 V03 脚本
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
```

如果发现需要修改 V02 核心脚本，必须先停并返回：

```text
BATTLEPREPARE_SEAM_REQUIRES_V02_CORE_TOUCH
```

不得直接改。

## 5. 允许新增的接缝

允许新增 devOnly / optional seam：

```text
IShapePlacementSessionProvider
IShapeItemPayloadProvider
IShapeGridReceiverProvider
IBattlePrepareGhostPreviewAdapter
IBattlePreparePlacementCommitAdapter
IBattlePreparePlacementCancelAdapter
```

或等价命名。

接缝必须满足：

```text
默认 null / disabled。
没有 V0.4 devOnly adapter 注入时，正式 BattlePrepare 行为完全不变。
```

## 6. 必须保持的正式行为

没有启用 V0.4 devOnly adapter 时：

```text
原整备入口不变。
原道具栏显示不变。
原拖拽手感不变。
原继续战斗不变。
原棋盘显示不变。
原道具放置 / 返回 / 取消不变。
```

## 7. 禁止范围

禁止：

```text
重写 V03BattlePrepareInteractionController
重画道具栏
重画棋盘
替换成熟 BattlePrepare UI
修改正式 RunFlow / PageState / FormationState
修改 SaveData / PlayerPrefs / MainTrialProgressData
修改 Boss / 奖励 / 掉落 / 数值
修改正式 V02 / V03 场景布局
覆盖用户手调 RectTransform
恢复 overlay + ignoreLayout + 延后一帧读 drop
FeatureFlag 默认 true
devOnly = false
isEnabled = true
commit / tag / push
```

## 8. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BattlePrepareExtensionSeamReport.md
Docs/V0.4/Reports/BattlePrepareExtensionSeamMap.csv
Docs/V0.4/Reports/BattlePrepareExtensionSeamLeakCheckReport.md
```

报告必须说明：

```text
新增了哪些 seam
修改了哪些成熟 BattlePrepare 文件
默认正式行为是否保持
V0.4 devOnly adapter 如何注入
是否触碰 V02 核心
是否改场景布局 / RectTransform
是否改 RunFlow / SaveData / Boss / 奖励 / 数值
是否恢复旧 overlay 补丁路线
```

## 9. 验收标准

必须满足：

```text
C# 编译通过
无本包 Error / Warning
报告 PASS
默认正式 BattlePrepare 路径不变
V0.4 devOnly adapter 可接入 seam
未触碰禁止范围
```

如果无法在不触碰 V02 核心 / 正式场景布局的情况下建立 seam，必须返回：

```text
BATTLEPREPARE_SEAM_BLOCKED
```

并说明阻塞点。

## 10. 下一步

本包通过后，回到：

```text
V0.4-MobileShapePlacementRuntimeIntegrationFix01
```

重新把 ShapePlacementSession / ShapeAwareItemTrayGrid / MobileShapePlacementInputExtension 接入新 seam。
