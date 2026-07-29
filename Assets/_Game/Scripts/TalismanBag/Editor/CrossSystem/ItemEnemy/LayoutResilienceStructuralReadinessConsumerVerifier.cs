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
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness;
using TalismanBag.EnemySystem.RequirementChannel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class LayoutResilienceStructuralReadinessConsumerVerifier
    {
        private const string ExpectedSignature =
            "sha256:e38538f2de1b85ce00721236790f9b2beab63f19df076a34e0508aad4993c01a";
        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer";
        private const string ReportDirectory = "Docs/V0.4/Reports";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory + "/LayoutResilienceStructuralReadinessConsumerPrimitives.cs",
            RuntimeDirectory + "/LayoutResilienceStructuralReadinessConsumerPrimitives.cs.meta",
            RuntimeDirectory + "/LayoutResilienceStructuralReadinessConsumer.cs",
            RuntimeDirectory + "/LayoutResilienceStructuralReadinessConsumer.cs.meta",
            RuntimeDirectory + "/LayoutResilienceStructuralReadinessConsumerValidation.cs",
            RuntimeDirectory + "/LayoutResilienceStructuralReadinessConsumerValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs.meta",
            ReportDirectory + "/LayoutResilienceStructuralReadinessConsumerReport.md",
            ReportDirectory + "/LayoutResilienceStructuralReadinessConsumerSpec.csv",
            ReportDirectory + "/LayoutResilienceStructuralReadinessConsumerTruthTable.csv",
            ReportDirectory + "/LayoutResilienceStructuralReadinessConsumerFixtureCases.csv",
            ReportDirectory + "/LayoutResilienceStructuralReadinessConsumerLeakCheckReport.md"
        };

        private static readonly string[] P3Files =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssembler.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssembler.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly/LayoutResilienceEvaluationInputAssemblerValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs.meta",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerReport.md",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerSpec.csv",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerTruthTable.csv",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerFixtureCases.csv",
            "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerLeakCheckReport.md"
        };

        private static readonly string[] P2Files =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourcePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourcePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs.meta",
            "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterReport.md",
            "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterSpec.csv",
            "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterFieldMatrix.csv",
            "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterFixtureCases.csv",
            "Docs/V0.4/Reports/AuthoredLayoutPressureSourceAdapterLeakCheckReport.md"
        };

        private static readonly string[] P1Files =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionAdapter.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionAdapter.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection/LayoutResilienceItemFactProjectionValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs.meta",
            "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterReport.md",
            "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterSpec.csv",
            "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterFieldMatrix.csv",
            "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterFixtureCases.csv",
            "Docs/V0.4/Reports/LayoutResilienceItemFactProjectionAdapterLeakCheckReport.md"
        };

        private static readonly string[] N01CFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicatePrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateSnapshots.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/LayoutResilienceStructuralPredicateEvaluator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs.meta",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractReport.md",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractSpec.csv",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFieldMatrix.csv",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractFixtureCases.csv",
            "Docs/V0.4/Reports/LayoutResilienceStructuralPredicateContractLeakCheckReport.md"
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

        private static readonly string[] IF01Files =
        {
            "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs",
            "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingValidator.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemInstancePlacementBindingContractVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemInstancePlacementBindingContractVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractReport.md",
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractSpec.csv",
            "Docs/V0.4/Reports/ItemInstancePlacementBindingContractLeakCheckReport.md"
        };

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

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Layout Resilience Structural Readiness Consumer")]
        public static void VerifyMenu()
        {
            VerifyOffline();
        }
#endif

        public static void VerifyOffline()
        {
            Summary summary = VerifyCore();
            if (summary.Failed != 0)
            {
                throw new InvalidOperationException(summary.Message);
            }
            Console.WriteLine(summary.Message);
        }

        public static void VerifyStaticBatch()
        {
            try
            {
                VerifyOffline();
#if UNITY_EDITOR
                UnityEngine.Debug.Log(
                    "LAYOUT_RESILIENCE_STRUCTURAL_READINESS_CONSUMER PASS");
                EditorApplication.Exit(0);
#endif
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogException(exception);
                EditorApplication.Exit(1);
#else
                Console.Error.WriteLine(exception);
                throw;
#endif
            }
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

        private static Summary VerifyCore()
        {
            string root = FindRoot();
            List<Check> checks = new List<Check>();
            AssertEnum<LayoutResilienceStructuralReadinessConsumerStatus>(checks,
                "consumer-status",
                new[] { "Complete", "Unknown", "Invalid" },
                new[] { 1, 2, 3 });
            Add(checks, "schema.id",
                "LayoutResilienceStructuralReadinessConsumer.v1",
                LayoutResilienceStructuralReadinessConsumerSchema.SchemaId,
                LayoutResilienceStructuralReadinessConsumerSchema.SchemaId ==
                    "LayoutResilienceStructuralReadinessConsumer.v1");
            Add(checks, "schema.version", "1",
                LayoutResilienceStructuralReadinessConsumerSchema.SchemaVersion
                    .ToString(CultureInfo.InvariantCulture),
                LayoutResilienceStructuralReadinessConsumerSchema.SchemaVersion == 1);
            VerifySurface(checks);

            IReadOnlyList<Fixture> fixtures = CreateFixtures(checks);
            Add(checks, "fixtures.count", "20", fixtures.Count.ToString(
                CultureInfo.InvariantCulture), fixtures.Count == 20);
            foreach (Fixture fixture in fixtures)
            {
                Add(checks, "fixture." + fixture.Id, "PASS",
                    fixture.Passed ? "PASS" : fixture.Detail, fixture.Passed);
            }
            Add(checks, "fixtures.pass", "20",
                fixtures.Count(value => value.Passed).ToString(
                    CultureInfo.InvariantCulture),
                fixtures.Count(value => value.Passed) == 20);

            string observed = fixtures[0].Result.CanonicalSignature;
            Add(checks, "canonical.format", "sha256+64 lowercase hex", observed,
                IsSignature(observed));
            Add(checks, "canonical.fixed", ExpectedSignature, observed,
                observed == ExpectedSignature);

            VerifySourceBoundaries(checks, root);
            VerifyReports(checks, root);
            VerifyProtected(checks, root);

            int failed = checks.Count(value => !value.Passed);
            string failures = string.Join(" | ", checks.Where(value => !value.Passed)
                .Select(value => value.Id + " expected=" + value.Expected +
                    " actual=" + value.Actual));
            string message = "LayoutResilienceStructuralReadinessConsumer verifier: " +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) + "/" +
                checks.Count.ToString(CultureInfo.InvariantCulture) +
                " PASS; fixtures=" + fixtures.Count(value => value.Passed).ToString(
                    CultureInfo.InvariantCulture) + "/20; signature=" + observed +
                "; C02 blocked=32/32; Actual Unknown reduction=0" +
                (failed == 0 ? string.Empty : "; failures=" + failures);
            return new Summary(failed, message);
        }

        private static void VerifySurface(ICollection<Check> checks)
        {
            AssertProperties(checks, "issue",
                typeof(LayoutResilienceStructuralReadinessConsumerIssue),
                new[] { "Code", "Status", "Path", "Message" });
            AssertProperties(checks, "input",
                typeof(LayoutResilienceStructuralReadinessConsumerInput),
                new[] { "SchemaId", "SchemaVersion", "AssemblerResult" });
            AssertProperties(checks, "result",
                typeof(LayoutResilienceStructuralReadinessConsumerResult),
                new[]
                {
                    "SchemaId", "SchemaVersion", "Status", "PredicateResult",
                    "Issues", "CanonicalSignature"
                });

            ConstructorInfo[] publicConstructors =
                typeof(DefaultLayoutResilienceStructuralReadinessConsumer)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public);
            bool parameterlessOnly = publicConstructors.Length == 1 &&
                publicConstructors[0].GetParameters().Length == 0;
            Add(checks, "surface.public-constructors", "one parameterless",
                string.Join("|", publicConstructors.Select(value =>
                    value.GetParameters().Length.ToString(
                        CultureInfo.InvariantCulture))), parameterlessOnly);

            MemberInfo[] publicMembers =
                typeof(DefaultLayoutResilienceStructuralReadinessConsumer)
                    .GetMembers(BindingFlags.Instance | BindingFlags.Static |
                        BindingFlags.Public);
            bool noInjection = !publicMembers.OfType<MethodBase>()
                    .SelectMany(value => value.GetParameters())
                    .Any(value => IsEvaluatorOrValidator(value.ParameterType)) &&
                !publicMembers.OfType<PropertyInfo>().Any(value =>
                    IsEvaluatorOrValidator(value.PropertyType) || value.CanWrite);
            Add(checks, "surface.no-public-injection", "0", noInjection ? "0" : "1",
                noInjection);
            Add(checks, "surface.interface.consume", "1",
                typeof(ILayoutResilienceStructuralReadinessConsumer).GetMethods().Length
                    .ToString(CultureInfo.InvariantCulture),
                typeof(ILayoutResilienceStructuralReadinessConsumer).GetMethods()
                    .Length == 1);
        }

        private static bool IsEvaluatorOrValidator(Type type)
        {
            return type != null && (typeof(ILayoutResilienceStructuralPredicateEvaluator)
                .IsAssignableFrom(type) ||
                typeof(ILayoutResilienceStructuralPredicateValidator)
                    .IsAssignableFrom(type));
        }

        private static IReadOnlyList<Fixture> CreateFixtures(ICollection<Check> checks)
        {
            List<Fixture> fixtures = new List<Fixture>();
            LayoutResilienceEvaluationInput trueSingle = Applicable(
                "dev.consumer.01", false,
                new[] { LayoutResiliencePredicateClauseKind.CountedLayoutPresent });
            LayoutResilienceEvaluationInput falseSingle = Applicable(
                "dev.consumer.02", true,
                new[]
                {
                    LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable
                });
            LayoutResilienceEvaluationInput trueMulti = Applicable(
                "dev.consumer.03", false,
                new[]
                {
                    LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                    LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
                    LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable,
                    LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable
                });
            LayoutResilienceEvaluationInput falseMulti = Applicable(
                "dev.consumer.04", true,
                new[]
                {
                    LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable,
                    LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable,
                    LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
                    LayoutResiliencePredicateClauseKind.CountedLayoutPresent
                });

            Fixture case01 = Run("case01.complete.single.satisfied",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    trueSingle, null, FixedSignature('1'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                LayoutResiliencePredicateState.KnownTrue, 1, 1, 1, 1, null);
            fixtures.Add(case01);
            fixtures.Add(Run("case02.complete.single.violated",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    falseSingle, null, FixedSignature('2'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                LayoutResiliencePredicateState.KnownFalse, 1, 1, 1, 1, null));
            fixtures.Add(Run("case03.complete.multi.satisfied",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    trueMulti, null, FixedSignature('3'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                LayoutResiliencePredicateState.KnownTrue, 4, 1, 1, 1, null));
            fixtures.Add(Run("case04.complete.multi.mixed",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    falseMulti, null, FixedSignature('4'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                LayoutResiliencePredicateState.KnownFalse, 4, 1, 1, 1, null));

            LayoutResilienceEvaluationInput notApplicable =
                new LayoutResilienceEvaluationInput(
                    "dev.consumer.05",
                    EnemyRequirementApplicabilityState.NotApplicable,
                    LayoutResilienceInputCompleteness.NotRequired,
                    LayoutResilienceInputCompleteness.NotRequired,
                    null, null);
            fixtures.Add(Run("case05.complete.not-applicable",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    notApplicable, null, FixedSignature('5'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                LayoutResiliencePredicateState.NotApplicable, 0, 1, 1, 1, null));

            LayoutResilienceEvaluationInput unknownExplicit =
                new LayoutResilienceEvaluationInput(
                    "dev.consumer.06",
                    EnemyRequirementApplicabilityState.Unknown,
                    LayoutResilienceInputCompleteness.Complete,
                    LayoutResilienceInputCompleteness.Complete,
                    Build(false), Pressure(false,
                        new[]
                        {
                            LayoutResiliencePredicateClauseKind.CountedLayoutPresent
                        }));
            fixtures.Add(Run("case06.unknown.explicit.complete",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                    unknownExplicit, null, FixedSignature('6'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                LayoutResiliencePredicateState.Unknown, 0, 1, 1, 1, null));

            LayoutResilienceEvaluationInput buildIncomplete =
                new LayoutResilienceEvaluationInput(
                    "dev.consumer.07",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Incomplete,
                    LayoutResilienceInputCompleteness.Complete,
                    null, Pressure(false,
                        new[]
                        {
                            LayoutResiliencePredicateClauseKind.CountedLayoutPresent
                        }));
            fixtures.Add(Run("case07.unknown.build-incomplete",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                    buildIncomplete, UnknownIssues("build"), FixedSignature('7'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                LayoutResiliencePredicateState.Unknown, 0, 1, 1, 1, "P3.SOURCE_UNKNOWN"));

            LayoutResilienceEvaluationInput pressureIncomplete =
                new LayoutResilienceEvaluationInput(
                    "dev.consumer.08",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Complete,
                    LayoutResilienceInputCompleteness.Incomplete,
                    Build(false), null);
            fixtures.Add(Run("case08.unknown.pressure-incomplete",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                    pressureIncomplete, UnknownIssues("pressure"),
                    FixedSignature('8'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                LayoutResiliencePredicateState.Unknown, 0, 1, 1, 1, "P3.SOURCE_UNKNOWN"));

            fixtures.Add(Run("case09.unknown.null-input",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                    null, UnknownIssues("evaluationInput"), FixedSignature('9'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                null, 0, 0, 0, 1, "P3.SOURCE_UNKNOWN"));
            fixtures.Add(Run("case10.p3-null",
                Input(null),
                LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                null, 0, 0, 0, 0,
                LayoutResilienceStructuralReadinessConsumerValidationCodes
                    .AssemblerResultMissing));
            fixtures.Add(Run("case11.p3-invalid",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    null, InvalidIssues(), FixedSignature('a'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 0, 0, 1, "P3.SOURCE_INVALID"));

            Fixture case12 = Run("case12.p3-malformed",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    null, null, "sha256:BAD")),
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 0, 0, 1,
                LayoutResilienceStructuralReadinessConsumerValidationCodes
                    .AssemblerResultRejected);
            LayoutResilienceStructuralReadinessConsumerResult invalidNonNull =
                DefaultLayoutResilienceStructuralReadinessConsumer.Instance.Consume(
                    Input(RawP3(
                        LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                        trueSingle, InvalidIssues(), FixedSignature('b'))));
            case12.Passed = case12.Passed && invalidNonNull.Status ==
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid &&
                invalidNonNull.PredicateResult == null;
            fixtures.Add(case12);

            Fixture case13 = Run("case13.undefined-enums",
                Input(RawP3((LayoutResilienceEvaluationInputAssemblerStatus)77,
                    null, InvalidIssues(), FixedSignature('c'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 0, 0, 1,
                LayoutResilienceStructuralReadinessConsumerValidationCodes
                    .AssemblerResultRejected);
            LayoutResilienceStructuralReadinessConsumerResult rawUndefined =
                RawConsumer((LayoutResilienceStructuralReadinessConsumerStatus)77,
                    null, InvalidConsumerIssues(), FixedSignature('d'));
            bool consumerUndefinedRejected =
                DefaultLayoutResilienceStructuralReadinessConsumerValidator.Instance
                    .ValidateResult(rawUndefined).Any(value => value.Code ==
                        LayoutResilienceStructuralReadinessConsumerValidationCodes
                            .ResultStatusUndefined);
            CountProbe enumProbe = new CountProbe();
            LayoutResiliencePredicateResultSnapshot undefinedN01C =
                new LayoutResiliencePredicateResultSnapshot(
                    LayoutResilienceStructuralPredicateSchema.SchemaId, 1,
                    trueSingle.EvaluationId, trueSingle.ApplicabilityState,
                    (LayoutResiliencePredicateState)77,
                    Array.Empty<LayoutResiliencePredicateClauseSnapshot>(),
                    FixedSignature('e'));
            LayoutResilienceStructuralReadinessConsumerResult undefinedN01COutput =
                Harness(new FixedEvaluator(undefinedN01C), enumProbe).Consume(
                    Input(RawP3(
                        LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                        trueSingle, null, FixedSignature('f'))));
            case13.Passed = case13.Passed && consumerUndefinedRejected &&
                undefinedN01COutput.Status ==
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid &&
                enumProbe.Evaluate == 1 && enumProbe.PredicateValidate == 1;
            fixtures.Add(case13);

            Fixture case14 = Run("case14.consumer-null-schema",
                null,
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 0, 0, 0,
                LayoutResilienceStructuralReadinessConsumerValidationCodes.InputNull);
            LayoutResilienceStructuralReadinessConsumerResult badSchema =
                DefaultLayoutResilienceStructuralReadinessConsumer.Instance.Consume(
                    new LayoutResilienceStructuralReadinessConsumerInput(null,
                        "wrong", 99));
            case14.Passed = case14.Passed && badSchema.Status ==
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid &&
                badSchema.Issues.Any(value => value.Code ==
                    LayoutResilienceStructuralReadinessConsumerValidationCodes
                        .SchemaMismatch);
            fixtures.Add(case14);

            LayoutResilienceEvaluationInput malformedInput =
                new LayoutResilienceEvaluationInput(
                    "dev.consumer.15",
                    EnemyRequirementApplicabilityState.Applicable,
                    LayoutResilienceInputCompleteness.Complete,
                    LayoutResilienceInputCompleteness.Complete,
                    new LayoutResilienceBuildFactSnapshot(-1, Cell(0, 0),
                        new[] { Placement("dev.bad", Cell(0, 0)) }),
                    Pressure(false,
                        new[]
                        {
                            LayoutResiliencePredicateClauseKind.CountedLayoutPresent
                        }));
            fixtures.Add(Run("case15.n01c-input-rejection",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    malformedInput, null, FixedSignature('1'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 1, 0, 1,
                LayoutResilienceStructuralReadinessConsumerValidationCodes
                    .EvaluatorException));

            fixtures.Add(Run("case16.injected-throw",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    trueSingle, null, FixedSignature('2'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 1, 0, 1,
                LayoutResilienceStructuralReadinessConsumerValidationCodes
                    .EvaluatorException,
                new ThrowingEvaluator()));
            FixedEvaluator nullEvaluator = new FixedEvaluator(null);
            Fixture case17 = Run("case17.injected-null",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    trueSingle, null, FixedSignature('3'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 1, 1, 1,
                LayoutResilienceStructuralReadinessConsumerValidationCodes
                    .PredicateResultRejected,
                nullEvaluator);
            case17.Passed = case17.Passed && ReferenceEquals(
                nullEvaluator.LastInput,
                case17.Source.AssemblerResult.EvaluationInput);
            fixtures.Add(case17);

            LayoutResiliencePredicateResultSnapshot malformedResult =
                new LayoutResiliencePredicateResultSnapshot(
                    "wrong.schema", 9, "wrong.id",
                    EnemyRequirementApplicabilityState.Unknown,
                    LayoutResiliencePredicateState.KnownTrue,
                    new[]
                    {
                        new LayoutResiliencePredicateClauseSnapshot(
                            (LayoutResiliencePredicateClauseKind)99,
                            (LayoutResiliencePredicateClauseState)99)
                    },
                    "invalid-signature");
            FixedEvaluator malformedEvaluator = new FixedEvaluator(malformedResult);
            Fixture case18 = Run("case18.injected-malformed",
                Input(RawP3(LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                    trueSingle, null, FixedSignature('4'))),
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                null, 0, 1, 1, 1,
                LayoutResilienceStructuralReadinessConsumerValidationCodes
                    .PredicateResultRejected,
                malformedEvaluator);
            case18.Passed = case18.Passed && ReferenceEquals(
                malformedEvaluator.LastInput,
                case18.Source.AssemblerResult.EvaluationInput);
            fixtures.Add(case18);

            fixtures.Add(VerifyDeterminism(trueSingle));
            fixtures.Add(VerifyImmutabilityAndSensitivity(checks));

            Add(checks, "state.known-true", "4",
                fixtures.Count(value => value.Result != null &&
                    value.Result.PredicateResult != null &&
                    value.Result.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownTrue).ToString(
                            CultureInfo.InvariantCulture),
                fixtures.Count(value => value.Result != null &&
                    value.Result.PredicateResult != null &&
                    value.Result.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownTrue) == 4);
            Add(checks, "state.known-false", "2",
                fixtures.Count(value => value.Result != null &&
                    value.Result.PredicateResult != null &&
                    value.Result.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownFalse).ToString(
                            CultureInfo.InvariantCulture),
                fixtures.Count(value => value.Result != null &&
                    value.Result.PredicateResult != null &&
                    value.Result.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownFalse) == 2);
            Add(checks, "state.not-applicable", "1",
                fixtures.Count(value => value.Result != null &&
                    value.Result.PredicateResult != null &&
                    value.Result.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.NotApplicable).ToString(
                            CultureInfo.InvariantCulture),
                fixtures.Count(value => value.Result != null &&
                    value.Result.PredicateResult != null &&
                    value.Result.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.NotApplicable) == 1);
            Add(checks, "state.unknown", "5",
                fixtures.Count(value => value.Result != null &&
                    value.Result.Status ==
                        LayoutResilienceStructuralReadinessConsumerStatus.Unknown)
                    .ToString(CultureInfo.InvariantCulture),
                fixtures.Count(value => value.Result != null &&
                    value.Result.Status ==
                        LayoutResilienceStructuralReadinessConsumerStatus.Unknown) == 5);
            Add(checks, "state.invalid", "8",
                fixtures.Count(value => value.Result != null &&
                    value.Result.Status ==
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid)
                    .ToString(CultureInfo.InvariantCulture),
                fixtures.Count(value => value.Result != null &&
                    value.Result.Status ==
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid) == 8);
            return fixtures.AsReadOnly();
        }

        private static Fixture Run(
            string id,
            LayoutResilienceStructuralReadinessConsumerInput source,
            LayoutResilienceStructuralReadinessConsumerStatus expectedStatus,
            LayoutResiliencePredicateState? expectedState,
            int expectedClauses,
            int expectedEvaluate,
            int expectedPredicateValidate,
            int expectedAssemblerValidate,
            string expectedCode,
            ILayoutResilienceStructuralPredicateEvaluator syntheticEvaluator = null)
        {
            CountProbe probe = new CountProbe();
            DefaultLayoutResilienceStructuralReadinessConsumer consumer =
                Harness(syntheticEvaluator, probe);
            LayoutResilienceStructuralReadinessConsumerResult result =
                consumer.Consume(source);
            IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>
                resultIssues =
                    DefaultLayoutResilienceStructuralReadinessConsumerValidator.Instance
                        .ValidateResult(result);
            bool passed = result != null && result.Status == expectedStatus &&
                ((expectedState.HasValue && result.PredicateResult != null &&
                    result.PredicateResult.PredicateState == expectedState.Value &&
                    result.PredicateResult.ClauseRows.Count == expectedClauses) ||
                 (!expectedState.HasValue && result.PredicateResult == null)) &&
                probe.Evaluate == expectedEvaluate &&
                probe.PredicateValidate == expectedPredicateValidate &&
                probe.AssemblerValidate == expectedAssemblerValidate &&
                resultIssues.Count == 0 && IsSignature(result.CanonicalSignature) &&
                (string.IsNullOrEmpty(expectedCode) || result.Issues.Any(value =>
                    value.Code == expectedCode));
            string detail = "status=" + result.Status + ",state=" +
                (result.PredicateResult == null
                    ? "null"
                    : result.PredicateResult.PredicateState.ToString()) +
                ",clauses=" + (result.PredicateResult == null ? 0 :
                    result.PredicateResult.ClauseRows.Count).ToString(
                        CultureInfo.InvariantCulture) +
                ",evaluate=" + probe.Evaluate.ToString(CultureInfo.InvariantCulture) +
                ",validate=" + probe.PredicateValidate.ToString(
                    CultureInfo.InvariantCulture) +
                ",p3validate=" + probe.AssemblerValidate.ToString(
                    CultureInfo.InvariantCulture);
            return new Fixture(id, source, result, passed, detail);
        }

        private static Fixture VerifyDeterminism(
            LayoutResilienceEvaluationInput unused)
        {
            LayoutResilienceEvaluationInput forward = DeterministicInput(false);
            LayoutResilienceEvaluationInput reverse = DeterministicInput(true);
            LayoutResilienceStructuralReadinessConsumerInput left = Input(RawP3(
                LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                forward, null, FixedSignature('9')));
            LayoutResilienceStructuralReadinessConsumerInput right = Input(RawP3(
                LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                reverse, null, FixedSignature('9')));
            CountProbe probe = new CountProbe();
            DefaultLayoutResilienceStructuralReadinessConsumer consumer =
                Harness(null, probe);
            CultureInfo original = CultureInfo.CurrentCulture;
            LayoutResilienceStructuralReadinessConsumerResult first;
            LayoutResilienceStructuralReadinessConsumerResult second;
            LayoutResilienceStructuralReadinessConsumerResult repeat;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                first = consumer.Consume(left);
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                second = consumer.Consume(right);
                repeat = consumer.Consume(left);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
            bool passed = first.CanonicalSignature == second.CanonicalSignature &&
                first.CanonicalSignature == repeat.CanonicalSignature &&
                first.PredicateResult.CanonicalSignature ==
                    second.PredicateResult.CanonicalSignature &&
                probe.Evaluate == 3 && probe.PredicateValidate == 3 &&
                probe.AssemblerValidate == 3;
            return new Fixture("case19.order-culture-repeat", left, first, passed,
                "equal=" + passed.ToString());
        }

        private static Fixture VerifyImmutabilityAndSensitivity(
            ICollection<Check> checks)
        {
            List<LayoutCellCoordinate> shape = new List<LayoutCellCoordinate>
            {
                Cell(0, 0)
            };
            List<LayoutResiliencePlacedItemFactSnapshot> placements =
                new List<LayoutResiliencePlacedItemFactSnapshot>
                {
                    new LayoutResiliencePlacedItemFactSnapshot(
                        "dev.placement.mutable", "dev.item.mutable", Cell(0, 0), 0,
                        shape, shape, Cell(0, 0), true, true)
                };
            LayoutResilienceBuildFactSnapshot build =
                new LayoutResilienceBuildFactSnapshot(3, Cell(1, 1), placements);
            List<LayoutCellCoordinate> domain = Domain(3).ToList();
            List<LayoutCellCoordinate> usable = Domain(3).ToList();
            List<LayoutResiliencePredicateClauseKind> clauses =
                new List<LayoutResiliencePredicateClauseKind>
                {
                    LayoutResiliencePredicateClauseKind.CountedLayoutPresent
                };
            LayoutPressureSnapshot pressure = new LayoutPressureSnapshot(
                "dev.pressure.mutable",
                new[] { LayoutPressureKind.PollutedCellMask },
                domain, usable, Cell(1, 1),
                Array.Empty<LayoutCellConnection>(), clauses);
            LayoutResilienceEvaluationInput input = new LayoutResilienceEvaluationInput(
                "dev.consumer.20", EnemyRequirementApplicabilityState.Applicable,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Complete, build, pressure);
            LayoutResilienceStructuralReadinessConsumerInput source = Input(RawP3(
                LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                input, null, FixedSignature('a')));
            LayoutResilienceStructuralReadinessConsumerResult result =
                DefaultLayoutResilienceStructuralReadinessConsumer.Instance.Consume(source);
            string signature = result.CanonicalSignature;
            shape.Add(Cell(2, 2));
            placements.Clear();
            domain.Clear();
            usable.Clear();
            clauses.Clear();

            bool readOnly = ThrowsNotSupported(() =>
                ((IList)result.PredicateResult.ClauseRows).Add(
                    new LayoutResiliencePredicateClauseSnapshot(
                        LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseState.Violated))) &&
                ThrowsNotSupported(() => ((IList)result.Issues).Add(
                    new LayoutResilienceStructuralReadinessConsumerIssue(
                        "x", LayoutResilienceStructuralReadinessConsumerStatus.Unknown,
                        "x", "x")));
            bool unchanged = result.CanonicalSignature == signature &&
                result.PredicateResult.ClauseRows.Count == 1 &&
                result.PredicateResult.PredicateState ==
                    LayoutResiliencePredicateState.KnownTrue;

            LayoutResilienceStructuralReadinessConsumerResult changedP3Signature =
                DefaultLayoutResilienceStructuralReadinessConsumer.Instance.Consume(
                    Input(RawP3(
                        LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                        input, null, FixedSignature('b'))));
            LayoutResilienceStructuralReadinessConsumerResult changedInput =
                DefaultLayoutResilienceStructuralReadinessConsumer.Instance.Consume(
                    Input(RawP3(
                        LayoutResilienceEvaluationInputAssemblerStatus.Complete,
                        Applicable("dev.consumer.20.changed", false,
                            new[]
                            {
                                LayoutResiliencePredicateClauseKind
                                    .CountedLayoutPresent
                            }),
                        null, FixedSignature('a'))));
            bool sensitive = signature != changedP3Signature.CanonicalSignature &&
                signature != changedInput.CanonicalSignature;

            string canonicalTypeName =
                "TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness." +
                "LayoutResilienceStructuralReadinessConsumerCanonical";
            Type canonicalType = typeof(DefaultLayoutResilienceStructuralReadinessConsumer)
                .Assembly.GetType(canonicalTypeName, true);
            MethodInfo create = canonicalType.GetMethod("Create",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            LayoutResiliencePredicateResultSnapshot original = result.PredicateResult;
            string stateChanged = (string)create.Invoke(null, new object[]
            {
                source,
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                new LayoutResiliencePredicateResultSnapshot(
                    original.SchemaId, original.SchemaVersion, original.EvaluationId,
                    original.ApplicabilityState,
                    LayoutResiliencePredicateState.KnownFalse,
                    new[]
                    {
                        new LayoutResiliencePredicateClauseSnapshot(
                            LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                            LayoutResiliencePredicateClauseState.Violated)
                    },
                    original.CanonicalSignature),
                result.Issues
            });
            string clauseChanged = (string)create.Invoke(null, new object[]
            {
                source,
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                new LayoutResiliencePredicateResultSnapshot(
                    original.SchemaId, original.SchemaVersion, original.EvaluationId,
                    original.ApplicabilityState, original.PredicateState,
                    new[]
                    {
                        new LayoutResiliencePredicateClauseSnapshot(
                            LayoutResiliencePredicateClauseKind
                                .CountedPlacementCellsUsable,
                            LayoutResiliencePredicateClauseState.Satisfied)
                    },
                    original.CanonicalSignature),
                result.Issues
            });
            string issueChanged = (string)create.Invoke(null, new object[]
            {
                source,
                LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                original,
                new[]
                {
                    new LayoutResilienceStructuralReadinessConsumerIssue(
                        "dev.issue", LayoutResilienceStructuralReadinessConsumerStatus
                            .Unknown, "dev", "changed")
                }
            });
            bool fieldSensitive = signature != stateChanged &&
                signature != clauseChanged && signature != issueChanged;
            Add(checks, "immutability.read-only", "True", readOnly.ToString(), readOnly);
            Add(checks, "canonical.field-sensitive", "True",
                fieldSensitive.ToString(), fieldSensitive);
            bool passed = readOnly && unchanged && sensitive && fieldSensitive;
            return new Fixture("case20.immutability-sensitivity", source, result,
                passed, "readonly=" + readOnly + ",unchanged=" + unchanged +
                    ",sensitive=" + sensitive + ",fields=" + fieldSensitive);
        }

        private static DefaultLayoutResilienceStructuralReadinessConsumer Harness(
            ILayoutResilienceStructuralPredicateEvaluator evaluator,
            CountProbe probe)
        {
            return (DefaultLayoutResilienceStructuralReadinessConsumer)
                Activator.CreateInstance(
                    typeof(DefaultLayoutResilienceStructuralReadinessConsumer),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[]
                    {
                        evaluator,
                        (Action)(() => probe.Evaluate++),
                        (Action)(() => probe.PredicateValidate++),
                        (Action)(() => probe.AssemblerValidate++)
                    },
                    CultureInfo.InvariantCulture);
        }

        private static LayoutResilienceStructuralReadinessConsumerInput Input(
            LayoutResilienceEvaluationInputAssemblerResult value)
        {
            return new LayoutResilienceStructuralReadinessConsumerInput(value);
        }

        private static LayoutResilienceEvaluationInputAssemblerResult RawP3(
            LayoutResilienceEvaluationInputAssemblerStatus status,
            LayoutResilienceEvaluationInput evaluationInput,
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> issues,
            string signature)
        {
            return (LayoutResilienceEvaluationInputAssemblerResult)
                Activator.CreateInstance(
                    typeof(LayoutResilienceEvaluationInputAssemblerResult),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { status, evaluationInput, issues, signature },
                    CultureInfo.InvariantCulture);
        }

        private static LayoutResilienceStructuralReadinessConsumerResult RawConsumer(
            LayoutResilienceStructuralReadinessConsumerStatus status,
            LayoutResiliencePredicateResultSnapshot predicateResult,
            IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> issues,
            string signature)
        {
            return (LayoutResilienceStructuralReadinessConsumerResult)
                Activator.CreateInstance(
                    typeof(LayoutResilienceStructuralReadinessConsumerResult),
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new object[] { status, predicateResult, issues, signature },
                    CultureInfo.InvariantCulture);
        }

        private static LayoutResilienceEvaluationInput Applicable(
            string id,
            bool violatePlacement,
            IEnumerable<LayoutResiliencePredicateClauseKind> clauses,
            bool reverse = false)
        {
            return new LayoutResilienceEvaluationInput(
                id,
                EnemyRequirementApplicabilityState.Applicable,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Complete,
                Build(reverse),
                Pressure(violatePlacement, clauses, reverse));
        }

        private static LayoutResilienceEvaluationInput DeterministicInput(bool reverse)
        {
            LayoutCellCoordinate[] domain = Domain(3).ToArray();
            LayoutCellCoordinate[] usable = Domain(3).ToArray();
            LayoutCellConnection[] connections =
            {
                new LayoutCellConnection(Cell(0, 0), Cell(1, 0)),
                new LayoutCellConnection(Cell(1, 0), Cell(2, 0))
            };
            LayoutResiliencePredicateClauseKind[] clauses =
            {
                LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
                LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable
            };
            if (reverse)
            {
                Array.Reverse(domain);
                Array.Reverse(usable);
                Array.Reverse(connections);
                Array.Reverse(clauses);
            }
            LayoutPressureSnapshot pressure = new LayoutPressureSnapshot(
                "dev.pressure.deterministic",
                new[] { LayoutPressureKind.PollutedCellMask },
                domain, usable, Cell(1, 1), connections, clauses);
            return new LayoutResilienceEvaluationInput(
                "dev.consumer.19",
                EnemyRequirementApplicabilityState.Applicable,
                LayoutResilienceInputCompleteness.Complete,
                LayoutResilienceInputCompleteness.Complete,
                Build(reverse), pressure);
        }

        private static LayoutResilienceBuildFactSnapshot Build(bool reverse)
        {
            LayoutResiliencePlacedItemFactSnapshot[] rows =
            {
                Placement("dev.placement.b", Cell(2, 2)),
                Placement("dev.placement.a", Cell(0, 0))
            };
            if (reverse)
            {
                Array.Reverse(rows);
            }
            return new LayoutResilienceBuildFactSnapshot(3, Cell(1, 1), rows);
        }

        private static LayoutResiliencePlacedItemFactSnapshot Placement(
            string id,
            LayoutCellCoordinate cell)
        {
            return new LayoutResiliencePlacedItemFactSnapshot(
                id, "dev.item." + id, cell, 0,
                new[] { cell }, new[] { cell }, cell, true, true);
        }

        private static LayoutPressureSnapshot Pressure(
            bool violatePlacement,
            IEnumerable<LayoutResiliencePredicateClauseKind> clauses,
            bool reverse = false)
        {
            LayoutCellCoordinate[] domain = Domain(3).ToArray();
            LayoutCellCoordinate[] usable = domain.Where(value =>
                !violatePlacement || !(value.X == 0 && value.Y == 0)).ToArray();
            LayoutResiliencePredicateClauseKind[] clauseRows =
                (clauses ?? Array.Empty<LayoutResiliencePredicateClauseKind>())
                .ToArray();
            if (reverse)
            {
                Array.Reverse(domain);
                Array.Reverse(usable);
                Array.Reverse(clauseRows);
            }
            return new LayoutPressureSnapshot(
                "dev.pressure.3",
                new[] { LayoutPressureKind.PollutedCellMask },
                domain,
                usable,
                Cell(1, 1),
                Array.Empty<LayoutCellConnection>(),
                clauseRows);
        }

        private static IEnumerable<LayoutCellCoordinate> Domain(int boardSize)
        {
            for (int y = 0; y < boardSize; y++)
            {
                for (int x = 0; x < boardSize; x++)
                {
                    yield return Cell(x, y);
                }
            }
        }

        private static LayoutCellCoordinate Cell(int x, int y)
        {
            return new LayoutCellCoordinate(x, y);
        }

        private static IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue>
            UnknownIssues(string path)
        {
            return new[]
            {
                new LayoutResilienceEvaluationInputAssemblerIssue(
                    "SOURCE_UNKNOWN",
                    LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                    path,
                    "Synthetic P3 source fact is incomplete.")
            };
        }

        private static IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue>
            InvalidIssues()
        {
            return new[]
            {
                new LayoutResilienceEvaluationInputAssemblerIssue(
                    "SOURCE_INVALID",
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    "source",
                    "Synthetic P3 source fact is invalid.")
            };
        }

        private static IEnumerable<
            LayoutResilienceStructuralReadinessConsumerIssue> InvalidConsumerIssues()
        {
            return new[]
            {
                new LayoutResilienceStructuralReadinessConsumerIssue(
                    "SYNTHETIC_INVALID",
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                    "source",
                    "Synthetic consumer result is invalid.")
            };
        }

        private static string FixedSignature(char value)
        {
            return "sha256:" + new string(value, 64);
        }

        private static bool ThrowsNotSupported(Action action)
        {
            try
            {
                action();
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static void VerifySourceBoundaries(
            ICollection<Check> checks,
            string root)
        {
            string runtime = string.Join("\n", OutputPaths
                .Where(value => value.StartsWith(RuntimeDirectory,
                    StringComparison.Ordinal) && value.EndsWith(".cs",
                    StringComparison.Ordinal))
                .Select(value => Read(root, value)));
            string consumer = Read(root, RuntimeDirectory +
                "/LayoutResilienceStructuralReadinessConsumer.cs");
            Add(checks, "authority.evaluate-call-sites", "1",
                Count(consumer, "evaluator.Evaluate(").ToString(
                    CultureInfo.InvariantCulture),
                Count(consumer, "evaluator.Evaluate(") == 1);
            Add(checks, "authority.n01c-result-validator-call-sites", "1",
                Count(consumer,
                    "DefaultLayoutResilienceStructuralPredicateValidator.Instance")
                    .ToString(CultureInfo.InvariantCulture),
                Count(consumer,
                    "DefaultLayoutResilienceStructuralPredicateValidator.Instance") == 1);
            Add(checks, "authority.p3-result-validator-call-sites", "1",
                Count(consumer,
                    "DefaultLayoutResilienceEvaluationInputAssemblerValidator.Instance")
                    .ToString(CultureInfo.InvariantCulture),
                Count(consumer,
                    "DefaultLayoutResilienceEvaluationInputAssemblerValidator.Instance")
                    == 1);
            string[] forbidden =
            {
                ".Assemble(",
                "DefaultLayoutResilienceEvaluationInputAssembler.Instance",
                "LayoutResilienceItemFactProjection",
                "AuthoredLayoutPressureSource",
                "EnemyRequirementChannelApplicability",
                "BuildCapabilityReadContract",
                "BuildCapability",
                "basisPoint",
                "MapRule",
                "Encounter",
                "requirementId",
                "IsReady",
                "ReadinessBand",
                "UnityEngine",
                "MonoBehaviour"
            };
            int leaks = forbidden.Count(token => runtime.IndexOf(token,
                StringComparison.Ordinal) >= 0);
            Add(checks, "leak.runtime", "0", leaks.ToString(
                CultureInfo.InvariantCulture), leaks == 0);
            Add(checks, "api.no-evaluator-setter", "0",
                Count(runtime, "set; }").ToString(CultureInfo.InvariantCulture),
                Count(runtime, "set; }") == 0);
        }

        private static void VerifyReports(
            ICollection<Check> checks,
            string root)
        {
            Add(checks, "outputs.count", "14", OutputPaths.Length.ToString(
                CultureInfo.InvariantCulture), OutputPaths.Length == 14);
            Add(checks, "outputs.exist", "14",
                OutputPaths.Count(value => File.Exists(Absolute(root, value))).ToString(
                    CultureInfo.InvariantCulture),
                OutputPaths.All(value => File.Exists(Absolute(root, value))));
            string main = Read(root, ReportDirectory +
                "/LayoutResilienceStructuralReadinessConsumerReport.md");
            string fixture = Read(root, ReportDirectory +
                "/LayoutResilienceStructuralReadinessConsumerFixtureCases.csv");
            string leak = Read(root, ReportDirectory +
                "/LayoutResilienceStructuralReadinessConsumerLeakCheckReport.md");
            string[] required =
            {
                "20/20", ExpectedSignature, "Evaluate = 1", "ValidateResult = 1",
                "C02 blocked rows: `32/32`", "Actual Unknown reduction: `0`",
                "Leak Count: `0`", "existing files modified: `0`"
            };
            foreach (string token in required)
            {
                Add(checks, "report.main." + token, "present", token,
                    main.IndexOf(token, StringComparison.Ordinal) >= 0);
            }
            Add(checks, "report.fixtures.rows", "20",
                Math.Max(0, fixture.Split(new[] { '\n' },
                    StringSplitOptions.RemoveEmptyEntries).Length - 1).ToString(
                        CultureInfo.InvariantCulture),
                fixture.Split(new[] { '\n' },
                    StringSplitOptions.RemoveEmptyEntries).Length - 1 == 20);
            Add(checks, "report.leak-count", "Leak Count: `0`", leak,
                leak.IndexOf("Leak Count: `0`", StringComparison.Ordinal) >= 0);
            Add(checks, "report.c02", "32/32 and 0", main,
                main.IndexOf("C02 blocked rows: `32/32`", StringComparison.Ordinal) >= 0 &&
                main.IndexOf("Actual Unknown reduction: `0`",
                    StringComparison.Ordinal) >= 0);
        }

        private static void VerifyProtected(
            ICollection<Check> checks,
            string root)
        {
            AddProtection(checks, root, "p3", P3Files, true, 14,
                "c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c");
            AddProtection(checks, root, "p2", P2Files, true, 14,
                "15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050");
            AddProtection(checks, root, "p1", P1Files, true, 14,
                "7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48");
            AddProtection(checks, root, "n01c", N01CFiles, true, 16,
                "691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b");
            AddProtection(checks, root, "n01b", N01BFiles, true, 14,
                "acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316");
            AddProtection(checks, root, "if01", IF01Files, true, 11,
                "2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8");
            AddProtection(checks, root, "c01", C01Files, false, 9,
                "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a");
            AddProtection(checks, root, "c02", C02Files, false, 9,
                "08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4");
            AddProtection(checks, root, "c02a", C02AFiles, false, 7,
                "a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb");
            AddProtection(checks, root, "n01", N01Files, true, 8,
                "03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52");
            AddProtection(checks, root, "c02a-d1", C02AD1Files, true, 7,
                "c0e789c386206749aad7b2937b0e9939e86b83831f025fee1aaf56512ff2eca0");
            AddProtection(checks, root, "n01a-survey", N01AFiles, true, 8,
                "705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de");
            AggregateHash item = AggregateItem(root);
            Add(checks, "protected.item.count", "105",
                item.FileCount.ToString(CultureInfo.InvariantCulture),
                item.FileCount == 105);
            Add(checks, "protected.item.hash",
                "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1",
                item.Hash, item.Hash ==
                    "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1");
            AggregateHash enemy = AggregateEnemy(root);
            Add(checks, "protected.enemy.count", "89",
                enemy.FileCount.ToString(CultureInfo.InvariantCulture),
                enemy.FileCount == 89);
            Add(checks, "protected.enemy.hash",
                "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae",
                enemy.Hash, enemy.Hash ==
                    "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae");
            AddProtection(checks, root, "scenes",
                DirectoryFiles(root, "Assets/_Game/Scenes"), true, 14,
                "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b");
            AddProtection(checks, root, "prefabs",
                DirectoryFiles(root, "Assets/_Game/Prefabs"), true, 16,
                "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087");
            string buildSettings = FileHash(root,
                "ProjectSettings/EditorBuildSettings.asset", false);
            Add(checks, "protected.build-settings",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                buildSettings, buildSettings ==
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59");

            Add(checks, "protected.p3-signature",
                "sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a",
                Read(root, "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerReport.md"),
                Read(root, "Docs/V0.4/Reports/LayoutResilienceEvaluationInputAssemblerReport.md")
                    .IndexOf("sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a",
                        StringComparison.Ordinal) >= 0);
            string head = RunGit(root, "rev-parse HEAD").Trim();
            Add(checks, "protected.head",
                "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba", head,
                head == "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba");
        }

        private static void AddProtection(
            ICollection<Check> checks,
            string root,
            string id,
            IEnumerable<string> paths,
            bool trailing,
            int expectedCount,
            string expectedHash)
        {
            AggregateHash actual = AggregateFiles(root, paths, trailing);
            Add(checks, "protected." + id + ".count",
                expectedCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount == expectedCount);
            Add(checks, "protected." + id + ".hash", expectedHash, actual.Hash,
                actual.Hash == expectedHash);
        }

        private static AggregateHash AggregateItem(string root)
        {
            string directory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            return AggregateFiles(root,
                Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                    .Where(value => !value.StartsWith(excluded,
                            StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(value, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                    .Select(value => Relative(root, value)),
                false);
        }

        private static AggregateHash AggregateEnemy(string root)
        {
            string directory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta");
            return AggregateFiles(root,
                Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                    .Where(value => !value.StartsWith(excluded,
                            StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(value, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                    .Select(value => Relative(root, value)),
                false);
        }

        private static IEnumerable<string> DirectoryFiles(
            string root,
            string directory)
        {
            return Directory.GetFiles(Absolute(root, directory), "*",
                    SearchOption.AllDirectories)
                .Select(value => Relative(root, value));
        }

        private static AggregateHash AggregateFiles(
            string root,
            IEnumerable<string> paths,
            bool trailing)
        {
            string[] ordered = paths.OrderBy(value => value,
                trailing ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                .ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                payload.Append(ordered[index].Replace('\\', '/')).Append('|')
                    .Append(FileHash(root, ordered[index], !trailing));
                if (trailing || index + 1 < ordered.Length)
                {
                    payload.Append('\n');
                }
            }
            return new AggregateHash(ordered.Length,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static string FileHash(string root, string path, bool uppercase)
        {
            return Sha256(File.ReadAllBytes(Absolute(root, path)), uppercase);
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(bytes ?? Array.Empty<byte>())
                    .Select(value => value.ToString(uppercase ? "X2" : "x2",
                        CultureInfo.InvariantCulture)));
            }
        }

        private static void AssertEnum<T>(
            ICollection<Check> checks,
            string id,
            string[] names,
            int[] values)
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
                Enum.IsDefined(typeof(T), 0).ToString(),
                !Enum.IsDefined(typeof(T), 0));
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            IEnumerable<string> expected)
        {
            string[] wanted = expected.OrderBy(value => value,
                StringComparer.Ordinal).ToArray();
            string[] actual = type.GetProperties(BindingFlags.Instance |
                    BindingFlags.Public)
                .Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(checks, "surface." + id, string.Join("|", wanted),
                string.Join("|", actual), wanted.SequenceEqual(actual));
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Skip(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static int Count(string value, string token)
        {
            int count = 0;
            int index = 0;
            while (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(token) &&
                (index = value.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static string FindRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, "Assets")) &&
                    Directory.Exists(Path.Combine(directory.FullName, "Docs")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root,
                relative.Replace('/', Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string absolute)
        {
            return absolute.Substring(root.TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar).Length + 1)
                .Replace('\\', '/');
        }

        private static string Read(string root, string relative)
        {
            return File.ReadAllText(Absolute(root, relative), Encoding.UTF8);
        }

        private static string RunGit(string root, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo("git", arguments)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(info))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        "git " + arguments + " failed: " + error);
                }
                return output;
            }
        }

        private static void Add(
            ICollection<Check> checks,
            string id,
            string expected,
            string actual,
            bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private sealed class ThrowingEvaluator :
            ILayoutResilienceStructuralPredicateEvaluator
        {
            public LayoutResiliencePredicateResultSnapshot Evaluate(
                LayoutResilienceEvaluationInput input)
            {
                throw new InvalidOperationException(
                    "Synthetic localized exception text must not leak.");
            }
        }

        private sealed class FixedEvaluator :
            ILayoutResilienceStructuralPredicateEvaluator
        {
            private readonly LayoutResiliencePredicateResultSnapshot result;

            public FixedEvaluator(LayoutResiliencePredicateResultSnapshot result)
            {
                this.result = result;
            }

            public LayoutResilienceEvaluationInput LastInput { get; private set; }

            public LayoutResiliencePredicateResultSnapshot Evaluate(
                LayoutResilienceEvaluationInput input)
            {
                LastInput = input;
                return result;
            }
        }

        private sealed class CountProbe
        {
            public int Evaluate;
            public int PredicateValidate;
            public int AssemblerValidate;
        }

        private sealed class Fixture
        {
            public Fixture(
                string id,
                LayoutResilienceStructuralReadinessConsumerInput source,
                LayoutResilienceStructuralReadinessConsumerResult result,
                bool passed,
                string detail)
            {
                Id = id;
                Source = source;
                Result = result;
                Passed = passed;
                Detail = detail;
            }

            public string Id { get; }
            public LayoutResilienceStructuralReadinessConsumerInput Source { get; }
            public LayoutResilienceStructuralReadinessConsumerResult Result { get; }
            public bool Passed { get; set; }
            public string Detail { get; }
        }

        private sealed class Check
        {
            public Check(string id, string expected, string actual, bool passed)
            {
                Id = id;
                Expected = expected;
                Actual = actual;
                Passed = passed;
            }

            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private sealed class Summary
        {
            public Summary(int failed, string message)
            {
                Failed = failed;
                Message = message;
            }

            public int Failed { get; }
            public string Message { get; }
        }

        private struct AggregateHash
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; }
            public string Hash { get; }
        }
    }
}
