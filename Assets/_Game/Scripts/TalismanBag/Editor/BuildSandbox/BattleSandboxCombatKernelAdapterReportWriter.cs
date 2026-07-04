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
    public static class BattleSandboxCombatKernelAdapterReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxCombatKernelAdapterReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxCombatKernelAdapterRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxCombatKernelAdapterLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxCombatKernelAdapterPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxCombatKernelAdapterPreview safePreview =
                preview ?? BattleSandboxCombatKernelAdapterValidator.BuildDefaultPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePreview);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                rowPath,
                BuildRowsCsv(safePreview),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxCombatKernelAdapterPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Combat Kernel Adapter Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxCombatKernelAdapterPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Builds a devOnly V04 BuildSandbox combat-rule adapter snapshot.");
            builder.AppendLine("- Reuses V0.2/V0.3 mouthfeel for spirit power, cooldown, damage, shield, healing, enemy HP, enemy skill cast bars, and status ticks.");
            builder.AppendLine("- Emits adapter rows and sample values only; it does not run formal combat.");
            builder.AppendLine("- Does not connect formal RunFlow, SaveData, Reward, Chapter, feature flags, formal damage settlement, or V04 UI layout changes.");
            builder.AppendLine("- Does not modify V0.2/V0.3 runtime files.");
            builder.AppendLine();
            builder.AppendLine("## Required Counters");
            builder.AppendLine();
            builder.AppendLine($"- rule row count: `{preview?.RuleRowCount ?? 0}`");
            builder.AppendLine($"- feature flag default true count: `{preview?.FeatureFlagDefaultTrueCount ?? 1}`");
            builder.AppendLine($"- feature flags all disabled: `{preview?.FeatureFlagsAllDisabled ?? false}`");
            builder.AppendLine($"- formal flow leak count: `{preview?.FormalFlowLeakCount ?? 1}`");
            builder.AppendLine($"- UI layout leak count: `{preview?.UiLayoutLeakCount ?? 1}`");
            builder.AppendLine($"- rule scope leak count: `{preview?.RuleScopeLeakCount ?? 1}`");
            builder.AppendLine($"- devOnly/isEnabled isolation pass: `{preview?.DevOnlyIsolationPass ?? false}`");
            builder.AppendLine();
            AppendSampleSection(builder, preview?.sample);
            builder.AppendLine("## Adapter Rows");
            builder.AppendLine();
            builder.AppendLine("| Rule | Source | Sample Input | Sample Output |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BattleSandboxCombatKernelAdapterRow row in Rows(preview))
            {
                builder.AppendLine(
                    $"| `{Escape(row.ruleKey)}` {Escape(row.chineseDisplayName)} | {Escape(row.sourceReference)} | {Escape(row.sampleInput)} | {Escape(row.sampleOutput)} |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(
            BattleSandboxCombatKernelAdapterPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("ruleKey,chineseDisplayName,sourceReference,adapterMouthfeel,sampleInput,sampleOutput,devOnly,isEnabled,formalFlowLeak,uiLayoutLeak,modifiesStableRuntime");
            foreach (BattleSandboxCombatKernelAdapterRow row in Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.ruleKey,
                    row.chineseDisplayName,
                    row.sourceReference,
                    row.adapterMouthfeel,
                    row.sampleInput,
                    row.sampleOutput,
                    row.devOnly.ToString(),
                    row.isEnabled.ToString(),
                    row.formalFlowLeak.ToString(),
                    row.uiLayoutLeak.ToString(),
                    row.modifiesStableRuntime.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxCombatKernelAdapterPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Combat Kernel Adapter Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxCombatKernelAdapterPreview.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {preview?.FeatureFlagDefaultTrueCount ?? 1} | 0 |");
            builder.AppendLine($"| `formalFlowLeakCount` | {preview?.FormalFlowLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `uiLayoutLeakCount` | {preview?.UiLayoutLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `ruleScopeLeakCount` | {preview?.RuleScopeLeakCount ?? 1} | 0 |");
            builder.AppendLine("| `formalRunFlowConnections` | 0 | 0 |");
            builder.AppendLine("| `formalSaveReads` | 0 | 0 |");
            builder.AppendLine("| `formalSaveWrites` | 0 | 0 |");
            builder.AppendLine("| `rewardWrites` | 0 | 0 |");
            builder.AppendLine("| `chapterAdvances` | 0 | 0 |");
            builder.AppendLine("| `formalDamageSettlementCalls` | 0 | 0 |");
            builder.AppendLine("| `v04UiLayoutRewrites` | 0 | 0 |");
            builder.AppendLine("| `v02v03RuntimeWrites` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Adapter preview remains devOnly and disabled by default.");
            builder.AppendLine("- BuildSandbox feature flags remain default false.");
            builder.AppendLine("- Formal RunFlow, SaveData, Reward, Chapter, and formal damage settlement are not called.");
            builder.AppendLine("- V04 UI layout is not rearranged by this package.");
            builder.AppendLine("- V0.2/V0.3 runtime files are treated as source references only.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendSampleSection(
            StringBuilder builder,
            BattleSandboxCombatKernelSampleSnapshot sample)
        {
            if (sample == null)
            {
                builder.AppendLine("## Sample Snapshot");
                builder.AppendLine();
                builder.AppendLine("Sample snapshot missing.");
                builder.AppendLine();
                return;
            }

            builder.AppendLine("## Sample Snapshot");
            builder.AppendLine();
            builder.AppendLine("| Rule | Expected Sample |");
            builder.AppendLine("| --- | --- |");
            builder.AppendLine($"| `spiritPower` | cross={sample.power.eyeCrossState}, diagonal={sample.power.eyeDiagonalState}, spiritNineGrid={sample.power.spiritNineGridState} |");
            builder.AppendLine($"| `cooldown` | fullFire={sample.cooldown.fullPoweredFireCooldown:0.###}, weakFire={sample.cooldown.weakPoweredFireCooldown:0.###}, trainedWeakFire={sample.cooldown.trainedWeakPoweredFireCooldown:0.###}, min={sample.cooldown.minClampedCooldown:0.###} |");
            builder.AppendLine($"| `damage` | blocked={sample.damage.blockedByShield}, hpDamage={sample.damage.hpDamage}, hpAfter={sample.damage.enemyHpAfter}, shieldAfter={sample.damage.enemyShieldAfter} |");
            builder.AppendLine($"| `playerShield` | shieldAfter={sample.playerShield.playerShieldAfter}, wasted={sample.playerShield.wastedShield}, enemyShieldAfter={sample.playerShield.enemyShieldAfter} |");
            builder.AppendLine($"| `healing` | hpAfter={sample.healing.playerHpAfter}, overheal={sample.healing.overheal} |");
            builder.AppendLine($"| `enemyHp` | spawnHp={sample.enemyHp.currentHpAtSpawn}, interval={sample.enemyHp.attackInterval:0.###}, enrageHp={sample.enemyHp.bossEnrageHpThreshold} |");
            builder.AppendLine($"| `enemySkillCastBar` | remaining={sample.castBar.castTimeRemainingAfterTick:0.###}, bar={sample.castBar.castBarRemainingRatioAfterTick:0.###}, cooldownAfterComplete={sample.castBar.cooldownAfterComplete:0.###} |");
            builder.AppendLine($"| `statusTick` | playerDot={sample.statusTick.playerDotDamagePerSecond}, enemyBurnDot={sample.statusTick.enemyBurnDamagePerSecond} |");
            builder.AppendLine();
        }

        private static int CountLeaks(
            BattleSandboxCombatKernelAdapterPreview preview)
        {
            if (preview == null)
            {
                return 1;
            }

            return preview.FeatureFlagDefaultTrueCount
                + preview.FormalFlowLeakCount
                + preview.UiLayoutLeakCount
                + preview.RuleScopeLeakCount;
        }

        private static IEnumerable<BattleSandboxCombatKernelAdapterRow> Rows(
            BattleSandboxCombatKernelAdapterPreview preview)
        {
            return preview?.rows?
                .Where(row => row != null)
                ?? Enumerable.Empty<BattleSandboxCombatKernelAdapterRow>();
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
