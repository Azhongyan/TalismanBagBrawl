using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public static class LayoutResilienceRequirementMigrationSurveyVerifier
    {
        private const string ExpectedSignature =
            "sha256:4a77a47181d41ecd18f589f86be911ce08fc466725807a16c42ab1d3e5068126";
        private const string FullE10Signature =
            "sha256:377fb8d27d35dd127b915944d8a00c1b17afd6a580254dc21bc8d44e51a815a3";
        private const string PlayerSafeE10Signature =
            "sha256:aa1c4ccde3fccaadc09da4bcb0e72bf53ee9c110ed84d366e20ebd875bf4775d";
        private const string ReportDirectory = "Docs/V0.4/Reports";
        private const string CandidatePath = ReportDirectory +
            "/LayoutResilienceRequirementMigrationSurveyCandidateRows.csv";
        private const string CollisionPath = ReportDirectory +
            "/LayoutResilienceRequirementMigrationSurveyIdentityCollisionMatrix.csv";
        private const string GapPath = ReportDirectory +
            "/LayoutResilienceRequirementMigrationSurveyPressureFactGap.csv";
        private const string ContextPath = ReportDirectory +
            "/LayoutResilienceRequirementMigrationSurveyContextMatrix.csv";
        private const string DecisionPath = ReportDirectory +
            "/LayoutResilienceRequirementMigrationSurveyUserDecisionSheet.csv";

        private static readonly string[] OutputPaths =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementMigrationSurveyVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementMigrationSurveyVerifier.cs.meta",
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyReport.md",
            CandidatePath,
            CollisionPath,
            GapPath,
            ContextPath,
            DecisionPath,
            ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyLeakCheckReport.md"
        };

        private static readonly CandidateExpectation[] ExpectedCandidates =
        {
            new CandidateExpectation("1", "dev_boss_bronze_formation_general",
                "dev_boss_bronze_formation_general.required", "Required", "All",
                "capability.placement_shape", "4631",
                "layout_resilience.bronze_formation_general.placement_shape"),
            new CandidateExpectation("2", "dev_enemy_formation_eye_problem",
                "dev_enemy_formation_eye_problem.required", "Required", "All",
                "capability.placement_shape", "5340",
                "layout_resilience.formation_eye.placement_shape"),
            new CandidateExpectation("3", "dev_enemy_polluted_tile_problem",
                "dev_enemy_polluted_tile_problem.required", "Required", "All",
                "capability.placement_shape", "5383",
                "layout_resilience.polluted_tile.placement_shape"),
            new CandidateExpectation("4", "dev_enemy_spirit_thief_problem",
                "dev_enemy_spirit_thief_problem.recommended", "Recommended", "Any",
                "capability.placement_shape", "2884",
                "layout_resilience.spirit_thief.placement_shape")
        };

#if UNITY_EDITOR
        [MenuItem("Tools/TalismanBag/Verify Layout Resilience Requirement Migration Survey")]
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
                    "LAYOUT_RESILIENCE_REQUIREMENT_MIGRATION_SURVEY PASS");
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
            string protectionBefore = CaptureProtectedState(root);
            List<Check> checks = new List<Check>();
            List<Row> candidates = LoadRows(root, CandidatePath);
            List<Row> collisions = LoadRows(root, CollisionPath);
            List<Row> gaps = LoadRows(root, GapPath);
            List<Row> contexts = LoadRows(root, ContextPath);
            List<Row> decisions = LoadRows(root, DecisionPath);
            string main = Read(root,
                ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyReport.md");
            string leak = Read(root,
                ReportDirectory + "/LayoutResilienceRequirementMigrationSurveyLeakCheckReport.md");
            List<CanonicalSource> canonicalSources = LoadCanonicalSources(root);
            string canonical = CanonicalSignature(canonicalSources,
                FullE10Signature, PlayerSafeE10Signature);

            VerifyOutputs(checks, root, main, leak, candidates, collisions, gaps,
                contexts, decisions, canonical);
            VerifyCandidates(checks, root, candidates);
            VerifyCollisions(checks, collisions);
            VerifyGaps(checks, gaps);
            VerifyContexts(checks, contexts);
            VerifyDecisions(checks, decisions);
            VerifyCanonical(checks, root, canonicalSources, canonical);
            VerifyProtected(checks, root, protectionBefore);

            if (checks.Count != 112)
            {
                throw new InvalidOperationException("Verifier definition error: expected " +
                    "112 assertions but built " + checks.Count.ToString(
                        CultureInfo.InvariantCulture) + ".");
            }
            int failed = checks.Count(value => !value.Passed);
            string failures = string.Join("; ", checks.Where(value => !value.Passed)
                .Select(value => value.Id + " expected=[" + value.Expected +
                    "] actual=[" + value.Actual + "]"));
            string message = "LayoutResilienceRequirementMigrationSurvey verifier: " +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) +
                "/112 PASS; fixtures=20/20; signature=" + canonical +
                (failed == 0 ? string.Empty : "; failures=" + failures);
            return new Summary(failed, message);
        }

        private static void VerifyOutputs(
            ICollection<Check> checks,
            string root,
            string main,
            string leak,
            IList<Row> candidates,
            IList<Row> collisions,
            IList<Row> gaps,
            IList<Row> contexts,
            IList<Row> decisions,
            string canonical)
        {
            Add(checks, "outputs.count", "9", OutputPaths.Length.ToString(
                CultureInfo.InvariantCulture), OutputPaths.Length == 9);
            Add(checks, "outputs.exist", "9",
                OutputPaths.Count(value => File.Exists(Absolute(root, value))).ToString(
                    CultureInfo.InvariantCulture),
                OutputPaths.All(value => File.Exists(Absolute(root, value))));
            Add(checks, "report.package", "present", main,
                Has(main, "V0.4-LayoutResilienceRequirementMigrationSurvey01"));
            Add(checks, "report.receipts", "all three", main,
                Has(main, "GUARD_RETURN_SPLIT_LAYOUTRESILIENCEREQUIREMENTMIGRATION01") &&
                Has(main, "ENEMY_GUARD_RETURN_SPLIT_LAYOUTRESILIENCEREQUIREMENTMIGRATION01") &&
                Has(main, "CAPABILITY_ALGORITHM_GUARD_CONFIRM_LAYOUTRESILIENCEREQUIREMENTMIGRATIONSURVEY01"));
            Add(checks, "report.schema", "LayoutResilienceRequirementMigrationSurvey.v1",
                main, Has(main, "LayoutResilienceRequirementMigrationSurvey.v1"));
            Add(checks, "report.disposition", "fixed triple", main,
                Has(main, "MIGRATION_CANDIDATE") &&
                Has(main, "APPLICABILITY_UNRESOLVED") &&
                Has(main, "USER_AUTHORING_REQUIRED"));
            Add(checks, "report.impact", "32/32 and zero reduction", main,
                Has(main, "32/32 blocked") && Has(main, "Actual Unknown reduction: 0") &&
                Has(main, "Behavior changes: 0"));
            Add(checks, "report.calls", "P3/P4/Evaluator zero", main,
                Has(main, "P3 assembler calls: 0") &&
                Has(main, "P4 readiness-consumer calls: 0") &&
                Has(main, "N01C evaluator calls: 0"));
            Add(checks, "report.leak", "Leak Count: 0", leak,
                Has(leak, "Leak Count: 0") && Has(leak, canonical));

            List<bool> fixtures = BuildFixtures(candidates, collisions, gaps,
                contexts, decisions, canonical);
            Add(checks, "fixtures.count", "20", fixtures.Count.ToString(
                CultureInfo.InvariantCulture), fixtures.Count == 20);
            Add(checks, "fixtures.pass", "20", fixtures.Count(value => value).ToString(
                CultureInfo.InvariantCulture), fixtures.All(value => value));
        }

        private static List<bool> BuildFixtures(
            IList<Row> candidates,
            IList<Row> collisions,
            IList<Row> gaps,
            IList<Row> contexts,
            IList<Row> decisions,
            string canonical)
        {
            List<bool> result = new List<bool>();
            for (int index = 0; index < ExpectedCandidates.Length; index++)
            {
                CandidateExpectation expected = ExpectedCandidates[index];
                result.Add(candidates.Any(row => ExactCandidate(row, expected)));
            }
            foreach (CandidateExpectation expected in ExpectedCandidates)
            {
                result.Add(collisions.Count(row =>
                    row["requirementGroupId"] == expected.Group &&
                    row["groupOnlyIdentityUnique"] == "false") > 1);
            }
            result.Add(collisions.All(row =>
                row["groupOnlyBindingDisposition"] == "REJECTED"));
            result.Add(candidates.All(row =>
                row["candidateIdApprovalState"] == "USER_DECISION_REQUIRED"));
            result.Add(candidates.Select(row => row["candidateMigrationRequirementIdA"])
                .Distinct(StringComparer.Ordinal).Count() == 4);
            result.Add(candidates.All(row => row["role"] == "Required" ||
                row["role"] == "Recommended"));
            result.Add(candidates.All(row => row["candidateActivated"] == "false"));
            result.Add(gaps.Count == 32 && gaps.All(row =>
                row["notApplicableInference"] == "REJECTED"));
            result.Add(gaps.All(row => row["approvedValue"] == "<null>"));
            result.Add(gaps.Select(row => row["fieldName"]).Distinct(
                StringComparer.Ordinal).Count() == 8);
            result.Add(candidates.All(row => row["converted"] == "false" &&
                row["evaluated"] == "false" && row["compared"] == "false"));
            result.Add(contexts.Count(row => row["scope"] == "ACTIVE_E10") == 7 &&
                contexts.Count(row => row["scope"] == "BROADER_E03_ONLY") == 3);
            result.Add(decisions.All(row =>
                row["decisionStatus"] == "USER_DECISION_REQUIRED"));
            result.Add(IsSignature(canonical));
            return result;
        }

        private static void VerifyCandidates(
            ICollection<Check> checks,
            string root,
            IList<Row> candidates)
        {
            List<Row> e10 = LoadRows(root,
                ReportDirectory + "/DevEncounterSeedPressureWindowRows.csv");
            string n01bFixtures = Read(root, ReportDirectory +
                "/EnemyRequirementChannelApplicabilitySchemaContractFixtureRows.csv");
            foreach (CandidateExpectation expected in ExpectedCandidates)
            {
                Row row = candidates.SingleOrDefault(value =>
                    value["candidateOrdinal"] == expected.Ordinal);
                Add(checks, "candidate." + expected.Ordinal + ".exact", "exact",
                    row == null ? "<missing>" : row.Raw,
                    row != null && ExactCandidate(row, expected));
                int sourceMatches = e10.Count(value =>
                    value["entityType"] == "Requirement" &&
                    value["ownerId"] == expected.Owner &&
                    value["entityId"] == expected.Group &&
                    value["role"] == expected.Role &&
                    value["matchMode"] == expected.Mode &&
                    value["referenceKind"] == "BuildCapability" &&
                    value["referenceId"] == expected.Key &&
                    value["value"] == expected.Bp);
                Add(checks, "candidate." + expected.Ordinal + ".e10-source", "1",
                    sourceMatches.ToString(CultureInfo.InvariantCulture),
                    sourceMatches == 1 && row != null &&
                    row["evidencePath"] ==
                        "Docs/V0.4/Reports/DevEncounterSeedPressureWindowRows.csv");
                string id = row == null ? string.Empty :
                    row["candidateMigrationRequirementIdA"];
                Add(checks, "candidate." + expected.Ordinal + ".id", expected.Id,
                    id, id == expected.Id && !n01bFixtures.Contains(id) &&
                    row["candidateMigrationRequirementIdB"] == "<null>");
                Add(checks, "candidate." + expected.Ordinal + ".dispositions",
                    "fixed triple", row == null ? "<missing>" : row.Raw,
                    row != null && row["routeDisposition"] == "MIGRATION_CANDIDATE" &&
                    row["applicabilityDisposition"] == "APPLICABILITY_UNRESOLVED" &&
                    row["pressureAuthoringDisposition"] == "USER_AUTHORING_REQUIRED");
                Add(checks, "candidate." + expected.Ordinal + ".bp-quarantine",
                    "true/false/false/false/false/false/false",
                    row == null ? "<missing>" : row.Raw,
                    row != null && row["legacyBpQuarantined"] == "true" &&
                    False(row, "evaluated", "converted", "compared",
                        "includedInN01CInput", "includedInP4Result",
                        "includedInStructuralCanonicalAsThreshold"));
                Add(checks, "candidate." + expected.Ordinal + ".inactive",
                    "USER_DECISION_REQUIRED/false/NONE",
                    row == null ? "<missing>" : row.Raw,
                    row != null &&
                    row["candidateIdApprovalState"] == "USER_DECISION_REQUIRED" &&
                    row["declaredChannelCandidateA"] == "StructuralPredicate" &&
                    row["declaredChannelCandidateB"] == "KEEP_DEFERRED" &&
                    row["declaredChannelDecisionState"] == "USER_DECISION_REQUIRED" &&
                    row["applicabilityCandidateA"] ==
                        "AUTHOR_EXPLICIT_APPLICABILITY" &&
                    row["applicabilityCandidateB"] == "KEEP_DEFERRED" &&
                    row["applicabilityDecisionState"] == "USER_DECISION_REQUIRED" &&
                    row["pressureInputIdCandidateA"] == "<null>" &&
                    row["pressureInputIdCandidateB"] == "<null>" &&
                    row["pressureInputIdDecisionState"] == "USER_AUTHORING_REQUIRED" &&
                    row["requiredPredicateClausesCandidateA"] == "<null>" &&
                    row["requiredPredicateClausesCandidateB"] == "<null>" &&
                    row["requiredPredicateClausesDecisionState"] ==
                        "USER_AUTHORING_REQUIRED" &&
                    row["candidateActivated"] == "false" &&
                    row["behaviorChange"] == "NONE");
            }
        }

        private static void VerifyCollisions(
            ICollection<Check> checks,
            IList<Row> rows)
        {
            Add(checks, "collision.rows", "9", rows.Count.ToString(
                CultureInfo.InvariantCulture), rows.Count == 9);
            Add(checks, "collision.membership-kinds", "4/5",
                rows.Count(row => row["membershipKind"] == "PLACEMENT_CANDIDATE") +
                    "/" + rows.Count(row =>
                        row["membershipKind"] == "SIBLING_NON_PLACEMENT"),
                rows.Count(row => row["membershipKind"] == "PLACEMENT_CANDIDATE") == 4 &&
                rows.Count(row => row["membershipKind"] ==
                    "SIBLING_NON_PLACEMENT") == 5);
            Add(checks, "collision.unique-rows", "9",
                rows.Select(CollisionIdentity).Distinct(StringComparer.Ordinal).Count()
                    .ToString(CultureInfo.InvariantCulture),
                rows.Select(CollisionIdentity).Distinct(StringComparer.Ordinal).Count() == 9);
            Dictionary<string, int> groupCounts = rows.GroupBy(row =>
                    row["requirementGroupId"], StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.Count(),
                    StringComparer.Ordinal);
            Add(checks, "collision.group-counts", "3/2/2/2",
                string.Join("|", groupCounts.OrderBy(value => value.Key,
                    StringComparer.Ordinal).Select(value => value.Value)),
                groupCounts.Count == 4 && groupCounts.Values.OrderBy(value => value)
                    .SequenceEqual(new[] { 2, 2, 2, 3 }));
            Add(checks, "collision.group-only-rejected", "all",
                rows.Count(row => row["groupOnlyIdentityUnique"] == "false" &&
                    row["groupOnlyBindingDisposition"] == "REJECTED").ToString(
                        CultureInfo.InvariantCulture),
                rows.All(row => row["groupOnlyIdentityUnique"] == "false" &&
                    row["groupOnlyBindingDisposition"] == "REJECTED"));
            string actualSets = string.Join(";", rows.GroupBy(row =>
                    row["requirementGroupId"], StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => group.Key + "=" + string.Join("|", group.Select(row =>
                    row["buildCapabilityKey"]).OrderBy(value => value,
                        StringComparer.Ordinal))));
            const string expectedSets =
                "dev_boss_bronze_formation_general.required=capability.energy_stability|capability.guard_power|capability.placement_shape;" +
                "dev_enemy_formation_eye_problem.required=capability.energy_stability|capability.placement_shape;" +
                "dev_enemy_polluted_tile_problem.required=capability.cleanse_power|capability.placement_shape;" +
                "dev_enemy_spirit_thief_problem.recommended=capability.control_power|capability.placement_shape";
            Add(checks, "collision.capability-sets", expectedSets, actualSets,
                actualSets == expectedSets);
            Add(checks, "collision.role-mode", "Required/All and Recommended/Any",
                string.Join("|", rows.Select(row => row["role"] + "/" +
                    row["matchMode"]).Distinct(StringComparer.Ordinal).OrderBy(value =>
                        value, StringComparer.Ordinal)),
                rows.All(row => (row["role"] == "Required" &&
                    row["matchMode"] == "All") ||
                    (row["role"] == "Recommended" && row["matchMode"] == "Any")));
            Add(checks, "collision.proof", "4", groupCounts.Count.ToString(
                CultureInfo.InvariantCulture),
                groupCounts.Count == 4 && groupCounts.All(pair => pair.Value > 1));
        }

        private static void VerifyGaps(
            ICollection<Check> checks,
            IList<Row> rows)
        {
            string[] fields =
            {
                "DeclaredBoardSize",
                "EffectiveEyeAnchorCellAfterPressure",
                "LayoutDomainCells",
                "PreservedStructuralConnectionsAfterPressure",
                "PressureInputId",
                "PressureKinds",
                "RequiredPredicateClauses",
                "UsableCellsAfterPressure"
            };
            string[] ids = ExpectedCandidates.Select(value => value.Id).OrderBy(
                value => value, StringComparer.Ordinal).ToArray();
            Add(checks, "gap.rows", "32", rows.Count.ToString(
                CultureInfo.InvariantCulture), rows.Count == 32);
            Add(checks, "gap.unique", "32",
                rows.Select(row => row["candidateMigrationRequirementId"] + "|" +
                    row["fieldName"]).Distinct(StringComparer.Ordinal).Count().ToString(
                        CultureInfo.InvariantCulture),
                rows.Select(row => row["candidateMigrationRequirementId"] + "|" +
                    row["fieldName"]).Distinct(StringComparer.Ordinal).Count() == 32);
            Add(checks, "gap.fields", string.Join("|", fields),
                string.Join("|", rows.Select(row => row["fieldName"]).Distinct(
                    StringComparer.Ordinal).OrderBy(value => value,
                        StringComparer.Ordinal)),
                fields.SequenceEqual(rows.Select(row => row["fieldName"]).Distinct(
                    StringComparer.Ordinal).OrderBy(value => value,
                        StringComparer.Ordinal)));
            Add(checks, "gap.candidates", string.Join("|", ids),
                string.Join("|", rows.Select(row =>
                    row["candidateMigrationRequirementId"]).Distinct(
                        StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal)),
                ids.SequenceEqual(rows.Select(row =>
                    row["candidateMigrationRequirementId"]).Distinct(
                        StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal)));
            Add(checks, "gap.eight-each", "8/8/8/8",
                string.Join("|", rows.GroupBy(row =>
                    row["candidateMigrationRequirementId"], StringComparer.Ordinal)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .Select(group => group.Count())),
                rows.GroupBy(row => row["candidateMigrationRequirementId"],
                    StringComparer.Ordinal).All(group => group.Count() == 8));
            Add(checks, "gap.state", "MISSING", string.Join("|", rows.Select(row =>
                row["evidenceState"]).Distinct(StringComparer.Ordinal)),
                rows.All(row => row["evidenceState"] == "MISSING"));
            Add(checks, "gap.required-status",
                "USER_AUTHORING_REQUIRED / INCOMPLETE",
                string.Join("|", rows.Select(row => row["requiredStatus"]).Distinct(
                    StringComparer.Ordinal)),
                rows.All(row => row["requiredStatus"] ==
                    "USER_AUTHORING_REQUIRED / INCOMPLETE"));
            Add(checks, "gap.null-cardinality", "<null>/UNRESOLVED",
                rows.Count(row => row["approvedValue"] == "<null>" &&
                    row["contextCardinality"] == "UNRESOLVED").ToString(
                        CultureInfo.InvariantCulture),
                rows.All(row => row["approvedValue"] == "<null>" &&
                    row["contextCardinality"] == "UNRESOLVED"));
            Add(checks, "gap.no-applicability-inference", "REJECTED",
                string.Join("|", rows.Select(row =>
                    row["notApplicableInference"]).Distinct(StringComparer.Ordinal)),
                rows.All(row => row["notApplicableInference"] == "REJECTED") &&
                rows.All(row => !Has(row.Raw, "NotApplicable") &&
                    !Has(row.Raw, "NotInChannel")));
            Add(checks, "gap.evidence", "32 nonempty", rows.Count(row =>
                !string.IsNullOrWhiteSpace(row["sourceEvidence"])).ToString(
                    CultureInfo.InvariantCulture),
                rows.All(row => !string.IsNullOrWhiteSpace(row["sourceEvidence"])));
        }

        private static void VerifyContexts(
            ICollection<Check> checks,
            IList<Row> rows)
        {
            Add(checks, "context.rows", "10", rows.Count.ToString(
                CultureInfo.InvariantCulture), rows.Count == 10);
            Add(checks, "context.active", "7", rows.Count(row =>
                row["scope"] == "ACTIVE_E10").ToString(CultureInfo.InvariantCulture),
                rows.Count(row => row["scope"] == "ACTIVE_E10") == 7);
            Add(checks, "context.broader", "3", rows.Count(row =>
                row["scope"] == "BROADER_E03_ONLY").ToString(
                    CultureInfo.InvariantCulture),
                rows.Count(row => row["scope"] == "BROADER_E03_ONLY") == 3);
            Add(checks, "context.unique", "10", rows.Select(ContextIdentity)
                .Distinct(StringComparer.Ordinal).Count().ToString(
                    CultureInfo.InvariantCulture),
                rows.Select(ContextIdentity).Distinct(StringComparer.Ordinal).Count() == 10);
            Add(checks, "context.status", "all not activated", rows.Count(row =>
                row["pressureAuthoringTargetStatus"] ==
                    "NOT_ACTIVATED / USER_DECISION_REQUIRED").ToString(
                        CultureInfo.InvariantCulture),
                rows.All(row => row["pressureAuthoringTargetStatus"] ==
                    "NOT_ACTIVATED / USER_DECISION_REQUIRED"));
            string active = string.Join(";", rows.Where(row =>
                    row["scope"] == "ACTIVE_E10").Select(ContextIdentity)
                .OrderBy(value => value, StringComparer.Ordinal));
            string expectedActive = string.Join(";", new[]
            {
                "dev_seed_3_10_cleanse_corner|dev_encounter_3_10_cleanse_corner|dev_map_bluestone_damp|dev_enemy_polluted_tile_problem",
                "dev_seed_3_10_guard_wall|dev_encounter_3_10_guard_wall|dev_map_copper_bell_night|dev_boss_bronze_formation_general",
                "dev_seed_4_10_furnace_core|dev_encounter_4_10_furnace_core|dev_map_furnace_ash_fall|dev_enemy_formation_eye_problem",
                "dev_seed_4_10_furnace_core|dev_encounter_4_10_furnace_core|dev_map_furnace_ash_fall|dev_enemy_polluted_tile_problem",
                "dev_seed_4_10_furnace_core|dev_encounter_4_10_furnace_core|dev_map_furnace_ash_fall|dev_enemy_spirit_thief_problem",
                "dev_seed_4_10_thunder_fire_cross|dev_encounter_4_10_thunder_fire_cross|dev_map_bluestone_crack|dev_enemy_formation_eye_problem",
                "dev_seed_4_10_thunder_fire_cross|dev_encounter_4_10_thunder_fire_cross|dev_map_bluestone_crack|dev_enemy_spirit_thief_problem"
            }.OrderBy(value => value, StringComparer.Ordinal));
            Add(checks, "context.active-set", expectedActive, active,
                active == expectedActive);
            string broader = string.Join("|", rows.Where(row =>
                    row["scope"] == "BROADER_E03_ONLY").Select(row => row["ownerId"])
                .OrderBy(value => value, StringComparer.Ordinal));
            const string expectedBroader =
                "dev_boss_bronze_formation_general|dev_enemy_formation_eye_problem|dev_enemy_spirit_thief_problem";
            Add(checks, "context.broader-set", expectedBroader, broader,
                broader == expectedBroader && rows.Where(row =>
                    row["scope"] == "BROADER_E03_ONLY").All(row =>
                        row["mapRuleId"] == "dev_map_formation_eye_shift"));
            Add(checks, "context.binding-kinds",
                "EncounterSlot.MechanicProfileIds + Seed.PrimaryMapRuleId/MapRuleMechanic",
                string.Join("|", rows.Select(row => row["sourceBindingKind"]).Distinct(
                    StringComparer.Ordinal).OrderBy(value => value,
                        StringComparer.Ordinal)),
                rows.Where(row => row["scope"] == "ACTIVE_E10").All(row =>
                    row["sourceBindingKind"] ==
                        "EncounterSlot.MechanicProfileIds + Seed.PrimaryMapRuleId") &&
                rows.Where(row => row["scope"] == "BROADER_E03_ONLY").All(row =>
                    row["sourceBindingKind"] == "MapRuleMechanic"));
            Add(checks, "context.broader-null", "<null>", rows.Count(row =>
                row["scope"] == "BROADER_E03_ONLY" &&
                row["seedId"] == "<null>" && row["encounterId"] == "<null>")
                    .ToString(CultureInfo.InvariantCulture),
                rows.Where(row => row["scope"] == "BROADER_E03_ONLY").All(row =>
                    row["seedId"] == "<null>" && row["encounterId"] == "<null>"));
        }

        private static void VerifyDecisions(
            ICollection<Check> checks,
            IList<Row> rows)
        {
            Add(checks, "decision.rows", "12", rows.Count.ToString(
                CultureInfo.InvariantCulture), rows.Count == 12);
            Add(checks, "decision.ids", "D1-D9",
                string.Join("|", rows.Select(row => row["decisionId"]).Distinct(
                    StringComparer.Ordinal).OrderBy(value => value,
                        StringComparer.Ordinal)),
                Enumerable.Range(1, 9).Select(value => "D" + value.ToString(
                    CultureInfo.InvariantCulture)).SequenceEqual(rows.Select(row =>
                        row["decisionId"]).Distinct(StringComparer.Ordinal).OrderBy(
                            value => value, StringComparer.Ordinal)));
            Add(checks, "decision.d3", "4", rows.Count(row =>
                row["decisionId"] == "D3").ToString(CultureInfo.InvariantCulture),
                rows.Count(row => row["decisionId"] == "D3") == 4 &&
                rows.Where(row => row["decisionId"] == "D3").Select(row =>
                    row["subjectId"]).Distinct(StringComparer.Ordinal).Count() == 4);
            Add(checks, "decision.status", "USER_DECISION_REQUIRED", rows.Count(row =>
                row["decisionStatus"] == "USER_DECISION_REQUIRED").ToString(
                    CultureInfo.InvariantCulture),
                rows.All(row => row["decisionStatus"] == "USER_DECISION_REQUIRED"));
            Add(checks, "decision.inactive", "false", rows.Count(row =>
                row["activated"] == "false").ToString(CultureInfo.InvariantCulture),
                rows.All(row => row["activated"] == "false"));
            Add(checks, "decision.two-options", "two nonempty", rows.Count(row =>
                !string.IsNullOrWhiteSpace(row["candidateA"]) &&
                !string.IsNullOrWhiteSpace(row["candidateB"])).ToString(
                    CultureInfo.InvariantCulture),
                rows.All(row => !string.IsNullOrWhiteSpace(row["candidateA"]) &&
                    !string.IsNullOrWhiteSpace(row["candidateB"])));
            Add(checks, "decision.d1", "selected/deferred", RowHas(rows, "D1",
                "MIGRATE_SELECTED_CANDIDATES", "KEEP_ALL_DEFERRED").ToString(),
                RowHas(rows, "D1", "MIGRATE_SELECTED_CANDIDATES",
                    "KEEP_ALL_DEFERRED"));
            Add(checks, "decision.d2", "exact identity", RowHas(rows, "D2",
                "APPROVE_EXACT_IDS_AND_TUPLES", "REAUTHOR_IDS_OR_TUPLES").ToString(),
                RowHas(rows, "D2", "APPROVE_EXACT_IDS_AND_TUPLES",
                    "REAUTHOR_IDS_OR_TUPLES"));
            Add(checks, "decision.d4", "requirement/context", RowHas(rows, "D4",
                "AUTHOR_PER_REQUIREMENT", "AUTHOR_PER_CONTEXT").ToString(),
                RowHas(rows, "D4", "AUTHOR_PER_REQUIREMENT", "AUTHOR_PER_CONTEXT"));
            Add(checks, "decision.d5", "complete/incomplete", RowHas(rows, "D5",
                "AUTHOR_COMPLETE_EIGHT_FIELDS_PER_SELECTED_CONTEXT",
                "KEEP_AUTHORING_INCOMPLETE").ToString(),
                RowHas(rows, "D5",
                    "AUTHOR_COMPLETE_EIGHT_FIELDS_PER_SELECTED_CONTEXT",
                    "KEEP_AUTHORING_INCOMPLETE"));
            Add(checks, "decision.d6", "clauses/defer", RowHas(rows, "D6",
                "SELECT_FROM_FIVE_CLAUSES", "KEEP_PREDICATE_UNRESOLVED").ToString(),
                RowHas(rows, "D6", "SELECT_FROM_FIVE_CLAUSES",
                    "KEEP_PREDICATE_UNRESOLVED"));
            Add(checks, "decision.d7-d9", "quarantine/readiness/sequence",
                string.Join("|", rows.Where(row => row["decisionId"] == "D7" ||
                    row["decisionId"] == "D8" || row["decisionId"] == "D9")
                    .Select(row => row.Raw)), rows.Any(row =>
                    row["decisionId"] == "D7" && Has(row.Raw, "QUARANTINED")) &&
                rows.Any(row => row["decisionId"] == "D8" &&
                    Has(row.Raw, "TOTAL_READINESS")) &&
                rows.Any(row => row["decisionId"] == "D9" &&
                    Has(row.Raw, "STAGED_AUTHORING")));
        }

        private static void VerifyCanonical(
            ICollection<Check> checks,
            string root,
            IList<CanonicalSource> sources,
            string canonical)
        {
            Add(checks, "canonical.format", "sha256 + 64 lowercase", canonical,
                IsSignature(canonical));
            Add(checks, "canonical.fixed", ExpectedSignature, canonical,
                canonical == ExpectedSignature);
            Add(checks, "canonical.regeneration", canonical,
                CanonicalSignature(LoadCanonicalSources(root), FullE10Signature,
                    PlayerSafeE10Signature),
                canonical == CanonicalSignature(LoadCanonicalSources(root),
                    FullE10Signature, PlayerSafeE10Signature));
            Add(checks, "canonical.reverse", canonical,
                CanonicalSignature(Reverse(sources), FullE10Signature,
                    PlayerSafeE10Signature),
                canonical == CanonicalSignature(Reverse(sources), FullE10Signature,
                    PlayerSafeE10Signature));
            CultureInfo prior = CultureInfo.CurrentCulture;
            string cultureSignature;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                cultureSignature = CanonicalSignature(sources, FullE10Signature,
                    PlayerSafeE10Signature);
            }
            finally
            {
                CultureInfo.CurrentCulture = prior;
            }
            Add(checks, "canonical.culture", canonical, cultureSignature,
                canonical == cultureSignature);
            AddMutation(checks, "tuple", sources,
                "dev_boss_bronze_formation_general.required",
                "dev_boss_bronze_formation_general.Required", canonical);
            AddMutation(checks, "bp", sources, ",4631,", ",4632,", canonical);
            AddMutation(checks, "sibling", sources,
                "capability.guard_power,4513", "capability.guard_power,4514",
                canonical);
            AddMutation(checks, "candidate-id", sources,
                "layout_resilience.formation_eye.placement_shape",
                "layout_resilience.formation_eye.placement_shape.changed", canonical);
            AddMutation(checks, "gap", sources,
                "No authored layout domain exists.",
                "No authored layout domain exists. changed", canonical);
            AddMutation(checks, "decision", sources,
                "KEEP_ALL_DEFERRED", "KEEP_SELECTED_DEFERRED", canonical);
            string provenanceMutation = CanonicalSignature(sources,
                FullE10Signature.Replace("377f", "477f"), PlayerSafeE10Signature);
            Add(checks, "canonical.mutation.provenance", "changed",
                provenanceMutation, provenanceMutation != canonical);
        }

        private static void VerifyProtected(
            ICollection<Check> checks,
            string root,
            string before)
        {
            AddProtection(checks, root, "e10", E10Files(root), true, 21,
                "3ee938a175bb879d663f6da2847278caaad99d5c636179c0aa93dce79bab96be");
            AddFileProtection(checks, root, "e10-catalog",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/SeedData/DevEncounterSeedDataCatalog.cs",
                "9f08a734b82af27f9b6777946f431fd7247466017b95abccef6da4116424eaa0");
            AddFileProtection(checks, root, "e06",
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/PressureWindow/BuildPressureSnapshots.cs",
                "56cc65badaf89be6882629ae847fcf03c2d0bac3d800bdb562de66e275e4d1c1");
            AddFileProtection(checks, root, "e10-pressure",
                ReportDirectory + "/DevEncounterSeedPressureWindowRows.csv",
                "9e543c04022bf7c4a3e646511cf67bb20d10f620f5bfb0c551c848baad9ee50c");
            AddFileProtection(checks, root, "e10-composition",
                ReportDirectory + "/DevEncounterSeedCompositionRows.csv",
                "7feaad7c4bef64e2ea3dcb3bd51dfbd78690a80337d576cd5f018fab2960472e");
            AddFileProtection(checks, root, "e03-inventory",
                ReportDirectory + "/EnemyValidationMechanicInventory.csv",
                "f993828a64b112a8ac2c640d592f50ee139bb374821550520932f7e6637b04f6");
            AddProtection(checks, root, "p4", P4Files(root), true, 14,
                "8e660348e36341b29bde9ff775baef0a72f42ebb7f52bea7fa3544d616700530");
            AddProtection(checks, root, "p3", P3Files(root), true, 14,
                "c9bc9c02f1887216a2011892d298c99ef85aba86ac5f9e178a6d46533705d96c");
            AddProtection(checks, root, "p2", P2Files(root), true, 14,
                "15af10d550413c1803b2e5e655d3f2065415fc438fbbb13aac2efe66b1a6e050");
            AddProtection(checks, root, "p1", P1Files(root), true, 14,
                "7d44549d8055bedc8908fdf337ec39235199c7956af4de6c150f968c90e2dc48");
            AddProtection(checks, root, "n01c", N01CFiles(root), true, 16,
                "691ab876aa4d150defb1c662732490940e6f51384d767ae3d941ff25ed23382b");
            AddProtection(checks, root, "n01b", N01BFiles(root), true, 14,
                "acbe35a05464c320d71ff79561724f9f15032936e38a2a1e71f3c7966edcf316");
            AddAggregateProtection(checks, "item", AggregateItem(root), 105,
                "2c3f755292183a5000c3d79540b91d12e8254743d3478145b60e841e57c569b1");
            AddProtection(checks, root, "if01", IF01Files(root), true, 11,
                "2c35b366d61c5048afb620ff976c6db2ae09dca65e4fad9a7362366271c4eeb8");
            AddAggregateProtection(checks, "enemy", AggregateEnemy(root), 89,
                "7660b90d0d207b8c19c6cf488030bba509d9f8123638197c8f89d57e9f5bbfae");
            AddProtection(checks, root, "c01", C01Files(root), false, 9,
                "fbc9c37d07eec652abc3005d6c9323cb65b13d0941f4f234b48eaa945865700a");
            AddProtection(checks, root, "c02", C02Files(root), false, 9,
                "08af276af4dc1924240f49408eef08e5a6647d288295f686da3a6fff35fe98e4");
            AddProtection(checks, root, "c02a", C02AFiles(root), false, 7,
                "a4a61fc2f8a41f14a2a071d1dec00bc5da783a034f9671e6efae63a9425d53fb");
            AddProtection(checks, root, "n01", N01Files(root), true, 8,
                "03d266c7d067a6ed7d345c9fdd05dc50895d87cf1ec5fe4827fff71eef12fc52");
            AddProtection(checks, root, "n01a", N01AFiles(root), true, 8,
                "705cace17fd7377e13deedecd4af0a7cd6d154eaf78d20c598927ff7f60626de");
            AddProtection(checks, root, "scenes",
                DirectoryFiles(root, "Assets/_Game/Scenes"), true, 14,
                "da29c51ff7cc5a814caa83a22d45b1ba3b1c54c826334d914215407611ae6d0b");
            AddProtection(checks, root, "prefabs",
                DirectoryFiles(root, "Assets/_Game/Prefabs"), true, 16,
                "7fe361444bc9568f0599076d65aa707172d02da55d56a5be1348bf186b237087");
            AddFileProtection(checks, root, "build-settings",
                "ProjectSettings/EditorBuildSettings.asset",
                "08a277e3ca465a44e792318c0d3c210afdba61069f1170b74fa5a1a18598fe59");
            string head = RunGit(root, "rev-parse HEAD").Trim();
            Add(checks, "protected.head",
                "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba", head,
                head == "f80fbd8ffb8e2d75b0032058a0d09b484e6f63ba");
            string after = CaptureProtectedState(root);
            Add(checks, "protected.before-after", before, after, before == after);
            AddFileProtection(checks, root, "assignment",
                "Docs/V0.4/LayoutResilienceRequirementMigrationSurvey01_Assignment.md",
                "5f9c57208e83b6d8cecf2c18b796a7889af1a4589bdb688c95f6a4bc769d3ae3");
        }

        private static bool ExactCandidate(Row row, CandidateExpectation expected)
        {
            return row["candidateOrdinal"] == expected.Ordinal &&
                row["ownerId"] == expected.Owner &&
                row["requirementGroupId"] == expected.Group &&
                row["role"] == expected.Role &&
                row["matchMode"] == expected.Mode &&
                row["referenceKind"] == "BuildCapability" &&
                row["buildCapabilityKey"] == expected.Key &&
                row["expectedLegacyMinimumCapabilityBasisPoints"] == expected.Bp;
        }

        private static bool False(Row row, params string[] fields)
        {
            return fields.All(field => row[field] == "false");
        }

        private static bool RowHas(
            IEnumerable<Row> rows,
            string id,
            string optionA,
            string optionB)
        {
            return rows.Any(row => row["decisionId"] == id &&
                row["candidateA"] == optionA && row["candidateB"] == optionB);
        }

        private static string CollisionIdentity(Row row)
        {
            return row["ownerId"] + "|" + row["requirementGroupId"] + "|" +
                row["buildCapabilityKey"];
        }

        private static string ContextIdentity(Row row)
        {
            return row["seedId"] + "|" + row["encounterId"] + "|" +
                row["mapRuleId"] + "|" + row["ownerId"];
        }

        private static List<CanonicalSource> LoadCanonicalSources(string root)
        {
            return new[]
            {
                CandidatePath, CollisionPath, GapPath, ContextPath, DecisionPath
            }.Select(path => new CanonicalSource(Path.GetFileName(path),
                File.ReadAllLines(Absolute(root, path), Encoding.UTF8).ToList()))
                .ToList();
        }

        private static string CanonicalSignature(
            IEnumerable<CanonicalSource> input,
            string fullSignature,
            string playerSafeSignature)
        {
            List<string> rows = new List<string>();
            foreach (CanonicalSource source in input)
            {
                if (source.Lines.Count < 1)
                {
                    continue;
                }
                string[] headers = ParseCsv(source.Lines[0]).ToArray();
                for (int index = 1; index < source.Lines.Count; index++)
                {
                    if (string.IsNullOrWhiteSpace(source.Lines[index]))
                    {
                        continue;
                    }
                    List<string> values = ParseCsv(source.Lines[index]);
                    Dictionary<string, string> fields = new Dictionary<string, string>(
                        StringComparer.Ordinal);
                    for (int field = 0; field < headers.Length; field++)
                    {
                        fields[headers[field]] = field < values.Count ?
                            values[field] : string.Empty;
                    }
                    string key;
                    if (source.Name.EndsWith("CandidateRows.csv",
                        StringComparison.Ordinal))
                    {
                        key = fields["ownerId"] + "|" + fields["requirementGroupId"] +
                            "|" + fields["buildCapabilityKey"];
                    }
                    else if (source.Name.EndsWith("IdentityCollisionMatrix.csv",
                        StringComparison.Ordinal))
                    {
                        key = fields["ownerId"] + "|" + fields["requirementGroupId"] +
                            "|" + fields["buildCapabilityKey"];
                    }
                    else if (source.Name.EndsWith("PressureFactGap.csv",
                        StringComparison.Ordinal))
                    {
                        key = fields["candidateMigrationRequirementId"] + "|" +
                            fields["fieldName"];
                    }
                    else if (source.Name.EndsWith("ContextMatrix.csv",
                        StringComparison.Ordinal))
                    {
                        key = fields["seedId"] + "|" + fields["encounterId"] + "|" +
                            fields["mapRuleId"] + "|" + fields["ownerId"];
                    }
                    else
                    {
                        key = fields["decisionId"] + "|" + fields["subjectId"];
                    }
                    string body = string.Join("|", headers.Select(header =>
                        header + "=" + fields[header]));
                    rows.Add(source.Name + "|" + key + "|" + body);
                }
            }
            rows.Sort(StringComparer.Ordinal);
            string[] prefix =
            {
                "schemaId=LayoutResilienceRequirementMigrationSurvey.v1",
                "schemaVersion=1",
                "e10Full=" + fullSignature,
                "e10PlayerSafe=" + playerSafeSignature,
                "actualUnknownReduction=0",
                "c02Blocked=32/32",
                "behaviorChanges=0",
                "e10E08Modifications=0",
                "p3Calls=0",
                "p4Calls=0",
                "evaluatorCalls=0",
                "realMigrationRows=0",
                "realReadinessRows=0"
            };
            return "sha256:" + Sha256(Encoding.UTF8.GetBytes(
                string.Join("\n", prefix.Concat(rows))), false);
        }

        private static List<CanonicalSource> Reverse(
            IEnumerable<CanonicalSource> sources)
        {
            List<CanonicalSource> reversed = new List<CanonicalSource>();
            foreach (CanonicalSource source in sources.Reverse())
            {
                List<string> lines = new List<string> { source.Lines[0] };
                lines.AddRange(source.Lines.Skip(1).Reverse());
                reversed.Add(new CanonicalSource(source.Name, lines));
            }
            return reversed;
        }

        private static void AddMutation(
            ICollection<Check> checks,
            string id,
            IEnumerable<CanonicalSource> sources,
            string oldValue,
            string newValue,
            string original)
        {
            bool replaced = false;
            List<CanonicalSource> clone = sources.Select(source =>
                new CanonicalSource(source.Name, source.Lines.Select(line =>
                {
                    if (!replaced && line.IndexOf(oldValue,
                        StringComparison.Ordinal) >= 0)
                    {
                        replaced = true;
                        return ReplaceFirst(line, oldValue, newValue);
                    }
                    return line;
                }).ToList())).ToList();
            string mutated = CanonicalSignature(clone, FullE10Signature,
                PlayerSafeE10Signature);
            Add(checks, "canonical.mutation." + id, "changed", mutated,
                replaced && mutated != original);
        }

        private static string ReplaceFirst(
            string value,
            string oldValue,
            string newValue)
        {
            int index = value.IndexOf(oldValue, StringComparison.Ordinal);
            return index < 0 ? value : value.Substring(0, index) + newValue +
                value.Substring(index + oldValue.Length);
        }

        private static List<Row> LoadRows(string root, string path)
        {
            string[] lines = File.ReadAllLines(Absolute(root, path), Encoding.UTF8);
            if (lines.Length == 0)
            {
                return new List<Row>();
            }
            string[] headers = ParseCsv(lines[0]).ToArray();
            List<Row> rows = new List<Row>();
            for (int index = 1; index < lines.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(lines[index]))
                {
                    continue;
                }
                List<string> values = ParseCsv(lines[index]);
                Dictionary<string, string> fields = new Dictionary<string, string>(
                    StringComparer.Ordinal);
                for (int field = 0; field < headers.Length; field++)
                {
                    fields[headers[field]] = field < values.Count ?
                        values[field] : string.Empty;
                }
                rows.Add(new Row(lines[index], fields));
            }
            return rows;
        }

        private static List<string> ParseCsv(string line)
        {
            List<string> values = new List<string>();
            StringBuilder value = new StringBuilder();
            bool quoted = false;
            for (int index = 0; index < line.Length; index++)
            {
                char current = line[index];
                if (current == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        value.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (current == ',' && !quoted)
                {
                    values.Add(value.ToString());
                    value.Length = 0;
                }
                else
                {
                    value.Append(current);
                }
            }
            values.Add(value.ToString());
            return values;
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

        private static IEnumerable<string> P4Files(string root)
        {
            return ContractFiles(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/StructuralReadinessConsumer",
                null,
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralReadinessConsumerVerifier.cs",
                "LayoutResilienceStructuralReadinessConsumer");
        }

        private static IEnumerable<string> P3Files(string root)
        {
            return ContractFiles(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/EvaluationInputAssembly",
                null,
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceEvaluationInputAssemblerVerifier.cs",
                "LayoutResilienceEvaluationInputAssembler");
        }

        private static IEnumerable<string> P2Files(string root)
        {
            return ContractFiles(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure",
                null,
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/AuthoredLayoutPressureSourceAdapterVerifier.cs",
                "AuthoredLayoutPressureSourceAdapter");
        }

        private static IEnumerable<string> P1Files(string root)
        {
            return ContractFiles(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/ItemFactProjection",
                null,
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceItemFactProjectionAdapterVerifier.cs",
                "LayoutResilienceItemFactProjectionAdapter");
        }

        private static IEnumerable<string> N01CFiles(string root)
        {
            return ContractFiles(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience",
                "LayoutResilienceStructuralPredicate",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceStructuralPredicateContractVerifier.cs",
                "LayoutResilienceStructuralPredicateContract");
        }

        private static IEnumerable<string> N01BFiles(string root)
        {
            return ContractFiles(root,
                "Assets/_Game/Scripts/TalismanBag/EnemySystem/RequirementChannel",
                null,
                "Assets/_Game/Scripts/TalismanBag/Editor/EnemySystem/EnemyRequirementChannelApplicabilitySchemaContractVerifier.cs",
                "EnemyRequirementChannelApplicabilitySchemaContract");
        }

        private static IEnumerable<string> IF01Files(string root)
        {
            List<string> values = ContractFiles(root,
                "Assets/_Game/Scripts/TalismanBag/Items/Capability",
                "ItemInstancePlacementBinding",
                "Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability/ItemInstancePlacementBindingContractVerifier.cs",
                "ItemInstancePlacementBindingContract").ToList();
            values.Add("Assets/_Game/Scripts/TalismanBag/Editor/ItemCapability.meta");
            return values;
        }

        private static IEnumerable<string> C01Files(string root)
        {
            return FlatFiles(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapter.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemBuildCapabilityProjectionAdapterVerifier.cs",
                "ItemBuildCapabilityProjection");
        }

        private static IEnumerable<string> C02Files(string root)
        {
            return FlatFiles(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulation.cs",
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemEnemyMatchupSimulationVerifier.cs",
                "ItemEnemyMatchup");
        }

        private static IEnumerable<string> C02AFiles(string root)
        {
            return FlatFiles(root, null,
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/ItemCapabilityMappingGapSurveyVerifier.cs",
                "ItemCapabilityMapping");
        }

        private static IEnumerable<string> N01Files(string root)
        {
            return FlatFiles(root, null,
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityNormalizationRuleSurveyVerifier.cs",
                "BuildCapabilityNormalization");
        }

        private static IEnumerable<string> N01AFiles(string root)
        {
            return FlatFiles(root, null,
                "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/BuildCapabilityRemainingBlockerSemanticSurveyVerifier.cs",
                "BuildCapabilityRemainingBlocker");
        }

        private static IEnumerable<string> ContractFiles(
            string root,
            string runtimeDirectory,
            string runtimePrefix,
            string verifier,
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

        private static IEnumerable<string> FlatFiles(
            string root,
            string runtime,
            string verifier,
            string reportPrefix)
        {
            List<string> values = new List<string>();
            if (runtime != null)
            {
                values.Add(runtime);
                values.Add(runtime + ".meta");
            }
            values.Add(verifier);
            values.Add(verifier + ".meta");
            values.AddRange(ReportFiles(root, reportPrefix));
            return values;
        }

        private static IEnumerable<string> ReportFiles(string root, string prefix)
        {
            return Directory.GetFiles(Absolute(root, ReportDirectory), prefix + "*",
                    SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path));
        }

        private static IEnumerable<string> DirectoryFiles(
            string root,
            string directory,
            bool recursive = true)
        {
            return Directory.GetFiles(Absolute(root, directory), "*",
                    recursive ? SearchOption.AllDirectories :
                        SearchOption.TopDirectoryOnly)
                .Select(path => Relative(root, path));
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

        private static AggregateHash AggregateFiles(
            string root,
            IEnumerable<string> paths,
            bool trailingLf)
        {
            string[] ordered = paths.Distinct(StringComparer.Ordinal).OrderBy(path =>
                path, trailingLf ? StringComparer.Ordinal :
                    StringComparer.OrdinalIgnoreCase).ToArray();
            StringBuilder payload = new StringBuilder();
            for (int index = 0; index < ordered.Length; index++)
            {
                payload.Append(ordered[index].Replace('\\', '/')).Append('|')
                    .Append(Sha256(File.ReadAllBytes(Absolute(root, ordered[index])),
                        !trailingLf));
                if (trailingLf || index + 1 < ordered.Length)
                {
                    payload.Append('\n');
                }
            }
            return new AggregateHash(ordered.Length, Sha256(
                Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static void AddProtection(
            ICollection<Check> checks,
            string root,
            string id,
            IEnumerable<string> files,
            bool trailingLf,
            int expectedCount,
            string expectedHash)
        {
            AggregateHash actual = AggregateFiles(root, files, trailingLf);
            AddAggregateProtection(checks, id, actual, expectedCount, expectedHash);
        }

        private static void AddAggregateProtection(
            ICollection<Check> checks,
            string id,
            AggregateHash actual,
            int expectedCount,
            string expectedHash)
        {
            Add(checks, "protected." + id, expectedCount + "|" + expectedHash,
                actual.FileCount.ToString(CultureInfo.InvariantCulture) + "|" +
                    actual.Hash,
                actual.FileCount == expectedCount && actual.Hash == expectedHash);
        }

        private static void AddFileProtection(
            ICollection<Check> checks,
            string root,
            string id,
            string path,
            string expected)
        {
            string actual = Sha256(File.ReadAllBytes(Absolute(root, path)), false);
            Add(checks, "protected." + id, expected, actual, actual == expected);
        }

        private static string CaptureProtectedState(string root)
        {
            List<string> state = new List<string>
            {
                AggregateFiles(root, E10Files(root), true).Hash,
                AggregateFiles(root, P4Files(root), true).Hash,
                AggregateFiles(root, P3Files(root), true).Hash,
                AggregateFiles(root, P2Files(root), true).Hash,
                AggregateFiles(root, P1Files(root), true).Hash,
                AggregateFiles(root, N01CFiles(root), true).Hash,
                AggregateFiles(root, N01BFiles(root), true).Hash,
                AggregateItem(root).Hash,
                AggregateFiles(root, IF01Files(root), true).Hash,
                AggregateEnemy(root).Hash,
                AggregateFiles(root, C01Files(root), false).Hash,
                AggregateFiles(root, C02Files(root), false).Hash,
                AggregateFiles(root, C02AFiles(root), false).Hash,
                AggregateFiles(root, N01Files(root), true).Hash,
                AggregateFiles(root, N01AFiles(root), true).Hash,
                AggregateFiles(root, DirectoryFiles(root, "Assets/_Game/Scenes"),
                    true).Hash,
                AggregateFiles(root, DirectoryFiles(root, "Assets/_Game/Prefabs"),
                    true).Hash,
                Sha256(File.ReadAllBytes(Absolute(root,
                    "ProjectSettings/EditorBuildSettings.asset")), false),
                RunGit(root, "rev-parse HEAD").Trim()
            };
            return Sha256(Encoding.UTF8.GetBytes(string.Join("\n", state)), false);
        }

        private static string Sha256(byte[] bytes, bool uppercase)
        {
            using (SHA256 hash = SHA256.Create())
            {
                return string.Concat(hash.ComputeHash(bytes ?? new byte[0]).Select(
                    value => value.ToString(uppercase ? "X2" : "x2",
                        CultureInfo.InvariantCulture)));
            }
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Skip(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static bool Has(string value, string token)
        {
            return value != null && value.IndexOf(token,
                StringComparison.Ordinal) >= 0;
        }

        private static string FindRoot()
        {
            DirectoryInfo directory = new DirectoryInfo(
                Directory.GetCurrentDirectory());
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
            return Path.Combine(root, relative.Replace('/',
                Path.DirectorySeparatorChar));
        }

        private static string Relative(string root, string absolute)
        {
            return absolute.Substring(root.TrimEnd(Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar).Length + 1).Replace('\\', '/');
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
                    throw new InvalidOperationException("git " + arguments +
                        " failed: " + error);
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

        private sealed class CandidateExpectation
        {
            public CandidateExpectation(
                string ordinal,
                string owner,
                string group,
                string role,
                string mode,
                string key,
                string bp,
                string id)
            {
                Ordinal = ordinal;
                Owner = owner;
                Group = group;
                Role = role;
                Mode = mode;
                Key = key;
                Bp = bp;
                Id = id;
            }

            public string Ordinal { get; private set; }
            public string Owner { get; private set; }
            public string Group { get; private set; }
            public string Role { get; private set; }
            public string Mode { get; private set; }
            public string Key { get; private set; }
            public string Bp { get; private set; }
            public string Id { get; private set; }
        }

        private sealed class Row
        {
            private readonly IDictionary<string, string> fields;

            public Row(string raw, IDictionary<string, string> fields)
            {
                Raw = raw;
                this.fields = fields;
            }

            public string Raw { get; private set; }

            public string this[string name]
            {
                get
                {
                    string value;
                    return fields.TryGetValue(name, out value) ? value : string.Empty;
                }
            }
        }

        private sealed class CanonicalSource
        {
            public CanonicalSource(string name, List<string> lines)
            {
                Name = name;
                Lines = lines;
            }

            public string Name { get; private set; }
            public List<string> Lines { get; private set; }
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

        private sealed class Summary
        {
            public Summary(int failed, string message)
            {
                Failed = failed;
                Message = message;
            }

            public int Failed { get; private set; }
            public string Message { get; private set; }
        }

        private struct AggregateHash
        {
            public AggregateHash(int fileCount, string hash)
            {
                FileCount = fileCount;
                Hash = hash;
            }

            public int FileCount { get; private set; }
            public string Hash { get; private set; }
        }
    }
}
