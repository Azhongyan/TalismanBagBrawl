#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class DevChapterContentPoolValidator
    {
        private const int RequiredProfileCount = 65;

        private static readonly (string Prefix, int Count)[] RequiredSeries =
        {
            ("DevBuildTest_Thunder", 10),
            ("DevBuildTest_Fire", 10),
            ("DevBuildTest_Shield", 10),
            ("DevBuildTest_Cleanse", 10),
            ("DevBuildTest_Control", 10),
            ("DevBuildTest_Energy", 10),
            ("DevBossTest_Mixed", 5)
        };

        private static readonly string[] SourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/DevChapterContentPool.cs"
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
            "FormalForge",
            "StageConfig",
            "RunFlow",
            "MainTrial"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Dev Chapter Content Pool");
            DevChapterContentPool pool = DevChapterContentPool.CreateDefault();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            ValidatePool(report, pool, enemyBossPool);
            ValidateFeatureFlags(report);
            ValidateSimulatorRead(report, pool, BuildDevChapterSimulationBenchmark());
            ValidateSourceIsolation(report);
            return report;
        }

        public static BuildSimulationBenchmarkReport BuildDevChapterSimulationBenchmark()
        {
            DevChapterContentPool chapterPool = DevChapterContentPool.CreateDefault();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            ModifierEventBridgeValidator.BuildSamplePreview(
                out BuildEvaluationResult buildEvaluation,
                out AffixRarityEvaluationResult affixEvaluation,
                out CombatModifierBundle modifierBundle,
                out EffectEventBundle eventBundle);

            BuildSandboxLayoutSnapshot snapshot = SynergyEvaluatorCoreValidator.BuildSeedSnapshot(
                BuildSandboxConfigValidator.CollectSynergyConfigs());
            IReadOnlyList<BuildSimulationScenario> scenarios = chapterPool.CreateSimulationScenarios(
                enemyBossPool,
                snapshot,
                buildEvaluation,
                affixEvaluation,
                modifierBundle,
                eventBundle);
            return BuildSimulationRunner.RunBatch(scenarios);
        }

        public static void ValidatePool(
            BuildSandboxValidationReport report,
            DevChapterContentPool pool,
            EnemyBossValidationPool enemyBossPool)
        {
            if (pool == null)
            {
                report.AddError("DEV_CHAPTER_POOL_NULL", "DevChapterContentPool is missing.", nameof(DevChapterContentPool));
                return;
            }

            if (!pool.devOnly)
            {
                report.AddError("DEV_CHAPTER_POOL_DEVONLY_FALSE", "DevChapterContentPool must keep devOnly=true.", nameof(DevChapterContentPool));
            }

            if (pool.isEnabled)
            {
                report.AddError("DEV_CHAPTER_POOL_ENABLED_TRUE", "DevChapterContentPool must keep isEnabled=false.", nameof(DevChapterContentPool));
            }

            IReadOnlyList<DevChapterProfile> chapters = pool.chapters != null
                ? (IReadOnlyList<DevChapterProfile>)pool.chapters
                : Array.Empty<DevChapterProfile>();
            if (chapters.Count < RequiredProfileCount)
            {
                report.AddError(
                    "DEV_CHAPTER_PROFILE_COUNT_LOW",
                    $"DevChapterContentPool01 requires at least {RequiredProfileCount} profiles; actual={chapters.Count}.",
                    nameof(DevChapterProfile));
            }

            ValidateRequiredSeries(report, chapters);
            ValidateProfiles(report, chapters, enemyBossPool);
        }

        private static void ValidateRequiredSeries(
            BuildSandboxValidationReport report,
            IReadOnlyList<DevChapterProfile> chapters)
        {
            HashSet<string> ids = new(
                chapters
                    .Where(chapter => chapter != null && !string.IsNullOrWhiteSpace(chapter.chapterId))
                    .Select(chapter => chapter.chapterId),
                StringComparer.Ordinal);

            foreach ((string prefix, int count) in RequiredSeries)
            {
                for (int index = 1; index <= count; index++)
                {
                    string id = $"{prefix}_{index:00}";
                    if (ids.Contains(id))
                    {
                        report.AddInfo("DEV_CHAPTER_REQUIRED_PRESENT", $"Required profile present: {id}.", nameof(DevChapterProfile));
                        continue;
                    }

                    report.AddError("DEV_CHAPTER_REQUIRED_MISSING", $"Required profile missing: {id}.", nameof(DevChapterProfile));
                }
            }
        }

        private static void ValidateProfiles(
            BuildSandboxValidationReport report,
            IReadOnlyList<DevChapterProfile> chapters,
            EnemyBossValidationPool enemyBossPool)
        {
            Dictionary<string, string> ids = new(StringComparer.Ordinal);
            foreach (DevChapterProfile chapter in chapters)
            {
                string path = $"{nameof(DevChapterProfile)}:{chapter?.chapterId}";
                if (chapter == null)
                {
                    report.AddError("DEV_CHAPTER_PROFILE_NULL", "Chapter profile is null.", nameof(DevChapterProfile));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(chapter.chapterId))
                {
                    report.AddError("DEV_CHAPTER_ID_MISSING", "Chapter profile id is empty.", path);
                }
                else if (ids.TryGetValue(chapter.chapterId, out string existingPath))
                {
                    report.AddError("DEV_CHAPTER_ID_DUPLICATE", $"Duplicate chapterId {chapter.chapterId}; first={existingPath}.", path);
                }
                else
                {
                    ids.Add(chapter.chapterId, path);
                }

                if (string.IsNullOrWhiteSpace(chapter.chineseRole))
                {
                    report.AddError("DEV_CHAPTER_CHINESE_ROLE_MISSING", "Chapter Chinese positioning is empty.", path);
                }

                if (chapter.validationTargetBuilds == null || chapter.validationTargetBuilds.Count == 0)
                {
                    report.AddError("DEV_CHAPTER_TARGET_BUILD_MISSING", "Chapter must declare a Build validation target.", path);
                }

                if (string.IsNullOrWhiteSpace(chapter.enemyId) && string.IsNullOrWhiteSpace(chapter.bossId))
                {
                    report.AddError("DEV_CHAPTER_COMBAT_PROFILE_MISSING", "Chapter must reference a devOnly enemy or Boss profile.", path);
                }

                ValidateEnemyReference(report, chapter, enemyBossPool, path);
                ValidateBossReference(report, chapter, enemyBossPool, path);
                ValidateIsolationFlags(report, chapter, path);

                report.AddInfo(
                    "DEV_CHAPTER_PROFILE_SCANNED",
                    $"{chapter.chapterId} ({chapter.chineseRole}) devOnly={chapter.devOnly}, isEnabled={chapter.isEnabled}, simulatorReadable={chapter.simulatorReadable}.",
                    path);
            }
        }

        private static void ValidateEnemyReference(
            BuildSandboxValidationReport report,
            DevChapterProfile chapter,
            EnemyBossValidationPool enemyBossPool,
            string path)
        {
            if (string.IsNullOrWhiteSpace(chapter.enemyId))
            {
                return;
            }

            BuildSandboxEnemyProfile enemy = enemyBossPool.FindEnemy(chapter.enemyId);
            if (enemy == null)
            {
                report.AddError("DEV_CHAPTER_ENEMY_UNKNOWN", $"Chapter references unknown dev enemy profile: {chapter.enemyId}.", path);
                return;
            }

            if (!enemy.devOnly || enemy.isEnabled || enemy.entersFormalFlow)
            {
                report.AddError("DEV_CHAPTER_ENEMY_ISOLATION_FAIL", $"Referenced enemy is not isolated: {chapter.enemyId}.", path);
            }

            if (!enemy.simulatorReadable)
            {
                report.AddError("DEV_CHAPTER_ENEMY_NOT_READABLE", $"Referenced enemy is not simulator readable: {chapter.enemyId}.", path);
            }
        }

        private static void ValidateBossReference(
            BuildSandboxValidationReport report,
            DevChapterProfile chapter,
            EnemyBossValidationPool enemyBossPool,
            string path)
        {
            if (string.IsNullOrWhiteSpace(chapter.bossId))
            {
                return;
            }

            BuildSandboxBossProfile boss = enemyBossPool.FindBoss(chapter.bossId);
            if (boss == null)
            {
                report.AddError("DEV_CHAPTER_BOSS_UNKNOWN", $"Chapter references unknown dev Boss profile: {chapter.bossId}.", path);
                return;
            }

            if (!boss.devOnly || boss.isEnabled || boss.entersFormalFlow)
            {
                report.AddError("DEV_CHAPTER_BOSS_ISOLATION_FAIL", $"Referenced Boss is not isolated: {chapter.bossId}.", path);
            }

            if (!boss.simulatorReadable)
            {
                report.AddError("DEV_CHAPTER_BOSS_NOT_READABLE", $"Referenced Boss is not simulator readable: {chapter.bossId}.", path);
            }
        }

        private static void ValidateIsolationFlags(
            BuildSandboxValidationReport report,
            DevChapterProfile chapter,
            string path)
        {
            if (!chapter.devOnly)
            {
                report.AddError("DEV_CHAPTER_DEVONLY_FALSE", "Chapter profile must keep devOnly=true.", path);
            }

            if (chapter.isEnabled)
            {
                report.AddError("DEV_CHAPTER_ENABLED_TRUE", "Chapter profile must keep isEnabled=false.", path);
            }

            if (!chapter.simulatorReadable)
            {
                report.AddError("DEV_CHAPTER_SIMULATOR_NOT_READABLE", "Chapter profile must be readable by BuildSimulationRunner.", path);
            }

            if (chapter.entersFormalFlow
                || chapter.usesFormalStageData
                || chapter.usesFormalFlowHook
                || chapter.appearsInFormalEntrance
                || chapter.usesProductReward
                || chapter.usesProductDrop)
            {
                report.AddError("DEV_CHAPTER_FORMAL_LEAK", "Chapter profile must not touch product flow, product entry, reward, drop, or formal stage data.", path);
            }
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError(
                        "DEV_CHAPTER_FEATURE_FLAG_TRUE",
                        $"{flag.Key} must stay false for DevChapterContentPool01.",
                        nameof(BuildSandboxFeatureFlags));
                    continue;
                }

                report.AddInfo("DEV_CHAPTER_FEATURE_FLAG_FALSE", $"{flag.Key}=false.", nameof(BuildSandboxFeatureFlags));
            }
        }

        private static void ValidateSimulatorRead(
            BuildSandboxValidationReport report,
            DevChapterContentPool pool,
            BuildSimulationBenchmarkReport benchmark)
        {
            IReadOnlyList<DevChapterProfile> chapters = pool?.chapters != null
                ? (IReadOnlyList<DevChapterProfile>)pool.chapters
                : Array.Empty<DevChapterProfile>();
            if (benchmark == null || benchmark.results == null || benchmark.results.Count == 0)
            {
                report.AddError("DEV_CHAPTER_SIMULATION_EMPTY", "BuildSimulationRunner did not read dev chapter profiles.", nameof(BuildSimulationRunner));
                return;
            }

            if (benchmark.results.Count < chapters.Count)
            {
                report.AddError(
                    "DEV_CHAPTER_SIMULATION_COUNT_LOW",
                    $"BuildSimulationRunner produced fewer results than chapter profiles. profiles={chapters.Count}, results={benchmark.results.Count}.",
                    nameof(BuildSimulationRunner));
            }

            foreach (DevChapterProfile chapter in chapters)
            {
                BuildSimulationResult result = benchmark.results.FirstOrDefault(item =>
                    string.Equals(item.buildId, chapter.chapterId, StringComparison.Ordinal));
                string path = $"{nameof(BuildSimulationResult)}:{chapter.chapterId}";
                if (result == null)
                {
                    report.AddError("DEV_CHAPTER_SIMULATION_RESULT_MISSING", $"Missing simulation result for {chapter.chapterId}.", path);
                    continue;
                }

                if (!result.devOnly || result.isEnabled)
                {
                    report.AddError("DEV_CHAPTER_SIMULATION_ISOLATION_FAIL", "Simulation result must stay devOnly=true and isEnabled=false.", path);
                }

                if (!result.simulatorReadable)
                {
                    report.AddError("DEV_CHAPTER_SIMULATION_NOT_READABLE", "Simulation result says source profile is not simulator readable.", path);
                }

                if (result.entersFormalFlow || result.affectsFormalCombat)
                {
                    report.AddError("DEV_CHAPTER_SIMULATION_FORMAL_LEAK", "Simulation result must not enter product flow or affect formal combat.", path);
                }
            }

            report.AddInfo(
                "DEV_CHAPTER_SIMULATION_READABLE",
                $"BuildSimulationRunner read {benchmark.results.Count} dev chapter scenario(s).",
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
                    report.AddError("DEV_CHAPTER_SOURCE_FILE_MISSING", $"Source file missing: {relativePath}.", relativePath);
                    continue;
                }

                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenFormalReferenceTokens)
                {
                    if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "DEV_CHAPTER_FORBIDDEN_FORMAL_TOKEN",
                            $"Source references forbidden formal-system token: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo(
                "DEV_CHAPTER_SOURCE_ISOLATION_SCANNED",
                "Dev chapter content pool source scan completed.",
                nameof(DevChapterContentPool));
        }
    }
}
#endif
