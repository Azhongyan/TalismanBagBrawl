using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.Items.Capability
{
    public enum ItemCapabilityUnitResolutionStatus
    {
        KnownValue = 0,
        KnownZero = 1,
        Unknown = 2,
        Invalid = 3
    }

    public sealed class ItemCapabilityUnitDefinition
    {
        public ItemCapabilityUnitDefinition(
            string capabilityId,
            string valueDomainId,
            string unitKey,
            string operationKey,
            long minKnownValueUnits,
            long maxKnownValueUnits,
            bool allowsExplicitZero = true)
        {
            this.capabilityId = ItemCapabilityUnitCanonical.Normalize(capabilityId);
            this.valueDomainId = ItemCapabilityUnitCanonical.Normalize(valueDomainId);
            this.unitKey = ItemCapabilityUnitCanonical.Normalize(unitKey);
            this.operationKey = ItemCapabilityUnitCanonical.Normalize(operationKey);
            this.minKnownValueUnits = minKnownValueUnits;
            this.maxKnownValueUnits = maxKnownValueUnits;
            this.allowsExplicitZero = allowsExplicitZero;
        }

        public string capabilityId { get; }
        public string valueDomainId { get; }
        public string unitKey { get; }
        public string operationKey { get; }
        public long minKnownValueUnits { get; }
        public long maxKnownValueUnits { get; }
        public bool allowsExplicitZero { get; }
    }

    public sealed class ItemCapabilityUnitContractSnapshot
    {
        public const string CurrentSchemaId = "ItemCapabilityUnitContractSnapshot.v1";

        private readonly ReadOnlyCollection<ItemCapabilityUnitDefinition> definitions;

        internal ItemCapabilityUnitContractSnapshot(
            IEnumerable<ItemCapabilityUnitDefinition> source)
        {
            schemaId = CurrentSchemaId;
            definitions = Array.AsReadOnly((source ??
                    Array.Empty<ItemCapabilityUnitDefinition>())
                .Where(value => value != null)
                .Select(Clone)
                .OrderBy(value => value.capabilityId, StringComparer.Ordinal)
                .ThenBy(value => value.valueDomainId, StringComparer.Ordinal)
                .ThenBy(value => value.unitKey, StringComparer.Ordinal)
                .ThenBy(value => value.operationKey, StringComparer.Ordinal)
                .ToArray());
            canonicalSignature = ItemCapabilityUnitCanonical.Sha256(BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public IReadOnlyList<ItemCapabilityUnitDefinition> Definitions => definitions;
        public string canonicalSignature { get; }

        public ItemCapabilityUnitDefinition Find(string capabilityId)
        {
            string normalized = ItemCapabilityUnitCanonical.Normalize(capabilityId);
            return definitions.FirstOrDefault(value => string.Equals(
                value.capabilityId,
                normalized,
                StringComparison.Ordinal));
        }

        public string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            ItemCapabilityUnitCanonical.AppendField(builder, "schemaId", schemaId);
            ItemCapabilityUnitCanonical.AppendField(builder, "definitionCount",
                definitions.Count.ToString(CultureInfo.InvariantCulture));
            foreach (ItemCapabilityUnitDefinition definition in definitions)
            {
                ItemCapabilityUnitCanonical.AppendField(builder,
                    "capabilityId", definition.capabilityId);
                ItemCapabilityUnitCanonical.AppendField(builder,
                    "valueDomainId", definition.valueDomainId);
                ItemCapabilityUnitCanonical.AppendField(builder,
                    "unitKey", definition.unitKey);
                ItemCapabilityUnitCanonical.AppendField(builder,
                    "operationKey", definition.operationKey);
                ItemCapabilityUnitCanonical.AppendField(builder,
                    "minKnownValueUnits",
                    definition.minKnownValueUnits.ToString(CultureInfo.InvariantCulture));
                ItemCapabilityUnitCanonical.AppendField(builder,
                    "maxKnownValueUnits",
                    definition.maxKnownValueUnits.ToString(CultureInfo.InvariantCulture));
                ItemCapabilityUnitCanonical.AppendField(builder,
                    "allowsExplicitZero",
                    definition.allowsExplicitZero ? "true" : "false");
            }

            return builder.ToString();
        }

        private static ItemCapabilityUnitDefinition Clone(
            ItemCapabilityUnitDefinition value)
        {
            return new ItemCapabilityUnitDefinition(
                value.capabilityId,
                value.valueDomainId,
                value.unitKey,
                value.operationKey,
                value.minKnownValueUnits,
                value.maxKnownValueUnits,
                value.allowsExplicitZero);
        }
    }

    public static class ItemCapabilityUnitContract
    {
        private static readonly ItemCapabilityUnitContractSnapshot DefaultSnapshot =
            Create(new[]
            {
                Definition("affix_control_up", "control_point", "point",
                    "AddFlat", 1, 10),
                Definition("affix_cleanse_up", "cleanse_stack", "stack",
                    "AddFlat", 1, 5),
                Definition("affix_chain_target", "target_count", "count",
                    "ExtraTarget", 1, 6),
                Definition("affix_trigger_refund", "nian_point", "point",
                    "Refund", 1, 6),
                Definition("affix_first_trigger_bonus", "effect_basis_point",
                    "basisPoint", "ExtraTrigger", 3000, 8000),
                Definition("affix_nian_efficiency", "nian_cost_basis_point",
                    "basisPoint", "ReducePercent", 200, 1800)
            });

        public static ItemCapabilityUnitContractSnapshot CreateDefault()
        {
            return new ItemCapabilityUnitContractSnapshot(DefaultSnapshot.Definitions);
        }

        public static ItemCapabilityUnitContractSnapshot Create(
            IEnumerable<ItemCapabilityUnitDefinition> definitions)
        {
            ItemCapabilityUnitDefinition[] values = (definitions ??
                    Array.Empty<ItemCapabilityUnitDefinition>())
                .Where(value => value != null)
                .ToArray();
            if (values.Length == 0)
            {
                throw new ArgumentException(
                    "At least one explicit capability unit definition is required.",
                    nameof(definitions));
            }

            if (values.Any(value => string.IsNullOrWhiteSpace(value.capabilityId)
                    || string.IsNullOrWhiteSpace(value.valueDomainId)
                    || string.IsNullOrWhiteSpace(value.unitKey)
                    || string.IsNullOrWhiteSpace(value.operationKey)
                    || value.minKnownValueUnits <= 0
                    || value.maxKnownValueUnits < value.minKnownValueUnits))
            {
                throw new ArgumentException(
                    "Capability, domain, unit, operation, and a positive ordered range are required.",
                    nameof(definitions));
            }

            if (values.GroupBy(value => value.capabilityId, StringComparer.Ordinal)
                .Any(group => group.Count() != 1))
            {
                throw new ArgumentException(
                    "capabilityId must be unique under Ordinal comparison.",
                    nameof(definitions));
            }

            return new ItemCapabilityUnitContractSnapshot(values);
        }

        private static ItemCapabilityUnitDefinition Definition(
            string capabilityId,
            string valueDomainId,
            string unitKey,
            string operationKey,
            long minUnits,
            long maxUnits)
        {
            return new ItemCapabilityUnitDefinition(
                capabilityId,
                valueDomainId,
                unitKey,
                operationKey,
                minUnits,
                maxUnits,
                true);
        }
    }

    internal static class ItemCapabilityUnitCanonical
    {
        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static void AppendField(StringBuilder builder, string key, string value)
        {
            string safeKey = key ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeKey.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeKey).Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append('\n');
        }

        public static string Sha256(string payload)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] digest = sha.ComputeHash(
                    Encoding.UTF8.GetBytes(payload ?? string.Empty));
                return "sha256:" + BitConverter.ToString(digest)
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }
    }
}
