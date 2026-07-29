using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class ItemCapabilityRuleDecisionVerifier
    {
        private const string PackageName = "V0.4-ItemCapabilityRuleDecision01";
        private const string GuardReceipt = "GUARD_PASS_ITEMCAPABILITYRULEDECISION01";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string VerifierPath =
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityRuleDecisionVerifier.cs";
        private const string MainReportPath = ReportDirectory + "/ItemCapabilityRuleDecisionReport.md";
        private const string CandidatesPath = ReportDirectory + "/ItemCapabilityRuleCandidates.csv";
        private const string ImpactPath = ReportDirectory + "/ItemCapabilityRuleImpactMatrix.csv";
        private const string DecisionSheetPath = ReportDirectory + "/ItemCapabilityRuleDecisionSheet.csv";
        private const string LeakPath = ReportDirectory + "/ItemCapabilityRuleDecisionLeakCheckReport.md";

        private const string MatrixPath = ReportDirectory + "/ItemEnemyMatchupMatrix.csv";
        private const string CoveragePath = ReportDirectory + "/ItemEnemyMatchupCapabilityCoverage.csv";
        private const string BlockersPath = ReportDirectory + "/ItemEnemyMatchupUnknownBlockers.csv";
        private const string CandidateSourcePath =
            "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs";
        private const string CandidateAssetPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ProjectionContractPath =
            "Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs";
        private const string AffixSchemaPath =
            "Assets/_Game/Scripts/TalismanBag/Items/Generation/Affixes/ItemAffixPoolAndRangeSchema.cs";
        private const string ItemSnapshotPath =
            "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs";
        private const string CapabilityPrimitivesPath =
            "Assets/_Game/Scripts/TalismanBag/EnemySystem/CapabilityRead/BuildCapabilityReadPrimitives.cs";
        private const string C01AdapterPath =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs";

        private const string ExpectedItemHash =
            "7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663";
        private const string ExpectedEnemyHash =
            "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae";
        private const string ExpectedC01Hash =
            "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a";
        private const string ExpectedC02Hash =
            "08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4";
        private const string ExpectedC02AHash =
            "a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb";
        private const int ExpectedItemFileCount = 105;
        private const int ExpectedEnemyFileCount = 89;

        private static readonly string[] OutputPaths =
        {
            MainReportPath,
            CandidatesPath,
            ImpactPath,
            DecisionSheetPath,
            LeakPath
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
        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/ItemCapabilityRuleDecision01/[QA Only] Verify And Write Reports")]
        public static void VerifyMenu()
        {
            VerifyAndWrite(false, "Unity Editor menu");
        }
#endif

        public static void VerifyOffline()
        {
            VerifyAndWrite(false, "Pure C# offline verifier");
        }

        public static void VerifyBatch()
        {
            VerifyAndWrite(true, "Unity batch compile/verifier");
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

        private static void VerifyAndWrite(bool exitWhenDone, string mode)
        {
            string root = FindProjectRoot();
            ProtectionSnapshot before = CaptureProtection(root);
            List<DecisionRow> decisions = BuildDecisions();
            List<CandidateRow> candidates = BuildCandidates();
            List<MatrixRow> matrix = ReadMatrix(root);
            List<ImpactRow> impacts = BuildImpacts(decisions, candidates, matrix);
            List<Check> checks = new List<Check>();
            List<LeakRow> leaks = new List<LeakRow>();

            try
            {
                RunDecisionChecks(checks, decisions, candidates);
                RunC02Checks(checks, root, decisions, candidates, matrix, impacts);
                RunSourceChecks(checks, root);
                RunLeakChecks(checks, leaks, root);
                ProtectionSnapshot after = CaptureProtection(root);
                RunProtectionChecks(checks, before, after);
                RunOutputScopeChecks(checks, root);

                Dictionary<string, string> first = RenderReports(
                    mode, decisions, candidates, impacts, checks, leaks, before, after);
                Dictionary<string, string> second = RenderReports(
                    mode, decisions, candidates, impacts, checks, leaks, before, after);
                bool deterministic = first.Count == second.Count && first.All(pair =>
                    second.TryGetValue(pair.Key, out string value)
                    && string.Equals(pair.Value, value, StringComparison.Ordinal));
                Add(checks, "reports.render-deterministic", "true", deterministic.ToString(), deterministic);

                Dictionary<string, string> final = RenderReports(
                    mode, decisions, candidates, impacts, checks, leaks, before, after);
                foreach (KeyValuePair<string, string> pair in final)
                {
                    WriteUtf8(Absolute(root, pair.Key), pair.Value);
                }
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
                ProtectionSnapshot afterFailure = CaptureProtection(root);
                Dictionary<string, string> failure = RenderReports(
                    mode, decisions, candidates, impacts, checks, leaks, before, afterFailure);
                foreach (KeyValuePair<string, string> pair in failure)
                {
                    WriteUtf8(Absolute(root, pair.Key), pair.Value);
                }
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine((pass
                ? "ITEM_CAPABILITY_RULE_DECISION_PASS "
                : "ITEM_CAPABILITY_RULE_DECISION_FAIL ")
                + checks.Count(value => value.Passed).ToString(CultureInfo.InvariantCulture)
                + "/" + checks.Count.ToString(CultureInfo.InvariantCulture));

#if UNITY_EDITOR
            if (exitWhenDone && Application.isBatchMode)
            {
                EditorApplication.Exit(pass ? 0 : 1);
            }
#endif
            if (!pass)
            {
                throw new InvalidOperationException("Item capability rule decision verification failed.");
            }
        }

        private static List<DecisionRow> BuildDecisions()
        {
            return new List<DecisionRow>
            {
                new DecisionRow(
                    "ICRD01-D01", "capability.energy_stability", 11, 32, 4,
                    "dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross",
                    "affix_nian_efficiency schema definition and candidate payload; affix_trigger_refund candidate payload",
                    "affix_nian_efficiency: schema basisPoint but payload point; affix_trigger_refund: point",
                    "No safe eligibility rule can be evaluated before the nian-efficiency schema/payload conflict is resolved; both source effects are conditional.",
                    "NO_UNIT_SAFE_AGGREGATION",
                    "NO_UNIT_SAFE_CONVERSION; no ratio is selected",
                    "NO_UNIT_SAFE_CLAMP_OR_THRESHOLD",
                    "Any missing, conflicting, incomplete, or conditional source remains Unknown.",
                    "Known Zero is unavailable while the source contract conflicts; absence is not zero.",
                    "YES: before_trigger/nian_cost_gt_0 and after_trigger/trigger_success are runtime conditions.",
                    "Schema/payload unit conflict plus two different resource semantics; a guessed ratio would corrupt the BP contract.",
                    "NO_UNIT_SAFE_RULE / KEEP_UNKNOWN", "KEEP_UNKNOWN",
                    CandidateAssetPath + ";" + CandidateSourcePath,
                    "affix_nian_efficiency unitKey/valueUnitKey; BuildRandomAffixDictionary resource candidates"),
                new DecisionRow(
                    "ICRD01-D02", "capability.control_power", 10, 32, 4,
                    "dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross",
                    "ItemInstanceProjectionContractSnapshot.Affixes[affix_control_up].rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey",
                    "point",
                    "A complete projection and schema could identify a lit placed affix source, but eligibility does not create a point-to-BP scale.",
                    "NO_UNIT_SAFE_AGGREGATION until conversion and cross-item stacking are approved",
                    "NO_UNIT_SAFE_CONVERSION; point-to-BP ratio is absent",
                    "NO_EVIDENCE_BASED_THRESHOLD",
                    "Missing projection, schema, binding, placement, or conversion remains Unknown.",
                    "Known Zero is unavailable until a complete rule and complete eligible source set exist.",
                    "NO for the always/NONE payload itself; current static facts are sufficient only to identify a source, not to convert it.",
                    "The unconditional target is direct, but the generic 0..10000 BP capability scale supplies no point normalization.",
                    "NO_UNIT_SAFE_RULE / KEEP_UNKNOWN", "KEEP_UNKNOWN",
                    CandidateSourcePath + ";" + ProjectionContractPath,
                    "BuildRandomAffixDictionary / affix_control_up; ItemInstanceProjectionAffixSnapshot.rawUnits"),
                new DecisionRow(
                    "ICRD01-D03", "capability.cleanse_power", 8, 32, 4,
                    "dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross",
                    "ItemInstanceProjectionContractSnapshot.Affixes[affix_cleanse_up].rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey",
                    "stack",
                    "A source would require a complete lit placement binding and a successful cleanse runtime event; static possession is not eligibility.",
                    "NO_UNIT_SAFE_AGGREGATION until stack conversion and event-scoped stacking are approved",
                    "NO_UNIT_SAFE_CONVERSION; stack-to-BP ratio is absent",
                    "NO_EVIDENCE_BASED_THRESHOLD",
                    "Missing runtime success, projection, schema, binding, placement, or conversion remains Unknown.",
                    "No event or missing event evidence is Unknown, not zero; Known Zero requires a future complete event fact and approved rule.",
                    "YES: on_cleanse/cleanse_success.",
                    "Conditional event semantics and stack units both block a static BP rule.",
                    "NO_UNIT_SAFE_RULE / KEEP_UNKNOWN", "KEEP_UNKNOWN",
                    CandidateSourcePath + ";" + ProjectionContractPath,
                    "BuildRandomAffixDictionary / affix_cleanse_up; ItemInstanceProjectionAffixSnapshot.rawUnits"),
                new DecisionRow(
                    "ICRD01-D04", "capability.placement_shape", 7, 32, 4,
                    "dev_encounter_3_10_cleanse_corner;dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross",
                    "ItemSystemCatalogItemSnapshot.shapeId/ShapeCells; ItemSystemPlacementSnapshot.OccupiedCells/isLit/isCountedInBuild/ActiveCoreEffectIds",
                    "cells; booleans; categorical IDs",
                    "The snapshot can prove placement and lighting, but no approved subset says whether size, compactness, lighting, Build membership, or core activity constitutes this capability.",
                    "NO_SEMANTICALLY_SAFE_AGGREGATION",
                    "NO_UNIT_SAFE_CONVERSION from geometry/categorical facts to BP",
                    "NO_APPROVED_DIRECTION_DENOMINATOR_OR_THRESHOLD",
                    "Missing snapshot or binding remains Unknown; a present shape without an approved meaning also remains Unknown.",
                    "Empty placements cannot become Known Zero before the capability meaning and complete input boundary are approved.",
                    "NO for current snapshot facts; YES only if a later rule chooses a runtime-only placement outcome, which this package does not.",
                    "Stable geometry exists, but benefit direction and normalization do not; inventing either would create game rules.",
                    "NO_UNIT_SAFE_RULE / KEEP_UNKNOWN", "KEEP_UNKNOWN",
                    ItemSnapshotPath,
                    "ItemSystemCatalogItemSnapshot; ItemSystemPlacementSnapshot"),
                new DecisionRow(
                    "ICRD01-D05", "capability.burst_window", 5, 24, 3,
                    "dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross",
                    "ItemInstanceProjectionContractSnapshot.Affixes[affix_first_trigger_bonus].rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey; ItemSystemPlacementSnapshot.isLit",
                    "basisPoint",
                    "Complete valid projection and affix schema; explicit instance-to-placement binding; same base item; placement isLit=true; future stable runtime fact proves on_first_trigger and first_trigger_in_battle for that source. Main-Build membership is not added without evidence.",
                    "DIRECT_BASIS_POINT_SATURATING_SUM_BY_QUALIFIED_ITEM_INSTANCE",
                    "identity BP conversion",
                    "reject negative as Unknown; saturating clamp to inclusive 0..10000",
                    "Missing or invalid projection/schema/binding/placement/runtime trigger fact remains Unknown; unit mismatch remains Unknown.",
                    "Only after approval and a complete stable runtime fact: complete eligible evaluation with zero qualifying fired sources may be Known Zero. Today it remains Unknown.",
                    "YES: on_first_trigger/first_trigger_in_battle; the condition may not be projected as unconditionally active.",
                    "The unit is safe, but payload maturity is BALANCE_CANDIDATE/NOT_BATTLE_CONNECTED and no formal runtime trigger fact is exposed; sum semantics also require user/Guard approval.",
                    "NEEDS_ITEM_GUARD_FACT / KEEP_UNKNOWN", "KEEP_UNKNOWN",
                    CandidateSourcePath + ";" + CandidateAssetPath + ";" + C01AdapterPath,
                    "BuildRandomAffixDictionary / affix_first_trigger_bonus; DIRECT_BASIS_POINT_SATURATING_SUM_0_10000"),
                new DecisionRow(
                    "ICRD01-D06", "capability.clear_power", 2, 16, 2,
                    "dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross",
                    "ItemInstanceProjectionContractSnapshot.Affixes[affix_chain_target].rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey",
                    "count",
                    "A source would require a complete lit placement binding plus a stable runtime fact proving on_consecutive_trigger and chain_count_3; static possession is not eligibility.",
                    "NO_UNIT_SAFE_AGGREGATION until target-count conversion and event-scoped stacking are approved",
                    "NO_UNIT_SAFE_CONVERSION; count-to-BP ratio is absent",
                    "NO_EVIDENCE_BASED_THRESHOLD beyond the source condition, which still has no BP meaning",
                    "Missing runtime chain fact, projection, schema, binding, placement, or conversion remains Unknown.",
                    "No chain event or missing chain evidence is Unknown, not zero; Known Zero requires a future complete event fact and approved rule.",
                    "YES: on_consecutive_trigger/chain_count_3.",
                    "Extra-target count may relate to clear breadth, but neither semantic equivalence nor normalization is contracted.",
                    "NO_UNIT_SAFE_RULE / KEEP_UNKNOWN", "KEEP_UNKNOWN",
                    CandidateSourcePath + ";" + ProjectionContractPath,
                    "BuildRandomAffixDictionary / affix_chain_target; ItemInstanceProjectionAffixSnapshot.rawUnits")
            };
        }

        private static List<CandidateRow> BuildCandidates()
        {
            return new List<CandidateRow>
            {
                new CandidateRow(
                    "ICRD01-D05", "capability.burst_window", "ICRD01-BURST-A",
                    "ItemInstanceProjectionContractSnapshot.v1 + ItemAffixPoolAndRangeSchemaSnapshot.v1 + ItemSystemSnapshot.v1 + REQUIRED_FUTURE_RUNTIME_TRIGGER_FACT",
                    "Affixes.affixId=affix_first_trigger_bonus; Affixes.rawUnits; AffixDefinition.valueUnitKey; placement binding; placement.isLit; REQUIRED_FUTURE_RUNTIME_TRIGGER_FACT(on_first_trigger,first_trigger_in_battle)",
                    "basisPoint",
                    "Complete valid projection/schema and explicit matching lit placement; count a source only when a future stable runtime contract proves its on_first_trigger + first_trigger_in_battle condition. Do not require main-Build membership and do not count mere possession.",
                    "Saturating sum once per qualified itemInstanceId; deterministic itemInstanceId ordering; duplicate source identity is invalid/Unknown.",
                    "Identity: contributionBP = affix.rawUnits.",
                    "Any negative source is Unknown; each source and aggregate clamp to BuildCapabilityScale 0..10000.",
                    "Incomplete/invalid input, absent trigger proof, unit mismatch, missing binding, or identity mismatch is Unknown.",
                    "After rule approval and a complete runtime fact only, a complete eligible set with zero condition-satisfied sources is Known Zero; current absence remains Unknown.",
                    "YES_REQUIRED_FUTURE_STABLE_RUNTIME_TRIGGER_FACT",
                    3, "dev_encounter_3_10_guard_wall;dev_encounter_4_10_furnace_core;dev_encounter_4_10_thunder_fire_cross",
                    5, 24, 8, 8, 0,
                    "Direct BP identity and the existing C01 saturating-sum precedent avoid inventing a numeric conversion.",
                    "Conditional trigger cannot be treated as static; candidate payload is DEV_ONLY/BALANCE_CANDIDATE/NOT_BATTLE_CONNECTED; cross-item sum semantics are not approved.",
                    "NEEDS_ITEM_GUARD_FACT",
                    CandidateSourcePath + ";" + CandidateAssetPath + ";" + ProjectionContractPath + ";" + AffixSchemaPath + ";" + ItemSnapshotPath + ";" + CapabilityPrimitivesPath + ";" + C01AdapterPath,
                    "affix_first_trigger_bonus; ItemInstanceProjectionAffixSnapshot.rawUnits; ItemAffixDefinitionSnapshot.valueUnitKey; ItemSystemPlacementSnapshot.isLit; BuildCapabilityScale.MaximumValueBasisPoints; DIRECT_BASIS_POINT_SATURATING_SUM_0_10000")
            };
        }

        private static List<MatrixRow> ReadMatrix(string root)
        {
            List<string[]> rows = ReadCsv(Absolute(root, MatrixPath));
            if (rows.Count == 0)
            {
                throw new InvalidOperationException("C02 matrix is empty.");
            }

            Dictionary<string, int> header = Header(rows[0]);
            return rows.Skip(1).Where(value => value.Length > 0).Select(value => new MatrixRow(
                Field(value, header, "scenarioId"),
                Field(value, header, "encounterId"),
                Field(value, header, "evaluationStatus"),
                Field(value, header, "unknownCapabilities"))).ToList();
        }

        private static List<ImpactRow> BuildImpacts(
            IReadOnlyList<DecisionRow> decisions,
            IReadOnlyList<CandidateRow> candidates,
            IReadOnlyList<MatrixRow> matrix)
        {
            List<ImpactRow> output = new List<ImpactRow>();
            foreach (DecisionRow decision in decisions)
            {
                CandidateRow candidate = candidates.SingleOrDefault(value =>
                    string.Equals(value.CapabilityKey, decision.CapabilityKey, StringComparison.Ordinal));
                foreach (IGrouping<string, MatrixRow> scenario in matrix.GroupBy(value => value.ScenarioId, StringComparer.Ordinal))
                {
                    int encounterIndex = 0;
                    foreach (MatrixRow row in scenario)
                    {
                        bool candidateExists = candidate != null;
                        List<string> unknown = SplitKeys(row.UnknownCapabilities);
                        bool referenced = unknown.Contains(decision.CapabilityKey, StringComparer.Ordinal);
                        List<string> remaining = candidateExists
                            ? unknown.Where(value => !string.Equals(value, decision.CapabilityKey, StringComparison.Ordinal)).ToList()
                            : unknown;
                        int requirementReduction = candidateExists && referenced ? 1 : 0;
                        int ruleReduction = candidateExists && encounterIndex == 0 ? 1 : 0;
                        int c01Reduction = candidateExists && encounterIndex == 0 ? 1 : 0;
                        int blockedRowReduction = candidateExists && referenced && remaining.Count == 0 ? 1 : 0;
                        output.Add(new ImpactRow(
                            decision.DecisionId,
                            decision.CapabilityKey,
                            candidateExists ? candidate.CandidateId : "NONE_NO_UNIT_SAFE_RULE",
                            candidateExists,
                            row.ScenarioId,
                            row.EncounterId,
                            referenced,
                            row.EvaluationStatus,
                            requirementReduction,
                            ruleReduction,
                            c01Reduction,
                            requirementReduction + ruleReduction + c01Reduction,
                            blockedRowReduction,
                            string.Join(";", remaining),
                            candidateExists
                                ? "HYPOTHETICAL_APPROVAL_PLUS_FUTURE_MAPPING_ONLY"
                                : "NO_CANDIDATE_NO_REDUCTION"));
                        encounterIndex++;
                    }
                }
            }
            return output;
        }

        private static void RunDecisionChecks(
            ICollection<Check> checks,
            IReadOnlyList<DecisionRow> decisions,
            IReadOnlyList<CandidateRow> candidates)
        {
            string[] expectedKeys =
            {
                "capability.energy_stability",
                "capability.control_power",
                "capability.cleanse_power",
                "capability.placement_shape",
                "capability.burst_window",
                "capability.clear_power"
            };
            Add(checks, "decision.count", "6", decisions.Count.ToString(CultureInfo.InvariantCulture), decisions.Count == 6);
            Add(checks, "decision.keys", string.Join(";", expectedKeys),
                string.Join(";", decisions.Select(value => value.CapabilityKey)),
                expectedKeys.SequenceEqual(decisions.Select(value => value.CapabilityKey)));
            Add(checks, "decision.safe-defaults", "6 KEEP_UNKNOWN",
                decisions.Count(value => value.SafeDefault == "KEEP_UNKNOWN").ToString(CultureInfo.InvariantCulture),
                decisions.All(value => value.SafeDefault == "KEEP_UNKNOWN"));
            Add(checks, "candidate.count.total", "1", candidates.Count.ToString(CultureInfo.InvariantCulture), candidates.Count == 1);
            Add(checks, "candidate.max-two-per-capability", "true",
                candidates.GroupBy(value => value.CapabilityKey).All(value => value.Count() <= 2).ToString(),
                candidates.GroupBy(value => value.CapabilityKey).All(value => value.Count() <= 2));
            Add(checks, "candidate.unit-safe.count", "1",
                candidates.Count(value => value.SourceUnit == "basisPoint").ToString(CultureInfo.InvariantCulture),
                candidates.Count(value => value.SourceUnit == "basisPoint") == 1);
            Add(checks, "capability.no-unit-safe.count", "5",
                decisions.Count(value => value.Disposition.StartsWith("NO_UNIT_SAFE_RULE", StringComparison.Ordinal))
                    .ToString(CultureInfo.InvariantCulture),
                decisions.Count(value => value.Disposition.StartsWith("NO_UNIT_SAFE_RULE", StringComparison.Ordinal)) == 5);
            Add(checks, "energy.no-candidate", "0",
                candidates.Count(value => value.CapabilityKey == "capability.energy_stability").ToString(CultureInfo.InvariantCulture),
                candidates.All(value => value.CapabilityKey != "capability.energy_stability"));
            Add(checks, "energy.disposition", "NO_UNIT_SAFE_RULE / KEEP_UNKNOWN",
                decisions.Single(value => value.CapabilityKey == "capability.energy_stability").Disposition,
                decisions.Single(value => value.CapabilityKey == "capability.energy_stability").Disposition
                    == "NO_UNIT_SAFE_RULE / KEEP_UNKNOWN");

            CandidateRow burst = candidates.Single();
            Add(checks, "burst.runtime-gated", "first_trigger_in_battle",
                burst.EligibilityRule, burst.EligibilityRule.Contains("first_trigger_in_battle"));
            Add(checks, "burst.not-static-possession", "do not count mere possession",
                burst.EligibilityRule, burst.EligibilityRule.Contains("do not count mere possession"));
            Add(checks, "burst.identity-conversion", "Identity BP", burst.ConversionRule,
                burst.ConversionRule.StartsWith("Identity", StringComparison.Ordinal));
            Add(checks, "burst.keep-unknown-disposition", "NEEDS_ITEM_GUARD_FACT / KEEP_UNKNOWN",
                decisions.Single(value => value.CapabilityKey == "capability.burst_window").Disposition,
                decisions.Single(value => value.CapabilityKey == "capability.burst_window").Disposition
                    == "NEEDS_ITEM_GUARD_FACT / KEEP_UNKNOWN");
            Add(checks, "candidate.required-fields", "all non-empty", "all non-empty",
                candidates.All(value => value.RequiredFieldsPresent));
        }

        private static void RunC02Checks(
            ICollection<Check> checks,
            string root,
            IReadOnlyList<DecisionRow> decisions,
            IReadOnlyList<CandidateRow> candidates,
            IReadOnlyList<MatrixRow> matrix,
            IReadOnlyList<ImpactRow> impacts)
        {
            Add(checks, "c02.matrix.rows", "32", matrix.Count.ToString(CultureInfo.InvariantCulture), matrix.Count == 32);
            Add(checks, "c02.matrix.scenarios", "8",
                matrix.Select(value => value.ScenarioId).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                matrix.Select(value => value.ScenarioId).Distinct(StringComparer.Ordinal).Count() == 8);
            Add(checks, "c02.matrix.encounters", "4",
                matrix.Select(value => value.EncounterId).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                matrix.Select(value => value.EncounterId).Distinct(StringComparer.Ordinal).Count() == 4);
            Add(checks, "impact.rows", "192", impacts.Count.ToString(CultureInfo.InvariantCulture), impacts.Count == 192);
            Add(checks, "impact.no-readiness-output", "baseline status only", "baseline status only",
                impacts.All(value => value.BaselineEvaluationStatus == "BLOCKED_BY_UNKNOWN"));

            Dictionary<string, string[]> coverage = ReadCsvByFirstColumn(Absolute(root, CoveragePath));
            List<string[]> blockerRows = ReadCsv(Absolute(root, BlockersPath));
            Dictionary<string, int> blockerHeader = Header(blockerRows[0]);
            IEnumerable<string[]> blockerData = blockerRows.Skip(1);
            foreach (DecisionRow decision in decisions)
            {
                string[] coverageRow = coverage[decision.CapabilityKey];
                int actualRefs = ParseInt(coverageRow[3]);
                int actualBlocked = ParseInt(coverageRow[5]);
                Add(checks, "coverage.refs." + decision.CapabilityKey,
                    decision.E10RequirementReferences.ToString(CultureInfo.InvariantCulture), actualRefs.ToString(CultureInfo.InvariantCulture),
                    actualRefs == decision.E10RequirementReferences);
                Add(checks, "coverage.blocked." + decision.CapabilityKey,
                    decision.C02RequirementUnknownBlockers.ToString(CultureInfo.InvariantCulture), actualBlocked.ToString(CultureInfo.InvariantCulture),
                    actualBlocked == decision.C02RequirementUnknownBlockers);

                string capability = decision.CapabilityKey;
                int requirement = blockerData.Count(value =>
                    Field(value, blockerHeader, "capabilityKey") == capability
                    && Field(value, blockerHeader, "sourceCode") == "E08_CAPABILITY_VALUE_UNKNOWN");
                int c01 = blockerData.Count(value =>
                    Field(value, blockerHeader, "capabilityKey") == capability
                    && Field(value, blockerHeader, "sourceCode") == "C01_UNKNOWN_NOT_MAPPED");
                int rule = blockerData.Count(value =>
                    Field(value, blockerHeader, "capabilityKey") == capability
                    && Field(value, blockerHeader, "sourceCode") == "CAPABILITY_RULE_NOT_CONFIRMED");
                int encounters = blockerData.Where(value =>
                        Field(value, blockerHeader, "capabilityKey") == capability
                        && Field(value, blockerHeader, "sourceCode") == "E08_CAPABILITY_VALUE_UNKNOWN")
                    .Select(value => Field(value, blockerHeader, "encounterId"))
                    .Distinct(StringComparer.Ordinal).Count();
                string actualEncounterIds = string.Join(";", blockerData.Where(value =>
                        Field(value, blockerHeader, "capabilityKey") == capability
                        && Field(value, blockerHeader, "sourceCode") == "E08_CAPABILITY_VALUE_UNKNOWN")
                    .Select(value => Field(value, blockerHeader, "encounterId"))
                    .Distinct(StringComparer.Ordinal));
                Add(checks, "blockers.requirement." + capability,
                    decision.C02RequirementUnknownBlockers.ToString(CultureInfo.InvariantCulture), requirement.ToString(CultureInfo.InvariantCulture),
                    requirement == decision.C02RequirementUnknownBlockers);
                Add(checks, "blockers.c01." + capability, "8", c01.ToString(CultureInfo.InvariantCulture), c01 == 8);
                Add(checks, "blockers.rule." + capability, "8", rule.ToString(CultureInfo.InvariantCulture), rule == 8);
                Add(checks, "blockers.encounters." + capability,
                    decision.AffectedEncounterCount.ToString(CultureInfo.InvariantCulture), encounters.ToString(CultureInfo.InvariantCulture),
                    encounters == decision.AffectedEncounterCount);
                Add(checks, "blockers.encounter-ids." + capability,
                    decision.AffectedEncounterIds, actualEncounterIds,
                    SplitKeys(decision.AffectedEncounterIds).OrderBy(value => value, StringComparer.Ordinal)
                        .SequenceEqual(SplitKeys(actualEncounterIds).OrderBy(value => value, StringComparer.Ordinal)));
            }

            CandidateRow candidate = candidates.Single();
            List<ImpactRow> candidateImpacts = impacts.Where(value => value.CandidateExists).ToList();
            Add(checks, "candidate.impact.requirement", candidate.RequirementBlockerReduction.ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.RequirementBlockerReduction).ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.RequirementBlockerReduction) == candidate.RequirementBlockerReduction);
            Add(checks, "candidate.impact.rule", candidate.RuleBlockerReduction.ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.RuleBlockerReduction).ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.RuleBlockerReduction) == candidate.RuleBlockerReduction);
            Add(checks, "candidate.impact.c01", candidate.C01BlockerReduction.ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.C01BlockerReduction).ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.C01BlockerReduction) == candidate.C01BlockerReduction);
            Add(checks, "candidate.impact.total", candidate.TotalBlockerReduction.ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.TotalBlockerReduction).ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.TotalBlockerReduction) == candidate.TotalBlockerReduction);
            Add(checks, "candidate.impact.blocked-rows", "0",
                candidateImpacts.Sum(value => value.BlockedRowReduction).ToString(CultureInfo.InvariantCulture),
                candidateImpacts.Sum(value => value.BlockedRowReduction) == 0);
            Add(checks, "no-candidate.impact.zero", "true",
                impacts.Where(value => !value.CandidateExists).All(value => value.TotalBlockerReduction == 0).ToString(),
                impacts.Where(value => !value.CandidateExists).All(value => value.TotalBlockerReduction == 0));
        }

        private static void RunSourceChecks(ICollection<Check> checks, string root)
        {
            string candidateSource = File.ReadAllText(Absolute(root, CandidateSourcePath));
            string candidateAsset = File.ReadAllText(Absolute(root, CandidateAssetPath));
            string projection = File.ReadAllText(Absolute(root, ProjectionContractPath));
            string affixSchema = File.ReadAllText(Absolute(root, AffixSchemaPath));
            string itemSnapshot = File.ReadAllText(Absolute(root, ItemSnapshotPath));
            string capability = File.ReadAllText(Absolute(root, CapabilityPrimitivesPath));
            string c01 = File.ReadAllText(Absolute(root, C01AdapterPath));

            Add(checks, "source.energy.schema-bp", "basisPoint near affix_nian_efficiency",
                Near(candidateAsset, "  - affixId: affix_nian_efficiency", "unitKey: basisPoint", 240).ToString(),
                Near(candidateAsset, "  - affixId: affix_nian_efficiency", "unitKey: basisPoint", 240));
            Add(checks, "source.energy.payload-point", "point near affix_nian_efficiency",
                Near(candidateSource, "\"affix_nian_efficiency\"", "\"point\"", 420).ToString(),
                Near(candidateSource, "\"affix_nian_efficiency\"", "\"point\"", 420));
            Add(checks, "source.energy.refund-point", "point near affix_trigger_refund",
                Near(candidateSource, "\"affix_trigger_refund\"", "\"point\"", 420).ToString(),
                Near(candidateSource, "\"affix_trigger_refund\"", "\"point\"", 420));
            Add(checks, "source.control.point", "point near affix_control_up",
                Near(candidateSource, "\"affix_control_up\"", "\"point\"", 420).ToString(),
                Near(candidateSource, "\"affix_control_up\"", "\"point\"", 420));
            Add(checks, "source.cleanse.stack", "stack near affix_cleanse_up",
                Near(candidateSource, "\"affix_cleanse_up\"", "\"stack\"", 420).ToString(),
                Near(candidateSource, "\"affix_cleanse_up\"", "\"stack\"", 420));
            Add(checks, "source.burst.bp", "basisPoint near affix_first_trigger_bonus",
                Near(candidateSource, "\"affix_first_trigger_bonus\"", "\"basisPoint\"", 520).ToString(),
                Near(candidateSource, "\"affix_first_trigger_bonus\"", "\"basisPoint\"", 520));
            Add(checks, "source.burst.condition", "first_trigger_in_battle near affix_first_trigger_bonus",
                Near(candidateSource, "\"affix_first_trigger_bonus\"", "\"first_trigger_in_battle\"", 520).ToString(),
                Near(candidateSource, "\"affix_first_trigger_bonus\"", "\"first_trigger_in_battle\"", 520));
            Add(checks, "source.clear.count", "count near affix_chain_target",
                Near(candidateSource, "\"affix_chain_target\"", "\"count\"", 520).ToString(),
                Near(candidateSource, "\"affix_chain_target\"", "\"count\"", 520));
            Add(checks, "source.clear.condition", "chain_count_3 near affix_chain_target",
                Near(candidateSource, "\"affix_chain_target\"", "\"chain_count_3\"", 520).ToString(),
                Near(candidateSource, "\"affix_chain_target\"", "\"chain_count_3\"", 520));
            Add(checks, "source.payload.maturity", "BALANCE_CANDIDATE", "BALANCE_CANDIDATE",
                candidateAsset.Contains("dataMaturity: BALANCE_CANDIDATE"));
            Add(checks, "source.payload.not-battle-connected", "NOT_BATTLE_CONNECTED", "NOT_BATTLE_CONNECTED",
                candidateAsset.Contains("battleState: NOT_BATTLE_CONNECTED"));
            Add(checks, "contract.projection.affix", "affixId/rawUnits", "affixId/rawUnits",
                projection.Contains("public string affixId { get; }") && projection.Contains("public long rawUnits { get; }"));
            Add(checks, "contract.affix.unit", "valueUnitKey", "valueUnitKey",
                affixSchema.Contains("x.valueUnitKey"));
            Add(checks, "contract.item.placement", "isLit/isCountedInBuild/OccupiedCells", "isLit/isCountedInBuild/OccupiedCells",
                itemSnapshot.Contains("public bool isLit { get; }")
                && itemSnapshot.Contains("public bool isCountedInBuild { get; }")
                && itemSnapshot.Contains("OccupiedCells"));
            Add(checks, "contract.capability.scale", "MaximumValueBasisPoints = 10000", "MaximumValueBasisPoints = 10000",
                capability.Contains("MaximumValueBasisPoints = 10000"));
            Add(checks, "c01.bp-precedent", "DIRECT_BASIS_POINT_SATURATING_SUM_0_10000",
                "DIRECT_BASIS_POINT_SATURATING_SUM_0_10000",
                c01.Contains("DIRECT_BASIS_POINT_SATURATING_SUM_0_10000"));
            Add(checks, "c01.reject-unit-guess", "no conversion is guessed", "no conversion is guessed",
                c01.Contains("no conversion is guessed"));
        }

        private static void RunLeakChecks(
            ICollection<Check> checks,
            ICollection<LeakRow> leaks,
            string root)
        {
            string source = StripCommentsAndStringLiterals(File.ReadAllText(Absolute(root, VerifierPath)));
            string[] forbiddenTokens =
            {
                "EnemyOfflineReadiness" + "Evaluator",
                "TalismanBag.Battle",
                "TalismanBag.Board",
                "SceneManager",
                "PrefabUtility",
                "AssetDatabase.CreateAsset",
                "PlayerPrefs",
                "SaveData",
                "RewardConfig"
            };
            foreach (string token in forbiddenTokens)
            {
                int count = CountOrdinal(source, token);
                if (count > 0)
                {
                    leaks.Add(new LeakRow(VerifierPath, token, count));
                }
            }
            Add(checks, "leak.count", "0",
                leaks.Sum(value => value.Count).ToString(CultureInfo.InvariantCulture), leaks.Count == 0);
        }

        private static void RunProtectionChecks(
            ICollection<Check> checks,
            ProtectionSnapshot before,
            ProtectionSnapshot after)
        {
            AddProtection(checks, "item", before.Item, after.Item, ExpectedItemFileCount, ExpectedItemHash);
            AddProtection(checks, "enemy", before.Enemy, after.Enemy, ExpectedEnemyFileCount, ExpectedEnemyHash);
            AddProtection(checks, "c01", before.C01, after.C01, C01ProtectedFiles.Length, ExpectedC01Hash);
            AddProtection(checks, "c02", before.C02, after.C02, C02ProtectedFiles.Length, ExpectedC02Hash);
            AddProtection(checks, "c02a", before.C02A, after.C02A, C02AProtectedFiles.Length, ExpectedC02AHash);
        }

        private static void AddProtection(
            ICollection<Check> checks,
            string id,
            AggregateHash before,
            AggregateHash after,
            int expectedCount,
            string expectedHash)
        {
            Add(checks, "protected." + id + ".file-count",
                expectedCount.ToString(CultureInfo.InvariantCulture), before.FileCount.ToString(CultureInfo.InvariantCulture),
                before.FileCount == expectedCount);
            Add(checks, "protected." + id + ".accepted-hash", expectedHash, before.Hash,
                string.Equals(before.Hash, expectedHash, StringComparison.Ordinal));
            Add(checks, "protected." + id + ".before-after", before.Hash, after.Hash, before.Equals(after));
        }

        private static void RunOutputScopeChecks(ICollection<Check> checks, string root)
        {
            Add(checks, "output.allowed-count", "5", OutputPaths.Length.ToString(CultureInfo.InvariantCulture),
                OutputPaths.Length == 5);
            Add(checks, "output.verifier-exists", "true", File.Exists(Absolute(root, VerifierPath)).ToString(),
                File.Exists(Absolute(root, VerifierPath)));
            Add(checks, "output.directory-exists", "true", Directory.Exists(Absolute(root, ReportDirectory)).ToString(),
                Directory.Exists(Absolute(root, ReportDirectory)));
        }

        private static Dictionary<string, string> RenderReports(
            string mode,
            IReadOnlyList<DecisionRow> decisions,
            IReadOnlyList<CandidateRow> candidates,
            IReadOnlyList<ImpactRow> impacts,
            IReadOnlyList<Check> checks,
            IReadOnlyList<LeakRow> leaks,
            ProtectionSnapshot before,
            ProtectionSnapshot after)
        {
            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Dictionary<string, string> reports = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [CandidatesPath] = RenderCandidates(candidates),
                [ImpactPath] = RenderImpacts(impacts),
                [DecisionSheetPath] = RenderDecisionSheet(decisions, candidates),
                [LeakPath] = RenderLeak(pass, leaks),
                [MainReportPath] = RenderMain(mode, pass, decisions, candidates, checks, leaks, before, after)
            };
            return reports;
        }

        private static string RenderCandidates(IReadOnlyList<CandidateRow> candidates)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("decisionId,capabilityKey,candidateId,candidateSourceContract,candidateStableKeys,sourceUnit,eligibilityRule,aggregationRule,conversionRule,clampOrThresholdRule,missingSourceSemantics,knownZeroSemantics,runtimeDependency,affectedEncounterCount,affectedEncounterIds,e10RequirementReferenceCount,projectedRequirementBlockerReduction,projectedRuleConfirmationBlockerReduction,projectedC01UnknownBlockerReductionAfterFutureMapping,projectedTotalBlockerReferenceReductionAfterApprovalAndFutureMapping,projectedBlockedRowReduction,advantages,risks,recommendedDisposition,evidenceFile,evidenceLineOrSymbol");
            foreach (CandidateRow row in candidates)
            {
                builder.AppendLine(Csv(
                    row.DecisionId, row.CapabilityKey, row.CandidateId, row.CandidateSourceContract,
                    row.CandidateStableKeys, row.SourceUnit, row.EligibilityRule, row.AggregationRule,
                    row.ConversionRule, row.ClampOrThresholdRule, row.MissingSourceSemantics,
                    row.KnownZeroSemantics, row.RuntimeDependency,
                    row.AffectedEncounterCount.ToString(CultureInfo.InvariantCulture),
                    row.AffectedEncounterIds,
                    row.E10RequirementReferenceCount.ToString(CultureInfo.InvariantCulture),
                    row.RequirementBlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.RuleBlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.C01BlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.TotalBlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.BlockedRowReduction.ToString(CultureInfo.InvariantCulture),
                    row.Advantages, row.Risks, row.RecommendedDisposition, row.EvidenceFile, row.EvidenceLineOrSymbol));
            }
            return builder.ToString();
        }

        private static string RenderImpacts(IReadOnlyList<ImpactRow> impacts)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("decisionId,capabilityKey,candidateId,candidateExists,itemScenarioId,encounterId,capabilityRequirementReferenced,baselineEvaluationStatus,projectedRequirementBlockerReduction,projectedRuleConfirmationBlockerReduction,projectedC01UnknownBlockerReductionAfterFutureMapping,projectedTotalBlockerReferenceReduction,projectedBlockedMatrixRowReduction,remainingUnknownCapabilitiesAfterHypothesis,hypothesisBoundary");
            foreach (ImpactRow row in impacts)
            {
                builder.AppendLine(Csv(
                    row.DecisionId, row.CapabilityKey, row.CandidateId, Bool(row.CandidateExists),
                    row.ScenarioId, row.EncounterId, Bool(row.CapabilityRequirementReferenced),
                    row.BaselineEvaluationStatus,
                    row.RequirementBlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.RuleBlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.C01BlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.TotalBlockerReduction.ToString(CultureInfo.InvariantCulture),
                    row.BlockedRowReduction.ToString(CultureInfo.InvariantCulture),
                    row.RemainingUnknownCapabilities, row.HypothesisBoundary));
            }
            return builder.ToString();
        }

        private static string RenderDecisionSheet(
            IReadOnlyList<DecisionRow> decisions,
            IReadOnlyList<CandidateRow> candidates)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("decisionId,capabilityKey,candidateCount,unitSafeCandidateCount,candidateStableFacts,sourceUnits,eligibilityBoundary,aggregationBoundary,conversionBoundary,clampOrThresholdBoundary,missingSourceSemantics,knownZeroSemantics,runtimeDependency,affectedEncounterCount,affectedEncounterIds,e10RequirementReferenceCount,c02RequirementUnknownBlockerCount,c02RuleConfirmationBlockerCount,c02C01UnknownBlockerCount,coverageCeilingTotalBlockerReferences,candidateProjectedTotalBlockerReduction,candidateProjectedBlockedRowReduction,risks,recommendedDisposition,safeDefault,evidenceFile,evidenceLineOrSymbol");
            foreach (DecisionRow row in decisions)
            {
                List<CandidateRow> capabilityCandidates = candidates.Where(value =>
                    value.CapabilityKey == row.CapabilityKey).ToList();
                builder.AppendLine(Csv(
                    row.DecisionId, row.CapabilityKey,
                    capabilityCandidates.Count.ToString(CultureInfo.InvariantCulture),
                    capabilityCandidates.Count(value => value.SourceUnit == "basisPoint").ToString(CultureInfo.InvariantCulture),
                    row.StableFacts, row.SourceUnits, row.EligibilityBoundary, row.AggregationBoundary,
                    row.ConversionBoundary, row.ClampBoundary, row.MissingSourceSemantics,
                    row.KnownZeroSemantics, row.RuntimeDependency,
                    row.AffectedEncounterCount.ToString(CultureInfo.InvariantCulture),
                    row.AffectedEncounterIds,
                    row.E10RequirementReferences.ToString(CultureInfo.InvariantCulture),
                    row.C02RequirementUnknownBlockers.ToString(CultureInfo.InvariantCulture),
                    "8", "8", (row.C02RequirementUnknownBlockers + 16).ToString(CultureInfo.InvariantCulture),
                    capabilityCandidates.Sum(value => value.TotalBlockerReduction).ToString(CultureInfo.InvariantCulture),
                    capabilityCandidates.Sum(value => value.BlockedRowReduction).ToString(CultureInfo.InvariantCulture),
                    row.Risks, row.Disposition, row.SafeDefault, row.EvidenceFile, row.EvidenceLineOrSymbol));
            }
            return builder.ToString();
        }

        private static string RenderMain(
            string mode,
            bool pass,
            IReadOnlyList<DecisionRow> decisions,
            IReadOnlyList<CandidateRow> candidates,
            IReadOnlyList<Check> checks,
            IReadOnlyList<LeakRow> leaks,
            ProtectionSnapshot before,
            ProtectionSnapshot after)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Item Capability Rule Decision Report")
                .AppendLine()
                .AppendLine("- Package: `" + PackageName + "`")
                .AppendLine("- Guard receipt: `" + GuardReceipt + "`")
                .AppendLine("- Execution: `" + mode + "`")
                .AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine("- Scope: `read-only rule decision material; no mapping implementation`")
                .AppendLine("- Capability decisions: `6/6`")
                .AppendLine("- Rule candidates: `" + candidates.Count.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Unit-safe candidates: `1`")
                .AppendLine("- Capabilities with no unit-safe rule: `5`")
                .AppendLine("- Remaining Unknown: `" + string.Join(";", decisions.Select(value => value.CapabilityKey)) + "`")
                .AppendLine("- Existing files modified by this package: `0`")
                .AppendLine("- Mapping/status changes: `0`")
                .AppendLine("- Candidate approvals: `0`")
                .AppendLine("- Leak Count: `" + leaks.Sum(value => value.Count).ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Offline verifier: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine("- Unity batch verifier: `" +
                    (mode == "Unity batch compile/verifier" ? (pass ? "PASS" : "FAIL") : "PENDING_UNITY_BATCH") + "`")
                .AppendLine()
                .AppendLine("No candidate is approved by this report. All six values remain sparse omissions/Unknown until the user selects a rule and Guard authorizes a separate implementation package.")
                .AppendLine()
                .AppendLine("## Decision summary")
                .AppendLine()
                .AppendLine("| Capability | Candidates | Unit-safe | E10 refs | E08 blocker refs | Affected encounters | Candidate blocker reduction | Blocked-row reduction | Disposition |")
                .AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---|");
            foreach (DecisionRow row in decisions)
            {
                List<CandidateRow> rows = candidates.Where(value => value.CapabilityKey == row.CapabilityKey).ToList();
                builder.Append("| ").Append(row.CapabilityKey).Append(" | ")
                    .Append(rows.Count.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(rows.Count(value => value.SourceUnit == "basisPoint").ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(row.E10RequirementReferences.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(row.C02RequirementUnknownBlockers.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(row.AffectedEncounterCount.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(rows.Sum(value => value.TotalBlockerReduction).ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(rows.Sum(value => value.BlockedRowReduction).ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(Md(row.Disposition)).AppendLine(" |");
            }

            builder.AppendLine()
                .AppendLine("Impact counts are hypothetical only. For the sole candidate, `40 = 24 E08 requirement-Unknown references + 8 rule-confirmation references + 8 C01-Unknown references after a future C02B mapping`. Candidate approval alone can address only the 8 rule-confirmation references. The projected C02 blocked-row reduction is `0` because every affected row retains other decisive Unknown capabilities.")
                .AppendLine()
                .AppendLine("## Candidate")
                .AppendLine();
            CandidateRow candidate = candidates.Single();
            builder.AppendLine("### " + candidate.CandidateId + " — " + candidate.CapabilityKey)
                .AppendLine()
                .AppendLine("- Source contract: `" + candidate.CandidateSourceContract + "`")
                .AppendLine("- Stable/current members: `" + candidate.CandidateStableKeys + "`")
                .AppendLine("- Source unit: `" + candidate.SourceUnit + "`")
                .AppendLine("- Eligibility: " + candidate.EligibilityRule)
                .AppendLine("- Aggregation: " + candidate.AggregationRule)
                .AppendLine("- Conversion: " + candidate.ConversionRule)
                .AppendLine("- Clamp/threshold: " + candidate.ClampOrThresholdRule)
                .AppendLine("- Missing source: " + candidate.MissingSourceSemantics)
                .AppendLine("- Known Zero boundary: " + candidate.KnownZeroSemantics)
                .AppendLine("- Runtime dependency: `" + candidate.RuntimeDependency + "`")
                .AppendLine("- Affected encounters: `" + candidate.AffectedEncounterIds + "`")
                .AppendLine("- Advantages: " + candidate.Advantages)
                .AppendLine("- Risks: " + candidate.Risks)
                .AppendLine("- Recommendation: `" + candidate.RecommendedDisposition + "`; safe default remains `KEEP_UNKNOWN`.")
                .AppendLine("- Evidence: `" + candidate.EvidenceFile + "` / `" + candidate.EvidenceLineOrSymbol + "`")
                .AppendLine()
                .AppendLine("This candidate is unit-safe but not executable today: the BP identity is stable, while the trigger qualification fact is not. Static possession must not be interpreted as the conditional trigger having occurred.")
                .AppendLine()
                .AppendLine("## No-unit-safe decisions")
                .AppendLine();
            foreach (DecisionRow row in decisions.Where(value =>
                         value.Disposition.StartsWith("NO_UNIT_SAFE_RULE", StringComparison.Ordinal)))
            {
                builder.AppendLine("### " + row.CapabilityKey)
                    .AppendLine()
                    .AppendLine("- Stable facts: " + row.StableFacts)
                    .AppendLine("- Source units: `" + row.SourceUnits + "`")
                    .AppendLine("- Eligibility: " + row.EligibilityBoundary)
                    .AppendLine("- Aggregation: `" + row.AggregationBoundary + "`")
                    .AppendLine("- Conversion: `" + row.ConversionBoundary + "`")
                    .AppendLine("- Clamp/threshold: `" + row.ClampBoundary + "`")
                    .AppendLine("- Missing source: " + row.MissingSourceSemantics)
                    .AppendLine("- Known Zero boundary: " + row.KnownZeroSemantics)
                    .AppendLine("- Runtime dependency: " + row.RuntimeDependency)
                    .AppendLine("- Affected encounters: `" + row.AffectedEncounterIds + "`")
                    .AppendLine("- Impact ceiling if a later safe rule exists: `" + row.C02RequirementUnknownBlockers
                        .ToString(CultureInfo.InvariantCulture) + " E08 refs; "
                        + (row.C02RequirementUnknownBlockers + 16).ToString(CultureInfo.InvariantCulture)
                        + " total blocker refs; 0 currently projected blocked rows`.")
                    .AppendLine("- Risk: " + row.Risks)
                    .AppendLine("- Disposition: `" + row.Disposition + "`")
                    .AppendLine("- Evidence: `" + row.EvidenceFile + "` / `" + row.EvidenceLineOrSymbol + "`")
                    .AppendLine();
            }

            builder.AppendLine("## Protected hashes")
                .AppendLine()
                .AppendLine("| Scope | Files | Before | After | Same |")
                .AppendLine("|---|---:|---|---|---|");
            AppendProtection(builder, "Item", before.Item, after.Item);
            AppendProtection(builder, "Enemy", before.Enemy, after.Enemy);
            AppendProtection(builder, "C01", before.C01, after.C01);
            AppendProtection(builder, "C02", before.C02, after.C02);
            AppendProtection(builder, "C02A", before.C02A, after.C02A);

            builder.AppendLine()
                .AppendLine("## Verifier checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks)
            {
                builder.Append("| ").Append(Md(check.Id)).Append(" | ")
                    .Append(Md(check.Expected)).Append(" | ")
                    .Append(Md(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }

            builder.AppendLine()
                .AppendLine("## Forbidden scope")
                .AppendLine()
                .AppendLine("- Item-to-capability mapping implementation: `0`")
                .AppendLine("- Supported/Known Zero status changes: `0`")
                .AppendLine("- Item / Enemy / Runtime / Battle / Board changes: `0`")
                .AppendLine("- Scene / Prefab / Config changes: `0`")
                .AppendLine("- Formal readiness, difficulty, win/loss, or drop conclusions: `0`")
                .AppendLine("- C02B / C02R1 / C03 starts: `0`")
                .AppendLine("- Commit / tag / push: `0`");
            return builder.ToString();
        }

        private static string RenderLeak(bool pass, IReadOnlyList<LeakRow> leaks)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Item Capability Rule Decision Leak Check Report")
                .AppendLine()
                .AppendLine("- Result: `" + (pass && leaks.Count == 0 ? "PASS" : "FAIL") + "`")
                .AppendLine("- Leak Count: `" + leaks.Sum(value => value.Count).ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Scan scope: `Editor-only verifier executable source after comments and string literals are stripped`")
                .AppendLine("- Added files in package whitelist: `7`")
                .AppendLine("- Existing files modified by package: `0`")
                .AppendLine("- Item-to-capability mapping implementation: `0`")
                .AppendLine("- Unknown-to-Supported/Known-Zero changes: `0`")
                .AppendLine("- Item / Enemy / Runtime / Battle / Board changes: `0`")
                .AppendLine("- Scene / Prefab / Config changes: `0`")
                .AppendLine("- Formal readiness evaluator calls: `0`")
                .AppendLine("- Candidate approvals: `0`")
                .AppendLine("- C02B / C02R1 / C03 starts: `0`");
            if (leaks.Count > 0)
            {
                builder.AppendLine()
                    .AppendLine("| File | Token | Count |")
                    .AppendLine("|---|---|---:|");
                foreach (LeakRow leak in leaks)
                {
                    builder.Append("| ").Append(Md(leak.File)).Append(" | ")
                        .Append(Md(leak.Token)).Append(" | ")
                        .Append(leak.Count.ToString(CultureInfo.InvariantCulture)).AppendLine(" |");
                }
            }
            return builder.ToString();
        }

        private static ProtectionSnapshot CaptureProtection(string root)
        {
            return new ProtectionSnapshot(
                AggregateDirectory(root, "Assets/_Game/Scripts/TalismanBag/Items"),
                AggregateDirectory(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem"),
                AggregateFiles(root, C01ProtectedFiles),
                AggregateFiles(root, C02ProtectedFiles),
                AggregateFiles(root, C02AProtectedFiles));
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

        private static void AppendProtection(
            StringBuilder builder,
            string scope,
            AggregateHash before,
            AggregateHash after)
        {
            builder.Append("| ").Append(scope).Append(" | ")
                .Append(before.FileCount.ToString(CultureInfo.InvariantCulture)).Append(" | `")
                .Append(before.Hash).Append("` | `")
                .Append(after.Hash).Append("` | ")
                .Append(before.Equals(after) ? "YES" : "NO").AppendLine(" |");
        }

        private static List<string[]> ReadCsv(string path)
        {
            return File.ReadAllLines(path)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => ParseCsvLine(value).ToArray()).ToList();
        }

        private static Dictionary<string, string[]> ReadCsvByFirstColumn(string path)
        {
            List<string[]> rows = ReadCsv(path);
            return rows.Skip(1).ToDictionary(value => value[0], value => value, StringComparer.Ordinal);
        }

        private static Dictionary<string, int> Header(string[] row)
        {
            return row.Select((value, index) => new { value, index })
                .ToDictionary(value => value.value, value => value.index, StringComparer.Ordinal);
        }

        private static string Field(string[] row, IReadOnlyDictionary<string, int> header, string key)
        {
            return header.TryGetValue(key, out int index) && index >= 0 && index < row.Length
                ? row[index]
                : string.Empty;
        }

        private static IEnumerable<string> ParseCsvLine(string line)
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
                    current.Clear();
                }
                else
                {
                    current.Append(value);
                }
            }
            values.Add(current.ToString());
            return values;
        }

        private static List<string> SplitKeys(string value)
        {
            return (value ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim()).Where(item => item.Length > 0).ToList();
        }

        private static bool Near(string text, string anchor, string expected, int span)
        {
            int start = (text ?? string.Empty).IndexOf(anchor, StringComparison.Ordinal);
            if (start < 0)
            {
                return false;
            }
            int found = text.IndexOf(expected, start, StringComparison.Ordinal);
            return found >= start && found - start <= span;
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

        private static string StripCommentsAndStringLiterals(string source)
        {
            StringBuilder output = new StringBuilder(source?.Length ?? 0);
            bool lineComment = false;
            bool blockComment = false;
            bool literal = false;
            bool verbatim = false;
            char literalDelimiter = '\0';
            for (int index = 0; index < (source ?? string.Empty).Length; index++)
            {
                char current = source[index];
                char next = index + 1 < source.Length ? source[index + 1] : '\0';
                if (lineComment)
                {
                    if (current == '\n')
                    {
                        lineComment = false;
                        output.Append('\n');
                    }
                    else
                    {
                        output.Append(' ');
                    }
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
                    else
                    {
                        output.Append(current == '\n' ? '\n' : ' ');
                    }
                    continue;
                }
                if (literal)
                {
                    output.Append(current == '\n' ? '\n' : ' ');
                    if (verbatim && current == '"' && next == '"')
                    {
                        output.Append(' ');
                        index++;
                        continue;
                    }
                    if (!verbatim && current == '\\' && next != '\0')
                    {
                        output.Append(next == '\n' ? '\n' : ' ');
                        index++;
                        continue;
                    }
                    if (current == literalDelimiter)
                    {
                        literal = false;
                        verbatim = false;
                    }
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
                    literal = true;
                    verbatim = true;
                    literalDelimiter = '"';
                }
                else if (current == '"' || current == '\'')
                {
                    output.Append(' ');
                    literal = true;
                    literalDelimiter = current;
                }
                else
                {
                    output.Append(current);
                }
            }
            return output.ToString();
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

        private static void WriteUtf8(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", (values ?? Array.Empty<string>()).Select(value =>
                "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\""));
        }

        private static string Md(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string Bool(bool value)
        {
            return value ? "true" : "false";
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
                ? result
                : -1;
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string format = uppercase ? "X2" : "x2";
                return string.Concat(sha256.ComputeHash(bytes ?? Array.Empty<byte>())
                    .Select(value => value.ToString(format, CultureInfo.InvariantCulture)));
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

        private sealed class DecisionRow
        {
            public DecisionRow(
                string decisionId, string capabilityKey, int e10RequirementReferences,
                int c02RequirementUnknownBlockers, int affectedEncounterCount, string affectedEncounterIds,
                string stableFacts, string sourceUnits, string eligibilityBoundary,
                string aggregationBoundary, string conversionBoundary, string clampBoundary,
                string missingSourceSemantics, string knownZeroSemantics, string runtimeDependency,
                string risks, string disposition, string safeDefault, string evidenceFile, string evidenceLineOrSymbol)
            {
                DecisionId = decisionId;
                CapabilityKey = capabilityKey;
                E10RequirementReferences = e10RequirementReferences;
                C02RequirementUnknownBlockers = c02RequirementUnknownBlockers;
                AffectedEncounterCount = affectedEncounterCount;
                AffectedEncounterIds = affectedEncounterIds;
                StableFacts = stableFacts;
                SourceUnits = sourceUnits;
                EligibilityBoundary = eligibilityBoundary;
                AggregationBoundary = aggregationBoundary;
                ConversionBoundary = conversionBoundary;
                ClampBoundary = clampBoundary;
                MissingSourceSemantics = missingSourceSemantics;
                KnownZeroSemantics = knownZeroSemantics;
                RuntimeDependency = runtimeDependency;
                Risks = risks;
                Disposition = disposition;
                SafeDefault = safeDefault;
                EvidenceFile = evidenceFile;
                EvidenceLineOrSymbol = evidenceLineOrSymbol;
            }

            public string DecisionId { get; }
            public string CapabilityKey { get; }
            public int E10RequirementReferences { get; }
            public int C02RequirementUnknownBlockers { get; }
            public int AffectedEncounterCount { get; }
            public string AffectedEncounterIds { get; }
            public string StableFacts { get; }
            public string SourceUnits { get; }
            public string EligibilityBoundary { get; }
            public string AggregationBoundary { get; }
            public string ConversionBoundary { get; }
            public string ClampBoundary { get; }
            public string MissingSourceSemantics { get; }
            public string KnownZeroSemantics { get; }
            public string RuntimeDependency { get; }
            public string Risks { get; }
            public string Disposition { get; }
            public string SafeDefault { get; }
            public string EvidenceFile { get; }
            public string EvidenceLineOrSymbol { get; }
        }

        private sealed class CandidateRow
        {
            public CandidateRow(
                string decisionId, string capabilityKey, string candidateId,
                string candidateSourceContract, string candidateStableKeys, string sourceUnit,
                string eligibilityRule, string aggregationRule, string conversionRule,
                string clampOrThresholdRule, string missingSourceSemantics, string knownZeroSemantics,
                string runtimeDependency, int affectedEncounterCount, string affectedEncounterIds,
                int e10RequirementReferenceCount,
                int requirementBlockerReduction, int ruleBlockerReduction, int c01BlockerReduction,
                int blockedRowReduction, string advantages, string risks, string recommendedDisposition,
                string evidenceFile, string evidenceLineOrSymbol)
            {
                DecisionId = decisionId;
                CapabilityKey = capabilityKey;
                CandidateId = candidateId;
                CandidateSourceContract = candidateSourceContract;
                CandidateStableKeys = candidateStableKeys;
                SourceUnit = sourceUnit;
                EligibilityRule = eligibilityRule;
                AggregationRule = aggregationRule;
                ConversionRule = conversionRule;
                ClampOrThresholdRule = clampOrThresholdRule;
                MissingSourceSemantics = missingSourceSemantics;
                KnownZeroSemantics = knownZeroSemantics;
                RuntimeDependency = runtimeDependency;
                AffectedEncounterCount = affectedEncounterCount;
                AffectedEncounterIds = affectedEncounterIds;
                E10RequirementReferenceCount = e10RequirementReferenceCount;
                RequirementBlockerReduction = requirementBlockerReduction;
                RuleBlockerReduction = ruleBlockerReduction;
                C01BlockerReduction = c01BlockerReduction;
                BlockedRowReduction = blockedRowReduction;
                Advantages = advantages;
                Risks = risks;
                RecommendedDisposition = recommendedDisposition;
                EvidenceFile = evidenceFile;
                EvidenceLineOrSymbol = evidenceLineOrSymbol;
            }

            public string DecisionId { get; }
            public string CapabilityKey { get; }
            public string CandidateId { get; }
            public string CandidateSourceContract { get; }
            public string CandidateStableKeys { get; }
            public string SourceUnit { get; }
            public string EligibilityRule { get; }
            public string AggregationRule { get; }
            public string ConversionRule { get; }
            public string ClampOrThresholdRule { get; }
            public string MissingSourceSemantics { get; }
            public string KnownZeroSemantics { get; }
            public string RuntimeDependency { get; }
            public int AffectedEncounterCount { get; }
            public string AffectedEncounterIds { get; }
            public int E10RequirementReferenceCount { get; }
            public int RequirementBlockerReduction { get; }
            public int RuleBlockerReduction { get; }
            public int C01BlockerReduction { get; }
            public int TotalBlockerReduction => RequirementBlockerReduction + RuleBlockerReduction + C01BlockerReduction;
            public int BlockedRowReduction { get; }
            public string Advantages { get; }
            public string Risks { get; }
            public string RecommendedDisposition { get; }
            public string EvidenceFile { get; }
            public string EvidenceLineOrSymbol { get; }
            public bool RequiredFieldsPresent => new[]
            {
                DecisionId, CapabilityKey, CandidateId, CandidateSourceContract, CandidateStableKeys,
                SourceUnit, EligibilityRule, AggregationRule, ConversionRule, ClampOrThresholdRule,
                MissingSourceSemantics, KnownZeroSemantics, RuntimeDependency, AffectedEncounterIds, Advantages, Risks,
                RecommendedDisposition, EvidenceFile, EvidenceLineOrSymbol
            }.All(value => !string.IsNullOrWhiteSpace(value));
        }

        private sealed class MatrixRow
        {
            public MatrixRow(string scenarioId, string encounterId, string evaluationStatus, string unknownCapabilities)
            {
                ScenarioId = scenarioId;
                EncounterId = encounterId;
                EvaluationStatus = evaluationStatus;
                UnknownCapabilities = unknownCapabilities;
            }

            public string ScenarioId { get; }
            public string EncounterId { get; }
            public string EvaluationStatus { get; }
            public string UnknownCapabilities { get; }
        }

        private sealed class ImpactRow
        {
            public ImpactRow(
                string decisionId, string capabilityKey, string candidateId, bool candidateExists,
                string scenarioId, string encounterId, bool capabilityRequirementReferenced,
                string baselineEvaluationStatus, int requirementBlockerReduction,
                int ruleBlockerReduction, int c01BlockerReduction, int totalBlockerReduction,
                int blockedRowReduction, string remainingUnknownCapabilities, string hypothesisBoundary)
            {
                DecisionId = decisionId;
                CapabilityKey = capabilityKey;
                CandidateId = candidateId;
                CandidateExists = candidateExists;
                ScenarioId = scenarioId;
                EncounterId = encounterId;
                CapabilityRequirementReferenced = capabilityRequirementReferenced;
                BaselineEvaluationStatus = baselineEvaluationStatus;
                RequirementBlockerReduction = requirementBlockerReduction;
                RuleBlockerReduction = ruleBlockerReduction;
                C01BlockerReduction = c01BlockerReduction;
                TotalBlockerReduction = totalBlockerReduction;
                BlockedRowReduction = blockedRowReduction;
                RemainingUnknownCapabilities = remainingUnknownCapabilities;
                HypothesisBoundary = hypothesisBoundary;
            }

            public string DecisionId { get; }
            public string CapabilityKey { get; }
            public string CandidateId { get; }
            public bool CandidateExists { get; }
            public string ScenarioId { get; }
            public string EncounterId { get; }
            public bool CapabilityRequirementReferenced { get; }
            public string BaselineEvaluationStatus { get; }
            public int RequirementBlockerReduction { get; }
            public int RuleBlockerReduction { get; }
            public int C01BlockerReduction { get; }
            public int TotalBlockerReduction { get; }
            public int BlockedRowReduction { get; }
            public string RemainingUnknownCapabilities { get; }
            public string HypothesisBoundary { get; }
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

        private sealed class AggregateHash : IEquatable<AggregateHash>
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash ?? string.Empty;
            }

            public int FileCount { get; }
            public string Hash { get; }

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
            public ProtectionSnapshot(
                AggregateHash item, AggregateHash enemy, AggregateHash c01,
                AggregateHash c02, AggregateHash c02A)
            {
                Item = item;
                Enemy = enemy;
                C01 = c01;
                C02 = c02;
                C02A = c02A;
            }

            public AggregateHash Item { get; }
            public AggregateHash Enemy { get; }
            public AggregateHash C01 { get; }
            public AggregateHash C02 { get; }
            public AggregateHash C02A { get; }
        }
    }
}
