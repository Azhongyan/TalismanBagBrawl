# BuildSynergyCore01 Report

- Package: `BuildSynergyCore01`
- Guard receipt target: `GUARD_PASS_BUILDSYNERGYCORE01`
- Verification: PASS
- Scope: independent Item Sandbox only.

## Implemented
- Lit-only faMen Build count for valid non-JuNian placements.
- Lit-only qiLei Build count for valid non-JuNian placements.
- faMen stages: Build2 / Build4 / Build6.
- qiLei stages: Build2 / Build4.
- ItemDetailViewModel projection for count, stage, progress, and validationErrors.
- Greybox overview and read-only `IItemBuildSynergySnapshotProvider` interface.
- placementId dedupe plus baseItemId/itemId uniqueness validation; duplicate base items are excluded from Build counts.
- validationErrors for duplicate base, duplicate/empty placementId, missing catalog, invalid tag, and null snapshot cases.
- No selected main Build, MonitorSlots, Build effects, numeric bonuses, skill releases, or settlement logic.

## Notes
- Build tracks checked: faMen Build2/4/6 and qiLei Build2/4 resolve from lit, non-JuNian item counts.
- Counting exclusions checked: only isLit=true, unique-base, non-JuNian, valid faMenTag/qiLeiTag placements count.
- Validation checked: duplicate placementId is deduped; duplicate base item is reported and excluded; empty placementId, missing catalog, and invalid tags are reported in validationErrors.
- Main Build and MonitorSlots checked: selectedMainBuildId remains empty and no monitor slot type/member is generated.
- ItemDetailViewModel projection checked: countedInBuild and faMen/qiLei progress are visible without mainBuild or monitor-slot projection.
- Snapshot interface checked: future packages can request read-only Build track and placement results.
- Greybox overview checked: status text summarizes faMen, qiLei, selectedMainBuildId reserved state, and validationErrors.
- Null snapshot checked: resolver emits empty track data and validationErrors instead of throwing.
- Source scope check completed: Build synergy files stay inside Items / ItemSandbox and do not reference formal battle, bridge, save, reward, boss, or BuildSettings writers.

## Errors
- None

## Modified Files
- `Assets/_Game/Scripts/TalismanBag/Items/Build.meta`
- `Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Build/ItemBuildSynergyRules.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/BuildSynergyCoreVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/BuildSynergyCoreVerifier.cs.meta`
- `Docs/V0.4/Reports/BuildSynergyCoreReport.md`
- `Docs/V0.4/Reports/BuildSynergyCoreSpec.csv`
- `Docs/V0.4/Reports/BuildSynergyCoreLeakCheckReport.md`
