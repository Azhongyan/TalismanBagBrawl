using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.RealEvaluationPipeline
{
    public static class RealLayoutResilienceEvaluationPipelineValidationCodes
    {
        public const string InputMissing = "INPUT_MISSING";
        public const string InputSchemaInvalid = "INPUT_SCHEMA_INVALID";
        public const string EvaluationBatchIdMissing =
            "EVALUATION_BATCH_ID_MISSING";
        public const string OverlayAuthorityException =
            "OVERLAY_AUTHORITY_EXCEPTION";
        public const string OverlayResultInvalid = "OVERLAY_RESULT_INVALID";
        public const string BuildProjectionAuthorityException =
            "BUILD_PROJECTION_AUTHORITY_EXCEPTION";
        public const string BuildProjectionResultInvalid =
            "BUILD_PROJECTION_RESULT_INVALID";
        public const string PressureSourceIdentityInvalid =
            "PRESSURE_SOURCE_IDENTITY_INVALID";
        public const string P2AuthorityException = "P2_AUTHORITY_EXCEPTION";
        public const string P2ResultInvalid = "P2_RESULT_INVALID";
        public const string P3AuthorityException = "P3_AUTHORITY_EXCEPTION";
        public const string P3ResultInvalid = "P3_RESULT_INVALID";
        public const string P4AuthorityException = "P4_AUTHORITY_EXCEPTION";
        public const string P4ResultValidatorException =
            "P4_RESULT_VALIDATOR_EXCEPTION";
        public const string P4ResultRejected = "P4_RESULT_REJECTED";
        public const string P4ResultInvalid = "P4_RESULT_INVALID";
        public const string RouteEvaluationUnknown =
            "ROUTE_EVALUATION_UNKNOWN";
        public const string ResultMissing = "RESULT_MISSING";
        public const string ResultSchemaInvalid = "RESULT_SCHEMA_INVALID";
        public const string ResultShapeInvalid = "RESULT_SHAPE_INVALID";
        public const string ResultIssueInvalid = "RESULT_ISSUE_INVALID";
        public const string ResultRouteInvalid = "RESULT_ROUTE_INVALID";
        public const string ResultPredicateInvalid = "RESULT_PREDICATE_INVALID";
        public const string ResultCanonicalInvalid = "RESULT_CANONICAL_INVALID";
    }

    public sealed class DefaultRealLayoutResilienceEvaluationPipelineValidator
    {
        public static readonly
            DefaultRealLayoutResilienceEvaluationPipelineValidator Instance =
                new DefaultRealLayoutResilienceEvaluationPipelineValidator();

        public IReadOnlyList<RealLayoutResilienceEvaluationPipelineIssue>
            ValidateResult(RealLayoutResilienceEvaluationPipelineResult result)
        {
            List<RealLayoutResilienceEvaluationPipelineIssue> issues =
                new List<RealLayoutResilienceEvaluationPipelineIssue>();
            if (result == null)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultMissing,
                    "Result",
                    "Pipeline result is required.");
                return Array.AsReadOnly(issues.ToArray());
            }

            ValidateTopLevel(result, issues);
            ValidateIssues(result, issues);
            ValidateRows(result, issues);
            string expected = RealLayoutResilienceEvaluationPipelineCanonical
                .Create(result);
            if (!RealLayoutResilienceEvaluationPipelineValidationUtility
                    .IsSignature(result.CanonicalSignature) ||
                !string.Equals(result.CanonicalSignature, expected,
                    StringComparison.Ordinal))
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultCanonicalInvalid,
                    "Result.CanonicalSignature",
                    "Pipeline canonical signature is malformed or does not match the DTO payload.");
            }
            return Array.AsReadOnly(issues
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray());
        }

        private static void ValidateTopLevel(
            RealLayoutResilienceEvaluationPipelineResult result,
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> issues)
        {
            if (!string.Equals(result.SchemaId,
                    RealLayoutResilienceEvaluationPipelineSchema.SchemaId,
                    StringComparison.Ordinal) ||
                result.SchemaVersion !=
                    RealLayoutResilienceEvaluationPipelineSchema.SchemaVersion ||
                !result.DevOnly || result.IsEnabled)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultSchemaInvalid,
                    "Result.Schema",
                    "Pipeline result schema and dev-only isolation flags are invalid.");
            }
            bool missingBatchAllowed = result.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid &&
                result.Rows != null && result.Rows.Count == 0;
            if (!Defined(result.Status) ||
                (string.IsNullOrWhiteSpace(result.EvaluationBatchId) &&
                    !missingBatchAllowed))
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultShapeInvalid,
                    "Result.Status",
                    "Pipeline status must be defined and EvaluationBatchId must be non-empty.");
            }
            if (!Presence(
                    result.BuildProjectionResultPresent,
                    result.BuildProjectionStatus.HasValue,
                    result.BuildProjectionCanonicalSignature) ||
                (result.BuildProjectionResultPresent &&
                    (!Defined(result.BuildProjectionStatus.Value) ||
                     !result.BuildFactsCompleteness.HasValue ||
                     !Defined(result.BuildFactsCompleteness.Value))) ||
                (!result.BuildProjectionResultPresent &&
                    result.BuildFactsCompleteness.HasValue))
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultShapeInvalid,
                    "Result.BuildProjection",
                    "Build projection presence, status, completeness, and canonical fields are inconsistent.");
            }
            if (!Presence(
                    result.OverlayResultPresent,
                    result.OverlayResultPresent,
                    result.OverlayCanonicalSignature))
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultShapeInvalid,
                    "Result.Overlay",
                    "Overlay presence and canonical fields are inconsistent.");
            }
        }

        private static void ValidateIssues(
            RealLayoutResilienceEvaluationPipelineResult result,
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> findings)
        {
            if (result.Issues == null)
            {
                Add(findings,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultIssueInvalid,
                    "Result.Issues",
                    "Pipeline issues collection must be non-null.");
                return;
            }
            foreach (RealLayoutResilienceEvaluationPipelineIssue issue in
                result.Issues)
            {
                if (issue == null || string.IsNullOrWhiteSpace(issue.Code) ||
                    string.IsNullOrWhiteSpace(issue.Path) ||
                    string.IsNullOrWhiteSpace(issue.Message) ||
                    issue.Status ==
                        RealLayoutResilienceEvaluationPipelineStatus.Complete ||
                    !Defined(issue.Status))
                {
                    Add(findings,
                        RealLayoutResilienceEvaluationPipelineValidationCodes
                            .ResultIssueInvalid,
                        "Result.Issues",
                        "Each issue must contain non-empty text and a defined Unknown or Invalid status.");
                    break;
                }
            }
            bool anyUnknown = result.Issues.Any(value => value != null &&
                value.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Unknown);
            bool anyInvalid = result.Issues.Any(value => value != null &&
                value.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid);
            bool issueShape = result.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Complete
                ? result.Issues.Count == 0
                : result.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Unknown
                    ? anyUnknown && !anyInvalid
                    : anyInvalid;
            if (!issueShape)
            {
                Add(findings,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultIssueInvalid,
                    "Result.Issues",
                    "Issue severity distribution does not match Pipeline Status.");
            }
        }

        private static void ValidateRows(
            RealLayoutResilienceEvaluationPipelineResult result,
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> issues)
        {
            if (result.Rows == null)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultShapeInvalid,
                    "Result.Rows",
                    "Pipeline rows collection must be non-null.");
                return;
            }
            if (result.Rows.Count != 0 && result.Rows.Count != 4)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultShapeInvalid,
                    "Result.Rows",
                    "Pipeline rows must contain zero pre-gate rows or exactly four route rows.");
                return;
            }
            if (result.Rows.Count == 0)
            {
                if (result.Status !=
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid)
                {
                    Add(issues,
                        RealLayoutResilienceEvaluationPipelineValidationCodes
                            .ResultShapeInvalid,
                        "Result.Rows",
                        "A zero-row result is legal only for an Invalid pre-gate result.");
                }
                return;
            }
            if (!result.BuildProjectionResultPresent ||
                !result.OverlayResultPresent ||
                result.BuildProjectionStatus ==
                    LayoutResilienceItemFactProjectionStatus.Invalid)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultShapeInvalid,
                    "Result.Rows",
                    "Four route rows require present non-Invalid P1 and P5-M results.");
            }

            RealLayoutResilienceEvaluationRouteRowSnapshot[] ordered = result.Rows
                .OrderBy(value => value == null
                        ? string.Empty
                        : value.MigrationRouteId,
                    StringComparer.Ordinal).ToArray();
            RealLayoutResilienceEvaluationRouteIdentity[] expected =
                RealLayoutResilienceEvaluationRouteIdentity.All
                    .OrderBy(value => value.MigrationRouteId,
                        StringComparer.Ordinal).ToArray();
            for (int index = 0; index < ordered.Length; index++)
            {
                RealLayoutResilienceEvaluationRouteRowSnapshot row = ordered[index];
                string path = "Result.Rows[" + index + "]";
                if (row == null || !expected[index].Matches(row))
                {
                    Add(issues,
                        RealLayoutResilienceEvaluationPipelineValidationCodes
                            .ResultRouteInvalid,
                        path,
                        "Route identity must exactly match the frozen P5-M tuple and StructuralPredicate Applicable channel.");
                    continue;
                }
                ValidateRow(result.EvaluationBatchId, row, path, issues);
            }

            RealLayoutResilienceEvaluationPipelineStatus aggregate = ordered
                .Any(value => value != null && value.Status ==
                    RealLayoutResilienceEvaluationPipelineStatus.Invalid)
                    ? RealLayoutResilienceEvaluationPipelineStatus.Invalid
                    : ordered.Any(value => value != null && value.Status ==
                        RealLayoutResilienceEvaluationPipelineStatus.Unknown)
                        ? RealLayoutResilienceEvaluationPipelineStatus.Unknown
                        : RealLayoutResilienceEvaluationPipelineStatus.Complete;
            if (result.Status != aggregate)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultShapeInvalid,
                    "Result.Status",
                    "Pipeline Status must aggregate route states using Invalid > Unknown > Complete.");
            }
        }

        private static void ValidateRow(
            string evaluationBatchId,
            RealLayoutResilienceEvaluationRouteRowSnapshot row,
            string path,
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> issues)
        {
            bool presence = P2Presence(row) && P3Presence(row) && P4Presence(row);
            if (!presence || !Defined(row.Status))
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultRouteInvalid,
                    path,
                    "Route presence, nullable authority fields, or route Status are inconsistent.");
            }
            if (!row.P4ResultValidated && row.PredicateResult != null)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultPredicateInvalid,
                    path + ".PredicateResult",
                    "A non-validated P4 result cannot expose a PredicateResult.");
            }
            if (row.P4ResultValidated && !row.P4ResultPresent)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultRouteInvalid,
                    path + ".P4ResultValidated",
                    "P4ResultValidated requires a present P4 result.");
            }

            switch (row.Status)
            {
                case RealLayoutResilienceEvaluationPipelineStatus.Complete:
                    if (!AllAuthoritiesComplete(row) ||
                        !row.P4ResultValidated ||
                        row.PredicateResult == null ||
                        (row.PredicateResult.PredicateState !=
                            LayoutResiliencePredicateState.KnownTrue &&
                         row.PredicateResult.PredicateState !=
                            LayoutResiliencePredicateState.KnownFalse))
                    {
                        Add(issues,
                            RealLayoutResilienceEvaluationPipelineValidationCodes
                                .ResultRouteInvalid,
                            path,
                            "Complete route requires Complete P2/P3/P4, validated P4, and a KnownTrue or KnownFalse predicate.");
                    }
                    break;
                case RealLayoutResilienceEvaluationPipelineStatus.Unknown:
                    if (!row.P2ResultPresent || !row.P3ResultPresent ||
                        !row.P4ResultPresent || !row.P4ResultValidated ||
                        row.P4Status !=
                            LayoutResilienceStructuralReadinessConsumerStatus.Unknown ||
                        (row.PredicateResult != null &&
                         row.PredicateResult.PredicateState !=
                            LayoutResiliencePredicateState.Unknown))
                    {
                        Add(issues,
                            RealLayoutResilienceEvaluationPipelineValidationCodes
                                .ResultRouteInvalid,
                            path,
                            "Unknown route requires present authorities, validated Unknown P4, and only a null or Unknown predicate.");
                    }
                    break;
                case RealLayoutResilienceEvaluationPipelineStatus.Invalid:
                    if (row.PredicateResult != null)
                    {
                        Add(issues,
                            RealLayoutResilienceEvaluationPipelineValidationCodes
                                .ResultRouteInvalid,
                            path,
                            "Invalid route must expose an authority failure or rejected result and no predicate.");
                    }
                    break;
            }

            ValidatePredicate(evaluationBatchId, row, path, issues);
        }

        private static void ValidatePredicate(
            string evaluationBatchId,
            RealLayoutResilienceEvaluationRouteRowSnapshot row,
            string path,
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> issues)
        {
            LayoutResiliencePredicateResultSnapshot predicate =
                row.PredicateResult;
            if (predicate == null)
            {
                return;
            }
            bool valid = string.Equals(predicate.SchemaId,
                    LayoutResilienceStructuralPredicateSchema.SchemaId,
                    StringComparison.Ordinal) &&
                predicate.SchemaVersion ==
                    LayoutResilienceStructuralPredicateSchema.SchemaVersion &&
                string.Equals(predicate.EvaluationId,
                    evaluationBatchId + "|" + row.MigrationRouteId,
                    StringComparison.Ordinal) &&
                predicate.ApplicabilityState ==
                    EnemyRequirementApplicabilityState.Applicable &&
                Enum.IsDefined(typeof(LayoutResiliencePredicateState),
                    predicate.PredicateState) &&
                predicate.PredicateState !=
                    LayoutResiliencePredicateState.NotApplicable &&
                predicate.ClauseRows != null &&
                predicate.ClauseRows.All(value => value != null &&
                    Enum.IsDefined(
                        typeof(LayoutResiliencePredicateClauseKind),
                        value.ClauseKind) &&
                    Enum.IsDefined(
                        typeof(LayoutResiliencePredicateClauseState),
                        value.ClauseState)) &&
                RealLayoutResilienceEvaluationPipelineValidationUtility
                    .IsSignature(predicate.CanonicalSignature);
            if (!valid)
            {
                Add(issues,
                    RealLayoutResilienceEvaluationPipelineValidationCodes
                        .ResultPredicateInvalid,
                    path + ".PredicateResult",
                    "Predicate must preserve the applicable N01C schema, evaluation identity, defined state, clauses, and canonical signature.");
            }
        }

        private static bool P2Presence(
            RealLayoutResilienceEvaluationRouteRowSnapshot row)
        {
            return Presence(row.P2ResultPresent, row.P2Status.HasValue,
                    row.P2CanonicalSignature) &&
                (row.P2ResultPresent
                    ? row.P2Completeness.HasValue &&
                        Defined(row.P2Status.Value) &&
                        Defined(row.P2Completeness.Value)
                    : !row.P2Completeness.HasValue);
        }

        private static bool P3Presence(
            RealLayoutResilienceEvaluationRouteRowSnapshot row)
        {
            return Presence(row.P3ResultPresent, row.P3Status.HasValue,
                    row.P3CanonicalSignature) &&
                (!row.P3ResultPresent || Defined(row.P3Status.Value));
        }

        private static bool P4Presence(
            RealLayoutResilienceEvaluationRouteRowSnapshot row)
        {
            return Presence(row.P4ResultPresent, row.P4Status.HasValue,
                    row.P4CanonicalSignature) &&
                (!row.P4ResultPresent || Defined(row.P4Status.Value));
        }

        private static bool Presence(
            bool present,
            bool statusPresent,
            string signature)
        {
            return present
                ? statusPresent &&
                    RealLayoutResilienceEvaluationPipelineValidationUtility
                        .IsSignature(signature)
                : !statusPresent && string.IsNullOrEmpty(signature);
        }

        private static bool AllAuthoritiesComplete(
            RealLayoutResilienceEvaluationRouteRowSnapshot row)
        {
            return row.P2ResultPresent && row.P3ResultPresent &&
                row.P4ResultPresent &&
                row.P2Status == AuthoredLayoutPressureSourceStatus.Complete &&
                row.P2Completeness == LayoutResilienceInputCompleteness.Complete &&
                row.P3Status ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Complete &&
                row.P4Status ==
                    LayoutResilienceStructuralReadinessConsumerStatus.Complete;
        }

        private static bool HasAuthorityFailure(
            RealLayoutResilienceEvaluationRouteRowSnapshot row)
        {
            return !row.P2ResultPresent || !row.P3ResultPresent ||
                !row.P4ResultPresent || !row.P4ResultValidated ||
                row.P2Status == AuthoredLayoutPressureSourceStatus.Invalid ||
                row.P3Status ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid ||
                row.P4Status ==
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid;
        }

        private static bool Defined(
            RealLayoutResilienceEvaluationPipelineStatus value)
        {
            return Enum.IsDefined(
                typeof(RealLayoutResilienceEvaluationPipelineStatus), value);
        }

        private static bool Defined(
            LayoutResilienceItemFactProjectionStatus value)
        {
            return Enum.IsDefined(
                typeof(LayoutResilienceItemFactProjectionStatus), value);
        }

        private static bool Defined(LayoutResilienceInputCompleteness value)
        {
            return Enum.IsDefined(typeof(LayoutResilienceInputCompleteness), value);
        }

        private static bool Defined(AuthoredLayoutPressureSourceStatus value)
        {
            return Enum.IsDefined(typeof(AuthoredLayoutPressureSourceStatus), value);
        }

        private static bool Defined(
            LayoutResilienceEvaluationInputAssemblerStatus value)
        {
            return Enum.IsDefined(
                typeof(LayoutResilienceEvaluationInputAssemblerStatus), value);
        }

        private static bool Defined(
            LayoutResilienceStructuralReadinessConsumerStatus value)
        {
            return Enum.IsDefined(
                typeof(LayoutResilienceStructuralReadinessConsumerStatus), value);
        }

        private static void Add(
            ICollection<RealLayoutResilienceEvaluationPipelineIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new RealLayoutResilienceEvaluationPipelineIssue(
                code,
                RealLayoutResilienceEvaluationPipelineStatus.Invalid,
                path,
                message));
        }
    }

    internal static class
        RealLayoutResilienceEvaluationPipelineValidationUtility
    {
        public static bool IsSignature(string value)
        {
            if (value == null || value.Length != 71 ||
                !value.StartsWith("sha256:", StringComparison.Ordinal))
            {
                return false;
            }
            for (int index = 7; index < value.Length; index++)
            {
                char character = value[index];
                if (!((character >= '0' && character <= '9') ||
                    (character >= 'a' && character <= 'f')))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
