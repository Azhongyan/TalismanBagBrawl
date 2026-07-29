# V0.4-BoneAspectBossArtMechanicLineageSurvey01 Guard Assignment

Guard marker: `ENEMY_GUARD_ASSIGNMENT_BONEASPECTBOSSARTMECHANICLINEAGESURVEY01 / READY_FOR_DEVELOPMENT`

Upstream:

```text
V0.4-BoneAspectContentDesignLock01-GuardFix01 / GUARD_ACCEPTED
V0.4-BoneAspectCarrierPresentationCatalog01 / GUARD_ACCEPTED
V0.4-BoneAspectMechanicGapSurvey01-GuardFix01 / GUARD_ACCEPTED
DIRECT_USER_DESIGN_DECISION_SYNC_AND_NEXT_REVIEW_REQUEST / BA-D1_AND_BA-D2_RESOLVED
```

Risk: `YELLOW / REPORT_ONLY_ART_MECHANIC_LINEAGE_AND_DECISION_OVERLAY`

Owner: Enemy / Boss / Encounter System Guard

Date: `2026-07-27`

## 0. Package Purpose

This package records two newly selected user design decisions without rewriting accepted historical evidence, then audits seven BoneAspect art-mechanic boards as developer design evidence.

It answers:

```text
哪些图中文字已经被 P0/P1 广义承诺支持；
哪些只是表现参考；
哪些是尚未批准的 BossPhase/Skill 候选；
哪些需要未来 Battle/CrossSystem 合同；
哪些已被用户决定明确否决；
每条美术语义未来可进入哪一个数据包。
```

This package does not implement or modify:

```text
Vocabulary
MechanicProfile
SkillPattern
CounterWindow
BossPhase
EnemyRuntimeState
Battle / Targeting / Resource execution
Scene / Prefab / Animator / VFX
Encounter runtime
formal chapter flow
Reward / Drop / Save / RunFlow
```

## 1. Historical Evidence Preservation

The accepted historical files remain immutable:

```text
Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv
Docs/V0.4/Reports/BoneAspectContentDesignLockReport.md
Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv
Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/**
Docs/V0.4/Reports/BoneAspectCarrierPresentation*.*
Docs/V0.4/Reports/BoneAspectMechanicGapSurvey*.*
```

They correctly record the earlier state:

```text
BA-D1 = USER_DECISION_REQUIRED / NOT_SELECTED
BA-D2 = USER_DECISION_REQUIRED / NOT_SELECTED
BA-D3 = USER_DECISION_REQUIRED / NOT_SELECTED
BA-D4 = USER_DECISION_REQUIRED / NOT_SELECTED
```

Do not modify those rows to make history look as if the decision had always existed.

This package creates a report-only decision-resolution overlay:

```text
HistoricalSourceStatus -> NewUserDecisionStatus
```

The overlay is authoritative only for future design packages. It is not a Runtime contract, feature flag, migration, or formal-flow binding.

## 2. Required Reading

Read completely before work:

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
Docs/V0.4/BoneAspectBossArtMechanicLineageSurvey01_Assignment.md
```

Accepted report inputs:

```text
Docs/V0.4/Reports/BoneAspectContentCatalog.csv
Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv
Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv
Docs/V0.4/Reports/BoneAspectVisualLanguageSpec.csv
Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationInventory.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationArtSlotRows.csv
Docs/V0.4/Reports/BoneAspectCarrierPresentationMechanicReferenceRows.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReport.md
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMatrix.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyDecisionExclusions.csv
Docs/V0.4/Reports/BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv
```

Accepted canonical evidence:

```text
P0 canonical:
sha256:3350c02ee92165f46ba87e92056d58b79bd6e2824f5b194cbe5a827eb97d7926

P1 full canonical:
sha256:56c245b02fc6dfeee0e47d37aeab969c34c61ec2e3ac13f1c66abf97b7567f70

Gap Survey canonical:
sha256:f954a947fe835979bf85c9a65e8a37f0892c3d635805737e0076a7df0c3a0a3e
```

## 3. Frozen Art Sources

The package must inspect all seven files in full. They are developer art/design evidence only.

| ArtSourceId | Path | Dimensions | SHA-256 | Board role |
| --- | --- | --- | --- | --- |
| `BA-ART-01` | `C:\Users\Ella\Downloads\file_00000000836881fda70926a2ba8d2f5d.png` | `1536x1024` | `ef3222f1bf10118d2ad7e5d42203cc1a7451c1daa961ae4e197bf3d662b0950b` | 12 Enemy / 4 Boss / 4 chapter / 12 node overview |
| `BA-ART-02` | `C:\Users\Ella\Downloads\file_000000001f6481fd9b963bd9a385e9fc.png` | `1086x1448` | `e7645f5398d24158b1840076afd0025656199b5eb837e407f84ba1ecb8f16dd2` | Bone Guard stage-two concept |
| `BA-ART-03` | `C:\Users\Ella\Downloads\file_000000000f4881fda5a3329d534b9f01.png` | `1024x1536` | `20c33661c2fd4ffb6faa0aa36f32d62c390b8c43d3f35ff593c69dc24f08ed58` | Bone Guard stage-three concept |
| `BA-ART-04` | `C:\Users\Ella\Downloads\file_00000000f20881fda74f80c9f02e5369.png` | `1024x1536` | `dbeab2baa65fe0079ecec008d1d93798818b90a0613460b5493bdc37ef65fd5c` | Bone Guard stage-one concept |
| `BA-ART-05` | `C:\Users\Ella\AppData\Local\Temp\codex-clipboard-bb724b92-05ee-4f5c-bb16-f7ba29e2f95f.png` | `1402x1122` | `54c0b93f5b91eb3896dbfd634e5776bbc16601474b1b9c724175f2558365d067` | Identity Bidder old-man/base concept |
| `BA-ART-06` | `C:\Users\Ella\AppData\Local\Temp\codex-clipboard-a1a67bae-a45f-40ed-a2cb-fddf4d04e76d.png` | `1448x1086` | `08aca8c40873b2ad5fc169295326c6dc278bab2edc3c4359026d22db4f1f1439` | Identity Bidder sequential three-identity rotation concept |
| `BA-ART-07` | `C:\Users\Ella\AppData\Local\Temp\codex-clipboard-8fccd4bf-fbc7-4b50-bc75-25c29cff557d.png` | `1536x1024` | `ecd48e412e660d7873ac109f9261a40bcba8191c23926f58c4e3e11b81558838` | Identity Bidder simultaneous three-form stage, now superseded |

Rules:

- Verify path, dimensions and SHA-256 before transcription.
- Do not copy, move, rename, edit, re-encode, resize or import the images.
- Do not create Unity assets, `.meta` files, Resources keys or Addressables keys.
- Do not infer text that cannot be read reliably.
- An uncertain transcription must be marked `TRANSCRIPTION_UNCERTAIN`, cannot be used to accept a design, and must be excluded from candidate counts.
- Image text is evidence, not a Runtime source.

## 4. Decision Resolution Overlay

Schema:

```text
SchemaId = BoneAspectBossDecisionResolutionOverlay.v1
SchemaVersion = 1
```

Exactly four decision rows are required.

### 4.1 BA-D1: User Selected

Frozen status:

```text
decisionId = BA-D1
historicalStatus = USER_DECISION_REQUIRED
historicalSelectedOption = NOT_SELECTED
overlayStatus = USER_SELECTED
overlaySelection = NARRATIVE_ONLY_NON_RUNTIME_PROTECTED_WOMAN
decisionOwner = USER
effectiveForFuturePackages = true
rewritesHistoricalSource = false
```

Exact semantics:

```text
protectedWomanHasHp = false
protectedWomanTargetable = false
protectedWomanIsRuntimeCombatActor = false
protectedWomanIsDamageReceiver = false
protectedWomanIsTargetSelectionCandidate = false
protectedWomanIsSpawnedUnit = false
protectedWomanOwnsProtectionState = false
protectedWomanIsSeparateTruthSource = false
crossTargetDamageTransferFromWomanEntity = false
```

Allowed retained meaning:

```text
The blind woman remains narrative and presentation identity.
Bone Guard may visually read as guarding/protecting her.
```

Forbidden inference:

```text
No woman HP.
No target selection.
No spawned actor.
No damage routing from or through a woman entity.
No protection-state machine owned by the woman.
No second truth source.
```

The art phrase `替主承伤` does not authorize cross-target damage transfer. Any future player-facing action must be expressed through Bone Guard's own shield, guard, body, movement or self-state and must be independently approved as Boss design.

### 4.2 BA-D2: User Selected

Frozen status:

```text
decisionId = BA-D2
historicalStatus = USER_DECISION_REQUIRED
historicalSelectedOption = NOT_SELECTED
overlayStatus = USER_SELECTED
overlaySelection = FALSE_CLONE
decisionOwner = USER
effectiveForFuturePackages = true
rewritesHistoricalSource = false
```

Exact semantics:

```text
auxiliaryRoute = FALSE_CLONE
sequentialSingleActiveIdentityRotation = true
simultaneousThreeActiveCombatForms = false
simultaneousThreeFormSeizureStageSelected = false
```

The broad future direction remains:

```text
Scholar -> General -> Merchant
Only one identity is active at a time.
```

The package must not invent:

```text
cloneCount
cloneTargetable
cloneHasHp
cloneDamage
cloneLifetime
cloneSpawnTiming
cloneSwapTiming
cloneAI
cloneTargetRules
cloneVisualOnlyOrGameplayActor
```

Each remains `LATER_DESIGN_REQUIRED / NOT_AUTHORED`.

All runtime statements on `BA-ART-07` that depend on simultaneous three active forms, simultaneous body seizure or a simultaneous three-form final stage are:

```text
SUPERSEDED_BY_USER_DECISION
selectedForRuntime = false
```

The image remains historical concept evidence only.

### 4.3 BA-D3 and BA-D4

They remain:

```text
overlayStatus = USER_DECISION_REQUIRED
overlaySelection = NOT_SELECTED
effectiveForFuturePackages = false
rewritesHistoricalSource = false
```

Do not resolve Copy scope, Item internals, sealed-target granularity or Myriad Bone Beast formal two-form semantics.

## 5. Statement Inventory

Create stable statement IDs in this format:

```text
BA-ART-01-S001
BA-ART-01-S002
...
BA-ART-07-S001
```

Order is top-to-bottom, then left-to-right within each board.

The statement inventory must include every readable:

```text
title
identity name
chapter family
encounter node
phase/stage heading
form heading
skill heading
mechanic statement
transition statement
target or actor statement
timing or window statement
damage/effect statement
animation/VFX cue
drop/reward prop
presentation/material statement needed to interpret a mechanic
```

Minimum exact coverage:

```text
Art sources = 7 / 7
Overview identities = 16 / 16
Overview chapter families = 4 / 4
Overview encounter nodes = 12 / 12
Overview broad mechanic marker groups = 16 / 16
Bone Guard stage boards = BA-ART-02, BA-ART-03, BA-ART-04
Identity Bidder boards = BA-ART-05, BA-ART-06, BA-ART-07
All readable phase headings = covered
All readable named skill headings = covered
All D1 actor/target/damage-transfer implications = covered
All D2 rotation/simultaneous-form implications = covered
All displayed drop/reward props = covered as OUT_OF_SCOPE
```

Do not merge separate skill headings into one row.

Do not treat a visual detail as a mechanic unless the board text makes that relation explicit.

## 6. Unique Lineage Classification

Every statement used in the lineage matrix must have exactly one primary classification:

```text
ACCEPTED_BROAD_COMMITMENT
NEW_DESIGN_CANDIDATE
USER_DECISION_RESOLVED
PRESENTATION_ONLY
CROSS_SYSTEM_REQUIRED
SUPERSEDED_BY_USER_DECISION
OUT_OF_SCOPE
```

Meanings:

| Classification | Meaning |
| --- | --- |
| `ACCEPTED_BROAD_COMMITMENT` | Exact P0/P1 broad identity or mechanic commitment already exists. It does not approve phase/skill execution. |
| `NEW_DESIGN_CANDIDATE` | Compatible new Boss phase/skill/action detail that still needs a later design lock. |
| `USER_DECISION_RESOLVED` | The statement's meaning is determined by the BA-D1 or BA-D2 overlay. |
| `PRESENTATION_ONLY` | Silhouette, material, pose, composition or visual motif without gameplay truth. |
| `CROSS_SYSTEM_REQUIRED` | Future execution would require Battle, Targeting, Resource, Item or another owner. This package does not create that contract. |
| `SUPERSEDED_BY_USER_DECISION` | The statement conflicts with the selected BA-D1 or BA-D2 semantics and cannot enter future runtime design. |
| `OUT_OF_SCOPE` | Reward, Drop, Save, formal chapter, marketing, or another non-Enemy owner. |

Rules:

1. A statement receives one primary classification only.
2. `ACCEPTED_BROAD_COMMITMENT` must cite an exact P0/P1 row.
3. `NEW_DESIGN_CANDIDATE` is not approved SkillPattern, BossPhase or CounterWindow data.
4. `USER_DECISION_RESOLVED` must cite BA-D1 or BA-D2 and the exact overlay field.
5. `CROSS_SYSTEM_REQUIRED` must name fact owner, runtime owner and forbidden direct connection.
6. `SUPERSEDED_BY_USER_DECISION` must set:

```text
selectedForRuntime = false
candidateEligible = false
```

7. Any `BA-ART-07` phase/skill/runtime statement requiring simultaneous forms must be `SUPERSEDED_BY_USER_DECISION`.
8. Neutral material or composition details in BA-ART-07 may remain `PRESENTATION_ONLY`, but cannot be used to reconstruct the rejected stage.
9. The blind woman's narrative identity is `USER_DECISION_RESOLVED`; any statement requiring her HP, targetability, actor status, damage receiving or damage-transfer ownership is `SUPERSEDED_BY_USER_DECISION`.
10. Bone Guard shield/body actions compatible with BA-D1 may be `NEW_DESIGN_CANDIDATE`, but cannot inherit target or damage-transfer semantics from the woman.

## 7. Candidate Inventories

Survey-only candidate IDs may use:

```text
ba_boss_phase_candidate.<content_slug>.<ordinal_slug>
ba_boss_skill_candidate.<content_slug>.<ordinal_slug>
```

They are not Runtime IDs and must contain:

```text
candidateStatus = SURVEY_CANDIDATE_ONLY
acceptedDataId = ""
runtimeImplemented = false
devOnly = true
isEnabled = false
entersFormalFlow = false
userApprovalRequired = true
```

Candidate rules:

- Do not create a phase candidate for the rejected simultaneous three-form stage.
- Do not create Skill candidates whose only valid execution requires the woman to be a Runtime actor.
- Sequential scholar/general/merchant rotation is a resolved broad direction, but exact phase count, order timing and per-form skill loadouts remain candidates.
- Exact skill names shown in art are candidates, not accepted data.
- Candidate lists must retain exact supporting statement IDs.
- Candidate lists must not invent cooldowns, damage, target rules, animation timings or AI.

## 8. Exact Output Whitelist

The package may add exactly these 9 files:

```text
Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageSurveyReport.md
Docs/V0.4/Reports/BoneAspectBossArtSourceInventory.csv
Docs/V0.4/Reports/BoneAspectBossArtSourceStatementInventory.csv
Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageMatrix.csv
Docs/V0.4/Reports/BoneAspectBossDecisionResolutionOverlay.csv
Docs/V0.4/Reports/BoneAspectBossPhaseCandidateInventory.csv
Docs/V0.4/Reports/BoneAspectBossSkillCandidateInventory.csv
Docs/V0.4/Reports/BoneAspectBossArtSupersededSemantics.csv
Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageLeakCheckReport.md
```

Existing files modified: `0`.

No `.cs`, `.meta`, `.asset`, `.unity`, `.prefab`, image, animation, VFX, config, localization, Queue or Guard-rule file may be added or modified.

## 9. Required Fields

### 9.1 Source Inventory

```text
artSourceId
sourcePath
width
height
sha256
boardRole
evidenceAuthority
runtimeSource
playerSafe
fileModified
notesDeveloperOnly
```

Fixed:

```text
evidenceAuthority = DEVELOPER_ART_EVIDENCE_ONLY
runtimeSource = false
playerSafe = false
fileModified = false
```

### 9.2 Statement Inventory

```text
statementId
artSourceId
sourceRegion
statementKind
sourceTextExact
transcriptionStatus
contentId
chapterId
sourceOrder
notesDeveloperOnly
```

### 9.3 Lineage Matrix

```text
statementId
artSourceId
contentId
sourceTextExact
statementKind
primaryClassification
acceptedEvidenceId
decisionId
decisionStatus
selectedOption
factOwner
runtimeStateOwner
recommendedNextPackage
candidateId
candidateEligible
selectedForRuntime
runtimeImplemented
playerSafe
semanticReason
forbiddenInference
notesDeveloperOnly
```

### 9.4 Decision Overlay

The decision overlay must provide explicit columns for every frozen BA-D1 and BA-D2 boolean/selection in section 4. No semantics may be hidden in free text only.

It must include source path, historical status, overlay status, overlay selection, evidence date, decision owner, future-package effect and historical rewrite flag.

### 9.5 Superseded Semantics

```text
supersededId
decisionId
statementId
artSourceId
sourceTextExact
supersededMeaning
userSelectedMeaning
selectedForRuntime
candidateEligible
historicalEvidenceRetained
replacementRequiresLaterDesign
notesDeveloperOnly
```

## 10. Canonical Signature

Canonical input order:

```text
1. BoneAspectBossArtSourceInventory.csv
2. BoneAspectBossArtSourceStatementInventory.csv
3. BoneAspectBossArtMechanicLineageMatrix.csv
4. BoneAspectBossDecisionResolutionOverlay.csv
5. BoneAspectBossPhaseCandidateInventory.csv
6. BoneAspectBossSkillCandidateInventory.csv
7. BoneAspectBossArtSupersededSemantics.csv
```

Per file:

```text
UTF-8
no BOM
LF line endings
RFC 4180 CSV
preserve exact bytes otherwise
lowercase SHA-256
```

Join:

```text
fileName + "\0" + lowercaseSha256(fileBytes)
```

Use the exact file order above, separated by LF. Final canonical is lowercase SHA-256 of the UTF-8 joined text.

The main report and leak report must record the same canonical.

## 11. Protected Baseline

Use task-start disk state, not Git HEAD.

Guard-time reference:

```text
P0 accepted reports:
8 files
sha256:2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5

P1 accepted package:
21 files
sha256:21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209

Gap Survey accepted reports:
8 files
sha256:0a79e5e9a40cb8e88d0d25fc0db97972e03452cf3168878df98ed26871d9fca0
```

At task start, freeze exact per-file hashes for those 37 files and for:

```text
Assets/_Game/Scripts/TalismanBag/EnemySystem/**
Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/**
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

Concurrent external drift must be attributed, never restored, reformatted or claimed.

## 12. Static Acceptance

Required:

```text
Art source files = 7 / 7 exact path, dimensions and SHA-256
Art source modifications = 0
Overview identities = 16 / 16 exact accepted content mapping
Overview chapter families = 4 / 4
Overview encounter nodes = 12 / 12
BA-D1 rows = USER_SELECTED / NARRATIVE_ONLY_NON_RUNTIME_PROTECTED_WOMAN
BA-D1 forbidden Runtime booleans = all false
BA-D2 rows = USER_SELECTED / FALSE_CLONE
BA-D2 sequentialSingleActiveIdentityRotation = true
BA-D2 simultaneousThreeActiveCombatForms = false
BA-D3 / BA-D4 = USER_DECISION_REQUIRED / NOT_SELECTED
Simultaneous three-form Runtime statements selected = 0
Superseded simultaneous-stage statements = all covered
Blind-woman Runtime actor/target/damage rows selected = 0
Every readable phase heading = classified once
Every readable skill heading = classified once
Every lineage row has one primary classification
Uncertain transcription accepted as design = 0
Accepted phase IDs = 0
Accepted skill IDs = 0
Runtime implementation rows = 0
Player-facing answer projection = 0
Formal-flow references = 0
Existing files modified = 0
New files = 9 / 9
Unexpected files = 0
UTF-8 / LF / no BOM / no trailing whitespace = 9 / 9
Package-scoped text check = PASS
Protected baseline = unchanged or externally attributed
```

The report must state exact derived counts for:

```text
source statements
lineage rows by classification
phase candidates
skill candidates
superseded statements
uncertain transcriptions
out-of-scope drop/reward rows
```

## 13. Unity and Process Boundary

This package is report-only:

```text
Unity compile = NOT_REQUIRED
Unity Verifier = NOT_REQUIRED
Scene handtest = NOT_REQUIRED
```

Do not start Unity, batch mode, Builder, asset import or any process that can save Scene/Prefab.

Do not close, wait on or terminate the user's Editor or another task's process.

Package-owned `.meta`, Unity locks, helper processes and temp directories must remain `0`.

## 14. Hard Prohibitions

Do not:

- modify P0, P1 or Gap Survey history;
- modify CarrierPresentation code or art slots;
- implement Vocabulary Extension;
- create accepted phase, skill, mechanic, counter-window or runtime IDs;
- treat the art boards as Runtime source;
- make the blind woman an actor, target, HP owner, damage receiver or damage-transfer source;
- select simultaneous three active Identity Bidder forms;
- infer any false-clone implementation detail not explicitly selected by the user;
- resolve BA-D3 or BA-D4;
- modify Item, Battle, Board, CrossSystem, Scene, Prefab, Animator, VFX, UI, Config or BuildSettings;
- modify formal 1-10/2-10, Reward, Drop, Save, RunFlow or Chapter;
- expose the detailed boards as player-facing answer surfaces;
- update Queue or Guard rules;
- commit, tag, push, stage, reset, rollback or clean;
- start any later package.

## 15. Completion and Stop

On completion, send directly:

```text
TASK_STATUS_SYNC_TO_GUARD_REPOOPS

Package:
V0.4-BoneAspectBossArtMechanicLineageSurvey01

Guard marker:
ENEMY_GUARD_ASSIGNMENT_BONEASPECTBOSSARTMECHANICLINEAGESURVEY01
```

Include:

- exact 9-file list;
- seven source path/dimension/hash checks;
- statement and classification counts;
- BA-D1/BA-D2 overlay fields;
- BA-D3/BA-D4 unchanged proof;
- phase/skill candidate lists;
- complete superseded statement list for simultaneous forms and woman-actor implications;
- source/canonical hashes;
- protected baseline and leak result;
- `Unity NOT_RUN / NOT_REQUIRED`;
- `commit / tag / push = 0 / 0 / 0`.

Then stop.

All of these remain:

```text
V0.4-BoneAspectMechanicVocabularyExtension01 = NOT_STARTED / NOT_RELEASED
BoneAspectBossPhaseDesignLock01 = NOT_STARTED / NOT_RELEASED
BoneAspectSkillCounterWindowBossPhaseData01 = NOT_STARTED / NOT_RELEASED
EnemyRuntimeState01 = NOT_STARTED / NOT_RELEASED
```
