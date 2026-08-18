# V0.4 BoneAspect Boss Art Mechanic Lineage Survey Report

Package: `V0.4-BoneAspectBossArtMechanicLineageSurvey01`

Guard marker: `ENEMY_GUARD_ASSIGNMENT_BONEASPECTBOSSARTMECHANICLINEAGESURVEY01`

Assignment SHA-256: `011a429e5da23845da441202e6a51f04f8745cf58639d9eb4add293aa503c74f`

Status: `REPORT_COMPLETE / STATIC_QA_PASS / GUARD_REVIEW_REQUESTED`

## 1. Purpose and Boundary

This report preserves accepted P0/P1/Gap Survey history, overlays the user's new BA-D1 and BA-D2 selections for future design work, and classifies every readable statement from seven frozen art boards. The boards are `DEVELOPER_ART_EVIDENCE_ONLY`; they are not Runtime sources and do not authorize Vocabulary, MechanicProfile, SkillPattern, CounterWindow, BossPhase, EnemyRuntimeState, Battle, Scene, Prefab, Reward, Drop, Save, RunFlow or formal chapter changes.

BA-D3 and BA-D4 remain `USER_DECISION_REQUIRED / NOT_SELECTED`. No historical P0/P1/Gap file was rewritten.

## 2. Art Source Verification

All seven exact paths, dimensions and SHA-256 values matched before transcription. Image modifications, copying, moving, importing, resizing and re-encoding: `0`.

| Source | Dimensions | SHA-256 | Exact path |
| --- | --- | --- | --- |
| `BA-ART-01` | `1536x1024` | `ef3222f1bf10118d2ad7e5d42203cc1a7451c1daa961ae4e197bf3d662b0950b` | `C:\Users\Ella\Downloads\file_00000000836881fda70926a2ba8d2f5d.png` |
| `BA-ART-02` | `1086x1448` | `e7645f5398d24158b1840076afd0025656199b5eb837e407f84ba1ecb8f16dd2` | `C:\Users\Ella\Downloads\file_000000001f6481fd9b963bd9a385e9fc.png` |
| `BA-ART-03` | `1024x1536` | `20c33661c2fd4ffb6faa0aa36f32d62c390b8c43d3f35ff593c69dc24f08ed58` | `C:\Users\Ella\Downloads\file_000000000f4881fda5a3329d534b9f01.png` |
| `BA-ART-04` | `1024x1536` | `dbeab2baa65fe0079ecec008d1d93798818b90a0613460b5493bdc37ef65fd5c` | `C:\Users\Ella\Downloads\file_00000000f20881fda74f80c9f02e5369.png` |
| `BA-ART-05` | `1402x1122` | `54c0b93f5b91eb3896dbfd634e5776bbc16601474b1b9c724175f2558365d067` | `C:\Users\Ella\AppData\Local\Temp\codex-clipboard-bb724b92-05ee-4f5c-bb16-f7ba29e2f95f.png` |
| `BA-ART-06` | `1448x1086` | `08aca8c40873b2ad5fc169295326c6dc278bab2edc3c4359026d22db4f1f1439` | `C:\Users\Ella\AppData\Local\Temp\codex-clipboard-a1a67bae-a45f-40ed-a2cb-fddf4d04e76d.png` |
| `BA-ART-07` | `1536x1024` | `ecd48e412e660d7873ac109f9261a40bcba8191c23926f58c4e3e11b81558838` | `C:\Users\Ella\AppData\Local\Temp\codex-clipboard-8fccd4bf-fbc7-4b50-bc75-25c29cff557d.png` |

## 3. Statement Coverage

| Source | Statement rows |
| --- | ---: |
| `BA-ART-01` | 75 |
| `BA-ART-02` | 41 |
| `BA-ART-03` | 32 |
| `BA-ART-04` | 31 |
| `BA-ART-05` | 35 |
| `BA-ART-06` | 57 |
| `BA-ART-07` | 41 |
| Total | 312 |

Overview coverage:

- identities: `16 / 16` exact accepted content mapping;
- chapter groups: `4 / 4`;
- encounter nodes: `12 / 12` exact accepted node mapping;
- broad mechanic carrier groups: `16 / 16`;
- Bone Guard detail boards: `BA-ART-02 / 03 / 04`;
- Identity Bidder detail boards: `BA-ART-05 / 06 / 07`;
- named phase and skill headings: all readable headings independently inventoried;
- displayed drop/reward props: `4`, all `OUT_OF_SCOPE`;
- uncertain transcriptions: `16`; candidate-eligible uncertain rows: `0`.

## 4. Unique Lineage Classification

| Primary classification | Count |
| --- | ---: |
| `ACCEPTED_BROAD_COMMITMENT` | 66 |
| `NEW_DESIGN_CANDIDATE` | 38 |
| `USER_DECISION_RESOLVED` | 21 |
| `PRESENTATION_ONLY` | 101 |
| `CROSS_SYSTEM_REQUIRED` | 33 |
| `SUPERSEDED_BY_USER_DECISION` | 49 |
| `OUT_OF_SCOPE` | 4 |
| Total | 312 |

Every statement has exactly one allowed primary classification. Runtime-implemented rows, selected-for-runtime rows and player-safe answer rows are all `0`.

`ACCEPTED_BROAD_COMMITMENT` only cites P0/P1 identities, nodes or broad mechanic commitments; it never approves exact phase/skill execution. `NEW_DESIGN_CANDIDATE` and `CROSS_SYSTEM_REQUIRED` remain survey-only. `PRESENTATION_ONLY` never becomes gameplay truth.

## 5. Decision Resolution Overlay

### BA-D1

- historical: `USER_DECISION_REQUIRED / NOT_SELECTED`;
- overlay: `USER_SELECTED / NARRATIVE_ONLY_NON_RUNTIME_PROTECTED_WOMAN`;
- `protectedWomanHasHp=false`;
- `protectedWomanTargetable=false`;
- `protectedWomanIsRuntimeCombatActor=false`;
- `protectedWomanIsDamageReceiver=false`;
- `protectedWomanIsTargetSelectionCandidate=false`;
- `protectedWomanIsSpawnedUnit=false`;
- `protectedWomanOwnsProtectionState=false`;
- `protectedWomanIsSeparateTruthSource=false`;
- `crossTargetDamageTransferFromWomanEntity=false`.

The blind woman remains narrative/presentation identity only. Guarding poses, the “忠” core and Bone Guard's own shield/body actions may be reviewed later, but no woman entity can own HP, targeting, damage, transfer or protection state.

### BA-D2

- historical: `USER_DECISION_REQUIRED / NOT_SELECTED`;
- overlay: `USER_SELECTED / FALSE_CLONE`;
- `sequentialSingleActiveIdentityRotation=true`;
- `simultaneousThreeActiveCombatForms=false`;
- `simultaneousThreeFormSeizureStageSelected=false`;
- broad order retained: `Scholar -> General -> Merchant`, only one active at a time;
- clone count, targetability, HP, damage, lifetime, spawn timing, swap timing, AI, target rules and visual/gameplay actor status: `LATER_DESIGN_REQUIRED / NOT_AUTHORED`.

The bidding-shield branch and all BA-ART-07 simultaneous three-form/seizure Runtime statements are superseded.

### BA-D3 / BA-D4

Both remain `USER_DECISION_REQUIRED / NOT_SELECTED`, `effectiveForFuturePackages=false`, `rewritesHistoricalSource=false`. This package does not select Copy scope, Item internals, sealed-target granularity or the Myriad Bone Beast's formal two-form semantics.

## 6. Superseded Semantics

- total: `49`;
- BA-D1: `22`;
- BA-D2: `27`;
- BA-ART-07 simultaneous-stage statements: `22`;
- selected for Runtime: `0`;
- candidate eligible: `0`.

Complete BA-D1 statement list:

`BA-ART-01-S051;BA-ART-02-S002;BA-ART-02-S003;BA-ART-02-S004;BA-ART-02-S008;BA-ART-02-S009;BA-ART-02-S015;BA-ART-02-S018;BA-ART-02-S024;BA-ART-02-S025;BA-ART-02-S030;BA-ART-02-S031;BA-ART-02-S033;BA-ART-02-S035;BA-ART-03-S024;BA-ART-03-S025;BA-ART-03-S027;BA-ART-04-S016;BA-ART-04-S020;BA-ART-04-S021;BA-ART-04-S025;BA-ART-04-S028`

Complete BA-D2 statement list:

`BA-ART-01-S055;BA-ART-05-S027;BA-ART-05-S028;BA-ART-06-S039;BA-ART-06-S040;BA-ART-07-S001;BA-ART-07-S004;BA-ART-07-S005;BA-ART-07-S007;BA-ART-07-S009;BA-ART-07-S010;BA-ART-07-S013;BA-ART-07-S014;BA-ART-07-S015;BA-ART-07-S016;BA-ART-07-S017;BA-ART-07-S018;BA-ART-07-S020;BA-ART-07-S022;BA-ART-07-S027;BA-ART-07-S028;BA-ART-07-S029;BA-ART-07-S030;BA-ART-07-S033;BA-ART-07-S034;BA-ART-07-S038;BA-ART-07-S041`

Complete BA-ART-07 superseded statement list:

`BA-ART-07-S001;BA-ART-07-S004;BA-ART-07-S005;BA-ART-07-S007;BA-ART-07-S009;BA-ART-07-S010;BA-ART-07-S013;BA-ART-07-S014;BA-ART-07-S015;BA-ART-07-S016;BA-ART-07-S017;BA-ART-07-S018;BA-ART-07-S020;BA-ART-07-S022;BA-ART-07-S027;BA-ART-07-S028;BA-ART-07-S029;BA-ART-07-S030;BA-ART-07-S033;BA-ART-07-S034;BA-ART-07-S038;BA-ART-07-S041`

The exact source text, rejected meaning and user-selected replacement meaning are preserved row-by-row in `BoneAspectBossArtSupersededSemantics.csv`.

## 7. Survey-only Phase Candidates (5)

- `ba_boss_phase_candidate.bone_guard.first_stage_gate_guard_form` — 守门之相 — BA-ART-02-S032;BA-ART-03-S026;BA-ART-04-S002;BA-ART-04-S003;BA-ART-04-S009;BA-ART-04-S031
- `ba_boss_phase_candidate.bone_guard.third_stage_loyal_bone_martyr_form` — 忠骨殉相 — BA-ART-03-S002;BA-ART-03-S003;BA-ART-03-S005;BA-ART-03-S006;BA-ART-03-S028
- `ba_boss_phase_candidate.identity_bidder.scholar_form` — 书生相 — BA-ART-06-S008;BA-ART-06-S046
- `ba_boss_phase_candidate.identity_bidder.general_form` — 将军相 — BA-ART-06-S014;BA-ART-06-S047
- `ba_boss_phase_candidate.identity_bidder.merchant_form` — 富商相 — BA-ART-06-S020;BA-ART-06-S048

No phase candidate exists for `第二阶段：替骨承伤` or `第三阶段·三相夺身`. Accepted phase IDs: `0`.

## 8. Survey-only Skill Candidates (18)

- `ba_boss_skill_candidate.bone_guard.gate_wall` — 守门壁 — BA-ART-04-S018;BA-ART-04-S019
- `ba_boss_skill_candidate.bone_guard.threshold_push` — 门槛推压 — BA-ART-04-S022;BA-ART-04-S023;BA-ART-04-S030
- `ba_boss_skill_candidate.bone_guard.ring_intercept` — 环形拦截 — BA-ART-02-S010;BA-ART-02-S026;BA-ART-02-S027
- `ba_boss_skill_candidate.bone_guard.bone_rope_pull` — 骨绳牵引 — BA-ART-02-S011;BA-ART-02-S016;BA-ART-02-S028;BA-ART-02-S029
- `ba_boss_skill_candidate.bone_guard.loyalty_glyph_shock` — 忠字震击 — BA-ART-03-S018
- `ba_boss_skill_candidate.bone_guard.broken_gate_charge` — 残门突进 — BA-ART-03-S020;BA-ART-03-S021
- `ba_boss_skill_candidate.bone_guard.martyr_bone_rebound` — 殉骨反震 — BA-ART-03-S022
- `ba_boss_skill_candidate.identity_bidder.mark_price` — 标价 — BA-ART-05-S023;BA-ART-05-S024
- `ba_boss_skill_candidate.identity_bidder.read_contract` — 读契 — BA-ART-05-S025;BA-ART-05-S026;BA-ART-06-S027;BA-ART-06-S028
- `ba_boss_skill_candidate.identity_bidder.price_drop_seal` — 落价印 — BA-ART-05-S029;BA-ART-05-S030
- `ba_boss_skill_candidate.identity_bidder.seal_action` — 封招 — BA-ART-06-S029;BA-ART-06-S030
- `ba_boss_skill_candidate.identity_bidder.ink_prison` — 墨落成狱 — BA-ART-06-S031;BA-ART-06-S032
- `ba_boss_skill_candidate.identity_bidder.bidding_charge` — 竞价冲阵 — BA-ART-06-S034
- `ba_boss_skill_candidate.identity_bidder.blazing_blade_sweep` — 烈刃横扫 — BA-ART-06-S035
- `ba_boss_skill_candidate.identity_bidder.marked_price_pursuit` — 标价追索 — BA-ART-06-S036
- `ba_boss_skill_candidate.identity_bidder.buyout_mark` — 买断标记 — BA-ART-06-S041
- `ba_boss_skill_candidate.identity_bidder.account_pressure_array` — 账压成阵 — BA-ART-06-S054
- `ba_boss_skill_candidate.identity_bidder.price_collapse` — 崩价 — BA-ART-07-S031;BA-ART-07-S032

No candidate is created for `替主承伤`, `护板转移`, `护主回身`, `护主绝行`, `契柜代价`, `加价护账`, `三价齐出`, `夺身拉扯` or `无价反噬（破绽）`. Accepted skill IDs: `0`.

Every candidate has `candidateStatus=SURVEY_CANDIDATE_ONLY`, `acceptedDataId=""`, `runtimeImplemented=false`, `devOnly=true`, `isEnabled=false`, `entersFormalFlow=false`, and `userApprovalRequired=true`.

## 9. Canonical Evidence

Canonical input order is the Assignment-defined seven CSV order. Each CSV is UTF-8, LF, no BOM, RFC 4180; the join is `fileName + NUL + lowercaseSha256`, separated by LF.

| CSV | SHA-256 |
| --- | --- |
| `BoneAspectBossArtSourceInventory.csv` | `0af7437ef77673e2de2f468ccae488422d7484b9291a6e42a0ae2867ae3f393f` |
| `BoneAspectBossArtSourceStatementInventory.csv` | `615d5591bebc4448beeff05f8f6f56c8a7304bd668f5953aa7f397f3329c0c2c` |
| `BoneAspectBossArtMechanicLineageMatrix.csv` | `42f9c2d83df2394dd71704922f1402036939a0694cf7441e4eda2759003c7978` |
| `BoneAspectBossDecisionResolutionOverlay.csv` | `fa34c48320a8e7ca2a8a299ebe3344a9d3f749051de9e1961ff955fa06443923` |
| `BoneAspectBossPhaseCandidateInventory.csv` | `163f04532010c33d929fcb6fe3fe430ad7736c37ee0f79985d54a07fb07972e4` |
| `BoneAspectBossSkillCandidateInventory.csv` | `efe3d055845928ddfad48442c01486e22a0131947f69cb736c026429c4e476be` |
| `BoneAspectBossArtSupersededSemantics.csv` | `9eea47466f96c0c50f2ad02e6dba3195d42c079ea547e3ab716a30fbd415a137` |

Canonical signature:

`sha256:4d6ff9619133ed654db8f26941c2eec6a3016a5f1f9abb208823262039ec24e0`

The same value is recorded in the Leak Check report.

## 10. Protected Baseline

Task-start disk state, not Git HEAD, is the baseline.

- P0 accepted reports: `8 / 8`, `sha256:2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5`;
- P1 accepted package: `21 / 21`, `sha256:21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209`;
- Gap Survey accepted reports: `8 / 8`, `sha256:0a79e5e9a40cb8e88d0d25fc0db97972e03452cf3168878df98ed26871d9fca0`.

| Protected group | Files | Task-start/final aggregate |
| --- | ---: | --- |
| EnemySystem | 108 | `sha256:6b8b5f14b9841eaeb7872077401287de71b384db27f58310e19faae27bb001ee` |
| EditorEnemySystem | 26 | `sha256:d7631c471509891eaab6904873f15ead4970f0466a1bdce204e68ca8b2ebc111` |
| Items | 141 | `sha256:4b41e4dbede32a14a661f0f985cb1682051c30836b54d811a4c55546652b18e4` |
| BuildSandbox | 180 | `sha256:2c0845949e347db3ad475df4f2b0f7c85f3d4afff684974b01039ab15ea64cc5` |
| CrossSystem | 63 | `sha256:dfd3445869710d3b28e3ddacdca20a7850ea8c0125246599b14e1ed649e1a64d` |
| ScenesAndUnity | 14 | `sha256:e11d46573ca87c3cc7809f8c30451ebe73624f8d5b61c40f535576911fcc76a5` |
| Prefabs | 5 | `sha256:a0274a939eeed890c98522bde495a37b2f4b533fdcd753bdb2896cf6b5be485e` |
| Configs | 121 | `sha256:2383359ab713475d8d85748032965c3188f47c436e3644d40abfb54260807769` |
| ProjectSettings | 21 | `sha256:1969814e5ec3983ba426b7d5f19f13de7f480277cb5118a92d06c66b58fcbac7` |
| Packages | 2 | `sha256:11efaaf4e1df1215e977f884d2d5e2c2bdbbbd1e62337b0517f6a0502a929cb6` |
| GuardAndQueues | 4 | `sha256:e4cfa9f381e62cf296d771fb6c718db26f62e375577056063b7e0923aca221cf` |

Final static recheck status: `UNCHANGED / NO_EXTERNAL_DRIFT OBSERVED`. No protected file was restored, reformatted or claimed.

## 11. Output and Stop

1. `Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageSurveyReport.md`
2. `Docs/V0.4/Reports/BoneAspectBossArtSourceInventory.csv`
3. `Docs/V0.4/Reports/BoneAspectBossArtSourceStatementInventory.csv`
4. `Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageMatrix.csv`
5. `Docs/V0.4/Reports/BoneAspectBossDecisionResolutionOverlay.csv`
6. `Docs/V0.4/Reports/BoneAspectBossPhaseCandidateInventory.csv`
7. `Docs/V0.4/Reports/BoneAspectBossSkillCandidateInventory.csv`
8. `Docs/V0.4/Reports/BoneAspectBossArtSupersededSemantics.csv`
9. `Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageLeakCheckReport.md`

Existing files modified: `0`. New files: `9 / 9`. Unexpected files: `0`.

Unity compile / Verifier / Scene handtest: `NOT_RUN / NOT_REQUIRED`. Unity, batch, Builder, import and Scene/Prefab save were not started. Package-owned Unity lock, helper process, temporary directory and .meta file: `0` after cleanup.

`commit / tag / push = 0 / 0 / 0`.

Later packages remain `NOT_STARTED / NOT_RELEASED`.
