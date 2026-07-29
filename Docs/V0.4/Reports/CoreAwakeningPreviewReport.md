# ItemCoreAwakeningCore4NodeExpansion01 Report

- Package: `V0.4-ItemCoreAwakeningCore4NodeExpansion01`
- Guard receipt: `ITEM_GUARD_PASS_ITEMCOREAWAKENINGCORE4NODEEXPANSION01`
- Verification: FAIL
- Scope: independent Item Sandbox only.

## Implemented
- `I001-I030` expose five explicit item-specific Awakening definitions each: Core1/Core2/Core3/Core4/Ultimate (`150` unique definitions total).
- Core4 is `{itemId}_CORE_04`, `nodeKind=Core4`, `unlockLevel=40`, `requiredRarityKey=purple`; Ultimate remains independent as `{itemId}_CORE_ULT` with orange metadata.
- Existing enum identities remain `Core1=0`, `Core2=1`, `Core3=2`, `Ultimate=3`; `Core4=4` is appended while explicit node output remains `01/02/03/04/ULT`.
- Levels resolve as `resolvedLevel = Clamp(inputLevel, 1, 40)` while preserving raw `inputLevel`.
- Missing placement input defaults to Lv1 with `inputSource=MissingInputDefaultLv1` and validationErrors.
- High-rarity preview behavior remains Ultimate-only; rarity metadata does not unlock either Lv40 node.
- Core4 and Ultimate both unlock at resolvedLevel >= 40 but remain independent identities and states.
- `IItemCoreEffectDefinitionProvider` provides read-only definitions and I031 returns none.
- Definition validation covers Core4 missing/duplicate/wrong-level/swapped/shared-id cases plus existing missing, duplicate, mismatch, empty and null cases.
- `ItemSystemSnapshot.v2` five-node round-trip preserves independent Core4/Ultimate state without modifying the snapshot schema.
- Candidate profiles remain the protected semantic authority for ExtraTrigger/Convert content and stay at 150 unique rows.
- Node output includes coreEffectId, itemId, nodeKind, unlockLevel, isUnlocked, isActive, stateKey, and blockedReason.
- Build, array bonus, selected main Build, and monitor slots do not participate in awakening unlocks.
- No ExtraTrigger/Convert execution, formal upgrade, rarity unlock, core numeric effect, combat execution, save, reward, boss, bridge, run flow, UI, Scene, Prefab or BuildSettings work.

## Notes
- I001_I030_AWAKENING_150_PASS
- I031_EXCLUSION_PASS
- Candidate authority checked: 150 unique rows remain byte-protected and semantically distinct from Awakening ids.
- Level boundaries checked: Lv1, Lv9/10, Lv19/20, Lv29/30, and Lv39/40 unlock only by resolvedLevel thresholds; Core4 and Ultimate remain separate at Lv40.
- High-rarity preview checked: reserved preview remains Ultimate-only and does not unlock or reclassify Core4.
- JuNian source checked: I031 has no awakening support, basic effect, or core nodes.
- Input validation checked: inputLevel=0, inputLevel=41, and missing placement input are stable and non-throwing.
- Definition validation checked: Core4 missing/duplicate/wrong-level/shared-id isolation plus existing id, nodeKind, item, null and reversed-order cases.
- ItemDetailViewModel projection checked: inputLevel, resolvedLevel, coreEffectIds, stateKey, blockedReason, and validationErrors are visible.
- Sandbox preview catalog checked: normal preview levels stay within 1-40, exclude Lv0, and include 1/10/20/30/40 without relying on placementId names.
- Build/array isolation checked: Build6 and active array bonus do not unlock core effects; selectedMainBuildId remains empty and monitor slots remain absent.
- Lighting, ArrayBonus, and BuildSynergyCore regressions checked inside CoreAwakening verifier.
- Interfaces checked: future systems can request read-only inputs, resolved snapshots, and item-specific core effect definitions.
- ITEMSYSTEM_SNAPSHOT_ROUNDTRIP_PASS
- PROTECTED_HASHES_FAIL
- Source scope check completed: runtime CoreAwakening preview paths do not reference formal battle, bridge, run flow, save, reward, boss, BuildSettings writers, or monitor slots.

## Errors
- Protected hash mismatch for Assets/_Game/Scripts/TalismanBag/BuildSandbox/ItemSystemBattleSandboxBoardAuthority.cs: expected e1add9eddca28802cd297bc8becac378494e8cbcfcec2754ca50c732c78b0359, actual 6bef274da275d26afad97e6ecbc6ec61523e602d8cc000744fd290870bfd9a20.

## Modified Files
- `Assets/_Game/Scripts/TalismanBag/Items/Awakening/ItemCoreAwakeningRules.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/ItemSandbox/CoreAwakeningPreviewVerifier.cs`
- `Docs/V0.4/Reports/CoreAwakeningPreviewReport.md`
- `Docs/V0.4/Reports/CoreAwakeningPreviewSpec.csv`
- `Docs/V0.4/Reports/CoreAwakeningPreviewLeakCheckReport.md`
