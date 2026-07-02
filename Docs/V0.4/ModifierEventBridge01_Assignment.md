# V0.3/V0.4-BuildSandbox-ModifierEventBridge01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_MODIFIEREVENTBRIDGE01
```

## 1. 包定位

```text
Build 结果到战斗效果的桥接结构包
```

本包是 BuildSandbox 队列的 Package 5。目标是把 `BuildEvaluationResult` 转换为沙盒态的 `CombatModifierBundle` / `EffectEventBundle` 预览输出。

本包不是正式战斗接入包。它只能证明“如果未来开启 Build 系统，会产生哪些 Modifier / Event”，不得让这些结果影响当前正式战斗。

## 2. 上游前置

前置包已通过：

```text
V0.3/V0.4-BuildSandbox-GuardBaseline01
USER_ACCEPTED / QA_PASSED_BY_USER

V0.3/V0.4-BuildSandbox-SynergyDataFoundation01
USER_ACCEPTED / QA_PASSED_BY_USER

V0.3/V0.4-BuildSandbox-ItemShapeOccupancy01
USER_ACCEPTED / QA_PASSED_BY_USER

V0.3/V0.4-BuildSandbox-SynergyEvaluatorCore01
USER_ACCEPTED / QA_PASSED_BY_USER

V0.3/V0.4-BuildSandbox-AffixRaritySandbox01
USER_ACCEPTED / QA_PASSED_BY_USER
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/SynergyEvaluatorCore01_Assignment.md
```

## 3. 必须做

新增 BuildSandbox 中间桥结构，至少包含：

```text
CombatModifierBundle
EffectEventBundle
BuildModifierPreview
BuildEventPreview
ModifierEventBridge
```

允许从以下沙盒输入生成预览：

```text
BuildEvaluationResult
activeSynergies
activeThresholds
Affix preview result
```

## 4. Modifier 类型

至少预留：

```text
damageBonus
cooldownBonus
shieldBreakBonus
shieldBonus
cleanseBonus
controlDurationBonus
energyReturnBonus
```

## 5. Event 类型

至少预留：

```text
onBattleStart
onShieldBreak
onCleanse
onShieldBroken
onEnemyKill
onBossSkillCast
onLowHp
onEnergyConnected
```

## 6. FeatureFlag 硬规则

必须保持：

```text
EnableBuildModifierInCombat = false
EnableSynergyBuild = false
EnableAffixSystem = false
```

本包只能：

```text
能计算
能输出
能写报告
```

不得：

```text
影响正式战斗
修改正式伤害
修改正式冷却
修改正式护盾
修改正式道具触发
```

## 7. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Assets/_Game/Configs/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展前置 BuildSandbox 校验器、GuardRunner、ReportWriter，使其能输出 Modifier / Event 预览报告。

允许输出：

```text
Docs/V0.4/Reports/ModifierEventBridgeReport.md
Docs/V0.4/Reports/CombatModifierBundlePreview.csv
Docs/V0.4/Reports/EffectEventBundlePreview.csv
```

## 8. 禁止范围

本包禁止：

```text
接入正式战斗
修改正式伤害结算
修改正式道具触发
修改敌人技能
修改 Boss 技能或触发
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改奖励 / 掉落 / 数值
修改正式 RewardConfig / UpgradeConfig
修改当前正式战斗 UI
修改当前正式整备交互
默认启用任何 BuildSandbox FeatureFlag
让 devOnly 内容进入正式流程
开发 BuildSimulationBenchmark 批量模拟器
commit / tag / push
```

## 9. Config Validation 要求

本包必须让 Config Validator 至少能检查：

```text
Modifier 类型是否有效
Event 类型是否有效
Modifier 是否缺 sourceSynergy / sourceAffix / sourceThreshold
Event 是否缺 trigger
Bridge 输出是否只引用 BuildSandbox 数据
是否存在正式战斗类引用
所有新增配置是否 devOnly=true
所有新增配置是否 isEnabled=false
所有 FeatureFlag 是否默认 false
```

## 10. 报告要求

必须输出：

```text
Docs/V0.4/Reports/ModifierEventBridgeReport.md
Docs/V0.4/Reports/CombatModifierBundlePreview.csv
Docs/V0.4/Reports/EffectEventBundlePreview.csv
```

报告至少包含：

```text
输入 BuildEvaluationResult 摘要
输入 Affix preview 摘要
生成的 Modifier 类型
生成的 Event 类型
sourceSynergy / sourceAffix / sourceThreshold
是否影响正式战斗：必须为 false
FeatureFlag 状态
devOnly / isEnabled 检查
是否触碰禁止范围
```

## 11. 验收标准

必须满足：

```text
1. C# 编译通过。
2. BuildEvaluationResult 能生成 CombatModifierBundle 预览。
3. BuildEvaluationResult 能生成 EffectEventBundle 预览。
4. Affix preview 可参与 Modifier / Event 预览输出。
5. FeatureFlag 关闭时不影响正式战斗。
6. 报告能看到将会产生哪些 Modifier / Event。
7. 没有任何正式战斗数值变化。
8. 所有新增配置 devOnly=true / isEnabled=false。
9. 未修改正式 UI、主流程、存档、奖励、数值、Boss。
```

## 12. 用户手测建议

本包仍是后台沙盒包，用户不需要验证正式战斗。

用户只需确认：

```text
当前 UI 手调没有被影响。
报告显示 Modifier / Event 只是 preview。
FeatureFlag 仍为 false。
devOnly / isEnabled 隔离成立。
正式战斗没有被接入。
```

## 13. 通过后下一包

通过后进入：

```text
V0.3/V0.4-BuildSandbox-BuildSimulationBenchmark01
```

