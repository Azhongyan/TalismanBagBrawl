using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.Items.Awakening.RuntimeState
{
    public static class ItemCoreEffectRuntimeStateValidationCodes
    {
        public const string None = "NONE";
        public const string InputMissing = "INPUT_MISSING";
        public const string IdentityCatalogMissing = "IDENTITY_CATALOG_MISSING";
        public const string IdentityCatalogInvalid = "IDENTITY_CATALOG_INVALID";
        public const string IdentityCountInvalid = "IDENTITY_COUNT_INVALID";
        public const string IdentityDuplicate = "IDENTITY_DUPLICATE";
        public const string IdentityItemInvalid = "IDENTITY_ITEM_INVALID";
        public const string IdentityMappingInvalid = "IDENTITY_MAPPING_INVALID";
        public const string IdentityDefinitionMismatch =
            "IDENTITY_DEFINITION_MISMATCH";
        public const string CultivationRosterMissing =
            "CULTIVATION_ROSTER_MISSING";
        public const string CultivationRosterUnknown =
            "CULTIVATION_ROSTER_UNKNOWN";
        public const string CultivationDuplicate = "CULTIVATION_DUPLICATE";
        public const string CultivationIdentityMissing =
            "CULTIVATION_IDENTITY_MISSING";
        public const string CultivationItemMismatch =
            "CULTIVATION_ITEM_MISMATCH";
        public const string CultivationLevelInvalid =
            "CULTIVATION_LEVEL_INVALID";
        public const string CultivationSourceMissing =
            "CULTIVATION_SOURCE_MISSING";
        public const string ProjectionMissing = "PROJECTION_MISSING";
        public const string ProjectionInvalid = "PROJECTION_INVALID";
        public const string ProjectionIdentityDuplicate =
            "PROJECTION_IDENTITY_DUPLICATE";
        public const string ProjectionI031Forbidden =
            "PROJECTION_I031_FORBIDDEN";
        public const string BindingMissing = "BINDING_MISSING";
        public const string BindingInvalid = "BINDING_INVALID";
        public const string BindingOrphan = "BINDING_ORPHAN";
        public const string BindingIdentityMismatch =
            "BINDING_IDENTITY_MISMATCH";
        public const string ItemSystemMissing = "ITEM_SYSTEM_MISSING";
        public const string ItemSystemInvalid = "ITEM_SYSTEM_INVALID";
        public const string PlacementMissing = "PLACEMENT_MISSING";
        public const string PlacementOrphan = "PLACEMENT_ORPHAN";
        public const string PlacementIdentityMismatch =
            "PLACEMENT_IDENTITY_MISMATCH";
        public const string AwakeningResultMissing =
            "AWAKENING_RESULT_MISSING";
        public const string AwakeningResultMismatch =
            "AWAKENING_RESULT_MISMATCH";
        public const string AwakeningNodeMismatch =
            "AWAKENING_NODE_MISMATCH";
        public const string ActiveStateContradiction =
            "ACTIVE_STATE_CONTRADICTION";
        public const string SupersededExpansionDrift =
            "SUPERSEDED_EXPANSION_DRIFT";
        public const string I031OrdinaryForbidden = "I031_ORDINARY_FORBIDDEN";
        public const string AssemblyException = "ASSEMBLY_EXCEPTION";
    }

    public static class ItemCoreEffectIdentityCatalogBuilder
    {
        private static readonly ItemCoreAwakeningNodeKind[] ExplicitKinds =
        {
            ItemCoreAwakeningNodeKind.Core1,
            ItemCoreAwakeningNodeKind.Core2,
            ItemCoreAwakeningNodeKind.Core3,
            ItemCoreAwakeningNodeKind.Ultimate
        };

        public static ItemCoreEffectIdentityCatalogSnapshot Build(
            ItemBalanceWorkbenchCatalog workbenchCatalog,
            IItemCoreEffectDefinitionProvider awakeningDefinitionProvider)
        {
            List<ItemCoreEffectRuntimeStateValidationError> errors = new();
            List<ItemCoreEffectIdentityRow> rows = new();
            if (workbenchCatalog == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityCatalogMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, "Candidate workbench catalog is missing."));
                return Snapshot(rows, errors);
            }
            if (awakeningDefinitionProvider == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityCatalogMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, "Awakening definition provider is missing."));
                return Snapshot(rows, errors);
            }

            string[] expectedItems = Enumerable.Range(1, 30)
                .Select(index => "I" + index.ToString(
                    "000", CultureInfo.InvariantCulture))
                .ToArray();
            ItemBalanceProfile[] profiles = (workbenchCatalog.profiles ??
                    new List<ItemBalanceProfile>())
                .Where(value => value != null)
                .OrderBy(value => value.baseItemId, StringComparer.Ordinal)
                .ToArray();
            foreach (string duplicate in profiles
                         .GroupBy(value => value.baseItemId, StringComparer.Ordinal)
                         .Where(group => group.Count() != 1)
                         .Select(group => group.Key))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.IdentityDuplicate,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    duplicate, null, "Candidate profile identity is duplicated."));
            }
            if (!profiles.Select(value => value.baseItemId)
                    .SequenceEqual(expectedItems, StringComparer.Ordinal))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.IdentityCountInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null,
                    "Candidate profiles must be exactly I001-I030; I031 is excluded."));
            }

            foreach (string baseItemId in expectedItems)
            {
                ItemBalanceProfile profile = profiles.FirstOrDefault(value =>
                    string.Equals(value.baseItemId, baseItemId,
                        StringComparison.Ordinal));
                if (profile == null)
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityItemInvalid,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        baseItemId, null, "Candidate profile is missing."));
                    continue;
                }

                Dictionary<ItemCoreAwakeningNodeKind, ItemBalanceCoreCandidate>
                    candidates = BuildCandidateMap(profile, errors);
                Dictionary<ItemCoreAwakeningNodeKind, ItemCoreEffectDefinition>
                    awakenings = BuildAwakeningMap(
                        baseItemId, awakeningDefinitionProvider, errors);
                foreach (ItemCoreAwakeningNodeKind nodeKind in ExplicitKinds)
                {
                    candidates.TryGetValue(nodeKind, out ItemBalanceCoreCandidate
                        candidate);
                    awakenings.TryGetValue(nodeKind, out ItemCoreEffectDefinition
                        awakening);
                    if (candidate == null || awakening == null)
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .IdentityMappingInvalid,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            baseItemId,
                            candidate?.coreEffectId,
                            "Explicit baseItemId + nodeKind mapping is incomplete.",
                            awakening?.coreEffectId));
                        continue;
                    }

                    string expectedCandidateId =
                        ExplicitCandidateDefinitionId(baseItemId, nodeKind);
                    string expectedAwakeningId =
                        ExplicitAwakeningNodeId(baseItemId, nodeKind);
                    string candidateRarityKey =
                        candidate.requiredRarity.ToStableKey();
                    bool semanticValid = SemanticIsValid(candidate, nodeKind);
                    bool matches =
                        string.Equals(candidate.coreEffectId,
                            expectedCandidateId, StringComparison.Ordinal)
                        && string.Equals(awakening.coreEffectId,
                            expectedAwakeningId, StringComparison.Ordinal)
                        && string.Equals(awakening.itemId, baseItemId,
                            StringComparison.Ordinal)
                        && candidate.unlockLevel == awakening.unlockLevel
                        && candidate.unlockLevel ==
                            ItemCoreAwakeningResolver.ExpectedUnlockLevel(nodeKind)
                        && string.Equals(candidateRarityKey,
                            awakening.requiredRarityKey,
                            StringComparison.Ordinal)
                        && candidate.isUltimate ==
                            (nodeKind == ItemCoreAwakeningNodeKind.Ultimate)
                        && semanticValid;
                    if (!matches)
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .IdentityDefinitionMismatch,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            baseItemId,
                            candidate.coreEffectId,
                            "Candidate and Awakening authorities disagree for the explicit nodeKind.",
                            awakening.coreEffectId));
                        continue;
                    }

                    rows.Add(new ItemCoreEffectIdentityRow(
                        baseItemId,
                        nodeKind,
                        candidate.coreEffectId,
                        awakening.coreEffectId,
                        candidate.displayName,
                        candidate.description,
                        candidate.unlockLevel,
                        awakening.unlockLevel,
                        candidate.requiredRarity,
                        awakening.requiredRarityKey,
                        "ItemBalanceWorkbenchCatalog/"
                            + workbenchCatalog.balanceDataRevision + "/"
                            + baseItemId + "/" + candidate.coreEffectId,
                        awakeningDefinitionProvider.GetType().FullName + "/"
                            + baseItemId + "/" + awakening.coreEffectId,
                        BuildPayloadIdentity(candidate.effectPayload),
                        candidate.effectPayload?.effectCategory.ToString()
                            ?? string.Empty,
                        candidate.effectPayload?.operation.ToString()
                            ?? string.Empty));
                }
            }

            ValidateAcceptedRows(rows, errors);
            return Snapshot(rows, errors);
        }

        public static string ExplicitCandidateDefinitionId(
            string baseItemId,
            ItemCoreAwakeningNodeKind nodeKind)
        {
            string stem = "candidate_core_"
                + (baseItemId ?? string.Empty).ToLowerInvariant();
            return nodeKind switch
            {
                ItemCoreAwakeningNodeKind.Core1 => stem + "_01",
                ItemCoreAwakeningNodeKind.Core2 => stem + "_02",
                ItemCoreAwakeningNodeKind.Core3 => stem + "_03",
                ItemCoreAwakeningNodeKind.Core4 => stem + "_04",
                ItemCoreAwakeningNodeKind.Ultimate => stem + "_ultimate",
                _ => string.Empty
            };
        }

        public static string ExplicitAwakeningNodeId(
            string baseItemId,
            ItemCoreAwakeningNodeKind nodeKind)
        {
            return (baseItemId ?? string.Empty) + (nodeKind switch
            {
                ItemCoreAwakeningNodeKind.Core1 => "_CORE_01",
                ItemCoreAwakeningNodeKind.Core2 => "_CORE_02",
                ItemCoreAwakeningNodeKind.Core3 => "_CORE_03",
                ItemCoreAwakeningNodeKind.Core4 => "_CORE_04",
                ItemCoreAwakeningNodeKind.Ultimate => "_CORE_ULT",
                _ => string.Empty
            });
        }

        private static Dictionary<ItemCoreAwakeningNodeKind,
            ItemBalanceCoreCandidate> BuildCandidateMap(
                ItemBalanceProfile profile,
                ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            var map = new Dictionary<ItemCoreAwakeningNodeKind,
                ItemBalanceCoreCandidate>();
            foreach (ItemBalanceCoreCandidate candidate in
                     profile.coreCandidates ?? new List<ItemBalanceCoreCandidate>())
            {
                if (candidate == null
                    || !TryNodeKind(candidate.nodeKind, out
                        ItemCoreAwakeningNodeKind nodeKind))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityMappingInvalid,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        profile.baseItemId,
                        candidate?.coreEffectId,
                        "Candidate nodeKind is missing or unsupported."));
                    continue;
                }
                if (nodeKind == ItemCoreAwakeningNodeKind.Core4)
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .SupersededExpansionDrift,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        profile.baseItemId,
                        candidate.coreEffectId,
                        "Candidate Core4 is superseded expansion drift and cannot enter the formal four-core identity catalog."));
                    continue;
                }
                if (!map.TryAdd(nodeKind, candidate))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityMappingInvalid,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        profile.baseItemId,
                        candidate.coreEffectId,
                        "Candidate nodeKind is duplicated."));
                }
            }
            return map;
        }

        private static Dictionary<ItemCoreAwakeningNodeKind,
            ItemCoreEffectDefinition> BuildAwakeningMap(
                string baseItemId,
                IItemCoreEffectDefinitionProvider provider,
                ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            var map = new Dictionary<ItemCoreAwakeningNodeKind,
                ItemCoreEffectDefinition>();
            IReadOnlyList<ItemCoreEffectDefinition> definitions;
            try
            {
                definitions = provider.GetDefinitions(baseItemId);
            }
            catch (Exception exception)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityCatalogInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    baseItemId, null,
                    "Awakening provider threw " + exception.GetType().Name + "."));
                return map;
            }
            foreach (ItemCoreEffectDefinition definition in definitions ??
                     Array.Empty<ItemCoreEffectDefinition>())
            {
                if (definition?.nodeKind == ItemCoreAwakeningNodeKind.Core4)
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .SupersededExpansionDrift,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        baseItemId, null,
                        "Awakening Core4 is deprecated/reserved compatibility evidence and cannot enter the formal four-core identity catalog.",
                        definition.coreEffectId));
                    continue;
                }
                if (definition == null
                    || !map.TryAdd(definition.nodeKind, definition))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityMappingInvalid,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        baseItemId, null,
                        "Awakening nodeKind is null or duplicated.",
                        definition?.coreEffectId));
                }
            }
            return map;
        }

        private static bool TryNodeKind(
            string value,
            out ItemCoreAwakeningNodeKind nodeKind)
        {
            switch (value)
            {
                case "Core1":
                    nodeKind = ItemCoreAwakeningNodeKind.Core1;
                    return true;
                case "Core2":
                    nodeKind = ItemCoreAwakeningNodeKind.Core2;
                    return true;
                case "Core3":
                    nodeKind = ItemCoreAwakeningNodeKind.Core3;
                    return true;
                case "Core4":
                    nodeKind = ItemCoreAwakeningNodeKind.Core4;
                    return true;
                case "Ultimate":
                    nodeKind = ItemCoreAwakeningNodeKind.Ultimate;
                    return true;
                default:
                    nodeKind = ItemCoreAwakeningNodeKind.Core1;
                    return false;
            }
        }

        private static bool SemanticIsValid(
            ItemBalanceCoreCandidate candidate,
            ItemCoreAwakeningNodeKind nodeKind)
        {
            if (candidate?.effectPayload == null)
            {
                return false;
            }
            if (nodeKind == ItemCoreAwakeningNodeKind.Core4)
            {
                return candidate.effectPayload.operation ==
                    ItemCandidateEffectOperation.ExtraTrigger
                    && candidate.effectPayload.internalCooldownUnits > 0;
            }
            if (nodeKind == ItemCoreAwakeningNodeKind.Ultimate)
            {
                return candidate.effectPayload.effectCategory ==
                    ItemCandidateEffectCategory.MechanicConversion
                    && candidate.effectPayload.operation ==
                    ItemCandidateEffectOperation.Convert;
            }
            return true;
        }

        private static void ValidateAcceptedRows(
            IReadOnlyCollection<ItemCoreEffectIdentityRow> rows,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            if (rows.Count != 120)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.IdentityCountInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null,
                    "Accepted identity catalog must contain exactly 120 rows."));
            }
            if (rows.Any(value => string.Equals(
                    value.baseItemId, "I031", StringComparison.Ordinal)))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .I031OrdinaryForbidden,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    "I031", null,
                    "I031 cannot have ordinary core-effect identities."));
            }
            foreach (IGrouping<string, ItemCoreEffectIdentityRow> group in rows
                         .GroupBy(value => value.baseItemId,
                             StringComparer.Ordinal))
            {
                if (group.Count() != 4
                    || group.Select(value => value.nodeKind).Distinct().Count() != 4
                    || group.Any(value => value.nodeKind ==
                        ItemCoreAwakeningNodeKind.Core4))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityMappingInvalid,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        group.Key, null,
                        "Each ordinary base item requires Core1/Core2/Core3/Ultimate only."));
                }
            }
            if (rows.GroupBy(value => value.candidateDefinitionId,
                    StringComparer.Ordinal).Any(group => group.Count() != 1)
                || rows.GroupBy(value => value.awakeningNodeId,
                    StringComparer.Ordinal).Any(group => group.Count() != 1))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.IdentityDuplicate,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null,
                    "Candidate and Awakening identities must both be globally unique."));
            }
        }

        private static ItemCoreEffectIdentityCatalogSnapshot Snapshot(
            IEnumerable<ItemCoreEffectIdentityRow> rows,
            IEnumerable<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            ItemCoreEffectRuntimeStateValidationError[] frozen =
                errors.ToArray();
            return new ItemCoreEffectIdentityCatalogSnapshot(
                ItemInstanceCoreEffectRuntimeStateValidation.ResolveStatus(
                    frozen),
                rows,
                frozen);
        }

        private static string BuildPayloadIdentity(
            ItemCandidateEffectPayload payload)
        {
            if (payload == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new();
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "effectId", payload.effectId);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "effectCategory", payload.effectCategory.ToString());
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "operation", payload.operation.ToString());
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "triggerEventId", payload.triggerEventId);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "conditionId", payload.conditionId);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "targetSelector", payload.targetSelector);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "targetStatId", payload.targetStatId);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "valueUnitKey", payload.valueUnitKey);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "valueUnits",
                payload.valueUnits.ToString(CultureInfo.InvariantCulture));
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "secondaryValueUnits",
                payload.secondaryValueUnits.ToString(CultureInfo.InvariantCulture));
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "durationUnits",
                payload.durationUnits.ToString(CultureInfo.InvariantCulture));
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "stackLimit",
                payload.stackLimit.ToString(CultureInfo.InvariantCulture));
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "internalCooldownUnits",
                payload.internalCooldownUnits.ToString(
                    CultureInfo.InvariantCulture));
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "parameterProfileId", payload.parameterProfileId);
            ItemCoreEffectRuntimeStateCanonical.Field(
                builder, "mutexGroupId", payload.mutexGroupId);
            foreach (string tag in (payload.effectTags ?? new List<string>())
                         .OrderBy(value => value, StringComparer.Ordinal))
            {
                ItemCoreEffectRuntimeStateCanonical.Field(
                    builder, "effectTag", tag);
            }
            return payload.effectId + "#"
                + ItemCoreEffectRuntimeStateCanonical.Sha256(builder.ToString());
        }

        private static ItemCoreEffectRuntimeStateValidationError Error(
            string code,
            ItemCoreEffectRuntimeStateStatus status,
            string baseItemId,
            string candidateDefinitionId,
            string message,
            string awakeningNodeId = null)
        {
            return new ItemCoreEffectRuntimeStateValidationError(
                code, status, null, null, baseItemId, candidateDefinitionId,
                awakeningNodeId, message);
        }
    }

    public static class ItemInstanceCoreEffectRuntimeStateValidation
    {
        public static IReadOnlyList<ItemCoreEffectRuntimeStateValidationError>
            ValidateIdentityRows(
                IReadOnlyList<ItemCoreEffectIdentityRow> rows)
        {
            List<ItemCoreEffectRuntimeStateValidationError> errors = new();
            ItemCoreEffectIdentityRow[] safe = (rows ??
                    Array.Empty<ItemCoreEffectIdentityRow>())
                .Where(value => value != null).ToArray();
            if (safe.Length != 120)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityCountInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null,
                    "Identity catalog must contain exactly 120 rows."));
            }
            string[] expectedItems = Enumerable.Range(1, 30)
                .Select(index => "I" + index.ToString(
                    "000", CultureInfo.InvariantCulture)).ToArray();
            if (!safe.Select(value => value.baseItemId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(expectedItems, StringComparer.Ordinal))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityItemInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null,
                    "Identity catalog roster must be exactly I001-I030."));
            }
            if (safe.Any(value => string.Equals(
                    value.baseItemId, "I031", StringComparison.Ordinal)))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .I031OrdinaryForbidden,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, "I031",
                    "I031 cannot have an ordinary core-effect identity."));
            }
            if (safe.GroupBy(value => value.candidateDefinitionId,
                    StringComparer.Ordinal).Any(group => group.Count() != 1)
                || safe.GroupBy(value => value.awakeningNodeId,
                    StringComparer.Ordinal).Any(group => group.Count() != 1))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityDuplicate,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null,
                    "Candidate and Awakening identities must be globally unique."));
            }
            foreach (IGrouping<string, ItemCoreEffectIdentityRow> group in safe
                         .GroupBy(value => value.baseItemId,
                             StringComparer.Ordinal))
            {
                if (group.Count() != 4
                    || group.Select(value => value.nodeKind)
                        .Distinct().Count() != 4
                    || group.Any(value => value.nodeKind ==
                        ItemCoreAwakeningNodeKind.Core4))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityMappingInvalid,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        null, null, group.Key,
                        "Each ordinary item requires Core1/Core2/Core3/Ultimate only."));
                }
            }
            foreach (ItemCoreEffectIdentityRow row in safe)
            {
                if (row.nodeKind == ItemCoreAwakeningNodeKind.Core4)
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .SupersededExpansionDrift,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        null, null, row.baseItemId,
                        "Core4 is reserved compatibility evidence and cannot be a formal identity row.",
                        row.candidateDefinitionId,
                        row.awakeningNodeId));
                    continue;
                }
                int expectedLevel =
                    ItemCoreAwakeningResolver.ExpectedUnlockLevel(
                        row.nodeKind);
                ItemInstanceRarity expectedRarity = row.nodeKind switch
                {
                    ItemCoreAwakeningNodeKind.Core1 =>
                        ItemInstanceRarity.White,
                    ItemCoreAwakeningNodeKind.Core2 =>
                        ItemInstanceRarity.Green,
                    ItemCoreAwakeningNodeKind.Core3 =>
                        ItemInstanceRarity.Blue,
                    ItemCoreAwakeningNodeKind.Ultimate =>
                        ItemInstanceRarity.Orange,
                    _ => (ItemInstanceRarity)(-1)
                };
                string expectedRarityKey = row.nodeKind switch
                {
                    ItemCoreAwakeningNodeKind.Core1 => "white",
                    ItemCoreAwakeningNodeKind.Core2 => "green",
                    ItemCoreAwakeningNodeKind.Core3 => "blue",
                    ItemCoreAwakeningNodeKind.Ultimate => "orange",
                    _ => string.Empty
                };
                bool semanticValid = row.nodeKind !=
                    ItemCoreAwakeningNodeKind.Ultimate
                    || string.Equals(row.candidateEffectCategory,
                        "MechanicConversion", StringComparison.Ordinal)
                    && string.Equals(row.candidateEffectOperation,
                        "Convert", StringComparison.Ordinal);
                if (!string.Equals(
                        row.candidateDefinitionId,
                        ItemCoreEffectIdentityCatalogBuilder
                            .ExplicitCandidateDefinitionId(
                                row.baseItemId, row.nodeKind),
                        StringComparison.Ordinal)
                    || !string.Equals(
                        row.awakeningNodeId,
                        ItemCoreEffectIdentityCatalogBuilder
                            .ExplicitAwakeningNodeId(
                                row.baseItemId, row.nodeKind),
                        StringComparison.Ordinal)
                    || row.candidateUnlockLevel != expectedLevel
                    || row.awakeningUnlockLevel != expectedLevel
                    || row.requiredRarity != expectedRarity
                    || !string.Equals(row.requiredRarityKey,
                        expectedRarityKey,
                        StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(row.displayName)
                    || string.IsNullOrWhiteSpace(row.description)
                    || string.IsNullOrWhiteSpace(
                        row.candidatePayloadIdentity)
                    || string.IsNullOrWhiteSpace(
                        row.candidateSourceIdentity)
                    || string.IsNullOrWhiteSpace(
                        row.awakeningSourceIdentity)
                    || !semanticValid)
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityDefinitionMismatch,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        null, null, row.baseItemId,
                        "Identity row conflicts with explicit Candidate/Awakening authority.",
                        row.candidateDefinitionId,
                        row.awakeningNodeId));
                }
            }
            return errors;
        }

        public static IReadOnlyList<ItemCoreEffectRuntimeStateValidationError>
            ValidateRuntimeItems(
                IReadOnlyList<ItemInstanceCoreEffectRuntimeItemSnapshot> items)
        {
            List<ItemCoreEffectRuntimeStateValidationError> errors = new();
            ItemInstanceCoreEffectRuntimeItemSnapshot[] safe = (items ??
                    Array.Empty<ItemInstanceCoreEffectRuntimeItemSnapshot>())
                .Where(value => value != null).ToArray();
            foreach (IGrouping<string,
                         ItemInstanceCoreEffectRuntimeItemSnapshot> duplicate in
                     safe.GroupBy(value => value.itemInstanceId,
                             StringComparer.Ordinal)
                         .Where(group => group.Count() != 1))
            {
                ItemInstanceCoreEffectRuntimeItemSnapshot item =
                    duplicate.First();
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .ProjectionIdentityDuplicate,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    item.itemInstanceId, item.placementId, item.baseItemId,
                    "Runtime itemInstanceId is duplicated."));
            }
            foreach (ItemInstanceCoreEffectRuntimeItemSnapshot item in safe)
            {
                if (string.Equals(item.baseItemId, "I031",
                        StringComparison.Ordinal))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .I031OrdinaryForbidden,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "I031 cannot have ordinary runtime core-effect rows."));
                    continue;
                }

                ItemInstanceCoreEffectRuntimeRow[] rows =
                    item.CoreEffectRows.Where(value => value != null).ToArray();
                if (rows.Length != 4
                    || rows.Select(value => value.nodeKind).Distinct().Count() != 4)
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .AwakeningNodeMismatch,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "Runtime item must contain Core1/Core2/Core3/Ultimate exactly once."));
                }
                if (rows.Any(value => value.nodeKind ==
                        ItemCoreAwakeningNodeKind.Core4))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .SupersededExpansionDrift,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "Core4 cannot appear in formal runtime rows."));
                }
                if (rows.GroupBy(value => value.candidateDefinitionId,
                        StringComparer.Ordinal).Any(group => group.Count() != 1)
                    || rows.GroupBy(value => value.awakeningNodeId,
                        StringComparer.Ordinal).Any(group => group.Count() != 1))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .IdentityDuplicate,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "Runtime Candidate and Awakening identities must be unique."));
                }

                foreach (ItemInstanceCoreEffectRuntimeRow row in rows)
                {
                    if (!string.Equals(row.itemInstanceId,
                            item.itemInstanceId, StringComparison.Ordinal)
                        || !string.Equals(row.baseItemId,
                            item.baseItemId, StringComparison.Ordinal))
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .IdentityDefinitionMismatch,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            item.itemInstanceId, item.placementId,
                            item.baseItemId,
                            "Runtime row identity does not match its item.",
                            row.candidateDefinitionId,
                            row.awakeningNodeId));
                    }
                }

                if (item.stateCompleteness !=
                    ItemCoreEffectFactCompleteness.Complete)
                {
                    continue;
                }

                if (!item.cultivationLevel.HasValue
                    || item.cultivationFactCompleteness !=
                        ItemCoreEffectFactCompleteness.Complete
                    || string.IsNullOrWhiteSpace(
                        item.cultivationLevelSource))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .CultivationIdentityMissing,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "Complete runtime item requires explicit cultivation authority."));
                }

                bool inventory =
                    item.location == ItemCoreEffectRuntimeLocation.Inventory;
                bool board =
                    item.location == ItemCoreEffectRuntimeLocation.Board;
                if (!inventory && !board)
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .PlacementIdentityMismatch,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "Complete runtime item location must be Inventory or Board."));
                    continue;
                }
                if (inventory
                    && (!string.IsNullOrWhiteSpace(item.placementId)
                        || item.isLitFact !=
                            ItemCoreEffectBooleanFact.NotApplicable))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .ActiveStateContradiction,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "Inventory placement and lighting facts must be NotApplicable."));
                }
                if (board
                    && (string.IsNullOrWhiteSpace(item.placementId)
                        || !IsKnown(item.isLitFact)))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .PlacementIdentityMismatch,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        item.itemInstanceId, item.placementId, item.baseItemId,
                        "Board item requires a placementId and a known lighting fact."));
                }

                foreach (ItemInstanceCoreEffectRuntimeRow row in rows)
                {
                    ItemCoreEffectBooleanFact expectedUnlocked =
                        item.cultivationLevel.HasValue
                            ? Fact(item.cultivationLevel.Value >=
                                row.requiredLevel)
                            : ItemCoreEffectBooleanFact.Unknown;
                    if (!IsKnown(row.eligibleFact)
                        || !IsKnown(row.visibleFact)
                        || row.unlockedFact != expectedUnlocked)
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .AwakeningNodeMismatch,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            item.itemInstanceId, item.placementId,
                            item.baseItemId,
                            "Complete runtime row has unknown or contradictory projection/unlock facts.",
                            row.candidateDefinitionId,
                            row.awakeningNodeId));
                    }
                    if (inventory)
                    {
                        if (row.litFact !=
                                ItemCoreEffectBooleanFact.NotApplicable
                            || row.activeFact !=
                                ItemCoreEffectBooleanFact.NotApplicable)
                        {
                            errors.Add(Error(
                                ItemCoreEffectRuntimeStateValidationCodes
                                    .ActiveStateContradiction,
                                ItemCoreEffectRuntimeStateStatus.Invalid,
                                item.itemInstanceId, item.placementId,
                                item.baseItemId,
                                "Inventory row lighting and active facts must be NotApplicable.",
                                row.candidateDefinitionId,
                                row.awakeningNodeId));
                        }
                        continue;
                    }

                    ItemCoreEffectBooleanFact expectedActive =
                        row.eligibleFact ==
                            ItemCoreEffectBooleanFact.KnownTrue
                        && row.unlockedFact ==
                            ItemCoreEffectBooleanFact.KnownTrue
                        && item.isLitFact ==
                            ItemCoreEffectBooleanFact.KnownTrue
                            ? ItemCoreEffectBooleanFact.KnownTrue
                            : ItemCoreEffectBooleanFact.KnownFalse;
                    if (row.litFact != item.isLitFact
                        || row.activeFact != expectedActive)
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .ActiveStateContradiction,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            item.itemInstanceId, item.placementId,
                            item.baseItemId,
                            "Runtime active fact must equal rarity eligible AND cultivation unlocked AND Board lit.",
                            row.candidateDefinitionId,
                            row.awakeningNodeId));
                    }
                }
            }
            return errors;
        }

        public static IReadOnlyList<ItemCoreEffectRuntimeStateValidationError>
            Validate(ItemInstanceCoreEffectRuntimeStateInput input)
        {
            List<ItemCoreEffectRuntimeStateValidationError> errors = new();
            if (input == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.InputMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "Runtime-state input is missing."));
                return errors;
            }

            ValidateIdentity(input.identityCatalog, errors);
            ValidateCultivationInput(input, errors);
            ValidateProjection(input.projectionSet, errors);
            ValidateBinding(input.bindingSnapshot, errors);
            ValidateItemSystem(input.itemSystemSnapshot, errors);
            if (ResolveStatus(errors) == ItemCoreEffectRuntimeStateStatus.Invalid
                || input.projectionSet == null
                || input.bindingSnapshot == null
                || input.itemSystemSnapshot == null)
            {
                return errors;
            }

            ValidateCrossSources(input, errors);
            return errors;
        }

        public static IReadOnlyList<ItemCoreEffectRuntimeStateValidationError>
            ValidateCultivation(
                ItemCoreEffectRosterCompleteness completeness,
                IReadOnlyList<ItemCoreEffectCultivationRow> rows)
        {
            List<ItemCoreEffectRuntimeStateValidationError> errors = new();
            ItemCoreEffectCultivationRow[] safe = (rows ??
                    Array.Empty<ItemCoreEffectCultivationRow>())
                .Where(value => value != null).ToArray();
            foreach (IGrouping<string, ItemCoreEffectCultivationRow> duplicate in
                     safe.GroupBy(value => value.itemInstanceId,
                             StringComparer.Ordinal)
                         .Where(group => group.Count() != 1))
            {
                ItemCoreEffectCultivationRow row = duplicate.First();
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .CultivationDuplicate,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    row.itemInstanceId, null, row.baseItemId,
                    "Cultivation itemInstanceId is duplicated."));
            }
            foreach (ItemCoreEffectCultivationRow row in safe)
            {
                if (string.IsNullOrWhiteSpace(row.itemInstanceId)
                    || string.IsNullOrWhiteSpace(row.baseItemId))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .CultivationIdentityMissing,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        row.itemInstanceId, null, row.baseItemId,
                        "Cultivation identity fields are required."));
                }
                if (string.Equals(row.baseItemId, "I031",
                        StringComparison.Ordinal))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .I031OrdinaryForbidden,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        row.itemInstanceId, null, row.baseItemId,
                        "I031 cannot have an ordinary cultivation row."));
                }
                if (row.factCompleteness ==
                    ItemCoreEffectFactCompleteness.Complete)
                {
                    if (!row.inputLevel.HasValue
                        || row.inputLevel.Value < 1
                        || row.inputLevel.Value > 40)
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .CultivationLevelInvalid,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            row.itemInstanceId, null, row.baseItemId,
                            "Complete cultivation level must be within 1..40."));
                    }
                    if (string.IsNullOrWhiteSpace(row.sourceKey))
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .CultivationSourceMissing,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            row.itemInstanceId, null, row.baseItemId,
                            "Complete cultivation fact requires a sourceKey."));
                    }
                }
                else if (row.factCompleteness ==
                    ItemCoreEffectFactCompleteness.Unknown)
                {
                    if (row.inputLevel.HasValue
                        || !string.IsNullOrWhiteSpace(row.sourceKey))
                    {
                        errors.Add(Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .CultivationLevelInvalid,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            row.itemInstanceId, null, row.baseItemId,
                            "Unknown cultivation cannot expose a level or source."));
                    }
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .CultivationRosterUnknown,
                        ItemCoreEffectRuntimeStateStatus.Unknown,
                        row.itemInstanceId, null, row.baseItemId,
                        "Cultivation fact is explicitly Unknown."));
                }
                else
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .CultivationLevelInvalid,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        row.itemInstanceId, null, row.baseItemId,
                        "Cultivation does not support NotApplicable facts."));
                }
            }
            if (completeness == ItemCoreEffectRosterCompleteness.Complete
                && safe.Length == 0)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .CultivationIdentityMissing,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null,
                    "A complete cultivation roster cannot be empty."));
            }
            if (completeness != ItemCoreEffectRosterCompleteness.Complete)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .CultivationRosterUnknown,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null,
                    "Cultivation roster is not complete."));
            }
            return errors;
        }

        public static ItemCoreEffectRuntimeStateStatus ResolveStatus(
            IEnumerable<ItemCoreEffectRuntimeStateValidationError> errors,
            ItemCoreEffectRuntimeStateStatus fallback =
                ItemCoreEffectRuntimeStateStatus.Valid)
        {
            ItemCoreEffectRuntimeStateValidationError[] safe = (errors ??
                    Array.Empty<ItemCoreEffectRuntimeStateValidationError>())
                .Where(value => value != null).ToArray();
            if (safe.Any(value =>
                    value.status == ItemCoreEffectRuntimeStateStatus.Invalid))
            {
                return ItemCoreEffectRuntimeStateStatus.Invalid;
            }
            if (safe.Any(value =>
                    value.status == ItemCoreEffectRuntimeStateStatus.Unknown))
            {
                return ItemCoreEffectRuntimeStateStatus.Unknown;
            }
            return fallback;
        }

        private static bool IsKnown(ItemCoreEffectBooleanFact value)
        {
            return value == ItemCoreEffectBooleanFact.KnownFalse
                || value == ItemCoreEffectBooleanFact.KnownTrue;
        }

        private static ItemCoreEffectBooleanFact Fact(bool value)
        {
            return value
                ? ItemCoreEffectBooleanFact.KnownTrue
                : ItemCoreEffectBooleanFact.KnownFalse;
        }

        private static void ValidateIdentity(
            ItemCoreEffectIdentityCatalogSnapshot catalog,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            if (catalog == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityCatalogMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "Identity catalog is missing."));
                return;
            }
            foreach (ItemCoreEffectRuntimeStateValidationError error in
                     catalog.ValidationErrors)
            {
                errors.Add(error);
            }
            if (catalog.status == ItemCoreEffectRuntimeStateStatus.Unknown)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityCatalogMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "Identity catalog is Unknown."));
            }
            else if (!catalog.isValid
                || catalog.Rows.Count != 120
                || !string.Equals(
                    catalog.schemaId,
                    ItemCoreEffectIdentityCatalogSnapshot.CurrentSchemaId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    catalog.authorityRevision,
                    ItemCoreEffectIdentityCatalogSnapshot
                        .CurrentAuthorityRevision,
                    StringComparison.Ordinal))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .IdentityCatalogInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null,
                    "Identity catalog is not the accepted v2 120-row four-core authority."));
            }
        }

        private static void ValidateCultivationInput(
            ItemInstanceCoreEffectRuntimeStateInput input,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            if (input.cultivationRoster == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .CultivationRosterMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "Cultivation roster is missing."));
                return;
            }
            foreach (ItemCoreEffectRuntimeStateValidationError error in
                     input.cultivationRoster.ValidationErrors)
            {
                errors.Add(error);
            }
            if (input.rosterCompleteness !=
                    ItemCoreEffectRosterCompleteness.Complete
                || input.cultivationRoster.rosterCompleteness !=
                    ItemCoreEffectRosterCompleteness.Complete
                || input.cultivationRoster.status ==
                    ItemCoreEffectRuntimeStateStatus.Unknown)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .CultivationRosterUnknown,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null,
                    "Complete source roster was not supplied."));
            }
            else if (!input.cultivationRoster.isValid)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .CultivationLevelInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null, "Cultivation roster is invalid."));
            }
        }

        private static void ValidateProjection(
            ItemInstanceProjectionSetSnapshot projectionSet,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            if (projectionSet == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.ProjectionMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "ProjectionSet is missing."));
                return;
            }
            if (!projectionSet.isValid)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.ProjectionInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null, "ProjectionSet is invalid."));
            }
            foreach (IGrouping<string, ItemInstanceProjectionContractSnapshot>
                         duplicate in projectionSet.Projections
                         .Where(value => value != null)
                         .GroupBy(value => value.itemInstanceId,
                             StringComparer.Ordinal)
                         .Where(group => group.Count() != 1))
            {
                ItemInstanceProjectionContractSnapshot row = duplicate.First();
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .ProjectionIdentityDuplicate,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    row.itemInstanceId, null, row.baseItemId,
                    "Projection itemInstanceId is duplicated."));
            }
            foreach (ItemInstanceProjectionContractSnapshot projection in
                     projectionSet.Projections.Where(value => value != null))
            {
                if (string.Equals(projection.baseItemId, "I031",
                    StringComparison.Ordinal))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .ProjectionI031Forbidden,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        projection.itemInstanceId, null, projection.baseItemId,
                        "I031 cannot appear in an ordinary ProjectionSet."));
                }
            }
        }

        private static void ValidateBinding(
            ItemInstancePlacementBindingContractSnapshot binding,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            if (binding == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.BindingMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "IF01 binding snapshot is missing."));
            }
            else if (binding.status == ItemInstancePlacementBindingStatus.Unknown)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.BindingMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "IF01 binding snapshot is Unknown."));
            }
            else if (!binding.isValid)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.BindingInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null, "IF01 binding snapshot is invalid."));
            }
        }

        private static void ValidateItemSystem(
            ItemSystemSnapshot snapshot,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            if (snapshot == null)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.ItemSystemMissing,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null, "ItemSystemSnapshot.v2 is missing."));
            }
            else if (!snapshot.isValid)
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes.ItemSystemInvalid,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null, "ItemSystemSnapshot.v2 is invalid."));
            }
        }

        private static void ValidateCrossSources(
            ItemInstanceCoreEffectRuntimeStateInput input,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            Dictionary<string, ItemInstanceProjectionContractSnapshot>
                projectionByInstance = input.projectionSet.Projections
                .Where(value => value != null)
                .GroupBy(value => value.itemInstanceId, StringComparer.Ordinal)
                .Where(group => group.Count() == 1)
                .ToDictionary(group => group.Key, group => group.First(),
                    StringComparer.Ordinal);
            if (input.rosterCompleteness ==
                    ItemCoreEffectRosterCompleteness.Complete
                && input.cultivationRoster != null
                && (input.cultivationRoster.Rows.Count !=
                    projectionByInstance.Count
                    || input.cultivationRoster.Rows.Any(row =>
                        !projectionByInstance.TryGetValue(
                            row.itemInstanceId, out
                            ItemInstanceProjectionContractSnapshot projection)
                        || !string.Equals(projection.baseItemId, row.baseItemId,
                            StringComparison.Ordinal))))
            {
                errors.Add(Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .CultivationItemMismatch,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    null, null, null,
                    "Complete cultivation roster must match ProjectionSet exactly."));
            }

            Dictionary<string, ItemInstancePlacementBindingSnapshot>
                bindingByPlacement = input.bindingSnapshot.Bindings
                .Where(value => value != null)
                .ToDictionary(value => value.placementId, value => value,
                    StringComparer.Ordinal);
            foreach (ItemInstancePlacementBindingSnapshot binding in
                     input.bindingSnapshot.Bindings)
            {
                if (!projectionByInstance.TryGetValue(binding.itemInstanceId,
                        out ItemInstanceProjectionContractSnapshot projection))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes.BindingOrphan,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        binding.itemInstanceId, binding.placementId,
                        binding.baseItemId,
                        "Binding has no exact Projection."));
                    continue;
                }
                ItemSystemPlacementSnapshot placement =
                    input.itemSystemSnapshot.FindPlacement(binding.placementId);
                if (!string.Equals(projection.baseItemId, binding.baseItemId,
                        StringComparison.Ordinal)
                    || placement == null
                    || !string.Equals(placement.itemId, binding.baseItemId,
                        StringComparison.Ordinal))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes
                            .BindingIdentityMismatch,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        binding.itemInstanceId, binding.placementId,
                        binding.baseItemId,
                        "Projection, IF01 and ItemSystem identities disagree."));
                }
            }
            foreach (ItemSystemPlacementSnapshot placement in
                     input.itemSystemSnapshot.placements
                         .Where(value => value != null
                             && !string.Equals(value.itemId, "I031",
                                 StringComparison.Ordinal)))
            {
                if (!bindingByPlacement.TryGetValue(placement.placementId,
                        out ItemInstancePlacementBindingSnapshot binding)
                    || !string.Equals(binding.baseItemId, placement.itemId,
                        StringComparison.Ordinal))
                {
                    errors.Add(Error(
                        ItemCoreEffectRuntimeStateValidationCodes.PlacementOrphan,
                        ItemCoreEffectRuntimeStateStatus.Invalid,
                        binding?.itemInstanceId, placement.placementId,
                        placement.itemId,
                        "Ordinary ItemSystem placement has no exact IF01 binding."));
                }
            }
        }

        internal static ItemCoreEffectRuntimeStateValidationError Error(
            string code,
            ItemCoreEffectRuntimeStateStatus status,
            string itemInstanceId,
            string placementId,
            string baseItemId,
            string message,
            string candidateDefinitionId = null,
            string awakeningNodeId = null)
        {
            return new ItemCoreEffectRuntimeStateValidationError(
                code, status, itemInstanceId, placementId, baseItemId,
                candidateDefinitionId, awakeningNodeId, message);
        }
    }

    internal static class ItemCoreEffectRuntimeStateCanonical
    {
        public static string Required(string value)
        {
            return value?.Trim() ?? string.Empty;
        }

        public static string Optional(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public static int NodeRank(ItemCoreAwakeningNodeKind nodeKind)
        {
            return nodeKind switch
            {
                ItemCoreAwakeningNodeKind.Core1 => 0,
                ItemCoreAwakeningNodeKind.Core2 => 1,
                ItemCoreAwakeningNodeKind.Core3 => 2,
                ItemCoreAwakeningNodeKind.Ultimate => 3,
                ItemCoreAwakeningNodeKind.Core4 => int.MaxValue,
                _ => int.MaxValue
            };
        }

        public static void Field(
            StringBuilder builder,
            string name,
            string value)
        {
            string safeName = name ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeName.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeName)
                .Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append('\n');
        }

        public static void OptionalField(
            StringBuilder builder,
            string name,
            string value)
        {
            Field(builder, name + ".present", value == null ? "0" : "1");
            if (value != null)
            {
                Field(builder, name + ".value", value);
            }
        }

        public static void NullableInt(
            StringBuilder builder,
            string name,
            int? value)
        {
            OptionalField(builder, name,
                value?.ToString(CultureInfo.InvariantCulture));
        }

        public static void AppendErrors(
            StringBuilder builder,
            IEnumerable<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            ItemCoreEffectRuntimeStateValidationError[] safe =
                ItemCoreEffectRuntimeStateReadOnly.FreezeErrors(errors).ToArray();
            Field(builder, "validationErrorCount",
                safe.Length.ToString(CultureInfo.InvariantCulture));
            foreach (ItemCoreEffectRuntimeStateValidationError error in safe)
            {
                Field(builder, "validationCode", error.code);
                Field(builder, "validationStatus", error.status.ToString());
                OptionalField(builder, "validationItemInstanceId",
                    error.itemInstanceId);
                OptionalField(builder, "validationPlacementId",
                    error.placementId);
                OptionalField(builder, "validationBaseItemId",
                    error.baseItemId);
                OptionalField(builder, "validationCandidateDefinitionId",
                    error.candidateDefinitionId);
                OptionalField(builder, "validationAwakeningNodeId",
                    error.awakeningNodeId);
                Field(builder, "validationMessage", error.message);
            }
        }

        public static string Sha256(string payload)
        {
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(
                Encoding.UTF8.GetBytes(payload ?? string.Empty));
            return string.Concat(hash.Select(value =>
                value.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
