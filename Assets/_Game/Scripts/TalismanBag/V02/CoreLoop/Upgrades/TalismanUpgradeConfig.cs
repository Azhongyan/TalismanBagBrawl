using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Resources;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Upgrades
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/CoreLoop/Talisman Upgrade Config", fileName = "CoreLoopTalismanUpgradeConfig")]
    public sealed class TalismanUpgradeConfig : ScriptableObject
    {
        public const string DefaultResourcePath = "CoreLoop/CoreLoopTalismanUpgradeConfig";

        public List<TalismanLevelConfig> levels = new();

        public TalismanLevelConfig FindUpgrade(string itemId, int currentLevel)
        {
            if (string.IsNullOrWhiteSpace(itemId) || levels == null)
            {
                return null;
            }

            foreach (TalismanLevelConfig level in levels)
            {
                if (level != null && level.Matches(itemId, currentLevel))
                {
                    return level;
                }
            }

            return null;
        }

        public static TalismanUpgradeConfig CreateRuntimeDefault()
        {
            TalismanUpgradeConfig config = CreateInstance<TalismanUpgradeConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.levels = BuildDefaultLevels();
            return config;
        }

        public static List<TalismanLevelConfig> BuildDefaultLevels()
        {
            return new List<TalismanLevelConfig>
            {
                CreateLevel("fire_talisman_basic", "火符", new StatModifier
                {
                    damageMultiplier = 1.25f,
                    summary = "伤害 +25%"
                }),
                CreateLevel("thunder_talisman_basic", "雷符", new StatModifier
                {
                    damageMultiplier = 1.15f,
                    breakShieldMultiplier = 1.3f,
                    summary = "伤害 +15%，破盾效率 +30%"
                }),
                CreateLevel("shield_talisman_basic", "护身符", new StatModifier
                {
                    shieldMultiplier = 1.25f,
                    summary = "护盾量 +25%"
                }),
                CreateLevel("purify_talisman_basic", "净化符", new StatModifier
                {
                    cooldownMultiplier = 0.8f,
                    summary = "冷却 -20%"
                }),
                CreateLevel("soul_suppress_talisman_basic", "镇魂符", new StatModifier
                {
                    controlDurationMultiplier = 1.2f,
                    summary = "压制时间 +20%"
                })
            };
        }

        private static TalismanLevelConfig CreateLevel(string itemId, string displayName, StatModifier modifier)
        {
            return new TalismanLevelConfig
            {
                itemId = itemId,
                displayName = displayName,
                fromLevel = 1,
                toLevel = 2,
                costs = BuildCommonCosts(),
                statModifier = modifier
            };
        }

        private static List<ResourceCost> BuildCommonCosts()
        {
            return new List<ResourceCost>
            {
                new(ResourceType.SpiritStone, 100),
                new(ResourceType.TalismanPaper, 50),
                new(ResourceType.Cinnabar, 8),
                new(ResourceType.BasicTalismanEmbryo, 1)
            };
        }
    }
}
