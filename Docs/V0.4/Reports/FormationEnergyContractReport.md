# Formation Energy Contract Report

Package: `V0.4-FormationEnergyContract01`
Guard Pass: `GUARD_PASS_FORMATION_ENERGY_CONTRACT01`
Generated: `2026-07-06 manual static snapshot`
Status: `PASS_WITH_RECORDED_LEGACY_DIAGNOSTICS`
Errors: `0`
Warnings: `1`
Recorded Contract Diagnostics: `6`

## Locked Contract

- Eye / formation core is layout state, not a formal energy source.
- Eye only grants `WeakPulse`; `WeakPulse` must not set `isPowered=true`.
- Only spirit stone / energy stone family items can be formal `Powered` providers.
- Energy incense, furnace/core, peach-wood, seal, and provider-like tags are diagnostics only.
- `isPowered` is derived from `energyState == Powered`.
- Scope stays BuildSandbox devOnly; no formal flow, chapter, reward, save, or scene layout writes.

## Code Survey

| Area | Finding | Contract Response |
| --- | --- | --- |
| `BuildSandboxPlacedItemSnapshot` | Existing snapshot already carried placement, tags, `isPowered`, source id, formation core fields, and item stat. | Added `energyState`, eye connection, formal source, weak-pulse, provider, and diagnostic fields; `isPowered` is derived. |
| `FormationCorePowerRange` | Legacy resolver treated energy incense, furnace/core, and provider tags as sources. | Resolver now delegates to `FormationEnergyContractResolver`; old report path remains compatibility-only. |
| `BattleSandboxCombatKernelAdapter` | Eye cross sample was previously `FullPowered`. | Eye cross / upgraded eye area now resolves to `WeakPowered`; stone range remains full power. |
| `BattleSandboxRuntimeLoop` | Runtime bonuses and mana loop still read `isPowered` and non-stone mana stats. | Runtime checks now read `energyState`; mana generation only comes from Powered energy stones. |
| `BuildGridInteractionPreviewController` | It builds current layout snapshots and calls preview energy links. | No UI/scene changes; current snapshot energy links now resolve through the new contract. |
| `Reports/FormationCorePowerRange*` | Rows previously listed incense/core as providers. | Rows now mark only energy stones as providers. |

## Similarities With Current V0.4

- Reuses BuildSandbox snapshot, preview, report, and validator infrastructure.
- Reuses existing `spirit_stone_basic` / `spirit_stone` identity as the legal provider family.
- Keeps item stat and battle preview as devOnly data surfaces.
- Keeps FormationCorePowerRange feedback path available for existing reports.

## Active Counters

| Counter | Value |
| --- | ---: |
| energyStoneProviderCount | 1 |
| poweredItemCount | 2 |
| weakPulseItemCount | 3 |
| suppressedItemCount | 0 |
| forbiddenProviderCandidateCount | 3 |
| tagProviderViolationCount | 1 |
| legacyProviderViolationCount | 2 |
| eyeCellOccupiedCount | 0 |
| scopeLeakCount | 0 |

## Notes

- `preview_energy_incense` and `preview_stone_core` remain in the default sample to prove they are no longer formal providers.
- `preview_taomu_sword`, `preview_energy_incense`, and `preview_stone_core` still carry non-stone mana/stat data in old item stat rows; the runtime mana loop now ignores them as energy sources.
- The default sample now includes `spirit_stone_basic` at the eye edge and keeps the eye cell empty.
