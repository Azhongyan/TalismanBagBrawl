# BattlePrepare Component Adapter Runtime Playtest Report

Package: `V0.4-BattlePrepareComponentAdapterRuntimePlaytest01`
Generated: `2026-07-01`
Status: `RUNTIME_PLAYTEST_BLOCKED`

## Current Window Result

This window implemented the real Unity Play entry path, but did not certify hand-feel pass. Unity was not available from this shell, so the current Codex window could not personally enter Play and touch the runtime path.

Blocker:

- `Unity` / `Unity.exe` was not found in PATH.
- `C:\Program Files\Unity\Hub\Editor` was not present in this environment.
- No static report is treated as runtime hand-feel pass.

## Runtime Entry

| Field | Value |
| --- | --- |
| Manual Menu Path | `Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[Manual Only] Open Runtime Playtest` |
| QA Menu Path | `Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[QA Only] Run Runtime Playtest Validation` |
| Target Scene | `Assets/_Game/Scenes/Scene_TalismanBag_V02_FormationCounter.unity` |
| Runtime Object | `V04BattlePrepareRuntimePlaytestController` |
| Extension Layer | `V04BattlePrepareRuntimePlaytestExtensionLayer` |
| Touch Path | Manual Unity Play, DontSave launcher, V03 BattlePrepare runtime entrypoint |

## Implemented Scope

- The manual menu opens the existing V02 FormationCounter scene, enters Unity Play, and injects a DontSave runtime controller.
- The runtime controller calls `V03BattlePrepareInteractionController.TryOpenPrepareThen` and reuses the mature BattlePrepare board, item tray, drag gesture, and pull-up motion.
- V0.4 multi-cell shape, rotation, and occupancy feedback are shown only in a devOnly extension overlay.
- QA validation confirms the entry/isolation contract. Human hand-feel pass still requires running the Manual Only menu in Unity.

## Guardrail Answers

| Question | Answer |
| --- | --- |
| Real Unity Play touchable path implemented | `true` |
| This window executed Unity Play handtest | `false` |
| Reuses mature V0.2 / V0.3 BattlePrepare board/tray/drag/pull-up | `true` |
| Redrew V04 board / item tray | `false` |
| Rewrote drag / pull-up animation | `false` |
| Modified formal V02 / V03 scenes | `false` |
| Overwrote user hand-tuned RectTransform | `false` |
| Touched formal RunFlow / PageState / FormationState / SaveData | `false` |
| Touched Boss / reward / drop / numeric systems | `false` |
| BuildSandbox FeatureFlag default enabled | `false` |
| Automation claims hand-feel pass | `false` |

## Runtime Surfaces

| Surface | Mature Source | Runtime Evidence |
| --- | --- | --- |
| 成熟整备打开 | `V03BattlePrepareInteractionController.TryOpenPrepareThen` | Manual launcher calls the existing mature prepare entrypoint after Unity enters Play. |
| 多格占格扩展 | `V02FormationGridFrame + TalismanGridSlotView` | DontSave overlay follows mature board slots and previews occupied cells without moving board RectTransforms. |
| 道具栏形状扩展 | `PrepareItemTrayRoot` | User still selects and drags mature BattlePrepare item views; shape data appears in the extension panel. |
| 拖拽旋转反馈 | `DraggableTalismanItemView` | Existing click/drag events and pointer movement drive the extension preview. |
| 占格合法性反馈 | Existing board slot positions | Valid cells are green; overlap/out-of-grid cells are red. |
| 上拉动画复用 | `PrepareMotionRoot` | The mature board + tray synchronized pull-up remains owned by V03 BattlePrepare. |

## Required Follow-Up

Run the Manual Only menu inside Unity. If the menu cannot enter Play, cannot open mature BattlePrepare, or cannot make board/tray/drag/rotation feedback touchable, keep this package at `RUNTIME_PLAYTEST_BLOCKED`.
