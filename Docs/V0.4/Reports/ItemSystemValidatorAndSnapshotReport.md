# ItemSystemValidatorAndSnapshot01 Report

Result: PASS
Spec rows: 57
Passed rows: 57
Failed rows: 0

## Snapshot v1 Contract
- schemaVersion is ItemSystemSnapshot.v1.
- Provider input is cloned; generated snapshots expose read-only collections and deterministic ordering.
- Snapshot covers board, eye cell, AP cells, catalog, placements, lighting, array bonus, Build, awakening, skill monitor, selected main Build, and validation errors.
- Detail view models remain projections; they are not used as Item System fact sources.

## Notes
- ItemSystemSnapshot v1 provider returns immutable read-only snapshots for catalog, placement, lighting, array bonus, Build, awakening, and skill monitor facts.
- Invalid provider input returns stable validation error codes instead of throwing unhandled exceptions.
- Sandbox preview now reads scenario facts from ItemSystemSnapshot and only projects them back to existing greybox views.

## Covered Case IDs
- standard-valid-layout: PASS
- deterministic-same-input-twice: PASS
- snapshot-immutable-after-input-mutation: PASS
- deterministic-reversed-input: PASS
- deterministic-invalid-input: PASS
- canonical-signature-covers-all-branches: PASS
- duplicate-placement-id: PASS
- empty-placement-id: PASS
- out-of-bounds: PASS
- placement-overlap: PASS
- eye-covered: PASS
- array-bonus-coordinates-wrong: PASS
- missing-junian: PASS
- multiple-junian: PASS
- ordinary-item-forged-source: PASS
- main-build-non-explicit-auto: PASS
- qilei-selected-as-main-build: PASS
- skill-monitor-fixed-order: PASS
- duplicate-base-item-excluded-from-build: PASS
- unlit-item-counted-in-build: PASS
- unlit-item-array-bonus-active: PASS
- unopened-core-effect-active: PASS
- opened-but-unlit-core-effect-active: PASS
- build-snapshot-auto-selected-main: PASS
- skill-monitor-slot-count-order-wrong: PASS
- skill-monitor-runtime-state-forbidden: PASS
- external-top-level-collection-mutation-blocked: PASS
- external-nested-collection-mutation-blocked: PASS
- forged-placement-overlap: PASS
- forged-catalog-duplicate-id: PASS
- forged-ordinary-lighting-source: PASS
- forged-invalid-rotation: PASS
- lighting-result-missing: PASS
- lighting-result-orphan: PASS
- lighting-itemid-mismatch: PASS
- lighting-state-mismatch: PASS
- array-result-missing: PASS
- array-state-mismatch: PASS
- build-result-missing: PASS
- build-unlit-counted-only-in-child-snapshot: PASS
- build-track-count-mismatch: PASS
- build-track-wrong-tag-source: PASS
- build-track-source-itemids-mismatch: PASS
- build-track-missing-counted-source: PASS
- build-track-kind-or-buildid-mismatch: PASS
- awakening-result-missing: PASS
- awakening-active-only-in-child-snapshot: PASS
- provider-unlit-item-excluded-from-build: PASS
- provider-unlit-item-cannot-activate-array: PASS
- provider-unopened-core-not-active: PASS
- provider-opened-but-unlit-core-not-active: PASS
- stale-placement-id-no-itemid-fallback: PASS
- distinct-base-placement-state-isolated: PASS
- i031-catalog-preview-no-placement-state: PASS
- i031-placed-instance-source-boundary: PASS
- catalog-31-full-snapshot-and-detail-projection: PASS
- runtime-leak-check: PASS
