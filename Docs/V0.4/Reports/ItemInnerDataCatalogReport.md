# ItemInnerDataCatalog01 Report

- Package: `TASK_START_ITEMINNERDATACATALOG01`
- Guard receipt target: `GUARD_PASS_ITEMINNERDATACATALOG01`
- Verification: PASS
- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`

## Summary
- Catalog item count: 31 / 31
- 30 法门道具 + 1 聚念石: verified by static catalog checks.
- 阵眼石: not present in catalog; eyeCell remains non-item / non-placeable / non-ordinary-cell by Guard rule.
- 聚念石: only entry marked `isLightingSource=true`, with `faMenTag=zhonggong` and `qiLeiTag=zhonggongQi`.
- Item Sandbox detail UI: bound through `ItemDetailViewModel` via `ItemInnerDataCatalogProvider`.
- DevStubProvider boundary: retained as old greybox sample source; active sandbox scene uses CatalogProvider for 31 readonly catalog entries.

## Catalog Groups
- lihuo / 离火法: 6
- taibai / 太白法: 6
- xuanshui / 玄水法: 6
- zhenlei / 震雷法: 6
- zhonggong / 中宫: 1
- zhongyue / 中岳法: 6

## Required Fields
- Every item has itemId / displayName / faMenTag / qiLeiTag / shapeId / coreCellLocal.
- Every item has shapeCells, default rarity, allowed rarities, power text, stats, trigger/effect/affix/placement/flavor/icon placeholder fields.
- `coreCellLocal` is included in each item's `shapeCells`.

## Notes
- Lighting source check passed: 聚念石 is the only catalog lighting source marker.
- Catalog data check passed: 31 items, stable ids, required fields, and coreCellLocal inside shapeCells.
- BuildSettings check passed: Item Sandbox scene is not registered.
- Scene provider check passed: CatalogProvider exposes 31 readonly entries and no formal system flags.
- Scene list check passed: sandbox list has 31 item buttons.
- Hierarchy naming check passed: no Chinese GameObject names were added.
- Source scope check completed for Item catalog and Item Sandbox adapter files.

## Errors
- None

