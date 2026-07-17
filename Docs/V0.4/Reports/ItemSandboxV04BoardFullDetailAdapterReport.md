# ItemSandbox V04 Board Full Detail Adapter Report

- Package: `V0.4-ItemSandboxV04BoardFullDetailAdapter01`
- Status: **PASS**
- Rework receipt: `TASK_REWORK2_COMPLETE_AWAITING_USER_PREFAB_BASELINE_DECISION`
- Baseline acceptance: `USER_ACCEPT_BASELINE_ITEMSANDBOXV04BOARDFULLDETAILADAPTER01` — User accepted current hand-tested scene/prefab as new protected baseline.
- ItemDetailMaxDaoArtTemplate01 exclusion: ItemDetailPanel child-subtree only; no new hash or geometry baseline accepted.
- Guard PASS receipt: `GUARD_PASS_ITEMSANDBOXV04BOARDFULLDETAILADAPTER01`
- Checks: 35/35
- Scene: `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`

## Result

- PASS `SCENE_ADAPTER` — Target scene has the one V04 adapter and existing providers. Evidence: adapter + providers serialized
- PASS `TRAY_31` — Tray is exactly I001-I031 in stable order. Evidence: I001|I002|I003|I004|I005|I006|I007|I008|I009|I010|I011|I012|I013|I014|I015|I016|I017|I018|I019|I020|I021|I022|I023|I024|I025|I026|I027|I028|I029|I030|I031
- PASS `JUNIAN_UNIQUE_SOURCE` — I031 is the only source item and normal items are I001-I030. Evidence: ordinary=30; source=I031
- PASS `SHAPE_CATALOG` — Every tray entry uses catalog shape cells and a core cell inside its shape. Evidence: shape_block2x2=1; shape_corner3=2; shape_line2_h=9; shape_line2_v=10; shape_line3_h=2; shape_line3_v=1; shape_single_1=6
- PASS `CANDIDATE_150` — 30 normal items × 5 rarities all generate successfully. Evidence: success=150
- PASS `CANDIDATE_RANGE_600` — Each candidate exposes four configured stat ranges. Evidence: rangeRows=600
- PASS `CANDIDATE_DETAIL_150` — All 150 candidates compose runtime ItemDetailPanel models. Evidence: detailModels=150
- PASS `CANDIDATE_DETERMINISTIC` — Same item/rarity/seed is deterministic and cached. Evidence: deterministic=150
- PASS `CORE_UNLOCK_LEVELS` — Candidate profiles expose configured core unlock levels. Evidence: candidateMaps=150
- PASS `INSTANCE_ID_UNIQUE` — Inventory may own repeated base items as independent instance ids before placement. Evidence: wb_i001_white_11 | wb_i001_white_12
- PASS `BOARD_RULES` — Validation layout resolves on the 5×5 board with eye (2,2) and four array cells. Evidence: layout=True; board=5; array=4
- PASS `PLACEMENT_IDS` — Every placed instance has an independent placementId. Evidence: placements=7; unique=7
- PASS `JUNIAN_ONCE` — Validation layout contains exactly one I031 placement. Evidence: I031 placements=1
- PASS `LIGHTING` — Direct/relay lighting resolves and no occupied cell is the eye. Evidence: direct=1; relay=6
- PASS `BUILD_TRACKS` — Qualified lit items expose Build2/4/6 track state for FaMen and Build2/4 for QiLei. Evidence: FaMen=5; QiLei=5
- PASS `MAIN_BUILD_DEFAULT_NONE` — MainBuild is explicit and defaults to None. Evidence: selected=None
- PASS `SKILL_MONITOR_4` — Exactly four readonly monitor slots resolve. Evidence: 普攻|Build2|Build4|Build6
- PASS `SANDBOX_LEVEL_1_40` — Lv1-Lv40 recomputes readonly visible/unlocked/lit/active core states. Evidence: Lv1 unlocked=0; Lv40 unlocked=30; Lv40 active=30
- PASS `DETAIL_PLACED` — Placed selection composes a runtime detail model with placement state. Evidence: selectedPlacementId=P_WORKBENCH_0007
- PASS `DETAIL_BUILD_PROGRESS_2_4_6` — Placed item detail Build sections expose final names while keeping stage skill descriptions. Evidence: 2/6, 4/6, 6/6, qilei 2/4 naming
- PASS `TRAY_BINDINGS` — Existing 40 copied slots are bound; exactly 31 are visible Item entries. Evidence: bound=40; active=31
- PASS `TRAY_SCROLL_31_ACCESS` — ItemTrayPreview is a vertical ScrollRect whose content owns all 30 normal items plus I031. Evidence: activeSlots=31; content=ItemTrayContent; vertical=True; horizontal=False
- PASS `CANDIDATE_CONTROLS_STAY_VISIBLE` — CandidateInstancePreviewControls remains active and interactable while candidate/detail data refreshes. Evidence: activeInHierarchy=True
- PASS `BOARD_BINDINGS` — Existing copied board has exactly 25 bound cells covering (0,0)-(4,4). Evidence: cells=25; unique=25
- PASS `DETAIL_PANEL_REUSE` — Existing runtime ItemDetailPanel is reused. Evidence: ItemDetailPanelView present
- PASS `FEEDBACK_BINDING` — FeedbackRoot owns the additive ItemSandbox feedback Text. Evidence: ItemSandboxFeedbackText serialized
- PASS `PROTECTED_GEOMETRY` — Surveyed BattleLikePreviewArea/FeedbackRoot/ItemTrayPreview/CandidateInstancePreviewControls geometry remains readonly; ItemDetailPanel is the sole authorized exclusion and the pre-task PopupLayer absence is preserved without rebuilding it. Evidence: PopupLayer=pre-task absent; CandidateInstancePreviewControls sibling=4 active=True pos=(106.00, -161.00) size=(127.32, 104.38); BattleLikePreviewArea sibling=6 active=True pos=(0.00, 23.00) size=(960.00, 1920.00); FeedbackRoot sibling=8 active=True pos=(0.00, 132.00) size=(0.00, 0.00); ItemTrayPreview sibling=2 active=True pos=(0.00, -515.00) size=(800.00, 800.00)
- PASS `NO_RUNTIME_LAYOUT_WRITES` — Runtime adapter/view scripts contain no hierarchy or RectTransform layout writes. Evidence: tokens scanned=8
- PASS `NO_FORMAL_SYSTEM_LEAK` — New ItemSandbox adapter does not reference formal flow/reward/save/Boss/battle systems. Evidence: tokens scanned=7
- PASS `TRAY_DRAG_CLICK_SPLIT` — Tray drag path does not open detail via SelectTrayItem; clicks remain the detail trigger. Evidence: click=SelectTrayItem; drag=SelectTrayItemForDrag
- PASS `HASH_ITEMSYSTEMSNAPSHOT_CS` — Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs remains byte-identical to pre-task baseline. Evidence: 821477FF5AE05E531953EF16B0E08FEC84F74FBCA63067C6C126BE7CF7EF13DF
- PASS `HASH_EDITORBUILDSETTINGS_ASSET` — ProjectSettings/EditorBuildSettings.asset remains byte-identical to pre-task baseline. Evidence: 08A277E3CA465A44E792318C0D3C210AFDBA61069F1170B74FA5A1A18598FE59
- PASS `TREE_HASH_ITEMBALANCEWORKBENCH` — Assets/_Game/Configs/ItemBalanceWorkbench candidate/schema tree remains byte-identical to the task baseline. Evidence: F941C253288D9D0D77AC25EB50716BA76F49E293DD58EA921927798102301915
- PASS `TREE_HASH_GENERATION` — Assets/_Game/Scripts/TalismanBag/Items/Generation candidate/schema tree remains byte-identical to the task baseline. Evidence: 5434BD4C7422681FBAC775ACA2C496B4C8A9EC4B772F1BFB096366533BB527B8
- PASS `REGRESSION_9_ITEM_8_ALGORITHM` — All nine historical Item System verifiers and eight Item Algorithm packages pass. Evidence: ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_REGRESSION_PASS

## Verified coverage

- Tray: I001-I030 normal items 30/30; I031 exactly once; legacy V0.4 test entries 0.
- Candidate matrix: 150/150 details and 600/600 configured stat ranges.
- Board: 5×5; eye `(2,2)` blocked; four array cells; direct + relay lighting; Build2/4/6; explicit MainBuild default None; four readonly monitor slots.
- Core preview: Sandbox Lv1-Lv40 uses configured profile unlock levels and recomputes visible/unlocked/lit/active state.

## UI layout protection

- Non-ItemDetail Survey structure remains readonly; no new geometry baseline was accepted.
- `CandidateInstancePreviewControls`: parent `ItemSandboxRoot`, active, sibling 4, anchoredPosition `(106,-161)`, sizeDelta `(127.32349,104.375206)`.
- `BattleLikePreviewArea`: parent `ItemSandboxRoot`, active, sibling 6, anchoredPosition `(0,23)`, sizeDelta `(960,1920)`.
- `ItemDetailPanel`: authorized child-subtree exclusion for `ItemDetailMaxDaoArtTemplate01`; no whole-Scene or whole-Prefab hash was accepted here.
- `PopupLayer`: absent in the pre-task Scene YAML; this package did not rebuild it.
- `FeedbackRoot`: parent `ItemSandboxRoot`, active, sibling 8, stretch anchors, anchoredPosition `(0,132)`, sizeDelta `(0,0)`.
- `ItemTrayPreview`: parent `BattleLikePreviewArea`, active, sibling 2, anchoredPosition `(0,-515)`, sizeDelta `(800,800)`.
- Geometry baseline check and runtime layout-write scan both PASS when all integrity checks pass.

## Regression

- Historical Item System verifiers: 9/9 PASS.
- Item Algorithm foundation packages: 8/8 PASS.
- Marker: `ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_REGRESSION_PASS`.
- Integrity gate report: `Docs/V0.4/Reports/ItemFullDetailBuildSandboxWorkbenchRegressionIntegrityReport.md`.

## Scope boundary

The adapter is ItemSandbox-only. It consumes existing catalog, generation projection, placement, lighting, array, Build, awakening, skill-monitor, and ItemDetailPanel contracts. It does not connect formal battle, RunFlow, reward, save, Boss, drop, cultivation, or BuildSettings.

## Manual test

1. Open `Scene_TalismanBag_V04_ItemSandbox`, enter Play, and confirm the tray shows I001-I031 only.
2. Click I001 and switch white/green/blue/purple/orange; each click must immediately update ItemDetailPanel. Change Seed and regenerate; already placed instances must not change.
3. Create two same-name normal instances (including different rarity or Seed). The first may be placed; the second must be rejected with a readable duplicate baseItemId/itemId reason. Drag I031 twice; the second placement must still be rejected.
4. Try out-of-bounds, overlap, and eye `(2,2)` placements; each must be rejected with a readable reason. Move, rotate, and remove a valid placement.
5. Reproduce the legal Build6 layout: I031 at `(0,0)`; orange I001-I006 at anchors `(1,0)`, `(2,0)`, `(4,0)`, `(2,1)`, `(4,2)`, `(2,3)` using the verifier-reported seeds. Confirm all six baseItemIds are distinct.
6. Confirm direct + relay lighting, array state, FaMen/QiLei counts, Build2/4/6, MainBuild default None, explicit MainBuild selection, and four monitor slots.
7. Set Sandbox Lv1 then Lv40 and inspect placed-item details; unlocked and active core states must rise according to configured profile levels and lighting.
8. Exit Play and confirm the three protected copied roots retain their Inspector-authored hierarchy and RectTransform values.

## Batch commands and logs

- Bind: `Unity.exe -batchmode -nographics -executeMethod TalismanBag.ItemSandbox.Editor.ItemSandboxV04BoardFullDetailSceneBinder.BindBatch -quit` → `Logs/codex_item_sandbox_v04_board_full_detail_bind.log`.
- Verify: `Unity.exe -batchmode -nographics -executeMethod TalismanBag.ItemSandbox.Editor.ItemSandboxV04BoardFullDetailAdapterVerifier.VerifyBatch -quit` → `Logs/codex_item_sandbox_v04_board_full_detail_verify.log`.
- Regression: `Unity.exe -batchmode -nographics -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemFullDetailBuildSandboxWorkbenchRegressionRunner.RunBatch -quit` → `Logs/codex_item_sandbox_v04_board_full_detail_regression.log`.

## Marker

ITEM_SANDBOX_V04_BOARD_FULL_DETAIL_ADAPTER01_PASS
