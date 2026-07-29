# Bone Aspect Mechanic Vocabulary Extension Leak Check

- Overall: `PASS`
- Checks: `53/53 PASS`
- Package files: `17 additions / 0 existing modifications`
- Runtime/formal-flow consumers: `0 / 0`
- Global E02 default replacement: `false`
- BA-D3 / BA-D4: `USER_DECISION_REQUIRED / NOT_SELECTED`
- Base canonical: `sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833`
- Extension canonical: `sha256:45b4e2ca726d4934afe5a13f5fe25e0d9be132e950141714ce3f70c752f589ec`
- Composed canonical: `sha256:2a397064621c315fac161889b74082bfb777a6c763387f2adf2535318da339cc`

The explicit composer returns a new input and never stores it as a global default.
Any concurrent presentation/BuildSandbox/Scene drift remains external and is never restored or claimed.

## External Concurrent Drift Evidence

- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BoneHoundVisualPrototypeController.cs`: `ATTRIBUTED` / V0.4-BoneHoundLimitedMotionVfxPrototype01 / Visual Guard / start=`ABSENT` / current=`286204bf68846c8a87d3b0f243d10ed79df2bed68349d31fd5fb1587ef69eeb3`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BoneHoundVisualPrototypeController.cs.meta`: `ATTRIBUTED` / V0.4-BoneHoundLimitedMotionVfxPrototype01 / Visual Guard / start=`ABSENT` / current=`d30a27d0a7af4e0bba980aec0c953975860cebb351b70a1c31be8f74523c3eb3`
- `Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs`: `ATTRIBUTED` / V0.4-BattleSandboxTrayArtworkLayerSeparationGuardFix01 / Item Guard + Overall Shared Presentation / start=`12e4ee303b4457920b38aa71a55ff155baa56aa2816c5d9da548debd2e3d3993` / current=`e4782ae0933c8c1e89af89e1ecfffda2b3388d985be711d7c68f0c3604cce6e9`

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

## Checks

- `extension-schema-id`: `PASS`
- `extension-schema-version`: `PASS`
- `extension-id`: `PASS`
- `extension-base-schema`: `PASS`
- `extension-safe-flags`: `PASS`
- `extension-entry-count`: `PASS`
- `extension-exact-stable-keys`: `PASS`
- `extension-exact-categories`: `PASS`
- `extension-exact-labels`: `PASS`
- `extension-exact-descriptions`: `PASS`
- `extension-exact-candidate-lineage`: `PASS`
- `extension-exact-survey-lineage`: `PASS`
- `extension-player-visible-zero`: `PASS`
- `extension-developer-only-zero`: `PASS`
- `extension-runtime-implemented-zero`: `PASS`
- `base-entry-count`: `PASS`
- `base-category-counts`: `PASS`
- `base-legacy-count`: `PASS`
- `base-canonical-unchanged`: `PASS`
- `composed-entry-count`: `PASS`
- `composed-category-counts`: `PASS`
- `composed-legacy-count-unchanged`: `PASS`
- `composition-base-input-immutability`: `PASS`
- `composition-extension-input-immutability`: `PASS`
- `extension-defensive-copy`: `PASS`
- `extension-read-only-collections`: `PASS`
- `extension-canonical-determinism`: `PASS`
- `composition-canonical-determinism`: `PASS`
- `composition-reverse-input-determinism`: `PASS`
- `composition-culture-determinism`: `PASS`
- `extension-canonical-field-sensitivity`: `PASS`
- `composition-canonical-field-sensitivity`: `PASS`
- `reject-duplicate-extension-key`: `PASS`
- `reject-base-key-collision`: `PASS`
- `reject-prefix-category-mismatch`: `PASS`
- `reject-unapproved-eighth-entry`: `PASS`
- `reject-missing-required-entry`: `PASS`
- `reject-unknown-survey-row`: `PASS`
- `reject-candidate-lineage-mismatch`: `PASS`
- `reject-null-entry`: `PASS`
- `reject-schema-mismatch`: `PASS`
- `reject-unsafe-flags`: `PASS`
- `reject-player-visible-entry`: `PASS`
- `no-runtime-consumer`: `PASS`
- `no-formal-flow-consumer`: `PASS`
- `package-path-whitelist`: `PASS`
- `report-repeat-determinism`: `PASS`
- `missing-report-regeneration`: `PASS`
- `guid-uniqueness`: `PASS`
- `text-integrity`: `PASS`
- `protected-e02-baseline`: `PASS`
- `protected-upstream-baseline`: `PASS`
- `protected-broad-baseline`: `PASS`
