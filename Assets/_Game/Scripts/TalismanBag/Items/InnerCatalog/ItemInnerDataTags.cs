namespace TalismanBag.Items.InnerCatalog
{
    public enum ItemFamilyTag
    {
        FaMenCombat,
        ZhonggongSource
    }

    public enum ItemFaMenTag
    {
        Zhenlei,
        Lihuo,
        Zhongyue,
        Xuanshui,
        Taibai,
        Zhonggong
    }

    public enum ItemQiLeiTag
    {
        Fu,
        Yin,
        Ling,
        Jing,
        Fa,
        ZhonggongQi
    }

    public enum ItemCatalogRarity
    {
        Bai,
        Qing,
        Lan,
        Zi,
        Cheng
    }

    public static class ItemInnerDataTagNames
    {
        public static string ToStableKey(this ItemFamilyTag tag)
        {
            return tag switch
            {
                ItemFamilyTag.FaMenCombat => "famenCombat",
                ItemFamilyTag.ZhonggongSource => "zhonggongSource",
                _ => "unknown"
            };
        }

        public static string ToStableKey(this ItemFaMenTag tag)
        {
            return tag switch
            {
                ItemFaMenTag.Zhenlei => "zhenlei",
                ItemFaMenTag.Lihuo => "lihuo",
                ItemFaMenTag.Zhongyue => "zhongyue",
                ItemFaMenTag.Xuanshui => "xuanshui",
                ItemFaMenTag.Taibai => "taibai",
                ItemFaMenTag.Zhonggong => "zhonggong",
                _ => "unknown"
            };
        }

        public static string ToDisplayName(this ItemFaMenTag tag)
        {
            return tag switch
            {
                ItemFaMenTag.Zhenlei => "震雷法",
                ItemFaMenTag.Lihuo => "离火法",
                ItemFaMenTag.Zhongyue => "中岳法",
                ItemFaMenTag.Xuanshui => "玄水法",
                ItemFaMenTag.Taibai => "太白法",
                ItemFaMenTag.Zhonggong => "中宫",
                _ => "未知法门"
            };
        }

        public static string ToStableKey(this ItemQiLeiTag tag)
        {
            return tag switch
            {
                ItemQiLeiTag.Fu => "fu",
                ItemQiLeiTag.Yin => "yin",
                ItemQiLeiTag.Ling => "ling",
                ItemQiLeiTag.Jing => "jing",
                ItemQiLeiTag.Fa => "fa",
                ItemQiLeiTag.ZhonggongQi => "zhonggongQi",
                _ => "unknown"
            };
        }

        public static string ToDisplayName(this ItemQiLeiTag tag)
        {
            return tag switch
            {
                ItemQiLeiTag.Fu => "符",
                ItemQiLeiTag.Yin => "印",
                ItemQiLeiTag.Ling => "令",
                ItemQiLeiTag.Jing => "镜",
                ItemQiLeiTag.Fa => "法",
                ItemQiLeiTag.ZhonggongQi => "中宫符器",
                _ => "未知器类"
            };
        }

        public static string ToStableKey(this ItemCatalogRarity rarity)
        {
            return rarity switch
            {
                ItemCatalogRarity.Bai => "bai",
                ItemCatalogRarity.Qing => "qing",
                ItemCatalogRarity.Lan => "lan",
                ItemCatalogRarity.Zi => "zi",
                ItemCatalogRarity.Cheng => "cheng",
                _ => "unknown"
            };
        }

        public static string ToDisplayName(this ItemCatalogRarity rarity)
        {
            return rarity switch
            {
                ItemCatalogRarity.Bai => "白",
                ItemCatalogRarity.Qing => "青",
                ItemCatalogRarity.Lan => "蓝",
                ItemCatalogRarity.Zi => "紫",
                ItemCatalogRarity.Cheng => "橙",
                _ => "未知"
            };
        }
    }
}
