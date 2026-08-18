# V0.4-C1Lv1EarlyEncounterBalance01

Guard marker: `ENEMY_GUARD_ASSIGNMENT_C1LV1EARLYENCOUNTERBALANCE01`

## 1. Outcome

Create one immutable, read-only Enemy balance carrier for the user-approved
`CAMPAIGN_NORMAL_LV1` early loop:

`1-1 -> guaranteed I002@white -> rearrange -> 1-2`

The package freezes only the Enemy-owned HP and basic attack-wave parameters
needed to make the accepted Item lighting choice visibly consequential. It does
not bind the profile to Battle or formal Stage flow.

## 2. Product Context And Approved Numbers

Product context: `CAMPAIGN_NORMAL_LV1`

| Encounter | Actor balance | Total HP | Attack wave | Expected result |
|---|---|---:|---|---|
| `campaign.normal.lv1.encounter.c1.1-1` | Shattered Host x2, `300` HP each | `600` | one `8` damage application every `4.5` seconds | `16s / 3 hits / player 76 of analysis-fixture 100 HP` |
| `campaign.normal.lv1.encounter.c1.1-2` weak | Shattered Host x2, `600` HP each; Porcelain Hound `720` HP | `1920` | one `8` damage application every `4.5` seconds | I002 unlit: `48s / 10 hits / player 20 HP` |
| `campaign.normal.lv1.encounter.c1.1-2` target | same Enemy profile | `1920` | same | I002 lit: `36s / 7 hits / player 44 HP` |

The target layout is `25%` faster and takes three fewer accepted Enemy attack
applications. The current Item contract exposes only I002 unlit/lit. A middle
layout is `NOT_DISTINCT`; do not invent a third output tier.

These numbers are not derived from `DEV_SHOWCASE_LV40`. Existing Shattered Host
HP `650`, Porcelain Hound HP `180`, Porcelain Hound shell `100`, Lv40 Item
damage, player HP `9999`, and showcase action schedules are excluded.

## 3. Task Class And User Review Gate

| Field | Value |
|---|---|
| Task class | `CONTAINED_ONE_GUARD / NEW BALANCE DATA CONTRACT` |
| Primary Guard | Enemy Guard |
| User Review Gate | `YES / COMPLETE` |
| USER_ACCEPTED_SCOPE | `USER_APPROVED_ENEMY_NUMERIC_BASELINE_CONTINUE_P0` |
| Further user numeric decision | `NONE` |
| User handtest | `NOT_REQUIRED` for this isolated data/contract package; the later Battle integration owns the product handtest. |

The required policy order has been completed: `$game-producer-coach` first,
then `$talisman-balance-simulator` with deterministic discrete events and
separate `-20% / baseline / +20%` sensitivity.

## 4. Ownership

- Enemy owns the immutable encounter balance rows: actor HP, basic attack-wave
  damage, first resolve time, interval, action subset, and activation state.
- Item owns `I001@white` and `I002@white` damage/cooldown/lighting. The verifier
  reads `C1Lv1StarterItemFactProjection.v1`; Enemy must not copy `82/2` or
  `28/2` into its runtime carrier.
- Battle owns clock advancement, same-tick ordering, target/source actor
  selection, player damage application, player HP, dedupe, and application
  ledger. This package performs no Battle execution.
- Chapter Flow owns Stage launch and the binding from launch context to the
  balance profile. This package performs no binding.
- Presentation owns no balance or combat truth.

The approved `player HP=100` and `player_then_enemy` same-tick ordering are
analysis fixtures only. They must appear in reports and tests, not in the
public Enemy balance carrier.

## 5. Required Stable Identities

```text
SchemaId = C1Lv1EarlyEncounterBalance.v1
ProfileRevision = C1LV1_EARLY_ENCOUNTER_BALANCE_R1
ProductContext = CAMPAIGN_NORMAL_LV1
BalanceProfileId = campaign.normal.lv1.balance.identity.c1
EncounterVariant1_1 = campaign.normal.lv1.encounter.c1.1-1
EncounterVariant1_2 = campaign.normal.lv1.encounter.c1.1-2
AttackCadenceScope = ENCOUNTER_WAVE_SINGLE_APPLICATION
ActionSubset = BASIC_ATTACK_ONLY
ActivationStatus = APPROVED_NOT_BOUND
ItemProjectionSchema = C1Lv1StarterItemFactProjection.v1
ItemProfileRevision = C1LV1_STARTER_ITEM_BASELINE_R1
ItemCatalogCanonical = d0dd84f6e2f59344d3a49416d883eb5f4f84b408ab85a96a8dcf6751de97e357
```

Use existing exact Enemy identities:

```text
Shattered Host contentId = bone_aspect_enemy_c1_01_shattered_host
Shattered Host runtimeProfileId = bone_aspect.runtime.c1.shattered_host.v1
Porcelain Hound contentId = bone_aspect_enemy_c1_02_porcelain_hound
Porcelain Hound runtimeProfileId = bone_aspect.runtime.c1.porcelain_hound.v1
```

## 6. Public Contract Shape

The runtime carrier must expose immutable defensive-copy views for exactly two
encounter profiles. Tightly coupled public types may be contained in the two
allowed runtime source files.

Each encounter profile must expose at least:

```text
schemaId
productContext
profileRevision
balanceProfileId
encounterVariantId
stageId
actors[]
attackWave
actionSubset
activationStatus
isEnabled
entersFormalFlow
runtimeBoundToBattle
canonicalSignature
```

Each actor row must expose at least:

```text
actorBalanceId
contentId
runtimeProfileId
occurrenceOrdinal
maxHp
shellEnabled
skillEnabled
canonicalSignature
```

Each attack-wave row must expose at least:

```text
cadenceScope
effectRequestKey
firstResolveSeconds
intervalSeconds
damagePerApplication
applicationsPerWave
canonicalSignature
```

Required values:

- both encounters: `firstResolveSeconds=4.5`, `intervalSeconds=4.5`,
  `damagePerApplication=8`, `applicationsPerWave=1`;
- `effectRequestKey=battle.effect_request.direct_player_damage`;
- `isEnabled=false`, `entersFormalFlow=false`,
  `runtimeBoundToBattle=false`;
- all five actor rows: `shellEnabled=false`, `skillEnabled=false`;
- no bone-swap remnant, Boss, 1-3..1-10, reward, player, Item, Scene, or UI row.

The campaign carrier does not modify or overwrite existing dev-only runtime
profiles. The later Bridge must instantiate/apply the approved Campaign balance
row rather than reading the old showcase HP/shell/action timing as Campaign
truth.

## 7. Exact Allowed Writes

Development may add exactly these 12 files and modify no existing file:

1. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance.meta`
2. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Lv1EarlyEncounterBalanceContract.cs`
3. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Lv1EarlyEncounterBalanceContract.cs.meta`
4. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Lv1EarlyEncounterBalanceValidation.cs`
5. `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/C1Lv1EarlyEncounterBalanceValidation.cs.meta`
6. `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1Lv1EarlyEncounterBalanceVerifier.cs`
7. `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/C1Lv1EarlyEncounterBalanceVerifier.cs.meta`
8. `Docs/V0.4/Reports/C1Lv1EarlyEncounterBalanceReport.md`
9. `Docs/V0.4/Reports/C1Lv1EarlyEncounterBalanceSpec.csv`
10. `Docs/V0.4/Reports/C1Lv1EarlyEncounterBalanceTrace.csv`
11. `Docs/V0.4/Reports/C1Lv1EarlyEncounterBalanceSensitivity.csv`
12. `Docs/V0.4/Reports/C1Lv1EarlyEncounterBalanceLeakCheckReport.md`

All 12 targets were absent at Assignment freeze time.

## 8. Forbidden Writes And Behavior

- Do not modify Item, C1 existing runtime/action/reducer/validation, Battle,
  Bridge, Chapter Flow, Stage assets, Scene, Prefab, UI, animation/VFX, Reward,
  Drop, Save, Inventory, RunFlow, BuildSettings, ProjectSettings, Packages,
  Queue, `AGENTS.md`, or `Docs/LOCKED/*`.
- Do not bind the profile to `CampaignStage_1-1.asset` or
  `CampaignStage_1-2.asset`.
- Do not change Item damage/cooldown or copy those values into Enemy runtime
  fields/constants.
- Do not enable Porcelain Hound shell/charge or Shattered Host Skill for this
  P0 balance trace.
- Do not add middle-tier Build semantics, miss chance, crit, mitigation,
  healing, shield, targeting heuristics, stagger, status, RNG, or animation
  completion gating.
- Do not start 1-3..1-10, elite, Boss, BA-D3, BA-D4, formal integration, or a
  later package.
- Do not run Unity, Builder, Scene/Prefab serialization, Git, commit, tag,
  push, reset, clean, stage, or rollback.

## 9. Required Deterministic Analysis

The verifier must consume Item projections read-only and build analysis-only
events. It may not place Item damage/cooldown in the Enemy carrier.

Baseline assumptions:

```text
playerHp = 100 (analysis only)
sameTickOrder = player_then_enemy (analysis only)
lit Item firstResolve = its authoritative cooldownSeconds
lit Item interval = its authoritative cooldownSeconds
unlit Item produces zero events
```

Required baseline results:

| Scenario | TTK | Enemy applications | Player HP |
|---|---:|---:|---:|
| `1-1 / I001 lit` | `16` | `3` | `76` |
| `1-2 / I002 unlit` | `48` | `10` | `20` |
| `1-2 / I002 lit` | `36` | `7` | `44` |

Required consequence assertions:

```text
SpeedGain = (48 - 36) / 48 = 0.25
EnemyApplicationsAvoided = 10 - 7 = 3
Weak/middle/target unique outputs = 2
Middle status = NOT_DISTINCT
All baseline scenarios = VICTORY
Animation completion calls = 0
Battle applications = 0
Formal bindings = 0
```

Required sensitivity evidence, one variable changed at a time:

- I002 direct damage `-20% / +20%`: target `38s / 34s`, Enemy applications
  `8 / 7`, player HP `36 / 44`;
- 1-1 total Enemy HP `-20% / +20%`: `12s / 18s`, Enemy applications
  `2 / 3`, player HP `84 / 76`;
- 1-2 total Enemy HP `-20% / +20%`:
  - weak `38s / 58s`, player HP `36 / 4`;
  - target `28s / 42s`, player HP `52 / 28`;
- Enemy attack damage `-20% / +20%`:
  - weak player HP `36 / 4`;
  - target player HP `55.2 / 32.8`;
  - TTK unchanged.

Sensitivity rows are analysis evidence only. Fractional damage does not enter
the runtime Enemy profile.

## 10. Validation And Negative Fixtures

Validation must fail explicitly for at least:

- wrong schema, product context, profile revision, balance profile ID, or
  encounter variant ID;
- missing/duplicate encounter, actor, actorBalanceId, occurrence ordinal, or
  canonical signature;
- wrong actor count/identity/order for 1-1 or 1-2;
- nonpositive HP, cadence, interval, damage, or applications per wave;
- actor HP sum not equal to the expected encounter total;
- shell or skill enabled;
- action subset other than `BASIC_ATTACK_ONLY`;
- cadence scope other than `ENCOUNTER_WAVE_SINGLE_APPLICATION`;
- enabled/formal/Battle-bound profile;
- missing, wrong-revision, wrong-schema, or wrong-canonical Item projection;
- I002 unlit producing an event;
- a fabricated distinct middle result;
- culture/order-dependent canonical output;
- mutable returned collections or caller mutation affecting catalog state.

Unknown, unsupported, missing, or conflicted facts must fail closed. They must
never become zero or a successful empty profile.

## 11. Read-Only Protected Sources

The development task must freeze these exact hashes before writing and confirm
they remain unchanged at completion:

```text
Assets/_Game/Scripts/TalismanBag/Items/CampaignBaseline/C1Lv1StarterItemBaseline.cs
a4c454ffcc0a3b5ce1bf2181a39b79ca73c809fd81fd5d3aa3257c7908a140c7

Docs/V0.4/Reports/C1Lv1StarterItemBaseline01Report.md
66ff3f7885a1aaec38da88ddbc3d9564d79b3e20e3570d48c3561c0b70e7cd9e

Docs/V0.4/Reports/C1Lv1StarterItemBaseline01Spec.csv
a528fb48cd07ac6393f18c54e0dc9f999fceb7e64a25373c4b51842e01dc969a

Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1StageWaveEncounterTruth.cs
22133b148e59e78f4f19e3fab0b9dd74da09317025e673495ba40a5c998b4af5

Assets/_Game/Resources/TalismanBag/Campaign/Chapter1/Stages/CampaignStage_1-1.asset
b9e88d0860f686f6bd95aebf70686246f97897a2bd6f42b47a3470d90c589d81

Assets/_Game/Resources/TalismanBag/Campaign/Chapter1/Stages/CampaignStage_1-2.asset
ef084c7d5678efa48e51d7c7074498eee5cba3478d2d76e2d26eedd379ecf706

Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimePrimitives.cs
8b102bc08f30d79016dada658a8a0698c456f872dcbb15a0d5eba99c549e0f14

Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeCatalog.cs
f60aea2213edff0d7397ca5849452ccedf6f284a64d20259bdbfd2b6aa22347a

Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/C1EnemyRuntime/C1EnemyRuntimeValidation.cs
109cfa7a7f21490d3215f05e8984df87647005c5e465aa8a6deaee820192ae4c
```

If any protected source changes during the task, stop and report exact-path
drift. Do not rebaseline, restore, format, stage, or claim the other task's
file.

## 12. Verification

Minimum QA only:

1. compile the package runtime sources and the exact read-only dependencies
   needed by the verifier using the repository's established offline C# path;
2. run the deterministic same-source verifier;
3. assert the exact two profiles, five actor rows, baseline traces, sensitivity
   rows, negative fixtures, canonical determinism, defensive copies, and
   protected hashes;
4. generate the five reports twice and prove byte-identical output;
5. run package-scoped UTF-8/LF/no-BOM/no-trailing-whitespace and unexpected-file
   checks.

`UNITY_NOT_REQUIRED`. There is no Unity API, import, serialization, Scene,
Prefab, or visual dependency in this package. Do not run Unity batch merely to
obtain an extra receipt.

Do not use Git for verification. Report `GitOperations=NONE`.

## 13. Completion And Stop Gate

Close as `PACKAGE_COMPLETE / AUTOMATED_QA_PASS` when:

- exact additions `12/12`, existing modifications `0`, unexpected files `0`;
- exact profile/actor/attack values and activation flags pass;
- baseline and sensitivity traces match section 9;
- Item facts were consumed read-only and not copied into Enemy runtime truth;
- all protected hashes remain unchanged;
- no Unity or package-owned helper process remains;
- no later package was started.

At terminal completion, sync one compact result to Enemy Guard and RepoOps and
release the accepted profile as a read-only prerequisite to the Battle/Bridge
owner. Do not dispatch that integration from the development task.

