# Item Balance Candidate Detail Sandbox Adapter Report

- Package: `V0.4-ItemBalanceCandidateDetailSandboxAdapter01`
- Result: FAIL
- Marker: `NOT_EMITTED`
- Base items: 30/30
- Candidate combinations: 150/150
- Read-only projections: 150/150
- ItemDetailViewModels: 150/150
- Candidate stat ranges: 600/600
- Determinism: 150/150
- Explicit seed switches: 150/150
- I031 ordinary generation: 0
- Regression reports: 15/15 PASS
- Data maturity: `BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED`
- Sandbox Item Detail connected: YES
- Formal Item Detail connected: NO
- Battle connected: NO
- Reward/Save connected: NO

## Protected boundary hashes

| category | before | after | result |
|---|---|---|---|
| 30 candidate profiles | `8DF5C186DF9FA61DE9E0854D6C4CCCAECF49E5C63900E45ACE98A5299D89A636` | `8DF5C186DF9FA61DE9E0854D6C4CCCAECF49E5C63900E45ACE98A5299D89A636` | PASS |
| workbench catalog | `683FE25CFB12BC863C510B52024C23AC5D01F6BB611F8B5DA025F807DBB1317B` | `683FE25CFB12BC863C510B52024C23AC5D01F6BB611F8B5DA025F807DBB1317B` | PASS |
| runtime/schema | `AEEEA9B9B4608B62DB16C9B7A9A836C5CADEA589C9F755D881B7B8AFD6F3208E` | `AEEEA9B9B4608B62DB16C9B7A9A836C5CADEA589C9F755D881B7B8AFD6F3208E` | PASS |
| prefabs | `85650BBB8ED3D23D8FB14433C22B97811C4AA66297C6AF2D9426E8756A58D302` | `85650BBB8ED3D23D8FB14433C22B97811C4AA66297C6AF2D9426E8756A58D302` | PASS |
| scenes outside ItemSandbox | `4859EF768FA8153F3C0E96C941EEA1F135B2B156803F55775AEE911D32A5C00A` | `4859EF768FA8153F3C0E96C941EEA1F135B2B156803F55775AEE911D32A5C00A` | PASS |
| BuildSettings | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | `472269AA0CD12A093098703ACF5D12BC7B1FD901975E62F46B37727A62B6FAA3` | PASS |

## Unity batch

- Scene authoring command: `Unity.exe -batchmode -nographics -quit -projectPath F:\Porject\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemSandboxDetailUiSceneBuilder.ApplyItemBalanceCandidateDetailSandboxLayoutBatch`
- Scene authoring log: `Logs/codex_item_balance_candidate_detail_sandbox_builder.log`
- Verifier command: `Unity.exe -batchmode -nographics -quit -projectPath F:\Porject\TalismanBagBrawl -executeMethod TalismanBag.EditorTools.ItemSandbox.ItemBalanceCandidateDetailSandboxAdapterVerifier.VerifyBatch`
- Verifier log: `Logs/codex_item_balance_candidate_detail_sandbox_verify.log`

## User hand test

1. Open `Scene_TalismanBag_V04_ItemSandbox.unity` and enter Candidate Instance Preview.
2. Select I001 and switch white / green / blue / purple / orange.
3. Keep Seed fixed; reopen detail tabs and confirm identity and rolls do not change.
4. Click Regenerate and confirm Seed plus legal rolls change.
5. Switch items and confirm detail identity, ranges, affixes, core visibility and BuildQualification do not cross.
6. Confirm no lit/array/awakening/battle/build-count/skill-trigger state is fabricated.
7. Return to Catalog Preview and confirm 31 catalog entries including I031 remain available.
8. Confirm no Battle, Reward, Inventory, SaveData or formal drop flow is entered.

## Errors

- layout-1080x1920: expected no overlap/text overflow; long scroll; item switch resets top, actual detail=974x403; content=2028; debug=974x403; reset=True
- layout-720x1280: expected no overlap/text overflow; long scroll; item switch resets top, actual detail=614x133; content=2028; debug=614x133; reset=True
