# V0.4-C1Stage3To5PlaytestEnemyEncounterConfig01

Guard marker: `ENEMY_GUARD_ASSIGNMENT_C1STAGE3TO5PLAYTESTENEMYENCOUNTERCONFIG01`

## 1. Outcome

Add one immutable Enemy-owned sidecar catalog for Chapter 1 stages `1-3`,
`1-4`, and `1-5`. The rows are the user-approved `PLAYTEST_V1` numbers for
the `CAMPAIGN_NORMAL_LV1` route. They are deliberately not final balance.

This package is a technical prerequisite only:

```text
PlayerVisibleDelivery=NONE
MilestoneCompletionEvidence=NO
FreshPlay=NOT_APPLICABLE_UNTIL_MAINLINE_MOUNTS_THE_CATALOG
```

Existing `1-1` and `1-2` Enemy balance sources must remain byte-identical.

## 2. Task Boundary

| Field | Value |
|---|---|
| Task class | `COMPLEX_GUARDED_ONCE / PURE ENEMY CONFIG AND CONTRACT` |
| Primary Guard | Enemy Guard |
| Development owner | one visible local Enemy development task |
| User Review Gate | `YES / COMPLETE` |
| Accepted scope | `DIRECT_SUPPLIER_REQUEST_C1_1_3_TO_1_5_PLAYTEST_V1_ENEMY_ENCOUNTER_CONFIG` |
| Product context | `CAMPAIGN_NORMAL_LV1` |
| Candidate disposition | `PLAYTEST_V1_NOT_FINAL_BALANCE` |
| Further numeric decision | `NONE` |

The prior producer and balance gates produced the candidate table. The user
has explicitly accepted that table as `PLAYTEST_V1`; do not reopen tuning in
this package.

## 3. Technical Director Decision

- Authoritative carrier: new read-only Enemy Config/System sidecar.
- Runtime truth owner: Enemy owns enemy identity, HP and action/operator
  identities; Battle later owns clock, target selection, damage application,
  player HP and event ledger.
- Reused carriers: existing C1 Enemy runtime identities, existing Bone Swap
  operator identity, existing Item build-envelope APIs, and existing Chapter 1
  roster truth as protected read-only evidence.
- New Scene: none.
- New Prefab: none.
- Scene/Prefab/material/import serialization: none.
- Unity lease: not required; Unity must not be started.
- QA class: focused C# compile plus deterministic verifier/simulation.
- Mainline later supplies StageConfig/launch context and Battle binding. This
  package cannot be played directly.

## 4. Player Promise Fidelity

```text
MilestoneId: C1_STAGE1_TO5_CORE_PLAYABILITY_SLICE
PlayerPromise: From WorldMap, stages 1-1 through 1-5 run as live Unified
  battles with stage-specific enemy rosters, HP and actions, and real victory
  or defeat from the Battle Session.
PackageContribution: immutable Enemy facts for 1-3 through 1-5 only.
ExplicitNonCompletionCases: catalog validation, compilation or simulation do
  not prove WorldMap routing, live Battle application, visible enemies,
  rewards, save, presentation or Fresh Play.
RequiredRealPathOwner: Mainline Guard.
```

## 5. Stable Identities

```text
SchemaId = C1Stage3To5PlaytestEnemyEncounter.v1
ProfileRevision = C1_STAGE3_TO5_PLAYTEST_ENEMY_ENCOUNTER_R1
ProductContext = CAMPAIGN_NORMAL_LV1
CandidateDisposition = PLAYTEST_V1_NOT_FINAL_BALANCE
ActivationStatus = PLAYTEST_V1_RELEASED_FOR_FORMAL_CONSUMPTION
BalanceProfileId = campaign.normal.lv1.balance.c1.stage3to5.playtest.v1

Stage1_3 = 1-3
EncounterVariant1_3 = campaign.normal.lv1.encounter.c1.1-3.playtest.v1
Stage1_4 = 1-4
EncounterVariant1_4 = campaign.normal.lv1.encounter.c1.1-4.playtest.v1
Stage1_5 = 1-5
EncounterVariant1_5 = campaign.normal.lv1.encounter.c1.1-5.playtest.v1
```

Use existing exact Enemy identities and action IDs; do not create aliases:

```text
bone_aspect_enemy_c1_01_shattered_host
bone_aspect.runtime.c1.shattered_host.v1
c1.shattered_host.basic_attack
c1.shattered_host.polluted_pulse_skill

bone_aspect_enemy_c1_02_porcelain_hound
bone_aspect.runtime.c1.porcelain_hound.v1
c1.porcelain_hound.charge_attack

bone_aspect_enemy_c1_03_bone_swap_remnant
bone_aspect.runtime.c1.bone_swap_remnant.operator.v1
c1.bone_swap_remnant.return_last_direct_pulse
```

## 6. Approved Encounter Rows

All three profiles must expose `isEnabled=true`, `entersFormalFlow=true`, and
`runtimeBoundToBattle=false`. The last flag is false because the Battle
adapter is outside this package.

| Stage | Roster in exact order | Total HP |
|---|---|---:|
| `1-3` | Shattered Host `500`; Porcelain Hound `600`; Porcelain Hound `600` | `1700` |
| `1-4` | Porcelain Hound `720`; Bone Swap Remnant `930` | `1650` |
| `1-5` | Shattered Host `500`; Porcelain Hound `600`; Bone Swap Remnant `700` | `1800` |

Each profile has one encounter-wave basic damage row:

```text
cadenceScope = ENCOUNTER_WAVE_SINGLE_APPLICATION
effectRequestKey = battle.effect_request.direct_player_damage
firstResolveSeconds = 4.5
intervalSeconds = 4.5
damagePerApplication = 7
applicationsPerWave = 1
```

Actor rows must expose at least:

```text
actorBalanceId
contentId
runtimeProfileId or operatorProfileId
occurrenceOrdinal
maxHp
actionIds[]
effectRequestKeys[]
canonicalSignature
```

The catalog must not copy old dev/showcase HP, shell, cooldown or action timing.
Shattered Host and Porcelain Hound may reference existing action IDs and their
existing request keys, but those references do not authorize Battle execution.
If an existing request is still dev-only, expose the explicit downstream
condition `BATTLE_CONTRACT_REQUIRED`; do not relabel it as supported.

## 7. Bone Swap PLAYTEST_V1 Release Row

Add one immutable mapping row for `1-4` and `1-5` consumption:

```text
contentId = bone_aspect_enemy_c1_03_bone_swap_remnant
operatorProfileId = bone_aspect.runtime.c1.bone_swap_remnant.operator.v1
actionId = c1.bone_swap_remnant.return_last_direct_pulse
effectRequestKey = battle.effect_request.direct_player_damage
sourceFactKind = PLAYER_RESOLVED_SINGLE_TARGET_DIRECT_DAMAGE
copyRatioBasisPoints = 2500
copyCapDamage = 7
telegraphTicks = 1500
recoverTicks = 18000
pendingPolicy = LATEST_WINS_ONE_SLOT
tuningDisposition = PLAYTEST_V1
entersFormalFlow = true
runtimeBoundToBattle = false
```

The mapping records only the latest Battle-accepted single-target direct-damage
numeric pulse. It does not copy Item code, cooldown, state, targets, chains,
proc behavior or internal implementation. The existing pure operator remains
protected and unchanged. The downstream Battle adapter must explicitly bind
the release row; this package does not execute the request.

The historical ChapterFlow `HELD_BY_BA-D3` row remains byte-identical. The new
sidecar is the explicit future-consumption overlay; Mainline/Battle must select
it by exact `balanceProfileId` and `encounterVariantId`, not by mutating or
guessing from the historical row.

## 8. Public API

Expose immutable defensive-copy APIs equivalent to:

```text
C1Stage3To5PlaytestEnemyEncounterCatalog.All
C1Stage3To5PlaytestEnemyEncounterCatalog.FindByStageId(stageId)
C1Stage3To5PlaytestEnemyEncounterCatalog.FindByEncounterVariantId(id)
C1Stage3To5PlaytestEnemyEncounterCatalog.Validation
C1Stage3To5PlaytestEnemyEncounterCatalog.CanonicalSignature
C1Stage3To5PlaytestEnemyEncounterCatalog.BoneSwapRelease
```

Each profile must carry exact stage, balance and encounter identities, roster,
attack wave, action/operator references, activation flags, and a canonical
signature. Missing, unsupported or conflicted facts must fail validation; they
must never become zero, an empty successful row, or a guessed fallback.

## 9. Deterministic Analysis

The verifier must read, not duplicate, these Item APIs:

```text
C1CampaignPool15BaseCombatFacts.Resolve()
C1CampaignPool15BuildEnvelopeSimulator.Resolve()
C1CampaignPool15BuildEnvelopeSimulator.TraceInventory(...)
```

It must assert the accepted Item canonical sources and obtain p50/p90 direct
pulse facts from their returned snapshots. Item values may appear in generated
analysis reports, but not in the Enemy runtime catalog.

Analysis-only assumptions:

```text
playerHp = 100
sameTickOrder = player_then_enemy
Enemy basic first/interval = 4.5 seconds
Bone Swap resolve = accepted pulse tick + 1.5 seconds
Bone Swap recovery = 18 seconds with one latest-wins pending slot
animationCompletionControlsGameplay = false
```

Required baseline trace summaries:

| Stage/input | TTK | Enemy basic hits | Bone Swap returns | Player HP |
|---|---:|---:|---:|---:|
| `1-3 / p50 direct 82 every 2s` | `42s` | `9` | `0` | `37` |
| `1-3 / p90 direct 110 every 2s` | `32s` | `7` | `0` | `51` |
| `1-4 / p50 direct 82 every 2s` | `42s` | `9` | `2` | `23` |
| `1-4 / p90 direct 110 every 2s` | `30s` | `6` | `2` | `44` |
| `1-5 / p50 direct 82 every 2s` | `44s` | `9` | `3` | `16` |
| `1-5 / p90 direct 110 every 2s` | `34s` | `7` | `2` | `37` |

Also produce one-variable-at-a-time `-20% / baseline / +20%` sensitivity for
Item direct pulse, total encounter HP and Enemy basic damage. Required
properties: deterministic ordering, monotonic TTK/damage directions, baseline
victory, no fabricated Item event, no Unknown/Unsupported-to-zero conversion,
and no animation-completion dependency.

## 10. Exact Development Write Whitelist

Development may add exactly these 13 files and modify no existing file:

1. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest.meta`
2. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterContract.cs`
3. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterContract.cs.meta`
4. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterValidation.cs`
5. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterValidation.cs.meta`
6. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterSimulation.cs`
7. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Stage3To5Playtest/C1Stage3To5PlaytestEnemyEncounterSimulation.cs.meta`
8. `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1Stage3To5PlaytestEnemyEncounterVerifier.cs`
9. `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1Stage3To5PlaytestEnemyEncounterVerifier.cs.meta`
10. `Docs/V0.4/Reports/C1Stage3To5PlaytestEnemyEncounterReport.md`
11. `Docs/V0.4/Reports/C1Stage3To5PlaytestEnemyEncounterSpec.csv`
12. `Docs/V0.4/Reports/C1Stage3To5PlaytestEnemyEncounterTrace.csv`
13. `Docs/V0.4/Reports/C1Stage3To5PlaytestEnemyEncounterSensitivity.csv`

All 13 targets were absent at Assignment freeze time.

## 11. Protected Read-Only Sources

Freeze and recheck these exact SHA-256 values. Do not rebaseline, restore,
format, stage or claim drift:

```text
bd85b3400b54f5456edd1e1c9182cc8e88cb100d8fed7f273bb04bd14f22e46d  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Lv1EarlyEncounterBalanceContract.cs
d8bf1777090df46de1e917d38639330f2dec15b1a27b72331e9bfa544b818b86  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Lv1EarlyEncounterBalanceValidation.cs
7e28709fe10ce5dbbe111d86469ff4a7008aef2ebada8175d45faddacc6dacc1  Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1Lv1EarlyEncounterBalanceVerifier.cs
8b102bc08f30d79016dada658a8a0698c456f872dcbb15a0d5eba99c549e0f14  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs
f60aea2213edff0d7397ca5849452ccedf6f284a64d20259bdbfd2b6aa22347a  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs
7f3b1244b2727ffb022c08c49296793b641994574a4e1b3d9d52606e9fa0a149  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimePrimitives.cs
ff6b2845f3d5651a5007f64bb471dce3af7aca45c814fbedaa9cfeb0d97fffc4  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantCopyProfile.cs
ee1688b8cf6a4717821dff18558212e94f63727167bff1b281d9830868a60d66  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeOperator.cs
08fb5ad80c6b7e5069316f2850d145b62f033dd87d2dd5f747b05276039c687a  Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/BoneSwapRemnant/BoneSwapRemnantRuntimeValidation.cs
22133b148e59e78f4f19e3fab0b9dd74da09317025e673495ba40a5c998b4af5  Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1StageWaveEncounterTruth.cs
a6f037fdf369da144dfea841a964bcc1e88a64b8aee5a9c5bcf4212d50a28174  Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1StageWaveEncounterValidation.cs
97994e47b6cccda7ba92d2bb53a99fce11cc8b36523cea7307be3013ded3bbf9  Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/C1CampaignPool15BaseCombatFacts.cs
e4fa694c44d47f66556f1280190fa9a16cadb075bc61c71555bbb7e83cfa7b90  Assets/_Game/Scripts/TalismanBag/Items/CampaignLoot/C1CampaignPool15BuildEnvelopeSimulator.cs
```

## 12. Forbidden Scope

- No modification to existing `1-1`/`1-2` balance files or reports.
- No modification to existing C1 runtime, Bone Swap operator, Item, Battle,
  Bridge, Mainline, ChapterFlow, StageConfig, WorldMap, Unified, Scene, Prefab,
  Reward, Drop, Save, UI, animation/VFX, ProjectSettings, BuildSettings,
  Packages, Queue, `AGENTS.md` or `Docs/LOCKED/*`.
- No final balance claim, DEV_SHOWCASE_LV40 value reuse, shell value invention,
  target heuristic, RNG, player HP truth or animation-controlled combat.
- No Unity, Builder, Git, worktree, commit, tag, push, reset, rollback, clean or
  stage operation.
- Do not start a downstream package.

## 13. Verification

1. Verify Assignment SHA and all 13 protected hashes before writing.
2. Focused offline compile of the three new runtime files, exact read-only
   Enemy/Item dependencies and verifier: zero errors and zero warnings.
3. Deterministic verifier covers exact 3 profiles, 8 actor rows, roster order,
   HP totals, identities, flags, action/operator mappings, Bone Swap values,
   canonical determinism and defensive copies.
4. Run baseline and sensitivity analysis twice; generated four reports must be
   byte-identical on repeat.
5. Negative fixtures must reject wrong context/ID/revision/roster/order/HP,
   duplicate actor, missing action/operator, unsupported-to-zero coercion,
   enabled Battle binding, stale Item canonical and mutable returned data.
6. Exact whitelist, UTF-8/LF/no BOM/no trailing whitespace, protected hashes,
   forbidden namespace and no package-owned helper process at terminal.

Unity is explicitly `NOT_REQUIRED` because no Unity API, import, Scene, Prefab
or serialization is involved. Fresh Play belongs to Mainline after mounting.

## 14. Completion And Return

Return to Enemy Guard only when:

```text
PACKAGE_COMPLETE / AUTOMATED_QA_PASS
exact additions = 13/13
existing modifications = 0
PlayerVisibleDelivery = NONE
MilestoneCompletionEvidence = NO
FreshPlay = NOT_APPLICABLE
Unity = NOT_STARTED
GitOperations = NONE
NEXT_PACKAGE = NOT_STARTED
```

The Enemy Guard performs one close review, then sends Mainline exactly one
`ACCEPTED_ENEMY_PLAYTEST_V1_RELEASE` inventory. A development task must not
dispatch or message Mainline itself.
