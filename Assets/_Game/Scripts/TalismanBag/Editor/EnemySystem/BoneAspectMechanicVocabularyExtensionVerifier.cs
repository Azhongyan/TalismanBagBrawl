using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.EnemySystem.BoneAspect.VocabularyExtension;
using TalismanBag.EnemySystem.Vocabulary;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.EnemySystem
{
    public static class BoneAspectMechanicVocabularyExtensionVerifier
    {
        private const string AssignmentPath =
            "Docs/V0.4/BoneAspectMechanicVocabularyExtension01_Assignment.md";
        private const string CandidateSheetPath =
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv";
        private const string SurveyMatrixPath =
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMatrix.csv";
        private const string DetailReportPath =
            "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionReport.md";
        private const string SpecCsvPath =
            "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSpec.csv";
        private const string InventoryCsvPath =
            "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionInventory.csv";
        private const string SourceEvidenceCsvPath =
            "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionSourceEvidence.csv";
        private const string CompositionCsvPath =
            "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionComposition.csv";
        private const string LeakReportPath =
            "Docs/V0.4/Reports/BoneAspectMechanicVocabularyExtensionLeakCheckReport.md";
        private const string ExpectedBaseCanonical =
            "sha256:b05ab2c8785c116be0267ed47aad6b2499731d077a506da4841c0ab7500f2833";

        private static readonly string[] CheckIds =
        {
            "extension-schema-id",
            "extension-schema-version",
            "extension-id",
            "extension-base-schema",
            "extension-safe-flags",
            "extension-entry-count",
            "extension-exact-stable-keys",
            "extension-exact-categories",
            "extension-exact-labels",
            "extension-exact-descriptions",
            "extension-exact-candidate-lineage",
            "extension-exact-survey-lineage",
            "extension-player-visible-zero",
            "extension-developer-only-zero",
            "extension-runtime-implemented-zero",
            "base-entry-count",
            "base-category-counts",
            "base-legacy-count",
            "base-canonical-unchanged",
            "composed-entry-count",
            "composed-category-counts",
            "composed-legacy-count-unchanged",
            "composition-base-input-immutability",
            "composition-extension-input-immutability",
            "extension-defensive-copy",
            "extension-read-only-collections",
            "extension-canonical-determinism",
            "composition-canonical-determinism",
            "composition-reverse-input-determinism",
            "composition-culture-determinism",
            "extension-canonical-field-sensitivity",
            "composition-canonical-field-sensitivity",
            "reject-duplicate-extension-key",
            "reject-base-key-collision",
            "reject-prefix-category-mismatch",
            "reject-unapproved-eighth-entry",
            "reject-missing-required-entry",
            "reject-unknown-survey-row",
            "reject-candidate-lineage-mismatch",
            "reject-null-entry",
            "reject-schema-mismatch",
            "reject-unsafe-flags",
            "reject-player-visible-entry",
            "no-runtime-consumer",
            "no-formal-flow-consumer",
            "package-path-whitelist",
            "report-repeat-determinism",
            "missing-report-regeneration",
            "guid-uniqueness",
            "text-integrity",
            "protected-e02-baseline",
            "protected-upstream-baseline",
            "protected-broad-baseline"
        };

        private static readonly string[] PackageManifest =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/BoneAspectMechanicVocabularyExtensionCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionComposer.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/EnemyMechanicVocabularyExtensionValidation.cs.meta",
            CompositionCsvPath,
            InventoryCsvPath,
            LeakReportPath,
            DetailReportPath,
            SourceEvidenceCsvPath,
            SpecCsvPath
        };

        private static readonly string[] SourceAndMetaManifest =
            PackageManifest.Take(11).ToArray();

        private static readonly string[] ReportPaths =
        {
            DetailReportPath,
            SpecCsvPath,
            InventoryCsvPath,
            SourceEvidenceCsvPath,
            CompositionCsvPath,
            LeakReportPath
        };

        private static readonly string[] ExpectedStableKeys =
        {
            "counter_window.recognition_reveal",
            "counter_window.weakpoint_exposure",
            "mechanic.charge_attack",
            "mechanic.contested_mark",
            "mechanic.damage_reduction",
            "mechanic.possession_state",
            "mechanic.status_stack"
        };

        private static readonly string[] ExpectedCandidateIds =
        {
            "ba_gap_candidate.charge_attack",
            "ba_gap_candidate.contested_mark",
            "ba_gap_candidate.damage_reduction",
            "ba_gap_candidate.possession_state",
            "ba_gap_candidate.recognition_reveal_window",
            "ba_gap_candidate.status_stack",
            "ba_gap_candidate.weakpoint_exposure_window"
        };

        private static readonly string[] ExpectedSurveyRows =
        {
            "GAP-001",
            "GAP-003",
            "GAP-005",
            "GAP-010",
            "GAP-015",
            "GAP-016",
            "GAP-039"
        };

        private static readonly IReadOnlyDictionary<EnemyVocabularyCategory, int> BaseCategoryCounts =
            new Dictionary<EnemyVocabularyCategory, int>
            {
                { EnemyVocabularyCategory.Mechanic, 12 },
                { EnemyVocabularyCategory.BuildCapability, 16 },
                { EnemyVocabularyCategory.PressureChannel, 9 },
                { EnemyVocabularyCategory.CounterWindowType, 6 },
                { EnemyVocabularyCategory.PlayerHintCategory, 10 },
                { EnemyVocabularyCategory.DeveloperDiagnosticCategory, 10 }
            };

        private static readonly IReadOnlyDictionary<EnemyVocabularyCategory, int> ComposedCategoryCounts =
            new Dictionary<EnemyVocabularyCategory, int>
            {
                { EnemyVocabularyCategory.Mechanic, 17 },
                { EnemyVocabularyCategory.BuildCapability, 16 },
                { EnemyVocabularyCategory.PressureChannel, 9 },
                { EnemyVocabularyCategory.CounterWindowType, 8 },
                { EnemyVocabularyCategory.PlayerHintCategory, 10 },
                { EnemyVocabularyCategory.DeveloperDiagnosticCategory, 10 }
            };

        private static readonly string[] E02Files =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyModel.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/EnemyVocabularyValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyMechanicVocabularyVerifier.cs.meta",
            "Docs/V0.4/Reports/EnemyMechanicVocabularyReport.md",
            "Docs/V0.4/Reports/EnemyMechanicVocabularySpec.csv",
            "Docs/V0.4/Reports/EnemyMechanicVocabularyInventory.csv",
            "Docs/V0.4/Reports/EnemyMechanicVocabularyLegacyMapping.csv",
            "Docs/V0.4/Reports/EnemyMechanicVocabularyLeakCheckReport.md"
        };

        private static readonly string[] P0Files =
        {
            "Docs/V0.4/Reports/BoneAspectContentDesignLockReport.md",
            "Docs/V0.4/Reports/BoneAspectContentCatalog.csv",
            "Docs/V0.4/Reports/BoneAspectEncounterNodeCatalog.csv",
            "Docs/V0.4/Reports/BoneAspectMechanicCommitmentMatrix.csv",
            "Docs/V0.4/Reports/BoneAspectVisualLanguageSpec.csv",
            "Docs/V0.4/Reports/BoneAspectExternalReferenceBoundary.csv",
            "Docs/V0.4/Reports/BoneAspectUserDecisionSheet.csv",
            "Docs/V0.4/Reports/BoneAspectContentDesignLockLeakCheckReport.md"
        };

        private static readonly string[] P1Files =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationProvider.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/CarrierPresentation/BoneAspectCarrierPresentationValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectCarrierPresentationCatalogVerifier.cs.meta",
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogReport.md",
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogSpec.csv",
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationInventory.csv",
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationArtSlotRows.csv",
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationMechanicReferenceRows.csv",
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationPlayerSafeFieldMatrix.csv",
            "Docs/V0.4/Reports/BoneAspectCarrierPresentationCatalogLeakCheckReport.md"
        };

        private static readonly string[] GapSurveyFiles =
        {
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReport.md",
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMatrix.csv",
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyReuseEvidence.csv",
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyMissingConcepts.csv",
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyOwnershipMatrix.csv",
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyDecisionExclusions.csv",
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyExtensionCandidateSheet.csv",
            "Docs/V0.4/Reports/BoneAspectMechanicGapSurveyLeakCheckReport.md"
        };

        private static readonly string[] BossArtFiles =
        {
            "Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageSurveyReport.md",
            "Docs/V0.4/Reports/BoneAspectBossArtSourceInventory.csv",
            "Docs/V0.4/Reports/BoneAspectBossArtSourceStatementInventory.csv",
            "Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageMatrix.csv",
            "Docs/V0.4/Reports/BoneAspectBossDecisionResolutionOverlay.csv",
            "Docs/V0.4/Reports/BoneAspectBossPhaseCandidateInventory.csv",
            "Docs/V0.4/Reports/BoneAspectBossSkillCandidateInventory.csv",
            "Docs/V0.4/Reports/BoneAspectBossArtSupersededSemantics.csv",
            "Docs/V0.4/Reports/BoneAspectBossArtMechanicLineageLeakCheckReport.md"
        };

        private static readonly IReadOnlyDictionary<string, BaselineExpectation> BroadBaselines =
            new Dictionary<string, BaselineExpectation>(StringComparer.Ordinal)
            {
                { "EnemySystem", new BaselineExpectation(108, "6b8b5f14b9841eaeb7872077401287de71b384db27f58310e19faae27bb001ee") },
                { "EditorEnemySystem", new BaselineExpectation(26, "d7631c471509891eaab6904873f15ead4970f0466a1bdce204e68ca8b2ebc111") },
                { "Items", new BaselineExpectation(141, "4b41e4dbede32a14a661f0f985cb1682051c30836b54d811a4c55546652b18e4") },
                { "BuildSandbox", new BaselineExpectation(180, "78a2b34e535b76022e2a187756edc610f91df016594060bb72e38f1849652705") },
                { "CrossSystem", new BaselineExpectation(63, "dfd3445869710d3b28e3ddacdca20a7850ea8c0125246599b14e1ed649e1a64d") },
                { "Scenes", new BaselineExpectation(14, "e11d46573ca87c3cc7809f8c30451ebe73624f8d5b61c40f535576911fcc76a5") },
                { "UnityAssets", new BaselineExpectation(7, "ff3c5b0f04cdf90d26db102a5418cdcabf163b87d7ae03417d7faabd1247d45c") },
                { "Prefabs", new BaselineExpectation(5, "a0274a939eeed890c98522bde495a37b2f4b533fdcd753bdb2896cf6b5be485e") },
                { "Configs", new BaselineExpectation(121, "2383359ab713475d8d85748032965c3188f47c436e3644d40abfb54260807769") },
                { "ProjectSettings", new BaselineExpectation(21, "1969814e5ec3983ba426b7d5f19f13de7f480277cb5118a92d06c66b58fcbac7") },
                { "Packages", new BaselineExpectation(2, "11efaaf4e1df1215e977f884d2d5e2c2bdbbbd1e62337b0517f6a0502a929cb6") },
                { "GuardQueues", new BaselineExpectation(4, "e4cfa9f381e62cf296d771fb6c718db26f62e375577056063b7e0923aca221cf") }
            };

        private static readonly ExternalDriftEvidence[] ExternalDrifts =
        {
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BoneHoundVisualPrototypeController.cs",
                null,
                "286204bf68846c8a87d3b0f243d10ed79df2bed68349d31fd5fb1587ef69eeb3",
                "V0.4-BoneHoundLimitedMotionVfxPrototype01 / Visual Guard",
                true),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BoneHoundVisualPrototypeController.cs.meta",
                null,
                "d30a27d0a7af4e0bba980aec0c953975860cebb351b70a1c31be8f74523c3eb3",
                "V0.4-BoneHoundLimitedMotionVfxPrototype01 / Visual Guard",
                true),
            new ExternalDriftEvidence(
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildItemPreviewCardView.cs",
                "12e4ee303b4457920b38aa71a55ff155baa56aa2816c5d9da548debd2e3d3993",
                "e4782ae0933c8c1e89af89e1ecfffda2b3388d985be711d7c68f0c3604cce6e9",
                "V0.4-BattleSandboxTrayArtworkLayerSeparationGuardFix01 / Item Guard + Overall Shared Presentation",
                true)
        };

        private static readonly DefaultEnemyMechanicVocabularyProvider Provider =
            DefaultEnemyMechanicVocabularyProvider.Instance;
        private static readonly EnemyMechanicVocabularyExtensionValidator ExtensionValidator =
            EnemyMechanicVocabularyExtensionValidator.Instance;
        private static readonly EnemyMechanicVocabularyExtensionComposer Composer =
            EnemyMechanicVocabularyExtensionComposer.Instance;

#if UNITY_EDITOR
        [MenuItem("TalismanBag/Enemy System/Run Bone Aspect Mechanic Vocabulary Extension Verifier")]
        public static void RunMenu()
        {
            VerifyAndWriteReports(false);
        }
#endif

        public static void RunBatch()
        {
            VerifyAndWriteReports(true);
        }

        public static void RunOffline()
        {
            VerifyAndWriteReports(false);
        }

        private static void VerifyAndWriteReports(bool exitWhenDone)
        {
#if UNITY_EDITOR
            int exitCode = 1;
#endif
            try
            {
                string projectRoot = FindProjectRoot();
                VerificationContext context = RunVerification(projectRoot);
                WriteReports(projectRoot, context);
                if (!context.Result.Passed)
                {
                    throw new InvalidOperationException(
                        "Bone Aspect vocabulary extension verification failed: "
                        + context.Result.PassedCount.ToString(CultureInfo.InvariantCulture)
                        + "/" + context.Result.Rows.Count.ToString(CultureInfo.InvariantCulture)
                        + " PASS.");
                }

#if UNITY_EDITOR
                exitCode = 0;
#endif
                Log("Bone Aspect vocabulary extension verification PASS: 53/53.");
            }
            catch (Exception exception)
            {
                LogError(exception.ToString());
                if (!exitWhenDone)
                {
                    throw;
                }
            }
            finally
            {
#if UNITY_EDITOR
                if (exitWhenDone && Application.isBatchMode)
                {
                    EditorApplication.Exit(exitCode);
                }
#endif
            }
        }

        private static VerificationContext RunVerification(string projectRoot)
        {
            VerificationResult result = new VerificationResult();
            EnemyMechanicVocabularySnapshotInput baseInput =
                DefaultEnemyMechanicVocabularyCatalog.CreateInput();
            EnemyMechanicVocabularySnapshot baseSnapshot = Provider.CreateSnapshot(baseInput);
            EnemyMechanicVocabularyExtensionSnapshot extension =
                BoneAspectMechanicVocabularyExtensionCatalog.CreateSnapshot();
            string baseBefore = baseSnapshot.BuildCanonicalSignature();
            string extensionBefore = extension.CanonicalSignature;
            EnemyMechanicVocabularySnapshotInput composedInput = Composer.Compose(baseInput, extension);
            EnemyMechanicVocabularySnapshot composedSnapshot = Provider.CreateSnapshot(composedInput);

            Add(result, "extension-schema-id", "schema",
                EnemyMechanicVocabularyExtensionSchema.SchemaId, extension.SchemaId,
                extension.SchemaId == EnemyMechanicVocabularyExtensionSchema.SchemaId,
                "Exact incremental schema identity.");
            Add(result, "extension-schema-version", "schema", "1",
                extension.SchemaVersion.ToString(CultureInfo.InvariantCulture),
                extension.SchemaVersion == 1, "Exact extension schema version.");
            Add(result, "extension-id", "schema",
                EnemyMechanicVocabularyExtensionSchema.ExtensionId, extension.ExtensionId,
                extension.ExtensionId == EnemyMechanicVocabularyExtensionSchema.ExtensionId,
                "Bone Aspect extension identity is stable.");
            Add(result, "extension-base-schema", "schema",
                "EnemyMechanicVocabulary.v1@1",
                extension.BaseSchemaId + "@" + extension.BaseSchemaVersion.ToString(CultureInfo.InvariantCulture),
                extension.BaseSchemaId == EnemyMechanicVocabularySchema.SchemaId
                    && extension.BaseSchemaVersion == EnemyMechanicVocabularySchema.SchemaVersion,
                "Explicit E02 compatibility only.");
            Add(result, "extension-safe-flags", "scope",
                "true/false/false/false",
                Bool(extension.DevOnly) + "/" + Bool(extension.IsEnabled) + "/"
                    + Bool(extension.EntersFormalFlow) + "/" + Bool(extension.RuntimeImplemented),
                extension.DevOnly && !extension.IsEnabled && !extension.EntersFormalFlow
                    && !extension.RuntimeImplemented,
                "devOnly/isEnabled/entersFormalFlow/runtimeImplemented.");
            Add(result, "extension-entry-count", "identity", "7",
                extension.Entries.Count.ToString(CultureInfo.InvariantCulture),
                extension.Entries.Count == 7, "Exactly seven approved entries.");

            string[] actualKeys = extension.Entries.Select(value => value.StableKey)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(result, "extension-exact-stable-keys", "identity",
                string.Join("|", ExpectedStableKeys), string.Join("|", actualKeys),
                ExpectedStableKeys.SequenceEqual(actualKeys, StringComparer.Ordinal),
                "No rename, merge, split, or eighth key.");

            Dictionary<string, string> expectedCategories = ExpectedDefinitions()
                .ToDictionary(value => value.StableKey, value => value.Category, StringComparer.Ordinal);
            bool categoriesExact = extension.Entries.All(value =>
                expectedCategories.TryGetValue(value.StableKey, out string expected)
                && expected == EnemyVocabularyCategoryNames.StableName(value.Category));
            Add(result, "extension-exact-categories", "identity", "5 Mechanic + 2 CounterWindowType",
                CategorySummary(extension.Entries), categoriesExact,
                "Categories match the approved seven.");

            Dictionary<string, ExpectedDefinition> definitions = ExpectedDefinitions()
                .ToDictionary(value => value.StableKey, value => value, StringComparer.Ordinal);
            bool labelsExact = extension.Entries.All(value =>
                definitions.TryGetValue(value.StableKey, out ExpectedDefinition expected)
                && expected.Label == value.DeveloperLabelZh);
            Add(result, "extension-exact-labels", "identity", "7/7 exact",
                labelsExact ? "7/7 exact" : "mismatch", labelsExact,
                "Developer labels are assignment-exact.");
            bool descriptionsExact = extension.Entries.All(value =>
                definitions.TryGetValue(value.StableKey, out ExpectedDefinition expected)
                && expected.Description == value.Description);
            Add(result, "extension-exact-descriptions", "identity", "7/7 exact",
                descriptionsExact ? "7/7 exact" : "mismatch", descriptionsExact,
                "Neutral descriptions are assignment-exact.");

            string candidateSheet = File.ReadAllText(Absolute(projectRoot, CandidateSheetPath));
            bool candidateLineage = extension.Entries.All(value =>
                CandidateSheetHas(candidateSheet, value));
            Add(result, "extension-exact-candidate-lineage", "lineage", "7/7 source rows",
                candidateLineage ? "7/7 source rows" : "mismatch", candidateLineage,
                "Catalog fields match the accepted Gap Survey candidate sheet.");

            string surveyMatrix = File.ReadAllText(Absolute(projectRoot, SurveyMatrixPath));
            bool surveyLineage = extension.Entries.All(value =>
                value.SupportingSurveyRowIds.Count == 1
                && SurveyMatrixHas(surveyMatrix, value.SupportingSurveyRowIds[0], value.CandidateId));
            Add(result, "extension-exact-survey-lineage", "lineage", "7/7 survey rows",
                surveyLineage ? "7/7 survey rows" : "mismatch", surveyLineage,
                "Each entry retains one accepted survey row.");
            Add(result, "extension-player-visible-zero", "flags", "0",
                extension.Entries.Count(value => value.PlayerVisible).ToString(CultureInfo.InvariantCulture),
                extension.Entries.All(value => !value.PlayerVisible),
                "No player projection.");
            Add(result, "extension-developer-only-zero", "flags", "0",
                extension.Entries.Count(value => value.DeveloperOnly).ToString(CultureInfo.InvariantCulture),
                extension.Entries.All(value => !value.DeveloperOnly),
                "Neutral vocabulary entries are not diagnostics.");
            Add(result, "extension-runtime-implemented-zero", "flags", "0",
                extension.Entries.Count(value => value.RuntimeImplemented).ToString(CultureInfo.InvariantCulture),
                extension.Entries.All(value => !value.RuntimeImplemented),
                "No runtime implementation claim.");

            Add(result, "base-entry-count", "composition", "63",
                baseSnapshot.Entries.Count.ToString(CultureInfo.InvariantCulture),
                baseSnapshot.Entries.Count == 63, "Frozen E02 default.");
            Add(result, "base-category-counts", "composition",
                CountsText(BaseCategoryCounts), CountsText(CountCategories(baseSnapshot.Entries)),
                CountsEqual(BaseCategoryCounts, CountCategories(baseSnapshot.Entries)),
                "12/16/9/6/10/10.");
            Add(result, "base-legacy-count", "composition", "124",
                baseSnapshot.LegacyMappings.Count.ToString(CultureInfo.InvariantCulture),
                baseSnapshot.LegacyMappings.Count == 124, "Legacy mappings remain on the base.");
            Add(result, "base-canonical-unchanged", "composition",
                ExpectedBaseCanonical, baseBefore,
                baseBefore == ExpectedBaseCanonical, "E02 canonical remains byte-stable.");
            Add(result, "composed-entry-count", "composition", "70",
                composedSnapshot.Entries.Count.ToString(CultureInfo.InvariantCulture),
                composedSnapshot.Entries.Count == 70, "Explicit 63 + 7 composition.");
            Add(result, "composed-category-counts", "composition",
                CountsText(ComposedCategoryCounts), CountsText(CountCategories(composedSnapshot.Entries)),
                CountsEqual(ComposedCategoryCounts, CountCategories(composedSnapshot.Entries)),
                "17/16/9/8/10/10.");
            Add(result, "composed-legacy-count-unchanged", "composition", "124 (+0)",
                composedSnapshot.LegacyMappings.Count.ToString(CultureInfo.InvariantCulture) + " (+"
                    + (composedSnapshot.LegacyMappings.Count - baseSnapshot.LegacyMappings.Count)
                        .ToString(CultureInfo.InvariantCulture) + ")",
                composedSnapshot.LegacyMappings.Count == 124
                    && composedSnapshot.LegacyMappings.Count == baseSnapshot.LegacyMappings.Count,
                "The extension adds no legacy mapping.");
            Add(result, "composition-base-input-immutability", "immutability", baseBefore,
                Provider.CreateSnapshot(baseInput).BuildCanonicalSignature(),
                baseBefore == Provider.CreateSnapshot(baseInput).BuildCanonicalSignature(),
                "Composer did not mutate the explicit E02 input.");
            Add(result, "composition-extension-input-immutability", "immutability", extensionBefore,
                extension.CanonicalSignature, extensionBefore == extension.CanonicalSignature,
                "Composer did not mutate the extension input.");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> mutableEntries =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            List<string> mutableRows = new List<string> { "GAP-003" };
            EnemyMechanicVocabularyExtensionEntrySnapshot copiedRowEntry =
                new EnemyMechanicVocabularyExtensionEntrySnapshot(
                    "ba_gap_candidate.charge_attack",
                    EnemyVocabularyCategory.Mechanic,
                    "mechanic.charge_attack",
                    "冲撞攻击",
                    "敌方以突进或冲撞作为可识别攻击意图的中立机制概念。",
                    mutableRows,
                    false,
                    false,
                    false);
            mutableRows.Add("GAP-SYNTHETIC");
            EnemyMechanicVocabularyExtensionSnapshot copiedSnapshot = SnapshotWith(
                extension,
                mutableEntries);
            mutableEntries.Clear();
            bool defensiveCopy = copiedSnapshot.Entries.Count == 7
                && copiedRowEntry.SupportingSurveyRowIds.Count == 1;
            Add(result, "extension-defensive-copy", "immutability", "entries=7;rows=1",
                "entries=" + copiedSnapshot.Entries.Count.ToString(CultureInfo.InvariantCulture)
                    + ";rows=" + copiedRowEntry.SupportingSurveyRowIds.Count.ToString(CultureInfo.InvariantCulture),
                defensiveCopy, "Caller-owned lists cannot mutate snapshots.");

            bool entryReadOnly = RejectsListMutation(extension.Entries, extension.Entries[0]);
            bool rowReadOnly = RejectsListMutation(
                extension.Entries[0].SupportingSurveyRowIds,
                "GAP-SYNTHETIC");
            Add(result, "extension-read-only-collections", "immutability", "entries=true;rows=true",
                "entries=" + Bool(entryReadOnly) + ";rows=" + Bool(rowReadOnly),
                entryReadOnly && rowReadOnly, "Collections reject mutation.");
            Add(result, "extension-canonical-determinism", "canonical", extensionBefore,
                BoneAspectMechanicVocabularyExtensionCatalog.CreateSnapshot().CanonicalSignature,
                extensionBefore == BoneAspectMechanicVocabularyExtensionCatalog.CreateSnapshot().CanonicalSignature,
                "Repeated construction is deterministic.");
            Add(result, "composition-canonical-determinism", "canonical",
                composedSnapshot.BuildCanonicalSignature(),
                Provider.CreateSnapshot(Composer.Compose(
                    DefaultEnemyMechanicVocabularyCatalog.CreateInput(),
                    BoneAspectMechanicVocabularyExtensionCatalog.CreateSnapshot()))
                    .BuildCanonicalSignature(),
                composedSnapshot.BuildCanonicalSignature()
                    == Provider.CreateSnapshot(Composer.Compose(
                        DefaultEnemyMechanicVocabularyCatalog.CreateInput(),
                        BoneAspectMechanicVocabularyExtensionCatalog.CreateSnapshot()))
                        .BuildCanonicalSignature(),
                "Repeated explicit composition is deterministic.");

            EnemyMechanicVocabularySnapshot reverseComposed = Provider.CreateSnapshot(
                Composer.Compose(
                    DefaultEnemyMechanicVocabularyCatalog.CreateInput(true),
                    BoneAspectMechanicVocabularyExtensionCatalog.CreateSnapshot(true)));
            Add(result, "composition-reverse-input-determinism", "canonical",
                composedSnapshot.BuildCanonicalSignature(), reverseComposed.BuildCanonicalSignature(),
                composedSnapshot.BuildCanonicalSignature() == reverseComposed.BuildCanonicalSignature(),
                "Ordinal canonicalization is independent of input order.");

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
            string cultureExtension;
            string cultureComposition;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ar-SA");
                EnemyMechanicVocabularyExtensionSnapshot cultureSnapshot =
                    BoneAspectMechanicVocabularyExtensionCatalog.CreateSnapshot(true);
                cultureExtension = cultureSnapshot.CanonicalSignature;
                cultureComposition = Provider.CreateSnapshot(
                    Composer.Compose(
                        DefaultEnemyMechanicVocabularyCatalog.CreateInput(true),
                        cultureSnapshot)).BuildCanonicalSignature();
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }

            Add(result, "composition-culture-determinism", "canonical",
                extensionBefore + "|" + composedSnapshot.BuildCanonicalSignature(),
                cultureExtension + "|" + cultureComposition,
                extensionBefore == cultureExtension
                    && composedSnapshot.BuildCanonicalSignature() == cultureComposition,
                "Integer and string canonicalization are culture invariant.");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> changedExtensionEntries =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            changedExtensionEntries[0] = CopyExtensionEntry(
                changedExtensionEntries[0],
                label: changedExtensionEntries[0].DeveloperLabelZh + "SYNTHETIC");
            EnemyMechanicVocabularyExtensionSnapshot changedExtension =
                SnapshotWith(extension, changedExtensionEntries);
            Add(result, "extension-canonical-field-sensitivity", "canonical", "different",
                extension.CanonicalSignature == changedExtension.CanonicalSignature ? "same" : "different",
                extension.CanonicalSignature != changedExtension.CanonicalSignature,
                "A synthetic field mutation changes the extension signature.");

            List<EnemyVocabularyEntrySnapshot> changedBaseEntries = baseInput.Entries
                .Select(value => CopyBaseEntry(value)).ToList();
            changedBaseEntries[0] = CopyBaseEntry(
                changedBaseEntries[0],
                changedBaseEntries[0].DeveloperLabelZh + "SYNTHETIC");
            EnemyMechanicVocabularySnapshotInput changedBase = new EnemyMechanicVocabularySnapshotInput(
                changedBaseEntries,
                baseInput.LegacyMappings,
                baseInput.SchemaId,
                baseInput.SchemaVersion);
            string changedComposition = Provider.CreateSnapshot(
                Composer.Compose(changedBase, extension)).BuildCanonicalSignature();
            Add(result, "composition-canonical-field-sensitivity", "canonical", "different",
                composedSnapshot.BuildCanonicalSignature() == changedComposition ? "same" : "different",
                composedSnapshot.BuildCanonicalSignature() != changedComposition,
                "A synthetic base field mutation changes explicit composition.");

            RunNegativeFixtures(result, baseInput, extension);
            RunLeakAndScopeChecks(result, projectRoot);

            Dictionary<string, string> repeatA = BuildRepeatProbe(
                result,
                baseSnapshot,
                extension,
                composedSnapshot);
            Dictionary<string, string> repeatB = BuildRepeatProbe(
                result,
                baseSnapshot,
                extension,
                composedSnapshot);
            bool repeatDeterministic = repeatA.Count == repeatB.Count
                && repeatA.All(pair => repeatB.TryGetValue(pair.Key, out string value)
                    && value == pair.Value);
            Add(result, "report-repeat-determinism", "reports", "6/6 byte-identical",
                repeatDeterministic ? "6/6 byte-identical" : "mismatch",
                repeatDeterministic, "Two pure report projections are identical.");
            Add(result, "missing-report-regeneration", "reports", "6 unconditional generators",
                ReportPaths.Length.ToString(CultureInfo.InvariantCulture) + " unconditional generators",
                ReportPaths.Length == 6 && repeatA.Count == 6,
                "Every run rewrites all six reports from the same verified state.");

            RunIntegrityAndBaselineChecks(
                result,
                projectRoot,
                baseSnapshot,
                extension,
                composedSnapshot);
            bool checkSetExact = result.Rows.Count == CheckIds.Length
                && CheckIds.SequenceEqual(
                    result.Rows.Select(value => value.CheckId),
                    StringComparer.Ordinal);
            if (!checkSetExact)
            {
                throw new InvalidOperationException(
                    "Verifier check set/order mismatch. Expected 53 exact Assignment check IDs.");
            }

            return new VerificationContext(
                result,
                baseSnapshot,
                extension,
                composedSnapshot);
        }

        private static void RunNegativeFixtures(
            VerificationResult result,
            EnemyMechanicVocabularySnapshotInput baseInput,
            EnemyMechanicVocabularyExtensionSnapshot extension)
        {
            List<EnemyMechanicVocabularyExtensionEntrySnapshot> duplicate =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            duplicate.Add(CopyExtensionEntry(extension.Entries[0]));
            AddRejection(result, "reject-duplicate-extension-key",
                SnapshotWith(extension, duplicate), "EXTENSION_STABLE_KEY_DUPLICATE");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> collision =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            collision[0] = CopyExtensionEntry(collision[0], stableKey: "mechanic.basic_pressure");
            bool collisionRejected = ThrowsComposer(
                baseInput,
                SnapshotWith(extension, collision),
                "BASE_KEY_COLLISION");
            Add(result, "reject-base-key-collision", "negative", "BASE_KEY_COLLISION",
                collisionRejected ? "BASE_KEY_COLLISION" : "not rejected",
                collisionRejected, "Synthetic base collision never enters the catalog.");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> prefix =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            prefix[0] = CopyExtensionEntry(prefix[0], stableKey: "mechanic.synthetic_window",
                category: EnemyVocabularyCategory.CounterWindowType);
            AddRejection(result, "reject-prefix-category-mismatch",
                SnapshotWith(extension, prefix), "STABLE_KEY_PREFIX_OR_FORMAT_MISMATCH");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> eighth =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            eighth.Add(new EnemyMechanicVocabularyExtensionEntrySnapshot(
                "ba_gap_candidate.synthetic_eighth",
                EnemyVocabularyCategory.Mechanic,
                "mechanic.synthetic_eighth",
                "合成第八项",
                "仅用于负向校验。",
                new[] { "GAP-SYNTHETIC" },
                false,
                false,
                false));
            AddRejection(result, "reject-unapproved-eighth-entry",
                SnapshotWith(extension, eighth), "UNAPPROVED_ENTRY");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> missing =
                extension.Entries.Skip(1).Select(value => CopyExtensionEntry(value)).ToList();
            AddRejection(result, "reject-missing-required-entry",
                SnapshotWith(extension, missing), "REQUIRED_ENTRY_MISSING");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> unknownRow =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            unknownRow[0] = CopyExtensionEntry(
                unknownRow[0],
                surveyRows: new[] { "GAP-SYNTHETIC" });
            AddRejection(result, "reject-unknown-survey-row",
                SnapshotWith(extension, unknownRow), "UNKNOWN_SURVEY_ROW");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> lineage =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            lineage[0] = CopyExtensionEntry(
                lineage[0],
                label: lineage[0].DeveloperLabelZh + "SYNTHETIC");
            AddRejection(result, "reject-candidate-lineage-mismatch",
                SnapshotWith(extension, lineage), "CANDIDATE_LINEAGE_MISMATCH");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> nullEntry =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            nullEntry[0] = null;
            AddRejection(result, "reject-null-entry",
                SnapshotWith(extension, nullEntry), "ENTRY_NULL");

            EnemyMechanicVocabularyExtensionSnapshot schemaMismatch =
                new EnemyMechanicVocabularyExtensionSnapshot(
                    "EnemyMechanicVocabularyExtension.synthetic",
                    extension.SchemaVersion,
                    extension.ExtensionId,
                    extension.BaseSchemaId,
                    extension.BaseSchemaVersion,
                    extension.DevOnly,
                    extension.IsEnabled,
                    extension.EntersFormalFlow,
                    extension.RuntimeImplemented,
                    extension.Entries);
            AddRejection(result, "reject-schema-mismatch",
                schemaMismatch, "SCHEMA_ID_MISMATCH");

            EnemyMechanicVocabularyExtensionSnapshot unsafeFlags =
                new EnemyMechanicVocabularyExtensionSnapshot(
                    extension.SchemaId,
                    extension.SchemaVersion,
                    extension.ExtensionId,
                    extension.BaseSchemaId,
                    extension.BaseSchemaVersion,
                    false,
                    true,
                    true,
                    true,
                    extension.Entries);
            AddRejection(result, "reject-unsafe-flags",
                unsafeFlags, "EXTENSION_FLAGS_UNSAFE");

            List<EnemyMechanicVocabularyExtensionEntrySnapshot> playerVisible =
                extension.Entries.Select(value => CopyExtensionEntry(value)).ToList();
            playerVisible[0] = CopyExtensionEntry(playerVisible[0], playerVisible: true);
            AddRejection(result, "reject-player-visible-entry",
                SnapshotWith(extension, playerVisible), "PLAYER_VISIBLE_ENTRY_FORBIDDEN");
        }

        private static void RunLeakAndScopeChecks(VerificationResult result, string projectRoot)
        {
            HashSet<string> sourceSet = new HashSet<string>(
                SourceAndMetaManifest.Where(value => value.EndsWith(".cs", StringComparison.Ordinal)),
                StringComparer.Ordinal);
            string[] codeFiles = Directory.GetFiles(
                Absolute(projectRoot, "Assets/_Game/Scripts"),
                "*.cs",
                SearchOption.AllDirectories);
            string[] extensionTokens =
            {
                "EnemyMechanicVocabularyExtensionSchema",
                "EnemyMechanicVocabularyExtensionSnapshot",
                "extension.bone_aspect.mechanic_vocabulary_01"
            };
            List<string> runtimeConsumers = new List<string>();
            foreach (string file in codeFiles)
            {
                string relative = Relative(projectRoot, file);
                if (sourceSet.Contains(relative))
                {
                    continue;
                }

                string text = File.ReadAllText(file);
                if (extensionTokens.Any(token => text.IndexOf(token, StringComparison.Ordinal) >= 0))
                {
                    runtimeConsumers.Add(relative);
                }
            }

            Add(result, "no-runtime-consumer", "leak", "0", runtimeConsumers.Count.ToString(CultureInfo.InvariantCulture),
                runtimeConsumers.Count == 0,
                runtimeConsumers.Count == 0
                    ? "No runtime consumer outside the package."
                    : string.Join("|", runtimeConsumers.OrderBy(value => value, StringComparer.Ordinal)));

            string[] formalRoots =
            {
                "Assets/_Game/Scripts/TalismanBag/Items",
                "Assets/_Game/Scripts/TalismanBag/BuildSandbox",
                "Assets/_Game/Scripts/TalismanBag/CrossSystem",
                "Assets/_Game/Scenes",
                "Assets/_Game/Configs"
            };
            List<string> formalLeaks = new List<string>();
            foreach (string root in formalRoots)
            {
                string absoluteRoot = Absolute(projectRoot, root);
                if (!Directory.Exists(absoluteRoot))
                {
                    continue;
                }

                foreach (string file in Directory.GetFiles(absoluteRoot, "*", SearchOption.AllDirectories))
                {
                    if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string text;
                    try
                    {
                        text = File.ReadAllText(file);
                    }
                    catch
                    {
                        continue;
                    }

                    if (ExpectedStableKeys.Any(key => text.IndexOf(key, StringComparison.Ordinal) >= 0)
                        || text.IndexOf(
                            EnemyMechanicVocabularyExtensionSchema.ExtensionId,
                            StringComparison.Ordinal) >= 0)
                    {
                        formalLeaks.Add(Relative(projectRoot, file));
                    }
                }
            }

            Add(result, "no-formal-flow-consumer", "leak", "0",
                formalLeaks.Count.ToString(CultureInfo.InvariantCulture),
                formalLeaks.Count == 0,
                formalLeaks.Count == 0
                    ? "No Battle/Item/BuildSandbox/CrossSystem/Scene/Config binding."
                    : string.Join("|", formalLeaks.OrderBy(value => value, StringComparer.Ordinal)));

            string packageDirectory = Absolute(
                projectRoot,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension");
            string[] expectedPackageDirectoryFiles = SourceAndMetaManifest
                .Where(value => value.StartsWith(
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/BoneAspect/VocabularyExtension/",
                    StringComparison.Ordinal))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] actualPackageDirectoryFiles = Directory.Exists(packageDirectory)
                ? Directory.GetFiles(packageDirectory, "*", SearchOption.AllDirectories)
                    .Select(value => Relative(projectRoot, value))
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray()
                : Array.Empty<string>();
            bool sourcesPresent = SourceAndMetaManifest.All(
                value => File.Exists(Absolute(projectRoot, value)));
            bool exactDirectory = expectedPackageDirectoryFiles.SequenceEqual(
                actualPackageDirectoryFiles,
                StringComparer.Ordinal);
            string verifierSource = File.ReadAllText(Absolute(
                projectRoot,
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BoneAspectMechanicVocabularyExtensionVerifier.cs"));
            string packageSourceText = string.Join(
                "\n",
                SourceAndMetaManifest
                    .Where(value => value.EndsWith(".cs", StringComparison.Ordinal))
                    .Select(value => File.ReadAllText(Absolute(projectRoot, value))));
            string effectiveEntry = typeof(BoneAspectMechanicVocabularyExtensionVerifier).FullName
                + ".RunBatch";
            bool namespaceRegressionPass =
                verifierSource.IndexOf(
                    "namespace TalismanBag.EditorTools.EnemySystem",
                    StringComparison.Ordinal) >= 0
                && !Regex.IsMatch(
                    packageSourceText,
                    "namespace\\s+TalismanBag\\.Editor(?:\\.|\\s|\\{)",
                    RegexOptions.CultureInvariant)
                && string.Equals(
                    effectiveEntry,
                    "TalismanBag.EditorTools.EnemySystem.BoneAspectMechanicVocabularyExtensionVerifier.RunBatch",
                    StringComparison.Ordinal)
                && verifierSource.IndexOf(
                    "[MenuItem(\"TalismanBag/Enemy System/Run Bone Aspect Mechanic Vocabulary Extension Verifier\")]",
                    StringComparison.Ordinal) >= 0;
            bool manifestRegressionPass = PackageManifest.Length == 17
                && PackageManifest.Distinct(StringComparer.Ordinal).Count() == 17
                && PackageManifest.SequenceEqual(
                    PackageManifest.OrderBy(value => value, StringComparer.Ordinal),
                    StringComparer.Ordinal)
                && PackageManifestProjectionIsExact()
                && Regex.Matches(
                    verifierSource,
                    "builder\\.Append\\(BuildPackageManifestSection\\(\\)\\)",
                    RegexOptions.CultureInvariant).Count == 2;
            Add(result, "package-path-whitelist", "scope", "11 source/meta + 6 generated reports",
                sourcesPresent && exactDirectory && namespaceRegressionPass && manifestRegressionPass
                    ? "exact whitelist;namespace + report manifest regression PASS"
                    : "mismatch",
                sourcesPresent && exactDirectory && namespaceRegressionPass && manifestRegressionPass,
                "Assignment paths are exact/unique/Ordinal and projected to both Markdown reports; verifier resolves under TalismanBag.EditorTools.EnemySystem and introduces no TalismanBag.Editor namespace.");
        }

        private static void RunIntegrityAndBaselineChecks(
            VerificationResult result,
            string projectRoot,
            EnemyMechanicVocabularySnapshot baseSnapshot,
            EnemyMechanicVocabularyExtensionSnapshot extension,
            EnemyMechanicVocabularySnapshot composedSnapshot)
        {
            string[] allMetaFiles = Directory.GetFiles(
                Absolute(projectRoot, "Assets"),
                "*.meta",
                SearchOption.AllDirectories);
            Dictionary<string, List<string>> filesByGuid =
                new Dictionary<string, List<string>>(StringComparer.Ordinal);
            Regex guidPattern = new Regex(
                "(?m)^guid: ([0-9a-f]{32})$",
                RegexOptions.CultureInvariant);
            foreach (string file in allMetaFiles)
            {
                Match match = guidPattern.Match(File.ReadAllText(file));
                if (!match.Success)
                {
                    continue;
                }

                string guid = match.Groups[1].Value;
                if (!filesByGuid.TryGetValue(guid, out List<string> files))
                {
                    files = new List<string>();
                    filesByGuid.Add(guid, files);
                }

                files.Add(Relative(projectRoot, file));
            }

            int packageGuidCount = SourceAndMetaManifest
                .Where(value => value.EndsWith(".meta", StringComparison.Ordinal))
                .Count(value => guidPattern.IsMatch(File.ReadAllText(Absolute(projectRoot, value))));
            int duplicateGuidCount = filesByGuid.Count(pair => pair.Value.Count > 1);
            Add(result, "guid-uniqueness", "integrity", "package=6;duplicates=0",
                "package=" + packageGuidCount.ToString(CultureInfo.InvariantCulture)
                    + ";duplicates=" + duplicateGuidCount.ToString(CultureInfo.InvariantCulture),
                packageGuidCount == 6 && duplicateGuidCount == 0,
                "All package GUIDs are present and globally unique.");

            bool sourceTextIntegrity = SourceAndMetaManifest.All(
                value => HasTextIntegrity(File.ReadAllBytes(Absolute(projectRoot, value))));
            Dictionary<string, string> projectedReports = BuildRepeatProbe(
                result,
                baseSnapshot,
                extension,
                composedSnapshot);
            bool projectedReportIntegrity = projectedReports.Count == 6
                && projectedReports.Values.All(value =>
                    HasTextIntegrity(new UTF8Encoding(false).GetBytes(value)));
            bool textIntegrity = sourceTextIntegrity && projectedReportIntegrity;
            Add(result, "text-integrity", "integrity", "UTF-8/LF/no BOM/no trailing whitespace/final LF",
                textIntegrity ? "PASS" : "FAIL", textIntegrity,
                "11 source/meta files + 6 deterministic report projections checked.");

            AggregateResult e02 = AggregateFiles(projectRoot, E02Files);
            bool e02Pass = e02.Count == 14
                && e02.Hash == "b1ee6ea5a5e407b0f809de35d03e3325331824ed296272dcb886fae668d78166";
            Add(result, "protected-e02-baseline", "baseline",
                "14:b1ee6ea5a5e407b0f809de35d03e3325331824ed296272dcb886fae668d78166",
                e02.Count.ToString(CultureInfo.InvariantCulture) + ":" + e02.Hash,
                e02Pass, "Accepted E02 package remains byte-identical.");

            AggregateResult p0 = AggregateFiles(projectRoot, P0Files);
            AggregateResult p1 = AggregateFiles(projectRoot, P1Files);
            AggregateResult gap = AggregateFiles(projectRoot, GapSurveyFiles);
            AggregateResult art = AggregateFiles(projectRoot, BossArtFiles);
            bool upstreamPass =
                p0.Count == 8 && p0.Hash == "2ff9b38ebfba74650be776b2cc0bac362161901a870e2c89145e959cdf4e27c5"
                && p1.Count == 21 && p1.Hash == "21fbae0ab9dbfcaa65084cb63132cf6674534958a93beb6868113597933da209"
                && gap.Count == 8 && gap.Hash == "0a79e5e9a40cb8e88d0d25fc0db97972e03452cf3168878df98ed26871d9fca0"
                && art.Count == 9 && art.Hash == "93659d9b894251e73ad85a0d854c8a7c62d0f795c54ca38a5d3bf2065b841dc6";
            Add(result, "protected-upstream-baseline", "baseline",
                "P0=8;P1=21;Gap=8;BossArt=9 accepted",
                "P0=" + p0.Count + ";P1=" + p1.Count + ";Gap=" + gap.Count + ";BossArt=" + art.Count,
                upstreamPass,
                "All accepted Bone Aspect evidence packages remain byte-identical.");

            Dictionary<string, AggregateResult> broad = ComputeBroadAggregates(projectRoot);
            bool broadPass = BroadBaselines.All(pair =>
                broad.TryGetValue(pair.Key, out AggregateResult actual)
                && actual.Count == pair.Value.Count
                && actual.Hash == pair.Value.Hash);
            ExternalDriftStatus[] driftStatuses = ReadExternalDriftStatuses(projectRoot);
            bool driftEvidencePass = driftStatuses.All(value =>
                value.Evidence.Attributed && value.CurrentHashMatches);
            broadPass = broadPass && driftEvidencePass;
            string broadActual = string.Join(
                "|",
                broad.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => pair.Key + "=" + pair.Value.Count + ":" + pair.Value.Hash));
            Add(result, "protected-broad-baseline", "baseline", "12 task-start groups unchanged",
                broadPass
                    ? "11/12 unchanged;BuildSandbox=EXTERNAL_CONCURRENT_DRIFT_ATTRIBUTED"
                    : broadActual + "|drift="
                        + string.Join(
                            ";",
                            driftStatuses.Select(value => value.Evidence.Path + ":"
                                + (value.Evidence.Attributed ? value.Evidence.Owner : "PENDING")
                                + ":" + (value.CurrentHashMatches ? "HASH_MATCH" : "HASH_MISMATCH"))),
                broadPass,
                broadPass
                    ? "Frozen baseline preserved after explicit owner/hash attribution; no external file was restored or claimed."
                    : "Concurrent drift requires external owner attribution; verifier never restores it.");
        }

        private static Dictionary<string, AggregateResult> ComputeBroadAggregates(string projectRoot)
        {
            HashSet<string> additions = new HashSet<string>(
                SourceAndMetaManifest,
                StringComparer.Ordinal);
            Dictionary<string, AggregateResult> values =
                new Dictionary<string, AggregateResult>(StringComparer.Ordinal)
                {
                    {
                        "EnemySystem",
                        AggregateScope(
                            projectRoot,
                            "Assets/_Game/Scripts/TalismanBag/EnemySystem",
                            null,
                            additions)
                    },
                    {
                        "EditorEnemySystem",
                        AggregateScope(
                            projectRoot,
                            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem",
                            null,
                            additions)
                    },
                    { "Items", AggregateScope(projectRoot, "Assets/_Game/Scripts/TalismanBag/Items") },
                    {
                        "BuildSandbox",
                        AggregateScope(
                            projectRoot,
                            "Assets/_Game/Scripts/TalismanBag/BuildSandbox",
                            null,
                            BuildAttributedAdditionExclusions(projectRoot),
                            BuildAttributedHashOverrides(projectRoot))
                    },
                    { "CrossSystem", AggregateScope(projectRoot, "Assets/_Game/Scripts/TalismanBag/CrossSystem") },
                    { "Scenes", AggregateScope(projectRoot, "Assets/_Game/Scenes") },
                    {
                        "UnityAssets",
                        AggregateScope(projectRoot, "Assets", value =>
                            value.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    },
                    {
                        "Prefabs",
                        AggregateScope(projectRoot, "Assets", value =>
                            value.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    },
                    { "Configs", AggregateScope(projectRoot, "Assets/_Game/Configs") },
                    { "ProjectSettings", AggregateScope(projectRoot, "ProjectSettings") },
                    { "Packages", AggregateScope(projectRoot, "Packages") },
                    {
                        "GuardQueues",
                        AggregateFiles(
                            projectRoot,
                            new[]
                            {
                                "Docs/V0.4/EnemySystemGuard_CurrentRules.md",
                                "Docs/V0.4/ENEMY_SYSTEM_PACKAGE_QUEUE.md",
                                "Docs/V0.4/BUILD_PACKAGE_QUEUE.md",
                                "Docs/V0.4/BUILD_PHASE2_PACKAGE_QUEUE.md"
                            })
                    }
                };
            return values;
        }

        private static Dictionary<string, string> BuildRepeatProbe(
            VerificationResult result,
            EnemyMechanicVocabularySnapshot baseSnapshot,
            EnemyMechanicVocabularyExtensionSnapshot extension,
            EnemyMechanicVocabularySnapshot composedSnapshot)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { DetailReportPath, BuildDetailReport(result, baseSnapshot, extension, composedSnapshot) },
                { SpecCsvPath, BuildSpecCsv(result) },
                { InventoryCsvPath, BuildInventoryCsv(extension) },
                { SourceEvidenceCsvPath, BuildSourceEvidenceCsv(extension) },
                { CompositionCsvPath, BuildCompositionCsv(baseSnapshot, extension, composedSnapshot) },
                { LeakReportPath, BuildLeakReport(result, baseSnapshot, extension, composedSnapshot) }
            };
        }

        private static void WriteReports(string projectRoot, VerificationContext context)
        {
            Dictionary<string, string> reports = BuildRepeatProbe(
                context.Result,
                context.BaseSnapshot,
                context.Extension,
                context.ComposedSnapshot);
            foreach (string path in ReportPaths)
            {
                WriteUtf8(Absolute(projectRoot, path), reports[path]);
            }
        }

        private static string BuildDetailReport(
            VerificationResult result,
            EnemyMechanicVocabularySnapshot baseSnapshot,
            EnemyMechanicVocabularyExtensionSnapshot extension,
            EnemyMechanicVocabularySnapshot composedSnapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Bone Aspect Mechanic Vocabulary Extension Report");
            builder.AppendLine();
            builder.AppendLine("- Package: `V0.4-BoneAspectMechanicVocabularyExtension01`");
            builder.AppendLine("- Status: `" + (result.Passed ? "QA_PASS" : "QA_FAIL") + "`");
            builder.AppendLine("- Verification: `" + result.PassedCount + "/" + result.Rows.Count + " PASS`");
            builder.AppendLine("- Execution modes: `OFFLINE_SAME_SOURCE`, `UNITY_BATCH`");
            builder.AppendLine("- Unity scene handtest: `NOT_REQUIRED`");
            builder.AppendLine("- Runtime consumer: `NONE`");
            builder.AppendLine("- Formal-flow consumer: `NONE`");
            builder.AppendLine("- `devOnly=true / isEnabled=false / entersFormalFlow=false / runtimeImplemented=false`");
            builder.AppendLine();
            builder.AppendLine("## Canonical Signatures");
            builder.AppendLine();
            builder.AppendLine("- BaseCanonicalSignature: `" + baseSnapshot.BuildCanonicalSignature() + "`");
            builder.AppendLine("- ExtensionCanonicalSignature: `" + extension.CanonicalSignature + "`");
            builder.AppendLine("- ComposedCanonicalSignature: `" + composedSnapshot.BuildCanonicalSignature() + "`");
            builder.AppendLine("- The composed signature is an explicit read-only composition result, not a new global E02 default.");
            builder.AppendLine();
            builder.AppendLine("## Composition");
            builder.AppendLine();
            builder.AppendLine("- Entries: `63 + 7 = 70`");
            builder.AppendLine("- Legacy mappings: `124 + 0 = 124`");
            builder.AppendLine("- Extension categories: `Mechanic=5`, `CounterWindowType=2`");
            builder.AppendLine("- BA-D3 / BA-D4: `USER_DECISION_REQUIRED / NOT_SELECTED`");
            builder.AppendLine("- BA-D1 / BA-D2 runtime: `OUT_OF_SCOPE`");
            builder.AppendLine();
            builder.AppendLine("## Boundary");
            builder.AppendLine();
            builder.AppendLine("The package adds only neutral vocabulary identities. It does not bind Carrier, Skill, Phase,");
            builder.AppendLine("Pressure, Readiness, Battle, Item, BuildSandbox, CrossSystem, Scene, Prefab, UI, or formal flow.");
            builder.AppendLine("It does not consume Bone Hound prototype state or presentation semantics.");
            builder.AppendLine();
            builder.AppendLine("## External Concurrent Drift");
            builder.AppendLine();
            foreach (ExternalDriftEvidence drift in ExternalDrifts)
            {
                builder.AppendLine("- `" + drift.Path + "`: `"
                    + (drift.Attributed ? "ATTRIBUTED" : "PENDING_RECEIPT")
                    + "` — " + drift.Owner + "; start=`"
                    + (drift.StartHash ?? "ABSENT") + "`; current=`" + drift.CurrentHash + "`.");
            }

            builder.AppendLine("- These files are outside package ownership and were not restored, formatted, modified, or claimed.");
            builder.AppendLine();
            builder.Append(BuildPackageManifestSection());
            builder.AppendLine();
            builder.Append(BuildUnityEntryCorrectionSection());
            builder.AppendLine();
            builder.AppendLine("## Verification Summary");
            builder.AppendLine();
            foreach (CheckRow row in result.Rows)
            {
                builder.AppendLine("- `" + row.CheckId + "`: `" + (row.Passed ? "PASS" : "FAIL") + "` — " + row.Notes);
            }

            return NormalizeText(builder.ToString());
        }

        private static string BuildSpecCsv(VerificationResult result)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("\"checkId\",\"area\",\"expected\",\"actual\",\"status\",\"notes\"");
            foreach (CheckRow row in result.Rows)
            {
                builder.AppendLine(
                    Csv(row.CheckId) + "," + Csv(row.Area) + "," + Csv(row.Expected) + ","
                    + Csv(row.Actual) + "," + Csv(row.Passed ? "PASS" : "FAIL") + ","
                    + Csv(row.Notes));
            }

            return NormalizeText(builder.ToString());
        }

        private static string BuildInventoryCsv(
            EnemyMechanicVocabularyExtensionSnapshot extension)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("\"schemaId\",\"schemaVersion\",\"extensionId\",\"baseSchemaId\",\"baseSchemaVersion\",\"candidateId\",\"category\",\"stableKey\",\"developerLabelZh\",\"description\",\"supportingSurveyRowIds\",\"playerVisible\",\"developerOnly\",\"runtimeImplemented\",\"devOnly\",\"isEnabled\",\"entersFormalFlow\",\"extensionCanonicalSignature\"");
            foreach (EnemyMechanicVocabularyExtensionEntrySnapshot entry in extension.Entries)
            {
                builder.AppendLine(
                    Csv(extension.SchemaId) + ","
                    + Csv(extension.SchemaVersion.ToString(CultureInfo.InvariantCulture)) + ","
                    + Csv(extension.ExtensionId) + ","
                    + Csv(extension.BaseSchemaId) + ","
                    + Csv(extension.BaseSchemaVersion.ToString(CultureInfo.InvariantCulture)) + ","
                    + Csv(entry.CandidateId) + ","
                    + Csv(EnemyVocabularyCategoryNames.StableName(entry.Category)) + ","
                    + Csv(entry.StableKey) + ","
                    + Csv(entry.DeveloperLabelZh) + ","
                    + Csv(entry.Description) + ","
                    + Csv(string.Join("|", entry.SupportingSurveyRowIds)) + ","
                    + Csv(Bool(entry.PlayerVisible)) + ","
                    + Csv(Bool(entry.DeveloperOnly)) + ","
                    + Csv(Bool(entry.RuntimeImplemented)) + ","
                    + Csv(Bool(extension.DevOnly)) + ","
                    + Csv(Bool(extension.IsEnabled)) + ","
                    + Csv(Bool(extension.EntersFormalFlow)) + ","
                    + Csv(extension.CanonicalSignature));
            }

            return NormalizeText(builder.ToString());
        }

        private static string BuildSourceEvidenceCsv(
            EnemyMechanicVocabularyExtensionSnapshot extension)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("\"candidateId\",\"stableKey\",\"surveyRowId\",\"candidateSheetPath\",\"surveyMatrixPath\",\"assignmentPath\",\"lineageStatus\",\"runtimeBinding\"");
            foreach (EnemyMechanicVocabularyExtensionEntrySnapshot entry in extension.Entries)
            {
                builder.AppendLine(
                    Csv(entry.CandidateId) + ","
                    + Csv(entry.StableKey) + ","
                    + Csv(string.Join("|", entry.SupportingSurveyRowIds)) + ","
                    + Csv(CandidateSheetPath) + ","
                    + Csv(SurveyMatrixPath) + ","
                    + Csv(AssignmentPath) + ","
                    + Csv("GUARD_APPROVED_EXACT_LINEAGE") + ","
                    + Csv("NONE"));
            }

            return NormalizeText(builder.ToString());
        }

        private static string BuildCompositionCsv(
            EnemyMechanicVocabularySnapshot baseSnapshot,
            EnemyMechanicVocabularyExtensionSnapshot extension,
            EnemyMechanicVocabularySnapshot composedSnapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("\"rowKind\",\"category\",\"baseCount\",\"extensionCount\",\"composedCount\",\"baseCanonicalSignature\",\"extensionCanonicalSignature\",\"composedCanonicalSignature\",\"globalDefaultReplaced\"");
            foreach (EnemyVocabularyCategory category in Enum.GetValues(typeof(EnemyVocabularyCategory)))
            {
                int baseCount = baseSnapshot.Entries.Count(value => value.Category == category);
                int extensionCount = extension.Entries.Count(value => value.Category == category);
                int composedCount = composedSnapshot.Entries.Count(value => value.Category == category);
                builder.AppendLine(
                    Csv("CATEGORY") + ","
                    + Csv(EnemyVocabularyCategoryNames.StableName(category)) + ","
                    + Csv(baseCount.ToString(CultureInfo.InvariantCulture)) + ","
                    + Csv(extensionCount.ToString(CultureInfo.InvariantCulture)) + ","
                    + Csv(composedCount.ToString(CultureInfo.InvariantCulture)) + ","
                    + Csv(baseSnapshot.BuildCanonicalSignature()) + ","
                    + Csv(extension.CanonicalSignature) + ","
                    + Csv(composedSnapshot.BuildCanonicalSignature()) + ","
                    + Csv("false"));
            }

            builder.AppendLine(
                Csv("TOTAL_ENTRIES") + "," + Csv("ALL") + ","
                + Csv(baseSnapshot.Entries.Count.ToString(CultureInfo.InvariantCulture)) + ","
                + Csv(extension.Entries.Count.ToString(CultureInfo.InvariantCulture)) + ","
                + Csv(composedSnapshot.Entries.Count.ToString(CultureInfo.InvariantCulture)) + ","
                + Csv(baseSnapshot.BuildCanonicalSignature()) + ","
                + Csv(extension.CanonicalSignature) + ","
                + Csv(composedSnapshot.BuildCanonicalSignature()) + ","
                + Csv("false"));
            builder.AppendLine(
                Csv("LEGACY_MAPPINGS") + "," + Csv("ALL") + ","
                + Csv(baseSnapshot.LegacyMappings.Count.ToString(CultureInfo.InvariantCulture)) + ","
                + Csv("0") + ","
                + Csv(composedSnapshot.LegacyMappings.Count.ToString(CultureInfo.InvariantCulture)) + ","
                + Csv(baseSnapshot.BuildCanonicalSignature()) + ","
                + Csv(extension.CanonicalSignature) + ","
                + Csv(composedSnapshot.BuildCanonicalSignature()) + ","
                + Csv("false"));
            return NormalizeText(builder.ToString());
        }

        private static string BuildLeakReport(
            VerificationResult result,
            EnemyMechanicVocabularySnapshot baseSnapshot,
            EnemyMechanicVocabularyExtensionSnapshot extension,
            EnemyMechanicVocabularySnapshot composedSnapshot)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Bone Aspect Mechanic Vocabulary Extension Leak Check");
            builder.AppendLine();
            builder.AppendLine("- Overall: `" + (result.Passed ? "PASS" : "FAIL") + "`");
            builder.AppendLine("- Checks: `" + result.PassedCount + "/" + result.Rows.Count + " PASS`");
            builder.AppendLine("- Package files: `17 additions / 0 existing modifications`");
            builder.AppendLine("- Runtime/formal-flow consumers: `0 / 0`");
            builder.AppendLine("- Global E02 default replacement: `false`");
            builder.AppendLine("- BA-D3 / BA-D4: `USER_DECISION_REQUIRED / NOT_SELECTED`");
            builder.AppendLine("- Base canonical: `" + baseSnapshot.BuildCanonicalSignature() + "`");
            builder.AppendLine("- Extension canonical: `" + extension.CanonicalSignature + "`");
            builder.AppendLine("- Composed canonical: `" + composedSnapshot.BuildCanonicalSignature() + "`");
            builder.AppendLine();
            builder.AppendLine("The explicit composer returns a new input and never stores it as a global default.");
            builder.AppendLine("Any concurrent presentation/BuildSandbox/Scene drift remains external and is never restored or claimed.");
            builder.AppendLine();
            builder.AppendLine("## External Concurrent Drift Evidence");
            builder.AppendLine();
            foreach (ExternalDriftEvidence drift in ExternalDrifts)
            {
                builder.AppendLine("- `" + drift.Path + "`: `"
                    + (drift.Attributed ? "ATTRIBUTED" : "PENDING_RECEIPT")
                    + "` / " + drift.Owner + " / start=`"
                    + (drift.StartHash ?? "ABSENT") + "` / current=`" + drift.CurrentHash + "`");
            }

            builder.AppendLine();
            builder.Append(BuildPackageManifestSection());
            builder.AppendLine();
            builder.Append(BuildUnityEntryCorrectionSection());
            builder.AppendLine();
            builder.AppendLine("## Checks");
            builder.AppendLine();
            foreach (CheckRow row in result.Rows)
            {
                builder.AppendLine("- `" + row.CheckId + "`: `" + (row.Passed ? "PASS" : "FAIL") + "`");
            }

            return NormalizeText(builder.ToString());
        }

        private static string BuildPackageManifestSection()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("## Package Manifest");
            builder.AppendLine();
            builder.AppendLine("- Scope: `additions=17 / existing modifications=0`");
            builder.AppendLine("- Additions: `17`");
            builder.AppendLine("- Existing task-start files modified by this package: `0`");
            builder.AppendLine("- Exact paths below use `StringComparer.Ordinal` order:");
            foreach (string path in PackageManifest)
            {
                builder.AppendLine("- `" + path + "`");
            }

            return NormalizeText(builder.ToString());
        }

        private static bool PackageManifestProjectionIsExact()
        {
            string section = BuildPackageManifestSection();
            string[] projectedPaths = section.Split('\n')
                .Where(value => value.StartsWith("- `", StringComparison.Ordinal)
                    && value.EndsWith("`", StringComparison.Ordinal))
                .Select(value => value.Substring(3, value.Length - 4))
                .ToArray();
            return section.IndexOf("- Additions: `17`", StringComparison.Ordinal) >= 0
                && section.IndexOf(
                    "- Scope: `additions=17 / existing modifications=0`",
                    StringComparison.Ordinal) >= 0
                && section.IndexOf(
                    "- Existing task-start files modified by this package: `0`",
                    StringComparison.Ordinal) >= 0
                && projectedPaths.SequenceEqual(PackageManifest, StringComparer.Ordinal);
        }

        private static string BuildUnityEntryCorrectionSection()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("## Unity Entry Guard Correction");
            builder.AppendLine();
            builder.AppendLine("- Historical defective Assignment entry (preserved as evidence): `TalismanBag.Editor.EnemySystem.BoneAspectMechanicVocabularyExtensionVerifier.RunBatch`");
            builder.AppendLine("- Effective compile-safe entry: `TalismanBag.EditorTools.EnemySystem.BoneAspectMechanicVocabularyExtensionVerifier.RunBatch`");
            builder.AppendLine("- Guard reason: prevents creation of sibling namespace `TalismanBag.Editor` and matches all established Enemy verifier namespaces.");
            builder.AppendLine("- Menu item remains: `TalismanBag/Enemy System/Run Bone Aspect Mechanic Vocabulary Extension Verifier`");
            return NormalizeText(builder.ToString());
        }

        private static IReadOnlyList<ExpectedDefinition> ExpectedDefinitions()
        {
            return new[]
            {
                new ExpectedDefinition(
                    "ba_gap_candidate.charge_attack", "Mechanic", "mechanic.charge_attack",
                    "冲撞攻击", "敌方以突进或冲撞作为可识别攻击意图的中立机制概念。", "GAP-003"),
                new ExpectedDefinition(
                    "ba_gap_candidate.contested_mark", "Mechanic", "mechanic.contested_mark",
                    "争夺标记", "敌方围绕某目标建立可争夺关系标记的中立机制概念，不定义资源结算。", "GAP-015"),
                new ExpectedDefinition(
                    "ba_gap_candidate.damage_reduction", "Mechanic", "mechanic.damage_reduction",
                    "减伤", "敌方以非护盾方式降低承伤的中立机制概念。", "GAP-016"),
                new ExpectedDefinition(
                    "ba_gap_candidate.possession_state", "Mechanic", "mechanic.possession_state",
                    "附身状态", "敌方进入或施加附身/寄宿关系状态的中立机制概念。", "GAP-001"),
                new ExpectedDefinition(
                    "ba_gap_candidate.status_stack", "Mechanic", "mechanic.status_stack",
                    "状态叠加", "同类敌方状态可累积层数或强度的中立机制概念，不指定状态类型。", "GAP-005"),
                new ExpectedDefinition(
                    "ba_gap_candidate.recognition_reveal_window", "CounterWindowType",
                    "counter_window.recognition_reveal", "识破后显露窗口",
                    "玩家完成识破后出现显露或失防窗口的中立反制类别，不预设净化。", "GAP-010"),
                new ExpectedDefinition(
                    "ba_gap_candidate.weakpoint_exposure_window", "CounterWindowType",
                    "counter_window.weakpoint_exposure", "弱点暴露窗口",
                    "敌方弱点或核心进入可暴露窗口的中立反制类别，不预设触发源。", "GAP-039")
            };
        }

        private static bool CandidateSheetHas(
            string candidateSheet,
            EnemyMechanicVocabularyExtensionEntrySnapshot entry)
        {
            string prefix = Csv(entry.CandidateId) + ",";
            string line = candidateSheet.Split('\n')
                .FirstOrDefault(value => value.StartsWith(prefix, StringComparison.Ordinal));
            return line != null
                && line.IndexOf(Csv(entry.DeveloperLabelZh), StringComparison.Ordinal) >= 0
                && line.IndexOf(Csv(entry.Description), StringComparison.Ordinal) >= 0
                && line.IndexOf(
                    Csv(EnemyVocabularyCategoryNames.StableName(entry.Category)),
                    StringComparison.Ordinal) >= 0
                && entry.SupportingSurveyRowIds.All(
                    row => line.IndexOf(Csv(row), StringComparison.Ordinal) >= 0);
        }

        private static bool SurveyMatrixHas(string matrix, string rowId, string candidateId)
        {
            string prefix = Csv(rowId) + ",";
            string line = matrix.Split('\n')
                .FirstOrDefault(value => value.StartsWith(prefix, StringComparison.Ordinal));
            return line != null
                && line.IndexOf(Csv(candidateId), StringComparison.Ordinal) >= 0;
        }

        private static EnemyMechanicVocabularyExtensionSnapshot SnapshotWith(
            EnemyMechanicVocabularyExtensionSnapshot source,
            IReadOnlyList<EnemyMechanicVocabularyExtensionEntrySnapshot> entries)
        {
            return new EnemyMechanicVocabularyExtensionSnapshot(
                source.SchemaId,
                source.SchemaVersion,
                source.ExtensionId,
                source.BaseSchemaId,
                source.BaseSchemaVersion,
                source.DevOnly,
                source.IsEnabled,
                source.EntersFormalFlow,
                source.RuntimeImplemented,
                entries);
        }

        private static EnemyMechanicVocabularyExtensionEntrySnapshot CopyExtensionEntry(
            EnemyMechanicVocabularyExtensionEntrySnapshot value,
            string stableKey = null,
            string label = null,
            IReadOnlyList<string> surveyRows = null,
            EnemyVocabularyCategory? category = null,
            bool? playerVisible = null)
        {
            return new EnemyMechanicVocabularyExtensionEntrySnapshot(
                value.CandidateId,
                category ?? value.Category,
                stableKey ?? value.StableKey,
                label ?? value.DeveloperLabelZh,
                value.Description,
                surveyRows ?? value.SupportingSurveyRowIds,
                playerVisible ?? value.PlayerVisible,
                value.DeveloperOnly,
                value.RuntimeImplemented);
        }

        private static EnemyVocabularyEntrySnapshot CopyBaseEntry(
            EnemyVocabularyEntrySnapshot value,
            string label = null)
        {
            EnemyVocabularyStableKey key;
            switch (value.Category)
            {
                case EnemyVocabularyCategory.Mechanic:
                    key = new MechanicKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.BuildCapability:
                    key = new BuildCapabilityKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.PressureChannel:
                    key = new PressureChannelKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.CounterWindowType:
                    key = new CounterWindowTypeKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.PlayerHintCategory:
                    key = new PlayerHintCategoryKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.DeveloperDiagnosticCategory:
                    key = new DeveloperDiagnosticCategoryKey(value.StableKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new EnemyVocabularyEntrySnapshot(
                key,
                label ?? value.DeveloperLabelZh,
                value.Description,
                value.PlayerVisible,
                value.DeveloperOnly);
        }

        private static void AddRejection(
            VerificationResult result,
            string checkId,
            EnemyMechanicVocabularyExtensionSnapshot snapshot,
            string expectedCode)
        {
            IReadOnlyList<EnemyMechanicVocabularyExtensionValidationIssue> issues =
                ExtensionValidator.Validate(snapshot);
            bool passed = issues.Any(value => value.Code == expectedCode);
            Add(result, checkId, "negative", expectedCode,
                passed ? expectedCode : string.Join("|", issues.Select(value => value.Code)),
                passed, "Synthetic fixture is rejected and never enters the catalog.");
        }

        private static bool ThrowsComposer(
            EnemyMechanicVocabularySnapshotInput baseInput,
            EnemyMechanicVocabularyExtensionSnapshot extension,
            string code)
        {
            try
            {
                Composer.Compose(baseInput, extension);
                return false;
            }
            catch (EnemyMechanicVocabularyExtensionValidationException exception)
            {
                return exception.Issues.Any(value => value.Code == code);
            }
        }

        private static bool RejectsListMutation<T>(IReadOnlyList<T> values, T injected)
        {
            IList<T> list = values as IList<T>;
            if (list == null)
            {
                return true;
            }

            try
            {
                list.Add(injected);
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static Dictionary<EnemyVocabularyCategory, int> CountCategories(
            IReadOnlyList<EnemyVocabularyEntrySnapshot> entries)
        {
            return Enum.GetValues(typeof(EnemyVocabularyCategory))
                .Cast<EnemyVocabularyCategory>()
                .ToDictionary(
                    category => category,
                    category => entries.Count(value => value.Category == category));
        }

        private static bool CountsEqual(
            IReadOnlyDictionary<EnemyVocabularyCategory, int> expected,
            IReadOnlyDictionary<EnemyVocabularyCategory, int> actual)
        {
            return expected.Count == actual.Count
                && expected.All(pair => actual.TryGetValue(pair.Key, out int value)
                    && value == pair.Value);
        }

        private static string CountsText(
            IReadOnlyDictionary<EnemyVocabularyCategory, int> counts)
        {
            return string.Join(
                "/",
                Enum.GetValues(typeof(EnemyVocabularyCategory))
                    .Cast<EnemyVocabularyCategory>()
                    .Select(category => counts.TryGetValue(category, out int count)
                        ? count.ToString(CultureInfo.InvariantCulture)
                        : "0"));
        }

        private static string CategorySummary(
            IReadOnlyList<EnemyMechanicVocabularyExtensionEntrySnapshot> entries)
        {
            return string.Join(
                "|",
                entries.GroupBy(value => value.Category)
                    .OrderBy(
                        group => EnemyVocabularyCategoryNames.StableName(group.Key),
                        StringComparer.Ordinal)
                    .Select(group => EnemyVocabularyCategoryNames.StableName(group.Key)
                        + "=" + group.Count().ToString(CultureInfo.InvariantCulture)));
        }

        private static AggregateResult AggregateScope(
            string projectRoot,
            string relativeRoot,
            Func<string, bool> filter = null,
            ISet<string> exclusions = null,
            IReadOnlyDictionary<string, string> hashOverrides = null)
        {
            string root = Absolute(projectRoot, relativeRoot);
            if (!Directory.Exists(root))
            {
                return AggregateRows(Array.Empty<KeyValuePair<string, string>>());
            }

            List<KeyValuePair<string, string>> rows = new List<KeyValuePair<string, string>>();
            foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                string relative = Relative(projectRoot, file);
                if ((filter != null && !filter(relative))
                    || (exclusions != null && exclusions.Contains(relative)))
                {
                    continue;
                }

                string hash = hashOverrides != null
                    && hashOverrides.TryGetValue(relative, out string baselineHash)
                        ? baselineHash
                        : Sha256File(file);
                rows.Add(new KeyValuePair<string, string>(relative, hash));
            }

            return AggregateRows(rows);
        }

        private static HashSet<string> BuildAttributedAdditionExclusions(string projectRoot)
        {
            return new HashSet<string>(
                ReadExternalDriftStatuses(projectRoot)
                    .Where(value => value.Evidence.Attributed
                        && value.CurrentHashMatches
                        && value.Evidence.StartHash == null)
                    .Select(value => value.Evidence.Path),
                StringComparer.Ordinal);
        }

        private static IReadOnlyDictionary<string, string> BuildAttributedHashOverrides(
            string projectRoot)
        {
            return ReadExternalDriftStatuses(projectRoot)
                .Where(value => value.Evidence.Attributed
                    && value.CurrentHashMatches
                    && value.Evidence.StartHash != null)
                .ToDictionary(
                    value => value.Evidence.Path,
                    value => value.Evidence.StartHash,
                    StringComparer.Ordinal);
        }

        private static ExternalDriftStatus[] ReadExternalDriftStatuses(string projectRoot)
        {
            return ExternalDrifts.Select(value =>
            {
                string absolute = Absolute(projectRoot, value.Path);
                string current = File.Exists(absolute) ? Sha256File(absolute) : null;
                return new ExternalDriftStatus(
                    value,
                    string.Equals(current, value.CurrentHash, StringComparison.Ordinal));
            }).ToArray();
        }

        private static AggregateResult AggregateFiles(
            string projectRoot,
            IEnumerable<string> relativePaths)
        {
            List<KeyValuePair<string, string>> rows = new List<KeyValuePair<string, string>>();
            foreach (string relativePath in relativePaths.OrderBy(
                value => value,
                StringComparer.Ordinal))
            {
                string absolute = Absolute(projectRoot, relativePath);
                if (!File.Exists(absolute))
                {
                    rows.Add(new KeyValuePair<string, string>(relativePath, "<missing>"));
                }
                else
                {
                    rows.Add(new KeyValuePair<string, string>(relativePath, Sha256File(absolute)));
                }
            }

            return AggregateRows(rows);
        }

        private static AggregateResult AggregateRows(
            IEnumerable<KeyValuePair<string, string>> sourceRows)
        {
            KeyValuePair<string, string>[] rows = sourceRows
                .OrderBy(value => value.Key, StringComparer.Ordinal)
                .ToArray();
            string payload = string.Join(
                "\n",
                rows.Select(value => value.Key + "\0" + value.Value.ToLowerInvariant()));
            return new AggregateResult(rows.Length, Sha256Text(payload));
        }

        private static bool HasTextIntegrity(byte[] bytes)
        {
            if (bytes.Length == 0
                || (bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf)
                || bytes[bytes.Length - 1] != (byte)'\n'
                || bytes.Any(value => value == (byte)'\r'))
            {
                return false;
            }

            string text = new UTF8Encoding(false, true).GetString(bytes);
            return text.Split('\n')
                .Take(Math.Max(0, text.Split('\n').Length - 1))
                .All(line => line.Length == line.TrimEnd(' ', '\t').Length);
        }

        private static string Sha256File(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return Hex(sha256.ComputeHash(stream));
            }
        }

        private static string Sha256Text(string text)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Hex(sha256.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty)));
            }
        }

        private static string Hex(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
            {
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static string Absolute(string projectRoot, string relativePath)
        {
            return Path.Combine(
                projectRoot,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string Relative(string projectRoot, string absolutePath)
        {
            string root = Path.GetFullPath(projectRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(absolutePath);
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Path is outside the project root: " + full);
            }

            return full.Substring(root.Length).Replace('\\', '/');
        }

        private static string FindProjectRoot()
        {
#if UNITY_EDITOR
            string current = Path.GetDirectoryName(Application.dataPath);
#else
            string current = Directory.GetCurrentDirectory();
#endif
            while (!string.IsNullOrEmpty(current))
            {
                if (Directory.Exists(Path.Combine(current, "Assets"))
                    && Directory.Exists(Path.Combine(current, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(current, "Packages")))
                {
                    return current;
                }

                DirectoryInfo parent = Directory.GetParent(current);
                current = parent == null ? null : parent.FullName;
            }

            throw new DirectoryNotFoundException("Could not locate the Unity project root.");
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        private static string NormalizeText(string text)
        {
            string normalized = (text ?? string.Empty)
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .TrimEnd('\n');
            return normalized + "\n";
        }

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, NormalizeText(content), new UTF8Encoding(false));
        }

        private static void Add(
            VerificationResult result,
            string checkId,
            string area,
            string expected,
            string actual,
            bool passed,
            string notes)
        {
            result.Add(checkId, area, expected, actual, passed, notes);
        }

        private static void Log(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#else
            Console.WriteLine(message);
#endif
        }

        private static void LogError(string message)
        {
#if UNITY_EDITOR
            Debug.LogError(message);
#else
            Console.Error.WriteLine(message);
#endif
        }

        private sealed class VerificationContext
        {
            public VerificationContext(
                VerificationResult result,
                EnemyMechanicVocabularySnapshot baseSnapshot,
                EnemyMechanicVocabularyExtensionSnapshot extension,
                EnemyMechanicVocabularySnapshot composedSnapshot)
            {
                Result = result;
                BaseSnapshot = baseSnapshot;
                Extension = extension;
                ComposedSnapshot = composedSnapshot;
            }

            public VerificationResult Result { get; }
            public EnemyMechanicVocabularySnapshot BaseSnapshot { get; }
            public EnemyMechanicVocabularyExtensionSnapshot Extension { get; }
            public EnemyMechanicVocabularySnapshot ComposedSnapshot { get; }
        }

        private sealed class VerificationResult
        {
            private readonly List<CheckRow> rows = new List<CheckRow>();
            public IReadOnlyList<CheckRow> Rows => rows;
            public int PassedCount => rows.Count(value => value.Passed);
            public bool Passed => rows.Count == CheckIds.Length
                && rows.All(value => value.Passed);

            public void Add(
                string checkId,
                string area,
                string expected,
                string actual,
                bool passed,
                string notes)
            {
                rows.Add(new CheckRow(
                    checkId,
                    area,
                    expected,
                    actual,
                    passed,
                    notes));
            }
        }

        private sealed class CheckRow
        {
            public CheckRow(
                string checkId,
                string area,
                string expected,
                string actual,
                bool passed,
                string notes)
            {
                CheckId = checkId;
                Area = area;
                Expected = expected;
                Actual = actual;
                Passed = passed;
                Notes = notes;
            }

            public string CheckId { get; }
            public string Area { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
            public string Notes { get; }
        }

        private sealed class ExpectedDefinition
        {
            public ExpectedDefinition(
                string candidateId,
                string category,
                string stableKey,
                string label,
                string description,
                string surveyRow)
            {
                CandidateId = candidateId;
                Category = category;
                StableKey = stableKey;
                Label = label;
                Description = description;
                SurveyRow = surveyRow;
            }

            public string CandidateId { get; }
            public string Category { get; }
            public string StableKey { get; }
            public string Label { get; }
            public string Description { get; }
            public string SurveyRow { get; }
        }

        private sealed class BaselineExpectation
        {
            public BaselineExpectation(int count, string hash)
            {
                Count = count;
                Hash = hash;
            }

            public int Count { get; }
            public string Hash { get; }
        }

        private sealed class AggregateResult
        {
            public AggregateResult(int count, string hash)
            {
                Count = count;
                Hash = hash;
            }

            public int Count { get; }
            public string Hash { get; }
        }

        private sealed class ExternalDriftEvidence
        {
            public ExternalDriftEvidence(
                string path,
                string startHash,
                string currentHash,
                string owner,
                bool attributed)
            {
                Path = path;
                StartHash = startHash;
                CurrentHash = currentHash;
                Owner = owner;
                Attributed = attributed;
            }

            public string Path { get; }
            public string StartHash { get; }
            public string CurrentHash { get; }
            public string Owner { get; }
            public bool Attributed { get; }
        }

        private sealed class ExternalDriftStatus
        {
            public ExternalDriftStatus(
                ExternalDriftEvidence evidence,
                bool currentHashMatches)
            {
                Evidence = evidence;
                CurrentHashMatches = currentHashMatches;
            }

            public ExternalDriftEvidence Evidence { get; }
            public bool CurrentHashMatches { get; }
        }
    }
}
