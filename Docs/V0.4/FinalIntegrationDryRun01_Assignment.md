# V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01 Assignment

Guard 回执：

```text
GUARD_PASS_BUILDSANDBOX_FINALINTEGRATIONDRYRUN01
```

## 1. 包定位

```text
BuildSandbox 全链路干跑包
```

本包是 BuildSandbox 队列的 Package 11，也是本轮 BuildSandbox 并行沙盒队列的收口包。

目标是不正式打开任何 FeatureFlag，但证明 Build / 标签 / 多格占位 / 羁绊 / 词条 / Modifier / Event / 模拟器 / 验证池 / DevChapter / LedgerHook / ConfigValidator 全链路可用。

本包不做新正式功能，不接正式玩法，不影响当前 V0.3 UI / V0.2 稳定基线。

## 2. 上游前置

前置包已通过：

```text
GuardBaseline01
SynergyDataFoundation01
ItemShapeOccupancy01
SynergyEvaluatorCore01
AffixRaritySandbox01
ModifierEventBridge01
BuildSimulationBenchmark01
EnemyBossValidationPool01
DevChapterContentPool01
LedgerTaskBuildHooks01
ConfigValidatorReport01
```

必须继续遵守：

```text
Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
```

## 3. 必须证明

本包必须证明：

```text
1. 当前正式主流程不受影响。
2. Build 系统可以读取 BuildSandboxLayoutSnapshot。
3. ItemShapeOccupancy 可以输出 occupiedCells。
4. SynergyEvaluator 能正确识别羁绊。
5. Affix 系统能生成词条预览。
6. ModifierBundle / EventBundle 能生成但默认不影响战斗。
7. Benchmark 能批量跑报告。
8. Dev 敌人 / Boss 验证池能被模拟调用。
9. Dev 章节池能被模拟调用。
10. 照灯账本 Build 任务钩子可配置。
11. 所有 devOnly 内容不会进入正式流程。
12. 所有 FeatureFlag 默认关闭。
```

## 4. 必须输出

至少输出：

```text
Docs/V0.4/Reports/FinalIntegrationDryRunReport.md
Docs/V0.4/Reports/BuildSandboxFullPipelineReport.md
Docs/V0.4/Reports/BuildSandboxFeatureFlagFinalCheck.md
Docs/V0.4/Reports/BuildSandboxDevOnlyFinalCheck.md
Docs/V0.4/Reports/BuildSandboxFormalFlowLeakFinalCheck.md
```

可复用前置包报告，但必须生成最终总报告。

## 5. 允许新增 / 修改范围

允许在独立沙盒目录新增或扩展：

```text
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/**
Docs/V0.4/Reports/**
Docs/V0.4/BuildSandbox/**
```

允许扩展 BuildSandbox GuardRunner、ConfigValidator、ReportWriter。

允许新增统一菜单入口，例如：

```text
Tools/Talisman Bag/V0.4/BuildSandbox/FinalIntegrationDryRun01/[QA Only] Run Final Integration Dry Run
```

## 6. 禁止范围

本包禁止：

```text
打开正式 Build FeatureFlag
接正式掉落
接正式洗练
影响正式战斗
影响正式 UI
修改用户手调 RectTransform
修改 RunFlow / PageState / FormationState
修改 V02RunFlowController
修改 V02FormationGridFrame
修改 DamageText
修改 SaveData / MainTrialProgressData / PlayerPrefs
修改正式 Boss / 敌人 / 章节 / 奖励 / 数值
修改当前正式战斗 UI
修改当前正式整备交互
自动修配置
删除资产
commit / tag / push
```

## 7. Final Check 要求

必须最终检查：

```text
FeatureFlag 全部 false
所有 BuildSandbox 配置 devOnly=true / isEnabled=false
所有进入正式流程字段为 false
没有正式 StageConfig 引用
没有正式 Reward / Drop / UpgradeConfig 接入
没有正式 SaveData / PlayerPrefs / MainTrialProgressData 写入
没有 Runtime UI / RectTransform 写入
没有 RunFlow / PageState / FormationState 改动
没有 V02FormationGridFrame / DamageText 改动
```

## 8. 报告要求

`FinalIntegrationDryRunReport.md` 至少包含：

```text
总状态：PASS / FAIL
前置 11 包状态
全链路输入
全链路输出
FeatureFlag 最终状态
devOnly 最终状态
正式流程泄漏检查
报告索引
是否允许 BuildSandbox 队列阶段收口
```

## 9. 验收标准

必须满足：

```text
1. C# 编译通过。
2. FinalIntegrationDryRun 菜单 / batch 可运行。
3. 全链路报告完整输出。
4. FeatureFlag 全 false。
5. devOnly / isEnabled 隔离成立。
6. Formal flow leak check 通过。
7. Benchmark 报告可生成。
8. ConfigValidationReport 通过。
9. 正式 1-10 / 2-10 不受影响。
10. 当前 UI 手调不受影响。
```

## 10. 用户手测建议

本包仍是后台沙盒收口包。

用户只需确认：

```text
非 Play 状态菜单可运行。
Console 无本包红色 Error / 黄色 Warning。
FinalIntegrationDryRunReport.md 显示 PASS。
FeatureFlag Final Check 全 false。
Formal Flow Leak Final Check 通过。
当前 UI 手调没有被影响。
```

## 11. 通过后状态

通过后：

```text
V0.3/V0.4-BuildSandbox Technical Queue = PHASE_COMPLETE / WAITING_REPOOPS_CHECKPOINT
```

后续是否进入正式玩法接入，必须重新开新 Roadmap / Package Queue，不得由本队列直接打开 FeatureFlag。

