using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly
{
    public static class LayoutResilienceEvaluationInputAssemblerValidationCodes
    {
        public const string InputNull = "INPUT_NULL";
        public const string SchemaMismatch = "ASSEMBLER_SCHEMA_MISMATCH";
        public const string EvaluationIdInvalid = "EVALUATION_ID_INVALID";
        public const string RequirementIdInvalid = "REQUIREMENT_ID_INVALID";
        public const string ApplicabilitySourceMissing = "APPLICABILITY_SOURCE_MISSING";
        public const string ApplicabilitySourceInvalid = "APPLICABILITY_SOURCE_INVALID";
        public const string RequirementJoinMissing = "REQUIREMENT_JOIN_MISSING";
        public const string RequirementJoinDuplicate = "REQUIREMENT_JOIN_DUPLICATE";
        public const string NotInChannelRejected = "NOT_IN_CHANNEL_REJECTED";
        public const string BuildSourceMissing = "BUILD_SOURCE_MISSING";
        public const string BuildSourceUnknown = "BUILD_SOURCE_UNKNOWN";
        public const string BuildSourceInvalid = "BUILD_SOURCE_INVALID";
        public const string BuildSourceShapeInvalid = "BUILD_SOURCE_SHAPE_INVALID";
        public const string PressureSourceMissing = "PRESSURE_SOURCE_MISSING";
        public const string PressureSourceUnknown = "PRESSURE_SOURCE_UNKNOWN";
        public const string PressureSourceInvalid = "PRESSURE_SOURCE_INVALID";
        public const string PressureSourceShapeInvalid = "PRESSURE_SOURCE_SHAPE_INVALID";
        public const string N01CValidatorRejected = "N01C_VALIDATOR_REJECTED";
        public const string ResultSchemaInvalid = "RESULT_SCHEMA_INVALID";
        public const string ResultStatusUndefined = "RESULT_STATUS_UNDEFINED";
        public const string ResultShapeInvalid = "RESULT_SHAPE_INVALID";
        public const string ResultIssueInvalid = "RESULT_ISSUE_INVALID";
        public const string ResultSignatureInvalid = "RESULT_SIGNATURE_INVALID";
    }

    public sealed class DefaultLayoutResilienceEvaluationInputAssemblerValidator
    {
        public static readonly DefaultLayoutResilienceEvaluationInputAssemblerValidator
            Instance = new DefaultLayoutResilienceEvaluationInputAssemblerValidator();

        public IReadOnlyList<LayoutResilienceEvaluationInputAssemblerIssue> ValidateResult(
            LayoutResilienceEvaluationInputAssemblerResult result)
        {
            List<LayoutResilienceEvaluationInputAssemblerIssue> issues =
                new List<LayoutResilienceEvaluationInputAssemblerIssue>();
            if (result == null)
            {
                Add(issues, LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ResultShapeInvalid,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    "$", "Assembler result is required.");
                return Freeze(issues);
            }

            if (!string.Equals(result.SchemaId,
                    LayoutResilienceEvaluationInputAssemblerSchema.SchemaId,
                    StringComparison.Ordinal) ||
                result.SchemaVersion !=
                    LayoutResilienceEvaluationInputAssemblerSchema.SchemaVersion)
            {
                Add(issues, LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ResultSchemaInvalid,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    "schema", "Result schema must match exactly.");
            }
            if (!IsStatusDefined(result.Status))
            {
                Add(issues, LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ResultStatusUndefined,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    "status", "Result status must be a named value.");
            }

            bool shapeValid =
                (result.Status == LayoutResilienceEvaluationInputAssemblerStatus.Complete &&
                 result.EvaluationInput != null && result.Issues.All(issue =>
                     issue != null && issue.Status !=
                        LayoutResilienceEvaluationInputAssemblerStatus.Invalid)) ||
                (result.Status == LayoutResilienceEvaluationInputAssemblerStatus.Unknown &&
                 result.Issues.All(issue => issue != null && issue.Status ==
                     LayoutResilienceEvaluationInputAssemblerStatus.Unknown)) ||
                (result.Status == LayoutResilienceEvaluationInputAssemblerStatus.Invalid &&
                 result.EvaluationInput == null && result.Issues.Any(issue =>
                     issue != null && issue.Status ==
                        LayoutResilienceEvaluationInputAssemblerStatus.Invalid));
            if (!shapeValid)
            {
                Add(issues, LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ResultShapeInvalid,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    "result", "Status, payload, and issue shape is inconsistent.");
            }

            if (result.Issues.Any(issue => issue == null ||
                !IsStatusDefined(issue.Status) ||
                issue.Status == LayoutResilienceEvaluationInputAssemblerStatus.Complete ||
                string.IsNullOrWhiteSpace(issue.Code) ||
                string.IsNullOrWhiteSpace(issue.Path)))
            {
                Add(issues, LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ResultIssueInvalid,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    "issues", "Issues must be non-null Unknown or Invalid diagnostics.");
            }
            if (!IsSignature(result.CanonicalSignature))
            {
                Add(issues, LayoutResilienceEvaluationInputAssemblerValidationCodes
                    .ResultSignatureInvalid,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    "canonicalSignature",
                    "Canonical signature must be sha256 plus 64 lowercase hex digits.");
            }
            return Freeze(issues);
        }

        internal static bool IsStatusDefined(
            LayoutResilienceEvaluationInputAssemblerStatus value)
        {
            return value == LayoutResilienceEvaluationInputAssemblerStatus.Complete ||
                value == LayoutResilienceEvaluationInputAssemblerStatus.Unknown ||
                value == LayoutResilienceEvaluationInputAssemblerStatus.Invalid;
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

        internal static bool IdentityValid(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                string.Equals(value, value.Trim(), StringComparison.Ordinal);
        }

        internal static ReadOnlyCollection<LayoutResilienceEvaluationInputAssemblerIssue>
            Freeze(IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> values)
        {
            return Array.AsReadOnly((values ??
                    Array.Empty<LayoutResilienceEvaluationInputAssemblerIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray());
        }

        internal static void Add(
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues,
            string code,
            LayoutResilienceEvaluationInputAssemblerStatus status,
            string path,
            string message)
        {
            issues.Add(new LayoutResilienceEvaluationInputAssemblerIssue(
                code, status, path, message));
        }
    }
}
