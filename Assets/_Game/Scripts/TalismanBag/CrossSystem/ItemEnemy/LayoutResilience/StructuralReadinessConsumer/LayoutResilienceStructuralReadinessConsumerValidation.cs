using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EnemyApplicabilityState = TalismanBag.EnemySystem.RequirementChannel.EnemyRequirementApplicabilityState;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.StructuralReadiness
{
    public static class LayoutResilienceStructuralReadinessConsumerValidationCodes
    {
        public const string InputNull = "INPUT_NULL";
        public const string SchemaMismatch = "CONSUMER_SCHEMA_MISMATCH";
        public const string AssemblerResultMissing = "P3_RESULT_MISSING";
        public const string AssemblerResultValidationException =
            "P3_RESULT_VALIDATION_EXCEPTION";
        public const string AssemblerResultRejected = "P3_RESULT_REJECTED";
        public const string AssemblerSourceInvalid = "P3_SOURCE_INVALID";
        public const string AssemblerDispositionInvalid = "P3_DISPOSITION_INVALID";
        public const string EvaluatorException = "N01C_EVALUATOR_EXCEPTION";
        public const string PredicateValidationException =
            "N01C_RESULT_VALIDATION_EXCEPTION";
        public const string PredicateResultRejected = "N01C_RESULT_REJECTED";
        public const string PredicateDispositionMismatch =
            "N01C_DISPOSITION_MISMATCH";
        public const string ResultNull = "RESULT_NULL";
        public const string ResultSchemaInvalid = "RESULT_SCHEMA_INVALID";
        public const string ResultStatusUndefined = "RESULT_STATUS_UNDEFINED";
        public const string ResultShapeInvalid = "RESULT_SHAPE_INVALID";
        public const string ResultPredicateInvalid = "RESULT_PREDICATE_INVALID";
        public const string ResultIssueInvalid = "RESULT_ISSUE_INVALID";
        public const string ResultSignatureInvalid = "RESULT_SIGNATURE_INVALID";
    }

    public sealed class DefaultLayoutResilienceStructuralReadinessConsumerValidator
    {
        public static readonly
            DefaultLayoutResilienceStructuralReadinessConsumerValidator Instance =
                new DefaultLayoutResilienceStructuralReadinessConsumerValidator();

        public IReadOnlyList<LayoutResilienceStructuralReadinessConsumerIssue>
            ValidateResult(LayoutResilienceStructuralReadinessConsumerResult result)
        {
            List<LayoutResilienceStructuralReadinessConsumerIssue> issues =
                new List<LayoutResilienceStructuralReadinessConsumerIssue>();
            if (result == null)
            {
                Add(issues,
                    LayoutResilienceStructuralReadinessConsumerValidationCodes.ResultNull,
                    "$", "Consumer result is required.");
                return Freeze(issues);
            }

            if (!string.Equals(result.SchemaId,
                    LayoutResilienceStructuralReadinessConsumerSchema.SchemaId,
                    StringComparison.Ordinal) ||
                result.SchemaVersion !=
                    LayoutResilienceStructuralReadinessConsumerSchema.SchemaVersion)
            {
                Add(issues,
                    LayoutResilienceStructuralReadinessConsumerValidationCodes
                        .ResultSchemaInvalid,
                    "schema", "Consumer result schema must match exactly.");
            }
            if (!IsStatusDefined(result.Status))
            {
                Add(issues,
                    LayoutResilienceStructuralReadinessConsumerValidationCodes
                        .ResultStatusUndefined,
                    "status", "Consumer status must be a named value.");
            }

            bool shapeValid = false;
            if (result.Status ==
                    LayoutResilienceStructuralReadinessConsumerStatus.Complete &&
                result.PredicateResult != null && result.Issues.Count == 0)
            {
                LayoutResiliencePredicateState state =
                    result.PredicateResult.PredicateState;
                shapeValid = state == LayoutResiliencePredicateState.KnownTrue ||
                    state == LayoutResiliencePredicateState.KnownFalse ||
                    state == LayoutResiliencePredicateState.NotApplicable;
            }
            else if (result.Status ==
                LayoutResilienceStructuralReadinessConsumerStatus.Unknown)
            {
                shapeValid = (result.PredicateResult == null ||
                        result.PredicateResult.PredicateState ==
                            LayoutResiliencePredicateState.Unknown) &&
                    result.Issues.All(value => value != null &&
                        value.Status ==
                            LayoutResilienceStructuralReadinessConsumerStatus.Unknown);
            }
            else if (result.Status ==
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid)
            {
                shapeValid = result.PredicateResult == null &&
                    result.Issues.Any(value => value != null &&
                        value.Status ==
                            LayoutResilienceStructuralReadinessConsumerStatus.Invalid);
            }
            if (!shapeValid)
            {
                Add(issues,
                    LayoutResilienceStructuralReadinessConsumerValidationCodes
                        .ResultShapeInvalid,
                    "result", "Consumer status, payload, and issues are inconsistent.");
            }

            if (result.PredicateResult != null &&
                !PredicateResultShapeValid(result.PredicateResult))
            {
                Add(issues,
                    LayoutResilienceStructuralReadinessConsumerValidationCodes
                        .ResultPredicateInvalid,
                    "predicateResult",
                    "Predicate result must preserve a valid N01C public result shape.");
            }

            if (result.Issues.Any(value => value == null ||
                (value.Status !=
                    LayoutResilienceStructuralReadinessConsumerStatus.Unknown &&
                 value.Status !=
                    LayoutResilienceStructuralReadinessConsumerStatus.Invalid) ||
                string.IsNullOrWhiteSpace(value.Code) ||
                string.IsNullOrWhiteSpace(value.Path)))
            {
                Add(issues,
                    LayoutResilienceStructuralReadinessConsumerValidationCodes
                        .ResultIssueInvalid,
                    "issues", "Issues must be non-null Unknown or Invalid diagnostics.");
            }
            if (!IsSignature(result.CanonicalSignature))
            {
                Add(issues,
                    LayoutResilienceStructuralReadinessConsumerValidationCodes
                        .ResultSignatureInvalid,
                    "canonicalSignature",
                    "Canonical signature must be sha256 plus 64 lowercase hex digits.");
            }
            return Freeze(issues);
        }

        internal static bool IsStatusDefined(
            LayoutResilienceStructuralReadinessConsumerStatus value)
        {
            return value ==
                    LayoutResilienceStructuralReadinessConsumerStatus.Complete ||
                value == LayoutResilienceStructuralReadinessConsumerStatus.Unknown ||
                value == LayoutResilienceStructuralReadinessConsumerStatus.Invalid;
        }

        internal static bool IsSignature(string value)
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

        private static bool PredicateResultShapeValid(
            LayoutResiliencePredicateResultSnapshot value)
        {
            if (value == null ||
                !string.Equals(value.SchemaId,
                    LayoutResilienceStructuralPredicateSchema.SchemaId,
                    StringComparison.Ordinal) ||
                value.SchemaVersion !=
                    LayoutResilienceStructuralPredicateSchema.SchemaVersion ||
                string.IsNullOrWhiteSpace(value.EvaluationId) ||
                !string.Equals(value.EvaluationId, value.EvaluationId.Trim(),
                    StringComparison.Ordinal) ||
                !ApplicabilityDefined(value.ApplicabilityState) ||
                !PredicateStateDefined(value.PredicateState) ||
                !IsSignature(value.CanonicalSignature))
            {
                return false;
            }

            HashSet<LayoutResiliencePredicateClauseKind> seen =
                new HashSet<LayoutResiliencePredicateClauseKind>();
            if (value.ClauseRows.Any(row => row == null ||
                !ClauseKindDefined(row.ClauseKind) ||
                !ClauseStateDefined(row.ClauseState) ||
                !seen.Add(row.ClauseKind)))
            {
                return false;
            }

            bool known = value.PredicateState ==
                    LayoutResiliencePredicateState.KnownTrue ||
                value.PredicateState == LayoutResiliencePredicateState.KnownFalse;
            if (known)
            {
                return value.ApplicabilityState ==
                        EnemyApplicabilityState.Applicable &&
                    value.ClauseRows.Count > 0 &&
                    (value.PredicateState != LayoutResiliencePredicateState.KnownTrue ||
                        value.ClauseRows.All(row => row.ClauseState ==
                            LayoutResiliencePredicateClauseState.Satisfied)) &&
                    (value.PredicateState != LayoutResiliencePredicateState.KnownFalse ||
                        value.ClauseRows.Any(row => row.ClauseState ==
                            LayoutResiliencePredicateClauseState.Violated));
            }
            if (value.ClauseRows.Count != 0)
            {
                return false;
            }
            if (value.PredicateState == LayoutResiliencePredicateState.NotApplicable)
            {
                return value.ApplicabilityState ==
                    EnemyApplicabilityState.NotApplicable;
            }
            return value.PredicateState == LayoutResiliencePredicateState.Unknown &&
                (value.ApplicabilityState == EnemyApplicabilityState.Unknown ||
                 value.ApplicabilityState == EnemyApplicabilityState.Applicable);
        }

        private static bool ApplicabilityDefined(
            EnemyApplicabilityState value)
        {
            return value == EnemyApplicabilityState.Applicable ||
                value == EnemyApplicabilityState.Unknown ||
                value == EnemyApplicabilityState.NotApplicable ||
                value == EnemyApplicabilityState.NotInChannel;
        }

        private static bool PredicateStateDefined(LayoutResiliencePredicateState value)
        {
            return value == LayoutResiliencePredicateState.KnownTrue ||
                value == LayoutResiliencePredicateState.KnownFalse ||
                value == LayoutResiliencePredicateState.Unknown ||
                value == LayoutResiliencePredicateState.NotApplicable;
        }

        private static bool ClauseKindDefined(
            LayoutResiliencePredicateClauseKind value)
        {
            return value == LayoutResiliencePredicateClauseKind.CountedLayoutPresent ||
                value == LayoutResiliencePredicateClauseKind
                    .CountedPlacementCellsUsable ||
                value == LayoutResiliencePredicateClauseKind
                    .CountedPlacementCoresUsable ||
                value == LayoutResiliencePredicateClauseKind
                    .EffectiveEyeAnchorUsable ||
                value == LayoutResiliencePredicateClauseKind
                    .EyeToCountedCoreStructurallyConnected;
        }

        private static bool ClauseStateDefined(
            LayoutResiliencePredicateClauseState value)
        {
            return value == LayoutResiliencePredicateClauseState.Satisfied ||
                value == LayoutResiliencePredicateClauseState.Violated;
        }

        private static ReadOnlyCollection<
            LayoutResilienceStructuralReadinessConsumerIssue> Freeze(
            IEnumerable<LayoutResilienceStructuralReadinessConsumerIssue> values)
        {
            return Array.AsReadOnly((values ??
                    Array.Empty<LayoutResilienceStructuralReadinessConsumerIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray());
        }

        private static void Add(
            ICollection<LayoutResilienceStructuralReadinessConsumerIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new LayoutResilienceStructuralReadinessConsumerIssue(
                code,
                LayoutResilienceStructuralReadinessConsumerStatus.Invalid,
                path,
                message));
        }
    }
}
