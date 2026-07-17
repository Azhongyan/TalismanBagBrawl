using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TalismanBag.EnemySystem.Vocabulary
{
    public sealed class EnemyVocabularyValidationIssue
    {
        public EnemyVocabularyValidationIssue(string code, string path, string message)
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

    public interface IEnemyMechanicVocabularyValidator
    {
        IReadOnlyList<EnemyVocabularyValidationIssue> Validate(EnemyMechanicVocabularySnapshotInput input);
    }

    public interface IEnemyMechanicVocabularyProvider
    {
        EnemyMechanicVocabularySnapshot CreateSnapshot(EnemyMechanicVocabularySnapshotInput input);
    }

    public sealed class EnemyMechanicVocabularyValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<EnemyVocabularyValidationIssue> issues;

        public EnemyMechanicVocabularyValidationException(IReadOnlyList<EnemyVocabularyValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly((issues ?? Array.Empty<EnemyVocabularyValidationIssue>()).ToArray());
        }

        public IReadOnlyList<EnemyVocabularyValidationIssue> Issues => issues;

        private static string BuildMessage(IReadOnlyList<EnemyVocabularyValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder("Enemy mechanic vocabulary validation failed.");
            foreach (EnemyVocabularyValidationIssue issue in issues ?? Array.Empty<EnemyVocabularyValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class DefaultEnemyMechanicVocabularyProvider : IEnemyMechanicVocabularyProvider
    {
        public static readonly DefaultEnemyMechanicVocabularyProvider Instance = new DefaultEnemyMechanicVocabularyProvider();

        private readonly IEnemyMechanicVocabularyValidator validator;

        public DefaultEnemyMechanicVocabularyProvider(IEnemyMechanicVocabularyValidator validator = null)
        {
            this.validator = validator ?? DefaultEnemyMechanicVocabularyValidator.Instance;
        }

        public EnemyMechanicVocabularySnapshot CreateSnapshot(EnemyMechanicVocabularySnapshotInput input)
        {
            IReadOnlyList<EnemyVocabularyValidationIssue> issues = validator.Validate(input);
            if (issues.Count > 0)
            {
                throw new EnemyMechanicVocabularyValidationException(issues);
            }

            return new EnemyMechanicVocabularySnapshot(input);
        }
    }

    public sealed class DefaultEnemyMechanicVocabularyValidator : IEnemyMechanicVocabularyValidator
    {
        public static readonly DefaultEnemyMechanicVocabularyValidator Instance = new DefaultEnemyMechanicVocabularyValidator();

        private static readonly Regex StableKeyPattern = new Regex(
            "^[a-z][a-z0-9_]*\\.[a-z][a-z0-9_]*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly string[] PlayerAnswerTokens =
        {
            "solution",
            "answer",
            "requiredsynergy",
            "requiredaffix",
            "requiredstats",
            "hardsolutiontags",
            "minimumkeysrequired",
            "dropbias",
            "exactthreshold"
        };

        public IReadOnlyList<EnemyVocabularyValidationIssue> Validate(EnemyMechanicVocabularySnapshotInput input)
        {
            List<EnemyVocabularyValidationIssue> issues = new List<EnemyVocabularyValidationIssue>();
            if (input == null)
            {
                issues.Add(new EnemyVocabularyValidationIssue("INPUT_NULL", "$", "Vocabulary input is required."));
                return Array.AsReadOnly(issues.ToArray());
            }

            if (!string.Equals(input.SchemaId, EnemyMechanicVocabularySchema.SchemaId, StringComparison.Ordinal))
            {
                issues.Add(new EnemyVocabularyValidationIssue("SCHEMA_ID_MISMATCH", "schemaId", "Schema ID must match exactly."));
            }

            if (input.SchemaVersion != EnemyMechanicVocabularySchema.SchemaVersion)
            {
                issues.Add(new EnemyVocabularyValidationIssue("SCHEMA_VERSION_MISMATCH", "schemaVersion", "Schema version must match exactly."));
            }

            Dictionary<string, int> firstEntryByIdentity = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, EnemyVocabularyCategory> firstCategoryByStableKey = new Dictionary<string, EnemyVocabularyCategory>(StringComparer.Ordinal);
            HashSet<string> resolvableTargets = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < input.Entries.Count; index++)
            {
                EnemyVocabularyEntrySnapshot entry = input.Entries[index];
                string path = "entries[" + index + "]";
                if (entry == null || entry.Key == null)
                {
                    issues.Add(new EnemyVocabularyValidationIssue("ENTRY_NULL", path, "Vocabulary entries and their keys cannot be null."));
                    continue;
                }

                string identity = EnemyMechanicVocabularySnapshot.EntryIdentity(entry.Category, entry.StableKey);
                if (firstEntryByIdentity.TryGetValue(identity, out int firstIndex))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("STABLE_KEY_DUPLICATE", path + ".stableKey", "Duplicate exact category/key; first seen at " + firstIndex + "."));
                }
                else
                {
                    firstEntryByIdentity.Add(identity, index);
                    resolvableTargets.Add(identity);
                }

                if (firstCategoryByStableKey.TryGetValue(entry.StableKey, out EnemyVocabularyCategory firstCategory)
                    && firstCategory != entry.Category)
                {
                    issues.Add(new EnemyVocabularyValidationIssue("STABLE_KEY_CATEGORY_COLLISION", path + ".stableKey", "The same full stable key cannot be shared across vocabulary categories."));
                }
                else
                {
                    firstCategoryByStableKey[entry.StableKey] = entry.Category;
                }

                ValidateStableKey(entry, path, issues);
                if (string.IsNullOrWhiteSpace(entry.DeveloperLabelZh))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("DEVELOPER_LABEL_EMPTY", path + ".developerLabelZh", "Developer Chinese label is required."));
                }

                if (string.IsNullOrWhiteSpace(entry.Description))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("DESCRIPTION_EMPTY", path + ".description", "Developer description is required."));
                }

                if (entry.PlayerVisible && entry.DeveloperOnly)
                {
                    issues.Add(new EnemyVocabularyValidationIssue("VISIBILITY_FLAGS_CONFLICT", path, "An entry cannot be player-visible and developer-only at the same time."));
                }

                if (entry.Category == EnemyVocabularyCategory.PlayerHintCategory)
                {
                    if (!entry.PlayerVisible || entry.DeveloperOnly)
                    {
                        issues.Add(new EnemyVocabularyValidationIssue("PLAYER_HINT_VISIBILITY_INVALID", path, "Player hint categories must be player-visible and not developer-only."));
                    }

                    string playerPayload = (entry.StableKey + " " + entry.DeveloperLabelZh + " " + entry.Description).Replace("_", string.Empty);
                    if (PlayerAnswerTokens.Any(token => playerPayload.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        issues.Add(new EnemyVocabularyValidationIssue("PLAYER_HINT_ANSWER_LEAK", path, "Player hint category contains an answer-bearing token."));
                    }
                }

                if (entry.Category == EnemyVocabularyCategory.DeveloperDiagnosticCategory
                    && (entry.PlayerVisible || !entry.DeveloperOnly))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("DIAGNOSTIC_VISIBILITY_INVALID", path, "Developer diagnostics must remain hidden from players."));
                }
            }

            Dictionary<string, int> firstMappingByIdentity = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int index = 0; index < input.LegacyMappings.Count; index++)
            {
                LegacyEnemyVocabularyMappingSnapshot mapping = input.LegacyMappings[index];
                string path = "legacyMappings[" + index + "]";
                if (mapping == null)
                {
                    issues.Add(new EnemyVocabularyValidationIssue("LEGACY_MAPPING_NULL", path, "Legacy mappings cannot contain null entries."));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(mapping.LegacySourceKind))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("LEGACY_SOURCE_KIND_EMPTY", path + ".legacySourceKind", "Legacy source kind is required."));
                }

                if (string.IsNullOrWhiteSpace(mapping.LegacyKey))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("LEGACY_KEY_EMPTY", path + ".legacyKey", "Legacy key is required."));
                }

                if (string.IsNullOrWhiteSpace(mapping.Reason))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("LEGACY_REASON_EMPTY", path + ".reason", "Mapping or OUT_OF_SCOPE reason is required."));
                }

                string mappingIdentity = EnemyMechanicVocabularySnapshot.MappingIdentity(mapping.LegacySourceKind, mapping.LegacyKey);
                if (firstMappingByIdentity.TryGetValue(mappingIdentity, out int firstIndex))
                {
                    issues.Add(new EnemyVocabularyValidationIssue("LEGACY_SOURCE_DUPLICATE", path, "Duplicate exact legacy source key; first seen at " + firstIndex + "."));
                }
                else
                {
                    firstMappingByIdentity.Add(mappingIdentity, index);
                }

                if (mapping.Status == LegacyEnemyVocabularyMappingStatus.Mapped && mapping.Targets.Count == 0)
                {
                    issues.Add(new EnemyVocabularyValidationIssue("LEGACY_MAPPED_WITHOUT_TARGET", path, "Mapped legacy keys require at least one canonical target."));
                }

                if (mapping.Status == LegacyEnemyVocabularyMappingStatus.OutOfScope && mapping.Targets.Count != 0)
                {
                    issues.Add(new EnemyVocabularyValidationIssue("LEGACY_OUT_OF_SCOPE_WITH_TARGET", path, "OUT_OF_SCOPE entries cannot carry canonical targets."));
                }

                HashSet<string> targetIdentities = new HashSet<string>(StringComparer.Ordinal);
                for (int targetIndex = 0; targetIndex < mapping.Targets.Count; targetIndex++)
                {
                    EnemyVocabularyKeyReferenceSnapshot target = mapping.Targets[targetIndex];
                    string targetPath = path + ".targets[" + targetIndex + "]";
                    if (target == null)
                    {
                        issues.Add(new EnemyVocabularyValidationIssue("LEGACY_TARGET_NULL", targetPath, "Legacy targets cannot be null."));
                        continue;
                    }

                    string targetIdentity = EnemyMechanicVocabularySnapshot.EntryIdentity(target.Category, target.StableKey);
                    if (!targetIdentities.Add(targetIdentity))
                    {
                        issues.Add(new EnemyVocabularyValidationIssue("LEGACY_TARGET_DUPLICATE", targetPath, "A mapping cannot repeat the same exact target."));
                    }

                    if (!resolvableTargets.Contains(targetIdentity))
                    {
                        issues.Add(new EnemyVocabularyValidationIssue("LEGACY_TARGET_UNRESOLVED", targetPath, "Canonical target does not resolve with strict ordinal semantics."));
                    }
                }
            }

            return Array.AsReadOnly(issues.ToArray());
        }

        private static void ValidateStableKey(
            EnemyVocabularyEntrySnapshot entry,
            string path,
            ICollection<EnemyVocabularyValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(entry.StableKey))
            {
                issues.Add(new EnemyVocabularyValidationIssue("STABLE_KEY_EMPTY", path + ".stableKey", "Stable key is required."));
                return;
            }

            if (!string.Equals(entry.StableKey, entry.StableKey.Trim(), StringComparison.Ordinal))
            {
                issues.Add(new EnemyVocabularyValidationIssue("STABLE_KEY_OUTER_WHITESPACE", path + ".stableKey", "Stable keys are never trimmed implicitly."));
            }

            if (!StableKeyPattern.IsMatch(entry.StableKey))
            {
                issues.Add(new EnemyVocabularyValidationIssue("STABLE_KEY_FORMAT_INVALID", path + ".stableKey", "Expected namespaced lowercase snake-case identity."));
            }

            string prefix = EnemyVocabularyCategoryNames.RequiredPrefix(entry.Category);
            if (!entry.StableKey.StartsWith(prefix, StringComparison.Ordinal))
            {
                issues.Add(new EnemyVocabularyValidationIssue("STABLE_KEY_PREFIX_MISMATCH", path + ".stableKey", "Stable key prefix does not match its category."));
            }
        }
    }
}
