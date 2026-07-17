# ItemPlacementUniqueBaseRule01 Report

- Package: `ItemPlacementUniqueBaseRule01`
- Guard receipt target: `GUARD_PASS_ITEMPLACEMENTUNIQUEBASERULE01`
- Result: **PASS**
- Checks: 18/18

## Rule

- Inventory ownership may contain multiple ordinary instances with the same baseItemId/itemId.
- One placed layout may contain only one ordinary item per baseItemId/itemId, independent of rarity, affixes, Seed, or itemInstanceId.
- I031 remains governed by its existing unique-source rule.
- Invalid duplicate bases are reported as `DUPLICATE_BASE_ITEM_PLACED` and excluded from Build counts.

## Checks

- PASS `WORKBENCH_CATALOG` — Item Sandbox workbench catalog exists. Evidence: Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset
- PASS `SAME_BASE_DIFFERENT_RARITY_REJECTED` — I001 with different rarities may be owned but cannot be co-placed. Evidence: white=wb_i001_white_710001; orange=wb_i001_orange_710002; second=False/DuplicateBaseItemPlaced
- PASS `SAME_BASE_DIFFERENT_SEED_REJECTED` — I001 with different seeds cannot be co-placed. Evidence: seeds=720001|720002; second=False/DuplicateBaseItemPlaced
- PASS `DIFFERENT_BASE_ALLOWED` — I001 and I002 can be placed together when board geometry is legal. Evidence: I001=True; I002=True/None
- PASS `I031_UNIQUE_SOURCE_RETAINED` — I031 keeps its existing unique-source rejection. Evidence: first=True; second=False/DuplicateJuNian
- PASS `SNAPSHOT_DUPLICATE_ERROR_METADATA` — Provider output is invalid and reports DUPLICATE_BASE_ITEM_PLACED with base item and placement ids. Evidence: Error:DUPLICATE_BASE_ITEM_PLACED: placementId=P_I001_B itemId=I001 baseItemId/itemId 'I001' is already placed by placementId 'P_I001_A'; one layout may place only one ordinary item with the same base identity.
- PASS `FORGED_SNAPSHOT_VALIDATOR_REJECTS` — Standalone validator rejects a manually constructed duplicate-base snapshot. Evidence: Error:DUPLICATE_BASE_ITEM_PLACED: placementId=P_I001_B itemId=I001 baseItemId/itemId 'I001' is already placed by placementId 'P_I001_A'; one layout may place only one ordinary item with the same base identity.
- PASS `DUPLICATE_BASE_CANNOT_BUILD2` — A duplicate I001 is excluded and cannot turn one legal base item into Build2. Evidence: count=1; activeStage=0; duplicateCounted=False
- PASS `LEGAL_BUILD6_USES_DISTINCT_BASES` — The legal validation Build6 uses I001-I006 exactly once each. Evidence: ids=I001|I002|I003|I004|I005|I006; seeds=610000|610000|610001|610000|610000|610000; count=6; Build6=True
- PASS `READABLE_DUPLICATE_FEEDBACK` — Sandbox feedback names the duplicate base id and explains the one-per-layout rule. Evidence: 重复上阵：同一个基础道具（I001）在同一布局中只能放置 1 件；不同品阶、词条、Seed 或实例 ID 仍视为同一 baseItemId/itemId。
- PASS `SCOPE_Assets__Game_Scripts_TalismanBag_Items_ItemSystemSnapshot_cs` — Scoped runtime source does not connect forbidden formal systems. Evidence: clean
- PASS `SCOPE_Assets__Game_Scripts_TalismanBag_Items_Build_ItemBuildSynergyRules_cs` — Scoped runtime source does not connect forbidden formal systems. Evidence: clean
- PASS `SCOPE_Assets__Game_Scripts_TalismanBag_ItemSandbox_ItemFullDetailBuildSandboxWorkbenchSession_cs` — Scoped runtime source does not connect forbidden formal systems. Evidence: clean
- PASS `SCOPE_Assets__Game_Scripts_TalismanBag_ItemSandbox_ItemSandboxV04BoardFullDetailAdapter_cs` — Scoped runtime source does not connect forbidden formal systems. Evidence: clean
- PASS `PROTECTED_Assets__Game_Scenes_Scene_TalismanBag_V04_ItemSandbox_unity` — Verifier does not rewrite protected Scene/Prefab/BuildSettings content. Evidence: before=8A9614B50BB732A46778356CE704CB405E60F7568055C82AD8BABD851CCB0343; after=8A9614B50BB732A46778356CE704CB405E60F7568055C82AD8BABD851CCB0343
- PASS `PROTECTED_Assets__Game_Scenes_Scene_TalismanBag_V04_BattleSandboxPreview_unity` — Verifier does not rewrite protected Scene/Prefab/BuildSettings content. Evidence: before=554FC001244E76218D47E04C51BD060A560E3297DEE247EEB91E5273F5FD6C17; after=554FC001244E76218D47E04C51BD060A560E3297DEE247EEB91E5273F5FD6C17
- PASS `PROTECTED_Assets__Game_Prefabs` — Verifier does not rewrite protected Scene/Prefab/BuildSettings content. Evidence: before=372B1E7DB1C94BA48E412D7946686556D640D414EAB77E339C32FA3E88044B5D; after=372B1E7DB1C94BA48E412D7946686556D640D414EAB77E339C32FA3E88044B5D
- PASS `PROTECTED_ProjectSettings_EditorBuildSettings_asset` — Verifier does not rewrite protected Scene/Prefab/BuildSettings content. Evidence: before=1079B5D8CEC522B3825F7E55C79FC972C7DA1D0F024FAF7885CBFD92F94EE9D0; after=1079B5D8CEC522B3825F7E55C79FC972C7DA1D0F024FAF7885CBFD92F94EE9D0

## Legal Build6
- I031 `(0,0)`; distinct orange I001-I006 anchors `(1,0) (2,0) (4,0) (2,1) (4,2) (2,3)`.
- The verifier discovers deterministic qualifying seeds and records them in `LEGAL_BUILD6_USES_DISTINCT_BASES`.

## Scope
- Item Sandbox / Item System only. No formal V0.4 Battle, V0.3 RunFlow, SaveData, Reward, Boss, drop, cultivation, or BuildSettings integration.
- Scene, Prefab, and BuildSettings protected hashes remain unchanged during verification.

## Marker
ITEM_PLACEMENT_UNIQUE_BASE_RULE01_PASS
