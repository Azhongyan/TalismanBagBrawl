# ItemCoreAwakeningCore4NodeExpansion01 Leak Check Report

- Package: `V0.4-ItemCoreAwakeningCore4NodeExpansion01`
- Result: FAIL
- Scope: existing Item Core Awakening resolver/definition provider and verifier only; reports are regenerated in place.
- BuildSettings: no writes; Item Sandbox remains manual-only.
- Forbidden integrations: formal battle resolver, bridge, run flow, save data, rewards, drops, boss data, formal upgrade systems, main Build selection, monitor slots, and BuildSettings writers.
- Not implemented: ExtraTrigger/Convert execution, formal item upgrade, experience, rarity evolution, storage, save serialization, combat skill release, damage/heal/shield/status settlement, rewards, drops, boss logic, selected main Build, MonitorSlots, or ItemSkillMonitorSlot.
- ItemSystemSnapshot.v2, Candidate content, Item Detail UI, Scene, Prefab and BuildSettings remain protected and byte-identical.
- Known unrelated dirty files intentionally untouched are outside this verifier's source scope.

## Passed Checks / Notes
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

