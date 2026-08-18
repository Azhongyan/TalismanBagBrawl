# CHAPTER1_CANONICAL_ENEMY_WATER

Status: PLAYTEST_V4_EARLY_HOST_HP_WRITTEN / COMPILE_PENDING / PLAYER_RETEST_PENDING
Classification: COMPLEX_GUARDED_ONCE / CURRENT_WINDOW_SELF_EXECUTION
Product context: CAMPAIGN_NORMAL_LV1

## Player result

The same Chapter 1 enemy keeps the same HP, Shell, basic damage, first attack,
attack interval and released mechanic in every stage. A stage selects only the
enemy composition and wave order.

## Scope

- Establish one formal ordinary-enemy definition catalog for Shattered Host,
  Porcelain Hound and Bone-swap Remnant using the approved PLAYTEST_V1 values.
- Reuse `V04Chapter1StageWaveEncounterTruth` as the only Stage -> Wave -> Actor
  composition source.
- Route formal stages 1-1 through 1-5 through that definition + composition
  chain.
- Give every live actor its own damage and cadence; never divide one stage-wide
  damage budget between actors.
- Preserve the existing pollution, shell and Bone-swap runtime behavior and
  exact source/target cues.

## Allowed writes

- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/`
- `Assets/_Game/Scripts/TalismanBag/V04/ChapterFlow/Chapter1/V04Chapter1StageWaveEncounterTruth.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleEncounterResolver.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSessionAdapter.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- one existing focused Battle verifier only where required by the changed boundary
- this contract and the current Component Ledger

## Protected

- Reward, Item, Nian, Save, WorldMap and Mainline flow
- Scene, Prefab, Presentation, HUD and all decoration work
- user-owned Unity lifecycle
- stage 1-6 through 1-10 routing and Boss runtime
- product tuning beyond the approved PLAYTEST_V1 enemy candidates
- legacy cleanup not required to remove a current formal consumer

## Stop condition

STOP after compile and focused proof show that 1-1 through 1-5 resolve their
actors from the shared enemy definitions, each actor owns its own attack cadence
and damage, Hound always owns Shell, and Remnant keeps the existing return
operator. This is INTERNAL_QA, not player completion.

## Completion receipt — 2026-08-18

- `C1FormalEnemyDefinitionCatalog` is the single formal ordinary-enemy data
  source for Shattered Host, Porcelain Hound and Bone-swap Remnant.
- `V04Chapter1StageWaveEncounterTruth` remains the Stage -> Wave -> Actor
  composition source. The released formal route consumes it for 1-1 through
  1-5; Stage data no longer owns duplicate combat numbers.
- `C1FormalRealtimeBattleSession` schedules one action per live Enemy instance
  using that Enemy's own damage, first attack time and repeat interval. Defeat
  cancels only the defeated source's pending actions.
- Existing pollution, Shell and BoneSwap behavior remain in the same live
  Session. BoneSwap uses the released operator profile rather than a second
  Enemy runtime.
- Unity compilation and the existing focused verifier passed:
  `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`. The aggregate proof covers all
  five released stages plus exact source damage and death cancellation.
- Former per-stage balance catalogs remain compatibility code only and are no
  longer the current formal feeder. Their physical retirement is a separate
  consumer-graph cleanup, not part of this package.
- No Reward, Item, Nian, Save, WorldMap, Scene, Prefab, Presentation or HUD
  files were changed. Fresh Play was not run, so this is not player completion.

## PLAYTEST_V2 correction — 2026-08-18

Player evidence overrides the prior internal result: the player still has 100
HP and cannot survive the new per-actor attack pressure. The architecture is
kept; the PLAYTEST_V1 numbers are not.

- Keep one shared Enemy definition and one independent action per live actor.
- Retune only Enemy HP, Shell, attack damage/cadence and released BoneSwap
  pressure. Do not change Player HP, Item, Nian, Reward or Guard algorithms.
- Use a generic 400 ms startup phase between stable actor instances so
  duplicate enemies do not commit on the same frame. Repeat cadence remains
  owned by each Enemy definition.
- PLAYTEST_V2 candidates:
  - Shattered Host: 40 HP, 0 Shell, 2 damage, first attack 2.4 s, interval 4.0 s.
  - Porcelain Hound: 80 HP, 60 Shell, 5 damage, first attack 2.8 s, interval 4.6 s.
  - Bone-swap Remnant: 140 HP, 0 Shell, 4 damage, first attack 3.0 s,
    interval 4.8 s; return 20%, cap 8, telegraph 1.5 s, recovery 12 s.
- Resulting 1-1 through 1-5 effective durability is approximately
  80 / 120 / 180 / 280 / 320 before BoneSwap. These values arise from the
  shared templates and stage composition, never per-stage stat overrides.
- Pollution remains the existing bounded status application in this package;
  no second damage-over-time system is introduced during balance correction.

### PLAYTEST_V2 internal result

- The unique formal Enemy catalog now identifies itself as
  `campaign.normal.lv1.enemy.definition.c1.playtest.v2`.
- Shared-template effective durability is 80 / 120 / 180 / 280 / 320 for
  stages 1-1 through 1-5.
- Initial attacks receive a generic 400 ms stable-actor phase. Repeat attacks
  retain each Enemy definition's own interval; no Stage or Enemy-ID branch was
  added.
- Analysis-only worst-case survival with 100 HP, no defense, no healing and no
  Enemy deaths is 98.8 / 66.8 / 63.0 / 53.4 / 42.2 seconds. Real battles can
  only reduce this basic-attack pressure as Enemy actors die; BoneSwap remains
  separate encounter pressure.
- Unity compilation and the existing focused proof passed:
  `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`. Unity exited naturally and the
  project lock is clear.
- This is internal balance evidence, not proof of target remaining HP or fun.
  The next gate is one normal player-route retest; no further product code is
  authorized inside this package before that evidence.

## PLAYTEST_V3 HP-only correction — 2026-08-18

New player evidence overrides the earlier timing estimate: the released normal
route now clears 1-1 in approximately 5 seconds even though the encounter owns
two Shattered Hosts and 80 total HP. The Enemy architecture and Skill scheme
remain accepted; only ordinary-enemy HP is adjusted in this correction.

- Shattered Host: 40 -> 80 HP.
- Porcelain Hound: 80 -> 160 HP; Shell remains 60.
- Bone-swap Remnant: 140 -> 280 HP.
- Attack damage, first attack, repeat interval, Pollution, Charge, Reflect,
  Player HP, Item output, Nian, Guard and all stage compositions remain
  unchanged.
- Shared-template effective durability becomes 160 / 240 / 300 / 500 / 580
  for stages 1-1 through 1-5.
- The 1-1 estimate is approximately 10 seconds when scaled from the player's
  current 5-second evidence. This estimate is not a completion claim; one
  normal-route player retest must confirm actual duration and survival pressure.
- Catalog identity advances to
  `campaign.normal.lv1.enemy.definition.c1.playtest.v3`; values remain
  playtest tuning rather than final balance.

### PLAYTEST_V3 internal result

- Unity compilation and the existing formal per-actor Enemy proof passed:
  `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`.
- The batch-owned Unity lifecycle exited normally and the project lock is
  clear.
- No Player, Item, Nian, Enemy attack/Skill, Stage composition, Scene, Prefab
  or Presentation value was changed.
- This is `INTERNAL_QA`, not a claim that encounter pacing is correct. STOP
  until the player reports the normal-route 1-1 duration and remaining HP.

## PLAYTEST_V4 early Host HP correction — 2026-08-18

The V3 uniform HP doubling makes the three-Host 1-2 encounter too likely to
stop a legal early Build after only one random reward. Keep the shared-template
architecture and correct the reusable Host rather than adding a stage override.

- Shattered Host: 80 -> 60 HP.
- Porcelain Hound remains 160 HP + 60 Shell.
- Bone-swap Remnant remains 280 HP.
- Shared-template effective durability becomes 120 / 180 / 280 / 500 / 560
  for stages 1-1 through 1-5.
- Scaling from the player's earlier five-second 1-1 result at 80 total HP,
  1-1 is estimated near 7.5 seconds at 120 total HP. This is a playtest estimate,
  not runtime proof.
- Enemy attacks, Skills, cadence, Stage composition, Player, Item, Nian and
  Guard remain unchanged. Catalog identity advances to
  `campaign.normal.lv1.enemy.definition.c1.playtest.v4`.
- Unity compilation is intentionally deferred until the concurrent Decor Item
  package has stopped writing, so an Enemy balance check does not compile a
  second in-progress package again.
