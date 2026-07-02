#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxConfigValidator
    {
        public const string ConfigRoot = "Assets/_Game/Configs/BuildSandbox";

        private static readonly int[] AllowedSynergyThresholds = { 2, 4, 6, 8 };

        private static readonly string[] SupportedPlacementConditionTypes =
        {
            "none",
            "adjacent",
            "adjacent_to_tag",
            "neighbor",
            "separated",
            "separated_from_tag",
            "not_adjacent_to_tag",
            "same_row",
            "same_column",
            "edge",
            "corner",
            "near_center",
            "near_eye",
            "around_core"
        };

        private static readonly string[] RequiredFoundationTags =
        {
            "离火",
            "惊雷",
            "护阵",
            "净厄",
            "镇魂",
            "聚能",
            "剑器",
            "符箓",
            "法器",
            "黑炉污染"
        };

        private static readonly string[] DevOnlyConfigTypeFilters =
        {
            "t:BuildSandboxDevOnlyConfig",
            "t:ItemTagConfig",
            "t:SynergyConfig",
            "t:BuildArchetypeConfig",
            "t:ItemShapeConfig",
            "t:RarityTierConfig",
            "t:AffixConfig",
            "t:MapRuleConfig",
            "t:EnemyProblemConfig",
            "t:BossProblemConfig",
            "t:BuildReadinessCheckConfig",
            "t:WeaknessWindowConfig",
            "t:DropBiasConfig",
            "t:FailureHintConfig"
        };

        private static readonly string[] ForbiddenReferenceTokens =
        {
            "RewardConfig",
            "RewardDropTable",
            "UpgradeConfig",
            "V02RunFlowController",
            "MainTrialProgressData",
            "PlayerPrefs",
            "SaveData",
            "BossReward",
            "FormalDrop",
            "FormalForge",
            "MainTrial"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Config Validation");
            ValidateFeatureFlags(report);
            ValidateConfigAssets(report);
            return report;
        }

        public static IReadOnlyList<BuildSandboxDevOnlyConfig> CollectConfigAssets()
        {
            if (!AssetDatabase.IsValidFolder(ConfigRoot))
            {
                return Array.Empty<BuildSandboxDevOnlyConfig>();
            }

            return DevOnlyConfigTypeFilters
                .SelectMany(filter => AssetDatabase.FindAssets(filter, new[] { ConfigRoot }))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct(StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<BuildSandboxDevOnlyConfig>)
                .Where(config => config != null)
                .OrderBy(config => AssetDatabase.GetAssetPath(config), StringComparer.Ordinal)
                .ToArray();
        }

        public static IReadOnlyList<ItemTagConfig> CollectItemTagConfigs()
        {
            return CollectTypedConfigAssets<ItemTagConfig>("t:ItemTagConfig");
        }

        public static IReadOnlyList<SynergyConfig> CollectSynergyConfigs()
        {
            return CollectTypedConfigAssets<SynergyConfig>("t:SynergyConfig");
        }

        public static IReadOnlyList<BuildArchetypeConfig> CollectBuildArchetypeConfigs()
        {
            return CollectTypedConfigAssets<BuildArchetypeConfig>("t:BuildArchetypeConfig");
        }

        public static IReadOnlyList<ItemShapeConfig> CollectItemShapeConfigs()
        {
            return CollectTypedConfigAssets<ItemShapeConfig>("t:ItemShapeConfig");
        }

        public static IReadOnlyList<RarityTierConfig> CollectRarityTierConfigs()
        {
            return CollectTypedConfigAssets<RarityTierConfig>("t:RarityTierConfig");
        }

        public static IReadOnlyList<AffixConfig> CollectAffixConfigs()
        {
            return CollectTypedConfigAssets<AffixConfig>("t:AffixConfig");
        }

        public static IReadOnlyList<string> RequiredTags => RequiredFoundationTags;

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError(
                        "FEATURE_FLAG_DEFAULT_TRUE",
                        $"{flag.Key} must default false.",
                        "BuildSandboxFeatureFlags");
                    continue;
                }

                report.AddInfo(
                    "FEATURE_FLAG_DEFAULT_FALSE",
                    $"{flag.Key}=false ({flag.Scope}).",
                    "BuildSandboxFeatureFlags");
            }
        }

        private static void ValidateConfigAssets(BuildSandboxValidationReport report)
        {
            IReadOnlyList<BuildSandboxDevOnlyConfig> configs = CollectConfigAssets();
            if (configs.Count == 0)
            {
                report.AddInfo(
                    "NO_BUILDSANDBOX_CONFIG_ASSETS",
                    "No BuildSandboxDevOnlyConfig assets found. Runtime feature flags still default false.",
                    ConfigRoot);
                ValidateDevChapterContentPool(report);
                ValidateLedgerTaskBuildHooks(report);
                return;
            }

            foreach (BuildSandboxDevOnlyConfig config in configs)
            {
                string path = AssetDatabase.GetAssetPath(config);
                if (string.IsNullOrWhiteSpace(config.configId))
                {
                    report.AddError("CONFIG_ID_MISSING", "Config id is empty.", path);
                }

                foreach (string reference in config.referencedSystemKeys ?? Enumerable.Empty<string>())
                {
                    string normalized = reference ?? string.Empty;
                    if (ForbiddenReferenceTokens.Any(token =>
                            normalized.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        report.AddError(
                            "FORMAL_SYSTEM_REFERENCE",
                            $"Config references a formal system key: {normalized}.",
                            path);
                    }
                }

                report.AddInfo(
                    "CONFIG_ASSET_SCANNED",
                    $"Scanned {config.configId}. devOnly={config.devOnly}, isEnabled={config.isEnabled}.",
                    path);
            }

            ValidateSynergyDataFoundation(report);
            ValidateItemShapeConfigs(report);
            ValidateEnemyBossValidationPool(report);
            ValidateDevChapterContentPool(report);
            ValidateLedgerTaskBuildHooks(report);
        }

        private static IReadOnlyList<T> CollectTypedConfigAssets<T>(string filter)
            where T : BuildSandboxDevOnlyConfig
        {
            if (!AssetDatabase.IsValidFolder(ConfigRoot))
            {
                return Array.Empty<T>();
            }

            return AssetDatabase
                .FindAssets(filter, new[] { ConfigRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct(StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(config => config != null)
                .OrderBy(config => AssetDatabase.GetAssetPath(config), StringComparer.Ordinal)
                .ToArray();
        }

        private static void ValidateSynergyDataFoundation(BuildSandboxValidationReport report)
        {
            IReadOnlyList<ItemTagConfig> itemTags = CollectItemTagConfigs();
            IReadOnlyList<SynergyConfig> synergies = CollectSynergyConfigs();
            IReadOnlyList<BuildArchetypeConfig> archetypes = CollectBuildArchetypeConfigs();

            ValidateItemTags(report, itemTags);
            ValidateSynergies(report, synergies);
            ValidateArchetypes(report, archetypes, synergies);

            report.AddInfo(
                "SYNERGY_DATA_FOUNDATION_COUNTS",
                $"ItemTagConfig={itemTags.Count}, SynergyConfig={synergies.Count}, BuildArchetypeConfig={archetypes.Count}.",
                ConfigRoot);
        }

        private static void ValidateItemTags(
            BuildSandboxValidationReport report,
            IReadOnlyList<ItemTagConfig> itemTags)
        {
            if (itemTags.Count == 0)
            {
                report.AddError(
                    "ITEM_TAG_CONFIG_MISSING",
                    "SynergyDataFoundation01 requires at least one ItemTagConfig asset.",
                    ConfigRoot);
                return;
            }

            HashSet<string> observedTags = new(StringComparer.Ordinal);
            foreach (ItemTagConfig config in itemTags)
            {
                string path = AssetDatabase.GetAssetPath(config);
                if (string.IsNullOrWhiteSpace(config.itemId))
                {
                    report.AddError("ITEM_ID_MISSING", "ItemTagConfig itemId is empty.", path);
                }

                if (config.tags == null || config.tags.Count == 0)
                {
                    report.AddError("ITEM_TAGS_EMPTY", "ItemTagConfig tags list is empty.", path);
                    continue;
                }

                HashSet<string> localTags = new(StringComparer.Ordinal);
                foreach (string rawTag in config.tags)
                {
                    string tag = rawTag ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        report.AddError("ITEM_TAG_EMPTY", "ItemTagConfig contains an empty tag.", path);
                        continue;
                    }

                    if (!localTags.Add(tag))
                    {
                        report.AddError("ITEM_TAG_DUPLICATE", $"ItemTagConfig repeats tag: {tag}.", path);
                    }

                    observedTags.Add(tag);
                }
            }

            foreach (string requiredTag in RequiredFoundationTags)
            {
                if (!observedTags.Contains(requiredTag))
                {
                    report.AddError(
                        "REQUIRED_TAG_MISSING",
                        $"Required BuildSandbox tag is missing: {requiredTag}.",
                        ConfigRoot);
                }
            }

            if (RequiredFoundationTags.All(tag => observedTags.Contains(tag)))
            {
                report.AddInfo(
                    "REQUIRED_TAGS_PRESENT",
                    "All first-batch BuildSandbox tags are reserved in ItemTagConfig assets.",
                    ConfigRoot);
            }
        }

        private static void ValidateSynergies(
            BuildSandboxValidationReport report,
            IReadOnlyList<SynergyConfig> synergies)
        {
            if (synergies.Count == 0)
            {
                report.AddError(
                    "SYNERGY_CONFIG_MISSING",
                    "SynergyDataFoundation01 requires at least one SynergyConfig asset.",
                    ConfigRoot);
                return;
            }

            Dictionary<string, string> synergyIds = new(StringComparer.Ordinal);
            foreach (SynergyConfig config in synergies)
            {
                string path = AssetDatabase.GetAssetPath(config);
                if (string.IsNullOrWhiteSpace(config.synergyId))
                {
                    report.AddError("SYNERGY_ID_MISSING", "SynergyConfig synergyId is empty.", path);
                }
                else if (synergyIds.TryGetValue(config.synergyId, out string existingPath))
                {
                    report.AddError(
                        "SYNERGY_ID_DUPLICATE",
                        $"Synergy id '{config.synergyId}' is already used by {existingPath}.",
                        path);
                }
                else
                {
                    synergyIds.Add(config.synergyId, path);
                }

                ValidateSynergyRequiredTags(report, config, path);
                ValidateThresholds(report, config.thresholds, path);
                ValidateConditionIsolation(report, config.placementConditions, path, "SYNERGY_PLACEMENT");
                ValidateConditionIsolation(report, config.energyConditions, path, "SYNERGY_ENERGY");
            }
        }

        private static void ValidateSynergyRequiredTags(
            BuildSandboxValidationReport report,
            SynergyConfig config,
            string path)
        {
            if (config.requiredTags == null || config.requiredTags.Count == 0)
            {
                report.AddError("SYNERGY_REQUIRED_TAGS_MISSING", "SynergyConfig has no requiredTags.", path);
                return;
            }

            HashSet<string> tags = new(StringComparer.Ordinal);
            foreach (string rawTag in config.requiredTags)
            {
                string tag = rawTag ?? string.Empty;
                if (string.IsNullOrWhiteSpace(tag))
                {
                    report.AddError("SYNERGY_REQUIRED_TAG_EMPTY", "SynergyConfig contains an empty requiredTag.", path);
                    continue;
                }

                if (!tags.Add(tag))
                {
                    report.AddError(
                        "SYNERGY_REQUIRED_TAG_DUPLICATE",
                        $"SynergyConfig repeats requiredTag: {tag}.",
                        path);
                }
            }
        }

        private static void ValidateThresholds(
            BuildSandboxValidationReport report,
            IReadOnlyList<SynergyThresholdConfig> thresholds,
            string path)
        {
            if (thresholds == null || thresholds.Count == 0)
            {
                report.AddError("SYNERGY_THRESHOLDS_MISSING", "SynergyConfig has no thresholds.", path);
                return;
            }

            HashSet<int> localThresholds = new();
            foreach (SynergyThresholdConfig threshold in thresholds)
            {
                if (!AllowedSynergyThresholds.Contains(threshold.pieceCount))
                {
                    report.AddError(
                        "SYNERGY_THRESHOLD_INVALID",
                        $"Threshold {threshold.pieceCount} must be one of 2 / 4 / 6 / 8.",
                        path);
                }

                if (!localThresholds.Add(threshold.pieceCount))
                {
                    report.AddError(
                        "SYNERGY_THRESHOLD_DUPLICATE",
                        $"Threshold {threshold.pieceCount} is repeated in one SynergyConfig.",
                        path);
                }

                if (string.IsNullOrWhiteSpace(threshold.effectToken))
                {
                    report.AddError(
                        "SYNERGY_THRESHOLD_EFFECT_TOKEN_MISSING",
                        $"Threshold {threshold.pieceCount} is missing effectToken.",
                        path);
                }

                if (string.IsNullOrWhiteSpace(threshold.effectSummary))
                {
                    report.AddError(
                        "SYNERGY_THRESHOLD_EFFECT_SUMMARY_MISSING",
                        $"Threshold {threshold.pieceCount} is missing effectSummary.",
                        path);
                }

                if (!threshold.devOnly)
                {
                    report.AddError("THRESHOLD_DEVONLY_FALSE", "SynergyThresholdConfig must keep devOnly=true.", path);
                }

                if (threshold.isEnabled)
                {
                    report.AddError("THRESHOLD_ENABLED_TRUE", "SynergyThresholdConfig must keep isEnabled=false.", path);
                }
            }
        }

        private static void ValidateConditionIsolation(
            BuildSandboxValidationReport report,
            IReadOnlyList<PlacementConditionConfig> conditions,
            string path,
            string codePrefix)
        {
            foreach (PlacementConditionConfig condition in conditions ?? Enumerable.Empty<PlacementConditionConfig>())
            {
                if (!condition.devOnly)
                {
                    report.AddError($"{codePrefix}_DEVONLY_FALSE", "PlacementConditionConfig must keep devOnly=true.", path);
                }

                if (condition.isEnabled)
                {
                    report.AddError($"{codePrefix}_ENABLED_TRUE", "PlacementConditionConfig must keep isEnabled=false.", path);
                }

                if (string.IsNullOrWhiteSpace(condition.conditionId))
                {
                    report.AddError($"{codePrefix}_CONDITION_ID_MISSING", "PlacementConditionConfig conditionId is empty.", path);
                }

                string conditionType = (condition.conditionType ?? string.Empty).Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(conditionType))
                {
                    report.AddError($"{codePrefix}_CONDITION_TYPE_MISSING", "PlacementConditionConfig conditionType is empty.", path);
                }
                else if (!SupportedPlacementConditionTypes.Contains(conditionType))
                {
                    report.AddError(
                        $"{codePrefix}_CONDITION_TYPE_UNSUPPORTED",
                        $"PlacementConditionConfig conditionType is unsupported: {condition.conditionType}.",
                        path);
                }

                if (condition.requiredCount < 0)
                {
                    report.AddError(
                        $"{codePrefix}_REQUIRED_COUNT_NEGATIVE",
                        "PlacementConditionConfig requiredCount cannot be negative.",
                        path);
                }
            }
        }

        private static void ValidateConditionIsolation(
            BuildSandboxValidationReport report,
            IReadOnlyList<EnergyConditionConfig> conditions,
            string path,
            string codePrefix)
        {
            foreach (EnergyConditionConfig condition in conditions ?? Enumerable.Empty<EnergyConditionConfig>())
            {
                if (!condition.devOnly)
                {
                    report.AddError($"{codePrefix}_DEVONLY_FALSE", "EnergyConditionConfig must keep devOnly=true.", path);
                }

                if (condition.isEnabled)
                {
                    report.AddError($"{codePrefix}_ENABLED_TRUE", "EnergyConditionConfig must keep isEnabled=false.", path);
                }

                if (string.IsNullOrWhiteSpace(condition.conditionId))
                {
                    report.AddError($"{codePrefix}_CONDITION_ID_MISSING", "EnergyConditionConfig conditionId is empty.", path);
                }

                if (condition.minimumEnergy < 0)
                {
                    report.AddError(
                        $"{codePrefix}_MINIMUM_NEGATIVE",
                        "EnergyConditionConfig minimumEnergy cannot be negative.",
                        path);
                }

                if (condition.maximumEnergy < 0)
                {
                    report.AddError(
                        $"{codePrefix}_MAXIMUM_NEGATIVE",
                        "EnergyConditionConfig maximumEnergy cannot be negative.",
                        path);
                }

                if (condition.maximumEnergy > 0 && condition.minimumEnergy > condition.maximumEnergy)
                {
                    report.AddError(
                        $"{codePrefix}_MINIMUM_EXCEEDS_MAXIMUM",
                        "EnergyConditionConfig minimumEnergy cannot exceed maximumEnergy.",
                        path);
                }
            }
        }

        private static void ValidateArchetypes(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildArchetypeConfig> archetypes,
            IReadOnlyList<SynergyConfig> synergies)
        {
            if (archetypes.Count == 0)
            {
                report.AddError(
                    "BUILD_ARCHETYPE_CONFIG_MISSING",
                    "SynergyDataFoundation01 requires at least one BuildArchetypeConfig asset.",
                    ConfigRoot);
                return;
            }

            HashSet<string> knownSynergyIds = new(
                synergies
                    .Where(synergy => !string.IsNullOrWhiteSpace(synergy.synergyId))
                    .Select(synergy => synergy.synergyId),
                StringComparer.Ordinal);

            foreach (BuildArchetypeConfig config in archetypes)
            {
                string path = AssetDatabase.GetAssetPath(config);
                if (string.IsNullOrWhiteSpace(config.archetypeId))
                {
                    report.AddError("ARCHETYPE_ID_MISSING", "BuildArchetypeConfig archetypeId is empty.", path);
                }

                foreach (string synergyId in config.targetSynergyIds ?? Enumerable.Empty<string>())
                {
                    if (!knownSynergyIds.Contains(synergyId))
                    {
                        report.AddWarning(
                            "ARCHETYPE_UNKNOWN_SYNERGY",
                            $"BuildArchetypeConfig references a synergy id that is not in this seed set: {synergyId}.",
                            path);
                    }
                }

                ValidateConditionIsolation(report, config.placementConditions, path, "ARCHETYPE_PLACEMENT");
                ValidateConditionIsolation(report, config.energyConditions, path, "ARCHETYPE_ENERGY");
            }
        }

        private static void ValidateItemShapeConfigs(BuildSandboxValidationReport report)
        {
            IReadOnlyList<ItemShapeConfig> shapes = CollectItemShapeConfigs();
            if (shapes.Count == 0)
            {
                report.AddError(
                    "ITEM_SHAPE_CONFIG_MISSING",
                    "ItemShapeOccupancy01 requires first-batch ItemShapeConfig assets.",
                    ConfigRoot);
                return;
            }

            Dictionary<string, string> shapeIds = new(StringComparer.Ordinal);
            foreach (ItemShapeConfig shape in shapes)
            {
                string path = AssetDatabase.GetAssetPath(shape);
                if (string.IsNullOrWhiteSpace(shape.shapeId))
                {
                    report.AddError("ITEM_SHAPE_ID_MISSING", "ItemShapeConfig shapeId is empty.", path);
                }
                else if (shapeIds.TryGetValue(shape.shapeId, out string existingPath))
                {
                    report.AddError(
                        "ITEM_SHAPE_ID_DUPLICATE",
                        $"ItemShapeConfig shapeId '{shape.shapeId}' is already used by {existingPath}.",
                        path);
                }
                else
                {
                    shapeIds.Add(shape.shapeId, path);
                }

                if (!shape.devOnly)
                {
                    report.AddError("ITEM_SHAPE_DEVONLY_FALSE", "ItemShapeConfig must keep devOnly=true.", path);
                }

                if (shape.isEnabled)
                {
                    report.AddError("ITEM_SHAPE_ENABLED_TRUE", "ItemShapeConfig must keep isEnabled=false.", path);
                }

                ValidateItemShapeOffsets(report, shape, path);
            }

            RequireItemShape(report, shapeIds, "Single1");
            RequireItemShape(report, shapeIds, "Vertical2");
            RequireItemShape(report, shapeIds, "Corner3");
            RequireItemShape(report, shapeIds, "Square4");

            report.AddInfo(
                "ITEM_SHAPE_CONFIG_COUNTS",
                $"ItemShapeConfig={shapes.Count}.",
                ConfigRoot);
        }

        private static void ValidateEnemyBossValidationPool(BuildSandboxValidationReport report)
        {
            EnemyBossValidationPool pool = EnemyBossValidationPool.CreateDefault();
            if (pool.enemies == null || pool.enemies.Count < 11)
            {
                report.AddError(
                    "CONFIG_ENEMY_PROFILE_COUNT_LOW",
                    $"Enemy/Boss validation pool requires at least 11 enemy profiles; actual={pool.enemies?.Count ?? 0}.",
                    nameof(EnemyBossValidationPool));
            }

            if (pool.bosses == null || pool.bosses.Count < 7)
            {
                report.AddError(
                    "CONFIG_BOSS_PROFILE_COUNT_LOW",
                    $"Enemy/Boss validation pool requires at least 7 boss profiles; actual={pool.bosses?.Count ?? 0}.",
                    nameof(EnemyBossValidationPool));
            }

            ValidateEnemyBossProfileIsolation(
                report,
                pool.enemies,
                profile => profile.enemyId,
                profile => profile.chineseRole,
                profile => profile.validationTargetBuilds,
                profile => profile.validationTags,
                profile => profile.recommendedSynergies,
                profile => profile.devOnly,
                profile => profile.isEnabled,
                profile => profile.entersFormalFlow,
                profile => profile.simulatorReadable,
                "CONFIG_ENEMY");

            ValidateEnemyBossProfileIsolation(
                report,
                pool.bosses,
                profile => profile.bossId,
                profile => profile.chineseRole,
                profile => profile.validationTargetBuilds,
                profile => profile.validationTags,
                profile => profile.recommendedSynergies,
                profile => profile.devOnly,
                profile => profile.isEnabled,
                profile => profile.entersFormalFlow,
                profile => profile.simulatorReadable,
                "CONFIG_BOSS");
        }

        private static void ValidateDevChapterContentPool(BuildSandboxValidationReport report)
        {
            DevChapterContentPoolValidator.ValidatePool(
                report,
                DevChapterContentPool.CreateDefault(),
                EnemyBossValidationPool.CreateDefault());
        }

        private static void ValidateLedgerTaskBuildHooks(BuildSandboxValidationReport report)
        {
            LedgerTaskBuildHooksValidator.ValidateInto(report);
        }

        private static void ValidateEnemyBossProfileIsolation<T>(
            BuildSandboxValidationReport report,
            IReadOnlyList<T> profiles,
            Func<T, string> getId,
            Func<T, string> getChineseRole,
            Func<T, IReadOnlyList<string>> getTargetBuilds,
            Func<T, IReadOnlyList<string>> getTags,
            Func<T, IReadOnlyList<string>> getSynergies,
            Func<T, bool> getDevOnly,
            Func<T, bool> getIsEnabled,
            Func<T, bool> getEntersFormalFlow,
            Func<T, bool> getSimulatorReadable,
            string codePrefix)
        {
            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (T profile in profiles ?? Array.Empty<T>())
            {
                string id = getId(profile) ?? string.Empty;
                string path = $"{typeof(T).Name}:{id}";
                if (string.IsNullOrWhiteSpace(id))
                {
                    report.AddError($"{codePrefix}_ID_MISSING", "Enemy/Boss profile id is empty.", path);
                }
                else if (!ids.Add(id))
                {
                    report.AddError($"{codePrefix}_ID_DUPLICATE", $"Enemy/Boss profile id is duplicated: {id}.", path);
                }

                if (string.IsNullOrWhiteSpace(getChineseRole(profile)))
                {
                    report.AddError($"{codePrefix}_CHINESE_ROLE_MISSING", "Enemy/Boss profile Chinese positioning is empty.", path);
                }

                if (getTargetBuilds(profile) == null || getTargetBuilds(profile).Count == 0)
                {
                    report.AddError($"{codePrefix}_TARGET_BUILD_MISSING", "Enemy/Boss profile target Build list is empty.", path);
                }

                if (getTags(profile) == null || getTags(profile).Count == 0)
                {
                    report.AddError($"{codePrefix}_TAG_MISSING", "Enemy/Boss profile validation tags are empty.", path);
                }

                if (getSynergies(profile) == null || getSynergies(profile).Count == 0)
                {
                    report.AddError($"{codePrefix}_SYNERGY_MISSING", "Enemy/Boss profile recommended synergies are empty.", path);
                }

                if (!getDevOnly(profile))
                {
                    report.AddError($"{codePrefix}_DEVONLY_FALSE", "Enemy/Boss profile must keep devOnly=true.", path);
                }

                if (getIsEnabled(profile))
                {
                    report.AddError($"{codePrefix}_ENABLED_TRUE", "Enemy/Boss profile must keep isEnabled=false.", path);
                }

                if (getEntersFormalFlow(profile))
                {
                    report.AddError($"{codePrefix}_FORMAL_FLOW_LEAK", "Enemy/Boss profile must not enter formal flow.", path);
                }

                if (!getSimulatorReadable(profile))
                {
                    report.AddError($"{codePrefix}_SIMULATOR_NOT_READABLE", "Enemy/Boss profile must be readable by the sandbox simulator.", path);
                }
            }

            report.AddInfo(
                $"{codePrefix}_PROFILE_CONFIG_CHECKED",
                $"Checked {profiles?.Count ?? 0} Enemy/Boss validation profile(s).",
                nameof(EnemyBossValidationPool));
        }

        private static void ValidateItemShapeOffsets(
            BuildSandboxValidationReport report,
            ItemShapeConfig shape,
            string path)
        {
            if (shape.occupiedOffsets == null || shape.occupiedOffsets.Count == 0)
            {
                report.AddError("ITEM_SHAPE_OFFSETS_EMPTY", "ItemShapeConfig occupiedOffsets is empty.", path);
                return;
            }

            if (shape.cellCount != shape.occupiedOffsets.Count)
            {
                report.AddError(
                    "ITEM_SHAPE_CELL_COUNT_MISMATCH",
                    $"cellCount={shape.cellCount} must equal occupiedOffsets count={shape.occupiedOffsets.Count}.",
                    path);
            }

            HashSet<ItemShapeCell> offsets = new();
            foreach (ItemShapeCell offset in shape.occupiedOffsets)
            {
                if (offset.x < 0 || offset.y < 0)
                {
                    report.AddError(
                        "ITEM_SHAPE_OFFSET_INVALID",
                        $"Offset {offset} must be non-negative.",
                        path);
                }

                if (!offsets.Add(offset))
                {
                    report.AddError(
                        "ITEM_SHAPE_OFFSET_DUPLICATE",
                        $"Offset {offset} is repeated.",
                        path);
                }
            }
        }

        private static void RequireItemShape(
            BuildSandboxValidationReport report,
            IReadOnlyDictionary<string, string> shapeIds,
            string shapeId)
        {
            if (shapeIds.ContainsKey(shapeId))
            {
                report.AddInfo(
                    "ITEM_SHAPE_REQUIRED_PRESENT",
                    $"Required ItemShapeConfig is present: {shapeId}.",
                    shapeIds[shapeId]);
                return;
            }

            report.AddError(
                "ITEM_SHAPE_REQUIRED_MISSING",
                $"Required ItemShapeConfig is missing: {shapeId}.",
                ConfigRoot);
        }
    }
}
#endif
