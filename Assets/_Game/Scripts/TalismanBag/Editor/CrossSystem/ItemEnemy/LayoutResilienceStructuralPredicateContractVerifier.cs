using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.EnemySystem.RequirementChannel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class LayoutResilienceStructuralPredicateContractVerifier
    {
        private const string PackageName =
            "V0.4-LayoutResilienceStructuralPredicateContract01";
        private const string GuardReceipt =
            "GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01";
        private const string EnemyGuardReceipt =
            "ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01";
        private const string ItemGuardReceipt =
            "ITEM_GUARD_CONFIRM_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01";
        private const string AlgorithmGuardReceipt =
            "CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCESTRUCTURALPREDICATECONTRACT01";
        private const string ExpectedCanonicalSignature =
            "sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335";
        private const string ExpectedHead =
            "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba";
        private const string ExpectedItemHash =
            "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1";
        private const string ExpectedEnemyHash =
            "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae";
        private const string ExpectedC01Hash =
            "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a";
        private const string ExpectedC02Hash =
            "08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4";
        private const string ExpectedC02AHash =
            "a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb";
        private const string ExpectedN01Hash =
            "03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52";
        private const string ExpectedC02AD1Hash =
            "c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0";
        private const string ExpectedN01AHash =
            "705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de";
        private const string ExpectedN01BHash =
            "acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316";
        private const string ExpectedE06Hash =
            "c91d77499c07e6a6b828d8fb2e21c08096d656c6395e94159a0a547fdf3e5569";
        private const string ExpectedE07Hash =
            "b4be0330a411537e3e8e16968c2d45a5cdbbc688d9b588044ffe7e15daa505df";
        private const string ExpectedE08Hash =
            "45a40d4993e1bf6bb78f1ad14e6ac9dfe84c0c5a69ebffc5502fabb3815821d4";
        private const string ExpectedE10Hash =
            "3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be";
        private const string ExpectedScenesHash =
            "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b";
        private const string ExpectedPrefabsHash =
            "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087";
        private const string ExpectedBuildSettingsHash =
            "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59";
        private const string ExpectedN01ASignature =
            "sha256:de8f5d758bd8837d88621c33f80dc952c0b7c2dccd7b18f96e71c54649240e79";
        private const string ExpectedN01BSignature =
            "sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326";
        private const string ExpectedIF01Signature =
            "sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428";
        private const string ExpectedIF02Signature =
            "sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673";
        private const string ExpectedIF03Signature =
            "sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a";
        private const string ExpectedAffixSignature =
            "sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f";
        private const string ExpectedRollSignature =
            "sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8";
        private const string ExpectedProjectionSignature =
            "sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2";
        private const string ExpectedPreexistingCatalogHash =
            "5cc59ab0bb20e3b054c98b73676a9173fda0451f24441e877e08ba53b2350d45";
        private const string ExpectedPreexistingCandidateHash =
            "c6be8eb8fdc6011f903652d662b77ca3c2b3e5b853624378bf590b707477c0fb";

        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReport = ReportDirectory +
            "/LayoutResilienceStructuralPredicateContractReport.md";
        private const string SpecReport = ReportDirectory +
            "/LayoutResilienceStructuralPredicateContractSpec.csv";
        private const string FieldReport = ReportDirectory +
            "/LayoutResilienceStructuralPredicateContractFieldMatrix.csv";
        private const string FixtureReport = ReportDirectory +
            "/LayoutResilienceStructuralPredicateContractFixtureCases.csv";
        private const string LeakReport = ReportDirectory +
            "/LayoutResilienceStructuralPredicateContractLeakCheckReport.md";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicatePrimitives.cs",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicatePrimitives.cs.meta",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicateSnapshots.cs",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicateSnapshots.cs.meta",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicateValidation.cs",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicateValidation.cs.meta",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicateEvaluator.cs",
            RuntimeDirectory + "/LayoutResilienceStructuralPredicateEvaluator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs.meta",
            MainReport,
            SpecReport,
            FieldReport,
            FixtureReport,
            LeakReport
        };

        private static readonly string[] RuntimePaths = OutputPaths
            .Where(value => value.StartsWith(RuntimeDirectory + "/",
                StringComparison.Ordinal) && value.EndsWith(".cs", StringComparison.Ordinal))
            .ToArray();

        private static readonly string[] C01Files =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionAdapterReport.md",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionFieldMap.csv",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionFixtureRows.csv",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionUnknowns.csv",
            "Docs/V0.4/Reports/ItemBuildCapabilityProjectionLeakCheckReport.md"
        };

        private static readonly string[] C02Files =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulationVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulationVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemEnemyMatchupSimulationReport.md",
            "Docs/V0.4/Reports/ItemEnemyMatchupMatrix.csv",
            "Docs/V0.4/Reports/ItemEnemyMatchupCapabilityCoverage.csv",
            "Docs/V0.4/Reports/ItemEnemyMatchupUnknownBlockers.csv",
            "Docs/V0.4/Reports/ItemEnemyMatchupLeakCheckReport.md"
        };

        private static readonly string[] C02AFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyReport.md",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapMatrix.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingSourceEvidence.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingDecisionList.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyLeakCheckReport.md"
        };

        private static readonly string[] N01Files =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs.meta",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationRuleSurveyReport.md",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationSourceEligibilityMatrix.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationRuleCandidates.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationC02ImpactMatrix.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationUserDecisionSheet.csv",
            "Docs/V0.4/Reports/BuildCapabilityNormalizationRuleSurveyLeakCheckReport.md"
        };

        private static readonly string[] C02AD1Files =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionReport.md",
            "Docs/V0.4/Reports/ItemCapabilityRuleCandidates.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleImpactMatrix.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionSheet.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionLeakCheckReport.md"
        };

        private static readonly string[] N01AFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs.meta",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticMatrix.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSourceEvidence.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerC02Impact.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerUserDecisionSheet.csv",
            "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerLeakCheckReport.md"
        };

        private static readonly string[] N01BFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilitySnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel/EnemyRequirementChannelApplicabilityValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs.meta",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractReport.md",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractSpec.csv",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFieldMatrix.csv",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractFixtureRows.csv",
            "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/[QA Only] Verify Layout Resilience Structural Predicate")]
        public static void VerifyMenu()
        {
            VerifyOrThrow("Unity Editor menu");
        }
#endif

        public static void VerifyOffline()
        {
            VerifyOrThrow("Pure C# offline verifier");
        }

        public static void VerifyStaticBatch()
        {
            VerifyOrThrow("Unity batch compile/verifier");
        }

        public static int Main(string[] args)
        {
            try
            {
                VerifyOffline();
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                return 1;
            }
        }

        private static void VerifyOrThrow(string mode)
        {
            string root = FindProjectRoot();
            ProtectionSnapshot before = CaptureProtection(root);
            List<Check> checks = new List<Check>();
            VerifySchemaAndShapes(checks);
            VerifyTruthTable(checks);
            List<OutcomeCase> outcomes = VerifyOutcomes(checks);
            int invalidCount = VerifyInvalidCases(checks, outcomes);
            VerifyDeterminismAndImmutability(checks, outcomes);
            VerifyReportsAndLeaks(root, checks, outcomes, invalidCount);
            VerifyBaselineBehavior(root, checks);
            VerifyRepositoryBoundary(root, checks);
            ProtectionSnapshot after = CaptureProtection(root);
            VerifyProtection(checks, before, after);

            Check[] failed = checks.Where(value => !value.Passed).ToArray();
            string signature = outcomes[0].Result.CanonicalSignature;
            Console.WriteLine("LAYOUT_RESILIENCE_STRUCTURAL_PREDICATE_CONTRACT01 " +
                (failed.Length == 0 ? "PASS" : "FAIL") + " checks=" +
                checks.Count.ToString(CultureInfo.InvariantCulture) + " mode=" + mode);
            Console.WriteLine("CANONICAL_SIGNATURE=" + signature);
            Console.WriteLine("OUTCOME_FIXTURES=10/10 INVALID_ASSERTIONS=" +
                invalidCount.ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("DISPOSITION=KnownTrue:3 KnownFalse:3 Unknown:3 NotApplicable:1");
            Console.WriteLine("LEAK_COUNT=" + checks.Count(value =>
                value.Id.StartsWith("leak.", StringComparison.Ordinal) && !value.Passed)
                .ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("N01B_EXACT14=" + before.N01B.Hash);
            foreach (Check failure in failed)
            {
                Console.Error.WriteLine(failure.Id + " expected=" + failure.Expected +
                    " actual=" + failure.Actual);
            }
            if (failed.Length > 0)
            {
                throw new InvalidOperationException(failed.Length.ToString(
                    CultureInfo.InvariantCulture) + " verifier assertion(s) failed.");
            }
        }

        private static void VerifySchemaAndShapes(ICollection<Check> checks)
        {
            Add(checks, "schema.id", "LayoutResilienceStructuralPredicate.v1",
                LayoutResilienceStructuralPredicateSchema.SchemaId,
                LayoutResilienceStructuralPredicateSchema.SchemaId ==
                    "LayoutResilienceStructuralPredicate.v1");
            Add(checks, "schema.version", "1",
                LayoutResilienceStructuralPredicateSchema.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture),
                LayoutResilienceStructuralPredicateSchema.SchemaVersion == 1);
            AssertEnum<LayoutResiliencePredicateState>(checks, "predicate-state",
                new[] { "KnownTrue", "KnownFalse", "Unknown", "NotApplicable" },
                new[] { 1, 2, 3, 4 });
            AssertEnum<LayoutResilienceInputCompleteness>(checks, "completeness",
                new[] { "Complete", "Incomplete", "NotRequired" },
                new[] { 1, 2, 3 });
            AssertEnum<LayoutPressureKind>(checks, "pressure-kind",
                new[] { "PollutedCellMask", "EyeRelocationOrDisruption",
                    "StructuralConnectionCut" }, new[] { 1, 2, 3 });
            AssertEnum<LayoutResiliencePredicateClauseKind>(checks, "clause-kind",
                new[] { "CountedLayoutPresent", "CountedPlacementCellsUsable",
                    "CountedPlacementCoresUsable", "EffectiveEyeAnchorUsable",
                    "EyeToCountedCoreStructurallyConnected" },
                new[] { 1, 2, 3, 4, 5 });
            AssertEnum<LayoutResiliencePredicateClauseState>(checks, "clause-state",
                new[] { "Satisfied", "Violated" }, new[] { 1, 2 });

            AssertProperties(checks, "shape.coordinate", typeof(LayoutCellCoordinate),
                "X", "Y");
            AssertProperties(checks, "shape.connection", typeof(LayoutCellConnection),
                "CellA", "CellB");
            AssertProperties(checks, "shape.placed",
                typeof(LayoutResiliencePlacedItemFactSnapshot), "AnchorCell", "CoreCellWorld",
                "IsCountedInBuild", "IsLit", "ItemId", "OccupiedCells", "PlacementId",
                "Rotation", "ShapeCells");
            AssertProperties(checks, "shape.build", typeof(LayoutResilienceBuildFactSnapshot),
                "BaselineEyeCell", "BoardSize", "PlacementRows");
            AssertProperties(checks, "shape.pressure", typeof(LayoutPressureSnapshot),
                "EffectiveEyeAnchorCellAfterPressure", "LayoutDomainCells",
                "PreservedStructuralConnectionsAfterPressure", "PressureInputId",
                "PressureKinds", "RequiredPredicateClauses", "UsableCellsAfterPressure");
            AssertProperties(checks, "shape.input", typeof(LayoutResilienceEvaluationInput),
                "ApplicabilityState", "BuildFacts", "BuildFactsCompleteness", "EvaluationId",
                "Pressure", "PressureFactsCompleteness", "SchemaId", "SchemaVersion");
            AssertProperties(checks, "shape.clause",
                typeof(LayoutResiliencePredicateClauseSnapshot), "ClauseKind", "ClauseState");
            AssertProperties(checks, "shape.result",
                typeof(LayoutResiliencePredicateResultSnapshot), "ApplicabilityState",
                "CanonicalSignature", "ClauseRows", "EvaluationId", "PredicateState",
                "SchemaId", "SchemaVersion");
        }

        private static void VerifyTruthTable(ICollection<Check> checks)
        {
            ILayoutResilienceStructuralPredicateValidator validator =
                DefaultLayoutResilienceStructuralPredicateValidator.Instance;
            int valid = 0;
            int invalid = 0;
            foreach (EnemyRequirementApplicabilityState applicability in new[]
            {
                EnemyRequirementApplicabilityState.Applicable,
                EnemyRequirementApplicabilityState.Unknown,
                EnemyRequirementApplicabilityState.NotApplicable,
                EnemyRequirementApplicabilityState.NotInChannel
            })
            {
                foreach (LayoutResilienceInputCompleteness build in CompletenessValues())
                {
                    foreach (LayoutResilienceInputCompleteness pressure in CompletenessValues())
                    {
                        bool expected = (applicability ==
                                EnemyRequirementApplicabilityState.Applicable ||
                            applicability == EnemyRequirementApplicabilityState.Unknown)
                            ? build != LayoutResilienceInputCompleteness.NotRequired &&
                                pressure != LayoutResilienceInputCompleteness.NotRequired
                            : applicability == EnemyRequirementApplicabilityState.NotApplicable &&
                                build == LayoutResilienceInputCompleteness.NotRequired &&
                                pressure == LayoutResilienceInputCompleteness.NotRequired;
                        LayoutResilienceEvaluationInput input = new LayoutResilienceEvaluationInput(
                            "dev_layout_truth_table", applicability, build, pressure,
                            build == LayoutResilienceInputCompleteness.Complete ? BaseBuild() : null,
                            pressure == LayoutResilienceInputCompleteness.Complete
                                ? BasePressure(AllClauses()) : null);
                        bool actual = validator.Validate(input).Count == 0;
                        Add(checks, "truth." + applicability + "." + build + "." + pressure,
                            expected.ToString(), actual.ToString(), expected == actual);
                        if (expected) valid++; else invalid++;
                    }
                }
            }
            Add(checks, "truth.valid-count", "9", valid.ToString(
                CultureInfo.InvariantCulture), valid == 9);
            Add(checks, "truth.invalid-count", "27", invalid.ToString(
                CultureInfo.InvariantCulture), invalid == 27);
        }

        private static List<OutcomeCase> VerifyOutcomes(ICollection<Check> checks)
        {
            List<OutcomeCase> cases = BuildOutcomeCases();
            foreach (OutcomeCase fixture in cases)
            {
                fixture.Result = DefaultLayoutResilienceStructuralPredicateEvaluator.Instance
                    .Evaluate(fixture.Input);
                Add(checks, "outcome." + fixture.Id + ".state",
                    fixture.ExpectedState.ToString(), fixture.Result.PredicateState.ToString(),
                    fixture.Result.PredicateState == fixture.ExpectedState);
                string actualViolated = string.Join(";", fixture.Result.ClauseRows
                    .Where(value => value.ClauseState ==
                        LayoutResiliencePredicateClauseState.Violated)
                    .Select(value => value.ClauseKind.ToString()));
                Add(checks, "outcome." + fixture.Id + ".violated",
                    fixture.ExpectedViolated, actualViolated,
                    fixture.ExpectedViolated == actualViolated);
                Add(checks, "outcome." + fixture.Id + ".result-valid", "0",
                    DefaultLayoutResilienceStructuralPredicateValidator.Instance
                        .ValidateResult(fixture.Input, fixture.Result).Count.ToString(
                            CultureInfo.InvariantCulture),
                    DefaultLayoutResilienceStructuralPredicateValidator.Instance
                        .ValidateResult(fixture.Input, fixture.Result).Count == 0);
            }
            Add(checks, "outcome.count", "10", cases.Count.ToString(
                CultureInfo.InvariantCulture), cases.Count == 10);
            Add(checks, "outcome.known-true", "3", cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.KnownTrue)
                .ToString(CultureInfo.InvariantCulture), cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.KnownTrue) == 3);
            Add(checks, "outcome.known-false", "3", cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.KnownFalse)
                .ToString(CultureInfo.InvariantCulture), cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.KnownFalse) == 3);
            Add(checks, "outcome.unknown", "3", cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.Unknown)
                .ToString(CultureInfo.InvariantCulture), cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.Unknown) == 3);
            Add(checks, "outcome.not-applicable", "1", cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.NotApplicable)
                .ToString(CultureInfo.InvariantCulture), cases.Count(value =>
                value.Result.PredicateState == LayoutResiliencePredicateState.NotApplicable) == 1);
            return cases;
        }

        private static int VerifyInvalidCases(
            ICollection<Check> checks,
            IList<OutcomeCase> outcomes)
        {
            List<InvalidCase> invalid = new List<InvalidCase>();
            invalid.Add(new InvalidCase("null-input", null, "INPUT_NULL"));
            invalid.Add(new InvalidCase("schema-id", Input(BaseBuild(), BasePressure(AllClauses()),
                schemaId: "wrong"), "SCHEMA_ID_MISMATCH"));
            invalid.Add(new InvalidCase("schema-version", Input(BaseBuild(),
                BasePressure(AllClauses()), schemaVersion: 2), "SCHEMA_VERSION_MISMATCH"));
            invalid.Add(new InvalidCase("evaluation-empty", Input(BaseBuild(),
                BasePressure(AllClauses()), evaluationId: string.Empty), "EVALUATION_ID_EMPTY"));
            invalid.Add(new InvalidCase("evaluation-whitespace", Input(BaseBuild(),
                BasePressure(AllClauses()), evaluationId: " dev_layout"),
                "EVALUATION_ID_OUTER_WHITESPACE"));
            invalid.Add(new InvalidCase("applicability-zero", Input(BaseBuild(),
                BasePressure(AllClauses()), applicability:
                    (EnemyRequirementApplicabilityState)0), "APPLICABILITY_UNDEFINED"));
            invalid.Add(new InvalidCase("applicability-undefined", Input(BaseBuild(),
                BasePressure(AllClauses()), applicability:
                    (EnemyRequirementApplicabilityState)99), "APPLICABILITY_UNDEFINED"));
            invalid.Add(new InvalidCase("not-in-channel", Input(BaseBuild(),
                BasePressure(AllClauses()), applicability:
                    EnemyRequirementApplicabilityState.NotInChannel),
                "NOT_IN_CHANNEL_REJECTED"));
            invalid.Add(new InvalidCase("build-completeness-zero",
                new LayoutResilienceEvaluationInput("dev_layout_invalid",
                    EnemyRequirementApplicabilityState.Applicable,
                    (LayoutResilienceInputCompleteness)0,
                    LayoutResilienceInputCompleteness.Complete, BaseBuild(),
                    BasePressure(AllClauses())), "BUILD_COMPLETENESS_UNDEFINED"));
            invalid.Add(new InvalidCase("pressure-completeness-zero",
                new LayoutResilienceEvaluationInput("dev_layout_invalid",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Complete,
                    (LayoutResilienceInputCompleteness)99, BaseBuild(),
                    BasePressure(AllClauses())), "PRESSURE_COMPLETENESS_UNDEFINED"));
            invalid.Add(new InvalidCase("complete-build-missing",
                new LayoutResilienceEvaluationInput("dev_layout_invalid",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Complete,
                    LayoutResilienceInputCompleteness.Incomplete, null, null),
                "BUILD_FACTS_REQUIRED"));
            invalid.Add(new InvalidCase("complete-pressure-missing",
                new LayoutResilienceEvaluationInput("dev_layout_invalid",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Incomplete,
                    LayoutResilienceInputCompleteness.Complete, null, null),
                "PRESSURE_FACTS_REQUIRED"));
            invalid.Add(new InvalidCase("not-required-build-payload",
                new LayoutResilienceEvaluationInput("dev_layout_invalid",
                    EnemyRequirementApplicabilityState.NotApplicable,
                    LayoutResilienceInputCompleteness.NotRequired,
                    LayoutResilienceInputCompleteness.NotRequired, BaseBuild(), null),
                "BUILD_NOT_REQUIRED_PAYLOAD"));
            invalid.Add(new InvalidCase("not-required-pressure-payload",
                new LayoutResilienceEvaluationInput("dev_layout_invalid",
                    EnemyRequirementApplicabilityState.NotApplicable,
                    LayoutResilienceInputCompleteness.NotRequired,
                    LayoutResilienceInputCompleteness.NotRequired, null,
                    BasePressure(AllClauses())), "PRESSURE_NOT_REQUIRED_PAYLOAD"));

            invalid.Add(new InvalidCase("board-size-missing", Input(
                Build(null, Cell(0, 0), BasePlacements()), BasePressure(AllClauses())),
                "BOARD_SIZE_MISSING"));
            invalid.Add(new InvalidCase("board-size-zero", Input(
                Build(0, Cell(0, 0), BasePlacements()), BasePressure(AllClauses())),
                "BOARD_SIZE_INVALID"));
            invalid.Add(new InvalidCase("baseline-eye-missing", Input(
                Build(3, null, BasePlacements()), BasePressure(AllClauses())),
                "BASELINE_EYE_MISSING"));
            invalid.Add(new InvalidCase("baseline-eye-out-of-bounds", Input(
                Build(3, Cell(3, 0), BasePlacements()), BasePressure(AllClauses())),
                "BASELINE_EYE_OUT_OF_BOUNDS"));
            invalid.Add(new InvalidCase("placement-null", Input(
                Build(3, Cell(0, 0), new LayoutResiliencePlacedItemFactSnapshot[] { null }),
                BasePressure(AllClauses())), "PLACEMENT_ROW_NULL"));
            invalid.Add(new InvalidCase("placement-id-empty", Input(
                BuildWith(Placement(string.Empty, "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "PLACEMENT_ID_EMPTY"));
            invalid.Add(new InvalidCase("placement-id-whitespace", Input(
                BuildWith(Placement(" dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "PLACEMENT_ID_OUTER_WHITESPACE"));
            invalid.Add(new InvalidCase("item-id-empty", Input(
                BuildWith(Placement("dev_layout_p", string.Empty, Cell(0, 1), 0,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "ITEM_ID_EMPTY"));
            LayoutResiliencePlacedItemFactSnapshot duplicate = BasePlacements()[0];
            invalid.Add(new InvalidCase("placement-duplicate", Input(
                Build(3, Cell(0, 0), new[] { duplicate, duplicate }),
                BasePressure(AllClauses())), "PLACEMENT_ID_DUPLICATE"));
            invalid.Add(new InvalidCase("rotation-invalid", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 45,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "ROTATION_INVALID"));
            invalid.Add(new InvalidCase("shape-empty", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    new LayoutCellCoordinate[0], Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "SHAPE_CELLS_EMPTY"));
            invalid.Add(new InvalidCase("shape-duplicate", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0, 0, 0), Cells(0, 1, 1, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "SHAPE_CELL_DUPLICATE"));
            invalid.Add(new InvalidCase("occupied-empty", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), new LayoutCellCoordinate[0], Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "OCCUPIED_CELLS_EMPTY"));
            invalid.Add(new InvalidCase("occupied-duplicate", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0, 1, 0), Cells(0, 1, 0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "OCCUPIED_CELL_DUPLICATE"));
            invalid.Add(new InvalidCase("cardinality-mismatch", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0, 1, 0), Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "SHAPE_OCCUPIED_CARDINALITY_MISMATCH"));
            invalid.Add(new InvalidCase("core-missing", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), Cells(0, 1), null, true, true)),
                BasePressure(AllClauses())), "CORE_CELL_MISSING"));
            invalid.Add(new InvalidCase("core-not-occupied", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), Cells(0, 1), Cell(1, 1), true, true)),
                BasePressure(AllClauses())), "CORE_CELL_NOT_OCCUPIED"));
            invalid.Add(new InvalidCase("counted-not-lit", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), false, true)),
                BasePressure(AllClauses())), "COUNTED_NOT_LIT"));
            invalid.Add(new InvalidCase("anchor-missing", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", null, 0,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "ANCHOR_CELL_MISSING"));
            invalid.Add(new InvalidCase("anchor-out-of-bounds", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(3, 0), 0,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), true, true)),
                BasePressure(AllClauses())), "ANCHOR_CELL_OUT_OF_BOUNDS"));
            invalid.Add(new InvalidCase("core-out-of-bounds", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), Cells(3, 1), Cell(3, 1), true, true)),
                BasePressure(AllClauses())), "CORE_CELL_OUT_OF_BOUNDS"));
            invalid.Add(new InvalidCase("occupied-out-of-bounds", Input(
                BuildWith(Placement("dev_layout_p", "dev_layout_item", Cell(0, 1), 0,
                    Cells(0, 0), Cells(3, 1), Cell(3, 1), true, true)),
                BasePressure(AllClauses())), "OCCUPIED_CELL_OUT_OF_BOUNDS"));

            invalid.Add(new InvalidCase("pressure-id-empty", Input(BaseBuild(),
                Pressure(string.Empty, Kinds(LayoutPressureKind.PollutedCellMask), Domain(),
                    Domain(), Cell(0, 0), BaseConnections(), AllClauses())),
                "PRESSURE_INPUT_ID_EMPTY"));
            invalid.Add(new InvalidCase("pressure-id-whitespace", Input(BaseBuild(),
                Pressure("dev_layout_pressure ", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), Cell(0, 0), BaseConnections(), AllClauses())),
                "PRESSURE_INPUT_ID_OUTER_WHITESPACE"));
            invalid.Add(new InvalidCase("pressure-kind-empty", Input(BaseBuild(),
                Pressure("dev_layout_pressure", new LayoutPressureKind[0], Domain(), Domain(),
                    Cell(0, 0), BaseConnections(), AllClauses())), "PRESSURE_KIND_EMPTY"));
            invalid.Add(new InvalidCase("pressure-kind-undefined", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds((LayoutPressureKind)0), Domain(),
                    Domain(), Cell(0, 0), BaseConnections(), AllClauses())),
                "PRESSURE_KIND_UNDEFINED"));
            invalid.Add(new InvalidCase("pressure-kind-duplicate", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask,
                    LayoutPressureKind.PollutedCellMask), Domain(), Domain(), Cell(0, 0),
                    BaseConnections(), AllClauses())), "PRESSURE_KIND_DUPLICATE"));
            invalid.Add(new InvalidCase("required-clause-empty", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), Cell(0, 0), BaseConnections(),
                    new LayoutResiliencePredicateClauseKind[0])), "REQUIRED_CLAUSE_EMPTY"));
            invalid.Add(new InvalidCase("required-clause-undefined", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), Cell(0, 0), BaseConnections(),
                    Clauses((LayoutResiliencePredicateClauseKind)99))),
                "REQUIRED_CLAUSE_UNDEFINED"));
            invalid.Add(new InvalidCase("required-clause-duplicate", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), Cell(0, 0), BaseConnections(),
                    Clauses(LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseKind.CountedLayoutPresent))),
                "REQUIRED_CLAUSE_DUPLICATE"));
            invalid.Add(new InvalidCase("domain-duplicate", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain().Concat(Cells(0, 0)), Domain(), Cell(0, 0), BaseConnections(),
                    AllClauses())), "DOMAIN_CELL_DUPLICATE"));
            invalid.Add(new InvalidCase("domain-null-cell", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain().Cast<LayoutCellCoordinate>().Concat(
                        new LayoutCellCoordinate[] { null }), Domain(), Cell(0, 0),
                    BaseConnections(), AllClauses())), "DOMAIN_CELL_NULL"));
            invalid.Add(new InvalidCase("domain-not-exact", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain().Where(value => !value.Equals(Cell(2, 2))), Domain(), Cell(0, 0),
                    BaseConnections(), AllClauses())), "DOMAIN_NOT_EXACT_BOARD"));
            invalid.Add(new InvalidCase("usable-outside-domain", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain().Concat(Cells(3, 3)), Cell(0, 0), BaseConnections(),
                    AllClauses())), "USABLE_CELL_OUTSIDE_DOMAIN"));
            invalid.Add(new InvalidCase("usable-duplicate", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain().Concat(Cells(0, 0)), Cell(0, 0), BaseConnections(),
                    AllClauses())), "USABLE_CELL_DUPLICATE"));
            invalid.Add(new InvalidCase("effective-eye-missing", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), null, BaseConnections(), AllClauses())),
                "EFFECTIVE_EYE_MISSING"));
            invalid.Add(new InvalidCase("effective-eye-out-of-bounds", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), Cell(3, 0), BaseConnections(), AllClauses())),
                "EFFECTIVE_EYE_OUT_OF_BOUNDS"));
            invalid.Add(new InvalidCase("connection-null", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), Cell(0, 0), new LayoutCellConnection[] { null },
                    AllClauses())), "CONNECTION_NULL"));
            invalid.Add(new InvalidCase("connection-endpoint-missing", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.PollutedCellMask),
                    Domain(), Domain(), Cell(0, 0),
                    new[] { new LayoutCellConnection(null, Cell(1, 0)) }, AllClauses())),
                "CONNECTION_ENDPOINT_MISSING"));
            invalid.Add(new InvalidCase("connection-self-loop", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.StructuralConnectionCut),
                    Domain(), Domain(), Cell(0, 0),
                    new[] { Connection(0, 0, 0, 0) }, AllClauses())),
                "CONNECTION_SELF_LOOP"));
            invalid.Add(new InvalidCase("connection-not-normalized", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.StructuralConnectionCut),
                    Domain(), Domain(), Cell(0, 0),
                    new[] { Connection(1, 0, 0, 0) }, AllClauses())),
                "CONNECTION_NOT_NORMALIZED"));
            invalid.Add(new InvalidCase("connection-endpoint-not-usable", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.StructuralConnectionCut),
                    Domain(), Domain().Where(value => !value.Equals(Cell(1, 0))), Cell(0, 0),
                    new[] { Connection(0, 0, 1, 0) }, AllClauses())),
                "CONNECTION_ENDPOINT_NOT_USABLE"));
            LayoutCellConnection connection = Connection(0, 0, 1, 0);
            invalid.Add(new InvalidCase("connection-duplicate", Input(BaseBuild(),
                Pressure("dev_layout_pressure", Kinds(LayoutPressureKind.StructuralConnectionCut),
                    Domain(), Domain(), Cell(0, 0), new[] { connection, connection },
                    AllClauses())), "CONNECTION_DUPLICATE"));

            foreach (InvalidCase fixture in invalid)
            {
                AssertInvalid(checks, "invalid." + fixture.Id, fixture.Input,
                    fixture.ExpectedCode);
            }

            OutcomeCase knownTrue = outcomes[0];
            AssertInvalidResult(checks, "invalid-result.null", knownTrue.Input, null,
                "RESULT_NULL");
            AssertInvalidResult(checks, "invalid-result.known-true-violated",
                knownTrue.Input, Forged(knownTrue.Input,
                    LayoutResiliencePredicateState.KnownTrue,
                    new[] { Clause(LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseState.Violated) },
                    knownTrue.Result.CanonicalSignature), "KNOWN_TRUE_HAS_VIOLATED");
            AssertInvalidResult(checks, "invalid-result.known-false-no-violation",
                knownTrue.Input, Forged(knownTrue.Input,
                    LayoutResiliencePredicateState.KnownFalse,
                    knownTrue.Result.ClauseRows.Select(value => Clause(value.ClauseKind,
                        LayoutResiliencePredicateClauseState.Satisfied)),
                    knownTrue.Result.CanonicalSignature), "KNOWN_FALSE_WITHOUT_VIOLATED");
            AssertInvalidResult(checks, "invalid-result.missing-required-clause",
                knownTrue.Input, Forged(knownTrue.Input,
                    LayoutResiliencePredicateState.KnownTrue,
                    new[] { Clause(LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseState.Satisfied) },
                    knownTrue.Result.CanonicalSignature), "RESULT_CLAUSE_SET_MISMATCH");
            AssertInvalidResult(checks, "invalid-result.duplicate-clause", knownTrue.Input,
                Forged(knownTrue.Input, LayoutResiliencePredicateState.KnownTrue,
                    knownTrue.Result.ClauseRows.Concat(new[] { Clause(
                        LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseState.Satisfied) }),
                    knownTrue.Result.CanonicalSignature), "RESULT_CLAUSE_DUPLICATE");
            AssertInvalidResult(checks, "invalid-result.clause-kind-undefined",
                knownTrue.Input, Forged(knownTrue.Input,
                    LayoutResiliencePredicateState.KnownTrue,
                    new[] { Clause((LayoutResiliencePredicateClauseKind)99,
                        LayoutResiliencePredicateClauseState.Satisfied) },
                    knownTrue.Result.CanonicalSignature), "RESULT_CLAUSE_KIND_UNDEFINED");
            AssertInvalidResult(checks, "invalid-result.clause-state-undefined",
                knownTrue.Input, Forged(knownTrue.Input,
                    LayoutResiliencePredicateState.KnownTrue,
                    new[] { Clause(LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        (LayoutResiliencePredicateClauseState)99) },
                    knownTrue.Result.CanonicalSignature), "RESULT_CLAUSE_STATE_UNDEFINED");
            AssertInvalidResult(checks, "invalid-result.signature", knownTrue.Input,
                Forged(knownTrue.Input, knownTrue.Result.PredicateState,
                    knownTrue.Result.ClauseRows, "sha256:0000000000000000000000000000000000000000000000000000000000000000"),
                "RESULT_SIGNATURE_MISMATCH");
            OutcomeCase unknown = outcomes[6];
            AssertInvalidResult(checks, "invalid-result.unknown-has-clause", unknown.Input,
                Forged(unknown.Input, LayoutResiliencePredicateState.Unknown,
                    new[] { Clause(LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseState.Satisfied) },
                    unknown.Result.CanonicalSignature), "NON_KNOWN_RESULT_HAS_CLAUSES");
            Add(checks, "invalid.assertion-count.minimum", ">=24",
                (invalid.Count + 9).ToString(CultureInfo.InvariantCulture),
                invalid.Count + 9 >= 24);
            return invalid.Count + 9;
        }

        private static void VerifyDeterminismAndImmutability(
            ICollection<Check> checks,
            IList<OutcomeCase> outcomes)
        {
            LayoutResiliencePredicateResultSnapshot baseline = outcomes[0].Result;
            LayoutResiliencePredicateResultSnapshot repeat =
                DefaultLayoutResilienceStructuralPredicateEvaluator.Instance
                    .Evaluate(BuildOutcomeCases()[0].Input);
            Add(checks, "canonical.expected", ExpectedCanonicalSignature,
                baseline.CanonicalSignature,
                baseline.CanonicalSignature == ExpectedCanonicalSignature);
            Add(checks, "canonical.format", "sha256:+64 lowercase hex",
                baseline.CanonicalSignature, IsSignature(baseline.CanonicalSignature));
            Add(checks, "canonical.repeat", baseline.CanonicalSignature,
                repeat.CanonicalSignature,
                baseline.CanonicalSignature == repeat.CanonicalSignature);

            LayoutResilienceBuildFactSnapshot reversedBuild = Build(3, Cell(0, 0),
                BasePlacements().Reverse());
            LayoutPressureSnapshot reversedPressure = Pressure("dev_layout_pressure_safe",
                Kinds(LayoutPressureKind.PollutedCellMask).Reverse(), Domain().Reverse(),
                Domain().Where(value => !value.Equals(Cell(1, 2))).Reverse(), Cell(0, 0),
                BaseConnections().Reverse(), AllClauses().Reverse());
            LayoutResiliencePredicateResultSnapshot reversed =
                DefaultLayoutResilienceStructuralPredicateEvaluator.Instance.Evaluate(
                    Input(reversedBuild, reversedPressure,
                        evaluationId: "dev_layout_case_01_safe_pollution"));
            Add(checks, "canonical.order-insensitive", baseline.CanonicalSignature,
                reversed.CanonicalSignature,
                baseline.CanonicalSignature == reversed.CanonicalSignature);

            LayoutResiliencePredicateResultSnapshot changedIdentity =
                DefaultLayoutResilienceStructuralPredicateEvaluator.Instance.Evaluate(
                    Input(BaseBuild(), BasePressure(AllClauses()),
                        evaluationId: "dev_layout_identity_changed"));
            Add(checks, "canonical.identity-sensitive", "different",
                changedIdentity.CanonicalSignature,
                changedIdentity.CanonicalSignature != baseline.CanonicalSignature);
            Add(checks, "canonical.coordinate-sensitive", "different",
                outcomes[2].Result.CanonicalSignature,
                outcomes[2].Result.CanonicalSignature != baseline.CanonicalSignature);
            Add(checks, "canonical.connection-sensitive", "different",
                outcomes[3].Result.CanonicalSignature,
                outcomes[3].Result.CanonicalSignature != outcomes[2].Result.CanonicalSignature);
            Add(checks, "canonical.bool-sensitive", "different",
                outcomes[4].Result.CanonicalSignature,
                outcomes[4].Result.CanonicalSignature != baseline.CanonicalSignature);
            Add(checks, "canonical.completeness-sensitive", "different",
                outcomes[7].Result.CanonicalSignature,
                outcomes[7].Result.CanonicalSignature != baseline.CanonicalSignature);
            Add(checks, "canonical.applicability-sensitive", "different",
                outcomes[6].Result.CanonicalSignature,
                outcomes[6].Result.CanonicalSignature != baseline.CanonicalSignature);
            Add(checks, "canonical.clause-outcome-sensitive", "different",
                outcomes[1].Result.CanonicalSignature,
                outcomes[1].Result.CanonicalSignature != baseline.CanonicalSignature);

            List<LayoutCellCoordinate> mutableCells = Domain().ToList();
            List<LayoutCellConnection> mutableConnections = BaseConnections().ToList();
            List<LayoutResiliencePlacedItemFactSnapshot> mutableRows =
                BasePlacements().ToList();
            LayoutResilienceBuildFactSnapshot build = Build(3, Cell(0, 0), mutableRows);
            LayoutPressureSnapshot pressure = Pressure("dev_layout_immutability",
                Kinds(LayoutPressureKind.PollutedCellMask), mutableCells, mutableCells,
                Cell(0, 0), mutableConnections, AllClauses());
            LayoutResilienceEvaluationInput input = Input(build, pressure,
                evaluationId: "dev_layout_immutability_case");
            LayoutResiliencePredicateResultSnapshot beforeMutation =
                DefaultLayoutResilienceStructuralPredicateEvaluator.Instance.Evaluate(input);
            mutableCells.Clear();
            mutableConnections.Clear();
            mutableRows.Clear();
            LayoutResiliencePredicateResultSnapshot afterMutation =
                DefaultLayoutResilienceStructuralPredicateEvaluator.Instance.Evaluate(input);
            Add(checks, "immutability.external-source", beforeMutation.CanonicalSignature,
                afterMutation.CanonicalSignature,
                beforeMutation.CanonicalSignature == afterMutation.CanonicalSignature &&
                    input.BuildFacts.PlacementRows.Count == 3 &&
                    input.Pressure.LayoutDomainCells.Count == 9);
            AddThrowsNotSupported(checks, "immutability.placements-readonly", delegate
            {
                ((IList<LayoutResiliencePlacedItemFactSnapshot>)
                    input.BuildFacts.PlacementRows).Clear();
            });
            AddThrowsNotSupported(checks, "immutability.cells-readonly", delegate
            {
                ((IList<LayoutCellCoordinate>)input.Pressure.LayoutDomainCells).Clear();
            });
            AddThrowsNotSupported(checks, "immutability.clauses-readonly", delegate
            {
                ((IList<LayoutResiliencePredicateClauseSnapshot>)
                    beforeMutation.ClauseRows).Clear();
            });
        }

        private static void VerifyReportsAndLeaks(
            string root,
            ICollection<Check> checks,
            IList<OutcomeCase> outcomes,
            int invalidCount)
        {
            foreach (string path in OutputPaths)
            {
                Add(checks, "output.exists." + Path.GetFileName(path), "True",
                    File.Exists(Absolute(root, path)).ToString(),
                    File.Exists(Absolute(root, path)));
            }
            Add(checks, "output.count", "16", OutputPaths.Length.ToString(
                CultureInfo.InvariantCulture), OutputPaths.Length == 16 &&
                OutputPaths.All(value => File.Exists(Absolute(root, value))));
            string main = Read(root, MainReport);
            AddContains(checks, "report.package", main, PackageName);
            AddContains(checks, "report.guard", main, GuardReceipt);
            AddContains(checks, "report.enemy-guard", main, EnemyGuardReceipt);
            AddContains(checks, "report.item-guard", main, ItemGuardReceipt);
            AddContains(checks, "report.algorithm-guard", main, AlgorithmGuardReceipt);
            AddContains(checks, "report.signature", main, ExpectedCanonicalSignature);
            AddContains(checks, "report.no-bp", main, "BP/score/ratio conversion rows: `0`");
            AddContains(checks, "report.no-migration", main,
                "Real requirement/Encounter mapping rows: `0`");
            Add(checks, "report.spec.rows", ">40", CsvRows(root, SpecReport).Length
                .ToString(CultureInfo.InvariantCulture), CsvRows(root, SpecReport).Length > 40);
            Add(checks, "report.field.rows", "46", CsvRows(root, FieldReport).Length
                .ToString(CultureInfo.InvariantCulture), CsvRows(root, FieldReport).Length == 46);
            Add(checks, "report.fixture.rows", "10", CsvRows(root, FixtureReport).Length
                .ToString(CultureInfo.InvariantCulture), CsvRows(root, FixtureReport).Length == 10);
            Add(checks, "report.fixture.valid", "10", CsvRows(root, FixtureReport)
                .Count(value => value.EndsWith(",true", StringComparison.Ordinal))
                .ToString(CultureInfo.InvariantCulture), CsvRows(root, FixtureReport)
                .Count(value => value.EndsWith(",true", StringComparison.Ordinal)) == 10);
            string leak = Read(root, LeakReport);
            AddContains(checks, "report.leak.total", leak, "Leak Count: `0`");
            foreach (string section in new[]
            {
                "Item existing-source mutation", "Enemy existing-source mutation",
                "BP or score conversion", "Real requirement migration", "Readiness consumer",
                "Board Map or Battle connection", "Runtime Producer", "Real identity fixture",
                "Package allowlist", "GUID collision", "Trailing whitespace"
            })
            {
                AddContains(checks, "report.leak.section." + section, leak, section);
            }

            string runtime = string.Join("\n", RuntimePaths.Select(path => Read(root, path)));
            foreach (string token in new[]
            {
                "using UnityEngine", "TalismanBag.Items", "PressureWindow",
                "CapabilityRead", "ReadinessEvaluation", "TalismanBag.Battle",
                "TalismanBag.Board", "Vector2Int", "MonoBehaviour", "ScriptableObject",
                "BuildCapabilityKey", "RequirementId", "EncounterId", "ReadinessBand",
                "BasisPoint", "Score", "Ratio", "Threshold", "Weight", "Clamp",
                "PlayerSafeSignature"
            })
            {
                Add(checks, "leak.runtime." + token, "0",
                    CountOrdinal(runtime, token).ToString(CultureInfo.InvariantCulture),
                    CountOrdinal(runtime, token) == 0);
            }
            string fixtureText = Read(root, FixtureReport);
            foreach (string token in new[]
            {
                "capability.placement_shape", "dev_encounter_3_10",
                "dev_encounter_4_10", "requirement.", "2500"
            })
            {
                Add(checks, "leak.fixture." + token, "0",
                    CountOrdinal(fixtureText, token).ToString(CultureInfo.InvariantCulture),
                    CountOrdinal(fixtureText, token) == 0);
            }
            Add(checks, "leak.fixture-prefix", "10", outcomes.Count(value =>
                value.Id.StartsWith("dev_layout_case_", StringComparison.Ordinal))
                .ToString(CultureInfo.InvariantCulture), outcomes.All(value =>
                value.Id.StartsWith("dev_layout_case_", StringComparison.Ordinal)));
            Add(checks, "invalid.reported-minimum", ">=24",
                invalidCount.ToString(CultureInfo.InvariantCulture), invalidCount >= 24);
        }

        private static void VerifyBaselineBehavior(string root, ICollection<Check> checks)
        {
            string[] c02 = CsvRows(root, "Docs/V0.4/Reports/ItemEnemyMatchupMatrix.csv");
            Add(checks, "baseline.c02.rows", "32", c02.Length.ToString(
                CultureInfo.InvariantCulture), c02.Length == 32);
            Add(checks, "baseline.c02.blocked", "32", c02.Count(value =>
                value.Contains("BLOCKED_BY_UNKNOWN")).ToString(CultureInfo.InvariantCulture),
                c02.Count(value => value.Contains("BLOCKED_BY_UNKNOWN")) == 32);
            string n01a = Read(root,
                "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerSemanticSurveyReport.md");
            AddContains(checks, "baseline.n01a.unknown-reduction", n01a,
                "Actual Unknown-reference reduction: `0`");
            AddContains(checks, "baseline.n01a.remaining", n01a,
                "Remaining decisive Unknown references: `64`");
            AddContains(checks, "baseline.n01a.blocked", n01a, "Blocked rows: `32/32`");
            AddContains(checks, "baseline.n01a.signature", n01a, ExpectedN01ASignature);
            AddContains(checks, "baseline.signature.if01", n01a, ExpectedIF01Signature);
            AddContains(checks, "baseline.signature.if02", n01a, ExpectedIF02Signature);
            AddContains(checks, "baseline.signature.if03", n01a, ExpectedIF03Signature);
            AddContains(checks, "baseline.signature.affix", n01a, ExpectedAffixSignature);
            AddContains(checks, "baseline.signature.roll", n01a, ExpectedRollSignature);
            AddContains(checks, "baseline.signature.projection", n01a,
                ExpectedProjectionSignature);
            string n01b = Read(root,
                "Docs/V0.4/Reports/EnemyRequirementChannelApplicabilitySchemaContractReport.md");
            AddContains(checks, "baseline.n01b.signature", n01b, ExpectedN01BSignature);
            string[] impact = CsvRows(root,
                "Docs/V0.4/Reports/BuildCapabilityRemainingBlockerC02Impact.csv");
            Add(checks, "baseline.placement-shape.unknown", "32", impact.Count(value =>
                value.Contains("placement_shape")).ToString(CultureInfo.InvariantCulture),
                impact.Count(value => value.Contains("placement_shape")) == 32);
            Add(checks, "baseline.projected-reduction", "0", impact.Count(value =>
                !value.Contains("ACTUAL_REDUCTION_ZERO") &&
                value.Contains("ACTUAL_REDUCTION")).ToString(CultureInfo.InvariantCulture),
                impact.All(value => value.Contains("ACTUAL_REDUCTION_ZERO")));
        }

        private static void VerifyRepositoryBoundary(string root, ICollection<Check> checks)
        {
            string head = RunGit(root, "rev-parse HEAD").Trim();
            Add(checks, "repo.head", ExpectedHead, head, head == ExpectedHead);
            string tree = RunGit(root, "ls-tree -r --name-only HEAD").Replace('\\', '/');
            foreach (string path in OutputPaths)
            {
                Add(checks, "repo.new." + Path.GetFileName(path), "False",
                    TreeContains(tree, path).ToString(), !TreeContains(tree, path));
            }
            string[] modified = RunGit(root, "diff --name-only").Split(
                new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Replace('\\', '/')).ToArray();
            string[] accepted =
            {
                "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset",
                "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs"
            };
            Add(checks, "repo.existing-modified-preexisting-only", string.Join("|", accepted),
                string.Join("|", modified), modified.OrderBy(value => value,
                    StringComparer.Ordinal).SequenceEqual(accepted.OrderBy(value => value,
                    StringComparer.Ordinal)));
            Add(checks, "repo.preexisting.catalog", ExpectedPreexistingCatalogHash,
                FileHash(root, accepted[0], false),
                FileHash(root, accepted[0], false) == ExpectedPreexistingCatalogHash);
            Add(checks, "repo.preexisting.candidate", ExpectedPreexistingCandidateHash,
                FileHash(root, accepted[1], false),
                FileHash(root, accepted[1], false) == ExpectedPreexistingCandidateHash);
            Add(checks, "repo.package-item-paths", "0", OutputPaths.Count(value =>
                value.StartsWith("Assets/_Game/Scripts/TalismanBag/Items/",
                    StringComparison.Ordinal)).ToString(CultureInfo.InvariantCulture),
                OutputPaths.All(value => !value.StartsWith(
                    "Assets/_Game/Scripts/TalismanBag/Items/", StringComparison.Ordinal)));

            int whitespace = OutputPaths.Sum(path => File.ReadAllLines(Absolute(root, path),
                Encoding.UTF8).Count(line => line.EndsWith(" ", StringComparison.Ordinal) ||
                    line.EndsWith("\t", StringComparison.Ordinal)));
            Add(checks, "leak.trailing-whitespace", "0", whitespace.ToString(
                CultureInfo.InvariantCulture), whitespace == 0);
            string[] metas = OutputPaths.Where(value => value.EndsWith(".meta",
                StringComparison.Ordinal)).ToArray();
            string[] guids = metas.Select(path => ReadGuid(Absolute(root, path))).ToArray();
            Add(checks, "leak.guid.new-unique", "6", guids.Distinct(StringComparer.Ordinal)
                .Count().ToString(CultureInfo.InvariantCulture), guids.Length == 6 &&
                guids.Distinct(StringComparer.Ordinal).Count() == 6);
            string[] allGuids = Directory.GetFiles(root, "*.meta", SearchOption.AllDirectories)
                .Select(ReadGuid).Where(value => value.Length > 0).ToArray();
            Add(checks, "leak.guid.project-collision", "0", guids.Count(guid =>
                allGuids.Count(value => value == guid) != 1).ToString(
                    CultureInfo.InvariantCulture), guids.All(guid =>
                allGuids.Count(value => value == guid) == 1));
        }

        private static List<OutcomeCase> BuildOutcomeCases()
        {
            List<OutcomeCase> cases = new List<OutcomeCase>();
            cases.Add(new OutcomeCase("dev_layout_case_01_safe_pollution",
                Input(BaseBuild(), Pressure("dev_layout_pressure_safe",
                    Kinds(LayoutPressureKind.PollutedCellMask), Domain(), Domain().Where(
                        value => !value.Equals(Cell(1, 2))), Cell(0, 0), BaseConnections(),
                    AllClauses()), evaluationId: "dev_layout_case_01_safe_pollution"),
                LayoutResiliencePredicateState.KnownTrue, string.Empty,
                "PollutedCellMask"));
            cases.Add(new OutcomeCase("dev_layout_case_02_counted_cell_unusable",
                Input(BaseBuild(), Pressure("dev_layout_pressure_cell_block",
                    Kinds(LayoutPressureKind.PollutedCellMask), Domain(), Domain().Where(
                        value => !value.Equals(Cell(0, 1))), Cell(0, 0), BaseConnections().Where(
                        value => !value.CellB.Equals(Cell(0, 1))),
                    Clauses(LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable)),
                    evaluationId: "dev_layout_case_02_counted_cell_unusable"),
                LayoutResiliencePredicateState.KnownFalse,
                "CountedPlacementCellsUsable", "PollutedCellMask"));
            cases.Add(new OutcomeCase("dev_layout_case_03_eye_relocated_connected",
                Input(BaseBuild(), Pressure("dev_layout_pressure_eye_relocated",
                    Kinds(LayoutPressureKind.EyeRelocationOrDisruption), Domain(), Domain(),
                    Cell(1, 0), BaseConnections(), Clauses(
                        LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable,
                        LayoutResiliencePredicateClauseKind
                            .EyeToCountedCoreStructurallyConnected)),
                    evaluationId: "dev_layout_case_03_eye_relocated_connected"),
                LayoutResiliencePredicateState.KnownTrue, string.Empty,
                "EyeRelocationOrDisruption"));
            cases.Add(new OutcomeCase("dev_layout_case_04_connection_cut",
                Input(BaseBuild(), Pressure("dev_layout_pressure_connection_cut",
                    Kinds(LayoutPressureKind.StructuralConnectionCut), Domain(), Domain(),
                    Cell(0, 0), BaseConnections().Where(value =>
                        !(value.CellA.Equals(Cell(2, 0)) && value.CellB.Equals(Cell(2, 1)))),
                    Clauses(LayoutResiliencePredicateClauseKind
                        .EyeToCountedCoreStructurallyConnected)),
                    evaluationId: "dev_layout_case_04_connection_cut"),
                LayoutResiliencePredicateState.KnownFalse,
                "EyeToCountedCoreStructurallyConnected", "StructuralConnectionCut"));
            cases.Add(new OutcomeCase("dev_layout_case_05_empty_counted_required_present",
                Input(EmptyCountedBuild(), BasePressure(Clauses(
                    LayoutResiliencePredicateClauseKind.CountedLayoutPresent)),
                    evaluationId: "dev_layout_case_05_empty_counted_required_present"),
                LayoutResiliencePredicateState.KnownFalse, "CountedLayoutPresent",
                "PollutedCellMask"));
            cases.Add(new OutcomeCase("dev_layout_case_06_empty_counted_vacuous_usable",
                Input(EmptyCountedBuild(), BasePressure(Clauses(
                    LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable)),
                    evaluationId: "dev_layout_case_06_empty_counted_vacuous_usable"),
                LayoutResiliencePredicateState.KnownTrue, string.Empty,
                "PollutedCellMask"));
            cases.Add(new OutcomeCase("dev_layout_case_07_applicability_unknown",
                Input(BaseBuild(), BasePressure(AllClauses()),
                    EnemyRequirementApplicabilityState.Unknown,
                    evaluationId: "dev_layout_case_07_applicability_unknown"),
                LayoutResiliencePredicateState.Unknown, string.Empty,
                "PollutedCellMask"));
            cases.Add(new OutcomeCase("dev_layout_case_08_build_incomplete",
                new LayoutResilienceEvaluationInput("dev_layout_case_08_build_incomplete",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Incomplete,
                    LayoutResilienceInputCompleteness.Complete, null,
                    BasePressure(AllClauses())), LayoutResiliencePredicateState.Unknown,
                string.Empty, "PollutedCellMask"));
            cases.Add(new OutcomeCase("dev_layout_case_09_pressure_incomplete",
                new LayoutResilienceEvaluationInput("dev_layout_case_09_pressure_incomplete",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Complete,
                    LayoutResilienceInputCompleteness.Incomplete, BaseBuild(), null),
                LayoutResiliencePredicateState.Unknown, string.Empty, "Incomplete"));
            cases.Add(new OutcomeCase("dev_layout_case_10_not_applicable",
                new LayoutResilienceEvaluationInput("dev_layout_case_10_not_applicable",
                    EnemyRequirementApplicabilityState.NotApplicable,
                    LayoutResilienceInputCompleteness.NotRequired,
                    LayoutResilienceInputCompleteness.NotRequired, null, null),
                LayoutResiliencePredicateState.NotApplicable, string.Empty, "NotRequired"));
            return cases;
        }

        private static LayoutResilienceBuildFactSnapshot BaseBuild()
        {
            return Build(3, Cell(0, 0), BasePlacements());
        }

        private static LayoutResilienceBuildFactSnapshot EmptyCountedBuild()
        {
            return Build(3, Cell(0, 0), BasePlacements().Where(value =>
                !value.IsCountedInBuild));
        }

        private static LayoutResilienceBuildFactSnapshot BuildWith(
            LayoutResiliencePlacedItemFactSnapshot row)
        {
            return Build(3, Cell(0, 0), new[] { row });
        }

        private static LayoutResilienceBuildFactSnapshot Build(
            int? boardSize,
            LayoutCellCoordinate eye,
            IEnumerable<LayoutResiliencePlacedItemFactSnapshot> rows)
        {
            return new LayoutResilienceBuildFactSnapshot(boardSize, eye, rows);
        }

        private static LayoutResiliencePlacedItemFactSnapshot[] BasePlacements()
        {
            return new[]
            {
                Placement("dev_layout_placement_a", "dev_layout_item_a", Cell(0, 1), 0,
                    Cells(0, 0), Cells(0, 1), Cell(0, 1), true, true),
                Placement("dev_layout_placement_b", "dev_layout_item_b", Cell(2, 1), 0,
                    Cells(0, 0), Cells(2, 1), Cell(2, 1), true, true),
                Placement("dev_layout_placement_uncounted", "dev_layout_item_uncounted",
                    Cell(1, 2), 0, Cells(0, 0), Cells(1, 2), Cell(1, 2), false, false)
            };
        }

        private static LayoutResiliencePlacedItemFactSnapshot Placement(
            string placementId, string itemId, LayoutCellCoordinate anchor, int rotation,
            IEnumerable<LayoutCellCoordinate> shape,
            IEnumerable<LayoutCellCoordinate> occupied, LayoutCellCoordinate core,
            bool lit, bool counted)
        {
            return new LayoutResiliencePlacedItemFactSnapshot(placementId, itemId, anchor,
                rotation, shape, occupied, core, lit, counted);
        }

        private static LayoutPressureSnapshot BasePressure(
            IEnumerable<LayoutResiliencePredicateClauseKind> clauses)
        {
            return Pressure("dev_layout_pressure_base",
                Kinds(LayoutPressureKind.PollutedCellMask), Domain(), Domain(), Cell(0, 0),
                BaseConnections(), clauses);
        }

        private static LayoutPressureSnapshot Pressure(
            string id, IEnumerable<LayoutPressureKind> kinds,
            IEnumerable<LayoutCellCoordinate> domain,
            IEnumerable<LayoutCellCoordinate> usable, LayoutCellCoordinate eye,
            IEnumerable<LayoutCellConnection> connections,
            IEnumerable<LayoutResiliencePredicateClauseKind> clauses)
        {
            return new LayoutPressureSnapshot(id, kinds, domain, usable, eye, connections,
                clauses);
        }

        private static LayoutResilienceEvaluationInput Input(
            LayoutResilienceBuildFactSnapshot build,
            LayoutPressureSnapshot pressure,
            EnemyRequirementApplicabilityState applicability =
                EnemyRequirementApplicabilityState.Applicable,
            string evaluationId = "dev_layout_invalid",
            string schemaId = LayoutResilienceStructuralPredicateSchema.SchemaId,
            int schemaVersion = LayoutResilienceStructuralPredicateSchema.SchemaVersion)
        {
            return new LayoutResilienceEvaluationInput(evaluationId, applicability,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Complete, build, pressure, schemaId,
                schemaVersion);
        }

        private static LayoutCellCoordinate Cell(int x, int y)
        {
            return new LayoutCellCoordinate(x, y);
        }

        private static LayoutCellCoordinate[] Cells(params int[] coordinates)
        {
            List<LayoutCellCoordinate> cells = new List<LayoutCellCoordinate>();
            for (int index = 0; index + 1 < coordinates.Length; index += 2)
            {
                cells.Add(Cell(coordinates[index], coordinates[index + 1]));
            }
            return cells.ToArray();
        }

        private static LayoutCellCoordinate[] Domain()
        {
            List<LayoutCellCoordinate> cells = new List<LayoutCellCoordinate>();
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++) cells.Add(Cell(x, y));
            }
            return cells.ToArray();
        }

        private static LayoutCellConnection Connection(int ax, int ay, int bx, int by)
        {
            return new LayoutCellConnection(Cell(ax, ay), Cell(bx, by));
        }

        private static LayoutCellConnection[] BaseConnections()
        {
            return new[]
            {
                Connection(0, 0, 1, 0), Connection(1, 0, 2, 0),
                Connection(0, 0, 0, 1), Connection(2, 0, 2, 1)
            };
        }

        private static LayoutPressureKind[] Kinds(params LayoutPressureKind[] values)
        {
            return values;
        }

        private static LayoutResiliencePredicateClauseKind[] Clauses(
            params LayoutResiliencePredicateClauseKind[] values)
        {
            return values;
        }

        private static LayoutResiliencePredicateClauseKind[] AllClauses()
        {
            return Enum.GetValues(typeof(LayoutResiliencePredicateClauseKind))
                .Cast<LayoutResiliencePredicateClauseKind>().ToArray();
        }

        private static LayoutResilienceInputCompleteness[] CompletenessValues()
        {
            return new[] { LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Incomplete,
                LayoutResilienceInputCompleteness.NotRequired };
        }

        private static LayoutResiliencePredicateClauseSnapshot Clause(
            LayoutResiliencePredicateClauseKind kind,
            LayoutResiliencePredicateClauseState state)
        {
            return new LayoutResiliencePredicateClauseSnapshot(kind, state);
        }

        private static LayoutResiliencePredicateResultSnapshot Forged(
            LayoutResilienceEvaluationInput input, LayoutResiliencePredicateState state,
            IEnumerable<LayoutResiliencePredicateClauseSnapshot> clauses, string signature)
        {
            return new LayoutResiliencePredicateResultSnapshot(input.SchemaId,
                input.SchemaVersion, input.EvaluationId, input.ApplicabilityState, state,
                clauses, signature);
        }

        private static void AssertInvalid(ICollection<Check> checks, string id,
            LayoutResilienceEvaluationInput input, string code)
        {
            IReadOnlyList<LayoutResilienceValidationIssue> issues =
                DefaultLayoutResilienceStructuralPredicateValidator.Instance.Validate(input);
            Add(checks, id, code, string.Join("|", issues.Select(value => value.Code)),
                issues.Any(value => value.Code == code));
        }

        private static void AssertInvalidResult(ICollection<Check> checks, string id,
            LayoutResilienceEvaluationInput input,
            LayoutResiliencePredicateResultSnapshot result, string code)
        {
            IReadOnlyList<LayoutResilienceValidationIssue> issues =
                DefaultLayoutResilienceStructuralPredicateValidator.Instance
                    .ValidateResult(input, result);
            Add(checks, id, code, string.Join("|", issues.Select(value => value.Code)),
                issues.Any(value => value.Code == code));
        }

        private static void AssertEnum<T>(ICollection<Check> checks, string id,
            string[] names, int[] values)
        {
            string[] actualNames = Enum.GetNames(typeof(T));
            int[] actualValues = Enum.GetValues(typeof(T)).Cast<object>()
                .Select(value => Convert.ToInt32(value, CultureInfo.InvariantCulture))
                .ToArray();
            Add(checks, "enum." + id + ".names", string.Join("|", names),
                string.Join("|", actualNames), names.SequenceEqual(actualNames));
            Add(checks, "enum." + id + ".values", string.Join("|", values),
                string.Join("|", actualValues), values.SequenceEqual(actualValues));
            Add(checks, "enum." + id + ".zero-absent", "False",
                Enum.IsDefined(typeof(T), 0).ToString(), !Enum.IsDefined(typeof(T), 0));
            Add(checks, "enum." + id + ".undefined-absent", "False",
                Enum.IsDefined(typeof(T), 99).ToString(), !Enum.IsDefined(typeof(T), 99));
        }

        private static void AssertProperties(ICollection<Check> checks, string id,
            Type type, params string[] expected)
        {
            string[] actual = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(value => value.Name).OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            string[] sorted = expected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(checks, id + ".exact", string.Join("|", sorted), string.Join("|", actual),
                sorted.SequenceEqual(actual));
            Add(checks, id + ".readonly", "0", type.GetProperties(
                BindingFlags.Instance | BindingFlags.Public).Count(value =>
                value.SetMethod != null).ToString(CultureInfo.InvariantCulture),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .All(value => value.SetMethod == null));
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 && value.StartsWith("sha256:",
                StringComparison.Ordinal) && value.Substring(7).All(character =>
                (character >= '0' && character <= '9') ||
                (character >= 'a' && character <= 'f'));
        }

        private static void AddThrowsNotSupported(ICollection<Check> checks, string id,
            Action action)
        {
            bool threw = false;
            try { action(); }
            catch (NotSupportedException) { threw = true; }
            Add(checks, id, "True", threw.ToString(), threw);
        }

        private static string[] CsvRows(string root, string path)
        {
            return File.ReadAllLines(Absolute(root, path), Encoding.UTF8).Skip(1)
                .Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        }

        private static int CountOrdinal(string source, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = (source ?? string.Empty).IndexOf(token, index,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static void VerifyProtection(ICollection<Check> checks,
            ProtectionSnapshot before, ProtectionSnapshot after)
        {
            AddProtection(checks, "item", before.Item, after.Item, 105, ExpectedItemHash);
            AddProtection(checks, "enemy", before.Enemy, after.Enemy, 89, ExpectedEnemyHash);
            AddProtection(checks, "c01", before.C01, after.C01, 9, ExpectedC01Hash);
            AddProtection(checks, "c02", before.C02, after.C02, 9, ExpectedC02Hash);
            AddProtection(checks, "c02a", before.C02A, after.C02A, 7, ExpectedC02AHash);
            AddProtection(checks, "n01", before.N01, after.N01, 8, ExpectedN01Hash);
            AddProtection(checks, "c02a-d1", before.C02AD1, after.C02AD1, 7,
                ExpectedC02AD1Hash);
            AddProtection(checks, "n01a", before.N01A, after.N01A, 8, ExpectedN01AHash);
            AddProtection(checks, "n01b", before.N01B, after.N01B, 14, ExpectedN01BHash);
            AddProtection(checks, "e06", before.E06, after.E06, 16, ExpectedE06Hash);
            AddProtection(checks, "e07", before.E07, after.E07, 16, ExpectedE07Hash);
            AddProtection(checks, "e08", before.E08, after.E08, 16, ExpectedE08Hash);
            AddProtection(checks, "e10", before.E10, after.E10, 21, ExpectedE10Hash);
            AddProtection(checks, "scenes", before.Scenes, after.Scenes, 14,
                ExpectedScenesHash);
            AddProtection(checks, "prefabs", before.Prefabs, after.Prefabs, 16,
                ExpectedPrefabsHash);
            Add(checks, "protected.build-settings.accepted", ExpectedBuildSettingsHash,
                before.BuildSettings, before.BuildSettings == ExpectedBuildSettingsHash);
            Add(checks, "protected.build-settings.before-after", before.BuildSettings,
                after.BuildSettings, before.BuildSettings == after.BuildSettings);
        }

        private static void AddProtection(ICollection<Check> checks, string id,
            AggregateHash before, AggregateHash after, int count, string hash)
        {
            Add(checks, "protected." + id + ".count", count.ToString(
                CultureInfo.InvariantCulture), before.FileCount.ToString(
                CultureInfo.InvariantCulture), before.FileCount == count);
            Add(checks, "protected." + id + ".accepted", hash, before.Hash,
                before.Hash == hash);
            Add(checks, "protected." + id + ".before-after", before.Hash, after.Hash,
                before.Equals(after));
        }

        private static ProtectionSnapshot CaptureProtection(string root)
        {
            return new ProtectionSnapshot(AggregateItem(root), AggregateEnemyExisting(root),
                AggregateFiles(root, C01Files, false), AggregateFiles(root, C02Files, false),
                AggregateFiles(root, C02AFiles, false), AggregateFiles(root, N01Files, true),
                AggregateFiles(root, C02AD1Files, true),
                AggregateFiles(root, N01AFiles, true), AggregateFiles(root, N01BFiles, true),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/CounterWindowAndPressureSchemaVerifier.cs",
                    "CounterWindowAndPressureSchema", 5),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/BuildCapabilityReadContractVerifier.cs",
                    "BuildCapabilityReadContract", 5),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/ReadinessEvaluation.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyOfflineReadinessEvaluatorVerifier.cs",
                    "EnemyOfflineReadinessEvaluator", 5),
                AggregatePackage(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData",
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData.meta",
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs",
                    "DevEncounterSeed", 8),
                AggregateDirectory(root, "Assets/_Game/Scenes", true),
                AggregateDirectory(root, "Assets/_Game/Prefabs", true),
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false));
        }

        private static AggregateHash AggregatePackage(string root, string runtimeDirectory,
            string runtimeMeta, string verifier, string reportPrefix, int reportCount)
        {
            List<string> paths = Directory.GetFiles(Absolute(root, runtimeDirectory), "*",
                SearchOption.AllDirectories).Select(value => Relative(root, value)).ToList();
            paths.Add(runtimeMeta);
            paths.Add(verifier);
            paths.Add(verifier + ".meta");
            string[] reports = Directory.GetFiles(Absolute(root, ReportDirectory),
                reportPrefix + "*", SearchOption.TopDirectoryOnly).Select(value =>
                Relative(root, value)).ToArray();
            if (reports.Length != reportCount) paths.Add("__unexpected_" + reports.Length);
            paths.AddRange(reports);
            return AggregateFiles(root, paths, true);
        }

        private static AggregateHash AggregateItem(string root)
        {
            string directory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Items");
            string excluded = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                SearchOption.AllDirectories).Where(value => !value.StartsWith(excluded,
                StringComparison.OrdinalIgnoreCase) && !string.Equals(value, excludedMeta,
                StringComparison.OrdinalIgnoreCase)).Select(value => Relative(root, value)),
                false);
        }

        private static AggregateHash AggregateEnemyExisting(string root)
        {
            string directory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem");
            string excluded = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                SearchOption.AllDirectories).Where(value => !value.StartsWith(excluded,
                StringComparison.OrdinalIgnoreCase) && !string.Equals(value, excludedMeta,
                StringComparison.OrdinalIgnoreCase)).Select(value => Relative(root, value)),
                false);
        }

        private static AggregateHash AggregateDirectory(string root, string directory,
            bool trailing)
        {
            return AggregateFiles(root, Directory.GetFiles(Absolute(root, directory), "*",
                SearchOption.AllDirectories).Select(value => Relative(root, value)), trailing);
        }

        private static AggregateHash AggregateFiles(string root, IEnumerable<string> paths,
            bool trailing)
        {
            string[] ordered = paths.OrderBy(value => value,
                trailing ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase).ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                payload.Append(ordered[index].Replace('\\', '/')).Append('|')
                    .Append(FileHash(root, ordered[index], !trailing));
                if (trailing || index + 1 < ordered.Length) payload.Append('\n');
            }
            return new AggregateHash(ordered.Length, Sha256(
                Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static string FileHash(string root, string path, bool uppercase)
        {
            return Sha256(File.ReadAllBytes(Absolute(root, path)), uppercase);
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(bytes ?? new byte[0]).Select(value =>
                    value.ToString(uppercase ? "X2" : "x2", CultureInfo.InvariantCulture)));
            }
        }

        private static string RunGit(string root, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = root, UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0) throw new InvalidOperationException(
                    "git " + arguments + " failed: " + error);
                return output;
            }
        }

        private static bool TreeContains(string tree, string path)
        {
            return ("\n" + tree.Trim() + "\n").IndexOf("\n" + path.Replace('\\', '/') +
                "\n", StringComparison.Ordinal) >= 0;
        }

        private static string ReadGuid(string path)
        {
            string line = File.ReadLines(path).FirstOrDefault(value =>
                value.StartsWith("guid: ", StringComparison.Ordinal));
            return line == null ? string.Empty : line.Substring(6).Trim();
        }

        private static string Read(string root, string path)
        {
            return File.ReadAllText(Absolute(root, path), Encoding.UTF8);
        }

        private static string FindProjectRoot()
        {
            foreach (string seed in new[] { Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory })
            {
                DirectoryInfo current = new DirectoryInfo(seed);
                while (current != null)
                {
                    if (Directory.Exists(Path.Combine(current.FullName, "Assets")) &&
                        Directory.Exists(Path.Combine(current.FullName, "ProjectSettings")) &&
                        Directory.Exists(Path.Combine(current.FullName, "Packages")))
                        return current.FullName;
                    current = current.Parent;
                }
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string path)
        {
            return Path.Combine(root, (path ?? string.Empty).Replace('/',
                Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string path)
        {
            Uri rootUri = new Uri(root.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(new Uri(path)).ToString())
                .Replace('\\', '/');
        }

        private static void AddContains(ICollection<Check> checks, string id,
            string source, string expected)
        {
            Add(checks, id, expected, source == null ? "<null>" : "present/absent",
                source != null && source.IndexOf(expected, StringComparison.Ordinal) >= 0);
        }

        private static void Add(ICollection<Check> checks, string id, string expected,
            string actual, bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private sealed class OutcomeCase
        {
            public OutcomeCase(string id, LayoutResilienceEvaluationInput input,
                LayoutResiliencePredicateState expectedState, string expectedViolated,
                string pressureKind)
            {
                Id = id; Input = input; ExpectedState = expectedState;
                ExpectedViolated = expectedViolated; PressureKind = pressureKind;
            }
            public string Id { get; private set; }
            public LayoutResilienceEvaluationInput Input { get; private set; }
            public LayoutResiliencePredicateState ExpectedState { get; private set; }
            public string ExpectedViolated { get; private set; }
            public string PressureKind { get; private set; }
            public LayoutResiliencePredicateResultSnapshot Result { get; set; }
        }

        private sealed class InvalidCase
        {
            public InvalidCase(string id, LayoutResilienceEvaluationInput input, string code)
            { Id = id; Input = input; ExpectedCode = code; }
            public string Id { get; private set; }
            public LayoutResilienceEvaluationInput Input { get; private set; }
            public string ExpectedCode { get; private set; }
        }

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            { Id = id; Expected = expected; Actual = actual; Passed = passed; }
            public string Id { get; private set; }
            public string Expected { get; private set; }
            public string Actual { get; private set; }
            public bool Passed { get; private set; }
        }

        private sealed class AggregateHash : IEquatable<AggregateHash>
        {
            public AggregateHash(int count, string hash) { FileCount = count; Hash = hash; }
            public int FileCount { get; private set; }
            public string Hash { get; private set; }
            public bool Equals(AggregateHash other) { return other != null &&
                FileCount == other.FileCount && Hash == other.Hash; }
            public override bool Equals(object obj) { return Equals(obj as AggregateHash); }
            public override int GetHashCode() { return (FileCount * 397) ^ Hash.GetHashCode(); }
        }

        private sealed class ProtectionSnapshot
        {
            public ProtectionSnapshot(AggregateHash item, AggregateHash enemy,
                AggregateHash c01, AggregateHash c02, AggregateHash c02A,
                AggregateHash n01, AggregateHash c02AD1, AggregateHash n01A,
                AggregateHash n01B, AggregateHash e06, AggregateHash e07,
                AggregateHash e08, AggregateHash e10, AggregateHash scenes,
                AggregateHash prefabs, string buildSettings)
            {
                Item = item; Enemy = enemy; C01 = c01; C02 = c02; C02A = c02A;
                N01 = n01; C02AD1 = c02AD1; N01A = n01A; N01B = n01B;
                E06 = e06; E07 = e07; E08 = e08; E10 = e10;
                Scenes = scenes; Prefabs = prefabs; BuildSettings = buildSettings;
            }
            public AggregateHash Item { get; private set; }
            public AggregateHash Enemy { get; private set; }
            public AggregateHash C01 { get; private set; }
            public AggregateHash C02 { get; private set; }
            public AggregateHash C02A { get; private set; }
            public AggregateHash N01 { get; private set; }
            public AggregateHash C02AD1 { get; private set; }
            public AggregateHash N01A { get; private set; }
            public AggregateHash N01B { get; private set; }
            public AggregateHash E06 { get; private set; }
            public AggregateHash E07 { get; private set; }
            public AggregateHash E08 { get; private set; }
            public AggregateHash E10 { get; private set; }
            public AggregateHash Scenes { get; private set; }
            public AggregateHash Prefabs { get; private set; }
            public string BuildSettings { get; private set; }
        }
    }
}
