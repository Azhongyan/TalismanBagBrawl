# I031 Trigger Fact Water

Status: WATER_COMPLETE / INTERNAL_QA_PASS / DECOR_HANDOFF_READY

## Outcome

I031 uses the existing formal Nian mutation path and exposes the same minimum
trigger facts needed by shared Battle presentation: exact source instance,
authoritative next trigger time, periodic delivery, Nian gain result, exact
receiver and actual committed amount.

## Boundary

- Product context: `CAMPAIGN_NORMAL_LV1`.
- Reuse `C1FormalRealtimeBattleSession` and the existing immutable Battle
  state/cue contracts.
- Do not change I031 values, Item definitions, Reward, Enemy, Stage, Save,
  Scene, Prefab, Profile, animation or VFX.
- Do not add an I031-specific presentation branch or a second scheduler.

## Verification

The focused verifier must prove that I031 exposes its next due Battle time and
that the due event first changes real Nian, then emits an exact
`ITEM_TRIGGER / PERIODIC / NIAN_GAIN` cue and advances the next due time.

PlayerVisibleDelivery: NONE. Decoration remains responsible for countdown,
source emphasis, path and floating text.

## Completion

- `C1FormalRealtimeBattleNianStateSnapshot` now exposes the authoritative
  generation amount, interval and next due Battle time from the existing
  Session scheduler.
- The existing generation event still commits real Nian first and then emits
  the exact I031 source, Player receiver, `ITEM_TRIGGER`, `PERIODIC` and
  `NIAN_GAIN` facts.
- Focused Unity verification passed:
  `I031_TRIGGER_FACT_WATER_PASS checks=2`.
- No Item value, Scene, Prefab, Profile, Reward, Enemy, Stage, Save or
  Presentation asset changed. No player Fresh Play was run.
