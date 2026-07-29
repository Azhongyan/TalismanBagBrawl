using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy;
using TalismanBag.EnemySystem.Normalization;
using TalismanBag.EnemySystem.ReadinessEvaluation;
using TalismanBag.EnemySystem.SeedData;
using TalismanBag.EnemySystem.Vocabulary;
using TalismanBag.EditorTools.EnemySystem;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class ItemEnemyMatchupSimulationVerifier
    {
        private const string Package = "V0.4-ItemEnemyMatchupSimulation01";
        private const string GuardReceipt = "GUARD_PASS_ITEMENEMYMATCHUPSIMULATION01";
        private const string CatalogPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReportName = "ItemEnemyMatchupSimulationReport.md";
        private const string MatrixName = "ItemEnemyMatchupMatrix.csv";
        private const string CoverageName = "ItemEnemyMatchupCapabilityCoverage.csv";
        private const string BlockersName = "ItemEnemyMatchupUnknownBlockers.csv";
        private const string LeakName = "ItemEnemyMatchupLeakCheckReport.md";
        private const string ExpectedItemHash =
            "7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663";
        private const string ExpectedEnemyHash =
            "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae";
        private const string ExpectedC01Hash =
            "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a";
        private const int ExpectedItemFileCount = 105;
        private const int ExpectedEnemyFileCount = 89;

        private static readonly string[] C01ProtectedFiles =
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

        private static readonly string[] SourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulationVerifier.cs"
        };

        private static readonly string[] ReportFiles =
        {
            MainReportName,
            MatrixName,
            CoverageName,
            BlockersName,
            LeakName
        };

        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/ItemEnemyMatchupSimulation01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }

        public static void VerifyOffline()
        {
            VerifyAndWrite(false, "Offline deterministic verifier");
        }

        public static void VerifyBatch()
        {
            VerifyAndWrite(true, "Unity batch compile/verifier");
        }

        private static void VerifyAndWrite(bool exitWhenDone, string mode)
        {
            string root = FindProjectRoot();
            List<Check> checks = new List<Check>();
            Fixture fixture = null;
            List<LeakRow> leakRows = new List<LeakRow>();
            try
            {
                fixture = CreateFixture();
                RunFixtureChecks(checks, fixture);
                RunSimulationChecks(checks, fixture);
                RunProtectedChecks(checks, root, fixture);
                RunLeakChecks(checks, root, fixture, leakRows);
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
            }

            if (fixture != null)
            {
                WriteReports(root, mode, fixture, checks, leakRows);
                string first = ReportsHash(root);
                WriteReports(root, mode, fixture, checks, leakRows);
                string second = ReportsHash(root);
                Add(checks, "reports.deterministic", first, second, first == second);
                Add(checks, "reports.complete", "5", ReportFiles.Count(name =>
                    File.Exists(Path.Combine(root, ReportDirectory, name))).ToString(
                        CultureInfo.InvariantCulture),
                    ReportFiles.All(name => File.Exists(Path.Combine(root, ReportDirectory, name))));
                WriteReports(root, mode, fixture, checks, leakRows);
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine((pass ? "ITEM_ENEMY_MATCHUP_SIMULATION_PASS " :
                "ITEM_ENEMY_MATCHUP_SIMULATION_FAIL ")
                + checks.Count(value => value.Passed).ToString(CultureInfo.InvariantCulture)
                + "/" + checks.Count.ToString(CultureInfo.InvariantCulture));
            if (exitWhenDone && Application.isBatchMode)
            {
                EditorApplication.Exit(pass ? 0 : 1);
            }

            if (!pass)
            {
                throw new InvalidOperationException(
                    "ItemEnemyMatchupSimulation01 verifier failed: "
                    + string.Join(" | ", checks.Where(value => !value.Passed)
                        .Select(value => value.Id + "=" + value.Actual)));
            }
        }

        private static Fixture CreateFixture()
        {
            ItemBalanceWorkbenchCatalog catalog =
                AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(CatalogPath);
            if (catalog == null)
            {
                throw new InvalidOperationException(
                    "FIXTURE_SOURCE_BLOCKED: existing Item balance catalog is unavailable.");
            }

            ItemBalanceCompiledData compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
            ItemInstanceProjectionContractSnapshot offense = Preview(
                catalog, compiled, "I003", ItemInstanceRarity.Green, 1101L,
                value => HasAffix(value, "affix_break_up"));
            ItemInstanceProjectionContractSnapshot defense = Preview(
                catalog, compiled, "I013", ItemInstanceRarity.Green, 1201L,
                value => HasAffix(value, "affix_guard_up"));
            ItemInstanceProjectionContractSnapshot cooldown = Preview(
                catalog, compiled, "I001", ItemInstanceRarity.Green, 1301L,
                value => HasAffix(value, "affix_cooldown_reduction"));
            ItemInstanceProjectionContractSnapshot sparse = Preview(
                catalog, compiled, "I019", ItemInstanceRarity.Green, 1401L,
                value => !HasAnyMappedAffix(value));
            ItemInstanceProjectionContractSnapshot sameBaseA = Preview(
                catalog, compiled, "I003", ItemInstanceRarity.Blue, 1501L,
                value => HasAffix(value, "affix_break_up"));
            ItemInstanceProjectionContractSnapshot sameBaseB = Preview(
                catalog, compiled, "I003", ItemInstanceRarity.Blue, 1502L,
                value => HasAffix(value, "affix_break_up"));
            ItemInstanceProjectionContractSnapshot rarityGreen = Preview(
                catalog, compiled, "I013", ItemInstanceRarity.Green, 1601L,
                value => HasAffix(value, "affix_guard_up"));
            ItemInstanceProjectionContractSnapshot rarityBlue = Preview(
                catalog, compiled, "I013", ItemInstanceRarity.Blue, 1602L,
                value => HasAffix(value, "affix_guard_up"));
            ItemInstanceProjectionContractSnapshot rarityPurple = Preview(
                catalog, compiled, "I013", ItemInstanceRarity.Purple, 1603L,
                value => HasAffix(value, "affix_guard_up"));

            ItemEnemyMatchupScenarioInput[] scenarios =
            {
                new ItemEnemyMatchupScenarioInput("EMPTY",
                    Array.Empty<ItemInstanceProjectionContractSnapshot>(), compiled.AffixSchema),
                new ItemEnemyMatchupScenarioInput("OFFENSE_SUPPORTED",
                    new[] { offense }, compiled.AffixSchema),
                new ItemEnemyMatchupScenarioInput("DEFENSE_SUPPORTED",
                    new[] { defense }, compiled.AffixSchema),
                new ItemEnemyMatchupScenarioInput("COOLDOWN_SUPPORTED",
                    new[] { cooldown }, compiled.AffixSchema),
                new ItemEnemyMatchupScenarioInput("MIXED_SUPPORTED",
                    new[] { offense, defense, cooldown }, compiled.AffixSchema),
                new ItemEnemyMatchupScenarioInput("SPARSE_UNKNOWN",
                    new[] { sparse }, compiled.AffixSchema),
                new ItemEnemyMatchupScenarioInput("SAME_BASE_MULTI_INSTANCE",
                    new[] { sameBaseA, sameBaseB }, compiled.AffixSchema),
                new ItemEnemyMatchupScenarioInput("RARITY_VARIANTS",
                    new[] { rarityGreen, rarityBlue, rarityPurple }, compiled.AffixSchema)
            };

            EnemyMechanicVocabularySnapshot vocabulary =
                DefaultEnemyMechanicVocabularyProvider.Instance.CreateSnapshot(
                    DefaultEnemyMechanicVocabularyCatalog.CreateInput(false));
            EnemyValidationContentSnapshot normalization =
                EnemyValidationContentNormalizer.CreateSnapshot(false);
            DevEncounterSeedDataSnapshot encounters =
                DefaultDevEncounterSeedDataProvider.Instance.CreateSnapshot(
                    new DevEncounterSeedDataInput(vocabulary, normalization));
            MatchupResolver resolver = new MatchupResolver(vocabulary, normalization);
            ItemEnemyMatchupSimulationResult result =
                DefaultItemEnemyMatchupSimulation.Instance.Run(scenarios, encounters, resolver);
            ItemEnemyMatchupSimulationResult repeat =
                DefaultItemEnemyMatchupSimulation.Instance.Run(
                    scenarios.Reverse(), encounters, resolver);
            return new Fixture(catalog, compiled, scenarios, encounters, result, repeat);
        }

        private static ItemInstanceProjectionContractSnapshot Preview(
            ItemBalanceWorkbenchCatalog catalog,
            ItemBalanceCompiledData compiled,
            string baseItemId,
            ItemInstanceRarity rarity,
            long firstSeed,
            Func<ItemInstanceProjectionContractSnapshot, bool> predicate)
        {
            for (long seed = firstSeed; seed < firstSeed + 4096L; seed++)
            {
                ItemBalancePreviewResult preview = ItemBalanceWorkbenchCompiler.Preview(
                    catalog, compiled, baseItemId, rarity, seed);
                ItemInstanceProjectionContractSnapshot projection =
                    preview.ProjectionResult == null ? null : preview.ProjectionResult.snapshot;
                if (preview.isSuccess && projection != null && predicate(projection))
                {
                    return projection;
                }
            }

            throw new InvalidOperationException(
                "FIXTURE_SOURCE_BLOCKED: no read-only candidate projection satisfies "
                + baseItemId + "@" + rarity + ".");
        }

        private static bool HasAffix(
            ItemInstanceProjectionContractSnapshot projection,
            string affixId)
        {
            return projection.Affixes.Any(value =>
                string.Equals(value.affixId, affixId, StringComparison.Ordinal));
        }

        private static bool HasAnyMappedAffix(ItemInstanceProjectionContractSnapshot projection)
        {
            return projection.Affixes.Any(value =>
                value.affixId == "affix_break_up"
                || value.affixId == "affix_guard_up"
                || value.affixId == "affix_cooldown_reduction");
        }

        private static void RunFixtureChecks(ICollection<Check> checks, Fixture fixture)
        {
            Add(checks, "fixture.catalog", CatalogPath, fixture.Catalog == null ? "missing" : CatalogPath,
                fixture.Catalog != null);
            Add(checks, "fixture.scenarioCount", "8", fixture.Scenarios.Count.ToString(
                CultureInfo.InvariantCulture), fixture.Scenarios.Count == 8);
            string[] expected =
            {
                "EMPTY", "OFFENSE_SUPPORTED", "DEFENSE_SUPPORTED", "COOLDOWN_SUPPORTED",
                "MIXED_SUPPORTED", "SPARSE_UNKNOWN", "SAME_BASE_MULTI_INSTANCE", "RARITY_VARIANTS"
            };
            Add(checks, "fixture.scenarioIds", string.Join(";", expected.OrderBy(value => value,
                    StringComparer.Ordinal)),
                string.Join(";", fixture.Scenarios.Select(value => value.ScenarioId)
                    .OrderBy(value => value, StringComparer.Ordinal)),
                expected.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                    fixture.Scenarios.Select(value => value.ScenarioId)
                        .OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal));
            Add(checks, "fixture.encounterCount", "4", fixture.Encounters.SeedProfiles.Count.ToString(
                CultureInfo.InvariantCulture), fixture.Encounters.SeedProfiles.Count == 4);
            Add(checks, "fixture.e10Isolation", "true/false/false",
                fixture.Encounters.DevOnly + "/" + fixture.Encounters.IsEnabled + "/"
                + fixture.Encounters.EntersFormalFlow,
                fixture.Encounters.DevOnly && !fixture.Encounters.IsEnabled
                    && !fixture.Encounters.EntersFormalFlow);
            AssertScenarioProjection(checks, fixture, "OFFENSE_SUPPORTED", "capability.break_power", true);
            AssertScenarioProjection(checks, fixture, "DEFENSE_SUPPORTED", "capability.guard_power", true);
            AssertScenarioProjection(checks, fixture, "COOLDOWN_SUPPORTED",
                "capability.cooldown_recovery", true);
            foreach (string key in new[]
            {
                "capability.break_power", "capability.guard_power", "capability.cooldown_recovery"
            })
            {
                AssertScenarioProjection(checks, fixture, "MIXED_SUPPORTED", key, true);
            }

            AssertScenarioProjection(checks, fixture, "SPARSE_UNKNOWN",
                "capability.break_power", false);
            ItemEnemyMatchupScenarioInput sameBase = fixture.Scenarios.First(value =>
                value.ScenarioId == "SAME_BASE_MULTI_INSTANCE");
            Add(checks, "fixture.sameBaseMultiInstance", "2/I003", sameBase.ItemInstances.Count + "/"
                    + string.Join(";", sameBase.ItemInstances.Select(value => value.baseItemId)
                        .Distinct(StringComparer.Ordinal)),
                sameBase.ItemInstances.Count == 2 && sameBase.ItemInstances.All(value =>
                    value.baseItemId == "I003") && sameBase.ItemInstances.Select(value => value.itemInstanceId)
                    .Distinct(StringComparer.Ordinal).Count() == 2);
            ItemEnemyMatchupScenarioInput rarities = fixture.Scenarios.First(value =>
                value.ScenarioId == "RARITY_VARIANTS");
            Add(checks, "fixture.rarityVariants", ">=3 same base", rarities.ItemInstances.Count + "/"
                    + string.Join(";", rarities.ItemInstances.Select(value => value.rarity)
                        .Distinct().OrderBy(value => value)),
                rarities.ItemInstances.Count >= 3
                    && rarities.ItemInstances.Select(value => value.baseItemId)
                        .Distinct(StringComparer.Ordinal).Count() == 1
                    && rarities.ItemInstances.Select(value => value.rarity).Distinct().Count() >= 3);
        }

        private static void AssertScenarioProjection(
            ICollection<Check> checks,
            Fixture fixture,
            string scenarioId,
            string capabilityKey,
            bool expectedSupported)
        {
            ItemEnemyMatchupScenarioInput scenario = fixture.Scenarios.First(value =>
                value.ScenarioId == scenarioId);
            ItemBuildCapabilityProjectionResult projection =
                new DefaultItemBuildCapabilityProjectionAdapter().Project(
                    new ItemBuildCapabilityProjectionInput(
                        scenario.ItemInstances, scenario.AffixSchema));
            bool supported = projection.FieldMap.Any(value =>
                value.TargetCapabilityKey == capabilityKey
                && value.MappingStatus == ItemBuildCapabilityMappingStatus.SUPPORTED);
            Add(checks, "fixture." + scenarioId + "." + capabilityKey,
                expectedSupported ? "SUPPORTED" : "NOT_SUPPORTED",
                supported ? "SUPPORTED" : "NOT_SUPPORTED", supported == expectedSupported);
        }

        private static void RunSimulationChecks(ICollection<Check> checks, Fixture fixture)
        {
            ItemEnemyMatchupSimulationResult result = fixture.Result;
            Add(checks, "matrix.rowCount", "32", result.MatrixRows.Count.ToString(
                CultureInfo.InvariantCulture), result.MatrixRows.Count == 32);
            Add(checks, "matrix.crossProduct", "8x4", result.MatrixRows
                    .Select(value => value.ScenarioId + "\u001f" + value.EncounterId)
                    .Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                result.MatrixRows.Select(value => value.ScenarioId + "\u001f" + value.EncounterId)
                    .Distinct(StringComparer.Ordinal).Count() == 32);
            Add(checks, "coverage.keyCount", "16", result.CoverageRows.Count.ToString(
                CultureInfo.InvariantCulture), result.CoverageRows.Count == 16);
            Add(checks, "coverage.uniqueKeys", "16", result.CoverageRows
                    .Select(value => value.CapabilityKey).Distinct(StringComparer.Ordinal).Count()
                    .ToString(CultureInfo.InvariantCulture),
                result.CoverageRows.Select(value => value.CapabilityKey)
                    .Distinct(StringComparer.Ordinal).Count() == 16);
            Add(checks, "trace.mappingVersion", DefaultItemBuildCapabilityProjectionAdapter.MappingVersion,
                string.Join(";", result.MatrixRows.Select(value => value.MappingVersion)
                    .Distinct(StringComparer.Ordinal)),
                result.MatrixRows.All(value => value.MappingVersion ==
                    DefaultItemBuildCapabilityProjectionAdapter.MappingVersion));
            Add(checks, "trace.itemSource", "all C01 ibcr<64-hex>", result.MatrixRows.Count(value =>
                    IsC01SourceRevision(value.ItemSourceSignature)).ToString(CultureInfo.InvariantCulture),
                result.MatrixRows.All(value => IsC01SourceRevision(value.ItemSourceSignature)));
            Add(checks, "trace.enemySnapshot", fixture.Encounters.EnemySystemSnapshot.CanonicalSignature,
                string.Join(";", result.MatrixRows.Select(value => value.EnemySnapshotSignature)
                    .Distinct(StringComparer.Ordinal)),
                result.MatrixRows.All(value => value.EnemySnapshotSignature ==
                    fixture.Encounters.EnemySystemSnapshot.CanonicalSignature));
            Add(checks, "unknown.bandSuppressed", "all blocked/no-relevant rows blank",
                result.MatrixRows.Count(value =>
                    (value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.BLOCKED_BY_UNKNOWN
                        || value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.NO_RELEVANT_SUPPORTED_CAPABILITY)
                    && string.IsNullOrEmpty(value.ReadinessBand)).ToString(CultureInfo.InvariantCulture),
                result.MatrixRows.Where(value =>
                    value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.BLOCKED_BY_UNKNOWN
                    || value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.NO_RELEVANT_SUPPORTED_CAPABILITY)
                    .All(value => string.IsNullOrEmpty(value.ReadinessBand)));
            Add(checks, "unknown.notZero", "Unknown counts retained", result.MatrixRows.Sum(value =>
                    value.UnknownRequirementCount).ToString(CultureInfo.InvariantCulture),
                result.MatrixRows.Any(value => value.UnknownRequirementCount > 0));
            Add(checks, "unknown.blockerKinds", "5 enums / required kinds present",
                string.Join(";", result.UnknownBlockers.Select(value => value.BlockerKind)
                    .Distinct().OrderBy(value => value)),
                Enum.GetValues(typeof(ItemEnemyMatchupUnknownBlockerKind)).Length == 5
                    && result.UnknownBlockers.Any(value => value.BlockerKind ==
                        ItemEnemyMatchupUnknownBlockerKind.ITEM_CAPABILITY_MAPPING_MISSING)
                    && result.UnknownBlockers.Any(value => value.BlockerKind ==
                        ItemEnemyMatchupUnknownBlockerKind.ITEM_RUNTIME_FACT_MISSING)
                    && result.UnknownBlockers.Any(value => value.BlockerKind ==
                        ItemEnemyMatchupUnknownBlockerKind.ENCOUNTER_REQUIREMENT_UNKNOWN)
                    && result.UnknownBlockers.Any(value => value.BlockerKind ==
                        ItemEnemyMatchupUnknownBlockerKind.OUT_OF_SCOPE_RUNTIME_TIMING));
            Add(checks, "determinism.signature", result.CanonicalSignature,
                fixture.Repeat.CanonicalSignature,
                result.CanonicalSignature == fixture.Repeat.CanonicalSignature);
            Add(checks, "determinism.matrixRows", "all stable",
                result.MatrixRows.Zip(fixture.Repeat.MatrixRows,
                    (left, right) => left.ResultCanonicalSignature == right.ResultCanonicalSignature)
                    .Count(value => value).ToString(CultureInfo.InvariantCulture),
                result.MatrixRows.Select(value => value.ResultCanonicalSignature).SequenceEqual(
                    fixture.Repeat.MatrixRows.Select(value => value.ResultCanonicalSignature),
                    StringComparer.Ordinal));
            Add(checks, "isolation.result", "true/false/false",
                result.DevOnly + "/" + result.IsEnabled + "/" + result.EntersFormalFlow,
                result.DevOnly && !result.IsEnabled && !result.EntersFormalFlow);
            int publicPlayerAnswerFields = typeof(ItemEnemyMatchupMatrixRowSnapshot)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Count(value => value.PropertyType == typeof(EnemyReadinessPlayerHintProjection));
            Add(checks, "playerAnswerFields", "0", publicPlayerAnswerFields.ToString(
                CultureInfo.InvariantCulture), publicPlayerAnswerFields == 0);
            Add(checks, "invalidInputRows", "0", result.MatrixRows.Count(value =>
                    value.EvaluationStatus == ItemEnemyMatchupEvaluationStatus.INVALID_INPUT)
                    .ToString(CultureInfo.InvariantCulture),
                result.MatrixRows.All(value =>
                    value.EvaluationStatus != ItemEnemyMatchupEvaluationStatus.INVALID_INPUT));
        }

        private static void RunProtectedChecks(
            ICollection<Check> checks,
            string root,
            Fixture fixture)
        {
            fixture.ItemHash = AggregateDirectory(root,
                "Assets/_Game/Scripts/TalismanBag/Items");
            fixture.EnemyHash = AggregateDirectory(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem");
            fixture.C01Hash = AggregateFiles(root, C01ProtectedFiles);
            Add(checks, "protected.item.fileCount", ExpectedItemFileCount.ToString(
                CultureInfo.InvariantCulture), fixture.ItemHash.FileCount.ToString(
                CultureInfo.InvariantCulture), fixture.ItemHash.FileCount == ExpectedItemFileCount);
            Add(checks, "protected.item.hash", ExpectedItemHash, fixture.ItemHash.Hash,
                fixture.ItemHash.Hash == ExpectedItemHash);
            Add(checks, "protected.enemy.fileCount", ExpectedEnemyFileCount.ToString(
                CultureInfo.InvariantCulture), fixture.EnemyHash.FileCount.ToString(
                CultureInfo.InvariantCulture), fixture.EnemyHash.FileCount == ExpectedEnemyFileCount);
            Add(checks, "protected.enemy.hash", ExpectedEnemyHash, fixture.EnemyHash.Hash,
                fixture.EnemyHash.Hash == ExpectedEnemyHash);
            Add(checks, "protected.c01.fileCount", C01ProtectedFiles.Length.ToString(
                CultureInfo.InvariantCulture), fixture.C01Hash.FileCount.ToString(
                CultureInfo.InvariantCulture), fixture.C01Hash.FileCount == C01ProtectedFiles.Length);
            Add(checks, "protected.c01.hash", ExpectedC01Hash, fixture.C01Hash.Hash,
                fixture.C01Hash.Hash == ExpectedC01Hash);
        }

        private static void RunLeakChecks(
            ICollection<Check> checks,
            string root,
            Fixture fixture,
            ICollection<LeakRow> leaks)
        {
            Dictionary<string, string[]> forbiddenByFile = new Dictionary<string, string[]>(
                StringComparer.Ordinal)
            {
                [SourceFiles[0]] = new[]
                {
                    "using UnityEngine", "using UnityEditor", "MonoBehaviour", "GameObject",
                    "ScriptableObject", "TalismanBag.Battle", "TalismanBag.Board", "RunFlow",
                    "SaveData", "RewardConfig", "SceneManager", "PrefabUtility", "AssetDatabase"
                },
                [SourceFiles[1]] = new[]
                {
                    "AssetDatabase.CreateAsset", "PrefabUtility", "SceneManager", "TalismanBag.Battle",
                    "TalismanBag.Board", "RunFlow", "SaveData", "RewardConfig"
                }
            };
            foreach (KeyValuePair<string, string[]> pair in forbiddenByFile)
            {
                string source = StripCommentsAndStringLiterals(
                    File.ReadAllText(Path.Combine(root, pair.Key)));
                foreach (string token in pair.Value)
                {
                    int count = Count(source, token);
                    if (count > 0)
                    {
                        leaks.Add(new LeakRow(pair.Key, token, count));
                    }
                }
            }

            int playerLeak = fixture.Result.MatrixRows.Count(value =>
                value.ReadinessBand.IndexOf("过易", StringComparison.Ordinal) >= 0
                || value.ReadinessBand.IndexOf("合适", StringComparison.Ordinal) >= 0
                || value.ReadinessBand.IndexOf("过难", StringComparison.Ordinal) >= 0);
            if (playerLeak > 0)
            {
                leaks.Add(new LeakRow("matrix", "inventedDifficultyConclusion", playerLeak));
            }

            Add(checks, "leak.count", "0", leaks.Sum(value => value.Count).ToString(
                CultureInfo.InvariantCulture), leaks.Count == 0);
            Add(checks, "leak.formalFlow", "0", fixture.Result.EntersFormalFlow ? "1" : "0",
                !fixture.Result.EntersFormalFlow);
        }

        private static void WriteReports(
            string root,
            string mode,
            Fixture fixture,
            IReadOnlyList<Check> checks,
            IReadOnlyList<LeakRow> leaks)
        {
            string directory = Path.Combine(root, ReportDirectory);
            Directory.CreateDirectory(directory);
            Write(Path.Combine(directory, MainReportName), MainReport(mode, fixture, checks));
            Write(Path.Combine(directory, MatrixName), MatrixReport(fixture.Result));
            Write(Path.Combine(directory, CoverageName), CoverageReport(fixture.Result));
            Write(Path.Combine(directory, BlockersName), BlockerReport(fixture.Result));
            Write(Path.Combine(directory, LeakName), LeakReport(fixture, leaks));
        }

        private static string MainReport(
            string mode,
            Fixture fixture,
            IReadOnlyList<Check> checks)
        {
            ItemEnemyMatchupSimulationResult result = fixture.Result;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Item Enemy Matchup Simulation Report").AppendLine();
            builder.AppendLine("- Package: `" + Package + "`");
            builder.AppendLine("- Guard receipt: `" + GuardReceipt + "`");
            builder.AppendLine("- Mode: `" + mode + "`");
            builder.AppendLine("- Isolation: `devOnly=true / isEnabled=false / entersFormalFlow=false`");
            builder.AppendLine("- Scenario / Encounter / Matrix: `" + fixture.Scenarios.Count + " / "
                + fixture.Encounters.SeedProfiles.Count + " / " + result.MatrixRows.Count + "`");
            builder.AppendLine("- Capability coverage: `" + result.CoverageRows.Count + "/16`");
            foreach (ItemEnemyMatchupEvaluationStatus status in
                Enum.GetValues(typeof(ItemEnemyMatchupEvaluationStatus)))
            {
                builder.AppendLine("- " + status + ": `" + result.MatrixRows.Count(value =>
                    value.EvaluationStatus == status) + "`");
            }

            builder.AppendLine("- C01 mapping version: `"
                + DefaultItemBuildCapabilityProjectionAdapter.MappingVersion + "`");
            builder.AppendLine("- Enemy snapshot signature: `" + result.EnemySnapshotSignature + "`");
            builder.AppendLine("- Simulation canonical signature: `" + result.CanonicalSignature + "`");
            builder.AppendLine("- Protected C01 / Item / Enemy: `" + fixture.C01Hash.Hash + " / "
                + fixture.ItemHash.Hash + " / " + fixture.EnemyHash.Hash + "`");
            builder.AppendLine("- Checks: `" + checks.Count(value => value.Passed) + "/"
                + checks.Count + " PASS`").AppendLine();
            builder.AppendLine("Each E10 pressure profile is evaluated independently through E08. "
                + "No map-rule adjustment is inferred, no readiness band is promoted into a difficulty label, "
                + "and decisive Unknown requirements suppress the matrix readinessBand field.").AppendLine();
            builder.AppendLine("## Checks").AppendLine();
            builder.AppendLine("| Check | Expected | Actual | Result |");
            builder.AppendLine("|---|---|---|---|");
            foreach (Check check in checks.OrderBy(value => value.Id, StringComparer.Ordinal))
            {
                builder.Append("| ").Append(Md(check.Id)).Append(" | ").Append(Md(check.Expected))
                    .Append(" | ").Append(Md(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }

            return builder.ToString();
        }

        private static string MatrixReport(ItemEnemyMatchupSimulationResult result)
        {
            StringBuilder builder = new StringBuilder(
                "scenarioId,itemSourceSignature,encounterId,chapterLabelDevOnly,evaluationStatus,readinessBand,knownRequirementCount,unknownRequirementCount,metRequirementCount,gapCount,supportedCapabilities,unknownCapabilities,resultCanonicalSignature,notes\n");
            foreach (ItemEnemyMatchupMatrixRowSnapshot row in result.MatrixRows)
            {
                builder.Append(Row(row.ScenarioId, row.ItemSourceSignature, row.EncounterId,
                    row.ChapterLabelDevOnly, row.EvaluationStatus.ToString(), row.ReadinessBand,
                    row.KnownRequirementCount.ToString(CultureInfo.InvariantCulture),
                    row.UnknownRequirementCount.ToString(CultureInfo.InvariantCulture),
                    row.MetRequirementCount.ToString(CultureInfo.InvariantCulture),
                    row.GapCount.ToString(CultureInfo.InvariantCulture),
                    string.Join(";", row.SupportedCapabilities),
                    string.Join(";", row.UnknownCapabilities),
                    row.ResultCanonicalSignature, row.Notes));
            }

            return builder.ToString();
        }

        private static string CoverageReport(ItemEnemyMatchupSimulationResult result)
        {
            StringBuilder builder = new StringBuilder(
                "capabilityKey,c01MappingStatus,itemScenariosWithKnownValue,encounterRequirementReferences,evaluableMatchupCount,blockedMatchupCount,nextAction\n");
            foreach (ItemEnemyMatchupCapabilityCoverageSnapshot row in result.CoverageRows)
            {
                builder.Append(Row(row.CapabilityKey, row.MappingStatus.ToString(),
                    row.ItemScenariosWithKnownValue.ToString(CultureInfo.InvariantCulture),
                    row.EncounterRequirementReferences.ToString(CultureInfo.InvariantCulture),
                    row.EvaluableMatchupCount.ToString(CultureInfo.InvariantCulture),
                    row.BlockedMatchupCount.ToString(CultureInfo.InvariantCulture), row.NextAction));
            }

            return builder.ToString();
        }

        private static string BlockerReport(ItemEnemyMatchupSimulationResult result)
        {
            StringBuilder builder = new StringBuilder(
                "scenarioId,encounterId,capabilityKey,blockerKind,sourceCode,detail\n");
            foreach (ItemEnemyMatchupUnknownBlockerSnapshot row in result.UnknownBlockers)
            {
                builder.Append(Row(row.ScenarioId, row.EncounterId, row.CapabilityKey,
                    row.BlockerKind.ToString(), row.SourceCode, row.Detail));
            }

            return builder.ToString();
        }

        private static string LeakReport(Fixture fixture, IReadOnlyList<LeakRow> leaks)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Item Enemy Matchup Leak Check Report").AppendLine();
            builder.AppendLine("- Package: `" + Package + "`");
            builder.AppendLine("- Runtime/editor source files scanned: `2`");
            builder.AppendLine("- Player answer projection fields: `0`");
            builder.AppendLine("- Formal flow references: `0`");
            builder.AppendLine("- Scene / Prefab / Config writers: `0`");
            builder.AppendLine("- Leak count: `" + leaks.Sum(value => value.Count) + "`").AppendLine();
            builder.AppendLine("| File | Token | Count | Result |");
            builder.AppendLine("|---|---|---:|---|");
            if (leaks.Count == 0)
            {
                builder.AppendLine("| C02 whitelist sources | none | 0 | PASS |");
            }
            else
            {
                foreach (LeakRow leak in leaks)
                {
                    builder.Append("| ").Append(Md(leak.File)).Append(" | ")
                        .Append(Md(leak.Token)).Append(" | ").Append(leak.Count)
                        .AppendLine(" | FAIL |");
                }
            }

            builder.AppendLine().AppendLine(
                "Unknown remains sparse and diagnostic-only; it is not serialized as zero, failure, Ready, or a difficulty conclusion.");
            return builder.ToString();
        }

        private static AggregateHash AggregateDirectory(string root, string relativeDirectory)
        {
            string directory = Path.Combine(root, relativeDirectory);
            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return Aggregate(root, files);
        }

        private static AggregateHash AggregateFiles(string root, IEnumerable<string> relativeFiles)
        {
            string[] files = relativeFiles.Select(value => Path.Combine(root, value))
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return Aggregate(root, files);
        }

        private static AggregateHash Aggregate(string root, IReadOnlyList<string> files)
        {
            string payload = string.Join("\n", files.Select(file =>
                Relative(root, file) + "|" + Sha256(File.ReadAllBytes(file), true)));
            return new AggregateHash(files.Count, Sha256(Encoding.UTF8.GetBytes(payload), false));
        }

        private static string ReportsHash(string root)
        {
            string directory = Path.Combine(root, ReportDirectory);
            string payload = string.Join("\n", ReportFiles.OrderBy(value => value, StringComparer.Ordinal)
                .Select(name => name + "|" + Sha256(File.ReadAllBytes(Path.Combine(directory, name)), true)));
            return Sha256(Encoding.UTF8.GetBytes(payload), false);
        }

        private static string Row(params string[] values)
        {
            return string.Join(",", values.Select(Csv)) + "\n";
        }

        private static string Csv(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
        }

        private static string Md(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static void Write(string path, string text)
        {
            File.WriteAllText(path, text ?? string.Empty, new UTF8Encoding(false));
        }

        private static int Count(string source, string token)
        {
            int count = 0;
            int offset = 0;
            while ((offset = (source ?? string.Empty).IndexOf(token, offset,
                StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }

            return count;
        }

        private static string StripCommentsAndStringLiterals(string source)
        {
            string text = source ?? string.Empty;
            StringBuilder output = new StringBuilder(text.Length);
            bool lineComment = false;
            bool blockComment = false;
            bool regularString = false;
            bool verbatimString = false;
            bool character = false;
            for (int index = 0; index < text.Length; index++)
            {
                char current = text[index];
                char next = index + 1 < text.Length ? text[index + 1] : '\0';
                if (lineComment)
                {
                    if (current == '\n')
                    {
                        lineComment = false;
                        output.Append('\n');
                    }
                    else output.Append(' ');
                    continue;
                }

                if (blockComment)
                {
                    if (current == '*' && next == '/')
                    {
                        output.Append("  ");
                        index++;
                        blockComment = false;
                    }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }

                if (regularString)
                {
                    if (current == '\\' && next != '\0')
                    {
                        output.Append("  ");
                        index++;
                    }
                    else if (current == '"')
                    {
                        output.Append(' ');
                        regularString = false;
                    }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }

                if (verbatimString)
                {
                    if (current == '"' && next == '"')
                    {
                        output.Append("  ");
                        index++;
                    }
                    else if (current == '"')
                    {
                        output.Append(' ');
                        verbatimString = false;
                    }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }

                if (character)
                {
                    if (current == '\\' && next != '\0')
                    {
                        output.Append("  ");
                        index++;
                    }
                    else if (current == '\'')
                    {
                        output.Append(' ');
                        character = false;
                    }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }

                if (current == '/' && next == '/')
                {
                    output.Append("  ");
                    index++;
                    lineComment = true;
                }
                else if (current == '/' && next == '*')
                {
                    output.Append("  ");
                    index++;
                    blockComment = true;
                }
                else if (current == '@' && next == '"')
                {
                    output.Append("  ");
                    index++;
                    verbatimString = true;
                }
                else if (current == '"')
                {
                    output.Append(' ');
                    regularString = true;
                }
                else if (current == '\'')
                {
                    output.Append(' ');
                    character = true;
                }
                else output.Append(current);
            }

            return output.ToString();
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.StartsWith("sha256:", StringComparison.Ordinal)
                && value.Length == 71;
        }

        private static bool IsC01SourceRevision(string value)
        {
            return value != null && value.Length == 68
                && value.StartsWith("ibcr", StringComparison.Ordinal)
                && value.Substring(4).All(character =>
                    (character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f'));
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha = SHA256.Create())
            {
                string format = uppercase ? "X2" : "x2";
                return string.Concat(sha.ComputeHash(bytes ?? Array.Empty<byte>())
                    .Select(value => value.ToString(format, CultureInfo.InvariantCulture)));
            }
        }

        private static string Relative(string root, string path)
        {
            string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            return Path.GetFullPath(path).Substring(prefix.Length).Replace('\\', '/');
        }

        private static string FindProjectRoot()
        {
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "Assets"))
                    && Directory.Exists(Path.Combine(current.FullName, "ProjectSettings"))
                    && Directory.Exists(Path.Combine(current.FullName, "Packages")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Unity project root was not found.");
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

        private sealed class Fixture
        {
            public Fixture(
                ItemBalanceWorkbenchCatalog catalog,
                ItemBalanceCompiledData compiled,
                IReadOnlyList<ItemEnemyMatchupScenarioInput> scenarios,
                DevEncounterSeedDataSnapshot encounters,
                ItemEnemyMatchupSimulationResult result,
                ItemEnemyMatchupSimulationResult repeat)
            {
                Catalog = catalog;
                Compiled = compiled;
                Scenarios = scenarios;
                Encounters = encounters;
                Result = result;
                Repeat = repeat;
            }

            public ItemBalanceWorkbenchCatalog Catalog { get; }
            public ItemBalanceCompiledData Compiled { get; }
            public IReadOnlyList<ItemEnemyMatchupScenarioInput> Scenarios { get; }
            public DevEncounterSeedDataSnapshot Encounters { get; }
            public ItemEnemyMatchupSimulationResult Result { get; }
            public ItemEnemyMatchupSimulationResult Repeat { get; }
            public AggregateHash C01Hash { get; set; }
            public AggregateHash ItemHash { get; set; }
            public AggregateHash EnemyHash { get; set; }
        }

        private sealed class MatchupResolver : IEnemyReadinessReferenceResolver
        {
            private readonly EnemyMechanicVocabularySnapshot vocabulary;
            private readonly EnemyValidationContentSnapshot normalization;

            public MatchupResolver(
                EnemyMechanicVocabularySnapshot vocabulary,
                EnemyValidationContentSnapshot normalization)
            {
                this.vocabulary = vocabulary;
                this.normalization = normalization;
            }

            public bool HasBuildCapabilityKey(string key)
            {
                return vocabulary.TryGetEntry(EnemyVocabularyCategory.BuildCapability, key, out _);
            }

            public bool HasMapRule(string mapRuleId)
            {
                return normalization.TryGetMapRule(mapRuleId, out _);
            }
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

        private sealed class AggregateHash
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; }
            public string Hash { get; }
        }

        private sealed class LeakRow
        {
            public LeakRow(string file, string token, int count)
            {
                File = file;
                Token = token;
                Count = count;
            }

            public string File { get; }
            public string Token { get; }
            public int Count { get; }
        }
    }
}
