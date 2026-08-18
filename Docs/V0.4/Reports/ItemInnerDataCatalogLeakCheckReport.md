# ItemInnerDataCatalog01 Leak Check Report

- Result: PASS
- BuildSettings: checked readonly; not modified; Item Sandbox scene is not registered.
- Formal flow: no V0.3 RunFlow, V0.4 Battle system, UnifiedBattlePage, Boss, Reward, SaveData, or formal scene integration.
- Real systems not implemented: lighting, adjacent relay, Build, awakening, drop/acquire, upgrade/reroll, save, battle settlement.
- Scene boundary: only `Scene_TalismanBag_V04_ItemSandbox.unity` is used for readonly catalog preview.
- ItemDetailViewModel boundary: CatalogProvider projects catalog data into ViewModel; UI does not read formal battle/runtime objects.
- Forbidden catalog entry: 阵眼石 is not in the catalog.
- Lighting source marker: only 聚念石 is marked as first-version lighting source identity.

## Passed Checks / Notes
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

