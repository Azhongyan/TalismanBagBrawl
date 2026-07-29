# ItemInnerDataCatalog01 Report

- Package: `TASK_START_ITEMINNERDATACATALOG01`
- Guard receipt target: `GUARD_PASS_ITEMINNERDATACATALOG01`
- Verification: PASS
- I009/I029 shape fix marker: `I009_I029_ITEM_SHAPE_FIX01_PASS`
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
- I009 shape invariant passed: shape_single_1, only (0,0) occupied, coreCellLocal (0,0).
- I009 placement invariant passed: rotations 0/90/180/270 each occupy only (0,0).
- I029 shape invariant passed: shape_single_1, only (0,0) occupied, coreCellLocal (0,0).
- I029 placement invariant passed: rotations 0/90/180/270 each occupy only (0,0).
- Catalog data check passed: 31 items, stable ids, required fields, and coreCellLocal inside shapeCells.
- BuildSettings check passed: Item Sandbox scene is not registered.
- Scene provider check passed: CatalogProvider exposes 31 readonly entries and no formal system flags.
- I009 derived contract passed: Candidate shapeDescription, five rarities, two seeds per rarity, Roll, Projection, and detail display all inherit shape_single_1.
- I029 derived contract passed: Candidate shapeDescription, five rarities, two seeds per rarity, Roll, Projection, and detail display all inherit shape_single_1.
- Scene list check passed: sandbox list has 31 item buttons.
- Hierarchy naming check passed: no Chinese GameObject names were added.
- Item detail artwork routing passed: I009/I029 displayShapeName resolves through the shared single_1 rule to DaojuSingleCellImage; DaojuMultiCellImage remains the multi-cell route, with no item-id special case.
- Source scope check completed for Item catalog and Item Sandbox adapter files.

## Errors
- None

