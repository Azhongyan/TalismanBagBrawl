#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class LedgerTaskBuildHooksValidator
    {
        private static readonly string[] RequiredGoalIds =
        {
            "ledger_goal_activate_synergy_2",
            "ledger_goal_activate_synergy_4",
            "ledger_goal_activate_synergy_6",
            "ledger_goal_thunder_defeat_shield_boss",
            "ledger_goal_cleanse_negative_status",
            "ledger_goal_complete_specified_build_validation",
            "ledger_goal_obtain_orange_core_affix",
            "ledger_goal_complete_affix_reroll"
        };

        private static readonly string[] SourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/LedgerTaskBuildHooks.cs"
        };

        private static readonly string[] ForbiddenFormalReferenceTokens =
        {
            "V02RunFlowController",
            "V02FormationGridFrame",
            "DamageText",
            "PageState",
            "FormationState",
            "MainTrialProgressData",
            "PlayerPrefs",
            "SaveData",
            "RewardConfig",
            "UpgradeConfig",
            "BossReward",
            "FormalDrop",
            "FormalForge",
            "TaskReward",
            "DailyTask",
            "QuestProgress"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Ledger Task Build Hooks");
            ValidateInto(report);
            return report;
        }

        public static void ValidateInto(BuildSandboxValidationReport report)
        {
            if (report == null)
            {
                return;
            }

            BuildPreview(
                out LedgerBuildTaskHook hook,
                out List<LedgerBuildTaskEvent> events,
                out List<LedgerBuildTaskProgressPreview> progress);
            ValidateHook(report, hook);
            ValidateGoals(report, hook?.taskGoals);
            ValidateEvents(report, events);
            ValidateProgress(report, hook?.taskGoals, progress);
            ValidateFeatureFlags(report);
            ValidateSourceIsolation(report);
        }

        public static void BuildPreview(
            out LedgerBuildTaskHook hook,
            out List<LedgerBuildTaskEvent> events,
            out List<LedgerBuildTaskProgressPreview> progress)
        {
            hook = LedgerTaskBuildHooksPreview.CreateDefaultHook();
            ModifierEventBridgeValidator.BuildSamplePreview(
                out BuildEvaluationResult buildEvaluation,
                out AffixRarityEvaluationResult affixEvaluation,
                out _,
                out _);
            BuildSimulationBenchmarkReport benchmark =
                DevChapterContentPoolValidator.BuildDevChapterSimulationBenchmark();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            DevChapterContentPool devChapterPool = DevChapterContentPool.CreateDefault();
            events = LedgerTaskBuildHooksPreview.BuildEvents(
                buildEvaluation,
                benchmark,
                affixEvaluation,
                enemyBossPool,
                devChapterPool);
            progress = LedgerTaskBuildHooksPreview.PreviewProgress(hook, events);
        }

        private static void ValidateHook(BuildSandboxValidationReport report, LedgerBuildTaskHook hook)
        {
            if (hook == null)
            {
                report.AddError("LEDGER_TASK_HOOK_NULL", "LedgerBuildTaskHook preview is missing.", nameof(LedgerBuildTaskHook));
                return;
            }

            if (string.IsNullOrWhiteSpace(hook.hookId))
            {
                report.AddError("LEDGER_TASK_HOOK_ID_MISSING", "LedgerBuildTaskHook hookId is empty.", nameof(LedgerBuildTaskHook));
            }

            ValidateIsolation(
                report,
                "LEDGER_TASK_HOOK",
                hook.devOnly,
                hook.isEnabled,
                hook.entersFormalFlow,
                hook.grantsFormalReward,
                hook.entersFormalTaskList,
                hook.readsFormalPlayerProgress,
                nameof(LedgerBuildTaskHook));

            if (hook.taskGoals == null || hook.taskGoals.Count < RequiredGoalIds.Length)
            {
                report.AddError(
                    "LEDGER_TASK_GOAL_COUNT_LOW",
                    $"LedgerTaskBuildHooks01 requires at least {RequiredGoalIds.Length} task goals; actual={hook.taskGoals?.Count ?? 0}.",
                    nameof(LedgerBuildTaskGoal));
            }

            report.AddInfo(
                "LEDGER_TASK_HOOK_SCANNED",
                $"hookId={hook.hookId}, goals={hook.taskGoals?.Count ?? 0}, devOnly={hook.devOnly}, isEnabled={hook.isEnabled}.",
                nameof(LedgerBuildTaskHook));
        }

        private static void ValidateGoals(
            BuildSandboxValidationReport report,
            IReadOnlyList<LedgerBuildTaskGoal> goals)
        {
            IReadOnlyList<LedgerBuildTaskGoal> safeGoals = goals ?? Array.Empty<LedgerBuildTaskGoal>();
            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (LedgerBuildTaskGoal goal in safeGoals)
            {
                string path = $"{nameof(LedgerBuildTaskGoal)}:{goal?.taskGoalId}";
                if (goal == null)
                {
                    report.AddError("LEDGER_TASK_GOAL_NULL", "Ledger task goal is null.", nameof(LedgerBuildTaskGoal));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(goal.taskGoalId))
                {
                    report.AddError("LEDGER_TASK_GOAL_ID_MISSING", "taskGoalId is empty.", path);
                }
                else if (!ids.Add(goal.taskGoalId))
                {
                    report.AddError("LEDGER_TASK_GOAL_ID_DUPLICATE", $"Duplicate taskGoalId: {goal.taskGoalId}.", path);
                }

                if (string.IsNullOrWhiteSpace(goal.chineseDescription))
                {
                    report.AddError("LEDGER_TASK_GOAL_DESCRIPTION_MISSING", "Chinese goal description is empty.", path);
                }

                if (!LedgerBuildTaskGoalTypes.All.Contains(goal.goalType))
                {
                    report.AddError("LEDGER_TASK_GOAL_TYPE_INVALID", $"Unsupported goalType: {goal.goalType}.", path);
                }

                if (!LedgerBuildTaskEventSources.All.Contains(goal.sourceEvent))
                {
                    report.AddError("LEDGER_TASK_GOAL_SOURCE_INVALID", $"Unsupported sourceEvent: {goal.sourceEvent}.", path);
                }

                if (goal.requiredValue <= 0)
                {
                    report.AddError("LEDGER_TASK_GOAL_REQUIRED_VALUE_INVALID", "requiredValue must be positive.", path);
                }

                ValidateIsolation(
                    report,
                    "LEDGER_TASK_GOAL",
                    goal.devOnly,
                    goal.isEnabled,
                    goal.entersFormalFlow,
                    goal.grantsFormalReward,
                    goal.entersFormalTaskList,
                    goal.readsFormalPlayerProgress,
                    path);

                report.AddInfo(
                    "LEDGER_TASK_GOAL_SCANNED",
                    $"{goal.taskGoalId}: {goal.chineseDescription}, source={goal.sourceEvent}, required={goal.requiredValue}.",
                    path);
            }

            foreach (string requiredGoalId in RequiredGoalIds)
            {
                if (ids.Contains(requiredGoalId))
                {
                    report.AddInfo("LEDGER_TASK_GOAL_REQUIRED_PRESENT", $"Required task goal present: {requiredGoalId}.", nameof(LedgerBuildTaskGoal));
                    continue;
                }

                report.AddError("LEDGER_TASK_GOAL_REQUIRED_MISSING", $"Required task goal missing: {requiredGoalId}.", nameof(LedgerBuildTaskGoal));
            }
        }

        private static void ValidateEvents(
            BuildSandboxValidationReport report,
            IReadOnlyList<LedgerBuildTaskEvent> events)
        {
            if (events == null || events.Count == 0)
            {
                report.AddError(
                    "LEDGER_TASK_EVENT_PREVIEW_EMPTY",
                    "Ledger task hooks must consume BuildSandbox event previews.",
                    nameof(LedgerBuildTaskEvent));
                return;
            }

            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (LedgerBuildTaskEvent taskEvent in events)
            {
                string path = $"{nameof(LedgerBuildTaskEvent)}:{taskEvent?.eventId}";
                if (taskEvent == null)
                {
                    report.AddError("LEDGER_TASK_EVENT_NULL", "Ledger task event is null.", nameof(LedgerBuildTaskEvent));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(taskEvent.eventId))
                {
                    report.AddError("LEDGER_TASK_EVENT_ID_MISSING", "eventId is empty.", path);
                }
                else if (!ids.Add(taskEvent.eventId))
                {
                    report.AddError("LEDGER_TASK_EVENT_ID_DUPLICATE", $"Duplicate eventId: {taskEvent.eventId}.", path);
                }

                if (!LedgerBuildTaskEventSources.All.Contains(taskEvent.sourceEvent))
                {
                    report.AddError("LEDGER_TASK_EVENT_SOURCE_INVALID", $"Unsupported sourceEvent: {taskEvent.sourceEvent}.", path);
                }

                if (!taskEvent.devOnly)
                {
                    report.AddError("LEDGER_TASK_EVENT_DEVONLY_FALSE", "Event preview must keep devOnly=true.", path);
                }

                if (taskEvent.isEnabled)
                {
                    report.AddError("LEDGER_TASK_EVENT_ENABLED_TRUE", "Event preview must keep isEnabled=false.", path);
                }

                if (taskEvent.entersFormalFlow
                    || taskEvent.grantsFormalReward
                    || taskEvent.readsFormalPlayerProgress)
                {
                    report.AddError("LEDGER_TASK_EVENT_FORMAL_LEAK", "Event preview must not enter product flow, grant product reward, or read product player progress.", path);
                }
            }

            foreach (string source in LedgerBuildTaskEventSources.All)
            {
                bool present = events.Any(taskEvent =>
                    string.Equals(taskEvent?.sourceEvent, source, StringComparison.Ordinal));
                if (present)
                {
                    report.AddInfo("LEDGER_TASK_EVENT_SOURCE_PRESENT", $"Event source preview present: {source}.", nameof(LedgerBuildTaskEvent));
                    continue;
                }

                report.AddWarning("LEDGER_TASK_EVENT_SOURCE_EMPTY", $"No preview event generated for source: {source}.", nameof(LedgerBuildTaskEvent));
            }
        }

        private static void ValidateProgress(
            BuildSandboxValidationReport report,
            IReadOnlyList<LedgerBuildTaskGoal> goals,
            IReadOnlyList<LedgerBuildTaskProgressPreview> progress)
        {
            IReadOnlyList<LedgerBuildTaskGoal> safeGoals = goals ?? Array.Empty<LedgerBuildTaskGoal>();
            IReadOnlyList<LedgerBuildTaskProgressPreview> safeProgress =
                progress ?? Array.Empty<LedgerBuildTaskProgressPreview>();
            if (safeProgress.Count < safeGoals.Count)
            {
                report.AddError(
                    "LEDGER_TASK_PROGRESS_COUNT_LOW",
                    $"Progress preview should include every task goal. goals={safeGoals.Count}, progress={safeProgress.Count}.",
                    nameof(LedgerBuildTaskProgressPreview));
            }

            foreach (LedgerBuildTaskGoal goal in safeGoals)
            {
                LedgerBuildTaskProgressPreview preview = safeProgress.FirstOrDefault(item =>
                    string.Equals(item?.taskGoalId, goal.taskGoalId, StringComparison.Ordinal));
                string path = $"{nameof(LedgerBuildTaskProgressPreview)}:{goal.taskGoalId}";
                if (preview == null)
                {
                    report.AddError("LEDGER_TASK_PROGRESS_MISSING", $"Missing progress preview for {goal.taskGoalId}.", path);
                    continue;
                }

                ValidateIsolation(
                    report,
                    "LEDGER_TASK_PROGRESS",
                    preview.devOnly,
                    preview.isEnabled,
                    preview.entersFormalFlow,
                    preview.grantsFormalReward,
                    preview.entersFormalTaskList,
                    false,
                    path);

                if (preview.matchedEventIds == null || preview.matchedEventIds.Count == 0)
                {
                    report.AddWarning("LEDGER_TASK_PROGRESS_NO_MATCH", $"No sandbox event currently completes {goal.taskGoalId}; hook remains configurable.", path);
                    continue;
                }

                report.AddInfo(
                    "LEDGER_TASK_PROGRESS_MATCHED",
                    $"{goal.taskGoalId} observed={preview.observedValue}, required={preview.requiredValue}, completed={preview.completed}.",
                    path);
            }
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                if (flag.DefaultValue)
                {
                    report.AddError(
                        "LEDGER_TASK_FEATURE_FLAG_TRUE",
                        $"{flag.Key} must stay false for LedgerTaskBuildHooks01.",
                        nameof(BuildSandboxFeatureFlags));
                    continue;
                }

                report.AddInfo("LEDGER_TASK_FEATURE_FLAG_FALSE", $"{flag.Key}=false.", nameof(BuildSandboxFeatureFlags));
            }
        }

        private static void ValidateSourceIsolation(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetCurrentDirectory();
            foreach (string relativePath in SourceFiles)
            {
                string path = Path.Combine(projectRoot, relativePath);
                if (!File.Exists(path))
                {
                    report.AddError("LEDGER_TASK_SOURCE_FILE_MISSING", $"Source file missing: {relativePath}.", relativePath);
                    continue;
                }

                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenFormalReferenceTokens)
                {
                    if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "LEDGER_TASK_FORBIDDEN_FORMAL_TOKEN",
                            $"Source references forbidden product-system token: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo(
                "LEDGER_TASK_SOURCE_ISOLATION_SCANNED",
                "Ledger task hook source scan completed.",
                nameof(LedgerBuildTaskHook));
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            string codePrefix,
            bool devOnly,
            bool isEnabled,
            bool entersFormalFlow,
            bool grantsFormalReward,
            bool entersFormalTaskList,
            bool readsFormalPlayerProgress,
            string path)
        {
            if (!devOnly)
            {
                report.AddError($"{codePrefix}_DEVONLY_FALSE", "Ledger task hook data must keep devOnly=true.", path);
            }

            if (isEnabled)
            {
                report.AddError($"{codePrefix}_ENABLED_TRUE", "Ledger task hook data must keep isEnabled=false.", path);
            }

            if (entersFormalFlow || entersFormalTaskList)
            {
                report.AddError($"{codePrefix}_FORMAL_FLOW_LEAK", "Ledger task hook data must not enter product task flow.", path);
            }

            if (grantsFormalReward)
            {
                report.AddError($"{codePrefix}_FORMAL_REWARD_LEAK", "Ledger task hook data must not grant product rewards.", path);
            }

            if (readsFormalPlayerProgress)
            {
                report.AddError($"{codePrefix}_PLAYER_PROGRESS_LEAK", "Ledger task hook data must not read product player task progress.", path);
            }
        }
    }
}
#endif
