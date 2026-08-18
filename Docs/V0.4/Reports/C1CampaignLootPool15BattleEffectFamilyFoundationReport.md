# C1 Campaign Loot Pool15 Battle Effect Family Foundation

- Package: `V0.4-C1CampaignLootPool15BattleEffectFamilyFoundation01`
- Status: `TECHNICAL_PREREQUISITE_COMPLETE`
- ProductContext: `CAMPAIGN_NORMAL_LV1`
- PlayerVisibleDelivery: `NONE`
- MilestoneCompletionEvidence: `NO`
- Pool signature: `572c3421a4874ef203634e64896f59950c5746aa023026fc2503a643b99a47a0`
- Source catalog signature: `85e67946c6394a9977238a74c53969190b2472214b67bc15cec4b3f3668b0bdf`
- Candidate profile: `C1_CAMPAIGN_LV1_POOL15_BATTLE_EFFECT_CANDIDATE_R1 / CANDIDATE / entersFormalFlow=false`
- Executor coverage: `15/15 descriptors / 5/5 families / 3 variants each`
- Matrix accepted applications: `15`
- Same-trigger accepted applications: `2`
- Rejected fixtures: `17`
- Idempotent duplicate rejections: `1`
- Source stagger: `120ms / stable source identity order`
- Fixed point: `basisPoint / HALF_AWAY_FROM_ZERO`
- Formal Session integration: `NOT_INTEGRATED`
- UnityOperations: `NONE`
- GitOperations: `NONE`
- WorktreeUsed: `NO`

## Family routing

| Family | Items | Variants | Executor status |
|---|---|---:|---|
| DIRECT_TEMPO | I002,I007,I026 | 3 | PASS |
| ENEMY_CONTROL | I003,I025,I028 | 3 | PASS |
| GUARD_SUSTAIN | I013,I015,I024 | 3 | PASS |
| POSITIONAL_ADJACENCY | I010,I014,I022 | 3 | PASS |
| TARGET_SPREAD | I004,I011,I030 | 3 | PASS |

## Rejection and idempotency matrix

| Fixture | Expected | Actual | Status |
|---|---|---|---|
| missing-event-id | `EVENT_ID_REQUIRED` | `EVENT_ID_REQUIRED` | PASS |
| missing-source | `SOURCE_REQUIRED` | `SOURCE_REQUIRED` | PASS |
| unknown-source | `SOURCE_UNKNOWN` | `SOURCE_UNKNOWN` | PASS |
| unknown-trigger | `TRIGGER_REJECTED` | `TRIGGER_REJECTED` | PASS |
| missing-condition | `CONDITION_REJECTED` | `CONDITION_REJECTED` | PASS |
| missing-target-identity | `TARGET_REJECTED` | `TARGET_REJECTED` | PASS |
| unknown-target-scope | `TARGET_REJECTED` | `TARGET_REJECTED` | PASS |
| duplicate-source | `SOURCE_DUPLICATE` | `SOURCE_DUPLICATE` | PASS |
| percentage-base-missing | `UNIT_REJECTED` | `UNIT_REJECTED` | PASS |
| fixed-point-overflow | `FIXED_POINT_OVERFLOW` | `FIXED_POINT_OVERFLOW` | PASS |
| numeric-profile-missing | `NUMERIC_PROFILE_PENDING` | `NUMERIC_PROFILE_PENDING` | PASS |
| numeric-amount-zero | `AMOUNT_REJECTED` | `AMOUNT_REJECTED` | PASS |
| numeric-unit-mismatch | `UNIT_REJECTED` | `UNIT_REJECTED` | PASS |
| descriptor-operation-unknown | `OPERATION_REJECTED` | `OPERATION_REJECTED` | PASS |
| descriptor-condition-unknown | `CONDITION_REJECTED` | `CONDITION_REJECTED` | PASS |
| descriptor-target-unknown | `TARGET_REJECTED` | `TARGET_REJECTED` | PASS |
| descriptor-unit-unknown | `UNIT_REJECTED` | `UNIT_REJECTED` | PASS |

`C1_POOL15_BATTLE_EFFECT_FAMILY_FOUNDATION_RELEASED`
