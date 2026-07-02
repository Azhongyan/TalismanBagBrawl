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
    public static class BuildProblemRulePoolReportWriter
    {
        public const string BuildProblemRulePoolReportPath =
            "Docs/V0.4/Reports/BuildProblemRulePoolReport.md";

        public const string MapRuleSchemaReportPath =
            "Docs/V0.4/Reports/MapRuleSchemaReport.csv";

        public const string EnemyProblemSchemaReportPath =
            "Docs/V0.4/Reports/EnemyProblemSchemaReport.csv";

        public const string BossProblemKeySchemaReportPath =
            "Docs/V0.4/Reports/BossProblemKeySchemaReport.csv";

        public const string BuildReadinessSchemaReportPath =
            "Docs/V0.4/Reports/BuildReadinessSchemaReport.md";

        public const string DropBiasLeakCheckReportPath =
            "Docs/V0.4/Reports/DropBiasLeakCheckReport.md";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, BuildProblemRulePoolReportPath);
            string mapPath = Path.Combine(projectRoot, MapRuleSchemaReportPath);
            string enemyPath = Path.Combine(projectRoot, EnemyProblemSchemaReportPath);
            string bossPath = Path.Combine(projectRoot, BossProblemKeySchemaReportPath);
            string readinessPath = Path.Combine(projectRoot, BuildReadinessSchemaReportPath);
            string dropPath = Path.Combine(projectRoot, DropBiasLeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(mainPath, BuildMainReport(reports, errors, warnings), new UTF8Encoding(false));
            File.WriteAllText(mapPath, BuildSchemaCsv("MapRuleConfig", MapRuleSchemaRows()), new UTF8Encoding(false));
            File.WriteAllText(enemyPath, BuildSchemaCsv("EnemyProblemConfig", EnemyProblemSchemaRows()), new UTF8Encoding(false));
            File.WriteAllText(bossPath, BuildSchemaCsv("BossProblemConfig", BossProblemSchemaRows()), new UTF8Encoding(false));
            File.WriteAllText(readinessPath, BuildReadinessMarkdown(reports, errors), new UTF8Encoding(false));
            File.WriteAllText(dropPath, BuildDropBiasLeakMarkdown(reports, errors), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, mapPath, enemyPath, bossPath, readinessPath, dropPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Build Problem Rule Pool Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BuildProblemRulePool01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Rule schema layer only for V0.4 BuildSandbox Phase 2.");
            builder.AppendLine("- No formal chapter, combat connection, visible page, scene edit, product data edit, reward edit, drop edit, Boss edit, numeric edit, or persistence edit.");
            builder.AppendLine("- All new config types default to `devOnly=true` and `isEnabled=false`.");
            builder.AppendLine("- FeatureFlags remain default false.");
            builder.AppendLine();
            builder.AppendLine("## Added Config Types");
            builder.AppendLine();
            builder.AppendLine("| Type | Purpose | Default Isolation |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine("| `MapRuleConfig` | Declares how a map rule asks a Build problem. | `devOnly=true`, `isEnabled=false` |");
            builder.AppendLine("| `EnemyProblemConfig` | Declares how an enemy exposes a Build gap. | `devOnly=true`, `isEnabled=false` |");
            builder.AppendLine("| `BossProblemConfig` | Declares a multi-key Boss Build lock. | `devOnly=true`, `isEnabled=false` |");
            builder.AppendLine("| `BossProblemKeyRequirement` | Serializable key requirement inside Boss problem configs. | `devOnly=true`, `isEnabled=false` |");
            builder.AppendLine("| `BuildReadinessCheckConfig` | Reports readiness before a problem is attempted. | `devOnly=true`, `isEnabled=false`, `reportsOnly=true` |");
            builder.AppendLine("| `WeaknessWindowConfig` | Describes a devOnly weakness window for reports and later preview. | `devOnly=true`, `isEnabled=false` |");
            builder.AppendLine("| `DropBiasConfig` | Describes devOnly drop tendency for future seed data. | `devOnly=true`, `isEnabled=false`, `reportsOnly=true` |");
            builder.AppendLine("| `FailureHintConfig` | Describes report-only failure hints. | `devOnly=true`, `isEnabled=false`, `reportsOnly=true` |");
            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Status");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{Escape(flag.Key)}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

            AppendValidationSummary(builder, reports);
            builder.AppendLine();
            builder.AppendLine("## Formal Flow Leak Check");
            builder.AppendLine();
            builder.AppendLine("- Formal 1-10 / 2-10 flow: not referenced by this package.");
            builder.AppendLine("- Product enemy, Boss, reward, drop, upgrade, numeric, progress, and persistence data: not edited.");
            builder.AppendLine("- Current UI scenes and hand-tuned layout: not edited.");
            builder.AppendLine("- BuildReadiness output is report-only and does not write product panels.");
            builder.AppendLine("- DropBias output is report-only and does not touch product drop tables.");
            builder.AppendLine();
            builder.AppendLine("## Next SeedData Gate");
            builder.AppendLine();
            builder.AppendLine($"- `V0.4-BuildProblemSeedData01` should add `{BuildProblemRulePoolValidator.NextSeedMapRuleCount}` map rule seeds.");
            builder.AppendLine($"- `V0.4-BuildProblemSeedData01` should add `{BuildProblemRulePoolValidator.NextSeedEnemyProblemCount}` enemy problem seeds.");
            builder.AppendLine($"- `V0.4-BuildProblemSeedData01` should add `{BuildProblemRulePoolValidator.NextSeedBossProblemCount}` Boss problem seeds.");
            builder.AppendLine("- Seed data must remain devOnly and disabled until a later Guard assignment changes the boundary.");
            return builder.ToString();
        }

        private static string BuildSchemaCsv(string configType, IReadOnlyList<SchemaRow> rows)
        {
            StringBuilder csv = new();
            csv.AppendLine("configType,fieldName,fieldType,defaultValue,purpose,isolationRule");
            foreach (SchemaRow row in rows)
            {
                csv.AppendLine(Csv(
                    configType,
                    row.FieldName,
                    row.FieldType,
                    row.DefaultValue,
                    row.Purpose,
                    row.IsolationRule));
            }

            return csv.ToString();
        }

        private static string BuildReadinessMarkdown(IReadOnlyList<BuildSandboxValidationReport> reports, int errors)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Build Readiness Schema Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BuildProblemRulePool01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## BuildReadinessCheckConfig");
            AppendSchemaTable(builder, BuildReadinessSchemaRows());
            builder.AppendLine();
            builder.AppendLine("## WeaknessWindowConfig");
            AppendSchemaTable(builder, WeaknessWindowSchemaRows());
            builder.AppendLine();
            builder.AppendLine("## FailureHintConfig");
            AppendSchemaTable(builder, FailureHintSchemaRows());
            builder.AppendLine();
            builder.AppendLine("## Isolation");
            builder.AppendLine();
            builder.AppendLine("- Readiness checks default to report-only.");
            builder.AppendLine("- Weakness windows default to simulator-readable preview data only.");
            builder.AppendLine("- Failure hints default to report-only copy and do not write product UI.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildDropBiasLeakMarkdown(IReadOnlyList<BuildSandboxValidationReport> reports, int errors)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Drop Bias Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.4-BuildProblemRulePool01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## DropBiasConfig Schema");
            AppendSchemaTable(builder, DropBiasSchemaRows());
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Expected | Result |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine("| `featureFlagDefaultTrue` | 0 | 0 when validation PASS |");
            builder.AppendLine("| `dropBiasTouchesProductDropTable` | 0 | 0 when validation PASS |");
            builder.AppendLine("| `dropBiasGrantsProductReward` | 0 | 0 when validation PASS |");
            builder.AppendLine("| `dropBiasEntersFormalFlow` | 0 | 0 when validation PASS |");
            builder.AppendLine();
            builder.AppendLine("## Confirmation");
            builder.AppendLine();
            builder.AppendLine("- DropBias is a devOnly tendency schema for later seed data.");
            builder.AppendLine("- This package creates no product drop table asset and edits no product reward data.");
            builder.AppendLine("- Future seed data must keep `devOnly=true` and `isEnabled=false`.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendSchemaTable(StringBuilder builder, IReadOnlyList<SchemaRow> rows)
        {
            builder.AppendLine("| Field | Type | Default | Purpose | Isolation |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (SchemaRow row in rows)
            {
                builder.AppendLine(
                    $"| `{Escape(row.FieldName)}` | `{Escape(row.FieldType)}` | `{Escape(row.DefaultValue)}` | {Escape(row.Purpose)} | {Escape(row.IsolationRule)} |");
            }
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

        private static IReadOnlyList<SchemaRow> MapRuleSchemaRows()
        {
            return new[]
            {
                Row("mapRuleId", "string", "map_rule_schema", "Unique map rule id.", "devOnly seed id only."),
                Row("mapRuleType", "string", "problem_map", "Rule category for problem selection.", "No product stage hookup."),
                Row("targetBuildTags", "List<string>", "empty", "Build tags required by the map problem.", "Schema only."),
                Row("enemyProblemIds", "List<string>", "empty", "Enemy problems attached to this map rule.", "devOnly references only."),
                Row("bossProblemIds", "List<string>", "empty", "Boss problems attached to this map rule.", "devOnly references only."),
                Row("readinessCheckIds", "List<string>", "empty", "Readiness checks used by this map rule.", "Report-only checks."),
                Row("weaknessWindowIds", "List<string>", "empty", "Weakness windows used by this map rule.", "Preview-only windows."),
                Row("dropBiasId", "string", "empty", "Optional devOnly drop tendency id.", "No product drop edit."),
                Row("devOnly", "bool", "true", "BuildSandbox isolation flag.", "Must stay true."),
                Row("isEnabled", "bool", "false", "Activation gate.", "Must stay false.")
            };
        }

        private static IReadOnlyList<SchemaRow> EnemyProblemSchemaRows()
        {
            return new[]
            {
                Row("enemyProblemId", "string", "enemy_problem_schema", "Unique enemy problem id.", "devOnly seed id only."),
                Row("enemyRole", "string", "problem_enemy", "Enemy role used to expose a Build gap.", "Not a product enemy definition."),
                Row("exposureType", "string", "build_gap", "How the gap is exposed.", "Report and preview only."),
                Row("targetBuildGap", "string", "missing_counter", "Named Build gap under test.", "Schema only."),
                Row("targetBuildTags", "List<string>", "empty", "Build tags challenged by the enemy.", "devOnly tags only."),
                Row("recommendedSynergyIds", "List<string>", "empty", "Synergies recommended by the problem.", "devOnly references only."),
                Row("weaknessWindowIds", "List<string>", "empty", "Linked weakness windows.", "Preview-only windows."),
                Row("readinessCheckIds", "List<string>", "empty", "Checks needed before this problem.", "Report-only checks."),
                Row("failureHintIds", "List<string>", "empty", "Failure hints shown in reports.", "No product UI write."),
                Row("devOnly", "bool", "true", "BuildSandbox isolation flag.", "Must stay true."),
                Row("isEnabled", "bool", "false", "Activation gate.", "Must stay false.")
            };
        }

        private static IReadOnlyList<SchemaRow> BossProblemSchemaRows()
        {
            return new[]
            {
                Row("bossProblemId", "string", "boss_problem_schema", "Unique Boss problem id.", "devOnly seed id only."),
                Row("bossLockType", "string", "multi_key_lock", "How the Boss checks Build completeness.", "Not product Boss logic."),
                Row("keyRequirements", "List<BossProblemKeyRequirement>", "empty", "Multi-key requirements.", "devOnly and disabled."),
                Row("minimumKeysRequired", "int", "1", "Minimum solved keys for readiness.", "Schema only."),
                Row("targetBuildTags", "List<string>", "empty", "Build tags checked by the Boss.", "devOnly tags only."),
                Row("readinessCheckIds", "List<string>", "empty", "Readiness checks used by the Boss.", "Report-only checks."),
                Row("weaknessWindowIds", "List<string>", "empty", "Weakness windows used by the Boss.", "Preview-only windows."),
                Row("failureHintIds", "List<string>", "empty", "Failure hints used by reports.", "No product UI write."),
                Row("BossProblemKeyRequirement.devOnly", "bool", "true", "Nested key isolation.", "Must stay true."),
                Row("BossProblemKeyRequirement.isEnabled", "bool", "false", "Nested key activation gate.", "Must stay false.")
            };
        }

        private static IReadOnlyList<SchemaRow> BuildReadinessSchemaRows()
        {
            return new[]
            {
                Row("readinessCheckId", "string", "readiness_check_schema", "Unique readiness check id.", "devOnly seed id only."),
                Row("checkType", "string", "tag_threshold", "Readiness rule type.", "Report-only."),
                Row("requiredBuildTags", "List<string>", "empty", "Tags required to solve the problem.", "Schema only."),
                Row("requiredSynergyIds", "List<string>", "empty", "Synergies required to solve the problem.", "Schema only."),
                Row("requiredScore", "int", "1", "Pass score.", "Report-only."),
                Row("failureHintIds", "List<string>", "empty", "Hints used when readiness fails.", "No product UI write."),
                Row("reportsOnly", "bool", "true", "Prevents runtime product panel writes.", "Must stay true.")
            };
        }

        private static IReadOnlyList<SchemaRow> WeaknessWindowSchemaRows()
        {
            return new[]
            {
                Row("weaknessWindowId", "string", "weakness_window_schema", "Unique weakness window id.", "devOnly seed id only."),
                Row("windowType", "string", "timed_vulnerability", "Window category.", "Preview-only."),
                Row("startSecond", "float", "0", "Preview start time.", "No product combat timing write."),
                Row("durationSecond", "float", "3", "Preview window duration.", "No product combat timing write."),
                Row("exposedBuildTags", "List<string>", "empty", "Tags exposed by the weakness.", "Schema only."),
                Row("recommendedSynergyIds", "List<string>", "empty", "Synergies recommended in the window.", "Schema only.")
            };
        }

        private static IReadOnlyList<SchemaRow> FailureHintSchemaRows()
        {
            return new[]
            {
                Row("failureHintId", "string", "failure_hint_schema", "Unique failure hint id.", "devOnly seed id only."),
                Row("hintType", "string", "missing_build_key", "Hint category.", "Report-only."),
                Row("priority", "int", "1", "Hint ordering.", "Report-only."),
                Row("headline", "string", "Build readiness hint", "Short report heading.", "No product UI write."),
                Row("detail", "string", "devOnly failure hint schema.", "Long report detail.", "No product UI write."),
                Row("recommendedBuildTags", "List<string>", "empty", "Recommended tags.", "Schema only."),
                Row("reportsOnly", "bool", "true", "Prevents runtime product UI writes.", "Must stay true.")
            };
        }

        private static IReadOnlyList<SchemaRow> DropBiasSchemaRows()
        {
            return new[]
            {
                Row("dropBiasId", "string", "drop_bias_schema", "Unique drop bias id.", "devOnly seed id only."),
                Row("biasType", "string", "dev_only_problem_support", "Bias category.", "Report-only."),
                Row("targetBuildTags", "List<string>", "empty", "Build tags the bias supports.", "Schema only."),
                Row("targetItemTags", "List<string>", "empty", "Item tags the bias supports.", "Schema only."),
                Row("targetAffixIds", "List<string>", "empty", "Affixes the bias supports.", "Schema only."),
                Row("previewWeight", "float", "1", "Preview tendency weight.", "No product drop edit."),
                Row("reportsOnly", "bool", "true", "Prevents product data writes.", "Must stay true."),
                Row("touchesProductDropTable", "bool", "false", "Leak guard.", "Must stay false."),
                Row("grantsProductReward", "bool", "false", "Leak guard.", "Must stay false.")
            };
        }

        private static SchemaRow Row(
            string fieldName,
            string fieldType,
            string defaultValue,
            string purpose,
            string isolationRule)
        {
            return new SchemaRow(fieldName, fieldType, defaultValue, purpose, isolationRule);
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

        private readonly struct SchemaRow
        {
            public SchemaRow(
                string fieldName,
                string fieldType,
                string defaultValue,
                string purpose,
                string isolationRule)
            {
                FieldName = fieldName;
                FieldType = fieldType;
                DefaultValue = defaultValue;
                Purpose = purpose;
                IsolationRule = isolationRule;
            }

            public string FieldName { get; }
            public string FieldType { get; }
            public string DefaultValue { get; }
            public string Purpose { get; }
            public string IsolationRule { get; }
        }
    }
}
#endif
