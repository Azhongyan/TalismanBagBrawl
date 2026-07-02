#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildProblemRulePoolValidator
    {
        public const int NextSeedMapRuleCount = 10;
        public const int NextSeedEnemyProblemCount = 10;
        public const int NextSeedBossProblemCount = 6;

        private static readonly string[] RuntimeSourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BuildProblemRuleConfigs.cs"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "StageConfig",
            "RewardConfig",
            "RewardDropTable",
            "UpgradeConfig",
            "SaveData",
            "PlayerPrefs",
            "MainTrialProgressData",
            "RunFlow",
            "PageState",
            "FormationState",
            "V02RunFlowController",
            "V02FormationGridFrame",
            "DamageText",
            "BossReward",
            "FormalDrop",
            "FormalForge"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Build Problem Rule Pool");
            ValidateFeatureFlags(report);
            ValidateDefaultConfig<MapRuleConfig>(
                report,
                nameof(MapRuleConfig),
                config =>
                {
                    if (!config.simulatorReadable)
                    {
                        report.AddError("MAP_RULE_SIMULATOR_UNREADABLE", "MapRuleConfig must be simulator readable by default.", nameof(MapRuleConfig));
                    }

                    ValidateFalse(report, "MAP_RULE_FORMAL_FLOW", config.entersFormalFlow, nameof(MapRuleConfig));
                    ValidateFalse(report, "MAP_RULE_FORMAL_STAGE", config.usesFormalStageData, nameof(MapRuleConfig));
                    ValidateFalse(report, "MAP_RULE_RUNTIME_UI_WRITE", config.writesRuntimeUi, nameof(MapRuleConfig));
                    ValidateFalse(report, "MAP_RULE_BALANCE_TOUCH", config.modifiesProductBalance, nameof(MapRuleConfig));
                });
            ValidateDefaultConfig<EnemyProblemConfig>(
                report,
                nameof(EnemyProblemConfig),
                config =>
                {
                    if (!config.simulatorReadable)
                    {
                        report.AddError("ENEMY_PROBLEM_SIMULATOR_UNREADABLE", "EnemyProblemConfig must be simulator readable by default.", nameof(EnemyProblemConfig));
                    }

                    ValidateFalse(report, "ENEMY_PROBLEM_FORMAL_FLOW", config.entersFormalFlow, nameof(EnemyProblemConfig));
                    ValidateFalse(report, "ENEMY_PROBLEM_FORMAL_POOL", config.referencesFormalEnemyPool, nameof(EnemyProblemConfig));
                    ValidateFalse(report, "ENEMY_PROBLEM_COMBAT_TOUCH", config.affectsFormalCombat, nameof(EnemyProblemConfig));
                });
            ValidateDefaultConfig<BossProblemConfig>(
                report,
                nameof(BossProblemConfig),
                config =>
                {
                    if (!config.simulatorReadable)
                    {
                        report.AddError("BOSS_PROBLEM_SIMULATOR_UNREADABLE", "BossProblemConfig must be simulator readable by default.", nameof(BossProblemConfig));
                    }

                    ValidateFalse(report, "BOSS_PROBLEM_FORMAL_FLOW", config.entersFormalFlow, nameof(BossProblemConfig));
                    ValidateFalse(report, "BOSS_PROBLEM_FORMAL_POOL", config.referencesFormalBossPool, nameof(BossProblemConfig));
                    ValidateFalse(report, "BOSS_PROBLEM_COMBAT_TOUCH", config.affectsFormalCombat, nameof(BossProblemConfig));
                });
            ValidateDefaultConfig<BuildReadinessCheckConfig>(
                report,
                nameof(BuildReadinessCheckConfig),
                config =>
                {
                    if (!config.reportsOnly)
                    {
                        report.AddError("READINESS_NOT_REPORT_ONLY", "BuildReadinessCheckConfig must default to reportsOnly=true.", nameof(BuildReadinessCheckConfig));
                    }

                    ValidateFalse(report, "READINESS_FORMAL_FLOW", config.entersFormalFlow, nameof(BuildReadinessCheckConfig));
                    ValidateFalse(report, "READINESS_BOSS_INFO_TOUCH", config.writesBossInfoPanel, nameof(BuildReadinessCheckConfig));
                    ValidateFalse(report, "READINESS_COMBAT_TOUCH", config.affectsFormalCombat, nameof(BuildReadinessCheckConfig));
                });
            ValidateDefaultConfig<WeaknessWindowConfig>(
                report,
                nameof(WeaknessWindowConfig),
                config =>
                {
                    if (!config.simulatorReadable)
                    {
                        report.AddError("WEAKNESS_WINDOW_SIMULATOR_UNREADABLE", "WeaknessWindowConfig must be simulator readable by default.", nameof(WeaknessWindowConfig));
                    }

                    if (config.durationSecond <= 0f)
                    {
                        report.AddError("WEAKNESS_WINDOW_DURATION_INVALID", "WeaknessWindowConfig duration must be positive.", nameof(WeaknessWindowConfig));
                    }

                    ValidateFalse(report, "WEAKNESS_WINDOW_FORMAL_FLOW", config.entersFormalFlow, nameof(WeaknessWindowConfig));
                    ValidateFalse(report, "WEAKNESS_WINDOW_COMBAT_TOUCH", config.affectsFormalCombat, nameof(WeaknessWindowConfig));
                });
            ValidateDefaultConfig<DropBiasConfig>(
                report,
                nameof(DropBiasConfig),
                config =>
                {
                    if (!config.reportsOnly)
                    {
                        report.AddError("DROP_BIAS_NOT_REPORT_ONLY", "DropBiasConfig must default to reportsOnly=true.", nameof(DropBiasConfig));
                    }

                    if (config.previewWeight < 0f)
                    {
                        report.AddError("DROP_BIAS_WEIGHT_NEGATIVE", "DropBiasConfig previewWeight cannot be negative.", nameof(DropBiasConfig));
                    }

                    ValidateFalse(report, "DROP_BIAS_FORMAL_FLOW", config.entersFormalFlow, nameof(DropBiasConfig));
                    ValidateFalse(report, "DROP_BIAS_PRODUCT_TABLE_TOUCH", config.touchesProductDropTable, nameof(DropBiasConfig));
                    ValidateFalse(report, "DROP_BIAS_PRODUCT_REWARD_TOUCH", config.grantsProductReward, nameof(DropBiasConfig));
                });
            ValidateDefaultConfig<FailureHintConfig>(
                report,
                nameof(FailureHintConfig),
                config =>
                {
                    if (!config.reportsOnly)
                    {
                        report.AddError("FAILURE_HINT_NOT_REPORT_ONLY", "FailureHintConfig must default to reportsOnly=true.", nameof(FailureHintConfig));
                    }

                    ValidateFalse(report, "FAILURE_HINT_FORMAL_FLOW", config.entersFormalFlow, nameof(FailureHintConfig));
                    ValidateFalse(report, "FAILURE_HINT_RUNTIME_UI_WRITE", config.writesRuntimeUi, nameof(FailureHintConfig));
                });

            ValidateBossKeyRequirement(report);
            ValidateSourceIsolation(report);
            report.AddInfo(
                "NEXT_SEEDDATA_COUNTS",
                $"Next package should add {NextSeedMapRuleCount} map rule seed(s), {NextSeedEnemyProblemCount} enemy problem seed(s), and {NextSeedBossProblemCount} boss problem seed(s).",
                "Docs/V0.4/BuildProblemRulePool01_Assignment.md");
            return report;
        }

        private static void ValidateDefaultConfig<T>(
            BuildSandboxValidationReport report,
            string typeName,
            Action<T> validateExtra)
            where T : BuildSandboxDevOnlyConfig
        {
            T config = ScriptableObject.CreateInstance<T>();
            try
            {
                if (!config.devOnly)
                {
                    report.AddError($"{typeName.ToUpperInvariant()}_DEVONLY_FALSE", $"{typeName} must default devOnly=true.", typeName);
                }

                if (config.isEnabled)
                {
                    report.AddError($"{typeName.ToUpperInvariant()}_ENABLED_TRUE", $"{typeName} must default isEnabled=false.", typeName);
                }

                if (config.devOnly && !config.isEnabled)
                {
                    report.AddInfo("RULE_CONFIG_DEFAULT_ISOLATED", $"{typeName} defaults devOnly=true and isEnabled=false.", typeName);
                }

                validateExtra?.Invoke(config);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        private static void ValidateBossKeyRequirement(BuildSandboxValidationReport report)
        {
            BossProblemKeyRequirement key = new();
            if (!key.devOnly)
            {
                report.AddError("BOSS_KEY_DEVONLY_FALSE", "BossProblemKeyRequirement must default devOnly=true.", nameof(BossProblemKeyRequirement));
            }

            if (key.isEnabled)
            {
                report.AddError("BOSS_KEY_ENABLED_TRUE", "BossProblemKeyRequirement must default isEnabled=false.", nameof(BossProblemKeyRequirement));
            }

            ValidateFalse(report, "BOSS_KEY_FORMAL_BOSS_GATE", key.gatesFormalBoss, nameof(BossProblemKeyRequirement));
            report.AddInfo(
                "BOSS_KEY_SCHEMA_PRESENT",
                "BossProblemKeyRequirement schema exists for later multi-key Boss problem seeds.",
                nameof(BossProblemKeyRequirement));
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError("BUILD_PROBLEM_FEATURE_FLAG_TRUE", $"{flag.Key} must remain false.", nameof(BuildSandboxFeatureFlags));
                    continue;
                }

                report.AddInfo("BUILD_PROBLEM_FEATURE_FLAG_FALSE", $"{flag.Key}=false.", nameof(BuildSandboxFeatureFlags));
            }
        }

        private static void ValidateSourceIsolation(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            foreach (string relativePath in RuntimeSourceFiles)
            {
                string path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    report.AddError("BUILD_PROBLEM_SOURCE_MISSING", $"Source file missing: {relativePath}.", relativePath);
                    continue;
                }

                string source = StripCommentsAndStrings(File.ReadAllText(path));
                foreach (string token in ForbiddenRuntimeTokens)
                {
                    if (source.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "BUILD_PROBLEM_FORBIDDEN_FORMAL_TOKEN",
                            $"Rule config source contains forbidden product-flow token: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo("BUILD_PROBLEM_SOURCE_ISOLATION_SCANNED", "Build problem rule source isolation scan completed.", nameof(BuildProblemRulePoolValidator));
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string path)
        {
            if (value)
            {
                report.AddError(code, "Formal leak guard must default false.", path);
            }
        }

        private static string StripCommentsAndStrings(string source)
        {
            System.Text.StringBuilder builder = new(source.Length);
            bool inString = false;
            bool inChar = false;
            bool inLineComment = false;
            bool inBlockComment = false;
            bool verbatimString = false;

            for (int i = 0; i < source.Length; i++)
            {
                char current = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : '\0';

                if (inLineComment)
                {
                    if (current == '\n')
                    {
                        inLineComment = false;
                        builder.Append('\n');
                    }

                    continue;
                }

                if (inBlockComment)
                {
                    if (current == '*' && next == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }

                    continue;
                }

                if (inString)
                {
                    if (verbatimString && current == '"' && next == '"')
                    {
                        i++;
                        continue;
                    }

                    if (current == '"' && (verbatimString || !IsEscaped(source, i)))
                    {
                        inString = false;
                        verbatimString = false;
                    }

                    continue;
                }

                if (inChar)
                {
                    if (current == '\'' && !IsEscaped(source, i))
                    {
                        inChar = false;
                    }

                    continue;
                }

                if (current == '/' && next == '/')
                {
                    inLineComment = true;
                    i++;
                    continue;
                }

                if (current == '/' && next == '*')
                {
                    inBlockComment = true;
                    i++;
                    continue;
                }

                if (current == '@' && next == '"')
                {
                    inString = true;
                    verbatimString = true;
                    i++;
                    continue;
                }

                if (current == '"')
                {
                    inString = true;
                    continue;
                }

                if (current == '\'')
                {
                    inChar = true;
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        private static bool IsEscaped(string source, int index)
        {
            int slashCount = 0;
            for (int i = index - 1; i >= 0 && source[i] == '\\'; i--)
            {
                slashCount++;
            }

            return slashCount % 2 == 1;
        }
    }
}
#endif
