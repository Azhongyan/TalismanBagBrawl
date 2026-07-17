# Item Balance Candidate Detail Sandbox Adapter Report

- Package: `V0.4-ItemBalanceCandidateDetailSandboxAdapter01`
- Result: PASS
- Marker: `ITEM_BALANCE_CANDIDATE_DETAIL_SANDBOX_ADAPTER01_PASS`
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
| 30 candidate profiles | `699753758E2241DA97EC4A27C7AEE1C906EB74E380C0F7911376E289D07C630E` | `699753758E2241DA97EC4A27C7AEE1C906EB74E380C0F7911376E289D07C630E` | PASS |
| workbench catalog | `E0362AB77E338117D0D238DB4DFA0ED56C7C5C80333774E2FF2DED2455BF6230` | `E0362AB77E338117D0D238DB4DFA0ED56C7C5C80333774E2FF2DED2455BF6230` | PASS |
| runtime/schema | `1578CC150361B43E1128D274E74A2110996D93DAE82371F5361330A73A37273D` | `1578CC150361B43E1128D274E74A2110996D93DAE82371F5361330A73A37273D` | PASS |
| prefabs | `3B38B60DC7203C1B851B4C83CDD4B809803916EA1483D2647151A8ED89C9DB01` | `3B38B60DC7203C1B851B4C83CDD4B809803916EA1483D2647151A8ED89C9DB01` | PASS |
| scenes outside ItemSandbox | `DAECF8CBAE64C66F05EE8007B6F425AEA94480594FBA491434E1A3706B181909` | `DAECF8CBAE64C66F05EE8007B6F425AEA94480594FBA491434E1A3706B181909` | PASS |
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

- None
