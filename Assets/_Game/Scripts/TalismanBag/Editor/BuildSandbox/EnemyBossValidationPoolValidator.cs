#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class EnemyBossValidationPoolValidator
    {
        private static readonly string[] RequiredEnemyRoles =
        {
            "普通怪",
            "护盾怪",
            "毒怪",
            "燃烧怪",
            "偷灵怪",
            "封符怪",
            "群怪",
            "高爆发怪",
            "施法怪",
            "厚血怪",
            "阵眼干扰怪"
        };

        private static readonly string[] RequiredBossRoles =
        {
            "护盾Boss",
            "群怪Boss",
            "高爆发Boss",
            "负面状态Boss",
            "施法Boss",
            "供能干扰Boss",
            "混合机制Boss"
        };

        private static readonly string[] SourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/EnemyBossValidationPool.cs",
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildSimulationBenchmark.cs"
        };

        private static readonly string[] ForbiddenFormalReferenceTokens =
        {
            "V02RunFlowController",
            "V02FormationGridFrame",
            "DamageText",
            "PageState",
            "FormationState",
            "MainTrialProgressData",
            "PlayerPrefs",
            "SaveData",
            "RewardConfig",
            "UpgradeConfig",
            "BossReward",
            "FormalDrop",
            "FormalForge"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Enemy/Boss Validation Pool");
            EnemyBossValidationPool pool = EnemyBossValidationPool.CreateDefault();
            ValidatePool(report, pool);
            ValidateFeatureFlags(report);
            ValidateSimulatorRead(report, BuildPoolSimulationBenchmark());
            ValidateSourceIsolation(report);
            return report;
        }

        public static BuildSimulationBenchmarkReport BuildPoolSimulationBenchmark()
        {
            EnemyBossValidationPool pool = EnemyBossValidationPool.CreateDefault();
            ModifierEventBridgeValidator.BuildSamplePreview(
                out BuildEvaluationResult buildEvaluation,
                out AffixRarityEvaluationResult affixEvaluation,
                out CombatModifierBundle modifierBundle,
                out EffectEventBundle eventBundle);

            BuildSandboxLayoutSnapshot snapshot = SynergyEvaluatorCoreValidator.BuildSeedSnapshot(
                BuildSandboxConfigValidator.CollectSynergyConfigs());
            List<BuildSimulationScenario> scenarios = new();
            foreach (BuildSandboxEnemyProfile enemy in pool.enemies)
            {
                scenarios.Add(CreateEnemyScenario(
                    enemy,
                    snapshot,
                    buildEvaluation,
                    affixEvaluation,
                    modifierBundle,
                    eventBundle));
            }

            foreach (BuildSandboxBossProfile boss in pool.bosses)
            {
                scenarios.Add(CreateBossScenario(
                    boss,
                    snapshot,
                    buildEvaluation,
                    affixEvaluation,
                    modifierBundle,
                    eventBundle));
            }

            return BuildSimulationRunner.RunBatch(scenarios);
        }

        public static void ValidatePool(BuildSandboxValidationReport report, EnemyBossValidationPool pool)
        {
            if (pool == null)
            {
                report.AddError("ENEMY_BOSS_POOL_NULL", "EnemyBossValidationPool is missing.", nameof(EnemyBossValidationPool));
                return;
            }

            if (!pool.devOnly)
            {
                report.AddError("ENEMY_BOSS_POOL_DEVONLY_FALSE", "EnemyBossValidationPool must keep devOnly=true.", nameof(EnemyBossValidationPool));
            }

            if (pool.isEnabled)
            {
                report.AddError("ENEMY_BOSS_POOL_ENABLED_TRUE", "EnemyBossValidationPool must keep isEnabled=false.", nameof(EnemyBossValidationPool));
            }

            ValidateEnemies(report, pool.enemies);
            ValidateBosses(report, pool.bosses);
        }

        private static void ValidateEnemies(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildSandboxEnemyProfile> enemies)
        {
            if (enemies == null || enemies.Count < 11)
            {
                report.AddError(
                    "ENEMY_PROFILE_COUNT_LOW",
                    $"EnemyBossValidationPool01 requires at least 11 enemy profiles; actual={enemies?.Count ?? 0}.",
                    nameof(BuildSandboxEnemyProfile));
                return;
            }

            Dictionary<string, string> ids = new(StringComparer.Ordinal);
            HashSet<string> roles = new(StringComparer.Ordinal);
            foreach (BuildSandboxEnemyProfile enemy in enemies)
            {
                string path = $"{nameof(BuildSandboxEnemyProfile)}:{enemy?.enemyId}";
                if (enemy == null)
                {
                    report.AddError("ENEMY_PROFILE_NULL", "Enemy profile is null.", nameof(BuildSandboxEnemyProfile));
                    continue;
                }

                ValidateSharedProfile(
                    report,
                    path,
                    enemy.enemyId,
                    enemy.chineseRole,
                    enemy.validationTargetBuilds,
                    enemy.validationTags,
                    enemy.recommendedSynergies,
                    enemy.devOnly,
                    enemy.isEnabled,
                    enemy.simulatorReadable,
                    enemy.entersFormalFlow,
                    enemy.referencesFormalEnemyPool,
                    enemy.referencesFormalBossPool);

                if (!string.IsNullOrWhiteSpace(enemy.enemyId) && ids.TryGetValue(enemy.enemyId, out string existing))
                {
                    report.AddError("ENEMY_PROFILE_ID_DUPLICATE", $"Duplicate enemyId {enemy.enemyId}; first={existing}.", path);
                }
                else if (!string.IsNullOrWhiteSpace(enemy.enemyId))
                {
                    ids.Add(enemy.enemyId, path);
                }

                if (!string.IsNullOrWhiteSpace(enemy.chineseRole))
                {
                    roles.Add(enemy.chineseRole);
                }

                report.AddInfo(
                    "ENEMY_PROFILE_SCANNED",
                    $"{enemy.enemyId} ({enemy.chineseRole}) devOnly={enemy.devOnly}, isEnabled={enemy.isEnabled}, simulatorReadable={enemy.simulatorReadable}.",
                    path);
            }

            foreach (string role in RequiredEnemyRoles)
            {
                if (roles.Contains(role))
                {
                    report.AddInfo("REQUIRED_ENEMY_ROLE_PRESENT", $"Enemy role present: {role}.", nameof(BuildSandboxEnemyProfile));
                    continue;
                }

                report.AddError("REQUIRED_ENEMY_ROLE_MISSING", $"Enemy role missing: {role}.", nameof(BuildSandboxEnemyProfile));
            }
        }

        private static void ValidateBosses(
            BuildSandboxValidationReport report,
            IReadOnlyList<BuildSandboxBossProfile> bosses)
        {
            if (bosses == null || bosses.Count < 7)
            {
                report.AddError(
                    "BOSS_PROFILE_COUNT_LOW",
                    $"EnemyBossValidationPool01 requires at least 7 boss profiles; actual={bosses?.Count ?? 0}.",
                    nameof(BuildSandboxBossProfile));
                return;
            }

            Dictionary<string, string> ids = new(StringComparer.Ordinal);
            foreach (BuildSandboxBossProfile boss in bosses)
            {
                string path = $"{nameof(BuildSandboxBossProfile)}:{boss?.bossId}";
                if (boss == null)
                {
                    report.AddError("BOSS_PROFILE_NULL", "Boss profile is null.", nameof(BuildSandboxBossProfile));
                    continue;
                }

                ValidateSharedProfile(
                    report,
                    path,
                    boss.bossId,
                    boss.chineseRole,
                    boss.validationTargetBuilds,
                    boss.validationTags,
                    boss.recommendedSynergies,
                    boss.devOnly,
                    boss.isEnabled,
                    boss.simulatorReadable,
                    boss.entersFormalFlow,
                    boss.referencesFormalEnemyPool,
                    boss.referencesFormalBossPool);

                if (!string.IsNullOrWhiteSpace(boss.bossId) && ids.TryGetValue(boss.bossId, out string existing))
                {
                    report.AddError("BOSS_PROFILE_ID_DUPLICATE", $"Duplicate bossId {boss.bossId}; first={existing}.", path);
                }
                else if (!string.IsNullOrWhiteSpace(boss.bossId))
                {
                    ids.Add(boss.bossId, path);
                }

                report.AddInfo(
                    "BOSS_PROFILE_SCANNED",
                    $"{boss.bossId} ({boss.chineseRole}) targets={FormatStrings(boss.validationTargetBuilds)}.",
                    path);
            }

            foreach (string rolePrefix in RequiredBossRoles)
            {
                bool present = bosses.Any(boss =>
                    !string.IsNullOrWhiteSpace(boss?.chineseRole)
                    && boss.chineseRole.StartsWith(rolePrefix, StringComparison.Ordinal));
                if (present)
                {
                    report.AddInfo("REQUIRED_BOSS_ROLE_PRESENT", $"Boss role present: {rolePrefix}.", nameof(BuildSandboxBossProfile));
                    continue;
                }

                report.AddError("REQUIRED_BOSS_ROLE_MISSING", $"Boss role missing: {rolePrefix}.", nameof(BuildSandboxBossProfile));
            }
        }

        private static void ValidateSharedProfile(
            BuildSandboxValidationReport report,
            string path,
            string id,
            string chineseRole,
            IReadOnlyList<string> targetBuilds,
            IReadOnlyList<string> tags,
            IReadOnlyList<string> recommendedSynergies,
            bool devOnly,
            bool isEnabled,
            bool simulatorReadable,
            bool entersFormalFlow,
            bool referencesFormalEnemyPool,
            bool referencesFormalBossPool)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                report.AddError("PROFILE_ID_MISSING", "Profile id is empty.", path);
            }

            if (string.IsNullOrWhiteSpace(chineseRole))
            {
                report.AddError("PROFILE_CHINESE_ROLE_MISSING", "Profile Chinese positioning is empty.", path);
            }

            if (targetBuilds == null || targetBuilds.Count == 0)
            {
                report.AddError("PROFILE_TARGET_BUILD_MISSING", "Profile must declare validation target Build.", path);
            }

            if (tags == null || tags.Count == 0)
            {
                report.AddError("PROFILE_VALIDATION_TAGS_MISSING", "Profile must declare validation tags.", path);
            }

            if (recommendedSynergies == null || recommendedSynergies.Count == 0)
            {
                report.AddError("PROFILE_RECOMMENDED_SYNERGY_MISSING", "Profile must declare recommended synergies.", path);
            }

            if (!devOnly)
            {
                report.AddError("PROFILE_DEVONLY_FALSE", "Profile must keep devOnly=true.", path);
            }

            if (isEnabled)
            {
                report.AddError("PROFILE_ENABLED_TRUE", "Profile must keep isEnabled=false.", path);
            }

            if (!simulatorReadable)
            {
                report.AddError("PROFILE_SIMULATOR_NOT_READABLE", "Profile must be readable by BuildSimulationRunner.", path);
            }

            if (entersFormalFlow)
            {
                report.AddError("PROFILE_FORMAL_FLOW_LEAK", "Profile must not enter formal flow.", path);
            }

            if (referencesFormalEnemyPool || referencesFormalBossPool)
            {
                report.AddError("PROFILE_FORMAL_POOL_REFERENCE", "Profile must not reference formal enemy/Boss pools.", path);
            }
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError(
                        "ENEMY_BOSS_FEATURE_FLAG_TRUE",
                        $"{flag.Key} must stay false for EnemyBossValidationPool01.",
                        nameof(BuildSandboxFeatureFlags));
                    continue;
                }

                report.AddInfo(
                    "ENEMY_BOSS_FEATURE_FLAG_FALSE",
                    $"{flag.Key}=false.",
                    nameof(BuildSandboxFeatureFlags));
            }
        }

        private static void ValidateSimulatorRead(
            BuildSandboxValidationReport report,
            BuildSimulationBenchmarkReport benchmark)
        {
            if (benchmark == null || benchmark.results == null || benchmark.results.Count == 0)
            {
                report.AddError("ENEMY_BOSS_SIMULATION_EMPTY", "BuildSimulationRunner did not read pool profiles.", nameof(BuildSimulationRunner));
                return;
            }

            foreach (BuildSimulationResult result in benchmark.results)
            {
                string path = $"{nameof(BuildSimulationResult)}:{result.buildId}";
                if (!result.devOnly || result.isEnabled)
                {
                    report.AddError("ENEMY_BOSS_SIMULATION_ISOLATION_FAIL", "Simulation result must stay devOnly=true and isEnabled=false.", path);
                }

                if (!result.simulatorReadable)
                {
                    report.AddError("ENEMY_BOSS_SIMULATION_NOT_READABLE", "Simulation result says source profile is not simulator readable.", path);
                }

                if (result.entersFormalFlow || result.affectsFormalCombat)
                {
                    report.AddError("ENEMY_BOSS_SIMULATION_FORMAL_LEAK", "Simulation result must not enter formal flow or affect formal combat.", path);
                }
            }

            report.AddInfo(
                "ENEMY_BOSS_SIMULATION_READABLE",
                $"BuildSimulationRunner read {benchmark.results.Count} enemy/Boss validation profile scenario(s).",
                nameof(BuildSimulationRunner));
        }

        private static void ValidateSourceIsolation(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetCurrentDirectory();
            foreach (string relativePath in SourceFiles)
            {
                string path = Path.Combine(projectRoot, relativePath);
                if (!File.Exists(path))
                {
                    report.AddError("ENEMY_BOSS_SOURCE_FILE_MISSING", $"Source file missing: {relativePath}.", relativePath);
                    continue;
                }

                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenFormalReferenceTokens)
                {
                    if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "ENEMY_BOSS_FORBIDDEN_FORMAL_TOKEN",
                            $"Source references forbidden formal-system token: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo(
                "ENEMY_BOSS_SOURCE_ISOLATION_SCANNED",
                "Enemy/Boss validation pool source scan completed.",
                nameof(EnemyBossValidationPool));
        }

        private static BuildSimulationScenario CreateEnemyScenario(
            BuildSandboxEnemyProfile enemy,
            BuildSandboxLayoutSnapshot snapshot,
            BuildEvaluationResult buildEvaluation,
            AffixRarityEvaluationResult affixEvaluation,
            CombatModifierBundle modifierBundle,
            EffectEventBundle eventBundle)
        {
            return CreateScenario(
                $"pool_enemy_{enemy.enemyId}",
                enemy.chineseRole,
                enemy.enemyType,
                "none",
                enemy.enemyId,
                enemy.chineseRole,
                string.Empty,
                string.Empty,
                enemy.validationTargetBuilds,
                enemy.validationTags,
                enemy.recommendedSynergies,
                enemy.simulatorReadable,
                enemy.entersFormalFlow,
                snapshot,
                buildEvaluation,
                affixEvaluation,
                modifierBundle,
                eventBundle);
        }

        private static BuildSimulationScenario CreateBossScenario(
            BuildSandboxBossProfile boss,
            BuildSandboxLayoutSnapshot snapshot,
            BuildEvaluationResult buildEvaluation,
            AffixRarityEvaluationResult affixEvaluation,
            CombatModifierBundle modifierBundle,
            EffectEventBundle eventBundle)
        {
            return CreateScenario(
                $"pool_boss_{boss.bossId}",
                boss.chineseRole,
                "dev_boss_validation_shell",
                boss.bossMechanic,
                string.Empty,
                string.Empty,
                boss.bossId,
                boss.chineseRole,
                boss.validationTargetBuilds,
                boss.validationTags,
                boss.recommendedSynergies,
                boss.simulatorReadable,
                boss.entersFormalFlow,
                snapshot,
                buildEvaluation,
                affixEvaluation,
                modifierBundle,
                eventBundle);
        }

        private static BuildSimulationScenario CreateScenario(
            string buildId,
            string displayName,
            string enemyType,
            string bossMechanic,
            string enemyProfileId,
            string enemyChineseRole,
            string bossProfileId,
            string bossChineseRole,
            IReadOnlyList<string> targetBuilds,
            IReadOnlyList<string> tags,
            IReadOnlyList<string> synergies,
            bool simulatorReadable,
            bool entersFormalFlow,
            BuildSandboxLayoutSnapshot snapshot,
            BuildEvaluationResult buildEvaluation,
            AffixRarityEvaluationResult affixEvaluation,
            CombatModifierBundle modifierBundle,
            EffectEventBundle eventBundle)
        {
            List<string> itemIds = snapshot?.placedItems?
                .Where(item => item != null && !string.IsNullOrWhiteSpace(item.itemId))
                .Select(item => item.itemId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .ToList() ?? new List<string>();
            if (itemIds.Count == 0)
            {
                itemIds.Add("buildsandbox_pool_seed");
            }

            return new BuildSimulationScenario
            {
                buildId = buildId,
                displayName = displayName,
                enemyType = enemyType,
                bossMechanic = bossMechanic,
                enemyProfileId = enemyProfileId,
                enemyChineseRole = enemyChineseRole,
                bossProfileId = bossProfileId,
                bossChineseRole = bossChineseRole,
                validationTags = tags?.ToList() ?? new List<string>(),
                recommendedSynergies = synergies?.ToList() ?? new List<string>(),
                buildItemIds = itemIds,
                itemRarity = "mixed_preview",
                affixCombination = affixEvaluation?.affixIds?.ToList() ?? new List<string>(),
                energyCondition = "powered_sample",
                placementRelation = FormatStrings(targetBuilds),
                simulatorReadable = simulatorReadable,
                entersFormalFlow = entersFormalFlow,
                devOnly = true,
                isEnabled = false,
                layoutSnapshot = snapshot ?? new BuildSandboxLayoutSnapshot(),
                buildEvaluation = buildEvaluation ?? new BuildEvaluationResult(),
                affixEvaluation = affixEvaluation ?? new AffixRarityEvaluationResult(),
                modifierBundle = modifierBundle ?? new CombatModifierBundle(),
                eventBundle = eventBundle ?? new EffectEventBundle(),
                notes = "sandbox estimate; profile read from EnemyBossValidationPool; no formal flow."
            };
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }
}
#endif
