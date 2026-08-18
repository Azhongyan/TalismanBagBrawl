# V0.4-BoneAspectMechanicGapSurvey01 Guard Assignment

Guard marker: `ENEMY_GUARD_ASSIGNMENT_BONEASPECTMECHANICGAPSURVEY01 / READY_FOR_DEVELOPMENT`

Upstream:

```text
V0.4-BoneAspectContentDesignLock01-GuardFix01 / GUARD_ACCEPTED
V0.4-BoneAspectCarrierPresentationCatalog01 / GUARD_ACCEPTED / QA_PASS
```

Risk: `YELLOW / REPORT_ONLY_MECHANIC_GAP_SURVEY`

Owner: Enemy / Boss / Encounter System Guard

Date: `2026-07-27`

## 0. Package Position

This is the next BoneAspect Enemy data-line package after the accepted Carrier Presentation Catalog.

The legacy `ENEMY_SYSTEM_PACKAGE_QUEUE.md` has no registered post-E10 executable package. It does not authorize jumping directly into a vocabulary implementation. The accepted P1 catalog instead freezes:

```text
gapSurveyRequired = 16 / 16
runtimeImplemented = false
```

Therefore this package is a narrow evidence survey before `V0.4-BoneAspectMechanicVocabularyExtension01`.

Plain purpose:

```text
逐条确认《骨相》16 个 Carrier 的策划现象中：
哪些已经能精确复用 E02；
哪些可以由现有 E02 组合表达；
哪些确实缺少中立词汇；
哪些其实属于 Battle Runtime、跨系统合同、表现层或用户未决设计。
```

This package does not add vocabulary, implement mechanics, or select user decisions.

## 1. Required Reading

Before work, read completely:

```text
AGENTS.md
Docs/LOCKED/*
Docs/ROADMAP/VERSION_ROADMAP.md
Docs/CURRENT/V0.3_PRODUCT_FLOW01.md
Docs/V0.3/V0.3_PACKAGE_QUEUE.md
Docs/ROADMAP/V0.4_BUILD_SYNERGY_ROADMAP.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
Docs/V0.4/UnifiedBattlePageStrategy_GuardSync.md
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/SHARED_MODULE_LAYERING_AND_PREFAB_PRESENTATION_GUARD.md
Docs/V0.4/BoneAspectContentDesignLock01_Assignment.md
Docs/V0.4/BoneAspectCarrierPresentationCatalog01_Assignment.md
Docs/V0.4/BoneAspectMechanicGapSurvey01_Assignment.md
```

Authoritative accepted inputs:

```text
Docs/V0.4/Reports/BoneAspectContentDesignLockReport.md
Docs/V0.4/Reports/BoneAspectContentCatalog.csv
Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv
Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogReport.md
Docs/V0.4/Reports/BoneAspectCarrierPresentationInventory.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationMechanicReferenceRows.csv
Docs/V0.4/Reports/EnemyMechanicVocabularyInventory.csv
Docs/V0.4/Reports/EnemyMechanicVocabularySpec.csv
Docs/V0.4/Reports/EnemyMechanicVocabularyLegacyMapping.csv
Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/**
```

Accepted canonical evidence:

```text
P0 canonical:
sha256:3350c02ee92165f46ba87e92056d58b79bd6e2824f5b194cbe5a827eb97d7926

P1 full canonical:
sha256:56c245b02fc6dfeee0e47d37aeab969c34c61ec2e3ac13f1c66abf97b7567f70

P1 player-safe canonical:
sha256:4e4882ba5ffa77dcc9c9d0d7372c874971808878ca2b8e6d8e0346331ff1cd24

P1 E02 dependency canonical:
sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833
```

## 2. Exact Survey Universe

The survey must cover exactly:

```text
Carrier rows = 16
Enemy rows = 12
Boss rows = 4
Accepted P1 E02 candidate references = 34
P0 newMechanicGapCandidates atomic occurrences = 44
Total evidence occurrences = 78
User decisions = BA-D1, BA-D2, BA-D3, BA-D4
```

The 34 P1 references must be projected exactly from:

```text
BoneAspectCarrierPresentationMechanicReferenceRows.csv
```

Each gets a stable survey row ID:

```text
REUSE-001 .. REUSE-034
```

The 44 gap occurrences must be projected in Ordinal carrier order and source-field order from:

```text
BoneAspectMechanicCommitmentMatrix.csv.newMechanicGapCandidates
```

Each gets a stable survey row ID:

```text
GAP-001 .. GAP-044
```

Do not merge duplicate occurrences before evidence classification. A separate candidate/concept sheet may deduplicate them only through explicit occurrence references.

If the accepted source does not yield exactly `34 + 44`, stop with `GUARD_RETURN_SOURCE_COUNT_MISMATCH`. Do not repair or reinterpret P0/P1 source data in this package.

## 3. Unique Primary Classification

Every one of the 78 evidence rows must have exactly one `primaryClassification`:

```text
EXACT_E02_REUSE
COMPOSED_FROM_EXISTING_E02
NEW_VOCABULARY_CANDIDATE
RUNTIME_ONLY_NOT_VOCABULARY
CROSS_SYSTEM_CONTRACT_REQUIRED
USER_DECISION_BLOCKED
PRESENTATION_ONLY
OUT_OF_SCOPE
```

Semantics:

| Classification | Meaning |
| --- | --- |
| `EXACT_E02_REUSE` | One existing E02 stable key expresses the planning concept without semantic widening. |
| `COMPOSED_FROM_EXISTING_E02` | Two or more existing E02 keys can express the planning concept; no new atomic key is justified. |
| `NEW_VOCABULARY_CANDIDATE` | The planning concept is stable, Enemy-owned, not decision-blocked, and not expressible by existing E02 vocabulary. This is only a candidate for the later package. |
| `RUNTIME_ONLY_NOT_VOCABULARY` | The statement describes timing, execution success, target legality, state transition, damage routing, or another runtime fact rather than a reusable vocabulary concept. |
| `CROSS_SYSTEM_CONTRACT_REQUIRED` | The Enemy side may declare intent, but another domain owns the fact or execution. No Enemy vocabulary entry can substitute for that contract. |
| `USER_DECISION_BLOCKED` | The exact behavior depends on BA-D1..D4 and must remain unselected. |
| `PRESENTATION_ONLY` | The statement only requires visual/readability treatment and does not define gameplay truth. |
| `OUT_OF_SCOPE` | The statement belongs to formal chapter, Reward, Save, Drop, Operations, Marketing, costume, login, or another excluded owner. |

Secondary evidence fields may describe ownership or recommended later handling, but must not create a second primary classification.

## 4. Evidence Rules

Each survey row must contain at least:

```text
surveyRowId
sourceCarrierId
sourceKind
sourceField
sourceOrdinal
sourceTextExact
sourceDecisionIds
sourceCrossSystemCandidate
primaryClassification
matchedE02Category
matchedE02StableKeys
semanticComparison
factOwner
runtimeStateOwner
recommendedNextChannel
extensionEligible
decisionBlocked
evidenceSource
evidenceConfidence
notesDeveloperOnly
```

Rules:

1. `sourceTextExact` must preserve the accepted P0/P1 value. No synonym replacement.
2. All matching is semantic and evidence-backed. Names, art keys, Chinese display labels, tags, VFX, legacy BP, and Item names are not evidence.
3. `EXACT_E02_REUSE` must reference exactly one existing E02 key.
4. `COMPOSED_FROM_EXISTING_E02` must reference at least two existing E02 keys and explain why the combination does not create a new atomic mechanic.
5. `NEW_VOCABULARY_CANDIDATE` must not mint an accepted `mechanic.*`, `pressure.*`, `counter_window.*`, `player_hint.*`, or `diagnostic.*` key.
6. A neutral survey candidate ID may be used only in this format:

```text
ba_gap_candidate.<ordinal_slug>
```

It is not an E02 stable key and is not runtime-addressable.
7. `RUNTIME_ONLY_NOT_VOCABULARY`, `CROSS_SYSTEM_CONTRACT_REQUIRED`, `PRESENTATION_ONLY`, and `OUT_OF_SCOPE` must have `extensionEligible=false`.
8. No row may claim Runtime, Skill, MechanicProfile, BossPhase, CounterWindow, Battle Executor, or formal flow implementation exists for BoneAspect.

## 5. Ownership Review

The survey may recommend only these ownership destinations:

```text
ENEMY_DATA
ENEMY_RUNTIME_STATE
BATTLE_RESOLVER
BATTLE_FIELD_RUNTIME
BATTLE_TARGETING_RUNTIME
CROSS_SYSTEM_EFFECT_CONTRACT
ITEM_READONLY_FACT_CONTRACT
SHARED_ENCOUNTER_PRESENTATION
FORMAL_PRODUCT_FLOW_OUT_OF_SCOPE
USER_DESIGN_DECISION
```

Guard boundaries:

- Enemy data may describe mechanic intent, skill intent, phase intent, and player-safe feedback categories.
- Enemy Runtime may later own only current Enemy HP, shell/shield, cast, status, phase, form, and Enemy-side relation state.
- Battle owns target legality, hit resolution, damage transfer, successful interrupt, field hazard execution, resource mutation, and result facts.
- Item owns Item identity/category/effect facts. Enemy must not create a second Item truth source.
- Shared Encounter Presentation owns arena visual profiles and sidecar bindings. Scene and Prefab do not own mechanics.
- ProductFlow, Reward, Save, Drop, Operations, Marketing, costume, and login content remain outside this package.

The survey must not turn an ownership destination into an implementation.

## 6. BA-D1..D4 Decision Exclusion

All four decisions must remain:

```text
decisionStatus = USER_DECISION_REQUIRED
selectedOption = ""
implementationStatus = NOT_IMPLEMENTED
```

Minimum exclusion rules:

- `BA-D1`: do not select the real protected target, targetability model, or damage-transfer execution.
- `BA-D2`: do not select false clone versus bidding shield.
- `BA-D3`: do not select visual-only, numeric copy, trigger copy, or execution scope.
- `BA-D4`: do not select sealed target granularity or the formal semantics of the two forms.

Every affected atomic occurrence must be classified `USER_DECISION_BLOCKED`, unless the accepted P0 decision sheet explicitly separates a stable non-blocked sub-concept. Such separation must cite the exact accepted field and must not infer from names or art.

Decision-blocked rows:

```text
extensionEligible = false
candidateId = ""
selectedOption = ""
```

These rows cannot enter `BoneAspectMechanicVocabularyExtension01`.

## 7. Required Outputs

The package may add exactly these 8 files:

```text
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReport.md
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMatrix.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReuseEvidence.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMissingConcepts.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyOwnershipMatrix.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyDecisionExclusions.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyLeakCheckReport.md
```

Existing files modified: `0`.

No `.cs`, `.meta`, `.asset`, `.unity`, `.prefab`, image, ScriptableObject, config, queue, Guard rule, or localization file may be added or changed.

### 7.1 Report

The main report must state:

- why the survey precedes vocabulary extension;
- exact source counts and coverage;
- counts by primary classification;
- exact E02 reuse result;
- exact list of extension-eligible neutral candidate IDs;
- exact list of runtime/cross-system/decision-blocked findings;
- unresolved evidence count;
- explicit next-package gate.

### 7.2 Matrix

`BoneAspectMechanicGapSurveyMatrix.csv` is the complete 78-row evidence matrix.

### 7.3 Reuse Evidence

`BoneAspectMechanicGapSurveyReuseEvidence.csv` contains:

- all 34 accepted P1 candidate references;
- every additional exact or composed E02 match found among the 44 gap rows;
- exact source row references and semantic comparison.

It must not change the P1 `CandidateReuseOnly` status.

### 7.4 Missing Concepts

`BoneAspectMechanicGapSurveyMissingConcepts.csv` contains only deduplicated `NEW_VOCABULARY_CANDIDATE` concepts. It must include every supporting `GAP-*` occurrence and cannot include decision-blocked or runtime-only rows.

### 7.5 Ownership Matrix

`BoneAspectMechanicGapSurveyOwnershipMatrix.csv` maps every unique surveyed concept to fact owner, runtime owner, consumer, forbidden direct connection, and recommended package family.

### 7.6 Decision Exclusions

`BoneAspectMechanicGapSurveyDecisionExclusions.csv` must cover BA-D1..D4 and every affected occurrence. It records no selected option.

### 7.7 Extension Candidate Sheet

This sheet is the only allowed handoff into the later vocabulary package. Every row must be:

```text
extensionEligible = true
decisionBlocked = false
runtimeImplemented = false
devOnly = true
isEnabled = false
entersFormalFlow = false
```

It may recommend an E02 category but must not create an accepted stable key.

### 7.8 Leak Check

The leak report must prove:

```text
Runtime code changes = 0
Enemy fact-source changes = 0
E02 vocabulary changes = 0
Item / BuildSandbox / CrossSystem changes = 0
Battle / Board / Scene / Prefab changes = 0
Config / BuildSettings / ProjectSettings changes = 0
Formal flow / Save / Reward / Chapter changes = 0
BA-D1..D4 selected options = 0
Player-facing answer projection = 0
Unexpected files = 0
```

## 8. Protected Baseline

Protection uses task-start disk state, not Git HEAD.

Guard-time reference aggregates were calculated on `2026-07-27` from:

```text
aggregateInput = Ordinal-sorted "repoRelativePath\0lowercaseFileSha256" lines
encoding = UTF-8
lineSeparator = LF
hash = SHA-256
```

Reference groups:

```text
P0 accepted reports: 8 files
sha256:2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5

P1 accepted package: 21 files
sha256:21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209

E02 vocabulary package: 14 files
sha256:b1ee6ea5a5e407b0f809de35d03e3325331824ed296272dcb886fae668d78166
```

At development start, capture exact per-file hashes for those 43 protected files and for these read-only areas:

```text
Assets/_Game/Scripts/TalismanBag/Items/**
Assets/_Game/Scripts/TalismanBag/BuildSandbox/**
Assets/_Game/Scripts/TalismanBag/CrossSystem/**
Assets/_Game/Scenes/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/_Game/Configs/**
ProjectSettings/**
Packages/**
Docs/V0.4/EnemySystemGuard_CurrentRules.md
Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PACKAGE_QUEUE.md
Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md
```

If concurrent Item/BuildSandbox work changes protected files, do not restore, format, stage, or claim them. Record exact external drift and request owner attribution. Package scope can pass only if all 8 allowed outputs are attributable to this package and protected drift is externally owned.

## 9. Unity and Concurrency

This package is report-only:

```text
Unity compile = NOT_REQUIRED
Unity Verifier = NOT_REQUIRED
Scene handtest = NOT_REQUIRED
```

At Guard freeze time, Unity processes and the project `UnityLockfile` were active. Therefore:

- do not launch Unity batch;
- do not wait on, close, or terminate the user's Editor or another task's process;
- do not run Builder, asset import, report regeneration through Unity, or any command that may save Scene/Prefab;
- use static, offline, read-only inspection only.

Any non-whitelist `.meta`, temp directory, or lock created by this task is a package failure and must not be retained.

## 10. Verification

Required static acceptance:

```text
Carrier coverage = 16 / 16
Enemy / Boss = 12 / 4
P1 reuse occurrences = 34 / 34
P0 gap occurrences = 44 / 44
Evidence rows = 78 / 78
Unique primary classifications = 78 / 78
Unresolved classifications = 0
Invalid E02 references = 0
Decision records = 4 / 4
Selected decisions = 0 / 4
Decision-blocked extension rows = 0
Extension candidates with accepted E02 stable keys = 0
Runtime implementation rows = 0
Formal flow references = 0
Player answer leak = 0
Existing files modified = 0
New files = 8 / 8
Unexpected files = 0
UTF-8 / LF / no BOM / no trailing whitespace = 8 / 8
Package-scoped text check = PASS
Protected source groups = unchanged or externally attributed
```

CSV rules:

- UTF-8, comma-delimited, RFC 4180 quoting;
- header order frozen by the report and repeated generation;
- Ordinal ordering only;
- invariant booleans `true` / `false`;
- no formulas;
- no locale-dependent numbers or dates;
- empty string is explicit and not replaced by `null`, `0`, `false`, or `NotApplicable`.

Canonical signature:

```text
Input files:
1. BoneAspectMechanicGapSurveyMatrix.csv
2. BoneAspectMechanicGapSurveyReuseEvidence.csv
3. BoneAspectMechanicGapSurveyMissingConcepts.csv
4. BoneAspectMechanicGapSurveyOwnershipMatrix.csv
5. BoneAspectMechanicGapSurveyDecisionExclusions.csv
6. BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv

Per file:
Normalize line endings to LF.
Require no BOM.
Preserve exact UTF-8 bytes otherwise.

Join:
fileName + "\0" + lowercaseSha256(fileBytes)
in the exact file order above, separated by LF.

Final:
lowercase SHA-256 of the UTF-8 joined text.
```

The main report and leak report must record the same canonical value.

## 11. Hard Prohibitions

Do not:

- modify E02 vocabulary or mint accepted vocabulary keys;
- implement `BoneAspectMechanicVocabularyExtension01`;
- modify P0/P1 facts or reports;
- create MechanicProfile, SkillPattern, CounterWindow, BossPhase, Encounter, Wave, Slot, MapRule, requirement, readiness, or seed data;
- implement EnemyRuntimeState or any Battle execution;
- read or mutate Item runtime state;
- modify Item, Board, BuildSandbox, CrossSystem bridges, Scene, Prefab, UI, Config, BuildSettings, or ProjectSettings;
- modify formal 1-10 / 2-10, RunFlow, SaveData, Reward, Drop, Chapter, Operations, Marketing, costume, or login systems;
- choose BA-D1, BA-D2, BA-D3, or BA-D4;
- infer mechanics from carrier names, art, presentation keys, labels, VFX, Item names, or legacy BP;
- run Unity, Builder, or a report writer that touches non-whitelist files;
- update Package Queue or Guard rules;
- commit, tag, push, reset, rollback, clean, or stage;
- start P3 or any later package.

## 12. Completion and Stop

When complete, send directly:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BoneAspectMechanicGapSurvey01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_BONEASPECTMECHANICGAPSURVEY01
```

The status must include:

- exact 8-file list;
- 16/34/44/78 coverage;
- counts by primary classification;
- extension-eligible candidate count and IDs;
- BA-D1..D4 exclusion result;
- protected baseline result and any externally attributed drift;
- canonical signature;
- leak and text checks;
- `Unity NOT_RUN / NOT_REQUIRED`;
- `commit / tag / push = 0 / 0 / 0`.

Then stop.

`V0.4-BoneAspectMechanicVocabularyExtension01` remains:

```text
NOT_STARTED / NOT_RELEASED
```

It may be assigned only after Enemy Guard accepts this survey and freezes the exact eligible vocabulary candidates.
