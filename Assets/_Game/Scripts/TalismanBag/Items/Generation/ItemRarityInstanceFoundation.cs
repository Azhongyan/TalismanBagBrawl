using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.InnerCatalog;

namespace TalismanBag.Items.Generation
{
    internal static class ItemGenerationReadOnly
    {
        public static ReadOnlyCollection<T> Freeze<T>(IEnumerable<T> values)
        {
            return Array.AsReadOnly((values ?? Array.Empty<T>()).ToArray());
        }
    }

    public static class ItemGenerationValidationCodes
    {
        public const string None = "NONE";
        public const string FoundationMissing = "FOUNDATION_MISSING";
        public const string CatalogItemMissing = "CATALOG_ITEM_MISSING";
        public const string CatalogItemDuplicate = "CATALOG_ITEM_DUPLICATE";
        public const string ItemInstanceIdEmpty = "ITEM_INSTANCE_ID_EMPTY";
        public const string BaseItemIdEmpty = "BASE_ITEM_ID_EMPTY";
        public const string BaseItemUnknown = "BASE_ITEM_UNKNOWN";
        public const string BaseItemNotOrdinary = "BASE_ITEM_NOT_ORDINARY";
        public const string RarityKeyInvalid = "RARITY_KEY_INVALID";
        public const string GenerationVersionInvalid = "GENERATION_VERSION_INVALID";
        public const string InstanceIdEqualsBaseItemId = "INSTANCE_ID_EQUALS_BASE_ITEM_ID";
    }

    public sealed class ItemGenerationValidationError
    {
        public ItemGenerationValidationError(string code, string message)
        {
            this.code = code ?? string.Empty;
            this.message = message ?? string.Empty;
        }

        public string code { get; }
        public string message { get; }
    }

    public sealed class ItemArchetypeIdentitySnapshot
    {
        internal ItemArchetypeIdentitySnapshot(
            string baseItemId,
            string displayName,
            string faMenKey,
            string qiLeiKey,
            string shapeId,
            bool isOrdinaryGeneratedItem,
            bool isCoreProgressionItem)
        {
            this.baseItemId = baseItemId ?? string.Empty;
            this.displayName = displayName ?? string.Empty;
            this.faMenKey = faMenKey ?? string.Empty;
            this.qiLeiKey = qiLeiKey ?? string.Empty;
            this.shapeId = shapeId ?? string.Empty;
            this.isOrdinaryGeneratedItem = isOrdinaryGeneratedItem;
            this.isCoreProgressionItem = isCoreProgressionItem;
        }

        public string baseItemId { get; }
        public string displayName { get; }
        public string faMenKey { get; }
        public string qiLeiKey { get; }
        public string shapeId { get; }
        public bool isOrdinaryGeneratedItem { get; }
        public bool isCoreProgressionItem { get; }
    }

    public readonly struct ItemRarityVersionKey : IEquatable<ItemRarityVersionKey>, IComparable<ItemRarityVersionKey>
    {
        public ItemRarityVersionKey(string baseItemId, ItemInstanceRarity rarity)
        {
            this.baseItemId = baseItemId ?? string.Empty;
            this.rarity = rarity;
        }

        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public string canonicalKey => $"{baseItemId}@{rarity.ToStableKey()}";

        public bool Equals(ItemRarityVersionKey other)
        {
            return string.Equals(baseItemId, other.baseItemId, StringComparison.Ordinal)
                && rarity == other.rarity;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemRarityVersionKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((baseItemId != null ? StringComparer.Ordinal.GetHashCode(baseItemId) : 0) * 397)
                    ^ (int)rarity;
            }
        }

        public int CompareTo(ItemRarityVersionKey other)
        {
            int byBaseItem = string.Compare(baseItemId, other.baseItemId, StringComparison.Ordinal);
            return byBaseItem != 0 ? byBaseItem : rarity.ToTierIndex().CompareTo(other.rarity.ToTierIndex());
        }

        public override string ToString()
        {
            return canonicalKey;
        }
    }

    public interface IItemCultivationPotentialRef
    {
        string cultivationPotentialProfileId { get; }
    }

    public sealed class ItemInstanceIdentitySnapshot : IItemCultivationPotentialRef
    {
        public const string CurrentSchemaId = "ItemInstanceIdentitySnapshot.v1";

        internal ItemInstanceIdentitySnapshot(
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            int generationVersion,
            long rootSeed,
            string cultivationPotentialProfileId)
        {
            schemaId = CurrentSchemaId;
            this.itemInstanceId = itemInstanceId ?? string.Empty;
            this.baseItemId = baseItemId ?? string.Empty;
            this.rarity = rarity;
            this.generationVersion = generationVersion;
            this.rootSeed = rootSeed;
            this.cultivationPotentialProfileId = cultivationPotentialProfileId ?? string.Empty;
        }

        public string schemaId { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public int generationVersion { get; }
        public long rootSeed { get; }
        public string cultivationPotentialProfileId { get; }

        public string BuildCanonicalSignature()
        {
            return string.Join(
                "|",
                schemaId,
                itemInstanceId,
                baseItemId,
                rarity.ToStableKey(),
                generationVersion.ToString(CultureInfo.InvariantCulture),
                rootSeed.ToString(CultureInfo.InvariantCulture),
                cultivationPotentialProfileId);
        }
    }

    public sealed class ItemInstanceIdentityCreationResult
    {
        private readonly ReadOnlyCollection<ItemGenerationValidationError> validationErrors;

        internal ItemInstanceIdentityCreationResult(
            ItemInstanceIdentitySnapshot snapshot,
            IEnumerable<ItemGenerationValidationError> validationErrors)
        {
            this.snapshot = snapshot;
            this.validationErrors = ItemGenerationReadOnly.Freeze(validationErrors);
        }

        public bool isValid => snapshot != null && validationErrors.Count == 0;
        public ItemInstanceIdentitySnapshot snapshot { get; }
        public IReadOnlyList<ItemGenerationValidationError> ValidationErrors => validationErrors;
        public string primaryValidationCode => validationErrors.Count == 0
            ? ItemGenerationValidationCodes.None
            : validationErrors[0].code;
    }

    public sealed class ItemGenerationFoundationSnapshot
    {
        public const string CurrentSchemaId = "ItemRarityInstanceFoundation.v1";

        private readonly ReadOnlyCollection<ItemArchetypeIdentitySnapshot> archetypes;
        private readonly ReadOnlyCollection<ItemArchetypeIdentitySnapshot> ordinaryArchetypes;
        private readonly ReadOnlyCollection<ItemArchetypeIdentitySnapshot> systemArchetypes;
        private readonly ReadOnlyCollection<ItemInstanceRarityDefinition> rarityDefinitions;
        private readonly ReadOnlyCollection<ItemRarityVersionKey> rarityVersionKeys;
        private readonly ReadOnlyCollection<ItemGenerationValidationError> validationErrors;

        internal ItemGenerationFoundationSnapshot(
            IEnumerable<ItemArchetypeIdentitySnapshot> archetypes,
            IEnumerable<ItemInstanceRarityDefinition> rarityDefinitions,
            IEnumerable<ItemRarityVersionKey> rarityVersionKeys,
            IEnumerable<ItemGenerationValidationError> validationErrors)
        {
            schemaId = CurrentSchemaId;
            this.archetypes = ItemGenerationReadOnly.Freeze((archetypes ?? Array.Empty<ItemArchetypeIdentitySnapshot>())
                .OrderBy(item => item.baseItemId, StringComparer.Ordinal));
            ordinaryArchetypes = ItemGenerationReadOnly.Freeze(this.archetypes.Where(item => item.isOrdinaryGeneratedItem));
            systemArchetypes = ItemGenerationReadOnly.Freeze(this.archetypes.Where(item => item.isCoreProgressionItem));
            this.rarityDefinitions = ItemGenerationReadOnly.Freeze((rarityDefinitions ?? Array.Empty<ItemInstanceRarityDefinition>())
                .OrderBy(definition => definition.tierIndex));
            this.rarityVersionKeys = ItemGenerationReadOnly.Freeze((rarityVersionKeys ?? Array.Empty<ItemRarityVersionKey>())
                .OrderBy(key => key));
            this.validationErrors = ItemGenerationReadOnly.Freeze(validationErrors);
        }

        public string schemaId { get; }
        public bool isValid => validationErrors.Count == 0;
        public IReadOnlyList<ItemArchetypeIdentitySnapshot> Archetypes => archetypes;
        public IReadOnlyList<ItemArchetypeIdentitySnapshot> OrdinaryArchetypes => ordinaryArchetypes;
        public IReadOnlyList<ItemArchetypeIdentitySnapshot> SystemArchetypes => systemArchetypes;
        public IReadOnlyList<ItemInstanceRarityDefinition> RarityDefinitions => rarityDefinitions;
        public IReadOnlyList<ItemRarityVersionKey> RarityVersionKeys => rarityVersionKeys;
        public IReadOnlyList<ItemGenerationValidationError> ValidationErrors => validationErrors;

        public ItemArchetypeIdentitySnapshot FindArchetype(string baseItemId)
        {
            return archetypes.FirstOrDefault(item => string.Equals(item.baseItemId, baseItemId, StringComparison.Ordinal));
        }

        public string BuildCanonicalSignature()
        {
            StringBuilder builder = new();
            builder.Append(schemaId);
            foreach (ItemArchetypeIdentitySnapshot item in archetypes)
            {
                builder.Append("\nA|")
                    .Append(item.baseItemId).Append('|')
                    .Append(item.displayName).Append('|')
                    .Append(item.faMenKey).Append('|')
                    .Append(item.qiLeiKey).Append('|')
                    .Append(item.shapeId).Append('|')
                    .Append(item.isOrdinaryGeneratedItem ? '1' : '0').Append('|')
                    .Append(item.isCoreProgressionItem ? '1' : '0');
            }

            foreach (ItemInstanceRarityDefinition definition in rarityDefinitions)
            {
                builder.Append("\nR|")
                    .Append(definition.stableKey).Append('|')
                    .Append(definition.displayName).Append('|')
                    .Append(definition.tierIndex.ToString(CultureInfo.InvariantCulture));
            }

            foreach (ItemRarityVersionKey key in rarityVersionKeys)
            {
                builder.Append("\nV|").Append(key.canonicalKey);
            }

            foreach (ItemGenerationValidationError error in validationErrors)
            {
                builder.Append("\nE|").Append(error.code).Append('|').Append(error.message);
            }

            return builder.ToString();
        }
    }

    public static class ItemRarityInstanceFoundation
    {
        public const string CoreProgressionBaseItemId = "I031";

        public static ItemGenerationFoundationSnapshot Create(
            IReadOnlyList<ItemInnerDataDefinition> catalogItems = null)
        {
            IReadOnlyList<ItemInnerDataDefinition> source = catalogItems ?? ItemInnerDataCatalog.AllItems;
            List<ItemGenerationValidationError> errors = new();
            List<ItemInnerDataDefinition> ordered = (source ?? Array.Empty<ItemInnerDataDefinition>())
                .Where(item => item != null && IsKnownIdentity(item.itemId))
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ThenBy(item => item.displayName, StringComparer.Ordinal)
                .ThenBy(item => item.FaMenKey, StringComparer.Ordinal)
                .ThenBy(item => item.QiLeiKey, StringComparer.Ordinal)
                .ThenBy(item => item.shapeId, StringComparer.Ordinal)
                .ToList();

            foreach (IGrouping<string, ItemInnerDataDefinition> group in ordered.GroupBy(item => item.itemId, StringComparer.Ordinal))
            {
                if (group.Count() > 1)
                {
                    errors.Add(new ItemGenerationValidationError(
                        ItemGenerationValidationCodes.CatalogItemDuplicate,
                        $"Catalog identity '{group.Key}' appears {group.Count()} times; the stable first projection was used."));
                }
            }

            List<ItemInnerDataDefinition> stableItems = ordered
                .GroupBy(item => item.itemId, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToList();
            foreach (string expectedId in ExpectedBaseItemIds())
            {
                if (!stableItems.Any(item => string.Equals(item.itemId, expectedId, StringComparison.Ordinal)))
                {
                    errors.Add(new ItemGenerationValidationError(
                        ItemGenerationValidationCodes.CatalogItemMissing,
                        $"Required catalog identity '{expectedId}' is missing."));
                }
            }

            List<ItemArchetypeIdentitySnapshot> archetypes = stableItems
                .Select(ProjectIdentity)
                .ToList();
            List<ItemRarityVersionKey> versionKeys = archetypes
                .Where(item => item.isOrdinaryGeneratedItem)
                .SelectMany(item => ItemInstanceRarityCatalog.All.Select(definition =>
                    new ItemRarityVersionKey(item.baseItemId, definition.rarity)))
                .ToList();

            return new ItemGenerationFoundationSnapshot(
                archetypes,
                ItemInstanceRarityCatalog.All,
                versionKeys,
                errors);
        }

        public static ItemInstanceIdentityCreationResult TryCreateOrdinaryInstance(
            ItemGenerationFoundationSnapshot foundation,
            string itemInstanceId,
            string baseItemId,
            string rarityStableKey,
            int generationVersion,
            long rootSeed,
            string cultivationPotentialProfileId = "")
        {
            if (!ItemInstanceRarityCatalog.TryParseStableKey(rarityStableKey, out ItemInstanceRarity rarity))
            {
                return Invalid(
                    ItemGenerationValidationCodes.RarityKeyInvalid,
                    $"Rarity stable key '{rarityStableKey ?? string.Empty}' is not one of white/green/blue/purple/orange.");
            }

            return TryCreateOrdinaryInstance(
                foundation,
                itemInstanceId,
                baseItemId,
                rarity,
                generationVersion,
                rootSeed,
                cultivationPotentialProfileId);
        }

        public static ItemInstanceIdentityCreationResult TryCreateOrdinaryInstance(
            ItemGenerationFoundationSnapshot foundation,
            string itemInstanceId,
            string baseItemId,
            ItemInstanceRarity rarity,
            int generationVersion,
            long rootSeed,
            string cultivationPotentialProfileId = "")
        {
            List<ItemGenerationValidationError> errors = new();
            string normalizedInstanceId = Normalize(itemInstanceId);
            string normalizedBaseItemId = Normalize(baseItemId);

            if (foundation == null)
            {
                errors.Add(new ItemGenerationValidationError(
                    ItemGenerationValidationCodes.FoundationMissing,
                    "Item generation foundation is missing."));
            }

            if (string.IsNullOrWhiteSpace(normalizedInstanceId))
            {
                errors.Add(new ItemGenerationValidationError(
                    ItemGenerationValidationCodes.ItemInstanceIdEmpty,
                    "itemInstanceId must be supplied by the caller."));
            }

            if (string.IsNullOrWhiteSpace(normalizedBaseItemId))
            {
                errors.Add(new ItemGenerationValidationError(
                    ItemGenerationValidationCodes.BaseItemIdEmpty,
                    "baseItemId is required."));
            }

            if (!ItemInstanceRarityCatalog.TryGetDefinition(rarity, out _))
            {
                errors.Add(new ItemGenerationValidationError(
                    ItemGenerationValidationCodes.RarityKeyInvalid,
                    $"Rarity enum value '{rarity}' is not supported."));
            }

            if (generationVersion <= 0)
            {
                errors.Add(new ItemGenerationValidationError(
                    ItemGenerationValidationCodes.GenerationVersionInvalid,
                    "generationVersion must be greater than zero."));
            }

            if (!string.IsNullOrWhiteSpace(normalizedInstanceId)
                && string.Equals(normalizedInstanceId, normalizedBaseItemId, StringComparison.Ordinal))
            {
                errors.Add(new ItemGenerationValidationError(
                    ItemGenerationValidationCodes.InstanceIdEqualsBaseItemId,
                    "itemInstanceId and baseItemId are separate identity layers and must not be equal."));
            }

            if (foundation != null && !string.IsNullOrWhiteSpace(normalizedBaseItemId))
            {
                ItemArchetypeIdentitySnapshot archetype = foundation.FindArchetype(normalizedBaseItemId);
                if (archetype == null)
                {
                    errors.Add(new ItemGenerationValidationError(
                        ItemGenerationValidationCodes.BaseItemUnknown,
                        $"Unknown baseItemId '{normalizedBaseItemId}'."));
                }
                else if (!archetype.isOrdinaryGeneratedItem || archetype.isCoreProgressionItem)
                {
                    errors.Add(new ItemGenerationValidationError(
                        ItemGenerationValidationCodes.BaseItemNotOrdinary,
                        $"baseItemId '{normalizedBaseItemId}' is a directed core-progression item and is excluded from ordinary generation."));
                }
            }

            if (errors.Count > 0)
            {
                return new ItemInstanceIdentityCreationResult(null, errors);
            }

            return new ItemInstanceIdentityCreationResult(
                new ItemInstanceIdentitySnapshot(
                    normalizedInstanceId,
                    normalizedBaseItemId,
                    rarity,
                    generationVersion,
                    rootSeed,
                    Normalize(cultivationPotentialProfileId)),
                Array.Empty<ItemGenerationValidationError>());
        }

        public static bool IsOrdinaryBaseItemId(string baseItemId)
        {
            if (string.IsNullOrWhiteSpace(baseItemId)
                || baseItemId.Length != 4
                || baseItemId[0] != 'I')
            {
                return false;
            }

            return int.TryParse(
                    baseItemId.Substring(1),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out int number)
                && number >= 1
                && number <= 30;
        }

        private static ItemArchetypeIdentitySnapshot ProjectIdentity(ItemInnerDataDefinition item)
        {
            bool ordinary = IsOrdinaryBaseItemId(item.itemId);
            bool coreProgression = string.Equals(item.itemId, CoreProgressionBaseItemId, StringComparison.Ordinal);
            return new ItemArchetypeIdentitySnapshot(
                item.itemId,
                item.displayName,
                item.FaMenKey,
                item.QiLeiKey,
                item.shapeId,
                ordinary,
                coreProgression);
        }

        private static ItemInstanceIdentityCreationResult Invalid(string code, string message)
        {
            return new ItemInstanceIdentityCreationResult(
                null,
                new[] { new ItemGenerationValidationError(code, message) });
        }

        private static bool IsKnownIdentity(string itemId)
        {
            return IsOrdinaryBaseItemId(itemId)
                || string.Equals(itemId, CoreProgressionBaseItemId, StringComparison.Ordinal);
        }

        private static IEnumerable<string> ExpectedBaseItemIds()
        {
            for (int index = 1; index <= 31; index++)
            {
                yield return $"I{index:000}";
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
