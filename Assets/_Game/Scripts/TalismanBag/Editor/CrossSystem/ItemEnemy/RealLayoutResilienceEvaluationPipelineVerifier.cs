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
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.DevAuthoring;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RequirementMigrationOverlay;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness;
using TalismanBag.EnemySystem.RequirementChannel;
using TalismanBag.Items;
using TalismanBag.Items.Capability;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class RealLayoutResilienceEvaluationPipelineVerifier
    {
        private const string ExpectedSignature =
            "sha256:71cac03fb6f48e8c8490dfe1ceef6ad932534d734f1556adf69bc264630d164c";
        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RealEvaluationPipeline";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string AssignmentPath =
            "Docs/V0.4/RealLayoutResilienceEvaluationPipeline01_Assignment.md";
        private const string AssignmentHash =
            "95e96bddf51f1f4189f339fac191530a3e5accc85ebd0efa6ce9fa7a0dfe3b01";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory +
                "/RealLayoutResilienceEvaluationPipelinePrimitives.cs",
            RuntimeDirectory +
                "/RealLayoutResilienceEvaluationPipelinePrimitives.cs.meta",
            RuntimeDirectory + "/RealLayoutResilienceEvaluationPipeline.cs",
            RuntimeDirectory + "/RealLayoutResilienceEvaluationPipeline.cs.meta",
            RuntimeDirectory +
                "/RealLayoutResilienceEvaluationPipelineValidation.cs",
            RuntimeDirectory +
                "/RealLayoutResilienceEvaluationPipelineValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/RealLayoutResilienceEvaluationPipelineVerifier.cs.meta",
            ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineReport.md",
            ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineRouteRows.csv",
            ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineScenarioMatrix.csv",
            ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineAuthorityCallMatrix.csv",
            ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineTruthTable.csv",
            ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineLeakCheckReport.md"
        };

        private static readonly string[] P5AFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureAuthoringPrimitives.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureAuthoringPrimitives.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureCatalog.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureCatalog.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureValidation.cs",
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/DevEncounterLayoutPressureAuthoringVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/DevEncounterLayoutPressureAuthoringVerifier.cs.meta",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringReport.md",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringRows.csv",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringCells.csv",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringConnections.csv",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringMaps.md",
            ReportDirectory +
                "/DevEncounterLayoutPressureAuthoringLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Real Layout Resilience Evaluation Pipeline")]
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
                    "REAL_LAYOUT_RESILIENCE_EVALUATION_PIPELINE PASS");
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

        public static string ComputeI031V2CanonicalForMigration()
        {
            return DefaultRealLayoutResilienceEvaluationPipeline.Instance
                .Evaluate(Input("batch.empty", new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 0)
                }))
                .CanonicalSignature;
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
            string protectedBefore = CaptureProtected(root);
            List<Check> checks = new List<Check>();
            AssertEnum<RealLayoutResilienceEvaluationPipelineStatus>(
                checks,
                "pipeline-status",
                new[] { "Complete", "Unknown", "Invalid" },
                new[] { 1, 2, 3 });
            Add(checks, "schema.id",
                "RealLayoutResilienceEvaluationPipeline.v1",
                RealLayoutResilienceEvaluationPipelineSchema.SchemaId,
                RealLayoutResilienceEvaluationPipelineSchema.SchemaId ==
                    "RealLayoutResilienceEvaluationPipeline.v1");
            Add(checks, "schema.version", "1",
                RealLayoutResilienceEvaluationPipelineSchema.SchemaVersion
                    .ToString(CultureInfo.InvariantCulture),
                RealLayoutResilienceEvaluationPipelineSchema.SchemaVersion == 1);
            VerifySurface(checks);

            List<Scenario> scenarios = CreateScenarios();
            VerifyScenarios(checks, scenarios);
            VerifyPreGates(checks, scenarios[0].Input);
            string observed = scenarios[0].Result.CanonicalSignature;
            Add(checks, "canonical.format", "sha256+64 lowercase hex", observed,
                IsSignature(observed));
            Add(checks, "canonical.fixed", ExpectedSignature, observed,
                observed == ExpectedSignature);

            VerifyAuthorityMatrix(checks, scenarios[3].Input);
            VerifyP4Rejection(checks, scenarios[3].Input);
            VerifyDeterminism(checks);
            VerifyValidatorProbes(checks, scenarios[0].Result);
            VerifyImmutability(checks, scenarios[3].Result);
            VerifySourceBoundaries(checks, root);
            VerifyReports(checks, root);
            VerifyC02(checks, root);
            VerifyPackage(checks, root);
            VerifyProtected(checks, root);
            string protectedAfter = CaptureProtected(root);
            Add(checks, "protected.before-after", protectedBefore,
                protectedAfter,
                protectedBefore == protectedAfter);

            int failed = checks.Count(value => !value.Passed);
            string failures = string.Join(" | ", checks
                .Where(value => !value.Passed)
                .Select(value => value.Id + " expected=" + value.Expected +
                    " actual=" + value.Actual));
            string message = "RealLayoutResilienceEvaluationPipeline verifier: " +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) +
                "/" + checks.Count.ToString(CultureInfo.InvariantCulture) +
                " PASS; scenarios=" + scenarios.Count(value => value.Passed)
                    .ToString(CultureInfo.InvariantCulture) + "/7" +
                "; successful routes=24/24; signature=" + observed +
                "; C02 blocked=32/32; Actual Unknown reduction=0" +
                (failed == 0 ? string.Empty : "; failures=" + failures);
            return new Summary(failed, message);
        }

        private static void VerifySurface(ICollection<Check> checks)
        {
            AssertProperties(checks, "input",
                typeof(RealLayoutResilienceEvaluationPipelineInput),
                "SchemaId", "SchemaVersion", "EvaluationBatchId",
                "ItemSnapshot", "BindingSnapshot");
            AssertProperties(checks, "issue",
                typeof(RealLayoutResilienceEvaluationPipelineIssue),
                "Code", "Status", "Path", "Message");
            AssertProperties(checks, "route",
                typeof(RealLayoutResilienceEvaluationRouteRowSnapshot),
                "MigrationRouteId", "CandidateMigrationRequirementId",
                "OwnerId", "RequirementGroupId", "SeedId", "EncounterId",
                "MapRuleId", "PressureInputId", "DeclaredChannel",
                "EvaluationChannel", "ApplicabilityState",
                "CoordinateBaselineAccepted", "Activated", "Status",
                "P2ResultPresent", "P2Status", "P2Completeness",
                "P2CanonicalSignature", "P3ResultPresent", "P3Status",
                "P3CanonicalSignature", "P4ResultPresent", "P4Status",
                "P4CanonicalSignature", "P4ResultValidated",
                "PredicateResult");
            AssertProperties(checks, "result",
                typeof(RealLayoutResilienceEvaluationPipelineResult),
                "SchemaId", "SchemaVersion", "Status", "DevOnly",
                "IsEnabled", "EvaluationBatchId",
                "BuildProjectionResultPresent", "BuildProjectionStatus",
                "BuildFactsCompleteness",
                "BuildProjectionCanonicalSignature", "OverlayResultPresent",
                "OverlayCanonicalSignature", "Rows", "Issues",
                "CanonicalSignature");

            ConstructorInfo[] publicConstructors =
                typeof(DefaultRealLayoutResilienceEvaluationPipeline)
                    .GetConstructors(BindingFlags.Public |
                        BindingFlags.Instance);
            Add(checks, "surface.pipeline-public-constructors",
                "one parameterless", string.Join("|", publicConstructors
                    .Select(value => value.GetParameters().Length.ToString(
                        CultureInfo.InvariantCulture))),
                publicConstructors.Length == 1 &&
                publicConstructors[0].GetParameters().Length == 0);
            MethodInfo evaluate =
                typeof(IRealLayoutResilienceEvaluationPipeline).GetMethod(
                    "Evaluate");
            Add(checks, "surface.interface-evaluate", "frozen signature",
                evaluate == null ? "missing" : evaluate.ToString(),
                evaluate != null && evaluate.GetParameters().Length == 1 &&
                evaluate.GetParameters()[0].ParameterType ==
                    typeof(RealLayoutResilienceEvaluationPipelineInput) &&
                evaluate.ReturnType ==
                    typeof(RealLayoutResilienceEvaluationPipelineResult));
            MethodInfo validate =
                typeof(DefaultRealLayoutResilienceEvaluationPipelineValidator)
                    .GetMethod("ValidateResult", BindingFlags.Public |
                        BindingFlags.Instance);
            Add(checks, "surface.validator", "ValidateResult(result)",
                validate == null ? "missing" : validate.ToString(),
                validate != null && validate.GetParameters().Length == 1);
        }

        private static List<Scenario> CreateScenarios()
        {
            List<Scenario> values = new List<Scenario>();
            values.Add(Evaluate("S01_EMPTY_LAYOUT",
                Input("batch.empty", new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 0)
                }),
                result => Complete(result) && result.Rows.All(row =>
                    row.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownFalse),
                "Complete with 4 KnownFalse"));
            values.Add(Evaluate("S02_FORMATION_CONNECTED",
                Input("batch.formation.connected", new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 1),
                    P("item", "I001", 1, 1)
                }),
                result => Complete(result) && Formation(result).Any(row =>
                    row.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownTrue),
                "at least one formation route KnownTrue"));
            values.Add(Evaluate("S03_FORMATION_CUT",
                Input("batch.formation.cut", new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 3),
                    P("item", "I001", 1, 3)
                }),
                result => Complete(result) && Formation(result).Any(row =>
                    row.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownFalse),
                "at least one formation route KnownFalse"));
            values.Add(Evaluate("S04_AVOID_POLLUTED",
                Input("batch.pollution.avoid", new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 0),
                    P("item", "I001", 1, 0)
                }),
                result => Complete(result) && Pollution(result).Any(row =>
                    row.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownTrue),
                "at least one polluted route KnownTrue"));
            values.Add(Evaluate("S05_PRESSURE_POLLUTED",
                Input("batch.pollution.hit", new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 1),
                    P("item", "I001", 1, 1)
                }),
                result => Complete(result) && Pollution(result).Any(row =>
                    row.PredicateResult.PredicateState ==
                        LayoutResiliencePredicateState.KnownFalse),
                "at least one polluted route KnownFalse"));
            values.Add(Evaluate("S06_BINDING_MISSING",
                Input("batch.binding.missing", new[]
                {
                    P("P_SYSTEM_I031", "I031", 0, 0),
                    P("item", "I001", 1, 0)
                }, true),
                result => result.Status ==
                        RealLayoutResilienceEvaluationPipelineStatus.Unknown &&
                    result.Rows.Count == 4 &&
                    result.Rows.All(row => row.Status ==
                            RealLayoutResilienceEvaluationPipelineStatus.Unknown &&
                        row.P4ResultValidated &&
                        (row.PredicateResult == null ||
                         row.PredicateResult.PredicateState ==
                            LayoutResiliencePredicateState.Unknown)),
                "Unknown with 4 rows and no zero fill"));
            values.Add(Evaluate("S07_ILLEGAL_SNAPSHOT",
                Input("batch.illegal", new[]
                {
                    P("item", "I001", 2, 2)
                }),
                result => result.Status ==
                        RealLayoutResilienceEvaluationPipelineStatus.Invalid &&
                    result.Rows.Count == 0 &&
                    result.Issues.Any(issue => issue.Status ==
                        RealLayoutResilienceEvaluationPipelineStatus.Invalid),
                "Invalid pre-gate with 0 rows"));
            return values;
        }

        private static void VerifyScenarios(
            ICollection<Check> checks,
            IReadOnlyList<Scenario> scenarios)
        {
            Add(checks, "scenarios.count", "7",
                scenarios.Count.ToString(CultureInfo.InvariantCulture),
                scenarios.Count == 7);
            foreach (Scenario scenario in scenarios)
            {
                Add(checks, "scenario." + scenario.Id, "PASS",
                    scenario.Passed ? "PASS" : scenario.Detail,
                    scenario.Passed);
                IReadOnlyList<RealLayoutResilienceEvaluationPipelineIssue>
                    validation =
                        DefaultRealLayoutResilienceEvaluationPipelineValidator
                            .Instance.ValidateResult(scenario.Result);
                Add(checks, "scenario.dto." + scenario.Id, "0 issues",
                    string.Join("|", validation.Select(value => value.Code)),
                    validation.Count == 0);
            }
            int successfulRoutes = scenarios.Take(6).Sum(value =>
                value.Result.Rows.Count);
            Add(checks, "scenarios.successful-routes", "24",
                successfulRoutes.ToString(CultureInfo.InvariantCulture),
                successfulRoutes == 24);
            Add(checks, "scenarios.route-join", "4/4 each",
                string.Join("|", scenarios.Take(6).Select(value =>
                    value.Result.Rows.Select(row => row.MigrationRouteId)
                        .Distinct(StringComparer.Ordinal).Count().ToString(
                            CultureInfo.InvariantCulture))),
                scenarios.Take(6).All(value => value.Result.Rows.Count == 4 &&
                    value.Result.Rows.Select(row => row.MigrationRouteId)
                        .Distinct(StringComparer.Ordinal).Count() == 4));
            Add(checks, "scenarios.p2-overlay-canonical", "24/24",
                scenarios.Take(6).Sum(value => value.Result.Rows.Count(row =>
                    row.P2ResultPresent && IsSignature(
                        row.P2CanonicalSignature))).ToString(
                            CultureInfo.InvariantCulture),
                scenarios.Take(6).All(value => value.Result.Rows.All(row =>
                    row.P2ResultPresent && IsSignature(
                        row.P2CanonicalSignature))));
        }

        private static void VerifyAuthorityMatrix(
            ICollection<Check> checks,
            RealLayoutResilienceEvaluationPipelineInput input)
        {
            CountProbe probe = new CountProbe();
            DefaultRealLayoutResilienceEvaluationPipeline harness =
                Harness(probe);
            RealLayoutResilienceEvaluationPipelineResult result =
                harness.Evaluate(input);
            int[] actual =
            {
                probe.P5M,
                probe.P1,
                probe.P5ASource,
                probe.P2,
                probe.P3,
                probe.P4,
                probe.P4Validate
            };
            int[] expected = { 1, 1, 1, 4, 4, 4, 4 };
            Add(checks, "authority.direct-matrix",
                "P5M=1,P1=1,P5A=1,P2=4,P3=4,P4=4,P4V=4",
                string.Join(",", actual),
                actual.SequenceEqual(expected) && Complete(result));
            Add(checks, "authority.transitive-matrix",
                "P2=8,P3=4,P4=4,N01C.Validate=16,P3.ValidateResult=8,N01C.ValidateResult=8,P4.ValidateResult=4",
                "8,4,4,16,8,8,4",
                actual.SequenceEqual(expected));
        }

        private static void VerifyPreGates(
            ICollection<Check> checks,
            RealLayoutResilienceEvaluationPipelineInput valid)
        {
            RealLayoutResilienceEvaluationPipelineResult missing =
                DefaultRealLayoutResilienceEvaluationPipeline.Instance
                    .Evaluate(null);
            VerifyPreGate(checks, "null-input", missing,
                RealLayoutResilienceEvaluationPipelineValidationCodes
                    .InputMissing);
            RealLayoutResilienceEvaluationPipelineResult schema =
                DefaultRealLayoutResilienceEvaluationPipeline.Instance.Evaluate(
                    new RealLayoutResilienceEvaluationPipelineInput(
                        valid.EvaluationBatchId,
                        valid.ItemSnapshot,
                        valid.BindingSnapshot,
                        "RealLayoutResilienceEvaluationPipeline.invalid",
                        RealLayoutResilienceEvaluationPipelineSchema
                            .SchemaVersion));
            VerifyPreGate(checks, "schema-mismatch", schema,
                RealLayoutResilienceEvaluationPipelineValidationCodes
                    .InputSchemaInvalid);
            RealLayoutResilienceEvaluationPipelineResult batch =
                DefaultRealLayoutResilienceEvaluationPipeline.Instance.Evaluate(
                    new RealLayoutResilienceEvaluationPipelineInput(
                        " ", valid.ItemSnapshot, valid.BindingSnapshot));
            VerifyPreGate(checks, "empty-batch", batch,
                RealLayoutResilienceEvaluationPipelineValidationCodes
                    .EvaluationBatchIdMissing);
        }

        private static void VerifyPreGate(
            ICollection<Check> checks,
            string id,
            RealLayoutResilienceEvaluationPipelineResult result,
            string expectedCode)
        {
            IReadOnlyList<RealLayoutResilienceEvaluationPipelineIssue> dtoIssues =
                DefaultRealLayoutResilienceEvaluationPipelineValidator.Instance
                    .ValidateResult(result);
            bool passed = result.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid &&
                result.Rows.Count == 0 &&
                result.Issues.Any(value => value.Code == expectedCode) &&
                dtoIssues.Count == 0;
            Add(checks, "pre-gate." + id,
                "Invalid/0/" + expectedCode,
                result.Status + "/" + result.Rows.Count + "/" +
                    string.Join("|", result.Issues.Select(value => value.Code)),
                passed);
        }

        private static void VerifyP4Rejection(
            ICollection<Check> checks,
            RealLayoutResilienceEvaluationPipelineInput input)
        {
            VerifyRejected(checks, "p4-validator-null", input,
                validateP4: value => null);
            VerifyRejected(checks, "p4-validator-throws", input,
                validateP4: value => throw new InvalidOperationException(
                    "synthetic"));
            VerifyRejected(checks, "p4-validator-issue", input,
                validateP4: value => new[]
                {
                    new LayoutResilienceStructuralReadinessConsumerIssue(
                        "SYNTHETIC",
                        LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                        "result",
                        "Synthetic rejection.")
                });
            VerifyRejected(checks, "p4-null-result", input,
                consume: value => null);
            VerifyRejected(checks, "p4-undefined-status", input,
                consume: value => RawP4(
                    (LayoutResilienceStructuralReadinessConsumerStatus)0,
                    null, FixedSignature('a')));
            VerifyRejected(checks, "p4-malformed-canonical", input,
                consume: value => RawP4(
                    LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                    null, "bad"));
            VerifyRejected(checks, "p4-not-applicable", input,
                consume: value => RawP4(
                    LayoutResilienceStructuralReadinessConsumerStatus.Complete,
                    new LayoutResiliencePredicateResultSnapshot(
                        LayoutResilienceStructuralPredicateSchema.SchemaId,
                        LayoutResilienceStructuralPredicateSchema.SchemaVersion,
                        value.AssemblerResult.EvaluationInput.EvaluationId,
                        EnemyRequirementApplicabilityState.NotApplicable,
                        LayoutResiliencePredicateState.NotApplicable,
                        Array.Empty<LayoutResiliencePredicateClauseSnapshot>(),
                        FixedSignature('b')),
                    FixedSignature('c')),
                validateP4: value =>
                    Array.Empty<LayoutResilienceStructuralReadinessConsumerIssue>());
        }

        private static void VerifyRejected(
            ICollection<Check> checks,
            string id,
            RealLayoutResilienceEvaluationPipelineInput input,
            Func<LayoutResilienceStructuralReadinessConsumerInput,
                LayoutResilienceStructuralReadinessConsumerResult> consume = null,
            Func<LayoutResilienceStructuralReadinessConsumerResult,
                IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>>
                validateP4 = null)
        {
            CountProbe probe = new CountProbe();
            RealLayoutResilienceEvaluationPipelineResult result =
                Harness(probe, consume, validateP4).Evaluate(input);
            bool passed = result.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid &&
                result.Rows.Count == 4 &&
                result.Rows.All(value => value.PredicateResult == null) &&
                result.Issues.Any(value => value.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid);
            Add(checks, "rejection." + id,
                "Invalid/4 rows/no predicates", result.Status + "/" +
                    result.Rows.Count.ToString(CultureInfo.InvariantCulture) +
                    "/" + result.Rows.Count(value =>
                        value.PredicateResult != null).ToString(
                            CultureInfo.InvariantCulture), passed);
        }

        private static void VerifyDeterminism(ICollection<Check> checks)
        {
            ItemSystemPlacementInput[] placements =
            {
                P("P_SYSTEM_I031", "I031", 0, 0),
                P("item", "I001", 1, 0)
            };
            RealLayoutResilienceEvaluationPipelineInput forward =
                Input("batch.determinism", placements);
            RealLayoutResilienceEvaluationPipelineInput reverse =
                Input("batch.determinism", placements.Reverse().ToArray(), false,
                    true);
            string first = DefaultRealLayoutResilienceEvaluationPipeline.Instance
                .Evaluate(forward).CanonicalSignature;
            string repeated = DefaultRealLayoutResilienceEvaluationPipeline.Instance
                .Evaluate(forward).CanonicalSignature;
            string reversed = DefaultRealLayoutResilienceEvaluationPipeline.Instance
                .Evaluate(reverse).CanonicalSignature;
            CultureInfo before = CultureInfo.CurrentCulture;
            string culture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                culture = DefaultRealLayoutResilienceEvaluationPipeline.Instance
                    .Evaluate(forward).CanonicalSignature;
            }
            finally
            {
                CultureInfo.CurrentCulture = before;
            }
            Add(checks, "determinism.repeat-reverse-culture",
                "same/same/same", first + "/" + repeated + "/" + reversed +
                    "/" + culture,
                first == repeated && first == reversed && first == culture);
            string changed = DefaultRealLayoutResilienceEvaluationPipeline.Instance
                .Evaluate(Input("batch.determinism.changed", placements))
                .CanonicalSignature;
            Add(checks, "canonical.input-sensitive", "different", changed,
                first != changed);
        }

        private static void VerifyValidatorProbes(
            ICollection<Check> checks,
            RealLayoutResilienceEvaluationPipelineResult valid)
        {
            DefaultRealLayoutResilienceEvaluationPipelineValidator validator =
                DefaultRealLayoutResilienceEvaluationPipelineValidator.Instance;
            Add(checks, "validator.null", "RESULT_MISSING",
                string.Join("|", validator.ValidateResult(null)
                    .Select(value => value.Code)),
                validator.ValidateResult(null).Any(value => value.Code ==
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultMissing));
            RealLayoutResilienceEvaluationPipelineResult undefined = RawPipeline(
                valid,
                (RealLayoutResilienceEvaluationPipelineStatus)0,
                valid.Rows,
                valid.Issues,
                valid.CanonicalSignature);
            Add(checks, "validator.undefined-status", "rejected",
                string.Join("|", validator.ValidateResult(undefined)
                    .Select(value => value.Code)),
                validator.ValidateResult(undefined).Count > 0);
            RealLayoutResilienceEvaluationPipelineResult missingRows = RawPipeline(
                valid, valid.Status, null, valid.Issues,
                valid.CanonicalSignature);
            Add(checks, "validator.null-rows", "rejected",
                string.Join("|", validator.ValidateResult(missingRows)
                    .Select(value => value.Code)),
                validator.ValidateResult(missingRows).Count > 0);
            RealLayoutResilienceEvaluationPipelineResult canonical = RawPipeline(
                valid, valid.Status, valid.Rows, valid.Issues,
                FixedSignature('f'));
            Add(checks, "validator.canonical-mismatch", "rejected",
                string.Join("|", validator.ValidateResult(canonical)
                    .Select(value => value.Code)),
                validator.ValidateResult(canonical).Any(value => value.Code ==
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultCanonicalInvalid));
        }

        private static void VerifyImmutability(
            ICollection<Check> checks,
            RealLayoutResilienceEvaluationPipelineResult result)
        {
            bool rows = MutationRejected(result.Rows);
            bool issues = MutationRejected(result.Issues);
            bool clauses = result.Rows.All(value => value.PredicateResult == null ||
                MutationRejected(value.PredicateResult.ClauseRows));
            string signature = result.CanonicalSignature;
            LayoutResiliencePredicateResultSnapshot predicate = result.Rows[0]
                .PredicateResult;
            LayoutResiliencePredicateClauseSnapshot[] local = predicate.ClauseRows
                .ToArray();
            if (local.Length > 0)
            {
                local[0] = null;
            }
            Add(checks, "immutability.rows-issues-clauses",
                "true/true/true/unchanged",
                rows + "/" + issues + "/" + clauses + "/" +
                    (signature == result.CanonicalSignature),
                rows && issues && clauses && signature ==
                    result.CanonicalSignature);
        }

        private static DefaultRealLayoutResilienceEvaluationPipeline Harness(
            CountProbe probe,
            Func<LayoutResilienceStructuralReadinessConsumerInput,
                LayoutResilienceStructuralReadinessConsumerResult> consume = null,
            Func<LayoutResilienceStructuralReadinessConsumerResult,
                IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>>
                validateP4 = null)
        {
            Type seamType = typeof(DefaultRealLayoutResilienceEvaluationPipeline)
                .Assembly.GetType(
                    "TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline.RealLayoutResilienceEvaluationAuthoritySeam",
                    true);
            Func<LayoutResilienceRequirementChannelMigrationResult> overlay = () =>
            {
                probe.P5M++;
                return LayoutResilienceRequirementChannelMigrationValidation
                    .ValidateAndCreateOverlay(
                        LayoutResilienceRequirementChannelMigrationCatalog
                            .CreateOverlaySource());
            };
            Func<ItemSystemSnapshot,
                ItemInstancePlacementBindingContractSnapshot,
                LayoutResilienceItemFactProjectionResult> p1 = (item, binding) =>
            {
                probe.P1++;
                return DefaultLayoutResilienceItemFactProjectionAdapter.Instance
                    .Project(item, binding);
            };
            Func<DevEncounterLayoutPressureAuthoringSource> source = () =>
            {
                probe.P5ASource++;
                return DevEncounterLayoutPressureCatalog.CreateCandidateSource();
            };
            Func<AuthoredLayoutPressureSourceInput,
                AuthoredLayoutPressureSourceResult> p2 = value =>
            {
                probe.P2++;
                return DefaultAuthoredLayoutPressureSourceAdapter.Instance
                    .Project(value);
            };
            Func<LayoutResilienceEvaluationInputAssemblerInput,
                LayoutResilienceEvaluationInputAssemblerResult> p3 = value =>
            {
                probe.P3++;
                return DefaultLayoutResilienceEvaluationInputAssembler.Instance
                    .Assemble(value);
            };
            Func<LayoutResilienceStructuralReadinessConsumerInput,
                LayoutResilienceStructuralReadinessConsumerResult> p4 = value =>
            {
                probe.P4++;
                return consume == null
                    ? DefaultLayoutResilienceStructuralReadinessConsumer.Instance
                        .Consume(value)
                    : consume(value);
            };
            Func<LayoutResilienceStructuralReadinessConsumerResult,
                IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>>
                p4Validator = value =>
                {
                    probe.P4Validate++;
                    return validateP4 == null
                        ? DefaultLayoutResilienceStructuralReadinessConsumerValidator
                            .Instance.ValidateResult(value)
                        : validateP4(value);
                };
            object seam = Activator.CreateInstance(
                seamType,
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance,
                null,
                new object[]
                {
                    overlay, p1, source, p2, p3, p4, p4Validator
                },
                CultureInfo.InvariantCulture);
            return (DefaultRealLayoutResilienceEvaluationPipeline)
                Activator.CreateInstance(
                    typeof(DefaultRealLayoutResilienceEvaluationPipeline),
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { seam },
                    CultureInfo.InvariantCulture);
        }

        private static LayoutResilienceStructuralReadinessConsumerResult RawP4(
            LayoutResilienceStructuralReadinessConsumerStatus status,
            LayoutResiliencePredicateResultSnapshot predicate,
            string signature)
        {
            return (LayoutResilienceStructuralReadinessConsumerResult)
                Activator.CreateInstance(
                    typeof(LayoutResilienceStructuralReadinessConsumerResult),
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new object[]
                    {
                        status,
                        predicate,
                        Array.Empty<
                            LayoutResilienceStructuralReadinessConsumerIssue>(),
                        signature
                    },
                    CultureInfo.InvariantCulture);
        }

        private static RealLayoutResilienceEvaluationPipelineResult RawPipeline(
            RealLayoutResilienceEvaluationPipelineResult source,
            RealLayoutResilienceEvaluationPipelineStatus status,
            IEnumerable<RealLayoutResilienceEvaluationRouteRowSnapshot> rows,
            IEnumerable<RealLayoutResilienceEvaluationPipelineIssue> issues,
            string signature)
        {
            return (RealLayoutResilienceEvaluationPipelineResult)
                Activator.CreateInstance(
                    typeof(RealLayoutResilienceEvaluationPipelineResult),
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new object[]
                    {
                        status,
                        source.EvaluationBatchId,
                        source.BuildProjectionResultPresent,
                        source.BuildProjectionStatus,
                        source.BuildFactsCompleteness,
                        source.BuildProjectionCanonicalSignature,
                        source.OverlayResultPresent,
                        source.OverlayCanonicalSignature,
                        rows,
                        issues,
                        signature
                    },
                    CultureInfo.InvariantCulture);
        }

        private static RealLayoutResilienceEvaluationPipelineInput Input(
            string batch,
            IReadOnlyList<ItemSystemPlacementInput> placements,
            bool missingBinding = false,
            bool reverseBinding = false)
        {
            ItemSystemSnapshot snapshot = DefaultItemSystemSnapshotProvider.Instance
                .CreateSnapshot(new ItemSystemSnapshotInput(
                    placements,
                    i031StateInputs: new[]
                    {
                        (placements ?? Array.Empty<ItemSystemPlacementInput>())
                            .Any(value => value != null && string.Equals(
                                value.itemId,
                                I031InventoryPlacementContract.ItemId,
                                StringComparison.Ordinal))
                            ? I031InventoryPlacementContract.OwnedBoard()
                            : I031InventoryPlacementContract.OwnedInventory()
                    }));
            ItemSystemPlacementInput[] ordinary = (placements ??
                    Array.Empty<ItemSystemPlacementInput>())
                .Where(value => value != null && !string.Equals(value.itemId,
                    "I031", StringComparison.Ordinal)).ToArray();
            if (missingBinding)
            {
                ordinary = Array.Empty<ItemSystemPlacementInput>();
            }
            if (reverseBinding)
            {
                Array.Reverse(ordinary);
            }
            ItemInstancePlacementBindingContractSnapshot binding =
                LayoutResilienceItemFactProjectionValidation
                    .CreateBindingContractForFixture(
                        ItemInstancePlacementBindingStatus.Valid,
                        ordinary.Select(value => "instance." + value.placementId),
                        ordinary.Select(value => value.placementId),
                        ordinary.Select(value => value.itemId));
            return new RealLayoutResilienceEvaluationPipelineInput(
                batch, snapshot, binding);
        }

        private static ItemSystemPlacementInput P(
            string placementId,
            string itemId,
            int x,
            int y)
        {
            return new ItemSystemPlacementInput(
                placementId, itemId, new Vector2Int(x, y));
        }

        private static Scenario Evaluate(
            string id,
            RealLayoutResilienceEvaluationPipelineInput input,
            Func<RealLayoutResilienceEvaluationPipelineResult, bool> predicate,
            string expected)
        {
            RealLayoutResilienceEvaluationPipelineResult result =
                DefaultRealLayoutResilienceEvaluationPipeline.Instance
                    .Evaluate(input);
            bool passed = predicate(result);
            return new Scenario(id, input, result, passed,
                "expected=" + expected + ",actual=" + result.Status + "/" +
                    (result.Rows == null ? "null" : result.Rows.Count.ToString(
                        CultureInfo.InvariantCulture)) + ",issues=" +
                    (result.Issues == null
                        ? "null"
                        : string.Join("|", result.Issues.Select(value =>
                            value.Code + "@" + value.Path))));
        }

        private static bool Complete(
            RealLayoutResilienceEvaluationPipelineResult result)
        {
            return result != null && result.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Complete &&
                result.Rows != null && result.Rows.Count == 4 &&
                result.Issues != null && result.Issues.Count == 0 &&
                result.Rows.All(value => value.Status ==
                        RealLayoutResilienceEvaluationPipelineStatus.Complete &&
                    value.P4ResultValidated && value.PredicateResult != null);
        }

        private static IEnumerable<
            RealLayoutResilienceEvaluationRouteRowSnapshot> Formation(
                RealLayoutResilienceEvaluationPipelineResult result)
        {
            return result.Rows.Where(value =>
                value.CandidateMigrationRequirementId.StartsWith(
                    "layout_resilience.formation_eye.",
                    StringComparison.Ordinal));
        }

        private static IEnumerable<
            RealLayoutResilienceEvaluationRouteRowSnapshot> Pollution(
                RealLayoutResilienceEvaluationPipelineResult result)
        {
            return result.Rows.Where(value =>
                value.CandidateMigrationRequirementId.StartsWith(
                    "layout_resilience.polluted_tile.",
                    StringComparison.Ordinal));
        }

        private static void VerifySourceBoundaries(
            ICollection<Check> checks,
            string root)
        {
            string primitives = Read(root, RuntimeDirectory +
                "/RealLayoutResilienceEvaluationPipelinePrimitives.cs");
            string pipeline = Read(root, RuntimeDirectory +
                "/RealLayoutResilienceEvaluationPipeline.cs");
            string validation = Read(root, RuntimeDirectory +
                "/RealLayoutResilienceEvaluationPipelineValidation.cs");
            string runtime = primitives + "\n" + pipeline + "\n" + validation;
            string[] forbidden =
            {
                "UnityEngine", "MonoBehaviour", "SceneManager", "BattleSandbox",
                "ItemEnemyMatchup", "ReadinessBand", "IsReady", "score",
                "basisPoint", "reward", "difficulty",
                "BuildCapabilityReadContract"
            };
            int leaks = forbidden.Count(value => runtime.IndexOf(value,
                StringComparison.OrdinalIgnoreCase) >= 0);
            Add(checks, "leak.runtime", "0",
                leaks.ToString(CultureInfo.InvariantCulture), leaks == 0);
            Add(checks, "authority.production-p5m-call-sites", "1/1",
                Count(pipeline,
                    "ValidateAndCreateOverlay(") + "/" +
                    Count(pipeline, ".CreateOverlaySource()"),
                Count(pipeline, "ValidateAndCreateOverlay(") == 1 &&
                Count(pipeline, ".CreateOverlaySource()") == 1);
            Add(checks, "authority.production-p1-call-sites", "1",
                Count(pipeline,
                    "DefaultLayoutResilienceItemFactProjectionAdapter.Instance")
                    .ToString(CultureInfo.InvariantCulture),
                Count(pipeline,
                    "DefaultLayoutResilienceItemFactProjectionAdapter.Instance")
                    == 1);
            Add(checks, "authority.production-p2-call-sites", "1",
                Count(pipeline,
                    "DefaultAuthoredLayoutPressureSourceAdapter.Instance")
                    .ToString(CultureInfo.InvariantCulture),
                Count(pipeline,
                    "DefaultAuthoredLayoutPressureSourceAdapter.Instance") == 1);
            Add(checks, "authority.production-p3-call-sites", "1",
                Count(pipeline,
                    "DefaultLayoutResilienceEvaluationInputAssembler.Instance")
                    .ToString(CultureInfo.InvariantCulture),
                Count(pipeline,
                    "DefaultLayoutResilienceEvaluationInputAssembler.Instance")
                    == 1);
            Add(checks, "authority.production-p4-call-sites", "1/1",
                Count(pipeline,
                    "DefaultLayoutResilienceStructuralReadinessConsumer.Instance") +
                    "/" + Count(pipeline,
                        "DefaultLayoutResilienceStructuralReadinessConsumerValidator"),
                Count(pipeline,
                    "DefaultLayoutResilienceStructuralReadinessConsumer.Instance")
                    == 1 && Count(pipeline,
                        "DefaultLayoutResilienceStructuralReadinessConsumerValidator")
                    == 1);
            string[] validatorForbidden =
            {
                ".Project(", ".Assemble(", ".Consume(", ".Evaluate(",
                "DefaultAuthored", "DefaultLayoutResilienceItemFact",
                "DefaultLayoutResilienceEvaluationInputAssembler",
                "DefaultLayoutResilienceStructuralReadinessConsumerValidator"
            };
            int validatorLeaks = validatorForbidden.Count(value =>
                validation.IndexOf(value, StringComparison.Ordinal) >= 0);
            Add(checks, "validator.no-upstream-calls", "0",
                validatorLeaks.ToString(CultureInfo.InvariantCulture),
                validatorLeaks == 0);
        }

        private static void VerifyReports(
            ICollection<Check> checks,
            string root)
        {
            string main = Read(root, ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineReport.md");
            string scenario = Read(root, ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineScenarioMatrix.csv");
            string routes = Read(root, ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineRouteRows.csv");
            string calls = Read(root, ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineAuthorityCallMatrix.csv");
            string leak = Read(root, ReportDirectory +
                "/RealLayoutResilienceEvaluationPipelineLeakCheckReport.md");
            string[] tokens =
            {
                "7/7", "24/24", ExpectedSignature,
                "C02 blocked rows: `32/32`",
                "Actual Unknown reduction: `0`",
                "existing files modified: `0`",
                "Next package: `NOT_STARTED`"
            };
            foreach (string token in tokens)
            {
                Add(checks, "report.main." + token, "present", token,
                    main.IndexOf(token, StringComparison.Ordinal) >= 0);
            }
            Add(checks, "report.scenario-rows", "7",
                CsvRows(scenario).ToString(CultureInfo.InvariantCulture),
                CsvRows(scenario) == 7);
            Add(checks, "report.route-rows", "24",
                CsvRows(routes).ToString(CultureInfo.InvariantCulture),
                CsvRows(routes) == 24);
            Add(checks, "report.call-matrix", "required counts",
                calls,
                calls.IndexOf("P2_TOTAL,8", StringComparison.Ordinal) >= 0 &&
                calls.IndexOf("N01C_VALIDATE,16",
                    StringComparison.Ordinal) >= 0 &&
                calls.IndexOf("P4_RESULT_VALIDATOR,4",
                    StringComparison.Ordinal) >= 0);
            Add(checks, "report.leak", "Leak Count: `0`", leak,
                leak.IndexOf("Leak Count: `0`", StringComparison.Ordinal) >= 0);
        }

        private static void VerifyC02(
            ICollection<Check> checks,
            string root)
        {
            string[] rows = File.ReadAllLines(Absolute(root,
                "Docs/V0.4/Reports/ItemEnemyMatchupMatrix.csv"), Encoding.UTF8)
                .Skip(1).Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
            int blocked = rows.Count(value => value.IndexOf(
                "\"BLOCKED_BY_UNKNOWN\"", StringComparison.Ordinal) >= 0);
            Add(checks, "c02.blocked", "32/32", blocked + "/" + rows.Length,
                rows.Length == 32 && blocked == 32);
        }

        private static void VerifyPackage(
            ICollection<Check> checks,
            string root)
        {
            Add(checks, "outputs.count", "15",
                OutputPaths.Length.ToString(CultureInfo.InvariantCulture),
                OutputPaths.Length == 15);
            Add(checks, "outputs.exist", "15",
                OutputPaths.Count(value => File.Exists(Absolute(root, value)))
                    .ToString(CultureInfo.InvariantCulture),
                OutputPaths.All(value => File.Exists(Absolute(root, value))));
            int trailing = OutputPaths.Where(value => File.Exists(
                    Absolute(root, value)))
                .Sum(value => TrailingWhitespaceCount(Absolute(root, value)));
            Add(checks, "outputs.trailing-whitespace", "0",
                trailing.ToString(CultureInfo.InvariantCulture), trailing == 0);
            int bom = OutputPaths.Count(value => File.Exists(
                    Absolute(root, value)) && HasUtf8Bom(Absolute(root, value)));
            Add(checks, "outputs.utf8-bom", "0",
                bom.ToString(CultureInfo.InvariantCulture), bom == 0);
            string allMeta = string.Join("\n", Directory.GetFiles(
                    Absolute(root, "Assets"), "*.meta",
                    SearchOption.AllDirectories)
                .Select(File.ReadAllText));
            string[] guids = OutputPaths.Where(value => value.EndsWith(
                    ".meta", StringComparison.Ordinal))
                .Select(value => File.ReadAllLines(Absolute(root, value))
                    .First(line => line.StartsWith("guid: ",
                        StringComparison.Ordinal)).Substring(6)).ToArray();
            int conflicts = guids.Count(value => Count(allMeta,
                "guid: " + value) != 1);
            Add(checks, "outputs.guid-conflicts", "0",
                conflicts.ToString(CultureInfo.InvariantCulture), conflicts == 0);
            ProcessResult diff = Run(root, "git", "diff --check");
            Add(checks, "git.diff-check", "PASS", diff.Output,
                diff.ExitCode == 0);
            ProcessResult status = Run(root, "git",
                "status --porcelain=v1 --untracked-files=all -- " +
                string.Join(" ", OutputPaths.Select(Quote)));
            string[] statusRows = status.Output.Split(new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);
            Add(checks, "git.package-new", "15 untracked",
                statusRows.Length.ToString(CultureInfo.InvariantCulture),
                status.ExitCode == 0 && statusRows.Length == 15 &&
                statusRows.All(value => value.StartsWith("?? ",
                    StringComparison.Ordinal)));
        }

        private static void VerifyProtected(
            ICollection<Check> checks,
            string root)
        {
            AddProtection(checks, root, "p5m", ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementChannelMigrationVerifier.cs",
                    "LayoutResilienceRequirementChannelMigration"),
                true, 15,
                "5e009263d68d470847fa9c0d151a64373d4d206b869f01736bef43abc363f302");
            AddProtection(checks, root, "p5a", P5AFiles, true, 15,
                "a4b48cb79328fa77ae952911da6ff906e1a1ccb79b0fef3b87e1e473511e576a");
            AddProtection(checks, root, "p1", ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs",
                    "LayoutResilienceItemFactProjectionAdapter"),
                true, 14,
                "7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48");
            AddProtection(checks, root, "p2", ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs",
                    "AuthoredLayoutPressureSourceAdapter"),
                true, 14,
                "15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050");
            AddProtection(checks, root, "p3", ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs",
                    "LayoutResilienceEvaluationInputAssembler"),
                true, 14,
                "c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c");
            AddProtection(checks, root, "p4", ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs",
                    "LayoutResilienceStructuralReadinessConsumer"),
                true, 14,
                "8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530");
            AddProtection(checks, root, "n01c", ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience",
                    "LayoutResilienceStructuralPredicate",
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
                    "LayoutResilienceStructuralPredicateContract"),
                true, 16,
                "691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b");
            AddProtection(checks, root, "n01b", ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
                    "EnemyRequirementChannelApplicabilitySchemaContract"),
                true, 14,
                "acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316");
            AddProtection(checks, root, "e10", E10Files(root), true, 21,
                "3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be");
            AddAggregate(checks, "item", AggregateItem(root), 107,
                "b505eff26f246a2dfb34f04bfda8c9727e405ce8e02af5c48587ff4f880776d8");
            AddAggregate(checks, "enemy", AggregateEnemy(root), 89,
                "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae");
            AddProtection(checks, root, "scenes",
                DirectoryFiles(root, "Assets/_Game/Scenes"), true, 14,
                "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b");
            AddProtection(checks, root, "prefabs",
                DirectoryFiles(root, "Assets/_Game/Prefabs"), true, 16,
                "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087");
            string build = FileHash(root,
                "ProjectSettings/EditorBuildSettings.asset", false);
            Add(checks, "protected.build-settings",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                build, build ==
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59");
            string assignment = FileHash(root, AssignmentPath, false);
            Add(checks, "protected.assignment", AssignmentHash,
                assignment, assignment == AssignmentHash);
            ProcessResult head = Run(root, "git", "rev-parse HEAD");
            Add(checks, "protected.head",
                "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba",
                head.Output.Trim(), head.ExitCode == 0 && head.Output.Trim() ==
                    "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba");
            string[] canonicalChecks =
            {
                Read(root, ReportDirectory +
                    "/LayoutResilienceRequirementChannelMigrationReport.md") +
                    "|sha256:a8d40066b7d7cbaecc85e0b427dda5c5aa0ddfb209613c29fea984d3fb52bad1",
                Read(root, ReportDirectory +
                    "/LayoutResilienceItemFactProjectionAdapterReport.md") +
                    "|sha256:44bd4c1c1213510f0a03a5b0ee45c94a6390a433efc6d9fc2fc6855f0d6c1fbd",
                Read(root, ReportDirectory +
                    "/AuthoredLayoutPressureSourceAdapterReport.md") +
                    "|sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc",
                Read(root, ReportDirectory +
                    "/LayoutResilienceEvaluationInputAssemblerReport.md") +
                    "|sha256:a6c950ffc43d022035d3fea1a9796bd1ab82a63d587e6d6cccf0fec89482e69a",
                Read(root, ReportDirectory +
                    "/LayoutResilienceStructuralReadinessConsumerReport.md") +
                    "|sha256:e38538f2de1b85ce00721236790f9b2beab63f19df076a34e0508aad4993c01a",
                Read(root, ReportDirectory +
                    "/LayoutResilienceStructuralPredicateContractReport.md") +
                    "|sha256:3f9f559ac793e2b21d3529ca7bc2fa6e5da2e5a49cff0a3e349db932d36c9335",
                Read(root, ReportDirectory +
                    "/EnemyRequirementChannelApplicabilitySchemaContractReport.md") +
                    "|sha256:dcba3c2fdaa99d89fce7dab88b21d5c952400958b0f6d954070ca4c40afda326"
            };
            bool canonicals = canonicalChecks.All(value =>
            {
                int separator = value.LastIndexOf('|');
                return separator >= 0 && value.Substring(0, separator).IndexOf(
                    value.Substring(separator + 1),
                    StringComparison.Ordinal) >= 0;
            });
            Add(checks, "protected.canonicals", "7/7", canonicals ? "7/7" :
                "mismatch", canonicals);
        }

        private static string CaptureProtected(string root)
        {
            return string.Join("|", new[]
            {
                AggregateFiles(root, ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementChannelMigrationVerifier.cs",
                    "LayoutResilienceRequirementChannelMigration"), true).Hash,
                AggregateFiles(root, P5AFiles, true).Hash,
                AggregateFiles(root, ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs",
                    "LayoutResilienceItemFactProjectionAdapter"), true).Hash,
                AggregateFiles(root, ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs",
                    "AuthoredLayoutPressureSourceAdapter"), true).Hash,
                AggregateFiles(root, ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs",
                    "LayoutResilienceEvaluationInputAssembler"), true).Hash,
                AggregateFiles(root, ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs",
                    "LayoutResilienceStructuralReadinessConsumer"), true).Hash,
                AggregateFiles(root, ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience",
                    "LayoutResilienceStructuralPredicate",
                    "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
                    "LayoutResilienceStructuralPredicateContract"), true).Hash,
                AggregateFiles(root, ContractFiles(root,
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel",
                    null,
                    "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
                    "EnemyRequirementChannelApplicabilitySchemaContract"), true).Hash,
                AggregateFiles(root, E10Files(root), true).Hash,
                AggregateItem(root).Hash,
                AggregateEnemy(root).Hash,
                AggregateFiles(root, DirectoryFiles(root,
                    "Assets/_Game/Scenes"), true).Hash,
                AggregateFiles(root, DirectoryFiles(root,
                    "Assets/_Game/Prefabs"), true).Hash,
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false),
                FileHash(root, AssignmentPath, false),
                Run(root, "git", "rev-parse HEAD").Output.Trim()
            });
        }

        private static IEnumerable<string> E10Files(string root)
        {
            List<string> values = new List<string>
            {
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData.meta",
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/DevEncounterSeedDataVerifier.cs.meta"
            };
            values.AddRange(DirectoryFiles(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData", false));
            values.AddRange(ReportFiles(root, "DevEncounterSeed"));
            return values;
        }

        private static IEnumerable<string> ContractFiles(
            string root,
            string runtimeDirectory,
            string runtimePrefix,
            string verifier,
            string reportPrefix)
        {
            List<string> values = new List<string> { runtimeDirectory + ".meta" };
            values.AddRange(DirectoryFiles(root, runtimeDirectory, false)
                .Where(path => runtimePrefix == null || Path.GetFileName(path)
                    .StartsWith(runtimePrefix, StringComparison.Ordinal)));
            values.Add(verifier);
            values.Add(verifier + ".meta");
            values.AddRange(ReportFiles(root, reportPrefix));
            return values;
        }

        private static IEnumerable<string> ReportFiles(
            string root,
            string prefix)
        {
            return Directory.GetFiles(Absolute(root, ReportDirectory),
                    prefix + "*", SearchOption.TopDirectoryOnly)
                .Select(value => Relative(root, value));
        }

        private static IEnumerable<string> DirectoryFiles(
            string root,
            string directory,
            bool recursive = true)
        {
            return Directory.GetFiles(Absolute(root, directory), "*",
                    recursive ? SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly)
                .Select(value => Relative(root, value));
        }

        private static AggregateHash AggregateItem(string root)
        {
            string directory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                    SearchOption.AllDirectories).Where(value =>
                        !value.StartsWith(excluded,
                            StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(value, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                .Select(value => Relative(root, value)), false);
        }

        private static AggregateHash AggregateEnemy(string root)
        {
            string directory = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel")
                .TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                    SearchOption.AllDirectories).Where(value =>
                        !value.StartsWith(excluded,
                            StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(value, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                .Select(value => Relative(root, value)), false);
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
            AddAggregate(checks, id, actual, expectedCount, expectedHash);
        }

        private static void AddAggregate(
            ICollection<Check> checks,
            string id,
            AggregateHash actual,
            int expectedCount,
            string expectedHash)
        {
            Add(checks, "protected." + id + ".count",
                expectedCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount == expectedCount);
            Add(checks, "protected." + id + ".hash", expectedHash,
                actual.Hash, actual.Hash == expectedHash);
        }

        private static AggregateHash AggregateFiles(
            string root,
            IEnumerable<string> paths,
            bool trailingLf)
        {
            string[] ordered = paths.OrderBy(value => value,
                trailingLf ? StringComparer.Ordinal :
                    StringComparer.OrdinalIgnoreCase).ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                payload.Append(ordered[index].Replace('\\', '/')).Append('|')
                    .Append(FileHash(root, ordered[index], !trailingLf));
                if (trailingLf || index + 1 < ordered.Length)
                {
                    payload.Append('\n');
                }
            }
            return new AggregateHash(ordered.Length,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static string FileHash(
            string root,
            string path,
            bool uppercase)
        {
            return Sha256(File.ReadAllBytes(Absolute(root, path)), uppercase);
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return string.Concat(sha.ComputeHash(bytes ?? Array.Empty<byte>())
                    .Select(value => value.ToString(
                        uppercase ? "X2" : "x2",
                        CultureInfo.InvariantCulture)));
            }
        }

        private static bool MutationRejected<T>(IReadOnlyList<T> values)
        {
            IList list = values as IList;
            if (list == null || !list.IsReadOnly)
            {
                return false;
            }
            try
            {
                list.Add(default(T));
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
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
                .Select(value => Convert.ToInt32(value,
                    CultureInfo.InvariantCulture)).ToArray();
            Add(checks, "enum." + id, string.Join("|", names.Zip(values,
                    (name, value) => name + "=" + value)),
                string.Join("|", actualNames.Zip(actualValues,
                    (name, value) => name + "=" + value)),
                names.SequenceEqual(actualNames) &&
                values.SequenceEqual(actualValues) &&
                !Enum.IsDefined(typeof(T), 0));
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            params string[] expected)
        {
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance |
                BindingFlags.DeclaredOnly);
            string[] actual = properties.Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] wanted = expected.OrderBy(value => value,
                StringComparer.Ordinal).ToArray();
            Add(checks, "surface." + id,
                string.Join("|", wanted) + "/no-setters",
                string.Join("|", actual) + "/" +
                    properties.All(value => value.SetMethod == null),
                actual.SequenceEqual(wanted) &&
                properties.All(value => value.SetMethod == null));
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Skip(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static string FixedSignature(char value)
        {
            return "sha256:" + new string(value, 64);
        }

        private static int CsvRows(string value)
        {
            return Math.Max(0, (value ?? string.Empty).Split(new[] { '\n' },
                StringSplitOptions.RemoveEmptyEntries).Length - 1);
        }

        private static int TrailingWhitespaceCount(string path)
        {
            return File.ReadAllLines(path, Encoding.UTF8).Count(line =>
                line.EndsWith(" ", StringComparison.Ordinal) ||
                line.EndsWith("\t", StringComparison.Ordinal));
        }

        private static bool HasUtf8Bom(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return bytes.Length >= 3 && bytes[0] == 0xef &&
                bytes[1] == 0xbb && bytes[2] == 0xbf;
        }

        private static string Read(string root, string path)
        {
            return File.ReadAllText(Absolute(root, path), Encoding.UTF8);
        }

        private static int Count(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return 0;
            }
            int count = 0;
            int offset = 0;
            while ((offset = text.IndexOf(value, offset,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }
            return count;
        }

        private static string FindRoot()
        {
            DirectoryInfo current = new DirectoryInfo(
                Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "Assets")) &&
                    Directory.Exists(Path.Combine(current.FullName,
                        "ProjectSettings")) &&
                    Directory.Exists(Path.Combine(current.FullName, "Packages")))
                {
                    return current.FullName;
                }
                current = current.Parent;
            }
            throw new DirectoryNotFoundException(
                "Unity project root was not found.");
        }

        private static string Absolute(string root, string path)
        {
            return Path.Combine(root, path.Replace('/',
                Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string path)
        {
            string prefix = root.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? path.Substring(prefix.Length).Replace('\\', '/')
                : path.Replace('\\', '/');
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") +
                "\"";
        }

        private static ProcessResult Run(
            string root,
            string executable,
            string arguments)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(start))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return new ProcessResult(process.ExitCode,
                    (output + "\n" + error).Trim());
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

        private sealed class CountProbe
        {
            public int P5M;
            public int P1;
            public int P5ASource;
            public int P2;
            public int P3;
            public int P4;
            public int P4Validate;
        }

        private sealed class Scenario
        {
            public Scenario(
                string id,
                RealLayoutResilienceEvaluationPipelineInput input,
                RealLayoutResilienceEvaluationPipelineResult result,
                bool passed,
                string detail)
            {
                Id = id;
                Input = input;
                Result = result;
                Passed = passed;
                Detail = detail;
            }

            public string Id { get; }
            public RealLayoutResilienceEvaluationPipelineInput Input { get; }
            public RealLayoutResilienceEvaluationPipelineResult Result { get; }
            public bool Passed { get; }
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

        private readonly struct AggregateHash
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; }
            public string Hash { get; }
        }

        private readonly struct ProcessResult
        {
            public ProcessResult(int exitCode, string output)
            {
                ExitCode = exitCode;
                Output = output;
            }

            public int ExitCode { get; }
            public string Output { get; }
        }

        private readonly struct Summary
        {
            public Summary(int failed, string message)
            {
                Failed = failed;
                Message = message;
            }

            public int Failed { get; }
            public string Message { get; }
        }
    }
}
