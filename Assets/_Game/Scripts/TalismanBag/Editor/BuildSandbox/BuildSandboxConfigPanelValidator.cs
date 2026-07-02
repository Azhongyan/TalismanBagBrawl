#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxConfigPanelValidator
    {
        public const int RequiredTabCount = 8;
        public const int ExpectedWeaknessWindowCount = 6;
        public const int ExpectedDropBiasCount = 18;

        private static readonly string[] RequiredTabs =
        {
            nameof(BuildSandboxConfigPanelTab.Overview),
            nameof(BuildSandboxConfigPanelTab.MapRule),
            nameof(BuildSandboxConfigPanelTab.EnemyProblem),
            nameof(BuildSandboxConfigPanelTab.BossProblem),
            nameof(BuildSandboxConfigPanelTab.WeaknessWindow),
            nameof(BuildSandboxConfigPanelTab.DropBias),
            nameof(BuildSandboxConfigPanelTab.BuildReadiness),
            nameof(BuildSandboxConfigPanelTab.Validation)
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

        private static readonly string[] PanelSourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildSandboxConfigPanelWindow.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildSandboxConfigPanelValidator.cs",
            "Assets/_Game/Scripts/TalismanBag/Editor/BuildSandbox/BuildSandboxConfigPanelReportWriter.cs"
        };

        private static readonly string[] ForbiddenSourceTokens =
        {
            "DataCatalogEditorWindow",
            "StageConfigPanelEditorUi",
            "V02RunConfig",
            "EnemyDefinition",
            "EnemyGroupConfig",
            "RewardConfig",
            "RewardDropTable",
            "BossInfoConfig",
            "TutorialGuideConfig",
            "TalismanItemDefinition",
            "V02RunFlowController",
            "V02FormationGridFrame",
            "DamageText",
            "SaveData",
            "PlayerPrefs",
            "MainTrialProgressData",
            "UpgradeConfig",
            "DropTable"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                BuildProblemRulePoolValidator.Validate(),
                BuildProblemSeedDataValidator.Validate(),
                Validate()
            };
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BuildSandbox Config Panel 01");
            BuildProblemSeedDataset dataset = BuildProblemSeedDataset.CreateDefault();

            ValidateMenu(report);
            ValidateTabs(report);
            ValidateFeatureFlags(report);
            ValidateDataset(report, dataset);
            ValidateReadinessCoverage(report, dataset);
            ValidateSourceIsolation(report);
            return report;
        }

        private static void ValidateMenu(BuildSandboxValidationReport report)
        {
            MethodInfo openMethod = typeof(BuildSandboxConfigPanelWindow).GetMethod(
                nameof(BuildSandboxConfigPanelWindow.Open),
                BindingFlags.Public | BindingFlags.Static);
            MenuItem menuItem = openMethod?
                .GetCustomAttributes(typeof(MenuItem), false)
                .OfType<MenuItem>()
                .FirstOrDefault();

            if (menuItem == null)
            {
                report.AddError(
                    "CONFIG_PANEL_MENU_MISSING",
                    "BuildSandbox Config Panel 01 Open method must have a MenuItem attribute.",
                    nameof(BuildSandboxConfigPanelWindow));
                return;
            }

            if (!string.Equals(menuItem.menuItem, BuildSandboxConfigPanelWindow.MenuPath, StringComparison.Ordinal))
            {
                report.AddError(
                    "CONFIG_PANEL_MENU_PATH_MISMATCH",
                    $"Menu path mismatch. actual={menuItem.menuItem}",
                    nameof(BuildSandboxConfigPanelWindow));
                return;
            }

            report.AddInfo(
                "CONFIG_PANEL_MENU_PATH_PRESENT",
                BuildSandboxConfigPanelWindow.MenuPath,
                nameof(BuildSandboxConfigPanelWindow));
        }

        private static void ValidateTabs(BuildSandboxValidationReport report)
        {
            string[] tabs = Enum.GetNames(typeof(BuildSandboxConfigPanelTab));
            if (tabs.Length < RequiredTabCount)
            {
                report.AddError(
                    "CONFIG_PANEL_TAB_COUNT_LOW",
                    $"Panel must expose at least {RequiredTabCount} tab(s); actual={tabs.Length}.",
                    nameof(BuildSandboxConfigPanelTab));
            }
            else
            {
                report.AddInfo(
                    "CONFIG_PANEL_TAB_COUNT_PASS",
                    $"Panel tabs={tabs.Length}.",
                    nameof(BuildSandboxConfigPanelTab));
            }

            HashSet<string> present = new(tabs, StringComparer.Ordinal);
            foreach (string required in RequiredTabs)
            {
                if (present.Contains(required))
                {
                    report.AddInfo("CONFIG_PANEL_REQUIRED_TAB_PRESENT", required, nameof(BuildSandboxConfigPanelTab));
                }
                else
                {
                    report.AddError("CONFIG_PANEL_REQUIRED_TAB_MISSING", required, nameof(BuildSandboxConfigPanelTab));
                }
            }
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError(
                        "CONFIG_PANEL_FEATURE_FLAG_TRUE",
                        $"{flag.Key} must stay false for BuildSandboxConfigPanel01.",
                        nameof(BuildSandboxFeatureFlags));
                }
                else
                {
                    report.AddInfo(
                        "CONFIG_PANEL_FEATURE_FLAG_FALSE",
                        $"{flag.Key}=false.",
                        nameof(BuildSandboxFeatureFlags));
                }
            }
        }

        private static void ValidateDataset(BuildSandboxValidationReport report, BuildProblemSeedDataset dataset)
        {
            if (dataset == null)
            {
                report.AddError("CONFIG_PANEL_DATASET_NULL", "BuildProblemSeedDataset is missing.", nameof(BuildProblemSeedDataset));
                return;
            }

            ValidateSharedFlags(report, nameof(BuildProblemSeedDataset), dataset.devOnly, dataset.isEnabled, false);
            ValidateCount(report, "CONFIG_PANEL_MAP_RULE_COUNT", "MapRule", dataset.mapRules.Count, BuildProblemSeedDataValidator.RequiredMapRuleCount);
            ValidateCount(report, "CONFIG_PANEL_ENEMY_PROBLEM_COUNT", "EnemyProblem", dataset.enemyProblems.Count, BuildProblemSeedDataValidator.RequiredEnemyProblemCount);
            ValidateCount(report, "CONFIG_PANEL_BOSS_PROBLEM_COUNT", "BossProblem", dataset.bossProblems.Count, BuildProblemSeedDataValidator.RequiredBossProblemCount);
            ValidateCount(report, "CONFIG_PANEL_WEAKNESS_WINDOW_COUNT", "WeaknessWindow", dataset.weaknessWindows.Count, ExpectedWeaknessWindowCount);
            ValidateCount(report, "CONFIG_PANEL_DROP_BIAS_COUNT", "DropBias", dataset.dropBiases.Count, ExpectedDropBiasCount);

            foreach (MapRuleSeed rule in dataset.mapRules)
            {
                ValidateSharedFlags(report, $"MapRule:{rule.mapRuleId}", rule.devOnly, rule.isEnabled, rule.entersFormalFlow || rule.usesFormalStageData);
            }

            foreach (EnemyProblemSeed problem in dataset.enemyProblems)
            {
                ValidateSharedFlags(
                    report,
                    $"EnemyProblem:{problem.problemType}",
                    problem.devOnly,
                    problem.isEnabled,
                    problem.entersFormalFlow || problem.referencesProductEnemyList || problem.affectsFormalCombat);
                ValidateFailureHint(report, problem.failureHint);
            }

            foreach (BossProblemSeed boss in dataset.bossProblems)
            {
                ValidateSharedFlags(
                    report,
                    $"BossProblem:{boss.bossProblemId}",
                    boss.devOnly,
                    boss.isEnabled,
                    boss.entersFormalFlow || boss.referencesProductBossList || boss.affectsFormalCombat);
                foreach (BossProblemKeySeed key in boss.keyRequirements)
                {
                    ValidateSharedFlags(report, $"BossKey:{key.keyId}", key.devOnly, key.isEnabled, key.gatesProductBoss);
                }
            }

            foreach (WeaknessWindowSeed window in dataset.weaknessWindows)
            {
                ValidateSharedFlags(
                    report,
                    $"WeaknessWindow:{window.weaknessWindowId}",
                    window.devOnly,
                    window.isEnabled,
                    window.entersFormalFlow || window.affectsFormalCombat);
            }

            foreach (DropBiasSeed drop in dataset.dropBiases)
            {
                ValidateSharedFlags(
                    report,
                    $"DropBias:{drop.dropBiasId}",
                    drop.devOnly,
                    drop.isEnabled,
                    drop.entersFormalFlow || drop.referencesProductDrops || drop.grantsProductItems);
                if (!drop.reportsOnly)
                {
                    report.AddError("CONFIG_PANEL_DROP_BIAS_NOT_REPORT_ONLY", "DropBias must stay reportsOnly=true.", drop.dropBiasId);
                }
            }

            foreach (FailureHintSeed hint in dataset.failureHints)
            {
                ValidateFailureHint(report, hint);
            }
        }

        private static void ValidateReadinessCoverage(BuildSandboxValidationReport report, BuildProblemSeedDataset dataset)
        {
            HashSet<string> covered = new(StringComparer.Ordinal);
            foreach (BossProblemSeed boss in dataset.bossProblems)
            {
                foreach (string attribute in boss.requiredProblemAttributes ?? Enumerable.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(attribute))
                    {
                        covered.Add(attribute);
                    }
                }

                foreach (BossProblemKeySeed key in boss.keyRequirements ?? Enumerable.Empty<BossProblemKeySeed>())
                {
                    if (!string.IsNullOrWhiteSpace(key.problemAttribute))
                    {
                        covered.Add(key.problemAttribute);
                    }
                }
            }

            foreach (string required in RequiredProblemAttributes)
            {
                if (covered.Contains(required))
                {
                    report.AddInfo("CONFIG_PANEL_READINESS_ATTRIBUTE_COVERED", required, nameof(BuildSandboxConfigPanelTab.BuildReadiness));
                }
                else
                {
                    report.AddError("CONFIG_PANEL_READINESS_ATTRIBUTE_MISSING", required, nameof(BuildSandboxConfigPanelTab.BuildReadiness));
                }
            }
        }

        private static void ValidateSourceIsolation(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            foreach (string relativePath in PanelSourceFiles)
            {
                string path = Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(path))
                {
                    report.AddError("CONFIG_PANEL_SOURCE_MISSING", $"Source file missing: {relativePath}.", relativePath);
                    continue;
                }

                string text = StripCommentsAndStrings(File.ReadAllText(path));
                foreach (string token in ForbiddenSourceTokens)
                {
                    if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "CONFIG_PANEL_FORBIDDEN_FORMAL_TOKEN",
                            $"Panel source references forbidden formal data type outside comments/strings: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo(
                "CONFIG_PANEL_SOURCE_ISOLATION_SCANNED",
                "BuildSandbox Config Panel source isolation scan completed.",
                nameof(BuildSandboxConfigPanelValidator));
        }

        private static void ValidateCount(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual < expected)
            {
                report.AddError(code, $"{label} count too low. actual={actual}, expected>={expected}.", label);
                return;
            }

            report.AddInfo(code, $"{label} count pass. actual={actual}, expected>={expected}.", label);
        }

        private static void ValidateSharedFlags(
            BuildSandboxValidationReport report,
            string path,
            bool devOnly,
            bool isEnabled,
            bool formalLeak)
        {
            if (!devOnly)
            {
                report.AddError("CONFIG_PANEL_DEVONLY_FALSE", "Panel data must keep devOnly=true.", path);
            }

            if (isEnabled)
            {
                report.AddError("CONFIG_PANEL_ENABLED_TRUE", "Panel data must keep isEnabled=false.", path);
            }

            if (formalLeak)
            {
                report.AddError("CONFIG_PANEL_FORMAL_LEAK", "Panel data must not reference or enter formal product flow.", path);
            }
        }

        private static void ValidateFailureHint(BuildSandboxValidationReport report, FailureHintSeed hint)
        {
            if (hint == null)
            {
                report.AddError("CONFIG_PANEL_FAILURE_HINT_NULL", "FailureHint is null.", nameof(FailureHintSeed));
                return;
            }

            ValidateSharedFlags(
                report,
                $"FailureHint:{hint.failureHintId}",
                hint.devOnly,
                hint.isEnabled,
                hint.entersFormalFlow || hint.writesRuntimeUi);
            if (!hint.reportsOnly)
            {
                report.AddError("CONFIG_PANEL_FAILURE_HINT_NOT_REPORT_ONLY", "FailureHint must stay reportsOnly=true.", hint.failureHintId);
            }
        }

        private static string StripCommentsAndStrings(string source)
        {
            StringBuilder builder = new(source.Length);
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
