# Item Shape Catalog Batch Correction Report

- Package: `V0.4-ITEM_SHAPE_CATALOG_BATCH_CORRECTION01`
- Guard assignment: `GUARD_PASS_ASSIGNMENT_ITEM_SHAPE_CATALOG_BATCH_CORRECTION01`
- Result: `PASS`
- Marker: `ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0`
- Approved input: `Docs/V0.4/ItemShapeCatalogApprovedMatrix01.csv` (read-only)

## Counts

- Approved Items: `30/30`
- KEEP / CHANGE / unresolved: `14 / 16 / 0`
- Rotation cases: `120`
- Rarity versions: `150`; seed inheritance checks: `300`
- Item105 verifier migrations: `11`

## Canonical and aggregate evidence

- Catalog aggregate: `AF6E7C38B466B268DCF318D07DA3B9998E7F7667615077ECD0C6F82D9059F8E3` -> `A18DA1B5575C87281BF233AB690709A3FEE94F9F17153A60242E8D9E22550DB8`
- Profiles I001-I030 aggregate: `24074D6E801BD9967C2CC04CA1EBF812AA0638407A08D03B6356403FAC11C81C` -> `A4546761DFDB4837579E9C7EAA55A4D4C07059632C548B59BDCEB93C6EAAC7FC`
- Foundation signature: `sha256:dc0ad3a1e2f686507ad0196ef06bbb4189bd72f610406a1de2f3b16342ed80d3`
- Snapshot signature: `sha256:07961b49d2c0217bc454ca30088aa7a2de216209de532c5120c3315d8b62d025`
- Candidate signature: `sha256:cef35782e5b01565341eb915736d3e29d6bc78e6a50e600201a3f5143929f284`
- Roll150: `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8` (unchanged)
- Projection150: `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2` (unchanged)
- Item105 accepted baseline: `b81e3035a3f66d80d52970c442c56c203485164026572b251896197cf54cd1d4` -> `2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1`
- Item105 current dirty-worktree actual: `b57db6da9d92115a36d5601def6c7cad4de6cb9fe5546a9d27318f894d7c8152`

The accepted Item105 baseline is derived from the previously accepted 105-file input plus only the approved catalog shape correction. The current actual is not adopted because a pre-existing Item Detail UI delta is outside this package.

## Assertions

| Check | Result | Detail |
|---|---|---|
| `matrix.row-count` | PASS | 30/30 |
| `matrix.unique-item-ids` | PASS | unique=30 |
| `matrix.keep-count` | PASS | keep=14 |
| `matrix.change-count` | PASS | change=16 |
| `matrix.unresolved-count` | PASS | unresolved=0 |
| `catalog.ordinary-count` | PASS | 30/30 |
| `catalog.I001.matrix-exact` | PASS | shape_single_1/(0,0)/(0,0) |
| `catalog.I001.cells-valid` | PASS | count=1 |
| `catalog.I002.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I002.cells-valid` | PASS | count=2 |
| `catalog.I003.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I003.cells-valid` | PASS | count=2 |
| `catalog.I004.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,1) |
| `catalog.I004.cells-valid` | PASS | count=2 |
| `catalog.I005.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,1) |
| `catalog.I005.cells-valid` | PASS | count=2 |
| `catalog.I006.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I006.cells-valid` | PASS | count=2 |
| `catalog.I007.matrix-exact` | PASS | shape_single_1/(0,0)/(0,0) |
| `catalog.I007.cells-valid` | PASS | count=1 |
| `catalog.I008.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I008.cells-valid` | PASS | count=2 |
| `catalog.I009.matrix-exact` | PASS | shape_single_1/(0,0)/(0,0) |
| `catalog.I009.cells-valid` | PASS | count=1 |
| `catalog.I010.matrix-exact` | PASS | shape_corner3/(0,0);(1,0);(0,1)/(0,1) |
| `catalog.I010.cells-valid` | PASS | count=3 |
| `catalog.I011.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,1) |
| `catalog.I011.cells-valid` | PASS | count=2 |
| `catalog.I012.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I012.cells-valid` | PASS | count=2 |
| `catalog.I013.matrix-exact` | PASS | shape_single_1/(0,0)/(0,0) |
| `catalog.I013.cells-valid` | PASS | count=1 |
| `catalog.I014.matrix-exact` | PASS | shape_line3_v/(0,0);(0,1);(0,2)/(0,1) |
| `catalog.I014.cells-valid` | PASS | count=3 |
| `catalog.I015.matrix-exact` | PASS | shape_line3_v/(0,0);(0,1);(0,2)/(0,0) |
| `catalog.I015.cells-valid` | PASS | count=3 |
| `catalog.I016.matrix-exact` | PASS | shape_corner3/(0,0);(1,0);(0,1)/(0,1) |
| `catalog.I016.cells-valid` | PASS | count=3 |
| `catalog.I017.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,1) |
| `catalog.I017.cells-valid` | PASS | count=2 |
| `catalog.I018.matrix-exact` | PASS | shape_block2x2/(0,0);(1,0);(0,1);(1,1)/(1,1) |
| `catalog.I018.cells-valid` | PASS | count=4 |
| `catalog.I019.matrix-exact` | PASS | shape_single_1/(0,0)/(0,0) |
| `catalog.I019.cells-valid` | PASS | count=1 |
| `catalog.I020.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I020.cells-valid` | PASS | count=2 |
| `catalog.I021.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I021.cells-valid` | PASS | count=2 |
| `catalog.I022.matrix-exact` | PASS | shape_line3_v/(0,0);(0,1);(0,2)/(0,1) |
| `catalog.I022.cells-valid` | PASS | count=3 |
| `catalog.I023.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,1) |
| `catalog.I023.cells-valid` | PASS | count=2 |
| `catalog.I024.matrix-exact` | PASS | shape_line2_h/(0,0);(1,0)/(0,0) |
| `catalog.I024.cells-valid` | PASS | count=2 |
| `catalog.I025.matrix-exact` | PASS | shape_single_1/(0,0)/(0,0) |
| `catalog.I025.cells-valid` | PASS | count=1 |
| `catalog.I026.matrix-exact` | PASS | shape_line3_v/(0,0);(0,1);(0,2)/(0,0) |
| `catalog.I026.cells-valid` | PASS | count=3 |
| `catalog.I027.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,0) |
| `catalog.I027.cells-valid` | PASS | count=2 |
| `catalog.I028.matrix-exact` | PASS | shape_line2_v/(0,0);(0,1)/(0,1) |
| `catalog.I028.cells-valid` | PASS | count=2 |
| `catalog.I029.matrix-exact` | PASS | shape_single_1/(0,0)/(0,0) |
| `catalog.I029.cells-valid` | PASS | count=1 |
| `catalog.I030.matrix-exact` | PASS | shape_line3_v/(0,0);(0,1);(0,2)/(0,1) |
| `catalog.I030.cells-valid` | PASS | count=3 |
| `catalog.matrix-exact-total` | PASS | 30/30 |
| `catalog.I024.authored-horizontal` | PASS | (0,0);(1,0) |
| `catalog.I009.absorbed-single-fix` | PASS | (0,0) |
| `catalog.I029.absorbed-single-fix` | PASS | (0,0) |
| `catalog.I031.unchanged` | PASS | shape_single_1/(0,0) |
| `rotation.I001.0` | PASS | (4,4) |
| `rotation.I001.90` | PASS | (4,4) |
| `rotation.I001.180` | PASS | (4,4) |
| `rotation.I001.270` | PASS | (4,4) |
| `rotation.I002.0` | PASS | (4,4);(4,5) |
| `rotation.I002.90` | PASS | (3,4);(4,4) |
| `rotation.I002.180` | PASS | (4,3);(4,4) |
| `rotation.I002.270` | PASS | (4,4);(5,4) |
| `rotation.I003.0` | PASS | (4,4);(4,5) |
| `rotation.I003.90` | PASS | (3,4);(4,4) |
| `rotation.I003.180` | PASS | (4,3);(4,4) |
| `rotation.I003.270` | PASS | (4,4);(5,4) |
| `rotation.I004.0` | PASS | (4,4);(4,5) |
| `rotation.I004.90` | PASS | (3,4);(4,4) |
| `rotation.I004.180` | PASS | (4,3);(4,4) |
| `rotation.I004.270` | PASS | (4,4);(5,4) |
| `rotation.I005.0` | PASS | (4,4);(4,5) |
| `rotation.I005.90` | PASS | (3,4);(4,4) |
| `rotation.I005.180` | PASS | (4,3);(4,4) |
| `rotation.I005.270` | PASS | (4,4);(5,4) |
| `rotation.I006.0` | PASS | (4,4);(4,5) |
| `rotation.I006.90` | PASS | (3,4);(4,4) |
| `rotation.I006.180` | PASS | (4,3);(4,4) |
| `rotation.I006.270` | PASS | (4,4);(5,4) |
| `rotation.I007.0` | PASS | (4,4) |
| `rotation.I007.90` | PASS | (4,4) |
| `rotation.I007.180` | PASS | (4,4) |
| `rotation.I007.270` | PASS | (4,4) |
| `rotation.I008.0` | PASS | (4,4);(4,5) |
| `rotation.I008.90` | PASS | (3,4);(4,4) |
| `rotation.I008.180` | PASS | (4,3);(4,4) |
| `rotation.I008.270` | PASS | (4,4);(5,4) |
| `rotation.I009.0` | PASS | (4,4) |
| `rotation.I009.90` | PASS | (4,4) |
| `rotation.I009.180` | PASS | (4,4) |
| `rotation.I009.270` | PASS | (4,4) |
| `rotation.I010.0` | PASS | (4,4);(5,4);(4,5) |
| `rotation.I010.90` | PASS | (3,4);(4,4);(4,5) |
| `rotation.I010.180` | PASS | (4,3);(3,4);(4,4) |
| `rotation.I010.270` | PASS | (4,3);(4,4);(5,4) |
| `rotation.I011.0` | PASS | (4,4);(4,5) |
| `rotation.I011.90` | PASS | (3,4);(4,4) |
| `rotation.I011.180` | PASS | (4,3);(4,4) |
| `rotation.I011.270` | PASS | (4,4);(5,4) |
| `rotation.I012.0` | PASS | (4,4);(4,5) |
| `rotation.I012.90` | PASS | (3,4);(4,4) |
| `rotation.I012.180` | PASS | (4,3);(4,4) |
| `rotation.I012.270` | PASS | (4,4);(5,4) |
| `rotation.I013.0` | PASS | (4,4) |
| `rotation.I013.90` | PASS | (4,4) |
| `rotation.I013.180` | PASS | (4,4) |
| `rotation.I013.270` | PASS | (4,4) |
| `rotation.I014.0` | PASS | (4,4);(4,5);(4,6) |
| `rotation.I014.90` | PASS | (2,4);(3,4);(4,4) |
| `rotation.I014.180` | PASS | (4,2);(4,3);(4,4) |
| `rotation.I014.270` | PASS | (4,4);(5,4);(6,4) |
| `rotation.I015.0` | PASS | (4,4);(4,5);(4,6) |
| `rotation.I015.90` | PASS | (2,4);(3,4);(4,4) |
| `rotation.I015.180` | PASS | (4,2);(4,3);(4,4) |
| `rotation.I015.270` | PASS | (4,4);(5,4);(6,4) |
| `rotation.I016.0` | PASS | (4,4);(5,4);(4,5) |
| `rotation.I016.90` | PASS | (3,4);(4,4);(4,5) |
| `rotation.I016.180` | PASS | (4,3);(3,4);(4,4) |
| `rotation.I016.270` | PASS | (4,3);(4,4);(5,4) |
| `rotation.I017.0` | PASS | (4,4);(4,5) |
| `rotation.I017.90` | PASS | (3,4);(4,4) |
| `rotation.I017.180` | PASS | (4,3);(4,4) |
| `rotation.I017.270` | PASS | (4,4);(5,4) |
| `rotation.I018.0` | PASS | (4,4);(5,4);(4,5);(5,5) |
| `rotation.I018.90` | PASS | (3,4);(4,4);(3,5);(4,5) |
| `rotation.I018.180` | PASS | (3,3);(4,3);(3,4);(4,4) |
| `rotation.I018.270` | PASS | (4,3);(5,3);(4,4);(5,4) |
| `rotation.I019.0` | PASS | (4,4) |
| `rotation.I019.90` | PASS | (4,4) |
| `rotation.I019.180` | PASS | (4,4) |
| `rotation.I019.270` | PASS | (4,4) |
| `rotation.I020.0` | PASS | (4,4);(4,5) |
| `rotation.I020.90` | PASS | (3,4);(4,4) |
| `rotation.I020.180` | PASS | (4,3);(4,4) |
| `rotation.I020.270` | PASS | (4,4);(5,4) |
| `rotation.I021.0` | PASS | (4,4);(4,5) |
| `rotation.I021.90` | PASS | (3,4);(4,4) |
| `rotation.I021.180` | PASS | (4,3);(4,4) |
| `rotation.I021.270` | PASS | (4,4);(5,4) |
| `rotation.I022.0` | PASS | (4,4);(4,5);(4,6) |
| `rotation.I022.90` | PASS | (2,4);(3,4);(4,4) |
| `rotation.I022.180` | PASS | (4,2);(4,3);(4,4) |
| `rotation.I022.270` | PASS | (4,4);(5,4);(6,4) |
| `rotation.I023.0` | PASS | (4,4);(4,5) |
| `rotation.I023.90` | PASS | (3,4);(4,4) |
| `rotation.I023.180` | PASS | (4,3);(4,4) |
| `rotation.I023.270` | PASS | (4,4);(5,4) |
| `rotation.I024.0` | PASS | (4,4);(5,4) |
| `rotation.I024.90` | PASS | (4,4);(4,5) |
| `rotation.I024.180` | PASS | (3,4);(4,4) |
| `rotation.I024.270` | PASS | (4,3);(4,4) |
| `rotation.I025.0` | PASS | (4,4) |
| `rotation.I025.90` | PASS | (4,4) |
| `rotation.I025.180` | PASS | (4,4) |
| `rotation.I025.270` | PASS | (4,4) |
| `rotation.I026.0` | PASS | (4,4);(4,5);(4,6) |
| `rotation.I026.90` | PASS | (2,4);(3,4);(4,4) |
| `rotation.I026.180` | PASS | (4,2);(4,3);(4,4) |
| `rotation.I026.270` | PASS | (4,4);(5,4);(6,4) |
| `rotation.I027.0` | PASS | (4,4);(4,5) |
| `rotation.I027.90` | PASS | (3,4);(4,4) |
| `rotation.I027.180` | PASS | (4,3);(4,4) |
| `rotation.I027.270` | PASS | (4,4);(5,4) |
| `rotation.I028.0` | PASS | (4,4);(4,5) |
| `rotation.I028.90` | PASS | (3,4);(4,4) |
| `rotation.I028.180` | PASS | (4,3);(4,4) |
| `rotation.I028.270` | PASS | (4,4);(5,4) |
| `rotation.I029.0` | PASS | (4,4) |
| `rotation.I029.90` | PASS | (4,4) |
| `rotation.I029.180` | PASS | (4,4) |
| `rotation.I029.270` | PASS | (4,4) |
| `rotation.I030.0` | PASS | (4,4);(4,5);(4,6) |
| `rotation.I030.90` | PASS | (2,4);(3,4);(4,4) |
| `rotation.I030.180` | PASS | (4,2);(4,3);(4,4) |
| `rotation.I030.270` | PASS | (4,4);(5,4);(6,4) |
| `rotation.total` | PASS | 120/120 |
| `determinism.repeat` | PASS | 07961b49d2c0217bc454ca30088aa7a2de216209de532c5120c3315d8b62d025 |
| `determinism.reverse-input` | PASS | 07961b49d2c0217bc454ca30088aa7a2de216209de532c5120c3315d8b62d025 |
| `foundation.valid` | PASS | errors=0 |
| `foundation.ordinary-count` | PASS | 30/30 |
| `foundation.rarity-key-count` | PASS | 150/150 |
| `foundation.reverse-input` | PASS | dc0ad3a1e2f686507ad0196ef06bbb4189bd72f610406a1de2f3b16342ed80d3 |
| `foundation.I031-system-only` | PASS | I031 ordinary keys=0 |
| `candidate.catalog-exists` | PASS | Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset |
| `candidate.profile-count` | PASS | ordinaryProfiles=30 |
| `foundation.I001.shape` | PASS | shape_single_1 |
| `candidate.I001.shape-description` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `candidate.I001.five-rarities` | PASS | versions=5 |
| `candidate.I001.white.version` | PASS | I001@white |
| `candidate.I001.white.seed-100001` | PASS |  |
| `candidate.I001.white.seed-100002` | PASS |  |
| `candidate.I001.green.version` | PASS | I001@green |
| `candidate.I001.green.seed-100011` | PASS |  |
| `candidate.I001.green.seed-100012` | PASS |  |
| `candidate.I001.blue.version` | PASS | I001@blue |
| `candidate.I001.blue.seed-100021` | PASS |  |
| `candidate.I001.blue.seed-100022` | PASS |  |
| `candidate.I001.purple.version` | PASS | I001@purple |
| `candidate.I001.purple.seed-100031` | PASS |  |
| `candidate.I001.purple.seed-100032` | PASS |  |
| `candidate.I001.orange.version` | PASS | I001@orange |
| `candidate.I001.orange.seed-100041` | PASS |  |
| `candidate.I001.orange.seed-100042` | PASS |  |
| `foundation.I002.shape` | PASS | shape_line2_v |
| `candidate.I002.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I002.five-rarities` | PASS | versions=5 |
| `candidate.I002.white.version` | PASS | I002@white |
| `candidate.I002.white.seed-200001` | PASS |  |
| `candidate.I002.white.seed-200002` | PASS |  |
| `candidate.I002.green.version` | PASS | I002@green |
| `candidate.I002.green.seed-200011` | PASS |  |
| `candidate.I002.green.seed-200012` | PASS |  |
| `candidate.I002.blue.version` | PASS | I002@blue |
| `candidate.I002.blue.seed-200021` | PASS |  |
| `candidate.I002.blue.seed-200022` | PASS |  |
| `candidate.I002.purple.version` | PASS | I002@purple |
| `candidate.I002.purple.seed-200031` | PASS |  |
| `candidate.I002.purple.seed-200032` | PASS |  |
| `candidate.I002.orange.version` | PASS | I002@orange |
| `candidate.I002.orange.seed-200041` | PASS |  |
| `candidate.I002.orange.seed-200042` | PASS |  |
| `foundation.I003.shape` | PASS | shape_line2_v |
| `candidate.I003.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I003.five-rarities` | PASS | versions=5 |
| `candidate.I003.white.version` | PASS | I003@white |
| `candidate.I003.white.seed-300001` | PASS |  |
| `candidate.I003.white.seed-300002` | PASS |  |
| `candidate.I003.green.version` | PASS | I003@green |
| `candidate.I003.green.seed-300011` | PASS |  |
| `candidate.I003.green.seed-300012` | PASS |  |
| `candidate.I003.blue.version` | PASS | I003@blue |
| `candidate.I003.blue.seed-300021` | PASS |  |
| `candidate.I003.blue.seed-300022` | PASS |  |
| `candidate.I003.purple.version` | PASS | I003@purple |
| `candidate.I003.purple.seed-300031` | PASS |  |
| `candidate.I003.purple.seed-300032` | PASS |  |
| `candidate.I003.orange.version` | PASS | I003@orange |
| `candidate.I003.orange.seed-300041` | PASS |  |
| `candidate.I003.orange.seed-300042` | PASS |  |
| `foundation.I004.shape` | PASS | shape_line2_v |
| `candidate.I004.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `candidate.I004.five-rarities` | PASS | versions=5 |
| `candidate.I004.white.version` | PASS | I004@white |
| `candidate.I004.white.seed-400001` | PASS |  |
| `candidate.I004.white.seed-400002` | PASS |  |
| `candidate.I004.green.version` | PASS | I004@green |
| `candidate.I004.green.seed-400011` | PASS |  |
| `candidate.I004.green.seed-400012` | PASS |  |
| `candidate.I004.blue.version` | PASS | I004@blue |
| `candidate.I004.blue.seed-400021` | PASS |  |
| `candidate.I004.blue.seed-400022` | PASS |  |
| `candidate.I004.purple.version` | PASS | I004@purple |
| `candidate.I004.purple.seed-400031` | PASS |  |
| `candidate.I004.purple.seed-400032` | PASS |  |
| `candidate.I004.orange.version` | PASS | I004@orange |
| `candidate.I004.orange.seed-400041` | PASS |  |
| `candidate.I004.orange.seed-400042` | PASS |  |
| `foundation.I005.shape` | PASS | shape_line2_v |
| `candidate.I005.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `candidate.I005.five-rarities` | PASS | versions=5 |
| `candidate.I005.white.version` | PASS | I005@white |
| `candidate.I005.white.seed-500001` | PASS |  |
| `candidate.I005.white.seed-500002` | PASS |  |
| `candidate.I005.green.version` | PASS | I005@green |
| `candidate.I005.green.seed-500011` | PASS |  |
| `candidate.I005.green.seed-500012` | PASS |  |
| `candidate.I005.blue.version` | PASS | I005@blue |
| `candidate.I005.blue.seed-500021` | PASS |  |
| `candidate.I005.blue.seed-500022` | PASS |  |
| `candidate.I005.purple.version` | PASS | I005@purple |
| `candidate.I005.purple.seed-500031` | PASS |  |
| `candidate.I005.purple.seed-500032` | PASS |  |
| `candidate.I005.orange.version` | PASS | I005@orange |
| `candidate.I005.orange.seed-500041` | PASS |  |
| `candidate.I005.orange.seed-500042` | PASS |  |
| `foundation.I006.shape` | PASS | shape_line2_v |
| `candidate.I006.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I006.five-rarities` | PASS | versions=5 |
| `candidate.I006.white.version` | PASS | I006@white |
| `candidate.I006.white.seed-600001` | PASS |  |
| `candidate.I006.white.seed-600002` | PASS |  |
| `candidate.I006.green.version` | PASS | I006@green |
| `candidate.I006.green.seed-600011` | PASS |  |
| `candidate.I006.green.seed-600012` | PASS |  |
| `candidate.I006.blue.version` | PASS | I006@blue |
| `candidate.I006.blue.seed-600021` | PASS |  |
| `candidate.I006.blue.seed-600022` | PASS |  |
| `candidate.I006.purple.version` | PASS | I006@purple |
| `candidate.I006.purple.seed-600031` | PASS |  |
| `candidate.I006.purple.seed-600032` | PASS |  |
| `candidate.I006.orange.version` | PASS | I006@orange |
| `candidate.I006.orange.seed-600041` | PASS |  |
| `candidate.I006.orange.seed-600042` | PASS |  |
| `foundation.I007.shape` | PASS | shape_single_1 |
| `candidate.I007.shape-description` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `candidate.I007.five-rarities` | PASS | versions=5 |
| `candidate.I007.white.version` | PASS | I007@white |
| `candidate.I007.white.seed-700001` | PASS |  |
| `candidate.I007.white.seed-700002` | PASS |  |
| `candidate.I007.green.version` | PASS | I007@green |
| `candidate.I007.green.seed-700011` | PASS |  |
| `candidate.I007.green.seed-700012` | PASS |  |
| `candidate.I007.blue.version` | PASS | I007@blue |
| `candidate.I007.blue.seed-700021` | PASS |  |
| `candidate.I007.blue.seed-700022` | PASS |  |
| `candidate.I007.purple.version` | PASS | I007@purple |
| `candidate.I007.purple.seed-700031` | PASS |  |
| `candidate.I007.purple.seed-700032` | PASS |  |
| `candidate.I007.orange.version` | PASS | I007@orange |
| `candidate.I007.orange.seed-700041` | PASS |  |
| `candidate.I007.orange.seed-700042` | PASS |  |
| `foundation.I008.shape` | PASS | shape_line2_v |
| `candidate.I008.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I008.five-rarities` | PASS | versions=5 |
| `candidate.I008.white.version` | PASS | I008@white |
| `candidate.I008.white.seed-800001` | PASS |  |
| `candidate.I008.white.seed-800002` | PASS |  |
| `candidate.I008.green.version` | PASS | I008@green |
| `candidate.I008.green.seed-800011` | PASS |  |
| `candidate.I008.green.seed-800012` | PASS |  |
| `candidate.I008.blue.version` | PASS | I008@blue |
| `candidate.I008.blue.seed-800021` | PASS |  |
| `candidate.I008.blue.seed-800022` | PASS |  |
| `candidate.I008.purple.version` | PASS | I008@purple |
| `candidate.I008.purple.seed-800031` | PASS |  |
| `candidate.I008.purple.seed-800032` | PASS |  |
| `candidate.I008.orange.version` | PASS | I008@orange |
| `candidate.I008.orange.seed-800041` | PASS |  |
| `candidate.I008.orange.seed-800042` | PASS |  |
| `foundation.I009.shape` | PASS | shape_single_1 |
| `candidate.I009.shape-description` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `candidate.I009.five-rarities` | PASS | versions=5 |
| `candidate.I009.white.version` | PASS | I009@white |
| `candidate.I009.white.seed-900001` | PASS |  |
| `candidate.I009.white.seed-900002` | PASS |  |
| `candidate.I009.green.version` | PASS | I009@green |
| `candidate.I009.green.seed-900011` | PASS |  |
| `candidate.I009.green.seed-900012` | PASS |  |
| `candidate.I009.blue.version` | PASS | I009@blue |
| `candidate.I009.blue.seed-900021` | PASS |  |
| `candidate.I009.blue.seed-900022` | PASS |  |
| `candidate.I009.purple.version` | PASS | I009@purple |
| `candidate.I009.purple.seed-900031` | PASS |  |
| `candidate.I009.purple.seed-900032` | PASS |  |
| `candidate.I009.orange.version` | PASS | I009@orange |
| `candidate.I009.orange.seed-900041` | PASS |  |
| `candidate.I009.orange.seed-900042` | PASS |  |
| `foundation.I010.shape` | PASS | shape_corner3 |
| `candidate.I010.shape-description` | PASS | shape_corner3 · 3格 · 核心格(0,1) |
| `candidate.I010.five-rarities` | PASS | versions=5 |
| `candidate.I010.white.version` | PASS | I010@white |
| `candidate.I010.white.seed-1000001` | PASS |  |
| `candidate.I010.white.seed-1000002` | PASS |  |
| `candidate.I010.green.version` | PASS | I010@green |
| `candidate.I010.green.seed-1000011` | PASS |  |
| `candidate.I010.green.seed-1000012` | PASS |  |
| `candidate.I010.blue.version` | PASS | I010@blue |
| `candidate.I010.blue.seed-1000021` | PASS |  |
| `candidate.I010.blue.seed-1000022` | PASS |  |
| `candidate.I010.purple.version` | PASS | I010@purple |
| `candidate.I010.purple.seed-1000031` | PASS |  |
| `candidate.I010.purple.seed-1000032` | PASS |  |
| `candidate.I010.orange.version` | PASS | I010@orange |
| `candidate.I010.orange.seed-1000041` | PASS |  |
| `candidate.I010.orange.seed-1000042` | PASS |  |
| `foundation.I011.shape` | PASS | shape_line2_v |
| `candidate.I011.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `candidate.I011.five-rarities` | PASS | versions=5 |
| `candidate.I011.white.version` | PASS | I011@white |
| `candidate.I011.white.seed-1100001` | PASS |  |
| `candidate.I011.white.seed-1100002` | PASS |  |
| `candidate.I011.green.version` | PASS | I011@green |
| `candidate.I011.green.seed-1100011` | PASS |  |
| `candidate.I011.green.seed-1100012` | PASS |  |
| `candidate.I011.blue.version` | PASS | I011@blue |
| `candidate.I011.blue.seed-1100021` | PASS |  |
| `candidate.I011.blue.seed-1100022` | PASS |  |
| `candidate.I011.purple.version` | PASS | I011@purple |
| `candidate.I011.purple.seed-1100031` | PASS |  |
| `candidate.I011.purple.seed-1100032` | PASS |  |
| `candidate.I011.orange.version` | PASS | I011@orange |
| `candidate.I011.orange.seed-1100041` | PASS |  |
| `candidate.I011.orange.seed-1100042` | PASS |  |
| `foundation.I012.shape` | PASS | shape_line2_v |
| `candidate.I012.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I012.five-rarities` | PASS | versions=5 |
| `candidate.I012.white.version` | PASS | I012@white |
| `candidate.I012.white.seed-1200001` | PASS |  |
| `candidate.I012.white.seed-1200002` | PASS |  |
| `candidate.I012.green.version` | PASS | I012@green |
| `candidate.I012.green.seed-1200011` | PASS |  |
| `candidate.I012.green.seed-1200012` | PASS |  |
| `candidate.I012.blue.version` | PASS | I012@blue |
| `candidate.I012.blue.seed-1200021` | PASS |  |
| `candidate.I012.blue.seed-1200022` | PASS |  |
| `candidate.I012.purple.version` | PASS | I012@purple |
| `candidate.I012.purple.seed-1200031` | PASS |  |
| `candidate.I012.purple.seed-1200032` | PASS |  |
| `candidate.I012.orange.version` | PASS | I012@orange |
| `candidate.I012.orange.seed-1200041` | PASS |  |
| `candidate.I012.orange.seed-1200042` | PASS |  |
| `foundation.I013.shape` | PASS | shape_single_1 |
| `candidate.I013.shape-description` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `candidate.I013.five-rarities` | PASS | versions=5 |
| `candidate.I013.white.version` | PASS | I013@white |
| `candidate.I013.white.seed-1300001` | PASS |  |
| `candidate.I013.white.seed-1300002` | PASS |  |
| `candidate.I013.green.version` | PASS | I013@green |
| `candidate.I013.green.seed-1300011` | PASS |  |
| `candidate.I013.green.seed-1300012` | PASS |  |
| `candidate.I013.blue.version` | PASS | I013@blue |
| `candidate.I013.blue.seed-1300021` | PASS |  |
| `candidate.I013.blue.seed-1300022` | PASS |  |
| `candidate.I013.purple.version` | PASS | I013@purple |
| `candidate.I013.purple.seed-1300031` | PASS |  |
| `candidate.I013.purple.seed-1300032` | PASS |  |
| `candidate.I013.orange.version` | PASS | I013@orange |
| `candidate.I013.orange.seed-1300041` | PASS |  |
| `candidate.I013.orange.seed-1300042` | PASS |  |
| `foundation.I014.shape` | PASS | shape_line3_v |
| `candidate.I014.shape-description` | PASS | shape_line3_v · 3格 · 核心格(0,1) |
| `candidate.I014.five-rarities` | PASS | versions=5 |
| `candidate.I014.white.version` | PASS | I014@white |
| `candidate.I014.white.seed-1400001` | PASS |  |
| `candidate.I014.white.seed-1400002` | PASS |  |
| `candidate.I014.green.version` | PASS | I014@green |
| `candidate.I014.green.seed-1400011` | PASS |  |
| `candidate.I014.green.seed-1400012` | PASS |  |
| `candidate.I014.blue.version` | PASS | I014@blue |
| `candidate.I014.blue.seed-1400021` | PASS |  |
| `candidate.I014.blue.seed-1400022` | PASS |  |
| `candidate.I014.purple.version` | PASS | I014@purple |
| `candidate.I014.purple.seed-1400031` | PASS |  |
| `candidate.I014.purple.seed-1400032` | PASS |  |
| `candidate.I014.orange.version` | PASS | I014@orange |
| `candidate.I014.orange.seed-1400041` | PASS |  |
| `candidate.I014.orange.seed-1400042` | PASS |  |
| `foundation.I015.shape` | PASS | shape_line3_v |
| `candidate.I015.shape-description` | PASS | shape_line3_v · 3格 · 核心格(0,0) |
| `candidate.I015.five-rarities` | PASS | versions=5 |
| `candidate.I015.white.version` | PASS | I015@white |
| `candidate.I015.white.seed-1500001` | PASS |  |
| `candidate.I015.white.seed-1500002` | PASS |  |
| `candidate.I015.green.version` | PASS | I015@green |
| `candidate.I015.green.seed-1500011` | PASS |  |
| `candidate.I015.green.seed-1500012` | PASS |  |
| `candidate.I015.blue.version` | PASS | I015@blue |
| `candidate.I015.blue.seed-1500021` | PASS |  |
| `candidate.I015.blue.seed-1500022` | PASS |  |
| `candidate.I015.purple.version` | PASS | I015@purple |
| `candidate.I015.purple.seed-1500031` | PASS |  |
| `candidate.I015.purple.seed-1500032` | PASS |  |
| `candidate.I015.orange.version` | PASS | I015@orange |
| `candidate.I015.orange.seed-1500041` | PASS |  |
| `candidate.I015.orange.seed-1500042` | PASS |  |
| `foundation.I016.shape` | PASS | shape_corner3 |
| `candidate.I016.shape-description` | PASS | shape_corner3 · 3格 · 核心格(0,1) |
| `candidate.I016.five-rarities` | PASS | versions=5 |
| `candidate.I016.white.version` | PASS | I016@white |
| `candidate.I016.white.seed-1600001` | PASS |  |
| `candidate.I016.white.seed-1600002` | PASS |  |
| `candidate.I016.green.version` | PASS | I016@green |
| `candidate.I016.green.seed-1600011` | PASS |  |
| `candidate.I016.green.seed-1600012` | PASS |  |
| `candidate.I016.blue.version` | PASS | I016@blue |
| `candidate.I016.blue.seed-1600021` | PASS |  |
| `candidate.I016.blue.seed-1600022` | PASS |  |
| `candidate.I016.purple.version` | PASS | I016@purple |
| `candidate.I016.purple.seed-1600031` | PASS |  |
| `candidate.I016.purple.seed-1600032` | PASS |  |
| `candidate.I016.orange.version` | PASS | I016@orange |
| `candidate.I016.orange.seed-1600041` | PASS |  |
| `candidate.I016.orange.seed-1600042` | PASS |  |
| `foundation.I017.shape` | PASS | shape_line2_v |
| `candidate.I017.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `candidate.I017.five-rarities` | PASS | versions=5 |
| `candidate.I017.white.version` | PASS | I017@white |
| `candidate.I017.white.seed-1700001` | PASS |  |
| `candidate.I017.white.seed-1700002` | PASS |  |
| `candidate.I017.green.version` | PASS | I017@green |
| `candidate.I017.green.seed-1700011` | PASS |  |
| `candidate.I017.green.seed-1700012` | PASS |  |
| `candidate.I017.blue.version` | PASS | I017@blue |
| `candidate.I017.blue.seed-1700021` | PASS |  |
| `candidate.I017.blue.seed-1700022` | PASS |  |
| `candidate.I017.purple.version` | PASS | I017@purple |
| `candidate.I017.purple.seed-1700031` | PASS |  |
| `candidate.I017.purple.seed-1700032` | PASS |  |
| `candidate.I017.orange.version` | PASS | I017@orange |
| `candidate.I017.orange.seed-1700041` | PASS |  |
| `candidate.I017.orange.seed-1700042` | PASS |  |
| `foundation.I018.shape` | PASS | shape_block2x2 |
| `candidate.I018.shape-description` | PASS | shape_block2x2 · 4格 · 核心格(1,1) |
| `candidate.I018.five-rarities` | PASS | versions=5 |
| `candidate.I018.white.version` | PASS | I018@white |
| `candidate.I018.white.seed-1800001` | PASS |  |
| `candidate.I018.white.seed-1800002` | PASS |  |
| `candidate.I018.green.version` | PASS | I018@green |
| `candidate.I018.green.seed-1800011` | PASS |  |
| `candidate.I018.green.seed-1800012` | PASS |  |
| `candidate.I018.blue.version` | PASS | I018@blue |
| `candidate.I018.blue.seed-1800021` | PASS |  |
| `candidate.I018.blue.seed-1800022` | PASS |  |
| `candidate.I018.purple.version` | PASS | I018@purple |
| `candidate.I018.purple.seed-1800031` | PASS |  |
| `candidate.I018.purple.seed-1800032` | PASS |  |
| `candidate.I018.orange.version` | PASS | I018@orange |
| `candidate.I018.orange.seed-1800041` | PASS |  |
| `candidate.I018.orange.seed-1800042` | PASS |  |
| `foundation.I019.shape` | PASS | shape_single_1 |
| `candidate.I019.shape-description` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `candidate.I019.five-rarities` | PASS | versions=5 |
| `candidate.I019.white.version` | PASS | I019@white |
| `candidate.I019.white.seed-1900001` | PASS |  |
| `candidate.I019.white.seed-1900002` | PASS |  |
| `candidate.I019.green.version` | PASS | I019@green |
| `candidate.I019.green.seed-1900011` | PASS |  |
| `candidate.I019.green.seed-1900012` | PASS |  |
| `candidate.I019.blue.version` | PASS | I019@blue |
| `candidate.I019.blue.seed-1900021` | PASS |  |
| `candidate.I019.blue.seed-1900022` | PASS |  |
| `candidate.I019.purple.version` | PASS | I019@purple |
| `candidate.I019.purple.seed-1900031` | PASS |  |
| `candidate.I019.purple.seed-1900032` | PASS |  |
| `candidate.I019.orange.version` | PASS | I019@orange |
| `candidate.I019.orange.seed-1900041` | PASS |  |
| `candidate.I019.orange.seed-1900042` | PASS |  |
| `foundation.I020.shape` | PASS | shape_line2_v |
| `candidate.I020.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I020.five-rarities` | PASS | versions=5 |
| `candidate.I020.white.version` | PASS | I020@white |
| `candidate.I020.white.seed-2000001` | PASS |  |
| `candidate.I020.white.seed-2000002` | PASS |  |
| `candidate.I020.green.version` | PASS | I020@green |
| `candidate.I020.green.seed-2000011` | PASS |  |
| `candidate.I020.green.seed-2000012` | PASS |  |
| `candidate.I020.blue.version` | PASS | I020@blue |
| `candidate.I020.blue.seed-2000021` | PASS |  |
| `candidate.I020.blue.seed-2000022` | PASS |  |
| `candidate.I020.purple.version` | PASS | I020@purple |
| `candidate.I020.purple.seed-2000031` | PASS |  |
| `candidate.I020.purple.seed-2000032` | PASS |  |
| `candidate.I020.orange.version` | PASS | I020@orange |
| `candidate.I020.orange.seed-2000041` | PASS |  |
| `candidate.I020.orange.seed-2000042` | PASS |  |
| `foundation.I021.shape` | PASS | shape_line2_v |
| `candidate.I021.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I021.five-rarities` | PASS | versions=5 |
| `candidate.I021.white.version` | PASS | I021@white |
| `candidate.I021.white.seed-2100001` | PASS |  |
| `candidate.I021.white.seed-2100002` | PASS |  |
| `candidate.I021.green.version` | PASS | I021@green |
| `candidate.I021.green.seed-2100011` | PASS |  |
| `candidate.I021.green.seed-2100012` | PASS |  |
| `candidate.I021.blue.version` | PASS | I021@blue |
| `candidate.I021.blue.seed-2100021` | PASS |  |
| `candidate.I021.blue.seed-2100022` | PASS |  |
| `candidate.I021.purple.version` | PASS | I021@purple |
| `candidate.I021.purple.seed-2100031` | PASS |  |
| `candidate.I021.purple.seed-2100032` | PASS |  |
| `candidate.I021.orange.version` | PASS | I021@orange |
| `candidate.I021.orange.seed-2100041` | PASS |  |
| `candidate.I021.orange.seed-2100042` | PASS |  |
| `foundation.I022.shape` | PASS | shape_line3_v |
| `candidate.I022.shape-description` | PASS | shape_line3_v · 3格 · 核心格(0,1) |
| `candidate.I022.five-rarities` | PASS | versions=5 |
| `candidate.I022.white.version` | PASS | I022@white |
| `candidate.I022.white.seed-2200001` | PASS |  |
| `candidate.I022.white.seed-2200002` | PASS |  |
| `candidate.I022.green.version` | PASS | I022@green |
| `candidate.I022.green.seed-2200011` | PASS |  |
| `candidate.I022.green.seed-2200012` | PASS |  |
| `candidate.I022.blue.version` | PASS | I022@blue |
| `candidate.I022.blue.seed-2200021` | PASS |  |
| `candidate.I022.blue.seed-2200022` | PASS |  |
| `candidate.I022.purple.version` | PASS | I022@purple |
| `candidate.I022.purple.seed-2200031` | PASS |  |
| `candidate.I022.purple.seed-2200032` | PASS |  |
| `candidate.I022.orange.version` | PASS | I022@orange |
| `candidate.I022.orange.seed-2200041` | PASS |  |
| `candidate.I022.orange.seed-2200042` | PASS |  |
| `foundation.I023.shape` | PASS | shape_line2_v |
| `candidate.I023.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `candidate.I023.five-rarities` | PASS | versions=5 |
| `candidate.I023.white.version` | PASS | I023@white |
| `candidate.I023.white.seed-2300001` | PASS |  |
| `candidate.I023.white.seed-2300002` | PASS |  |
| `candidate.I023.green.version` | PASS | I023@green |
| `candidate.I023.green.seed-2300011` | PASS |  |
| `candidate.I023.green.seed-2300012` | PASS |  |
| `candidate.I023.blue.version` | PASS | I023@blue |
| `candidate.I023.blue.seed-2300021` | PASS |  |
| `candidate.I023.blue.seed-2300022` | PASS |  |
| `candidate.I023.purple.version` | PASS | I023@purple |
| `candidate.I023.purple.seed-2300031` | PASS |  |
| `candidate.I023.purple.seed-2300032` | PASS |  |
| `candidate.I023.orange.version` | PASS | I023@orange |
| `candidate.I023.orange.seed-2300041` | PASS |  |
| `candidate.I023.orange.seed-2300042` | PASS |  |
| `foundation.I024.shape` | PASS | shape_line2_h |
| `candidate.I024.shape-description` | PASS | shape_line2_h · 2格 · 核心格(0,0) |
| `candidate.I024.five-rarities` | PASS | versions=5 |
| `candidate.I024.white.version` | PASS | I024@white |
| `candidate.I024.white.seed-2400001` | PASS |  |
| `candidate.I024.white.seed-2400002` | PASS |  |
| `candidate.I024.green.version` | PASS | I024@green |
| `candidate.I024.green.seed-2400011` | PASS |  |
| `candidate.I024.green.seed-2400012` | PASS |  |
| `candidate.I024.blue.version` | PASS | I024@blue |
| `candidate.I024.blue.seed-2400021` | PASS |  |
| `candidate.I024.blue.seed-2400022` | PASS |  |
| `candidate.I024.purple.version` | PASS | I024@purple |
| `candidate.I024.purple.seed-2400031` | PASS |  |
| `candidate.I024.purple.seed-2400032` | PASS |  |
| `candidate.I024.orange.version` | PASS | I024@orange |
| `candidate.I024.orange.seed-2400041` | PASS |  |
| `candidate.I024.orange.seed-2400042` | PASS |  |
| `foundation.I025.shape` | PASS | shape_single_1 |
| `candidate.I025.shape-description` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `candidate.I025.five-rarities` | PASS | versions=5 |
| `candidate.I025.white.version` | PASS | I025@white |
| `candidate.I025.white.seed-2500001` | PASS |  |
| `candidate.I025.white.seed-2500002` | PASS |  |
| `candidate.I025.green.version` | PASS | I025@green |
| `candidate.I025.green.seed-2500011` | PASS |  |
| `candidate.I025.green.seed-2500012` | PASS |  |
| `candidate.I025.blue.version` | PASS | I025@blue |
| `candidate.I025.blue.seed-2500021` | PASS |  |
| `candidate.I025.blue.seed-2500022` | PASS |  |
| `candidate.I025.purple.version` | PASS | I025@purple |
| `candidate.I025.purple.seed-2500031` | PASS |  |
| `candidate.I025.purple.seed-2500032` | PASS |  |
| `candidate.I025.orange.version` | PASS | I025@orange |
| `candidate.I025.orange.seed-2500041` | PASS |  |
| `candidate.I025.orange.seed-2500042` | PASS |  |
| `foundation.I026.shape` | PASS | shape_line3_v |
| `candidate.I026.shape-description` | PASS | shape_line3_v · 3格 · 核心格(0,0) |
| `candidate.I026.five-rarities` | PASS | versions=5 |
| `candidate.I026.white.version` | PASS | I026@white |
| `candidate.I026.white.seed-2600001` | PASS |  |
| `candidate.I026.white.seed-2600002` | PASS |  |
| `candidate.I026.green.version` | PASS | I026@green |
| `candidate.I026.green.seed-2600011` | PASS |  |
| `candidate.I026.green.seed-2600012` | PASS |  |
| `candidate.I026.blue.version` | PASS | I026@blue |
| `candidate.I026.blue.seed-2600021` | PASS |  |
| `candidate.I026.blue.seed-2600022` | PASS |  |
| `candidate.I026.purple.version` | PASS | I026@purple |
| `candidate.I026.purple.seed-2600031` | PASS |  |
| `candidate.I026.purple.seed-2600032` | PASS |  |
| `candidate.I026.orange.version` | PASS | I026@orange |
| `candidate.I026.orange.seed-2600041` | PASS |  |
| `candidate.I026.orange.seed-2600042` | PASS |  |
| `foundation.I027.shape` | PASS | shape_line2_v |
| `candidate.I027.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `candidate.I027.five-rarities` | PASS | versions=5 |
| `candidate.I027.white.version` | PASS | I027@white |
| `candidate.I027.white.seed-2700001` | PASS |  |
| `candidate.I027.white.seed-2700002` | PASS |  |
| `candidate.I027.green.version` | PASS | I027@green |
| `candidate.I027.green.seed-2700011` | PASS |  |
| `candidate.I027.green.seed-2700012` | PASS |  |
| `candidate.I027.blue.version` | PASS | I027@blue |
| `candidate.I027.blue.seed-2700021` | PASS |  |
| `candidate.I027.blue.seed-2700022` | PASS |  |
| `candidate.I027.purple.version` | PASS | I027@purple |
| `candidate.I027.purple.seed-2700031` | PASS |  |
| `candidate.I027.purple.seed-2700032` | PASS |  |
| `candidate.I027.orange.version` | PASS | I027@orange |
| `candidate.I027.orange.seed-2700041` | PASS |  |
| `candidate.I027.orange.seed-2700042` | PASS |  |
| `foundation.I028.shape` | PASS | shape_line2_v |
| `candidate.I028.shape-description` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `candidate.I028.five-rarities` | PASS | versions=5 |
| `candidate.I028.white.version` | PASS | I028@white |
| `candidate.I028.white.seed-2800001` | PASS |  |
| `candidate.I028.white.seed-2800002` | PASS |  |
| `candidate.I028.green.version` | PASS | I028@green |
| `candidate.I028.green.seed-2800011` | PASS |  |
| `candidate.I028.green.seed-2800012` | PASS |  |
| `candidate.I028.blue.version` | PASS | I028@blue |
| `candidate.I028.blue.seed-2800021` | PASS |  |
| `candidate.I028.blue.seed-2800022` | PASS |  |
| `candidate.I028.purple.version` | PASS | I028@purple |
| `candidate.I028.purple.seed-2800031` | PASS |  |
| `candidate.I028.purple.seed-2800032` | PASS |  |
| `candidate.I028.orange.version` | PASS | I028@orange |
| `candidate.I028.orange.seed-2800041` | PASS |  |
| `candidate.I028.orange.seed-2800042` | PASS |  |
| `foundation.I029.shape` | PASS | shape_single_1 |
| `candidate.I029.shape-description` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `candidate.I029.five-rarities` | PASS | versions=5 |
| `candidate.I029.white.version` | PASS | I029@white |
| `candidate.I029.white.seed-2900001` | PASS |  |
| `candidate.I029.white.seed-2900002` | PASS |  |
| `candidate.I029.green.version` | PASS | I029@green |
| `candidate.I029.green.seed-2900011` | PASS |  |
| `candidate.I029.green.seed-2900012` | PASS |  |
| `candidate.I029.blue.version` | PASS | I029@blue |
| `candidate.I029.blue.seed-2900021` | PASS |  |
| `candidate.I029.blue.seed-2900022` | PASS |  |
| `candidate.I029.purple.version` | PASS | I029@purple |
| `candidate.I029.purple.seed-2900031` | PASS |  |
| `candidate.I029.purple.seed-2900032` | PASS |  |
| `candidate.I029.orange.version` | PASS | I029@orange |
| `candidate.I029.orange.seed-2900041` | PASS |  |
| `candidate.I029.orange.seed-2900042` | PASS |  |
| `foundation.I030.shape` | PASS | shape_line3_v |
| `candidate.I030.shape-description` | PASS | shape_line3_v · 3格 · 核心格(0,1) |
| `candidate.I030.five-rarities` | PASS | versions=5 |
| `candidate.I030.white.version` | PASS | I030@white |
| `candidate.I030.white.seed-3000001` | PASS |  |
| `candidate.I030.white.seed-3000002` | PASS |  |
| `candidate.I030.green.version` | PASS | I030@green |
| `candidate.I030.green.seed-3000011` | PASS |  |
| `candidate.I030.green.seed-3000012` | PASS |  |
| `candidate.I030.blue.version` | PASS | I030@blue |
| `candidate.I030.blue.seed-3000021` | PASS |  |
| `candidate.I030.blue.seed-3000022` | PASS |  |
| `candidate.I030.purple.version` | PASS | I030@purple |
| `candidate.I030.purple.seed-3000031` | PASS |  |
| `candidate.I030.purple.seed-3000032` | PASS |  |
| `candidate.I030.orange.version` | PASS | I030@orange |
| `candidate.I030.orange.seed-3000041` | PASS |  |
| `candidate.I030.orange.seed-3000042` | PASS |  |
| `candidate.version-total` | PASS | 150/150 |
| `candidate.seed-inheritance-total` | PASS | 300/300 |
| `profile.I001.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I001.serialized-shape` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `profile.I001.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I002.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I002.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I002.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I003.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I003.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I003.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I004.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I004.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `profile.I004.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I005.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I005.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `profile.I005.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I006.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I006.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I006.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I007.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I007.serialized-shape` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `profile.I007.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I008.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I008.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I008.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I009.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I009.serialized-shape` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `profile.I010.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I010.serialized-shape` | PASS | shape_corner3 · 3格 · 核心格(0,1) |
| `profile.I010.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I011.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I011.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `profile.I011.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I012.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I012.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I012.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I013.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I013.serialized-shape` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `profile.I013.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I014.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I014.serialized-shape` | PASS | shape_line3_v · 3格 · 核心格(0,1) |
| `profile.I014.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I015.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I015.serialized-shape` | PASS | shape_line3_v · 3格 · 核心格(0,0) |
| `profile.I015.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I016.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I016.serialized-shape` | PASS | shape_corner3 · 3格 · 核心格(0,1) |
| `profile.I016.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I017.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I017.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `profile.I017.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I018.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I018.serialized-shape` | PASS | shape_block2x2 · 4格 · 核心格(1,1) |
| `profile.I018.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I019.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I019.serialized-shape` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `profile.I019.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I020.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I020.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I020.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I021.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I021.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I021.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I022.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I022.serialized-shape` | PASS | shape_line3_v · 3格 · 核心格(0,1) |
| `profile.I022.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I023.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I023.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `profile.I023.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I024.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I024.serialized-shape` | PASS | shape_line2_h · 2格 · 核心格(0,0) |
| `profile.I024.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I025.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I025.serialized-shape` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `profile.I025.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I026.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I026.serialized-shape` | PASS | shape_line3_v · 3格 · 核心格(0,0) |
| `profile.I026.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I027.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I027.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,0) |
| `profile.I027.keep-no-diff` | PASS | APPROVED_KEEP |
| `profile.I028.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I028.serialized-shape` | PASS | shape_line2_v · 2格 · 核心格(0,1) |
| `profile.I028.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.I029.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I029.serialized-shape` | PASS | shape_single_1 · 1格 · 核心格(0,0) |
| `profile.I030.non-shape-fields` | PASS | only shapeDescription may differ |
| `profile.I030.serialized-shape` | PASS | shape_line3_v · 3格 · 核心格(0,1) |
| `profile.I030.approved-change-present` | PASS | current differs from HEAD only by approved shapeDescription |
| `profile.non-shape-total` | PASS | 30/30 |
| `detail.shared-single-multi-route` | PASS | Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs |
| `detail.item-id-special-cases` | PASS | specialCaseCount=0 |
| `shape.second-formal-table` | PASS | none |
| `shape.rarity-and-instance-no-copy` | PASS | rarityShapeField=False;identityShapeMember=False |
| `readonly.catalog` | PASS | System.Collections.Generic.IReadOnlyList`1[[TalismanBag.Items.InnerCatalog.ItemInnerDataDefinition, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]] |
| `readonly.foundation-archetypes` | PASS | ICollection.IsReadOnly |
| `readonly.foundation-rarity-keys` | PASS | ICollection.IsReadOnly |
| `readonly.snapshot-catalog` | PASS | ICollection.IsReadOnly |
| `readonly.snapshot-placements` | PASS | ICollection.IsReadOnly |
| `protected.approved-matrix` | PASS | 1/1 3AB8D46F58DC309112AD18F41DB6E6B59BC6D5863B7727DF0FE446D015A500E0 |
| `protected.scenes` | PASS | 7/7 F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B |
| `protected.prefabs` | PASS | 5/5 3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6 |
| `protected.build-settings` | PASS | 1/1 BB060D6524E555139F3A2B201FB8EDDFC3AFBCA08A6F02E0581F42C1D620F3A2 |
| `protected.png-and-importer-meta` | PASS | 938/938 44A0327AC7DE256C2032005BAF54AA4F5D54068B9FD4EC0DB26426E804A2C8DF |
| `protected.item-detail-ui-runtime` | PASS | 12/12 AC07A72D2B40CE501B2EC6740D5987E36A189BB458F12B48B48AAC5607512BF2 |
| `protected.cross-system-runtime` | PASS | 63/63 ADF2A87C1A4F139E0F61403F95DF1877EBACCE12CCECBE921275519D509BDDD6 |
| `protected.enemy-system-runtime` | PASS | 96/96 22C436BE429B063E3CC57DE4E76CD830066426472E4C5C0D6339A6B0A5852A49 |
| `protected.item-capability-runtime` | PASS | 13/13 8115BA53258C1ABDF99718126FA9C838A6C122024BD9899E24F5369B9E0C2765 |
| `item105.EnemyRequirementChannelApplicabilitySchemaContractVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.LayoutResilienceItemFactProjectionAdapterVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.LayoutResilienceStructuralPredicateContractVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.LayoutResilienceStructuralReadinessConsumerVerifier` | PASS | newOccurrences=2;oldOccurrences=0 |
| `item105.DevEncounterLayoutPressureAuthoringVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.LayoutResilienceRequirementChannelMigrationVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.LayoutResilienceRequirementMigrationSurveyVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.RealLayoutResilienceEvaluationPipelineVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.BuildCapabilityNormalizationRuleSurveyVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.BuildCapabilityRemainingBlockerSemanticSurveyVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.LayoutResilienceBattleSandboxPlaytestAdapterVerifier` | PASS | newOccurrences=1;oldOccurrences=0 |
| `item105.whitelist-count` | PASS | 11/11 |
| `canonical.roll150` | PASS | sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8 |
| `canonical.projection150` | PASS | sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2 |
| `capability.three-formal-bp` | PASS | break=1000;guard=800;cooldown=900 |
| `c02.blocked-baseline` | PASS | blocked=32/32;actualUnknownReduction=0 |

## Shortest user hand-test

1. Open the existing Item Sandbox only; do not save the scene.
2. Select I024 and confirm the detail shape is `shape_line2_h · 2格 · 核心格(0,0)`; select I010 and confirm `shape_corner3 · 3格 · 核心格(0,1)`. 
3. Select I009 and I029 and confirm both remain single-cell and use the existing single-cell artwork slot; spot-check one vertical multi-cell Item uses the existing multi-cell slot.

## Notes / pre-existing evidence

- PREEXISTING_ITEM105_WORKTREE_MISMATCH: scoped accepted=2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1; actual=b57db6da9d92115a36d5601def6c7cad4de6cb9fe5546a9d27318f894d7c8152; the package does not absorb the pre-existing Item Detail UI delta.

## Errors

- None

