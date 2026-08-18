# C1 Formal Realtime Battle Session Report

- Package: `V0.4-C1FormalActiveItemAdmissionAndRealDefeat01`
- Classification: `CONTAINED_ONE_GUARD`
- Product context: `CAMPAIGN_NORMAL_LV1`
- PlayerVisibleDelivery: `NONE`
- MilestoneCompletionEvidence: `NO`
- Status: `TECHNICAL_PREREQUISITE_COMPLETE / AUTOMATED_QA_PASS`
- Assignment SHA before: `a1f49466ddeb2101ccc38e83f6dbbe96e45ee91b0c3ded597771f27b77dc4fc8`
- Assignment SHA after: `a1f49466ddeb2101ccc38e83f6dbbe96e45ee91b0c3ded597771f27b77dc4fc8`
- Focused compile: `PASS_REQUIRED_TO_EXECUTE_VERIFIER`
- Live-session non-immediate proof: `PASS`
- GitOperations: `NONE`
- WorktreeUsed: `NO`

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

## Verification summary

- Checks: `87`
- Failures: `0`
- Cue sequence is monotonic and dedupable: `PASS`
- Save writes: `0`
- Reward grants: `0`
- Scene/Prefab writes: `0`

`BATTLE_0_1_2_ACTIVE_ITEM_ADMISSION_RELEASED`
