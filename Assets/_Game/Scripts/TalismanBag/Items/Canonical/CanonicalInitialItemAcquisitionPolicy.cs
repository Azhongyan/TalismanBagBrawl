using System;
using System.Linq;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Canonical
{
    public static class CanonicalInitialItemAcquisitionPolicy
    {
        public const string PolicyId = "canonical-initial-acquisition-playtest-v1";
        public const string ItemInstanceId = "c1formal_initial_item_white_v1";
        public const long RootSeed = 1101001L;

        public static bool TryCreate(
            CanonicalItemDefinitionResolver resolver,
            out CanonicalItemDefinition definition,
            out ItemGeneratedInstanceSnapshot generatedItem,
            out string errorCode)
        {
            definition = null;
            generatedItem = null;
            errorCode = string.Empty;
            if (resolver?.Snapshot == null)
            {
                errorCode = "INITIAL_ACQUISITION_CANONICAL_CATALOG_MISSING";
                return false;
            }

            CanonicalItemDefinition[] candidates = resolver.Snapshot.OrdinaryDropDefinitions
                .Where(value => value.GetRarityProfile(ItemInstanceRarity.White) != null)
                .Where(value => string.Equals(value.primaryStatId, "damage", StringComparison.Ordinal))
                .Where(value => value.ShapeCells.Count > 0)
                .OrderBy(value => value.ShapeCells.Count)
                .ThenBy(value => value.GetRarityProfile(ItemInstanceRarity.White).candidateItemPower)
                .ThenBy(value => value.baseItemId, StringComparer.Ordinal)
                .ToArray();
            if (candidates.Length == 0)
            {
                errorCode = "INITIAL_ACQUISITION_BEGINNER_DAMAGE_ITEM_MISSING";
                return false;
            }

            definition = candidates[0];
            ItemInstanceRollResult roll = CanonicalItemInstanceFactory.Create(
                resolver,
                ItemInstanceId,
                definition.baseItemId,
                ItemInstanceRarity.White,
                RootSeed);
            if (roll?.isSuccess != true || roll.snapshot == null)
            {
                errorCode = roll?.primaryValidationCode
                            ?? "INITIAL_ACQUISITION_GENERATION_FAILED";
                definition = null;
                return false;
            }

            generatedItem = roll.snapshot;
            return true;
        }
    }
}
