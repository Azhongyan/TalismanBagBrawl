#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Upgrades;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class Task08UpgradeConfigMigration
    {
        private const string UpgradeConfigPath =
            "Assets/_Game/Resources/CoreLoop/CoreLoopTalismanUpgradeConfig.asset";
        private const string ItemRoot =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/Items";

        private static readonly string[] ExpectedItemIds =
        {
            "fire_talisman_basic",
            "thunder_talisman_basic",
            "shield_talisman_basic",
            "purify_talisman_basic",
            "soul_suppress_talisman_basic"
        };

        public static void ExecuteBatch()
        {
            try
            {
                TalismanUpgradeConfig config = LoadConfig();
                config.configId = "core_loop_talisman_upgrades";
                config.displayName = "CoreLoop \u7b26\u7b93\u57f9\u517b";
                config.sourceType = CatalogSourceType.Verified;
                config.isDebugOnly = false;
                config.isDeprecated = false;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[StageConfigPanel01][Task08] MIGRATION_SUCCESS");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        public static void VerifyBatch()
        {
            TalismanUpgradeConfig config = LoadConfig();
            Require(config.configId == "core_loop_talisman_upgrades", "UpgradeConfig id is incorrect.");
            Require(config.sourceType == CatalogSourceType.Verified && !config.isDebugOnly && !config.isDeprecated,
                "UpgradeConfig must be an active verified asset.");
            Require(config.levels != null && config.levels.Count == ExpectedItemIds.Length,
                "UpgradeConfig must preserve exactly five verified Lv.1 to Lv.2 rows.");

            HashSet<string> actualIds = new(
                config.levels.Where(level => level != null).Select(level => level.itemId),
                StringComparer.Ordinal);
            Require(actualIds.SetEquals(ExpectedItemIds), "UpgradeConfig item rows do not match the verified set.");

            foreach (TalismanLevelConfig level in config.levels)
            {
                Require(level != null && level.fromLevel == 1 && level.toLevel == 2,
                    "Every verified upgrade row must remain Lv.1 to Lv.2.");
                Require(HasCommonCosts(level.costs), $"{level.itemId} does not preserve the common upgrade costs.");

                TalismanItemDefinition item = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(
                    $"{ItemRoot}/{level.itemId}.asset");
                Require(item != null && item.canUpgrade && !item.isDebugOnly && !item.isDeprecated,
                    $"{level.itemId} must reference an active upgradeable ItemDefinition.");
            }

            RequireModifier(config, "fire_talisman_basic", damage: 1.25f);
            RequireModifier(config, "thunder_talisman_basic", damage: 1.15f, breakShield: 1.3f);
            RequireModifier(config, "shield_talisman_basic", shield: 1.25f);
            RequireModifier(config, "purify_talisman_basic", cooldown: 0.8f);
            RequireModifier(config, "soul_suppress_talisman_basic", controlDuration: 1.2f);

            TalismanUpgradeConfig loadedFromResources =
                UnityEngine.Resources.Load<TalismanUpgradeConfig>(TalismanUpgradeConfig.DefaultResourcePath);
            Require(loadedFromResources == config, "UpgradeService resource path does not resolve the verified config.");

            IReadOnlyList<DataCatalogValidationResult> validation = DataCatalogValidator.Validate(DataCatalog.Collect());
            Require(validation.All(result => result.Level != DataCatalogValidationLevel.Error),
                "DataCatalog contains Error results.");
            Debug.Log($"[StageConfigPanel01][Task08] SMOKE_SUCCESS upgrades={config.levels.Count}, validation={validation.Count}");
        }

        private static TalismanUpgradeConfig LoadConfig()
        {
            TalismanUpgradeConfig config =
                AssetDatabase.LoadAssetAtPath<TalismanUpgradeConfig>(UpgradeConfigPath);
            if (config == null)
            {
                throw new InvalidOperationException("Verified CoreLoopTalismanUpgradeConfig asset is missing.");
            }

            return config;
        }

        private static bool HasCommonCosts(IReadOnlyList<ResourceCost> costs)
        {
            if (costs == null || costs.Count != 4)
            {
                return false;
            }

            Dictionary<ResourceType, int> expected = new()
            {
                [ResourceType.SpiritStone] = 100,
                [ResourceType.TalismanPaper] = 50,
                [ResourceType.Cinnabar] = 8,
                [ResourceType.BasicTalismanEmbryo] = 1
            };
            return costs.All(cost =>
                expected.TryGetValue(cost.resourceType, out int amount) && amount == cost.amount) &&
                   costs.Select(cost => cost.resourceType).Distinct().Count() == expected.Count;
        }

        private static void RequireModifier(
            TalismanUpgradeConfig config,
            string itemId,
            float damage = 1f,
            float cooldown = 1f,
            float shield = 1f,
            float breakShield = 1f,
            float controlDuration = 1f)
        {
            TalismanLevelConfig level = config.levels.FirstOrDefault(row =>
                row != null && string.Equals(row.itemId, itemId, StringComparison.Ordinal));
            Require(level?.statModifier != null, $"Missing StatModifier for {itemId}.");
            StatModifier modifier = level.statModifier;
            Require(Mathf.Approximately(modifier.damageMultiplier, damage) &&
                    Mathf.Approximately(modifier.cooldownMultiplier, cooldown) &&
                    Mathf.Approximately(modifier.shieldMultiplier, shield) &&
                    Mathf.Approximately(modifier.breakShieldMultiplier, breakShield) &&
                    Mathf.Approximately(modifier.controlDurationMultiplier, controlDuration),
                $"{itemId} modifier values changed from the verified snapshot.");
            Require(!string.IsNullOrWhiteSpace(modifier.summary), $"{itemId} modifier summary is empty.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
#endif
