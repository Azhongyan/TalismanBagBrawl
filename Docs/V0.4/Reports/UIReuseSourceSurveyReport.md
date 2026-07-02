# UI Reuse Source Survey Report

Package: `V0.4-BuildGridInteractionPreview01-UIReuseCorrection01`
Generated: `2026-07-01`
Status: `PASS / REPORT_ONLY`
Guard: `GUARD_PASS_BUILDGRIDINTERACTIONPREVIEW01_UIREUSE_CORRECTION01`

## Scope

- This package only surveys UI reuse sources and drafts Adapter mappings.
- No V02/V03 formal scenes were modified.
- No V04 hand-tuned RectTransform, layout, backpack, board, pull-up animation, or item info popup was redrawn.
- `Scene_TalismanBag_V04_BattleSandboxPreview` UI created for BuildGridInteractionPreview01 is downgraded to `temporary preview / logic test bench`.

## Documents Read

- `Docs/LOCKED/BUILD_SANDBOX_BOUNDARY_LOCK.md`
- `Docs/ROADMAP/V0.4_BUILD_SANDBOX_PLAYABLE_ROADMAP.md`
- `Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md`
- `Docs/V0.4/UI_REUSE_SOURCE_REGISTRY.md`
- `Docs/V0.4/BuildGridInteractionPreview01_Assignment.md`
- `Docs/V0.4/BattlePageViewAdapter01_Assignment.md`
- `Docs/V0.4/BuildGridInteractionPreview01_UIReuseCorrection01_Assignment.md`

## Logic Preview Disposition

`V0.4-BuildGridInteractionPreview01` is accepted as a temporary logic preview.

The existing report `Docs/V0.4/Reports/BuildGridInteractionPreviewReport.md` records `Status: PASS`, `Errors: 0`, `Warnings: 0`, and sample coverage for:

| Capability | Keep As Logic Validation | Notes |
| --- | --- | --- |
| Multi-cell shape preview | `YES` | Validates Single1, Vertical2, Corner3, Square4 data flow. |
| Rotation | `YES` | Validates rotated occupied-cell calculation. |
| Out-of-bounds feedback | `YES` | Chinese placement feedback is present. |
| Overlap feedback | `YES` | Occupied-cell collision is reported. |
| Legal placement feedback | `YES` | Valid placement state is reported. |
| Category filtering | `YES` | All/Talisman/Tool/Material/Consumable/Special preview categories are present. |
| PreviewContext data flow | `YES` | BuildSandbox PreviewContext remains the data source for sandbox validation. |

## Temporary V04 UI Conclusion

The following V04 UI should not be promoted as formal UI truth:

| V04 Area | Temporary Status | Keep For | Mature UI Direction |
| --- | --- | --- | --- |
| `BoardGridPreview` and `BoardGridCell_*` | `TEMPORARY_PREVIEW` | Shape placement, rotation, bounds, overlap checks. | Reuse V02/V03 battle board and formation-grid language through Adapter. |
| `ItemTrayPreview`, `ItemTrayViewport`, `ItemTrayContent`, `ItemTraySlot_*` | `TEMPORARY_PREVIEW` | Scroll/category/data preview. | Reuse V03 BattlePrepare or V03 Upgrade item tray source. |
| V04 category buttons | `TEMPORARY_PREVIEW` | Category filter validation. | Reuse `ItemTrayCategoryTabs` language from V03 sources. |
| `SelectedItemInfo` | `TEMPORARY_PREVIEW` | Small selected-item summary during logic testing. | Fold into mature item tooltip/info panel ViewModel. |
| `BuildSandboxItemInfoPanel_Runtime` | `TEMPORARY_PREVIEW` | Sandbox-only item information structure exploration. | Replace with Adapter into V02/V03 item info/Tooltip structure. |
| `PlacementFeedback` | `TEMPORARY_PREVIEW` | Placement legality and Chinese feedback validation. | Reuse battle hint, tooltip, or BattleLog feedback language. |
| V04 drag ghost / occupied-cell highlight | `TEMPORARY_PREVIEW` | Interaction proof only. | Adapter should feed mature board affordance, not keep V04 visual source. |
| `ProblemSelector` / BuildSandbox data blocks | `TEMPORARY_PREVIEW` | Dev-only preview flow. | Move full fields into unified developer data panel. |

## Mature UI Reuse Source Candidates

| Candidate | Source Path | Reuse Target | Survey Result | Adapter / Risk Notes |
| --- | --- | --- | --- | --- |
| V03 BattlePrepare pull-up and tray | `Assets/_Game/Scripts/TalismanBag/V03/BattlePrepare/V03BattlePrepareInteractionController.cs` | Backpack pull-up, battle prepare motion, board/tray relation, item tray scroll, category tabs, prepare/continue buttons. | Mature interaction language exists: `V03BattlePrepareMotionRoot`, `V03BattlePrepareItemTrayRoot`, `ItemTrayCategoryTabs`, `ItemTrayScroll`, `ItemTrayGridSlot_*`, `V03PrepareToggleButton`. | Do not modify formal controller. It is coupled to `AutoCombatController`, `V02RunFlowController`, and `V02FormationGridFrame`; use as source language and later Adapter target after Guard approval. |
| V03 Upgrade item tray | `Assets/_Game/Scripts/TalismanBag/V03/Forge/V03TalismanUpgradeSceneController.cs` and `Assets/_Game/Scripts/TalismanBag/V03/Editor/V03TalismanUpgradeSceneBuilder.cs` | 5x8 item list, ScrollRect/GridLayout, category tabs, item detail panel rhythm. | Mature authored scene contract exists: `V03Upgrade_TalismanListPanel`, `ItemTrayCategoryTabs`, `ItemTrayScroll`, `ItemTrayVerticalScrollbar`, `ItemTrayGridSlot_01..40`. | Tied to upgrade services/save at runtime. Treat as read-only structure source unless a formal Adapter package is approved. |
| V02 talisman tooltip | `Assets/_Game/Scripts/TalismanBag/V02/UI/V02TalismanTooltipUI.cs` | Main item info panel/Tooltip structure. | Strongest item info structure candidate. It already has close button, selected outline, outside-click close, and grouped sections: function, value, upgrade, counter. | BuildSandbox should map fields into a ViewModel. Do not reuse formal scene GameObjects or modify this script in this package. |
| V03 Upgrade info popup | `Assets/_Game/Scripts/TalismanBag/V03/Forge/V03TalismanUpgradeSceneController.cs` | Upgrade-oriented item popup and before/after detail blocks. | Useful as secondary structure source for name, level, before/after, cost, status, and popup body. | Runtime is tied to upgrade page and save services; use as read-only reference for structure only. |
| Status tooltip | `Assets/_Game/Scripts/TalismanBag/V02/UI/StatusTooltipPanel.cs` | Small status/condition tooltip language. | Good lightweight source for status/condition explanations. | Not enough for full item info; use for small condition hints and status flavor. |
| Boss info panel | `Assets/_Game/Scripts/TalismanBag/V02/CoreLoop/Boss/BossInfoPanel.cs` and `BossInfoViewModel.cs` | BossProblem and BuildProblem masked clue display. | Mature Boss language exists: boss name, mechanism tags, main threats, recommended tools, pre-battle prompt. | Player side must receive vague Chinese clues only; Boss six-key full answer stays in developer data panel. |
| Battle event router and logs | `Assets/_Game/Scripts/TalismanBag/Feedback/BattleEventRouter.cs`, `Assets/_Game/Scripts/TalismanBag/UI/BattleLogUI.cs`, `Assets/_Game/Scripts/TalismanBag/UI/Mobile/MobileBattleLogPanel.cs` | Placement feedback, failure feedback, mechanism hints, result language. | Mature category language exists: Normal, Mana, Damage, Defense, Heal, Combo, Danger, Counter, Seal, Result. | BuildSandbox hints should map into categories, not create one panel per mechanic. |
| Floating/trigger/counter feedback | `Assets/_Game/Scripts/TalismanBag/Feedback/FloatingCombatText.cs`, `TalismanTriggerFeedback.cs`, `Assets/_Game/Scripts/TalismanBag/V02/Feedback/CounterFeedbackController.cs` | Micro feedback, item trigger flash, counter text, damage/energy feedback. | Existing short Chinese feedback language exists for damage, shield, heal, interrupt, counter, seal, unseal. | Use as feedback vocabulary. Do not connect BuildSandbox Modifier/Event to formal combat in this package. |

## Reuse Gaps And Risks

| Area | Why It Cannot Be Directly Reused Now | Risk If Ignored | Required Correction |
| --- | --- | --- | --- |
| V03 BattlePrepare controller | It binds formal combat, run flow, and V02 formation objects. | Accidentally mutates formal battle prepare behavior or RectTransform. | Add a future Adapter/ViewModel layer before any formal integration. |
| V02 item tooltip formal objects | Formal popup is wired to live `DraggableTalismanItemView` clicks and formal item definitions. | Leaks BuildSandbox fields or changes V02 formal tooltip behavior. | Build `BuildSandboxItemInfoAdapter` later; keep player text masked. |
| V03 Upgrade popup | Runtime depends on upgrade services and formal save/progress. | Reads/writes formal progression data by accident. | Use only as structure reference for now. |
| BossInfoPanel | Formal Boss flow owns start/adjust callbacks and Boss config. | Shows complete answer or touches Boss formal flow. | Map BuildProblem/BossProblem to vague clue ViewModel; full data goes to developer panel. |
| BattleLog/FloatingText | Formal combat emits live battle events. | Sandbox feedback could enter formal combat logs. | Reuse language and event categories only until a safe Adapter is assigned. |

## Player And Developer Data Boundary

Player side:

- Chinese-only text.
- Shows clues, placement feedback, atmosphere hints, and failure feedback.
- Does not show `hardSolutionTags`, `requiredSynergy`, `requiredAffix`, `requiredStats`, `DropBias` weights, or the complete Boss six-key answer.

Developer side:

- May show full rules, thresholds, Readiness, DropBias, Boss keys, and complete solution fields.
- Every field must carry a Chinese display name and an English stable key in config/report/developer data panel layers.

## Final Survey Conclusion

`BuildGridInteractionPreview01` logic preview is useful and can be kept as a test bench. Its V04 board, backpack/tray, pull-up feel, selected info summary, and sandbox item info popup are explicitly temporary. The next UI work should be Adapter/ViewModel mapping from BuildSandbox data into the registered V0.2/V0.3 mature UI sources, not another redraw of the backpack, board, pull-up animation, or item info popup.
