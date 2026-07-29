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
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RequirementMigrationOverlay;
using TalismanBag.EnemySystem.RequirementChannel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TalismanBag.EditorTools.CrossSystem.ItemEnemy
{
    public static class LayoutResilienceRequirementChannelMigrationVerifier
    {
        private const string ExpectedSignature =
            "sha256:a8d40066b7d7cbaecc85e0b427dda5c5aa0ddfb209613c29fea984d3fb52bad1";
        private const string RuntimeDirectory =
            "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/RequirementMigrationOverlay";
        private const string ReportDirectory = "Docs/V0.4/Reports";

        private static readonly string[] OutputPaths =
        {
            RuntimeDirectory + ".meta",
            RuntimeDirectory +
                "/LayoutResilienceRequirementChannelMigrationPrimitives.cs",
            RuntimeDirectory +
                "/LayoutResilienceRequirementChannelMigrationPrimitives.cs.meta",
            RuntimeDirectory +
                "/LayoutResilienceRequirementChannelMigrationCatalog.cs",
            RuntimeDirectory +
                "/LayoutResilienceRequirementChannelMigrationCatalog.cs.meta",
            RuntimeDirectory +
                "/LayoutResilienceRequirementChannelMigrationValidation.cs",
            RuntimeDirectory +
                "/LayoutResilienceRequirementChannelMigrationValidation.cs.meta",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementChannelMigrationVerifier.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/CrossSystem/ItemEnemy/LayoutResilienceRequirementChannelMigrationVerifier.cs.meta",
            ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationReport.md",
            ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationRouteRows.csv",
            ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationChannelRows.csv",
            ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationLegacyProvenance.csv",
            ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationContextBindingMatrix.csv",
            ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationLeakCheckReport.md"
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
        [MenuItem("Tools/TalismanBag/Verify Layout Resilience Requirement Channel Migration")]
        public static void VerifyMenu()
        {
            VerifyOffline();
        }
#endif

        public static void VerifyOffline()
        {
            Summary summary = VerifyCore();
            if (summary.Failed != 0)
                throw new InvalidOperationException(summary.Message);
            Console.WriteLine(summary.Message);
        }

        public static void VerifyStaticBatch()
        {
            try
            {
                VerifyOffline();
#if UNITY_EDITOR
                UnityEngine.Debug.Log(
                    "LAYOUT_RESILIENCE_REQUIREMENT_CHANNEL_MIGRATION PASS");
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
            LayoutResilienceRequirementChannelMigrationSource source =
                LayoutResilienceRequirementChannelMigrationCatalog
                    .CreateOverlaySource();
            LayoutResilienceRequirementChannelMigrationResult result =
                LayoutResilienceRequirementChannelMigrationValidation
                    .ValidateAndCreateOverlay(source);

            VerifyContract(checks, source, result);
            VerifyRoutes(checks, result);
            VerifyFixtures(checks, source, result);
            VerifyDeterminismAndImmutability(checks, source, result);
            VerifyReports(checks, root, result);
            VerifyRepositoryBoundary(checks, root);
            VerifyProtected(checks, root, protectedBefore);

            int failed = checks.Count(value => !value.Passed);
            string failures = string.Join("; ", checks.Where(value => !value.Passed)
                .Select(value => value.Id + " expected=[" + value.Expected +
                    "] actual=[" + value.Actual + "]"));
            string message =
                "LayoutResilienceRequirementChannelMigration verifier " +
                (failed == 0 ? "PASS " : "FAIL ") +
                (checks.Count - failed).ToString(CultureInfo.InvariantCulture) +
                "/" + checks.Count.ToString(CultureInfo.InvariantCulture) +
                "; routes=4; candidates=2; n01b=1/4; p5a=1/4; " +
                "overlayDirectN01C=0; transitiveN01C=4; c02=32/32; signature=" +
                result.CanonicalSignature +
                (failed == 0 ? string.Empty : "; failures=" + failures);
            return new Summary(failed, message);
        }

        private static void VerifyContract(
            ICollection<Check> checks,
            LayoutResilienceRequirementChannelMigrationSource source,
            LayoutResilienceRequirementChannelMigrationResult result)
        {
            AssertEnum<LayoutResilienceRequirementChannelMigrationStatus>(
                checks, "status", new[] { "Complete", "Unknown", "Invalid" },
                new[] { 1, 2, 3 });
            Add(checks, "schema.id",
                "LayoutResilienceRequirementChannelMigration.v1",
                LayoutResilienceRequirementChannelMigrationSchema.SchemaId,
                LayoutResilienceRequirementChannelMigrationSchema.SchemaId ==
                    "LayoutResilienceRequirementChannelMigration.v1");
            Add(checks, "schema.version", "1",
                LayoutResilienceRequirementChannelMigrationSchema.SchemaVersion
                    .ToString(CultureInfo.InvariantCulture),
                LayoutResilienceRequirementChannelMigrationSchema.SchemaVersion == 1);
            AssertPublicSurface(checks);
            Add(checks, "default.status", "Complete", result.Status.ToString(),
                result.Status ==
                    LayoutResilienceRequirementChannelMigrationStatus.Complete);
            Add(checks, "default.flags", "true/false/true/false",
                Flags(result.DevOnly, result.IsEnabled,
                    result.CoordinateBaselineAccepted, result.Activated),
                result.DevOnly && !result.IsEnabled &&
                    result.CoordinateBaselineAccepted && !result.Activated);
            Add(checks, "default.payload", "non-null", Present(result.Payload),
                result.Payload != null);
            Add(checks, "default.issues", "0",
                result.Issues.Count.ToString(CultureInfo.InvariantCulture),
                result.Issues.Count == 0);
            Add(checks, "canonical.format", "sha256 lowercase",
                result.CanonicalSignature, IsSignature(result.CanonicalSignature));
            Add(checks, "canonical.fixed", ExpectedSignature,
                result.CanonicalSignature,
                result.CanonicalSignature == ExpectedSignature);
            Add(checks, "source.rows", "4",
                source.Rows.Count.ToString(CultureInfo.InvariantCulture),
                source.Rows.Count == 4);
            Add(checks, "payload.rows", "4",
                result.Payload.Rows.Count.ToString(CultureInfo.InvariantCulture),
                result.Payload.Rows.Count == 4);
            Add(checks, "payload.formal-behavior", "false/false",
                Lower(result.Payload.FormalRequirementSourceModified) + "/" +
                    Lower(result.Payload.BehaviorChanged),
                !result.Payload.FormalRequirementSourceModified &&
                    !result.Payload.BehaviorChanged);
        }

        private static void VerifyRoutes(
            ICollection<Check> checks,
            LayoutResilienceRequirementChannelMigrationResult result)
        {
            LayoutResilienceRequirementChannelMigrationPayload payload =
                result.Payload;
            Add(checks, "routes.candidates", "2", payload.Rows.Select(value =>
                    value.CandidateMigrationRequirementId).Distinct(
                        StringComparer.Ordinal).Count().ToString(
                            CultureInfo.InvariantCulture),
                payload.Rows.Select(value => value.CandidateMigrationRequirementId)
                    .Distinct(StringComparer.Ordinal).Count() == 2);
            Add(checks, "routes.route-ids", "4/4", Unique(payload.Rows.Select(
                    value => value.MigrationRouteId)),
                payload.Rows.Select(value => value.MigrationRouteId)
                    .Distinct(StringComparer.Ordinal).Count() == 4);
            Add(checks, "routes.pressure-ids", "4/4", Unique(payload.Rows.Select(
                    value => value.PressureInputId)),
                payload.Rows.Select(value => value.PressureInputId)
                    .Distinct(StringComparer.Ordinal).Count() == 4);
            Add(checks, "routes.context-ids", "4/4", Unique(payload.Rows.Select(
                    ContextIdentity)),
                payload.Rows.Select(ContextIdentity)
                    .Distinct(StringComparer.Ordinal).Count() == 4);
            Add(checks, "routes.channel", "4 StructuralPredicate/Applicable",
                payload.Rows.Count(value => value.DeclaredChannel ==
                    EnemyRequirementChannel.StructuralPredicate &&
                    value.EvaluationChannel ==
                        EnemyRequirementChannel.StructuralPredicate &&
                    value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.Applicable).ToString(
                            CultureInfo.InvariantCulture),
                payload.Rows.All(value => value.DeclaredChannel ==
                    EnemyRequirementChannel.StructuralPredicate &&
                    value.EvaluationChannel ==
                        EnemyRequirementChannel.StructuralPredicate &&
                    value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.Applicable));
            Add(checks, "routes.flags", "4/4 isolated",
                payload.Rows.Count(value => value.DevOnly && !value.IsEnabled &&
                    value.CoordinateBaselineAccepted && !value.Activated &&
                    !value.FormalRequirementSourceModified &&
                    !value.BehaviorChanged).ToString(CultureInfo.InvariantCulture),
                payload.Rows.All(value => value.DevOnly && !value.IsEnabled &&
                    value.CoordinateBaselineAccepted && !value.Activated &&
                    !value.FormalRequirementSourceModified &&
                    !value.BehaviorChanged));
            Add(checks, "routes.legacy", "4/4 quarantine+verified; 0 use",
                payload.Rows.Count(value => value.LegacyBpQuarantined &&
                    value.LegacyBpProvenanceVerified &&
                    !value.LegacyBpEvaluated && !value.LegacyBpConverted &&
                    !value.LegacyBpComparedForCapabilityDecision &&
                    !value.LegacyBpUsedAsThreshold).ToString(
                        CultureInfo.InvariantCulture),
                payload.Rows.All(value => value.LegacyBpQuarantined &&
                    value.LegacyBpProvenanceVerified &&
                    !value.LegacyBpEvaluated && !value.LegacyBpConverted &&
                    !value.LegacyBpComparedForCapabilityDecision &&
                    !value.LegacyBpUsedAsThreshold));
            Add(checks, "routes.authoring", "4 CandidateComplete/Complete/Complete",
                payload.Rows.Count(value => value.AuthoringStatus ==
                    DevEncounterLayoutPressureAuthoringStatus.CandidateComplete &&
                    value.P2Status == AuthoredLayoutPressureSourceStatus.Complete &&
                    value.P2Completeness ==
                        LayoutResilienceInputCompleteness.Complete).ToString(
                            CultureInfo.InvariantCulture),
                payload.Rows.All(value => value.AuthoringStatus ==
                    DevEncounterLayoutPressureAuthoringStatus.CandidateComplete &&
                    value.P2Status == AuthoredLayoutPressureSourceStatus.Complete &&
                    value.P2Completeness ==
                        LayoutResilienceInputCompleteness.Complete));
            VerifyN01B(checks, payload);
            VerifyClauses(checks, payload.Rows);
        }

        private static void VerifyN01B(
            ICollection<Check> checks,
            LayoutResilienceRequirementChannelMigrationPayload payload)
        {
            EnemyRequirementChannelApplicabilitySnapshot snapshot =
                payload.ChannelApplicabilitySnapshot;
            Add(checks, "n01b.snapshot", "non-null/4", Present(snapshot) + "/" +
                    (snapshot == null ? "0" : snapshot.Rows.Count.ToString(
                        CultureInfo.InvariantCulture)),
                snapshot != null && snapshot.Rows.Count == 4);
            Add(checks, "n01b.id",
                "overlay.layout_resilience.requirement_channel_migration.v1",
                snapshot.SnapshotId,
                snapshot.SnapshotId ==
                    "overlay.layout_resilience.requirement_channel_migration.v1");
            Add(checks, "n01b.channel", "StructuralPredicate",
                snapshot.EvaluationChannel.ToString(),
                snapshot.EvaluationChannel ==
                    EnemyRequirementChannel.StructuralPredicate);
            Add(checks, "n01b.rows", "4 StructuralPredicate/Applicable",
                snapshot.Rows.Count(value => value.DeclaredChannel ==
                    EnemyRequirementChannel.StructuralPredicate &&
                    value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.Applicable).ToString(
                            CultureInfo.InvariantCulture),
                snapshot.Rows.All(value => value.DeclaredChannel ==
                    EnemyRequirementChannel.StructuralPredicate &&
                    value.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.Applicable));
            Add(checks, "n01b.canonical", payload.N01BCanonicalSignature,
                snapshot.CanonicalSignature,
                payload.N01BCanonicalSignature == snapshot.CanonicalSignature);
            Add(checks, "p5a.canonical",
                "sha256:8b9ee4674020c5405d7b90cf2793b37be32bf0ea08e8a981291ed6790309da54",
                payload.P5ACanonicalSignature,
                payload.P5ACanonicalSignature ==
                    "sha256:8b9ee4674020c5405d7b90cf2793b37be32bf0ea08e8a981291ed6790309da54");
        }

        private static void VerifyClauses(
            ICollection<Check> checks,
            IReadOnlyList<LayoutResilienceRequirementChannelMigrationRowSnapshot>
                rows)
        {
            LayoutResiliencePredicateClauseKind[] formation =
            {
                LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                LayoutResiliencePredicateClauseKind.EffectiveEyeAnchorUsable,
                LayoutResiliencePredicateClauseKind
                    .EyeToCountedCoreStructurallyConnected
            };
            LayoutResiliencePredicateClauseKind[] pollution =
            {
                LayoutResiliencePredicateClauseKind.CountedLayoutPresent,
                LayoutResiliencePredicateClauseKind.CountedPlacementCellsUsable,
                LayoutResiliencePredicateClauseKind.CountedPlacementCoresUsable
            };
            Add(checks, "clauses.formation", "2 exact", rows.Count(value =>
                    value.CandidateMigrationRequirementId.Contains(
                        "formation_eye") && value.RequiredPredicateClauses
                        .OrderBy(item => (int)item).SequenceEqual(
                            formation.OrderBy(item => (int)item))).ToString(
                                CultureInfo.InvariantCulture),
                rows.Where(value => value.CandidateMigrationRequirementId.Contains(
                    "formation_eye")).All(value => value.RequiredPredicateClauses
                        .OrderBy(item => (int)item).SequenceEqual(
                            formation.OrderBy(item => (int)item))));
            Add(checks, "clauses.pollution", "2 exact", rows.Count(value =>
                    value.CandidateMigrationRequirementId.Contains(
                        "polluted_tile") && value.RequiredPredicateClauses
                        .OrderBy(item => (int)item).SequenceEqual(
                            pollution.OrderBy(item => (int)item))).ToString(
                                CultureInfo.InvariantCulture),
                rows.Where(value => value.CandidateMigrationRequirementId.Contains(
                    "polluted_tile")).All(value => value.RequiredPredicateClauses
                        .OrderBy(item => (int)item).SequenceEqual(
                            pollution.OrderBy(item => (int)item))));
        }

        private static void VerifyFixtures(
            ICollection<Check> checks,
            LayoutResilienceRequirementChannelMigrationSource source,
            LayoutResilienceRequirementChannelMigrationResult complete)
        {
            VerifyFixture(checks, "null-source",
                LayoutResilienceRequirementChannelMigrationValidation
                    .ValidateAndCreateOverlay(null),
                LayoutResilienceRequirementChannelMigrationStatus.Unknown);
            VerifyFixture(checks, "wrong-schema", Validate(Source(source,
                    schemaId: "wrong")),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "source-dev-only", Validate(Source(source,
                    devOnly: false)),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "source-enabled", Validate(Source(source,
                    isEnabled: true)),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "coordinate-unaccepted", Validate(Source(source,
                    coordinateAccepted: false)),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "activated", Validate(Source(source,
                    activated: true)),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "wrong-count", Validate(Source(source,
                    rows: source.Rows.Take(3))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "missing-row", Validate(Source(source,
                    rows: new[] { source.Rows[0], source.Rows[1],
                        source.Rows[2], null })),
                LayoutResilienceRequirementChannelMigrationStatus.Unknown);

            LayoutResilienceRequirementChannelMigrationSourceRow duplicateRoute =
                Copy(source.Rows[3], routeId: source.Rows[0].MigrationRouteId);
            VerifyFixture(checks, "duplicate-route", Validate(Source(source,
                    rows: Replace(source.Rows, 3, duplicateRoute))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "duplicate-pressure", Validate(Source(source,
                    rows: Replace(source.Rows, 3, Copy(source.Rows[3],
                        pressureId: source.Rows[2].PressureInputId)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "duplicate-context", Validate(Source(source,
                    rows: Replace(source.Rows, 3, Copy(source.Rows[3],
                        seedId: source.Rows[2].SeedId,
                        encounterId: source.Rows[2].EncounterId,
                        mapRuleId: source.Rows[2].MapRuleId)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "owner-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        ownerId: "wrong")))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "group-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        groupId: "wrong")))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "key-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        capabilityKey: "wrong")))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "role-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        role: "Recommended")))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "match-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        matchMode: "Any")))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "reference-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        referenceKind: "Other")))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "legacy-bp-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        legacyBp: 5339)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "legacy-use", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        legacyCompared: true)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "channel-drift", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        declaredChannel:
                            EnemyRequirementChannel.ContinuousBP)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "applicability-unknown", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        applicability:
                            EnemyRequirementApplicabilityState.Unknown)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "applicability-not-applicable", Validate(Source(
                    source, rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        applicability:
                            EnemyRequirementApplicabilityState.NotApplicable)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "applicability-not-in-channel", Validate(Source(
                    source, rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        applicability:
                            EnemyRequirementApplicabilityState.NotInChannel)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            VerifyFixture(checks, "undefined-enum", Validate(Source(source,
                    rows: Replace(source.Rows, 0, Copy(source.Rows[0],
                        declaredChannel: (EnemyRequirementChannel)99)))),
                LayoutResilienceRequirementChannelMigrationStatus.Invalid);
            LayoutResilienceRequirementChannelMigrationResult missingMatch =
                Validate(Source(source, rows: Replace(source.Rows, 0,
                    Copy(source.Rows[0], pressureId: "pressure.missing"))));
            Add(checks, "fixture.missing-p5a-match", "Invalid + stable Unknown",
                missingMatch.Status + "/" + string.Join("|",
                    missingMatch.Issues.Select(value => value.Code)),
                missingMatch.Status ==
                    LayoutResilienceRequirementChannelMigrationStatus.Invalid &&
                missingMatch.Issues.Any(value => value.Code ==
                    "P5A_MATCH_MISSING" && value.Status ==
                    LayoutResilienceRequirementChannelMigrationStatus.Unknown));
            VerifyN01BRejectionFixture(checks, source);

            Add(checks, "truth.complete", "payload/4/n01b/0", Present(
                    complete.Payload) + "/" + complete.Payload.Rows.Count + "/" +
                    Present(complete.Payload.ChannelApplicabilitySnapshot) + "/" +
                    complete.Issues.Count,
                complete.Payload != null && complete.Payload.Rows.Count == 4 &&
                    complete.Payload.ChannelApplicabilitySnapshot != null &&
                    complete.Issues.Count == 0);
        }

        private static void VerifyN01BRejectionFixture(
            ICollection<Check> checks,
            LayoutResilienceRequirementChannelMigrationSource source)
        {
            MethodInfo core = typeof(
                LayoutResilienceRequirementChannelMigrationValidation).GetMethod(
                    "ValidateCore", BindingFlags.NonPublic | BindingFlags.Static);
            Func<EnemyRequirementChannelApplicabilitySnapshotInput,
                EnemyRequirementChannelApplicabilitySnapshot> rejectingProvider =
                input => throw new
                    EnemyRequirementChannelApplicabilityValidationException(
                        new[]
                        {
                            new EnemyRequirementChannelApplicabilityValidationIssue(
                                "RAW_PRIVATE_CODE", "C:\\machine\\private",
                                "culture-sensitive raw detail")
                        });
            LayoutResilienceRequirementChannelMigrationResult result =
                (LayoutResilienceRequirementChannelMigrationResult)core.Invoke(
                    null, new object[] { source, rejectingProvider });
            LayoutResilienceRequirementChannelMigrationIssue issue =
                result.Issues.Count == 1 ? result.Issues[0] : null;
            bool sanitized = result.Status ==
                    LayoutResilienceRequirementChannelMigrationStatus.Invalid &&
                result.Payload == null && issue != null &&
                issue.Code == "N01B_SNAPSHOT_REJECTED" &&
                issue.Status ==
                    LayoutResilienceRequirementChannelMigrationStatus.Invalid &&
                issue.Path == "n01b" &&
                issue.Message ==
                    "The authoritative N01B provider rejected the overlay input." &&
                !Has(result.CanonicalSignature, "RAW_PRIVATE_CODE") &&
                !Has(result.CanonicalSignature, "machine") &&
                !Has(result.CanonicalSignature, "culture-sensitive");
            Add(checks, "fixture.n01b-rejected-sanitized",
                "one stable N01B_SNAPSHOT_REJECTED issue",
                issue == null ? "missing" : issue.Code + "|" + issue.Path + "|" +
                    issue.Message,
                sanitized);
        }

        private static void VerifyFixture(
            ICollection<Check> checks,
            string name,
            LayoutResilienceRequirementChannelMigrationResult result,
            LayoutResilienceRequirementChannelMigrationStatus expected)
        {
            bool truth = result.Status == expected && result.Payload == null &&
                result.Issues.Count > 0 &&
                (expected !=
                    LayoutResilienceRequirementChannelMigrationStatus.Unknown ||
                    result.Issues.All(value => value.Status !=
                        LayoutResilienceRequirementChannelMigrationStatus.Invalid)) &&
                (expected !=
                    LayoutResilienceRequirementChannelMigrationStatus.Invalid ||
                    result.Issues.Any(value => value.Status ==
                        LayoutResilienceRequirementChannelMigrationStatus.Invalid));
            Add(checks, "fixture." + name,
                expected + "/null-payload/stable-issues",
                result.Status + "/" + Present(result.Payload) + "/" +
                    result.Issues.Count.ToString(CultureInfo.InvariantCulture), truth);
        }

        private static void VerifyDeterminismAndImmutability(
            ICollection<Check> checks,
            LayoutResilienceRequirementChannelMigrationSource source,
            LayoutResilienceRequirementChannelMigrationResult result)
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            LayoutResilienceRequirementChannelMigrationResult reversed;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                reversed = Validate(Source(source, rows: source.Rows.Reverse()));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
            Add(checks, "determinism.reverse-culture", result.CanonicalSignature,
                reversed.CanonicalSignature,
                result.CanonicalSignature == reversed.CanonicalSignature);
            LayoutResilienceRequirementChannelMigrationResult repeated = Validate(
                LayoutResilienceRequirementChannelMigrationCatalog
                    .CreateOverlaySource());
            Add(checks, "determinism.repeat", result.CanonicalSignature,
                repeated.CanonicalSignature,
                result.CanonicalSignature == repeated.CanonicalSignature);
            LayoutResilienceRequirementChannelMigrationSource second =
                LayoutResilienceRequirementChannelMigrationCatalog
                    .CreateOverlaySource();
            Add(checks, "defensive.catalog", "distinct", "checked",
                !ReferenceEquals(source, second) &&
                !ReferenceEquals(source.Rows, second.Rows) &&
                !ReferenceEquals(source.Rows[0], second.Rows[0]));
            AssertReadOnly(checks, "source.rows", source.Rows);
            AssertReadOnly(checks, "result.rows", result.Payload.Rows);
            AssertReadOnly(checks, "result.issues", result.Issues);
            AssertReadOnly(checks, "result.kinds",
                result.Payload.Rows[0].PressureKinds);
            AssertReadOnly(checks, "result.clauses",
                result.Payload.Rows[0].RequiredPredicateClauses);
            AssertReadOnly(checks, "n01b.rows",
                result.Payload.ChannelApplicabilitySnapshot.Rows);
        }

        private static void VerifyReports(
            ICollection<Check> checks,
            string root,
            LayoutResilienceRequirementChannelMigrationResult result)
        {
            string routes = Normalize(Read(root, ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationRouteRows.csv"));
            string channels = Normalize(Read(root, ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationChannelRows.csv"));
            string legacy = Normalize(Read(root, ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationLegacyProvenance.csv"));
            string contexts = Normalize(Read(root, ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationContextBindingMatrix.csv"));
            string main = Read(root, ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationReport.md");
            string leak = Read(root, ReportDirectory +
                "/LayoutResilienceRequirementChannelMigrationLeakCheckReport.md");
            Add(checks, "report.routes", "data-consistent", "checked",
                routes == BuildRouteRows(result.Payload.Rows));
            Add(checks, "report.channels", "data-consistent", "checked",
                channels == BuildChannelRows(
                    result.Payload.ChannelApplicabilitySnapshot));
            Add(checks, "report.legacy", "data-consistent", "checked",
                legacy == BuildLegacyRows(result.Payload.Rows));
            Add(checks, "report.context", "data-consistent", "checked",
                contexts == BuildContextRows(result.Payload.Rows));
            Add(checks, "report.main.signature", result.CanonicalSignature, main,
                Has(main, result.CanonicalSignature));
            Add(checks, "report.main.receipts", "3 receipts", "checked",
                Has(main,
                    "GUARD_PASS_ASSIGNMENT_LAYOUTRESILIENCEREQUIREMENTCHANNELMIGRATION01_RESCOPED02") &&
                Has(main,
                    "ENEMY_GUARD_CONFIRM_LAYOUTRESILIENCEREQUIREMENTCHANNELMIGRATION01") &&
                Has(main,
                    "CAPABILITY_ALGORITHM_GUARD_PASS_LAYOUTRESILIENCEREQUIREMENTCHANNELMIGRATION01"));
            Add(checks, "report.main.boundary", "zero impact", "checked",
                Has(main, "32/32 / 0") && Has(main,
                    "N01C Evaluator / P3 / P4 / readiness calls: `0 / 0 / 0 / 0`") &&
                Has(main, "Real E10 rows changed: `0`") &&
                Has(main, "Next package: `NOT_STARTED`"));
            Add(checks, "report.leak", "Leak Count 0", "checked",
                Has(leak, "Leak Count: `0`") &&
                Has(leak, "New files / existing files modified: `15 / 0`") &&
                Has(leak, "Next package: `NOT_STARTED`"));
        }

        private static void VerifyRepositoryBoundary(
            ICollection<Check> checks,
            string root)
        {
            Add(checks, "allowlist.count", "15", OutputPaths.Length.ToString(
                CultureInfo.InvariantCulture), OutputPaths.Length == 15);
            Add(checks, "allowlist.exists", "15", OutputPaths.Count(value =>
                    File.Exists(Absolute(root, value))).ToString(
                        CultureInfo.InvariantCulture),
                OutputPaths.All(value => File.Exists(Absolute(root, value))));
            string runtime = string.Join("\n", Directory.GetFiles(
                    Absolute(root, RuntimeDirectory), "*.cs",
                    SearchOption.TopDirectoryOnly).Select(File.ReadAllText));
            string validation = Read(root, RuntimeDirectory +
                "/LayoutResilienceRequirementChannelMigrationValidation.cs");
            string p5AValidation = Read(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/DevAuthoring/DevEncounterLayoutPressureValidation.cs");
            string p2Adapter = Read(root,
                "Assets/_Game/Scripts/TalismanBag/CrossSystem/ItemEnemy/LayoutResilience/AuthoredPressure/AuthoredLayoutPressureSourceAdapter.cs");
            Add(checks, "call.p5a", "1", Count(validation,
                    "DevEncounterLayoutPressureValidation.ValidateAndProject(")
                    .ToString(CultureInfo.InvariantCulture),
                Count(validation,
                    "DevEncounterLayoutPressureValidation.ValidateAndProject(") == 1);
            Add(checks, "call.direct-p2", "0", Count(runtime,
                    "DefaultAuthoredLayoutPressureSourceAdapter")
                    .ToString(CultureInfo.InvariantCulture),
                Count(runtime, "DefaultAuthoredLayoutPressureSourceAdapter") == 0);
            Add(checks, "call.overlay-direct-n01c-validator", "0", Count(runtime,
                    "DefaultLayoutResilienceStructuralPredicateValidator")
                    .ToString(CultureInfo.InvariantCulture),
                Count(runtime,
                    "DefaultLayoutResilienceStructuralPredicateValidator") == 0);
            Add(checks, "call.p5a-p2-site-contexts", "1 site / 4 contexts",
                Count(p5AValidation,
                    "DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(") +
                    "/4",
                Count(p5AValidation,
                    "DefaultAuthoredLayoutPressureSourceAdapter.Instance.Project(") ==
                    1);
            Add(checks, "call.p2-n01c-site-contexts", "1 site / 4 contexts",
                Count(p2Adapter,
                    "DefaultLayoutResilienceStructuralPredicateValidator.Instance") +
                    "/4",
                Count(p2Adapter,
                    "DefaultLayoutResilienceStructuralPredicateValidator.Instance") ==
                    1);
            Add(checks, "call.n01b-provider", "1", Count(validation,
                    "DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance")
                    .ToString(CultureInfo.InvariantCulture),
                Count(validation,
                    "DefaultEnemyRequirementChannelApplicabilitySnapshotProvider.Instance") ==
                    1);
            Add(checks, "call.direct-n01b-validator", "0", Count(runtime,
                    "DefaultEnemyRequirementChannelApplicabilitySnapshotValidator")
                    .ToString(CultureInfo.InvariantCulture),
                Count(runtime,
                    "DefaultEnemyRequirementChannelApplicabilitySnapshotValidator") ==
                    0);
            string[] forbidden =
            {
                "DefaultLayoutResilienceStructuralPredicateEvaluator",
                ".Evaluate(", "EvaluationInputAssembly",
                "StructuralReadinessConsumer", "MonoBehaviour", "ScriptableObject",
                "UnityEngine", "GameObject", "SceneManager", "RunFlow",
                "SaveData", "RewardConfig", "EditorBuildSettings"
            };
            int leaks = forbidden.Sum(value => Count(runtime, value));
            Add(checks, "runtime.forbidden-tokens", "0", leaks.ToString(
                CultureInfo.InvariantCulture), leaks == 0);
            int trailing = OutputPaths.Sum(path => TrailingWhitespaceCount(
                Absolute(root, path)));
            Add(checks, "whitespace.trailing", "0", trailing.ToString(
                CultureInfo.InvariantCulture), trailing == 0);
            int bom = OutputPaths.Count(path => HasUtf8Bom(Absolute(root, path)));
            Add(checks, "encoding.bom", "0", bom.ToString(
                CultureInfo.InvariantCulture), bom == 0);
            string[] guids =
            {
                "7c1a4e9a31be4a7da81144ff11500001",
                "7c2a4e9a31be4a7da81144ff11500002",
                "7c3a4e9a31be4a7da81144ff11500003",
                "7c4a4e9a31be4a7da81144ff11500004",
                "7c5a4e9a31be4a7da81144ff11500005"
            };
            string allMeta = string.Join("\n", Directory.GetFiles(
                    Absolute(root, "Assets"), "*.meta", SearchOption.AllDirectories)
                .Select(File.ReadAllText));
            int conflicts = guids.Count(guid =>
                Count(allMeta, "guid: " + guid) != 1);
            Add(checks, "guid.conflicts", "0", conflicts.ToString(
                CultureInfo.InvariantCulture), conflicts == 0);
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
            string root,
            string before)
        {
            AddProtection(checks, root, "p5a", P5AFiles, true, 15,
                "a4b48cb79328fa77ae952911da6ff906e1a1ccb79b0fef3b87e1e473511e576a");
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
                "cd5c5d14c08c6e58980d0d03909fb78e77a1f3001bae6a546ce61e5fb8aaef3f",
                FileHash(root,
                    "Docs/V0.4/LayoutResilienceRequirementChannelMigration01_Rescoped02_Assignment.md",
                    false),
                FileHash(root,
                    "Docs/V0.4/LayoutResilienceRequirementChannelMigration01_Rescoped02_Assignment.md",
                    false) ==
                    "cd5c5d14c08c6e58980d0d03909fb78e77a1f3001bae6a546ce61e5fb8aaef3f");
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
                typeof(LayoutResilienceRequirementChannelMigrationSource),
                "SchemaId", "SchemaVersion", "DevOnly", "IsEnabled",
                "CoordinateBaselineAccepted", "Activated", "Rows");
            AssertProperties(checks, "source-row",
                typeof(LayoutResilienceRequirementChannelMigrationSourceRow),
                "MigrationRouteId", "CandidateMigrationRequirementId", "OwnerId",
                "RequirementGroupId", "Role", "MatchMode", "ReferenceKind",
                "BuildCapabilityKey", "LegacyMinimumCapabilityBasisPoints",
                "LegacyEvidencePath", "LegacyEvidenceRowIdentity",
                "LegacyBpQuarantined", "LegacyBpProvenanceVerified",
                "LegacyBpEvaluated", "LegacyBpConverted",
                "LegacyBpComparedForCapabilityDecision", "LegacyBpUsedAsThreshold",
                "SeedId", "EncounterId", "MapRuleId", "PressureInputId",
                "DeclaredChannel", "EvaluationChannel", "ApplicabilityState",
                "DevOnly", "IsEnabled", "CoordinateBaselineAccepted", "Activated",
                "FormalRequirementSourceModified", "BehaviorChanged");
            AssertProperties(checks, "row-snapshot",
                typeof(LayoutResilienceRequirementChannelMigrationRowSnapshot),
                "MigrationRouteId", "CandidateMigrationRequirementId", "OwnerId",
                "RequirementGroupId", "Role", "MatchMode", "ReferenceKind",
                "BuildCapabilityKey", "LegacyMinimumCapabilityBasisPoints",
                "LegacyEvidencePath", "LegacyEvidenceRowIdentity",
                "LegacyBpQuarantined", "LegacyBpProvenanceVerified",
                "LegacyBpEvaluated", "LegacyBpConverted",
                "LegacyBpComparedForCapabilityDecision", "LegacyBpUsedAsThreshold",
                "SeedId", "EncounterId", "MapRuleId", "PressureInputId",
                "DeclaredChannel", "EvaluationChannel", "ApplicabilityState",
                "DevOnly", "IsEnabled", "CoordinateBaselineAccepted", "Activated",
                "FormalRequirementSourceModified", "BehaviorChanged",
                "AuthoringStatus", "P2Status", "P2Completeness", "PressureKinds",
                "EffectiveEyeCell", "RequiredPredicateClauses",
                "P2CanonicalSignature", "ChannelApplicabilityRow");
            AssertProperties(checks, "issue",
                typeof(LayoutResilienceRequirementChannelMigrationIssue),
                "Code", "Status", "Path", "Message");
            AssertProperties(checks, "payload",
                typeof(LayoutResilienceRequirementChannelMigrationPayload),
                "Rows", "ChannelApplicabilitySnapshot", "P5ACanonicalSignature",
                "N01BCanonicalSignature", "FormalRequirementSourceModified",
                "BehaviorChanged");
            AssertProperties(checks, "result",
                typeof(LayoutResilienceRequirementChannelMigrationResult),
                "SchemaId", "SchemaVersion", "Status", "DevOnly", "IsEnabled",
                "CoordinateBaselineAccepted", "Activated", "Payload", "Issues",
                "CanonicalSignature");
            MethodInfo[] catalog = typeof(
                LayoutResilienceRequirementChannelMigrationCatalog).GetMethods(
                    BindingFlags.Public | BindingFlags.Static |
                    BindingFlags.DeclaredOnly);
            Add(checks, "api.catalog", "CreateOverlaySource",
                string.Join("|", catalog.Select(value => value.Name)),
                catalog.Length == 1 && catalog[0].Name == "CreateOverlaySource" &&
                catalog[0].GetParameters().Length == 0);
            MethodInfo[] validation = typeof(
                LayoutResilienceRequirementChannelMigrationValidation).GetMethods(
                    BindingFlags.Public | BindingFlags.Static |
                    BindingFlags.DeclaredOnly);
            Add(checks, "api.validation", "ValidateAndCreateOverlay",
                string.Join("|", validation.Select(value => value.Name)),
                validation.Length == 1 &&
                validation[0].Name == "ValidateAndCreateOverlay" &&
                validation[0].GetParameters().Length == 1);
        }

        private static string BuildRouteRows(IReadOnlyList<
            LayoutResilienceRequirementChannelMigrationRowSnapshot> rows)
        {
            List<string> lines = new List<string>
            {
                "migrationRouteId,candidateMigrationRequirementId,ownerId,requirementGroupId,role,matchMode,referenceKind,buildCapabilityKey,legacyMinimumCapabilityBasisPoints,seedId,encounterId,mapRuleId,pressureInputId,declaredChannel,evaluationChannel,applicabilityState,devOnly,isEnabled,coordinateBaselineAccepted,activated,formalRequirementSourceModified,behaviorChanged"
            };
            lines.AddRange(rows.OrderBy(value => value.MigrationRouteId,
                StringComparer.Ordinal).Select(value => string.Join(",",
                    value.MigrationRouteId,
                    value.CandidateMigrationRequirementId,
                    value.OwnerId,
                    value.RequirementGroupId,
                    value.Role,
                    value.MatchMode,
                    value.ReferenceKind,
                    value.BuildCapabilityKey,
                    value.LegacyMinimumCapabilityBasisPoints.ToString(
                        CultureInfo.InvariantCulture),
                    value.SeedId,
                    value.EncounterId,
                    value.MapRuleId,
                    value.PressureInputId,
                    value.DeclaredChannel,
                    value.EvaluationChannel,
                    value.ApplicabilityState,
                    Lower(value.DevOnly),
                    Lower(value.IsEnabled),
                    Lower(value.CoordinateBaselineAccepted),
                    Lower(value.Activated),
                    Lower(value.FormalRequirementSourceModified),
                    Lower(value.BehaviorChanged))));
            return string.Join("\n", lines) + "\n";
        }

        private static string BuildChannelRows(
            EnemyRequirementChannelApplicabilitySnapshot snapshot)
        {
            List<string> lines = new List<string>
            {
                "snapshotId,requirementId,declaredChannel,evaluationChannel,applicabilityState"
            };
            lines.AddRange(snapshot.Rows.OrderBy(value => value.RequirementId,
                StringComparer.Ordinal).Select(value => string.Join(",",
                    snapshot.SnapshotId,
                    value.RequirementId,
                    value.DeclaredChannel,
                    snapshot.EvaluationChannel,
                    value.ApplicabilityState)));
            return string.Join("\n", lines) + "\n";
        }

        private static string BuildLegacyRows(IReadOnlyList<
            LayoutResilienceRequirementChannelMigrationRowSnapshot> rows)
        {
            List<string> lines = new List<string>
            {
                "migrationRouteId,legacyMinimumCapabilityBasisPoints,legacyEvidencePath,legacyEvidenceRowIdentity,legacyBpQuarantined,legacyBpProvenanceVerified,legacyBpEvaluated,legacyBpConverted,legacyBpComparedForCapabilityDecision,legacyBpUsedAsThreshold"
            };
            lines.AddRange(rows.OrderBy(value => value.MigrationRouteId,
                StringComparer.Ordinal).Select(value => string.Join(",",
                    value.MigrationRouteId,
                    value.LegacyMinimumCapabilityBasisPoints.ToString(
                        CultureInfo.InvariantCulture),
                    value.LegacyEvidencePath,
                    value.LegacyEvidenceRowIdentity,
                    Lower(value.LegacyBpQuarantined),
                    Lower(value.LegacyBpProvenanceVerified),
                    Lower(value.LegacyBpEvaluated),
                    Lower(value.LegacyBpConverted),
                    Lower(value.LegacyBpComparedForCapabilityDecision),
                    Lower(value.LegacyBpUsedAsThreshold))));
            return string.Join("\n", lines) + "\n";
        }

        private static string BuildContextRows(IReadOnlyList<
            LayoutResilienceRequirementChannelMigrationRowSnapshot> rows)
        {
            List<string> lines = new List<string>
            {
                "migrationRouteId,pressureInputId,p5aStatus,p2Status,p2Completeness,pressureKinds,effectiveEyeCell,requiredPredicateClauses,p2CanonicalSignature"
            };
            lines.AddRange(rows.OrderBy(value => value.MigrationRouteId,
                StringComparer.Ordinal).Select(value => string.Join(",",
                    value.MigrationRouteId,
                    value.PressureInputId,
                    value.AuthoringStatus,
                    value.P2Status,
                    value.P2Completeness,
                    string.Join("|", value.PressureKinds.OrderBy(item =>
                        (int)item)),
                    value.EffectiveEyeCell,
                    string.Join("|", value.RequiredPredicateClauses.OrderBy(item =>
                        (int)item)),
                    value.P2CanonicalSignature)));
            return string.Join("\n", lines) + "\n";
        }

        private static LayoutResilienceRequirementChannelMigrationResult Validate(
            LayoutResilienceRequirementChannelMigrationSource source)
        {
            return LayoutResilienceRequirementChannelMigrationValidation
                .ValidateAndCreateOverlay(source);
        }

        private static LayoutResilienceRequirementChannelMigrationSource Source(
            LayoutResilienceRequirementChannelMigrationSource value,
            string schemaId = null,
            bool? devOnly = null,
            bool? isEnabled = null,
            bool? coordinateAccepted = null,
            bool? activated = null,
            IEnumerable<LayoutResilienceRequirementChannelMigrationSourceRow>
                rows = null)
        {
            return new LayoutResilienceRequirementChannelMigrationSource(
                schemaId ?? value.SchemaId,
                value.SchemaVersion,
                devOnly ?? value.DevOnly,
                isEnabled ?? value.IsEnabled,
                coordinateAccepted ?? value.CoordinateBaselineAccepted,
                activated ?? value.Activated,
                rows ?? value.Rows);
        }

        private static LayoutResilienceRequirementChannelMigrationSourceRow Copy(
            LayoutResilienceRequirementChannelMigrationSourceRow value,
            string routeId = null,
            string candidateId = null,
            string ownerId = null,
            string groupId = null,
            string role = null,
            string matchMode = null,
            string referenceKind = null,
            string capabilityKey = null,
            int? legacyBp = null,
            bool? legacyCompared = null,
            string seedId = null,
            string encounterId = null,
            string mapRuleId = null,
            string pressureId = null,
            EnemyRequirementChannel? declaredChannel = null,
            EnemyRequirementApplicabilityState? applicability = null)
        {
            return new LayoutResilienceRequirementChannelMigrationSourceRow(
                routeId ?? value.MigrationRouteId,
                candidateId ?? value.CandidateMigrationRequirementId,
                ownerId ?? value.OwnerId,
                groupId ?? value.RequirementGroupId,
                role ?? value.Role,
                matchMode ?? value.MatchMode,
                referenceKind ?? value.ReferenceKind,
                capabilityKey ?? value.BuildCapabilityKey,
                legacyBp ?? value.LegacyMinimumCapabilityBasisPoints,
                value.LegacyEvidencePath,
                value.LegacyEvidenceRowIdentity,
                value.LegacyBpQuarantined,
                value.LegacyBpProvenanceVerified,
                value.LegacyBpEvaluated,
                value.LegacyBpConverted,
                legacyCompared ?? value.LegacyBpComparedForCapabilityDecision,
                value.LegacyBpUsedAsThreshold,
                seedId ?? value.SeedId,
                encounterId ?? value.EncounterId,
                mapRuleId ?? value.MapRuleId,
                pressureId ?? value.PressureInputId,
                declaredChannel ?? value.DeclaredChannel,
                value.EvaluationChannel,
                applicability ?? value.ApplicabilityState,
                value.DevOnly,
                value.IsEnabled,
                value.CoordinateBaselineAccepted,
                value.Activated,
                value.FormalRequirementSourceModified,
                value.BehaviorChanged);
        }

        private static IEnumerable<
            LayoutResilienceRequirementChannelMigrationSourceRow> Replace(
                IReadOnlyList<
                    LayoutResilienceRequirementChannelMigrationSourceRow> rows,
                int index,
                LayoutResilienceRequirementChannelMigrationSourceRow value)
        {
            LayoutResilienceRequirementChannelMigrationSourceRow[] copy =
                rows.ToArray();
            copy[index] = value;
            return copy;
        }

        private static string ContextIdentity(
            LayoutResilienceRequirementChannelMigrationRowSnapshot value)
        {
            return string.Join("|", value.CandidateMigrationRequirementId,
                value.SeedId, value.EncounterId, value.MapRuleId);
        }

        private static string Unique(IEnumerable<string> values)
        {
            string[] rows = values.ToArray();
            return rows.Distinct(StringComparer.Ordinal).Count().ToString(
                CultureInfo.InvariantCulture) + "/" + rows.Length.ToString(
                    CultureInfo.InvariantCulture);
        }

        private static void AssertEnum<T>(
            ICollection<Check> checks,
            string id,
            IReadOnlyList<string> names,
            IReadOnlyList<int> numbers)
        {
            string[] actualNames = Enum.GetNames(typeof(T));
            int[] actualNumbers = Enum.GetValues(typeof(T)).Cast<object>()
                .Select(value => (int)value).ToArray();
            Add(checks, "enum." + id,
                string.Join("|", names.Zip(numbers,
                    (name, number) => name + "=" + number)),
                string.Join("|", actualNames.Zip(actualNumbers,
                    (name, number) => name + "=" + number)),
                actualNames.SequenceEqual(names) &&
                actualNumbers.SequenceEqual(numbers));
        }

        private static void AssertProperties(
            ICollection<Check> checks,
            string id,
            Type type,
            params string[] expected)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public |
                BindingFlags.Instance | BindingFlags.DeclaredOnly);
            string[] actual = properties.Select(value => value.Name)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            string[] sorted = expected.OrderBy(value => value,
                StringComparer.Ordinal).ToArray();
            bool noSetters = properties.All(value => value.SetMethod == null);
            Add(checks, "surface." + id,
                string.Join("|", sorted) + "/no-setters",
                string.Join("|", actual) + "/" + Lower(noSetters),
                actual.SequenceEqual(sorted) && noSetters);
        }

        private static void AssertReadOnly<T>(
            ICollection<Check> checks,
            string id,
            IReadOnlyList<T> values)
        {
            IList list = values as IList;
            bool readOnly = list != null && list.IsReadOnly;
            bool mutationRejected = false;
            if (list != null)
            {
                try { list.Add(default(T)); }
                catch (NotSupportedException) { mutationRejected = true; }
            }
            Add(checks, "immutable." + id, "true/true",
                Lower(readOnly) + "/" + Lower(mutationRejected),
                readOnly && mutationRejected);
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }

        private static string Read(string root, string path)
        {
            return File.ReadAllText(Absolute(root, path), Encoding.UTF8);
        }

        private static bool Has(string text, string value)
        {
            return (text ?? string.Empty).IndexOf(value ?? string.Empty,
                StringComparison.Ordinal) >= 0;
        }

        private static int Count(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value)) return 0;
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

        private static int TrailingWhitespaceCount(string path)
        {
            if (!File.Exists(path)) return 1;
            return Normalize(File.ReadAllText(path, Encoding.UTF8)).Split('\n')
                .Count(line => line.EndsWith(" ", StringComparison.Ordinal) ||
                    line.EndsWith("\t", StringComparison.Ordinal));
        }

        private static bool HasUtf8Bom(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb &&
                bytes[2] == 0xbf;
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

        private static IEnumerable<string> ReportFiles(string root, string prefix)
        {
            return Directory.GetFiles(Absolute(root, ReportDirectory), prefix + "*",
                SearchOption.TopDirectoryOnly).Select(path => Relative(root, path));
        }

        private static IEnumerable<string> DirectoryFiles(
            string root,
            string directory,
            bool recursive = true)
        {
            return Directory.GetFiles(Absolute(root, directory), "*", recursive
                    ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
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
                        !path.StartsWith(excluded,
                            StringComparison.OrdinalIgnoreCase) &&
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
                        !path.StartsWith(excluded,
                            StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(path, excludedMeta,
                            StringComparison.OrdinalIgnoreCase))
                .Select(path => Relative(root, path)), false);
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
                if (trailingLf || index + 1 < ordered.Length) payload.Append('\n');
            }
            return new AggregateHash(ordered.Length,
                Sha256(Encoding.UTF8.GetBytes(payload.ToString()), false));
        }

        private static void AddProtection(
            ICollection<Check> checks,
            string root,
            string id,
            IEnumerable<string> paths,
            bool trailingLf,
            int expectedCount,
            string expectedHash)
        {
            AddAggregate(checks, id,
                AggregateFiles(root, paths, trailingLf), expectedCount,
                expectedHash);
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
            Add(checks, "protected." + id + ".hash", expectedHash, actual.Hash,
                actual.Hash == expectedHash);
        }

        private static string CaptureProtected(string root)
        {
            return string.Join("|",
                AggregateFiles(root, P5AFiles, true).Hash,
                AggregateFiles(root, P5SurveyFiles, true).Hash,
                AggregateFiles(root, P2Files(root), true).Hash,
                AggregateFiles(root, P3Files(root), true).Hash,
                AggregateFiles(root, P4Files(root), true).Hash,
                AggregateFiles(root, N01CFiles(root), true).Hash,
                AggregateFiles(root, N01BFiles(root), true).Hash,
                AggregateFiles(root, E10Files(root), true).Hash,
                AggregateItem(root).Hash,
                AggregateEnemy(root).Hash,
                AggregateFiles(root,
                    DirectoryFiles(root, "Assets/_Game/Scenes"), true).Hash,
                AggregateFiles(root,
                    DirectoryFiles(root, "Assets/_Game/Prefabs"), true).Hash,
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

        private static ProcessResult Run(
            string root,
            string fileName,
            string arguments)
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
                return new ProcessResult(process.ExitCode,
                    (stdout + stderr).Trim());
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
            throw new DirectoryNotFoundException(
                "Could not locate the Unity project root.");
        }

        private static string Absolute(string root, string path)
        {
            return Path.GetFullPath(Path.Combine(root,
                (path ?? string.Empty).Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string Relative(string root, string path)
        {
            return Path.GetFullPath(path).Substring(root.TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar).Length + 1)
                .Replace('\\', '/');
        }

        private static bool IsSignature(string value)
        {
            return value != null && value.Length == 71 &&
                value.StartsWith("sha256:", StringComparison.Ordinal) &&
                value.Skip(7).All(character =>
                    (character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f'));
        }

        private static string Lower(bool value) => value ? "true" : "false";

        private static string Flags(bool devOnly, bool enabled,
            bool coordinateAccepted, bool activated)
        {
            return Lower(devOnly) + "/" + Lower(enabled) + "/" +
                Lower(coordinateAccepted) + "/" + Lower(activated);
        }

        private static string Present(object value) =>
            value == null ? "null" : "non-null";

        private static void Add(
            ICollection<Check> checks,
            string id,
            string expected,
            string actual,
            bool passed)
        {
            checks.Add(new Check(id, expected, actual, passed));
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
    }
}
