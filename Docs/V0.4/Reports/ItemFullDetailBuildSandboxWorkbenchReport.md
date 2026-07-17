# ItemFullDetailBuildSandboxWorkbench01 Report

- Result: PASS
- Marker: `ITEM_FULL_DETAIL_BUILD_SANDBOX_WORKBENCH01_PASS`
- Guard receipt target: `GUARD_PASS_ITEMFULLDETAILBUILDSANDBOXWORKBENCH01`
- Spec: 228/228 PASS
- Candidates: 150/150
- Stat ranges: 600/600
- Instance projections: 150/150
- Full details: 150/150
- Determinism: 150/150
- I031 ordinary pool entries: 0

## Editable Build validation layout

- Source: fixed `I031` at `(0,0)`.
- Ordinary instances: six distinct real `I001-I006@orange` projections at anchors `(1,0) (2,0) (4,0) (2,1) (4,2) (2,3)`.
- Seeds: `610000 | 610000 | 610001 | 610000 | 610000 | 610000`.
- Result: real generated FaMenOnly/Dual qualification, direct + relay lighting, array activation, editable placement records, Build2/4/6 PASS.

## Main Build and monitors

- Default is None; only `UserClick` creates an explicit selection.
- Selection does not write into or change qualified Build counts.
- BasicAttack / Build2 / Build4 / Build6 fixed-order monitor slots all follow the selected FaMen Build and execute no skill.

## Core effect layers

- Candidate eligible/visible IDs remain projection facts.
- Sandbox Lv1-Lv40 produces preview-only unlock input; active requires visible + unlocked + lit.
- No formal cultivation, save, battle effect, or asset write is connected.

## UI and layout

- Runtime layout hard-code scan: PASS when this report is PASS.
- 1080x1920 and 720x1280 anchor-column checks: PASS when this report is PASS.
- Runtime initialization preserves serialized RectTransform state; repeated rows use twelve serialized layout-controlled slots.
- Existing Runtime ItemDetailPanel prefab is reused and was not modified.

## Protected hashes

| path | before | after | result |
|---|---|---|---|
| `Assets/_Game/Configs/ItemBalanceWorkbench` | `F941C253288D9D0D77AC25EB50716BA76F49E293DD58EA921927798102301915` | `F941C253288D9D0D77AC25EB50716BA76F49E293DD58EA921927798102301915` | PASS |
| `Assets/_Game/Scripts/TalismanBag/Items/Generation` | `5434BD4C7422681FBAC775ACA2C496B4C8A9EC4B772F1BFB096366533BB527B8` | `5434BD4C7422681FBAC775ACA2C496B4C8A9EC4B772F1BFB096366533BB527B8` | PASS |
| `Assets/_Game/Prefabs` | `372B1E7DB1C94BA48E412D7946686556D640D414EAB77E339C32FA3E88044B5D` | `372B1E7DB1C94BA48E412D7946686556D640D414EAB77E339C32FA3E88044B5D` | PASS |
| `ProjectSettings/EditorBuildSettings.asset` | `1079B5D8CEC522B3825F7E55C79FC972C7DA1D0F024FAF7885CBFD92F94EE9D0` | `1079B5D8CEC522B3825F7E55C79FC972C7DA1D0F024FAF7885CBFD92F94EE9D0` | PASS |
| `Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs` | `E517B783D839941488F73BFDB23E18B78DF3A8F81FB342622FF1056EDFA58342` | `E517B783D839941488F73BFDB23E18B78DF3A8F81FB342622FF1056EDFA58342` | PASS |

## Regression evidence

Existing Item System and eight Item Algorithm verifier reports were checked for PASS markers; dedicated batch regressions are recorded in the final run log set.

## Batch commands and logs

- Builder: `Unity.exe -batchmode -nographics -projectPath F:\Porject\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemFullDetailBuildSandboxWorkbenchSceneBuilder.ApplyBatch -quit`
- Verifier: `Unity.exe -batchmode -nographics -projectPath F:\Porject\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemFullDetailBuildSandboxWorkbenchVerifier.VerifyBatch -quit`
- Logs: `Logs/codex_item_full_detail_workbench_builder.log`, `Logs/codex_item_full_detail_workbench_verify.log`, `Logs/codex_item_full_detail_workbench_regression.log`.

## Errors

- None.
