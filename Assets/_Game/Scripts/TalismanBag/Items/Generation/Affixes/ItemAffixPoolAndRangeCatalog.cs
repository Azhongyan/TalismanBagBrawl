using System;

namespace TalismanBag.Items.Generation.Affixes
{
    public sealed class ItemAffixPoolAndRangeCatalogNotice
    {
        internal ItemAffixPoolAndRangeCatalogNotice(string dataMaturityKey, string coverageStatus,
            string fixtureStatus, string balanceStatus, string generationDataStatus)
        {
            this.dataMaturityKey = dataMaturityKey ?? string.Empty;
            this.coverageStatus = coverageStatus ?? string.Empty;
            this.fixtureStatus = fixtureStatus ?? string.Empty;
            this.balanceStatus = balanceStatus ?? string.Empty;
            this.generationDataStatus = generationDataStatus ?? string.Empty;
        }

        public string dataMaturityKey { get; }
        public string coverageStatus { get; }
        public string fixtureStatus { get; }
        public string balanceStatus { get; }
        public string generationDataStatus { get; }
    }

    public static class ItemAffixPoolAndRangeCatalog
    {
        public const string SchemaOnlyMaturityKey = "SCHEMA_ONLY";
        public const string UnresolvedCoverageStatus = "UNRESOLVED_DATA_COVERAGE";
        public const string QaFixtureStatus = "QA_FIXTURE_ONLY";
        public const string NotBalanceApprovedStatus = "NOT_BALANCE_APPROVED";
        public const string NotFormalGenerationDataStatus = "NOT_FORMAL_GENERATION_DATA";

        public const int DefaultAffixDefinitionCount = 0;
        public const int DefaultValueProfileCount = 0;
        public const int DefaultSlotPolicyCount = 0;
        public const int DefaultRandomPoolCount = 0;
        public const int DefaultItemGenerationProfileCount = 0;
        public const int DefaultFixedBindingCount = 0;
        public const int DefaultDefinedWeightCount = 0;
        public const int DefaultMutexGroupCount = 0;

        public static ItemAffixPoolAndRangeSchemaSnapshot CreateDefaultSchema()
        {
            return ItemAffixPoolAndRangeSchema.Create(
                Array.Empty<ItemAffixDefinitionSnapshot>(),
                Array.Empty<ItemAffixValueProfileSnapshot>(),
                Array.Empty<ItemAffixSlotPolicySnapshot>(),
                Array.Empty<ItemRandomAffixPoolSnapshot>(),
                Array.Empty<ItemAffixGenerationProfileSnapshot>());
        }

        public static ItemAffixPoolAndRangeCatalogNotice GetCatalogNotice()
        {
            return new ItemAffixPoolAndRangeCatalogNotice(
                SchemaOnlyMaturityKey,
                UnresolvedCoverageStatus,
                QaFixtureStatus,
                NotBalanceApprovedStatus,
                NotFormalGenerationDataStatus);
        }
    }
}
