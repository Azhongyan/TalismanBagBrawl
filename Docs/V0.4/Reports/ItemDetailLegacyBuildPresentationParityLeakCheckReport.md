# ItemDetailLegacyBuildPresentationParity01 Leak Check

Status: OFFLINE_PASS / USER_HANDTEST_WAITING

- `ItemDetailPanelView.cs` changed only for local runtime Build authored-state/theme compatibility, exact visible-frame outside-dismiss geometry, and same-PointerDown consumption.
- Existing direct wheel/touch scrolling was restored; no Viewport Graphic, ScrollRect routing or inertia change remains.
- `ItemDetailSectionView.cs` unchanged.
- BattleSandbox Scene and ItemDetailPanel Prefab unchanged.
- `BuildItemTrayPreviewView.cs` and tray physics/layout unchanged.
- QualifiedBuild contract/assembler, ItemSystemSnapshot and ItemBuildSynergyRules unchanged.
- No Scene Builder, Prefab migration, RunFlow, Save, Reward, Enemy, Battle, RNG, commit, tag, push, reset or rollback.
