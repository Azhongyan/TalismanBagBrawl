using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class AuthoredLayoutPressureSourceAdapterVerifier
    {
        private const string ExpectedSignature =
            "sha256:7b4896576e31cf1312f0a0f17135e3887934113446454fda7c27c1c3e8911ecc";
        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure";
        private const string ReportDirectory = "Docs/V0.4/Reports";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory + "/AuthoredLayoutPressureSourcePrimitives.cs",
            RuntimeDirectory + "/AuthoredLayoutPressureSourcePrimitives.cs.meta",
            RuntimeDirectory + "/AuthoredLayoutPressureSourceAdapter.cs",
            RuntimeDirectory + "/AuthoredLayoutPressureSourceAdapter.cs.meta",
            RuntimeDirectory + "/AuthoredLayoutPressureSourceValidation.cs",
            RuntimeDirectory + "/AuthoredLayoutPressureSourceValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs.meta",
            ReportDirectory + "/AuthoredLayoutPressureSourceAdapterReport.md",
            ReportDirectory + "/AuthoredLayoutPressureSourceAdapterSpec.csv",
            ReportDirectory + "/AuthoredLayoutPressureSourceAdapterFieldMatrix.csv",
            ReportDirectory + "/AuthoredLayoutPressureSourceAdapterFixtureCases.csv",
            ReportDirectory + "/AuthoredLayoutPressureSourceAdapterLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Authored Layout Pressure Source Adapter")]
        public static void VerifyMenu()
        {
            VerifyOffline();
        }
#endif

        public static void VerifyOffline()
        {
            VerificationSummary summary = VerifyCore();
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
                UnityEngine.Debug.Log("AUTHORED_LAYOUT_PRESSURE_SOURCE_ADAPTER PASS");
                EditorApplication.Exit(0);
#endif
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.LogException(exception);
                EditorApplication.Exit(1);
#else
                _ = exception;
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

        private static VerificationSummary VerifyCore()
        {
            List<Check> checks = new List<Check>();
            IAuthoredLayoutPressureSourceAdapter adapter =
                DefaultAuthoredLayoutPressureSourceAdapter.Instance;

            AssertEnum<AuthoredLayoutPressureAuthoringCompleteness>(checks,
                "authoring-completeness", new[] { "Complete", "Incomplete" },
                new[] { 1, 2 });
            AssertEnum<AuthoredLayoutPressureSourceStatus>(checks, "status",
                new[] { "Complete", "Unknown", "Invalid" }, new[] { 1, 2, 3 });
            Add(checks, "schema.id", "AuthoredLayoutPressureSourceAdapter.v1",
                AuthoredLayoutPressureSourceSchema.SchemaId,
                AuthoredLayoutPressureSourceSchema.SchemaId ==
                    "AuthoredLayoutPressureSourceAdapter.v1");
            Add(checks, "schema.version", "1",
                AuthoredLayoutPressureSourceSchema.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture),
                AuthoredLayoutPressureSourceSchema.SchemaVersion == 1);
            AssertPublicSurface(checks);

            List<Scenario> scenarios = new List<Scenario>();
            AuthoredLayoutPressureSourceResult case1 = adapter.Project(Valid(
                "dev.polluted", new[] { LayoutPressureKind.PollutedCellMask },
                Domain3().Where(cell => !(cell.X == 2 && cell.Y == 2)),
                Array.Empty<LayoutCellConnection>(),
                new[]
                {
                    LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
                    LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable
                }));
            AddScenario(scenarios, "P2-01", case1,
                AuthoredLayoutPressureSourceStatus.Complete, true);

            AuthoredLayoutPressureSourceResult case2 = adapter.Project(Valid(
                "dev.eye", new[] { LayoutPressureKind.EyeRelocationOrDisruption },
                Domain3(), Array.Empty<LayoutCellConnection>(),
                new[] { LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable },
                Cell(2, 2)));
            AddScenario(scenarios, "P2-02", case2,
                AuthoredLayoutPressureSourceStatus.Complete, true);

            LayoutCellConnection forward = Edge(0, 0, 1, 0);
            AuthoredLayoutPressureSourceResult case3 = adapter.Project(Valid(
                "dev.edge", new[] { LayoutPressureKind.StructuralConnectionCut },
                Domain3(), new[] { forward },
                new[]
                {
                    LayoutResiliencePredicateClauseKind
                        .EyeToCountedCoreStructurallyConnected
                }));
            AddScenario(scenarios, "P2-03", case3,
                AuthoredLayoutPressureSourceStatus.Complete, true);

            AuthoredLayoutPressureSourceResult case4 = adapter.Project(Valid(
                "dev.combined",
                Enum.GetValues(typeof(LayoutPressureKind)).Cast<LayoutPressureKind>(),
                Domain3(), new[] { Edge(0, 0, 0, 1), Edge(0, 1, 1, 1) },
                new[]
                {
                    LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                    LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
                    LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable,
                    LayoutResiliencePredicateClauseKind
                        .EyeToCountedCoreStructurallyConnected
                }));
            AddScenario(scenarios, "P2-04", case4,
                AuthoredLayoutPressureSourceStatus.Complete, true);

            AuthoredLayoutPressureSourceResult case5 = adapter.Project(Valid(
                "dev.one-clause", new[] { LayoutPressureKind.PollutedCellMask },
                Domain3(), Array.Empty<LayoutCellConnection>(),
                new[] { LayoutResiliencePredicateClauseKind.CountedLayoutPresent }));
            AddScenario(scenarios, "P2-05", case5,
                AuthoredLayoutPressureSourceStatus.Complete, true);
            Add(checks, "fixture.05.no-default-clauses", "1",
                case5.PressureSnapshot.RequiredPredicateClauses.Count.ToString(
                    CultureInfo.InvariantCulture),
                case5.PressureSnapshot.RequiredPredicateClauses.Count == 1);

            AuthoredLayoutPressureSourceResult case6 = adapter.Project(Valid(
                "dev.empty-mask", new[] { LayoutPressureKind.PollutedCellMask },
                Array.Empty<LayoutCellCoordinate>(),
                Array.Empty<LayoutCellConnection>(),
                new[] { LayoutResiliencePredicateClauseKind.CountedLayoutPresent }));
            AddScenario(scenarios, "P2-06", case6,
                AuthoredLayoutPressureSourceStatus.Complete, true);
            Add(checks, "fixture.06.explicit-empty-preserved", "0/0",
                case6.PressureSnapshot.UsableCellsAfterPressure.Count + "/" +
                    case6.PressureSnapshot
                        .PreservedStructuralConnectionsAfterPressure.Count,
                case6.PressureSnapshot.UsableCellsAfterPressure.Count == 0 &&
                case6.PressureSnapshot
                    .PreservedStructuralConnectionsAfterPressure.Count == 0);

            AuthoredLayoutPressureSourceResult case7 = adapter.Project(Valid(
                "dev.edge", new[] { LayoutPressureKind.StructuralConnectionCut },
                Domain3(), new[] { Edge(1, 0, 0, 0) },
                new[]
                {
                    LayoutResiliencePredicateClauseKind
                        .EyeToCountedCoreStructurallyConnected
                }));
            AddScenario(scenarios, "P2-07", case7,
                AuthoredLayoutPressureSourceStatus.Complete, true);
            Add(checks, "fixture.07.edge-normalized-signature", case3.CanonicalSignature,
                case7.CanonicalSignature,
                case3.CanonicalSignature == case7.CanonicalSignature);
            LayoutCellConnection normalized = case7.PressureSnapshot
                .PreservedStructuralConnectionsAfterPressure.Single();
            Add(checks, "fixture.07.edge-normalized-output", "0:0>1:0",
                normalized.CellA.X + ":" + normalized.CellA.Y + ">" +
                    normalized.CellB.X + ":" + normalized.CellB.Y,
                normalized.CellA.Equals(Cell(0, 0)) &&
                normalized.CellB.Equals(Cell(1, 0)));

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            AuthoredLayoutPressureSourceResult case8;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                case8 = adapter.Project(Reverse(Valid(
                    "dev.combined",
                    Enum.GetValues(typeof(LayoutPressureKind))
                        .Cast<LayoutPressureKind>(),
                    Domain3(),
                    new[] { Edge(0, 0, 0, 1), Edge(0, 1, 1, 1) },
                    new[]
                    {
                        LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseKind
                            .CountedPlacementCellsUsable,
                        LayoutResiliencePredicateClauseKind
                            .EffectiveEyeAnchorUsable,
                        LayoutResiliencePredicateClauseKind
                            .EyeToCountedCoreStructurallyConnected
                    })));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
            AddScenario(scenarios, "P2-08", case8,
                AuthoredLayoutPressureSourceStatus.Complete, true);
            Add(checks, "fixture.08.order-culture-repeat", case4.CanonicalSignature,
                case8.CanonicalSignature,
                case4.CanonicalSignature == case8.CanonicalSignature &&
                adapter.Project(Valid(
                    "dev.combined",
                    Enum.GetValues(typeof(LayoutPressureKind))
                        .Cast<LayoutPressureKind>(),
                    Domain3(),
                    new[] { Edge(0, 0, 0, 1), Edge(0, 1, 1, 1) },
                    new[]
                    {
                        LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                        LayoutResiliencePredicateClauseKind
                            .CountedPlacementCellsUsable,
                        LayoutResiliencePredicateClauseKind
                            .EffectiveEyeAnchorUsable,
                        LayoutResiliencePredicateClauseKind
                            .EyeToCountedCoreStructurallyConnected
                    })).CanonicalSignature == case4.CanonicalSignature);

            AddScenario(scenarios, "P2-09", adapter.Project(null),
                AuthoredLayoutPressureSourceStatus.Unknown, false);
            AddScenario(scenarios, "P2-10", adapter.Project(new InputBuilder
            {
                Completeness = AuthoredLayoutPressureAuthoringCompleteness.Incomplete,
                PressureInputId = null
            }.Build()), AuthoredLayoutPressureSourceStatus.Unknown, false);
            AddScenario(scenarios, "P2-11", adapter.Project(new InputBuilder
            {
                Completeness = AuthoredLayoutPressureAuthoringCompleteness.Incomplete,
                Domain = null,
                Eye = null,
                Connections = null,
                Clauses = null
            }.Build()), AuthoredLayoutPressureSourceStatus.Unknown, false);
            AddScenario(scenarios, "P2-12", adapter.Project(new InputBuilder
            {
                Usable = null
            }.Build()), AuthoredLayoutPressureSourceStatus.Unknown, false);
            AuthoredLayoutPressureSourceResult case13 = adapter.Project(new InputBuilder
            {
                Kinds = Array.Empty<LayoutPressureKind>(),
                Domain = Array.Empty<LayoutCellCoordinate>(),
                Usable = Array.Empty<LayoutCellCoordinate>(),
                Eye = null,
                Clauses = Array.Empty<LayoutResiliencePredicateClauseKind>()
            }.Build());
            AddScenario(scenarios, "P2-13", case13,
                AuthoredLayoutPressureSourceStatus.Unknown, false);
            Add(checks, "fixture.13.missing-domain-cell", "Unknown",
                adapter.Project(new InputBuilder
                {
                    Domain = Domain3().Where(cell => !(cell.X == 2 && cell.Y == 2))
                }.Build()).Status.ToString(),
                adapter.Project(new InputBuilder
                {
                    Domain = Domain3().Where(cell => !(cell.X == 2 && cell.Y == 2))
                }.Build()).Status == AuthoredLayoutPressureSourceStatus.Unknown);

            List<LayoutCellCoordinate> duplicateDomain = Domain3().ToList();
            duplicateDomain.Add(Cell(0, 0));
            AddScenario(scenarios, "P2-14", adapter.Project(new InputBuilder
            {
                Kinds = new[]
                {
                    (LayoutPressureKind)0,
                    LayoutPressureKind.PollutedCellMask,
                    LayoutPressureKind.PollutedCellMask
                },
                Domain = duplicateDomain,
                Clauses = new[]
                {
                    (LayoutResiliencePredicateClauseKind)0,
                    LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                    LayoutResiliencePredicateClauseKind.CountedLayoutPresent
                }
            }.Build()), AuthoredLayoutPressureSourceStatus.Invalid, false);
            AddScenario(scenarios, "P2-15", adapter.Project(new InputBuilder
            {
                BoardSize = 0,
                Usable = new[] { Cell(9, 9) },
                Eye = Cell(9, 9)
            }.Build()), AuthoredLayoutPressureSourceStatus.Invalid, false);
            AddScenario(scenarios, "P2-16", adapter.Project(new InputBuilder
            {
                Usable = new[] { Cell(0, 0), Cell(1, 0) },
                Connections = new[]
                {
                    Edge(0, 0, 0, 0),
                    Edge(0, 0, 1, 0),
                    Edge(1, 0, 0, 0),
                    Edge(0, 0, 2, 2)
                }
            }.Build()), AuthoredLayoutPressureSourceStatus.Invalid, false);

            foreach (Scenario scenario in scenarios)
            {
                Add(checks, "scenario." + scenario.Id + ".status",
                    scenario.Status.ToString(), scenario.Result.Status.ToString(),
                    scenario.Result.Status == scenario.Status);
                Add(checks, "scenario." + scenario.Id + ".completeness",
                    scenario.HasSnapshot ? "Complete" : "Incomplete",
                    scenario.Result.PressureFactsCompleteness.ToString(),
                    scenario.Result.PressureFactsCompleteness ==
                        (scenario.HasSnapshot
                            ? LayoutResilienceInputCompleteness.Complete
                            : LayoutResilienceInputCompleteness.Incomplete));
                Add(checks, "scenario." + scenario.Id + ".snapshot",
                    scenario.HasSnapshot ? "present" : "null",
                    scenario.Result.PressureSnapshot == null ? "null" : "present",
                    (scenario.Result.PressureSnapshot != null) ==
                        scenario.HasSnapshot);
                Add(checks, "scenario." + scenario.Id + ".signature", "valid",
                    scenario.Result.CanonicalSignature,
                    IsSignature(scenario.Result.CanonicalSignature));
            }
            Add(checks, "fixture.count", "16", scenarios.Count.ToString(
                CultureInfo.InvariantCulture), scenarios.Count == 16);
            Add(checks, "fixture.complete-count", "8", scenarios.Count(value =>
                value.Status == AuthoredLayoutPressureSourceStatus.Complete).ToString(
                    CultureInfo.InvariantCulture), scenarios.Count(value =>
                value.Status == AuthoredLayoutPressureSourceStatus.Complete) == 8);
            Add(checks, "fixture.unknown-count", "5", scenarios.Count(value =>
                value.Status == AuthoredLayoutPressureSourceStatus.Unknown).ToString(
                    CultureInfo.InvariantCulture), scenarios.Count(value =>
                value.Status == AuthoredLayoutPressureSourceStatus.Unknown) == 5);
            Add(checks, "fixture.invalid-count", "3", scenarios.Count(value =>
                value.Status == AuthoredLayoutPressureSourceStatus.Invalid).ToString(
                    CultureInfo.InvariantCulture), scenarios.Count(value =>
                value.Status == AuthoredLayoutPressureSourceStatus.Invalid) == 3);

            AuthoredLayoutPressureSourceResult priority = adapter.Project(
                new InputBuilder
                {
                    Completeness = AuthoredLayoutPressureAuthoringCompleteness.Incomplete,
                    PressureInputId = null,
                    BoardSize = -1,
                    Domain = null
                }.Build());
            Add(checks, "precedence.invalid-over-unknown", "Invalid",
                priority.Status.ToString(),
                priority.Status == AuthoredLayoutPressureSourceStatus.Invalid);

            VerifyN01CValidatorOnly(checks, scenarios);
            VerifyCanonicalSensitivity(checks, adapter, case1);
            VerifyImmutability(checks, adapter);
            VerifyRepositoryBoundary(checks);

            string observedSignature = case4.CanonicalSignature;
            Add(checks, "canonical.fixed", ExpectedSignature, observedSignature,
                string.Equals(ExpectedSignature, observedSignature,
                    StringComparison.Ordinal));
            int failed = checks.Count(value => !value.Passed);
            string message = "AuthoredLayoutPressureSourceAdapter verifier " +
                (failed == 0 ? "PASS" : "FAIL") + " " +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) + "/" +
                checks.Count.ToString(CultureInfo.InvariantCulture) +
                "; fixtures=16/16; signature=" + observedSignature +
                (failed == 0 ? string.Empty : "; failed=" + string.Join(",", checks
                    .Where(value => !value.Passed).Select(value => value.Id)));
            return new VerificationSummary(checks.Count, failed, observedSignature,
                message);
        }

        private static void VerifyN01CValidatorOnly(
            ICollection<Check> checks,
            IEnumerable<Scenario> scenarios)
        {
            foreach (Scenario scenario in scenarios.Where(value => value.HasSnapshot))
            {
                LayoutResilienceEvaluationInput input =
                    new LayoutResilienceEvaluationInput(
                        "dev.authored-pressure.verify",
                        TalismanBag.EnemySystem.RequirementChannel
                            .EnemyRequirementApplicabilityState.Unknown,
                        LayoutResilienceInputCompleteness.Incomplete,
                        LayoutResilienceInputCompleteness.Complete,
                        null,
                        scenario.Result.PressureSnapshot);
                int count = DefaultLayoutResilienceStructuralPredicateValidator.Instance
                    .Validate(input).Count;
                Add(checks, "n01c.validator." + scenario.Id, "0",
                    count.ToString(CultureInfo.InvariantCulture), count == 0);
            }
        }

        private static void VerifyCanonicalSensitivity(
            ICollection<Check> checks,
            IAuthoredLayoutPressureSourceAdapter adapter,
            AuthoredLayoutPressureSourceResult ignored)
        {
            _ = ignored;
            AuthoredLayoutPressureSourceResult baseline =
                adapter.Project(new InputBuilder().Build());
            AuthoredLayoutPressureSourceInput[] mutations =
            {
                new InputBuilder { PressureInputId = "dev.changed" }.Build(),
                new InputBuilder { BoardSize = 2, Domain = Domain2(), Usable = Domain2(), Eye = Cell(1, 1) }.Build(),
                new InputBuilder { Kinds = new[] { LayoutPressureKind.StructuralConnectionCut } }.Build(),
                new InputBuilder { Usable = Domain3().Where(cell => cell.X != 2) }.Build(),
                new InputBuilder { Eye = Cell(2, 2) }.Build(),
                new InputBuilder { Connections = new[] { Edge(0, 0, 1, 0) } }.Build(),
                new InputBuilder { Clauses = new[] { LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable } }.Build(),
                new InputBuilder { Completeness = AuthoredLayoutPressureAuthoringCompleteness.Incomplete }.Build(),
                new InputBuilder { Usable = null }.Build()
            };
            int changed = mutations.Count(value => adapter.Project(value)
                .CanonicalSignature != baseline.CanonicalSignature);
            Add(checks, "canonical.field-presence-status-output-sensitive", "9",
                changed.ToString(CultureInfo.InvariantCulture), changed == 9);
        }

        private static void VerifyImmutability(
            ICollection<Check> checks,
            IAuthoredLayoutPressureSourceAdapter adapter)
        {
            List<LayoutPressureKind> kinds = new List<LayoutPressureKind>
                { LayoutPressureKind.PollutedCellMask };
            List<LayoutCellCoordinate> domain = Domain3().ToList();
            List<LayoutCellCoordinate> usable = Domain3().ToList();
            List<LayoutCellConnection> edges = new List<LayoutCellConnection>();
            List<LayoutResiliencePredicateClauseKind> clauses =
                new List<LayoutResiliencePredicateClauseKind>
                { LayoutResiliencePredicateClauseKind.CountedLayoutPresent };
            AuthoredLayoutPressureSourceInput input = new InputBuilder
            {
                Kinds = kinds,
                Domain = domain,
                Usable = usable,
                Connections = edges,
                Clauses = clauses
            }.Build();
            AuthoredLayoutPressureSourceResult result = adapter.Project(input);
            string signature = result.CanonicalSignature;
            kinds.Clear();
            domain.Clear();
            usable.Clear();
            edges.Add(Edge(0, 0, 1, 0));
            clauses.Clear();
            Add(checks, "immutability.source-defensive-copy", signature,
                result.CanonicalSignature,
                signature == result.CanonicalSignature &&
                result.PressureSnapshot.LayoutDomainCells.Count == 9);
            AssertReadOnly(checks, "input.kinds", input.PressureKinds);
            AssertReadOnly(checks, "output.kinds", result.PressureSnapshot.PressureKinds);
            AssertReadOnly(checks, "output.domain",
                result.PressureSnapshot.LayoutDomainCells);
            AssertReadOnly(checks, "result.issues", result.Issues);
        }

        private static void VerifyRepositoryBoundary(ICollection<Check> checks)
        {
            string root = FindProjectRoot();
            Add(checks, "allowlist.files", "14",
                OutputPaths.Count(path => File.Exists(Absolute(root, path))).ToString(
                    CultureInfo.InvariantCulture),
                OutputPaths.All(path => File.Exists(Absolute(root, path))));
            string runtime = string.Join("\n", Directory.GetFiles(
                    Absolute(root, RuntimeDirectory), "*.cs", SearchOption.TopDirectoryOnly)
                .Select(File.ReadAllText));
            string adapterSource = File.ReadAllText(Absolute(root,
                RuntimeDirectory + "/AuthoredLayoutPressureSourceAdapter.cs"));
            Add(checks, "isolation.evaluator-type", "0",
                Count(runtime, "DefaultLayoutResilienceStructuralPredicateEvaluator")
                    .ToString(CultureInfo.InvariantCulture),
                Count(runtime, "DefaultLayoutResilienceStructuralPredicateEvaluator") == 0);
            Add(checks, "isolation.evaluate-call", "0",
                Count(runtime, ".Evaluate(").ToString(CultureInfo.InvariantCulture),
                Count(runtime, ".Evaluate(") == 0);
            Add(checks, "isolation.validator-call", "1",
                Count(adapterSource,
                    "DefaultLayoutResilienceStructuralPredicateValidator.Instance")
                    .ToString(CultureInfo.InvariantCulture),
                Count(adapterSource,
                    "DefaultLayoutResilienceStructuralPredicateValidator.Instance") == 1);
            string[] forbidden =
            {
                "ItemFactProjection", "TalismanBag.Items", "MapRule", "Encounter",
                "Readiness", "MonoBehaviour", "GameObject", "RectTransform",
                "UnityEngine", "BuildCapability", "Pathfinding", "ReadinessBand"
            };
            int leaks = forbidden.Sum(token => Count(runtime, token));
            Add(checks, "isolation.forbidden-runtime-tokens", "0",
                leaks.ToString(CultureInfo.InvariantCulture), leaks == 0);
            PropertyInfo[] publicProperties = typeof(AuthoredLayoutPressureSourceInput)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Concat(typeof(AuthoredLayoutPressureSourceResult).GetProperties(
                    BindingFlags.Public | BindingFlags.Instance)).ToArray();
            int applicability = publicProperties.Count(value =>
                value.Name.IndexOf("Applicability", StringComparison.Ordinal) >= 0 ||
                value.PropertyType.FullName.IndexOf("Applicability",
                    StringComparison.Ordinal) >= 0);
            Add(checks, "isolation.applicability-public-io", "0",
                applicability.ToString(CultureInfo.InvariantCulture),
                applicability == 0);
            string report = File.ReadAllText(Absolute(root,
                ReportDirectory + "/AuthoredLayoutPressureSourceAdapterReport.md"));
            Add(checks, "report.c02-blocked", "present",
                report.Contains("32/32") ? "present" : "absent",
                report.Contains("32/32"));
            Add(checks, "report.unknown-reduction", "present",
                report.Contains("Actual Unknown reduction: `0`")
                    ? "present" : "absent",
                report.Contains("Actual Unknown reduction: `0`"));
        }

        private static void AssertPublicSurface(ICollection<Check> checks)
        {
            AssertProperties(checks, "input", typeof(AuthoredLayoutPressureSourceInput),
                "SchemaId", "SchemaVersion", "AuthoringCompleteness",
                "PressureInputId", "DeclaredBoardSize", "PressureKinds",
                "LayoutDomainCells", "UsableCellsAfterPressure",
                "EffectiveEyeAnchorCellAfterPressure",
                "PreservedStructuralConnectionsAfterPressure",
                "RequiredPredicateClauses");
            AssertProperties(checks, "result", typeof(AuthoredLayoutPressureSourceResult),
                "SchemaId", "SchemaVersion", "Status",
                "PressureFactsCompleteness", "PressureSnapshot", "Issues",
                "CanonicalSignature");
        }

        private static AuthoredLayoutPressureSourceInput Valid(
            string id,
            IEnumerable<LayoutPressureKind> kinds,
            IEnumerable<LayoutCellCoordinate> usable,
            IEnumerable<LayoutCellConnection> edges,
            IEnumerable<LayoutResiliencePredicateClauseKind> clauses,
            LayoutCellCoordinate eye = null)
        {
            return new InputBuilder
            {
                PressureInputId = id,
                Kinds = kinds,
                Usable = usable,
                Connections = edges,
                Clauses = clauses,
                Eye = eye ?? Cell(1, 1)
            }.Build();
        }

        private static AuthoredLayoutPressureSourceInput Reverse(
            AuthoredLayoutPressureSourceInput value)
        {
            return new AuthoredLayoutPressureSourceInput(
                value.AuthoringCompleteness,
                value.PressureInputId,
                value.DeclaredBoardSize,
                value.PressureKinds.Reverse(),
                value.LayoutDomainCells.Reverse(),
                value.UsableCellsAfterPressure.Reverse(),
                value.EffectiveEyeAnchorCellAfterPressure,
                value.PreservedStructuralConnectionsAfterPressure.Reverse(),
                value.RequiredPredicateClauses.Reverse(),
                value.SchemaId,
                value.SchemaVersion);
        }

        private static IEnumerable<LayoutCellCoordinate> Domain3()
        {
            return Domain(3);
        }

        private static IEnumerable<LayoutCellCoordinate> Domain2()
        {
            return Domain(2);
        }

        private static IEnumerable<LayoutCellCoordinate> Domain(int size)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    yield return Cell(x, y);
                }
            }
        }

        private static LayoutCellCoordinate Cell(int x, int y)
        {
            return new LayoutCellCoordinate(x, y);
        }

        private static LayoutCellConnection Edge(int ax, int ay, int bx, int by)
        {
            return new LayoutCellConnection(Cell(ax, ay), Cell(bx, by));
        }

        private static void AddScenario(
            ICollection<Scenario> scenarios,
            string id,
            AuthoredLayoutPressureSourceResult result,
            AuthoredLayoutPressureSourceStatus status,
            bool hasSnapshot)
        {
            scenarios.Add(new Scenario(id, result, status, hasSnapshot));
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
            Add(checks, "enum." + id + ".zero-absent", "false",
                Enum.IsDefined(typeof(T), 0).ToString().ToLowerInvariant(),
                !Enum.IsDefined(typeof(T), 0));
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            params string[] expected)
        {
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);
            string[] actual = properties.Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] sorted = expected.OrderBy(value => value,
                StringComparer.Ordinal).ToArray();
            Add(checks, "properties." + id + ".exact", string.Join("|", sorted),
                string.Join("|", actual), sorted.SequenceEqual(actual));
            Add(checks, "properties." + id + ".readonly", "0",
                properties.Count(value => value.SetMethod != null).ToString(
                    CultureInfo.InvariantCulture),
                properties.All(value => value.SetMethod == null));
        }

        private static void AssertReadOnly<T>(
            ICollection<Check> checks,
            string id,
            IReadOnlyList<T> values)
        {
            bool threw = false;
            try
            {
                ((IList)values).Add(default(T));
            }
            catch (NotSupportedException)
            {
                threw = true;
            }
            Add(checks, "readonly." + id, "true", threw.ToString()
                .ToLowerInvariant(), threw);
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Substring(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static int Count(string source, string token)
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

        private static string FindProjectRoot()
        {
            foreach (string seed in new[]
            {
                Directory.GetCurrentDirectory(), AppContext.BaseDirectory
            })
            {
                DirectoryInfo current = new DirectoryInfo(seed);
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
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string path)
        {
            return Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar));
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

        private sealed class InputBuilder
        {
            public string SchemaId = AuthoredLayoutPressureSourceSchema.SchemaId;
            public int SchemaVersion = AuthoredLayoutPressureSourceSchema.SchemaVersion;
            public AuthoredLayoutPressureAuthoringCompleteness Completeness =
                AuthoredLayoutPressureAuthoringCompleteness.Complete;
            public string PressureInputId = "dev.pressure";
            public int? BoardSize = 3;
            public IEnumerable<LayoutPressureKind> Kinds =
                new[] { LayoutPressureKind.PollutedCellMask };
            public IEnumerable<LayoutCellCoordinate> Domain = Domain3();
            public IEnumerable<LayoutCellCoordinate> Usable = Domain3();
            public LayoutCellCoordinate Eye = Cell(1, 1);
            public IEnumerable<LayoutCellConnection> Connections =
                Array.Empty<LayoutCellConnection>();
            public IEnumerable<LayoutResiliencePredicateClauseKind> Clauses =
                new[] { LayoutResiliencePredicateClauseKind.CountedLayoutPresent };

            public AuthoredLayoutPressureSourceInput Build()
            {
                return new AuthoredLayoutPressureSourceInput(
                    Completeness, PressureInputId, BoardSize, Kinds, Domain, Usable,
                    Eye, Connections, Clauses, SchemaId, SchemaVersion);
            }
        }

        private readonly struct Check
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

        private sealed class Scenario
        {
            public Scenario(
                string id,
                AuthoredLayoutPressureSourceResult result,
                AuthoredLayoutPressureSourceStatus status,
                bool hasSnapshot)
            {
                Id = id;
                Result = result;
                Status = status;
                HasSnapshot = hasSnapshot;
            }

            public string Id { get; }
            public AuthoredLayoutPressureSourceResult Result { get; }
            public AuthoredLayoutPressureSourceStatus Status { get; }
            public bool HasSnapshot { get; }
        }

        private readonly struct VerificationSummary
        {
            public VerificationSummary(
                int total,
                int failed,
                string signature,
                string message)
            {
                Total = total;
                Failed = failed;
                Signature = signature;
                Message = message;
            }

            public int Total { get; }
            public int Failed { get; }
            public string Signature { get; }
            public string Message { get; }
        }
    }
}
