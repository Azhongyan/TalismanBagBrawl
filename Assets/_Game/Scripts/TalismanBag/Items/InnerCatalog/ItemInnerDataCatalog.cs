using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.Items.InnerCatalog
{
    public static class ItemInnerDataCatalog
    {
        private static readonly ItemCatalogRarity[] DefaultAllowedRarities =
        {
            ItemCatalogRarity.Bai,
            ItemCatalogRarity.Qing,
            ItemCatalogRarity.Lan,
            ItemCatalogRarity.Zi,
            ItemCatalogRarity.Cheng
        };

        private static readonly Vector2Int[] Single = { new(0, 0) };
        private static readonly Vector2Int[] Line2H = { new(0, 0), new(1, 0) };
        private static readonly Vector2Int[] Line2V = { new(0, 0), new(0, 1) };
        private static readonly Vector2Int[] Line3H = { new(0, 0), new(1, 0), new(2, 0) };
        private static readonly Vector2Int[] Line3V = { new(0, 0), new(0, 1), new(0, 2) };
        private static readonly Vector2Int[] Corner3 = { new(0, 0), new(1, 0), new(0, 1) };
        private static readonly Vector2Int[] Block2X2 = { new(0, 0), new(1, 0), new(0, 1), new(1, 1) };

        private static readonly List<ItemInnerDataDefinition> Items = new()
        {
            CreateFaMenItem("I001", "震雷符", ItemFaMenTag.Zhenlei, ItemQiLeiTag.Fu, "shape_single_1", Single, new Vector2Int(0, 0), ItemCatalogRarity.Bai, "引雷短击", "战斗中按节奏引下一记短雷。"),
            CreateFaMenItem("I002", "五雷急符", ItemFaMenTag.Zhenlei, ItemQiLeiTag.Fu, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Qing, "急雷连引", "连续触发时提高雷法出手频率预览。"),
            CreateFaMenItem("I003", "震雷破壳印", ItemFaMenTag.Zhenlei, ItemQiLeiTag.Yin, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Lan, "破壳压印", "对护壳与减伤目标提供破势预览。"),
            CreateFaMenItem("I004", "五雷急令", ItemFaMenTag.Zhenlei, ItemQiLeiTag.Ling, "shape_line2_v", Line2V, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "急令催发", "条件满足时催发一次震雷令文。"),
            CreateFaMenItem("I005", "照壳雷镜", ItemFaMenTag.Zhenlei, ItemQiLeiTag.Jing, "shape_line2_v", Line2V, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "照壳回雷", "照出护壳弱点并回响雷击预览。"),
            CreateFaMenItem("I006", "天鼓槌", ItemFaMenTag.Zhenlei, ItemQiLeiTag.Fa, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Zi, "天鼓震鸣", "周期性震鸣，作为震雷大件预览。"),

            CreateFaMenItem("I007", "离火符", ItemFaMenTag.Lihuo, ItemQiLeiTag.Fu, "shape_single_1", Single, new Vector2Int(0, 0), ItemCatalogRarity.Bai, "燃符小火", "点燃一段离火伤害预览。"),
            CreateFaMenItem("I008", "焚邪长符", ItemFaMenTag.Lihuo, ItemQiLeiTag.Fu, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Qing, "长符焚邪", "长符持续压制邪气目标预览。"),
            CreateFaMenItem("I009", "离火焚邪印", ItemFaMenTag.Lihuo, ItemQiLeiTag.Yin, "shape_single_1", Single, new Vector2Int(0, 0), ItemCatalogRarity.Lan, "焚邪压印", "印心落火，强化离火持续段预览。"),
            CreateFaMenItem("I010", "风火令旗", ItemFaMenTag.Lihuo, ItemQiLeiTag.Ling, "shape_corner3", Corner3, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "风火起令", "以令旗带起短促风火节奏。"),
            CreateFaMenItem("I011", "照焚铜镜", ItemFaMenTag.Lihuo, ItemQiLeiTag.Jing, "shape_line2_v", Line2V, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "照焚回光", "镜照后留下离火余焰预览。"),
            CreateFaMenItem("I012", "长明灯", ItemFaMenTag.Lihuo, ItemQiLeiTag.Fa, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Zi, "灯火长明", "离火法大件，提供长明火势预览。"),

            CreateFaMenItem("I013", "护身符", ItemFaMenTag.Zhongyue, ItemQiLeiTag.Fu, "shape_single_1", Single, new Vector2Int(0, 0), ItemCatalogRarity.Bai, "护身缓冲", "提供一次轻量护身缓冲预览。"),
            CreateFaMenItem("I014", "镇宅长符", ItemFaMenTag.Zhongyue, ItemQiLeiTag.Fu, "shape_line3_v", Line3V, new Vector2Int(0, 1), ItemCatalogRarity.Qing, "镇宅连符", "横向镇守，提供中岳稳固预览。"),
            CreateFaMenItem("I015", "中岳镇守章", ItemFaMenTag.Zhongyue, ItemQiLeiTag.Yin, "shape_line3_v", Line3V, new Vector2Int(0, 0), ItemCatalogRarity.Lan, "镇守落章", "章印落定后提供镇守防护预览。"),
            CreateFaMenItem("I016", "护坛令旗", ItemFaMenTag.Zhongyue, ItemQiLeiTag.Ling, "shape_corner3", Corner3, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "护坛号令", "令旗立坛，提供短时护坛预览。"),
            CreateFaMenItem("I017", "照邪八卦镜", ItemFaMenTag.Zhongyue, ItemQiLeiTag.Jing, "shape_line2_v", Line2V, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "照邪定影", "照出邪影并提供防护判断预览。"),
            CreateFaMenItem("I018", "镇山铜鼎", ItemFaMenTag.Zhongyue, ItemQiLeiTag.Fa, "shape_block2x2", Block2X2, new Vector2Int(1, 1), ItemCatalogRarity.Zi, "镇山压阵", "中岳大件，提供压阵与守势预览。"),

            CreateFaMenItem("I019", "净水符", ItemFaMenTag.Xuanshui, ItemQiLeiTag.Fu, "shape_single_1", Single, new Vector2Int(0, 0), ItemCatalogRarity.Bai, "净水拂秽", "净去一层秽气倾向预览。"),
            CreateFaMenItem("I020", "涤秽长符", ItemFaMenTag.Xuanshui, ItemQiLeiTag.Fu, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Qing, "长符涤秽", "持续清涤负面状态预览。"),
            CreateFaMenItem("I021", "玄水涤秽印", ItemFaMenTag.Xuanshui, ItemQiLeiTag.Yin, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Lan, "涤秽印心", "以印心凝水，提供清秽回护预览。"),
            CreateFaMenItem("I022", "玄水流令", ItemFaMenTag.Xuanshui, ItemQiLeiTag.Ling, "shape_line3_v", Line3V, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "流令回转", "令文流转，提供节奏回转预览。"),
            CreateFaMenItem("I023", "照秽水镜", ItemFaMenTag.Xuanshui, ItemQiLeiTag.Jing, "shape_line2_v", Line2V, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "照秽成涟", "镜照秽气并触发水涟预览。"),
            CreateFaMenItem("I024", "净水盂", ItemFaMenTag.Xuanshui, ItemQiLeiTag.Fa, "shape_line2_h", Line2H, new Vector2Int(0, 0), ItemCatalogRarity.Zi, "净盂蓄水", "玄水大件，蓄水净化与回护预览。"),

            CreateFaMenItem("I025", "封煞符", ItemFaMenTag.Taibai, ItemQiLeiTag.Fu, "shape_single_1", Single, new Vector2Int(0, 0), ItemCatalogRarity.Bai, "封煞短符", "短暂封住煞气行动预览。"),
            CreateFaMenItem("I026", "太白斩符", ItemFaMenTag.Taibai, ItemQiLeiTag.Fu, "shape_line3_v", Line3V, new Vector2Int(0, 0), ItemCatalogRarity.Qing, "太白斩符", "以符气形成一次斩击预览。"),
            CreateFaMenItem("I027", "太白斩煞印", ItemFaMenTag.Taibai, ItemQiLeiTag.Yin, "shape_line2_v", Line2V, new Vector2Int(0, 0), ItemCatalogRarity.Lan, "斩煞压印", "印心锁定煞气并斩断预览。"),
            CreateFaMenItem("I028", "断煞令", ItemFaMenTag.Taibai, ItemQiLeiTag.Ling, "shape_line2_v", Line2V, new Vector2Int(0, 1), ItemCatalogRarity.Lan, "断煞号令", "令文触发断煞窗口预览。"),
            CreateFaMenItem("I029", "照煞镜", ItemFaMenTag.Taibai, ItemQiLeiTag.Jing, "shape_single_1", Single, new Vector2Int(0, 0), ItemCatalogRarity.Lan, "照煞回锋", "镜中照煞并形成回锋预览。"),
            CreateFaMenItem("I030", "桃木剑", ItemFaMenTag.Taibai, ItemQiLeiTag.Fa, "shape_line3_v", Line3V, new Vector2Int(0, 1), ItemCatalogRarity.Zi, "桃木斩邪", "太白大件，以桃木剑斩邪预览。"),

            CreateZhonggongSource()
        };

        public static IReadOnlyList<ItemInnerDataDefinition> AllItems => Items;

        public static ItemInnerDataDefinition FindById(string itemId)
        {
            return Items.FirstOrDefault(item => string.Equals(item.itemId, itemId, System.StringComparison.Ordinal));
        }

        private static ItemInnerDataDefinition CreateFaMenItem(
            string itemId,
            string displayName,
            ItemFaMenTag faMenTag,
            ItemQiLeiTag qiLeiTag,
            string shapeId,
            IReadOnlyList<Vector2Int> cells,
            Vector2Int coreCell,
            ItemCatalogRarity rarity,
            string basePowerText,
            string basicEffectText)
        {
            string faMenName = faMenTag.ToDisplayName();
            string qiLeiName = qiLeiTag.ToDisplayName();
            return new ItemInnerDataDefinition
            {
                itemId = itemId,
                displayName = displayName,
                itemFamily = ItemFamilyTag.FaMenCombat,
                faMenTag = faMenTag,
                qiLeiTag = qiLeiTag,
                shapeId = shapeId,
                defaultLocalCells = cells.ToList(),
                coreCellLocal = coreCell,
                displayRarityName = rarity.ToDisplayName(),
                rarityDefault = rarity,
                allowedRarities = DefaultAllowedRarities.ToList(),
                basePowerText = basePowerText,
                itemPower = string.Empty,
                primaryStats = BuildPrimaryStats(shapeId, cells, coreCell, faMenName, qiLeiName),
                triggerText = "目录预览：点亮后可在战斗中发动。本包不计算真实点亮、接亮、冷却或结算。",
                basicEffectText = basicEffectText,
                coreEffectPreviewText = $"核心效果预览：{displayName} 开窍后强化 {faMenName} 的核心节奏。",
                awakeningPreview = "开窍预览：Lv.10 / Lv.20 / Lv.30 / Lv.40 节点仅作目录占位。",
                fixedAffixPreview = $"{faMenName}固定词条预览：强化本法门基础倾向。",
                randomAffixPreview = "随机词条预览：后续词条池接入前仅显示占位。",
                orangeAffixPreview = rarity == ItemCatalogRarity.Cheng
                    ? $"橙色专属预览：{displayName} 道痕待开。"
                    : "橙色专属预览：高品阶后可出现，本包不 roll 词条。",
                placementHint = "摆放提示：避开 5x5 中心阵眼石；coreCell 需要进入点亮范围或接亮链。",
                flavorText = $"旧物记：{displayName} 属于 {faMenName} · {qiLeiName}，当前为内层目录只读数据。",
                iconPlaceholderKey = itemId.ToLowerInvariant() + "_placeholder",
                isLightingSource = false
            };
        }

        private static ItemInnerDataDefinition CreateZhonggongSource()
        {
            return new ItemInnerDataDefinition
            {
                itemId = "I031",
                displayName = "聚念石",
                itemFamily = ItemFamilyTag.ZhonggongSource,
                faMenTag = ItemFaMenTag.Zhonggong,
                qiLeiTag = ItemQiLeiTag.ZhonggongQi,
                shapeId = "shape_single_1",
                defaultLocalCells = Single.ToList(),
                coreCellLocal = new Vector2Int(0, 0),
                displayRarityName = ItemCatalogRarity.Qing.ToDisplayName(),
                rarityDefault = ItemCatalogRarity.Qing,
                allowedRarities = DefaultAllowedRarities.ToList(),
                basePowerText = "唯一点亮来源",
                itemPower = string.Empty,
                primaryStats = BuildPrimaryStats("shape_single_1", Single, new Vector2Int(0, 0), "中宫", "中宫符器"),
                triggerText = "目录预览：聚念石是第一版唯一点亮来源；本包不生成 litRangeCells，不做真实点亮。",
                basicEffectText = "接入阵眼后提供点亮来源身份预览，不直接解锁任何道具核心效果。",
                coreEffectPreviewText = "核心效果预览：聚念回流。需后续开窍系统接入后再生效。",
                awakeningPreview = "开窍预览：聚念石可有养成节点，但当前只记录目录身份。",
                fixedAffixPreview = "固定词条预览：聚念源。",
                randomAffixPreview = "随机词条预览：本包不 roll 词条。",
                orangeAffixPreview = "橙色专属预览：中宫道痕预留。",
                placementHint = "摆放提示：聚念石围绕阵眼建立点亮来源；阵眼石本身不可覆盖且不进入 catalog。",
                flavorText = "旧物记：一枚沉在中宫气息里的石子，只在目录中标记为点亮来源，不执行点亮逻辑。",
                iconPlaceholderKey = "i031_junian_stone_placeholder",
                isLightingSource = true
            };
        }

        private static List<ItemInnerStatLine> BuildPrimaryStats(
            string shapeId,
            IReadOnlyList<Vector2Int> cells,
            Vector2Int coreCell,
            string faMenName,
            string qiLeiName)
        {
            return new List<ItemInnerStatLine>
            {
                new("法门", faMenName, "稳定 tag 用于后续 Build 统计"),
                new("器类", qiLeiName, "稳定 tag 用于后续器类 Build 统计"),
                new("形状", $"{shapeId} · {cells.Count}格", "只读目录数据，不做摆放验证"),
                new("核心格", ItemInnerDataDefinition.FormatCell(coreCell), "本包只记录 coreCellLocal")
            };
        }
    }
}
