using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TalismanBag.EnemySystem.RequirementChannel
{
    public sealed class EnemyRequirementChannelApplicabilityValidationIssue
    {
        public EnemyRequirementChannelApplicabilityValidationIssue(
            string code,
            string path,
            string message)
        {
            Code = code ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public string Code { get; }
        public string Path { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Code + " @ " + Path + ": " + Message;
        }
    }

    public sealed class EnemyRequirementChannelApplicabilityValidationException :
        ArgumentException
    {
        private readonly ReadOnlyCollection<
            EnemyRequirementChannelApplicabilityValidationIssue> issues;

        public EnemyRequirementChannelApplicabilityValidationException(
            IReadOnlyList<EnemyRequirementChannelApplicabilityValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly((issues ??
                    Array.Empty<EnemyRequirementChannelApplicabilityValidationIssue>())
                .ToArray());
        }

        public IReadOnlyList<EnemyRequirementChannelApplicabilityValidationIssue> Issues =>
            issues;

        private static string BuildMessage(
            IReadOnlyList<EnemyRequirementChannelApplicabilityValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(
                "Enemy requirement channel applicability validation failed.");
            foreach (EnemyRequirementChannelApplicabilityValidationIssue issue in
                issues ?? Array.Empty<EnemyRequirementChannelApplicabilityValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class DefaultEnemyRequirementChannelApplicabilitySnapshotProvider :
        IEnemyRequirementChannelApplicabilitySnapshotProvider
    {
        public static readonly DefaultEnemyRequirementChannelApplicabilitySnapshotProvider
            Instance = new DefaultEnemyRequirementChannelApplicabilitySnapshotProvider();

        private readonly IEnemyRequirementChannelApplicabilitySnapshotValidator validator;

        public DefaultEnemyRequirementChannelApplicabilitySnapshotProvider(
            IEnemyRequirementChannelApplicabilitySnapshotValidator validator = null)
        {
            this.validator = validator ??
                DefaultEnemyRequirementChannelApplicabilitySnapshotValidator.Instance;
        }

        public EnemyRequirementChannelApplicabilitySnapshot CreateSnapshot(
            EnemyRequirementChannelApplicabilitySnapshotInput input)
        {
            IReadOnlyList<EnemyRequirementChannelApplicabilityValidationIssue> issues =
                validator.Validate(input);
            if (issues.Count > 0)
            {
                throw new EnemyRequirementChannelApplicabilityValidationException(issues);
            }

            return new EnemyRequirementChannelApplicabilitySnapshot(input);
        }
    }

    public sealed class DefaultEnemyRequirementChannelApplicabilitySnapshotValidator :
        IEnemyRequirementChannelApplicabilitySnapshotValidator
    {
        public static readonly DefaultEnemyRequirementChannelApplicabilitySnapshotValidator
            Instance = new DefaultEnemyRequirementChannelApplicabilitySnapshotValidator();

        public IReadOnlyList<EnemyRequirementChannelApplicabilityValidationIssue> Validate(
            EnemyRequirementChannelApplicabilitySnapshotInput input)
        {
            List<EnemyRequirementChannelApplicabilityValidationIssue> issues =
                new List<EnemyRequirementChannelApplicabilityValidationIssue>();
            if (input == null)
            {
                Add(issues, "INPUT_NULL", "$", "Snapshot input is required.");
                return Freeze(issues);
            }

            if (!string.Equals(
                input.SchemaId,
                EnemyRequirementChannelApplicabilitySchema.SchemaId,
                StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "SCHEMA_ID_MISMATCH",
                    "schemaId",
                    "Schema ID must match exactly with ordinal semantics.");
            }

            if (input.SchemaVersion !=
                EnemyRequirementChannelApplicabilitySchema.SchemaVersion)
            {
                Add(
                    issues,
                    "SCHEMA_VERSION_MISMATCH",
                    "schemaVersion",
                    "Schema version must match exactly.");
            }

            ValidateId(input.SnapshotId, "snapshotId", "SNAPSHOT_ID", issues);
            if (!IsDefined(input.EvaluationChannel))
            {
                Add(
                    issues,
                    "EVALUATION_CHANNEL_UNDEFINED",
                    "evaluationChannel",
                    "Evaluation channel must be one of the three named schema values.");
            }

            HashSet<string> requirementIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < input.Rows.Count; index++)
            {
                EnemyRequirementChannelApplicabilityRowSnapshot row = input.Rows[index];
                string path = "rows[" + index + "]";
                if (row == null)
                {
                    Add(issues, "ROW_NULL", path, "Rows cannot contain null entries.");
                    continue;
                }

                ValidateId(
                    row.RequirementId,
                    path + ".requirementId",
                    "REQUIREMENT_ID",
                    issues);
                if (!requirementIds.Add(row.RequirementId))
                {
                    Add(
                        issues,
                        "REQUIREMENT_ID_DUPLICATE",
                        path + ".requirementId",
                        "Requirement IDs must be unique with ordinal semantics.");
                }

                bool declaredDefined = IsDefined(row.DeclaredChannel);
                bool stateDefined = IsDefined(row.ApplicabilityState);
                if (!declaredDefined)
                {
                    Add(
                        issues,
                        "DECLARED_CHANNEL_UNDEFINED",
                        path + ".declaredChannel",
                        "Declared channel must be one of the three named schema values.");
                }

                if (!stateDefined)
                {
                    Add(
                        issues,
                        "APPLICABILITY_STATE_UNDEFINED",
                        path + ".applicabilityState",
                        "Applicability state must be one of the four named schema values.");
                }

                if (IsDefined(input.EvaluationChannel) && declaredDefined && stateDefined)
                {
                    bool sameChannel = row.DeclaredChannel == input.EvaluationChannel;
                    if (sameChannel && row.ApplicabilityState ==
                        EnemyRequirementApplicabilityState.NotInChannel)
                    {
                        Add(
                            issues,
                            "SAME_CHANNEL_NOT_IN_CHANNEL",
                            path + ".applicabilityState",
                            "NotInChannel requires DeclaredChannel to differ from EvaluationChannel.");
                    }
                    else if (!sameChannel && row.ApplicabilityState !=
                        EnemyRequirementApplicabilityState.NotInChannel)
                    {
                        Add(
                            issues,
                            "CROSS_CHANNEL_STATE_INVALID",
                            path + ".applicabilityState",
                            "A row outside the evaluation channel must use NotInChannel.");
                    }
                }
            }

            return Freeze(issues);
        }

        private static bool IsDefined(EnemyRequirementChannel value)
        {
            return value == EnemyRequirementChannel.ContinuousBP
                || value == EnemyRequirementChannel.StructuralPredicate
                || value == EnemyRequirementChannel.RuntimeEventSignal;
        }

        private static bool IsDefined(EnemyRequirementApplicabilityState value)
        {
            return value == EnemyRequirementApplicabilityState.Applicable
                || value == EnemyRequirementApplicabilityState.Unknown
                || value == EnemyRequirementApplicabilityState.NotApplicable
                || value == EnemyRequirementApplicabilityState.NotInChannel;
        }

        private static void ValidateId(
            string value,
            string path,
            string codePrefix,
            ICollection<EnemyRequirementChannelApplicabilityValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Add(
                    issues,
                    codePrefix + "_EMPTY",
                    path,
                    "A non-empty ordinal identity is required.");
                return;
            }

            if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                Add(
                    issues,
                    codePrefix + "_OUTER_WHITESPACE",
                    path,
                    "Identity values are never trimmed implicitly.");
            }
        }

        private static ReadOnlyCollection<
            EnemyRequirementChannelApplicabilityValidationIssue> Freeze(
                IEnumerable<EnemyRequirementChannelApplicabilityValidationIssue> issues)
        {
            return Array.AsReadOnly((issues ??
                    Array.Empty<EnemyRequirementChannelApplicabilityValidationIssue>())
                .OrderBy(value => value.Path, StringComparer.Ordinal)
                .ThenBy(value => value.Code, StringComparer.Ordinal)
                .ToArray());
        }

        private static void Add(
            ICollection<EnemyRequirementChannelApplicabilityValidationIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new EnemyRequirementChannelApplicabilityValidationIssue(
                code,
                path,
                message));
        }
    }
}

