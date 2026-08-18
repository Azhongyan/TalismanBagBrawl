#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.UnifiedBattle;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.UnifiedBattle
{
    public static class UnifiedBattlePageShellReportWriter
    {
        public const string ReportPath = "Docs/V0.4/Reports/UnifiedBattlePageShellReport.md";
        public const string HierarchyMapPath = "Docs/V0.4/Reports/UnifiedBattlePageShellHierarchyMap.csv";
        public const string AdapterSlotMapPath = "Docs/V0.4/Reports/UnifiedBattlePageShellAdapterSlotMap.csv";
        public const string LeakCheckReportPath = "Docs/V0.4/Reports/UnifiedBattlePageShellLeakCheckReport.md";
        public const string ManualTestPath = "Docs/V0.4/Reports/UnifiedBattlePageShellManualTest.md";

        private const string MenuRoot =
            "Tools/Talisman Bag/QA/UnifiedBattle/Run Validation Reports";
        public static void RunFromMenu()
        {
            BuildSandboxValidationReport report = UnifiedBattlePageShellVerifier.Validate(out UnifiedBattlePageShellValidationSnapshot snapshot);
            WriteReports(report, snapshot, "UNITY_EDITOR_MENU");
            AssetDatabase.Refresh();

            string status = report.Passed && snapshot.LeakCount == 0 ? "PASS" : "FAIL";
            Debug.Log($"UnifiedBattlePageShell01 validation reports written. status={status}, leakCount={snapshot.LeakCount}");
        }

        public static void WriteReports(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot,
            string validationSource)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
            string reportsDirectory = Path.Combine(projectRoot, "Docs/V0.4/Reports");
            Directory.CreateDirectory(reportsDirectory);

            File.WriteAllText(
                Path.Combine(projectRoot, ReportPath),
                BuildMainReport(report, snapshot, validationSource),
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(projectRoot, HierarchyMapPath),
                BuildHierarchyCsv(),
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(projectRoot, AdapterSlotMapPath),
                BuildAdapterSlotMapCsv(),
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(projectRoot, LeakCheckReportPath),
                BuildLeakCheckReport(report, snapshot, validationSource),
                new UTF8Encoding(false));
            File.WriteAllText(
                Path.Combine(projectRoot, ManualTestPath),
                BuildManualTestReport(report, snapshot, validationSource),
                new UTF8Encoding(false));
        }

        private static string BuildMainReport(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot,
            string validationSource)
        {
            StringBuilder builder = new();
            AppendHeader(builder, "UnifiedBattlePageShell01 Report", validationSource);
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine("- Package: V0.4-UnifiedBattlePageShell01");
            builder.AppendLine("- Purpose: define an isolated UnifiedBattlePage shell only.");
            builder.AppendLine("- Formal route connected: NO");
            builder.AppendLine("- Replaces V02/V03 battle page: NO");
            builder.AppendLine("- Starts formal battle: NO");
            builder.AppendLine("- Grants reward or writes save: NO");
            builder.AppendLine();
            builder.AppendLine("## Asset Paths");
            builder.AppendLine($"- DevOnly scene path: `{UnifiedBattlePageShellMarker.ScenePath}`");
            builder.AppendLine($"- DevOnly prefab path: `{UnifiedBattlePageShellMarker.PrefabPath}`");
            builder.AppendLine($"- Scene exists: {Bool(snapshot.SceneExists)}");
            builder.AppendLine($"- Prefab exists: {Bool(snapshot.PrefabExists)}");
            builder.AppendLine($"- Scene in BuildSettings: {Bool(snapshot.SceneInBuildSettings)}");
            builder.AppendLine();
            builder.AppendLine("## Slot Contract");
            builder.AppendLine("| Slot | Required | Notes |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (string slot in UnifiedBattlePageShellSlotNames.RequiredSlots)
            {
                string notes = slot == UnifiedBattlePageShellSlotNames.DevOnlyDiagnosticsSlot
                    ? "Developer-only diagnostics slot, separated from player-visible slots."
                    : "Required shell slot.";
                builder.AppendLine($"| `{slot}` | YES | {notes} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Sample BattleContract Binding");
            builder.AppendLine("- `BattleStartRequest`: devOnly=true, formalFlow=false, entrySource=EditorValidation.");
            builder.AppendLine("- `BattleLayoutSnapshot`: placeholder grid and item data only.");
            builder.AppendLine("- `BattleEnemySnapshot`: placeholder enemy/boss display data only.");
            builder.AppendLine("- `BuildEvaluationSnapshot`: player-safe hints plus developer-only diagnostics.");
            builder.AppendLine("- `BattleResultSnapshot`: devOnly=true, shouldWriteSave=false, shouldGrantReward=false.");
            builder.AppendLine();
            AppendValidationSummary(builder, report, snapshot);
            return builder.ToString();
        }

        private static string BuildHierarchyCsv()
        {
            StringBuilder builder = new();
            builder.AppendLine("path,activeSelf,componentTypes,source");
            foreach (UnifiedBattlePageShellHierarchyRow row in UnifiedBattlePageShellVerifier.BuildSourceStaticHierarchyRows())
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(row.Path),
                    Csv(row.ActiveSelf ? "TRUE" : "FALSE"),
                    Csv(row.ComponentTypes),
                    Csv(row.Source)));
            }

            return builder.ToString();
        }

        private static string BuildAdapterSlotMapCsv()
        {
            IReadOnlyList<(string Slot, string ContractBinding, string PlayerVisible, string DeveloperOnly, string Notes)> rows =
                new List<(string Slot, string ContractBinding, string PlayerVisible, string DeveloperOnly, string Notes)>
                {
                    (UnifiedBattlePageShellSlotNames.BattlePageRoot, "UnifiedBattlePageShellMarker;UnifiedBattlePageShell", "TRUE", "FALSE", "Root container only; devOnly marker disables formal flow."),
                    (UnifiedBattlePageShellSlotNames.BoardArea, "BattleLayoutSnapshot.gridRows/gridColumns/placedItems", "TRUE", "FALSE", "Placeholder board display only."),
                    (UnifiedBattlePageShellSlotNames.ItemTrayArea, "BattleLayoutSnapshot.placedItems", "TRUE", "FALSE", "Placeholder item tray summary only."),
                    (UnifiedBattlePageShellSlotNames.EnemyInfoArea, "BattleEnemySnapshot.displayName/health/weaknessHints", "TRUE", "FALSE", "Player-safe enemy information only."),
                    (UnifiedBattlePageShellSlotNames.BossCastBarSlot, "BattleEnemySnapshot.bossIntentLabel", "TRUE", "FALSE", "Placeholder cast label only; does not reference BossInfoPanel."),
                    (UnifiedBattlePageShellSlotNames.BattleFeedbackLayer, "BuildEvaluationSnapshot.readinessSummary/recommendedAction", "TRUE", "FALSE", "Player-safe feedback text only."),
                    (UnifiedBattlePageShellSlotNames.StoryGuidePopupLayer, "BattleStartRequest.chapterId/pageId", "TRUE", "FALSE", "Placeholder guide copy only."),
                    (UnifiedBattlePageShellSlotNames.ResultRewardPlaceholder, "BattleResultSnapshot.rewardPreview/shouldWriteSave/shouldGrantReward", "TRUE", "FALSE", "No save write and no reward grant."),
                    (UnifiedBattlePageShellSlotNames.V03FlowAdapterSlot, "none", "FALSE", "TRUE", "Empty adapter placeholder for later package; formal route disconnected."),
                    (UnifiedBattlePageShellSlotNames.V04SandboxAdapterSlot, "none", "FALSE", "TRUE", "Empty adapter placeholder for later package; sandbox route disconnected."),
                    (UnifiedBattlePageShellSlotNames.DevOnlyDiagnosticsSlot, "BuildEvaluationSnapshot.developerNotes/devOnlyDiagnostics", "FALSE", "TRUE", "Developer-only diagnostics; not player visible.")
                };

            StringBuilder builder = new();
            builder.AppendLine("slotName,contractBinding,playerVisible,developerOnly,formalRouteConnected,notes");
            foreach ((string slot, string binding, string playerVisible, string developerOnly, string notes) in rows)
            {
                builder.AppendLine(string.Join(
                    ",",
                    Csv(slot),
                    Csv(binding),
                    Csv(playerVisible),
                    Csv(developerOnly),
                    Csv("FALSE"),
                    Csv(notes)));
            }

            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot,
            string validationSource)
        {
            StringBuilder builder = new();
            AppendHeader(builder, "UnifiedBattlePageShell01 Leak Check Report", validationSource);
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine($"- Player-visible answer token leaks: {snapshot.PlayerVisibleAnswerLeakCount}");
            builder.AppendLine($"- Forbidden runtime formal-flow/save/reward/Boss references: {snapshot.ForbiddenRuntimeReferenceCount}");
            builder.AppendLine($"- Sample result devOnly violation: {(snapshot.SampleResultDevOnly ? 0 : 1)}");
            builder.AppendLine($"- Sample shouldWriteSave=true: {(snapshot.SampleResultShouldWriteSave ? 1 : 0)}");
            builder.AppendLine($"- Sample shouldGrantReward=true: {(snapshot.SampleResultShouldGrantReward ? 1 : 0)}");
            builder.AppendLine($"- Shell scene in BuildSettings: {(snapshot.SceneInBuildSettings ? 1 : 0)}");
            builder.AppendLine($"- Total leak count: {snapshot.LeakCount}");
            builder.AppendLine();
            builder.AppendLine("## Protected Areas");
            builder.AppendLine("- V02/V03 formal scenes: untouched by this writer/verifier.");
            builder.AppendLine("- Scene_TalismanBag_V04_BattleSandboxPreview: untouched by this writer/verifier.");
            builder.AppendLine("- BuildSettings/ProjectSettings: read-only validation only.");
            builder.AppendLine("- RunFlow/SaveData/RewardService/BossInfoPanel/chapter progression: no runtime references in UnifiedBattle shell source.");
            builder.AppendLine();
            AppendValidationSummary(builder, report, snapshot);
            return builder.ToString();
        }

        private static string BuildManualTestReport(
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot,
            string validationSource)
        {
            StringBuilder builder = new();
            AppendHeader(builder, "UnifiedBattlePageShell01 Manual Test", validationSource);
            builder.AppendLine();
            builder.AppendLine("## Unity Editor QA Menus");
            builder.AppendLine("- Build scene/prefab: `Tools/Talisman Bag/Authoring/UnifiedBattle/Build Shell Scene And Prefab [Writes Scene]`");
            builder.AppendLine($"- Run validation reports: `{MenuRoot}`");
            builder.AppendLine();
            builder.AppendLine("## Manual Checks");
            builder.AppendLine("1. Open the devOnly shell scene after running the build menu.");
            builder.AppendLine("2. Confirm `BattlePageRoot` exists and contains every required slot.");
            builder.AppendLine("3. Confirm `UnifiedBattlePageShellMarker` is devOnly=true, isEnabled=false, formalFlow=false, connectedToFormalRoute=false.");
            builder.AppendLine("4. Confirm sample BattleContract data is displayed without starting formal battle.");
            builder.AppendLine("5. Confirm `ResultRewardPlaceholder` displays placeholder copy only and does not grant reward or write save.");
            builder.AppendLine("6. Confirm `DevOnlyDiagnosticsSlot` is visually separated from player-visible areas.");
            builder.AppendLine("7. Confirm the shell scene is not added to BuildSettings.");
            builder.AppendLine("8. Confirm V02/V03 formal scenes, RunFlow, SaveData, RewardService, BossInfoPanel, and chapter progression remain unchanged.");
            builder.AppendLine();
            AppendValidationSummary(builder, report, snapshot);
            return builder.ToString();
        }

        private static void AppendHeader(StringBuilder builder, string title, string validationSource)
        {
            builder.AppendLine($"# {title}");
            builder.AppendLine();
            builder.AppendLine($"- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"- Validation source: {validationSource}");
            builder.AppendLine("- Package: V0.4-UnifiedBattlePageShell01");
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            BuildSandboxValidationReport report,
            UnifiedBattlePageShellValidationSnapshot snapshot)
        {
            string status = report.Passed && snapshot.LeakCount == 0 ? "PASS" : "FAIL";
            builder.AppendLine("## Validation Summary");
            builder.AppendLine($"- Status: {status}");
            builder.AppendLine($"- Validation mode: {snapshot.ValidationMode}");
            builder.AppendLine($"- Required slots: {snapshot.RequiredSlotCount}");
            builder.AppendLine($"- Code-defined slots: {snapshot.CodeDefinedSlotCount}");
            builder.AppendLine($"- Asset slots present: {snapshot.AssetSlotPresentCount}");
            builder.AppendLine($"- Sample layout items: {snapshot.SampleLayoutItemCount}");
            builder.AppendLine($"- Error count: {report.ErrorCount}");
            builder.AppendLine($"- Warning count: {report.WarningCount}");
            builder.AppendLine($"- Leak count: {snapshot.LeakCount}");
            builder.AppendLine();
            builder.AppendLine("## Issues");
            foreach (BuildSandboxValidationIssue issue in report.Issues)
            {
                builder.Append("- ");
                builder.Append(issue.Level);
                builder.Append(": ");
                builder.Append(issue.Code);
                builder.Append(" - ");
                builder.Append(issue.Message);
                if (!string.IsNullOrWhiteSpace(issue.AssetPath))
                {
                    builder.Append(" (`");
                    builder.Append(issue.AssetPath);
                    builder.Append("`)");
                }

                builder.AppendLine();
            }
        }

        private static string Bool(bool value)
        {
            return value ? "YES" : "NO";
        }

        private static string Csv(string value)
        {
            value ??= string.Empty;
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
#endif
