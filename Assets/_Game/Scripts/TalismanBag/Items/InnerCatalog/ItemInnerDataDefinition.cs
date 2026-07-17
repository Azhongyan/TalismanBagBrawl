using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.Items.InnerCatalog
{
    public sealed class ItemInnerDataDefinition
    {
        public string itemId;
        public string displayName;
        public ItemFamilyTag itemFamily;
        public ItemFaMenTag faMenTag;
        public ItemQiLeiTag qiLeiTag;
        public string shapeId;
        public List<Vector2Int> defaultLocalCells = new();
        public Vector2Int coreCellLocal;
        public string displayRarityName;
        public ItemCatalogRarity rarityDefault;
        public List<ItemCatalogRarity> allowedRarities = new();
        public string basePowerText;
        public string itemPower;
        public List<ItemInnerStatLine> primaryStats = new();
        public string triggerText;
        public string basicEffectText;
        public string coreEffectPreviewText;
        public string awakeningPreview;
        public string fixedAffixPreview;
        public string randomAffixPreview;
        public string orangeAffixPreview;
        public string placementHint;
        public string flavorText;
        public string iconPlaceholderKey;
        public bool isLightingSource;

        public IReadOnlyList<Vector2Int> ShapeCells => defaultLocalCells;
        public string ItemFamilyKey => itemFamily.ToStableKey();
        public string FaMenKey => faMenTag.ToStableKey();
        public string QiLeiKey => qiLeiTag.ToStableKey();
        public string FaMenDisplayName => faMenTag.ToDisplayName();
        public string QiLeiDisplayName => qiLeiTag.ToDisplayName();

        public bool HasRequiredFields()
        {
            return !string.IsNullOrWhiteSpace(itemId)
                && !string.IsNullOrWhiteSpace(displayName)
                && !string.IsNullOrWhiteSpace(shapeId)
                && defaultLocalCells.Count > 0
                && defaultLocalCells.Contains(coreCellLocal)
                && !string.IsNullOrWhiteSpace(displayRarityName)
                && !string.IsNullOrWhiteSpace(triggerText)
                && !string.IsNullOrWhiteSpace(basicEffectText)
                && !string.IsNullOrWhiteSpace(coreEffectPreviewText)
                && !string.IsNullOrWhiteSpace(awakeningPreview)
                && !string.IsNullOrWhiteSpace(placementHint)
                && !string.IsNullOrWhiteSpace(flavorText)
                && !string.IsNullOrWhiteSpace(iconPlaceholderKey);
        }

        public string FormatCells()
        {
            return string.Join(";", defaultLocalCells.Select(FormatCell));
        }

        public string FormatAllowedRarities()
        {
            return string.Join("|", allowedRarities.Select(rarity => rarity.ToStableKey()));
        }

        public static string FormatCell(Vector2Int cell)
        {
            return $"({cell.x},{cell.y})";
        }
    }

    public sealed class ItemInnerStatLine
    {
        public string label;
        public string value;
        public string hint;

        public ItemInnerStatLine(string label, string value, string hint = "")
        {
            this.label = label;
            this.value = value;
            this.hint = hint;
        }
    }
}
