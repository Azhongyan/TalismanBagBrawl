# V0.3/V0.4-BuildSandbox-GuardBaseline01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_GUARDBASELINE01
```

## 1. 包定位

```text
BuildSandbox 稳定版本保护与沙盒开发护栏包
```

本包是 Build / 词条 / 羁绊 / 多格占位 / 验证池后台沙盒线的第一个包。

目标不是开发正式 Build 玩法，而是先建立护栏，保证后续包不会误伤当前 V0.3 UI 与 V0.2 稳定基线。

## 2. 必须做

```text
1. 建立 BuildSandbox FeatureFlag 默认关闭配置。
2. 建立 devOnly / isEnabled 隔离规则。
3. 建立 Config Validation 入口。
4. 建立 UI Layout Guard 入口。
5. 建立 CoreFlow Smoke Test 入口或占位运行器。
6. 建立 BuildSandbox 报告输出目录与基础报告格式。
7. 建立 Guard 自动检查菜单或 Editor 执行入口。
```

## 3. FeatureFlag

至少包含：

```text
EnableSynergyBuild = false
EnableAffixSystem = false
EnableDevBuildContent = false
EnableBuildModifierInCombat = false
EnableBuildDebugReport = false
EnableItemShapeOccupancy = false
EnableShapePlacementSandbox = false
EnableShapeRotation = false
```

任何默认值为 `true` 均为失败。

## 4. 允许新增 / 修改范围

优先新增独立沙盒目录，例如：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许新增：

```text
BuildSandboxFeatureFlags
BuildSandboxGuardRunner
BuildSandboxConfigValidator
BuildSandboxDevOnlyValidator
BuildSandboxUiLayoutGuard
BuildSandboxCoreFlowSmokeEntry
BuildSandboxReportWriter
```

命名可由开发窗口根据工程风格微调，但必须保持“独立沙盒 / 不接正式流程”。

## 5. 禁止范围

本包禁止：

```text
开发 SynergyEvaluator
开发词条系统
开发多格正式背包
开发正式 Build 玩法
接入正式战斗
修改当前 UI 场景或 RectTransform
修改首页 / BottomBar / BattlePrepare / Upgrade 页布局
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改 Boss / 奖励 / 掉落 / 数值
修改正式 RewardConfig / UpgradeConfig
修改 AGENTS.md / Docs/LOCKED/*
commit / tag / push
```

## 6. UI Layout Guard 第一版检查目标

本包只做检查入口，不要求一次性覆盖所有场景。

第一版至少能检查或报告：

```text
是否存在明显写 RectTransform 的 BuildSandbox 代码
BuildSandbox 是否尝试访问当前 UI Scene
BuildSandbox 是否包含 Runtime 创建正式 UI 的代码
是否有 FeatureFlag 默认 true
是否有 devOnly=false 的沙盒配置
```

如果开发窗口选择做更细检查，也只能是只读报告，不得自动修 UI。

## 7. Config Validation 第一版检查目标

至少检查：

```text
所有 BuildSandbox FeatureFlag 默认 false
所有 BuildSandbox 测试配置 devOnly=true
所有 BuildSandbox 测试配置 isEnabled=false
不存在接入正式掉落 / 正式锻造 / 正式主线的引用
```

## 8. 交付报告

本包应输出至少一个报告：

```text
Docs/V0.4/Reports/BuildSandboxGuardBaselineReport.md
```

报告包含：

```text
FeatureFlag 默认值
devOnly 检查结果
Config Validation 结果
UI Layout Guard 结果
是否触碰禁止范围
下一包是否可进入
```

## 9. 验收标准

必须满足：

```text
1. C# 编译通过。
2. FeatureFlag 全部默认 false。
3. devOnly 隔离检查可运行。
4. Config Validation 入口可运行。
5. UI Layout Guard 入口可运行，且不写场景。
6. 本包未修改当前 UI 场景 / 主流程 / 存档 / 奖励 / 数值。
7. 输出 BuildSandboxGuardBaselineReport.md。
```

## 10. 用户手测建议

本包主要是后台护栏包，用户不需要在 Unity 中验证新玩法。

用户只需确认：

```text
当前 UI 手调工作没有被打断。
当前首页 / 战斗 / 升级页视觉没有被本包改动。
开发窗口提供的报告能说明 FeatureFlag 和 devOnly 均关闭。
```

## 11. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-SynergyDataFoundation01
```

