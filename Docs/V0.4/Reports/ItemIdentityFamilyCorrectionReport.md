# Item Identity Family Correction Report

Package: `V0.4-ItemIdentityFamilyCorrection01`
Generated: `2026-07-04`
Status: `PASS`

## Scope

- BuildSandbox devOnly correction only.
- V0.2/V0.3 formal base item ids are read-only references and are not edited.
- V0.4 preview item ids remain preview ids and are not promoted into formal RunFlow.
- No UI layout, scene layout, SaveData, Reward, Chapter, Boss, drop, or formal numeric flow writes.

## Fields

| Field | Meaning |
| --- | --- |
| `itemFamily` | Stable family key used by BuildSandbox identity reports. |
| `baseItemId` | Read-only V0.2/V0.3 base reference when one exists; empty for new mechanisms. |
| `tier` | One of `basic`, `advanced`, `core`, `support`, `test_only`. |
| `relationshipToBase` | One of `base`, `advanced_variant`, `new_mechanic`, `support_variant`, `test_only`. |

## Counters

- basic item rows: `13`
- V0.4 preview item rows: `10`
- old formal itemId replacement rows: `0`
- V0.4 enabled rows: `0`
- V0.4 devOnly=false rows: `0`

## Classification A - V0.2/V0.3 Basic Items

| Item | itemId | itemFamily | baseItemId | tier | relationshipToBase | Scope |
| --- | --- | --- | --- | --- | --- | --- |
| 火符 | `fire_talisman_basic` | `fire_talisman` | `fire_talisman_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 雷符 | `thunder_talisman_basic` | `thunder_talisman` | `thunder_talisman_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 护身符 | `shield_talisman_basic` | `guardian_ward` | `shield_talisman_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 丹药 | `qi_pill_basic` | `pill` | `qi_pill_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 聚灵石 | `spirit_stone_basic` | `spirit_stone` | `spirit_stone_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 剑丸 | `sword_pill_basic` | `sword_pill` | `sword_pill_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 连锁雷符 | `chain_thunder_talisman_basic` | `chain_thunder_talisman` | `chain_thunder_talisman_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 净化符 | `purify_talisman_basic` | `purify_talisman` | `purify_talisman_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 镇魂符 | `soul_suppress_talisman_basic` | `soul_suppress_talisman` | `soul_suppress_talisman_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 法印 | `seal_basic` | `seal` | `seal_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 水符 | `water_talisman_basic` | `water_talisman` | `water_talisman_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 驱邪铃 | `exorcism_bell_basic` | `exorcism_bell` | `exorcism_bell_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |
| 桃木牌 | `peach_wood_basic` | `peach_wood` | `peach_wood_basic` | `basic` | `base` | `v02_v03_formal_reference_read_only` |

## Classification B - V0.4 Advanced / New Mechanism Items

| Item | itemId | itemFamily | baseItemId | tier | relationshipToBase | Scope |
| --- | --- | --- | --- | --- | --- | --- |
| 炽火符 | `preview_fire_talisman` | `fire_talisman` | `fire_talisman_basic` | `advanced` | `advanced_variant` | `buildsandbox_devonly_preview` |
| 雷引剑符 | `preview_thunder_sword` | `thunder_talisman` | `thunder_talisman_basic` | `advanced` | `advanced_variant` | `buildsandbox_devonly_preview` |
| 护阵木牌 | `preview_x2_wood_talisman` | `guardian_ward` | `shield_talisman_basic` | `support` | `support_variant` | `buildsandbox_devonly_preview` |
| 守护木牌 | `preview_guard_wood` | `guardian_ward` | `shield_talisman_basic` | `support` | `support_variant` | `buildsandbox_devonly_preview` |
| 净化折符 | `preview_cleanse_corner` | `purify_talisman` | `purify_talisman_basic` | `advanced` | `advanced_variant` | `buildsandbox_devonly_preview` |
| 炉芯石 | `preview_stone_core` | `furnace_core` | `` | `core` | `new_mechanic` | `buildsandbox_devonly_preview` |
| 醒符香 | `preview_energy_incense` | `awakening_incense` | `` | `support` | `new_mechanic` | `buildsandbox_devonly_preview` |
| 镇邪铃 | `preview_old_bell` | `exorcism_bell` | `exorcism_bell_basic` | `advanced` | `advanced_variant` | `buildsandbox_devonly_preview` |
| 镇魂法印 | `preview_soul_seal` | `soul_suppress_seal` | `seal_basic` | `core` | `advanced_variant` | `buildsandbox_devonly_preview` |
| 桃木剑 | `preview_taomu_sword` | `peach_wood` | `peach_wood_basic` | `advanced` | `advanced_variant` | `buildsandbox_devonly_preview` |

## Validation Summary

| Check | Status | Detail |
| --- | --- | --- |
| Required fields present | `PASS` | `itemFamily`, `baseItemId`, `tier`, `relationshipToBase` are present on BuildSandbox placed snapshots and preview rows. |
| Allowed tier values | `PASS` | Only `basic`, `advanced`, `core`, `support`, `test_only` are used. |
| Allowed relationship values | `PASS` | Only `base`, `advanced_variant`, `new_mechanic`, `support_variant`, `test_only` are used. |
| Basic item preservation | `PASS` | All 13 V0.2/V0.3 rows stay `tier=basic` and `relationshipToBase=base`. |
| V0.4 identity correction | `PASS` | All 10 V0.4 rows stay preview/devOnly identities rather than old itemId replacements. |

