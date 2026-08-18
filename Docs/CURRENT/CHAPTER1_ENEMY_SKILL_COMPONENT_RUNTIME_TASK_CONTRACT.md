# CHAPTER1_ENEMY_SKILL_COMPONENT_RUNTIME

Status: INTERNAL_QA_COMPLETE / PLAYER_RETEST_PENDING / STOP
Classification: COMPLEX_GUARDED_ONCE
Product context: CAMPAIGN_NORMAL_LV1

## Player result

Each Enemy is one data container with base attributes and a list of reusable
Skill definitions. Active, passive, status and reflected-damage behavior use
the same Formal Battle operators instead of Enemy-ID branches.

## Scope

- Add reusable Enemy Skill, Effect and Status definitions to the existing
  formal Enemy definition catalog.
- Keep the current PLAYTEST_V2 HP, Shell, basic damage and basic cadence.
- Migrate Shattered Host pollution, Porcelain Hound spawn Shell and charge,
  and Bone-swap Remnant reflect buff/passive to the shared operators.
- Apply and expose authoritative status duration, ticks, stacks and exact
  source/target cues through the existing live Battle Session.
- Extend one existing focused Battle verifier for the changed boundary.

## Allowed writes

- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CampaignBalance/`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleCanonicalEncounterResolver.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleEncounterResolver.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/C1FormalRealtimeBattleSessionContracts.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleSessionVerifier.cs`
- this contract and the current Component Ledger

## Protected

- Player HP and all Item, Nian, Reward, Save and Mainline rules
- Stage composition, WorldMap and progression
- Scene, Prefab, Presentation, HUD, animation and VFX
- Enemy content IDs and current PLAYTEST_V2 base attributes/basic attacks
- user-owned Unity lifecycle

## Stop condition

STOP after compile and focused proof show: pollution stacks, ticks and expires;
Hound gains Shell from OnSpawn and its active charge commits direct damage;
Remnant's active skill applies/refreshes its reflect buff and incoming player
damage causes immediate passive reflect only while that buff is present. No
Enemy content-ID branch may own those effect rules.

This is INTERNAL_QA, not player-visible completion.

## Completion receipt — 2026-08-18

- One formal Enemy definition now owns base attributes plus a list of reusable
  Skill definitions. Runtime branching is by activation, trigger, target,
  effect and status behavior; it does not branch on Enemy or Stage ID.
- Shattered Host composes basic attack, on-hit Pollution and a timed active
  strike. Pollution stacks to 3, refreshes its 6-second lifetime, ticks every
  2 seconds and deals 1 damage per stack per tick; the active strike deals 5
  direct damage and applies one Pollution stack.
- Porcelain Hound composes an OnSpawn 60 Shell passive and a timed Charge that
  deals 11 direct damage. Its base attack cadence and PLAYTEST_V2 attributes
  remain unchanged.
- Bone-swap Remnant composes a timed Reflect Window and an OnDamaged passive.
  The window lasts 8 seconds, stacks to 2 and refreshes; each stack reflects
  10 percent of committed incoming damage, with a total cap of 6 per hit and
  a minimum of 1 for a positive hit. Reflect commits immediately without
  emitting a basic-attack action.
- The previous unreachable BoneSwap-specific delayed-return scheduler and
  binding were removed from the current Formal Session. Legacy constants and
  standalone legacy verifier assets remain outside this runtime path.
- Unity proof: `ENEMY_SKILL_COMPONENT_RUNTIME_PASS checks=4`.
- Regression proof: `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`.
- Unity exited normally and the project lock is clear. Fresh Play was not run;
  player-visible skill/status/VFX readability remains a downstream gate.
