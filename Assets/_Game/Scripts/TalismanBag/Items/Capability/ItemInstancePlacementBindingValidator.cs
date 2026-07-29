using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Generation.Projection;

namespace TalismanBag.Items.Capability
{
    public interface IItemInstancePlacementBindingValidator
    {
        ItemInstancePlacementBindingValidationResult Validate(
            ItemInstanceProjectionSetSnapshot instanceProjection,
            ItemSystemSnapshot placementSnapshot,
            IReadOnlyList<ItemInstancePlacementBindingInput> explicitBindings);
    }

    public sealed class ItemInstancePlacementBindingValidator :
        IItemInstancePlacementBindingValidator
    {
        public static readonly ItemInstancePlacementBindingValidator Instance =
            new ItemInstancePlacementBindingValidator();

        public ItemInstancePlacementBindingValidationResult Validate(
            ItemInstanceProjectionSetSnapshot instanceProjection,
            ItemSystemSnapshot placementSnapshot,
            IReadOnlyList<ItemInstancePlacementBindingInput> explicitBindings)
        {
            List<ItemInstancePlacementBindingValidationError> errors =
                new List<ItemInstancePlacementBindingValidationError>();
            List<ItemInstancePlacementBindingSnapshot> verifiedBindings =
                new List<ItemInstancePlacementBindingSnapshot>();

            bool projectionTrustworthy = instanceProjection != null;
            bool placementTrustworthy = placementSnapshot != null;

            if (instanceProjection == null)
            {
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.ProjectionSourceMissing,
                    ItemInstancePlacementBindingStatus.Unknown,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "ItemInstanceProjectionContractSnapshot.v1 source is required.");
            }
            else if (!instanceProjection.isValid)
            {
                projectionTrustworthy = false;
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.ProjectionSourceInvalid,
                    ItemInstancePlacementBindingStatus.Invalid,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "Instance Projection source contains validation errors.");
            }

            if (placementSnapshot == null)
            {
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.PlacementSourceMissing,
                    ItemInstancePlacementBindingStatus.Unknown,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "ItemSystemSnapshot.v2 placement source is required.");
            }
            else if (!placementSnapshot.isValid)
            {
                placementTrustworthy = false;
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.PlacementSourceInvalid,
                    ItemInstancePlacementBindingStatus.Invalid,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "Placement source contains validation errors.");
            }

            ItemInstanceProjectionContractSnapshot[] projections =
                (instanceProjection?.Projections ??
                    Array.Empty<ItemInstanceProjectionContractSnapshot>())
                .Where(value => value != null)
                .ToArray();
            ItemSystemPlacementSnapshot[] placements =
                (placementSnapshot?.placements ?? Array.Empty<ItemSystemPlacementSnapshot>())
                .Where(value => value != null)
                .ToArray();
            ItemInstancePlacementBindingInput[] bindings =
                (explicitBindings ?? Array.Empty<ItemInstancePlacementBindingInput>())
                .ToArray();

            foreach (ItemInstanceProjectionContractSnapshot projection in projections)
            {
                if (string.IsNullOrWhiteSpace(projection.itemInstanceId))
                {
                    projectionTrustworthy = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.ItemInstanceIdMissing,
                        ItemInstancePlacementBindingStatus.Invalid,
                        projection.itemInstanceId,
                        string.Empty,
                        projection.baseItemId,
                        "Projection itemInstanceId must be non-empty.");
                }

                if (string.IsNullOrWhiteSpace(projection.baseItemId))
                {
                    projectionTrustworthy = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.BaseItemIdMissing,
                        ItemInstancePlacementBindingStatus.Invalid,
                        projection.itemInstanceId,
                        string.Empty,
                        projection.baseItemId,
                        "Projection baseItemId must be non-empty.");
                }

                if (IsI031(projection.baseItemId))
                {
                    projectionTrustworthy = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.I031OrdinaryInstanceForbidden,
                        ItemInstancePlacementBindingStatus.Invalid,
                        projection.itemInstanceId,
                        string.Empty,
                        projection.baseItemId,
                        "I031 is a system JuNian placement and must not be projected as an ordinary generated instance.");
                }
            }

            IGrouping<string, ItemInstanceProjectionContractSnapshot>[] duplicateProjections =
                projections
                    .Where(value => !string.IsNullOrWhiteSpace(value.itemInstanceId))
                    .GroupBy(value => value.itemInstanceId, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .ToArray();
            foreach (IGrouping<string, ItemInstanceProjectionContractSnapshot> group in duplicateProjections)
            {
                projectionTrustworthy = false;
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.ProjectionItemInstanceIdDuplicate,
                    ItemInstancePlacementBindingStatus.Invalid,
                    group.Key,
                    string.Empty,
                    group.First().baseItemId,
                    "Instance Projection itemInstanceId must be unique; actual rows "
                    + group.Count().ToString(CultureInfo.InvariantCulture) + ".");
            }

            foreach (ItemSystemPlacementSnapshot placement in placements)
            {
                if (string.IsNullOrWhiteSpace(placement.placementId))
                {
                    placementTrustworthy = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.PlacementIdMissing,
                        ItemInstancePlacementBindingStatus.Invalid,
                        string.Empty,
                        placement.placementId,
                        placement.itemId,
                        "Placement placementId must be non-empty.");
                }

                if (string.IsNullOrWhiteSpace(placement.itemId))
                {
                    placementTrustworthy = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.BaseItemIdMissing,
                        ItemInstancePlacementBindingStatus.Invalid,
                        string.Empty,
                        placement.placementId,
                        placement.itemId,
                        "Placement.itemId must be non-empty.");
                }
            }

            IGrouping<string, ItemSystemPlacementSnapshot>[] duplicatePlacements =
                placements
                    .Where(value => !string.IsNullOrWhiteSpace(value.placementId))
                    .GroupBy(value => value.placementId, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .ToArray();
            foreach (IGrouping<string, ItemSystemPlacementSnapshot> group in duplicatePlacements)
            {
                placementTrustworthy = false;
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.PlacementIdDuplicate,
                    ItemInstancePlacementBindingStatus.Invalid,
                    string.Empty,
                    group.Key,
                    group.First().itemId,
                    "Placement source placementId must be unique; actual rows "
                    + group.Count().ToString(CultureInfo.InvariantCulture) + ".");
            }

            HashSet<string> duplicateBindingItemInstanceIds = DuplicateIds(
                bindings.Where(value => value != null).Select(value => value.itemInstanceId));
            HashSet<string> duplicateBindingPlacementIds = DuplicateIds(
                bindings.Where(value => value != null).Select(value => value.placementId));

            foreach (string itemInstanceId in duplicateBindingItemInstanceIds.OrderBy(
                value => value,
                StringComparer.Ordinal))
            {
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.BindingItemInstanceIdDuplicate,
                    ItemInstancePlacementBindingStatus.Invalid,
                    itemInstanceId,
                    string.Empty,
                    string.Empty,
                    "Explicit binding itemInstanceId must be unique.");
            }

            foreach (string placementId in duplicateBindingPlacementIds.OrderBy(
                value => value,
                StringComparer.Ordinal))
            {
                AddError(errors,
                    ItemInstancePlacementBindingValidationCodes.BindingPlacementIdDuplicate,
                    ItemInstancePlacementBindingStatus.Invalid,
                    string.Empty,
                    placementId,
                    string.Empty,
                    "Explicit binding placementId must be unique.");
            }

            ILookup<string, ItemInstanceProjectionContractSnapshot> projectionsByInstance =
                projections.ToLookup(value => value.itemInstanceId, StringComparer.Ordinal);
            ILookup<string, ItemSystemPlacementSnapshot> placementsById =
                placements.ToLookup(value => value.placementId, StringComparer.Ordinal);

            foreach (ItemInstancePlacementBindingInput binding in bindings)
            {
                if (binding == null)
                {
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.BindingRowMissing,
                        ItemInstancePlacementBindingStatus.Unknown,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        "Explicit binding row is null.");
                    continue;
                }

                bool identityComplete = true;
                if (binding.itemInstanceId.Length == 0)
                {
                    identityComplete = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.ItemInstanceIdMissing,
                        ItemInstancePlacementBindingStatus.Unknown,
                        binding.itemInstanceId,
                        binding.placementId,
                        binding.baseItemId,
                        "Explicit binding itemInstanceId is required; it cannot be inferred from placementId.");
                }

                if (binding.placementId.Length == 0)
                {
                    identityComplete = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.PlacementIdMissing,
                        ItemInstancePlacementBindingStatus.Unknown,
                        binding.itemInstanceId,
                        binding.placementId,
                        binding.baseItemId,
                        "Explicit binding placementId is required; it cannot be inferred from itemInstanceId.");
                }

                if (binding.baseItemId.Length == 0)
                {
                    identityComplete = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.BaseItemIdMissing,
                        ItemInstancePlacementBindingStatus.Unknown,
                        binding.itemInstanceId,
                        binding.placementId,
                        binding.baseItemId,
                        "Explicit binding baseItemId is required.");
                }

                if (!identityComplete)
                {
                    continue;
                }

                if (IsI031(binding.baseItemId))
                {
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.I031OrdinaryInstanceForbidden,
                        ItemInstancePlacementBindingStatus.Invalid,
                        binding.itemInstanceId,
                        binding.placementId,
                        binding.baseItemId,
                        "I031 must not be forged as an ordinary generated item instance binding.");
                    continue;
                }

                if (duplicateBindingItemInstanceIds.Contains(binding.itemInstanceId)
                    || duplicateBindingPlacementIds.Contains(binding.placementId))
                {
                    continue;
                }

                ItemInstanceProjectionContractSnapshot[] matchingProjections =
                    projectionsByInstance[binding.itemInstanceId].ToArray();
                ItemSystemPlacementSnapshot[] matchingPlacements =
                    placementsById[binding.placementId].ToArray();
                bool rowValid = true;

                if (matchingProjections.Length == 0)
                {
                    rowValid = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.ProjectionOrphan,
                        ItemInstancePlacementBindingStatus.Unknown,
                        binding.itemInstanceId,
                        binding.placementId,
                        binding.baseItemId,
                        "Explicit binding has no matching Instance Projection itemInstanceId.");
                }
                else if (matchingProjections.Length > 1)
                {
                    rowValid = false;
                }

                if (matchingPlacements.Length == 0)
                {
                    rowValid = false;
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.PlacementOrphan,
                        ItemInstancePlacementBindingStatus.Unknown,
                        binding.itemInstanceId,
                        binding.placementId,
                        binding.baseItemId,
                        "Explicit binding has no matching Placement.placementId.");
                }
                else if (matchingPlacements.Length > 1)
                {
                    rowValid = false;
                }

                if (matchingProjections.Length == 1)
                {
                    ItemInstanceProjectionContractSnapshot projection = matchingProjections[0];
                    if (IsI031(projection.baseItemId))
                    {
                        rowValid = false;
                        AddError(errors,
                            ItemInstancePlacementBindingValidationCodes.I031OrdinaryInstanceForbidden,
                            ItemInstancePlacementBindingStatus.Invalid,
                            binding.itemInstanceId,
                            binding.placementId,
                            projection.baseItemId,
                            "Instance Projection must not represent I031 as an ordinary generated instance.");
                    }
                    else if (!string.Equals(
                        projection.baseItemId,
                        binding.baseItemId,
                        StringComparison.Ordinal))
                    {
                        rowValid = false;
                        AddError(errors,
                            ItemInstancePlacementBindingValidationCodes.ProjectionBaseItemIdMismatch,
                            ItemInstancePlacementBindingStatus.Invalid,
                            binding.itemInstanceId,
                            binding.placementId,
                            binding.baseItemId,
                            "Binding baseItemId must match Instance Projection baseItemId '"
                            + projection.baseItemId + "'.");
                    }
                }

                if (matchingPlacements.Length == 1)
                {
                    ItemSystemPlacementSnapshot placement = matchingPlacements[0];
                    if (IsI031(placement.itemId))
                    {
                        rowValid = false;
                        AddError(errors,
                            ItemInstancePlacementBindingValidationCodes.I031OrdinaryInstanceForbidden,
                            ItemInstancePlacementBindingStatus.Invalid,
                            binding.itemInstanceId,
                            binding.placementId,
                            placement.itemId,
                            "I031 placement is a system source and cannot receive an ordinary instance binding.");
                    }
                    else if (!string.Equals(
                        placement.itemId,
                        binding.baseItemId,
                        StringComparison.Ordinal))
                    {
                        rowValid = false;
                        AddError(errors,
                            ItemInstancePlacementBindingValidationCodes.PlacementBaseItemIdMismatch,
                            ItemInstancePlacementBindingStatus.Invalid,
                            binding.itemInstanceId,
                            binding.placementId,
                            binding.baseItemId,
                            "Binding baseItemId must match Placement.itemId '"
                            + placement.itemId + "'.");
                    }
                }

                if (rowValid && projectionTrustworthy && placementTrustworthy)
                {
                    verifiedBindings.Add(new ItemInstancePlacementBindingSnapshot(
                        binding.itemInstanceId,
                        binding.placementId,
                        binding.baseItemId));
                }
            }

            HashSet<string> representedItemInstances = new HashSet<string>(
                bindings.Where(value => value != null && value.itemInstanceId.Length > 0)
                    .Select(value => value.itemInstanceId),
                StringComparer.Ordinal);
            HashSet<string> representedPlacements = new HashSet<string>(
                bindings.Where(value => value != null && value.placementId.Length > 0)
                    .Select(value => value.placementId),
                StringComparer.Ordinal);

            foreach (ItemInstanceProjectionContractSnapshot projection in projections
                .Where(value => !string.IsNullOrWhiteSpace(value.itemInstanceId)
                    && !IsI031(value.baseItemId))
                .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal))
            {
                if (!representedItemInstances.Contains(projection.itemInstanceId))
                {
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.ProjectionBindingMissing,
                        ItemInstancePlacementBindingStatus.Unknown,
                        projection.itemInstanceId,
                        string.Empty,
                        projection.baseItemId,
                        "Instance Projection has no explicit binding row; equality with a placementId is not evidence.");
                }
            }

            foreach (ItemSystemPlacementSnapshot placement in placements
                .Where(value => !string.IsNullOrWhiteSpace(value.placementId)
                    && !IsI031(value.itemId))
                .OrderBy(value => value.placementId, StringComparer.Ordinal))
            {
                if (!representedPlacements.Contains(placement.placementId))
                {
                    AddError(errors,
                        ItemInstancePlacementBindingValidationCodes.PlacementBindingMissing,
                        ItemInstancePlacementBindingStatus.Unknown,
                        string.Empty,
                        placement.placementId,
                        placement.itemId,
                        "Placement has no explicit binding row; equality with an itemInstanceId is not evidence.");
                }
            }

            ItemInstancePlacementBindingValidationError[] normalizedErrors = errors
                .GroupBy(value => value.status + "|" + value.code + "|"
                    + value.itemInstanceId + "|" + value.placementId + "|"
                    + value.baseItemId + "|" + value.message,
                    StringComparer.Ordinal)
                .Select(group => group.First())
                .ToArray();
            ItemInstancePlacementBindingStatus overallStatus =
                normalizedErrors.Any(value => value.status ==
                    ItemInstancePlacementBindingStatus.Invalid)
                    ? ItemInstancePlacementBindingStatus.Invalid
                    : normalizedErrors.Any(value => value.status ==
                        ItemInstancePlacementBindingStatus.Unknown)
                        ? ItemInstancePlacementBindingStatus.Unknown
                        : ItemInstancePlacementBindingStatus.Valid;

            return new ItemInstancePlacementBindingValidationResult(
                new ItemInstancePlacementBindingContractSnapshot(
                    overallStatus,
                    verifiedBindings,
                    normalizedErrors));
        }

        private static HashSet<string> DuplicateIds(IEnumerable<string> ids)
        {
            return new HashSet<string>((ids ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .GroupBy(value => value, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key),
                StringComparer.Ordinal);
        }

        private static bool IsI031(string value)
        {
            return string.Equals(value, "I031", StringComparison.Ordinal);
        }

        private static void AddError(
            ICollection<ItemInstancePlacementBindingValidationError> errors,
            string code,
            ItemInstancePlacementBindingStatus status,
            string itemInstanceId,
            string placementId,
            string baseItemId,
            string message)
        {
            errors.Add(new ItemInstancePlacementBindingValidationError(
                code,
                status,
                itemInstanceId,
                placementId,
                baseItemId,
                message));
        }
    }
}
