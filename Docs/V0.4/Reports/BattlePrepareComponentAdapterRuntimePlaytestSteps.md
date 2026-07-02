# BattlePrepare Component Adapter Runtime Playtest Steps

Package: `V0.4-BattlePrepareComponentAdapterRuntimePlaytest01`
Status: `RUNTIME_PLAYTEST_BLOCKED` until a real Unity Play handtest is executed.

## Manual Unity Play Steps

| Step | Action | Expected |
| --- | --- | --- |
| `openMenu` | Run `Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterRuntimePlaytest01/[Manual Only] Open Runtime Playtest`. | Unity opens the V02 FormationCounter scene and enters Play with a DontSave runtime launcher. |
| `waitForPrepare` | Wait for the devOnly runtime banner and mature BattlePrepare tray. | Existing board, item tray, drag gesture, and pull-up motion are visible and touchable. |
| `selectItem` | Click or drag a mature BattlePrepare item. | The V0.4 extension panel binds a sandbox shape to the selected mature item. |
| `rotate` | Press `R` or click `Rotate Shape`. | Rotation changes occupied-cell preview without replacing mature drag behavior. |
| `hoverOrDragBoard` | Hover or drag over board cells. | The devOnly overlay shows green valid cells or red invalid overlap/out-of-grid feedback. |
| `stopPlay` | Stop Play. | DontSave launcher and extension layer disappear; no V02/V03 scene asset or hand-tuned RectTransform is saved. |

## Handtest Checklist

- Mature BattlePrepare opens through Play and remains touchable.
- Mature item tray click/drag hand feel still matches V0.2 / V0.3 behavior.
- Mature board hover/drag focus remains stable and has no layout jump.
- V0.4 occupied cells change when pressing `R` or clicking `Rotate Shape`.
- Valid placement feedback appears green.
- Overlap and out-of-grid feedback appears red.
- Stopping Play leaves no scene/layout changes to save.

This checklist is intentionally manual. Automated QA cannot certify touch feel.
