# C1 Pool15 Formal Battle Effect Family Integration Report

- Package: `V0.4-C1CampaignLootPool15FormalBattleEffectFamilyIntegration01`
- Classification: `COMPLEX_GUARDED_ONCE`
- Product context: `CAMPAIGN_NORMAL_LV1`
- PlayerVisibleDelivery: `NONE`
- MilestoneCompletionEvidence: `NO`
- FormalMainlineBinding: `NOT_ASSEMBLED`
- Candidate profile: `C1_CAMPAIGN_LV1_POOL15_BATTLE_EFFECT_CANDIDATE_R1 / CANDIDATE`
- Status: `TECHNICAL_PREREQUISITE_COMPLETE / AUTOMATED_QA_PASS`
- Assignment SHA before: `b90bb3fdada77314cd25dcda66bfea849f9b363ab79df8bbc233a2c06de9c65a`
- Assignment SHA after: `b90bb3fdada77314cd25dcda66bfea849f9b363ab79df8bbc233a2c06de9c65a`
- Focused compile: `PASS_REQUIRED_TO_EXECUTE_VERIFIER`
- Live-session non-immediate proof: `PASS`
- GitOperations: `NONE`
- WorktreeUsed: `NO`
- UnityOperations: `NONE`

## Stage 1-2 active Item admission matrix

| Entitlement arrangement | Active | Start | Terminal | Duration | Enemy applications | Player HP | Item applications | Oracle terminal source | Status |
|---|---:|---|---|---:|---:|---:|---:|---|---|
| stage-1-2-zero-active | 0 | non-terminal 0ms | defeat | 58500 | 13 | 0 | 0 | realtime Session | PASS |
| stage-1-2-i001-only | 1 | non-terminal 0ms | victory | 48000 | 10 | 20 | 24 | optional oracle evidence | PASS |
| stage-1-2-i002-only | 1 | non-terminal 0ms | defeat | 58500 | 13 | 0 | 29 | realtime Session | PASS |
| stage-1-2-both-active | 2 | non-terminal 0ms | victory | 36000 | 7 | 44 | 35 | optional oracle evidence | PASS |
| stage-1-2-placed-unlit-excluded | 0 | non-terminal 0ms | defeat | 58500 | 13 | 0 | 0 | realtime Session | PASS |

## Rejection matrix

| Case | Result | Error code |
|---|---|---|
| duplicate-instance | REJECTED | `ITEM_INPUT_REJECTED` |
| duplicate-base-item | REJECTED | `ITEM_INPUT_REJECTED` |
| unknown-item | REJECTED | `ITEM_INPUT_REJECTED` |
| missing-entitlement | REJECTED | `ITEM_INPUT_REJECTED` |
| extra-entitlement | REJECTED | `ITEM_INPUT_REJECTED` |
| stale-instance-canonical | REJECTED | `ITEM_INPUT_REJECTED` |
| stale-catalog-canonical | REJECTED | `ITEM_INPUT_REJECTED` |
| inconsistent-placement-lighting | REJECTED | `ITEM_INPUT_REJECTED` |
| stale-item-session-envelope | REJECTED | `ITEM_INPUT_REJECTED` |
| wrong-product-context | REJECTED | `CONTEXT_REJECTED` |
| stale-reset-envelope | REJECTED | `RESET_GENERATION_MISMATCH` |

## Preserved oracle parity traces

| Scenario | Live terminal | Oracle canonical | Status |
|---|---|---|---|
| 1-1 | 16000ms / enemy=3 / playerHp=76 | `22633fe15ea2a3b26cbed5f3829d437045622d7c8ec59d0ddd5e85f88577b2be` | PASS |
| 1-2-weak | 48000ms / enemy=10 / playerHp=20 | `a5ac770d230dc981fe0769f9b5d3ec9c529072fdef88b1d2d4d02230e9b18a26` | PASS |
| 1-2-tray | 48000ms / enemy=10 / playerHp=20 | `b6ad157fb68ce3e4264050f13a65811227718a082c7c72ba5e645906c7d5ca51` | PASS |
| 1-2-target | 36000ms / enemy=7 / playerHp=44 | `d7aa4c4e4c29805490a68f2dce377d009cc63269680228f4372c07fc267de503` | PASS |

## Pool15 formal integration coverage

| Item | Family | Variant | Trigger | Condition | Unit | Target before/after | Resolve | Maturity | Status |
|---|---|---|---|---|---|---:|---:|---|---|
| I002 | DIRECT_TEMPO | c1.campaign.direct.tempo.i002.v1 | on_consecutive_trigger | within_2_turns | point | 0 -> 1 | 0 | CANDIDATE | PASS |
| I003 | ENEMY_CONTROL | c1.campaign.enemy.control.i003.v1 | on_hit | target_has_shield | basisPoint | 0 -> 39 | 0 | CANDIDATE | PASS |
| I004 | TARGET_SPREAD | c1.campaign.target.spread.i004.v1 | on_direct_lit | next_zhenlei_trigger | count | 0 -> 1 | 0 | CANDIDATE | PASS |
| I007 | DIRECT_TEMPO | c1.campaign.direct.tempo.i007.v1 | on_first_hit | is_lit | stack | 0 -> 1 | 0 | CANDIDATE | PASS |
| I010 | POSITIONAL_ADJACENCY | c1.campaign.positional.adjacency.i010.v1 | on_lit | adjacent_lihuo | point | 0 -> 1 | 0 | CANDIDATE | PASS |
| I011 | TARGET_SPREAD | c1.campaign.target.spread.i011.v1 | on_mirror_trigger | target_has_burn | stack | 0 -> 1 | 0 | CANDIDATE | PASS |
| I013 | GUARD_SUSTAIN | c1.campaign.guard.sustain.i013.v1 | on_damaged | guard_below_threshold | flat | 0 -> 4 | 0 | CANDIDATE | PASS |
| I014 | POSITIONAL_ADJACENCY | c1.campaign.positional.adjacency.i014.v1 | on_layout_evaluate | horizontal_adjacent | basisPoint | 0 -> 4 | 0 | CANDIDATE | PASS |
| I015 | GUARD_SUSTAIN | c1.campaign.guard.sustain.i015.v1 | on_direct_lit | is_direct_lit | flat | 0 -> 8 | 0 | CANDIDATE | PASS |
| I022 | POSITIONAL_ADJACENCY | c1.campaign.positional.adjacency.i022.v1 | on_effect_end | adjacent_lit_item | turn | 0 -> 1 | 0 | CANDIDATE | PASS |
| I024 | GUARD_SUSTAIN | c1.campaign.guard.sustain.i024.v1 | on_cleanse | water_charge_3 | flat | 0 -> 12 | 0 | CANDIDATE | PASS |
| I025 | ENEMY_CONTROL | c1.campaign.enemy.control.i025.v1 | on_hit | target_casting | turn | 0 -> 1 | 0 | CANDIDATE | PASS |
| I026 | DIRECT_TEMPO | c1.campaign.direct.tempo.i026.v1 | on_slash | target_has_flaw | basisPoint | 0 -> 48 | 0 | CANDIDATE | PASS |
| I028 | ENEMY_CONTROL | c1.campaign.enemy.control.i028.v1 | on_command_trigger | next_taibai_hit | count | 0 -> 1 | 0 | CANDIDATE | PASS |
| I030 | TARGET_SPREAD | c1.campaign.target.spread.i030.v1 | on_kill_flaw | next_slash | count | 0 -> 1 | 0 | CANDIDATE | PASS |

## Active fact filtering

| Layout | Expected active | Actual active | Status |
|---|---:|---:|---|
| filter-weak | 0 | 0 | PASS |
| filter-middle | 1 | 1 | PASS |
| filter-target | 2 | 2 | PASS |

## Atomic rejection matrix

| Case | Result | Diagnostic |
|---|---|---|
| invalid-trigger | PASS | `POOL15_EVENT_TRIGGER_REJECTED` |
| missing-condition | PASS | `POOL15_EVENT_CONDITION_REJECTED` |
| duplicate-event | PASS | `POOL15_EVENT_DUPLICATE` |
| overflow-atomic | PASS | `POOL15_ATOMICITY_REJECTED` |
| missing-target | PASS | `POOL15_ATOMICITY_REJECTED` |
| missing-percentage-base | PASS | `POOL15_ATOMICITY_REJECTED` |
| unit-conflict-or-stale-profile | PASS | `POOL15_SNAPSHOT_REJECTED` |
| stale-target-descriptor | PASS | `POOL15_SNAPSHOT_REJECTED` |
| missing-entitlement-row | PASS | `POOL15_SNAPSHOT_REJECTED` |
| extra-entitlement-row | PASS | `POOL15_SNAPSHOT_REJECTED` |
| duplicate-base-item | PASS | `POOL15_SNAPSHOT_REJECTED` |
| inconsistent-tray-lighting | PASS | `POOL15_SNAPSHOT_REJECTED` |

## Verification summary

- Checks: `130`
- Failures: `0`
- Cue sequence is monotonic and dedupable: `PASS`
- Save writes: `0`
- Reward grants: `0`
- Scene/Prefab writes: `0`

`C1_POOL15_FORMAL_BATTLE_EFFECT_FAMILY_INTEGRATION_RELEASED`
