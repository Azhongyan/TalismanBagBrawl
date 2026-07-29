# V0.4-BuildCapabilityNormalizationRuleSurvey01 Guard Assignment

状态：`GUARD_PASS_BUILDCAPABILITYNORMALIZATIONRULESURVEY01 / READY_FOR_REPORT_ONLY_SURVEY`  
风险：`YELLOW / CROSS_SYSTEM_RULE_ANALYSIS_ONLY`  
维护方：Capability Algorithm Guard  
日期：2026-07-19

## 0. 包定位

本包只分析六项能力的归一化候选、资格、聚合、状态语义与 C02 条件性解阻影响。唯一允许的代码是 Editor-only 只读报告 Verifier。不得实现 C02B、正式映射、Runtime Producer 或任何战斗逻辑。Survey 完成后六项能力仍全部保持 `Unknown`。

覆盖：

```text
capability.energy_stability
capability.control_power
capability.cleanse_power
capability.placement_shape
capability.burst_window
capability.clear_power
```

## 1. 允许文件

只允许新增：

```text
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs
Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs.meta
Docs/V0.4/Reports/BuildCapabilityNormalizationRuleSurveyReport.md
Docs/V0.4/Reports/BuildCapabilityNormalizationSourceEligibilityMatrix.csv
Docs/V0.4/Reports/BuildCapabilityNormalizationRuleCandidates.csv
Docs/V0.4/Reports/BuildCapabilityNormalizationC02ImpactMatrix.csv
Docs/V0.4/Reports/BuildCapabilityNormalizationUserDecisionSheet.csv
Docs/V0.4/Reports/BuildCapabilityNormalizationRuleSurveyLeakCheckReport.md
```

修改已有文件数必须为 `0`。

## 2. 禁止文件与行为

```text
Item / Enemy 事实源
C01 / C02 / C02A / C02A-D1
IF01 / IF02 / IF03
BuildCapabilityReadContract
ItemBuildCapabilityProjectionAdapter
ItemEnemyMatchupSimulation
AGENTS.md / Docs/LOCKED/*
所有 Package Queue / Guard CurrentRules
Scene / Prefab / Config / RectTransform / BuildSettings
Battle / Board / RunFlow / SaveData / Reward / Chapter / Boss
```

同时禁止：

```text
实现 C02B 或正式映射
新建 Runtime Fact Producer
把 point、stack、count、nian point、basisPoint 直接互换
猜测比例、权重、阈值、分母或上限
把 Unknown、缺事实或缺绑定当作 0
输出正式 Readiness、难度、胜负、奖励、掉落结论
commit / tag / push
自动启动下一包
```

## 3. 报告要求

主报告必须逐项回答：

```text
1. 每种 Item 原生事实如何或是否能够归一化到 0–10000 BP。
2. 哪些来源有资格参与计算。
3. 多件道具采用 sum / max / count / weighted sum 或不可安全聚合。
4. 是否需要 clamp、阈值与上限；依据是什么。
5. 未触发、未放置、未点亮、缺事实分别为 Zero、Unknown 或 Invalid。
6. placement_shape 的策划含义是否有足够证据。
7. Runtime Fact 如何参与 burst、cleanse、clear、energy。
8. 每套候选规则对 C02 32 行的预计解阻数量。
9. 哪些规则必须由用户确认后才能实施。
10. 哪些能力必须继续保持 Unknown。
```

每项允许 `0–2` 套候选，不得为凑数制造第二套。

每条候选至少包含：

```text
candidateId
capabilityKey
planningMeaning
sourceContracts / sourceFields
nativeValueDomain / nativeUnit
eligibilityRule
runtimeFactGate
aggregationRule
normalizationFormula
unresolvedParameters
parameterAuthority
clamp / threshold / cap
untriggeredSemantics
unplacedSemantics
unlitSemantics
missingFactSemantics
invalidSemantics
affectedEncounters
affectedC02Rows
projectedUnknownReferenceReduction
projectedBlockedRowReduction
remainingUnknowns
implementationReadiness
userDecisionRequired
evidencePath / evidenceSymbol
advantages / risks
```

## 4. C02 影响分析

以现有 `8×4 = 32` 行为固定基线，只允许计算：

```text
候选覆盖的 requirement
减少的 Unknown 引用
仍剩余的决定性 Unknown
单候选预计解阻行数
批准候选组合后的条件性解阻行数
```

不得生成 ReadinessBand 或难度结论；预计解阻为 `0` 是合法结果。

## 5. 状态语义硬门

```text
缺事实、事实不完整、缺身份绑定、缺资格证明 = Unknown
单位冲突、身份错配、重复事件、非法值、越界 = Invalid
KnownFalse 不等于缺事实
已确认未放置或未点亮 = 不具备资格
仅在完整输入域、完整资格集合与已批准规则同时成立时，总能力才可能 Known Zero
未触发但没有完整 Runtime Fact = Unknown
```

Survey 完成后六项能力仍全部保持 `Unknown`。

## 6. 候选验收条件

```text
六项能力全部有证据链、语义结论与 disposition
每项候选不超过两套
无批准来源的常量只能是符号参数并标记 USER_DECISION_REQUIRED
control point / cleanse stack / chain count / nian point / basisPoint 分域处理
聚合明确为 sum / max / count / weighted sum 或不可安全聚合
clamp、阈值、上限必须有来源
placement_shape 证据不足时必须 SEMANTIC_UNRESOLVED / KEEP_UNKNOWN
Runtime 候选只能引用 IF03 已发布字段
依赖 Producer 的候选必须 NEEDS_RUNTIME_PRODUCER
32 行影响不得双计数，不得假设其他 Unknown 已解决
Protected hashes 前后一致
Leak Count = 0
现有文件修改数 = 0
```

## 7. 用户决策点

报告必须整理供用户逐项决定：

```text
point / stack / count / nian point 到 BP 的归一化曲线与尺度锚点
basisPoint 是否允许同域 identity conversion，多实例如何聚合
道具资格是否要求已放置、已点亮、计入 Build 或完成 Runtime 触发
聚合采用 sum / max / count / weighted sum
clamp、阈值、单件上限与总上限
KnownFalse、未放置、未点亮的零值边界
placement_shape 表示占格效率、布局稳定性、形状适配或其他目标
Runtime Fact 的时间域：单事件、单道具、单场战斗或其他
批准哪套候选进入实现；未批准项继续 Unknown
```

## 8. Protected hashes

以下基线必须前后完全一致，不得刷新或接受新 baseline：

```text
Item existing scope (105): b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
Enemy (89): 7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae
C01 (9): fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a
C02 (9): 08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4
C02A (7): a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb
IF01 Canonical: sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428
IF02 Canonical: sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673
IF03 Canonical: sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a
Affix Schema: sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f
150 Roll: sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8
150 Projection: sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2
Scenes (14): da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b
Prefabs (16): 7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087
BuildSettings: 08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59
```

基线更正记录：

```text
GUARD_CORRECT_BUILDCAPABILITYNORMALIZATIONRULESURVEY01_ITEM_BASELINE01
Old stale baseline: 7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663
Correct post-IF02 accepted baseline: b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
```

原因：旧值来自 IF02 修正 `affix_nian_efficiency` 之前；正确值对应 IF02/IF03 已验收后的同一 105-file pre-`Items/Capability` scope。该更正只修复 Assignment 的历史基线错误，不允许刷新其他保护值，不允许回退 `ItemCompleteCandidateContent.cs` 或撤销省念单位修正。

SHA-256 长度更正：

```text
GUARD_CORRECT_BUILDCAPABILITYNORMALIZATIONRULESURVEY01_SHA256_LENGTH01
Rejected 65-char transcription: b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4f
Accepted 64-char computed hash: b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4
```

末尾多出的 `f` 是 Guard 抄录错误，不属于工程状态变化。Verifier 的 expected baseline 必须使用上述 64 位实算值。

## 9. 后续顺序

```text
1. V0.4-BuildCapabilityNormalizationRuleSurvey01
2. 用户逐能力选择 / 拒绝候选；Guard 二次收口
3. V0.4-BuildCapabilityNormalizationRuleContract01
4. V0.4-ItemCapabilityRuntimeFactProducerAdapter01（仅对确需 Runtime 的批准候选）
5. C02B V0.4-ItemCapabilityMappingExpansion01
6. C02R1 V0.4-ItemEnemyMatchupSimulation01-Rerun01
7. C03，仅在 C02R1 出现可信可评估行后解冻
```

任何一步均不得自动启动。

## 10. 回传

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package: V0.4-BuildCapabilityNormalizationRuleSurvey01
Guard receipt: GUARD_PASS_BUILDCAPABILITYNORMALIZATIONRULESURVEY01
新增文件 / 修改已有文件
Six capability dispositions
Candidate counts
Source eligibility summary
Symbolic vs approved constants
C02 single-candidate and combination impact
Projected blocked-row reduction
Remaining Unknowns
User decision list
Protected hashes
Offline verifier
Unity verifier
Leak Count
git diff --check
Forbidden scope touched
HEAD status
```
