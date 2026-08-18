# C1 Lv1 Starter Item Baseline 01 Report

- Package: `V0.4-C1Lv1StarterItemBaseline01`
- Assignment SHA-256: `62d567cbc22ffc9662589f86e126d626bb45173f06bd24f8264ba7a71fd884b8`
- Status: `DEV_COMPLETE / QA_PASS / USER_HANDTEST_NOT_APPLICABLE`
- Verifier marker: `C1LV1_STARTER_ITEM_BASELINE01_PASS`
- Assertions: `69 total / 0 failed`

## Frozen Baseline

- `CAMPAIGN_NORMAL_LV1 / I001@white / directDamage=82 / cooldownSeconds=2 / LIT_REQUIRED`
- `CAMPAIGN_NORMAL_LV1 / I002@white / directDamage=28 / cooldownSeconds=2 / LIT_REQUIRED`
- Baseline schema: `C1Lv1StarterItemBaseline.v1`
- Projection schema: `C1Lv1StarterItemFactProjection.v1`
- Profile revision: `C1LV1_STARTER_ITEM_BASELINE_R1`

## Canonical Signatures

- Catalog: `d0dd84f6e2f59344d3a49416d883eb5f4f84b408ab85a96a8dcf6751de97e357`
- I001 baseline: `d272004505a969a148ce33ed9f5a70c7871991c01f6fd5ebbbeb9f98383462e2`
- I001 projection: `10251a4ee2a3b29b44f6d67565de89e077ab4a8242f96c245db3a0e657aa35d9`
- I002 baseline: `8db60eef44175941b790f16dc0d05c4e64b734a7c90654a9b92941a02573bd21`
- I002 projection: `9830747edfb3de3318910795820588b2114d28bf108e79fb5b76ef9f6cf664d3`

## Workbench Isolation

- Profile count: `30`
- Aggregate: `1fb4e25cc46caee1adfb0b2262e061bdbc86bd3cf9f89f425fcfd97ca15ea2c3`
- I001 asset: `9AB02D0088793B08D97B74523E989B4C8D3837CE0060EB65302858D2B004655B`
- I002 asset: `626120DEDFCF84D522FBD883438CE323494A921F3B6E3BF1AB7F79BF9BB09473`

## Non-executable Design Evidence

- Legal weak 1-2 layout, I002 unlit: `48 seconds / 10 enemy attack waves`.
- Target 1-2 layout, I002 lit: `36 seconds / 7 enemy attack waves`.
- I002 direct-damage sensitivity: `-20% => 38 seconds`; `+20% => 34 seconds`.
- These measurements are report-only evidence and are not executable fields.

## Scope

- Scoped write whitelist: `PASS`
- Assignment unchanged: `PASS`
- No Unity batch, Play mode, Scene/Prefab authoring, Builder, or Git operation is part of this verifier.

## Assertions

- `PASS` source / assignment-sha-before
- `PASS` catalog / catalog-validation
- `PASS` catalog / catalog-row-count
- `PASS` catalog / catalog-exact-identities
- `PASS` catalog / catalog-excludes-I031
- `PASS` projection / approved-projection-I001@white
- `PASS` projection / approved-projection-I002@white
- `PASS` canonical / catalog-canonical-signature-present
- `PASS` negative / negative-unsupported-context
- `PASS` negative / negative-white-only
- `PASS` negative / negative-I031
- `PASS` negative / negative-unknown-item
- `PASS` negative / negative-malformed-version
- `PASS` negative / negative-duplicate-separator
- `PASS` negative / negative-version-base-mismatch
- `PASS` negative / negative-unsupported-revision
- `PASS` negative / negative-empty-context
- `PASS` negative / negative-empty-base-item
- `PASS` negative / negative-empty-version-key
- `PASS` negative / negative-empty-revision
- `PASS` negative / negative-duplicate-identity-fixture
- `PASS` negative / negative-null-fixture
- `PASS` negative / negative-null-row-fixture
- `PASS` canonical / canonical-repeat-100
- `PASS` canonical / canonical-reversed-enumeration
- `PASS` canonical / canonical-invariant-culture
- `PASS` immutability / immutable-public-surface
- `PASS` source / runtime-source-present
- `PASS` leak / runtime-forbidden-identifier-Reward
- `PASS` leak / runtime-forbidden-identifier-Drop
- `PASS` leak / runtime-forbidden-identifier-Battle
- `PASS` leak / runtime-forbidden-identifier-Enemy
- `PASS` leak / runtime-forbidden-identifier-Stage
- `PASS` leak / runtime-forbidden-identifier-Board
- `PASS` leak / runtime-forbidden-identifier-Inventory
- `PASS` leak / runtime-forbidden-identifier-Save
- `PASS` leak / runtime-forbidden-identifier-UI
- `PASS` leak / runtime-forbidden-identifier-Scene
- `PASS` leak / runtime-forbidden-identifier-Build
- `PASS` leak / runtime-forbidden-identifier-Roll
- `PASS` leak / runtime-forbidden-identifier-RNG
- `PASS` leak / runtime-forbidden-identifier-probability
- `PASS` leak / runtime-forbidden-identifier-Core
- `PASS` leak / runtime-forbidden-identifier-Affix
- `PASS` leak / runtime-forbidden-text-UnityEngine
- `PASS` leak / runtime-forbidden-text-UnityEditor
- `PASS` leak / runtime-forbidden-text-MonoBehaviour
- `PASS` leak / runtime-forbidden-text-ScriptableObject
- `PASS` leak / runtime-forbidden-text-System-Random
- `PASS` leak / runtime-forbidden-text-Guid-NewGuid
- `PASS` leak / runtime-forbidden-text-DateTime-Now
- `PASS` leak / runtime-forbidden-text-DateTime-UtcNow
- `PASS` leak / runtime-forbidden-text-Environment-TickCount
- `PASS` leak / runtime-forbidden-text-ItemBalanceWorkbench
- `PASS` leak / runtime-forbidden-text-BALANCE_CANDIDATE
- `PASS` leak / runtime-forbidden-text-designEvidence
- `PASS` source / runtime-required-item-dependency-ItemInstanceRarity
- `PASS` source / runtime-required-item-dependency-ItemInnerDataCatalog-FindById
- `PASS` isolation / workbench-profile-count
- `PASS` isolation / workbench-aggregate
- `PASS` isolation / workbench-I001
- `PASS` isolation / workbench-I002
- `PASS` scope / scoped-write-whitelist
- `PASS` source / assignment-sha-after
- `PASS` reports / output-present-C1Lv1StarterItemBaseline01Report.md
- `PASS` reports / output-present-C1Lv1StarterItemBaseline01Spec.csv
- `PASS` reports / output-present-C1Lv1StarterItemBaseline01CanonicalDeterminism.csv
- `PASS` reports / output-present-C1Lv1StarterItemBaseline01WorkbenchIsolation.csv
- `PASS` reports / output-present-C1Lv1StarterItemBaseline01LeakCheckReport.md

C1LV1_STARTER_ITEM_BASELINE01_PASS
