using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    /// <summary>
    /// Read-only verifier for the report-only normalization rule survey.
    /// It never creates a mapping, a runtime producer, or a gameplay result.
    /// </summary>
    public static class BuildCapabilityNormalizationRuleSurveyVerifier
    {
        private const string PackageName = "V0.4-BuildCapabilityNormalizationRuleSurvey01";
        private const string GuardReceipt = "GUARD_PASS_BUILDCAPABILITYNORMALIZATIONRULESURVEY01";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReportPath = ReportDirectory + "/BuildCapabilityNormalizationRuleSurveyReport.md";
        private const string SourcePath = ReportDirectory + "/BuildCapabilityNormalizationSourceEligibilityMatrix.csv";
        private const string CandidatesPath = ReportDirectory + "/BuildCapabilityNormalizationRuleCandidates.csv";
        private const string ImpactPath = ReportDirectory + "/BuildCapabilityNormalizationC02ImpactMatrix.csv";
        private const string DecisionsPath = ReportDirectory + "/BuildCapabilityNormalizationUserDecisionSheet.csv";
        private const string LeakPath = ReportDirectory + "/BuildCapabilityNormalizationRuleSurveyLeakCheckReport.md";
        private const string C02MatrixPath = ReportDirectory + "/ItemEnemyMatchupMatrix.csv";

        private const string ExpectedItemHash = "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1";
        private const string ExpectedEnemyHash = "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae";
        private const string ExpectedC01Hash = "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a";
        private const string ExpectedC02Hash = "08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4";
        private const string ExpectedC02AHash = "a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb";
        private const string ExpectedBindingSignature = "sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428";
        private const string ExpectedUnitSignature = "sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673";
        private const string ExpectedRuntimeSignature = "sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a";
        private const string ExpectedAffixSignature = "sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f";
        private const string ExpectedRollSignature = "sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8";
        private const string ExpectedProjectionSignature = "sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2";
        private const string ExpectedScenesHash = "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b";
        private const string ExpectedPrefabsHash = "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087";
        private const string ExpectedBuildSettingsHash = "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59";

        private static readonly string[] OutputPaths =
        {
            MainReportPath,
            SourcePath,
            CandidatesPath,
            ImpactPath,
            DecisionsPath,
            LeakPath
        };

        private static readonly string[] CapabilityKeys =
        {
            "capability.energy_stability",
            "capability.control_power",
            "capability.cleanse_power",
            "capability.placement_shape",
            "capability.burst_window",
            "capability.clear_power"
        };

        private static readonly Dictionary<string, int> ExpectedCandidateCounts =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                { "capability.energy_stability", 2 },
                { "capability.control_power", 2 },
                { "capability.cleanse_power", 2 },
                { "capability.placement_shape", 0 },
                { "capability.burst_window", 2 },
                { "capability.clear_power", 2 }
            };

        private static readonly Dictionary<string, int> ExpectedCandidateImpact =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                { "capability.energy_stability", 32 },
                { "capability.control_power", 32 },
                { "capability.cleanse_power", 32 },
                { "capability.burst_window", 24 },
                { "capability.clear_power", 16 }
            };

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

        private static readonly string[] C02ProtectedFiles =
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

        private static readonly string[] C02AProtectedFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyReport.md",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapMatrix.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingSourceEvidence.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingDecisionList.csv",
            "Docs/V0.4/Reports/ItemCapabilityMappingGapSurveyLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/BuildCapabilityNormalizationRuleSurvey01/[QA Only] Verify Reports Read Only")]
        public static void VerifyMenu()
        {
            VerifyOrThrow(false, "Unity Editor menu");
        }
#endif

        public static void VerifyOffline()
        {
            VerifyOrThrow(false, "Pure C# offline verifier");
        }

        public static void VerifyBatch()
        {
            VerifyOrThrow(true, "Unity batch compile/verifier");
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

        private static void VerifyOrThrow(bool exitWhenDone, string mode)
        {
            bool passed = false;
            List<Check> checks = new List<Check>();
            try
            {
                string root = FindProjectRoot();
                ProtectionSnapshot before = CaptureProtection(root);
                RunReportChecks(root, checks);
                RunSourceEvidenceChecks(root, checks);
                RunLeakChecks(root, checks);
                ProtectionSnapshot after = CaptureProtection(root);
                RunProtectionChecks(checks, before, after);
                passed = checks.Count > 0 && checks.All(value => value.Passed);
            }
            catch (Exception exception)
            {
                checks.Add(new Check("verifier.exception", "no exception", exception.ToString(), false));
            }

            Console.WriteLine((passed
                    ? "BUILD_CAPABILITY_NORMALIZATION_RULE_SURVEY01_PASS "
                    : "BUILD_CAPABILITY_NORMALIZATION_RULE_SURVEY01_FAIL ")
                + checks.Count(value => value.Passed).ToString(CultureInfo.InvariantCulture)
                + "/" + checks.Count.ToString(CultureInfo.InvariantCulture)
                + " mode=" + mode);
            foreach (Check failed in checks.Where(value => !value.Passed))
            {
                Console.Error.WriteLine(failed.Id + " expected=" + failed.Expected + " actual=" + failed.Actual);
            }

#if UNITY_EDITOR
            if (exitWhenDone)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
#endif
            if (!passed)
            {
                throw new InvalidOperationException(PackageName + " verification failed.");
            }
        }

        private static void RunReportChecks(string root, ICollection<Check> checks)
        {
            foreach (string path in OutputPaths)
            {
                Add(checks, "output.exists." + Path.GetFileName(path), "true",
                    File.Exists(Absolute(root, path)).ToString(), File.Exists(Absolute(root, path)));
            }

            string main = Read(root, MainReportPath);
            string leak = Read(root, LeakPath);
            AddContains(checks, "main.package", main, PackageName);
            AddContains(checks, "main.guard-receipt", main, GuardReceipt);
            AddContains(checks, "main.six-unknown", main, "Survey completion state: `6/6 Unknown`");
            AddContains(checks, "main.combination-impact", main, "136");
            AddContains(checks, "main.blocked-row-reduction", main, "Projected blocked-row reduction: `0`");
            foreach (string capability in CapabilityKeys)
            {
                AddContains(checks, "main.capability." + capability, main, capability);
            }
            AddContains(checks, "leak.pass", leak, "Leak Count: `0`");

            CsvTable candidates = CsvTable.Read(Absolute(root, CandidatesPath));
            RequireHeaders(checks, "candidates", candidates,
                "candidateId", "capabilityKey", "planningMeaning", "sourceContracts", "sourceFields",
                "nativeValueDomain", "nativeUnit", "eligibilityRule", "runtimeFactGate", "aggregationRule",
                "normalizationFormula", "unresolvedParameters", "parameterAuthority", "clamp", "threshold",
                "cap", "untriggeredSemantics", "unplacedSemantics", "unlitSemantics", "missingFactSemantics",
                "invalidSemantics", "affectedEncounters", "affectedC02Rows", "projectedUnknownReferenceReduction",
                "projectedBlockedRowReduction", "remainingUnknowns", "implementationReadiness",
                "userDecisionRequired", "runtimeProducerDependency", "evidencePath", "evidenceSymbol",
                "advantages", "risks", "disposition");
            Add(checks, "candidates.total", "10", candidates.Rows.Count.ToString(CultureInfo.InvariantCulture),
                candidates.Rows.Count == 10);
            Add(checks, "candidates.unique-id", candidates.Rows.Count.ToString(CultureInfo.InvariantCulture),
                candidates.Rows.Select(row => candidates.Field(row, "candidateId")).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                candidates.Rows.Select(row => candidates.Field(row, "candidateId")).Distinct(StringComparer.Ordinal).Count() == candidates.Rows.Count);

            foreach (KeyValuePair<string, int> expected in ExpectedCandidateCounts)
            {
                int actual = candidates.Rows.Count(row => string.Equals(
                    candidates.Field(row, "capabilityKey"), expected.Key, StringComparison.Ordinal));
                Add(checks, "candidates.count." + expected.Key, expected.Value.ToString(CultureInfo.InvariantCulture),
                    actual.ToString(CultureInfo.InvariantCulture), actual == expected.Value);
            }
            foreach (string[] row in candidates.Rows)
            {
                string id = candidates.Field(row, "candidateId");
                string capability = candidates.Field(row, "capabilityKey");
                Add(checks, "candidate.disposition." + id, "KEEP_UNKNOWN",
                    candidates.Field(row, "disposition"), string.Equals(candidates.Field(row, "disposition"), "KEEP_UNKNOWN", StringComparison.Ordinal));
                Add(checks, "candidate.readiness." + id, "NOT_IMPLEMENTATION_READY",
                    candidates.Field(row, "implementationReadiness"), string.Equals(candidates.Field(row, "implementationReadiness"), "NOT_IMPLEMENTATION_READY", StringComparison.Ordinal));
                Add(checks, "candidate.user-decision." + id, "contains USER_DECISION_REQUIRED",
                    candidates.Field(row, "userDecisionRequired"), candidates.Field(row, "userDecisionRequired").Contains("USER_DECISION_REQUIRED"));
                Add(checks, "candidate.blocked-row." + id, "0", candidates.Field(row, "projectedBlockedRowReduction"),
                    string.Equals(candidates.Field(row, "projectedBlockedRowReduction"), "0", StringComparison.Ordinal));
                int expectedImpact;
                bool hasImpact = ExpectedCandidateImpact.TryGetValue(capability, out expectedImpact);
                Add(checks, "candidate.unknown-impact." + id, hasImpact ? expectedImpact.ToString(CultureInfo.InvariantCulture) : "0",
                    candidates.Field(row, "projectedUnknownReferenceReduction"), hasImpact && string.Equals(
                        candidates.Field(row, "projectedUnknownReferenceReduction"), expectedImpact.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
            }
            int producerCandidates = candidates.Rows.Count(row => string.Equals(
                candidates.Field(row, "runtimeProducerDependency"), "NEEDS_RUNTIME_PRODUCER", StringComparison.Ordinal));
            Add(checks, "candidates.runtime-producer-dependent", "8", producerCandidates.ToString(CultureInfo.InvariantCulture),
                producerCandidates == 8);

            CsvTable sources = CsvTable.Read(Absolute(root, SourcePath));
            RequireHeaders(checks, "sources", sources, "sourceId", "sourceContract", "sourceFields", "authority",
                "capabilityKeys", "nativeValueDomains", "nativeUnits", "eligibilityStatus", "qualifiedUse",
                "forbiddenUse", "requiredCompleteness", "identityOrJoinRule", "runtimeProducerStatus",
                "evidencePath", "evidenceSymbol");
            Add(checks, "sources.minimum-row-count", ">=14", sources.Rows.Count.ToString(CultureInfo.InvariantCulture),
                sources.Rows.Count >= 14);
            foreach (string contract in new[]
            {
                "ItemInstancePlacementBindingContractSnapshot.v1",
                "ItemCapabilityUnitContractSnapshot.v1",
                "ItemCapabilityRuntimeFactContractSnapshot.v1",
                "BuildCapabilityReadContract.v1"
            })
            {
                bool found = sources.Rows.Any(row => sources.Field(row, "sourceContract").Contains(contract));
                Add(checks, "sources.contract." + contract, "present", found ? "present" : "missing", found);
            }

            CsvTable decisions = CsvTable.Read(Absolute(root, DecisionsPath));
            RequireHeaders(checks, "decisions", decisions, "decisionId", "capabilityKey", "decisionTopic", "options",
                "candidateIds", "requiredForImplementation", "userDecision", "safeDefaultWithoutDecision",
                "dependencyIfApproved", "notes");
            Add(checks, "decisions.nonempty", ">=16", decisions.Rows.Count.ToString(CultureInfo.InvariantCulture),
                decisions.Rows.Count >= 16);
            int pending = decisions.Rows.Count(row => string.Equals(decisions.Field(row, "userDecision"), "PENDING", StringComparison.Ordinal));
            Add(checks, "decisions.all-pending", decisions.Rows.Count.ToString(CultureInfo.InvariantCulture),
                pending.ToString(CultureInfo.InvariantCulture), pending == decisions.Rows.Count);
            int keepUnknown = decisions.Rows.Count(row => string.Equals(decisions.Field(row, "safeDefaultWithoutDecision"), "KEEP_UNKNOWN", StringComparison.Ordinal));
            Add(checks, "decisions.safe-default", decisions.Rows.Count.ToString(CultureInfo.InvariantCulture),
                keepUnknown.ToString(CultureInfo.InvariantCulture), keepUnknown == decisions.Rows.Count);

            RunImpactChecks(root, checks);
        }

        private static void RunImpactChecks(string root, ICollection<Check> checks)
        {
            CsvTable baseline = CsvTable.Read(Absolute(root, C02MatrixPath));
            CsvTable impact = CsvTable.Read(Absolute(root, ImpactPath));
            RequireHeaders(checks, "impact", impact, "rowId", "scenarioId", "encounterId",
                "baselineUnknownCapabilities", "applicableSingleCandidateIds",
                "singleCandidateUnknownReferenceReductionEach", "singleCandidateProjectedBlockedRowReductionEach",
                "conditionalCombinationPolicy", "conditionalCombinationUnknownReferenceReduction",
                "conditionalCombinationProjectedBlockedRowReduction", "remainingDecisiveUnknowns", "analysisBoundary");
            Add(checks, "baseline.rows", "32", baseline.Rows.Count.ToString(CultureInfo.InvariantCulture), baseline.Rows.Count == 32);
            Add(checks, "impact.rows", "32", impact.Rows.Count.ToString(CultureInfo.InvariantCulture), impact.Rows.Count == 32);

            Dictionary<string, string[]> baselineByKey = baseline.Rows.ToDictionary(row =>
                baseline.Field(row, "scenarioId") + "|" + baseline.Field(row, "encounterId"), row => row, StringComparer.Ordinal);
            HashSet<string> impactKeys = new HashSet<string>(StringComparer.Ordinal);
            int combinationReduction = 0;
            int combinationBlockedReduction = 0;
            bool rowMatch = true;
            foreach (string[] row in impact.Rows)
            {
                string key = impact.Field(row, "scenarioId") + "|" + impact.Field(row, "encounterId");
                rowMatch &= impactKeys.Add(key) && baselineByKey.ContainsKey(key);
                if (baselineByKey.ContainsKey(key))
                {
                    rowMatch &= string.Equals(impact.Field(row, "baselineUnknownCapabilities"),
                        baseline.Field(baselineByKey[key], "unknownCapabilities"), StringComparison.Ordinal);
                }
                combinationReduction += ParseInt(impact.Field(row, "conditionalCombinationUnknownReferenceReduction"));
                combinationBlockedReduction += ParseInt(impact.Field(row, "conditionalCombinationProjectedBlockedRowReduction"));
                rowMatch &= string.Equals(impact.Field(row, "singleCandidateProjectedBlockedRowReductionEach"), "0", StringComparison.Ordinal);
            }
            Add(checks, "impact.baseline-exact-keys-and-unknowns", "true", rowMatch.ToString(), rowMatch);
            Add(checks, "impact.combination-unknown-reduction", "136", combinationReduction.ToString(CultureInfo.InvariantCulture),
                combinationReduction == 136);
            Add(checks, "impact.combination-blocked-row-reduction", "0", combinationBlockedReduction.ToString(CultureInfo.InvariantCulture),
                combinationBlockedReduction == 0);
        }

        private static void RunSourceEvidenceChecks(string root, ICollection<Check> checks)
        {
            Dictionary<string, string[]> evidence = new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemCapabilityUnitContract.cs", new[] { "control_point", "cleanse_stack", "target_count", "nian_point", "effect_basis_point", "nian_cost_basis_point" } },
                { "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs", new[] { "firstTriggerInBattle", "cleanseExtraStackCount", "consecutiveTriggerCount", "nianCostBefore", "nianCostAfter", "refundUnits" } },
                { "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs", new[] { "ItemInstancePlacementBindingContractSnapshot", "itemInstanceId", "placementId", "baseItemId" } },
                { "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs", new[] { "ShapeCells", "OccupiedCells", "isLit", "isCountedInBuild" } },
                { "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs", new[] { "capability.energy_stability", "capability.control_power", "capability.cleanse_power", "capability.placement_shape", "capability.burst_window", "capability.clear_power" } },
                { "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs", new[] { "MaximumValueBasisPoints", "10000" } }
            };
            foreach (KeyValuePair<string, string[]> pair in evidence)
            {
                string text = Read(root, pair.Key);
                foreach (string token in pair.Value)
                {
                    AddContains(checks, "evidence." + Path.GetFileName(pair.Key) + "." + token, text, token);
                }
            }

            AddContains(checks, "signature.binding", Read(root, ReportDirectory + "/ItemInstancePlacementBindingContractReport.md"), ExpectedBindingSignature);
            AddContains(checks, "signature.unit", Read(root, ReportDirectory + "/ItemCapabilityUnitContractReport.md"), ExpectedUnitSignature);
            string runtimeReport = Read(root, ReportDirectory + "/ItemCapabilityRuntimeFactContractReport.md");
            AddContains(checks, "signature.runtime", runtimeReport, ExpectedRuntimeSignature);
            AddContains(checks, "signature.affix", runtimeReport, ExpectedAffixSignature);
            AddContains(checks, "signature.roll", runtimeReport, ExpectedRollSignature);
            AddContains(checks, "signature.projection", runtimeReport, ExpectedProjectionSignature);
        }

        private static void RunLeakChecks(string root, ICollection<Check> checks)
        {
            string[] forbiddenTokens =
            {
                string.Concat("Readiness", "Band"),
                string.Concat("ITEM_ENEMY_", "READY"),
                string.Concat("ITEM_ENEMY_NOT_", "READY"),
                string.Concat("Unknown", "=0"),
                string.Concat("Unknown", " = 0"),
                string.Concat("UNKNOWN_AS_", "ZERO"),
                string.Concat("class ItemCapabilityRuntimeFact", "Producer"),
                string.Concat("DefaultBuildCapabilityNormalization", "Mapper")
            };
            int leakCount = 0;
            foreach (string path in OutputPaths)
            {
                string text = Read(root, path);
                foreach (string token in forbiddenTokens)
                {
                    leakCount += CountOrdinal(text, token);
                }
            }
            Add(checks, "leak.count", "0", leakCount.ToString(CultureInfo.InvariantCulture), leakCount == 0);
        }

        private static void RunProtectionChecks(
            ICollection<Check> checks,
            ProtectionSnapshot before,
            ProtectionSnapshot after)
        {
            AddProtection(checks, "item", before.Item, after.Item, 105, ExpectedItemHash);
            AddProtection(checks, "enemy", before.Enemy, after.Enemy, 89, ExpectedEnemyHash);
            AddProtection(checks, "c01", before.C01, after.C01, 9, ExpectedC01Hash);
            AddProtection(checks, "c02", before.C02, after.C02, 9, ExpectedC02Hash);
            AddProtection(checks, "c02a", before.C02A, after.C02A, 7, ExpectedC02AHash);
            AddProtection(checks, "scenes", before.Scenes, after.Scenes, 14, ExpectedScenesHash);
            AddProtection(checks, "prefabs", before.Prefabs, after.Prefabs, 16, ExpectedPrefabsHash);
            Add(checks, "protected.build-settings.accepted", ExpectedBuildSettingsHash, before.BuildSettings,
                string.Equals(before.BuildSettings, ExpectedBuildSettingsHash, StringComparison.Ordinal));
            Add(checks, "protected.build-settings.before-after", before.BuildSettings, after.BuildSettings,
                string.Equals(before.BuildSettings, after.BuildSettings, StringComparison.Ordinal));
        }

        private static void AddProtection(
            ICollection<Check> checks,
            string id,
            AggregateHash before,
            AggregateHash after,
            int expectedCount,
            string expectedHash)
        {
            Add(checks, "protected." + id + ".file-count", expectedCount.ToString(CultureInfo.InvariantCulture),
                before.FileCount.ToString(CultureInfo.InvariantCulture), before.FileCount == expectedCount);
            Add(checks, "protected." + id + ".accepted-hash", expectedHash, before.Hash,
                string.Equals(before.Hash, expectedHash, StringComparison.Ordinal));
            Add(checks, "protected." + id + ".before-after", before.Hash, after.Hash, before.Equals(after));
        }

        private static ProtectionSnapshot CaptureProtection(string root)
        {
            return new ProtectionSnapshot(
                AggregateItemExistingScope(root),
                AggregateDirectory(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem"),
                AggregateFiles(root, C01ProtectedFiles),
                AggregateFiles(root, C02ProtectedFiles),
                AggregateFiles(root, C02AProtectedFiles),
                AggregateDirectoryWithTrailingNewline(root, "Assets/_Game/Scenes"),
                AggregateDirectoryWithTrailingNewline(root, "Assets/_Game/Prefabs"),
                Sha256(File.ReadAllBytes(Absolute(root, "ProjectSettings/EditorBuildSettings.asset")), false));
        }

        private static AggregateHash AggregateDirectory(string root, string relativeDirectory)
        {
            string[] files = Directory.GetFiles(Absolute(root, relativeDirectory), "*", SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return Aggregate(root, files);
        }

        private static AggregateHash AggregateItemExistingScope(string root)
        {
            string itemRoot = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Items");
            string capabilityRoot = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Items/Capability")
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string capabilityMeta = Absolute(root, "Assets/_Game/Scripts/TalismanBag/Items/Capability.meta");
            string[] files = Directory.GetFiles(itemRoot, "*", SearchOption.AllDirectories)
                .Where(value => !value.StartsWith(capabilityRoot, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(value, capabilityMeta, StringComparison.OrdinalIgnoreCase))
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return Aggregate(root, files);
        }

        private static AggregateHash AggregateDirectoryWithTrailingNewline(string root, string relativeDirectory)
        {
            string[] files = Directory.GetFiles(Absolute(root, relativeDirectory), "*", SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            StringBuilder payload = new StringBuilder();
            foreach (string file in files)
            {
                payload.Append(Relative(root, file)).Append('|')
                    .Append(Sha256(File.ReadAllBytes(file), false)).Append('\n');
            }
            return new AggregateHash(files.Length, Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static AggregateHash AggregateFiles(string root, IEnumerable<string> relativeFiles)
        {
            string[] files = relativeFiles.Select(value => Absolute(root, value))
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return Aggregate(root, files);
        }

        private static AggregateHash Aggregate(string root, IReadOnlyList<string> files)
        {
            string payload = string.Join("\n", files.Select(file =>
                Relative(root, file) + "|" + Sha256(File.ReadAllBytes(file), true)));
            return new AggregateHash(files.Count, Sha256(Encoding.UTF8.GetBytes(payload), false));
        }

        private static string Read(string root, string relativePath)
        {
            return File.ReadAllText(Absolute(root, relativePath), Encoding.UTF8);
        }

        private static string FindProjectRoot()
        {
            foreach (string seed in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                DirectoryInfo current = new DirectoryInfo(seed);
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
            }
            throw new DirectoryNotFoundException("Unity project root was not found.");
        }

        private static string Absolute(string root, string relative)
        {
            return Path.Combine(root, (relative ?? string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string path)
        {
            string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(path);
            return full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? full.Substring(prefix.Length).Replace(Path.DirectorySeparatorChar, '/')
                : full.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string format = uppercase ? "X2" : "x2";
                return string.Concat(sha256.ComputeHash(bytes ?? new byte[0])
                    .Select(value => value.ToString(format, CultureInfo.InvariantCulture)));
            }
        }

        private static int CountOrdinal(string text, string token)
        {
            int count = 0;
            int index = 0;
            while (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(token)
                && (index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static int ParseInt(string value)
        {
            int result;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ? result : -1000000;
        }

        private static void AddContains(ICollection<Check> checks, string id, string actual, string token)
        {
            bool found = (actual ?? string.Empty).IndexOf(token, StringComparison.Ordinal) >= 0;
            Add(checks, id, "contains " + token, found ? "present" : "missing", found);
        }

        private static void RequireHeaders(ICollection<Check> checks, string id, CsvTable table, params string[] names)
        {
            foreach (string name in names)
            {
                bool found = table.Header.ContainsKey(name);
                Add(checks, id + ".header." + name, "present", found ? "present" : "missing", found);
            }
        }

        private static void Add(ICollection<Check> checks, string id, string expected, string actual, bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
        }

        private sealed class CsvTable
        {
            private CsvTable(Dictionary<string, int> header, List<string[]> rows)
            {
                Header = header;
                Rows = rows;
            }

            public Dictionary<string, int> Header { get; private set; }
            public List<string[]> Rows { get; private set; }

            public string Field(string[] row, string name)
            {
                int index;
                return Header.TryGetValue(name, out index) && index >= 0 && index < row.Length
                    ? row[index]
                    : string.Empty;
            }

            public static CsvTable Read(string path)
            {
                List<string[]> rows = File.ReadAllLines(path, Encoding.UTF8)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => ParseLine(value).ToArray()).ToList();
                if (rows.Count == 0)
                {
                    throw new InvalidDataException("CSV is empty: " + path);
                }
                Dictionary<string, int> header = rows[0]
                    .Select((value, index) => new { value, index })
                    .ToDictionary(value => value.value, value => value.index, StringComparer.Ordinal);
                return new CsvTable(header, rows.Skip(1).ToList());
            }

            private static IEnumerable<string> ParseLine(string line)
            {
                List<string> values = new List<string>();
                StringBuilder current = new StringBuilder();
                bool quoted = false;
                for (int index = 0; index < (line ?? string.Empty).Length; index++)
                {
                    char value = line[index];
                    if (value == '"')
                    {
                        if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                        {
                            current.Append('"');
                            index++;
                        }
                        else
                        {
                            quoted = !quoted;
                        }
                    }
                    else if (value == ',' && !quoted)
                    {
                        values.Add(current.ToString());
                        current.Length = 0;
                    }
                    else
                    {
                        current.Append(value);
                    }
                }
                values.Add(current.ToString());
                return values;
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

            public string Id { get; private set; }
            public string Expected { get; private set; }
            public string Actual { get; private set; }
            public bool Passed { get; private set; }
        }

        private sealed class AggregateHash : IEquatable<AggregateHash>
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash ?? string.Empty;
            }

            public int FileCount { get; private set; }
            public string Hash { get; private set; }

            public bool Equals(AggregateHash other)
            {
                return other != null && FileCount == other.FileCount
                    && string.Equals(Hash, other.Hash, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as AggregateHash);
            }

            public override int GetHashCode()
            {
                return (FileCount * 397) ^ Hash.GetHashCode();
            }
        }

        private sealed class ProtectionSnapshot
        {
            public ProtectionSnapshot(AggregateHash item, AggregateHash enemy, AggregateHash c01,
                AggregateHash c02, AggregateHash c02A, AggregateHash scenes, AggregateHash prefabs,
                string buildSettings)
            {
                Item = item;
                Enemy = enemy;
                C01 = c01;
                C02 = c02;
                C02A = c02A;
                Scenes = scenes;
                Prefabs = prefabs;
                BuildSettings = buildSettings;
            }

            public AggregateHash Item { get; private set; }
            public AggregateHash Enemy { get; private set; }
            public AggregateHash C01 { get; private set; }
            public AggregateHash C02 { get; private set; }
            public AggregateHash C02A { get; private set; }
            public AggregateHash Scenes { get; private set; }
            public AggregateHash Prefabs { get; private set; }
            public string BuildSettings { get; private set; }
        }
    }
}
