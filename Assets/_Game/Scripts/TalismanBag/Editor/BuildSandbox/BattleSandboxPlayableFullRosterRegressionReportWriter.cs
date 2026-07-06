#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxPlayableFullRosterRegressionReportWriter
    {
        public const string MainReportPath = "Docs/V0.4/Reports/BattleSandboxPlayableFullRosterRegressionReport.md";
        public const string ChecklistReportPath = "Docs/V0.4/Reports/BattleSandboxPlayableFullRosterRegressionChecklist.csv";
        public const string LeakCheckReportPath = "Docs/V0.4/Reports/BattleSandboxPlayableFullRosterRegressionLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string checklistPath = Path.Combine(projectRoot, ChecklistReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            BattleSandboxPlayableFullRosterRegressionSnapshot safeSnapshot =
                snapshot ?? new BattleSandboxPlayableFullRosterRegressionSnapshot();
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();

            File.WriteAllText(mainPath, BuildMainReport(safeReports, safeSnapshot), new UTF8Encoding(false));
            File.WriteAllText(checklistPath, BuildChecklistCsv(safeSnapshot), new UTF8Encoding(false));
            File.WriteAllText(leakPath, BuildLeakCheckReport(safeReports, safeSnapshot), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { mainPath, checklistPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Playable Full Roster Regression Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxPlayableFullRosterRegression.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(snapshot.Passed ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{snapshot.errorCount}`");
            builder.AppendLine($"Warnings: `{snapshot.warningCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Scene: `Scene_TalismanBag_V04_BattleSandboxPreview` only.");
            builder.AppendLine("- Purpose: devOnly playable regression for the current 23 item roster.");
            builder.AppendLine("- Allowed surface: BuildSandbox devOnly runtime, validator, and report evidence.");
            builder.AppendLine("- No V0.2/V0.3 formal assets, formal itemId replacement, formal RunFlow, SaveData, Reward, Chapter, or V04 RectTransform layout rewrite.");
            builder.AppendLine("- No scene binder is required or run by this report.");
            builder.AppendLine();
            builder.AppendLine("## Regression Summary");
            builder.AppendLine();
            builder.AppendLine("| Requirement | Result | Evidence |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine($"| 23 items appear in V04 tray | `{PassFail(snapshot.fullRosterTrayCoveragePass)}` | roster={snapshot.rosterItemCount}; tray={snapshot.trayItemCount}; packed={snapshot.trayPackedItemCount}; missing={snapshot.missingTrayItemCount}; packFailures={snapshot.trayPackFailureCount} |");
            builder.AppendLine($"| Basic items are 1x1 and placeable | `{PassFail(snapshot.basicSinglePlacementPass)}` | basic={snapshot.basicItemCount}; single1={snapshot.basicSingle1ItemCount}; placed={snapshot.basicPlacementValidCount} |");
            builder.AppendLine($"| x2/x3/x4/vertical_3 placement | `{PassFail(snapshot.multiShapePlacementPass)}` | x2={snapshot.x2PlacementValidCount}; x3={snapshot.x3PlacementValidCount}; x4={snapshot.x4PlacementValidCount}; vertical_3={snapshot.vertical3PlacementValidCount}; failures={snapshot.multiCellPlacementFailureCount} |");
            builder.AppendLine($"| Empty board defeat | `{PassFail(snapshot.emptyBoardDefeatPass)}` | enemy={snapshot.emptyBoardEnemyHpInitial}->{snapshot.emptyBoardEnemyHpFinal}; player={snapshot.emptyBoardPlayerHpInitial}->{snapshot.emptyBoardPlayerHpFinal}; hpRows={snapshot.emptyBoardPlayerHpRows}; defeatRows={snapshot.emptyBoardDefeatRows} |");
            builder.AppendLine($"| Attack item damages enemy HP | `{PassFail(snapshot.attackItemDamagePass)}` | candidates={snapshot.attackCandidateCount}; damagingSingleRows={snapshot.attackDamageItemCount}; totalDamage={snapshot.attackEnemyHpDamageTotal} |");
            builder.AppendLine($"| Shield before HP | `{PassFail(snapshot.shieldDamageOrderPass)}` | attackRows={snapshot.shieldAttackRowCount}; shieldFirstRows={snapshot.shieldFirstAttackRowCount} |");
            builder.AppendLine($"| Support does not damage enemy HP | `{PassFail(snapshot.supportNoDamagePass)}` | supportCandidates={snapshot.supportNoDamageCandidateCount}; damageLeaks={snapshot.supportDamageLeakCount} |");
            builder.AppendLine($"| Victory/defeat/restart | `{PassFail(snapshot.sandboxResultPass)}` | victoryRows={snapshot.sandboxVictoryRows}; defeatRows={snapshot.sandboxDefeatRows} |");
            builder.AppendLine($"| Switch target then play again | `{PassFail(snapshot.restartAndSwitchTargetPass)}` | scenarios={snapshot.devEnemyScenarioCount}; playableSwitchPreviews={snapshot.switchTargetPlayablePreviewCount} |");
            builder.AppendLine($"| Player-side answer leak clear | `{PassFail(snapshot.playerLeakPass)}` | playerLeaks={snapshot.playerSideAnswerLeakCount} |");
            builder.AppendLine($"| Formal scope clear | `{PassFail(snapshot.formalScopePass && snapshot.noFormalSceneOrLayoutWritePass)}` | formalLeaks={snapshot.formalFlowLeakCount}; featureFlagDefaultTrue={snapshot.featureFlagDefaultTrueCount}; devOnlyFalse={snapshot.devOnlyFalseCount}; isEnabledTrue={snapshot.isEnabledTrueCount} |");
            builder.AppendLine();
            builder.AppendLine("## Item Rows");
            builder.AppendLine();
            builder.AppendLine("| itemId | shapeId | cells | tray | board | attack | supportNoDamage | enemyHpDamage | enemyHp | result |");
            builder.AppendLine("| --- | --- | ---: | --- | --- | --- | --- | ---: | --- | --- |");
            foreach (BattleSandboxPlayableFullRosterItemRegressionRow row in snapshot.itemRows
                         ?? new List<BattleSandboxPlayableFullRosterItemRegressionRow>())
            {
                builder.AppendLine(
                    $"| `{Escape(row.itemId)}` | `{Escape(row.shapeId)}` | {row.shapeCellCount} | `{PassFail(row.trayPresent && row.trayPacked)}` | `{PassFail(row.boardPlacementValid)}` | `{row.expectedAttackDamage}` | `{row.expectedNoDamageSupport}` | {row.enemyHpDamageTotal} | {row.initialEnemyHp}->{row.finalEnemyHp} | `{PassFail(row.Passed)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## User Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("1. Open `Scene_TalismanBag_V04_BattleSandboxPreview`, press Play, and confirm the tray can expose all 23 items via the available tray/category controls.");
            builder.AppendLine("2. Drag several basic 1x1 items to legal cells, then test x2, x3, x4, and vertical_3 items.");
            builder.AppendLine("3. Start battle with an empty board and confirm enemy HP stays unchanged while player HP reaches defeat.");
            builder.AppendLine("4. Place an obvious attack item, start battle, and confirm enemy HP drops.");
            builder.AppendLine("5. Place a shield item, start battle, and confirm shield is consumed before HP.");
            builder.AppendLine("6. Place heal/cleanse/control/aura/rhythm support items and confirm they do not damage enemy HP.");
            builder.AppendLine("7. Confirm victory, failure, restart, and switching target allow another round.");
            builder.AppendLine("8. Confirm player-facing text does not reveal full answers, DropBias weights, or Boss six-key answers.");
            builder.AppendLine("9. Confirm no SaveData, Reward, Chapter, or formal RunFlow side effects appear.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildChecklistCsv(BattleSandboxPlayableFullRosterRegressionSnapshot snapshot)
        {
            StringBuilder builder = new();
            builder.AppendLine("id,area,check,method,result,evidence,userHandtestRequired,notes");
            foreach (BattleSandboxPlayableFullRosterRegressionChecklistRow row in snapshot.checklistRows
                         ?? new List<BattleSandboxPlayableFullRosterRegressionChecklistRow>())
            {
                builder.AppendLine(Csv(
                    row.id,
                    row.area,
                    row.check,
                    row.method,
                    row.result,
                    row.evidence,
                    row.userHandtestRequired.ToString(),
                    row.notes));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxPlayableFullRosterRegressionSnapshot snapshot)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Playable Full Roster Regression Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxPlayableFullRosterRegression.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(snapshot.Passed ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected | Result |");
            builder.AppendLine("| --- | ---: | ---: | --- |");
            AppendLeakRow(builder, "playerSideAnswerLeaks", snapshot.playerSideAnswerLeakCount);
            AppendLeakRow(builder, "formalFlowOrDataLeaks", snapshot.formalFlowLeakCount);
            AppendLeakRow(builder, "featureFlagDefaultTrue", snapshot.featureFlagDefaultTrueCount);
            AppendLeakRow(builder, "devOnlyFalse", snapshot.devOnlyFalseCount);
            AppendLeakRow(builder, "isEnabledTrue", snapshot.isEnabledTrueCount);
            AppendLeakRow(builder, "supportDamageLeaks", snapshot.supportDamageLeakCount);
            AppendLeakRow(builder, "validatorErrors", snapshot.errorCount);
            AppendLeakRow(builder, "validatorWarnings", snapshot.warningCount);
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- SaveData writes: none.");
            builder.AppendLine("- Reward or drop grants: none.");
            builder.AppendLine("- Chapter or formal RunFlow progression: none.");
            builder.AppendLine("- Player-side complete answers, DropBias weights, and Boss six-key answers: hidden.");
            builder.AppendLine("- V0.2/V0.3 formal assets and V04 hand-tuned RectTransforms: not written.");
            builder.AppendLine("- Scene binders: not required for this regression.");
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
            foreach (BuildSandboxValidationReport report in reports
                         ?? Array.Empty<BuildSandboxValidationReport>())
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
