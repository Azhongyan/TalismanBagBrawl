using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.VocabularyExtension
{
    public static class EnemyMechanicVocabularyExtensionSchema
    {
        public const string SchemaId = "EnemyMechanicVocabularyExtension.v1";
        public const int SchemaVersion = 1;
        public const string ExtensionId = "extension.bone_aspect.mechanic_vocabulary_01";
        public const string BaseSchemaId = EnemyMechanicVocabularySchema.SchemaId;
        public const int BaseSchemaVersion = EnemyMechanicVocabularySchema.SchemaVersion;
    }

    public sealed class EnemyMechanicVocabularyExtensionEntrySnapshot
    {
        private readonly ReadOnlyCollection<string> supportingSurveyRowIds;

        public EnemyMechanicVocabularyExtensionEntrySnapshot(
            string candidateId,
            EnemyVocabularyCategory category,
            string stableKey,
            string developerLabelZh,
            string description,
            IReadOnlyList<string> supportingSurveyRowIds,
            bool playerVisible,
            bool developerOnly,
            bool runtimeImplemented)
        {
            CandidateId = candidateId ?? string.Empty;
            Category = category;
            StableKey = stableKey ?? string.Empty;
            DeveloperLabelZh = developerLabelZh ?? string.Empty;
            Description = description ?? string.Empty;
            this.supportingSurveyRowIds = Array.AsReadOnly(
                (supportingSurveyRowIds ?? Array.Empty<string>())
                    .Select(value => value ?? string.Empty)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray());
            PlayerVisible = playerVisible;
            DeveloperOnly = developerOnly;
            RuntimeImplemented = runtimeImplemented;
        }

        public string CandidateId { get; }
        public EnemyVocabularyCategory Category { get; }
        public string StableKey { get; }
        public string DeveloperLabelZh { get; }
        public string Description { get; }
        public IReadOnlyList<string> SupportingSurveyRowIds => supportingSurveyRowIds;
        public bool PlayerVisible { get; }
        public bool DeveloperOnly { get; }
        public bool RuntimeImplemented { get; }

        internal EnemyMechanicVocabularyExtensionEntrySnapshot Clone()
        {
            return new EnemyMechanicVocabularyExtensionEntrySnapshot(
                CandidateId,
                Category,
                StableKey,
                DeveloperLabelZh,
                Description,
                supportingSurveyRowIds,
                PlayerVisible,
                DeveloperOnly,
                RuntimeImplemented);
        }
    }

    public sealed class EnemyMechanicVocabularyExtensionSnapshot
    {
        private readonly ReadOnlyCollection<EnemyMechanicVocabularyExtensionEntrySnapshot> entries;

        public EnemyMechanicVocabularyExtensionSnapshot(
            string schemaId,
            int schemaVersion,
            string extensionId,
            string baseSchemaId,
            int baseSchemaVersion,
            bool devOnly,
            bool isEnabled,
            bool entersFormalFlow,
            bool runtimeImplemented,
            IReadOnlyList<EnemyMechanicVocabularyExtensionEntrySnapshot> entries)
        {
            SchemaId = schemaId ?? string.Empty;
            SchemaVersion = schemaVersion;
            ExtensionId = extensionId ?? string.Empty;
            BaseSchemaId = baseSchemaId ?? string.Empty;
            BaseSchemaVersion = baseSchemaVersion;
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
            RuntimeImplemented = runtimeImplemented;
            this.entries = Array.AsReadOnly(
                (entries ?? Array.Empty<EnemyMechanicVocabularyExtensionEntrySnapshot>())
                    .Select(value => value == null ? null : value.Clone())
                    .OrderBy(
                        value => value == null
                            ? string.Empty
                            : EnemyVocabularyCategoryNames.StableName(value.Category)
                                + "\u001f" + value.StableKey
                                + "\u001f" + value.CandidateId,
                        StringComparer.Ordinal)
                    .ToArray());
            CanonicalSignature = EnemyMechanicVocabularyExtensionCanonical.BuildSnapshot(this);
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public string ExtensionId { get; }
        public string BaseSchemaId { get; }
        public int BaseSchemaVersion { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
        public bool RuntimeImplemented { get; }
        public IReadOnlyList<EnemyMechanicVocabularyExtensionEntrySnapshot> Entries => entries;
        public string CanonicalSignature { get; }
    }

    internal static class EnemyMechanicVocabularyExtensionCanonical
    {
        public static string BuildSnapshot(EnemyMechanicVocabularyExtensionSnapshot snapshot)
        {
            StringBuilder builder = new StringBuilder(8192);
            Field(builder, "schemaId", snapshot.SchemaId);
            Field(builder, "schemaVersion", snapshot.SchemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "extensionId", snapshot.ExtensionId);
            Field(builder, "baseSchemaId", snapshot.BaseSchemaId);
            Field(builder, "baseSchemaVersion", snapshot.BaseSchemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "devOnly", snapshot.DevOnly ? "1" : "0");
            Field(builder, "isEnabled", snapshot.IsEnabled ? "1" : "0");
            Field(builder, "entersFormalFlow", snapshot.EntersFormalFlow ? "1" : "0");
            Field(builder, "runtimeImplemented", snapshot.RuntimeImplemented ? "1" : "0");
            string[] rows = snapshot.Entries
                .Select(value => value == null ? "<null>" : Entry(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(builder, "entries.count", rows.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(builder, "entries[" + index.ToString(CultureInfo.InvariantCulture) + "]", rows[index]);
            }

            return Hash(builder.ToString());
        }

        public static string Entry(EnemyMechanicVocabularyExtensionEntrySnapshot value)
        {
            StringBuilder builder = new StringBuilder(1024);
            Field(builder, "candidateId", value.CandidateId);
            Field(builder, "category", EnemyVocabularyCategoryNames.StableName(value.Category));
            Field(builder, "stableKey", value.StableKey);
            Field(builder, "developerLabelZh", value.DeveloperLabelZh);
            Field(builder, "description", value.Description);
            string[] rows = value.SupportingSurveyRowIds
                .OrderBy(row => row, StringComparer.Ordinal)
                .ToArray();
            Field(builder, "supportingSurveyRowIds.count", rows.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(builder, "supportingSurveyRowIds[" + index.ToString(CultureInfo.InvariantCulture) + "]", rows[index]);
            }

            Field(builder, "playerVisible", value.PlayerVisible ? "1" : "0");
            Field(builder, "developerOnly", value.DeveloperOnly ? "1" : "0");
            Field(builder, "runtimeImplemented", value.RuntimeImplemented ? "1" : "0");
            return builder.ToString();
        }

        private static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeName)
                .Append('=').Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append(';');
        }

        private static string Hash(string payload)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
                StringBuilder builder = new StringBuilder(71).Append("sha256:");
                foreach (byte value in bytes)
                {
                    builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
