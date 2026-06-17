using TalismanBag.Grid;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.CoreLoop.Upgrades;
using TalismanBag.V02.Formation;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Battle
{
    public sealed class BattleLoadoutSnapshotBuilder : MonoBehaviour
    {
        [SerializeField] private SaveService saveService;
        [SerializeField] private TalismanUpgradeConfig upgradeConfig;

        private TalismanUpgradeConfig runtimeDefaultConfig;

        public BattleLoadoutSnapshot Build(TalismanBagGrid grid, FormationPowerResolver formationPowerResolver)
        {
            BattleLoadoutSnapshot snapshot = new();
            if (grid == null)
            {
                return snapshot;
            }

            SaveData saveData = EnsureSaveService().EnsureLoaded();
            PlayerTalismanProgressData progressData = saveData.talismanProgressData ?? new PlayerTalismanProgressData();
            progressData.Normalize();

            foreach (TalismanItemRuntime item in grid.GetAllPlacedItems())
            {
                if (item?.definition == null)
                {
                    continue;
                }

                string itemId = item.definition.itemId;
                int permanentLevel = progressData.GetLevel(itemId);
                int effectiveLevel = Mathf.Max(1, Mathf.Max(item.level, permanentLevel));
                bool isPowered = formationPowerResolver == null || formationPowerResolver.IsItemPowered(item);

                snapshot.items.Add(new BattleLoadoutItemSnapshot
                {
                    runtimeId = item.runtimeId,
                    itemId = itemId,
                    displayName = item.definition.displayName,
                    level = effectiveLevel,
                    gridPosition = item.gridPosition,
                    isPowered = isPowered,
                    computedStats = BuildComputedStats(item.definition, effectiveLevel)
                });
            }

            return snapshot;
        }

        private ComputedTalismanStats BuildComputedStats(TalismanItemDefinition definition, int level)
        {
            ComputedTalismanStats stats = new()
            {
                computedCooldown = definition != null ? Mathf.Max(0.1f, definition.baseCooldown) : 1f,
                computedBreakShieldRate = 1f,
                computedControlDuration = 1f
            };

            if (definition == null)
            {
                return stats;
            }

            int baseValue = Mathf.Max(0, definition.baseValue);
            stats.computedDamage = definition.itemType == TalismanItemType.AttackTalisman || definition.itemType == TalismanItemType.SupportTalisman
                ? baseValue
                : 0;
            stats.computedShieldValue = definition.itemType == TalismanItemType.ShieldTalisman ? baseValue : 0;

            if (level <= 1)
            {
                return stats;
            }

            TalismanLevelConfig levelConfig = EnsureUpgradeConfig().FindUpgrade(definition.itemId, 1);
            StatModifier modifier = levelConfig?.statModifier;
            if (modifier == null)
            {
                return stats;
            }

            if (stats.computedDamage > 0 && !Mathf.Approximately(modifier.damageMultiplier, 1f))
            {
                stats.computedDamage = Mathf.Max(1, Mathf.RoundToInt(stats.computedDamage * modifier.damageMultiplier));
            }

            if (!Mathf.Approximately(modifier.cooldownMultiplier, 1f))
            {
                stats.computedCooldown = Mathf.Max(0.1f, stats.computedCooldown * modifier.cooldownMultiplier);
            }

            if (stats.computedShieldValue > 0 && !Mathf.Approximately(modifier.shieldMultiplier, 1f))
            {
                stats.computedShieldValue = Mathf.Max(1, Mathf.RoundToInt(stats.computedShieldValue * modifier.shieldMultiplier));
            }

            if (!Mathf.Approximately(modifier.breakShieldMultiplier, 1f))
            {
                stats.computedBreakShieldRate = Mathf.Max(1f, modifier.breakShieldMultiplier);
            }

            if (!Mathf.Approximately(modifier.controlDurationMultiplier, 1f))
            {
                stats.computedControlDuration = Mathf.Max(1f, modifier.controlDurationMultiplier);
            }

            return stats;
        }

        private SaveService EnsureSaveService()
        {
            saveService ??= SaveService.GetOrCreate();
            return saveService;
        }

        private TalismanUpgradeConfig EnsureUpgradeConfig()
        {
            if (upgradeConfig != null)
            {
                return upgradeConfig;
            }

            upgradeConfig = UnityEngine.Resources.Load<TalismanUpgradeConfig>(TalismanUpgradeConfig.DefaultResourcePath);
            if (upgradeConfig != null)
            {
                return upgradeConfig;
            }

            runtimeDefaultConfig ??= TalismanUpgradeConfig.CreateRuntimeDefault();
            return runtimeDefaultConfig;
        }
    }
}
