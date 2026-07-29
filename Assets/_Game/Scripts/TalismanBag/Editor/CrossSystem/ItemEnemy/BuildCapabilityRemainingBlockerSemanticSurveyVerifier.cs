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
    /// Editor-only, read-only verifier for the remaining-blocker semantic Survey.
    /// It validates reports and evidence; it never creates a rule, schema, mapping, or Producer.
    /// </summary>
    public static class BuildCapabilityRemainingBlockerSemanticSurveyVerifier
    {
        private const string PackageName = "V0.4-BuildCapabilityRemainingBlockerSemanticSurvey01";
        private const string GuardReceipt = "GUARD_PASS_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01";
        private const string EnemyGuardConfirmation =
            "ENEMY_GUARD_CONFIRM_BUILDCAPABILITYREMAININGBLOCKERSEMANTICSURVEY01";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReportPath = ReportDirectory + "/BuildCapabilityRemainingBlockerSemanticSurveyReport.md";
        private const string SemanticPath = ReportDirectory + "/BuildCapabilityRemainingBlockerSemanticMatrix.csv";
        private const string SourcePath = ReportDirectory + "/BuildCapabilityRemainingBlockerSourceEvidence.csv";
        private const string ImpactPath = ReportDirectory + "/BuildCapabilityRemainingBlockerC02Impact.csv";
        private const string DecisionPath = ReportDirectory + "/BuildCapabilityRemainingBlockerUserDecisionSheet.csv";
        private const string LeakPath = ReportDirectory + "/BuildCapabilityRemainingBlockerLeakCheckReport.md";
        private const string C02MatrixPath = ReportDirectory + "/ItemEnemyMatchupMatrix.csv";
        private const string N01ImpactPath = ReportDirectory + "/BuildCapabilityNormalizationC02ImpactMatrix.csv";

        private const string ExpectedSurveySignature =
            "sha256:de8f5d758bd8837d88621c33f80dc952c0b7c2dccd7b18f96e71c54649240e79";
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
        private const string ExpectedBindingSignature =
            "sha256:3d747ae0d365a3df0383c43cf8df7db80d6a9b4fb6a5e8d7e9bf0d7264cae428";
        private const string ExpectedUnitSignature =
            "sha256:e3c9a8741e5386e516c6886276ee81324d9ba456da6b73093b9df14ae9b48673";
        private const string ExpectedRuntimeSignature =
            "sha256:03b2bda459eb729da773848eaca00351c1f618bfbd1f98131cf868437ca05e7a";
        private const string ExpectedAffixSignature =
            "sha256:23d666597c31f15d88bbbf1adc4d456af4e67e5d04ed40b37a1aeebb5fafad4f";
        private const string ExpectedRollSignature =
            "sha256:d0e226d250adbff8d490d627215363e74262d6cacfc478480becc42de59938a8";
        private const string ExpectedProjectionSignature =
            "sha256:f137d20d107b39715c1fe2c66e2938010b8d52e9f528f9cc1982991e91843ac2";
        private const string ExpectedScenesHash =
            "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b";
        private const string ExpectedPrefabsHash =
            "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087";
        private const string ExpectedBuildSettingsHash =
            "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59";
        private const string ExpectedHead = "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba";

        private static readonly string[] OutputPaths =
        {
            MainReportPath,
            SemanticPath,
            SourcePath,
            ImpactPath,
            DecisionPath,
            LeakPath
        };

        private static readonly string[] CanonicalPaths =
        {
            SemanticPath,
            SourcePath,
            ImpactPath,
            DecisionPath
        };

        private static readonly string[] CapabilityKeys =
        {
            "capability.placement_shape",
            "capability.debuff_counter",
            "capability.interrupt_timing",
            "capability.spirit_lock"
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

        private static readonly string[] N01ProtectedFiles =
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

        private static readonly string[] C02AD1ProtectedFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs.meta",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionReport.md",
            "Docs/V0.4/Reports/ItemCapabilityRuleCandidates.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleImpactMatrix.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionSheet.csv",
            "Docs/V0.4/Reports/ItemCapabilityRuleDecisionLeakCheckReport.md"
        };

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/BuildCapabilityRemainingBlockerSemanticSurvey01/[QA Only] Verify Reports Read Only")]
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
                    ? "BUILD_CAPABILITY_REMAINING_BLOCKER_SEMANTIC_SURVEY01_PASS "
                    : "BUILD_CAPABILITY_REMAINING_BLOCKER_SEMANTIC_SURVEY01_FAIL ")
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
            AddContains(checks, "main.guard", main, GuardReceipt);
            AddContains(checks, "main.enemy-guard", main, EnemyGuardConfirmation);
            AddContains(checks, "main.four-unknown", main, "Survey completion state: `4/4 Unknown`");
            AddContains(checks, "main.actual-unknown-zero", main, "Actual Unknown-reference reduction: `0`");
            AddContains(checks, "main.actual-blocked-zero", main, "Actual blocked-row reduction: `0`");
            AddContains(checks, "main.remaining-64", main, "Remaining decisive Unknown references: `64`");
            AddContains(checks, "main.blocked-32", main, "Blocked rows: `32/32`");
            AddContains(checks, "main.head", main, ExpectedHead);
            AddContains(checks, "leak.zero", leak, "Leak Count: `0`");

            CsvTable semantic = CsvTable.Read(Absolute(root, SemanticPath));
            RequireHeaders(checks, "semantic", semantic, "capabilityKey", "uniquePrimaryClassification",
                "enemyPlanningMeaning", "currentStableSourceConclusion", "missingRequiredFact",
                "ineligibleInference", "recommendedRequirementChannel", "currentOfflineBlocking",
                "recommendedOfflineBlocking", "surveyOutputState", "noPressureSemantics",
                "incompleteApplicableSemantics", "completeApplicableSemantics",
                "buildCapabilityReadDisposition", "userDecisionRequired", "behaviorChange");
            Add(checks, "semantic.rows", "4", semantic.Rows.Count.ToString(CultureInfo.InvariantCulture),
                semantic.Rows.Count == 4);
            Add(checks, "semantic.keys", string.Join(";", CapabilityKeys),
                string.Join(";", semantic.Rows.Select(row => semantic.Field(row, "capabilityKey"))),
                CapabilityKeys.SequenceEqual(semantic.Rows.Select(row => semantic.Field(row, "capabilityKey"))));

            Dictionary<string, string> classifications = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "capability.placement_shape", "STRUCTURAL_PREDICATE" },
                { "capability.debuff_counter", "NO_CURRENT_STABLE_SOURCE" },
                { "capability.interrupt_timing", "RUNTIME_EVENT_SIGNAL" },
                { "capability.spirit_lock", "NO_CURRENT_STABLE_SOURCE" }
            };
            Dictionary<string, string> channels = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "capability.placement_shape", "LayoutResilience / EncounterStructuralPredicate" },
                { "capability.debuff_counter", "StatusMitigation / Resistance" },
                { "capability.interrupt_timing", "RuntimeCounterWindow / EventSignal" },
                { "capability.spirit_lock", "AntiDrainDefense structural/runtime channel" }
            };
            foreach (string[] row in semantic.Rows)
            {
                string capability = semantic.Field(row, "capabilityKey");
                CheckField(checks, semantic, row, "classification." + capability,
                    "uniquePrimaryClassification", classifications[capability]);
                CheckField(checks, semantic, row, "channel." + capability,
                    "recommendedRequirementChannel", channels[capability]);
                CheckField(checks, semantic, row, "current-offline." + capability,
                    "currentOfflineBlocking", "true");
                CheckField(checks, semantic, row, "recommended-offline." + capability,
                    "recommendedOfflineBlocking", "false");
                CheckField(checks, semantic, row, "survey-state." + capability,
                    "surveyOutputState", "Unknown");
                CheckField(checks, semantic, row, "behavior." + capability, "behaviorChange", "NONE");
                AddContains(checks, "decision-required." + capability,
                    semantic.Field(row, "userDecisionRequired"), "USER_DECISION_REQUIRED");
                AddContains(checks, "not-applicable." + capability,
                    semantic.Field(row, "noPressureSemantics"), "NotApplicable");
            }
            string[] interrupt = semantic.Rows.Single(row => semantic.Field(row, "capabilityKey")
                == "capability.interrupt_timing");
            CheckField(checks, semantic, interrupt, "interrupt.complete", "completeApplicableSemantics",
                "KnownTrue / KnownFalse");
            CheckField(checks, semantic, interrupt, "interrupt.offline", "buildCapabilityReadDisposition",
                "NotInChannel");
            string[] placement = semantic.Rows.Single(row => semantic.Field(row, "capabilityKey")
                == "capability.placement_shape");
            CheckField(checks, semantic, placement, "placement.incomplete", "incompleteApplicableSemantics",
                "side-channel Unknown");

            RunSourceTableChecks(checks, CsvTable.Read(Absolute(root, SourcePath)));
            RunImpactChecks(root, checks, CsvTable.Read(Absolute(root, ImpactPath)));
            RunDecisionChecks(checks, CsvTable.Read(Absolute(root, DecisionPath)));

            string forward = CanonicalSignature(root, false);
            string reversed = CanonicalSignature(root, true);
            Add(checks, "canonical.expected", ExpectedSurveySignature, forward,
                string.Equals(forward, ExpectedSurveySignature, StringComparison.Ordinal));
            Add(checks, "canonical.reverse-order", forward, reversed,
                string.Equals(forward, reversed, StringComparison.Ordinal));
            AddContains(checks, "main.canonical", main, forward);
            AddContains(checks, "leak.canonical", leak, forward);
        }

        private static void RunSourceTableChecks(ICollection<Check> checks, CsvTable source)
        {
            RequireHeaders(checks, "source", source, "sourceId", "capabilityKey", "evidencePath",
                "evidenceSymbol", "sourceKind", "stability", "qualifiedUse", "qualificationStatus",
                "missingFact", "forbiddenInference", "conclusion");
            Add(checks, "source.rows", ">=16", source.Rows.Count.ToString(CultureInfo.InvariantCulture),
                source.Rows.Count >= 16);
            Add(checks, "source.unique-id", source.Rows.Count.ToString(CultureInfo.InvariantCulture),
                source.Rows.Select(row => source.Field(row, "sourceId")).Distinct(StringComparer.Ordinal).Count()
                    .ToString(CultureInfo.InvariantCulture),
                source.Rows.Select(row => source.Field(row, "sourceId")).Distinct(StringComparer.Ordinal).Count()
                    == source.Rows.Count);
            foreach (string capability in CapabilityKeys)
            {
                Add(checks, "source.capability." + capability, "present",
                    source.Rows.Any(row => source.Field(row, "capabilityKey") == capability)
                        ? "present" : "missing",
                    source.Rows.Any(row => source.Field(row, "capabilityKey") == capability));
            }
            foreach (string status in new[]
            {
                "QUALIFIED_PLANNING_MEANING_ONLY",
                "QUALIFIED_STABLE_INPUT_FACT_ONLY",
                "QUALIFIED_IDENTITY_ONLY",
                "NO_CURRENT_STABLE_SOURCE",
                "INELIGIBLE",
                "REFERENCE_ONLY"
            })
            {
                Add(checks, "source.status." + status, "present",
                    source.Rows.Any(row => source.Field(row, "qualificationStatus") == status)
                        ? "present" : "missing",
                    source.Rows.Any(row => source.Field(row, "qualificationStatus") == status));
            }
            string combined = string.Join("\n", source.Rows.Select(row => string.Join("|", row)));
            foreach (string token in new[]
            {
                "shapeId or occupiedCells",
                "cleanse_power is not debuff_counter",
                "control_power potential is not successful timing",
                "energy_stability facts are not spirit_lock"
            })
            {
                AddContains(checks, "source.forbidden-inference." + token, combined, token);
            }
        }

        private static void RunImpactChecks(string root, ICollection<Check> checks, CsvTable impact)
        {
            RequireHeaders(checks, "impact", impact, "rowId", "scenarioId", "encounterId",
                "baselineUnknownCapabilities", "remainingDecisiveUnknowns", "actualRemainingUnknowns",
                "actualUnknownReferenceReduction", "actualBlockedRowReduction", "actualEvaluationStatus",
                "placementOnlyRemaining", "placementOnlyUnblocked", "debuffOnlyRemaining",
                "debuffOnlyUnblocked", "interruptOnlyRemaining", "interruptOnlyUnblocked",
                "spiritOnlyRemaining", "spiritOnlyUnblocked", "placementDebuffRemaining",
                "placementDebuffUnblocked", "placementInterruptRemaining", "placementInterruptUnblocked",
                "placementSpiritRemaining", "placementSpiritUnblocked", "allFourRemaining",
                "allFourUnblocked", "analysisBoundary");
            Add(checks, "impact.rows", "32", impact.Rows.Count.ToString(CultureInfo.InvariantCulture),
                impact.Rows.Count == 32);

            CsvTable c02 = CsvTable.Read(Absolute(root, C02MatrixPath));
            CsvTable n01 = CsvTable.Read(Absolute(root, N01ImpactPath));
            Dictionary<string, string[]> c02ByKey = c02.Rows.ToDictionary(row =>
                c02.Field(row, "scenarioId") + "|" + c02.Field(row, "encounterId"), row => row,
                StringComparer.Ordinal);
            Dictionary<string, string[]> n01ByKey = n01.Rows.ToDictionary(row =>
                n01.Field(row, "scenarioId") + "|" + n01.Field(row, "encounterId"), row => row,
                StringComparer.Ordinal);
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            int decisiveReferences = 0;
            int actualUnknownReduction = 0;
            int actualBlockedReduction = 0;
            bool exactBaselines = true;
            foreach (string[] row in impact.Rows)
            {
                string key = impact.Field(row, "scenarioId") + "|" + impact.Field(row, "encounterId");
                exactBaselines &= keys.Add(key) && c02ByKey.ContainsKey(key) && n01ByKey.ContainsKey(key);
                if (c02ByKey.ContainsKey(key))
                {
                    exactBaselines &= impact.Field(row, "baselineUnknownCapabilities")
                        == c02.Field(c02ByKey[key], "unknownCapabilities");
                }
                if (n01ByKey.ContainsKey(key))
                {
                    exactBaselines &= SetText(impact.Field(row, "remainingDecisiveUnknowns"))
                        == SetText(n01.Field(n01ByKey[key], "remainingDecisiveUnknowns"));
                }
                string baseline = SetText(impact.Field(row, "remainingDecisiveUnknowns"));
                decisiveReferences += SplitSet(baseline).Length;
                actualUnknownReduction += ParseInt(impact.Field(row, "actualUnknownReferenceReduction"));
                actualBlockedReduction += ParseInt(impact.Field(row, "actualBlockedRowReduction"));
                exactBaselines &= SetText(impact.Field(row, "actualRemainingUnknowns")) == baseline;
                exactBaselines &= impact.Field(row, "actualEvaluationStatus") == "BLOCKED_BY_UNKNOWN";
                ValidateConditional(checks, impact, row, "placementOnly",
                    "capability.placement_shape");
                ValidateConditional(checks, impact, row, "debuffOnly",
                    "capability.debuff_counter");
                ValidateConditional(checks, impact, row, "interruptOnly",
                    "capability.interrupt_timing");
                ValidateConditional(checks, impact, row, "spiritOnly",
                    "capability.spirit_lock");
                ValidateConditional(checks, impact, row, "placementDebuff",
                    "capability.placement_shape", "capability.debuff_counter");
                ValidateConditional(checks, impact, row, "placementInterrupt",
                    "capability.placement_shape", "capability.interrupt_timing");
                ValidateConditional(checks, impact, row, "placementSpirit",
                    "capability.placement_shape", "capability.spirit_lock");
                ValidateConditional(checks, impact, row, "allFour",
                    "capability.placement_shape", "capability.debuff_counter",
                    "capability.interrupt_timing", "capability.spirit_lock");
            }
            Add(checks, "impact.exact-row-baselines", "true", exactBaselines.ToString(), exactBaselines);
            Add(checks, "impact.unique-keys", "32", keys.Count.ToString(CultureInfo.InvariantCulture),
                keys.Count == 32);
            Add(checks, "impact.decisive-references", "64", decisiveReferences.ToString(CultureInfo.InvariantCulture),
                decisiveReferences == 64);
            Add(checks, "impact.actual-unknown-reduction", "0",
                actualUnknownReduction.ToString(CultureInfo.InvariantCulture), actualUnknownReduction == 0);
            Add(checks, "impact.actual-blocked-reduction", "0",
                actualBlockedReduction.ToString(CultureInfo.InvariantCulture), actualBlockedReduction == 0);
            Add(checks, "impact.blocked-rows", "32",
                impact.Rows.Count(row => impact.Field(row, "actualEvaluationStatus") == "BLOCKED_BY_UNKNOWN")
                    .ToString(CultureInfo.InvariantCulture),
                impact.Rows.All(row => impact.Field(row, "actualEvaluationStatus") == "BLOCKED_BY_UNKNOWN"));

            CheckReferences(checks, impact, "capability.placement_shape", 32);
            CheckReferences(checks, impact, "capability.debuff_counter", 8);
            CheckReferences(checks, impact, "capability.interrupt_timing", 8);
            CheckReferences(checks, impact, "capability.spirit_lock", 16);
            CheckUnblocked(checks, impact, "placementOnlyUnblocked", 0);
            CheckUnblocked(checks, impact, "debuffOnlyUnblocked", 0);
            CheckUnblocked(checks, impact, "interruptOnlyUnblocked", 0);
            CheckUnblocked(checks, impact, "spiritOnlyUnblocked", 0);
            CheckUnblocked(checks, impact, "placementDebuffUnblocked", 8);
            CheckUnblocked(checks, impact, "placementInterruptUnblocked", 8);
            CheckUnblocked(checks, impact, "placementSpiritUnblocked", 16);
            CheckUnblocked(checks, impact, "allFourUnblocked", 32);
        }

        private static void ValidateConditional(
            ICollection<Check> checks,
            CsvTable impact,
            string[] row,
            string prefix,
            params string[] removed)
        {
            string expected = RemoveSet(impact.Field(row, "remainingDecisiveUnknowns"), removed);
            string actual = SetText(impact.Field(row, prefix + "Remaining"));
            string expectedUnblocked = string.IsNullOrEmpty(expected) ? "true" : "false";
            string id = impact.Field(row, "rowId") + "." + prefix;
            Add(checks, "impact.set-subtraction." + id, expected, actual, expected == actual);
            CheckField(checks, impact, row, "impact.unblocked." + id,
                prefix + "Unblocked", expectedUnblocked);
        }

        private static void CheckReferences(
            ICollection<Check> checks,
            CsvTable impact,
            string capability,
            int expected)
        {
            int actual = impact.Rows.Count(row => SplitSet(impact.Field(row, "remainingDecisiveUnknowns"))
                .Contains(capability, StringComparer.Ordinal));
            Add(checks, "impact.references." + capability, expected.ToString(CultureInfo.InvariantCulture),
                actual.ToString(CultureInfo.InvariantCulture), actual == expected);
        }

        private static void CheckUnblocked(
            ICollection<Check> checks,
            CsvTable impact,
            string field,
            int expected)
        {
            int actual = impact.Rows.Count(row => impact.Field(row, field) == "true");
            Add(checks, "impact.conditional." + field, expected.ToString(CultureInfo.InvariantCulture),
                actual.ToString(CultureInfo.InvariantCulture), actual == expected);
        }

        private static void RunDecisionChecks(ICollection<Check> checks, CsvTable decisions)
        {
            RequireHeaders(checks, "decisions", decisions, "decisionId", "decisionTopic",
                "guardRecommendation", "options", "impactIfApproved", "prerequisiteContracts",
                "owningSystems", "status", "safeDefault", "notAuthorizedBySurvey");
            string[] expectedIds = { "D1", "D2", "D3", "D4", "D5", "D6" };
            Add(checks, "decisions.rows", "6", decisions.Rows.Count.ToString(CultureInfo.InvariantCulture),
                decisions.Rows.Count == 6);
            Add(checks, "decisions.ids", string.Join(";", expectedIds),
                string.Join(";", decisions.Rows.Select(row => decisions.Field(row, "decisionId"))),
                expectedIds.SequenceEqual(decisions.Rows.Select(row => decisions.Field(row, "decisionId"))));
            foreach (string[] row in decisions.Rows)
            {
                string id = decisions.Field(row, "decisionId");
                CheckField(checks, decisions, row, "decision.status." + id, "status",
                    "USER_DECISION_REQUIRED");
                CheckField(checks, decisions, row, "decision.default." + id, "safeDefault",
                    "KEEP_UNKNOWN");
                foreach (string field in new[]
                {
                    "decisionTopic", "guardRecommendation", "options", "impactIfApproved",
                    "prerequisiteContracts", "owningSystems", "notAuthorizedBySurvey"
                })
                {
                    Add(checks, "decision.nonempty." + id + "." + field, "non-empty",
                        decisions.Field(row, field), !string.IsNullOrWhiteSpace(decisions.Field(row, field)));
                }
            }
        }

        private static void RunSourceEvidenceChecks(string root, ICollection<Check> checks)
        {
            string vocabulary = Read(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/Vocabulary/DefaultEnemyMechanicVocabularyCatalog.cs");
            foreach (string token in new[]
            {
                "capability.placement_shape", "capability.debuff_counter",
                "capability.interrupt_timing", "capability.spirit_lock",
                "mechanic.polluted_tile", "mechanic.formation_eye_disruption",
                "mechanic.long_cast", "mechanic.energy_drain"
            })
            {
                AddContains(checks, "evidence.vocabulary." + token, vocabulary, token);
            }

            string item = Read(root, "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs");
            foreach (string token in new[]
            {
                "ShapeCells", "OccupiedCells", "isLit", "isCountedInBuild", "eyeCell", "coreCellWorld"
            })
            {
                AddContains(checks, "evidence.item." + token, item, token);
            }

            string binding = Read(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/ItemInstancePlacementBindingContract.cs");
            foreach (string token in new[] { "itemInstanceId", "placementId", "baseItemId" })
            {
                AddContains(checks, "evidence.binding." + token, binding, token);
            }

            string runtime = Read(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability/RuntimeFacts/ItemCapabilityRuntimeFactContract.cs");
            foreach (string token in new[]
            {
                "triggerSuccess", "cleanseSuccess", "cleanseExtraStackCount",
                "consecutiveTriggerCount", "nianCostBefore", "nianCostAfter", "refundUnits"
            })
            {
                AddContains(checks, "evidence.runtime-existing." + token, runtime, token);
            }
            foreach (string absent in new[]
            {
                "interruptSuccess", "debuffResistance", "spiritLockSuccess", "placementPressure"
            })
            {
                Add(checks, "evidence.runtime-absent." + absent, "absent",
                    runtime.Contains(absent) ? "present" : "absent", !runtime.Contains(absent));
            }

            string readContract = Read(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs");
            AddContains(checks, "evidence.bp-only", readContract, "MaximumValueBasisPoints = 10000");

            AddContains(checks, "signature.binding",
                Read(root, ReportDirectory + "/ItemInstancePlacementBindingContractReport.md"),
                ExpectedBindingSignature);
            AddContains(checks, "signature.unit",
                Read(root, ReportDirectory + "/ItemCapabilityUnitContractReport.md"),
                ExpectedUnitSignature);
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
                "READINESS_BAND",
                "ITEM_ENEMY_READY",
                "ITEM_ENEMY_NOT_READY",
                "MAPPING_IMPLEMENTED",
                "RUNTIME_PRODUCER_CREATED",
                "SCHEMA_MIGRATION_APPLIED",
                "D1_APPROVED",
                "D2_APPROVED",
                "D3_APPROVED",
                "D4_APPROVED",
                "D5_APPROVED",
                "D6_APPROVED",
                "KNOWN_ZERO_FROM_UNKNOWN",
                "SURVEY_ACTUAL_REDUCTION_NONZERO"
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
            AddProtection(checks, "n01", before.N01, after.N01, 8, ExpectedN01Hash);
            AddProtection(checks, "c02a-d1", before.C02AD1, after.C02AD1, 7, ExpectedC02AD1Hash);
            AddProtection(checks, "scenes", before.Scenes, after.Scenes, 14, ExpectedScenesHash);
            AddProtection(checks, "prefabs", before.Prefabs, after.Prefabs, 16, ExpectedPrefabsHash);
            Add(checks, "protected.build-settings.accepted", ExpectedBuildSettingsHash,
                before.BuildSettings, before.BuildSettings == ExpectedBuildSettingsHash);
            Add(checks, "protected.build-settings.before-after", before.BuildSettings,
                after.BuildSettings, before.BuildSettings == after.BuildSettings);
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
                before.Hash == expectedHash);
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
                AggregateFilesWithTrailingNewline(root, N01ProtectedFiles),
                AggregateFilesWithTrailingNewline(root, C02AD1ProtectedFiles),
                AggregateDirectoryWithTrailingNewline(root, "Assets/_Game/Scenes"),
                AggregateDirectoryWithTrailingNewline(root, "Assets/_Game/Prefabs"),
                Sha256(File.ReadAllBytes(Absolute(root, "ProjectSettings/EditorBuildSettings.asset")), false));
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
                    && value != capabilityMeta)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return Aggregate(root, files);
        }

        private static AggregateHash AggregateDirectory(string root, string relativeDirectory)
        {
            string[] files = Directory.GetFiles(Absolute(root, relativeDirectory), "*", SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return Aggregate(root, files);
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

        private static AggregateHash AggregateDirectoryWithTrailingNewline(string root, string relativeDirectory)
        {
            string[] files = Directory.GetFiles(Absolute(root, relativeDirectory), "*", SearchOption.AllDirectories)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
            return AggregateWithTrailingNewline(root, files);
        }

        private static AggregateHash AggregateFilesWithTrailingNewline(
            string root,
            IEnumerable<string> relativeFiles)
        {
            string[] files = relativeFiles.Select(value => Absolute(root, value))
                .OrderBy(value => Relative(root, value), StringComparer.Ordinal).ToArray();
            return AggregateWithTrailingNewline(root, files);
        }

        private static AggregateHash AggregateWithTrailingNewline(string root, IReadOnlyList<string> files)
        {
            StringBuilder payload = new StringBuilder();
            foreach (string file in files)
            {
                payload.Append(Relative(root, file)).Append('|')
                    .Append(Sha256(File.ReadAllBytes(file), false)).Append('\n');
            }
            return new AggregateHash(files.Count,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static string CanonicalSignature(string root, bool reverseInput)
        {
            IEnumerable<string> paths = reverseInput ? CanonicalPaths.Reverse() : CanonicalPaths;
            List<string> lines = new List<string>();
            foreach (string path in paths)
            {
                IEnumerable<string> rows = File.ReadAllLines(Absolute(root, path), Encoding.UTF8)
                    .Skip(1).Where(value => !string.IsNullOrWhiteSpace(value));
                if (reverseInput)
                {
                    rows = rows.Reverse();
                }
                foreach (string row in rows)
                {
                    lines.Add(Path.GetFileName(path) + "|" + row);
                }
            }
            string payload = string.Join("\n", lines.OrderBy(value => value, StringComparer.Ordinal));
            return "sha256:" + Sha256(Encoding.UTF8.GetBytes(payload), false);
        }

        private static string RemoveSet(string source, params string[] removed)
        {
            HashSet<string> excluded = new HashSet<string>(removed ?? new string[0], StringComparer.Ordinal);
            return string.Join(";", SplitSet(source).Where(value => !excluded.Contains(value))
                .OrderBy(value => value, StringComparer.Ordinal));
        }

        private static string SetText(string value)
        {
            return string.Join(";", SplitSet(value).OrderBy(item => item, StringComparer.Ordinal));
        }

        private static string[] SplitSet(string value)
        {
            return (value ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim()).Where(item => item.Length > 0)
                .Distinct(StringComparer.Ordinal).ToArray();
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
            string prefix = Path.GetFullPath(root).TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
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

        private static int ParseInt(string value)
        {
            int result;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                ? result : -1000000;
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

        private static void CheckField(
            ICollection<Check> checks,
            CsvTable table,
            string[] row,
            string id,
            string field,
            string expected)
        {
            string actual = table.Field(row, field);
            Add(checks, id, expected, actual, actual == expected);
        }

        private static void AddContains(ICollection<Check> checks, string id, string actual, string token)
        {
            bool found = (actual ?? string.Empty).IndexOf(token, StringComparison.Ordinal) >= 0;
            Add(checks, id, "contains " + token, found ? "present" : "missing", found);
        }

        private static void RequireHeaders(
            ICollection<Check> checks,
            string id,
            CsvTable table,
            params string[] names)
        {
            foreach (string name in names)
            {
                bool found = table.Header.ContainsKey(name);
                Add(checks, id + ".header." + name, "present", found ? "present" : "missing", found);
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
                    ? row[index] : string.Empty;
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
                return other != null && FileCount == other.FileCount && Hash == other.Hash;
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
            public ProtectionSnapshot(
                AggregateHash item,
                AggregateHash enemy,
                AggregateHash c01,
                AggregateHash c02,
                AggregateHash c02A,
                AggregateHash n01,
                AggregateHash c02AD1,
                AggregateHash scenes,
                AggregateHash prefabs,
                string buildSettings)
            {
                Item = item;
                Enemy = enemy;
                C01 = c01;
                C02 = c02;
                C02A = c02A;
                N01 = n01;
                C02AD1 = c02AD1;
                Scenes = scenes;
                Prefabs = prefabs;
                BuildSettings = buildSettings;
            }

            public AggregateHash Item { get; private set; }
            public AggregateHash Enemy { get; private set; }
            public AggregateHash C01 { get; private set; }
            public AggregateHash C02 { get; private set; }
            public AggregateHash C02A { get; private set; }
            public AggregateHash N01 { get; private set; }
            public AggregateHash C02AD1 { get; private set; }
            public AggregateHash Scenes { get; private set; }
            public AggregateHash Prefabs { get; private set; }
            public string BuildSettings { get; private set; }
        }
    }
}
