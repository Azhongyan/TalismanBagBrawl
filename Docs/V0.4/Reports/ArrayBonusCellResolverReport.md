# ArrayBonusCellResolver01 Report

- Package: `ArrayBonusCellResolver01`
- Guard receipt target: `GUARD_PASS_ARRAYBONUSCELLRESOLVER01`
- Verification: PASS
- Scope: independent Item Sandbox only.

## Implemented
- Fixed AP cells: AP01=(2,1), AP02=(2,3), AP03=(1,2), AP04=(3,2).
- `placementId` identity for placed item previews, lighting placements, lighting results, and ItemDetailViewModel projection.
- Pure `ItemArrayBonusResolver` snapshot with cell states and per-placement item states.
- Active rule: `isLit && isOnArrayBonusCell`.
- State only: no numeric attack, defense, cooldown, multiplier, or settlement effects.

## Notes
- Rule constants checked: boardSize=5, eyeCell=(2,2), AP01/AP02/AP03/AP04 are fixed and exclude the eye.
- Placement rules checked: AP cells are legal, eyeCell remains blocked, multi-cell item can occupy two AP cells without covering the eye.
- Array samples checked: single-cell, unlit occupied, off-array, and legal multi-cell AP occupation all resolve as expected.
- placementId identity checked: two I001 placements resolve independently in lighting and array snapshots.
- No-light rule checked: occupying AP01 does not directly light, relay, or activate by itself.
- ItemDetailViewModel projection checked: placementId, AP occupancy, and active flags are visible in detail state.
- Source scope check completed: Item Sandbox array bonus files do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.

## Errors
- None

## Modified Files
- `Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemArrayBonusRules.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemArrayBonusRules.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/Items/Lighting/ItemLightingRules.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemGridPlacementRulePreview.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingScenarioCatalog.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ArrayBonusCellResolverVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ArrayBonusCellResolverVerifier.cs.meta`
- `Docs/V0.4/Reports/ArrayBonusCellResolverReport.md`
- `Docs/V0.4/Reports/ArrayBonusCellResolverSpec.csv`
- `Docs/V0.4/Reports/ArrayBonusCellResolverLeakCheckReport.md`
