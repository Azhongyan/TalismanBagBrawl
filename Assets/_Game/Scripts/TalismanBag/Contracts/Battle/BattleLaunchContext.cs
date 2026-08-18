using System;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleLaunchContext
    {
        public const string SchemaId = "BattleLaunchContext.v1";
        public const int CurrentSchemaVersion = 1;

        public BattleLaunchContext(
            string schemaId,
            int schemaVersion,
            string launchId,
            string token,
            long generation,
            string productContext,
            string chapterId,
            string stageId,
            string balanceProfileId,
            string encounterVariantId,
            string stageThemeProfileId,
            string enemyPresentationProfileId,
            string sourceRoute,
            string returnRoute)
        {
            Schema = Normalize(schemaId);
            SchemaVersion = schemaVersion;
            LaunchId = Normalize(launchId);
            Token = Normalize(token);
            Generation = generation;
            ProductContext = Normalize(productContext);
            ChapterId = Normalize(chapterId);
            StageId = Normalize(stageId);
            BalanceProfileId = Normalize(balanceProfileId);
            EncounterVariantId = Normalize(encounterVariantId);
            StageThemeProfileId = Normalize(stageThemeProfileId);
            EnemyPresentationProfileId = Normalize(enemyPresentationProfileId);
            SourceRoute = Normalize(sourceRoute);
            ReturnRoute = Normalize(returnRoute);
        }

        public string Schema { get; }
        public int SchemaVersion { get; }
        public string LaunchId { get; }
        public string Token { get; }
        public long Generation { get; }
        public string ProductContext { get; }
        public string ChapterId { get; }
        public string StageId { get; }
        public string BalanceProfileId { get; }
        public string EncounterVariantId { get; }
        public string StageThemeProfileId { get; }
        public string EnemyPresentationProfileId { get; }
        public string SourceRoute { get; }
        public string ReturnRoute { get; }

        public bool HasSameEnvelope(BattleLaunchContext other)
        {
            return other != null
                && SchemaVersion == other.SchemaVersion
                && Generation == other.Generation
                && EqualsOrdinal(Schema, other.Schema)
                && EqualsOrdinal(LaunchId, other.LaunchId)
                && EqualsOrdinal(Token, other.Token)
                && EqualsOrdinal(ProductContext, other.ProductContext)
                && EqualsOrdinal(ChapterId, other.ChapterId)
                && EqualsOrdinal(StageId, other.StageId)
                && EqualsOrdinal(BalanceProfileId, other.BalanceProfileId)
                && EqualsOrdinal(EncounterVariantId, other.EncounterVariantId)
                && EqualsOrdinal(StageThemeProfileId, other.StageThemeProfileId)
                && EqualsOrdinal(
                    EnemyPresentationProfileId,
                    other.EnemyPresentationProfileId)
                && EqualsOrdinal(SourceRoute, other.SourceRoute)
                && EqualsOrdinal(ReturnRoute, other.ReturnRoute);
        }

        private static bool EqualsOrdinal(string left, string right)
        {
            return string.Equals(left, right, StringComparison.Ordinal);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
