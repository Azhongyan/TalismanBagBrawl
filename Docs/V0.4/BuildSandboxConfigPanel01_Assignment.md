# V0.4-BuildSandboxConfigPanel01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOXCONFIGPANEL01
```

## 1. 包定位

```text
V0.4 BuildSandbox devOnly 配置面板包
```

本包目标是建立 V0.4 自己的 BuildSandbox 配置面板，形态参考现有：

```text
Tools / Talisman Bag / V0.2 / Data / [Manual Only] Stage Config Panel 01
```

但本包不得直接改造或扩展 V0.2 的正式配置面板数据源。

新增面板建议路径：

```text
Tools / Talisman Bag / V0.4 / BuildSandbox / Data / [Manual Only] BuildSandbox Config Panel 01
```

窗口标题建议：

```text
BuildSandbox Config Panel 01
```

## 2. 上游前置

必须读取并遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/BuildProblemRulePool01_Assignment.md
Docs/V0.4/BuildProblemSeedData01_Assignment.md
```

已完成数据：

```text
MapRuleConfig
EnemyProblemConfig
BossProblemConfig
BuildReadinessCheckConfig
WeaknessWindowConfig
DropBiasConfig
FailureHintConfig
10 地图规则
10 敌人题目
6 Boss 题目
6 弱点窗口
18 DropBias
```

## 3. 可参考的旧面板形态

可参考：

```text
Assets/_Game/Scripts/TalismanBag/V02/Config/Editor/DataCatalogEditorWindow.cs
Assets/_Game/Scripts/TalismanBag/V02/Config/Editor/StageConfigPanelEditorUi.cs
```

可复用的设计方式：

```text
EditorWindow
Tab 页
Foldout 展开
Reload / Validate / Save
Validation 页
HelpBox 风险提示
SerializedObject / SerializedProperty 编辑方式
```

但不得直接让 V0.4 面板编辑 V0.2 / V0.3 正式数据源。

## 4. 建议页签

```text
Overview / 总览
MapRule / 地图规则
EnemyProblem / 敌人题目
BossProblem / Boss 多钥匙锁
WeaknessWindow / 弱点窗口
DropBias / 定向倾向
BuildReadiness / 破题准备度
DevChapter / devOnly 章节预留
Simulation / 模拟结果预留
Validation / 校验
```

本包至少应实现前 7 个页签和 Validation。

`DevChapter` / `Simulation` 可以先做只读占位，后续包再填。

## 5. 允许新增 / 修改范围

允许新增或修改：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增：

```text
BuildSandboxConfigPanelWindow
BuildSandboxConfigPanelTab enum
BuildSandboxConfigPanelEditorUi
BuildSandboxConfigPanelValidator / panel validation adapter
BuildSandboxConfigPanelReportWriter
```

允许读取并展示：

```text
BuildProblemSeedData
BuildProblemRulePoolValidator
BuildProblemSeedData reports
Phase 1 BuildSandbox config assets
```

## 6. 禁止范围

禁止修改：

```text
Assets/_Game/Scripts/TalismanBag/V02/Config/Editor/DataCatalogEditorWindow.cs
Assets/_Game/Scripts/TalismanBag/V02/Config/Editor/StageConfigPanelEditorUi.cs
V02RunConfig
EnemyDefinition 正式资产
EnemyGroupConfig 正式资产
Reward / DropTable 正式资产
BossConfig 正式资产
Tutorial 正式资产
Item 正式资产
V02RunFlowController
V02FormationGridFrame
DamageText
RunFlow / PageState / FormationState
SaveData / PlayerPrefs / MainTrialProgressData
V03 MainHome / BattlePrepare / TalismanUpgrade 场景
用户手调 RectTransform
```

禁止行为：

```text
通过 V0.4 面板写正式 1-10 / 2-10
通过 V0.4 面板写正式奖励 / 掉落 / Boss / 存档
把 devOnly seed 变成正式配置
MapRule 默认启用
FeatureFlag 默认 true
devOnly = false
isEnabled = true
自动修配置
删除资产
commit / tag / push
```

## 7. 数据权威

本包允许两种实现路径，由开发窗口 Code Survey 后择一，但不得混入正式数据：

### 路径 A：只读静态 seed 面板

```text
读取 BuildProblemSeedData 静态数据并展示。
面板主要用于查看、搜索、校验、导出报告。
```

优点：安全，适合首版。

### 路径 B：devOnly ScriptableObject 面板

```text
为 V0.4 BuildSandbox 建立独立 devOnly ScriptableObject assets。
面板只编辑 Assets/_Game/Configs/BuildSandbox/** 下的资产。
```

优点：更接近 Stage Config Panel 01。

限制：必须确保所有资产默认 `devOnly=true / isEnabled=false`，不得引用正式配置。

Guard 建议首版优先路径 A 或 A+B 的最小安全混合：

```text
先只读展示 seed；
若已有 BuildSandbox config assets，可编辑其 devOnly/isEnabled 之外的安全说明字段；
不得在本包做正式数据 migration。
```

## 8. 报告要求

至少输出：

```text
Docs/V0.4/Reports/BuildSandboxConfigPanelReport.md
Docs/V0.4/Reports/BuildSandboxConfigPanelTabReport.csv
Docs/V0.4/Reports/BuildSandboxConfigPanelLeakCheckReport.md
```

报告必须说明：

```text
菜单路径是否存在
窗口是否可打开
页签数量
各页签数据数量
Validation 是否可运行
是否编辑正式数据
是否触碰 V02 DataCatalogEditorWindow
FeatureFlag / devOnly / isEnabled 状态
```

## 9. 验收标准

必须满足：

```text
C# 编译通过
Unity 菜单可打开 BuildSandbox Config Panel 01
面板至少展示 MapRule / EnemyProblem / BossProblem / WeaknessWindow / DropBias / BuildReadiness / Validation
Validate 可运行并输出报告
不修改 V02 Stage Config Panel 01
不写正式 V02 / V03 数据
不打开 FeatureFlag
不让 devOnly 内容进入正式流程
```

## 10. 用户手测建议

用户在 Unity 非 Play 状态下打开：

```text
Tools / Talisman Bag / V0.4 / BuildSandbox / Data / [Manual Only] BuildSandbox Config Panel 01
```

确认：

```text
窗口能打开
页签能切换
MapRule 显示 10 条
EnemyProblem 显示 10 条
BossProblem 显示 6 条
DropBias 显示 18 条
Validation 无红色 Error / 黄色 Warning
没有修改 V0.2 Stage Config Panel 01
```
