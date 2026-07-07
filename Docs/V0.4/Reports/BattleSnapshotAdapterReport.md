# Battle Snapshot Adapter Report

Package: `V0.4-BattleSnapshotAdapter01`
Generated: `2026-07-07 15:51:55`
Status: `PASS`
Errors: `0`
Warnings: `0`
Leak Count: `0`

## Scope

- Adds neutral `TalismanBag.Contracts.Battle` data contracts and read-only adapters.
- Does not create or modify UnifiedBattlePage, scenes, prefabs, UI layout, BuildSettings, RunFlow, SaveData, rewards, Boss, or chapter progression.
- V0.4 sandbox results stay devOnly and cannot grant reward or write save through this contract layer.

## Contract Types

| Type | Layer | Purpose |
| --- | --- | --- |
| `BattleStartRequest` | Contracts/Battle | Read-only battle request data. |
| `BattleLayoutSnapshot` | Contracts/Battle | Neutral board/layout state. |
| `BattleItemSnapshot` | Contracts/Battle | Neutral item placement and stat state. |
| `BattleEnemySnapshot` | Contracts/Battle | Neutral enemy/boss readable state. |
| `BuildEvaluationSnapshot` | Contracts/Battle | Player build summary plus dev diagnostics. |
| `BattleResultSnapshot` | Contracts/Battle | Result data, not a commit command. |
| `BattleContractDiagnostics` | Contracts/Battle | Report/dev-only diagnostics. |

## Validation Samples

| Check | Value | Expected |
| --- | ---: | ---: |
| V0.3 placed item count | 1 | >= 1 |
| V0.3 single-cell errors | 0 | 0 |
| V0.4 placed item count | 9 | >= 1 |
| V0.4 multi-cell items | 6 | >= 1 |
| V0.4 multi-cell errors | 0 | 0 |
| Player/dev field leak count | 0 | 0 |
| Sandbox result devOnly | True | true |
| Sandbox result shouldWriteSave | False | false |
| Sandbox result shouldGrantReward | False | false |

## V0.3 Single Cell Mapping

| Item | Shape | Rotation | Anchor | Occupied | Legacy |
| --- | --- | ---: | --- | --- | --- |
| `fire_talisman_basic` | `Single1` | 0 | `1,2` | `1,2` | `True` |

## V0.4 Layout Normalization

| Item | Shape | Rotation | Occupied Count | Energy | Source Container |
| --- | --- | ---: | ---: | --- | --- |
| `preview_taomu_sword` | `vertical_3` | 0 | 3 | `None` | `v04_buildsandbox_layout` |
| `preview_energy_incense` | `Vertical2` | 0 | 2 | `WeakPulse` | `v04_buildsandbox_layout` |
| `preview_fire_talisman` | `Single1` | 0 | 1 | `None` | `v04_buildsandbox_layout` |
| `preview_thunder_sword` | `Single1` | 0 | 1 | `None` | `v04_buildsandbox_layout` |
| `preview_x2_wood_talisman` | `Vertical2` | 0 | 2 | `None` | `v04_buildsandbox_layout` |
| `preview_guard_wood` | `Vertical2` | 0 | 2 | `WeakPulse` | `v04_buildsandbox_layout` |
| `preview_cleanse_corner` | `Corner3` | 0 | 3 | `WeakPulse` | `v04_buildsandbox_layout` |
| `spirit_stone_basic` | `Single1` | 0 | 1 | `Powered` | `v04_buildsandbox_layout` |
| `preview_stone_core` | `Square4` | 0 | 4 | `Powered` | `v04_buildsandbox_layout` |

## Validation Summary

| Check | Status | Errors | Warnings | Info |
| --- | --- | ---: | ---: | ---: |
| BattleSnapshotAdapter01 Field Validator | `PASS` | 0 | 0 | 14 |

## Issues

| Level | Code | Message | Path |
| --- | --- | --- | --- |
| `Info` | `V03_SINGLE_CELL_MAP` | V0.3 sample maps to Single1, rotation 0, one occupied cell. | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `V04_MULTI_CELL_MAP` | V0.4 sample normalizes at least one multi-cell item. | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `PLAYER_DEV_FIELD_SPLIT` | Player fields do not include developer answer-layer tokens. | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `SANDBOX_RESULT_DEFAULTS` | Sandbox result defaults to devOnly/no-save/no-reward. | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `FORMAL_WRITE_LEAKS` | Adapters do not write formal flow, save, reward, scene UI, or settlements. | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `FEATURE_FLAG_DEFAULT_TRUE` | BuildSandbox feature flag defaults remain disabled. | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `DEVONLY_FORMAL_FLOW_LEAKS` | devOnly sandbox preview does not leak into formal flow. | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `BATTLE_SNAPSHOT_SAMPLE` | v03PlacedItems=1 | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `BATTLE_SNAPSHOT_SAMPLE` | v04PlacedItems=9 | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `BATTLE_SNAPSHOT_SAMPLE` | v04MultiCellItems=6 | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `BATTLE_SNAPSHOT_SAMPLE` | activeSynergies=3 | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `BATTLE_SNAPSHOT_SAMPLE` | sandboxResultDevOnly=True | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `BATTLE_SNAPSHOT_SAMPLE` | sandboxShouldWriteSave=False | `V0.4-BattleSnapshotAdapter01` |
| `Info` | `BATTLE_SNAPSHOT_SAMPLE` | sandboxShouldGrantReward=False | `V0.4-BattleSnapshotAdapter01` |
