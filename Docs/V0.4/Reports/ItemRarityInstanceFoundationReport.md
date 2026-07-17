# ItemRarityInstanceFoundation01 Report

- Package: `V0.4-ItemRarityInstanceFoundation01`
- Run time (UTC): `2026-07-16T08:55:14.0783696Z`
- Verification: PASS
- Ordinary archetypes: 30
- System items: 1
- Rarity stable keys: white / green / blue / purple / orange
- Derived rarity versions: 150
- I031 exclusion: excluded from ordinary generation and retained as directed core-progression identity
- Instance identity fields: baseItemId / cultivationPotentialProfileId / generationVersion / itemInstanceId / rarity / rootSeed / schemaId
- ItemSystemSnapshot.v1 compatibility: PASS; existing itemId/placementId schema and Canonical Signature implementation were not modified.

## Code Survey Conclusion

- `ItemCatalogRarity` (`bai/qing/lan/zi/cheng`) remains legacy Catalog preview background. The new `ItemInstanceRarity` is isolated and accepts only `white/green/blue/purple/orange`.
- `rarityDefault`, `allowedRarities`, display rarity, and affix preview fields do not determine ordinary generation eligibility or prototype-fixed quality.
- I031 still carries legacy rarity/affix preview fields in the untouched Catalog, but explicit identity classification excludes it from ordinary generation.
- Existing `ItemSystemSnapshot.v1` keeps `itemId` as Catalog identity and `placementId` as board placement identity. This package adds a separate `baseItemId` / `itemInstanceId` snapshot without `placementId`.
- The new foundation owns a separate Canonical Signature and never appends to or rewrites `ItemSystemSnapshot.BuildDebugSignature()`.

## Historical Regressions

- ItemInnerDataCatalog verifier: PASS
- ItemSystemValidatorAndSnapshot verifier: PASS
- BuildSynergyCore verifier: PASS
- CoreAwakeningPreview verifier: PASS
- ItemSkillTriggerContract verifier: PASS
- ItemDetailProjectionComplete verifier: PASS
- JuNian Lighting verifier: PASS
- ArrayBonus verifier: PASS

## Errors

- None

## Warnings

- None
