# V0.4 BoneAspect Mechanic Gap Survey Leak Check Report

Package: `V0.4-BoneAspectMechanicGapSurvey01-GuardFix01`

Base Assignment Package: `V0.4-BoneAspectMechanicGapSurvey01`

Guard marker: `ENEMY_GUARD_REWORK_BONEASPECTMECHANICGAPSURVEY01_GUARDFIX01`

Status: `PASS`

## 1. Whitelist and Change Boundary

The base package output set remains exactly the 8 Assignment-whitelisted files:

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

Frozen package files:

- `BoneAspectMechanicGapSurveyMissingConcepts.csv` unchanged at `sha256:f1c20e3473d7280a10cd25bbeced43ff78fa266e927b819fcf93f92dce48efab`
- `BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv` unchanged at `sha256:58af2c330a37b993a37ade402f1edba8ef72ec4ef3848b1846dec16fefde57eb`

Unexpected files: `0`.

Non-whitelist `.meta`, temporary directories, and package-owned locks: `0`.

## 2. Required Leak Assertions

| Assertion | Result | Evidence |
| --- | --- | --- |
| Runtime code changes | `0 / PASS` | No `.cs` or runtime file added or modified. |
| Enemy fact-source changes | `0 / PASS` | P0/P1 accepted facts are read-only projections; 43/43 protected source files unchanged. |
| E02 vocabulary changes | `0 / PASS` | 14/14 E02 files unchanged; no accepted stable key minted. |
| Item / BuildSandbox / CrossSystem changes | `0 / PASS` | Task-start protected aggregates unchanged. |
| Battle / Board / Scene / Prefab changes | `0 / PASS` | No write target outside the 8 reports; protected scene/prefab aggregates unchanged. |
| Config / BuildSettings / ProjectSettings changes | `0 / PASS` | No config/settings write; protected aggregates unchanged. |
| Formal flow / Save / Reward / Chapter changes | `0 / PASS` | No implementation or formal-flow content produced. |
| BA-D1..D4 selected options | `0 / PASS` | Four decisions remain `USER_DECISION_REQUIRED`; survey selection is `""`; implementation is `NOT_IMPLEMENTED`. |
| Player-facing answer projection | `0 / PASS` | All decision notes are developer-only evidence; no player-visible answer or route is created. |
| Unexpected files | `0 / PASS` | Package output set equals the exact 8-file whitelist. |

## 3. Survey Integrity

| Check | Result |
| --- | ---: |
| Carrier coverage | 16 / 16 |
| Enemy / Boss | 12 / 4 |
| P1 reuse occurrences | 34 / 34 |
| P0 gap occurrences | 44 / 44 |
| Evidence rows | 78 / 78 |
| Unique primary classifications | 78 / 78 |
| Unresolved classifications | 0 |
| Invalid E02 references | 0 |
| Decision records | 4 / 4 |
| Selected decisions | 0 / 4 |
| Decision-blocked extension rows | 0 |
| Extension candidates with accepted E02 stable keys | 0 |
| Runtime implementation rows | 0 |
| Formal-flow references | 0 |
| Player answer leak | 0 |

Classification total: `43 + 2 + 7 + 9 + 8 + 7 + 2 + 0 = 78`.

The 7 extension-candidate rows alone have `extensionEligible=true`. All 7 decision-blocked occurrences, 9 runtime-only occurrences, 8 cross-system occurrences, 2 presentation-only occurrences, 43 exact reuse occurrences, and 2 composed reuse occurrences are excluded from the extension sheet.

## 4. BA-D1..D4 Exclusion Proof

Every exclusion row records:

```text
decisionStatus = USER_DECISION_REQUIRED
sourceSelectedOption = NOT_SELECTED
surveySelectedOption = ""
implementationStatus = NOT_IMPLEMENTED
decisionBlocked = true
extensionEligible = false
candidateId = ""
```

Affected occurrence coverage:

- `BA-D1`: `GAP-032`
- `BA-D2`: `GAP-036`
- `BA-D3`: `GAP-007`, `GAP-022`, `GAP-023`
- `BA-D4`: `GAP-041`, `GAP-042`

Decision records: 4 / 4. Affected atomic occurrences: 7 / 7. Selected options: 0 / 4.

`GAP-021` is absent from DecisionExclusions. Its retained `sourceDecisionIds=BA-D3` is source projection only: BA-D3 constrains Copy/模仿 scope, while “偷念” independently reuses `mechanic.energy_drain`. Resource identity and actual mutation remain with the existing Player/Battle Resource Owner and cross-system effect contract.

## 5. Extension Boundary Proof

The only handoff rows are:

```text
ba_gap_candidate.charge_attack
ba_gap_candidate.contested_mark
ba_gap_candidate.damage_reduction
ba_gap_candidate.possession_state
ba_gap_candidate.recognition_reveal_window
ba_gap_candidate.status_stack
ba_gap_candidate.weakpoint_exposure_window
```

Each remains `SURVEY_CANDIDATE_ONLY`, has `acceptedStableKey=""`, and is not runtime-addressable. No accepted `mechanic.*`, `pressure.*`, `counter_window.*`, `player_hint.*`, or `diagnostic.*` key was created.

`V0.4-BoneAspectMechanicVocabularyExtension01 = NOT_STARTED / NOT_RELEASED`.

## 6. Protected Baseline Proof

Baseline source: task-start disk state, not Git HEAD.

Accepted source sets:

| Set | Result | Aggregate |
| --- | --- | --- |
| P0 accepted reports | 8 / 8 unchanged | `sha256:2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5` |
| P1 accepted package | 21 / 21 unchanged | `sha256:21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209` |
| E02 vocabulary package | 14 / 14 unchanged | `sha256:b1ee6ea5a5e407b0f809de35d03e3325331824ed296272dcb886fae668d78166` |
| Combined protected source files | 43 / 43 unchanged | `PASS` |

Broad task-start baseline: `1115 / 1115 unchanged`.

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

External concurrent drift: `NONE`.

No protected file was changed, restored, formatted, staged, or claimed by this package.

## 7. CSV and Text Checks

All 6 CSVs were parsed as spreadsheets and independently imported as CSV. Checks:

- exact row counts and frozen header orders;
- exact `34 + 44 = 78` source projection;
- unique `REUSE-001..034` and `GAP-001..044`;
- exactly one primary classification per occurrence;
- exact-match rows reference one accepted E02 key;
- composed rows reference at least two accepted E02 keys;
- all matched keys resolve against accepted E02 inventory;
- only `NEW_VOCABULARY_CANDIDATE` rows enter the extension sheet;
- all extension rows are non-blocked, non-runtime, dev-only, disabled, and outside formal flow;
- no formulas or spreadsheet error values.

All 8 files pass: UTF-8, LF, no BOM, no CR, no trailing whitespace, and exactly one final LF.

Package-scoped text check: `PASS`.

## 8. Canonical Signature

Per-file SHA-256:

| File | SHA-256 |
| --- | --- |
| `BoneAspectMechanicGapSurveyMatrix.csv` | `0d029675c9d9079853de446cbc9ec587ddca94de81a845b92051f7de53756a0d` |
| `BoneAspectMechanicGapSurveyReuseEvidence.csv` | `0d1bef56c423d8d4d54ab41b390aad2451cf3985f47e31dd5d93748713fc57fe` |
| `BoneAspectMechanicGapSurveyMissingConcepts.csv` | `f1c20e3473d7280a10cd25bbeced43ff78fa266e927b819fcf93f92dce48efab` |
| `BoneAspectMechanicGapSurveyOwnershipMatrix.csv` | `022e4a30d45801d5e4bd58174e7a550dafc2f9741308f1a82fc989b03ab1bc7b` |
| `BoneAspectMechanicGapSurveyDecisionExclusions.csv` | `11e0aa65c6155c9d54d861c42e875cb309ee22091bdd1679ce084a26d6b241e9` |
| `BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv` | `58af2c330a37b993a37ade402f1edba8ef72ec4ef3848b1846dec16fefde57eb` |

Assignment-defined canonical signature:

```text
sha256:f954a947fe835979bf85c9a65e8a37f0892c3d635805737e0076a7df0c3a0a3e
```

Node `crypto` and WebCrypto reproduced the same value. This report and the main report record the same canonical signature.

## 9. Unity, Process, and Git Boundary

- Unity compile: `NOT_RUN / NOT_REQUIRED`
- Unity Verifier: `NOT_RUN / NOT_REQUIRED`
- Scene handtest: `NOT_RUN / NOT_REQUIRED`
- Active Unity/ShaderCompiler processes: externally owned, observed only, not waited on, closed, killed, or modified
- Project `Temp/UnityLockfile`: present and externally owned by the active Unity session; not opened for write, removed, or replaced
- Package-owned Unity processes/helpers/temp locks: `0`
- `commit / tag / push = 0 / 0 / 0`
- `stage / reset / rollback / clean = 0 / 0 / 0 / 0`

Final leak result: `PASS`.
