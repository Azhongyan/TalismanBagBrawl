using System.Linq;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Canonical
{
    public static class CanonicalItemInstanceFactory
    {
        public static ItemInstanceRollResult Create(
            CanonicalItemDefinitionResolver resolver,
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            long rootSeed)
        {
            if (resolver?.Snapshot?.CompiledData == null)
            {
                return Failure(ItemInstanceRollValidationCodes.CanonicalDomainInvalid,
                    "Canonical item catalog is unavailable.");
            }

            CanonicalItemDefinition definition = resolver.GetDefinition(baseItemId);
            if (definition == null || !definition.isOrdinaryDropEligible)
            {
                return Failure(ItemInstanceRollValidationCodes.FoundationIdentityInvalid,
                    "Requested item is not an ordinary generated item in the canonical catalog.");
            }

            CanonicalItemRarityProfile rarityProfile = definition.GetRarityProfile(rarity);
            if (rarityProfile == null)
            {
                return Failure(ItemInstanceRollValidationCodes.CoreProfileMissing,
                    "Canonical rarity profile is missing.");
            }

            ItemBalanceCompiledData compiled = resolver.Snapshot.CompiledData;
            ItemInstanceIdentityCreationResult identity = ItemRarityInstanceFoundation.TryCreateOrdinaryInstance(
                compiled.Foundation,
                itemInstanceId,
                definition.baseItemId,
                rarity,
                DeterministicItemRandom.SupportedGenerationVersion,
                rootSeed,
                rarityProfile.cultivationPotentialProfileId);
            if (!identity.isValid)
            {
                return new ItemInstanceRollResult(null, identity.ValidationErrors.Select(value =>
                    new ItemInstanceRollValidationError(value.code, value.message)));
            }

            ItemBuildQualificationRollProfile sourceBuildProfile = rarity == ItemInstanceRarity.White
                ? null
                : compiled.FindBuildProfile(rarity);
            ItemBuildQualificationRollProfile buildProfile = sourceBuildProfile == null
                ? null
                : new ItemBuildQualificationRollProfile(
                    "canonical_build_" + rarity.ToStableKey(),
                    rarity,
                    ItemGenerationDataStatus.PlaytestV1Canonical,
                    sourceBuildProfile.Entries);
            return ItemInstanceRollEngine.Generate(new ItemInstanceRollRequest(
                identity.snapshot,
                compiled.StatSchema,
                compiled.AffixSchema,
                compiled.CoreBuildSchema,
                buildProfile,
                ItemGenerationDataStatus.PlaytestV1Canonical));
        }

        private static ItemInstanceRollResult Failure(string code, string message)
        {
            return new ItemInstanceRollResult(null,
                new[] { new ItemInstanceRollValidationError(code, message) });
        }
    }
}
