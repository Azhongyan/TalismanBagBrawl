using UnityEngine;

namespace TalismanBag.Items.Detail.UI
{
    [CreateAssetMenu(
        fileName = "ItemDetailVisualTheme",
        menuName = "TalismanBag/Item Detail/Visual Theme")]
    public sealed class ItemDetailVisualTheme : ScriptableObject
    {
        [Header("Surfaces")]
        public Color pageBackground = new Color32(0x17, 0x16, 0x13, 0xff);
        public Color cardBackground = new Color32(0x24, 0x20, 0x19, 0xff);
        public Color secondaryCardBackground = new Color32(0x30, 0x29, 0x1f, 0xff);

        [Header("Text")]
        public Color titleText = new Color32(0xb9, 0x9a, 0x61, 0xff);
        public Color bodyText = new Color32(0xd8, 0xcc, 0xb7, 0xff);
        public Color secondaryText = new Color32(0x99, 0x8f, 0x7c, 0xff);
        public Color weakText = new Color32(0x81, 0x79, 0x6d, 0xff);
        public Color restrictionText = new Color32(0xc9, 0x4e, 0x3b, 0xff);

        [Header("Build states")]
        public Color buildActive = new Color32(0x82, 0xae, 0x4f, 0xff);
        public Color buildNearActive = new Color32(0xc0, 0xa4, 0x5e, 0xff);
        public Color buildInactive = new Color32(0x71, 0x6f, 0x69, 0xff);

        [Header("Array modifier")]
        public Color arrayModifierColor = new Color32(0x55, 0xc6, 0xb3, 0xff);
        public Sprite arrayModifierIcon;

        [Header("Rarity")]
        public Color rarityWhite = new Color32(0xe3, 0xd8, 0xc3, 0xff);
        public Color rarityGreen = new Color32(0x87, 0xb6, 0x6a, 0xff);
        public Color rarityBlue = new Color32(0x68, 0xa9, 0xe6, 0xff);
        public Color rarityPurple = new Color32(0xb2, 0x7d, 0xdf, 0xff);
        public Color rarityOrange = new Color32(0xe4, 0xa1, 0x4b, 0xff);
    }

    public static class ItemDetailVisualThemeDefaults
    {
        public static readonly Color PageBackground = new Color32(0x17, 0x16, 0x13, 0xff);
        public static readonly Color CardBackground = new Color32(0x24, 0x20, 0x19, 0xff);
        public static readonly Color SecondaryCardBackground = new Color32(0x30, 0x29, 0x1f, 0xff);
        public static readonly Color TitleText = new Color32(0xb9, 0x9a, 0x61, 0xff);
        public static readonly Color BodyText = new Color32(0xd8, 0xcc, 0xb7, 0xff);
        public static readonly Color SecondaryText = new Color32(0x99, 0x8f, 0x7c, 0xff);
        public static readonly Color WeakText = new Color32(0x81, 0x79, 0x6d, 0xff);
        public static readonly Color RestrictionText = new Color32(0xc9, 0x4e, 0x3b, 0xff);
        public static readonly Color BuildActive = new Color32(0x82, 0xae, 0x4f, 0xff);
        public static readonly Color BuildNearActive = new Color32(0xc0, 0xa4, 0x5e, 0xff);
        public static readonly Color BuildInactive = new Color32(0x71, 0x6f, 0x69, 0xff);
        public static readonly Color ArrayModifierColor = new Color32(0x55, 0xc6, 0xb3, 0xff);
        public static readonly Color RarityWhite = new Color32(0xe3, 0xd8, 0xc3, 0xff);
        public static readonly Color RarityGreen = new Color32(0x87, 0xb6, 0x6a, 0xff);
        public static readonly Color RarityBlue = new Color32(0x68, 0xa9, 0xe6, 0xff);
        public static readonly Color RarityPurple = new Color32(0xb2, 0x7d, 0xdf, 0xff);
        public static readonly Color RarityOrange = new Color32(0xe4, 0xa1, 0x4b, 0xff);
    }
}
