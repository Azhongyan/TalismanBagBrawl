# CoreAwakeningPreview01 Report

- Package: `CoreAwakeningPreview01`
- Guard receipt target: `GUARD_PASS_COREAWAKENINGPREVIEW01`
- Verification: PASS
- Scope: independent Item Sandbox only.

## Implemented
- Levels resolve as `resolvedLevel = Clamp(inputLevel, 1, 40)` while preserving raw `inputLevel`.
- Missing placement input defaults to Lv1 with `inputSource=MissingInputDefaultLv1` and validationErrors.
- High-rarity fields are reserved only; Ultimate unlocks only at resolvedLevel >= 40.
- Core effect definitions are item-specific: `{itemId}_CORE_01`, `{itemId}_CORE_02`, `{itemId}_CORE_03`, `{itemId}_CORE_ULT`.
- `IItemCoreEffectDefinitionProvider` provides read-only definitions and I031 returns none.
- Definition validation covers missing, duplicate id, duplicate nodeKind, wrong level, item mismatch, empty id, null definition, and null provider.
- Node output includes coreEffectId, itemId, nodeKind, unlockLevel, isUnlocked, isActive, stateKey, and blockedReason.
- Build, array bonus, selected main Build, and monitor slots do not participate in awakening unlocks.
- No formal upgrade, experience, rarity unlock, core numeric effect, combat execution, save, reward, boss, bridge, run flow, or BuildSettings work.

## Notes
- Definition provider checked: ordinary item definitions use itemId-specific ids and I031 returns no definitions.
- Level boundaries checked: Lv1, Lv9/10, Lv19/20, Lv29/30, and Lv39/40 unlock only by resolvedLevel thresholds.
- High-rarity preview checked: reserved rarity fields do not unlock Ultimate.
- JuNian source checked: I031 has no awakening support, basic effect, or core nodes.
- Input validation checked: inputLevel=0, inputLevel=41, and missing placement input are stable and non-throwing.
- Definition validation checked: missing, duplicate id, duplicate nodeKind, wrong level, item mismatch, empty id, null definition, and null provider.
- ItemDetailViewModel projection checked: inputLevel, resolvedLevel, coreEffectIds, stateKey, blockedReason, and validationErrors are visible.
- Sandbox preview catalog checked: normal preview levels stay within 1-40, exclude Lv0, and include 1/10/20/30/40 without relying on placementId names.
- Build/array isolation checked: Build6 and active array bonus do not unlock core effects; selectedMainBuildId remains empty and monitor slots remain absent.
- Lighting, ArrayBonus, and BuildSynergyCore regressions checked inside CoreAwakening verifier.
- Interfaces checked: future systems can request read-only inputs, resolved snapshots, and item-specific core effect definitions.
- Source scope check completed: runtime CoreAwakening preview paths do not reference formal battle, bridge, run flow, save, reward, boss, BuildSettings writers, or monitor slots.

## Errors
- None

## Modified Files
- `Assets/_Game/Scripts/TalismanBag/Items/Awakening.meta`
- `Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/InnerCatalog/ItemInnerDataCatalogProvider.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxCoreAwakeningPreviewCatalog.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxCoreAwakeningPreviewCatalog.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxLightingDetailProjection.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDevStubProvider.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs.meta`
- `Docs/V0.4/Reports/CoreAwakeningPreviewReport.md`
- `Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv`
- `Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md`
