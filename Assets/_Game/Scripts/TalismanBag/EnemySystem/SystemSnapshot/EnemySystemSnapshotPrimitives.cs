using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.EnemySystem.SystemSnapshot
{
    public static class EnemySystemSnapshotSchema
    {
        public const string SchemaId = "EnemySystemSnapshot.v1";
        public const int SchemaVersion = 1;
    }

    public enum EnemySystemComponentKind
    {
        E01 = 1,
        E02 = 2,
        E03 = 3,
        E04 = 4,
        E05 = 5,
        E06 = 6,
        E07 = 7,
        E08 = 8
    }

    public enum EnemySystemComponentMode
    {
        StaticEmbedded = 0,
        TransientContract = 1
    }

    public enum EnemySystemValidationCategory
    {
        Schema = 0,
        Identity = 1,
        Reference = 2,
        Relation = 3,
        Isolation = 4,
        Signature = 5,
        PlayerLeak = 6
    }

    public enum EnemySystemIdentityKind
    {
        Enemy = 0,
        Boss = 1,
        MechanicProfile = 2,
        MapRule = 3,
        Encounter = 4,
        SkillPattern = 5,
        SkillSequence = 6,
        BossPhase = 7,
        BossPhasePlan = 8,
        BuildPressureProfile = 9,
        CounterWindow = 10,
        OptionalRegistry = 11,
        EncounterSlot = 12,
        CarrierSkillBinding = 13,
        PressureSource = 14,
        CounterWindowSource = 15
    }

    public enum EnemySystemRelationKind
    {
        CarrierMechanicProfile = 0,
        CarrierSkillPattern = 1,
        BossPhaseReference = 2,
        OptionalRegistryReference = 3,
        NormalizedCarrierMechanicProfile = 4,
        MapRuleMechanicProfile = 5,
        EncounterMapRule = 6,
        EncounterSlotCarrier = 7,
        EncounterSlotMechanicProfile = 8,
        SkillPatternMechanicProfile = 9,
        SkillSequencePattern = 10,
        CarrierSkillSequence = 11,
        BossPhaseSequence = 12,
        BossPlanPhase = 13,
        PressureSourceTarget = 14,
        CounterWindowSourceTarget = 15,
        PressureCounterWindow = 16
    }

    public sealed class EnemySystemSchemaManifestEntry
    {
        public EnemySystemSchemaManifestEntry(
            string componentId,
            string schemaId,
            int schemaVersion,
            EnemySystemComponentMode componentMode,
            bool embeddedInRoot,
            string componentCanonicalSignature,
            string playerSafeCanonicalSignature,
            string contractCanonicalSignature)
        {
            ComponentId = EnemySystemSnapshotReadOnly.Text(componentId);
            SchemaId = EnemySystemSnapshotReadOnly.Text(schemaId);
            SchemaVersion = schemaVersion;
            ComponentMode = componentMode;
            EmbeddedInRoot = embeddedInRoot;
            ComponentCanonicalSignature = EnemySystemSnapshotReadOnly.Text(componentCanonicalSignature);
            PlayerSafeCanonicalSignature = EnemySystemSnapshotReadOnly.Text(playerSafeCanonicalSignature);
            ContractCanonicalSignature = EnemySystemSnapshotReadOnly.Text(contractCanonicalSignature);
        }

        public string ComponentId { get; }
        public string SchemaId { get; }
        public int SchemaVersion { get; }
        public EnemySystemComponentMode ComponentMode { get; }
        public bool EmbeddedInRoot { get; }
        public string ComponentCanonicalSignature { get; }
        public string PlayerSafeCanonicalSignature { get; }
        public string ContractCanonicalSignature { get; }
    }

    public sealed class EnemySystemIdentitySnapshot :
        TalismanBag.EnemySystem.Domain.IEnemyDomainIsolationMetadata
    {
        public EnemySystemIdentitySnapshot(
            EnemySystemIdentityKind identityKind,
            string stableId,
            string owningComponentId,
            bool devOnly,
            bool isEnabled,
            bool entersFormalFlow)
        {
            IdentityKind = identityKind;
            StableId = EnemySystemSnapshotReadOnly.Text(stableId);
            OwningComponentId = EnemySystemSnapshotReadOnly.Text(owningComponentId);
            DevOnly = devOnly;
            IsEnabled = isEnabled;
            EntersFormalFlow = entersFormalFlow;
        }

        public EnemySystemIdentityKind IdentityKind { get; }
        public string StableId { get; }
        public string OwningComponentId { get; }
        public bool DevOnly { get; }
        public bool IsEnabled { get; }
        public bool EntersFormalFlow { get; }
        public bool DeveloperOnly => true;
    }

    public sealed class EnemySystemRelationSnapshot
    {
        public EnemySystemRelationSnapshot(
            EnemySystemIdentityKind sourceKind,
            string sourceId,
            EnemySystemRelationKind relationKind,
            EnemySystemIdentityKind targetKind,
            string targetId,
            string owningComponentId,
            bool resolved,
            bool isolationCompatible)
        {
            SourceKind = sourceKind;
            SourceId = EnemySystemSnapshotReadOnly.Text(sourceId);
            RelationKind = relationKind;
            TargetKind = targetKind;
            TargetId = EnemySystemSnapshotReadOnly.Text(targetId);
            OwningComponentId = EnemySystemSnapshotReadOnly.Text(owningComponentId);
            Resolved = resolved;
            IsolationCompatible = isolationCompatible;
        }

        public EnemySystemIdentityKind SourceKind { get; }
        public string SourceId { get; }
        public EnemySystemRelationKind RelationKind { get; }
        public EnemySystemIdentityKind TargetKind { get; }
        public string TargetId { get; }
        public string OwningComponentId { get; }
        public bool Resolved { get; }
        public bool IsolationCompatible { get; }
        public bool DeveloperOnly => true;
    }

    internal static class EnemySystemSnapshotReadOnly
    {
        public static string Text(string value)
        {
            return value ?? string.Empty;
        }

        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    internal static class EnemySystemSnapshotCanonical
    {
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

        public static bool IsSignature(string value)
        {
            return value != null
                && value.Length == 71
                && value.StartsWith("sha256:", StringComparison.Ordinal)
                && value.Substring(7).All(character =>
                    (character >= '0' && character <= '9')
                    || (character >= 'a' && character <= 'f'));
        }

        public static void Field(StringBuilder builder, string name, string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeName).Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append(';');
        }

        public static void Rows(StringBuilder builder, string name, IEnumerable<string> values)
        {
            string[] rows = (values ?? Array.Empty<string>())
                .Select(value => value ?? string.Empty)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Field(builder, name + ".count", rows.Length.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < rows.Length; index++)
            {
                Field(builder, name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]", rows[index]);
            }
        }

        public static string Isolation(bool devOnly, bool isEnabled, bool entersFormalFlow)
        {
            return (devOnly ? "1" : "0") + "|" + (isEnabled ? "1" : "0") + "|" + (entersFormalFlow ? "1" : "0");
        }

        public static string ManifestContract(
            string schemaId,
            int schemaVersion,
            EnemySystemComponentMode mode,
            bool embedded)
        {
            StringBuilder builder = new StringBuilder();
            Field(builder, "schemaId", schemaId);
            Field(builder, "schemaVersion", schemaVersion.ToString(CultureInfo.InvariantCulture));
            Field(builder, "mode", ((int)mode).ToString(CultureInfo.InvariantCulture));
            Field(builder, "embedded", embedded ? "1" : "0");
            return Hash(builder.ToString());
        }
    }
}
