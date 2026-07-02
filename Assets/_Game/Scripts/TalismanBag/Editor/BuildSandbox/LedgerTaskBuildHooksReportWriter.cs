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
    public static class LedgerTaskBuildHooksReportWriter
    {
        public const string LedgerTaskBuildHooksReportPath =
            "Docs/V0.4/Reports/LedgerTaskBuildHooksReport.md";

        public const string LedgerBuildTaskGoalReportPath =
            "Docs/V0.4/Reports/LedgerBuildTaskGoalReport.csv";

        public const string LedgerBuildTaskLeakCheckReportPath =
            "Docs/V0.4/Reports/LedgerBuildTaskLeakCheckReport.md";

        public static string[] WriteReports(IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string hookPath = Path.Combine(projectRoot, LedgerTaskBuildHooksReportPath);
            string goalPath = Path.Combine(projectRoot, LedgerBuildTaskGoalReportPath);
            string leakPath = Path.Combine(projectRoot, LedgerBuildTaskLeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(hookPath) ?? projectRoot);

            LedgerTaskBuildHooksValidator.BuildPreview(
                out LedgerBuildTaskHook hook,
                out List<LedgerBuildTaskEvent> events,
                out List<LedgerBuildTaskProgressPreview> progress);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            File.WriteAllText(
                hookPath,
                BuildHookMarkdown(hook, events, progress, reports, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                goalPath,
                BuildGoalCsv(hook, progress),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckMarkdown(hook, events, progress, reports, errors, warnings),
                new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { hookPath, goalPath, leakPath };
        }

        private static string BuildHookMarkdown(
            LedgerBuildTaskHook hook,
            IReadOnlyList<LedgerBuildTaskEvent> events,
            IReadOnlyList<LedgerBuildTaskProgressPreview> progress,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Ledger Task Build Hooks Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-LedgerTaskBuildHooks01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine("Data label: `devOnly / disabled / event preview only / not product task flow`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Dev-only Build task hooks reserved for future ledger / growth manual systems.");
            builder.AppendLine("- No product task UI, product reward, product task list, product player progress, product home UI, RunFlow, PageState, FormationState, save, boss, reward, drop, numeric, DamageText, or V02FormationGridFrame connection.");
            builder.AppendLine("- Event inputs come only from BuildSandbox previews: BuildEvaluationResult, BuildSimulationResult, Affix preview, EnemyBossValidationPool result, and DevChapterContentPool result.");
            builder.AppendLine("- Every hook and progress preview remains `devOnly=true`, `isEnabled=false`, `entersFormalFlow=false`, and `grantsFormalReward=false`.");
            builder.AppendLine();
            builder.AppendLine("## Hook Flags");
            builder.AppendLine();
            builder.AppendLine("| hookId | devOnly | isEnabled | entersFormalFlow | grantsFormalReward | entersFormalTaskList | readsFormalPlayerProgress |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            builder.AppendLine(
                $"| `{Escape(hook.hookId)}` | `{hook.devOnly}` | `{hook.isEnabled}` | `{hook.entersFormalFlow}` | `{hook.grantsFormalReward}` | `{hook.entersFormalTaskList}` | `{hook.readsFormalPlayerProgress}` |");
            builder.AppendLine();
            builder.AppendLine("## Task Goals");
            builder.AppendLine();
            builder.AppendLine("| taskGoalId | 中文目标描述 | goalType | sourceEvent | requiredValue | devOnly | isEnabled | entersFormalFlow | grantsFormalReward | 是否进入正式任务列表 |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | --- | --- | --- | --- | --- |");
            foreach (LedgerBuildTaskGoal goal in hook.taskGoals)
            {
                builder.AppendLine(
                    $"| `{Escape(goal.taskGoalId)}` | {Escape(goal.chineseDescription)} | `{Escape(goal.goalType)}` | `{Escape(goal.sourceEvent)}` | {goal.requiredValue} | `{goal.devOnly}` | `{goal.isEnabled}` | `{goal.entersFormalFlow}` | `{goal.grantsFormalReward}` | `{goal.entersFormalTaskList}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Event Preview");
            builder.AppendLine();
            builder.AppendLine("| eventId | sourceEvent | buildId | synergyId | threshold | bossId | affixId | rarityId | tags | devOnly | isEnabled | formalFlow | reward | playerProgress |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (LedgerBuildTaskEvent taskEvent in events)
            {
                builder.AppendLine(
                    $"| `{Escape(taskEvent.eventId)}` | `{Escape(taskEvent.sourceEvent)}` | `{Escape(taskEvent.buildId)}` | `{Escape(taskEvent.synergyId)}` | {taskEvent.thresholdPieceCount} | `{Escape(taskEvent.bossProfileId)}` | `{Escape(taskEvent.affixId)}` | `{Escape(taskEvent.rarityId)}` | `{Escape(FormatStrings(taskEvent.tags))}` | `{taskEvent.devOnly}` | `{taskEvent.isEnabled}` | `{taskEvent.entersFormalFlow}` | `{taskEvent.grantsFormalReward}` | `{taskEvent.readsFormalPlayerProgress}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Progress Preview");
            builder.AppendLine();
            builder.AppendLine("| taskGoalId | observedValue | requiredValue | completed | matchedEvents | devOnly | isEnabled | formalFlow | reward | formalTaskList |");
            builder.AppendLine("| --- | ---: | ---: | --- | --- | --- | --- | --- | --- | --- |");
            foreach (LedgerBuildTaskProgressPreview item in progress)
            {
                builder.AppendLine(
                    $"| `{Escape(item.taskGoalId)}` | {item.observedValue} | {item.requiredValue} | `{item.completed}` | `{Escape(FormatStrings(item.matchedEventIds))}` | `{item.devOnly}` | `{item.isEnabled}` | `{item.entersFormalFlow}` | `{item.grantsFormalReward}` | `{item.entersFormalTaskList}` |");
            }

            AppendValidationSummary(builder, reports);
            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Check");
            builder.AppendLine();
            builder.AppendLine("- Product ledger / growth manual UI: not implemented.");
            builder.AppendLine("- Product task list: not connected.");
            builder.AppendLine("- Product rewards: not granted.");
            builder.AppendLine("- Product player task progress: not read or written.");
            builder.AppendLine("- Home UI / current UI scene / hand-tuned layout: not touched.");
            builder.AppendLine("- RunFlow / PageState / FormationState / Save / Boss / reward / numeric systems: not touched.");
            builder.AppendLine("- FeatureFlags: remain false.");
            builder.AppendLine();
            builder.AppendLine("## User Check");
            builder.AppendLine();
            builder.AppendLine("- Confirm every goal is marked devOnly and disabled.");
            builder.AppendLine("- Confirm reports say the goals do not enter product task list and do not grant product rewards.");
            builder.AppendLine("- Confirm FeatureFlags remain false and current UI hand tuning is unaffected.");
            return builder.ToString();
        }

        private static string BuildGoalCsv(
            LedgerBuildTaskHook hook,
            IReadOnlyList<LedgerBuildTaskProgressPreview> progress)
        {
            StringBuilder csv = new();
            csv.AppendLine("taskGoalId,chineseDescription,goalType,sourceEvent,requiredValue,observedValue,completed,devOnly,isEnabled,entersFormalFlow,grantsFormalReward,entersFormalTaskList,formalTaskListEntry");
            foreach (LedgerBuildTaskGoal goal in hook.taskGoals)
            {
                LedgerBuildTaskProgressPreview preview = progress.FirstOrDefault(item =>
                    string.Equals(item.taskGoalId, goal.taskGoalId, StringComparison.Ordinal));
                csv.AppendLine(Csv(
                    goal.taskGoalId,
                    goal.chineseDescription,
                    goal.goalType,
                    goal.sourceEvent,
                    goal.requiredValue.ToString(),
                    preview?.observedValue.ToString() ?? "0",
                    preview?.completed.ToString() ?? "False",
                    goal.devOnly.ToString(),
                    goal.isEnabled.ToString(),
                    goal.entersFormalFlow.ToString(),
                    goal.grantsFormalReward.ToString(),
                    goal.entersFormalTaskList.ToString(),
                    "false"));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckMarkdown(
            LedgerBuildTaskHook hook,
            IReadOnlyList<LedgerBuildTaskEvent> events,
            IReadOnlyList<LedgerBuildTaskProgressPreview> progress,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            int enabledGoals = hook.taskGoals.Count(goal => goal.isEnabled);
            int nonDevOnlyGoals = hook.taskGoals.Count(goal => !goal.devOnly);
            int formalTaskGoals = hook.taskGoals.Count(goal => goal.entersFormalFlow || goal.entersFormalTaskList);
            int formalRewardGoals = hook.taskGoals.Count(goal => goal.grantsFormalReward);
            int playerProgressReads = hook.taskGoals.Count(goal => goal.readsFormalPlayerProgress);
            int formalEventLeaks = events.Count(item =>
                item.entersFormalFlow || item.grantsFormalReward || item.readsFormalPlayerProgress || item.isEnabled || !item.devOnly);
            int formalProgressLeaks = progress.Count(item =>
                item.entersFormalFlow || item.entersFormalTaskList || item.grantsFormalReward || item.isEnabled || !item.devOnly);

            StringBuilder builder = new();
            builder.AppendLine("# Ledger Build Task Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-LedgerTaskBuildHooks01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `enabledGoals` | {enabledGoals} | 0 |");
            builder.AppendLine($"| `nonDevOnlyGoals` | {nonDevOnlyGoals} | 0 |");
            builder.AppendLine($"| `formalTaskGoals` | {formalTaskGoals} | 0 |");
            builder.AppendLine($"| `formalRewardGoals` | {formalRewardGoals} | 0 |");
            builder.AppendLine($"| `playerProgressReads` | {playerProgressReads} | 0 |");
            builder.AppendLine($"| `formalEventLeaks` | {formalEventLeaks} | 0 |");
            builder.AppendLine($"| `formalProgressLeaks` | {formalProgressLeaks} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Per Goal Flags");
            builder.AppendLine();
            builder.AppendLine("| taskGoalId | devOnly | isEnabled | entersFormalFlow | grantsFormalReward | entersFormalTaskList | readsFormalPlayerProgress |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (LedgerBuildTaskGoal goal in hook.taskGoals)
            {
                builder.AppendLine(
                    $"| `{Escape(goal.taskGoalId)}` | `{goal.devOnly}` | `{goal.isEnabled}` | `{goal.entersFormalFlow}` | `{goal.grantsFormalReward}` | `{goal.entersFormalTaskList}` | `{goal.readsFormalPlayerProgress}` |");
            }

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

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
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
