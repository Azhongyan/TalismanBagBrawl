using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.Items.Capability
{
    public static class ItemCapabilityUnitValidationCodes
    {
        public const string None = "NONE";
        public const string ContractMissing = "CONTRACT_MISSING";
        public const string CapabilityMissing = "CAPABILITY_MISSING";
        public const string CapabilityUnknown = "CAPABILITY_UNKNOWN";
        public const string ValueMissing = "VALUE_MISSING";
        public const string UnitMissing = "UNIT_MISSING";
        public const string OperationMissing = "OPERATION_MISSING";
        public const string UnitConflict = "UNIT_CONFLICT";
        public const string OperationConflict = "OPERATION_CONFLICT";
        public const string NegativeValue = "NEGATIVE_VALUE";
        public const string ValueOutOfRange = "VALUE_OUT_OF_RANGE";
        public const string ExplicitZeroForbidden = "EXPLICIT_ZERO_FORBIDDEN";
    }

    public sealed class ItemCapabilityUnitValueInput
    {
        public ItemCapabilityUnitValueInput(
            string capabilityId,
            string unitKey,
            string operationKey,
            long? valueUnits)
        {
            this.capabilityId = ItemCapabilityUnitCanonical.Normalize(capabilityId);
            this.unitKey = ItemCapabilityUnitCanonical.Normalize(unitKey);
            this.operationKey = ItemCapabilityUnitCanonical.Normalize(operationKey);
            this.valueUnits = valueUnits;
        }

        public string capabilityId { get; }
        public string unitKey { get; }
        public string operationKey { get; }
        public long? valueUnits { get; }
    }

    public sealed class ItemCapabilityUnitValidationError
    {
        public ItemCapabilityUnitValidationError(string code, string message)
        {
            this.code = ItemCapabilityUnitCanonical.Normalize(code);
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemCapabilityUnitValidationResult
    {
        private readonly ReadOnlyCollection<ItemCapabilityUnitValidationError> errors;

        internal ItemCapabilityUnitValidationResult(
            ItemCapabilityUnitResolutionStatus status,
            ItemCapabilityUnitDefinition definition,
            long? valueUnits,
            IEnumerable<ItemCapabilityUnitValidationError> errors)
        {
            this.status = status;
            this.definition = definition == null
                ? null
                : new ItemCapabilityUnitDefinition(
                    definition.capabilityId,
                    definition.valueDomainId,
                    definition.unitKey,
                    definition.operationKey,
                    definition.minKnownValueUnits,
                    definition.maxKnownValueUnits,
                    definition.allowsExplicitZero);
            this.valueUnits = valueUnits;
            this.errors = Array.AsReadOnly((errors ??
                    Array.Empty<ItemCapabilityUnitValidationError>())
                .Where(value => value != null)
                .Select(value => new ItemCapabilityUnitValidationError(
                    value.code, value.message))
                .OrderBy(value => value.code, StringComparer.Ordinal)
                .ThenBy(value => value.message, StringComparer.Ordinal)
                .ToArray());
        }

        public ItemCapabilityUnitResolutionStatus status { get; }
        public ItemCapabilityUnitDefinition definition { get; }
        public long? valueUnits { get; }
        public IReadOnlyList<ItemCapabilityUnitValidationError> ValidationErrors => errors;
        public string primaryValidationCode => errors.Count == 0
            ? ItemCapabilityUnitValidationCodes.None
            : errors[0].code;
    }

    public static class ItemCapabilityUnitValidator
    {
        public static ItemCapabilityUnitValidationResult Resolve(
            ItemCapabilityUnitContractSnapshot contract,
            ItemCapabilityUnitValueInput input)
        {
            if (contract == null)
            {
                return Result(ItemCapabilityUnitResolutionStatus.Unknown, null, null,
                    ItemCapabilityUnitValidationCodes.ContractMissing,
                    "ItemCapabilityUnitContractSnapshot.v1 is required.");
            }

            if (input == null || string.IsNullOrWhiteSpace(input.capabilityId))
            {
                return Result(ItemCapabilityUnitResolutionStatus.Unknown, null,
                    input?.valueUnits,
                    ItemCapabilityUnitValidationCodes.CapabilityMissing,
                    "capabilityId is required.");
            }

            ItemCapabilityUnitDefinition definition = contract.Find(input.capabilityId);
            if (definition == null)
            {
                return Result(ItemCapabilityUnitResolutionStatus.Unknown, null,
                    input.valueUnits,
                    ItemCapabilityUnitValidationCodes.CapabilityUnknown,
                    "No explicit native unit contract exists for capabilityId.");
            }

            if (string.IsNullOrWhiteSpace(input.unitKey))
            {
                return Result(ItemCapabilityUnitResolutionStatus.Unknown, definition,
                    input.valueUnits,
                    ItemCapabilityUnitValidationCodes.UnitMissing,
                    "unitKey is required.");
            }

            if (string.IsNullOrWhiteSpace(input.operationKey))
            {
                return Result(ItemCapabilityUnitResolutionStatus.Unknown, definition,
                    input.valueUnits,
                    ItemCapabilityUnitValidationCodes.OperationMissing,
                    "operationKey is required.");
            }

            if (!input.valueUnits.HasValue)
            {
                return Result(ItemCapabilityUnitResolutionStatus.Unknown, definition, null,
                    ItemCapabilityUnitValidationCodes.ValueMissing,
                    "valueUnits is missing; it must not be represented as zero.");
            }

            if (!string.Equals(input.unitKey, definition.unitKey,
                StringComparison.Ordinal))
            {
                return Result(ItemCapabilityUnitResolutionStatus.Invalid, definition,
                    input.valueUnits,
                    ItemCapabilityUnitValidationCodes.UnitConflict,
                    "unitKey conflicts with the explicit native unit domain.");
            }

            if (!string.Equals(input.operationKey, definition.operationKey,
                StringComparison.Ordinal))
            {
                return Result(ItemCapabilityUnitResolutionStatus.Invalid, definition,
                    input.valueUnits,
                    ItemCapabilityUnitValidationCodes.OperationConflict,
                    "operationKey conflicts with the explicit native contract.");
            }

            long value = input.valueUnits.Value;
            if (value < 0)
            {
                return Result(ItemCapabilityUnitResolutionStatus.Invalid, definition, value,
                    ItemCapabilityUnitValidationCodes.NegativeValue,
                    "Negative values are invalid.");
            }

            if (value == 0)
            {
                return definition.allowsExplicitZero
                    ? Result(ItemCapabilityUnitResolutionStatus.KnownZero,
                        definition, value, null, null)
                    : Result(ItemCapabilityUnitResolutionStatus.Invalid,
                        definition, value,
                        ItemCapabilityUnitValidationCodes.ExplicitZeroForbidden,
                        "This native capability domain does not allow explicit zero.");
            }

            if (value < definition.minKnownValueUnits
                || value > definition.maxKnownValueUnits)
            {
                return Result(ItemCapabilityUnitResolutionStatus.Invalid, definition, value,
                    ItemCapabilityUnitValidationCodes.ValueOutOfRange,
                    "Positive valueUnits is outside the declared native range.");
            }

            return Result(ItemCapabilityUnitResolutionStatus.KnownValue,
                definition, value, null, null);
        }

        private static ItemCapabilityUnitValidationResult Result(
            ItemCapabilityUnitResolutionStatus status,
            ItemCapabilityUnitDefinition definition,
            long? valueUnits,
            string code,
            string message)
        {
            ItemCapabilityUnitValidationError[] errors = string.IsNullOrWhiteSpace(code)
                ? Array.Empty<ItemCapabilityUnitValidationError>()
                : new[] { new ItemCapabilityUnitValidationError(code, message) };
            return new ItemCapabilityUnitValidationResult(
                status, definition, valueUnits, errors);
        }
    }
}
