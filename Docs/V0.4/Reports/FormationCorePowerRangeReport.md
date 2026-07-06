# Formation Core And Power Range Report

Package: `V0.4-FormationCoreAndPowerRange01`
Guard Pass: `GUARD_PASS_FORMATION_CORE_POWER_RANGE01`
Generated: `2026-07-06 static Codex snapshot`
Status: `PASS`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- FormationCorePowerRange is a compatibility view over `V0.4-FormationEnergyContract01`.
- Runtime board feedback reads `FormationEnergyContractResolver` and `item.energyState`.
- The V04 sandbox eye cell is `(2,2)` on the 5x5 board.
- Eye range is visualized as `WeakPulse` only.
- Formal `Powered` provider range is visualized only from spirit/energy stone family items.
- Energy incense, furnace/core, peach-wood, seal, and provider-like tags are diagnostics only; they do not become providers.
- No Scene YAML, RectTransform, LayoutGroup, Image, Text layout field, formal RunFlow, SaveData, reward, chapter, Boss, or BuildSettings writes.

## Code Survey

| File | Finding |
| --- | --- |
| `FormationEnergyContract.cs` | Defines `EnergyState` and the source-of-truth resolver. Eye gives `WeakPulse`; energy stones give `Powered`; forbidden families only emit diagnostics. |
| `FormationCorePowerRange.cs` | Kept as a compatibility/report view; rows now copy `EnergyState`, weak-pulse cells, provider diagnostics, and derived `isPowered`. |
| `BuildSandboxPlacedItemSnapshot.cs` | Already carries energy state/source/diagnostic fields needed by the V04 sandbox snapshot. |
| `BuildGridInteractionPreviewController.cs` | Runtime overlay now reads `FormationEnergyContractResolver`, shows eye, WeakPulse range, Powered stone range, and per-item state text. |
| `BuildGridPreviewSlotView.cs` | No layout or scene field changes; slot view remains the board cell host for DontSave overlays. |
| `BuildItemPreviewCardView.cs` | No persistent card layout changes; selected item info displays current energy state from the live board snapshot. |
| `BattleSandboxRuntimeLoop.cs` | Existing runtime loop already reads `energyState` for formal power checks. |
| `BattleSandboxBuildCombatPreview.cs` | Formation feedback rows now use contract states and the required masked Chinese prompts. |
| `FormationEnergyContractReport.md` | Existing contract report confirms `isPowered` is derived from `energyState == Powered`. |
| `FormationCorePowerRangeReport.md` | This report now covers the FormationCoreAndPowerRange01 visual/report acceptance counters. |

## Required Coverage

| Check | Result |
| --- | --- |
| eyeCell exists | `True` |
| eyeCell occupied | `0` |
| WeakPulse cells count | `4` |
| Powered provider count | `1` |
| Powered cells count | `5` |
| forbidden non-provider misidentified as provider | `0` |
| old `isPowered` main judgment mismatch | `0` |
| tag auto power provider count | `0` |
| formal flow / UI scene / Save / Reward / Chapter touch | `0` |
| Leak Check | `0` |

## Counters

| Counter | Value |
| --- | ---: |
| eyeCellExists | 1 |
| eyeCellOccupiedCount | 0 |
| weakPulseCellCount | 4 |
| weakPulseItemCount | 3 |
| poweredProviderCount | 1 |
| poweredItemCount | 2 |
| unpoweredOrWeakItemCount | 7 |
| poweredCellCount | 5 |
| forbiddenProviderCandidateRows | 3 |
| forbiddenProviderMisidentifiedCount | 0 |
| tagProviderViolationRows | 1 |
| tagAutoPowerProviderCount | 0 |
| legacyProviderViolationRows | 2 |
| oldIsPoweredDerivedMismatchCount | 0 |
| formationFeedbackRows | 1 |
| featureFlagDefaultTrueCount | 0 |
| formalFlowLeakCount | 0 |
| playerSideAnswerLeakCount | 0 |

## Sample Energy States

| Sample Item | EnergyState | Provider | Provider Misidentified | Notes |
| --- | --- | --- | --- | --- |
| `spirit_stone_basic` | `Powered` | `True` | `False` | legal Powered provider family |
| `preview_energy_incense` | `WeakPulse` | `False` | `False` | forbidden provider family remains receiver only |
| `preview_stone_core` | `Powered` | `False` | `False` | receives stone power but is not a provider |
| `preview_taomu_sword` | `None` | `False` | `False` | forbidden provider family remains ordinary item |
| `preview_soul_seal` | `NotInDefaultSample` | `False` | `False` | Square4 item remains available for manual placement; resolver token prevents seal-family provider promotion |

## Formation Power Rows

| Item | EnergyState | Provider | Powered | EyeCell | Source | Radius | Cells | Powered Range | Diagnostics | Player Feedback |
| --- | --- | --- | --- | --- | --- | ---: | --- | --- | --- | --- |
| `preview_taomu_sword` | `None` | `False` | `False` | `False` | `` | 0 | `0,0;0,1;0,2` | `` | `FORBIDDEN_PROVIDER_CANDIDATE;ORDINARY_MANA_SOURCE` | 供能断开，部分道具沉寂。 |
| `preview_energy_incense` | `WeakPulse` | `False` | `False` | `False` | `v04_center_eye` | 0 | `2,3;2,4` | `` | `FORBIDDEN_PROVIDER_CANDIDATE;LEGACY_PROVIDER_VIOLATION;ORDINARY_MANA_SOURCE` | 阵眼微亮，符阵仅被弱激活。 |
| `preview_fire_talisman` | `None` | `False` | `False` | `False` | `` | 0 | `1,1` | `` | `` | 供能断开，部分道具沉寂。 |
| `preview_thunder_sword` | `None` | `False` | `False` | `False` | `` | 0 | `1,0` | `` | `` | 供能断开，部分道具沉寂。 |
| `preview_x2_wood_talisman` | `None` | `False` | `False` | `False` | `` | 0 | `4,0;4,1` | `` | `` | 供能断开，部分道具沉寂。 |
| `preview_guard_wood` | `WeakPulse` | `False` | `False` | `False` | `v04_center_eye` | 0 | `1,2;1,3` | `` | `` | 阵眼微亮，符阵仅被弱激活。 |
| `preview_cleanse_corner` | `WeakPulse` | `False` | `False` | `False` | `v04_center_eye` | 0 | `2,0;3,0;2,1` | `` | `` | 阵眼微亮，符阵仅被弱激活。 |
| `spirit_stone_basic` | `Powered` | `True` | `True` | `False` | `spirit_stone_basic` | 1 | `3,2` | `3,1;2,2;3,2;4,2;3,3` | `` | 聚能石连通，符阵供能稳定。 |
| `preview_stone_core` | `Powered` | `False` | `True` | `False` | `spirit_stone_basic` | 0 | `3,3;4,3;3,4;4,4` | `` | `FORBIDDEN_PROVIDER_CANDIDATE;TAG_PROVIDER_VIOLATION;LEGACY_PROVIDER_VIOLATION;ORDINARY_MANA_SOURCE` | 聚能石连通，符阵供能稳定。 |

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| UI Layout Guard | `PASS` | 0 | 0 | 29 |
| Formation Core And Power Range 01 | `PASS` | 0 | 0 | 1 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `FORMATION_POWER_COUNTS` | eyeCellExists=True, eyeCellOccupied=0, weakPulseCells=4, providers=1, powered=2, rangeCells=5, forbiddenProviderMisidentified=0, tagAutoPowerProvider=0. | `FormationCorePowerRangePreview` |
