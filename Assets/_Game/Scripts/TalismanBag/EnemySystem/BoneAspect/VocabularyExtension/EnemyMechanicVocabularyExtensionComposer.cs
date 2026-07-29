using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.EnemySystem.Vocabulary;

namespace TalismanBag.EnemySystem.BoneAspect.VocabularyExtension
{
    public sealed class EnemyMechanicVocabularyExtensionComposer
    {
        public static readonly EnemyMechanicVocabularyExtensionComposer Instance =
            new EnemyMechanicVocabularyExtensionComposer();

        private readonly IEnemyMechanicVocabularyProvider provider;
        private readonly IEnemyMechanicVocabularyValidator baseValidator;
        private readonly EnemyMechanicVocabularyExtensionValidator extensionValidator;

        public EnemyMechanicVocabularyExtensionComposer(
            IEnemyMechanicVocabularyProvider provider = null,
            IEnemyMechanicVocabularyValidator baseValidator = null,
            EnemyMechanicVocabularyExtensionValidator extensionValidator = null)
        {
            this.provider = provider ?? DefaultEnemyMechanicVocabularyProvider.Instance;
            this.baseValidator = baseValidator ?? DefaultEnemyMechanicVocabularyValidator.Instance;
            this.extensionValidator = extensionValidator ?? EnemyMechanicVocabularyExtensionValidator.Instance;
        }

        public EnemyMechanicVocabularySnapshotInput Compose(
            EnemyMechanicVocabularySnapshotInput baseInput,
            EnemyMechanicVocabularyExtensionSnapshot extension)
        {
            List<EnemyMechanicVocabularyExtensionValidationIssue> issues =
                new List<EnemyMechanicVocabularyExtensionValidationIssue>();
            if (baseInput == null)
            {
                issues.Add(new EnemyMechanicVocabularyExtensionValidationIssue(
                    "BASE_INPUT_NULL",
                    "baseInput",
                    "An explicit E02 base input is required."));
            }

            issues.AddRange(extensionValidator.Validate(extension));
            if (baseInput != null)
            {
                if (!string.Equals(
                        baseInput.SchemaId,
                        EnemyMechanicVocabularyExtensionSchema.BaseSchemaId,
                        StringComparison.Ordinal)
                    || baseInput.SchemaVersion != EnemyMechanicVocabularyExtensionSchema.BaseSchemaVersion)
                {
                    issues.Add(new EnemyMechanicVocabularyExtensionValidationIssue(
                        "BASE_SCHEMA_MISMATCH",
                        "baseInput",
                        "Explicit base input must use the frozen E02 schema."));
                }

                foreach (EnemyVocabularyValidationIssue issue in baseValidator.Validate(baseInput))
                {
                    issues.Add(new EnemyMechanicVocabularyExtensionValidationIssue(
                        "BASE_" + issue.Code,
                        "baseInput." + issue.Path,
                        issue.Message));
                }
            }

            if (baseInput != null && extension != null)
            {
                HashSet<string> baseKeys = new HashSet<string>(
                    baseInput.Entries
                        .Where(value => value != null)
                        .Select(value => value.StableKey),
                    StringComparer.Ordinal);
                foreach (EnemyMechanicVocabularyExtensionEntrySnapshot entry
                    in extension.Entries.Where(value => value != null))
                {
                    if (baseKeys.Contains(entry.StableKey))
                    {
                        issues.Add(new EnemyMechanicVocabularyExtensionValidationIssue(
                            "BASE_KEY_COLLISION",
                            "extension.entries." + entry.StableKey,
                            "Extension stable keys cannot collide with the E02 base."));
                    }
                }
            }

            if (issues.Count > 0)
            {
                throw new EnemyMechanicVocabularyExtensionValidationException(
                    Array.AsReadOnly(issues.ToArray()));
            }

            List<EnemyVocabularyEntrySnapshot> composedEntries = baseInput.Entries
                .Select(CopyEntry)
                .ToList();
            composedEntries.AddRange(extension.Entries.Select(ConvertEntry));
            List<LegacyEnemyVocabularyMappingSnapshot> mappings = baseInput.LegacyMappings
                .Select(CopyMapping)
                .ToList();
            EnemyMechanicVocabularySnapshotInput composed =
                new EnemyMechanicVocabularySnapshotInput(
                    composedEntries,
                    mappings,
                    baseInput.SchemaId,
                    baseInput.SchemaVersion);

            provider.CreateSnapshot(composed);
            return composed;
        }

        private static EnemyVocabularyEntrySnapshot ConvertEntry(
            EnemyMechanicVocabularyExtensionEntrySnapshot value)
        {
            EnemyVocabularyStableKey key;
            switch (value.Category)
            {
                case EnemyVocabularyCategory.Mechanic:
                    key = new MechanicKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.CounterWindowType:
                    key = new CounterWindowTypeKey(value.StableKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value.Category,
                        "The approved extension contains only Mechanic and CounterWindowType entries.");
            }

            return new EnemyVocabularyEntrySnapshot(
                key,
                value.DeveloperLabelZh,
                value.Description,
                value.PlayerVisible,
                value.DeveloperOnly);
        }

        private static EnemyVocabularyEntrySnapshot CopyEntry(EnemyVocabularyEntrySnapshot value)
        {
            EnemyVocabularyStableKey key;
            switch (value.Category)
            {
                case EnemyVocabularyCategory.Mechanic:
                    key = new MechanicKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.BuildCapability:
                    key = new BuildCapabilityKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.PressureChannel:
                    key = new PressureChannelKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.CounterWindowType:
                    key = new CounterWindowTypeKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.PlayerHintCategory:
                    key = new PlayerHintCategoryKey(value.StableKey);
                    break;
                case EnemyVocabularyCategory.DeveloperDiagnosticCategory:
                    key = new DeveloperDiagnosticCategoryKey(value.StableKey);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value.Category,
                        "Unknown base vocabulary category.");
            }

            return new EnemyVocabularyEntrySnapshot(
                key,
                value.DeveloperLabelZh,
                value.Description,
                value.PlayerVisible,
                value.DeveloperOnly);
        }

        private static LegacyEnemyVocabularyMappingSnapshot CopyMapping(
            LegacyEnemyVocabularyMappingSnapshot value)
        {
            return new LegacyEnemyVocabularyMappingSnapshot(
                value.LegacySourceKind,
                value.LegacyKey,
                value.Targets
                    .Select(target => new EnemyVocabularyKeyReferenceSnapshot(
                        target.Category,
                        target.StableKey))
                    .ToArray(),
                value.Status,
                value.Reason);
        }
    }
}
