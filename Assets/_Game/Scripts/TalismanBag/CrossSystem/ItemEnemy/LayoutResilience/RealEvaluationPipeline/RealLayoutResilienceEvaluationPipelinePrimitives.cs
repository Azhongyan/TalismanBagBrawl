using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness;
using TalismanBag.EnemySystem.RequirementChannel;
using TalismanBag.Items;
using TalismanBag.Items.Capability;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline
{
    public static class RealLayoutResilienceEvaluationPipelineSchema
    {
        public const string SchemaId =
            "RealLayoutResilienceEvaluationPipeline.v1";
        public const int SchemaVersion = 1;
    }

    public enum RealLayoutResilienceEvaluationPipelineStatus
    {
        Complete = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class RealLayoutResilienceEvaluationPipelineInput
    {
        public RealLayoutResilienceEvaluationPipelineInput(
            string evaluationBatchId,
            ItemSystemSnapshot itemSnapshot,
            ItemInstancePlacementBindingContractSnapshot bindingSnapshot,
            string schemaId = RealLayoutResilienceEvaluationPipelineSchema.SchemaId,
            int schemaVersion = RealLayoutResilienceEvaluationPipelineSchema
                .SchemaVersion)
        {
            SchemaId = schemaId;
            SchemaVersion = schemaVersion;
            EvaluationBatchId = evaluationBatchId;
            ItemSnapshot = itemSnapshot;
            BindingSnapshot = bindingSnapshot;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string EvaluationBatchId { get; }
        public ItemSystemSnapshot ItemSnapshot { get; }
        public ItemInstancePlacementBindingContractSnapshot BindingSnapshot { get; }
    }

    public sealed class RealLayoutResilienceEvaluationPipelineIssue
    {
        public RealLayoutResilienceEvaluationPipelineIssue(
            string code,
            RealLayoutResilienceEvaluationPipelineStatus status,
            string path,
            string message)
        {
            Code = code ?? string.Empty;
            Status = status;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public RealLayoutResilienceEvaluationPipelineStatus Status { get; }
        public string Path { get; }
        public string Message { get; }

        internal RealLayoutResilienceEvaluationPipelineIssue Clone()
        {
            return new RealLayoutResilienceEvaluationPipelineIssue(
                Code, Status, Path, Message);
        }
    }

    public sealed class RealLayoutResilienceEvaluationRouteRowSnapshot
    {
        internal RealLayoutResilienceEvaluationRouteRowSnapshot(
            string migrationRouteId,
            string candidateMigrationRequirementId,
            string ownerId,
            string requirementGroupId,
            string seedId,
            string encounterId,
            string mapRuleId,
            string pressureInputId,
            EnemyRequirementChannel declaredChannel,
            EnemyRequirementChannel evaluationChannel,
            EnemyRequirementApplicabilityState applicabilityState,
            bool coordinateBaselineAccepted,
            bool activated,
            RealLayoutResilienceEvaluationPipelineStatus status,
            bool p2ResultPresent,
            AuthoredLayoutPressureSourceStatus? p2Status,
            LayoutResilienceInputCompleteness? p2Completeness,
            string p2CanonicalSignature,
            bool p3ResultPresent,
            LayoutResilienceEvaluationInputAssemblerStatus? p3Status,
            string p3CanonicalSignature,
            bool p4ResultPresent,
            LayoutResilienceStructuralReadinessConsumerStatus? p4Status,
            string p4CanonicalSignature,
            bool p4ResultValidated,
            LayoutResiliencePredicateResultSnapshot predicateResult)
        {
            MigrationRouteId = migrationRouteId ?? string.Empty;
            CandidateMigrationRequirementId =
                candidateMigrationRequirementId ?? string.Empty;
            OwnerId = ownerId ?? string.Empty;
            RequirementGroupId = requirementGroupId ?? string.Empty;
            SeedId = seedId ?? string.Empty;
            EncounterId = encounterId ?? string.Empty;
            MapRuleId = mapRuleId ?? string.Empty;
            PressureInputId = pressureInputId ?? string.Empty;
            DeclaredChannel = declaredChannel;
            EvaluationChannel = evaluationChannel;
            ApplicabilityState = applicabilityState;
            CoordinateBaselineAccepted = coordinateBaselineAccepted;
            Activated = activated;
            Status = status;
            P2ResultPresent = p2ResultPresent;
            P2Status = p2Status;
            P2Completeness = p2Completeness;
            P2CanonicalSignature = p2CanonicalSignature ?? string.Empty;
            P3ResultPresent = p3ResultPresent;
            P3Status = p3Status;
            P3CanonicalSignature = p3CanonicalSignature ?? string.Empty;
            P4ResultPresent = p4ResultPresent;
            P4Status = p4Status;
            P4CanonicalSignature = p4CanonicalSignature ?? string.Empty;
            P4ResultValidated = p4ResultValidated;
            PredicateResult = CopyPredicate(predicateResult);
        }

        private RealLayoutResilienceEvaluationRouteRowSnapshot(
            RealLayoutResilienceEvaluationRouteRowSnapshot value)
            : this(
                value.MigrationRouteId,
                value.CandidateMigrationRequirementId,
                value.OwnerId,
                value.RequirementGroupId,
                value.SeedId,
                value.EncounterId,
                value.MapRuleId,
                value.PressureInputId,
                value.DeclaredChannel,
                value.EvaluationChannel,
                value.ApplicabilityState,
                value.CoordinateBaselineAccepted,
                value.Activated,
                value.Status,
                value.P2ResultPresent,
                value.P2Status,
                value.P2Completeness,
                value.P2CanonicalSignature,
                value.P3ResultPresent,
                value.P3Status,
                value.P3CanonicalSignature,
                value.P4ResultPresent,
                value.P4Status,
                value.P4CanonicalSignature,
                value.P4ResultValidated,
                value.PredicateResult)
        {
        }

        public string MigrationRouteId { get; }
        public string CandidateMigrationRequirementId { get; }
        public string OwnerId { get; }
        public string RequirementGroupId { get; }
        public string SeedId { get; }
        public string EncounterId { get; }
        public string MapRuleId { get; }
        public string PressureInputId { get; }
        public EnemyRequirementChannel DeclaredChannel { get; }
        public EnemyRequirementChannel EvaluationChannel { get; }
        public EnemyRequirementApplicabilityState ApplicabilityState { get; }
        public bool CoordinateBaselineAccepted { get; }
        public bool Activated { get; }
        public RealLayoutResilienceEvaluationPipelineStatus Status { get; }
        public bool P2ResultPresent { get; }
        public AuthoredLayoutPressureSourceStatus? P2Status { get; }
        public LayoutResilienceInputCompleteness? P2Completeness { get; }
        public string P2CanonicalSignature { get; }
        public bool P3ResultPresent { get; }
        public LayoutResilienceEvaluationInputAssemblerStatus? P3Status { get; }
        public string P3CanonicalSignature { get; }
        public bool P4ResultPresent { get; }
        public LayoutResilienceStructuralReadinessConsumerStatus? P4Status { get; }
        public string P4CanonicalSignature { get; }
        public bool P4ResultValidated { get; }
        public LayoutResiliencePredicateResultSnapshot PredicateResult { get; }

        internal RealLayoutResilienceEvaluationRouteRowSnapshot Clone()
        {
            return new RealLayoutResilienceEvaluationRouteRowSnapshot(this);
        }

        private static LayoutResiliencePredicateResultSnapshot CopyPredicate(
            LayoutResiliencePredicateResultSnapshot value)
        {
            return value == null
                ? null
                : new LayoutResiliencePredicateResultSnapshot(
                    value.SchemaId,
                    value.SchemaVersion,
                    value.EvaluationId,
                    value.ApplicabilityState,
                    value.PredicateState,
                    value.ClauseRows,
                    value.CanonicalSignature);
        }
    }

    public sealed class RealLayoutResilienceEvaluationPipelineResult
    {
        private readonly ReadOnlyCollection<
            RealLayoutResilienceEvaluationRouteRowSnapshot> rows;
        private readonly ReadOnlyCollection<
            RealLayoutResilienceEvaluationPipelineIssue> issues;

        internal RealLayoutResilienceEvaluationPipelineResult(
            RealLayoutResilienceEvaluationPipelineStatus status,
            string evaluationBatchId,
            bool buildProjectionResultPresent,
            LayoutResilienceItemFactProjectionStatus? buildProjectionStatus,
            LayoutResilienceInputCompleteness? buildFactsCompleteness,
            string buildProjectionCanonicalSignature,
            bool overlayResultPresent,
            string overlayCanonicalSignature,
            IEnumerable<RealLayoutResilienceEvaluationRouteRowSnapshot> rows,
            IEnumerable<RealLayoutResilienceEvaluationPipelineIssue> issues,
            string canonicalSignature)
        {
            SchemaId = RealLayoutResilienceEvaluationPipelineSchema.SchemaId;
            SchemaVersion = RealLayoutResilienceEvaluationPipelineSchema
                .SchemaVersion;
            Status = status;
            DevOnly = true;
            IsEnabled = false;
            EvaluationBatchId = evaluationBatchId ?? string.Empty;
            BuildProjectionResultPresent = buildProjectionResultPresent;
            BuildProjectionStatus = buildProjectionStatus;
            BuildFactsCompleteness = buildFactsCompleteness;
            BuildProjectionCanonicalSignature =
                buildProjectionCanonicalSignature ?? string.Empty;
            OverlayResultPresent = overlayResultPresent;
            OverlayCanonicalSignature = overlayCanonicalSignature ?? string.Empty;
            this.rows = rows == null
                ? null
                : Array.AsReadOnly(rows.Select(value =>
                        value == null ? null : value.Clone())
                    .OrderBy(value => value == null
                            ? string.Empty
                            : value.MigrationRouteId,
                        StringComparer.Ordinal)
                    .ToArray());
            this.issues = issues == null
                ? null
                : Array.AsReadOnly(issues.Select(value =>
                        value == null ? null : value.Clone())
                    .OrderBy(value => value == null ? string.Empty : value.Path,
                        StringComparer.Ordinal)
                    .ThenBy(value => value == null ? string.Empty : value.Code,
                        StringComparer.Ordinal)
                    .ThenBy(value => value == null ? string.Empty : value.Message,
                        StringComparer.Ordinal)
                    .ToArray());
            CanonicalSignature = canonicalSignature ?? string.Empty;
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public RealLayoutResilienceEvaluationPipelineStatus Status { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public string EvaluationBatchId { get; }
        public bool BuildProjectionResultPresent { get; }
        public LayoutResilienceItemFactProjectionStatus? BuildProjectionStatus { get; }
        public LayoutResilienceInputCompleteness? BuildFactsCompleteness { get; }
        public string BuildProjectionCanonicalSignature { get; }
        public bool OverlayResultPresent { get; }
        public string OverlayCanonicalSignature { get; }
        public IReadOnlyList<RealLayoutResilienceEvaluationRouteRowSnapshot> Rows =>
            rows;
        public IReadOnlyList<RealLayoutResilienceEvaluationPipelineIssue> Issues =>
            issues;
        public string CanonicalSignature { get; }
    }

    public interface IRealLayoutResilienceEvaluationPipeline
    {
        RealLayoutResilienceEvaluationPipelineResult Evaluate(
            RealLayoutResilienceEvaluationPipelineInput input);
    }

    internal static class RealLayoutResilienceEvaluationPipelineCanonical
    {
        public static string Create(
            RealLayoutResilienceEvaluationPipelineResult result)
        {
            StringBuilder builder = new StringBuilder(16384);
            Field(builder, "result.present", result == null ? null : "true");
            if (result == null)
            {
                return Hash(builder.ToString());
            }

            Field(builder, "schemaId", result.SchemaId);
            Field(builder, "schemaVersion", Integer(result.SchemaVersion));
            Field(builder, "status", Integer((int)result.Status));
            Field(builder, "devOnly", Boolean(result.DevOnly));
            Field(builder, "isEnabled", Boolean(result.IsEnabled));
            Field(builder, "evaluationBatchId", result.EvaluationBatchId);
            Field(builder, "buildProjectionResultPresent",
                Boolean(result.BuildProjectionResultPresent));
            Field(builder, "buildProjectionStatus",
                NullableInteger(result.BuildProjectionStatus.HasValue
                    ? (int?)result.BuildProjectionStatus.Value
                    : null));
            Field(builder, "buildFactsCompleteness",
                NullableInteger(result.BuildFactsCompleteness.HasValue
                    ? (int?)result.BuildFactsCompleteness.Value
                    : null));
            Field(builder, "buildProjectionCanonicalSignature",
                result.BuildProjectionCanonicalSignature);
            Field(builder, "overlayResultPresent",
                Boolean(result.OverlayResultPresent));
            Field(builder, "overlayCanonicalSignature",
                result.OverlayCanonicalSignature);

            Rows(builder, result.Rows);
            Issues(builder, result.Issues);
            return Hash(builder.ToString());
        }

        private static void Rows(
            StringBuilder builder,
            IReadOnlyList<RealLayoutResilienceEvaluationRouteRowSnapshot> values)
        {
            Field(builder, "rows.present", values == null ? "false" : "true");
            if (values == null)
            {
                return;
            }
            RealLayoutResilienceEvaluationRouteRowSnapshot[] rows = values
                .OrderBy(value => value == null
                        ? string.Empty
                        : value.MigrationRouteId,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "rows.count", Integer(rows.Length));
            for (int index = 0; index < rows.Length; index++)
            {
                Row(builder, "rows[" + Integer(index) + "].", rows[index]);
            }
        }

        private static void Row(
            StringBuilder builder,
            string prefix,
            RealLayoutResilienceEvaluationRouteRowSnapshot row)
        {
            Field(builder, prefix + "present", row == null ? "false" : "true");
            if (row == null)
            {
                return;
            }
            Field(builder, prefix + "migrationRouteId", row.MigrationRouteId);
            Field(builder, prefix + "candidateMigrationRequirementId",
                row.CandidateMigrationRequirementId);
            Field(builder, prefix + "ownerId", row.OwnerId);
            Field(builder, prefix + "requirementGroupId", row.RequirementGroupId);
            Field(builder, prefix + "seedId", row.SeedId);
            Field(builder, prefix + "encounterId", row.EncounterId);
            Field(builder, prefix + "mapRuleId", row.MapRuleId);
            Field(builder, prefix + "pressureInputId", row.PressureInputId);
            Field(builder, prefix + "declaredChannel",
                Integer((int)row.DeclaredChannel));
            Field(builder, prefix + "evaluationChannel",
                Integer((int)row.EvaluationChannel));
            Field(builder, prefix + "applicabilityState",
                Integer((int)row.ApplicabilityState));
            Field(builder, prefix + "coordinateBaselineAccepted",
                Boolean(row.CoordinateBaselineAccepted));
            Field(builder, prefix + "activated", Boolean(row.Activated));
            Field(builder, prefix + "status", Integer((int)row.Status));
            Field(builder, prefix + "p2ResultPresent",
                Boolean(row.P2ResultPresent));
            Field(builder, prefix + "p2Status",
                NullableInteger(row.P2Status.HasValue
                    ? (int?)row.P2Status.Value
                    : null));
            Field(builder, prefix + "p2Completeness",
                NullableInteger(row.P2Completeness.HasValue
                    ? (int?)row.P2Completeness.Value
                    : null));
            Field(builder, prefix + "p2CanonicalSignature",
                row.P2CanonicalSignature);
            Field(builder, prefix + "p3ResultPresent",
                Boolean(row.P3ResultPresent));
            Field(builder, prefix + "p3Status",
                NullableInteger(row.P3Status.HasValue
                    ? (int?)row.P3Status.Value
                    : null));
            Field(builder, prefix + "p3CanonicalSignature",
                row.P3CanonicalSignature);
            Field(builder, prefix + "p4ResultPresent",
                Boolean(row.P4ResultPresent));
            Field(builder, prefix + "p4Status",
                NullableInteger(row.P4Status.HasValue
                    ? (int?)row.P4Status.Value
                    : null));
            Field(builder, prefix + "p4CanonicalSignature",
                row.P4CanonicalSignature);
            Field(builder, prefix + "p4ResultValidated",
                Boolean(row.P4ResultValidated));
            Predicate(builder, prefix + "predicate.", row.PredicateResult);
        }

        private static void Predicate(
            StringBuilder builder,
            string prefix,
            LayoutResiliencePredicateResultSnapshot value)
        {
            Field(builder, prefix + "present", value == null ? "false" : "true");
            if (value == null)
            {
                return;
            }
            Field(builder, prefix + "schemaId", value.SchemaId);
            Field(builder, prefix + "schemaVersion", Integer(value.SchemaVersion));
            Field(builder, prefix + "evaluationId", value.EvaluationId);
            Field(builder, prefix + "applicabilityState",
                Integer((int)value.ApplicabilityState));
            Field(builder, prefix + "predicateState",
                Integer((int)value.PredicateState));
            Field(builder, prefix + "canonicalSignature", value.CanonicalSignature);
            Field(builder, prefix + "clauses.present",
                value.ClauseRows == null ? "false" : "true");
            if (value.ClauseRows == null)
            {
                return;
            }
            LayoutResiliencePredicateClauseSnapshot[] clauses = value.ClauseRows
                .OrderBy(row => row == null ? 0 : (int)row.ClauseKind)
                .ToArray();
            Field(builder, prefix + "clauses.count", Integer(clauses.Length));
            for (int index = 0; index < clauses.Length; index++)
            {
                LayoutResiliencePredicateClauseSnapshot clause = clauses[index];
                string valueText = clause == null
                    ? null
                    : Integer((int)clause.ClauseKind) + ":" +
                        Integer((int)clause.ClauseState);
                Field(builder, prefix + "clauses[" + Integer(index) + "]",
                    valueText);
            }
        }

        private static void Issues(
            StringBuilder builder,
            IReadOnlyList<RealLayoutResilienceEvaluationPipelineIssue> values)
        {
            Field(builder, "issues.present", values == null ? "false" : "true");
            if (values == null)
            {
                return;
            }
            RealLayoutResilienceEvaluationPipelineIssue[] issues = values
                .OrderBy(value => value == null ? string.Empty : value.Path,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.Code,
                    StringComparer.Ordinal)
                .ThenBy(value => value == null ? string.Empty : value.Message,
                    StringComparer.Ordinal)
                .ToArray();
            Field(builder, "issues.count", Integer(issues.Length));
            for (int index = 0; index < issues.Length; index++)
            {
                RealLayoutResilienceEvaluationPipelineIssue issue = issues[index];
                string prefix = "issues[" + Integer(index) + "].";
                Field(builder, prefix + "present",
                    issue == null ? "false" : "true");
                if (issue == null)
                {
                    continue;
                }
                Field(builder, prefix + "code", issue.Code);
                Field(builder, prefix + "status", Integer((int)issue.Status));
                Field(builder, prefix + "path", issue.Path);
                Field(builder, prefix + "message", issue.Message);
            }
        }

        private static string Boolean(bool value)
        {
            return value ? "true" : "false";
        }

        private static string NullableInteger(int? value)
        {
            return value.HasValue ? Integer(value.Value) : null;
        }

        private static string Integer(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static void Field(
            StringBuilder builder,
            string name,
            string value)
        {
            string safeName = name ?? string.Empty;
            builder.Append(Integer(safeName.Length)).Append(':').Append(safeName)
                .Append('=');
            if (value == null)
            {
                builder.Append("N;");
                return;
            }
            builder.Append('V').Append(Integer(value.Length)).Append(':')
                .Append(value).Append(';');
        }

        private static string Hash(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return "sha256:" + string.Concat(sha.ComputeHash(
                        Encoding.UTF8.GetBytes(payload ?? string.Empty))
                    .Select(value => value.ToString("x2",
                        CultureInfo.InvariantCulture)));
            }
        }
    }
}
