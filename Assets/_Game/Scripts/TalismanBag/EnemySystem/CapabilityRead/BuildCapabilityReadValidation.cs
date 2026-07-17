using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TalismanBag.EnemySystem.CapabilityRead
{
    public sealed class BuildCapabilityValidationIssue
    {
        public BuildCapabilityValidationIssue(
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

    public sealed class BuildCapabilityValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<BuildCapabilityValidationIssue> issues;

        public BuildCapabilityValidationException(
            IReadOnlyList<BuildCapabilityValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly(
                (issues ?? Array.Empty<BuildCapabilityValidationIssue>()).ToArray());
        }

        public IReadOnlyList<BuildCapabilityValidationIssue> Issues => issues;

        private static string BuildMessage(
            IReadOnlyList<BuildCapabilityValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(
                "Build capability snapshot validation failed.");
            foreach (BuildCapabilityValidationIssue issue in
                issues ?? Array.Empty<BuildCapabilityValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class DefaultBuildCapabilitySnapshotProvider :
        IBuildCapabilitySnapshotProvider
    {
        public static readonly DefaultBuildCapabilitySnapshotProvider Instance =
            new DefaultBuildCapabilitySnapshotProvider();

        private readonly IBuildCapabilitySnapshotValidator validator;

        public DefaultBuildCapabilitySnapshotProvider(
            IBuildCapabilitySnapshotValidator validator = null)
        {
            this.validator = validator ??
                DefaultBuildCapabilitySnapshotValidator.Instance;
        }

        public BuildCapabilitySnapshot CreateSnapshot(
            BuildCapabilitySnapshotInput input,
            IBuildCapabilityVocabularyResolver resolver)
        {
            IReadOnlyList<BuildCapabilityValidationIssue> issues =
                validator.Validate(input, resolver);
            if (issues.Count > 0)
            {
                throw new BuildCapabilityValidationException(issues);
            }

            return new BuildCapabilitySnapshot(input);
        }
    }

    public sealed class DefaultBuildCapabilitySnapshotValidator :
        IBuildCapabilitySnapshotValidator
    {
        public static readonly DefaultBuildCapabilitySnapshotValidator Instance =
            new DefaultBuildCapabilitySnapshotValidator();

        private static readonly string[] SensitiveIdTokens =
        {
            "asset",
            "scene",
            "account",
            "user",
            "save",
            "slot",
            "playerprefs"
        };

        public IReadOnlyList<BuildCapabilityValidationIssue> Validate(
            BuildCapabilitySnapshotInput input,
            IBuildCapabilityVocabularyResolver resolver)
        {
            List<BuildCapabilityValidationIssue> issues =
                new List<BuildCapabilityValidationIssue>();
            if (input == null)
            {
                Add(issues, "INPUT_NULL", "$", "Snapshot input is required.");
            }

            if (resolver == null)
            {
                Add(issues, "RESOLVER_NULL", "resolver", "Vocabulary resolver is required.");
            }

            if (input == null)
            {
                return Array.AsReadOnly(issues.ToArray());
            }

            if (!string.Equals(
                input.SchemaId,
                BuildCapabilityReadSchema.SchemaId,
                StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "SCHEMA_ID_MISMATCH",
                    "schemaId",
                    "Schema ID must match exactly.");
            }

            if (input.SchemaVersion != BuildCapabilityReadSchema.SchemaVersion)
            {
                Add(
                    issues,
                    "SCHEMA_VERSION_MISMATCH",
                    "schemaVersion",
                    "Schema version must match exactly.");
            }

            ValidateOpaqueId(input.SnapshotId, "snapshotId", issues);
            ValidateOpaqueId(input.SourceRevisionId, "sourceRevisionId", issues);

            if (!Enum.IsDefined(
                typeof(BuildCapabilityCoverageMode),
                input.CoverageMode))
            {
                Add(
                    issues,
                    "COVERAGE_MODE_UNKNOWN",
                    "coverageMode",
                    "Coverage mode is not defined by this schema.");
            }

            IReadOnlyList<string> knownKeys = resolver == null
                ? null
                : resolver.GetKnownBuildCapabilityKeys();
            HashSet<string> knownKeySet = ValidateResolverKeys(
                resolver,
                knownKeys,
                issues);
            ValidateCapabilityValues(input, resolver, knownKeySet, issues);

            if (!input.DevOnly)
            {
                Add(
                    issues,
                    "DEV_ONLY_REQUIRED",
                    "devOnly",
                    "Capability snapshots must remain developer-only.");
            }

            if (input.IsEnabled)
            {
                Add(
                    issues,
                    "ENABLED_FORBIDDEN",
                    "isEnabled",
                    "Capability snapshots must remain disabled.");
            }

            if (input.EntersFormalFlow)
            {
                Add(
                    issues,
                    "FORMAL_FLOW_FORBIDDEN",
                    "entersFormalFlow",
                    "Capability snapshots cannot enter formal flow.");
            }

            return Array.AsReadOnly(issues.ToArray());
        }

        private static HashSet<string> ValidateResolverKeys(
            IBuildCapabilityVocabularyResolver resolver,
            IReadOnlyList<string> knownKeys,
            ICollection<BuildCapabilityValidationIssue> issues)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
            if (resolver == null)
            {
                return result;
            }

            if (knownKeys == null)
            {
                Add(
                    issues,
                    "RESOLVER_KEYS_NULL",
                    "resolver.knownKeys",
                    "Resolver known-key collection cannot be null.");
                return result;
            }

            for (int index = 0; index < knownKeys.Count; index++)
            {
                string key = knownKeys[index];
                string path = "resolver.knownKeys[" + index + "]";
                if (string.IsNullOrWhiteSpace(key))
                {
                    Add(
                        issues,
                        "RESOLVER_KEY_EMPTY",
                        path,
                        "Resolver keys cannot be null, empty, or whitespace.");
                    continue;
                }

                if (!string.Equals(key, key.Trim(), StringComparison.Ordinal))
                {
                    Add(
                        issues,
                        "RESOLVER_KEY_OUTER_WHITESPACE",
                        path,
                        "Resolver keys are never trimmed implicitly.");
                }

                if (!key.StartsWith("capability.", StringComparison.Ordinal))
                {
                    Add(
                        issues,
                        "RESOLVER_KEY_CATEGORY_INVALID",
                        path,
                        "Resolver keys must belong to the BuildCapability category.");
                }

                if (!result.Add(key))
                {
                    Add(
                        issues,
                        "RESOLVER_KEY_DUPLICATE",
                        path,
                        "Resolver keys must be unique with ordinal semantics.");
                }

                if (!resolver.HasBuildCapabilityKey(key))
                {
                    Add(
                        issues,
                        "RESOLVER_KEY_UNRESOLVED",
                        path,
                        "Every enumerated resolver key must resolve as BuildCapability.");
                }
            }

            return result;
        }

        private static void ValidateCapabilityValues(
            BuildCapabilitySnapshotInput input,
            IBuildCapabilityVocabularyResolver resolver,
            HashSet<string> knownKeys,
            ICollection<BuildCapabilityValidationIssue> issues)
        {
            HashSet<string> valueKeys = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < input.CapabilityValues.Count; index++)
            {
                BuildCapabilityValueSnapshot value = input.CapabilityValues[index];
                string path = "capabilityValues[" + index + "]";
                if (value == null)
                {
                    Add(
                        issues,
                        "CAPABILITY_VALUE_NULL",
                        path,
                        "Capability value entries cannot be null.");
                    continue;
                }

                ValidateCapabilityKey(value.BuildCapabilityKey, path, resolver, issues);
                if (!valueKeys.Add(value.BuildCapabilityKey))
                {
                    Add(
                        issues,
                        "BUILD_CAPABILITY_KEY_DUPLICATE",
                        path + ".buildCapabilityKey",
                        "Capability keys must be unique with ordinal semantics.");
                }

                if (value.ValueBasisPoints < BuildCapabilityScale.MinimumValueBasisPoints
                    || value.ValueBasisPoints > BuildCapabilityScale.MaximumValueBasisPoints)
                {
                    Add(
                        issues,
                        "VALUE_BASIS_POINTS_OUT_OF_RANGE",
                        path + ".valueBasisPoints",
                        "Capability values must be in the inclusive 0..10000 range.");
                }

                ValidateSourceSummaries(value.SourceSummaries, path, issues);
            }

            if (input.CoverageMode == BuildCapabilityCoverageMode.Complete)
            {
                if (valueKeys.Count != knownKeys.Count)
                {
                    Add(
                        issues,
                        "COMPLETE_KEY_COUNT_MISMATCH",
                        "capabilityValues",
                        "Complete coverage must contain every resolver key exactly once.");
                }

                foreach (string knownKey in knownKeys)
                {
                    if (!valueKeys.Contains(knownKey))
                    {
                        Add(
                            issues,
                            "COMPLETE_KEY_MISSING",
                            "capabilityValues",
                            "Complete coverage is missing resolver key " + knownKey + ".");
                    }
                }
            }
        }

        private static void ValidateCapabilityKey(
            string key,
            string path,
            IBuildCapabilityVocabularyResolver resolver,
            ICollection<BuildCapabilityValidationIssue> issues)
        {
            string keyPath = path + ".buildCapabilityKey";
            if (string.IsNullOrWhiteSpace(key))
            {
                Add(
                    issues,
                    "BUILD_CAPABILITY_KEY_EMPTY",
                    keyPath,
                    "BuildCapability key is required.");
                return;
            }

            if (!string.Equals(key, key.Trim(), StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "BUILD_CAPABILITY_KEY_OUTER_WHITESPACE",
                    keyPath,
                    "BuildCapability keys are never trimmed implicitly.");
            }

            if (!key.StartsWith("capability.", StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "BUILD_CAPABILITY_KEY_CATEGORY_INVALID",
                    keyPath,
                    "The key must belong to the BuildCapability category.");
            }

            if (resolver != null && !resolver.HasBuildCapabilityKey(key))
            {
                Add(
                    issues,
                    "BUILD_CAPABILITY_KEY_UNKNOWN",
                    keyPath,
                    "The key is not present in the current BuildCapability vocabulary.");
            }
        }

        private static void ValidateSourceSummaries(
            IReadOnlyList<BuildCapabilitySourceSummarySnapshot> summaries,
            string valuePath,
            ICollection<BuildCapabilityValidationIssue> issues)
        {
            HashSet<string> categoryIds = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < summaries.Count; index++)
            {
                BuildCapabilitySourceSummarySnapshot summary = summaries[index];
                string path = valuePath + ".sourceSummaries[" + index + "]";
                if (summary == null)
                {
                    Add(
                        issues,
                        "SOURCE_SUMMARY_NULL",
                        path,
                        "Source summaries cannot contain null entries.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(summary.SourceCategoryId))
                {
                    Add(
                        issues,
                        "SOURCE_CATEGORY_ID_EMPTY",
                        path + ".sourceCategoryId",
                        "Source category ID is required.");
                }
                else if (!string.Equals(
                    summary.SourceCategoryId,
                    summary.SourceCategoryId.Trim(),
                    StringComparison.Ordinal))
                {
                    Add(
                        issues,
                        "SOURCE_CATEGORY_ID_OUTER_WHITESPACE",
                        path + ".sourceCategoryId",
                        "Source category IDs are never trimmed implicitly.");
                }

                if (!categoryIds.Add(summary.SourceCategoryId))
                {
                    Add(
                        issues,
                        "SOURCE_CATEGORY_ID_DUPLICATE",
                        path + ".sourceCategoryId",
                        "Source category IDs must be unique within one capability value.");
                }

                if (summary.SourceCount < 1)
                {
                    Add(
                        issues,
                        "SOURCE_COUNT_INVALID",
                        path + ".sourceCount",
                        "Source count must be at least one.");
                }

                if (summary.ContributionBasisPoints <
                        BuildCapabilityScale.MinimumValueBasisPoints
                    || summary.ContributionBasisPoints >
                        BuildCapabilityScale.MaximumValueBasisPoints)
                {
                    Add(
                        issues,
                        "CONTRIBUTION_BASIS_POINTS_OUT_OF_RANGE",
                        path + ".contributionBasisPoints",
                        "Diagnostic contribution must be in the inclusive 0..10000 range.");
                }
            }
        }

        private static void ValidateOpaqueId(
            string value,
            string path,
            ICollection<BuildCapabilityValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Add(issues, "ID_EMPTY", path, "Opaque stable ID is required.");
                return;
            }

            if (!string.Equals(value, value.Trim(), StringComparison.Ordinal))
            {
                Add(
                    issues,
                    "ID_OUTER_WHITESPACE",
                    path,
                    "Opaque IDs are never trimmed implicitly.");
            }

            bool pathLike = value.IndexOf('/') >= 0
                || value.IndexOf('\\') >= 0
                || value.IndexOf(':') >= 0;
            bool semanticLeak = SensitiveIdTokens.Any(token =>
                value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
            if (pathLike || semanticLeak)
            {
                Add(
                    issues,
                    "ID_SENSITIVE_SEMANTIC",
                    path,
                    "Opaque IDs cannot expose paths, accounts, scenes, or persistence slots.");
            }
        }

        private static void Add(
            ICollection<BuildCapabilityValidationIssue> issues,
            string code,
            string path,
            string message)
        {
            issues.Add(new BuildCapabilityValidationIssue(code, path, message));
        }
    }
}
