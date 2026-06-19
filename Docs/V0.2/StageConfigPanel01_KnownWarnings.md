# StageConfigPanel01 DataCatalog Known Warnings

## Validation Summary

- Error: 0
- Warning: 15
- Info: 0
- Current 1-10 / 2-10 main-path impact: none

The duplicate warning is emitted once for every asset in a duplicate group. Seven duplicate item-ID pairs therefore produce fourteen warning rows.

| # | warningId | assetPath | warningReason | Main-path impact | Classification | Allowed to remain |
|---|---|---|---|---|---|---|
| 1 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/fire_talisman_basic.asset` | Verified and retained Legacy assets share `fire_talisman_basic`. The Legacy asset is excluded from the active catalog. | No. V0.2 scene references the Verified asset. | Verified pair member | Yes, until legacy migration |
| 2 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/qi_pill_basic.asset` | Verified and retained Legacy assets share `qi_pill_basic`. | No. | Verified pair member | Yes |
| 3 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/seal_basic.asset` | Verified and retained Legacy assets share `seal_basic`. | No. | Verified pair member | Yes |
| 4 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/shield_talisman_basic.asset` | Verified and retained Legacy assets share `shield_talisman_basic`. | No. Fixed reward resolves an active ID. | Verified pair member | Yes |
| 5 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/spirit_stone_basic.asset` | Verified and retained Legacy assets share `spirit_stone_basic`. | No. | Verified pair member | Yes |
| 6 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/sword_pill_basic.asset` | Verified and retained Legacy assets share `sword_pill_basic`. | No. 1-10 reward resolves an active ID. | Verified pair member | Yes |
| 7 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/thunder_talisman_basic.asset` | Verified and retained Legacy assets share `thunder_talisman_basic`. | No. Fixed reward and UpgradeConfig resolve an active ID. | Verified pair member | Yes |
| 8 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/fire_talisman_basic.asset` | Retained old scene/shop asset shares the Verified ID. It is `Legacy + Deprecated`. | No. Excluded from active catalog, but still referenced by old content. | Legacy / Deprecated | Yes |
| 9 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/qi_pill_basic.asset` | Retained old scene/shop asset shares the Verified ID. It is `Legacy + Deprecated`. | No. | Legacy / Deprecated | Yes |
| 10 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/seal_basic.asset` | Retained old scene/shop asset shares the Verified ID. It is `Legacy + Deprecated`. | No. | Legacy / Deprecated | Yes |
| 11 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/shield_talisman_basic.asset` | Retained old scene/shop asset shares the Verified ID. It is `Legacy + Deprecated`. | No. | Legacy / Deprecated | Yes |
| 12 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/spirit_stone_basic.asset` | Retained old scene/shop asset shares the Verified ID. It is `Legacy + Deprecated`. | No. | Legacy / Deprecated | Yes |
| 13 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/sword_pill_basic.asset` | Retained old scene/shop asset shares the Verified ID. It is `Legacy + Deprecated`. | No. | Legacy / Deprecated | Yes |
| 14 | `DUPLICATE_itemId` | `Assets/_Game/ScriptableObjects/TalismanBag/thunder_talisman_basic.asset` | Retained old scene/shop asset shares the Verified ID. It is `Legacy + Deprecated`. | No. | Legacy / Deprecated | Yes |
| 15 | `SAME_NAME_DIFFERENT_enemyId` | `heart_demon_boss` and `heart_demon_boss_placeholder` | Both old Boss assets display `心魔邪修`, but use different IDs. The placeholder is deprecated. | No. Current 1-10/2-10 path uses the formation-breaker Boss chain. | Legacy / Deprecated placeholder | Yes |

## Retention Rule

These warnings may remain for the current package because:

1. DataCatalog has no Error results.
2. Every active V0.2 item ID resolves.
3. Current 1-10 and 2-10 scene/config references use active assets.
4. Old assets are still referenced by historical scenes, RunConfigs, or shop pools.
5. Deleting or renaming them belongs to a separately approved legacy migration package.

If any warning becomes attached to two active, non-debug, non-deprecated assets, it must be upgraded to a blocking defect.

