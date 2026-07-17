# ItemSandbox Build Label Handtest Rework Report

Date: 2026-07-13

## Scope

- Updated build progress wording:
  - FaMen: `法门构筑：x/6`
  - QiLei: `符类构筑：x/4`, `印类构筑：x/4`, `令类构筑：x/4`, `镜类构筑：x/4`, `法类构筑：x/4`
  - Unknown QiLei fallback: `器类构筑：x/4`
- Added a six-item FaMen member list below the FaMen progress row.
- Did not change build skill descriptions or effect payload formatting.

## FaMen member coloring

- The list is built from all catalog items sharing the selected FaMen track.
- Items counted by the current Build result `SourceItemIds` are displayed in active green.
- Items not counted/lit are displayed in inactive grey.

## Static checks

- `构筑进度：` no longer appears in ItemSandbox build display/verifier source scope.
- `Build progress` remains only as a leak-guard token.
- Verifier text checks were updated to the new Chinese labels.

## Unity / protected file status

- Last successful Unity compile before the final verifier-token sync: `Tundra build success (0.04 seconds), 0 items updated, 129 evaluated`.
- A later minimal Unity batch compile attempt did not create a log and was manually stopped; it is not counted as validation.
- User accepted current hand-tested scene/prefab as new protected baseline: `USER_ACCEPT_BASELINE_ITEMSANDBOXV04BOARDFULLDETAILADAPTER01`.
- `ItemDetailPanel.prefab` hash remains `1FF92E482B1E4384ABFCDB614D10D25F52FDF4EEE4380A1FA3180E01D862A56D`.
- `EditorBuildSettings.asset` hash remains `08A277E3CA465A44E792318C0D3C210AFDBA61069F1170B74FA5A1A18598FE59`.
- `Scene_TalismanBag_V04_ItemSandbox.unity` hash is accepted as `975C59668C3FCA0B8AD826A0EE29316A7DF8A92400450E90F5ECEF1D37D23E43`.

## Final status

Code-side handtest request is implemented; package PASS remains gated on the verifier/regression rerun and protected hash mutation gate.
