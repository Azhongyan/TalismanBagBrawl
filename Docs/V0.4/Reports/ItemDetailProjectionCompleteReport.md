# ItemDetailProjectionComplete01 Rework Report

Result: FAIL

## Runtime Assets
- Runtime prefab: `Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab`
- Runtime view: `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs`
- Runtime section view: `Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailSectionView.cs`

## Context Rules
- CatalogPreview ignores placed snapshots and shows header states `尚未入阵/阵脉未计算`.
- PlacedInstance requires an explicit placementId and queries snapshots strictly by placementId.
- Stale placement `P_I001_STALE` shows `实例状态不可用` and records missing snapshot diagnostics.
- No `FindItemResult(itemId)` fallback is allowed in the composer or Sandbox projection path.

## Regression Checks
- projection-context-mode: PASS: context source and composed state present
- section-count-order: PASS: 15/3
- duplicateProjectionCheck: PASS: same-signature
- player-debug-separation: PASS: debugFields=25
- strict-placement-query: PASS: I001 lit / I007 unlit
- stale-placement-no-item-fallback: PASS: 实例状态不可用
- catalog-preview-no-placement-state: PASS: 尚未入阵/阵脉未计算
- catalog-preview-junian-no-placed-source: PASS: no P_SOURCE
- placed-junian-requires-placement: PASS: P_SOURCE lit source
- snapshot-missing-placement-no-throw: PASS: 状态不可用/未接入
- runtime-view-namespace: PASS: TalismanBag.Items.Detail.UI
- reusable-prefab-exists: PASS: Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab
- prefab-viewmodel-only: PASS: runtime-only
- prefab-visual-contract: PASS: visual-contract
- meta-text-contract: PASS: 良品 + 玄水法 / 符 + 竖排两格
- power-text-contract: PASS: 321 -> 987 -> -- (dynamic rarity color)
- typography-color-contract: PASS: #87B66A/#D8CCB7/#B99A61; font sizes unlocked
- five-rarity-palette-contract: PASS: #E3D8C3/#87B66A/#68A9E6/#B27DDF/#E4A14B
- rich-field-style-contract: PASS: stats/status/core/build tags applied
- sandbox-popup-open-close-contract: PASS: popup-contract
- sandbox-reuses-runtime-view: PASS: adapter
- catalog-all-31-I001: PASS: PASS
- catalog-all-31-I002: PASS: PASS
- catalog-all-31-I003: PASS: PASS
- catalog-all-31-I004: PASS: PASS
- catalog-all-31-I005: PASS: PASS
- catalog-all-31-I006: PASS: PASS
- catalog-all-31-I007: PASS: PASS
- catalog-all-31-I008: PASS: PASS
- catalog-all-31-I009: PASS: PASS
- catalog-all-31-I010: PASS: PASS
- catalog-all-31-I011: PASS: PASS
- catalog-all-31-I012: PASS: PASS
- catalog-all-31-I013: PASS: PASS
- catalog-all-31-I014: PASS: PASS
- catalog-all-31-I015: PASS: PASS
- catalog-all-31-I016: PASS: PASS
- catalog-all-31-I017: PASS: PASS
- catalog-all-31-I018: PASS: PASS
- catalog-all-31-I019: PASS: PASS
- catalog-all-31-I020: PASS: PASS
- catalog-all-31-I021: PASS: PASS
- catalog-all-31-I022: PASS: PASS
- catalog-all-31-I023: PASS: PASS
- catalog-all-31-I024: PASS: PASS
- catalog-all-31-I025: PASS: PASS
- catalog-all-31-I026: PASS: PASS
- catalog-all-31-I027: PASS: PASS
- catalog-all-31-I028: PASS: PASS
- catalog-all-31-I029: PASS: PASS
- catalog-all-31-I030: PASS: PASS
- catalog-all-31-I031: PASS: PASS
- catalog-all-31: PASS: count=31; I031=True
- layout-1080x1920: FAIL: 1080x1920; detail=viewport=1557x840; content=1557x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1557x840; content=1557x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- manual-geometry-layout-1080x1920: PASS: 1080x1920; detail=viewport=1557x840; content=1557x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1557x840; content=1557x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- scroll-reset-layout-1080x1920: PASS: 1080x1920; detail=viewport=1557x840; content=1557x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1557x840; content=1557x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- long-text-scroll-layout-1080x1920: PASS: 1080x1920; detail=viewport=1557x840; content=1557x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1557x840; content=1557x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- layout-720x1280: FAIL: 720x1280; detail=viewport=1003x424; content=1003x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1003x424; content=1003x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- manual-geometry-layout-720x1280: PASS: 720x1280; detail=viewport=1003x424; content=1003x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1003x424; content=1003x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- scroll-reset-layout-720x1280: PASS: 720x1280; detail=viewport=1003x424; content=1003x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1003x424; content=1003x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- long-text-scroll-layout-720x1280: PASS: 720x1280; detail=viewport=1003x424; content=1003x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1003x424; content=1003x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- scope-leak-check: PASS: clean
- regression-ItemSandboxDetailUiReport: PASS: Docs/V0.4/Reports/ItemSandboxDetailUiReport.md
- regression-ItemInnerDataCatalogReport: PASS: Docs/V0.4/Reports/ItemInnerDataCatalogReport.md
- regression-ItemGridPlacementAndEyeRuleReport: PASS: Docs/V0.4/Reports/ItemGridPlacementAndEyeRuleReport.md
- regression-JuNianLightingAndAdjacentRelayReport: PASS: Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md
- regression-ArrayBonusCellResolverReport: PASS: Docs/V0.4/Reports/ArrayBonusCellResolverReport.md
- regression-BuildSynergyCoreReport: PASS: Docs/V0.4/Reports/BuildSynergyCoreReport.md
- regression-CoreAwakeningPreviewReport: PASS: Docs/V0.4/Reports/CoreAwakeningPreviewReport.md
- regression-ItemSkillTriggerContractReport: PASS: Docs/V0.4/Reports/ItemSkillTriggerContractReport.md

## Notes
- Runtime prefab path: Assets/_Game/Prefabs/TalismanBag/Items/ItemDetailPanel.prefab
- Runtime view path: Assets/_Game/Scripts/TalismanBag/Items/Detail/UI/ItemDetailPanelView.cs
- Catalog all-31 rows written to ItemDetailProjectionCompleteSpec.csv.
- Visual contract: old-paper dark-gold card, rarity badge, three state badges, artwork/image slot, top-right close button, five player chapters plus one dev chapter, per-section accents, and dual vertical scrollbars.
- Long text layout: 1080x1920; detail=viewport=1557x840; content=1557x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1557x840; content=1557x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True; 720x1280; detail=viewport=1003x424; content=1003x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1003x424; content=1003x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True.

## Errors
- Layout check failed for layout-1080x1920: 1080x1920; detail=viewport=1557x840; content=1557x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1557x840; content=1557x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
- Layout check failed for layout-720x1280: 720x1280; detail=viewport=1003x424; content=1003x1902; scrollbar=optional-present; autoLayout=present; debug=viewport=1003x424; content=1003x446; scrollbar=optional-present; autoLayout=present; sections=12/14 active (15/15 bound),3/3; manualGeometryPositive=True; headerColumnsAdvisory=False; reset=True; longScroll=True
