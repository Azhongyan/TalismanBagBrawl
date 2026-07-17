using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.Items.Generation
{
    public enum ItemInstanceRarity
    {
        White,
        Green,
        Blue,
        Purple,
        Orange
    }

    public sealed class ItemInstanceRarityDefinition
    {
        internal ItemInstanceRarityDefinition(
            ItemInstanceRarity rarity,
            string stableKey,
            string displayName,
            int tierIndex)
        {
            this.rarity = rarity;
            this.stableKey = stableKey ?? string.Empty;
            this.displayName = displayName ?? string.Empty;
            this.tierIndex = tierIndex;
        }

        public ItemInstanceRarity rarity { get; }
        public string stableKey { get; }
        public string displayName { get; }
        public int tierIndex { get; }
    }

    public static class ItemInstanceRarityCatalog
    {
        private static readonly ReadOnlyCollection<ItemInstanceRarityDefinition> Definitions =
            Array.AsReadOnly(new[]
            {
                new ItemInstanceRarityDefinition(ItemInstanceRarity.White, "white", "凡品", 0),
                new ItemInstanceRarityDefinition(ItemInstanceRarity.Green, "green", "良品", 1),
                new ItemInstanceRarityDefinition(ItemInstanceRarity.Blue, "blue", "灵品", 2),
                new ItemInstanceRarityDefinition(ItemInstanceRarity.Purple, "purple", "玄品", 3),
                new ItemInstanceRarityDefinition(ItemInstanceRarity.Orange, "orange", "道品", 4)
            });

        private static readonly IReadOnlyDictionary<string, ItemInstanceRarityDefinition> ByStableKey =
            Definitions.ToDictionary(definition => definition.stableKey, StringComparer.Ordinal);

        private static readonly IReadOnlyDictionary<ItemInstanceRarity, ItemInstanceRarityDefinition> ByRarity =
            Definitions.ToDictionary(definition => definition.rarity);

        public static IReadOnlyList<ItemInstanceRarityDefinition> All => Definitions;

        public static bool TryParseStableKey(string stableKey, out ItemInstanceRarity rarity)
        {
            if (!string.IsNullOrWhiteSpace(stableKey)
                && ByStableKey.TryGetValue(stableKey.Trim(), out ItemInstanceRarityDefinition definition))
            {
                rarity = definition.rarity;
                return true;
            }

            rarity = default;
            return false;
        }

        public static bool TryGetDefinition(
            ItemInstanceRarity rarity,
            out ItemInstanceRarityDefinition definition)
        {
            return ByRarity.TryGetValue(rarity, out definition);
        }

        public static string ToStableKey(this ItemInstanceRarity rarity)
        {
            return TryGetDefinition(rarity, out ItemInstanceRarityDefinition definition)
                ? definition.stableKey
                : string.Empty;
        }

        public static string ToDisplayName(this ItemInstanceRarity rarity)
        {
            return TryGetDefinition(rarity, out ItemInstanceRarityDefinition definition)
                ? definition.displayName
                : string.Empty;
        }

        public static int ToTierIndex(this ItemInstanceRarity rarity)
        {
            return TryGetDefinition(rarity, out ItemInstanceRarityDefinition definition)
                ? definition.tierIndex
                : -1;
        }
    }
}
