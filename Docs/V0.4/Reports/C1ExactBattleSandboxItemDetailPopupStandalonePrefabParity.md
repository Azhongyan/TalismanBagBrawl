# C1 Exact BattleSandbox Item Detail Popup Standalone Prefab Parity

- Result: `PASS`
- Source: `Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity`
- Source path: `BuildSandboxPreviewCanvas/MobileSafeAreaRoot/SafeAreaRoot/PopupLayer/ItemDetailPanel`
- Output: `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemDetailPopupStandalone.prefab`
- Source Scene SHA-256: `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA`
- Output Prefab SHA-256: `F3164CE168A2167065EA3F695995E092C8BA137676B5AFB8F26A756B05E222A1`

## Retained carrier

```text
C1ExactBattleSandboxItemDetailPopupStandalone
└─ MobileSafeAreaRoot
   └─ SafeAreaRoot
      └─ PopupLayer
         └─ ItemDetailPanel
            ├─ CloseButton
            ├─ ItemDetailHeader
            ├─ ItemDetailFixedBaseStatsRoot
            ├─ ItemDetailScrollView
            └─ ItemDebugScrollView
```

## Serialized parity

| Category | Source | Standalone | Result |
|---|---:|---:|---|
| Retained objects | 295 | 295 | PASS |
| ItemDetailPanel objects | 291 | 291 | PASS |
| Components | 1013 | 1013 | PASS |
| Images | 157 | 157 | PASS |
| Legacy Text | 80 | 80 | PASS |
| TMP Text | 0 | 0 | PASS |
| ScrollRect | 2 | 2 | PASS |
| Mask / RectMask2D | 2 | 2 | PASS |
| Selectable / Button / Scrollbar | 3 | 3 | PASS |

Exact PASS signatures:

- descendant authored RectTransform geometry: `3140F9FF8DCA1C0061C62D72E809E61AA40DFFF4E4465A164A279C4762D35E83`
- hierarchy/active/sibling order: `75E5260F73B46976754EFCED98DAE8CFC9A57CF4F280506D691E382028520CF0`
- component type/order: `8942F768BA0D0B3C7AB96B80ADAFA24E2FA7EE0E9F033B9FE9960F4F60BC1FCA`
- Canvas/CanvasScaler/GraphicRaycaster/Popup behavior: `75727D9480C00B969588EDDC3F44DA6101392F83B2ED726EEF2B76633BCD8EE7`
- static Image sprite/material/type/color/raycast/maskable: `8B793DCC7541E3063D88C1AA2F5F1FC5AFA32EBA318718D76326C8DAE2557EEB`
- data-image style excluding intentionally cleared sprite: `81DD04285118C7EA030513BCB854910C0ECC0F74466E8569FB1A8C3665E64001`
- static Text/TMP font/material/size/alignment/color/overflow: `49DC87E0E88964459BF82D87363F0030C1985FE265488C892481382AEF9D9155` / `E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855`
- data-text style excluding intentionally cleared string: `F48A19AF2BE57605A859F3D8576C96ED7B12B118087C1ACB35EB506D509144D6`
- ScrollRect: `CECDD527DC888CADE2EE93878116021D298FE73CAE1B5D326273BDB072CB54BB`
- Mask/RectMask2D: `5F95FD79DA49318799340E02E4BD1C3CD9EAD3B1B8B42A7346B7ADEAE0EF4211`
- Selectable/Button/Scrollbar: `91AF2EE20D006EFF5DAF7FA40AD0CD583AA0510E9A65F7CC0DB79925187A7BD8`

## Intentional differences

- Root renamed from `BuildSandboxPreviewCanvas` to `C1ExactBattleSandboxItemDetailPopupStandalone`.
- Only the minimum Canvas → MobileSafeAreaRoot → SafeAreaRoot → PopupLayer → ItemDetailPanel chain is retained.
- The context-driven root Canvas numeric RectTransform is excluded from geometry parity; its unique RectTransform/Canvas/CanvasScaler/GraphicRaycaster contract, component order and exact Canvas properties remain validated. Every descendant authored RectTransform remains in strict numeric parity.
- Removed source siblings `EnemyCombatFeedbackFloatingRoot` and `BuildSandboxItemInfoPanel_Runtime` with all other non-retained Canvas children.
- Removed runtime/Sandbox owners: `TalismanBag.ItemSandbox.ItemSandboxDetailPanelView`, `TalismanBag.ItemSandbox.ItemSandboxDetailSectionView`.
- Cleared selected-item/sample/debug projection without changing active states or styling: text=46, sprite=54.
- Cleared external Scene/object references: 0.
- Removed external/null persistent UnityEvent listeners: 0.
- Retained MobileSafeAreaFitter; its target is local to the standalone Prefab.
- Existing `ItemDetailPanelView` and project assets remain referenced; no `ItemSandboxDetailPanelView`, runtime truth, fixture or integration binding is carried.

## Standalone Prefab Stage inspection

- `PREFAB_STAGE_OPEN_PASS / NORMAL_EDITOR_VIEW_AVAILABLE`
- This package does not claim Unified mounting, live Item projection, navigation, Fresh Play or user visual acceptance.

## Protected boundaries

- BattleSandbox source Scene remains frozen at SHA-256 `1D7A367B803F9A7477E261A90E05D58FBAB2F9FAA1F3464A86806B8A9FAF9FCA`.
- External protected carriers were captured before authoring and remained byte-identical through every authoring phase:
  - `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemBoard.prefab` operation-start SHA-256 `46949A5FECAAF94960D2F2A31C6715F0C1297CDAEAD1C71FF749959762FBB7A8` — unchanged by this authorer.
  - `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemCard.prefab` operation-start SHA-256 `C4D449A1C298BF5B2A91072A4BEA2A67AFD5E1B1D713A4D769CB843D059DDD8C` — unchanged by this authorer.
  - `Assets/_Game/Prefabs/TalismanBag/Items/C1ExactBattleSandboxItemTray.prefab` operation-start SHA-256 `6FA5F995433B4433B201613CA7FBA939779F6664E44E19CC02ACA060E4F4D552` — unchanged by this authorer.
  - `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab` operation-start SHA-256 `2A6924DA0823B74E2639B593926D8C2F8422101B01EDBE4073AF33CD424E027C` — unchanged by this authorer.
  - `Assets/_Game/Prefabs/TalismanBag/UnifiedBattle/UnifiedBattlePageShell.prefab` operation-start SHA-256 `69E34855B9146DA50F941CF19F389FEEB005ED35861D1011C30293D6BC147090` — unchanged by this authorer.
  - `Assets/_Game/Scenes/Scene_TalismanBag_V04_UnifiedBattlePageShell.unity` operation-start SHA-256 `D432B6E492EDF0CB0EEA8B62E66E27A6EE3F7773C8BBCC8AD886A86A1A0BDD63` — unchanged by this authorer.
  - `Assets/_Game/Scripts/TalismanBag/Editor/UnifiedBattle/C1ExactBattleSandboxItemDetailPhaseBAuthoring.cs` operation-start SHA-256 `EE637E23526A0F1E492536199568C3AFBBE4326C9DD7D9C72E4E6DB4860F3431` — unchanged by this authorer.
  - `Assets/_Game/Scripts/TalismanBag/UnifiedBattle/Presentation/ExactItemDetail/C1ExactBattleSandboxItemDetailPresenter.cs` operation-start SHA-256 `4942A8543BAF064CAEB440BE15757B2241A400982277CA73A8869B95C30CD4BE` — unchanged by this authorer.
