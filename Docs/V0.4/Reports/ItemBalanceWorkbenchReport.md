# Item Balance Workbench GuardFix01 Report

- result: FAIL
- marker: NOT_PRINTED
- Spec: 1283 PASS / 3 FAIL / 1286 TOTAL
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
| ItemInnerDataCatalog | PASS from current invocation | marker=True, fresh=True | 2026-07-21T07:23:42.8660598Z | PASS |
| ItemSystemValidatorAndSnapshot | PASS from current invocation | marker=True, fresh=True | 2026-07-21T07:23:43.4040050Z | PASS |
| BuildSynergyCore | PASS from current invocation | marker=True, fresh=True | 2026-07-21T07:23:43.5776693Z | PASS |
| CoreAwakeningPreview | PASS from current invocation | marker=True, fresh=True | 2026-07-21T07:23:43.6038809Z | PASS |
| ItemSkillTriggerContract | PASS from current invocation | marker=True, fresh=True | 2026-07-21T07:23:43.6379180Z | PASS |
| ItemDetailProjectionComplete | PASS from current invocation | marker=False, fresh=True | 2026-07-21T07:23:43.7707946Z | FAIL |
| JuNian Lighting | PASS from current invocation | marker=True, fresh=True | 2026-07-21T07:23:44.2000244Z | PASS |
| ArrayBonus | PASS from current invocation | marker=True, fresh=True | 2026-07-21T07:23:44.3150436Z | PASS |
| Foundation | 159/159 | 159/159 | 2026-07-21T07:23:44.3441350Z | PASS |
| StatRange | 39/39 | 39/39 | 2026-07-21T07:23:44.9916963Z | PASS |
| CorePotential | 69/69 | 69/69 | 2026-07-21T07:23:45.6968577Z | PASS |
| AffixSchema | 88/88 | 88/88 | 2026-07-21T07:23:47.0810678Z | PASS |
| RollEngine | 72/72 | 72/72 | 2026-07-21T07:23:49.8744424Z | PASS |
| DropSandbox | 87/87 + Determinism 6/6 | 87/87 + Determinism 6/6 | 2026-07-21T07:23:52.7849912Z | PASS |
| Simulation | 65/65 + Distribution 46/46 + Determinism 12/12 + violations 0 | 65/65 + Distribution 46/46 + Determinism 12/12 + violations 0 | 2026-07-21T07:23:52.8825725Z | PASS |
| ProjectionContract | 156/156 | 155/156 | 2026-07-21T07:24:20.4131845Z | FAIL |

## Protected boundary hashes

| category | scanRoot | before | after | result |
|---|---|---|---|---|
| candidate_assets | Assets/_Game/Configs/ItemBalanceWorkbench | `2B4F6D574DAFF01FCD893A3C090BCBA8DECF92547EBAE2112ED7C8172352DAA0` | `2B4F6D574DAFF01FCD893A3C090BCBA8DECF92547EBAE2112ED7C8172352DAA0` | PASS |
| workbench_function_files | Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance | `40E8103EE34A33F335FC883EDDB9E3BF42B5A08ECC2EDE2154477ADF956D53B5` | `40E8103EE34A33F335FC883EDDB9E3BF42B5A08ECC2EDE2154477ADF956D53B5` | PASS |
| balance_runtime_and_workbench_schema | Assets/_Game/Scripts/TalismanBag/Items/Balance | `12BC82F037BCB0BD8025662DAA04F13551CEF962A1FBD27B6218DD515358559E` | `12BC82F037BCB0BD8025662DAA04F13551CEF962A1FBD27B6218DD515358559E` | PASS |
| protected_item_runtime_and_schema | Assets/_Game/Scripts/TalismanBag/Items | `805FAC45FA8C43A9789B325C9C563277526915BDE35556FD92B58FB272A98DEE` | `805FAC45FA8C43A9789B325C9C563277526915BDE35556FD92B58FB272A98DEE` | PASS |
| scene_files | Assets/_Game/Scenes | `9E91B90DF023B90D66AD30B1C066C2135B0931805AC22ADF36EDF84936DDFBB2` | `9E91B90DF023B90D66AD30B1C066C2135B0931805AC22ADF36EDF84936DDFBB2` | PASS |
| prefab_files | Assets/_Game/Prefabs | `85650BBB8ED3D23D8FB14433C22B97811C4AA66297C6AF2D9426E8756A58D302` | `85650BBB8ED3D23D8FB14433C22B97811C4AA66297C6AF2D9426E8756A58D302` | PASS |
| build_settings | ProjectSettings/EditorBuildSettings.asset | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | PASS |

## Warnings

- AFFIX_DURATION_BRIDGE: affix_duration_up is a candidate-only definition required by duration secondary-stat pools.

## Failures

- regression.itemdetailprojectioncomplete: expected=PASS from current invocation, actual=marker=False, fresh=True
- regression.projectioncontract: expected=156/156, actual=155/156
- leak.buildsettings: expected=0, actual=1
