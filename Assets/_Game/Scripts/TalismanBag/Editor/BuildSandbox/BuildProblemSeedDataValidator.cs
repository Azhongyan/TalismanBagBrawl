#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildProblemSeedDataValidator
    {
        public const int RequiredMapRuleCount = 10;
        public const int RequiredEnemyProblemCount = 10;
        public const int RequiredBossProblemCount = 6;

        private static readonly string[] RequiredMapRuleNames =
        {
            "黑炉烟尘",
            "青石回潮",
            "灯火不足",
            "铜铃夜响",
            "阵眼偏移",
            "符纸受潮",
            "旧巷回音",
            "炉灰落阵",
            "夜巡灯灭",
            "青石裂纹"
        };

        private static readonly string[] RequiredEnemyProblemNames =
        {
            "护盾",
            "群怪",
            "毒 / 燃",
            "偷灵",
            "封符",
            "高爆发",
            "施法",
            "厚血",
            "污染格",
            "阵眼干扰"
        };

        private static readonly string[] RequiredBossNames =
        {
            "黑炉护壳师",
            "秽签梦母",
            "偷灵炉心",
            "叩阵铜将",
            "纸人千面阵",
            "黑炉复合阵眼"
        };

        private static readonly string[] RequiredProblemAttributes =
        {
            "BreakPower",
            "CleansePower",
            "ControlPower",
            "GuardPower",
            "EnergyStability",
            "ClearPower",
            "BurstWindow"
        };

        private static readonly string[] SourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemSeedData.cs"
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
            "BossConfig",
            "DropTable",
            "StageConfig"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Build Problem Seed Data");
            BuildProblemSeedDataset dataset = BuildProblemSeedDataset.CreateDefault();
            ValidateDataset(report, dataset);
            ValidateFeatureFlags(report);
            ValidateSourceIsolation(report);
            return report;
        }

        public static void ValidateDataset(BuildSandboxValidationReport report, BuildProblemSeedDataset dataset)
        {
            if (dataset == null)
            {
                report.AddError("BUILD_PROBLEM_SEED_DATASET_NULL", "BuildProblemSeedDataset is missing.", nameof(BuildProblemSeedDataset));
                return;
            }

            if (!dataset.devOnly)
            {
                report.AddError("BUILD_PROBLEM_SEED_DATASET_DEVONLY_FALSE", "Dataset must keep devOnly=true.", nameof(BuildProblemSeedDataset));
            }

            if (dataset.isEnabled)
            {
                report.AddError("BUILD_PROBLEM_SEED_DATASET_ENABLED_TRUE", "Dataset must keep isEnabled=false.", nameof(BuildProblemSeedDataset));
            }

            ValidateMapRules(report, dataset.mapRules);
            ValidateEnemyProblems(report, dataset.enemyProblems);
            ValidateBossProblems(report, dataset.bossProblems);
            ValidateWeaknessWindows(report, dataset.bossProblems, dataset.weaknessWindows);
            ValidateDropBiases(report, dataset.bossProblems, dataset.dropBiases);
            ValidateFailureHints(report, dataset.failureHints);
            ValidateProblemAttributeCoverage(report, dataset.bossProblems);
        }

        private static void ValidateMapRules(BuildSandboxValidationReport report, IReadOnlyList<MapRuleSeed> mapRules)
        {
            if (mapRules == null || mapRules.Count < RequiredMapRuleCount)
            {
                report.AddError(
                    "MAP_RULE_SEED_COUNT_LOW",
                    $"BuildProblemSeedData01 requires at least {RequiredMapRuleCount} map rules; actual={mapRules?.Count ?? 0}.",
                    nameof(MapRuleSeed));
                return;
            }

            ValidateRequiredNames(report, RequiredMapRuleNames, mapRules.Select(rule => rule.displayName), "MAP_RULE_REQUIRED_MISSING", nameof(MapRuleSeed));
            ValidateUniqueIds(report, mapRules.Select(rule => rule.mapRuleId), "MAP_RULE_ID_DUPLICATE", nameof(MapRuleSeed));

            foreach (MapRuleSeed rule in mapRules)
            {
                string path = $"{nameof(MapRuleSeed)}:{rule?.mapRuleId}";
                if (rule == null)
                {
                    report.AddError("MAP_RULE_NULL", "Map rule seed is null.", nameof(MapRuleSeed));
                    continue;
                }

                ValidateSharedSeedFlags(report, path, rule.devOnly, rule.isEnabled, rule.entersFormalFlow);
                ValidateRequiredText(report, "MAP_RULE_ID_MISSING", rule.mapRuleId, "mapRuleId", path);
                ValidateRequiredText(report, "MAP_RULE_DISPLAY_MISSING", rule.displayName, "displayName", path);
                ValidateRequiredText(report, "MAP_RULE_DESCRIPTION_MISSING", rule.description, "description", path);
                ValidateRequiredText(report, "MAP_RULE_WARNING_MISSING", rule.warningText, "warningText", path);
                ValidateListNotEmpty(report, "MAP_RULE_AFFECTED_TAGS_MISSING", rule.affectedTags, "affectedTags", path);
                if ((rule.buffTags == null || rule.buffTags.Count == 0)
                    && (rule.debuffTags == null || rule.debuffTags.Count == 0))
                {
                    report.AddError("MAP_RULE_BUFF_DEBUFF_MISSING", "Map rule must declare buffTags or debuffTags.", path);
                }

                if (rule.usesFormalStageData)
                {
                    report.AddError("MAP_RULE_FORMAL_STAGE_LEAK", "Map rule seed must not use product stage data.", path);
                }

                report.AddInfo("MAP_RULE_SEED_SCANNED", $"{rule.displayName} devOnly={rule.devOnly}, isEnabled={rule.isEnabled}.", path);
            }
        }

        private static void ValidateEnemyProblems(BuildSandboxValidationReport report, IReadOnlyList<EnemyProblemSeed> enemyProblems)
        {
            if (enemyProblems == null || enemyProblems.Count < RequiredEnemyProblemCount)
            {
                report.AddError(
                    "ENEMY_PROBLEM_SEED_COUNT_LOW",
                    $"BuildProblemSeedData01 requires at least {RequiredEnemyProblemCount} enemy problems; actual={enemyProblems?.Count ?? 0}.",
                    nameof(EnemyProblemSeed));
                return;
            }

            ValidateRequiredNames(report, RequiredEnemyProblemNames, enemyProblems.Select(problem => problem.displayName), "ENEMY_PROBLEM_REQUIRED_MISSING", nameof(EnemyProblemSeed));
            ValidateUniqueIds(report, enemyProblems.Select(problem => problem.problemType), "ENEMY_PROBLEM_ID_DUPLICATE", nameof(EnemyProblemSeed));

            foreach (EnemyProblemSeed problem in enemyProblems)
            {
                string path = $"{nameof(EnemyProblemSeed)}:{problem?.problemType}";
                if (problem == null)
                {
                    report.AddError("ENEMY_PROBLEM_NULL", "Enemy problem seed is null.", nameof(EnemyProblemSeed));
                    continue;
                }

                ValidateSharedSeedFlags(report, path, problem.devOnly, problem.isEnabled, problem.entersFormalFlow);
                ValidateRequiredText(report, "ENEMY_PROBLEM_TYPE_MISSING", problem.problemType, "problemType", path);
                ValidateRequiredText(report, "ENEMY_PROBLEM_PRESSURE_MISSING", problem.pressureType, "pressureType", path);
                ValidateListNotEmpty(report, "ENEMY_PROBLEM_HARD_TAGS_MISSING", problem.hardSolutionTags, "hardSolutionTags", path);
                ValidateListNotEmpty(report, "ENEMY_PROBLEM_SOFT_TAGS_MISSING", problem.softSolutionTags, "softSolutionTags", path);
                ValidateListNotEmpty(report, "ENEMY_PROBLEM_VALIDATED_TAGS_MISSING", problem.validatedBuildTags, "validatedBuildTags", path);
                ValidateRequiredText(report, "ENEMY_PROBLEM_ACTION_MISSING", problem.recommendedAction, "recommendedAction", path);

                if (problem.failureHint == null || string.IsNullOrWhiteSpace(problem.failureHint.failureHintId))
                {
                    report.AddError("ENEMY_PROBLEM_FAILURE_HINT_MISSING", "Enemy problem must include a failure hint.", path);
                }

                if (problem.referencesProductEnemyList || problem.affectsFormalCombat)
                {
                    report.AddError("ENEMY_PROBLEM_FORMAL_LEAK", "Enemy problem seed must not touch product enemy lists or product combat.", path);
                }

                report.AddInfo("ENEMY_PROBLEM_SEED_SCANNED", $"{problem.displayName} devOnly={problem.devOnly}, isEnabled={problem.isEnabled}.", path);
            }
        }

        private static void ValidateBossProblems(BuildSandboxValidationReport report, IReadOnlyList<BossProblemSeed> bossProblems)
        {
            if (bossProblems == null || bossProblems.Count < RequiredBossProblemCount)
            {
                report.AddError(
                    "BOSS_PROBLEM_SEED_COUNT_LOW",
                    $"BuildProblemSeedData01 requires at least {RequiredBossProblemCount} boss problems; actual={bossProblems?.Count ?? 0}.",
                    nameof(BossProblemSeed));
                return;
            }

            ValidateRequiredNames(report, RequiredBossNames, bossProblems.Select(boss => boss.displayName), "BOSS_PROBLEM_REQUIRED_MISSING", nameof(BossProblemSeed));
            ValidateUniqueIds(report, bossProblems.Select(boss => boss.bossProblemId), "BOSS_PROBLEM_ID_DUPLICATE", nameof(BossProblemSeed));

            foreach (BossProblemSeed boss in bossProblems)
            {
                string path = $"{nameof(BossProblemSeed)}:{boss?.bossProblemId}";
                if (boss == null)
                {
                    report.AddError("BOSS_PROBLEM_NULL", "Boss problem seed is null.", nameof(BossProblemSeed));
                    continue;
                }

                ValidateSharedSeedFlags(report, path, boss.devOnly, boss.isEnabled, boss.entersFormalFlow);
                ValidateRequiredText(report, "BOSS_PROBLEM_ID_MISSING", boss.bossProblemId, "bossProblemId", path);
                ValidateRequiredText(report, "BOSS_PROBLEM_GOAL_MISSING", boss.validationGoal, "validationGoal", path);
                ValidateListNotEmpty(report, "BOSS_PROBLEM_ATTRIBUTES_MISSING", boss.requiredProblemAttributes, "requiredProblemAttributes", path);

                int keyCount = boss.keyRequirements?.Count ?? 0;
                if (keyCount < 3 || keyCount > 5)
                {
                    report.AddError("BOSS_KEY_COUNT_OUT_OF_RANGE", $"Boss must check 3-5 keys; actual={keyCount}.", path);
                }

                if (boss.minimumKeysRequired < 1 || boss.minimumKeysRequired > keyCount)
                {
                    report.AddError("BOSS_MIN_KEYS_INVALID", "minimumKeysRequired must be within key count.", path);
                }

                foreach (BossProblemKeySeed key in boss.keyRequirements ?? Enumerable.Empty<BossProblemKeySeed>())
                {
                    string keyPath = $"{path}/{key?.keyId}";
                    if (key == null)
                    {
                        report.AddError("BOSS_KEY_NULL", "Boss key is null.", path);
                        continue;
                    }

                    ValidateSharedSeedFlags(report, keyPath, key.devOnly, key.isEnabled, key.gatesProductBoss);
                    ValidateRequiredText(report, "BOSS_KEY_ID_MISSING", key.keyId, "keyId", keyPath);
                    ValidateRequiredText(report, "BOSS_KEY_CATEGORY_MISSING", key.keyCategory, "keyCategory", keyPath);
                    ValidateRequiredText(report, "BOSS_KEY_ATTRIBUTE_MISSING", key.problemAttribute, "problemAttribute", keyPath);
                    if (key.requiredScore <= 0)
                    {
                        report.AddError("BOSS_KEY_SCORE_INVALID", "Boss key requiredScore must be positive.", keyPath);
                    }
                }

                if (boss.referencesProductBossList || boss.affectsFormalCombat)
                {
                    report.AddError("BOSS_PROBLEM_FORMAL_LEAK", "Boss problem seed must not touch product Boss lists or product combat.", path);
                }

                report.AddInfo("BOSS_PROBLEM_SEED_SCANNED", $"{boss.displayName} keys={keyCount}, devOnly={boss.devOnly}, isEnabled={boss.isEnabled}.", path);
            }
        }

        private static void ValidateWeaknessWindows(
            BuildSandboxValidationReport report,
            IReadOnlyList<BossProblemSeed> bossProblems,
            IReadOnlyList<WeaknessWindowSeed> weaknessWindows)
        {
            foreach (BossProblemSeed boss in bossProblems ?? Enumerable.Empty<BossProblemSeed>())
            {
                if (boss.weaknessWindows == null || boss.weaknessWindows.Count == 0)
                {
                    report.AddError("BOSS_WEAKNESS_WINDOW_MISSING", "Each Boss must have at least one weakness window.", boss.bossProblemId);
                }
            }

            ValidateUniqueIds(report, weaknessWindows.Select(window => window.weaknessWindowId), "WEAKNESS_WINDOW_ID_DUPLICATE", nameof(WeaknessWindowSeed));
            foreach (WeaknessWindowSeed window in weaknessWindows ?? Enumerable.Empty<WeaknessWindowSeed>())
            {
                string path = $"{nameof(WeaknessWindowSeed)}:{window?.weaknessWindowId}";
                if (window == null)
                {
                    report.AddError("WEAKNESS_WINDOW_NULL", "Weakness window seed is null.", nameof(WeaknessWindowSeed));
                    continue;
                }

                ValidateSharedSeedFlags(report, path, window.devOnly, window.isEnabled, window.entersFormalFlow);
                ValidateRequiredText(report, "WEAKNESS_WINDOW_TRIGGER_MISSING", window.triggerCondition, "triggerCondition", path);
                ValidateListNotEmpty(report, "WEAKNESS_WINDOW_TAGS_MISSING", window.exposedBuildTags, "exposedBuildTags", path);
                if (window.durationSecond <= 0f)
                {
                    report.AddError("WEAKNESS_WINDOW_DURATION_INVALID", "Weakness window duration must be positive.", path);
                }

                if (window.affectsFormalCombat)
                {
                    report.AddError("WEAKNESS_WINDOW_FORMAL_COMBAT_LEAK", "Weakness window must not affect product combat.", path);
                }
            }
        }

        private static void ValidateDropBiases(
            BuildSandboxValidationReport report,
            IReadOnlyList<BossProblemSeed> bossProblems,
            IReadOnlyList<DropBiasSeed> dropBiases)
        {
            foreach (BossProblemSeed boss in bossProblems ?? Enumerable.Empty<BossProblemSeed>())
            {
                int count = boss.dropBiases?.Count ?? 0;
                if (count < 2 || count > 3)
                {
                    report.AddError("BOSS_DROP_BIAS_COUNT_OUT_OF_RANGE", $"Each Boss needs 2-3 DropBias seeds; actual={count}.", boss.bossProblemId);
                }
            }

            ValidateUniqueIds(report, dropBiases.Select(drop => drop.dropBiasId), "DROP_BIAS_ID_DUPLICATE", nameof(DropBiasSeed));
            foreach (DropBiasSeed drop in dropBiases ?? Enumerable.Empty<DropBiasSeed>())
            {
                string path = $"{nameof(DropBiasSeed)}:{drop?.dropBiasId}";
                if (drop == null)
                {
                    report.AddError("DROP_BIAS_NULL", "DropBias seed is null.", nameof(DropBiasSeed));
                    continue;
                }

                ValidateSharedSeedFlags(report, path, drop.devOnly, drop.isEnabled, drop.entersFormalFlow);
                ValidateRequiredText(report, "DROP_BIAS_ID_MISSING", drop.dropBiasId, "dropBiasId", path);
                ValidateListNotEmpty(report, "DROP_BIAS_BUILD_TAGS_MISSING", drop.targetBuildTags, "targetBuildTags", path);
                ValidateListNotEmpty(report, "DROP_BIAS_ITEM_TAGS_MISSING", drop.targetItemTags, "targetItemTags", path);
                if (!drop.reportsOnly)
                {
                    report.AddError("DROP_BIAS_NOT_REPORT_ONLY", "DropBias seed must remain reportsOnly=true.", path);
                }

                if (drop.previewWeight < 0f)
                {
                    report.AddError("DROP_BIAS_WEIGHT_NEGATIVE", "DropBias previewWeight cannot be negative.", path);
                }

                if (drop.referencesProductDrops || drop.grantsProductItems)
                {
                    report.AddError("DROP_BIAS_PRODUCT_LEAK", "DropBias seed must not reference product drops or grant product items.", path);
                }
            }
        }

        private static void ValidateFailureHints(BuildSandboxValidationReport report, IReadOnlyList<FailureHintSeed> failureHints)
        {
            ValidateUniqueIds(report, failureHints.Select(hint => hint.failureHintId), "FAILURE_HINT_ID_DUPLICATE", nameof(FailureHintSeed));
            foreach (FailureHintSeed hint in failureHints ?? Enumerable.Empty<FailureHintSeed>())
            {
                string path = $"{nameof(FailureHintSeed)}:{hint?.failureHintId}";
                if (hint == null)
                {
                    report.AddError("FAILURE_HINT_NULL", "Failure hint seed is null.", nameof(FailureHintSeed));
                    continue;
                }

                ValidateSharedSeedFlags(report, path, hint.devOnly, hint.isEnabled, hint.entersFormalFlow);
                ValidateRequiredText(report, "FAILURE_HINT_ID_MISSING", hint.failureHintId, "failureHintId", path);
                ValidateRequiredText(report, "FAILURE_HINT_HEADLINE_MISSING", hint.headline, "headline", path);
                ValidateRequiredText(report, "FAILURE_HINT_DETAIL_MISSING", hint.detail, "detail", path);
                if (!hint.reportsOnly)
                {
                    report.AddError("FAILURE_HINT_NOT_REPORT_ONLY", "Failure hint seed must remain reportsOnly=true.", path);
                }

                if (hint.writesRuntimeUi)
                {
                    report.AddError("FAILURE_HINT_RUNTIME_UI_LEAK", "Failure hint seed must not write runtime UI.", path);
                }
            }
        }

        private static void ValidateProblemAttributeCoverage(BuildSandboxValidationReport report, IReadOnlyList<BossProblemSeed> bossProblems)
        {
            HashSet<string> covered = new(StringComparer.Ordinal);
            foreach (BossProblemSeed boss in bossProblems ?? Enumerable.Empty<BossProblemSeed>())
            {
                foreach (string attribute in boss.requiredProblemAttributes ?? Enumerable.Empty<string>())
                {
                    covered.Add(attribute);
                }

                foreach (BossProblemKeySeed key in boss.keyRequirements ?? Enumerable.Empty<BossProblemKeySeed>())
                {
                    if (!string.IsNullOrWhiteSpace(key?.problemAttribute))
                    {
                        covered.Add(key.problemAttribute);
                    }
                }
            }

            foreach (string attribute in RequiredProblemAttributes)
            {
                if (covered.Contains(attribute))
                {
                    report.AddInfo("PROBLEM_ATTRIBUTE_COVERED", $"Problem attribute covered: {attribute}.", nameof(BossProblemSeed));
                    continue;
                }

                report.AddError("PROBLEM_ATTRIBUTE_MISSING", $"Problem attribute missing: {attribute}.", nameof(BossProblemSeed));
            }
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError("SEEDDATA_FEATURE_FLAG_TRUE", $"{flag.Key} must stay false for BuildProblemSeedData01.", nameof(BuildSandboxFeatureFlags));
                    continue;
                }

                report.AddInfo("SEEDDATA_FEATURE_FLAG_FALSE", $"{flag.Key}=false.", nameof(BuildSandboxFeatureFlags));
            }
        }

        private static void ValidateSourceIsolation(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetCurrentDirectory();
            foreach (string relativePath in SourceFiles)
            {
                string path = Path.Combine(projectRoot, relativePath);
                if (!File.Exists(path))
                {
                    report.AddError("SEEDDATA_SOURCE_FILE_MISSING", $"Source file missing: {relativePath}.", relativePath);
                    continue;
                }

                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenFormalReferenceTokens)
                {
                    if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "SEEDDATA_FORBIDDEN_FORMAL_TOKEN",
                            $"Seed data source references forbidden product-system token: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo("SEEDDATA_SOURCE_ISOLATION_SCANNED", "Build problem seed source scan completed.", nameof(BuildProblemSeedDataValidator));
        }

        private static void ValidateRequiredNames(
            BuildSandboxValidationReport report,
            IEnumerable<string> requiredNames,
            IEnumerable<string> actualNames,
            string code,
            string path)
        {
            HashSet<string> actual = new(
                (actualNames ?? Enumerable.Empty<string>()).Where(name => !string.IsNullOrWhiteSpace(name)),
                StringComparer.Ordinal);
            foreach (string name in requiredNames)
            {
                if (actual.Contains(name))
                {
                    report.AddInfo($"{code}_PRESENT", $"Required seed present: {name}.", path);
                    continue;
                }

                report.AddError(code, $"Required seed missing: {name}.", path);
            }
        }

        private static void ValidateUniqueIds(
            BuildSandboxValidationReport report,
            IEnumerable<string> ids,
            string code,
            string path)
        {
            HashSet<string> seen = new(StringComparer.Ordinal);
            foreach (string id in ids ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (!seen.Add(id))
                {
                    report.AddError(code, $"Duplicate id: {id}.", path);
                }
            }
        }

        private static void ValidateSharedSeedFlags(
            BuildSandboxValidationReport report,
            string path,
            bool devOnly,
            bool isEnabled,
            bool entersFormalFlow)
        {
            if (!devOnly)
            {
                report.AddError("SEED_DEVONLY_FALSE", "Seed must keep devOnly=true.", path);
            }

            if (isEnabled)
            {
                report.AddError("SEED_ENABLED_TRUE", "Seed must keep isEnabled=false.", path);
            }

            if (entersFormalFlow)
            {
                report.AddError("SEED_FORMAL_FLOW_LEAK", "Seed must not enter product flow.", path);
            }
        }

        private static void ValidateRequiredText(
            BuildSandboxValidationReport report,
            string code,
            string value,
            string fieldName,
            string path)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                report.AddError(code, $"{fieldName} is required.", path);
            }
        }

        private static void ValidateListNotEmpty(
            BuildSandboxValidationReport report,
            string code,
            IReadOnlyCollection<string> values,
            string fieldName,
            string path)
        {
            if (values == null || values.Count == 0)
            {
                report.AddError(code, $"{fieldName} must not be empty.", path);
            }
        }
    }
}
#endif
