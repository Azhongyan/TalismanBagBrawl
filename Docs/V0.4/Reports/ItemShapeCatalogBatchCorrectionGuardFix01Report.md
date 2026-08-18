# Item Shape Catalog Batch Correction GuardFix01 Report

- Package: `V0.4-ItemShapeCatalogBatchCorrection01-GuardFix01`
- Result: `PASS`
- User decision: `KEEP_CURRENT_UI_SCENE_PREFAB`
- Verification method: `TalismanBag.EditorTools.ItemShape.ItemShapeCatalogBatchCorrectionVerifier.VerifyStaticBatch`
- Batch log: `Logs/codex_item_shape_catalog_batch_correction01_guardfix01.log`
- Final marker: `ITEM_SHAPE_CATALOG_BATCH_CORRECTION01_PASS items=30 keep=14 change=16 unresolved=0`

## Approved baseline migration

| Scope | Count | Previous approved aggregate | New approved aggregate | Actual before batch | Actual after batch | Result |
|---|---:|---|---|---|---|---|
| Scene | 7 | `E8E0ABC43941ECB689EF47615C4A1B937180D704DC7F503819EEBC593004693C` | `F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B` | `F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B` | `F44C2A856E47CF77B33172E9ACA557CF1D7EB8CA731EB91F4BDFBBCAC86C746B` | PASS |
| Prefab | 5 | `DC689B674A701A62D4D999EA7DD26579E8B9964E0F15A9CF8B48DF1BC561187D` | `3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6` | `3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6` | `3D3807A74B4CC00EEAEB54335CBE9D40BB95CBF739D1D1855C1CE87A2E7DFBA6` | PASS |

Only the `scenes` and `prefabs` expected aggregate constants in `ItemShapeCatalogBatchCorrectionVerifier.cs` were replaced. A byte-level reverse replacement restored the exact pre-change source SHA256 `6782A87B17AF2570C5A1427778FE4ACA0DE3EE25EA81E102F32A2B6950F7F12A`, confirming that no other verifier bytes changed.

## Approved protected assets

| Asset | SHA256 before/after | LastWriteTimeUtc before/after | Result |
|---|---|---|---|
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_ItemSandbox.unity` | `A92D5755B2BE84973935805A2A3A02E4BD3A64A02CCE0F0B106A238E9C7FE7F1` | `2026-07-21T10:39:15.7380582Z` | UNCHANGED |
| `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` | `C6271C18F1F97CFD6BD401641C6402315F83B1938EF2601E4B84C86C59CFD450` | `2026-07-21T10:32:08.4689080Z` | UNCHANGED |
| `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/BattleLikePreviewAreaBridge.prefab` | `4DB1B94B619FDD73A948ADBC2762FBB567576BD66F9672713FCBEF990856D1BE` | `2026-07-21T10:32:14.1564111Z` | UNCHANGED |

The GuardFix did not edit, save, rebuild, or format any Scene or Prefab asset.

## Batch verification

- Unity exclusive precondition: `Unity process=0`; `Temp/UnityLockfile=absent`.
- Unity compile: `PASS`; `error CS=0`; Tundra build success.
- Matrix: `30/30 PASS`.
- KEEP / CHANGE / UNRESOLVED: `14 / 16 / 0`.
- Unexpected shape changes: `0`.
- I024: `shape_line2_h / (0,0);(1,0) / core(0,0)`.
- Rotation: `120/120 PASS`.
- Rarity inheritance: `150/150 PASS`.
- Dual seed inheritance: `300/300 PASS`.
- Candidate/Profile non-shape fields: `PASS`.
- Roll Canonical: `sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8`, `UNCHANGED`.
- Projection Canonical: `sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2`, `UNCHANGED`.
- Capability BP: `break=1000 / guard=800 / cooldown=900`.
- Item105 ledger: `11/11 PASS`.
- C02: `32/32 blocked / Actual Unknown reduction=0`.
- Enemy Runtime changed: `NO`.
- CrossSystem Runtime changed: `NO`.
- All protected scopes: `PASS`.
- Unity exclusive postcondition: `Unity process=0`; `Temp/UnityLockfile=absent`.

The assignment's displayed Roll Canonical value has 63 hexadecimal characters. The GuardFix did not modify that protected baseline; the verifier and regenerated reports retain the unchanged valid 64-character SHA256 shown above.

## Report refresh and diff audit

The batch refreshed only these five approved reports:

- `Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionReport.md`
- `Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionSpec.csv`
- `Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionChangeLedger.csv`
- `Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionCanonicalDelta.csv`
- `Docs/V0.4/Reports/ItemShapeCatalogBatchCorrectionLeakCheckReport.md`

This GuardFix report is the single newly added report. No Runtime, Item data, Scene, Prefab, BuildSettings, PNG/importer, Item Detail UI, matrix, or other protected-domain file was changed by GuardFix01.

The verifier source and this GuardFix report have no trailing whitespace. The regenerated main report retains one pre-existing generator-authored trailing space in its hand-test text; the assignment permits no generator-source change outside the two aggregate constants. Repository-wide `git diff --check` remains non-zero because the approved pre-existing Scene deltas contain Unity-serialized trailing spaces and two unrelated pre-existing reports contain a blank line at EOF; GuardFix01 did not alter those protected or unrelated bytes.
