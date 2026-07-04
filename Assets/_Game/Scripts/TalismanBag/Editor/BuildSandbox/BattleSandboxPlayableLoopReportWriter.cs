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
    public static class BattleSandboxPlayableLoopReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxPlayableLoopReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxPlayableLoopRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxPlayableLoopLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxPlayableLoopSnapshot snapshot)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            BattleSandboxPlayableLoopSnapshot safeSnapshot =
                snapshot ?? new BattleSandboxPlayableLoopSnapshot();
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();

            File.WriteAllText(mainPath, BuildMainReport(safeReports, safeSnapshot), new UTF8Encoding(false));
            File.WriteAllText(rowPath, BuildRowsCsv(safeSnapshot), new UTF8Encoding(false));
            File.WriteAllText(leakPath, BuildLeakCheckReport(safeReports, safeSnapshot), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxPlayableLoopSnapshot snapshot)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Playable Loop Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxPlayableLoop.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(snapshot.Passed && errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Target scene: `Scene_TalismanBag_V04_BattleSandboxPreview` only.");
            builder.AppendLine("- Reuses `BattleSandboxRuntimeLoop01`; this package does not rewrite the battle loop.");
            builder.AppendLine("- Adds sandbox-only victory / defeat prompts, restart behavior evidence, and devOnly 3-10 / 4-10 target switching validation.");
            builder.AppendLine("- Does not connect formal RunFlow, SaveData, Reward, Drop, Chapter progress, or formal Build Settings.");
            builder.AppendLine("- Player-facing result text includes brief Build feedback but hides full answers, DropBias weights, and Boss six-key answers.");
            builder.AppendLine();
            builder.AppendLine("## Playable Loop Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Result | Evidence |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine($"| Reuse RuntimeLoop01 | `{PassFail(snapshot.reusesBattleSandboxRuntimeLoop && !snapshot.rewritesBattleLoop)}` | runtimeLoopPackage={Escape(snapshot.runtimeLoopPackageName)} |");
            builder.AppendLine($"| Runtime running | `{PassFail(snapshot.runtimeRunningStateVisible)}` | runningStateVisible={snapshot.runtimeRunningStateVisible} |");
            builder.AppendLine($"| Enemy attack timer | `{PassFail(snapshot.enemyAttackTimerAdvances)}` | attackTimerAdvances={snapshot.enemyAttackTimerAdvances} |");
            builder.AppendLine($"| Enemy attack source | `{PassFail(snapshot.enemyAttackDamageFromDevOnlyProfile)}` | devOnlyProfileDamage={snapshot.enemyAttackDamageFromDevOnlyProfile} |");
            builder.AppendLine($"| Shield before HP | `{PassFail(snapshot.playerShieldBeforeHpDamage)}` | shieldFirst={snapshot.playerShieldBeforeHpDamage} |");
            builder.AppendLine($"| HP zero allowed | `{PassFail(snapshot.runtimeAllowsPlayerHpZero)}` | runtimeAllowsPlayerHpZero={snapshot.runtimeAllowsPlayerHpZero} |");
            builder.AppendLine($"| Victory condition | `{PassFail(snapshot.victoryConditionEnemyHpZero)}` | victoryRows={snapshot.victoryRowCount}; enemy HP <= 0 |");
            builder.AppendLine($"| Defeat condition | `{PassFail(snapshot.defeatConditionPlayerHpZero)}` | defeatRows={snapshot.defeatRowCount}; player HP <= 0 |");
            builder.AppendLine($"| Result prompts | `{PassFail(snapshot.victoryPromptVisible && snapshot.defeatPromptVisible)}` | victoryPrompt={snapshot.victoryPromptVisible}; defeatPrompt={snapshot.defeatPromptVisible} |");
            builder.AppendLine($"| Build brief feedback | `{PassFail(snapshot.buildBriefFeedbackVisible)}` | buildFeedbackRows={snapshot.buildFeedbackRowCount} |");
            builder.AppendLine($"| Restart | `{PassFail(snapshot.restartRetainsCurrentBuild && snapshot.restartResetsHpShieldManaCooldownLogCast)}` | retainsBuild={snapshot.restartRetainsCurrentBuild}; resetRuntimeState={snapshot.restartResetsHpShieldManaCooldownLogCast} |");
            builder.AppendLine($"| Target switch | `{PassFail(snapshot.switchDevTargets310410)}` | 3-10={snapshot.chapter310TargetCount}; 4-10={snapshot.chapter410TargetCount} |");
            builder.AppendLine($"| Runtime rows | `{PassFail(snapshot.HasRequiredRuntimeResetCoverage)}` | mana={snapshot.runtimeManaRowCount}; cooldown={snapshot.runtimeCooldownRowCount}; log={snapshot.runtimeCombatLogRowCount}; cast={snapshot.runtimeBossCastRowCount} |");
            builder.AppendLine($"| Answer masking | `{PassFail(snapshot.playerSideAnswerLeakCount == 0)}` | leaks={snapshot.playerSideAnswerLeakCount} |");
            builder.AppendLine($"| Formal isolation | `{PassFail(snapshot.formalLeakCount == 0 && snapshot.settlementLeakCount == 0)}` | formalLeaks={snapshot.formalLeakCount}; settlementLeaks={snapshot.settlementLeakCount} |");
            builder.AppendLine();
            builder.AppendLine("## User Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("1. Open `Scene_TalismanBag_V04_BattleSandboxPreview` directly and press Play.");
            builder.AppendLine("2. Keep the current Build, enter sandbox battle mode, and let the loop reach victory or defeat.");
            builder.AppendLine("3. Confirm victory appears when enemy HP reaches 0, and defeat appears when player HP reaches 0.");
            builder.AppendLine("4. Confirm enemy attack damage first consumes player shield, then lowers player HP.");
            builder.AppendLine("5. Confirm player HP can reach 0 and is not held at 1.");
            builder.AppendLine("6. Confirm the result prompt shows only a short Build feedback line, not complete answers, DropBias weights, or Boss six-key answers.");
            builder.AppendLine("7. Click restart and confirm the same Build remains while HP, shield, mana, cooldown, log, and cast bar restart.");
            builder.AppendLine("8. Click switch target and confirm devOnly 3-10 / 4-10 test targets can be cycled without entering formal chapters.");
            builder.AppendLine("9. Confirm no formal rewards, saves, drops, chapter progress, or Build Settings changes occur.");
            builder.AppendLine();
            AppendRows(builder, snapshot);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxPlayableLoopSnapshot snapshot)
        {
            StringBuilder csv = new();
            csv.AppendLine("rowId,rowKind,scenarioStageId,devChapterLabel,targetDisplayNameChinese,enemyHpAfter,playerHpAfter,sandboxVictory,sandboxDefeat,promptVisible,buildBriefVisible,restartRetainsCurrentBuild,restartResetsRuntimeState,supportsTargetSwitch,playerSideAnswerLeak,formalFlowLeak,uiLayoutWrite,resultTitleChinese,resultBodyChinese,restartHintChinese");
            foreach (BattleSandboxPlayableLoopRow row in snapshot.rows ?? new List<BattleSandboxPlayableLoopRow>())
            {
                csv.AppendLine(Csv(
                    row.rowId,
                    row.rowKind,
                    row.scenarioStageId,
                    row.devChapterLabel,
                    row.targetDisplayNameChinese,
                    row.enemyHpAfter.ToString(),
                    row.playerHpAfter.ToString(),
                    row.sandboxVictory.ToString(),
                    row.sandboxDefeat.ToString(),
                    row.promptVisible.ToString(),
                    row.buildBriefVisible.ToString(),
                    row.restartRetainsCurrentBuild.ToString(),
                    row.restartResetsRuntimeState.ToString(),
                    row.supportsTargetSwitch.ToString(),
                    row.playerSideAnswerLeak.ToString(),
                    row.formalFlowLeak.ToString(),
                    row.uiLayoutWrite.ToString(),
                    row.resultTitleChinese,
                    row.resultBodyChinese,
                    row.restartHintChinese));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxPlayableLoopSnapshot snapshot)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            int totalLeakCount = snapshot.featureFlagDefaultTrueCount
                + snapshot.formalLeakCount
                + snapshot.settlementLeakCount
                + snapshot.playerSideAnswerLeakCount
                + snapshot.uiLayoutWriteCount
                + (snapshot.modifiesFormalBuildSettings ? 1 : 0)
                + (snapshot.touchesV02OrV03 ? 1 : 0)
                + (snapshot.touchesFormalUiLayout ? 1 : 0);

            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Playable Loop Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxPlayableLoop.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && totalLeakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected | Result |");
            builder.AppendLine("| --- | ---: | ---: | --- |");
            AppendLeakRow(builder, "featureFlagDefaultTrue", snapshot.featureFlagDefaultTrueCount);
            AppendLeakRow(builder, "formalFlowLeaks", snapshot.formalLeakCount);
            AppendLeakRow(builder, "formalSettlementLeaks", snapshot.settlementLeakCount);
            AppendLeakRow(builder, "playerSideAnswerLeaks", snapshot.playerSideAnswerLeakCount);
            AppendLeakRow(builder, "uiLayoutWrites", snapshot.uiLayoutWriteCount);
            AppendLeakRow(builder, "formalBuildSettingsWrites", snapshot.modifiesFormalBuildSettings ? 1 : 0);
            AppendLeakRow(builder, "v02OrV03Touch", snapshot.touchesV02OrV03 ? 1 : 0);
            AppendLeakRow(builder, "formalUiLayoutTouch", snapshot.touchesFormalUiLayout ? 1 : 0);
            AppendLeakRow(builder, "totalLeaks", totalLeakCount);
            builder.AppendLine();
            builder.AppendLine("## Player-Side Masking");
            builder.AppendLine();
            builder.AppendLine("- Complete answers are not shown.");
            builder.AppendLine("- DropBias weights are not shown.");
            builder.AppendLine("- Boss six-key answers are not shown.");
            builder.AppendLine("- Result feedback is limited to a short Build summary and sandbox outcome.");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- No formal RunFlow connection.");
            builder.AppendLine("- No SaveData / PlayerPrefs / MainTrialProgressData writes.");
            builder.AppendLine("- No Reward / Drop grants.");
            builder.AppendLine("- No Chapter progress.");
            builder.AppendLine("- No formal Build Settings edits.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendRows(
            StringBuilder builder,
            BattleSandboxPlayableLoopSnapshot snapshot)
        {
            builder.AppendLine("## Rows");
            builder.AppendLine();
            builder.AppendLine("| Row | Kind | Target | Enemy HP | Player HP | Prompt | Build Brief | Restart | Switch |");
            builder.AppendLine("| --- | --- | --- | ---: | ---: | --- | --- | --- | --- |");
            foreach (BattleSandboxPlayableLoopRow row in snapshot.rows ?? new List<BattleSandboxPlayableLoopRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.rowId)}` | `{Escape(row.rowKind)}` | {Escape(row.devChapterLabel)} {Escape(row.targetDisplayNameChinese)} | {row.enemyHpAfter} | {row.playerHpAfter} | `{PassFail(row.promptVisible)}` | `{PassFail(row.buildBriefVisible)}` | `{PassFail(row.restartRetainsCurrentBuild && row.restartResetsRuntimeState)}` | `{PassFail(row.supportsTargetSwitch)}` |");
            }

            builder.AppendLine();
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
            foreach (BuildSandboxValidationReport report in reports ?? Array.Empty<BuildSandboxValidationReport>())
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in (reports ?? Array.Empty<BuildSandboxValidationReport>())
                         .SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static void AppendLeakRow(StringBuilder builder, string label, int count)
        {
            builder.AppendLine($"| `{Escape(label)}` | {count} | 0 | `{(count == 0 ? "PASS" : "FAIL")}` |");
        }

        private static string PassFail(bool passed)
        {
            return passed ? "PASS" : "FAIL";
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
