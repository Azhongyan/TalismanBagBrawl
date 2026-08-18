# V0.4 BoneAspect Mechanic Gap Survey Report

Package: `V0.4-BoneAspectMechanicGapSurvey01-GuardFix01`

Base Assignment Package: `V0.4-BoneAspectMechanicGapSurvey01`

Guard marker: `ENEMY_GUARD_REWORK_BONEASPECTMECHANICGAPSURVEY01_GUARDFIX01`

Assignment SHA-256: `fcc480866dbecfcf367ce5610dd9930950b89aad1fada49e650264e0dd33c21b`

Status: `GUARD_FIX_COMPLETE / STATIC_QA_PASS / GUARD_REVIEW_REQUESTED`

## 1. Survey Position

The accepted Carrier Presentation Catalog freezes `gapSurveyRequired = 16 / 16` and `runtimeImplemented = false`. The legacy Enemy queue does not authorize a vocabulary implementation after E10. This survey therefore precedes any vocabulary extension so that existing E02 reuse, composition, genuinely missing neutral concepts, runtime facts, cross-system contracts, presentation-only statements, and user-decision-blocked behavior are separated before any stable vocabulary key can be proposed.

This package adds evidence reports only. It does not add or modify vocabulary, runtime code, fact sources, bindings, formal-flow content, scenes, prefabs, configs, assets, or user decisions.

Accepted prerequisites:

- `V0.4-BoneAspectContentDesignLock01-GuardFix01 / GUARD_ACCEPTED`
- `V0.4-BoneAspectCarrierPresentationCatalog01 / GUARD_ACCEPTED / QA_PASS`

## 2. Exact Survey Universe

| Check | Result |
| --- | ---: |
| Carrier coverage | 16 / 16 |
| Enemy / Boss coverage | 12 / 4 |
| Accepted P1 E02 candidate references | 34 / 34 |
| P0 atomic gap occurrences | 44 / 44 |
| Evidence rows | 78 / 78 |
| Unique primary classifications | 78 / 78 |
| Unresolved evidence | 0 |

The P1 rows are exact projections from `BoneAspectCarrierPresentationMechanicReferenceRows.csv`, numbered `REUSE-001` through `REUSE-034`. The P0 gaps are exact, unmerged source occurrences from `BoneAspectMechanicCommitmentMatrix.csv.newMechanicGapCandidates`, numbered `GAP-001` through `GAP-044` in accepted carrier and source-field order.

## 3. Primary Classification Result

| Primary classification | Count |
| --- | ---: |
| `EXACT_E02_REUSE` | 43 |
| `COMPOSED_FROM_EXISTING_E02` | 2 |
| `NEW_VOCABULARY_CANDIDATE` | 7 |
| `RUNTIME_ONLY_NOT_VOCABULARY` | 9 |
| `CROSS_SYSTEM_CONTRACT_REQUIRED` | 8 |
| `USER_DECISION_BLOCKED` | 7 |
| `PRESENTATION_ONLY` | 2 |
| `OUT_OF_SCOPE` | 0 |
| Total | 78 |

Every evidence occurrence has exactly one primary classification. Secondary ownership and routing fields do not create additional classifications.

## 4. E02 Reuse Result

All 34 accepted P1 E02 references remain `CandidateReuseOnly`; the survey does not promote them to bindings. All 34 resolve to accepted E02 stable keys.

Among the 44 P0 gap occurrences, 9 are additional exact matches:

- `GAP-004` 破壳后短暂虚弱 → `counter_window.shell_break`
- `GAP-011` 封招 → `mechanic.talisman_seal`
- `GAP-013` 多目标竞争 → `pressure.multi_target`
- `GAP-017` 破盾后核心暴露 → `counter_window.shell_break`
- `GAP-020` 限制技能触发 → `mechanic.talisman_seal`
- `GAP-021` 偷念 → `mechanic.energy_drain`
- `GAP-024` 数量优势 → `pressure.multi_target`
- `GAP-028` 持续读契 → `mechanic.long_cast`
- `GAP-033` “忠”字骨铭核心暴露 → `counter_window.shell_break`

Two additional gaps can be composed without creating a new atomic mechanic:

- `GAP-019` 压制 → `mechanic.talisman_seal;pressure.seal_control`
- `GAP-029` 复合法门攻击 → `mechanic.basic_pressure;mechanic.burst_spike`

`BoneAspectMechanicGapSurveyReuseEvidence.csv` therefore contains 45 rows: 34 accepted P1 references, 9 additional exact matches, and 2 additional composed matches. Across all survey matches, 14 distinct accepted E02 stable keys are referenced. Invalid E02 references: `0`.

For `GAP-021`, `mechanic.energy_drain` expresses only the “偷念” resource-drain/theft mechanic intent. It does not answer BA-D3 Copy scope and does not authorize Enemy to store a second resource truth source or execute resource changes; resource identity and actual mutation remain with the existing Player/Battle Resource Owner through the cross-system effect contract.

## 5. Extension-Eligible Neutral Candidates

Exactly 7 deduplicated neutral candidates are eligible for later Guard review:

- `ba_gap_candidate.charge_attack` — 冲撞攻击 — `GAP-003`
- `ba_gap_candidate.contested_mark` — 争夺标记 — `GAP-015`
- `ba_gap_candidate.damage_reduction` — 减伤 — `GAP-016`
- `ba_gap_candidate.possession_state` — 附身状态 — `GAP-001`
- `ba_gap_candidate.recognition_reveal_window` — 识破后显露窗口 — `GAP-010`
- `ba_gap_candidate.status_stack` — 状态叠加 — `GAP-005`
- `ba_gap_candidate.weakpoint_exposure_window` — 弱点暴露窗口 — `GAP-039`

For every row: `extensionEligible=true`, `decisionBlocked=false`, `runtimeImplemented=false`, `devOnly=true`, `isEnabled=false`, `entersFormalFlow=false`, and `acceptedStableKey=""`. These IDs are survey-only identifiers, not E02 stable keys and not runtime-addressable.

## 6. Runtime-Only Findings

The following 9 occurrences describe execution, timing, state transitions, or runtime facts rather than missing reusable vocabulary:

- `GAP-002` 死亡后短暂骨粉区域
- `GAP-006` 低血量分裂
- `GAP-025` 死亡后短暂骨纹区域
- `GAP-030` 切换两种属性
- `GAP-034` 三身份切换
- `GAP-035` 弱点随身份变化
- `GAP-037` 石相防守
- `GAP-038` 符影进攻
- `GAP-040` 停止攻击并让路

The classification is ownership routing only. No Runtime, Skill, MechanicProfile, BossPhase, CounterWindow, Battle Executor, or formal-flow implementation is claimed.

## 7. Cross-System Contract Findings

The following 8 occurrences require a Battle, targeting, effect, or Item read-only fact contract. Enemy vocabulary cannot substitute for the owning system:

- `GAP-008` 错误目标
- `GAP-009` 交换真假位置
- `GAP-012` 打断玩家一个效果
- `GAP-014` 抢夺增益
- `GAP-018` 打断
- `GAP-026` 封存道具效果
- `GAP-027` 锁定一个器类
- `GAP-043` 空白骨片解除封存

No Item state is read or mutated, no CrossSystem bridge is added, and no Battle execution is implemented.

## 8. User-Decision Exclusions

All four decisions remain unselected:

```text
decisionStatus = USER_DECISION_REQUIRED
sourceSelectedOption = NOT_SELECTED
surveySelectedOption = ""
implementationStatus = NOT_IMPLEMENTED
```

The accepted P0 sheet records `NOT_SELECTED`; the survey's selection field remains the required explicit empty string. The 7 affected atomic occurrences are excluded from extension:

- `BA-D1`: `GAP-032` 保护盲眼女人
- `BA-D2`: `GAP-036` 辅助机制二选一
- `BA-D3`: `GAP-007` 复制一种简单能力; `GAP-022` 复制; `GAP-023` 短暂模仿玩家一个效果
- `BA-D4`: `GAP-041` 交易封存; `GAP-042` 两种能力形态切换

Every affected row has `USER_DECISION_BLOCKED`, `extensionEligible=false`, and `candidateId=""`. No protected target model, false-clone/bidding-shield choice, copy scope, sealed-target granularity, or formal two-form semantics is selected.

Accepted P0 explicitly separates these non-blocked sub-concepts, so they remain independently classified without answering the decisions:

- `GAP-005` 状态叠加 and `GAP-006` 低血量分裂 are independent from BA-D3 copy scope.
- `GAP-021` 偷念 is an independent atomic resource-drain/theft intent exactly covered by `mechanic.energy_drain`; BA-D3 only constrains the same Carrier's Copy/模仿 atoms.
- `GAP-033` “忠”字骨铭核心暴露 is independent from BA-D1 targetability and damage transfer.
- `GAP-034` 三身份切换 and `GAP-035` 弱点随身份变化 are independent from BA-D2 auxiliary choice.
- `GAP-043` 空白骨片解除封存 and `GAP-044` 记忆回流崩解 are independent from BA-D4 sealed-target and formal-form choices.

## 9. Presentation-Only Findings

- `GAP-031` 预演最终 Boss 机制
- `GAP-044` 记忆回流崩解

These are routed to shared encounter presentation/readability only and do not establish gameplay truth.

## 10. Ownership and Implementation State

`BoneAspectMechanicGapSurveyOwnershipMatrix.csv` covers every unique surveyed concept and routes it only to the Assignment-authorized ownership destinations. The matrix is not an implementation plan and creates no direct connection.

Runtime implementation rows: `0`.

Formal-flow references: `0`.

Player-facing answer projection: `0`.

## 11. Output Files

The base package output set remains exactly:

1. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReport.md`
2. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMatrix.csv`
3. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReuseEvidence.csv`
4. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMissingConcepts.csv`
5. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyOwnershipMatrix.csv`
6. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyDecisionExclusions.csv`
7. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv`
8. `Docs/V0.4/Reports/BoneAspectMechanicGapSurveyLeakCheckReport.md`

GuardFix content-modification whitelist: `6 / 6`.

GuardFix new files: `0`.

Existing files outside the accepted base-package output set modified: `0`.

Frozen package files unchanged:

- `BoneAspectMechanicGapSurveyMissingConcepts.csv` = `sha256:f1c20e3473d7280a10cd25bbeced43ff78fa266e927b819fcf93f92dce48efab`
- `BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv` = `sha256:58af2c330a37b993a37ade402f1edba8ef72ec4ef3848b1846dec16fefde57eb`

Unexpected files: `0`.

## 12. Frozen CSV Header Order

`BoneAspectMechanicGapSurveyMatrix.csv`

```text
surveyRowId,sourceCarrierId,sourceKind,sourceField,sourceOrdinal,sourceTextExact,sourceDecisionIds,sourceCrossSystemCandidate,primaryClassification,conceptId,candidateId,matchedE02Category,matchedE02StableKeys,semanticComparison,factOwner,runtimeStateOwner,recommendedNextChannel,extensionEligible,decisionBlocked,runtimeImplemented,evidenceSource,evidenceConfidence,notesDeveloperOnly
```

`BoneAspectMechanicGapSurveyReuseEvidence.csv`

```text
evidenceRowId,surveyRowId,sourceCarrierId,sourceKind,sourceTextExact,sourceCommitmentExact,primaryClassification,matchedE02Category,matchedE02StableKeys,bindingStatus,runtimeImplemented,semanticComparison,evidenceSource,evidenceConfidence,notesDeveloperOnly
```

`BoneAspectMechanicGapSurveyMissingConcepts.csv`

```text
candidateId,conceptLabelZh,definitionNeutral,recommendedE02Category,supportingSurveyRowIds,supportingCarrierIds,sourceTextsExact,e02Comparison,primaryFactOwner,runtimeStateOwner,evidenceConfidence,extensionEligible,decisionBlocked,runtimeImplemented,devOnly,isEnabled,entersFormalFlow,candidateStatus
```

`BoneAspectMechanicGapSurveyOwnershipMatrix.csv`

```text
conceptId,conceptKind,conceptLabelZh,primaryClassification,supportingSurveyRowIds,factOwner,runtimeStateOwner,consumer,forbiddenDirectConnection,recommendedPackageFamily,extensionEligible,decisionBlocked,runtimeImplemented,notesDeveloperOnly
```

`BoneAspectMechanicGapSurveyDecisionExclusions.csv`

```text
decisionId,decisionTitle,decisionStatus,sourceSelectedOption,surveySelectedOption,implementationStatus,affectedSurveyRowId,sourceCarrierId,sourceTextExact,exclusionReason,decisionBlocked,extensionEligible,candidateId,evidenceSource
```

`BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv`

```text
candidateId,conceptLabelZh,definitionNeutral,recommendedE02Category,supportingSurveyRowIds,candidateStatus,extensionEligible,decisionBlocked,runtimeImplemented,devOnly,isEnabled,entersFormalFlow,acceptedStableKey,nextPackageGate,notesDeveloperOnly
```

All 6 CSVs are UTF-8, comma-delimited RFC 4180 data with LF endings, no BOM, no formulas, invariant lowercase booleans, no locale-dependent values, no trailing whitespace, and one final LF.

## 13. Canonical Evidence

Canonical input order:

1. `BoneAspectMechanicGapSurveyMatrix.csv`
2. `BoneAspectMechanicGapSurveyReuseEvidence.csv`
3. `BoneAspectMechanicGapSurveyMissingConcepts.csv`
4. `BoneAspectMechanicGapSurveyOwnershipMatrix.csv`
5. `BoneAspectMechanicGapSurveyDecisionExclusions.csv`
6. `BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv`

Per-file SHA-256:

| File | SHA-256 |
| --- | --- |
| `BoneAspectMechanicGapSurveyMatrix.csv` | `0d029675c9d9079853de446cbc9ec587ddca94de81a845b92051f7de53756a0d` |
| `BoneAspectMechanicGapSurveyReuseEvidence.csv` | `0d1bef56c423d8d4d54ab41b390aad2451cf3985f47e31dd5d93748713fc57fe` |
| `BoneAspectMechanicGapSurveyMissingConcepts.csv` | `f1c20e3473d7280a10cd25bbeced43ff78fa266e927b819fcf93f92dce48efab` |
| `BoneAspectMechanicGapSurveyOwnershipMatrix.csv` | `022e4a30d45801d5e4bd58174e7a550dafc2f9741308f1a82fc989b03ab1bc7b` |
| `BoneAspectMechanicGapSurveyDecisionExclusions.csv` | `11e0aa65c6155c9d54d861c42e875cb309ee22091bdd1679ce084a26d6b241e9` |
| `BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv` | `58af2c330a37b993a37ade402f1edba8ef72ec4ef3848b1846dec16fefde57eb` |

Canonical signature:

```text
sha256:f954a947fe835979bf85c9a65e8a37f0892c3d635805737e0076a7df0c3a0a3e
```

The signature was independently reproduced with Node `crypto` and WebCrypto over the Assignment-defined filename/hash join.

## 14. Protected Baseline and Isolation

Protection uses task-start disk state, never Git HEAD.

The 43 accepted source files remained unchanged:

- P0 accepted reports: 8 / 8; aggregate `sha256:2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5`
- P1 accepted package: 21 / 21; aggregate `sha256:21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209`
- E02 vocabulary package: 14 / 14; aggregate `sha256:b1ee6ea5a5e407b0f809de35d03e3325331824ed296272dcb886fae668d78166`

The broad task-start protected baseline contained 1115 files. Final comparison: 1115 / 1115 unchanged; external drift: `NONE`.

| Protected group | Files | Task-start/final aggregate |
| --- | ---: | --- |
| Items | 141 | `sha256:4b41e4dbede32a14a661f0f985cb1682051c30836b54d811a4c55546652b18e4` |
| BuildSandbox | 180 | `sha256:14ed3ef4fb36acaa0b70d95e25fad2825ef9989bb1d8f0657d67896177bd6a7c` |
| CrossSystem | 63 | `sha256:dfd3445869710d3b28e3ddacdca20a7850ea8c0125246599b14e1ed649e1a64d` |
| Scenes and `.unity` | 14 | `sha256:e11d46573ca87c3cc7809f8c30451ebe73624f8d5b61c40f535576911fcc76a5` |
| Prefabs | 5 | `sha256:a0274a939eeed890c98522bde495a37b2f4b533fdcd753bdb2896cf6b5be485e` |
| Configs | 121 | `sha256:2383359ab713475d8d85748032965c3188f47c436e3644d40abfb54260807769` |
| ProjectSettings | 21 | `sha256:daa82ad48a9ec97754cf9fc42d513c6dbfef7201d2787190cf1dfebfaa6d1d83` |
| Packages | 2 | `sha256:11efaaf4e1df1215e977f884d2d5e2c2bdbbbd1e62337b051f6a0502a929cb6` |
| Guard and queues | 4 | `sha256:e4cfa9f381e62cf296d771fb6c718db26f62e375577056063b7e0923aca221cf` |

No protected file was restored, formatted, staged, or claimed.

## 15. Verification and Stop Gate

- Package-scoped static and text checks: `PASS`
- Leak check: `PASS`
- Unity compile: `NOT_RUN / NOT_REQUIRED`
- Unity Verifier: `NOT_RUN / NOT_REQUIRED`
- Scene handtest: `NOT_RUN / NOT_REQUIRED`
- Active external Unity/ShaderCompiler processes and the project `UnityLockfile` were observed and not touched.
- Package-created processes, helpers, temporary directories, `.meta` files, or locks: `0`
- `commit / tag / push = 0 / 0 / 0`

The next package remains:

```text
V0.4-BoneAspectMechanicVocabularyExtension01
NOT_STARTED / NOT_RELEASED
```

It may be assigned only after Enemy Guard accepts this survey and freezes the exact eligible candidate set.
