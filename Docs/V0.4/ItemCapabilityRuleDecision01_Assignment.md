# V0.4-ItemCapabilityRuleDecision01 Guard Assignment

状态：`GUARD_PASS_ITEMCAPABILITYRULEDECISION01 / READY_FOR_ANALYSIS`  
风险：`YELLOW / REPORT_ONLY_RULE_DECISION_PREPARATION`  
维护方：总体 / 跨系统 Guard  
日期：2026-07-18

## 0. 包定位

本包是 C02A 与 C02B 之间的规则决策准备包，只为以下 6 个能力制作可审计、可比较、可由用户选择的规则候选：

```text
capability.energy_stability
capability.control_power
capability.cleanse_power
capability.placement_shape
capability.burst_window
capability.clear_power
```

本包不修改 Item→Capability 映射，不修改 Item 或 Enemy 事实源，不把任何候选规则视为正式规则。用户明确选择且 Guard 再次放行前，6 项能力必须继续保持 `Unknown`。

## 1. 必读

开发窗口必须先按真实工程启动规则完整读取 AGENTS / LOCKED / ROADMAP / CURRENT / Package Queue，并读取：

```text
Docs/V0.4/CROSS_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/ItemCapabilityMappingGapSurvey01_Assignment.md
Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyReport.md
Docs/V0.4/Reports/ItemCapabilityMappingGapMatrix.csv
Docs/V0.4/Reports/ItemCapabilityMappingSourceEvidence.csv
Docs/V0.4/Reports/ItemCapabilityMappingDecisionList.csv
Docs/V0.4/Reports/ItemEnemyMatchupUnknownBlockers.csv
Docs/V0.4/Reports/ItemEnemyMatchupCapabilityCoverage.csv
Docs/V0.4/ItemSystemGuard_CurrentRules.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ITEM_ALGORITHM_PACKAGE_QUEUE.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
```

还必须只读核对真实 `BuildCapabilityReadContract`、C01 projection、Item Snapshot / Affix / Shape / Build 事实字段。不得只根据 C02A CSV 推导。

## 2. 本包必须回答的问题

每个能力必须分别回答：

```text
1. 哪些 stable key / contract member 可以作为候选输入？
2. 输入单位是什么？是否能与 BuildCapability 输出单位安全换算？
3. 哪些 Item 才有资格参与：已放置、已点亮、已供能、主 Build、触发成功或其他稳定条件？
4. 多个来源如何聚合：sum / max / count / weighted sum / 不可安全聚合？
5. 是否需要 clamp、阈值或条件门槛？依据来自哪里？
6. 空来源应为 Known Zero 还是 Unknown？
7. 候选规则会减少多少 E10 requirement blocker 和 C02 blocked rows？
8. 规则是否可由静态 Item Snapshot 完成，还是仍依赖 Runtime 信号？
```

## 3. 候选规则口径

每项最多提供两套候选，不得为了凑数伪造第二套。允许结论为：

```text
READY_FOR_USER_DECISION
NEEDS_ITEM_GUARD_FACT
NO_UNIT_SAFE_RULE
KEEP_UNKNOWN
```

候选规则必须完整记录：

```text
decisionId
capabilityKey
candidateId
candidateSourceContract
candidateStableKeys
sourceUnit
eligibilityRule
aggregationRule
conversionRule
clampOrThresholdRule
missingSourceSemantics
knownZeroSemantics
runtimeDependency
affectedEncounterCount
projectedBlockedRowReduction
advantages
risks
recommendedDisposition
evidenceFile
evidenceLineOrSymbol
```

硬要求：

```text
不得通过中文名称、UI 文案、图标或章节编号建立规则。
不得把缺失来源解释为 0。
不得把条件触发词条无条件计入能力。
不得在单位不一致时自行选择换算比例。
不得把 Runtime-only 信号塞进静态 Item Projection。
不得输出正式 Readiness、难度、胜负或掉落结论。
```

`energy_stability` 当前已发现单位冲突；若真实合同不能消除冲突，必须输出 `NO_UNIT_SAFE_RULE / KEEP_UNKNOWN`，不得强行给数值。

## 4. 影响分析

必须基于 C02 的 8×4 矩阵做只读投影，输出：

```text
每个候选规则覆盖哪些 capability requirement
影响哪些 Encounter
理论上减少多少 Unknown blocker 引用
理论上减少多少 blocked rows
仍然剩余哪些 Unknown
```

这里只允许输出“解阻覆盖估计”，不得重新运行或改写 Enemy Readiness，不得把候选规则当成已批准规则。

## 5. 允许新增范围

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs
对应 .meta
Docs/V0.4/Reports/ItemCapabilityRuleDecisionReport.md
Docs/V0.4/Reports/ItemCapabilityRuleCandidates.csv
Docs/V0.4/Reports/ItemCapabilityRuleImpactMatrix.csv
Docs/V0.4/Reports/ItemCapabilityRuleDecisionSheet.csv
Docs/V0.4/Reports/ItemCapabilityRuleDecisionLeakCheckReport.md
```

如确有必要，只可在同一 Editor-only 目录新增一个同前缀纯报告辅助文件。修改已有文件数量必须为 0。

## 6. 禁止范围

```text
修改 C01 / C02 / C02A 文件 = 0
修改 Item / Enemy 既有文件 = 0
修改 Runtime / Battle / Board = 0
修改 Scene / Prefab / Config = 0
修改任何 Canonical Signature 或保护基线 = 0
```

禁止：

```text
实现 C02B 映射。
把 6 项 Unknown 改为 Supported 或 Known Zero。
新增或修改 Item 数值、词条、品阶、形状、Build、供能规则。
新增或修改 Enemy requirement、权重、阈值、技能、Boss phase。
调用 EnemyOfflineReadinessEvaluator 输出正式结论。
运行 Scene/Prefab Builder 或会写资源的历史回归入口。
修改 AGENTS.md / Docs/LOCKED/* / Guard owner docs。
commit / tag / push / reset / rollback。
自动启动 C02B、C02R1 或 C03。
```

## 7. 验收

```text
6/6 能力都有证据链和明确 disposition。
每项候选不超过 2 套。
所有候选都明确单位、资格条件、聚合、换算、缺失语义与风险。
无法安全换算的候选保持 Unknown。
影响矩阵覆盖 C02 的 8×4 样本，但不输出 Readiness/难度结论。
Item / Enemy / C01 / C02 / C02A protected hash 前后一致。
现有文件修改数 = 0。
Runtime / Battle / Board / Scene / Prefab / Config diff = 0。
Leak Count = 0。
尾随空白 = 0。
git diff --check PASS。
Offline verifier PASS。
Unity batch verifier PASS；若被用户 Editor 占用，必须如实记录 BLOCKED。
```

## 8. 回传

完成后只回传：

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-ItemCapabilityRuleDecision01
Guard receipt: GUARD_PASS_ITEMCAPABILITYRULEDECISION01
新增文件 / 修改已有文件
6 capability dispositions
Candidate count per capability
Unit-safe / no-unit-safe counts
Projected blocker reduction by candidate
Remaining Unknown list
Protected hashes
Offline verifier
Unity verifier
Leak count
git diff --check
Forbidden scope touched
HEAD status
```

不得替用户批准任何候选，不得自动启动 C02B，不得 commit / tag / push。
