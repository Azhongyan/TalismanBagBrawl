using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.Items.Capability.RuntimeFacts
{
    public enum ItemCapabilityRuntimeFactTruthStatus
    {
        KnownTrue = 0,
        KnownFalse = 1,
        Unknown = 2,
        Invalid = 3
    }

    public enum ItemCapabilityRuntimeFactCompleteness
    {
        Complete = 0,
        Incomplete = 1
    }

    public sealed class ItemCapabilityRuntimeFactInput
    {
        public ItemCapabilityRuntimeFactInput(
            string battleSessionId,
            string eventId,
            long? eventSequence,
            string itemInstanceId,
            string baseItemId,
            string placementId,
            long? triggerOrdinal,
            bool? triggerSuccess,
            bool? firstTriggerInBattle,
            bool? cleanseSuccess,
            long? cleanseExtraStackCount,
            string cleanseExtraStackUnitKey,
            long? consecutiveTriggerCount,
            string consecutiveTriggerCountUnitKey,
            bool? chainCount3,
            long? nianCostBefore,
            string nianCostBeforeUnitKey,
            long? nianCostAfter,
            string nianCostAfterUnitKey,
            long? refundUnits,
            string refundUnitKey,
            ItemCapabilityRuntimeFactCompleteness? factCompleteness)
        {
            this.battleSessionId = RuntimeFactCanonical.Normalize(battleSessionId);
            this.eventId = RuntimeFactCanonical.Normalize(eventId);
            this.eventSequence = eventSequence;
            this.itemInstanceId = RuntimeFactCanonical.Normalize(itemInstanceId);
            this.baseItemId = RuntimeFactCanonical.Normalize(baseItemId);
            this.placementId = RuntimeFactCanonical.Normalize(placementId);
            this.triggerOrdinal = triggerOrdinal;
            this.triggerSuccess = triggerSuccess;
            this.firstTriggerInBattle = firstTriggerInBattle;
            this.cleanseSuccess = cleanseSuccess;
            this.cleanseExtraStackCount = cleanseExtraStackCount;
            this.cleanseExtraStackUnitKey = RuntimeFactCanonical.Normalize(
                cleanseExtraStackUnitKey);
            this.consecutiveTriggerCount = consecutiveTriggerCount;
            this.consecutiveTriggerCountUnitKey = RuntimeFactCanonical.Normalize(
                consecutiveTriggerCountUnitKey);
            this.chainCount3 = chainCount3;
            this.nianCostBefore = nianCostBefore;
            this.nianCostBeforeUnitKey = RuntimeFactCanonical.Normalize(
                nianCostBeforeUnitKey);
            this.nianCostAfter = nianCostAfter;
            this.nianCostAfterUnitKey = RuntimeFactCanonical.Normalize(
                nianCostAfterUnitKey);
            this.refundUnits = refundUnits;
            this.refundUnitKey = RuntimeFactCanonical.Normalize(refundUnitKey);
            this.factCompleteness = factCompleteness;
        }

        public string battleSessionId { get; }
        public string eventId { get; }
        public long? eventSequence { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string placementId { get; }
        public long? triggerOrdinal { get; }
        public bool? triggerSuccess { get; }
        public bool? firstTriggerInBattle { get; }
        public bool? cleanseSuccess { get; }
        public long? cleanseExtraStackCount { get; }
        public string cleanseExtraStackUnitKey { get; }
        public long? consecutiveTriggerCount { get; }
        public string consecutiveTriggerCountUnitKey { get; }
        public bool? chainCount3 { get; }
        public long? nianCostBefore { get; }
        public string nianCostBeforeUnitKey { get; }
        public long? nianCostAfter { get; }
        public string nianCostAfterUnitKey { get; }
        public long? refundUnits { get; }
        public string refundUnitKey { get; }
        public ItemCapabilityRuntimeFactCompleteness? factCompleteness { get; }
    }

    public interface IItemCapabilityRuntimeFactInputProvider
    {
        IReadOnlyList<ItemCapabilityRuntimeFactInput> GetRuntimeFacts();
    }

    public sealed class ItemCapabilityRuntimeFactValidationError
    {
        public ItemCapabilityRuntimeFactValidationError(
            string code,
            ItemCapabilityRuntimeFactTruthStatus status,
            string battleSessionId,
            string eventId,
            string itemInstanceId,
            string placementId,
            string message)
        {
            this.code = RuntimeFactCanonical.Normalize(code);
            this.status = status;
            this.battleSessionId = RuntimeFactCanonical.Normalize(battleSessionId);
            this.eventId = RuntimeFactCanonical.Normalize(eventId);
            this.itemInstanceId = RuntimeFactCanonical.Normalize(itemInstanceId);
            this.placementId = RuntimeFactCanonical.Normalize(placementId);
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public ItemCapabilityRuntimeFactTruthStatus status { get; }
        public string battleSessionId { get; }
        public string eventId { get; }
        public string itemInstanceId { get; }
        public string placementId { get; }
        public string message { get; }
    }

    public sealed class ItemCapabilityRuntimeFactSnapshot
    {
        internal ItemCapabilityRuntimeFactSnapshot(
            string battleSessionId,
            string eventId,
            long? eventSequence,
            string itemInstanceId,
            string baseItemId,
            string placementId,
            long? triggerOrdinal,
            ItemCapabilityRuntimeFactTruthStatus triggerSuccess,
            ItemCapabilityRuntimeFactTruthStatus firstTriggerInBattle,
            ItemCapabilityRuntimeFactTruthStatus cleanseSuccess,
            long? cleanseExtraStackCount,
            string cleanseExtraStackUnitKey,
            long? consecutiveTriggerCount,
            string consecutiveTriggerCountUnitKey,
            ItemCapabilityRuntimeFactTruthStatus chainCount3,
            long? nianCostBefore,
            string nianCostBeforeUnitKey,
            long? nianCostAfter,
            string nianCostAfterUnitKey,
            long? refundUnits,
            string refundUnitKey,
            ItemCapabilityRuntimeFactCompleteness factCompleteness)
        {
            this.battleSessionId = RuntimeFactCanonical.Normalize(battleSessionId);
            this.eventId = RuntimeFactCanonical.Normalize(eventId);
            this.eventSequence = eventSequence;
            this.itemInstanceId = RuntimeFactCanonical.Normalize(itemInstanceId);
            this.baseItemId = RuntimeFactCanonical.Normalize(baseItemId);
            this.placementId = RuntimeFactCanonical.Normalize(placementId);
            this.triggerOrdinal = triggerOrdinal;
            this.triggerSuccess = triggerSuccess;
            this.firstTriggerInBattle = firstTriggerInBattle;
            this.cleanseSuccess = cleanseSuccess;
            this.cleanseExtraStackCount = cleanseExtraStackCount;
            this.cleanseExtraStackUnitKey = RuntimeFactCanonical.Normalize(
                cleanseExtraStackUnitKey);
            this.consecutiveTriggerCount = consecutiveTriggerCount;
            this.consecutiveTriggerCountUnitKey = RuntimeFactCanonical.Normalize(
                consecutiveTriggerCountUnitKey);
            this.chainCount3 = chainCount3;
            this.nianCostBefore = nianCostBefore;
            this.nianCostBeforeUnitKey = RuntimeFactCanonical.Normalize(
                nianCostBeforeUnitKey);
            this.nianCostAfter = nianCostAfter;
            this.nianCostAfterUnitKey = RuntimeFactCanonical.Normalize(
                nianCostAfterUnitKey);
            this.refundUnits = refundUnits;
            this.refundUnitKey = RuntimeFactCanonical.Normalize(refundUnitKey);
            this.factCompleteness = factCompleteness;
        }

        public string battleSessionId { get; }
        public string eventId { get; }
        public long? eventSequence { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string placementId { get; }
        public long? triggerOrdinal { get; }
        public ItemCapabilityRuntimeFactTruthStatus triggerSuccess { get; }
        public ItemCapabilityRuntimeFactTruthStatus firstTriggerInBattle { get; }
        public ItemCapabilityRuntimeFactTruthStatus cleanseSuccess { get; }
        public long? cleanseExtraStackCount { get; }
        public string cleanseExtraStackUnitKey { get; }
        public long? consecutiveTriggerCount { get; }
        public string consecutiveTriggerCountUnitKey { get; }
        public ItemCapabilityRuntimeFactTruthStatus chainCount3 { get; }
        public long? nianCostBefore { get; }
        public string nianCostBeforeUnitKey { get; }
        public long? nianCostAfter { get; }
        public string nianCostAfterUnitKey { get; }
        public long? refundUnits { get; }
        public string refundUnitKey { get; }
        public ItemCapabilityRuntimeFactCompleteness factCompleteness { get; }
        public bool isValid => triggerSuccess != ItemCapabilityRuntimeFactTruthStatus.Invalid
            && firstTriggerInBattle != ItemCapabilityRuntimeFactTruthStatus.Invalid
            && cleanseSuccess != ItemCapabilityRuntimeFactTruthStatus.Invalid
            && chainCount3 != ItemCapabilityRuntimeFactTruthStatus.Invalid;
    }

    public sealed class ItemCapabilityRuntimeFactContractSnapshot
    {
        public const string CurrentSchemaId =
            "ItemCapabilityRuntimeFactContractSnapshot.v1";

        private readonly ReadOnlyCollection<ItemCapabilityRuntimeFactSnapshot> facts;
        private readonly ReadOnlyCollection<ItemCapabilityRuntimeFactValidationError>
            validationErrors;

        internal ItemCapabilityRuntimeFactContractSnapshot(
            string bindingContractCanonicalSignature,
            string unitContractCanonicalSignature,
            ItemCapabilityRuntimeFactCompleteness factCompleteness,
            IEnumerable<ItemCapabilityRuntimeFactSnapshot> facts,
            IEnumerable<ItemCapabilityRuntimeFactValidationError> validationErrors)
        {
            schemaId = CurrentSchemaId;
            this.bindingContractCanonicalSignature = RuntimeFactCanonical.Normalize(
                bindingContractCanonicalSignature);
            this.unitContractCanonicalSignature = RuntimeFactCanonical.Normalize(
                unitContractCanonicalSignature);
            this.factCompleteness = factCompleteness;
            this.facts = RuntimeFactReadOnly.Freeze((facts ??
                    Array.Empty<ItemCapabilityRuntimeFactSnapshot>())
                .Where(value => value != null)
                .Select(Clone)
                .OrderBy(value => value.battleSessionId, StringComparer.Ordinal)
                .ThenBy(value => value.eventSequence.HasValue ? 0 : 1)
                .ThenBy(value => value.eventSequence.GetValueOrDefault())
                .ThenBy(value => value.eventId, StringComparer.Ordinal)
                .ThenBy(value => value.itemInstanceId, StringComparer.Ordinal));
            this.validationErrors = RuntimeFactReadOnly.Freeze((validationErrors ??
                    Array.Empty<ItemCapabilityRuntimeFactValidationError>())
                .Where(value => value != null)
                .Select(Clone)
                .OrderBy(value => value.status)
                .ThenBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.battleSessionId, StringComparer.Ordinal)
                .ThenBy(value => value.eventId, StringComparer.Ordinal)
                .ThenBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ThenBy(value => value.placementId, StringComparer.Ordinal)
                .ThenBy(value => value.message, StringComparer.Ordinal));
            canonicalSignature = RuntimeFactCanonical.Sha256(BuildCanonicalPayload());
        }

        public string schemaId { get; }
        public string bindingContractCanonicalSignature { get; }
        public string unitContractCanonicalSignature { get; }
        public ItemCapabilityRuntimeFactCompleteness factCompleteness { get; }
        public IReadOnlyList<ItemCapabilityRuntimeFactSnapshot> Facts => facts;
        public IReadOnlyList<ItemCapabilityRuntimeFactValidationError> ValidationErrors =>
            validationErrors;
        public string canonicalSignature { get; }
        public bool hasInvalidFacts => facts.Any(value => !value.isValid)
            || validationErrors.Any(value => value.status ==
                ItemCapabilityRuntimeFactTruthStatus.Invalid);
        public bool isValid => !hasInvalidFacts && validationErrors.Count == 0;

        public ItemCapabilityRuntimeFactSnapshot FindByEventId(string eventId)
        {
            string normalized = RuntimeFactCanonical.Normalize(eventId);
            return facts.FirstOrDefault(value => string.Equals(
                value.eventId, normalized, StringComparison.Ordinal));
        }

        public string BuildCanonicalPayload()
        {
            StringBuilder builder = new StringBuilder();
            RuntimeFactCanonical.AppendField(builder, "schemaId", schemaId);
            RuntimeFactCanonical.AppendField(builder,
                "bindingContractCanonicalSignature",
                bindingContractCanonicalSignature);
            RuntimeFactCanonical.AppendField(builder,
                "unitContractCanonicalSignature",
                unitContractCanonicalSignature);
            RuntimeFactCanonical.AppendField(builder, "factCompleteness",
                factCompleteness.ToString());
            RuntimeFactCanonical.AppendField(builder, "factCount",
                facts.Count.ToString(CultureInfo.InvariantCulture));
            RuntimeFactCanonical.AppendField(builder, "validationErrorCount",
                validationErrors.Count.ToString(CultureInfo.InvariantCulture));

            foreach (ItemCapabilityRuntimeFactSnapshot fact in facts)
            {
                RuntimeFactCanonical.AppendField(builder, "battleSessionId",
                    fact.battleSessionId);
                RuntimeFactCanonical.AppendField(builder, "eventId", fact.eventId);
                RuntimeFactCanonical.AppendField(builder, "eventSequence",
                    RuntimeFactCanonical.NullableLong(fact.eventSequence));
                RuntimeFactCanonical.AppendField(builder, "itemInstanceId",
                    fact.itemInstanceId);
                RuntimeFactCanonical.AppendField(builder, "baseItemId",
                    fact.baseItemId);
                RuntimeFactCanonical.AppendField(builder, "placementId",
                    fact.placementId);
                RuntimeFactCanonical.AppendField(builder, "triggerOrdinal",
                    RuntimeFactCanonical.NullableLong(fact.triggerOrdinal));
                RuntimeFactCanonical.AppendField(builder, "triggerSuccess",
                    fact.triggerSuccess.ToString());
                RuntimeFactCanonical.AppendField(builder, "firstTriggerInBattle",
                    fact.firstTriggerInBattle.ToString());
                RuntimeFactCanonical.AppendField(builder, "cleanseSuccess",
                    fact.cleanseSuccess.ToString());
                RuntimeFactCanonical.AppendField(builder, "cleanseExtraStackCount",
                    RuntimeFactCanonical.NullableLong(fact.cleanseExtraStackCount));
                RuntimeFactCanonical.AppendField(builder,
                    "cleanseExtraStackUnitKey", fact.cleanseExtraStackUnitKey);
                RuntimeFactCanonical.AppendField(builder, "consecutiveTriggerCount",
                    RuntimeFactCanonical.NullableLong(fact.consecutiveTriggerCount));
                RuntimeFactCanonical.AppendField(builder,
                    "consecutiveTriggerCountUnitKey",
                    fact.consecutiveTriggerCountUnitKey);
                RuntimeFactCanonical.AppendField(builder, "chainCount3",
                    fact.chainCount3.ToString());
                RuntimeFactCanonical.AppendField(builder, "nianCostBefore",
                    RuntimeFactCanonical.NullableLong(fact.nianCostBefore));
                RuntimeFactCanonical.AppendField(builder, "nianCostBeforeUnitKey",
                    fact.nianCostBeforeUnitKey);
                RuntimeFactCanonical.AppendField(builder, "nianCostAfter",
                    RuntimeFactCanonical.NullableLong(fact.nianCostAfter));
                RuntimeFactCanonical.AppendField(builder, "nianCostAfterUnitKey",
                    fact.nianCostAfterUnitKey);
                RuntimeFactCanonical.AppendField(builder, "refundUnits",
                    RuntimeFactCanonical.NullableLong(fact.refundUnits));
                RuntimeFactCanonical.AppendField(builder, "refundUnitKey",
                    fact.refundUnitKey);
                RuntimeFactCanonical.AppendField(builder, "factCompleteness",
                    fact.factCompleteness.ToString());
            }

            foreach (ItemCapabilityRuntimeFactValidationError error in validationErrors)
            {
                RuntimeFactCanonical.AppendField(builder, "validationCode", error.code);
                RuntimeFactCanonical.AppendField(builder, "validationStatus",
                    error.status.ToString());
                RuntimeFactCanonical.AppendField(builder,
                    "validationBattleSessionId", error.battleSessionId);
                RuntimeFactCanonical.AppendField(builder, "validationEventId",
                    error.eventId);
                RuntimeFactCanonical.AppendField(builder,
                    "validationItemInstanceId", error.itemInstanceId);
                RuntimeFactCanonical.AppendField(builder, "validationPlacementId",
                    error.placementId);
                RuntimeFactCanonical.AppendField(builder, "validationMessage",
                    error.message);
            }

            return builder.ToString();
        }

        private static ItemCapabilityRuntimeFactSnapshot Clone(
            ItemCapabilityRuntimeFactSnapshot value)
        {
            return new ItemCapabilityRuntimeFactSnapshot(
                value.battleSessionId, value.eventId, value.eventSequence,
                value.itemInstanceId, value.baseItemId, value.placementId,
                value.triggerOrdinal, value.triggerSuccess,
                value.firstTriggerInBattle, value.cleanseSuccess,
                value.cleanseExtraStackCount, value.cleanseExtraStackUnitKey,
                value.consecutiveTriggerCount,
                value.consecutiveTriggerCountUnitKey, value.chainCount3,
                value.nianCostBefore, value.nianCostBeforeUnitKey,
                value.nianCostAfter, value.nianCostAfterUnitKey,
                value.refundUnits, value.refundUnitKey, value.factCompleteness);
        }

        private static ItemCapabilityRuntimeFactValidationError Clone(
            ItemCapabilityRuntimeFactValidationError value)
        {
            return new ItemCapabilityRuntimeFactValidationError(
                value.code, value.status, value.battleSessionId, value.eventId,
                value.itemInstanceId, value.placementId, value.message);
        }
    }

    internal static class RuntimeFactReadOnly
    {
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    internal static class RuntimeFactCanonical
    {
        public const string MissingValue = "<missing>";

        public static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static string NullableLong(long? value)
        {
            return value.HasValue
                ? value.Value.ToString(CultureInfo.InvariantCulture)
                : MissingValue;
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
