# ItemDetailProjectionComplete01 Rework Leak Check

Result: FAIL

Checked scoped files:
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailProjectionComposer.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailSectionView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailUiController.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs`

Forbidden formal-system tokens:
- `UnifiedBattlePage`
- `BattleBridge`
- `BattleResolver`
- `V02RunFlow`
- `V03RunFlow`
- `Scene_TalismanBag_V04_BattleSandboxPreview`
- `EditorBuildSettings.scenes =`
- `SaveSystem`
- `RewardResolver`
- `BossController`

Runtime UI dependency boundary:
- No `TalismanBag.ItemSandbox`, resolver, Battle, RunFlow, SaveData dependency inside runtime UI folder.
- Sandbox detail panel and section are adapters over runtime UI.

Errors:
- Layout check failed for layout-1080x1920: 1080x1920; detail=viewport=1557x840; content=1557x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1557x840; content=1557x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- Layout check failed for layout-720x1280: 720x1280; detail=viewport=1003x424; content=1003x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1003x424; content=1003x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
