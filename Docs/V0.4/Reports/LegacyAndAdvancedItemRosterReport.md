# Legacy And Advanced Item Roster Report

Package: `V0.4-LegacyAndAdvancedItemRoster01`  
Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`  
Generated: `2026-07-04`  
Status: `PASS`

## Summary

- Added a BuildSandbox-only roster catalog for `23` testable items.
- Reused the `ItemIdentityFamilyCorrection01` identity fields: `itemFamily`, `baseItemId`, `tier`, and `relationshipToBase`.
- Kept all V0.2/V0.3 formal item identities read-only; the V04 sandbox roster wrapper is `devOnly=true` and `isEnabled=false`.
- Kept the existing V04 serialized category button count at `6`; the buttons now map to `basic`, `advanced`, `core`, `support`, `artifact`, and `test/devOnly`.
- Added runtime-only item-card expansion so the 23-item roster can be tested without serializing new V04 UI cards or reordering RectTransforms.

## Roster Counts

| Scope | Count | Result |
| --- | ---: | --- |
| V0.2/V0.3 basic items | 13 | PASS |
| V0.4 advanced or new-mechanic items | 10 | PASS |
| Total testable roster rows | 23 | PASS |
| Basic item shape rows using `Single1` | 13 | PASS |
| Stable category ids covered | 6 | PASS |
| Tray occupied cells in all-item order | 38 / 40 | PASS |

## Basic Items

All basic items use `Single1` / `单格` in the V04 sandbox and keep their formal `itemId` unchanged.

| Item | itemId | family | Stat source |
| --- | --- | --- | --- |
| 火符 | `fire_talisman_basic` | `fire_talisman` | V02 damage / cost / cooldown |
| 雷符 | `thunder_talisman_basic` | `thunder_talisman` | V02 damage + shield-break / cost / cooldown |
| 护身符 | `shield_talisman_basic` | `guardian_ward` | V02 shield / cost / cooldown |
| 丹药 | `qi_pill_basic` | `pill` | V02 heal / cost / cooldown |
| 聚灵石 | `spirit_stone_basic` | `spirit_stone` | V02 energy generation / interval |
| 剑丸 | `sword_pill_basic` | `sword_pill` | V02 burst damage / cost / cooldown |
| 连锁雷符 | `chain_thunder_talisman_basic` | `chain_thunder_talisman` | V02 chain damage / cost / cooldown |
| 净化符 | `purify_talisman_basic` | `purify_talisman` | V02 cleanse / cost / cooldown |
| 镇魂符 | `soul_suppress_talisman_basic` | `soul_suppress_talisman` | V02 anti-ghost control / cost / cooldown |
| 法印 | `seal_basic` | `seal` | V02 passive enhance, normalized to sandbox stat interval |
| 水符 | `water_talisman_basic` | `water_talisman` | V0.2/V0.3 support heal / cost / cooldown |
| 驱邪铃 | `exorcism_bell_basic` | `exorcism_bell` | V0.2/V0.3 anti-ghost attack / cost / cooldown |
| 桃木牌 | `peach_wood_basic` | `peach_wood` | V0.2/V0.3 passive exorcism support |

## V0.4 Items

The V0.4 rows remain in the roster with their corrected family relationships:

`preview_fire_talisman`, `preview_thunder_sword`, `preview_x2_wood_talisman`, `preview_guard_wood`, `preview_cleanse_corner`, `preview_stone_core`, `preview_energy_incense`, `preview_old_bell`, `preview_soul_seal`, `preview_taomu_sword`.

## Implementation Notes

- `BuildGridInteractionPreviewController.CreatePreviewItems()` now reads from `BuildSandboxLegacyAndAdvancedItemRosterCatalog`.
- `BuildSandboxItemStatCatalog.Resolve()` has exact entries for all 13 legacy basics plus the retained V0.4 items.
- `BattleSandboxBuildCombatPreviewBuilder` now resolves basic-item tags and preview affixes when a legacy item is dragged onto the sandbox board.
- The V04 item tray starts in an all-items internal state, while visible buttons map to the six requested stable roster categories.

Detailed rows are in `Docs/V0.4/Reports/LegacyAndAdvancedItemRoster.csv`.
