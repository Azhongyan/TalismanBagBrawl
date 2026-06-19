#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Enemies;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Upgrades;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class DataCatalogValidator
    {
        [MenuItem("Tools/Talisman Bag/Validate Data Catalog")]
        public static void ValidateAndLog()
        {
            IReadOnlyList<DataCatalogValidationResult> results = Validate(DataCatalog.Collect());
            int errors = results.Count(result => result.Level == DataCatalogValidationLevel.Error);
            int warnings = results.Count(result => result.Level == DataCatalogValidationLevel.Warning);
            int infos = results.Count(result => result.Level == DataCatalogValidationLevel.Info);
            Debug.Log($"[DataCatalog] Validation finished. Error={errors}, Warning={warnings}, Info={infos}");
            foreach (DataCatalogValidationResult result in results)
            {
                switch (result.Level)
                {
                    case DataCatalogValidationLevel.Error:
                        Debug.LogError(result.ToString(), result.Context);
                        break;
                    case DataCatalogValidationLevel.Warning:
                        Debug.LogWarning(result.ToString(), result.Context);
                        break;
                    default:
                        Debug.Log(result.ToString(), result.Context);
                        break;
                }
            }
        }

        public static IReadOnlyList<DataCatalogValidationResult> Validate(DataCatalog catalog)
        {
            List<DataCatalogValidationResult> results = new();
            if (catalog == null)
            {
                results.Add(new DataCatalogValidationResult(
                    DataCatalogValidationLevel.Error,
                    "CATALOG_NULL",
                    "DataCatalog collection failed."));
                return results;
            }

            ValidateItems(catalog, results);
            ValidateEnemies(catalog, results);
            ValidateEnemyGroups(catalog, results);
            ValidateStages(catalog, results);
            ValidateRewards(catalog, results);
            ValidateDropTables(catalog, results);
            ValidateIdleDropConfigs(catalog, results);
            ValidateBossConfigs(catalog, results);
            ValidateUpgradeConfigs(catalog, results);

            if (results.Count == 0)
            {
                results.Add(new DataCatalogValidationResult(
                    DataCatalogValidationLevel.Info,
                    "CATALOG_OK",
                    "No catalog issues found."));
            }

            return results
                .OrderByDescending(result => result.Level)
                .ThenBy(result => result.Code, StringComparer.Ordinal)
                .ThenBy(result => result.AssetPath, StringComparer.Ordinal)
                .ToList();
        }

        private static void ValidateItems(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.Items,
                item => item.itemId,
                item => item.displayName,
                item => item.isDeprecated || item.isDebugOnly || item.sourceType == CatalogSourceType.Legacy,
                "itemId",
                results);
        }

        private static void ValidateEnemies(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.Enemies,
                enemy => enemy.enemyId,
                enemy => enemy.displayName,
                enemy => enemy.isDeprecated || enemy.isDebugOnly || enemy.sourceType == CatalogSourceType.Legacy,
                "enemyId",
                results);
        }

        private static void ValidateEnemyGroups(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.EnemyGroups,
                group => group.enemyGroupId,
                group => group.displayName,
                group => group.isDeprecated || group.isDebugOnly,
                "enemyGroupId",
                results);

            foreach (EnemyGroupConfig group in catalog.EnemyGroups)
            {
                if (group == null)
                {
                    continue;
                }

                if (group.entries == null || group.entries.Count == 0)
                {
                    Add(results, DataCatalogValidationLevel.Error, "ENEMY_GROUP_EMPTY", "Enemy group has no entries.", group);
                    continue;
                }

                for (int i = 0; i < group.entries.Count; i++)
                {
                    EnemyGroupEntry entry = group.entries[i];
                    if (entry?.enemy == null)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "ENEMY_GROUP_ORPHAN", $"Entry {i + 1} has no enemy reference.", group);
                        continue;
                    }

                    if (entry.enemy.isDebugOnly && !group.isDebugOnly)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "DEBUG_ENEMY_IN_PRODUCTION_GROUP", $"{entry.enemy.GetCatalogLabel()} is debug-only.", group);
                    }

                    if (entry.enemy.isDeprecated && !group.isDeprecated)
                    {
                        Add(results, DataCatalogValidationLevel.Warning, "DEPRECATED_ENEMY_IN_GROUP", $"{entry.enemy.GetCatalogLabel()} is deprecated.", group);
                    }
                }
            }
        }

        private static void ValidateStages(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            List<StageAssetRow> stages = new();
            foreach (V02RunConfig runConfig in catalog.RunConfigs)
            {
                AddStages(stages, runConfig, runConfig?.rounds, "rounds");
                AddStages(stages, runConfig, runConfig?.chapterTwoRounds, "chapterTwoRounds");
            }

            ValidateUnique(
                stages,
                stage => stage.Round?.levelId,
                stage => stage.Round?.roundTitle,
                _ => false,
                "stageId",
                results,
                stage => stage.Owner);

            HashSet<string> stageIds = new(
                stages.Select(stage => Normalize(stage.Round?.levelId)).Where(id => !string.IsNullOrEmpty(id)),
                StringComparer.Ordinal);

            foreach (StageAssetRow stage in stages)
            {
                V02RoundConfig round = stage.Round;
                if (round == null)
                {
                    Add(results, DataCatalogValidationLevel.Error, "STAGE_NULL", $"{stage.CollectionName} contains a null stage.", stage.Owner);
                    continue;
                }

                if (round.ResolveEnemyDefinition() == null)
                {
                    Add(results, DataCatalogValidationLevel.Error, "STAGE_ENEMY_ORPHAN", $"{round.levelId} has no enemy or enemy group.", stage.Owner);
                }

                string nextStageId = round.ResolveNextStageId();
                if (!string.IsNullOrEmpty(nextStageId) && !stageIds.Contains(nextStageId))
                {
                    Add(results, DataCatalogValidationLevel.Error, "STAGE_NEXT_ORPHAN", $"{round.levelId} references missing nextStageId '{nextStageId}'.", stage.Owner);
                }

                if (round.enemyGroup != null && (round.enemyGroup.isDebugOnly || round.enemyGroup.isDeprecated))
                {
                    Add(results, DataCatalogValidationLevel.Error, "NON_PRODUCTION_GROUP_IN_STAGE", $"{round.levelId} references hidden enemy group {round.enemyGroup.GetCatalogLabel()}.", stage.Owner);
                }

                if (round.stageType == StageType.IdleNormal && round.dropTable == null)
                {
                    Add(results, DataCatalogValidationLevel.Error, "IDLE_STAGE_DROP_TABLE_MISSING", $"{round.levelId} has no configured drop table.", stage.Owner);
                }

                if (round.stageType == StageType.Boss)
                {
                    if (round.bossConfig == null)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "BOSS_STAGE_CONFIG_MISSING", $"{round.levelId} has no BossConfig.", stage.Owner);
                    }
                    else
                    {
                        EnemyDefinition enemy = round.ResolveEnemyDefinition();
                        if (enemy != null &&
                            !string.Equals(
                                Normalize(enemy.enemyId),
                                Normalize(round.bossConfig.bossId),
                                StringComparison.Ordinal))
                        {
                            Add(
                                results,
                                DataCatalogValidationLevel.Error,
                                "BOSS_STAGE_CONFIG_MISMATCH",
                                $"{round.levelId} uses enemy '{enemy.enemyId}' but BossConfig '{round.bossConfig.bossId}'.",
                                stage.Owner);
                        }
                    }
                }
            }
        }

        private static void ValidateRewards(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.Rewards,
                reward => reward.rewardId,
                reward => reward.displayName,
                reward => reward.isDeprecated || reward.isDebugOnly,
                "rewardTableId",
                results);

            HashSet<string> activeItemIds = new(
                catalog.Items
                    .Where(item => item != null && !item.isDeprecated && !item.isDebugOnly)
                    .Select(item => Normalize(item.itemId))
                    .Where(id => !string.IsNullOrEmpty(id)),
                StringComparer.Ordinal);

            foreach (RewardConfig rewardConfig in catalog.Rewards)
            {
                if (rewardConfig?.rewards == null)
                {
                    continue;
                }

                foreach (RewardEntry reward in rewardConfig.rewards)
                {
                    if (reward == null || reward.rewardType != RewardType.Item)
                    {
                        continue;
                    }

                    string itemId = Normalize(reward.itemId);
                    if (string.IsNullOrEmpty(itemId) || !activeItemIds.Contains(itemId))
                    {
                        Add(results, DataCatalogValidationLevel.Error, "REWARD_ITEM_ORPHAN", $"{rewardConfig.rewardId} references missing itemId '{reward.itemId}'.", rewardConfig);
                    }
                }
            }
        }

        private static void ValidateDropTables(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.DropTables,
                table => table.tableId,
                table => table.displayName,
                table => table.isDeprecated || table.isDebugOnly,
                "dropTableId",
                results);

            HashSet<string> activeItemIds = new(
                catalog.Items
                    .Where(item => item != null && !item.isDeprecated && !item.isDebugOnly)
                    .Select(item => Normalize(item.itemId))
                    .Where(id => !string.IsNullOrEmpty(id)),
                StringComparer.Ordinal);

            foreach (RewardDropTable table in catalog.DropTables)
            {
                if (table == null)
                {
                    continue;
                }

                if (table.drops == null || table.drops.Count == 0)
                {
                    Add(results, DataCatalogValidationLevel.Error, "DROP_TABLE_EMPTY", "Drop table has no entries.", table);
                    continue;
                }

                for (int i = 0; i < table.drops.Count; i++)
                {
                    RewardDropEntry drop = table.drops[i];
                    if (drop?.reward == null || !drop.reward.IsValid)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "DROP_ENTRY_INVALID", $"Entry {i + 1} has an invalid reward.", table);
                        continue;
                    }

                    if (drop.chance <= 0f)
                    {
                        Add(results, DataCatalogValidationLevel.Warning, "DROP_ENTRY_DISABLED", $"Entry {i + 1} has zero chance.", table);
                    }

                    if (drop.minAmount <= 0 || drop.maxAmount < drop.minAmount)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "DROP_AMOUNT_INVALID", $"Entry {i + 1} has invalid amount range {drop.minAmount}-{drop.maxAmount}.", table);
                    }

                    if (drop.reward.rewardType == RewardType.Item)
                    {
                        string itemId = Normalize(drop.reward.itemId);
                        if (string.IsNullOrEmpty(itemId) || !activeItemIds.Contains(itemId))
                        {
                            Add(results, DataCatalogValidationLevel.Error, "DROP_ITEM_ORPHAN", $"{table.tableId} references missing itemId '{drop.reward.itemId}'.", table);
                        }
                    }
                }
            }
        }

        private static void ValidateIdleDropConfigs(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.IdleDropConfigs,
                config => config.configId,
                config => config.displayName,
                config => config.isDeprecated || config.isDebugOnly,
                "idleDropConfigId",
                results);

            foreach (IdleDropConfig config in catalog.IdleDropConfigs)
            {
                if (config == null)
                {
                    continue;
                }

                if (config.normalStageDropTable == null)
                {
                    Add(results, DataCatalogValidationLevel.Error, "IDLE_DROP_TABLE_ORPHAN", "Idle drop config has no normal-stage drop table.", config);
                }

                if (config.rollsPerStageClear <= 0)
                {
                    Add(results, DataCatalogValidationLevel.Error, "IDLE_DROP_ROLLS_INVALID", "rollsPerStageClear must be at least 1.", config);
                }
            }
        }

        private static void ValidateBossConfigs(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.BossConfigs,
                boss => boss.bossId,
                boss => boss.bossName,
                boss => boss.isDeprecated || boss.isDebugOnly,
                "bossConfigId",
                results);

            foreach (BossInfoConfig boss in catalog.BossConfigs)
            {
                if (boss == null)
                {
                    continue;
                }

                if (boss.bossEnemy == null)
                {
                    Add(results, DataCatalogValidationLevel.Error, "BOSS_ENEMY_ORPHAN", "BossConfig has no enemy reference.", boss);
                }
                else
                {
                    if (boss.bossEnemy.enemyType != EnemyType.Boss)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "BOSS_ENEMY_TYPE_INVALID", $"{boss.bossEnemy.GetCatalogLabel()} is not a Boss enemy.", boss);
                    }

                    if (!string.Equals(Normalize(boss.bossId), Normalize(boss.bossEnemy.enemyId), StringComparison.Ordinal))
                    {
                        Add(results, DataCatalogValidationLevel.Error, "BOSS_ID_MISMATCH", $"BossConfig id '{boss.bossId}' does not match enemy id '{boss.bossEnemy.enemyId}'.", boss);
                    }
                }

                if (boss.summonPhaseMinHpRatio < 0f ||
                    boss.shieldPhaseMinHpRatio > 1f ||
                    boss.summonPhaseMinHpRatio >= boss.shieldPhaseMinHpRatio)
                {
                    Add(results, DataCatalogValidationLevel.Error, "BOSS_PHASE_THRESHOLD_INVALID", "Boss phase thresholds must satisfy 0 <= summon < shield <= 1.", boss);
                }

                if (boss.firstActionDelay <= 0f ||
                    boss.shieldInterval <= 0f ||
                    boss.summonInterval <= 0f ||
                    boss.sealEyeInterval <= 0f ||
                    boss.sealDuration <= 0f ||
                    boss.energyDisruptionDuration <= 0f)
                {
                    Add(results, DataCatalogValidationLevel.Error, "BOSS_TIMING_INVALID", "Boss timing values must be greater than zero.", boss);
                }

                if (boss.shieldAmount < 0 || boss.summonDamage < 0 || boss.poisonStack < 0)
                {
                    Add(results, DataCatalogValidationLevel.Error, "BOSS_EFFECT_INVALID", "Boss effect values cannot be negative.", boss);
                }
            }
        }

        private static void ValidateUpgradeConfigs(DataCatalog catalog, List<DataCatalogValidationResult> results)
        {
            ValidateUnique(
                catalog.UpgradeConfigs,
                config => config.configId,
                config => config.displayName,
                config => config.isDeprecated || config.isDebugOnly,
                "upgradeConfigId",
                results);

            Dictionary<string, TalismanItemDefinition> activeItems = catalog.Items
                .Where(item => item != null && !item.isDeprecated && !item.isDebugOnly)
                .GroupBy(item => Normalize(item.itemId), StringComparer.Ordinal)
                .Where(group => !string.IsNullOrEmpty(group.Key))
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (TalismanUpgradeConfig config in catalog.UpgradeConfigs)
            {
                if (config == null)
                {
                    continue;
                }

                if (config.levels == null || config.levels.Count == 0)
                {
                    Add(results, DataCatalogValidationLevel.Error, "UPGRADE_CONFIG_EMPTY", "UpgradeConfig has no level rows.", config);
                    continue;
                }

                Dictionary<string, List<TalismanLevelConfig>> levelsByItem = new(StringComparer.Ordinal);
                for (int i = 0; i < config.levels.Count; i++)
                {
                    TalismanLevelConfig level = config.levels[i];
                    if (level == null)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "UPGRADE_LEVEL_NULL", $"Upgrade row {i + 1} is null.", config);
                        continue;
                    }

                    string itemId = Normalize(level.itemId);
                    string id = $"{itemId}:{level.fromLevel}->{level.toLevel}";
                    if (!ids.Add(id))
                    {
                        Add(results, DataCatalogValidationLevel.Error, "DUPLICATE_upgradeConfigId", $"Duplicate upgrade key '{id}'.", config);
                    }

                    if (string.IsNullOrEmpty(itemId) || !activeItems.TryGetValue(itemId, out TalismanItemDefinition item))
                    {
                        Add(results, DataCatalogValidationLevel.Error, "UPGRADE_ITEM_ORPHAN", $"Upgrade row {i + 1} references missing itemId '{level.itemId}'.", config);
                    }
                    else if (!item.canUpgrade)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "UPGRADE_ITEM_DISABLED", $"{item.GetCatalogLabel()} is not marked canUpgrade.", config);
                    }

                    if (!level.IsValid || level.fromLevel < 1)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "UPGRADE_LEVEL_RANGE_INVALID", $"{itemId} has invalid level range {level.fromLevel}->{level.toLevel}.", config);
                    }

                    if (!levelsByItem.TryGetValue(itemId, out List<TalismanLevelConfig> itemLevels))
                    {
                        itemLevels = new List<TalismanLevelConfig>();
                        levelsByItem[itemId] = itemLevels;
                    }
                    itemLevels.Add(level);

                    if (level.costs == null || level.costs.Count == 0)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "UPGRADE_COSTS_EMPTY", $"{id} has no resource costs.", config);
                    }
                    else
                    {
                        HashSet<ResourceType> costTypes = new();
                        foreach (ResourceCost cost in level.costs)
                        {
                            if (!cost.IsValid)
                            {
                                Add(results, DataCatalogValidationLevel.Error, "UPGRADE_COST_INVALID", $"{id} has an invalid {cost.resourceType} cost.", config);
                            }

                            if (!costTypes.Add(cost.resourceType))
                            {
                                Add(results, DataCatalogValidationLevel.Error, "UPGRADE_COST_DUPLICATE", $"{id} repeats resource cost {cost.resourceType}.", config);
                            }
                        }
                    }

                    StatModifier modifier = level.statModifier;
                    if (modifier == null)
                    {
                        Add(results, DataCatalogValidationLevel.Error, "UPGRADE_MODIFIER_MISSING", $"{id} has no StatModifier.", config);
                    }
                    else
                    {
                        bool positive = modifier.damageMultiplier > 0f &&
                                        modifier.cooldownMultiplier > 0f &&
                                        modifier.shieldMultiplier > 0f &&
                                        modifier.breakShieldMultiplier > 0f &&
                                        modifier.controlDurationMultiplier > 0f;
                        if (!positive)
                        {
                            Add(results, DataCatalogValidationLevel.Error, "UPGRADE_MODIFIER_INVALID", $"{id} has a non-positive multiplier.", config);
                        }

                        bool hasEffect = !Mathf.Approximately(modifier.damageMultiplier, 1f) ||
                                         !Mathf.Approximately(modifier.cooldownMultiplier, 1f) ||
                                         !Mathf.Approximately(modifier.shieldMultiplier, 1f) ||
                                         !Mathf.Approximately(modifier.breakShieldMultiplier, 1f) ||
                                         !Mathf.Approximately(modifier.controlDurationMultiplier, 1f);
                        if (!hasEffect)
                        {
                            Add(results, DataCatalogValidationLevel.Warning, "UPGRADE_MODIFIER_NO_EFFECT", $"{id} does not change any stat.", config);
                        }
                    }
                }

                foreach (KeyValuePair<string, List<TalismanLevelConfig>> pair in levelsByItem)
                {
                    List<TalismanLevelConfig> itemLevels = pair.Value
                        .OrderBy(level => level.fromLevel)
                        .ToList();
                    for (int i = 1; i < itemLevels.Count; i++)
                    {
                        if (itemLevels[i].fromLevel != itemLevels[i - 1].toLevel)
                        {
                            Add(results, DataCatalogValidationLevel.Error, "UPGRADE_LEVEL_GAP", $"{pair.Key} has a gap between Lv.{itemLevels[i - 1].toLevel} and Lv.{itemLevels[i].fromLevel}.", config);
                        }
                    }
                }
            }
        }

        private static void AddStages(
            List<StageAssetRow> destination,
            V02RunConfig owner,
            IReadOnlyList<V02RoundConfig> rounds,
            string collectionName)
        {
            if (owner == null || rounds == null)
            {
                return;
            }

            foreach (V02RoundConfig round in rounds)
            {
                destination.Add(new StageAssetRow(owner, round, collectionName));
            }
        }

        private static void ValidateUnique<T>(
            IReadOnlyList<T> assets,
            Func<T, string> idSelector,
            Func<T, string> nameSelector,
            Func<T, bool> hiddenSelector,
            string idLabel,
            List<DataCatalogValidationResult> results,
            Func<T, UnityEngine.Object> contextSelector = null)
            where T : class
        {
            Dictionary<string, List<T>> byId = new(StringComparer.Ordinal);
            Dictionary<string, HashSet<string>> idsByName = new(StringComparer.OrdinalIgnoreCase);

            foreach (T asset in assets)
            {
                if (asset == null)
                {
                    continue;
                }

                UnityEngine.Object context = contextSelector != null ? contextSelector(asset) : asset as UnityEngine.Object;
                string id = Normalize(idSelector(asset));
                if (string.IsNullOrEmpty(id))
                {
                    Add(results, DataCatalogValidationLevel.Error, $"MISSING_{idLabel}", $"Asset has an empty {idLabel}.", context);
                    continue;
                }

                if (!byId.TryGetValue(id, out List<T> rows))
                {
                    rows = new List<T>();
                    byId[id] = rows;
                }

                rows.Add(asset);

                string displayName = Normalize(nameSelector(asset));
                if (!string.IsNullOrEmpty(displayName))
                {
                    if (!idsByName.TryGetValue(displayName, out HashSet<string> ids))
                    {
                        ids = new HashSet<string>(StringComparer.Ordinal);
                        idsByName[displayName] = ids;
                    }

                    ids.Add(id);
                }
            }

            foreach ((string id, List<T> rows) in byId)
            {
                if (rows.Count <= 1)
                {
                    continue;
                }

                bool hasMultipleActive = rows.Count(row => !hiddenSelector(row)) > 1;
                foreach (T row in rows)
                {
                    UnityEngine.Object context = contextSelector != null ? contextSelector(row) : row as UnityEngine.Object;
                    Add(
                        results,
                        hasMultipleActive ? DataCatalogValidationLevel.Error : DataCatalogValidationLevel.Warning,
                        $"DUPLICATE_{idLabel}",
                        $"{idLabel} '{id}' is used by {rows.Count} assets. Hidden legacy/debug rows are excluded from the active catalog.",
                        context);
                }
            }

            foreach ((string name, HashSet<string> ids) in idsByName)
            {
                if (ids.Count <= 1)
                {
                    continue;
                }

                results.Add(new DataCatalogValidationResult(
                    DataCatalogValidationLevel.Warning,
                    $"SAME_NAME_DIFFERENT_{idLabel}",
                    $"Display name '{name}' maps to multiple IDs: {string.Join(", ", ids)}."));
            }
        }

        private static void Add(
            List<DataCatalogValidationResult> results,
            DataCatalogValidationLevel level,
            string code,
            string message,
            UnityEngine.Object context)
        {
            results.Add(new DataCatalogValidationResult(level, code, message, context, DataCatalog.GetPath(context)));
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class StageAssetRow
        {
            public StageAssetRow(V02RunConfig owner, V02RoundConfig round, string collectionName)
            {
                Owner = owner;
                Round = round;
                CollectionName = collectionName;
            }

            public V02RunConfig Owner { get; }
            public V02RoundConfig Round { get; }
            public string CollectionName { get; }
        }
    }
}
#endif
