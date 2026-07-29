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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class DevEncounterLayoutPressureAuthoringVerifier
    {
        private const string ExpectedSignature =
            "sha256:8b9ee4674020c5405d7b90cf2793b37be32bf0ea08e8a981291ed6790309da54";
        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring";
        private const string ReportDirectory = "Docs/V0.4/Reports";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory + "/DevEncounterLayoutPressureAuthoringPrimitives.cs",
            RuntimeDirectory + "/DevEncounterLayoutPressureAuthoringPrimitives.cs.meta",
            RuntimeDirectory + "/DevEncounterLayoutPressureCatalog.cs",
            RuntimeDirectory + "/DevEncounterLayoutPressureCatalog.cs.meta",
            RuntimeDirectory + "/DevEncounterLayoutPressureValidation.cs",
            RuntimeDirectory + "/DevEncounterLayoutPressureValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/DevEncounterLayoutPressureAuthoringVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/DevEncounterLayoutPressureAuthoringVerifier.cs.meta",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringReport.md",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringRows.csv",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringCells.csv",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringConnections.csv",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringMaps.md",
            ReportDirectory + "/DevEncounterLayoutPressureAuthoringLeakCheckReport.md"
        };

        private static readonly string[] P5SurveyFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementMigrationSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementMigrationSurveyVerifier.cs.meta",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyReport.md",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyCandidateRows.csv",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyIdentityCollisionMatrix.csv",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyPressureFactGap.csv",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyContextMatrix.csv",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyUserDecisionSheet.csv",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Dev Encounter Layout Pressure Authoring")]
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
                    "DEV_ENCOUNTER_LAYOUT_PRESSURE_AUTHORING PASS");
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
            string protectedBefore = CaptureProtected(root);
            List<Check> checks = new List<Check>();
            DevEncounterLayoutPressureAuthoringSource source =
                DevEncounterLayoutPressureCatalog.CreateCandidateSource();
            DevEncounterLayoutPressureAuthoringResult result =
                DevEncounterLayoutPressureValidation.ValidateAndProject(source);

            VerifyContract(checks, source, result);
            VerifyContexts(checks, source, result);
            VerifyDeterminismAndImmutability(checks, source, result);
            VerifyReports(checks, root, result);
            VerifyRepositoryBoundary(checks, root, result);
            VerifyProtected(checks, root, protectedBefore);

            int failed = checks.Count(value => !value.Passed);
            string failures = string.Join("; ", checks.Where(value => !value.Passed)
                .Select(value => value.Id + " expected=[" + value.Expected +
                    "] actual=[" + value.Actual + "]"));
            string message = "DevEncounterLayoutPressureAuthoring verifier " +
                (failed == 0 ? "PASS " : "FAIL ") +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) + "/" +
                checks.Count.ToString(CultureInfo.InvariantCulture) +
                "; contexts=4/4; cells=100; edges=126; maps=4; signature=" +
                result.CanonicalSignature +
                (failed == 0 ? string.Empty : "; failures=" + failures);
            return new Summary(failed, message);
        }

        private static void VerifyContract(
            ICollection<Check> checks,
            DevEncounterLayoutPressureAuthoringSource source,
            DevEncounterLayoutPressureAuthoringResult result)
        {
            AssertEnum<DevEncounterLayoutPressureAuthoringStatus>(checks, "status",
                new[] { "CandidateComplete", "Unknown", "Invalid" },
                new[] { 1, 2, 3 });
            Add(checks, "schema.id", "DevEncounterLayoutPressureAuthoring.v1",
                DevEncounterLayoutPressureAuthoringSchema.SchemaId,
                DevEncounterLayoutPressureAuthoringSchema.SchemaId ==
                    "DevEncounterLayoutPressureAuthoring.v1");
            Add(checks, "schema.version", "1",
                DevEncounterLayoutPressureAuthoringSchema.SchemaVersion.ToString(
                    CultureInfo.InvariantCulture),
                DevEncounterLayoutPressureAuthoringSchema.SchemaVersion == 1);
            AssertPublicSurface(checks);
            Add(checks, "result.status", "CandidateComplete", result.Status.ToString(),
                result.Status == DevEncounterLayoutPressureAuthoringStatus.CandidateComplete);
            Add(checks, "result.flags", "true/false",
                Lower(result.DevOnly) + "/" + Lower(result.IsEnabled),
                result.DevOnly && !result.IsEnabled);
            Add(checks, "result.issues", "0", result.Issues.Count.ToString(
                CultureInfo.InvariantCulture), result.Issues.Count == 0);
            Add(checks, "canonical.format", "sha256 lowercase", result.CanonicalSignature,
                IsSignature(result.CanonicalSignature));
            Add(checks, "canonical.fixed", ExpectedSignature, result.CanonicalSignature,
                string.Equals(ExpectedSignature, result.CanonicalSignature,
                    StringComparison.Ordinal));

            DevEncounterLayoutPressureAuthoringResult unknown =
                DevEncounterLayoutPressureValidation.ValidateAndProject(null);
            Add(checks, "status.unknown", "Unknown", unknown.Status.ToString(),
                unknown.Status == DevEncounterLayoutPressureAuthoringStatus.Unknown);
            DevEncounterLayoutPressureAuthoringResult invalid =
                DevEncounterLayoutPressureValidation.ValidateAndProject(
                    new DevEncounterLayoutPressureAuthoringSource("wrong", 1, true,
                        false, source.Rows));
            Add(checks, "status.invalid", "Invalid", invalid.Status.ToString(),
                invalid.Status == DevEncounterLayoutPressureAuthoringStatus.Invalid);
        }

        private static void VerifyContexts(
            ICollection<Check> checks,
            DevEncounterLayoutPressureAuthoringSource source,
            DevEncounterLayoutPressureAuthoringResult result)
        {
            Add(checks, "contexts.source", "4", source.Rows.Count.ToString(
                CultureInfo.InvariantCulture), source.Rows.Count == 4);
            Add(checks, "contexts.result", "4", result.Rows.Count.ToString(
                CultureInfo.InvariantCulture), result.Rows.Count == 4);
            Add(checks, "contexts.candidates", "2", result.Rows.Select(value =>
                    value.CandidateMigrationRequirementId).Distinct(
                        StringComparer.Ordinal).Count().ToString(
                            CultureInfo.InvariantCulture),
                result.Rows.Select(value => value.CandidateMigrationRequirementId)
                    .Distinct(StringComparer.Ordinal).Count() == 2);
            Add(checks, "contexts.pressure-ids", "4", result.Rows.Select(value =>
                    value.PressureInputId).Distinct(StringComparer.Ordinal).Count()
                    .ToString(CultureInfo.InvariantCulture),
                result.Rows.Select(value => value.PressureInputId)
                    .Distinct(StringComparer.Ordinal).Count() == 4);
            Add(checks, "contexts.identity", "4", result.Rows.Select(ContextIdentity)
                    .Distinct(StringComparer.Ordinal).Count().ToString(
                        CultureInfo.InvariantCulture),
                result.Rows.Select(ContextIdentity).Distinct(StringComparer.Ordinal)
                    .Count() == 4);
            Add(checks, "contexts.p2-complete", "4", result.Rows.Count(value =>
                    value.P2Status == AuthoredLayoutPressureSourceStatus.Complete &&
                    value.P2Completeness == LayoutResilienceInputCompleteness.Complete &&
                    value.PressureSnapshot != null).ToString(
                        CultureInfo.InvariantCulture),
                result.Rows.All(value =>
                    value.P2Status == AuthoredLayoutPressureSourceStatus.Complete &&
                    value.P2Completeness == LayoutResilienceInputCompleteness.Complete &&
                    value.PressureSnapshot != null));
            Add(checks, "contexts.flags", "4/4 isolated", result.Rows.Count(value =>
                    value.DevOnly && !value.IsEnabled && !value.UserCoordinateAccepted &&
                    !value.Activated).ToString(CultureInfo.InvariantCulture),
                result.Rows.All(value => value.DevOnly && !value.IsEnabled &&
                    !value.UserCoordinateAccepted && !value.Activated));

            int[] usable = result.Rows.Select(value =>
                value.PressureSnapshot.UsableCellsAfterPressure.Count).ToArray();
            int[] edges = result.Rows.Select(value =>
                value.PressureSnapshot.PreservedStructuralConnectionsAfterPressure.Count)
                .ToArray();
            Add(checks, "counts.domains", "25/25/25/25",
                string.Join("/", result.Rows.Select(value =>
                    value.PressureSnapshot.LayoutDomainCells.Count)),
                result.Rows.All(value =>
                    value.PressureSnapshot.LayoutDomainCells.Count == 25));
            Add(checks, "counts.usable", "25/25/23/21", string.Join("/", usable),
                usable.SequenceEqual(new[] { 25, 25, 23, 21 }));
            Add(checks, "counts.edges", "35/35/32/24", string.Join("/", edges),
                edges.SequenceEqual(new[] { 35, 35, 32, 24 }));
            Add(checks, "counts.cells-total", "100", result.Rows.Sum(value =>
                    value.PressureSnapshot.LayoutDomainCells.Count).ToString(
                        CultureInfo.InvariantCulture),
                result.Rows.Sum(value =>
                    value.PressureSnapshot.LayoutDomainCells.Count) == 100);
            Add(checks, "counts.edges-total", "126", edges.Sum().ToString(
                CultureInfo.InvariantCulture), edges.Sum() == 126);
            Add(checks, "counts.clauses", "3/3/3/3", string.Join("/",
                    result.Rows.Select(value =>
                        value.PressureSnapshot.RequiredPredicateClauses.Count)),
                result.Rows.All(value =>
                    value.PressureSnapshot.RequiredPredicateClauses.Count == 3));
            Add(checks, "eyes", "2:1/3:2/2:2/2:2", string.Join("/",
                    result.Rows.Select(value => Cell(value.PressureSnapshot
                        .EffectiveEyeAnchorCellAfterPressure))),
                result.Rows.Select(value => Cell(value.PressureSnapshot
                    .EffectiveEyeAnchorCellAfterPressure)).SequenceEqual(
                        new[] { "2:1", "3:2", "2:2", "2:2" }));
        }

        private static void VerifyDeterminismAndImmutability(
            ICollection<Check> checks,
            DevEncounterLayoutPressureAuthoringSource source,
            DevEncounterLayoutPressureAuthoringResult result)
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            DevEncounterLayoutPressureAuthoringResult reversed;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                reversed = DevEncounterLayoutPressureValidation.ValidateAndProject(
                    Reverse(source));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
            Add(checks, "determinism.reverse-culture", result.CanonicalSignature,
                reversed.CanonicalSignature,
                result.CanonicalSignature == reversed.CanonicalSignature);
            Add(checks, "determinism.repeat", result.CanonicalSignature,
                DevEncounterLayoutPressureValidation.ValidateAndProject(
                    DevEncounterLayoutPressureCatalog.CreateCandidateSource())
                    .CanonicalSignature,
                result.CanonicalSignature == DevEncounterLayoutPressureValidation
                    .ValidateAndProject(
                        DevEncounterLayoutPressureCatalog.CreateCandidateSource())
                    .CanonicalSignature);
            DevEncounterLayoutPressureAuthoringSource second =
                DevEncounterLayoutPressureCatalog.CreateCandidateSource();
            Add(checks, "defensive.source-copy", "distinct", "checked",
                !ReferenceEquals(source, second) &&
                !ReferenceEquals(source.Rows, second.Rows) &&
                !ReferenceEquals(source.Rows[0].PressureSource,
                    second.Rows[0].PressureSource) &&
                !ReferenceEquals(source.Rows[0].PressureSource.LayoutDomainCells[0],
                    second.Rows[0].PressureSource.LayoutDomainCells[0]));
            AssertReadOnly(checks, "source.rows", source.Rows);
            AssertReadOnly(checks, "source.domain",
                source.Rows[0].PressureSource.LayoutDomainCells);
            AssertReadOnly(checks, "result.rows", result.Rows);
            AssertReadOnly(checks, "result.issues", result.Issues);
            AssertReadOnly(checks, "result.edges", result.Rows[0].PressureSnapshot
                .PreservedStructuralConnectionsAfterPressure);
        }

        private static void VerifyReports(
            ICollection<Check> checks,
            string root,
            DevEncounterLayoutPressureAuthoringResult result)
        {
            string rows = Normalize(Read(root,
                ReportDirectory + "/DevEncounterLayoutPressureAuthoringRows.csv"));
            string cells = Normalize(Read(root,
                ReportDirectory + "/DevEncounterLayoutPressureAuthoringCells.csv"));
            string connections = Normalize(Read(root,
                ReportDirectory + "/DevEncounterLayoutPressureAuthoringConnections.csv"));
            string maps = Normalize(Read(root,
                ReportDirectory + "/DevEncounterLayoutPressureAuthoringMaps.md"));
            string main = Read(root,
                ReportDirectory + "/DevEncounterLayoutPressureAuthoringReport.md");
            string leak = Read(root,
                ReportDirectory + "/DevEncounterLayoutPressureAuthoringLeakCheckReport.md");
            Add(checks, "report.rows", "data-consistent", "checked",
                rows == BuildRows(result));
            Add(checks, "report.cells", "100 data-consistent", "checked",
                cells == BuildCells(result));
            Add(checks, "report.connections", "126 data-consistent", "checked",
                connections == BuildConnections(result));
            Add(checks, "report.maps", "4 Chinese readable maps", "checked",
                maps == BuildMaps(result));
            Add(checks, "report.main.signature", result.CanonicalSignature, main,
                Has(main, result.CanonicalSignature));
            Add(checks, "report.main.receipts", "three receipts", main,
                Has(main, "GUARD_PASS_ASSIGNMENT_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01_REVISION01") &&
                Has(main, "ENEMY_GUARD_CONFIRM_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01") &&
                Has(main, "CAPABILITY_ALGORITHM_GUARD_PASS_DEVENCOUNTERLAYOUTPRESSUREAUTHORING01"));
            Add(checks, "report.main.boundary", "isolated", main,
                Has(main, "userCoordinateAccepted = false") &&
                Has(main, "activated = false") && Has(main, "devOnly = true") &&
                Has(main, "isEnabled = false") && Has(main, "32/32") &&
                Has(main, "Unknown reduction: `0`"));
            Add(checks, "report.leak", "Leak Count: `0`", leak,
                Has(leak, "Leak Count: `0`") && Has(leak, "Next package: `NOT_STARTED`") &&
                Has(leak, "Existing files modified: `0`") &&
                Has(leak, "Direct N01C Validator/Evaluator calls: `0 / 0`"));
        }

        private static void VerifyRepositoryBoundary(
            ICollection<Check> checks,
            string root,
            DevEncounterLayoutPressureAuthoringResult result)
        {
            Add(checks, "allowlist.count", "15", OutputPaths.Length.ToString(
                CultureInfo.InvariantCulture), OutputPaths.Length == 15);
            Add(checks, "allowlist.exists", "15", OutputPaths.Count(value =>
                    File.Exists(Absolute(root, value))).ToString(
                        CultureInfo.InvariantCulture),
                OutputPaths.All(value => File.Exists(Absolute(root, value))));
            string catalog = Read(root,
                RuntimeDirectory + "/DevEncounterLayoutPressureCatalog.cs");
            string validation = Read(root,
                RuntimeDirectory + "/DevEncounterLayoutPressureValidation.cs");
            string runtime = string.Join("\n", Directory.GetFiles(
                    Absolute(root, RuntimeDirectory), "*.cs", SearchOption.TopDirectoryOnly)
                .Select(File.ReadAllText));
            string[] generationTokens =
            {
                "for (", "foreach (", "Enumerable.Range", ".Where(",
                "GetRange(", "GenerateEdges", "GenerateDomain", "FillRectangle"
            };
            int generators = generationTokens.Sum(value => Count(catalog, value));
            Add(checks, "catalog.programmatic-generation", "0", generators.ToString(
                CultureInfo.InvariantCulture), generators == 0);
            Add(checks, "p2.project-call-site", "1", Count(validation,
                    "DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(")
                    .ToString(CultureInfo.InvariantCulture),
                Count(validation,
                    "DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(") == 1);
            int n01cValidator = Count(runtime,
                "DefaultLayoutResilienceStructuralPredicateValidator");
            int n01cEvaluator = Count(runtime,
                "DefaultLayoutResilienceStructuralPredicateEvaluator") +
                Count(runtime, ".Evaluate(");
            Add(checks, "direct.n01c-validator", "0", n01cValidator.ToString(
                CultureInfo.InvariantCulture), n01cValidator == 0);
            Add(checks, "direct.n01c-evaluator", "0", n01cEvaluator.ToString(
                CultureInfo.InvariantCulture), n01cEvaluator == 0);
            string[] forbidden =
            {
                "EvaluationInputAssembly", "StructuralReadinessConsumer",
                "EnemyRequirementApplicability", "MonoBehaviour", "ScriptableObject",
                "UnityEngine", "GameObject", "SceneManager", "RunFlow", "SaveData",
                "RewardConfig", "BuildSettings"
            };
            int leaks = forbidden.Sum(value => Count(runtime, value));
            Add(checks, "runtime.forbidden-tokens", "0", leaks.ToString(
                CultureInfo.InvariantCulture), leaks == 0);
            Add(checks, "behavior.migration-p3-p4-readiness", "0", "0",
                !Has(runtime, "MigrationCreated") && !Has(runtime, "P3") &&
                !Has(runtime, "P4") && !Has(runtime, "ReadinessAggregation"));
            int trailing = OutputPaths.Sum(path => TrailingWhitespaceCount(
                Absolute(root, path)));
            Add(checks, "whitespace.trailing", "0", trailing.ToString(
                CultureInfo.InvariantCulture), trailing == 0);
            string[] guids =
            {
                "6d014e9a31be4a7da81144ff11100001",
                "6d024e9a31be4a7da81144ff11100002",
                "6d034e9a31be4a7da81144ff11100003",
                "6d044e9a31be4a7da81144ff11100004",
                "6d054e9a31be4a7da81144ff11100005"
            };
            string allMeta = string.Join("\n", Directory.GetFiles(
                    Absolute(root, "Assets"), "*.meta", SearchOption.AllDirectories)
                .Select(File.ReadAllText));
            int guidConflicts = guids.Count(guid => Count(allMeta, "guid: " + guid) != 1);
            Add(checks, "guid.conflicts", "0", guidConflicts.ToString(
                CultureInfo.InvariantCulture), guidConflicts == 0);
            ProcessResult diff = Run(root, "git", "diff --check");
            Add(checks, "git.diff-check", "PASS", diff.Output,
                diff.ExitCode == 0);
            ProcessResult packageStatus = Run(root, "git",
                "status --porcelain=v1 --untracked-files=all -- " +
                string.Join(" ", OutputPaths.Select(Quote)));
            int statusRows = packageStatus.Output.Split(new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries).Length;
            Add(checks, "git.package-new-files", "15", statusRows.ToString(
                CultureInfo.InvariantCulture), packageStatus.ExitCode == 0 &&
                statusRows == 15 && packageStatus.Output.Split(new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries).All(value =>
                        value.StartsWith("?? ", StringComparison.Ordinal)));
            Add(checks, "canonical.output", ExpectedSignature,
                result.CanonicalSignature,
                result.CanonicalSignature == ExpectedSignature);
        }

        private static void VerifyProtected(
            ICollection<Check> checks,
            string root,
            string before)
        {
            AddProtection(checks, root, "p5s", P5SurveyFiles, true, 9,
                "bebd04922410cb83db1690ecd6505d62c58417e151b3b6bc94d7c0774f68a704");
            AddProtection(checks, root, "p2", P2Files(root), true, 14,
                "15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050");
            AddProtection(checks, root, "p3", P3Files(root), true, 14,
                "c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c");
            AddProtection(checks, root, "p4", P4Files(root), true, 14,
                "8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530");
            AddProtection(checks, root, "n01c", N01CFiles(root), true, 16,
                "691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b");
            AddProtection(checks, root, "n01b", N01BFiles(root), true, 14,
                "acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316");
            AddProtection(checks, root, "e10", E10Files(root), true, 21,
                "3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be");
            AddAggregate(checks, "item", AggregateItem(root), 105,
                "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1");
            AddAggregate(checks, "enemy", AggregateEnemy(root), 89,
                "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae");
            AddProtection(checks, root, "scenes", DirectoryFiles(root,
                    "Assets/_Game/Scenes"), true, 14,
                "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b");
            AddProtection(checks, root, "prefabs", DirectoryFiles(root,
                    "Assets/_Game/Prefabs"), true, 16,
                "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087");
            Add(checks, "protected.build-settings",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59",
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false),
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false) ==
                    "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59");
            Add(checks, "protected.assignment",
                "a470d9e6e8c845365fe08eae9b1755b23a1ea8074924baf7dee5bf483c1c52e2",
                FileHash(root,
                    "Docs/V0.4/DevEncounterLayoutPressureAuthoring01_Assignment.md",
                    false), FileHash(root,
                    "Docs/V0.4/DevEncounterLayoutPressureAuthoring01_Assignment.md",
                    false) ==
                    "a470d9e6e8c845365fe08eae9b1755b23a1ea8074924baf7dee5bf483c1c52e2");
            string head = Run(root, "git", "rev-parse HEAD").Output.Trim();
            Add(checks, "protected.head",
                "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba", head,
                head == "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba");
            Add(checks, "protected.before-after", before, CaptureProtected(root),
                before == CaptureProtected(root));
        }

        private static void AssertPublicSurface(ICollection<Check> checks)
        {
            AssertProperties(checks, "source",
                typeof(DevEncounterLayoutPressureAuthoringSource),
                "SchemaId", "SchemaVersion", "DevOnly", "IsEnabled", "Rows");
            AssertProperties(checks, "source-row",
                typeof(DevEncounterLayoutPressureAuthoringSourceRow),
                "CandidateMigrationRequirementId", "OwnerId", "RequirementGroupId",
                "BuildCapabilityKey", "SeedId", "EncounterId", "MapRuleId",
                "PressureInputId", "DevOnly", "IsEnabled", "UserCoordinateAccepted",
                "Activated", "PressureSource");
            AssertProperties(checks, "snapshot",
                typeof(DevEncounterLayoutPressureAuthoringRowSnapshot),
                "CandidateMigrationRequirementId", "OwnerId", "RequirementGroupId",
                "BuildCapabilityKey", "SeedId", "EncounterId", "MapRuleId",
                "PressureInputId", "DevOnly", "IsEnabled", "UserCoordinateAccepted",
                "Activated", "P2Status", "P2Completeness", "PressureSnapshot",
                "P2CanonicalSignature", "P2Issues");
            AssertProperties(checks, "issue",
                typeof(DevEncounterLayoutPressureAuthoringIssue),
                "Code", "Status", "Path", "Message");
            AssertProperties(checks, "result",
                typeof(DevEncounterLayoutPressureAuthoringResult),
                "SchemaId", "SchemaVersion", "Status", "DevOnly", "IsEnabled",
                "Rows", "Issues", "CanonicalSignature");
            MethodInfo[] catalog = typeof(DevEncounterLayoutPressureCatalog).GetMethods(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            Add(checks, "api.catalog", "CreateCandidateSource", string.Join("|",
                    catalog.Select(value => value.Name)),
                catalog.Length == 1 && catalog[0].Name == "CreateCandidateSource" &&
                catalog[0].GetParameters().Length == 0);
            MethodInfo[] validation = typeof(DevEncounterLayoutPressureValidation)
                .GetMethods(BindingFlags.Public | BindingFlags.Static |
                    BindingFlags.DeclaredOnly);
            Add(checks, "api.validation", "ValidateAndProject", string.Join("|",
                    validation.Select(value => value.Name)),
                validation.Length == 1 && validation[0].Name == "ValidateAndProject" &&
                validation[0].GetParameters().Length == 1);
        }

        private static DevEncounterLayoutPressureAuthoringSource Reverse(
            DevEncounterLayoutPressureAuthoringSource source)
        {
            return new DevEncounterLayoutPressureAuthoringSource(source.SchemaId,
                source.SchemaVersion, source.DevOnly, source.IsEnabled,
                source.Rows.Reverse().Select(row =>
                    new DevEncounterLayoutPressureAuthoringSourceRow(
                        row.CandidateMigrationRequirementId, row.OwnerId,
                        row.RequirementGroupId, row.BuildCapabilityKey, row.SeedId,
                        row.EncounterId, row.MapRuleId, row.PressureInputId,
                        row.DevOnly, row.IsEnabled, row.UserCoordinateAccepted,
                        row.Activated, Reverse(row.PressureSource))));
        }

        private static AuthoredLayoutPressureSourceInput Reverse(
            AuthoredLayoutPressureSourceInput value)
        {
            return new AuthoredLayoutPressureSourceInput(value.AuthoringCompleteness,
                value.PressureInputId, value.DeclaredBoardSize,
                value.PressureKinds.Reverse(), value.LayoutDomainCells.Reverse(),
                value.UsableCellsAfterPressure.Reverse(),
                value.EffectiveEyeAnchorCellAfterPressure,
                value.PreservedStructuralConnectionsAfterPressure.Reverse().Select(edge =>
                    new LayoutCellConnection(edge.CellB, edge.CellA)),
                value.RequiredPredicateClauses.Reverse(), value.SchemaId,
                value.SchemaVersion);
        }

        private static string BuildRows(DevEncounterLayoutPressureAuthoringResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("candidateMigrationRequirementId,ownerId,requirementGroupId,buildCapabilityKey,seedId,encounterId,mapRuleId,pressureInputId,pressureKinds,effectiveEye,clauses,p2Status,p2Completeness,devOnly,isEnabled,userCoordinateAccepted,activated\n");
            foreach (DevEncounterLayoutPressureAuthoringRowSnapshot row in result.Rows)
            {
                text.Append(row.CandidateMigrationRequirementId).Append(',')
                    .Append(row.OwnerId).Append(',').Append(row.RequirementGroupId)
                    .Append(',').Append(row.BuildCapabilityKey).Append(',')
                    .Append(row.SeedId).Append(',').Append(row.EncounterId).Append(',')
                    .Append(row.MapRuleId).Append(',').Append(row.PressureInputId)
                    .Append(',').Append(string.Join("|", row.PressureSnapshot.PressureKinds))
                    .Append(',').Append(Cell(row.PressureSnapshot.EffectiveEyeAnchorCellAfterPressure))
                    .Append(',').Append(string.Join("|", row.PressureSnapshot.RequiredPredicateClauses))
                    .Append(',').Append(row.P2Status).Append(',').Append(row.P2Completeness)
                    .Append(',').Append(Lower(row.DevOnly)).Append(',')
                    .Append(Lower(row.IsEnabled)).Append(',')
                    .Append(Lower(row.UserCoordinateAccepted)).Append(',')
                    .Append(Lower(row.Activated)).Append('\n');
            }
            return text.ToString();
        }

        private static string BuildCells(DevEncounterLayoutPressureAuthoringResult result)
        {
            StringBuilder text = new StringBuilder(
                "pressureInputId,x,y,domain,usable,baselineEye,effectiveEye\n");
            foreach (DevEncounterLayoutPressureAuthoringRowSnapshot row in result.Rows)
            {
                HashSet<LayoutCellCoordinate> usable = new HashSet<LayoutCellCoordinate>(
                    row.PressureSnapshot.UsableCellsAfterPressure);
                foreach (LayoutCellCoordinate cell in row.PressureSnapshot.LayoutDomainCells
                    .OrderBy(value => value.Y).ThenBy(value => value.X))
                {
                    text.Append(row.PressureInputId).Append(',').Append(cell.X)
                        .Append(',').Append(cell.Y).Append(",true,")
                        .Append(Lower(usable.Contains(cell))).Append(',')
                        .Append(Lower(cell.X == 2 && cell.Y == 2)).Append(',')
                        .Append(Lower(cell.Equals(row.PressureSnapshot
                            .EffectiveEyeAnchorCellAfterPressure))).Append('\n');
                }
            }
            return text.ToString();
        }

        private static string BuildConnections(
            DevEncounterLayoutPressureAuthoringResult result)
        {
            StringBuilder text = new StringBuilder("pressureInputId,ax,ay,bx,by\n");
            foreach (DevEncounterLayoutPressureAuthoringRowSnapshot row in result.Rows)
            {
                foreach (LayoutCellConnection edge in row.PressureSnapshot
                    .PreservedStructuralConnectionsAfterPressure.Select(Normalize)
                    .OrderBy(value => value.CellA.Y).ThenBy(value => value.CellA.X)
                    .ThenBy(value => value.CellB.Y).ThenBy(value => value.CellB.X))
                {
                    text.Append(row.PressureInputId).Append(',').Append(edge.CellA.X)
                        .Append(',').Append(edge.CellA.Y).Append(',')
                        .Append(edge.CellB.X).Append(',').Append(edge.CellB.Y)
                        .Append('\n');
                }
            }
            return text.ToString();
        }

        private static string BuildMaps(DevEncounterLayoutPressureAuthoringResult result)
        {
            StringBuilder text = new StringBuilder();
            text.Append("# Dev Encounter 5×5 压力格图\n\n")
                .Append("坐标：x 从左到右 0..4；y 从上到下 0..4。图例：· 可用，污 不可用，基 基准阵眼，效 有效阵眼，同 基准与有效阵眼重合。\n\n");
            foreach (DevEncounterLayoutPressureAuthoringRowSnapshot row in result.Rows)
            {
                text.Append("## ").Append(DisplayTitle(row.PressureInputId)).Append("\n\n")
                    .Append("- PressureInputId: `").Append(row.PressureInputId).Append("`\n")
                    .Append("- Context: `").Append(row.SeedId).Append(" / ")
                    .Append(row.EncounterId).Append(" / ").Append(row.MapRuleId)
                    .Append("`\n- 有效阵眼：`").Append(Cell(row.PressureSnapshot
                        .EffectiveEyeAnchorCellAfterPressure)).Append("`\n")
                    .Append("- 保留连接：`").Append(row.PressureSnapshot
                        .PreservedStructuralConnectionsAfterPressure.Count)
                    .Append("` 条\n- 压力说明：").Append(PressureNote(row.PressureInputId))
                    .Append("\n\n```text\n    x=0 x=1 x=2 x=3 x=4\n");
                HashSet<LayoutCellCoordinate> usable = new HashSet<LayoutCellCoordinate>(
                    row.PressureSnapshot.UsableCellsAfterPressure);
                for (int y = 0; y < 5; y++)
                {
                    text.Append("y=").Append(y).Append("  ");
                    for (int x = 0; x < 5; x++)
                    {
                        LayoutCellCoordinate cell = new LayoutCellCoordinate(x, y);
                        bool baseline = x == 2 && y == 2;
                        bool effective = cell.Equals(row.PressureSnapshot
                            .EffectiveEyeAnchorCellAfterPressure);
                        string mark = !usable.Contains(cell) ? "污" :
                            baseline && effective ? "同" : baseline ? "基" :
                            effective ? "效" : "·";
                        text.Append(mark);
                        if (x < 4) text.Append("   ");
                    }
                    text.Append('\n');
                }
                text.Append("```\n\n");
            }
            return text.ToString().TrimEnd('\n') + "\n";
        }

        private static string DisplayTitle(string id)
        {
            if (Has(id, "formation_eye") && Has(id, "furnace_core"))
                return "阵眼压力 / 4-10 炉心灰落";
            if (Has(id, "formation_eye")) return "阵眼压力 / 4-10 雷火交叉·青石裂隙";
            if (Has(id, "3_10")) return "污染格压力 / 3-10 净化角·青石潮湿";
            return "污染格压力 / 4-10 炉心灰落";
        }

        private static string PressureNote(string id)
        {
            if (Has(id, "formation_eye") && Has(id, "furnace_core"))
                return "阵眼移至 (2,1)；y=1 与 y=2 之间的 5 条正交边未保留。";
            if (Has(id, "formation_eye"))
                return "阵眼移至 (3,2)；x=2 与 x=3 之间的 5 条正交边未保留。";
            if (Has(id, "3_10")) return "污染格为 (1,1)、(3,3)。";
            return "污染格为 (1,1)、(3,1)、(1,3)、(3,3)。";
        }

        private static LayoutCellConnection Normalize(LayoutCellConnection edge)
        {
            int compare = edge.CellA.Y.CompareTo(edge.CellB.Y);
            if (compare == 0) compare = edge.CellA.X.CompareTo(edge.CellB.X);
            return compare <= 0 ? edge : new LayoutCellConnection(edge.CellB, edge.CellA);
        }

        private static string ContextIdentity(
            DevEncounterLayoutPressureAuthoringRowSnapshot row)
        {
            return string.Join("|", row.CandidateMigrationRequirementId, row.SeedId,
                row.EncounterId, row.MapRuleId);
        }

        private static string Cell(LayoutCellCoordinate value)
        {
            return value == null ? "null" : value.X.ToString(
                CultureInfo.InvariantCulture) + ":" + value.Y.ToString(
                    CultureInfo.InvariantCulture);
        }

        private static string Lower(bool value) => value ? "true" : "false";
        private static string Normalize(string value) => (value ?? string.Empty)
            .Replace("\r\n", "\n").Replace("\r", "\n");

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
            Add(checks, "enum." + id + ".undefined", "false",
                Enum.IsDefined(typeof(T), 0).ToString().ToLowerInvariant(),
                !Enum.IsDefined(typeof(T), 0));
        }

        private static void AssertProperties(ICollection<Check> checks, string id,
            Type type, params string[] expected)
        {
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);
            string[] actual = properties.Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] wanted = expected.OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Add(checks, "properties." + id + ".exact", string.Join("|", wanted),
                string.Join("|", actual), wanted.SequenceEqual(actual));
            Add(checks, "properties." + id + ".readonly", "0",
                properties.Count(value => value.SetMethod != null).ToString(
                    CultureInfo.InvariantCulture),
                properties.All(value => value.SetMethod == null));
        }

        private static void AssertReadOnly<T>(ICollection<Check> checks, string id,
            IReadOnlyList<T> values)
        {
            bool threw = false;
            try { ((IList)values).Add(default(T)); }
            catch (NotSupportedException) { threw = true; }
            Add(checks, "readonly." + id, "true", Lower(threw), threw);
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Substring(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static bool Has(string source, string token)
        {
            return (source ?? string.Empty).IndexOf(token ?? string.Empty,
                StringComparison.Ordinal) >= 0;
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

        private static int TrailingWhitespaceCount(string path)
        {
            if (!File.Exists(path)) return 1;
            return Normalize(File.ReadAllText(path)).Split('\n').Count(line =>
                line.EndsWith(" ", StringComparison.Ordinal) ||
                line.EndsWith("\t", StringComparison.Ordinal));
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

        private static IEnumerable<string> P4Files(string root) => ContractFiles(root,
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer",
            null,
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs",
            "LayoutResilienceStructuralReadinessConsumer");

        private static IEnumerable<string> P3Files(string root) => ContractFiles(root,
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly",
            null,
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs",
            "LayoutResilienceEvaluationInputAssembler");

        private static IEnumerable<string> P2Files(string root) => ContractFiles(root,
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure",
            null,
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs",
            "AuthoredLayoutPressureSourceAdapter");

        private static IEnumerable<string> N01CFiles(string root) => ContractFiles(root,
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience",
            "LayoutResilienceStructuralPredicate",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
            "LayoutResilienceStructuralPredicateContract");

        private static IEnumerable<string> N01BFiles(string root) => ContractFiles(root,
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel", null,
            "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
            "EnemyRequirementChannelApplicabilitySchemaContract");

        private static IEnumerable<string> ContractFiles(string root,
            string runtimeDirectory, string runtimePrefix, string verifier,
            string reportPrefix)
        {
            List<string> values = new List<string> { runtimeDirectory + ".meta" };
            values.AddRange(DirectoryFiles(root, runtimeDirectory, false).Where(path =>
                runtimePrefix == null || Path.GetFileName(path).StartsWith(
                    runtimePrefix, StringComparison.Ordinal)));
            values.Add(verifier);
            values.Add(verifier + ".meta");
            values.AddRange(ReportFiles(root, reportPrefix));
            return values;
        }

        private static IEnumerable<string> ReportFiles(string root, string prefix)
        {
            return Directory.GetFiles(Absolute(root, ReportDirectory), prefix + "*",
                    SearchOption.TopDirectoryOnly).Select(path => Relative(root, path));
        }

        private static IEnumerable<string> DirectoryFiles(string root,
            string directory, bool recursive = true)
        {
            return Directory.GetFiles(Absolute(root, directory), "*", recursive
                    ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path));
        }

        private static AggregateHash AggregateItem(string root)
        {
            string directory = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Items");
            string excluded = Absolute(root,
                    "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string excludedMeta = Absolute(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            return AggregateFiles(root, Directory.GetFiles(directory, "*",
                    SearchOption.AllDirectories).Where(path =>
                        !path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(path, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                .Select(path => Relative(root, path)), false);
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
                    SearchOption.AllDirectories).Where(path =>
                        !path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(path, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                .Select(path => Relative(root, path)), false);
        }

        private static AggregateHash AggregateFiles(string root,
            IEnumerable<string> paths, bool trailingLf)
        {
            string[] ordered = paths.OrderBy(value => value,
                trailingLf ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                .ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                payload.Append(ordered[index].Replace('\\', '/')).Append('|')
                    .Append(FileHash(root, ordered[index], !trailingLf));
                if (trailingLf || index + 1 < ordered.Length) payload.Append('\n');
            }
            return new AggregateHash(ordered.Length,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static void AddProtection(ICollection<Check> checks, string root,
            string id, IEnumerable<string> paths, bool trailingLf,
            int expectedCount, string expectedHash)
        {
            AggregateHash actual = AggregateFiles(root, paths, trailingLf);
            AddAggregate(checks, id, actual, expectedCount, expectedHash);
        }

        private static void AddAggregate(ICollection<Check> checks, string id,
            AggregateHash actual, int expectedCount, string expectedHash)
        {
            Add(checks, "protected." + id + ".count",
                expectedCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount.ToString(CultureInfo.InvariantCulture),
                actual.FileCount == expectedCount);
            Add(checks, "protected." + id + ".hash", expectedHash, actual.Hash,
                actual.Hash == expectedHash);
        }

        private static string CaptureProtected(string root)
        {
            return string.Join("|",
                AggregateFiles(root, P5SurveyFiles, true).Hash,
                AggregateFiles(root, P2Files(root), true).Hash,
                AggregateFiles(root, P3Files(root), true).Hash,
                AggregateFiles(root, P4Files(root), true).Hash,
                AggregateFiles(root, N01CFiles(root), true).Hash,
                AggregateFiles(root, N01BFiles(root), true).Hash,
                AggregateFiles(root, E10Files(root), true).Hash,
                AggregateItem(root).Hash, AggregateEnemy(root).Hash,
                AggregateFiles(root, DirectoryFiles(root,
                    "Assets/_Game/Scenes"), true).Hash,
                AggregateFiles(root, DirectoryFiles(root,
                    "Assets/_Game/Prefabs"), true).Hash,
                FileHash(root, "ProjectSettings/EditorBuildSettings.asset", false));
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

        private static ProcessResult Run(string root, string fileName, string arguments)
        {
            ProcessStartInfo start = new ProcessStartInfo(fileName, arguments)
            {
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(start))
            {
                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return new ProcessResult(process.ExitCode, (stdout + stderr).Trim());
            }
        }

        private static string Quote(string value) => "\"" + value + "\"";

        private static string FindRoot()
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
                        return current.FullName;
                    current = current.Parent;
                }
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string path) => Path.Combine(root,
            path.Replace('/', Path.DirectorySeparatorChar));

        private static string Relative(string root, string path) => path
            .Substring(root.TrimEnd(Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar).Length + 1).Replace('\\', '/');

        private static string Read(string root, string path) => File.ReadAllText(
            Absolute(root, path));

        private static void Add(ICollection<Check> checks, string id,
            string expected, string actual, bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private readonly struct Check
        {
            public Check(string id, string expected, string actual, bool passed)
            { Id = id; Expected = expected; Actual = actual; Passed = passed; }
            public string Id { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Passed { get; }
        }

        private readonly struct Summary
        {
            public Summary(int failed, string message)
            { Failed = failed; Message = message; }
            public int Failed { get; }
            public string Message { get; }
        }

        private readonly struct AggregateHash
        {
            public AggregateHash(int fileCount, string hash)
            { FileCount = fileCount; Hash = hash; }
            public int FileCount { get; }
            public string Hash { get; }
        }

        private readonly struct ProcessResult
        {
            public ProcessResult(int exitCode, string output)
            { ExitCode = exitCode; Output = output; }
            public int ExitCode { get; }
            public string Output { get; }
        }
    }
}
