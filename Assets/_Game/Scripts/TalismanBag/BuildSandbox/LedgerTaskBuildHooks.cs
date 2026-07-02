using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    public static class LedgerBuildTaskGoalTypes
    {
        public const string ActivateSynergyThreshold = "activate_synergy_threshold";
        public const string DefeatShieldBossWithBuild = "defeat_shield_boss_with_build";
        public const string CleanseNegativeStatusWithBuild = "cleanse_negative_status_with_build";
        public const string CompleteBuildValidation = "complete_build_validation";
        public const string ObtainOrangeCoreAffix = "obtain_orange_core_affix";
        public const string CompleteAffixReroll = "complete_affix_reroll";

        public static readonly string[] All =
        {
            ActivateSynergyThreshold,
            DefeatShieldBossWithBuild,
            CleanseNegativeStatusWithBuild,
            CompleteBuildValidation,
            ObtainOrangeCoreAffix,
            CompleteAffixReroll
        };
    }

    public static class LedgerBuildTaskEventSources
    {
        public const string BuildEvaluationResult = "BuildEvaluationResult";
        public const string BuildSimulationResult = "BuildSimulationResult";
        public const string AffixPreview = "AffixPreview";
        public const string EnemyBossValidationPoolResult = "EnemyBossValidationPoolResult";
        public const string DevChapterContentPoolResult = "DevChapterContentPoolResult";

        public static readonly string[] All =
        {
            BuildEvaluationResult,
            BuildSimulationResult,
            AffixPreview,
            EnemyBossValidationPoolResult,
            DevChapterContentPoolResult
        };
    }

    [Serializable]
    public sealed class LedgerBuildTaskHook
    {
        public string hookId = "ledger_build_task_hooks_dev_preview";
        public string packageName = "V0.3/V0.4-BuildSandbox-LedgerTaskBuildHooks01";
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool grantsFormalReward;
        public bool entersFormalTaskList;
        public bool readsFormalPlayerProgress;
        public List<LedgerBuildTaskGoal> taskGoals = new();
    }

    [Serializable]
    public sealed class LedgerBuildTaskGoal
    {
        public string taskGoalId = string.Empty;
        public string chineseDescription = string.Empty;
        public string goalType = string.Empty;
        public string sourceEvent = string.Empty;
        public int requiredValue = 1;
        public string requiredBuildId = string.Empty;
        public string requiredSynergyId = string.Empty;
        public string requiredAffixId = string.Empty;
        public string requiredRarityId = string.Empty;
        public List<string> requiredTags = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool grantsFormalReward;
        public bool entersFormalTaskList;
        public bool readsFormalPlayerProgress;
        public string notes = "devOnly ledger task goal preview. Disabled and not connected to product task or reward flow.";
    }

    [Serializable]
    public sealed class LedgerBuildTaskEvent
    {
        public string eventId = string.Empty;
        public string sourceEvent = string.Empty;
        public string buildId = string.Empty;
        public string synergyId = string.Empty;
        public int thresholdPieceCount;
        public string bossProfileId = string.Empty;
        public string bossMechanic = string.Empty;
        public bool defeatedBoss;
        public bool cleansedNegativeStatus;
        public string completedValidationId = string.Empty;
        public string rarityId = string.Empty;
        public string affixId = string.Empty;
        public bool orangeCoreAffix;
        public bool affixRerollCompleted;
        public int valueDelta = 1;
        public List<string> tags = new();
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool grantsFormalReward;
        public bool readsFormalPlayerProgress;
        public string sourceSummary = string.Empty;
    }

    [Serializable]
    public sealed class LedgerBuildTaskProgressPreview
    {
        public string taskGoalId = string.Empty;
        public string chineseDescription = string.Empty;
        public string goalType = string.Empty;
        public string sourceEvent = string.Empty;
        public int requiredValue;
        public int observedValue;
        public bool completed;
        public bool devOnly = true;
        public bool isEnabled;
        public bool entersFormalFlow;
        public bool grantsFormalReward;
        public bool entersFormalTaskList;
        public List<string> matchedEventIds = new();
    }

    public static class LedgerTaskBuildHooksPreview
    {
        public static LedgerBuildTaskHook CreateDefaultHook()
        {
            return new LedgerBuildTaskHook
            {
                taskGoals = new List<LedgerBuildTaskGoal>
                {
                    CreateGoal(
                        "ledger_goal_activate_synergy_2",
                        "激活一次 2 件羁绊",
                        LedgerBuildTaskGoalTypes.ActivateSynergyThreshold,
                        LedgerBuildTaskEventSources.BuildEvaluationResult,
                        2,
                        requiredTags: new[] { "synergy", "2_piece" }),
                    CreateGoal(
                        "ledger_goal_activate_synergy_4",
                        "激活一次 4 件羁绊",
                        LedgerBuildTaskGoalTypes.ActivateSynergyThreshold,
                        LedgerBuildTaskEventSources.BuildEvaluationResult,
                        4,
                        requiredTags: new[] { "synergy", "4_piece" }),
                    CreateGoal(
                        "ledger_goal_activate_synergy_6",
                        "激活一次 6 件羁绊",
                        LedgerBuildTaskGoalTypes.ActivateSynergyThreshold,
                        LedgerBuildTaskEventSources.BuildEvaluationResult,
                        6,
                        requiredTags: new[] { "synergy", "6_piece" }),
                    CreateGoal(
                        "ledger_goal_thunder_defeat_shield_boss",
                        "用惊雷 Build 击败护盾 Boss",
                        LedgerBuildTaskGoalTypes.DefeatShieldBossWithBuild,
                        LedgerBuildTaskEventSources.BuildSimulationResult,
                        1,
                        "jing_lei_break",
                        requiredTags: new[] { "thunder", "shield", "boss" }),
                    CreateGoal(
                        "ledger_goal_cleanse_negative_status",
                        "用净厄 Build 清除负面状态",
                        LedgerBuildTaskGoalTypes.CleanseNegativeStatusWithBuild,
                        LedgerBuildTaskEventSources.BuildSimulationResult,
                        1,
                        "jing_e_cleanse",
                        requiredTags: new[] { "cleanse", "negative_status" }),
                    CreateGoal(
                        "ledger_goal_complete_specified_build_validation",
                        "完成一次指定 Build 验证",
                        LedgerBuildTaskGoalTypes.CompleteBuildValidation,
                        LedgerBuildTaskEventSources.DevChapterContentPoolResult,
                        1,
                        "combo_build",
                        requiredTags: new[] { "build_validation", "dev_chapter" }),
                    CreateGoal(
                        "ledger_goal_obtain_orange_core_affix",
                        "获得一个橙色核心词条",
                        LedgerBuildTaskGoalTypes.ObtainOrangeCoreAffix,
                        LedgerBuildTaskEventSources.AffixPreview,
                        1,
                        requiredAffixId: "orange_core",
                        requiredRarityId: "orange",
                        requiredTags: new[] { "affix", "orange_core" }),
                    CreateGoal(
                        "ledger_goal_complete_affix_reroll",
                        "完成一次词条洗练",
                        LedgerBuildTaskGoalTypes.CompleteAffixReroll,
                        LedgerBuildTaskEventSources.AffixPreview,
                        1,
                        requiredTags: new[] { "affix", "reroll" })
                }
            };
        }

        public static List<LedgerBuildTaskEvent> BuildEvents(
            BuildEvaluationResult buildEvaluation,
            BuildSimulationBenchmarkReport simulationReport,
            AffixRarityEvaluationResult affixEvaluation,
            EnemyBossValidationPool enemyBossPool,
            DevChapterContentPool devChapterPool)
        {
            List<LedgerBuildTaskEvent> events = new();
            AddBuildEvaluationEvents(events, buildEvaluation);
            AddSimulationEvents(events, simulationReport);
            AddEnemyBossPoolEvents(events, enemyBossPool);
            AddDevChapterEvents(events, devChapterPool);
            AddAffixEvents(events, affixEvaluation);
            return events
                .Where(item => item != null)
                .OrderBy(item => item.sourceEvent, StringComparer.Ordinal)
                .ThenBy(item => item.eventId, StringComparer.Ordinal)
                .ToList();
        }

        public static List<LedgerBuildTaskProgressPreview> PreviewProgress(
            LedgerBuildTaskHook hook,
            IEnumerable<LedgerBuildTaskEvent> events)
        {
            List<LedgerBuildTaskEvent> sourceEvents = (events ?? Enumerable.Empty<LedgerBuildTaskEvent>())
                .Where(item => item != null)
                .ToList();
            return (hook?.taskGoals ?? new List<LedgerBuildTaskGoal>())
                .Where(goal => goal != null)
                .Select(goal => BuildProgress(goal, sourceEvents))
                .ToList();
        }

        private static LedgerBuildTaskGoal CreateGoal(
            string goalId,
            string chineseDescription,
            string goalType,
            string sourceEvent,
            int requiredValue,
            string requiredBuildId = "",
            string requiredAffixId = "",
            string requiredRarityId = "",
            IEnumerable<string> requiredTags = null)
        {
            return new LedgerBuildTaskGoal
            {
                taskGoalId = goalId,
                chineseDescription = chineseDescription,
                goalType = goalType,
                sourceEvent = sourceEvent,
                requiredValue = requiredValue,
                requiredBuildId = requiredBuildId ?? string.Empty,
                requiredAffixId = requiredAffixId ?? string.Empty,
                requiredRarityId = requiredRarityId ?? string.Empty,
                requiredTags = Clean(requiredTags)
            };
        }

        private static void AddBuildEvaluationEvents(
            List<LedgerBuildTaskEvent> events,
            BuildEvaluationResult result)
        {
            foreach (ActiveSynergyResult synergy in result?.activeSynergies ?? Enumerable.Empty<ActiveSynergyResult>())
            {
                foreach (int threshold in ExtractThresholds(synergy))
                {
                    events.Add(new LedgerBuildTaskEvent
                    {
                        eventId = $"event_synergy_{SafeId(synergy.synergyId)}_{threshold}",
                        sourceEvent = LedgerBuildTaskEventSources.BuildEvaluationResult,
                        buildId = "build_evaluation_preview",
                        synergyId = synergy.synergyId ?? string.Empty,
                        thresholdPieceCount = threshold,
                        tags = new List<string> { "synergy", $"{threshold}_piece" },
                        sourceSummary = "BuildEvaluationResult active synergy threshold preview."
                    });
                }
            }
        }

        private static void AddSimulationEvents(
            List<LedgerBuildTaskEvent> events,
            BuildSimulationBenchmarkReport report)
        {
            foreach (BuildSimulationResult result in report?.results ?? Enumerable.Empty<BuildSimulationResult>())
            {
                if (result == null || result.entersFormalFlow || result.affectsFormalCombat || result.numericAnomaly)
                {
                    continue;
                }

                List<string> tags = Clean(result.validationTags)
                    .Concat(Clean(result.recommendedSynergies))
                    .ToList();
                string joined = string.Join(";", tags);
                bool bossLike = !string.IsNullOrWhiteSpace(result.bossProfileId)
                    || ContainsAny(result.bossMechanic, "boss", "shield")
                    || ContainsAny(result.buildId, "Boss", "boss");
                bool thunderShield = ContainsAny(result.buildId, "Thunder", "thunder")
                    || ContainsAny(joined, "thunder", "shield_break", "shield");
                bool cleanse = ContainsAny(result.buildId, "Cleanse", "cleanse")
                    || ContainsAny(joined, "cleanse", "negative_status", "purify");

                if (bossLike || thunderShield || cleanse)
                {
                    events.Add(new LedgerBuildTaskEvent
                    {
                        eventId = $"event_simulation_{SafeId(result.buildId)}",
                        sourceEvent = LedgerBuildTaskEventSources.BuildSimulationResult,
                        buildId = result.buildId ?? string.Empty,
                        bossProfileId = result.bossProfileId ?? string.Empty,
                        bossMechanic = result.bossMechanic ?? string.Empty,
                        defeatedBoss = bossLike && result.simulatedWinRate >= 0.5f,
                        cleansedNegativeStatus = cleanse,
                        tags = tags,
                        valueDelta = 1,
                        sourceSummary = "BuildSimulationResult sandbox outcome preview."
                    });
                }
            }
        }

        private static void AddEnemyBossPoolEvents(
            List<LedgerBuildTaskEvent> events,
            EnemyBossValidationPool pool)
        {
            foreach (BuildSandboxBossProfile boss in pool?.bosses ?? Enumerable.Empty<BuildSandboxBossProfile>())
            {
                if (boss == null || !boss.devOnly || boss.isEnabled || boss.entersFormalFlow)
                {
                    continue;
                }

                events.Add(new LedgerBuildTaskEvent
                {
                    eventId = $"event_pool_boss_{SafeId(boss.bossId)}",
                    sourceEvent = LedgerBuildTaskEventSources.EnemyBossValidationPoolResult,
                    bossProfileId = boss.bossId ?? string.Empty,
                    bossMechanic = boss.bossMechanic ?? string.Empty,
                    tags = Clean(boss.validationTags).Concat(Clean(boss.recommendedSynergies)).ToList(),
                    sourceSummary = "EnemyBossValidationPool devOnly boss profile preview."
                });
            }
        }

        private static void AddDevChapterEvents(
            List<LedgerBuildTaskEvent> events,
            DevChapterContentPool pool)
        {
            foreach (DevChapterProfile chapter in pool?.chapters ?? Enumerable.Empty<DevChapterProfile>())
            {
                if (chapter == null
                    || !chapter.devOnly
                    || chapter.isEnabled
                    || chapter.entersFormalFlow
                    || !chapter.simulatorReadable)
                {
                    continue;
                }

                List<string> targetBuilds = Clean(chapter.validationTargetBuilds);
                events.Add(new LedgerBuildTaskEvent
                {
                    eventId = $"event_devchapter_{SafeId(chapter.chapterId)}",
                    sourceEvent = LedgerBuildTaskEventSources.DevChapterContentPoolResult,
                    buildId = targetBuilds.FirstOrDefault() ?? string.Empty,
                    completedValidationId = chapter.chapterId ?? string.Empty,
                    bossProfileId = chapter.bossId ?? string.Empty,
                    tags = targetBuilds
                        .Concat(Clean(chapter.validationTags))
                        .Concat(Clean(chapter.recommendedSynergies))
                        .ToList(),
                    sourceSummary = "DevChapterContentPool simulator-readable validation preview."
                });
            }
        }

        private static void AddAffixEvents(
            List<LedgerBuildTaskEvent> events,
            AffixRarityEvaluationResult result)
        {
            foreach (AffixRarityItemResult item in result?.itemResults ?? Enumerable.Empty<AffixRarityItemResult>())
            {
                if (item == null)
                {
                    continue;
                }

                bool orangeCore = ContainsAny(item.rarityId, "orange")
                    || (item.selectedAffixes ?? new List<string>()).Any(affix => ContainsAny(affix, "orange", "core"));
                string affixId = string.Join(";", Clean(item.selectedAffixes));
                events.Add(new LedgerBuildTaskEvent
                {
                    eventId = $"event_affix_{SafeId(item.itemId)}",
                    sourceEvent = LedgerBuildTaskEventSources.AffixPreview,
                    buildId = item.itemId ?? string.Empty,
                    rarityId = item.rarityId ?? string.Empty,
                    affixId = affixId,
                    orangeCoreAffix = orangeCore,
                    affixRerollCompleted = item.selectedAffixes != null && item.selectedAffixes.Count > 0,
                    tags = Clean(item.sourceTags)
                        .Concat(Clean(item.selectedAffixes))
                        .Concat(new[] { item.rarityId ?? string.Empty })
                        .ToList(),
                    sourceSummary = "Affix preview selected-affix result."
                });
            }
        }

        private static LedgerBuildTaskProgressPreview BuildProgress(
            LedgerBuildTaskGoal goal,
            IReadOnlyList<LedgerBuildTaskEvent> events)
        {
            List<LedgerBuildTaskEvent> matched = events
                .Where(item => Matches(goal, item))
                .ToList();
            int observedValue = ResolveObservedValue(goal, matched);
            return new LedgerBuildTaskProgressPreview
            {
                taskGoalId = goal.taskGoalId,
                chineseDescription = goal.chineseDescription,
                goalType = goal.goalType,
                sourceEvent = goal.sourceEvent,
                requiredValue = goal.requiredValue,
                observedValue = observedValue,
                completed = observedValue >= goal.requiredValue,
                matchedEventIds = matched
                    .Select(item => item.eventId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList()
            };
        }

        private static bool Matches(LedgerBuildTaskGoal goal, LedgerBuildTaskEvent taskEvent)
        {
            if (goal == null
                || taskEvent == null
                || !taskEvent.devOnly
                || taskEvent.isEnabled
                || taskEvent.entersFormalFlow
                || taskEvent.grantsFormalReward
                || taskEvent.readsFormalPlayerProgress)
            {
                return false;
            }

            if (!string.Equals(goal.sourceEvent, taskEvent.sourceEvent, StringComparison.Ordinal))
            {
                return false;
            }

            return goal.goalType switch
            {
                LedgerBuildTaskGoalTypes.ActivateSynergyThreshold =>
                    taskEvent.thresholdPieceCount >= goal.requiredValue,
                LedgerBuildTaskGoalTypes.DefeatShieldBossWithBuild =>
                    taskEvent.defeatedBoss && HasRequiredBuildOrTags(goal, taskEvent),
                LedgerBuildTaskGoalTypes.CleanseNegativeStatusWithBuild =>
                    taskEvent.cleansedNegativeStatus && HasRequiredBuildOrTags(goal, taskEvent),
                LedgerBuildTaskGoalTypes.CompleteBuildValidation =>
                    !string.IsNullOrWhiteSpace(taskEvent.completedValidationId)
                    && HasRequiredBuildOrTags(goal, taskEvent),
                LedgerBuildTaskGoalTypes.ObtainOrangeCoreAffix =>
                    taskEvent.orangeCoreAffix
                    || ContainsAny(taskEvent.rarityId, goal.requiredRarityId)
                    || ContainsAny(taskEvent.affixId, goal.requiredAffixId),
                LedgerBuildTaskGoalTypes.CompleteAffixReroll =>
                    taskEvent.affixRerollCompleted,
                _ => false
            };
        }

        private static bool HasRequiredBuildOrTags(LedgerBuildTaskGoal goal, LedgerBuildTaskEvent taskEvent)
        {
            if (string.IsNullOrWhiteSpace(goal.requiredBuildId)
                || ContainsAny(taskEvent.buildId, goal.requiredBuildId)
                || (taskEvent.tags ?? new List<string>()).Any(tag => ContainsAny(tag, goal.requiredBuildId)))
            {
                return true;
            }

            return (goal.requiredTags ?? new List<string>()).Any(required =>
                ContainsAny(taskEvent.buildId, required)
                || ContainsAny(taskEvent.bossMechanic, required)
                || (taskEvent.tags ?? new List<string>()).Any(tag => ContainsAny(tag, required)));
        }

        private static int ResolveObservedValue(
            LedgerBuildTaskGoal goal,
            IReadOnlyList<LedgerBuildTaskEvent> matched)
        {
            if (goal.goalType == LedgerBuildTaskGoalTypes.ActivateSynergyThreshold)
            {
                return matched
                    .Select(item => item.thresholdPieceCount)
                    .DefaultIfEmpty(0)
                    .Max();
            }

            return matched.Sum(item => Math.Max(1, item.valueDelta));
        }

        private static IEnumerable<int> ExtractThresholds(ActiveSynergyResult synergy)
        {
            HashSet<int> thresholds = new();
            foreach (string threshold in synergy?.activeThresholds ?? Enumerable.Empty<string>())
            {
                int parsed = ParseTrailingInt(threshold);
                if (parsed > 0)
                {
                    thresholds.Add(parsed);
                }
            }

            int matched = Math.Max(0, synergy?.matchedCount ?? 0);
            foreach (int required in new[] { 2, 4, 6 })
            {
                if (matched >= required)
                {
                    thresholds.Add(required);
                }
            }

            return thresholds.OrderBy(value => value);
        }

        private static int ParseTrailingInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            int index = value.LastIndexOf(':');
            string token = index >= 0 && index < value.Length - 1
                ? value.Substring(index + 1)
                : value;
            return int.TryParse(token, out int parsed) ? parsed : 0;
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return tokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static List<string> Clean(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList();
        }

        private static string SafeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unknown";
            }

            char[] chars = value.Select(ch =>
                    char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' ? ch : '_')
                .ToArray();
            return new string(chars);
        }
    }
}
