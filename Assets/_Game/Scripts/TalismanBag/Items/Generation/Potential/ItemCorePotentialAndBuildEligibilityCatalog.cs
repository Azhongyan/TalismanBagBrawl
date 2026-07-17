using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TalismanBag.Items.Generation.Potential
{
    public sealed class ItemCorePotentialAndBuildCatalogNotice
    {
        internal ItemCorePotentialAndBuildCatalogNotice(
            string dataMaturityKey,
            string coverageStatus,
            string fixtureStatus,
            string approvalStatus,
            string generationDataStatus)
        {
            this.dataMaturityKey = dataMaturityKey ?? string.Empty;
            this.coverageStatus = coverageStatus ?? string.Empty;
            this.fixtureStatus = fixtureStatus ?? string.Empty;
            this.approvalStatus = approvalStatus ?? string.Empty;
            this.generationDataStatus = generationDataStatus ?? string.Empty;
        }

        public string dataMaturityKey { get; }
        public string coverageStatus { get; }
        public string fixtureStatus { get; }
        public string approvalStatus { get; }
        public string generationDataStatus { get; }
    }

    public static class ItemCorePotentialAndBuildEligibilityCatalog
    {
        public const string SchemaOnlyMaturityKey = "SCHEMA_ONLY";
        public const string UnresolvedCoverageStatus = "UNRESOLVED_DATA_COVERAGE";
        public const string QaFixtureStatus = "QA_FIXTURE_ONLY";
        public const string NotDesignApprovedStatus = "NOT_DESIGN_APPROVED";
        public const string NotFormalGenerationDataStatus = "NOT_FORMAL_GENERATION_DATA";
        public const int DefaultCorePotentialProfileCount = 0;
        public const int DefaultQualificationInstanceCount = 0;
        public const int DefaultProbabilityProfileCount = 0;

        private static readonly ReadOnlyCollection<ItemCoreRarityPolicySnapshot> CorePolicies =
            Array.AsReadOnly(new[]
            {
                CorePolicy(ItemInstanceRarity.White, false),
                CorePolicy(ItemInstanceRarity.Green, false),
                CorePolicy(ItemInstanceRarity.Blue, false),
                CorePolicy(ItemInstanceRarity.Purple, false),
                CorePolicy(ItemInstanceRarity.Orange, true)
            });

        private static readonly ReadOnlyCollection<ItemBuildQualificationRarityPolicySnapshot> BuildPolicies =
            Array.AsReadOnly(new[]
            {
                BuildPolicy(ItemInstanceRarity.White, ItemBuildQualificationPolicyMode.LockedNone),
                BuildPolicy(ItemInstanceRarity.Green, ItemBuildQualificationPolicyMode.ProbabilityUnresolved),
                BuildPolicy(ItemInstanceRarity.Blue, ItemBuildQualificationPolicyMode.ProbabilityUnresolved),
                BuildPolicy(ItemInstanceRarity.Purple, ItemBuildQualificationPolicyMode.ProbabilityUnresolved),
                BuildPolicy(ItemInstanceRarity.Orange, ItemBuildQualificationPolicyMode.ProbabilityUnresolved)
            });

        public static IReadOnlyList<ItemCoreRarityPolicySnapshot> DefaultCoreRarityPolicies => CorePolicies;
        public static IReadOnlyList<ItemBuildQualificationRarityPolicySnapshot> DefaultBuildRarityPolicies => BuildPolicies;

        public static ItemCorePotentialAndBuildEligibilitySchemaSnapshot CreateDefaultSchema()
        {
            return ItemCorePotentialAndBuildEligibilitySchema.Create(
                CorePolicies,
                Array.Empty<ItemCorePotentialProfileSnapshot>(),
                BuildPolicies);
        }

        public static ItemCorePotentialAndBuildCatalogNotice GetCatalogNotice()
        {
            return new ItemCorePotentialAndBuildCatalogNotice(
                SchemaOnlyMaturityKey,
                UnresolvedCoverageStatus,
                QaFixtureStatus,
                NotDesignApprovedStatus,
                NotFormalGenerationDataStatus);
        }

        private static ItemCoreRarityPolicySnapshot CorePolicy(
            ItemInstanceRarity rarity,
            bool allowsUltimate)
        {
            return new ItemCoreRarityPolicySnapshot(
                rarity,
                ItemCorePotentialResolutionStatus.Unresolved,
                allowsUltimate,
                SchemaOnlyMaturityKey);
        }

        private static ItemBuildQualificationRarityPolicySnapshot BuildPolicy(
            ItemInstanceRarity rarity,
            ItemBuildQualificationPolicyMode mode)
        {
            return new ItemBuildQualificationRarityPolicySnapshot(
                rarity,
                mode,
                string.Empty,
                SchemaOnlyMaturityKey);
        }
    }
}
