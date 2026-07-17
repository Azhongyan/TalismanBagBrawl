using System;

namespace TalismanBag.Items.Generation.Stats
{
    public static class ItemStatRangeSchemaCatalog
    {
        public const string QaSeedUsageStatus = "QA_SEED_ONLY";
        public const string QaSeedBalanceStatus = "NOT_BALANCE_APPROVED";
        public const string QaSeedGenerationStatus = "NOT_FORMAL_GENERATION_DATA";
        public const string QaDamageStatId = "qa_damage_example";

        public static ItemStatRangeSchemaSnapshot CreateQaSeedSchema()
        {
            ItemStatDefinitionSnapshot definition = new(
                QaDamageStatId,
                "QA Damage Example",
                "flat",
                ItemStatDirection.HigherIsBetter,
                decimalPlaces: 0,
                stepUnits: 1,
                ItemStatRoundingMode.Nearest,
                ItemStatDataMaturity.SEED_DATA);
            ItemStatRangeProfileSnapshot profile = new(
                "I001",
                QaDamageStatId,
                ItemStatDataMaturity.SEED_DATA,
                new ItemNumericRangeSnapshot(1, 10),
                new[]
                {
                    new ItemRarityStatRangeSnapshot(ItemInstanceRarity.White, 1, 2),
                    new ItemRarityStatRangeSnapshot(ItemInstanceRarity.Green, 3, 5),
                    new ItemRarityStatRangeSnapshot(ItemInstanceRarity.Blue, 5, 8),
                    new ItemRarityStatRangeSnapshot(ItemInstanceRarity.Purple, 6, 9),
                    new ItemRarityStatRangeSnapshot(ItemInstanceRarity.Orange, 9, 10)
                });

            return ItemStatRangeSchema.Create(new[] { definition }, new[] { profile });
        }

        public static IReadOnlySeedNotice GetQaSeedNotice()
        {
            return new SeedNotice(QaSeedUsageStatus, QaSeedBalanceStatus, QaSeedGenerationStatus);
        }

        public interface IReadOnlySeedNotice
        {
            string usageStatus { get; }
            string balanceStatus { get; }
            string generationStatus { get; }
        }

        private sealed class SeedNotice : IReadOnlySeedNotice
        {
            public SeedNotice(string usageStatus, string balanceStatus, string generationStatus)
            {
                this.usageStatus = usageStatus ?? string.Empty;
                this.balanceStatus = balanceStatus ?? string.Empty;
                this.generationStatus = generationStatus ?? string.Empty;
            }

            public string usageStatus { get; }
            public string balanceStatus { get; }
            public string generationStatus { get; }
        }
    }
}
