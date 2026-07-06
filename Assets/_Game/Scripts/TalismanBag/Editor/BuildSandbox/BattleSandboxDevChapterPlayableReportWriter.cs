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
    public static class BattleSandboxDevChapterPlayableReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxDevChapterPlayableReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxDevChapterPlayableRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxDevChapterPlayableLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxDevChapterPlayableSnapshot snapshot)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            BattleSandboxDevChapterPlayableSnapshot safeSnapshot =
                snapshot ?? new BattleSandboxDevChapterPlayableSnapshot();
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
            BattleSandboxDevChapterPlayableSnapshot snapshot)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Dev Chapter Playable Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxDevChapterPlayable.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(snapshot.Passed && errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Target scene: `Scene_TalismanBag_V04_BattleSandboxPreview` only.");
            builder.AppendLine("- Reuses `BattleSandboxRuntimeLoop01`, `BattleSandboxPlayableLoop01`, and `BattleSandboxPlayableFullRosterRegression01` evidence.");
            builder.AppendLine("- Runtime selector binding uses the existing `DevChapterDropdownSlot`; no RectTransform layout rewrite or scene binder run is required.");
            builder.AppendLine("- No formal RunFlow, StageConfig, Boss table, Reward, SaveData, Drop, Chapter progress, or V0.2/V0.3 chapter surface is touched.");
            builder.AppendLine("- Player-facing text keeps full answers, DropBias weights, and Boss six-key answers hidden.");
            builder.AppendLine();
            builder.AppendLine("## Requirement Summary");
            builder.AppendLine();
            builder.AppendLine("| Requirement | Result | Evidence |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine($"| 3-10 selectable | `{PassFail(snapshot.chapter310Selectable)}` | scenarios={snapshot.chapter310ScenarioCount} |");
            builder.AppendLine($"| 4-10 selectable | `{PassFail(snapshot.chapter410Selectable)}` | scenarios={snapshot.chapter410ScenarioCount} |");
            builder.AppendLine($"| Different enemy/Boss profile | `{PassFail(snapshot.differentBossProfileAcrossChapters)}` | distinctProfiles={snapshot.distinctBossProfileCount} |");
            builder.AppendLine($"| Different mechanic feedback | `{PassFail(snapshot.differentMechanicFeedbackAcrossChapters)}` | feedbackRows={snapshot.mechanicFeedbackRowCount} |");
            builder.AppendLine($"| Different Build pressure | `{PassFail(snapshot.differentBuildPressureAcrossChapters)}` | pressureRows={snapshot.buildPressureRowCount} |");
            builder.AppendLine($"| Victory/failure | `{PassFail(snapshot.victoryRowCount > 0 && snapshot.defeatRowCount > 0)}` | victoryRows={snapshot.victoryRowCount}; defeatRows={snapshot.defeatRowCount} |");
            builder.AppendLine($"| Restart/switch | `{PassFail(snapshot.restartSupported && snapshot.chapterSwitchSupported)}` | restart={snapshot.restartSupported}; switch={snapshot.chapterSwitchSupported} |");
            builder.AppendLine($"| Runtime reuse | `{PassFail(snapshot.reusesBattleSandboxRuntimeLoop && !snapshot.rewritesBattleLoop)}` | runtime={BattleSandboxDevChapterPlayable.RuntimeLoopPackageName} |");
            builder.AppendLine($"| PlayableLoop/FullRoster reuse | `{PassFail(snapshot.playableLoopResultPassed && snapshot.fullRosterResultPassed)}` | playable={snapshot.playableLoopResultPassed}; fullRoster={snapshot.fullRosterResultPassed} |");
            builder.AppendLine($"| Player masking | `{PassFail(snapshot.playerSideAnswerLeakCount == 0)}` | playerLeaks={snapshot.playerSideAnswerLeakCount} |");
            builder.AppendLine($"| Formal isolation | `{PassFail(snapshot.formalFlowLeakCount == 0)}` | formalLeaks={snapshot.formalFlowLeakCount}; flags={snapshot.featureFlagDefaultTrueCount} |");
            builder.AppendLine($"| UI layout preservation | `{PassFail(snapshot.uiLayoutWriteCount == 0 && snapshot.devChapterSelectorLayoutWriteCount == 0 && !snapshot.runsSceneBinder)}` | runtimeLayoutWrites={snapshot.uiLayoutWriteCount}; selectorLayoutWrites={snapshot.devChapterSelectorLayoutWriteCount}; binder={snapshot.runsSceneBinder} |");
            builder.AppendLine();
            AppendRows(builder, snapshot);
            builder.AppendLine("## User Handtest Checklist");
            builder.AppendLine();
            builder.AppendLine("1. Open `Scene_TalismanBag_V04_BattleSandboxPreview` directly and press Play.");
            builder.AppendLine("2. Click the dev chapter selector and confirm it switches between 3-10 and 4-10 without moving the UI layout.");
            builder.AppendLine("3. Start sandbox battle on 3-10 and confirm the enemy/Boss name, mechanic feedback, and Build pressure text match that level.");
            builder.AppendLine("4. Switch to 4-10 and confirm the enemy/Boss name, mechanic feedback, and Build pressure text change.");
            builder.AppendLine("5. Let one run reach victory or failure, then click restart and confirm HP, shield, mana, log, cast bar, and result state reset for the same Build.");
            builder.AppendLine("6. Switch level after a result and confirm the next run starts with the selected level.");
            builder.AppendLine("7. Confirm no full answer, DropBias weight, or Boss six-key answer appears on the player side.");
            builder.AppendLine("8. Confirm no SaveData, Reward, Drop, Chapter progress, formal RunFlow, formal StageConfig, or formal Boss table side effect occurs.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxDevChapterPlayableSnapshot snapshot)
        {
            StringBuilder csv = new();
            csv.AppendLine("rowId,stageId,devChapterLabel,targetDisplayNameChinese,bossProfileId,attackDamage,attackIntervalSeconds,enemyHpAfter,playerHpAfter,sandboxVictory,sandboxDefeat,selectable,usesDevOnlyBossProfile,promptVisible,mechanicFeedbackVisible,buildPressureVisible,restartSupported,chapterSwitchSupported,playerSideAnswerLeak,formalFlowLeak,uiLayoutWrite,mechanicFeedbackChinese,buildPressureChinese");
            foreach (BattleSandboxDevChapterPlayableRow row in snapshot.rows ?? new List<BattleSandboxDevChapterPlayableRow>())
            {
                csv.AppendLine(Csv(
                    row.rowId,
                    row.stageId,
                    row.devChapterLabel,
                    row.targetDisplayNameChinese,
                    row.bossProfileId,
                    row.attackDamage.ToString(),
                    row.attackIntervalSeconds.ToString("0.00"),
                    row.enemyHpAfter.ToString(),
                    row.playerHpAfter.ToString(),
                    row.sandboxVictory.ToString(),
                    row.sandboxDefeat.ToString(),
                    row.selectable.ToString(),
                    row.usesDevOnlyBossProfile.ToString(),
                    row.promptVisible.ToString(),
                    row.mechanicFeedbackVisible.ToString(),
                    row.buildPressureVisible.ToString(),
                    row.restartSupported.ToString(),
                    row.chapterSwitchSupported.ToString(),
                    row.playerSideAnswerLeak.ToString(),
                    row.formalFlowLeak.ToString(),
                    row.uiLayoutWrite.ToString(),
                    row.mechanicFeedbackChinese,
                    row.buildPressureChinese));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxDevChapterPlayableSnapshot snapshot)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            int totalLeakCount = snapshot.playerSideAnswerLeakCount
                + snapshot.formalFlowLeakCount
                + snapshot.featureFlagDefaultTrueCount
                + snapshot.uiLayoutWriteCount
                + snapshot.devChapterSelectorLayoutWriteCount
                + (snapshot.formalRunFlowConnected ? 1 : 0)
                + (snapshot.writesSaveData ? 1 : 0)
                + (snapshot.grantsReward ? 1 : 0)
                + (snapshot.advancesChapter ? 1 : 0)
                + (snapshot.touchesFormalStageConfig ? 1 : 0)
                + (snapshot.touchesFormalBossTable ? 1 : 0)
                + (snapshot.touchesFormalRewardTable ? 1 : 0)
                + (snapshot.touchesV02OrV03 ? 1 : 0)
                + (snapshot.runsSceneBinder ? 1 : 0);

            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Dev Chapter Playable Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxDevChapterPlayable.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && warnings == 0 && totalLeakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected | Result |");
            builder.AppendLine("| --- | ---: | ---: | --- |");
            AppendLeakRow(builder, "playerSideAnswerLeaks", snapshot.playerSideAnswerLeakCount);
            AppendLeakRow(builder, "formalFlowOrDataLeaks", snapshot.formalFlowLeakCount);
            AppendLeakRow(builder, "featureFlagDefaultTrue", snapshot.featureFlagDefaultTrueCount);
            AppendLeakRow(builder, "runtimeUiLayoutWrites", snapshot.uiLayoutWriteCount);
            AppendLeakRow(builder, "selectorRectTransformWrites", snapshot.devChapterSelectorLayoutWriteCount);
            AppendLeakRow(builder, "formalRunFlowConnected", snapshot.formalRunFlowConnected ? 1 : 0);
            AppendLeakRow(builder, "saveDataWrites", snapshot.writesSaveData ? 1 : 0);
            AppendLeakRow(builder, "rewardGrants", snapshot.grantsReward ? 1 : 0);
            AppendLeakRow(builder, "chapterProgress", snapshot.advancesChapter ? 1 : 0);
            AppendLeakRow(builder, "formalStageConfigTouch", snapshot.touchesFormalStageConfig ? 1 : 0);
            AppendLeakRow(builder, "formalBossTableTouch", snapshot.touchesFormalBossTable ? 1 : 0);
            AppendLeakRow(builder, "formalRewardTableTouch", snapshot.touchesFormalRewardTable ? 1 : 0);
            AppendLeakRow(builder, "v02OrV03Touch", snapshot.touchesV02OrV03 ? 1 : 0);
            AppendLeakRow(builder, "sceneBinderRuns", snapshot.runsSceneBinder ? 1 : 0);
            AppendLeakRow(builder, "totalLeaks", totalLeakCount);
            builder.AppendLine();
            builder.AppendLine("## Masking Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Complete answers are hidden.");
            builder.AppendLine("- DropBias weights are hidden.");
            builder.AppendLine("- Boss six-key answers are hidden.");
            builder.AppendLine("- The player side only receives level label, enemy/Boss display name, mechanic feedback, Build pressure, and short sandbox result text.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendRows(
            StringBuilder builder,
            BattleSandboxDevChapterPlayableSnapshot snapshot)
        {
            builder.AppendLine("## Rows");
            builder.AppendLine();
            builder.AppendLine("| Row | Chapter | Target | Boss Profile | Result | Mechanic | Pressure | Pass |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (BattleSandboxDevChapterPlayableRow row in snapshot.rows ?? new List<BattleSandboxDevChapterPlayableRow>())
            {
                string result = row.sandboxVictory ? "Victory" : row.sandboxDefeat ? "Defeat" : "None";
                builder.AppendLine(
                    $"| `{Escape(row.rowId)}` | `{Escape(row.devChapterLabel)}` | {Escape(row.targetDisplayNameChinese)} | `{Escape(row.bossProfileId)}` | `{result}` | `{PassFail(row.mechanicFeedbackVisible)}` | `{PassFail(row.buildPressureVisible)}` | `{PassFail(row.Passed)}` |");
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
