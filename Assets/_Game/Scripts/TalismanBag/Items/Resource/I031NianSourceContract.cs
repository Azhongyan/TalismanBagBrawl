using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TalismanBag.Items.Resource
{
    public enum I031NianSourceStatus
    {
        Invalid = 0,
        Valid = 1
    }

    public sealed class I031NianCostRequestFact
    {
        public I031NianCostRequestFact(string itemId, int nianCost)
        {
            this.itemId = itemId ?? string.Empty;
            this.nianCost = nianCost;
            rarityKey = "Orange";
            itemLevel = 40;
            resourceKey = I031NianSourceSnapshot.ResourceKey;
            sourceKey = "ItemLv40OrangeProjection.nianCost";
            requestFactId = "I031_NIAN_COST|" + this.itemId + "|L40|Orange|" +
                            nianCost.ToString(CultureInfo.InvariantCulture);
        }

        public string requestFactId { get; }
        public string itemId { get; }
        public string rarityKey { get; }
        public int itemLevel { get; }
        public string resourceKey { get; }
        public int nianCost { get; }
        public string sourceKey { get; }
        public bool devOnly => true;
    }

    public sealed class I031NianSourceSnapshot
    {
        public const string SchemaVersion = "I031NianSourceSnapshot.v1";
        public const string ResourceKey = "nian";
        public const int GenerationPerPulse = 4;

        private readonly ReadOnlyCollection<I031NianCostRequestFact> costRequestFacts;
        private readonly ReadOnlyCollection<string> validationErrors;

        public I031NianSourceSnapshot(
            int sourceGeneration,
            string itemSystemSchemaVersion,
            string itemSystemSignature,
            bool isOwned,
            I031Location location,
            bool isLightingSource,
            bool isEligible,
            IReadOnlyList<I031NianCostRequestFact> costRequestFacts,
            IReadOnlyList<string> validationErrors)
        {
            schemaVersion = SchemaVersion;
            resourceKey = ResourceKey;
            this.sourceGeneration = sourceGeneration;
            this.itemSystemSchemaVersion = itemSystemSchemaVersion ?? string.Empty;
            this.itemSystemSignature = itemSystemSignature ?? string.Empty;
            itemId = I031InventoryPlacementContract.ItemId;
            specialIdentityId = I031InventoryPlacementContract.SpecialIdentityId;
            stablePlacementId = I031InventoryPlacementContract.StablePlacementId;
            this.isOwned = isOwned;
            this.location = location;
            this.isLightingSource = isLightingSource;
            this.isEligible = isEligible;
            generationPerPulse = GenerationPerPulse;
            this.costRequestFacts = new ReadOnlyCollection<I031NianCostRequestFact>(
                (costRequestFacts ?? Array.Empty<I031NianCostRequestFact>())
                .Where(value => value != null)
                .Select(value => new I031NianCostRequestFact(value.itemId, value.nianCost))
                .ToList());
            this.validationErrors = new ReadOnlyCollection<string>(
                (validationErrors ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList());
            status = isEligible && this.validationErrors.Count == 0
                ? I031NianSourceStatus.Valid
                : I031NianSourceStatus.Invalid;
            canonicalSignature = NianCanonical.Sha256(BuildCanonicalText());
        }

        public string schemaVersion { get; }
        public bool devOnly => true;
        public bool formalRuntimeAuthority => false;
        public I031NianSourceStatus status { get; }
        public int sourceGeneration { get; }
        public string resourceKey { get; }
        public string itemSystemSchemaVersion { get; }
        public string itemSystemSignature { get; }
        public string itemId { get; }
        public string specialIdentityId { get; }
        public string stablePlacementId { get; }
        public bool isOwned { get; }
        public I031Location location { get; }
        public bool isLightingSource { get; }
        public bool isEligible { get; }
        public int generationPerPulse { get; }
        public IReadOnlyList<I031NianCostRequestFact> CostRequestFacts => costRequestFacts;
        public IReadOnlyList<string> ValidationErrors => validationErrors;
        public string canonicalSignature { get; }

        public I031NianCostRequestFact FindCostFact(string itemIdValue)
        {
            return costRequestFacts.FirstOrDefault(value => string.Equals(
                value.itemId, itemIdValue, StringComparison.Ordinal));
        }

        private string BuildCanonicalText()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(schemaVersion).Append('|')
                .Append(sourceGeneration.ToString(CultureInfo.InvariantCulture)).Append('|')
                .Append(resourceKey).Append('|')
                .Append(itemSystemSchemaVersion).Append('|')
                .Append(itemSystemSignature).Append('|')
                .Append(itemId).Append('|')
                .Append(specialIdentityId).Append('|')
                .Append(stablePlacementId).Append('|')
                .Append(NianCanonical.Bool(isOwned)).Append('|')
                .Append((int)location).Append('|')
                .Append(NianCanonical.Bool(isLightingSource)).Append('|')
                .Append(NianCanonical.Bool(isEligible)).Append('|')
                .Append(generationPerPulse.ToString(CultureInfo.InvariantCulture));
            foreach (I031NianCostRequestFact fact in costRequestFacts)
            {
                builder.Append("|C:")
                    .Append(fact.requestFactId).Append(':')
                    .Append(fact.itemId).Append(':')
                    .Append(fact.nianCost.ToString(CultureInfo.InvariantCulture));
            }

            foreach (string error in validationErrors)
            {
                builder.Append("|E:").Append(error);
            }

            return builder.ToString();
        }
    }

    internal static class NianCanonical
    {
        public static string Bool(bool value)
        {
            return value ? "1" : "0";
        }

        public static string Sha256(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty));
                StringBuilder builder = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }
}
