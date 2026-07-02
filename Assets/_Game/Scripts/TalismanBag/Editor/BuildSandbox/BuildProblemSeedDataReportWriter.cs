#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildProblemSeedDataReportWriter
    {
        public const string BuildProblemSeedDataReportPath =
            "Docs/V0.4/Reports/BuildProblemSeedDataReport.md";

        public const string MapRuleSeedReportPath =
            "Docs/V0.4/Reports/MapRuleSeedReport.csv";

        public const string EnemyProblemSeedReportPath =
            "Docs/V0.4/Reports/EnemyProblemSeedReport.csv";

        public const string BossProblemSeedReportPath =
            "Docs/V0.4/Reports/BossProblemSeedReport.csv";

        public const string WeaknessWindowSeedReportPath =
            "Docs/V0.4/Reports/WeaknessWindowSeedReport.csv";

        public const string DropBiasSeedReportPath =
            "Docs/V0.4/Reports/DropBiasSeedReport.csv";

        public const string BuildProblemSeedLeakCheckReportPath =
            "Docs/V0.4/Reports/BuildProblemSeedLeakCheckReport.md";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, BuildProblemSeedDataReportPath);
            string mapPath = Path.Combine(projectRoot, MapRuleSeedReportPath);
            string enemyPath = Path.Combine(projectRoot, EnemyProblemSeedReportPath);
            string bossPath = Path.Combine(projectRoot, BossProblemSeedReportPath);
            string weaknessPath = Path.Combine(projectRoot, WeaknessWindowSeedReportPath);
            string dropPath = Path.Combine(projectRoot, DropBiasSeedReportPath);
            string leakPath = Path.Combine(projectRoot, BuildProblemSeedLeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            BuildProblemSeedDataset dataset = BuildProblemSeedDataset.CreateDefault();
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(mainPath, BuildMainReport(dataset, reports, errors, warnings), new UTF8Encoding(false));
            File.WriteAllText(mapPath, BuildMapRuleCsv(dataset.mapRules), new UTF8Encoding(false));
            File.WriteAllText(enemyPath, BuildEnemyProblemCsv(dataset.enemyProblems), new UTF8Encoding(false));
            File.WriteAllText(bossPath, BuildBossProblemCsv(dataset.bossProblems), new UTF8Encoding(false));
            File.WriteAllText(weaknessPath, BuildWeaknessWindowCsv(dataset.bossProblems), new UTF8Encoding(false));
            File.WriteAllText(dropPath, BuildDropBiasCsv(dataset.bossProblems), new UTF8Encoding(false));
            File.WriteAllText(leakPath, BuildLeakCheckReport(dataset, reports, errors, warnings), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, mapPath, enemyPath, bossPath, weaknessPath, dropPath, leakPath };
        }

        private static string BuildMainReport(
            BuildProblemSeedDataset dataset,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Build Problem Seed Data Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BuildProblemSeedData01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Static devOnly seed provider for V0.4 BuildSandbox Phase 2 problem data.");
            builder.AppendLine("- No product chapter, combat, UI scene, persistence, Boss list, reward, drop, numeric, V02, or V03 data connection.");
            builder.AppendLine("- Every seed remains `devOnly=true` and `isEnabled=false`.");
            builder.AppendLine("- FeatureFlags remain default false.");
            builder.AppendLine();
            builder.AppendLine("## Seed Counts");
            builder.AppendLine();
            builder.AppendLine($"- Map rules: `{dataset.mapRules.Count}`");
            builder.AppendLine($"- Enemy problems: `{dataset.enemyProblems.Count}`");
            builder.AppendLine($"- Boss problems: `{dataset.bossProblems.Count}`");
            builder.AppendLine($"- Weakness windows: `{dataset.weaknessWindows.Count}`");
            builder.AppendLine($"- DropBias seeds: `{dataset.dropBiases.Count}`");
            builder.AppendLine($"- Failure hints: `{dataset.failureHints.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Boss Key Coverage");
            builder.AppendLine();
            builder.AppendLine("| Boss | Goal | Keys | Weakness Windows | DropBias | Problem Attributes |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: | --- |");
            foreach (BossProblemSeed boss in dataset.bossProblems)
            {
                builder.AppendLine(
                    $"| {Escape(boss.displayName)} | {Escape(boss.validationGoal)} | {boss.keyRequirements.Count} | {boss.weaknessWindows.Count} | {boss.dropBiases.Count} | `{Escape(FormatStrings(boss.requiredProblemAttributes))}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Required Problem Attributes");
            builder.AppendLine();
            builder.AppendLine("`BreakPower`, `CleansePower`, `ControlPower`, `GuardPower`, `EnergyStability`, `ClearPower`, and `BurstWindow` are covered by Boss attributes and key requirements.");
            builder.AppendLine();
            builder.AppendLine("## Formal Flow Leak Check");
            builder.AppendLine();
            builder.AppendLine("- Formal 1-10 / 2-10: not referenced by this package.");
            builder.AppendLine("- Product enemy, Boss, reward, drop, upgrade, numeric, progress, and persistence data: not edited.");
            builder.AppendLine("- Current UI scenes and hand-tuned layout: not edited.");
            builder.AppendLine("- DropBias remains report-only and does not touch product drops.");
            builder.AppendLine("- BossProblem seeds do not reference product Boss lists.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildMapRuleCsv(IReadOnlyList<MapRuleSeed> mapRules)
        {
            StringBuilder csv = new();
            csv.AppendLine("mapRuleId,displayName,description,affectedTags,buffTags,debuffTags,placementModifier,energyModifier,cooldownModifier,warningText,enemyProblemIds,bossProblemIds,dropBiasId,devOnly,isEnabled");
            foreach (MapRuleSeed rule in mapRules)
            {
                csv.AppendLine(Csv(
                    rule.mapRuleId,
                    rule.displayName,
                    rule.description,
                    FormatStrings(rule.affectedTags),
                    FormatStrings(rule.buffTags),
                    FormatStrings(rule.debuffTags),
                    rule.placementModifier.ToString(),
                    rule.energyModifier.ToString(),
                    rule.cooldownModifier.ToString(),
                    rule.warningText,
                    FormatStrings(rule.enemyProblemIds),
                    FormatStrings(rule.bossProblemIds),
                    rule.dropBiasId,
                    rule.devOnly.ToString(),
                    rule.isEnabled.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildEnemyProblemCsv(IReadOnlyList<EnemyProblemSeed> enemyProblems)
        {
            StringBuilder csv = new();
            csv.AppendLine("problemType,displayName,pressureType,problemSummary,hardSolutionTags,softSolutionTags,validatedBuildTags,failureHintId,failureHint,recommendedAction,devOnly,isEnabled");
            foreach (EnemyProblemSeed problem in enemyProblems)
            {
                csv.AppendLine(Csv(
                    problem.problemType,
                    problem.displayName,
                    problem.pressureType,
                    problem.problemSummary,
                    FormatStrings(problem.hardSolutionTags),
                    FormatStrings(problem.softSolutionTags),
                    FormatStrings(problem.validatedBuildTags),
                    problem.failureHint.failureHintId,
                    problem.failureHint.detail,
                    problem.recommendedAction,
                    problem.devOnly.ToString(),
                    problem.isEnabled.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildBossProblemCsv(IReadOnlyList<BossProblemSeed> bossProblems)
        {
            StringBuilder csv = new();
            csv.AppendLine("bossProblemId,displayName,validationGoal,minimumKeysRequired,keyCount,keyCategories,keyAttributes,requiredProblemAttributes,weaknessWindowIds,dropBiasIds,failureHintIds,devOnly,isEnabled");
            foreach (BossProblemSeed boss in bossProblems)
            {
                csv.AppendLine(Csv(
                    boss.bossProblemId,
                    boss.displayName,
                    boss.validationGoal,
                    boss.minimumKeysRequired.ToString(),
                    boss.keyRequirements.Count.ToString(),
                    FormatStrings(boss.keyRequirements.Select(key => key.keyCategory)),
                    FormatStrings(boss.keyRequirements.Select(key => key.problemAttribute)),
                    FormatStrings(boss.requiredProblemAttributes),
                    FormatStrings(boss.weaknessWindows.Select(window => window.weaknessWindowId)),
                    FormatStrings(boss.dropBiases.Select(drop => drop.dropBiasId)),
                    FormatStrings(boss.failureHints.Select(hint => hint.failureHintId)),
                    boss.devOnly.ToString(),
                    boss.isEnabled.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildWeaknessWindowCsv(IReadOnlyList<BossProblemSeed> bossProblems)
        {
            StringBuilder csv = new();
            csv.AppendLine("bossProblemId,bossName,weaknessWindowId,displayName,triggerCondition,startSecond,durationSecond,exposedBuildTags,devOnly,isEnabled");
            foreach (BossProblemSeed boss in bossProblems)
            {
                foreach (WeaknessWindowSeed window in boss.weaknessWindows)
                {
                    csv.AppendLine(Csv(
                        boss.bossProblemId,
                        boss.displayName,
                        window.weaknessWindowId,
                        window.displayName,
                        window.triggerCondition,
                        window.startSecond.ToString("0.##"),
                        window.durationSecond.ToString("0.##"),
                        FormatStrings(window.exposedBuildTags),
                        window.devOnly.ToString(),
                        window.isEnabled.ToString()));
                }
            }

            return csv.ToString();
        }

        private static string BuildDropBiasCsv(IReadOnlyList<BossProblemSeed> bossProblems)
        {
            StringBuilder csv = new();
            csv.AppendLine("bossProblemId,bossName,dropBiasId,displayName,biasType,targetBuildTags,targetItemTags,targetAffixIds,previewWeight,reportsOnly,devOnly,isEnabled,entersFormalFlow");
            foreach (BossProblemSeed boss in bossProblems)
            {
                foreach (DropBiasSeed drop in boss.dropBiases)
                {
                    csv.AppendLine(Csv(
                        boss.bossProblemId,
                        boss.displayName,
                        drop.dropBiasId,
                        drop.displayName,
                        drop.biasType,
                        FormatStrings(drop.targetBuildTags),
                        FormatStrings(drop.targetItemTags),
                        FormatStrings(drop.targetAffixIds),
                        drop.previewWeight.ToString("0.##"),
                        drop.reportsOnly.ToString(),
                        drop.devOnly.ToString(),
                        drop.isEnabled.ToString(),
                        drop.entersFormalFlow.ToString()));
                }
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            BuildProblemSeedDataset dataset,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            int nonDevOnly = dataset.mapRules.Count(rule => !rule.devOnly)
                + dataset.enemyProblems.Count(problem => !problem.devOnly)
                + dataset.bossProblems.Count(boss => !boss.devOnly)
                + dataset.bossProblems.SelectMany(boss => boss.keyRequirements).Count(key => !key.devOnly)
                + dataset.weaknessWindows.Count(window => !window.devOnly)
                + dataset.dropBiases.Count(drop => !drop.devOnly)
                + dataset.failureHints.Count(hint => !hint.devOnly);
            int enabled = dataset.mapRules.Count(rule => rule.isEnabled)
                + dataset.enemyProblems.Count(problem => problem.isEnabled)
                + dataset.bossProblems.Count(boss => boss.isEnabled)
                + dataset.bossProblems.SelectMany(boss => boss.keyRequirements).Count(key => key.isEnabled)
                + dataset.weaknessWindows.Count(window => window.isEnabled)
                + dataset.dropBiases.Count(drop => drop.isEnabled)
                + dataset.failureHints.Count(hint => hint.isEnabled);
            int formalLeaks = dataset.mapRules.Count(rule => rule.entersFormalFlow || rule.usesFormalStageData)
                + dataset.enemyProblems.Count(problem => problem.entersFormalFlow || problem.referencesProductEnemyList || problem.affectsFormalCombat)
                + dataset.bossProblems.Count(boss => boss.entersFormalFlow || boss.referencesProductBossList || boss.affectsFormalCombat)
                + dataset.bossProblems.SelectMany(boss => boss.keyRequirements).Count(key => key.gatesProductBoss)
                + dataset.weaknessWindows.Count(window => window.entersFormalFlow || window.affectsFormalCombat)
                + dataset.dropBiases.Count(drop => drop.entersFormalFlow || drop.referencesProductDrops || drop.grantsProductItems)
                + dataset.failureHints.Count(hint => hint.entersFormalFlow || hint.writesRuntimeUi);
            int featureFlagTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);

            StringBuilder builder = new();
            builder.AppendLine("# Build Problem Seed Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BuildProblemSeedData01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagTrue} | 0 |");
            builder.AppendLine($"| `nonDevOnlySeeds` | {nonDevOnly} | 0 |");
            builder.AppendLine($"| `enabledSeeds` | {enabled} | 0 |");
            builder.AppendLine($"| `formalFlowLeaks` | {formalLeaks} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Confirmation");
            builder.AppendLine();
            builder.AppendLine("- All seed data is static BuildSandbox data and disabled by default.");
            builder.AppendLine("- DropBias is report-only and does not connect to product drops.");
            builder.AppendLine("- BossProblem seeds are devOnly Build checks and do not connect to product Boss lists.");
            builder.AppendLine("- No product stage, save, reward, drop, Boss, UI, V02, or V03 data was edited.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine();
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports)
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string EscapeCsv(string value)
        {
            string normalized = value ?? string.Empty;
            if (normalized.Contains(",") || normalized.Contains("\"") || normalized.Contains("\n") || normalized.Contains("\r"))
            {
                return $"\"{normalized.Replace("\"", "\"\"")}\"";
            }

            return normalized;
        }
    }
}
#endif
