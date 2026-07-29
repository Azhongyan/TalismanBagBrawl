using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Capability;

namespace TalismanBag.Items.Capability.RuntimeFacts
{
    public static class ItemCapabilityRuntimeFactValidationCodes
    {
        public const string None = "NONE";
        public const string BindingContractMissing = "BINDING_CONTRACT_MISSING";
        public const string BindingContractInvalid = "BINDING_CONTRACT_INVALID";
        public const string UnitContractMissing = "UNIT_CONTRACT_MISSING";
        public const string UnitContractInvalid = "UNIT_CONTRACT_INVALID";
        public const string FactsMissing = "FACTS_MISSING";
        public const string FactRowMissing = "FACT_ROW_MISSING";
        public const string IdentityMissing = "IDENTITY_MISSING";
        public const string IdentityOrphan = "IDENTITY_ORPHAN";
        public const string IdentityMismatch = "IDENTITY_MISMATCH";
        public const string I031Forbidden = "I031_RUNTIME_FACT_FORBIDDEN";
        public const string FactCompletenessMissing = "FACT_COMPLETENESS_MISSING";
        public const string FactIncomplete = "FACT_INCOMPLETE";
        public const string FactFieldMissing = "FACT_FIELD_MISSING";
        public const string EventIdDuplicate = "EVENT_ID_DUPLICATE";
        public const string EventSequenceDuplicate = "EVENT_SEQUENCE_DUPLICATE";
        public const string NegativeValue = "NEGATIVE_VALUE";
        public const string UnitMissing = "UNIT_MISSING";
        public const string UnitConflict = "UNIT_CONFLICT";
        public const string FirstTriggerConflict = "FIRST_TRIGGER_CONFLICT";
        public const string ChainCountConflict = "CHAIN_COUNT3_CONFLICT";
        public const string TriggerOrdinalRegression = "TRIGGER_ORDINAL_REGRESSION";
        public const string TriggerOrdinalGap = "TRIGGER_ORDINAL_GAP";
    }

    public static class ItemCapabilityRuntimeFactValidator
    {
        private const string CleanseCapabilityId = "affix_cleanse_up";
        private const string ChainCapabilityId = "affix_chain_target";
        private const string RefundCapabilityId = "affix_trigger_refund";
        private const string NianEfficiencyCapabilityId = "affix_nian_efficiency";

        public static ItemCapabilityRuntimeFactContractSnapshot Validate(
            ItemInstancePlacementBindingContractSnapshot bindingContract,
            ItemCapabilityUnitContractSnapshot unitContract,
            IItemCapabilityRuntimeFactInputProvider provider)
        {
            return Validate(bindingContract, unitContract,
                provider == null ? null : provider.GetRuntimeFacts());
        }

        public static ItemCapabilityRuntimeFactContractSnapshot Validate(
            ItemInstancePlacementBindingContractSnapshot bindingContract,
            ItemCapabilityUnitContractSnapshot unitContract,
            IReadOnlyList<ItemCapabilityRuntimeFactInput> inputs)
        {
            List<ItemCapabilityRuntimeFactValidationError> errors =
                new List<ItemCapabilityRuntimeFactValidationError>();
            List<ItemCapabilityRuntimeFactSnapshot> facts =
                new List<ItemCapabilityRuntimeFactSnapshot>();

            bool bindingReady = ValidateBindingContract(bindingContract, errors);
            bool unitReady = ValidateUnitContract(unitContract, errors,
                out UnitDomains units);
            ItemCapabilityRuntimeFactInput[] source = (inputs ??
                    Array.Empty<ItemCapabilityRuntimeFactInput>())
                .ToArray();

            if (inputs == null || source.Length == 0)
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.FactsMissing,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, null,
                    "At least one authoritative in-memory runtime fact is required.");
            }

            if (!bindingReady || !unitReady)
            {
                return Snapshot(bindingContract, unitContract, facts, errors);
            }

            HashSet<string> duplicateEventIds = new HashSet<string>(source
                .Where(value => value != null && value.eventId.Length > 0)
                .GroupBy(value => value.eventId, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key), StringComparer.Ordinal);
            HashSet<string> duplicateSequences = new HashSet<string>(source
                .Where(value => value != null
                    && value.battleSessionId.Length > 0
                    && value.eventSequence.HasValue)
                .GroupBy(value => SequenceKey(
                    value.battleSessionId, value.eventSequence.Value),
                    StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key), StringComparer.Ordinal);

            foreach (string eventId in duplicateEventIds.OrderBy(
                value => value, StringComparer.Ordinal))
            {
                ItemCapabilityRuntimeFactInput first = source.First(value =>
                    value != null && string.Equals(
                        value.eventId, eventId, StringComparison.Ordinal));
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.EventIdDuplicate,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, first,
                    "eventId must be unique in the contract snapshot.");
            }

            foreach (string key in duplicateSequences.OrderBy(
                value => value, StringComparer.Ordinal))
            {
                ItemCapabilityRuntimeFactInput first = source.First(value =>
                    value != null && value.eventSequence.HasValue
                    && string.Equals(SequenceKey(value.battleSessionId,
                        value.eventSequence.Value), key, StringComparison.Ordinal));
                Add(errors,
                    ItemCapabilityRuntimeFactValidationCodes.EventSequenceDuplicate,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, first,
                    "(battleSessionId,eventSequence) must be unique.");
            }

            Dictionary<string, long> lastSuccessfulOrdinal =
                new Dictionary<string, long>(StringComparer.Ordinal);
            ItemCapabilityRuntimeFactInput[] ordered = source
                .Where(value => value != null)
                .OrderBy(value => value.battleSessionId, StringComparer.Ordinal)
                .ThenBy(value => value.eventSequence.HasValue ? 0 : 1)
                .ThenBy(value => value.eventSequence.GetValueOrDefault())
                .ThenBy(value => value.eventId, StringComparer.Ordinal)
                .ThenBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .ToArray();

            if (source.Any(value => value == null))
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.FactRowMissing,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, null,
                    "Null runtime fact rows are Unknown and are not materialized.");
            }

            foreach (ItemCapabilityRuntimeFactInput input in ordered)
            {
                if (!TryValidateIdentity(bindingContract, input, errors))
                {
                    continue;
                }

                string sequenceKey = input.eventSequence.HasValue
                    ? SequenceKey(input.battleSessionId,
                        input.eventSequence.Value)
                    : string.Empty;
                if (duplicateEventIds.Contains(input.eventId)
                    || duplicateSequences.Contains(sequenceKey))
                {
                    continue;
                }

                ItemCapabilityRuntimeFactCompleteness completeness =
                    ResolveCompleteness(input, errors);
                bool fieldsComplete = ValidateRequiredFields(input, errors);
                bool canResolve = completeness ==
                        ItemCapabilityRuntimeFactCompleteness.Complete
                    && fieldsComplete;

                ItemCapabilityRuntimeFactTruthStatus trigger = ResolveBoolean(
                    input.triggerSuccess, canResolve);
                ItemCapabilityRuntimeFactTruthStatus first = ResolveBoolean(
                    input.firstTriggerInBattle, canResolve);
                ItemCapabilityRuntimeFactTruthStatus cleanse = ResolveBoolean(
                    input.cleanseSuccess, canResolve);
                ItemCapabilityRuntimeFactTruthStatus chain = ResolveBoolean(
                    input.chainCount3, canResolve);

                bool invalidValue = ValidateNonNegativeValues(input, errors);
                bool invalidUnit = ValidateUnits(input, units, errors);
                if (invalidValue || invalidUnit)
                {
                    trigger = ItemCapabilityRuntimeFactTruthStatus.Invalid;
                    first = ItemCapabilityRuntimeFactTruthStatus.Invalid;
                    cleanse = ItemCapabilityRuntimeFactTruthStatus.Invalid;
                    chain = ItemCapabilityRuntimeFactTruthStatus.Invalid;
                }
                else if (canResolve)
                {
                    ValidateFirstTrigger(input, ref first, errors);
                    ValidateChain(input, ref chain, errors);
                    ValidateOrdinal(input, lastSuccessfulOrdinal,
                        ref trigger, ref first, errors);
                }

                facts.Add(new ItemCapabilityRuntimeFactSnapshot(
                    input.battleSessionId,
                    input.eventId,
                    input.eventSequence,
                    input.itemInstanceId,
                    input.baseItemId,
                    input.placementId,
                    input.triggerOrdinal,
                    trigger,
                    first,
                    cleanse,
                    input.cleanseExtraStackCount,
                    input.cleanseExtraStackUnitKey,
                    input.consecutiveTriggerCount,
                    input.consecutiveTriggerCountUnitKey,
                    chain,
                    input.nianCostBefore,
                    input.nianCostBeforeUnitKey,
                    input.nianCostAfter,
                    input.nianCostAfterUnitKey,
                    input.refundUnits,
                    input.refundUnitKey,
                    completeness));
            }

            return Snapshot(bindingContract, unitContract, facts, errors);
        }

        private static bool ValidateBindingContract(
            ItemInstancePlacementBindingContractSnapshot contract,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            if (contract == null)
            {
                Add(errors,
                    ItemCapabilityRuntimeFactValidationCodes.BindingContractMissing,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, null,
                    "ItemInstancePlacementBindingContractSnapshot.v1 is required.");
                return false;
            }

            bool valid = contract.schemaId ==
                    ItemInstancePlacementBindingContractSnapshot.CurrentSchemaId
                && contract.isValid
                && contract.Bindings.Count > 0;
            valid &= !contract.Bindings.Any(value => value == null
                || string.IsNullOrWhiteSpace(value.itemInstanceId)
                || string.IsNullOrWhiteSpace(value.placementId)
                || string.IsNullOrWhiteSpace(value.baseItemId)
                || string.Equals(value.baseItemId, "I031",
                    StringComparison.Ordinal));
            valid &= !contract.Bindings.GroupBy(value => value.itemInstanceId,
                StringComparer.Ordinal).Any(group => group.Count() > 1);
            valid &= !contract.Bindings.GroupBy(value => value.placementId,
                StringComparer.Ordinal).Any(group => group.Count() > 1);

            if (!valid)
            {
                Add(errors,
                    ItemCapabilityRuntimeFactValidationCodes.BindingContractInvalid,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, null,
                    "Binding contract must be valid, explicit, unique, and free of I031 ordinary instances.");
            }
            return valid;
        }

        private static bool ValidateUnitContract(
            ItemCapabilityUnitContractSnapshot contract,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors,
            out UnitDomains units)
        {
            units = null;
            if (contract == null)
            {
                Add(errors,
                    ItemCapabilityRuntimeFactValidationCodes.UnitContractMissing,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, null,
                    "ItemCapabilityUnitContractSnapshot.v1 is required.");
                return false;
            }

            ItemCapabilityUnitDefinition cleanse = contract.Find(CleanseCapabilityId);
            ItemCapabilityUnitDefinition chain = contract.Find(ChainCapabilityId);
            ItemCapabilityUnitDefinition refund = contract.Find(RefundCapabilityId);
            ItemCapabilityUnitDefinition nianEfficiency =
                contract.Find(NianEfficiencyCapabilityId);
            bool valid = contract.schemaId ==
                    ItemCapabilityUnitContractSnapshot.CurrentSchemaId
                && Matches(cleanse, "cleanse_stack", "stack", "AddFlat")
                && Matches(chain, "target_count", "count", "ExtraTarget")
                && Matches(refund, "nian_point", "point", "Refund")
                && Matches(nianEfficiency, "nian_cost_basis_point",
                    "basisPoint", "ReducePercent")
                && !string.Equals(refund.unitKey, nianEfficiency.unitKey,
                    StringComparison.Ordinal);
            if (!valid)
            {
                Add(errors,
                    ItemCapabilityRuntimeFactValidationCodes.UnitContractInvalid,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, null,
                    "Required stack/count/nian point domains or the distinct nian basis-point parameter contract are invalid.");
                return false;
            }

            units = new UnitDomains(cleanse.unitKey, chain.unitKey, refund.unitKey);
            return true;
        }

        private static bool Matches(
            ItemCapabilityUnitDefinition definition,
            string domain,
            string unit,
            string operation)
        {
            return definition != null
                && string.Equals(definition.valueDomainId, domain,
                    StringComparison.Ordinal)
                && string.Equals(definition.unitKey, unit,
                    StringComparison.Ordinal)
                && string.Equals(definition.operationKey, operation,
                    StringComparison.Ordinal);
        }

        private static bool TryValidateIdentity(
            ItemInstancePlacementBindingContractSnapshot contract,
            ItemCapabilityRuntimeFactInput input,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            if (input.itemInstanceId.Length == 0
                || input.baseItemId.Length == 0
                || input.placementId.Length == 0)
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.IdentityMissing,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, input,
                    "itemInstanceId, baseItemId, and placementId are all required; no identity is inferred.");
                return false;
            }
            if (string.Equals(input.baseItemId, "I031", StringComparison.Ordinal))
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.I031Forbidden,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                    "I031 cannot be forged as an ordinary item runtime fact.");
                return false;
            }

            ItemInstancePlacementBindingSnapshot byInstance =
                contract.FindByItemInstanceId(input.itemInstanceId);
            ItemInstancePlacementBindingSnapshot byPlacement =
                contract.FindByPlacementId(input.placementId);
            if (byInstance == null || byPlacement == null)
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.IdentityOrphan,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                    "The event identity is an orphan outside the validated Binding contract.");
                return false;
            }
            if (!ReferenceEquals(byInstance, byPlacement)
                && (!string.Equals(byInstance.itemInstanceId,
                        byPlacement.itemInstanceId, StringComparison.Ordinal)
                    || !string.Equals(byInstance.placementId,
                        byPlacement.placementId, StringComparison.Ordinal)))
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.IdentityMismatch,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                    "itemInstanceId and placementId resolve to different Binding rows.");
                return false;
            }
            if (!string.Equals(byInstance.baseItemId, input.baseItemId,
                    StringComparison.Ordinal)
                || !string.Equals(byPlacement.baseItemId, input.baseItemId,
                    StringComparison.Ordinal))
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.IdentityMismatch,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                    "baseItemId must exactly match the validated three-way Binding.");
                return false;
            }
            return true;
        }

        private static ItemCapabilityRuntimeFactCompleteness ResolveCompleteness(
            ItemCapabilityRuntimeFactInput input,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            if (!input.factCompleteness.HasValue)
            {
                Add(errors,
                    ItemCapabilityRuntimeFactValidationCodes.FactCompletenessMissing,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, input,
                    "factCompleteness is missing.");
                return ItemCapabilityRuntimeFactCompleteness.Incomplete;
            }
            if (!Enum.IsDefined(typeof(ItemCapabilityRuntimeFactCompleteness),
                input.factCompleteness.Value))
            {
                Add(errors,
                    ItemCapabilityRuntimeFactValidationCodes.FactIncomplete,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                    "factCompleteness contains an undefined value.");
                return ItemCapabilityRuntimeFactCompleteness.Incomplete;
            }
            if (input.factCompleteness.Value ==
                ItemCapabilityRuntimeFactCompleteness.Incomplete)
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.FactIncomplete,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, input,
                    "Incomplete facts resolve to Unknown even when a raw boolean is present.");
            }
            return input.factCompleteness.Value;
        }

        private static bool ValidateRequiredFields(
            ItemCapabilityRuntimeFactInput input,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            List<string> missing = new List<string>();
            if (input.battleSessionId.Length == 0) missing.Add("battleSessionId");
            if (input.eventId.Length == 0) missing.Add("eventId");
            if (!input.eventSequence.HasValue) missing.Add("eventSequence");
            if (!input.triggerOrdinal.HasValue) missing.Add("triggerOrdinal");
            if (!input.triggerSuccess.HasValue) missing.Add("triggerSuccess");
            if (!input.firstTriggerInBattle.HasValue) missing.Add("firstTriggerInBattle");
            if (!input.cleanseSuccess.HasValue) missing.Add("cleanseSuccess");
            if (!input.cleanseExtraStackCount.HasValue)
                missing.Add("cleanseExtraStackCount");
            if (input.cleanseExtraStackUnitKey.Length == 0)
                missing.Add("cleanseExtraStackUnitKey");
            if (!input.consecutiveTriggerCount.HasValue)
                missing.Add("consecutiveTriggerCount");
            if (input.consecutiveTriggerCountUnitKey.Length == 0)
                missing.Add("consecutiveTriggerCountUnitKey");
            if (!input.chainCount3.HasValue) missing.Add("chainCount3");
            if (!input.nianCostBefore.HasValue) missing.Add("nianCostBefore");
            if (input.nianCostBeforeUnitKey.Length == 0)
                missing.Add("nianCostBeforeUnitKey");
            if (!input.nianCostAfter.HasValue) missing.Add("nianCostAfter");
            if (input.nianCostAfterUnitKey.Length == 0)
                missing.Add("nianCostAfterUnitKey");
            if (!input.refundUnits.HasValue) missing.Add("refundUnits");
            if (input.refundUnitKey.Length == 0) missing.Add("refundUnitKey");
            if (missing.Count == 0) return true;
            Add(errors, ItemCapabilityRuntimeFactValidationCodes.FactFieldMissing,
                ItemCapabilityRuntimeFactTruthStatus.Unknown, input,
                "Missing fields: " + string.Join(",", missing) + ".");
            return false;
        }

        private static bool ValidateNonNegativeValues(
            ItemCapabilityRuntimeFactInput input,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            bool invalid = input.eventSequence.GetValueOrDefault() < 0
                || input.triggerOrdinal.GetValueOrDefault() < 0
                || input.cleanseExtraStackCount.GetValueOrDefault() < 0
                || input.consecutiveTriggerCount.GetValueOrDefault() < 0
                || input.nianCostBefore.GetValueOrDefault() < 0
                || input.nianCostAfter.GetValueOrDefault() < 0
                || input.refundUnits.GetValueOrDefault() < 0;
            if (invalid)
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.NegativeValue,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                    "Event sequence, ordinals, counts, nian points, and refund units cannot be negative.");
            }
            return invalid;
        }

        private static bool ValidateUnits(
            ItemCapabilityRuntimeFactInput input,
            UnitDomains units,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            string[] actual =
            {
                input.cleanseExtraStackUnitKey,
                input.consecutiveTriggerCountUnitKey,
                input.nianCostBeforeUnitKey,
                input.nianCostAfterUnitKey,
                input.refundUnitKey
            };
            if (actual.Any(string.IsNullOrWhiteSpace))
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.UnitMissing,
                    ItemCapabilityRuntimeFactTruthStatus.Unknown, input,
                    "Every reported count or nian value requires its native unit key.");
                return false;
            }
            bool conflict = !string.Equals(actual[0], units.Cleanse,
                    StringComparison.Ordinal)
                || !string.Equals(actual[1], units.Chain, StringComparison.Ordinal)
                || !string.Equals(actual[2], units.Nian, StringComparison.Ordinal)
                || !string.Equals(actual[3], units.Nian, StringComparison.Ordinal)
                || !string.Equals(actual[4], units.Nian, StringComparison.Ordinal);
            if (conflict)
            {
                Add(errors, ItemCapabilityRuntimeFactValidationCodes.UnitConflict,
                    ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                    "Runtime facts require stack/count/point; nian basisPoint parameters are not point facts.");
            }
            return conflict;
        }

        private static void ValidateFirstTrigger(
            ItemCapabilityRuntimeFactInput input,
            ref ItemCapabilityRuntimeFactTruthStatus status,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            bool conflict = input.firstTriggerInBattle.Value
                ? !input.triggerSuccess.Value || input.triggerOrdinal.Value != 1
                : input.triggerSuccess.Value && input.triggerOrdinal.Value == 1;
            if (!conflict) return;
            status = ItemCapabilityRuntimeFactTruthStatus.Invalid;
            Add(errors,
                ItemCapabilityRuntimeFactValidationCodes.FirstTriggerConflict,
                ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                "KnownTrue requires a successful ordinal 1; successful ordinal 1 cannot be KnownFalse.");
        }

        private static void ValidateChain(
            ItemCapabilityRuntimeFactInput input,
            ref ItemCapabilityRuntimeFactTruthStatus status,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            bool expected = input.consecutiveTriggerCount.Value >= 3;
            if (input.chainCount3.Value == expected) return;
            status = ItemCapabilityRuntimeFactTruthStatus.Invalid;
            Add(errors, ItemCapabilityRuntimeFactValidationCodes.ChainCountConflict,
                ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                "chainCount3 is true for count >= 3 and false for count < 3.");
        }

        private static void ValidateOrdinal(
            ItemCapabilityRuntimeFactInput input,
            IDictionary<string, long> lastSuccessfulOrdinal,
            ref ItemCapabilityRuntimeFactTruthStatus trigger,
            ref ItemCapabilityRuntimeFactTruthStatus first,
            ICollection<ItemCapabilityRuntimeFactValidationError> errors)
        {
            if (!input.triggerSuccess.Value) return;
            string key = input.battleSessionId + "\u001f" + input.itemInstanceId;
            long previous;
            long expected = lastSuccessfulOrdinal.TryGetValue(key, out previous)
                ? previous + 1
                : 1;
            long actual = input.triggerOrdinal.Value;
            if (actual == expected)
            {
                lastSuccessfulOrdinal[key] = actual;
                return;
            }

            trigger = ItemCapabilityRuntimeFactTruthStatus.Invalid;
            first = ItemCapabilityRuntimeFactTruthStatus.Invalid;
            string code = actual < expected
                ? ItemCapabilityRuntimeFactValidationCodes.TriggerOrdinalRegression
                : ItemCapabilityRuntimeFactValidationCodes.TriggerOrdinalGap;
            Add(errors, code, ItemCapabilityRuntimeFactTruthStatus.Invalid, input,
                "Successful triggerOrdinal expected "
                + expected.ToString(CultureInfo.InvariantCulture) + " but was "
                + actual.ToString(CultureInfo.InvariantCulture) + ".");
        }

        private static ItemCapabilityRuntimeFactTruthStatus ResolveBoolean(
            bool? value,
            bool canResolve)
        {
            if (!canResolve || !value.HasValue)
                return ItemCapabilityRuntimeFactTruthStatus.Unknown;
            return value.Value
                ? ItemCapabilityRuntimeFactTruthStatus.KnownTrue
                : ItemCapabilityRuntimeFactTruthStatus.KnownFalse;
        }

        private static ItemCapabilityRuntimeFactContractSnapshot Snapshot(
            ItemInstancePlacementBindingContractSnapshot bindingContract,
            ItemCapabilityUnitContractSnapshot unitContract,
            IEnumerable<ItemCapabilityRuntimeFactSnapshot> facts,
            IEnumerable<ItemCapabilityRuntimeFactValidationError> errors)
        {
            ItemCapabilityRuntimeFactSnapshot[] factArray = (facts ??
                    Array.Empty<ItemCapabilityRuntimeFactSnapshot>()).ToArray();
            ItemCapabilityRuntimeFactValidationError[] errorArray = (errors ??
                    Array.Empty<ItemCapabilityRuntimeFactValidationError>())
                .GroupBy(value => value.status + "|" + value.code + "|"
                    + value.battleSessionId + "|" + value.eventId + "|"
                    + value.itemInstanceId + "|" + value.placementId + "|"
                    + value.message, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
            ItemCapabilityRuntimeFactCompleteness completeness =
                factArray.Length > 0
                && factArray.All(value => value.factCompleteness ==
                    ItemCapabilityRuntimeFactCompleteness.Complete)
                && !errorArray.Any(value => value.status ==
                    ItemCapabilityRuntimeFactTruthStatus.Unknown)
                    ? ItemCapabilityRuntimeFactCompleteness.Complete
                    : ItemCapabilityRuntimeFactCompleteness.Incomplete;
            return new ItemCapabilityRuntimeFactContractSnapshot(
                bindingContract?.canonicalSignature ?? string.Empty,
                unitContract?.canonicalSignature ?? string.Empty,
                completeness,
                factArray,
                errorArray);
        }

        private static string SequenceKey(string sessionId, long sequence)
        {
            return (sessionId ?? string.Empty) + "\u001f"
                + sequence.ToString(CultureInfo.InvariantCulture);
        }

        private static void Add(
            ICollection<ItemCapabilityRuntimeFactValidationError> errors,
            string code,
            ItemCapabilityRuntimeFactTruthStatus status,
            ItemCapabilityRuntimeFactInput input,
            string message)
        {
            errors.Add(new ItemCapabilityRuntimeFactValidationError(
                code,
                status,
                input?.battleSessionId,
                input?.eventId,
                input?.itemInstanceId,
                input?.placementId,
                message));
        }

        private sealed class UnitDomains
        {
            public UnitDomains(string cleanse, string chain, string nian)
            {
                Cleanse = cleanse;
                Chain = chain;
                Nian = nian;
            }

            public string Cleanse { get; }
            public string Chain { get; }
            public string Nian { get; }
        }
    }
}
