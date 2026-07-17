using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.InnerCatalog;

namespace TalismanBag.Items.Generation.Projection
{
    public static class ItemInstanceProjectionProvider
    {
        public static ItemInstanceProjectionResult Project(ItemInstanceProjectionRequest request)
        {
            if (request == null)
            {
                return ProjectionFailure(ItemInstanceProjectionValidationCodes.RequestNull,
                    "Item instance projection request is null.");
            }

            if (request.generatedInstance == null)
            {
                return ProjectionFailure(ItemInstanceProjectionValidationCodes.GeneratedInstanceNull,
                    "Generated item instance is null.");
            }

            try
            {
                IReadOnlyList<ItemInstanceProjectionValidationError> errors =
                    ValidateGeneratedInstance(request.generatedInstance);
                if (errors.Count > 0)
                {
                    return new ItemInstanceProjectionResult(null, errors);
                }

                return new ItemInstanceProjectionResult(
                    CreateProjection(request.generatedInstance),
                    Array.Empty<ItemInstanceProjectionValidationError>());
            }
            catch (Exception exception)
            {
                return ProjectionFailure(ItemInstanceProjectionValidationCodes.ProjectionInternalError,
                    "Projection failed without exposing an exception: " + exception.GetType().Name + ".");
            }
        }

        public static ItemInstanceProjectionSetResult ProjectSet(ItemInstanceProjectionSetRequest request)
        {
            if (request == null)
            {
                return SetFailure(ItemInstanceProjectionValidationCodes.RequestNull,
                    "Item instance projection set request is null.");
            }

            if (!request.hasInput || request.GeneratedInstances == null)
            {
                return SetFailure(ItemInstanceProjectionValidationCodes.SetInputNull,
                    "Generated instance set input is null.");
            }

            try
            {
                List<ItemInstanceProjectionValidationError> errors = new();
                IReadOnlyList<ItemGeneratedInstanceSnapshot> sources = request.GeneratedInstances;
                for (int index = 0; index < sources.Count; index++)
                {
                    ItemGeneratedInstanceSnapshot source = sources[index];
                    if (source == null)
                    {
                        Add(errors, ItemInstanceProjectionValidationCodes.SetEntryNull,
                            $"Generated instance set entry {index} is null.");
                        continue;
                    }

                    foreach (ItemInstanceProjectionValidationError error in ValidateGeneratedInstance(source))
                    {
                        Add(errors, error.code,
                            $"Set entry {index} ('{source.itemInstanceId ?? string.Empty}'): {error.message}");
                    }
                }

                foreach (IGrouping<string, ItemGeneratedInstanceSnapshot> duplicate in sources
                    .Where(value => value != null && !string.IsNullOrWhiteSpace(value.itemInstanceId))
                    .GroupBy(value => value.itemInstanceId.Trim(), StringComparer.Ordinal)
                    .Where(group => group.Count() > 1)
                    .OrderBy(group => group.Key, StringComparer.Ordinal))
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.ItemInstanceIdDuplicate,
                        $"itemInstanceId '{duplicate.Key}' appears {duplicate.Count()} times in the projection set.");
                }

                if (errors.Count > 0)
                {
                    return new ItemInstanceProjectionSetResult(null, errors);
                }

                ItemInstanceProjectionContractSnapshot[] projections = sources
                    .Select(CreateProjection)
                    .OrderBy(value => value.itemInstanceId, StringComparer.Ordinal)
                    .ToArray();
                ItemInstanceProjectionSetSnapshot snapshot = new(
                    projections,
                    Array.Empty<ItemInstanceProjectionValidationError>());
                return new ItemInstanceProjectionSetResult(
                    snapshot,
                    Array.Empty<ItemInstanceProjectionValidationError>());
            }
            catch (Exception exception)
            {
                return SetFailure(ItemInstanceProjectionValidationCodes.ProjectionInternalError,
                    "Projection set failed without exposing an exception: " + exception.GetType().Name + ".");
            }
        }

        private static IReadOnlyList<ItemInstanceProjectionValidationError> ValidateGeneratedInstance(
            ItemGeneratedInstanceSnapshot source)
        {
            List<ItemInstanceProjectionValidationError> errors = new();
            if (source == null)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.GeneratedInstanceNull,
                    "Generated item instance is null.");
                return ProjectionReadOnly.Freeze(errors);
            }

            if (!string.Equals(source.schemaId, ItemGeneratedInstanceSnapshot.CurrentSchemaId,
                StringComparison.Ordinal))
            {
                Add(errors, ItemInstanceProjectionValidationCodes.SourceSchemaInvalid,
                    $"source schema must be '{ItemGeneratedInstanceSnapshot.CurrentSchemaId}'.");
            }

            if (string.IsNullOrWhiteSpace(source.generationAlgorithmId))
            {
                Add(errors, ItemInstanceProjectionValidationCodes.SourceAlgorithmIdEmpty,
                    "source generationAlgorithmId is required.");
            }

            if (string.IsNullOrWhiteSpace(source.generationDataStatus))
            {
                Add(errors, ItemInstanceProjectionValidationCodes.GenerationDataStatusEmpty,
                    "source generationDataStatus is required and is passed through without promotion.");
            }

            string itemInstanceId = Normalize(source.itemInstanceId);
            if (itemInstanceId.Length == 0)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.ItemInstanceIdEmpty,
                    "source itemInstanceId is required.");
            }

            ValidateBaseItem(source.baseItemId, errors);

            if (!ItemInstanceRarityCatalog.TryGetDefinition(source.rarity, out _))
            {
                Add(errors, ItemInstanceProjectionValidationCodes.RarityInvalid,
                    $"source rarity enum value '{source.rarity}' is not supported.");
            }

            string rarityKey = source.rarity.ToStableKey();
            if (rarityKey.Length == 0
                || !ItemInstanceRarityCatalog.TryParseStableKey(rarityKey, out ItemInstanceRarity parsedRarity)
                || parsedRarity != source.rarity)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.RarityKeyInvalid,
                    "source rarity must map exactly to white/green/blue/purple/orange.");
            }

            if (source.generationVersion <= 0)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.GenerationVersionInvalid,
                    "source generationVersion must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(source.cultivationPotentialProfileId))
            {
                Add(errors, ItemInstanceProjectionValidationCodes.CultivationProfileIdEmpty,
                    "source cultivationPotentialProfileId is required.");
            }

            ValidateStats(source.GeneratedStats, errors);
            ValidateAffixes(source.GeneratedAffixes, errors);
            ValidateCorePotential(source.GeneratedCorePotential, errors);
            ValidateBuildQualification(source.rarity, source.buildQualification, errors);
            return ProjectionReadOnly.Freeze(errors);
        }

        private static void ValidateBaseItem(
            string sourceBaseItemId,
            List<ItemInstanceProjectionValidationError> errors)
        {
            string baseItemId = Normalize(sourceBaseItemId);
            if (baseItemId.Length == 0)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.BaseItemIdEmpty,
                    "source baseItemId is required.");
                return;
            }

            ItemInnerDataDefinition definition = ItemInnerDataCatalog.FindById(baseItemId);
            if (definition == null)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.BaseItemUnknown,
                    $"source baseItemId '{baseItemId}' does not exist in the Item Catalog.");
                return;
            }

            if (string.Equals(baseItemId, ItemRarityInstanceFoundation.CoreProgressionBaseItemId,
                StringComparison.Ordinal))
            {
                Add(errors, ItemInstanceProjectionValidationCodes.I031Forbidden,
                    "I031 is a directed core-progression item and cannot enter ordinary instance projection.");
                Add(errors, ItemInstanceProjectionValidationCodes.BaseItemNotOrdinary,
                    "I031 is not an ordinary I001-I030 generated item.");
                return;
            }

            if (!ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(baseItemId))
            {
                Add(errors, ItemInstanceProjectionValidationCodes.BaseItemNotOrdinary,
                    $"source baseItemId '{baseItemId}' is not an ordinary generated item.");
            }
        }

        private static void ValidateStats(
            IReadOnlyList<ItemGeneratedStatSnapshot> stats,
            List<ItemInstanceProjectionValidationError> errors)
        {
            if (stats == null)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.StatEntryNull,
                    "source stat collection is null.");
                return;
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            for (int index = 0; index < stats.Count; index++)
            {
                ItemGeneratedStatSnapshot stat = stats[index];
                if (stat == null)
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.StatEntryNull,
                        $"source stat entry {index} is null.");
                    continue;
                }

                string statId = Normalize(stat.statId);
                if (statId.Length == 0)
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.StatIdEmpty,
                        $"source stat entry {index} has an empty statId.");
                }
                else if (!ids.Add(statId))
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.StatIdDuplicate,
                        $"source statId '{statId}' is duplicated.");
                }
            }
        }

        private static void ValidateAffixes(
            IReadOnlyList<ItemGeneratedAffixSnapshot> affixes,
            List<ItemInstanceProjectionValidationError> errors)
        {
            if (affixes == null)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.AffixEntryNull,
                    "source affix collection is null.");
                return;
            }

            HashSet<string> slotIds = new(StringComparer.Ordinal);
            for (int index = 0; index < affixes.Count; index++)
            {
                ItemGeneratedAffixSnapshot affix = affixes[index];
                if (affix == null)
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.AffixEntryNull,
                        $"source affix entry {index} is null.");
                    continue;
                }

                string slotId = Normalize(affix.slotId);
                if (slotId.Length == 0)
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.AffixSlotIdEmpty,
                        $"source affix entry {index} has an empty slotId.");
                }
                else if (!slotIds.Add(slotId))
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.AffixSlotIdDuplicate,
                        $"source affix slotId '{slotId}' is duplicated.");
                }

                if (!Enum.IsDefined(typeof(ItemAffixSlotKind), affix.slotKind))
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.AffixSlotKindInvalid,
                        $"source affix slot '{slotId}' has an unsupported slotKind.");
                }

                if (string.IsNullOrWhiteSpace(affix.affixId))
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.AffixIdEmpty,
                        $"source affix slot '{slotId}' has an empty affixId.");
                }

                if (string.IsNullOrWhiteSpace(affix.affixValueProfileId))
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.AffixValueProfileIdEmpty,
                        $"source affix slot '{slotId}' has an empty affixValueProfileId.");
                }
            }
        }

        private static void ValidateCorePotential(
            ItemGeneratedCorePotentialSnapshot core,
            List<ItemInstanceProjectionValidationError> errors)
        {
            if (core == null)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.CorePotentialNull,
                    "source core potential snapshot is null.");
                return;
            }

            HashSet<string> eligible = ValidateCoreIds(core.EligibleCoreEffectIds, true, errors);
            HashSet<string> visible = ValidateCoreIds(core.VisibleCoreEffectIds, false, errors);
            foreach (string effectId in visible.OrderBy(value => value, StringComparer.Ordinal))
            {
                if (!eligible.Contains(effectId))
                {
                    Add(errors, ItemInstanceProjectionValidationCodes.CoreVisibleNotEligible,
                        $"visible core effect '{effectId}' is not present in eligible core effects.");
                }
            }
        }

        private static HashSet<string> ValidateCoreIds(
            IReadOnlyList<string> source,
            bool eligibleList,
            List<ItemInstanceProjectionValidationError> errors)
        {
            HashSet<string> ids = new(StringComparer.Ordinal);
            if (source == null)
            {
                Add(errors,
                    eligibleList
                        ? ItemInstanceProjectionValidationCodes.CoreEligibleIdEmpty
                        : ItemInstanceProjectionValidationCodes.CoreVisibleIdEmpty,
                    eligibleList ? "eligible core effect collection is null." : "visible core effect collection is null.");
                return ids;
            }

            for (int index = 0; index < source.Count; index++)
            {
                string effectId = Normalize(source[index]);
                if (effectId.Length == 0)
                {
                    Add(errors,
                        eligibleList
                            ? ItemInstanceProjectionValidationCodes.CoreEligibleIdEmpty
                            : ItemInstanceProjectionValidationCodes.CoreVisibleIdEmpty,
                        $"{(eligibleList ? "eligible" : "visible")} core effect entry {index} is empty.");
                }
                else if (!ids.Add(effectId))
                {
                    Add(errors,
                        eligibleList
                            ? ItemInstanceProjectionValidationCodes.CoreEligibleIdDuplicate
                            : ItemInstanceProjectionValidationCodes.CoreVisibleIdDuplicate,
                        $"{(eligibleList ? "eligible" : "visible")} core effect '{effectId}' is duplicated.");
                }
            }

            return ids;
        }

        private static void ValidateBuildQualification(
            ItemInstanceRarity rarity,
            ItemBuildQualification qualification,
            List<ItemInstanceProjectionValidationError> errors)
        {
            if (!Enum.IsDefined(typeof(ItemBuildQualification), qualification)
                || qualification == ItemBuildQualification.Unresolved)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.BuildQualificationInvalid,
                    $"source BuildQualification '{qualification}' is not a resolved contract value.");
            }

            if (rarity == ItemInstanceRarity.White && qualification != ItemBuildQualification.None)
            {
                Add(errors, ItemInstanceProjectionValidationCodes.WhiteBuildQualificationMustBeNone,
                    "white rarity must use BuildQualification.None.");
            }
        }

        private static ItemInstanceProjectionContractSnapshot CreateProjection(
            ItemGeneratedInstanceSnapshot source)
        {
            return new ItemInstanceProjectionContractSnapshot(
                source.schemaId,
                source.generationAlgorithmId,
                source.generationDataStatus,
                source.itemInstanceId,
                source.baseItemId,
                source.rarity,
                source.rarity.ToStableKey(),
                source.generationVersion,
                source.rootSeed,
                source.cultivationPotentialProfileId,
                source.GeneratedStats.Select(value =>
                    new ItemInstanceProjectionStatSnapshot(value.statId, value.rawUnits)),
                source.GeneratedAffixes.Select(value =>
                    new ItemInstanceProjectionAffixSnapshot(value.slotId, value.slotKind,
                        value.affixId, value.affixValueProfileId, value.rawUnits)),
                source.GeneratedCorePotential.EligibleCoreEffectIds,
                source.GeneratedCorePotential.VisibleCoreEffectIds,
                source.buildQualification,
                source.BuildCanonicalSignature());
        }

        private static ItemInstanceProjectionResult ProjectionFailure(string code, string message)
        {
            return new ItemInstanceProjectionResult(null,
                new[] { new ItemInstanceProjectionValidationError(code, message) });
        }

        private static ItemInstanceProjectionSetResult SetFailure(string code, string message)
        {
            return new ItemInstanceProjectionSetResult(null,
                new[] { new ItemInstanceProjectionValidationError(code, message) });
        }

        private static void Add(
            ICollection<ItemInstanceProjectionValidationError> errors,
            string code,
            string message)
        {
            errors.Add(new ItemInstanceProjectionValidationError(code, message));
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
