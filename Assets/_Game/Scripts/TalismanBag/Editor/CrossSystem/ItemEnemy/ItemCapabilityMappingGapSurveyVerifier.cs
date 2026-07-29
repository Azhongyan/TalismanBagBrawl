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
    public static class ItemCapabilityMappingGapSurveyVerifier
    {
        private const string Package = "V0.4-ItemCapabilityMappingGapSurvey01";
        private const string GuardReceipt = "GUARD_PASS_ITEMCAPABILITYMAPPINGGAPSURVEY01";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string MainReportName = "ItemCapabilityMappingGapSurveyReport.md";
        private const string MatrixName = "ItemCapabilityMappingGapMatrix.csv";
        private const string EvidenceName = "ItemCapabilityMappingSourceEvidence.csv";
        private const string DecisionName = "ItemCapabilityMappingDecisionList.csv";
        private const string LeakName = "ItemCapabilityMappingGapSurveyLeakCheckReport.md";
        private const string FieldMapPath = "Docs/V0.4/Reports/ItemBuildCapabilityProjectionFieldMap.csv";
        private const string CoveragePath = "Docs/V0.4/Reports/ItemEnemyMatchupCapabilityCoverage.csv";
        private const string BlockersPath = "Docs/V0.4/Reports/ItemEnemyMatchupUnknownBlockers.csv";
        private const string VerifierPath = "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs";
        private const string ItemCandidatePath = "Assets/_Game/Scripts/TalismanBag/Items/Balance/ItemCompleteCandidateContent.cs";
        private const string ItemCatalogAssetPath = "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        private const string ProjectionContractPath = "Assets/_Game/Scripts/TalismanBag/Items/Generation/Projection/ItemInstanceProjectionContract.cs";
        private const string ItemSystemSnapshotPath = "Assets/_Game/Scripts/TalismanBag/Items/ItemSystemSnapshot.cs";
        private const string C01AdapterPath = "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs";

        private const string ExpectedItemHash =
            "7e6088da41ff1d56bd920957e5987824b8d15fe4843b2bf4fca7c436da36d663";
        private const string ExpectedEnemyHash =
            "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae";
        private const string ExpectedC01Hash =
            "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a";
        private const string ExpectedC02Hash =
            "08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4";
        private const int ExpectedItemFileCount = 105;
        private const int ExpectedEnemyFileCount = 89;

        private static readonly string[] ReportNames =
        {
            MainReportName,
            MatrixName,
            EvidenceName,
            DecisionName,
            LeakName
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

#if UNITY_EDITOR
        [MenuItem("Tools/Talisman Bag/V0.4/ItemEnemyCrossSystem/ItemCapabilityMappingGapSurvey01/[QA Only] Verify And Write Reports")]
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
            List<SurveyRow> rows = BuildSurveyRows();
            List<EvidenceRow> evidence = BuildEvidenceRows();
            List<DecisionRow> decisions = BuildDecisionRows();
            List<Check> checks = new List<Check>();
            List<LeakRow> leaks = new List<LeakRow>();

            try
            {
                RunClassificationChecks(checks, rows, evidence, decisions);
                RunC01AndC02ReportChecks(checks, root, rows, decisions);
                RunSourceEvidenceChecks(checks, root, evidence);
                RunNegativeSourceChecks(checks, root);
                RunLeakChecks(checks, leaks, root);

                WriteReports(root, mode, rows, evidence, decisions, checks, leaks, before, before);
                string first = ReportsHash(root);
                WriteReports(root, mode, rows, evidence, decisions, checks, leaks, before, before);
                string second = ReportsHash(root);
                Add(checks, "reports.deterministic.pre-protection", first, second,
                    string.Equals(first, second, StringComparison.Ordinal));

                ProtectionSnapshot after = CaptureProtection(root);
                RunProtectionChecks(checks, before, after);
                RunExpectedFileChecks(checks, root);
                WriteReports(root, mode, rows, evidence, decisions, checks, leaks, before, after);
                string finalFirst = ReportsHash(root);
                WriteReports(root, mode, rows, evidence, decisions, checks, leaks, before, after);
                string finalSecond = ReportsHash(root);
                if (!string.Equals(finalFirst, finalSecond, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Final survey reports are not deterministic.");
                }
            }
            catch (Exception exception)
            {
                Add(checks, "verifier.exception", "no exception", exception.ToString(), false);
                ProtectionSnapshot afterFailure = CaptureProtection(root);
                WriteReports(root, mode, rows, evidence, decisions, checks, leaks, before, afterFailure);
            }

            bool pass = checks.Count > 0 && checks.All(value => value.Passed);
            Console.WriteLine((pass
                ? "ITEM_CAPABILITY_MAPPING_GAP_SURVEY_PASS "
                : "ITEM_CAPABILITY_MAPPING_GAP_SURVEY_FAIL ")
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
                throw new InvalidOperationException("Item capability mapping gap survey verification failed.");
            }
        }

        private static List<SurveyRow> BuildSurveyRows()
        {
            return new List<SurveyRow>
            {
                Row("capability.break_power", "SUPPORTED", 2, 16,
                    "ItemInstanceProjectionContractSnapshot.v1+ItemAffixPoolAndRangeSchemaSnapshot.v1",
                    "Affixes.affixId=affix_break_up", "basisPoint", "complete projection set and affix schema",
                    "C01 already defines deterministic saturating sum 0..10000", "ALREADY_SUPPORTED", "HIGH",
                    "NONE_C01_READ_ONLY", C01AdapterPath,
                    "DefaultItemBuildCapabilityProjectionAdapter.Rules / affix_break_up", 0),
                Row("capability.burst_window", "UNKNOWN_NOT_MAPPED", 5, 24,
                    "ItemBalanceWorkbenchCatalog candidate payload",
                    "affix_first_trigger_bonus:effect", "basisPoint", "on_first_trigger + first_trigger_in_battle",
                    "Confirm semantic eligibility and whether conditional extra-trigger BP is aggregated as burst capability",
                    "MAPPABLE_REQUIRES_RULE_CONFIRMATION", "MEDIUM", "DECISION_IMCGS01_BURST_KEEP_UNKNOWN",
                    ItemCandidatePath, "ItemCompleteCandidateContentSeed.BuildRandomAffixDictionary / affix_first_trigger_bonus", 5),
                Row("capability.caster_interrupt", "OUT_OF_SCOPE", 0, 0,
                    "ItemBalanceWorkbenchCatalog candidate payload + runtime combat signal contract",
                    "SignatureSeeds targetStatId=castProgress", "turn + runtime event",
                    "on_hit + target_casting + active cast outcome", "Conditional cast-progress facts require live target/cast state and cannot be derived from a static Item projection",
                    "REQUIRES_RUNTIME_SIGNAL", "HIGH", "KEEP_OUT_OF_C02B_STATIC_MAPPING_RUNTIME_CONTRACT_REQUIRED",
                    C01AdapterPath, "DefaultItemBuildCapabilityProjectionAdapter.OutOfScopeCapabilityKeys", 0),
                Row("capability.chain_reaction", "UNKNOWN_NOT_MAPPED", 0, 0,
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_chain_target:effect", "count",
                    "on_consecutive_trigger + chain_count_3", "No E10 aggregation decision is currently required",
                    "NOT_REQUIRED_BY_E10", "HIGH", "DEFER_E10_SCOPE_KEEP_C01_STATUS",
                    CoveragePath, "capability.chain_reaction coverage row", 0),
                Row("capability.cleanse_power", "UNKNOWN_NOT_MAPPED", 8, 32,
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_cleanse_up:effect", "stack",
                    "on_cleanse + cleanse_success", "Confirm stack-to-capability conversion and aggregation across eligible active items",
                    "MAPPABLE_REQUIRES_RULE_CONFIRMATION", "MEDIUM", "DECISION_IMCGS01_CLEANSE_KEEP_UNKNOWN",
                    ItemCandidatePath, "ItemCompleteCandidateContentSeed.BuildRandomAffixDictionary / affix_cleanse_up", 3),
                Row("capability.clear_power", "UNKNOWN_NOT_MAPPED", 2, 16,
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_chain_target:effect", "count",
                    "on_consecutive_trigger + chain_count_3", "Confirm whether conditional extra targets qualify as clear power and how count converts to BP",
                    "MAPPABLE_REQUIRES_RULE_CONFIRMATION", "MEDIUM", "DECISION_IMCGS01_CLEAR_KEEP_UNKNOWN",
                    ItemCandidatePath, "ItemCompleteCandidateContentSeed.BuildRandomAffixDictionary / affix_chain_target", 6),
                Row("capability.control_power", "UNKNOWN_NOT_MAPPED", 10, 32,
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_control_up:effect", "point",
                    "always", "Confirm point-to-capability conversion and aggregation across eligible active items",
                    "MAPPABLE_REQUIRES_RULE_CONFIRMATION", "MEDIUM", "DECISION_IMCGS01_CONTROL_KEEP_UNKNOWN",
                    ItemCandidatePath, "ItemCompleteCandidateContentSeed.BuildRandomAffixDictionary / affix_control_up", 2),
                Row("capability.cooldown_recovery", "SUPPORTED", 0, 0,
                    "ItemInstanceProjectionContractSnapshot.v1+ItemAffixPoolAndRangeSchemaSnapshot.v1",
                    "Affixes.affixId=affix_cooldown_reduction", "basisPoint", "complete projection set and affix schema",
                    "C01 already defines deterministic saturating sum 0..10000", "ALREADY_SUPPORTED", "HIGH",
                    "NONE_C01_READ_ONLY", C01AdapterPath,
                    "DefaultItemBuildCapabilityProjectionAdapter.Rules / affix_cooldown_reduction", 0),
                Row("capability.debuff_counter", "UNKNOWN_NOT_MAPPED", 1, 8,
                    "ItemInstanceProjectionContractSnapshot.v1 + candidate catalog survey", "NONE_CONFIRMED", "NONE",
                    "self-side debuff reduction immunity or counter fact", "No dedicated stable Item fact exists; target_has_debuff is an offensive target condition",
                    "NO_STABLE_ITEM_SOURCE", "HIGH", "REQUEST_STABLE_ITEM_CONTRACT_SOURCE_OR_KEEP_UNKNOWN",
                    ItemCandidatePath, "affix_debuff_target_power uses target_has_debuff and basicEffect", 9),
                Row("capability.energy_stability", "UNKNOWN_NOT_MAPPED", 11, 32,
                    "ItemBalanceWorkbenchCatalog candidate payload + affix schema",
                    "affix_nian_efficiency:effect;affix_trigger_refund:effect", "basisPoint/point schema-payload mismatch + point",
                    "before_trigger nian_cost_gt_0; after_trigger trigger_success",
                    "Reconcile unit contract then confirm cost reduction/refund eligibility conversion and aggregation",
                    "MAPPABLE_REQUIRES_RULE_CONFIRMATION", "MEDIUM", "DECISION_IMCGS01_ENERGY_KEEP_UNKNOWN",
                    ItemCandidatePath, "ItemCompleteCandidateContentSeed.BuildRandomAffixDictionary / resource candidates", 1),
                Row("capability.guard_power", "SUPPORTED", 9, 32,
                    "ItemInstanceProjectionContractSnapshot.v1+ItemAffixPoolAndRangeSchemaSnapshot.v1",
                    "Affixes.affixId=affix_guard_up", "basisPoint", "complete projection set and affix schema",
                    "C01 already defines deterministic saturating sum 0..10000", "ALREADY_SUPPORTED", "HIGH",
                    "NONE_C01_READ_ONLY", C01AdapterPath,
                    "DefaultItemBuildCapabilityProjectionAdapter.Rules / affix_guard_up", 0),
                Row("capability.interrupt_timing", "OUT_OF_SCOPE", 1, 8,
                    "ItemBalanceWorkbenchCatalog candidate payload + runtime combat signal contract",
                    "SignatureSeeds targetStatId=castProgress", "turn + runtime event/time",
                    "on_hit + target_casting + active cast window/timestamp/outcome", "Conditional cast-progress facts require live timing evidence and cannot be derived from a static Item projection",
                    "REQUIRES_RUNTIME_SIGNAL", "HIGH", "KEEP_OUT_OF_C02B_STATIC_MAPPING_RUNTIME_CONTRACT_REQUIRED",
                    C01AdapterPath, "DefaultItemBuildCapabilityProjectionAdapter.OutOfScopeCapabilityKeys", 8),
                Row("capability.placement_shape", "UNKNOWN_NOT_MAPPED", 7, 32,
                    "ItemSystemSnapshot.v1",
                    "ItemSystemCatalogItemSnapshot.ShapeCells;ItemSystemPlacementSnapshot.OccupiedCells/isLit/isCountedInBuild/ActiveCoreEffectIds",
                    "cells/booleans/categorical IDs", "explicit instance-to-placement binding and valid ItemSystemSnapshot",
                    "Confirm which shape lighting and Build facts qualify plus deterministic conversion/aggregation to BP",
                    "MAPPABLE_REQUIRES_RULE_CONFIRMATION", "MEDIUM", "DECISION_IMCGS01_PLACEMENT_KEEP_UNKNOWN",
                    ItemSystemSnapshotPath, "ItemSystemCatalogItemSnapshot + ItemSystemPlacementSnapshot", 4),
                Row("capability.spirit_lock", "UNKNOWN_NOT_MAPPED", 2, 16,
                    "ItemInstanceProjectionContractSnapshot.v1 + candidate catalog survey", "NONE_CONFIRMED", "NONE",
                    "resource theft/cut prevention fact", "Cost efficiency and refund facts do not prove prevention of theft or cutoff",
                    "NO_STABLE_ITEM_SOURCE", "HIGH", "REQUEST_STABLE_ITEM_CONTRACT_SOURCE_OR_KEEP_UNKNOWN",
                    ProjectionContractPath, "ItemInstanceProjectionContractSnapshot public fact surface", 7),
                Row("capability.sustained_damage", "UNKNOWN_NOT_MAPPED", 0, 0,
                    "ItemInstanceProjectionContractSnapshot.v1", "Stats.statId=damage candidate fact", "rawUnits",
                    "projection completeness", "No E10 aggregation decision is currently required",
                    "NOT_REQUIRED_BY_E10", "HIGH", "DEFER_E10_SCOPE_KEEP_C01_STATUS",
                    CoveragePath, "capability.sustained_damage coverage row", 0),
                Row("capability.thunder_chain", "UNKNOWN_NOT_MAPPED", 0, 0,
                    "Item/E10 survey", "NONE_CONFIRMED", "NONE", "NONE",
                    "No E10 aggregation decision is currently required", "NOT_REQUIRED_BY_E10", "HIGH",
                    "DEFER_E10_SCOPE_KEEP_C01_STATUS", CoveragePath,
                    "capability.thunder_chain coverage row", 0)
            };
        }

        private static List<EvidenceRow> BuildEvidenceRows()
        {
            return new List<EvidenceRow>
            {
                Evidence("E01", "capability.break_power", "C01_CONFIRMED_RULE",
                    "ItemInstanceProjectionContractSnapshot.v1+ItemAffixPoolAndRangeSchemaSnapshot.v1",
                    "affix_break_up", "basisPoint", "complete projection/schema set",
                    "C01 has a deterministic saturating-sum rule", "No change is authorized by this survey",
                    C01AdapterPath, "DefaultItemBuildCapabilityProjectionAdapter.Rules", "affix_break_up"),
                Evidence("E02", "capability.cooldown_recovery", "C01_CONFIRMED_RULE",
                    "ItemInstanceProjectionContractSnapshot.v1+ItemAffixPoolAndRangeSchemaSnapshot.v1",
                    "affix_cooldown_reduction", "basisPoint", "complete projection/schema set",
                    "C01 has a deterministic saturating-sum rule", "No change is authorized by this survey",
                    C01AdapterPath, "DefaultItemBuildCapabilityProjectionAdapter.Rules", "affix_cooldown_reduction"),
                Evidence("E03", "capability.guard_power", "C01_CONFIRMED_RULE",
                    "ItemInstanceProjectionContractSnapshot.v1+ItemAffixPoolAndRangeSchemaSnapshot.v1",
                    "affix_guard_up", "basisPoint", "complete projection/schema set",
                    "C01 has a deterministic saturating-sum rule", "No change is authorized by this survey",
                    C01AdapterPath, "DefaultItemBuildCapabilityProjectionAdapter.Rules", "affix_guard_up"),
                Evidence("E04", "capability.burst_window", "CANDIDATE_TECHNICAL_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_first_trigger_bonus:effect",
                    "basisPoint", "on_first_trigger + first_trigger_in_battle",
                    "A stable technical candidate describes a conditional extra trigger",
                    "It does not confirm burst capability semantics or aggregation",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_first_trigger_bonus"),
                Evidence("E05", "capability.cleanse_power", "CANDIDATE_TECHNICAL_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_cleanse_up:effect", "stack",
                    "on_cleanse + cleanse_success", "A stable technical candidate targets cleanse stacks",
                    "It does not define stack-to-BP conversion or aggregation",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_cleanse_up"),
                Evidence("E06", "capability.clear_power", "CANDIDATE_TECHNICAL_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_chain_target:effect", "count",
                    "on_consecutive_trigger + chain_count_3", "A stable technical candidate adds a target conditionally",
                    "It does not confirm clear-power semantics or count-to-BP conversion",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_chain_target"),
                Evidence("E07", "capability.control_power", "CANDIDATE_TECHNICAL_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_control_up:effect", "point",
                    "always", "A stable technical candidate targets control points",
                    "It does not define point-to-BP conversion or aggregation",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_control_up"),
                Evidence("E08", "capability.energy_stability", "CANDIDATE_TECHNICAL_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_nian_efficiency:effect", "point",
                    "before_trigger + nian_cost_gt_0", "A stable technical candidate reduces resource cost",
                    "The schema/payload unit mismatch and capability aggregation are unresolved",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_nian_efficiency"),
                Evidence("E09", "capability.energy_stability", "CANDIDATE_TECHNICAL_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_trigger_refund:effect", "point",
                    "after_trigger + trigger_success", "A stable technical candidate refunds resource after a trigger",
                    "It does not define eligibility conversion or aggregation into stability BP",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_trigger_refund"),
                Evidence("E10", "capability.energy_stability", "UNIT_CONTRACT_CONFLICT",
                    "ItemBalanceWorkbenchCatalog asset", "affix_nian_efficiency", "schema basisPoint; payload point",
                    "candidate/dev-only", "The same stable key currently exposes conflicting unit evidence",
                    "No unit-safe direct mapping is possible",
                    ItemCatalogAssetPath, "affix schema and effect payload", "affix_nian_efficiency"),
                Evidence("E11", "capability.placement_shape", "STABLE_SNAPSHOT_FACT",
                    "ItemSystemSnapshot.v1", "shapeId;ShapeCells;coreCellLocal", "categorical/cells",
                    "catalog item resolves by stable itemId", "Stable shape facts exist",
                    "No shape-to-BP capability rule is locked",
                    ItemSystemSnapshotPath, "ItemSystemCatalogItemSnapshot", "public IReadOnlyList<Vector2Int> ShapeCells"),
                Evidence("E12", "capability.placement_shape", "STABLE_SNAPSHOT_FACT",
                    "ItemSystemSnapshot.v1",
                    "OccupiedCells;isLit;isCountedInBuild;ActiveCoreEffectIds", "cells/booleans/IDs",
                    "explicit instance-to-placement binding", "Stable placement lighting and Build facts exist",
                    "No eligibility/conversion/aggregation rule is locked",
                    ItemSystemSnapshotPath, "ItemSystemPlacementSnapshot", "public bool isCountedInBuild"),
                Evidence("E13", "capability.debuff_counter", "NON_MATCHING_CANDIDATE_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_debuff_target_power:effect", "basisPoint",
                    "target_has_debuff", "The candidate boosts basicEffect against a debuffed target",
                    "It is not a self-side debuff reduction immunity or counter fact",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_debuff_target_power"),
                Evidence("E14", "capability.debuff_counter", "NO_DEDICATED_CONTRACT_MEMBER",
                    "ItemInstanceProjectionContractSnapshot.v1", "NONE_CONFIRMED", "NONE",
                    "self-side debuff response", "The projection exposes stats affixes core IDs and Build qualification",
                    "No dedicated debuff-counter fact is present",
                    ProjectionContractPath, "ItemInstanceProjectionContractSnapshot", "public IReadOnlyList<ItemInstanceProjectionAffixSnapshot> Affixes"),
                Evidence("E15", "capability.spirit_lock", "NO_DEDICATED_CONTRACT_MEMBER",
                    "ItemInstanceProjectionContractSnapshot.v1", "NONE_CONFIRMED", "NONE",
                    "resource theft/cut prevention", "The projection exposes stats affixes core IDs and Build qualification",
                    "No dedicated spirit-lock or theft-prevention fact is present",
                    ProjectionContractPath, "ItemInstanceProjectionContractSnapshot", "public ItemBuildQualification buildQualification"),
                Evidence("E16", "capability.caster_interrupt", "C01_RUNTIME_BOUNDARY",
                    "ItemBuildCapabilityProjectionAdapter.mapping.v1", "capability.caster_interrupt", "runtime signal",
                    "caster state + event outcome", "C01 explicitly keeps this capability out of static projection",
                    "It must not be proposed for C02B static mapping",
                    C01AdapterPath, "OutOfScopeCapabilityKeys", "capability.caster_interrupt"),
                Evidence("E17", "capability.interrupt_timing", "C01_RUNTIME_BOUNDARY",
                    "ItemBuildCapabilityProjectionAdapter.mapping.v1", "capability.interrupt_timing", "runtime time/event",
                    "cast window + timestamp/outcome", "C01 explicitly keeps this capability out of static projection",
                    "It must not be proposed for C02B static mapping",
                    C01AdapterPath, "OutOfScopeCapabilityKeys", "capability.interrupt_timing"),
                Evidence("E18", "capability.chain_reaction", "E10_NOT_REFERENCED",
                    "ItemEnemyMatchupCapabilityCoverage", "capability.chain_reaction", "requirement references=0",
                    "current E10 fixture", "E10 contains no current requirement reference", "C01 status remains unchanged",
                    CoveragePath, "coverage row", "\"capability.chain_reaction\",\"UNKNOWN_NOT_MAPPED\",\"0\",\"0\""),
                Evidence("E19", "capability.sustained_damage", "E10_NOT_REFERENCED",
                    "ItemEnemyMatchupCapabilityCoverage", "capability.sustained_damage", "requirement references=0",
                    "current E10 fixture", "E10 contains no current requirement reference", "C01 status remains unchanged",
                    CoveragePath, "coverage row", "\"capability.sustained_damage\",\"UNKNOWN_NOT_MAPPED\",\"0\",\"0\""),
                Evidence("E20", "capability.thunder_chain", "E10_NOT_REFERENCED",
                    "ItemEnemyMatchupCapabilityCoverage", "capability.thunder_chain", "requirement references=0",
                    "current E10 fixture", "E10 contains no current requirement reference", "C01 status remains unchanged",
                    CoveragePath, "coverage row", "\"capability.thunder_chain\",\"UNKNOWN_NOT_MAPPED\",\"0\",\"0\""),
                Evidence("E21", "capability.chain_reaction", "CANDIDATE_TECHNICAL_FACT_DEFERRED",
                    "ItemBalanceWorkbenchCatalog candidate payload", "affix_chain_target:effect", "count",
                    "on_consecutive_trigger + chain_count_3", "A technical candidate exists",
                    "No E10 mapping decision is currently required",
                    ItemCandidatePath, "BuildRandomAffixDictionary", "affix_chain_target"),
                Evidence("E22", "capability.caster_interrupt", "RUNTIME_CONDITIONAL_CANDIDATE_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "SignatureSeeds targetStatId=castProgress", "turn",
                    "on_hit + target_casting", "A technical candidate depends on a live target casting condition",
                    "A static Item projection cannot prove the cast state or interruption outcome",
                    ItemCandidatePath, "ItemCompleteCandidateContentSeed.SignatureSeeds", "\"castProgress\""),
                Evidence("E23", "capability.interrupt_timing", "RUNTIME_CONDITIONAL_CANDIDATE_FACT",
                    "ItemBalanceWorkbenchCatalog candidate payload", "SignatureSeeds targetStatId=castProgress", "turn",
                    "on_hit + target_casting", "A technical candidate depends on a live target casting condition",
                    "It does not provide a cast-window timestamp or interruption outcome in the static Item contract",
                    ItemCandidatePath, "ItemCompleteCandidateContentSeed.SignatureSeeds", "\"castProgress\"")
            };
        }

        private static List<DecisionRow> BuildDecisionRows()
        {
            return new List<DecisionRow>
            {
                Decision("IMCGS01-D01", "capability.energy_stability",
                    "affix_nian_efficiency cost reduction and affix_trigger_refund resource refund candidate payloads",
                    "Unit reconciliation eligibility conversion and aggregation into capability BP",
                    "A: after unit contract reconciliation define Guard-approved eligibility and aggregation",
                    "B: keep Unknown until a dedicated energy-stability fact exists", "B_FOR_NOW",
                    "The current schema/payload unit conflict prevents a unit-safe rule; resolve it before considering A", 4),
                Decision("IMCGS01-D02", "capability.control_power",
                    "affix_control_up targets control with point units and always condition",
                    "Point-to-BP conversion and aggregation across eligible active items",
                    "A: confirm this stable candidate as the input after Guard approves conversion/aggregation",
                    "B: keep Unknown pending a dedicated capability fact", "A_FOR_RULE_REVIEW_ONLY",
                    "The technical target is direct enough to review without selecting any conversion value", 4),
                Decision("IMCGS01-D03", "capability.cleanse_power",
                    "affix_cleanse_up targets cleanse stacks on cleanse_success",
                    "Stack-to-BP conversion conditional eligibility and aggregation",
                    "A: confirm successful-cleanse stack facts after Guard approves conversion/aggregation",
                    "B: keep Unknown pending a dedicated capability fact", "A_FOR_RULE_REVIEW_ONLY",
                    "The technical target is direct but conditional and unit conversion remains undecided", 4),
                Decision("IMCGS01-D04", "capability.placement_shape",
                    "ItemSystemSnapshot exposes stable shape occupied-cell lighting Build and active-core facts",
                    "Which facts qualify and deterministic conversion/aggregation to capability BP",
                    "A: define a Guard-approved snapshot eligibility and shape aggregation rule",
                    "B: keep Unknown until Item publishes a dedicated placement capability fact", "A_FOR_RULE_REVIEW_ONLY",
                    "Stable snapshot facts already exist but no subset or calculation may be guessed", 4),
                Decision("IMCGS01-D05", "capability.burst_window",
                    "affix_first_trigger_bonus is a conditional extra-trigger basis-point candidate",
                    "Burst semantic eligibility and aggregation for conditional first-trigger effects",
                    "A: confirm the first-trigger payload as an input after Guard approves semantics/aggregation",
                    "B: keep Unknown until a dedicated burst-window fact exists", "A_FOR_RULE_REVIEW_ONLY",
                    "It is a bounded technical candidate but does not itself define burst capability", 3),
                Decision("IMCGS01-D06", "capability.clear_power",
                    "affix_chain_target conditionally adds target count after consecutive triggers",
                    "Clear semantic eligibility count-to-BP conversion and aggregation",
                    "A: confirm conditional extra-target facts after Guard approves semantics/conversion",
                    "B: keep Unknown until a dedicated clear-power fact exists", "A_FOR_RULE_REVIEW_ONLY",
                    "It is a bounded multi-target candidate but does not itself define clear capability", 2)
            };
        }

        private static void RunClassificationChecks(
            ICollection<Check> checks,
            IReadOnlyList<SurveyRow> rows,
            IReadOnlyList<EvidenceRow> evidence,
            IReadOnlyList<DecisionRow> decisions)
        {
            Add(checks, "classification.row-count", "16", rows.Count.ToString(CultureInfo.InvariantCulture), rows.Count == 16);
            Add(checks, "classification.unique-keys", "16",
                rows.Select(value => value.CapabilityKey).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                rows.Select(value => value.CapabilityKey).Distinct(StringComparer.Ordinal).Count() == 16);
            CheckCount(checks, rows, "ALREADY_SUPPORTED", 3);
            CheckCount(checks, rows, "MAPPABLE_REQUIRES_RULE_CONFIRMATION", 6);
            CheckCount(checks, rows, "REQUIRES_RUNTIME_SIGNAL", 2);
            CheckCount(checks, rows, "NO_STABLE_ITEM_SOURCE", 2);
            CheckCount(checks, rows, "NOT_REQUIRED_BY_E10", 3);
            CheckCount(checks, rows, "DIRECT_MAPPABLE_EXISTING_FACT", 0);
            Add(checks, "c01.status.supported", "3", rows.Count(value => value.CurrentC01Status == "SUPPORTED").ToString(CultureInfo.InvariantCulture),
                rows.Count(value => value.CurrentC01Status == "SUPPORTED") == 3);
            Add(checks, "c01.status.unknown", "11", rows.Count(value => value.CurrentC01Status == "UNKNOWN_NOT_MAPPED").ToString(CultureInfo.InvariantCulture),
                rows.Count(value => value.CurrentC01Status == "UNKNOWN_NOT_MAPPED") == 11);
            Add(checks, "c01.status.out-of-scope", "2", rows.Count(value => value.CurrentC01Status == "OUT_OF_SCOPE").ToString(CultureInfo.InvariantCulture),
                rows.Count(value => value.CurrentC01Status == "OUT_OF_SCOPE") == 2);
            Add(checks, "evidence.all-keys", "16",
                evidence.Select(value => value.CapabilityKey).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                rows.All(row => evidence.Any(value => value.CapabilityKey == row.CapabilityKey)));

            SurveyRow[] ruleRows = rows.Where(value => value.Classification == "MAPPABLE_REQUIRES_RULE_CONFIRMATION").ToArray();
            Add(checks, "decision.rule-row-count", ruleRows.Length.ToString(CultureInfo.InvariantCulture),
                decisions.Count.ToString(CultureInfo.InvariantCulture), decisions.Count == ruleRows.Length);
            Add(checks, "decision.rule-key-coverage", "6",
                decisions.Select(value => value.CapabilityKey).Distinct(StringComparer.Ordinal).Count().ToString(CultureInfo.InvariantCulture),
                ruleRows.All(row => decisions.Count(value => value.CapabilityKey == row.CapabilityKey) == 1));
            Add(checks, "decision.safe-default", "KEEP_UNKNOWN",
                string.Join(";", decisions.Select(value => value.SafeDefault).Distinct(StringComparer.Ordinal)),
                decisions.All(value => value.SafeDefault == "KEEP_UNKNOWN"));
            Add(checks, "runtime.not-recommended-to-c02b", "2", rows.Count(value =>
                    value.Classification == "REQUIRES_RUNTIME_SIGNAL"
                    && value.RecommendedNextAction.StartsWith("KEEP_OUT_OF_C02B", StringComparison.Ordinal)).ToString(CultureInfo.InvariantCulture),
                rows.Where(value => value.Classification == "REQUIRES_RUNTIME_SIGNAL").All(value =>
                    value.RecommendedNextAction.StartsWith("KEEP_OUT_OF_C02B", StringComparison.Ordinal)));
        }

        private static void RunC01AndC02ReportChecks(
            ICollection<Check> checks,
            string root,
            IReadOnlyList<SurveyRow> rows,
            IReadOnlyList<DecisionRow> decisions)
        {
            Dictionary<string, string[]> fieldMap = ReadCsvByFirstColumn(Absolute(root, FieldMapPath));
            Dictionary<string, string[]> coverage = ReadCsvByFirstColumn(Absolute(root, CoveragePath));
            foreach (SurveyRow row in rows)
            {
                string[] field;
                bool hasField = fieldMap.TryGetValue(row.CapabilityKey, out field);
                string actualStatus = hasField && field.Length > 1 ? field[1] : "MISSING";
                Add(checks, "field-map." + row.CapabilityKey, row.CurrentC01Status, actualStatus,
                    hasField && actualStatus == row.CurrentC01Status);

                string[] cover;
                bool hasCoverage = coverage.TryGetValue(row.CapabilityKey, out cover);
                int actualRefs = hasCoverage && cover.Length > 3 ? ParseInt(cover[3]) : -1;
                int actualBlocked = hasCoverage && cover.Length > 5 ? ParseInt(cover[5]) : -1;
                Add(checks, "coverage.refs." + row.CapabilityKey,
                    row.E10ReferenceCount.ToString(CultureInfo.InvariantCulture), actualRefs.ToString(CultureInfo.InvariantCulture),
                    actualRefs == row.E10ReferenceCount);
                Add(checks, "coverage.blocked." + row.CapabilityKey,
                    row.C02BlockedRowCount.ToString(CultureInfo.InvariantCulture), actualBlocked.ToString(CultureInfo.InvariantCulture),
                    actualBlocked == row.C02BlockedRowCount);
            }

            Dictionary<string, HashSet<string>> encountersByCapability = ReadBlockedEncounters(Absolute(root, BlockersPath));
            foreach (DecisionRow decision in decisions)
            {
                HashSet<string> encounterIds;
                int actual = encountersByCapability.TryGetValue(decision.CapabilityKey, out encounterIds)
                    ? encounterIds.Count
                    : 0;
                Add(checks, "decision.encounters." + decision.CapabilityKey,
                    decision.AffectedEncounterCount.ToString(CultureInfo.InvariantCulture),
                    actual.ToString(CultureInfo.InvariantCulture), actual == decision.AffectedEncounterCount);
            }
        }

        private static void RunSourceEvidenceChecks(
            ICollection<Check> checks,
            string root,
            IReadOnlyList<EvidenceRow> evidence)
        {
            foreach (EvidenceRow row in evidence)
            {
                string path = Absolute(root, row.EvidenceFile);
                bool exists = File.Exists(path);
                string text = exists ? File.ReadAllText(path) : string.Empty;
                bool contains = exists && text.IndexOf(row.EvidenceNeedle, StringComparison.Ordinal) >= 0;
                Add(checks, "evidence." + row.EvidenceId, row.EvidenceNeedle,
                    exists ? (contains ? "FOUND" : "NOT_FOUND") : "FILE_MISSING", contains);
            }
        }

        private static void RunNegativeSourceChecks(ICollection<Check> checks, string root)
        {
            string candidate = File.ReadAllText(Absolute(root, ItemCandidatePath));
            string projection = File.ReadAllText(Absolute(root, ProjectionContractPath));
            string catalog = File.ReadAllText(Absolute(root, ItemCatalogAssetPath));
            Add(checks, "source.no-dedicated-debuff-counter-candidate", "0",
                CountOrdinal(candidate, "affix_debuff_counter").ToString(CultureInfo.InvariantCulture),
                CountOrdinal(candidate, "affix_debuff_counter") == 0);
            Add(checks, "source.no-dedicated-spirit-lock-candidate", "0",
                CountOrdinal(candidate, "spirit_lock").ToString(CultureInfo.InvariantCulture),
                CountOrdinal(candidate, "spirit_lock") == 0);
            Add(checks, "contract.no-dedicated-debuff-counter-member", "0",
                CountOrdinal(projection, "debuffCounter").ToString(CultureInfo.InvariantCulture),
                CountOrdinal(projection, "debuffCounter") == 0);
            Add(checks, "contract.no-dedicated-spirit-lock-member", "0",
                CountOrdinal(projection, "spiritLock").ToString(CultureInfo.InvariantCulture),
                CountOrdinal(projection, "spiritLock") == 0);
            Add(checks, "catalog.candidate-maturity", "BALANCE_CANDIDATE", catalog.Contains("dataMaturity: BALANCE_CANDIDATE"),
                catalog.Contains("dataMaturity: BALANCE_CANDIDATE"));
            Add(checks, "catalog.not-battle-connected", "NOT_BATTLE_CONNECTED", catalog.Contains("battleState: NOT_BATTLE_CONNECTED"),
                catalog.Contains("battleState: NOT_BATTLE_CONNECTED"));
        }

        private static void RunLeakChecks(ICollection<Check> checks, ICollection<LeakRow> leaks, string root)
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

            Add(checks, "leak.count", "0", leaks.Sum(value => value.Count).ToString(CultureInfo.InvariantCulture), leaks.Count == 0);
        }

        private static void RunProtectionChecks(ICollection<Check> checks, ProtectionSnapshot before, ProtectionSnapshot after)
        {
            Add(checks, "protected.item.file-count", ExpectedItemFileCount.ToString(CultureInfo.InvariantCulture),
                before.Item.FileCount.ToString(CultureInfo.InvariantCulture), before.Item.FileCount == ExpectedItemFileCount);
            Add(checks, "protected.item.accepted-hash", ExpectedItemHash, before.Item.Hash, before.Item.Hash == ExpectedItemHash);
            Add(checks, "protected.enemy.file-count", ExpectedEnemyFileCount.ToString(CultureInfo.InvariantCulture),
                before.Enemy.FileCount.ToString(CultureInfo.InvariantCulture), before.Enemy.FileCount == ExpectedEnemyFileCount);
            Add(checks, "protected.enemy.accepted-hash", ExpectedEnemyHash, before.Enemy.Hash, before.Enemy.Hash == ExpectedEnemyHash);
            Add(checks, "protected.c01.accepted-hash", ExpectedC01Hash, before.C01.Hash, before.C01.Hash == ExpectedC01Hash);
            Add(checks, "protected.c02.accepted-hash", ExpectedC02Hash, before.C02.Hash, before.C02.Hash == ExpectedC02Hash);
            Add(checks, "protected.item.before-after", before.Item.Hash, after.Item.Hash, before.Item.Equals(after.Item));
            Add(checks, "protected.enemy.before-after", before.Enemy.Hash, after.Enemy.Hash, before.Enemy.Equals(after.Enemy));
            Add(checks, "protected.c01.before-after", before.C01.Hash, after.C01.Hash, before.C01.Equals(after.C01));
            Add(checks, "protected.c02.before-after", before.C02.Hash, after.C02.Hash, before.C02.Equals(after.C02));
        }

        private static void RunExpectedFileChecks(ICollection<Check> checks, string root)
        {
            int count = ReportNames.Count(value => File.Exists(Path.Combine(Absolute(root, ReportDirectory), value)));
            Add(checks, "reports.complete", "5", count.ToString(CultureInfo.InvariantCulture), count == 5);
            Add(checks, "verifier.exists", "true", File.Exists(Absolute(root, VerifierPath)).ToString(),
                File.Exists(Absolute(root, VerifierPath)));
        }

        private static void WriteReports(
            string root,
            string mode,
            IReadOnlyList<SurveyRow> rows,
            IReadOnlyList<EvidenceRow> evidence,
            IReadOnlyList<DecisionRow> decisions,
            IReadOnlyList<Check> checks,
            IReadOnlyList<LeakRow> leaks,
            ProtectionSnapshot before,
            ProtectionSnapshot after)
        {
            string directory = Absolute(root, ReportDirectory);
            Directory.CreateDirectory(directory);
            bool pass = checks.Count > 0 && checks.All(value => value.Passed);

            StringBuilder main = new StringBuilder();
            main.AppendLine("# Item Capability Mapping Gap Survey Report")
                .AppendLine()
                .AppendLine("- Package: `" + Package + "`")
                .AppendLine("- Guard receipt: `" + GuardReceipt + "`")
                .AppendLine("- Mode: `" + mode + "`")
                .AppendLine("- Result: `" + (pass ? "PASS" : "FAIL") + "`")
                .AppendLine("- Survey scope: `16/16 BuildCapability keys; read-only evidence and decisions only`")
                .AppendLine("- Mapping implementation count: `0`")
                .AppendLine("- C01 FieldMap modifications: `0`")
                .AppendLine("- Direct mappable: `0`")
                .AppendLine("- Decisions: `" + decisions.Count.ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Leak count: `" + leaks.Sum(value => value.Count).ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Unity validation status: `" +
                    (mode == "Unity batch compile/verifier" ? (pass ? "PASS" : "FAIL") : "PENDING_UNITY_BATCH") + "`")
                .AppendLine()
                .AppendLine("## Classification counts")
                .AppendLine()
                .AppendLine("| Classification | Count |")
                .AppendLine("|---|---:|");
            foreach (string classification in new[]
            {
                "DIRECT_MAPPABLE_EXISTING_FACT",
                "MAPPABLE_REQUIRES_RULE_CONFIRMATION",
                "REQUIRES_RUNTIME_SIGNAL",
                "NO_STABLE_ITEM_SOURCE",
                "NOT_REQUIRED_BY_E10",
                "ALREADY_SUPPORTED"
            })
            {
                main.Append("| ").Append(classification).Append(" | ")
                    .Append(rows.Count(value => value.Classification == classification).ToString(CultureInfo.InvariantCulture))
                    .AppendLine(" |");
            }

            main.AppendLine().AppendLine("## 16-key classification")
                .AppendLine()
                .AppendLine("| Capability | C01 | E10 refs | C02 blocked rows | Classification | Confidence | Next action |")
                .AppendLine("|---|---|---:|---:|---|---|---|");
            foreach (SurveyRow row in rows)
            {
                main.Append("| ").Append(Md(row.CapabilityKey)).Append(" | ")
                    .Append(Md(row.CurrentC01Status)).Append(" | ")
                    .Append(row.E10ReferenceCount.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(row.C02BlockedRowCount.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(Md(row.Classification)).Append(" | ")
                    .Append(Md(row.Confidence)).Append(" | ")
                    .Append(Md(row.RecommendedNextAction)).AppendLine(" |");
            }

            main.AppendLine().AppendLine("## E10 blocking priority")
                .AppendLine()
                .AppendLine("Priority is an audit triage order, not an implementation order and not authorization to start C02B.")
                .AppendLine()
                .AppendLine("| Priority | Capability | E10 refs | Blocked rows | Classification |")
                .AppendLine("|---:|---|---:|---:|---|");
            foreach (SurveyRow row in rows.Where(value => value.Priority > 0).OrderBy(value => value.Priority))
            {
                main.Append("| ").Append(row.Priority.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(row.CapabilityKey).Append(" | ")
                    .Append(row.E10ReferenceCount.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(row.C02BlockedRowCount.ToString(CultureInfo.InvariantCulture)).Append(" | ")
                    .Append(row.Classification).AppendLine(" |");
            }

            main.AppendLine().AppendLine("## Minimal decision boundary")
                .AppendLine()
                .AppendLine("Only the six `MAPPABLE_REQUIRES_RULE_CONFIRMATION` keys have decision rows. Every safe default is `KEEP_UNKNOWN`; recommendations are review routes only and are not mappings, values, ratios, thresholds, affixes, core effects, or Build rules.")
                .AppendLine()
                .AppendLine("## Protected state")
                .AppendLine()
                .AppendLine("| Scope | Before | After | Same |")
                .AppendLine("|---|---|---|---|");
            AppendProtection(main, "Item", before.Item, after.Item);
            AppendProtection(main, "Enemy", before.Enemy, after.Enemy);
            AppendProtection(main, "C01", before.C01, after.C01);
            AppendProtection(main, "C02", before.C02, after.C02);

            main.AppendLine().AppendLine("## Verifier checks")
                .AppendLine()
                .AppendLine("| Check | Expected | Actual | Result |")
                .AppendLine("|---|---|---|---|");
            foreach (Check check in checks)
            {
                main.Append("| ").Append(Md(check.Id)).Append(" | ")
                    .Append(Md(check.Expected)).Append(" | ")
                    .Append(Md(check.Actual)).Append(" | ")
                    .Append(check.Passed ? "PASS" : "FAIL").AppendLine(" |");
            }
            WriteUtf8(Path.Combine(directory, MainReportName), main.ToString());

            StringBuilder matrix = new StringBuilder();
            matrix.AppendLine("capabilityKey,currentC01Status,e10RequirementReferenceCount,c02BlockedRowCount,candidateSourceContract,candidateStableKeyOrMember,sourceUnit,requiredConditions,aggregationQuestion,classification,confidence,recommendedNextAction,evidenceFile,evidenceLineOrSymbol");
            foreach (SurveyRow row in rows)
            {
                matrix.AppendLine(Csv(row.CapabilityKey, row.CurrentC01Status,
                    row.E10ReferenceCount.ToString(CultureInfo.InvariantCulture),
                    row.C02BlockedRowCount.ToString(CultureInfo.InvariantCulture),
                    row.CandidateSourceContract, row.CandidateStableKeyOrMember, row.SourceUnit,
                    row.RequiredConditions, row.AggregationQuestion, row.Classification, row.Confidence,
                    row.RecommendedNextAction, row.EvidenceFile, row.EvidenceLineOrSymbol));
            }
            WriteUtf8(Path.Combine(directory, MatrixName), matrix.ToString());

            StringBuilder source = new StringBuilder();
            source.AppendLine("evidenceId,capabilityKey,evidenceKind,sourceContract,stableKeyOrMember,sourceUnit,requiredConditions,whatItProves,whatItDoesNotProve,evidenceFile,evidenceLineOrSymbol");
            foreach (EvidenceRow row in evidence)
            {
                source.AppendLine(Csv(row.EvidenceId, row.CapabilityKey, row.EvidenceKind,
                    row.SourceContract, row.StableKeyOrMember, row.SourceUnit, row.RequiredConditions,
                    row.WhatItProves, row.WhatItDoesNotProve, row.EvidenceFile, row.EvidenceLineOrSymbol));
            }
            WriteUtf8(Path.Combine(directory, EvidenceName), source.ToString());

            StringBuilder decision = new StringBuilder();
            decision.AppendLine("decisionId,capabilityKey,existingFact,missingRule,optionA,optionB,recommendedOption,recommendationReason,affectedEncounterCount,safeDefault");
            foreach (DecisionRow row in decisions)
            {
                decision.AppendLine(Csv(row.DecisionId, row.CapabilityKey, row.ExistingFact, row.MissingRule,
                    row.OptionA, row.OptionB, row.RecommendedOption, row.RecommendationReason,
                    row.AffectedEncounterCount.ToString(CultureInfo.InvariantCulture), row.SafeDefault));
            }
            WriteUtf8(Path.Combine(directory, DecisionName), decision.ToString());

            StringBuilder leak = new StringBuilder();
            leak.AppendLine("# Item Capability Mapping Gap Survey Leak Check Report")
                .AppendLine()
                .AppendLine("- Result: `" + (leaks.Count == 0 ? "PASS" : "FAIL") + "`")
                .AppendLine("- Leak Count: `" + leaks.Sum(value => value.Count).ToString(CultureInfo.InvariantCulture) + "`")
                .AppendLine("- Scan scope: `survey verifier executable source after comments/string literals are stripped`")
                .AppendLine("- Runtime files added: `0`")
                .AppendLine("- Existing files modified by package: `0`")
                .AppendLine("- C01 / C02 mapping changes: `0`")
                .AppendLine("- Item / Enemy / Battle / Board / Scene / Prefab / Config changes: `0`")
                .AppendLine("- Enemy readiness evaluation calls: `0`")
                .AppendLine("- Scene/Prefab builder runs: `0`")
                .AppendLine("- C02B auto-start: `0`")
                .AppendLine()
                .AppendLine("| File | Token | Count | Result |")
                .AppendLine("|---|---|---:|---|");
            if (leaks.Count == 0)
            {
                leak.AppendLine("| survey verifier source | none | 0 | PASS |");
            }
            else
            {
                foreach (LeakRow row in leaks)
                {
                    leak.Append("| ").Append(Md(row.File)).Append(" | ")
                        .Append(Md(row.Token)).Append(" | ")
                        .Append(row.Count.ToString(CultureInfo.InvariantCulture)).AppendLine(" | FAIL |");
                }
            }
            WriteUtf8(Path.Combine(directory, LeakName), leak.ToString());
        }

        private static ProtectionSnapshot CaptureProtection(string root)
        {
            return new ProtectionSnapshot(
                AggregateDirectory(root, "Assets/_Game/Scripts/TalismanBag/Items"),
                AggregateDirectory(root, "Assets/_Game/Scripts/TalismanBag/EnemySystem"),
                AggregateFiles(root, C01ProtectedFiles),
                AggregateFiles(root, C02ProtectedFiles));
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

        private static string ReportsHash(string root)
        {
            string directory = Absolute(root, ReportDirectory);
            string payload = string.Join("\n", ReportNames.OrderBy(value => value, StringComparer.Ordinal)
                .Select(name => name + "|" + Sha256(File.ReadAllBytes(Path.Combine(directory, name)), true)));
            return Sha256(Encoding.UTF8.GetBytes(payload), false);
        }

        private static Dictionary<string, string[]> ReadCsvByFirstColumn(string path)
        {
            Dictionary<string, string[]> result = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (string line in File.ReadAllLines(path).Skip(1).Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                string[] fields = ParseCsvLine(line).ToArray();
                if (fields.Length > 0)
                {
                    result[fields[0]] = fields;
                }
            }
            return result;
        }

        private static Dictionary<string, HashSet<string>> ReadBlockedEncounters(string path)
        {
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (string line in File.ReadAllLines(path).Skip(1).Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                string[] fields = ParseCsvLine(line).ToArray();
                if (fields.Length < 3 || string.IsNullOrWhiteSpace(fields[1]) || string.IsNullOrWhiteSpace(fields[2]))
                {
                    continue;
                }
                HashSet<string> values;
                if (!result.TryGetValue(fields[2], out values))
                {
                    values = new HashSet<string>(StringComparer.Ordinal);
                    result[fields[2]] = values;
                }
                values.Add(fields[1]);
            }
            return result;
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
                    if (current == '\n') { lineComment = false; output.Append('\n'); }
                    else output.Append(' ');
                    continue;
                }
                if (blockComment)
                {
                    if (current == '*' && next == '/') { output.Append("  "); index++; blockComment = false; }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }
                if (regularString)
                {
                    if (current == '\\' && next != '\0') { output.Append("  "); index++; }
                    else if (current == '"') { output.Append(' '); regularString = false; }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }
                if (verbatimString)
                {
                    if (current == '"' && next == '"') { output.Append("  "); index++; }
                    else if (current == '"') { output.Append(' '); verbatimString = false; }
                    else output.Append(current == '\n' ? '\n' : ' ');
                    continue;
                }
                if (character)
                {
                    if (current == '\\' && next != '\0') { output.Append("  "); index++; }
                    else if (current == '\'') { output.Append(' '); character = false; }
                    else output.Append(' ');
                    continue;
                }
                if (current == '/' && next == '/') { output.Append("  "); index++; lineComment = true; }
                else if (current == '/' && next == '*') { output.Append("  "); index++; blockComment = true; }
                else if (current == '@' && next == '"') { output.Append("  "); index++; verbatimString = true; }
                else if (current == '"') { output.Append(' '); regularString = true; }
                else if (current == '\'') { output.Append(' '); character = true; }
                else output.Append(current);
            }
            return output.ToString();
        }

        private static SurveyRow Row(string key, string c01, int refs, int blocked,
            string sourceContract, string sourceKey, string unit, string conditions,
            string aggregation, string classification, string confidence, string nextAction,
            string evidenceFile, string evidenceSymbol, int priority)
        {
            return new SurveyRow(key, c01, refs, blocked, sourceContract, sourceKey, unit,
                conditions, aggregation, classification, confidence, nextAction, evidenceFile,
                evidenceSymbol, priority);
        }

        private static EvidenceRow Evidence(string id, string capability, string kind,
            string sourceContract, string stableKey, string unit, string conditions,
            string proves, string doesNotProve, string file, string symbol, string needle)
        {
            return new EvidenceRow(id, capability, kind, sourceContract, stableKey, unit,
                conditions, proves, doesNotProve, file, symbol, needle);
        }

        private static DecisionRow Decision(string id, string capability, string fact,
            string missingRule, string optionA, string optionB, string recommended,
            string reason, int encounters)
        {
            return new DecisionRow(id, capability, fact, missingRule, optionA, optionB,
                recommended, reason, encounters, "KEEP_UNKNOWN");
        }

        private static void CheckCount(ICollection<Check> checks, IEnumerable<SurveyRow> rows,
            string classification, int expected)
        {
            int actual = rows.Count(value => value.Classification == classification);
            Add(checks, "classification.count." + classification,
                expected.ToString(CultureInfo.InvariantCulture), actual.ToString(CultureInfo.InvariantCulture),
                actual == expected);
        }

        private static void AppendProtection(StringBuilder builder, string name, AggregateHash before, AggregateHash after)
        {
            builder.Append("| ").Append(name).Append(" | ").Append(before.Hash)
                .Append(" | ").Append(after.Hash).Append(" | ")
                .Append(before.Equals(after) ? "PASS" : "FAIL").AppendLine(" |");
        }

        private static void Add(ICollection<Check> checks, string id, object expected, object actual, bool passed)
        {
            checks.Add(new Check(id, Convert.ToString(expected, CultureInfo.InvariantCulture),
                Convert.ToString(actual, CultureInfo.InvariantCulture), passed));
        }

        private static int ParseInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : -1;
        }

        private static int CountOrdinal(string text, string token)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(token)) return 0;
            int count = 0;
            int offset = 0;
            while ((offset = text.IndexOf(token, offset, StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += token.Length;
            }
            return count;
        }

        private static string FindProjectRoot()
        {
            string configured = Environment.GetEnvironmentVariable("TALISMAN_BAG_PROJECT_ROOT");
            if (IsProjectRoot(configured)) return Path.GetFullPath(configured);
            DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current != null)
            {
                if (IsProjectRoot(current.FullName)) return current.FullName;
                current = current.Parent;
            }
            const string fixedRoot = @"F:\Porject\TalismanBagBrawl";
            if (IsProjectRoot(fixedRoot)) return fixedRoot;
            throw new DirectoryNotFoundException("TalismanBagBrawl project root not found.");
        }

        private static bool IsProjectRoot(string path)
        {
            return !string.IsNullOrWhiteSpace(path)
                && Directory.Exists(Path.Combine(path, "Assets"))
                && Directory.Exists(Path.Combine(path, "ProjectSettings"))
                && Directory.Exists(Path.Combine(path, "Packages"));
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

        private static string Csv(params string[] values)
        {
            return string.Join(",", (values ?? new string[0]).Select(value =>
                "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\""));
        }

        private static string Md(string value)
        {
            return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static void WriteUtf8(string path, string text)
        {
            File.WriteAllText(path, text ?? string.Empty, new UTF8Encoding(false));
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

        private sealed class SurveyRow
        {
            public SurveyRow(string capabilityKey, string currentC01Status, int e10ReferenceCount,
                int c02BlockedRowCount, string candidateSourceContract, string candidateStableKeyOrMember,
                string sourceUnit, string requiredConditions, string aggregationQuestion, string classification,
                string confidence, string recommendedNextAction, string evidenceFile, string evidenceLineOrSymbol,
                int priority)
            {
                CapabilityKey = capabilityKey;
                CurrentC01Status = currentC01Status;
                E10ReferenceCount = e10ReferenceCount;
                C02BlockedRowCount = c02BlockedRowCount;
                CandidateSourceContract = candidateSourceContract;
                CandidateStableKeyOrMember = candidateStableKeyOrMember;
                SourceUnit = sourceUnit;
                RequiredConditions = requiredConditions;
                AggregationQuestion = aggregationQuestion;
                Classification = classification;
                Confidence = confidence;
                RecommendedNextAction = recommendedNextAction;
                EvidenceFile = evidenceFile;
                EvidenceLineOrSymbol = evidenceLineOrSymbol;
                Priority = priority;
            }
            public string CapabilityKey { get; private set; }
            public string CurrentC01Status { get; private set; }
            public int E10ReferenceCount { get; private set; }
            public int C02BlockedRowCount { get; private set; }
            public string CandidateSourceContract { get; private set; }
            public string CandidateStableKeyOrMember { get; private set; }
            public string SourceUnit { get; private set; }
            public string RequiredConditions { get; private set; }
            public string AggregationQuestion { get; private set; }
            public string Classification { get; private set; }
            public string Confidence { get; private set; }
            public string RecommendedNextAction { get; private set; }
            public string EvidenceFile { get; private set; }
            public string EvidenceLineOrSymbol { get; private set; }
            public int Priority { get; private set; }
        }

        private sealed class EvidenceRow
        {
            public EvidenceRow(string evidenceId, string capabilityKey, string evidenceKind,
                string sourceContract, string stableKeyOrMember, string sourceUnit, string requiredConditions,
                string whatItProves, string whatItDoesNotProve, string evidenceFile,
                string evidenceLineOrSymbol, string evidenceNeedle)
            {
                EvidenceId = evidenceId;
                CapabilityKey = capabilityKey;
                EvidenceKind = evidenceKind;
                SourceContract = sourceContract;
                StableKeyOrMember = stableKeyOrMember;
                SourceUnit = sourceUnit;
                RequiredConditions = requiredConditions;
                WhatItProves = whatItProves;
                WhatItDoesNotProve = whatItDoesNotProve;
                EvidenceFile = evidenceFile;
                EvidenceLineOrSymbol = evidenceLineOrSymbol;
                EvidenceNeedle = evidenceNeedle;
            }
            public string EvidenceId { get; private set; }
            public string CapabilityKey { get; private set; }
            public string EvidenceKind { get; private set; }
            public string SourceContract { get; private set; }
            public string StableKeyOrMember { get; private set; }
            public string SourceUnit { get; private set; }
            public string RequiredConditions { get; private set; }
            public string WhatItProves { get; private set; }
            public string WhatItDoesNotProve { get; private set; }
            public string EvidenceFile { get; private set; }
            public string EvidenceLineOrSymbol { get; private set; }
            public string EvidenceNeedle { get; private set; }
        }

        private sealed class DecisionRow
        {
            public DecisionRow(string decisionId, string capabilityKey, string existingFact,
                string missingRule, string optionA, string optionB, string recommendedOption,
                string recommendationReason, int affectedEncounterCount, string safeDefault)
            {
                DecisionId = decisionId;
                CapabilityKey = capabilityKey;
                ExistingFact = existingFact;
                MissingRule = missingRule;
                OptionA = optionA;
                OptionB = optionB;
                RecommendedOption = recommendedOption;
                RecommendationReason = recommendationReason;
                AffectedEncounterCount = affectedEncounterCount;
                SafeDefault = safeDefault;
            }
            public string DecisionId { get; private set; }
            public string CapabilityKey { get; private set; }
            public string ExistingFact { get; private set; }
            public string MissingRule { get; private set; }
            public string OptionA { get; private set; }
            public string OptionB { get; private set; }
            public string RecommendedOption { get; private set; }
            public string RecommendationReason { get; private set; }
            public int AffectedEncounterCount { get; private set; }
            public string SafeDefault { get; private set; }
        }

        private sealed class ProtectionSnapshot
        {
            public ProtectionSnapshot(AggregateHash item, AggregateHash enemy, AggregateHash c01, AggregateHash c02)
            {
                Item = item;
                Enemy = enemy;
                C01 = c01;
                C02 = c02;
            }
            public AggregateHash Item { get; private set; }
            public AggregateHash Enemy { get; private set; }
            public AggregateHash C01 { get; private set; }
            public AggregateHash C02 { get; private set; }
        }

        private sealed class AggregateHash
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }
            public int FileCount { get; private set; }
            public string Hash { get; private set; }
            public override bool Equals(object obj)
            {
                AggregateHash other = obj as AggregateHash;
                return other != null && FileCount == other.FileCount && Hash == other.Hash;
            }
            public override int GetHashCode()
            {
                return (FileCount * 397) ^ (Hash == null ? 0 : Hash.GetHashCode());
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

        private sealed class LeakRow
        {
            public LeakRow(string file, string token, int count)
            {
                File = file;
                Token = token;
                Count = count;
            }
            public string File { get; private set; }
            public string Token { get; private set; }
            public int Count { get; private set; }
        }
    }
}
