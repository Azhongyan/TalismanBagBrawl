# BuildSandbox ItemStat Foundation Report

Package: `V0.4-BuildSandboxItemStatFoundation01`
Generated: `2026-07-04 09:11:49`
Status: `PASS`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- User-specified devOnly direct package; not listed in the current V0.4 Package Queue.
- Adds report-only ItemStat foundations to BuildSandbox placed snapshots.
- Adds one devOnly preview item: 桃木剑, using shape id `vertical_3`.
- Keeps all ItemStat profiles devOnly=true and isEnabled=false.
- Does not touch V0.2/V0.3, formal battle, saves, rewards, Boss configs, formal number mainline, or scene UI layout.

## Counters

- placed item snapshot count: `8`
- item stat profile count: `8`
- item stat scope leak count: `0`
- taomu sword present: `True`
- taomu sword shape: `vertical_3`
- taomu sword stat profile: `stat_taomu_sword_vertical_3`
- player-side answer leak count: `0`
- formal flow leak count: `0`
- feature flag default true count: `0`

## ItemStat Rows

| Item | Shape | Cells | Stat Profile | Attack | Guard | Spirit | Control | Break | Cleanse | devOnly | isEnabled |
| --- | --- | ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| `preview_taomu_sword` | `vertical_3` | 3 | `stat_taomu_sword_vertical_3` | 8 | 5 | 2 | 1 | 4 | 2 | `True` | `False` |
| `preview_energy_incense` | `Vertical2` | 2 | `stat_energy_incense_vertical_2` | 1 | 1 | 7 | 0 | 0 | 0 | `True` | `False` |
| `preview_fire_talisman` | `Single1` | 1 | `stat_fire_talisman_single_1` | 6 | 0 | 3 | 0 | 1 | 0 | `True` | `False` |
| `preview_thunder_sword` | `Single1` | 1 | `stat_thunder_sword_single_1` | 7 | 1 | 3 | 1 | 5 | 0 | `True` | `False` |
| `preview_x2_wood_talisman` | `Vertical2` | 2 | `stat_guard_wood_vertical_2` | 2 | 6 | 3 | 1 | 1 | 0 | `True` | `False` |
| `preview_guard_wood` | `Vertical2` | 2 | `stat_guard_wood_vertical_2` | 2 | 6 | 3 | 1 | 1 | 0 | `True` | `False` |
| `preview_cleanse_corner` | `Corner3` | 3 | `stat_cleanse_corner_3` | 2 | 3 | 4 | 4 | 0 | 6 | `True` | `False` |
| `preview_stone_core` | `Square4` | 4 | `stat_stone_core_square_4` | 5 | 6 | 6 | 1 | 2 | 0 | `True` | `False` |

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| UI Layout Guard | `PASS` | 0 | 0 | 27 |
| BuildSandbox ItemStat Foundation 01 | `PASS` | 0 | 0 | 2 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs` |
| `Info` | `FORMAL_UI_SOURCE_REFERENCE_ONLY_ALLOWED` | This devOnly runtime playtest references mature formal UI sources without writing formal scene assets. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxEnemyCombatFeedbackController.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxEnemyEncounterPreviewController.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridInteractionPreviewController.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildGridPreviewSlotView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemTrayPreviewView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildPlacementFeedbackView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSandboxItemInfoPanel.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/MobileShapePlacementRuntimeIntegration.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/ShapeAwareItemTrayFixtureView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayGridReservationView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/BuildSandbox/TrayItemLayoutView.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxCombatInfoHudSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyCombatFeedbackUiReuseSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyCombatFeedbackUiReuseValidator.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyEncounterPreviewSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxEnemyEncounterPreviewValidator.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneBuilder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneReportWriter.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxPreviewSceneVerifier.cs` |
| `Info` | `FORMAL_UI_SOURCE_REFERENCE_ONLY_ALLOWED` | This devOnly runtime playtest references mature formal UI sources without writing formal scene assets. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BattleSandboxShapePlacementVerticalSliceReportWriter.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildGridInteractionPreviewReportWriter.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildGridInteractionPreviewSceneBinder.cs` |
| `Info` | `INDEPENDENT_PREVIEW_SCENE_AUTHORING_ALLOWED` | Independent V04 BuildSandbox preview packages are allowed to author devOnly preview scene UI inside the sandbox scene. | `Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildGridInteractionPreviewValidator.cs` |
| `Info` | `UI_LAYOUT_GUARD_PASS` | BuildSandbox code scan found no scene access, formal UI creation, or layout write tokens. | `` |
| `Info` | `ITEM_STAT_PROFILE_COUNT` | ItemStat profiles present on placed snapshots: 8. | `BuildSandboxPlacedItemSnapshot` |
| `Info` | `TAOMU_SWORD_VERTICAL3_READY` | Taomu sword uses vertical_3 and a disabled devOnly ItemStat profile. | `BuildSandboxPlacedItemSnapshot` |
