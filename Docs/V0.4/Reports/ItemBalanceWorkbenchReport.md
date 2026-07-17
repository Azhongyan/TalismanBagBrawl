# Item Balance Workbench GuardFix01 Report

- result: PASS
- marker: ITEM_BALANCE_WORKBENCH_150_CANDIDATE_SEED01_GUARDFIX01_PASS
- Spec: 1286 PASS / 0 FAIL / 1286 TOTAL
- profiles: 30/30
- versions: 150/150
- stat ranges: 600/600
- rolls: 150/150
- projections: 150/150
- deterministic: 150/150
- data maturity: BALANCE_CANDIDATE
- I031 profile count: 0
- CSV/Undo fixture: in-memory ScriptableObject clones only; no candidate asset was saved.

## Current-invocation prerequisite regressions

| suite | expected | actual | invokedAtUtc | result |
|---|---:|---:|---|---|
| ItemInnerDataCatalog | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:27.1163829Z | PASS |
| ItemSystemValidatorAndSnapshot | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:27.1974371Z | PASS |
| BuildSynergyCore | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:27.3161070Z | PASS |
| CoreAwakeningPreview | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:27.3276413Z | PASS |
| ItemSkillTriggerContract | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:27.3386711Z | PASS |
| ItemDetailProjectionComplete | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:27.4226650Z | PASS |
| JuNian Lighting | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:28.0483133Z | PASS |
| ArrayBonus | PASS from current invocation | marker=True, fresh=True | 2026-07-12T04:31:28.1372200Z | PASS |
| Foundation | 159/159 | 159/159 | 2026-07-12T04:31:28.1482660Z | PASS |
| StatRange | 39/39 | 39/39 | 2026-07-12T04:31:29.1944340Z | PASS |
| CorePotential | 69/69 | 69/69 | 2026-07-12T04:31:30.2940201Z | PASS |
| AffixSchema | 88/88 | 88/88 | 2026-07-12T04:31:32.3574631Z | PASS |
| RollEngine | 72/72 | 72/72 | 2026-07-12T04:31:36.4820240Z | PASS |
| DropSandbox | 87/87 + Determinism 6/6 | 87/87 + Determinism 6/6 | 2026-07-12T04:31:41.0755549Z | PASS |
| Simulation | 65/65 + Distribution 46/46 + Determinism 12/12 + violations 0 | 65/65 + Distribution 46/46 + Determinism 12/12 + violations 0 | 2026-07-12T04:31:41.1778236Z | PASS |
| ProjectionContract | 156/156 | 156/156 | 2026-07-12T04:32:17.3998983Z | PASS |

## Protected boundary hashes

| category | scanRoot | before | after | result |
|---|---|---|---|---|
| candidate_assets | Assets/_Game/Configs/ItemBalanceWorkbench | `7228D21FB30FC868FF4EDA234E556B82FCDEB9B6C8D1D3CB7BCC753CC2C874E7` | `7228D21FB30FC868FF4EDA234E556B82FCDEB9B6C8D1D3CB7BCC753CC2C874E7` | PASS |
| workbench_function_files | Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | `BCBE5425790A3309AD0218D48749571762B76B5212B7C09C6097EB5B744BF1D2` | `BCBE5425790A3309AD0218D48749571762B76B5212B7C09C6097EB5B744BF1D2` | PASS |
| balance_runtime_and_workbench_schema | Assets/_Game/Scripts/TalismanBag/Items/Balance | `288F44DA969C17762C3BAD36996906E73947A649A477715F4E24B962863B865C` | `288F44DA969C17762C3BAD36996906E73947A649A477715F4E24B962863B865C` | PASS |
| protected_item_runtime_and_schema | Assets/_Game/Scripts/TalismanBag/Items | `7B6892650BDC8AFC1A2A66F1036753D46C34FA2A852BE44DE55F17D8DECD6C59` | `7B6892650BDC8AFC1A2A66F1036753D46C34FA2A852BE44DE55F17D8DECD6C59` | PASS |
| scene_files | Assets/_Game/Scenes | `5DAEDEF6603F4623FB2C8AE3F9D0DFA88E3154153F572063F4BDDBE3745C3BE1` | `5DAEDEF6603F4623FB2C8AE3F9D0DFA88E3154153F572063F4BDDBE3745C3BE1` | PASS |
| prefab_files | Assets/_Game/Prefabs | `3B38B60DC7203C1B851B4C83CDD4B809803916EA1483D2647151A8ED89C9DB01` | `3B38B60DC7203C1B851B4C83CDD4B809803916EA1483D2647151A8ED89C9DB01` | PASS |
| build_settings | ProjectSettings/EditorBuildSettings.asset | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | PASS |

## Warnings

- AFFIX_DURATION_BRIDGE: affix_duration_up is a candidate-only definition required by duration secondary-stat pools.

## Failures

- None
