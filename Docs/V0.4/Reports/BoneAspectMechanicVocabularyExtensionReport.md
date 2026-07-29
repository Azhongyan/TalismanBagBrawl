# Bone Aspect Mechanic Vocabulary Extension Report

- Package: `V0.4-BoneAspectMechanicVocabularyExtension01`
- Status: `QA_PASS`
- Verification: `53/53 PASS`
- Execution modes: `OFFLINE_SAME_SOURCE`, `UNITY_BATCH`
- Unity scene handtest: `NOT_REQUIRED`
- Runtime consumer: `NONE`
- Formal-flow consumer: `NONE`
- `devOnly=true / isEnabled=false / entersFormalFlow=false / runtimeImplemented=false`

## Canonical Signatures

- BaseCanonicalSignature: `sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833`
- ExtensionCanonicalSignature: `sha256:45b4e2ca726d4934afe5a13f5fe25e0d9be132e950141714ce3f70c752f589ec`
- ComposedCanonicalSignature: `sha256:2a397064621c315fac161889b74082bfb777a6c763387f2adf2535318da339cc`
- The composed signature is an explicit read-only composition result, not a new global E02 default.

## Composition

- Entries: `63 + 7 = 70`
- Legacy mappings: `124 + 0 = 124`
- Extension categories: `Mechanic=5`, `CounterWindowType=2`
- BA-D3 / BA-D4: `USER_DECISION_REQUIRED / NOT_SELECTED`
- BA-D1 / BA-D2 runtime: `OUT_OF_SCOPE`

## Boundary

The package adds only neutral vocabulary identities. It does not bind Carrier, Skill, Phase,
Pressure, Readiness, Battle, Item, BuildSandbox, CrossSystem, Scene, Prefab, UI, or formal flow.
It does not consume Bone Hound prototype state or presentation semantics.

## External Concurrent Drift

- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BoneHoundVisualPrototypeController.cs`: `ATTRIBUTED` — V0.4-BoneHoundLimitedMotionVfxPrototype01 / Visual Guard; start=`ABSENT`; current=`286204bf68846c8a87d3b0f243d10ed79df2bed68349d31fd5fb1587ef69eeb3`.
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BoneHoundVisualPrototypeController.cs.meta`: `ATTRIBUTED` — V0.4-BoneHoundLimitedMotionVfxPrototype01 / Visual Guard; start=`ABSENT`; current=`d30a27d0a7af4e0bba980aec0c953975860cebb351b70a1c31be8f74523c3eb3`.
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs`: `ATTRIBUTED` — V0.4-BattleSandboxTrayArtworkLayerSeparationGuardFix01 / Item Guard + Overall Shared Presentation; start=`12e4ee303b4457920b38aa71a55ff155baa56aa2816c5d9da548debd2e3d3993`; current=`e4782ae0933c8c1e89af89e1ecfffda2b3388d985be711d7c68f0c3604cce6e9`.
- These files are outside package ownership and were not restored, formatted, modified, or claimed.

## Package Manifest

- Scope: `additions=17 / existing modifications=0`
- Additions: `17`
- Existing task-start files modified by this package: `0`
- Exact paths below use `StringComparer.Ordinal` order:
- `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs`
- `Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs.meta`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs`
- `Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs.meta`
- `Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionComposition.csv`
- `Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionInventory.csv`
- `Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionLeakCheckReport.md`
- `Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionReport.md`
- `Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSourceEvidence.csv`
- `Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSpec.csv`

## Unity Entry Guard Correction

- Historical defective Assignment entry (preserved as evidence): `TalismanBag.Editor.EnemySystem.BoneAspectMechanicVocabularyExtensionVerifier.RunBatch`
- Effective compile-safe entry: `TalismanBag.EditorTools.EnemySystem.BoneAspectMechanicVocabularyExtensionVerifier.RunBatch`
- Guard reason: prevents creation of sibling namespace `TalismanBag.Editor` and matches all established Enemy verifier namespaces.
- Menu item remains: `TalismanBag/Enemy System/Run Bone Aspect Mechanic Vocabulary Extension Verifier`

## Verification Summary

- `extension-schema-id`: `PASS` — Exact incremental schema identity.
- `extension-schema-version`: `PASS` — Exact extension schema version.
- `extension-id`: `PASS` — Bone Aspect extension identity is stable.
- `extension-base-schema`: `PASS` — Explicit E02 compatibility only.
- `extension-safe-flags`: `PASS` — devOnly/isEnabled/entersFormalFlow/runtimeImplemented.
- `extension-entry-count`: `PASS` — Exactly seven approved entries.
- `extension-exact-stable-keys`: `PASS` — No rename, merge, split, or eighth key.
- `extension-exact-categories`: `PASS` — Categories match the approved seven.
- `extension-exact-labels`: `PASS` — Developer labels are assignment-exact.
- `extension-exact-descriptions`: `PASS` — Neutral descriptions are assignment-exact.
- `extension-exact-candidate-lineage`: `PASS` — Catalog fields match the accepted Gap Survey candidate sheet.
- `extension-exact-survey-lineage`: `PASS` — Each entry retains one accepted survey row.
- `extension-player-visible-zero`: `PASS` — No player projection.
- `extension-developer-only-zero`: `PASS` — Neutral vocabulary entries are not diagnostics.
- `extension-runtime-implemented-zero`: `PASS` — No runtime implementation claim.
- `base-entry-count`: `PASS` — Frozen E02 default.
- `base-category-counts`: `PASS` — 12/16/9/6/10/10.
- `base-legacy-count`: `PASS` — Legacy mappings remain on the base.
- `base-canonical-unchanged`: `PASS` — E02 canonical remains byte-stable.
- `composed-entry-count`: `PASS` — Explicit 63 + 7 composition.
- `composed-category-counts`: `PASS` — 17/16/9/8/10/10.
- `composed-legacy-count-unchanged`: `PASS` — The extension adds no legacy mapping.
- `composition-base-input-immutability`: `PASS` — Composer did not mutate the explicit E02 input.
- `composition-extension-input-immutability`: `PASS` — Composer did not mutate the extension input.
- `extension-defensive-copy`: `PASS` — Caller-owned lists cannot mutate snapshots.
- `extension-read-only-collections`: `PASS` — Collections reject mutation.
- `extension-canonical-determinism`: `PASS` — Repeated construction is deterministic.
- `composition-canonical-determinism`: `PASS` — Repeated explicit composition is deterministic.
- `composition-reverse-input-determinism`: `PASS` — Ordinal canonicalization is independent of input order.
- `composition-culture-determinism`: `PASS` — Integer and string canonicalization are culture invariant.
- `extension-canonical-field-sensitivity`: `PASS` — A synthetic field mutation changes the extension signature.
- `composition-canonical-field-sensitivity`: `PASS` — A synthetic base field mutation changes explicit composition.
- `reject-duplicate-extension-key`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-base-key-collision`: `PASS` — Synthetic base collision never enters the catalog.
- `reject-prefix-category-mismatch`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-unapproved-eighth-entry`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-missing-required-entry`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-unknown-survey-row`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-candidate-lineage-mismatch`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-null-entry`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-schema-mismatch`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-unsafe-flags`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `reject-player-visible-entry`: `PASS` — Synthetic fixture is rejected and never enters the catalog.
- `no-runtime-consumer`: `PASS` — No runtime consumer outside the package.
- `no-formal-flow-consumer`: `PASS` — No Battle/Item/BuildSandbox/CrossSystem/Scene/Config binding.
- `package-path-whitelist`: `PASS` — Assignment paths are exact/unique/Ordinal and projected to both Markdown reports; verifier resolves under TalismanBag.EditorTools.EnemySystem and introduces no TalismanBag.Editor namespace.
- `report-repeat-determinism`: `PASS` — Two pure report projections are identical.
- `missing-report-regeneration`: `PASS` — Every run rewrites all six reports from the same verified state.
- `guid-uniqueness`: `PASS` — All package GUIDs are present and globally unique.
- `text-integrity`: `PASS` — 11 source/meta files + 6 deterministic report projections checked.
- `protected-e02-baseline`: `PASS` — Accepted E02 package remains byte-identical.
- `protected-upstream-baseline`: `PASS` — All accepted Bone Aspect evidence packages remain byte-identical.
- `protected-broad-baseline`: `PASS` — Frozen baseline preserved after explicit owner/hash attribution; no external file was restored or claimed.
