# BATTLE_FEEDBACK_FACT_CONTRACT_V1

Status: INTERNAL_QA_COMPLETE / DECOR_HANDOFF_READY / PLAYER_PATH_NOT_READY / STOP
Classification: COMPLEX_GUARDED_ONCE
Primary owner: Battle / Bridge

## Player result dependency

Every battle feedback result can tell Presentation who caused it, why it
triggered, how it was delivered, what authoritative state changed and which
exact Actor received it. Presentation must not infer those facts from Item ID,
Enemy name, text, color or animation.

## Allowed writes

- `Assets/_Game/Scripts/TalismanBag/Contracts/Battle/C1FormalRealtimeBattleSessionContracts.cs`
- `Assets/_Game/Scripts/TalismanBag/BattleBridge/Formal/C1FormalRealtimeBattleSession.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleSessionVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/BattleBridge/C1FormalRealtimeBattleCanonicalEffectOperatorFocusedVerifier.cs`
- this Task Contract and the current Component Ledger

## Protected

- Battle damage, timing, target and status algorithms
- Enemy and Item definitions and numerical values
- Reward, Save, Mainline, Stage composition and WorldMap
- Scene, Prefab, Profile, Presentation, HUD, animation and VFX
- existing player-authored RectTransforms and Unity assets

## Implementation boundary

- Extend the existing immutable Battle Cue; do not create a second feedback
  state machine, Battle Session, scheduler, semantic database or Manager.
- Preserve existing source and receiver identities, requested/applied values
  and accepted application identity.
- Add only the missing explicit source stable order, Trigger, Delivery and
  Result facts on real result-bearing cues.
- Emit an explicit Guard-damage result when incoming damage is absorbed by
  the player's real Guard. Do not create visual output in this package.
- Status-triggered results retain their original Enemy or Item source lineage.
- Do not add Enemy-ID, Item-ID or Stage-ID branches.

## Focused acceptance

The existing Battle verifier must prove representative mappings for Enemy
basic attack, active Skill, passive reflect, periodic Pollution, Item trigger,
Shell/Guard and exact source/receiver identity. STOP after scoped compile and
focused verification. This package is a Decor prerequisite, not player-visible
completion.

## Completion receipt

- Existing `C1FormalRealtimeBattleCue` now carries explicit
  `sourceStableOrder`, `triggerKind`, `deliveryKind` and `resultKind` facts in
  addition to its existing exact source, receiver, requested/applied and
  accepted-application identities.
- Trigger facts cover Basic Action, Skill, Passive, Item Trigger, Status
  Trigger and Environment Trigger. Delivery facts cover Instant, Periodic,
  Delayed and Continuous. Result facts cover the current HP, Shell, Guard,
  Heal, status, Control, Cleanse and Nian state sinks.
- Damage absorbed by the player's real Guard emits `PLAYER_GUARD_CHANGED` as
  an authoritative `GUARD_DAMAGE` result instead of being visually inferred.
- Canonical direct-damage Item verification selects by effect semantics from
  the unique Canonical Item catalog; it does not branch on a concrete Item ID.
- The existing Canonical Effect verifier was cut over from the retired stage
  1-3 to 1-5 Enemy catalog signature to the current formal Enemy catalog ID.
  Its Cleanse fixtures were aligned with the current formal Enemy Skill
  schedule; no product Cleanse or Enemy behavior changed.
- Unity verification passed:
  `BATTLE_FEEDBACK_FACT_CONTRACT_PASS checks=6`,
  `CanonicalEffectRuntimeProof COMPLETE checks=43`,
  `ENEMY_SKILL_COMPONENT_RUNTIME_PASS checks=4`, and
  `PER_ACTOR_ENEMY_ACTION_WATER_PASS checks=4`.
- No Scene, Prefab, Profile, Presentation, Item/Enemy value, Reward, Save,
  Mainline or Stage asset was changed. Fresh Play was not run.
