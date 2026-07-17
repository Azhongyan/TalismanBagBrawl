using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.EditorTools.ItemGeneration;
using TalismanBag.EditorTools.ItemSandbox;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.ItemBalance
{
    public static class ItemBalanceWorkbenchVerifier
    {
        private const string MenuPath = "Tools/Talisman Bag/V0.4/Data/Item Balance Workbench/[QA Only] Run Verification";
        private const string PassMarker = "ITEM_BALANCE_WORKBENCH_150_CANDIDATE_SEED01_GUARDFIX01_PASS";
        private static readonly UTF8Encoding Utf8 = new(false);

        private static readonly string[] PackageReportNames =
        {
            "ItemBalanceCandidate150.csv",
            "ItemBalanceStatRangeInventory.csv",
            "ItemBalanceWorkbenchSpec.csv",
            "ItemBalanceStatDictionary.csv",
            "ItemBalanceAffixCoreBuildInventory.csv",
            "ItemBalanceWorkbenchReport.md",
            "ItemBalanceWorkbenchLeakCheckReport.md"
        };

        [MenuItem(MenuPath)]
        public static void RunVerification()
        {
            bool pass = VerifyAndWrite(out string summary);
            EditorUtility.DisplayDialog(pass ? "Item Balance Verification PASS" : "Item Balance Verification FAIL",
                summary, "OK");
        }

        public static void VerifyBatch()
        {
            if (!VerifyAndWrite(out string summary))
                throw new InvalidOperationException(summary);
        }

        public static bool VerifyAndWrite(out string summary)
        {
            string projectRoot = ProjectRoot();
            List<SpecCase> specs = new();
            List<RegressionEvidence> regressions = new();
            List<string> warnings = new();
            int rolls = 0;
            int projections = 0;
            int deterministic = 0;

            ItemBalanceWorkbenchCatalog catalog = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(
                ItemBalanceCandidateSeedBuilder.CatalogPath);
            ProtectedBoundarySnapshot boundary = ProtectedBoundarySnapshot.Capture(projectRoot);

            AddSpec(specs, "catalog.loaded", "catalog", "not null", catalog == null ? "null" : "loaded", catalog != null);
            ItemBalanceValidationReport validation = ItemBalanceWorkbenchValidation.Validate(catalog, true);
            warnings.AddRange(validation.Warnings);
            AddSpec(specs, "catalog.validation", "catalog", "0 errors",
                validation.Errors.Count + " errors", validation.Errors.Count == 0);

            if (catalog != null)
            {
                AddCatalogAssertions(catalog, specs);
                ItemBalanceCompiledData compiled = null;
                try
                {
                    compiled = ItemBalanceWorkbenchCompiler.Compile(catalog);
                    AddSpec(specs, "schema.foundation", "schema_compile", "compiled", compiled.Foundation == null ? "null" : "compiled", compiled.Foundation != null);
                    AddSpec(specs, "schema.stat", "schema_compile", "compiled", compiled.StatSchema == null ? "null" : "compiled", compiled.StatSchema != null);
                    AddSpec(specs, "schema.affix", "schema_compile", "compiled", compiled.AffixSchema == null ? "null" : "compiled", compiled.AffixSchema != null);
                    AddSpec(specs, "schema.core_build", "schema_compile", "compiled", compiled.CoreBuildSchema == null ? "null" : "compiled", compiled.CoreBuildSchema != null);
                    AddSpec(specs, "schema.build_profiles", "schema_compile", "4", compiled.BuildProfiles.Count.ToString(CultureInfo.InvariantCulture), compiled.BuildProfiles.Count == 4);
                    AddSpec(specs, "schema.drop_profiles", "schema_compile", "5", compiled.DropProfiles.Count.ToString(CultureInfo.InvariantCulture), compiled.DropProfiles.Count == 5);
                }
                catch (Exception exception)
                {
                    AddSpec(specs, "schema.compile.exception", "schema_compile", "no exception", exception.GetType().Name + ": " + exception.Message, false);
                }

                RunCsvAndUndoChecks(catalog, specs);
                if (compiled != null)
                    RunCandidatePreviewAssertions(catalog, compiled, specs, ref rolls, ref projections, ref deterministic);
            }

            using (RegressionSideEffectScope sideEffects = RegressionSideEffectScope.Capture(projectRoot))
            {
                RunHistoricalRegressions(projectRoot, regressions, specs);
            }

            AssetDatabase.ImportAsset(ItemBalanceCandidateSeedBuilder.CatalogPath, ImportAssetOptions.ForceUpdate);
            catalog = AssetDatabase.LoadAssetAtPath<ItemBalanceWorkbenchCatalog>(ItemBalanceCandidateSeedBuilder.CatalogPath);
            int finalProfiles = catalog?.profiles.Count(value => value != null) ?? 0;
            int finalVersions = catalog?.profiles.Where(value => value != null).Sum(value => value.rarityVersions.Count(version => version != null)) ?? 0;
            int finalRanges = catalog?.profiles.Where(value => value != null).Sum(value => value.rarityVersions
                .Where(version => version != null).Sum(version => version.statRanges.Count(range => range != null))) ?? 0;
            AddSpec(specs, "report_input.profile_count", "report_input_reload", "30", finalProfiles.ToString(CultureInfo.InvariantCulture), finalProfiles == 30);
            AddSpec(specs, "report_input.version_count", "report_input_reload", "150", finalVersions.ToString(CultureInfo.InvariantCulture), finalVersions == 150);
            AddSpec(specs, "report_input.range_count", "report_input_reload", "600", finalRanges.ToString(CultureInfo.InvariantCulture), finalRanges == 600);

            List<LeakScanResult> leakScans = RunLeakScans(projectRoot);
            foreach (LeakScanResult scan in leakScans)
                AddSpec(specs, "leak." + scan.Category, "formal_system_not_wired", "0", scan.MatchCount.ToString(CultureInfo.InvariantCulture), scan.Pass);

            IReadOnlyList<BoundaryEvidence> boundaryResults = boundary.Compare();
            foreach (BoundaryEvidence evidence in boundaryResults)
                AddSpec(specs, "readonly." + evidence.Category, "read_only_boundary",
                    evidence.BeforeHash, evidence.AfterHash, evidence.Pass);

            AddSpec(specs, "count.rolls", "candidate_preview", "150", rolls.ToString(CultureInfo.InvariantCulture), rolls == 150);
            AddSpec(specs, "count.projections", "candidate_preview", "150", projections.ToString(CultureInfo.InvariantCulture), projections == 150);
            AddSpec(specs, "count.deterministic", "candidate_preview", "150", deterministic.ToString(CultureInfo.InvariantCulture), deterministic == 150);

            List<string> failures = specs.Where(value => !value.Pass)
                .Select(value => value.CaseId + ": expected=" + value.Expected + ", actual=" + value.Actual).ToList();
            WriteReports(catalog, specs, regressions, leakScans, boundaryResults, failures, warnings,
                rolls, projections, deterministic);

            bool pass = failures.Count == 0;
            int specPass = specs.Count(value => value.Pass);
            int specFail = specs.Count - specPass;
            summary = pass
                ? PassMarker + "\nprofiles=30\nversions=150\nstatRanges=600\nrolls=150\nprojections=150\ndeterministic=150\nspec=" + specPass + "/" + specs.Count + "\nleaks=0"
                : "ITEM_BALANCE_WORKBENCH_GUARDFIX01_VERIFICATION_FAIL\nspecPass=" + specPass + "\nspecFail=" + specFail + "\n" + string.Join("\n", failures.Take(40));
            if (pass) Debug.Log(summary); else Debug.LogError(summary);
            return pass;
        }

        private static void AddCatalogAssertions(ItemBalanceWorkbenchCatalog catalog, List<SpecCase> specs)
        {
            ItemBalanceProfile[] profiles = catalog.profiles.Where(value => value != null).OrderBy(value => value.baseItemId).ToArray();
            int versionCount = profiles.Sum(value => value.rarityVersions.Count(version => version != null));
            int rangeCount = profiles.Sum(value => value.rarityVersions.Where(version => version != null)
                .Sum(version => version.statRanges.Count(range => range != null)));
            AddSpec(specs, "catalog.profile_count", "prototype_30", "30", profiles.Length.ToString(CultureInfo.InvariantCulture), profiles.Length == 30);
            AddSpec(specs, "catalog.version_count", "version_150", "150", versionCount.ToString(CultureInfo.InvariantCulture), versionCount == 150);
            AddSpec(specs, "catalog.range_count", "stat_range_600", "600", rangeCount.ToString(CultureInfo.InvariantCulture), rangeCount == 600);
            int i031Count = profiles.Count(value => value.baseItemId == "I031");
            AddSpec(specs, "catalog.i031_excluded", "i031_exclusion", "0", i031Count.ToString(CultureInfo.InvariantCulture), i031Count == 0);
            AddSpec(specs, "catalog.maturity", "data_maturity", ItemBalanceWorkbenchCatalog.BalanceCandidate,
                catalog.dataMaturity, catalog.dataMaturity == ItemBalanceWorkbenchCatalog.BalanceCandidate);

            foreach (ItemBalanceProfile profile in profiles)
            {
                bool expectedId = int.TryParse(profile.baseItemId?.Substring(1), out int id) && id >= 1 && id <= 30;
                AddSpec(specs, "profile." + profile.baseItemId, "prototype_30", "I001-I030 unique",
                    profile.baseItemId, expectedId && profiles.Count(value => value.baseItemId == profile.baseItemId) == 1);
                foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                {
                    ItemBalanceRarityVersion version = profile.FindVersion(rarity.rarity);
                    string key = profile.baseItemId + "@" + rarity.stableKey;
                    bool versionPass = version != null
                        && version.versionKey == key
                        && version.dataMaturity == ItemBalanceWorkbenchCatalog.BalanceCandidate;
                    AddSpec(specs, "version." + key, "version_150", key + " / BALANCE_CANDIDATE",
                        version == null ? "missing" : version.versionKey + " / " + version.dataMaturity, versionPass);
                    foreach (ItemBalanceRange range in version?.statRanges.Where(value => value != null) ?? Enumerable.Empty<ItemBalanceRange>())
                    {
                        ItemBalanceStatDefinition definition = catalog.FindStat(range.statId);
                        bool rangePass = definition != null && definition.stepUnits > 0
                            && range.minUnits <= range.maxUnits
                            && range.minUnits % definition.stepUnits == 0
                            && range.maxUnits % definition.stepUnits == 0;
                        AddSpec(specs, "range." + key + "." + range.statId, "stat_range_600",
                            "defined, ordered, step-aligned", range.minUnits + ".." + range.maxUnits,
                            rangePass);
                    }
                }
            }
        }

        private static void RunCandidatePreviewAssertions(ItemBalanceWorkbenchCatalog catalog,
            ItemBalanceCompiledData compiled, List<SpecCase> specs, ref int rolls, ref int projections,
            ref int deterministic)
        {
            int index = 0;
            foreach (ItemBalanceProfile profile in catalog.profiles.Where(value => value != null).OrderBy(value => value.baseItemId))
            foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
            {
                string key = profile.baseItemId + "@" + rarity.stableKey;
                long seed = 150001L + index++;
                ItemBalancePreviewResult first = ItemBalanceWorkbenchCompiler.Preview(catalog, compiled,
                    profile.baseItemId, rarity.rarity, seed);
                ItemBalancePreviewResult second = ItemBalanceWorkbenchCompiler.Preview(catalog, compiled,
                    profile.baseItemId, rarity.rarity, seed);
                bool rollPass = first.RollResult?.isSuccess == true;
                bool projectionPass = first.ProjectionResult?.isSuccess == true;
                string firstSignature = first.RollResult?.snapshot?.BuildCanonicalSignature();
                string secondSignature = second.RollResult?.snapshot?.BuildCanonicalSignature();
                bool deterministicPass = !string.IsNullOrEmpty(firstSignature) && firstSignature == secondSignature;
                if (rollPass) rolls++;
                if (projectionPass) projections++;
                if (deterministicPass) deterministic++;
                AddSpec(specs, "roll." + key, "roll_150", "PASS", rollPass ? "PASS" : string.Join(";", first.Errors), rollPass);
                AddSpec(specs, "projection." + key, "projection_150", "PASS", projectionPass ? "PASS" : "FAIL", projectionPass);
                AddSpec(specs, "determinism." + key, "determinism_150", firstSignature ?? "non-empty signature",
                    secondSignature ?? "null", deterministicPass);
            }
        }

        private static void RunCsvAndUndoChecks(ItemBalanceWorkbenchCatalog source, List<SpecCase> specs)
        {
            string tempRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/ItemBalanceWorkbenchGuardFixQa"));
            Directory.CreateDirectory(tempRoot);
            string invalidI031 = Path.Combine(tempRoot, "invalid_i031.csv");
            string invalidUnknown = Path.Combine(tempRoot, "invalid_unknown.csv");
            string valid = Path.Combine(tempRoot, "valid_preview.csv");
            const string header = "baseItemId,rarity,statId,minUnits,maxUnits\n";
            File.WriteAllText(invalidI031, header + "I031,white,damage,1,2\n", Utf8);
            File.WriteAllText(invalidUnknown, header + "I999,white,damage,1,2\n", Utf8);

            ItemBalanceWorkbenchCatalog catalog = UnityEngine.Object.Instantiate(source);
            catalog.hideFlags = HideFlags.HideAndDontSave;
            List<ItemBalanceProfile> clones = source.profiles.Where(value => value != null).Select(value =>
            {
                ItemBalanceProfile clone = UnityEngine.Object.Instantiate(value);
                clone.hideFlags = HideFlags.HideAndDontSave;
                return clone;
            }).ToList();
            catalog.profiles = clones;
            try
            {
                ItemBalanceCsvImportPreview i031 = ItemBalanceWorkbenchCsvUtility.PreviewImport(catalog, invalidI031);
                ItemBalanceCsvImportPreview unknown = ItemBalanceWorkbenchCsvUtility.PreviewImport(catalog, invalidUnknown);
                AddSpec(specs, "csv.reject_i031", "csv_rejection", "rejected", i031.errors.Count == 0 ? "accepted" : "rejected", i031.errors.Count > 0);
                AddSpec(specs, "csv.reject_unknown", "csv_rejection", "rejected", unknown.errors.Count == 0 ? "accepted" : "rejected", unknown.errors.Count > 0);

                ItemBalanceProfile profile = catalog.FindProfile("I001");
                ItemBalanceRarityVersion version = profile?.FindVersion(ItemInstanceRarity.White);
                ItemBalanceRange range = version?.FindRange(profile?.primaryStatId);
                if (profile == null || version == null || range == null || range.maxUnits <= range.minUnits)
                {
                    AddSpec(specs, "csv.valid_preview", "csv_preview", "fixture available", "fixture missing", false);
                    AddSpec(specs, "undo.redo", "undo_redo", "PASS", "fixture missing", false);
                    return;
                }

                long originalMin = range.minUnits;
                long originalMax = range.maxUnits;
                long changedMax = originalMax - Math.Max(1, catalog.FindStat(range.statId)?.stepUnits ?? 1);
                File.WriteAllText(valid, header + string.Join(",", profile.baseItemId, "white", range.statId,
                    originalMin.ToString(CultureInfo.InvariantCulture), changedMax.ToString(CultureInfo.InvariantCulture)) + "\n", Utf8);
                ItemBalanceCsvImportPreview preview = ItemBalanceWorkbenchCsvUtility.PreviewImport(catalog, valid);
                AddSpec(specs, "csv.valid_preview", "csv_preview", "canApply=true", "canApply=" + preview.canApply, preview.canApply);

                Undo.RecordObject(profile, "GuardFix clone-only Undo Redo verification");
                range.maxUnits = changedMax;
                EditorUtility.SetDirty(profile);
                Undo.FlushUndoRecordObjects();
                bool changed = profile.FindVersion(ItemInstanceRarity.White)?.FindRange(profile.primaryStatId)?.maxUnits == changedMax;
                Undo.PerformUndo();
                bool undone = profile.FindVersion(ItemInstanceRarity.White)?.FindRange(profile.primaryStatId)?.maxUnits == originalMax;
                Undo.PerformRedo();
                bool redone = profile.FindVersion(ItemInstanceRarity.White)?.FindRange(profile.primaryStatId)?.maxUnits == changedMax;
                Undo.PerformUndo();
                bool restored = profile.FindVersion(ItemInstanceRarity.White)?.FindRange(profile.primaryStatId)?.maxUnits == originalMax;
                AddSpec(specs, "undo.redo", "undo_redo", "changed/undo/redo/restored=true",
                    changed + "/" + undone + "/" + redone + "/" + restored, changed && undone && redone && restored);
            }
            finally
            {
                foreach (ItemBalanceProfile clone in clones)
                    UnityEngine.Object.DestroyImmediate(clone);
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }

        private static void RunHistoricalRegressions(string projectRoot, List<RegressionEvidence> evidence,
            List<SpecCase> specs)
        {
            RunReportRegression("ItemInnerDataCatalog", "historical_item_regression",
                "Docs/V0.4/Reports/ItemInnerDataCatalogReport.md", "- Verification: PASS",
                ItemInnerDataCatalogVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunReportRegression("ItemSystemValidatorAndSnapshot", "historical_item_regression",
                "Docs/V0.4/Reports/ItemSystemValidatorAndSnapshotReport.md", "Result: PASS",
                ItemSystemValidatorAndSnapshotVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunReportRegression("BuildSynergyCore", "historical_item_regression",
                "Docs/V0.4/Reports/BuildSynergyCoreReport.md", "- Verification: PASS",
                BuildSynergyCoreVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunReportRegression("CoreAwakeningPreview", "historical_item_regression",
                "Docs/V0.4/Reports/CoreAwakeningPreviewReport.md", "- Verification: PASS",
                CoreAwakeningPreviewVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunReportRegression("ItemSkillTriggerContract", "historical_item_regression",
                "Docs/V0.4/Reports/ItemSkillTriggerContractReport.md", "- Verification: PASS",
                ItemSkillTriggerContractVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunReportRegression("ItemDetailProjectionComplete", "historical_item_regression",
                "Docs/V0.4/Reports/ItemDetailProjectionCompleteReport.md", "Result: PASS",
                ItemDetailProjectionCompleteVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunReportRegression("JuNian Lighting", "historical_item_regression",
                "Docs/V0.4/Reports/JuNianLightingAndAdjacentRelayReport.md", "- Verification: PASS",
                JuNianLightingAndAdjacentRelayVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunReportRegression("ArrayBonus", "historical_item_regression",
                "Docs/V0.4/Reports/ArrayBonusCellResolverReport.md", "- Verification: PASS",
                ArrayBonusCellResolverVerifier.VerifyMenu, projectRoot, evidence, specs);

            RunCsvRegression("Foundation", "159/159", "Docs/V0.4/Reports/ItemRarityInstanceFoundationSpec.csv",
                159, ItemRarityInstanceFoundationVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunCsvRegression("StatRange", "39/39", "Docs/V0.4/Reports/ItemStatRangeSchemaSpec.csv",
                39, ItemStatRangeSchemaVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunCsvRegression("CorePotential", "69/69", "Docs/V0.4/Reports/ItemCorePotentialAndBuildEligibilitySchemaSpec.csv",
                69, ItemCorePotentialAndBuildEligibilitySchemaVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunCsvRegression("AffixSchema", "88/88", "Docs/V0.4/Reports/ItemAffixPoolAndRangeSchemaSpec.csv",
                88, ItemAffixPoolAndRangeSchemaVerifier.VerifyMenu, projectRoot, evidence, specs);
            RunCsvRegression("RollEngine", "72/72", "Docs/V0.4/Reports/ItemInstanceRollEngineSpec.csv",
                72, ItemInstanceRollEngineVerifier.VerifyMenu, projectRoot, evidence, specs);

            DateTime dropStarted = DateTime.UtcNow;
            Exception dropException = Invoke(ItemDropGenerationSandboxVerifier.VerifyMenu);
            CsvResult drop = ReadCsvResult(projectRoot, "Docs/V0.4/Reports/ItemDropGenerationSandboxSpec.csv", dropStarted);
            CsvResult dropDet = ReadCsvResult(projectRoot, "Docs/V0.4/Reports/ItemDropGenerationSandboxDeterminism.csv", dropStarted);
            AddRegression("DropSandbox", "87/87 + Determinism 6/6", drop.Passed + "/" + drop.Total
                + " + Determinism " + dropDet.Passed + "/" + dropDet.Total, dropException == null && drop.Is(87) && dropDet.Is(6),
                dropStarted, dropException, evidence, specs);

            DateTime simulationStarted = DateTime.UtcNow;
            Exception simulationException = Invoke(ItemGenerationSimulationValidator.VerifyMenu);
            CsvResult simulation = ReadCsvResult(projectRoot, "Docs/V0.4/Reports/ItemGenerationSimulationValidatorSpec.csv", simulationStarted);
            CsvResult distribution = ReadCsvResult(projectRoot, "Docs/V0.4/Reports/ItemGenerationSimulationDistribution.csv", simulationStarted);
            CsvResult simulationDet = ReadCsvResult(projectRoot, "Docs/V0.4/Reports/ItemGenerationSimulationDeterminism.csv", simulationStarted);
            int violations = ReadViolationCount(projectRoot, "Docs/V0.4/Reports/ItemGenerationSimulationInvariantViolations.csv", simulationStarted);
            string simulationActual = simulation.Passed + "/" + simulation.Total + " + Distribution " + distribution.Passed + "/" + distribution.Total
                + " + Determinism " + simulationDet.Passed + "/" + simulationDet.Total + " + violations " + violations;
            bool simulationPass = simulationException == null && simulation.Is(65) && distribution.Is(46)
                && simulationDet.Is(12) && violations == 0;
            AddRegression("Simulation", "65/65 + Distribution 46/46 + Determinism 12/12 + violations 0",
                simulationActual, simulationPass, simulationStarted, simulationException, evidence, specs);

            RunCsvRegression("ProjectionContract", "156/156", "Docs/V0.4/Reports/ItemInstanceProjectionContractSpec.csv",
                156, ItemInstanceProjectionContractVerifier.VerifyMenu, projectRoot, evidence, specs);
        }

        private static void RunReportRegression(string caseId, string category, string relativePath,
            string passMarker, Action action, string projectRoot, List<RegressionEvidence> evidence, List<SpecCase> specs)
        {
            DateTime started = DateTime.UtcNow;
            Exception exception = Invoke(action);
            string path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            bool fresh = File.Exists(path) && File.GetLastWriteTimeUtc(path) >= started.AddSeconds(-3);
            bool marker = File.Exists(path) && File.ReadAllText(path).Contains(passMarker, StringComparison.Ordinal);
            string actual = exception != null ? exception.GetType().Name + ": " + exception.Message
                : "marker=" + marker + ", fresh=" + fresh;
            AddRegression(caseId, "PASS from current invocation", actual, exception == null && marker && fresh,
                started, exception, evidence, specs, category);
        }

        private static void RunCsvRegression(string caseId, string expected, string relativePath, int count,
            Action action, string projectRoot, List<RegressionEvidence> evidence, List<SpecCase> specs)
        {
            DateTime started = DateTime.UtcNow;
            Exception exception = Invoke(action);
            CsvResult result = ReadCsvResult(projectRoot, relativePath, started);
            AddRegression(caseId, expected, result.Passed + "/" + result.Total,
                exception == null && result.Is(count), started, exception, evidence, specs);
        }

        private static Exception Invoke(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return exception;
            }
        }

        private static void AddRegression(string caseId, string expected, string actual, bool pass,
            DateTime started, Exception exception, List<RegressionEvidence> evidence, List<SpecCase> specs,
            string category = "prerequisite_regression")
        {
            if (exception != null) actual += " / " + exception.GetType().Name + ": " + exception.Message;
            evidence.Add(new RegressionEvidence(caseId, expected, actual, pass, started));
            AddSpec(specs, "regression." + Slug(caseId), category, expected, actual, pass);
        }

        private static CsvResult ReadCsvResult(string projectRoot, string relativePath, DateTime started)
        {
            string path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path) || File.GetLastWriteTimeUtc(path) < started.AddSeconds(-3))
                return new CsvResult(0, 0, false);
            string[] lines = File.ReadAllLines(path).Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
            if (lines.Length < 2) return new CsvResult(0, 0, true);
            string[] header = ItemBalanceWorkbenchCsvUtility.ParseLine(lines[0]).ToArray();
            int resultIndex = Array.FindIndex(header, value => value.Equals("result", StringComparison.OrdinalIgnoreCase)
                || value.Equals("passed", StringComparison.OrdinalIgnoreCase)
                || value.Equals("isPass", StringComparison.OrdinalIgnoreCase)
                || value.Equals("validationResult", StringComparison.OrdinalIgnoreCase));
            int total = lines.Length - 1;
            int passed = 0;
            foreach (string line in lines.Skip(1))
            {
                string[] fields = ItemBalanceWorkbenchCsvUtility.ParseLine(line).ToArray();
                string value = resultIndex >= 0 && resultIndex < fields.Length ? fields[resultIndex] : fields.LastOrDefault();
                if (string.Equals(value, "PASS", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)) passed++;
            }
            return new CsvResult(total, passed, true);
        }

        private static int ReadViolationCount(string projectRoot, string relativePath, DateTime started)
        {
            string path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path) || File.GetLastWriteTimeUtc(path) < started.AddSeconds(-3)) return -1;
            foreach (string line in File.ReadAllLines(path).Skip(1))
            {
                string[] fields = ItemBalanceWorkbenchCsvUtility.ParseLine(line).ToArray();
                if (fields.Length >= 2 && fields[0] == "__SUMMARY__"
                    && int.TryParse(fields.Last(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)) return value;
            }
            return -1;
        }

        private static List<LeakScanResult> RunLeakScans(string projectRoot)
        {
            string[] roots =
            {
                Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/Items/Balance"),
                Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance")
            };
            return new List<LeakScanResult>
            {
                Scan("reward", roots, @"\b(?:RewardConfig|RewardService|RewardResolver|RewardPipeline)\b"),
                Scan("runflow", roots, @"\b(?:V02RunFlowController|RunFlowController|RunFlowService|MainTrialFlowService)\b"),
                Scan("inventory", roots, @"\b(?:InventoryService|InventoryController|InventoryRepository|PlayerInventory)\b"),
                Scan("savedata", roots, @"\b(?:SaveData|SaveService|SaveRepository|PlayerPrefs)\b"),
                Scan("battle", roots, @"\b(?:BattleSnapshot|BattleResolver|AutoCombatController|BattleService|UnifiedBattle)\b"),
                Scan("boss", roots, @"\b(?:BossConfig|BossController|BossService|MiniBoss)\b"),
                Scan("scene", roots, @"\b(?:SceneManager|EditorSceneManager|SceneAsset)\b|\.unity\b"),
                Scan("prefab", roots, @"\b(?:PrefabUtility|PrefabStage|PrefabAsset)\b|\.prefab\b"),
                Scan("buildsettings", roots, @"\b(?:EditorBuildSettings|BuildSettings|BuildPipeline)\b"),
                Scan("formal_runtime_wiring", roots, @"\b(?:RuntimeInitializeOnLoadMethod|InitializeOnLoad|Resources\.Load|Addressables\.LoadAssetAsync|ItemStatRangeSchemaCatalog|ItemAffixPoolAndRangeCatalog|ItemCorePotentialAndBuildEligibilityCatalog)\b")
            };
        }

        private static LeakScanResult Scan(string category, IEnumerable<string> roots, string pattern)
        {
            Regex regex = new(pattern, RegexOptions.CultureInvariant);
            List<string> matches = new();
            foreach (string root in roots.Where(Directory.Exists))
            foreach (string file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (file.EndsWith("ItemBalanceWorkbenchVerifier.cs", StringComparison.OrdinalIgnoreCase)) continue;
                int lineNumber = 0;
                foreach (string line in File.ReadLines(file))
                {
                    lineNumber++;
                    foreach (Match match in regex.Matches(line))
                        matches.Add(RelativeProjectPath(file) + ":" + lineNumber + " => " + match.Value);
                }
            }
            return new LeakScanResult(category, string.Join("; ", roots.Select(RelativeProjectPath)), pattern, matches);
        }

        private static void WriteReports(ItemBalanceWorkbenchCatalog catalog, IReadOnlyList<SpecCase> specs,
            IReadOnlyList<RegressionEvidence> regressions, IReadOnlyList<LeakScanResult> leakScans,
            IReadOnlyList<BoundaryEvidence> boundary, IReadOnlyList<string> failures,
            IReadOnlyList<string> warnings, int rolls, int projections, int deterministic)
        {
            string root = Path.Combine(ProjectRoot(), "Docs/V0.4/Reports");
            Directory.CreateDirectory(root);
            WriteCandidate150(catalog, Path.Combine(root, "ItemBalanceCandidate150.csv"));
            WriteStatRangeInventory(catalog, Path.Combine(root, "ItemBalanceStatRangeInventory.csv"));
            WriteSpec(specs, Path.Combine(root, "ItemBalanceWorkbenchSpec.csv"));
            WriteDictionary(catalog, Path.Combine(root, "ItemBalanceStatDictionary.csv"));
            WriteInventory(catalog, Path.Combine(root, "ItemBalanceAffixCoreBuildInventory.csv"));
            File.WriteAllText(Path.Combine(root, "ItemBalanceWorkbenchReport.md"), BuildReport(catalog, specs,
                regressions, boundary, failures, warnings, rolls, projections, deterministic), Utf8);
            File.WriteAllText(Path.Combine(root, "ItemBalanceWorkbenchLeakCheckReport.md"),
                BuildLeakReport(leakScans, boundary), Utf8);
            AssetDatabase.Refresh();
        }

        private static void WriteCandidate150(ItemBalanceWorkbenchCatalog catalog, string path)
        {
            StringBuilder csv = new();
            csv.AppendLine("baseItemId,rarity,versionKey,faMenTag,qiLeiTag,primaryStatId,secondaryStatId,statCount,fixedAffixId,randomPoolId,coreEligibleCount,coreVisibleCount,buildNoneWeight,buildFaMenWeight,buildQiLeiWeight,buildDualWeight,dataMaturity,validationResult");
            foreach (ItemBalanceProfile profile in catalog?.profiles.Where(value => value != null).OrderBy(value => value.baseItemId) ?? Enumerable.Empty<ItemBalanceProfile>())
            foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
            {
                ItemBalanceRarityVersion version = profile.FindVersion(rarity.rarity);
                ItemBalanceBuildPolicy build = catalog.FindBuildPolicy(rarity.rarity);
                Append(csv, profile.baseItemId, rarity.stableKey, version?.versionKey, profile.faMenTag,
                    profile.qiLeiTag, profile.primaryStatId, profile.secondaryStatId,
                    version?.statRanges.Count ?? 0, profile.fixedAffixId, profile.randomPoolId,
                    version?.eligibleCoreEffectIds.Count ?? 0, version?.visibleCoreEffectIds.Count ?? 0,
                    build?.noneWeight ?? 0, build?.faMenWeight ?? 0, build?.qiLeiWeight ?? 0,
                    build?.dualWeight ?? 0, version?.dataMaturity, version == null ? "FAIL" : "PASS");
            }
            File.WriteAllText(path, csv.ToString(), Utf8);
        }

        private static void WriteStatRangeInventory(ItemBalanceWorkbenchCatalog catalog, string path)
        {
            StringBuilder csv = new();
            csv.AppendLine("baseItemId,rarity,statId,minUnits,maxUnits,direction,unitKey,stepUnits,role,dataMaturity");
            foreach (ItemBalanceProfile profile in catalog?.profiles.Where(value => value != null).OrderBy(value => value.baseItemId) ?? Enumerable.Empty<ItemBalanceProfile>())
            foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
            foreach (ItemBalanceRange range in profile.FindVersion(rarity.rarity)?.statRanges.Where(value => value != null) ?? Enumerable.Empty<ItemBalanceRange>())
            {
                ItemBalanceStatDefinition definition = catalog.FindStat(range.statId);
                string role = range.statId == profile.primaryStatId ? "primary" : range.statId == profile.secondaryStatId ? "secondary" : "base";
                Append(csv, profile.baseItemId, rarity.stableKey, range.statId, range.minUnits, range.maxUnits,
                    definition?.direction, definition?.unitKey, definition?.stepUnits ?? 0, role,
                    ItemBalanceWorkbenchCatalog.BalanceCandidate);
            }
            File.WriteAllText(path, csv.ToString(), Utf8);
        }

        private static void WriteSpec(IEnumerable<SpecCase> specs, string path)
        {
            StringBuilder csv = new();
            csv.AppendLine("caseId,category,expected,actual,result");
            foreach (SpecCase spec in specs)
                Append(csv, spec.CaseId, spec.Category, spec.Expected, spec.Actual, spec.Pass ? "PASS" : "FAIL");
            File.WriteAllText(path, csv.ToString(), Utf8);
        }

        private static void WriteDictionary(ItemBalanceWorkbenchCatalog catalog, string path)
        {
            StringBuilder csv = new();
            csv.AppendLine("statId,displayName,unitKey,direction,decimalPlaces,stepUnits,roundingMode,dataMaturity");
            foreach (ItemBalanceStatDefinition value in catalog?.statDefinitions.Where(value => value != null).OrderBy(value => value.statId) ?? Enumerable.Empty<ItemBalanceStatDefinition>())
                Append(csv, value.statId, value.displayName, value.unitKey, value.direction, value.decimalPlaces,
                    value.stepUnits, value.roundingMode, ItemBalanceWorkbenchCatalog.BalanceCandidate);
            File.WriteAllText(path, csv.ToString(), Utf8);
        }

        private static void WriteInventory(ItemBalanceWorkbenchCatalog catalog, string path)
        {
            StringBuilder csv = new();
            csv.AppendLine("recordType,id,rarity,detailA,detailB,detailC,detailD,dataMaturity");
            foreach (ItemBalanceProfile profile in catalog?.profiles.Where(value => value != null).OrderBy(value => value.baseItemId) ?? Enumerable.Empty<ItemBalanceProfile>())
                Append(csv, "profile", profile.baseItemId, string.Empty, profile.fixedAffixId,
                    string.Join("|", profile.randomAffixes.Select(value => value.affixId + ":" + value.weight)),
                    profile.coreCandidates.Count, profile.randomPoolId, ItemBalanceWorkbenchCatalog.BalanceCandidate);
            foreach (ItemBalanceBuildPolicy build in catalog?.buildPolicies.Where(value => value != null) ?? Enumerable.Empty<ItemBalanceBuildPolicy>())
                Append(csv, "build", "candidate_build_" + build.rarity.ToStableKey(), build.rarity.ToStableKey(),
                    build.noneWeight, build.faMenWeight, build.qiLeiWeight, build.dualWeight, ItemBalanceWorkbenchCatalog.BalanceCandidate);
            foreach (ItemBalanceDropSegment drop in catalog?.dropSegments.Where(value => value != null) ?? Enumerable.Empty<ItemBalanceDropSegment>())
                Append(csv, "drop", drop.minStage + "-" + (drop.openEnded ? "+" : drop.maxStage.ToString()), string.Empty,
                    drop.whiteWeight, drop.greenWeight, drop.blueWeight, drop.purpleWeight + "/" + drop.orangeWeight,
                    ItemBalanceWorkbenchCatalog.BalanceCandidate);
            File.WriteAllText(path, csv.ToString(), Utf8);
        }

        private static string BuildReport(ItemBalanceWorkbenchCatalog catalog, IReadOnlyList<SpecCase> specs,
            IReadOnlyList<RegressionEvidence> regressions, IReadOnlyList<BoundaryEvidence> boundary,
            IReadOnlyList<string> failures, IReadOnlyList<string> warnings, int rolls, int projections,
            int deterministic)
        {
            int profiles = catalog?.profiles.Count(value => value != null) ?? 0;
            int versions = catalog?.profiles.Where(value => value != null).Sum(value => value.rarityVersions.Count) ?? 0;
            int statRanges = catalog?.profiles.Where(value => value != null)
                .Sum(value => value.rarityVersions.Sum(version => version.statRanges.Count)) ?? 0;
            int specPass = specs.Count(value => value.Pass);
            int specFail = specs.Count - specPass;
            StringBuilder report = new();
            report.AppendLine("# Item Balance Workbench GuardFix01 Report").AppendLine();
            report.AppendLine("- result: " + (failures.Count == 0 ? "PASS" : "FAIL"));
            report.AppendLine("- marker: " + (failures.Count == 0 ? PassMarker : "NOT_PRINTED"));
            report.AppendLine("- Spec: " + specPass + " PASS / " + specFail + " FAIL / " + specs.Count + " TOTAL");
            report.AppendLine("- profiles: " + profiles + "/30");
            report.AppendLine("- versions: " + versions + "/150");
            report.AppendLine("- stat ranges: " + statRanges + "/600");
            report.AppendLine("- rolls: " + rolls + "/150");
            report.AppendLine("- projections: " + projections + "/150");
            report.AppendLine("- deterministic: " + deterministic + "/150");
            report.AppendLine("- data maturity: " + (catalog?.dataMaturity ?? "null"));
            report.AppendLine("- I031 profile count: " + (catalog?.profiles.Count(value => value != null && value.baseItemId == "I031") ?? -1));
            report.AppendLine("- CSV/Undo fixture: in-memory ScriptableObject clones only; no candidate asset was saved.").AppendLine();
            report.AppendLine("## Current-invocation prerequisite regressions").AppendLine();
            report.AppendLine("| suite | expected | actual | invokedAtUtc | result |");
            report.AppendLine("|---|---:|---:|---|---|");
            foreach (RegressionEvidence value in regressions)
                report.AppendLine("| " + EscapeMarkdown(value.CaseId) + " | " + EscapeMarkdown(value.Expected) + " | "
                    + EscapeMarkdown(value.Actual) + " | " + value.StartedUtc.ToString("O", CultureInfo.InvariantCulture)
                    + " | " + (value.Pass ? "PASS" : "FAIL") + " |");
            report.AppendLine().AppendLine("## Protected boundary hashes").AppendLine();
            report.AppendLine("| category | scanRoot | before | after | result |");
            report.AppendLine("|---|---|---|---|---|");
            foreach (BoundaryEvidence value in boundary)
                report.AppendLine("| " + value.Category + " | " + EscapeMarkdown(value.ScanRoot) + " | `" + value.BeforeHash
                    + "` | `" + value.AfterHash + "` | " + (value.Pass ? "PASS" : "FAIL") + " |");
            report.AppendLine().AppendLine("## Warnings").AppendLine();
            report.Append(warnings.Count == 0 ? "- None\n" : string.Join("\n", warnings.Select(value => "- " + value)) + "\n");
            report.AppendLine().AppendLine("## Failures").AppendLine();
            report.Append(failures.Count == 0 ? "- None\n" : string.Join("\n", failures.Select(value => "- " + value)) + "\n");
            return report.ToString();
        }

        private static string BuildLeakReport(IReadOnlyList<LeakScanResult> scans,
            IReadOnlyList<BoundaryEvidence> boundary)
        {
            bool pass = scans.All(value => value.Pass) && boundary.All(value => value.Pass);
            StringBuilder report = new();
            report.AppendLine("# Item Balance Workbench GuardFix01 Leak Check").AppendLine();
            report.AppendLine("- result: " + (pass ? "PASS" : "FAIL"));
            report.AppendLine("- source scope: Items/Balance and Editor/ItemBalance; this verifier is excluded because it contains the QA scanner and regression entry points.");
            report.AppendLine("- interpretation: matchCount is a concrete forbidden-reference regex hit count, not an inferred runtime state.").AppendLine();
            report.AppendLine("| category | scanRoot | matchCount | result |");
            report.AppendLine("|---|---|---:|---|");
            foreach (LeakScanResult scan in scans)
                report.AppendLine("| " + scan.Category + " | " + EscapeMarkdown(scan.ScanRoot) + " | " + scan.MatchCount + " | " + (scan.Pass ? "PASS" : "FAIL") + " |");
            report.AppendLine().AppendLine("## Patterns and matches").AppendLine();
            foreach (LeakScanResult scan in scans)
            {
                report.AppendLine("### " + scan.Category).AppendLine();
                report.AppendLine("- pattern: `" + scan.Pattern.Replace("`", "\\`") + "`");
                if (scan.Matches.Count == 0) report.AppendLine("- matches: none");
                else foreach (string match in scan.Matches) report.AppendLine("- " + match);
                report.AppendLine();
            }
            report.AppendLine("## Protected file boundary checks").AppendLine();
            report.AppendLine("| category | scanRoot | beforeHash | afterHash | mismatchCount | result |");
            report.AppendLine("|---|---|---|---|---:|---|");
            foreach (BoundaryEvidence value in boundary)
                report.AppendLine("| " + value.Category + " | " + EscapeMarkdown(value.ScanRoot) + " | `" + value.BeforeHash
                    + "` | `" + value.AfterHash + "` | " + (value.Pass ? 0 : 1) + " | " + (value.Pass ? "PASS" : "FAIL") + " |");
            return report.ToString();
        }

        private static void AddSpec(List<SpecCase> specs, string caseId, string category,
            object expected, object actual, bool pass)
        {
            specs.Add(new SpecCase(caseId, category,
                Convert.ToString(expected, CultureInfo.InvariantCulture),
                Convert.ToString(actual, CultureInfo.InvariantCulture), pass));
        }

        private static void Append(StringBuilder csv, params object[] values)
        {
            csv.AppendLine(string.Join(",", values.Select(value => Escape(Convert.ToString(value, CultureInfo.InvariantCulture)))));
        }

        private static string Escape(string value)
        {
            string safe = value ?? string.Empty;
            return safe.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? "\"" + safe.Replace("\"", "\"\"") + "\"" : safe;
        }

        private static string EscapeMarkdown(string value) => (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        private static string Slug(string value) => Regex.Replace((value ?? string.Empty).ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
        private static string ProjectRoot() => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        private static string RelativeProjectPath(string path)
        {
            string root = ProjectRoot().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return Path.GetFullPath(path).StartsWith(root, StringComparison.OrdinalIgnoreCase)
                ? Path.GetFullPath(path).Substring(root.Length).Replace('\\', '/')
                : Path.GetFullPath(path).Replace('\\', '/');
        }

        private sealed class SpecCase
        {
            public SpecCase(string caseId, string category, string expected, string actual, bool pass)
            { CaseId = caseId; Category = category; Expected = expected; Actual = actual; Pass = pass; }
            public string CaseId { get; }
            public string Category { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Pass { get; }
        }

        private sealed class RegressionEvidence
        {
            public RegressionEvidence(string caseId, string expected, string actual, bool pass, DateTime startedUtc)
            { CaseId = caseId; Expected = expected; Actual = actual; Pass = pass; StartedUtc = startedUtc; }
            public string CaseId { get; }
            public string Expected { get; }
            public string Actual { get; }
            public bool Pass { get; }
            public DateTime StartedUtc { get; }
        }

        private readonly struct CsvResult
        {
            public CsvResult(int total, int passed, bool fresh) { Total = total; Passed = passed; Fresh = fresh; }
            public int Total { get; }
            public int Passed { get; }
            public bool Fresh { get; }
            public bool Is(int expected) => Fresh && Total == expected && Passed == expected;
        }

        private sealed class LeakScanResult
        {
            public LeakScanResult(string category, string scanRoot, string pattern, List<string> matches)
            { Category = category; ScanRoot = scanRoot; Pattern = pattern; Matches = matches; }
            public string Category { get; }
            public string ScanRoot { get; }
            public string Pattern { get; }
            public List<string> Matches { get; }
            public int MatchCount => Matches.Count;
            public bool Pass => MatchCount == 0;
        }

        private sealed class BoundaryEvidence
        {
            public BoundaryEvidence(string category, string scanRoot, string beforeHash, string afterHash)
            { Category = category; ScanRoot = scanRoot; BeforeHash = beforeHash; AfterHash = afterHash; }
            public string Category { get; }
            public string ScanRoot { get; }
            public string BeforeHash { get; }
            public string AfterHash { get; }
            public bool Pass => BeforeHash == AfterHash;
        }

        private sealed class ProtectedBoundarySnapshot
        {
            private readonly string _projectRoot;
            private readonly List<BoundaryRecord> _records;

            private ProtectedBoundarySnapshot(string projectRoot, List<BoundaryRecord> records)
            { _projectRoot = projectRoot; _records = records; }

            public static ProtectedBoundarySnapshot Capture(string projectRoot)
            {
                string verifier = Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance/ItemBalanceWorkbenchVerifier.cs");
                List<BoundaryRecord> records = new()
                {
                    BoundaryRecord.ForDirectory("candidate_assets", Path.Combine(projectRoot, "Assets/_Game/Configs/ItemBalanceWorkbench")),
                    BoundaryRecord.ForDirectory("workbench_function_files", Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/Editor/ItemBalance"), verifier),
                    BoundaryRecord.ForDirectory("balance_runtime_and_workbench_schema", Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/Items/Balance")),
                    BoundaryRecord.ForDirectory("protected_item_runtime_and_schema", Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/Items"), Path.Combine(projectRoot, "Assets/_Game/Scripts/TalismanBag/Items/Balance")),
                    BoundaryRecord.ForDirectory("scene_files", Path.Combine(projectRoot, "Assets/_Game/Scenes")),
                    BoundaryRecord.ForDirectory("prefab_files", Path.Combine(projectRoot, "Assets/_Game/Prefabs")),
                    BoundaryRecord.ForFile("build_settings", Path.Combine(projectRoot, "ProjectSettings/EditorBuildSettings.asset"))
                };
                foreach (BoundaryRecord record in records) record.BeforeHash = HashFiles(record.CurrentFiles());
                return new ProtectedBoundarySnapshot(projectRoot, records);
            }

            public IReadOnlyList<BoundaryEvidence> Compare()
            {
                return _records.Select(record => new BoundaryEvidence(record.Category,
                    RelativeProjectPath(record.ScanRoot), record.BeforeHash, HashFiles(record.CurrentFiles()))).ToArray();
            }

            private sealed class BoundaryRecord
            {
                public string Category;
                public string ScanRoot;
                public string[] Excludes;
                public bool IsSingleFile;
                public string BeforeHash;

                public static BoundaryRecord ForDirectory(string category, string root, params string[] excludes)
                {
                    return new BoundaryRecord { Category = category, ScanRoot = root, Excludes = excludes ?? Array.Empty<string>() };
                }

                public static BoundaryRecord ForFile(string category, string path)
                {
                    return new BoundaryRecord { Category = category, ScanRoot = path, Excludes = Array.Empty<string>(), IsSingleFile = true };
                }

                public string[] CurrentFiles()
                {
                    if (IsSingleFile) return File.Exists(ScanRoot) ? new[] { ScanRoot } : Array.Empty<string>();
                    return Directory.Exists(ScanRoot) ? Directory.GetFiles(ScanRoot, "*", SearchOption.AllDirectories)
                        .Where(file => !Excludes.Any(exclude => Path.GetFullPath(file).Equals(Path.GetFullPath(exclude), StringComparison.OrdinalIgnoreCase)
                            || Path.GetFullPath(file).StartsWith(Path.GetFullPath(exclude).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)))
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray() : Array.Empty<string>();
                }
            }
        }

        private sealed class RegressionSideEffectScope : IDisposable
        {
            private readonly string[] _roots;
            private readonly Dictionary<string, FileState> _files;

            private RegressionSideEffectScope(string[] roots, Dictionary<string, FileState> files)
            { _roots = roots; _files = files; }

            public static RegressionSideEffectScope Capture(string projectRoot)
            {
                string[] roots =
                {
                    Path.Combine(projectRoot, "Docs/V0.4/Reports"),
                    Path.Combine(projectRoot, "Assets/_Game/Prefabs"),
                    Path.Combine(projectRoot, "Assets/_Game/Scenes")
                };
                HashSet<string> packageReports = new(PackageReportNames, StringComparer.OrdinalIgnoreCase);
                Dictionary<string, FileState> files = new(StringComparer.OrdinalIgnoreCase);
                foreach (string root in roots.Where(Directory.Exists))
                foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                {
                    if (root.EndsWith("Reports", StringComparison.OrdinalIgnoreCase) && packageReports.Contains(Path.GetFileName(file))) continue;
                    files[file] = new FileState(File.ReadAllBytes(file), File.GetLastWriteTimeUtc(file));
                }
                return new RegressionSideEffectScope(roots, files);
            }

            public void Dispose()
            {
                HashSet<string> packageReports = new(PackageReportNames, StringComparer.OrdinalIgnoreCase);
                foreach (string root in _roots.Where(Directory.Exists))
                foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
                {
                    if (root.EndsWith("Reports", StringComparison.OrdinalIgnoreCase) && packageReports.Contains(Path.GetFileName(file))) continue;
                    if (!_files.ContainsKey(file)) File.Delete(file);
                }
                foreach (KeyValuePair<string, FileState> entry in _files)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(entry.Key) ?? ProjectRoot());
                    File.WriteAllBytes(entry.Key, entry.Value.Bytes);
                    File.SetLastWriteTimeUtc(entry.Key, entry.Value.LastWriteUtc);
                }
                AssetDatabase.Refresh();
            }

            private readonly struct FileState
            {
                public FileState(byte[] bytes, DateTime lastWriteUtc) { Bytes = bytes; LastWriteUtc = lastWriteUtc; }
                public byte[] Bytes { get; }
                public DateTime LastWriteUtc { get; }
            }
        }

        private static string HashFiles(IEnumerable<string> files)
        {
            using SHA256 sha = SHA256.Create();
            StringBuilder manifest = new();
            foreach (string file in files.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
            {
                manifest.Append(RelativeProjectPath(file)).Append('|');
                if (!File.Exists(file)) manifest.Append("MISSING");
                else
                {
                    using SHA256 fileSha = SHA256.Create();
                    manifest.Append(ToHex(fileSha.ComputeHash(File.ReadAllBytes(file))));
                }
                manifest.Append('\n');
            }
            return ToHex(sha.ComputeHash(Utf8.GetBytes(manifest.ToString())));
        }

        private static string ToHex(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", string.Empty);
    }
}
