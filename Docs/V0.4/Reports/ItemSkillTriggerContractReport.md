# ItemSkillTriggerContract01 Report

- Package: `ItemSkillTriggerContract01`
- Guard receipt target: `GUARD_PASS_ITEMSKILLTRIGGERCONTRACT01`
- Verification: PASS
- Scope: independent Item Sandbox only.

## Implemented
- Explicit Sandbox Preview main Build selection input; default is None.
- Main Build selection accepts only faMen Build ids.
- Four fixed monitor slots: BasicAttack / Build2 / Build4 / Build6.
- Locked vs Monitoring greybox state only; no skill trigger, cooldown, charge, animation, damage, heal, shield, status, reward, save, boss, or battle wiring.
- Read-only future presentation fields: iconKey, tooltipText, presentationCueId, chibiActionKey.
- Independent ItemDetailViewModel `skillMonitorPreview` projection; BuildSynergyCore result remains pure Build statistics.

## Notes
- ItemDetailViewModel projection checked: independent skillMonitorPreview appears only when a SkillMonitor snapshot is supplied.
- Sandbox greybox UI checked: explicit None + five faMen choices and four fixed icon slots exist in the Item Sandbox scene.
- Lighting / ArrayBonus / BuildSynergyCore / CoreAwakening regression checks passed inside ItemSkillTriggerContract01 verifier.
- Source scope check completed: Item skill monitor contract stays inside Items/Skills and ItemSandbox preview paths, with no formal battle, bridge, run flow, save, reward, boss, or BuildSettings writes.

## Errors
- None

## Modified Files
- `Assets/_Game/Scripts/TalismanBag/Items/Skills/ItemSkillMonitorRules.cs`
- `Assets/_Game/Scripts/TalismanBag/Items/Detail/ItemDetailViewModel.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxSkillMonitorDetailProjection.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxGridPlacementPreviewView.cs`
- `Assets/_Game/Scripts/TalismanBag/ItemSandbox/ItemSandboxDetailPanelView.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSandboxDetailUiSceneBuilder.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/ItemSkillTriggerContractVerifier.cs`
- `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity`
- `Docs/V0.4/Reports/ItemSkillTriggerContractReport.md`
- `Docs/V0.4/Reports/ItemSkillTriggerContractSpec.csv`
- `Docs/V0.4/Reports/ItemSkillTriggerContractLeakCheckReport.md`
