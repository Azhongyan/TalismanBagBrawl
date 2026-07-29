using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Capability;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Lighting;
using UnityEngine;

namespace TalismanBag.Items.Awakening.RuntimeState
{
    public interface IItemInstanceCoreEffectRuntimeStateAssembler
    {
        ItemInstanceCoreEffectRuntimeStateSnapshot Assemble(
            ItemInstanceCoreEffectRuntimeStateInput input);
    }

    public sealed class ItemInstanceCoreEffectRuntimeStateAssembler :
        IItemInstanceCoreEffectRuntimeStateAssembler
    {
        public static readonly ItemInstanceCoreEffectRuntimeStateAssembler
            Instance = new();

        public ItemInstanceCoreEffectRuntimeStateSnapshot Assemble(
            ItemInstanceCoreEffectRuntimeStateInput input)
        {
            string identitySignature =
                input?.identityCatalog?.canonicalSignature;
            string cultivationSignature =
                input?.cultivationRoster?.canonicalSignature;
            string projectionSignature = input?.projectionSet == null
                ? null
                : ItemCoreEffectRuntimeStateCanonical.Sha256(
                    input.projectionSet.BuildCanonicalSignature());
            string bindingSignature =
                input?.bindingSnapshot?.canonicalSignature;
            string itemSystemSignature = input?.itemSystemSnapshot == null
                ? null
                : ItemCoreEffectRuntimeStateCanonical.Sha256(
                    input.itemSystemSnapshot.BuildDebugSignature());
            ItemCoreEffectRosterCompleteness completeness =
                input?.rosterCompleteness
                ?? ItemCoreEffectRosterCompleteness.Unknown;
            List<ItemCoreEffectRuntimeStateValidationError> errors =
                ItemInstanceCoreEffectRuntimeStateValidation.Validate(input)
                    .ToList();
            try
            {
                ItemCoreEffectRuntimeStateStatus status =
                    ItemInstanceCoreEffectRuntimeStateValidation.ResolveStatus(
                        errors);
                if (status == ItemCoreEffectRuntimeStateStatus.Invalid)
                {
                    return Create(status, completeness, identitySignature,
                        cultivationSignature, projectionSignature,
                        bindingSignature, itemSystemSignature, null, errors);
                }
                if (status == ItemCoreEffectRuntimeStateStatus.Unknown)
                {
                    return Create(status, completeness, identitySignature,
                        cultivationSignature, projectionSignature,
                        bindingSignature, itemSystemSignature,
                        BuildUnknownRows(input), errors);
                }

                ItemInstanceCoreEffectRuntimeItemSnapshot[] items =
                    BuildValidRows(input, errors);
                status =
                    ItemInstanceCoreEffectRuntimeStateValidation.ResolveStatus(
                        errors);
                return status == ItemCoreEffectRuntimeStateStatus.Valid
                    ? Create(status, completeness, identitySignature,
                        cultivationSignature, projectionSignature,
                        bindingSignature, itemSystemSignature, items, errors)
                    : Create(status, completeness, identitySignature,
                        cultivationSignature, projectionSignature,
                        bindingSignature, itemSystemSignature, null, errors);
            }
            catch (Exception exception)
            {
                errors.Add(ItemInstanceCoreEffectRuntimeStateValidation.Error(
                    ItemCoreEffectRuntimeStateValidationCodes.AssemblyException,
                    ItemCoreEffectRuntimeStateStatus.Unknown,
                    null, null, null,
                    "Core-effect runtime assembly did not complete: "
                    + exception.GetType().Name + "."));
                return Create(ItemCoreEffectRuntimeStateStatus.Unknown,
                    completeness, identitySignature, cultivationSignature,
                    projectionSignature, bindingSignature, itemSystemSignature,
                    BuildUnknownRows(input), errors);
            }
        }

        private static ItemInstanceCoreEffectRuntimeStateSnapshot Create(
            ItemCoreEffectRuntimeStateStatus status,
            ItemCoreEffectRosterCompleteness completeness,
            string identitySignature,
            string cultivationSignature,
            string projectionSignature,
            string bindingSignature,
            string itemSystemSignature,
            IEnumerable<ItemInstanceCoreEffectRuntimeItemSnapshot> items,
            IEnumerable<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            return new ItemInstanceCoreEffectRuntimeStateSnapshot(
                status,
                completeness,
                identitySignature,
                cultivationSignature,
                projectionSignature,
                bindingSignature,
                itemSystemSignature,
                items,
                errors);
        }

        private static ItemInstanceCoreEffectRuntimeItemSnapshot[]
            BuildValidRows(
                ItemInstanceCoreEffectRuntimeStateInput input,
                ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            var definitionProvider = new IdentityCatalogDefinitionProvider(
                input.identityCatalog);
            Dictionary<string, ItemInstancePlacementBindingSnapshot>
                bindingByInstance = input.bindingSnapshot.Bindings
                .ToDictionary(value => value.itemInstanceId, value => value,
                    StringComparer.Ordinal);
            List<ItemInstanceCoreEffectRuntimeItemSnapshot> rows = new();
            foreach (ItemInstanceProjectionContractSnapshot projection in
                     input.projectionSet.Projections
                         .OrderBy(value => value.itemInstanceId,
                             StringComparer.Ordinal))
            {
                ItemCoreEffectCultivationRow cultivation =
                    input.cultivationRoster.Find(projection.itemInstanceId);
                if (cultivation == null
                    || cultivation.factCompleteness !=
                        ItemCoreEffectFactCompleteness.Complete
                    || !cultivation.inputLevel.HasValue)
                {
                    errors.Add(
                        ItemInstanceCoreEffectRuntimeStateValidation.Error(
                            ItemCoreEffectRuntimeStateValidationCodes
                                .CultivationIdentityMissing,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            projection.itemInstanceId, null,
                            projection.baseItemId,
                            "Valid assembly requires an exact complete cultivation row."));
                    continue;
                }

                bindingByInstance.TryGetValue(projection.itemInstanceId,
                    out ItemInstancePlacementBindingSnapshot binding);
                if (binding == null)
                {
                    ItemCoreAwakeningItemResult awakening =
                        ResolveInventoryAwakening(
                            projection,
                            cultivation,
                            definitionProvider);
                    if (!ValidateAwakening(
                            projection,
                            null,
                            null,
                            cultivation,
                            awakening,
                            input.identityCatalog,
                            errors))
                    {
                        continue;
                    }
                    rows.Add(BuildItem(
                        projection,
                        null,
                        null,
                        cultivation,
                        awakening,
                        input.identityCatalog,
                        ItemCoreEffectRuntimeLocation.Inventory,
                        ItemCoreEffectBooleanFact.NotApplicable));
                    continue;
                }

                ItemSystemPlacementSnapshot placement =
                    input.itemSystemSnapshot.FindPlacement(binding.placementId);
                ItemSystemAwakeningResultSnapshot awakeningSnapshot =
                    input.itemSystemSnapshot.awakeningResults
                        .FirstOrDefault(value => value != null
                            && string.Equals(value.placementId,
                                binding.placementId,
                                StringComparison.Ordinal));
                ItemCoreAwakeningItemResult boardAwakening =
                    awakeningSnapshot == null
                        ? null
                        : input.itemSystemSnapshot
                            .ToCoreAwakeningResolutionResult()
                            .FindPlacementResult(binding.placementId);
                if (!ValidateAwakening(
                        projection,
                        binding,
                        placement,
                        cultivation,
                        boardAwakening,
                        input.identityCatalog,
                        errors))
                {
                    continue;
                }
                rows.Add(BuildItem(
                    projection,
                    binding,
                    placement,
                    cultivation,
                    boardAwakening,
                    input.identityCatalog,
                    ItemCoreEffectRuntimeLocation.Board,
                    Fact(placement.isLit)));
            }
            return rows.ToArray();
        }

        private static ItemInstanceCoreEffectRuntimeItemSnapshot BuildItem(
            ItemInstanceProjectionContractSnapshot projection,
            ItemInstancePlacementBindingSnapshot binding,
            ItemSystemPlacementSnapshot placement,
            ItemCoreEffectCultivationRow cultivation,
            ItemCoreAwakeningItemResult awakening,
            ItemCoreEffectIdentityCatalogSnapshot identityCatalog,
            ItemCoreEffectRuntimeLocation location,
            ItemCoreEffectBooleanFact itemLitFact)
        {
            bool inventory = location ==
                ItemCoreEffectRuntimeLocation.Inventory;
            List<ItemInstanceCoreEffectRuntimeRow> coreRows = new();
            foreach (ItemCoreEffectIdentityRow identity in identityCatalog.Rows
                         .Where(value => string.Equals(
                             value.baseItemId,
                             projection.baseItemId,
                             StringComparison.Ordinal))
                         .OrderBy(value =>
                             ItemCoreEffectRuntimeStateCanonical.NodeRank(
                                 value.nodeKind)))
            {
                ItemCoreAwakeningNodeState node =
                    awakening.NodeStates.Single(value =>
                        value.nodeKind == identity.nodeKind);
                bool eligible = projection.EligibleCoreEffectIds.Contains(
                    identity.candidateDefinitionId,
                    StringComparer.Ordinal);
                bool visible = projection.VisibleCoreEffectIds.Contains(
                    identity.candidateDefinitionId,
                    StringComparer.Ordinal);
                bool lit = placement?.isLit == true;
                coreRows.Add(new ItemInstanceCoreEffectRuntimeRow(
                    projection.baseItemId,
                    projection.itemInstanceId,
                    identity.nodeKind,
                    identity.candidateDefinitionId,
                    identity.awakeningNodeId,
                    identity.displayName,
                    identity.description,
                    identity.awakeningUnlockLevel,
                    identity.requiredRarity,
                    identity.requiredRarityKey,
                    Fact(eligible),
                    Fact(visible),
                    Fact(node.isUnlocked),
                    inventory
                        ? ItemCoreEffectBooleanFact.NotApplicable
                        : Fact(lit),
                    inventory
                        ? ItemCoreEffectBooleanFact.NotApplicable
                        : Fact(ResolveActiveFact(
                            eligible,
                            node.isUnlocked,
                            lit)),
                    ItemCoreEffectFactCompleteness.Complete,
                    Sources(
                        identity,
                        projection,
                        binding,
                        cultivation,
                        placement)));
            }
            return new ItemInstanceCoreEffectRuntimeItemSnapshot(
                projection.itemInstanceId,
                projection.baseItemId,
                projection.rarity,
                location,
                inventory ? null : binding.placementId,
                cultivation.inputLevel,
                cultivation.sourceKey,
                cultivation.factCompleteness,
                itemLitFact,
                coreRows,
                ItemCoreEffectFactCompleteness.Complete);
        }

        private static bool ValidateAwakening(
            ItemInstanceProjectionContractSnapshot projection,
            ItemInstancePlacementBindingSnapshot binding,
            ItemSystemPlacementSnapshot placement,
            ItemCoreEffectCultivationRow cultivation,
            ItemCoreAwakeningItemResult awakening,
            ItemCoreEffectIdentityCatalogSnapshot identityCatalog,
            ICollection<ItemCoreEffectRuntimeStateValidationError> errors)
        {
            if (awakening == null)
            {
                errors.Add(ItemInstanceCoreEffectRuntimeStateValidation.Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .AwakeningResultMissing,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    projection.itemInstanceId,
                    binding?.placementId,
                    projection.baseItemId,
                    "Existing Awakening resolver result is missing."));
                return false;
            }
            bool inventory = binding == null;
            bool headerMatches =
                string.Equals(awakening.itemId, projection.baseItemId,
                    StringComparison.Ordinal)
                && awakening.inputLevel == cultivation.inputLevel
                && awakening.resolvedLevel == cultivation.inputLevel
                && string.Equals(awakening.inputSource, cultivation.sourceKey,
                    StringComparison.Ordinal)
                && awakening.supportsAwakening
                && awakening.ValidationErrors.Count == 0
                && (inventory
                    || string.Equals(awakening.placementId,
                        binding.placementId, StringComparison.Ordinal))
                && (inventory || placement != null)
                && (inventory || awakening.isLit == placement.isLit);
            if (!headerMatches)
            {
                errors.Add(ItemInstanceCoreEffectRuntimeStateValidation.Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .AwakeningResultMismatch,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    projection.itemInstanceId,
                    binding?.placementId,
                    projection.baseItemId,
                    "Awakening result does not match identity, cultivation or lighting authority."));
                return false;
            }

            ItemCoreEffectIdentityRow[] identities = identityCatalog.Rows
                .Where(value => string.Equals(value.baseItemId,
                    projection.baseItemId, StringComparison.Ordinal))
                .ToArray();
            if (identities.Length != 4
                || awakening.NodeStates.Count != 4
                || identities.Any(value => value.nodeKind ==
                    ItemCoreAwakeningNodeKind.Core4)
                || awakening.NodeStates.Any(value => value.nodeKind ==
                    ItemCoreAwakeningNodeKind.Core4))
            {
                errors.Add(ItemInstanceCoreEffectRuntimeStateValidation.Error(
                    ItemCoreEffectRuntimeStateValidationCodes
                        .AwakeningNodeMismatch,
                    ItemCoreEffectRuntimeStateStatus.Invalid,
                    projection.itemInstanceId,
                    binding?.placementId,
                    projection.baseItemId,
                    "Awakening result must contain Core1/Core2/Core3/Ultimate exactly once."));
                return false;
            }
            foreach (ItemCoreEffectIdentityRow identity in identities)
            {
                ItemCoreAwakeningNodeState[] matches = awakening.NodeStates
                    .Where(value => value.nodeKind == identity.nodeKind)
                    .ToArray();
                if (matches.Length != 1
                    || !string.Equals(matches[0].coreEffectId,
                        identity.awakeningNodeId, StringComparison.Ordinal)
                    || matches[0].unlockLevel !=
                        identity.awakeningUnlockLevel
                    || matches[0].isActive
                        && (!matches[0].isUnlocked
                            || inventory
                            || placement?.isLit != true))
                {
                    errors.Add(
                        ItemInstanceCoreEffectRuntimeStateValidation.Error(
                            matches.Length == 1 && matches[0].isActive
                                ? ItemCoreEffectRuntimeStateValidationCodes
                                    .ActiveStateContradiction
                                : ItemCoreEffectRuntimeStateValidationCodes
                                    .AwakeningNodeMismatch,
                            ItemCoreEffectRuntimeStateStatus.Invalid,
                            projection.itemInstanceId,
                            binding?.placementId,
                            projection.baseItemId,
                            "Mapped Awakening node identity or state is contradictory.",
                            identity.candidateDefinitionId,
                            identity.awakeningNodeId));
                    return false;
                }
            }
            return true;
        }

        private static bool ResolveActiveFact(
            bool rarityEligible,
            bool cultivationUnlocked,
            bool isLit)
        {
            return rarityEligible && cultivationUnlocked && isLit;
        }

        private static ItemCoreAwakeningItemResult ResolveInventoryAwakening(
            ItemInstanceProjectionContractSnapshot projection,
            ItemCoreEffectCultivationRow cultivation,
            IItemCoreEffectDefinitionProvider definitionProvider)
        {
            ItemLightingItemResult syntheticUnlit = new(
                projection.baseItemId,
                projection.baseItemId,
                Array.Empty<Vector2Int>(),
                Vector2Int.zero,
                false,
                false,
                false,
                string.Empty,
                -1,
                "INVENTORY_" + projection.itemInstanceId,
                string.Empty);
            return ItemCoreAwakeningResolver.ResolveItem(
                syntheticUnlit,
                new ItemCoreAwakeningInput(
                    projection.baseItemId,
                    syntheticUnlit.placementId,
                    cultivation.inputLevel.Value,
                    false,
                    cultivation.sourceKey),
                definitionProvider);
        }

        private static ItemInstanceCoreEffectRuntimeItemSnapshot[]
            BuildUnknownRows(ItemInstanceCoreEffectRuntimeStateInput input)
        {
            if (input?.identityCatalog?.Rows == null
                || input.projectionSet?.Projections == null)
            {
                return Array.Empty<
                    ItemInstanceCoreEffectRuntimeItemSnapshot>();
            }
            List<ItemInstanceCoreEffectRuntimeItemSnapshot> rows = new();
            foreach (ItemInstanceProjectionContractSnapshot projection in
                     input.projectionSet.Projections
                         .Where(value => value != null
                             && !string.Equals(value.baseItemId, "I031",
                                 StringComparison.Ordinal))
                         .GroupBy(value => value.itemInstanceId,
                             StringComparer.Ordinal)
                         .Where(group => group.Count() == 1)
                         .Select(group => group.First())
                         .OrderBy(value => value.itemInstanceId,
                             StringComparer.Ordinal))
            {
                ItemInstancePlacementBindingSnapshot binding =
                    input.bindingSnapshot?.status ==
                        ItemInstancePlacementBindingStatus.Valid
                        ? input.bindingSnapshot.FindByItemInstanceId(
                            projection.itemInstanceId)
                        : null;
                ItemSystemPlacementSnapshot placement = binding == null
                    ? null
                    : input.itemSystemSnapshot?.FindPlacement(
                        binding.placementId);
                bool exactBoard = placement != null
                    && string.Equals(placement.itemId, projection.baseItemId,
                        StringComparison.Ordinal)
                    && string.Equals(binding.baseItemId,
                        projection.baseItemId, StringComparison.Ordinal);
                ItemCoreEffectCultivationRow cultivation =
                    input.cultivationRoster?.Find(projection.itemInstanceId);
                List<ItemInstanceCoreEffectRuntimeRow> coreRows = new();
                foreach (ItemCoreEffectIdentityRow identity in
                         input.identityCatalog.Rows.Where(value =>
                             string.Equals(value.baseItemId,
                                 projection.baseItemId,
                                 StringComparison.Ordinal)))
                {
                    coreRows.Add(new ItemInstanceCoreEffectRuntimeRow(
                        projection.baseItemId,
                        projection.itemInstanceId,
                        identity.nodeKind,
                        identity.candidateDefinitionId,
                        identity.awakeningNodeId,
                        identity.displayName,
                        identity.description,
                        identity.awakeningUnlockLevel,
                        identity.requiredRarity,
                        identity.requiredRarityKey,
                        Fact(projection.EligibleCoreEffectIds.Contains(
                            identity.candidateDefinitionId,
                            StringComparer.Ordinal)),
                        Fact(projection.VisibleCoreEffectIds.Contains(
                            identity.candidateDefinitionId,
                            StringComparer.Ordinal)),
                        ItemCoreEffectBooleanFact.Unknown,
                        exactBoard
                            ? Fact(placement.isLit)
                            : ItemCoreEffectBooleanFact.Unknown,
                        ItemCoreEffectBooleanFact.Unknown,
                        ItemCoreEffectFactCompleteness.Unknown,
                        Sources(identity, projection, binding, cultivation,
                            placement)));
                }
                rows.Add(new ItemInstanceCoreEffectRuntimeItemSnapshot(
                    projection.itemInstanceId,
                    projection.baseItemId,
                    projection.rarity,
                    exactBoard
                        ? ItemCoreEffectRuntimeLocation.Board
                        : ItemCoreEffectRuntimeLocation.Unknown,
                    exactBoard ? binding.placementId : null,
                    cultivation?.factCompleteness ==
                        ItemCoreEffectFactCompleteness.Complete
                        ? cultivation.inputLevel
                        : null,
                    cultivation?.factCompleteness ==
                        ItemCoreEffectFactCompleteness.Complete
                        ? cultivation.sourceKey
                        : null,
                    cultivation?.factCompleteness
                        ?? ItemCoreEffectFactCompleteness.Unknown,
                    exactBoard
                        ? Fact(placement.isLit)
                        : ItemCoreEffectBooleanFact.Unknown,
                    coreRows,
                    ItemCoreEffectFactCompleteness.Unknown));
            }
            return rows.ToArray();
        }

        private static IEnumerable<string> Sources(
            ItemCoreEffectIdentityRow identity,
            ItemInstanceProjectionContractSnapshot projection,
            ItemInstancePlacementBindingSnapshot binding,
            ItemCoreEffectCultivationRow cultivation,
            ItemSystemPlacementSnapshot placement)
        {
            yield return identity.candidateSourceIdentity;
            yield return identity.awakeningSourceIdentity;
            yield return identity.candidatePayloadIdentity;
            yield return "Projection/"
                + ItemCoreEffectRuntimeStateCanonical.Sha256(
                    projection.BuildCanonicalSignature());
            if (cultivation != null
                && !string.IsNullOrWhiteSpace(cultivation.sourceKey))
            {
                yield return "Cultivation/" + cultivation.sourceKey;
            }
            if (binding != null)
            {
                yield return "IF01/" + binding.itemInstanceId + "/"
                    + binding.placementId + "/" + binding.baseItemId;
            }
            if (placement != null)
            {
                yield return "ItemSystemSnapshot.v2/"
                    + placement.placementId + "/" + placement.itemId;
            }
        }

        private static ItemCoreEffectBooleanFact Fact(bool value)
        {
            return value
                ? ItemCoreEffectBooleanFact.KnownTrue
                : ItemCoreEffectBooleanFact.KnownFalse;
        }

        private sealed class IdentityCatalogDefinitionProvider :
            IItemCoreEffectDefinitionProvider
        {
            private readonly ItemCoreEffectIdentityCatalogSnapshot catalog;

            public IdentityCatalogDefinitionProvider(
                ItemCoreEffectIdentityCatalogSnapshot catalog)
            {
                this.catalog = catalog;
            }

            public IReadOnlyList<ItemCoreEffectDefinition> GetDefinitions(
                string itemId)
            {
                return (catalog?.Rows ??
                        Array.Empty<ItemCoreEffectIdentityRow>())
                    .Where(value => string.Equals(
                        value.baseItemId, itemId, StringComparison.Ordinal))
                    .OrderBy(value =>
                        ItemCoreEffectRuntimeStateCanonical.NodeRank(
                            value.nodeKind))
                    .Select(value => new ItemCoreEffectDefinition(
                        value.baseItemId,
                        value.awakeningNodeId,
                        value.nodeKind,
                        value.awakeningUnlockLevel,
                        value.description,
                        value.requiredRarityKey))
                    .ToArray();
            }
        }
    }
}
