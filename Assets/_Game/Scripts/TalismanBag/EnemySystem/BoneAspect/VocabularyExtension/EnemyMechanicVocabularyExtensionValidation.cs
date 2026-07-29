using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.VocabularyExtension
{
    public sealed class EnemyMechanicVocabularyExtensionValidationIssue
    {
        public EnemyMechanicVocabularyExtensionValidationIssue(string code, string path, string message)
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

    public sealed class EnemyMechanicVocabularyExtensionValidationException : ArgumentException
    {
        private readonly ReadOnlyCollection<EnemyMechanicVocabularyExtensionValidationIssue> issues;

        public EnemyMechanicVocabularyExtensionValidationException(
            IReadOnlyList<EnemyMechanicVocabularyExtensionValidationIssue> issues)
            : base(BuildMessage(issues))
        {
            this.issues = Array.AsReadOnly(
                (issues ?? Array.Empty<EnemyMechanicVocabularyExtensionValidationIssue>()).ToArray());
        }

        public IReadOnlyList<EnemyMechanicVocabularyExtensionValidationIssue> Issues => issues;

        private static string BuildMessage(
            IReadOnlyList<EnemyMechanicVocabularyExtensionValidationIssue> issues)
        {
            StringBuilder builder = new StringBuilder(
                "Enemy mechanic vocabulary extension validation failed.");
            foreach (EnemyMechanicVocabularyExtensionValidationIssue issue
                in issues ?? Array.Empty<EnemyMechanicVocabularyExtensionValidationIssue>())
            {
                builder.Append(' ').Append(issue);
            }

            return builder.ToString();
        }
    }

    public sealed class EnemyMechanicVocabularyExtensionValidator
    {
        public static readonly EnemyMechanicVocabularyExtensionValidator Instance =
            new EnemyMechanicVocabularyExtensionValidator();

        private static readonly Regex StableKeyPattern = new Regex(
            "^[a-z][a-z0-9_]*\\.[a-z][a-z0-9_]*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly IReadOnlyDictionary<string, ExpectedEntry> ExpectedByCandidate =
            new ReadOnlyDictionary<string, ExpectedEntry>(
                new Dictionary<string, ExpectedEntry>(StringComparer.Ordinal)
                {
                    {
                        "ba_gap_candidate.charge_attack",
                        new ExpectedEntry(
                            EnemyVocabularyCategory.Mechanic,
                            "mechanic.charge_attack",
                            "冲撞攻击",
                            "敌方以突进或冲撞作为可识别攻击意图的中立机制概念。",
                            "GAP-003")
                    },
                    {
                        "ba_gap_candidate.contested_mark",
                        new ExpectedEntry(
                            EnemyVocabularyCategory.Mechanic,
                            "mechanic.contested_mark",
                            "争夺标记",
                            "敌方围绕某目标建立可争夺关系标记的中立机制概念，不定义资源结算。",
                            "GAP-015")
                    },
                    {
                        "ba_gap_candidate.damage_reduction",
                        new ExpectedEntry(
                            EnemyVocabularyCategory.Mechanic,
                            "mechanic.damage_reduction",
                            "减伤",
                            "敌方以非护盾方式降低承伤的中立机制概念。",
                            "GAP-016")
                    },
                    {
                        "ba_gap_candidate.possession_state",
                        new ExpectedEntry(
                            EnemyVocabularyCategory.Mechanic,
                            "mechanic.possession_state",
                            "附身状态",
                            "敌方进入或施加附身/寄宿关系状态的中立机制概念。",
                            "GAP-001")
                    },
                    {
                        "ba_gap_candidate.status_stack",
                        new ExpectedEntry(
                            EnemyVocabularyCategory.Mechanic,
                            "mechanic.status_stack",
                            "状态叠加",
                            "同类敌方状态可累积层数或强度的中立机制概念，不指定状态类型。",
                            "GAP-005")
                    },
                    {
                        "ba_gap_candidate.recognition_reveal_window",
                        new ExpectedEntry(
                            EnemyVocabularyCategory.CounterWindowType,
                            "counter_window.recognition_reveal",
                            "识破后显露窗口",
                            "玩家完成识破后出现显露或失防窗口的中立反制类别，不预设净化。",
                            "GAP-010")
                    },
                    {
                        "ba_gap_candidate.weakpoint_exposure_window",
                        new ExpectedEntry(
                            EnemyVocabularyCategory.CounterWindowType,
                            "counter_window.weakpoint_exposure",
                            "弱点暴露窗口",
                            "敌方弱点或核心进入可暴露窗口的中立反制类别，不预设触发源。",
                            "GAP-039")
                    }
                });

        private EnemyMechanicVocabularyExtensionValidator()
        {
        }

        public IReadOnlyList<EnemyMechanicVocabularyExtensionValidationIssue> Validate(
            EnemyMechanicVocabularyExtensionSnapshot snapshot)
        {
            List<EnemyMechanicVocabularyExtensionValidationIssue> issues =
                new List<EnemyMechanicVocabularyExtensionValidationIssue>();
            if (snapshot == null)
            {
                issues.Add(Issue("INPUT_NULL", "$", "Extension snapshot is required."));
                return Array.AsReadOnly(issues.ToArray());
            }

            if (!string.Equals(
                snapshot.SchemaId,
                EnemyMechanicVocabularyExtensionSchema.SchemaId,
                StringComparison.Ordinal))
            {
                issues.Add(Issue("SCHEMA_ID_MISMATCH", "schemaId", "Extension schema ID must match exactly."));
            }

            if (snapshot.SchemaVersion != EnemyMechanicVocabularyExtensionSchema.SchemaVersion)
            {
                issues.Add(Issue("SCHEMA_VERSION_MISMATCH", "schemaVersion", "Extension schema version must match exactly."));
            }

            if (!string.Equals(
                snapshot.ExtensionId,
                EnemyMechanicVocabularyExtensionSchema.ExtensionId,
                StringComparison.Ordinal))
            {
                issues.Add(Issue("EXTENSION_ID_MISMATCH", "extensionId", "Extension ID must match exactly."));
            }

            if (!string.Equals(
                    snapshot.BaseSchemaId,
                    EnemyMechanicVocabularyExtensionSchema.BaseSchemaId,
                    StringComparison.Ordinal)
                || snapshot.BaseSchemaVersion != EnemyMechanicVocabularyExtensionSchema.BaseSchemaVersion)
            {
                issues.Add(Issue("BASE_SCHEMA_MISMATCH", "baseSchema", "Base schema identity must match E02 exactly."));
            }

            if (!snapshot.DevOnly || snapshot.IsEnabled || snapshot.EntersFormalFlow || snapshot.RuntimeImplemented)
            {
                issues.Add(Issue(
                    "EXTENSION_FLAGS_UNSAFE",
                    "$",
                    "Extension must remain devOnly=true, isEnabled=false, entersFormalFlow=false, runtimeImplemented=false."));
            }

            if (snapshot.Entries.Count != ExpectedByCandidate.Count)
            {
                issues.Add(Issue("ENTRY_COUNT_MISMATCH", "entries", "Extension must contain exactly seven approved entries."));
            }

            Dictionary<string, int> firstByStableKey = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> firstByCandidate = new Dictionary<string, int>(StringComparer.Ordinal);
            HashSet<string> seenExpectedCandidates = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < snapshot.Entries.Count; index++)
            {
                EnemyMechanicVocabularyExtensionEntrySnapshot entry = snapshot.Entries[index];
                string path = "entries[" + index + "]";
                if (entry == null)
                {
                    issues.Add(Issue("ENTRY_NULL", path, "Extension entries cannot be null."));
                    continue;
                }

                if (firstByStableKey.TryGetValue(entry.StableKey, out int firstStableIndex))
                {
                    issues.Add(Issue(
                        "EXTENSION_STABLE_KEY_DUPLICATE",
                        path + ".stableKey",
                        "Duplicate stable key; first seen at " + firstStableIndex + "."));
                }
                else
                {
                    firstByStableKey[entry.StableKey] = index;
                }

                if (firstByCandidate.TryGetValue(entry.CandidateId, out int firstCandidateIndex))
                {
                    issues.Add(Issue(
                        "CANDIDATE_ID_DUPLICATE",
                        path + ".candidateId",
                        "Duplicate candidate ID; first seen at " + firstCandidateIndex + "."));
                }
                else
                {
                    firstByCandidate[entry.CandidateId] = index;
                }

                string requiredPrefix = EnemyVocabularyCategoryNames.RequiredPrefix(entry.Category);
                if (!StableKeyPattern.IsMatch(entry.StableKey)
                    || !entry.StableKey.StartsWith(requiredPrefix, StringComparison.Ordinal))
                {
                    issues.Add(Issue(
                        "STABLE_KEY_PREFIX_OR_FORMAT_MISMATCH",
                        path + ".stableKey",
                        "Stable key must use the exact category prefix and lowercase namespaced format."));
                }

                if (entry.PlayerVisible)
                {
                    issues.Add(Issue(
                        "PLAYER_VISIBLE_ENTRY_FORBIDDEN",
                        path + ".playerVisible",
                        "Extension entries cannot be player visible."));
                }

                if (entry.DeveloperOnly || entry.RuntimeImplemented)
                {
                    issues.Add(Issue(
                        "ENTRY_FLAGS_UNSAFE",
                        path,
                        "Entry developerOnly and runtimeImplemented flags must both remain false."));
                }

                if (!ExpectedByCandidate.TryGetValue(entry.CandidateId, out ExpectedEntry expected))
                {
                    issues.Add(Issue(
                        "UNAPPROVED_ENTRY",
                        path + ".candidateId",
                        "Only the seven Guard-approved candidate identities are allowed."));
                    foreach (string surveyRowId in entry.SupportingSurveyRowIds)
                    {
                        if (!ExpectedByCandidate.Values.Any(
                            value => string.Equals(value.SurveyRowId, surveyRowId, StringComparison.Ordinal)))
                        {
                            issues.Add(Issue(
                                "UNKNOWN_SURVEY_ROW",
                                path + ".supportingSurveyRowIds",
                                "Survey row is not one of the seven approved source rows."));
                        }
                    }

                    continue;
                }

                seenExpectedCandidates.Add(entry.CandidateId);
                if (entry.Category != expected.Category
                    || !string.Equals(entry.StableKey, expected.StableKey, StringComparison.Ordinal)
                    || !string.Equals(entry.DeveloperLabelZh, expected.DeveloperLabelZh, StringComparison.Ordinal)
                    || !string.Equals(entry.Description, expected.Description, StringComparison.Ordinal))
                {
                    issues.Add(Issue(
                        "CANDIDATE_LINEAGE_MISMATCH",
                        path,
                        "Candidate category, stable key, label, and description must match the accepted lineage."));
                }

                if (entry.SupportingSurveyRowIds.Count != 1
                    || !string.Equals(
                        entry.SupportingSurveyRowIds[0],
                        expected.SurveyRowId,
                        StringComparison.Ordinal))
                {
                    bool hasUnknown = entry.SupportingSurveyRowIds.Any(
                        value => !ExpectedByCandidate.Values.Any(
                            expectedValue => string.Equals(
                                expectedValue.SurveyRowId,
                                value,
                                StringComparison.Ordinal)));
                    issues.Add(Issue(
                        hasUnknown ? "UNKNOWN_SURVEY_ROW" : "CANDIDATE_LINEAGE_MISMATCH",
                        path + ".supportingSurveyRowIds",
                        "Candidate must cite its one exact accepted survey row."));
                }
            }

            foreach (string candidateId in ExpectedByCandidate.Keys.OrderBy(value => value, StringComparer.Ordinal))
            {
                if (!seenExpectedCandidates.Contains(candidateId))
                {
                    issues.Add(Issue(
                        "REQUIRED_ENTRY_MISSING",
                        "entries",
                        "Missing required candidate " + candidateId + "."));
                }
            }

            return Array.AsReadOnly(issues.ToArray());
        }

        public void EnsureValid(EnemyMechanicVocabularyExtensionSnapshot snapshot)
        {
            IReadOnlyList<EnemyMechanicVocabularyExtensionValidationIssue> issues = Validate(snapshot);
            if (issues.Count > 0)
            {
                throw new EnemyMechanicVocabularyExtensionValidationException(issues);
            }
        }

        private static EnemyMechanicVocabularyExtensionValidationIssue Issue(
            string code,
            string path,
            string message)
        {
            return new EnemyMechanicVocabularyExtensionValidationIssue(code, path, message);
        }

        private sealed class ExpectedEntry
        {
            public ExpectedEntry(
                EnemyVocabularyCategory category,
                string stableKey,
                string developerLabelZh,
                string description,
                string surveyRowId)
            {
                Category = category;
                StableKey = stableKey;
                DeveloperLabelZh = developerLabelZh;
                Description = description;
                SurveyRowId = surveyRowId;
            }

            public EnemyVocabularyCategory Category { get; }
            public string StableKey { get; }
            public string DeveloperLabelZh { get; }
            public string Description { get; }
            public string SurveyRowId { get; }
        }
    }
}
