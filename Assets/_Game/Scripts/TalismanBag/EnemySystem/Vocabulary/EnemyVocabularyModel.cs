using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.Vocabulary
{
    public static class EnemyMechanicVocabularySchema
    {
        public const string SchemaId = "EnemyMechanicVocabulary.v1";
        public const int SchemaVersion = 1;
    }

    public enum EnemyVocabularyCategory
    {
        Mechanic = 0,
        BuildCapability = 1,
        PressureChannel = 2,
        CounterWindowType = 3,
        PlayerHintCategory = 4,
        DeveloperDiagnosticCategory = 5
    }

    public static class EnemyVocabularyCategoryNames
    {
        public static string StableName(EnemyVocabularyCategory category)
        {
            switch (category)
            {
                case EnemyVocabularyCategory.Mechanic:
                    return "Mechanic";
                case EnemyVocabularyCategory.BuildCapability:
                    return "BuildCapability";
                case EnemyVocabularyCategory.PressureChannel:
                    return "PressureChannel";
                case EnemyVocabularyCategory.CounterWindowType:
                    return "CounterWindowType";
                case EnemyVocabularyCategory.PlayerHintCategory:
                    return "PlayerHintCategory";
                case EnemyVocabularyCategory.DeveloperDiagnosticCategory:
                    return "DeveloperDiagnosticCategory";
                default:
                    return "Unknown";
            }
        }

        public static string RequiredPrefix(EnemyVocabularyCategory category)
        {
            switch (category)
            {
                case EnemyVocabularyCategory.Mechanic:
                    return "mechanic.";
                case EnemyVocabularyCategory.BuildCapability:
                    return "capability.";
                case EnemyVocabularyCategory.PressureChannel:
                    return "pressure.";
                case EnemyVocabularyCategory.CounterWindowType:
                    return "counter_window.";
                case EnemyVocabularyCategory.PlayerHintCategory:
                    return "player_hint.";
                case EnemyVocabularyCategory.DeveloperDiagnosticCategory:
                    return "diagnostic.";
                default:
                    return string.Empty;
            }
        }
    }

    public abstract class EnemyVocabularyStableKey : IEquatable<EnemyVocabularyStableKey>
    {
        protected EnemyVocabularyStableKey(string value, EnemyVocabularyCategory category)
        {
            Value = value ?? string.Empty;
            Category = category;
        }

        public string Value { get; }
        public EnemyVocabularyCategory Category { get; }

        public bool Equals(EnemyVocabularyStableKey other)
        {
            return other != null
                && Category == other.Category
                && string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EnemyVocabularyStableKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Category * 397) ^ StringComparer.Ordinal.GetHashCode(Value);
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public sealed class MechanicKey : EnemyVocabularyStableKey
    {
        public MechanicKey(string value) : base(value, EnemyVocabularyCategory.Mechanic) { }
    }

    public sealed class BuildCapabilityKey : EnemyVocabularyStableKey
    {
        public BuildCapabilityKey(string value) : base(value, EnemyVocabularyCategory.BuildCapability) { }
    }

    public sealed class PressureChannelKey : EnemyVocabularyStableKey
    {
        public PressureChannelKey(string value) : base(value, EnemyVocabularyCategory.PressureChannel) { }
    }

    public sealed class CounterWindowTypeKey : EnemyVocabularyStableKey
    {
        public CounterWindowTypeKey(string value) : base(value, EnemyVocabularyCategory.CounterWindowType) { }
    }

    public sealed class PlayerHintCategoryKey : EnemyVocabularyStableKey
    {
        public PlayerHintCategoryKey(string value) : base(value, EnemyVocabularyCategory.PlayerHintCategory) { }
    }

    public sealed class DeveloperDiagnosticCategoryKey : EnemyVocabularyStableKey
    {
        public DeveloperDiagnosticCategoryKey(string value) : base(value, EnemyVocabularyCategory.DeveloperDiagnosticCategory) { }
    }

    internal static class EnemyVocabularyKeyFactory
    {
        public static EnemyVocabularyStableKey Create(EnemyVocabularyCategory category, string value)
        {
            switch (category)
            {
                case EnemyVocabularyCategory.Mechanic:
                    return new MechanicKey(value);
                case EnemyVocabularyCategory.BuildCapability:
                    return new BuildCapabilityKey(value);
                case EnemyVocabularyCategory.PressureChannel:
                    return new PressureChannelKey(value);
                case EnemyVocabularyCategory.CounterWindowType:
                    return new CounterWindowTypeKey(value);
                case EnemyVocabularyCategory.PlayerHintCategory:
                    return new PlayerHintCategoryKey(value);
                case EnemyVocabularyCategory.DeveloperDiagnosticCategory:
                    return new DeveloperDiagnosticCategoryKey(value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown vocabulary category.");
            }
        }
    }

    public sealed class EnemyVocabularyEntrySnapshot
    {
        public EnemyVocabularyEntrySnapshot(
            EnemyVocabularyStableKey key,
            string developerLabelZh,
            string description,
            bool playerVisible,
            bool developerOnly)
        {
            Key = key == null ? null : EnemyVocabularyKeyFactory.Create(key.Category, key.Value);
            DeveloperLabelZh = developerLabelZh ?? string.Empty;
            Description = description ?? string.Empty;
            PlayerVisible = playerVisible;
            DeveloperOnly = developerOnly;
        }

        public EnemyVocabularyStableKey Key { get; }
        public EnemyVocabularyCategory Category => Key == null ? EnemyVocabularyCategory.Mechanic : Key.Category;
        public string StableKey => Key == null ? string.Empty : Key.Value;
        public string DeveloperLabelZh { get; }
        public string Description { get; }
        public bool PlayerVisible { get; }
        public bool DeveloperOnly { get; }

        internal EnemyVocabularyEntrySnapshot Clone()
        {
            return new EnemyVocabularyEntrySnapshot(Key, DeveloperLabelZh, Description, PlayerVisible, DeveloperOnly);
        }
    }

    public sealed class EnemyVocabularyKeyReferenceSnapshot
    {
        public EnemyVocabularyKeyReferenceSnapshot(EnemyVocabularyCategory category, string stableKey)
        {
            Category = category;
            StableKey = stableKey ?? string.Empty;
        }

        public EnemyVocabularyCategory Category { get; }
        public string StableKey { get; }

        internal EnemyVocabularyKeyReferenceSnapshot Clone()
        {
            return new EnemyVocabularyKeyReferenceSnapshot(Category, StableKey);
        }
    }

    public enum LegacyEnemyVocabularyMappingStatus
    {
        Mapped = 0,
        OutOfScope = 1
    }

    public sealed class LegacyEnemyVocabularyMappingSnapshot
    {
        private readonly ReadOnlyCollection<EnemyVocabularyKeyReferenceSnapshot> targets;

        public LegacyEnemyVocabularyMappingSnapshot(
            string legacySourceKind,
            string legacyKey,
            IReadOnlyList<EnemyVocabularyKeyReferenceSnapshot> targets,
            LegacyEnemyVocabularyMappingStatus status,
            string reason)
        {
            LegacySourceKind = legacySourceKind ?? string.Empty;
            LegacyKey = legacyKey ?? string.Empty;
            this.targets = Array.AsReadOnly((targets ?? Array.Empty<EnemyVocabularyKeyReferenceSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
            Status = status;
            Reason = reason ?? string.Empty;
        }

        public string LegacySourceKind { get; }
        public string LegacyKey { get; }
        public IReadOnlyList<EnemyVocabularyKeyReferenceSnapshot> Targets => targets;
        public LegacyEnemyVocabularyMappingStatus Status { get; }
        public string Reason { get; }
        public string MappingMode => Status == LegacyEnemyVocabularyMappingStatus.OutOfScope
            ? "OUT_OF_SCOPE"
            : Targets.Count > 1 ? "ONE_TO_MANY" : "ONE_TO_ONE";

        internal LegacyEnemyVocabularyMappingSnapshot Clone()
        {
            return new LegacyEnemyVocabularyMappingSnapshot(LegacySourceKind, LegacyKey, targets, Status, Reason);
        }
    }

    public sealed class EnemyMechanicVocabularySnapshotInput
    {
        private readonly ReadOnlyCollection<EnemyVocabularyEntrySnapshot> entries;
        private readonly ReadOnlyCollection<LegacyEnemyVocabularyMappingSnapshot> legacyMappings;

        public EnemyMechanicVocabularySnapshotInput(
            IReadOnlyList<EnemyVocabularyEntrySnapshot> entries,
            IReadOnlyList<LegacyEnemyVocabularyMappingSnapshot> legacyMappings,
            string schemaId = EnemyMechanicVocabularySchema.SchemaId,
            int schemaVersion = EnemyMechanicVocabularySchema.SchemaVersion)
        {
            SchemaId = schemaId ?? string.Empty;
            SchemaVersion = schemaVersion;
            this.entries = Array.AsReadOnly((entries ?? Array.Empty<EnemyVocabularyEntrySnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
            this.legacyMappings = Array.AsReadOnly((legacyMappings ?? Array.Empty<LegacyEnemyVocabularyMappingSnapshot>())
                .Select(value => value == null ? null : value.Clone())
                .ToArray());
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EnemyVocabularyEntrySnapshot> Entries => entries;
        public IReadOnlyList<LegacyEnemyVocabularyMappingSnapshot> LegacyMappings => legacyMappings;
    }

    public interface IEnemyMechanicVocabularyLookup
    {
        bool TryGetEntry(EnemyVocabularyCategory category, string stableKey, out EnemyVocabularyEntrySnapshot entry);
        bool TryGetLegacyMapping(string legacySourceKind, string legacyKey, out LegacyEnemyVocabularyMappingSnapshot mapping);
    }

    public sealed class EnemyMechanicVocabularySnapshot : IEnemyMechanicVocabularyLookup
    {
        private readonly ReadOnlyCollection<EnemyVocabularyEntrySnapshot> entries;
        private readonly ReadOnlyCollection<LegacyEnemyVocabularyMappingSnapshot> legacyMappings;
        private readonly IReadOnlyDictionary<string, EnemyVocabularyEntrySnapshot> entryByIdentity;
        private readonly IReadOnlyDictionary<string, LegacyEnemyVocabularyMappingSnapshot> mappingByIdentity;

        internal EnemyMechanicVocabularySnapshot(EnemyMechanicVocabularySnapshotInput input)
        {
            SchemaId = input.SchemaId;
            SchemaVersion = input.SchemaVersion;
            entries = Array.AsReadOnly(input.Entries.Select(value => value.Clone()).ToArray());
            legacyMappings = Array.AsReadOnly(input.LegacyMappings.Select(value => value.Clone()).ToArray());
            entryByIdentity = new ReadOnlyDictionary<string, EnemyVocabularyEntrySnapshot>(entries.ToDictionary(
                value => EntryIdentity(value.Category, value.StableKey),
                value => value,
                StringComparer.Ordinal));
            mappingByIdentity = new ReadOnlyDictionary<string, LegacyEnemyVocabularyMappingSnapshot>(legacyMappings.ToDictionary(
                value => MappingIdentity(value.LegacySourceKind, value.LegacyKey),
                value => value,
                StringComparer.Ordinal));
        }

        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public IReadOnlyList<EnemyVocabularyEntrySnapshot> Entries => entries;
        public IReadOnlyList<LegacyEnemyVocabularyMappingSnapshot> LegacyMappings => legacyMappings;

        public bool TryGetEntry(EnemyVocabularyCategory category, string stableKey, out EnemyVocabularyEntrySnapshot entry)
        {
            if (stableKey == null)
            {
                entry = null;
                return false;
            }

            return entryByIdentity.TryGetValue(EntryIdentity(category, stableKey), out entry);
        }

        public bool TryGetLegacyMapping(string legacySourceKind, string legacyKey, out LegacyEnemyVocabularyMappingSnapshot mapping)
        {
            if (legacySourceKind == null || legacyKey == null)
            {
                mapping = null;
                return false;
            }

            return mappingByIdentity.TryGetValue(MappingIdentity(legacySourceKind, legacyKey), out mapping);
        }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new StringBuilder(32768);
            EnemyVocabularyCanonical.Field(builder, "schemaId", SchemaId);
            EnemyVocabularyCanonical.Field(builder, "schemaVersion", SchemaVersion.ToString(CultureInfo.InvariantCulture));
            EnemyVocabularyCanonical.Collection(builder, "entries", entries, EnemyVocabularyCanonical.Entry);
            EnemyVocabularyCanonical.Collection(builder, "legacyMappings", legacyMappings, EnemyVocabularyCanonical.Mapping);
            return EnemyVocabularyCanonical.Hash(builder.ToString());
        }

        internal static string EntryIdentity(EnemyVocabularyCategory category, string stableKey)
        {
            return ((int)category).ToString(CultureInfo.InvariantCulture) + "\u001f" + (stableKey ?? string.Empty);
        }

        internal static string MappingIdentity(string sourceKind, string legacyKey)
        {
            return (sourceKind ?? string.Empty) + "\u001f" + (legacyKey ?? string.Empty);
        }
    }

    internal static class EnemyVocabularyCanonical
    {
        public static string Entry(EnemyVocabularyEntrySnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "category", ((int)value.Category).ToString(CultureInfo.InvariantCulture));
            Field(builder, "stableKey", value.StableKey);
            Field(builder, "developerLabelZh", value.DeveloperLabelZh);
            Field(builder, "description", value.Description);
            Field(builder, "playerVisible", value.PlayerVisible ? "1" : "0");
            Field(builder, "developerOnly", value.DeveloperOnly ? "1" : "0");
            return builder.ToString();
        }

        public static string Mapping(LegacyEnemyVocabularyMappingSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "legacySourceKind", value.LegacySourceKind);
            Field(builder, "legacyKey", value.LegacyKey);
            Field(builder, "status", ((int)value.Status).ToString(CultureInfo.InvariantCulture));
            Field(builder, "reason", value.Reason);
            Collection(builder, "targets", value.Targets, Target);
            return builder.ToString();
        }

        public static string Target(EnemyVocabularyKeyReferenceSnapshot value)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "category", ((int)value.Category).ToString(CultureInfo.InvariantCulture));
            Field(builder, "stableKey", value.StableKey);
            return builder.ToString();
        }

        public static void Collection<T>(StringBuilder builder, string name, IEnumerable<T> values, Func<T, string> canonical)
        {
            string[] rows = (values ?? Array.Empty<T>())
                .Select(value => value == null ? "<null>" : canonical(value))
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(builder, name + ".count", rows.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(builder, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", rows[index]);
            }
        }

        public static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeName)
                .Append('=').Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append(';');
        }

        public static string Hash(string payload)
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
