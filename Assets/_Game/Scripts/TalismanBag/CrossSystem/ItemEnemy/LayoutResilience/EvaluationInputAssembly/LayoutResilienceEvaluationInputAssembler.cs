using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.AuthoredPressure;
using TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.ItemFactProjection;
using TalismanBag.EnemySystem.RequirementChannel;

namespace TalismanBag.CrossSystem.ItemEnemy.LayoutResilience.EvaluationInputAssembly
{
    public sealed class DefaultLayoutResilienceEvaluationInputAssembler :
        ILayoutResilienceEvaluationInputAssembler
    {
        public static readonly DefaultLayoutResilienceEvaluationInputAssembler Instance =
            new DefaultLayoutResilienceEvaluationInputAssembler();

        public LayoutResilienceEvaluationInputAssemblerResult Assemble(
            LayoutResilienceEvaluationInputAssemblerInput input)
        {
            List<LayoutResilienceEvaluationInputAssemblerIssue> issues =
                new List<LayoutResilienceEvaluationInputAssemblerIssue>();
            EnemyRequirementChannelApplicabilityRowSnapshot selectedRow = null;

            if (input == null)
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes.InputNull,
                    "$", "Assembler input is required.");
                return Finish(input, null,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    null, issues);
            }

            ValidateAssemblerInput(input, issues);
            ValidateApplicabilitySource(input.ApplicabilitySnapshot, issues);
            BuildDisposition build = InspectBuild(input.BuildProjectionResult, issues);
            PressureDisposition pressure = InspectPressure(
                input.PressureSourceResult, issues);

            if (input.ApplicabilitySnapshot != null &&
                DefaultLayoutResilienceEvaluationInputAssemblerValidator.IdentityValid(
                    input.RequirementId))
            {
                EnemyRequirementChannelApplicabilityRowSnapshot[] matches = input
                    .ApplicabilitySnapshot.Rows.Where(row => row != null &&
                        string.Equals(row.RequirementId, input.RequirementId,
                            StringComparison.Ordinal)).ToArray();
                if (matches.Length == 0)
                {
                    Invalid(issues,
                        LayoutResilienceEvaluationInputAssemblerValidationCodes
                            .RequirementJoinMissing,
                        "requirementId",
                        "RequirementId must match exactly one N01B row.");
                }
                else if (matches.Length > 1)
                {
                    Invalid(issues,
                        LayoutResilienceEvaluationInputAssemblerValidationCodes
                            .RequirementJoinDuplicate,
                        "requirementId",
                        "RequirementId matched more than one N01B row.");
                }
                else
                {
                    selectedRow = matches[0];
                    if (selectedRow.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.NotInChannel)
                    {
                        Invalid(issues,
                            LayoutResilienceEvaluationInputAssemblerValidationCodes
                                .NotInChannelRejected,
                            "applicabilitySnapshot.selectedRow.applicabilityState",
                            "NotInChannel cannot form a structural evaluation input.");
                    }
                }
            }

            if (HasInvalid(issues))
            {
                return Finish(input, selectedRow,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    null, issues);
            }

            if (input.ApplicabilitySnapshot == null)
            {
                Unknown(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .ApplicabilitySourceMissing,
                    "applicabilitySnapshot",
                    "Missing N01B facts cannot be synthesized as applicability Unknown.");
                return Finish(input, null,
                    LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                    null, issues);
            }

            LayoutResilienceEvaluationInput candidate;
            LayoutResilienceEvaluationInputAssemblerStatus status;
            if (selectedRow.ApplicabilityState ==
                EnemyRequirementApplicabilityState.NotApplicable)
            {
                candidate = new LayoutResilienceEvaluationInput(
                    input.EvaluationId,
                    EnemyRequirementApplicabilityState.NotApplicable,
                    LayoutResilienceInputCompleteness.NotRequired,
                    LayoutResilienceInputCompleteness.NotRequired,
                    null,
                    null);
                status = LayoutResilienceEvaluationInputAssemblerStatus.Complete;
                RemoveSourceUnknowns(issues);
            }
            else
            {
                LayoutResilienceInputCompleteness buildCompleteness = build.IsComplete
                    ? LayoutResilienceInputCompleteness.Complete
                    : LayoutResilienceInputCompleteness.Incomplete;
                LayoutResilienceInputCompleteness pressureCompleteness =
                    pressure.IsComplete
                        ? LayoutResilienceInputCompleteness.Complete
                        : LayoutResilienceInputCompleteness.Incomplete;
                candidate = new LayoutResilienceEvaluationInput(
                    input.EvaluationId,
                    selectedRow.ApplicabilityState,
                    buildCompleteness,
                    pressureCompleteness,
                    build.Payload,
                    pressure.Payload);
                bool complete = selectedRow.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.Applicable &&
                    build.IsComplete && pressure.IsComplete;
                status = complete
                    ? LayoutResilienceEvaluationInputAssemblerStatus.Complete
                    : LayoutResilienceEvaluationInputAssemblerStatus.Unknown;
            }

            IReadOnlyList<LayoutResilienceValidationIssue> validationIssues =
                DefaultLayoutResilienceStructuralPredicateValidator.Instance
                    .Validate(candidate);
            foreach (LayoutResilienceValidationIssue issue in validationIssues)
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .N01CValidatorRejected,
                    "n01c." + issue.Path,
                    issue.Code + ": " + issue.Message);
            }
            if (validationIssues.Count > 0)
            {
                return Finish(input, selectedRow,
                    LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                    null, issues);
            }

            return Finish(input, selectedRow, status, candidate, issues);
        }

        private static void ValidateAssemblerInput(
            LayoutResilienceEvaluationInputAssemblerInput input,
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            if (!string.Equals(input.SchemaId,
                    LayoutResilienceEvaluationInputAssemblerSchema.SchemaId,
                    StringComparison.Ordinal) ||
                input.SchemaVersion !=
                    LayoutResilienceEvaluationInputAssemblerSchema.SchemaVersion)
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes.SchemaMismatch,
                    "schema", "Assembler schema identity and version must match exactly.");
            }
            if (!DefaultLayoutResilienceEvaluationInputAssemblerValidator.IdentityValid(
                input.EvaluationId))
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .EvaluationIdInvalid,
                    "evaluationId", "EvaluationId must be non-empty and untrimmed.");
            }
            if (!DefaultLayoutResilienceEvaluationInputAssemblerValidator.IdentityValid(
                input.RequirementId))
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .RequirementIdInvalid,
                    "requirementId", "RequirementId must be non-empty and untrimmed.");
            }
        }

        private static void ValidateApplicabilitySource(
            EnemyRequirementChannelApplicabilitySnapshot value,
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            if (value == null)
            {
                return;
            }
            bool invalid = !string.Equals(value.SchemaId,
                    EnemyRequirementChannelApplicabilitySchema.SchemaId,
                    StringComparison.Ordinal) ||
                value.SchemaVersion !=
                    EnemyRequirementChannelApplicabilitySchema.SchemaVersion ||
                !DefaultLayoutResilienceEvaluationInputAssemblerValidator.IdentityValid(
                    value.SnapshotId) ||
                value.EvaluationChannel != EnemyRequirementChannel.StructuralPredicate ||
                !DefaultLayoutResilienceEvaluationInputAssemblerValidator.IsSignature(
                    value.CanonicalSignature);
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (EnemyRequirementChannelApplicabilityRowSnapshot row in value.Rows)
            {
                if (row == null ||
                    !DefaultLayoutResilienceEvaluationInputAssemblerValidator.IdentityValid(
                        row.RequirementId) ||
                    !ids.Add(row.RequirementId) ||
                    !ChannelDefined(row.DeclaredChannel) ||
                    !ApplicabilityDefined(row.ApplicabilityState))
                {
                    invalid = true;
                    continue;
                }
                bool same = row.DeclaredChannel == value.EvaluationChannel;
                if ((same && row.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.NotInChannel) ||
                    (!same && row.ApplicabilityState !=
                        EnemyRequirementApplicabilityState.NotInChannel))
                {
                    invalid = true;
                }
            }
            if (invalid)
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .ApplicabilitySourceInvalid,
                    "applicabilitySnapshot",
                    "N01B schema, channel, rows, identity, or signature is invalid.");
            }
        }

        private static BuildDisposition InspectBuild(
            LayoutResilienceItemFactProjectionResult value,
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            if (value == null)
            {
                Unknown(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .BuildSourceMissing,
                    "buildProjectionResult", "P1 result is missing.");
                return BuildDisposition.Missing;
            }
            bool statusDefined = value.Status ==
                    LayoutResilienceItemFactProjectionStatus.Complete ||
                value.Status == LayoutResilienceItemFactProjectionStatus.Unknown ||
                value.Status == LayoutResilienceItemFactProjectionStatus.Invalid;
            bool completenessDefined = CompletenessDefined(value.BuildFactsCompleteness);
            bool issueShape = value.Issues.All(issue => issue != null &&
                (issue.Status == LayoutResilienceItemFactProjectionStatus.Unknown ||
                 issue.Status == LayoutResilienceItemFactProjectionStatus.Invalid));
            bool shape =
                (value.Status == LayoutResilienceItemFactProjectionStatus.Complete &&
                 value.BuildFactsCompleteness == LayoutResilienceInputCompleteness.Complete &&
                 value.BuildFacts != null && value.Issues.Count == 0) ||
                (value.Status == LayoutResilienceItemFactProjectionStatus.Unknown &&
                 value.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete) ||
                (value.Status == LayoutResilienceItemFactProjectionStatus.Invalid &&
                 value.BuildFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete &&
                 value.BuildFacts == null);
            if (!string.Equals(value.SchemaId,
                    LayoutResilienceItemFactProjectionSchema.SchemaId,
                    StringComparison.Ordinal) ||
                value.SchemaVersion != LayoutResilienceItemFactProjectionSchema.SchemaVersion ||
                !statusDefined || !completenessDefined || !issueShape || !shape ||
                !DefaultLayoutResilienceEvaluationInputAssemblerValidator.IsSignature(
                    value.CanonicalSignature))
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .BuildSourceShapeInvalid,
                    "buildProjectionResult",
                    "P1 public result shape or signature is invalid.");
                return BuildDisposition.Invalid;
            }
            foreach (LayoutResilienceItemFactProjectionIssue issue in value.Issues)
            {
                AddUpstream(issues, "p1." + issue.Code,
                    issue.Status == LayoutResilienceItemFactProjectionStatus.Invalid,
                    "buildProjectionResult.issues." + issue.Code, issue.Message);
            }
            if (value.Status == LayoutResilienceItemFactProjectionStatus.Invalid)
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .BuildSourceInvalid,
                    "buildProjectionResult.status", "P1 reported Invalid.");
                return BuildDisposition.Invalid;
            }
            if (value.Status == LayoutResilienceItemFactProjectionStatus.Unknown)
            {
                if (value.Issues.Count == 0)
                {
                    Unknown(issues,
                        LayoutResilienceEvaluationInputAssemblerValidationCodes
                            .BuildSourceUnknown,
                        "buildProjectionResult.status", "P1 reported Unknown.");
                }
                return new BuildDisposition(false, value.BuildFacts);
            }
            return new BuildDisposition(true, value.BuildFacts);
        }

        private static PressureDisposition InspectPressure(
            AuthoredLayoutPressureSourceResult value,
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            if (value == null)
            {
                Unknown(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .PressureSourceMissing,
                    "pressureSourceResult", "P2 result is missing.");
                return PressureDisposition.Missing;
            }
            bool statusDefined = value.Status == AuthoredLayoutPressureSourceStatus.Complete ||
                value.Status == AuthoredLayoutPressureSourceStatus.Unknown ||
                value.Status == AuthoredLayoutPressureSourceStatus.Invalid;
            bool issueShape = value.Issues.All(issue => issue != null &&
                (issue.Status == AuthoredLayoutPressureSourceStatus.Unknown ||
                 issue.Status == AuthoredLayoutPressureSourceStatus.Invalid));
            bool shape =
                (value.Status == AuthoredLayoutPressureSourceStatus.Complete &&
                 value.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Complete &&
                 value.PressureSnapshot != null && value.Issues.Count == 0) ||
                (value.Status == AuthoredLayoutPressureSourceStatus.Unknown &&
                 value.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete &&
                 value.PressureSnapshot == null) ||
                (value.Status == AuthoredLayoutPressureSourceStatus.Invalid &&
                 value.PressureFactsCompleteness ==
                    LayoutResilienceInputCompleteness.Incomplete &&
                 value.PressureSnapshot == null);
            if (!string.Equals(value.SchemaId, AuthoredLayoutPressureSourceSchema.SchemaId,
                    StringComparison.Ordinal) ||
                value.SchemaVersion != AuthoredLayoutPressureSourceSchema.SchemaVersion ||
                !statusDefined || !CompletenessDefined(value.PressureFactsCompleteness) ||
                !issueShape || !shape ||
                !DefaultLayoutResilienceEvaluationInputAssemblerValidator.IsSignature(
                    value.CanonicalSignature))
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .PressureSourceShapeInvalid,
                    "pressureSourceResult",
                    "P2 public result shape or signature is invalid.");
                return PressureDisposition.Invalid;
            }
            foreach (AuthoredLayoutPressureSourceIssue issue in value.Issues)
            {
                AddUpstream(issues, "p2." + issue.Code,
                    issue.Status == AuthoredLayoutPressureSourceStatus.Invalid,
                    "pressureSourceResult.issues." + issue.Path, issue.Message);
            }
            if (value.Status == AuthoredLayoutPressureSourceStatus.Invalid)
            {
                Invalid(issues,
                    LayoutResilienceEvaluationInputAssemblerValidationCodes
                        .PressureSourceInvalid,
                    "pressureSourceResult.status", "P2 reported Invalid.");
                return PressureDisposition.Invalid;
            }
            if (value.Status == AuthoredLayoutPressureSourceStatus.Unknown)
            {
                if (value.Issues.Count == 0)
                {
                    Unknown(issues,
                        LayoutResilienceEvaluationInputAssemblerValidationCodes
                            .PressureSourceUnknown,
                        "pressureSourceResult.status", "P2 reported Unknown.");
                }
                return PressureDisposition.Missing;
            }
            return new PressureDisposition(true, value.PressureSnapshot);
        }

        private static LayoutResilienceEvaluationInputAssemblerResult Finish(
            LayoutResilienceEvaluationInputAssemblerInput source,
            EnemyRequirementChannelApplicabilityRowSnapshot selectedRow,
            LayoutResilienceEvaluationInputAssemblerStatus status,
            LayoutResilienceEvaluationInput output,
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            LayoutResilienceEvaluationInputAssemblerIssue[] ordered = (issues ??
                    Array.Empty<LayoutResilienceEvaluationInputAssemblerIssue>())
                .Where(value => value != null)
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ThenBy(value => value.Message, StringComparer.Ordinal)
                .ToArray();
            string signature = LayoutResilienceEvaluationInputAssemblerCanonical.Create(
                source, selectedRow, status, output, ordered);
            LayoutResilienceEvaluationInputAssemblerResult result =
                new LayoutResilienceEvaluationInputAssemblerResult(
                    status, output, ordered, signature);
            if (DefaultLayoutResilienceEvaluationInputAssemblerValidator.Instance
                    .ValidateResult(result).Count != 0)
            {
                throw new InvalidOperationException(
                    "Internal evaluation input assembler result validation failed.");
            }
            return result;
        }

        private static void RemoveSourceUnknowns(
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            List<LayoutResilienceEvaluationInputAssemblerIssue> list = issues as
                List<LayoutResilienceEvaluationInputAssemblerIssue>;
            if (list != null)
            {
                list.RemoveAll(value => value.Status ==
                    LayoutResilienceEvaluationInputAssemblerStatus.Unknown &&
                    (value.Path.StartsWith("buildProjectionResult",
                         StringComparison.Ordinal) ||
                     value.Path.StartsWith("pressureSourceResult",
                         StringComparison.Ordinal)));
            }
        }

        private static bool HasInvalid(
            IEnumerable<LayoutResilienceEvaluationInputAssemblerIssue> issues)
        {
            return issues.Any(value => value.Status ==
                LayoutResilienceEvaluationInputAssemblerStatus.Invalid);
        }

        private static bool ChannelDefined(EnemyRequirementChannel value)
        {
            return value == EnemyRequirementChannel.ContinuousBP ||
                value == EnemyRequirementChannel.StructuralPredicate ||
                value == EnemyRequirementChannel.RuntimeEventSignal;
        }

        private static bool ApplicabilityDefined(
            EnemyRequirementApplicabilityState value)
        {
            return value == EnemyRequirementApplicabilityState.Applicable ||
                value == EnemyRequirementApplicabilityState.Unknown ||
                value == EnemyRequirementApplicabilityState.NotApplicable ||
                value == EnemyRequirementApplicabilityState.NotInChannel;
        }

        private static bool CompletenessDefined(LayoutResilienceInputCompleteness value)
        {
            return value == LayoutResilienceInputCompleteness.Complete ||
                value == LayoutResilienceInputCompleteness.Incomplete ||
                value == LayoutResilienceInputCompleteness.NotRequired;
        }

        private static void AddUpstream(
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues,
            string code,
            bool invalid,
            string path,
            string message)
        {
            DefaultLayoutResilienceEvaluationInputAssemblerValidator.Add(
                issues, code,
                invalid
                    ? LayoutResilienceEvaluationInputAssemblerStatus.Invalid
                    : LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                path, message);
        }

        private static void Unknown(
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues,
            string code,
            string path,
            string message)
        {
            DefaultLayoutResilienceEvaluationInputAssemblerValidator.Add(
                issues, code, LayoutResilienceEvaluationInputAssemblerStatus.Unknown,
                path, message);
        }

        private static void Invalid(
            ICollection<LayoutResilienceEvaluationInputAssemblerIssue> issues,
            string code,
            string path,
            string message)
        {
            DefaultLayoutResilienceEvaluationInputAssemblerValidator.Add(
                issues, code, LayoutResilienceEvaluationInputAssemblerStatus.Invalid,
                path, message);
        }

        private sealed class BuildDisposition
        {
            public static readonly BuildDisposition Missing =
                new BuildDisposition(false, null);
            public static readonly BuildDisposition Invalid =
                new BuildDisposition(false, null);

            public BuildDisposition(bool complete, LayoutResilienceBuildFactSnapshot payload)
            {
                IsComplete = complete;
                Payload = payload;
            }

            public bool IsComplete { get; }
            public LayoutResilienceBuildFactSnapshot Payload { get; }
        }

        private sealed class PressureDisposition
        {
            public static readonly PressureDisposition Missing =
                new PressureDisposition(false, null);
            public static readonly PressureDisposition Invalid =
                new PressureDisposition(false, null);

            public PressureDisposition(bool complete, LayoutPressureSnapshot payload)
            {
                IsComplete = complete;
                Payload = payload;
            }

            public bool IsComplete { get; }
            public LayoutPressureSnapshot Payload { get; }
        }
    }
}
